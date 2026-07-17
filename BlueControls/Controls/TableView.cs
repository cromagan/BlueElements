// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Renderer;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using static BlueTable.Classes.Table;
using BlueControls.Classes.TableItems;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(SelectedRowChanged))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableView : ZoomPad, IContextMenu, ITranslateable, IHasTable, IStyleable {

    #region Fields

    public const string CellDataFormat = "BlueElements.CellLink";

    /// <summary>
    /// Interner Chapter-Marker für angepinnte Zeilen. Erzeugt keinen
    /// sichtbaren Header (AddCaptions legt nur Header für echte Chapter-Werte
    /// der Kapitel-Spalte an). Sorgt dafür, dass angepinnte Zeilen ganz oben
    /// gruppiert werden — ohne eigenen Überschrifts-Eintrag.
    /// </summary>
    public const string DummyPinned = "Angesetzt";

    public const string Ohne = "-?-";
    private readonly Dictionary<string, RowBackground> _allViewItems = [];

    /// <summary>
    /// Wird beim Aufbau der View (<see cref="CalculateAllViewItems_AddCaptionsAndRows"/>)
    /// pro aktuellem Kapitel-Header befüllt. Der Key ist die Header-Instanz aus
    /// <see cref="_sortedViewItems"/>, der Wert alle Zeilen des Blocks — auch
    /// die eingeklappten. So liefert <see cref="GetChapterBlockRows"/> einen
    /// vollständigen Block, ohne dass der Block aufklappen muss (z. B. für
    /// Drag/Drop eines zugeklappten Kapitels).
    /// </summary>
    private readonly Dictionary<RowCaptionListItem, List<RowItem>> _chapterBlockRows = [];

    /// <summary>
    /// Großschreibung
    /// </summary>
    private readonly List<string> _collapsed = [];

    /// <summary>
    /// NumberStyle (SYS_ROWSORTINDEX aktiv): Menge der KeyNames der ersten
    /// Zeile unter jedem eingeklappten Header-Block. Dient dazu, dass
    /// derselbe Chapter-Text mehrfach auftreten kann — jeder Block ist
    /// unabhängig auf-/zuklappbar. Der Key ist stabil gegen UI-Änderungen,
    /// solange die Zeile nicht gelöscht wird.
    /// </summary>
    private readonly HashSet<string> _collapsedBlockFirstRowKeys = [];

    private readonly object _lockUserAction = new();
    private string _arrangement = string.Empty;
    private AutoFilter? _autoFilter;
    private List<RowListItem> _cachedRowViewItems = [];
    private bool _consumeNextMouseDown;
    private int _dragInsertIndex = -1;

    /// <summary>
    /// Das Objekt, das gerade per Drag/Drop verschoben wird.
    /// Entweder ein <see cref="ColumnViewItem"/>, ein <see cref="RowItem"/>
    /// oder ein <see cref="RowCaptionListItem"/> (Kapitel-Block).
    /// </summary>
    private object? _dragItem;

    private Point _dragMouseDown;

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
    private Dictionary<RowItem, RowListItem> _rowLookup = [];
    private List<RowItem> _rowsVisibleUnique = new([]);
    private RowSortDefinition? _sortDefinitionTemporary;
    private List<RowBackground> _sortedViewItems = [];
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
    /// Gibt an, ob das Standard-Kontextmenu der Tabellenansicht angezeitgt werden soll oder nicht
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
                // Arrangement wurde neu aufgebaut: Batching-Flag zurücksetzen,
                // damit der nächste RowAdded wieder eine Invalidierung auslöst.
                _pendingRowAddedRebuild = false;
            }

            // Lokale Kopie des Backing-Fields: Invalidate_CurrentArrangement kann
            // (z. B. über Table-DisposingEvent oder synchrones Refresh im Table-Setter)
            // field zwischen den einzelnen Zugriffen auf null setzen. Mit der lokalen
            // Variablen wird auf einer konsistenten Momentaufnahme gearbeitet.
            var ca = field;

            if (ca is { IsDisposed: false }) {
                ca.Ansichtbearbeitung = Ansichtbearbeitung;

                // On-demand virtuelle Spalten ein-/ausblenden:
                // - Hinzufügen: bei Ansichtbearbeitung (oder Admin ohne echte Spalten)
                // - Pin: wenn mindestens eine Zeile angepinnt ist
                // Persistente virtuelle Spalten (VIR_…) stehen bereits in der
                // Collection und werden vom Reconcile nicht angerührt.
                // Die Number-Spalte hat keinen on-demand-Trigger.
                var needAdd = Ansichtbearbeitung
                    || (tb.IsAdministrator() && tb.Column.Count > 0 && ca.First() is null);
                var needPin = PinnedRows.Count > 0;

                ca.ReconcileVirtualColumns(needPin, needAdd);
            } else if (ca is { IsDisposed: true }) {
                // Veraltete (disposete) Collection verwerfen, beim nächsten
                // Zugriff wird neu aufgebaut.
                field = null;
                return null;
            }

            if (ca is not null) {
                ca.SheetStyle = SheetStyle;
                ca.ComputeAllColumnPositions(AvailableControlPaintArea.Width, Zoom);
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
    public RowListItem? CursorPosRow { get; private set; }

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; }

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
    /// Zusammengeführtes Ergebnis aus Filter und FilterFix.
    /// Dies ist der tatsächlich aktive Filter, der die angezeigten Zeilen bestimmt.
    /// Wird automatisch bei Änderungen von Filter oder FilterFix aktualisiert.
    /// </summary>
    public FilterCollection FilterCombined { get; } = new("TableFilterCombined");

    /// <summary>
    /// Fixfilter, die von übergeordneten Elementen (ConnectedFormula) übergeben wurden.
    /// Können vom Benutzer nicht geändert werden.
    /// Werden bei FilterCombined berücksichtigt und auch im FilterOutput weitergegeben.
    /// </summary>
    public FilterCollection FilterFix { get; } = new("FilterFix");

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

    public List<RowListItem> RowViewItems => [.. _cachedRowViewItems];

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
    /// Aktuell zugewiesene Tabelle. Beim Setzen werden alle Events automatisch angebunden/abgemeldet.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? Table {
        get;
        set {
            if (field == value) { return; }

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
                tb1.DisposingEvent -= _table_Disposing;
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
                tb2.DisposingEvent += _table_Disposing;
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
    /// Interne Benutzerfilter (AutoFilter, Textsuche, FlexiControls).
    /// Dürfen vom Control frei geändert werden.
    /// Werden zusammen mit FilterFix in FilterCombined zusammengeführt.
    /// </summary>
    internal FilterCollection Filter { get; } = new("DefaultTableFilter");

    protected override bool ShowSliderX => true;

    protected override int SmallChangeY => 10;

    private Dictionary<string, RowBackground>? AllViewItems {
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
    /// Erzeugt den Kontext für ein Rechtsklick-Menü mit Angabe der
    /// <see cref="ColumnViewItem" />. Dies ist für virtuelle Spalten
    /// (Pin, Nummer, Hinzufügen) erforderlich, die kein eigenes
    /// <see cref="ColumnItem" /> haben. Ist der ViewItem einer echten
    /// Spalte zugeordnet, wird deren ColumnItem zusätzlich gesetzt.
    /// </summary>
    public static object ContextMenuItemGenerate(TableView tableView, ColumnViewItem? viewItem, ColumnItem? column, RowItem? row, IReadOnlyList<RowItem>? visibleRows) {
        return new { Column = column, Row = row, VisibleRows = visibleRows, TableView = tableView, ViewItem = viewItem };
    }

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
        var colFirst = tb.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder_TextOneLine.Instance);
        var colDate = tb.Column.GenerateAndAdd("Aenderdatum", "Änderdatum", ColumnFormatHolder_DateTime.Instance);
        var colAnderer = tb.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder_TextOneLine.Instance);
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

    public static string Export_CSV(Table tbl, FirstRow firstRow, IEnumerable<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows) {
        var columns = columnList?.ToList() ?? [.. tbl.Column.Where(c => c is not null)];

        var sb = new StringBuilder();
        switch (firstRow) {
            case FirstRow.Without:
                break;

            case FirstRow.ColumnCaption:
                AppendCsvRow(sb, columns, c => c.ReadableText().Replace(';', '|').Replace(" |", "|").Replace("| ", "|"));
                break;

            case FirstRow.ColumnInternalName:
                AppendCsvRow(sb, columns, c => c.KeyName);
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow is not { IsDisposed: false }) { continue; }
            AppendCsvRow(sb, columns, c => FormatCellForCsv(thisRow, c));
        }

        return sb.ToString().TrimEnd('\r', '\n');
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
        using ImportCsv x = new(table, csvtxt);
        x.ShowDialog();
    }

    public static List<string> Permission_AllUsed(bool mitRowCreator) {
        var l = new List<string>();

        foreach (var thisTb in AllFiles) {
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

    public static Renderer_Abstract RendererOf(ColumnItem? column, string style) {
        if (column is null || string.IsNullOrEmpty(column.DefaultRenderer)) { return Renderer_Abstract.Default; }
        return RendererOf(column.DefaultRenderer, column.RendererSettings, style);
    }

    /// <summary>
    /// Erzeugt einen Renderer aus Typname und serialisierten Einstellungen.
    /// Wird für virtuelle Spalten benötigt, die kein eigenes ColumnItem haben.
    /// Bei ungültigem Typ oder fehlerhaften Einstellungen wird der
    /// Standard-Renderer geliefert.
    /// </summary>
    public static Renderer_Abstract RendererOf(string? rendererString, string rendererSettings, string style) {
        if (string.IsNullOrEmpty(rendererString)) { return Renderer_Abstract.Default; }

        var renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(rendererString);
        if (renderer is null) { return Renderer_Abstract.Default; }

        if (!renderer.Parse(rendererSettings)) { return Renderer_Abstract.Default; }
        renderer.SheetStyle = style;

        return renderer;
    }

    public static void SearchNextText(string searchTxt, TableView tableView, ColumnViewItem? column, RowListItem? row, out ColumnViewItem? foundColumn, out RowListItem? foundRow, bool vereinfachteSuche) {
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
            if (thisf is TableScriptEditor tbs && tbs.Table != tbl) { e.CancelReason = "Fremder Skript Editor geöffnet"; return; }
        }
    }

    //    if (column.Function == ColumnFunction.Verknüpfung_zu_anderer_Tabellex) {
    //        var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
    //        return lcolumn is not null && lrow is not null ? ContentSize(lcolumn, lrow, renderer)
    //            : new CanvasSize(16, 16);
    //    }
    public static string Table_NeedPassword() => InputBox.Show("Bitte geben sie das Passwort ein,<br>um Zugriff auf diese Tabelle<br>zu erhalten:", string.Empty, FormatHolder_Text.Instance);

    public static void WriteColumnArrangementsInto(ComboBox? columnArrangementSelector, Table? table, string showingKey) {
        if (columnArrangementSelector is not { IsDisposed: false }) { return; }

        columnArrangementSelector.AutoSort = false;

        columnArrangementSelector.ItemClear();
        columnArrangementSelector.DropDownStyle = ComboBoxStyle.DropDownList;

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
            showingKey = columnArrangementSelector.ItemCount > 1 ? columnArrangementSelector[1].KeyName ?? string.Empty : string.Empty;
        }

        columnArrangementSelector.Text = showingKey;
    }

    public (ColumnViewItem? column, RowBackground? row) CellOnLastMouseDown() => (ColumnOnCoordinate(CurrentArrangement, MouseDownData), RowItemAtPosition(MouseDownData?.ControlY ?? 0));

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
                if (thisItem is RowCaptionListItem { IsDisposed: false } rcli
                    && GetChapterBlockRows(rcli) is { Count: > 0 } blockRows
                    && blockRows[0] is { IsDisposed: false } firstRow) {
                    did |= _collapsedBlockFirstRowKeys.Add(firstRow.KeyName);
                }
            }
        } else {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowCaptionListItem { IsDisposed: false, IsExpanded: true } rcli) { rcli.IsExpanded = false; did = true; }
            }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    public void CursorPos_Set(ColumnViewItem? column, RowBackground? row, bool ensureVisible) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is null || column is null ||
            CurrentArrangement is not { IsDisposed: false } ca2 || !ca2.Contains(column) ||
            AllViewItems is not { } avi || !avi.ContainsValue(row)) {
            column = null;
            row = null;
        }

        var sameRow = CursorPosRow == row;

        if (CursorPosColumn == column && CursorPosRow == row) { return; }
        QuickInfo = string.Empty;
        CursorPosColumn = column;
        CursorPosRow = row as RowListItem;

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

    public bool EnsureVisible(ColumnViewItem? viewItem, RowBackground? row) => EnsureVisible(viewItem) && EnsureVisible(row);

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
                if (thisItem is RowCaptionListItem { IsDisposed: false, IsExpanded: false } rcli) { rcli.IsExpanded = true; did = true; }
            }
        }

        if (did) {
            CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

            Invalidate_AllViewItems(false);
        }
    }

    public string Export_CSV(FirstRow firstRow) => Table is null ? string.Empty : Export_CSV(Table, firstRow, CurrentArrangement?.ListOfUsedColumn(), RowsVisibleUnique());

    public string Export_CSV(FirstRow firstRow, ColumnItem onlyColumn) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return string.Empty; }
        List<ColumnItem> l = [onlyColumn];
        return Export_CSV(Table, firstRow, l, RowsVisibleUnique());
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
                if (thisItem is RowListItem { IsDisposed: false } rdli && rdli.Visible) {
                    da.RowBeginn();
                    foreach (var thisColumn in ca) {
                        if (thisColumn.Column is not null) {
                            var lcColumn = thisColumn.Column;
                            var lCrow = rdli.Row;

                            if (lcColumn is not null) {
                                da.CellAdd(string.Join("<br>", lCrow.CellGetList(lcColumn)), thisColumn.Column.BackColor);
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
        if (Focused) { return; }
        base.Focus();
    }

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) {
        List<AbstractListItem> contextMenu = [];

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
                    contextMenu.Add(ItemOf("Zeile nicht mehr pinnen", ImageCode.Pinnadel, ContextMenu_Unpin, true));
                } else {
                    contextMenu.Add(ItemOf("Zeile anpinnen", ImageCode.Pinnadel, ContextMenu_Pin, true));
                }
            }

            #endregion

            if (column is not null && row is not null) {
                contextMenu.Add(ItemOf("Notiz", true));
                var existingNote = CellNoteHelper.GetNoteData(column, row);
                contextMenu.Add(ItemOf("Notiz bearbeiten", ImageCode.Stift, ContextMenu_Note_Edit, true));
                contextMenu.Add(ItemOf("Notiz entfernen", ImageCode.Kreuz, ContextMenu_Note_Remove, existingNote is not null));
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
                var editable = string.IsNullOrEmpty(CellCollection.IsCellEditable(column, row, row.ChunkValue));

                contextMenu.Add(ItemOf("Zelle", true));

                contextMenu.Add(ItemOf("Inhalt kopieren", ImageCode.Kopieren, ContextMenu_ContentCopy, column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Inhalt einfügen", ImageCode.Clipboard, ContextMenu_ContentPaste, editable && column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Inhalt löschen", ImageCode.Radiergummi, ContextMenu_ContentDelete, editable && column.CanBeChangedByRules()));
                contextMenu.Add(ItemOf("Vorherigen Inhalt wiederherstellen", QuickImage.Get(ImageCode.Undo, 16), ContextMenu_RestorePreviousContent, editable && column.CanBeChangedByRules() && column.SaveContent, string.Empty));
                contextMenu.Add(ItemOf("Suchen und ersetzen", QuickImage.Get(ImageCode.Lupe, 16), ContextMenu_SearchAndReplace, tb.IsAdministrator(), string.Empty));
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
                    contextMenu.Add(ItemOf("Spalte ausblenden", ImageCode.Kreuz, ContextMenu_HideOrDeleteColumn, true));
                } else {
                    contextMenu.Add(ItemOf("Spalteneigenschaften bearbeiten", ImageCode.Stift, ContextMenu_EditColumnProperties, tb.IsAdministrator()));

                    if (IsAnsicht0(ca)) {
                        contextMenu.Add(ItemOf("Spalte permanent löschen", ImageCode.Papierkorb, ContextMenu_HideOrDeleteColumn, tb.IsAdministrator()));
                    } else {
                        contextMenu.Add(ItemOf("Spalte ausblenden", ImageCode.Kreuz, ContextMenu_HideOrDeleteColumn, true));
                    }

                    contextMenu.Add(ItemOf("Spalte erstellen / einblenden", ImageCode.PlusZeichen, ContextMenu_NewColumn, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren", ImageCode.Clipboard, ContextMenu_CopyAll, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren + sortieren", ImageCode.Clipboard, ContextMenu_CopyAllSorted, tb.IsAdministrator()));
                    contextMenu.Add(ItemOf("Statistik", QuickImage.Get(ImageCode.Balken, 16), ContextMenu_Statistics, tb.IsAdministrator(), string.Empty));
                    contextMenu.Add(ItemOf("Summe", ImageCode.Summe, ContextMenu_Sum, tb.IsAdministrator()));
                }
            }

            #endregion

            if (row is not null) {
                contextMenu.Add(ItemOf("Zeile", true));

                contextMenu.Add(ItemOf("Zeile löschen", QuickImage.Get(ImageCode.Kreuz, 16), ContextMenu_DeleteRow, tb.IsAdministrator() && tb.IsThisScriptOk(ScriptEventTypes.row_deleting, true), string.Empty));
                contextMenu.Add(ItemOf("Komplette Datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), ContextMenu_DataValidation, tb.CanDoValueChangedScript(true), string.Empty));

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

    public string IsCellEditable(ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, string? newChunkVal, bool maychangeview) {
        if (CellCollection.IsCellEditable(cellInThisTableColumn?.Column, cellInThisTableRow?.Row, newChunkVal) is { Length: > 0 } f) { return f; }

        // CellCollection.IsCellEditable kann bei Chunk-Spalten über TableChunk.IsValueEditable
        // einen Chunk-Ladevorgang auslösen, der OnLoaded und damit Invalidate_CurrentArrangement
        // feuert. Das bisherige ColumnViewItem ist danach nicht mehr im neu erzeugten Arrangement
        // enthalten. Deswegen über die zugrundeliegende Spalte neu auflösen.
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
        PinnedRows.Add(row);
        Invalidate_AllViewItems(false);
        OnPinnedChanged();
    }

    public void PinRemove(RowItem? row) {
        if (row is not { IsDisposed: false }) { return; }
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
            CursorPos_Set(CurrentArrangement?[column], GetRow(row, false), false);
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
    /// Schaltet den Auf-/Zuklapp-Zustand aller Kapitel um: Ist mindestens
    /// ein Kapitel ausgeklappt, werden alle geschlossen. Sind alle
    /// eingeklappt, werden alle geöffnet.
    /// </summary>
    public void ToggleAllChapters() {
        if (AllViewItems is not { } avi) { return; }

        var anyExpanded = false;

        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            // NumberStyle: ein Block ist ausgeklappt, wenn sein FirstRow-Key
            // nicht in _collapsedBlockFirstRowKeys verzeichnet ist.
            foreach (var thisItem in _sortedViewItems) {
                if (thisItem is RowCaptionListItem { IsDisposed: false } rcli
                    && GetChapterBlockRows(rcli) is { Count: > 0 } blockRows
                    && blockRows[0] is { IsDisposed: false } firstRow
                    && !_collapsedBlockFirstRowKeys.Contains(firstRow.KeyName)) {
                    anyExpanded = true;
                    break;
                }
            }
        } else {
            foreach (var thisItem in avi.Values) {
                if (thisItem is RowCaptionListItem { IsDisposed: false, IsExpanded: true }) {
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

    public RowListItem? View_NextRow(RowListItem? row) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        var idx = _sortedViewItems.IndexOf(row);
        return idx < 0 ? null : FindVisibleRowListItem(idx + 1, 1);
    }

    public RowListItem? View_PreviousRow(RowListItem? row) {
        if (IsDisposed || Table is not { IsDisposed: false } || row is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        var idx = _sortedViewItems.IndexOf(row);
        return idx < 0 ? null : FindVisibleRowListItem(idx - 1, -1);
    }

    public RowListItem? View_RowFirst() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
        _ = AllViewItems;
        return FindVisibleRowListItem(0, 1);
    }

    public RowListItem? View_RowLast() {
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

    internal static void RepairColumnArrangements(Table tb) {
        if (!string.IsNullOrEmpty(tb.IsGenericEditable(false))) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        for (var z = 0; z < Math.Max(2, tcvc.Count); z++) {
            if (tcvc.Count < z + 1) { tcvc.Add(new ColumnViewCollection(tb, string.Empty)); }
            tcvc[z].Repair(z);
        }

        tb.ColumnArrangements = tcvc.AsReadOnly();
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
    /// Ermittelt die Zeilen des zusammenhängenden Kapitel-Blocks, der unter
    /// dem übergebenen Header beginnt. Primär über das Mapping
    /// <see cref="_chapterBlockRows"/>, das beim View-Aufbau pro Header alle
    /// Block-Zeilen enthält — auch die eingeklappten. So funktioniert der
    /// Zugriff unabhängig vom Aufklapp-Zustand (z. B. Drag eines zugeklappten
    /// Kapitels). Fällt das Mapping leer aus, wird als Fallback in
    /// <see cref="_sortedViewItems"/> per Referenzgleichheit gesucht; der Block
    /// endet am nächsten Kapitel-Header. Wird der Header nicht gefunden,
    /// wird <c>null</c> zurückgegeben.
    /// </summary>
    internal List<RowItem>? GetChapterBlockRows(RowCaptionListItem header) {
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
            if (item is RowCaptionListItem) { break; }

            if (item is RowListItem rli && rli.Row is { IsDisposed: false } row) {
                rows.Add(row);
            }
        }

        return found ? rows : null;
    }

    internal void Invalidate_CurrentArrangement() {
        CurrentArrangement = null;
        Invalidate_AllViewItems(false); // Spaltenbreite, Slider
        Invalidate();
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

        // AllViewItems sicherstellen, damit _sortedViewItems befüllt ist.
        // Ohne diesen Aufruf wäre _sortedViewItems leer, weil DrawControl
        // erst NACH base.DrawControl() auf AllViewItems zugreift.
        _ = AllViewItems;

        if (CurrentArrangement is { } ca) {
            // Indent wird beim Zeichnen zu den Spalten-Positionen addiert
            // (RowBackground.DrawExplicit). Daher muss der maximale Indent aller
            // sichtbaren Zeilen auch in die Canvas-Breite einfließen — sonst
            // werden eingerückte Spalten am rechten Rand abgeschnitten.
            var maxIndent = _sortedViewItems is { Count: > 0 } items ? items.Max(i => i.Indent) : 0;
            x = (int)ca.ControlColumnsWidth().ControlToCanvas(Zoom) + RowBackground.IndentWidth * maxIndent;
        }

        if (_sortedViewItems is { Count: > 0 }) {
            (_, _, y, _) = CanvasItemData(_sortedViewItems, Design.Item_ListBox);
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

                FilterCombined.Dispose();
                FilterFix.Dispose();
                Filter.Dispose();

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

            if (ca.Count < 1) {
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

            avi.TryGetValue(TableEndListItem.Identifier, out var teli);

            if (teli is not TableEndListItem || !teli.Visible) {
                DrawWaitScreen(gr, "Fehler in der Zeilenberechung");
                _tableDrawError = DateTime.UtcNow; // Cooldown aktivieren statt Invalidate-Loop
                return;
            }

            if (ca.ShowHead) {
                avi.TryGetValue(ColumnsHeadListItem.Identifier, out var rcli);

                if (rcli is not ColumnsHeadListItem rowcap || !rowcap.IsVisible(AvailableControlPaintArea, Zoom, OffsetX, OffsetY)) {
                    DrawWaitScreen(gr, "Fehler in der Zeilenberechung");
                    _tableDrawError = DateTime.UtcNow; // Cooldown aktivieren statt Invalidate-Loop
                    return;
                }
            }

            if (state.HasFlag(States.Standard_Disabled)) { CursorPos_Reset(); }

            ca.SheetStyle = SheetStyle;
            ca.ComputeAllColumnPositions(AvailableControlPaintArea.Width, Zoom);

            // Haupt-Aufbau-Routine ------------------------------------
            // Zuerst Zeilen (ohne IgnoreYOffset) zeichnen, dann Kopfzeilen (mit IgnoreYOffset) darüber,
            // damit der Spaltenkopf beim Scrollen nicht von Zeilen überdeckt wird.
            // Die Where-Enumeratoren sind bewusst lazy; ein ToList würde bei jedem Paint
            // neue Listen allozieren.
            // Normale Zeilen, die vollständig hinter dem fixierten Header liegen,
            // werden über den clipTop-Parameter ausgeschlossen und gar nicht erst gezeichnet.
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
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        lock (_lockUserAction) {
            if (_isinDoubleClick) { return; }
            _isinDoubleClick = true;
            try {
                var (_mouseOverColumn, _mouseOverRow) = CellOnLastMouseDown();

                if (_mouseOverRow is RowListItem rli) {
                    Cell_Edit(_mouseOverColumn, rli, true, rli.Row.ChunkValue);
                } else if (_mouseOverRow is NewRowListItem nrli) {
                    if (tb.Column.ChunkValueColumn == tb.Column.First) {
                        Cell_Edit(_mouseOverColumn, nrli, true, null);
                    } else {
                        Cell_Edit(_mouseOverColumn, nrli, true, FilterCombined.ChunkVal);
                    }
                } else if (_mouseOverRow is ColumnsHeadListItem chli && _mouseOverColumn is { IsDisposed: false } cvi && cvi.Column is { IsDisposed: false }) {
                    if (IsAdministrator()) { chli.EditCaption(cvi, this); }
                } else if (Ansichtbearbeitung && _mouseOverRow is CaptionBarListItem cbli && IsAdministrator()) {
                    var anchor = _mouseOverColumn is { IsDisposed: false } anchorCvi ? anchorCvi : CurrentArrangement?.FirstOrDefault();
                    if (anchor is { IsDisposed: false } && anchor.Column is { IsDisposed: false }) {
                        cbli.EditCaptionGroup(anchor, this);
                    }
                } else if (_mouseOverRow is RowCaptionListItem rcli && rcli.CanEditChapter
                           && !rcli.IsArrowButtonHit(MouseDownData?.ControlX ?? 0, MouseDownData?.ControlY ?? 0, Zoom, OffsetX, OffsetY)) {
                    rcli.EditChapter(this);
                }
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
            || CursorPosColumn?.Column is not { IsDisposed: false } c
            || CursorPosRow?.Row is not { IsDisposed: false } r) { return; }

        lock (_lockUserAction) {
            if (_isinKeyDown) { return; }
            _isinKeyDown = true;
            try {
                //_table.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

                // var chunkval = r.ChunkValue;
                switch (e.KeyCode) {
                    //case Keys.Oemcomma: // normales ,
                    //    if (e.Modifiers == Keys.Control) {
                    //        var lp = EditableErrorReason(CursorPosColumn, CursorPosRow, EditableErrorReasonType.EditCurrently, true, false, true, chunkval);
                    //        Neighbour(CursorPosColumn, CursorPosRow, Direction.Oben, out _, out var newRow);
                    //        if (newRow == CursorPosRow) { lp = "Das geht nicht bei dieser Zeile."; }
                    //        if (string.IsNullOrEmpty(lp) && newRow?.Row is not null) {
                    //            UserEdited(this, newRow.Row.CellGetString(c), CursorPosColumn, CursorPosRow, true, oldval);
                    //        } else {
                    //            NotEditableInfo(lp);
                    //        }
                    //    }
                    //    break;

                    case Keys.X:
                        if (e.Modifiers == Keys.Control) {
                            var cp = CursorPosRow?.ControlPosition(Zoom, OffsetX, OffsetY) ?? Rectangle.Empty;
                            CopyToClipboard(c, CursorPosRow?.Row, true, PointToScreen(new Point(CursorPosColumn?.ControlColumnRight(OffsetX) ?? 0, cp.Y)));
                            NotEditableInfo(UserEdited(this, c.DefaultValueForColumn(), CursorPosColumn, CursorPosRow, true));
                        }
                        break;

                    case Keys.Delete:
                        NotEditableInfo(UserEdited(this, c.DefaultValueForColumn(), CursorPosColumn, CursorPosRow, true));
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
                    case Keys.PageUp: //Bildab
                    case Keys.Home:
                    case Keys.End:
                        CursorPos_Reset();
                        break;

                    case Keys.C:
                        if (e.Modifiers == Keys.Control) {
                            var cp = CursorPosRow?.ControlPosition(Zoom, OffsetX, OffsetY) ?? Rectangle.Empty;
                            CopyToClipboard(c, CursorPosRow?.Row, true, PointToScreen(new Point(CursorPosColumn?.ControlColumnRight(OffsetX) ?? 0, cp.Y)));
                        }
                        break;

                    case Keys.F:
                        if (e.Modifiers == Keys.Control) {
                            OpenSearchInCells();
                        }
                        break;

                    case Keys.F2:
                        Cell_Edit(CursorPosColumn, CursorPosRow, true, r.ChunkValue);
                        break;

                    case Keys.V:
                        if (e.Modifiers == Keys.Control) {
                            PasteToCursor();
                        }
                        break;
                }
            } finally {
                _isinKeyDown = false;
            }
        }
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (CurrentArrangement is not { IsDisposed: false } ca) { return; }

        // Wurde eine Edit-TextBox durch Klick auf die Tabelle geschlossen
        // (LostFocus), konsumiert dieser Klick nur das Schließen — die
        // eigentliche Aktion (Cursor setzen etc.) darf erst beim nächsten
        // Klick erfolgen.
        if (_consumeNextMouseDown) {
            _consumeNextMouseDown = false;
            return;
        }

        lock (_lockUserAction) {
            if (_isinMouseDown) { return; }
            _isinMouseDown = true;
            //_table.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());
            try {
                var (_mouseOverColumn, _mouseOverRow) = CellOnCoordinate(ca, e);
                // Die beiden Befehle nur in Mouse Down!
                // Wenn der Cursor bei Click/Up/Down geändert wird, wird ein Ereignis ausgelöst.
                // Das könnte auch sehr Zeit intensiv sein. Dann kann die Maus inzwischen wo ander sein.
                // Somit würde das Ereignis doppelt und dreifach ausgelöste werden können.
                // Beipiel: MouseDown-> Bildchen im Pad erzeugen, dauert.... Maus bewegt sich
                //          MouseUp  -> Cursor wird umgesetzt, Ereginis CursorChanged wieder ausgelöst, noch ein Bildchen
                // Auf-/Zuklappen NUR über den Pfeil-Button links.
                // Klick auf das Wort startet Drag/Drop (siehe unten).
                if (_mouseOverRow is RowCaptionListItem rcli
                    && rcli.IsArrowButtonHit(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY)) {
                    CursorPos_Reset(); // Wenn eine Zeile markiert ist, man scrollt und expandiert, springt der Screen zurück, was sehr irriteiert

                    ToggleChapterExpanded(rcli);
                    Invalidate_AllViewItems(false);
                }
                EnsureVisible(_mouseOverColumn, _mouseOverRow);
                CursorPos_Set(_mouseOverColumn, _mouseOverRow, false);

                // Drag/Drop-Potential speichern.
                // Der Drag selbst startet erst in OnMouseMove nach Überschreiten
                // einer Bewegungsschwelle (vermeidet versehentliches Dragging bei Klick).
                // Bei einem Kapitel-Header werden alle Zeilen des Blocks verschoben.
                _dragItem = null;

                if (e.Button == MouseButtons.Left && !IsAnsicht0(ca)
                    && _mouseOverColumn is { IsDisposed: false } dragCvi && dragCvi.IsOk()) {
                    var mc = dragCvi.Column;
                    if (_mouseOverRow is RowListItem dragRli
                        && dragRli.Row is { IsDisposed: false } dragRow
                        && !PinnedRows.Contains(dragRow)
                        && string.IsNullOrEmpty(CellCollection.IsCellEditable(mc, dragRow, dragRow.ChunkValue))) {
                        _dragItem = dragRow;
                    } else if (_mouseOverRow is RowCaptionListItem dragRcli
                               && !dragRcli.IsArrowButtonHit(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY)
                               && dragRcli.CanEditChapter
                               && Table.Column.SysRowSortIndex is { IsDisposed: false }) {
                        // Den Block NICHT aufklappen — GetChapterBlockRows
                        // liefert über _chapterBlockRows alle Zeilen des
                        // Blocks, auch wenn er eingeklappt ist.
                        _ = AllViewItems; // _sortedViewItems sicherstellen

                        // Den aktuellen Header über die Mausposition finden.
                        var blockHeader = _sortedViewItems?.OfType<RowCaptionListItem>()
                            .FirstOrDefault(h => string.Equals(h.ChapterText, dragRcli.ChapterText, StringComparison.OrdinalIgnoreCase)
                                                 && h.ControlPosition(Zoom, OffsetX, OffsetY).Contains(e.ControlX, e.ControlY))
                            ?? dragRcli;

                        // Nur als Drag-Quelle merken, wenn der Block verschiebbare Zeilen enthält.
                        if (GetDragSourceRows(blockHeader).Count > 0) {
                            _dragItem = blockHeader;
                        }
                    } else if (_mouseOverRow is RowBackground { IgnoreYOffset: true } and not NewRowListItem
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
                            AutoScrollDuringColumnDrag(e.ControlX);
                        } else {
                            _dragInsertIndex = inside ? CalculateRowSortInsertIndex(e.ControlY) : -1;
                            AutoScrollDuringDrag(e.ControlY);
                        }
                        Invalidate();
                        return;
                    }
                }

                var (_mouseOverColumn, _mouseOverRowItem) = CellOnCoordinate(ca, e);

                if (_mouseOverColumn is { IsDisposed: false } &&
                    _mouseOverRowItem is RowBackground { } rbi &&
                    e.Button == MouseButtons.None) {
                    var mxInCol = e.ControlX - _mouseOverColumn.ControlColumnLeft(OffsetX);
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
            var _mouseOverRow = _mouseOverRowItem as RowListItem;

            // TXTBox_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder gschlossen wird
            // AutoFilter_Close() NICHT! Weil sonst nach dem Öffnen sofort wieder geschlossen wird
            FloatingForm.Close(this, Design.Form_ContextMenu);

            if (_mouseOverColumn is not { IsDisposed: false }) { return; }

            var isRealColumn = _mouseOverColumn.Column is { IsDisposed: false };

            if (e.Button == MouseButtons.Left) {
                if (isRealColumn && _mouseOverRowItem is FilterBarListItem cfli) {
                    var screenX = Cursor.Position.X - e.ControlX;
                    var screenY = Cursor.Position.Y - e.ControlY;
                    AutoFilter_Show(ca, _mouseOverColumn, screenX, screenY, cfli.ControlPosition(Zoom, OffsetX, OffsetY).Bottom);
                    return;
                }

                if (isRealColumn && _mouseOverRowItem is CollapesBarListItem && _mouseOverColumn.CollapsableEnabled(SheetStyle)) {
                    _mouseOverColumn.IsExpanded = !_mouseOverColumn.IsExpanded;
                    Invalidate_AllViewItems(false);
                    return;
                }

                if (_mouseOverRowItem is RowBackground rbli) {
                    var mouseXinColumn = e.ControlX - _mouseOverColumn.ControlColumnLeft(OffsetX);
                    var mouseYinColumn = e.ControlY - rbli.ControlPosition(Zoom, OffsetX, OffsetY).Top;
                    if (rbli.HandleClick(ca, _mouseOverColumn, mouseXinColumn, mouseYinColumn, Zoom, this)) {
                        Invalidate_CurrentArrangement();
                        return;
                    }
                }

                if (isRealColumn && _mouseOverRow?.Row is { IsDisposed: false } r) {
                    OnCellClicked(new CellEventArgs(_mouseOverColumn.Column!, r));
                    Invalidate();
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
                Invalidate_AllViewItems(false); // Zellen können ihre Größe ändern. z.B. die Zeilenhöhe
                                                //CurrentArrangement?.Invalidate_DrawWithOfAllItems();
            } finally {
                _isinSizeChanged = false;
            }
        }
    }

    protected void OnViewLoading(JsonEventArgs e) => ViewLoading?.Invoke(this, e);

    protected void OnViewSaving(JsonEventArgs e) => ViewSaving?.Invoke(this, e);

    protected override void OnZoomChanged() {
        Invalidate_CurrentArrangement();
        base.OnZoomChanged();
    }

    protected override void WndProc(ref Message m) {
        const int WM_MOUSEWHEEL = 0x020A;
        if (m.Msg == WM_MOUSEWHEEL && (BTB.Visible || BCB.Visible || BTS.Visible)) {
            return;
        }
        base.WndProc(ref m);
    }

    private static void AppendCsvRow(StringBuilder sb, List<ColumnItem> columns, Func<ColumnItem, string> formatter) {
        for (var colNr = 0; colNr < columns.Count; colNr++) {
            if (columns[colNr] is { } col) {
                sb.Append(formatter(col));
                if (colNr < columns.Count - 1) { sb.Append(';'); }
            }
        }
        sb.Append("\r\n");
    }

    private static HashSet<string> CalculateAllViewItems_AddCaptions(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement, List<RowItem> filteredRows, HashSet<string> parentCaps) {
        HashSet<string> allCaps = [];

        // NumberStyle (SYS_ROWSORTINDEX aktiv): strikte Sortierung hat Vorrang.
        // Der Kapitel-Trenner '\' wird komplett ignoriert — jeder Kapitel-Wert
        // ist ein flaches Kapitel auf einer Ebene, keine Hierarchie, keine
        // Unterkapitel, keine "X\Ohne"-Sub-Section.
        var numberStyle = arrangement.Table is { IsDisposed: false } tb
                          && tb.Column.SysRowSortIndex is { IsDisposed: false };

        if (arrangement.ColumnForChapter is { IsDisposed: false } cap) {
            var caps = cap.Contents(filteredRows, Ohne);

            foreach (var capValue in caps) {
                if (numberStyle) {
                    allCaps.Add(capValue.ChapterPathNormalize());
                } else {
                    allCaps.UnionWith(capValue.ChapterPathHierarchy());
                }
            }

            if (!numberStyle) {
                foreach (var capValue in caps) {
                    var norm = capValue.ChapterPathNormalize();
                    if (parentCaps.Contains(norm)) {
                        allCaps.UnionWith((norm + RowCaptionListItem.Kapiteltrenner + Ohne).ChapterPathHierarchy());
                    }
                }
            }

            if (caps.Count == 0) {
                allCaps.Add(Ohne);
            }
        }

        foreach (var thisCap in allCaps) {
            var capId = RowCaptionListItem.Identifier(thisCap);
            if (allItems.TryGetValue(capId, out var existingCap) && existingCap is RowCaptionListItem existingRcli) {
                existingRcli.Arrangement = arrangement;
            } else {
                var capi = new RowCaptionListItem(thisCap, arrangement);
                allItems[capi.KeyName] = capi;
            }
        }

        return allCaps;
    }

    private static List<string> CalculateAllViewItems_CaptionOrder(List<RowListItem> sortedRows) {
        var captionOrder = new List<string>();

        // Angepinnte Zeilen erscheinen ganz oben — ihr Chapter-Marker
        // (ohne eigenen Header) muss in captionOrder zuerst stehen.
        if (sortedRows.Exists(x => string.Equals(x.AlignsToChapter, DummyPinned, StringComparison.OrdinalIgnoreCase))) {
            captionOrder.Add(DummyPinned.ToUpperInvariant());
        }

        // Caption-Reihenfolge aus der sortierten Liste ableiten.
        // Für jede Zeile die vollständige Pfad-Hierarchie eintragen — bei
        // "PFAD1\PFAD2" also sowohl "PFAD1" als auch "PFAD1\PFAD2". So werden
        // auch die Parent-Header an der Position ihres ersten Unter-Eintrags
        // eingefügt (analog zum Windows Datei-Explorer).
        // Das bestimmt später nur noch die Geschwister-Reihenfolge INNERHALB einer Ebene.
        var discovered = new List<string>();
        foreach (var dataItem in sortedRows) {
            var hierarchy = dataItem.AlignsToChapter.ChapterPathHierarchy();

            if (hierarchy.Count == 0) {
                discovered.AddIfNotExists(dataItem.AlignsToChapter);
            } else {
                foreach (var ancestor in hierarchy) {
                    discovered.AddIfNotExists(ancestor);
                }
            }
        }

        // Hierarchisch (Tiefensuche) neu ordnen. Ohne diesen Schritt
        // rutscht z.B. "Pfad1\Ohne" ans Ende der Gesamtliste, wenn die
        // zugehörige Zeile erst spät im sortierten Ergebnis auftaucht.
        var childrenByParent = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var topLevel = new List<string>();

        foreach (var cap in discovered) {
            var pos = cap.LastIndexOf(RowCaptionListItem.Kapiteltrenner);
            if (pos < 0) {
                topLevel.AddIfNotExists(cap);
            } else {
                var parent = cap[..pos];
                if (!childrenByParent.TryGetValue(parent, out var list)) {
                    list = [];
                    childrenByParent[parent] = list;
                }
                list.AddIfNotExists(cap);
            }
        }

        void AddRecursive(string cap) {
            captionOrder.AddIfNotExists(cap);

            if (!childrenByParent.TryGetValue(cap, out var children)) { return; }

            // "X\Ohne" (falls vorhanden) immer zuerst unter seinem Parent,
            // danach die übrigen Kinder in Entdeckungsreihenfolge.
            var ohneChild = cap + RowCaptionListItem.Kapiteltrenner + Ohne;
            var ordered = new List<string>();
            if (children.Contains(ohneChild, StringComparer.OrdinalIgnoreCase)) {
                ordered.Add(ohneChild);
            }
            foreach (var child in children) {
                if (!string.Equals(child, ohneChild, StringComparison.OrdinalIgnoreCase)) {
                    ordered.Add(child);
                }
            }

            foreach (var child in ordered) {
                AddRecursive(child);
            }
        }

        foreach (var top in topLevel) {
            AddRecursive(top);
        }

        return captionOrder;
    }

    private static (int BiggestItemX, int BiggestItemY, int HeightAdded, BlueBasics.Enums.Orientation Orientation) CanvasItemData(IEnumerable<RowBackground> item, Design itemDesign) {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            PreComputeSize(item, itemDesign);

            foreach (var thisItem in item) {
                if (thisItem is { Visible: true }) {
                    var s = thisItem.UntrimmedCanvasSize(itemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                }
            }

            // RowBackgroundListItem ist nie eine TextListItem -> waagerechte Orientierung.
            return (w, h, hall, BlueBasics.Enums.Orientation.Waagerecht);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return CanvasItemData(item, itemDesign);
        }
    }

    /// <summary>
    /// Ermittelt, zu welchen Überschriften eine Zeile zugeordnet werden muss.
    /// Chapters, die selbst als Parent fungieren (für die es Unterpfade gibt),
    /// werden durch "X\Ohne" ersetzt — sonst hätte eine Zeile mit Chapter
    /// "Pfad1" denselben Text wie der Parent-Header "Pfad1" und würde direkt
    /// darunter angezeigt. Stattdessen erscheint die Zeile unter
    /// "Pfad1 &gt; Ohne" in einer eigenen Sub-Section.
    /// </summary>
    private static List<string> CapsOfRow(RowItem row, bool isfiltered, bool isPinned, ColumnViewCollection arrangement, bool mustHaveACap, HashSet<string> parentCaps) {
        List<string> capsOfRow = [];

        if (isfiltered) {
            capsOfRow = arrangement.ColumnForChapter is { IsDisposed: false } sc ? row.CellGetList(sc) : [];
            capsOfRow.Remove(string.Empty);

            // Parent-Chapters zu "X\Ohne" umleiten, damit sie unter dem
            // Parent-Header in einer eigenen Ohne-Sub-Section erscheinen.
            for (var i = 0; i < capsOfRow.Count; i++) {
                var norm = capsOfRow[i].ChapterPathNormalize();
                if (parentCaps.Contains(norm)) {
                    capsOfRow[i] = norm + RowCaptionListItem.Kapiteltrenner + Ohne;
                }
            }
        }

        if (capsOfRow.Count == 0 && mustHaveACap && arrangement.ColumnForChapter is not null) {
            capsOfRow.Add(Ohne);
        }

        // Angepinnte Zeilen erscheinen doppelt: ganz oben (über den Marker
        // "Angepinnt") UND an ihrem regulären Platz. Beide Male gelb markiert.
        // Der Marker wird hier eingefügt, BEVOR Ohne/Empty-Fallbacks greifen —
        // sonst würden nicht gefilterte angepinnte Zeilen fälschlich unter
        // "Ohne" landen. Bei gefilterten Zeilen ohne Kapitel wird zusätzlich
        // die reguläre Position (leerer String) gesichert.
        if (isPinned) {
            if (capsOfRow.Count == 0 && isfiltered) { capsOfRow.Add(string.Empty); }
            capsOfRow.Add(DummyPinned);
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
    /// Zeichnet das ausgefüllte Rechteck mit halbtransparentem Rahmen
    /// als Einfüge-Indikator für Drag/Drop-Operationen.
    /// </summary>
    private static void DrawInsertIndicatorRect(Graphics gr, Rectangle rect) {
        using var brush = new SolidBrush(Color.FromArgb(40, 0, 120, 215));
        gr.FillRectangle(brush, rect);
        using var pen = new Pen(Color.FromArgb(200, 0, 120, 215), 2);
        gr.DrawRectangle(pen, rect);
    }

    private static void DrawItems(IEnumerable<RowBackground>? list, Graphics gr, Rectangle visControlArea, int offsetX, int offsetY, States controlState, Design controlDesign, Design itemDesign, float zoom, int clipTop) {
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

    private static string FormatCellForCsv(RowItem row, ColumnItem column) {
        var tmp = row.CellGetString(column);

        if (column.TextFormatingAllowed) {
            using var t = new ExtText();
            t.HtmlText = tmp;
            tmp = t.PlainText;
        }

        return tmp.Replace("\r\n", "|").Replace('\r', '|').Replace('\n', '|').Replace(";", "<sk>");
    }

    /// <summary>
    /// Holt ein Kopf-Element (<see cref="AbstractListItem"/>) aus <paramref name="allItems"/>
    /// oder legt es neu an. Alle Kopf-Elemente liegen oberhalb des Scroll-Bereichs,
    /// daher wird <see cref="AbstractListItem.IgnoreYOffset"/> hier zentral auf <c>true</c> gesetzt.
    /// </summary>
    private static T GetOrCreateHeadItem<T>(Dictionary<string, RowBackground> allItems, string identifier, ColumnViewCollection arrangement, Func<T> factory) where T : RowBackground {
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
    /// Prüft, ob ein Vorgänger-Pfad (strict ancestor) von <paramref name="chapterText"/>
    /// in <paramref name="collapsedParents"/> enthalten ist. Entspricht der bisherigen
    /// <c>StartsWith(parent + "\")</c>-Prüfung, ist aber O(Tiefe) statt O(Anzahl Parents).
    /// </summary>
    private static bool HasCollapsedAncestor(string chapterText, HashSet<string> collapsedParents) {
        var pos = chapterText.IndexOf(RowCaptionListItem.Kapiteltrenner);
        while (pos >= 0) {
            if (collapsedParents.Contains(chapterText[..pos])) { return true; }
            pos = chapterText.IndexOf(RowCaptionListItem.Kapiteltrenner, pos + 1);
        }
        return false;
    }

    /// <summary>
    /// Ein Drop-Ziel ist jede sichtbare, nicht disposed Zeile oder jeder
    /// Kapitel-Header. Letzterer wird wie eine Zeile behandelt, damit direkt
    /// unter ihm abgelegt werden kann.
    /// </summary>
    private static bool IsDroppableTarget(RowBackground item)
        => item is RowListItem or RowCaptionListItem && !item.IsDisposed && item.Visible;

    private static void NotEditableInfo(string reason) {
        if (string.IsNullOrEmpty(reason)) { return; }
        Notification.Show(LanguageTool.DoTranslate(reason), ImageCode.Kreuz);
        QuickNote.Show(NoteSymbols.Critical, "Nicht möglich");
    }

    private static void PreComputeSize(IEnumerable<RowBackground> item, Design itemDesign) {
        try {
            Parallel.ForEach(item, thisItem => thisItem?.UntrimmedCanvasSize(itemDesign));
        } catch {
            Develop.AbortAppIfStackOverflow();
            PreComputeSize(item, itemDesign);
        }
    }

    /// <summary>
    /// Liefert entweder eine Einzelelement-Liste (wenn <paramref name="row"/> gesetzt)
    /// oder eine Kopie aller <paramref name="rows"/>. Typischer Aufruf in
    /// Kontextmenü-Handlern, die sowohl mit einer einzelnen als auch mit mehreren
    /// markierten Zeilen arbeiten können.
    /// </summary>
    private static List<RowItem> RowsFromContext(RowItem? row, IReadOnlyList<RowItem> rows)
        => row is not null ? [row] : [.. rows];

    private static string UserEdited(TableView table, string newValue, ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, bool formatWarnung) {
        if (cellInThisTableColumn?.Column is not { IsDisposed: false } contentHolderCellColumn) { return "Spalte nicht vorhanden"; } // Dummy prüfung

        #region Den wahren Zellkern finden contentHolderCellColumn, contentHolderCellRow

        var contentHolderCellRow = cellInThisTableRow?.Row;
        if (contentHolderCellRow is { IsDisposed: false } && contentHolderCellColumn.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = contentHolderCellRow.LinkedCellData(contentHolderCellColumn, true, true);
            if (contentHolderCellColumn is null || contentHolderCellRow is null) { return "Spalte/Zeile nicht vorhanden"; } // Dummy prüfung
        }

        #endregion

        #region Format prüfen

        if (formatWarnung) {
            if (!newValue.IsFormat(contentHolderCellColumn, contentHolderCellColumn.ValueRequired)) {
                if (Forms.MessageBox.Show("Ihre Eingabe entspricht<br><u>nicht</u> dem erwarteten Format!<br><br>Trotzdem übernehmen?", ImageCode.Information, "Ja", "Nein") != 0) {
                    return "Abbruch, da das erwartete Format nicht eingehalten wurde.";
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
            var fe = table.IsCellEditable(cellInThisTableColumn, null, newChunkVal, true);
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

            var rd = table.GetRow(newRow, false);
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

            var check1 = table.IsCellEditable(cellInThisTableColumn, cellInThisTableRow, newChunkVal, true);
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

            if (table.Table == cellInThisTableColumn.Column?.Table) { table.CursorPos_Set(cellInThisTableColumn, cellInThisTableRow, false); }
        }

        return string.Empty;
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Table = null;

    private void _Table_SortParameterChanged(object? sender, System.EventArgs e) => Invalidate_AllViewItems(false);

    private void _Table_StoreView(object? sender, System.EventArgs e) => _storedView = ViewToJson();

    private void _Table_TableLoaded(object? sender, FirstEventArgs e) {
        if (IsDisposed) { return; }
        // Wird auch bei einem Reload ausgeführt.
        // Es kann aber sein, dass eine Ansicht zurückgeholt wurde, und die Werte stimmen.
        // Deswegen prüfen, ob wirklich alles gelöscht werden muss, oder weiter behalten werden kann.
        // Auf Nothing muss auch geprüft werden, da bei einem Dispose oder beim Beenden sich die Tabelle auch änsdert....

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
                    var clipTmp = Clipboard.GetText().RemoveChars(Char_NotFromClip).TrimEnd('\r', '\n');
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

        _autoFilter = new AutoFilter(columnviewitem.Column, FilterCombined, PinnedRows, columnviewitem.CanvasContentWidth(SheetStyle), columnviewitem.GetRenderer(SheetStyle));
        _autoFilter.Position_LocateToPosition(new Point(screenx + headX, screeny + bottom));
        _autoFilter.Show();
        _autoFilter.FilterCommand += AutoFilter_FilterCommand;
    }

    private void AutoScrollDuringColumnDrag(int controlX) {
        var area = AvailableControlPaintArea;
        var threshold = (int)(20 * Zoom);

        if (controlX < area.Left + threshold) {
            OffsetX += 20;
        } else if (controlX > area.Right - threshold) {
            OffsetX -= 20;
        }
    }

    private void AutoScrollDuringDrag(int controlY) {
        var area = AvailableControlPaintArea;
        var threshold = (int)(20 * Zoom);
        var rowsTop = RowsAreaTop();

        if (controlY < rowsTop + threshold) {
            OffsetY += 20;
        } else if (controlY > area.Bottom - threshold) {
            OffsetY -= 20;
        }
    }

    private void BB_EnterKey(object sender, System.EventArgs e) {
        if (sender is TextBox tb && tb.MultiLine) { return; }
        if (sender is TextBoxSuggestions tbs && tbs.MultiLine) { return; }
        CloseAllComponents();
    }

    private void BB_EscKey(object sender, System.EventArgs e) {
        BTB.Tag = null;
        BTB.Visible = false;
        BCB.Tag = null;
        BCB.Visible = false;
        BTS.Tag = null;
        BTS.Visible = false;
        CloseAllComponents();
    }

    private void BB_LostFocus(object sender, System.EventArgs e) {
        if (FloatingForm.IsShowing(BTB) || FloatingForm.IsShowing(BCB)) { return; }

        // Ist die Textbox noch sichtbar, wurde der Fokusverlust durch einen
        // Klick auf die Tabelle ausgelöst (nicht durch Enter/Tab/Esc — dort
        // wird Visible vorher auf false gesetzt). Den folgenden MouseDown
        // konsumieren, damit der Cursor nicht springt.
        if (sender is Control c && c.Visible) {
            _consumeNextMouseDown = true;
        }

        CloseAllComponents();
    }

    private void BB_TabKey(object sender, System.EventArgs e) => CloseAllComponents();

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        tb.Edit();
    }

    private void CalculateAllViewItems(Dictionary<string, RowBackground> allItems) {
        if (IsDisposed || Table is not { IsDisposed: false } tb
            || allItems is null
            || SortUsed() is not { } sortused
            || CurrentArrangement is not { } arrangement) {
            _rowsVisibleUnique = new([]);
            allItems?.Clear();
            return;
        }

        if (arrangement.ControlColumnsWidth() <= 0 && arrangement.Count > 0) {
            arrangement.Invalidated = true;
            arrangement.ComputeAllColumnPositions(AvailableControlPaintArea.Width, Zoom);
        }

        _newRowsAllowed = UserEdit_NewRowAllowed();

        List<RowItem> pinnedRows = [.. PinnedRows];
        // BUGFIX Performance: Bisher wurde hier sortused.SortedRows(FilterCombined.Rows) aufgerufen.
        // Das war eine verschwendete Sortierung: filteredRows wird unten NUR iterativ konsumiert
        // (AddCaptions, CalculateAllViewItems_Rows). Die echte Sortierung passiert erst weiter unten
        // über UserDefCompareKey (Zeile ~2868) und das OrderBy(item => item.CompareKey()) bei ~2663.
        // SortedRows hat zudem je Zeile einen CompareKey-String allokriert (Doppelmoral mit Zeile 2868).
        List<RowItem> filteredRows = [.. FilterCombined.Rows];

        List<RowItem> allrows = [.. pinnedRows, .. filteredRows];
        allrows = [.. allrows.Distinct()];

        var sortedItems = new List<RowBackground>();

        // Parent-Chapters berechnen: Ein Chapter ist ein Parent, wenn ein
        // anderes Chapter existiert, das als Unterpfad dient. Beispiel: Für
        // "Pfad1\Pfad2\Pfad3" sind sowohl "Pfad1" als auch "Pfad1\Pfad2"
        // Parents. Rows mit Chapter "Pfad1" oder "Pfad1\Pfad2" werden später
        // zu "X\Ohne" umgeleitet, damit sie unter dem jeweiligen Parent-Header
        // in einer eigenen Ohne-Sub-Section erscheinen.
        // Alle Vorfahren (nicht nur direkte Parents) berücksichtigen — sonst
        // fehlt bei tiefen Hierarchien die Ohne-Section auf Zwischenebenen.
        // NumberStyle (SYS_ROWSORTINDEX): der Kapitel-Trenner wird ignoriert,
        // es gibt keine Hierarchie — parentCaps bleibt leer.
        var parentCaps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (tb.Column.SysRowSortIndex is null or { IsDisposed: true }
            && arrangement.ColumnForChapter is { IsDisposed: false } parentCapCol) {
            foreach (var capValue in parentCapCol.Contents(filteredRows, Ohne)) {
                var hierarchy = capValue.ChapterPathHierarchy();
                // Alle Pfade außer dem letzten (dem Pfad selbst) sind Vorfahren.
                for (var i = 0; i < hierarchy.Count - 1; i++) {
                    parentCaps.Add(hierarchy[i]);
                }
            }
        }

        CalculateAllViewItems_AddHeadElements(allItems, arrangement, sortedItems, FilterCombined, sortused);

        CalculateAllViewItems_NewRow(allItems, arrangement, tb, _newRowsAllowed, true, sortedItems);

        var allVisibleCaps = CalculateAllViewItems_AddCaptions(allItems, arrangement, filteredRows, parentCaps);

        var visibleRowListItems = CalculateAllViewItems_Rows(allItems, arrangement, allrows, pinnedRows, allVisibleCaps, sortused, filteredRows, parentCaps);

        CalculateAllViewItems_Collapsed(allItems);

        CalculateAllViewItems_HildeAllItems(allItems, arrangement);

        var captionOrder = CalculateAllViewItems_CaptionOrder(visibleRowListItems);

        CalculateAllViewItems_AddCaptionsAndRows(allItems, sortedItems, captionOrder, visibleRowListItems);

        CalculateAllViewItems_NewRow(allItems, arrangement, tb, _newRowsAllowed, false, sortedItems);
        CalculateAllViewItems_AddFootElements(allItems, arrangement, sortedItems);

        CalculateAllViewItems_CalculateYPosition(sortedItems, arrangement);

        DoCursorPos();

        _rowsVisibleUnique = allrows;
        _sortedViewItems = sortedItems;
        _cachedRowViewItems = [.. sortedItems.OfType<RowListItem>()];
        _rowLookup.Clear();
        foreach (var rli in _cachedRowViewItems) {
            _rowLookup.TryAdd(rli.Row, rli);
        }
    }

    private void CalculateAllViewItems_AddCaptionsAndRows(Dictionary<string, RowBackground> allItems, List<RowBackground> sortedItems, List<string> captionOrder, List<RowListItem> sortedRows) {
        _chapterBlockRows.Clear();

        // NumberStyle (SYS_ROWSORTINDEX aktiv): strikte Sortierreihenfolge
        // beibehalten. Zeilen werden NICHT nach Kapiteln zusammengefasst,
        // sondern in ihrer sortierten Reihenfolge belassen. Bei jedem Wechsel
        // des Kapitels wird ein eigener flacher Header eingefügt (als Clone
        // des gecachten Canonical-Items in allItems). So kann dasselbe Kapitel
        // mehrfach auftreten, wenn seine Zeilen verstreut sind. Der Kapitel-
        // Trenner '\' wird ignoriert — Indent und Hierarchie entfallen
        // (siehe RowCaptionListItem-/RowListItem-Konstruktor).
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            // Angepinnte Zeilen ganz oben — ohne Kapitel-Header.
            // Sie werden aus dem regulären Chapter-Fluss herausgelöst.
            List<RowListItem> pinnedItems = [];
            List<RowListItem> regularItems = [];
            foreach (var rli in sortedRows) {
                if (string.Equals(rli.AlignsToChapter, DummyPinned, StringComparison.OrdinalIgnoreCase)) {
                    pinnedItems.Add(rli);
                } else {
                    regularItems.Add(rli);
                }
            }

            sortedItems.AddRange(pinnedItems);

            var previousChapter = (string?)null;
            List<RowItem>? currentBlockRows = null;
            // Collapse-Zustand wird pro Block genau einmal (beim Kapitelwechsel
            // anhand der ersten Zeile des Blocks) ermittelt und für alle Zeilen
            // dieses Blocks beibehalten. _collapsedBlockFirstRowKeys enthält nur
            // den Key der ersten Zeile je Block — eine prüfungsweise Abfrage für
            // jede Zeile würde daher nur die erste Zeile ausblenden (Bug).
            var blockCollapsed = false;

            foreach (var rli in regularItems) {
                var chapter = rli.AlignsToChapter;

                // Header nur bei definiertem Kapitel und nur bei tatsächlichem
                // Wechsel einfügen. Bei fehlender Chapter-Spalte ist chapter leer.
                if (!string.IsNullOrEmpty(chapter)
                    && !string.Equals(chapter, previousChapter, StringComparison.OrdinalIgnoreCase)) {
                    // Neuer Block: Collapse-Zustand anhand seiner ersten Zeile
                    // bestimmen. rli ist genau diese erste Zeile, weil der
                    // Header nur beim erstmaligen Auftreten des Kapitels
                    // eingefügt wird.
                    blockCollapsed = rli.Row is { IsDisposed: false } firstRow
                                     && _collapsedBlockFirstRowKeys.Contains(firstRow.KeyName);

                    // Original-Schreibweise aus dem gecachten Canonical-Item
                    // holen, da AlignsToChapter upper-cased ist und die
                    // sichtbaren Header Clone sind. IsExpanded pro Block aus
                    // dem zustandslosen Cache bestimmen — so bleibt jeder Block
                    // mit gleichem Chapter-Text unabhängig auf/zu klappbar.
                    RowCaptionListItem headerItem;
                    if (allItems.TryGetValue(RowCaptionListItem.Identifier(chapter), out var capItem) && capItem is RowCaptionListItem rcli) {
                        headerItem = new RowCaptionListItem(rcli.ChapterText, rli.Arrangement) { IsExpanded = !blockCollapsed };
                    } else {
                        headerItem = new RowCaptionListItem(chapter, rli.Arrangement) { IsExpanded = !blockCollapsed };
                    }

                    sortedItems.Add(headerItem);
                    previousChapter = chapter;
                    currentBlockRows = [];
                    _chapterBlockRows[headerItem] = currentBlockRows;
                }

                // Zeile ihrem Block zuordnen — unabhängig vom Aufklapp-Zustand.
                // Das Mapping ist die Grundlage, damit ein eingeklappter Block
                // als Ganzes verschoben werden kann (Drag/Drop ohne Aufklappen).
                if (currentBlockRows is not null
                    && !string.IsNullOrEmpty(chapter)
                    && rli.Row is { IsDisposed: false } blockRow) {
                    currentBlockRows.Add(blockRow);
                }

                // Zeile nur anzeigen, wenn der Block nicht eingeklappt ist.
                // Zeilen ohne Kapitel gehören zu keinem Block und werden immer
                // angezeigt (blockCollapsed wäre ein vererbter Fremdwert).
                if (string.IsNullOrEmpty(chapter) || !blockCollapsed) {
                    sortedItems.Add(rli);
                }
            }

            return;
        }

        var grouped = sortedRows.ToLookup(x => x.AlignsToChapter);

        // _collapsed enthält nur die direkt eingeklappten Kapitel. Um auch
        // Nachfahren zu verbergen, zusätzlich auf eingeklappte Vorfahren prüfen.
        var collapsedSet = new HashSet<string>(_collapsed, StringComparer.OrdinalIgnoreCase);

        foreach (var captionKey in captionOrder) {
            // Vorfahr eingeklappt? → Header und Zeilen komplett überspringen.
            // Der Parent-Header selbst wird weiterhin angezeigt (eingeklappt).
            if (HasCollapsedAncestor(captionKey, collapsedSet)) { continue; }

            allItems.TryGetValue(RowCaptionListItem.Identifier(captionKey), out var caption);
            if (caption is RowCaptionListItem capRcli) {
                sortedItems.Add(capRcli);
                _chapterBlockRows[capRcli] = [.. grouped[captionKey].Where(x => x.Row is { IsDisposed: false }).Select(x => x.Row)];
            } else if (caption is not null) {
                sortedItems.Add(caption);
            }

            // Direkt eingeklappt? → Header ja, aber keine Zeilen darunter.
            if (!collapsedSet.Contains(captionKey)) {
                sortedItems.AddRange(grouped[captionKey]);
            }
        }
    }

    private void CalculateAllViewItems_AddFootElements(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement, List<RowBackground> sortedItems) {
        allItems.TryGetValue(TableEndListItem.Identifier, out var teli);
        if (teli is not TableEndListItem tableEnd) {
            tableEnd = new TableEndListItem(arrangement);
            allItems.Add(tableEnd.KeyName, tableEnd);
        }
        tableEnd.Visible = arrangement.ShowHead;
        tableEnd.IgnoreYOffset = false;
        sortedItems.Add(tableEnd);
    }

    private void CalculateAllViewItems_AddHeadElements(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement, List<RowBackground> sortedItems, FilterCollection filterCombined, RowSortDefinition sortused) {
        if (!arrangement.ShowHead) { return; }

        // Spaltenbuchstaben-Leiste (A, B, C, ...) ganz oben.
        // Bei ColumnHeaderMode.Ohne wird die Leiste gar nicht erst zu sortedItems hinzugefügt,
        // da CalculateAllViewItems_CalculateYPosition die Sichtbarkeit aller Items pauschal auf true setzt.
        if (arrangement.ColumnHeaderMode != ColumnHeaderMode.Ohne) {
            var columnHeaderBar = GetOrCreateHeadItem(allItems, ColumnHeaderBarListItem.Identifier, arrangement, () => new ColumnHeaderBarListItem(arrangement));
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
                var captionBar = GetOrCreateHeadItem(allItems, CaptionBarListItem.Identifier(z), arrangement, () => new CaptionBarListItem(arrangement, z));
                captionBar.Visible = arrangement.ShowHead;
                sortedItems.Add(captionBar);
            }
        }

        // Grüner Expand Button
        var collapseBar = GetOrCreateHeadItem(allItems, CollapesBarListItem.Identifier, arrangement, () => new CollapesBarListItem(arrangement));
        collapseBar.Visible = arrangement.ShowHead;
        sortedItems.Add(collapseBar);

        // Spaltenköpfe direkt
        var columnHead = GetOrCreateHeadItem(allItems, ColumnsHeadListItem.Identifier, arrangement, () => new ColumnsHeadListItem(arrangement));
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
        var sortAnzeige = GetOrCreateHeadItem(allItems, SortBarListItem.Identifier, arrangement, () => new SortBarListItem(arrangement));
        sortAnzeige.Visible = arrangement.ShowHead;
        sortAnzeige.FilterCombined = filterCombined;
        sortAnzeige.Sort = sortused;
        sortedItems.Add(sortAnzeige);

        // Filterleiste
        var columnFilter = GetOrCreateHeadItem(allItems, FilterBarListItem.Identifier, arrangement, () => new FilterBarListItem(arrangement));
        columnFilter.Visible = arrangement.ShowHead;
        columnFilter.FilterCombined = filterCombined;
        columnFilter.RowsFilteredCount = filterCombined.Rows.Count;
        sortedItems.Add(columnFilter);

        // Ansichtbearbeitung-Leiste
        if (Ansichtbearbeitung) {
            var editBar = GetOrCreateHeadItem(allItems, EditBarListItem.Identifier, arrangement, () => new EditBarListItem(arrangement));
            editBar.Visible = true;
            sortedItems.Add(editBar);
        }
    }

    private void CalculateAllViewItems_CalculateYPosition(List<RowBackground> sortedItems, ColumnViewCollection arrangement) {
        var columnsWidth = (int)arrangement.ControlColumnsWidth().ControlToCanvas(Zoom);

        var y = 0;

        foreach (var thisItem in sortedItems) {
            thisItem.Visible = true;
            // Indent zur Spaltenbreite addieren: Beim Zeichnen werden die Spalten
            // um IndentWidth * Indent nach rechts verschoben. Die CanvasPosition-
            // Breite muss das abdecken, sonst wird die Zeile zu früh abgeschnitten.
            var wi = columnsWidth + RowBackground.IndentWidth * thisItem.Indent;
            thisItem.CanvasPosition = new Rectangle(0, y, wi, thisItem.HeightInControl(ListBoxAppearance.Listbox, columnsWidth, Design.Item_ListBox));
            y = thisItem.CanvasPosition.Bottom;
        }
    }

    /// <summary>
    /// Berechnet die Variable _collapsed. Enthält NUR die direkt
    /// eingeklappten Kapitel (als Großschreibung). Nachfahren von
    /// eingeklappten Kapiteln werden in CalculateAllViewItems_AddCaptionsAndRows
    /// via HasCollapsedAncestor behandelt — dort wird entschieden, ob ein
    /// Sub-Header bzw. dessen Zeilen angezeigt werden.
    /// </summary>
    private void CalculateAllViewItems_Collapsed(Dictionary<string, RowBackground> allItems) {
        _collapsed.Clear();

        foreach (var thisR in allItems.Values) {
            if (thisR is RowCaptionListItem { IsDisposed: false, IsExpanded: false } rcli) {
                _collapsed.Add(rcli.ChapterText.ToUpperInvariant());
            }
        }
    }

    private void CalculateAllViewItems_HildeAllItems(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement) {
        var columnsWidth = (int)arrangement.ControlColumnsWidth().ControlToCanvas(Zoom);

        foreach (var thisItem in allItems.Values) {
            thisItem?.Visible = false;

            if (thisItem is RowBackground rbli) {
                rbli.Arrangement = arrangement;
                var wi = columnsWidth + RowBackground.IndentWidth * rbli.Indent;
                rbli.CanvasPosition = rbli.CanvasPosition with { Width = wi };
            }
        }
    }

    private void CalculateAllViewItems_NewRow(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement, Table tb, string newRowsAllowed, bool headPosition, List<RowBackground> sortedItems) {
        if (!string.IsNullOrEmpty(newRowsAllowed)) { return; }

        if (tb.Column.SysRowSortIndex is { IsDisposed: false } == headPosition) { return; }

        allItems.TryGetValue(NewRowListItem.Identifier, out var nri);
        if (nri is not NewRowListItem newRow) {
            newRow = new NewRowListItem(arrangement);
            allItems.Add(newRow.KeyName, newRow);
        }
        newRow.IgnoreYOffset = headPosition;
        newRow.Visible = true;
        newRow.FilterCombined = FilterCombined;

        sortedItems.Add(newRow);
    }

    private List<RowListItem> CalculateAllViewItems_Rows(Dictionary<string, RowBackground> allItems, ColumnViewCollection arrangement, List<RowItem> allrows, List<RowItem> pinnedRows, HashSet<string> allVisibleCaps, RowSortDefinition sortused, List<RowItem> filteredRows, HashSet<string> parentCaps) {
        var hasCaps = allVisibleCaps.Count > 0;
        var visibleRowListItems = new List<RowListItem>(allrows.Count);
        var pinnedSet = new HashSet<RowItem>(pinnedRows);
        var filteredSet = new HashSet<RowItem>(filteredRows);

        foreach (var thisRow in allrows) {
            var isPinned = pinnedSet.Contains(thisRow);
            var isFiltered = filteredSet.Contains(thisRow);

            foreach (var thisCap in CapsOfRow(thisRow, isFiltered, isPinned, arrangement, hasCaps, parentCaps)) {
                var id = RowListItem.Identifier(thisRow, thisCap);

                if (!allItems.TryGetValue(id, out var it2) || it2 is not RowListItem rowListItem) {
                    rowListItem = new RowListItem(thisRow, thisCap, arrangement);
                    allItems.Add(rowListItem.KeyName, rowListItem);
                }
                rowListItem.Arrangement = arrangement;

                rowListItem.UserDefCompareKey = rowListItem.Row.CompareKey(sortused.UsedColumns);
                rowListItem.Visible = false;
                rowListItem.MarkYellow = isPinned;
                visibleRowListItems.Add(rowListItem);
            }
        }

        return sortused.Reverse
              ? visibleRowListItems.OrderByDescending(item => item.CompareKey()).ToList()
              : visibleRowListItems.OrderBy(item => item.CompareKey()).ToList();
    }

    /// <summary>
    /// Berechnet den Einfüge-Index (Index in CurrentArrangement) für das Spalten-Drag/Drop,
    /// basierend auf der Maus-X-Position.
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
    /// Berechnet den Einfüge-Index für den Drag/Drop der SYS_ROWSORTINDEX-Spalte,
    /// basierend auf der Maus-Y-Position. Der Index bezieht sich auf
    /// <see cref="_sortedViewItems"/> (Header + Zeilen) — NICHT auf
    /// <see cref="_cachedRowViewItems"/>. Ein <see cref="RowCaptionListItem"/>
    /// wird dabei wie ein <see cref="RowListItem"/> als Drop-Ziel behandelt,
    /// damit eine Zeile direkt unter einem Kapitel-Header (als erste Zeile des
    /// Blocks) abgelegt werden kann. Wird jedoch ein gesamter Kapitel-Block
    /// (<see cref="RowCaptionListItem"/>) verschoben, wird die Position über
    /// <see cref="EnsureValidChapterInsertIndex"/> so korrigiert, dass der
    /// Vorgänger weder ein anders lautender Kapitel-Header noch ein anderer
    /// Spezial-Eintrag ist. Rückgabe -1 bedeutet: kein gültiges Ziel.
    /// </summary>
    private int CalculateRowSortInsertIndex(int controlY) {
        if (_sortedViewItems is not { Count: > 0 }) { return -1; }

        var draggedChapter = _dragItem as RowCaptionListItem;

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
        // Skripte können auf Hintergrund-Threads laufen und WriteBackVariables
        // feuert CellValueChanged synchron auf dem aufrufenden Thread. Ohne
        // Marshalling greifen UI- und Skript-Thread gleichzeitig auf
        // _allViewItems zu → "Collection was modified". BeginInvoke blockiert
        // den Skript-Thread nicht (vermeidet Deadlocks bei mehrfachem WriteBack).
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

    private void Cell_Edit(ColumnViewItem? viewItem, RowBackground? rowItem, bool preverDropDown, string? chunkval) {
        var f = IsCellEditable(viewItem, rowItem as RowListItem, chunkval, true);
        if (!string.IsNullOrEmpty(f)) { NotEditableInfo(f); return; }

        if (viewItem?.Column is not { IsDisposed: false } contentHolderCellColumn) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var contentHolderCellRow = (rowItem as RowListItem)?.Row;

        if (contentHolderCellRow is { IsDisposed: false } && viewItem.Column.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = contentHolderCellRow.LinkedCellData(contentHolderCellColumn, true, true);
        }

        if (contentHolderCellColumn is not { IsDisposed: false }) {
            NotEditableInfo("Keine Spalte angeklickt.");
            return;
        }

        var dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, preverDropDown);

        if (dia == EditTypeTable.None && (contentHolderCellColumn.Table?.PowerEdit ?? false)) {
            dia = ColumnItem.UserEditDialogTypeInTable(contentHolderCellColumn, false, true);
        }

        switch (dia) {
            case EditTypeTable.Textfeld:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_TextBox(viewItem, rowItem, BTB, 0);
                break;

            case EditTypeTable.Textfeld_mit_Auswahlknopf:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_TextBox(viewItem, rowItem, BCB, 20);
                break;

            case EditTypeTable.Dropdown_Single:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_Dropdown(viewItem, rowItem, contentHolderCellColumn, contentHolderCellRow);
                break;

            case EditTypeTable.Textfeld_mit_Vorschlägen:
                contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);
                Cell_Edit_TextboxWithSuggestions(viewItem, rowItem, 0);
                break;

            case EditTypeTable.None:
                break;

            case EditTypeTable.DragDrop:
                NotEditableInfo("Werte ändern sich automatisch durch\r\nVerschieben der Zeilen.");
                break;

            default:
                Develop.DebugPrint(dia);
                NotEditableInfo("Unbekannte Bearbeitungs-Methode");
                break;
        }
    }

    private void Cell_Edit_Dropdown(ColumnViewItem viewItem, RowBackground? cellInThisTableRow, ColumnItem contentHolderCellColumn, RowItem? contentHolderCellRow) {
        if (viewItem.Column != contentHolderCellColumn) {
            if (contentHolderCellRow is null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
            if (cellInThisTableRow is null) {
                NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return;
            }
        }

        if (cellInThisTableRow is not RowListItem rli) { return; }

        var t = new List<AbstractListItem>();

        var r = viewItem.GetRenderer(SheetStyle);
        var cell = new CellExtEventArgs(viewItem, cellInThisTableRow as RowListItem);

        t.AddRange(ItemsOf(contentHolderCellColumn, contentHolderCellRow, 1000, r));
        if (t.Count == 0) {
            // Hm ... Dropdown kein Wert vorhanden.... also gar kein Dropdown öffnen!
            if (contentHolderCellColumn.EditableWithTextInput) { Cell_Edit(viewItem, cellInThisTableRow, false, rli.Row.ChunkValue ?? FilterCombined.ChunkVal); } else {
                NotEditableInfo("Keine Items zum Auswählen vorhanden.");
            }
            return;
        }

        if (contentHolderCellColumn.EditableWithTextInput) {
            if (t.Count == 0 && string.IsNullOrWhiteSpace(rli.Row.CellGetString(viewItem.Column))) {
                // Bei nur einem Wert, wenn Texteingabe erlaubt, Dropdown öffnen
                Cell_Edit(viewItem, cellInThisTableRow, false, rli.Row.ChunkValue ?? FilterCombined.ChunkVal);
                return;
            }
            var erw = ItemOf("Erweiterte Eingabe", "#Erweitert", QuickImage.Get(ImageCode.Stift), true, FirstSortChar + "1");

            t.Add(erw);
            t.Add(SeparatorWith(FirstSortChar + "2"));
        }

        List<string> toc = [];

        if (contentHolderCellRow is not null) {
            toc.AddRange(contentHolderCellRow.CellGetList(contentHolderCellColumn));
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(t, CheckBehavior.MultiSelection, toc, this, Translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
        dropDownMenu.ItemClicked += (sender, e) => DropDownMenu_ItemClicked(e, cell);
        Develop.Debugprint_BackgroundThread();
    }

    private bool Cell_Edit_TextBox(ColumnViewItem viewItem, RowBackground? cellInThisTableRow, TextBox box, int addWith) {
        if (IsDisposed || viewItem.Column is null) { return false; }

        //if (contentHolderCellColumn != viewItem.Column) {
        //    if (contentHolderCellRow is null) {
        //        NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
        //        return false;
        //    }
        //    if (cellInThisTableRow is null) {
        //        NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
        //        return false;
        //    }
        //}

        box.GetStyleFrom(viewItem.Column);
        box.QuickInfo = viewItem.Column.QuickInfo;

        var (controlPos, contentHolderCellRow, cellText) = GetEditBounds(viewItem, cellInThisTableRow);

        box.Location = new Point(viewItem.ControlColumnLeft(OffsetX), controlPos.Y);
        box.Size = new Size(viewItem.ControlColumnWidth() + addWith, controlPos.Height);
        box.Text = cellText;
        box.Tag = (List<object?>)[viewItem, cellInThisTableRow];

        if (box is ComboBox cbox) {
            cbox.ItemClear();
            cbox.ItemAddRange(ItemsOf(viewItem.Column, contentHolderCellRow, 1000, viewItem.GetRenderer(SheetStyle)));
            if (cbox.ItemCount == 0) {
                return Cell_Edit_TextBox(viewItem, cellInThisTableRow, BTB, 0);
            }
        }

        box.Verhalten = controlPos.Height > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;

        box.Visible = true;
        box.BringToFront();
        box.Focus();
        return true;
    }

    private void Cell_Edit_TextboxWithSuggestions(ColumnViewItem viewItem, RowBackground? cellInThisTableRow, int addWith) {
        if (IsDisposed || viewItem.Column is null) { return; }

        var items = ItemsOf(viewItem.Column, null, 1000, viewItem.GetRenderer(SheetStyle));

        if (items.Count is 0 or > 30) {
            Cell_Edit_TextBox(viewItem, cellInThisTableRow, BTB, 0);
            return;
        }

        BTS.GetStyleFrom(viewItem.Column);
        BTS.QuickInfo = viewItem.Column.QuickInfo;
        BTS.Suggestions = items.Select(s => s.KeyName).ToList().AsReadOnly();

        var (controlPos, _, cellText) = GetEditBounds(viewItem, cellInThisTableRow);
        var totalWidth = viewItem.ControlColumnWidth() + addWith;

        BTS.TextboxSize = new Size(totalWidth, controlPos.Height);
        BTS.Location = new Point(viewItem.ControlColumnLeft(OffsetX), controlPos.Y);
        BTS.Size = new Size(totalWidth, BTS.GetEstimatedHeight(totalWidth, controlPos.Height));
        BTS.Text = cellText;
        BTS.Verhalten = controlPos.Height > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;

        BTS.Tag = (List<object?>)[viewItem, cellInThisTableRow];
        BTS.Visible = true;
        BTS.BringToFront();
        BTS.Focus();
    }

    private (ColumnViewItem?, RowBackground?) CellOnCoordinate(ColumnViewCollection? ca, CanvasMouseEventArgs e) => (ColumnOnCoordinate(ca, e), RowItemAtPosition(e.ControlY));

    private void CloseAllComponents() {
        if (InvokeRequired) {
            Invoke(new Action(CloseAllComponents));
            return;
        }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        TXTBox_Close(BTB);
        TXTBox_Close(BCB);
        TXTBox_Close(BTS);
        FloatingForm.Close(this);
        AutoFilter_Close();
        Forms.QuickInfo.Close();
    }

    private void CollapseThis(string[] t) {
        if (AllViewItems is not { } avi) { return; }
        var did = false;
        var tSet = new HashSet<string>(t);

        foreach (var thisItem in avi.Values) {
            if (thisItem is RowCaptionListItem { IsDisposed: false } rcli) {
                if (rcli.IsExpanded == tSet.Contains(rcli.ChapterText)) {
                    rcli.IsExpanded = !rcli.IsExpanded;
                    did = true;
                }
            }
        }

        if (did) { Invalidate_AllViewItems(false); }
    }

    private void Column_ItemRemoving(object? sender, ColumnEventArgs e) {
        if (e.Column == CursorPosColumn?.Column) { CursorPos_Reset(); }
    }

    private ColumnViewItem? ColumnOnCoordinate(ColumnViewCollection? ca, CanvasMouseEventArgs? e) {
        if (ca is not { IsDisposed: false } || e is null) { return null; }

        foreach (var thisViewItem in ca.RenderingItems) {
            if (e.ControlX >= thisViewItem.ControlColumnLeft(OffsetX) && e.ControlX <= thisViewItem.ControlColumnRight(OffsetX)) { return thisViewItem; }
        }

        return null;
    }

    private void ContextMenu_ContentCopy(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, _, _) = GetContextData(e.HotItem);
        var rli = GetRow(row, false);
        var cp = rli?.ControlPosition(Zoom, OffsetX, OffsetY) ?? Rectangle.Empty;
        var vi = CurrentArrangement?[column];
        CopyToClipboard(column, row, true, PointToScreen(new Point(vi?.ControlColumnRight(OffsetX) ?? 0, cp.Y)));
    }

    private void ContextMenu_ContentDelete(object? sender, ContextMenuEventArgs e) {
        var (column, row, _, _, _) = GetContextData(e.HotItem);

        if (TableViewForm.EditableErrorMessage(row?.Table, row)) { return; }
        row?.CellSet(column, string.Empty, "Inhalt Löschen Kontextmenü");
    }

    private void ContextMenu_ContentPaste(object? sender, ContextMenuEventArgs e) => PasteToCursor();

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

        if (TableViewForm.EditableErrorMessage(tb, null)) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        var currentArr = tcvc.GetByKey(ca.KeyName);
        if (currentArr is null) { return; }

        var virtualToRemove = currentArr.FirstOrDefault(x => x.StorageKey == viewItem?.StorageKey);
        currentArr.Remove(virtualToRemove);

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
        ColumnsHeadListItem.ShowDummyColumnDropDown(ca, this, column);
    }

    private void ContextMenu_Pin(object? sender, ContextMenuEventArgs e) {
        var (_, row, _, _, _) = GetContextData(e.HotItem);

        PinAdd(row);
    }

    private void ContextMenu_ResetSort(object? sender, ContextMenuEventArgs e) {
        SortDefinitionTemporary = null;
    }

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

        if (!sourceCvi.IsDummyColumn) {
            var hasPermanentAfter = false;
            for (var i = newIndex + 1; i < ca.Count; i++) {
                if (ca[i] is { Permanent: true } c && !c.IsDummyColumn) {
                    hasPermanentAfter = true;
                    break;
                }
            }

            if (hasPermanentAfter) {
                sourceCvi.Permanent = true;
            } else if (sourceCvi.Permanent) {
                var hasNonPermanentBefore = false;
                for (var i = 0; i < newIndex; i++) {
                    if (ca[i] is { Permanent: false } c && !c.IsDummyColumn) {
                        hasNonPermanentBefore = true;
                        break;
                    }
                }
                if (hasNonPermanentBefore) {
                    sourceCvi.Permanent = false;
                }
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
        foreach (var rdli in _cachedRowViewItems) {
            rdli.Column = CursorPosRow == rdli ? CursorPosColumn?.Column : null;
        }
    }

    /// <summary>
    /// Führt den Benutzerfilter (Filter) und die Fixfilter (FilterFix) zusammen
    /// und schreibt das Ergebnis in FilterCombined.
    /// Wird bei jeder Änderung von Filter oder FilterFix aufgerufen.
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
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return; }
        if (sourceRows.Count == 0) { return; }

        // Alle CellSets in dieser Methode gebündelt feuern (Kapitel-Update +
        // Neu-Nummerierung). Ohne Suppression löst JEDER CellSet synchron
        // Cell_CellValueChanged aus — mit CurrentArrangement-Zugriff,
        // ComputeAllColumnPositions, Invalidate_AllViewItems usw. Bei N Zeilen
        // entstehen N teure Voll-Layouts hintereinander. ResumeEvents feuert
        // einmalig ViewChanged → ein einziger Aufbau am Ende.
        tb.SuppressEvents();
        try {
            // Alle Zeilen in der aktuellen Sortierung sammeln
            var sortedRows = SortUsed()?.SortedRows(tb.Row) ?? [.. tb.Row];
            if (sortedRows.Count == 0) { return; }

            // Quell-Zeilen aus der Liste entfernen
            foreach (var sr in sourceRows) {
                if (sr is { IsDisposed: false }) { sortedRows.Remove(sr); }
            }

            // Einfüge-Position in der sortierten Liste bestimmen.
            // insertIndex ist ein Index in _sortedViewItems (Header + Zeilen).
            // Die Ziel-Position ist "vor" der ersten Zeile (RowListItem) ab
            // insertIndex — ein Header als Drop-Ziel bedeutet "erste Zeile unter
            // diesem Kapitel". Bei eingeklappten Kapiteln sind die Zeilen nicht
            // in _sortedViewItems; dann wird die erste Zeile des Blocks aus
            // _chapterBlockRows verwendet.
            RowItem? targetRow = null;
            for (var i = Math.Max(0, insertIndex); i < _sortedViewItems.Count; i++) {
                if (_sortedViewItems[i] is RowListItem tRli && tRli.Row is { IsDisposed: false }) {
                    targetRow = tRli.Row;
                    break;
                }
                if (_sortedViewItems[i] is RowCaptionListItem capRcli
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
                RowListItem? singleRli = null;
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
            var nr = 1;
            foreach (var thisRow in sortedRows) {
                if (thisRow is { IsDisposed: false }) {
                    thisRow.CellSet(sortCol, nr, "Drag/Drop Sortierung");
                    nr++;
                }
            }
        } finally {
            tb.ResumeEvents();
        }
    }

    /// <summary>
    /// Zeichnet den Einfüge-Indikator für das Spalten-Drag/Drop.
    /// Der Indikator ist 16 Pixel breit (jeweils 8 Pixel in die linke und rechte Spalte),
    /// um die Einfüge-Position zwischen zwei Spalten zu markieren.
    /// Entspricht die Einfüge-Position der aktuellen Position, wird die eigene Spalte markiert.
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
    /// Zeichnet den Einfüge-Indikator für das Zeilen-Drag/Drop.
    /// Der Indikator ist 16 Pixel hoch (jeweils 8 Pixel in die obere und untere Zeile),
    /// um die Einfüge-Position zwischen zwei Zeilen zu markieren.
    /// Entspricht die Einfüge-Position der aktuellen Position, wird die eigene Zeile markiert.
    /// </summary>
    private void DrawRowSortInsertIndicator(Graphics gr, ColumnViewCollection ca) {
        if (_sortedViewItems is not { Count: > 0 }) { return; }
        if (_dragInsertIndex < 0 || _dragInsertIndex > _sortedViewItems.Count) { return; }

        var columnsLeft = 0;
        var columnsRight = (int)ca.ControlColumnsWidth() + columnsLeft;
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
    /// Berechent die Y-CanvasPosition auf dem aktuellen Controll
    /// </summary>
    /// <returns></returns>
    private void DropDownMenu_ItemClicked(AbstractListItemEventArgs e, CellExtEventArgs ck) {
        FloatingForm.Close(this);

        if (CurrentArrangement is not { IsDisposed: false }) { return; }

        if (ck?.ColumnView?.Column is not { IsDisposed: false } c) { return; }

        var toAdd = e.Item.KeyName;
        var toRemove = string.Empty;
        if (toAdd == "#Erweitert") {
            Cell_Edit(ck.ColumnView, ck.RowData, false, ck.RowData?.Row.ChunkValue ?? FilterCombined.ChunkVal);
            return;
        }
        if (ck.RowData?.Row is not { IsDisposed: false } r) {
            // Neue Zeile!
            NotEditableInfo(UserEdited(this, toAdd, ck.ColumnView, null, false));
            return;
        }

        if (c.MultiLine) {
            var li = r.CellGetList(c);
            if (li.Contains(toAdd, StringComparer.OrdinalIgnoreCase)) {
                // Ist das angeklickte Element schon vorhanden, dann soll es wohl abgewählt (gelöscht) werden.
                if (li.Count > 1 || !c.ValueRequired) {
                    toRemove = toAdd;
                    toAdd = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(toRemove)) { li.RemoveString(toRemove, false); }
            if (!string.IsNullOrEmpty(toAdd)) { li.Add(toAdd); }
            NotEditableInfo(UserEdited(this, string.Join('\r', li), ck.ColumnView, ck.RowData, false));
        } else {
            if (!c.ValueRequired) {
                if (toAdd == ck.RowData.Row.CellGetString(c)) {
                    NotEditableInfo(UserEdited(this, string.Empty, ck.ColumnView, ck.RowData, false));
                    return;
                }
            }
            NotEditableInfo(UserEdited(this, toAdd, ck.ColumnView, ck.RowData, false));
        }
    }

    /// <summary>
    /// Korrigiert den Einfüge-Index beim Verschieben eines Kapitel-Blocks
    /// (<see cref="RowCaptionListItem"/>). Der direkte Vorgänger der Zielposition
    /// muss entweder eine feste Y-Position haben
    /// (<see cref="RowBackground.IgnoreYOffset"/> — z. B. Spaltenkopf,
    /// Neue-Zeile-Eintrag), ein <see cref="RowListItem"/> oder ein
    /// <see cref="RowCaptionListItem"/> mit derselben Überschrift wie
    /// <paramref name="draggedChapter"/> sein. Ein Kapitel-Header mit anderer
    /// Überschrift als direkter Vorgänger ist verboten — der einzufügende
    /// Kapitel-Header würde sonst einen leeren Block direkt unter einem anderen
    /// Header erzeugen. Gleichnamige Kapitel dürfen jedoch direkt aufeinander
    /// folgen (sie verschmelzen zu einem gemeinsamen Block). Ausnahme: ist der
    /// Vorgänger eingeklappt, sind seine Zeilen zwar ausgeblendet, aber
    /// vorhanden — ein direktes Aufeinandertreffen der Header ist dann
    /// zulässig. Die Position wird dafür solange nach oben verschoben, bis ein
    /// gültiger Vorgänger gefunden ist (bzw. der Listenanfang erreicht ist).
    /// Ist <paramref name="draggedChapter"/> null (kein Kapitel-Block wird
    /// verschoben), wird <paramref name="index"/> unverändert zurückgegeben.
    /// </summary>
    private int EnsureValidChapterInsertIndex(int index, RowCaptionListItem? draggedChapter) {
        if (draggedChapter is null) { return index; }

        while (index > 0) {
            var predecessor = _sortedViewItems[index - 1];
            if (predecessor.IgnoreYOffset || predecessor is RowListItem) { break; }
            if (predecessor is RowCaptionListItem predChapter) {
                if (string.Equals(predChapter.ChapterText, draggedChapter.ChapterText, StringComparison.OrdinalIgnoreCase)) { break; }
                // Eingeklappter Vorgänger: Seine Zeilen sind vorhanden, nur
                // ausgeblendet. Ein direktes Aufeinandertreffen der Header
                // ist daher zulässig.
                if (IsChapterCollapsed(predChapter)) { break; }
            }
            index--;
        }
        return index;
    }

    private bool EnsureVisible(RowBackground? rowdata) {
        if (rowdata is not RowListItem rli) { return false; }

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

        //if (viewItem.ViewType == ViewType.PermanentColumn) {
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
    /// Filtert die übergebenen Zeilen auf solche, die per Drag/Drop verschoben
    /// werden dürfen: nicht disposed, nicht angepinnt und editierbar.
    /// </summary>
    private List<RowItem> FilterDraggableRows(IEnumerable<RowItem> rows) {
        var result = new List<RowItem>();
        if (Table is not { IsDisposed: false } tb) { return result; }
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return result; }

        foreach (var br in rows) {
            if (br is { IsDisposed: false }
                && !PinnedRows.Contains(br)
                && string.IsNullOrEmpty(CellCollection.IsCellEditable(sortCol, br, br.ChunkValue))) {
                result.Add(br);
            }
        }

        return result;
    }

    private void FilterFix_PropertyChanged(object? sender, PropertyChangedEventArgs e) => DoFilterCombined();

    /// <summary>
    /// Durchläuft <see cref="_sortedViewItems"/> ab <paramref name="startIndex"/> mit der
    /// Schrittweite <paramref name="step"/> und liefert das erste sichtbare <see cref="RowListItem"/>.
    /// Aufrufer müssen zuvor <c>_ = AllViewItems</c> ausgeführt haben, damit die Liste befüllt ist.
    /// </summary>
    private RowListItem? FindVisibleRowListItem(int startIndex, int step) {
        for (var i = startIndex; i >= 0 && i < _sortedViewItems.Count; i += step) {
            if (_sortedViewItems[i] is RowListItem rli && rli.Visible) { return rli; }
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
            } else if (item is RowItem or RowCaptionListItem) {
                var srcRows = GetDragSourceRows(item);
                if (srcRows.Count > 0) {
                    // No-Op: Wird auf den eigenen Block zurückgezogen, nichts verschieben.
                    // Gleiche Bedingung wie in DrawRowSortInsertIndicator.
                    var (firstSrc, lastSrc) = SourceIndexRange(srcRows, item);
                    if (firstSrc < 0 || insertIndex < firstSrc || insertIndex > lastSrc + 1) {
                        // Bei Kapitel-Block werden KEINE Überschriften geändert.
                        DoRowSortReorder(srcRows, insertIndex, item is RowCaptionListItem, mouseControlY);
                    }
                }
            }
        }

        Invalidate();
    }

    /// <summary>
    /// Erstes Drop-Ziel (Zeile oder Kapitel-Header) in <see cref="_sortedViewItems"/>
    /// ab <paramref name="fromIndex"/> oder null.
    /// </summary>
    private RowBackground? FirstDroppableViewItem(int fromIndex) {
        for (var i = Math.Max(0, fromIndex); i < _sortedViewItems.Count; i++) {
            if (IsDroppableTarget(_sortedViewItems[i])) { return _sortedViewItems[i]; }
        }
        return null;
    }

    /// <summary>
    /// Liefert die verschiebbaren Quell-Zeilen für das übergebene Drag-Objekt.
    /// <paramref name="item"/> ist entweder ein <see cref="RowItem"/> (Einzelzeile)
    /// oder ein <see cref="RowCaptionListItem"/> (gesamter Kapitel-Block).
    /// </summary>
    private List<RowItem> GetDragSourceRows(object? item) {
        switch (item) {
            case RowItem r:
                return r.IsDisposed ? [] : FilterDraggableRows([r]);

            case RowCaptionListItem rcli:
                return GetChapterBlockRows(rcli) is { } blockRows ? FilterDraggableRows(blockRows) : [];

            default:
                return [];
        }
    }

    /// <summary>
    /// Berechnet Position, ggf. Inhalt-Zeile und Zelltext für ein Edit-Control.
    /// </summary>
    private (Rectangle controlPos, RowItem? contentRow, string cellText) GetEditBounds(ColumnViewItem viewItem, RowBackground? cellInThisTableRow) {
        if (cellInThisTableRow is null) { return (Rectangle.Empty, null, string.Empty); }

        var controlPos = cellInThisTableRow.ControlPosition(Zoom, OffsetX, OffsetY);

        if (cellInThisTableRow is RowListItem rli) {
            var cellText = rli.Row.CellGetString(viewItem.Column);

            // Spalte erlaubt Mehrzeiler, wird aber einzeilig angezeigt ->
            // Edit-Feld vergrößern, damit mehrzeilig getippt werden kann.
            if (viewItem.Column is { IsDisposed: false, MultiLine: true } && controlPos.Height <= 30) {
                var lineCount = Math.Clamp(cellText.CountChar('\r') + 1, 3, 6);
                controlPos.Height = controlPos.Height * lineCount;
            }

            return (controlPos, rli.Row, cellText);
        }

        return (controlPos, null, string.Empty);
    }

    private RowListItem? GetRow(RowItem? row, bool onlyIfVisible) {
        if (row is null) { return null; }

        if (onlyIfVisible && _mustDoAllViewItems) { return null; }

        _ = AllViewItems;

        return _rowLookup.TryGetValue(row, out var rli) ? rli : null;
    }

    private void Invalidate_AllViewItems(bool andclear) {
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
                    if (kvp.Value is RowListItem rli && rli.Row.IsDisposed) {
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

    /// <summary>
    /// Prüft, ob das angegebene Arrangement die Ansicht 0 ("Alle Spalten") ist.
    /// In Ansicht 0 darf die Reihenfolge nicht verändert werden.
    /// </summary>
    private bool IsAnsicht0(ColumnViewCollection ca) {
        if (Table is not { IsDisposed: false } tb) { return false; }
        if (tb.ColumnArrangements.Count <= 0) { return false; }
        return string.Equals(tb.ColumnArrangements[0].KeyName, ca.KeyName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gibt wahr, wenn der Block unter dem übergebenen Header eingeklappt ist.
    /// Im NumberStyle wird der Zustand pro Block in
    /// <see cref="_collapsedBlockFirstRowKeys"/> gespeichert, sonst am
    /// <see cref="RowCaptionListItem.IsExpanded"/> des Originals.
    /// </summary>
    private bool IsChapterCollapsed(RowCaptionListItem rcli) {
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            var blockRows = GetChapterBlockRows(rcli);
            return blockRows is { Count: > 0 } && blockRows[0] is { IsDisposed: false } firstRow
                && _collapsedBlockFirstRowKeys.Contains(firstRow.KeyName);
        }
        return !rcli.IsExpanded;
    }

    private RowBackground? ItemAtPosition(int controlY, bool ignoreYOffset) {
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
    /// Letztes Drop-Ziel (Zeile oder Kapitel-Header) in <see cref="_sortedViewItems"/>
    /// oder null.
    /// </summary>
    private RowBackground? LastDroppableViewItem() {
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
        Invalidate_AllViewItems(false); // evtl. muss [Neue Zeile] ein/ausgebelndet werden
        Invalidate_MaxBounds();
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    private void PasteToCursor() {
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

    private void RemoveRowItems(RowItem row) {
        var toRemove = _allViewItems.Where(kvp => kvp.Value is RowListItem rli && !rli.IsDisposed && rli.Row == row)
                                     .Select(kvp => kvp.Key)
                                     .ToList();

        if (toRemove.Count == 0) { return; }
        foreach (var key in toRemove) {
            _allViewItems.Remove(key);
        }
        Invalidate_AllViewItems(false);
    }

    private void Row_RowAdded(object? sender, RowEventArgs e) {
        // RowAdded - da sind wirklich neue ZEilen in die Datenbank gekommen.
        // Deswegen können sich die Spaltenbreiten ändern.
        // Bei Bulk-Operationen (Load, Generate, Clear+Neu) feuern viele RowAdded-Events
        // in Folge. Der teure Invalidate_CurrentArrangement (setzt CurrentArrangement=null
        // und erzwingt ein ParseAll beim nächsten Paint) wird daher gebündelt: nur das
        // erste Event pro Paint-Zyklus führt ihn aus, alle weiteren setzen lediglich
        // _mustDoAllViewItems, damit die neuen Zeilen beim nächsten Aufbau berücksichtigt
        // werden. Das Flag wird in DrawControl zurückgesetzt.
        if (_pendingRowAddedRebuild) {
            _mustDoAllViewItems = true;
            return;
        }
        _pendingRowAddedRebuild = true;
        Invalidate_CurrentArrangement();
    }

    private void Row_RowRemoved(object? sender, RowEventArgs e) {
        if (GetRow(e.Row, true) is not null) {
            Invalidate_AllViewItems(false);
        }
    }

    // im Gegensatz zu Filter.RowsChanged - da sind nur die vorhandenen Zeilen geändert worden
    private void Row_RowRemoving(object? sender, RowEventArgs e) {
        if (IsDisposed) { return; }
        if (e.Row == CursorPosRow?.Row) { CursorPos_Reset(); }
        if (PinnedRows.Contains(e.Row)) {
            PinnedRows.Remove(e.Row);
        }
        // Veraltete Block-Zustände der entfernten Zeile verwerfen, damit
        // keine "Geister-Zustände" im NumberStyle übrig bleiben.
        _collapsedBlockFirstRowKeys.Remove(e.Row.KeyName);
    }

    /// <summary>
    /// Ermittelt das Zeilen-Element an der übergebenen Control-Y-Koordinate.
    /// Im Gegensatz zu <see cref="AbstractListItemExtension.ElementAtPosition"/>
    /// werden hierbei Elemente mit <see cref="AbstractListItem.IgnoreYOffset"/>
    /// (Spaltenkopf, Filterleiste, etc.) bevorzugt behandelt, da diese beim
    /// Zeichnen über den Zeilen liegen. Ohne diese Bevorzugung würde bei
    /// nach unten gescrolltem Y-Offset ein Klick auf den Spaltenkopf als
    /// Klick auf die darunterliegende Zeile gewertet werden.
    /// </summary>
    private RowBackground? RowItemAtPosition(int controlY) {
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
    /// Liefert die Y-Control-Koordinate, an der der Zeilenbereich beginnt
    /// (also die Unterkante aller IgnoreYOffset-Elemente wie Spaltenkopf,
    /// Filterleiste etc.). Drag/Drop-Operationen nutzen diesen Wert, um
    /// den Indikator nicht in den Head zu zeichnen und AutoScroll korrekt
    /// auf den Zeilenbereich zu begrenzen.
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
    /// Setzt den Auf-/Ausklapp-Zustand eines Kapitel-Headers auf den
    /// übergebenen Wert. Im NumberStyle (SYS_ROWSORTINDEX aktiv) wird der
    /// Zustand pro Block in <see cref="_collapsedBlockFirstRowKeys"/> abgelegt
    /// — der Block wird über den Key der ersten Zeile unter dem Header
    /// identifiziert. So bleibt jeder Block unabhängig, auch wenn derselbe
    /// Chapter-Text mehrfach vorkommt. Im Standard-Modus wird der Zustand am
    /// Original-Caption in <see cref="_allViewItems"/> synchronisiert.
    /// </summary>
    private void SetChapterExpanded(RowCaptionListItem rcli, bool expanded) {
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
        if (_allViewItems.TryGetValue(RowCaptionListItem.Identifier(rcli.ChapterText), out var cached)
            && cached is RowCaptionListItem cachedRcli
            && cachedRcli != rcli) {
            cachedRcli.IsExpanded = expanded;
        }
    }

    private RowSortDefinition? SortUsed() {
        if (Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false } sortCol) {
            return new RowSortDefinition(tb, sortCol, false);
        }
        return _sortDefinitionTemporary ?? Table?.SortDefinition;
    }

    /// <summary>
    /// Sucht den ersten und letzten Index in <see cref="_sortedViewItems"/>,
    /// dessen Row in <paramref name="srcRows"/> enthalten ist. Wird ein
    /// Kapitel-Block verschoben, gehört der Header (<see cref="RowCaptionListItem"/>
    /// aus <paramref name="dragItem"/>) ebenfalls zum Quell-Bereich. Ist der Block
    /// eingeklappt, sind seine Zeilen nicht in <see cref="_sortedViewItems"/>
    /// enthalten — der Quell-Bereich ist dann nur der Header selbst.
    /// </summary>
    private (int firstIdx, int lastIdx) SourceIndexRange(List<RowItem> srcRows, object? dragItem) {
        var first = -1;
        var last = -1;

        // Bei einem Kapitel-Block-Drag gehört der Header ebenfalls zum
        // Quell-Bereich — er verbraucht einen eigenen Index in _sortedViewItems.
        if (dragItem is RowCaptionListItem rcli) {
            var capIdx = _sortedViewItems.IndexOf(rcli);
            if (capIdx >= 0) { first = capIdx; }
        }

        for (var i = 0; i < _sortedViewItems.Count; i++) {
            if (_sortedViewItems[i] is RowListItem rli && srcRows.Contains(rli.Row)) {
                if (first < 0 || i < first) { first = i; }
                if (i > last) { last = i; }
            }
        }

        // Kapitel eingeklappt: Die Zeilen sind nicht in _sortedViewItems
        // enthalten. Der Quell-Bereich ist dann nur der Header selbst.
        if (last < 0 && dragItem is RowCaptionListItem) { last = first; }

        return (first, last);
    }

    private void Table_InvalidateView(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    /// <summary>
    /// Schaltet den Auf-/Ausklapp-Zustand eines Kapitel-Headers um.
    /// Siehe <see cref="SetChapterExpanded"/> für die Details zur
    /// Zustandsverwaltung im NumberStyle.
    /// </summary>
    private void ToggleChapterExpanded(RowCaptionListItem rcli) => SetChapterExpanded(rcli, IsChapterCollapsed(rcli));

    private void TXTBox_Close(GenericControl? textbox) {
        if (IsDisposed || textbox is null || Table is not { IsDisposed: false }) { return; }
        if (!textbox.Visible) { return; }
        if (textbox.Tag is not List<object?> { Count: >= 2 } tags) {
            textbox.Visible = false;
            return; // Ohne dem hier wird ganz am Anfang ein Ereignis ausgelöst
        }
        var w = textbox.Text;

        ColumnViewItem? column = null;
        RowListItem? row = null;

        if (tags[0] is ColumnViewItem c) { column = c; }
        if (tags[1] is RowListItem r) { row = r; }

        var isCaptionEdit = tags.Count >= 3 && tags[2] is string s && s == "CaptionEdit";
        var isCaptionGroupEdit = tags.Count >= 3 && tags[2] is string s2 && s2 == "CaptionGroupEdit";
        var isChapterEdit = tags.Count >= 3 && tags[2] is string s3 && s3 == "ChapterEdit";

        textbox.Tag = null;
        textbox.Visible = false;

        if (isCaptionEdit && column?.Column is { IsDisposed: false } col) {
            var newCaption = w.Replace("\r\n", "\r").Trim();
            if (!string.IsNullOrEmpty(newCaption)) {
                var namesMatch = col.Caption.Equals(col.KeyName, StringComparison.OrdinalIgnoreCase);
                col.Caption = newCaption;
                if (namesMatch) {
                    var newKey = newCaption.ReduceToChars(AllowedCharsVariableName).ToUpperInvariant();
                    if (!string.IsNullOrEmpty(newKey) && ColumnItem.IsValidColumnKey(newKey)) {
                        col.KeyName = newKey;
                    }
                }
            }
            Invalidate_CurrentArrangement();
        } else if (isCaptionGroupEdit && column?.Column is { IsDisposed: false } col2 && tags.Count >= 4 && tags[3] is int captionIndex) {
            var newGroup = w.Replace("\r\n", "\r").Trim();
            switch (captionIndex) {
                case 0:
                    col2.CaptionGroup1 = newGroup;
                    break;

                case 1:
                    col2.CaptionGroup2 = newGroup;
                    break;

                case 2:
                    col2.CaptionGroup3 = newGroup;
                    break;
            }
            Invalidate_CurrentArrangement();
        } else if (isChapterEdit && tags[1] is RowCaptionListItem rcli
            && rcli.Arrangement?.ColumnForChapter is { IsDisposed: false } capCol
            && rcli.Arrangement.Table is { IsDisposed: false } tbChapter) {
            var oldChapter = rcli.ChapterText;
            // NumberStyle: Kapitel-Trenner wird ignoriert — der gesamte Wert
            // ist ein flaches Kapitel. Es gibt keinen Parent-Pfad und keine
            // Unterpfade, der eingegebene Text ersetzt den Kapitel-Wert komplett.
            var numberStyle = RowCaptionListItem.IsNumberStyle(rcli.Arrangement);
            var parentPath = numberStyle ? string.Empty : oldChapter.ChapterPathParent();

            // Bei Ohne-Kapiteln ("-?-") enthalten die Zellen den Parent-Pfad
            // (oder leer bei Top-Level), nicht den Text "-?-".
            var isOhne = rcli.IsOhneChapter;
            var cellMatchValue = isOhne ? parentPath : oldChapter;

            // newChapter: im NumberStyle der komplette neue Wert, sonst nur das
            // letzte Segment ersetzen (Parent bleibt erhalten).
            var newLastName = w.Replace("\r\n", "\r").ChapterPathNormalize();
            var newChapter = numberStyle || string.IsNullOrEmpty(parentPath)
                ? newLastName
                : parentPath + RowCaptionListItem.Kapiteltrenner + newLastName;

            // Nur der zusammenhängende Block unter dem geklickten Header
            // (tags[3]). Ist der Block nicht ermittelt worden (z. B. weil
            // die Ansicht inzwischen neu aufgebaut wurde), auf alle Zeilen
            // zurückgreifen.
            var blockRows = tags.Count >= 4 && tags[3] is List<RowItem> br
                ? br
                : [.. tbChapter.Row.Where(r => r is { IsDisposed: false })];

            if (!string.IsNullOrEmpty(newLastName) && newChapter != oldChapter) {
                // Kapitel umbenennen.
                foreach (var tableRow in blockRows) {
                    if (tableRow is not { IsDisposed: false }) { continue; }
                    var values = tableRow.CellGetList(capCol);
                    var changed = false;
                    for (var i = 0; i < values.Count; i++) {
                        // Vergleich normalisiert durchführen.
                        var valueNorm = values[i].ChapterPathNormalize();
                        if (valueNorm == cellMatchValue) {
                            values[i] = newChapter;
                            changed = true;
                        } else if (!numberStyle && !isOhne && valueNorm.StartsWith(oldChapter + RowCaptionListItem.Kapiteltrenner, StringComparison.Ordinal)) {
                            // Suffix hinter dem Prefix unverändert übernehmen.
                            // Bei Ohne-Kapiteln gibt es keine Unterpfade.
                            values[i] = newChapter + valueNorm[oldChapter.Length..];
                            changed = true;
                        }
                    }

                    // Bei Ohne-Kapiteln kann CellGetList eine leere Liste
                    // zurückgeben (Zelle hat gar keinen Wert). In diesem Fall
                    // newChapter als einzigen Wert setzen.
                    if (isOhne && !changed && values.Count == 0) {
                        values.Add(newChapter);
                        changed = true;
                    }

                    if (changed) {
                        tableRow.CellSet(capCol, values, "Kapitel umbenannt: " + oldChapter + " → " + newChapter);
                    }
                }
            } else if (string.IsNullOrEmpty(newLastName) && !isOhne) {
                // Kapitel löschen: passenden Wert aus den Zellen entfernen,
                // sodass die Zeilen unter -?- (Ohne) erscheinen.
                foreach (var tableRow in blockRows) {
                    if (tableRow is not { IsDisposed: false }) { continue; }
                    var values = tableRow.CellGetList(capCol);
                    var changed = false;
                    for (var i = 0; i < values.Count; i++) {
                        if (values[i].ChapterPathNormalize() == oldChapter) {
                            values[i] = string.Empty;
                            changed = true;
                        }
                    }
                    if (changed) {
                        tableRow.CellSet(capCol, values, "Kapitel entfernt: " + oldChapter);
                    }
                }
            }
            Invalidate_AllViewItems(true);
        } else {
            NotEditableInfo(UserEdited(this, w, column, row, true));
        }

        Focus();
    }

    /// <summary>
    /// Aktualisiert das Kapitel der verschobenen Zeile, wenn sie in einen
    /// anderen Kapitel-Bereich verschoben wurde. Das Ziel-Kapitel wird anhand
    /// der direkt oberhalb liegenden Zeile bestimmt (oder unterhalb am Anfang).
    /// </summary>
    private void UpdateChapterOnRowSortMove(RowListItem sourceRli, int mouseControlY) {
        if (sourceRli.Arrangement is not { IsDisposed: false } ca) { return; }
        if (ca.ColumnForChapter is not { IsDisposed: false } capCol) { return; }
        if (sourceRli.Row is not { IsDisposed: false } sourceRow) { return; }

        _ = AllViewItems; // _sortedViewItems sicherstellen

        // Ziel-Kapitel anhand der Mausposition bestimmen: _sortedViewItems
        // (mit Headern und Zeilen) wird top-down iteriert. Der letzte Header,
        // dessen Top-Kante auf oder oberhalb der Mausposition liegt, ist das
        // Ziel-Kapitel. Sobald ein Item unterhalb der Maus beginnt, wird
        // abgebrochen. So wird auch erkannt, wenn die Maus direkt auf einem
        // Kapitel-Header steht (die Zeile soll als erste unter diesem
        // Kapitel landen) — früher wurde fälschlich die Nachbar-ZEILE
        // oberhalb herangezogen, was an Kapitel-Grenzen das falsche Kapitel
        // lieferte.
        string? targetChapterText = null;

        foreach (var item in _sortedViewItems) {
            if (!item.Visible) { continue; }

            var itemTop = item.ControlPosition(Zoom, OffsetX, OffsetY).Top;
            if (itemTop > mouseControlY) { break; }

            if (item is RowCaptionListItem header) {
                targetChapterText = header.ChapterText;
            }
        }

        // Kein Header oberhalb der Maus → "Ohne" (kein Kapitel)
        if (string.IsNullOrEmpty(targetChapterText)) {
            if (sourceRow.CellGetString(capCol) is { Length: > 0 }) {
                sourceRow.CellSet(capCol, string.Empty, "Drag/Drop: Kapitel entfernt");
            }
            return;
        }

        // Spezial-Kapitel (Angepinnt) nicht als Ziel verwenden
        if (string.Equals(targetChapterText, DummyPinned, StringComparison.OrdinalIgnoreCase)) { return; }

        // Bei "Ohne" (-?-): Kapitel der Zeile leeren
        if (string.Equals(targetChapterText, Ohne, StringComparison.OrdinalIgnoreCase)) {
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