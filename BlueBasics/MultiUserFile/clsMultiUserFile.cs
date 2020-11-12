using BlueBasics.Enums;
using BlueBasics.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;


namespace BlueBasics.MultiUserFile
{
    public abstract class clsMultiUserFile : IDisposable
    {




        #region Shareds
        public static List<clsMultiUserFile> AllFiles = new List<clsMultiUserFile>();


        public static void SaveAll(bool mustSave)
        {

            if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss


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
            filePath = modConverter.SerialNr2Path(filePath);

            foreach (var ThisFile in AllFiles)
            {
                if (ThisFile != null && ThisFile.Filename.ToLower() == filePath.ToLower()) { return ThisFile; }
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



        private readonly System.ComponentModel.BackgroundWorker PureBinSaver;
        private readonly Timer Checker;


        private readonly bool _zipped;

        private bool _CheckedAndReloadNeed;

        private DateTime _LastMessageUTC = DateTime.UtcNow.AddMinutes(-10);

        private string _LastSaveCode;

        protected string _dataOnDisk;

        public bool ReadOnly { get; private set; }

        public bool EasyMode { get; private set; }

        /// <summary>
        /// Load oder SaveAsAndChangeTo benutzen
        /// </summary>
        public string Filename { get; private set; }

        public bool AutoDeleteBAK { get; set; }

        protected int _ReloadDelaySecond = 10;

        private bool _isParsing;

        private bool _IsLoading = false;
        private bool _IsSaving = false;

        public DateTime UserEditedAktionUTC;


        private DateTime _CanWriteNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _CanWriteError = string.Empty;

        private DateTime _EditNormalyNextCheckUTC = DateTime.UtcNow.AddSeconds(-30);
        private string _EditNormalyError = string.Empty;

        #endregion


        #region  Event-Deklarationen 

        public event EventHandler<LoadedEventArgs> Loaded;
        public event EventHandler<LoadingEventArgs> Loading;
        public event EventHandler SavedToDisk;
        public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;
        public static event EventHandler<MultiUserFileGiveBackEventArgs> MultiUserFileAdded;

        /// <summary>
        /// Wird ausgegeben, sobald isParsed false ist, noch vor den automatischen Reperaturen.
        /// Dieses Event kann verwendet werden, um die Datei automatisch zu reparieren, bevor sich automatische Dialoge öffnen.
        /// </summary>
        public event EventHandler Parsed;

        #endregion



        protected clsMultiUserFile(bool readOnly, bool easyMode, bool zipped)
        {

            _zipped = zipped;


            AllFiles.Add(this);
            OnMultiUserFileAdded(this);

            PureBinSaver = new System.ComponentModel.BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            PureBinSaver.DoWork += new System.ComponentModel.DoWorkEventHandler(PureBinSaver_DoWork);
            PureBinSaver.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(PureBinSaver_ProgressChanged);

            Checker = new Timer(Checker_Tick);


            Filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegnagen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
            _CheckedAndReloadNeed = true;
            _LastSaveCode = string.Empty;
            _dataOnDisk = string.Empty;
            ReadOnly = readOnly;
            EasyMode = easyMode;
            AutoDeleteBAK = false;
            UserEditedAktionUTC = new DateTime(1900, 1, 1);

            Checker.Change(2000, 2000);
        }

        private void PureBinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) { return; }

            var Data = (Tuple<string, string, string>)e.UserState;

            SaveRoutine(true, Data.Item1, Data.Item2, Data.Item3);
        }

        private void PureBinSaver_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                var Data = WriteTempFileToDisk(true);
                PureBinSaver.ReportProgress(100, Data);
            }
            catch
            {
                // OPeration completed bereits aufgerufen
            }

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



        private Tuple<List<byte>, string> LoadBytesFromDisk(bool OnlyReload)
        {
            byte[] _tmp = null;
            var tmpLastSaveCode2 = string.Empty;

            var StartTime = DateTime.UtcNow;
            do
            {
                try
                {

                    if (OnlyReload && !ReloadNeeded()) { return null; } // PRoblem hat sich aufgelöst

                    var f = ErrorReason(enErrorReason.Load);

                    if (string.IsNullOrEmpty(f))
                    {
                        var tmpLastSaveCode1 = GetFileInfo(true);
                        _tmp = File.ReadAllBytes(Filename);
                        tmpLastSaveCode2 = GetFileInfo(true);

                        if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }

                        f = "Datei wurde während des Ladens verändert.";

                    }

                    Develop.DebugPrint(enFehlerArt.Info, f + "\r\n" + Filename);
                    Pause(0.5, false);
                }
                catch (Exception ex)
                {
                    // Home Office kann lange blokieren....
                    if (DateTime.UtcNow.Subtract(StartTime).TotalSeconds > 300)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                        return null;
                    }
                }
            } while (true);

            var _BLoaded = new List<byte>();

            if (_tmp.Length > 4 && BitConverter.ToInt32(_tmp, 0) == 67324752)
            {
                // Gezipte Daten-Kennung gefunden
                _BLoaded.AddRange(UnzipIt(_tmp));
            }
            else
            {
                _BLoaded.AddRange(_tmp);
            }

            return new Tuple<List<byte>, string>(_BLoaded, tmpLastSaveCode2);
        }



        private void WaitLoaded()
        {
            var x = DateTime.Now;


            while (_IsLoading)
            {
                Develop.DoEvents();
                if (DateTime.Now.Subtract(x).TotalMinutes > 2)
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "WaitLoaded hängt");
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
        /// Führt - falls nötig - einen Reload der Datei aus. Der Prozess wartet solange, bis der Reload erfolgreich war.
        /// </summary>
        public void Load_Reload()
        {

            WaitLoaded();

            if (string.IsNullOrEmpty(Filename)) { return; }

            _IsLoading = true;

            //Wichtig, das _LastSaveCode geprüft wird, das ReloadNeeded im EasyMode immer false zurück gibt.
            if (!string.IsNullOrEmpty(_LastSaveCode) && !ReloadNeeded()) { _IsLoading = false; return; }

            var OnlyReload = !string.IsNullOrEmpty(_LastSaveCode);

            if (OnlyReload && !ReloadNeeded()) { _IsLoading = false; return; } // Wird in der Schleife auch geprüft

            var ec = new LoadingEventArgs(OnlyReload);
            OnLoading(ec);

            if (OnlyReload && ReadOnly && ec.TryCancel) { _IsLoading = false; return; }

            var Data = LoadBytesFromDisk(OnlyReload);
            if (Data == null) { _IsLoading = false; return; }

            var tmpLastSaveCode = Data.Item2;
            var _BLoaded = Data.Item1;
            _dataOnDisk = _BLoaded.ToStringConvert();

            PrepeareDataForCheckingBeforeLoad();
            ParseInternal(_BLoaded);
            _LastSaveCode = tmpLastSaveCode; // initialize setzt zurück
            _CheckedAndReloadNeed = false;


            CheckDataAfterReload();


            OnLoaded(new LoadedEventArgs(OnlyReload));
            TryOnOldBlockFileExists();

            _IsLoading = false;
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

        /// <summary>
        /// Entfernt im regelfall die Temporäre Datei
        /// </summary>
        /// <param name="FromParallelProzess"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="StateOfOriginalFile"></param>
        /// <param name="SavedData"></param>
        /// <returns></returns>
        private string SaveRoutine(bool FromParallelProzess, string savedFileName, string StateOfOriginalFile, string SavedData)
        {

            if (ReadOnly) { return Feedback("Datei ist Readonly"); }

            if (savedFileName == null || string.IsNullOrEmpty(savedFileName) ||
                StateOfOriginalFile == null || string.IsNullOrEmpty(StateOfOriginalFile) ||
                SavedData == null || string.IsNullOrEmpty(SavedData)) { return Feedback("Keine Daten angekommen."); }

            if (!FromParallelProzess && PureBinSaver.IsBusy) { return Feedback("Anderer interner binärer Speichervorgang noch nicht abgeschlossen."); }

            var f = ErrorReason(enErrorReason.Save);
            if (!string.IsNullOrEmpty(f)) { return Feedback("Fehler: " + f); }

            if (string.IsNullOrEmpty(Filename)) { return Feedback("Kein Dateiname angegeben"); }


            if (_IsSaving) { return Feedback("Speichervorgang von verschiedenen Routinen aufgerufen."); }

            _IsSaving = true;

            if (!EasyMode)
            {
                var erfolg = CreateBlockDatei();
                if (!erfolg)
                {
                    _IsSaving = false;
                    return Feedback("Blockdatei konnte nicht erzeugt werden.");
                }


                // Blockdatei da, wir sind save. Andere Computer lassen die Datei ab jetzt in Ruhe!

                if (GetFileInfo(true) != StateOfOriginalFile)
                {
                    DeleteFile(Blockdateiname(), true);
                    _IsSaving = false;
                    return Feedback("Datei wurde inzwischen verändert.");
                }


                if (SavedData != ToString())
                {
                    DeleteFile(Blockdateiname(), true);
                    _IsSaving = false;
                    return Feedback("Daten wurden inzwischen verändert.");
                }
            }

            //OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(Backupdateiname())) { DeleteFile(Backupdateiname(), true); }
    
            //Haupt-Datei wird zum Backup umbenannt
            RenameFile(Filename, Backupdateiname(), true);

            // --- TmpFile wird zum Haupt ---
            RenameFile(savedFileName, Filename, true);

            // ---- Steuerelemente Sagen, was gespeichert wurde
            _dataOnDisk = SavedData;

            if (!EasyMode)
            {
                // Und nun den Block entfernen
                CanWrite(Filename, 30); // sobald die Hauptdatei wieder frei ist
                DeleteFile(Blockdateiname(), true);
            }


            // Evtl. das BAK löschen
            if (AutoDeleteBAK && FileExists(Backupdateiname()))
            {
                DeleteFile(Backupdateiname(), false);
            }


            if (!EasyMode)
            {
                // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
                var Data = LoadBytesFromDisk(false);
                if (SavedData != Data.Item1.ToArray().ToStringConvert())
                {
                    // OK, es sind andere Daten auf der Festplatte?!? Seltsam, zählt als sozusagen ungespeichter und ungeladen.
                    _CheckedAndReloadNeed = true;
                    _LastSaveCode = "Fehler";
                    Develop.DebugPrint(enFehlerArt.Warnung, "Speichern fehlgeschlagen!!!" + Filename);
                    _IsSaving = false;
                    return Feedback("Speichervorgang fehlgeschlagen.");
                }
                else
                {
                    _CheckedAndReloadNeed = false;
                    _LastSaveCode = Data.Item2;
                }
            }
            else
            {
                _CheckedAndReloadNeed = false;
                _LastSaveCode = GetFileInfo(true);
            }

            DoWorkAfterSaving();

            OnSavedToDisk();
            _IsSaving = false;
            return string.Empty;


            string Feedback(string txt)
            {
                DeleteFile(savedFileName, false);
                Develop.DebugPrint(enFehlerArt.Info, "Speichern der Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + txt);
                TryOnOldBlockFileExists();
                return txt;
            }


        }

        private bool CreateBlockDatei()
        {


            var Inhalt = (UserName() + "\r\n" + DateTime.UtcNow.ToString(Constants.Format_Date5));

            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            var done = false;
            try
            {
                var bInhalt = Inhalt.ToByte();
                //Nicht modAllgemein, wegen den strengen Datei-Rechten 
                using (var x = new FileStream(Blockdateiname(), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    x.Write(bInhalt, 0, bInhalt.Length);
                    x.Flush();
                    x.Close();
                }
                done = true;

            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }


            if (!done)
            {
                // Letztens aufgetreten, dass eine Blockdatei schon vorhanden war. Anscheinden Zeitgleiche Kopie?
                Develop.DebugPrint(enFehlerArt.Info, "Befehl anscheinend abgebrochen:\r\n" + Filename);
                return false;
            }



            if (AgeOfBlockDatei() < 0)
            {
                Develop.DebugPrint("Block-Datei Konflikt 1\r\n" + Filename);
                return false;
            }



            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);
            try
            {
                var Inhalt2 = modAllgemein.LoadFromDisk(Blockdateiname());
                if (Inhalt != Inhalt2)
                {
                    Develop.DebugPrint("Block-Datei Konflikt 3\r\n" + Filename + "\r\nSoll: " + Inhalt  + "\r\n\r\nIst: " + Inhalt2);
                }

            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return false;
            }
            return true;

        }


        /// <summary>
        /// EasyMode gibt immer false zurück
        /// </summary>
        /// <returns></returns>
        public bool ReloadNeeded()
        {
            if (EasyMode) { return false; }
            if (string.IsNullOrEmpty(Filename)) { return false; }

            if (_CheckedAndReloadNeed) { return true; }

            if (_IsSaving || _IsLoading) { return false; }

            if (GetFileInfo(false) != _LastSaveCode)
            {
                _CheckedAndReloadNeed = true;
                return true;
            }

            return false;
        }



        protected abstract List<byte> ToListOfByte(bool willSave);


        /// <summary>
        ///  Der Richtige Ort, um das "PendingChanges" flag auf false zu setzen.
        /// </summary>
        protected abstract void DoWorkAfterSaving();





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

            var count = 0;
            do
            {

                if (count > 0) { Pause(1, false); }
                Load_Reload();
                count++;
                if (count > 10) { Develop.DebugPrint(enFehlerArt.Fehler, "Datei nicht korrekt geladen (nicht mehr aktuell)"); }
            } while (ReloadNeeded());


        }



        public void SaveAsAndChangeTo(string NewFileName)
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (NewFileName.ToUpper() == Filename.ToUpper()) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            Save(true); // Original-Datei speichern, die ist ja dann weg.

            Filename = NewFileName;
            var l = ToListOfByte(true);

            using (var x = new FileStream(NewFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }

            _dataOnDisk = l.ToStringConvert();


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
                Develop.CheckStackForOverflow();
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
            _LastMessageUTC = DateTime.UtcNow;

            var sec = AgeOfBlockDatei();


            try
            {

                //Nach 15 Minuten versuchen die Datei zu reparieren
                if (sec >= 900)
                {
                    if (!FileExists(Filename)) { return; }

                    var x = LoadFromDisk(Blockdateiname());
                    Develop.DebugPrint(enFehlerArt.Warnung, "Repariere MultiUserFile: " + x + " " + Filename);

                    if (!CreateBlockDatei()) { return; }


                    var AutoRepairName = TempFile(Filename.FilePath(), Filename.FileNameWithoutSuffix() + "_BeforeAutoRepair", "AUT");
                    if (!CopyFile(Filename, AutoRepairName, false))
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 1: " + Filename);
                        return;
                    }
                    if (!DeleteFile(Blockdateiname(), false))
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Autoreparatur fehlgeschlagen 2: " + Filename);
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
            Parsed?.Invoke(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
        /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
        /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
        /// </summary>
        public bool Save(bool mustSave)
        {

            if (ReadOnly) { TryOnOldBlockFileExists(); return false; }

            if (isSomethingDiscOperatingsBlocking())
            {
                if (!mustSave) { TryOnOldBlockFileExists(); return false; }
                Develop.DebugPrint(enFehlerArt.Warnung, "Release unmöglich, Dateistatus geblockt");
                return false;
            }

            if (string.IsNullOrEmpty(Filename)) { return false; }

            OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken




            var D = DateTime.UtcNow; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.


            if (!mustSave && AgeOfBlockDatei() >= 0) { TryOnOldBlockFileExists(); return false; }

            while (HasPendingChanges())
            {

                Load_Reload();
                var Data = WriteTempFileToDisk(false); // Dateiname, Stand der Originaldatei, was gespeichert wurde
                var f = SaveRoutine(false, Data?.Item1, Data?.Item2, Data?.Item3);

                if (!string.IsNullOrEmpty(f))
                {
                    if (DateTime.UtcNow.Subtract(D).TotalSeconds > 40)
                    {
                        // Da liegt ein größerer Fehler vor...
                        if (mustSave) { Develop.DebugPrint(enFehlerArt.Warnung, "Datei nicht gespeichert: " + Filename + " " + f); }
                        TryOnOldBlockFileExists();
                        return false;
                    }

                    if (!mustSave && DateTime.UtcNow.Subtract(D).TotalSeconds > 5 && AgeOfBlockDatei() < 0)
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Optionales Speichern nach 5 Sekunden abgebrochen bei " + Filename + " " + f);
                        return false;
                    }
                }

            }
            return true;
        }


        private byte[] UnzipIt(byte[] data)
        {
            using (var originalFileStream = new MemoryStream(data))
            {

                using (var zipArchive = new ZipArchive(originalFileStream))
                {

                    var entry = zipArchive.GetEntry("Main.bin");

                    using (var stream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }


        private byte[] ZipIt(byte[] data)
        {
            // https://stackoverflow.com/questions/17217077/create-zip-file-from-byte

            using (var compressedFileStream = new MemoryStream())
            {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {

                    //Create a zip entry for each attachment
                    var zipEntry = zipArchive.CreateEntry("Main.bin");

                    //Get the stream of the attachment
                    using (var originalFileStream = new MemoryStream(data))
                    {
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            originalFileStream.CopyTo(zipEntryStream);
                        }
                    }

                }

                return compressedFileStream.ToArray();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Dateiname, Stand der Originaldatei, was gespeichert wurde</returns>
        private Tuple<string, string, string> WriteTempFileToDisk(bool iAmThePureBinSaver)
        {

            string FileInfoBeforeSaving;
            string TMPFileName;
            string DataUncompressed;

            byte[] Writer_BinaryData;

            var count = 0;


            if (!iAmThePureBinSaver && PureBinSaver.IsBusy) { return null; }

            do
            {
                var f = ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { return null; }

                FileInfoBeforeSaving = GetFileInfo(true);



                if (_zipped)
                {
                    var bytes = ToListOfByte(true).ToArray();
                    DataUncompressed = bytes.ToStringConvert();
                    Writer_BinaryData = ZipIt(bytes);
                }
                else
                {
                    Writer_BinaryData = ToListOfByte(true).ToArray();
                    DataUncompressed = Writer_BinaryData.ToStringConvert();
                }


                TMPFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + modAllgemein.UserName().ToUpper());

                try
                {
                    using (var x = new FileStream(TMPFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        x.Write(Writer_BinaryData, 0, Writer_BinaryData.Length);
                        x.Flush();
                        x.Close();
                    }


                    //File.SetLastWriteTimeUtc(TMP, DateTime.UtcNow);
                    //SetFileDate();
                    break;
                }
                catch (Exception ex)
                {
                    DeleteFile(TMPFileName, false);

                    count++;
                    if (count > 30)
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Speichern der TMP-Datei abgebrochen.<br>Datei: " + Filename + "<br><br><u>Grund:</u><br>" + ex.Message);
                        return null;
                    }
                    Pause(0.5, true);
                }
            } while (true);

            return new Tuple<string, string, string>(TMPFileName, FileInfoBeforeSaving, DataUncompressed);

        }

        public abstract bool HasPendingChanges();

        public void UnlockHard()
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            try
            {
                Load_Reload();
                if (AgeOfBlockDatei() >= 0) { DeleteFile(Blockdateiname(), true); }
                Save(true);
            }
            catch
            {
            }
        }
        private string Backupdateiname()
        {
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            return Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";
        }



        private string Blockdateiname()
        {
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            return Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
        public double AgeOfBlockDatei()
        {
            if (!FileExists(Blockdateiname())) { return -1; }

            var f = new FileInfo(Blockdateiname());

            var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;


            return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
        }


        public bool IsParsing()
        {
            return _isParsing;
        }

        public void OnConnectedControlsStopAllWorking(MultiUserFileStopWorkingEventArgs e)
        {
            if (e.AllreadyStopped.Contains(Filename.ToLower())) { return; }
            e.AllreadyStopped.Add(Filename.ToLower());
            ConnectedControlsStopAllWorking?.Invoke(this, e);
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

            while (_isParsing)
            {
                Develop.DoEvents();
                if (DateTime.Now.Subtract(x).TotalSeconds > 60) { return; }
            }
        }

        private int Checker_Tick_count = -5;

        private void Checker_Tick(object state)
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            if (EasyMode && ReadOnly) { return; }

            if (_IsLoading) { return; }
            if (PureBinSaver.IsBusy || _IsSaving) { return; }
            if (string.IsNullOrEmpty(Filename)) { return; }


            // Ausstehende Arbeiten ermittelen
            var _MustReload = ReloadNeeded();
            var _MustSave = !ReadOnly && HasPendingChanges();
            var _MustBackup = !ReadOnly && IsThereBackgroundWorkToDo(_MustSave);


            Checker_Tick_count++;

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
                var f = ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }

                // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
                Load_Reload();
                return;
            }



            if (_MustSave && Checker_Tick_count > Count_Save)
            {
                var f = ErrorReason(enErrorReason.Save);
                if (!string.IsNullOrEmpty(f)) { return; }


                // Eigentlich sollte die folgende Abfrage überflüssig sein. Ist sie aber nicht
                if (!PureBinSaver.IsBusy) { PureBinSaver.RunWorkerAsync(); }
                Checker_Tick_count = 0;
                return;
            }


            // Überhaupt nix besonderes. Ab und zu mal Reloaden
            if (_MustReload && Checker_Tick_count > _ReloadDelaySecond && string.IsNullOrEmpty(ErrorReason(enErrorReason.Load)))
            {
                var f = ErrorReason(enErrorReason.Load);
                if (!string.IsNullOrEmpty(f)) { return; }
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

            //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
            if (mode == enErrorReason.Load)
            {
                if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }

                if (DateTime.UtcNow.Subtract(UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen 

                if (PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (IsBackgroundWorkerBusy()) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
                if (_isParsing) { return "Es werden aktuell Daten geparsed."; }

                var sec = -1d;
                if (!EasyMode) { sec = AgeOfBlockDatei(); }
                if (sec >= 0 && sec < 10) { return "Ein anderer Computer speichert gerade Daten ab."; }
                if (isSomethingDiscOperatingsBlocking()) { return "Reload unmöglich, vererbte Klasse gab Fehler zurück"; }
                return string.Empty;
            }


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

                var sec = AgeOfBlockDatei();

                if (sec > 60)
                {
                    _EditNormalyError = "Eine Blockdatei ist anscheinend dauerhaft vorhanden. Administrator verständigen.";
                    return _EditNormalyError;
                }
            }

            //----------EditAcut, EditGeneral ---------------------------------------------------------------------- 
            if (mode.HasFlag(enErrorReason.EditAcut) || mode.HasFlag(enErrorReason.EditGeneral))
            {
                // Wird gespeichert, werden am Ende Penings zu Undos. Diese werden evtl nicht mitgespeichert.
                if (PureBinSaver.IsBusy) { return "Aktuell werden im Hintergrund Daten gespeichert."; }
                if (_IsLoading) { return "Aktuell werden Daten geladen."; }
            }

            //----------EditGeneral------------------------------------------------------------------------------------------
            if (mode.HasFlag(enErrorReason.EditGeneral))
            {
                if (IsBackgroundWorkerBusy()) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
                if (ReloadNeeded()) { return "Die Datei muss neu eingelesen werden."; }
            }


            //---------- Save ------------------------------------------------------------------------------------------

            if (mode.HasFlag(enErrorReason.Save))
            {
                if (_IsLoading) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }

                if (DateTime.UtcNow.Subtract(UserEditedAktionUTC).TotalSeconds < 6) { return "Aktuell werden Daten berabeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4


                if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
                if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------

                if (CheckForLastError(ref _CanWriteNextCheckUTC, ref _CanWriteError))
                {
                    if (!string.IsNullOrEmpty(_CanWriteError)) { return _CanWriteError; }
                }


                if (AgeOfBlockDatei() >= 0)
                {
                    _CanWriteError = "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt.";
                    return _CanWriteError;
                }


                try
                {
                    var f2 = new FileInfo(Filename);
                    if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5)
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
                    _CanWriteError = "Windows blockiert die Datei.";
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
                if (!string.IsNullOrEmpty(ErrorReason(enErrorReason.EditNormaly))) { return; }// Nur anzeige-Dateien sind immer Schreibgeschützt
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

        public new string ToString()
        {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            return ToListOfByte(false).ToArray().ToStringConvert();
        }


        protected static void OnMultiUserFileAdded(clsMultiUserFile file)
        {
            var e = new MultiUserFileGiveBackEventArgs
            {
                File = file
            };
            MultiUserFileAdded?.Invoke(null, e);
        }


        protected bool IsFileAllowedToLoad(string fileName)
        {
            foreach (var ThisFile in AllFiles)
            {
                if (ThisFile != null && ThisFile.Filename.ToLower() == fileName.ToLower())
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
