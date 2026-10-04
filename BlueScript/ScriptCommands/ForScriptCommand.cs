// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Führt den Codeblock mehrfach aus, wobei die Zählvariable von Min bis Max in Schritten von Step verändert wird.
/// Die Zählvariable darf noch nicht deklariert sein und ist nur innerhalb des Codeblocks verfügbar.
/// Ist Step negativ, wird von Min abwärts bis Max gezählt.
/// Mit Break kann die Schleife vorab verlassen werden.
/// Der Codeblock wird eine Skript-Stufe tiefer ausgeführt (Verschachtelungslimit: 10).
/// </summary>
internal class ForScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[UnknownScriptVariable.ClassId], [DoubleScriptVariable.ClassId], [DoubleScriptVariable.ClassId], [DoubleScriptVariable.ClassId]];
    public override string Command => "for";
    public override bool GetCodeBlockAfter => true;
    public override string Syntax => "For(Variable, Min, Max, Step) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(attvar); }

        var varnam = attvar.Attributes[0] is UnknownScriptVariable vkn ? vkn.Value : string.Empty;

        if (!ScriptVariable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true); }

        var vari = varCol.GetByKey(varnam);
        if (vari is not null) { return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true); }

        var step = attvar.ValueNumGet(3);
        if (step == 0) { return new DoItFeedback("Step darf nicht 0 sein.", true); }

        var min = attvar.ValueNumGet(1);
        var max = attvar.ValueNumGet(2);

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, BreakScriptCommand.Method], scp.Stufe + 1, scp.Chain);

        var v = min;
        while (step > 0 ? v <= max : v >= max) {
            var addme = new List<ScriptVariable>() { new DoubleScriptVariable(varnam, v, true, "Iterations-Variable") };

            scx = CallByFilenameScriptCommand.CallSub(varCol, scp2, infos.CodeBlockAfterText, infos.Line - 1, infos.Subname, addme, null, "For");
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }

            v += step;
        }

        if (scx is null) { return new DoItFeedback(); }

        scx.ConsumeBreak(); // Muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
        return scx;
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}