// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Sortiert die Liste. Der Zahlenwert wird verwendet wenn der StringScriptCommand nicht in eine Zahl umgewandelt werden kann.
/// </summary>
internal class SortNumScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, FloatVal];
    public override string Command => "sortnum";

    public override string Syntax => "SortNum(ListVariable, Defaultwert);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        var nums = new List<double>();
        foreach (var txt in attvar.ValueListStringGet(0)) {
            nums.Add(txt.IsNumeral() ? DoubleParse(txt) : attvar.ValueNumGet(1));
        }

        nums.Sort();

        if (attvar.Attributes[0] is not ListOfStringsScriptVariable vli) {
            return DoItFeedback.AttributFehler(attvar);
        }

        vli.ValueList = nums.ConvertAll(i => i.ToString1_5());
        return DoItFeedback.Null();
    }

    #endregion
}
