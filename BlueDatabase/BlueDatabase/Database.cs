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
using static BlueBasics.FileOperations;
using static BlueBasics.Converter;
using BlueScript.Variables;
using BlueBasics.Interfaces;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Database : IDisposable, IDisposableExtended {

    #region Fields

    public const string DatabaseVersion = "4.00";
    public static readonly ListExt<Database> AllFiles = new();
    public readonly SqlBack? _sql;
    public readonly CellCollection Cell;
    public readonly ColumnCollection Column;
    public readonly ListExt<ColumnViewCollection> ColumnArrangements = new();
    public readonly ListExt<string> DatenbankAdmin = new();

    /// <summary>
    /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
    /// Der Head-Editor kann und muss (manuelles L�schen) auf die Exporte Zugreifen und k�mmert sich auch um die Pendings
    /// </summary>
    public readonly ListExt<ExportDefinition?> Export = new();

    public readonly LayoutCollection Layouts = new();

    //public readonly ListExt<ColumnViewCollection> OldFormulaViews = new();
    public readonly ListExt<string> PermissionGroupsNewRow = new();

    public readonly RowCollection Row;

    public readonly ListExt<string> Tags = new();

    public readonly string UserName = Generic.UserName().ToUpper();
    public string UserGroup;

    public ListExt<WorkItem> Works;

    //private readonly List<string> _filesAfterLoadingLCase;

    private readonly BlueBasics.MultiUserFile.MultiUserFile? _muf;
    private string _additionaFilesPfad;

    private string _additionaFilesPfadtmp = string.Empty;

    private string _caption = string.Empty;

    private string _createDate = string.Empty;

    private string _creator = string.Empty;

    private double _globalScale;

    //private string _filterImagePfad;
    private string _globalShowPass = string.Empty;

    private string _rulesScript = string.Empty;

    ///// <summary>
    ///// Variable nur tempor�r f�r den BinReloader, um m�gliche Datenverluste zu entdecken.
    ///// </summary>
    //private string _LastWorkItem = string.Empty;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    //private enJoinTyp _JoinTyp;
    private int _undoCount;

    //private VerwaisteDaten _verwaisteDaten;

    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    //private string _WorkItemsBefore = string.Empty;
    public Database(Stream stream) : this(stream, string.Empty, true, false) { }

    public Database(bool readOnly) : this(null, string.Empty, readOnly, true) { }

    public Database(string filename, bool readOnly, bool create) : this(null, filename, readOnly, create) { }

    private Database(Stream? stream, string filename, bool readOnly, bool create) {
        AllFiles.Add(this);

        _muf = new BlueBasics.MultiUserFile.MultiUserFile(readOnly, true);

        _muf.ConnectedControlsStopAllWorking += OnConnectedControlsStopAllWorking;
        _muf.Loaded += OnLoaded;
        _muf.Loading += OnLoading;
        _muf.SavedToDisk += OnSavedToDisk;
        _muf.ShouldICancelSaveOperations += OnShouldICancelSaveOperations;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += HasPendingChanges;
        _muf.RepairAfterParse += RepairAfterParse;
        _muf.DoWorkAfterSaving += DoWorkAfterSaving;
        _muf.IsThereBackgroundWorkToDo += IsThereBackgroundWorkToDo;
        _muf.ParseExternal += ParseExternal;
        _muf.ToListOfByte += ToListOfByte;
        _muf.DoBackGroundWork += DoBackGroundWork;

        Develop.StartService();

        //if (FileExists(filename)) { _sql = new SqlBack("D:\\" + filename.FileNameWithoutSuffix() + ".mdf", true); }

        //CultureInfo culture = new("de-DE");
        //CultureInfo.DefaultThreadCurrentCulture = culture;
        //CultureInfo.DefaultThreadCurrentUICulture = culture;
        Cell = new CellCollection(this);

        Row = new RowCollection(this);
        Row.RowRemoving += Row_RowRemoving;
        Row.RowAdded += Row_RowAdded;

        Column = new ColumnCollection(this);
        Column.ItemRemoving += Column_ItemRemoving;
        Column.ItemRemoved += Column_ItemRemoved;
        Column.ItemAdded += Column_ItemAdded;

        Works = new ListExt<WorkItem>();
        //_filesAfterLoadingLCase = new List<string>();
        ColumnArrangements.Changed += ColumnArrangements_ListOrItemChanged;
        Layouts.Changed += Layouts_ListOrItemChanged;
        Layouts.ItemSeted += Layouts_ItemSeted;
        PermissionGroupsNewRow.Changed += PermissionGroups_NewRow_ListOrItemChanged;
        Tags.Changed += DatabaseTags_ListOrItemChanged;
        Export.Changed += Export_ListOrItemChanged;
        DatenbankAdmin.Changed += DatabaseAdmin_ListOrItemChanged;

        Initialize();

        UserGroup = "#Administrator";
        if (!string.IsNullOrEmpty(filename)) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
            _muf.Load(filename, create);
        } else if (stream != null) {
            _muf.LoadFromStream(stream);
        } else {
            RepairAfterParse(null, null);
        }
        QuickImage.NeedImage += QuickImage_NeedImage;
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
    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zus�tzlichen Dateien.")]
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

    public string Filename => _muf.Filename;

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
    public bool IsLoading => _muf.IsLoading;

    public bool IsParsing => _muf.IsParsing;

    public string LoadedVersion { get; private set; }

    public DateTime PowerEdit { get; set; }

    public bool ReadOnly => _muf.ReadOnly;

    [Browsable(false)]
    public int ReloadDelaySecond {
        get => _muf.ReloadDelaySecond;
        set {
            if (_muf.ReloadDelaySecond == value) { return; }
            AddPending(DatabaseDataType.ReloadDelaySecond, -1, -1, _muf.ReloadDelaySecond.ToString(), value.ToString(), true);
        }
    }

    public bool ReloadNeeded => _muf.ReloadNeeded;

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

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Browsable(false)]
    [Description("Das standardm��ige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            AddPending(DatabaseDataType.StandardFormulaFile, -1, -1, _standardFormulaFile, value, true);
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

    //public VerwaisteDaten VerwaisteDaten {
    //    get => _verwaisteDaten;
    //    set {
    //        if (_verwaisteDaten == value) { return; }
    //        AddPending(DatabaseDataType.VerwaisteDaten, -1, -1, ((int)_verwaisteDaten).ToString(), ((int)value).ToString(), true);
    //    }
    //}

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

    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="checkOnlyFilenameToo"></param>
    /// <param name="readOnly"></param>
    /// <returns></returns>
    public static Database? GetByFilename(string filename, bool checkOnlyFilenameToo, bool readOnly) {
        if (string.IsNullOrEmpty(filename)) { return null; }

        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, filename, StringComparison.OrdinalIgnoreCase)) {
                thisFile.BlockReload(false);
                return thisFile;
            }
        }

        if (checkOnlyFilenameToo) {
            foreach (var thisFile in AllFiles) {
                if (thisFile != null && thisFile.Filename.ToLower().FileNameWithSuffix() == filename.ToLower().FileNameWithSuffix()) {
                    thisFile.BlockReload(false);
                    return thisFile;
                }
            }
        }

        if (!FileExists(filename)) { return null; }

        return new Database(filename, readOnly, false);
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
    /// Der komplette Pfad mit abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string AdditionaFilesPfadWhole() {
        // @ ist ein erkennungszeichen, dass der Pfad schon gepr�ft wurde, aber nicht vorhanden ist
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

    //public List<string> AllConnectedFilesLCase() {
    //    List<string> columnAll = new();
    //    var lockMe = new object();

    //    foreach (var thisColumnItem in Column) {
    //        if (thisColumnItem != null && thisColumnItem.Format == DataFormat.Link_To_Filesystem) {
    //            var tmp = thisColumnItem.Contents();
    //            Parallel.ForEach(tmp, thisTmp => {
    //                var x = thisColumnItem.BestFile(thisTmp, false).ToLower();
    //                lock (lockMe) {
    //                    columnAll.Add(x);
    //                }
    //            });
    //        }
    //    }
    //    //foreach (var ThisColumnItem in Column) {
    //    //    if (ThisColumnItem != null) {
    //    //        if (ThisColumnItem.Format == DataFormat.Link_To_Filesystem) {
    //    //            var tmp = ThisColumnItem.Contents();
    //    //            foreach (var thisTmp in tmp) {
    //    //                Column_All.AddIfNotExists(ThisColumnItem.BestFile(thisTmp, false).ToLower());
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    return columnAll.SortedDistinctList();
    //}

    public List<RowData?> AllRows() {
        var sortedRows = new List<RowData?>();
        foreach (var thisRowItem in Row) {
            if (thisRowItem != null) {
                sortedRows.Add(new RowData(thisRowItem));
            }
        }
        return sortedRows;
    }

    public bool BlockSaveOperations() => RowItem.DoingScript || _muf.BlockSaveOperations();

    public void CancelBackGroundWorker() {
        _muf.CancelBackGroundWorker();
    }

    public void CloneDataFrom(Database sourceDatabase) {

        #region Einstellungen der Urspr�nglichen Datenbank auf die Kopie �bertragen

        RulesScript = sourceDatabase.RulesScript;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        DatenbankAdmin.CloneFrom(sourceDatabase.DatenbankAdmin);
        PermissionGroupsNewRow.CloneFrom(sourceDatabase.PermissionGroupsNewRow);
        ReloadDelaySecond = sourceDatabase.ReloadDelaySecond;
        //VerwaisteDaten = sourceDatabase.VerwaisteDaten;
        ZeilenQuickInfo = sourceDatabase.ZeilenQuickInfo;
        StandardFormulaFile = sourceDatabase.StandardFormulaFile;

        if (SortDefinition.ToString() != sourceDatabase.SortDefinition.ToString()) {
            SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition.ToString());
        }

        foreach (var ThisColumn in Column) {
            var l = sourceDatabase.Column.Exists(ThisColumn.Name);
            if (l != null) {
                ThisColumn.CloneFrom(l);
            }
        }

        #endregion
    }

    /// <summary>
    /// Datenbankpfad mit Forms und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() => string.IsNullOrEmpty(_muf.Filename) ? string.Empty : Filename.FilePath() + "Forms\\";

    /// <summary>
    /// Datenbankpfad mit Layouts und abschlie�enden \
    /// </summary>
    public string DefaultLayoutPath() => string.IsNullOrEmpty(_muf.Filename) ? string.Empty : Filename.FilePath() + "Layouts\\";

    public void DiscardPendingChanges(object sender, System.EventArgs e) => ChangeWorkItems(ItemState.Pending, ItemState.Undo);

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason(ErrorReason mode) {
        var f = _muf.ErrorReason(mode);

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
    /// <param name="column">Die Spalte, die zur�ckgegeben wird.</param>
    /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle Zeilen zur�ck.</param>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, ColumnItem? column, List<RowData>? sortedRows) =>
        //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        Export_CSV(firstRow, new List<ColumnItem> { column }, sortedRows);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="columnList">Die Spalten, die zur�ckgegeben werden. NULL gibt alle Spalten zur�ck.</param>
    /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle ZEilen zur�ck.</param>
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
    /// <param name="arrangement">Die Spalten, die zur�ckgegeben werden. NULL gibt alle Spalten zur�ck.</param>
    /// <param name="sortedRows">Die Zeilen, die zur�ckgegeben werden. NULL gibt alle ZEilen zur�ck.</param>
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
    public void Export_HTML(string filename, List<ColumnItem?>? columnList, List<RowData?>? sortedRows, bool execute) {
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
                        if (thisColumn.Format is DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
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
    /// Testet die Standard-Verzeichnisse und gibt das Formular zur�ck, falls es existiert
    /// </summary>
    /// <returns></returns>
    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionaFilesPfadWhole() + _standardFormulaFile)) { return AdditionaFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    public void HasPendingChanges(object? sender, MultiUserFileHasPendingChangesEventArgs e) {
        try {
            if (_muf.ReadOnly) { return; }

            e.HasPendingChanges = Works.Any(thisWork => thisWork.State == ItemState.Pending);
        } catch {
            HasPendingChanges(sender, e);
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
                vars.Add(new VariableBool("IsCaption", spalteZuordnen && z == 0, true, false, "Wenn TRUE, ist das die erste Zeile, die �berschriften enth�lt."));
                vars.Add(new VariableString("Seperator", splitChar, true, false, "Das Trennzeichen"));
                var x = new BlueScript.Script(vars, string.Empty, false);
                x.ScriptText = script;
                if (!x.Parse()) {
                    OnDropMessage(FehlerArt.Warnung, "Skript-Fehler, Import kann nicht ausgef�hrt werden.");
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
            OnDropMessage(FehlerArt.Warnung, "Import kann nicht ausgef�hrt werden.");
            return "Import kann nicht ausgef�hrt werden.";
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

    /// <summary>
    /// F�gt Comandos manuell hinzu. Vorsicht: Kann Datenbank besch�digen
    /// </summary>
    public void InjectCommand(DatabaseDataType comand, string changedTo) => AddPending(comand, -1, -1, string.Empty, changedTo, true);

    public bool IsAdministrator() {
        if (UserGroup.ToUpper() == "#ADMINISTRATOR") { return true; }
        if (DatenbankAdmin == null || DatenbankAdmin.Count == 0) { return false; }
        if (DatenbankAdmin.Contains("#EVERYBODY", false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && DatenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && DatenbankAdmin.Contains(UserGroup, false);
    }

    public void Load_Reload() {
        _muf.Load_Reload();
    }

    public void OnConnectedControlsStopAllWorking(object? sender, MultiUserFileStopWorkingEventArgs e) {
        ConnectedControlsStopAllWorking?.Invoke(this, e);
    }

    public void OnScriptError(RowCancelEventArgs e) {
        if (IsDisposed) { return; }
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

    public void ParseExternal(object sender, MultiUserParseEventArgs e2) {
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

        var b = e2.Data;

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
                    // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzuf�gen
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
                        _muf.RemoveFilename();
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
        // Spalten, die nach dem Reload nicht mehr ben�tigt werden, l�schen
        //ColumnsOld.DisposeAndRemoveAll();
        Row.RemoveNullOrEmpty();
        //Row.RemoveNullOrDisposed();
        Cell.RemoveOrphans();
        //LoadPicsIntoImageChache();
        //_filesAfterLoadingLCase.Clear();
        //_filesAfterLoadingLCase.AddRange(AllConnectedFilesLCase());
        Works.AddRange(oldPendings);
        oldPendings.Clear();
        ExecutePending();
        Column.ThrowEvents = true;
        if (IntParse(LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))) { SetReadOnly(); }
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

    public bool PermissionCheck(ListExt<string>? allowed, RowItem? row) {
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

    public void RepairAfterParse(object? sender, System.EventArgs? e) {
        Column.Repair();
        CheckViewsAndArrangements();
        Layouts.Check();
    }

    public bool Save(bool mustSave) => _muf.Save(mustSave);

    public void SaveAsAndChangeTo(string fileName) {
        _muf.SaveAsAndChangeTo(fileName);
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

    public void UnlockHard() {
        _muf.UnlockHard();
    }

    public void WaitEditable() {
        _muf.WaitEditable();
    }

    public void WaitParsed() {
        _muf.WaitParsed();
    }

    internal void AddPending(DatabaseDataType comand, ColumnItem column, string previousValue, string changedTo, bool executeNow) => AddPending(comand, column.Key, -1, previousValue, changedTo, executeNow);

    internal void AddPending(DatabaseDataType comand, long columnKey, string listExt, bool executeNow) => AddPending(comand, columnKey, -1, "", listExt, executeNow);

    internal void AddPending(DatabaseDataType comand, long columnKey, long rowKey, string previousValue, string changedTo, bool executeNow) {
        if (executeNow) {
            ParseThis(comand, changedTo, Column.SearchByKey(columnKey), Row.SearchByKey(rowKey), -1, -1);
        }
        if (_muf.IsParsing) { return; }
        if (_muf.ReadOnly) {
            if (!string.IsNullOrEmpty(Filename)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + Filename);
            }
            return;
        }
        // Keine Doppelten Rausfiltern, ansonstn stimmen die Undo nicht mehr

        if (comand != DatabaseDataType.AutoExport) { _muf.SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

        if (rowKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "RowKey darf hier nicht <-100 sein!"); }
        if (columnKey < -100) { Develop.DebugPrint(FehlerArt.Fehler, "ColKey darf hier nicht <-100 sein!"); }
        Works.Add(new WorkItem(comand, columnKey, rowKey, previousValue, changedTo, UserName));

        _sql?.AddUndo(Filename.FileNameWithoutSuffix(), comand, columnKey, rowKey, previousValue, changedTo, UserName);
    }

    internal void BlockReload(bool crashIsCurrentlyLoading) {
        _muf.BlockReload(crashIsCurrentlyLoading);
    }

    internal void Column_NameChanged(string oldName, ColumnItem newName) {
        if (string.IsNullOrEmpty(oldName)) { return; }
        // Cells ----------------------------------------------
        //   Cell.ChangeCaptionName(OldName, cColumnItem.Name, cColumnItem)
        //  Undo -----------------------------------------
        // Nicht n�tig, da die Spalten als Verwei� gespeichert sind
        // Layouts -----------------------------------------
        // Werden �ber das Skript gesteuert
        // Sortierung -----------------------------------------
        // Nicht n�tig, da die Spalten als Verwei� gespeichert sind
        // _ColumnArrangements-----------------------------------------
        // Nicht n�tig, da die Spalten als Verwei� gespeichert sind
        // _Views-----------------------------------------
        // Nicht n�tig, da die Spalten als Verwei� gespeichert sind
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
            t = t + " - Bef�llt mit " + l.Count + " verschiedenen Werten";
        }
        return t;
    }

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nParsing: " + _muf.IsParsing;
            t += "\r\nLoading: " + _muf.IsLoading;
            t += "\r\nSaving: " + _muf.IsSaving;
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nFile: " + Filename;
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
        SaveToByteList(list, 0, 7); //Zeile-Un�tig
        list.AddRange(b);
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
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Tempor�r erzeugt werden!

        if (_muf.IsParsing) { return; }

        for (var z = 0; z < Math.Max(2, ColumnArrangements.Count); z++) {
            if (ColumnArrangements.Count < z + 1) { ColumnArrangements.Add(new ColumnViewCollection(this, string.Empty)); }
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
        if (_muf.IsParsing) { Develop.DebugPrint(FehlerArt.Warnung, "Parsing Falsch!"); }
        CheckViewsAndArrangements();

        Layouts.Check();
    }

    //private void ChangeRowKeyInPending(int OldKey, int NewKey) {
    //    foreach (var ThisPending in Works) {
    //        if (ThisPending.State == enItemState.Pending) {
    //            if (ThisPending.RowKey == OldKey) {
    //                if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
    //                ThisPending.RowKey = NewKey; // Generell den Schl�ssel �ndern
    //                if (_LastWorkItem == "X") {
    //                    _LastWorkItem = ThisPending.ToString();
    //                    //Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem ge�ndert: " + _LastWorkItem);
    //                }
    //                switch (ThisPending.Comand) {
    //                    case enDatabaseDataType.dummyComand_AddRow:
    //                    case enDatabaseDataType.dummyComand_RemoveRow:
    //                        ThisPending.ChangedTo = NewKey.ToString();
    //                        break;
    private void Column_ItemRemoving(object sender, ListEventArgs e) {
        var key = ((ColumnItem)e.Item).Key;
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
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt werden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.ColumnArrangement, -1, ColumnArrangements.ToString(), false);
    }

    //private void ChangeColumnKeyInPending(int oldKey, int newKey) {
    //    foreach (var ThisPending in Works) {
    //        if (ThisPending.State == enItemState.Pending) {
    //            if (ThisPending.ColKey == oldKey) {
    //                if (ThisPending.ToString() == _LastWorkItem) { _LastWorkItem = "X"; }
    //                ThisPending.ColKey = newKey; // Generell den Schl�ssel �ndern
    //                if (_LastWorkItem == "X") {
    //                    _LastWorkItem = ThisPending.ToString();
    //                    //Develop.DebugPrint(enFehlerArt.Info, "LastWorkitem ge�ndert: " + _LastWorkItem);
    //                }
    //                switch (ThisPending.Comand) {
    //                    case enDatabaseDataType.AddColumn:
    //                    case enDatabaseDataType.dummyComand_RemoveColumn:
    //                        ThisPending.ChangedTo = newKey.ToString();
    //                        break;
    private void DatabaseAdmin_ListOrItemChanged(object sender, System.EventArgs e) {
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.DatabaseAdminGroups, -1, DatenbankAdmin.JoinWithCr(), false);
    }

    //            protected override void OnItemAdded(ColumnItem item) {//    base.OnItemAdded(item);//    Database.CheckViewsAndArrangements();//}//protected override void OnItemRemoved() {//    base.OnItemRemoved();//    Database.CheckViewsAndArrangements();//    Database.Layouts.Check();//}
    private void DatabaseTags_ListOrItemChanged(object sender, System.EventArgs e) {
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.Tags, -1, Tags.JoinWithCr(), false);
    }

    private void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        IsDisposed = true;

        OnDisposing();
        AllFiles.Remove(this);

        _muf.Dispose();
        //base.Dispose(disposing); // speichert und l�scht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }
        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten �berschreiben.
        // TODO: gro�e Felder auf Null setzen.
        ColumnArrangements.Changed -= ColumnArrangements_ListOrItemChanged;
        Layouts.Changed -= Layouts_ListOrItemChanged;
        Layouts.ItemSeted -= Layouts_ItemSeted;
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
    //                if (!Filename.Contains("AutoVue") && !Filename.Contains("Plandaten") && !Filename.Contains("Ketten.") && !Filename.Contains("Kettenr�der.") && !Filename.Contains("TVW") && !Filename.Contains("Work") && !Filename.Contains("Beh�lter")) {
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
                HasPendingChanges(null, e2);

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

    private void DoWorkAfterSaving(object sender, System.EventArgs e) {
        ChangeWorkItems(ItemState.Pending, ItemState.Undo);
        //var filesNewLCase = AllConnectedFilesLCase();
        //List<string> writerFilesToDeleteLCase = new();
        //if (_verwaisteDaten == VerwaisteDaten.L�schen) {
        //    writerFilesToDeleteLCase = _filesAfterLoadingLCase.Except(filesNewLCase).ToList();
        //}
        //_filesAfterLoadingLCase.Clear();
        //_filesAfterLoadingLCase.AddRange(filesNewLCase);
        //if (writerFilesToDeleteLCase.Count > 0) { DeleteFile(writerFilesToDeleteLCase); }
    }

    private void ExecutePending() {
        if (!_muf.IsParsing) { Develop.DebugPrint(FehlerArt.Fehler, "Nur w�hrend des Parsens m�glich"); }

        var e2 = new MultiUserFileHasPendingChangesEventArgs();
        HasPendingChanges(null, e2);

        if (!e2.HasPendingChanges) { return; }
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
        //// Dann den neuen Zeilen / Spalten Tats�chlich eine neue ID zuweisen
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
        // Und nun alles ausf�hren!
        foreach (var thisPending in Works.Where(thisPending => thisPending.State == ItemState.Pending)) {
            if (thisPending.Comand == DatabaseDataType.ColumnName) {
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
                        Develop.DebugPrint("Pending verworfen, Zeile gel�scht.<br>" + Filename + "<br>" + thisPendingItem.ToString());
                        return;
                    }
                }
            }
            ColumnItem? col = null;
            if (thisPendingItem.ColKey > -1) {
                col = Column.SearchByKey(thisPendingItem.ColKey);
                if (col == null) {
                    if (thisPendingItem.Comand != DatabaseDataType.AddColumn && thisPendingItem.User != UserName) {
                        Develop.DebugPrint("Pending verworfen, Spalte gel�scht.<br>" + Filename + "<br>" + thisPendingItem.ToString());
                        return;
                    }
                }
            }
            ParseThis(thisPendingItem.Comand, thisPendingItem.ChangedTo, col, row, 0, 0);
        }
    }

    private void Export_ListOrItemChanged(object sender, System.EventArgs e) {
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.AutoExport, -1, Export.ToString(true), false);
    }

    private void Initialize() {
        Cell.Initialize();
        Works.Clear();
        ColumnArrangements.Clear();
        Layouts.Clear();
        PermissionGroupsNewRow.Clear();
        Tags.Clear();
        Export.Clear();
        DatenbankAdmin.Clear();
        _globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _muf.ReloadDelaySecond = 600;
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

    private void InvalidateExports(string layoutId) {
        if (_muf.ReadOnly) { return; }

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
        var e2 = new MultiUserFileHasPendingChangesEventArgs();
        HasPendingChanges(null, e2);

        if (e2.HasPendingChanges) { e.BackGroundWork = true; return; }
        CancelEventArgs ec = new(false);
        OnExporting(ec);
        if (ec.Cancel) { return; }

        foreach (var thisExport in Export) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) { e.BackGroundWork = true; return; }
                if (DateTime.UtcNow.Subtract(thisExport.LastExportTimeUtc).TotalDays > thisExport.BackupInterval) { e.BackGroundWork = true; return; }
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
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.Layouts, -1, Layouts.JoinWithCr(), false);
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

    private void OnSavedToDisk(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        SavedToDisk?.Invoke(this, e);
    }

    private void OnShouldICancelSaveOperations(object sender, CancelEventArgs e) {
        if (IsDisposed) { return; }
        ShouldICancelSaveOperations?.Invoke(this, e);
    }

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private string ParseThis(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        if ((int)type is >= 100 and <= 199) {
            _sql?.CheckIn(Filename.FileNameWithoutSuffix(), type, value, column, null, -1, -1);
            return column.Load(type, value);
        }

        _sql?.CheckIn(Filename.FileNameWithoutSuffix(), type, value, column, row, width, height);

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
                _muf.ReloadDelaySecond = IntParse(value);
                break;

            case DatabaseDataType.DatabaseAdminGroups:
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

            case (DatabaseDataType)65://DatabaseDataType.FilterImagePfad:
                //_filterImagePfad = value;
                break;

            case DatabaseDataType.AdditionaFilesPfad:
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
                Works.Clear();
                var uio = value.SplitAndCutByCr();
                for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                    WorkItem tmpWork = new(uio[z]) {
                        State = ItemState.Undo // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos �ndern.
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
                    if (!_muf.ReadOnly) {
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
        if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt wetrden. Kann zu Endlosschleifen f�hren.
        AddPending(DatabaseDataType.PermissionGroupsNewRow, -1, PermissionGroupsNewRow.JoinWithCr(), false);
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        if (e.Bmp != null) { return; }
        try {
            if (string.IsNullOrWhiteSpace(AdditionaFilesPfadWhole())) { return; }
            var n = e.Name.RemoveChars(Constants.Char_DateiSonderZeichen);

            if (FileExists(AdditionaFilesPfadWhole() + n + ".png")) {
                e.Bmp = new BitmapExt(AdditionaFilesPfadWhole() + n + ".png");
            }
        } catch { }
    }

    private void Row_RowAdded(object sender, RowEventArgs e) {
        if (!_muf.IsParsing) {
            AddPending(DatabaseDataType.dummyComand_AddRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);
        }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) => AddPending(DatabaseDataType.dummyComand_RemoveRow, -1, e.Row.Key, "", e.Row.Key.ToString(), false);

    private void SetReadOnly() {
        _muf.SetReadOnly();
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {
        try {
            var cryptPos = -1;
            List<byte> l = new();
            // Wichtig, Reihenfolge und L�nge NIE ver�ndern!
            SaveToByteList(l, DatabaseDataType.Formatkennung, "BlueDatabase");
            SaveToByteList(l, DatabaseDataType.Version, DatabaseVersion);
            SaveToByteList(l, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
            // Passw�rter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden k�nnen
            if (string.IsNullOrEmpty(_globalShowPass)) {
                SaveToByteList(l, DatabaseDataType.CryptionState, false.ToPlusMinus());
            } else {
                SaveToByteList(l, DatabaseDataType.CryptionState, true.ToPlusMinus());
                cryptPos = l.Count;
                SaveToByteList(l, DatabaseDataType.CryptionTest, "OK");
            }
            SaveToByteList(l, DatabaseDataType.GlobalShowPass, _globalShowPass);
            //SaveToByteList(l, DatabaseDataType.FileEncryptionKey, _fileEncryptionKey);
            SaveToByteList(l, DatabaseDataType.Creator, _creator);
            SaveToByteList(l, DatabaseDataType.CreateDateUTC, _createDate);
            SaveToByteList(l, DatabaseDataType.Caption, _caption);
            //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString());
            //SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString());
            SaveToByteList(l, DatabaseDataType.Tags, Tags.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.PermissionGroupsNewRow, PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.DatabaseAdminGroups, DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.GlobalScale, _globalScale.ToString(Constants.Format_Float1));
            //SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString());
            SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, _muf.ReloadDelaySecond.ToString());
            //SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
            SaveToByteList(l, DatabaseDataType.RulesScript, _rulesScript);
            //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
            //SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _filterImagePfad);
            SaveToByteList(l, DatabaseDataType.AdditionaFilesPfad, _additionaFilesPfad);
            SaveToByteList(l, DatabaseDataType.RowQuickInfo, _zeilenQuickInfo);
            SaveToByteList(l, DatabaseDataType.StandardFormulaFile, _standardFormulaFile);
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
            SaveToByteList(l, DatabaseDataType.Layouts, Layouts.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.AutoExport, Export.ToString(true));
            // Beim Erstellen des Undo-Speichers die Works nicht ver�ndern, da auch bei einem nicht
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
            e.Data = cryptPos > 0 ? Cryptography.SimpleCrypt(l.ToArray(), _globalShowPass, 1, cryptPos, l.Count - 1) : l.ToArray();
        } catch {
            ToListOfByte(sender, e);
        }
    }

    #endregion

    //private void Views_ListOrItemChanged(object sender, System.EventArgs e) {
    //    if (_muf.IsParsing) { return; } // hier schon raus, es muss kein ToString ausgef�hrt werden. Kann zu Endlosschleifen f�hren.
    //    AddPending(DatabaseDataType.Views, -1, OldFormulaViews.ToString(true), false);
    //}

    // // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
    // ~Database()
    // {
    //     // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}