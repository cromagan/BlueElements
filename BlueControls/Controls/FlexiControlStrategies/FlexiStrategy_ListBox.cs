// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
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
        if (ListItems != null) {
            var itemsToAdd = new List<AbstractListItem>(ListItems);
            if (AutoSort) { itemsToAdd.Sort(); }
            _control?.ItemAddRange(itemsToAdd);
        }

        _control?.AddAllowed = AddAllowed != AddType.None
            ? AddAllowed
            : UserEditDialogType switch {
                EditTypeTable.Textfeld => AddType.Text,
                EditTypeTable.Textfeld_mit_Vorschlägen => AddType.Text,
                _ => AddType.None
            };
        _control?.MoveAllowed = false;
        _control?.RemoveAllowed = RemoveAllowed;
        _control?.Appearance = ListBoxAppearance.Listbox_Boxes;
        _control?.CustomContextMenuItems = CustomContextMenuItems;
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) {
        if (_control is null) { return; }
        var values = value.SplitAndCutByCr();
        foreach (var v in values) {
            if (!string.IsNullOrEmpty(v) && _control[v] == null) {
                _control.ItemAdd(ItemOf(v));
            }
        }
        _control.Check(values, true);
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(string.Join('\r', _control.Checked));

    private void ListBox_ItemRemoved(object? sender, AbstractListItemEventArgs e) => OnItemRemoved(e);

    #endregion
}