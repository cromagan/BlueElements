// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt den Wert einer Zelle zurück
/// Ähnlicher Befehl: Lookup
/// </summary>
public class CellGetRowScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar];
    public override string Command => "cellgetrow";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "CellGetRowScriptCommand(Column, RowScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueRowGet(1) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Zugriff der Werte der eigenen Zeile nur über Variablen möglich.", true);
        }

        //if (db != myDb && !db.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{db.Caption}' sind die Skripte defekt", false); }

        if (tb.Column[attvar.ValueStringGet(0)] is not { IsDisposed: false } c) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true); }

        var v = RowItem.CellToVariable(c, row, true, false);
        if (v is null) { return new DoItFeedback($"Wert der Variable konnte nicht gelesen werden - ist die Spalte '{c.KeyName} 'im Skript vorhanden'?", true); }

        var l = new List<string>();

        switch (v) {
            case ListOfStringsScriptVariable vl:
                l.AddRange(vl.ValueList);
                break;

            case StringScriptVariable vs:
                var w = vs.ValueString;
                if (!string.IsNullOrEmpty(w)) { l.Add(w); }
                break;

            case DoubleScriptVariable vf:
                var wd = vf.ValueForReplace;
                if (!string.IsNullOrEmpty(wd)) { l.Add(wd); }
                break;

            default:
                return new DoItFeedback("Spaltentyp nicht unterstützt.", true);
        }

        return new DoItFeedback(string.Join('\r', l));
    }

    #endregion
}
