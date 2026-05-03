// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyCaption : FlexiStrategyBase {

    #region Fields

    private Caption? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Caption();
    }

    public override void SubscribeEvents() { }

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() => SetValueToControl();

    public override void HandleCaptionClick() { }

    protected override void SetValueToControl() => _control?.Text = $"<b><i>{Value}</b> {Suffix}";

    #endregion
}