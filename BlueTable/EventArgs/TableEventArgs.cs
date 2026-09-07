// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class TableEventArgs : System.EventArgs, IHasTable {

    #region Constructors

    public TableEventArgs(Table? table) => Table = table;

    #endregion

    #region Properties

    public Table? Table { get; }

    #endregion
}