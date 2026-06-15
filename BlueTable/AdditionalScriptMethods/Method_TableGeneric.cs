// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;

namespace BlueTable.AdditionalScriptMethods;

public abstract class Method_TableGeneric : Method {

    #region Fields

    public static readonly List<string> FilterVar = [VariableFilterItem.ShortName_Variable];

    public static readonly List<string> RowVar = [VariableRowItem.ShortName_Variable];

    public static readonly List<string> TableVar = [VariableTable.ShortName_Variable];

    #endregion

    #region Methods

    protected static RowItem? BlockedRow(ScriptProperties scp) {
        if (scp.ScriptAttributes.Contains(TableScriptDescription.CellValuesReadOnly)) { return null; }
        if (scp.AdditionalInfo is RowItem r) { return r; }
        return null;
    }

    protected static ColumnItem? Column(ScriptProperties scp, SplittedAttributesFeedback attvar, int no) {
        var c = attvar.Attributes[no];
        if (c is null) { return null; }

        if (c.KeyName.StartsWith("ID_", StringComparison.OrdinalIgnoreCase)) {
            return MyTable(scp)?.Column[c.SearchValue];
        }

        return MyTable(scp)?.Column[c.KeyName];
    }

    protected static Table? MyTable(ScriptProperties scp) {
        if (scp.AdditionalInfo is Table { IsDisposed: false } tb) { return tb; }
        if (scp.AdditionalInfo is RowItem r) { return r.Table; }
        return null;
    }

    #endregion
}