// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class CellEventArgs : System.EventArgs {

    #region Constructors

    public CellEventArgs(ColumnItem column, RowItem row) {
        Column = column;
        Row = row;
    }

    #endregion

    #region Properties

    public ColumnItem Column { get; }
    public RowItem Row { get; }

    #endregion
}