#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;


namespace BlueDatabase
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Database : System.Windows.Forms.Control
    {
        #region  Shareds 

        public static readonly string DatabaseVersion = "3.30";
        public static List<Database> AllDatabases = new List<Database>();


        /// <summary>
        /// Prüft, ob die Datenbank bereits geladen ist. 
        /// Lädt die Datenbank oder gibt die Datenbank, die breits im Speicher ist zurück.
        /// </summary>
        /// <param name="cFileName"></param>
        /// <param name="ReadOnly"></param>
        /// <param name="passwordSub"></param>
        /// <param name="GenLayout"></param>
        /// <param name="RenameColumn"></param>
        /// <returns></returns>
        public static Database Load(string cFileName, bool ReadOnly, GetPassword passwordSub, GenerateLayout_Internal GenLayout, RenameColumnInLayout RenameColumn)
        {
            var d = GetByFilename(cFileName);
            if (d != null) { return d; }
            return Load_Internal(cFileName, ReadOnly, passwordSub, GenLayout, RenameColumn);
        }

        public static Database Load(string cFileName, bool ReadOnly)
        {
            var d = GetByFilename(cFileName);
            if (d != null) { return d; }
            return Load_Internal(cFileName, ReadOnly, null, null, null);
        }


        public static List<Database> GetByCaption(string Caption)
        {
            var l = new List<Database>();
            foreach (var ThisDatabase in AllDatabases)
            {
                if (ThisDatabase != null)
                {
                    if (ThisDatabase.Caption.ToUpper() == Caption.ToUpper()) { l.Add(ThisDatabase); }
                }
            }

            return l;
        }


        private static Database Load(Stream Stream, GetPassword passwordSub, GenerateLayout_Internal GenLayout, RenameColumnInLayout RenameColumn)
        {
            if (Stream == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }



            var db = new Database(true, passwordSub, GenLayout, RenameColumn);

            //TODO: In AllDatabases Aufnehmen!!!!!

            db.LoadFromStream(Stream);
            db.OnLoaded(new LoadedEventArgs(false));
            return db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="Name"></param>
        /// <param name="BlueBasicsSubDir"></param>
        /// <param name="FehlerAusgeben"></param>
        /// <param name="MustBeStream"></param>
        /// <param name="passwordSub">Bestenfalls: Table.Database_NeedPassword</param>
        /// <returns></returns>
        public static Database LoadResource(Assembly assembly, string Name, string BlueBasicsSubDir, bool FehlerAusgeben, bool MustBeStream, GetPassword passwordSub, GenerateLayout_Internal GenLayout, RenameColumnInLayout RenameColumn)
        {

            if (Develop.IsHostRunning() && !MustBeStream)
            {

                var x = -1;
                var pf = string.Empty;

                do
                {
                    x += 1;
                    pf = string.Empty;
                    switch (x)
                    {
                        case 0:
                            // BeCreative, At Home, 10.11.2018
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 1:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 2:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 3:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 4:
                            pf = System.Windows.Forms.Application.StartupPath + "\\" + Name;
                            break;
                        case 5:
                            pf = System.Windows.Forms.Application.StartupPath + "\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 6:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio VSTS\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 7:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio VSTS\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 8:
                            // BeCreative, Firma, 09.11.2018
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio VSTS\\BlueControls\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                        case 9:
                            // Bildzeichen-Liste, Firma, 09.11.2018
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio VSTS\\BlueControls\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;

                    }

                    if (FileExists(pf)) { return Load(pf, false, passwordSub, GenLayout, RenameColumn); }

                } while (pf != string.Empty);
            }

            var d = modAllgemein.GetEmmbedResource(assembly, Name);
            if (d != null) { return Load(d, null, null, null); }

            if (FehlerAusgeben) { Develop.DebugPrint(enFehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + BlueBasicsSubDir + " - " + Name); }


            return null;
        }

        public string DoImportScript(string TextToImport, RowItem row, bool MeldeFehlgeschlageneZeilen)
        {
            if (string.IsNullOrEmpty(_ImportScript)) { return "Kein Import-Skript vorhanden."; }
            if (string.IsNullOrEmpty(TextToImport)) { return "Kein Text zum Importieren angegeben."; }


            var cmds = _ImportScript.FromNonCritical().SplitByCRToList();

            if (row == null)
            {
                row = Row.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            }



            foreach (var thiscmd in cmds)
            {
                var feh = DoImportScript(TextToImport, thiscmd.Replace(";cr;", "\r").Replace(";tab;", "\t").SplitBy("|"), row);

                if (!string.IsNullOrEmpty(feh))
                {
                    if (feh.StartsWith("!"))
                    {
                        return feh.Substring(1) + "<br><br>Zeile:<br>" + thiscmd;
                    }

                    if (MeldeFehlgeschlageneZeilen)
                    {
                        return feh + "<br><br>Zeile:<br>" + thiscmd;
                    }

                }
            }



            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToImport"></param>
        /// <param name="cmd"></param>
        /// <returns>Wenn Erfolgreich wird nichts zurückgegeben. Schwere Fehler beginnen mit einem !</returns>
        private string DoImportScript(string textToImport, string[] cmd, RowItem row)
        {
            if (row == null) { return "!Keine Zeile angegeben."; }

            if (cmd == null) { return "!Kein Befehl übergeben"; }
            if (cmd.GetUpperBound(0) != 3) { return "!Format muss 4 | haben."; }

            var c = Column[cmd[1]];
            if (c == null) { return "!Spalte nicht in der Datenbank gefunden."; }

            if (string.IsNullOrEmpty(cmd[2])) { return "!Suchtext 'vorher' ist nicht angegeben"; }
            if (string.IsNullOrEmpty(cmd[3])) { return "!Suchtext 'nachher' ist nicht angegeben"; }

            var vh = textToImport.IndexOf(cmd[2]);
            if (vh < 0) { return "Suchtext 'vorher' im Text nicht vorhanden."; }

            var nh = textToImport.IndexOf(cmd[3], vh + cmd[2].Length);
            if (nh < 0) { return "Suchtext 'nachher' im Text nicht vorhanden."; }

            var txt = textToImport.Substring(vh + cmd[2].Length, nh - vh - cmd[2].Length);

            switch (cmd[0].ToUpper())
            {
                case "IMPORT1":
                    row.CellSet(c, txt);
                    return string.Empty;

                case "IMPORT2":
                    var l = row.CellGetList(c);
                    l.Add(txt);
                    row.CellSet(c, l);
                    return string.Empty;


                default:
                    return "!Befehl nicht erkannt.";
            }

        }

        private static Database GetByFilename(string cFileName)
        {
            cFileName = modConverter.SerialNr2Path(cFileName);

            foreach (var ThisDatabase in AllDatabases)
            {
                if (ThisDatabase != null && ThisDatabase.Filename.ToLower() == cFileName.ToLower()) { return ThisDatabase; }
            }

            return null;

        }




        private static Database Load_Internal(string fileName, bool readOnly, GetPassword passwordSub, GenerateLayout_Internal GenLayout, RenameColumnInLayout RenameColumn)
        {

            if (string.IsNullOrEmpty(fileName)) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname nicht angegeben!"); }
            fileName = modConverter.SerialNr2Path(fileName);

            if (!FileExists(fileName))
            {
                if (!readOnly) { Develop.DebugPrint(enFehlerArt.Warnung, "Datenbank existiert nicht: " + fileName); } // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                return null;
            }

            if (!CanWriteInDirectory(fileName.FilePath())) { readOnly = true; }


            foreach (var ThisDatabase in AllDatabases)
            {

                if (ThisDatabase != null && ThisDatabase.Filename.ToLower() == fileName.ToLower())
                {
                    if (ThisDatabase.ReadOnly == readOnly) { return ThisDatabase; }

                    ThisDatabase.Release(true);
                }
            }

            var db = new Database(readOnly, passwordSub, GenLayout, RenameColumn);

            db.Filename = fileName;
            if (FileExists(fileName))
            {
                // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
                db.LoadFromDisk();
                db.OnLoaded(new LoadedEventArgs(false));
            }

            AllDatabases.Add(db);

            if (db.ReloadNeeded()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank nicht aktuell"); }

            return db;
        }

        public RuleItem Rules_Has(ColumnItem column, string SystemKey)
        {
            foreach (var Rule in Rules)
            {
                if (Rule.SystemKey == column.Key + "|" + SystemKey) { return Rule; }
            }
            return null;
        }

        public static void ReleaseAll(bool MUSTRelease)
        {

            var x = AllDatabases.Count;

            foreach (var ThisDatabase in AllDatabases)
            {
                ThisDatabase?.Release(MUSTRelease);

                if (x != AllDatabases.Count)
                {
                    // Die Auflistung wurde verändert! Selten, aber kann passieren!
                    ReleaseAll(MUSTRelease);
                    return;
                }
            }
        }

        #endregion


        #region  Variablen-Deklarationen 
        internal System.Windows.Forms.Timer Checker;
        private IContainer components;
        private BackgroundWorker BinReLoader;
        private BackgroundWorker BinSaver;
        private BackgroundWorker Backup;


        public readonly ColumnCollection Column;
        public readonly CellCollection Cell;
        public readonly RowCollection Row;


        private bool _CheckedAndReloadNeed;

        private DateTime _LoadedFileDate;
        private DateTime _UserEditedAktion;



        public List<WorkItem> Works;

        private readonly List<string> FilesAfterLoadingLCase;

        private string _Creator;
        private string _CreateDate;
        private int _UndoCount;

        private int _ReloadDelaySecond;

        private string _GlobalShowPass;
        private string _FileEncryptionKey;

        private string _LoadedVersion;
        private string _Caption;
        private enJoinTyp _JoinTyp;
        private enVerwaisteDaten _VerwaisteDaten;
        private string _ImportScript;
        private enAnsicht _Ansicht;
        private int _Skin;
        private double _GlobalScale;

        public readonly bool ReadOnly;
        public readonly ListExt<RuleItem> Rules = new ListExt<RuleItem>();
        public readonly ListExt<ColumnViewCollection> ColumnArrangements = new ListExt<ColumnViewCollection>();
        public readonly ListExt<ColumnViewCollection> Views = new ListExt<ColumnViewCollection>();
        public readonly ListExt<string> Tags = new ListExt<string>();

        /// <summary>
        /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
        /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
        /// </summary>
        public readonly ListExt<ExportDefinition> _Export = new ListExt<ExportDefinition>();

        public readonly ListExt<clsNamedBinary> Bins = new ListExt<clsNamedBinary>();
        public readonly ListExt<string> DatenbankAdmin = new ListExt<string>();
        public readonly ListExt<string> PermissionGroups_NewRow = new ListExt<string>();
        public readonly ListExt<string> Layouts = new ListExt<string>(); // Print Views werden nicht immer benötigt. Deswegen werden sie als String gespeichert. Der Richtige Typ wäre CreativePad

        public string UserGroup = "#Administrator";
        public readonly string UserName = modAllgemein.UserName().ToUpper();


        private RowSortDefinition _sortDefinition;
        private bool _isParsing;

        /// <summary>
        /// Variable wird einzig und allein vom BinWriter verändert.
        /// Kein Reset oder Initalize verändert den Inhalt.
        /// </summary>
        private List<byte> Writer_BinaryData;

        /// <summary>
        /// Variable wird einzig und allein vom BinWriter verändert.
        /// Kein Reset oder Initalize verändert den Inhalt.
        /// </summary>
        private List<string> Writer_FilesToDeleteLCase;

        /// <summary>
        /// Feedback-Variable, ob der Process abgeschlossen wurde. Erhällt immer den reporteden UserState, wenn Fertig.
        /// Variable wird einzig und allein vom BinWriter verändert.
        /// Kein Reset oder Initalize verändert den Inhalt.
        /// </summary>
        private string Writer_ProcessDone = string.Empty;



        /// <summary>
        /// Table würde eine Statische Routine enthalten, die dafür geeignet wäre.
        /// </summary>
        public readonly GetPassword _PasswordSub;

        /// <summary>
        /// Creative-Pad würde eine Statische Routine enthalten, die dafür geeignet wäre.
        /// </summary>
        public readonly GenerateLayout_Internal _GenerateLayout;

        /// <summary>
        /// Creative-Pad würde eine Statische Routine enthalten, die dafür geeignet wäre.
        /// </summary>
        public readonly RenameColumnInLayout _RenameColumnInLayout;





        #endregion


        #region  Event-Deklarationen 


        public event EventHandler<LoadedEventArgs> Loaded;
        public event EventHandler SortParameterChanged;
        public event EventHandler<DatabaseStoppedEventArgs> ConnectedControlsStopAllWorking;
        public event EventHandler StoreView;
        public event EventHandler RestoreView;
        public event EventHandler SavedToDisk;
        public event EventHandler SaveAborded;
        public event CancelEventHandler Exporting;
        public event EventHandler<DatabaseSettingsEventHandler> LoadingLinkedDatabase;
        public event CancelEventHandler Reloading;
        public event EventHandler<KeyChangedEventArgs> RowKeyChanged;
        public event EventHandler<KeyChangedEventArgs> ColumnKeyChanged;

        public delegate string GetPassword();
        public delegate string RenameColumnInLayout(Database database, string LayoutCode, string OldName, ColumnItem Column);
        public delegate void GenerateLayout_Internal(RowItem Row, int LayoutNr, bool DirectPrint, bool DirectSave, string OptionalFilename);

        #endregion


        #region  Construktor + Initialize 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readOnly"></param>
        /// <param name="passwordSub">Bestenfalls: Table.Database_NeedPassword</param>
        /// <param name="GenLayout">Bestenfalls: CreativePad.Generate_Layout</param>
        /// <param name="RenameColumn">Bestenfalls: CreativePad.RenameColumnInLayout</param>
        public Database(bool readOnly, GetPassword passwordSub, GenerateLayout_Internal GenLayout, RenameColumnInLayout RenameColumn)
        {
            var culture = new System.Globalization.CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            ReadOnly = readOnly;
            _PasswordSub = passwordSub;
            _GenerateLayout = GenLayout;
            _RenameColumnInLayout = RenameColumn;

            CreateControl();
            InitializeComponent();

            Cell = new CellCollection(this);
            Column = new ColumnCollection(this);
            Row = new RowCollection(this);

            Works = new List<WorkItem>();

            FilesAfterLoadingLCase = new List<string>();

            Checker_Tick_count = 0;


            ColumnArrangements.ListOrItemChanged += ColumnArrangements_ListOrItemChanged;
            Layouts.ListOrItemChanged += Layouts_ListOrItemChanged;
            Layouts.ItemSeted += Layouts_ItemSeted;

            Views.ListOrItemChanged += Views_ListOrItemChanged;
            Rules.ListOrItemChanged += Rules_ListOrItemChanged;
            PermissionGroups_NewRow.ListOrItemChanged += PermissionGroups_NewRow_ListOrItemChanged;
            Tags.ListOrItemChanged += DatabaseTags_ListOrItemChanged;
            Bins.ListOrItemChanged += Bins_ListOrItemChanged;
            DatenbankAdmin.ListOrItemChanged += DatabaseAdmin_ListOrItemChanged;



            Row.RowRemoving += Row_BeforeRemoveRow;
            Row.RowAdded += Row_RowAdded;

            Column.ItemAdded += Column_ItemAdded;
            Column.ItemRemoving += Column_ItemRemoving;
            Column.ItemRemoved += Column_ItemRemoved;


            _isParsing = true;
            Initialize();
            _isParsing = false;

            Filename = "";


            UserGroup = "#Administrator";

            Checker.Enabled = true;
        }
        private void Initialize()
        {

            Cell.Initialize();

            Column.Initialize();

            Row.Initialize();

            Cell.RemoveOrphans();

            Works.Clear();

            ColumnArrangements.Clear();


            Layouts.Clear();



            Views.Clear();



            Rules.Clear();



            PermissionGroups_NewRow.Clear();



            Tags.Clear();



            _Export.Clear();



            Bins.Clear();

            //_TryMode.Clear();



            DatenbankAdmin.Clear();


            _GlobalShowPass = string.Empty;
            _FileEncryptionKey = string.Empty;


            _Creator = UserName;
            _CreateDate = DateTime.Now.ToString();

            _ReloadDelaySecond = 600;
            _UndoCount = 300;

            _Caption = string.Empty;
            _JoinTyp = enJoinTyp.Zeilen_verdoppeln;
            _VerwaisteDaten = enVerwaisteDaten.Ignorieren;
            _LoadedVersion = DatabaseVersion;
            _ImportScript = string.Empty;

            _GlobalScale = 1f;
            _Skin = -1; //enSkin.Unverändert;
            _Ansicht = enAnsicht.Unverändert;


            _sortDefinition = null;

            _CheckedAndReloadNeed = true;
            _LoadedFileDate = new DateTime(1900, 1, 1);
            _UserEditedAktion = new DateTime(1900, 1, 1);
        }


        #endregion

        #region  Properties 

        public string Filename { get; private set; }







        [Browsable(false)]
        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                if (_Caption == value) { return; }
                AddPending(enDatabaseDataType.Caption, -1, -1, _Caption, value, true);
            }
        }


        [Browsable(false)]
        public int Skin
        {
            get
            {
                return _Skin;
            }
            set
            {
                if (_Skin == value) { return; }
                AddPending(enDatabaseDataType.Skin, -1, -1, _Skin.ToString(), value.ToString(), true);
            }
        }

        [Browsable(false)]
        public double GlobalScale
        {
            get
            {
                return _GlobalScale;
            }
            set
            {
                if (_GlobalScale == value) { return; }
                AddPending(enDatabaseDataType.GlobalScale, -1, -1, _GlobalScale.ToString(), value.ToString(), true);
            }
        }

        [Browsable(false)]
        public enAnsicht Ansicht
        {
            get
            {
                return _Ansicht;
            }
            set
            {
                if (_Ansicht == value) { return; }
                AddPending(enDatabaseDataType.Ansicht, -1, -1, Convert.ToInt32(_Ansicht).ToString(), Convert.ToInt32(value).ToString(), true);
            }
        }


        [Browsable(false)]
        public RowSortDefinition SortDefinition
        {
            get
            {
                return _sortDefinition;
            }
            set
            {
                var Alt = string.Empty;
                var Neu = string.Empty;

                if (_sortDefinition != null) { Alt = _sortDefinition.ToString(); }
                if (value != null) { Neu = value.ToString(); }

                if (Alt == Neu) { return; }
                AddPending(enDatabaseDataType.SortDefinition, -1, -1, Alt, Neu, false);
                _sortDefinition = new RowSortDefinition(this, Neu);
                OnSortParameterChanged();

            }
        }


        [Browsable(false)]
        public string Creator
        {
            get
            {
                return _Creator.Trim();
            }
            set
            {
                if (_Creator == value) { return; }
                AddPending(enDatabaseDataType.Creator, -1, -1, _Creator, value, true);
            }
        }

        [Browsable(false)]
        public int UndoCount
        {
            get
            {
                return _UndoCount;
            }
            set
            {
                if (_UndoCount == value) { return; }
                AddPending(enDatabaseDataType.UndoCount, -1, -1, _UndoCount.ToString(), value.ToString(), true);
            }
        }

        [Browsable(false)]
        public string CreateDate
        {
            get
            {
                return _CreateDate;
            }
            set
            {
                if (_CreateDate == value) { return; }
                AddPending(enDatabaseDataType.CreateDate, -1, -1, _CreateDate, value, true);
            }
        }

        [Browsable(false)]
        public int ReloadDelaySecond
        {
            get
            {
                return _ReloadDelaySecond;
            }
            set
            {
                if (_ReloadDelaySecond == value) { return; }
                AddPending(enDatabaseDataType.ReloadDelaySecond, -1, -1, _ReloadDelaySecond.ToString(), value.ToString(), true);
            }
        }

        public string GlobalShowPass
        {
            get
            {
                return _GlobalShowPass;
            }
            set
            {
                if (_GlobalShowPass == value) { return; }
                AddPending(enDatabaseDataType.GlobalShowPass, -1, -1, _GlobalShowPass, value, true);
            }
        }


        public enJoinTyp JoinTyp
        {
            get
            {
                return _JoinTyp;
            }
            set
            {
                if (_JoinTyp == value) { return; }
                AddPending(enDatabaseDataType.JoinTyp, -1, -1, Convert.ToInt32(_JoinTyp).ToString(), Convert.ToInt32(value).ToString(), true);
            }
        }

        public enVerwaisteDaten VerwaisteDaten
        {
            get
            {
                return _VerwaisteDaten;
            }
            set
            {
                if (_VerwaisteDaten == value) { return; }
                AddPending(enDatabaseDataType.VerwaisteDaten, -1, -1, Convert.ToInt32(_VerwaisteDaten).ToString(), Convert.ToInt32(value).ToString(), true);
            }
        }

        public string ImportScript
        {
            get
            {
                return _ImportScript;
            }
            set
            {
                if (_ImportScript == value) { return; }
                AddPending(enDatabaseDataType.ImportScript, -1, -1, _ImportScript, value, true);
            }
        }

        public string FileEncryptionKey
        {
            get
            {
                return _FileEncryptionKey;
            }
            set
            {
                if (_FileEncryptionKey == value) { return; }
                AddPending(enDatabaseDataType.FileEncryptionKey, -1, -1, _FileEncryptionKey, value, true);
            }
        }



        #endregion


        private void Column_ItemRemoved(object sender, System.EventArgs e)
        {
            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Parsing Falsch!"); }
            CheckViewsAndArrangements();
        }

        private void Column_ItemRemoving(object sender, ListEventArgs e)
        {
            var Key = ((ColumnItem)e.Item).Key;
            AddPending(enDatabaseDataType.dummyComand_RemoveColumn, Key, -1, string.Empty, Key.ToString(), false);

            var L = new List<RuleItem>();
            foreach (var ThisRule in Rules)
            {
                if (ThisRule.SystemKey.StartsWith(Key + "|"))
                {
                    L.Add(ThisRule);
                }
            }

            if (L.Count == 0) { return; }
            Rules.Remove(L);
        }


        private void Column_ItemAdded(object sender, ListEventArgs e)
        {
            AddPending(enDatabaseDataType.dummyComand_AddColumn, ((ColumnItem)e.Item).Key, -1, "", ((ColumnItem)e.Item).Key.ToString(), false);
            AddPending(enDatabaseDataType.co_Name, ((ColumnItem)e.Item).Key, -1, "", Name, false);
        }

        public void AbortBackup()
        {
            if (Backup.IsBusy && !Backup.CancellationPending) { Backup.CancelAsync(); }
        }

        private void Row_RowAdded(object sender, RowEventArgs e)
        {
            if (!IsParsing())
            {
                AddPending(enDatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
                e.Row.Repair(true);
            }
        }

        private void Row_BeforeRemoveRow(object sender, RowEventArgs e)
        {
            AddPending(enDatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
        }

        private void Layouts_ItemSeted(object sender, ListEventArgs e)
        {
            if (e != null) { InvalidateExports(Layouts.IndexOf((string)e.Item)); }
        }



        //public void CopyLayout(Database Source, bool BinsToo)
        //{

        //    Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

        //    // Fehlende Spalten hinzufügen
        //    foreach (var ThisColumnItem in Source.Column)
        //    {
        //        if (ThisColumnItem != null && string.IsNullOrEmpty(ThisColumnItem.Identifier))
        //        {
        //            var DestC = Column[ThisColumnItem.Name];
        //            if (DestC == null) { DestC = new ColumnItem(ThisColumnItem, true); }
        //        }
        //    }

        //    // Spalten, die die Quelle nicht benutzt, löschen
        //    foreach (var ThisColumnItem in Column)
        //    {
        //        if (ThisColumnItem != null && string.IsNullOrEmpty(ThisColumnItem.Identifier))
        //        {
        //            if (Source.Column[ThisColumnItem.Name] == null)
        //            {
        //                Column.Remove(ThisColumnItem);
        //            }
        //        }
        //    }


        //    _Caption = Source.Caption;
        //    _Ansicht = Source.Ansicht;
        //    _Skin = Source.Skin;
        //    _JoinTyp = Source.JoinTyp;
        //    _VerwaisteDaten = Source.VerwaisteDaten;

        //    Tags.Clear();
        //    Tags.AddRange(Source.Tags);


        //    if (BinsToo)
        //    {
        //        Bins.Clear();
        //        Bins.AddRange(Source.Bins);

        //    }


        //    DatenbankAdmin.Clear();
        //    DatenbankAdmin.AddRange(Source.DatenbankAdmin);

        //    PermissionGroups_NewRow.Clear();
        //    PermissionGroups_NewRow.AddRange(Source.PermissionGroups_NewRow);

        //    Rules.Clear();
        //    foreach (var ThisRule in Source.Rules)
        //    {
        //        Rules.Add(new RuleItem(this, Source.Column.ChangeKeysToNames(ThisRule.ToString())));
        //    }

        //    _Export.Clear();
        //    foreach (var ThisExport in Source._Export)
        //    {
        //        _Export.Add(new ExportDefinition(this, ThisExport.ToString(), true));
        //    }


        //    ColumnArrangements.Clear();
        //    foreach (var ThisArrangement in Source.ColumnArrangements)
        //    {
        //        ColumnArrangements.Add(new ColumnViewCollection(this, Source.Column.ChangeKeysToNames(ThisArrangement.ToString())));
        //    }

        //    Layouts.Clear();
        //    Layouts.AddRange(Source.Layouts);

        //    Views.Clear();
        //    foreach (var ThisArrangement in Source.Views)
        //    {
        //        Views.Add(new ColumnViewCollection(this, Source.Column.ChangeKeysToNames(ThisArrangement.ToString())));
        //    }


        //    _sortDefinition = new RowSortDefinition(this, Source.Column.ChangeKeysToNames(Source.SortDefinition.ToString()));


        //    Column.Repair();
        //}



        private void CheckViewsAndArrangements()
        {

            foreach (var ThisCol in ColumnArrangements)
            {
                ThisCol.Repair(this);
            }

            foreach (var ThisCol in Views)
            {
                ThisCol.Repair(this);
            }


            if (Views != null)
            {
                if (Views.Count > 0 && Views[0].PermissionGroups_Show.Count > 0) { Views[0].PermissionGroups_Show.Clear(); }
                if (Views.Count > 1 && !Views[1].PermissionGroups_Show.Contains("#Everybody")) { Views[1].PermissionGroups_Show.Add("#Everybody"); }
            }


        }





        private void DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
        }

        private void DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.DatenbankAdmin, -1, DatenbankAdmin.JoinWithCr(), false);
        }

        private void PermissionGroups_NewRow_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.PermissionGroups_NewRow, -1, PermissionGroups_NewRow.JoinWithCr(), false);
        }

        private void Layouts_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
        }

        private void Bins_ListOrItemChanged(object sender, System.EventArgs e)
        {

            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.BinaryDataInOne, -1, Bins.ToString(true), false);
        }


        private void Rules_ListOrItemChanged(object sender, System.EventArgs e)
        {

            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.

            if (sender == Rules)
            {
                AddPending(enDatabaseDataType.Rules, -1, Rules.ToString(true), false);
                return;
            }

            if (sender is RuleItem RL)
            {
                if (!Rules.Contains(RL)) { return; }
                AddPending(enDatabaseDataType.Rules, -1, Rules.ToString(true), false);
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Typ ist kein RuleItem");
            }
        }

        private void Views_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Views, -1, Views.ToString(true), false);
        }




        private void ColumnArrangements_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (_isParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(true), false);
            //OnColumnArrangementsChanged();
        }






        ///// <summary>
        ///// Wartet, bis die Datenbank-Datei zugänglich ist.
        ///// </summary>
        ///// <param name="Sec"></param>
        ///// <param name="MitReload">Wenn es aus einem Worker aufgerufen wird, muss MitReload false sein!</param>
        ///// <returns></returns>
        //public bool WindowsWaitFree(int Sec, bool MitReload)
        //{

        //    if (ReadOnly) { return true; }
        //    if (string.IsNullOrEmpty(Filename)) { return true; }
        //    if (!FileExists(Filename)) { return true; }


        //    var x = DateTime.Now;
        //    do
        //    {
        //        if (MitReload && !_isParsing) { Reload(); }

        //        var Fed = SavebleErrorReason_WindowsOnly();

        //        if (MitReload && ReloadNeeded())
        //        {

        //            if (Backup.IsBusy || BinSaver.IsBusy || BinReLoader.IsBusy)
        //            {
        //                Fed = "Reload kann nicht ausgeführt werden";
        //                Develop.DebugPrint(enFehlerArt.Warnung, "Reload kann nicht ausgeführt werden, Worker gerade in Arbeit");
        //            }


        //            if (_isParsing)
        //            {
        //                Fed = "Reload kann nicht ausgeführt werden";
        //                Develop.DebugPrint(enFehlerArt.Warnung, "Reload kann nicht ausgeführt werden, kein Parsing Handling definiert");
        //            }

        //        }


        //        if (string.IsNullOrEmpty(Fed)) { return true; }

        //        if (DateTime.Now.Subtract(x).TotalSeconds > Sec) { break; }

        //        modAllgemein.Pause(0.2, false);
        //    } while (true);

        //   return false;
        //}


        public void SaveAsAndChangeTo(string NewFileName)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);

            if (NewFileName.ToUpper() == Filename.ToUpper()) { Develop.DebugPrint(enFehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }

            Release(true); // Original-Datenbank speichern, die ist ja dann weg.
            var l = ToListOfByte();
            using (var x = new FileStream(NewFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                x.Write(l.ToArray(), 0, l.ToArray().Length);
                x.Flush();
                x.Close();
            }
            Filename = NewFileName;
            _LoadedFileDate = new FileInfo(Filename).LastWriteTime;
            _CheckedAndReloadNeed = false;
        }




        /// <summary>
        /// Lädt die Datenbank von der Festplatte.
        /// Es die parallelen Threads benutzt. Diese Routine wartet aber, bis die Daten sichern neu geladen wurden.
        /// Falls die Datenbank bereits geladen wurde, und kein Reload nötig ist, wird die Datenbank nicht geladen.
        /// Der Unterschied zum Reload ist, dass bei einem Reload Stor und Restore Events ausgelöst werden.
        /// </summary>
        private void LoadFromDisk()
        {
            // Stelle sicher, dass der Prozess nicht läuft- Nur Sicherheitshalber, sollte eigentlich eh nicht sein.
            while (BinReLoader.IsBusy) { Develop.DoEvents(); }

            // Nun starte den Prozess
            BinReLoader.RunWorkerAsync();

            // Stelle Sicher, dass er auch läuft
            while (!BinReLoader.IsBusy) { Develop.DoEvents(); }

            // Und nun warte, bis er fertig ist :-)
            while (BinReLoader.IsBusy) { Develop.DoEvents(); }
        }


        public void Parse(List<byte> _BLoaded, ref int Pointer, ref enDatabaseDataType Art, ref int ColNR, ref int RowNR, ref string Wert, ref int X, ref int Y)
        {

            var Les = 0;

            switch ((enRoutinen)_BLoaded[Pointer])
            {
                case enRoutinen.CellFormat_OLD:
                    {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = NummerCode3(_BLoaded, Pointer + 5);
                        RowNR = NummerCode3(_BLoaded, Pointer + 8);

                        //  Wert = String.
                        var b = new byte[Les];
                        _BLoaded.CopyTo(Pointer + 11, b, 0, Les);
                        Wert = Encoding.Default.GetString(b);

                        //     Wert = System.Text.Encoding.Default.GetString(BLoaded, Pointer + 11, Les)
                        X = NummerCode2(_BLoaded, Pointer + 11 + Les);
                        Y = NummerCode2(_BLoaded, Pointer + 11 + Les + 2);
                        Pointer += 11 + Les + 4;


                        break;
                    }
                case enRoutinen.DatenAllgemein:
                    {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = -1;
                        RowNR = -1;
                        var b = new byte[Les];
                        _BLoaded.CopyTo(Pointer + 5, b, 0, Les);
                        Wert = Encoding.Default.GetString(b);


                        //   Wert = System.Text.Encoding.Default.GetString(BLoaded, Pointer + 5, Les)
                        X = 0;
                        Y = 0;
                        Pointer += 5 + Les;

                        break;
                    }
                case enRoutinen.Column:
                    {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = NummerCode3(_BLoaded, Pointer + 5);
                        RowNR = NummerCode3(_BLoaded, Pointer + 8);
                        var b = new byte[Les];
                        _BLoaded.CopyTo(Pointer + 11, b, 0, Les);
                        Wert = Encoding.Default.GetString(b);

                        X = 0;
                        Y = 0;
                        Pointer += 11 + Les;
                        break;
                    }
                default:
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Laderoutine nicht definiert: " + _BLoaded[Pointer]);
                        break;
                    }
            }
        }



        public bool IsParsing()
        {
            return _isParsing;
        }

        private void Parse(List<byte> B)
        {
            //lock (Lock_Parsing)
            //{
            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Doppelter Parse!"); }
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank eingefroren"); }

            _isParsing = true;
            Column.ThrowEvents = false;

            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            enDatabaseDataType Art = 0;
            var Pointer = 0;

            var Inhalt = "";
            ColumnItem _Column = null;


            RowItem _Row = null;

            var X = 0;
            var Y = 0;


            var ColKey = 0;
            var RowKey = 0;


            var ColumnsOld = new List<ColumnItem>();
            ColumnsOld.AddRange(Column);
            Column.Clear();


            var OldPendings = new List<WorkItem>();
            foreach (var ThisWork in Works)
            {
                if (ThisWork.State == enItemState.Pending) { OldPendings.Add(ThisWork); }
                if (ThisWork.State == enItemState.FreezedPending) { Develop.DebugPrint(enFehlerArt.Fehler, "FreezedPending vorhanden"); }
            }

            Works.Clear();
            do
            {
                if (Pointer >= B.Count) { break; }

                Parse(B, ref Pointer, ref Art, ref ColKey, ref RowKey, ref Inhalt, ref X, ref Y);

                if (RowKey > -1)
                {
                    if (_Row == null || _Row.Key != RowKey)
                    {
                        _Row = Row.SearchByKey(RowKey);
                        if (_Row == null)
                        {
                            _Row = new RowItem(this, RowKey);
                            Row.Add(_Row);
                        }
                    }
                }

                if (ColKey > -1)
                {
                    if (_Column == null || _Column.Key != ColKey)
                    {
                        // Zuerst schauen, ob die Column schon (wieder) in der richtigen Collection ist
                        _Column = Column.SearchByKey(ColKey);

                        if (_Column == null)
                        {
                            // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzufügen
                            foreach (var ThisColumn in ColumnsOld)
                            {
                                if (ThisColumn != null && ThisColumn.Key == ColKey)
                                {
                                    _Column = ThisColumn;
                                }
                            }

                            if (_Column != null)
                            {
                                // Prima, gefunden! Noch die Collections korrigieren
                                Column.Add(_Column);
                                ColumnsOld.Remove(_Column);
                            }
                            else
                            {
                                // Nicht gefunden, als neu machen
                                _Column = new ColumnItem(this, ColKey, "", true);
                            }
                        }
                    }
                }


                if (Art == enDatabaseDataType.CryptionState)
                {
                    if (Inhalt.FromPlusMinus())
                    {
                        var Pass = _PasswordSub();

                        B = modAllgemein.SimpleCrypt(B, Pass, -1, Pointer, B.Count - 1);
                        if (B[Pointer] != 1 || B[Pointer + 1] != 3 || B[Pointer + 2] != 0 || B[Pointer + 3] != 0 || B[Pointer + 4] != 2 || B[Pointer + 5] != 79 || B[Pointer + 6] != 75 || B[Pointer + 7] != 1)
                        {
                            Filename = "";
                            _LoadedVersion = "9.99";
                            //MessageBox.Show("Zugriff verweigrt, Passwort falsch!", enImageCode.Kritisch, "OK");
                            break;
                        }

                    }
                }


                var _Fehler = ParseThis(Art, Inhalt, _Column, _Row, X, Y);

                if (Art == enDatabaseDataType.EOF) { break; }


                if (!string.IsNullOrEmpty(_Fehler))
                {
                    _LoadedVersion = "9.99";
                    Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + Filename + "<br>Meldung: " + _Fehler);
                }


            } while (true);

            // Spalten, die nach dem Reload nicht mehr benötigt werden, löschen
            //ColumnsOld.DisposeAndRemoveAll();


            Row.RemoveNullOrEmpty();
            //Row.RemoveNullOrDisposed();
            Cell.RemoveOrphans();



            LoadPicsIntoImageChache();

            FilesAfterLoadingLCase.Clear();
            FilesAfterLoadingLCase.AddRange(AllConnectedFilesLCase());

            Works.AddRange(OldPendings);
            OldPendings.Clear();
            ExecutePending();


            Column.ThrowEvents = true;
            _isParsing = false;

            //Repair NACH ExecutePendung, vielleicht ist es schon repariert
            //Repair NACH _isParsing, da es auch abgespeichert werden soll
            Repair();

        }


        public void Repair()
        {

            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            // System-Spalten checken und alte Formate auf neuen Stand bringen
            Column.Repair();

            // Evtl. Defekte Rows reparieren
            Row.Repair();

            //// Regeln prüfen lassen
            //CheckRules();

            //Defekte Ansichten reparieren - Teil 1
            for (var z = 0; z <= 1; z++)
            {
                if (ColumnArrangements.Count < z + 1)
                {
                    ColumnArrangements.Add(new ColumnViewCollection(this, ""));
                }

                if (z == 0 || ColumnArrangements[z].Count() < 1)
                {
                    ColumnArrangements[z].ShowAllColumns(this);
                }
                if (string.IsNullOrEmpty(ColumnArrangements[z].Name))
                {
                    switch (z)
                    {
                        case 0:
                            ColumnArrangements[z].Name = "Alle Spalten";
                            break;
                        case 1:
                            ColumnArrangements[z].Name = "Standard";
                            break;
                    }
                }
                if (z == 1 && !ColumnArrangements[z].PermissionGroups_Show.Contains("#Everybody")) { ColumnArrangements[z].PermissionGroups_Show.Add("#Everybody"); }
                ColumnArrangements[z].PermissionGroups_Show.RemoveString("#Administrator", false);
            }

            CheckViewsAndArrangements();
        }



        public string ParseThis(enDatabaseDataType Art, string Inhalt, ColumnItem _Column, RowItem _Row, int X, int Y)
        {


            if (Art >= enDatabaseDataType.Info_ColumDataSart && Art <= enDatabaseDataType.Info_ColumnDataEnd)
            {
                return _Column.Load(Art, Inhalt);
            }





            switch (Art)
            {
                case enDatabaseDataType.Formatkennung:
                    break;
                case enDatabaseDataType.Version:
                    _LoadedVersion = Inhalt.Trim();
                    if (_LoadedVersion != DatabaseVersion)
                    {
                        Initialize();
                        _LoadedVersion = Inhalt.Trim();
                    }
                    else
                    {
                        //Cell.RemoveOrphans();
                        Row.RemoveNullOrEmpty();
                        Cell.SetAllValuesToEmpty();
                    }
                    break;

                case enDatabaseDataType.Werbung:
                    break;
                case enDatabaseDataType.CryptionState:
                    break;
                case enDatabaseDataType.CryptionTest:
                    break;
                case enDatabaseDataType.Creator:
                    _Creator = Inhalt;
                    break;
                case enDatabaseDataType.CreateDate:
                    _CreateDate = Inhalt;
                    break;
                case enDatabaseDataType.ReloadDelaySecond:
                    _ReloadDelaySecond = int.Parse(Inhalt);
                    break;
                case enDatabaseDataType.DatenbankAdmin:
                    DatenbankAdmin.SplitByCR_QuickSortAndRemoveDouble(Inhalt);
                    break;
                case enDatabaseDataType.SortDefinition:
                    _sortDefinition = new RowSortDefinition(this, Inhalt);
                    break;
                case enDatabaseDataType.Caption:
                    _Caption = Inhalt;
                    break;
                case enDatabaseDataType.Skin:
                    _Skin = int.Parse(Inhalt);
                    break;
                case enDatabaseDataType.GlobalScale:
                    _GlobalScale = double.Parse(Inhalt);
                    break;
                case enDatabaseDataType.Ansicht:
                    _Ansicht = (enAnsicht)int.Parse(Inhalt);
                    break;
                case enDatabaseDataType.Tags:
                    Tags.SplitByCR_QuickSortAndRemoveDouble(Inhalt);
                    break;
                case enDatabaseDataType.BinaryDataInOne:
                    Bins.Clear();
                    var l = new List<string>(Inhalt.SplitByCR());
                    foreach (var t in l)
                    {
                        Bins.Add(new clsNamedBinary(t));
                    }
                    break;
                case enDatabaseDataType.Layouts:
                    Layouts.SplitByCR_QuickSortAndRemoveDouble(Inhalt);
                    break;
                case enDatabaseDataType.AutoExport:
                    _Export.Clear();
                    var AE = new List<string>(Inhalt.SplitByCR());
                    foreach (var t in AE)
                    {
                        _Export.Add(new ExportDefinition(this, t));
                    }
                    break;

                case enDatabaseDataType.Rules:
                    Rules.Clear();
                    var RU = Inhalt.SplitByCR();
                    for (var z = 0; z <= RU.GetUpperBound(0); z++)
                    {
                        Rules.Add(new RuleItem(this, RU[z]));
                    }
                    break;

                case enDatabaseDataType.ColumnArrangement:
                    ColumnArrangements.Clear();
                    var CA = new List<string>(Inhalt.SplitByCR());
                    foreach (var t in CA)
                    {
                        ColumnArrangements.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.Views:
                    Views.Clear();
                    var VI = new List<string>(Inhalt.SplitByCR());
                    foreach (var t in VI)
                    {
                        Views.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.PermissionGroups_NewRow:
                    PermissionGroups_NewRow.SplitByCR_QuickSortAndRemoveDouble(Inhalt);
                    break;

                case enDatabaseDataType.LastRowKey:
                    return Row.Load_310(Art, Inhalt);

                case enDatabaseDataType.LastColumnKey:
                    return Column.Load_310(Art, Inhalt);

                case enDatabaseDataType.GlobalShowPass:
                    _GlobalShowPass = Inhalt;
                    break;

                case (enDatabaseDataType)30:
                    // TODO: Entferne GlobalInfo
                    break;

                case enDatabaseDataType.JoinTyp:
                    _JoinTyp = (enJoinTyp)int.Parse(Inhalt);
                    break;

                case enDatabaseDataType.VerwaisteDaten:
                    _VerwaisteDaten = (enVerwaisteDaten)int.Parse(Inhalt);
                    break;

                case enDatabaseDataType.ImportScript:
                    _ImportScript = Inhalt;
                    break;

                case enDatabaseDataType.FileEncryptionKey:
                    _FileEncryptionKey = Inhalt;
                    break;


                case enDatabaseDataType.ce_Value_withSizeData:
                case enDatabaseDataType.ce_UTF8Value_withSizeData:
                case enDatabaseDataType.ce_Value_withoutSizeData:
                    if (Art == enDatabaseDataType.ce_UTF8Value_withSizeData) { Inhalt = modConverter.UTF8toString(Inhalt); }
                    Cell.Load_310(_Column, _Row, Inhalt, X, Y);
                    break;

                case enDatabaseDataType.UndoCount:
                    _UndoCount = int.Parse(Inhalt);
                    break;


                case enDatabaseDataType.UndoInOne:
                    Works.Clear();
                    var UIO = Inhalt.SplitByCR();
                    for (var z = 0; z <= UIO.GetUpperBound(0); z++)
                    {
                        var tmpWork = new WorkItem(UIO[z]);
                        tmpWork.State = enItemState.Undo; // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Desegen die alten "Pendings" zu Undos ändern.
                        Works.Add(tmpWork);
                    }
                    break;


                case enDatabaseDataType.dummyComand_AddRow:
                    var Key = int.Parse(Inhalt);
                    _Row = Row.SearchByKey(Key);
                    if (_Row == null)
                    {
                        _Row = new RowItem(this, Key);
                        Row.Add(_Row);
                    }
                    break;

                case enDatabaseDataType.dummyComand_AddColumn:
                    var tKey = int.Parse(Inhalt);
                    _Column = Column.SearchByKey(tKey);
                    if (_Column == null) { _Column = new ColumnItem(this, tKey, "", true); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveRow:
                    var trKey = int.Parse(Inhalt);
                    _Row = Row.SearchByKey(trKey);
                    if (_Row != null) { Row.Remove(trKey); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveColumn:
                    var rvKey = int.Parse(Inhalt);
                    _Column = Column.SearchByKey(rvKey);
                    if (_Column != null) { Column.Remove(_Column); }
                    break;

                case enDatabaseDataType.EOF:
                    return "";

                default:
                    _LoadedVersion = "9.99";
                    if (!ReadOnly)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Laden von Datentyp \'" + Art + "\' nicht definiert.<br>Wert: " + Inhalt + "<br>Datei: " + Filename);
                    }
                    break;
            }

            return "";
        }


        public void LoadPicsIntoImageChache()
        {
            foreach (var bmp in Bins)
            {
                if (bmp.Picture != null)
                {
                    if (!string.IsNullOrEmpty(bmp.Name))
                    {
                        QuickImage.Add("DB_" + bmp.Name, bmp.Picture);
                    }
                }

            }
        }

        internal void Column_NameChanged(string OldName, ColumnItem cColumnItem)
        {


            // Cells ----------------------------------------------
            //   Cell.ChangeCaptionName(OldName, cColumnItem.Name, cColumnItem)


            //  Undo -----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind

            // Layouts -----------------------------------------
            if (Layouts != null && Layouts.Count > 0)
            {
                for (var cc = 0; cc < Layouts.Count; cc++)
                {
                    Layouts[cc] = _RenameColumnInLayout(this, Layouts[cc], OldName, cColumnItem);
                }
            }


            // Sortierung -----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind

            // _ColumnArrangements-----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind

            // _Views-----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind


            // Rules -----------------------------------------
            // Wichtig, dass Rechenformeln richtiggestellt werden
            foreach (var ThisRule in Rules)
            {
                ThisRule?.RenameColumn(OldName, cColumnItem);
            }
        }


        private void LoadFromStream(Stream Stream)
        {

            var _BLoaded = new List<byte>();


            using (var r = new BinaryReader(Stream))
            {
                _BLoaded.AddRange(r.ReadBytes((int)Stream.Length));
                r.Close();
            }

            Parse(_BLoaded);
        }


        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content)
        {
            List.Add((byte)enRoutinen.DatenAllgemein);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, Content.Length, 3);
            List.AddRange(Content.ToByte());
        }

        internal void SaveToByteList(List<byte> List, KeyValuePair<string, CellItem> vCell)
        {

            if (string.IsNullOrEmpty(vCell.Value.Value)) { return; }

            Cell.DataOfCellKey(vCell.Key, out var tColumn, out var tRow);

            var s = vCell.Value.Value;
            var tx = enDatabaseDataType.ce_Value_withSizeData;

            if (tColumn.Format.NeedUTF8())
            {
                s = modConverter.StringtoUTF8(s);
                tx = enDatabaseDataType.ce_UTF8Value_withSizeData;
            }

            List.Add((byte)enRoutinen.CellFormat_OLD);
            List.Add((byte)tx);
            SaveToByteList(List, s.Length, 3);
            SaveToByteList(List, tColumn.Key, 3);
            SaveToByteList(List, tRow.Key, 3);
            List.AddRange(s.ToByte());
            var ContentSize = Cell.ContentSizeToSave(vCell, tColumn);
            SaveToByteList(List, ContentSize.Width, 2);
            SaveToByteList(List, ContentSize.Height, 2);

        }


        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content, int TargetColumNr)
        {
            List.Add((byte)enRoutinen.Column);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, Content.Length, 3);
            SaveToByteList(List, TargetColumNr, 3);
            SaveToByteList(List, 0, 3); //Zeile-Unötig
            List.AddRange(Content.ToByte());
        }

        private static int NummerCode3(List<byte> b, int pointer)
        {
            return b[pointer] * 65025 + b[pointer + 1] * 255 + b[pointer + 2];
        }

        private static int NummerCode2(List<byte> b, int pointer)
        {
            return b[pointer] * 255 + b[pointer + 1];
        }

        private void SaveToByteList(List<byte> List, int NrToAdd, int ByteAnzahl)
        {

            switch (ByteAnzahl)
            {
                case 3:
                    List.Add(Convert.ToByte(Math.Truncate(NrToAdd / 65025.0)));
                    List.Add(Convert.ToByte(Math.Truncate(NrToAdd % 65025 / 255.0)));
                    List.Add((byte)(NrToAdd % 65025 % 255));
                    break;
                case 2:
                    List.Add(Convert.ToByte(Math.Truncate(NrToAdd / 255.0)));
                    List.Add((byte)(NrToAdd % 255));
                    break;

                case 1:
                    List.Add((byte)NrToAdd);
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Byteanzahl unbekannt!");
                    break;
            }
        }


        public void UnlockHard()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (IsSaveAble() && !BlockDateiVorhanden()) { return; }
            Reload();
            if (BlockDateiVorhanden()) { DeleteFile(Blockdateiname(), true); }
            Release(true);
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


        /// <summary>
        /// Gibt einen Fehlergrund zurück, wenn gerade eben kein Speicherzugriff auf die Datei möglich ist.
        /// </summary>
        /// <returns></returns>
        public string SavebleErrorReason()
        {
            if (ReadOnly) { return "Datenbank wurde schreibgeschützt geöffnet"; }
            if (Cell.Freezed) { return "Datenbank gerade eingefroren."; }


            if (int.Parse(_LoadedVersion.Replace(".", "")) > int.Parse(DatabaseVersion.Replace(".", ""))) { return "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."; }

            if (BlockDateiVorhanden()) { return "Beim letzten Versuch, die Datei zu speichern, ist der Speichervorgang nicht korrekt beendet worden. Speichern ist solange deaktiviert, bis ein Administrator die Freigabe zum Speichern erteilt."; }


            if (BinReLoader.IsBusy) { return "Speichern aktuell nicht möglich, da gerade Daten geladen werden."; }
            if (Backup.IsBusy) { return "Speichern aktuell nicht möglich, da gerade Sicherheitskopien erstellt werden."; }

            //var f = NeedEditLockReason(null, null);
            //if (!string.IsNullOrEmpty(f)) { return f; }


            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
            if (!FileExists(Filename)) { return string.Empty; }



            if (!CanWriteInDirectory(Filename.FilePath())) { return "Sie haben im Verzeichnis der Datenbank keine Schreibrechte."; }

            if (DateTime.Now.Subtract(SavebleErrorReason_WindowsOnly_lastChecked).TotalSeconds < 0) { return "Windows blockiert die Datenbank-Datei."; }

            if (!CanWrite(Filename, 0.5))
            {
                SavebleErrorReason_WindowsOnly_lastChecked = DateTime.Now.AddSeconds(5);
                return "Windows blockiert die Datenbank-Datei.";
            }



            return string.Empty;
        }


        public bool ReloadNeeded()
        {

            //        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);


            if (string.IsNullOrEmpty(Filename)) { return false; }

            if (_CheckedAndReloadNeed) { return true; }


            if (new FileInfo(Filename).LastWriteTime.CompareTo(_LoadedFileDate) != 0)
            {
                _CheckedAndReloadNeed = true;
                return true;
            }


            return false;
        }

        public bool HasPendingChanges()
        {
            if (ReadOnly) { return false; }

            foreach (var ThisWork in Works)
            {
                if (ThisWork.State == enItemState.Pending) { return true; }
            }
            return false;
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



        #region  Export CSV / HTML 


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow FirstRow)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            return Export_CSV(FirstRow, (List<ColumnItem>)null, null);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow FirstRow, List<ColumnItem> ColList, List<RowItem> SortedRows)
        {
            if (ColList == null)
            {
                ColList = new List<ColumnItem>();
                foreach (var ThisColumnItem in Column)
                {
                    if (ThisColumnItem != null)
                    {
                        ColList.Add(ThisColumnItem);
                    }
                }
            }

            if (SortedRows == null)
            {
                SortedRows = new List<RowItem>();
                foreach (var ThisRowItem in Row)
                {
                    if (ThisRowItem != null)
                    {
                        SortedRows.Add(ThisRowItem);
                    }
                }
            }

            var sb = new StringBuilder();


            switch (FirstRow)
            {
                case enFirstRow.Without:

                    break;
                case enFirstRow.ColumnCaption:
                    for (var ColNr = 0; ColNr < ColList.Count; ColNr++)
                    {
                        if (ColList[ColNr] != null)
                        {
                            sb.Append(ColList[ColNr].ReadableText().Replace("; ", "|"));
                            if (ColNr < ColList.Count - 1) { sb.Append(";"); }
                        }
                    }
                    sb.Append("\r\n");


                    break;
                case enFirstRow.ColumnInternalName:
                    for (var ColNr = 0; ColNr < ColList.Count; ColNr++)
                    {
                        if (ColList[ColNr] != null)
                        {
                            sb.Append(ColList[ColNr].Name);
                            if (ColNr < ColList.Count - 1) { sb.Append(";"); }
                        }
                    }
                    sb.Append("\r\n");

                    break;
                default:
                    Develop.DebugPrint(FirstRow);
                    break;
            }





            foreach (var ThisRow in SortedRows)
            {
                if (ThisRow != null)
                {
                    for (var ColNr = 0; ColNr < ColList.Count; ColNr++)
                    {
                        if (ColList[ColNr] != null)
                        {
                            sb.Append(Cell.GetString(ColList[ColNr], ThisRow).Replace("\r", "|"));
                            if (ColNr < ColList.Count - 1)
                            {
                                sb.Append(";");
                            }
                        }
                    }
                    sb.Append("\r\n");
                }
            }


            return sb.ToString().TrimEnd("\r\n");
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow FirstRow, ColumnViewCollection Arr, List<RowItem> SortedRows)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            return Export_CSV(FirstRow, Arr.ListOfUsedColumn(), SortedRows);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow FirstRow, int ArrNr, FilterCollection Filter)
        {
            //    Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            return Export_CSV(FirstRow, ColumnArrangements[ArrNr].ListOfUsedColumn(), Row.CalculateSortedRows(Filter, SortDefinition));
        }


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string vFilename)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_HTML(vFilename, (List<ColumnItem>)null, null, false);
        }


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string vFilename, int ArrNr, FilterCollection Filter)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_HTML(vFilename, ColumnArrangements[ArrNr].ListOfUsedColumn(), Row.CalculateSortedRows(Filter, SortDefinition), false);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(bool Execute)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_HTML(string.Empty, (List<ColumnItem>)null, null, Execute);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string vFilename, List<ColumnItem> ColList, List<RowItem> SortedRows, bool Execute)
        {
            if (ColList == null || ColList.Count == 0)
            {
                ColList = new List<ColumnItem>();
                foreach (var ThisColumnItem in Column)
                {
                    if (ThisColumnItem != null)
                    {
                        ColList.Add(ThisColumnItem);
                    }
                }
            }

            if (SortedRows == null)
            {
                SortedRows = new List<RowItem>();
                foreach (var ThisRowItem in Row)
                {
                    if (ThisRowItem != null)
                    {
                        SortedRows.Add(ThisRowItem);
                    }
                }
            }

            if (string.IsNullOrEmpty(vFilename))
            {
                vFilename = TempFile(string.Empty, "Export", "html");
            }


            var da = new List<string>();
            modAllgemein.HTML_AddHead(da, Filename.FileNameWithoutSuffix());
            da.Add("  <Font face=\"Arial\" Size=\"7\">" + _Caption + "</h1><br>");
            da.Add("  <Font face=\"Arial\" Size=\"2\"><table border=\"1\" BORDERCOLOR=\"#aaaaaa\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">");
            da.Add("      <tr>");


            foreach (var ThisColumn in ColList)
            {
                if (ThisColumn != null)
                {
                    da.Add("        <th BORDERCOLOR=\"#aaaaaa\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"><b>" + ThisColumn.ReadableText().Replace(";", "<br>") + "</b></th>");
                }
            }
            da.Add("      </tr>");


            foreach (var ThisRow in SortedRows)
            {
                if (ThisRow != null)
                {
                    da.Add("      <tr>");
                    foreach (var ThisColumn in ColList)
                    {
                        if (ThisColumn != null)
                        {

                            var LCColumn = ThisColumn;
                            var LCrow = ThisRow;
                            if (ThisColumn.Format == enDataFormat.LinkedCell) { Cell.LinkedCellData(ThisColumn, ThisRow, out LCColumn, out LCrow); }

                            if (LCrow != null && LCColumn != null)
                            {
                                da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">" + LCrow.CellGetStringForExport(LCColumn) + "</th>");
                            }
                            else
                            {
                                da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"> </th>");

                            }


                        }
                    }
                    da.Add("      </tr>");
                }
            }


            // Summe----
            da.Add("      <tr>");
            foreach (var ThisColumn in ColList)
            {
                if (ThisColumn != null)
                {
                    var s = ThisColumn.Summe(SortedRows);
                    if (s == null)
                    {
                        da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                    }
                    else
                    {
                        da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">&sum; " + s + "</th>");
                    }
                }
            }
            da.Add("      </tr>");

            // ----------------------

            da.Add("    </table>");
            modAllgemein.HTML_AddFoot(da);
            da.Save(vFilename, Execute);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string vFilename, ColumnViewCollection Arr, List<RowItem> SortedRows, bool Execute)
        {
            Export_HTML(vFilename, Arr.ListOfUsedColumn(), SortedRows, Execute);
        }


        #endregion


        public bool PermissionCheckWithoutAdmin(string Allowed, RowItem vRowx)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            var UN = "#*Dummy*#";
            var UG = "#*Dummy*#";
            if (!string.IsNullOrEmpty(UserName)) { UN = UserName.ToUpper(); }
            if (!string.IsNullOrEmpty(UserGroup)) { UG = UserGroup.ToUpper(); }






            if (Allowed.ToUpper() == "#EVERYBODY")
            {
                return true;
            }

            if (Allowed.ToUpper() == "#ROWCREATOR")
            {
                if (vRowx != null && Cell.GetString(Column.SysRowCreator, vRowx).ToUpper() == UN)
                {
                    return true;
                }
            }
            else if (Allowed.ToUpper() == "#ROWCHANGER")
            {
                if (vRowx != null && Cell.GetString(Column.SysRowChanger, vRowx).ToUpper() == UN)
                {
                    return true;
                }
            }
            else if (Allowed.ToUpper() == "#USER: " + UN)
            {
                return true;
            }
            else if (Allowed.ToUpper() == "#USER:" + UN)
            {
                return true;
            }
            else if (Allowed.ToUpper() == UG)
            {
                return true;
            }
            else if (Allowed.ToUpper() == "#DATABASECREATOR")
            {
                if (UserName.ToUpper() == _Creator.ToUpper()) { return true; }
            }

            return false;
        }


        public bool PermissionCheck(ListExt<string> allowed, RowItem row)
        {
            try
            {
                //if (InvokeRequiredXXX) { return false; }

                //{                if (InvokeRequiredXXX)

                //    return (bool)Invoke(new InvokePermissionCheckEventHandler(PermissionCheck), Allowed, Row);
                //}


                if (IsAdministrator()) { return true; }
                if (allowed == null || allowed.Count == 0) { return false; }

                foreach (var ThisString in allowed)
                {
                    if (PermissionCheckWithoutAdmin(ThisString, row)) { return true; }
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
            }

            return false;
        }

        public List<string> Permission_AllUsed(bool DatabaseEbene)
        {
            var e = new List<string>();

            foreach (var ThisColumnItem in Column)
            {
                if (ThisColumnItem != null)
                {
                    e.AddRange(ThisColumnItem.PermissionGroups_ChangeCell);
                }
            }

            e.AddRange(PermissionGroups_NewRow);
            e.AddRange(DatenbankAdmin);

            foreach (var ThisArrangement in ColumnArrangements)
            {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }

            foreach (var ThisArrangement in Views)
            {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }



            e.Add("#DatabaseCreator");
            e.Add("#Everybody");
            e.Add("#User: " + UserName);

            if (!DatabaseEbene)
            {
                e.Add("#RowCreator");
                e.Add("#RowChanger");
            }
            else
            {
                e.RemoveString("#RowCreator", false);
                e.RemoveString("#RowChanger", false);

            }

            e.RemoveString("#Administrator", false);
            if (!IsAdministrator())
            {
                e.Add(UserGroup);
            }
            return e.SortedDistinctList();
        }


        public bool IsAdministrator()
        {
            if (DatenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
            if (string.IsNullOrEmpty(UserGroup)) { return false; }
            if (DatenbankAdmin.Contains(UserGroup, false)) { return true; }
            return Convert.ToBoolean(UserGroup.ToUpper() == "#ADMINISTRATOR");
        }


        public new string ToString()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            return ToListOfByte().ToArray().ToStringConvert();
        }


        private List<byte> ToListOfByte()
        {
            var CryptPos = -1;
            var l = new List<byte>();

            // Wichtig, Reihenfolge und Länge NIE verändern!
            SaveToByteList(l, enDatabaseDataType.Formatkennung, "BlueDatabase");
            SaveToByteList(l, enDatabaseDataType.Version, DatabaseVersion);
            SaveToByteList(l, enDatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...

            // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
            if (string.IsNullOrEmpty(_GlobalShowPass))
            {
                SaveToByteList(l, enDatabaseDataType.CryptionState, false.ToPlusMinus());
            }
            else
            {
                SaveToByteList(l, enDatabaseDataType.CryptionState, true.ToPlusMinus());
                CryptPos = l.Count;
                SaveToByteList(l, enDatabaseDataType.CryptionTest, "OK");
            }


            SaveToByteList(l, enDatabaseDataType.GlobalShowPass, _GlobalShowPass);
            SaveToByteList(l, enDatabaseDataType.FileEncryptionKey, _FileEncryptionKey);
            SaveToByteList(l, enDatabaseDataType.Creator, _Creator);
            SaveToByteList(l, enDatabaseDataType.CreateDate, _CreateDate);

            SaveToByteList(l, enDatabaseDataType.Caption, _Caption);
            SaveToByteList(l, enDatabaseDataType.JoinTyp, Convert.ToInt32(_JoinTyp).ToString());
            SaveToByteList(l, enDatabaseDataType.VerwaisteDaten, Convert.ToInt32(_VerwaisteDaten).ToString());
            SaveToByteList(l, enDatabaseDataType.Tags, Tags.JoinWithCr());
            SaveToByteList(l, enDatabaseDataType.PermissionGroups_NewRow, PermissionGroups_NewRow.JoinWithCr());
            SaveToByteList(l, enDatabaseDataType.DatenbankAdmin, DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, enDatabaseDataType.Skin, _Skin.ToString());
            SaveToByteList(l, enDatabaseDataType.GlobalScale, _GlobalScale.ToString());
            SaveToByteList(l, enDatabaseDataType.Ansicht, Convert.ToInt32(_Ansicht).ToString());
            SaveToByteList(l, enDatabaseDataType.ReloadDelaySecond, _ReloadDelaySecond.ToString());
            SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);

            SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));


            Column.SaveToByteList(l);
            Row.SaveToByteList(l);

            Cell.SaveToByteList(ref l);

            if (SortDefinition == null)
            {
                // Ganz neue Datenbank
                SaveToByteList(l, enDatabaseDataType.SortDefinition, "");
            }
            else
            {
                SaveToByteList(l, enDatabaseDataType.SortDefinition, _sortDefinition.ToString());
            }


            SaveToByteList(l, enDatabaseDataType.Rules, Rules.ToString(true));



            SaveToByteList(l, enDatabaseDataType.ColumnArrangement, ColumnArrangements.ToString(true));

            SaveToByteList(l, enDatabaseDataType.Views, Views.ToString(true));

            SaveToByteList(l, enDatabaseDataType.Layouts, Layouts.JoinWithCr());

            SaveToByteList(l, enDatabaseDataType.AutoExport, _Export.ToString(true));

            // Beim Erstellen des Undo-Speichers die Works nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
            var Works2 = new List<string>();
            foreach (var thisWorkItem in Works)
            {
                if (thisWorkItem.Comand != enDatabaseDataType.ce_Value_withoutSizeData)
                {
                    Works2.Add(thisWorkItem.ToString());
                }
                else
                {
                    if (Column.SearchByKey(thisWorkItem.ColKey) is ColumnItem C && C.ShowUndo)
                    {
                        Works2.Add(thisWorkItem.ToString());
                    }
                }
            }



            SaveToByteList(l, enDatabaseDataType.UndoCount, _UndoCount.ToString());

            if (Works2.Count > _UndoCount) { Works2.RemoveRange(0, Works2.Count - _UndoCount); }


            SaveToByteList(l, enDatabaseDataType.UndoInOne, Works2.JoinWithCr());

            SaveToByteList(l, enDatabaseDataType.EOF, "END");



            if (CryptPos > 0)
            {
                return modAllgemein.SimpleCrypt(l, _GlobalShowPass, 1, CryptPos, l.Count - 1);
            }
            return l;
        }

        /// <summary>
        /// Führt - falls nötig - einen Reload der Datenbank aus. Der Prozess wartet solange, bis der Reload erfolgreich war.
        /// </summary>
        public void Reload()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Reload()));
                return;
            }

            if (!ReloadNeeded()) { return; }

            if (_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, da gerade geparst wird"); }
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Reload unmöglich, Datenbankstatus eingefroren"); }

            //Der View-Code muss vom Table Selbst verwaltet werden. Jede Table/Formula kann ja eine eigene Ansicht haben!
            OnStoreView();
            LoadFromDisk();
            OnLoaded(new LoadedEventArgs(true));
            OnStoreReView();
        }

        private void OnStoreReView()
        {
            RestoreView?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnStoreView()
        {
            StoreView?.Invoke(this, System.EventArgs.Empty);
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


        private void OnSavedToDisk()
        {
            SavedToDisk?.Invoke(this, System.EventArgs.Empty);
        }
        private void OnSaveAborded()
        {
            SaveAborded?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnExporting(CancelEventArgs e)
        {
            Exporting?.Invoke(this, e);
        }

        internal void OnLoadingLinkedDatabase(DatabaseSettingsEventHandler e)
        {
            LoadingLinkedDatabase?.Invoke(this, e);
        }

        /// <summary>
        /// Das Cancel-Event wird nur berücksichtigt, wenn die Datenbank Readonly ist.
        /// Es kann ja sein, das Berechtigungen entzogen wurden, deswegen MUSS reloaded werden
        /// </summary>
        /// <param name="e"></param>
        private void OnReloading(CancelEventArgs e)
        {
            Reloading?.Invoke(this, e);
        }


        //private void OnColumnArrangementsChanged()
        //{
        //    ColumnArrangementsChanged?.Invoke(this, System.EventArgs.Empty);
        //}

        private void OnSortParameterChanged()
        {
            SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
        }


        public void OnConnectedControlsStopAllWorking(DatabaseStoppedEventArgs e)
        {
            if (e.AllreadyStopped.Contains(this)) { return; }
            e.AllreadyStopped.Add(this);
            ConnectedControlsStopAllWorking?.Invoke(this, e);
        }










        public void OnLoaded(LoadedEventArgs e)
        {
            Loaded?.Invoke(this, e);
        }





        public List<string> AllConnectedFilesLCase()
        {
            var Column_All = new List<string>();

            foreach (var ThisColumnItem in Column)
            {
                if (ThisColumnItem != null)
                {

                    if (ThisColumnItem.Format == enDataFormat.Link_To_Filesystem)
                    {
                        var tmp = ThisColumnItem.Contents(null);

                        foreach (var thisTmp in tmp)
                        {
                            Column_All.AddIfNotExists(ThisColumnItem.BestFile(thisTmp).ToLower());
                        }
                    }

                }


            }

            return Column_All.SortedDistinctList();
        }

        /// <summary>
        /// Angehängte Formulare werden aufgefordert, ihre Bearbeitung zu beenden. Geöffnete Benutzereingaben werden geschlossen.
        /// Ist die Datei in Bearbeitung wird diese freigegeben. Zu guter letzt werden PendingChanges fest gespeichert.
        /// Dadurch ist evtl. ein Reload nötig. Ein Reload wird nur bei Pending Changes ausgelöst!
        /// </summary>
        public bool Release(bool MUSTRelease)
        {
            if (ReadOnly) { return false; }

            if (Cell.Freezed)
            {
                if (!MUSTRelease) { return false; }
                Develop.DebugPrint(enFehlerArt.Fehler, "Release unmöglich, Datenbankstatus eingefroren");
            }
            OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs()); // Sonst meint der Benutzer evtl. noch, er könne Weiterarbeiten... Und Controlls haben die Möglichkeit, ihre Änderungen einzuchecken


            if (string.IsNullOrEmpty(Filename)) { return false; }


            var D = DateTime.Now; // Manchmal ist eine Block-Datei vorhanden, die just in dem Moment gelöscht wird. Also ein ganz kurze "Löschzeit" eingestehen.


            if (!MUSTRelease && BlockDateiVorhanden()) { return false; }

            while (HasPendingChanges())
            {
                if (!BinSaver.IsBusy) { BinSaver.RunWorkerAsync(); }
                Develop.DoEvents();
                if (DateTime.Now.Subtract(D).TotalSeconds > 30 && !BinSaver.IsBusy && HasPendingChanges() && BlockDateiVorhanden()) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank aufgrund der Blockdatei nicht freigegeben: " + Filename); }
            }
            return true;
        }



        #region  Undo 


        public string UndoText(string CellKey)
        {

            if (Works == null || Works.Count == 0) { return string.Empty; }

            var t = "";

            for (var z = Works.Count - 1; z >= 0; z--)
            {
                if (Works[z].CellKey == CellKey)
                {

                    if (Works[z].HistorischRelevant)
                    {
                        t = t + Works[z].UndoTextTableMouseOver() + "<br>";
                    }
                }

            }
            t = t.Trim("<br>");
            t = t.Trim("<hr>");
            t = t.Trim("<br>");
            t = t.Trim("<hr>");
            return t;
        }





        #endregion



        public string DefaultLayoutPath()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }

            return Filename.FilePath() + "Layouts\\";
        }


        //public void EditPrintLayout(string vAdditionalLayoutPath, string Layout = "")
        //{
        //    Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

        //    if (Backup.IsBusy && !Backup.CancellationPending) { Backup.CancelAsync(); }

        //    if (!IsSaveAble(true)) { return; }

        //    if (_EditorWindow) { return; }

        //    OnOpenEditor(new OpenEditorEventArgs("Layout", string.Empty, Layout, vAdditionalLayoutPath));
        //}



        private void InitializeComponent()
        {
            components = new Container();
            Checker = new System.Windows.Forms.Timer(components);
            BinReLoader = new BackgroundWorker();
            BinSaver = new BackgroundWorker();
            Backup = new BackgroundWorker();
            SuspendLayout();
            // 
            // Checker
            // 
            Checker.Interval = 1000;
            Checker.Tick += Checker_Tick;
            // 
            // BinReLoader
            // 
            BinReLoader.WorkerReportsProgress = true;
            BinReLoader.DoWork += BinReLoader_DoWork;
            BinReLoader.ProgressChanged += BinReLoader_ProgressChanged;
            // 
            // BinSaver
            // 
            BinSaver.WorkerReportsProgress = true;
            BinSaver.DoWork += BinSaver_DoWork;
            BinSaver.ProgressChanged += BinSaver_ProgressChanged;
            // 
            // Backup
            // 
            Backup.WorkerReportsProgress = true;
            Backup.WorkerSupportsCancellation = true;
            Backup.DoWork += Backup_DoWork;
            Backup.ProgressChanged += Backup_ProgressChanged;
            ResumeLayout(false);

        }


        private DateTime SavebleErrorReason_WindowsOnly_lastChecked = DateTime.Now.AddSeconds(-30);
        private int Checker_Tick_count;

        private void Checker_Tick(object sender, System.EventArgs e)
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            if (BinReLoader.IsBusy) { return; }
            if (BinSaver.IsBusy) { return; }
            if (string.IsNullOrEmpty(Filename)) { return; }


            // Ausstehende Arbeiten ermittelen
            var _MustReload = ReloadNeeded();
            var _MustSave = HasPendingChanges();
            var _MustBackup = _MustSave;
            foreach (var ThisExport in _Export)
            {
                if (ThisExport != null)
                {
                    if (ThisExport.Typ == enExportTyp.EinzelnMitFormular) { _MustBackup = true; }
                    if (ThisExport.LastExportTime.Subtract(DateTime.Now).TotalDays > 10) { _MustBackup = true; }
                }
            }

            Checker_Tick_count += 1;
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



            if (DateTime.Now.Subtract(_UserEditedAktion).TotalSeconds < Count_UserWork) { return; } // Benutzer arbeiten lassen
            if (Checker_Tick_count > Count_Save && _MustSave && Backup.IsBusy && !Backup.CancellationPending) { Backup.CancelAsync(); }
            if (Checker_Tick_count > _ReloadDelaySecond && Backup.IsBusy && !Backup.CancellationPending && _MustReload) { Backup.CancelAsync(); }
            if (Backup.IsBusy) { return; }



            if (_MustBackup && !_MustReload && Checker_Tick_count < Count_Save && Checker_Tick_count >= Count_BackUp && IsSaveAble())
            {
                Backup.RunWorkerAsync();
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




        public void AddPending(enDatabaseDataType Comand, ColumnItem column, string PreviousValue, string ChangedTo, bool ExecuteNow)
        {
            AddPending(Comand, column.Key, -1, PreviousValue, ChangedTo, ExecuteNow, false);
        }

        public void AddPending(enDatabaseDataType Comand, int ColumnKey, string ListExt, bool ExecuteNow)
        {
            AddPending(Comand, ColumnKey, -1, "", ListExt, ExecuteNow, false);
        }

        public void AddPending(enDatabaseDataType Comand, int ColumnKey, int RowKey, string PreviousValue, string ChangedTo, bool ExecuteNow)
        {
            AddPending(Comand, ColumnKey, RowKey, PreviousValue, ChangedTo, ExecuteNow, false);
        }


        public void AddPending(enDatabaseDataType Comand, int ColumnKey, int RowKey, string PreviousValue, string ChangedTo, bool ExecuteNow, bool FreezedMode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddPending(Comand, ColumnKey, RowKey, PreviousValue, ChangedTo, ExecuteNow, FreezedMode)));
                return;
            }


            if (Cell.Freezed != FreezedMode)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "FreezeMode-Inkonsitent: " + Comand);
                return;
            }


            if (Cell.Freezed)
            {
                if (Comand != enDatabaseDataType.ce_Value_withoutSizeData)
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "FreezeCommand = " + Comand.ToString());
                    return;
                }
                if (!ExecuteNow)
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Nicht ExecuteNow, FreezeCommand = " + Comand.ToString());
                    return;
                }
                Cell.AddFreeze(ColumnKey, RowKey, PreviousValue);
            }






            if (ExecuteNow)
            {
                ParseThis(Comand, ChangedTo, Column.SearchByKey(ColumnKey), Row.SearchByKey(RowKey), -1, -1);
            }

            if (_isParsing) { return; }
            if (ReadOnly)
            {
                if (!string.IsNullOrEmpty(Filename))
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Datei ist Readonly, " + Comand + ", " + Filename);
                }
                return;
            }





            // Keine Doppelten Rausfiltern, ansonstn stimmen die Undo nicht mehr

            _UserEditedAktion = DateTime.Now;

            if (RowKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
            if (ColumnKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }

            Works.Add(new WorkItem(Comand, ColumnKey, RowKey, PreviousValue, ChangedTo, UserName, UserGroup, FreezedMode));


        }


        private void ExecutePending()
        {
            if (!_isParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Nur während des Parsens möglich"); }
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank eingefroren!"); }
            if (!HasPendingChanges()) { return; }


            // Erst die Neuen Zeilen / Spalten alle neutralisieren
            var dummy = -1000;
            foreach (var ThisPending in Works)
            {

                if (ThisPending.State == enItemState.Pending)
                {
                    if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow)
                    {
                        dummy -= 1;
                        ChangeRowKeyInPending(ThisPending.RowKey, dummy);
                    }
                    if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddColumn)
                    {
                        dummy -= 1;
                        ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
                    }
                }
            }

            // Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
            foreach (var ThisPending in Works)
            {

                if (ThisPending.State == enItemState.Pending)
                {
                    switch (ThisPending.Comand)
                    {
                        case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen:
                            {
                                var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
                                var fRow = Row[Value];

                                if (!string.IsNullOrEmpty(Value) && fRow != null)
                                {
                                    ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
                                }
                                else
                                {
                                    ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
                                }

                                break;
                            }
                        case enDatabaseDataType.dummyComand_AddRow:
                            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
                            break;
                        case enDatabaseDataType.dummyComand_AddColumn:
                            ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey());
                            break;
                    }
                }
            }


            // Und nun alles ausführen!
            foreach (var ThisPending in Works)
            {
                if (ThisPending.State == enItemState.Pending)
                {

                    if (ThisPending.Comand == enDatabaseDataType.co_Name)
                    {
                        ThisPending.ChangedTo = Column.Freename(ThisPending.ChangedTo);
                    }
                    ExecutePending(ThisPending);
                }
            }
        }

        private void ExecutePending(WorkItem ThisPendingItem)
        {
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank eingefroren!"); }
            if (ThisPendingItem.State == enItemState.Pending)
            {

                ColumnItem _Col = null;
                RowItem _Row = null;

                if (ThisPendingItem.RowKey > -1)
                {
                    _Row = Row.SearchByKey(ThisPendingItem.RowKey);
                    if (_Row == null)
                    {
                        if (ThisPendingItem.Comand != enDatabaseDataType.dummyComand_AddRow)
                        {
                            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }

                    }
                }

                _Col = null;
                if (ThisPendingItem.ColKey > -1)
                {
                    _Col = Column.SearchByKey(ThisPendingItem.ColKey);
                    if (_Col == null)
                    {
                        if (ThisPendingItem.Comand != enDatabaseDataType.dummyComand_AddColumn)
                        {
                            Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ParseThis(ThisPendingItem.Comand, ThisPendingItem.ChangedTo, _Col, _Row, 0, 0);
            }
        }


        private string SearchKeyValueInPendingsOf(int RowKey)
        {
            var F = string.Empty;
            foreach (var ThisPending in Works)
            {

                if (ThisPending.State == enItemState.Pending)
                {
                    if (ThisPending.RowKey == RowKey && ThisPending.Comand == enDatabaseDataType.ce_Value_withoutSizeData && ThisPending.ColKey == Column[0].Key)
                    {
                        F = ThisPending.ChangedTo;
                    }
                }
            }
            return F;
        }



        private void ChangeRowKeyInPending(int OldKey, int NewKey)
        {
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank eingefroren!"); }

            foreach (var ThisPending in Works)
            {

                if (ThisPending.State == enItemState.Pending)
                {

                    if (ThisPending.RowKey == OldKey)
                    {
                        ThisPending.RowKey = NewKey;

                        switch (ThisPending.Comand)
                        {
                            case enDatabaseDataType.dummyComand_AddRow:
                            case enDatabaseDataType.dummyComand_RemoveRow:
                                ThisPending.ChangedTo = NewKey.ToString();
                                break;
                            default:
                                if (ThisPending.PreviousValue.Contains("RowKey=" + OldKey)) { Develop.DebugPrint("Replace machen (Old): " + OldKey); }
                                if (ThisPending.ChangedTo.Contains("RowKey=" + OldKey)) { Develop.DebugPrint("Replace machen (New): " + OldKey); }
                                break;
                        }

                    }

                }

                OnRowKeyChanged(new KeyChangedEventArgs(OldKey, NewKey));


            }
        }

        private void OnRowKeyChanged(KeyChangedEventArgs e)
        {
            RowKeyChanged?.Invoke(this, e);
        }



        private void ChangeColumnKeyInPending(int OldKey, int NewKey)
        {
            if (Cell.Freezed) { Develop.DebugPrint(enFehlerArt.Fehler, "Datenbank eingefroren!"); }

            foreach (var ThisPending in Works)
            {
                if (ThisPending.State == enItemState.Pending)
                {

                    if (ThisPending.ColKey == OldKey)
                    {
                        ThisPending.ColKey = NewKey;

                        switch (ThisPending.Comand)
                        {
                            case enDatabaseDataType.dummyComand_AddColumn:
                            case enDatabaseDataType.dummyComand_RemoveColumn:
                                ThisPending.ChangedTo = NewKey.ToString();
                                break;
                            default:
                                if (ThisPending.PreviousValue.Contains(ColumnCollection.ParsableColumnKey(OldKey)))
                                {
                                    Develop.DebugPrint("Replace machen (Old): " + OldKey);
                                }
                                if (ThisPending.ChangedTo.Contains(ColumnCollection.ParsableColumnKey(OldKey)))
                                {
                                    Develop.DebugPrint("Replace machen (New): " + OldKey);
                                }
                                break;
                        }

                    }

                }

                OnColumnKeyChanged(new KeyChangedEventArgs(OldKey, NewKey));

            }
        }

        private void OnColumnKeyChanged(KeyChangedEventArgs e)
        {
            ColumnKeyChanged?.Invoke(this, e);
        }




        internal void ChangeWorkItems(enItemState OldState, enItemState NewState)
        {

            foreach (var ThisWork in Works)
            {
                if (ThisWork.State == OldState) { ThisWork.State = NewState; }
            }
        }




        private void InvalidateExports(int LayoutNo)
        {
            if (ReadOnly) { return; }


            var Done = false;

            foreach (var ThisExport in _Export)
            {
                if (ThisExport != null)
                {
                    if (ThisExport.Typ == enExportTyp.EinzelnMitFormular)
                    {
                        if (int.TryParse(ThisExport.ExportFormular, out var r))
                        {
                            if (r == LayoutNo)
                            {
                                Done = true;
                                ThisExport.LastExportTime = new DateTime(1900, 1, 1);
                            }
                        }
                    }
                }
            }

            if (Done)
            {
                AddPending(enDatabaseDataType.AutoExport, -1, _Export.ToString(true), false);
            }
        }





        public string PreviewsFile()
        {

            var L = new List<string>();

            foreach (var ThisExport in _Export)
            {

                if (ThisExport.Typ == enExportTyp.DatenbankOriginalFormat)
                {
                    L.AddRange(ThisExport.BereitsExportiert);
                }
            }

            L = L.SortedDistinctList();

            if (L.Count == 0) { return string.Empty; }


            var Neues = string.Empty;
            var NeuestDate = DateTime.Now.AddDays(-10000);



            foreach (var ThisString in L)
            {

                if (ThisString.Contains("|"))
                {
                    var x = ThisString.SplitBy("|");
                    if (x.GetUpperBound(0) == 1)
                    {


                        var d = DateTimeParse(x[1]).Subtract(NeuestDate);

                        if (d.TotalDays > 0)
                        {

                            if (FileExists(x[0]))
                            {
                                Neues = x[0];
                                NeuestDate = DateTimeParse(x[1]);
                            }
                        }
                    }
                }
            }

            return Neues;

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

            // BlockDatei erstellen
            try
            {
                if (FileExists(tBackup))
                {
                    RenameFile(tBackup, BlockDatei, true);
                }
                else
                {
                    CopyFile(Filename, BlockDatei, true);
                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
                return;
            }

            if (!BlockDateiVorhanden()) { Develop.DebugPrint("Block-Datei Konflikt 1"); }

            // Im Parallelen-Process, Reload-Needed ist auch ein Dateizugriff
            if (ReloadNeeded()) { BinaryWriter_ReportProgressAndWait(5, "Reload"); }

            BinaryWriter_ReportProgressAndWait(10, "GetBinData");

            //OK, nun gehts rund: Haupt-Datei wird zum Backup kopiert.
            CopyFile(Filename, tBackup, true);

            // Und hier wird nun die neue Datei als TMP erzeugt erzeugt.
            var count = 0;
            var TMP = TempFile(Filename + "-" + UserName);

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

            // Und nun den Block entfernen
            CanWrite(Filename, 30); // sobald die Hauptdatei wieder frei ist
            DeleteFile(BlockDatei, true);




            BinaryWriter_ReportProgressAndWait(15, "GetFileState");
            BinaryWriter_ReportProgressAndWait(20, "ChangePendingToUndo");
            BinaryWriter_ReportProgressAndWait(25, "GetLoadedFiles");

            if (Writer_FilesToDeleteLCase.Count > 0)
            {
                if (_VerwaisteDaten == enVerwaisteDaten.Löschen) { DeleteFile(Writer_FilesToDeleteLCase); }
            }
            OnSavedToDisk();
        }

        /// <summary>
        /// Diese Routine ist ein Paraleller Process.
        /// Er prüft, ob Daten Reloaded werden müssen. Falls KEINE Daten da sind, werden sie geladen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BinReLoader_DoWork(object sender, DoWorkEventArgs e)
        {

            var ec = new CancelEventArgs(false);
            OnReloading(ec);
            if (ReadOnly && ec.Cancel) { return; }


            if (!ReloadNeeded()) { return; }

            FileInfo f;
            byte[] _tmp = null;

            var StartTime = DateTime.Now;
            do
            {
                try
                {
                    if (string.IsNullOrEmpty(Filename))
                    {
                        Develop.DebugPrint(enFehlerArt.Warnung, "Dateiname ist leer: " + Caption);
                        return;
                    }

                    ////if (!BlockDateiVorhanden())
                    //{
                    _tmp = File.ReadAllBytes(Filename);
                    f = new FileInfo(Filename);

                    Pause(0.5, false);

                    if (new FileInfo(Filename).Length == _tmp.Length) { break; }
                    //}
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

            BinReLoader.ReportProgress(0, _BLoaded); // 0 = Parsen
            BinReLoader.ReportProgress(100, f.LastWriteTime); // 100 = Abschluß
        }

        private void Backup_DoWork(object sender, DoWorkEventArgs e)
        {
            //if (!IsSaveAble(false)) { return; }
            if (ReadOnly) { return; }

            var ec = new CancelEventArgs(false);
            OnExporting(ec);
            if (ec.Cancel) { return; }

            var ReportAChange = false;
            var tmp = false;

            try
            {

                if (!Backup.CancellationPending)
                {

                    foreach (var ThisExport in _Export)
                    {
                        if (ThisExport != null) { tmp = ThisExport.DeleteOutdatedBackUps(Backup); }
                        if (tmp) { ReportAChange = true; }
                        if (Backup.CancellationPending) { break; }

                    }

                }

                if (!Backup.CancellationPending)
                {
                    foreach (var ThisExport in _Export)
                    {
                        if (ThisExport != null) { tmp = ThisExport.DoBackUp(Backup, _GenerateLayout); }
                        if (tmp) { ReportAChange = true; }
                    }

                }


            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }


            if (ReportAChange)
            {
                Backup.ReportProgress(100, "AddPending");
            }
        }

        private void BinSaver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            switch ((string)e.UserState)
            {

                case "ResetProcess":
                    break;

                case "Reload":
                    if (BinReLoader.IsBusy) { return; }
                    BinReLoader.RunWorkerAsync();
                    break;

                case "GetBinData":
                    Writer_BinaryData = ToListOfByte();
                    break;

                case "GetFileState":
                    _LoadedFileDate = new FileInfo(Filename).LastWriteTime;
                    _CheckedAndReloadNeed = false;
                    break;

                case "ChangePendingToUndo":
                    ChangeWorkItems(enItemState.Pending, enItemState.Undo);
                    break;


                case "GetLoadedFiles":
                    var FilesNewLCase = AllConnectedFilesLCase();
                    FilesAfterLoadingLCase.Remove(FilesNewLCase);

                    // Hier erst reintun, dass der Worker nicht zu früh reagiert!
                    Writer_FilesToDeleteLCase = new List<string>();
                    Writer_FilesToDeleteLCase.AddRange(FilesAfterLoadingLCase);

                    FilesAfterLoadingLCase.Clear();
                    FilesAfterLoadingLCase.AddRange(FilesNewLCase);
                    break;


                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;

            }


            Writer_ProcessDone = (string)e.UserState;



        }

        private void BinReLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    Parse((List<byte>)e.UserState);
                    break;

                case 100:
                    _CheckedAndReloadNeed = false;
                    _LoadedFileDate = (DateTime)e.UserState; // initialize setzt zurück
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;

            }

        }

        private void Backup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((string)e.UserState)
            {
                case "AddPending":
                    AddPending(enDatabaseDataType.AutoExport, -1, _Export.ToString(true), false);
                    break;

                default:
                    Develop.DebugPrint("Unbekannter Befehl:" + (string)e.UserState);
                    break;

            }
        }


        public bool AllRulesOK()
        {
            return AllRulesOK(Rules);
        }


        public static bool AllRulesOK(ListExt<RuleItem> RulesToCheck)
        {
            foreach (var thisRule in RulesToCheck)
            {
                if (thisRule != null && !thisRule.IsOk()) { return false; }
            }
            return true;
        }

    }
}
