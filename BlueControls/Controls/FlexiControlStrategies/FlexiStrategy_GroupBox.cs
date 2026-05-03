// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using System.Windows.Forms;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyGroupBox : FlexiStrategyBase {

    #region Fields

    private GroupBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new GroupBox() {
            GroupBoxStyle = GroupBoxStyle.NormalBold,
            Text = string.Empty
        };
    }

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight);
        _control?.Text = caption;
    }

    public override void SubscribeEvents() {
    }

    public override void UnsubscribeEvents() {
    }

    protected override void SetValueToControl() {
    }

    #endregion
}