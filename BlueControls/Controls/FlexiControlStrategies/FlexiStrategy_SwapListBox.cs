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

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight);

        _control?.SuggestionsClear();
        if (items is null) { return; }

        _control?.SuggestionsAdd(items);

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
    }

    public override void SubscribeEvents() => _control?.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;

    public override void UnsubscribeEvents() => _control?.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;

    protected override void SetValueToControl() => _control?.Check(Value.SplitAndCutByCr());

    private void SwapListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => Value = string.Join('\r', _control.Checked);

    #endregion
}