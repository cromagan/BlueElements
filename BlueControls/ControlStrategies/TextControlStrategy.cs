// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.ControlStrategies;

public class TextControlStrategy : ControlStrategy {

    #region Fields

    private Caption? _control;

    #endregion

    #region Properties

    public static string ClassId => "Text";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string Description => "Zeigt den Wert als hervorgehobenen, nicht editierbaren Text.";

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new Caption();

    public override Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Gänsefüßchen);

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() => _control?.QuickInfo = QuickInfo;

    protected override void SetValueToControlInternal(string value) {
        var text = string.IsNullOrEmpty(value) ? Caption : $"<b><i>{value}</b>";
        var image = string.IsNullOrEmpty(ImageCode) ? string.Empty : $"<imagecode={ImageCode}>";
        _control?.Text = $"{image}{text} {Suffix}";
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}