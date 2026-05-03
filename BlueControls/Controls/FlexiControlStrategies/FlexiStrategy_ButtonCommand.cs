// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using System.Windows.Forms;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonCommand : FlexiStrategyBase {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Button() {
            Name = "CommandButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = string.Empty
        };
    }

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight, customContextMenuItems);
        _control?.Text = caption;
        _control?.CustomContextMenuItems = customContextMenuItems;
    }

    public override void SubscribeEvents() => _control?.Click += CommandButton_Click;

    public override void UnsubscribeEvents() => _control?.Click -= CommandButton_Click;

    protected override void SetValueToControl() {
    }

    private void CommandButton_Click(object? sender, System.EventArgs e) => OnButtonClicked();

    #endregion
}