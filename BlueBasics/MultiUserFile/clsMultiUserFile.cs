// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

namespace BlueBasics.MultiUserFile {

    public abstract class clsMultiUserFile : IDisposable {

        #region Fields

        public static readonly ListExt<clsMultiUserFile> AllFiles = new();

        protected byte[] _dataOnDisk;
        protected int _ReloadDelaySecond = 10;
        private readonly long _startTick = DateTime.UtcNow.Ticks;
        private readonly bool _zipped;
        private readonly BackgroundWorker BackgroundWorker;
        private readonly Timer Checker;
        private readonly BackgroundWorker PureBinSaver;
        private DateTime _BlockReload = new(1900, 1, 1);
        private string _CanWriteError = string.Empty;
        private DateTime _CanWriteNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private bool _CheckedAndReloadNeed;
        private bool _DoingTempFile = false;
        private string _EditNormalyError = string.Empty;
        private DateTime _EditNormalyNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _Filename;
        private string _inhaltBlockdatei = string.Empty;
        private bool _InitialLoadDone = false;
        private DateTime _LastMessageUTC = DateTime.UtcNow.AddMinutes(-10);
        private string _LastSaveCode;
        private DateTime _LastUserActionUTC = new DateTime(1900, 1, 1);
        private int _loadingThreadId = -1;
        private int Checker_Tick_count = -5;
        private FileSystemWatcher Watcher;

        #endregion

        #region Constructors

        protected clsMultiUserFile(bool readOnly, bool zipped) {
            _zipped = zipped;
            AllFiles.Add(this);
            //OnMultiUserFileCreated(this); // Ruft ein statisches Event auf, deswegen geht das.
            PureBinSaver = new BackgroundWorker {
                WorkerReportsProgress = true
            };
            PureBinSaver.DoWork += PureBinSaver_DoWork;
            PureBinSaver.ProgressChanged += PureBinSaver_ProgressChanged;
            BackgroundWorker = new BackgroundWorker {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true,
            };
            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            Checker = new Timer(Checker_Tick);
            Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            ReCreateWatcher();
            _CheckedAndReloadNeed = true;
            _LastSaveCode = string.Empty;
            _dataOnDisk = new byte[0];
            ReadOnly = readOnly;
            AutoDeleteBAK = false;
            BlockReload(false);
            Checker.Change(2000, 2000);
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        ~clsMultiUserFile() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(false);
        }

        #endregion

        #region Events

        public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;

        public event EventHandler Disposing;

        public event EventHandler<LoadedEventArgs> Loaded;

        // Disposing leider als Variable vorhanden
        public event EventHandler<LoadingEventArgs> Loading;

        /// <summary>
        /// Wird ausgegeben, sobald isParsed false ist, noch vor den automatischen Reperaturen.
        /// Dieses Event kann verwendet werden, um die Datei automatisch zu reparieren, bevor sich automatische Dialoge öffnen.
        /// </summary>
        public event EventHandler Parsed;

        public event EventHandler SavedToDisk;

        /// <summary>
        /// Dient dazu, offene Dialoge abzufragen
        /// </summary>
        public event EventHandler<CancelEventArgs> ShouldICancelDiscOperations;

        #endregion

        #region Properties

        public bool AutoDeleteBAK { get; set; }

        public bool Disposed { get; private set; } = false;

        /// <summary>
        /// Load oder SaveAsAndChangeTo benutzen
        /// </summary>
        public string Filename {
            get => _Filename;
            private set {
                if (string.IsNullOrEmpty(value)) {
                    _Filename = string.Empty;
                } else {
                    var tmp = Path.GetFullPath(value);
                    _Filename = tmp;
                }
            }
        }

        /// <summary>
        /// Ab aktuell die "Save" Routine vom Code aufgerufen wird, und diese auf einen erfolgreichen Speichervorgang abwartet
        /// </summary>
        public bool IsInSaveingLoop { get; private set; } = false;

        public bool IsLoading { get; private set; } = false;

        public bool IsParsing { get; private set; } = false;

        // private string _loadingInfo = string.Empty;
        public bool IsSaving { get; private set; } = false;

        public bool ReadOnly { get; private set; }

        public bool ReloadNeeded {
            get {
                if (string.IsNullOrEmpty(Filename)) { return false; }
                if (_CheckedAndReloadNeed) { return true; }
                if (GetFileInfo(Filename, false) != _LastSaveCode) {
                    _CheckedAndReloadNeed = true;
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

        public static byte[] UnzipIt(byte[] data) {
            using MemoryStream originalFileStream = new(data);
            using ZipArchive zipArchive = new(originalFileStream);
            var entry = zipArchive.GetEntry("Main.bin");
            using var stream = entry.Open();
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
        public double AgeOfBlockDatei() {
            if (!FileExists(Blockdateiname())) { return -1; }
            FileInfo f = new(Blockdateiname());
            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }

        public void BlockReload(bool crashisiscurrentlyloading) {
            WaitLoaded(crashisiscurrentlyloading);
            if (IsInSaveingLoop) { return; } // Ausnahme, bearbeitung sollte eh blockiert sein...
            if (IsSaving) { return; }
            _BlockReload = DateTime.UtcNow;
        }

        public void CancelBackGroundWorker() {
            if (BackgroundWorker.IsBusy && !BackgroundWorker.CancellationPending) { BackgroundWorker.CancelAsync(); }
        }

        public abstract void DiscardPendingChanges();

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }

        public virtual string ErrorReason(enErrorReason mode) {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }

            //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
            if (mode is enErrorReason.Load or enErrorReason.LoadForCheckingOnly) {
                if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
                var sec = AgeOfBlockDatei();
                if (sec is >= 0 and < 10) { return "Ein anderer Computer speichert gerade Daten ab."; }
            }

            if (mode == enErrorReason.Load) {
                var x = DateTime.UtcNow.Subtract(_BlockReload).TotalSeconds;
                if (x < 5 && _InitialLoadDone) { return "Laden noch " + (5 - x).ToString() + " Sekunden blockiert."; }

                if (DateTime.UtcNow.Subtract(_LastUserActionUTC).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; }  // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
                if (PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (BackgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
                if (IsParsing) { return "Es werden aktuell Daten geparsed."; }
                if (BlockDiskOperations()) { return "Reload unmöglich, vererbte Klasse gab Fehler zurück"; }
                return string.Empty;
            }

            //----------Alle Edits und Save ------------------------------------------------------------------------
            //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
            //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
            if (ReadOnly) { return "Die Datei wurde schreibgeschützt geöffnet."; }
            if (CheckForLastError(ref _EditNormalyNextCheckUTC, ref _EditNormalyError)) { return _EditNormalyError; }
            if (!string.IsNullOrEmpty(Filename)) {
                if (!CanWriteInDirectory(Filename.FilePath())) {
                    _EditNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                    return _EditNormalyError;
                }
                if (AgeOfBlockDatei() > 60) {
                    _EditNormalyError = "Eine Blockdatei ist anscheinend dauerhaft vorhanden. Administrator verständigen.";
                    return _EditNormalyError;
                }
            }

            //----------EditAcut, EditGeneral ----------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditAcut) || mode.HasFlag(enErrorReason.EditGeneral)) {
                // Wird gespeichert, werden am Ende Penings zu Undos. Diese werden evtl nicht mitgespeichert.
                if (PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (IsInSaveingLoop) { return "Aktuell werden Daten gespeichert."; }
                if (IsLoading) { return "Aktuell werden Daten geladen."; }
            }

            //----------EditGeneral, Save------------------------------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditGeneral) || mode.HasFlag(enErrorReason.Save)) {
                if (BackgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
                if (ReloadNeeded) { return "Die Datei muss neu eingelesen werden."; }
            }

            //---------- Save ------------------------------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.Save)) {
                if (IsLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }
                if (DateTime.UtcNow.Subtract(_LastUserActionUTC).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
                if (BlockDiskOperations()) { return "Speichern unmöglich, vererbte Klasse gab Fehler zurück"; }
                if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
                if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------
                if (CheckForLastError(ref _CanWriteNextCheckUTC, ref _CanWriteError) && !string.IsNullOrEmpty(_CanWriteError)) {
                    return _CanWriteError;
                }
                if (AgeOfBlockDatei() >= 0) {
                    _CanWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                    return _CanWriteError;
                }
                try {
                    FileInfo f2 = new(Filename);
                    if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5) {
                        _CanWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                        return _CanWriteError;
                    }
                } catch {
                    _CanWriteError = "Dateizugriffsfehler.";
                    return _CanWriteError;
                }
                if (!CanWrite(Filename, 0.5)) {
                    _CanWriteError = "Windows blockiert die Datei.";
                    return _CanWriteError;
                }
            }
            return string.Empty;
            // Gibt true zurück, wenn die letzte Prüfung noch gülig ist
            static bool CheckForLastError(ref DateTime nextCheckUTC, ref string lastMessage) {
                if (DateTime.UtcNow.Subtract(nextCheckUTC).TotalSeconds < 0) { return true; }
                lastMessage = string.Empty;
                nextCheckUTC = DateTime.UtcNow.AddSeconds(5);
                return false;
            }
        }

        public abstract bool HasPendingChanges();

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
            if (_InitialLoadDone && !ReloadNeeded) { IsLoading = false; return; } // Wird in der Schleife auch geprüft

            LoadingEventArgs ec = new(_InitialLoadDone);
            OnLoading(ec);

            if (_InitialLoadDone && ReadOnly && ec.TryCancel) { IsLoading = false; return; }

            (var _BLoaded, var tmpLastSaveCode) = LoadBytesFromDisk(enErrorReason.Load);
            if (_BLoaded == null) { IsLoading = false; return; }
            _dataOnDisk = _BLoaded;
            PrepeareDataForCheckingBeforeLoad();
            ParseInternal(_BLoaded);
            _LastSaveCode = tmpLastSaveCode; // initialize setzt zurück

            var OnlyReload = _InitialLoadDone;
            _InitialLoadDone = true;
            _CheckedAndReloadNeed = false;

            CheckDataAfterReload();
            OnLoaded(new LoadedEventArgs(OnlyReload));
            RepairOldBlockFiles();

            IsLoading = false;
            WaitLoaded(true); // nur um BlockReload neu zu setzen
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

        public abstract void RepairAfterParse();

        public void RepairOldBlockFiles() {
            if (DateTime.UtcNow.Subtract(_LastMessageUTC).TotalMinutes < 1) { return; }
            _LastMessageUTC = DateTime.UtcNow;
            var sec = AgeOfBlockDatei();
            try {
                // Nach 15 Minuten versuchen die Datei zu reparieren
                if (sec >= 900) {
                    if (!FileExists(Filename)) { return; }
                    var x = File.ReadAllText(Blockdateiname(), System.Text.Encoding.UTF8);
                    Develop.DebugPrint(enFehlerArt.Info, "Repariere MultiUserFile: " + Filename + " \r\n" + x);
                    if (!CreateBlockDatei()) { return; }
                    var AutoRepairName = TempFile(Filename.FilePath(), Filename.FileNameWithoutSuffix() + "_BeforeAutoRepair", "AUT");
                    if (!CopyFile(Filename, AutoRepairName, false)) {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 1: " + Filename);
                        return;
                    }
                    if (!DeleteBlockDatei(true, false)) {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 2: " + Filename);
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
            var D = DateTime.UtcNow; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.
            if (!mustSave && AgeOfBlockDatei() >= 0) { RepairOldBlockFiles(); return false; }
            while (HasPendingChanges()) {
                IsInSaveingLoop = true;
                CancelBackGroundWorker();
                Load_Reload();
                (var TMPFileName, var FileInfoBeforeSaving, var DataUncompressed) = WriteTempFileToDisk(false); // Dateiname, Stand der Originaldatei, was gespeichert wurde
                var f = SaveRoutine(false, TMPFileName, FileInfoBeforeSaving, DataUncompressed);
                if (!string.IsNullOrEmpty(f)) {
                    if (DateTime.UtcNow.Subtract(D).TotalSeconds > 40) {
                        // Da liegt ein größerer Fehler vor...
                        if (mustSave) { Develop.DebugPrint(enFehlerArt.Warnung, "Datei nicht gespeichert: " + Filename + " " + f); }
                        RepairOldBlockFiles();
                        IsInSaveingLoop = false;
                        return false;
                    }
                    if (!mustSave && DateTime.UtcNow.Subtract(D).TotalSeconds > 5 && AgeOfBlockDatei() < 0) {
                        Develop.DebugPrint(enFehlerArt.Info, "Optionales Speichern nach 5 Sekunden abgebrochen bei " + Filename + " " + f);
                        IsInSaveingLoop = false;
                        return false;
                    }
                }
            }
            IsInSaveingLoop = false;
            return true;
        }

        public void SaveAsAndChangeTo(string newFileName) {
            if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
            Save(true); // Original-Datei speichern, die ist ja dann weg.
            // Jetzt kann es aber immern noch sein, das PendingChanges da sind.
            // Wenn kein Dateiname angegeben ist oder die Datei Readonkly wir nicht gespeichert und die Pendings bleiben erhalten!
            RemoveWatcher();
            Filename = newFileName;
            DiscardPendingChanges(); // Oben beschrieben. Sonst passiert bei Reload, dass diese wiederholt werden.
            var l = ToListOfByte();
            using (FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }
            _dataOnDisk = l;
            _LastSaveCode = GetFileInfo(Filename, true);
            _CheckedAndReloadNeed = false;
            CreateWatcher();
        }

        public void SetReadOnly() {
            Develop.DebugPrint(enFehlerArt.Info, "ReadOnly gesetzt<br>" + Filename);
            ReadOnly = true;
        }

        public void SetUserDidSomething() {
            _LastUserActionUTC = DateTime.UtcNow;
        }

        public void UnlockHard() {
            try {
                Load_Reload();
                if (AgeOfBlockDatei() >= 0) { DeleteBlockDatei(true, true); }
                Save(true);
            } catch {
            }
        }

        public void WaitEditable() {
            while (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut))) {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }// Nur anzeige-Dateien sind immer Schreibgeschützt
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
                Develop.DebugPrint(enFehlerArt.Fehler, "Darf nur von einem BackgroundThread aufgerufen werden!");
            }
            var x = DateTime.Now;
            while (IsParsing) {
                Develop.DoEvents();
                if (DateTime.Now.Subtract(x).TotalSeconds > 60) { return; }
            }
        }

        internal bool BlockDateiCheck() {
            if (AgeOfBlockDatei() < 0) {
                Develop.DebugPrint("Block-Datei Konflikt 4\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei);
                return false;
            }
            try {
                var Inhalt2 = File.ReadAllText(Blockdateiname(), System.Text.Encoding.UTF8);
                if (_inhaltBlockdatei != Inhalt2) {
                    Develop.DebugPrint("Block-Datei Konflikt 3\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei + "\r\n\r\nIst: " + Inhalt2);
                    return false;
                }
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }
            return true;
        }

        internal void ParseInternal(byte[] bLoaded) {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }
            IsParsing = true;
            ParseExternal(bLoaded);
            IsParsing = false;
            // Repair NACH ExecutePendung, vielleicht ist es schon repariert
            // Repair NACH _isParsing, da es auch abgespeichert werden soll
            OnParsed();
            RepairAfterParse();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="checkOnlyFilenameToo">Prüft, ob die Datei ohne Dateipfad - also nur Dateiname und Suffix - existiert und gibt diese zurück.</param>
        /// <returns></returns>
        protected static clsMultiUserFile GetByFilename(string filePath, bool checkOnlyFilenameToo) {
            //filePath = modConverter.SerialNr2Path(filePath);
            foreach (var ThisFile in AllFiles) {
                if (ThisFile != null && string.Equals(ThisFile.Filename, filePath, StringComparison.OrdinalIgnoreCase)) {
                    ThisFile.BlockReload(false);
                    return ThisFile;
                }
            }
            if (checkOnlyFilenameToo) {
                foreach (var ThisFile in AllFiles) {
                    if (ThisFile != null && ThisFile.Filename.ToLower().FileNameWithSuffix() == filePath.ToLower().FileNameWithSuffix()) {
                        ThisFile.BlockReload(false);
                        return ThisFile;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Alle Abfragen, die nicht durch Standard abfragen gehandelt werden kann.
        /// Z.B. Offen Dialoge, Prozesse die nur die die abgeleitete Klasse kennt
        /// Die offenen Dialoge werden aber bereits hier mit dem Event mit AreDiskOperationsBlocked abgefragt
        /// </summary>
        /// <returns></returns>
        protected virtual bool BlockDiskOperations() {
            var e = new CancelEventArgs();
            e.Cancel = false;
            OnShouldICancelDiscOperations(e);

            return e.Cancel;
        }

        /// <summary>
        /// gibt die Möglichkeit, Fehler in ein Protokoll zu schreiben, wenn nach dem Reload eine Inkonsitenz aufgetreten ist.
        /// Nicht für Reperaturzwecke gedacht.
        /// </summary>
        protected abstract void CheckDataAfterReload();

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                OnDisposing();
                AllFiles.Remove(this);
                if (disposing) {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }
                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.
                Save(false);
                while (PureBinSaver.IsBusy) { Pause(0.5, true); }
                SetReadOnly(); // Ja nix mehr speichern!!!
                // https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker
                PureBinSaver.Dispose();
                Checker.Dispose();
                Checker.Dispose();
                Disposed = true;
            }
        }

        protected abstract void DoBackGroundWork(BackgroundWorker listenToMyCancel);

        /// <summary>
        ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
        /// </summary>
        protected abstract void DoWorkAfterSaving();

        // Dient zur Erkennung redundanter Aufrufe.
        protected bool IsFileAllowedToLoad(string fileName) {
            foreach (var ThisFile in AllFiles) {
                if (ThisFile != null && string.Equals(ThisFile.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    ThisFile.Save(true);
                    Develop.DebugPrint(enFehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
            return true;
        }

        protected abstract bool IsThereBackgroundWorkToDo();

        protected void Load(string fileNameToLoad, bool createWhenNotExisting) {
            if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(enFehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
            if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);
            if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
            if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
            if (!FileExists(fileNameToLoad)) {
                if (createWhenNotExisting) {
                    if (ReadOnly) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                        return;
                    }
                    SaveAsAndChangeTo(fileNameToLoad);
                } else {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                    SetReadOnly();
                    return;
                }
            }
            Filename = fileNameToLoad;
            ReCreateWatcher();
            // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
            Load_Reload();
        }

        protected void LoadFromStream(Stream stream) {
            OnLoading(new LoadingEventArgs(false));
            byte[] _BLoaded = null;
            using (BinaryReader r = new(stream)) {
                _BLoaded = r.ReadBytes((int)stream.Length);
                r.Close();
            }
            if (_BLoaded.Length > 4 && BitConverter.ToInt32(_BLoaded, 0) == 67324752) {
                // Gezipte Daten-Kennung gefunden
                _BLoaded = UnzipIt(_BLoaded);
            }
            ParseInternal(_BLoaded);
            OnLoaded(new LoadedEventArgs(false));
            RepairOldBlockFiles();
        }

        protected virtual void OnLoaded(LoadedEventArgs e) {
            if (Disposed) { return; }
            Loaded?.Invoke(this, e);
        }

        protected abstract void ParseExternal(byte[] bLoaded);

        /// <summary>
        /// Gibt die Möglichkeit, vor einem Reload Daten in Variablen zu speichern. Diese kann nach dem Reload mit CheckDataAfterReload zu prüfen, ob alles geklappt hat.
        /// </summary>
        protected abstract void PrepeareDataForCheckingBeforeLoad();

        protected abstract byte[] ToListOfByte();

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) => DoBackGroundWork((BackgroundWorker)sender);

        private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

        private string Blockdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";

        private void Checker_Tick(object state) {
            if (DateTime.UtcNow.Subtract(_BlockReload).TotalSeconds < 5) { return; }
            if (IsLoading) { return; }
            if (PureBinSaver.IsBusy || IsSaving) { return; }
            if (string.IsNullOrEmpty(Filename)) { return; }

            Checker_Tick_count++;
            if (Checker_Tick_count < 0) { return; }

            // Ausstehende Arbeiten ermittelen
            var _editable = string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly));
            var _MustReload = ReloadNeeded;
            var _MustSave = _editable && HasPendingChanges();
            var _MustBackup = _editable && IsThereBackgroundWorkToDo();
            if (!_MustReload && !_MustSave && !_MustBackup) {
                RepairOldBlockFiles();
                Checker_Tick_count = 0;
                return;
            }

            // Zeiten berechnen
            _ReloadDelaySecond = Math.Max(_ReloadDelaySecond, 10);
            var Count_BackUp = Math.Min(_ReloadDelaySecond / 10f + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save
            var Count_Save = (Count_BackUp * 2) + 1; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird
            var Count_UserWork = (Count_Save / 5f) + 2; // Soviele Sekunden hat die User-Bearbeitung vorrang. Verhindert, dass die Bearbeitung des Users spontan abgebrochen wird.

            if (DateTime.UtcNow.Subtract(_LastUserActionUTC).TotalSeconds < Count_UserWork || BlockDiskOperations()) { CancelBackGroundWorker(); return; } // Benutzer arbeiten lassen

            if (Checker_Tick_count > Count_Save && _MustSave) { CancelBackGroundWorker(); }
            if (Checker_Tick_count > _ReloadDelaySecond && _MustReload) { CancelBackGroundWorker(); }
            if (BackgroundWorker.IsBusy) { return; }

            if (_MustReload && _MustSave) {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.Load))) { return; }
                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                Load_Reload();
                return;
            }

            if (_MustSave && Checker_Tick_count > Count_Save) {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.Save))) { return; }
                if (!PureBinSaver.IsBusy) { PureBinSaver.RunWorkerAsync(); } // Eigentlich sollte diese Abfrage überflüssig sein. Ist sie aber nicht
                Checker_Tick_count = 0;
                return;
            }

            if (_MustBackup && !_MustReload && !_MustSave && Checker_Tick_count >= Count_BackUp && string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut))) {
                var nowsek = (DateTime.UtcNow.Ticks - _startTick) / 30000000;
                if (nowsek % 20 != 0) { return; } // Lasten startabhängig verteilen. Bei Pending changes ist es eh immer true;

                StartBackgroundWorker();
                return;
            }

            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && Checker_Tick_count > _ReloadDelaySecond) {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.Load))) { return; }
                Load_Reload();
                Checker_Tick_count = 0;
            }
        }

        private bool CreateBlockDatei() {
            var tmpInhalt = UserName() + "\r\n" + DateTime.UtcNow.ToString(Constants.Format_Date5) + "\r\nThread: " + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;
            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try {
                var bInhalt = tmpInhalt.UTF8_ToByte();
                // Nicht modAllgemein, wegen den strengen Datei-Rechten
                using (FileStream x = new(Blockdateiname(), FileMode.Create, FileAccess.Write, FileShare.None)) {
                    x.Write(bInhalt, 0, bInhalt.Length);
                    x.Flush();
                    x.Close();
                }
                _inhaltBlockdatei = tmpInhalt;
            } catch {
                //Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }

            if (AgeOfBlockDatei() < 0) {
                Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + Filename);
                return false;
            }

            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);
            return BlockDateiCheck();
        }

        private void CreateWatcher() {
            if (!string.IsNullOrEmpty(Filename)) {
                Watcher = new FileSystemWatcher(Filename.FilePath());
                Watcher.Changed += Watcher_Changed;
                Watcher.Created += Watcher_Created;
                Watcher.Deleted += Watcher_Deleted;
                Watcher.Renamed += Watcher_Renamed;
                Watcher.Error += Watcher_Error;
                Watcher.EnableRaisingEvents = true;
            }
        }

        private bool DeleteBlockDatei(bool ignorechecking, bool mustDoIt) {
            if (ignorechecking || BlockDateiCheck()) {
                if (DeleteFile(Blockdateiname(), mustDoIt)) {
                    _inhaltBlockdatei = string.Empty;
                    return true;
                }
            }
            if (mustDoIt) {
                Develop.DebugPrint("Block-Datei nicht gelöscht\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei);
            }
            return false;
        }

        /// <summary>
        /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
        /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
        /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
        /// </summary>
        /// <param name="checkmode"></param>
        /// <returns></returns>
        private (byte[] data, string fileinfo) LoadBytesFromDisk(enErrorReason checkmode) {
            string tmpLastSaveCode2;
            var StartTime = DateTime.UtcNow;
            byte[] _BLoaded;
            while (true) {
                try {
                    if (_InitialLoadDone && !ReloadNeeded) { return (null, string.Empty); } // Problem hat sich aufgelöst
                    var f = ErrorReason(checkmode);
                    if (string.IsNullOrEmpty(f)) {
                        var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                        _BLoaded = File.ReadAllBytes(Filename);
                        tmpLastSaveCode2 = GetFileInfo(Filename, true);
                        if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }
                        f = "Datei wurde während des Ladens verändert.";
                    }

                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 20) {
                        Develop.DebugPrint(enFehlerArt.Warnung, f + "\r\n" + Filename);
                    }

                    Pause(0.5, false);
                } catch (Exception ex) {
                    // Home Office kann lange blokieren....
                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 300) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                        return (null, string.Empty);
                    }
                }
            }
            if (_BLoaded.Length > 4 && BitConverter.ToInt32(_BLoaded, 0) == 67324752) {
                // Gezipte Daten-Kennung gefunden
                _BLoaded = UnzipIt(_BLoaded);
            }
            return (_BLoaded, tmpLastSaveCode2);
        }

        private void OnDisposing() {
            if (Disposed) { return; }
            Disposing?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnLoading(LoadingEventArgs e) {
            if (Disposed) { return; }
            Loading?.Invoke(this, e);
        }

        private void OnParsed() {
            if (Disposed) { return; }
            Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnSavedToDisk() {
            if (Disposed) { return; }
            SavedToDisk?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnShouldICancelDiscOperations(CancelEventArgs e) {
            ShouldICancelDiscOperations?.Invoke(this, e);
        }

        private void PureBinSaver_DoWork(object sender, DoWorkEventArgs e) {
            try {
                var Data = WriteTempFileToDisk(true);
                PureBinSaver.ReportProgress(100, Data);
            } catch {
                // OPeration completed bereits aufgerufen
            }
        }

        private void PureBinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.UserState == null) { return; }
            var Data = (ValueTuple<string, string, byte[]>)e.UserState;
            // var Data = (Tuple<string, string, string>)e.UserState;
            SaveRoutine(true, Data.Item1, Data.Item2, Data.Item3);
        }

        private void ReCreateWatcher() {
            RemoveWatcher();
            CreateWatcher();
        }

        private void RemoveWatcher() {
            if (Watcher != null) {
                Watcher.EnableRaisingEvents = false;
                Watcher.Changed -= Watcher_Changed;
                Watcher.Created -= Watcher_Created;
                Watcher.Deleted -= Watcher_Deleted;
                Watcher.Renamed -= Watcher_Renamed;
                Watcher.Error -= Watcher_Error;
                Watcher.Dispose();
                Watcher = null;
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
            if (!fromParallelProzess && PureBinSaver.IsBusy) { return Feedback("Anderer interner binärer Speichervorgang noch nicht abgeschlossen."); }
            if (fromParallelProzess && IsInSaveingLoop) { return Feedback("Anderer manuell ausgelöster binärer Speichervorgang noch nicht abgeschlossen."); }
            var f = ErrorReason(enErrorReason.Save);
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
            if (!savedDataUncompressed.SequenceEqual(ToListOfByte())) {
                DeleteBlockDatei(false, true);
                IsSaving = false;
                return Feedback("Daten wurden inzwischen verändert.");
            }
            // OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(Backupdateiname())) { DeleteFile(Backupdateiname(), true); }
            // Haupt-Datei wird zum Backup umbenannt
            RenameFile(Filename, Backupdateiname(), true);
            // --- TmpFile wird zum Haupt ---
            RenameFile(tmpFileName, Filename, true);
            // ---- Steuerelemente Sagen, was gespeichert wurde
            _dataOnDisk = savedDataUncompressed;
            // Und nun den Block entfernen
            CanWrite(Filename, 30); // sobald die Hauptdatei wieder frei ist
            DeleteBlockDatei(false, true);
            // Evtl. das BAK löschen
            if (AutoDeleteBAK && FileExists(Backupdateiname())) {
                DeleteFile(Backupdateiname(), false);
            }
            // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
            (var data, var fileinfo) = LoadBytesFromDisk(enErrorReason.LoadForCheckingOnly);
            if (data == null || !savedDataUncompressed.SequenceEqual(data)) {
                // OK, es sind andere Daten auf der Festplatte?!? Seltsam, zählt als sozusagen ungespeichter und ungeladen.
                _CheckedAndReloadNeed = true;
                _LastSaveCode = "Fehler";
                Develop.DebugPrint(enFehlerArt.Warnung, "Speichern fehlgeschlagen!!! " + Filename);
                IsSaving = false;
                return Feedback("Speichervorgang fehlgeschlagen.");
            } else {
                _CheckedAndReloadNeed = false;
                _LastSaveCode = fileinfo;
            }
            DoWorkAfterSaving();
            OnSavedToDisk();
            IsSaving = false;
            return string.Empty;
            string Feedback(string txt) {
                DeleteFile(tmpFileName, false);
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br>Grund:<br>" + txt);
                RepairOldBlockFiles();
                return txt;
            }
        }

        private void StartBackgroundWorker() {
            try {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }
                if (!BackgroundWorker.IsBusy) { BackgroundWorker.RunWorkerAsync(); }
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
                        Develop.DebugPrint(enFehlerArt.Warnung, "WaitLoaded hängt: " + Filename);
                    }
                    //Develop.DebugPrint(enFehlerArt.Warnung, "WaitLoaded hängt: " + Filename);
                    return;
                }
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e) {
            if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            _CheckedAndReloadNeed = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e) {
            if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            _CheckedAndReloadNeed = true;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
            if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            _CheckedAndReloadNeed = true;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e) =>
                                    // Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
                                    _CheckedAndReloadNeed = true;

        private void Watcher_Renamed(object sender, RenamedEventArgs e) {
            if (!string.Equals(e.FullPath, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            _CheckedAndReloadNeed = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="iAmThePureBinSaver"></param>
        /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
        private (string TMPFileName, string FileInfoBeforeSaving, byte[] DataUncompressed) WriteTempFileToDisk(bool iAmThePureBinSaver) {
            string FileInfoBeforeSaving;
            string TMPFileName;
            byte[] DataUncompressed;
            byte[] Writer_BinaryData;
            var count = 0;
            if (!iAmThePureBinSaver && PureBinSaver.IsBusy) { return (string.Empty, string.Empty, null); }
            if (_DoingTempFile) {
                if (!iAmThePureBinSaver) { Develop.DebugPrint("Erstelle bereits TMP-File"); }
                return (string.Empty, string.Empty, null);
            }
            _DoingTempFile = true;
            while (true) {
                if (!iAmThePureBinSaver) {
                    // Also, im NICHT-parallelen Prozess ist explizit der Save angestoßen worden.
                    // Somit sollte des Prgramm auf Warteschleife sein und keine Benutzereingabe mehr kommen.
                    // Problem: Wenn die ganze Save-Routine in einem Parallelen-Thread ist
                    _LastUserActionUTC = new DateTime(1900, 1, 1);
                }
                var f = ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { _DoingTempFile = false; return (string.Empty, string.Empty, null); }
                FileInfoBeforeSaving = GetFileInfo(Filename, true);
                DataUncompressed = ToListOfByte();
                Writer_BinaryData = _zipped ? ZipIt(DataUncompressed) : DataUncompressed;
                TMPFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName().ToUpper());
                try {
                    using FileStream x = new(TMPFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    x.Write(Writer_BinaryData, 0, Writer_BinaryData.Length);
                    x.Flush();
                    x.Close();
                    break;
                } catch (Exception ex) {
                    // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen
                    count++;
                    if (count > 15) {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        _DoingTempFile = false;
                        return (string.Empty, string.Empty, null);
                    }
                    Pause(1, true);
                }
            }
            _DoingTempFile = false;
            return (TMPFileName, FileInfoBeforeSaving, DataUncompressed);
        }

        private byte[] ZipIt(byte[] data) {
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

        #endregion
    }
}