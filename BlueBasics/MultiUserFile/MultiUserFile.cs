﻿// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using static BlueBasics.FileOperations;
using static BlueBasics.Generic;

namespace BlueBasics.MultiUserFile;

public sealed class MultiUserFile : IDisposable {

    #region Fields

    public int ReloadDelaySecond = 10;
    private static readonly ListExt<MultiUserFile> _all_Files = new();
    private readonly BackgroundWorker _backgroundWorker;
    private readonly Timer _checker;
    private readonly BackgroundWorker _pureBinSaver;
    private readonly long _startTick = DateTime.UtcNow.Ticks;
    private readonly bool _zipped;
    private DateTime _blockReload = new(1900, 1, 1);
    private string _canWriteError = string.Empty;
    private DateTime _canWriteNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private bool _checkedAndReloadNeed;
    private int _checkerTickCount = -5;
    private byte[] _data_On_Disk;
    private bool _doingTempFile;
    private string _editNormalyError = string.Empty;
    private DateTime _editNormalyNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private string _filename;
    private string _inhaltBlockdatei = string.Empty;
    private bool _initialLoadDone;
    private DateTime _lastMessageUtc = DateTime.UtcNow.AddMinutes(-10);
    private string _lastSaveCode;
    private DateTime _lastUserActionUtc = new(1900, 1, 1);
    private int _loadingThreadId = -1;
    private FileSystemWatcher? _watcher;

    #endregion

    #region Constructors

    public MultiUserFile(bool readOnly, bool zipped) {
        _zipped = zipped;
        _all_Files.Add(this);
        //OnMultiUserFileCreated(this); // Ruft ein statisches Event auf, deswegen geht das.
        _pureBinSaver = new BackgroundWorker {
            WorkerReportsProgress = true
        };
        _pureBinSaver.DoWork += PureBinSaver_DoWork;
        _pureBinSaver.ProgressChanged += PureBinSaver_ProgressChanged;
        _backgroundWorker = new BackgroundWorker {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true
        };
        _backgroundWorker.DoWork += BackgroundWorker_DoWork;
        _checker = new Timer(Checker_Tick);
        Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegangen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
        ReCreateWatcher();
        _checkedAndReloadNeed = true;
        _lastSaveCode = string.Empty;
        _data_On_Disk = Array.Empty<byte>();
        ReadOnly = readOnly;
        AutoDeleteBak = false;
        BlockReload(false);
        _checker.Change(2000, 2000);
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
    ~MultiUserFile() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;

    public event EventHandler DiscardPendingChanges;

    public event EventHandler<MultiUserFileBackgroundWorkerEventArgs> DoBackGroundWork;

    public event EventHandler DoWorkAfterSaving;

    public event EventHandler<MultiUserFileHasPendingChangesEventArgs> HasPendingChanges;

    public event EventHandler<MultiUserIsThereBackgroundWorkToDoEventArgs> IsThereBackgroundWorkToDo;

    public event EventHandler<LoadedEventArgs> Loaded;

    public event EventHandler<LoadingEventArgs> Loading;

    public event EventHandler<MultiUserParseEventArgs> ParseExternal;

    public event EventHandler RepairAfterParse;

    public event EventHandler SavedToDisk;

    /// <summary>
    /// Dient dazu, offene Dialoge abzufragen
    /// </summary>
    public event EventHandler<CancelEventArgs> ShouldICancelSaveOperations;

    public event EventHandler<MultiUserToListEventArgs> ToListOfByte;

    #endregion

    #region Properties

    /// <summary>
    ///
    /// </summary>
    /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
    public double AgeOfBlockDatei {
        get {
            if (!FileExists(Blockdateiname())) { return -1; }
            FileInfo f = new(Blockdateiname());
            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }
    }

    public bool AutoDeleteBak { get; set; }

    public bool Disposed { get; private set; }

    /// <summary>
    /// Load oder SaveAsAndChangeTo benutzen
    /// </summary>
    public string Filename {
        get => _filename;
        private set {
            if (string.IsNullOrEmpty(value)) {
                _filename = string.Empty;
            } else {
                var tmp = Path.GetFullPath(value);
                _filename = tmp;
            }
        }
    }

    /// <summary>
    /// Ab aktuell die "Save" Routine vom Code aufgerufen wird, und diese auf einen erfolgreichen Speichervorgang abwartet
    /// </summary>
    public bool IsInSaveingLoop { get; private set; }

    public bool IsLoading { get; private set; }

    public bool IsParsing { get; private set; }

    public bool IsSaving { get; private set; }

    public bool ReadOnly { get; private set; }

    public bool ReloadNeeded {
        get {
            if (string.IsNullOrEmpty(Filename)) { return false; }
            if (_checkedAndReloadNeed) { return true; }
            if (GetFileInfo(Filename, false) != _lastSaveCode) {
                _checkedAndReloadNeed = true;
                return true;
            }
            return false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet, um so viel wie möglich zu speichern - falls eine Datei blockiert ist.</param>
    public static void SaveAll(bool mustSave) {
        if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss
        // Parallel.ForEach(AllFiles, thisFile => {
        //    thisFile?.Save(mustSave);
        // });
        var x = _all_Files.Count;
        foreach (var thisFile in _all_Files) {
            thisFile?.Save(mustSave);
            if (x != _all_Files.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                SaveAll(mustSave);
                return;
            }
        }
    }

    public static byte[]? UnzipIt(byte[] data) {
        using MemoryStream originalFileStream = new(data);
        using ZipArchive zipArchive = new(originalFileStream);
        var entry = zipArchive.GetEntry("Main.bin");
        if (entry != null) {
            using var stream = entry.Open();
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        return null;
    }

    public void BlockReload(bool crashisiscurrentlyloading) {
        WaitLoaded(crashisiscurrentlyloading);
        if (IsInSaveingLoop) { return; } // Ausnahme, bearbeitung sollte eh blockiert sein...
        if (IsSaving) { return; }
        _blockReload = DateTime.UtcNow;
    }

    /// <summary>
    /// Alle Abfragen, die nicht durch standard Abfragen gehandelt werden können.
    /// Z.B. Offen Dialoge, Prozesse die nur die die abgeleitete Klasse kennt
    /// </summary>
    /// <returns></returns>
    public bool BlockSaveOperations() {
        var e = new CancelEventArgs {
            Cancel = false
        };
        OnShouldICancelSaveOperations(e);
        return e.Cancel;
    }

    public void CancelBackGroundWorker() {
        if (_backgroundWorker.IsBusy && !_backgroundWorker.CancellationPending) { _backgroundWorker.CancelAsync(); }
    }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);
        // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing) {
        if (!Disposed) {
            _all_Files.Remove(this);
            if (disposing) {
                // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
            }
            // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
            // TODO: große Felder auf Null setzen.
            Save(false);
            while (_pureBinSaver.IsBusy) { Pause(0.5, true); }
            SetReadOnly(); // Ja nix mehr speichern!!!
            // https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker
            _pureBinSaver.Dispose();
            _checker.Dispose();
            _checker.Dispose();
            Disposed = true;
        }
    }

    public string ErrorReason(ErrorReason mode) {
        if (mode == Enums.ErrorReason.OnlyRead) { return string.Empty; }

        //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
        if (mode is Enums.ErrorReason.Load or Enums.ErrorReason.LoadForCheckingOnly) {
            if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
            var sec = AgeOfBlockDatei;
            if (sec is >= 0 and < 10) { return "Ein anderer Computer speichert gerade Daten ab."; }
        }

        if (mode == Enums.ErrorReason.Load) {
            var x = DateTime.UtcNow.Subtract(_blockReload).TotalSeconds;
            if (x < 5 && _initialLoadDone) { return "Laden noch " + (5 - x) + " Sekunden blockiert."; }

            if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; }  // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
            if (_pureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
            if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
            if (IsParsing) { return "Es werden aktuell Daten geparsed."; }
            //if (BlockDiskOperations()) { return "Reload unmöglich, vererbte Klasse gab Fehler zurück"; }
            return string.Empty;
        }

        //----------Alle Edits und Save ------------------------------------------------------------------------
        //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
        //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
        if (ReadOnly) { return "Die Datei wurde schreibgeschützt geöffnet."; }
        if (CheckForLastError(ref _editNormalyNextCheckUtc, ref _editNormalyError)) { return _editNormalyError; }
        if (!string.IsNullOrEmpty(Filename)) {
            if (!CanWriteInDirectory(Filename.FilePath())) {
                _editNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                return _editNormalyError;
            }
            if (AgeOfBlockDatei > 60) {
                _editNormalyError = "Eine Blockdatei ist anscheinend dauerhaft vorhanden. Administrator verständigen.";
                return _editNormalyError;
            }
        }

        //----------EditAcut, EditGeneral ----------------------------------------------------------------------
        if (mode.HasFlag(Enums.ErrorReason.EditAcut) || mode.HasFlag(Enums.ErrorReason.EditGeneral)) {
            // Wird gespeichert, werden am Ende Penings zu Undos. Diese werden evtl nicht mitgespeichert.
            if (_pureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
            if (IsInSaveingLoop) { return "Aktuell werden Daten gespeichert."; }
            if (IsLoading) { return "Aktuell werden Daten geladen."; }
        }

        //----------EditGeneral, Save------------------------------------------------------------------------------------------
        if (mode.HasFlag(Enums.ErrorReason.EditGeneral) || mode.HasFlag(Enums.ErrorReason.Save)) {
            if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
            if (ReloadNeeded) { return "Die Datei muss neu eingelesen werden."; }
        }

        //---------- Save ------------------------------------------------------------------------------------------
        if (mode.HasFlag(Enums.ErrorReason.Save)) {
            if (IsLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }
            if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
            if (BlockSaveOperations()) { return "Speichern unmöglich, vererbte Klasse blockiert Speichervorgänge"; }
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
            if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------
            if (CheckForLastError(ref _canWriteNextCheckUtc, ref _canWriteError) && !string.IsNullOrEmpty(_canWriteError)) {
                return _canWriteError;
            }
            if (AgeOfBlockDatei >= 0) {
                _canWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                return _canWriteError;
            }
            try {
                FileInfo f2 = new(Filename);
                if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5) {
                    _canWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                    return _canWriteError;
                }
            } catch {
                _canWriteError = "Dateizugriffsfehler.";
                return _canWriteError;
            }
            if (!CanWrite(Filename, 0.5)) {
                _canWriteError = "Windows blockiert die Datei.";
                return _canWriteError;
            }
        }
        return string.Empty;
        // Gibt true zurück, wenn die letzte Prüfung noch gülig ist
        static bool CheckForLastError(ref DateTime nextCheckUtc, ref string lastMessage) {
            if (DateTime.UtcNow.Subtract(nextCheckUtc).TotalSeconds < 0) { return true; }
            lastMessage = string.Empty;
            nextCheckUtc = DateTime.UtcNow.AddSeconds(5);
            return false;
        }
    }

    /// <summary>
    /// gibt die Möglichkeit, Fehler in ein Protokoll zu schreiben, wenn nach dem Reload eine Inkonsitenz aufgetreten ist.
    /// Nicht für Reperaturzwecke gedacht.
    /// </summary>
    //protected abstract void CheckDataAfterReload();
    // Dient zur Erkennung redundanter Aufrufe.
    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in _all_Files) {
            if (thisFile != null && string.Equals(thisFile.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                thisFile.Save(true);
                Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                return false;
            }
        }
        return true;
    }

    public void Load(string fileNameToLoad, bool createWhenNotExisting) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(FehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);
        if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                if (ReadOnly) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                    return;
                }
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                SetReadOnly();
                return;
            }
        }
        Filename = fileNameToLoad;
        ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        Load_Reload();
    }

    /// <summary>
    /// Führt - falls nötig - einen Reload der Datei aus.
    /// Der Prozess wartet solange, bis der Reload erfolgreich war.
    /// Ein bereits eventuell bestehender Ladevorgang wird abgewartet.
    /// </summary>
    public void Load_Reload() {
        WaitLoaded(true);

        if (string.IsNullOrEmpty(Filename)) { return; }

        IsLoading = true;
        _loadingThreadId = Thread.CurrentThread.ManagedThreadId;

        //// Wichtig, das _LastSaveCode geprüft wird, das ReloadNeeded im EasyMode immer false zurück gibt.
        //if (!string.IsNullOrEmpty(_LastSaveCode) && !ReloadNeeded) { IsLoading = false; return; }
        //var OnlyReload = !string.IsNullOrEmpty(_LastSaveCode);
        if (_initialLoadDone && !ReloadNeeded) { IsLoading = false; return; } // Wird in der Schleife auch geprüft

        LoadingEventArgs ec = new(_initialLoadDone);
        OnLoading(ec);

        if (_initialLoadDone && ReadOnly && ec.TryCancel) { IsLoading = false; return; }

        var (bLoaded, tmpLastSaveCode) = LoadBytesFromDisk(Enums.ErrorReason.Load);
        if (bLoaded == null) { IsLoading = false; return; }
        _data_On_Disk = bLoaded;
        //PrepeareDataForCheckingBeforeLoad();
        ParseInternal(bLoaded);
        _lastSaveCode = tmpLastSaveCode; // initialize setzt zurück

        var onlyReload = _initialLoadDone;
        _initialLoadDone = true;
        _checkedAndReloadNeed = false;

        //CheckDataAfterReload();
        OnLoaded(new LoadedEventArgs(onlyReload));
        RepairOldBlockFiles();

        IsLoading = false;
        WaitLoaded(true); // nur um BlockReload neu zu setzen
    }

    public void LoadFromStream(Stream stream) {
        OnLoading(new LoadingEventArgs(false));
        byte[] bLoaded;
        using (BinaryReader r = new(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }
        if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = UnzipIt(bLoaded);
        }
        ParseInternal(bLoaded);
        OnLoaded(new LoadedEventArgs(false));
        RepairOldBlockFiles();
    }

    public void OnConnectedControlsStopAllWorking(MultiUserFileStopWorkingEventArgs e) {
        if (Disposed) { return; }
        if (e.AllreadyStopped.Contains(Filename.ToLower())) { return; }
        e.AllreadyStopped.Add(Filename.ToLower());
        ConnectedControlsStopAllWorking?.Invoke(this, e);
    }

    public void RemoveFilename() {
        Filename = string.Empty;
        ReCreateWatcher();
        SetReadOnly();
    }

    public void RepairOldBlockFiles() {
        if (DateTime.UtcNow.Subtract(_lastMessageUtc).TotalMinutes < 1) { return; }
        _lastMessageUtc = DateTime.UtcNow;
        var sec = AgeOfBlockDatei;
        try {
            // Nach 15 Minuten versuchen die Datei zu reparieren
            if (sec >= 900) {
                if (!FileExists(Filename)) { return; }
                _ = File.ReadAllText(Blockdateiname(), System.Text.Encoding.UTF8);
                if (!CreateBlockDatei()) { return; }
                var autoRepairName = TempFile(Filename.FilePath(), Filename.FileNameWithoutSuffix() + "_BeforeAutoRepair", "AUT");
                if (!CopyFile(Filename, autoRepairName, false)) {
                    Develop.DebugPrint(FehlerArt.Warnung, "Autoreparatur fehlgeschlagen 1: " + Filename);
                    return;
                }
                if (!DeleteBlockDatei(true, false)) {
                    Develop.DebugPrint(FehlerArt.Warnung, "Autoreparatur fehlgeschlagen 2: " + Filename);
                }
            }
        } catch {
            //Develop.DebugPrint(ex);
        }
    }

    /// <summary>
    /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
    /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
    /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
    /// </summary>
    /// <param name="mustSave"></param>
    public bool Save(bool mustSave) {
        if (ReadOnly) { return false; }
        if (IsInSaveingLoop) { return false; }
        //if (isSomethingDiscOperatingsBlocking()) {
        //    if (!mustSave) { RepairOldBlockFiles(); return false; }
        //    Develop.DebugPrint(enFehlerArt.Warnung, "Release unmöglich, Dateistatus geblockt");
        //    return false;
        //}
        if (string.IsNullOrEmpty(Filename)) { return false; }
        OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken
        var d = DateTime.UtcNow; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.
        if (!mustSave && AgeOfBlockDatei >= 0) { RepairOldBlockFiles(); return false; }
        while (OnHasPendingChanges()) {
            IsInSaveingLoop = true;
            CancelBackGroundWorker();
            Load_Reload();
            var (tmpFileName, fileInfoBeforeSaving, dataUncompressed) = WriteTempFileToDisk(false); // Dateiname, Stand der Originaldatei, was gespeichert wurde
            var f = SaveRoutine(false, tmpFileName, fileInfoBeforeSaving, dataUncompressed);
            if (!string.IsNullOrEmpty(f)) {
                if (DateTime.UtcNow.Subtract(d).TotalSeconds > 40) {
                    // Da liegt ein größerer Fehler vor...
                    if (mustSave) { Develop.DebugPrint(FehlerArt.Warnung, "Datei nicht gespeichert: " + Filename + " " + f); }
                    RepairOldBlockFiles();
                    IsInSaveingLoop = false;
                    return false;
                }
                if (!mustSave && DateTime.UtcNow.Subtract(d).TotalSeconds > 5 && AgeOfBlockDatei < 0) {
                    //Develop.DebugPrint(enFehlerArt.Info, "Optionales Speichern nach 5 Sekunden abgebrochen bei " + Filename + " " + f);
                    IsInSaveingLoop = false;
                    return false;
                }
            }
        }
        IsInSaveingLoop = false;
        return true;
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
        Save(true); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immern noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!
        RemoveWatcher();
        Filename = newFileName;
        OnDiscardPendingChanges(); // Oben beschrieben. Sonst passiert bei Reload, dass diese wiederholt werden.
        var l = OnToListOfByte();
        using (FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
            x.Write(l.ToArray(), 0, l.ToArray().Length);
            x.Flush();
            x.Close();
        }
        _data_On_Disk = l;
        _lastSaveCode = GetFileInfo(Filename, true);
        _checkedAndReloadNeed = false;
        CreateWatcher();
    }

    public void SetReadOnly() {
        Develop.DebugPrint(FehlerArt.Info, "ReadOnly gesetzt<br>" + Filename);
        ReadOnly = true;
    }

    public void SetUserDidSomething() => _lastUserActionUtc = DateTime.UtcNow;

    public void UnlockHard() {
        try {
            Load_Reload();
            if (AgeOfBlockDatei >= 0) { DeleteBlockDatei(true, true); }
            Save(true);
        } catch {
        }
    }

    public void WaitEditable() {
        while (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditAcut))) {
            if (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditNormaly))) { return; }// Nur anzeige-Dateien sind immer Schreibgeschützt
            Pause(0.2, true);
        }
    }

    /// <summary>
    /// Darf nur von einem Background-Thread aufgerufen werden.
    /// Nach einer Minute wird der trotzdem diese Routine verlassen, vermutlich liegt dann ein Fehler vor,
    /// da der Parse unabhängig vom Netzwerk ist
    /// </summary>
    public void WaitParsed() {
        if (!Thread.CurrentThread.IsBackground) {
            Develop.DebugPrint(FehlerArt.Fehler, "Darf nur von einem BackgroundThread aufgerufen werden!");
        }
        var x = DateTime.Now;
        while (IsParsing) {
            Develop.DoEvents();
            if (DateTime.Now.Subtract(x).TotalSeconds > 60) { return; }
        }
    }

    internal bool BlockDateiCheck() {
        if (AgeOfBlockDatei < 0) {
            //Develop.DebugPrint(enFehlerArt.Info, "Block-Datei Konflikt: Block-Datei zu jung\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei);
            return false;
        }
        try {
            var inhalt2 = File.ReadAllText(Blockdateiname(), System.Text.Encoding.UTF8);
            if (_inhaltBlockdatei != inhalt2) {
                //Develop.DebugPrint(enFehlerArt.Info, "Block-Datei Konflikt: Inhalte unterschiedlich\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei + "\r\n\r\nIst: " + Inhalt2);
                return false;
            }
        } catch {
            //Develop.DebugPrint(enFehlerArt.Info, ex);
            return false;
        }
        return true;
    }

    internal void ParseInternal(byte[] bLoaded) {
        if (IsParsing) { Develop.DebugPrint(FehlerArt.Fehler, "Doppelter Parse!"); }
        IsParsing = true;
        OnParseExternal(bLoaded);
        IsParsing = false;
        // Repair NACH ExecutePendung, vielleicht ist es schon repariert
        // Repair NACH _isParsing, da es auch abgespeichert werden soll
        //OnParsed();
        OnRepairAfterParse();
    }

    protected void OnLoaded(LoadedEventArgs e) {
        if (Disposed) { return; }
        Loaded?.Invoke(this, e);
    }

    private static byte[] ZipIt(byte[] data) {
        // https://stackoverflow.com/questions/17217077/create-zip-file-from-byte
        using MemoryStream compressedFileStream = new();
        // Create an archive and store the stream in memory.
        using (ZipArchive zipArchive = new(compressedFileStream, ZipArchiveMode.Create, false)) {
            // Create a zip entry for each attachment
            var zipEntry = zipArchive.CreateEntry("Main.bin");
            // Get the stream of the attachment
            using MemoryStream originalFileStream = new(data);
            using var zipEntryStream = zipEntry.Open();
            // Copy the attachment stream to the zip entry stream
            originalFileStream.CopyTo(zipEntryStream);
        }
        return compressedFileStream.ToArray();
    }

    private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) => OnDoBackGroundWork((BackgroundWorker)sender);

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    private string Blockdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";

    private void Checker_Tick(object state) {
        if (DateTime.UtcNow.Subtract(_blockReload).TotalSeconds < 5) { return; }
        if (IsLoading) { return; }
        if (_pureBinSaver.IsBusy || IsSaving) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        // Ausstehende Arbeiten ermittelen
        var editable = string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditNormaly));
        var mustReload = ReloadNeeded;
        var mustSave = editable && OnHasPendingChanges();
        var mustBackup = editable && OnIsThereBackgroundWorkToDo();
        if (!mustReload && !mustSave && !mustBackup) {
            RepairOldBlockFiles();
            _checkerTickCount = 0;
            return;
        }

        // Zeiten berechnen
        ReloadDelaySecond = Math.Max(ReloadDelaySecond, 10);
        var countBackUp = Math.Min((ReloadDelaySecond / 10f) + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save
        var countSave = (countBackUp * 2) + 1; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird
        var countUserWork = (countSave / 5f) + 2; // Soviele Sekunden hat die User-Bearbeitung vorrang. Verhindert, dass die Bearbeitung des Users spontan abgebrochen wird.

        if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < countUserWork || BlockSaveOperations()) { CancelBackGroundWorker(); return; } // Benutzer arbeiten lassen

        if (_checkerTickCount > countSave && mustSave) { CancelBackGroundWorker(); }
        if (_checkerTickCount > ReloadDelaySecond && mustReload) { CancelBackGroundWorker(); }
        if (_backgroundWorker.IsBusy) { return; }

        if (mustReload && mustSave) {
            if (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.Load))) { return; }
            // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
            Load_Reload();
            return;
        }

        if (mustSave && _checkerTickCount > countSave) {
            if (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.Save))) { return; }
            if (!_pureBinSaver.IsBusy) { _pureBinSaver.RunWorkerAsync(); } // Eigentlich sollte diese Abfrage überflüssig sein. Ist sie aber nicht
            _checkerTickCount = 0;
            return;
        }

        if (mustBackup && !mustReload && !mustSave && _checkerTickCount >= countBackUp && string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditAcut))) {
            var nowsek = (DateTime.UtcNow.Ticks - _startTick) / 30000000;
            if (nowsek % 20 != 0) { return; } // Lasten startabhängig verteilen. Bei Pending changes ist es eh immer true;

            StartBackgroundWorker();
            return;
        }

        // Überhaupt nix besonderes. Ab und zu mal Reloaden
        if (mustReload && _checkerTickCount > ReloadDelaySecond) {
            if (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.Load))) { return; }
            Load_Reload();
            _checkerTickCount = 0;
        }
    }

    private bool CreateBlockDatei() {
        var tmpInhalt = UserName() + "\r\n" + DateTime.UtcNow.ToString(Constants.Format_Date5) + "\r\nThread: " + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;
        // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
        try {
            var bInhalt = tmpInhalt.UTF8_ToByte();
            // Nicht Generic, wegen den strengen Datei-Rechten
            using (FileStream x = new(Blockdateiname(), FileMode.Create, FileAccess.Write,
                       FileShare.None)) {
                x.Write(bInhalt, 0, bInhalt.Length);
                x.Flush();
                x.Close();
            }
            _inhaltBlockdatei = tmpInhalt;
        } catch {
            //Develop.DebugPrint(enFehlerArt.Warnung, ex);
            return false;
        }

        if (AgeOfBlockDatei < 0) {
            //Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + Filename);
            return false;
        }

        // Kontrolle, ob kein Netzwerkkonflikt vorliegt
        Pause(1, false);
        return BlockDateiCheck();
    }

    private void CreateWatcher() {
        if (!string.IsNullOrEmpty(Filename)) {
            _watcher = new FileSystemWatcher(Filename.FilePath());
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Error += Watcher_Error;
            _watcher.EnableRaisingEvents = true;
        }
    }

    private bool DeleteBlockDatei(bool ignorechecking, bool tryHard) {
        if (ignorechecking || BlockDateiCheck()) {
            if (DeleteFile(Blockdateiname(), tryHard)) {
                _inhaltBlockdatei = string.Empty;
                return true;
            }
        }
        //if (mustDoIt) {
        //    Develop.DebugPrint(enFehlerArt.Info, "Block-Datei nicht gelöscht\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei);
        //}
        return false;
    }

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private (byte[]? data, string fileinfo) LoadBytesFromDisk(ErrorReason checkmode) {
        string tmpLastSaveCode2;
        var startTime = DateTime.UtcNow;
        byte[] bLoaded;
        while (true) {
            try {
                if (_initialLoadDone && !ReloadNeeded) { return (null, string.Empty); } // Problem hat sich aufgelöst
                var f = ErrorReason(checkmode);
                if (string.IsNullOrEmpty(f)) {
                    var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                    bLoaded = File.ReadAllBytes(Filename);
                    tmpLastSaveCode2 = GetFileInfo(Filename, true);
                    if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }
                    f = "Datei wurde während des Ladens verändert.";
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 20) {
                    Develop.DebugPrint(FehlerArt.Info, f + "\r\n" + Filename);
                }

                Pause(0.5, false);
            } catch (Exception ex) {
                // Home Office kann lange blokieren....
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 300) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    return (null, string.Empty);
                }
            }
        }

        if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = UnzipIt(bLoaded);
        }
        return (bLoaded, tmpLastSaveCode2);
    }

    private void OnDiscardPendingChanges() {
        DiscardPendingChanges?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnDoBackGroundWork(BackgroundWorker bgw) {
        DoBackGroundWork?.Invoke(this, new MultiUserFileBackgroundWorkerEventArgs(bgw));
    }

    /// <summary>
    ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
    /// </summary>
    private void OnDoWorkAfterSaving() {
        DoWorkAfterSaving?.Invoke(this, System.EventArgs.Empty);
    }

    private bool OnHasPendingChanges() {
        var x = new MultiUserFileHasPendingChangesEventArgs();
        HasPendingChanges?.Invoke(this, x);
        return x.HasPendingChanges;
    }

    private bool OnIsThereBackgroundWorkToDo() {
        var x = new MultiUserIsThereBackgroundWorkToDoEventArgs();
        IsThereBackgroundWorkToDo?.Invoke(this, x);
        return x.BackGroundWork;
    }

    private void OnLoading(LoadingEventArgs e) {
        if (Disposed) { return; }
        Loading?.Invoke(this, e);
    }

    private void OnParseExternal(byte[] toParse) {
        ParseExternal?.Invoke(this, new MultiUserParseEventArgs(toParse));
    }

    private void OnRepairAfterParse() {
        RepairAfterParse?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnSavedToDisk() {
        if (Disposed) { return; }
        SavedToDisk?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnShouldICancelSaveOperations(CancelEventArgs e) => ShouldICancelSaveOperations?.Invoke(this, e);

    private byte[] OnToListOfByte() {
        var x = new MultiUserToListEventArgs();

        ToListOfByte?.Invoke(this, x);
        return x.Data;
    }

    private void PureBinSaver_DoWork(object sender, DoWorkEventArgs e) {
        try {
            var data = WriteTempFileToDisk(true);
            _pureBinSaver.ReportProgress(100, data);
        } catch {
            // OPeration completed bereits aufgerufen
        }
    }

    private void PureBinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        if (e.UserState == null) { return; }
        var (s, item2, bytes) = ((string, string, byte[]))e.UserState;
        // var Data = (Tuple<string, string, string>)e.UserState;
        SaveRoutine(true, s, item2, bytes);
    }

    private void ReCreateWatcher() {
        RemoveWatcher();
        CreateWatcher();
    }

    private void RemoveWatcher() {
        if (_watcher != null) {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= Watcher_Changed;
            _watcher.Created -= Watcher_Created;
            _watcher.Deleted -= Watcher_Deleted;
            _watcher.Renamed -= Watcher_Renamed;
            _watcher.Error -= Watcher_Error;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    /// <summary>
    /// Entfernt im Regelfall die Temporäre Datei
    /// </summary>
    /// <param name="fromParallelProzess"></param>
    /// <param name="tmpFileName"></param>
    /// <param name="fileInfoBeforeSaving"></param>
    /// <param name="savedDataUncompressed"></param>
    /// <returns></returns>
    private string SaveRoutine(bool fromParallelProzess, string tmpFileName, string fileInfoBeforeSaving, byte[] savedDataUncompressed) {
        if (ReadOnly) { return Feedback("Datei ist Readonly"); }
        if (tmpFileName == null || string.IsNullOrEmpty(tmpFileName) ||
            fileInfoBeforeSaving == null || string.IsNullOrEmpty(fileInfoBeforeSaving) ||
            savedDataUncompressed == null || savedDataUncompressed.Length == 0) { return Feedback("Keine Daten angekommen."); }
        if (!fromParallelProzess && _pureBinSaver.IsBusy) { return Feedback("Anderer interner binärer Speichervorgang noch nicht abgeschlossen."); }
        if (fromParallelProzess && IsInSaveingLoop) { return Feedback("Anderer manuell ausgelöster binärer Speichervorgang noch nicht abgeschlossen."); }
        var f = ErrorReason(Enums.ErrorReason.Save);
        if (!string.IsNullOrEmpty(f)) { return Feedback("Fehler: " + f); }
        if (string.IsNullOrEmpty(Filename)) { return Feedback("Kein Dateiname angegeben"); }
        if (IsSaving) { return Feedback("Speichervorgang von verschiedenen Routinen aufgerufen."); }
        IsSaving = true;
        var erfolg = CreateBlockDatei();
        if (!erfolg) {
            IsSaving = false;
            return Feedback("Blockdatei konnte nicht erzeugt werden.");
        }
        // Blockdatei da, wir sind save. Andere Computer lassen die Datei ab jetzt in Ruhe!
        if (GetFileInfo(Filename, true) != fileInfoBeforeSaving) {
            DeleteBlockDatei(false, true);
            IsSaving = false;
            return Feedback("Datei wurde inzwischen verändert.");
        }
        if (!savedDataUncompressed.SequenceEqual(OnToListOfByte())) {
            DeleteBlockDatei(false, true);
            IsSaving = false;
            return Feedback("Daten wurden inzwischen verändert.");
        }
        // OK, nun gehts rund: Zuerst das Backup löschen
        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return Feedback("Backup löschen fehlgeschlagen"); }
        }

        // Haupt-Datei wird zum Backup umbenannt
        if (!RenameFile(Filename, Backupdateiname(), false)) { return Feedback("Umbenennen der Hauptdatei fehlgeschlagen"); }

        // --- TmpFile wird zum Haupt ---
        RenameFile(tmpFileName, Filename, true);

        // ---- Steuerelemente Sagen, was gespeichert wurde
        _data_On_Disk = savedDataUncompressed;

        // Und nun den Block entfernen
        CanWrite(Filename, 30); // sobald die Hauptdatei wieder frei ist
        DeleteBlockDatei(false, true);

        // Evtl. das BAK löschen
        if (AutoDeleteBak && FileExists(Backupdateiname())) {
            DeleteFile(Backupdateiname(), false);
        }

        // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
        var (data, fileinfo) = LoadBytesFromDisk(Enums.ErrorReason.LoadForCheckingOnly);
        if (data == null || !savedDataUncompressed.SequenceEqual(data)) {
            // OK, es sind andere Daten auf der Festplatte?!? Seltsam, zählt als sozusagen ungespeichter und ungeladen.
            _checkedAndReloadNeed = true;
            _lastSaveCode = "Fehler";
            //Develop.DebugPrint(enFehlerArt.Warnung, "Speichern fehlgeschlagen!!! " + Filename);
            IsSaving = false;
            return Feedback("Speichervorgang fehlgeschlagen.");
        }

        _checkedAndReloadNeed = false;
        _lastSaveCode = fileinfo;
        OnDoWorkAfterSaving();
        OnSavedToDisk();
        IsSaving = false;
        return string.Empty;
        string Feedback(string txt) {
            DeleteFile(tmpFileName, false);
            //Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br>Grund:<br>" + txt);
            RepairOldBlockFiles();
            return txt;
        }
    }

    private void StartBackgroundWorker() {
        try {
            if (!string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditNormaly))) { return; }
            if (!_backgroundWorker.IsBusy) { _backgroundWorker.RunWorkerAsync(); }
        } catch {
            StartBackgroundWorker();
        }
    }

    /// <summary>
    /// Wartet bis der Reload abgeschlossen ist.
    /// Ist der LoadThread der aktuelle Thread, wir nicht gewartet!
    /// </summary>
    /// <param name="hardmode"></param>
    private void WaitLoaded(bool hardmode) {
        if (_loadingThreadId == Thread.CurrentThread.ManagedThreadId) { return; }
        var x = DateTime.Now;
        while (IsLoading) {
            Develop.DoEvents();
            if (!hardmode && !IsParsing) { return; }
            if (DateTime.Now.Subtract(x).TotalMinutes > 1) {
                if (hardmode) {
                    Develop.DebugPrint(FehlerArt.Warnung, "WaitLoaded hängt: " + Filename);
                }
                //Develop.DebugPrint(enFehlerArt.Warnung, "WaitLoaded hängt: " + Filename);
                return;
            }
        }
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e) {
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Created(object sender, FileSystemEventArgs e) {
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Error(object sender, ErrorEventArgs e) =>
        // Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
        _checkedAndReloadNeed = true;

    private void Watcher_Renamed(object sender, RenamedEventArgs e) {
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="iAmThePureBinSaver"></param>
    /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
    private (string TMPFileName, string FileInfoBeforeSaving, byte[]? DataUncompressed) WriteTempFileToDisk(bool iAmThePureBinSaver) {
        string fileInfoBeforeSaving;
        string tmpFileName;
        byte[] dataUncompressed;
        var count = 0;
        if (!iAmThePureBinSaver && _pureBinSaver.IsBusy) { return (string.Empty, string.Empty, null); }
        if (_doingTempFile) {
            if (!iAmThePureBinSaver) { Develop.DebugPrint("Erstelle bereits TMP-File"); }
            return (string.Empty, string.Empty, null);
        }
        _doingTempFile = true;
        while (true) {
            if (!iAmThePureBinSaver) {
                // Also, im NICHT-parallelen Prozess ist explizit der Save angestoßen worden.
                // Somit sollte des Prgramm auf Warteschleife sein und keine Benutzereingabe mehr kommen.
                // Problem: Wenn die ganze Save-Routine in einem Parallelen-Thread ist
                _lastUserActionUtc = new DateTime(1900, 1, 1);
            }
            var f = ErrorReason(Enums.ErrorReason.Save);
            if (!string.IsNullOrEmpty(f)) { _doingTempFile = false; return (string.Empty, string.Empty, null); }
            fileInfoBeforeSaving = GetFileInfo(Filename, true);
            dataUncompressed = OnToListOfByte();
            var writerBinaryData = _zipped ? ZipIt(dataUncompressed) : dataUncompressed;
            tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName().ToUpper());
            try {
                using FileStream x = new(tmpFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                x.Write(writerBinaryData, 0, writerBinaryData.Length);
                x.Flush();
                x.Close();
                break;
            } catch (Exception ex) {
                // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen
                count++;
                if (count > 15) {
                    Develop.DebugPrint(FehlerArt.Warnung, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                    _doingTempFile = false;
                    return (string.Empty, string.Empty, null);
                }
                Pause(1, true);
            }
        }
        _doingTempFile = false;
        return (tmpFileName, fileInfoBeforeSaving, dataUncompressed);
    }

    #endregion
}