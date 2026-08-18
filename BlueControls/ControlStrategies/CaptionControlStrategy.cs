// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class CaptionControlStrategy : ControlStrategy {

    #region Fields

    private GroupBox? _control;

    #endregion

    #region Properties

    public static string ClassId => "Caption";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string Description => "Zeigt eine fette Beschriftung als Gruppen-Rahmen ohne Wert-Bearbeitung.";

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new GroupBox() {
        GroupBoxStyle = GroupBoxStyle.NormalBold,
        Text = string.Empty
    };

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Groupbox);

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() => _control?.Text = Caption;

    protected override void SetValueToControlInternal(string value) { }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}