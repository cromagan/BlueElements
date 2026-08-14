// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class CalculateScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "calculate";
    public override string Description => "Berechet die Formel im ScriptVariables.String. Falls die Berechung fehlschlägt, wird NaN-Value zurückgegeben.";
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "Calculate(string, NaNValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (MathFormulaParser.Ergebnis(attvar.ValueStringGet(0)) is { } dbl) {
            return new DoItFeedback(dbl);
        }

        return new DoItFeedback(attvar.ValueNumGet(1));
    }

    #endregion
}