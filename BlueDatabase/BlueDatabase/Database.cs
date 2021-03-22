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


using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.FileOperations;


namespace BlueDatabase {
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Database : BlueBasics.MultiUserFile.clsMultiUserFile {
        #region  Shareds 

        public static readonly string DatabaseVersion = "3.50";


        public static Database LoadResource(Assembly assembly, string Name, string BlueBasicsSubDir, bool FehlerAusgeben, bool MustBeStream) {

            if (Develop.IsHostRunning() && !MustBeStream) {

                int x = -1;
                string pf;

                do {
                    x++;
                    pf = string.Empty;
                    switch (x) {
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

                    if (FileExists(pf)) {
                        BlueBasics.MultiUserFile.clsMultiUserFile tmp = GetByFilename(pf, false);
                        if (tmp != null) { return (Database)tmp; }
                        tmp = new Database(pf, false, false);
                        return (Database)tmp;
                    }

                } while (pf != string.Empty);
            }

            Stream d = modAllgemein.GetEmmbedResource(assembly, Name);
            if (d != null) { return new Database(d); }

            if (FehlerAusgeben) { Develop.DebugPrint(enFehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + BlueBasicsSubDir + " - " + Name); }


            return null;
        }


        #endregion


        #region  Variablen-Deklarationen 

        //private IContainer components;

        public readonly ColumnCollection Column;
        public readonly CellCollection Cell;
        public readonly RowCollection Row;
        public ListExt<WorkItem> Works;

        private readonly long _startTick = DateTime.UtcNow.Ticks;

        private readonly List<string> FilesAfterLoadingLCase;

        private string _Creator;
        private string _CreateDate;
        private int _UndoCount;

        private string _GlobalShowPass;
        private string _FileEncryptionKey;

        public string LoadedVersion { get; private set; }
        private string _Caption;
        private enJoinTyp _JoinTyp;
        private enVerwaisteDaten _VerwaisteDaten;
        private string _ImportScript;
        private string _RulesScript;
        private enAnsicht _Ansicht;
        private double _GlobalScale;
        private string _FilterImagePfad;
        private string _ZeilenQuickInfo;


        public readonly ListExt<ColumnViewCollection> ColumnArrangements = new ListExt<ColumnViewCollection>();
        public readonly ListExt<ColumnViewCollection> Views = new ListExt<ColumnViewCollection>();
        public readonly ListExt<string> Tags = new ListExt<string>();

        /// <summary>
        /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
        /// Der Head-Editor kann und muss (manuelles L�schen) auf die Exporte Zugreifen und k�mmert sich auch um die Pendings
        /// </summary>
        public readonly ListExt<ExportDefinition> Export = new ListExt<ExportDefinition>();

        public readonly ListExt<clsNamedBinary> Bins = new ListExt<clsNamedBinary>();
        public readonly ListExt<string> DatenbankAdmin = new ListExt<string>();
        public readonly ListExt<string> PermissionGroups_NewRow = new ListExt<string>();
        public readonly ListExt<string> Layouts = new ListExt<string>(); // Print Views werden nicht immer ben�tigt. Deswegen werden sie als String gespeichert. Der Richtige Typ w�re CreativePad

        public string UserGroup = "#Administrator";
        public readonly string UserName = modAllgemein.UserName().ToUpper();


        private RowSortDefinition _sortDefinition;


        /// <summary>
        /// Variable nur tempor�r f�r den BinReloader, um m�gliche Datenverluste zu entdecken.
        /// </summary>
        private string _LastWorkItem = string.Empty;
        private string WVorher = string.Empty;

        #endregion

        #region  Event-Deklarationen 
        public event EventHandler SortParameterChanged;
        public event EventHandler ViewChanged;

        public event CancelEventHandler Exporting;
        public event EventHandler<DatabaseSettingsEventHandler> LoadingLinkedDatabase;

        public event EventHandler<KeyChangedEventArgs> RowKeyChanged;
        public event EventHandler<KeyChangedEventArgs> ColumnKeyChanged;
        public event EventHandler<ProgressbarEventArgs> ProgressbarInfo;

        public event EventHandler<PasswordEventArgs> NeedPassword;
        public event EventHandler<RenameColumnInLayoutEventArgs> RenameColumnInLayout;
        public event EventHandler<GenerateLayoutInternalEventargs> GenerateLayoutInternal;
        #endregion


        #region  Construktor + Initialize 

        public Database(Stream Stream) : this(Stream, string.Empty, true, false) { }

        public Database(bool readOnly) : this(null, string.Empty, readOnly, true) { }

        public Database(string filename, bool readOnly, bool create) : this(null, filename, readOnly, create) { }


        private Database(Stream stream, string filename, bool readOnly, bool create) : base(readOnly, true) {



            CultureInfo culture = new System.Globalization.CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Cell = new CellCollection(this);
            Column = new ColumnCollection(this);
            Row = new RowCollection(this);

            Works = new ListExt<WorkItem>();

            FilesAfterLoadingLCase = new List<string>();

            ColumnArrangements.Changed += ColumnArrangements_ListOrItemChanged;
            Layouts.Changed += Layouts_ListOrItemChanged;
            Layouts.ItemSeted += Layouts_ItemSeted;

            Views.Changed += Views_ListOrItemChanged;
            PermissionGroups_NewRow.Changed += PermissionGroups_NewRow_ListOrItemChanged;
            Tags.Changed += DatabaseTags_ListOrItemChanged;
            Bins.Changed += Bins_ListOrItemChanged;
            Export.Changed += Export_ListOrItemChanged;
            DatenbankAdmin.Changed += DatabaseAdmin_ListOrItemChanged;



            Row.RowRemoving += Row_BeforeRemoveRow;
            Row.RowAdded += Row_RowAdded;

            //Column.ItemAdded += Column_ItemAdded;
            Column.ItemRemoving += Column_ItemRemoving;
            Column.ItemRemoved += Column_ItemRemoved;


            //  _isParsing = true;
            Initialize();
            // _isParsing = false;


            UserGroup = "#Administrator";

            if (!string.IsNullOrEmpty(filename)) {
                Load(filename, create);
            } else if (stream != null) {
                LoadFromStream(stream);
            }
        }

        private void Initialize() {

            Cell.Initialize();

            Column.Initialize();

            Row.Initialize();

            Works.Clear();

            ColumnArrangements.Clear();

            Layouts.Clear();
            Views.Clear();


            PermissionGroups_NewRow.Clear();
            Tags.Clear();
            Export.Clear();

            Bins.Clear();

            DatenbankAdmin.Clear();


            _GlobalShowPass = string.Empty;
            _FileEncryptionKey = string.Empty;


            _Creator = UserName;
            _CreateDate = DateTime.Now.ToString(Constants.Format_Date5);

            _ReloadDelaySecond = 600;
            _UndoCount = 300;

            _Caption = string.Empty;
            _JoinTyp = enJoinTyp.Zeilen_verdoppeln;
            _VerwaisteDaten = enVerwaisteDaten.Ignorieren;
            LoadedVersion = DatabaseVersion;
            _ImportScript = string.Empty;
            _RulesScript = string.Empty;

            _GlobalScale = 1f;
            _Ansicht = enAnsicht.Unver�ndert;
            _FilterImagePfad = string.Empty;
            _ZeilenQuickInfo = string.Empty;

            _sortDefinition = null;
        }

        #endregion

        #region  Properties 

        [Browsable(false)]
        public string Caption {
            get {
                return _Caption;
            }
            set {
                if (_Caption == value) { return; }
                AddPending(enDatabaseDataType.Caption, -1, -1, _Caption, value, true);
            }
        }



        [Browsable(false)]
        public double GlobalScale {
            get {
                return _GlobalScale;
            }
            set {
                if (_GlobalScale == value) { return; }
                AddPending(enDatabaseDataType.GlobalScale, -1, -1, _GlobalScale.ToString(), value.ToString(), true);
                Cell.InvalidateAllSizes();
            }
        }

        [Browsable(false)]
        public string FilterImagePfad {
            get {
                return _FilterImagePfad;
            }
            set {
                if (_FilterImagePfad == value) { return; }
                AddPending(enDatabaseDataType.FilterImagePfad, -1, -1, _FilterImagePfad, value, true);
                Cell.InvalidateAllSizes();
            }
        }

        [Browsable(false)]
        public string ZeilenQuickInfo {
            get {
                return _ZeilenQuickInfo;
            }
            set {
                if (_ZeilenQuickInfo == value) { return; }
                AddPending(enDatabaseDataType.ZeilenQuickInfo, -1, -1, _ZeilenQuickInfo, value, true);
            }
        }

        [Browsable(false)]
        public enAnsicht Ansicht {
            get {
                return _Ansicht;
            }
            set {
                if (_Ansicht == value) { return; }
                AddPending(enDatabaseDataType.Ansicht, -1, -1, ((int)_Ansicht).ToString(), ((int)value).ToString(), true);
            }
        }


        [Browsable(false)]
        public RowSortDefinition SortDefinition {
            get {
                return _sortDefinition;
            }
            set {
                string Alt = string.Empty;
                string Neu = string.Empty;

                if (_sortDefinition != null) { Alt = _sortDefinition.ToString(); }
                if (value != null) { Neu = value.ToString(); }

                if (Alt == Neu) { return; }
                AddPending(enDatabaseDataType.SortDefinition, -1, -1, Alt, Neu, false);
                _sortDefinition = new RowSortDefinition(this, Neu);
                OnSortParameterChanged();

            }
        }


        [Browsable(false)]
        public string Creator {
            get {
                return _Creator.Trim();
            }
            set {
                if (_Creator == value) { return; }
                AddPending(enDatabaseDataType.Creator, -1, -1, _Creator, value, true);
            }
        }

        [Browsable(false)]
        public int UndoCount {
            get {
                return _UndoCount;
            }
            set {
                if (_UndoCount == value) { return; }
                AddPending(enDatabaseDataType.UndoCount, -1, -1, _UndoCount.ToString(), value.ToString(), true);
            }
        }

        [Browsable(false)]
        public string CreateDate {
            get {
                return _CreateDate;
            }
            set {
                if (_CreateDate == value) { return; }
                AddPending(enDatabaseDataType.CreateDate, -1, -1, _CreateDate, value, true);
            }
        }

        [Browsable(false)]
        public int ReloadDelaySecond {
            get {
                return _ReloadDelaySecond;
            }
            set {
                if (_ReloadDelaySecond == value) { return; }
                AddPending(enDatabaseDataType.ReloadDelaySecond, -1, -1, _ReloadDelaySecond.ToString(), value.ToString(), true);
            }
        }

        public string GlobalShowPass {
            get {
                return _GlobalShowPass;
            }
            set {
                if (_GlobalShowPass == value) { return; }
                AddPending(enDatabaseDataType.GlobalShowPass, -1, -1, _GlobalShowPass, value, true);
            }
        }


        public enJoinTyp JoinTyp {
            get {
                return _JoinTyp;
            }
            set {
                if (_JoinTyp == value) { return; }
                AddPending(enDatabaseDataType.JoinTyp, -1, -1, ((int)_JoinTyp).ToString(), ((int)value).ToString(), true);
            }
        }

        public enVerwaisteDaten VerwaisteDaten {
            get {
                return _VerwaisteDaten;
            }
            set {
                if (_VerwaisteDaten == value) { return; }
                AddPending(enDatabaseDataType.VerwaisteDaten, -1, -1, ((int)_VerwaisteDaten).ToString(), ((int)value).ToString(), true);
            }
        }

        public string ImportScript {
            get {
                return _ImportScript;
            }
            set {
                if (_ImportScript == value) { return; }
                AddPending(enDatabaseDataType.ImportScript, -1, -1, _ImportScript, value, true);
            }
        }

        public string RulesScript {
            get {
                return _RulesScript;
            }
            set {
                if (_RulesScript == value) { return; }
                AddPending(enDatabaseDataType.RulesScript, -1, -1, _RulesScript, value, true);
            }
        }

        public string FileEncryptionKey {
            get {
                return _FileEncryptionKey;
            }
            set {
                if (_FileEncryptionKey == value) { return; }
                AddPending(enDatabaseDataType.FileEncryptionKey, -1, -1, _FileEncryptionKey, value, true);
            }
        }



        #endregion



        internal void DevelopWarnung(string t) {

            try {
                t += "\r\nParsing: " + IsParsing.ToString();
                t += "\r\nLoading: " + IsLoading.ToString();
                t += "\r\nSaving: " + IsSaving.ToString();
                t += "\r\nColumn-Count: " + Column.Count.ToString();
                t += "\r\nRow-Count: " + Row.Count.ToString();
                t += "\r\nFile: " + Filename;
            } catch { }

            Develop.DebugPrint(enFehlerArt.Warnung, t);

        }

        public int LayoutIDToIndex(string exportFormularID) {

            for (int z = 0; z < Layouts.Count; z++) {
                if (Layouts[z].Contains("ID=" + exportFormularID + ",")) { return z; }
            }

            return -1;
        }

        public string DoImportScript(string TextToImport, RowItem row, bool MeldeFehlgeschlageneZeilen) {
            if (string.IsNullOrEmpty(_ImportScript)) { return "Kein Import-Skript vorhanden."; }
            if (string.IsNullOrEmpty(TextToImport)) { return "Kein Text zum Importieren angegeben."; }


            List<string> cmds = _ImportScript.FromNonCritical().SplitByCRToList();

            //if (row == null)
            //{
            //    row = Row.Add(DateTime.Now.ToString(Constants.Format_Date));
            //}
            //else
            //{
            if (row != null && row.CellGetBoolean(row.Database.Column.SysLocked)) { return "Die Zeile ist gesperrt (abgeschlossen)."; }
            //}



            foreach (string thiscmd in cmds) {
                (string fehlertext, bool bigfailure, RowItem importrow) = DoImportScript(TextToImport, thiscmd.Replace(";cr;", "\r").Replace(";tab;", "\t").SplitBy("|"), row);

                if (importrow == null) { return "Es konnte keine neue Zeile erzeugt werden."; }

                if (row == null) {
                    row = importrow;
                } else {
                    if (row != importrow) { return "Zeilen-Inkonsistenz festgestellt."; }
                }

                if (!string.IsNullOrEmpty(fehlertext)) {
                    if (bigfailure) { return fehlertext + "<br><br>Zeile:<br>" + thiscmd; }
                    if (MeldeFehlgeschlageneZeilen) { return fehlertext + "<br><br>Zeile:<br>" + thiscmd; }
                }
            }
            return string.Empty;
        }



        internal void OnProgressbarInfo(ProgressbarEventArgs e) {
            ProgressbarInfo?.Invoke(this, e);
        }



        private (string fehlertext, bool bigfailure, RowItem importrow) DoImportScript(string textToImport, string[] cmd, RowItem row) {

            if (cmd == null) { return ("Kein Befehl �bergeben", true, row); }
            if (cmd.GetUpperBound(0) != 3) { return ("Format muss 4 | haben.", true, row); }

            ColumnItem c = Column[cmd[1]];
            if (c == null) { return ("Spalte nicht in der Datenbank gefunden.", true, row); }

            if (string.IsNullOrEmpty(cmd[2])) { return ("Suchtext 'vorher' ist nicht angegeben", true, row); }
            if (string.IsNullOrEmpty(cmd[3])) { return ("Suchtext 'nachher' ist nicht angegeben", true, row); }

            int vh = textToImport.IndexOf(cmd[2]);
            if (vh < 0) { return ("Suchtext 'vorher' im Text nicht vorhanden.", false, row); }

            int nh = textToImport.IndexOf(cmd[3], vh + cmd[2].Length);
            if (nh < 0) { return ("Suchtext 'nachher' im Text nicht vorhanden.", false, row); }

            string txt = textToImport.Substring(vh + cmd[2].Length, nh - vh - cmd[2].Length);

            switch (cmd[0].ToUpper()) {
                case "IMPORT1":
                    if (c.IsFirst() && row == null) {
                        row = Row.Add(txt);
                    } else {
                        if (row == null) { return ("Keine Zeile angegeben.", true, row); }
                        row.CellSet(c, txt);
                    }
                    return (string.Empty, false, row);

                case "IMPORT2":
                    if (row == null) { return ("Keine Zeile angegeben.", true, row); }
                    List<string> l = row.CellGetList(c);
                    l.Add(txt);
                    row.CellSet(c, l);
                    return (string.Empty, false, row);

                default:
                    return ("Befehl nicht erkannt.", true, row);
            }

        }

        private void Column_ItemRemoved(object sender, System.EventArgs e) {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Parsing Falsch!"); }
            CheckViewsAndArrangements();
        }

        private void Column_ItemRemoving(object sender, ListEventArgs e) {
            int Key = ((ColumnItem)e.Item).Key;
            AddPending(enDatabaseDataType.dummyComand_RemoveColumn, Key, -1, string.Empty, Key.ToString(), false);

        }

        private void Row_RowAdded(object sender, RowEventArgs e) {
            if (!IsParsing) {
                AddPending(enDatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
            }
        }

        private void Row_BeforeRemoveRow(object sender, RowEventArgs e) {
            AddPending(enDatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
        }

        private void Layouts_ItemSeted(object sender, ListEventArgs e) {
            if (e != null) {
                string x = (string)e.Item;
                if (!x.StartsWith("{ID=#")) { Develop.DebugPrint("ID nicht gefunden: " + x); }
                int ko = x.IndexOf(", ");

                string id = x.Substring(4, ko - 4);

                InvalidateExports(id);
            }
        }

        private void CheckViewsAndArrangements() {

            foreach (ColumnViewCollection ThisCol in ColumnArrangements) {
                ThisCol.Repair();
            }

            foreach (ColumnViewCollection ThisCol in Views) {
                ThisCol.Repair();
            }


            if (Views != null) {
                if (Views.Count > 0 && Views[0].PermissionGroups_Show.Count > 0) { Views[0].PermissionGroups_Show.Clear(); }
                if (Views.Count > 1 && !Views[1].PermissionGroups_Show.Contains("#Everybody")) { Views[1].PermissionGroups_Show.Add("#Everybody"); }
            }

            for (int z = 0; z < Layouts.Count; z++) {
                if (!Layouts[z].StartsWith("{ID=#")) {
                    Layouts[z] = "{ID=#Converted" + z.ToString() + ", " + Layouts[z].Substring(1);
                }
            }
        }

        private void DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
        }

        private void DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.DatenbankAdmin, -1, DatenbankAdmin.JoinWithCr(), false);
        }

        private void PermissionGroups_NewRow_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.PermissionGroups_NewRow, -1, PermissionGroups_NewRow.JoinWithCr(), false);
        }

        private void Layouts_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
        }

        private void Bins_ListOrItemChanged(object sender, System.EventArgs e) {

            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.BinaryDataInOne, -1, Bins.ToString(true), false);
        }

        private void Export_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.AutoExport, -1, Export.ToString(true), false);
        }

        //private void Rules_ListOrItemChanged(object sender, System.EventArgs e) {

        //    if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.

        //    if (sender == Rules) {
        //        AddPending(enDatabaseDataType.Rules, -1, Rules.ToString(true), false);
        //        return;
        //    }

        //    if (sender is RuleItem RL) {
        //        if (!Rules.Contains(RL)) { return; }
        //        AddPending(enDatabaseDataType.Rules, -1, Rules.ToString(true), false);
        //    }
        //    else {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Typ ist kein RuleItem");
        //    }
        //}

        private void Views_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt werden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.Views, -1, Views.ToString(true), false);
        }

        private void ColumnArrangements_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt werden. Kann zu Endlosschleifen f�hren.
            AddPending(enDatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(), false);
        }

        public void Parse(List<byte> _BLoaded, ref int Pointer, ref enDatabaseDataType Art, ref int ColNR, ref int RowNR, ref string Wert, ref int X, ref int Y) {

            int Les;

            switch ((enRoutinen)_BLoaded[Pointer]) {
                case enRoutinen.CellFormat_OLD: {
                    Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                    Les = NummerCode3(_BLoaded, Pointer + 2);
                    ColNR = NummerCode3(_BLoaded, Pointer + 5);
                    RowNR = NummerCode3(_BLoaded, Pointer + 8);

                    //  Wert = String.
                    byte[] b = new byte[Les];
                    _BLoaded.CopyTo(Pointer + 11, b, 0, Les);
                    Wert = Encoding.Default.GetString(b);

                    //     Wert = System.Text.Encoding.Default.GetString(BLoaded, Pointer + 11, Les)
                    X = NummerCode2(_BLoaded, Pointer + 11 + Les);
                    Y = NummerCode2(_BLoaded, Pointer + 11 + Les + 2);
                    Pointer += 11 + Les + 4;


                    break;
                }
                case enRoutinen.DatenAllgemein: {
                    Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                    Les = NummerCode3(_BLoaded, Pointer + 2);
                    ColNR = -1;
                    RowNR = -1;
                    byte[] b = new byte[Les];
                    _BLoaded.CopyTo(Pointer + 5, b, 0, Les);
                    Wert = Encoding.Default.GetString(b);


                    //   Wert = System.Text.Encoding.Default.GetString(BLoaded, Pointer + 5, Les)
                    X = 0;
                    Y = 0;
                    Pointer += 5 + Les;

                    break;
                }
                case enRoutinen.Column: {
                    Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                    Les = NummerCode3(_BLoaded, Pointer + 2);
                    ColNR = NummerCode3(_BLoaded, Pointer + 5);
                    RowNR = NummerCode3(_BLoaded, Pointer + 8);
                    byte[] b = new byte[Les];
                    _BLoaded.CopyTo(Pointer + 11, b, 0, Les);
                    Wert = Encoding.Default.GetString(b);

                    X = 0;
                    Y = 0;
                    Pointer += 11 + Les;
                    break;
                }
                default: {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Laderoutine nicht definiert: " + _BLoaded[Pointer]);
                    break;
                }
            }
        }


        protected override void ParseExternal(List<byte> B) {

            Column.ThrowEvents = false;

            enDatabaseDataType Art = 0;
            int Pointer = 0;

            string Inhalt = "";
            ColumnItem _Column = null;


            RowItem _Row = null;

            int X = 0;
            int Y = 0;


            int ColKey = 0;
            int RowKey = 0;


            List<ColumnItem> ColumnsOld = new List<ColumnItem>();
            ColumnsOld.AddRange(Column);
            Column.Clear();


            List<WorkItem> OldPendings = new List<WorkItem>();
            foreach (WorkItem ThisWork in Works) {
                if (ThisWork.State == enItemState.Pending) { OldPendings.Add(ThisWork); }
            }

            Works.Clear();
            do {
                if (Pointer >= B.Count) { break; }

                Parse(B, ref Pointer, ref Art, ref ColKey, ref RowKey, ref Inhalt, ref X, ref Y);

                if (RowKey > -1) {
                    _Row = Row.SearchByKey(RowKey);
                    if (_Row == null) {
                        _Row = new RowItem(this, RowKey);
                        Row.Add(_Row);
                    }
                }

                if (ColKey > -1) {
                    // Zuerst schauen, ob die Column schon (wieder) in der richtigen Collection ist
                    _Column = Column.SearchByKey(ColKey);

                    if (_Column == null) {
                        // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzuf�gen
                        foreach (ColumnItem ThisColumn in ColumnsOld) {
                            if (ThisColumn != null && ThisColumn.Key == ColKey) {
                                _Column = ThisColumn;
                            }
                        }

                        if (_Column != null) {
                            // Prima, gefunden! Noch die Collections korrigieren
                            Column.Add(enDatabaseDataType.AddColumn, _Column);
                            ColumnsOld.Remove(_Column);
                        } else {
                            // Nicht gefunden, als neu machen
                            _Column = Column.Add(ColKey);
                        }
                    }
                }


                if (Art == enDatabaseDataType.CryptionState) {
                    if (Inhalt.FromPlusMinus()) {
                        PasswordEventArgs e = new PasswordEventArgs();
                        OnNeedPassword(e);

                        B = modAllgemein.SimpleCrypt(B, e.Password, -1, Pointer, B.Count - 1);
                        if (B[Pointer] != 1 || B[Pointer + 1] != 3 || B[Pointer + 2] != 0 || B[Pointer + 3] != 0 || B[Pointer + 4] != 2 || B[Pointer + 5] != 79 || B[Pointer + 6] != 75 || B[Pointer + 7] != 1) {
                            RemoveFilename();
                            LoadedVersion = "9.99";
                            //MessageBox.Show("Zugriff verweigrt, Passwort falsch!", enImageCode.Kritisch, "OK");
                            break;
                        }

                    }
                }


                string _Fehler = ParseThis(Art, Inhalt, _Column, _Row, X, Y);

                if (Art == enDatabaseDataType.EOF) { break; }


                if (!string.IsNullOrEmpty(_Fehler)) {
                    LoadedVersion = "9.99";
                    Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + Filename + "<br>Meldung: " + _Fehler);
                }


            } while (true);

            // Spalten, die nach dem Reload nicht mehr ben�tigt werden, l�schen
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

            if (int.Parse(LoadedVersion.Replace(".", "")) > int.Parse(DatabaseVersion.Replace(".", ""))) { SetReadOnly(); }


        }


        public override void RepairAfterParse() {

            // System-Spalten checken und alte Formate auf neuen Stand bringen
            Column.Repair();

            // Evtl. Defekte Rows reparieren
            Row.Repair();

            //Defekte Ansichten reparieren - Teil 1
            for (int z = 0; z <= 1; z++) {
                if (ColumnArrangements.Count < z + 1) {
                    ColumnArrangements.Add(new ColumnViewCollection(this, ""));
                }



                if (string.IsNullOrEmpty(ColumnArrangements[z].Name)) {
                    switch (z) {
                        case 0:
                            ColumnArrangements[z].Name = "Alle Spalten";
                            if (ColumnArrangements[z].Count() < 1) { ColumnArrangements[z].ShowAllColumns(this); }
                            break;
                        case 1:
                            ColumnArrangements[z].Name = "Standard";
                            if (!ColumnArrangements[z].PermissionGroups_Show.Contains("#Everybody")) { ColumnArrangements[z].PermissionGroups_Show.Add("#Everybody"); }
                            break;

                            //case 2:
                            //    if (ColumnArrangements[z].Name != "Filter waagerecht" && !string.IsNullOrEmpty(ColumnArrangements[z].Name))
                            //    {
                            //        ColumnArrangements.Add(new ColumnViewCollection(this, ""));
                            //        ColumnArrangements.Swap(z, ColumnArrangements.Count - 1);
                            //    }

                            //    ColumnArrangements[z].Name = "Filter waagerecht";
                            //    break;

                            //case 3:
                            //    if (ColumnArrangements[z].Name != "Filter senkrecht" && !string.IsNullOrEmpty(ColumnArrangements[z].Name))
                            //    {
                            //        ColumnArrangements.Add(new ColumnViewCollection(this, ""));
                            //        ColumnArrangements.Swap(z, ColumnArrangements.Count - 1);
                            //    }

                            //    ColumnArrangements[z].Name = "Filter senkrecht";
                            //    break;

                    }
                }

                ColumnArrangements[z].PermissionGroups_Show.RemoveString("#Administrator", false);
            }

            CheckViewsAndArrangements();
        }



        private string ParseThis(enDatabaseDataType Art, string content, ColumnItem column, RowItem row, int width, int height) {


            if (Art >= enDatabaseDataType.Info_ColumDataSart && Art <= enDatabaseDataType.Info_ColumnDataEnd) {
                return column.Load(Art, content);
            }





            switch (Art) {
                case enDatabaseDataType.Formatkennung:
                    break;
                case enDatabaseDataType.Version:
                    LoadedVersion = content.Trim();
                    if (LoadedVersion != DatabaseVersion) {
                        Initialize();
                        LoadedVersion = content.Trim();
                    } else {
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
                    _Creator = content;
                    break;
                case enDatabaseDataType.CreateDate:
                    _CreateDate = content;
                    break;
                case enDatabaseDataType.ReloadDelaySecond:
                    _ReloadDelaySecond = int.Parse(content);
                    break;
                case enDatabaseDataType.DatenbankAdmin:
                    DatenbankAdmin.SplitByCR_QuickSortAndRemoveDouble(content);
                    break;
                case enDatabaseDataType.SortDefinition:
                    _sortDefinition = new RowSortDefinition(this, content);
                    break;
                case enDatabaseDataType.Caption:
                    _Caption = content;
                    break;
                case enDatabaseDataType.GlobalScale:
                    _GlobalScale = double.Parse(content);
                    break;
                case enDatabaseDataType.FilterImagePfad:
                    _FilterImagePfad = content;
                    break;
                case enDatabaseDataType.ZeilenQuickInfo:
                    _ZeilenQuickInfo = content;
                    break;
                case enDatabaseDataType.Ansicht:
                    _Ansicht = (enAnsicht)int.Parse(content);
                    break;
                case enDatabaseDataType.Tags:
                    Tags.SplitByCR(content);
                    break;
                case enDatabaseDataType.BinaryDataInOne:
                    Bins.Clear();
                    List<string> l = new List<string>(content.SplitByCR());
                    foreach (string t in l) {
                        Bins.Add(new clsNamedBinary(t));
                    }
                    break;
                case enDatabaseDataType.Layouts:
                    Layouts.SplitByCR_QuickSortAndRemoveDouble(content);
                    break;
                case enDatabaseDataType.AutoExport:
                    Export.Clear();
                    List<string> AE = new List<string>(content.SplitByCR());
                    foreach (string t in AE) {
                        Export.Add(new ExportDefinition(this, t));
                    }
                    break;

                case enDatabaseDataType.Rules_ALT:
                    List<RuleItem_Old> Rules = new List<RuleItem_Old>();
                    string[] RU = content.SplitByCR();
                    for (int z = 0; z <= RU.GetUpperBound(0); z++) {
                        Rules.Add(new RuleItem_Old(this, RU[z]));
                    }
                    _RulesScript = GenerateScriptFromRules(Rules);
                    break;

                case enDatabaseDataType.ColumnArrangement:
                    ColumnArrangements.Clear();
                    List<string> CA = new List<string>(content.SplitByCR());
                    foreach (string t in CA) {
                        ColumnArrangements.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.Views:
                    Views.Clear();
                    List<string> VI = new List<string>(content.SplitByCR());
                    foreach (string t in VI) {
                        Views.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.PermissionGroups_NewRow:
                    PermissionGroups_NewRow.SplitByCR_QuickSortAndRemoveDouble(content);
                    break;

                case enDatabaseDataType.LastRowKey:
                    return Row.Load_310(Art, content);

                case enDatabaseDataType.LastColumnKey:
                    return Column.Load_310(Art, content);

                case enDatabaseDataType.GlobalShowPass:
                    _GlobalShowPass = content;
                    break;

                case (enDatabaseDataType)30:
                    // TODO: Entferne GlobalInfo
                    break;

                case (enDatabaseDataType)52:
                    // TODO: Entferne Skin
                    break;

                case enDatabaseDataType.JoinTyp:
                    _JoinTyp = (enJoinTyp)int.Parse(content);
                    break;

                case enDatabaseDataType.VerwaisteDaten:
                    _VerwaisteDaten = (enVerwaisteDaten)int.Parse(content);
                    break;

                case enDatabaseDataType.ImportScript:
                    _ImportScript = content;
                    break;

                case enDatabaseDataType.RulesScript:
                    _RulesScript = content;
                    break;

                case enDatabaseDataType.FileEncryptionKey:
                    _FileEncryptionKey = content;
                    break;


                case enDatabaseDataType.ce_Value_withSizeData:
                case enDatabaseDataType.ce_UTF8Value_withSizeData:
                case enDatabaseDataType.ce_Value_withoutSizeData:
                    if (Art == enDatabaseDataType.ce_UTF8Value_withSizeData) { content = modConverter.UTF8toString(content); }
                    Cell.Load_310(column, row, content, width, height);
                    break;

                case enDatabaseDataType.UndoCount:
                    _UndoCount = int.Parse(content);
                    break;


                case enDatabaseDataType.UndoInOne:
                    Works.Clear();
                    string[] UIO = content.SplitByCR();
                    for (int z = 0; z <= UIO.GetUpperBound(0); z++) {
                        WorkItem tmpWork = new WorkItem(UIO[z]) {
                            State = enItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos �ndern.
                        };
                        Works.Add(tmpWork);
                    }
                    break;


                case enDatabaseDataType.dummyComand_AddRow:
                    int addRowKey = int.Parse(content);
                    if (Row.SearchByKey(addRowKey) == null) { Row.Add(new RowItem(this, addRowKey)); }
                    break;

                case enDatabaseDataType.AddColumn:
                    int addColumnKey = int.Parse(content);
                    if (Column.SearchByKey(addColumnKey) == null) { Column.Add(enDatabaseDataType.AddColumn, new ColumnItem(this, addColumnKey)); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveRow:
                    int removeRowKey = int.Parse(content);
                    if (Row.SearchByKey(removeRowKey) is RowItem) { Row.Remove(removeRowKey); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveColumn:
                    int removeColumnKey = int.Parse(content);
                    if (Column.SearchByKey(removeColumnKey) is ColumnItem col) { Column.Remove(col); }
                    break;

                case enDatabaseDataType.EOF:
                    return "";

                default:
                    LoadedVersion = "9.99";
                    if (!ReadOnly) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Laden von Datentyp \'" + Art + "\' nicht definiert.<br>Wert: " + content + "<br>Datei: " + Filename);
                    }
                    break;
            }

            return "";
        }

        private string GenerateScriptFromRules(List<RuleItem_Old> rules) {
            if (rules is null || rules.Count == 0) { return string.Empty; }


            string txt = "// Automatische Umwandlung des alten Regel-Systems in die neue Skript-Sprache.\r\n";
            txt += "// - Zur Bearbeitung wird NOTEPAD++ empfohlen.\r\n";
            txt += "// - Zur Versch�nerung der Optik wird https://codebeautify.org/javaviewer empfohlen.\r\n\r\n\r\n";

            foreach (RuleItem_Old thisRegel in rules) {
                txt = txt + thisRegel.ToScript();
            }


            return txt;


        }

        public void LoadPicsIntoImageChache() {
            foreach (clsNamedBinary bmp in Bins) {
                if (bmp.Picture != null) {
                    if (!string.IsNullOrEmpty(bmp.Name)) {
                        QuickImage.Add("DB_" + bmp.Name, new BitmapExt((Bitmap)bmp.Picture.Clone()));
                    }
                }

            }
        }


        internal string Column_UsedIn(ColumnItem column) {

            string t = string.Empty;
            bool layout = false;
            foreach (string thisLayout in Layouts) {
                if (thisLayout.ToUpper().Contains(column.Name.ToUpper())) { layout = true; }
            }
            if (layout) { t += " - Layouts (f�r Export)"; }



            if (SortDefinition.Columns.Contains(column)) { t += " - Sortierung<br>"; }

            bool view = false;
            foreach (ColumnViewCollection thisView in Views) {
                if (thisView[column] != null) { view = true; }
            }
            if (view) { t += " - Formular-Ansichten<br>"; }

            bool cola = false;
            bool first = true;
            foreach (ColumnViewCollection thisView in ColumnArrangements) {
                if (!first && thisView[column] != null) { cola = true; }
                first = false;
            }
            if (cola) { t += " - Benutzerdefinierte Spalten-Anordnungen<br>"; }


            if (ImportScript.ToUpper().Contains(column.Name.ToUpper())) { t += " - Import-Skript<br>"; }

            if (RulesScript.ToUpper().Contains(column.Name.ToUpper())) { t += " - Regeln-Skript<br>"; }

            if (ZeilenQuickInfo.ToUpper().Contains(column.Name.ToUpper())) { t += " - Zeilen-Quick-Info<br>"; }

            if (Tags.JoinWithCr().ToUpper().Contains(column.Name.ToUpper())) { t += " - Datenbank-Tags<br>"; }

            //var rul = false;
            //foreach (var ThisRule in Rules) {
            //    if (ThisRule.Contains(column)) { rul = true; }
            //}
            //if (rul) { t += " - Regeln<br>"; }

            List<string> l = column.Contents(null);
            if (l.Count > 0) {
                t = t + " - Bef�llt mit " + l.Count.ToString() + " verschiedenen Werten";
            }

            return t;

        }


        internal void Column_NameChanged(string oldName, ColumnItem newName) {


            if (string.IsNullOrEmpty(oldName)) { return; }

            // Cells ----------------------------------------------
            //   Cell.ChangeCaptionName(OldName, cColumnItem.Name, cColumnItem)


            //  Undo -----------------------------------------
            // Nicht n�tig, da die Spalten als Verwei� gespeichert sind

            // Layouts -----------------------------------------
            if (Layouts != null && Layouts.Count > 0) {
                for (int cc = 0; cc < Layouts.Count; cc++) {

                    RenameColumnInLayoutEventArgs e = new RenameColumnInLayoutEventArgs(Layouts[cc], oldName, newName);
                    OnRenameColumnInLayout(e);

                    Layouts[cc] = e.LayoutCode;
                }
            }


            // Sortierung -----------------------------------------
            // Nicht n�tig, da die Spalten als Verwei� gespeichert sind

            // _ColumnArrangements-----------------------------------------
            // Nicht n�tig, da die Spalten als Verwei� gespeichert sind

            // _Views-----------------------------------------
            // Nicht n�tig, da die Spalten als Verwei� gespeichert sind



            // ImportScript -----------------------------------------
            List<string> x = ImportScript.FromNonCritical().SplitByCRToList();
            List<string> xn = new List<string>();

            foreach (string thisstring in x) {
                if (!string.IsNullOrEmpty(thisstring)) {
                    string[] x2 = thisstring.SplitBy("|");
                    if (x2.Length > 2 && x2[1].ToUpper() == oldName.ToUpper()) {
                        x2[1] = newName.Name.ToUpper();
                        xn.Add(x2.JoinWith("|"));
                    } else {
                        xn.Add(thisstring);
                    }
                }
            }
            ImportScript = xn.JoinWithCr().ToNonCritical();



            // Zeilen-Quick-Info -----------------------------------------
            ZeilenQuickInfo = ZeilenQuickInfo.Replace("&" + oldName + ";", "&" + newName.Name + ";", RegexOptions.IgnoreCase);
            ZeilenQuickInfo = ZeilenQuickInfo.Replace("&" + oldName + "(", "&" + newName.Name + "(", RegexOptions.IgnoreCase);


        }





        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content) {
            List.Add((byte)enRoutinen.DatenAllgemein);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, Content.Length, 3);
            List.AddRange(Content.ToByte());
        }

        internal void SaveToByteList(List<byte> List, KeyValuePair<string, CellItem> vCell) {

            if (string.IsNullOrEmpty(vCell.Value.Value)) { return; }

            Cell.DataOfCellKey(vCell.Key, out ColumnItem tColumn, out RowItem tRow);


            if (!tColumn.SaveContent) { return; }

            string s = vCell.Value.Value;
            enDatabaseDataType tx = enDatabaseDataType.ce_Value_withSizeData;

            if (tColumn.Format.NeedUTF8()) {
                s = modConverter.StringtoUTF8(s);
                tx = enDatabaseDataType.ce_UTF8Value_withSizeData;
            }

            List.Add((byte)enRoutinen.CellFormat_OLD);
            List.Add((byte)tx);
            SaveToByteList(List, s.Length, 3);
            SaveToByteList(List, tColumn.Key, 3);
            SaveToByteList(List, tRow.Key, 3);
            List.AddRange(s.ToByte());
            Size ContentSize = Cell.ContentSizeToSave(vCell, tColumn);
            SaveToByteList(List, ContentSize.Width, 2);
            SaveToByteList(List, ContentSize.Height, 2);

        }


        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content, int TargetColumNr) {
            List.Add((byte)enRoutinen.Column);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, Content.Length, 3);
            SaveToByteList(List, TargetColumNr, 3);
            SaveToByteList(List, 0, 3); //Zeile-Un�tig
            List.AddRange(Content.ToByte());
        }

        private static int NummerCode3(List<byte> b, int pointer) {
            return b[pointer] * 65025 + b[pointer + 1] * 255 + b[pointer + 2];
        }

        private static int NummerCode2(List<byte> b, int pointer) {
            return b[pointer] * 255 + b[pointer + 1];
        }

        private void SaveToByteList(List<byte> List, int NrToAdd, int ByteAnzahl) {

            switch (ByteAnzahl) {
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






        public override string ErrorReason(enErrorReason mode) {
            string f = base.ErrorReason(mode);
            if (!string.IsNullOrEmpty(f)) { return f; }

            if (mode == enErrorReason.OnlyRead) { return string.Empty; }
            if (int.Parse(LoadedVersion.Replace(".", "")) > int.Parse(DatabaseVersion.Replace(".", ""))) { return "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."; }

            return string.Empty;
        }

        public override bool HasPendingChanges() {

            try {
                if (ReadOnly) { return false; }

                foreach (WorkItem ThisWork in Works) {
                    if (ThisWork.State == enItemState.Pending) { return true; }
                }
                return false;
            } catch {
                return HasPendingChanges();
            }

        }




        #region  Export CSV / HTML 


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="column">Die Spalte, die zur�ckgegeben wird.</param>
        /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle Zeilen zur�ck.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, ColumnItem column, List<RowItem> sortedRows) {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            return Export_CSV(firstRow, new List<ColumnItem>() { column }, sortedRows);
        }


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="columnList">Die Spalten, die zur�ckgegeben werden. NULL gibt alle Spalten zur�ck.</param>
        /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle ZEilen zur�ck.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, List<ColumnItem> columnList, List<RowItem> sortedRows) {
            if (columnList == null) {
                columnList = new List<ColumnItem>();
                foreach (ColumnItem ThisColumnItem in Column) {
                    if (ThisColumnItem != null) {
                        columnList.Add(ThisColumnItem);
                    }
                }
            }

            if (sortedRows == null) {
                sortedRows = new List<RowItem>();
                foreach (RowItem ThisRowItem in Row) {
                    if (ThisRowItem != null) {
                        sortedRows.Add(ThisRowItem);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();


            switch (firstRow) {
                case enFirstRow.Without:

                    break;
                case enFirstRow.ColumnCaption:
                    for (int ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            string tmp = columnList[ColNr].ReadableText();
                            tmp = tmp.Replace(";", "|");
                            tmp = tmp.Replace(" |", "|");
                            tmp = tmp.Replace("| ", "|");
                            sb.Append(tmp);
                            if (ColNr < columnList.Count - 1) { sb.Append(";"); }
                        }
                    }
                    sb.Append("\r\n");


                    break;
                case enFirstRow.ColumnInternalName:
                    for (int ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            sb.Append(columnList[ColNr].Name);
                            if (ColNr < columnList.Count - 1) { sb.Append(";"); }
                        }
                    }
                    sb.Append("\r\n");

                    break;
                default:
                    Develop.DebugPrint(firstRow);
                    break;
            }





            foreach (RowItem ThisRow in sortedRows) {
                if (ThisRow != null) {
                    for (int ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            string tmp = Cell.GetString(columnList[ColNr], ThisRow);
                            tmp = tmp.Replace("\r\n", "|");
                            tmp = tmp.Replace("\r", "|");
                            tmp = tmp.Replace("\n", "|");
                            tmp = tmp.Replace(";", "<sk>");

                            sb.Append(tmp);
                            if (ColNr < columnList.Count - 1) { sb.Append(";"); }
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
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="arrangement">Die Spalten, die zur�ckgegeben werden. NULL gibt alle Spalten zur�ck.</param>
        /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle ZEilen zur�ck.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, ColumnViewCollection arrangement, List<RowItem> sortedRows) {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            return Export_CSV(firstRow, arrangement.ListOfUsedColumn(), sortedRows);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, int arrangementNo, FilterCollection filter, List<RowItem> pinned) {
            //    Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            return Export_CSV(firstRow, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned));
        }



        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, int arrangementNo, FilterCollection filter, List<RowItem> pinned) {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_HTML(filename, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned), false);
        }


        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, List<ColumnItem> columnList, List<RowItem> sortedRows, bool execute) {
            if (columnList == null || columnList.Count == 0) {
                columnList = new List<ColumnItem>();
                foreach (ColumnItem ThisColumnItem in Column) {
                    if (ThisColumnItem != null) {
                        columnList.Add(ThisColumnItem);
                    }
                }
            }

            if (sortedRows == null) {
                sortedRows = new List<RowItem>();
                foreach (RowItem ThisRowItem in Row) {
                    if (ThisRowItem != null) {
                        sortedRows.Add(ThisRowItem);
                    }
                }
            }

            if (string.IsNullOrEmpty(filename)) {
                filename = TempFile(string.Empty, "Export", "html");
            }


            HTML da = new HTML(Filename.FileNameWithoutSuffix());
            da.AddCaption(_Caption);

            da.TableBeginn();
            da.RowBeginn();


            foreach (ColumnItem ThisColumn in columnList) {
                if (ThisColumn != null) {
                    da.CellAdd(ThisColumn.ReadableText().Replace(";", "<br>"), ThisColumn.BackColor);
                    //da.Add("        <th bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"><b>" + ThisColumn.ReadableText().Replace(";", "<br>") + "</b></th>");
                }
            }
            da.RowEnd();


            foreach (RowItem ThisRow in sortedRows) {
                if (ThisRow != null) {
                    da.RowBeginn();
                    foreach (ColumnItem ThisColumn in columnList) {
                        if (ThisColumn != null) {

                            ColumnItem LCColumn = ThisColumn;
                            RowItem LCrow = ThisRow;
                            if (ThisColumn.Format == enDataFormat.LinkedCell) {
                                (LCColumn, LCrow) = CellCollection.LinkedCellData(ThisColumn, ThisRow, false, false);
                            }

                            if (LCrow != null && LCColumn != null) {
                                da.CellAdd(LCrow.CellGetValuesReadable(LCColumn, enShortenStyle.HTML).JoinWith("<br>"), ThisColumn.BackColor);
                            } else {
                                da.CellAdd(" ", ThisColumn.BackColor);
                            }
                        }
                    }
                    da.RowEnd();
                }
            }


            // Summe----
            da.RowBeginn();
            foreach (ColumnItem ThisColumn in columnList) {
                if (ThisColumn != null) {
                    double? s = ThisColumn.Summe(sortedRows);
                    if (s == null) {
                        da.CellAdd("-", ThisColumn.BackColor);

                        //da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                    } else {
                        da.CellAdd("&sum; " + s, ThisColumn.BackColor);
                        //da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">&sum; " + s + "</th>");
                    }
                }
            }
            da.RowEnd();

            // ----------------------
            da.TableEnd();

            da.AddFoot();
            da.Save(filename, execute);
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, ColumnViewCollection arrangement, List<RowItem> sortedRows, bool execute) {
            Export_HTML(filename, arrangement.ListOfUsedColumn(), sortedRows, execute);
        }


        #endregion


        private bool PermissionCheckWithoutAdmin(string allowed, RowItem row) {


            string tmpName = UserName.ToUpper();
            string tmpGroup = UserGroup.ToUpper();


            if (allowed.ToUpper() == "#EVERYBODY") {
                return true;
            } else if (allowed.ToUpper() == "#ROWCREATOR") {
                if (row != null && Cell.GetString(Column.SysRowCreator, row).ToUpper() == tmpName) {
                    return true;
                }
            }
              //else if (allowed.ToUpper() == "#ROWCHANGER")
              //{
              //    if (row != null && Cell.GetString(Column.SysRowChanger, row).ToUpper() == tmpName)
              //    {
              //        return true;
              //    }
              //}
              else if (allowed.ToUpper() == "#USER: " + tmpName) {
                return true;
            } else if (allowed.ToUpper() == "#USER:" + tmpName) {
                return true;
            } else if (allowed.ToUpper() == tmpGroup) {
                return true;
            }
            //else if (allowed.ToUpper() == "#DATABASECREATOR")
            //{
            //    if (UserName.ToUpper() == _Creator.ToUpper()) { return true; }
            //}

            return false;
        }


        public bool PermissionCheck(ListExt<string> allowed, RowItem row) {

            try {

                if (IsAdministrator()) { return true; }
                if (allowed == null || allowed.Count == 0) { return false; }

                foreach (string ThisString in allowed) {
                    if (PermissionCheckWithoutAdmin(ThisString, row)) { return true; }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
            }

            return false;
        }

        public List<string> Permission_AllUsed(bool DatabaseEbene) {
            List<string> e = new List<string>();

            foreach (ColumnItem ThisColumnItem in Column) {
                if (ThisColumnItem != null) {
                    e.AddRange(ThisColumnItem.PermissionGroups_ChangeCell);
                }
            }

            e.AddRange(PermissionGroups_NewRow);
            e.AddRange(DatenbankAdmin);

            foreach (ColumnViewCollection ThisArrangement in ColumnArrangements) {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }

            foreach (ColumnViewCollection ThisArrangement in Views) {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }



            //e.Add("#DatabaseCreator");
            e.Add("#Everybody");
            e.Add("#User: " + UserName);

            if (!DatabaseEbene) {
                e.Add("#RowCreator");
                //e.Add("#RowChanger");
            } else {
                e.RemoveString("#RowCreator", false);
                //e.RemoveString("#RowChanger", false);

            }

            e.RemoveString("#Administrator", false);
            if (!IsAdministrator()) {
                e.Add(UserGroup);
            }
            return e.SortedDistinctList();
        }


        public bool IsAdministrator() {
            if (DatenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
            if (string.IsNullOrEmpty(UserGroup)) { return false; }
            if (DatenbankAdmin.Contains(UserGroup, false)) { return true; }
            return Convert.ToBoolean(UserGroup.ToUpper() == "#ADMINISTRATOR");
        }

        protected override List<byte> ToListOfByte(bool willSave) {

            try {
                int CryptPos = -1;
                List<byte> l = new List<byte>();

                // Wichtig, Reihenfolge und L�nge NIE ver�ndern!
                SaveToByteList(l, enDatabaseDataType.Formatkennung, "BlueDatabase");
                SaveToByteList(l, enDatabaseDataType.Version, DatabaseVersion);
                SaveToByteList(l, enDatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...

                // Passw�rter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden k�nnen
                if (string.IsNullOrEmpty(_GlobalShowPass)) {
                    SaveToByteList(l, enDatabaseDataType.CryptionState, false.ToPlusMinus());
                } else {
                    SaveToByteList(l, enDatabaseDataType.CryptionState, true.ToPlusMinus());
                    CryptPos = l.Count;
                    SaveToByteList(l, enDatabaseDataType.CryptionTest, "OK");
                }


                SaveToByteList(l, enDatabaseDataType.GlobalShowPass, _GlobalShowPass);
                SaveToByteList(l, enDatabaseDataType.FileEncryptionKey, _FileEncryptionKey);
                SaveToByteList(l, enDatabaseDataType.Creator, _Creator);
                SaveToByteList(l, enDatabaseDataType.CreateDate, _CreateDate);

                SaveToByteList(l, enDatabaseDataType.Caption, _Caption);
                SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString());
                SaveToByteList(l, enDatabaseDataType.VerwaisteDaten, ((int)_VerwaisteDaten).ToString());
                SaveToByteList(l, enDatabaseDataType.Tags, Tags.JoinWithCr());
                SaveToByteList(l, enDatabaseDataType.PermissionGroups_NewRow, PermissionGroups_NewRow.JoinWithCr());
                SaveToByteList(l, enDatabaseDataType.DatenbankAdmin, DatenbankAdmin.JoinWithCr());
                SaveToByteList(l, enDatabaseDataType.GlobalScale, _GlobalScale.ToString());
                SaveToByteList(l, enDatabaseDataType.Ansicht, ((int)_Ansicht).ToString());
                SaveToByteList(l, enDatabaseDataType.ReloadDelaySecond, _ReloadDelaySecond.ToString());
                SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
                SaveToByteList(l, enDatabaseDataType.RulesScript, _RulesScript);

                SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));

                SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _FilterImagePfad);

                SaveToByteList(l, enDatabaseDataType.ZeilenQuickInfo, _ZeilenQuickInfo);

                Column.SaveToByteList(l);
                Row.SaveToByteList(l);

                Cell.SaveToByteList(ref l);

                if (SortDefinition == null) {
                    // Ganz neue Datenbank
                    SaveToByteList(l, enDatabaseDataType.SortDefinition, "");
                } else {
                    SaveToByteList(l, enDatabaseDataType.SortDefinition, _sortDefinition.ToString());
                }


                //SaveToByteList(l, enDatabaseDataType.Rules_ALT, Rules.ToString(true));



                SaveToByteList(l, enDatabaseDataType.ColumnArrangement, ColumnArrangements.ToString());

                SaveToByteList(l, enDatabaseDataType.Views, Views.ToString());

                SaveToByteList(l, enDatabaseDataType.Layouts, Layouts.JoinWithCr());

                SaveToByteList(l, enDatabaseDataType.AutoExport, Export.ToString(true));

                // Beim Erstellen des Undo-Speichers die Works nicht ver�ndern, da auch bei einem nicht
                // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
                // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
                List<string> Works2 = new List<string>();
                foreach (WorkItem thisWorkItem in Works) {
                    if (thisWorkItem.Comand != enDatabaseDataType.ce_Value_withoutSizeData) {
                        Works2.Add(thisWorkItem.ToString());
                    } else {
                        if (thisWorkItem.LogsUndo(this)) {
                            Works2.Add(thisWorkItem.ToString());
                        }
                    }
                }



                SaveToByteList(l, enDatabaseDataType.UndoCount, _UndoCount.ToString());

                if (Works2.Count > _UndoCount) { Works2.RemoveRange(0, Works2.Count - _UndoCount); }


                SaveToByteList(l, enDatabaseDataType.UndoInOne, Works2.JoinWithCr());

                SaveToByteList(l, enDatabaseDataType.EOF, "END");



                if (CryptPos > 0) {
                    return modAllgemein.SimpleCrypt(l, _GlobalShowPass, 1, CryptPos, l.Count - 1);
                }
                return l;

            } catch {
                return ToListOfByte(willSave);
            }
        }


        internal void OnViewChanged() {
            ViewChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnExporting(CancelEventArgs e) {
            Exporting?.Invoke(this, e);
        }

        internal void OnLoadingLinkedDatabase(DatabaseSettingsEventHandler e) {
            LoadingLinkedDatabase?.Invoke(this, e);
        }
        internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventargs e) {
            GenerateLayoutInternal?.Invoke(this, e);
        }

        private void OnSortParameterChanged() {
            SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnRenameColumnInLayout(RenameColumnInLayoutEventArgs e) {
            RenameColumnInLayout?.Invoke(this, e);
        }


        public List<string> AllConnectedFilesLCase() {
            List<string> Column_All = new List<string>();

            foreach (ColumnItem ThisColumnItem in Column) {
                if (ThisColumnItem != null) {

                    if (ThisColumnItem.Format == enDataFormat.Link_To_Filesystem) {
                        List<string> tmp = ThisColumnItem.Contents(null);

                        foreach (string thisTmp in tmp) {
                            Column_All.AddIfNotExists(ThisColumnItem.BestFile(thisTmp, false).ToLower());
                        }
                    }

                }


            }

            return Column_All.SortedDistinctList();
        }



        #region  Undo 


        public string UndoText(ColumnItem column, RowItem row) {

            if (Works == null || Works.Count == 0) { return string.Empty; }


            string CellKey = CellCollection.KeyOfCell(column, row);

            string t = "";

            for (int z = Works.Count - 1; z >= 0; z--) {
                if (Works[z].CellKey == CellKey) {

                    if (Works[z].HistorischRelevant) {
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


        public string DefaultLayoutPath() {
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; }

            return Filename.FilePath() + "Layouts\\";
        }

        /// <summary>
        /// F�gt Comandos manuell hinzu. Vorsicht: Kann Datenbank besch�digen
        /// </summary>
        public void InjectCommand(enDatabaseDataType Comand, string ChangedTo) {
            AddPending(Comand, -1, -1, string.Empty, ChangedTo, true);
        }


        internal void AddPending(enDatabaseDataType Comand, ColumnItem column, string PreviousValue, string ChangedTo, bool ExecuteNow) {
            AddPending(Comand, column.Key, -1, PreviousValue, ChangedTo, ExecuteNow);
        }

        internal void AddPending(enDatabaseDataType Comand, int ColumnKey, string ListExt, bool ExecuteNow) {
            AddPending(Comand, ColumnKey, -1, "", ListExt, ExecuteNow);
        }

        internal void AddPending(enDatabaseDataType Comand, int ColumnKey, int RowKey, string PreviousValue, string ChangedTo, bool ExecuteNow) {

            if (ExecuteNow) {
                ParseThis(Comand, ChangedTo, Column.SearchByKey(ColumnKey), Row.SearchByKey(RowKey), -1, -1);
            }

            if (IsParsing) { return; }
            if (ReadOnly) {
                if (!string.IsNullOrEmpty(Filename)) {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Datei ist Readonly, " + Comand + ", " + Filename);
                }
                return;
            }


            // Keine Doppelten Rausfiltern, ansonstn stimmen die Undo nicht mehr

            UserEditedAktionUTC = DateTime.UtcNow;

            if (RowKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
            if (ColumnKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }

            Works.Add(new WorkItem(Comand, ColumnKey, RowKey, PreviousValue, ChangedTo, UserName));


        }


        private void ExecutePending() {
            if (!IsParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Nur w�hrend des Parsens m�glich"); }
            if (!HasPendingChanges()) { return; }


            // Erst die Neuen Zeilen / Spalten alle neutralisieren
            int dummy = -1000;
            foreach (WorkItem ThisPending in Works) {

                if (ThisPending.State == enItemState.Pending) {
                    if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow) {
                        dummy--;
                        ChangeRowKeyInPending(ThisPending.RowKey, dummy);
                    }
                    if (ThisPending.Comand == enDatabaseDataType.AddColumn) {
                        dummy--;
                        ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
                    }
                }
            }

            // Dann den neuen Zeilen / Spalten Tats�chlich eine neue ID zuweisen
            foreach (WorkItem ThisPending in Works) {

                if (ThisPending.State == enItemState.Pending) {
                    switch (ThisPending.Comand) {
                        case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
                            string Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
                            RowItem fRow = Row[Value];

                            if (!string.IsNullOrEmpty(Value) && fRow != null) {
                                ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
                            } else {
                                ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
                            }

                            break;
                        }
                        case enDatabaseDataType.dummyComand_AddRow:
                            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
                            break;
                        case enDatabaseDataType.AddColumn:
                            ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey());
                            break;
                    }
                }
            }

            // Und nun alles ausf�hren!
            foreach (WorkItem ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {

                    if (ThisPending.Comand == enDatabaseDataType.co_Name) {
                        ThisPending.ChangedTo = Column.Freename(ThisPending.ChangedTo);
                    }
                    ExecutePending(ThisPending);
                }
            }
        }

        private void ExecutePending(WorkItem ThisPendingItem) {
            if (ThisPendingItem.State == enItemState.Pending) {


                RowItem _Row = null;
                if (ThisPendingItem.RowKey > -1) {
                    _Row = Row.SearchByKey(ThisPendingItem.RowKey);
                    if (_Row == null) {
                        if (ThisPendingItem.Comand != enDatabaseDataType.dummyComand_AddRow && ThisPendingItem.User != UserName) {
                            Develop.DebugPrint("Pending verworfen, Zeile gel�scht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }

                    }
                }

                ColumnItem _Col = null;
                if (ThisPendingItem.ColKey > -1) {
                    _Col = Column.SearchByKey(ThisPendingItem.ColKey);
                    if (_Col == null) {
                        if (ThisPendingItem.Comand != enDatabaseDataType.AddColumn && ThisPendingItem.User != UserName) {
                            Develop.DebugPrint("Pending verworfen, Spalte gel�scht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ParseThis(ThisPendingItem.Comand, ThisPendingItem.ChangedTo, _Col, _Row, 0, 0);
            }
        }


        private string SearchKeyValueInPendingsOf(int RowKey) {
            string F = string.Empty;
            foreach (WorkItem ThisPending in Works) {

                if (ThisPending.State == enItemState.Pending) {
                    if (ThisPending.RowKey == RowKey && ThisPending.Comand == enDatabaseDataType.ce_Value_withoutSizeData && ThisPending.ColKey == Column[0].Key) {
                        F = ThisPending.ChangedTo;
                    }
                }
            }
            return F;
        }



        private void ChangeRowKeyInPending(int OldKey, int NewKey) {

            foreach (WorkItem ThisPending in Works) {

                if (ThisPending.State == enItemState.Pending) {

                    if (ThisPending.RowKey == OldKey) {
                        if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }

                        ThisPending.RowKey = NewKey; // Generell den Schl�ssel �ndern

                        if (_LastWorkItem == "X") {
                            _LastWorkItem = ThisPending.ToString();
                            Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem ge�ndert: " + _LastWorkItem);
                        }

                        switch (ThisPending.Comand) {
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

        private void OnRowKeyChanged(KeyChangedEventArgs e) {
            RowKeyChanged?.Invoke(this, e);
        }

        private void OnNeedPassword(PasswordEventArgs e) {
            NeedPassword?.Invoke(this, e);
        }



        private void ChangeColumnKeyInPending(int OldKey, int NewKey) {

            foreach (WorkItem ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {

                    if (ThisPending.ColKey == OldKey) {
                        if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }

                        ThisPending.ColKey = NewKey; // Generell den Schl�ssel �ndern

                        if (_LastWorkItem == "X") {
                            _LastWorkItem = ThisPending.ToString();
                            Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem ge�ndert: " + _LastWorkItem);
                        }



                        switch (ThisPending.Comand) {
                            case enDatabaseDataType.AddColumn:
                            case enDatabaseDataType.dummyComand_RemoveColumn:
                                ThisPending.ChangedTo = NewKey.ToString();
                                break;
                            default:
                                if (ThisPending.PreviousValue.Contains(ColumnCollection.ParsableColumnKey(OldKey))) {
                                    Develop.DebugPrint("Replace machen (Old): " + OldKey);
                                }
                                if (ThisPending.ChangedTo.Contains(ColumnCollection.ParsableColumnKey(OldKey))) {
                                    Develop.DebugPrint("Replace machen (New): " + OldKey);
                                }
                                break;
                        }

                    }

                }

                OnColumnKeyChanged(new KeyChangedEventArgs(OldKey, NewKey));

            }
        }

        private void OnColumnKeyChanged(KeyChangedEventArgs e) {
            ColumnKeyChanged?.Invoke(this, e);
        }




        internal void ChangeWorkItems(enItemState OldState, enItemState NewState) {

            foreach (WorkItem ThisWork in Works) {
                if (ThisWork.State == OldState) { ThisWork.State = NewState; }
            }
        }

        private void InvalidateExports(string LayoutID) {
            if (ReadOnly) { return; }

            bool Done = false;

            foreach (ExportDefinition ThisExport in Export) {
                if (ThisExport != null) {
                    if (ThisExport.Typ == enExportTyp.EinzelnMitFormular) {
                        if (ThisExport.ExportFormularID == LayoutID) {
                            Done = true;
                            ThisExport.LastExportTimeUTC = new DateTime(1900, 1, 1);
                        }
                    }
                }
            }

            if (Done) {
                AddPending(enDatabaseDataType.AutoExport, -1, Export.ToString(true), false);
            }
        }


        protected override void DoBackGroundWork(BackgroundWorker listenToMyCancel) {
            if (ReadOnly) { return; }

            if (!HasPendingChanges()) {

                CancelEventArgs ec = new CancelEventArgs(false);
                OnExporting(ec);
                if (ec.Cancel) { return; }
            }
            bool ReportAChange = false;
            bool tmp = false;

            try {

                if (!listenToMyCancel.CancellationPending) {

                    foreach (ExportDefinition ThisExport in Export) {
                        if (ThisExport != null) { tmp = ThisExport.DeleteOutdatedBackUps(listenToMyCancel); }
                        if (tmp) { ReportAChange = true; }
                        if (listenToMyCancel.CancellationPending) { break; }
                    }

                }

                if (!listenToMyCancel.CancellationPending) {
                    foreach (ExportDefinition ThisExport in Export) {
                        if (ThisExport != null) { tmp = ThisExport.DoBackUp(listenToMyCancel); }
                        if (tmp) { ReportAChange = true; }
                    }
                }

            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }

            if (ReportAChange) {
                AddPending(enDatabaseDataType.AutoExport, -1, Export.ToString(true), false);
            }
        }


        protected override void BackgroundWorkerMessage(ProgressChangedEventArgs e) {
            //switch ((string)e.UserState) {
            //    case "AddPending":
            //        AddPending(enDatabaseDataType.AutoExport, -1, Export.ToString(true), false);
            //        break;

            //    default:
            //        Develop.DebugPrint("Unbekannter Befehl:" + (string)e.UserState);
            //        break;

            //}
        }

        //public bool AllRulesOK() {
        //    return AllRulesOK(Rules);
        //}

        //public static bool AllRulesOK(ListExt<RuleItem> RulesToCheck) {
        //    foreach (var thisRule in RulesToCheck) {
        //        if (thisRule != null && !thisRule.IsOk()) { return false; }
        //    }
        //    return true;
        //}

        protected override void PrepeareDataForCheckingBeforeLoad() {
            // Letztes WorkItem speichern, als Kontrolle
            WVorher = string.Empty;
            _LastWorkItem = string.Empty;
            if (Works != null && Works.Count > 0) {
                int c = 0;
                do {
                    c++;
                    if (c > 20 || Works.Count - c < 20) { break; }
                    int wn = Works.Count - c;
                    if (Works[wn].LogsUndo(this) && Works[wn].HistorischRelevant) { _LastWorkItem = Works[wn].ToString(); }

                } while (string.IsNullOrEmpty(_LastWorkItem));
                WVorher = Works.ToString();
            }
        }

        protected override void CheckDataAfterReload() {

            try {

                // Leztes WorkItem suchen. Auch Ohne LogUndo MUSS es vorhanden sein.
                if (!string.IsNullOrEmpty(_LastWorkItem)) {
                    bool ok = false;
                    string ok2 = string.Empty;
                    foreach (WorkItem ThisWorkItem in Works) {
                        string tmp = ThisWorkItem.ToString();
                        if (tmp == _LastWorkItem) {
                            ok = true;
                            break;
                        } else if (tmp.Substring(7) == _LastWorkItem.Substring(7)) {
                            ok2 = tmp;
                        }
                    }

                    if (!ok && string.IsNullOrEmpty(ok2)) {
                        if (!Filename.Contains("AutoVue") && !Filename.Contains("Plandaten") && !Filename.Contains("Ketten.") && !Filename.Contains("Kettenr�der.") && !Filename.Contains("TVW")) {
                            Develop.DebugPrint(enFehlerArt.Warnung, "WorkItem verschwunden<br>" + _LastWorkItem + "<br>" + Filename + "<br><br>Vorher:<br>" + WVorher + "<br><br>Nachher:<br>" + Works.ToString());
                        }
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        protected override void DoWorkAfterSaving() {

            ChangeWorkItems(enItemState.Pending, enItemState.Undo);

            List<string> FilesNewLCase = AllConnectedFilesLCase();

            List<string> Writer_FilesToDeleteLCase = new List<string>();

            if (_VerwaisteDaten == enVerwaisteDaten.L�schen) {
                Writer_FilesToDeleteLCase = FilesAfterLoadingLCase.Except(FilesNewLCase).ToList();
            }

            FilesAfterLoadingLCase.Clear();
            FilesAfterLoadingLCase.AddRange(FilesNewLCase);

            if (Writer_FilesToDeleteLCase.Count > 0) { DeleteFile(Writer_FilesToDeleteLCase); }
        }

        protected override bool isSomethingDiscOperatingsBlocking() {
            return false;
        }

        protected override bool IsThereBackgroundWorkToDo() {
            if (HasPendingChanges()) { return true; }

            CancelEventArgs ec = new CancelEventArgs(false);
            OnExporting(ec);
            if (ec.Cancel) { return false; }


            long nowsek = (DateTime.UtcNow.Ticks - _startTick) / 10000000;

            if (nowsek % 60 != 0) { return false; } // Lasten startabh�ngig verteilen. Bei Pending changes ist es eh immer true;



            foreach (ExportDefinition ThisExport in Export) {
                if (ThisExport != null) {
                    if (ThisExport.Typ == enExportTyp.EinzelnMitFormular) { return true; }
                    if (DateTime.UtcNow.Subtract(ThisExport.LastExportTimeUTC).TotalDays > ThisExport.Intervall) { return true; }
                }
            }

            return false;
        }

    }
}