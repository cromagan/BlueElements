// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class RowSortDefinitionEditor : EditorEasy, IHasTable {

    #region Constructors

    public RowSortDefinitionEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(RowSortDefinition);

    public Table? Table => ToEdit is RowSortDefinition r ? r.Table : null;

    #endregion

    #region Methods

    public override void Clear() {
        lbxSortierSpalten.ItemClear();
        lbxSortierSpalten.Suggestions.Clear();
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not RowSortDefinition { } rsd) { return false; }

        btnSortRichtung.Checked = rsd.Reverse;

        foreach (var thisColumn in rsd.UsedColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxSortierSpalten.AddAndCheck(ItemOf(thisColumn));
            }
        }

        lbxSortierSpalten.Suggestions.AddRange(ItemsOf(rsd.Table.Column, true));

        return true;
    }

    private void btnSortRichtung_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) => DoNewSort();

    private void DoNewSort() {
        if (Table is not { } tb) { return; }

        var colnam = lbxSortierSpalten.Items.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        var nr = new RowSortDefinition(tb, colnam, btnSortRichtung.Checked);

        if (ToEdit?.Equals(nr) ?? false) { return; }

        ToEdit = nr;
    }

    private void lbxSortierSpalten_ItemAddedByClick(object sender, EventArgs.AbstractListItemEventArgs e) => DoNewSort();

    private void lbxSortierSpalten_RemoveClicked(object sender, EventArgs.AbstractListItemEventArgs e) => DoNewSort();

    private void lbxSortierSpalten_UpDownClicked(object sender, System.EventArgs e) => DoNewSort();

    #endregion
}