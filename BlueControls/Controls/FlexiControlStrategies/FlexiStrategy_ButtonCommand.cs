// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonCommand : IFlexiStrategy {
    private Button? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new Button() {
            Enabled = owner.Enabled,
            Name = "CommandButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = owner.Caption
        };
        SubscribeEvents(owner);
    }

    public void SetValue(FlexiControl owner, string value) {
    }

    public void SubscribeEvents(FlexiControl owner) {
        if (_control is null) { return; }
        _control.Click += CommandButton_Click;
    }

    public void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.Click -= CommandButton_Click;
    }

    private void CommandButton_Click(object? sender, System.EventArgs e) => ((FlexiControl)((Button)sender).Parent).InvokeButtonClicked();
}