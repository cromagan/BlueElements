﻿using BlueBasics.Enums;
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
    public abstract class clsMultiUserFile : IDisposable
    {

        private System.ComponentModel.BackgroundWorker BinSaver;
        private Timer Checker;


        private bool _CheckedAndReloadNeed;


        private DateTime _LastMessageUTC = DateTime.UtcNow.AddMinutes(-10);

        private string _LastSaveCode;
        public bool ReadOnly { get; private set; }

        public bool EasyMode { get; private set; }

        /// <summary>
        /// Load benutzen
        /// </summary>
        public string Filename { get; private set; }

        public bool AutoDeleteBAK { get; set; }

        protected int _ReloadDelaySecond = 10;

        private bool _isParsing;

        private bool _IsLoading = false;

        public DateTime UserEditedAktionUTC;


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

        private DateTime _CanWriteNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _CanWriteError = string.Empty;

        private DateTime _EditNormalyNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _EditNormalyError = string.Empty;

        #region  Event-Deklarationen 

        public event EventHandler<LoadedEventArgs> Loaded;
        public event EventHandler<LoadingEventArgs> Loading;
        public event EventHandler SavedToDisk;
        public event EventHandler<OldBlockFileEventArgs> OldBlockFileExists;
        public event EventHandler<DatabaseStoppedEventArgs> ConnectedControlsStopAllWorking;

        /// <summary>
        /// Wird ausgegeben, sobald isParsed false ist, noch vor den automatischen Reperaturen.
        /// Diese Event kann verwendet werden, um die Datei automatisch zu reparieren, bevor sich automatische Dialoge öffnen.
        /// </summary>
        public event EventHandler Parsed;

        #endregion



        public clsMultiUserFile(bool readOnly, bool easyMode)
        {

            BinSaver = new System.ComponentModel.BackgroundWorker();
            BinSaver.WorkerReportsProgress = true;
            BinSaver.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BinSaver_DoWork);
            BinSaver.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BinSaver_ProgressChanged);

            Checker = new Timer(Checker_Tick);


            Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            _CheckedAndReloadNeed = true;
            _LastSaveCode = string.Empty;
            ReadOnly = readOnly;
            EasyMode = easyMode;
            AutoDeleteBAK = false;
            UserEditedAktionUTC = new DateTime(1900, 1, 1);

            Checker.Change(1000, 1000);
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
        /// Führt - falls nötig - einen Reload der Datenbank aus. Der Prozess wartet solange, bis der Reload erfolgreich war.
        /// </summary>
        public bool Load_Reload()
        {
            while (_IsLoading) { Develop.DoEvents(); }

            _IsLoading = true;

            if (string.IsNullOrEmpty(Filename)) { _IsLoading = false; return false; }

            //Wichtig, das _LastSaveCode geprüft wird, das ReloadNeeded im EasyMode immer false zurück gibt.
            if (!string.IsNullOrEmpty(_LastSaveCode) && !ReloadNeeded()) { _IsLoading = false; return false; }

            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, da gerade geparst wird"); }

            if (isSomethingDiscOperatingsBlocking()) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, vererbte Klasse gab Fehler zurück"); }

            var OnlyReload = !string.IsNullOrEmpty(_LastSaveCode);

            if (OnlyReload && !ReloadNeeded()) { _IsLoading = false; return false; }

            var ec = new LoadingEventArgs(OnlyReload);
            OnLoading(ec);

            if (OnlyReload && ReadOnly && ec.Cancel) { _IsLoading = false; return false; }

            string tmpLastSaveCode;
            byte[] _tmp = null;

            var StartTime = DateTime.UtcNow;
            do
            {
                try
                {
                    if (string.IsNullOrEmpty(Filename))
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname ist leer: " + Filename); // sollte vorher abgefangn sein.
                        _IsLoading = false; return false;
                    }

                    _tmp = File.ReadAllBytes(Filename);
                    tmpLastSaveCode = GetFileInfo(true);

                    if (EasyMode) { break; }

                    Pause(0.5, false);
                    if (new FileInfo(Filename).Length == _tmp.Length) { break; }

                }
                catch (Exception ex)
                {
                    // Home Office kann lange blokieren....
                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 300)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                        _IsLoading = false; return false;
                    }
                }
            } while (true);


            var _BLoaded = new List<byte>();
            _BLoaded.AddRange(_tmp);
            ThisIsOnDisk(_BLoaded);


            PrepeareDataForCheckingBeforeLoad();



            ParseInternal(_BLoaded);
            _LastSaveCode = tmpLastSaveCode; // initialize setzt zurück
            _CheckedAndReloadNeed = false;


            CheckDataAfterReload();


            OnLoaded(new LoadedEventArgs(OnlyReload));
            TryOnOldBlockFileExists();

            _IsLoading = false; return true;
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
            TryOnOldBlockFileExists();
        }


        internal void ParseInternal(List<byte> bLoaded)
        {
            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            _isParsing = true;
            ParseExternal(bLoaded);

            _isParsing = false;

            //Repair NACH ExecutePendung, vielleicht ist es schon repariert
            //Repair NACH _isParsing, da es auch abgespeichert werden soll
            OnParsed();


            RepairAfterParse();


        }

        public abstract void RepairAfterParse();
        protected abstract void ParseExternal(List<byte> bLoaded);


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
            if (EasyMode) { Develop.DebugPrint(enFehlerArt.Fehler, "Dürfte nicht passieren."); }

            var f = ErrorReason(enErrorReason.Save);

            if (!string.IsNullOrEmpty(f))
            {
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + f);
                TryOnOldBlockFileExists();
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
                    System.IO.File.SetCreationTimeUtc(tBackup, DateTime.UtcNow);
                    done = RenameFile(tBackup, BlockDatei, false);
                }
                else
                {
                    System.IO.File.SetCreationTimeUtc(Filename, DateTime.UtcNow);
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
                        Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
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

            // Evtl. das BAK löschen
            if (AutoDeleteBAK && FileExists(tBackup))
            {
                DeleteFile(tBackup, false);
            }


            BinaryWriter_ReportProgressAndWait(15, "GetFileState");

            BinaryWriter_ReportProgressAndWait(20, "DoWorkInSerialSavingThread");

            //BinaryWriter_ReportProgressAndWait(20, "ChangePendingToUndo");
            //BinaryWriter_ReportProgressAndWait(25, "GetLoadedFiles");


            DoWorkInParallelBinSaverThread();

            BinaryWriter_ReportProgressAndWait(30, "EventSaved");

        }

        protected abstract void ThisIsOnDisk(List<byte> binaryData);



        protected abstract void DoWorkInParallelBinSaverThread();

        /// <summary>
        /// EasyMode gibt immer false zurück
        /// </summary>
        /// <returns></returns>
        public bool ReloadNeeded()
        {
            if (EasyMode) { return false; }
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
                    Load_Reload();
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





        protected abstract bool isSomethingDiscOperatingsBlocking();


        public void Load(string fileNameToLoad, bool CreateWhenNotExisting)
        {

            if (fileNameToLoad.ToUpper() == Filename.ToUpper()) { return; }

            if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(enFehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }

            if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);


            if (!CanWriteInDirectory(fileNameToLoad.FilePath())) { ReadOnly = true; }


            if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }



            if (!FileExists(fileNameToLoad))
            {

                if (CreateWhenNotExisting)
                {
                    if (ReadOnly)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Readonly kann keine Datei erzeugen");
                        return;
                    }
                    SaveAsAndChangeTo(fileNameToLoad);
                }
                else
                {

                    Develop.DebugPrint(enFehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                    ReadOnly = true;
                    return;
                }
            }








            Filename = fileNameToLoad;


            // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
            Load_Reload();

            if (ReloadNeeded()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datei nicht korrekt geladen (nicht mehr aktuell)"); }
        }

        protected abstract bool IsFileAllowedToLoad(string fileName);

        public void SaveAsAndChangeTo(string NewFileName)
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (NewFileName.ToUpper() == Filename.ToUpper()) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            Release(true, 240); // Original-Datenbank speichern, die ist ja dann weg.

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
                return f.LastWriteTimeUtc.ToString(Constants.Format_Date) + "-" + f.Length.ToString();
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
        public void TryOnOldBlockFileExists()
        {

            if (DateTime.UtcNow.Subtract(_LastMessageUTC).TotalMinutes < 1) { return; }


            if (BlockDateiVorhanden())
            {
                _LastMessageUTC = DateTime.UtcNow;
                var f = new FileInfo(Blockdateiname());
                var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
                OldBlockFileExists?.Invoke(this, new OldBlockFileEventArgs(sec));
            }

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
            if (ReadOnly) { TryOnOldBlockFileExists(); return false; }

            if (isSomethingDiscOperatingsBlocking())
            {
                if (!MUSTRelease) { TryOnOldBlockFileExists(); return false; }
                Develop.DebugPrint(enFehlerArt.Fehler, "Release unmöglich, Datenbankstatus eingefroren");
            }
            OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken


            if (string.IsNullOrEmpty(Filename)) { return false; }

            MaxWaitSeconds = Math.Min(MaxWaitSeconds, 40);
            var D = DateTime.UtcNow; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.


            if (!MUSTRelease && BlockDateiVorhanden()) { TryOnOldBlockFileExists(); return false; }

            while (HasPendingChanges())
            {

                if (!BinSaver.IsBusy) { SaveProcess(); }
                Develop.DoEvents();
                if (DateTime.UtcNow.Subtract(D).TotalSeconds > MaxWaitSeconds)
                {
                    if (MUSTRelease)
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Datei nicht freigegeben...." + Filename);
                    }


                    TryOnOldBlockFileExists(); return false;
                } // KAcke, Da liegt ein größerer Fehler vor...
                if (!MUSTRelease && DateTime.UtcNow.Subtract(D).TotalSeconds > 20 && !BlockDateiVorhanden()) { TryOnOldBlockFileExists(); return false; } // Wenn der Saver hängt.... kommt auch vor :-(
                if (DateTime.UtcNow.Subtract(D).TotalSeconds > 30 && !BinSaver.IsBusy && HasPendingChanges() && BlockDateiVorhanden()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank aufgrund der Blockdatei nicht freigegeben: " + Filename); }
            }
            return true;
        }

        private void SaveProcess()
        {
            if (ReadOnly) { return; }

            if (!EasyMode)
            {
                if (!BinSaver.IsBusy) { BinSaver.RunWorkerAsync(); }
                return;
            }


            if (IsThereBackgroundWorkToDo(true))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Easymode unterstützt keine Backgroundworker");
            }





            var f = ErrorReason(enErrorReason.Save);

            if (!string.IsNullOrEmpty(f))
            {
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + f);
                TryOnOldBlockFileExists();
                return;
            }

            if (string.IsNullOrEmpty(Filename)) { return; }

            var tBackup = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";


            if (FileExists(tBackup)) { DeleteFile(tBackup, true); }


            if (FileExists(Filename))
            {
                var done = false;
                try
                {
                    done = RenameFile(Filename, tBackup, false);
                }
                catch (Exception ex)
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, ex);
                    return;
                }
                if (!done)
                {
                    Develop.DebugPrint(enFehlerArt.Info, "Befehl anscheinend abgebrochen:\r\n" + Filename);
                    return;
                }
            }



            Writer_BinaryData = ToListOfByte(true);





            // Und hier wird nun die neue Datei als TMP erzeugt erzeugt.
            var count = 0;

            do
            {
                try
                {
                    using (var x = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
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
                        Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        return;
                    }
                    Develop.DoEvents();
                }
            } while (true);



            // ---- Steuerelemente Sagen, was gespeichert wurde
            ThisIsOnDisk(Writer_BinaryData);



            // Evtl. das BAK löschen
            if (AutoDeleteBAK && FileExists(tBackup))
            {
                DeleteFile(tBackup, false);
            }

            _LastSaveCode = GetFileInfo(true);
            _CheckedAndReloadNeed = false;

            DoWorkInSerialSavingThread();

            DoWorkInParallelBinSaverThread(); // OK, gemogelt, aber sonst wirds ja gar nicht gemacht

            OnSavedToDisk();
        }

        public abstract bool HasPendingChanges();

        public void UnlockHard()
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            try
            {
                Load_Reload();
                if (BlockDateiVorhanden()) { DeleteFile(Blockdateiname(), true); }
                Release(true, 240);
            }
            catch
            {
            }
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
            var starttime = DateTime.UtcNow;
            do
            {
                try
                {
                    File.SetLastAccessTime(Filename, DateTime.UtcNow);
                    break;
                }
                catch
                {
                    if (DateTime.UtcNow.Subtract(starttime).TotalSeconds > 60)
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

        private void Checker_Tick(object state)
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            if (EasyMode && ReadOnly) { return; }

            if (_IsLoading) { return; }
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
                TryOnOldBlockFileExists();
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
            if (IsBackgroundWorkerBusy()) { return; }



            if (_MustBackup && !_MustReload && Checker_Tick_count < Count_Save && Checker_Tick_count >= Count_BackUp && string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut)))
            {
                StartBackgroundWorker();
                return;
            }


            if (_MustReload && _MustSave)
            {
                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                Load_Reload();
                return;
            }



            if (_MustSave && Checker_Tick_count > Count_Save)
            {
                SaveProcess();
                Checker_Tick_count = 0;
                return;
            }


            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && Checker_Tick_count > _ReloadDelaySecond)
            {
                Load_Reload();
                Checker_Tick_count = 0;
            }

        }

        protected abstract void StartBackgroundWorker();
        protected abstract bool IsBackgroundWorkerBusy();
        protected abstract void CancelBackGroundWorker();
        protected abstract bool IsThereBackgroundWorkToDo(bool mustSave);



        public virtual string ErrorReason(enErrorReason mode)
        {
            if (mode == enErrorReason.OnlyRead) { return string.Empty; }

            //----------Alle Edits und Save ------------------------------------------------------------------------
            //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
            //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
            if (ReadOnly) { return "Die Datei wurde schreibgeschützt geöffnet."; }

            if (CheckForLastError(ref _EditNormalyNextCheckUTC, ref _EditNormalyError)) { return _EditNormalyError; }


            if (!string.IsNullOrEmpty(Filename))
            {
                if (!CanWriteInDirectory(Filename.FilePath()))
                {
                    _EditNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                    return _EditNormalyError;
                }
                if (BlockDateiVorhanden())
                {
                    var f = new FileInfo(Blockdateiname());
                    if (DateTime.UtcNow.Subtract(f.LastWriteTime).TotalSeconds > 60)
                    {
                        _EditNormalyError = "Eine Blockdatei ist anscheinend dauerhaft vorhanden. Administrator verständigen.";
                        return _EditNormalyError;
                    }
                }
            }

            //----------EditAcut, EditGeneral ---------------------------------------------------------------------- 
            if (mode.HasFlag(enErrorReason.EditAcut) || mode.HasFlag(enErrorReason.EditGeneral))
            {
                // Wird gespeichert, werden am Ende Penings zu Undos. Diese werden evtl nicht mitgespeichert.
                if (BinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
            }

            //----------EditGeneral------------------------------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditGeneral))
            {
                if (IsBackgroundWorkerBusy()) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
                if (_IsLoading) { return "Aktuell werden im Hintergrund Daten geladen."; }
                if (ReloadNeeded()) { return "Die Datei muss neu eingelesen werden."; }
            }


            //---------- Save ------------------------------------------------------------------------------------------

            if (mode.HasFlag(enErrorReason.Save))
            {
                if (_IsLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }

                if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
                if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------

                if (CheckForLastError(ref _CanWriteNextCheckUTC, ref _CanWriteError))
                {
                    if (!string.IsNullOrEmpty(_CanWriteError)) { return _CanWriteError; }
                }


                if (BlockDateiVorhanden())
                {
                    _CanWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                    return _CanWriteError;
                }


                try
                {
                    var f2 = new FileInfo(Filename);
                    if (DateTime.UtcNow.Subtract(f2.LastWriteTime).TotalSeconds < 2)
                    {
                        _CanWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                        return _CanWriteError;
                    }
                }
                catch
                {
                    _CanWriteError = "Dateizugriffsfehler.";
                    return _CanWriteError;
                }

                if (!CanWrite(Filename, 0.5))
                {
                    _CanWriteError = "Windows blockiert die Datenbank-Datei.";
                    return _CanWriteError;
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

            while (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditAcut)))
            {
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }// Nur anzeige-Datenbanken sind immer Schreibgeschützt
                Pause(0.2, true);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                Release(false, 1);
                while (BinSaver.IsBusy) { Pause(0.1, true); }


                //  https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker

                BinSaver.Dispose();
                Checker.Dispose();

                Checker.Dispose();

                disposedValue = true;
            }
        }

        //TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        ~clsMultiUserFile()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(false);
        }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }
        #endregion


        //protected override void OnHandleDestroyed(System.EventArgs e)
        //{
        //    Release(false, 60);
        //    base.OnHandleDestroyed(e);
        //}

    }
}
