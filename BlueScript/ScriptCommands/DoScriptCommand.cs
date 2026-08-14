// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DoScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "do";
    public override string Description => "Führt den Codeblock dauerhaft aus, bis der Befehl Break empfangen wurde. Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.\r\nDie Variable INDEX zeigt an, bei welchen Eintrag der Zeiger sich gerade befindet.";
    public override bool GetCodeBlockAfter => true;
    public override string StartSequence => string.Empty;
    public override string Syntax => "Do { Break; }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(attvar); }

        var index = -1;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, BreakScriptCommand.Method], scp.Stufe, scp.Chain);

        ScriptEndedFeedback scx;

        do {
            index++;
            if (index > 100000) { return new DoItFeedback("Do-Schleife nach 100.000 Durchläufen abgebrochen.", true); }

            var addme = new List<ScriptVariable>() { new DoubleScriptVariable("Index", index, true, "Iterations-Variable") };
            scx = CallByFilenameScriptCommand.CallSub(varCol, scp2, infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, addme, null, "Do");
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }
        } while (true);

        scx.ConsumeBreak();// Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
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