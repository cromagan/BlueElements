// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class WriteAccessChangedEventArgs : System.EventArgs {

    #region Constructors

    public WriteAccessChangedEventArgs(bool isEditable, string reason) {
        IsEditable = isEditable;
        Reason = reason;
    }

    #endregion

    #region Properties

    public bool IsEditable { get; }

    public string Reason { get; }

    #endregion
}
