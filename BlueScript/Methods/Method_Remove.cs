// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Remove : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "remove";
    public override List<string> Constants => [];
    public override string Description => "Entfernt aus der Liste die angegebenen Werte.\r\nIst der Wert nicht in der Liste, wird kein Fehler ausgelöst.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Remove(ListVariable, CaseSensitive, Value1, Value2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) {
                tmpList.RemoveString(vs.ValueString, attvar.ValueBoolGet(1));
            }
            if (attvar.Attributes[z] is VariableListString vl) {
                tmpList.RemoveString(vl.ValueList, attvar.ValueBoolGet(1));
            }
        }
        return attvar.ValueListStringSet(0, tmpList, ld) ?? DoItFeedback.Null();
    }

    #endregion
}