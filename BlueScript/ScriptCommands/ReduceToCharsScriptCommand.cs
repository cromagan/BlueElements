// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Entfernt aus dem Text alle Zeichen die nicht erlaubt sind
/// </summary>
internal class ReduceToCharsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "reducetochars";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ReduceToChars(OriginalString, ErlaubteZeichenString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).ReduceToChars(attvar.ValueStringGet(1)));

    #endregion
}
