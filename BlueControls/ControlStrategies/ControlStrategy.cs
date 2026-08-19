// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.ControlStrategies;

public abstract class ControlStrategy : IDisposableExtended, ISupportInitialize, IReadableTextWithKey, ISimpleEditor, IInputFormat {

    #region Fields

    public static readonly AssemblyAwareCache<ControlStrategy> AllStrategies = new();

    /// <summary>
    /// Json-Key für <see cref="Border" /> innerhalb von <see cref="ControlStrategyParameter" />.
    /// </summary>
    private const string _borderKey = "border";

    private GroupBox? _borderBox;

    private volatile int _isDisposedFlag;
    private int _suspendCount;

    #endregion

    #region Events

    public event EventHandler? Disposed;

    public event EventHandler? DoUpdateSideOptionMenu;

    public event EventHandler? DropDownShowing;

    public event EventHandler? EnterKey;

    public event EventHandler? EscKey;

    public event EventHandler? ExecuteCommand;

    public event EventHandler<ListItemEventArgs>? ItemRemoved;

    public event EventHandler? LostFocus;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? TabKey;

    public event EventHandler<TextEventArgs>? ValueChanged;

    #endregion

    #region Properties

    public AddType AddAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public string AllowedChars {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    public System.Windows.Forms.AnchorStyles Anchor {
        get => Control?.Anchor ?? default;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Anchor = value; }
        }
    }

    public bool AutoSort {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Schaltet eine GroupBox als Rahmen um das erstellte Control.
    /// <see cref="Control" /> gibt dann die GroupBox zurück.
    /// </summary>
    public bool Border {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_borderKey, true);

            if (!IsEventsSuppressed) { OnDoUpdateSideOptionMenu(); }
        }
    }

    public CheckBehavior CheckBehavior {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Das anzuzeigende Control der Strategie. Bei aktivem <see cref="Border" />
    /// wird das erzeugte Control in eine GroupBox gefasst und diese zurückgegeben.
    /// </summary>
    public System.Windows.Forms.Control? Control {
        get {
            if (IsDisposed) { return null; }
            if (!Border) { return ControlCore; }
            if (_borderBox is { IsDisposed: false } box) { return box; }
            if (ControlCore is not { IsDisposed: false } inner) { return ControlCore; }

            _borderBox = new GroupBox { Text = string.Empty };
            inner.Dock = System.Windows.Forms.DockStyle.Fill;
            _borderBox.Controls.Add(inner);
            return _borderBox;
        }
    }

    /// <summary>
    /// Parameter der Strategie als Json. Jede Strategie liest daraus nur ihre
    /// eigenen Keys in ihre Properties (siehe <see cref="ReadParameters" />);
    /// unbekannte Keys werden verworfen. Ändert eine Property ihren Wert,
    /// schreibt sie den zugehörigen Key zurück, sodass das Json immer den
    /// aktuellen Zustand abbildet.
    /// </summary>
    public JsonObject ControlStrategyParameter {
        get;
        set {
            if (IsDisposed) { return; }
            BeginInit();
            try {
                field = new JsonObject();
                ReadParameters(value);
            } finally {
                EndInit();
            }
        }
    } = new();

    public ReadOnlyCollection<ListItem>? CustomContextMenuItems {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public IReadOnlySet<string>? CustomVocabulary {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Kurze Beschreibung der Strategie für den Property-Editor.
    /// </summary>
    public virtual string Description => string.Empty;

    public string ForbiddenChars {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    public int Height {
        get => Control?.Height ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Height = value; }
        }
    }

    /// <summary>
    /// True, wenn die Strategie ein Kommando-Knopf ist, der statt einer
    /// Wert-Eingabe ExecuteCommand auslöst.
    /// </summary>
    public virtual bool IsCommandButton => false;

    public bool IsDisposed => _isDisposedFlag == 1;

    /// <summary>
    /// True, während sich die Strategie in einer Initialisierungs- oder
    /// Parse-Phase befindet (zwischen <see cref="BeginInit" /> und
    /// <see cref="EndInit" />). Property-Setter lösen dann kein ApplyStyle aus.
    /// </summary>
    public bool IsEventsSuppressed => _suspendCount > 0;

    /// <summary>
    /// True für Strategien, die im ColumnEditor nicht wählbar sind, weil sie
    /// nur vom System gesetzt werden (DragDrop) oder Spezial-Kontexten
    /// dienen (Text, Line, Caption).
    /// </summary>
    public virtual bool IsSpecial => false;

    /// <summary>
    /// Eindeutiger, stabiler Key der Strategie (identisch zur statischen ClassId).
    /// Dient als Auswahl- und Serialisierungsschlüssel am ColumnItem.
    /// </summary>
    public virtual string KeyName => GetType().Name;

    public int Left {
        get => Control?.Left ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Left = value; }
        }
    }

    public List<ListItem>? ListItems {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public int MaxTextLength {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public int MinTextLength {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public bool MoveAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public bool MultiLine {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Meldung, warum kein Edit-Control geöffnet wird. Leer, wenn die
    /// Strategie eine Eingabe erlaubt.
    /// </summary>
    public virtual string NotEditableReason => string.Empty;

    public int ParentHeight {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public string QuickInfo {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    public int RaiseChangeDelay {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = 1;

    public string RegexCheck {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    public bool RemoveAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public string Suffix {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    /// <summary>
    /// Gibt an, ob diese Strategie eine Auswahlliste (Vorschläge) handhaben kann.
    /// Ist <c>true</c>, ermittelt die <see cref="TableView" /> beim Start des
    /// Edits die Items zur content-Spalte/-Zeile und weist sie
    /// <see cref="ListItems" /> zu — einheitlich, unabhängig vom konkreten
    /// Strategie-Typ. Hat der Aufrufer bereits Items übergeben, werden diese
    /// übernommen. Default <c>false</c>.
    /// </summary>
    public virtual bool SupportsSuggestions => false;

    /// <summary>
    /// True, wenn diese Strategie freie Text-Eingabe erlaubt. Fester Wert pro
    /// Klasse, nicht konfigurierbar.
    /// </summary>
    public virtual bool SupportsTextEdit => false;

    /// <summary>
    /// True, wenn der Benutzer den Wert grundsätzlich ändern kann — auch
    /// ohne Edit-Control, z. B. durch Verschieben der Zeile.
    /// </summary>
    public virtual bool SupportsValueChange => true;

    /// <summary>
    /// True, wenn die Strategie Wörter im Text hervorheben kann (HighlightWordsAsync).
    /// </summary>
    public virtual bool SupportsWordHighlighting => false;

    public bool TextInputAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    public int Top {
        get => Control?.Top ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Top = value; }
        }
    }

    /// <summary>
    /// Setzt den Wert im Control und löst <see cref="ValueChanged" /> aus.
    /// Prüft bewusst nicht IsDisposed: <see cref="ForceWriteBackValue" /> läuft
    /// während des Dispose und muss den Wert noch propagieren können.
    /// </summary>
    public string Value {
        protected get;
        set {
            field = value;

            UnsubscribeEvents();

            SetValueToControlInternal(value);

            SubscribeEvents();

            OnValueChanged(value);
        }
    } = string.Empty;

    public bool Visible {
        get => Control is { Visible: true, IsDisposed: false };
        set {
            if (IsDisposed) { return; }
            if (Control is not { IsDisposed: false } c) { return; }
            if (!value) { ForceWriteBackValue(); }
            c.Visible = value;
        }
    }

    public int Width {
        get => Control?.Width ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Width = value; }
        }
    }

    [DefaultValue(1f)]
    public float Zoom {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = 1f;

    /// <summary>
    /// Das von der konkreten Strategie erzeugte Control ohne Rahmen.
    /// </summary>
    protected abstract System.Windows.Forms.Control? ControlCore { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die dauerhaft gecachte Prototyp-Instanz zur Strategy (KeyName).
    /// Unbekannte Schlüssel fallen wie bei <see cref="CreateNew" /> auf die
    /// Textfeld-Strategie zurück. Nur für Fähigkeitsabfragen nutzen — die
    /// Instanz erzeugt keine Controls und darf weder konfiguriert noch
    /// verworfen werden.
    /// </summary>
    public static ControlStrategy Cached(string? editStrategyKey) =>
        AllStrategies[editStrategyKey] ?? AllStrategies[TextBoxControlStrategy.ClassId] ?? new TextBoxControlStrategy();

    /// <summary>
    /// Übersetzt die Zahlen des entfernten enums ControlStrategyFormula in die ClassId
    /// der zugehörigen Strategie. Unbekannte Zahlen liefern None.
    /// </summary>
    public static string ClassIdFromLegacyControlStrategy(string legacyControlStrategy) {
        if (!legacyControlStrategy.IsLong()) { return legacyControlStrategy; }

        switch (LongParse(legacyControlStrategy)) {
            case -1:
                return NoneControlStrategy.ClassId;

            case 0:
                return TextBoxControlStrategy.ClassId;

            case 1:
                return ComboBoxControlStrategy.ClassId;

            case 2:
                return SwapListBoxControlStrategy.ClassId;

            case 3:
                return TextBoxSuggestionsControlStrategy.ClassId;

            case 4:
                return YesNoButtonControlStrategy.ClassId;

            case 5:
                return ColorButtonControlStrategy.ClassId;

            case 22:
                return TextControlStrategy.ClassId;

            case 23:
                return CaptionControlStrategy.ClassId;

            case 26:
                return ListBoxControlStrategy.ClassId;

            case 1000:
                return LineControlStrategy.ClassId;

            case 1001:
                return CommandButtonControlStrategy.ClassId;

            case 1002:
                return TableControlStrategy.ClassId;

            default:
                return NoneControlStrategy.ClassId;
        }
    }

    /// <summary>
    /// Erzeugt eine frische Instanz zur übergebenen Strategy (KeyName) —
    /// inklusive None und DragDrop. Unbekannte Keys liefern die Textfeld-Strategie.
    /// </summary>
    public static ControlStrategy CreateNew(string? editStrategyKey) {
        var type = AllStrategies[editStrategyKey]?.GetType();
        if (type is not null && Activator.CreateInstance(type) is ControlStrategy strategy) {
            return strategy;
        }

        return new TextBoxControlStrategy();
    }

    public void BeginInit() {
        if (IsDisposed) { return; }
        _suspendCount++;
    }

    /// <summary>
    /// Berechnet die für das Control benötigte Größe anhand der aktuell
    /// gesetzten Eigenschaften (z. B. <see cref="ListItems" /> oder die
    /// Chip-Fläche der Suggestions). Strategien, die keine besondere Größe
    /// fordern, geben die übergebene Größe unverändert zurück. Die TableView
    /// ruft diese Methode einheitlich auf und fragt dabei weder den
    /// konkreten Strategy-Typ noch das Control ab.
    /// </summary>
    public virtual Size CalculateRequiredSize(int minWidth, int minHeight) => new(minWidth, minHeight);

    /// <summary>
    /// Erzeugt das Control der Strategie und gibt das anzuzeigende Control
    /// zurück — bei aktivem <see cref="Border" /> die umgebende GroupBox.
    /// Strategien ohne Control (None, DragDrop) geben null zurück.
    /// </summary>
    public System.Windows.Forms.Control? CreateControl() {
        if (ControlCore is null) { CreateControlCore(); }
        return Control;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void EndInit() {
        if (IsDisposed || _suspendCount == 0) { return; }
        _suspendCount--;
        if (!IsEventsSuppressed) { ApplyStyle(); }
    }

    /// <summary>
    /// Optionen der Strategie für den Property-Editor.
    /// Leer, wenn die Strategie keine konfigurierbaren Optionen hat.
    /// </summary>
    public virtual List<GenericControl> GetProperties(int widthOfControl) => [new FlexiControlForProperty<bool>(() => Border, "Rahmen")];

    public virtual void HandleCaptionClick() { }

    public virtual Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Prüft, ob die Strategie zu einer Spalte mit den übergebenen
    /// Bearbeitungs-Fähigkeiten passt. Basis-Implementierung vergleicht
    /// gegen <see cref="SupportsTextEdit" /> und <see cref="SupportsSuggestions" />.
    /// </summary>
    public bool IsAllowed(bool textEditable, bool mayHaveDropdownItems) {
        if (SupportsTextEdit && SupportsSuggestions) { return textEditable || mayHaveDropdownItems; }
        if (SupportsTextEdit) { return textEditable; }
        if (SupportsSuggestions) { return mayHaveDropdownItems; }
        return false;
    }

    /// <summary>
    /// Lesbarer Anzeigename der Strategie, z. B. für Auswahllisten.
    /// </summary>
    public virtual string ReadableText() => GetType().Name;

    public abstract void SubscribeEvents();

    /// <summary>
    /// Symbol zur Darstellung der Strategie, z. B. in Auswahllisten.
    /// </summary>
    public virtual QuickImage? SymbolForReadableText() => null;

    public abstract void UnsubscribeEvents();

    public virtual bool WasValueClicked() => false;

    protected abstract void ApplyStyle();

    protected abstract void CreateControlCore();

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            ForceWriteBackValue();
            OnDisposed();

            UnsubscribeEvents();

            Disposed = null;
            DoUpdateSideOptionMenu = null;
            DropDownShowing = null;
            EnterKey = null;
            EscKey = null;
            ExecuteCommand = null;
            ItemRemoved = null;
            LostFocus = null;
            NavigateToNext = null;
            TabKey = null;
            ValueChanged = null;

            if (_borderBox is { IsDisposed: false } box) {
                box.Visible = false;
                box.Dispose(); // Disposed auch das eingebettete Control
            } else if (ControlCore is { IsDisposed: false } control) {
                control.Visible = false;
                control.Dispose();
            }
        }
    }

    /// <summary>
    /// Holt den aktuellen Wert vom Control und schreibt ihn über den
    /// Value-Setter zurück, sodass <see cref="ValueChanged" /> ausgelöst wird.
    /// Wird beim Dispose, beim Unsichtbarmachen und von den
    /// ValueChanged-Routinen der Strategien aufgerufen.
    /// </summary>
    protected abstract void ForceWriteBackValue();

    protected void OnDoUpdateSideOptionMenu() => DoUpdateSideOptionMenu?.Invoke(this, System.EventArgs.Empty);

    protected void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    protected void OnEnterKey() => EnterKey?.Invoke(this, System.EventArgs.Empty);

    protected void OnEscKey() => EscKey?.Invoke(this, System.EventArgs.Empty);

    protected void OnExecuteCommand() => ExecuteCommand?.Invoke(this, System.EventArgs.Empty);

    protected void OnItemRemoved(ListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    protected void OnLostFocus() => LostFocus?.Invoke(this, System.EventArgs.Empty);

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected void OnTabKey() => TabKey?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Übernimmt die eigenen Keys aus dem Json in die Properties. Unbekannte
    /// Keys werden ignoriert und damit verworfen.
    /// </summary>
    protected virtual void ReadParameters(JsonObject json) => Border = json.GetBool(_borderKey, Border);

    protected abstract void SetValueToControlInternal(string value);

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnValueChanged(string newvalue) => ValueChanged?.Invoke(this, new TextEventArgs(newvalue));

    #endregion
}