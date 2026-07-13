// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_RowDeleteFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public override string Command => "rowdeletefilter";

    public override string Description => "Löscht die gefundenen Zeilen.\r\nGibt leer zurück, wenn erfolgreich. Anderfalls den Grund des Fehlschlagens.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "RowDeleteFilter(Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix); }

        var rows = allFi.Rows;
        allFi.Dispose();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        if (BlockedRow(scp) is { } mr && rows.Contains(mr)) {
            return new DoItFeedback($"Der Löschen-Befehl würde die eigene Zeile löschen. Evtl. RowDelete benutzen", needsScriptFix);
        }

        return new DoItFeedback(RowCollection.Remove(rows, "Script Command: RowDelete").FailedReason);
    }

    #endregion
}