// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class RowNullableEventArgs : System.EventArgs {

    #region Constructors

    public RowNullableEventArgs(RowItem? row) => Row = row;

    #endregion

    #region Properties

    public RowItem? Row { get; }

    #endregion
}