// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonYesNo : IFlexiStrategy {
    private Button? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new Button() {
            Enabled = owner.Enabled,
            Name = "YesNoButton",
            ButtonStyle = ButtonStyle.Yes_or_No,
            Text = string.Empty,
            ImageCode = string.Empty
        };
        SubscribeEvents(owner);
    }

    public void SetValue(FlexiControl owner, string value) {
        if (_control is not null) { _control.Checked = value.FromPlusMinus(); }
    }

    public void SubscribeEvents(FlexiControl owner) {
        if (_control is null) { return; }
        _control.CheckedChanged += YesNoButton_CheckedChanged;
    }

    public void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.CheckedChanged -= YesNoButton_CheckedChanged;
    }

    private void YesNoButton_CheckedChanged(object? sender, System.EventArgs e) => ((FlexiControl)((Button)sender).Parent).ValueSet(((Button)sender).Checked.ToPlusMinus(), false);
}