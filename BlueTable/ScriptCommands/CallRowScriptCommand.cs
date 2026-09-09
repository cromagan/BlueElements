// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Führt das Skript bei der angegebenen Zeile aus.
/// Wenn die Zeile Null ist, wird kein Fehler ausgegeben.
/// Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.
/// Kein Zugriff auf auf Tabellen-Variablen!
/// </summary>
public class CallRowScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal];
    public override string Command => "callrow";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.Optional;

    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Sub;
    public override string Syntax => "CallRow(Scriptname, RowScriptCommand, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueRowGet(1) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Mit der eigenen Zeile kann CallRowScriptCommand nicht benutzt werden. Evtl. CallScriptCommand in betracht ziehen.", true);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is StringScriptVariable vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var vs = attvar.ValueStringGet(0);

        var scx = row.Table?.ExecuteScript(null, vs, scp.ProduktivPhase, row, a, false, true, 0);
        if (scx is null || scx.Failed) {
            return new DoItFeedback($"'{vs}' bei  '{row.ReadableText()}' abgebrochen: {scx?.FailedReason ?? "Tabelle verworfen"}", false);
        }
        scx.ConsumeBreakAndReturn();
        return scx;
    }

    #endregion
}
