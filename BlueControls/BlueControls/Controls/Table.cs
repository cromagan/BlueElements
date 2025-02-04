// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("SelectedRowChanged")]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class Table : GenericControlReciverSender, IContextMenu, ITranslateable, IHasDatabase, IOpenScriptEditor, IStyleable {

    #region Fields

    public readonly FilterCollection Filter = new("DefaultTableFilter");
    private readonly List<string> _collapsed = [];
    private readonly object _lockUserAction = new();
    private string _arrangement = string.Empty;
    private AutoFilter? _autoFilter;
    private bool _controlPressing;
    private ColumnViewCollection? _currentArrangement;
    private DateTime? _databaseDrawError;
    private bool _editButton;
    private bool _isinClick;
    private bool _isinDoubleClick;
    private bool _isinKeyDown;
    private bool _isinMouseDown;
    private bool _isinMouseMove;
    private bool _isinMouseWheel;
    private bool _isinSizeChanged;
    private bool _isinVisibleChanged;

    /// <summary>
    ///  Wird DatabaseAdded gehandlet?
    /// </summary>
    private ColumnViewItem? _mouseOverColumn;

    private RowData? _mouseOverRow;
    private Progressbar? _pg;
    private List<RowData>? _rowsFilteredAndPinned;
    private SearchAndReplaceInCells? _searchAndReplaceInCells;
    private SearchAndReplaceInDBScripts? _searchAndReplaceInDBScripts;
    private string _sheetStyle = Win11;

    private bool _showNumber;

    private RowSortDefinition? _sortDefinitionTemporary;

    private string _storedView = string.Empty;

    private Rectangle _tmpCursorRect = Rectangle.Empty;

    private RowItem? _unterschiede;

    private float _zoom = 1f;

    #endregion

    #region Constructors

    public Table() : base(true, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        Filter.PropertyChanged += Filter_PropertyChanged;
        Filter.RowsChanged += Filter_PropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEditBlockReasonEventArgs>? BlockEdit;

    public event EventHandler<CellEventArgs>? CellClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? DatabaseChanged;

    public new event EventHandler<CellExtEventArgs>? DoubleClick;

    public event EventHandler? FilterChanged;

    public event EventHandler? PinnedChanged;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowNullableEventArgs>? SelectedRowChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler? VisibleRowsChanged;

    #endregion

    #region Properties

    [DefaultValue(1.0f)]
    public float AdditionalScale => 1f;

    [DefaultValue("")]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public string Arrangement {
        get => _arrangement;
        set {
            if (value != _arrangement) {
                _arrangement = value;

                OnViewChanged();
                CursorPos_Set(CursorPosColumn, CursorPosRow, true);
            }
        }
    }

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }

            if (_currentArrangement != null) { return _currentArrangement; }

            var tcvc = ColumnViewCollection.ParseAll(db);
            _currentArrangement = tcvc.Get(_arrangement);

            if (_currentArrangement == null && tcvc.Count > 1) { _currentArrangement = tcvc[1]; }
            if (_currentArrangement == null && tcvc.Count > 0) { _currentArrangement = tcvc[0]; }

            if (_currentArrangement is { }) {
                _currentArrangement.SheetStyle = _sheetStyle;
                _currentArrangement.ClientWidth = (int)(DisplayRectangleWithoutSlider().Width / _zoom);
                _currentArrangement.ComputeAllColumnPositions();
            }

            return _currentArrangement;
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnViewItem? CursorPosColumn { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowData? CursorPosRow { get; private set; }

    /// <summary>
    /// Datenbanken können mit DatabaseSet gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Database? Database { get; private set; }

    [DefaultValue(false)]
    public bool EditButton {
        get => _editButton;
        set {
            if (_editButton == value) { return; }
            _editButton = value;
            btnEdit.Visible = _editButton;
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Filterausgabe FilterOutputType { get; } = Filterausgabe.Gewähle_Zeile;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem> PinnedRows { get; } = [];

    public bool PowerEdit {
        set {
            if (IsDisposed || Database is not { IsDisposed: false }) { return; }
            Database.PowerEdit = value;
            Filter.Invalidate_FilteredRows(); // Split-Spalten-Filter
            Invalidate_SortedRowData(); // Neue Zeilen können nun erlaubt sein
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<RowItem> RowsFiltered {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return new List<RowItem>().AsReadOnly(); }
            return Filter.Rows;
        }
    }

    public string SheetStyle {
        get => _sheetStyle;
        set {
            if (IsDisposed) { return; }
            if (_sheetStyle == value) { return; }
            _sheetStyle = value;
            Invalidate_CurrentArrangement();
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool ShowNumber {
        get => _showNumber;
        set {
            if (value == _showNumber) { return; }
            CloseAllComponents();
            _showNumber = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowWaitScreen { get; set; } = true;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowSortDefinition? SortDefinitionTemporary {
        get => _sortDefinitionTemporary;
        set {
            if (_sortDefinitionTemporary != null && value != null && _sortDefinitionTemporary.ParseableItems().FinishParseable() == value.ParseableItems().FinishParseable()) { return; }
            if (_sortDefinitionTemporary == value) { return; }
            _sortDefinitionTemporary = value;
            _Database_SortParameterChanged(this, System.EventArgs.Empty);
        }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? Unterschiede {
        get => _unterschiede;
        set {
            //if (_Unterschiede != null && value != null && _sortDefinitionTemporary.ToString(false) == value.ToString(false)) { return; }
            if (_unterschiede == value) { return; }
            _unterschiede = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long VisibleRowCount { get; private set; }

    public float Zoom {
        get => _zoom;
        set {
            if (Math.Abs(_zoom - value) < DefaultTolerance) { return; }

            _zoom = value;
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public static (List<RowData> rows, long visiblerowcount) CalculateSortedRows(Database db, IEnumerable<RowItem> filteredRows, IEnumerable<RowItem>? pinnedRows, RowSortDefinition? sortused) {
        if (db.IsDisposed) { return ([], 0); }

        var vrc = 0;

        #region Ermitteln, ob mindestens eine Überschrift vorhanden ist (capName)

        var capName = pinnedRows != null && pinnedRows.Any();
        if (!capName && db.Column.SysChapter is { IsDisposed: false } cap) {
            foreach (var thisRow in filteredRows) {
                if (thisRow.Database != null && !thisRow.CellIsNullOrEmpty(cap)) {
                    capName = true;
                    break;
                }
            }
        }

        #endregion

        #region Refresh

        var colsToRefresh = new List<ColumnItem>();
        var reverse = false;
        if (sortused is { } rsd) { colsToRefresh.AddRange(rsd.Columns); reverse = rsd.Reverse; }
        //if (db.Column.SysChapter is { IsDisposed: false } csc) { _ = colsToRefresh.AddIfNotExists(csc); }
        if (db.Column.First() is { IsDisposed: false } cf) { _ = colsToRefresh.AddIfNotExists(cf); }

        #endregion

        var lockMe = new object();

        #region _Angepinnten Zeilen erstellen (_pinnedData)

        List<RowData> pinnedData = [];

        if (pinnedRows != null) {
            _ = Parallel.ForEach(pinnedRows, thisRow => {
                var rd = new RowData(thisRow, "Angepinnt");
                rd.PinStateSortAddition = "1";
                rd.MarkYellow = true;
                rd.AdditionalSort = thisRow.CompareKey(colsToRefresh);

                lock (lockMe) {
                    vrc++;
                    pinnedData.Add(rd);
                }
            });
        }

        #endregion

        #region Gefiltere Zeilen erstellen (_rowData)

        List<RowData> rowData = [];
        _ = Parallel.ForEach(filteredRows, thisRow => {
            var adk = thisRow.CompareKey(colsToRefresh);

            var markYellow = pinnedRows != null && pinnedRows.Contains(thisRow);
            var added = markYellow;

            List<string> caps;
            if (db.Column.SysChapter is { IsDisposed: false } sc) {
                caps = thisRow.CellGetList(sc);
            } else {
                caps = [];
            }

            if (caps.Count > 0) {
                if (caps.Contains(string.Empty)) {
                    _ = caps.Remove(string.Empty);
                    caps.Add("-?-");
                }
            }

            if (caps.Count == 0 && capName) { caps.Add("Weitere Zeilen"); }
            if (caps.Count == 0) { caps.Add(string.Empty); }

            foreach (var thisCap in caps) {
                var rd = new RowData(thisRow, thisCap);

                rd.PinStateSortAddition = "2";
                rd.MarkYellow = markYellow;
                rd.AdditionalSort = adk;
                lock (lockMe) {
                    rowData.Add(rd);
                    if (!added) { vrc++; added = true; }
                }
            }
        });

        #endregion

        pinnedData.Sort();
        rowData.Sort();

        if (reverse) { rowData.Reverse(); }

        rowData.InsertRange(0, pinnedData);
        return (rowData, vrc);
    }

    public static string ColumnUsage(ColumnItem? column) {
        if (column?.Database is not { IsDisposed: false } db) { return string.Empty; }

        var t = "<b><u>Verwendung von " + column.ReadableText() + "</b></u><br>";
        if (column.IsSystemColumn()) {
            t += " - Systemspalte<br>";
        }

        if (db.SortDefinition?.Columns.Contains(column) ?? false) { t += " - Sortierung<br>"; }
        //var view = false;
        //foreach (var thisView in OldFormulaViews) {
        //    if (thisView[column] != null) { view = true; }
        //}
        //if (view) { t += " - Formular-Ansichten<br>"; }
        var cola = false;
        var first = true;

        var tcvc = ColumnViewCollection.ParseAll(db);
        foreach (var thisView in tcvc) {
            if (!first && thisView[column] != null) { cola = true; }
            first = false;
        }
        if (cola) { t += " - Spalten-Anordnungen<br>"; }
        if (column.UsedInScript()) { t += " - Skripte<br>"; }
        if (db.ZeilenQuickInfo.ToUpperInvariant().Contains(column.KeyName.ToUpperInvariant())) { t += " - Zeilen-Quick-Info<br>"; }
        //if (column.Tags.JoinWithCr().ToUpperInvariant().Contains(column.KeyName.ToUpperInvariant())) { t += " - Datenbank-Tags<br>"; }

        if (!string.IsNullOrEmpty(column.Am_A_Key_For_Other_Column)) { t += column.Am_A_Key_For_Other_Column; }

        if (!string.IsNullOrEmpty(column.SystemInfo)) {
            t += "<br><br><b>Gesammelte Infos:</b><br>";
            t += column.SystemInfo;
        }
        var l = column.Contents();
        if (l.Count > 0) {
            t += "<br><br><b>Zusatz-Info:</b><br>";
            t = t + " - Befüllt mit " + l.Count + " verschiedenen Werten";
        }

        return t;
    }

    public static void CopyToClipboard(ColumnItem? column, RowItem? row, bool meldung) {
        try {
            if (row != null && column != null && column.Function.CanBeCheckedByRules()) {
                var c = row.CellGetString(column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");
                _ = Generic.CopytoClipboard(c);
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), ImageCode.Kopieren); }
            } else {
                if (meldung) { Notification.Show(LanguageTool.DoTranslate("Bei dieser Zelle nicht möglich."), ImageCode.Warnung); }
            }
        } catch {
            if (meldung) { Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), ImageCode.Warnung); }
        }
    }

    //    return renderer.GetSizeOfCellContent(column, row.CellGetString(column), Design.Table_Cell, States.Standard,
    //        column.BehaviorOfImageAndText, column.DoOpticalTranslation, column.OpticalReplace, db.GlobalScale, column.ConstantHeightOfImageCode);
    //}
    public static void Database_AdditionalRepair(object sender, System.EventArgs e) {
        if (sender is not Database db) { return; }

        RepairColumnArrangements(db);
    }

    //    if (column.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
    //        var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
    //        return lcolumn != null && lrow != null ? ContentSize(lcolumn, lrow, renderer)
    //            : new Size(16, 16);
    //    }
    public static string Database_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Datenbank<br>zu erhalten:", string.Empty, FormatHolder.Text);

    public static void DoUndo(ColumnItem? column, RowItem? row) {
        if (column is not { IsDisposed: false }) { return; }
        if (row is not { IsDisposed: false }) { return; }
        if (column.Function == ColumnFunction.Virtuelle_Spalte) { return; }

        if (column.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, true, false);
            if (lcolumn != null && lrow != null) { DoUndo(lcolumn, lrow); }
            return;
        }
        var cellKey = CellCollection.KeyOfCell(column, row);
        var i = UndoItems(column.Database, cellKey);
        if (i.Count < 1) {
            MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", ImageCode.Information, "OK");
            return;
        }
        var v = InputBoxListBoxStyle.Show("Vorherigen Eintrag wählen:", i, CheckBehavior.SingleSelection, ["Cancel"], AddType.None);
        if (v is not { Count: 1 }) { return; }
        if (v[0] == "Cancel") { return; } // =Aktueller Eintrag angeklickt
        row.CellSet(column, v[0].Substring(5), "Undo-Befehl");
        //row.Database?.Row.ExecuteValueChangedEvent(true);
    }

    public static void ImportBdb(Database database) {
        using ImportBdb x = new(database);
        _ = x.ShowDialog();
    }

    public static void ImportCsv(Database database, string csvtxt) {
        using ImportCsv x = new(database, csvtxt);
        _ = x.ShowDialog();
    }

    public static List<string> Permission_AllUsed(bool mitRowCreator) {
        var l = new List<string>();

        foreach (var thisDb in Database.AllFiles) {
            if (!thisDb.IsDisposed) {
                l.AddRange(Permission_AllUsedInThisDB(thisDb, mitRowCreator));
            }
        }

        return Database.RepairUserGroups(l);
    }

    public static List<string> Permission_AllUsedInThisDB(Database db, bool mitRowCreator) {
        List<string> e = [];
        foreach (var thisColumnItem in db.Column) {
            if (thisColumnItem != null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(db.PermissionGroupsNewRow);
        e.AddRange(db.DatenbankAdmin);

        var tcvc = ColumnViewCollection.ParseAll(db);
        foreach (var thisArrangement in tcvc) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }

        foreach (var thisEv in db.EventScript) {
            e.AddRange(thisEv.UserGroups);
        }

        e.Add(Everybody);
        e.Add("#User: " + Generic.UserName);

        if (mitRowCreator) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.Add(Generic.UserGroup);
        e.RemoveString(Administrator, false);

        return Database.RepairUserGroups(e);
    }

    public static string QuickInfoText(ColumnItem col, string additionalText) {
        if (col.IsDisposed || col.Database is not { IsDisposed: false }) { return string.Empty; }

        var T = string.Empty;
        if (!string.IsNullOrEmpty(col.QuickInfo)) { T += col.QuickInfo; }
        if (col.Database.IsAdministrator() && !string.IsNullOrEmpty(col.AdminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + col.AdminInfo; }
        if (col.Database.IsAdministrator() && col.Tags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + col.Tags.JoinWith("<br>"); }
        if (col.Database.IsAdministrator()) { T = T + "<br><br>" + ColumnUsage(col); }
        T = T.Trim();
        T = T.Trim("<br>");
        T = T.Trim();
        if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(additionalText)) {
            T = "<b><u>" + additionalText + "</b></u><br><br>" + T;
        }
        return T;
    }

    public static Renderer_Abstract RendererOf(ColumnItem? column, string style) {
        if (column == null || string.IsNullOrEmpty(column.DefaultRenderer)) { return Renderer_Abstract.Default; }

        var renderer = ParsebleItem.NewByTypeName<Renderer_Abstract>(column.DefaultRenderer);
        if (renderer == null) { return Renderer_Abstract.Default; }

        renderer.Parse(column.RendererSettings);
        renderer.SheetStyle = style;

        return renderer;
    }

    public static void SearchNextText(string searchTxt, Table tableView, ColumnViewItem? column, RowData? row, out ColumnViewItem? foundColumn, out RowData? foundRow, bool vereinfachteSuche) {
        if (tableView.Database is not { IsDisposed: false } db) {
            MessageBox.Show("Datenbank-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        searchTxt = searchTxt.Trim();
        if (tableView.CurrentArrangement is not { IsDisposed: false } ca) {
            MessageBox.Show("Datenbank-Ansichts-Fehler.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        row ??= tableView.View_RowLast();
        column ??= ca.Last();
        var rowsChecked = 0;
        if (string.IsNullOrEmpty(searchTxt)) {
            MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            foundColumn = null;
            foundRow = null;
            return;
        }

        do {
            column = ca.NextVisible(column);

            var renderer = column?.GetRenderer(tableView.SheetStyle);

            if (column is not { }) {
                column = ca.First();
                if (rowsChecked > db.Row.Count() + 1) {
                    foundColumn = null;
                    foundRow = null;
                    return;
                }
                rowsChecked++;
                row = tableView.View_NextRow(row) ?? tableView.View_RowFirst();
            }

            var ist1 = string.Empty;
            var ist2 = string.Empty;

            var tmprow = row?.Row;
            //if (column?.Column is { Function: ColumnFunction.Verknüpfung_zu_anderer_Datenbank } cv) {
            //    var (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(cv, tmprow, false, false);

            //    if (contentHolderCellColumn != null && contentHolderCellRow != null) {
            //        ist1 = contentHolderCellRow.CellGetString(contentHolderCellColumn);
            //        if (renderer is { }) {
            //            ist2 = renderer.ValueReadable(contentHolderCellRow.CellGetString(contentHolderCellColumn),
            //                ShortenStyle.Both, contentHolderCellColumn.DoOpticalTranslation);
            //        }
            //    }
            //} else {
            if (tmprow != null && column?.Column is { } c) {
                ist1 = tmprow.CellGetString(c);
                if (renderer is { }) {
                    ist2 = renderer.ValueReadable(tmprow.CellGetString(c), ShortenStyle.Both, c.DoOpticalTranslation);
                }
            }
            //}

            if (column?.Column is { FormatierungErlaubt: true }) {
                ExtText l = new(Design.TextBox, States.Standard) {
                    HtmlText = ist1
                };
                ist1 = l.PlainText;
            }
            // Allgemeine Prüfung
            if (!string.IsNullOrEmpty(ist1) && ist1.ToLowerInvariant().Contains(searchTxt.ToLowerInvariant())) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            // Prüfung mit und ohne Ersetzungen / Prefix / Suffix
            if (!string.IsNullOrEmpty(ist2) && ist2.ToLowerInvariant().Contains(searchTxt.ToLowerInvariant())) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            if (vereinfachteSuche) {
                var ist3 = ist2.StarkeVereinfachung(" ,", true);
                var searchTxt3 = searchTxt.StarkeVereinfachung(" ,", true);
                if (!string.IsNullOrEmpty(ist3) && ist3.ToLowerInvariant().Contains(searchTxt3.ToLowerInvariant())) {
                    foundColumn = column;
                    foundRow = row;
                    return;
                }
            }
        } while (true);
    }

    public static List<AbstractListItem> UndoItems(Database? db, string cellkey) {
        List<AbstractListItem> i = [];

        if (db is { IsDisposed: false }) {
            //database.GetUndoCache();

            if (db.Undo.Count == 0) { return i; }

            var isfirst = true;
            TextListItem? las = null;
            var lasNr = -1;
            var co = 0;
            for (var z = db.Undo.Count - 1; z >= 0; z--) {
                if (db.Undo[z].CellKey == cellkey) {
                    co++;
                    lasNr = z;
                    if (isfirst) {
                        las = new TextListItem(
                            "Aktueller Text - ab " + db.Undo[z].DateTimeUtc + " UTC, geändert von " +
                            db.Undo[z].User, "Cancel", null, false, true, string.Empty);
                    } else {
                        las = new TextListItem(
                            "ab " + db.Undo[z].DateTimeUtc + " UTC, geändert von " + db.Undo[z].User,
                            co.ToStringInt5() + db.Undo[z].ChangedTo, null, false, true,
                            string.Empty);
                    }
                    isfirst = false;
                    i.Add(las);
                }
            }

            if (las != null) {
                co++;
                i.Add(ItemOf("vor " + db.Undo[lasNr].DateTimeUtc + " UTC", co.ToStringInt5() + db.Undo[lasNr].PreviousValue));
            }
        }

        return i;
    }

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, Database? database, string showingKey) {
        if (columnArrangementSelector is not { IsDisposed: false }) { return; }

        columnArrangementSelector.AutoSort = false;

        columnArrangementSelector.ItemClear();
        columnArrangementSelector.DropDownStyle = ComboBoxStyle.DropDownList;

        if (database is { IsDisposed: false } db) {
            var tcvc = ColumnViewCollection.ParseAll(db);

            foreach (var thisArrangement in tcvc) {
                if (db.PermissionCheck(thisArrangement.PermissionGroups_Show, null)) {
                    columnArrangementSelector.ItemAdd(ItemOf(thisArrangement));
                }
            }
        }

        columnArrangementSelector.Enabled = columnArrangementSelector.ItemCount > 1;

        if (columnArrangementSelector[showingKey] == null) {
            if (columnArrangementSelector.ItemCount > 1) {
                showingKey = columnArrangementSelector[1].KeyName;
            } else {
                showingKey = string.Empty;
            }
        }

        columnArrangementSelector.Text = showingKey;
    }

    public void CheckView() {
        var db = Database;
        if (_mouseOverColumn?.Column?.Database != db) { _mouseOverColumn = null; }
        if (_mouseOverRow?.Row.Database != db) { _mouseOverRow = null; }
        if (CursorPosColumn?.Column?.Database != db) { CursorPosColumn = null; }
        if (CursorPosRow?.Row.Database != db) { CursorPosRow = null; }

        if (CurrentArrangement is { IsDisposed: false } ca && db != null) {
            if (!db.PermissionCheck(ca.PermissionGroups_Show, null)) { Arrangement = string.Empty; }
        } else {
            Arrangement = string.Empty;
        }
    }

    public void CollapesAll() {
        if (Database?.Column.SysChapter is not { IsDisposed: false } sc) { return; }

        _collapsed.Clear();
        _collapsed.AddRange(sc.Contents());

        Invalidate_SortedRowData();
    }

    public void CursorPos_Reset() => CursorPos_Set(null, null, false);

    public void CursorPos_Set(ColumnViewItem? column, RowData? row, bool ensureVisible) {
        if (IsDisposed || Database is not { IsDisposed: false } || row == null || column == null ||
            CurrentArrangement is not { } ca2 || !ca2.Contains(column) ||
            RowsFilteredAndPinned() is not { } s || !s.Contains(row)) {
            column = null;
            row = null;
        }

        var sameRow = CursorPosRow == row;

        if (CursorPosColumn == column && CursorPosRow == row) { return; }
        QuickInfo = string.Empty;
        CursorPosColumn = column;
        CursorPosRow = row;

        //if (CursorPosColumn != column) { return; }

        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (ensureVisible && CurrentArrangement is { } ca) {
            _ = EnsureVisible(ca, CursorPosColumn, CursorPosRow);
        }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowNullableEventArgs(row?.Row));

            if (FilterOutputType == Filterausgabe.Gewähle_Zeile) {
                if (row?.Row is { IsDisposed: false } setedrow) {
                    using var nfc = new FilterCollection(setedrow, "TableOutput");
                    FilterOutput.ChangeTo(nfc);
                } else {
                    FilterOutput.ChangeTo(new FilterItem(db, "Dummy"));
                }
            }
        }
    }

    public void DatabaseSet(Database? db, string viewCode) {
        if (Database == db && string.IsNullOrEmpty(viewCode)) { return; }

        CloseAllComponents();

        if (Database is { IsDisposed: false } db1) {
            // auch Disposed Datenbanken die Bezüge entfernen!
            db1.Cell.CellValueChanged -= _Database_CellValueChanged;
            db1.Loaded -= _Database_DatabaseLoaded;
            db1.Loading -= _Database_StoreView;
            db1.ViewChanged -= _Database_ViewChanged;
            db1.SortParameterChanged -= _Database_SortParameterChanged;
            db1.Row.RowRemoving -= Row_RowRemoving;
            db1.Row.RowRemoved -= Row_RowRemoved;
            db1.Row.RowGotData -= _Database_Row_RowGotData;
            db1.Column.ColumnRemoving -= Column_ItemRemoving;
            db1.Column.ColumnRemoved -= _Database_ViewChanged;
            db1.Column.ColumnAdded -= _Database_ViewChanged;
            db1.ProgressbarInfo -= _Database_ProgressbarInfo;
            db1.DisposingEvent -= _database_Disposing;
            db1.InvalidateView -= Database_InvalidateView;
            Database.ForceSaveAll();
            MultiUserFile.SaveAll(false);
        }
        ShowWaitScreen = true;
        Refresh(); // um die Uhr anzuzeigen
        Database = db;
        Invalidate_CurrentArrangement();
        Invalidate_SortedRowData();
        Filter.Database = db;

        _databaseDrawError = null;
        //InitializeSkin(); // Neue Schriftgrößen
        if (Database is { IsDisposed: false } db2) {
            RepairColumnArrangements(db2);
            FilterOutput.Database = db2;

            db2.Cell.CellValueChanged += _Database_CellValueChanged;
            db2.Loaded += _Database_DatabaseLoaded;
            db2.Loading += _Database_StoreView;
            db2.ViewChanged += _Database_ViewChanged;
            db2.SortParameterChanged += _Database_SortParameterChanged;
            db2.Row.RowRemoving += Row_RowRemoving;
            db2.Row.RowRemoved += Row_RowRemoved;
            db2.Row.RowGotData += _Database_Row_RowGotData;
            db2.Column.ColumnAdded += _Database_ViewChanged;
            db2.Column.ColumnRemoving += Column_ItemRemoving;
            db2.Column.ColumnRemoved += _Database_ViewChanged;
            db2.ProgressbarInfo += _Database_ProgressbarInfo;
            db2.DisposingEvent += _database_Disposing;
            db2.InvalidateView += Database_InvalidateView;
        }

        ParseView(viewCode);

        ShowWaitScreen = false;
        OnDatabaseChanged();
    }

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) => OnContextMenuItemClicked(e);

    public void DoZoom(bool zoomIn) {
        var nz = _zoom;

        if (zoomIn) {
            nz *= 1.05f;
        } else {
            nz *= 1f / 1.05f;
        }

        nz = Math.Max(nz, 0.5f);
        nz = Math.Min(nz, 4);
        Zoom = nz;
    }

    public string EditableErrorReason(ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, EditableErrorReasonType mode, bool checkUserRights, bool checkEditmode, bool maychangeview, FilterItem[]? filter) {
        var f = CellCollection.EditableErrorReason(cellInThisDatabaseColumn?.Column, cellInThisDatabaseRow?.Row, mode, checkUserRights, checkEditmode, true, false, filter);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (checkEditmode) {
            if (CurrentArrangement is not { IsDisposed: false } ca || !ca.Contains(cellInThisDatabaseColumn)) {
                return "Ansicht veraltet";
            }

            if (cellInThisDatabaseRow != null) {
                if (maychangeview && !EnsureVisible(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow)) {
                    return "Zelle konnte nicht angezeigt werden.";
                }
                if (!IsOnScreen(ca, cellInThisDatabaseColumn, cellInThisDatabaseRow, DisplayRectangle)) {
                    return "Die Zelle wird nicht angezeigt.";
                }
            } else {
                if (maychangeview && !EnsureVisible(cellInThisDatabaseColumn)) {
                    return "Zelle konnte nicht angezeigt werden.";
                }
                //if (!IsOnScreen(viewItem, DisplayRectangle)) {
                //    return "Die Zelle wird nicht angezeigt.";
                //}
            }
        }

        CellEditBlockReasonEventArgs ed = new(cellInThisDatabaseColumn?.Column, cellInThisDatabaseRow?.Row, string.Empty);
        OnBlockEdit(ed);
        return ed.BlockReason;
    }

    public bool EnsureVisible(ColumnViewCollection ca, ColumnViewItem? viewItem, RowData? row) => EnsureVisible(viewItem) && EnsureVisible(ca, row);

    public void ExpandAll() {
        if (Database?.Column.SysChapter is null) { return; }

        _collapsed.Clear();
        CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
        Invalidate_SortedRowData();
    }

    public string Export_CSV(FirstRow firstRow) => Database == null ? string.Empty : Database.Export_CSV(firstRow, CurrentArrangement?.ListOfUsedColumn(), RowsVisibleUnique());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return string.Empty; }
        List<ColumnItem> l = [onlyColumn];
        return Database.Export_CSV(firstRow, l, RowsVisibleUnique());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        Html da = new(db.TableName.FileNameWithoutSuffix());
        da.AddCaption(db.Caption);
        da.TableBeginn();

        #region Spaltenköpfe

        da.RowBeginn();
        foreach (var thisColumn in ca) {
            if (thisColumn.Column != null) {
                da.CellAdd(thisColumn.Column.ReadableText().Replace(";", "<br>"), thisColumn.Column.BackColor);
            }
        }

        da.RowEnd();

        #endregion

        #region Zeilen

        if (RowsFilteredAndPinned() is { } rw) {
            foreach (var thisRow in rw) {
                if (thisRow is { IsDisposed: false }) {
                    da.RowBeginn();
                    foreach (var thisColumn in ca) {
                        if (thisColumn.Column != null) {
                            var lcColumn = thisColumn.Column;
                            var lCrow = thisRow.Row;
                            //if (thisColumn.Column.Function is ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
                            //    (lcColumn, lCrow, _, _) = CellCollection.LinkedCellData(thisColumn.Column, thisRow.Row, false, false);
                            //}

                            if (lcColumn != null) {
                                da.CellAdd(lCrow.CellGetList(lcColumn).JoinWith("<br>"), thisColumn.Column.BackColor);
                            } else {
                                da.CellAdd(" ", thisColumn.Column.BackColor);
                            }
                        }
                    }

                    da.RowEnd();
                }
            }
        }

        #endregion

        da.TableEnd();
        da.AddFoot();
        da.Save(filename, execute);
    }

    /// <summary>
    /// Alle gefilteren Zeilen. Jede Zeile ist maximal einmal in dieser Liste vorhanden. Angepinnte Zeilen addiert worden
    /// </summary>
    /// <returns></returns>
    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || SliderY.Focused() || SliderX.Focused() || BTB.Focused || BCB.Focused;

    public void GetContextMenuItems(ContextMenuInitEventArgs e) => OnContextMenuInit(e);

    public void ImportBdb() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        ImportBdb(db);
    }

    public void ImportClipboard() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        if (!Clipboard.ContainsText()) {
            Notification.Show("Abbruch,<br>kein Text im Zwischenspeicher!", ImageCode.Information);
            return;
        }

        var nt = Clipboard.GetText();
        ImportCsv(nt);
    }

    public void ImportCsv(string csvtxt) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        ImportCsv(db, csvtxt);
    }

    public bool Mouse_IsInHead() {
        if (CurrentArrangement is not { IsDisposed: false } ca) { return false; }

        return MousePos().Y <= ca.HeadSize();
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OpenScriptEditor() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<DatabaseScriptEditor>(db);
        se.Row = CursorPosRow?.Row;
    }

    public void OpenSearchAndReplaceInCells() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditCurrently) || Database == null) { return; }

        if (!Database.IsAdministrator()) { return; }

        if (_searchAndReplaceInCells is not { IsDisposed: false } || !_searchAndReplaceInCells.Visible) {
            _searchAndReplaceInCells = new SearchAndReplaceInCells(this);
            _searchAndReplaceInCells.Show();
        }
    }

    public void OpenSearchAndReplaceInDBScripts() {
        if (!Generic.IsAdministrator()) { return; }

        if (_searchAndReplaceInDBScripts is not { IsDisposed: false } || !_searchAndReplaceInDBScripts.Visible) {
            _searchAndReplaceInDBScripts = new SearchAndReplaceInDBScripts();
            _searchAndReplaceInDBScripts.Show();
        }
    }

    public void Pin(List<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist

        rows ??= [];

        rows = rows.Distinct().ToList();
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void PinAdd(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        PinnedRows.Add(row);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        _ = PinnedRows.Remove(row);
        Invalidate_SortedRowData();
        OnPinnedChanged();
    }

    public void ResetView() {
        Filter.Clear();

        PinnedRows.Clear();
        _collapsed.Clear();

        QuickInfo = string.Empty;
        _sortDefinitionTemporary = null;
        _mouseOverColumn = null;
        _mouseOverRow = null;
        CursorPosColumn = null;
        CursorPosRow = null;
        _arrangement = string.Empty;
        _unterschiede = null;
        _zoom = 1f;

        OnViewChanged();
    }

    public List<RowData>? RowsFilteredAndPinned() {
        if (IsDisposed) { return null; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return null; }

        if (_rowsFilteredAndPinned != null) { return _rowsFilteredAndPinned; }

        try {
            var displayR = DisplayRectangleWithoutSlider();
            var maxY = 0;
            if (UserEdit_NewRowAllowed()) { maxY += 18; }
            var expanded = true;
            var lastCap = string.Empty;

            if (IsDisposed || Database is not { IsDisposed: false } db) {
                VisibleRowCount = 0;
                _rowsFilteredAndPinned = [];
            } else {
                (List<RowData> sortedRowDataNew, VisibleRowCount) = CalculateSortedRows(db, RowsFiltered, PinnedRows, SortUsed());

                var sortedRowDataTmp = new List<RowData>();

                foreach (var thisRow in sortedRowDataNew) {
                    var thisRowData = thisRow;

                    if (_mouseOverRow != null && _mouseOverRow.Row == thisRow.Row && _mouseOverRow.RowChapter == thisRow.RowChapter) {
                        _mouseOverRow.GetDataFrom(thisRowData);
                        thisRowData = _mouseOverRow;
                    } // Mouse-Daten wiederverwenden

                    if (CursorPosRow?.Row != null && CursorPosRow.Row == thisRow.Row && CursorPosRow.RowChapter == thisRow.RowChapter) {
                        CursorPosRow.GetDataFrom(thisRowData);
                        thisRowData = CursorPosRow;
                    } // Cursor-Daten wiederverwenden

                    thisRowData.Y = maxY;

                    #region Caption bestimmen

                    if (thisRow.RowChapter != lastCap) {
                        thisRowData.Y += ca.RowChapterHeight;
                        expanded = !_collapsed.Contains(thisRowData.RowChapter);
                        maxY += ca.RowChapterHeight;
                        thisRowData.ShowCap = true;
                        lastCap = thisRow.RowChapter;
                    } else {
                        thisRowData.ShowCap = false;
                    }

                    #endregion

                    #region Expaned (oder pinned) bestimmen

                    thisRowData.Expanded = expanded;
                    if (thisRowData.Expanded) {
                        thisRowData.CalculateDrawHeight(ca, ca.HeadSize(), displayR, SheetStyle);
                        maxY += thisRowData.DrawHeight;
                    }

                    #endregion

                    sortedRowDataTmp.Add(thisRowData);
                }

                if (CursorPosRow?.Row != null && !sortedRowDataTmp.Contains(CursorPosRow)) { CursorPos_Reset(); }
                _mouseOverRow = null;

                _rowsFilteredAndPinned = sortedRowDataTmp;
            }
            //         _ = EnsureVisible(CursorPosColumn, CursorPosRow);
            // EnsureVisible macht probleme, wenn mit der Maus auf ein linked Cell gezogen wird und eine andere Cell
            //markiert ist. Dann wird sortiren angestoßen, und der Cursor zurückgesprungen

            OnVisibleRowsChanged();

            return RowsFilteredAndPinned(); // Rekursiver Aufruf. Manchmal funktiniert OnRowsSorted nicht ...
        } catch {
            // Komisch, manchmal wird die Variable _sortedRowDatax verworfen.
            Develop.CheckStackForOverflow();
            Invalidate_SortedRowData();
            return RowsFilteredAndPinned();
        }
    }

    public List<RowItem> RowsVisibleUnique() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return []; }

        var f = RowsFiltered;

        ConcurrentBag<RowItem> l = [];

        try {
            var lockMe = new object();
            _ = Parallel.ForEach(Database.Row, thisRowItem => {
                if (thisRowItem != null) {
                    if (f.Contains(thisRowItem) || PinnedRows.Contains(thisRowItem)) {
                        lock (lockMe) { l.Add(thisRowItem); }
                    }
                }
            });
        } catch {
            Develop.CheckStackForOverflow();
            return RowsVisibleUnique();
        }

        return l.ToList();
    }

    public ColumnViewItem? View_ColumnFirst() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
        return CurrentArrangement is { Count: not 0 } ca ? ca[0] : null;
    }

    public RowData? View_NextRow(RowData? row) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
        if (row is not { IsDisposed: false }) { return null; }

        if (RowsFilteredAndPinned() is not { } sr) { return null; }

        var rowNr = sr.IndexOf(row);
        return rowNr < 0 || rowNr >= sr.Count - 1 ? null : sr[rowNr + 1];
    }

    public RowData? View_PreviousRow(RowData? row) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
        if (row is not { IsDisposed: false }) { return null; }
        if (RowsFilteredAndPinned() is not { } sr) { return null; }
        var rowNr = sr.IndexOf(row);
        return rowNr < 1 ? null : sr[rowNr - 1];
    }

    public RowData? View_RowFirst() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
        if (RowsFilteredAndPinned() is not { } sr) { return null; }
        return sr.Count == 0 ? null : sr[0];
    }

    public RowData? View_RowLast() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return null; }
        if (RowsFilteredAndPinned() is not { } sr) { return null; }
        return sr.Count == 0 ? null : sr[sr.Count - 1];
    }

    public string ViewToString() {
        List<string> result = [];
        result.ParseableAdd("Arrangement", _arrangement);
        result.ParseableAdd("Zoom", _zoom);
        result.ParseableAdd("Filters", (IStringable?)Filter);
        result.ParseableAdd("SliderX", SliderX.Value);
        result.ParseableAdd("SliderY", SliderY.Value);
        result.ParseableAdd("Pin", PinnedRows, false);
        result.ParseableAdd("Collapsed", _collapsed, false);
        result.ParseableAdd("Reduced", CurrentArrangement?.ReducedColumns(), false);
        result.ParseableAdd("TempSort", _sortDefinitionTemporary);
        result.ParseableAdd("CursorPos", CellCollection.KeyOfCell(CursorPosColumn?.Column, CursorPosRow?.Row));
        return result.FinishParseable();
    }

    internal static Renderer_Abstract RendererOf(ColumnViewItem columnViewItem, string style) {
        if (!string.IsNullOrEmpty(columnViewItem.Renderer)) {
            var renderer = ParsebleItem.NewByTypeName<Renderer_Abstract>(columnViewItem.Renderer);
            if (renderer == null) { return RendererOf(columnViewItem.Column, style); }

            renderer.Parse(columnViewItem.RendererSettings);

            return renderer;
        }

        return RendererOf(columnViewItem.Column, style);
    }

    internal static bool RepairColumnArrangements(Database db) {
        if (db.IsDisposed) { return false; }
        if (!string.IsNullOrEmpty(db.FreezedReason)) { return false; }

        var tcvc = ColumnViewCollection.ParseAll(db);

        for (var z = 0; z < Math.Max(2, tcvc.Count); z++) {
            if (tcvc.Count < z + 1) { tcvc.Add(new ColumnViewCollection(db, string.Empty)); }
            ColumnViewCollection.Repair(tcvc[z], z);
        }

        //var i = db.Column.IndexOf(tcvc[0][0].Column.KeyName);

        //if(i!= 0) {
        //    Develop.DebugPrint(FehlerArt.Warnung, "Spalte 0 nicht auf erster Position!");

        //    db.Column.RemoveAt

        //    Generic.Swap(db.Column[0], db.Column[i]);
        //}

        var n = tcvc.ToString(false);

        if (n != db.ColumnArrangements) {
            db.ColumnArrangements = n;
            return true;
        }

        return false;
    }

    internal void RowCleanUp() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        var l = new RowCleanUp(this);
        l.Show();
    }

    //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                Filter.PropertyChanged -= Filter_PropertyChanged;
                Filter.RowsChanged -= Filter_PropertyChanged;
                DatabaseSet(null, string.Empty); // Wichtig (nicht _Database) um Events zu lösen
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }
        base.DrawControl(gr, state);

        if (IsDisposed) { return; }

        _tmpCursorRect = Rectangle.Empty;

        if (_databaseDrawError is { } dt) {
            if (DateTime.UtcNow.Subtract(dt).TotalSeconds < 60) {
                DrawWaitScreen(gr, string.Empty, null);
                return;
            }
            _databaseDrawError = null;
        }

        // Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
        if (state.HasFlag(States.Standard_HasFocus)) {
            state ^= States.Standard_HasFocus;
        }

        if (Database is not { IsDisposed: false } db) {
            DrawWaitScreen(gr, "Keine Datenbank geladen.", null);
            return;
        }

        db.LastUsedDate = DateTime.UtcNow;

        if (db.ExecutingScript.Count > 0 && _rowsFilteredAndPinned is null) {
            DrawWaitScreen(gr, string.Empty, null);
            return;
        }

        if (DesignMode || ShowWaitScreen) {
            DrawWaitScreen(gr, string.Empty, null);
            return;
        }

        if (CurrentArrangement is not { IsDisposed: false } ca || ca.Count < 1) {
            DrawWaitScreen(gr, "Aktuelle Ansicht fehlerhaft", null);
            return;
        }

        if (!Filter.IsOk()) {
            DrawWaitScreen(gr, Filter.ErrorReason(), ca);
            return;
        }

        if (Filter.Database != null && Database != Filter.Database) {
            DrawWaitScreen(gr, "Filter fremder Datenbank: " + Filter.Database.Caption, ca);
            return;
        }

        List<RowData> sortedRowData = [.. RowsFilteredAndPinned()];

        if (state.HasFlag(States.Standard_Disabled)) { CursorPos_Reset(); }

        var displayRectangleWoSlider = DisplayRectangleWithoutSlider();

        // Haupt-Aufbau-Routine ------------------------------------

        var firstVisibleRow = sortedRowData.Count;
        var lastVisibleRow = -1;

        foreach (var thisRow in sortedRowData) {
            if (thisRow?.Row is { IsDisposed: false } r) {
                if (IsOnScreen(ca, thisRow, displayRectangleWoSlider)) {
                    var T = sortedRowData.IndexOf(thisRow);
                    firstVisibleRow = Math.Min(T, firstVisibleRow);
                    lastVisibleRow = Math.Max(T, lastVisibleRow);
                }
            }
        }

        #region Slider

        var lastC = ca[ca.Count - 1].RealHead(_zoom, 0);

        var maxY = 0;
        if (sortedRowData.Count > 0) {
            maxY = GetPix(sortedRowData[sortedRowData.Count - 1].DrawHeight + sortedRowData[sortedRowData.Count - 1].Y);
        }

        SliderY.Minimum = 0;
        SliderY.Maximum = Math.Max(maxY - displayRectangleWoSlider.Height + lastC.Bottom + GetPix(18), 0);
        SliderY.LargeChange = GetPix(displayRectangleWoSlider.Height - ca.HeadSize());
        SliderY.Enabled = SliderY.Maximum > 0;

        var maxX = 0;
        if (ca.Count > 1) {
            // Count größer 1! Weil wenns nur eine ist, diese als ganze breite angezeigt wird
            maxX = lastC.Right;
        }

        SliderX.Minimum = 0;
        SliderX.Maximum = maxX - displayRectangleWoSlider.Width + 1;
        SliderX.LargeChange = displayRectangleWoSlider.Width;
        SliderX.Enabled = SliderX.Maximum > 0;

        #endregion

        Draw_Table_Std(gr, sortedRowData, state, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, CurrentArrangement);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        if (Database is not { IsDisposed: false }) { return; }

        DoInputFilter(FilterOutput.Database, false);

        const string t = "Übergeordnetes Element";

        Filter.RemoveRange(t);

        if (FilterOutputType == Filterausgabe.Im_Element_Gewählte_Filter) {
            FilterOutput.ChangeTo(Filter);
        }

        Filter.RemoveOtherAndAdd(FilterInput, t);

        if (FilterOutputType == Filterausgabe.Alle_Filter) {
            FilterOutput.ChangeTo(Filter);
        }
    }

    protected override bool IsInputKey(Keys keyData) {
        // Ganz wichtig diese Routine!
        // Wenn diese NICHT ist, geht der Fokus weg, sobald der cursor gedrückt wird.
        switch (keyData) {
            case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                return true;

            default:
                return false;
        }
    }

    protected override void OnClick(System.EventArgs e) {
        base.OnClick(e);
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            if (_isinClick) { return; }
            _isinClick = true;

            (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, MousePos().X, MousePos().Y);
            _isinClick = false;
        }
    }

    protected void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    protected override void OnDoubleClick(System.EventArgs e) {
        //    base.OnDoubleClick(e); Wird komplett selbst gehandlet und das neue Ereigniss ausgelöst
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, MousePos().X, MousePos().Y);
            CellExtEventArgs ea = new(_mouseOverColumn, _mouseOverRow);
            DoubleClick?.Invoke(this, ea);

            if (!Mouse_IsInHead()) {
                var rc = RowCaptionOnCoordinate(MousePos().X, MousePos().Y);

                if (string.IsNullOrEmpty(rc)) {
                    Cell_Edit(ca, _mouseOverColumn, _mouseOverRow, true);
                }

                //if (_mouseOverRow != null || MousePos().Y <= ca.HeadSize() + 18) {
                //}
            }
            _isinDoubleClick = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        _controlPressing = e.Modifiers == Keys.Control;

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (CursorPosColumn?.Column is not { } c) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;

            Develop.SetUserDidSomething();

            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            switch (e.KeyCode) {
                case Keys.Oemcomma: // normales ,
                    if (e.Modifiers == Keys.Control) {
                        var lp = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true, null);
                        Neighbour(CursorPosColumn, CursorPosRow, Direction.Oben, out _, out var newRow);
                        if (newRow == CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                        if (string.IsNullOrEmpty(lp) && newRow?.Row != null) {
                            UserEdited(this, newRow.Row.CellGetString(c), CursorPosColumn, CursorPosRow, true);
                        } else {
                            NotEditableInfo(lp);
                        }
                    }
                    break;

                case Keys.X:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(c, CursorPosRow?.Row, true);
                        if (CursorPosRow?.Row is null || CursorPosRow.Row.CellIsNullOrEmpty(c)) {
                            _isinKeyDown = false;
                            return;
                        }
                        var l2 = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true, null);
                        if (string.IsNullOrEmpty(l2)) {
                            UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true);
                        } else {
                            NotEditableInfo(l2);
                        }
                    }
                    break;

                case Keys.Delete:
                    if (CursorPosRow?.Row is null || CursorPosRow.Row.CellIsNullOrEmpty(c)) {
                        _isinKeyDown = false;
                        return;
                    }
                    var l = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true, null);
                    if (string.IsNullOrEmpty(l)) {
                        UserEdited(this, string.Empty, CursorPosColumn, CursorPosRow, true);
                    } else {
                        NotEditableInfo(l);
                    }
                    break;

                case Keys.Left:
                    Cursor_Move(Direction.Links);
                    break;

                case Keys.Right:
                    Cursor_Move(Direction.Rechts);
                    break;

                case Keys.Up:
                    Cursor_Move(Direction.Oben);
                    break;

                case Keys.Down:
                    Cursor_Move(Direction.Unten);
                    break;

                case Keys.PageDown:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value += SliderY.LargeChange;
                    }
                    break;

                case Keys.PageUp: //Bildab
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value -= SliderY.LargeChange;
                    }
                    break;

                case Keys.Home:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value = SliderY.Minimum;
                    }
                    break;

                case Keys.End:
                    if (SliderY.Enabled) {
                        CursorPos_Reset();
                        SliderY.Value = SliderY.Maximum;
                    }
                    break;

                case Keys.C:
                    if (e.Modifiers == Keys.Control) {
                        CopyToClipboard(c, CursorPosRow?.Row, true);
                    }
                    break;

                case Keys.F:
                    if (e.Modifiers == Keys.Control) {
                        Search x = new(this);
                        x.Show();
                    }
                    break;

                case Keys.F2:
                    Cell_Edit(ca, CursorPosColumn, CursorPosRow, true);
                    break;

                case Keys.V:
                    if (e.Modifiers == Keys.Control) {
                        if (CursorPosColumn != null && CursorPosRow?.Row != null) {
                            if (!c.Function.TextboxEditPossible() && c.Function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                                NotEditableInfo("Die Zelle hat kein passendes Format.");
                                _isinKeyDown = false;
                                return;
                            }
                            if (!Clipboard.ContainsText()) {
                                NotEditableInfo("Kein Text in der Zwischenablage.");
                                _isinKeyDown = false;
                                return;
                            }
                            var ntxt = Clipboard.GetText();
                            if (CursorPosRow?.Row.CellGetString(c) == ntxt) {
                                _isinKeyDown = false;
                                return;
                            }
                            var l2 = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditAcut, true, false, true, null);
                            if (string.IsNullOrEmpty(l2)) {
                                UserEdited(this, ntxt, CursorPosColumn, CursorPosRow, true);
                            } else {
                                NotEditableInfo(l2);
                            }
                        }
                    }
                    break;
            }
            _isinKeyDown = false;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        _controlPressing = false;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e.X, e.Y);
            // Die beiden Befehle nur in Mouse Down!
            // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
            // Das könnte auch sehr Zeit intensiv sein. Dann kann die Maus inzwischen wo ander sein.
            // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
            // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus bewegt sich
            //          MouseUp  -> Cursor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
            if (_mouseOverRow == null) {
                var rc = RowCaptionOnCoordinate(e.X, e.Y);
                CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert
                _mouseOverColumn = null;
                _mouseOverRow = null;

                if (!string.IsNullOrEmpty(rc)) {
                    if (_collapsed.Contains(rc)) {
                        _ = _collapsed.Remove(rc);
                    } else {
                        _collapsed.Add(rc);
                    }
                    Invalidate_SortedRowData();
                }
            }
            _ = EnsureVisible(ca, _mouseOverColumn, _mouseOverRow);
            CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);
            Develop.SetUserDidSomething();
            _isinMouseDown = false;
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _controlPressing = false;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);

        lock (_lockUserAction) {
            if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
            if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

            if (_isinMouseMove) { return; }

            _isinMouseMove = true;

            (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e.X, e.Y);

            Develop.SetUserDidSomething();
            if (_mouseOverColumn?.Column is not { } c) { _isinMouseMove = false; return; }

            if (e.Button != MouseButtons.None) {
                //_database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            } else {
                if (e.Y < ca.HeadSize()) {
                    QuickInfo = QuickInfoText(c, string.Empty);
                } else if (_mouseOverRow != null) {
                    if (c.Function.NeedTargetDatabase()) {
                        if (c.LinkedDatabase != null) {
                            switch (c.Function) {
                                case ColumnFunction.Verknüpfung_zu_anderer_Datenbank:

                                    var (lcolumn, _, info, _) = CellCollection.LinkedCellData(c, _mouseOverRow?.Row, true, false);
                                    if (lcolumn != null) { QuickInfo = QuickInfoText(lcolumn, c.ReadableText() + " bei " + lcolumn.ReadableText() + ":"); }

                                    if (!string.IsNullOrEmpty(info) && db.IsAdministrator()) {
                                        if (string.IsNullOrEmpty(QuickInfo)) { QuickInfo += "\r\n"; }
                                        QuickInfo = "Verlinkungs-Status: " + info;
                                    }

                                    break;

                                case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                                    break;

                                default:
                                    Develop.DebugPrint(c.Function);
                                    break;
                            }
                        } else {
                            QuickInfo = "Verknüpfung zur Ziel-Datenbank fehlerhaft.";
                        }
                    } else if (db.IsAdministrator()) {
                        QuickInfo = Database.UndoText(_mouseOverColumn?.Column, _mouseOverRow?.Row);
                    }
                }
            }
            _isinMouseMove = false;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseUp(e);

        lock (_lockUserAction) {
            if (Database is not { IsDisposed: false } || CurrentArrangement is not { IsDisposed: false } ca) {
                return;
            }

            (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e.X, e.Y);
            // TXTBox_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Design.Form_KontextMenu);

            if (_mouseOverColumn?.Column is not { } column) { return; }

            if (e.Button == MouseButtons.Left) {
                if (Mouse_IsInAutofilter(_mouseOverColumn, e)) {
                    var screenX = Cursor.Position.X - e.X;
                    var screenY = Cursor.Position.Y - e.Y;
                    AutoFilter_Show(ca, _mouseOverColumn, screenX, screenY);
                    return;
                }

                if (Mouse_IsInRedcueButton(_mouseOverColumn, e)) {
                    _mouseOverColumn.Reduced = !_mouseOverColumn.Reduced;
                    Invalidate();
                    return;
                }

                if (_mouseOverRow?.Row is { IsDisposed: false } r) {
                    OnCellClicked(new CellEventArgs(column, r));
                    Invalidate();
                }
            }

            if (e.Button == MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, CellCollection.KeyOfCell(_mouseOverColumn?.Column, _mouseOverRow?.Row), e);
            }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        lock (_lockUserAction) {
            if (_isinMouseWheel) { return; }
            _isinMouseWheel = true;

            if (_controlPressing) {
                DoZoom(e.Delta > 0);
                _isinMouseWheel = false;
                return;
            }

            if (!SliderY.Visible) {
                _isinMouseWheel = false;
                return;
            }
            SliderY.DoMouseWheel(e);
            _isinMouseWheel = false;
        }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;

            Invalidate_CurrentArrangement();
            Invalidate_SortedRowData(); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
            //CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            _isinSizeChanged = false;
        }
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        lock (_lockUserAction) {
            if (_isinVisibleChanged) { return; }
            _isinVisibleChanged = true;
            //Database?.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            _isinVisibleChanged = false;
        }
    }

    private static void NotEditableInfo(string reason) => Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);

    private static void UserEdited(Table table, string newValue, ColumnViewItem? cellInThisDatabaseColumn, RowData? cellInThisDatabaseRow, bool formatWarnung) {
        var er = table.EditableErrorReason(cellInThisDatabaseColumn, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, false, true, table.Filter.ToArray());
        if (!string.IsNullOrEmpty(er)) { NotEditableInfo(er); return; }

        if (cellInThisDatabaseColumn?.Column is not { Function: not ColumnFunction.Virtuelle_Spalte }) { return; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        var contentHolderCellColumn = cellInThisDatabaseColumn.Column;
        var contentHolderCellRow = cellInThisDatabaseRow?.Row;
        if (contentHolderCellRow != null && contentHolderCellColumn.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(contentHolderCellColumn, contentHolderCellRow, true, true);
            if (contentHolderCellColumn == null || contentHolderCellRow == null) { return; } // Dummy prüfung
        }

        #endregion

        #region Format prüfen

        if (formatWarnung && !string.IsNullOrEmpty(newValue)) {
            if (!newValue.IsFormat(contentHolderCellColumn)) {
                if (MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    NotEditableInfo("Abbruch, da das erwartete Format nicht eingehalten wurde.");
                    return;
                }
            }
        }

        #endregion

        #region neue Zeile anlegen? (Das ist niemals in der ein LinkedCell-Datenbank)

        if (cellInThisDatabaseRow == null) {
            if (string.IsNullOrEmpty(newValue)) { return; }
            if (cellInThisDatabaseColumn.Column?.Database is not { IsDisposed: false } db) { return; }
            if (table?.Database?.Column.First() is not { } colfirst) { return; }

            var fc = new List<FilterItem>() { new FilterItem(colfirst, FilterType.Istgleich, newValue) };
            foreach (var thisFilter in table.Filter) {
                if (thisFilter.Column != null) {
                    fc.Add(thisFilter);
                }
            }

            var fe = table.EditableErrorReason(cellInThisDatabaseColumn, null, EditableErrorReasonType.EditCurrently, true, false, true, fc.ToArray());
            if (!string.IsNullOrEmpty(fe)) {
                NotEditableInfo(fe);
                return;
            }

            var newr = db.Row.GenerateAndAdd(fc.ToArray(), "Neue Zeile über Tabellen-Ansicht");

            if (!string.IsNullOrEmpty(newr.message)) {
                NotEditableInfo(newr.message);
            }

            var l = table.RowsFiltered;
            if (newr.newrow != null && !l.Contains(newr.newrow)) {
                if (MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                    table.PinAdd(newr.newrow);
                }
            }

            var sr = table.RowsFilteredAndPinned();
            if (newr.newrow != null) {
                var rd = sr.Get(newr.newrow);
                table.CursorPos_Set(table.View_ColumnFirst(), rd, true);
            }

            return;
        }

        #endregion

        newValue = contentHolderCellColumn.AutoCorrect(newValue, false);
        if (contentHolderCellRow != null) {
            if (newValue == contentHolderCellRow.CellGetString(contentHolderCellColumn)) { return; }

            var f = CellCollection.EditableErrorReason(contentHolderCellColumn, contentHolderCellRow, EditableErrorReasonType.EditCurrently, true, false, true, false, null);
            if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
            contentHolderCellRow.CellSet(contentHolderCellColumn, newValue, "Benutzerbearbeitung in Tabellenansicht");

            contentHolderCellRow.UpdateRow(true, true, "Nach Benutzereingabe");

            if (table.Database == cellInThisDatabaseColumn.Column?.Database) { table.CursorPos_Set(cellInThisDatabaseColumn, cellInThisDatabaseRow, false); }
        }
    }

    private void _Database_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        if (SortUsed() is { } rsd) {
            if (rsd.UsedForRowSort(e.Column) || e.Column == Database?.Column.SysChapter) {
                Invalidate_SortedRowData();
            }
        }

        if (e.Column.MultiLine) {
            if (CurrentArrangement is { IsDisposed: false } ca) {
                if (ca[e.Column] is { IsDisposed: false }) {
                    Invalidate_SortedRowData(); // Zeichenhöhe kann sich ändern...
                }

                //cv.Invalidate_ContentWidth(); // Kann auf sich selbst aufpassen
            }
        }

        Invalidate();
    }

    private void _Database_DatabaseLoaded(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        // Wird auch bei einem Reload ausgeführt.
        // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen.
        // Deswegen prüfen, ob wirklich alles gelöscht werden muss, oder weiter behalten werden kann.
        // Auf Nothing muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Datenbank auch änsdert....

        if (!string.IsNullOrEmpty(_storedView)) {
            ParseView(_storedView);
            _storedView = string.Empty;
        } else {
            ResetView();
        }

        Invalidate_FilterInput();
        Invalidate_RowsInput();
        Invalidate_SortedRowData(); // Neue Zeilen können nun erlaubt sein
        Invalidate_CurrentArrangement(); // Wegen der Spaltenbreite
        CheckView();
    }

    private void _database_Disposing(object sender, System.EventArgs e) => DatabaseSet(null, string.Empty);

    private void _Database_ProgressbarInfo(object sender, ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        if (e.Ends) {
            _pg?.Close();
            return;
        }
        if (e.Beginns) {
            _pg = Progressbar.Show(e.Name, e.Count);
            return;
        }
        _pg?.Update(e.Current);
    }

    private void _Database_Row_RowGotData(object sender, RowEventArgs e) {
        if (IsDisposed) { return; }
        Invalidate_SortedRowData();
    }

    private void _Database_SortParameterChanged(object sender, System.EventArgs e) => Invalidate_SortedRowData();

    private void _Database_StoreView(object sender, System.EventArgs e) =>
            //if (!string.IsNullOrEmpty(_StoredView)) { Develop.DebugPrint("Stored View nicht Empty!"); }
            _storedView = ViewToString();

    private void _Database_ViewChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate_CurrentArrangement();
        CursorPos_Set(CursorPosColumn, CursorPosRow, true);
    }

    private void AutoFilter_Close() {
        if (_autoFilter != null) {
            _autoFilter.FilterCommand -= AutoFilter_FilterCommand;
            _autoFilter?.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        switch (e.Command.ToLowerInvariant()) {
            case "":
                break;

            case "filter":
                Filter.RemoveOtherAndAdd(e.Filter);
                //Filter.Remove(e.Column);
                //Filter.Add(e.Filter);
                break;

            case "filterdelete":
                Filter.Remove(e.Column);
                break;

            case "doeinzigartig":
                Filter.Remove(e.Column);
                RowCollection.GetUniques(e.Column, RowsVisibleUnique(), out var einzigartig, out _);
                if (einzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                RowCollection.GetUniques(e.Column, RowsVisibleUnique(), out _, out var xNichtEinzigartig);
                if (xNichtEinzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, xNichtEinzigartig));
                    Notification.Show("Die aktuell <b>nicht</b> einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> einzigartig sind.", ImageCode.Trichter);
                }
                break;

            //case "dospaltenvergleich": {
            //        List<RowItem> ro = new();
            //        ro.AddRange(VisibleUniqueRows());

            //        ItemCollectionList ic = new();
            //        foreach (var thisColumnItem in e.Column.Database.Column) {
            //            if (thisColumnItem != null && thisColumnItem != e.Column) { ic.Add(thisColumnItem); }
            //        }
            //        ic.Sort();

            //        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, AddType.None, true);
            //        if (r == null || r.Count == 0) { return; }

            //        var c = e.Column.Database.Column[r[0]);

            //        List<string> d = new();
            //        foreach (var thisR in ro) {
            //            if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
            //        }
            //        if (d.Count > 0) {
            //            Filter.Add(new FilterItem(e.Column.Database.Column.First, FilterType.Istgleich_ODER_GroßKleinEgal, d));
            //            Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", ImageCode.Trichter);
            //        } else {
            //            Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> identisch sind.", ImageCode.Trichter);
            //        }
            //        break;
            //    }

            case "doclipboard": {
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd("\r\n");
                    Filter.Remove(e.Column);

                    var searchValue = new List<string>(clipTmp.SplitAndCutByCr()).SortedDistinctList();

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd("\r\n");
                    Filter.Remove(e.Column);

                    var searchValue = e.Column.Contents();//  db.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCrToList().SortedDistinctList();
                    searchValue.RemoveString(clipTmp.SplitAndCutByCrToList().SortedDistinctList(), false);

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.IstGleich_ODER | FilterType.GroßKleinEgal, searchValue));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Command: " + e.Command);
                break;
        }

        if (e.Filter?.Column is { IsDisposed: false } col) {
            col.AddSystemInfo("Filter Clicked", Generic.UserName);
        }

        OnAutoFilterClicked(new FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(ColumnViewCollection ca, ColumnViewItem columnviewitem, int screenx, int screeny) {
        if (columnviewitem.Column == null) { return; }
        if (!ca.ShowHead) { return; }
        if (!columnviewitem.AutoFilterSymbolPossible) { return; }
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (Filter.HasAlwaysFalse()) {
            MessageBox.Show("Ein Filter, der nie ein Ergebnis zurückgibt,\r\nverhindert aktuell Filterungen.", ImageCode.Information, "OK");
            return;
        }

        var t = string.Empty;
        foreach (var thisFilter in Filter) {
            if (thisFilter != null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Origin)) {
                t += "\r\n" + thisFilter.ReadableText();
            }
        }

        if (!string.IsNullOrEmpty(t)) {
            MessageBox.Show("<b>Diese(r) Filter wurde(n) automatisch gesetzt:</b>" + t, ImageCode.Information, "OK");
            return;
        }

        var headPos = columnviewitem.RealHead(_zoom, SliderX.Value);

        _autoFilter = new AutoFilter(columnviewitem.Column, Filter, PinnedRows, columnviewitem.DrawWidth(), columnviewitem.GetRenderer(SheetStyle));
        _autoFilter.Position_LocateToPosition(new Point(screenx + headPos.Left, screeny + headPos.Bottom));
        _autoFilter.Show();
        _autoFilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    private int Autofilter_Text(ColumnViewItem viewItem) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return 0; }
        if (viewItem.TmpIfFilterRemoved != null) { return (int)viewItem.TmpIfFilterRemoved; }
        using var fc = (FilterCollection)Filter.Clone("Autofilter_Text");
        fc.Remove(viewItem.Column);

        var ro = fc.Rows;
        var c = RowsFiltered.Count - ro.Count;
        viewItem.TmpIfFilterRemoved = c;
        return c;
    }

    /// <summary>
    /// Gibt die Anzahl der SICHTBAREN Zeilen zurück, die mehr angezeigt werden würden, wenn dieser Filter deaktiviert wäre.
    /// </summary>
    /// <returns></returns>
    private void BB_Enter(object sender, System.EventArgs e) {
        if (((TextBox)sender).MultiLine) { return; }
        CloseAllComponents();
    }

    private void BB_ESC(object sender, System.EventArgs e) {
        BTB.Tag = null;
        BTB.Visible = false;
        BCB.Tag = null;
        BCB.Visible = false;
        CloseAllComponents();
    }

    private void BB_LostFocus(object sender, System.EventArgs e) {
        if (FloatingForm.IsShowing(BTB) || FloatingForm.IsShowing(BCB)) { return; }
        CloseAllComponents();
    }

    private void BB_NeedDatabaseOfAdditinalSpecialChars(object sender, DatabaseFileGiveBackEventArgs e) => e.File = Database;

    private void BB_TAB(object sender, System.EventArgs e) => CloseAllComponents();

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        db.Edit(typeof(DatabaseHeadEditor));
    }

    private void Cell_Edit(ColumnViewCollection ca, ColumnViewItem? viewItem, RowData? cellInThisDatabaseRow, bool preverDropDown) {
        var f = EditableErrorReason(viewItem, cellInThisDatabaseRow, EditableErrorReasonType.EditCurrently, true, true, true, Filter.ToArray());
        if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }
        if (viewItem?.Column == null) { return; }// Klick ins Leere

        var contentHolderCellColumn = viewItem.Column;
        var contentHolderCellRow = cellInThisDatabaseRow?.Row;

        if (contentHolderCellColumn == null) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        if (viewItem.Column.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(contentHolderCellColumn, contentHolderCellRow, true, true);
        }

        if (contentHolderCellColumn == null) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, preverDropDown);

        switch (dia) {
            case EditTypeTable.Textfeld:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", Generic.UserName);
                _ = Cell_Edit_TextBox(ca, viewItem, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", Generic.UserName);
                _ = Cell_Edit_TextBox(ca, viewItem, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BCB, 20, 18);
                break;

            case EditTypeTable.Dropdown_Single:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", Generic.UserName);
                Cell_Edit_Dropdown(ca, viewItem, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow);
                break;

            case EditTypeTable.Farb_Auswahl_Dialog:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", Generic.UserName);
                if (viewItem.Column != contentHolderCellColumn || cellInThisDatabaseRow?.Row != contentHolderCellRow) {
                    NotEditableInfo("Verlinkte Zellen hier verboten.");
                    return;
                }
                Cell_Edit_Color(viewItem, cellInThisDatabaseRow);
                break;

            case EditTypeTable.Font_AuswahlDialog:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", Generic.UserName);
                Develop.DebugPrint_NichtImplementiert(false);
                //if (cellInThisDatabaseColumn != ContentHolderCellColumn || cellInThisDatabaseRow != ContentHolderCellRow)
                //{
                //    NotEditableInfo("Ziel-Spalte ist kein Textformat");
                //    return;
                //}
                //Cell_Edit_Font(cellInThisDatabaseColumn, cellInThisDatabaseRow);
                break;

            case EditTypeTable.WarnungNurFormular:
                NotEditableInfo("Dieser Zelltyp kann nur in einem Formular-Fenster bearbeitet werden");
                break;

            case EditTypeTable.None:
                break;

            default:
                Develop.DebugPrint(dia);
                NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                break;
        }
    }

    private void Cell_Edit_Color(ColumnViewItem viewItem, RowData? cellInThisDatabaseRow) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var colDia = new ColorDialog();

        if (cellInThisDatabaseRow?.Row is { IsDisposed: false } r) {
            colDia.Color = r.CellGetColor(viewItem.Column);
        }
        colDia.Tag = (List<object?>)[viewItem.Column, cellInThisDatabaseRow];
        List<int> colList = [];
        foreach (var thisRowItem in db.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.CellGetInteger(viewItem.Column) != 0) {
                    colList.Add(thisRowItem.CellGetColorBgr(viewItem.Column));
                }
            }
        }
        colList.Sort();
        colDia.CustomColors = colList.Distinct().ToArray();
        _ = colDia.ShowDialog();
        colDia.Dispose();

        UserEdited(this, Color.FromArgb(255, colDia.Color).ToArgb().ToString(), viewItem, cellInThisDatabaseRow, false);
    }

    private void Cell_Edit_Dropdown(ColumnViewCollection ca, ColumnViewItem viewItem, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
        if (viewItem.Column != contentHolderCellColumn) {
            if (contentHolderCellRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
            if (cellInThisDatabaseRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
        }

        var t = new List<AbstractListItem>();

        var r = viewItem.GetRenderer(SheetStyle);

        t.AddRange(ItemsOf(contentHolderCellColumn, contentHolderCellRow, 1000, r));
        if (t.Count == 0) {
            // Hm ... Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.TextBearbeitungErlaubt) { Cell_Edit(ca, viewItem, cellInThisDatabaseRow, false); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.TextBearbeitungErlaubt) {
            if (t.Count == 0 && (cellInThisDatabaseRow?.Row.CellIsNullOrEmpty(viewItem.Column) ?? true)) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(ca, viewItem, cellInThisDatabaseRow, false);
                return;
            }
            t.Add(ItemOf("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, FirstSortChar + "1"));
            t.Add(SeparatorWith(FirstSortChar + "2"));
        }

        List<string> toc = [];

        if (contentHolderCellRow != null) {
            toc.AddRange(contentHolderCellRow.CellGetList(contentHolderCellColumn));
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CheckBehavior.MultiSelection, toc, new CellExtEventArgs(viewItem, cellInThisDatabaseRow), this, Translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(ColumnViewCollection ca, ColumnViewItem viewItem, RowData? cellInThisDatabaseRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow, TextBox box, int addWith, int isHeight) {
        if (IsDisposed) { return false; }

        if (contentHolderCellColumn != viewItem.Column) {
            if (contentHolderCellRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
            if (cellInThisDatabaseRow == null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return false;
            }
        }

        var headPos = viewItem.RealHead(_zoom, SliderX.Value);

        if (contentHolderCellRow != null) {
            var h = cellInThisDatabaseRow?.DrawHeight ?? 18;// Row_DrawHeight(cellInThisDatabaseRow, DisplayRectangle);
            if (isHeight > 0) { h = isHeight; }
            box.Location = new Point(headPos.X, DrawY(ca, cellInThisDatabaseRow));
            box.Size = new Size(headPos.Width + addWith, GetPix(h));
            box.Text = contentHolderCellRow.CellGetString(contentHolderCellColumn);
        } else {
            // Neue Zeile...
            box.Location = new Point(headPos.X, headPos.Bottom);
            box.Size = new Size(headPos.Width + addWith, GetPix(18));
            box.Text = string.Empty;
        }

        box.GetStyleFrom(contentHolderCellColumn);

        box.Tag = (List<object?>)[viewItem, cellInThisDatabaseRow];

        if (box is ComboBox cbox) {
            cbox.ItemClear();
            cbox.ItemAddRange(ItemsOf(contentHolderCellColumn, contentHolderCellRow, 1000, viewItem.GetRenderer(SheetStyle)));
            if (cbox.ItemCount == 0) {
                return Cell_Edit_TextBox(ca, viewItem, cellInThisDatabaseRow, contentHolderCellColumn, contentHolderCellRow, BTB, 0, 0);
            }
        }

        box.Visible = true;
        box.BringToFront();
        _ = box.Focus();
        return true;
    }

    private (ColumnViewItem?, RowData?) CellOnCoordinate(ColumnViewCollection ca, int xpos, int ypos) => (ColumnOnCoordinate(ca, xpos), RowOnCoordinate(ca, ypos));

    private void CloseAllComponents() {
        if (InvokeRequired) {
            _ = Invoke(new Action(CloseAllComponents));
            return;
        }
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private void Column_ItemRemoving(object sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn?.Column) { CursorPos_Reset(); }
        if (e.Column == _mouseOverColumn?.Column) { _mouseOverColumn = null; }
    }

    private ColumnViewItem? ColumnOnCoordinate(ColumnViewCollection ca, int xpos) {
        if (ca.IsDisposed) { return null; }

        foreach (var thisViewItem in ca) {
            var headPos = thisViewItem.RealHead(_zoom, SliderX.Value);
            if (xpos >= headPos.Left && xpos <= headPos.Right) { return thisViewItem; }
        }

        return null;
    }

    private void Cursor_Move(Direction richtung) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        Neighbour(CursorPosColumn, CursorPosRow, richtung, out var newCol, out var newRow);
        CursorPos_Set(newCol, newRow, richtung != Direction.Nichts);
    }

    private void Database_InvalidateView(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    private Rectangle DisplayRectangleWithoutSlider() => new(DisplayRectangle.Left, DisplayRectangle.Left, DisplayRectangle.Width - SliderY.Width, DisplayRectangle.Height - SliderX.Height);

    private void Draw_Border(Graphics gr, ColumnViewItem column, Rectangle r, int yPos) {
        if (IsDisposed) { return; }

        for (var z = 0; z <= 1; z++) {
            int xPos;
            ColumnLineStyle lin;

            if (z == 0) {
                xPos = r.Left;
                lin = column.LineLeft;
            } else {
                xPos = r.Right;
                lin = column.LineRight;
            }

            switch (lin) {
                case ColumnLineStyle.Ohne:
                    break;

                case ColumnLineStyle.Dünn:
                    gr.DrawLine(Skin.PenLinieDünn, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.Kräftig:
                    gr.DrawLine(Skin.PenLinieKräftig, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.Dick:
                    gr.DrawLine(Skin.PenLinieDick, xPos, 0, xPos, yPos);
                    break;

                case ColumnLineStyle.ShadowRight:
                    var c = Skin.Color_Border(Design.Table_Lines_thick, States.Standard);
                    gr.DrawLine(Skin.PenLinieKräftig, xPos, 0, xPos, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), xPos + 1, 0, xPos + 1, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), xPos + 2, 0, xPos + 2, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), xPos + 3, 0, xPos + 3, yPos);
                    gr.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), xPos + 4, 0, xPos + 4, yPos);
                    break;

                default:
                    Develop.DebugPrint(lin);
                    break;
            }
        }
    }

    private void Draw_Column_Body(Graphics gr, ColumnViewItem cellInThisDatabaseColumn, Rectangle displayRectangleWoSlider, Rectangle r) {
        if (IsDisposed) { return; }

        //if (cellInThisDatabaseColumn.Column is not { IsDisposed: false } tmpc) { return; }

        gr.SmoothingMode = SmoothingMode.None;
        gr.FillRectangle(new SolidBrush(cellInThisDatabaseColumn.BackColor_ColumnCell), r.Left, r.Bottom, r.Width, displayRectangleWoSlider.Height - r.Bottom);
        Draw_Border(gr, cellInThisDatabaseColumn, r, displayRectangleWoSlider.Height);
    }

    private void Draw_Column_Cells(Graphics gr, IReadOnlyList<RowData> sr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, bool firstOnScreen, ColumnViewCollection ca, Rectangle r) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        if (viewItem.Column is not { IsDisposed: false } cellInThisDatabaseColumn) { return; }

        var isAdmin = db.IsAdministrator();
        var isCurrentThreadBackground = Thread.CurrentThread.IsBackground;

        var p16 = GetPix(16);
        var p14 = GetPix(14);
        var p2 = GetPix(2);
        var p1 = GetPix(1);
        var p23 = GetPix(23);
        var p28 = GetPix(28);
        var p5 = GetPix(5);
        var p6 = GetPix(6);
        var prc = GetPix(ca.RowChapterHeight);
        var chpF = ca.Font_RowChapter.Scale(_zoom);

        var drawWidth = r.Width - p2;
        var rowScript = db.CanDoValueChangedScript();

        if (SliderY.Value < p16 && UserEdit_NewRowAllowed()) {
            string txt;
            var plus = 0;
            QuickImage? qi;
            if (Database.Column.First() is { IsDisposed: false } columnFirst && cellInThisDatabaseColumn == columnFirst) {
                txt = "[Neue Zeile]";
                plus = p16;
                qi = QuickImage.Get(ImageCode.PlusZeichen, p14);
            } else {
                txt = FilterCollection.InitValue(cellInThisDatabaseColumn, false, Filter.ToArray());
                qi = QuickImage.Get(ImageCode.PlusZeichen, p14, Color.Transparent, Color.Transparent, 200);
            }

            if (!string.IsNullOrEmpty(txt)) {
                var pos = new Rectangle(r.Left + plus, (int)(-SliderY.Value + r.Bottom + p1), r.Width - plus, p16);
                gr.DrawImage(qi, new Point(r.Left + p1, (int)(-SliderY.Value + r.Bottom + p1)));
                viewItem.GetRenderer(SheetStyle).Draw(gr, txt, pos, cellInThisDatabaseColumn.DoOpticalTranslation, (Alignment)cellInThisDatabaseColumn.Align, _zoom);
            }
        }

        for (var currentRowNo = firstVisibleRow; currentRowNo <= lastVisibleRow; currentRowNo++) {
            var cellInThisDatabaseRowData = sr[currentRowNo];
            var cellInThisDatabaseRow = cellInThisDatabaseRowData.Row;
            gr.SmoothingMode = SmoothingMode.None;

            Rectangle cellrectangle = new(r.Left, DrawY(ca, cellInThisDatabaseRowData),
                r.Width, GetPix(cellInThisDatabaseRowData.DrawHeight));

            if (cellInThisDatabaseRowData.Expanded) {
                if (cellInThisDatabaseRowData.MarkYellow) { gr.FillRectangle(BrushYellowTransparent, cellrectangle); }

                if (isAdmin) {
                    if (rowScript) {
                        if (cellInThisDatabaseRow.NeedsRowUpdate(true, true)) {
                            gr.FillRectangle(BrushRedTransparent, cellrectangle);
                            if (RowCollection.FailedRows.Contains(cellInThisDatabaseRow)) {
                                gr.FillRectangle(BrushRedTransparent, cellrectangle);
                                gr.FillRectangle(BrushRedTransparent, cellrectangle);
                                gr.FillRectangle(BrushRedTransparent, cellrectangle);
                            } else {
                                RowCollection.WaitDelay = 0;
                            }
                        }
                    }
                }

                gr.DrawLine(Skin.PenLinieDünn, cellrectangle.Left, cellrectangle.Bottom - 1, cellrectangle.Right - 1, cellrectangle.Bottom - 1);

                if (!isCurrentThreadBackground && CursorPosRow == cellInThisDatabaseRowData) {
                    _tmpCursorRect = cellrectangle;
                    _tmpCursorRect.Height -= 1;

                    if (CursorPosColumn == viewItem) {
                        Draw_Cursor(gr, displayRectangleWoSlider, false);
                    }
                }

                #region Draw_CellTransparent

                if (cellInThisDatabaseColumn.Function == ColumnFunction.Virtuelle_Spalte) {
                    cellInThisDatabaseRow.CheckRowDataIfNeeded();
                }

                var toDrawd = cellInThisDatabaseRow.CellGetString(cellInThisDatabaseColumn);
                viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellrectangle, cellInThisDatabaseColumn.DoOpticalTranslation, (Alignment)cellInThisDatabaseColumn.Align, _zoom);

                #endregion

                if (_unterschiede != null && _unterschiede != cellInThisDatabaseRow) {
                    if (cellInThisDatabaseRow.CellGetString(cellInThisDatabaseColumn) != _unterschiede.CellGetString(cellInThisDatabaseColumn)) {
                        Rectangle tmpr = new(r.Left + 1, DrawY(ca, cellInThisDatabaseRowData) + 1, drawWidth, cellInThisDatabaseRowData.DrawHeight - 2);
                        gr.DrawRectangle(PenRed1, tmpr);
                    }
                }
            }

            if (firstOnScreen) {
                // Überschrift in der ersten Spalte zeichnen
                cellInThisDatabaseRowData.CaptionPos = Rectangle.Empty;
                if (cellInThisDatabaseRowData.ShowCap) {
                    var si = chpF.MeasureString(cellInThisDatabaseRowData.RowChapter);
                    gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(50)), 1, DrawY(ca, cellInThisDatabaseRowData) - prc, displayRectangleWoSlider.Width - 2, prc);
                    cellInThisDatabaseRowData.CaptionPos = new Rectangle(1, DrawY(ca, cellInThisDatabaseRowData) - prc, (int)si.Width + p28, (int)si.Height);

                    if (_collapsed.Contains(cellInThisDatabaseRowData.RowChapter)) {
                        var x = new ExtText(Design.Button_CheckBox, States.Checked);
                        Button.DrawButton(this, gr, Design.Button_CheckBox, States.Checked, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, cellInThisDatabaseRowData.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Unten_Scrollbar|" + p14 + "|||FF0000||200|200"), p5, DrawY(ca, cellInThisDatabaseRowData) - prc + p6);
                    } else {
                        var x = new ExtText(Design.Button_CheckBox, States.Standard);
                        Button.DrawButton(this, gr, Design.Button_CheckBox, States.Standard, null, Alignment.Horizontal_Vertical_Center, false, x, string.Empty, cellInThisDatabaseRowData.CaptionPos, false);
                        gr.DrawImage(QuickImage.Get("Pfeil_Rechts_Scrollbar|" + p14 + "|||||0"), p5, DrawY(ca, cellInThisDatabaseRowData) - prc + p6);
                    }
                    chpF.DrawString(gr, cellInThisDatabaseRowData.RowChapter, p23, DrawY(ca, cellInThisDatabaseRowData) - prc);
                    gr.DrawLine(Skin.PenLinieDick, 0, DrawY(ca, cellInThisDatabaseRowData), displayRectangleWoSlider.Width, DrawY(ca, cellInThisDatabaseRowData));
                }
            }
        }
    }

    private void Draw_Column_Head(Graphics gr, ColumnViewItem viewItem, Rectangle displayRectangleWoSlider, int lfdNo, ColumnViewCollection ca, Rectangle r) {
        if (IsDisposed) { return; }
        if (!ca.ShowHead) { return; }

        if (!IsOnScreen(r, displayRectangleWoSlider)) { return; }

        //        gr.FillRectangle(new SolidBrush(viewItem.BackColor_ColumnHead), r);
        Draw_Border(gr, viewItem, r, r.Bottom);
        gr.FillRectangle(new SolidBrush(viewItem.BackColor_ColumnHead), r);

        var down = 0;
        if (!string.IsNullOrEmpty(viewItem.CaptionGroup3)) {
            down = GetPix(ColumnCaptionSizeY * 3);
        } else if (!string.IsNullOrEmpty(viewItem.CaptionGroup2)) {
            down = GetPix(ColumnCaptionSizeY * 2);
        } else if (!string.IsNullOrEmpty(viewItem.CaptionGroup1)) {
            down = GetPix(ColumnCaptionSizeY);
        }

        #region Recude-Button zeichnen

        if (viewItem.DrawWidth() > 70 || viewItem.Reduced) {
            var reducebuttonlocation = viewItem.ReduceButtonLocation(_zoom, SliderX.Value);

            //viewItem.ReduceLocation = new Rectangle(r.Width - x18, down, x18, x18);

            gr.DrawImage(viewItem.Reduced ? QuickImage.Get("Pfeil_Rechts|" + reducebuttonlocation.Width + "|||FF0000|||||20")
                                         : QuickImage.Get("Pfeil_Links|" + reducebuttonlocation.Width + "||||||||75"), reducebuttonlocation.Left, reducebuttonlocation.Top);
        }

        #endregion

        #region Trichter-Text && trichterState

        var trichterText = string.Empty;
        var trichterState = States.Undefiniert;
        var fi = Filter[viewItem.Column];

        if (viewItem.AutoFilterSymbolPossible) {
            if (fi != null) {
                trichterState = States.Checked;
                var anz = Autofilter_Text(viewItem);
                trichterText = anz > -100 ? (anz * -1).ToString() : "∞";
            } else {
                trichterState = States.Standard;
            }
        }

        #endregion

        #region Roten Rand für Split-Spalten

        if (viewItem.Column is { } c && c == viewItem.Column?.Database?.Column.SplitColumn) {
            var t = r;
            t.Inflate(-3, -3);
            gr.DrawRectangle(new Pen(Color.Red, 6), t);
            Draw_Border(gr, viewItem, r, r.Bottom);
            trichterText = string.Empty;
        }

        #endregion

        #region Filter-Knopf mit Trichter

        QuickImage? trichterIcon = null;

        var autofilter = viewItem.AutoFilterLocation(_zoom, SliderX.Value);

        var p2 = GetPix(2);
        var p4 = GetPix(4);
        var p5 = GetPix(5);
        var p6 = GetPix(6);
        var p11 = GetPix(11);
        var paf = GetPix(ColumnViewItem.AutoFilterSize);

        var pts = GetPix(ColumnViewItem.AutoFilterSize - 4);

        if (Filter.HasAlwaysFalse() && viewItem.AutoFilterSymbolPossible) {
            trichterIcon = QuickImage.Get("Trichter|" + pts + "|||FF0000||170");
        } else if (fi != null) {
            trichterIcon = QuickImage.Get("Trichter|" + pts + "|||FF0000");
        } else if (Filter.MayHaveRowFilter(viewItem.Column)) {
            trichterIcon = QuickImage.Get("Trichter|" + pts + "|||227722");
        } else if (viewItem.AutoFilterSymbolPossible) {
            trichterIcon = QuickImage.Get("Trichter|" + pts);
        }

        if (trichterState != States.Undefiniert) {
            Skin.Draw_Back(gr, Design.Button_AutoFilter, trichterState, autofilter, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, trichterState, autofilter);
        }

        if (trichterIcon != null) {
            gr.DrawImage(trichterIcon, autofilter.Left + p2, autofilter.Top + p2);
        }

        if (!string.IsNullOrEmpty(trichterText)) {
            var s = viewItem.Font_TextInFilter.Scale(_zoom).MeasureString(trichterText, StringFormat.GenericDefault);

            viewItem.Font_TextInFilter.Scale(_zoom).DrawString(gr, trichterText,
                autofilter.Left + ((paf - s.Width) / 2),
                autofilter.Top + ((paf - s.Height) / 2));
        }

        #endregion Filter-Knopf mit Trichter

        #region LaufendeNummer

        if (_showNumber) {
            viewItem.Font_Numbers.Scale(_zoom).DrawString(gr, "#" + lfdNo, r.X, autofilter.Top);
        }

        #endregion LaufendeNummer

        var tx = viewItem.Caption;
        tx = LanguageTool.DoTranslate(tx, Translate).Replace("\r", "\r\n");
        var fs = viewItem.Font_Head_Default.Scale(_zoom).MeasureString(tx);

        if (viewItem.CaptionBitmap is { IsError: false } cb) {

            #region Spalte mit Bild zeichnen

            Point pos = new(r.X + (int)((r.Width - fs.Width) / 2.0), 3 + down);
            gr.DrawImageInRectAspectRatio(cb, r.X + 2, (int)(pos.Y + fs.Height), r.Width - 4, r.Bottom - (int)(pos.Y + fs.Height) - 6 - 18);
            // Dann der Text
            gr.TranslateTransform(pos.X, pos.Y);
            viewItem.Font_Head_Colored.Scale(_zoom).DrawString(gr, tx, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);

            #endregion
        } else {

            #region Spalte ohne Bild zeichnen

            Point pos = new(r.X + (int)((r.Width - fs.Height) / 2.0), r.Bottom - p4 - paf);
            gr.TranslateTransform(pos.X, pos.Y);
            gr.RotateTransform(-90);
            viewItem.Font_Head_Colored.Scale(_zoom).DrawString(gr, tx, 0, 0);
            gr.TranslateTransform(-pos.X, -pos.Y);
            gr.ResetTransform();

            #endregion
        }

        #region Sortierrichtung Zeichnen

        var tmpSortDefinition = SortUsed();
        if (tmpSortDefinition != null && (tmpSortDefinition.UsedForRowSort(viewItem.Column) || viewItem.Column == Database?.Column.SysChapter)) {
            gr.DrawImage(tmpSortDefinition.Reverse ? QuickImage.Get("ZA|" + p11 + "|" + p5 + "||||50") : QuickImage.Get("AZ|" + p11 + "|" + p5 + "||||50"),
                (float)(r.X + (r.Width / 2.0) - p6),
                r.Bottom - p6 - paf);
        }

        #endregion
    }

    /// <summary>
    /// Zeichnet die Überschriften
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="ca"></param>
    /// <param name="displayRectangleWoSlider"></param>
    /// <param name="columnno"></param>
    private void Draw_Column_Head_Captions(Graphics gr, ColumnViewCollection ca, Rectangle displayRectangleWoSlider, int columnno) {
        if (IsDisposed) { return; }

        ColumnViewItem? prevViewItemWithOtherCaption = null;
        ColumnViewItem? prevViewItem = null;
        var prevCaptionGroup = string.Empty;

        var pcch = GetPix(ColumnCaptionSizeY);
        var pccy = GetPix(columnno * ColumnCaptionSizeY);

        foreach (var thisViewItem in ca) {
            var thisItem = thisViewItem.RealHead(_zoom, SliderX.Value);

            if (IsOnScreen(thisItem, displayRectangleWoSlider)) {
                var newCaptionGroup = thisViewItem.Column?.CaptionGroup(columnno) ?? string.Empty;

                if (newCaptionGroup != prevCaptionGroup) {

                    #region Ende einer Gruppierung gefunden

                    if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { } && prevViewItemWithOtherCaption is { }) {
                        Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, thisItem, displayRectangleWoSlider, pccy, pcch, prevCaptionGroup);
                    }

                    prevViewItemWithOtherCaption = thisViewItem;

                    #endregion
                }

                prevViewItem = thisViewItem;
                prevCaptionGroup = newCaptionGroup;
            }
        }

        // Zeichen-Routine für das letzte Element aufrufen
        if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { } && prevViewItemWithOtherCaption is { }) {
            Draw_Column_Head_Captions_Now(gr, prevViewItemWithOtherCaption, Rectangle.Empty, displayRectangleWoSlider, pccy, pcch, prevCaptionGroup);
        }
    }

    private void Draw_Column_Head_Captions_Now(Graphics gr, ColumnViewItem prevViewItemWithOtherCaption, Rectangle thisItem, Rectangle displayRectangleWoSlider, int pccy, int pcch, string prevCaptionGroup) {
        var le = Math.Max(prevViewItemWithOtherCaption.RealHead(_zoom, SliderX.Value).Left, 0);
        var re = thisItem.Left > 0 ? Math.Min(thisItem.Left, displayRectangleWoSlider.Width) : displayRectangleWoSlider.Width;

        if (le < re) {
            Rectangle r = new(le, pccy, re - le, pcch);
            gr.FillRectangle(new SolidBrush(prevViewItemWithOtherCaption.BackColor_ColumnHead), r);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
            gr.DrawRectangle(Skin.PenLinieKräftig, r);
            Skin.Draw_FormatedText(gr, prevCaptionGroup, null, Alignment.Horizontal_Vertical_Center, r, this, false, prevViewItemWithOtherCaption.Font_Head_Default.Scale(_zoom), Translate);
        }
    }

    private void Draw_Cursor(Graphics gr, Rectangle displayRectangleWoSlider, bool onlyCursorLines) {
        if (_tmpCursorRect.Width < 1) { return; }

        var stat = States.Standard;
        if (Focused()) { stat = States.Standard_HasFocus; }

        if (onlyCursorLines) {
            Pen pen = new(Skin.Color_Border(Design.Table_Cursor, stat).SetAlpha(180));
            gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top - 1, displayRectangleWoSlider.Width + 2, _tmpCursorRect.Height + 1));
        } else {
            Skin.Draw_Back(gr, Design.Table_Cursor, stat, _tmpCursorRect, this, false);
            Skin.Draw_Border(gr, Design.Table_Cursor, stat, _tmpCursorRect);
        }
    }

    private void Draw_Table_Std(Graphics gr, List<RowData> sr, States state, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, ColumnViewCollection? ca) {
        try {
            if (IsDisposed || Database is not { IsDisposed: false } db || ca == null) { return; }   // Kommt vor, dass spontan doch geparsed wird...
            Skin.Draw_Back(gr, Design.Table_And_Pad, state, base.DisplayRectangle, this, true);

            //// Maximale Rechten Pixel der Permanenten Columns ermitteln
            //var permaX = 0;
            //foreach (var viewItem in ca) {
            //    if (viewItem is { Column: not null, ViewType: ViewType.PermanentColumn }) {
            //        //if (viewItem._drawWidth == null) {
            //        //    // Veränderte Werte!
            //        //    DrawControl(gr, state);
            //        //    return;
            //        //}

            //        permaX = Math.Max(permaX, viewItem.X_WithSlider ?? 0 + viewItem.DrawWidth());
            //    }
            //}

            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnBackBody, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.Cells, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnBackBody, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.Cells, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);
            // Den CursorLines zeichnen
            Draw_Cursor(gr, displayRectangleWoSlider, true);
            Draw_Table_What(gr, sr, TableDrawColumn.NonPermament, TableDrawType.ColumnHead, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);
            Draw_Table_What(gr, sr, TableDrawColumn.Permament, TableDrawType.ColumnHead, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, ca);

            // Überschriften 1-3 Zeichnen
            Draw_Column_Head_Captions(gr, ca, displayRectangleWoSlider, 0);
            Draw_Column_Head_Captions(gr, ca, displayRectangleWoSlider, 1);
            Draw_Column_Head_Captions(gr, ca, displayRectangleWoSlider, 2);
            Skin.Draw_Border(gr, Design.Table_And_Pad, state, displayRectangleWoSlider);

            ////gr.Clear(Color.White);
            //gr.DrawString("Test " + Constants.GlobalRND.Next().ToString(false), new Font("Arial", 20), Brushes.Black, new Point(0, 0));

            //return;

            //if (db.HasPendingChanges && !string.IsNullOrEmpty(db.Filename)) { gr.DrawImage(QuickImage.Get(ImageCode.Stift, 16), 16, 8); }
            if (db.ReadOnly) {
                gr.DrawImage(QuickImage.Get(ImageCode.Schloss, 32), 16, 8);
                if (!string.IsNullOrEmpty(db.FreezedReason)) {
                    ca.Font_RowChapter.DrawString(gr, db.FreezedReason, 52, 12);
                }
            }

            if (db.IsAdministrator() && !db.ReadOnly) {
                if (!string.IsNullOrEmpty(db.ScriptNeedFix)) {
                    gr.DrawImage(QuickImage.Get(ImageCode.Kritisch, 64), 16, 8);
                    ca.Font_RowChapter.DrawString(gr, "Skripte müssen repariert werden", 90, 12);
                } else {
                    foreach (var thisColumnItem in db.Column) {
                        if (thisColumnItem.LinkedDatabase is { } dbl && !string.IsNullOrEmpty(dbl.ScriptNeedFix)) {
                            gr.DrawImage(QuickImage.Get(ImageCode.Kritisch, 64), 16, 8);
                            ca.Font_RowChapter.DrawString(gr, $"Skripte von {dbl.Caption} müssen repariert werden", 90, 12);
                        }
                    }
                }
            }

            if (db.AmITemporaryMaster(5, 55, null)) {
                gr.DrawImage(QuickImage.Get(ImageCode.Stern, 8), 0, 0);
            } else if (db.AmITemporaryMaster(0, 55, null)) {
                gr.DrawImage(QuickImage.Get(ImageCode.Stern, 8, Color.Blue, Color.Transparent), 0, 0);
            }
        } catch {
            Invalidate();
            //Develop.DebugPrint(ex);
        }
    }

    private void Draw_Table_What(Graphics gr, List<RowData> sr, TableDrawColumn col, TableDrawType type, Rectangle displayRectangleWoSlider, int firstVisibleRow, int lastVisibleRow, ColumnViewCollection ca) {
        if (IsDisposed) { return; }

        var lfdno = 0;

        var firstOnScreen = true;

        foreach (var viewItem in ca) {
            var r = viewItem.RealHead(_zoom, SliderX.Value);

            lfdno++;
            if (IsOnScreen(r, displayRectangleWoSlider)) {
                if ((col == TableDrawColumn.NonPermament && viewItem.ViewType != ViewType.PermanentColumn && r.Right > GetPix(ca.WiederHolungsSpaltenWidth)) ||
                    (col == TableDrawColumn.Permament && viewItem.ViewType == ViewType.PermanentColumn)) {
                    switch (type) {
                        case TableDrawType.ColumnBackBody:
                            Draw_Column_Body(gr, viewItem, displayRectangleWoSlider, r);
                            break;

                        case TableDrawType.Cells:
                            Draw_Column_Cells(gr, sr, viewItem, displayRectangleWoSlider, firstVisibleRow, lastVisibleRow, firstOnScreen, ca, r);
                            break;

                        case TableDrawType.ColumnHead:
                            Draw_Column_Head(gr, viewItem, displayRectangleWoSlider, lfdno, ca, r);
                            break;
                    }
                }
                firstOnScreen = false;
            }
        }
    }

    private void DrawWaitScreen(Graphics gr, string info, ColumnViewCollection? ca) {
        if (SliderX != null) { SliderX.Enabled = false; }
        if (SliderY != null) { SliderY.Enabled = false; }

        Skin.Draw_Back(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle, this, true);

        var i = QuickImage.Get(ImageCode.Uhr, 64);
        gr.DrawImage(i, (Width - 64) / 2, (Height - 64) / 2);

        var fa = ca?.Font_RowChapter ?? BlueFont.DefaultFont;

        fa.DrawString(gr, info, 12, 12);

        Skin.Draw_Border(gr, Design.Table_And_Pad, States.Standard_Disabled, base.DisplayRectangle);
    }

    private int DrawY(ColumnViewCollection ca, RowData? r) => r == null ? 0 : (int)(GetPix(r.Y + ca.HeadSize()) - SliderY.Value);

    /// <summary>
    /// Berechent die Y-Position auf dem aktuellen Controll
    /// </summary>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        CellExtEventArgs? ck = null;
        if (e.HotItem is CellExtEventArgs tmp) { ck = tmp; }

        if (ck?.ColumnView?.Column is not { } c) { return; }

        var toAdd = e.Item.KeyName;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ca, ck.ColumnView, ck.RowData, false);
            return;
        }
        if (ck.RowData?.Row is not { } r) {
            // Neue Zeile!
            UserEdited(this, toAdd, ck.ColumnView, null, false);
            return;
        }

        if (c.MultiLine) {
            var li = r.CellGetList(c);
            if (li.Contains(toAdd, false)) {
                // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                if (li.Count > -1 || c.DropdownAllesAbwählenErlaubt) {
                    toRemove = toAdd;
                    toAdd = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(toRemove)) { li.RemoveString(toRemove, false); }
            if (!string.IsNullOrEmpty(toAdd)) { li.Add(toAdd); }
            UserEdited(this, li.JoinWithCr(), ck.ColumnView, ck.RowData, false);
        } else {
            if (c.DropdownAllesAbwählenErlaubt) {
                if (toAdd == ck.RowData.Row.CellGetString(c)) {
                    UserEdited(this, string.Empty, ck.ColumnView, ck.RowData, false);
                    return;
                }
            }
            UserEdited(this, toAdd, ck.ColumnView, ck.RowData, false);
        }
    }

    private bool EnsureVisible(ColumnViewCollection ca, RowData? rowdata) {
        if (rowdata?.Row is not { IsDisposed: false }) { return false; }
        var dispR = DisplayRectangleWithoutSlider();

        if (DrawY(ca, rowdata) < GetPix(ca.HeadSize())) {
            SliderY.Value = SliderY.Value + DrawY(ca, rowdata) - GetPix(ca.HeadSize());
        } else if (DrawY(ca, rowdata) + rowdata.DrawHeight > dispR.Height) {
            SliderY.Value = SliderY.Value + DrawY(ca, rowdata) + GetPix(rowdata.DrawHeight) - dispR.Height;
        }
        return true;
    }

    private bool EnsureVisible(ColumnViewItem? viewItem) {
        if (IsDisposed) { return false; }
        if (viewItem?.Column is not { IsDisposed: false }) { return false; }
        var dispR = DisplayRectangleWithoutSlider();

        if (CurrentArrangement is not { IsDisposed: false } ca) { return false; }

        var r = viewItem.RealHead(_zoom, SliderX.Value);

        if (viewItem.ViewType == ViewType.PermanentColumn) {
            if (r.Right <= dispR.Width) { return true; }
            return false;
        }

        if (r.Left < GetPix(ca.WiederHolungsSpaltenWidth)) {
            SliderX.Value = SliderX.Value + r.X - GetPix(ca.WiederHolungsSpaltenWidth);
        } else if (r.Right > dispR.Width) {
            SliderX.Value = SliderX.Value + r.Right - dispR.Width;
        }
        return true;
    }

    private void Filter_PropertyChanged(object sender, System.EventArgs e) {
        //if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        if (CurrentArrangement is not { } ca) { return; }

        foreach (var thisColumn in ca) {
            thisColumn.TmpIfFilterRemoved = null;
        }

        Invalidate_SortedRowData();
        OnFilterChanged();
    }

    private int GetPix(int pix) => (int)((pix * _zoom) + 0.5);

    private void Invalidate_CurrentArrangement() {
        _currentArrangement = null;
        Invalidate();
    }

    private void Invalidate_SortedRowData() {
        _rowsFilteredAndPinned = null;
        Invalidate();
    }

    private bool IsOnScreen(Rectangle head, Rectangle displayRectangleWoSlider) => !IsDisposed && head.Right > 0 && head.Left <= displayRectangleWoSlider.Width;

    private bool IsOnScreen(ColumnViewCollection ca, ColumnViewItem? viewItem, RowData? row, Rectangle displayRectangleWoSlider) {
        if (viewItem?.Column == null) { return false; }
        var r = viewItem.RealHead(_zoom, SliderX.Value);

        return IsOnScreen(r, displayRectangleWoSlider) && IsOnScreen(ca, row, displayRectangleWoSlider);
    }

    private bool IsOnScreen(ColumnViewCollection ca, RowData? vrow, Rectangle displayRectangleWoSlider) => vrow != null && DrawY(ca, vrow) + vrow.DrawHeight >= GetPix(ca.HeadSize()) && DrawY(ca, vrow) <= displayRectangleWoSlider.Height;

    private bool Mouse_IsInAutofilter(ColumnViewItem viewItem, MouseEventArgs e) => viewItem.AutoFilterLocation(_zoom, SliderX.Value).Contains(e.Location);

    private bool Mouse_IsInRedcueButton(ColumnViewItem viewItem, MouseEventArgs e) => viewItem.ReduceButtonLocation(_zoom, SliderX.Value).Contains(e.Location);

    private void Neighbour(ColumnViewItem? column, RowData? row, Direction direction, out ColumnViewItem? newColumn, out RowData? newRow) {
        if (CurrentArrangement is not { IsDisposed: false } ca) {
            newColumn = null;
            newRow = null;
            return;
        }

        newColumn = column;
        newRow = row;

        if (newColumn != null) {
            if (direction.HasFlag(Direction.Links)) {
                if (ca.PreviousVisible(newColumn) != null) {
                    newColumn = ca.PreviousVisible(newColumn);
                }
            }
            if (direction.HasFlag(Direction.Rechts)) {
                if (ca.NextVisible(newColumn) != null) {
                    newColumn = ca.NextVisible(newColumn);
                }
            }
        }

        if (newRow != null) {
            if (direction.HasFlag(Direction.Oben)) {
                if (View_PreviousRow(newRow) != null) { newRow = View_PreviousRow(newRow); }
            }
            if (direction.HasFlag(Direction.Unten)) {
                if (View_NextRow(newRow) != null) { newRow = View_NextRow(newRow); }
            }
        }
    }

    private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

    private void OnBlockEdit(CellEditBlockReasonEventArgs ed) => BlockEdit?.Invoke(this, ed);

    private void OnCellClicked(CellEventArgs e) => CellClicked?.Invoke(this, e);

    private void OnDatabaseChanged() => DatabaseChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnFilterChanged() => FilterChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnPinnedChanged() => PinnedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowNullableEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnViewChanged() {
        Invalidate_CurrentArrangement();
        Invalidate_SortedRowData(); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    private void ParseView(string toParse) {
        ResetView();

        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "arrangement":
                        Arrangement = pair.Value.FromNonCritical();
                        break;

                    case "arrangementnr":
                        break;

                    case "filters":
                        Filter.PropertyChanged -= Filter_PropertyChanged;
                        Filter.RowsChanged -= Filter_PropertyChanged;
                        Filter.Database = Database;
                        var code = pair.Value.FromNonCritical();
                        Filter.Clear();
                        Filter.Parse(code);
                        Filter.ParseFinished(code);
                        Filter.PropertyChanged += Filter_PropertyChanged;
                        Filter.RowsChanged += Filter_PropertyChanged;
                        Filter_PropertyChanged(this, System.EventArgs.Empty);
                        break;

                    case "sliderx":
                        SliderX.Maximum = Math.Max(SliderX.Maximum, IntParse(pair.Value));
                        SliderX.Value = IntParse(pair.Value);
                        break;

                    case "slidery":
                        SliderY.Maximum = Math.Max(SliderY.Maximum, IntParse(pair.Value));
                        SliderY.Value = IntParse(pair.Value);
                        break;

                    case "cursorpos":
                        db.Cell.DataOfCellKey(pair.Value.FromNonCritical(), out var column, out var row);
                        CursorPos_Set(CurrentArrangement?[column], RowsFilteredAndPinned().Get(row), false);
                        break;

                    case "tempsort":
                        _sortDefinitionTemporary = new RowSortDefinition(Database, pair.Value.FromNonCritical());
                        break;

                    case "pin":
                        foreach (var thisk in pair.Value.FromNonCritical().SplitBy("|")) {
                            var r = db.Row.SearchByKey(thisk);
                            if (r is { IsDisposed: false }) { PinnedRows.Add(r); }
                        }

                        break;

                    case "collapsed":
                        _collapsed.AddRange(pair.Value.FromNonCritical().SplitBy("|"));
                        break;

                    case "reduced":
                        var cols = pair.Value.FromNonCritical().SplitBy("|");
                        CurrentArrangement?.Reduce(cols);
                        break;

                    case "zoom":
                        Zoom = FloatParse(pair.Value.FromNonCritical());
                        break;
                }
            }
        }

        CheckView();
    }

    private void Row_RowRemoved(object sender, RowEventArgs e) {
        Invalidate_CurrentArrangement(); // Wegen der Spaltenbreite
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (IsDisposed) { return; }
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (e.Row == _mouseOverRow?.Row) { _mouseOverRow = null; }
        if (PinnedRows.Contains(e.Row)) {
            _ = PinnedRows.Remove(e.Row);
            Invalidate_SortedRowData();
        }
    }

    private string RowCaptionOnCoordinate(int pixelX, int pixelY) {
        try {
            var s = RowsFilteredAndPinned();
            if (s == null) { return string.Empty; }
            foreach (var thisRow in s) {
                if (thisRow is { ShowCap: true, CaptionPos: var r }) {
                    if (r.Contains(pixelX, pixelY)) { return thisRow.RowChapter; }
                }
            }
        } catch { }
        return string.Empty;
    }

    private RowData? RowOnCoordinate(ColumnViewCollection ca, int pixelY) {
        if (IsDisposed || Database is not { IsDisposed: false } || pixelY <= ca.HeadSize() * _zoom) { return null; }
        var s = RowsFilteredAndPinned();

        return s?.FirstOrDefault(thisRowItem => thisRowItem != null && pixelY >= DrawY(ca, thisRowItem) && pixelY <= DrawY(ca, thisRowItem) + GetPix(thisRowItem.DrawHeight) && thisRowItem.Expanded);
    }

    private void SliderX_ValueChanged(object sender, System.EventArgs e) {
        CloseAllComponents();
        Invalidate();
    }

    private void SliderY_ValueChanged(object sender, System.EventArgs e) {
        CloseAllComponents();
        Invalidate();
    }

    private RowSortDefinition? SortUsed() => _sortDefinitionTemporary?.Columns != null
        ? _sortDefinitionTemporary
        : Database?.SortDefinition?.Columns != null ? Database.SortDefinition : null;

    private void TXTBox_Close(TextBox? textbox) {
        if (IsDisposed || textbox == null || Database is not { IsDisposed: false }) { return; }
        if (!textbox.Visible) { return; }
        if (textbox.Tag is not List<object?> { Count: >= 2 } tags) {
            textbox.Visible = false;
            return; // Ohne dem hier wird ganz am Anfang ein Ereignis ausgelöst
        }
        var w = textbox.Text;

        ColumnViewItem? column = null;
        RowData? row = null;

        if (tags[0] is ColumnViewItem c) { column = c; }
        if (tags[1] is RowData r) { row = r; }

        textbox.Tag = null;
        textbox.Visible = false;
        UserEdited(this, w, column, row, true);
        Focus();
    }

    private bool UserEdit_NewRowAllowed() {
        if (IsDisposed || Database is not { IsDisposed: false } db || db.Column.Count == 0 || db.Column.First() is not { IsDisposed: false } fc) { return false; }
        if (db.Column.Count == 0) { return false; }
        if (CurrentArrangement?[fc] is not { } fcv) { return false; }

        if (!db.PermissionCheck(db.PermissionGroupsNewRow, null)) { return false; }

        return string.IsNullOrEmpty(EditableErrorReason(fcv, null, EditableErrorReasonType.EditNormaly, true, true, false, Filter.ToArray()));
    }

    #endregion
}