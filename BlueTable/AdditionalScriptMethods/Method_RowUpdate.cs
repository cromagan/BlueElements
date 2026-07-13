// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_RowUpdate : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar, FloatVal, FloatVal];

    public override string Command => "rowupdate";

    public override string Description => "Aktualisiert die Zeile, wenn das alter innerhalb des angegebenen Bereiches ist.\r\n" +
        "Gibt true zurück, wenn die Zeile im Bereich ist oder aktualisiert wurde.\r\n" +
        "Beispiel: RowUpdate(Row,2,10) aktualisiert nur, wenn die Zeile zwischen 2 und 10 Tagen alt ist.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "RowUpdate(Row, MinAgeInDays, MaxAgeInDays)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (scp.Stufe > 10) {
            return new DoItFeedback("'RowUpdate' wird zu verschachtelt aufgerufen.", true);
        }

        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Die eigene Zeile kann nicht aktualisiert werden.", true);
        }

        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return new DoItFeedback($"Zeilen-Status-Spalte in '{tb.KeyName}' nicht gefunden", true); }

        var minage = attvar.ValueNumGet(1);
        var maxage = attvar.ValueNumGet(2);

        if (minage < 0 || minage > maxage) {
            return new DoItFeedback("Die Zeitangaben sind ungültig.", true);
        }

        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";

        var coment = $"Skript-Befehl: 'RowUpdate' der Tabelle {cap}, Skript {scp.ScriptName}";

        var v = row.CellGetDateTime(srs);
        var age = DateTime.UtcNow.Subtract(v).TotalDays;

        if ((age >= minage && age <= maxage) || age > 10000) {
            if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }
            var f = Table.IsCellEditable(srs, row, row.ChunkValue, false);
            if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false); }
            row.InvalidateRowState(coment);
            var sce = row.UpdateRow(true, coment);

            if (sce.Failed) { return DoItFeedback.Falsch(); }
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}