// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_SortedRows : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "sortedrows";
    public override List<string> Constants => [];
    public override string Description => "Gibt die Zeilen der Tabelle in der Standard Sortierung zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListRow.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "SortedRows(table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }

        var r = tb.SortDefinition?.SortedRows(tb.Row) ?? new RowSortDefinition(tb, tb.Column.First, false).SortedRows(tb.Row);
        return new DoItFeedback(new VariableListRow(r));
    }

    #endregion
}