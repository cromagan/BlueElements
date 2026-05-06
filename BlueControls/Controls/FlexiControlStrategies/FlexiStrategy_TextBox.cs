// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyTextBox : FlexiStrategyBase {

    #region Fields

    private TextBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new TextBox();
    }

    public override async Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) {
        if (_control is not { IsDisposed: false } txb) { return; }

        var initT = await Develop.GetSafePropertyValueAsync(() => txb.Text);
        if (string.IsNullOrEmpty(initT)) { return; }

        cancellationToken.ThrowIfCancellationRequested();

        var ownUpper = ownWord.ToUpperInvariant();

        await Task.Run(async () => {
            bool processSuccessful;
            do {
                processSuccessful = true;
                try {
                    await Develop.InvokeAsync(() => {
                        if (!txb.IsDisposed) {
                            txb.Unmark(MarkState.MyOwn);
                            txb.Unmark(MarkState.Other);
                            txb.Invalidate();
                        }
                    });

                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var thisWord in words) {
                        var cap = 0;
                        do {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentText = await Develop.GetSafePropertyValueAsync(() => txb.Text);
                            if (initT == null || currentText != initT) { return; }

                            var fo = initT.IndexOfWord(thisWord, cap, RegexOptions.IgnoreCase);
                            if (fo < 0) { break; }

                            await Develop.InvokeAsync(() => {
                                if (!txb.IsDisposed) {
                                    if (thisWord.ToUpperInvariant() == ownUpper) {
                                        txb.Mark(MarkState.MyOwn, fo, fo + thisWord.Length - 1);
                                    } else {
                                        txb.Mark(MarkState.Other, fo, fo + thisWord.Length - 1);
                                    }
                                    txb.Invalidate();
                                }
                            });

                            cap = fo + thisWord.Length;
                        } while (true);
                    }
                } catch {
                    processSuccessful = false;
                    await Task.Delay(100, cancellationToken);
                }
            } while (!processSuccessful && !cancellationToken.IsCancellationRequested);
        }, cancellationToken);
    }

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_TextBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control?.NavigateToNext += _navigateHandler;
    }

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_TextBox;
        _control?.NavigateToNext -= _navigateHandler;
    }

    protected override void ApplyStyle() {
        _control?.CustomContextMenuItems = CustomContextMenuItems;
        _control?.RaiseChangeDelay = RaiseChangeDelay;
        _control?.GetStyleFrom(this);
        _control?.CustomVocabulary = CustomVocabulary;
        _control?.Suffix = Suffix;
        _control?.Verhalten = ParentHeight > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void ValueChanged_TextBox(object? sender, System.EventArgs e) => OnValueChanged(_control.Text);

    #endregion
}