// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyLine : FlexiStrategyBase {
    private Line? _control;

    public override System.Windows.Forms.Control? Control => _control;

    public override void CreateControl() {
        _control = new Line() { Orientation = Orientation.Waagerecht };
    }

    public override void SetValue(string value) {
    }

    public override void SubscribeEvents() {
    }

    public override void UnsubscribeEvents() {
    }
}
