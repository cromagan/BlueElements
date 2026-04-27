// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Add : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableDouble.ShortName_Plain]];
    public override string Command => "add";
    public override List<string> Constants => [];
    public override string Description => "Fügt einer Liste einen oder mehrere Werte hinzu.\r\nZahlen werden in Text (max. 5 Nachkommastellen) umgewandelt";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Add(ListVariable, Value1, Value2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);
        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) {
                tmpList.Add(vs.ValueString);
            }
            if (attvar.Attributes[z] is VariableListString vl) {
                tmpList.AddRange(vl.ValueList);
            }
            if (attvar.Attributes[z] is VariableDouble vf) {
                tmpList.Add(vf.ValueForReplace);
            }
        }

        return attvar.ValueListStringSet(0, tmpList, ld) ?? DoItFeedback.Null();
    }

    #endregion
}