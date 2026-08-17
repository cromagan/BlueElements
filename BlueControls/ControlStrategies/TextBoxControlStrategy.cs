// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.MarkRenderer;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.ControlStrategies;

public class TextBoxControlStrategy : ControlStrategy {

    #region Fields

    private TextBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public static string ClassId => "Texbox";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string KeyName => ClassId;

    public override bool SupportsTextEdit => true;

    public override bool SupportsWordHighlighting => true;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new TextBox();

    public override async Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) {
        if (_control is not { IsDisposed: false } txb) { return; }

        var initT = await Develop.GetSafePropertyValueAsync(() => txb.Text);
        if (string.IsNullOrEmpty(initT)) { return; }

        cancellationToken.ThrowIfCancellationRequested();

        await Task.Run(async () => {
            bool processSuccessful;
            do {
                processSuccessful = true;
                try {
                    await Develop.InvokeAsync(() => {
                        if (!txb.IsDisposed) {
                            txb.Unmark(MyOwnMarkRenderer.Type);
                            txb.Unmark(OtherRowMarkRenderer.Type);
                            txb.Invalidate();
                        }
                    });

                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var thisWord in words) {
                        var cap = 0;
                        do {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentText = await Develop.GetSafePropertyValueAsync(() => txb.Text);
                            if (initT is null || currentText != initT) { return; }

                            var fo = initT.IndexOfWord(thisWord, cap, RegexOptions.IgnoreCase);
                            if (fo < 0) { break; }

                            await Develop.InvokeAsync(() => {
                                if (!txb.IsDisposed) {
                                    if (string.Equals(thisWord, ownWord, StringComparison.OrdinalIgnoreCase)) {
                                        txb.Mark(MyOwnMarkRenderer.Type, fo, fo + thisWord.Length - 1);
                                    } else {
                                        txb.Mark(OtherRowMarkRenderer.Type, fo, fo + thisWord.Length - 1);
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

    public override string ReadableText() => "Textfeld";

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_TextBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control?.NavigateToNext += _navigateHandler;
        _control?.EnterKey += Control_EnterKey;
        _control?.EscKey += Control_EscKey;
        _control?.TabKey += Control_TabKey;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get("Textfeld");

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_TextBox;
        _control?.NavigateToNext -= _navigateHandler;
        _control?.EnterKey -= Control_EnterKey;
        _control?.EscKey -= Control_EscKey;
        _control?.TabKey -= Control_TabKey;
        _control?.LostFocus -= Control_LostFocus;
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
        _control?.Zoom = Zoom;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void Control_EnterKey(object? sender, System.EventArgs e) => OnEnterKey();

    private void Control_EscKey(object? sender, System.EventArgs e) => OnEscKey();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void Control_TabKey(object? sender, System.EventArgs e) => OnTabKey();

    private void ValueChanged_TextBox(object? sender, System.EventArgs e) {
        if (_control is not { IsDisposed: false }) { return; }
        OnValueChanged(_control.Text);
    }

    #endregion
}