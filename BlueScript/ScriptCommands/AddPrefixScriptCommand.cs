// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Fügt am Anfang jedes Listenobjekts einen Text hinzu.
/// </summary>
internal class AddPrefixScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, StringVal];
    public override string Command => "addprefix";
    public override string Syntax => "AddPrefix(VariableListe, PrefixText)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        var tmpList = attvar.ValueListStringGet(0);

        for (var z = 0; z < tmpList.Count; z++) {
            tmpList[z] = attvar.ReadableText(1) + tmpList[z];
        }

        return attvar.ValueListStringSet(0, tmpList) ?? DoItFeedback.Null();
    }

    #endregion
}
