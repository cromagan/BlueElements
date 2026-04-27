// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.EventArgs;

public class TableFileGiveBackEventArgs : System.EventArgs, IHasTable {

    #region Properties

    public Table? Table { get; set; }

    #endregion
}