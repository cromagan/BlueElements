// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class CommandButtonControlStrategy : ControlStrategy {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public static string ClassId => "CommandButton";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override bool IsCommandButton => true;

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new Button() {
        Name = "CommandButton",
        Checked = false,
        ButtonStyle = ButtonStyle.Button,
        Text = string.Empty
    };

    public override void SubscribeEvents() {
        _control?.Click += CommandButton_Click;
        _control?.LostFocus += Control_LostFocus;
    }

    public override void UnsubscribeEvents() {
        _control?.Click -= CommandButton_Click;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is not null) {
            _control.Text = Caption;
            _control.CustomContextMenuItems = CustomContextMenuItems;
            _control.ImageCode = ImageCode;
            _control.QuickInfo = QuickInfo;
        }
    }

    protected override void SetValueToControlInternal(string value) { }

    private void CommandButton_Click(object? sender, System.EventArgs e) => OnExecuteCommand();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}