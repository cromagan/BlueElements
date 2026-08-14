// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class StringScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[DoubleScriptVariable.ShortName_Plain, StringScriptVariable.ShortName_Plain]];
    public override string Command => "string";
    public override string Description => "Wandelt die Zahl in einen Text um. Kulanterweise werden Strings einfach als StringScriptCommand weitergegeben.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "StringScriptCommand(numeral)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ReadableText(0));

    #endregion
}