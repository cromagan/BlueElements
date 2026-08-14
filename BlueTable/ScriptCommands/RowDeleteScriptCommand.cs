// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class RowDeleteScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [RowVar];

    public override string Command => "rowdelete";

    public override string Description => "Löscht die Zeile. Kann auch die eigene Zeile löschen, wenn das Skript ReadOnly ist.\r\nGibt leer zurück, wenn erfolgreich. Anderfalls den Grund des Fehlschlagens.";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;

    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "RowDeleteScriptCommand(RowScriptCommand)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }
        if (row.Table is not { IsDisposed: false }) { return new DoItFeedback("Fehler in der Zeile", true); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Eigene Zeile kann nur bei ReadOnly Skripten gelöscht werden", true);
        }

        var r = RowCollection.Remove(row, "Script Command: RowDeleteScriptCommand");

        return new DoItFeedback(r.FailedReason);
    }

    #endregion
}