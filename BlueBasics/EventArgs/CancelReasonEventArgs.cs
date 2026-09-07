// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.EventArgs;

public class CancelReasonEventArgs : System.EventArgs {

    #region Properties

    public bool Cancel => !string.IsNullOrEmpty(CancelReason);
    public string CancelReason { get; set; } = string.Empty;

    #endregion
}