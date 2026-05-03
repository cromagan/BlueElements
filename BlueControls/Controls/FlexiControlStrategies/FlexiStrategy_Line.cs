// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyLine : IFlexiStrategy {
    private Line? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new Line() {
            Enabled = owner.Enabled,
            Orientation = Orientation.Waagerecht
        };
    }

    public void SetValue(FlexiControl owner, string value) {
    }

    public void SubscribeEvents(FlexiControl owner) {
    }

    public void UnsubscribeEvents() {
    }
}