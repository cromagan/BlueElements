// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

public class Method_AddRows : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, FloatVal, ListStringVar, FilterVar];
    public override string Command => "addrows";

    public override string Description => "Erstellt mehrere neue Zeilen.\r\n" +
                                          "Es werden nur neue Zeilen erstellt, die nicht vorhanden sind.\r\n" +
                                          "Ist sie bereits mehrfach vorhanden, werden diese zusammengefasst (maximal 5).\r\n" +
                                          "Leere KeyValues werden übersprungen.\r\n" +
                                          "Die Werte der Filter werden zusätzlich gesetzt.\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen\r\n" +
        "Mit AgeInDay kann angebeben werden, ab welchen Alter eine gefundene Zeile invalidiert werden soll.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.Optional;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.

    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override string Syntax => "AddRows(table, AgeInDays keyvalues, filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";

        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }

        if (!tb.IsThisScriptOk(ScriptEventTypes.InitialValues, true)) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false); }

        var keys = attvar.ValueListStringGet(2);
        keys = keys.SortedDistinctList();

        var stackTrace = new StackTrace();
        if (stackTrace.FrameCount > 400) {
            return new DoItFeedback("Stapelspeicherüberlauf", true);
        }

        
        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        if (tb.Column.First is not { IsDisposed: false } c) { return new DoItFeedback("Erste Spalte nicht vorhanden", true); }

        var d = attvar.ValueNumGet(1);

        foreach (var thisKey in keys) {

            #region  Filter ermitteln (allfi)

            var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, myTb, scp.ScriptName, false);
            if (!string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix); }

            allFi ??= new FilterCollection(tb, "AddRows");

            #endregion

            if (allFi[c] is not null) {
                allFi.Dispose();
                return new DoItFeedback("Initialwert doppelt belegt", true);
            }

            allFi.Add(new(c, FilterType.Istgleich_GroßKleinEgal, thisKey));

            var scx = Method_Row.UniqueRow(allFi, d, $"Skript-Befehl: 'AddRows' der Tabelle {cap}, Skript {scp.ScriptName}", scp);
            allFi.Dispose();
            if (scx.Failed) { return scx; }
        }

        return DoItFeedback.Null();
    }

    #endregion
}