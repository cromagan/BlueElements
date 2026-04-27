// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_RowDeleteFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public override string Command => "rowdeletefilter";


    public override string Description => "Löscht die gefundenen Zeilen";


    public override int LastArgMinCount => 1;

    public override MethodType MethodLevel => MethodType.LongTime;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public override string Returns => VariableBool.ShortName_Plain;

    public override string Syntax => "RowDeleteFilter(Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        var rows = allFi.Rows;
        allFi.Dispose();

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (BlockedRow(scp) is { } mr && rows.Contains(mr)) {
            return new DoItFeedback($"Der Löschen-Befehl würde die eigene Zeile löschen. Evtl. RowDelete benutzen", needsScriptFix, ld);
        }

        return new DoItFeedback(RowCollection.Remove(rows, "Script Command: RowDelete"));
    }

    #endregion
}