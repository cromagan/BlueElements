// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Erstellt für jeden KeyValue eine neue Zeile, sofern der Wert noch nicht in der ersten Spalte vorhanden ist.
/// Bereits vorhandene Zeilen werden zusammengefasst (maximal 5) und bei veraltetem AgeInDays invalidiert.
/// Die Werte des Filters werden zusätzlich gesetzt, leere KeyValues werden übersprungen.
/// </summary>
public class AddRowsScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar, FloatVal, ListStringVar, FilterVar];
    public override string Command => "addrows";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.Optional;

    public override string Syntax => "AddRows(Table, AgeInDays, KeyValues, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false); }

        if (tb.Column.First is not { IsDisposed: false } c) { return new DoItFeedback("Erste Spalte nicht vorhanden", true); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";
        var d = attvar.ValueNumGet(1);
        var keys = attvar.ValueListStringGet(2).SortedDistinctList();

        foreach (var thisKey in keys) {
            var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 3, myTb, scp.ScriptName, false);
            if (!string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix); }

            allFi ??= new FilterCollection(tb, "AddRows");

            if (allFi[c] is not null) {
                allFi.Dispose();
                return new DoItFeedback("Initialwert doppelt belegt", true);
            }

            allFi.Add(new(c, FilterType.Istgleich_GroßKleinEgal, thisKey));

            var scx = RowScriptCommand.UniqueRow(allFi, d, $"Skript-Befehl: 'AddRowsScriptCommand' der Tabelle {cap}, Skript {scp.ScriptName}", scp);
            allFi.Dispose();
            if (scx.Failed) { return scx; }
        }

        return DoItFeedback.Null();
    }

    #endregion
}