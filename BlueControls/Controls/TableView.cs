// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.ControlStrategies;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.TableElements;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueTable.ClassesStatic;
using BlueTable.ColumnFormats;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Interfaces.MiniToolbarExtension;
using static BlueTable.Classes.Table;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(SelectedRowChanged))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableView : ZoomPad, IContextMenu, IMiniToolbar, ITranslateable, IHasTable, IStyleable {

    #region Fields

    public const string CellDataFormat = "BlueElements.CellLink";

    private readonly Dictionary<string, TableElement> _allViewItems = [];

    /// <summary>
    /// Pro Kapitel-Header alle Block-Zeilen (auch eingeklappte), für Drag/Drop zugeklappter Kapitel.
    /// </summary>
    private readonly Dictionary<RowCaptionTableElement, List<RowItem>> _chapterBlockRows = [];

    private readonly List<string> _collapsed = [];

    /// <summary>
    /// NumberStyle: KeyNames der ersten Zeile je eingeklapptem Block. Erlaubt unabhängiges Auf-/Zuklappen gleicher Chapter-Texte.
    /// </summary>
    private readonly HashSet<string> _collapsedBlockFirstRowKeys = [];

    /// <summary>
    /// Lazy Cache der Inline-Edit-Strategien nach ClassId. Wird beim Dispose der TableView mit freigegeben.
    /// </summary>
    private readonly ConcurrentCache<string, ControlStrategy> _controlStrategyCache = new(16);

    private readonly object _lockUserAction = new();

    private readonly Dictionary<RowItem, RowTableElement> _rowLookup = [];

    private string _arrangement = string.Empty;

    private AutoFilter? _autoFilter;

    private List<RowTableElement> _cachedRowViewItems = [];

    private bool _consumeNextMouseDown;

    private int _dragInsertIndex = -1;

    /// <summary>
    /// Bei MouseDown ermittelter Drag-Kandidat (ColumnViewItem, RowItem oder RowCaptionListItem); null, solange kein Drag-Vorgang ansteht.
    /// </summary>
    private object? _dragItem;

    private Point _dragMouseDown;

    /// <summary>
    /// Commit-Callback des aktiven Inline-Edits; wird beim Schließen mit dem neuen Text aufgerufen, danach null.
    /// </summary>
    private Action<string>? _editCommit;

    /// <summary>
    /// true während BeginEdit ein neues Edit aufbaut. Darin entstehende LostFocus-Events dürfen CloseAllComponents nicht auslösen.
    /// </summary>
    private bool _isBeginningEdit;

    /// <summary>
    /// true, sobald die Maus die DragSize-Schwelle überschritten hat und der Drag tatsächlich läuft (im Gegensatz zu _dragItem, das bereits bei MouseDown gesetzt wird).
    /// </summary>
    private bool _isDragging;

    private bool _isinDoubleClick;

    private bool _isinKeyDown;

    private bool _isinMouseDown;

    private bool _isinMouseMove;

    private bool _isinSizeChanged;

    private bool _mustDoAllViewItems = true;

    private string _newRowsAllowed = string.Empty;

    private bool _pendingRowAddedRebuild;

    private bool _pendingSmoothScroll;

    private List<RowItem> _rowsVisibleUnique = new([]);

    private RowSortDefinition? _sortDefinitionTemporary;

    private List<TableElement> _sortedViewItems = [];

    private JsonObject? _storedView;

    private DateTime? _tableDrawError;

    #endregion

    #region Constructors

    public TableView() : base() {
        InitializeComponent();

        // Filter-Pipeline: Filter + FilterFix → FilterCombined
        Filter.RowsChanged += FilterAny_RowsChanged;
        Filter.PropertyChanged += Filter_PropertyChanged;
        // FilterCombined ist das Ergebnis, dessen Rows die angezeigten Zeilen bestimmen
        FilterCombined.RowsChanged += FilterAny_RowsChanged;
        FilterCombined.PropertyChanged += FilterCombined_PropertyChanged;
        // FilterFix-Änderungen lösen eine Neuberechnung von FilterCombined aus
        FilterFix.PropertyChanged += FilterFix_PropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<FilterEventArgs>? AutoFilterClicked;

    public event EventHandler<CellEventArgs>? CellClicked;

    public event EventHandler? FilterCombinedChanged;

    public event EventHandler? PinnedChanged;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowNullableEventArgs>? SelectedRowChanged;

    public event EventHandler? TableChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler<JsonEventArgs>? ViewLoading;

    public event EventHandler<JsonEventArgs>? ViewSaving;

    public event EventHandler? VisibleRowsChanged;

    #endregion

    #region Properties

    public bool Ansichtbearbeitung {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate_CurrentArrangement();
        }
    }

    [DefaultValue("")]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public string Arrangement {
        get => _arrangement;
        set {
            if (value != _arrangement) {
                _arrangement = value;

                OnViewChanged();
                CursorPos_Set(CursorPosColumn, CursorPosRow, true);

                if (CurrentArrangement is { StartCollapsed: true }) { CollapesAll(); }
            }
        }
    }

    /// <summary>
    /// Gibt an, ob das Standard-Kontextmenu angezeigt werden soll.
    /// </summary>
    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    public override bool ControlMustPressedForZoomWithWheel => true;

    public ColumnViewCollection? CurrentArrangement {
        get {
            if (IsDisposed || Table is not { IsDisposed: false } tb) { return null; }

            if (field is null) {
                var tcvc = ColumnViewCollection.ParseAll(tb);
                field = tcvc.GetByKey(_arrangement);
                if (field is null && tcvc.Count > 1) { field = tcvc[1]; }
                if (field is null && tcvc.Count > 0) { field = tcvc[0]; }
                _pendingRowAddedRebuild = false;
            }

            // Lokale Kopie: field kann zwischen den Zugriffen null werden (Table-Disposed, Refresh).
            var ca = field;

            if (ca is { IsDisposed: false }) {
                ca.Ansichtbearbeitung = Ansichtbearbeitung;

                // On-demand virtuelle Spalten: Hinzufügen bei Admins, Pin bei angepinnten Zeilen.
                var needAdd = tb.IsAdministrator()
                    && (IsAnsicht0(ca) || Ansichtbearbeitung || (tb.Column.Count > 0 && ca.First() is null));
                var needPin = PinnedRows.Count > 0;

                ca.ReconcileVirtualColumns(needPin, needAdd);
            } else if (ca is { IsDisposed: true }) {
                field = null;
                return null;
            }

            if (ca is not null) {
                ca.SheetStyle = SheetStyle;
                // Indent-Breite abziehen, damit ScaleToFit die Spalten so
                // skaliert, dass eingerückte Zeilen vollständig sichtbar bleiben.
                var availWidth = AvailableControlPaintArea.Width - TableElement.IndentWidth.CanvasToControl(Zoom) * MaxIndentOfRows;
                ca.ComputeAllColumnPositions(Math.Max(16, availWidth), Zoom);
            }

            return ca;
        }

        private set;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnViewItem? CursorPosColumn { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowTableElement? CursorPosRow { get; private set; }

    [DefaultValue(null)]
    public ReadOnlyCollection<ListItem>? CustomContextMenuItems { get; set; }

    /// <summary>
    /// KeyName eines EventScripts, das beim Doppelklick auf eine Zelle statt der Bearbeitung ausgeführt wird. Leer = Standardbearbeitung.
    /// </summary>
    [DefaultValue("")]
    public string DoubleClickScript { get; set; } = string.Empty;

    [DefaultValue(false)]
    public bool EditButton {
        get;
        set {
            if (field == value) { return; }
            field = value;
            btnEdit.Visible = field;
        }
    }

    /// <summary>
    /// Zusammengeführtes Ergebnis aus Filter und FilterFix. Bestimmt die angezeigten Zeilen.
    /// </summary>
    public FilterCollection FilterCombined { get; } = new("TableFilterCombined");

    /// <summary>
    /// Fixfilter von übergeordneten Elementen (ConnectedFormula), nicht vom Benutzer änderbar.
    /// </summary>
    public FilterCollection FilterFix { get; } = new("FilterFix");

    [DefaultValue(false)]
    public bool MiniToolbarEnabled { get; set; } = false;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem> PinnedRows { get; } = [];

    public bool PowerEdit {
        get => Table?.PowerEdit ?? false;
        set {
            if (IsDisposed || Table is not { IsDisposed: false }) { return; }
            Table.PowerEdit = value;
        }
    }

    public List<RowTableElement> RowViewItems => [.. _cachedRowViewItems];

    [DefaultValue(Win11)]
    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate_CurrentArrangement();
            Invalidate();
        }
    } = Win11;

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
            if (_sortDefinitionTemporary is not null && value is not null && _sortDefinitionTemporary.ParseableItems().FinishParseable() == value.ParseableItems().FinishParseable()) { return; }
            if (_sortDefinitionTemporary == value) { return; }
            _sortDefinitionTemporary = value;
            _Table_SortParameterChanged(this, System.EventArgs.Empty);
        }
    }

    /// <summary>
    /// Aktuell zugewiesene Tabelle. Events werden beim Setzen angebunden/abgemeldet.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? Table {
        get;
        set {
            if (field == value) { return; }

            // Bei verschlüsselten Tabellen wird das Passwort erst bei der Anzeige angefordert. Bei falschem Passwort bleibt die alte Anzeige aktiv.
            if (value is { IsDisposed: false, Unlocked: false } tbLocked) {
                var pwd = Table_NeedPassword();
                if (!string.IsNullOrEmpty(pwd) &&
                    string.Equals(pwd, tbLocked.GlobalShowPass, StringComparison.Ordinal)) {
                    tbLocked.Unlocked = true;
                } else {
                    return;
                }
            }

            CloseAllComponents();

            if (field is { IsDisposed: false } tb1) {
                tb1.CellValueChanged -= Cell_CellValueChanged;
                tb1.Loaded -= _Table_TableLoaded;
                tb1.Loading -= _Table_StoreView;
                tb1.ViewChanged -= _Table_ViewChanged;
                tb1.SortParameterChanged -= _Table_SortParameterChanged;
                tb1.Row.RowRemoving -= Row_RowRemoving;
                tb1.Row.RowRemoved -= Row_RowRemoved;
                tb1.Row.RowAdded -= Row_RowAdded;
                tb1.Column.ColumnRemoving -= Column_ItemRemoving;
                tb1.Column.ColumnRemoved -= _Table_ViewChanged;
                tb1.Column.ColumnAdded -= _Table_ViewChanged;
                tb1.Disposed -= _table_Disposed;
                tb1.InvalidateView -= Table_InvalidateView;
                SaveAll();
            }
            ShowWaitScreen = true;
            Refresh();
            _storedView = null;
            _collapsedBlockFirstRowKeys.Clear();
            field = value;
            Invalidate_CurrentArrangement();
            Invalidate_AllViewItems(true);
            Filter.PropertyChanged -= Filter_PropertyChanged;
            FilterFix.PropertyChanged -= FilterFix_PropertyChanged;
            Filter.Table = value;
            FilterFix.Table = value;
            FilterCombined.Table = value;
            FilterFix.PropertyChanged += FilterFix_PropertyChanged;
            Filter.PropertyChanged += Filter_PropertyChanged;
            DoFilterCombined();

            _tableDrawError = null;
            if (field is { IsDisposed: false } tb2) {
                RepairColumnArrangements(tb2);

                tb2.CellValueChanged += Cell_CellValueChanged;
                tb2.Loaded += _Table_TableLoaded;
                tb2.Loading += _Table_StoreView;
                tb2.ViewChanged += _Table_ViewChanged;
                tb2.SortParameterChanged += _Table_SortParameterChanged;
                tb2.Row.RowRemoving += Row_RowRemoving;
                tb2.Row.RowRemoved += Row_RowRemoved;
                tb2.Row.RowAdded += Row_RowAdded;
                tb2.Column.ColumnAdded += _Table_ViewChanged;
                tb2.Column.ColumnRemoving += Column_ItemRemoving;
                tb2.Column.ColumnRemoved += _Table_ViewChanged;
                tb2.Disposed += _table_Disposed;
                tb2.InvalidateView += Table_InvalidateView;
            }

            ShowWaitScreen = false;

            OnEnabledChanged(System.EventArgs.Empty);

            OnTableChanged();
        }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    /// <summary>
    /// Benutzerfilter (AutoFilter, Textsuche). Werden mit FilterFix in FilterCombined zusammengeführt.
    /// </summary>
    internal FilterCollection Filter { get; } = new("DefaultTableFilter");

    /// <summary>
    /// Maximaler Indent aller sichtbaren Zeilen, für Breiten- und Höhenberechnung.
    /// </summary>
    internal int MaxIndentOfRows => _sortedViewItems is { Count: > 0 } items ? items.Max(i => i.Indent) : 0;

    protected override bool ShowSliderX => true;

    protected override int SmallChangeY => 10;

    /// <summary>
    /// Aktive Inline-Edit-Strategie (Control sichtbar und nicht disposet), sonst null.
    /// </summary>
    private ControlStrategy? ActiveControlStrategy {
        get {
            if (_controlStrategyCache.IsDisposed) { return null; }
            foreach (var strategy in _controlStrategyCache.Values) {
                if (strategy.Control is Control c
                    && c.Visible
                    && !c.IsDisposed) {
                    return strategy;
                }
            }
            return null;
        }
    }

    private Dictionary<string, TableElement>? AllViewItems {
        get {
            if (IsDisposed) { return null; }
            if (!_mustDoAllViewItems) { return _allViewItems; }

            try {
                _mustDoAllViewItems = false;
                CalculateAllViewItems(_allViewItems);

                OnVisibleRowsChanged();
                return _allViewItems;
            } catch {
                // Cooldown aktivieren (DrawControl zeigt 5s Wait-Screen),
                // aber KEIN Invalidate_AllViewItems — sonst entsteht eine
                // Dauerschleife: Exception → Recalc → gleiche Exception → ...
                // _mustDoAllViewItems bleibt false; der Retry erfolgt nach
                // Ablauf der Cooldown (siehe DrawControl).
                _tableDrawError = DateTime.UtcNow;
                return _allViewItems;
            }
        }
    }

    #endregion

    #region Methods

    public static void ContextMenu_DataValidation(object? sender, ContextMenuEventArgs e) {
        var (_, row, rows, _, _) = GetContextData(e.HotItem);
        DoScript(RowsFromContext(row, rows), true, null, "Datenüberprüfung");
    }

    public static void ContextMenu_DeleteRow(object? sender, ContextMenuEventArgs e) {
        var (_, row, rows, _, _) = GetContextData(e.HotItem);
        var r = RowsFromContext(row, rows);

        if (r.Count == 0) {
            Forms.MessageBox.Show("Keine Zeilen zum Löschen vorhanden.", ImageCode.Kreuz, "OK");
            return;
        }

        if (r[0].Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }

        if (r.Count == 1) {
            if (Forms.MessageBox.Show($"Zeile wirklich löschen? (<b>{r[0].ReadableText()}</b>)", ImageCode.Frage, "Löschen", "Abbruch") != 0) { return; }
        } else {
            if (Forms.MessageBox.Show($"{r.Count} Zeilen wirklich löschen?", ImageCode.Frage, "Löschen", "Abbruch") != 0) { return; }
        }

        var m = RowCollection.Remove(r, "Benutzer: löschen Befehl");

        if (m.IsFailed) {
            NotEditableInfo(m.FailedReason);
        }
    }

    public static void ContextMenu_EditColumnProperties(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, tableView, _) = GetContextData(e.HotItem);

        if (column is not { IsDisposed: false }) { return; }

        if (TableViewForm.EditableErrorMessage(column.Table, null)) { return; }

        ColumnItem? columnLinked = null;
        var posError = false;

        if (column.RelationType == RelationType.CellValues && row is not null) {
            (columnLinked, _, _, _) = row.LinkedCellData(column, true, false);
            posError = true;
        }

        var bearbColumn = column;
        if (columnLinked is not null) {
            columnLinked.Repair();
            if (!columnLinked.IsOk()) {
                bearbColumn = columnLinked;
                Forms.MessageBox.Show("Zuerst muss die verlinkte Spalte\rrepariert werden.", ImageCode.Information, "Ok");
            } else if (Forms.MessageBox.Show("Welche Spalte bearbeiten?", ImageCode.Frage, "Spalte in dieser Tabelle", "Verlinkte Spalte") == 1) {
                bearbColumn = columnLinked;
            }
        } else {
            if (posError) {
                Notification.Show(
                    "Keine aktive Verlinkung.<br>Spalte in dieser Tabelle wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Tabelle vorhanden?",
                    ImageCode.Information);
            }
        }

        column.Repair();

        using var w = new ColumnEditor(bearbColumn, tableView);
        w.ShowDialog();

        bearbColumn.Repair();
    }

    public static void ContextMenu_ExecuteScript(object? sender, ContextMenuEventArgs e) {
        var (_, row, rows, tableView, _) = GetContextData(e.HotItem);

        if (tableView?.Table is not { IsDisposed: false } tb) { return; }

        var sc = tb.EventScript.GetByKey(e.Item.KeyName);
        if (sc is null || sc.Table is not { IsDisposed: false }) {
            QuickNote.Show(NoteSymbols.Critical, "Fehler");
            return;
        }

        if (sc.NeedRow) {
            DoScript(RowsFromContext(row, rows), false, sc, sc.KeyName);
            return;
        }
        if (TableViewForm.EditableErrorMessage(sc.Table, null)) { return; }

        var s = tb.ExecuteScript(sc, !sc.ValuesReadOnly, null, null, true, true, false);

        if (!s.Failed) {
            QuickNote.Show(NoteSymbols.Ok, "Skript erfolgreich ausgeführt");
        } else {
            Forms.MessageBox.Show("Skript abgebrochen:\r\n" + s.ProtocolText, ImageCode.Kreuz, "OK");
        }
    }

    /// <summary>
    /// Erzeugt den Kontext für ein Rechtsklick-Menü. Für virtuelle Spalten wird das ViewItem gesetzt, sonst das ColumnItem.
    /// </summary>
    public static object ContextMenuItemGenerate(TableView tableView, ColumnViewItem? viewItem, ColumnItem? column, RowItem? row, IReadOnlyList<RowItem>? visibleRows) => new { Column = column, Row = row, VisibleRows = visibleRows, TableView = tableView, ViewItem = viewItem };

    public static void CopyToClipboard(ColumnItem? column, RowItem? row, bool meldung, Point cellScreen = default) {
        try {
            if (row is not null && column is not null && column.Table is { IsDisposed: false } tb) {
                var c = row.CellGetString(column);
                c = c.Replace("\r\n", "\r");
                c = c.Replace("\r", "\r\n");

                var dataObject = new DataObject();

                if (tb is TableFile) {
                    dataObject.SetData(CellDataFormat, $"{tb.KeyName}\r{column.KeyName}\r{row.KeyName}");// 1. Als ExtChar-Format (für interne Verwendung)
                }
                dataObject.SetText(c);// 2. Als Plain Text (für externe Anwendungen)
                Clipboard.SetDataObject(dataObject, true);

                //_ = CopytoClipboard(c);
                if (meldung) {
                    QuickNote.Show(NoteSymbols.Ok, "Kopiert", cellScreen.X + 5, cellScreen.Y);
                    //   Notification.Show(LanguageTool.DoTranslate("<b>{0}</b><br>ist nun in der Zwischenablage.", true, c), ImageCode.Kopieren);
                }
            } else {
                if (meldung) {
                    QuickNote.Show(NoteSymbols.Warning, "Nicht möglich", cellScreen.X + 5, cellScreen.Y);
                    //Notification.Show(LanguageTool.DoTranslate("Bei dieser Zelle nicht möglich."), ImageCode.Warnung);
                }
            }
        } catch {
            if (meldung) {
                QuickNote.Show(NoteSymbols.Critical, "Fehler", cellScreen.X + 5, cellScreen.Y);
                //Notification.Show(LanguageTool.DoTranslate("Unerwarteter Fehler beim Kopieren."), ImageCode.Warnung);
            }
        }
    }

    public static void DoUndo(ColumnItem? column, RowItem? row) {
        if (column is not { IsDisposed: false }) { return; }
        if (row is not { IsDisposed: false }) { return; }
        if (!column.SaveContent) { return; }

        if (column.RelationType == RelationType.CellValues) {
            var (lcolumn, lrow, _, _) = row.LinkedCellData(column, true, false);
            if (lcolumn is not null && lrow is not null) { DoUndo(lcolumn, lrow); }
            return;
        }

        var cellKey = CellCollection.KeyOfCell(column, row);
        if (column.Table is not { IsDisposed: false } tbl) { return; }
        var sortedUndoItems = tbl.Undo.Where(item => item.CellKey == cellKey).OrderByDescending(item => item.DateTimeUtc).ToList();

        if (sortedUndoItems.Count == 0) {
            Forms.MessageBox.Show("Keine vorherigen Inhalte<br>(mehr) vorhanden.", ImageCode.Information, "OK");
            return;
        }

        var tb = Get();
        var colFirst = tb.Column.GenerateAndAdd("ID", "ID", TextOneLineColumnFormat.Instance);
        var colDate = tb.Column.GenerateAndAdd("Aenderdatum", "Änderdatum", DateTimeColumnFormat.Instance);
        var colAnderer = tb.Column.GenerateAndAdd("Aenderer", "Änderer", TextOneLineColumnFormat.Instance);
        var colText = tb.Column.GenerateAndAdd("VorherigerText", "Geändert zu", column);

        if (colText is { IsDisposed: false }) {
            colText.DefaultRenderer = column.DefaultRenderer;
            colText.RendererSettings = column.RendererSettings;
            colText.MultiLine = column.MultiLine;
        }

        if (colFirst is { IsDisposed: false }) {
            colFirst.IsFirst = true;
        }

        tb.Column.DisableAllEditing();

        RowItem? firstRow = null;
        var co = 0;
        foreach (var undoItem in sortedUndoItems) {
            co++;
            var r = tb.Row.GenerateAndAdd("UndoRow_" + co, string.Empty);
            if (r is null) { continue; }
            firstRow ??= r;
            if (colDate is { IsDisposed: false }) { r.CellSet(colDate, undoItem.DateTimeUtc, string.Empty); }
            if (undoItem.User is not null && colAnderer is { IsDisposed: false }) { r.CellSet(colAnderer, undoItem.User, string.Empty); }
            if (undoItem.ChangedTo is not null && colText is { IsDisposed: false }) { r.CellSet(colText, undoItem.ChangedTo, string.Empty); }
        }

        var lastUndo = sortedUndoItems[^1];
        var lastRow = tb.Row.GenerateAndAdd("UndoRow_before", string.Empty);
        if (lastRow is not null) {
            if (colDate is { IsDisposed: false }) { lastRow.CellSet(colDate, "01.01.1900", string.Empty); }
            if (colText is { IsDisposed: false }) { lastRow.CellSet(colText, lastUndo.PreviousValue, string.Empty); }
            if (colAnderer is { IsDisposed: false }) { lastRow.CellSet(colAnderer, "?", string.Empty); }
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns("Aenderdatum", "Aenderer", "VorherigerText");
        tb.ColumnArrangements = tcvc.AsReadOnly();

        tb.SortDefinition = new RowSortDefinition(tb, colDate, true);

        var selected = InputBoxTableSelect.Show("Vorherigen Eintrag wählen:", tb);

        if (selected is not { IsDisposed: false }) {
            tb.Dispose();
            return;
        }

        var chosenValue = selected.CellGetString(colText);
        row.CellSet(column, chosenValue, "Undo-Befehl");
        tb.Dispose();
    }

    public static (ColumnItem? column, RowItem? row, IReadOnlyList<RowItem> rows, TableView? tableView, ColumnViewItem? viewItem) GetContextData(object? context) {
        if (context is null) { return (null, null, [], null, null); }
        dynamic ctx = context;
        ColumnItem? column = ctx.Column;
        RowItem? row = ctx.Row;
        var visibleRows = (IReadOnlyList<RowItem>?)ctx.VisibleRows;
        var rows = visibleRows ?? [];
        TableView tableView = ctx.TableView;
        ColumnViewItem? viewItem = ctx.ViewItem;
        return (column, row, rows, tableView, viewItem);
    }

    public static void ImportCsv(Table table, string csvtxt) {
        using ImportCsvScriptCommand x = new(table, csvtxt);
        x.ShowDialog();
    }

    /// <summary>
    /// Prüft auf Tabellen-Ebene (Rechte, Sperren, Verknüpfungen), ob die Zelle
    /// bearbeitet werden kann — ohne Bezug zur Ansicht.
    /// Gibt einen Fehlergrund oder einen leeren String zurück.
    /// </summary>
    public static string IsCellEditable(ColumnItem? column, RowItem? row, string? newChunkValue) {
        if (column?.Table is not { IsDisposed: false } tb) { return "Es ist keine Spalte ausgewählt."; }

        if (row is { IsDisposed: true }) { return "Die Zeile wurde verworfen."; }

        var oldChunk = newChunkValue;

        if (ControlStrategy.Cached(column.ControlStrategy) is NoneControlStrategy && !tb.PowerEdit) {
            return "Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.";
        }

        if (row is null) {
            if (tb.Column.First is not { IsDisposed: false } firstcol || firstcol != column) {
                return "Neue Zeilen müssen mit der ersten Spalte beginnen.";
            }

            if (!tb.PermissionCheck(tb.PermissionGroupsNewRow, null, true)) {
                return "Sie haben nicht die nötigen Rechte, um neue Zeilen anzulegen.";
            }

            if (tb.Column.ChunkValueColumn is { } cvc && newChunkValue is not null) {
                if (cvc != tb.Column.First && string.IsNullOrEmpty(newChunkValue)) { return "Chunk-Wert fehlt."; }
            }
        } else {
            if (!tb.PowerEdit && tb.Column.SysLocked is not null) {
                if (column != tb.Column.SysLocked && row.CellGetBoolean(tb.Column.SysLocked) && !column.EditAllowedDespiteLock) {
                    return "Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.";
                }
            }
            oldChunk = row.ChunkValue;
        }

        if (!tb.PermissionCheck(column.PermissionGroupsChangeCell, row, true)) {
            return "Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.";
        }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return $"Tabellensperre: {f}"; }

        if (column.RelationType == RelationType.CellValues) {
            if (row is null) { return "Verlinkungs-Fehler"; }

            var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);

            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Table is not { IsDisposed: false } tb2) { return "Verknüpfte Tabelle verworfen."; }

            tb2.PowerEdit = tb.PowerEdit;

            if (lrow is not null) {
                var tmp = IsCellEditable(lcolumn, lrow, lrow.ChunkValue);
                return !string.IsNullOrEmpty(tmp) ? "Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp : string.Empty;
            }

            if (canrepair) { return string.Empty; }

            return "Allgemeiner Fehler.";
        }

        if (row is null && tb.Column.ChunkValueColumn == tb.Column.First && newChunkValue is null) {
            // Es soll eine neue Zeile erstellt werden, und die erste Spalte ist die Chunk-Spalte.
            // Wir wissen nicht, was das Ziel ist.
            return string.Empty;
        }

        if (oldChunk != newChunkValue) {
            if (tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, oldChunk) is { Length: > 0 } aadc) { return aadc; }
        }

        return tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
    }

    public static List<string> Permission_AllUsed(bool mitRowCreator) {
        var l = new List<string>();

        foreach (var thisTb in Table.AllInstances()) {
            if (!thisTb.IsDisposed) {
                l.AddRange(Permission_AllUsedInThisTable(thisTb, mitRowCreator));
            }
        }

        return RepairUserGroups(l);
    }

    public static List<string> Permission_AllUsedInThisTable(Table tb, bool mitRowCreator) {
        List<string> e = [];
        foreach (var thisColumnItem in tb.Column) {
            if (thisColumnItem is not null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(tb.PermissionGroupsNewRow);
        e.AddRange(tb.TableAdmin);

        var tcvc = ColumnViewCollection.ParseAll(tb);
        foreach (var thisArrangement in tcvc) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }

        foreach (var thisEv in tb.EventScript) {
            e.AddRange(thisEv.UserGroups);
        }

        e.Add(Everybody);
        e.Add("#User: " + UserName);

        if (mitRowCreator) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.Add(UserGroup);
        e.RemoveString(Administrator, false);

        return RepairUserGroups(e);
    }

    public static Renderer.Renderer RendererOf(ColumnItem? column, string style) {
        if (column is null || string.IsNullOrEmpty(column.DefaultRenderer)) { return Renderer.Renderer.Default; }
        return RendererOf(column.DefaultRenderer, column.RendererSettings, style);
    }

    /// <summary>
    /// Erzeugt einen Renderer aus Typname und Einstellungen. Für virtuelle Spalten ohne ColumnItem. Fallback: Standard-Renderer.
    /// </summary>
    public static Renderer.Renderer RendererOf(string? rendererString, string rendererSettings, string style) {
        if (string.IsNullOrEmpty(rendererString)) { return Renderer.Renderer.Default; }

        var renderer = ParseableItem.NewByTypeName<Renderer.Renderer>(rendererString);
        if (renderer is null) { return Renderer.Renderer.Default; }

        if (!renderer.Parse(rendererSettings)) { return Renderer.Renderer.Default; }
        renderer.SheetStyle = style;

        return renderer;
    }

    public static void SearchNextText(string searchTxt, TableView tableView, ColumnViewItem? column, RowTableElement? row, out ColumnViewItem? foundColumn, out RowTableElement? foundRow, bool vereinfachteSuche) {
        // Standard-Rückgabe: nichts gefunden
        foundColumn = null;
        foundRow = null;

        if (tableView.Table is not { IsDisposed: false } tb) {
            QuickNote.Show(NoteSymbols.Critical, "Tabellen-Fehler");
            return;
        }

        searchTxt = searchTxt.Trim();
        if (tableView.CurrentArrangement is not { IsDisposed: false } ca) {
            QuickNote.Show(NoteSymbols.Critical, "Ansichts-Fehler");
            return;
        }

        row ??= tableView.View_RowLast();
        column ??= ca.Last();

        if (string.IsNullOrEmpty(searchTxt)) {
            var cp = tableView.CursorPosRow?.ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY) ?? Rectangle.Empty;
            var sp = tableView.PointToScreen(new Point(tableView.CursorPosColumn?.ControlColumnRight(tableView.OffsetX) ?? 0, cp.Y));
            QuickNote.Show(NoteSymbols.Warning, "Eingabe nötig", sp.X + 5, sp.Y);
            return;
        }

        // Für vereinfachte Suche vorberechnen (unveränderlich innerhalb der Schleife)
        var searchTxtVereinfacht = vereinfachteSuche ? searchTxt.StarkeVereinfachung(" ,", true) : string.Empty;

        var rowsChecked = 0;
        do {
            column = ca.NextVisible(column);

            var renderer = column?.GetRenderer(tableView.SheetStyle);

            if (column is null) {
                column = ca.First();
                if (rowsChecked > tb.Row.Count + 1) { return; }
                rowsChecked++;
                row = tableView.View_NextRow(row) ?? tableView.View_RowFirst();
            }

            var tmprow = row?.Row;
            //if (column?.Column is { Function: ColumnFunction.Verknüpfung_zu_anderer_Tabellex } cv) {
            //    var (contentHolderCellColumn, contentHolderCellRow, _, _) = CellCollection.LinkedCellData(cv, tmprow, false, false);

            //    if (contentHolderCellColumn is not null && contentHolderCellRow is not null) {
            //        ist1 = contentHolderCellRow.CellGetString(contentHolderCellColumn);
            //        if (renderer is not null) {
            //            ist2 = renderer.ValueReadable(contentHolderCellRow.CellGetString(contentHolderCellColumn),
            //                ShortenStyle.Both, contentHolderCellColumn.DoOpticalTranslation);
            //        }
            //    }
            //} else {
            if (tmprow is null || column?.Column is not { IsDisposed: false } c) { continue; }

            var ist1 = tmprow.CellGetString(c);
            var ist2 = renderer?.ValueReadable(ist1, ShortenStyle.Both, c.DoOpticalTranslation) ?? string.Empty;

            // Bei formatierten Spalten den Klartext für die Suche verwenden
            if (c.TextFormatingAllowed) {
                var l = new ExtText(Design.TextBox, States.Standard) {
                    HtmlText = ist1
                };
                ist1 = l.PlainText;
            }

            // Allgemeine Prüfung und Prüfung mit Ersetzungen / Prefix / Suffix
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (ist1.Contains(searchTxt, comparison) || ist2.Contains(searchTxt, comparison)) {
                foundColumn = column;
                foundRow = row;
                return;
            }

            // Prüfung mit starker Vereinfachung
            if (vereinfachteSuche && !string.IsNullOrEmpty(searchTxtVereinfacht) &&
                ist2.StarkeVereinfachung(" ,", true).Contains(searchTxtVereinfacht, comparison)) {
                foundColumn = column;
                foundRow = row;
                return;
            }
            //}
        } while (true);
    }

    //    return renderer.GetSizeOfCellContent(column, row.CellGetString(column), Design.Table_Cell, States.Standard,
    //        column.BehaviorOfImageAndText, column.DoOpticalTranslation, column.OpticalReplace, tb.GlobalScale, column.ConstantHeightOfImageCode);
    //}
    public static void Table_AdditionalRepair(object? sender, System.EventArgs e) {
        if (sender is not Table tbl) { return; }

        RepairColumnArrangements(tbl);
    }

    public static void Table_CanDoScript(object? sender, CanDoScriptEventArgs e) {
        if (!string.IsNullOrEmpty(e.CancelReason)) { return; }

        if (sender is not Table tbl) { return; }
        if (!FormManager.Running) { e.CancelReason = "Programm wird beendet"; return; }

        foreach (var thisf in FormManager.Forms) {
            if (thisf is TableHeadEditor) { e.CancelReason = "Head Editor geöffnet"; return; }
            if (thisf is TableScriptEditorForm tsf && tsf.Object != tbl) { e.CancelReason = "Fremder Skript Editor geöffnet"; return; }
        }
    }

    //    if (column.Function == ColumnFunction.Verknüpfung_zu_anderer_Tabellex) {
    //        var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
    //        return lcolumn is not null && lrow is not null ? ContentSize(lcolumn, lrow, renderer)
    //            : new CanvasSize(16, 16);
    //    }
    public static string Table_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Tabelle<br>zu erhalten:", string.Empty, BlueBasics.Classes.Formats.TextFormat.Instance);

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, Table? table, string showingKey) {
        if (columnArrangementSelector is not { IsDisposed: false }) { return; }

        columnArrangementSelector.AutoSort = false;

        columnArrangementSelector.ItemClear();
        columnArrangementSelector.DropDownStyle = DropDownMode.DropDownList;

        if (table is { IsDisposed: false } tb) {
            var tcvc = ColumnViewCollection.ParseAll(tb);
            var addedCount = 0;

            foreach (var thisArrangement in tcvc) {
                if (tb.PermissionCheck(thisArrangement.PermissionGroups_Show, null, true)) {
                    var item = ItemOf(thisArrangement as IReadableTextWithKey);
                    if (addedCount < 2) { item.MoveLocked = true; item.RemoveLocked = true; }
                    columnArrangementSelector.ItemAdd(item);
                    addedCount++;
                }
            }
        }

        columnArrangementSelector.Enabled = columnArrangementSelector.ItemCount > 1;

        if (columnArrangementSelector[showingKey] is null) {
            showingKey = columnArrangementSelector.ItemCount > 1 && columnArrangementSelector[1] is { } second ? second.KeyName ?? string.Empty : string.Empty;
        }

        columnArrangementSelector.Text = showingKey;
    }

    public (ColumnViewItem? column, TableElement? row) CellOnLastMouseDown() {
        var row = RowItemAtPosition(MouseDownData?.ControlY ?? 0);
        return (ColumnOnCoordinate(CurrentArrangement, MouseDownData, row), row);
    }

    public void CheckView() {
        var tb = Table;
        if (CursorPosColumn?.Column?.Table != tb) { CursorPosColumn = null; }
        if (CursorPosRow?.Row.Table != tb) { CursorPosRow = null; }

        if (CurrentArrangement is { IsDisposed: false } ca && tb is not null) {
            if (!tb.PermissionCheck(ca.PermissionGroups_Show, null, true)) { Arrangement = string.Empty; }
        } else {
            Arrangement = string.Empty;
        }
    }

    public void CollapesAll() {
        var did = false;

        if (AllViewItems is not { } avi) { return; }

        // NumberStyle: alle sichtbaren Block-Header einklappen. Der Zustand
        // wird pro Block in _collapsedBlockFirstRowKeys gespeichert.
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            foreach (var thisItem in _sortedViewItems) {
                if (thisItem is RowCaptionTableElement { IsDisposed: false } rcli
                    && GetChapterBlockRows(rcli) is { Count: > 0 } blockRows
                    && blockRows[0] is { IsDisposed: false } firstRow) {
                    did |= _collapsedBlockFirstRowKeys.Add(firstRow.KeyName);
                }
            }
        } else {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowCaptionTableElement { IsDisposed: false, IsExpanded: true } rcli) { rcli.IsExpanded = false; did = true; }
            }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    public void CursorPos_Set(ColumnViewItem? column, TableElement? row, bool ensureVisible) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is null || column is null ||
            CurrentArrangement is not { IsDisposed: false } ca2 || !ca2.Contains(column) ||
            AllViewItems is not { } avi || !avi.ContainsValue(row)) {
            // Verwaiste Referenzen über die stabile RowItem-/ColumnItem-Identität migrieren.
            var oldRli = row as RowTableElement;
            var colItem = column?.Column;
            var rowItem = oldRli?.Row;
            var chapter = oldRli?.AlignsToChapter;
            var ca = CurrentArrangement;
            var freshCol = colItem is { IsDisposed: false } && ca is { IsDisposed: false } ? ca[colItem] : null;
            // _rowLookup umgehen: bei mehrfacher Anzeige einer Row würde sonst das falsche Kapitel geliefert.
            var freshRow = rowItem is { IsDisposed: false } ? GetRow(rowItem, chapter) : null;

            if (freshCol is not null && freshRow is not null
                && ca is { IsDisposed: false } && ca.Contains(freshCol)
                && AllViewItems is { } avi2 && avi2.ContainsValue(freshRow)) {
                column = freshCol;
                row = freshRow;
            } else {
                column = null;
                row = null;
            }
        }

        var sameRow = CursorPosRow == row;

        if (CursorPosColumn == column && CursorPosRow == row) { return; }
        QuickInfo = string.Empty;
        CursorPosColumn = column;
        CursorPosRow = row as RowTableElement;

        //if (CursorPosColumn != column) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        DoCursorPos();

        if (ensureVisible) {
            EnsureVisible(CursorPosColumn, CursorPosRow);
        }
        Invalidate();

        OnSelectedCellChanged(new CellExtEventArgs(CursorPosColumn, CursorPosRow));

        if (!sameRow) {
            OnSelectedRowChanged(new RowNullableEventArgs(CursorPosRow?.Row));
        }
    }

    public bool EnsureVisible(ColumnViewItem? viewItem, TableElement? row) => EnsureVisible(viewItem) && EnsureVisible(row);

    public void ExpandAll() {
        var did = false;

        if (AllViewItems is not { } avi) { return; }

        // NumberStyle: alle Block-Zustände verwerfen, damit jeder Block
        // ausgeklappt wird.
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            did = _collapsedBlockFirstRowKeys.Count > 0;
            _collapsedBlockFirstRowKeys.Clear();
        } else {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowCaptionTableElement { IsDisposed: false, IsExpanded: false } rcli) { rcli.IsExpanded = true; did = true; }
            }
        }

        if (did) {
            CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

            Invalidate_AllViewItems(false);
        }
    }

    public string Export_CSV(FirstRow firstRow) => Table is null ? string.Empty : CsvHelper.ExportCsv(Table, firstRow, CurrentArrangement?.ListOfUsedColumn(), RowsVisibleUnique());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return string.Empty; }
        List<ColumnItem> l = [onlyColumn];
        return CsvHelper.ExportCsv(Table, firstRow, l, RowsVisibleUnique());
    }

    public void Export_HTML(string filename = "", bool execute = true) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        if (string.IsNullOrEmpty(filename)) { filename = TempFile(string.Empty, string.Empty, "html"); }

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        var da = new Html(tb.KeyName.FileNameWithoutSuffix());
        da.AddCaption(tb.Caption);
        da.TableBeginn();

        #region Spaltenköpfe

        da.RowBeginn();
        foreach (var thisColumn in ca) {
            if (thisColumn.Column is not null) {
                da.CellAdd(thisColumn.Column.ReadableText().Replace(";", "<br>"), thisColumn.Column.BackColor);
            }
        }

        da.RowEnd();

        #endregion

        #region Zeilen

        if (AllViewItems is { } avi) {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowTableElement { IsDisposed: false } rdli && rdli.Visible) {
                    da.RowBeginn();
                    foreach (var thisColumn in ca) {
                        // Column ist bei virtuellen Spalten (Pin, Nummer) null.
                        if (thisColumn.Column is { IsDisposed: false } col) {
                            da.CellAdd(string.Join("<br>", rdli.Row.CellGetList(col)), col.BackColor);
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
    /// Setzt den Fokus auf die TableView.
    /// </summary>
    public new void Focus() {
        if (Focused) { return; }
        base.Focus();
    }

    public List<ListItem>? GetContextMenuItems(object? hotItem) {
        List<ListItem> contextMenu = [];

        if (ContextMenuDefault && Table is { IsDisposed: false } tb) {
            var (column, row, _, _, viewItem) = GetContextData(hotItem);

            if (CurrentArrangement is not { } ca) { return contextMenu; }

            if (ca.Kontextmenu_Skripte.Count > 0 && row is not null) {
                foreach (var thisString in ca.Kontextmenu_Skripte) {
                    if (tb.EventScript.GetByKey(thisString, StringComparison.OrdinalIgnoreCase) is { } thiss) {
                        var enabled = thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null, true) && thiss.NeedRow && thiss.IsOk();

                        contextMenu.Add(ItemOf(thiss.ReadableText(), thiss.SymbolForReadableText(), thiss.KeyName, ContextMenu_ExecuteScript, enabled, thiss.QuickInfo));
                    }
                }
                return contextMenu;
            }

            #region Pinnen

            if (row is not null) {
                contextMenu.Add(ItemOf("Anheften", true));
                if (PinnedRows.Contains(row)) {
                    contextMenu.Add(ItemOf("Zeile nicht mehr pinnen", QuickImage.Get(ImageCode.Pinnadel, IContextMenu.IconSize, ImageCode.Kreuz), ContextMenu_Unpin, true));
                } else {
                    contextMenu.Add(ItemOf("Zeile anpinnen", QuickImage.Get(ImageCode.Pinnadel, IContextMenu.IconSize, ImageCode.Häkchen), ContextMenu_Pin, true));
                }
            }

            #endregion

            if (column is not null && row is not null) {
                contextMenu.Add(ItemOf("Notiz", true));
                var existingNote = CellNoteHelper.GetNoteData(column, row);
                if (existingNote is null) {
                    contextMenu.Add(ItemOf("Notiz bearbeiten", QuickImage.Get(ImageCode.Textdatei, IContextMenu.IconSize, ImageCode.Stift), ContextMenu_Note_Edit, true));
                } else {
                    contextMenu.Add(ItemOf("Notiz entfernen", QuickImage.Get(ImageCode.Textdatei, IContextMenu.IconSize, ImageCode.Radiergummi), ContextMenu_Note_Remove, true));
                }
            }

            #region Sortierung

            if (column is not null) {
                var sortEnabled = tb.Column.SysRowSortIndex is not { IsDisposed: false };
                contextMenu.Add(ItemOf("Sortierung", true));
                contextMenu.Add(ItemOf("Sortierung zurückstetzen", QuickImage.Get("AZ|16|8|1"), ContextMenu_ResetSort, sortEnabled, string.Empty));
                contextMenu.Add(ItemOf("Nach dieser Spalte aufsteigend sortieren", QuickImage.Get("AZ|16|8"), ContextMenu_SortAZ, sortEnabled, string.Empty));
                contextMenu.Add(ItemOf("Nach dieser Spalte absteigend sortieren", QuickImage.Get("ZA|16|8"), ContextMenu_SortZA, sortEnabled, string.Empty));
            }

            #endregion

            #region Zelle

            if (column is not null && row is not null) {
                var editable = string.IsNullOrEmpty(TableView.IsCellEditable(column, row, row.ChunkValue));

                contextMenu.Add(ItemOf("Zelle", true));

                contextMenu.Add(ItemOf("Inhalt kopieren", ImageCode.Kopieren, ContextMenu_ContentCopy, column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Inhalt einfügen", ImageCode.Clipboard, ContextMenu_ContentPaste, editable && column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Inhalt löschen", ImageCode.Radiergummi, ContextMenu_ContentDelete, editable && column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Vorherigen Inhalt wiederherstellen", QuickImage.Get(ImageCode.Undo, IContextMenu.IconSize), ContextMenu_RestorePreviousContent, editable && column.CanBeChangedByRules() && column.SaveContent, string.Empty));
                contextMenu.Add(ItemOf("Suchen und ersetzen", QuickImage.Get(ImageCode.Lupe, IContextMenu.IconSize), ContextMenu_SearchAndReplace, tb.IsAdministrator(), string.Empty));
                contextMenu.Add(ItemOf("Zeilenschlüssel kopieren", ImageCode.Schlüssel, ContextMenu_KeyCopy, tb.IsAdministrator()));
            }

            #endregion

            #region Spalte

            if (column is not null || viewItem?.StorageKey is not null) {
                contextMenu.Add(ItemOf("Spalte", true));

                if (viewItem?.StorageKey is not null) {
                    // Virtuelle Spalten (Pin, Nummer, Hinzufügen) haben kein
                    // ColumnItem. Sie können nur aus der aktuellen Anordnung
                    // ausgeblendet werden; permanente Operationen, Eigenschaften,
                    // Kopieren, Statistik etc. sind nicht möglich.
                    contextMenu.Add(ItemOf("Spalte ausblenden", QuickImage.Get(ImageCode.Spalte, IContextMenu.IconSize, ImageCode.Kreuz), ContextMenu_HideOrDeleteColumn, tb.IsAdministrator()));
                } else {
                    contextMenu.Add(ItemOf("Spalteneigenschaften bearbeiten", QuickImage.Get(ImageCode.Spalte, IContextMenu.IconSize, ImageCode.Stift), ContextMenu_EditColumnProperties, tb.IsAdministrator()));

                    if (IsAnsicht0(ca)) {
                        contextMenu.Add(ItemOf("Spalte permanent löschen", ImageCode.Papierkorb, ContextMenu_HideOrDeleteColumn, tb.IsAdministrator()));
                    } else {
                        contextMenu.Add(ItemOf("Spalte ausblenden", QuickImage.Get(ImageCode.Spalte, IContextMenu.IconSize, ImageCode.Kreuz), ContextMenu_HideOrDeleteColumn, tb.IsAdministrator()));
                    }

                    contextMenu.Add(ItemOf("Spalte erstellen / einblenden", ImageCode.PlusZeichen, ContextMenu_NewColumn, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren", ImageCode.Clipboard, ContextMenu_CopyAll, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren + sortieren", ImageCode.Clipboard, ContextMenu_CopyAllSorted, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Statistik", QuickImage.Get(ImageCode.Balken, IContextMenu.IconSize), ContextMenu_Statistics, tb.IsAdministrator(), string.Empty));
                    contextMenu.Add(ItemOf("Summe", ImageCode.Summe, ContextMenu_Sum, tb.IsAdministrator()));
                }
            }

            #endregion

            if (row is not null) {
                contextMenu.Add(ItemOf("Zeile", true));

                contextMenu.Add(ItemOf("Zeile löschen", QuickImage.Get(ImageCode.Zeile, IContextMenu.IconSize, ImageCode.Kreuz), ContextMenu_DeleteRow, tb.IsAdministrator() && tb.IsThisScriptOk(ScriptEventTypes.row_deleting, true), string.Empty));
                contextMenu.Add(ItemOf("Komplette Datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, IContextMenu.IconSize), ContextMenu_DataValidation, tb.CanDoValueChangedScript(true), string.Empty));

                var didmenu = false;
                foreach (var thiss in tb.EventScript) {
                    if (thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null, true) && thiss.NeedRow && thiss.IsOk()) {
                        if (!didmenu) {
                            contextMenu.Add(ItemOf("Skripte", true));
                            didmenu = true;
                        }
                        var enabled = thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null, true) && thiss.NeedRow && thiss.IsOk();
                        contextMenu.Add(ItemOf("Skript: " + thiss.ReadableText(), thiss.SymbolForReadableText(), thiss.KeyName, ContextMenu_ExecuteScript, enabled, thiss.QuickInfo));
                    }
                }
            }
        }

        return contextMenu;
    }

    public List<ListItem>? GetMiniToolbarItems(object? hotItem) {
        List<ListItem> miniToolbar = [];

        if (Table is not { IsDisposed: false } tb) { return miniToolbar; }

        var (column, row, _, _, _) = GetContextData(hotItem);

        #region Pinnen

        if (row is not null) {
            if (PinnedRows.Contains(row)) {
                miniToolbar.Add(ItemOf(string.Empty, "Unpin", QuickImage.Get(ImageCode.Pinnadel, IMiniToolbar.IconSize, ImageCode.Kreuz), ContextMenu_Unpin, true, "Zeile nicht mehr pinnen"));
            } else {
                miniToolbar.Add(ItemOf(string.Empty, "Pin", QuickImage.Get(ImageCode.Pinnadel, IMiniToolbar.IconSize, ImageCode.Häkchen), ContextMenu_Pin, true, "Zeile anpinnen"));
            }
        }

        #endregion

        if (!IsAdministrator()) { return miniToolbar; }

        #region Notiz

        if (column is not null && row is not null) {
            var notenabled = tb.Column.SysCellNote is not null;
            var existingNote = CellNoteHelper.GetNoteData(column, row);
            if (existingNote is null) {
                miniToolbar.Add(ItemOf(string.Empty, "NoteEdit", QuickImage.Get(ImageCode.Textdatei, IMiniToolbar.IconSize, ImageCode.Stift), ContextMenu_Note_Edit, notenabled, "Notiz bearbeiten"));
            } else {
                miniToolbar.Add(ItemOf(string.Empty, "NoteRemove", QuickImage.Get(ImageCode.Textdatei, IMiniToolbar.IconSize, ImageCode.Radiergummi), ContextMenu_Note_Remove, notenabled, "Notiz entfernen"));
            }
        }

        #endregion

        //#region Zeile löschen

        //if (row is not null) {
        //    var canDelete = tb.IsAdministrator() && tb.IsThisScriptOk(ScriptEventTypes.row_deleting, true);
        //    miniToolbar.Add(ItemOf(string.Empty, "DeleteRow", QuickImage.Get(ImageCode.Zeile, IMiniToolbar.IconSize, ImageCode.Kreuz), ContextMenu_DeleteRow, canDelete, "Zeile löschen"));
        //}

        //#endregion

        #region Neue Zeile im selben Kapitel (nur bei aktiver SysRowSortIndex)

        if (row is not null) {
            var canAdd = string.IsNullOrEmpty(tb.IsNowNewRowPossible(row.ChunkValue, true));
            miniToolbar.Add(ItemOf(string.Empty, "NewRowInChapter",
                QuickImage.Get(ImageCode.Zeile, IMiniToolbar.IconSize, ImageCode.PlusZeichen),
                ContextMenu_NewRowInChapter, canAdd, "Leere Zeile einfügen"));
        }

        #endregion

        return miniToolbar;
    }

    public void ImportClipboard() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        if (!Clipboard.ContainsText()) {
            QuickNote.Show(NoteSymbols.Warning, "Kein Text");
            return;
        }

        var nt = Clipboard.GetText();
        ImportCsv(nt);
    }

    public void ImportCsv(string csvtxt) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        ImportCsv(tb, csvtxt);
    }

    /// <summary>
    /// Prüft die Editierbarkeit auf Tabellen-Ebene und zusätzlich, ob die Zelle
    /// in der aktuellen Ansicht liegt und sichtbar ist. Mit maychangeview wird
    /// die Zelle vorher ggf. sichtbar gescrollt.
    /// Gibt einen Fehlergrund oder einen leeren String zurück.
    /// </summary>
    public string IsCellEditableInView(ColumnViewItem? cellInThisTableColumn, RowTableElement? cellInThisTableRow, string? newChunkVal, bool maychangeview) {
        if (IsCellEditable(cellInThisTableColumn?.Column, cellInThisTableRow?.Row, newChunkVal) is { Length: > 0 } f) { return f; }

        // Chunk-Ladevorgang kann Invalidate_CurrentArrangement auslösen, danach ColumnViewItem neu auflösen.
        if (cellInThisTableColumn?.Column is { IsDisposed: false } col) {
            cellInThisTableColumn = CurrentArrangement?[col];
        }

        if (CurrentArrangement is not { IsDisposed: false } ca || !ca.Contains(cellInThisTableColumn)) {
            return "Ansicht veraltet";
        }

        if (cellInThisTableColumn is null) {
            return "Keine Spalte angekommen.";
        }

        //var visCanvasArea = AvailableControlPaintArea.ControlToCanvas(Zoom, OffsetX, OffsetY).ToRect();

        if (cellInThisTableRow is not null) {
            if (maychangeview && !EnsureVisible(cellInThisTableColumn, cellInThisTableRow)) {
                return "Zelle konnte nicht angezeigt werden.";
            }

            //var realHead = cellInThisTableColumn.RealHead(Zoom, OffsetX);
            //if (realHead.Right < 0 || realHead.Left > DisplayRectangle.Width) {
            //    return "Spalte konnte nicht angezeigt werden.";
            //}

            if (!cellInThisTableRow.IsVisible(AvailableControlPaintArea, Zoom, OffsetX, OffsetY)) {
                return "Die Zeile wird nicht angezeigt.";
            }
        } else {
            if (maychangeview && !EnsureVisible(cellInThisTableColumn)) {
                return "Zelle konnte nicht angezeigt werden.";
            }
        }

        return string.Empty;
    }

    public void OpenSearchAndReplaceInCells() {
        if (Table is not { IsDisposed: false } tb || !string.IsNullOrEmpty(tb.IsGenericEditable(false))) { return; }

        if (!Table.IsAdministrator()) { return; }

        IUniqueWindowExtension.ShowOrCreate<SearchAndReplaceInCells>(this);
    }

    public void OpenSearchAndReplaceInTbScripts() {
        if (TableViewForm.EditableErrorMessage(Table, null)) { return; }
        if (!IsAdministrator()) { return; }

        IUniqueWindowExtension.ShowOrCreate<SearchAndReplaceInTbScripts>(null);
    }

    public void OpenSearchInCells() => IUniqueWindowExtension.ShowOrCreate<OpenSearchInCells>(this);

    public void Pin(IReadOnlyList<RowItem>? rows) {
        // Arbeitet mit Rows, weil nur eine Anpinngug möglich ist
        rows ??= [];

        rows = [.. rows.Distinct()];
        if (!rows.IsDifferentTo(PinnedRows)) { return; }

        PinnedRows.Clear();
        PinnedRows.AddRange(rows);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void PinAdd(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        if (PinnedRows.Contains(row)) { return; }
        PinnedRows.Add(row);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
        if (!PinnedRows.Contains(row)) { return; }
        PinnedRows.Remove(row);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void ResetView() {
        CancelSmoothScroll();
        _pendingSmoothScroll = false;
        Filter.Clear();
        // FilterCombined wird automatisch durch Filter.Clear() → PropertyChanged → DoFilterCombined() neu berechnet.

        PinnedRows.Clear();
        _collapsedBlockFirstRowKeys.Clear();

        Invalidate_AllViewItems(true);

        QuickInfo = string.Empty;
        _sortDefinitionTemporary = null;
        CursorPosColumn = null;
        CursorPosRow = null;
        _arrangement = string.Empty;
        Zoom = 1f;
        OffsetX = 0;
        OffsetY = 0;

        OnViewChanged();
    }

    public IReadOnlyList<RowItem> RowsVisibleUnique() => _rowsVisibleUnique;

    public void SetView(JsonObject? view) {
        ResetView();

        if (IsDisposed || Table is not { IsDisposed: false } tb || view is null) { return; }

        var e = new JsonEventArgs(string.Empty, view);
        OnViewLoading(e);

        Arrangement = view.GetString("Arrangement");

        if (view.GetJson("Filters") is not null) {
            Filter.PropertyChanged -= Filter_PropertyChanged;
            Filter.Table = Table;
            Filter.Clear();
            Filter.Parse(view.GetString("Filters"));
            Filter.PropertyChanged += Filter_PropertyChanged;
            DoFilterCombined();
        }

        if (view.GetJson("CursorPos") is not null) {
            tb.Cell.DataOfCellKey(view.GetString("CursorPos"), out var column, out var row);
            CursorPos_Set(CurrentArrangement?[column], GetRow(row, null), false);
        }

        if (view.GetJson("TempSort") is not null) {
            _sortDefinitionTemporary = new RowSortDefinition(Table, view.GetString("TempSort"));
        }

        if (view.GetJson("Pin") is not null) {
            foreach (var thisk in view.GetString("Pin").SplitBy("|")) {
                var r = tb.Row.GetByKey(thisk);
                if (r is { IsDisposed: false }) { PinnedRows.Add(r); }
            }
        }

        if (view.GetJson("Collapsed") is not null) {
            CollapseThis(view.GetString("Collapsed").SplitAndCutBy("|"));
        }

        if (view.GetJson("CollapsedBlocks") is not null) {
            foreach (var thisk in view.GetString("CollapsedBlocks").SplitBy("|")) {
                _collapsedBlockFirstRowKeys.Add(thisk);
            }
        }

        if (view.GetJson("Reduced") is not null) {
            CurrentArrangement?.Reduce(view.GetString("Reduced").SplitBy("|"));
        }

        base.ParseView(view);

        CheckView();
    }

    /// <summary>
    /// Klappt alle Kapitel zu, falls eines ausgeklappt ist; sonst alle auf.
    /// </summary>
    public void ToggleAllChapters() {
        if (AllViewItems is not { } avi) { return; }

        var anyExpanded = false;

        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            // NumberStyle: ein Block ist ausgeklappt, wenn sein FirstRow-Key
            // nicht in _collapsedBlockFirstRowKeys verzeichnet ist.
            foreach (var thisItem in _sortedViewItems) {
                if (thisItem is RowCaptionTableElement { IsDisposed: false } rcli
                    && GetChapterBlockRows(rcli) is { Count: > 0 } blockRows
                    && blockRows[0] is { IsDisposed: false } firstRow
                    && !_collapsedBlockFirstRowKeys.Contains(firstRow.KeyName)) {
                    anyExpanded = true;
                    break;
                }
            }
        } else {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowCaptionTableElement { IsDisposed: false, IsExpanded: true }) {
                    anyExpanded = true;
                    break;
                }
            }
        }

        if (anyExpanded) {
            CollapesAll();
        } else {
            ExpandAll();
        }
    }

    public ColumnViewItem? View_ColumnFirst() => IsDisposed || Table is not { IsDisposed: false } ? null : CurrentArrangement is { Count: not 0 } ca ? ca[0] : null;

    public RowTableElement? View_NextRow(RowTableElement? row) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        var idx = _sortedViewItems.IndexOf(row);
        // Verwaiste Instanz über RowItem-Identität + Kapitel-Caption auflösen.
        if (idx < 0) {
            var fresh = GetRow(row.Row, row.AlignsToChapter);
            if (fresh is not null) {
                row = fresh;
                idx = _sortedViewItems.IndexOf(row);
            }
        }
        return idx < 0 ? null : FindVisibleRowListItem(idx + 1, 1);
    }

    public RowTableElement? View_PreviousRow(RowTableElement? row) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        var idx = _sortedViewItems.IndexOf(row);
        // Siehe View_NextRow.
        if (idx < 0) {
            var fresh = GetRow(row.Row, row.AlignsToChapter);
            if (fresh is not null) {
                row = fresh;
                idx = _sortedViewItems.IndexOf(row);
            }
        }
        return idx < 0 ? null : FindVisibleRowListItem(idx - 1, -1);
    }

    public RowTableElement? View_RowFirst() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        return FindVisibleRowListItem(0, 1);
    }

    public RowTableElement? View_RowLast() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        return FindVisibleRowListItem(_sortedViewItems.Count - 1, -1);
    }

    public override JsonObject ViewToJson() {
        var result = base.ViewToJson();

        if (!string.IsNullOrEmpty(_arrangement)) {
            result.Add("Arrangement", _arrangement);
        }

        if (Filter is { IsDisposed: false } filter && !filter.IsDisposed) {
            result.Add("Filters", filter.ParseableItems().FinishParseable());
        }

        var pin = PinnedRows.ToListOfString();
        if (pin.Count > 0) {
            result.Add("Pin", string.Join("|", pin));
        }

        if (_collapsed.Count > 0) {
            result.Add("Collapsed", string.Join("|", _collapsed));
        }

        if (_collapsedBlockFirstRowKeys.Count > 0) {
            result.Add("CollapsedBlocks", string.Join("|", _collapsedBlockFirstRowKeys));
        }

        var reduced = CurrentArrangement?.ReducedColumns().ToListOfString();
        if (reduced is { Count: > 0 }) {
            result.Add("Reduced", string.Join("|", reduced));
        }

        if (_sortDefinitionTemporary is not null) {
            result.Add("TempSort", _sortDefinitionTemporary.ParseableItems().FinishParseable());
        }

        var cursorPos = CellCollection.KeyOfCell(CursorPosColumn?.Column, CursorPosRow?.Row);
        if (!string.IsNullOrEmpty(cursorPos)) {
            result.Add("CursorPos", cursorPos);
        }

        OnViewSaving(new JsonEventArgs(string.Empty, result));

        return result;
    }

    internal static void NotEditableInfo(string reason) {
        if (string.IsNullOrEmpty(reason)) { return; }
        Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);
        QuickNote.Show(NoteSymbols.Critical, "Nicht möglich");
    }

    internal static void RepairColumnArrangements(Table tb) {
        if (!string.IsNullOrEmpty(tb.IsGenericEditable(false))) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        for (var z = 0; z < Math.Max(2, tcvc.Count); z++) {
            if (tcvc.Count < z + 1) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }
            tcvc[z].Repair(z);
        }

        tb.ColumnArrangements = tcvc.AsReadOnly();
    }

    internal static string UserEdited(TableView table, string newValue, ColumnViewItem? cellInThisTableColumn, RowTableElement? cellInThisTableRow, bool formatWarnung) {
        if (cellInThisTableColumn?.Column is not { IsDisposed: false } contentHolderCellColumn) { return "Spalte nicht vorhanden"; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        var contentHolderCellRow = cellInThisTableRow?.Row;
        if (contentHolderCellRow is { IsDisposed: false } cellRow && contentHolderCellColumn.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = cellRow.LinkedCellData(contentHolderCellColumn, true, true);
            if (contentHolderCellColumn is null || contentHolderCellRow is null) { return "Spalte/Zeile nicht vorhanden"; } // Dummy prüfung
        }

        #endregion

        #region Format prüfen

        if (formatWarnung) {
            var formatReason = newValue.IsFormat(contentHolderCellColumn, contentHolderCellColumn.MultiLine);
            if (formatReason is { Length: > 0 }) {
                if (Forms.MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format:<br><b>" + formatReason + "</b><br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    return "Abbruch, da das erwartete Format nicht eingehalten wurde: " + formatReason;
                }
            }
        }

        #endregion

        #region Info über Abwandlungen

        var tmpnewValue = contentHolderCellColumn.AutoCorrect(newValue, false);

        if (tmpnewValue != newValue.Replace("\r\n", "\r")) {
            QuickNote.Show(NoteSymbols.Pencil, "Eingabe automatisch korrigiert");
        }
        newValue = tmpnewValue;

        #endregion

        #region neue Zeile anlegen? (Das ist niemals in der ein LinkedCell-Tabelle)

        if (cellInThisTableRow is null) {
            if (string.IsNullOrEmpty(newValue)) { return string.Empty; }
            if (cellInThisTableColumn.Column?.Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }
            if (table.Table?.Column.First is not { IsDisposed: false } colfirst) { return "Keine Erstspalte definiert."; }

            using var filterColNewRow = new FilterCollection(table.Table, "Edit-Filter");
            filterColNewRow.AddIfNotExists(table.FilterCombined);
            filterColNewRow.RemoveOtherAndAdd(new FilterItem(colfirst, FilterType.Istgleich, newValue));

            var newChunkVal = filterColNewRow.ChunkVal;
            var fe = table.IsCellEditableInView(cellInThisTableColumn, null, newChunkVal, true);
            if (string.IsNullOrWhiteSpace(fe)) {
                fe = Table.IsCellEditable(cellInThisTableColumn?.Column, null, newChunkVal, false);
            }
            if (!string.IsNullOrEmpty(fe)) { return fe; }

            var nr = tb.Row.GenerateAndAdd([.. filterColNewRow], "Neue Zeile über Tabellen-Ansicht");

            if (nr.IsFailed || nr.Value is not RowItem newRow) { return nr.FailedReason; }

            if (!table.FilterCombined.Rows.Contains(newRow)) {
                if (Forms.MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                    table.PinAdd(newRow);
                }
            }

            var rd = table.GetRow(newRow, null);
            table.CursorPos_Set(table.View_ColumnFirst(), rd, true);

            return string.Empty;
        }

        #endregion

        if (contentHolderCellRow is not null) {
            var oldval = contentHolderCellRow.CellGetString(contentHolderCellColumn);

            if (newValue == oldval) { return string.Empty; }

            var newChunkVal = cellInThisTableRow.Row.ChunkValue;

            if (cellInThisTableColumn.Column == cellInThisTableColumn.Column.Table?.Column.ChunkValueColumn) {
                newChunkVal = newValue;
            }

            var check1 = table.IsCellEditableInView(cellInThisTableColumn, cellInThisTableRow, newChunkVal, true);
            if (string.IsNullOrWhiteSpace(check1)) {
                check1 = Table.IsCellEditable(cellInThisTableColumn?.Column, cellInThisTableRow?.Row, newChunkVal, false);
            }
            if (!string.IsNullOrEmpty(check1)) { return check1; }

            var cellResult = contentHolderCellRow.CellSet(contentHolderCellColumn, newValue, "Benutzerbearbeitung in Tabellenansicht");
            if (!string.IsNullOrEmpty(cellResult)) { return cellResult; }

            if (contentHolderCellColumn.SaveContent) {
                contentHolderCellRow.UpdateRow(true, "Nach Benutzereingabe");
            } else {
                // Variablen sind en nicht im Script enthalten, also nur die schnelle Berechnung
                contentHolderCellRow.InvalidateCheckData();
                contentHolderCellRow.CheckRow();
            }

            if (cellInThisTableColumn is { } citc && table.Table == citc.Column?.Table) { table.CursorPos_Set(citc, cellInThisTableRow, false); }
        }

        return string.Empty;
    }

    /// <summary>
    /// Universeller Einstieg für alle Inline-Edits: wählt die Strategie, konfiguriert das Control und aktiviert es.
    /// </summary>
    internal void BeginEdit(
        string editStrategyKey,
        Rectangle bounds,
        string value,
        Action<string> commit,
        IColumnInputFormat? styleSource,
        ColumnItem? contentColumn,
        RowItem? contentRow,
        List<ListItem>? listItems,
        CellExtEventArgs? cellInfo) {
        if (IsDisposed) { return; }

        // LostFocus während des Aufbaus ignorieren (Abbau/Fokus-Übergabe).
        _isBeginningEdit = true;
        try {
            HideAllEditControls();

            var strategyParameter = styleSource?.ControlStrategyParameter ?? string.Empty;
            var strategy = GetOrCreateControlStrategy(editStrategyKey, strategyParameter);

            if (strategy is NoneControlStrategy) {
                NotEditableInfo("Diese Spalte kann nicht bearbeitet werden.");
                return;
            }

            if (strategy is DragDropControlStrategy) {
                NotEditableInfo("Werte ändern sich automatisch durch\r\nVerschieben der Zeilen.");
                return;
            }

            if (strategy.Control is not Control c) { return; }

            // Vorschläge ermitteln, falls keine Items übergeben wurden.
            var items = listItems;
            if (strategy.SupportsSuggestions && items is not { Count: > 0 }) {
                items = CollectEditItems(contentColumn, styleSource as ColumnItem, contentRow, cellInfo);
            }

            // Auswahl-Strategie ohne Items und ohne Text-Fähigkeit: auf Textfeld zurückfallen.
            if (strategy.SupportsSuggestions && items is not { Count: > 0 } && !strategy.SupportsTextEdit) {
                strategy = GetOrCreateControlStrategy(TextBoxControlStrategy.ClassId, string.Empty);
                if (strategy.Control is not Control fallbackControl) { return; }
                c = fallbackControl;
            }

            // Style, MultiLine und QuickInfo aus der Style-Quelle ableiten.
            strategy.BeginInit();
            if (styleSource is not null) { strategy.GetStyleFrom(styleSource); }
            strategy.TextInputAllowed = styleSource?.EditableWithTextInput ?? false;
            strategy.ParseJson(strategyParameter);

            strategy.Zoom = Zoom;
            strategy.QuickInfo = (styleSource as IReadableTextWithKey)?.QuickInfo ?? string.Empty;
            strategy.ParentHeight = bounds.Height;
            if (items is { Count: > 0 }) { strategy.ListItems = items; }

            // Dropdown: Mehrfachauswahl und Auto-Sortierung aktivieren.
            if (strategy.SupportsSuggestions && contentColumn is { } cc && cc.MayHaveDropDown()) {
                strategy.CheckBehavior = CheckBehavior.MultiSelection;
                strategy.AutoSort = true;
            }

            strategy.EndInit();

            // Strategie-spezifische Größe berechnen.
            var size = strategy.CalculateRequiredSize(bounds.Width, bounds.Height);

            // TableView hat das letzte Wort: das Control darf nicht über den
            // sichtbaren Bereich hinausragen.
            var area = AvailableControlPaintArea;
            size.Width = Math.Min(size.Width, Math.Max(area.Right - bounds.X, 1));
            size.Height = Math.Min(size.Height, Math.Max(area.Bottom - bounds.Y, 1));

            c.Location = bounds.Location;
            c.Size = size;
            strategy.SetValueToControl(value);

            _editCommit = commit;

            c.Visible = true;
            c.BringToFront();
            c.Focus();
        } finally {
            _isBeginningEdit = false;
        }
    }

    internal void BeginSmoothScrollToColumn(int targetX, int targetY) {
        var savedOX = OffsetX;
        var savedOY = OffsetY;
        Invalidate_CurrentArrangement();
        UpdateSliderBounds();
        if (OffsetX != savedOX) { OffsetX = savedOX; }
        if (OffsetY != savedOY) { OffsetY = savedOY; }
        _pendingSmoothScroll = false;
        Invalidate();
        SmoothScrollTo(targetX, targetY);
    }

    internal void ContextMenu_ContentPaste(object? sender, ContextMenuEventArgs? e) {
        if (CursorPosColumn?.Column is not { IsDisposed: false } column || CursorPosRow?.Row is not { IsDisposed: false } row) {
            NotEditableInfo("Interner Fehler.");
            return;
        }

        if (!Clipboard.ContainsText()) {
            NotEditableInfo("Kein Text in der Zwischenablage.");
            return;
        }
        var ntxt = Clipboard.GetText();
        if (row.CellGetString(column) == ntxt) { return; }
        NotEditableInfo(UserEdited(this, ntxt, CursorPosColumn, CursorPosRow, true));
    }

    internal void EnsureVisibleX(int controlX) {
        if (CurrentArrangement is not { } ca) { return; }

        var controlLeft = ca.ControlColumnsPermanentWidth();
        var controlWidth = AvailableControlPaintArea.Right; // Bottom = Height

        if (controlX < controlLeft) {
            OffsetX = OffsetX - controlX + controlLeft;
        } else if (controlX > controlWidth) {
            OffsetX = OffsetX - controlX + controlWidth;
        }
    }

    internal void EnsureVisibleY(int controlY) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        _ = AllViewItems;

        var controlTop = RowsAreaTop();

        var controlHeight = AvailableControlPaintArea.Bottom; // Bottom = Height

        if (controlY < controlTop) {
            OffsetY = OffsetY - controlY + controlTop;
        } else if (controlY > controlHeight) {
            OffsetY = OffsetY - controlY + controlHeight;
        }
    }

    /// <summary>
    /// Liefert alle Zeilen des Kapitel-Blocks unter dem Header — auch eingeklappte. Fallback: Suche in _sortedViewItems bis zum nächsten Header.
    /// </summary>
    internal List<RowItem>? GetChapterBlockRows(RowCaptionTableElement header) {
        if (_chapterBlockRows.TryGetValue(header, out var mapped) && mapped is not null) {
            return mapped;
        }

        var rows = new List<RowItem>();
        var found = false;

        foreach (var item in _sortedViewItems) {
            if (!found) {
                if (ReferenceEquals(item, header)) { found = true; }
                continue;
            }

            // Der nächste Kapitel-Header beendet den Block.
            if (item is RowCaptionTableElement) { break; }

            if (item is RowTableElement rli && rli.Row is { IsDisposed: false } row) {
                rows.Add(row);
            }
        }

        return found ? rows : null;
    }

    internal void Invalidate_AllViewItems(bool andclear) {
        _mustDoAllViewItems = true;
        _sortedViewItems = [];
        _cachedRowViewItems = [];
        _rowLookup.Clear();
        if (andclear) {
            _allViewItems.Clear();
        } else {
            try {
                var keysToRemove = new List<string>();
                foreach (var kvp in _allViewItems) {
                    if (kvp.Value is RowTableElement rli && rli.Row.IsDisposed) {
                        keysToRemove.Add(kvp.Key);
                    } else if (kvp.Value is IDisposableExtended extendedRli && extendedRli.IsDisposed) {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove) {
                    _allViewItems.Remove(key);
                }
            } catch {
                _allViewItems.Clear(); // Tja, geht wohl nicht anders.
            }
        }
        Invalidate_MaxBounds();
        Invalidate();
    }

    internal void Invalidate_CurrentArrangement() {
        CurrentArrangement = null;
        Invalidate_AllViewItems(false); // Spaltenbreite, Slider
        Invalidate();
    }

    /// <summary>
    /// Prüft, ob das Arrangement die Ansicht 0 ("Alle Spalten") ist, in der keine Reihenfolgeänderung erlaubt ist.
    /// </summary>
    internal bool IsAnsicht0(ColumnViewCollection ca) {
        if (Table is not { IsDisposed: false } tb) { return false; }
        if (tb.ColumnArrangements.Count <= 0) { return false; }
        return string.Equals(tb.ColumnArrangements[0].KeyName, ca.KeyName, StringComparison.OrdinalIgnoreCase);
    }

    internal void RowCleanUp() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        var l = new RowCleanUp(this);
        l.Show();
    }

    internal void SetPendingSmoothScroll() {
        _pendingSmoothScroll = true;
        _mustDoAllViewItems = true;
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var x = AvailableControlPaintArea.Width;
        var y = AvailableControlPaintArea.Height;

        // WICHTIG: CurrentArrangement VOR AllViewItems abfragen — sonst wird mit veralteter Spaltenbreite gerechnet.
        if (CurrentArrangement is { } ca) {
            // Indent in die Canvas-Breite einfließen, sonst werden eingerückte Spalten abgeschnitten.
            x = (int)ca.ControlColumnsWidth().ControlToCanvas(Zoom) + TableElement.IndentWidth * MaxIndentOfRows;
        }

        // _sortedViewItems sicherstellen.
        _ = AllViewItems;

        if (_sortedViewItems is { Count: > 0 }) {
            // Höhe aus den mit korrektem Zoom berechneten CanvasPosition-Werten.
            y = _sortedViewItems.Max(i => i.CanvasPosition.Bottom);
        }

        return new RectangleF(0, 0, x + 8, y + 8);
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                AutoFilterClicked = null;
                CellClicked = null;
                FilterCombinedChanged = null;
                PinnedChanged = null;
                SelectedCellChanged = null;
                SelectedRowChanged = null;
                TableChanged = null;
                ViewChanged = null;
                ViewLoading = null;
                ViewSaving = null;
                VisibleRowsChanged = null;

                AutoFilter_Close();

                FilterCombined.Dispose();
                FilterFix.Dispose();
                Filter.Dispose();

                HideAllEditControls();
                _editCommit = null;
                _controlStrategyCache.Dispose();

                Table = null; // Wichtig um Events zu lösen
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        if (_pendingSmoothScroll) { return; }

        // Ein Paint-Zyklus beginnt: das Batching-Flag für RowAdded kann zurückgesetzt
        // werden, sodass der nächste Bulk-Block wieder eine Invalidierung auslöst.
        _pendingRowAddedRebuild = false;

        if (InvokeRequired) {
            Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }
        base.DrawControl(gr, state);

        // Haupthintergrund der gesamten Tabelle zeichnen
        Skin.Draw_Back(gr, Design.Table_And_Pad, state, base.DisplayRectangle, this, true);

        if (_tableDrawError is { } dt) {
            if (DateTime.UtcNow.Subtract(dt).TotalSeconds < 5) {
                DrawWaitScreen(gr, "5 Sekunden Sperre");
                return;
            }
            _tableDrawError = null;
            Invalidate_AllViewItems(true);// Nach Cooldown: Recalc erzwingen, damit der Retry echte Daten liefert
        }

        //// Listboxen bekommen keinen Focus, also Tabellen auch nicht. Basta.
        //if (state.HasFlag(States.Standard_HasFocus)) {
        //    state ^= States.Standard_HasFocus;
        //}

        if (Table is not { IsDisposed: false } tb) {
            DrawWaitScreen(gr, "Keine Tabelle geladen.");
            return;
        }

        tb.LastUsedDate = DateTime.UtcNow;

        if (DesignMode || ShowWaitScreen) {
            DrawWaitScreen(gr, string.Empty);
            return;
        }

        try {
            if (CurrentArrangement is not { IsDisposed: false } ca) {
                DrawWaitScreen(gr, "Ansicht nicht definiert");
                return;
            }

            if (!ca.RenderingItems.Any()) {
                if (tb.Column.Count > 0) {
                    DrawWaitScreen(gr, "Ansicht nicht definiert");
                    return;
                }
                DrawWaitScreen(gr, "Keine Spalten vorhanden");
                return;
            }

            if (!FilterCombined.IsOk()) {
                DrawWaitScreen(gr, FilterCombined.ErrorReason());
                return;
            }

            if (FilterCombined.Table is not null && Table != FilterCombined.Table) {
                DrawWaitScreen(gr, "Filter fremder Tabelle: " + FilterCombined.Table.Caption);
                return;
            }

            if (AllViewItems is not { } avi) {
                DrawWaitScreen(gr, "Fehler der angezeigten Zeilen");
                return;
            }

            avi.TryGetValue(TableEndTableElement.Identifier, out var teli);

            if (teli is not TableEndTableElement || !teli.Visible) {
                DrawWaitScreen(gr, "Fehler in der Zeilenberechung");
                _tableDrawError = DateTime.UtcNow; // Cooldown aktivieren statt Invalidate-Loop
                return;
            }

            if (ca.ShowHead) {
                avi.TryGetValue(ColumnsHeadTableElement.Identifier, out var rcli);

                if (rcli is not ColumnsHeadTableElement rowcap || !rowcap.IsVisible(AvailableControlPaintArea, Zoom, OffsetX, OffsetY)) {
                    DrawWaitScreen(gr, "Fehler in der Zeilenberechung");
                    _tableDrawError = DateTime.UtcNow; // Cooldown aktivieren statt Invalidate-Loop
                    return;
                }
            }

            if (state.HasFlag(States.Standard_Disabled)) { CursorPos_Reset(); }

            ca.SheetStyle = SheetStyle;
            // Indent-Breite abziehen, damit eingerückte Zeilen sichtbar bleiben.
            var availWidth = AvailableControlPaintArea.Width - TableElement.IndentWidth.CanvasToControl(Zoom) * MaxIndentOfRows;
            ca.ComputeAllColumnPositions(Math.Max(16, availWidth), Zoom);

            // Haupt-Aufbau: Zeilen zeichnen, dann Kopfzeilen darüber. Lazy Where-Enumeratoren.
            var rowsTop = RowsAreaTop();
            DrawItems(_sortedViewItems.Where(i => !i.IgnoreYOffset), gr, AvailableControlPaintArea, OffsetX, OffsetY, state, Design.Table_And_Pad, Design.Item_ListBox, Zoom, rowsTop);
            DrawItems(_sortedViewItems.Where(i => i.IgnoreYOffset), gr, AvailableControlPaintArea, OffsetX, OffsetY, state, Design.Table_And_Pad, Design.Item_ListBox, Zoom, 0);

            if (!string.IsNullOrEmpty(Table.FreezedReason)) {
                var i = QuickImage.Get(ImageCode.Schloss, 48);
                gr.DrawImageUnscaled(i, 10, 10);
                var fa = BlueFont.DefaultFont.Scale(2.5f);
                fa.DrawString(gr, Table.FreezedReason, 60, 15);
            }

            // Einfüge-Indikator für Drag/Drop zeichnen
            if (_isDragging && _dragInsertIndex >= 0) {
                if (_dragItem is ColumnViewItem) {
                    DrawColumnSortInsertIndicator(gr, ca);
                } else {
                    DrawRowSortInsertIndicator(gr, ca);
                }
            }

            // Rahmen um die gesamte Tabelle zeichnen
            Skin.Draw_Border(gr, Design.Table_And_Pad, state, base.DisplayRectangle);

            if (Table.AllowDuplicates) {
                CreativePad.DrawNotEditableOverlay(gr, base.DisplayRectangle, ImageCode.Information, $"ID: {tb.MyId}", States.Standard);
            }
        } catch {
            _tableDrawError = DateTime.UtcNow;
            DrawWaitScreen(gr, string.Empty);
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

    protected override void OnDoubleClick(System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            try {
                var (_mouseOverColumn, _mouseOverRow) = CellOnLastMouseDown();

                // Wenn ein DoubleClickScript definiert ist, wird bei Doppelklick
                // auf eine Datenzeile das Script ausgeführt statt bearbeitet.
                if (DoubleClickScript is { Length: > 0 } scriptKey
                    && _mouseOverRow is RowTableElement dclRli) {
                    ContextMenu_ExecuteScript(this, new ContextMenuEventArgs(
                        ItemOf(scriptKey),
                        ContextMenuItemGenerate(this, null, null, dclRli.Row, RowsVisibleUnique())));
                    return;
                }

                _mouseOverRow?.HandleDoubleClick(_mouseOverColumn, this);
            } finally {
                _isinDoubleClick = false;
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (IsDisposed
            || Table is not { IsDisposed: false }
            || CurrentArrangement is not { IsDisposed: false }
            || CursorPosColumn?.Column is not { IsDisposed: false }
            || CursorPosRow?.Row is not { IsDisposed: false }) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;
            try {
                switch (e.KeyCode) {
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
                    case Keys.PageUp: //Bildab
                    case Keys.Home:
                    case Keys.End:
                        CursorPos_Reset();
                        HideMiniToolbar();
                        break;

                    case Keys.F:
                        if (e.Modifiers == Keys.Control) {
                            OpenSearchInCells();
                        }
                        break;
                }

                // Zell-Aktionen (Ausschneiden, Kopieren, Einfügen, Editieren
                // via F2, Löschen) werden — analog zum Doppelklick — an das
                // unter dem Cursor liegende Row-Item delegiert. Die TableView
                // behält nur die Navigations-Tasten oben selbst.
                CursorPosRow?.HandleKeyDown(CursorPosColumn, this, e);
            } finally {
                _isinKeyDown = false;
            }
        }
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        // Edit wurde durch Klick geschlossen → Klick nur konsumieren, Aktion beim nächsten Klick.
        if (_consumeNextMouseDown) {
            _consumeNextMouseDown = false;
            return;
        }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            try {
                var (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e);
                // Auf-/Zuklappen nur über den Pfeil-Button, Klick auf das Wort startet Drag/Drop.
                if (_mouseOverRow is RowCaptionTableElement rcli
                    && rcli.IsArrowButtonHit(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY)) {
                    CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

                    ToggleChapterExpanded(rcli);
                    Invalidate_AllViewItems(false);
                }
                EnsureVisible(_mouseOverColumn, _mouseOverRow);
                CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);

                // Drag/Drop-Potential speichern; Drag startet in OnMouseMove nach Bewegungsschwelle.
                _dragItem = null;

                if (e.Button == MouseButtons.Left && !IsAnsicht0(ca)
                    && _mouseOverColumn is { IsDisposed: false } dragCvi && dragCvi.IsOk()) {
                    var mc = dragCvi.Column;
                    if (_mouseOverRow is RowTableElement dragRli
                        && dragRli.Row is { IsDisposed: false } dragRow
                        && !PinnedRows.Contains(dragRow)
                        && Table.Column.SysRowSortIndex is { IsDisposed: false }
                        && string.IsNullOrEmpty(TableView.IsCellEditable(mc, dragRow, dragRow.ChunkValue))) {
                        _dragItem = dragRow;
                    } else if (_mouseOverRow is RowCaptionTableElement dragRcli
                               && !dragRcli.IsArrowButtonHit(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY)
                               && dragRcli.CanEditChapter
                               && Table.Column.SysRowSortIndex is { IsDisposed: false }) {
                        // Block nicht aufklappen — GetChapterBlockRows liefert alle Zeilen auch eingeklappt.
                        _ = AllViewItems; // _sortedViewItems sicherstellen

                        // Den aktuellen Header über die Mausposition finden.
                        var blockHeader = _sortedViewItems?.OfType<RowCaptionTableElement>()
                            .FirstOrDefault(h => string.Equals(h.ChapterText, dragRcli.ChapterText, StringComparison.OrdinalIgnoreCase)
                                                 && h.ControlPosition(Zoom, OffsetX, OffsetY).Contains(e.ControlX, e.ControlY))
                            ?? dragRcli;

                        // Nur als Drag-Quelle merken, wenn der Block verschiebbare Zeilen enthält.
                        if (GetDragSourceRows(blockHeader).Count > 0) {
                            _dragItem = blockHeader;
                        }
                    } else if (_mouseOverRow is TableElement { IgnoreYOffset: true } and not NewRowTableElement
                               && Table.IsAdministrator()) {
                        _dragItem = dragCvi;
                    }

                    if (_dragItem is not null) {
                        _dragMouseDown = new Point(e.ControlX, e.ControlY);
                    }
                }
            } finally {
                _isinMouseDown = false;
            }
        }
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);

        lock (_lockUserAction) {
            if (IsDisposed || Table is not { IsDisposed: false }) { return; }
            if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

            if (_isinMouseMove) { return; }

            _isinMouseMove = true;
            try {
                // Drag/Drop: Drag starten und Einfüge-Position berechnen
                if (_dragItem is not null && e.Button == MouseButtons.Left) {
                    if (!_isDragging) {
                        var dx = Math.Abs(e.ControlX - _dragMouseDown.X);
                        var dy = Math.Abs(e.ControlY - _dragMouseDown.Y);
                        if (dx > SystemInformation.DragSize.Width / 2 || dy > SystemInformation.DragSize.Height / 2) {
                            _isDragging = true;
                            CloseAllComponents();
                        }
                    }
                    if (_isDragging) {
                        // Außerhalb des Zeichenbereichs: Markierung ausblenden.
                        // Beim Wiedereintritt wird sie in der nächsten OnMouseMove
                        // neu berechnet. AutoScroll weiterhin aktiv, damit der
                        // Benutzer zurückscrollen kann.
                        var inside = AvailableControlPaintArea.Contains(e.ControlX, e.ControlY);
                        if (_dragItem is ColumnViewItem) {
                            _dragInsertIndex = inside ? CalculateColumnSortInsertIndex(e.ControlX) : -1;
                            AutoScrollDuringDrag(e.ControlX, null);
                        } else {
                            _dragInsertIndex = inside ? CalculateRowSortInsertIndex(e.ControlY) : -1;
                            AutoScrollDuringDrag(null, e.ControlY);
                        }
                        Invalidate();
                        return;
                    }
                }

                var (_mouseOverColumn, _mouseOverRowItem) = CellOnCoordinate(ca, e);

                if (_mouseOverColumn is { IsDisposed: false } &&
                    _mouseOverRowItem is TableElement { } rbi &&
                    e.Button == MouseButtons.None) {
                    var indentOffset = TableElement.IndentWidth.CanvasToControl(Zoom) * rbi.Indent;
                    var mxInCol = e.ControlX - _mouseOverColumn.ControlColumnLeft(OffsetX) - indentOffset;
                    var myInCol = e.ControlY - rbi.ControlPosition(Zoom, OffsetX, OffsetY).Top;
                    QuickInfo = rbi.QuickInfoForColumn(_mouseOverColumn, mxInCol, myInCol, Zoom);
                } else {
                    QuickInfo = string.Empty;
                }
            } finally {
                _isinMouseMove = false;
            }
        }
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseUp(e);

        lock (_lockUserAction) {
            // Drag/Drop abschließen
            if (_isDragging) {
                // Außerhalb des Zeichenbereichs losgelassen → Drag abbrechen,
                // kein Reorder durchführen.
                if (AvailableControlPaintArea.Contains(e.ControlX, e.ControlY)) {
                    FinishDrag();
                } else {
                    _isDragging = false;
                    _dragItem = null;
                    _dragInsertIndex = -1;
                    Invalidate();
                }
                return;
            }
            _dragItem = null;

            if (Table is not { IsDisposed: false } || CurrentArrangement is not { IsDisposed: false } ca) {
                return;
            }

            var (_mouseOverColumn, _mouseOverRowItem) = CellOnCoordinate(ca, e);
            var _mouseOverRow = _mouseOverRowItem as RowTableElement;

            // TXTBox_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Design.Form_ContextMenu);

            if (_mouseOverColumn is not { IsDisposed: false }) { return; }

            var isRealColumn = _mouseOverColumn.Column is { IsDisposed: false };

            if (e.Button == MouseButtons.Left) {
                if (isRealColumn && _mouseOverRowItem is FilterBarTableElement cfli) {
                    var screenX = Cursor.Position.X - e.ControlX;
                    var screenY = Cursor.Position.Y - e.ControlY;
                    AutoFilter_Show(ca, _mouseOverColumn, screenX, screenY, cfli.ControlPosition(Zoom, OffsetX, OffsetY).Bottom);
                    return;
                }

                if (isRealColumn && _mouseOverRowItem is CollapesBarTableElement && _mouseOverColumn.CollapsableEnabled(SheetStyle)) {
                    _mouseOverColumn.IsExpanded = !_mouseOverColumn.IsExpanded;
                    Invalidate_AllViewItems(false);
                    return;
                }

                if (_mouseOverRowItem is TableElement rbli) {
                    var indentOffset = TableElement.IndentWidth.CanvasToControl(Zoom) * rbli.Indent;
                    var mouseXinColumn = e.ControlX - _mouseOverColumn.ControlColumnLeft(OffsetX) - indentOffset;
                    var mouseYinColumn = e.ControlY - rbli.ControlPosition(Zoom, OffsetX, OffsetY).Top;
                    if (rbli.HandleClick(ca, _mouseOverColumn, mouseXinColumn, mouseYinColumn, Zoom, this)) {
                        Invalidate_CurrentArrangement();
                        return;
                    }
                }

                if (_mouseOverColumn.Column is { IsDisposed: false } col && _mouseOverRow?.Row is { IsDisposed: false } r) {
                    OnCellClicked(new CellEventArgs(col, r));
                    Invalidate();

                    // Mini-Toolbar anzeigen. Ob sie tatsächlich erscheint oder
                    // bei einem erneuten Klick auf dieselbe Zelle ausgeblendet
                    // bleibt, entscheidet MiniToolbarShow anhand des HotItems.
                    ShowMiniToolbarAt(_mouseOverColumn, _mouseOverRowItem, r);
                }
            }

            if (e.Button == MouseButtons.Right) {
                ((IContextMenu)this).ContextMenuShow(ContextMenuItemGenerate(this, _mouseOverColumn, _mouseOverColumn.Column, _mouseOverRow?.Row, RowsVisibleUnique()));
            }
        }
    }

    protected override void OnOffsetXChanged() {
        base.OnOffsetXChanged();

        //Invalidate_CurrentArrangement();

        CloseAllComponents();
    }

    protected override void OnOffsetYChanged() {
        base.OnOffsetYChanged();

        CloseAllComponents();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;

            try {
                Invalidate_CurrentArrangement();
            } finally {
                _isinSizeChanged = false;
            }
        }
    }

    protected override void OnSliderVisibilityChanged() {
        // Wenn ein Slider ein-/ausgeblendet wird, ändert sich die
        // AvailableControlPaintArea-Breite. Die View-Items müssen neu
        // aufgebaut werden, damit CanvasPosition.Width jeder Zeile mit der
        // nun gültigen ControlColumnsWidth berechnet wird. Ohne diesen
        // Reset würde DrawExplicit (RowBackground) beim späteren Zeichnen
        // das rechte Ende der letzten Spalte mit der Hintergrundfarbe übermalen.
        _mustDoAllViewItems = true;
        _sortedViewItems = [];
    }

    protected void OnViewLoading(JsonEventArgs e) => ViewLoading?.Invoke(this, e);

    protected void OnViewSaving(JsonEventArgs e) => ViewSaving?.Invoke(this, e);

    protected override void OnZoomChanged() {
        Invalidate_CurrentArrangement();
        base.OnZoomChanged();
    }

    protected override void WndProc(ref Message m) {
        const int WM_MOUSEWHEEL = 0x020A;
        if (m.Msg == WM_MOUSEWHEEL && ActiveControlStrategy is not null) {
            return;
        }
        base.WndProc(ref m);
    }

    private static void CalculateAllViewItems_AddCaptions(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement, List<RowItem> filteredRows) {
        HashSet<string> allCaps = [];

        // Einheitliche Behandlung: Kapitel-Pfade werden immer hierarchisch
        // ausgewertet — der Kapitel-Trenner '\' wird nie ignoriert. Aus jedem
        // Kapitel-Wert werden alle Vorfahren-Pfade abgeleitet, sodass für
        // "A\B\C" die Header "A", "A\B" und "A\B\C" erzeugt werden.
        if (arrangement.ColumnForChapter is { IsDisposed: false } cap) {
            var caps = cap.Contents(filteredRows);

            foreach (var capValue in caps) {
                allCaps.UnionWith(capValue.ChapterPathHierarchy());
            }
        }

        foreach (var thisCap in allCaps) {
            var capId = RowCaptionTableElement.Identifier(thisCap);
            if (allItems.TryGetValue(capId, out var existingCap) && existingCap is RowCaptionTableElement existingRcli) {
                existingRcli.Arrangement = arrangement;
            } else {
                var capi = new RowCaptionTableElement(thisCap, arrangement);
                allItems[capi.KeyName] = capi;
            }
        }
    }

    /// <summary>
    /// Kapitel-Zuordnungen einer Zeile. Angepinnte Zeilen erhalten zusätzlich einen leeren Marker für die Darstellung ganz oben.
    /// </summary>
    private static List<string> CapsOfRow(RowItem row, bool isfiltered, bool isPinned, ColumnViewCollection arrangement) {
        List<string> capsOfRow = [];

        if (isfiltered) {
            capsOfRow = arrangement.ColumnForChapter is { IsDisposed: false } sc ? row.CellGetList(sc) : [];
            capsOfRow.Remove(string.Empty);
        }

        // Angepinnte Zeilen erscheinen doppelt: ganz oben (über den leeren
        // Marker, der via MarkYellow als "Angepinnt" erkannt wird) UND an
        // ihrem regulären Platz. Beide Male gelb markiert.
        // Ist die reguläre Position bereits der leere String (z. B. bei
        // gefilterten Zeilen ohne Kapitel), keinen weiteren Eintrag ergänzen —
        // derselbe Eintrag wird sonst zweimal angezeigt.
        if (isPinned) {
            if (capsOfRow.Count == 0 && isfiltered) { capsOfRow.Add(string.Empty); }
            if (!capsOfRow.Contains(string.Empty)) { capsOfRow.Add(string.Empty); }
        }

        if (capsOfRow.Count == 0) { capsOfRow.Add(string.Empty); }

        return capsOfRow;
    }

    private static void ContextMenu_Note_Edit(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, tableView, _) = GetContextData(e.HotItem);
        if (column is null || row is null) { return; }
        if (column.Table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.SysCellNote is null) {
            QuickNote.Show(NoteSymbols.Warning, "Keine Notizspalte vorhanden");
            return;
        }

        var existing = CellNoteHelper.GetNoteData(column, row);
        var note = new NoteEntry();
        if (existing.HasValue) {
            note.Symbol = existing.Value.Symbol;
            note.Note = existing.Value.Text;
        }
        InputBoxEditor.Edit(note, true);

        if (string.IsNullOrEmpty(note.Note)) {
            CellNoteHelper.RemoveNote(column, row);
        } else {
            CellNoteHelper.SetNote(column, row, note.Symbol, note.Note);
        }

        tableView?.Invalidate();
    }

    private static void ContextMenu_Note_Remove(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, tableView, _) = GetContextData(e.HotItem);
        if (column is null || row is null) { return; }
        if (column.Table is not { IsDisposed: false }) { return; }

        CellNoteHelper.RemoveNote(column, row);
        tableView?.Invalidate();
    }

    /// <summary>
    /// Erzeugt die Strategie zum übergebenen Strategy-Key (ClassId).
    /// </summary>
    private static ControlStrategy CreateControlStrategy(string editStrategyKey) => ControlStrategy.CreateNew(editStrategyKey);

    private static void DoScript(List<RowItem> rows, bool generic, TableScriptDescription? sc, string info) {
        var info2 = $"<b><u>{info}:</b></u>\r\n\r\n";

        if (rows.Count == 0) {
            Forms.MessageBox.Show($"{info2}Keine Zeilen zum Abarbeiten vorhanden.", ImageCode.Kreuz, "OK");
            return;
        }

        if (rows[0]?.Table is not { IsDisposed: false } tb) {
            Forms.MessageBox.Show($"{info2}Tabelle verworfen", ImageCode.Kreuz, "OK");
            return;
        }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) {
            Forms.MessageBox.Show($"{info2}{f}", ImageCode.Kreuz, "OK");
            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
            return;
        }

        if (!generic && sc is null) {
            Forms.MessageBox.Show($"{info2}Interner Programmfehler,\r\nkein Skript angekommen.", ImageCode.Kreuz, "OK");
            return;
        }
        foreach (var row in rows) {
            if (row.Table != tb) {
                Forms.MessageBox.Show($"{info2}Interner Programmfehler\r\nZeilen aus unterschiedlichen Datenbanken.", ImageCode.Kreuz, "OK");
                return;
            }
        }

        if (rows.Count > 1) {
            var t = string.Empty;

            var tmpsc = sc;

            if (generic) {
                var l = tb.EventScript.Get(ScriptEventTypes.value_changed);

                if (l.Count == 1) { tmpsc = l[0]; }
            }

            if (tmpsc is not null && tmpsc.StoppedTimeCount > 20) {
                var tm = Math.Round(tmpsc.AverageRunTime / 1000f * rows.Count / 60f, 1);
                t = $"\r\n<i>(geschätzte Dauer: {tm} Minuten)<i>";
            }

            if (Forms.MessageBox.Show($"<b>{info}</b>\r\nfür {rows.Count} Zeilen ausführen?{t}", ImageCode.Information, "Ja", "Nein") != 0) {
                //Forms.MessageBox.Show($"{info2}Abbruch durch Benutzer.", ImageCode.Information, "OK");
                QuickNote.Show(NoteSymbols.Critical, "Abbruch durch Benutzer");
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                return;
            }
        }

        var fehler = new List<ScriptEndedFeedback>();
        Progressbar? _pg = null;

        if (rows.Count > 3) {
            _pg = Progressbar.Show(info, rows.Count);
            _pg.CancelSupported = true;
        }

        var firstRow = rows[0];
        var all = rows.Count;
        var c = 0;
        while (rows.Count > 0) {
            Develop.Message(ErrorType.Info, tb, "Table", ImageCode.Skript, $"{info}: {rows[0].ReadableText()}", 0);

            _pg?.Update(c++);
            Develop.DoEvents();

            if (_pg is { IsCancelRequested: true }) {
                _pg.Close();
                QuickNote.Show(NoteSymbols.Critical, "Abbruch durch Benutzer");
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                return;
            }

            if (!tb.CanDoValueChangedScript(true)) {
                _pg?.Close();
                Forms.MessageBox.Show($"{info2}Abbruch, Skriptfehler sind aufgetreten.", ImageCode.Warnung, "OK");
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                return;
            }

            rows[0].InvalidateCheckData();

            ScriptEndedFeedback? fb;
            if (generic) {
                rows[0].InvalidateRowState($"TableView, Kontextmenü, {info}");
                fb = rows[0].UpdateRow(true, $"TableView, Kontextmenü, {info}");
            } else {
                fb = rows[0].Table?.ExecuteScript(null, sc?.KeyName ?? string.Empty, true, rows[0], null, true, true, 0);
            }

            if (fb?.Failed == true) {
                fehler.Add(fb);
            }

            rows.RemoveAt(0);
        }

        _pg?.Close();

        if (all == 1) {
            if (fehler.Count == 1) {
                Forms.MessageBox.Show($"{info2}<b>Es ist ein Skript-Fehler aufgetreten.</b>\r\n\r\n{fehler[0].ProtocolText}", ImageCode.Warnung, "Ok");
            } else {
                if (generic) {
                    Forms.MessageBox.Show($"{info2}{firstRow.CheckRow().Message}", ImageCode.HäkchenDoppelt, "Ok");
                } else {
                    Forms.MessageBox.Show($"{info2}Erfolgreich ausgeführt.", ImageCode.HäkchenDoppelt, "Ok");
                }
            }
        } else {
            if (fehler.Count > 0) {
                Forms.MessageBox.Show($"{info2}Alle {all} Zeilen abgearbeitet.\r\nEs sind in {fehler.Count} Zeile(n) Skript-Fehler aufgetreten", ImageCode.Warnung, "OK");
            } else {
                Forms.MessageBox.Show($"{info2}Alle {all} Zeilen erfolgreich abgearbeitet.", ImageCode.HäkchenDoppelt, "OK");
            }
        }
    }

    /// <summary>
    /// Zeichnet ein halbtransparentes Rechteck als Drag/Drop-Einfüge-Indikator.
    /// </summary>
    private static void DrawInsertIndicatorRect(Graphics gr, Rectangle rect) {
        using var brush = new SolidBrush(Color.FromArgb(40, 0, 120, 215));
        gr.FillRectangle(brush, rect);
        using var pen = new Pen(Color.FromArgb(200, 0, 120, 215), 2);
        gr.DrawRectangle(pen, rect);
    }

    private static void DrawItems(IEnumerable<TableElement>? list, Graphics gr, Rectangle visControlArea, int offsetX, int offsetY, States controlState, Design controlDesign, Design itemDesign, float zoom, int clipTop) {
        if (list is null) { return; }

        try {
            foreach (var thisItem in list) {
                if (!thisItem.IsVisible(visControlArea, zoom, offsetX, offsetY)) { continue; }

                if (clipTop > 0 && thisItem.ControlPosition(zoom, offsetX, offsetY).Bottom <= clipTop) { continue; }

                var itemState = controlState;

                if (!thisItem.Enabled || controlState.HasFlag(States.Standard_Disabled)) { itemState = States.Standard_Disabled; }

                thisItem.Draw(gr, visControlArea, offsetX, offsetY, controlDesign, itemDesign, itemState, true, string.Empty, false, Design.Undefined, zoom);
            }
        } catch { }
    }

    /// <summary>
    /// Holt ein Kopf-Element aus allItems oder legt es neu an. IgnoreYOffset wird zentral auf true gesetzt.
    /// </summary>
    private static T GetOrCreateHeadItem<T>(Dictionary<string, TableElement> allItems, string identifier, ColumnViewCollection arrangement, Func<T> factory) where T : TableElement {
        if (allItems.TryGetValue(identifier, out var existing) && existing is T typed) {
            typed.Arrangement = arrangement;
            typed.IgnoreYOffset = true;
            return typed;
        }
        var item = factory();
        allItems.Add(item.KeyName, item);
        item.IgnoreYOffset = true;
        return item;
    }

    /// <summary>
    /// Prüft, ob ein Vorgänger-Pfad von chapterText in collapsedParents enthalten ist.
    /// </summary>
    private static bool HasCollapsedAncestor(string chapterText, HashSet<string> collapsedParents) {
        var pos = chapterText.IndexOf(RowCaptionTableElement.Kapiteltrenner);
        while (pos >= 0) {
            if (collapsedParents.Contains(chapterText[..pos])) { return true; }
            pos = chapterText.IndexOf(RowCaptionTableElement.Kapiteltrenner, pos + 1);
        }
        return false;
    }

    /// <summary>
    /// true bei sichtbaren, nicht disposed Zeilen oder Kapitel-Headern.
    /// </summary>
    private static bool IsDroppableTarget(TableElement item)
        => item is RowTableElement or RowCaptionTableElement && !item.IsDisposed && item.Visible;

    /// <summary>
    /// Liefert den nächsten freien Default-Wert "NEU_X" für die Spalte.
    /// </summary>
    private static string NextNewDefaultValue(Table tb, ColumnItem column) {
        var n = 1;
        while (true) {
            var candidate = "NEU_" + n.ToString1();
            var exists = false;
            foreach (var r in tb.Row) {
                if (r is { IsDisposed: false } && string.Equals(r.CellGetString(column), candidate, StringComparison.OrdinalIgnoreCase)) {
                    exists = true;
                    break;
                }
            }
            if (!exists) { return candidate; }
            n++;
        }
    }

    /// <summary>
    /// Einzelelement-Liste bei gesetzter row, sonst Kopie aller rows.
    /// </summary>
    private static List<RowItem> RowsFromContext(RowItem? row, IReadOnlyList<RowItem> rows)
        => row is not null ? [row] : [.. rows];

    private void _table_Disposed(object? sender, System.EventArgs e) => Table = null;

    private void _Table_SortParameterChanged(object? sender, System.EventArgs e) => Invalidate_AllViewItems(false);

    private void _Table_StoreView(object? sender, System.EventArgs e) => _storedView = ViewToJson();

    private void _Table_TableLoaded(object? sender, FirstEventArgs e) {
        if (IsDisposed) { return; }

        if (e.IsFirst) {
            if (_storedView is not null) {
                SetView(_storedView);
                _storedView = null;
            } else {
                ResetView();
            }
        } else {
            _storedView = null;
        }

        //Invalidate_AllViewItems(false); // Neue Zeilen können nun erlaubt sein
        Invalidate_CurrentArrangement(); // Spaltenbreite, Slider
        CheckView();
    }

    private void _Table_ViewChanged(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        if (_pendingSmoothScroll) {
            _mustDoAllViewItems = true;
            return;
        }

        var savedOX = OffsetX;
        var savedOY = OffsetY;

        OnViewChanged();
        UpdateSliderBounds();

        if (savedOX != OffsetX) { OffsetX = savedOX; }
        if (savedOY != OffsetY) { OffsetY = savedOY; }

        CursorPos_Set(CursorPosColumn, CursorPosRow, false);
    }

    private void AutoFilter_Close() {
        if (_autoFilter is not null) {
            _autoFilter.FilterCommand -= AutoFilter_FilterCommand;
            _autoFilter.Dispose();
            _autoFilter = null;
        }
    }

    private void AutoFilter_FilterCommand(object? sender, FilterCommandEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

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
                RowCollection.GetUniques(e.Column, _rowsVisibleUnique, out var einzigartig, out _);
                if (einzigartig.Count > 0) {
                    Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, einzigartig));
                    Notification.Show("Die aktuell einzigartigen Einträge wurden berechnet<br>und als <b>ODER-Filter</b> gespeichert.", ImageCode.Trichter);
                } else {
                    Notification.Show("Filterung dieser Spalte gelöscht,<br>da <b>alle Einträge</b> mehrfach vorhanden sind.", ImageCode.Trichter);
                }
                break;

            case "donichteinzigartig":
                Filter.Remove(e.Column);
                RowCollection.GetUniques(e.Column, _rowsVisibleUnique, out _, out var xNichtEinzigartig);
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
            //        foreach (var thisColumnItem in e.Column.Table.Column) {
            //            if (thisColumnItem is not null && thisColumnItem != e.Column) { ic.Add(thisColumnItem); }
            //        }
            //        ic.Sort();

            //        var r = InputBoxListBoxStyle.Show("Mit welcher Spalte vergleichen?", ic, AddType.None, true);
            //        if (r is null || r.Count == 0) { return; }

            //        var c = e.Column.Table.Column[r[0]);

            //        List<string> d = new();
            //        foreach (var thisR in ro) {
            //            if (thisR.CellGetString(e.Column) != thisR.CellGetString(c)) { d.Add(thisR.CellFirstString()); }
            //        }
            //        if (d.Count > 0) {
            //            Filter.Add(new FilterItem(e.Column.Table.Column.First, FilterType.Istgleich_ODER_GroßKleinEgal, d));
            //            Notification.Show("Die aktuell <b>unterschiedlichen</b> Einträge wurden berechnet<br>und als <b>ODER-Filter</b> in der <b>ersten Spalte</b> gespeichert.", ImageCode.Trichter);
            //        } else {
            //            Notification.Show("Keine Filter verändert,<br>da <b>alle Einträge</b> identisch sind.", ImageCode.Trichter);
            //        }
            //        break;
            //    }

            case "doclipboard": {
                    var clipTmp = Clipboard.GetText().Replace('\n', '\r').RemoveChars(Char_NotFromClip).TrimEnd('\r', '\n');
                    Filter.Remove(e.Column);

                    var searchValue = new List<string>(clipTmp.SplitAndCutByCr()).SortedDistinctList();

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, searchValue));
                    }
                    break;
                }

            case "donotclipboard": {
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd('\r', '\n');
                    Filter.Remove(e.Column);

                    var searchValue = e.Column.Contents();//  tb.Export_CSV(FirstRow.Without, e.Column, null).SplitAndCutByCr().SortedDistinctList();
                    searchValue.RemoveString(clipTmp.SplitAndCutByCr().SortedDistinctList(), false);

                    if (searchValue.Count > 0) {
                        Filter.Add(new FilterItem(e.Column, FilterType.Istgleich_ODER_GroßKleinEgal, searchValue));
                    }
                    break;
                }
            default:
                Develop.DebugPrint("Unbekannter Command: " + e.Command);
                break;
        }

        if (e.Filter?.Column is { IsDisposed: false } col) {
            col.AddSystemInfo("Filter Clicked", UserName);
        }

        OnAutoFilterClicked(new FilterEventArgs(e.Filter));
    }

    private void AutoFilter_Show(ColumnViewCollection ca, ColumnViewItem columnviewitem, int screenx, int screeny, int bottom) {
        if (columnviewitem.Column is null) { return; }
        if (!ca.ShowHead) { return; }
        if (!columnviewitem.AutoFilterSymbolPossible) { return; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (FilterCombined.HasAlwaysFalse()) {
            Forms.MessageBox.Show("Ein Filter, der nie ein Ergebnis zurückgibt,\r\nverhindert aktuell Filterungen.", ImageCode.Information, "OK");
            return;
        }

        var sb = new StringBuilder();
        foreach (var thisFilter in Filter) {
            if (thisFilter is not null && thisFilter.Column == columnviewitem.Column && !string.IsNullOrEmpty(thisFilter.Origin)) {
                sb.AppendLine(thisFilter.Origin);
            }
        }

        if (FilterFix is { IsDisposed: false }) {
            foreach (var thisFilter in FilterFix) {
                if (thisFilter is not null && thisFilter.Column == columnviewitem.Column) {
                    var o = thisFilter.Origin;
                    if (string.IsNullOrEmpty(o)) { o = "Ein fix gesetzer Filter"; }
                    sb.AppendLine(o);
                }
            }
        }

        var t = sb.ToString();

        if (!string.IsNullOrEmpty(t)) {
            Forms.MessageBox.Show($"<b>Dieser Filter wurde automatisch gesetzt:</b>\r\n{t}", ImageCode.Information, "OK");
            return;
        }

        var headX = columnviewitem.ControlColumnLeft(OffsetX);
        //headX = headX.CanvasToControl(Zoom, OffsetX);// ControlToCanvasX((columnviewitem.ControlX ?? 0), Zoom) - OffsetX;

        // Altes AutoFilter schließen (verhindert Event-Subscription-Leak).
        AutoFilter_Close();

        _autoFilter = new AutoFilter(columnviewitem.Column, FilterCombined, PinnedRows, columnviewitem.CanvasContentWidth(SheetStyle), columnviewitem.GetRenderer(SheetStyle));
        _autoFilter.Position_LocateToPosition(new Point(screenx + headX, screeny + bottom));
        _autoFilter.Show();
        _autoFilter.FilterCommand += AutoFilter_FilterCommand;
    }

    private void AutoScrollDuringDrag(int? controlX, int? controlY) {
        var area = AvailableControlPaintArea;
        var threshold = 20.CanvasToControl(Zoom);

        if (controlX is not null) {
            if (controlX < area.Left + threshold) {
                OffsetX += 20;
            } else if (controlX > area.Right - threshold) {
                OffsetX -= 20;
            }
        }

        if (controlY is not null) {
            var rowsTop = RowsAreaTop();

            if (controlY < rowsTop + threshold) {
                OffsetY += 20;
            } else if (controlY > area.Bottom - threshold) {
                OffsetY -= 20;
            }
        }
    }

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        tb.Edit();
    }

    private void CalculateAllViewItems(Dictionary<string, TableElement> allItems) {
        if (IsDisposed || Table is not { IsDisposed: false } tb
            || allItems is null
            || CurrentArrangement is not { } arrangement) {
            _rowsVisibleUnique = new([]);
            allItems?.Clear();
            return;
        }

        // SortUsed() ist null bei Tabellen ohne echte Spalten. Leere Sortierung lässt die Pipeline vollständig durchlaufen.
        var sortused = SortUsed() ?? new RowSortDefinition(tb, (ColumnItem?)null, false);

        if (arrangement.ControlColumnsWidth() <= 0 && arrangement.Count > 0) {
            arrangement.Invalidated = true;
            var availWidth = AvailableControlPaintArea.Width - TableElement.IndentWidth.CanvasToControl(Zoom) * MaxIndentOfRows;
            arrangement.ComputeAllColumnPositions(Math.Max(16, availWidth), Zoom);
        }

        _newRowsAllowed = UserEdit_NewRowAllowed();

        List<RowItem> pinnedRows = [.. PinnedRows];
        // filteredRows wird nur iterativ konsumiert; die echte Sortierung passiert später über UserDefCompareKey.
        List<RowItem> filteredRows = [.. FilterCombined.Rows];

        List<RowItem> allrows = [.. pinnedRows, .. filteredRows];
        allrows = [.. allrows.Distinct()];

        var sortedItems = new List<TableElement>();

        CalculateAllViewItems_AddHeadElements(allItems, arrangement, sortedItems, FilterCombined, sortused);

        CalculateAllViewItems_NewRow(allItems, arrangement, tb, _newRowsAllowed, true, sortedItems);

        CalculateAllViewItems_AddCaptions(allItems, arrangement, filteredRows);

        var visibleRowListItems = CalculateAllViewItems_Rows(allItems, arrangement, allrows, pinnedRows, sortused, filteredRows);

        CalculateAllViewItems_Collapsed(allItems);

        CalculateAllViewItems_HildeAllItems(allItems, arrangement);

        CalculateAllViewItems_AddCaptionsAndRows(allItems, sortedItems, visibleRowListItems);

        CalculateAllViewItems_NewRow(allItems, arrangement, tb, _newRowsAllowed, false, sortedItems);
        CalculateAllViewItems_AddFootElements(allItems, arrangement, sortedItems);

        CalculateAllViewItems_CalculateYPosition(sortedItems, arrangement);

        _rowsVisibleUnique = allrows;
        _sortedViewItems = sortedItems;
        _cachedRowViewItems = [.. sortedItems.OfType<RowTableElement>()];
        _rowLookup.Clear();
        foreach (var rli in _cachedRowViewItems) {
            _rowLookup.TryAdd(rli.Row, rli);
        }

        // ERST nach _cachedRowViewItems/_rowLookup: DoCursorPos benötigt die
        // aktuellen Collections, um verwaiste CursorPosRow-/CursorPosColumn-
        // Referenzen auf die neuen Instanzen migrieren zu können.
        DoCursorPos();
    }

    /// <summary>
    /// Einheitlicher Caption- und Zeilen-Aufbau. Fügt bei jedem Kapitel-Wechsel die benötigten Header inkl. Hierarchie ein.
    /// </summary>
    private void CalculateAllViewItems_AddCaptionsAndRows(Dictionary<string, TableElement> allItems, List<TableElement> sortedItems, List<RowTableElement> sortedRows) {
        _chapterBlockRows.Clear();

        var numberStyle = Table is { IsDisposed: false } tbNs && tbNs.Column.SysRowSortIndex is { IsDisposed: false };

        // Angepinnte Zeilen ganz oben — ohne Kapitel-Header (MarkYellow + leer).
        foreach (var rli in sortedRows) {
            if (rli.MarkYellow && string.IsNullOrEmpty(rli.AlignsToChapter)) {
                sortedItems.Add(rli);
            }
        }

        // _collapsed enthält nur direkt eingeklappte Kapitel; Nachfahren über HasCollapsedAncestor verbergen.
        var collapsedSet = new HashSet<string>(_collapsed, StringComparer.OrdinalIgnoreCase);

        string? lastChapter = null;
        List<RowItem>? currentBlockRows = null;
        var blockCollapsed = false;

        // NumberStyle: eingeklapptes Vorfahr-Kapitel, dessen Nachfahren ebenfalls verbergen werden.
        string? collapsedAncestor = null;

        foreach (var rli in sortedRows) {
            if (rli.MarkYellow && string.IsNullOrEmpty(rli.AlignsToChapter)) { continue; }

            var chapter = rli.AlignsToChapter;

            if (string.IsNullOrEmpty(chapter)) {
                // NumberStyle: leerer Kapitel-Wert wird als eigener Block behandelt. Sonst: Zeile ohne Header.
                if (!numberStyle || rli.Arrangement?.ColumnForChapter is not { IsDisposed: false }) {
                    sortedItems.Add(rli);
                    lastChapter = null;
                    currentBlockRows = null;
                    blockCollapsed = false;
                    collapsedAncestor = null;
                    continue;
                }
            }

            // NumberStyle: Zeile unter eingeklapptem Vorfahr verbergen, aber in den Block aufnehmen (für Drag/Drop).
            if (numberStyle && collapsedAncestor is { Length: > 0 } anc
                && chapter.StartsWith(anc + RowCaptionTableElement.Kapiteltrenner, StringComparison.OrdinalIgnoreCase)) {
                if (currentBlockRows is not null && rli.Row is { IsDisposed: false } descRow) {
                    currentBlockRows.Add(descRow);
                }
                lastChapter = chapter;
                continue;
            }

            // Kapitel-Wechsel? Header einfügen.
            if (!string.Equals(chapter, lastChapter, StringComparison.OrdinalIgnoreCase)) {
                var hierarchy = chapter.ChapterPathHierarchy();
                if (hierarchy.Count == 0) {
                    // Leeres Kapitel (nur NumberStyle): einzelner Header auf Ebene 0.
                    hierarchy = [string.Empty];
                }
                var lastHierarchy = string.IsNullOrEmpty(lastChapter) ? [] : lastChapter.ChapterPathHierarchy();

                // LCP-Tiefe (Longest Common Prefix) der beiden Hierarchien.
                var lcpDepth = 0;
                while (lcpDepth < hierarchy.Count && lcpDepth < lastHierarchy.Count
                       && string.Equals(hierarchy[lcpDepth], lastHierarchy[lcpDepth], StringComparison.OrdinalIgnoreCase)) {
                    lcpDepth++;
                }

                // Bei Rückkehr an einen Vorfahr den Leaf-Header erneut anzeigen.
                var startDepth = lcpDepth == hierarchy.Count ? Math.Max(0, hierarchy.Count - 1) : lcpDepth;

                for (var i = startDepth; i < hierarchy.Count; i++) {
                    var headerChapter = hierarchy[i];

                    // Hierarchisch: eingeklappter Vorfahr → Header und Zeilen überspringen.
                    if (!numberStyle && HasCollapsedAncestor(headerChapter, collapsedSet)) {
                        blockCollapsed = true;
                        break;
                    }

                    // Original-Schreibweise aus dem gecachten Canonical-Item holen,
                    // da AlignsToChapter upper-cased ist.
                    if (rli.Arrangement is not { } rliArr) {
                        blockCollapsed = true;
                        break;
                    }

                    RowCaptionTableElement headerItem;
                    if (allItems.TryGetValue(RowCaptionTableElement.Identifier(headerChapter), out var capItem) && capItem is RowCaptionTableElement rcli) {
                        // NumberStyle: neue Instanz pro Block. Hierarchisch: Original wiederverwenden (IsExpanded erhalten).
                        headerItem = numberStyle
                            ? new RowCaptionTableElement(rcli.ChapterText, rliArr)
                            : rcli;
                    } else {
                        headerItem = new RowCaptionTableElement(headerChapter, rliArr);
                    }

                    if (i == hierarchy.Count - 1) {
                        // Leaf: Collapse-Zustand bestimmen.
                        if (numberStyle) {
                            // NumberStyle: pro Block anhand der ersten Zeile.
                            blockCollapsed = rli.Row is { IsDisposed: false } firstRow
                                             && _collapsedBlockFirstRowKeys.Contains(firstRow.KeyName);
                        } else {
                            // Hierarchisch: pro Kapitel anhand des Chapter-Texts.
                            blockCollapsed = collapsedSet.Contains(headerChapter);
                        }
                        headerItem.IsExpanded = !blockCollapsed;
                        currentBlockRows = [];
                        _chapterBlockRows[headerItem] = currentBlockRows;
                    } else {
                        // Vorfahr-Header: nur optische Gliederung. NumberStyle: immer expanded.
                        headerItem.IsExpanded = numberStyle || !collapsedSet.Contains(headerChapter);
                    }

                    sortedItems.Add(headerItem);
                }

                lastChapter = chapter;
            }

            // Zeile nur anzeigen, wenn der Block nicht eingeklappt ist.
            if (!blockCollapsed) {
                sortedItems.Add(rli);
            }

            // Block-Zeilen immer sammeln (auch eingeklappte, für Drag/Drop).
            if (currentBlockRows is not null && rli.Row is { IsDisposed: false } blockRow) {
                currentBlockRows.Add(blockRow);
            }

            // NumberStyle: eingeklappte Blöcke als Vorfahr merken, nicht eingeklappte setzen zurück.
            if (numberStyle) {
                collapsedAncestor = blockCollapsed ? chapter : null;
            }
        }
    }

    private void CalculateAllViewItems_AddFootElements(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement, List<TableElement> sortedItems) {
        allItems.TryGetValue(TableEndTableElement.Identifier, out var teli);
        if (teli is not TableEndTableElement tableEnd) {
            tableEnd = new TableEndTableElement(arrangement);
            allItems.Add(tableEnd.KeyName, tableEnd);
        }
        tableEnd.Visible = arrangement.ShowHead;
        tableEnd.IgnoreYOffset = false;
        sortedItems.Add(tableEnd);
    }

    private void CalculateAllViewItems_AddHeadElements(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement, List<TableElement> sortedItems, FilterCollection filterCombined, RowSortDefinition sortused) {
        if (!arrangement.ShowHead) { return; }

        // Spaltenbuchstaben-Leiste (A, B, C, ...) ganz oben.
        // Bei ColumnHeaderMode.Ohne wird die Leiste gar nicht erst zu sortedItems hinzugefügt,
        // da CalculateAllViewItems_CalculateYPosition die Sichtbarkeit aller Items pauschal auf true setzt.
        if (arrangement.ColumnHeaderMode != ColumnHeaderMode.Ohne) {
            var columnHeaderBar = GetOrCreateHeadItem(allItems, ColumnHeaderBarTableElement.Identifier, arrangement, () => new ColumnHeaderBarTableElement(arrangement));
            columnHeaderBar.Visible = true;
            columnHeaderBar.SheetStyle = SheetStyle;
            columnHeaderBar.Mode = arrangement.ColumnHeaderMode;
            sortedItems.Add(columnHeaderBar);
        }

        for (var z = 0; z < 3; z++) {
            var add = Ansichtbearbeitung;
            if (!add) {
                foreach (var thisColumn in arrangement) {
                    if (thisColumn.Column is { IsDisposed: false } c && !string.IsNullOrEmpty(c.CaptionGroup(z))) { add = true; break; }
                }
            }

            // Caption 1 bis 3 Expand Button
            if (add) {
                var captionBar = GetOrCreateHeadItem(allItems, CaptionBarListItemTableElement.Identifier(z), arrangement, () => new CaptionBarListItemTableElement(arrangement, z));
                captionBar.Visible = arrangement.ShowHead;
                sortedItems.Add(captionBar);
            }
        }

        // Grüner Expand Button
        var collapseBar = GetOrCreateHeadItem(allItems, CollapesBarTableElement.Identifier, arrangement, () => new CollapesBarTableElement(arrangement));
        collapseBar.Visible = arrangement.ShowHead;
        sortedItems.Add(collapseBar);

        // Spaltenköpfe direkt
        var columnHead = GetOrCreateHeadItem(allItems, ColumnsHeadTableElement.Identifier, arrangement, () => new ColumnsHeadTableElement(arrangement));
        columnHead.Visible = arrangement.ShowHead;
        sortedItems.Add(columnHead);

        //// Die Infos
        //allItems.TryGetValue(RowInfoListItem.Identifier, out var itemAdmin);
        //if (itemAdmin is not RowInfoListItem itemHeadx) {
        //    itemHeadx = new RowInfoListItem(arrangement);
        //    allItems.Add(itemHeadx.KeyName, itemHeadx);
        //}
        //itemHeadx.Visible = arrangement.ShowHead;
        //sortedItems.Add(itemHeadx);

        // Die Sortierung
        var sortAnzeige = GetOrCreateHeadItem(allItems, SortBarTableElement.Identifier, arrangement, () => new SortBarTableElement(arrangement));
        sortAnzeige.Visible = arrangement.ShowHead;
        sortAnzeige.FilterCombined = filterCombined;
        sortAnzeige.Sort = sortused;
        sortedItems.Add(sortAnzeige);

        // Filterleiste
        var columnFilter = GetOrCreateHeadItem(allItems, FilterBarTableElement.Identifier, arrangement, () => new FilterBarTableElement(arrangement));
        columnFilter.Visible = arrangement.ShowHead;
        columnFilter.FilterCombined = filterCombined;
        columnFilter.RowsFilteredCount = filterCombined.Rows.Count;
        sortedItems.Add(columnFilter);

        // Ansichtbearbeitung-Leiste
        if (Ansichtbearbeitung) {
            var editBar = GetOrCreateHeadItem(allItems, EditBarTableElement.Identifier, arrangement, () => new EditBarTableElement(arrangement));
            editBar.Visible = true;
            sortedItems.Add(editBar);
        }
    }

    private void CalculateAllViewItems_CalculateYPosition(List<TableElement> sortedItems, ColumnViewCollection arrangement) {
        var columnsWidth = (int)arrangement.ControlColumnsWidth().ControlToCanvas(Zoom);

        var y = 0;

        foreach (var thisItem in sortedItems) {
            thisItem.Visible = true;
            // Indent zur Spaltenbreite addieren: Beim Zeichnen werden die Spalten
            // um IndentWidth * Indent nach rechts verschoben. Die CanvasPosition-
            // Breite muss das abdecken, sonst wird die Zeile zu früh abgeschnitten.
            var wi = columnsWidth + TableElement.IndentWidth * thisItem.Indent;
            thisItem.CanvasPosition = new Rectangle(0, y, wi, thisItem.HeightInControl(ListBoxAppearance.Listbox, columnsWidth, Design.Item_ListBox));
            y = thisItem.CanvasPosition.Bottom;
        }
    }

    /// <summary>
    /// Befüllt _collapsed mit den direkt eingeklappten Kapiteln (als Großschreibung).
    /// </summary>
    private void CalculateAllViewItems_Collapsed(Dictionary<string, TableElement> allItems) {
        _collapsed.Clear();

        foreach (var thisR in allItems.Values) {
            if (thisR is RowCaptionTableElement { IsDisposed: false, IsExpanded: false } rcli) {
                _collapsed.Add(rcli.ChapterText.ToUpperInvariant());
            }
        }
    }

    private void CalculateAllViewItems_HildeAllItems(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement) {
        var columnsWidth = (int)arrangement.ControlColumnsWidth().ControlToCanvas(Zoom);

        foreach (var thisItem in allItems.Values) {
            thisItem?.Visible = false;

            if (thisItem is TableElement rbli) {
                rbli.Arrangement = arrangement;
                var wi = columnsWidth + TableElement.IndentWidth * rbli.Indent;
                rbli.CanvasPosition = rbli.CanvasPosition with { Width = wi };
            }
        }
    }

    private void CalculateAllViewItems_NewRow(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement, Table tb, string newRowsAllowed, bool headPosition, List<TableElement> sortedItems) {
        if (!string.IsNullOrEmpty(newRowsAllowed)) { return; }

        if (tb.Column.SysRowSortIndex is { IsDisposed: false } == headPosition) { return; }

        allItems.TryGetValue(NewRowTableElement.Identifier, out var nri);
        if (nri is not NewRowTableElement newRow) {
            newRow = new NewRowTableElement(arrangement);
            allItems.Add(newRow.KeyName, newRow);
        }
        newRow.IgnoreYOffset = headPosition;
        newRow.Visible = true;
        newRow.FilterCombined = FilterCombined;

        sortedItems.Add(newRow);
    }

    private List<RowTableElement> CalculateAllViewItems_Rows(Dictionary<string, TableElement> allItems, ColumnViewCollection arrangement, List<RowItem> allrows, List<RowItem> pinnedRows, RowSortDefinition sortused, List<RowItem> filteredRows) {
        var visibleRowListItems = new List<RowTableElement>(allrows.Count);
        var pinnedSet = new HashSet<RowItem>(pinnedRows);
        var filteredSet = new HashSet<RowItem>(filteredRows);

        // NumberStyle (SysRowSortIndex aktiv): streng nach Index sortieren —
        // Kapitel können verstreut sein, derselbe Header mehrfach auftreten.
        // Hierarchisch: Kapitel-Pfad als primären Sortierschlüssel voranstellen,
        // sodass alle Zeilen eines Kapitels (und ihrer Hierarchie) beieinander
        // liegen. Der Caption-Aufbau läuft dann einheitlich über beide Modi.
        var numberStyle = Table is { IsDisposed: false } tbNs && tbNs.Column.SysRowSortIndex is { IsDisposed: false };

        foreach (var thisRow in allrows) {
            var isPinned = pinnedSet.Contains(thisRow);
            var isFiltered = filteredSet.Contains(thisRow);

            foreach (var thisCap in CapsOfRow(thisRow, isFiltered, isPinned, arrangement)) {
                var id = RowTableElement.Identifier(thisRow, thisCap);

                if (!allItems.TryGetValue(id, out var it2) || it2 is not RowTableElement rowListItem) {
                    rowListItem = new RowTableElement(thisRow, thisCap, arrangement);
                    allItems.Add(rowListItem.KeyName, rowListItem);
                }
                rowListItem.Arrangement = arrangement;

                var rowKey = rowListItem.Row.CompareKey(sortused.UsedColumns);
                rowListItem.UserDefCompareKey = numberStyle
                    ? rowKey
                    : thisCap.ChapterPathSortKey() + FirstSortChar + rowKey;
                rowListItem.Visible = false;
                rowListItem.MarkYellow = isPinned;
                visibleRowListItems.Add(rowListItem);
            }
        }

        return sortused.Reverse
              ? visibleRowListItems.OrderByDescending(item => item.CompareKey(), StringComparer.OrdinalIgnoreCase).ToList()
              : visibleRowListItems.OrderBy(item => item.CompareKey(), StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Einfüge-Index für Spalten-Drag/Drop anhand der Maus-X-Position.
    /// </summary>
    private int CalculateColumnSortInsertIndex(int controlX) {
        if (CurrentArrangement is not { IsDisposed: false } ca) { return -1; }

        for (var i = 0; i < ca.Count; i++) {
            if (ca[i] is not { } cvi) { continue; }
            if (cvi.Column is null && cvi.FixedWidth == 0) { continue; }

            var left = cvi.ControlColumnLeft(OffsetX);
            var right = cvi.ControlColumnRight(OffsetX);

            if (controlX < left) { return i; }
            if (controlX <= right) {
                return controlX < left + (right - left) / 2 ? i : i + 1;
            }
        }

        return ca.Count;
    }

    /// <summary>
    /// Einfüge-Index für Zeilen-Drag/Drop anhand der Maus-Y-Position. -1 = kein gültiges Ziel.
    /// </summary>
    private int CalculateRowSortInsertIndex(int controlY) {
        if (_sortedViewItems is not { Count: > 0 }) { return -1; }

        var draggedChapter = _dragItem as RowCaptionTableElement;

        for (var i = 0; i < _sortedViewItems.Count; i++) {
            var item = _sortedViewItems[i];
            if (!IsDroppableTarget(item)) { continue; }

            var pos = item.ControlPosition(Zoom, OffsetX, OffsetY);
            if (controlY < pos.Top) {
                return EnsureValidChapterInsertIndex(i, draggedChapter);
            }
            if (controlY <= pos.Bottom) {
                var raw = controlY < pos.Top + pos.Height / 2 ? i : i + 1;
                return EnsureValidChapterInsertIndex(raw, draggedChapter);
            }
        }

        return EnsureValidChapterInsertIndex(_sortedViewItems.Count, draggedChapter);
    }

    private void Cell_CellValueChanged(object? sender, CellEventArgs e) {
        // Skript-Threads feuern CellValueChanged synchron; ohne Marshalling konkurrieren UI- und Skript-Thread auf _allViewItems.
        if (InvokeRequired) {
            BeginInvoke(new Action(() => Cell_CellValueChanged(sender, e)));
            return;
        }
        if (e.Row.IsDisposed || e.Column.IsDisposed) { return; }

        RemoveRowItems(e.Row);

        if (CurrentArrangement is { IsDisposed: false } ca) {
            if (SortUsed() is { } rsd) {
                // Kapitel-Spalte direkt geändert — die Hierarchie kann sich
                // verändert haben (neue Unter-/Überkapitel, z. B. "Gerüst" →
                // "Gerüst\111"). Full clear wie bei der Header-Umbenennung,
                // damit stale Captions (veralteter IsExpanded-Zustand etc.)
                // die neue Hierarchie nicht behindern.
                if (e.Column == ca.ColumnForChapter) {
                    Invalidate_AllViewItems(true);
                } else if (rsd.UsedForRowSort(e.Column)) {
                    Invalidate_AllViewItems(false);
                }
            }
            if (ca[e.Column] is { IsDisposed: false } cv) {
                if (e.Column.MultiLine) {
                    Invalidate_AllViewItems(false); // Zeichenhöhe kann sich ändern...
                }
                cv.Invalidate_CanvasContentWidth(); // Kann auf sich selbst aufpassen
            }
        }

        Invalidate();
    }

    private (ColumnViewItem?, TableElement?) CellOnCoordinate(ColumnViewCollection? ca, CanvasMouseEventArgs e) {
        var row = RowItemAtPosition(e.ControlY);
        return (ColumnOnCoordinate(ca, e, row), row);
    }

    private void CloseAllComponents() {
        if (InvokeRequired) {
            Invoke(new Action(CloseAllComponents));
            return;
        }
        if (IsDisposed) { return; }

        Edit_Close();
        AutoFilter_Close();
        HideMiniToolbar();

        if (Table is not { IsDisposed: false }) { return; }
        FloatingForm.Close(this);
        Forms.QuickInfo.Close();
    }

    private void CollapseThis(string[] t) {
        if (AllViewItems is not { } avi) { return; }
        var did = false;
        var tSet = new HashSet<string>(t);

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowCaptionTableElement { IsDisposed: false } rcli) {
                if (rcli.IsExpanded == tSet.Contains(rcli.ChapterText)) {
                    rcli.IsExpanded = !rcli.IsExpanded;
                    did = true;
                }
            }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    /// <summary>
    /// Auswahlliste für eine Strategie mit Suggestions. Content-Spalte hat Vorrang vor Style-Spalte.
    /// </summary>
    private List<ListItem> CollectEditItems(ColumnItem? contentColumn, ColumnItem? styleColumn, RowItem? contentRow, CellExtEventArgs? cellInfo) {
        var column = contentColumn ?? styleColumn;
        if (column is not { IsDisposed: false } col) { return []; }

        var renderer = cellInfo?.ColumnView is { IsDisposed: false } cv
            ? cv.GetRenderer(SheetStyle)
            : RendererOf(col, SheetStyle);

        return ItemsOf(col, contentRow, 1000, renderer);
    }

    private void Column_ItemRemoving(object? sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn?.Column) { CursorPos_Reset(); }
    }

    private ColumnViewItem? ColumnOnCoordinate(ColumnViewCollection? ca, CanvasMouseEventArgs? e, TableElement? row = null) {
        if (ca is not { IsDisposed: false } || e is null) { return null; }

        // Eingerückte Zeilen verschieben alle Spalten beim Zeichnen nach
        // rechts (RowBackground.DrawExplicit: indentOffset). Die Hit-Detection
        // muss denselben Versatz berücksichtigen, sonst wird der Klick der
        // falschen Spalte zugeordnet — z.B. trifft ein Klick auf den visuell
        // verschobenen Pin-Button eine andere Spalte, während ein Klick in
        // den leeren Indent-Bereich fälschlich als Pin-Klick gewertet wird.
        var indentOffset = row is null ? 0 : TableElement.IndentWidth.CanvasToControl(Zoom) * row.Indent;

        foreach (var thisViewItem in ca.RenderingItems) {
            if (e.ControlX >= thisViewItem.ControlColumnLeft(OffsetX) + indentOffset && e.ControlX <= thisViewItem.ControlColumnRight(OffsetX) + indentOffset) { return thisViewItem; }
        }

        return null;
    }

    private void ContextMenu_ContentCopy(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, _, _) = GetContextData(e.HotItem);
        var rli = GetRow(row, null);
        var cp = rli?.ControlPosition(Zoom, OffsetX, OffsetY) ?? Rectangle.Empty;
        var vi = CurrentArrangement?[column];
        CopyToClipboard(column, row, true, PointToScreen(new Point(vi?.ControlColumnRight(OffsetX) ?? 0, cp.Y)));
    }

    private void ContextMenu_ContentDelete(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, _, _) = GetContextData(e.HotItem);

        if (TableViewForm.EditableErrorMessage(row?.Table, row)) { return; }
        row?.CellSet(column, string.Empty, "Inhalt Löschen Kontextmenü");
    }

    private void ContextMenu_CopyAll(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (column is null) { return; }

        var txt = Export_CSV(FirstRow.Without, column);
        txt = txt.Replace("|", "\r\n");
        txt = txt.Replace(";", string.Empty);
        if (CopytoClipboard(txt)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void ContextMenu_CopyAllSorted(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (column is null) { return; }

        var txt = Export_CSV(FirstRow.Without, column);
        txt = txt.Replace("|", "\r\n");
        txt = txt.Replace(";", string.Empty);
        var l = string.Join('\r', txt.SplitAndCutByCr().SortedDistinctList());
        if (CopytoClipboard(l)) {
            QuickNote.Show(NoteSymbols.Ok, "In Zwischenablage");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void ContextMenu_HideOrDeleteColumn(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, viewItem) = GetContextData(e.HotItem);
        if (Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { } ca) { return; }

        // Defensive Admin-Prüfung (Enabled-Zustand im Kontextmenü ist nur UI).
        if (!tb.IsAdministrator()) { return; }

        if (TableViewForm.EditableErrorMessage(tb, null)) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        var currentArr = tcvc.GetByKey(ca.KeyName);
        if (currentArr is null) { return; }

        // Virtuelle Spalte (kein ColumnItem) nur ausblenden. Echte Spalten weiter unten behandeln.
        if (column is null) {
            if (viewItem?.StorageKey is { } sk && currentArr.FirstOrDefault(x => x.StorageKey == sk) is { } vItem) {
                currentArr.Remove(vItem);
                tb.ColumnArrangements = tcvc.AsReadOnly();
            }
            return;
        }

        if (IsAnsicht0(ca)) {
            if (Forms.MessageBox.Show($"Spalte <b>{column.Caption}</b> wirklich löschen?", ImageCode.Frage, "Löschen", "Abbrechen") != 0) { return; }

            tb.Column.Remove(column, "Kontextmenü: Spalte permanent gelöscht");
            foreach (var arr in tcvc) {
                if (arr[column] is { } vi) { arr.Remove(vi); }
            }
        } else {
            if (currentArr[column] is { } parsedViewItem) { currentArr.Remove(parsedViewItem); }
        }

        tb.ColumnArrangements = tcvc.AsReadOnly();
    }

    private void ContextMenu_KeyCopy(object? sender, ContextMenuEventArgs e) {
        var (_, row, _, _, _) = GetContextData(e.HotItem);
        if (row is null) { return; }

        if (CopytoClipboard(row.KeyName)) {
            QuickNote.Show(NoteSymbols.Ok, LanguageTool.DoTranslate("Kopiert.", true));
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void ContextMenu_NewColumn(object? sender, ContextMenuEventArgs e) {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { } ca) { return; }

        if (TableViewForm.EditableErrorMessage(tb, null)) { return; }

        var (column, _, _, _, _) = GetContextData(e.HotItem);
        ColumnsHeadTableElement.ShowDummyColumnDropDown(ca, this, column);
    }

    private void ContextMenu_NewRowInChapter(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        var (_, row, _, _, _) = GetContextData(e.HotItem);
        if (row is not { IsDisposed: false } srcRow) { return; }

        ColumnItem? chapterCol = ca.ColumnForChapter is { IsDisposed: false } cc ? cc : null;
        var chapterValue = chapterCol is not null ? srcRow.CellGetString(chapterCol) : string.Empty;

        // Filter inkl. Chunk-Filterung übernehmen; Chunk-Wert daraus erkennen.
        using var fc = new FilterCollection(tb, "Neue Zeile aus Mini-Toolbar");
        fc.AddIfNotExists(FilterCombined);

        if (chapterValue is { Length: > 0 } && chapterCol is not null) {
            fc.RemoveOtherAndAdd(new FilterItem(chapterCol, FilterType.Istgleich, chapterValue));
        } else if (chapterCol is not null) {
            // Ohne Kapitel: bestehenden Kapitel-Filter entfernen.
            fc.Remove(chapterCol);
        }

        // Zwingende Spalten (First, UniqueValue) mit Default-Wert "NEU_X" befüllen. Bestehende Filter haben Vorrang.
        var defaultColumns = new HashSet<ColumnItem>(ReferenceEqualityComparer.Instance);
        if (tb.Column.First is { IsDisposed: false } firstCol) {
            defaultColumns.Add(firstCol);
        }

        foreach (var uvd in tb.UniqueValues) {
            foreach (var keyCol in uvd.KeyColumns) {
                if (keyCol is { IsDisposed: false }) { defaultColumns.Add(keyCol); }
            }
        }

        foreach (var col in defaultColumns) {
            if (fc[col] is not null) { continue; }
            fc.RemoveOtherAndAdd(new FilterItem(col, FilterType.Istgleich, NextNewDefaultValue(tb, col)));
        }

        var nr = tb.Row.GenerateAndAdd([.. fc], "Neue Zeile aus Mini-Toolbar");
        if (nr.IsFailed || nr.Value is not RowItem newRow) {
            NotEditableInfo(nr.FailedReason);
            return;
        }

        // NumberStyle: neue Zeile unter der Quell-Zeile einsortieren (nachfolgende hochschieben).
        if (tb.Column.SysRowSortIndex is { IsDisposed: false } sortCol) {
            var srcIdx = srcRow.CellGetInteger(sortCol);

            tb.SuppressEvents();
            try {
                foreach (var r in tb.Row) {
                    if (r is not { IsDisposed: false } || ReferenceEquals(r, newRow)) { continue; }
                    var v = r.CellGetInteger(sortCol);
                    if (v > srcIdx) { r.CellSet(sortCol, v + 1, "Neue Zeile oberhalb eingefügt"); }
                }
                newRow.CellSet(sortCol, srcIdx + 1, "Neue Zeile aus Mini-Toolbar");
            } finally {
                tb.ResumeEvents();
            }
        }

        if (!FilterCombined.Rows.Contains(newRow)) {
            if (Forms.MessageBox.Show("Die neue Zeile ist ausgeblendet.<br>Soll sie <b>angepinnt</b> werden?", ImageCode.Pinnadel, "anpinnen", "abbrechen") == 0) {
                PinAdd(newRow);
            }
        }

        // Cursor auf die neue Zeile setzen. GetRow löst über _ = AllViewItems
        // den view-Aufbau aus, sodass die neue Zeile in _rowLookup liegt.
        var newRowItem = GetRow(newRow, null);
        if (View_ColumnFirst() is { } firstViewCol && newRowItem is not null) {
            CursorPos_Set(firstViewCol, newRowItem, true);
        }
    }

    private void ContextMenu_Pin(object? sender, ContextMenuEventArgs e) {
        var (_, row, _, _, _) = GetContextData(e.HotItem);

        PinAdd(row);
    }

    private void ContextMenu_ResetSort(object? sender, ContextMenuEventArgs e) => SortDefinitionTemporary = null;

    private void ContextMenu_RestorePreviousContent(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, _, _) = GetContextData(e.HotItem);

        if (TableViewForm.EditableErrorMessage(row?.Table, row)) { return; }
        DoUndo(column, row);
    }

    private void ContextMenu_SearchAndReplace(object? sender, ContextMenuEventArgs e) {
        if (Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }
        OpenSearchAndReplaceInCells();
    }

    private void ContextMenu_SortAZ(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (Table is not { IsDisposed: false } tb) { return; }

        SortDefinitionTemporary = new RowSortDefinition(tb, column, false);
    }

    private void ContextMenu_SortZA(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (Table is not { IsDisposed: false } tb) { return; }

        SortDefinitionTemporary = new RowSortDefinition(tb, column, true);
    }

    private void ContextMenu_Statistics(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (column is null) { return; }

        var split = false;
        if (column.MultiLine) {
            split = Forms.MessageBox.Show("Zeilen als Ganzes oder aufsplitten?", ImageCode.Frage, "Ganzes", "Splitten") != 0;
        }
        column.Statistik(_rowsVisibleUnique, !split);
    }

    private void ContextMenu_Sum(object? sender, ContextMenuEventArgs e) {
        var (column, _, _, _, _) = GetContextData(e.HotItem);
        if (column is null) { return; }

        var summe = column.Summe(FilterCombined);
        if (!summe.HasValue) {
            QuickNote.Show(NoteSymbols.Critical, "Summe fehlgeschlagen");
        } else {
            Forms.MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, ImageCode.Summe, "OK");
        }
    }

    private void ContextMenu_Unpin(object? sender, ContextMenuEventArgs e) {
        var (_, row, _, _, _) = GetContextData(e.HotItem);

        PinRemove(row);
    }

    private void Cursor_Move(Direction direction) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) {
            CursorPos_Set(null, null, false);
            return;
        }

        if (direction == Direction.None) { return; }

        var newColumn = CursorPosColumn;
        var newRow = CursorPosRow;

        if (newColumn is not null) {
            if (direction.HasFlag(Direction.Links)) {
                if (ca.PreviousVisible(newColumn) is { } c) { newColumn = c; }
            }
            if (direction.HasFlag(Direction.Rechts)) {
                if (ca.NextVisible(newColumn) is { } c) { newColumn = c; }
            }
        }

        if (newRow is not null) {
            if (direction.HasFlag(Direction.Oben)) {
                var prev = View_PreviousRow(newRow);
                if (prev is not null) { newRow = prev; }
            }
            if (direction.HasFlag(Direction.Unten)) {
                var next = View_NextRow(newRow);
                if (next is not null) { newRow = next; }
            }
        }

        CursorPos_Set(newColumn, newRow, true);

        // Die Mini-Toolbar erscheint ausschließlich bei Mausklick auf eine
        // Zelle. Tastatur-Navigation blendet sie lediglich aus.
        HideMiniToolbar();
    }

    private void CursorPos_Reset() => CursorPos_Set(null, null, false);

    private void DoColumnSortReorder(ColumnViewItem sourceCvi, int insertIndex) {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }
        if (!tb.IsAdministrator()) { return; }

        var editable = ca.IsNowEditable();
        if (!string.IsNullOrEmpty(editable)) {
            NotEditableInfo(editable);
            return;
        }

        var oldIndex = ca.IndexOf(sourceCvi);
        if (oldIndex < 0) { return; }

        ca.Move(oldIndex, insertIndex);

        var newIndex = ca.IndexOf(sourceCvi);

        var hasPermanentAfter = false;
        for (var i = newIndex + 1; i < ca.Count; i++) {
            if (ca[i] is { Permanent: true }) {
                hasPermanentAfter = true;
                break;
            }
        }

        if (hasPermanentAfter) {
            sourceCvi.Permanent = true;
        } else if (sourceCvi.Permanent) {
            var hasNonPermanentBefore = false;
            for (var i = 0; i < newIndex; i++) {
                if (ca[i] is { Permanent: false }) {
                    hasNonPermanentBefore = true;
                    break;
                }
            }
            if (hasNonPermanentBefore) {
                sourceCvi.Permanent = false;
            }
        }

        // Alle Arrangements serialisieren und in Table.ColumnArrangements schreiben
        var tcvc = new List<ColumnViewCollection>();
        foreach (var thisCa in tb.ColumnArrangements) {
            if (thisCa.KeyName == ca.KeyName) {
                tcvc.Add(new ColumnViewCollection(tb, ca.ParseableItems().FinishParseable(), ca.KeyName));
            } else {
                tcvc.Add(thisCa);
            }
        }
        tb.ColumnArrangements = tcvc.AsReadOnly();
    }

    private void DoCursorPos() {
        // Verwaiste CursorPosRow-/CursorPosColumn-Instanzen nach einem Neuaufbau migrieren.
        if (CursorPosRow is { Row: { IsDisposed: false } cursorRow } oldRli) {
            // Über RowItem-Identität + Kapitel-Caption migrieren, nicht über _rowLookup.
            var freshRli = GetRow(cursorRow, oldRli.AlignsToChapter);
            if (freshRli is not null && !ReferenceEquals(freshRli, oldRli)) {
                CursorPosRow = freshRli;
            }
        }

        if (CursorPosColumn is { Column: { IsDisposed: false } cursorCol } oldCvi
            && CurrentArrangement is { IsDisposed: false } ca
            && ca[cursorCol] is { } freshCvi && !ReferenceEquals(freshCvi, oldCvi)) {
            CursorPosColumn = freshCvi;
        }

        foreach (var rdli in _cachedRowViewItems) {
            rdli.Column = ReferenceEquals(CursorPosRow, rdli) ? CursorPosColumn?.Column : null;
        }
    }

    /// <summary>
    /// Führt Filter und FilterFix zusammen und schreibt das Ergebnis in FilterCombined.
    /// </summary>
    private void DoFilterCombined() {
        var filterEmpty = Filter.Count == 0;
        var fixEmpty = FilterFix is not { IsDisposed: false, Count: > 0 };

        if (filterEmpty && fixEmpty) {
            if (FilterCombined.Table != Filter.Table) {
                FilterCombined.ChangeTo(new FilterCollection(Filter.Table, "EmptyCombined"));
            } else {
                FilterCombined.Clear();
            }
        } else {
            using var nfc = new FilterCollection(Filter.Table, "TmpFilterCombined");

            nfc.Table = Filter.Table;
            nfc.RemoveOtherAndAdd(Filter, null);
            nfc.RemoveOtherAndAdd(FilterFix, "Filter aus übergeordneten Element");

            FilterCombined.ChangeTo(nfc);
        }

        Invalidate_AllViewItems(false);
    }

    private void DoRowSortReorder(List<RowItem> sourceRows, int insertIndex, bool isChapterBlock, int mouseControlY) {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false }) { return; }
        if (sourceRows.Count == 0) { return; }

        // CellSets bündeln: ohne Suppression entstünden N teure Voll-Layouts, ResumeEvents feuert einmalig ViewChanged.
        tb.SuppressEvents();
        try {
            // Alle Zeilen in der aktuellen Sortierung sammeln
            var sortedRows = SortUsed()?.SortedRows(tb.Row) ?? [.. tb.Row];
            if (sortedRows.Count == 0) { return; }

            // Quell-Zeilen aus der Liste entfernen
            foreach (var sr in sourceRows) {
                if (sr is { IsDisposed: false }) { sortedRows.Remove(sr); }
            }

            // Ziel-Position: erste Zeile ab insertIndex; Header als Drop-Ziel = erste Zeile unter dem Kapitel.
            // Eingeklappte Kapitel: erste Zeile aus _chapterBlockRows.
            RowItem? targetRow = null;
            for (var i = Math.Max(0, insertIndex); i < _sortedViewItems.Count; i++) {
                if (_sortedViewItems[i] is RowTableElement tRli && tRli.Row is { IsDisposed: false }) {
                    targetRow = tRli.Row;
                    break;
                }
                if (_sortedViewItems[i] is RowCaptionTableElement capRcli
                    && _chapterBlockRows.TryGetValue(capRcli, out var blockRows)
                    && blockRows.Count > 0
                    && blockRows[0] is { IsDisposed: false } firstBlockRow) {
                    targetRow = firstBlockRow;
                    break;
                }
            }

            var targetIndexInSorted = targetRow is null ? sortedRows.Count : sortedRows.IndexOf(targetRow);
            if (targetIndexInSorted < 0) { targetIndexInSorted = sortedRows.Count; }

            // Kapitel-Aktualisierung nur bei Einzelzeilen-Verschiebung.
            // Bei einem Kapitel-Block (isChapterBlock) behalten alle Zeilen ihr Kapitel.
            if (!isChapterBlock && sourceRows.Count == 1 && sourceRows[0] is { IsDisposed: false } singleRow) {
                RowTableElement? singleRli = null;
                for (var i = 0; i < _cachedRowViewItems.Count; i++) {
                    if (_cachedRowViewItems[i].Row == singleRow) { singleRli = _cachedRowViewItems[i]; break; }
                }
                if (singleRli is { IsDisposed: false }) {
                    UpdateChapterOnRowSortMove(singleRli, mouseControlY);
                }
            }

            // Quell-Zeilen als zusammenhängenden Block an der neuen Position einfügen
            foreach (var sr in sourceRows) {
                if (sr is { IsDisposed: false }) {
                    sortedRows.Insert(Math.Min(targetIndexInSorted, sortedRows.Count), sr);
                    targetIndexInSorted++;
                }
            }

            // Alle Zeilen neu nummerieren
            tb.RenumberRows(sortedRows, "Drag/Drop Sortierung");
        } finally {
            tb.ResumeEvents();
        }
    }

    /// <summary>
    /// Zeichnet den Einfüge-Indikator (16px breit) für das Spalten-Drag/Drop.
    /// </summary>
    private void DrawColumnSortInsertIndicator(Graphics gr, ColumnViewCollection ca) {
        var area = AvailableControlPaintArea;

        // Entspricht die Einfüge-Position der aktuellen Position, die eigene Spalte markieren
        if (_dragItem is ColumnViewItem srcCol && srcCol.IsOk()) {
            var sourceIdx = ca.IndexOf(srcCol);
            if (sourceIdx >= 0 && (_dragInsertIndex == sourceIdx || _dragInsertIndex == sourceIdx + 1)) {
                var left = srcCol.ControlColumnLeft(OffsetX);
                var width = srcCol.ControlColumnWidth();
                DrawInsertIndicatorRect(gr, new Rectangle(left, area.Top, width, area.Height));
                return;
            }
        }

        // 16-Pixel-Indikator an der Einfüge-Position
        const int indicatorHalf = 8;
        int indicatorX;

        // Die Spalte finden, vor der eingefügt wird
        ColumnViewItem? targetCvi = null;
        for (var i = 0; i < ca.Count; i++) {
            if (ca[i] is { } cvi && cvi.IsOk() && i >= _dragInsertIndex) {
                targetCvi = cvi;
                break;
            }
        }

        if (targetCvi is { IsDisposed: false }) {
            var left = targetCvi.ControlColumnLeft(OffsetX);
            indicatorX = left - indicatorHalf;
        } else {
            // Am Ende: nach der letzten sichtbaren Spalte
            ColumnViewItem? lastCvi = null;
            for (var i = ca.Count - 1; i >= 0; i--) {
                if (ca[i] is { } cvi && cvi.IsOk()) {
                    lastCvi = cvi;
                    break;
                }
            }
            if (lastCvi is not { IsDisposed: false }) { return; }
            var right = lastCvi.ControlColumnRight(OffsetX);
            indicatorX = right - indicatorHalf;
        }

        DrawInsertIndicatorRect(gr, new Rectangle(indicatorX, area.Top, indicatorHalf * 2, area.Height));
    }

    /// <summary>
    /// Zeichnet den Einfüge-Indikator (16px hoch) für das Zeilen-Drag/Drop.
    /// </summary>
    private void DrawRowSortInsertIndicator(Graphics gr, ColumnViewCollection ca) {
        if (_sortedViewItems is not { Count: > 0 }) { return; }
        if (_dragInsertIndex < 0 || _dragInsertIndex > _sortedViewItems.Count) { return; }

        var columnsLeft = 0;
        var columnsRight = ca.ControlColumnsWidth() + columnsLeft;
        var rowsTop = RowsAreaTop();

        // Entspricht die Einfüge-Position dem aktuellen Block, den gesamten Block markieren
        var srcRows = GetDragSourceRows(_dragItem);
        if (srcRows.Count > 0) {
            var (firstSrc, lastSrc) = SourceIndexRange(srcRows, _dragItem);
            if (firstSrc >= 0 && _dragInsertIndex >= firstSrc && _dragInsertIndex <= lastSrc + 1) {
                var firstPos = _sortedViewItems[firstSrc].ControlPosition(Zoom, OffsetX, OffsetY);
                var lastPos = _sortedViewItems[lastSrc].ControlPosition(Zoom, OffsetX, OffsetY);
                DrawInsertIndicatorRect(gr, new Rectangle(columnsLeft, firstPos.Top, columnsRight - columnsLeft, lastPos.Bottom - firstPos.Top));
                return;
            }
        }

        // 16-Pixel-Indikator an der Einfüge-Position.
        // Die Position liegt "vor" dem ersten Drop-Ziel (Zeile oder Header)
        // ab _dragInsertIndex — bzw. nach dem letzten, falls am Ende.
        const int indicatorHalf = 8;
        int indicatorY;

        var target = FirstDroppableViewItem(_dragInsertIndex);
        if (target is null) {
            var last = LastDroppableViewItem();
            if (last is null) { return; }
            indicatorY = last.ControlPosition(Zoom, OffsetX, OffsetY).Bottom - indicatorHalf;
        } else {
            indicatorY = target.ControlPosition(Zoom, OffsetX, OffsetY).Top - indicatorHalf;
        }

        // Indikator auf den Zeilenbereich begrenzen, damit er nicht im Spaltenkopf gezeichnet wird
        indicatorY = Math.Max(indicatorY, rowsTop);

        DrawInsertIndicatorRect(gr, new Rectangle(columnsLeft, indicatorY, columnsRight - columnsLeft, indicatorHalf * 2));
    }

    /// <summary>
    /// Schließt das aktive Edit und committet den Wert über _editCommit. Ohne aktives Edit nur EndEdit.
    /// </summary>
    private void Edit_Close() {
        if (IsDisposed || ActiveControlStrategy is not { } strategy) { return; }

        if (Table is not { IsDisposed: false } || _editCommit is not { } commit) {
            EndEdit();
            return;
        }

        var value = strategy.Control?.Text ?? string.Empty;
        EndEdit();
        commit(value);
        Focus();
    }

    private void Edit_EnterKey(object? sender, System.EventArgs e) {
        if (sender is ControlStrategy { MultiLine: true }) { return; }
        CloseAllComponents();
    }

    private void Edit_EscKey(object? sender, System.EventArgs e) {
        // Commit-Referenz vor EndEdit verwerfen, damit kein Commit ausgelöst wird.
        _editCommit = null;
        EndEdit();
        CloseAllComponents();
    }

    private void Edit_LostFocus(object? sender, System.EventArgs e) {
        // Während BeginEdit ignorieren (Abbau/Fokus-Übergabe).
        if (_isBeginningEdit) {
            return;
        }

        if (ActiveControlStrategy?.Control is { } activeControl) {
            if (FloatingForm.IsShowing(activeControl)) { return; }

            // Noch sichtbares Control: Fokusverlust durch Tabellenklick → nächsten MouseDown konsumieren.
            if (activeControl.Visible) {
                _consumeNextMouseDown = true;
            }
        }

        CloseAllComponents();
    }

    private void Edit_TabKey(object? sender, System.EventArgs e) => CloseAllComponents();

    /// <summary>
    /// Beendet das aktive Edit ohne Commit.
    /// </summary>
    private void EndEdit() {
        HideAllEditControls();
        _editCommit = null;
    }

    /// <summary>
    /// Korrigiert den Einfüge-Index beim Verschieben eines Kapitel-Blocks, damit der Vorgänger kein anders lautender Header ist.
    /// </summary>
    private int EnsureValidChapterInsertIndex(int index, RowCaptionTableElement? draggedChapter) {
        if (draggedChapter is null) { return index; }

        while (index > 0) {
            var predecessor = _sortedViewItems[index - 1];
            if (predecessor.IgnoreYOffset || predecessor is RowTableElement) { break; }
            if (predecessor is RowCaptionTableElement predChapter) {
                if (string.Equals(predChapter.ChapterText, draggedChapter.ChapterText, StringComparison.OrdinalIgnoreCase)) { break; }
                // Eingeklappter Vorgänger: direktes Aufeinandertreffen der Header ist zulässig.
                if (IsChapterCollapsed(predChapter)) { break; }
            }
            index--;
        }
        return index;
    }

    private bool EnsureVisible(TableElement? rowdata) {
        if (rowdata is not RowTableElement rli) { return false; }

        var p = rli.ControlPosition(Zoom, OffsetX, OffsetY);
        EnsureVisibleY(p.Bottom);
        EnsureVisibleY(p.Top);
        return true;
    }

    private bool EnsureVisible(ColumnViewItem? viewItem) {
        if (IsDisposed) { return false; }
        if (viewItem?.Column is not { IsDisposed: false }) { return false; }
        if (viewItem.Permanent) { return true; }
        //var dispR = DisplayRectangleWithoutSlider();

        //if (CurrentArrangement is not { IsDisposed: false } cax) { return false; }

        //var realhead = viewItem.RealHead(Zoom, OffsetX); // Filterleiste kann ignoriert werden, da nur ControlX-Koordinaten berechnet werden.

        //if (viewItem.Permanent) {
        //    return realhead.Right <= dispR.Width;
        //}

        //if (realhead.Left < ControlToCanvasX(cax.WiederHolungsSpaltenWidth, Zoom)) {
        //    OffsetX = OffsetX + realhead.ControlX - ControlToCanvasX(cax.WiederHolungsSpaltenWidth, Zoom);
        //} else if (realhead.Right > dispR.Width) {
        //    OffsetX = OffsetX + realhead.Right - dispR.Width;
        //}

        EnsureVisibleX(viewItem.ControlColumnRight(OffsetX));
        EnsureVisibleX(viewItem.ControlColumnLeft(OffsetX));

        return true;
    }

    private void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e) => DoFilterCombined();

    private void FilterAny_RowsChanged(object? sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (CurrentArrangement is { IsDisposed: false } ca) {
            foreach (var thisColumn in ca) {
                thisColumn.TmpIfFilterRemoved = null;
            }
        }

        Invalidate_AllViewItems(false);
    }

    private void FilterCombined_PropertyChanged(object? sender, PropertyChangedEventArgs e) => OnFilterCombinedChanged();

    /// <summary>
    /// Filtert auf nicht angepinnte, editierbare Zeilen.
    /// </summary>
    private List<RowItem> FilterDraggableRows(IEnumerable<RowItem> rows) {
        var result = new List<RowItem>();
        if (Table is not { IsDisposed: false } tb) { return result; }
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return result; }

        foreach (var br in rows) {
            if (br is { IsDisposed: false }
                && !PinnedRows.Contains(br)
                && string.IsNullOrEmpty(TableView.IsCellEditable(sortCol, br, br.ChunkValue))) {
                result.Add(br);
            }
        }

        return result;
    }

    private void FilterFix_PropertyChanged(object? sender, PropertyChangedEventArgs e) => DoFilterCombined();

    /// <summary>
    /// Erstes sichtbares RowListItem ab startIndex mit der Schrittweite step.
    /// </summary>
    private RowTableElement? FindVisibleRowListItem(int startIndex, int step) {
        for (var i = startIndex; i >= 0 && i < _sortedViewItems.Count; i += step) {
            if (_sortedViewItems[i] is RowTableElement rli && rli.Visible) { return rli; }
        }
        return null;
    }

    private void FinishDrag() {
        var item = _dragItem;
        var insertIndex = _dragInsertIndex;
        // Maus-Y in Control-Koordinaten live abfragen (Drop erfolgt synchron
        // in OnMouseUp direkt nach dem letzten OnMouseMove). Wird für die
        // Kapitel-Zuordnung an Kapitel-Grenzen benötigt.
        var mouseControlY = PointToClient(Cursor.Position).Y;

        _isDragging = false;
        _dragItem = null;
        _dragInsertIndex = -1;

        if (insertIndex >= 0) {
            if (item is ColumnViewItem cvi && cvi.IsOk()) {
                DoColumnSortReorder(cvi, insertIndex);
            } else if (item is RowItem or RowCaptionTableElement) {
                var srcRows = GetDragSourceRows(item);
                if (srcRows.Count > 0) {
                    // No-Op: Wird auf den eigenen Block zurückgezogen, nichts verschieben.
                    // Gleiche Bedingung wie in DrawRowSortInsertIndicator.
                    var (firstSrc, lastSrc) = SourceIndexRange(srcRows, item);
                    if (firstSrc < 0 || insertIndex < firstSrc || insertIndex > lastSrc + 1) {
                        // Bei Kapitel-Block werden KEINE Überschriften geändert.
                        DoRowSortReorder(srcRows, insertIndex, item is RowCaptionTableElement, mouseControlY);
                    }
                }
            }
        }

        Invalidate();
    }

    /// <summary>
    /// Erstes Drop-Ziel ab fromIndex oder null.
    /// </summary>
    private TableElement? FirstDroppableViewItem(int fromIndex) {
        for (var i = Math.Max(0, fromIndex); i < _sortedViewItems.Count; i++) {
            if (IsDroppableTarget(_sortedViewItems[i])) { return _sortedViewItems[i]; }
        }
        return null;
    }

    /// <summary>
    /// Verschiebbare Quell-Zeilen: Einzelzeile (RowItem) oder gesamter Kapitel-Block (RowCaptionListItem).
    /// </summary>
    private List<RowItem> GetDragSourceRows(object? item) {
        switch (item) {
            case RowItem r:
                return r.IsDisposed ? [] : FilterDraggableRows([r]);

            case RowCaptionTableElement rcli:
                return GetChapterBlockRows(rcli) is { } blockRows ? FilterDraggableRows(blockRows) : [];

            default:
                return [];
        }
    }

    /// <summary>
    /// Holt eine gecachte Strategie oder erzeugt sie. Verdrahtet die Event-Handler beim erstmaligen Anlegen.
    /// Der Parse-Code gehört zum Cache-Schlüssel, damit Instanzen keine Werte
    /// einer anderen Konfiguration übernehmen.
    /// </summary>
    private ControlStrategy GetOrCreateControlStrategy(string editStrategyKey, string strategyParameter) {
        var cacheKey = strategyParameter is { Length: > 0 } ? editStrategyKey + "|" + strategyParameter : editStrategyKey;
        var strategy = _controlStrategyCache.GetOrAdd(cacheKey, _ => CreateControlStrategy(editStrategyKey));

        if (strategy.Control is null) {
            strategy.CreateControl();
            if (strategy.Control is Control c) {
                c.Visible = false;
                Controls.Add(c);
            }

            strategy.EnterKey += Edit_EnterKey;
            strategy.EscKey += Edit_EscKey;
            strategy.TabKey += Edit_TabKey;
            strategy.LostFocus += Edit_LostFocus;
        }

        return strategy;
    }

    /// <summary>
    /// Liefert das RowListItem für eine Row. Bei gesetztem chapter die exakte Kombination, sonst das erste gefundene.
    /// </summary>
    private RowTableElement? GetRow(RowItem? row, string? chapter) {
        if (row is not { IsDisposed: false }) { return null; }

        _ = AllViewItems;

        if (chapter is null) {
            return _rowLookup.TryGetValue(row, out var firstRli) ? firstRli : null;
        }

        var id = RowTableElement.Identifier(row, chapter);
        return _allViewItems.TryGetValue(id, out var rb) && rb is RowTableElement rli && !rli.IsDisposed ? rli : null;
    }

    /// <summary>
    /// Macht alle Inline-Edit-Controls unsichtbar.
    /// </summary>
    private void HideAllEditControls() {
        if (_controlStrategyCache.IsDisposed) { return; }

        foreach (var strategy in _controlStrategyCache.Values) {
            if (strategy.Control is Control c
                && !c.IsDisposed
                && c.Visible) {
                c.Visible = false;
            }
        }
    }

    /// <summary>
    /// true, wenn der Block unter dem Header eingeklappt ist.
    /// </summary>
    private bool IsChapterCollapsed(RowCaptionTableElement rcli) {
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            var blockRows = GetChapterBlockRows(rcli);
            return blockRows is { Count: > 0 } && blockRows[0] is { IsDisposed: false } firstRow
                && _collapsedBlockFirstRowKeys.Contains(firstRow.KeyName);
        }
        return !rcli.IsExpanded;
    }

    private TableElement? ItemAtPosition(int controlY, bool ignoreYOffset) {
        if (_sortedViewItems.Count == 0) { return null; }

        for (var i = _sortedViewItems.Count - 1; i >= 0; i--) {
            var thisItem = _sortedViewItems[i];
            if (thisItem is { Visible: true } && thisItem.IgnoreYOffset == ignoreYOffset &&
                thisItem.ControlPosition(Zoom, OffsetX, OffsetY).Contains(1, controlY)) {
                return thisItem;
            }
        }
        return null;
    }

    /// <summary>
    /// Letztes Drop-Ziel oder null.
    /// </summary>
    private TableElement? LastDroppableViewItem() {
        for (var i = _sortedViewItems.Count - 1; i >= 0; i--) {
            if (IsDroppableTarget(_sortedViewItems[i])) { return _sortedViewItems[i]; }
        }
        return null;
    }

    private void OnAutoFilterClicked(FilterEventArgs e) => AutoFilterClicked?.Invoke(this, e);

    //private bool Mouse_IsInAutofilter(ColumnViewItem viewItem, MouseEventArgs e) => viewItem.AutoFilterLocation(Zoom, OffsetX, 0).Contains(e.Location);
    private void OnCellClicked(CellEventArgs e) => CellClicked?.Invoke(this, e);

    private void OnFilterCombinedChanged() =>
                                    // Bestehenden Code belassen
                                    FilterCombinedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnPinnedChanged() {
        // Pin-Spalte erscheint/verschwindet abhängig davon, ob Zeilen angepinnt
        // sind — daher Anordnung (inkl. virtueller Spalten) neu aufbauen.
        Invalidate_CurrentArrangement();
        PinnedChanged?.Invoke(this, System.EventArgs.Empty);
    }

    //DoFilterAndPinButtons(); // Die Flexs reagiren nur auf FilterOutput der Table
    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowNullableEventArgs e) => SelectedRowChanged?.Invoke(this, e);

    private void OnTableChanged() => TableChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnViewChanged() {
        Invalidate_CurrentArrangement();
        Filter.Invalidate_FilteredRows(); // Split-Spalten-Filter
        FilterCombined.Invalidate_FilteredRows();
        Invalidate_MaxBounds();
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    private void RemoveRowItems(RowItem row) {
        var toRemove = _allViewItems.Where(kvp => kvp.Value is RowTableElement rli && !rli.IsDisposed && rli.Row == row)
                                     .Select(kvp => kvp.Key)
                                     .ToList();

        if (toRemove.Count == 0) { return; }
        foreach (var key in toRemove) {
            _allViewItems.Remove(key);
        }
        Invalidate_AllViewItems(false);
    }

    private void Row_RowAdded(object? sender, RowEventArgs e) {
        // Bulk-Batching: Invalidate_CurrentArrangement nur beim ersten Event pro Paint-Zyklus, Rest setzt _mustDoAllViewItems.
        if (_pendingRowAddedRebuild) {
            _mustDoAllViewItems = true;
            return;
        }
        _pendingRowAddedRebuild = true;
        Invalidate_CurrentArrangement();
    }

    private void Row_RowRemoved(object? sender, RowEventArgs e) {
        // Nur bei aktuellen ViewItems ist die Abfrage, ob die Row überhaupt
        // angezeigt wurde, aussagekräftig. Bei pending Invalidate würde
        // GetRow sonst einen unnötigen Voll-Aufbau erzwingen.
        if (_mustDoAllViewItems) { return; }
        if (GetRow(e.Row, null) is not null) {
            Invalidate_AllViewItems(false);
        }
    }

    // im Gegensatz zu Filter.RowsChanged - da sind nur die vorhandenen Zeilen geändert worden
    private void Row_RowRemoving(object? sender, RowEventArgs e) {
        if (IsDisposed) { return; }
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (PinnedRows.Contains(e.Row)) {
            PinnedRows.Remove(e.Row);
            // Pin-Spalte ist von PinnedRows.Count abhängig. Ohne
            // OnPinnedChanged bleibt die virtuelle Pin-Spalte sichtbar,
            // bis ein anderes Invalidate_CurrentArrangement erfolgt.
            OnPinnedChanged();
        }
        // Veraltete Block-Zustände der entfernten Zeile verwerfen, damit
        // keine "Geister-Zustände" im NumberStyle übrig bleiben.
        _collapsedBlockFirstRowKeys.Remove(e.Row.KeyName);
    }

    /// <summary>
    /// Zeilen-Element an der Control-Y-Position. IgnoreYOffset-Elemente (Spaltenkopf etc.) werden bevorzugt.
    /// </summary>
    private TableElement? RowItemAtPosition(int controlY) {
        if (_sortedViewItems is not { Count: > 0 }) {
            _ = AllViewItems; // _sortedViewItems sicherstellen, falls invalidated wurde
            if (_sortedViewItems is not { Count: > 0 }) { return null; }
        }

        // 1. IgnoreYOffset-Elemente (Kopf/Filterleiste) - liegen visuell oben.
        // 2. Normale Zeilen (ohne IgnoreYOffset) - darunter.
        // Reihenfolge ist wichtig, da die IgnoreYOffset-Elemente beim Zeichnen
        // über den normalen Zeilen liegen und Klicks deshalb abfangen müssen.
        return ItemAtPosition(controlY, true) ?? ItemAtPosition(controlY, false);
    }

    /// <summary>
    /// Y-Koordinate, an der der Zeilenbereich beginnt (Unterkante aller IgnoreYOffset-Elemente).
    /// </summary>
    private int RowsAreaTop() {
        _ = AllViewItems; // _sortedViewItems sicherstellen, falls invalidated wurde

        var maxBottom = 0;

        if (_sortedViewItems is { Count: > 0 }) {
            foreach (var thisItem in _sortedViewItems) {
                if (thisItem.IgnoreYOffset) {
                    maxBottom = Math.Max(thisItem.CanvasPosition.Bottom, maxBottom);
                }
            }
        }

        return maxBottom.CanvasToControl(Zoom);
    }

    /// <summary>
    /// Setzt den Auf-/Zuklapp-Zustand eines Kapitel-Headers. NumberStyle: pro Block über _collapsedBlockFirstRowKeys; sonst am Original-Caption.
    /// </summary>
    private void SetChapterExpanded(RowCaptionTableElement rcli, bool expanded) {
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            var blockRows = GetChapterBlockRows(rcli);
            if (blockRows is { Count: > 0 } && blockRows[0] is { IsDisposed: false } firstRow) {
                if (expanded) {
                    _collapsedBlockFirstRowKeys.Remove(firstRow.KeyName);
                } else {
                    _collapsedBlockFirstRowKeys.Add(firstRow.KeyName);
                }
            }
            return;
        }

        rcli.IsExpanded = expanded;
        if (_allViewItems.TryGetValue(RowCaptionTableElement.Identifier(rcli.ChapterText), out var cached)
            && cached is RowCaptionTableElement cachedRcli
            && cachedRcli != rcli) {
            cachedRcli.IsExpanded = expanded;
        }
    }

    /// <summary>
    /// Zeigt die Mini-Toolbar unter der übergebenen Zelle an.
    /// </summary>
    private void ShowMiniToolbarAt(ColumnViewItem column, TableElement? rowItem, RowItem row) {
        if (column.Column is not { IsDisposed: false }) { return; }

        var screenOrigin = PointToScreen(Point.Empty);
        var posX = screenOrigin.X + column.ControlColumnLeft(OffsetX);
        var posY = screenOrigin.Y + (rowItem?.ControlPosition(Zoom, OffsetX, OffsetY).Bottom ?? 0) + 2;

        this.MiniToolbarShow(new Point(posX, posY),
             ContextMenuItemGenerate(this, column, column.Column, row, RowsVisibleUnique()));
    }

    private RowSortDefinition? SortUsed() {
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false } sortCol) {
            return new RowSortDefinition(tb, sortCol, false);
        }
        return _sortDefinitionTemporary ?? Table?.SortDefinition;
    }

    /// <summary>
    /// Erster und letzter Index in _sortedViewItems der Drag-Quell-Zeilen. Bei Kapitel-Block gehört der Header zum Bereich.
    /// </summary>
    private (int firstIdx, int lastIdx) SourceIndexRange(List<RowItem> srcRows, object? dragItem) {
        var first = -1;
        var last = -1;

        // Bei einem Kapitel-Block-Drag gehört der Header ebenfalls zum
        // Quell-Bereich — er verbraucht einen eigenen Index in _sortedViewItems.
        if (dragItem is RowCaptionTableElement rcli) {
            var capIdx = _sortedViewItems.IndexOf(rcli);
            if (capIdx >= 0) { first = capIdx; }
        }

        for (var i = 0; i < _sortedViewItems.Count; i++) {
            if (_sortedViewItems[i] is RowTableElement rli && srcRows.Contains(rli.Row)) {
                if (first < 0 || i < first) { first = i; }
                if (i > last) { last = i; }
            }
        }

        // Kapitel eingeklappt: Die Zeilen sind nicht in _sortedViewItems
        // enthalten. Der Quell-Bereich ist dann nur der Header selbst.
        if (last < 0 && dragItem is RowCaptionTableElement) { last = first; }

        return (first, last);
    }

    private void Table_InvalidateView(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    /// <summary>
    /// Schaltet den Auf-/Zuklapp-Zustand eines Kapitel-Headers um.
    /// </summary>
    private void ToggleChapterExpanded(RowCaptionTableElement rcli) => SetChapterExpanded(rcli, IsChapterCollapsed(rcli));

    /// <summary>
    /// Aktualisiert das Kapitel der verschobenen Zeile anhand des Ziel-Kapitels oberhalb der Mausposition.
    /// </summary>
    private void UpdateChapterOnRowSortMove(RowTableElement sourceRli, int mouseControlY) {
        if (sourceRli.Arrangement is not { IsDisposed: false } ca) { return; }
        if (ca.ColumnForChapter is not { IsDisposed: false } capCol) { return; }
        if (sourceRli.Row is not { IsDisposed: false } sourceRow) { return; }

        _ = AllViewItems; // _sortedViewItems sicherstellen

        // Ziel-Kapitel: letzter Header, dessen Top-Kante auf oder oberhalb der Mausposition liegt.
        string? targetChapterText = null;

        foreach (var item in _sortedViewItems) {
            if (!item.Visible) { continue; }

            var itemTop = item.ControlPosition(Zoom, OffsetX, OffsetY).Top;
            if (itemTop > mouseControlY) { break; }

            if (item is RowCaptionTableElement header) {
                targetChapterText = header.ChapterText;
            }
        }

        // Kein Header oberhalb der Maus → Zeile wird ohne Kapitel auf der
        // obersten Ebene eingeordnet.
        if (string.IsNullOrEmpty(targetChapterText)) {
            if (sourceRow.CellGetString(capCol) is { Length: > 0 }) {
                sourceRow.CellSet(capCol, string.Empty, "Drag/Drop: Kapitel entfernt");
            }
            return;
        }

        var sourceChapter = sourceRli.AlignsToChapter;

        // Nur aktualisieren wenn sich das Kapitel tatsächlich ändert.
        // targetChapterText ist die Original-Schreibweise aus dem Header
        // (normalisiert, aber nicht upper-cased).
        if (!string.Equals(sourceChapter, targetChapterText, StringComparison.OrdinalIgnoreCase)) {
            sourceRow.CellSet(capCol, targetChapterText, "Drag/Drop: Kapitel geändert");
        }
    }

    private string UserEdit_NewRowAllowed() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (tb.Column.First is not { IsDisposed: false } fc) { return "Erste Spalte nicht definiert"; }

        if (CurrentArrangement?[fc] is not { IsDisposed: false }) { return "Erste Spalte nicht sichtbar"; }

        string? chunkValue = null;

        if (fc != tb.Column.ChunkValueColumn) {
            chunkValue = FilterCombined.ChunkVal;
        }

        return tb.IsNowNewRowPossible(chunkValue, true);
    }

    #endregion
}