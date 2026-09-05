// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Ist das erste Argument ein Text, wird gezählt, wie oft der Suchstring im Text vorkommt.
/// Ist es eine Liste, wird gezählt, wie oft ein Listeneintrag dem Text entspricht.
/// Achtung: Groß/Kleinschreibung wird beachtet!
/// </summary>
internal class CountStringScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Variable, ListOfStringsScriptVariable.ShortName_Variable], StringVal];
    public override string Command => "countstring";

    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "CountString(Text/Liste, Suchstring)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        switch (attvar.Attributes[0]) {
            case StringScriptVariable vs:
                return new DoItFeedback(vs.ValueString.CountString(attvar.ValueStringGet(1)));

            case ListOfStringsScriptVariable vl:
                return new DoItFeedback(vl.ValueList.Count(s => s == attvar.ValueStringGet(1)));
        }

        return DoItFeedback.InternerFehler();
    }

    #endregion
}
