// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als String zurück.
/// 
/// Achtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinkedScriptCommand benutzen.
/// 
/// Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.
/// Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.
/// 
/// Ähnliche Befehle: CellGetRowScriptCommand, ImportLinkedScriptCommand
/// </summary>
public class CellGetKeyScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "cellgetkey";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "CellGetKeyScriptCommand(Table, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }
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
