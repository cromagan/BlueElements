using BlueBasics.Enums;
using BlueBasics.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;


namespace BlueBasics.MultiUserFile
{
    public abstract class clsMultiUserFile : System.Windows.Forms.Control
    {
        private System.ComponentModel.BackgroundWorker BinReLoader;
        private System.ComponentModel.BackgroundWorker BinSaver;
        private System.Windows.Forms.Timer Checker;
        private System.ComponentModel.IContainer components;

        private bool _CheckedAndReloadNeed;


        private string _LastSaveCode;
        public bool ReadOnly { get; private set; }
        public string Filename { get; private set; }

        protected int _ReloadDelaySecond = 10;
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BinReLoader = new System.ComponentModel.BackgroundWorker();
            this.BinSaver = new System.ComponentModel.BackgroundWorker();
            this.Checker = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // BinReLoader
            // 
            this.BinReLoader.WorkerReportsProgress = true;
            this.BinReLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BinReLoader_DoWork);
            this.BinReLoader.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BinReLoader_ProgressChanged);
            // 
            // BinSaver
            // 
            this.BinSaver.WorkerReportsProgress = true;
            this.BinSaver.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BinSaver_DoWork);
            this.BinSaver.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BinSaver_ProgressChanged);
            // 
            // Checker
            // 
            this.Checker.Interval = 1000;
            this.Checker.Tick += new System.EventHandler(this.Checker_Tick);
            this.ResumeLayout(false);

        }

        private bool _isParsing;
        private int _ParsedAndRepairedCount = 0;

        public DateTime UserEditedAktion;


        /// <summary>
        /// Variable wird einzig und allein vom BinWriter verändert.
        /// Kein Reset oder Initalize verändert den Inhalt.
        /// </summary>
        private List<byte> Writer_BinaryData;



        /// <summary>
        /// Feedback-Variable, ob der Process abgeschlossen wurde. Erhällt immer den reporteden UserState, wenn Fertig.
        /// Variable wird einzig und allein vom BinWriter verändert.
        /// Kein Reset oder Initalize verändert den Inhalt.
        /// </summary>
        private string Writer_ProcessDone = string.Empty;

        private DateTime SavebleErrorReason_WindowsOnly_lastChecked = DateTime.Now.AddSeconds(-30);

        #region  Event-Deklarationen 

        public event EventHandler<LoadedEventArgs> Loaded;
        public event EventHandler<LoadingEventArgs> Loading;
        public event EventHandler SavedToDisk;
        public event EventHandler SaveAborded;
        public event EventHandler<DatabaseStoppedEventArgs> ConnectedControlsStopAllWorking;

        /// <summary>
        /// Wird ausgegeben, sobals isparsed false ist, noch vor den automatischen reperaturen.
        /// Diese Event kann verwendet werden, um die Datenbank zu reparieren, bevor sich automatische Dialoge öffnen.
        /// </summary>
        public event EventHandler Parsed;

        #endregion



        public clsMultiUserFile(bool readOnly)
        {
            InitializeComponent();


            Filename = string.Empty;
            //Filename = filename; // KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            _CheckedAndReloadNeed = true;
            _LastSaveCode = string.Empty;
            ReadOnly = readOnly;
            UserEditedAktion = new DateTime(1900, 1, 1);

            Checker.Enabled = false;

        }

        public void SetReadOnly()
        {
            ReadOnly = true;
        }

        public void RemoveFilename()
        {
            Filename = string.Empty;
            ReadOnly = true;
        }


        /// <summary>
        /// Diese Routine ist ein Paraleller Process.
        /// Er prüft, ob Daten Reloaded werden müssen. Falls KEINE Daten da sind, werden sie geladen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BinReLoader_DoWork(object sender, DoWorkEventArgs e)
        {

            var OnlyReload = !string.IsNullOrEmpty(_LastSaveCode);


            if (OnlyReload && !ReloadNeeded()) { return; }

            var ec = new LoadingEventArgs(OnlyReload);
            OnLoading(ec);

            if (OnlyReload && ReadOnly && ec.Cancel) { return; }

            string tmpLastSaveCode;
            byte[] _tmp = null;

            var StartTime = DateTime.Now;
            do
            {
                try
                {
                    if (string.IsNullOrEmpty(Filename))
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Dateiname ist leer: " + Filename);
                        return;
                    }

                    _tmp = File.ReadAllBytes(Filename);
                    tmpLastSaveCode = GetFileInfo(true);


                    Pause(0.5, false);

                    if (new FileInfo(Filename).Length == _tmp.Length) { break; }
                }
                catch (Exception ex)
                {
                    // Home Office kann lange blokieren....
                    if (DateTime.Now.Subtract(StartTime).TotalSeconds > 300)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    }
                }
            } while (true);


            var _BLoaded = new List<byte>();
            _BLoaded.AddRange(_tmp);
            ThisIsOnDisk(_BLoaded);


            PrepeareDataForCheckingBeforeLoad();




            if (sender is BackgroundWorker w)
            {
                while (_isParsing) { Develop.DoEvents(); }
                var tmpParsCount = _ParsedAndRepairedCount;
                w.ReportProgress(1, _BLoaded);
                while (tmpParsCount == _ParsedAndRepairedCount || _isParsing) { Develop.DoEvents(); }
                w.ReportProgress(0, tmpLastSaveCode);

            }
            else
            {
                BinReLoader_ProgressChanged(null, new ProgressChangedEventArgs(1, _BLoaded));
                BinReLoader_ProgressChanged(null, new ProgressChangedEventArgs(0, tmpLastSaveCode));
            }


            CheckDataAfterReload();


            OnLoaded(new LoadedEventArgs(OnlyReload));
        }

        /// <summary>
        /// gibt die Möglichkeit, Fehler in ein Protokoll zu schreiben, wenn nach dem Reload eine Inkonsitenz aufgetreten ist.
        /// Nicht für Reperaturzwecke gedacht.
        /// </summary>
        protected abstract void CheckDataAfterReload();



        /// <summary>
        /// Gibt die Möglichkeit, vor einem Reload Daten zu speichern. Diese kann nach dem Reload mit CheckDataAfterReload zu prüfen, ob alles geklappt hat.
        /// </summary>
        protected abstract void PrepeareDataForCheckingBeforeLoad();


        protected void LoadFromStream(Stream Stream)
        {


            OnLoading(new LoadingEventArgs(false));

            var _BLoaded = new List<byte>();


            using (var r = new BinaryReader(Stream))
            {
                _BLoaded.AddRange(r.ReadBytes((int)Stream.Length));
                r.Close();
            }

            ParseInternal(_BLoaded);


            OnLoaded(new LoadedEventArgs(false));
        }


        internal void ParseInternal(List<byte> bLoaded)
        {
            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            _isParsing = true;
            ParseExternal(bLoaded);

            _isParsing = false;

            //Repair NACH ExecutePendung, vielleicht ist es schon repariert
            //Repair NACH _isParsing, da es auch abgespeichert werden soll
            OnParsed();


            RepairAfterParse();

            _ParsedAndRepairedCount++;

        }

        public abstract void RepairAfterParse();
        protected abstract void ParseExternal(List<byte> bLoaded);

        private void BinReLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    ParseInternal((List<byte>)e.UserState);
                    break;

                case 0:
                    _CheckedAndReloadNeed = false;
                    _LastSaveCode = (string)e.UserState; // initialize setzt zurück
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;

            }

        }

        private void BinaryWriter_ReportProgressAndWait(int percentProcess, string UserState)
        {

            if (UserState == Writer_ProcessDone)
            {
                BinSaver.ReportProgress(0, "ResetProcess");
                do
                {
                    Develop.DoEvents();
                } while (Writer_ProcessDone == "ResetProcess");
            }


            BinSaver.ReportProgress(percentProcess, UserState);

            do
            {
                Develop.DoEvents();
            } while (Writer_ProcessDone != UserState);


        }


        private void BinSaver_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ReadOnly) { return; }

            var f = SavebleErrorReason();

            if (!string.IsNullOrEmpty(f))
            {
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datenbank abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + f);
                OnSaveAborded();
                return;
            }

            if (string.IsNullOrEmpty(Filename)) { return; }

            var tBackup = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";
            var BlockDatei = Blockdateiname();

            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            var done = false;
            try
            {
                if (FileExists(tBackup))
                {
                    done = RenameFile(tBackup, BlockDatei, false);
                }
                else
                {
                    done = CopyFile(Filename, BlockDatei, false);
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return;
            }


            if (!done)
            {
                // Letztens aufgetreten, dass eine Blockdatei schon vorhanden war. Anscheinden Zeitgleiche Kopie?
                Develop.DebugPrint(enFehlerArt.Info, "Befehl anscheinend abgebrochen:\r\n" + Filename);
                return;
            }


            if (!BlockDateiVorhanden())
            {
                Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + Filename);
                return;
            }

            // Im Parallelen-Process, Reload-Needed ist auch ein Dateizugriff
            if (ReloadNeeded()) { BinaryWriter_ReportProgressAndWait(5, "Reload"); }

            BinaryWriter_ReportProgressAndWait(10, "GetBinData");

            //OK, nun gehts rund: Haupt-Datei wird zum Backup kopiert.
            CopyFile(Filename, tBackup, true);

            // Und hier wird nun die neue Datei als TMP erzeugt erzeugt.
            var count = 0;
            var TMP = TempFile(Filename + "-" + modAllgemein.UserName().ToUpper());

            do
            {
                try
                {
                    using (var x = new FileStream(TMP, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        x.Write(Writer_BinaryData.ToArray(), 0, Writer_BinaryData.ToArray().Length);
                        x.Flush();
                        x.Close();
                    }
                    SetFileDate();
                    break;
                }
                catch (Exception ex)
                {
                    count += 1;
                    if (count > 30)
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datenbank abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        DeleteFile(BlockDatei, true);
                        DeleteFile(TMP, true);
                        return;
                    }
                    Develop.DoEvents();
                }
            } while (true);

            // --- Haupt-Datei löschen ---
            DeleteFile(Filename, true);


            // --- TmpFile wird zum Haupt ---
            RenameFile(TMP, Filename, true);


            // ---- Steuerelemente Sagen, was gespeichert wurde
            ThisIsOnDisk(Writer_BinaryData);

            // Und nun den Block entfernen
            CanWrite(Filename, 30); // sobald die Hauptdatei wieder frei ist
            DeleteFile(BlockDatei, true);




            BinaryWriter_ReportProgressAndWait(15, "GetFileState");

            BinaryWriter_ReportProgressAndWait(20, "DoWorkInSerialSavingThread");

            //BinaryWriter_ReportProgressAndWait(20, "ChangePendingToUndo");
            //BinaryWriter_ReportProgressAndWait(25, "GetLoadedFiles");


            DoWorkInParallelBinSaverThread();

            BinaryWriter_ReportProgressAndWait(30, "EventSaved");

        }

        protected abstract void ThisIsOnDisk(List<byte> binaryData);

        protected virtual string SavebleErrorReason()
        {
            if (ReadOnly) { return "Datenbank wurde schreibgeschützt geöffnet"; }
            if (BlockDateiVorhanden()) { return "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt."; }


            if (BinReLoader.IsBusy) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }


            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            if (!FileExists(Filename)) { return string.Empty; }


            if (!CanWriteInDirectory(Filename.FilePath())) { return "Sie haben im Verzeichnis der Datenbank keine Schreibrechte."; }

            if (DateTime.Now.Subtract(SavebleErrorReason_WindowsOnly_lastChecked).TotalSeconds < 0) { return "Windows blockiert die Datenbank-Datei."; }


            try
            {
                var f = new FileInfo(Filename);
                if (DateTime.Now.Subtract(f.LastWriteTime).TotalSeconds < 2) { return "Anderer Speichervorgang noch nicht abgeschlossen."; }
            }
            catch
            {
                return "Dateizugriffsfehler.";
            }



            if (!CanWrite(Filename, 0.5))
            {
                SavebleErrorReason_WindowsOnly_lastChecked = DateTime.Now.AddSeconds(5);
                return "Windows blockiert die Datenbank-Datei.";
            }



            return string.Empty;

        }

        protected abstract void DoWorkInParallelBinSaverThread();

        public bool ReloadNeeded()
        {

            //        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);


            if (string.IsNullOrEmpty(Filename)) { return false; }

            if (_CheckedAndReloadNeed) { return true; }



            if (GetFileInfo(false) != _LastSaveCode)
            {
                _CheckedAndReloadNeed = true;
                return true;
            }


            return false;
        }

        private void BinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            switch ((string)e.UserState)
            {

                case "ResetProcess":
                    break;

                case "Reload":
                    if (BinReLoader.IsBusy) { Develop.DoEvents(); }
                    BinReLoader_DoWork(null, null);
                    break;

                case "GetBinData":
                    Writer_BinaryData = ToListOfByte(true);
                    break;

                case "GetFileState":
                    _LastSaveCode = GetFileInfo(true);
                    _CheckedAndReloadNeed = false;
                    break;

                case "DoWorkInSerialSavingThread":
                    DoWorkInSerialSavingThread();
                    break;

                case "EventSaved":
                    OnSavedToDisk();
                    break;


                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;

            }


            Writer_ProcessDone = (string)e.UserState;



        }

        protected abstract List<byte> ToListOfByte(bool willSave);


        /// <summary>
        ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
        /// </summary>
        protected abstract void DoWorkInSerialSavingThread();

        /// <summary>
        /// Führt - falls nötig - einen Reload der Datenbank aus. Der Prozess wartet solange, bis der Reload erfolgreich war.
        /// </summary>
        public void Load_Reload()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Load_Reload()));
                return;
            }

            if (!ReloadNeeded()) { return; }

            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, da gerade geparst wird"); }

            if (isSomethingDiscOperatingsBlocking()) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, vererbte Klasse gab Fehler zurück"); }



            //Der View-Code muss vom Table Selbst verwaltet werden. Jede Table/Formula kann ja eine eigene Ansicht haben!
            // Stelle sicher, dass der Prozess nicht läuft- Nur Sicherheitshalber, sollte eigentlich eh nicht sein.
            while (BinReLoader.IsBusy) { Develop.DoEvents(); }

            BinReLoader_DoWork(null, null);

            Checker.Enabled = true;
        }

        protected abstract bool isSomethingDiscOperatingsBlocking();

        public void Load(string fileName)
        {

            if (fileName.ToUpper() == Filename.ToUpper()) { return; }

            if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(enFehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }

            if (string.IsNullOrEmpty(fileName)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            fileName = modConverter.SerialNr2Path(fileName);

            if (!FileExists(fileName))
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Datenbank existiert nicht: " + fileName);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                ReadOnly = true;
                return;
            }

            if (!CanWriteInDirectory(fileName.FilePath())) { ReadOnly = true; }


            if (!IsFileAllowedToLoad(fileName)) { return; }


            Filename = fileName;


            // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
            Load_Reload();

            if (ReloadNeeded()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datei nicht korrekt geladen (nicht mehr aktuell)"); }
        }

        protected abstract bool IsFileAllowedToLoad(string fileName);

        public void SaveAsAndChangeTo(string NewFileName)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (NewFileName.ToUpper() == Filename.ToUpper()) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            Release(true, 180); // Original-Datenbank speichern, die ist ja dann weg.

            Filename = NewFileName;
            var l = ToListOfByte(true);

            using (var x = new FileStream(NewFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }

            ThisIsOnDisk(l);


            _LastSaveCode = GetFileInfo(true);
            _CheckedAndReloadNeed = false;
        }

        private void OnLoading(LoadingEventArgs e)
        {
            Loading?.Invoke(this, e);
        }
        protected virtual void OnLoaded(LoadedEventArgs e)
        {
            Loaded?.Invoke(this, e);
        }


        private string GetFileInfo(bool MustDo)
        {

            try
            {
                var f = new FileInfo(Filename);
                return f.LastWriteTime.ToString(Constants.Format_Date) + "-" + f.Length.ToString();
            }
            catch
            {
                if (!MustDo) { return string.Empty; }
                Pause(0.5, false);
                return GetFileInfo(MustDo);
            }
        }

        private void OnSavedToDisk()
        {
            SavedToDisk?.Invoke(this, System.EventArgs.Empty);
        }
        private void OnSaveAborded()
        {
            SaveAborded?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnParsed()
        {
            Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
        /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
        /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
        /// </summary>
        public bool Release(bool MUSTRelease, int MaxWaitSeconds)
        {
            if (ReadOnly) { return false; }

            if (isSomethingDiscOperatingsBlocking())
            {
                if (!MUSTRelease) { return false; }
                Develop.DebugPrint(enFehlerArt.Fehler, "Release unmöglich, Datenbankstatus eingefroren");
            }
            OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken


            if (string.IsNullOrEmpty(Filename)) { return false; }

            MaxWaitSeconds = Math.Min(MaxWaitSeconds, 40);
            var D = DateTime.Now; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.


            if (!MUSTRelease && BlockDateiVorhanden()) { return false; }

            while (HasPendingChanges())
            {

                if (!BinSaver.IsBusy) { BinSaver.RunWorkerAsync(); }
                Develop.DoEvents();
                if (DateTime.Now.Subtract(D).TotalSeconds > MaxWaitSeconds)
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Datenank nicht freigegeben...." + Filename);
                    return false;
                } // KAcke, Da liegt ein größerer Fehler vor...
                if (!MUSTRelease && DateTime.Now.Subtract(D).TotalSeconds > 20 && !BlockDateiVorhanden()) { return false; } // Wenn der Saver hängt.... kommt auch vor :-(
                if (DateTime.Now.Subtract(D).TotalSeconds > 30 && !BinSaver.IsBusy && HasPendingChanges() && BlockDateiVorhanden()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank aufgrund der Blockdatei nicht freigegeben: " + Filename); }
            }
            return true;
        }

        public abstract bool HasPendingChanges();

        public void UnlockHard()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (IsSaveAble() && !BlockDateiVorhanden()) { return; }
            Load_Reload();
            if (BlockDateiVorhanden()) { DeleteFile(Blockdateiname(), true); }
            Release(true, 180);
        }

        /// <summary>
        /// Prüft ob gerade eben Speicherzugriff auf die Datei möglich ist.
        /// </summary>
        /// <returns></returns>
        public bool IsSaveAble()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            var w = SavebleErrorReason();
            if (!string.IsNullOrEmpty(w))
            {
                //if (ShowErrorMessage) { Notification.Show("<b><u>Datenbank speichern nicht möglich:</b></u><br><br>" + w + "<br><br><i>Datei: " + _FileName.FileNameWithSuffix(), enImageCode.Kritisch); }
                return false;
            }

            return true;
        }


        private string Blockdateiname()
        {
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            return Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";
        }

        public bool BlockDateiVorhanden()
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            return FileExists(Blockdateiname());
        }

        private void SetFileDate()
        {
            var starttime = DateTime.Now;
            do
            {
                try
                {
                    File.SetLastAccessTime(Filename, DateTime.Now);
                    break;
                }
                catch
                {
                    if (DateTime.Now.Subtract(starttime).TotalSeconds > 60)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Wartezeit überschritten, ich konnte dem System nicht mitteilen, dass die Datenbank aktualisiert wurde.<br>" + Filename);
                    }
                }
            } while (true);
        }

        public bool IsParsing()
        {
            return _isParsing;
        }

        public void OnConnectedControlsStopAllWorking(DatabaseStoppedEventArgs e)
        {
            if (e.AllreadyStopped.Contains(Filename.ToLower())) { return; }
            e.AllreadyStopped.Add(Filename.ToLower());
            ConnectedControlsStopAllWorking?.Invoke(this, e);
        }

        /// <summary>
        /// Darf nur von einem Background-Thread aufgerufen werden.
        /// </summary>
        public void WaitParsed()
        {
            if (!Thread.CurrentThread.IsBackground)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Darf nur von einem BackgroundThread aufgerufen werden!");
            }

            while (_isParsing)
            {
                Develop.DoEvents();
            }
        }

        private int Checker_Tick_count = -5;

        private void Checker_Tick(object sender, System.EventArgs e)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            if (BinReLoader.IsBusy) { return; }
            if (BinSaver.IsBusy) { return; }
            if (string.IsNullOrEmpty(Filename)) { return; }


            // Ausstehende Arbeiten ermittelen
            var _MustReload = ReloadNeeded();
            var _MustSave = HasPendingChanges();
            var _MustBackup = IsThereBackgroundWorkToDo(_MustSave);


            Checker_Tick_count += 1;

            if (Checker_Tick_count < 0) { return; }

            if (!_MustReload && !_MustSave && !_MustBackup)
            {
                Checker_Tick_count = 0;
                return;
            }

            // Zeiten berechnen 
            _ReloadDelaySecond = Math.Max(_ReloadDelaySecond, 10);
            var Count_BackUp = Math.Min((int)(_ReloadDelaySecond / 10.0) + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save
            var Count_Save = Count_BackUp * 2 + 1; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird
            var Count_UserWork = Count_Save / 5 + 2; // Soviele Sekunden hat die User-Bearbeitung vorrang. Verhindert, dass die Bearbeitung des Users spontan abgebrochen wird.



            if (DateTime.Now.Subtract(UserEditedAktion).TotalSeconds < Count_UserWork) { return; } // Benutzer arbeiten lassen
            if (Checker_Tick_count > Count_Save && _MustSave) { CancelBackGroundWorker(); }
            if (Checker_Tick_count > _ReloadDelaySecond && _MustReload) { CancelBackGroundWorker(); }
            if (IsBackgroundWorkerBusy()) { return; }



            if (_MustBackup && !_MustReload && Checker_Tick_count < Count_Save && Checker_Tick_count >= Count_BackUp && IsSaveAble())
            {
                StartBackgroundWorker();
                return;
            }


            if (_MustReload && _MustSave)
            {
                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                BinReLoader.RunWorkerAsync();
                return;
            }



            if (_MustSave && Checker_Tick_count > Count_Save)
            {
                BinSaver.RunWorkerAsync();
                Checker_Tick_count = 0;
                return;
            }


            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && Checker_Tick_count > _ReloadDelaySecond)
            {
                BinReLoader.RunWorkerAsync();
                Checker_Tick_count = 0;
            }

        }

        protected abstract void StartBackgroundWorker();
        protected abstract bool IsBackgroundWorkerBusy();
        protected abstract void CancelBackGroundWorker();
        protected abstract bool IsThereBackgroundWorkToDo(bool mustSave);
    }
}
