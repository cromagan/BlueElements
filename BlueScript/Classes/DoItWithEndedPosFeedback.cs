// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
/// Extended feedback structure that includes position information
/// </summary>
public class DoItWithEndedPosFeedback : DoItFeedback {

    #region Constructors

    public DoItWithEndedPosFeedback(bool needsScriptFix, int endpos, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue) => Position = endpos;

    public DoItWithEndedPosFeedback(string failedReason, bool needsScriptFix) : base(failedReason, needsScriptFix) { }

    #endregion

    #region Properties

    public int Position { get; } = -1;

    #endregion
}
