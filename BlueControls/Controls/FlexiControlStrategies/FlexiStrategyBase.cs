// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Controls.FlexiControlStrategies;

public abstract class FlexiStrategyBase : IInputFormat, IDisposableExtended {

    #region Fields

    private bool _initializing;
    private volatile int _isDisposedFlag;

    #endregion

    #region Events

    public event EventHandler? DropDownShowing;

    public event EventHandler? ExecuteComand;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? ValueChanged;

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

    public bool DropdownAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    }

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

    public bool ShowValuesOfOtherCellsInDropdown {
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

    public string Suffix {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (!_initializing) { ApplyStyle(); }
        }
    } = string.Empty;

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

    public string Value {
        get;
        set {
            if (value == field) { return; }

            UnsubscribeEvents();

            field = value;

            SetValueToControl();

            SubscribeEvents();

            if (!_initializing) { OnValueChanged(); }
        }
    } = string.Empty;

    #endregion

    #region Methods

    public static FlexiStrategyBase? GetStrategy(EditTypeFormula editType) {
        return editType switch {
            EditTypeFormula.Textfeld => new FlexiStrategyTextBox(),
            EditTypeFormula.Textfeld_mit_Auswahlknopf => new FlexiStrategyComboBox(),
            EditTypeFormula.Listbox => new FlexiStrategyListBox(),
            EditTypeFormula.SwapListBox => new FlexiStrategySwapListBox(),
            EditTypeFormula.Ja_Nein_Knopf => new FlexiStrategyButtonYesNo(),
            EditTypeFormula.Button => new FlexiStrategyButtonCommand(),
            EditTypeFormula.Farb_Auswahl_Dialog => new FlexiStrategyButtonColor(),
            EditTypeFormula.Line => new FlexiStrategyLine(),
            EditTypeFormula.als_Überschrift_anzeigen => new FlexiStrategyGroupBox(),
            EditTypeFormula.nur_als_Text_anzeigen => new FlexiStrategyCaption(),
            _ => null
        };
    }

    public void BeginInit() => _initializing = true;

    public abstract void CreateControl();

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }

        UnsubscribeEvents();

        DropDownShowing = null;
        ExecuteComand = null;
        ItemRemoved = null;
        NavigateToNext = null;
        ValueChanged = null;

        if (Control is { IsDisposed: false } control) {
            control.Visible = false;
            control.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public void EndInit() {
        if (!_initializing) { return; }
        _initializing = false;
        ApplyStyle();
    }

    public virtual void HandleCaptionClick() { }

    public virtual Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) => Task.CompletedTask;

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    public virtual bool WasValueClicked() => false;

    protected abstract void ApplyStyle();

    protected void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    protected void OnExecuteComand() => ExecuteComand?.Invoke(this, System.EventArgs.Empty);

    protected void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected abstract void SetValueToControl();

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}