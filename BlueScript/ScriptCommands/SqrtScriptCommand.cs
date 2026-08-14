// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class SqrtScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "sqrt";

    public override string Description => "Berechnet die Quadartwurzel.";

    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "SqrtScriptCommand(NumberScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(Math.Sqrt(attvar.ValueNumGet(0)));

    #endregion
}