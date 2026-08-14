// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class RandomIntScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FloatVal];
    public override string Command => "randomint";
    public override string Description => "Gibt eine nicht negative Zufalls-Ganzzahl zurück,\rdie kleiner als das angegebene Maximum ist.";
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "RandomIntScriptCommand(maxValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(GlobalRnd.Next(0, attvar.ValueIntGet(0)));

    #endregion
}