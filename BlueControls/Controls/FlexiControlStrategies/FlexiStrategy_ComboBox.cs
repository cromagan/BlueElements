// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyComboBox : FlexiStrategyBase {

    #region Fields

    private ComboBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new ComboBox();
    }

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight);
        if (_control is null) { return; }
        _control.GetStyleFrom(inputFormat);
        _control.RaiseChangeDelay = delay;

        _control.DropDownStyle = editableWithTextInput ? System.Windows.Forms.ComboBoxStyle.DropDown : System.Windows.Forms.ComboBoxStyle.DropDownList;

        _control.ItemClear();
        _control.ItemEditAllowed = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        if (items != null) { _control.ItemAddRange(items); }
    }

    public override void SubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged += ValueChanged_ComboBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control.NavigateToNext += _navigateHandler;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged -= ValueChanged_ComboBox;
        if (_navigateHandler is not null) { _control.NavigateToNext -= _navigateHandler; }
    }

    protected override void SetValueToControl() {
        if (_control is not null) { _control.Text = Value; }
    }

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => Value = _control.Text;

    #endregion
}