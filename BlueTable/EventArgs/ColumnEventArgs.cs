// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Classes;

namespace BlueTable.EventArgs;

public class ColumnEventArgs : System.EventArgs {

    #region Constructors

    public ColumnEventArgs(ColumnItem column) => Column = column;

    #endregion

    #region Properties

    public ColumnItem Column { get; }

    #endregion
}