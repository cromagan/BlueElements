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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SQL_Database : DatabaseAbstract, IDisposable, IDisposableExtended {

    #region Fields

    public const string SQL_DatabaseVersion = "4.00";

    public static readonly ListExt<SQL_Database> AllFiles = new();

    public readonly SQLBackAbstract _sql;

    public readonly SQL_CellCollection Cell;

    public readonly SQL_ColumnCollection Column;

    public readonly ListExt<SQL_ColumnViewCollection> ColumnArrangements = new();

    public readonly ListExt<string> DatenbankAdmin = new();

    /// <summary>
    /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
    /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
    /// </summary>
    public readonly ListExt<SQL_ExportDefinition?> Export = new();

    public readonly LayoutCollection Layouts = new();

    public readonly ListExt<string> PermissionGroupsNewRow = new();

    public readonly SQL_RowCollection Row;

    public readonly ListExt<string> Tags = new();

    public readonly string UserName = Generic.UserName().ToUpper();

    public string UserGroup;

    private string _additionaFilesPfad;

    private string? _additionaFilesPfadtmp;

    private string _cachePfad;

    private string _caption = string.Empty;

    private string _createDate = string.Empty;

    private string _creator = string.Empty;

    private double _globalScale;

    //private string _filterImagePfad;
    private string _globalShowPass = string.Empty;

    private string _rulesScript = string.Empty;

    ///// <summary>
    ///// Variable nur temporär für den BinReloader, um mögliche Datenverluste zu entdecken.
    ///// </summary>
    //private string _LastWorkItem = string.Empty;
    private SQL_RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    //private enJoinTyp _JoinTyp;
    private int _undoCount;

    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    //private VerwaisteDaten _verwaisteDaten;
    public SQL_Database(bool readOnly) : this(null, readOnly, string.Empty) { }

    private SQL_Database(SQLBackAbstract sql, bool readOnly, string tablename) {
        AllFiles.Add(this);

        //_muf.Loaded += OnLoaded;
        //_muf.Loading += OnLoading;

        //_muf.RepairAfterParse += RepairAfterParse;
        //_muf.IsThereBackgroundWorkToDo += IsThereBackgroundWorkToDo;
        //_muf.DoBackGroundWork += DoBackGroundWork;

        _sql = sql;
        TableName = tablename.ToUpper();

        Develop.StartService();

        // CopyToSQL = new SqlBack("D:\\" + tablename.FileNameWithoutSuffix() + ".mdf", false);

        Cell = new SQL_CellCollection(this);

        Row = new SQL_RowCollection(this);
        Row.RowRemoving += Row_RowRemoving;
        Row.RowAdded += Row_RowAdded;

        Column = new SQL_ColumnCollection(this);
        Column.ItemRemoving += Column_ItemRemoving;
        Column.ItemRemoved += Column_ItemRemoved;
        Column.ItemAdded += Column_ItemAdded;

        ColumnArrangements.Changed += ColumnArrangements_ListOrItemChanged;
        Layouts.Changed += Layouts_ListOrItemChanged;
        Layouts.ItemSeted += Layouts_ItemSeted;
        PermissionGroupsNewRow.Changed += PermissionGroups_NewRow_ListOrItemChanged;
        Tags.Changed += SQL_DatabaseTags_ListOrItemChanged;
        Export.Changed += Export_ListOrItemChanged;
        DatenbankAdmin.Changed += SQL_DatabaseAdmin_ListOrItemChanged;

        Initialize();

        UserGroup = "#Administrator";
        if (sql != null) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + tablename.FileNameWithoutSuffix()));
            LoadFromSQLBack();
        } else {
            RepairAfterParse();
        }
        QuickImage.NeedImage += QuickImage_NeedImage;
    }

    #endregion

    #region Events

    public event EventHandler Disposing;

    public event EventHandler<MessageEventArgs> DropMessage;

    public event CancelEventHandler Exporting;

    public event EventHandler<SQL_GenerateLayoutInternalEventargs> GenerateLayoutInternal;

    public event EventHandler<LoadedEventArgs> Loaded;

    public event EventHandler<LoadingEventArgs> Loading;

    public event EventHandler<PasswordEventArgs> NeedPassword;

    public event EventHandler<ProgressbarEventArgs> ProgressbarInfo;

    public event EventHandler<SQL_RowCancelEventArgs> ScriptError;

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
            _additionaFilesPfadtmp = null;
            AddPending(DatabaseDataType.AdditionaFilesPath, -1, -1, _additionaFilesPfad, value, true);
            Cell.InvalidateAllSizes();
        }
    }

    [Browsable(false)]
    public string CachePfad {
        get => _cachePfad;
        set {
            if (_cachePfad == value) { return; }
            _cachePfad = value;
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
            AddPending(DatabaseDataType.CreateDateUTC, -1, -1, _createDate, value, true);
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

    public bool IsDisposed { get; private set; }

    public bool IsLoading { get; private set; }

    public string LoadedVersion { get; private set; }

    public DateTime PowerEdit { get; set; }

    public bool ReadOnly { get; private set; }

    public string RulesScript {
        get => _rulesScript;
        set {
            if (_rulesScript == value) { return; }
            AddPending(DatabaseDataType.RulesScript, -1, -1, _rulesScript, value, true);
        }
    }

    [Browsable(false)]
    public SQL_RowSortDefinition SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
            if (value != null) { neu = value.ToString(); }
            if (alt == neu) { return; }
            AddPending(DatabaseDataType.SortDefinition, -1, -1, alt, neu, false);
            _sortDefinition = new SQL_RowSortDefinition(this, neu);
            OnSortParameterChanged();
        }
    }

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Browsable(false)]
    [Description("Das standardmäßige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            AddPending(DatabaseDataType.StandardFormulaFile, -1, -1, _standardFormulaFile, value, true);
        }
    }

    public string TableName { get; private set; } = string.Empty;

    [Browsable(false)]
    public int UndoCount {
        get => _undoCount;
        set {
            if (_undoCount == value) { return; }
            AddPending(DatabaseDataType.UndoCount, -1, -1, _undoCount.ToString(), value.ToString(), true);
        }
    }

    [Browsable(false)]
    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            AddPending(DatabaseDataType.RowQuickInfo, -1, -1, _zeilenQuickInfo, value, true);
        }
    }

    #endregion

    #region Methods

    //public VerwaisteDaten VerwaisteDaten {
    //    get => _verwaisteDaten;
    //    set {
    //        if (_verwaisteDaten == value) { return; }
    //        AddPending(DatabaseDataType.VerwaisteDaten, -1, -1, ((int)_verwaisteDaten).ToString(), ((int)value).ToString(), true);
    //    }
    //}
    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="checkOnlyCaptionToo"></param>
    /// <param name="readOnly"></param>
    /// <returns></returns>
    public static SQL_Database? GetByFilename(string tablename, bool readOnly, SQLBackAbstract? sql) {
        if (string.IsNullOrEmpty(tablename)) { return null; }

        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.TableName, tablename, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }
        if (sql == null) { return null; }

        return new SQL_Database(sql.OtherTable(tablename), readOnly, tablename);
    }

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AdditionaFilesPfadWhole() {
        if (!string.IsNullOrEmpty(_additionaFilesPfadtmp)) { return _additionaFilesPfadtmp; }

        var t = _additionaFilesPfad.CheckPath();
        if (DirectoryExists(t)) {
            _additionaFilesPfadtmp = t;
            return t;
        }

        _additionaFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public List<SQL_RowData?> AllRows() {
        var sortedRows = new List<SQL_RowData?>();
        foreach (var thisSQL_RowItem in Row) {
            if (thisSQL_RowItem != null) {
                sortedRows.Add(new SQL_RowData(thisSQL_RowItem));
            }
        }
        return sortedRows;
    }

    //    return columnAll.SortedDistinctList();
    //}
    public bool BlockSaveOperations() => SQL_RowItem.DoingScript;

    public void CloneDataFrom(SQL_Database sourceSQL_Database) {

        #region Einstellungen der Ursprünglichen Datenbank auf die Kopie übertragen

        RulesScript = sourceSQL_Database.RulesScript;
        GlobalShowPass = sourceSQL_Database.GlobalShowPass;
        DatenbankAdmin.CloneFrom(sourceSQL_Database.DatenbankAdmin);
        PermissionGroupsNewRow.CloneFrom(sourceSQL_Database.PermissionGroupsNewRow);
        //ReloadDelaySecond = sourceSQL_Database.ReloadDelaySecond;
        //VerwaisteDaten = sourceSQL_Database.VerwaisteDaten;
        ZeilenQuickInfo = sourceSQL_Database.ZeilenQuickInfo;
        StandardFormulaFile = sourceSQL_Database.StandardFormulaFile;

        if (SortDefinition.ToString() != sourceSQL_Database.SortDefinition.ToString()) {
            SortDefinition = new SQL_RowSortDefinition(this, sourceSQL_Database.SortDefinition.ToString());
        }

        foreach (var ThisColumn in Column) {
            var l = sourceSQL_Database.Column.Exists(ThisColumn.Name);
            if (l != null) {
                ThisColumn.CloneFrom(l);
            }
        }

        #endregion
    }

    //    foreach (var thisSQL_ColumnItem in Column) {
    //        if (thisSQL_ColumnItem != null && thisSQL_ColumnItem.Format == DataFormat.Link_To_Filesystem) {
    //            var tmp = thisSQL_ColumnItem.Contents();
    //            Parallel.ForEach(tmp, thisTmp => {
    //                var x = thisSQL_ColumnItem.BestFile(thisTmp, false).ToLower();
    //                lock (lockMe) {
    //                    columnAll.Add(x);
    //                }
    //            });
    //        }
    //    }
    //    //foreach (var ThisSQL_ColumnItem in Column) {
    //    //    if (ThisSQL_ColumnItem != null) {
    //    //        if (ThisSQL_ColumnItem.Format == DataFormat.Link_To_Filesystem) {
    //    //            var tmp = ThisSQL_ColumnItem.Contents();
    //    //            foreach (var thisTmp in tmp) {
    //    //                Column_All.AddIfNotExists(ThisSQL_ColumnItem.BestFile(thisTmp, false).ToLower());
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //public void CancelBackGroundWorker() {
    //    _muf.CancelBackGroundWorker();
    //}
    /// <summary>
    /// Datenbankpfad mit Forms und abschließenden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() => string.IsNullOrEmpty(AdditionaFilesPfadWhole()) ? string.Empty : AdditionaFilesPfadWhole() + "Forms\\";

    //public List<string> AllConnectedFilesLCase() {
    //    List<string> columnAll = new();
    //    var lockMe = new object();
    /// <summary>
    /// Datenbankpfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() => string.IsNullOrEmpty(AdditionaFilesPfadWhole()) ? string.Empty : AdditionaFilesPfadWhole() + "Layouts\\";

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason(ErrorReason mode) {
        //var f = _muf.ErrorReason(mode);

        //if (!string.IsNullOrEmpty(f)) { return f; }
        if (mode == BlueBasics.Enums.ErrorReason.OnlyRead) { return string.Empty; }

        return IntParse(LoadedVersion.Replace(".", "")) > IntParse(SQL_DatabaseVersion.Replace(".", ""))
            ? "Diese Programm kann nur Datenbanken bis Version " + SQL_DatabaseVersion + " speichern."
            : string.Empty;
    }

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="column">Die Spalte, die zurückgegeben wird.</param>
    /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle Zeilen zurück.</param>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, SQL_ColumnItem? column, List<SQL_RowData>? sortedRows) =>
        //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        Export_CSV(firstRow, new List<SQL_ColumnItem> { column }, sortedRows);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="columnList">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
    /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, List<SQL_ColumnItem>? columnList, List<SQL_RowData>? sortedRows) {
        if (columnList == null) {
            columnList = Column.Where(thisSQL_ColumnItem => thisSQL_ColumnItem != null).ToList();
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
    public string Export_CSV(FirstRow firstRow, SQL_ColumnViewCollection? arrangement, List<SQL_RowData>? sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, int arrangementNo, SQL_FilterCollection? filter, List<SQL_RowItem>? pinned) => Export_CSV(firstRow, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null));

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, int arrangementNo, SQL_FilterCollection? filter, List<SQL_RowItem>? pinned) => Export_HTML(filename, ColumnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null), false);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, List<SQL_ColumnItem?>? columnList, List<SQL_RowData?>? sortedRows, bool execute) {
        if (columnList == null || columnList.Count == 0) {
            columnList = Column.Where(thisSQL_ColumnItem => thisSQL_ColumnItem != null).ToList();
        }

        if (sortedRows == null) { sortedRows = AllRows(); }

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        Html da = new(TableName);
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
                            (lcColumn, lCrow, _) = SQL_CellCollection.LinkedCellData(thisColumn, thisRow.Row, false, false);
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
    public void Export_HTML(string filename, SQL_ColumnViewCollection? arrangement, List<SQL_RowData?> sortedRows, bool execute) => Export_HTML(filename, arrangement.ListOfUsedColumn(), sortedRows, execute);

    /// <summary>
    /// Testet die Standard-Verzeichnisse und gibt das Formular zurück, falls es existiert
    /// </summary>
    /// <returns></returns>
    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionaFilesPfadWhole() + _standardFormulaFile)) { return AdditionaFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
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
                var x = new BlueScript.Script(vars, string.Empty, false);
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

        List<SQL_ColumnItem?> columns = new();
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
            SQL_RowItem? row = null;
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

        if (x.Count < 2) { return "Keine Zeilen zum importieren."; }

        var sep = ",";

        if (x[0].StartsWith("sep=", StringComparison.OrdinalIgnoreCase)) {
            if (x.Count < 3) { return "Keine Zeilen zum importieren."; }
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

    public void OnScriptError(SQL_RowCancelEventArgs e) {
        if (IsDisposed) { return; }
        ScriptError?.Invoke(this, e);
    }

    public List<string> Permission_AllUsed(bool cellLevel) {
        List<string> e = new();
        foreach (var thisSQL_ColumnItem in Column) {
            if (thisSQL_ColumnItem != null) {
                e.AddRange(thisSQL_ColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(PermissionGroupsNewRow);
        e.AddRange(DatenbankAdmin);
        foreach (var thisArrangement in ColumnArrangements) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }
        //foreach (var thisArrangement in OldFormulaViews) {
        //    e.AddRange(thisArrangement.PermissionGroups_Show);
        //}
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

    public bool PermissionCheck(ListExt<string>? allowed, SQL_RowItem? row) {
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

    public void RepairAfterParse() {
        Column.Repair();
        CheckViewsAndArrangements();
        Layouts.Check();
    }

    public string UndoText(SQL_ColumnItem? column, SQL_RowItem? row) {
        //if (Works == null || Works.Count == 0) { return string.Empty; }
        //var cellKey = SQL_CellCollection.KeyOfCell(column, row);
        var t = "";
        //for (var z = Works.Count - 1; z >= 0; z--) {
        //    if (Works[z] != null && Works[z].CellKey == cellKey) {
        //        if (Works[z].HistorischRelevant) {
        //            t = t + Works[z].UndoTextTableMouseOver() + "<br>";
        //        }
        //    }
        //}
        //t = t.Trim("<br>");
        //t = t.Trim("<hr>");
        //t = t.Trim("<br>");
        //t = t.Trim("<hr>");
        return t;
    }

    internal void AddPending(DatabaseDataType comand, SQL_ColumnItem column, string previousValue, string changedTo, bool executeNow) => AddPending(comand, column.Key, -1, previousValue, changedTo, executeNow);

    internal void AddPending(DatabaseDataType comand, long columnKey, string listExt, bool executeNow) => AddPending(comand, columnKey, -1, "", listExt, executeNow);

    internal void AddPending(DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, bool executeNow) {
        if (executeNow) {
            ParseThis(comand, changedTo, Column.SearchByKey(columnKey), Row.SearchByKey(rowKey), -1, -1);
        }
        if (IsLoading) { return; }
        if (ReadOnly) {
            if (!string.IsNullOrEmpty(TableName)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + TableName);
            }
            return;
        }
        // Keine Doppelten Rausfiltern, ansonstn stimmen die Undo nicht mehr

        //if (comand != DatabaseDataType.AutoExport) { _muf.SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

        if (rowKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
        if (columnKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }
        //Works.Add(new WorkItem(comand, columnKey, rowKey, previousValue, changedTo, UserName));

        _sql.AddUndo(TableName, comand, columnKey, rowKey, previousValue, changedTo, UserName);
    }

    internal void Column_NameChanged(string oldName, SQL_ColumnItem newName) {
        if (string.IsNullOrEmpty(oldName)) { return; }
        // Cells ----------------------------------------------
        //   Cell.ChangeCaptionName(OldName, cSQL_ColumnItem.Name, cSQL_ColumnItem)
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

    internal string Column_UsedIn(SQL_ColumnItem? column) {
        var t = string.Empty;
        if (SortDefinition.Columns.Contains(column)) { t += " - Sortierung<br>"; }
        //var view = false;
        //foreach (var thisView in OldFormulaViews) {
        //    if (thisView[column] != null) { view = true; }
        //}
        //if (view) { t += " - Formular-Ansichten<br>"; }
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
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + TableName;
        } catch { }
        Develop.DebugPrint(FehlerArt.Warnung, t);
    }

    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    internal void OnGenerateLayoutInternal(SQL_GenerateLayoutInternalEventargs e) {
        if (IsDisposed) { return; }
        GenerateLayoutInternal?.Invoke(this, e);
    }

    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
    }

    internal void OnViewChanged() {
        if (IsDisposed) { return; }
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(byte[] b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private void CheckViewsAndArrangements() {
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Temporär erzeugt werden!

        if (IsLoading) { return; }

        for (var z = 0; z < Math.Max(2, ColumnArrangements.Count); z++) {
            if (ColumnArrangements.Count < z + 1) { ColumnArrangements.Add(new SQL_ColumnViewCollection(this, string.Empty)); }
            ColumnArrangements[z].Repair(z, true);
        }

        //for (var z = 0; z < Math.Max(2, OldFormulaViews.Count); z++) {
        //    if (OldFormulaViews.Count < z + 1) { OldFormulaViews.Add(new ColumnViewCollection(this, string.Empty)); }
        //    OldFormulaViews[z].Repair(z, false);
        //}
    }

    private void Column_ItemAdded(object sender, ListEventArgs e) => CheckViewsAndArrangements();

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
        if (IsLoading) { Develop.DebugPrint(FehlerArt.Warnung, "Parsing Falsch!"); }
        CheckViewsAndArrangements();

        Layouts.Check();
    }

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
    private void Column_ItemRemoving(object sender, ListEventArgs e) {
        var key = ((SQL_ColumnItem)e.Item).Key;
        AddPending(DatabaseDataType.dummyComand_RemoveColumn, key, -1, string.Empty, key.ToString(), false);
    }

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
    private void ColumnArrangements_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt werden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(), false);
    }

    private void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        IsDisposed = true;

        OnDisposing();
        AllFiles.Remove(this);

        //base.Dispose(disposing); // speichert und löscht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }
        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
        // TODO: große Felder auf Null setzen.
        ColumnArrangements.Changed -= ColumnArrangements_ListOrItemChanged;
        Layouts.Changed -= Layouts_ListOrItemChanged;
        Layouts.ItemSeted -= Layouts_ItemSeted;
        PermissionGroupsNewRow.Changed -= PermissionGroups_NewRow_ListOrItemChanged;
        Tags.Changed -= SQL_DatabaseTags_ListOrItemChanged;
        Export.Changed -= Export_ListOrItemChanged;
        DatenbankAdmin.Changed -= SQL_DatabaseAdmin_ListOrItemChanged;

        Row.RowRemoving -= Row_RowRemoving;
        Row.RowAdded -= Row_RowAdded;

        Column.ItemRemoving -= Column_ItemRemoving;
        Column.ItemRemoved -= Column_ItemRemoved;
        Column.ItemAdded -= Column_ItemAdded;
        Column.Dispose();
        Cell.Dispose();
        Row.Dispose();
        ColumnArrangements.Dispose();
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
    private void DoBackGroundWork(object sender, MultiUserFileBackgroundWorkerEventArgs e) {
        if (ReadOnly) { return; }

        foreach (var thisExport in Export) {
            if (e.BackgroundWorker.CancellationPending) { return; }

            if (thisExport.IsOk()) {
                var e2 = new MultiUserFileHasPendingChangesEventArgs();

                if (!e2.HasPendingChanges) {
                    CancelEventArgs ec = new(false);
                    OnExporting(ec);
                    if (ec.Cancel) { return; }
                }

                thisExport.DeleteOutdatedBackUps(e.BackgroundWorker);
                if (e.BackgroundWorker.CancellationPending) { return; }
                thisExport.DoBackUp(e.BackgroundWorker);
                if (e.BackgroundWorker.CancellationPending) { return; }
            }
        }
    }

    private void Export_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.AutoExport, -1, Export.ToString(true), false);
    }

    private void Initialize() {
        Cell.Initialize();
        ColumnArrangements.Clear();
        Layouts.Clear();
        PermissionGroupsNewRow.Clear();
        Tags.Clear();
        Export.Clear();
        DatenbankAdmin.Clear();
        _globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _undoCount = 300;
        _caption = string.Empty;
        //_verwaisteDaten = VerwaisteDaten.Ignorieren;
        LoadedVersion = SQL_DatabaseVersion;
        _rulesScript = string.Empty;
        _globalScale = 1f;
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

    private void IsThereBackgroundWorkToDo(object sender, MultiUserIsThereBackgroundWorkToDoEventArgs e) {
        //var e2 = new MultiUserFileHasPendingChangesEventArgs();
        //HasPendingChanges(null, e2);

        //if (e2.HasPendingChanges) { e.BackGroundWork = true; return; }
        CancelEventArgs ec = new(false);
        OnExporting(ec);
        if (ec.Cancel) { return; }

        foreach (var thisExport in Export) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) { e.BackGroundWork = true; return; }
                if (DateTime.UtcNow.Subtract(thisExport.LastExportTimeUtc).TotalDays > thisExport.BackupInterval * 50) { e.BackGroundWork = true; return; }
            }
        }
        return;
    }

    private void Layouts_ItemSeted(object sender, ListEventArgs? e) {
        if (e != null) {
            var x = (string)e.Item;
            if (!x.StartsWith("{ID=#")) { Develop.DebugPrint("ID nicht gefunden: " + x); }
            var ko = x.IndexOf(", ", StringComparison.Ordinal);
            var id = x.Substring(4, ko - 4);
            InvalidateExports(id);
        }
    }

    private void Layouts_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
    }

    private void LoadFromSQLBack() {
        IsLoading = true;

        #region Spalten erstellen

        var cols = _sql.GetColumnNames(SQLBackAbstract.Prefix_Table + TableName.ToUpper());
        cols.Remove("RK");

        foreach (var thisCol in cols) {
            Column.Add(thisCol, _sql);
        }

        Column.GetSystems();

        #endregion

        #region Datenbank Eigenschaften laden

        var l = _sql.GetStylDataAll(TableName, string.Empty);
        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                ParseThis(thisstyle.Key, thisstyle.Value);
            }
        }

        #endregion

        #region  Alle Zellen laden

        _sql.LoadAllCells(TableName, Row);

        #endregion

        IsLoading = false;

        RepairAfterParse();
    }

    private void OnDisposing() {
        Disposing?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnExporting(CancelEventArgs e) {
        if (IsDisposed) { return; }
        Exporting?.Invoke(this, e);
    }

    private void OnLoaded(object sender, LoadedEventArgs e) {
        if (IsDisposed) { return; }
        Loaded?.Invoke(this, e);
    }

    private void OnLoading(object sender, LoadingEventArgs e) {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, e);
    }

    private void OnNeedPassword(PasswordEventArgs e) {
        if (IsDisposed) { return; }
        NeedPassword?.Invoke(this, e);
    }

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private string ParseThis(string type, string value) {
        //if (IsLoading) { return string.Empty; }

        //CopyToSQL.SetStyleData(Filename.FileNameWithoutSuffix(), type);

        switch (type.ToLower()) {
            case "version":
                break;

            case "creator":
                _creator = value;
                break;

            case "createdateutc":
                _createDate = value;
                break;

            case "reloaddelaysecond":
                //_muf.ReloadDelaySecond = IntParse(value);
                break;

            case "databaseadmingroups":
                DatenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case "sortdefinition":
                _sortDefinition = new SQL_RowSortDefinition(this, value);
                break;

            case "caption":
                _caption = value;
                break;

            case "globalscale":
                _globalScale = DoubleParse(value);
                break;

            case "additionafilespfad":
                _additionaFilesPfad = value;
                break;

            case "standardformulafile":
                _standardFormulaFile = value;
                break;

            case "rowquickinfo":
                _zeilenQuickInfo = value;
                break;

            case "tags":
                Tags.SplitAndCutByCr(value);
                break;

            case "layouts":
                Layouts.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case "autoexport":
                Export.Clear();
                List<string> ae = new(value.SplitAndCutByCr());
                foreach (var t in ae) {
                    Export.Add(new SQL_ExportDefinition(this, t));
                }
                break;

            case "columnarrangement":
                ColumnArrangements.Clear();
                List<string> ca = new(value.SplitAndCutByCr());
                foreach (var t in ca) {
                    ColumnArrangements.Add(new SQL_ColumnViewCollection(this, t));
                }
                break;

            case "permissiongroupsnewrow":
                PermissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case "globalshowpass":
                _globalShowPass = value;
                break;

            case "rulesscript":
                _rulesScript = value;
                break;

            case "undocount":
                _undoCount = IntParse(value);
                break;

            case "undoonone":
                //Works.Clear();
                //var uio = value.SplitAndCutByCr();
                //for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                //    WorkItem tmpWork = new(uio[z]) {
                //        State = ItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos ändern.
                //    };
                //    Works.Add(tmpWork);
                //}
                break;

            default:
                return "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + " <br> Table: " + TableName;
                //if (LoadedVersion == SQL_DatabaseVersion) {
                //    LoadedVersion = "9.99";
                //    if (!ReadOnly) {
                //        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert": " + value + "<br>Datei": " + Filename);
                //    }
                //}

                break;
        }
        return string.Empty;
    }

    private string ParseThis(DatabaseDataType type, string value, SQL_ColumnItem? column, SQL_RowItem? row, int width, int height) {
        if (IsLoading) { return string.Empty; }
        _sql.CheckIn(TableName, type, value, column, row, width, height);

        switch (type) {
            case DatabaseDataType.Formatkennung:
                break;

            case DatabaseDataType.Version:
                LoadedVersion = value.Trim();
                if (LoadedVersion != SQL_DatabaseVersion) {
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

            case DatabaseDataType.CreateDateUTC:
                _createDate = value;
                break;

            case DatabaseDataType.ReloadDelaySecond:
                //_muf.ReloadDelaySecond = IntParse(value);
                break;

            case DatabaseDataType.DatabaseAdminGroups:
                DatenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.SortDefinition:
                _sortDefinition = new SQL_RowSortDefinition(this, value);
                break;

            case DatabaseDataType.Caption:
                _caption = value;
                break;

            case DatabaseDataType.GlobalScale:
                _globalScale = DoubleParse(value);
                break;

            case (DatabaseDataType)65://DatabaseDataType.FilterImagePfad:
                //_filterImagePfad = value;
                break;

            case DatabaseDataType.AdditionaFilesPath:
                _additionaFilesPfad = value;
                break;

            case DatabaseDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case DatabaseDataType.RowQuickInfo:
                _zeilenQuickInfo = value;
                break;

            case (DatabaseDataType)53: //DatabaseDataType.Ansicht:
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
                    Export.Add(new SQL_ExportDefinition(this, t));
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
                    ColumnArrangements.Add(new SQL_ColumnViewCollection(this, t));
                }
                break;

            case (DatabaseDataType)33://DatabaseDataType.Views:
                //OldFormulaViews.Clear();
                //List<string> vi = new(value.SplitAndCutByCr());
                //foreach (var t in vi) {
                //    OldFormulaViews.Add(new ColumnViewCollection(this, t));
                //}
                break;

            case DatabaseDataType.PermissionGroupsNewRow:
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

            case (DatabaseDataType)62://DatabaseDataType.VerwaisteDaten:
                //_verwaisteDaten = (VerwaisteDaten)IntParse(value);
                break;

            case (DatabaseDataType)63://                    enDatabaseDataType.ImportScript:
                break;

            case DatabaseDataType.RulesScript:
                _rulesScript = value;
                break;

            case (DatabaseDataType)22://  DatabaseDataType.FileEncryptionKey:
                //_fileEncryptionKey = value;
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
                //Works.Clear();
                //var uio = value.SplitAndCutByCr();
                //for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                //    WorkItem tmpWork = new(uio[z]) {
                //        State = ItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos ändern.
                //    };
                //    Works.Add(tmpWork);
                //}
                break;

            case DatabaseDataType.dummyComand_AddRow:
                var addRowKey = LongParse(value);
                if (Row.SearchByKey(addRowKey) == null) { Row.Add(new SQL_RowItem(this, addRowKey)); }
                break;

            case DatabaseDataType.AddColumn:
                //    var addColumnKey = LongParse(value);
                //    if (Column.SearchByKey(addColumnKey) == null) { Column.AddFromParser(new SQL_ColumnItem(this, addColumnKey)); }
                break;

            case DatabaseDataType.dummyComand_RemoveRow:
                var removeRowKey = LongParse(value);
                if (Row.SearchByKey(removeRowKey) is SQL_RowItem) { Row.Remove(removeRowKey); }
                break;

            case DatabaseDataType.dummyComand_RemoveColumn:
                var removeColumnKey = LongParse(value);
                if (Column.SearchByKey(removeColumnKey) is SQL_ColumnItem col) { Column.Remove(col); }
                break;

            case DatabaseDataType.EOF:
                return string.Empty;

            default:
                if (LoadedVersion == SQL_DatabaseVersion) {
                    LoadedVersion = "9.99";
                    if (!ReadOnly) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Table: " + TableName);
                    }
                }

                break;
        }
        return string.Empty;
    }

    private bool PermissionCheckWithoutAdmin(string allowed, SQL_RowItem? row) {
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
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.PermissionGroupsNewRow, -1, PermissionGroupsNewRow.JoinWithCr(), false);
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            e.Done = true;

            if (string.IsNullOrWhiteSpace(AdditionaFilesPfadWhole())) { return; }

            var name = e.Name.RemoveChars(Constants.Char_DateiSonderZeichen);
            var hashname = name.GetHashString();

            var fullname = AdditionaFilesPfadWhole() + name + ".png";
            var fullhashname = CachePfad.TrimEnd("\\") + "\\" + hashname;

            if (!string.IsNullOrWhiteSpace(CachePfad)) {
                if (FileExists(fullhashname)) {
                    FileInfo f = new(fullhashname);
                    if (DateTime.Now.Subtract(f.CreationTime).TotalDays < 10) {
                        if (f.Length < 5) { return; }
                        e.Bmp = new BitmapExt(fullhashname);
                        return;
                    }
                    DeleteFile(fullhashname, false);
                }
            }

            if (FileExists(fullname)) {
                e.Bmp = new BitmapExt(fullname);
                if (!string.IsNullOrWhiteSpace(CachePfad)) {
                    BlueBasics.IO.CopyFile(fullname, fullhashname, false);
                }
                return;
            }

            var l = new List<string>();
            l.Save(fullhashname, Encoding.UTF8, false);
        } catch { }
    }

    private void Row_RowAdded(object sender, SQL_RowEventArgs e) {
        //if (!_muf.IsParsing) {
        //    AddPending(DatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
        //}
    }

    private void Row_RowRemoving(object sender, SQL_RowEventArgs e) => AddPending(DatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);

    private void SQL_DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.DatabaseAdminGroups, -1, DatenbankAdmin.JoinWithCr(), false);
    }

    //            protected override void OnItemAdded(SQL_ColumnItem item) {//    base.OnItemAdded(item);//    SQL_Database.CheckViewsAndArrangements();//}//protected override void OnItemRemoved() {//    base.OnItemRemoved();//    SQL_Database.CheckViewsAndArrangements();//    SQL_Database.Layouts.Check();//}
    private void SQL_DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsLoading) { return; } // hier schon raus, es muss kein ToString ausgeführt wetrden. Kann zu Endlosschleifen führen.
        AddPending(DatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
    }

    #endregion
}