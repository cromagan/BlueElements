// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyCaption : FlexiStrategyBase {

    #region Fields

    private Caption? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Caption();
    }

    public override Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    public override void SubscribeEvents() { }

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() {
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) {
        var text = string.IsNullOrEmpty(value) ? Caption : $"<b><i>{value}</b>";
        var image = string.IsNullOrEmpty(ImageCode) ? string.Empty : $"<imagecode={ImageCode}>";
        _control?.Text = $"{image}{text} {Suffix}";
    }

    #endregion
}