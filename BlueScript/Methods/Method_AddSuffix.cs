// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_AddSuffix : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, StringVal];
    public override string Command => "addsuffix";
    public override string Description => "Fügt am Ende jedes Listenobjekts einen Text hinzu.";
    public override string Syntax => "AddSuffix(VariableListe, SuffixText)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);

        for (var z = 0; z < tmpList.Count; z++) {
            tmpList[z] += attvar.ValueStringGet(1);
        }

        return attvar.ValueListStringSet(0, tmpList, ld) ?? DoItFeedback.Null();
    }

    #endregion
}