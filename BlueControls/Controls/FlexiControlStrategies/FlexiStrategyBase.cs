// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Controls.FlexiControlStrategies;

public abstract class FlexiStrategyBase : IInputFormat, IDisposableExtended, ISupportInitialize {

    #region Fields

    private bool _initializing;
    private volatile int _isDisposedFlag;

    #endregion

    #region Events

    public event EventHandler? Disposed;

    public event EventHandler? DropDownShowing;

    public event EventHandler? EnterKey;

    public event EventHandler? EscKey;

    public event EventHandler? ExecuteCommand;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler? LostFocus;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? TabKey;

    public event EventHandler<TextEventArgs>? ValueChanged;

    #endregion

    #region Properties

    public AddType AddAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public string AllowedChars {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public bool AutoSort {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public string Caption {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public CheckBehavior CheckBehavior {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public abstract System.Windows.Forms.Control? Control { get; }

    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public IReadOnlySet<string>? CustomVocabulary {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public string ForbiddenChars {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public string ImageCode {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public bool IsDisposed => _isDisposedFlag == 1;

    public List<AbstractListItem>? ListItems {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public int MaxTextLength {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public int MinTextLength {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public bool MoveAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public bool MultiLine {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public int ParentHeight {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public string QuickInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public int RaiseChangeDelay {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = 1;

    public string RegexCheck {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public bool RemoveAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public bool SpellCheckingEnabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    /// <summary>
    /// Generischer Strategie-Parameter, der pro Strategy unterschiedlich verwendet wird.
    /// Bei der CSV-Table-Strategy z. B. die Spaltennamen, getrennt mit ";".
    /// </summary>
    public string StrategyParameter {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public string Suffix {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

    public SuggestionPosition SuggestionPosition {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = SuggestionPosition.Bottom;

    /// <summary>
    /// Gibt an, ob diese Strategie eine Auswahlliste (Vorschläge) handhaben kann.
    /// Ist <c>true</c>, ermittelt die <see cref="TableView" /> beim Start des
    /// Edits die Items zur content-Spalte/-Zeile und weist sie
    /// <see cref="ListItems" /> zu — einheitlich, unabhängig vom konkreten
    /// Strategie-Typ. Hat der Aufrufer bereits Items übergeben, werden diese
    /// übernommen. Default <c>false</c>.
    /// </summary>
    public virtual bool SupportsSuggestions => false;

    public bool TextFormatingAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public bool TextInputAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    public EditTypeTable UserEditDialogType {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

    [DefaultValue(1f)]
    public float Zoom {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = 1f;

    #endregion

    #region Methods

    public static FlexiStrategyBase? GetStrategy(EditTypeFormula editType) {
        return editType switch {
            EditTypeFormula.Textfeld => new FlexiStrategyTextBox(),
            EditTypeFormula.Textfeld_mit_Suggestions => new FlexiStrategyTextBoxSuggestions(),
            EditTypeFormula.Textfeld_mit_Auswahlknopf => new FlexiStrategyComboBox(),
            EditTypeFormula.Listbox => new FlexiStrategyListBox(),
            EditTypeFormula.SwapListBox => new FlexiStrategySwapListBox(),
            EditTypeFormula.Ja_Nein_Knopf => new FlexiStrategyButtonYesNo(),
            EditTypeFormula.Button => new FlexiStrategyButtonCommand(),
            EditTypeFormula.Farb_Auswahl_Dialog => new FlexiStrategyButtonColor(),
            EditTypeFormula.Line => new FlexiStrategyLine(),
            EditTypeFormula.als_Überschrift_anzeigen => new FlexiStrategyGroupBox(),
            EditTypeFormula.nur_als_Text_anzeigen => new FlexiStrategyCaption(),
            EditTypeFormula.CSV_Table => new FlexiStrategyCsvTable(),
            _ => null
        };
    }

    public void BeginInit() => _initializing = true;

    /// <summary>
    /// Berechnet die für das Control benötigte Größe anhand der aktuell
    /// gesetzten Eigenschaften (z. B. <see cref="ListItems" /> oder die
    /// Chip-Fläche der Suggestions). Strategien, die keine besondere Größe
    /// fordern, geben die übergebene Größe unverändert zurück. Die TableView
    /// ruft diese Methode einheitlich auf und fragt dabei weder den
    /// konkreten Strategy-Typ noch das Control ab.
    /// </summary>
    public virtual Size CalculateRequiredSize(int minWidth, int minHeight) => new(minWidth, minHeight);

    public abstract void CreateControl();

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void EndInit() {
        if (!_initializing) { return; }
        _initializing = false;
        ApplyStyle();
    }

    public virtual void HandleCaptionClick() { }

    public virtual Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    public void OnValueChanged(string newvalue) => ValueChanged?.Invoke(this, new TextEventArgs(newvalue));

    /// <summary>
    /// Setz den Wert zum Control und löst kein Event aus
    /// </summary>
    /// <param name="value"></param>
    public void SetValueToControl(string value) {
        UnsubscribeEvents();

        SetValueToControlInternal(value);

        SubscribeEvents();
    }

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    public virtual bool WasValueClicked() => false;

    protected abstract void ApplyStyle();

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();

            UnsubscribeEvents();

            DropDownShowing = null;
            EnterKey = null;
            EscKey = null;
            ExecuteCommand = null;
            ItemRemoved = null;
            LostFocus = null;
            NavigateToNext = null;
            TabKey = null;
            ValueChanged = null;
            Disposed = null;

            if (Control is { IsDisposed: false } control) {
                control.Visible = false;
                control.Dispose();
            }
        }
    }

    protected void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    protected void OnEnterKey() => EnterKey?.Invoke(this, System.EventArgs.Empty);

    protected void OnEscKey() => EscKey?.Invoke(this, System.EventArgs.Empty);

    protected void OnExecuteCommand() => ExecuteCommand?.Invoke(this, System.EventArgs.Empty);

    protected void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    protected void OnLostFocus() => LostFocus?.Invoke(this, System.EventArgs.Empty);

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected void OnTabKey() => TabKey?.Invoke(this, System.EventArgs.Empty);

    protected abstract void SetValueToControlInternal(string value);

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion
}