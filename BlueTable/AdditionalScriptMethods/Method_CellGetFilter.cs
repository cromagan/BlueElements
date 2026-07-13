// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_CellGetFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];
    public override string Command => "cellgetfilter";
    public override string Description => "Lädt eine andere Tabelle sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als String zurück.\r\n\r\nAchtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinked benutzen.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\n\r\nÄhnlichr Befehle: CellGetRow, ImportLinked";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "CellGetFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, errorreason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, MyTable(scp), scp.ScriptName, true);

        if (allFi is null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"Filter-Fehler: {errorreason}", needsScriptFix); }

        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ValueStringGet(0)];
        if (returncolumn is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true); }
        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(1)); }
        if (r.Count > 1) { return new DoItFeedback(attvar.ValueStringGet(2)); }

        var v = RowItem.CellToVariable(returncolumn, r[0], true, false);
        if (v is null) { return new DoItFeedback($"Wert der Variable konnte nicht gelesen werden - ist die Spalte '{returncolumn.KeyName} 'im Skript vorhanden'?", true); }

        return new DoItFeedback(r[0].CellGetString(returncolumn));
    }

    #endregion
}