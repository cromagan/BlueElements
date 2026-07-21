// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
/// Extended feedback structure that includes position information
/// </summary>
public class DoItWithEndedPosFeedback : DoItFeedback {

    #region Constructors

    public DoItWithEndedPosFeedback(bool needsScriptFix, int endpos, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue) => Position = endpos;

    /// <summary>
    /// Konstruiert das Feedback aus einem bestehenden <see cref="DoItFeedback"/>
    /// (typischerweise das Ergebnis von <c>Method.DoIt</c>).
    /// Dabei werden auch <see cref="Line"/> und alle anderen Eigenschaften übernommen,
    /// sodass z.B. die Zeilennummer aus einem verschachtelten Skript-Block
    /// (if/foreach/do) beim Weiterreichen an den äußeren Parser erhalten bleibt.
    /// </summary>
    public DoItWithEndedPosFeedback(DoItFeedback source, int endpos) : base(source.NeedsScriptFix, source.BreakFired, source.ReturnFired, source.FailedReason, source.ReturnValue) {
        Position = endpos;
        _line = source.Line;
    }

    public DoItWithEndedPosFeedback(string failedReason, bool needsScriptFix) : base(failedReason, needsScriptFix) { }

    #endregion

    #region Properties

    /// <summary>
    /// Zeilennummer aus dem Quell-Feedback. 0 wenn keine gesetzt war.
    /// </summary>
    public override int Line => _line;

    public int Position { get; } = -1;

    private readonly int _line;

    #endregion
}
