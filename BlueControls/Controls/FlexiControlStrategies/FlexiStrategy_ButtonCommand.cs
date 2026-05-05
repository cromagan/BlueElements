// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonCommand : FlexiStrategyBase {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Button() {
            Name = "CommandButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = string.Empty
        };
    }

    public override void SubscribeEvents() => _control?.Click += CommandButton_Click;

    public override void UnsubscribeEvents() => _control?.Click -= CommandButton_Click;

    protected override void ApplyStyle() {
        if (_control is not null) {
            _control.Text = Caption;
            _control.CustomContextMenuItems = CustomContextMenuItems;
            _control.ImageCode = ImageCode;
            _control.QuickInfo = QuickInfo;
        }
    }

    protected override void SetValueToControl() { }

    private void CommandButton_Click(object? sender, System.EventArgs e) => OnExecuteComand();

    #endregion
}