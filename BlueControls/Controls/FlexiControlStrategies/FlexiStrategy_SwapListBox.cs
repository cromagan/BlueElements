// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategySwapListBox : FlexiStrategyBase {

    #region Fields

    private SwapListBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new SwapListBox();

    public override void SubscribeEvents() => _control?.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;

    public override void UnsubscribeEvents() => _control?.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;

    protected override void ApplyStyle() {
        _control?.SuggestionsClear();
        if (ListItems is not null) {
            var itemsToAdd = new List<AbstractListItem>(ListItems);
            if (AutoSort) { itemsToAdd.Sort(); }
            _control?.SuggestionsAdd(itemsToAdd);
        }

        _control?.AddAllowed = AddAllowed != AddType.None
        ? AddAllowed
        : UserEditDialogType switch {
            EditTypeTable.Textfeld => AddType.Text,
            EditTypeTable.Textfeld_mit_Vorschlägen => AddType.Text,
            _ => AddType.None
        };
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Check(value.SplitAndCutByCr());

    private void SwapListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(string.Join('\r', _control.Checked));

    #endregion
}