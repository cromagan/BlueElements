// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt eine nicht negative Zufalls-Ganzzahl zurück,
/// die kleiner als das angegebene Maximum ist.
/// </summary>
internal class RandomIntScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FloatVal];
    public override string Command => "randomint";
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "RandomIntScriptCommand(maxValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(GlobalRnd.Next(0, attvar.ValueIntGet(0)));

    #endregion
}
