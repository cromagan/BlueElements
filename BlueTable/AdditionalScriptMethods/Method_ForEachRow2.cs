// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;
using static BlueTable.AdditionalScriptVariables.VariableListRow;

namespace BlueTable.AdditionalScriptMethods;

internal class Method_ForEachRow2 : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableUnknown.ShortName_Plain], ListRowVar];
    public override string Command => "foreachrow2";
    public override string Description => "Führt den Codeblock für jede gefundene Zeile aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt, diese darf noch nicht deklariert sein.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.\r\nDie Variable INDEX zeigt an, bei welchen Eintrag der Zeiger sich gerade befindet.";
    public override bool GetCodeBlockAfter => true;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "ForEachRow2(NeueVariable, ListRow) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(infos.LogData, attvar); }

        var varnam = "value";
        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, infos.LogData); }

        var vari = varCol.GetByKey(varnam);
        if (vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, infos.LogData);
        }

        List<RowItem> r = [];
        if (attvar.Attributes[1] is VariableListRow vlr) { r = vlr.ValueList; }

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method], scp.Stufe + 1, scp.Chain);

        var maxCount = scp.ProduktivPhase ? r.Count : Math.Min(1, r.Count);

        for (var index = 0; index < maxCount; index++) {
            var addme = new List<Variable>() {
                new VariableRowItem(varnam, r[index], true, "Iterations-Variable"),
                new VariableDouble("Index", index, true, "Iterations-Variable")
            };

            scx = Method_CallByFilename.CallSub(varCol, scp2, infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, addme, null, "ForEachRow2", infos.LogData);
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }
        }

        if (scx == null) { return new DoItFeedback(); }

        scx.ConsumeBreak();
        return scx;
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}