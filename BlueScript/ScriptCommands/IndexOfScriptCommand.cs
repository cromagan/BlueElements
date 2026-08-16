// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class IndexOfScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Variable, ListOfStringsScriptVariable.ShortName_Variable], BoolVal, [StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];

    public override string Command => "indexof";

    public override string Description => "Bei String:\r\nSucht im ersten String nach dem zweiten String und gibt dessen Position zurück.\r\nBei Listen:\r\nSucht in der Liste den zweiten ScriptVariables.String.\r\nAllgemein:\r\nWird er nicht gefunden, wird -1 zurück gegeben. Wird er an erster Position gefunden, wird 0 zurück gegeben.";

    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "IndexOf(ListVariable/StringVariable, CaseSensitive, Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var search = attvar.ValueStringGet(2);
        var sens = attvar.ValueBoolGet(1) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var pos = -1;

        if (attvar.Attributes[0] is StringScriptVariable v) {
            pos = v.ValueString.IndexOf(search, sens);
        } else if (attvar.Attributes[0] is ListOfStringsScriptVariable vl) {
            pos = vl.ValueList.FindIndex(x => x.Equals(search, sens));
        }

        return new DoItFeedback(pos);
    }

    #endregion
}