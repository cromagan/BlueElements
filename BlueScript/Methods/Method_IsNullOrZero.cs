// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_IsNullOrZero : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "isnullorzero";
    public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist, keinen Inhalt hat, oder dem Zahlenwert 0 entspricht. Falls die Variable existiert, muss diese dem Typ Numeral entsprechen.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "isNullOrZero(Variable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);

        if (attvar.Attributes.Count == 0) {
            if (attvar.ScriptIssueType != ScriptIssueType.VariableNichtGefunden) {
                return DoItFeedback.AttributFehler(infos.LogData, attvar);
            }

            return DoItFeedback.Wahr();
        }

        var v = attvar.Attributes[0];
        if (v is null) { return DoItFeedback.InternerFehler(infos.LogData); }

        if (v.IsNullOrEmpty) { return DoItFeedback.Wahr(); }
        if (v is VariableUnknown) { return DoItFeedback.Wahr(); }

        if (v is VariableDouble f) {
            if (f.ValueNum == 0) { return DoItFeedback.Wahr(); }
            return DoItFeedback.Falsch();
        }

        return new DoItFeedback("Variable existiert, ist aber nicht vom Datentyp Numeral.", true, infos.LogData);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}