// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Legt eine neue Variable an, der Typ wird automatisch bestimmt.
/// Ist die Variable bereits vorhanden, wird der Befehl ignoriert.
/// </summary>
internal class VarTempScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[ScriptVariable.Any_Plain], [ScriptVariable.Any_Plain]];
    public override string Command => "vartemp";
    public override string Syntax => "VarTemp(VariablenName, Wert);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attributes = SplitAttributeToString(infos.AttributText);
        if (CheckArgumentCount(Command, attributes, Args.Count, LastArgMinCount) is { Length: > 0 } countError) { return new DoItFeedback(countError, true); }
        if (attributes is not { Count: 2 }) { return DoItFeedback.InternerFehler(); }

        var varnam = attributes[0];
        if (!ScriptVariable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true); }

        // Variable bereits vorhanden: Befehl ignorieren.
        if (varCol.GetByKey(varnam) is not null) { return DoItFeedback.Null(); }

        return VariablenBerechnung(varCol, scp, varnam + "=" + attributes[1] + ";", true);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.
        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}
