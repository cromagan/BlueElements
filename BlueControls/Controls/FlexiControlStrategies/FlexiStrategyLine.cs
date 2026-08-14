// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyLine : FlexiStrategyBase {

    #region Fields

    private Line? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Line() { Orientation = Orientation.Waagerecht };
    }

    public override void SubscribeEvents() {
        _control?.LostFocus += Control_LostFocus;
    }

    public override void UnsubscribeEvents() {
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) { }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}