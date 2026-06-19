// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
