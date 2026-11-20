// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.EventArgs;
using BlueBasics.Enums;

namespace BlueScript.EventArgs;

public class CanDoScriptEventArgs : CancelReasonEventArgs {

    #region Constructors

    public CanDoScriptEventArgs(ScriptEventTypes scriptType, bool extended) => (ScriptType, Extended) = (scriptType, extended);

    #endregion

    #region Properties

    public bool Extended { get; }

    /// <summary>
    /// Der Skripttyp, dessen Ausführbarkeit geprüft wird.
    /// </summary>
    public ScriptEventTypes ScriptType { get; }

    #endregion
}