// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Fügt am Ende jedes Listenobjekts einen Text hinzu.
/// </summary>
internal class AddSuffixScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, StringVal];
    public override string Command => "addsuffix";
    public override string Syntax => "AddSuffix(VariableListe, SuffixText)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        var tmpList = attvar.ValueListStringGet(0);

        for (var z = 0; z < tmpList.Count; z++) {
            tmpList[z] += attvar.ValueStringGet(1);
        }

        return attvar.ValueListStringSet(0, tmpList) ?? DoItFeedback.Null();
    }

    #endregion
}
