// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class TableFileGiveBackEventArgs : System.EventArgs, IHasTable {

    #region Properties

    public Table? Table { get; set; }

    #endregion
}