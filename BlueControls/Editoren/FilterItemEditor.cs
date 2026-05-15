// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;

namespace BlueTable.Editoren;

/// <summary>
/// Editor für ein FilterItem. Erlaubt die Auswahl von Spalte, Vergleichstyp und Suchwerten.
/// </summary>
public partial class FilterItemEditor : EditorEasy {

    #region Constructors

    public FilterItemEditor() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(FilterItem);

    #endregion

    #region Methods

    public override void Clear() {
        lstColumns.UncheckAll();
        cmbMethod.Text = string.Empty;
        cmbLogic.Text = string.Empty;
        chkIgnoreCase.Checked = true;
        txtSearchValue.Text = string.Empty;
    }

    protected override void InitializeComponentDefaultValues() {
        // Logik-Dropdown (Exakt, Und, Oder)
        cmbLogic.Items.Clear();
        cmbLogic.Items.Add("exakt");
        cmbLogic.Items.Add("und");
        cmbLogic.Items.Add("oder");

        // Methode (Istgleich, Instr)
        cmbMethod.Items.Clear();
        cmbMethod.Items.Add("Ist gleich");
        cmbMethod.Items.Add("Beinhaltet (Instr)");

        // Spaltenliste wird dynamisch über ToEdit (Table) befüllt,
        // falls der Editor initialisiert wird.
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not FilterItem fi) { return false; }

        // 1. Tabelle / Spalten laden
        if (fi.Table != null) {
            lstColumns.ItemClear();
            foreach (var col in fi.Table.Column) {
                lstColumns.ItemAdd(ItemOf(col.KeyName, col.ReadableText()));
            }
        }

        // 2. Werte setzen
        if (fi.Column != null) {
            lstColumns.Check(fi.Column.KeyName);
        }

        // Methode & Logik aus FilterType extrahieren
        chkIgnoreCase.Checked = fi.FilterType.HasFlag(FilterType.GroßKleinEgal);

        if (fi.FilterType.HasFlag(FilterType.Instr)) {
            cmbMethod.SelectedIndex = 1;
        } else {
            cmbMethod.SelectedIndex = 0;
        }

        if (fi.FilterType.HasFlag(FilterType.UND)) {
            cmbLogic.SelectedIndex = 1;
        } else if (fi.FilterType.HasFlag(FilterType.ODER)) {
            cmbLogic.SelectedIndex = 2;
        } else {
            cmbLogic.SelectedIndex = 0;
        }

        txtSearchValue.Text = string.Join("|", fi.SearchValue);

        return true;
    }

    private void Control_Changed(object sender, System.EventArgs e) {
        if (ToEdit is not FilterItem oldFi || oldFi.Table == null) { return; }

        var colKey = lstColumns.Checked.FirstOrDefault();
        var col = colKey != null ? oldFi.Table.Column[colKey] : null;

        // FilterType zusammenbauen
        FilterType ft = cmbMethod.SelectedIndex == 1 ? FilterType.Instr : FilterType.Istgleich;

        if (chkIgnoreCase.Checked) { ft |= FilterType.GroßKleinEgal; }

        if (cmbLogic.SelectedIndex == 1) {
            ft |= FilterType.UND;
        } else if (cmbLogic.SelectedIndex == 2) {
            ft |= FilterType.ODER;
        }

        var values = txtSearchValue.Text.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // Neues Objekt für ToEdit erstellen (löst Invalidate/Update aus)
        ToEdit = new FilterItem(oldFi.Table, col, ft, values, oldFi.Origin);
    }

    #endregion
}