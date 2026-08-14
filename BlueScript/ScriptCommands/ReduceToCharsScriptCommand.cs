// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class ReduceToCharsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "reducetochars";
    public override string Description => "Entfernt aus dem Text alle Zeichen die nicht erlaubt sind";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ReduceToCharsScriptCommand(OriginalString, ErlaubteZeichenString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).ReduceToChars(attvar.ValueStringGet(1)));

    #endregion
}