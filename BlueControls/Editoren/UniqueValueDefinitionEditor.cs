// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class UniqueValueDefinitionEditor : EditorEasy, IHasTable {

    #region Constructors

    public UniqueValueDefinitionEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(UniqueValueDefinition);

    public Table? Table => ToEdit is UniqueValueDefinition u ? u.Table : null;

    #endregion

    #region Methods

    public override void Clear() => lbxKeyColumns.ItemClear();

    protected override void InitializeComponentDefaultValues() { }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        if (toEdit is not UniqueValueDefinition { } uvd) { return false; }

        lbxKeyColumns.ItemAddRange(ItemsOf(uvd.Table.Column, true));

        foreach (var thisColumn in uvd.KeyColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxKeyColumns.Check(thisColumn.KeyName);
            }
        }

        return true;
    }

    private void DoNewDefinition() {
        if (Table is not { } tb) { return; }

        var colnam = lbxKeyColumns.CheckedItems.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        var nr = new UniqueValueDefinition(tb, colnam);

        if (ToEdit?.Equals(nr) ?? false) { return; }

        ToEdit = nr;
    }

    private void lbxKeyColumns_ItemCheckedChanged(object sender, System.EventArgs e) => DoNewDefinition();

    #endregion
}
