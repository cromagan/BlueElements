// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;

namespace BlueTable.AdditionalScriptMethods;

public class Method_SortedRows : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "sortedrows";
    public override string Description => "Gibt die Zeilen der Tabelle in der Standard Sortierung zurück.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListRow.ShortName_Variable;
    public override string Syntax => "SortedRows(table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }

        var r = tb.SortDefinition?.SortedRows(tb.Row) ?? new RowSortDefinition(tb, tb.Column.First, false).SortedRows(tb.Row);
        return new DoItFeedback(new VariableListRow(r));
    }

    #endregion
}