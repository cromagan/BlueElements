// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

internal class ForEachRowScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[UnknownScriptVariable.ShortName_Plain], FilterVar];
    public override string Command => "foreachrow";
    public override string Description => "Führt den Codeblock für jede gefundene Zeile aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt, diese darf noch nicht deklariert sein.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.\r\nDie Variable INDEX zeigt an, bei welchen Eintrag der Zeiger sich gerade befindet.";
    public override bool GetCodeBlockAfter => true;
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "ForEachRowScriptCommand(NeueVariable, Filter1, ...) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(attvar); }

        var varnam = "value";

        if (attvar.Attributes[0] is UnknownScriptVariable vkn) { varnam = vkn.Value; }

        if (!ScriptVariable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true); }

        var vari = varCol.GetByKey(varnam, StringComparison.OrdinalIgnoreCase);
        if (vari is not null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true);
        }

        var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 1, MyTable(scp), scp.ScriptName, true);

        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {failedReason}", needsScriptFix); }

        var r = allFi.Rows;
        allFi.Dispose();

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, BreakScriptCommand.Method], scp.Stufe + 1, scp.Chain);

        for (var index = 0; index < r.Count; index++) {
            var addme = new List<ScriptVariable>() {
                new RowScriptVariable(varnam, r[index], true, "Iterations-Variable"),
                new DoubleScriptVariable("Index", index, true, "Iterations-Variable")
            };

            scx = CallByFilenameScriptCommand.CallSub(varCol, scp2, infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, addme, null, "ForEachRowScriptCommand");
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }
        }

        if (scx is null) { return new DoItFeedback(); }

        scx.ConsumeBreak();
        return scx;
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}