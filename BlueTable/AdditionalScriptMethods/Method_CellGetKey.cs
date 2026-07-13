// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;

namespace BlueTable.AdditionalScriptMethods;

public class Method_CellGetKey : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "cellgetkey";
    public override string Description => "Sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als String zurück.\r\n\r\nAchtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinked benutzen.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\n\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "CellGetKey(Table, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }
        //if (tb != myDb && !tb.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false); }

        if (tb.Column.First is not { IsDisposed: false } cf) {
            return new DoItFeedback("Erste Spalte der Tabelle '" + attvar.ValueStringGet(0) + "' nicht gefunden", true);
        }

        var returncolumn = tb.Column[attvar.ValueStringGet(2)];
        if (returncolumn is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(2), true); }
        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        var r = FilterCollection.CalculateFilteredRows(tb, new FilterItem(cf, FilterType.Istgleich_GroßKleinEgal, attvar.ValueStringGet(1)));

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(3)); }
        if (r.Count > 1) { return new DoItFeedback(attvar.ValueStringGet(4)); }
        return new DoItFeedback(r[0].CellGetString(returncolumn));
    }

    #endregion
}