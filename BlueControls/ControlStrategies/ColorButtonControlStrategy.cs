// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class ColorButtonControlStrategy : ControlStrategy {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public static string ClassId => "ColorButton";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string Description => "Zeigt einen Knopf, mit dem eine Farbe gewählt werden kann.";

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Farbrad);

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() => _control?.QuickInfo = QuickInfo;

    protected override void CreateControlCore() => _control = new Button() {
        Name = "ColorButton",
        Checked = false,
        ButtonStyle = ButtonStyle.Button,
        Text = string.Empty
    };

    protected override void ForceWriteBackValue() { }

    protected override void SetValueToControlInternal(string value)
        => _control?.ImageCode = string.IsNullOrEmpty(value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(IntParse(value)).ToHtmlCode();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}
