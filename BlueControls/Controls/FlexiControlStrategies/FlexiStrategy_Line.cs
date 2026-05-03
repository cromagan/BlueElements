// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Orientation = BlueBasics.Enums.Orientation;

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
    }

    public override void UnsubscribeEvents() {
    }

    protected override void SetValueToControl() {
    }

    #endregion
}