// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_AddPrefix : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, StringVal];
    public override string Command => "addprefix";
    public override List<string> Constants => [];
    public override string Description => "Fügt am Anfang jedes Listenobjekts einen Text hinzu.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "AddPrefix(VariableListe, PrefixText)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);

        for (var z = 0; z < tmpList.Count; z++) {
            tmpList[z] = attvar.ReadableText(1) + tmpList[z];
        }

        return attvar.ValueListStringSet(0, tmpList, ld) ?? DoItFeedback.Null();
    }

    #endregion
}