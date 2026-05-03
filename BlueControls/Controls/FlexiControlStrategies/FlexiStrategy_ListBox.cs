// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Renderer;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyListBox : FlexiStrategyBase {

    #region Fields

    private ListBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new ListBox() { CheckBehavior = CheckBehavior.MultiSelection };
        _control.ItemClear();
        SubscribeEvents();
    }

    public override void SetValue(string value) {
        if (_control is not null) { _control.Check(value.SplitAndCutByCr(), true); }
    }

    public override void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) {
        base.StyleControl(context, column, caption);
        if (_control is null) { return; }

        _control.CheckBehavior = CheckBehavior.MultiSelection;
        if (column is not { IsDisposed: false }) { return; }

        var items = new List<AbstractListItem>();
        if (column.EditableWithDropdown) {
            var r = TableView.RendererOf(column, Constants.Win11);
            items.AddRange(ItemsOf(column, null, 10000, r));
            if (!column.ShowValuesOfOtherCellsInDropdown) {
                bool again;
                do {
                    again = false;
                    foreach (var thisItem in items) {
                        if (!column.DropDownItems.Contains(thisItem.KeyName)) {
                            again = true;
                            items.Remove(thisItem);
                            break;
                        }
                    }
                } while (again);
            }
        }

        _control.ItemAddRange(items);

        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                _control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                _control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                _control.AddAllowed = AddType.None;
                break;
        }

        _control.MoveAllowed = false;
        _control.RemoveAllowed = true;
        _control.Appearance = ListBoxAppearance.Listbox;
    }

    public override void SubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged += ListBox_ItemCheckedChanged;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(string.Join('\r', _control!.Checked));

    #endregion
}