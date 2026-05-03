// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyTextBox : FlexiStrategyBase {

    #region Fields

    private TextBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new TextBox();
    }

    public override void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight) {
        base.StyleControl(caption, inputFormat, delay, items, userEditDialogType, editableWithTextInput, editableWithDropdown, showValuesOfOtherCellsInDropdown, dropdownItems, customVocabulary, parentHeight);
        if (_control is null) { return; }
        _control.GetStyleFrom(inputFormat);
        _control.CustomVocabulary = customVocabulary;
        _control.RaiseChangeDelay = delay;
        _control.Verhalten = parentHeight > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
    }

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_TextBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control?.NavigateToNext += _navigateHandler;
    }

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_TextBox;
        _control?.NavigateToNext -= _navigateHandler;
    }

    protected override void SetValueToControl() => _control?.Text = Value;

    private void ValueChanged_TextBox(object? sender, System.EventArgs e) => Value = _control.Text;

    #endregion
}