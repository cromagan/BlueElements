// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.Interfaces;

namespace BlueControls.ControlStrategies;

public class TextBoxSuggestionsControlStrategy : ControlStrategy {

    #region Fields

    private const string _suggestionPositionKey = "suggestionposition";

    private TextBoxSuggestions? _control;

    #endregion

    #region Properties

    public static string ClassId => "TextBoxSuggestions";

    public override string Description => "Textfeld, das zusätzlich auswählbare Vorschläge als Chips anbietet.";
    public override string KeyName => ClassId;

    /// <summary>
    /// Wenn gewählt, wird der Text auf Schreibfehler geprüft und gefundene Stellen werden unterstreicht.
    /// </summary>
    public bool SpellCheckingEnabled {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(TextBoxControlStrategy.SpellCheckingEnabledKey, value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    }

    /// <summary>
    /// Legt fest, wo die Vorschlagsliste erscheint.
    /// </summary>
    public SuggestionPosition SuggestionPosition {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_suggestionPositionKey, (int)value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    } = SuggestionPosition.Bottom;

    public override bool SupportsSuggestions => true;
    public override bool SupportsTextEdit => true;

    /// <summary>
    /// Wenn gewählt, kann der Benutzer den Text gestalten, z. B. fett, kursiv oder farbig.
    /// </summary>
    public bool TextFormatingAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(TextBoxControlStrategy.TextFormatingAllowedKey, value);
            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    }

    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    /// <summary>
    /// Übernimmt die TextBox-Größe am Control und ermittelt die zusätzlich
    /// benötigte Gesamthöhe (Textbox + Chip-Fläche der Suggestions) über
    /// TextBoxSuggestions.GetEstimatedHeight. Die Breite wird
    /// dabei so vergrößert, dass der breiteste Chip vollständig Platz findet
    /// und nicht abgeschnitten wird; das umbrechende Verhalten nach unten
    /// bleibt unverändert.
    /// </summary>
    public override Rectangle CalculateRequiredBounds(Rectangle bounds) {
        var required = base.CalculateRequiredBounds(bounds);
        if (_control is not { } c) { return required; }
        var width = Math.Max(required.Width, c.GetEstimatedWidth());
        c.TextboxSize = new Size(width, required.Height);
        return new Rectangle(required.Location, new Size(width, c.GetEstimatedHeight(width, required.Height)));
    }

    public override List<GenericControl> GetProperties(int widthOfControl)
        => [.. base.GetProperties(widthOfControl),
            new FlexiControlForProperty<bool>(() => SpellCheckingEnabled, "Rechtschreibprüfung"),
            new FlexiControlForProperty<bool>(() => TextFormatingAllowed, "Formatierung erlauben"),
            new FlexiControlForProperty<SuggestionPosition>(() => SuggestionPosition, "Position der Vorschläge", ItemsOf(typeof(SuggestionPosition)))];

    public override string ReadableText() => "Textfeld mit Vorschlägen";

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_TextBoxSuggestions;
        _control?.NavigateToNext += Control_NavigateToNext;
        _control?.EnterKey += Control_EnterKey;
        _control?.EscKey += Control_EscKey;
        _control?.TabKey += Control_TabKey;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2);

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_TextBoxSuggestions;
        _control?.NavigateToNext -= Control_NavigateToNext;
        _control?.EnterKey -= Control_EnterKey;
        _control?.EscKey -= Control_EscKey;
        _control?.TabKey -= Control_TabKey;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is not { IsDisposed: false }) { return; }

        _control.CustomContextMenuItems = CustomContextMenuItems;
        _control.RaiseChangeDelay = RaiseChangeDelay;
        _control.GetStyleFrom(this);
        _control.CustomVocabulary = CustomVocabulary;
        _control.Suffix = Suffix;
        _control.SpellCheckingEnabled = SpellCheckingEnabled;
        _control.TextFormatingAllowed = TextFormatingAllowed;
        _control.SuggestionPosition = SuggestionPosition;
        _control.Verhalten = ParentHeight > 20.CanvasToControl(Zoom)
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        _control.QuickInfo = QuickInfo;
        _control.Zoom = Zoom;

        if (ListItems is { Count: > 0 } items) {
            _control.Suggestions = new System.Collections.ObjectModel.ReadOnlyCollection<string>([.. items.Select(i => i.KeyName)]);
        } else {
            _control.Suggestions = System.Collections.ObjectModel.ReadOnlyCollection<string>.Empty;
        }
    }

    protected override void CreateControlCore() => _control = new TextBoxSuggestions();

    protected override void ForceWriteBackValue() {
        if (_control is not { IsDisposed: false } c) { return; }
        Value = c.Text;
    }

    protected override void ReadParameters(JsonObject json) {
        base.ReadParameters(json);
        SpellCheckingEnabled = json.GetBool(TextBoxControlStrategy.SpellCheckingEnabledKey, SpellCheckingEnabled);
        TextFormatingAllowed = json.GetBool(TextBoxControlStrategy.TextFormatingAllowedKey, TextFormatingAllowed);
        SuggestionPosition = json.GetEnum(_suggestionPositionKey, SuggestionPosition);
    }

    protected override void SetValueToControlInternal(string value) {
        if (_control is { } c) { c.Text = value; }
    }

    private void Control_EnterKey(object? sender, System.EventArgs e) => OnEnterKey();

    private void Control_EscKey(object? sender, System.EventArgs e) => OnEscKey();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void Control_NavigateToNext(object? sender, NavigationDirectionEventArgs e) => OnNavigateToNext(e.Direction);

    private void Control_TabKey(object? sender, System.EventArgs e) => OnTabKey();

    private void ValueChanged_TextBoxSuggestions(object? sender, System.EventArgs e) => ForceWriteBackValue();

    #endregion
}