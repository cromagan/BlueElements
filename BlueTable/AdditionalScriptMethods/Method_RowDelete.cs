// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_RowDelete : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar];

    public override string Command => "rowdelete";

    public override string Description => "Löscht die Zeile. Kann auch die eigene Zeile löschen, wenn das Skript ReadOnly ist.\r\nGibt leer zurück, wenn erfolgreich. Anderfalls den Grund des Fehlschlagens.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "RowDelete(Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false }) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Eigene Zeile kann nur bei ReadOnly Skripten gelöscht werden", true, ld);
        }

        var r = RowCollection.Remove(row, "Script Command: RowDelete");

        return new DoItFeedback(r.FailedReason);
    }

    #endregion
}