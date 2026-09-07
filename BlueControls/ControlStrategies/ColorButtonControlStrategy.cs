// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt einen Knopf, mit dem eine Farbe gewählt werden kann.
/// </summary>
public class ColorButtonControlStrategy : ControlStrategy {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public static string ClassId => "ColorButton";

    public override string KeyName => ClassId;
    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override string ReadableText() => "Farbauswahl";

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Farbrad);

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
