// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Berechnet die Quadartwurzel.
/// </summary>
internal class SqrtScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "sqrt";

    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "Sqrt(NumberScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(Math.Sqrt(attvar.ValueNumGet(0)));

    #endregion
}
