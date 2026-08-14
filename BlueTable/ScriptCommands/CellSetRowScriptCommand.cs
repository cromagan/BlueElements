// Licensed under AGPL-3.0; see License.md for disclaimer and details.
using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class CellSetRowScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Plain, ListOfStringsScriptVariable.ShortName_Plain, DoubleScriptVariable.ShortName_Plain], StringVal, RowVar];
    public override string Command => "cellsetrow";
    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";

    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Sub;
    public override string Syntax => "CellSetRowScriptCommand(Value, Column, RowScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueRowGet(2) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true); }
        if (MyTable(scp) is { } myTb && tb != myTb && !tb.IsThisScriptOk(ScriptEventTypes.value_changed, true)) { return new DoItFeedback($"In der Tabelle '{tb.Caption}' sind die Skripte defekt", false); }

        var columnToSet = tb.Column[attvar.ValueStringGet(1)];
        if (columnToSet is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(1), true); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Die eigene Zelle kann nur über die Variablen geändert werden.", true);
        }

        if (!columnToSet.CanBeChangedByRules()) {
            return new DoItFeedback("Spalte kann nicht bearbeitet werden: " + attvar.ValueStringGet(1), true);
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is StringScriptVariable vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is ListOfStringsScriptVariable vl) { value = string.Join('\r', vl.ValueList); }
        if (attvar.Attributes[0] is DoubleScriptVariable vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        var newchunkval = row.ChunkValue;

        if (columnToSet == tb.Column.ChunkValueColumn) { newchunkval = value; }

        var f = Table.IsCellEditable(columnToSet, row, newchunkval, false);
        if (!string.IsNullOrEmpty(f)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) {
            if (row.CellGetString(columnToSet) != value) { return DoItFeedback.TestModusInaktiv(); }
            return DoItFeedback.Wahr();
        }

        row.CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + tb.Caption + "'");
        columnToSet.AddSystemInfo("Edit with Script", tb, scp.ScriptName);

        return row.CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}