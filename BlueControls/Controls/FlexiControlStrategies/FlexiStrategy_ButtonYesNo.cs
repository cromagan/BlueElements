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
        if (_control is null) { return; }
        _control.CheckedChanged += YesNoButton_CheckedChanged;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.CheckedChanged -= YesNoButton_CheckedChanged;
    }

    protected override void SetValueToControl() {
        if (_control is not null) { _control.Checked = Value.FromPlusMinus(); }
    }

    private void YesNoButton_CheckedChanged(object? sender, System.EventArgs e) => Value = _control.Checked.ToPlusMinus();

    #endregion
}