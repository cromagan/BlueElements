// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Löscht die gefundenen Zeilen.
/// Gibt leer zurück, wenn erfolgreich. Anderfalls den Grund des Fehlschlagens.
/// </summary>
public class RowDeleteFilterScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public override string Command => "rowdeletefilter";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;

    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "RowDeleteFilter(FilterScriptCommand, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {failedReason}", needsScriptFix); }

        var rows = allFi.Rows;
        allFi.Dispose();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        if (BlockedRow(scp) is { } mr && rows.Contains(mr)) {
            return new DoItFeedback($"Der Löschen-Befehl würde die eigene Zeile löschen. Evtl. RowDeleteScriptCommand benutzen", needsScriptFix);
        }

        return new DoItFeedback(RowCollection.Remove(rows, "Script Command: RowDeleteScriptCommand").FailedReason);
    }

    #endregion
}
