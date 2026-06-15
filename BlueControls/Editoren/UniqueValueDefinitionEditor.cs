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
    public override EditorMode SupportedModes => EditorMode.EditCopy | EditorMode.EditItem;
    public Table? Table => InputItem is UniqueValueDefinition u ? u.Table : null;

    #endregion

    #region Methods

    public override void Clear() {
        lbxKeyColumns.ItemClear();
    }

    public override object? CreateNewItem() {
        if (Table is not { IsDisposed: false } tb) { return null; }

        var colnam = lbxKeyColumns.CheckedItems.Select(thisk => ((ReadableListItem)thisk).Item as ColumnItem).Where(c => c is not null).Cast<ColumnItem>().ToList();
        return new UniqueValueDefinition(tb, colnam);
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        lbxKeyColumns.CheckBehavior = enabled ? CheckBehavior.MultiSelection : CheckBehavior.AllSelected;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not UniqueValueDefinition { } uvd) { return false; }

        lbxKeyColumns.ItemAddRange(ItemsOf(uvd.Table.Column.Where(c => c.RelationType == RelationType.None), true));

        foreach (var thisColumn in uvd.KeyColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxKeyColumns.Check(thisColumn.KeyName);
            }
        }

        return true;
    }

    #endregion
}