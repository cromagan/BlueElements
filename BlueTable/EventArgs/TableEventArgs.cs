// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.EventArgs;

public class TableEventArgs : System.EventArgs, IHasTable {

    #region Constructors

    public TableEventArgs(Table? table) => Table = table;

    #endregion

    #region Properties

    public Table? Table { get; }

    #endregion
}