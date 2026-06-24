// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.TableItems;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableViewWithFilters : GenericControlReciverSender, ITranslateable, IHasTable, IStyleable {

    #region Fields

    private bool _controlsCorrect;
    private bool _doingControls;
    private bool _scriptButtonsCorrect;

    #endregion

    #region Constructors

    public TableViewWithFilters() : base(false, false, false) {
        InitializeComponent();

        // --- Filter-Pipeline ---
        // Datenfluss:
        //   FilterFix (eigenständig, z.B. ViewLoading)
        //   + FilterInput (von Parent-Controls, aggregiert durch GenericControlReciver)
        //   → HandleChangesNow → TableInternal.FilterFix
        //   + TableInternal.Filter (Benutzerfilter aus FlexiControls, Zeilenfilter)
        //   → DoFilterCombined → TableInternal.FilterCombined
        //   → DoFilterOutput → FilterOutput (an Child-Controls)

        // FilterCombinedChanged feuert wenn FilterCombined sich ändert (egal ob durch Filter oder FilterFix).
        TableInternal.FilterCombinedChanged += TableInternal_FilterCombinedChanged;

        // Änderungen am eigenen FilterFix müssen die Pipeline triggern,
        // damit HandleChangesNow die Änderung an TableInternal.FilterFix weitergibt.
        FilterFix.PropertyChanged += FilterFix_PropertyChanged;
        TableInternal.VisibleRowsChanged += TableInternal_VisibleRowsChanged;
        TableInternal.SelectedRowChanged += TableInternal_SelectedRowChanged;
        TableInternal.ViewChanged += TableInternal_ViewChanged;
        TableInternal.SelectedCellChanged += TableInternal_SelectedCellChanged;
        TableInternal.TableChanged += TableInternal_TableChanged;
        TableInternal.CellClicked += TableInternal_CellClicked;
        TableInternal.DoubleClick += TableInternal_DoubleClick;
        TableInternal.PinnedChanged += TableInternal_PinnedChanged;
        TableInternal.ViewLoading += TableInternal_ViewLoading;
        TableInternal.ViewSaving += TableInternal_ViewSaving;
    }

    #endregion

    #region Events

    public event EventHandler<CellEventArgs>? CellClicked;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowNullableEventArgs>? SelectedRowChanged;

    public event EventHandler<TableEventArgs>? TableChanged;

    public event EventHandler<TableEventArgs>? ViewChanged;

    public event EventHandler<JsonEventArgs>? ViewLoading;

    public event EventHandler<JsonEventArgs>? ViewSaving;

    public event EventHandler<TableEventArgs>? VisibleRowsChanged;

    #endregion

    #region Properties

    public bool Ansichtbearbeitung {
        get => TableInternal.Ansichtbearbeitung;
        set => TableInternal.Ansichtbearbeitung = value;
    }

    [DefaultValue("")]
    [Description("Welche Spaltenanordnung angezeigt werden soll")]
    public string Arrangement {
        get => TableInternal.Arrangement;
        set => TableInternal.Arrangement = value;
    }

    [DefaultValue(false)]
    public bool AutoPin { get; set; }

    /// <summary>
    /// Gibt an, ob das Standard-Kontextmenu der Tabellenansicht angezeitgt werden soll oder nicht
    /// </summary>
    [DefaultValue(true)]
    public bool ContextMenuDefault {
        get => TableInternal.ContextMenuDefault;
        set => TableInternal.ContextMenuDefault = value;
    }

    public ColumnViewCollection? CurrentArrangement => TableInternal.CurrentArrangement;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnViewItem? CursorPosColumn => TableInternal.CursorPosColumn;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowListItem? CursorPosRow => TableInternal.CursorPosRow;

    [Browsable(false)]
    [DefaultValue(null)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
        get => TableInternal.CustomContextMenuItems;
        set => TableInternal.CustomContextMenuItems = value;
    }

    [DefaultValue(false)]
    public bool EditButton {
        get => TableInternal.EditButton;
        set => TableInternal.EditButton = value;
    }

    /// <summary>
    /// Zusammengeführter Filter aus Benutzerfilter + Fixfilter + FilterInput.
    /// Dies ist der tatsächlich aktive Filter. Wird von TableView automatisch berechnet.
    /// </summary>
    public FilterCollection FilterCombined => TableInternal.FilterCombined;

    /// <summary>
    /// Eigener Fixfilter von TableViewWithFilters, unabhängig vom FilterInput der Parent-Controls.
    /// Wird z.B. durch ViewLoading gesetzt und in HandleChangesNow zusammen mit FilterInput
    /// an TableInternal.FilterFix übergeben.
    /// </summary>
    public FilterCollection FilterFix { get; } = new("TableViewWithFilters.FilterFix");

    [DefaultValue(GroupBoxStyle.Nothing)]
    public GroupBoxStyle GroupBoxStyle {
        get;
        set {
            if (field == value) { return; }
            field = value;
            grpBorder.GroupBoxStyle = value;
            Invalidate();
        }
    } = GroupBoxStyle.Nothing;

    public List<RowItem>? PinnedRows => TableInternal.PinnedRows;

    public bool PowerEdit {
        get => TableInternal.PowerEdit;
        set => TableInternal.PowerEdit = value;
    }

    public List<RowListItem> RowViewItems => TableInternal.RowViewItems;

    [DefaultValue(Constants.Win11)]
    public string SheetStyle {
        get => TableInternal.SheetStyle;
        set => TableInternal.SheetStyle = value;
    }

    public bool ShowWaitScreen {
        get => TableInternal.ShowWaitScreen;
        internal set => TableInternal.ShowWaitScreen = value;
    }

    public RowSortDefinition? SortDefinitionTemporary {
        get => TableInternal.SortDefinitionTemporary;
        internal set => TableInternal.SortDefinitionTemporary = value;
    }

    /// <summary>
    /// Tabellen können über das Property gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? Table {
        get => TableInternal.Table;
        set => TableInternal.Table = value;
    }

    /// <summary>
    /// Interne TableView-Instanz.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TableView TableView => TableInternal;

    [DefaultValue(true)]
    public bool Translate {
        get => TableInternal.Translate;
        set => TableInternal.Translate = value;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom {
        get => TableInternal.Zoom;
        set => TableInternal.Zoom = value;
    }

    #endregion

    #region Methods

    public static List<JsonEntry> GetViews(string tableKey) {
        ViewManager.InitializeIfNeeded();
        return ViewManager.GetViews(tableKey);
    }

    public (ColumnViewItem?, RowBackground?) CellOnLastMouseDown() => TableInternal.CellOnLastMouseDown();

    public void CollapesAll() => TableInternal.CollapesAll();

    public void CursorPos_Set(ColumnViewItem? columnViewItem, RowListItem? rowDataListItem, bool ensureVisible) => TableInternal.CursorPos_Set(columnViewItem, rowDataListItem, ensureVisible);

    public void DeleteView(string viewName) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }
        ViewManager.DeleteView(tbf.KeyName, viewName);
    }

    public void DoZoom(bool zoomin) => TableInternal.DoZoom(zoomin);

    public void ExpandAll() => TableInternal.ExpandAll();

    public string Export_CSV(FirstRow columnCaption) => TableInternal.Export_CSV(columnCaption);

    public void Export_HTML(string filename, bool execute) => TableInternal.Export_HTML(filename, execute);

    public void Export_HTML() => TableInternal.Export_HTML();

    public void ImportClipboard() => TableInternal.ImportClipboard();

    public void ImportCsv(string csvtxt) => TableInternal.ImportCsv(csvtxt);

    public string IsCellEditable(ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, string? newChunkVal, bool maychangeview) => TableInternal.IsCellEditable(cellInThisTableColumn, cellInThisTableRow, newChunkVal, maychangeview);

    public void OpenSearchAndReplaceInCells() => TableInternal.OpenSearchAndReplaceInCells();

    public void OpenSearchAndReplaceInTbScripts() => TableInternal.OpenSearchAndReplaceInTbScripts();

    public void OpenSearchInCells() => TableInternal.OpenSearchInCells();

    public void ParseView(JsonObject? viewData) => TableInternal.ParseView(viewData);

    public void Pin(IReadOnlyList<RowItem>? rows) => TableInternal.Pin(rows);

    public void RowCleanUp() => TableInternal.RowCleanUp();

    public IReadOnlyList<RowItem> RowsVisibleUnique() => TableInternal.RowsVisibleUnique();

    public void SaveCurrentView(string viewName) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }
        ViewManager.SaveView(tbf.KeyName, viewName, ViewToJson());
    }

    public void SetView(JsonObject? view) => TableInternal.SetView(view);

    public bool TryLoadView(string viewName) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return false; }
        var savedViews = ViewManager.GetViews(tbf.KeyName);
        var entry = savedViews.Find(v => string.Equals(v.KeyName, viewName, StringComparison.OrdinalIgnoreCase));
        if (entry is not null && entry.JsonData.ValueKind != JsonValueKind.Undefined) {
            if (JsonSerializer.Deserialize<JsonObject>(entry.JsonData) is { } viewObj) {
                SetView(viewObj);
                return true;
            }
        }
        return false;
    }

    public ColumnViewItem? View_ColumnFirst() => TableInternal.View_ColumnFirst();

    public RowListItem? View_RowFirst() => TableInternal.View_RowFirst();

    public JsonObject ViewToJson() => TableInternal.ViewToJson();

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                FilterFix.Dispose();
                TableInternal.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        if (InvokeRequired) {
            Invoke(new Action(() => DrawControl(gr, state)));
            return;
        }
        base.DrawControl(gr, state);

        //if(state.HasFlag(States.Standard_HasFocus)) { state ^= Standard_HasFocus}

        // Hintergrund der Filterleiste zeichnen (falls nötig)
        //Skin.Draw_Back(gr, Design.GroupBox, States.Standard, base.DisplayRectangle, this, true);

        DoControls();
    }

    /// <summary>
    /// Synchronisiert die Filter-Pipeline:
    ///   FilterFix (eigenständig, z.B. aus ViewLoading)
    ///   + FilterInput (aggregiert aus Parent-Controls)
    ///   → TableInternal.FilterFix
    ///
    /// TableView berechnet dann automatisch:
    ///   TableInternal.Filter (Benutzerfilter) + TableInternal.FilterFix → FilterCombined
    ///
    /// Wird bei jedem Draw-Zyklus aufgerufen (via GenericControlReciver.DrawControl).
    /// </summary>
    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        if (Table is not { IsDisposed: false } tb) { return; }

        DoInputFilter(FilterOutput.Table, false);

        // Eigenen FilterFix und FilterInput zusammenführen → TableInternal.FilterFix
        // TableView.DoFilterCombined() wird automatisch durch FilterFix.PropertyChanged ausgelöst.
        using var nfc = new FilterCollection(tb, "CombinedFilterFix");

        // Erst den eigenen Fixfilter (ohne Origin-Änderung)
        if (FilterFix is { IsDisposed: false }) {
            nfc.RemoveOtherAndAdd(FilterFix, null);
        }

        // Dann FilterInput der Parents (mit Origin-Kennzeichnung)
        if (FilterInput is { IsDisposed: false }) {
            nfc.RemoveOtherAndAdd(FilterInput, "Filter aus übergeordneten Element");
        }

        TableInternal.FilterFix.ChangeTo(nfc);
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);

        CheckButtons();
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);

        DoFilterOutput();

        // Anfängliche Positionierung der Steuerelemente
        Invalidate_ScriptButtons();
        Invalidate_Controls();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        Invalidate_Controls();
    }

    protected virtual void OnViewLoading(JsonEventArgs e) => ViewLoading?.Invoke(this, e);

    protected virtual void OnViewSaving(JsonEventArgs e) => ViewSaving?.Invoke(this, e);

    protected override void OnVisibleChanged(System.EventArgs e) {
        OnTableChanged();
        base.OnVisibleChanged(e);
    }

    private void B_Click(object? sender, System.EventArgs e) {
        if (sender is not Button b ||
            b.Tag is not string keyn ||
            TableInternal.Table is not { IsDisposed: false } tb ||
            tb.EventScript.GetByKey(keyn, StringComparison.OrdinalIgnoreCase) is not { } script ||
            !script.IsOk()) {
            QuickNote.Show(NoteSymbols.Critical, "Interner Fehler");
            return;
        }

        ((IContextMenu)TableView).ExecuteContextMenuComand(TableView.ContextMenu_ExecuteScript, script, TableView.ContextMenuItemGenerate(TableInternal, null, null, RowsVisibleUnique()));
    }

    private void btnAlleFilterAus_Click(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        TableInternal.Filter.Clear();
    }

    private void btnPin_Click(object? sender, System.EventArgs e) => TableInternal.Pin(RowsVisibleUnique());

    private void btnPinZurück_Click(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        TableInternal.Pin(null);
    }

    private void btnTextLöschen_Click(object? sender, System.EventArgs e) => txbZeilenFilter.Text = string.Empty;

    private void btnViewManager_Click(object? sender, System.EventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }

        var savedViews = GetViews(tbf.KeyName);
        var autoLoad = ViewManager.GetAutoLoadLastView(tbf.KeyName);

        var items = new List<AbstractListItem>();

        var c = 0;

        foreach (var sv in savedViews) {
            if (string.Equals(sv.KeyName, ViewManager.Last, StringComparison.OrdinalIgnoreCase)) {
                if (autoLoad) { continue; }
                if (c == 0) { items.Add(ItemOf("Gepeicherte Ansichten:", true)); }

                items.Add(ItemOf("Letzte Ansicht laden", sv.KeyName, ImageCode.Uhr, ViewManager_LoadView, true, $"Stand: {sv.Modified.ToString5()}"));
            } else {
                if (c == 0) { items.Add(ItemOf("Gepeicherte Ansichten", true)); }

                var isStandard = string.Equals(sv.KeyName, ViewManager.Standard, StringComparison.OrdinalIgnoreCase);
                var symbol = isStandard ? ImageCode.Stern : ImageCode.Tabelle;
                var quickInfo = isStandard ? "Diese Ansicht wird automatisch geladen.\rZum Deaktivieren, Ansicht löschen." : string.Empty;
                items.Add(ItemOf(sv.KeyName, sv.KeyName, symbol, ViewManager_LoadView, true, quickInfo));
            }

            c++;
        }

        items.Add(ItemOf("Verwaltung:", true));

        var saveItem = ItemOf("Aktuelle Ansicht speichern", "SaveView", ImageCode.PlusZeichen, ViewManager_SaveView, true);
        saveItem.RemoveLocked = true;
        items.Add(saveItem);

        if (autoLoad) {
            var autoItem = ItemOf("Auto-Laden deaktivieren", "AutoLoadLastView", ImageCode.Häkchen, ViewManager_ToggleAutoLoad, true, "Letzte Ansicht automatisch laden\rAktuell in dieser Tabelle: <b>AKTIV");
            autoItem.RemoveLocked = true;
            items.Add(autoItem);
        } else {
            var autoItem = ItemOf("Auto-Laden aktivieren", "AutoLoadLastView", ImageCode.HäkchenDoppelt, ViewManager_ToggleAutoLoad, true, "Letzte Ansicht automatisch laden\rAktuell in dieser Tabelle: <b>INAKTIV");
            autoItem.RemoveLocked = true;
            items.Add(autoItem);
        }

        var setStdItem = ItemOf("Standard-Ansicht setzen", "SetStandardView", ImageCode.Stern, ViewManager_SetStandardView, true, "Aktuelle Ansicht als Standard-Ansicht speichern\rWird beim Öffnen automatisch geladen\rAuto-Laden wird dabei deaktiviert");
        setStdItem.RemoveLocked = true;
        items.Add(setStdItem);

        items.Add(Separator());
        var abortItem = ItemOf("Abbruch", ImageCode.TasteESC);
        abortItem.RemoveLocked = true;
        items.Add(abortItem);

        var dropDown = FloatingInputBoxListBoxStyle.Show(items, CheckBehavior.NoSelection, null, this, false, ListBoxAppearance.DropdownSelectbox, Design.Item_ContextMenu, false, savedViews.Count > 0);
        dropDown.ItemRemoved += DropDown_ItemRemoved;
    }

    private void CheckButtons() {
        if (!grpFilter.Visible) { return; }

        var hasTB = Table is not null && Enabled;
        var hasSortIndex = Table is { IsDisposed: false } tb && tb.Column.SysRowSortIndex is { IsDisposed: false };

        // Status der Steuerelemente aktualisieren
        btnPinZurück.Enabled = hasTB && !hasSortIndex && TableInternal.PinnedRows.Count > 0;
        txbZeilenFilter.Enabled = hasTB && LanguageTool.Translation is null;
        btnAlleFilterAus.Enabled = hasTB;
        btnPin.Enabled = hasTB && !hasSortIndex;
        btnViewManager.Enabled = Table is TableFile { IsDisposed: false };

        // Text im ZeilenFilter aktualisieren
        if (hasTB && TableInternal.Filter.IsRowFilterActiv()) {
            txbZeilenFilter.Text = TableInternal.Filter.RowFilterText;
        } else {
            txbZeilenFilter.Text = string.Empty;
        }

        // Status des Löschen-Buttons aktualisieren
        btnTextLöschen.Enabled = hasTB && !string.IsNullOrEmpty(txbZeilenFilter.Text);
    }

    private void DoControls() {
        if (InvokeRequired) {
            Invoke(new Action(DoControls));
            return;
        }

        if (IsDisposed) { return; }

        if (_controlsCorrect) { return; }

        if (_doingControls) { return; }
        _doingControls = true;
        _controlsCorrect = true;

        RepositionControls();
        CheckButtons();
        DoFilterAndPinButtons();
        DoScriptButtons();

        _doingControls = false;

        if (!_controlsCorrect) { Invalidate(); }
    }

    private void DoFilterAndPinButtons() {
        if (IsDisposed) { return; }
        if (!grpFilter.Visible) { return; }

        var cu = CurrentArrangement;

        #region Pin Button

        var hasSortIndex = Table is { IsDisposed: false } tbPin && tbPin.Column.SysRowSortIndex is { IsDisposed: false };
        btnPinZurück.Enabled = Table is not null && !hasSortIndex && TableInternal.PinnedRows.Count > 0;

        #endregion

        #region ZeilenFilter befüllen

        txbZeilenFilter.Text = Table is not null && TableInternal.Filter.IsRowFilterActiv()
                                ? TableInternal.Filter.RowFilterText
                                : string.Empty;

        #endregion

        #region Alle vorhnaden Flexis ermitteln und erst mal zum Löschen vormerken

        List<FlexiControlForFilter> flexsToDelete = [];

        foreach (var thisControl in grpFilter.Controls) {
            if (thisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
        }

        #endregion

        if (Table is { IsDisposed: false } tb) {

            #region Reihenfolge der Spalten bestimmen

            // columSort bestimmt, welche Spalten einen FlexiControlForFilter bekommen.
            // Quelle 1: Filter_immer_Anzeigen aus dem Arrangement (immer sichtbar).
            // Quelle 2: TableInternal.FilterCombined (alle aktiven Spaltenfilter).
            //
            // Ausnahme: Wenn FilterCombined ein AlwaysFalse enthält, kann keine Zeile
            // jemals sichtbar werden. Alle Filter-Controls wären nutzlos ("geht's auch
            // hier weiter") → keine FlexiControlForFilter erzeugen.
            List<ColumnItem> columSort = [];
            if (!TableInternal.FilterCombined.HasAlwaysFalse()) {
                if (cu?.Filter_immer_Anzeigen is { } orderArrangement) {
                    foreach (var thisColumn in orderArrangement) {
                        if (tb.Column[thisColumn] is { IsDisposed: false } ci) { columSort.AddIfNotExists(ci); }
                    }
                }

                foreach (var thisColumn in TableInternal.FilterCombined) {
                    if (thisColumn.Column is { IsDisposed: false } col) { columSort.AddIfNotExists(col); }
                }
            }

            #endregion

            #region Standard-Variablen ermitteln

            var filterHeight = btnAlleFilterAus.Height;
            var filterWidth = (int)(txbZeilenFilter.Width * 1.5);
            var startPositionX = btnViewManager.Right + (Skin.Padding * 3);
            var addX = filterWidth + Skin.PaddingSmal;
            var addY = filterHeight + Skin.PaddingSmal;
            var toppos = btnAlleFilterAus.Top;
            var leftpos = startPositionX - addX;

            #endregion

            var firstFilter = true;

            foreach (var thisColumn in columSort) {
                if (thisColumn.AutoFilterSymbolPossible()) {

                    #region Position berechnen und evtl. Schleife verlassen

                    leftpos += addX;

                    var thisFilterWidth = filterWidth;
                    if (firstFilter && leftpos + 72 <= grpFilter.Width) {
                        if (leftpos + filterWidth > grpFilter.Width) {
                            thisFilterWidth = grpFilter.Width - leftpos - Skin.PaddingSmal;
                        }
                        firstFilter = false;
                    } else {
                        if (leftpos + filterWidth > grpFilter.Width) {
                            leftpos = startPositionX;
                            toppos += addY;
                            if (toppos + filterHeight > grpFilter.Height || startPositionX + filterWidth > grpFilter.Width) { break; }
                        }
                    }

                    #endregion

                    #region Vorhandenen Filter suchen

                    FlexiControlForFilter? flx = null;
                    foreach (var thisControl in grpFilter.Controls) {
                        if (thisControl is FlexiControlForFilter flxc) {
                            if (flxc.FilterSingleColumn == thisColumn) { flx = flxc; break; }
                        }
                    }

                    #endregion

                    #region Filter bei Bedarf erstellen

                    if (flx is null) {
                        flx = new FlexiControlForFilter(thisColumn, CaptionPosition.Links_neben_dem_Feld, FlexiFilterDefaultOutput.Alles_Anzeigen, FlexiFilterDefaultFilter.Textteil, true, false);
                        flx.FilterOutput.Table = thisColumn.Table;
                        ChildIsBorn(flx);
                        flx.FilterOutputPropertyChanged += FlexSingeFilter_FilterOutputPropertyChanged;
                        grpFilter.Controls.Add(flx);
                    }

                    #endregion

                    #region Filter positionieren

                    flx.Top = toppos;
                    flx.Left = leftpos;
                    flx.Width = thisFilterWidth;
                    flx.Height = filterHeight;
                    flx.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    flexsToDelete.Remove(flx);

                    #endregion
                }
            }
        }

        #region Unnötige Flexis löschen

        foreach (var thisFlexi in flexsToDelete) {
            thisFlexi.FilterOutputPropertyChanged -= FlexSingeFilter_FilterOutputPropertyChanged;
            thisFlexi.Visible = false;
            grpFilter.Controls.Remove(thisFlexi);
            thisFlexi.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Schreibt FilterCombined (ggf. eingeschränkt auf die ausgewählte Zeile) in FilterOutput.
    /// FilterOutput wird an alle Child-Controls weitergegeben.
    /// Wird aufgerufen wenn FilterCombined sich ändert oder die Zeilenauswahl wechselt.
    /// </summary>
    private void DoFilterOutput() {
        if (IsDisposed) { return; }

        if (CursorPosRow?.Row is { IsDisposed: false } setedrow) {
            using var nfc = new FilterCollection(setedrow, "Temp TableOutput");
            nfc.RemoveOtherAndAdd(TableInternal.FilterCombined, null);
            if (!FilterOutput.IsDifferentTo(nfc)) { return; }
            FilterOutput.ChangeTo(nfc);
        } else {
            if (!FilterOutput.IsDifferentTo(TableInternal.FilterCombined)) { return; }
            FilterOutput.ChangeTo(TableInternal.FilterCombined);
        }
    }

    private void DoScriptButtons() {
        if (_scriptButtonsCorrect) { return; }

        if (!grpButtons.Visible) { return; }

        #region Controls löschen

        var controls = grpButtons.Controls.OfType<Control>().ToArray();
        foreach (var thisControl in controls) {
            if (thisControl is Button b) {
                b.Click -= B_Click;
            }
            thisControl?.Dispose();
        }
        grpButtons.Controls.Clear();

        #endregion

        if (CurrentArrangement is not { } ca || ca.Ausführbare_Skripte.Count < 1 || TableInternal.Table is not { IsDisposed: false } tb) {
            _scriptButtonsCorrect = true;
            return;
        }

        var leftP = Skin.Padding;
        var top = (grpButtons.Height - txbZeilenFilter.Height) / 2;
        var firstButton = true;
        var buttonWidth = txbZeilenFilter.Width;

        foreach (var thisString in ca.Ausführbare_Skripte) {
            if (tb.EventScript.GetByKey(thisString, StringComparison.OrdinalIgnoreCase) is { } thiss) {
                if (firstButton && leftP + 64 > grpButtons.Width) {
                    break;
                }

                var b = new Button();

                b.Enabled = thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null) && thiss.IsOk();
                b.Visible = true;
                b.Text = thiss.ReadableText();
                b.ImageCode = thiss.SymbolForReadableText().ToString();
                b.QuickInfo = thiss.QuickInfo;
                b.Click += B_Click;
                grpButtons.Controls.Add(b);

                if (firstButton && leftP + buttonWidth > grpButtons.Width) {
                    b.Width = grpButtons.Width - leftP;
                } else {
                    b.Width = buttonWidth;
                }
                firstButton = false;

                b.Height = txbZeilenFilter.Height;
                b.Top = top;
                b.Left = leftP;
                b.Tag = thisString;
                leftP = b.Right + Skin.PaddingSmal;

                //var enabled = thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null) && thiss.NeedRow && thiss.IsOk();
                //e.ContextMenu.Add(ItemOf(thiss.ReadableText(), thiss.SymbolForReadableText(), ContextMenu_ExecuteScript, new { Script = thiss, Row = row5 }, enabled, thiss.QuickInfo));
            }
        }

        _scriptButtonsCorrect = true;
    }

    private void DropDown_ItemRemoved(object? sender, AbstractListItemEventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false }) { return; }
        if (e.Item.RemoveLocked) { return; }
        if (e.Item is not TextListItem tli) { return; }

        DeleteView(tli.KeyName);
    }

    private void Filter_ZeilenFilterSetzen() {
        if (IsDisposed || (Table?.IsDisposed ?? true)) { return; }

        var currentFilter = TableInternal.Filter.RowFilterText;
        var newFilter = txbZeilenFilter.Text;

        if (string.Equals(currentFilter, newFilter, StringComparison.OrdinalIgnoreCase)) { return; }

        if (string.IsNullOrEmpty(newFilter)) {
            TableInternal.Filter.Remove_RowFilter();
            return;
        }

        TableInternal.Filter.RowFilterText = newFilter;
    }

    /// <summary>
    /// Reagiert auf Änderungen am eigenen FilterFix (z.B. externes .Add() oder ViewLoading).
    /// Löst HandleChangesNow aus, damit FilterFix + FilterInput → TableInternal.FilterFix neu berechnet wird.
    /// </summary>
    private void FilterFix_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }

        if (Table is not { IsDisposed: false } tb) {
            Invalidate_FilterInput();
            return;
        }

        using var nfc = new FilterCollection(tb, "FilterFixChanged");
        if (FilterFix is { IsDisposed: false }) {
            nfc.RemoveOtherAndAdd(FilterFix, null);
        }

        if (FilterInput is { IsDisposed: false }) {
            nfc.RemoveOtherAndAdd(FilterInput, "Filter aus übergeordneten Element");
        }

        TableInternal.FilterFix.ChangeTo(nfc);
    }

    private void FlexSingeFilter_FilterOutputPropertyChanged(object? sender, System.EventArgs e) {
        if (sender is not FlexiControlForFilter ffc) { return; }

        if (ffc.FilterOutput is not { } fc) { return; }

        var fi = fc[ffc.FilterSingleColumn];

        if (fi is null) {
            TableInternal.Filter.Remove(ffc.FilterSingleColumn);
        } else {
            if (!string.IsNullOrEmpty(fi.Origin)) { return; }
            // Besser wäre FilterInput[ffc.FilterSingleColumn].Equals(fi)
            // Aber das Origin stimmt nicht

            TableInternal.Filter.RemoveOtherAndAdd(fi);
        }

        //DoFilterOutput();
    }

    private void Invalidate_Controls() {
        _controlsCorrect = false;
        Invalidate();
    }

    private void Invalidate_ScriptButtons() {
        _scriptButtonsCorrect = false;
        Invalidate_Controls();
    }

    private void OnCellClicked(CellEventArgs e) => CellClicked?.Invoke(this, e);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowNullableEventArgs e) {
        DoFilterOutput();
        SelectedRowChanged?.Invoke(this, e);
    }

    private void OnTableChanged() {
        Invalidate_FilterInput();
        Invalidate_RowsInput();
        Invalidate_ScriptButtons();
        Invalidate_Controls();

        TableChanged?.Invoke(this, new TableEventArgs(Table));
    }

    private void OnViewChanged() => ViewChanged?.Invoke(this, new TableEventArgs(Table));

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, new TableEventArgs(Table));

    /// <summary>
    /// Positioniert die Steuerelemente in der Table neu, um Platz für die Filterleiste zu schaffen.
    /// </summary>
    private void RepositionControls() {
        var cu = CurrentArrangement;

        var filterRows = cu?.FilterRows ?? 0;

        var left = 0;
        var top = 0;
        var right = 0;
        var bottom = 0;

        switch (GroupBoxStyle) {
            case GroupBoxStyle.NormalBold:
            case GroupBoxStyle.Normal:
                left = Skin.Padding;
                right = Skin.Padding;
                bottom = Skin.Padding;
                top = Skin.Padding + 16;
                break;

            case GroupBoxStyle.RibbonBar:
                left = Skin.Padding;
                right = Skin.Padding;
                bottom = Skin.Padding + 16;
                top = Skin.Padding;
                break;

            case GroupBoxStyle.Nothing:
                break;

            case GroupBoxStyle.RoundRect:
                left = Skin.Padding;
                right = Skin.Padding;
                bottom = Skin.Padding;
                top = Skin.Padding;
                break;
        }

        var tableTop = top;

        if (cu?.Ausführbare_Skripte is { } aus && aus.Count > 0) {
            grpButtons.Left = left;
            grpButtons.Top = tableTop;
            grpButtons.Height = (btnAlleFilterAus.Top * 2) + btnAlleFilterAus.Height;
            grpButtons.Width = Width - left - right;

            tableTop = grpButtons.Bottom;

            grpButtons.Visible = true;
        } else {
            grpButtons.Visible = false;
        }

        // Filterleistenelemente positionieren
        if (filterRows > 0) {
            var firstRowY = 8; // Standard Y-CanvasPosition für erste Zeile
            // Hauptelemente (erste Zeile)
            txbZeilenFilter.Top = firstRowY;
            btnTextLöschen.Top = firstRowY;
            btnAlleFilterAus.Top = firstRowY;
            btnPin.Top = firstRowY;
            btnPinZurück.Top = firstRowY;
            btnViewManager.Top = firstRowY;

            grpFilter.Left = left;
            grpFilter.Top = tableTop;
            grpFilter.Height = (btnAlleFilterAus.Top * 2) + (filterRows * btnAlleFilterAus.Height) + ((filterRows - 1) * Skin.PaddingSmal);
            grpFilter.Width = Width - left - right;

            tableTop = grpFilter.Bottom;

            grpFilter.Visible = true;
        } else {
            grpFilter.Visible = false;
        }

        TableInternal.Top = tableTop;
        TableInternal.Height = Height - tableTop - bottom;
        TableInternal.Left = left;
        TableInternal.Width = Width - left - right;

        grpBorder.Text = Table?.Caption ?? "Keine Tabelle geladen";
    }

    private void TableInternal_CellClicked(object? sender, CellEventArgs e) => OnCellClicked(e);

    private void TableInternal_DoubleClick(object? sender, System.EventArgs e) => OnDoubleClick(e);

    /// <summary>
    /// Zentraler Handler für alle FilterCombined-Änderungen.
    /// Wird aufgerufen wenn Filter oder FilterFix sich ändern und FilterCombined neu berechnet wurde.
    /// </summary>
    private void TableInternal_FilterCombinedChanged(object? sender, System.EventArgs e) {
        DoFilterOutput();
        Invalidate_Controls();
    }

    private void TableInternal_PinnedChanged(object? sender, System.EventArgs e) => Invalidate_Controls();

    private void TableInternal_SelectedCellChanged(object? sender, CellExtEventArgs e) => OnSelectedCellChanged(e);

    private void TableInternal_SelectedRowChanged(object? sender, RowNullableEventArgs e) => OnSelectedRowChanged(e);

    private void TableInternal_TableChanged(object? sender, System.EventArgs e) {
        // Eigenen FilterFix an die neue Tabelle anpassen.
        // TableInternal.FilterFix wird durch das nachfolgende HandleChangesNow korrekt gesetzt.
        FilterFix.Table = Table;
        OnTableChanged();
    }

    private void TableInternal_ViewChanged(object? sender, System.EventArgs e) {
        Invalidate_ScriptButtons();
        Invalidate_Controls();
        OnViewChanged();
    }

    private void TableInternal_ViewLoading(object? sender, JsonEventArgs e) {
        // Geladene Fixfilter in das EIGENE FilterFix schreiben (nicht in TableInternal.FilterFix).
        // HandleChangesNow kombiniert FilterFix + FilterInput → TableInternal.FilterFix.
        // WICHTIG: ChangeTo statt Clear+Parse verwenden, da Parse kein PropertyChanged feuert.
        // Ohne PropertyChanged bleibt TableInternal.FilterFix leer und der Filter geht verloren.
        if (e.JsonData is not null && e.JsonData.GetJson("Filter") is not null && FilterFix is { IsDisposed: false }) {
            using var temp = new FilterCollection(Table, "TempLoad");
            temp.Parse(e.JsonData.GetString("Filter"));
            FilterFix.ChangeTo(temp);
        }

        OnViewLoading(e);
    }

    private void TableInternal_ViewSaving(object? sender, JsonEventArgs e) {
        // Eigenen Fixfilter speichern (nicht den kombinierten TableInternal.FilterFix).
        if (FilterFix is { IsDisposed: false } ff && ff.Count > 0) {
            e.JsonData.Add("Filter", ff.ParseableItems().FinishParseable());
        }

        OnViewSaving(e);
    }

    private void TableInternal_VisibleRowsChanged(object? sender, System.EventArgs e) => OnVisibleRowsChanged();

    private void txbZeilenFilter_Enter(object? sender, System.EventArgs e) => Filter_ZeilenFilterSetzen();

    private void txbZeilenFilter_TextChanged(object? sender, System.EventArgs e) {
        var neuerT = txbZeilenFilter.Text.TrimStart();
        btnTextLöschen.Enabled = !string.IsNullOrEmpty(neuerT);
        if (_doingControls) { return; }

        if (neuerT != txbZeilenFilter.Text) {
            txbZeilenFilter.Text = neuerT;
            return;
        }
        Filter_ZeilenFilterSetzen();
    }

    private void ViewManager_LoadView(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }

        var viewName = e.Item.KeyName;
        var savedViews = GetViews(tbf.KeyName);
        var entry = savedViews.Find(v => string.Equals(v.KeyName, viewName, StringComparison.OrdinalIgnoreCase));
        if (entry is null || entry.JsonData.ValueKind == JsonValueKind.Undefined) { return; }

        var viewObj = JsonSerializer.Deserialize<JsonObject>(entry.JsonData);
        SetView(viewObj);
        QuickNote.Show(NoteSymbols.Ok, "Geladen");
    }

    private void ViewManager_SaveView(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false }) { return; }

        var name = InputBox.Show("Ansicht speichern unter:", string.Empty, FormatHolder_SystemName.Instance);
        if (string.IsNullOrEmpty(name)) { return; }

        SaveCurrentView(name);
        QuickNote.Show(NoteSymbols.Ok, "Gespeichert");
    }

    private void ViewManager_SetStandardView(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }

        SaveCurrentView(ViewManager.Standard);
        ViewManager.SetAutoLoadLastView(tbf.KeyName, false);
        QuickNote.Show(NoteSymbols.Ok, "Gespeichert");
    }

    private void ViewManager_ToggleAutoLoad(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || Table is not TableFile { IsDisposed: false } tbf) { return; }

        var currentValue = ViewManager.GetAutoLoadLastView(tbf.KeyName);

        if (!currentValue && ViewManager.HasView(tbf.KeyName, ViewManager.Standard)) {
            if (Forms.MessageBox.Show("Auto-Laden aktivieren?\rDie Standard-Ansicht wird dabei entfernt.", ImageCode.Warnung, "Aktivieren", "Abbruch") != 0) { return; }
            ViewManager.DeleteView(tbf.KeyName, ViewManager.Standard);
        }

        ViewManager.SetAutoLoadLastView(tbf.KeyName, !currentValue);
        QuickNote.Show(NoteSymbols.Ok, !currentValue ? "Aktiviert" : "Deaktiviert");
    }

    #endregion
}