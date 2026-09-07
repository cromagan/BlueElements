// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Wandelt die Zahl in einen Text um. Kulanterweise werden Strings einfach als StringScriptCommand weitergegeben.
/// </summary>
internal class StringScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[DoubleScriptVariable.ShortName_Plain, StringScriptVariable.ShortName_Plain]];
    public override string Command => "string";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "StringScriptCommand(numeral)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ReadableText(0));

    #endregion
}
