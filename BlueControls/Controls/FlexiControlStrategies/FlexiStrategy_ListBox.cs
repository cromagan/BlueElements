// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;

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

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight, customContextMenuItems);

        _control?.CustomContextMenuItems = customContextMenuItems;

        _control?.CheckBehavior = CheckBehavior.MultiSelection;
        if (items is null) { return; }

        if (editableWithDropdown) {
            var itemsToAdd = new List<AbstractListItem>(items);
            if (!showValuesOfOtherCellsInDropdown && dropdownItems is not null) {
                bool again;
                do {
                    again = false;
                    foreach (var thisItem in itemsToAdd) {
                        if (!dropdownItems.Contains(thisItem.KeyName)) {
                            again = true;
                            itemsToAdd.Remove(thisItem);
                            break;
                        }
                    }
                } while (again);
            }
            _control?.ItemAddRange(itemsToAdd);
        }

        switch (userEditDialogType) {
            case EditTypeTable.Textfeld:
                _control?.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                _control?.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                _control?.AddAllowed = AddType.None;
                break;
        }

        _control?.MoveAllowed = false;
        _control?.RemoveAllowed = true;
        _control?.Appearance = ListBoxAppearance.Listbox;
        _control?.CustomContextMenuItems = customContextMenuItems;
    }

    public override void SubscribeEvents() {
        _control?.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        _control?.RemoveClicked += ListBox_ItemRemoved;
    }

    public override void UnsubscribeEvents() {
        _control?.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
        _control?.RemoveClicked -= ListBox_ItemRemoved;
    }

    protected override void SetValueToControl() {
        if (_control is not null) { _control.Check(Value.SplitAndCutByCr(), true); }
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => Value = string.Join('\r', _control.Checked);

    private void ListBox_ItemRemoved(object? sender, AbstractListItemEventArgs e) => OnItemRemoved(e);

    #endregion
}