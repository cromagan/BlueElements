// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Lädt eine andere Tabelle sucht eine Zeile mit einem FilterScriptCommand und gibt den Inhalt einer Spalte (ReturnColumn) als String zurück.
/// 
/// Achtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinkedScriptCommand benutzen.
/// 
/// Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.
/// Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.
/// Ein FilterScriptCommand kann mit dem Befehl 'FilterScriptCommand' erstellt werden.
/// 
/// Ähnlichr Befehle: CellGetRowScriptCommand, ImportLinkedScriptCommand
/// </summary>
public class CellGetFilterScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];
    public override string Command => "cellgetfilter";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "CellGetFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, FilterScriptCommand, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, errorreason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 3, MyTable(scp), scp.ScriptName, true);

        if (allFi is null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {errorreason}", needsScriptFix); }

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
