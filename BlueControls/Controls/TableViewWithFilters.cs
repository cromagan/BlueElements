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
using System.Linq;
using System.Windows.Forms;

using BlueBasics;

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;

using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;

using BlueControls.Extended_Text;
using BlueControls.Forms;

using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;

using BlueScript.Structures;

using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueTable.Table;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class TableViewWithFilters : GenericControlReciverSender, ITranslateable, IHasTable, IStyleable {

    #region Fields

    private bool _controlsCorrect = false;
    private bool _doingControls = false;
    private bool _scriptButtonsCorrect = false;

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

    public FilterCollection FilterCombined => TableInternal.FilterCombined;

    public FilterCollection FilterFix { get; } = new("FilterFix");

    public new bool Focused => base.Focused || TableInternal.Focused;

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
        set => TableInternal.PowerEdit = value;
    }

    public List<RowListItem> RowViewItems => TableInternal.RowViewItems;

    [DefaultValue(Constants.Win11)]
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

    protected override void OnVisibleChanged(System.EventArgs e) {
        OnTableChanged();
        base.OnVisibleChanged(e);
    }

    private void B_Click(object sender, System.EventArgs e) {
        if (sender is not Button b ||
            b.Tag is not string keyn ||
            TableInternal.Table is not { IsDisposed: false } tb ||
            tb.EventScript.GetByKey(keyn) is not { } script ||
            !script.IsOk()) {
            BlueControls.Forms.MessageBox.Show("Abbruch, interner Fehler");
            return;
        }
        TableView.ContextMenu_ExecuteScript(this, new ObjectEventArgs(new { Script = script, Rows = (Func<IReadOnlyList<RowItem>>)RowsVisibleUnique }));
    }

    private void btnAlleFilterAus_Click(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        TableInternal.Filter.Clear();
    }

    private void btnPin_Click(object sender, System.EventArgs e) => TableInternal.Pin(RowsVisibleUnique());

    private void btnPinZurück_Click(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        TableInternal.Pin(null);
    }

    private void btnTextLöschen_Click(object sender, System.EventArgs e) => txbZeilenFilter.Text = string.Empty;

    private void CheckButtons() {
        if (!grpFilter.Visible) { return; }

        var hasTB = Table != null && Enabled;

        // Status der Steuerelemente aktualisieren
        btnPinZurück.Enabled = hasTB && TableInternal.PinnedRows.Count > 0;
        txbZeilenFilter.Enabled = hasTB && LanguageTool.Translation == null;
        btnAlleFilterAus.Enabled = hasTB;
        btnPin.Enabled = hasTB;

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

        RepositionControls();
        CheckButtons();
        DoFilterAndPinButtons();
        DoScriptButtons();

        _doingControls = false;
        _controlsCorrect = true;
    }

    private void DoFilterAndPinButtons() {
        if (IsDisposed) { return; }
        if (!grpFilter.Visible) { return; }

        var cu = CurrentArrangement;

        #region Pin Button

        btnPinZurück.Enabled = Table is not null && TableInternal.PinnedRows.Count > 0;

        #endregion

        #region ZeilenFilter befüllen

        txbZeilenFilter.Text = Table != null && TableInternal.Filter.IsRowFilterActiv()
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

            List<ColumnItem> columSort = [];
            if (cu?.Filter_immer_Anzeigen is { } orderArrangement) {
                foreach (var thisColumn in orderArrangement) {
                    if (tb.Column[thisColumn] is { IsDisposed: false } ci) { columSort.AddIfNotExists(ci); }
                }
            }

            foreach (var thisColumn in TableInternal.FilterCombined) {
                if (thisColumn.Column is { } col) { columSort.AddIfNotExists(col); }
            }

            #endregion

            #region Standard-Variablen ermitteln

            var filterHeight = btnAlleFilterAus.Height;
            var filterWidth = (int)(txbZeilenFilter.Width * 1.5);
            var startPositionX = btnPinZurück.Right + (Skin.Padding * 3);
            var addX = filterWidth + Skin.PaddingSmal;
            var addY = filterHeight + Skin.PaddingSmal;
            var toppos = btnAlleFilterAus.Top - addY;
            var leftpos = grpFilter.Width;

            #endregion

            foreach (var thisColumn in columSort) {
                if (thisColumn.AutoFilterSymbolPossible()) {

                    #region Position berechnen und evtl. Schleife verlassen

                    leftpos += addX;

                    // Prüfen, ob wir in eine neue Zeile wechseln müssen
                    if (leftpos + filterWidth > grpFilter.Width) {
                        leftpos = startPositionX;
                        toppos += addY;
                        if (toppos + filterHeight > grpFilter.Height || startPositionX + filterWidth > grpFilter.Width) { break; }
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

                    if (flx == null) {
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
                    flx.Width = filterWidth;
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

        //Invalidate_Controls();
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

        foreach (var thisString in ca.Ausführbare_Skripte) {
            if (tb.EventScript.GetByKey(thisString) is { } thiss) {
                var b = new Button();

                b.Enabled = thiss is { UserGroups.Count: > 0 } && tb.PermissionCheck(thiss.UserGroups, null) && thiss.IsOk();
                b.Visible = true;
                b.Text = thiss.ReadableText();
                b.ImageCode = thiss.SymbolForReadableText().ToString();
                b.QuickInfo = thiss.QuickInfo;
                b.Click += B_Click;
                grpButtons.Controls.Add(b);
                b.Width = txbZeilenFilter.Width;
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

    private void FilterCombined_PropertyChanged(object sender, PropertyChangedEventArgs e) => DoFilterOutput();

    private void FilterFix_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        Invalidate_FilterInput();
        Invalidate_Controls();
    }

    private void FlexSingeFilter_FilterOutputPropertyChanged(object sender, System.EventArgs e) {
        if (sender is not FlexiControlForFilter ffc) { return; }

        if (ffc.FilterOutput is not { } fc) { return; }

        var fi = fc[ffc.FilterSingleColumn];

        if (fi == null) {
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
        Invalidate_ScriptButtons();
        Invalidate_Controls();

        TableChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnViewChanged() => ViewChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnVisibleRowsChanged() => VisibleRowsChanged?.Invoke(this, System.EventArgs.Empty);

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

    private void TableInternal_CellClicked(object sender, CellEventArgs e) => OnCellClicked(e);

    private void TableInternal_ContextMenuInit(object sender, ContextMenuInitEventArgs e) => OnContextMenuInit(e);

    private void TableInternal_DoubleClick(object sender, CellExtEventArgs e) => OnDoubleClick(e);

    private void TableInternal_FilterCombinedChanged(object sender, System.EventArgs e) {
        DoFilterOutput();
        Invalidate_Controls();
    }

    private void TableInternal_PinnedChanged(object sender, System.EventArgs e) {
        Invalidate_Controls();
    }

    private void TableInternal_SelectedCellChanged(object sender, CellExtEventArgs e) => OnSelectedCellChanged(e);

    private void TableInternal_SelectedRowChanged(object sender, RowNullableEventArgs e) => OnSelectedRowChanged(e);

    private void TableInternal_TableChanged(object sender, System.EventArgs e) {
        FilterFix.Table = Table;
        OnTableChanged();
    }

    private void TableInternal_ViewChanged(object sender, System.EventArgs e) {
        Invalidate_ScriptButtons();
        Invalidate_Controls();
        OnViewChanged();
    }

    private void TableInternal_VisibleRowsChanged(object sender, System.EventArgs e) => OnVisibleRowsChanged();

    private void txbZeilenFilter_Enter(object sender, System.EventArgs e) => Filter_ZeilenFilterSetzen();

    private void txbZeilenFilter_TextChanged(object sender, System.EventArgs e) {
        var neuerT = txbZeilenFilter.Text.TrimStart();
        btnTextLöschen.Enabled = !string.IsNullOrEmpty(neuerT);
        if (_doingControls) { return; }

        if (neuerT != txbZeilenFilter.Text) {
            txbZeilenFilter.Text = neuerT;
            return;
        }
        Filter_ZeilenFilterSetzen();
    }

    #endregion
}