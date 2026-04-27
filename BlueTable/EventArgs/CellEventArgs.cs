// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Classes;

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