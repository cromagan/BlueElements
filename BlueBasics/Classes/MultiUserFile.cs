// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using Timer = System.Threading.Timer;

namespace BlueBasics.MultiUserFile;

public abstract class MultiUserFile : IDisposableExtended, IHasKeyName, IParseable, INotifyPropertyChanged {

    #region Fields

    private static readonly List<MultiUserFile> AllFiles = [];
    private readonly Timer _checker;
    private string _canWriteError = string.Empty;
    private bool _checkedAndReloadNeed;
    private int _checkerTickCount = -5;
    private string _filename = string.Empty;
    private string _inhaltBlockdatei = string.Empty;
    private bool _initialLoadDone;

    /// <summary>
    /// Ab aktuell die "Save" Routine vom Code aufgerufen wird, und diese auf einen erfolgreichen Speichervorgang abwartet
    /// </summary>
    private bool _isInSaveingLoop;

    private bool _isLoading;
    private bool _isSaved = true;
    private bool _isSaving;
    private string _lastSaveCode;
    private int _lockCount;
    private System.IO.FileSystemWatcher? _watcher;

    #endregion

    #region Constructors

    protected MultiUserFile() {
        AllFiles.Add(this);
        _checker = new Timer(Checker_Tick);
        Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegangen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
        ReCreateWatcher();
        _checkedAndReloadNeed = true;
        _lastSaveCode = string.Empty;
        _ = _checker.Change(2000, 2000);
    }

    #endregion

    #region Events

    public event EventHandler<EditingEventArgs>? Editing;

    public event EventHandler? Loaded;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string CreateDate { get; private set; } = string.Empty;

    public string Creator { get; private set; } = string.Empty;

    /// <summary>
    /// Load oder SaveAsAndChangeTo benutzen
    /// </summary>
    public string Filename {
        get => _filename;
        private set {
            if (string.IsNullOrEmpty(value)) {
                _filename = string.Empty;
            } else {
                _filename = IO.CheckFile(value);
            }
        }
    }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgänig gemacht werden.
    /// Weil keine Undos mehr geladen werden, würde da nur Chaos entstehen.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Entspricht dem Dateinamen
    /// </summary>
    public string KeyName => Filename;

    public bool ReloadNeeded {
        get {
            if (string.IsNullOrEmpty(Filename)) { return false; }
            if (_checkedAndReloadNeed) { return true; }
            if (GetFileState(Filename, false) != _lastSaveCode) {
                _checkedAndReloadNeed = true;
                return true;
            }
            return false;
        }
    }

    public abstract string Type { get; }
    public abstract string Version { get; }

    /// <summary>
    ///
    /// </summary>
    /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
    private double AgeOfBlockDatei {
        get {
            if (!FileExists(Blockdateiname())) { return -1; }
            var f = GetFileInfo(Blockdateiname());
            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }
    }

    #endregion

    #region Methods

    public static void FreezeAll(string reason) {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile.Freeze(reason);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                FreezeAll(reason);
                return;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet, um so viel wie möglich zu speichern - falls eine Datei blokiert ist.</param>
    public static void SaveAll(bool mustSave) {
        if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss

        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile?.Save(mustSave);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                SaveAll(mustSave);
                return;
            }
        }
    }

    public static void UnlockAllHard() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile?.UnlockHard();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                UnlockAllHard();
                return;
            }
        }
    }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            _ = AllFiles.Remove(this);
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                //_ = Save(false);
                //while (_pureBinSaver.IsBusy) { Pause(0.5, true); }
                //// https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker
                //_pureBinSaver.Dispose();
                _watcher?.Dispose();
                _checker.Dispose();
            }
            // Nicht verwaltete Ressourcen (Bitmap, Tabellenverbindungen, ...)
            IsDisposed = true;
        }
    }

    public string EditableErrorReason(EditableErrorReasonType mode) {
        if (IsDisposed) { return "Daten verworfen."; }

        if (mode == EditableErrorReasonType.OnlyRead) { return string.Empty; }

        if (mode.HasFlag(EditableErrorReasonType.Save) && Develop.AllReadOnly) { return "Entwickler hat Speichern deaktiviert"; }

        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datei eingefroren: " + FreezedReason; }

        ////----------Load ------------------------------------------------------------------------
        //if (mode is EditableErrorReasonType.Load or EditableErrorReasonType.LoadForCheckingOnly) {
        //    if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        //    //if (_initialLoadDone && AmIBlocker(true)) { return "Die Datei ist garade von dem PC geblockt und kann nur geschrieben werden."; }
        //    if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 0.1) { return "Aktuell werden vom Benutzer Daten bearbeitet."; }  // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
        //    return string.Empty;
        //}

        //----------Alle Edits und Save ------------------------------------------------------------------------

        //----------EditAcut, EditCurrently ----------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.EditAcut) || mode.HasFlag(EditableErrorReasonType.EditCurrently)) {
            if (!AmIBlocker(true)) { return "Ein anderer Benutzer bearbeitet aktuell die Datei."; }
            if (_isInSaveingLoop) { return "Aktuell werden Daten gespeichert."; }
            if (_isLoading) { return "Aktuell werden Daten geladen."; }
        }

        //----------EditCurrently, Save------------------------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.EditCurrently) || mode.HasFlag(EditableErrorReasonType.Save)) {
            if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
            if (ReloadNeeded) { return "Die Datei muss neu eingelesen werden."; }
        }

        //---------- Save ------------------------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.Save)) {
            if (_isLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
            if (!CanWrite(Filename)) {
                _canWriteError = "Windows blockiert die Datei.";
                return _canWriteError;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Friert die Tabelle komplett ein, nur noch Ansicht möglich.
    /// Setzt auch ReadOnly.
    /// </summary>
    /// <param name="reason"></param>
    public void Freeze(string reason) {
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                thisFile.Save(true);
                Develop.DebugPrint(ErrorType.Warning, "Doppeltes Laden von " + fileName);
                return false;
            }
        }
        return true;
    }

    public void Load(string fileNameToLoad, bool createWhenNotExisting) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(ErrorType.Error, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(ErrorType.Error, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);

        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
        if (!FileExists(fileNameToLoad)) {
            //if (createWhenNotExisting) {
            //    SaveAsAndChangeTo(fileNameToLoad);
            //} else {
            Develop.DebugPrint(ErrorType.Warning, "Datei existiert nicht: " + fileNameToLoad);  // ReadOnly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
            return;
            //}
        }
        Filename = fileNameToLoad;
        ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesetzt wird, z.B: bei Bitmap import
        while (!Load_Reload()) { }
    }

    /// <summary>
    /// Führt - falls nötig - einen Reload der Datei aus.
    /// Der Prozess wartet so lange, bis der Reload erfolgreich war.
    /// Ein bereits eventuell bestehender Ladevorgang wird abgewartet.
    /// </summary>
    /// <returns>Gibt TRUE zurück, wenn die am Ende der Routine die Datei auf dem aktuellesten Stand ist</returns>
    public bool Load_Reload() {
        //if (!string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Load))) { return false; }

        try {
            _isLoading = true;

            if (_initialLoadDone && !ReloadNeeded) { return true; }

            var (data, tmpLastSaveCode) = LoadFromDisk();
            if (data.Length < 10) { return false; }

            if (!this.Parse(data)) { return false; }

            _lastSaveCode = tmpLastSaveCode; // initialize setzt zurück

            _initialLoadDone = true;
            _checkedAndReloadNeed = false;

            OnLoaded();

            return !ReloadNeeded;
        } catch {
            return false;
        } finally {
            _isLoading = false;
        }
    }

    public bool LockEditing() {
        if (_lockCount > 0) { return true; }

        if (!IsAdministrator()) { return false; }

        if (AgeOfBlockDatei is < 0 or > 3600) {
            //if (AmIBlocker()) { return false; }

            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;
            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try {
                _ = DeleteFile(Blockdateiname(), false);
                WriteAllText(Blockdateiname(), tmpInhalt, Constants.Win1252, false);
                _inhaltBlockdatei = tmpInhalt;
            } catch {
                return false;
            }

            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);
        }

        _lockCount++;
        return true;
    }

    public virtual List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Type", Type);
        result.ParseableAdd("Version", Version);
        result.ParseableAdd("CreateDate", CreateDate);
        result.ParseableAdd("CreateName", Creator);

        return result;
    }

    public virtual void ParseFinished(string parsed) { }

    public virtual bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "type":
                return true;

            case "version":
                return true;

            case "createdate":
                CreateDate = value.FromNonCritical();
                return true;

            case "createname":
                Creator = value.FromNonCritical();
                return true;
        }

        return false;
    }

    /// <summary>
    /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
    /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
    /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
    /// </summary>
    /// <param name="mustSave"></param>
    public void Save(bool mustSave) {
        if (Develop.AllReadOnly) { _isSaved = true; return; }

        if (_isInSaveingLoop) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        var tim = Stopwatch.StartNew();

        while (!_isSaved) {
            _isInSaveingLoop = true;

            var (tmpFileName, fileInfoBeforeSaving, dataUncompressed) = WriteTempFileToDisk();
            var f = SaveRoutine(tmpFileName, fileInfoBeforeSaving, dataUncompressed);

            if (string.IsNullOrEmpty(f) || !mustSave) { break; }

            if (tim.ElapsedMilliseconds > 40 * 1000) {
                Develop.DebugPrint(ErrorType.Warning, "Datei nicht gespeichert: " + Filename + " " + f);
                break;
            }
        }

        tim.Stop();
        _isInSaveingLoop = false;
    }

    public void UnlockEditing() {
        if (!AmIBlocker(false)) { return; }

        Save(true);

        _lockCount--;

        if (_lockCount > 0) { return; }

        UnlockAllHard();
    }

    internal bool AmIBlocker(bool silent) {
        if (AgeOfBlockDatei is < 0 or > 3600) { return false; }

        string inhalt;

        try {
            inhalt = ReadAllText(Blockdateiname(), Constants.Win1252);
        } catch {
            if (!silent) { _ = MessageBox.Show("Dateisystem Fehler"); }
            return false;
        }

        if (_inhaltBlockdatei == inhalt) { return true; }

        if (!silent) {
            _ = MessageBox.Show("<b>Bearbeiten nicht möglich.</b>\r\n" + inhalt);
        }

        return false;
    }

    protected void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        if (_isSaving || _isLoading) { return; }

        if (_lockCount < 1) {
            if (!LockEditing()) {
                Develop.DebugPrint(ErrorType.Error, $"Keine Änderungen an der Datei '{_filename.FileNameWithoutSuffix()}' möglich ({propertyName})!");
                return;
            }
        }

        //Develop.CheckStackForOverflow();
        _isSaved = false;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    private string Blockdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";

    private void Checker_Tick(object? state) {
        if (IsDisposed) { return; }
        if (_isLoading) { return; }
        if (_isSaving) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (!_checkedAndReloadNeed && _isSaved) {
            _checkerTickCount = 0;
            return;
        }

        // Zeiten berechnen
        var reloadDelaySecond = 30;
        var saveDelaySecond = 10; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird

        var mustReload = ReloadNeeded;

        if (!mustReload && _isSaved) {
            _checkerTickCount = 0;
        } else if (!_isSaved) {
            if (_checkerTickCount > saveDelaySecond) { Save(false); }
        } else if (mustReload) {
            if (_checkerTickCount > reloadDelaySecond) { _ = Load_Reload(); }
        }
    }

    private void CreateWatcher() {
        if (!string.IsNullOrEmpty(Filename)) {
            _watcher = new System.IO.FileSystemWatcher(Filename.FilePath());
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Error += Watcher_Error;
            _watcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private (string data, string fileinfo) LoadFromDisk() {
        string fileinfo;
        string data;
        var tim = Stopwatch.StartNew();

        while (true) {
            try {
                if (_initialLoadDone && !ReloadNeeded) { return (string.Empty, string.Empty); } // Problem hat sich aufgelöst

                var tmpFileInfo = GetFileState(Filename, true);
                data = ReadAllText(Filename, Constants.Win1252);
                fileinfo = GetFileState(Filename, true);
                if (tmpFileInfo == fileinfo) { break; }

                if (tim.ElapsedMilliseconds > 20 * 1000) { Develop.DebugPrint(ErrorType.Info, "Datei wurde während des Ladens verändert.\r\n" + Filename); }

                Pause(0.5, false);
            } catch (Exception ex) {
                // Home Office kann lange blokieren....
                if (tim.ElapsedMilliseconds > 300 * 1000) {
                    Develop.DebugPrint(ErrorType.Error, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    return (string.Empty, string.Empty);
                }
            }
        }

        return (data, fileinfo);
    }

    private void ReCreateWatcher() {
        RemoveWatcher();
        CreateWatcher();
    }

    private void RemoveWatcher() {
        try {
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= Watcher_Changed;
                _watcher.Created -= Watcher_Created;
                _watcher.Deleted -= Watcher_Deleted;
                _watcher.Renamed -= Watcher_Renamed;
                _watcher.Error -= Watcher_Error;
                _watcher?.Dispose();
                _watcher = null;
            }
        } catch { }
    }

    /// <summary>
    /// Entfernt im Regelfall die Temporäre Datei
    /// </summary>
    /// <param name="tmpFileName"></param>
    /// <param name="fileInfoBeforeSaving"></param>
    /// <param name="dataUncompressed"></param>
    /// <returns></returns>
    private string SaveRoutine(string tmpFileName, string fileInfoBeforeSaving, string dataUncompressed) {
        if (_isSaving) { return Feedback("Speichervorgang von verschiedenen Routinen aufgerufen.", false); }

        _isSaving = true;

        if (string.IsNullOrEmpty(tmpFileName) || string.IsNullOrEmpty(fileInfoBeforeSaving) || dataUncompressed.Length < 10) { return Feedback("Keine Daten angekommen.", true); }

        //if (_isInSaveingLoop) { return Feedback("Anderer manuell ausgelöster binärer Speichervorgang noch nicht abgeschlossen.", true); }

        var f = EditableErrorReason(EditableErrorReasonType.Save);
        if (!string.IsNullOrEmpty(f)) { return Feedback("Fehler: " + f, true); }

        var lb = ParseableItems().FinishParseable();
        if (dataUncompressed != lb) { return Feedback("Daten wurden inzwischen verändert.", true); }

        // OK, nun gehts rund: Zuerst das Backup löschen
        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return Feedback("Backup löschen fehlgeschlagen", true); }
        }

        // Haupt-Datei wird zum Backup umbenannt
        if (!MoveFile(Filename, Backupdateiname(), false)) { return Feedback("Umbenennen der Hauptdatei fehlgeschlagen", true); }

        // --- TmpFile wird zum Haupt ---
        _ = MoveFile(tmpFileName, Filename, true);

        // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
        var (data, fileinfo) = LoadFromDisk();
        if (dataUncompressed != data) {
            // OK, es sind andere Daten auf der Festplatte?!? Seltsam, zählt als sozusagen ungespeichert und ungeladen.
            _checkedAndReloadNeed = true;
            _lastSaveCode = "Fehler";
            return Feedback("Speichervorgang fehlgeschlagen.", true);
        }

        _checkedAndReloadNeed = false;
        _lastSaveCode = fileinfo;
        _isSaved = true;
        _isSaving = false;
        return string.Empty;

        string Feedback(string txt, bool removeSaving) {
            _ = DeleteFile(tmpFileName, false);
            if (removeSaving) { _isSaving = false; }
            return txt;
        }
    }

    private void UnlockHard() {
        if (DeleteFile(Blockdateiname(), false)) {
            _inhaltBlockdatei = string.Empty;
            _lockCount = 0;
        }
    }

    private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Created(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Deleted(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    /// Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Watcher_Error(object sender, System.IO.ErrorEventArgs e) {
        if (IsDisposed) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Renamed(object sender, System.IO.RenamedEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
    private (string TMPFileName, string FileInfoBeforeSaving, string dataUncompressed) WriteTempFileToDisk() {
        if (IsDisposed) { return (string.Empty, string.Empty, string.Empty); }
        if (Develop.AllReadOnly) { return (string.Empty, string.Empty, string.Empty); }

        string fileInfoBeforeSaving;
        string tmpFileName;
        string dataUncompressed;

        var count = 0;

        while (true) {
            count++;

            var f = EditableErrorReason(EditableErrorReasonType.Save);
            if (!string.IsNullOrEmpty(f)) { }

            fileInfoBeforeSaving = GetFileState(Filename, true);
            dataUncompressed = ParseableItems().FinishParseable();

            if (dataUncompressed.Length > 0) {
                tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());
                try {
                    WriteAllText(tmpFileName, dataUncompressed, Constants.Win1252, false);
                    break;
                } catch {
                    // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen
                }
            }

            if (count > 15) {
                Develop.DebugPrint(ErrorType.Warning, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + Filename);
                return (string.Empty, string.Empty, string.Empty);
            }

            Pause(1, true);
        }
        return (tmpFileName, fileInfoBeforeSaving, dataUncompressed);
    }

    #endregion
}