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

namespace BlueBasics.MultiUserFile
{
    public abstract class clsMultiUserFile : IDisposable
    {
        #region Shareds
        public static readonly List<clsMultiUserFile> AllFiles = new();

        /// <summary>
        ///
        /// </summary>
        /// <param name="mustSave">Falls TRUE wird erst ein Speichervorgang mit Fals eingeleitet, um so viel wie mögloch zu speichern, falls eine Datei blockiert ist.</param>
        public static void SaveAll(bool mustSave)
        {
            if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss

            // Parallel.ForEach(AllFiles, thisFile => {
            //    thisFile?.Save(mustSave);
            // });

            var x = AllFiles.Count;

            foreach (var thisFile in AllFiles)
            {
                thisFile?.Save(mustSave);

                if (x != AllFiles.Count)
                {
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
        public static clsMultiUserFile GetByFilename(string filePath, bool checkOnlyFilenameToo)
        {
            //filePath = modConverter.SerialNr2Path(filePath);

            foreach (var ThisFile in AllFiles)
            {
                if (ThisFile != null && string.Equals(ThisFile.Filename, filePath, StringComparison.OrdinalIgnoreCase)) { return ThisFile; }
            }

            if (checkOnlyFilenameToo)
            {
                foreach (var ThisFile in AllFiles)
                {
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

        protected byte[] _dataOnDisk;

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
        // private string _loadingInfo = string.Empty;

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

        public void BlockReload()
        {
            this.WaitLoaded(false);
            if (this.IsInSaveingLoop) { return; } // Ausnahme, bearbeitung sollte eh blockiert sein...
            if (this.IsSaving) { return; }
            this._BlockReload = DateTime.UtcNow;
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

        protected clsMultiUserFile(bool readOnly, bool zipped)
        {
            this._zipped = zipped;

            AllFiles.Add(this);
            OnMultiUserFileCreated(this);

            this.PureBinSaver = new System.ComponentModel.BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            this.PureBinSaver.DoWork += this.PureBinSaver_DoWork;
            this.PureBinSaver.ProgressChanged += this.PureBinSaver_ProgressChanged;

            this.BackgroundWorker = new System.ComponentModel.BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };
            this.BackgroundWorker.DoWork += this.BackgroundWorker_DoWork;
            this.BackgroundWorker.ProgressChanged += this.Backup_ProgressChanged;

            this.Checker = new Timer(this.Checker_Tick);

            this.Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            this.DoWatcher();
            this._CheckedAndReloadNeed = true;
            this._LastSaveCode = string.Empty;
            this._dataOnDisk = new byte[0];
            this.ReadOnly = readOnly;
            this.AutoDeleteBAK = false;
            this.UserEditedAktionUTC = new DateTime(1900, 1, 1);

            this.Checker.Change(2000, 2000);
        }

        private void PureBinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) { return; }

            var Data = (ValueTuple<string, string, byte[]>)e.UserState;

            // var Data = (Tuple<string, string, string>)e.UserState;
            this.SaveRoutine(true, Data.Item1, Data.Item2, Data.Item3);
        }

        private void PureBinSaver_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var Data = this.WriteTempFileToDisk(true);
                this.PureBinSaver.ReportProgress(100, Data);
            }
            catch
            {
                // OPeration completed bereits aufgerufen
            }
        }

        public void SetReadOnly()
        {
            Develop.DebugPrint(enFehlerArt.Info, "ReadOnly gesetzt<br>" + this.Filename);
            this.ReadOnly = true;
        }

        public void RemoveFilename()
        {
            this.Filename = string.Empty;
            this.DoWatcher();
            this.SetReadOnly();
        }

        /// <summary>
        /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
        /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
        /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
        /// </summary>
        /// <param name="onlyReload"></param>
        /// <param name="checkmode"></param>
        /// <returns></returns>
        private (byte[] data, string fileinfo) LoadBytesFromDisk(bool onlyReload, enErrorReason checkmode)
        {
            var tmpLastSaveCode2 = string.Empty;

            var StartTime = DateTime.UtcNow;
            byte[] _BLoaded;
            while (true)
            {
                try
                {
                    if (onlyReload && !this.ReloadNeeded) { return (null, string.Empty); } // Problem hat sich aufgelöst

                    var f = this.ErrorReason(checkmode);

                    if (string.IsNullOrEmpty(f))
                    {
                        var tmpLastSaveCode1 = GetFileInfo(this.Filename, true);
                        _BLoaded = File.ReadAllBytes(this.Filename);
                        tmpLastSaveCode2 = GetFileInfo(this.Filename, true);

                        if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }

                        f = "Datei wurde während des Ladens verändert.";
                    }

                    Develop.DebugPrint(enFehlerArt.Info, f + "\r\n" + this.Filename);
                    Pause(0.5, false);
                }
                catch (Exception ex)
                {
                    // Home Office kann lange blokieren....
                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 300)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + this.Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                        return (null, string.Empty);
                    }
                }
            }

            if (_BLoaded.Length > 4 && BitConverter.ToInt32(_BLoaded, 0) == 67324752)
            {
                // Gezipte Daten-Kennung gefunden
                _BLoaded = this.UnzipIt(_BLoaded);
            }

            return (_BLoaded, tmpLastSaveCode2);
        }

        /// <summary>
        /// Wartet bis der Reload abgeschlossen ist.
        /// Ist de LoadThread der aktuelle Thread, wir nicht gewartet!
        /// </summary>
        /// <param name="hardmode"></param>
        private void WaitLoaded(bool hardmode)
        {
            if (this._loadingThreadId == Thread.CurrentThread.ManagedThreadId) { return; }

            var x = DateTime.Now;

            while (this.IsLoading)
            {
                Develop.DoEvents();

                if (!hardmode && !this.IsParsing)
                {
                    return;
                }

                if (DateTime.Now.Subtract(x).TotalMinutes > 1)
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "WaitLoaded hängt: " + this.Filename);
                    return;
                }
            }
        }

        // private void WaitSaved()
        // {
        //    var x = DateTime.Now;

        // while (_IsSaving)
        //    {
        //        Develop.DoEvents();
        //        if (DateTime.Now.Subtract(x).TotalMinutes > 2)
        //        {
        //            Develop.DebugPrint(enFehlerArt.Fehler, "WaitSaved hängt");
        //            return;
        //        }

        // }
        // }

        /// <summary>
        /// Führt - falls nötig - einen Reload der Datei aus.
        /// Der Prozess wartet solange, bis der Reload erfolgreich war.
        /// Ein bereits eventuell bestehender Ladevorgang wird abgewartet.
        /// </summary>
        public void Load_Reload()
        {
            this.WaitLoaded(true);

            if (string.IsNullOrEmpty(this.Filename)) { return; }

            this.IsLoading = true;
            this._loadingThreadId = Thread.CurrentThread.ManagedThreadId;

            // var strace = new System.Diagnostics.StackTrace(true);
            // _loadingInfo = DateTime.Now.ToString(Constants.Format_Date) + " " + _BlockReload.ToString() + " #U " + Thread.CurrentThread.ManagedThreadId + " " + strace.GetFrame(1).GetMethod().ReflectedType.FullName + "/" + strace.GetFrame(1).GetMethod().ToString();

            // Wichtig, das _LastSaveCode geprüft wird, das ReloadNeeded im EasyMode immer false zurück gibt.
            if (!string.IsNullOrEmpty(this._LastSaveCode) && !this.ReloadNeeded) { this.IsLoading = false; return; }

            var OnlyReload = !string.IsNullOrEmpty(this._LastSaveCode);

            if (OnlyReload && !this.ReloadNeeded) { this.IsLoading = false; return; } // Wird in der Schleife auch geprüft

            var ec = new LoadingEventArgs(OnlyReload);
            this.OnLoading(ec);

            if (OnlyReload && this.ReadOnly && ec.TryCancel) { this.IsLoading = false; return; }

            (var _BLoaded, var tmpLastSaveCode) = this.LoadBytesFromDisk(OnlyReload, enErrorReason.Load);
            if (_BLoaded == null) { this.IsLoading = false; return; }

            this._dataOnDisk = _BLoaded;

            this.PrepeareDataForCheckingBeforeLoad();
            this.ParseInternal(_BLoaded);
            this._LastSaveCode = tmpLastSaveCode; // initialize setzt zurück
            this._CheckedAndReloadNeed = false;

            this.CheckDataAfterReload();

            this.OnLoaded(new LoadedEventArgs(OnlyReload));
            this.RepairOldBlockFiles();

            this.IsLoading = false;
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

        protected void LoadFromStream(Stream stream)
        {
            this.OnLoading(new LoadingEventArgs(false));

            byte[] _BLoaded = null;

            using (var r = new BinaryReader(stream))
            {
                _BLoaded = r.ReadBytes((int)stream.Length);
                r.Close();
            }

            if (_BLoaded.Length > 4 && BitConverter.ToInt32(_BLoaded, 0) == 67324752)
            {
                // Gezipte Daten-Kennung gefunden
                _BLoaded = this.UnzipIt(_BLoaded);
            }

            this.ParseInternal(_BLoaded);

            this.OnLoaded(new LoadedEventArgs(false));
            this.RepairOldBlockFiles();
        }

        internal void ParseInternal(byte[] bLoaded)
        {
            if (this.IsParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }

            this.IsParsing = true;
            this.ParseExternal(bLoaded);

            this.IsParsing = false;

            // Repair NACH ExecutePendung, vielleicht ist es schon repariert
            // Repair NACH _isParsing, da es auch abgespeichert werden soll
            this.OnParsed();

            this.RepairAfterParse();
        }

        public abstract void RepairAfterParse();
        protected abstract void ParseExternal(byte[] bLoaded);

        /// <summary>
        /// Entfernt im Regelfall die Temporäre Datei
        /// </summary>
        /// <param name="fromParallelProzess"></param>
        /// <param name="tmpFileName"></param>
        /// <param name="fileInfoBeforeSaving"></param>
        /// <param name="savedDataUncompressed"></param>
        /// <param name="savedDataUncompressedUTF8"></param>
        /// <returns></returns>
        private string SaveRoutine(bool fromParallelProzess, string tmpFileName, string fileInfoBeforeSaving, byte[] savedDataUncompressed)
        {
            if (this.ReadOnly) { return Feedback("Datei ist Readonly"); }

            if (tmpFileName == null || string.IsNullOrEmpty(tmpFileName) ||
            fileInfoBeforeSaving == null || string.IsNullOrEmpty(fileInfoBeforeSaving) ||
            savedDataUncompressed == null || savedDataUncompressed.Length == 0) { return Feedback("Keine Daten angekommen."); }

            if (!fromParallelProzess && this.PureBinSaver.IsBusy) { return Feedback("Anderer interner binärer Speichervorgang noch nicht abgeschlossen."); }

            if (fromParallelProzess && this.IsInSaveingLoop) { return Feedback("Anderer manuell ausgelöster binärer Speichervorgang noch nicht abgeschlossen."); }

            var f = this.ErrorReason(enErrorReason.Save);
            if (!string.IsNullOrEmpty(f)) { return Feedback("Fehler: " + f); }

            if (string.IsNullOrEmpty(this.Filename)) { return Feedback("Kein Dateiname angegeben"); }

            if (this.IsSaving) { return Feedback("Speichervorgang von verschiedenen Routinen aufgerufen."); }

            this.IsSaving = true;

            var erfolg = this.CreateBlockDatei();
            if (!erfolg)
            {
                this.IsSaving = false;
                return Feedback("Blockdatei konnte nicht erzeugt werden.");
            }

            // Blockdatei da, wir sind save. Andere Computer lassen die Datei ab jetzt in Ruhe!

            if (GetFileInfo(this.Filename, true) != fileInfoBeforeSaving)
            {
                this.DeleteBlockDatei(false, true);
                this.IsSaving = false;

                return Feedback("Datei wurde inzwischen verändert.");
            }

            if (!savedDataUncompressed.SequenceEqual(this.ToListOfByte()))
            {
                this.DeleteBlockDatei(false, true);
                this.IsSaving = false;
                return Feedback("Daten wurden inzwischen verändert.");
            }

            // OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(this.Backupdateiname())) { DeleteFile(this.Backupdateiname(), true); }

            // Haupt-Datei wird zum Backup umbenannt
            RenameFile(this.Filename, this.Backupdateiname(), true);

            // --- TmpFile wird zum Haupt ---
            RenameFile(tmpFileName, this.Filename, true);

            // ---- Steuerelemente Sagen, was gespeichert wurde
            this._dataOnDisk = savedDataUncompressed;

            // Und nun den Block entfernen
            CanWrite(this.Filename, 30); // sobald die Hauptdatei wieder frei ist
            this.DeleteBlockDatei(false, true);

            // Evtl. das BAK löschen
            if (this.AutoDeleteBAK && FileExists(this.Backupdateiname()))
            {
                DeleteFile(this.Backupdateiname(), false);
            }

            // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
            (var data, var fileinfo) = this.LoadBytesFromDisk(false, enErrorReason.LoadForCheckingOnly);
            if (!savedDataUncompressed.SequenceEqual(data))
            {
                // OK, es sind andere Daten auf der Festplatte?!? Seltsam, zählt als sozusagen ungespeichter und ungeladen.
                this._CheckedAndReloadNeed = true;
                this._LastSaveCode = "Fehler";
                Develop.DebugPrint(enFehlerArt.Warnung, "Speichern fehlgeschlagen!!! " + this.Filename);
                this.IsSaving = false;
                return Feedback("Speichervorgang fehlgeschlagen.");
            }
            else
            {
                this._CheckedAndReloadNeed = false;
                this._LastSaveCode = fileinfo;
            }

            this.DoWorkAfterSaving();

            this.OnSavedToDisk();
            this.IsSaving = false;
            return string.Empty;

            string Feedback(string txt)
            {
                DeleteFile(tmpFileName, false);
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + this.Filename + "<br><br>Grund:<br>" + txt);
                this.RepairOldBlockFiles();
                return txt;
            }
        }

        internal bool BlockDateiCheck()
        {
            if (this.AgeOfBlockDatei() < 0)
            {
                Develop.DebugPrint("Block-Datei Konflikt 4\r\n" + this.Filename + "\r\nSoll: " + this._inhaltBlockdatei);
                return false;
            }

            try
            {
                var Inhalt2 = File.ReadAllText(this.Blockdateiname(), System.Text.Encoding.UTF8);
                if (this._inhaltBlockdatei != Inhalt2)
                {
                    Develop.DebugPrint("Block-Datei Konflikt 3\r\n" + this.Filename + "\r\nSoll: " + this._inhaltBlockdatei + "\r\n\r\nIst: " + Inhalt2);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }

            return true;
        }

        private bool DeleteBlockDatei(bool ignorechecking, bool mustDoIt)
        {
            if (ignorechecking || this.BlockDateiCheck())
            {
                if (DeleteFile(this.Blockdateiname(), mustDoIt))
                {
                    this._inhaltBlockdatei = string.Empty;
                    return true;
                }
            }

            if (mustDoIt)
            {
                Develop.DebugPrint("Block-Datei nicht gelöscht\r\n" + this.Filename + "\r\nSoll: " + this._inhaltBlockdatei);
            }

            return false;
        }

        private bool CreateBlockDatei()
        {
            var tmpInhalt = UserName() + "\r\n" + DateTime.UtcNow.ToString(Constants.Format_Date5) + "\r\nThread: " + Thread.CurrentThread.ManagedThreadId + "\r\n" + Environment.MachineName;

            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try
            {
                var bInhalt = tmpInhalt.UTF8_ToByte();
                // Nicht modAllgemein, wegen den strengen Datei-Rechten
                using (var x = new FileStream(this.Blockdateiname(), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    x.Write(bInhalt, 0, bInhalt.Length);
                    x.Flush();
                    x.Close();
                }

                this._inhaltBlockdatei = tmpInhalt;
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }

            // if (!done)
            // {
            //    // Letztens aufgetreten, dass eine Blockdatei schon vorhanden war. Anscheinden Zeitgleiche Kopie?
            //    Develop.DebugPrint(enFehlerArt.Info, "Befehl anscheinend abgebrochen:\r\n" + Filename);
            //    return false;
            // }

            if (this.AgeOfBlockDatei() < 0)
            {
                Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + this.Filename);
                return false;
            }

            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);

            return this.BlockDateiCheck();
        }

        public bool ReloadNeeded
        {
            get
            {
                if (string.IsNullOrEmpty(this.Filename)) { return false; }

                if (this._CheckedAndReloadNeed) { return true; }

                if (GetFileInfo(this.Filename, false) != this._LastSaveCode)
                {
                    this._CheckedAndReloadNeed = true;
                    return true;
                }

                return false;
            }
        }

        protected abstract byte[] ToListOfByte();

        /// <summary>
        ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
        /// </summary>
        protected abstract void DoWorkAfterSaving();

        protected abstract bool isSomethingDiscOperatingsBlocking();

        protected void Load(string fileNameToLoad, bool createWhenNotExisting)
        {
            if (string.Equals(fileNameToLoad, this.Filename, StringComparison.OrdinalIgnoreCase)) { return; }

            if (!string.IsNullOrEmpty(this.Filename)) { Develop.DebugPrint(enFehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }

            if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);

            if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { this.SetReadOnly(); }

            if (!this.IsFileAllowedToLoad(fileNameToLoad)) { return; }

            if (!FileExists(fileNameToLoad))
            {
                if (createWhenNotExisting)
                {
                    if (this.ReadOnly)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                        return;
                    }

                    this.SaveAsAndChangeTo(fileNameToLoad);
                }
                else
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                    this.SetReadOnly();
                    return;
                }
            }

            this.Filename = fileNameToLoad;
            this.DoWatcher();

            // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
            this.Load_Reload();
            // var count = 0;
            // do {

            // if (count > 0) { Pause(1, false); }

            // count++;
            //    if (count > 10) { Develop.DebugPrint(enFehlerArt.Fehler, "Datei nicht korrekt geladen (nicht mehr aktuell)"); }
            // } while (ReloadNeeded());
        }

        private void DoWatcher()
        {
            if (this.Watcher != null)
            {
                this.Watcher.EnableRaisingEvents = false;
                this.Watcher.Changed -= this.Watcher_Changed;
                this.Watcher.Created -= this.Watcher_Created;
                this.Watcher.Deleted -= this.Watcher_Deleted;
                this.Watcher.Renamed -= this.Watcher_Renamed;
                this.Watcher.Error -= this.Watcher_Error;
                this.Watcher.Dispose();
                this.Watcher = null;
            }

            if (!string.IsNullOrEmpty(this.Filename))
            {
                this.Watcher = new FileSystemWatcher(this.Filename.FilePath());
                this.Watcher.Changed += this.Watcher_Changed;
                this.Watcher.Created += this.Watcher_Created;
                this.Watcher.Deleted += this.Watcher_Deleted;
                this.Watcher.Renamed += this.Watcher_Renamed;
                this.Watcher.Error += this.Watcher_Error;
                this.Watcher.EnableRaisingEvents = true;
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            // Develop.DebugPrint(enFehlerArt.Warnung, e.ToString());
            // Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
            this._CheckedAndReloadNeed = true;
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (!string.Equals(e.FullPath, this.Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            this._CheckedAndReloadNeed = true;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!string.Equals(e.FullPath, this.Filename, StringComparison.OrdinalIgnoreCase)) { return; }

            this._CheckedAndReloadNeed = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!string.Equals(e.FullPath, this.Filename, StringComparison.OrdinalIgnoreCase)) { return; }

            this._CheckedAndReloadNeed = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!string.Equals(e.FullPath, this.Filename, StringComparison.OrdinalIgnoreCase)) { return; }
            this._CheckedAndReloadNeed = true;
        }

        public void SaveAsAndChangeTo(string newFileName)
        {
            // Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (string.Equals(newFileName, this.Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            this.Save(true); // Original-Datei speichern, die ist ja dann weg.

            this.Filename = newFileName;
            this.DoWatcher();
            var l = this.ToListOfByte();

            using (var x = new FileStream(newFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }

            this._dataOnDisk = l;

            this._LastSaveCode = GetFileInfo(this.Filename, true);
            this._CheckedAndReloadNeed = false;
        }

        private void OnLoading(LoadingEventArgs e)
        {
            this.Loading?.Invoke(this, e);
        }

        protected virtual void OnLoaded(LoadedEventArgs e)
        {
            this.Loaded?.Invoke(this, e);
        }

        private void OnSavedToDisk()
        {
            this.SavedToDisk?.Invoke(this, System.EventArgs.Empty);
        }

        public void RepairOldBlockFiles()
        {
            if (DateTime.UtcNow.Subtract(this._LastMessageUTC).TotalMinutes < 1) { return; }
            this._LastMessageUTC = DateTime.UtcNow;

            var sec = this.AgeOfBlockDatei();

            try
            {
                // Nach 15 Minuten versuchen die Datei zu reparieren
                if (sec >= 900)
                {
                    if (!FileExists(this.Filename)) { return; }

                    var x = File.ReadAllText(this.Blockdateiname(), System.Text.Encoding.UTF8);
                    Develop.DebugPrint(enFehlerArt.Info, "Repariere MultiUserFile: " + this.Filename + " \r\n" + x);

                    if (!this.CreateBlockDatei()) { return; }

                    var AutoRepairName = TempFile(this.Filename.FilePath(), this.Filename.FileNameWithoutSuffix() + "_BeforeAutoRepair", "AUT");
                    if (!CopyFile(this.Filename, AutoRepairName, false))
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 1: " + this.Filename);
                        return;
                    }

                    if (!this.DeleteBlockDatei(true, false))
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 2: " + this.Filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }
        }

        private void OnParsed()
        {
            this.Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
        /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
        /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
        /// </summary>
        /// <param name="mustSave"></param>
        public bool Save(bool mustSave)
        {
            if (this.ReadOnly) { return false; }

            if (this.IsInSaveingLoop) { return false; }

            if (this.isSomethingDiscOperatingsBlocking())
            {
                if (!mustSave) { this.RepairOldBlockFiles(); return false; }
                Develop.DebugPrint(enFehlerArt.Warnung, "Release unmöglich, Dateistatus geblockt");
                return false;
            }

            if (string.IsNullOrEmpty(this.Filename)) { return false; }

            this.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken

            var D = DateTime.UtcNow; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.

            if (!mustSave && this.AgeOfBlockDatei() >= 0) { this.RepairOldBlockFiles(); return false; }

            while (this.HasPendingChanges())
            {
                this.IsInSaveingLoop = true;
                this.CancelBackGroundWorker();

                this.Load_Reload();
                (var TMPFileName, var FileInfoBeforeSaving, var DataUncompressed) = this.WriteTempFileToDisk(false); // Dateiname, Stand der Originaldatei, was gespeichert wurde
                var f = this.SaveRoutine(false, TMPFileName, FileInfoBeforeSaving, DataUncompressed);

                if (!string.IsNullOrEmpty(f))
                {
                    if (DateTime.UtcNow.Subtract(D).TotalSeconds > 40)
                    {
                        // Da liegt ein größerer Fehler vor...
                        if (mustSave) { Develop.DebugPrint(enFehlerArt.Warnung, "Datei nicht gespeichert: " + this.Filename + " " + f); }
                        this.RepairOldBlockFiles();
                        this.IsInSaveingLoop = false;
                        return false;
                    }

                    if (!mustSave && DateTime.UtcNow.Subtract(D).TotalSeconds > 5 && this.AgeOfBlockDatei() < 0)
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Optionales Speichern nach 5 Sekunden abgebrochen bei " + this.Filename + " " + f);
                        this.IsInSaveingLoop = false;
                        return false;
                    }
                }
            }

            this.IsInSaveingLoop = false;
            return true;
        }

        private byte[] UnzipIt(byte[] data)
        {
            using var originalFileStream = new MemoryStream(data);
            using var zipArchive = new ZipArchive(originalFileStream);
            var entry = zipArchive.GetEntry("Main.bin");

            using var stream = entry.Open();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);

            return ms.ToArray();
        }

        private byte[] ZipIt(byte[] data)
        {
            // https://stackoverflow.com/questions/17217077/create-zip-file-from-byte

            using var compressedFileStream = new MemoryStream();
            // Create an archive and store the stream in memory.
            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
            {
                // Create a zip entry for each attachment
                var zipEntry = zipArchive.CreateEntry("Main.bin");

                // Get the stream of the attachment
                using var originalFileStream = new MemoryStream(data);
                using var zipEntryStream = zipEntry.Open();
                // Copy the attachment stream to the zip entry stream
                originalFileStream.CopyTo(zipEntryStream);
            }

            return compressedFileStream.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="iAmThePureBinSaver"></param>
        /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
        private (string TMPFileName, string FileInfoBeforeSaving, byte[] DataUncompressed) WriteTempFileToDisk(bool iAmThePureBinSaver)
        {
            string FileInfoBeforeSaving;
            string TMPFileName;
            byte[] DataUncompressed;

            byte[] Writer_BinaryData;

            var count = 0;

            if (!iAmThePureBinSaver && this.PureBinSaver.IsBusy) { return (string.Empty, string.Empty, null); }

            if (this._DoingTempFile)
            {
                if (!iAmThePureBinSaver) { Develop.DebugPrint("Ersteller schon temp File"); }
                return (string.Empty, string.Empty, null);
            }

            this._DoingTempFile = true;

            while (true)
            {
                if (!iAmThePureBinSaver)
                {
                    // Also, im NICHT-parallelen Prozess ist explizit der Save angestoßen worden.
                    // Somit sollte des Prgramm auf Warteschleife sein und keine Benutzereingabe mehr kommen.
                    // Problem: Wenn die ganze Save-Routine in einem Parallelen-Thread ist
                    this.UserEditedAktionUTC = new DateTime(1900, 1, 1);
                }

                var f = this.ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { this._DoingTempFile = false; return (string.Empty, string.Empty, null); }

                FileInfoBeforeSaving = GetFileInfo(this.Filename, true);

                DataUncompressed = this.ToListOfByte();
                if (this._zipped)
                {
                    Writer_BinaryData = this.ZipIt(DataUncompressed);
                }
                else
                {
                    Writer_BinaryData = DataUncompressed;
                }

                TMPFileName = TempFile(this.Filename.FilePath() + this.Filename.FileNameWithoutSuffix() + ".tmp-" + modAllgemein.UserName().ToUpper());

                try
                {
                    using var x = new FileStream(TMPFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    x.Write(Writer_BinaryData, 0, Writer_BinaryData.Length);
                    x.Flush();
                    x.Close();

                    break;
                }
                catch (Exception ex)
                {
                    // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen

                    count++;
                    if (count > 15)
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + this.Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        this._DoingTempFile = false;
                        return (string.Empty, string.Empty, null);
                    }

                    Pause(1, true);
                }
            }

            this._DoingTempFile = false;
            return (TMPFileName, FileInfoBeforeSaving, DataUncompressed);
        }

        public abstract bool HasPendingChanges();

        public void UnlockHard()
        {
            try
            {
                this.Load_Reload();
                if (this.AgeOfBlockDatei() >= 0) { this.DeleteBlockDatei(true, true); }
                this.Save(true);
            }
            catch
            {
            }
        }

        private string Backupdateiname()
        {
            if (string.IsNullOrEmpty(this.Filename)) { return string.Empty; }
            return this.Filename.FilePath() + this.Filename.FileNameWithoutSuffix() + ".bak";
        }

        private string Blockdateiname()
        {
            if (string.IsNullOrEmpty(this.Filename)) { return string.Empty; }
            return this.Filename.FilePath() + this.Filename.FileNameWithoutSuffix() + ".blk";
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
        public double AgeOfBlockDatei()
        {
            if (!FileExists(this.Blockdateiname())) { return -1; }

            var f = new FileInfo(this.Blockdateiname());

            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;

            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }

        public void OnConnectedControlsStopAllWorking(MultiUserFileStopWorkingEventArgs e)
        {
            if (e.AllreadyStopped.Contains(this.Filename.ToLower())) { return; }
            e.AllreadyStopped.Add(this.Filename.ToLower());
            this.ConnectedControlsStopAllWorking?.Invoke(this, e);
        }

        /// <summary>
        /// Darf nur von einem Background-Thread aufgerufen werden.
        /// Nach einer Minute wird der trotzdem diese Routine verlassen, vermutlich liegt dann ein Fehler vor,
        /// da der Parse unabhängig vom Netzwerk ist
        /// </summary>
        public void WaitParsed()
        {
            if (!Thread.CurrentThread.IsBackground)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Darf nur von einem BackgroundThread aufgerufen werden!");
            }

            var x = DateTime.Now;

            while (this.IsParsing)
            {
                Develop.DoEvents();
                if (DateTime.Now.Subtract(x).TotalSeconds > 60) { return; }
            }
        }

        private int Checker_Tick_count = -5;

        private void Checker_Tick(object state)
        {
            if (DateTime.UtcNow.Subtract(this._BlockReload).TotalSeconds < 5) { return; }

            if (this.IsLoading) { return; }
            if (this.PureBinSaver.IsBusy || this.IsSaving) { return; }
            if (string.IsNullOrEmpty(this.Filename)) { return; }

            this.Checker_Tick_count++;
            if (this.Checker_Tick_count < 0) { return; }

            // Ausstehende Arbeiten ermittelen
            var _editable = string.IsNullOrEmpty(this.ErrorReason(enErrorReason.EditNormaly));
            var _MustReload = this.ReloadNeeded;
            var _MustSave = _editable && this.HasPendingChanges();
            var _MustBackup = _editable && this.IsThereBackgroundWorkToDo();

            if (!_MustReload && !_MustSave && !_MustBackup)
            {
                this.RepairOldBlockFiles();
                this.Checker_Tick_count = 0;
                return;
            }

            // Zeiten berechnen
            this._ReloadDelaySecond = Math.Max(this._ReloadDelaySecond, 10);
            var Count_BackUp = Math.Min((int)(this._ReloadDelaySecond / 10.0) + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save
            var Count_Save = Count_BackUp * 2 + 1; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird
            var Count_UserWork = Count_Save / 5 + 2; // Soviele Sekunden hat die User-Bearbeitung vorrang. Verhindert, dass die Bearbeitung des Users spontan abgebrochen wird.

            if (DateTime.UtcNow.Subtract(this.UserEditedAktionUTC).TotalSeconds < Count_UserWork) { return; } // Benutzer arbeiten lassen
            if (this.Checker_Tick_count > Count_Save && _MustSave) { this.CancelBackGroundWorker(); }
            if (this.Checker_Tick_count > this._ReloadDelaySecond && _MustReload) { this.CancelBackGroundWorker(); }
            if (this.BackgroundWorker.IsBusy) { return; }

            if (_MustBackup && !_MustReload && this.Checker_Tick_count < Count_Save && this.Checker_Tick_count >= Count_BackUp && string.IsNullOrEmpty(this.ErrorReason(enErrorReason.EditAcut)))
            {
                this.StartBackgroundWorker();
                return;
            }

            if (_MustReload && _MustSave)
            {
                var f = this.ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }
                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                this.Load_Reload();
                return;
            }

            if (_MustSave && this.Checker_Tick_count > Count_Save)
            {
                var f = this.ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { return; }

                // Eigentlich sollte die folgende Abfrage überflüssig sein. Ist sie aber nicht
                if (!this.PureBinSaver.IsBusy) { this.PureBinSaver.RunWorkerAsync(); }
                this.Checker_Tick_count = 0;
                return;
            }

            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && this.Checker_Tick_count > this._ReloadDelaySecond && string.IsNullOrEmpty(this.ErrorReason(enErrorReason.Load)))
            {
                var f = this.ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }
                this.Load_Reload();
                this.Checker_Tick_count = 0;
            }
        }

        public void CancelBackGroundWorker()
        {
            if (this.BackgroundWorker.IsBusy && !this.BackgroundWorker.CancellationPending) { this.BackgroundWorker.CancelAsync(); }
        }

        private void StartBackgroundWorker()
        {
            if (!string.IsNullOrEmpty(this.ErrorReason(enErrorReason.EditNormaly))) { return; }
            if (!this.BackgroundWorker.IsBusy) { this.BackgroundWorker.RunWorkerAsync(); }
        }

        private void Backup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.BackgroundWorkerMessage(e);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.DoBackGroundWork((BackgroundWorker)sender);
        }

        protected abstract void DoBackGroundWork(BackgroundWorker listenToMyCancel);
        protected abstract void BackgroundWorkerMessage(ProgressChangedEventArgs e);
        protected abstract bool IsThereBackgroundWorkToDo();

        public virtual string ErrorReason(enErrorReason mode)
        {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }

            //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------

            if (mode == enErrorReason.Load || mode == enErrorReason.LoadForCheckingOnly)
            {
                if (string.IsNullOrEmpty(this.Filename)) { return "Kein Dateiname angegeben."; }
                var sec = this.AgeOfBlockDatei();
                if (sec >= 0 && sec < 10) { return "Ein anderer Computer speichert gerade Daten ab."; }
            }

            if (mode == enErrorReason.Load)
            {
                var x = DateTime.UtcNow.Subtract(this._BlockReload).TotalSeconds;
                if (x < 5) { return "Laden noch " + (5 - x).ToString() + " Sekunden blockiert."; }

                if (DateTime.UtcNow.Subtract(this.UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen

                if (this.PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (this.BackgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
                if (this.IsParsing) { return "Es werden aktuell Daten geparsed."; }

                if (this.isSomethingDiscOperatingsBlocking()) { return "Reload unmöglich, vererbte Klasse gab Fehler zurück"; }
                return string.Empty;
            }

            //----------Alle Edits und Save ------------------------------------------------------------------------
            //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
            //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
            if (this.ReadOnly) { return "Die Datei wurde schreibgeschützt geöffnet."; }

            if (CheckForLastError(ref this._EditNormalyNextCheckUTC, ref this._EditNormalyError)) { return this._EditNormalyError; }

            if (!string.IsNullOrEmpty(this.Filename))
            {
                if (!CanWriteInDirectory(this.Filename.FilePath()))
                {
                    this._EditNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                    return this._EditNormalyError;
                }

                if (this.AgeOfBlockDatei() > 60)
                {
                    this._EditNormalyError = "Eine Blockdatei ist anscheinend dauerhaft vorhanden. Administrator verständigen.";
                    return this._EditNormalyError;
                }
            }

            //----------EditAcut, EditGeneral ----------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditAcut) || mode.HasFlag(enErrorReason.EditGeneral))
            {
                // Wird gespeichert, werden am Ende Penings zu Undos. Diese werden evtl nicht mitgespeichert.
                if (this.PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (this.IsInSaveingLoop) { return "Aktuell werden Daten gespeichert."; }
                if (this.IsLoading) { return "Aktuell werden Daten geladen."; }
            }

            //----------EditGeneral, Save------------------------------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditGeneral) || mode.HasFlag(enErrorReason.Save))
            {
                if (this.BackgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
                if (this.ReloadNeeded) { return "Die Datei muss neu eingelesen werden."; }
            }

            //---------- Save ------------------------------------------------------------------------------------------

            if (mode.HasFlag(enErrorReason.Save))
            {
                if (true) { return "Allgemeine Speicher-Sperre."; }
                if (this.IsLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }

                if (DateTime.UtcNow.Subtract(this.UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4

                if (string.IsNullOrEmpty(this.Filename)) { return string.Empty; } // EXIT -------------------
                if (!FileExists(this.Filename)) { return string.Empty; } // EXIT -------------------

                if (CheckForLastError(ref this._CanWriteNextCheckUTC, ref this._CanWriteError) && !string.IsNullOrEmpty(this._CanWriteError))
                {
                    return this._CanWriteError;
                }

                if (this.AgeOfBlockDatei() >= 0)
                {
                    this._CanWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                    return this._CanWriteError;
                }

                try
                {
                    var f2 = new FileInfo(this.Filename);
                    if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5)
                    {
                        this._CanWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                        return this._CanWriteError;
                    }
                }
                catch
                {
                    this._CanWriteError = "Dateizugriffsfehler.";
                    return this._CanWriteError;
                }

                if (!CanWrite(this.Filename, 0.5))
                {
                    this._CanWriteError = "Windows blockiert die Datei.";
                    return this._CanWriteError;
                }
            }

            return string.Empty;

            // Gibt true zurück, wenn die letzte Prüfung noch gülig ist
            bool CheckForLastError(ref DateTime nextCheckUTC, ref string lastMessage)
            {
                if (DateTime.UtcNow.Subtract(nextCheckUTC).TotalSeconds < 0) { return true; }
                lastMessage = string.Empty;
                nextCheckUTC = DateTime.UtcNow.AddSeconds(5);
                return false;
            }
        }

        public void WaitEditable()
        {
            while (!string.IsNullOrEmpty(this.ErrorReason(enErrorReason.EditAcut)))
            {
                if (!string.IsNullOrEmpty(this.ErrorReason(enErrorReason.EditNormaly))) { return; }// Nur anzeige-Dateien sind immer Schreibgeschützt
                Pause(0.2, true);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                this.Save(false);
                while (this.PureBinSaver.IsBusy) { Pause(0.5, true); }

                // https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker

                this.PureBinSaver.Dispose();
                this.Checker.Dispose();

                this.Checker.Dispose();

                this.disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        ~clsMultiUserFile()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            this.Dispose(false);
        }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            this.Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }
        #endregion

        protected static void OnMultiUserFileCreated(clsMultiUserFile file)
        {
            var e = new MultiUserFileGiveBackEventArgs
            {
                File = file
            };
            MultiUserFileCreated?.Invoke(null, e);
        }

        protected bool IsFileAllowedToLoad(string fileName)
        {
            foreach (var ThisFile in AllFiles)
            {
                if (ThisFile != null && string.Equals(ThisFile.Filename, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    ThisFile.Save(true);
                    Develop.DebugPrint(enFehlerArt.Fehler, "Doppletes Laden von " + fileName);
                    return false;
                }
            }

            return true;
        }
    }
}
