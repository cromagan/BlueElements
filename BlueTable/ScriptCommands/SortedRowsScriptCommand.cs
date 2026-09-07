// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt die Zeilen der Tabelle in der Standard Sortierung zurück.
/// </summary>
public class SortedRowsScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "sortedrows";
    public override bool MustUseReturnValue => true;
    public override string Returns => ListOfRowsScriptVariable.ShortName_Variable;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "SortedRowsScriptCommand(table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }

        var r = tb.SortDefinition?.SortedRows(tb.Row) ?? new RowSortDefinition(tb, tb.Column.First, false).SortedRows(tb.Row);
        return new DoItFeedback(new ListOfRowsScriptVariable(r));
    }

    #endregion
}
