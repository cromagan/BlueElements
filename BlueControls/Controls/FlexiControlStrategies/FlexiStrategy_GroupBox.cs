// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyGroupBox : IFlexiStrategy {
    private GroupBox? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new GroupBox() {
            Enabled = owner.Enabled,
            GroupBoxStyle = GroupBoxStyle.NormalBold,
            Text = owner.Caption
        };
    }

    public void SetValue(FlexiControl owner, string value) {
    }

    public void SubscribeEvents(FlexiControl owner) {
    }

    public void UnsubscribeEvents() {
    }
}