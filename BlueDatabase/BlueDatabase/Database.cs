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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        #region Fields

        public static readonly string DatabaseVersion = "3.51";

        public readonly CellCollection Cell;

        public readonly ColumnCollection Column;

        public readonly ListExt<ColumnViewCollection> ColumnArrangements = new();

        public readonly ListExt<string> DatenbankAdmin = new();

        /// <summary>
        /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
        /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
        /// </summary>
        public readonly ListExt<ExportDefinition> Export = new();

        public readonly LayoutCollection Layouts = new();

        public readonly ListExt<string> PermissionGroups_NewRow = new();

        public readonly RowCollection Row;

        public readonly ListExt<string> Tags = new();

        public readonly string UserName = Generic.UserName().ToUpper();

        public readonly ListExt<ColumnViewCollection> Views = new();

        public string UserGroup = "#Administrator";

        public ListExt<WorkItem> Works;

        private readonly List<string> FilesAfterLoadingLCase;

        private string _AdditionaFilesPfad;

        private string _AdditionaFilesPfadtmp = string.Empty;

        private enAnsicht _Ansicht;

        private string _Caption;

        private string _CreateDate;

        private string _Creator;

        private string _FileEncryptionKey;

        private string _FilterImagePfad;

        private double _GlobalScale;

        private string _GlobalShowPass;

        private enJoinTyp _JoinTyp;

        /// <summary>
        /// Variable nur temporär für den BinReloader, um mögliche Datenverluste zu entdecken.
        /// </summary>
        private string _LastWorkItem = string.Empty;

        private string _RulesScript;

        private RowSortDefinition _sortDefinition;

        private int _UndoCount;

        private enVerwaisteDaten _VerwaisteDaten;

        private string _WorkItemsBefore = string.Empty;

        private string _ZeilenQuickInfo;

        #endregion

        #region Constructors

        public Database(Stream Stream) : this(Stream, string.Empty, true, false) { }

        public Database(bool readOnly) : this(null, string.Empty, readOnly, true) { }

        public Database(string filename, bool readOnly, bool create) : this(null, filename, readOnly, create) { }

        private Database(Stream stream, string filename, bool readOnly, bool create) : base(readOnly, true) {
            CultureInfo culture = new("de-DE");
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
            Export.Changed += Export_ListOrItemChanged;
            DatenbankAdmin.Changed += DatabaseAdmin_ListOrItemChanged;
            Row.RowRemoving += Row_RowRemoving;
            Row.RowAdded += Row_RowAdded;
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
            } else {
                RepairAfterParse();
            }
            QuickImage.NeedImage += QuickImage_NeedImage;
        }

        #endregion

        #region Events

        public event EventHandler<KeyChangedEventArgs> ColumnKeyChanged;

        public event EventHandler<MessageEventArgs> DropMessage;

        public event CancelEventHandler Exporting;

        public event EventHandler<GenerateLayoutInternalEventargs> GenerateLayoutInternal;

        public event EventHandler<PasswordEventArgs> NeedPassword;

        public event EventHandler<ProgressbarEventArgs> ProgressbarInfo;

        public event EventHandler<KeyChangedEventArgs> RowKeyChanged;

        public event EventHandler<RowCancelEventArgs> ScriptError;

        public event EventHandler SortParameterChanged;

        public event EventHandler ViewChanged;

        #endregion

        #region Properties

        [Browsable(false)]
        [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.")]
        public string AdditionaFilesPfad {
            get => _AdditionaFilesPfad;
            set {
                if (_AdditionaFilesPfad == value) { return; }
                _AdditionaFilesPfadtmp = string.Empty;
                AddPending(enDatabaseDataType.AdditionaFilesPfad, -1, -1, _AdditionaFilesPfad, value, true);
                Cell.InvalidateAllSizes();
            }
        }

        [Browsable(false)]
        public enAnsicht Ansicht {
            get => _Ansicht;
            set {
                if (_Ansicht == value) { return; }
                AddPending(enDatabaseDataType.Ansicht, -1, -1, ((int)_Ansicht).ToString(), ((int)value).ToString(), true);
            }
        }

        [Browsable(false)]
        public string Caption {
            get => _Caption;
            set {
                if (_Caption == value) { return; }
                AddPending(enDatabaseDataType.Caption, -1, -1, _Caption, value, true);
            }
        }

        [Browsable(false)]
        public string CreateDate {
            get => _CreateDate;
            set {
                if (_CreateDate == value) { return; }
                AddPending(enDatabaseDataType.CreateDate, -1, -1, _CreateDate, value, true);
            }
        }

        [Browsable(false)]
        public string Creator {
            get => _Creator.Trim();
            set {
                if (_Creator == value) { return; }
                AddPending(enDatabaseDataType.Creator, -1, -1, _Creator, value, true);
            }
        }

        public string FileEncryptionKey {
            get => _FileEncryptionKey;
            set {
                if (_FileEncryptionKey == value) { return; }
                AddPending(enDatabaseDataType.FileEncryptionKey, -1, -1, _FileEncryptionKey, value, true);
            }
        }

        [Browsable(false)]
        [Description("Ein Bild, das in der senkrechte Filterleiste angezeigt werden kann.")]
        public string FilterImagePfad {
            get => _FilterImagePfad;
            set {
                if (_FilterImagePfad == value) { return; }
                AddPending(enDatabaseDataType.FilterImagePfad, -1, -1, _FilterImagePfad, value, true);
                Cell.InvalidateAllSizes();
            }
        }

        [Browsable(false)]
        public double GlobalScale {
            get => _GlobalScale;
            set {
                if (_GlobalScale == value) { return; }
                AddPending(enDatabaseDataType.GlobalScale, -1, -1, _GlobalScale.ToString(), value.ToString(), true);
                Cell.InvalidateAllSizes();
            }
        }

        public string GlobalShowPass {
            get => _GlobalShowPass;
            set {
                if (_GlobalShowPass == value) { return; }
                AddPending(enDatabaseDataType.GlobalShowPass, -1, -1, _GlobalShowPass, value, true);
            }
        }

        public enJoinTyp JoinTyp {
            get => _JoinTyp;
            set {
                if (_JoinTyp == value) { return; }
                AddPending(enDatabaseDataType.JoinTyp, -1, -1, ((int)_JoinTyp).ToString(), ((int)value).ToString(), true);
            }
        }

        public string LoadedVersion { get; private set; }

        [Browsable(false)]
        public int ReloadDelaySecond {
            get => _ReloadDelaySecond;
            set {
                if (_ReloadDelaySecond == value) { return; }
                AddPending(enDatabaseDataType.ReloadDelaySecond, -1, -1, _ReloadDelaySecond.ToString(), value.ToString(), true);
            }
        }

        public string RulesScript {
            get => _RulesScript;
            set {
                if (_RulesScript == value) { return; }
                AddPending(enDatabaseDataType.RulesScript, -1, -1, _RulesScript, value, true);
            }
        }

        [Browsable(false)]
        public RowSortDefinition SortDefinition {
            get => _sortDefinition;
            set {
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
        public int UndoCount {
            get => _UndoCount;
            set {
                if (_UndoCount == value) { return; }
                AddPending(enDatabaseDataType.UndoCount, -1, -1, _UndoCount.ToString(), value.ToString(), true);
            }
        }

        public enVerwaisteDaten VerwaisteDaten {
            get => _VerwaisteDaten;
            set {
                if (_VerwaisteDaten == value) { return; }
                AddPending(enDatabaseDataType.VerwaisteDaten, -1, -1, ((int)_VerwaisteDaten).ToString(), ((int)value).ToString(), true);
            }
        }

        [Browsable(false)]
        public string ZeilenQuickInfo {
            get => _ZeilenQuickInfo;
            set {
                if (_ZeilenQuickInfo == value) { return; }
                AddPending(enDatabaseDataType.ZeilenQuickInfo, -1, -1, _ZeilenQuickInfo, value, true);
            }
        }

        #endregion

        #region Methods

        public static Database GetByFilename(string filename, bool checkOnlyFilenameToo, bool read_only) {
            var tmpDB = GetByFilename(filename, checkOnlyFilenameToo);

            if (tmpDB is Database db) { return db; }

            if (tmpDB != null) { return null; }//  Daten im Speicher, aber keine Datenbank!

            return !FileExists(filename) ? null : new Database(filename, read_only, false);
        }

        public static Database LoadResource(Assembly assembly, string Name, string BlueBasicsSubDir, bool FehlerAusgeben, bool MustBeStream) {
            if (Develop.IsHostRunning() && !MustBeStream) {
                var x = -1;
                string pf;
                do {
                    x++;
                    pf = string.Empty;
                    switch (x) {
                        case 0:
                            // BeCreative, At Home, 31.11.2021
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
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;

                        case 7:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;

                        case 8:
                            // warscheinlich BeCreative, Firma
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;

                        case 9:
                            // Bildzeichen-Liste, Firma, 25.10.2021
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + BlueBasicsSubDir + "\\" + Name;
                            break;
                    }
                    if (FileExists(pf)) {
                        var tmp = GetByFilename(pf, false, false);
                        if (tmp != null) { return tmp; }
                        tmp = new Database(pf, false, false);
                        return tmp;
                    }
                } while (pf != string.Empty);
            }
            var d = Generic.GetEmmbedResource(assembly, Name);
            if (d != null) { return new Database(d); }
            if (FehlerAusgeben) { Develop.DebugPrint(enFehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + BlueBasicsSubDir + " - " + Name); }
            return null;
        }

        public string AdditionaFilesPfadWhole() {
            // @ ist ein erkennungszeichen, dass der Pfad schon geprüft wurde, aber nicht vorhanden ist
            if (_AdditionaFilesPfadtmp == "@") { return string.Empty; }
            if (!string.IsNullOrEmpty(_AdditionaFilesPfadtmp)) { return _AdditionaFilesPfadtmp; }
            var t = _AdditionaFilesPfad.CheckPath();
            if (PathExists(t)) {
                _AdditionaFilesPfadtmp = t;
                return t;
            }

            t = (Filename.FilePath() + _AdditionaFilesPfad.Trim("\\") + "\\").CheckPath();
            if (PathExists(t)) {
                _AdditionaFilesPfadtmp = t;
                return t;
            }
            _AdditionaFilesPfadtmp = "@";
            return string.Empty;
        }

        public List<string> AllConnectedFilesLCase() {
            List<string> Column_All = new();
            foreach (var ThisColumnItem in Column) {
                if (ThisColumnItem != null) {
                    if (ThisColumnItem.Format == enDataFormat.Link_To_Filesystem) {
                        var tmp = ThisColumnItem.Contents();
                        foreach (var thisTmp in tmp) {
                            Column_All.AddIfNotExists(ThisColumnItem.BestFile(thisTmp, false).ToLower());
                        }
                    }
                }
            }
            return Column_All.SortedDistinctList();
        }

        public List<RowData> AllRows() {
            var sortedRows = new List<RowData>();
            foreach (var thisRowItem in Row) {
                if (thisRowItem != null) {
                    sortedRows.Add(new RowData(thisRowItem));
                }
            }
            return sortedRows;
        }

        public string DefaultLayoutPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Layouts\\";

        public override void DiscardPendingChanges() => ChangeWorkItems(enItemState.Pending, enItemState.Undo);

        public override string ErrorReason(enErrorReason mode) {
            var f = base.ErrorReason(mode);
            return !string.IsNullOrEmpty(f)
                ? f
                : mode == enErrorReason.OnlyRead
                ? string.Empty
                : int.Parse(LoadedVersion.Replace(".", "")) > int.Parse(DatabaseVersion.Replace(".", ""))
                ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
                : string.Empty;
        }

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="column">Die Spalte, die zurückgegeben wird.</param>
        /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle Zeilen zurück.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, ColumnItem column, List<RowData> sortedRows) =>
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_CSV(firstRow, new List<ColumnItem>() { column }, sortedRows);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="columnList">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
        /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, List<ColumnItem> columnList, List<RowData> sortedRows) {
            if (columnList == null) {
                columnList = new List<ColumnItem>();
                foreach (var ThisColumnItem in Column) {
                    if (ThisColumnItem != null) {
                        columnList.Add(ThisColumnItem);
                    }
                }
            }

            if (sortedRows == null) { sortedRows = AllRows(); }

            StringBuilder sb = new();
            switch (firstRow) {
                case enFirstRow.Without:
                    break;

                case enFirstRow.ColumnCaption:
                    for (var ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            var tmp = columnList[ColNr].ReadableText();
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
                    for (var ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            sb.Append(columnList[ColNr].Name);
                            if (ColNr < columnList.Count - 1) { sb.Append(';'); }
                        }
                    }
                    sb.Append("\r\n");
                    break;

                default:
                    Develop.DebugPrint(firstRow);
                    break;
            }
            foreach (var ThisRow in sortedRows) {
                if (ThisRow != null) {
                    for (var ColNr = 0; ColNr < columnList.Count; ColNr++) {
                        if (columnList[ColNr] != null) {
                            var tmp = Cell.GetString(columnList[ColNr], ThisRow.Row);
                            tmp = tmp.Replace("\r\n", "|");
                            tmp = tmp.Replace("\r", "|");
                            tmp = tmp.Replace("\n", "|");
                            tmp = tmp.Replace(";", "<sk>");
                            sb.Append(tmp);
                            if (ColNr < columnList.Count - 1) { sb.Append(';'); }
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
        /// <param name="arrangement">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
        /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, ColumnViewCollection arrangement, List<RowData> sortedRows) =>
            Export_CSV(firstRow, arrangement.ListOfUsedColumn(), sortedRows);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(enFirstRow firstRow, int arrangementNo, FilterCollection filter, List<RowItem> pinned) =>
            Export_CSV(firstRow, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned));

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, int arrangementNo, FilterCollection filter, List<RowItem> pinned) =>
            Export_HTML(filename, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned), false);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, List<ColumnItem> columnList, List<RowData> sortedRows, bool execute) {
            if (columnList == null || columnList.Count == 0) {
                columnList = new List<ColumnItem>();
                foreach (var ThisColumnItem in Column) {
                    if (ThisColumnItem != null) {
                        columnList.Add(ThisColumnItem);
                    }
                }
            }

            if (sortedRows == null) { sortedRows = AllRows(); }

            if (string.IsNullOrEmpty(filename)) {
                filename = TempFile(string.Empty, "Export", "html");
            }

            HTML da = new(Filename.FileNameWithoutSuffix());
            da.AddCaption(_Caption);
            da.TableBeginn();
            da.RowBeginn();
            foreach (var ThisColumn in columnList) {
                if (ThisColumn != null) {
                    da.CellAdd(ThisColumn.ReadableText().Replace(";", "<br>"), ThisColumn.BackColor);
                    //da.Add("        <th bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"><b>" + ThisColumn.ReadableText().Replace(";", "<br>") + "</b></th>");
                }
            }
            da.RowEnd();
            foreach (var ThisRow in sortedRows) {
                if (ThisRow != null) {
                    da.RowBeginn();
                    foreach (var ThisColumn in columnList) {
                        if (ThisColumn != null) {
                            var LCColumn = ThisColumn;
                            var LCrow = ThisRow.Row;
                            if (ThisColumn.Format == enDataFormat.LinkedCell) {
                                (LCColumn, LCrow) = CellCollection.LinkedCellData(ThisColumn, ThisRow.Row, false, false);
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
            foreach (var ThisColumn in columnList) {
                if (ThisColumn != null) {
                    var s = ThisColumn.Summe(sortedRows);
                    if (s == null) {
                        da.CellAdd("-", ThisColumn.BackColor);
                        //da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                    } else {
                        da.CellAdd("~sum~ " + s, ThisColumn.BackColor);
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
        public void Export_HTML(string filename, ColumnViewCollection arrangement, List<RowData> sortedRows, bool execute) => Export_HTML(filename, arrangement.ListOfUsedColumn(), sortedRows, execute);

        public override bool HasPendingChanges() {
            try {
                if (ReadOnly) { return false; }
                foreach (var ThisWork in Works) {
                    if (ThisWork.State == enItemState.Pending) { return true; }
                }
                return false;
            } catch {
                return HasPendingChanges();
            }
        }

        public string Import(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart, bool dorowautmatic) {
            // Vorbereitung des Textes -----------------------------
            importText = importText.Replace("\r\n", "\r").Trim("\r");
            var ein = importText.SplitAndCutByCR();
            List<string[]> Zeil = new();
            var neuZ = 0;
            for (var z = 0; z <= ein.GetUpperBound(0); z++) {
                if (eliminateMultipleSplitter) {
                    ein[z] = ein[z].Replace(splitChar + splitChar, splitChar);
                }
                if (eleminateSplitterAtStart) {
                    ein[z] = ein[z].TrimStart(splitChar);
                }
                ein[z] = ein[z].TrimEnd(splitChar);
                Zeil.Add(ein[z].SplitAndCutBy(splitChar));
            }
            if (Zeil.Count == 0) {
                OnDropMessage("Import kann nicht ausgeführt werden.");
                return "Import kann nicht ausgeführt werden.";
            }
            List<ColumnItem> columns = new();
            var StartZ = 0;
            // -------------------------------------
            // --- Spalten-Reihenfolge ermitteln ---
            // -------------------------------------
            if (spalteZuordnen) {
                StartZ = 1;
                for (var SpaltNo = 0; SpaltNo < Zeil[0].GetUpperBound(0) + 1; SpaltNo++) {
                    if (string.IsNullOrEmpty(Zeil[0][SpaltNo])) {
                        OnDropMessage("Abbruch,<br>leerer Spaltenname.");
                        return "Abbruch,<br>leerer Spaltenname.";
                    }
                    Zeil[0][SpaltNo] = Zeil[0][SpaltNo].Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);
                    var Col = Column.Exists(Zeil[0][SpaltNo]);
                    if (Col == null) {
                        Col = Column.Add(Zeil[0][SpaltNo]);
                        Col.Caption = Zeil[0][SpaltNo];
                        Col.Format = enDataFormat.Text;
                    }
                    columns.Add(Col);
                }
            } else {
                foreach (var thisColumn in Column) {
                    if (thisColumn != null && string.IsNullOrEmpty(thisColumn.Identifier)) { columns.Add(thisColumn); }
                }
                while (columns.Count < Zeil[0].GetUpperBound(0) + 1) {
                    var newc = Column.Add();
                    newc.Caption = newc.Name;
                    newc.Format = enDataFormat.Text;
                    newc.MultiLine = true;
                    columns.Add(newc);
                }
            }
            // -------------------------------------
            // --- Importieren ---
            // -------------------------------------
            //OnDropMessage("Starte Importierevorgang...");
            for (var ZeilNo = StartZ; ZeilNo < Zeil.Count; ZeilNo++) {
                //P?.Update(ZeilNo);
                var tempVar2 = Math.Min(Zeil[ZeilNo].GetUpperBound(0) + 1, columns.Count);
                RowItem row = null;
                for (var SpaltNo = 0; SpaltNo < tempVar2; SpaltNo++) {
                    if (SpaltNo == 0) {
                        row = null;
                        if (zeileZuordnen && !string.IsNullOrEmpty(Zeil[ZeilNo][SpaltNo])) { row = Row[Zeil[ZeilNo][SpaltNo]]; }
                        if (row == null && !string.IsNullOrEmpty(Zeil[ZeilNo][SpaltNo])) {
                            row = Row.Add(Zeil[ZeilNo][SpaltNo]);
                            neuZ++;
                        }
                    } else {
                        if (row != null) {
                            row.CellSet(columns[SpaltNo], Zeil[ZeilNo][SpaltNo].SplitAndCutBy("|").JoinWithCr());
                        }
                    }
                    if (row != null && dorowautmatic) { row.DoAutomatic(true, true, false, "import"); }
                }
            }
            OnDropMessage("<b>Import abgeschlossen.</b>\r\n" + neuZ.ToString() + " neue Zeilen erstellt.");
            return string.Empty;
        }

        /// <summary>
        /// Fügt Comandos manuell hinzu. Vorsicht: Kann Datenbank beschädigen
        /// </summary>
        public void InjectCommand(enDatabaseDataType Comand, string ChangedTo) => AddPending(Comand, -1, -1, string.Empty, ChangedTo, true);

        public bool IsAdministrator() => DatenbankAdmin.Contains("#User: " + UserName, false)
                                        || (!string.IsNullOrEmpty(UserGroup) && DatenbankAdmin.Contains(UserGroup, false))
                                        || UserGroup.ToUpper() == "#ADMINISTRATOR";

        public void Parse(byte[] _BLoaded, ref int Pointer, ref enDatabaseDataType Art, ref int ColNR, ref int RowNR, ref string Wert, ref int X, ref int Y) {
            int Les;
            switch ((enRoutinen)_BLoaded[Pointer]) {
                case enRoutinen.CellFormat: {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = NummerCode3(_BLoaded, Pointer + 5);
                        RowNR = NummerCode3(_BLoaded, Pointer + 8);
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 11, b, 0, Les);
                        Wert = b.ToStringWIN1252();
                        X = NummerCode2(_BLoaded, Pointer + 11 + Les);
                        Y = NummerCode2(_BLoaded, Pointer + 11 + Les + 2);
                        Pointer += 11 + Les + 4;
                        break;
                    }
                case enRoutinen.CellFormatUTF8: {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = NummerCode3(_BLoaded, Pointer + 5);
                        RowNR = NummerCode3(_BLoaded, Pointer + 8);
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 11, b, 0, Les);
                        Wert = b.ToStringUTF8();
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
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 5, b, 0, Les);
                        Wert = b.ToStringWIN1252();
                        X = 0;
                        Y = 0;
                        Pointer += 5 + Les;
                        break;
                    }
                case enRoutinen.DatenAllgemeinUTF8: {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = -1;
                        RowNR = -1;
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 5, b, 0, Les);
                        Wert = b.ToStringUTF8();
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
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 11, b, 0, Les);
                        Wert = b.ToStringWIN1252();
                        X = 0;
                        Y = 0;
                        Pointer += 11 + Les;
                        break;
                    }
                case enRoutinen.ColumnUTF8: {
                        Art = (enDatabaseDataType)_BLoaded[Pointer + 1];
                        Les = NummerCode3(_BLoaded, Pointer + 2);
                        ColNR = NummerCode3(_BLoaded, Pointer + 5);
                        RowNR = NummerCode3(_BLoaded, Pointer + 8);
                        var b = new byte[Les];
                        Buffer.BlockCopy(_BLoaded, Pointer + 11, b, 0, Les);
                        Wert = b.ToStringUTF8();
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

        public List<string> Permission_AllUsed(bool cellLevel) {
            List<string> e = new();
            foreach (var ThisColumnItem in Column) {
                if (ThisColumnItem != null) {
                    e.AddRange(ThisColumnItem.PermissionGroups_ChangeCell);
                }
            }
            e.AddRange(PermissionGroups_NewRow);
            e.AddRange(DatenbankAdmin);
            foreach (var ThisArrangement in ColumnArrangements) {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }
            foreach (var ThisArrangement in Views) {
                e.AddRange(ThisArrangement.PermissionGroups_Show);
            }
            e.Add("#Everybody");
            e.Add("#User: " + UserName);
            if (cellLevel) {
                e.Add("#RowCreator");
            } else {
                e.RemoveString("#RowCreator", false);
            }
            e.RemoveString("#Administrator", false);
            if (!IsAdministrator()) { e.Add(UserGroup); }
            return e.SortedDistinctList();
        }

        public bool PermissionCheck(ListExt<string> allowed, RowItem row) {
            try {
                if (IsAdministrator()) { return true; }
                if (allowed == null || allowed.Count == 0) { return false; }
                foreach (var ThisString in allowed) {
                    if (PermissionCheckWithoutAdmin(ThisString, row)) { return true; }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(enFehlerArt.Warnung, ex);
            }
            return false;
        }

        public override void RepairAfterParse() {
            // System-Spalten checken und alte Formate auf neuen Stand bringen
            Column.Repair();
            // Evtl. Defekte Rows reparieren
            Row.Repair();
            //Defekte Ansichten reparieren - Teil 1
            for (var z = 0; z <= 1; z++) {
                if (ColumnArrangements.Count < z + 1) {
                    ColumnArrangements.Add(new ColumnViewCollection(this, ""));
                }
                if (string.IsNullOrEmpty(ColumnArrangements[z].Name)) {
                    switch (z) {
                        case 0:
                            ColumnArrangements[z].Name = "Alle Spalten";
                            if (ColumnArrangements[z].Count < 1) { ColumnArrangements[z].ShowAllColumns(); }
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

        public string UndoText(ColumnItem column, RowItem row) {
            if (Works == null || Works.Count == 0) { return string.Empty; }
            var CellKey = CellCollection.KeyOfCell(column, row);
            var t = "";
            for (var z = Works.Count - 1; z >= 0; z--) {
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

        internal void AddPending(enDatabaseDataType Comand, ColumnItem column, string PreviousValue, string ChangedTo, bool ExecuteNow) => AddPending(Comand, column.Key, -1, PreviousValue, ChangedTo, ExecuteNow);

        internal void AddPending(enDatabaseDataType Comand, int ColumnKey, string ListExt, bool ExecuteNow) => AddPending(Comand, ColumnKey, -1, "", ListExt, ExecuteNow);

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

            if (Comand != enDatabaseDataType.AutoExport) { SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

            if (RowKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
            if (ColumnKey < -100) { Develop.DebugPrint(enFehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }
            Works.Add(new WorkItem(Comand, ColumnKey, RowKey, PreviousValue, ChangedTo, UserName));
        }

        internal void Column_NameChanged(string oldName, ColumnItem newName) {
            if (string.IsNullOrEmpty(oldName)) { return; }
            // Cells ----------------------------------------------
            //   Cell.ChangeCaptionName(OldName, cColumnItem.Name, cColumnItem)
            //  Undo -----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind
            // Layouts -----------------------------------------
            // Werden über das Skript gesteuert
            // Sortierung -----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind
            // _ColumnArrangements-----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind
            // _Views-----------------------------------------
            // Nicht nötig, da die Spalten als Verweiß gespeichert sind
            // Zeilen-Quick-Info -----------------------------------------
            ZeilenQuickInfo = ZeilenQuickInfo.Replace("~" + oldName + ";", "~" + newName.Name + ";", RegexOptions.IgnoreCase);
            ZeilenQuickInfo = ZeilenQuickInfo.Replace("~" + oldName + "(", "~" + newName.Name + "(", RegexOptions.IgnoreCase);
        }

        internal string Column_UsedIn(ColumnItem column) {
            var t = string.Empty;
            if (SortDefinition.Columns.Contains(column)) { t += " - Sortierung<br>"; }
            var view = false;
            foreach (var thisView in Views) {
                if (thisView[column] != null) { view = true; }
            }
            if (view) { t += " - Formular-Ansichten<br>"; }
            var cola = false;
            var first = true;
            foreach (var thisView in ColumnArrangements) {
                if (!first && thisView[column] != null) { cola = true; }
                first = false;
            }
            if (cola) { t += " - Benutzerdefinierte Spalten-Anordnungen<br>"; }
            if (RulesScript.ToUpper().Contains(column.Name.ToUpper())) { t += " - Regeln-Skript<br>"; }
            if (ZeilenQuickInfo.ToUpper().Contains(column.Name.ToUpper())) { t += " - Zeilen-Quick-Info<br>"; }
            if (Tags.JoinWithCr().ToUpper().Contains(column.Name.ToUpper())) { t += " - Datenbank-Tags<br>"; }
            var layout = false;
            foreach (var thisLayout in Layouts) {
                if (thisLayout.Contains(column.Name.ToUpper())) { layout = true; }
            }
            if (layout) { t += " - Layouts<br>"; }
            var l = column.Contents();
            if (l.Count > 0) {
                t = t + "<br><br><b>Zusatz-Info:</b><br>";
                t = t + " - Befüllt mit " + l.Count.ToString() + " verschiedenen Werten";
            }
            return t;
        }

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

        internal void OnDropMessage(string message) {
            if (Disposed) { return; }
            DropMessage?.Invoke(this, new MessageEventArgs(message));
        }

        internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventargs e) {
            if (Disposed) { return; }
            GenerateLayoutInternal?.Invoke(this, e);
        }

        internal void OnProgressbarInfo(ProgressbarEventArgs e) {
            if (Disposed) { return; }
            ProgressbarInfo?.Invoke(this, e);
        }

        internal void OnScriptError(RowCancelEventArgs e) {
            if (Disposed) { return; }
            ScriptError?.Invoke(this, e);
        }

        internal void OnViewChanged() {
            if (Disposed) { return; }
            ViewChanged?.Invoke(this, System.EventArgs.Empty);
        }

        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content) {
            var b = Content.UTF8_ToByte();
            List.Add((byte)enRoutinen.DatenAllgemeinUTF8);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, b.Length, 3);
            List.AddRange(b);
        }

        internal void SaveToByteList(List<byte> List, KeyValuePair<string, CellItem> vCell) {
            if (string.IsNullOrEmpty(vCell.Value.Value)) { return; }
            Cell.DataOfCellKey(vCell.Key, out var tColumn, out var tRow);
            if (!tColumn.SaveContent) { return; }
            var b = vCell.Value.Value.UTF8_ToByte();
            var tx = enDatabaseDataType.ce_Value_withSizeData;
            List.Add((byte)enRoutinen.CellFormatUTF8);
            List.Add((byte)tx);
            SaveToByteList(List, b.Length, 3);
            SaveToByteList(List, tColumn.Key, 3);
            SaveToByteList(List, tRow.Key, 3);
            List.AddRange(b);
            var ContentSize = Cell.ContentSizeToSave(vCell, tColumn);
            SaveToByteList(List, ContentSize.Width, 2);
            SaveToByteList(List, ContentSize.Height, 2);
        }

        internal void SaveToByteList(List<byte> List, enDatabaseDataType DatabaseDataType, string Content, int TargetColumNr) {
            var b = Content.UTF8_ToByte();
            List.Add((byte)enRoutinen.ColumnUTF8);
            List.Add((byte)DatabaseDataType);
            SaveToByteList(List, b.Length, 3);
            SaveToByteList(List, TargetColumNr, 3);
            SaveToByteList(List, 0, 3); //Zeile-Unötig
            List.AddRange(b);
        }

        protected override bool BlockSaveOperations() {
            if (RowItem.DoingScript) { return true; }

            return base.BlockSaveOperations();
        }

        protected override void CheckDataAfterReload() {
            try {
                // Leztes WorkItem suchen. Auch Ohne LogUndo MUSS es vorhanden sein.
                if (!string.IsNullOrEmpty(_LastWorkItem)) {
                    var ok = false;
                    var ok2 = string.Empty;
                    foreach (var ThisWorkItem in Works) {
                        var tmp = ThisWorkItem.ToString();
                        if (tmp == _LastWorkItem) {
                            ok = true;
                            break;
                        } else if (tmp.Substring(7) == _LastWorkItem.Substring(7)) {
                            ok2 = tmp;
                        }
                    }
                    if (!ok && string.IsNullOrEmpty(ok2)) {
                        if (!Filename.Contains("AutoVue") && !Filename.Contains("Plandaten") && !Filename.Contains("Ketten.") && !Filename.Contains("Kettenräder.") && !Filename.Contains("TVW") && !Filename.Contains("Work")) {
                            Develop.DebugPrint(enFehlerArt.Warnung, "WorkItem verschwunden<br>" + _LastWorkItem + "<br>" + Filename + "<br><br>Vorher:<br>" + _WorkItemsBefore + "<br><br>Nachher:<br>" + Works.ToString());
                        }
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        protected override void Dispose(bool disposing) {
            if (!Disposed) { return; }
            base.Dispose(disposing); // speichert und löscht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
            if (disposing) {
                // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
            }
            // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
            // TODO: große Felder auf Null setzen.
            ColumnArrangements.Changed -= ColumnArrangements_ListOrItemChanged;
            Layouts.Changed -= Layouts_ListOrItemChanged;
            Layouts.ItemSeted -= Layouts_ItemSeted;
            Views.Changed -= Views_ListOrItemChanged;
            PermissionGroups_NewRow.Changed -= PermissionGroups_NewRow_ListOrItemChanged;
            Tags.Changed -= DatabaseTags_ListOrItemChanged;
            Export.Changed -= Export_ListOrItemChanged;
            DatenbankAdmin.Changed -= DatabaseAdmin_ListOrItemChanged;
            Row.RowRemoving -= Row_RowRemoving;
            Row.RowAdded -= Row_RowAdded;
            Column.ItemRemoving -= Column_ItemRemoving;
            Column.ItemRemoved -= Column_ItemRemoved;
            Column.Dispose();
            Cell.Dispose();
            Row.Dispose();
            Works.Dispose();
            ColumnArrangements.Dispose();
            Views.Dispose();
            Tags.Dispose();
            Export.Dispose();
            DatenbankAdmin.Dispose();
            PermissionGroups_NewRow.Dispose();
            Layouts.Dispose();
        }

        protected override void DoBackGroundWork(BackgroundWorker listenToMyCancel) {
            if (ReadOnly) { return; }

            foreach (var thisExport in Export) {
                if (listenToMyCancel.CancellationPending) { return; }

                if (thisExport.IsOk()) {
                    if (!HasPendingChanges()) {
                        CancelEventArgs ec = new(false);
                        OnExporting(ec);
                        if (ec.Cancel) { return; }
                    }

                    thisExport.DeleteOutdatedBackUps(listenToMyCancel);
                    if (listenToMyCancel.CancellationPending) { return; }
                    thisExport.DoBackUp(listenToMyCancel);
                    if (listenToMyCancel.CancellationPending) { return; }
                }
            }
        }

        protected override void DoWorkAfterSaving() {
            ChangeWorkItems(enItemState.Pending, enItemState.Undo);
            var FilesNewLCase = AllConnectedFilesLCase();
            List<string> Writer_FilesToDeleteLCase = new();
            if (_VerwaisteDaten == enVerwaisteDaten.Löschen) {
                Writer_FilesToDeleteLCase = FilesAfterLoadingLCase.Except(FilesNewLCase).ToList();
            }
            FilesAfterLoadingLCase.Clear();
            FilesAfterLoadingLCase.AddRange(FilesNewLCase);
            if (Writer_FilesToDeleteLCase.Count > 0) { DeleteFile(Writer_FilesToDeleteLCase); }
        }

        protected override bool IsThereBackgroundWorkToDo() {
            if (HasPendingChanges()) { return true; }
            CancelEventArgs ec = new(false);
            OnExporting(ec);
            if (ec.Cancel) { return false; }

            foreach (var ThisExport in Export) {
                if (ThisExport != null) {
                    if (ThisExport.Typ == enExportTyp.EinzelnMitFormular) { return true; }
                    if (DateTime.UtcNow.Subtract(ThisExport.LastExportTimeUTC).TotalDays > ThisExport.BackupInterval) { return true; }
                }
            }
            return false;
        }

        protected override void ParseExternal(byte[] B) {
            Column.ThrowEvents = false;
            enDatabaseDataType Art = 0;
            var Pointer = 0;
            var Inhalt = "";
            ColumnItem _Column = null;
            RowItem _Row = null;
            var X = 0;
            var Y = 0;
            var ColKey = 0;
            var RowKey = 0;
            List<ColumnItem> ColumnsOld = new();
            ColumnsOld.AddRange(Column);
            Column.Clear();
            List<WorkItem> OldPendings = new();
            foreach (var ThisWork in Works) {
                if (ThisWork.State == enItemState.Pending) { OldPendings.Add(ThisWork); }
            }
            Works.Clear();
            do {
                if (Pointer >= B.Count()) { break; }
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
                        // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzufügen
                        foreach (var ThisColumn in ColumnsOld) {
                            if (ThisColumn != null && ThisColumn.Key == ColKey) {
                                _Column = ThisColumn;
                            }
                        }
                        if (_Column != null) {
                            // Prima, gefunden! Noch die Collections korrigieren
                            Column.AddFromParser(_Column);
                            ColumnsOld.Remove(_Column);
                        } else {
                            // Nicht gefunden, als neu machen
                            _Column = Column.Add(ColKey);
                        }
                    }
                }
                if (Art == enDatabaseDataType.CryptionState) {
                    if (Inhalt.FromPlusMinus()) {
                        PasswordEventArgs e = new();
                        OnNeedPassword(e);
                        B = Cryptography.SimpleCrypt(B, e.Password, -1, Pointer, B.Length - 1);
                        if (B[Pointer + 1] != 3 || B[Pointer + 2] != 0 || B[Pointer + 3] != 0 || B[Pointer + 4] != 2 || B[Pointer + 5] != 79 || B[Pointer + 6] != 75) {
                            RemoveFilename();
                            LoadedVersion = "9.99";
                            //MessageBox.Show("Zugriff verweigrt, Passwort falsch!", enImageCode.Kritisch, "OK");
                            break;
                        }
                    }
                }
                var _Fehler = ParseThis(Art, Inhalt, _Column, _Row, X, Y);
                if (Art == enDatabaseDataType.EOF) { break; }
                if (!string.IsNullOrEmpty(_Fehler)) {
                    LoadedVersion = "9.99";
                    Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + Filename + "<br>Meldung: " + _Fehler);
                }
            } while (true);
            // Spalten, die nach dem Reload nicht mehr benötigt werden, löschen
            //ColumnsOld.DisposeAndRemoveAll();
            Row.RemoveNullOrEmpty();
            //Row.RemoveNullOrDisposed();
            Cell.RemoveOrphans();
            //LoadPicsIntoImageChache();
            FilesAfterLoadingLCase.Clear();
            FilesAfterLoadingLCase.AddRange(AllConnectedFilesLCase());
            Works.AddRange(OldPendings);
            OldPendings.Clear();
            ExecutePending();
            Column.ThrowEvents = true;
            if (int.Parse(LoadedVersion.Replace(".", "")) > int.Parse(DatabaseVersion.Replace(".", ""))) { SetReadOnly(); }
        }

        protected override void PrepeareDataForCheckingBeforeLoad() {
            // Letztes WorkItem speichern, als Kontrolle
            _WorkItemsBefore = string.Empty;
            _LastWorkItem = string.Empty;
            if (Works != null && Works.Count > 0) {
                var c = 0;
                do {
                    c++;
                    if (c > 20 || Works.Count - c < 20) { break; }
                    var wn = Works.Count - c;
                    if (Works[wn].LogsUndo(this) && Works[wn].HistorischRelevant) { _LastWorkItem = Works[wn].ToString(); }
                } while (string.IsNullOrEmpty(_LastWorkItem));
                _WorkItemsBefore = Works.ToString();
            }
        }

        protected override byte[] ToListOfByte() {
            try {
                var CryptPos = -1;
                List<byte> l = new();
                // Wichtig, Reihenfolge und Länge NIE verändern!
                SaveToByteList(l, enDatabaseDataType.Formatkennung, "BlueDatabase");
                SaveToByteList(l, enDatabaseDataType.Version, DatabaseVersion);
                SaveToByteList(l, enDatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
                                                                                                                                                                                                                                                                    // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
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
                //SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
                SaveToByteList(l, enDatabaseDataType.RulesScript, _RulesScript);
                //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
                SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _FilterImagePfad);
                SaveToByteList(l, enDatabaseDataType.AdditionaFilesPfad, _AdditionaFilesPfad);
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
                // Beim Erstellen des Undo-Speichers die Works nicht verändern, da auch bei einem nicht
                // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
                // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
                List<string> Works2 = new();
                foreach (var thisWorkItem in Works) {
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
                SaveToByteList(l, enDatabaseDataType.UndoInOne, Works2.JoinWithCr((int)(16581375 * 0.9)));
                SaveToByteList(l, enDatabaseDataType.EOF, "END");
                return CryptPos > 0 ? Cryptography.SimpleCrypt(l.ToArray(), _GlobalShowPass, 1, CryptPos, l.Count - 1) : l.ToArray();
            } catch {
                return ToListOfByte();
            }
        }

        private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

        private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

        private void ChangeColumnKeyInPending(int oldKey, int newKey) {
            foreach (var ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {
                    if (ThisPending.ColKey == oldKey) {
                        if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
                        ThisPending.ColKey = newKey; // Generell den Schlüssel ändern
                        if (_LastWorkItem == "X") {
                            _LastWorkItem = ThisPending.ToString();
                            Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem geändert: " + _LastWorkItem);
                        }
                        switch (ThisPending.Comand) {
                            case enDatabaseDataType.AddColumn:
                            case enDatabaseDataType.dummyComand_RemoveColumn:
                                ThisPending.ChangedTo = newKey.ToString();
                                break;

                            default:
                                if (ThisPending.PreviousValue.Contains(ColumnCollection.ParsableColumnKey(oldKey))) {
                                    Develop.DebugPrint("Replace machen (Old): " + oldKey);
                                }
                                if (ThisPending.ChangedTo.Contains(ColumnCollection.ParsableColumnKey(oldKey))) {
                                    Develop.DebugPrint("Replace machen (New): " + oldKey);
                                }
                                break;
                        }
                    }
                }
                OnColumnKeyChanged(new KeyChangedEventArgs(oldKey, newKey));
            }
        }

        private void ChangeRowKeyInPending(int OldKey, int NewKey) {
            foreach (var ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {
                    if (ThisPending.RowKey == OldKey) {
                        if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
                        ThisPending.RowKey = NewKey; // Generell den Schlüssel ändern
                        if (_LastWorkItem == "X") {
                            _LastWorkItem = ThisPending.ToString();
                            Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem geändert: " + _LastWorkItem);
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

        private void ChangeWorkItems(enItemState OldState, enItemState NewState) {
            foreach (var ThisWork in Works) {
                if (ThisWork.State == OldState) { ThisWork.State = NewState; }
            }
        }

        private void CheckViewsAndArrangements() {
            if (ReadOnly) { return; }
            foreach (var ThisCol in ColumnArrangements) {
                ThisCol.Repair();
            }
            foreach (var ThisCol in Views) {
                ThisCol.Repair();
            }
            if (Views != null) {
                if (Views.Count > 0 && Views[0].PermissionGroups_Show.Count > 0) { Views[0].PermissionGroups_Show.Clear(); }
                if (Views.Count > 1 && !Views[1].PermissionGroups_Show.Contains("#Everybody")) { Views[1].PermissionGroups_Show.Add("#Everybody"); }
            }
            Layouts.Check();
        }

        private void Column_ItemRemoved(object sender, System.EventArgs e) {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Parsing Falsch!"); }
            CheckViewsAndArrangements();
        }

        private void Column_ItemRemoving(object sender, ListEventArgs e) {
            var Key = ((ColumnItem)e.Item).Key;
            AddPending(enDatabaseDataType.dummyComand_RemoveColumn, Key, -1, string.Empty, Key.ToString(), false);
        }

        private void ColumnArrangements_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt werden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(), false);
        }

        private void DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.DatenbankAdmin, -1, DatenbankAdmin.JoinWithCr(), false);
        }

        private void DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
        }

        private void ExecutePending() {
            if (!IsParsing) { Develop.DebugPrint(enFehlerArt.Fehler, "Nur während des Parsens möglich"); }
            if (!HasPendingChanges()) { return; }
            // Erst die Neuen Zeilen / Spalten alle neutralisieren
            var dummy = -1000;
            foreach (var ThisPending in Works) {
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
            // Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
            foreach (var ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {
                    switch (ThisPending.Comand) {
                        case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
                                var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
                                var fRow = Row[Value];
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
                            ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey);
                            break;
                    }
                }
            }
            // Und nun alles ausführen!
            foreach (var ThisPending in Works) {
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
                            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ColumnItem _Col = null;
                if (ThisPendingItem.ColKey > -1) {
                    _Col = Column.SearchByKey(ThisPendingItem.ColKey);
                    if (_Col == null) {
                        if (ThisPendingItem.Comand != enDatabaseDataType.AddColumn && ThisPendingItem.User != UserName) {
                            Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + Filename + "<br>" + ThisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ParseThis(ThisPendingItem.Comand, ThisPendingItem.ChangedTo, _Col, _Row, 0, 0);
            }
        }

        private void Export_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.AutoExport, -1, Export.ToString(true), false);
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
            //Bins.Clear();
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
            _RulesScript = string.Empty;
            _GlobalScale = 1f;
            _Ansicht = enAnsicht.Unverändert;
            _FilterImagePfad = string.Empty;
            _AdditionaFilesPfad = "AdditionalFiles";
            _ZeilenQuickInfo = string.Empty;
            _sortDefinition = null;
        }

        private void InvalidateExports(string layoutID) {
            if (ReadOnly) { return; }

            foreach (var thisExport in Export) {
                if (thisExport != null) {
                    if (thisExport.Typ == enExportTyp.EinzelnMitFormular) {
                        if (thisExport.ExportFormularID == layoutID) {
                            thisExport.LastExportTimeUTC = new DateTime(1900, 1, 1);
                        }
                    }
                }
            }
        }

        private void Layouts_ItemSeted(object sender, ListEventArgs e) {
            if (e != null) {
                var x = (string)e.Item;
                if (!x.StartsWith("{ID=#")) { Develop.DebugPrint("ID nicht gefunden: " + x); }
                var ko = x.IndexOf(", ");
                var id = x.Substring(4, ko - 4);
                InvalidateExports(id);
            }
        }

        private void Layouts_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
        }

        private void OnColumnKeyChanged(KeyChangedEventArgs e) {
            if (Disposed) { return; }
            ColumnKeyChanged?.Invoke(this, e);
        }

        private void OnExporting(CancelEventArgs e) {
            if (Disposed) { return; }
            Exporting?.Invoke(this, e);
        }

        private void OnNeedPassword(PasswordEventArgs e) {
            if (Disposed) { return; }
            NeedPassword?.Invoke(this, e);
        }

        private void OnRowKeyChanged(KeyChangedEventArgs e) {
            if (Disposed) { return; }
            RowKeyChanged?.Invoke(this, e);
        }

        private void OnSortParameterChanged() {
            if (Disposed) { return; }
            SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private string ParseThis(enDatabaseDataType type, string value, ColumnItem column, RowItem row, int width, int height) {
            if (type is >= enDatabaseDataType.Info_ColumDataSart and <= enDatabaseDataType.Info_ColumnDataEnd) {
                return column.Load(type, value);
            }
            switch (type) {
                case enDatabaseDataType.Formatkennung:
                    break;

                case enDatabaseDataType.Version:
                    LoadedVersion = value.Trim();
                    if (LoadedVersion != DatabaseVersion) {
                        Initialize();
                        LoadedVersion = value.Trim();
                    } else {
                        //Cell.RemoveOrphans();
                        Row.RemoveNullOrEmpty();
                        Cell.Clear();
                    }
                    break;

                case enDatabaseDataType.Werbung:
                    break;

                case enDatabaseDataType.CryptionState:
                    break;

                case enDatabaseDataType.CryptionTest:
                    break;

                case enDatabaseDataType.Creator:
                    _Creator = value;
                    break;

                case enDatabaseDataType.CreateDate:
                    _CreateDate = value;
                    break;

                case enDatabaseDataType.ReloadDelaySecond:
                    _ReloadDelaySecond = int.Parse(value);
                    break;

                case enDatabaseDataType.DatenbankAdmin:
                    DatenbankAdmin.SplitAndCutByCR_QuickSortAndRemoveDouble(value);
                    break;

                case enDatabaseDataType.SortDefinition:
                    _sortDefinition = new RowSortDefinition(this, value);
                    break;

                case enDatabaseDataType.Caption:
                    _Caption = value;
                    break;

                case enDatabaseDataType.GlobalScale:
                    _GlobalScale = double.Parse(value);
                    break;

                case enDatabaseDataType.FilterImagePfad:
                    _FilterImagePfad = value;
                    break;

                case enDatabaseDataType.AdditionaFilesPfad:
                    _AdditionaFilesPfad = value;
                    break;

                case enDatabaseDataType.ZeilenQuickInfo:
                    _ZeilenQuickInfo = value;
                    break;

                case enDatabaseDataType.Ansicht:
                    _Ansicht = (enAnsicht)int.Parse(value);
                    break;

                case enDatabaseDataType.Tags:
                    Tags.SplitAndCutByCR(value);
                    break;

                case enDatabaseDataType.BinaryDataInOne:
                    break;

                case enDatabaseDataType.Layouts:
                    Layouts.SplitAndCutByCR_QuickSortAndRemoveDouble(value);
                    break;

                case enDatabaseDataType.AutoExport:
                    Export.Clear();
                    List<string> AE = new(value.SplitAndCutByCR());
                    foreach (var t in AE) {
                        Export.Add(new ExportDefinition(this, t));
                    }
                    break;

                case enDatabaseDataType.Rules_ALT:
                    //var Rules = new List<RuleItem_Old>();
                    //var RU = content.SplitAndCutByCR();
                    //for (var z = 0; z <= RU.GetUpperBound(0); z++) {
                    //    Rules.Add(new RuleItem_Old(this, RU[z]));
                    //}
                    _RulesScript = string.Empty;
                    break;

                case enDatabaseDataType.ColumnArrangement:
                    ColumnArrangements.Clear();
                    List<string> CA = new(value.SplitAndCutByCR());
                    foreach (var t in CA) {
                        ColumnArrangements.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.Views:
                    Views.Clear();
                    List<string> VI = new(value.SplitAndCutByCR());
                    foreach (var t in VI) {
                        Views.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case enDatabaseDataType.PermissionGroups_NewRow:
                    PermissionGroups_NewRow.SplitAndCutByCR_QuickSortAndRemoveDouble(value);
                    break;

                case enDatabaseDataType.LastRowKey:
                    return Row.Load_310(type, value);

                case enDatabaseDataType.LastColumnKey:
                    return Column.Load_310(type, value);

                case enDatabaseDataType.GlobalShowPass:
                    _GlobalShowPass = value;
                    break;

                case (enDatabaseDataType)30:
                    // TODO: Entferne GlobalInfo
                    break;

                case (enDatabaseDataType)52:
                    // TODO: Entferne Skin
                    break;

                case enDatabaseDataType.JoinTyp:
                    _JoinTyp = (enJoinTyp)int.Parse(value);
                    break;

                case enDatabaseDataType.VerwaisteDaten:
                    _VerwaisteDaten = (enVerwaisteDaten)int.Parse(value);
                    break;

                case (enDatabaseDataType)63://                    enDatabaseDataType.ImportScript:
                    break;

                case enDatabaseDataType.RulesScript:
                    _RulesScript = value;
                    break;

                case enDatabaseDataType.FileEncryptionKey:
                    _FileEncryptionKey = value;
                    break;

                case enDatabaseDataType.ce_Value_withSizeData:
                case enDatabaseDataType.ce_UTF8Value_withSizeData:
                case enDatabaseDataType.ce_Value_withoutSizeData:
                    if (type == enDatabaseDataType.ce_UTF8Value_withSizeData) {
                        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var enc1252 = Encoding.GetEncoding(1252);
                        value = Encoding.UTF8.GetString(enc1252.GetBytes(value));
                    }
                    Cell.Load_310(column, row, value, width, height);
                    break;

                case enDatabaseDataType.UndoCount:
                    _UndoCount = int.Parse(value);
                    break;

                case enDatabaseDataType.UndoInOne:
                    Works.Clear();
                    var UIO = value.SplitAndCutByCR();
                    for (var z = 0; z <= UIO.GetUpperBound(0); z++) {
                        WorkItem tmpWork = new(UIO[z]) {
                            State = enItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos ändern.
                        };
                        Works.Add(tmpWork);
                    }
                    break;

                case enDatabaseDataType.dummyComand_AddRow:
                    var addRowKey = int.Parse(value);
                    if (Row.SearchByKey(addRowKey) == null) { Row.Add(new RowItem(this, addRowKey)); }
                    break;

                case enDatabaseDataType.AddColumn:
                    var addColumnKey = int.Parse(value);
                    if (Column.SearchByKey(addColumnKey) == null) { Column.AddFromParser(new ColumnItem(this, addColumnKey)); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveRow:
                    var removeRowKey = int.Parse(value);
                    if (Row.SearchByKey(removeRowKey) is RowItem) { Row.Remove(removeRowKey); }
                    break;

                case enDatabaseDataType.dummyComand_RemoveColumn:
                    var removeColumnKey = int.Parse(value);
                    if (Column.SearchByKey(removeColumnKey) is ColumnItem col) { Column.Remove(col); }
                    break;

                case enDatabaseDataType.EOF:
                    return string.Empty;

                default:
                    LoadedVersion = "9.99";
                    if (!ReadOnly) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Datei: " + Filename);
                    }
                    break;
            }
            return string.Empty;
        }

        private bool PermissionCheckWithoutAdmin(string allowed, RowItem row) {
            var tmpName = UserName.ToUpper();
            var tmpGroup = UserGroup.ToUpper();
            if (allowed.ToUpper() == "#EVERYBODY") {
                return true;
            } else if (allowed.ToUpper() == "#ROWCREATOR") {
                if (row != null && Cell.GetString(Column.SysRowCreator, row).ToUpper() == tmpName) { return true; }
            } else if (allowed.ToUpper() == "#USER: " + tmpName) {
                return true;
            } else if (allowed.ToUpper() == "#USER:" + tmpName) {
                return true;
            } else if (allowed.ToUpper() == tmpGroup) {
                return true;
            }
            return false;
        }

        private void PermissionGroups_NewRow_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.PermissionGroups_NewRow, -1, PermissionGroups_NewRow.JoinWithCr(), false);
        }

        private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
            if (e.BMP != null) { return; }
            try {
                if (string.IsNullOrWhiteSpace(AdditionaFilesPfadWhole())) { return; }
                if (FileExists(AdditionaFilesPfadWhole() + e.Name + ".png")) {
                    e.BMP = new BitmapExt(AdditionaFilesPfadWhole() + e.Name + ".png");
                }
            } catch { }
        }

        private void Row_RowAdded(object sender, RowEventArgs e) {
            if (!IsParsing) {
                AddPending(enDatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
            }
        }

        private void Row_RowRemoving(object sender, RowEventArgs e) => AddPending(enDatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);

        private void SaveToByteList(List<byte> list, int numberToAdd, int byteCount) {
            switch (byteCount) {
                case 3:
                    list.Add(Convert.ToByte(Math.Truncate(numberToAdd / 65025.0)));
                    list.Add(Convert.ToByte(Math.Truncate(numberToAdd % 65025 / 255.0)));
                    list.Add((byte)(numberToAdd % 65025 % 255));
                    break;

                case 2:
                    list.Add(Convert.ToByte(Math.Truncate(numberToAdd / 255.0)));
                    list.Add((byte)(numberToAdd % 255));
                    break;

                case 1:
                    list.Add((byte)numberToAdd);
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Byteanzahl unbekannt!");
                    break;
            }
        }

        private string SearchKeyValueInPendingsOf(int RowKey) {
            var F = string.Empty;
            foreach (var ThisPending in Works) {
                if (ThisPending.State == enItemState.Pending) {
                    if (ThisPending.RowKey == RowKey && ThisPending.Comand == enDatabaseDataType.ce_Value_withoutSizeData && ThisPending.ColKey == Column[0].Key) {
                        F = ThisPending.ChangedTo;
                    }
                }
            }
            return F;
        }

        private void Views_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt werden. Kann zu Endlosschleifen führen.
            AddPending(enDatabaseDataType.Views, -1, Views.ToString(true), false);
        }

        #endregion
    }
}