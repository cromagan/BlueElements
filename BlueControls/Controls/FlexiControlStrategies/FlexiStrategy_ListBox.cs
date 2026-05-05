// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;

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
    }

    public override void SubscribeEvents() {
        _control?.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        _control?.RemoveClicked += ListBox_ItemRemoved;
    }

    public override void UnsubscribeEvents() {
        _control?.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
        _control?.RemoveClicked -= ListBox_ItemRemoved;
    }

    protected override void ApplyStyle() {
        _control?.CheckBehavior = CheckBehavior;
        _control?.ItemClear();
        if (ListItems is not null) {
            var itemsToAdd = new List<AbstractListItem>(ListItems);
            if (!ShowValuesOfOtherCellsInDropdown && DropdownItems is not null) {
                itemsToAdd.RemoveAll(it => !DropdownItems.Contains(it.KeyName));
            }
            if (AutoSort) { itemsToAdd.Sort(); }
            _control?.ItemAddRange(itemsToAdd);
        }
        _control?.AddAllowed = AddAllowed != AddType.None
            ? AddAllowed
            : UserEditDialogType switch {
                EditTypeTable.Textfeld => AddType.Text,
                EditTypeTable.Listbox => AddType.OnlySuggests,
                _ => AddType.None
            };
        _control?.MoveAllowed = false;
        _control?.RemoveAllowed = RemoveAllowed;
        _control?.Appearance = ListBoxAppearance.Listbox;
        _control?.CustomContextMenuItems = CustomContextMenuItems;
    }

    protected override void SetValueToControl() {
        if (_control is not null) { _control.Check(Value.SplitAndCutByCr(), true); }
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => Value = string.Join('\r', _control.Checked);

    private void ListBox_ItemRemoved(object? sender, AbstractListItemEventArgs e) => OnItemRemoved(e);

    #endregion
}