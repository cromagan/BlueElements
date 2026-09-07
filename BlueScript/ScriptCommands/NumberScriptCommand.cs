// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird NaN-Value zurückgegeben.
/// </summary>
internal class NumberScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "number";
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "NumberScriptCommand(string, NaNValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (DoubleTryParse(attvar.ValueStringGet(0), out var dbl)) {
            return new DoItFeedback(dbl);
        }

        return new DoItFeedback(attvar.ValueNumGet(1));
    }

    #endregion
}
