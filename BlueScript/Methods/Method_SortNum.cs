// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Converter;

namespace BlueScript.Methods;


internal class Method_SortNum : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, FloatVal];
    public override string Command => "sortnum";
    public override string Description => "Sortiert die Liste. Der Zahlenwert wird verwendet wenn der String nicht in eine Zahl umgewandelt werden kann.";

    public override string Syntax => "SortNum(ListVariable, Defaultwert);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var nums = new List<double>();
        foreach (var txt in attvar.ValueListStringGet(0)) {
            nums.Add(txt.IsNumeral() ? DoubleParse(txt) : attvar.ValueNumGet(1));
        }

        nums.Sort();

        if (attvar.Attributes[0] is not VariableListString vli) {
            return DoItFeedback.AttributFehler(ld, attvar);
        }

        vli.ValueList = nums.ConvertAll(i => i.ToString1_5());
        return DoItFeedback.Null();
    }

    #endregion
}