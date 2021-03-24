#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;

namespace BlueBasics.MultiUserFile {
    public abstract class clsMultiUserFile : IDisposable {




        #region Shareds
        public static readonly List<clsMultiUserFile> AllFiles = new();



        /// <summary>
        /// 
        /// </summary>
        /// <param name="mustSave">Falls TRUE wird erst ein Speichervorgang mit Fals eingeleitet, um so viel wie mögloch zu speichern, falls eine Datei blockiert ist.</param>
        public static void SaveAll(bool mustSave) {

            if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss


            //Parallel.ForEach(AllFiles, thisFile => {
            //    thisFile?.Save(mustSave);
            //});




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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="checkOnlyFilenameToo">Prüft, ob die Datei ohne Dateipfad - also nur Dateiname und Suffix - existiert und gibt diese zurück.</param>
        /// <returns></returns>
        public static clsMultiUserFile GetByFilename(string filePath, bool checkOnlyFilenameToo) {
            filePath = modConverter.SerialNr2Path(filePath);

            foreach (var ThisFile in AllFiles) {
                if (ThisFile != null && ThisFile.Filename.ToLower() == filePath.ToLower()) { return ThisFile; }
            }


            if (checkOnlyFilenameToo) {
                foreach (var ThisFile in AllFiles) {
                    if (ThisFile != null && ThisFile.Filename.ToLower().FileNameWithSuffix() == filePath.ToLower().FileNameWithSuffix()) { return ThisFile; }
                }
            }
            return null;
        }


        #endregion

        #region Variablen und Properties

        private readonly System.ComponentModel.BackgroundWorker BackgroundWorker;
        private readonly System.ComponentModel.BackgroundWorker PureBinSaver;
        private readonly Timer Checker;
        private FileSystemWatcher Watcher;


        private string _inhaltBlockdatei = string.Empty;

        private readonly bool _zipped;

        private bool _CheckedAndReloadNeed;

        private DateTime _LastMessageUTC = DateTime.UtcNow.AddMinutes(-10);

        private string _LastSaveCode;

        protected Byte[] _dataOnDisk;

        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Load oder SaveAsAndChangeTo benutzen
        /// </summary>
        public string Filename { get; private set; }

        public bool AutoDeleteBAK { get; set; }

        protected int _ReloadDelaySecond = 10;

        public bool IsParsing { get; private set; } = false;

        public bool IsLoading { get; private set; } = false;

        private int _loadingThreadId = -1;
        //private string _loadingInfo = string.Empty;




        public bool IsSaving { get; private set; } = false;


        /// <summary>
        /// Ab aktuell die "Save" Routine vom Code aufgerufen wird, und diese auf einen erfolgreichen Speichervorgang abwartet
        /// </summary>
        public bool IsInSaveingLoop { get; private set; } = false;


        private bool _DoingTempFile = false;

        public DateTime UserEditedAktionUTC;


        private DateTime _CanWriteNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _CanWriteError = string.Empty;

        private DateTime _EditNormalyNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _EditNormalyError = string.Empty;


        private DateTime _BlockReload = new(1900, 1, 1);

        #endregion


        public void BlockReload() {

            WaitLoaded(false);
            if (IsInSaveingLoop) { return; } //Ausnahme, bearbeitung sollte eh blockiert sein...
            if (IsSaving) { return; }
            _BlockReload = DateTime.UtcNow;
        }

        #region  Event-Deklarationen 

        public event EventHandler<LoadedEventArgs> Loaded;
        public event EventHandler<LoadingEventArgs> Loading;
        public event EventHandler SavedToDisk;
        public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;
        public static event EventHandler<MultiUserFileGiveBackEventArgs> MultiUserFileCreated;

        /// <summary>
        /// Wird ausgegeben, sobald isParsed false ist, noch vor den automatischen Reperaturen.
        /// Dieses Event kann verwendet werden, um die Datei automatisch zu reparieren, bevor sich automatische Dialoge öffnen.
        /// </summary>
        public event EventHandler Parsed;

        #endregion



        protected clsMultiUserFile(bool readOnly, bool zipped) {

            _zipped = zipped;


            AllFiles.Add(this);
            OnMultiUserFileCreated(this);

            PureBinSaver = new System.ComponentModel.BackgroundWorker {
                WorkerReportsProgress = true
            };
            PureBinSaver.DoWork += PureBinSaver_DoWork;
            PureBinSaver.ProgressChanged += PureBinSaver_ProgressChanged;



            BackgroundWorker = new System.ComponentModel.BackgroundWorker {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            BackgroundWorker.ProgressChanged += Backup_ProgressChanged;



            Checker = new Timer(Checker_Tick);

            Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            DoWatcher();
            _CheckedAndReloadNeed = true;
            _LastSaveCode = string.Empty;
            _dataOnDisk = new byte[0];
            ReadOnly = readOnly;
            AutoDeleteBAK = false;
            UserEditedAktionUTC = new DateTime(1900, 1, 1);

            Checker.Change(2000, 2000);
        }

        private void PureBinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.UserState == null) { return; }

            var Data = (ValueTuple<string, string, byte[]>)e.UserState;

            //          var Data = (Tuple<string, string, string>)e.UserState;
            SaveRoutine(true, Data.Item1, Data.Item2, Data.Item3);
        }

        private void PureBinSaver_DoWork(object sender, DoWorkEventArgs e) {

            try {
                var Data = WriteTempFileToDisk(true);
                PureBinSaver.ReportProgress(100, Data);
            } catch {
                // OPeration completed bereits aufgerufen
            }

        }

        public void SetReadOnly() {
            Develop.DebugPrint(enFehlerArt.Info, "ReadOnly gesetzt<br>" + Filename);
            ReadOnly = true;
        }

        public void RemoveFilename() {
            Filename = string.Empty;
            DoWatcher();
            SetReadOnly();
        }


        /// <summary>
        /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
        /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
        /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
        /// </summary>
        /// <param name="onlyReload"></param>
        /// <returns></returns>
        private (byte[] data, string fileinfo) LoadBytesFromDisk(bool onlyReload, enErrorReason checkmode) {
            var tmpLastSaveCode2 = string.Empty;

            var StartTime = DateTime.UtcNow;
            byte[] _BLoaded;
            do {
                try {

                    if (onlyReload && !ReloadNeeded) { return (null, string.Empty); } // Problem hat sich aufgelöst

                    var f = ErrorReason(checkmode);

                    if (string.IsNullOrEmpty(f)) {
                        var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                        _BLoaded = File.ReadAllBytes(Filename);
                        tmpLastSaveCode2 = GetFileInfo(Filename, true);

                        if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }

                        f = "Datei wurde während des Ladens verändert.";

                    }

                    Develop.DebugPrint(enFehlerArt.Info, f + "\r\n" + Filename);
                    Pause(0.5, false);
                } catch (Exception ex) {
                    // Home Office kann lange blokieren....
                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 300) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                        return (null, string.Empty);
                    }
                }
            } while (true);



            if (_BLoaded.Length > 4 && BitConverter.ToInt32(_BLoaded, 0) == 67324752) {
                // Gezipte Daten-Kennung gefunden
                _BLoaded = UnzipIt(_BLoaded);
            }

            return (_BLoaded, tmpLastSaveCode2);
        }


        /// <summary>
        /// Wartet bis der Reload abgeschlossen ist.
        /// Ist de LoadThread der aktuelle Thread, wir nicht gewartet!
        /// </summary>
        /// <param name="hardmode"></param>
        private void WaitLoaded(bool hardmode) {
            if (_loadingThreadId == Thread.CurrentThread.ManagedThreadId) { return; }

            var x = DateTime.Now;

            while (IsLoading) {
                Develop.DoEvents();

                if (!hardmode && !IsParsing) {
                    return;
                }

                if (DateTime.Now.Subtract(x).TotalMinutes > 1) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "WaitLoaded hängt: " + Filename);
                    return;
                }

            }
        }


        //private void WaitSaved()
        //{
        //    var x = DateTime.Now;


        //    while (_IsSaving)
        //    {
        //        Develop.DoEvents();
        //        if (DateTime.Now.Subtract(x).TotalMinutes > 2)
        //        {
        //            Develop.DebugPrint(enFehlerArt.Fehler, "WaitSaved hängt");
        //            return;
        //        }

        //    }
        //}

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


            //var strace = new System.Diagnostics.StackTrace(true);
            //_loadingInfo = DateTime.Now.ToString(Constants.Format_Date) + " " + _BlockReload.ToString() + " #U " + Thread.CurrentThread.ManagedThreadId + " " + strace.GetFrame(1).GetMethod().ReflectedType.FullName + "/" + strace.GetFrame(1).GetMethod().ToString();

            //Wichtig, das _LastSaveCode geprüft wird, das ReloadNeeded im EasyMode immer false zurück gibt.
            if (!string.IsNullOrEmpty(_LastSaveCode) && !ReloadNeeded) { IsLoading = false; return; }

            var OnlyReload = !string.IsNullOrEmpty(_LastSaveCode);

            if (OnlyReload && !ReloadNeeded) { IsLoading = false; return; } // Wird in der Schleife auch geprüft

            var ec = new LoadingEventArgs(OnlyReload);
            OnLoading(ec);

            if (OnlyReload && ReadOnly && ec.TryCancel) { IsLoading = false; return; }

            (var _BLoaded, var tmpLastSaveCode) = LoadBytesFromDisk(OnlyReload, enErrorReason.Load);
            if (_BLoaded == null) { IsLoading = false; return; }


            _dataOnDisk = _BLoaded;

            PrepeareDataForCheckingBeforeLoad();
            ParseInternal(_BLoaded);
            _LastSaveCode = tmpLastSaveCode; // initialize setzt zurück
            _CheckedAndReloadNeed = false;

            CheckDataAfterReload();

            OnLoaded(new LoadedEventArgs(OnlyReload));
            RepairOldBlockFiles();

            IsLoading = false;
        }

        /// <summary>
        /// gibt die Möglichkeit, Fehler in ein Protokoll zu schreiben, wenn nach dem Reload eine Inkonsitenz aufgetreten ist.
        /// Nicht für Reperaturzwecke gedacht.
        /// </summary>
        protected abstract void CheckDataAfterReload();

        /// <summary>
        /// Gibt die Möglichkeit, vor einem Reload Daten in Variablen zu speichern. Diese kann nach dem Reload mit CheckDataAfterReload zu prüfen, ob alles geklappt hat.
        /// </summary>
        protected abstract void PrepeareDataForCheckingBeforeLoad();


        protected void LoadFromStream(Stream Stream) {


            OnLoading(new LoadingEventArgs(false));


            byte[] _BLoaded = null;

            using (var r = new BinaryReader(Stream)) {
                _BLoaded = r.ReadBytes((int)Stream.Length);
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


        internal void ParseInternal(byte[] bLoaded) {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }

            IsParsing = true;
            ParseExternal(bLoaded);

            IsParsing = false;

            //Repair NACH ExecutePendung, vielleicht ist es schon repariert
            //Repair NACH _isParsing, da es auch abgespeichert werden soll
            OnParsed();


            RepairAfterParse();
        }

        public abstract void RepairAfterParse();
        protected abstract void ParseExternal(byte[] bLoaded);

        /// <summary>
        /// Entfernt im Regelfall die Temporäre Datei
        /// </summary>
        /// <param name="fromParallelProzess"></param>
        /// <param name="tmpFileName"></param>
        /// <param name="fileInfoBeforeSaving"></param>
        /// <param name="savedDataUncompressedUTF8"></param>
        /// <returns></returns>
        private string SaveRoutine(bool fromParallelProzess, string tmpFileName, string fileInfoBeforeSaving, byte[] savedDataUncompressed) {


            if (ReadOnly) { return Feedback("Datei ist Readonly"); }

            if (!ReadOnly) { return Feedback("Datei ist Readonly"); }

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


            if (!savedDataUncompressed.SequenceEqual(ToListOfByte(false))) {
                DeleteBlockDatei(false, true);
                IsSaving = false;
                return Feedback("Daten wurden inzwischen verändert.");
            }


            //OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(Backupdateiname())) { DeleteFile(Backupdateiname(), true); }

            //Haupt-Datei wird zum Backup umbenannt
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
            (var data, var fileinfo) = LoadBytesFromDisk(false, enErrorReason.LoadForCheckingOnly);
            if (!savedDataUncompressed.SequenceEqual(data)) {
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


        internal bool BlockDateiCheck() {

            if (AgeOfBlockDatei() < 0) {
                Develop.DebugPrint("Block-Datei Konflikt 4\r\n" + Filename + "\r\nSoll: " + _inhaltBlockdatei);
                return false;
            }

            try {
                var Inhalt2 = LoadFromDiskUTF8(Blockdateiname());
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


        private bool CreateBlockDatei() {
            var tmpInhalt = UserName() + "\r\n" + DateTime.UtcNow.ToString(Constants.Format_Date5) + "\r\nThread: " + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;

            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try {
                var bInhalt = tmpInhalt.ToByteUTF8();
                //Nicht modAllgemein, wegen den strengen Datei-Rechten 
                using (var x = new FileStream(Blockdateiname(), FileMode.Create, FileAccess.Write, FileShare.None)) {
                    x.Write(bInhalt, 0, bInhalt.Length);
                    x.Flush();
                    x.Close();
                }
                _inhaltBlockdatei = tmpInhalt;

            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }


            //if (!done)
            //{
            //    // Letztens aufgetreten, dass eine Blockdatei schon vorhanden war. Anscheinden Zeitgleiche Kopie?
            //    Develop.DebugPrint(enFehlerArt.Info, "Befehl anscheinend abgebrochen:\r\n" + Filename);
            //    return false;
            //}

            if (AgeOfBlockDatei() < 0) {
                Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + Filename);
                return false;
            }

            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);

            return BlockDateiCheck();
        }

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

        protected abstract byte[] ToListOfByte(bool willSave);


        /// <summary>
        ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
        /// </summary>
        protected abstract void DoWorkAfterSaving();

        protected abstract bool isSomethingDiscOperatingsBlocking();


        protected void Load(string fileNameToLoad, bool CreateWhenNotExisting) {

            if (fileNameToLoad.ToUpper() == Filename.ToUpper()) { return; }

            if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(enFehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }

            if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);


            if (!CreateWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }


            if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

            if (!FileExists(fileNameToLoad)) {

                if (CreateWhenNotExisting) {
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
            DoWatcher();


            // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
            Load_Reload();
            //var count = 0;
            //do {

            //    if (count > 0) { Pause(1, false); }

            //    count++;
            //    if (count > 10) { Develop.DebugPrint(enFehlerArt.Fehler, "Datei nicht korrekt geladen (nicht mehr aktuell)"); }
            //} while (ReloadNeeded());
        }

        private void DoWatcher() {



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

        private void Watcher_Error(object sender, ErrorEventArgs e) {
            //Develop.DebugPrint(enFehlerArt.Warnung, e.ToString());
            // Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
            _CheckedAndReloadNeed = true;
        }
        private void Watcher_Renamed(object sender, RenamedEventArgs e) {
            if (e.FullPath.ToUpper() != Filename.ToUpper()) { return; }
            _CheckedAndReloadNeed = true;
        }
        private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
            if (e.FullPath.ToUpper() != Filename.ToUpper()) { return; }

            _CheckedAndReloadNeed = true;
        }
        private void Watcher_Created(object sender, FileSystemEventArgs e) {
            if (e.FullPath.ToUpper() != Filename.ToUpper()) { return; }

            _CheckedAndReloadNeed = true;
        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e) {

            if (e.FullPath.ToUpper() != Filename.ToUpper()) { return; }
            _CheckedAndReloadNeed = true;

        }

        public void SaveAsAndChangeTo(string NewFileName) {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (NewFileName.ToUpper() == Filename.ToUpper()) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            Save(true); // Original-Datei speichern, die ist ja dann weg.

            Filename = NewFileName;
            DoWatcher();
            var l = ToListOfByte(true);

            using (var x = new FileStream(NewFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }

            _dataOnDisk = l;


            _LastSaveCode = GetFileInfo(Filename, true);
            _CheckedAndReloadNeed = false;
        }

        private void OnLoading(LoadingEventArgs e) {
            Loading?.Invoke(this, e);
        }

        protected virtual void OnLoaded(LoadedEventArgs e) {
            Loaded?.Invoke(this, e);
        }




        private void OnSavedToDisk() {
            SavedToDisk?.Invoke(this, System.EventArgs.Empty);
        }

        public void RepairOldBlockFiles() {

            if (DateTime.UtcNow.Subtract(_LastMessageUTC).TotalMinutes < 1) { return; }
            _LastMessageUTC = DateTime.UtcNow;

            var sec = AgeOfBlockDatei();


            try {

                //Nach 15 Minuten versuchen die Datei zu reparieren
                if (sec >= 900) {
                    if (!FileExists(Filename)) { return; }

                    var x = LoadFromDiskUTF8(Blockdateiname());
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
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }


        private void OnParsed() {
            Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
        /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
        /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
        /// </summary>
        public bool Save(bool mustSave) {

            if (ReadOnly) { return false; }

            if (IsInSaveingLoop) { return false; }

            if (isSomethingDiscOperatingsBlocking()) {
                if (!mustSave) { RepairOldBlockFiles(); return false; }
                Develop.DebugPrint(enFehlerArt.Warnung, "Release unmöglich, Dateistatus geblockt");
                return false;
            }

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


        private byte[] UnzipIt(byte[] data) {
            using var originalFileStream = new MemoryStream(data);
            using var zipArchive = new ZipArchive(originalFileStream);
            var entry = zipArchive.GetEntry("Main.bin");

            using var stream = entry.Open();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            
            return ms.ToArray();
        }


        private byte[] ZipIt(byte[] data) {
            // https://stackoverflow.com/questions/17217077/create-zip-file-from-byte

            using var compressedFileStream = new MemoryStream();
            //Create an archive and store the stream in memory.
            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false)) {

                //Create a zip entry for each attachment
                var zipEntry = zipArchive.CreateEntry("Main.bin");

                //Get the stream of the attachment
                using var originalFileStream = new MemoryStream(data);
                using var zipEntryStream = zipEntry.Open();
                //Copy the attachment stream to the zip entry stream
                originalFileStream.CopyTo(zipEntryStream);

            }

            return compressedFileStream.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
        private (string TMPFileName, string FileInfoBeforeSaving, byte[] DataUncompressed) WriteTempFileToDisk(bool iAmThePureBinSaver) {

            string FileInfoBeforeSaving;
            string TMPFileName;
            byte[] DataUncompressed;


            byte[] Writer_BinaryData;

            var count = 0;


            if (!iAmThePureBinSaver && PureBinSaver.IsBusy) { return (string.Empty, string.Empty, null); }

            if (_DoingTempFile) {
                if (!iAmThePureBinSaver) { Develop.DebugPrint("Ersteller schon temp File"); }
                return (string.Empty, string.Empty, null);
            }

            _DoingTempFile = true;

            do {

                if (!iAmThePureBinSaver) {
                    // Also, im NICHT-parallelen Prozess ist explizit der Save angestoßen worden.
                    // Somit sollte des Prgramm auf Warteschleife sein und keine Benutzereingabe mehr kommen.
                    // Problem: Wenn die ganze Save-Routine in einem Parallelen-Thread ist
                    UserEditedAktionUTC = new DateTime(1900, 1, 1);
                }

                var f = ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { _DoingTempFile = false; return (string.Empty, string.Empty, null); }

                FileInfoBeforeSaving = GetFileInfo(Filename, true);

                DataUncompressed = ToListOfByte(true);
                if (_zipped) {
                    Writer_BinaryData = ZipIt(DataUncompressed);
                } else {
                    Writer_BinaryData = DataUncompressed;
                }

                TMPFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + modAllgemein.UserName().ToUpper());

                try {
                    using var x = new FileStream(TMPFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    x.Write(Writer_BinaryData, 0, Writer_BinaryData.Length);
                    x.Flush();
                    x.Close();

                    break;
                } catch (Exception ex) {
                    //  DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen

                    count++;
                    if (count > 15) {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        _DoingTempFile = false;
                        return (string.Empty, string.Empty, null);
                    }
                    Pause(1, true);
                }
            } while (true);

            _DoingTempFile = false;
            return (TMPFileName, FileInfoBeforeSaving, DataUncompressed);

        }

        public abstract bool HasPendingChanges();

        public void UnlockHard() {
            try {
                Load_Reload();
                if (AgeOfBlockDatei() >= 0) { DeleteBlockDatei(true, true); }
                Save(true);
            } catch {
            }
        }
        private string Backupdateiname() {
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            return Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";
        }


        private string Blockdateiname() {
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            return Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
        public double AgeOfBlockDatei() {
            if (!FileExists(Blockdateiname())) { return -1; }

            var f = new FileInfo(Blockdateiname());

            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;


            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }


        public void OnConnectedControlsStopAllWorking(MultiUserFileStopWorkingEventArgs e) {
            if (e.AllreadyStopped.Contains(Filename.ToLower())) { return; }
            e.AllreadyStopped.Add(Filename.ToLower());
            ConnectedControlsStopAllWorking?.Invoke(this, e);
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

        private int Checker_Tick_count = -5;

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
            var Count_BackUp = Math.Min((int)(_ReloadDelaySecond / 10.0) + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save
            var Count_Save = Count_BackUp * 2 + 1; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird
            var Count_UserWork = Count_Save / 5 + 2; // Soviele Sekunden hat die User-Bearbeitung vorrang. Verhindert, dass die Bearbeitung des Users spontan abgebrochen wird.



            if (DateTime.UtcNow.Subtract(UserEditedAktionUTC).TotalSeconds < Count_UserWork) { return; } // Benutzer arbeiten lassen
            if (Checker_Tick_count > Count_Save && _MustSave) { CancelBackGroundWorker(); }
            if (Checker_Tick_count > _ReloadDelaySecond && _MustReload) { CancelBackGroundWorker(); }
            if (BackgroundWorker.IsBusy) { return; }



            if (_MustBackup && !_MustReload && Checker_Tick_count < Count_Save && Checker_Tick_count >= Count_BackUp && string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut))) {
                StartBackgroundWorker();
                return;
            }


            if (_MustReload && _MustSave) {
                var f = ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }
                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                Load_Reload();
                return;
            }



            if (_MustSave && Checker_Tick_count > Count_Save) {
                var f = ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { return; }


                // Eigentlich sollte die folgende Abfrage überflüssig sein. Ist sie aber nicht
                if (!PureBinSaver.IsBusy) { PureBinSaver.RunWorkerAsync(); }
                Checker_Tick_count = 0;
                return;
            }


            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && Checker_Tick_count > _ReloadDelaySecond && string.IsNullOrEmpty(ErrorReason(enErrorReason.Load))) {
                var f = ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }
                Load_Reload();
                Checker_Tick_count = 0;
            }

        }

        public void CancelBackGroundWorker() {
            if (BackgroundWorker.IsBusy && !BackgroundWorker.CancellationPending) { BackgroundWorker.CancelAsync(); }
        }


        private void StartBackgroundWorker() {
            if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }
            if (!BackgroundWorker.IsBusy) { BackgroundWorker.RunWorkerAsync(); }
        }

        private void Backup_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            BackgroundWorkerMessage(e);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            DoBackGroundWork((BackgroundWorker)sender);
        }

        protected abstract void DoBackGroundWork(BackgroundWorker listenToMyCancel);
        protected abstract void BackgroundWorkerMessage(ProgressChangedEventArgs e);
        protected abstract bool IsThereBackgroundWorkToDo();



        public virtual string ErrorReason(enErrorReason mode) {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }

            //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------

            if (mode == enErrorReason.Load || mode == enErrorReason.LoadForCheckingOnly) {
                if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
                var sec = AgeOfBlockDatei();
                if (sec >= 0 && sec < 10) { return "Ein anderer Computer speichert gerade Daten ab."; }
            }


            if (mode == enErrorReason.Load) {
                var x = DateTime.UtcNow.Subtract(_BlockReload).TotalSeconds;
                if (x < 5) { return "Laden noch " + (5 - x).ToString() + " Sekunden blockiert."; }

                if (DateTime.UtcNow.Subtract(UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen 

                if (PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (BackgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
                if (IsParsing) { return "Es werden aktuell Daten geparsed."; }

                if (isSomethingDiscOperatingsBlocking()) { return "Reload unmöglich, vererbte Klasse gab Fehler zurück"; }
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

                if (DateTime.UtcNow.Subtract(UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4


                if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
                if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------

                if (CheckForLastError(ref _CanWriteNextCheckUTC, ref _CanWriteError)) {
                    if (!string.IsNullOrEmpty(_CanWriteError)) { return _CanWriteError; }
                }


                if (AgeOfBlockDatei() >= 0) {
                    _CanWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                    return _CanWriteError;
                }


                try {
                    var f2 = new FileInfo(Filename);
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
            bool CheckForLastError(ref DateTime nextCheckUTC, ref string lastMessage) {
                if (DateTime.UtcNow.Subtract(nextCheckUTC).TotalSeconds < 0) { return true; }
                lastMessage = string.Empty;
                nextCheckUTC = DateTime.UtcNow.AddSeconds(5);
                return false;
            }

        }

        public void WaitEditable() {
            while (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut))) {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }// Nur anzeige-Dateien sind immer Schreibgeschützt
                Pause(0.2, true);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                Save(false);
                while (PureBinSaver.IsBusy) { Pause(0.5, true); }


                //  https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker

                PureBinSaver.Dispose();
                Checker.Dispose();

                Checker.Dispose();

                disposedValue = true;
            }
        }

        //TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        ~clsMultiUserFile() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(false);
        }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }
        #endregion



        protected static void OnMultiUserFileCreated(clsMultiUserFile file) {
            var e = new MultiUserFileGiveBackEventArgs {
                File = file
            };
            MultiUserFileCreated?.Invoke(null, e);
        }


        protected bool IsFileAllowedToLoad(string fileName) {
            foreach (var ThisFile in AllFiles) {
                if (ThisFile != null && ThisFile.Filename.ToLower() == fileName.ToLower()) {
                    ThisFile.Save(true);
                    Develop.DebugPrint(enFehlerArt.Fehler, "Doppletes Laden von " + fileName);
                    return false;
                }
            }

            return true;
        }

    }
}
