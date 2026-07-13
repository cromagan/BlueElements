// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;


public class Method_RowCount : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [FilterVar];
    public override string Command => "rowcount";
    public override string Description => "Zählt die Zeilen, die mit dem gegebenen Filter gefunden werden.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "RowCount(Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix); }

        var r = allFi.Rows;
        allFi.Dispose();

        return new DoItFeedback(r.Count);
    }

    #endregion
}