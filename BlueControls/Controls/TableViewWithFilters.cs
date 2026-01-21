// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableViewWithFilters : GenericControlReciverSender, ITranslateable, IHasTable, IOpenScriptEditor, IStyleable {

    #region Fields

    /// <summary>
    ///  Abstand zwischen den Zeilen
    /// </summary>
    private const int RowSpacing = 4;

    private readonly object _lockUserAction = new();
    private ColumnViewCollection? _ähnliche;
    private bool _isFilling;
    private bool _isinSizeChanged;
    private string _lastLooked = string.Empty;

    #endregion

    #region Constructors

    public TableViewWithFilters() : base(false, false, false) {
        // (bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight)

        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        TableInternal.FilterCombined.PropertyChanged += FilterCombined_PropertyChanged;
        TableInternal.FilterCombinedChanged += TableInternal_FilterCombinedChanged;
        TableInternal.ContextMenuInit += TableInternal_ContextMenuInit;
        TableInternal.VisibleRowsChanged += TableInternal_VisibleRowsChanged;
        TableInternal.SelectedRowChanged += TableInternal_SelectedRowChanged;
        TableInternal.ViewChanged += TableInternal_ViewChanged;
        TableInternal.SelectedCellChanged += TableInternal_SelectedCellChanged;
        TableInternal.TableChanged += TableInternal_TableChanged;
        TableInternal.CellClicked += TableInternal_CellClicked;
        TableInternal.DoubleClick += TableInternal_DoubleClick;
        TableInternal.PinnedChanged += TableInternal_PinnedChanged;
        FilterFix.PropertyChanged += FilterFix_PropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<CellEventArgs>? CellClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public new event EventHandler<CellExtEventArgs>? DoubleClick;

    public event EventHandler<CellExtEventArgs>? SelectedCellChanged;

    public event EventHandler<RowNullableEventArgs>? SelectedRowChanged;

    public event EventHandler? TableChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler? VisibleRowsChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Wenn "Ähnliche" als Schaltfläche vorhanden sein soll, muss hier der Name einer Spaltenanordnung stehen
    /// </summary>
    [DefaultValue("")]
    public string ÄhnlicheAnsichtName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            GetÄhnlich();
        }
    } = string.Empty;

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

    [DefaultValue(false)]
    public bool EditButton {
        get => TableInternal.EditButton;
        set => TableInternal.EditButton = value;
    }

    /// <summary>
    /// Welche Knöpfe angezeigt werden sollen. Muss der Name einer Spaltenanordnung sein.
    /// </summary>
    [DefaultValue("")]
    public string FilterAnsichtName { get; set; } = string.Empty;

    public FilterCollection FilterCombined => TableInternal.FilterCombined;

    public FilterCollection FilterFix { get; } = new("FilterFix");

    public int FilterleisteZeilen => CurrentArrangement?.FilterRows ?? 1;

    [DefaultValue(FilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter)]
    public FilterTypesToShow FilterTypesToShow { get; set; } = FilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter;

    public new bool Focused => base.Focused || TableInternal.Focused;

    public List<RowItem>? PinnedRows => TableInternal.PinnedRows;

    public bool PowerEdit {
        set => TableInternal.PowerEdit = value;
    }

    public List<RowListItem> RowViewItems => TableInternal.RowViewItems;

    public string SheetStyle {
        get => TableInternal.SheetStyle;
        set => TableInternal.SheetStyle = value;
    }

    [DefaultValue(false)]
    public bool ShowNumber {
        get => TableInternal.ShowNumber;
        set => TableInternal.ShowNumber = value;
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
    /// Tabellen können mit TableSet gesetzt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Table? Table => TableInternal.Table;

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

    /// <summary>
    /// Berechnet die Höhe der Filterleiste in Pixeln.
    /// </summary>
    private int FilterleisteHeight {
        get {
            if (FilterleisteZeilen < 1) { return 0; }

            return (btnAlleFilterAus.Top * 2) + (FilterleisteZeilen * btnAlleFilterAus.Height) + ((FilterleisteZeilen - 1) * RowSpacing);
        }
    }

    #endregion

    #region Methods

    public void CollapesAll() => TableInternal.CollapesAll();

    public void CursorPos_Set(ColumnViewItem? columnViewItem, RowListItem? rowDataListItem, bool ensureVisible) => TableInternal.CursorPos_Set(columnViewItem, rowDataListItem, ensureVisible);

    public void DoZoom(bool zoomin) => TableInternal.DoZoom(zoomin);

    public void ExpandAll() => TableInternal.ExpandAll();

    public string Export_CSV(FirstRow columnCaption) => TableInternal.Export_CSV(columnCaption);

    public void Export_HTML(string filename, bool execute) => TableInternal.Export_HTML(filename, execute);

    public void Export_HTML() => TableInternal.Export_HTML();

    public void ImportBtb() => TableInternal.ImportBtb();

    public void ImportClipboard() => TableInternal.ImportClipboard();

    public void ImportCsv(string csvtxt) => TableInternal.ImportCsv(csvtxt);

    public string IsCellEditable(ColumnViewItem? cellInThisTableColumn, RowListItem? cellInThisTableRow, string? newChunkVal, bool maychangeview) => TableInternal.IsCellEditable(cellInThisTableColumn, cellInThisTableRow, newChunkVal, maychangeview);

    public void OpenScriptEditor() => TableInternal.OpenScriptEditor();

    public void OpenSearchAndReplaceInCells() => TableInternal.OpenSearchAndReplaceInCells();

    public void OpenSearchAndReplaceInTbScripts() => TableInternal.OpenSearchAndReplaceInTbScripts();

    public void OpenSearchInCells() => TableInternal.OpenSearchInCells();

    public void ParseView(string toParse) => TableInternal.ParseView(toParse);

    public void Pin(IReadOnlyList<RowItem>? rows) => TableInternal.Pin(rows);

    public void RowCleanUp() => TableInternal.RowCleanUp();

    public IReadOnlyList<RowItem> RowsVisibleUnique() => TableInternal.RowsVisibleUnique();

    public void TableSet(Table? tb, string viewCode) => TableInternal.TableSet(tb, viewCode);

    public ColumnViewItem? View_ColumnFirst() => TableInternal.View_ColumnFirst();

    public RowListItem? View_RowFirst() => TableInternal.View_RowFirst();

    public List<string> ViewToString() => TableInternal.ViewToString();

    //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                TableInternal.FilterCombined.PropertyChanged -= FilterCombined_PropertyChanged;
                TableInternal.FilterCombinedChanged -= TableInternal_FilterCombinedChanged;
                TableInternal.ContextMenuInit -= TableInternal_ContextMenuInit;
                TableInternal.VisibleRowsChanged -= TableInternal_VisibleRowsChanged;
                TableInternal.SelectedRowChanged -= TableInternal_SelectedRowChanged;
                TableInternal.ViewChanged -= TableInternal_ViewChanged;
                TableInternal.SelectedCellChanged -= TableInternal_SelectedCellChanged;
                TableInternal.TableChanged -= TableInternal_TableChanged;
                TableInternal.PinnedChanged -= TableInternal_PinnedChanged;
                TableInternal.DoubleClick -= TableInternal_DoubleClick;
                TableInternal.CellClicked -= TableInternal_CellClicked;

                _ähnliche?.Dispose();
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
        Skin.Draw_Back(gr, Design.GroupBox, States.Standard, base.DisplayRectangle, this, true);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        if (Table is not { IsDisposed: false } tb) { return; }

        DoInputFilter(FilterOutput.Table, false);

        using var nfc = new FilterCollection(tb, "TmpFilterCombined");
        nfc.RemoveOtherAndAdd(FilterFix, "Filter aus übergeordneten Element");
        nfc.RemoveOtherAndAdd(FilterInput, null);
        TableInternal.FilterFix.ChangeTo(nfc);
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);

        // Status der Steuerelemente aktualisieren
        var hasDb = Table != null;
        var tableEnabled = Enabled;

        txbZeilenFilter.Enabled = hasDb && LanguageTool.Translation == null && Enabled && tableEnabled;
        btnAlleFilterAus.Enabled = hasDb && Enabled && tableEnabled;
        btnPin.Enabled = hasDb && Enabled && tableEnabled;
        btnPinZurück.Enabled = hasDb && TableInternal.PinnedRows.Count > 0 && Enabled && tableEnabled;

        // Filterleisten-Initialisierung
        btnTextLöschen.Enabled = false;
        btnÄhnliche.Visible = false;
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);

        DoFilterOutput();

        // Anfängliche Positionierung der Steuerelemente
        UpdateFilterleisteVisibility();

        GetÄhnlich();
        DoFilterAndPinButtons();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        lock (_lockUserAction) {
            if (_isinSizeChanged) { return; }
            _isinSizeChanged = true;

            RepositionControls();

            DoFilterAndPinButtons();

            _isinSizeChanged = false;
        }
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        OnTableChanged();
        base.OnVisibleChanged(e);
    }

    private void _Table_ViewChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        DoFilterAndPinButtons();
    }

    private void btnÄhnliche_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (tb.Column.First is not { IsDisposed: false } co) { return; }

        var fl = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);

        using var fc = new FilterCollection(fl, "ähnliche");

        var r = fc.Rows;
        if (r.Count != 1 || _ähnliche is not { Count: not 0 }) {
            Forms.MessageBox.Show("Aktion fehlgeschlagen", ImageCode.Information, "OK");
            return;
        }

        btnAlleFilterAus_Click(null, System.EventArgs.Empty);
        foreach (var thiscolumnitem in _ähnliche) {
            if (thiscolumnitem?.Column != null && TableInternal.FilterCombined != null) {
                if (thiscolumnitem.AutoFilterSymbolPossible) {
                    if (string.IsNullOrEmpty(r[0].CellGetString(thiscolumnitem.Column))) {
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, string.Empty);
                        TableInternal.Filter.Add(fi);
                    } else if (thiscolumnitem.Column.MultiLine) {
                        var l = r[0].CellGetList(thiscolumnitem.Column).SortedDistinctList();
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, l);
                        TableInternal.Filter.Add(fi);
                    } else {
                        var l = r[0].CellGetString(thiscolumnitem.Column);
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, l);
                        TableInternal.Filter.Add(fi);
                    }
                }
            }
        }

        btnÄhnliche.Enabled = false;
    }

    private void btnAlleFilterAus_Click(object? sender, System.EventArgs e) {
        _lastLooked = string.Empty;
        TableInternal.Filter.Clear();
    }

    private void btnPin_Click(object sender, System.EventArgs e) => TableInternal.Pin(RowsVisibleUnique());

    private void btnPinZurück_Click(object sender, System.EventArgs e) {
        _lastLooked = string.Empty;
        TableInternal.Pin(null);
    }

    private void btnTextLöschen_Click(object sender, System.EventArgs e) => txbZeilenFilter.Text = string.Empty;

    private void DoÄhnlich() {
        if (IsDisposed || Table is not { IsDisposed: false } tb || tb.Column.Count == 0) { return; }

        if (tb.Column.First is not { } colFirst) { return; } // Neue Tabelle?

        var fi = new FilterItem(colFirst, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);
        using var fc = new FilterCollection(fi, "doähnliche");

        var r = fc.Rows;
        if (_ähnliche != null) {
            btnÄhnliche.Visible = true;
            btnÄhnliche.Enabled = r.Count == 1;
        } else {
            btnÄhnliche.Visible = false;
        }

        if (AutoPin && r.Count == 1) {
            if (_lastLooked != r[0].KeyName) {
                if (RowViewItems.GetByKey(r[0].KeyName) == null) {
                    if (Forms.MessageBox.Show("Die Zeile wird durch Filterungen <b>ausgeblendet</b>.<br>Soll sie zusätzlich <b>angepinnt</b> werden?", ImageCode.Pinnadel, "Ja", "Nein") == 0) {
                        TableInternal.PinAdd(r[0]);
                    }
                    _lastLooked = r[0].KeyName;
                }
            }
        }
    }

    private void DoFilterAndPinButtons() {
        if (IsDisposed || FilterleisteZeilen <= 0) { return; }

        if (InvokeRequired) {
            Invoke(new Action(DoFilterAndPinButtons));
            return;
        }

        if (_isFilling) { return; }
        _isFilling = true;

        btnPinZurück.Enabled = Table is not null && TableInternal.PinnedRows.Count > 0;

        #region ZeilenFilter befüllen

        txbZeilenFilter.Text = Table != null && TableInternal.Filter.IsRowFilterActiv()
                                ? TableInternal.Filter.RowFilterText
                                : string.Empty;

        #endregion

        var consthe = btnAlleFilterAus.Height;

        #region Variablen für Waagerecht / Senkrecht bestimmen

        // Verfügbare Zeilen berechnen
        var availableRows = FilterleisteZeilen;

        // Startposition für die erste Zeile
        var toppos = btnAlleFilterAus.Top;
        var beginnx = btnPinZurück.Right + (Skin.Padding * 3);
        var leftpos = beginnx;
        var constwi = (int)(txbZeilenFilter.Width * 1.5);
        var right = constwi + Skin.PaddingSmal;
        const AnchorStyles anchor = AnchorStyles.Top | AnchorStyles.Left;

        #endregion

        List<FlexiControlForFilter> flexsToDelete = [];

        #region Vorhandene Flexis ermitteln

        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
        }

        #endregion

        var cu = CurrentArrangement;

        #region Neue Flexis erstellen / updaten

        if (Table is { IsDisposed: false } tb) {
            var tcvc = ColumnViewCollection.ParseAll(tb);
            List<ColumnItem> columSort = [];
            var orderArrangement = tcvc.GetByKey(FilterAnsichtName);

            #region Reihenfolge der Spalten bestimmen

            if (orderArrangement != null) {
                foreach (var thisclsVitem in orderArrangement) {
                    if (thisclsVitem?.Column is { IsDisposed: false } ci) { columSort.AddIfNotExists(ci); }
                }
            }

            if (cu != null) {
                foreach (var thisclsVitem in cu) {
                    if (thisclsVitem?.Column is { IsDisposed: false } ci) { columSort.AddIfNotExists(ci); }
                }
            }

            foreach (var thisColumn in Table.Column) {
                columSort.AddIfNotExists(thisColumn);
            }

            #endregion

            var currentRow = 1; // Die erste Zeile ist bereits belegt mit den Hauptsteuerelementen
                                // var count = 0;
            var itemsInCurrentRow = 0;

            foreach (var thisColumn in columSort) {
                var showMe = false;
                if (thisColumn.Table is { IsDisposed: false }) {
                    var viewItemOrder = orderArrangement?[thisColumn];
                    var viewItemCurrent = cu?[thisColumn];
                    var filterItem = TableInternal.FilterCombined[thisColumn];

                    #region Sichtbarkeit des Filterelements bestimmen

                    if (thisColumn.AutoFilterSymbolPossible()) {
                        if (viewItemOrder != null && FilterTypesToShow.HasFlag(FilterTypesToShow.NachDefinierterAnsicht)) { showMe = true; }
                        if (viewItemCurrent != null && FilterTypesToShow.HasFlag(FilterTypesToShow.AktuelleAnsicht_AktiveFilter) && filterItem != null) { showMe = true; }

                        if (FilterInput?[thisColumn] is { }) { showMe = true; }
                    }

                    #endregion

                    if (showMe && currentRow <= availableRows) {
                        var flx = FlexiItemOf(thisColumn);
                        if (flx != null) {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            flexsToDelete.Remove(flx);
                        } else {
                            // Na gut, eben neuen Flex erstellen
                            flx = new FlexiControlForFilter(thisColumn, CaptionPosition.Links_neben_dem_Feld, FlexiFilterDefaultOutput.Alles_Anzeigen, FlexiFilterDefaultFilter.Textteil, true, false);
                            flx.FilterOutput.Table = thisColumn.Table;
                            //flx.Standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
                            //flx.Filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
                            ChildIsBorn(flx);
                            flx.FilterOutputPropertyChanged += FlexSingeFilter_FilterOutputPropertyChanged;
                            Controls.Add(flx);
                        }

                        // Prüfen, ob wir in eine neue Zeile wechseln müssen
                        if (leftpos + constwi > Width && itemsInCurrentRow > 0) {
                            leftpos = beginnx;
                            toppos = btnAlleFilterAus.Top + (currentRow * (consthe + RowSpacing));
                            currentRow++;
                            itemsInCurrentRow = 0;

                            // Prüfen, ob wir noch in den verfügbaren Zeilen sind
                            if (currentRow >= availableRows) {
                                flexsToDelete.AddIfNotExists(flx);
                                break;
                            }
                        }

                        flx.Top = toppos;
                        flx.Left = leftpos;
                        flx.Width = constwi;
                        flx.Height = consthe;
                        flx.Anchor = anchor;
                        leftpos += right;
                        itemsInCurrentRow++;
                    }
                }
            }
        }

        #endregion

        #region Unnötige Flexis löschen

        foreach (var thisFlexi in flexsToDelete) {
            thisFlexi.FilterOutputPropertyChanged -= FlexSingeFilter_FilterOutputPropertyChanged;
            thisFlexi.Visible = false;
            Controls.Remove(thisFlexi);
            thisFlexi.Dispose();
        }

        #endregion

        _isFilling = false;
    }

    private void DoFilterOutput() {
        if (!FilterInputChangedHandled) {
            HandleChangesNow();
            return;
        }

        if (CursorPosRow?.Row is { IsDisposed: false } setedrow) {
            using var nfc = new FilterCollection(setedrow, "Temp TableOutput");
            nfc.RemoveOtherAndAdd(TableInternal.FilterCombined, null);
            if (!FilterOutput.IsDifferentTo(nfc)) { return; }
            FilterOutput.ChangeTo(nfc);
        } else {
            if (!FilterOutput.IsDifferentTo(TableInternal.FilterCombined)) { return; }
            FilterOutput.ChangeTo(TableInternal.FilterCombined);
        }

        DoFilterAndPinButtons();
    }

    private void Filter_ZeilenFilterSetzen() {
        if (IsDisposed || (Table?.IsDisposed ?? true)) {
            DoÄhnlich();
            return;
        }

        var currentFilter = TableInternal.Filter.RowFilterText;
        var newFilter = txbZeilenFilter.Text;

        if (string.Equals(currentFilter, newFilter, StringComparison.OrdinalIgnoreCase)) { return; }

        if (string.IsNullOrEmpty(newFilter)) {
            TableInternal.Filter.Remove_RowFilter();
            DoÄhnlich();
            return;
        }

        TableInternal.Filter.RowFilterText = newFilter;
        DoÄhnlich();
    }

    private void FilterCombined_PropertyChanged(object sender, PropertyChangedEventArgs e) => DoFilterOutput();

    private void FilterFix_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate_FilterInput();

    private FlexiControlForFilter? FlexiItemOf(ColumnItem column) {
        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) {
                if (flx.FilterSingleColumn == column) { return flx; }
            }
        }
        return null;
    }

    private void FlexSingeFilter_FilterOutputPropertyChanged(object sender, System.EventArgs e) {
        if (sender is not FlexiControlForFilter ffc) { return; }

        if (ffc.FilterOutput is not { } fc) { return; }

        var fi = fc[ffc.FilterSingleColumn];

        if (fi == null) {
            TableInternal.Filter.Remove(ffc.FilterSingleColumn);
        } else {
            if(!string.IsNullOrEmpty(fi.Origin)) { return; }
            // Besser wäre FilterInput[ffc.FilterSingleColumn].Equals(fi)
            // Aber das Origin stimmt nicht

            TableInternal.Filter.RemoveOtherAndAdd(fi);
        }

        //DoFilterOutput();
    }

    private void GetÄhnlich() {
        if (IsDisposed || FilterleisteZeilen <= 0) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        _ähnliche = tcvc.GetByKey(ÄhnlicheAnsichtName);
        DoÄhnlich();
    }

    private void OnCellClicked(CellEventArgs e) => CellClicked?.Invoke(this, e);

    private void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    private void OnDoubleClick(CellExtEventArgs e) => DoubleClick?.Invoke(this, e);

    private void OnSelectedCellChanged(CellExtEventArgs e) => SelectedCellChanged?.Invoke(this, e);

    private void OnSelectedRowChanged(RowNullableEventArgs e) {
        DoFilterOutput();
        SelectedRowChanged?.Invoke(this, e);
    }

    private void OnTableChanged() {
        Invalidate_FilterInput();
        Invalidate_RowsInput();

        GetÄhnlich();
        DoFilterAndPinButtons();
        UpdateFilterleisteVisibility();
        RepositionControls();

        TableChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnViewChanged() => ViewChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Positioniert die Steuerelemente in der Table neu, um Platz für die Filterleiste zu schaffen.
    /// </summary>
    private void RepositionControls() {
        if (InvokeRequired) {
            Invoke(new Action(RepositionControls));
            return;
        }

        // Filterleistenelemente positionieren
        if (FilterleisteZeilen > 0) {
            var firstRowY = 8; // Standard Y-CanvasPosition für erste Zeile

            // Hauptelemente (erste Zeile)
            txbZeilenFilter.Top = firstRowY;
            btnTextLöschen.Top = firstRowY;
            btnAlleFilterAus.Top = firstRowY;
            btnPin.Top = firstRowY;
            btnPinZurück.Top = firstRowY;

            // Elemente der zweiten Zeile wenn nötig
            if (FilterleisteZeilen > 1) {
                btnÄhnliche.Top = firstRowY + 32; // 32 = Höhe der ersten Reihe + Abstand
            }
        }

        var h = FilterleisteHeight;
        TableInternal.Top = h;
        TableInternal.Height = Height - h;
        TableInternal.Left = 0;
        TableInternal.Width = Width;

        // Bei kompletter Neupositionierung auch die FlexiFilterControls anpassen
        DoFilterAndPinButtons();
    }

    private void TableInternal_CellClicked(object sender, CellEventArgs e) => OnCellClicked(e);

    private void TableInternal_ContextMenuInit(object sender, ContextMenuInitEventArgs e) => OnContextMenuInit(e);

    private void TableInternal_DoubleClick(object sender, CellExtEventArgs e) => OnDoubleClick(e);

    private void TableInternal_FilterCombinedChanged(object sender, System.EventArgs e) => DoFilterOutput();

    private void TableInternal_PinnedChanged(object sender, System.EventArgs e) {
        DoFilterAndPinButtons();
        Invalidate();
    }

    private void TableInternal_SelectedCellChanged(object sender, CellExtEventArgs e) => OnSelectedCellChanged(e);

    private void TableInternal_SelectedRowChanged(object sender, RowNullableEventArgs e) => OnSelectedRowChanged(e);

    private void TableInternal_TableChanged(object sender, System.EventArgs e) {
        FilterFix.Table = Table;
        OnTableChanged();
    }

    private void TableInternal_ViewChanged(object sender, System.EventArgs e) => OnViewChanged();

    private void TableInternal_VisibleRowsChanged(object sender, System.EventArgs e) => OnVisibleRowsChanged();

    private void txbZeilenFilter_Enter(object sender, System.EventArgs e) => Filter_ZeilenFilterSetzen();

    private void txbZeilenFilter_TextChanged(object sender, System.EventArgs e) {
        var neuerT = txbZeilenFilter.Text.TrimStart();
        btnTextLöschen.Enabled = !string.IsNullOrEmpty(neuerT);
        if (_isFilling) { return; }

        if (neuerT != txbZeilenFilter.Text) {
            txbZeilenFilter.Text = neuerT;
            return;
        }
        Filter_ZeilenFilterSetzen();
        DoÄhnlich();
    }

    private void UpdateFilterleisteVisibility() {
        if (InvokeRequired) {
            Invoke(new Action(UpdateFilterleisteVisibility));
            return;
        }

        var visible = FilterleisteZeilen > 0;

        // Hauptsteuerelemente der Filterleiste
        btnTextLöschen.Visible = visible;
        txbZeilenFilter.Visible = visible;
        btnAlleFilterAus.Visible = visible;
        btnPin.Visible = visible;
        btnPinZurück.Visible = visible;

        if (visible) {
            // Status der Steuerelemente aktualisieren
            btnPinZurück.Enabled = Table is not null && TableInternal.PinnedRows.Count > 0;
            txbZeilenFilter.Enabled = Table != null && LanguageTool.Translation == null && Enabled;
            btnAlleFilterAus.Enabled = Table != null && Enabled;
            btnPin.Enabled = Table != null && Enabled;

            // Text im ZeilenFilter aktualisieren
            if (Table != null && TableInternal.Filter.IsRowFilterActiv()) {
                txbZeilenFilter.Text = TableInternal.Filter.RowFilterText;
            } else {
                txbZeilenFilter.Text = string.Empty;
            }

            // Status des Löschen-Buttons aktualisieren
            btnTextLöschen.Enabled = !string.IsNullOrEmpty(txbZeilenFilter.Text);
        }

        // btnÄhnliche wird separat gesteuert über GetÄhnlich()
        if (!visible) {
            btnÄhnliche.Visible = false;
        } else {
            GetÄhnlich(); // Status von btnÄhnliche aktualisieren
        }

        // Alle FlexiFilterControls ein-/ausblenden
        foreach (var control in Controls) {
            if (control is FlexiControlForFilter flx) {
                flx.Visible = visible;
            }
        }
    }

    #endregion
}