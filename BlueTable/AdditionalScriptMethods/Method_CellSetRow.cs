// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_CellSetRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableDouble.ShortName_Plain], StringVal, RowVar];
    public override string Command => "cellsetrow";
    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";

    public override MethodType MethodLevel => MethodType.Sub;

    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "CellSetRow(Value, Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (scp.SyntaxCheck) { return DoItFeedback.Wahr(); } // SyntaxCheck kann Rows nicht generieren und kann somit hier nix prüfen

        if (attvar.ValueRowGet(2) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }
        if (MyTable(scp) is { } myTb && tb != myTb && !tb.IsThisScriptBroken(ScriptEventTypes.value_changed, true)) { return new DoItFeedback($"In der Tabelle '{tb.Caption}' sind die Skripte defekt", false, ld); }

        var columnToSet = tb.Column[attvar.ValueStringGet(1)];
        if (columnToSet is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(1), true, ld); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Die eigene Zelle kann nur über die Variablen geändert werden.", true, ld);
        }

        if (!columnToSet.CanBeChangedByRules()) {
            return new DoItFeedback("Spalte kann nicht bearbeitet werden: " + attvar.ValueStringGet(1), true, ld);
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = string.Join('\r', vl.ValueList); }
        if (attvar.Attributes[0] is VariableDouble vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        var newchunkval = row.ChunkValue;

        if (columnToSet == tb.Column.ChunkValueColumn) { newchunkval = value; }

        var f = Table.AcquireWriteAccess(columnToSet, row, newchunkval, 120, false);
        if (!string.IsNullOrEmpty(f)) { return DoItFeedback.Falsch(); }

        if (!scp.ProduktivPhase) {
            if (row.CellGetString(columnToSet) != value) { return DoItFeedback.TestModusInaktiv(ld); }
            return DoItFeedback.Wahr();
        }

        row.CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + tb.Caption + "'");
        columnToSet.AddSystemInfo("Edit with Script", tb, scp.ScriptName);

        return row.CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}