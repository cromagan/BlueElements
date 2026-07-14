// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueTable.Editoren;

/// <summary>
/// Editor für ein FilterItem. Erlaubt die Auswahl von Spalte, Vergleichstyp und Suchwerten.
/// </summary>
public partial class FilterItemEditor : EditorEasy, IHasTable {

    #region Constructors

    public FilterItemEditor() {
        InitializeComponent();
        txtSearchValue.Suggestions = new List<string> { "~USER~", "~USERGROUP~" }.AsReadOnly();
        txtSearchValue.TextChanged += TxtSearchValue_TextChanged;
        UpdateLogicVisibility();
    }

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(FilterItem);
    public override EditorMode SupportedModes => EditorMode.EditCopy | EditorMode.EditItem;
    public Table? Table { get; private set; }

    #endregion

    #region Methods

    public override void Clear() {
        Table = null;
        lstColumns.ItemClear();
        lstColumns.Suggestions.Clear();
        cmbMethod.Text = string.Empty;
        cmbLogic.Text = string.Empty;
        chkIgnoreCase.Checked = true;
        txtSearchValue.Text = string.Empty;
    }

    public override object? CreateNewItem() {
        if (Table is not { IsDisposed: false } tb) { return null; }

        var colItem = lstColumns.CheckedItems
            .OfType<ReadableListItem>()
            .Select(thisk => thisk.Item)
            .OfType<ColumnItem>()
            .FirstOrDefault();

        FilterType ft = cmbMethod.Text == "Instr" ? FilterType.Instr : FilterType.Istgleich;

        if (chkIgnoreCase.Checked) { ft |= FilterType.GroßKleinEgal; }

        if (cmbLogic.Visible) {
            if (cmbLogic.Text == "UND") {
                ft |= FilterType.UND;
            } else if (cmbLogic.Text == "ODER") {
                ft |= FilterType.ODER;
            }
        }

        var values = txtSearchValue.Text.SplitByCr().Where(s => s.Length > 0).Select(v => v.FromNonCritical()).ToList();

        var origin = InputItem is FilterItem oldFi ? oldFi.Origin : string.Empty;
        return new FilterItem(tb, colItem, ft, values, origin);
    }

    protected override void InitializeComponentDefaultValues() {
        cmbLogic.ItemClear();
        cmbLogic.ItemAdd(ItemOf("einzeln", "Einzeln"));
        cmbLogic.ItemAdd(ItemOf("und", "UND"));
        cmbLogic.ItemAdd(ItemOf("oder", "ODER"));

        cmbMethod.ItemClear();
        cmbMethod.ItemAdd(ItemOf("Ist gleich", "Istgleich"));
        cmbMethod.ItemAdd(ItemOf("Beinhaltet (Instr)", "Instr"));
    }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        lstColumns.Enabled = enabled;
        cmbMethod.Enabled = enabled;
        cmbLogic.Enabled = enabled;
        chkIgnoreCase.Enabled = enabled;
        txtSearchValue.Enabled = enabled;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not FilterItem fi) { return false; }

        Table = fi.Table;

        if (Table is { IsDisposed: false } tb) {
            lstColumns.ItemAddRange(ItemsOf(tb.Column));

            if (fi.Column is { IsDisposed: false }) {
                lstColumns.Check(ItemOf(fi.Column));
            }
        }

        chkIgnoreCase.Checked = fi.FilterType.HasFlag(FilterType.GroßKleinEgal);

        cmbMethod.Text = fi.FilterType.HasFlag(FilterType.Instr) ? "Instr" : "Istgleich";

        if (fi.FilterType.HasFlag(FilterType.UND)) {
            cmbLogic.Text = "UND";
        } else if (fi.FilterType.HasFlag(FilterType.ODER)) {
            cmbLogic.Text = "ODER";
        } else {
            cmbLogic.Text = "Einzeln";
        }

        txtSearchValue.Text = string.Join("\r\n", fi.SearchValue.Select(v => v.ToNonCritical()));

        return true;
    }

    private void TxtSearchValue_TextChanged(object? sender, System.EventArgs e) {
        UpdateLogicVisibility();
    }

    private void UpdateLogicVisibility() {
        cmbLogic.Visible = txtSearchValue.Text.Contains("\r\n");
    }

    #endregion
}