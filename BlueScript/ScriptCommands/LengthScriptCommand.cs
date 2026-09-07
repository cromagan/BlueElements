// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Gibt die Anzahl der Zeichen des Strings zurück
/// </summary>
internal class LengthScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "length";

    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "LengthScriptCommand(StringScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).Length);

    #endregion
}
