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
using System.Reflection;
using System.Text;
using System.Threading;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatabaseAbstract : IDisposable, IDisposableExtended {

    #region Fields

    public const string DatabaseVersion = "4.00";

    public static readonly ListExt<DatabaseAbstract> AllFiles = new();
    public readonly ListExt<ColumnViewCollection> _columnArrangements = new();
    public readonly List<string> _datenbankAdmin = new();
    public readonly CellCollection Cell;

    public readonly ColumnCollection Column;

    public readonly RowCollection Row;
    public readonly string TableName = string.Empty;
    public readonly string UserName = Generic.UserName().ToUpper();
    public DatabaseAbstract? Mirror;

    public string UserGroup;

    private readonly BackgroundWorker _backgroundWorker;

    private readonly Timer _checker;

    private readonly LayoutCollection _layouts = new();
    private readonly List<string> _permissionGroupsNewRow = new();
    private readonly long _startTick = DateTime.UtcNow.Ticks;

    private readonly List<string> _tags = new();
    private string _additionaFilesPfad;

    private string? _additionaFilesPfadtmp;

    private string _cachePfad;

    private string _caption = string.Empty;

    private int _checkerTickCount = -5;

    private string _createDate = string.Empty;

    private string _creator = string.Empty;

    /// <summary>
    /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
    /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
    /// </summary>
    private ListExt<ExportDefinition?> _export = new();

    private double _globalScale;
    private string _globalShowPass = string.Empty;
    private DateTime _lastUserActionUtc = new(1900, 1, 1);
    private int _reloadDelaySecond;
    private string _rulesScript = string.Empty;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    private int _undoCount;

    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    protected DatabaseAbstract(string tablename, bool readOnly) {
        ReadOnly = readOnly;
        TableName = tablename.ToUpper();
        UserGroup = "#Administrator";
        Cell = new CellCollection(this);

        QuickImage.NeedImage += QuickImage_NeedImage;

        Row = new RowCollection(this);
        Row.RowRemoving += Row_RowRemoving;
        Row.RowAdded += Row_RowAdded;

        Column = new ColumnCollection(this);
        Column.ItemRemoving += Column_ItemRemoving;
        Column.ItemRemoved += Column_ItemRemoved;
        Column.ItemAdded += Column_ItemAdded;

        _backgroundWorker = new BackgroundWorker {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true
        };
        _backgroundWorker.DoWork += BackgroundWorker_DoWork;
        _checker = new Timer(Checker_Tick);
        _checker.Change(2000, 2000);
    }

    #endregion

    #region Events

    public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;

    public event EventHandler Disposing;

    public event EventHandler<MessageEventArgs> DropMessage;

    public event CancelEventHandler Exporting;

    public event EventHandler<GenerateLayoutInternalEventargs> GenerateLayoutInternal;

    public event EventHandler<LoadedEventArgs> Loaded;

    public event EventHandler<LoadingEventArgs> Loading;

    public event EventHandler<PasswordEventArgs> NeedPassword;

    public event EventHandler<ProgressbarEventArgs> ProgressbarInfo;

    public event EventHandler SavedToDisk;

    public event EventHandler<RowCancelEventArgs> ScriptError;

    /// <summary>
    /// Dient dazu, offene Dialoge abzufragen
    /// </summary>
    public event EventHandler<CancelEventArgs> ShouldICancelSaveOperations;

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
            ChangeData(DatabaseDataType.AdditionaFilesPath, null, null, _additionaFilesPfad, value);
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
            ChangeData(DatabaseDataType.Caption, null, null, _caption, value);
        }
    }

    public ListExt<ColumnViewCollection> ColumnArrangements {
        get => _columnArrangements;
        set {
            if (_columnArrangements.ToString() ==  value.ToString()) { return; }
            ChangeData(DatabaseDataType.ColumnArrangement, null, null, _columnArrangements.ToString(), value.ToString());
            OnViewChanged();
        }
    }

    public abstract string ConnectionID { get; }

    [Browsable(false)]
    public string CreateDate {
        get => _createDate;
        set {
            if (_createDate == value) { return; }
            ChangeData(DatabaseDataType.CreateDateUTC, null, null, _createDate, value);
        }
    }

    [Browsable(false)]
    public string Creator {
        get => _creator.Trim();
        set {
            if (_creator == value) { return; }
            ChangeData(DatabaseDataType.Creator, null, null, _creator, value);
        }
    }

    public List<string> DatenbankAdmin {
        get => _datenbankAdmin;
        set {
            if (!_datenbankAdmin.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.DatabaseAdminGroups, null, null, _datenbankAdmin.JoinWithCr(), value.JoinWithCr());
        }
    }

    public ListExt<ExportDefinition> Export {
        get => _export;
        set {
            if (_export.ToString() == value.ToString()) { return; }
            ChangeData(DatabaseDataType.AutoExport, null, null, _export.ToString(true), value.ToString(true));
        }
    }

    public abstract string Filename { get; }

    [Browsable(false)]
    public double GlobalScale {
        get => _globalScale;
        set {
            if (_globalScale == value) { return; }
            ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToString(CultureInfo.InvariantCulture), value.ToString(CultureInfo.InvariantCulture));
            Cell.InvalidateAllSizes();
        }
    }

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            ChangeData(DatabaseDataType.GlobalShowPass, null, null, _globalShowPass, value);
        }
    }

    public bool IsDisposed { get; private set; }

    public abstract bool IsLoading { get; protected set; }

    public LayoutCollection Layouts {
        get => _layouts;
        set {
            if (!_layouts.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.Layouts, null, null, _layouts.JoinWithCr(), value.JoinWithCr());
        }
    }

    public string LoadedVersion { get; private set; }

    public List<string>? PermissionGroupsNewRow {
        get => _permissionGroupsNewRow;
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.PermissionGroupsNewRow, null, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr());
        }
    }

    public DateTime PowerEdit { get; set; }

    public bool ReadOnly { get; private set; }

    [Browsable(false)]
    public int ReloadDelaySecond {
        get => _reloadDelaySecond;
        set {
            if (_reloadDelaySecond == value) { return; }
            ChangeData(DatabaseDataType.ReloadDelaySecond, null, null, _reloadDelaySecond.ToString(), value.ToString());
        }
    }

    public abstract bool ReloadNeeded { get; }

    public abstract bool ReloadNeededSoft { get; }

    public string RulesScript {
        get => _rulesScript;
        set {
            if (_rulesScript == value) { return; }
            ChangeData(DatabaseDataType.RulesScript, null, null, _rulesScript, value);
        }
    }

    [Browsable(false)]
    public RowSortDefinition? SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
            if (value != null) { neu = value.ToString(); }
            if (alt == neu) { return; }
            ChangeData(DatabaseDataType.SortDefinition, null, null, alt, neu);

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
            ChangeData(DatabaseDataType.StandardFormulaFile, null, null, _standardFormulaFile, value);
        }
    }

    public List<string> Tags {
        get => _tags;
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.Tags, null, null, _tags.JoinWithCr(), value.JoinWithCr());
        }
    }

    [Browsable(false)]
    public int UndoCount {
        get => _undoCount;
        set {
            if (_undoCount == value) { return; }
            ChangeData(DatabaseDataType.UndoCount, null, null, _undoCount.ToString(), value.ToString());
        }
    }

    [Browsable(false)]
    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            ChangeData(DatabaseDataType.RowQuickInfo, null, null, _zeilenQuickInfo, value);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="checkOnlyCaptionToo"></param>
    /// <param name="readOnly"></param>
    /// <returns></returns>
    public static DatabaseAbstract? GetByID(string connectionID, bool checkOnlyCaptionToo, bool readOnly, DatabaseAbstract? vorlage, string tablename) {
        if (string.IsNullOrEmpty(connectionID)) { return null; }

        foreach (var thisFile in AllFiles) {
            if (thisFile is DatabaseAbstract db && string.Equals(thisFile.ConnectionID, connectionID, StringComparison.OrdinalIgnoreCase)) {
                thisFile.BlockReload(false);
                return db;
            }
        }

        #region wenn captiontoo angewählt wurde, schauen ob eine ConnectionID einem Dateinamen entspricht

        if (checkOnlyCaptionToo) {
            foreach (var thisFile in AllFiles) {
                if (thisFile is DatabaseAbstract db && thisFile.TableName.ToLower() == connectionID.ToLower().FileNameWithSuffix()) {
                    thisFile.BlockReload(false);
                    return db;
                }
            }
        }

        #endregion

        if (vorlage != null) {
            return vorlage.GetOtherTable(tablename, readOnly);
        }

        #region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        if (FileExists(connectionID)) {
            if (connectionID.FileSuffix().ToLower() == "mdb") {
                return new Database(connectionID, readOnly, false, tablename);
            }

            if (connectionID.FileSuffix().ToLower() == "mdf") {
                var x = new SQLBackMicrosoftCE(connectionID, false);

                var tables = x.Tables();
                var tabn = string.Empty;
                if (tables.Count >= 1) { tabn = tables[0]; }

                if (string.IsNullOrEmpty(tabn)) {
                    tabn = "MAIN";
                    x.RepairAll(tabn);
                }

                return new DatabaseSQL(x, false, tabn);
            }
        }

        #endregion

        //SQLBackAbstract.GetSQLBacks();

        if (SQLBackAbstract.ConnectedSQLBack != null) {
            foreach (var thisSQL in SQLBackAbstract.ConnectedSQLBack) {
                var h = thisSQL.HandleMe(connectionID);
                if (h != null) { return new DatabaseSQL(h, readOnly, tablename); }
            }
        }

        return null;
    }

    public static DatabaseAbstract? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream, SQLBackAbstract? sql) {
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
                    var tmp = GetByID(pf, false, false, null, pf.FileNameWithoutSuffix());
                    if (tmp != null) { return tmp; }
                    tmp = new Database(pf, false, false, pf.FileNameWithoutSuffix());
                    return tmp;
                }
            } while (pf != string.Empty);
        }
        var d = Generic.GetEmmbedResource(assembly, name);
        if (d != null) { return new Database(d, name); }
        if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AdditionaFilesPfadWhole() {
        if (_additionaFilesPfadtmp != null) { return _additionaFilesPfadtmp; }

        if (!string.IsNullOrEmpty(_additionaFilesPfad)) {
            var t = _additionaFilesPfad.CheckPath();
            if (DirectoryExists(t)) {
                _additionaFilesPfadtmp = t;
                return t;
            }
        }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + _additionaFilesPfad.Trim("\\") + "\\").CheckPath();
            if (DirectoryExists(t)) {
                _additionaFilesPfadtmp = t;
                return t;
            }
        }
        _additionaFilesPfadtmp = string.Empty;
        return string.Empty;
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

    public abstract void BlockReload(bool crashIsCurrentlyLoading);

    public void CancelBackGroundWorker() {
        if (_backgroundWorker.IsBusy && !_backgroundWorker.CancellationPending) { _backgroundWorker.CancelAsync(); }
    }

    /// <summary>
    /// Das hier ist die richtige Methode, um einen Wert dauerhaft zu setzen.
    /// Stößt undo uns die Festsetzung an. Es wird aber nich geprüft, ob Readonly gesetzt ist und spiegelt auch den Wert bei bedarf.
    /// </summary>
    /// <param name="comand"></param>
    /// <param name="columnKey"></param>
    /// <param name="rowKey"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    public virtual void ChangeData(DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo) {
        SetValueInternal(comand, changedTo, column, row, -1, -1);

        if (Mirror != null && !Mirror.ReadOnly) {
            ColumnItem? c = null;
            if (column!= null) { c= Mirror.Column.SearchByKey(column.Key); }
            RowItem? r = null;
            if (row!= null) { r= Mirror.Row.SearchByKey(row.Key); }

            Mirror?.ChangeData(comand, c, r, previousValue, changedTo);
        }

        if (IsLoading) { return; }

        if (ReadOnly) {
            if (!string.IsNullOrEmpty(TableName)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + TableName);
            }
            return;
        }

        StoreValueToHardDisk(comand, column, row, changedTo);
        AddUndo(TableName, comand, column, row, previousValue, changedTo, UserName);

        if (comand != DatabaseDataType.AutoExport) { SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen
    }

    /// <summary>
    /// Einstellungen der Quell-Datenbank auf diese hier übertragen
    /// </summary>
    /// <param name="sourceDatabase"></param>
    public void CloneFrom(DatabaseAbstract sourceDatabase, bool cellDataToo) {
        Column.CloneFrom(sourceDatabase);

        if (cellDataToo) { Row.CloneFrom(sourceDatabase); }

        AdditionaFilesPfad = sourceDatabase.AdditionaFilesPfad;
        CachePfad = sourceDatabase.CachePfad; // Nicht so wichtig ;-)
        Caption = sourceDatabase.Caption;
        CreateDate = sourceDatabase.CreateDate;
        Creator = sourceDatabase.Creator;
        //Filename - nope
        GlobalScale = sourceDatabase.GlobalScale;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        ReloadDelaySecond = sourceDatabase.ReloadDelaySecond;
        RulesScript = sourceDatabase.RulesScript;
        if (SortDefinition == null || SortDefinition.ToString() != sourceDatabase.SortDefinition.ToString()) {
            SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition.ToString());
        }
        StandardFormulaFile = sourceDatabase.StandardFormulaFile;
        UndoCount = sourceDatabase.UndoCount;
        ZeilenQuickInfo = sourceDatabase.ZeilenQuickInfo;

        DatenbankAdmin= sourceDatabase.DatenbankAdmin;
        PermissionGroupsNewRow = sourceDatabase.PermissionGroupsNewRow;

        var tcvc = new ListExt<ColumnViewCollection>();
        foreach (var t in sourceDatabase.ColumnArrangements) {
            tcvc.Add(new ColumnViewCollection(this, t.ToString()));
        }

        ColumnArrangements = tcvc;
    }

    /// <summary>
    /// AdditionaFiles/Datenbankpfad mit Forms und abschließenden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionaFilesPfadWhole())) { return AdditionaFilesPfadWhole() + "Forms\\"; }
        if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionaFiles/Datenbankpfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionaFilesPfadWhole())) { return AdditionaFilesPfadWhole() + "Layouts\\"; }
        if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason(ErrorReason mode) {
        var f = SpecialErrorReason(mode);

        if (!string.IsNullOrEmpty(f)) { return f; }
        if (mode == BlueBasics.Enums.ErrorReason.OnlyRead) { return string.Empty; }

        if (mode.HasFlag(BlueBasics.Enums.ErrorReason.Load)) {
            if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
        }
        if (mode.HasFlag(BlueBasics.Enums.ErrorReason.EditGeneral) || mode.HasFlag(BlueBasics.Enums.ErrorReason.Save)) {
            if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
            if (ReloadNeeded) { return "Die Datei muss neu eingelesen werden."; }
        }

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
    public string Export_CSV(FirstRow firstRow, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_CSV(firstRow, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null));

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_HTML(filename, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null), false);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, List<ColumnItem?>? columnList, List<RowData?>? sortedRows, bool execute) {
        if (columnList == null || columnList.Count == 0) {
            columnList = Column.Where(thisColumnItem => thisColumnItem != null).ToList();
        }

        if (sortedRows == null) { sortedRows = AllRows(); }

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        Html da = new(TableName.FileNameWithoutSuffix());
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

    public void InvalidateExports(string layoutId) {
        if (ReadOnly) { return; }

        var ex = Export.CloneWithClones();

        foreach (var thisExport in ex) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) {
                    if (thisExport.ExportFormularId == layoutId) {
                        thisExport.LastExportTimeUtc = new DateTime(1900, 1, 1);
                    }
                }
            }
        }

        Export = ex;
    }

    public bool IsAdministrator() {
        if (UserGroup.ToUpper() == "#ADMINISTRATOR") { return true; }
        if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains("#EVERYBODY", false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    public abstract void Load_Reload();

    public void OnConnectedControlsStopAllWorking(object? sender, MultiUserFileStopWorkingEventArgs e) {
        ConnectedControlsStopAllWorking?.Invoke(this, e);
    }

    public void OnScriptError(RowCancelEventArgs e) {
        if (IsDisposed) { return; }
        ScriptError?.Invoke(this, e);
    }

    public void OnShouldICancelSaveOperations(object sender, CancelEventArgs e) {
        if (IsDisposed) { return; }
        ShouldICancelSaveOperations?.Invoke(this, e);
    }

    public List<string> Permission_AllUsed(bool cellLevel) {
        List<string> e = new();
        foreach (var thisColumnItem in Column) {
            if (thisColumnItem != null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(_permissionGroupsNewRow);
        e.AddRange(_datenbankAdmin);
        foreach (var thisArrangement in _columnArrangements) {
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

    public bool PermissionCheck(List<string>? allowed, RowItem? row) {
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
        _layouts.Check();
    }

    public abstract bool Save(bool mustDo);

    public virtual void SetReadOnly() {
        ReadOnly = true;
    }

    public abstract string UndoText(ColumnItem? column, RowItem? row);

    public abstract void UnlockHard();

    public abstract void WaitEditable();

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
        //var view = false;
        //foreach (var thisView in OldFormulaViews) {
        //    if (thisView[column] != null) { view = true; }
        //}
        //if (view) { t += " - Formular-Ansichten<br>"; }
        var cola = false;
        var first = true;
        foreach (var thisView in _columnArrangements) {
            if (!first && thisView[column] != null) { cola = true; }
            first = false;
        }
        if (cola) { t += " - Benutzerdefinierte Spalten-Anordnungen<br>"; }
        if (RulesScript.ToUpper().Contains(column.Name.ToUpper())) { t += " - Regeln-Skript<br>"; }
        if (ZeilenQuickInfo.ToUpper().Contains(column.Name.ToUpper())) { t += " - Zeilen-Quick-Info<br>"; }
        if (_tags.JoinWithCr().ToUpper().Contains(column.Name.ToUpper())) { t += " - Datenbank-Tags<br>"; }
        var layout = false;
        foreach (var thisLayout in _layouts) {
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
            t += "\r\nLoading: " + IsLoading;
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nFile: " + ConnectionID;
        } catch { }
        Develop.DebugPrint(FehlerArt.Warnung, t);
    }

    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventargs e) {
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

    protected abstract void AddUndo(string tableName, DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName);

    protected virtual void Dispose(bool disposing) {
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
        //ColumnArrangements.Changed -= ColumnArrangements_ListOrItemChanged;
        //Layouts.Changed -= Layouts_ListOrItemChanged;
        //Layouts.ItemSeted -= Layouts_ItemSeted;
        //PermissionGroupsNewRow.Changed -= PermissionGroups_NewRow_ListOrItemChanged;
        //Tags.Changed -= DatabaseTags_ListOrItemChanged;
        //Export.Changed -= Export_ListOrItemChanged;
        //DatenbankAdmin.Changed -= DatabaseAdmin_ListOrItemChanged;

        Row.RowRemoving -= Row_RowRemoving;
        Row.RowAdded -= Row_RowAdded;

        Column.ItemRemoving -= Column_ItemRemoving;
        Column.ItemRemoved -= Column_ItemRemoved;
        Column.ItemAdded -= Column_ItemAdded;
        Column.Dispose();
        Cell.Dispose();
        Row.Dispose();

        //_columnArrangements.Dispose();
        //_cags.Dispose();
        //_export.Dispose();
        //_datenbankAdmin.Dispose();
        //_permissionGroupsNewRow.Dispose();
        _layouts.Dispose();
    }

    protected abstract DatabaseAbstract? GetOtherTable(string tablename, bool readOnly);

    protected virtual void Initialize() {
        Cell.Initialize();

        _columnArrangements.Clear();
        _layouts.Clear();
        _permissionGroupsNewRow.Clear();
        _tags.Clear();
        _export.Clear();
        _datenbankAdmin.Clear();
        _globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _undoCount = 300;
        _caption = string.Empty;
        //_verwaisteDaten = VerwaisteDaten.Ignorieren;
        LoadedVersion = DatabaseVersion;
        _rulesScript = string.Empty;
        _globalScale = 1f;
        _additionaFilesPfad = "AdditionalFiles";
        _zeilenQuickInfo = string.Empty;
        _sortDefinition = null;
    }

    protected void OnExporting(CancelEventArgs e) {
        if (IsDisposed) { return; }
        Exporting?.Invoke(this, e);
    }

    protected void OnLoaded(object sender, LoadedEventArgs e) {
        if (IsDisposed) { return; }
        Loaded?.Invoke(this, e);
    }

    protected void OnLoading(object sender, LoadingEventArgs e) {
        if (IsDisposed) { return; }
        CancelBackGroundWorker();
        Loading?.Invoke(this, e);
    }

    protected void OnNeedPassword(PasswordEventArgs e) {
        if (IsDisposed) { return; }
        NeedPassword?.Invoke(this, e);
    }

    protected virtual void OnSavedToDisk(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        SavedToDisk?.Invoke(this, e);
    }

    protected virtual void SetUserDidSomething() {
        _lastUserActionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Die richtige Routine, wenn Werte auf den richtigen Speicherplatz geschrieben werden sollen.
    /// Nur von Laderoutinen aufzurufen, oder von ChangeData, wenn der Wert bereits fest in der Datenbank verankert ist.
    /// Vorsicht beim überschreiben! Da in Columns und Cells abgesprungen wird, und diese nicht überschrieben werde können.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    protected virtual string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        if (type.IsColumnTag()) {
            if (column == null) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type.ToString());
                return "Wert nicht gesetzt!";
            }
            return column.SetValueInternal(type, value);
        }

        if (type.IsCellValue()) {
            //        case DatabaseDataType.ce_Value_withSizeData:
            //case DatabaseDataType.ce_UTF8Value_withSizeData:
            //case DatabaseDataType.ce_Value_withoutSizeData:
            if (type == DatabaseDataType.ce_UTF8Value_withSizeData) {
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var enc1252 = Encoding.GetEncoding(1252);
                value = Encoding.UTF8.GetString(enc1252.GetBytes(value));
            }
            return Cell.SetValueInternal(column, row, value, width, height);
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

            case DatabaseDataType.CreateDateUTC:
                _createDate = value;
                break;

            case DatabaseDataType.ReloadDelaySecond:
                _reloadDelaySecond = IntParse(value);
                break;

            case DatabaseDataType.DatabaseAdminGroups:
                _datenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
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

            case DatabaseDataType.AdditionaFilesPath:
                _additionaFilesPfad = value;
                break;

            case DatabaseDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case DatabaseDataType.RowQuickInfo:
                _zeilenQuickInfo = value;
                break;

            case DatabaseDataType.Tags:
                _tags.SplitAndCutByCr(value);
                break;

            //case enDatabaseDataType.BinaryDataInOne:
            //    break;

            case DatabaseDataType.Layouts:
                _layouts.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.AutoExport:
                _export.Clear();
                List<string> ae = new(value.SplitAndCutByCr());
                foreach (var t in ae) {
                    _export.Add(new ExportDefinition(this, t));
                }
                break;

            case DatabaseDataType.ColumnArrangement:
                _columnArrangements.Clear();
                List<string> ca = new(value.SplitAndCutByCr());
                foreach (var t in ca) {
                    _columnArrangements.Add(new ColumnViewCollection(this, t));
                }
                break;

            case DatabaseDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            case DatabaseDataType.RulesScript:
                _rulesScript = value;
                break;

            case DatabaseDataType.UndoCount:
                _undoCount = IntParse(value);
                break;

            case DatabaseDataType.UndoInOne:
                // Muss eine übergeordnete Routine bei Befarf abfangen
                break;

            case DatabaseDataType.dummyComand_AddRow:
                var addRowKey = LongParse(value);
                if (Row.SearchByKey(addRowKey) == null) { Row.Add(new RowItem(this, addRowKey)); }
                break;

            //case DatabaseDataType.AddColumnKeyInfo: // TODO: Ummünzen auf AddColumnNameInfo
            //    var addColumnKey = LongParse(value);
            //    if (Column.SearchByKey(addColumnKey) == null) { Column.AddFromParser(new ColumnItem(this, addColumnKey)); }
            //    break;

            //case DatabaseDataType.AddColumnNameInfo: // TODO: Ummünzen auf AddColumnNameInfo
            //    //var addColumnKey = LongParse(value);
            //    //if (Column.SearchByKey(addColumnKey) == null) { Column.AddFromParser(new ColumnItem(this, addColumnKey)); }
            //    break;

            case DatabaseDataType.dummyComand_RemoveRow:
                var removeRowKey = LongParse(value);
                if (Row.SearchByKey(removeRowKey) != null) { Row.Remove(removeRowKey); }
                break;

            case DatabaseDataType.Comand_RemovingColumn:
                //var removeColumnKey = LongParse(value);
                //if (Column.SearchByKey(removeColumnKey) is ColumnItem col) { Column.Remove(col); }
                break;

            case DatabaseDataType.EOF:
                return string.Empty;

            case DatabaseDataType.Comand_ColumnAdded:
                break;

            case (DatabaseDataType)22:
            case (DatabaseDataType)62:
            case (DatabaseDataType)53:
            case (DatabaseDataType)33:
                break;

            default:
                // Variable type
                if (LoadedVersion == DatabaseVersion) {
                    SetReadOnly();
                    if (!ReadOnly) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Datei: " + ConnectionID);
                    }
                }

                break;
        }
        return string.Empty;
    }

    protected abstract string SpecialErrorReason(ErrorReason mode);

    /// <summary>
    /// Für Echtzeitbasierte Systeme ist hier der richtige Zeitpunkt, den Wert fest auf Festplatte zu schreiben.
    /// </summary>
    /// <param name="comand"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="value"></param>
    protected abstract void StoreValueToHardDisk(DatabaseDataType type, ColumnItem? column, RowItem? row, string value);

    private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (ReadOnly) { return; }

        var tmpe = _export.CloneWithClones();

        foreach (var thisExport in tmpe) {
            if (_backgroundWorker.CancellationPending) { break; }

            if (IsLoading) { break; }

            if (thisExport.IsOk()) {
                //var e2 = new MultiUserFileHasPendingChangesEventArgs();
                //HasPendingChanges(null, e2);

                //if (!e2.HasPendingChanges) {
                CancelEventArgs ec = new(false);
                OnExporting(ec);
                if (ec.Cancel) { break; }
                //}

                thisExport.DeleteOutdatedBackUps(_backgroundWorker);
                if (_backgroundWorker.CancellationPending) { break; }
                thisExport.DoBackUp(_backgroundWorker);
                if (_backgroundWorker.CancellationPending) { break; }
            }
        }

        Export = tmpe;
    }

    private bool BlockSaveOperations() {
        var e = new CancelEventArgs();

        OnShouldICancelSaveOperations(this, e);
        return e.Cancel;
    }

    private void Checker_Tick(object state) {
        if (IsLoading) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < 10 || BlockSaveOperations()) { CancelBackGroundWorker(); return; } // Benutzer arbeiten lassen

        var mustBackup = IsThereBackgroundWorkToDo();

        if (!mustBackup) {
            _checkerTickCount = 0;
            return;
        }

        // Zeiten berechnen
        ReloadDelaySecond = Math.Max(ReloadDelaySecond, 10);
        var countBackUp = Math.Min((ReloadDelaySecond / 10f) + 1, 10); // Soviele Sekunden können vergehen, bevor Backups gemacht werden. Der Wert muss kleiner sein, als Count_Save

        //if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < countUserWork || BlockSaveOperations()) { CancelBackGroundWorker(); return; } // Benutzer arbeiten lassen

        ////if (_checkerTickCount > countSave && mustSave) { CancelBackGroundWorker(); }

        //var mustReload = ReloadNeeded;

        //if (_checkerTickCount > ReloadDelaySecond && mustReload) { CancelBackGroundWorker(); }
        //if (_backgroundWorker.IsBusy) { return; }

        //if (string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditNormaly))) { return; }

        //if (mustReload && mustSave) {
        //    if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Load))) { return; }
        //    // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
        //    Load_Reload();
        //    return;
        //}

        //if (mustSave && _checkerTickCount > countSave) {
        //    if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Save))) { return; }
        //    if (!_pureBinSaver.IsBusy) { _pureBinSaver.RunWorkerAsync(); } // Eigentlich sollte diese Abfrage überflüssig sein. Ist sie aber nicht
        //    _checkerTickCount = 0;
        //    return;
        //}

        if (mustBackup && _checkerTickCount >= countBackUp && string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.EditAcut))) {
            var nowsek = (DateTime.UtcNow.Ticks - _startTick) / 30000000;
            if (nowsek % 20 != 0) { return; } // Lasten startabhängig verteilen. Bei Pending changes ist es eh immer true;

            StartBackgroundWorker();
        }

        //// Überhaupt nix besonderes. Ab und zu mal Reloaden
        //if (mustReload && _checkerTickCount > ReloadDelaySecond) {
        //    RepairOldBlockFiles();
        //    if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Load))) { return; }
        //    Load_Reload();
        //    _checkerTickCount = 0;
        //}
    }

    private void CheckViewsAndArrangements() {
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Temporär erzeugt werden!

        if (IsLoading) { return; }

        var x = _columnArrangements.CloneWithClones();

        for (var z = 0; z < Math.Max(2, x.Count); z++) {
            if (x.Count < z + 1) { x.Add(new ColumnViewCollection(this, string.Empty)); }
            x[z].Repair(z, true);
        }

        ColumnArrangements = x;
    }

    private void Column_ItemAdded(object sender, ListEventArgs e) => CheckViewsAndArrangements();

    private void Column_ItemRemoved(object sender, System.EventArgs e) {
        if (IsLoading) { Develop.DebugPrint(FehlerArt.Warnung, "Parsing Falsch!"); }
        CheckViewsAndArrangements();

        var lay = (LayoutCollection)Layouts.Clone();
        lay.Check();
        Layouts = lay;
    }

    private void Column_ItemRemoving(object sender, ListEventArgs e) {
        var c = (ColumnItem)e.Item;
        ChangeData(DatabaseDataType.Comand_RemovingColumn, c, null, string.Empty, c.Key.ToString());
    }

    private bool IsThereBackgroundWorkToDo() {
        //var e2 = new MultiUserFileHasPendingChangesEventArgs();
        //HasPendingChanges(null, e2);

        //if (e2.HasPendingChanges) { e.BackGroundWork = true; return; }
        CancelEventArgs ec = new(false);
        OnExporting(ec);
        if (ec.Cancel) { return false; }

        foreach (var thisExport in _export) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) { return true; }
                if (DateTime.UtcNow.Subtract(thisExport.LastExportTimeUtc).TotalDays > thisExport.BackupInterval * 50) { return true; }
            }
        }

        return false;
    }

    private void OnDisposing() {
        Disposing?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
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
                    if (DateTime.Now.Subtract(f.CreationTime).TotalDays < 20) {
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
                    try {
                        //File.SetLastWriteTime(fullhashname, DateTime.Now);
                        File.SetCreationTime(fullhashname, DateTime.Now);
                        //var x = new FileInfo(fullname);
                        //x.CreationTime = DateTime.Now;
                    } catch { }
                }
                return;
            }

            var l = new List<string>();
            l.Save(fullhashname, Encoding.UTF8, false);
        } catch { }
    }

    private void Row_RowAdded(object sender, RowEventArgs e) {
        if (IsLoading) {
            ChangeData(DatabaseDataType.dummyComand_AddRow, null, e.Row, String.Empty, e.Row.Key.ToString());
        }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) => ChangeData(DatabaseDataType.dummyComand_RemoveRow, null, e.Row, string.Empty, e.Row.Key.ToString());

    private void StartBackgroundWorker() {
        try {
            if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.EditNormaly))) { return; }
            if (!_backgroundWorker.IsBusy) { _backgroundWorker.RunWorkerAsync(); }
        } catch {
            StartBackgroundWorker();
        }
    }

    #endregion
}