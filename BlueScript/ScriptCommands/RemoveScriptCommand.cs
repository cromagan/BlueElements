// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class RemoveScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal, [StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain]];
    public override string Command => "remove";
    public override string Description => "Entfernt aus der Liste die angegebenen Werte.\r\nIst der Wert nicht in der Liste, wird kein Fehler ausgelöst.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override string Syntax => "RemoveScriptCommand(ListVariable, CaseSensitive, Value1, Value2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        var tmpList = attvar.ValueListStringGet(0);
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is StringScriptVariable vs) {
                tmpList.RemoveString(vs.ValueString, attvar.ValueBoolGet(1));
            } else if (attvar.Attributes[z] is ListOfStringsScriptVariable vl) {
                tmpList.RemoveString(vl.ValueList, attvar.ValueBoolGet(1));
            }
        }
        return attvar.ValueListStringSet(0, tmpList) ?? DoItFeedback.Null();
    }

    #endregion
}