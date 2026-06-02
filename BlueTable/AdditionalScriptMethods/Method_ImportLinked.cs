// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_ImportLinked : Method_TableGeneric {

    #region Properties

    public override string Command => "importlinked";
    public override string Description => "Lädt alle verlinkte Zellen mit dem aktuellsten Wert in den Variablen-Speicher.\r\nVorherige Variablen, die über den Befehl geladen wurden, werden gelöscht.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "ImportLinked();";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var t = "Befehl: ImportLinked";

        varCol.RemoveWithComment(t);

        #region  Meine Zeile ermitteln (r)

        var r = BlockedRow(scp);
        if (r?.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Zeilenfehler!", true, ld); }

        #endregion

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.IsDisposed) { continue; }

            if (thisColumn.RelationType != RelationType.CellValues) { continue; }

            var linkedTable = thisColumn.LinkedTable;
            if (linkedTable is not { IsDisposed: false }) { return new DoItFeedback($"Verlinkte Tabelle in der Spalte '{thisColumn.KeyName}' nicht vorhanden", true, ld); }

            //if (!linkedTable.AreScriptsExecutable()) { return new DoItFeedback("In der Tabelle '" + linkedTable.Caption + "' sind die Skripte defekt", false, ld); }

            var targetColumn = linkedTable.Column[thisColumn.ColumnKeyOfLinkedTable];
            if (targetColumn is null) { return new DoItFeedback($"Die verlinkte Spalte {thisColumn.ColumnKeyOfLinkedTable} ist in der Zieltabelle {linkedTable.Caption} nicht vorhanden. Auslösende Spalte: {thisColumn.KeyName}", true, ld); }

            var result = CellCollection.GetFilterFromLinkedCellData(linkedTable, thisColumn, r, varCol);
            if (result.IsFailed || result.Value is not FilterCollection { } fc) { return new DoItFeedback($"Berechnungsfehler im Tabellekopf von '{tb.Caption}' der verlinkten Zellen: {result.FailedReason}", true, ld); }

            var rows = fc.Rows;
            if (rows.Count > 1) { return new DoItFeedback("Suchergebnis liefert mehrere Ergebnisse.", true, ld); }

            var v = RowItem.CellToVariable(targetColumn, null, true, false);

            if (rows.Count == 1) {
                v = RowItem.CellToVariable(targetColumn, rows[0], true, false);
            }
            v ??= new VariableUnknown("xxx");
            v.KeyName = "Linked_" + thisColumn.KeyName;
            v.Comment = t;
            varCol.Add(v);
        }

        return DoItFeedback.Null();
    }

    #endregion
}