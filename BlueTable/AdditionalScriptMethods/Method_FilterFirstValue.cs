// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_FilterFirstValue : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];
    public override string Command => "filterfirstvalue";
    public override string Description => "Lädt eine andere Tabelle sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.\r\n\r\nAchtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird der nächstbeste zurückgegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nEs ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.\r\nDann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "FilterFirstValue(ReturnColumn, NothingFoundValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }
        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true, ld);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ValueStringGet(0)];
        if (returncolumn is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true, ld); }
        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(1)); }

        return new DoItFeedback(r[0].CellGetString(returncolumn));
    }

    #endregion
}