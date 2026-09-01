// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class TextControlStrategy : ControlStrategy {

    #region Fields

    private const string _imageCodeKey = "imagecode";

    private Caption? _control;

    #endregion

    #region Properties

    public static string ClassId => "Text";

    public override string Description => "Zeigt den Wert als nicht editierbaren Text.";

    /// <summary>
    /// Bildcode des Symbols vor dem Text.
    /// </summary>
    public string ImageCode {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(_imageCodeKey, value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    } = string.Empty;

    public override bool IsSpecial => true;

    public override string KeyName => ClassId;

    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl)
        => [.. base.GetProperties(widthOfControl),
            new FlexiControlForProperty<string>(() => ImageCode, "Bildcode")];

    public override string ReadableText() => "Reine Textanzeige";

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Gänsefüßchen);

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() {
        if (_control is not { IsDisposed: false }) { return; }
        _control.QuickInfo = QuickInfo;
        SetValueToControlInternal(Value);
    }

    protected override void CreateControlCore() => _control = new Caption();

    protected override void ForceWriteBackValue() { }

    protected override void ReadParameters(JsonObject json) {
        base.ReadParameters(json);
        ImageCode = json.GetString(_imageCodeKey, ImageCode);
    }

    protected override void SetValueToControlInternal(string value) {
        var image = string.IsNullOrEmpty(ImageCode) ? string.Empty : $"<imagecode={ImageCode}>";

        _control?.Text = $"{image}{value} {Suffix}";
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}