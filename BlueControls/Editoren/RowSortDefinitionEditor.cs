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

    public override EditorMode SupportedModes => EditorMode.EditCopy;

    public Table? Table => InputItem is RowSortDefinition r ? r.Table : null;

    #endregion

    #region Methods

    public override void Clear() {
        lbxSortierSpalten.ItemClear();
        lbxSortierSpalten.Suggestions.Clear();
    }

    public override object? CreateNewItem() {
        if (Table is not { } tb) { return null; }

        var colnam = lbxSortierSpalten.Items
            .OfType<ReadableListItem>()
            .Select(thisk => thisk.Item)
            .OfType<ColumnItem>()
            .ToList();
        return new RowSortDefinition(tb, colnam, btnSortRichtung.Checked);
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        btnSortRichtung.Enabled = enabled;
        lbxSortierSpalten.AddAllowed = enabled ? BlueControls.Enums.AddType.OnlySuggests : BlueControls.Enums.AddType.None;
        lbxSortierSpalten.RemoveAllowed = enabled;
        lbxSortierSpalten.MoveAllowed = enabled;
    }

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

    #endregion
}