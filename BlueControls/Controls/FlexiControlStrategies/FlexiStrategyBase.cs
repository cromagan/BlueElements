// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

public abstract class FlexiStrategyBase {

    #region Events

    public event EventHandler? ButtonClicked;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler<StrategyValueChangedEventArgs>? ValueChanged;

    #endregion

    #region Properties

    public abstract System.Windows.Forms.Control? Control { get; }

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

    public abstract void SetValue(string value);

    public virtual void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) { }

    public abstract void SubscribeEvents();

    public abstract void UnsubscribeEvents();

    protected void OnButtonClicked() => ButtonClicked?.Invoke(this, System.EventArgs.Empty);

    protected void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected void OnValueChanged(string value, bool updateControls = false) => ValueChanged?.Invoke(this, new StrategyValueChangedEventArgs(value, updateControls));

    #endregion
}