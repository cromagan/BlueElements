// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.EventArgs;

public class RowNullableEventArgs : System.EventArgs {

    #region Constructors

    public RowNullableEventArgs(RowItem? row) => Row = row;

    #endregion

    #region Properties

    public RowItem? Row { get; }

    #endregion
}