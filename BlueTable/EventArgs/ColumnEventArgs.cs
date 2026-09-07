// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class ColumnEventArgs : System.EventArgs {

    #region Constructors

    public ColumnEventArgs(ColumnItem column) => Column = column;

    #endregion

    #region Properties

    public ColumnItem Column { get; }

    #endregion
}