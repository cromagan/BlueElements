// Licensed under AGPL-3.0; see License.md for disclaimer and details.
using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Lädt eine andere Tabelle sucht eine Zeile mit einem FilterScriptCommand und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück.
/// 
/// Bei Sort True  werden alle Suchergebnisse kombiniert, gemischt und sortiert.
/// Ein FilterScriptCommand kann mit dem Befehl 'FilterScriptCommand' erstellt werden.
/// Es ist immer eine Count-Prüfung des Ergebnisses erforderlich, da auch eine Liste mit 0 Ergebnissen zurückgegeben werden kann.
/// Dann, wenn die Reihe gefunden wurde, aber kein Inhalt vorhanden ist.
/// Ähnliche Befehle: CellGetRowScriptCommand, ImportLinkedScriptCommand
/// </summary>
public class FilterAllValuesScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, FilterVar];
    public override string Command => "filterallvalues";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfStringsScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "FilterAllValuesScriptCommand(ReturnColumn, Sort, FilterScriptCommand, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 2, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {failedReason}", needsScriptFix); }

        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ValueStringGet(0)];
        if (returncolumn is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true); }
        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        List<string> list = [];
        foreach (var row in r) { list.AddRange(row.CellGetList(returncolumn)); }
        if (attvar.ValueBoolGet(1)) { list = list.SortedDistinctList(); }

        return new DoItFeedback(list);
    }

    #endregion
}
