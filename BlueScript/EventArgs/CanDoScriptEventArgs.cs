// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.EventArgs;

namespace BlueScript.EventArgs;

public class CanDoScriptEventArgs : CancelReasonEventArgs {

    #region Fields

    public readonly bool Extended;

    #endregion

    #region Constructors

    public CanDoScriptEventArgs(bool extended) => Extended = extended;

    #endregion
}