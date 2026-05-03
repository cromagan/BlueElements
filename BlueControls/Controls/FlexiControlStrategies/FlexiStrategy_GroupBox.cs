// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyGroupBox : FlexiStrategyBase {

    #region Fields

    private GroupBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new GroupBox() {
            GroupBoxStyle = GroupBoxStyle.NormalBold,
            Text = string.Empty
        };
    }

    public override void SetValue(string value) {
    }

    public override void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) {
        base.StyleControl(context, column, caption);
        _control?.Text = caption;
    }

    public override void SubscribeEvents() {
    }

    public override void UnsubscribeEvents() {
    }

    #endregion
}