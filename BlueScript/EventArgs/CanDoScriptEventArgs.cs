// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.EventArgs;

namespace BlueScript.EventArgs;

public class CanDoScriptEventArgs : CancelReasonEventArgs {

    #region Constructors

    public CanDoScriptEventArgs(bool extended) => Extended = extended;

    #endregion

    #region Properties

    public bool Extended { get; }

    #endregion
}