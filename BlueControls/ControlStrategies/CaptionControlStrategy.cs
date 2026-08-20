// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class CaptionControlStrategy : ControlStrategy {

    #region Fields

    private GroupBox? _control;

    #endregion

    #region Properties

    public static string ClassId => "Caption";

    public override string Description => "Zeigt den Wert als fette Beschriftung in einem Gruppen-Rahmen ohne Bearbeitung.";
    public override bool IsSpecial => true;
    public override string KeyName => ClassId;
    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override string ReadableText() => "Überschrift";

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Groupbox);

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() => SetValueToControlInternal(Value);

    protected override void CreateControlCore() => _control = new GroupBox() {
        GroupBoxStyle = GroupBoxStyle.NormalBold,
        Text = string.Empty
    };

    protected override void ForceWriteBackValue() { }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}