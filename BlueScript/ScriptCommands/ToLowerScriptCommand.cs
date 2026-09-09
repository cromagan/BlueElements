// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Gibt den Text in Kleinbuchstaben zurück
/// </summary>
internal class ToLowerScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "tolower";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ToLower(OriginalString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).ToLowerInvariant());

    #endregion
}
