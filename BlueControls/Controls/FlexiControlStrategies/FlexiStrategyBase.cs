// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;

namespace BlueControls.Controls.FlexiControlStrategies;

public abstract class FlexiStrategyBase {

    #region Events

    public event EventHandler? ButtonClicked;

    public event EventHandler? DropDownShowing;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    public abstract System.Windows.Forms.Control? Control { get; }

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

    public virtual void StyleControl(string caption, IInputFormat? inputFormat, int delay, List<AbstractListItem>? items, EditTypeTable userEditDialogType, bool editableWithTextInput, bool editableWithDropdown, bool showValuesOfOtherCellsInDropdown, IReadOnlyList<string>? dropdownItems, IReadOnlySet<string>? customVocabulary, int parentHeight, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) { }

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    protected void OnButtonClicked() => ButtonClicked?.Invoke(this, System.EventArgs.Empty);

    protected void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    protected void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, new NavigationDirectionEventArgs(e));

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected abstract void SetValueToControl();

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}