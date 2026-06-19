// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_Exists : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "exists";
    public override string Description => "Gibt TRUE zurück, wenn die Variable existiert.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "Exists(Variable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);

        if (attvar.Failed) {
            // Während des SyntaxChecks eine fehlende Variable als Dummy registrieren,
            // damit nachfolgender Code innerhalb eines Bodies - etwa if(exists(x), ... x verwenden ...) - validierbar bleibt.
            if (attvar.ScriptIssueType == ScriptIssueType.VariableNichtGefunden) {
                RegisterSyntaxCheckDummyVariable(varCol, scp, infos.AttributText);
            }
            return DoItFeedback.Falsch();
        }
        return DoItFeedback.Wahr();
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}