// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Wandelt einen Text in eine Liste um.
/// Es trennt den Text dabei mitteles dem angegebenen Trennzeichen.
/// </summary>
internal class SplitScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "split";
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ShortName_Plain;
    public override string Syntax => "SplitScriptCommand(StringScriptCommand, Trennzeichen)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).SplitBy(attvar.ValueStringGet(1)));

    #endregion
}
