// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Threading.Tasks;
using static BlueBasics.FileOperations;
using static BlueBasics.Converter;
using static BlueScript.Script;
using BlueScript.Variables;

namespace BlueDatabase {

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Database : BlueBasics.MultiUserFile.MultiUserFile {

        #region Fields

        public const string DatabaseVersion = "4.00";

        public readonly CellCollection Cell;

        public readonly ColumnCollection Column;

        public readonly ListExt<ColumnViewCollection> ColumnArrangements = new();

        public readonly ListExt<string> DatenbankAdmin = new();

        /// <summary>
        /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
        /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
        /// </summary>
        public readonly ListExt<ExportDefinition?> Export = new();

        public readonly LayoutCollection Layouts = new();

        public readonly ListExt<string> PermissionGroupsNewRow = new();

        public readonly RowCollection Row;

        public readonly ListExt<string> Tags = new();

        public readonly string UserName = Generic.UserName().ToUpper();

        public readonly ListExt<ColumnViewCollection?> Views = new();

        public string UserGroup;

        public ListExt<WorkItem> Works;

        private readonly List<string> _filesAfterLoadingLCase;

        private string _additionaFilesPfad;

        private string _additionaFilesPfadtmp = string.Empty;

        private Ansicht _ansicht;

        private string _caption = string.Empty;

        private string _createDate = string.Empty;

        private string _creator = string.Empty;

        private string _fileEncryptionKey = string.Empty;

        //private string _filterImagePfad;

        private double _globalScale;

        private string _globalShowPass = string.Empty;

        //private enJoinTyp _JoinTyp;

        ///// <summary>
        ///// Variable nur temporär für den BinReloader, um mögliche Datenverluste zu entdecken.
        ///// </summary>
        //private string _LastWorkItem = string.Empty;

        private string _rulesScript = string.Empty;

        private RowSortDefinition? _sortDefinition;

        private int _undoCount;

        private VerwaisteDaten _verwaisteDaten;

        //private string _WorkItemsBefore = string.Empty;

        private string _zeilenQuickInfo = string.Empty;

        #endregion

        #region Constructors

        public Database(Stream stream) : this(stream, string.Empty, true, false) { }

        public Database(bool readOnly) : this(null, string.Empty, readOnly, true) { }

        public Database(string filename, bool readOnly, bool create) : this(null, filename, readOnly, create) { }

        private Database(Stream? stream, string filename, bool readOnly, bool create) : base(readOnly, true) {
            CultureInfo culture = new("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Cell = new CellCollection(this);

            Row = new RowCollection(this);
            Row.RowRemoving += Row_RowRemoving;
            Row.RowAdded += Row_RowAdded;

            Column = new ColumnCollection(this);
            Column.ItemRemoving += Column_ItemRemoving;
            Column.ItemRemoved += Column_ItemRemoved;
            Column.ItemAdded += Column_ItemAdded;

            Works = new ListExt<WorkItem>();
            _filesAfterLoadingLCase = new List<string>();
            ColumnArrangements.Changed += ColumnArrangements_ListOrItemChanged;
            Layouts.Changed += Layouts_ListOrItemChanged;
            Layouts.ItemSeted += Layouts_ItemSeted;
            Views.Changed += Views_ListOrItemChanged;
            PermissionGroupsNewRow.Changed += PermissionGroups_NewRow_ListOrItemChanged;
            Tags.Changed += DatabaseTags_ListOrItemChanged;
            Export.Changed += Export_ListOrItemChanged;
            DatenbankAdmin.Changed += DatabaseAdmin_ListOrItemChanged;

            Initialize();

            UserGroup = "#Administrator";
            if (!string.IsNullOrEmpty(filename)) {
                //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
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

        public event EventHandler<MessageEventArgs> DropMessage;

        public event CancelEventHandler Exporting;

        public event EventHandler<GenerateLayoutInternalEventargs> GenerateLayoutInternal;

        public event EventHandler<PasswordEventArgs> NeedPassword;

        public event EventHandler<ProgressbarEventArgs> ProgressbarInfo;

        public event EventHandler<RowCancelEventArgs> ScriptError;

        public event EventHandler SortParameterChanged;

        public event EventHandler ViewChanged;

        #endregion

        #region Properties

        [Browsable(false)]
        [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.")]
        public string AdditionaFilesPfad {
            get => _additionaFilesPfad;
            set {
                if (_additionaFilesPfad == value) { return; }
                _additionaFilesPfadtmp = string.Empty;
                AddPending(DatabaseDataType.AdditionaFilesPfad, -1, -1, _additionaFilesPfad, value, true);
                Cell.InvalidateAllSizes();
            }
        }

        [Browsable(false)]
        public Ansicht Ansicht {
            get => _ansicht;
            set {
                if (_ansicht == value) { return; }
                AddPending(DatabaseDataType.Ansicht, -1, -1, ((int)_ansicht).ToString(), ((int)value).ToString(), true);
            }
        }

        [Browsable(false)]
        [Description("Der Name der Datenbank.")]
        public string Caption {
            get => _caption;
            set {
                if (_caption == value) { return; }
                AddPending(DatabaseDataType.Caption, -1, -1, _caption, value, true);
            }
        }

        [Browsable(false)]
        public string CreateDate {
            get => _createDate;
            set {
                if (_createDate == value) { return; }
                AddPending(DatabaseDataType.CreateDate, -1, -1, _createDate, value, true);
            }
        }

        [Browsable(false)]
        public string Creator {
            get => _creator.Trim();
            set {
                if (_creator == value) { return; }
                AddPending(DatabaseDataType.Creator, -1, -1, _creator, value, true);
            }
        }

        public string FileEncryptionKey {
            get => _fileEncryptionKey;
            set {
                if (_fileEncryptionKey == value) { return; }
                AddPending(DatabaseDataType.FileEncryptionKey, -1, -1, _fileEncryptionKey, value, true);
            }
        }

        [Browsable(false)]
        public double GlobalScale {
            get => _globalScale;
            set {
                if (_globalScale == value) { return; }
                AddPending(DatabaseDataType.GlobalScale, -1, -1, _globalScale.ToString(CultureInfo.InvariantCulture), value.ToString(CultureInfo.InvariantCulture), true);
                Cell.InvalidateAllSizes();
            }
        }

        public string GlobalShowPass {
            get => _globalShowPass;
            set {
                if (_globalShowPass == value) { return; }
                AddPending(DatabaseDataType.GlobalShowPass, -1, -1, _globalShowPass, value, true);
            }
        }

        public string LoadedVersion { get; private set; }

        public DateTime PowerEdit { get; set; }

        [Browsable(false)]
        public new int ReloadDelaySecond {
            get => base.ReloadDelaySecond;
            set {
                if (base.ReloadDelaySecond == value) { return; }
                AddPending(DatabaseDataType.ReloadDelaySecond, -1, -1, base.ReloadDelaySecond.ToString(), value.ToString(), true);
            }
        }

        public string RulesScript {
            get => _rulesScript;
            set {
                if (_rulesScript == value) { return; }
                AddPending(DatabaseDataType.RulesScript, -1, -1, _rulesScript, value, true);
            }
        }

        [Browsable(false)]
        public RowSortDefinition SortDefinition {
            get => _sortDefinition;
            set {
                var alt = string.Empty;
                var neu = string.Empty;
                if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
                if (value != null) { neu = value.ToString(); }
                if (alt == neu) { return; }
                AddPending(DatabaseDataType.SortDefinition, -1, -1, alt, neu, false);
                _sortDefinition = new RowSortDefinition(this, neu);
                OnSortParameterChanged();
            }
        }

        [Browsable(false)]
        public int UndoCount {
            get => _undoCount;
            set {
                if (_undoCount == value) { return; }
                AddPending(DatabaseDataType.UndoCount, -1, -1, _undoCount.ToString(), value.ToString(), true);
            }
        }

        public VerwaisteDaten VerwaisteDaten {
            get => _verwaisteDaten;
            set {
                if (_verwaisteDaten == value) { return; }
                AddPending(DatabaseDataType.VerwaisteDaten, -1, -1, ((int)_verwaisteDaten).ToString(), ((int)value).ToString(), true);
            }
        }

        [Browsable(false)]
        public string ZeilenQuickInfo {
            get => _zeilenQuickInfo;
            set {
                if (_zeilenQuickInfo == value) { return; }
                AddPending(DatabaseDataType.ZeilenQuickInfo, -1, -1, _zeilenQuickInfo, value, true);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="checkOnlyFilenameToo"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static Database? GetByFilename(string filename, bool checkOnlyFilenameToo, bool readOnly) {
            var tmpDb = GetByFilename(filename, checkOnlyFilenameToo);

            if (tmpDb is Database db) { return db; }

            if (tmpDb != null) { return null; }//  Daten im Speicher, aber keine Datenbank!

            return !FileExists(filename) ? null : new Database(filename, readOnly, false);
        }

        public static Database? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream) {
            if (Develop.IsHostRunning() && !mustBeStream) {
                var x = -1;
                string? pf;
                do {
                    x++;
                    pf = string.Empty;
                    switch (x) {
                        case 0:
                            // BeCreative, At Home, 31.11.2021
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 1:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 2:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 3:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 4:
                            pf = System.Windows.Forms.Application.StartupPath + "\\" + name;
                            break;

                        case 5:
                            pf = System.Windows.Forms.Application.StartupPath + "\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 6:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 7:
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 8:
                            // warscheinlich BeCreative, Firma
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                            break;

                        case 9:
                            // Bildzeichen-Liste, Firma, 25.10.2021
                            pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
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
            var d = Generic.GetEmmbedResource(assembly, name);
            if (d != null) { return new Database(d); }
            if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
            return null;
        }

        /// <summary>
        /// Der komplette Pfad mit abschließenden \
        /// </summary>
        /// <returns></returns>
        public string AdditionaFilesPfadWhole() {
            // @ ist ein erkennungszeichen, dass der Pfad schon geprüft wurde, aber nicht vorhanden ist
            if (_additionaFilesPfadtmp == "@") { return string.Empty; }
            if (!string.IsNullOrEmpty(_additionaFilesPfadtmp)) { return _additionaFilesPfadtmp; }
            var t = _additionaFilesPfad.CheckPath();
            if (PathExists(t)) {
                _additionaFilesPfadtmp = t;
                return t;
            }

            t = (Filename.FilePath() + _additionaFilesPfad.Trim("\\") + "\\").CheckPath();
            if (PathExists(t)) {
                _additionaFilesPfadtmp = t;
                return t;
            }
            _additionaFilesPfadtmp = "@";
            return string.Empty;
        }

        public List<string> AllConnectedFilesLCase() {
            List<string> columnAll = new();
            var lockMe = new object();

            foreach (var thisColumnItem in Column) {
                if (thisColumnItem != null && thisColumnItem.Format == DataFormat.Link_To_Filesystem) {
                    var tmp = thisColumnItem.Contents();
                    Parallel.ForEach(tmp, thisTmp => {
                        var x = thisColumnItem.BestFile(thisTmp, false).ToLower();
                        lock (lockMe) {
                            columnAll.Add(x);
                        }
                    });
                }
            }
            //foreach (var ThisColumnItem in Column) {
            //    if (ThisColumnItem != null) {
            //        if (ThisColumnItem.Format == DataFormat.Link_To_Filesystem) {
            //            var tmp = ThisColumnItem.Contents();
            //            foreach (var thisTmp in tmp) {
            //                Column_All.AddIfNotExists(ThisColumnItem.BestFile(thisTmp, false).ToLower());
            //            }
            //        }
            //    }
            //}

            return columnAll.SortedDistinctList();
        }

        public List<RowData?> AllRows() {
            var sortedRows = new List<RowData?>();
            foreach (var thisRowItem in Row) {
                if (thisRowItem != null) {
                    sortedRows.Add(new RowData(thisRowItem));
                }
            }
            return sortedRows;
        }

        public string DefaultLayoutPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Layouts\\";

        public override void DiscardPendingChanges() => ChangeWorkItems(ItemState.Pending, ItemState.Undo);

        public override string ErrorReason(ErrorReason mode) {
            var f = base.ErrorReason(mode);

            if (!string.IsNullOrEmpty(f)) { return f; }
            if (mode == BlueBasics.Enums.ErrorReason.OnlyRead) { return string.Empty; }

            return IntParse(LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))
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
        public string Export_CSV(FirstRow firstRow, ColumnItem? column, List<RowData>? sortedRows) =>
            //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            Export_CSV(firstRow, new List<ColumnItem> { column }, sortedRows);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
        /// <param name="columnList">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
        /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
        /// <returns></returns>
        public string Export_CSV(FirstRow firstRow, List<ColumnItem>? columnList, List<RowData>? sortedRows) {
            if (columnList == null) {
                columnList = Column.Where(thisColumnItem => thisColumnItem != null).ToList();
            }

            if (sortedRows == null) { sortedRows = AllRows(); }

            StringBuilder sb = new();
            switch (firstRow) {
                case FirstRow.Without:
                    break;

                case FirstRow.ColumnCaption:
                    for (var colNr = 0; colNr < columnList.Count; colNr++) {
                        if (columnList[colNr] != null) {
                            var tmp = columnList[colNr].ReadableText();
                            tmp = tmp.Replace(";", "|");
                            tmp = tmp.Replace(" |", "|");
                            tmp = tmp.Replace("| ", "|");
                            sb.Append(tmp);
                            if (colNr < columnList.Count - 1) { sb.Append(";"); }
                        }
                    }
                    sb.Append("\r\n");
                    break;

                case FirstRow.ColumnInternalName:
                    for (var colNr = 0; colNr < columnList.Count; colNr++) {
                        if (columnList[colNr] != null) {
                            sb.Append(columnList[colNr].Name);
                            if (colNr < columnList.Count - 1) { sb.Append(';'); }
                        }
                    }
                    sb.Append("\r\n");
                    break;

                default:
                    Develop.DebugPrint(firstRow);
                    break;
            }
            foreach (var thisRow in sortedRows) {
                if (thisRow != null) {
                    for (var colNr = 0; colNr < columnList.Count; colNr++) {
                        if (columnList[colNr] != null) {
                            var tmp = Cell.GetString(columnList[colNr], thisRow.Row);
                            tmp = tmp.Replace("\r\n", "|");
                            tmp = tmp.Replace("\r", "|");
                            tmp = tmp.Replace("\n", "|");
                            tmp = tmp.Replace(";", "<sk>");
                            sb.Append(tmp);
                            if (colNr < columnList.Count - 1) { sb.Append(';'); }
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
        public string Export_CSV(FirstRow firstRow, ColumnViewCollection? arrangement, List<RowData>? sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public string Export_CSV(FirstRow firstRow, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_CSV(firstRow, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null));

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_HTML(filename, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null), false);

        /// <summary>
        /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
        /// </summary>
        /// <returns></returns>
        public void Export_HTML(string filename, List<ColumnItem?> columnList, List<RowData?> sortedRows, bool execute) {
            if (columnList == null || columnList.Count == 0) {
                columnList = Column.Where(thisColumnItem => thisColumnItem != null).ToList();
            }

            if (sortedRows == null) { sortedRows = AllRows(); }

            if (string.IsNullOrEmpty(filename)) {
                filename = TempFile(string.Empty, "Export", "html");
            }

            Html da = new(Filename.FileNameWithoutSuffix());
            da.AddCaption(_caption);
            da.TableBeginn();
            da.RowBeginn();
            foreach (var thisColumn in columnList) {
                if (thisColumn != null) {
                    da.CellAdd(thisColumn.ReadableText().Replace(";", "<br>"), thisColumn.BackColor);
                    //da.Add("        <th bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"><b>" + ThisColumn.ReadableText().Replace(";", "<br>") + "</b></th>");
                }
            }
            da.RowEnd();
            foreach (var thisRow in sortedRows) {
                if (thisRow != null) {
                    da.RowBeginn();
                    foreach (var thisColumn in columnList) {
                        if (thisColumn != null) {
                            var lcColumn = thisColumn;
                            var lCrow = thisRow.Row;
                            if (thisColumn.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                                (lcColumn, lCrow, _) = CellCollection.LinkedCellData(thisColumn, thisRow.Row, false, false);
                            }
                            if (lCrow != null && lcColumn != null) {
                                da.CellAdd(lCrow.CellGetValuesReadable(lcColumn, ShortenStyle.HTML).JoinWith("<br>"), thisColumn.BackColor);
                            } else {
                                da.CellAdd(" ", thisColumn.BackColor);
                            }
                        }
                    }
                    da.RowEnd();
                }
            }
            // Summe----
            da.RowBeginn();
            foreach (var thisColumn in columnList) {
                if (thisColumn != null) {
                    var s = thisColumn.Summe(sortedRows);
                    if (s == null) {
                        da.CellAdd("-", thisColumn.BackColor);
                        //da.Add("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                    } else {
                        da.CellAdd("~sum~ " + s, thisColumn.BackColor);
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
        public void Export_HTML(string filename, ColumnViewCollection? arrangement, List<RowData?> sortedRows, bool execute) => Export_HTML(filename, arrangement.ListOfUsedColumn(), sortedRows, execute);

        public override bool HasPendingChanges() {
            try {
                if (ReadOnly) { return false; }

                return Works.Any(thisWork => thisWork.State == ItemState.Pending);
            } catch {
                return HasPendingChanges();
            }
        }

        public string Import(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart, bool dorowautmatic, string script) {
            // Vorbereitung des Textes -----------------------------
            importText = importText.Replace("\r\n", "\r").Trim("\r");

            #region die Zeilen (zeil) vorbereiten

            var ein = importText.SplitAndCutByCr();
            List<string[]> zeil = new();
            var neuZ = 0;
            for (var z = 0; z <= ein.GetUpperBound(0); z++) {

                #region Das Skript berechnen

                if (!string.IsNullOrEmpty(script)) {
                    var vars = new List<Variable>();
                    vars.Add(new VariableString("Row", ein[z], false, false, "Der Original-Text. Dieser kann (und soll) manipuliert werden."));
                    vars.Add(new VariableBool("IsCaption", spalteZuordnen && z == 0, true, false, "Wenn TRUE, ist das die erste Zeile, die Überschriften enthält."));
                    vars.Add(new VariableString("Seperator", splitChar, true, false, "Das Trennzeichen"));
                    var x = new BlueScript.Script(vars, string.Empty);
                    x.ScriptText = script;
                    if (!x.Parse()) {
                        OnDropMessage(FehlerArt.Warnung, "Skript-Fehler, Import kann nicht ausgeführt werden.");
                        return x.Error;
                    }
                    ein[z] = vars.GetString("Row");
                }

                #endregion

                if (eliminateMultipleSplitter) {
                    ein[z] = ein[z].Replace(splitChar + splitChar, splitChar);
                }
                if (eleminateSplitterAtStart) {
                    ein[z] = ein[z].TrimStart(splitChar);
                }
                ein[z] = ein[z].TrimEnd(splitChar);
                zeil.Add(ein[z].SplitAndCutBy(splitChar));
            }

            if (zeil.Count == 0) {
                OnDropMessage(FehlerArt.Warnung, "Import kann nicht ausgeführt werden.");
                return "Import kann nicht ausgeführt werden.";
            }

            #endregion

            #region Spaltenreihenfolge (columns) ermitteln

            List<ColumnItem?> columns = new();
            var startZ = 0;

            if (spalteZuordnen) {
                startZ = 1;
                for (var spaltNo = 0; spaltNo < zeil[0].GetUpperBound(0) + 1; spaltNo++) {
                    if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                        OnDropMessage(FehlerArt.Warnung, "Abbruch,<br>leerer Spaltenname.");
                        return "Abbruch,<br>leerer Spaltenname.";
                    }
                    zeil[0][spaltNo] = zeil[0][spaltNo].Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);
                    var col = Column.Exists(zeil[0][spaltNo]);
                    if (col == null) {
                        col = Column.Add(zeil[0][spaltNo]);
                        col.Caption = zeil[0][spaltNo];
                        col.Format = DataFormat.Text;
                    }
                    columns.Add(col);
                }
            } else {
                columns.AddRange(Column.Where(thisColumn => thisColumn != null && string.IsNullOrEmpty(thisColumn.Identifier)));
                while (columns.Count < zeil[0].GetUpperBound(0) + 1) {
                    var newc = Column.Add();
                    newc.Caption = newc.Name;
                    newc.Format = DataFormat.Text;
                    newc.MultiLine = true;
                    columns.Add(newc);
                }
            }

            #endregion

            // -------------------------------------
            // --- Importieren ---
            // -------------------------------------

            for (var zeilNo = startZ; zeilNo < zeil.Count; zeilNo++) {
                var tempVar2 = Math.Min(zeil[zeilNo].GetUpperBound(0) + 1, columns.Count);
                RowItem? row = null;
                for (var spaltNo = 0; spaltNo < tempVar2; spaltNo++) {
                    if (spaltNo == 0) {
                        row = null;
                        if (zeileZuordnen && !string.IsNullOrEmpty(zeil[zeilNo][spaltNo])) { row = Row[zeil[zeilNo][spaltNo]]; }
                        if (row == null && !string.IsNullOrEmpty(zeil[zeilNo][spaltNo])) {
                            row = Row.Add(zeil[zeilNo][spaltNo]);
                            neuZ++;
                        }
                    } else {
                        row?.CellSet(columns[spaltNo], zeil[zeilNo][spaltNo].SplitAndCutBy("|").JoinWithCr());
                    }
                    if (row != null && dorowautmatic) { row.DoAutomatic(true, true, "import"); }
                }
            }

            OnDropMessage(FehlerArt.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
            return string.Empty;
        }

        public string ImportCSV(string filename, string script) {
            if (!FileExists(filename)) { return "Datei nicht gefunden"; }
            var importText = File.ReadAllText(filename, Constants.Win1252);

            if (string.IsNullOrEmpty(importText)) { return "Dateiinhalt leer"; }

            var x = importText.SplitAndCutByCrToList();

            if (x.Count() < 2) { return "Keine Zeilen zum importieren."; }

            var sep = ",";

            if (x[0].StartsWith("sep=", StringComparison.OrdinalIgnoreCase)) {
                if (x.Count() < 3) { return "Keine Zeilen zum importieren."; }
                sep = x[0].Substring(4);
                x.RemoveAt(0);
                importText = x.JoinWithCr();
            }

            return Import(importText, true, true, sep, false, false, true, script);
        }

        /// <summary>
        /// Fügt Comandos manuell hinzu. Vorsicht: Kann Datenbank beschädigen
        /// </summary>
        public void InjectCommand(DatabaseDataType comand, string changedTo) => AddPending(comand, -1, -1, string.Empty, changedTo, true);

        public bool IsAdministrator() {
            if (UserGroup.ToUpper() == "#ADMINISTRATOR") { return true; }
            if (DatenbankAdmin == null || DatenbankAdmin.Count == 0) { return false; }
            if (DatenbankAdmin.Contains("#EVERYBODY", false)) { return true; }
            if (!string.IsNullOrEmpty(UserName) && DatenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
            return !string.IsNullOrEmpty(UserGroup) && DatenbankAdmin.Contains(UserGroup, false);
        }

        public void OnScriptError(RowCancelEventArgs e) {
            if (Disposed) { return; }
            ScriptError?.Invoke(this, e);
        }

        public void Parse(byte[] bLoaded, ref int pointer, ref DatabaseDataType type, ref long colKey, ref long rowKey, ref string value, ref int width, ref int height) {
            int les;
            switch ((Routinen)bLoaded[pointer]) {
                case Routinen.CellFormat: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode3(bLoaded, pointer + 5);
                        rowKey = NummerCode3(bLoaded, pointer + 8);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                        value = b.ToStringWin1252();
                        width = NummerCode2(bLoaded, pointer + 11 + les);
                        height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                        pointer += 11 + les + 4;
                        break;
                    }
                case Routinen.CellFormatUTF8: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode3(bLoaded, pointer + 5);
                        rowKey = NummerCode3(bLoaded, pointer + 8);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                        value = b.ToStringUtf8();
                        width = NummerCode2(bLoaded, pointer + 11 + les);
                        height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                        pointer += 11 + les + 4;
                        break;
                    }
                case Routinen.CellFormatUTF8_V400: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode7(bLoaded, pointer + 5);
                        rowKey = NummerCode7(bLoaded, pointer + 12);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                        value = b.ToStringUtf8();
                        width = NummerCode2(bLoaded, pointer + 19 + les);
                        height = NummerCode2(bLoaded, pointer + 19 + les + 2);
                        pointer += 19 + les + 4;
                        break;
                    }
                case Routinen.DatenAllgemein: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = -1;
                        rowKey = -1;
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                        value = b.ToStringWin1252();
                        width = 0;
                        height = 0;
                        pointer += 5 + les;
                        break;
                    }
                case Routinen.DatenAllgemeinUTF8: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = -1;
                        rowKey = -1;
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                        value = b.ToStringUtf8();
                        width = 0;
                        height = 0;
                        pointer += 5 + les;
                        break;
                    }
                case Routinen.Column: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode3(bLoaded, pointer + 5);
                        rowKey = NummerCode3(bLoaded, pointer + 8);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                        value = b.ToStringWin1252();
                        width = 0;
                        height = 0;
                        pointer += 11 + les;
                        break;
                    }
                case Routinen.ColumnUTF8: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode3(bLoaded, pointer + 5);
                        rowKey = NummerCode3(bLoaded, pointer + 8);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                        value = b.ToStringUtf8();
                        width = 0;
                        height = 0;
                        pointer += 11 + les;
                        break;
                    }
                case Routinen.ColumnUTF8_V400: {
                        type = (DatabaseDataType)bLoaded[pointer + 1];
                        les = NummerCode3(bLoaded, pointer + 2);
                        colKey = NummerCode7(bLoaded, pointer + 5);
                        rowKey = NummerCode7(bLoaded, pointer + 12);
                        var b = new byte[les];
                        Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                        value = b.ToStringUtf8();
                        width = 0;
                        height = 0;
                        pointer += 19 + les;
                        break;
                    }
                default: {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laderoutine nicht definiert: " + bLoaded[pointer]);
                        break;
                    }
            }
        }

        public List<string> Permission_AllUsed(bool cellLevel) {
            List<string> e = new();
            foreach (var thisColumnItem in Column) {
                if (thisColumnItem != null) {
                    e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
                }
            }
            e.AddRange(PermissionGroupsNewRow);
            e.AddRange(DatenbankAdmin);
            foreach (var thisArrangement in ColumnArrangements) {
                e.AddRange(thisArrangement.PermissionGroups_Show);
            }
            foreach (var thisArrangement in Views) {
                e.AddRange(thisArrangement.PermissionGroups_Show);
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

        public bool PermissionCheck(ListExt<string> allowed, RowItem? row) {
            try {
                if (IsAdministrator()) { return true; }
                if (PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }
                if (allowed == null || allowed.Count == 0) { return false; }
                if (allowed.Any(thisString => PermissionCheckWithoutAdmin(thisString, row))) {
                    return true;
                }
            } catch (Exception ex) {
                Develop.DebugPrint(FehlerArt.Warnung, ex);
            }
            return false;
        }

        public override void RepairAfterParse() {
            // System-Spalten checken und alte Formate auf neuen Stand bringen
            Column.Repair();
            // Evtl. Defekte Rows reparieren
            //Row.Repair();
            //Defekte Ansichten reparieren - Teil 1

            CheckViewsAndArrangements();

            Layouts.Check();
        }

        public string UndoText(ColumnItem? column, RowItem? row) {
            if (Works == null || Works.Count == 0) { return string.Empty; }
            var cellKey = CellCollection.KeyOfCell(column, row);
            var t = "";
            for (var z = Works.Count - 1; z >= 0; z--) {
                if (Works[z] != null && Works[z].CellKey == cellKey) {
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

        internal void AddPending(DatabaseDataType comand, ColumnItem column, string previousValue, string changedTo, bool executeNow) => AddPending(comand, column.Key, -1, previousValue, changedTo, executeNow);

        internal void AddPending(DatabaseDataType comand, long columnKey, string listExt, bool executeNow) => AddPending(comand, columnKey, -1, "", listExt, executeNow);

        internal void AddPending(DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, bool executeNow) {
            if (executeNow) {
                ParseThis(comand, changedTo, Column.SearchByKey(columnKey), Row.SearchByKey(rowKey), -1, -1);
            }
            if (IsParsing) { return; }
            if (ReadOnly) {
                if (!string.IsNullOrEmpty(Filename)) {
                    Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + Filename);
                }
                return;
            }
            // Keine Doppelten Rausfiltern, ansonstn stimmen die Undo nicht mehr

            if (comand != DatabaseDataType.AutoExport) { SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

            if (rowKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
            if (columnKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }
            Works.Add(new WorkItem(comand, columnKey, rowKey, previousValue, changedTo, UserName));
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
            //ZeilenQuickInfo = ZeilenQuickInfo.Replace("~" + oldName + ";", "~" + newName.Name + ";", RegexOptions.IgnoreCase);
            //ZeilenQuickInfo = ZeilenQuickInfo.Replace("~" + oldName + "(", "~" + newName.Name + "(", RegexOptions.IgnoreCase);
        }

        internal string Column_UsedIn(ColumnItem? column) {
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
                t += "<br><br><b>Zusatz-Info:</b><br>";
                t = t + " - Befüllt mit " + l.Count + " verschiedenen Werten";
            }
            return t;
        }

        internal void DevelopWarnung(string t) {
            try {
                t += "\r\nParsing: " + IsParsing;
                t += "\r\nLoading: " + IsLoading;
                t += "\r\nSaving: " + IsSaving;
                t += "\r\nColumn-Count: " + Column.Count;
                t += "\r\nRow-Count: " + Row.Count;
                t += "\r\nFile: " + Filename;
            } catch { }
            Develop.DebugPrint(FehlerArt.Warnung, t);
        }

        internal void OnDropMessage(FehlerArt type, string message) {
            if (Disposed) { return; }
            DropMessage?.Invoke(this, new MessageEventArgs(type, message));
        }

        internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventargs e) {
            if (Disposed) { return; }
            GenerateLayoutInternal?.Invoke(this, e);
        }

        internal void OnProgressbarInfo(ProgressbarEventArgs e) {
            if (Disposed) { return; }
            ProgressbarInfo?.Invoke(this, e);
        }

        internal void OnViewChanged() {
            if (Disposed) { return; }
            ViewChanged?.Invoke(this, System.EventArgs.Empty);
        }

        internal void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content) {
            var b = content.UTF8_ToByte();
            list.Add((byte)Routinen.DatenAllgemeinUTF8);
            list.Add((byte)databaseDataType);
            SaveToByteList(list, b.Length, 3);
            list.AddRange(b);
        }

        internal void SaveToByteList(List<byte> list, KeyValuePair<string, CellItem> cell) {
            if (string.IsNullOrEmpty(cell.Value.Value)) { return; }
            Cell.DataOfCellKey(cell.Key, out var tColumn, out var tRow);
            if (!tColumn.SaveContent) { return; }
            var b = cell.Value.Value.UTF8_ToByte();
            list.Add((byte)Routinen.CellFormatUTF8_V400);
            list.Add((byte)DatabaseDataType.ce_Value_withSizeData);
            SaveToByteList(list, b.Length, 3);
            SaveToByteList(list, tColumn.Key, 7);
            SaveToByteList(list, tRow.Key, 7);
            list.AddRange(b);
            var contentSize = Cell.ContentSizeToSave(cell, tColumn);
            SaveToByteList(list, contentSize.Width, 2);
            SaveToByteList(list, contentSize.Height, 2);
        }

        internal void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content, long columnKey) {
            var b = content.UTF8_ToByte();
            list.Add((byte)Routinen.ColumnUTF8_V400);
            list.Add((byte)databaseDataType);
            SaveToByteList(list, b.Length, 3);
            SaveToByteList(list, columnKey, 7);
            SaveToByteList(list, 0, 7); //Zeile-Unötig
            list.AddRange(b);
        }

        protected override bool BlockSaveOperations() => RowItem.DoingScript || base.BlockSaveOperations();

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
            PermissionGroupsNewRow.Changed -= PermissionGroups_NewRow_ListOrItemChanged;
            Tags.Changed -= DatabaseTags_ListOrItemChanged;
            Export.Changed -= Export_ListOrItemChanged;
            DatenbankAdmin.Changed -= DatabaseAdmin_ListOrItemChanged;

            Row.RowRemoving -= Row_RowRemoving;
            Row.RowAdded -= Row_RowAdded;

            Column.ItemRemoving -= Column_ItemRemoving;
            Column.ItemRemoved -= Column_ItemRemoved;
            Column.ItemAdded -= Column_ItemAdded;
            Column.Dispose();
            Cell.Dispose();
            Row.Dispose();
            Works.Dispose();
            ColumnArrangements.Dispose();
            Views.Dispose();
            Tags.Dispose();
            Export.Dispose();
            DatenbankAdmin.Dispose();
            PermissionGroupsNewRow.Dispose();
            Layouts.Dispose();
        }

        //protected override void CheckDataAfterReload() {
        //    try {
        //        // Leztes WorkItem suchen. Auch Ohne LogUndo MUSS es vorhanden sein.
        //        if (!string.IsNullOrEmpty(_LastWorkItem)) {
        //            var ok = false;
        //            var ok2 = string.Empty;
        //            foreach (var ThisWorkItem in Works) {
        //                var tmp = ThisWorkItem.ToString();
        //                if (tmp == _LastWorkItem) {
        //                    ok = true;
        //                    break;
        //                } else if (tmp.Substring(7) == _LastWorkItem.Substring(7)) {
        //                    ok2 = tmp;
        //                }
        //            }
        //            if (!ok && string.IsNullOrEmpty(ok2)) {
        //                if (!Filename.Contains("AutoVue") && !Filename.Contains("Plandaten") && !Filename.Contains("Ketten.") && !Filename.Contains("Kettenräder.") && !Filename.Contains("TVW") && !Filename.Contains("Work") && !Filename.Contains("Behälter")) {
        //                    Develop.DebugPrint(enFehlerArt.Warnung, "WorkItem verschwunden<br>" + _LastWorkItem + "<br>" + Filename + "<br><br>Vorher:<br>" + _WorkItemsBefore + "<br><br>Nachher:<br>" + Works.ToString());
        //                }
        //            }
        //        }
        //    } catch {
        //        //Develop.DebugPrint(ex);
        //    }
        //}
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
            ChangeWorkItems(ItemState.Pending, ItemState.Undo);
            var filesNewLCase = AllConnectedFilesLCase();
            List<string> writerFilesToDeleteLCase = new();
            if (_verwaisteDaten == VerwaisteDaten.Löschen) {
                writerFilesToDeleteLCase = _filesAfterLoadingLCase.Except(filesNewLCase).ToList();
            }
            _filesAfterLoadingLCase.Clear();
            _filesAfterLoadingLCase.AddRange(filesNewLCase);
            if (writerFilesToDeleteLCase.Count > 0) { DeleteFile(writerFilesToDeleteLCase); }
        }

        protected override bool IsThereBackgroundWorkToDo() {
            if (HasPendingChanges()) { return true; }
            CancelEventArgs ec = new(false);
            OnExporting(ec);
            if (ec.Cancel) { return false; }

            foreach (var thisExport in Export) {
                if (thisExport != null) {
                    if (thisExport.Typ == ExportTyp.EinzelnMitFormular) { return true; }
                    if (DateTime.UtcNow.Subtract(thisExport.LastExportTimeUtc).TotalDays > thisExport.BackupInterval) { return true; }
                }
            }
            return false;
        }

        protected override void ParseExternal(byte[] b) {
            Column.ThrowEvents = false;
            DatabaseDataType art = 0;
            var pointer = 0;
            var inhalt = "";
            ColumnItem? column = null;
            RowItem? row = null;
            var x = 0;
            var y = 0;
            long colKey = 0;
            long rowKey = 0;
            List<ColumnItem?> columnsOld = new();
            columnsOld.AddRange(Column);
            Column.Clear();
            var oldPendings = Works.Where(thisWork => thisWork.State == ItemState.Pending).ToList();
            Works.Clear();
            do {
                if (pointer >= b.Length) { break; }
                Parse(b, ref pointer, ref art, ref colKey, ref rowKey, ref inhalt, ref x, ref y);
                if (rowKey > -1) {
                    row = Row.SearchByKey(rowKey);
                    if (row == null) {
                        row = new RowItem(this, rowKey);
                        Row.Add(row);
                    }
                }
                if (colKey > -1) {
                    // Zuerst schauen, ob die Column schon (wieder) in der richtigen Collection ist
                    column = Column.SearchByKey(colKey);
                    if (column == null) {
                        // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzufügen
                        foreach (var thisColumn in columnsOld) {
                            if (thisColumn != null && thisColumn.Key == colKey) {
                                column = thisColumn;
                            }
                        }
                        if (column != null) {
                            // Prima, gefunden! Noch die Collections korrigieren
                            Column.AddFromParser(column);
                            columnsOld.Remove(column);
                        } else {
                            // Nicht gefunden, als neu machen
                            column = Column.Add(colKey);
                        }
                    }
                }
                if (art == DatabaseDataType.CryptionState) {
                    if (inhalt.FromPlusMinus()) {
                        PasswordEventArgs e = new();
                        OnNeedPassword(e);
                        b = Cryptography.SimpleCrypt(b, e.Password, -1, pointer, b.Length - 1);
                        if (b[pointer + 1] != 3 || b[pointer + 2] != 0 || b[pointer + 3] != 0 || b[pointer + 4] != 2 || b[pointer + 5] != 79 || b[pointer + 6] != 75) {
                            RemoveFilename();
                            LoadedVersion = "9.99";
                            //MessageBox.Show("Zugriff verweigrt, Passwort falsch!", ImageCode.Kritisch, "OK");
                            break;
                        }
                    }
                }
                var fehler = ParseThis(art, inhalt, column, row, x, y);
                if (art == DatabaseDataType.EOF) { break; }
                if (!string.IsNullOrEmpty(fehler)) {
                    LoadedVersion = "9.99";
                    Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + Filename + "<br>Meldung: " + fehler);
                }
            } while (true);
            // Spalten, die nach dem Reload nicht mehr benötigt werden, löschen
            //ColumnsOld.DisposeAndRemoveAll();
            Row.RemoveNullOrEmpty();
            //Row.RemoveNullOrDisposed();
            Cell.RemoveOrphans();
            //LoadPicsIntoImageChache();
            _filesAfterLoadingLCase.Clear();
            _filesAfterLoadingLCase.AddRange(AllConnectedFilesLCase());
            Works.AddRange(oldPendings);
            oldPendings.Clear();
            ExecutePending();
            Column.ThrowEvents = true;
            if (IntParse(LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))) { SetReadOnly(); }
        }

        protected override byte[] ToListOfByte() {
            try {
                var cryptPos = -1;
                List<byte> l = new();
                // Wichtig, Reihenfolge und Länge NIE verändern!
                SaveToByteList(l, DatabaseDataType.Formatkennung, "BlueDatabase");
                SaveToByteList(l, DatabaseDataType.Version, DatabaseVersion);
                SaveToByteList(l, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
                                                                                                                                                                                                                                                                  // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
                if (string.IsNullOrEmpty(_globalShowPass)) {
                    SaveToByteList(l, DatabaseDataType.CryptionState, false.ToPlusMinus());
                } else {
                    SaveToByteList(l, DatabaseDataType.CryptionState, true.ToPlusMinus());
                    cryptPos = l.Count;
                    SaveToByteList(l, DatabaseDataType.CryptionTest, "OK");
                }
                SaveToByteList(l, DatabaseDataType.GlobalShowPass, _globalShowPass);
                SaveToByteList(l, DatabaseDataType.FileEncryptionKey, _fileEncryptionKey);
                SaveToByteList(l, DatabaseDataType.Creator, _creator);
                SaveToByteList(l, DatabaseDataType.CreateDate, _createDate);
                SaveToByteList(l, DatabaseDataType.Caption, _caption);
                //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString());
                SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString());
                SaveToByteList(l, DatabaseDataType.Tags, Tags.JoinWithCr());
                SaveToByteList(l, DatabaseDataType.PermissionGroups_NewRow, PermissionGroupsNewRow.JoinWithCr());
                SaveToByteList(l, DatabaseDataType.DatenbankAdmin, DatenbankAdmin.JoinWithCr());
                SaveToByteList(l, DatabaseDataType.GlobalScale, _globalScale.ToString(Constants.Format_Float1));
                SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString());
                SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, base.ReloadDelaySecond.ToString());
                //SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
                SaveToByteList(l, DatabaseDataType.RulesScript, _rulesScript);
                //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
                //SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _filterImagePfad);
                SaveToByteList(l, DatabaseDataType.AdditionaFilesPfad, _additionaFilesPfad);
                SaveToByteList(l, DatabaseDataType.ZeilenQuickInfo, _zeilenQuickInfo);
                Column.SaveToByteList(l);
                //Row.SaveToByteList(l);
                Cell.SaveToByteList(ref l);
                if (SortDefinition == null) {
                    // Ganz neue Datenbank
                    SaveToByteList(l, DatabaseDataType.SortDefinition, string.Empty);
                } else {
                    SaveToByteList(l, DatabaseDataType.SortDefinition, _sortDefinition.ToString());
                }
                //SaveToByteList(l, enDatabaseDataType.Rules_ALT, Rules.ToString(true));
                SaveToByteList(l, DatabaseDataType.ColumnArrangement, ColumnArrangements.ToString());
                SaveToByteList(l, DatabaseDataType.Views, Views.ToString());
                SaveToByteList(l, DatabaseDataType.Layouts, Layouts.JoinWithCr());
                SaveToByteList(l, DatabaseDataType.AutoExport, Export.ToString(true));
                // Beim Erstellen des Undo-Speichers die Works nicht verändern, da auch bei einem nicht
                // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
                // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
                List<string> works2 = new();
                foreach (var thisWorkItem in Works) {
                    if (thisWorkItem != null) {
                        if (thisWorkItem.Comand != DatabaseDataType.ce_Value_withoutSizeData) {
                            works2.Add(thisWorkItem.ToString());
                        } else {
                            if (thisWorkItem.LogsUndo(this)) {
                                works2.Add(thisWorkItem.ToString());
                            }
                        }
                    }
                }
                SaveToByteList(l, DatabaseDataType.UndoCount, _undoCount.ToString());
                if (works2.Count > _undoCount) { works2.RemoveRange(0, works2.Count - _undoCount); }
                SaveToByteList(l, DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.9)));
                SaveToByteList(l, DatabaseDataType.EOF, "END");
                return cryptPos > 0 ? Cryptography.SimpleCrypt(l.ToArray(), _globalShowPass, 1, cryptPos, l.Count - 1) : l.ToArray();
            } catch {
                return ToListOfByte();
            }
        }

        //protected override void PrepeareDataForCheckingBeforeLoad() {
        //    // Letztes WorkItem speichern, als Kontrolle
        //    _WorkItemsBefore = string.Empty;
        //    _LastWorkItem = string.Empty;
        //    if (Works != null && Works.Count > 0) {
        //        var c = 0;
        //        do {
        //            c++;
        //            if (c > 20 || Works.Count - c < 20) { break; }
        //            var wn = Works.Count - c;
        //            if (Works[wn].LogsUndo(this) && Works[wn].HistorischRelevant) { _LastWorkItem = Works[wn].ToString(); }
        //        } while (string.IsNullOrEmpty(_LastWorkItem));
        //        _WorkItemsBefore = Works.ToString();
        //    }
        //}
        private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

        private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

        private static long NummerCode7(byte[] b, int pointer) {
            long nu = 0;
            for (var n = 0; n < 7; n++) {
                nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
            }
            return nu;
        }

        private static void SaveToByteList(List<byte> list, long numberToAdd, int byteCount) {
            //var tmp = numberToAdd;
            //var nn = byteCount;

            do {
                byteCount--;
                var te = (long)Math.Pow(255, byteCount);
                var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

                list.Add(mu);
                numberToAdd %= te;
            } while (byteCount > 0);

            //if (nn == 3) {
            //    if (NummerCode3(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
            //}

            //if (nn == 7) {
            //    if (NummerCode7(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
            //}
        }

        private void ChangeWorkItems(ItemState oldState, ItemState newState) {
            foreach (var thisWork in Works) {
                if (thisWork != null) {
                    if (thisWork.State == oldState) { thisWork.State = newState; }
                }
            }
        }

        private void CheckViewsAndArrangements() {
            //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Temporär erzeugt werden!

            if (IsParsing) { return; }

            for (var z = 0; z < Math.Max(2, ColumnArrangements.Count); z++) {
                if (ColumnArrangements.Count < z + 1) { ColumnArrangements.Add(new ColumnViewCollection(this, string.Empty)); }
                ColumnArrangements[z].Repair(z, true);
            }

            for (var z = 0; z < Math.Max(2, Views.Count); z++) {
                if (Views.Count < z + 1) { Views.Add(new ColumnViewCollection(this, string.Empty)); }
                Views[z].Repair(z, false);
            }
        }

        private void Column_ItemAdded(object sender, ListEventArgs e) => CheckViewsAndArrangements();//            protected override void OnItemAdded(ColumnItem item) {//    base.OnItemAdded(item);//    Database.CheckViewsAndArrangements();//}//protected override void OnItemRemoved() {//    base.OnItemRemoved();//    Database.CheckViewsAndArrangements();//    Database.Layouts.Check();//}

        //private void ChangeColumnKeyInPending(int oldKey, int newKey) {
        //    foreach (var ThisPending in Works) {
        //        if (ThisPending.State == enItemState.Pending) {
        //            if (ThisPending.ColKey == oldKey) {
        //                if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
        //                ThisPending.ColKey = newKey; // Generell den Schlüssel ändern
        //                if (_LastWorkItem == "X") {
        //                    _LastWorkItem = ThisPending.ToString();
        //                    //Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem geändert: " + _LastWorkItem);
        //                }
        //                switch (ThisPending.Comand) {
        //                    case enDatabaseDataType.AddColumn:
        //                    case enDatabaseDataType.dummyComand_RemoveColumn:
        //                        ThisPending.ChangedTo = newKey.ToString();
        //                        break;

        //                    default:
        //                        if (ThisPending.PreviousValue.Contains(ColumnCollection.ParsableColumnKey(oldKey))) {
        //                            Develop.DebugPrint("Replace machen (Old): " + oldKey);
        //                        }
        //                        if (ThisPending.ChangedTo.Contains(ColumnCollection.ParsableColumnKey(oldKey))) {
        //                            Develop.DebugPrint("Replace machen (New): " + oldKey);
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //        OnColumnKeyChanged(new KeyChangedEventArgs(oldKey, newKey));
        //    }
        //}

        //private void ChangeRowKeyInPending(int OldKey, int NewKey) {
        //    foreach (var ThisPending in Works) {
        //        if (ThisPending.State == enItemState.Pending) {
        //            if (ThisPending.RowKey == OldKey) {
        //                if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
        //                ThisPending.RowKey = NewKey; // Generell den Schlüssel ändern
        //                if (_LastWorkItem == "X") {
        //                    _LastWorkItem = ThisPending.ToString();
        //                    //Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem geändert: " + _LastWorkItem);
        //                }
        //                switch (ThisPending.Comand) {
        //                    case enDatabaseDataType.dummyComand_AddRow:
        //                    case enDatabaseDataType.dummyComand_RemoveRow:
        //                        ThisPending.ChangedTo = NewKey.ToString();
        //                        break;

        //                    default:
        //                        if (ThisPending.PreviousValue.Contains("RowKey=" + OldKey)) { Develop.DebugPrint("Replace machen (Old): " + OldKey); }
        //                        if (ThisPending.ChangedTo.Contains("RowKey=" + OldKey)) { Develop.DebugPrint("Replace machen (New): " + OldKey); }
        //                        break;
        //                }
        //            }
        //        }
        //        OnRowKeyChanged(new KeyChangedEventArgs(OldKey, NewKey));
        //    }
        //}
        private void Column_ItemRemoved(object sender, System.EventArgs e) {
            if (IsParsing) { Develop.DebugPrint(FehlerArt.Warnung, "Parsing Falsch!"); }
            CheckViewsAndArrangements();

            Layouts.Check();
        }

        private void Column_ItemRemoving(object sender, ListEventArgs e) {
            var key = ((ColumnItem)e.Item).Key;
            AddPending(DatabaseDataType.dummyComand_RemoveColumn, key, -1, string.Empty, key.ToString(), false);
        }

        private void ColumnArrangements_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt werden. Kann zu Endlosschleifen führen.
            AddPending(DatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(), false);
        }

        private void DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(DatabaseDataType.DatenbankAdmin, -1, DatenbankAdmin.JoinWithCr(), false);
        }

        private void DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(DatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
        }

        private void ExecutePending() {
            if (!IsParsing) { Develop.DebugPrint(FehlerArt.Fehler, "Nur während des Parsens möglich"); }
            if (!HasPendingChanges()) { return; }
            // Erst die Neuen Zeilen / Spalten alle neutralisieren
            //var dummy = -1000;
            //foreach (var ThisPending in Works) {
            //    if (ThisPending.State == enItemState.Pending) {
            //        //if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow) {
            //        //    dummy--;
            //        //    ChangeRowKeyInPending(ThisPending.RowKey, dummy);
            //        //}
            //        //if (ThisPending.Comand == enDatabaseDataType.AddColumn) {
            //        //    dummy--;
            //        //    ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
            //        //}
            //    }
            //}
            //// Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
            //foreach (var ThisPending in Works) {
            //    if (ThisPending.State == enItemState.Pending) {
            //        switch (ThisPending.Comand) {
            //            //case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
            //            //        var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
            //            //        var fRow = Row[Value];
            //            //        if (!string.IsNullOrEmpty(Value) && fRow != null) {
            //            //            ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
            //            //        } else {
            //            //            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
            //            //        }
            //            //        break;
            //            //    }
            //            //case enDatabaseDataType.dummyComand_AddRow:
            //            //    ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
            //            //    break;

            //            //case enDatabaseDataType.AddColumn:
            //            //    ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey);
            //            //    break;
            //        }
            //    }
            //}
            // Und nun alles ausführen!
            foreach (var thisPending in Works.Where(thisPending => thisPending.State == ItemState.Pending)) {
                if (thisPending.Comand == DatabaseDataType.co_Name) {
                    thisPending.ChangedTo = Column.Freename(thisPending.ChangedTo);
                }
                ExecutePending(thisPending);
            }
        }

        private void ExecutePending(WorkItem thisPendingItem) {
            if (thisPendingItem.State == ItemState.Pending) {
                RowItem? row = null;
                if (thisPendingItem.RowKey > -1) {
                    row = Row.SearchByKey(thisPendingItem.RowKey);
                    if (row == null) {
                        if (thisPendingItem.Comand != DatabaseDataType.dummyComand_AddRow && thisPendingItem.User != UserName) {
                            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + Filename + "<br>" + thisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ColumnItem? col = null;
                if (thisPendingItem.ColKey > -1) {
                    col = Column.SearchByKey(thisPendingItem.ColKey);
                    if (col == null) {
                        if (thisPendingItem.Comand != DatabaseDataType.AddColumn && thisPendingItem.User != UserName) {
                            Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + Filename + "<br>" + thisPendingItem.ToString());
                            return;
                        }
                    }
                }
                ParseThis(thisPendingItem.Comand, thisPendingItem.ChangedTo, col, row, 0, 0);
            }
        }

        private void Export_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
            AddPending(DatabaseDataType.AutoExport, -1, Export.ToString(true), false);
        }

        private void Initialize() {
            Cell.Initialize();
            Works.Clear();
            ColumnArrangements.Clear();
            Layouts.Clear();
            Views.Clear();
            PermissionGroupsNewRow.Clear();
            Tags.Clear();
            Export.Clear();
            DatenbankAdmin.Clear();
            _globalShowPass = string.Empty;
            _fileEncryptionKey = string.Empty;
            _creator = UserName;
            _createDate = DateTime.Now.ToString(Constants.Format_Date5);
            base.ReloadDelaySecond = 600;
            _undoCount = 300;
            _caption = string.Empty;
            _verwaisteDaten = VerwaisteDaten.Ignorieren;
            LoadedVersion = DatabaseVersion;
            _rulesScript = string.Empty;
            _globalScale = 1f;
            _ansicht = Ansicht.Unverändert;
            _additionaFilesPfad = "AdditionalFiles";
            _zeilenQuickInfo = string.Empty;
            _sortDefinition = null;
        }

        private void InvalidateExports(string layoutId) {
            if (ReadOnly) { return; }

            foreach (var thisExport in Export) {
                if (thisExport != null) {
                    if (thisExport.Typ == ExportTyp.EinzelnMitFormular) {
                        if (thisExport.ExportFormularId == layoutId) {
                            thisExport.LastExportTimeUtc = new DateTime(1900, 1, 1);
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
            AddPending(DatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
        }

        //private void OnColumnKeyChanged(KeyChangedEventArgs e) {
        //    if (Disposed) { return; }
        //    ColumnKeyChanged?.Invoke(this, e);
        //}

        private void OnExporting(CancelEventArgs e) {
            if (Disposed) { return; }
            Exporting?.Invoke(this, e);
        }

        private void OnNeedPassword(PasswordEventArgs e) {
            if (Disposed) { return; }
            NeedPassword?.Invoke(this, e);
        }

        //private void OnRowKeyChanged(KeyChangedEventArgs e) {
        //    if (Disposed) { return; }
        //    RowKeyChanged?.Invoke(this, e);
        //}

        private void OnSortParameterChanged() {
            if (Disposed) { return; }
            SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private string ParseThis(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
            if (type is >= DatabaseDataType.Info_ColumDataSart and <= DatabaseDataType.Info_ColumnDataEnd) {
                return column.Load(type, value);
            }
            switch (type) {
                case DatabaseDataType.Formatkennung:
                    break;

                case DatabaseDataType.Version:
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

                case DatabaseDataType.Werbung:
                    break;

                case DatabaseDataType.CryptionState:
                    break;

                case DatabaseDataType.CryptionTest:
                    break;

                case DatabaseDataType.Creator:
                    _creator = value;
                    break;

                case DatabaseDataType.CreateDate:
                    _createDate = value;
                    break;

                case DatabaseDataType.ReloadDelaySecond:
                    base.ReloadDelaySecond = IntParse(value);
                    break;

                case DatabaseDataType.DatenbankAdmin:
                    DatenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                    break;

                case DatabaseDataType.SortDefinition:
                    _sortDefinition = new RowSortDefinition(this, value);
                    break;

                case DatabaseDataType.Caption:
                    _caption = value;
                    break;

                case DatabaseDataType.GlobalScale:
                    _globalScale = DoubleParse(value);
                    break;

                case DatabaseDataType.FilterImagePfad:
                    //_filterImagePfad = value;
                    break;

                case DatabaseDataType.AdditionaFilesPfad:
                    _additionaFilesPfad = value;
                    break;

                case DatabaseDataType.ZeilenQuickInfo:
                    _zeilenQuickInfo = value;
                    break;

                case DatabaseDataType.Ansicht:
                    _ansicht = (Ansicht)IntParse(value);
                    break;

                case DatabaseDataType.Tags:
                    Tags.SplitAndCutByCr(value);
                    break;

                //case enDatabaseDataType.BinaryDataInOne:
                //    break;

                case DatabaseDataType.Layouts:
                    Layouts.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                    break;

                case DatabaseDataType.AutoExport:
                    Export.Clear();
                    List<string> ae = new(value.SplitAndCutByCr());
                    foreach (var t in ae) {
                        Export.Add(new ExportDefinition(this, t));
                    }
                    break;

                //case enDatabaseDataType.Rules_ALT:
                //    //var Rules = new List<RuleItem_Old>();
                //    //var RU = content.SplitAndCutByCr();
                //    //for (var z = 0; z <= RU.GetUpperBound(0); z++) {
                //    //    Rules.Add(new RuleItem_Old(this, RU[z]));
                //    //}
                //    _RulesScript = string.Empty;
                //    break;

                case DatabaseDataType.ColumnArrangement:
                    ColumnArrangements.Clear();
                    List<string> ca = new(value.SplitAndCutByCr());
                    foreach (var t in ca) {
                        ColumnArrangements.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case DatabaseDataType.Views:
                    Views.Clear();
                    List<string> vi = new(value.SplitAndCutByCr());
                    foreach (var t in vi) {
                        Views.Add(new ColumnViewCollection(this, t));
                    }
                    break;

                case DatabaseDataType.PermissionGroups_NewRow:
                    PermissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                    break;

                //case enDatabaseDataType.LastRowKey:
                //    //return Row.Load_310(type, value);
                //    break;

                //case enDatabaseDataType.LastColumnKey:
                //    //return Column.Load_310(type, value);
                //    break;

                case DatabaseDataType.GlobalShowPass:
                    _globalShowPass = value;
                    break;

                case (DatabaseDataType)34:
                    // TODO: Entfernen
                    break;

                case (DatabaseDataType)35:
                    // TODO: Entfernen
                    break;

                case (DatabaseDataType)30:
                    // TODO: Entferne GlobalInfo
                    break;

                case (DatabaseDataType)59:
                    // TODO: Entferne Skin
                    break;

                case (DatabaseDataType)52:
                    // TODO: Entferne Skin
                    break;

                case (DatabaseDataType)61:
                    // TODO: Entfernen
                    break;

                case (DatabaseDataType)60:
                    // TODO: Entfernen
                    break;

                //case enDatabaseDataType.JoinTyp:
                //    //_JoinTyp = (enJoinTyp)IntParse(value);
                //    break;

                case DatabaseDataType.VerwaisteDaten:
                    _verwaisteDaten = (VerwaisteDaten)IntParse(value);
                    break;

                case (DatabaseDataType)63://                    enDatabaseDataType.ImportScript:
                    break;

                case DatabaseDataType.RulesScript:
                    _rulesScript = value;
                    break;

                case DatabaseDataType.FileEncryptionKey:
                    _fileEncryptionKey = value;
                    break;

                case DatabaseDataType.ce_Value_withSizeData:
                case DatabaseDataType.ce_UTF8Value_withSizeData:
                case DatabaseDataType.ce_Value_withoutSizeData:
                    if (type == DatabaseDataType.ce_UTF8Value_withSizeData) {
                        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        var enc1252 = Encoding.GetEncoding(1252);
                        value = Encoding.UTF8.GetString(enc1252.GetBytes(value));
                    }
                    Cell.Load_310(column, row, value, width, height);
                    break;

                case DatabaseDataType.UndoCount:
                    _undoCount = IntParse(value);
                    break;

                case DatabaseDataType.UndoInOne:
                    Works.Clear();
                    var uio = value.SplitAndCutByCr();
                    for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                        WorkItem tmpWork = new(uio[z]) {
                            State = ItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos ändern.
                        };
                        Works.Add(tmpWork);
                    }
                    break;

                case DatabaseDataType.dummyComand_AddRow:
                    var addRowKey = LongParse(value);
                    if (Row.SearchByKey(addRowKey) == null) { Row.Add(new RowItem(this, addRowKey)); }
                    break;

                case DatabaseDataType.AddColumn:
                    var addColumnKey = LongParse(value);
                    if (Column.SearchByKey(addColumnKey) == null) { Column.AddFromParser(new ColumnItem(this, addColumnKey)); }
                    break;

                case DatabaseDataType.dummyComand_RemoveRow:
                    var removeRowKey = LongParse(value);
                    if (Row.SearchByKey(removeRowKey) is RowItem) { Row.Remove(removeRowKey); }
                    break;

                case DatabaseDataType.dummyComand_RemoveColumn:
                    var removeColumnKey = LongParse(value);
                    if (Column.SearchByKey(removeColumnKey) is ColumnItem col) { Column.Remove(col); }
                    break;

                case DatabaseDataType.EOF:
                    return string.Empty;

                default:
                    if (LoadedVersion == DatabaseVersion) {
                        LoadedVersion = "9.99";
                        if (!ReadOnly) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Datei: " + Filename);
                        }
                    }

                    break;
            }
            return string.Empty;
        }

        private bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
            var tmpName = UserName.ToUpper();
            var tmpGroup = UserGroup.ToUpper();
            if (allowed.ToUpper() == "#EVERYBODY") {
                return true;
            }

            if (allowed.ToUpper() == "#ROWCREATOR") {
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
            AddPending(DatabaseDataType.PermissionGroups_NewRow, -1, PermissionGroupsNewRow.JoinWithCr(), false);
        }

        private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
            if (e.Bmp != null) { return; }
            try {
                if (string.IsNullOrWhiteSpace(AdditionaFilesPfadWhole())) { return; }
                if (FileExists(AdditionaFilesPfadWhole() + e.Name + ".png")) {
                    e.Bmp = new BitmapExt(AdditionaFilesPfadWhole() + e.Name + ".png");
                }
            } catch { }
        }

        private void Row_RowAdded(object sender, RowEventArgs e) {
            if (!IsParsing) {
                AddPending(DatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
            }
        }

        private void Row_RowRemoving(object sender, RowEventArgs e) => AddPending(DatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);

        //private string SearchKeyValueInPendingsOf(long RowKey) {
        //    var F = string.Empty;
        //    foreach (var ThisPending in Works) {
        //        if (ThisPending.State == enItemState.Pending) {
        //            if (ThisPending.RowKey == RowKey && ThisPending.Comand == enDatabaseDataType.ce_Value_withoutSizeData && ThisPending.ColKey == Column[0].Key) {
        //                F = ThisPending.ChangedTo;
        //            }
        //        }
        //    }
        //    return F;
        //}

        private void Views_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsParsing) { return; } // hier schon raus, es muss kein ToString ausgeführt werden. Kann zu Endlosschleifen führen.
            AddPending(DatabaseDataType.Views, -1, Views.ToString(true), false);
        }

        #endregion
    }
}