// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
/// Extended feedback structure that includes position information
/// </summary>
public class DoItWithEndedPosFeedback : DoItFeedback {

    #region Constructors

    public DoItWithEndedPosFeedback(bool needsScriptFix, int endpos, bool breakFired, bool returnFired, string failedReason, Variable? returnValue, LogData ld) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue, ld) => Position = endpos;

    /// <summary>
    /// Nutzt den protected Base-Konstruktor OHNE LogData-Seiteneffekt.
    /// Wird gebraucht, wenn firstResult bereits von DoIt/CallSub ein vollständiges
    /// Protokoll in ld geschrieben hat und dieses nicht überschrieben werden darf.
    /// </summary>
    public DoItWithEndedPosFeedback(bool needsScriptFix, int endpos, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue) => Position = endpos;

    public DoItWithEndedPosFeedback(string failedReason, bool needsScriptFix, LogData ld) : base(failedReason, needsScriptFix, ld) { }

    #endregion

    #region Properties

    public int Position { get; } = -1;

    #endregion
}