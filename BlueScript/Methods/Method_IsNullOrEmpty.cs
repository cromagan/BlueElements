// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_IsNullOrEmpty : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "isnullorempty";
    public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist oder keinen Inhalt hat.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "isNullOrEmpty(Variable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);

        if (attvar.Attributes.Count == 0) {
            return attvar.ScriptIssueType != ScriptIssueType.VariableNichtGefunden
                ? DoItFeedback.AttributFehler(infos.LogData, attvar)
                : DoItFeedback.Wahr();
        }

        var v = attvar.Attributes[0];
        if (v is null) { return DoItFeedback.InternerFehler(null); }

        if (v.IsNullOrEmpty) { return DoItFeedback.Wahr(); }

        if (v is VariableUnknown) { return DoItFeedback.Wahr(); }

        return DoItFeedback.Falsch();
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}