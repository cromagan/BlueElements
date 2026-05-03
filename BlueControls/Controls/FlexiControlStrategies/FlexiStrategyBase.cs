// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueControls.Controls.FlexiControlStrategies;

public abstract class FlexiStrategyBase : IInputFormat {

    #region Events

    public event EventHandler? DropDownShowing;

    public event EventHandler? ExecuteComand;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public string AllowedChars {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public string Caption {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public abstract System.Windows.Forms.Control? Control { get; }

    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool DropdownAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public IReadOnlyList<string>? DropdownItems {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public List<AbstractListItem>? ListItems {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public int MaxTextLength {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool MultiLine {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public int RaiseChangeDelay {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    } = 1;

    public string RegexCheck {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool ShowValuesOfOtherCellsInDropdown {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool SpellCheckingEnabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool TextFormatingAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public bool TextInputAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
        }
    }

    public EditTypeTable UserEditDialogType {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ApplyStyle();
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

            OnValueChanged();
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

    public abstract void CreateControl();

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    protected abstract void ApplyStyle();

    protected void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    protected void OnExecuteComand() => ExecuteComand?.Invoke(this, System.EventArgs.Empty);

    protected void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected abstract void SetValueToControl();

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}