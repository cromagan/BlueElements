// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class RowEventArgs : System.EventArgs {

    #region Constructors

    public RowEventArgs(RowItem row) => Row = row;

    #endregion

    #region Properties

    public RowItem Row { get; }

    #endregion
}