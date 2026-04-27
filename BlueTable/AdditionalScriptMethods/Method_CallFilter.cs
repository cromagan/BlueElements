// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_CallFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public override string Command => "callfilter";

    public override string Description => "Sucht Zeilen und ruft in dessen Tabelle ein Skript für jede Zeile aus.\r\n" +
                                                "Über den Filtern kann bestimmt werden, welche Zeilen es betrifft.\r\n" +
                                            "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
                                            "Kein Zugriff auf auf Tabellen-Variablen!";


    public override int LastArgMinCount => 1;

    public override MethodType MethodLevel => MethodType.Sub;




    public override string Syntax => "CallFilter(SubName, Attribut0, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var r = allFi.Rows;
        allFi.Dispose();
        if (r.Count == 0) { return DoItFeedback.Null(); }

        List<string> a = [attvar.ValueStringGet(1)];
        var vs = attvar.ValueStringGet(0);

        foreach (var thisR in r) {
            if (thisR is { IsDisposed: false }) {
                var scx = thisR.Table?.ExecuteScript(null, vs, scp.ProduktivPhase, thisR, a, false, true, 0);
                if (scx == null || scx.Failed) {
                    return new DoItFeedback("'Subroutinen-Aufruf [" + vs + "]' wegen vorherigem Fehler bei Zeile '" + thisR.CellFirstString() + "' abgebrochen", false, ld);
                }
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}