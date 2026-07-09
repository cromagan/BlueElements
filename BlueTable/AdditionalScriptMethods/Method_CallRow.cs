// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_CallRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal];
    public override string Command => "callrow";

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
            "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Kein Zugriff auf auf Tabellen-Variablen!";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.Optional;

    public override MethodType MethodLevel => MethodType.Sub;

    public override string Returns => VariableString.ShortName_Plain;

    public override string Syntax => "CallRow(Scriptname, Row, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(1) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Mit der eigenen Zeile kann CallRow nicht benutzt werden. Evtl. Call in betracht ziehen.", true, ld);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var vs = attvar.ValueStringGet(0);

        var scx = row.Table?.ExecuteScript(null, vs, scp.ProduktivPhase, row, a, false, true, 0);
        if (scx is null || scx.Failed) {
            return new DoItFeedback($"'{vs}' bei  '{row.ReadableText()}' abgebrochen: {scx?.FailedReason ?? "Tabelle verworfen"}", false, ld);
        }
        scx.ConsumeBreakAndReturn();
        return scx;
    }

    #endregion
}