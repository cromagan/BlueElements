// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonYesNo : FlexiStrategyBase {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Button() {
            Name = "YesNoButton",
            ButtonStyle = ButtonStyle.Yes_or_No,
            Text = string.Empty,
            ImageCode = string.Empty
        };
    }

    public override void SubscribeEvents() {
        _control?.CheckedChanged += YesNoButton_CheckedChanged;
    }

    public override void UnsubscribeEvents() {
        _control?.CheckedChanged -= YesNoButton_CheckedChanged;
    }

    protected override void ApplyStyle() {
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) {
        _control?.Checked = value.FromPlusMinus();
    }

    private void YesNoButton_CheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(_control.Checked.ToPlusMinus());

    #endregion
}