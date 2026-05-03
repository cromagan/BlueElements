// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public interface IFlexiStrategy {

    #region Properties

    System.Windows.Forms.Control? Control { get; }

    #endregion

    #region Methods

    public static IFlexiStrategy? GetStrategy(EditTypeFormula editType) {
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

    void CreateControl(FlexiControl owner);

    void SetValue(FlexiControl owner, string value);

    void SubscribeEvents(FlexiControl owner);

    void UnsubscribeEvents();

    #endregion
}