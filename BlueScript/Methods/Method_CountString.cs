// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_CountString : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable], StringVal];
    public override string Command => "countstring";

    public override string Description => "Ist das erste Argument ein Text, wird gezählt, wie oft der Suchstring im Text vorkommt.\r\n" +
        "Ist es eine Liste, wird gezählt, wie oft ein Listeneintrag dem Text entspricht.\r\n" +
        "Achtung: Groß/Kleinschreibung wird beachtet!";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "CountString(Text/Liste, Suchstring)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        switch (attvar.Attributes[0]) {
            case VariableString vs:
                return new DoItFeedback(vs.ValueString.CountString(attvar.ValueStringGet(1)));

            case VariableListString vl:
                return new DoItFeedback(vl.ValueList.Count(s => s == attvar.ReadableText(1)));
        }

        return DoItFeedback.InternerFehler(ld);
    }

    #endregion
}