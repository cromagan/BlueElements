// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public abstract class TableGenericScriptCommand : ScriptCommand {

    #region Fields

    public static readonly List<string> FilterVar = [FilterScriptVariable.ShortName_Variable];

    public static readonly List<string> RowVar = [RowScriptVariable.ShortName_Variable];

    public static readonly List<string> TableVar = [TableScriptVariable.ShortName_Variable];

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
            return MyTable(scp)?.Column[c.ValueForCell];
        }

        return MyTable(scp)?.Column[c.KeyName];
    }

    protected static BlueTable.Classes.Table? MyTable(ScriptProperties scp) {
        if (scp.AdditionalInfo is BlueTable.Classes.Table { IsDisposed: false } tb) { return tb; }
        if (scp.AdditionalInfo is RowItem r) { return r.Table; }
        return null;
    }

    #endregion
}