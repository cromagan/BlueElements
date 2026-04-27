// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.EventArgs;

public class CancelReasonEventArgs : System.EventArgs {

    #region Properties

    public bool Cancel => !string.IsNullOrEmpty(CancelReason);
    public string CancelReason { get; set; } = string.Empty;

    #endregion
}