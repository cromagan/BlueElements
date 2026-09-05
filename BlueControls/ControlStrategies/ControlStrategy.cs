// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.ControlStrategies;

public abstract class ControlStrategy : IDisposableExtended, ISupportInitialize, IReadableTextWithKey, ISimpleEditor, IErrorCheckable, IInputFormat {

    #region Fields

    /// <summary>
    /// Feste Höhe einzeiliger Eingabe-Controls (z. B. Combobox, Textbox),
    /// unabhängig von der Zeilenhöhe der Zelle.
    /// </summary>
    public const int SingleLineHeight = 24;

    public static readonly AssemblyAwareCache<ControlStrategy> AllStrategies = new();

    /// <summary>
    /// Json-Key für Border innerhalb von ControlStrategyParameter.
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

    /// <summary>
    /// Legt fest, ob neue Listeneinträge hinzugefügt werden dürfen.
    /// </summary>
    public AddType AddAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Zusätzliche Prüfung, ob der Wert zum Spaltenformat passt.
    /// </summary>
    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Nur diese Zeichen dürfen eingegeben werden. Leer = alle erlaubt.
    /// </summary>
    public string AllowedChars {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    /// <summary>
    /// Verankerung des Controls in seinem Container.
    /// </summary>
    public System.Windows.Forms.AnchorStyles Anchor {
        get => Control?.Anchor ?? default;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Anchor = value; }
        }
    }

    /// <summary>
    /// Sortiert die Einträge der Liste automatisch alphabetisch.
    /// </summary>
    public bool AutoSort {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Zeichnet einen Rahmen mit Überschrift um das Feld.
    /// Bei „Kein Rahmen" bleibt nur das Feld selbst sichtbar.
    /// </summary>
    public GroupBoxStyle Border {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_borderKey, (int)value);

            if (!IsEventsSuppressed) { OnDoUpdateSideOptionMenu(); }
        }
    } = GroupBoxStyle.Nothing;

    /// <summary>
    /// Legt fest, ob mehrere Einträge gleichzeitig gewählt werden können.
    /// </summary>
    public CheckBehavior CheckBehavior {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = CheckBehavior.MultiSelection;

    /// <summary>
    /// Das anzuzeigende Control der Strategie. Bei Rahmen-Stil ungleich
    /// GroupBoxStyle.Nothing wird das erzeugte Control in eine
    /// GroupBox gefasst und diese zurückgegeben.
    /// Hat das Control bereits einen Parent, übernimmt die Box dessen Platz.
    /// </summary>
    public System.Windows.Forms.Control? Control {
        get {
            if (IsDisposed) { return null; }

            if (Border == GroupBoxStyle.Nothing) {
                if (_borderBox is { IsDisposed: false } oldBox) { RemoveBorderBox(oldBox); }
                return ControlCore;
            }

            if (_borderBox is { IsDisposed: false } box) {
                box.GroupBoxStyle = Border;
                return box;
            }
            if (ControlCore is not { IsDisposed: false } inner) { return null; }

            var parent = inner.Parent;
            var bounds = inner.Bounds;
            var anchor = inner.Anchor;

            _borderBox = new GroupBox {
                GroupBoxStyle = Border,
                Text = "Bearbeiten",
                Padding = new System.Windows.Forms.Padding(Skin.PaddingMedium)
            };
            inner.Dock = System.Windows.Forms.DockStyle.Fill;
            _borderBox.Controls.Add(inner);

            if (parent is { IsDisposed: false }) {
                _borderBox.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                _borderBox.Anchor = anchor;
                parent.Controls.Add(_borderBox);
            }
            return _borderBox;
        }
    }

    /// <summary>
    /// Parameter der Strategie als Json. Jede Strategie liest daraus nur ihre
    /// eigenen Keys in ihre Properties (siehe ReadParameters);
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

    /// <summary>
    /// Eigene zusätzliche Einträge für das Kontextmenü.
    /// </summary>
    public ReadOnlyCollection<ListItem>? CustomContextMenuItems {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Eigene Wörter, die die Rechtschreibprüfung nicht bemängelt.
    /// </summary>
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

    /// <summary>
    /// Diese Zeichen dürfen nicht eingegeben werden.
    /// </summary>
    public string ForbiddenChars {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    /// <summary>
    /// Höhe des Controls in Pixel.
    /// </summary>
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

    /// <summary>
    /// True, wenn die Strategie bereits freigegeben wurde.
    /// </summary>
    public bool IsDisposed => _isDisposedFlag == 1;

    /// <summary>
    /// True, während sich die Strategie in einer Initialisierungs- oder
    /// Parse-Phase befindet (zwischen BeginInit und
    /// EndInit). Property-Setter lösen dann kein ApplyStyle aus.
    /// </summary>
    public bool IsEventsSuppressed => _suspendCount > 0;

    /// <summary>
    /// True, wenn die Strategie nichts anzeigt, aber ein einfacher Klick
    /// sofort eine Aktion auslöst. Es wird kein Edit-Control geöffnet und
    /// ein Doppelklick bleibt wirkungslos.
    /// </summary>
    public virtual bool IsInstantAction => false;

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

    /// <summary>
    /// Abstand des Controls vom linken Rand in Pixel.
    /// </summary>
    public int Left {
        get => Control?.Left ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Left = value; }
        }
    }

    /// <summary>
    /// Die zur Auswahl angebotenen Einträge.
    /// </summary>
    public List<ListItem>? ListItems {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Maximale Zeichenanzahl der Eingabe. 0 = unbegrenzt.
    /// </summary>
    public int MaxTextLength {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Minimale Zeichenanzahl der Eingabe. 0 = keine Prüfung.
    /// </summary>
    public int MinTextLength {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Erlaubt das Verschieben von Listeneinträgen.
    /// </summary>
    public bool MoveAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Erlaubt mehrzeilige Eingaben im Textfeld.
    /// </summary>
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

    /// <summary>
    /// Höhe des umgebenden Bereichs, z. B. der Tabellenzelle.
    /// </summary>
    public int ParentHeight {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Hilfetext, der beim Zeigen auf das Control angezeigt wird.
    /// </summary>
    public string QuickInfo {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    /// <summary>
    /// Verzögerung, bevor eine Wertänderung gemeldet wird.
    /// </summary>
    public int RaiseChangeDelay {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = 1;

    /// <summary>
    /// Regulärer Ausdruck, dem die Eingabe entsprechen muss.
    /// </summary>
    public string RegexCheck {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    } = string.Empty;

    /// <summary>
    /// Erlaubt das Entfernen von Listeneinträgen.
    /// </summary>
    public bool RemoveAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Text, der hinter dem Wert angezeigt wird.
    /// </summary>
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
    /// Ist <c>true</c>, ermittelt die TableView beim Start des
    /// Edits die Items zur content-Spalte/-Zeile und weist sie
    /// ListItems zu — einheitlich, unabhängig vom konkreten
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

    /// <summary>
    /// Erlaubt die direkte Texteingabe ins Control.
    /// </summary>
    public bool TextInputAllowed {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            if (!IsEventsSuppressed) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Abstand des Controls vom oberen Rand in Pixel.
    /// </summary>
    public int Top {
        get => Control?.Top ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Top = value; }
        }
    }

    /// <summary>
    /// Setzt den Wert im Control und löst ValueChanged aus.
    /// Prüft bewusst nicht IsDisposed: ForceWriteBackValue läuft
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

    /// <summary>
    /// Legt fest, ob das Control sichtbar ist.
    /// </summary>
    public bool Visible {
        get => Control is { Visible: true, IsDisposed: false };
        set {
            if (IsDisposed) { return; }
            if (Control is not { IsDisposed: false } c) { return; }
            if (!value) { ForceWriteBackValue(); }
            c.Visible = value;
        }
    }

    /// <summary>
    /// Breite des Controls in Pixel.
    /// </summary>
    public int Width {
        get => Control?.Width ?? 0;
        set {
            if (IsDisposed) { return; }
            if (Control is { } c) { c.Width = value; }
        }
    }

    /// <summary>
    /// Skalierung des Controls. 1 = normale Größe.
    /// </summary>
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

    /// <summary>
    /// Maximale Höhe, bis zu der ein einzeiliges Control das ganze Feld
    /// ausfüllt (der Optik wegen): das 2,5-Fache der einzeiligen Höhe.
    /// </summary>
    protected int MaxSingleLineFillHeight => (2.5f * SingleLineHeight).CanvasToControl(Zoom);

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die dauerhaft gecachte Prototyp-Instanz zur Strategy (KeyName).
    /// Unbekannte Schlüssel fallen wie bei CreateNew auf die
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

    /// <summary>
    /// Führt bei Instant-Action-Strategien (IsInstantAction) den
    /// einfachen Klick auf eine Zelle aus — die Rechte der Spalte werden geprüft.
    /// Liefert True, wenn der Klick verarbeitet wurde.
    /// </summary>
    public static bool InstantActionClicked(ColumnItem? column, RowItem? row) {
        if (column is not { IsDisposed: false } col || row is not { IsDisposed: false }) { return false; }
        if (col.Table is not { IsDisposed: false } tb) { return false; }

        // Fähigkeitsabfrage über den Prototyp; die konfigurierte Instanz wird frisch erzeugt.
        if (!Cached(col.ControlStrategy).IsInstantAction) { return false; }

        var strategy = CreateNew(col.ControlStrategy);
        strategy.ControlStrategyParameter = col.ControlStrategyParameter;

        if (strategy is IHasColumn hasColumn) { hasColumn.Column = col; }

        if (!tb.PermissionCheck(col.PermissionGroupsChangeCell, row, true)) {
            TableView.NotEditableInfo("Sie haben nicht die nötigen Rechte, um diese Aktion auszuführen.");
            return true;
        }

        strategy.ExecuteInstantAction(col, row);
        return true;
    }

    public void BeginInit() {
        if (IsDisposed) { return; }
        _suspendCount++;
    }

    /// <summary>
    /// Berechnet den benötigten Bereich des Controls anhand der aktuell
    /// gesetzten Eigenschaften (z. B. ListItems oder die
    /// Chip-Fläche der Suggestions). Strategien ohne besondere Anforderungen
    /// geben den übergebenen Bereich unverändert zurück; Strategien mit
    /// Überschriften (z. B. Tabellen) beginnen über der Zelllinie. Die
    /// TableView begrenzt das Ergebnis auf den sichtbaren Bereich.
    /// </summary>
    public virtual Rectangle CalculateRequiredBounds(Rectangle bounds) {
        if (Border == GroupBoxStyle.Nothing || _borderBox is not { IsDisposed: false } box) { return bounds; }

        // Rahmen-Insets ausgleichen, damit das eingebettete Control voll sichtbar bleibt.
        var display = box.DisplayRectangle;
        return new Rectangle(bounds.Location,
            new Size(bounds.Width + box.Width - display.Width, bounds.Height + box.Height - display.Height));
    }

    /// <summary>
    /// Erzeugt das Control der Strategie und gibt das anzuzeigende Control
    /// zurück — bei aktivem Rahmen die umgebende GroupBox.
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
    /// Meldung, warum die Konfiguration der Strategie ungültig ist. Leer, wenn sie gültig ist.
    /// </summary>
    public virtual string ErrorReason() => string.Empty;

    /// <summary>
    /// Setzt den Fokus auf das werttragende Control — bei aktivem Rahmen
    /// (Border ungleich Nothing) auf das eingebettete Control statt die Rahmen-Box.
    /// </summary>
    public void Focus() {
        if (IsDisposed) { return; }
        if (Border != GroupBoxStyle.Nothing && ControlCore is { } core) {
            core.Focus();
            return;
        }
        Control?.Focus();
    }

    /// <summary>
    /// Optionen der Strategie für den Property-Editor. Instant-Action-Strategien
    /// zeigen nichts an und bieten daher auch keinen Rahmen an. Leer, wenn die
    /// Strategie keine konfigurierbaren Optionen hat.
    /// </summary>
    public virtual List<GenericControl> GetProperties(int widthOfControl) {
        if (IsInstantAction) { return []; }
        return [new FlexiControlForProperty<GroupBoxStyle>(() => Border, "Rahmen", ItemsOf(typeof(GroupBoxStyle)))];
    }

    public virtual void HandleCaptionClick() { }

    public virtual Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Prüft, ob die Strategie zu einer Spalte mit den übergebenen
    /// Bearbeitungs-Fähigkeiten passt. Basis-Implementierung vergleicht
    /// gegen SupportsTextEdit und SupportsSuggestions.
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
    public abstract string ReadableText();

    /// <summary>
    /// Stellt den Default-Zustand des Controls wieder her (z. B. Zeilen löschen,
    /// Scroll-Position zurücksetzen). Wird vor jeder Anzeige aufgerufen, da
    /// Strategien mitsamt Control wiederverwendet werden.
    /// </summary>
    public virtual void Reset() => ForceWriteBackValue();

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
    /// Führt die eigentliche Aktion des Klicks aus; nur das Ausführen wird an
    /// die Aufruf-Ebene (z. B. TableView.DoScript) übergeben.
    /// </summary>
    protected virtual void ExecuteInstantAction(ColumnItem column, RowItem row) { }

    /// <summary>
    /// Holt den aktuellen Wert vom Control und schreibt ihn über den
    /// Value-Setter zurück, sodass ValueChanged ausgelöst wird.
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
    protected virtual void ReadParameters(JsonObject json) => Border = json.GetEnum(_borderKey, Border);

    protected abstract void SetValueToControlInternal(string value);

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnValueChanged(string newvalue) => ValueChanged?.Invoke(this, new TextEventArgs(newvalue));

    /// <summary>
    /// Entfernt die Rahmen-Box und bettet das Control wieder direkt beim bisherigen Parent der Box ein.
    /// </summary>
    private void RemoveBorderBox(GroupBox box) {
        if (ControlCore is { IsDisposed: false } inner) {
            var parent = box.Parent;
            var bounds = box.Bounds;
            var anchor = box.Anchor;

            inner.Dock = System.Windows.Forms.DockStyle.None;
            box.Controls.Remove(inner);

            if (parent is { IsDisposed: false }) {
                inner.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                inner.Anchor = anchor;
                parent.Controls.Add(inner);
            }
        }
        box.Visible = false;
        box.Dispose();
        _borderBox = null;
    }

    #endregion
}