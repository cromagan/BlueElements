// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Sortiert die Liste. Falls das zweite Attribut TRUE ist, werden Doubletten und leere Einträge entfernt.
/// </summary>
internal class SortScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal];
    public override string Command => "sort";
    public override string Syntax => "Sort(ListVariable, EliminateDupes);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        if (attvar.Attributes[0] is not ListOfStringsScriptVariable vli) {
            return DoItFeedback.AttributFehler(attvar);
        }

        var x = attvar.ValueListStringGet(0);
        if (attvar.ValueBoolGet(1)) {
            x = x.SortedDistinctList();
        } else {
            x.Sort();
        }

        vli.ValueList = x;
        return DoItFeedback.Null();
    }

    #endregion
}
