// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyComboBox : IFlexiStrategy {
    private ComboBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new ComboBox();
        owner.StyleComboBox(_control, null, System.Windows.Forms.ComboBoxStyle.DropDownList, false, 1);
        SubscribeEvents(owner);
    }

    public void SetValue(FlexiControl owner, string value) {
        if (_control is not null) { _control.Text = value; }
    }

    public void SubscribeEvents(FlexiControl owner) {
        if (_control is null) { return; }
        _control.TextChanged += ValueChanged_ComboBox;
        _navigateHandler = (_, e) => owner.InvokeNavigateToNext(e.Direction);
        _control.NavigateToNext += _navigateHandler;
    }

    public void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged -= ValueChanged_ComboBox;
        if (_navigateHandler is not null) { _control.NavigateToNext -= _navigateHandler; }
    }

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => ((FlexiControl)((ComboBox)sender).Parent).ValueSet(((ComboBox)sender).Text, false);
}