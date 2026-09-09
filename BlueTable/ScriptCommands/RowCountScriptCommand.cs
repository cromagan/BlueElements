// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Zählt die Zeilen, die mit dem gegebenen FilterScriptCommand gefunden werden.
/// </summary>
public class RowCountScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [FilterVar];
    public override string Command => "rowcount";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "RowCount(FilterScriptCommand, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 0, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {failedReason}", needsScriptFix); }

        var r = allFi.Rows;
        allFi.Dispose();

        return new DoItFeedback(r.Count);
    }

    #endregion
}
