// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.MarkRenderer;
using BlueTable.Interfaces;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.ControlStrategies;

public class TextBoxControlStrategy : ControlStrategy {

    #region Fields

    /// <summary>
    /// JSON-Keys innerhalb von ControlStrategyParameter.
    /// </summary>
    public const string SpellCheckingEnabledKey = "spellcheckingenabled";

    public const string TextFormatingAllowedKey = "textformatingallowed";

    private TextBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public static string ClassId => "Texbox";

    public override string Description => "Einfaches Textfeld zur freien Eingabe von Text.";
    public override string KeyName => ClassId;

    public bool SpellCheckingEnabled {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(SpellCheckingEnabledKey, value);
            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    }

    public override bool SupportsTextEdit => true;
    public override bool SupportsWordHighlighting => true;

    public bool TextFormatingAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(TextFormatingAllowedKey, value);
            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    }

    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl)
        => [.. base.GetProperties(widthOfControl),
            new FlexiControlForProperty<bool>(() => SpellCheckingEnabled, "Rechtschreibprüfung"),
            new FlexiControlForProperty<bool>(() => TextFormatingAllowed, "Formatierung erlauben")];

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

    /// <summary>
    /// Begrenzt einzeilig die Höhe auf das 2,5-Fache der Zeilenhöhe, mehrzeilig
    /// bleibt die Feldhöhe unverändert.
    /// </summary>
    public override Rectangle CalculateRequiredBounds(Rectangle bounds) {
        if (!MultiLine) { bounds.Height = Math.Min(bounds.Height, MaxSingleLineFillHeight); }
        return base.CalculateRequiredBounds(bounds);
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

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Textfeld);

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
        _control?.SpellCheckingEnabled = SpellCheckingEnabled;
        _control?.TextFormatingAllowed = TextFormatingAllowed;
        _control?.Verhalten = ParentHeight > 20.CanvasToControl(Zoom)
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        _control?.QuickInfo = QuickInfo;
        _control?.Zoom = Zoom;
    }

    protected override void CreateControlCore() => _control = new TextBox();

    protected override void ForceWriteBackValue() {
        if (_control is not { IsDisposed: false } c) { return; }
        Value = c.Text;
    }

    protected override void ReadParameters(JsonObject json) {
        base.ReadParameters(json);
        SpellCheckingEnabled = json.GetBool(SpellCheckingEnabledKey, SpellCheckingEnabled);
        TextFormatingAllowed = json.GetBool(TextFormatingAllowedKey, TextFormatingAllowed);
    }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void Control_EnterKey(object? sender, System.EventArgs e) => OnEnterKey();

    private void Control_EscKey(object? sender, System.EventArgs e) => OnEscKey();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void Control_TabKey(object? sender, System.EventArgs e) => OnTabKey();

    private void ValueChanged_TextBox(object? sender, System.EventArgs e) => ForceWriteBackValue();

    #endregion
}