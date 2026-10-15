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

    /// <summary>
    /// Die Farbauswahl funktioniert unabhängig von Texteditierbarkeit oder Dropdown-Items der Spalte.
    /// </summary>
    public override bool IsAllowed(bool textEditable, bool mayHaveDropdownItems) => true;

    /// <summary>
    /// Die Spalte darf nicht mehrzeilig sein.
    /// </summary>
    public override string ErrorReason(bool mayHaveDropDown, bool multiLine) {
        if (multiLine) { return ColumnErrorConstants.NoMultilineAllowed; }
        return string.Empty;
    }

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
