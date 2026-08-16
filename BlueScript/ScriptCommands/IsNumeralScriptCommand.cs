// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class IsNumeralScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, DoubleScriptVariable.ShortName_Plain]];
    public override string Command => "isnumeral";
    public override string Description => "Prüft, ob der Inhalt der Variable eine gültige Zahl ist. ";
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "isNumeral(Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is DoubleScriptVariable) { return DoItFeedback.Wahr(); }
        if (attvar.Attributes[0] is StringScriptVariable vs) {
            if (vs.ValueString.IsNumeral()) { return DoItFeedback.Wahr(); }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}