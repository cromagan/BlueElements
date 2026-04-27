// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Classes;

namespace BlueTable.EventArgs;

public class RowEventArgs : System.EventArgs {

    #region Constructors

    public RowEventArgs(RowItem row) => Row = row;

    #endregion

    #region Properties

    public RowItem Row { get; }

    #endregion
}