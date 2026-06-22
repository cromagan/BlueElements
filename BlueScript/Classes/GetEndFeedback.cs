// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public readonly struct GetEndFeedback {

    #region Fields

    internal readonly int ContinuePosition;
    internal readonly string FailedReason = string.Empty;
    internal readonly string NormalizedText;
    internal readonly Variable? ReturnValue;

    #endregion

    #region Constructors

    public GetEndFeedback(Variable? returnvalue) {
        ContinuePosition = 0;
        NormalizedText = string.Empty;
        ReturnValue = returnvalue;
    }

    public GetEndFeedback(string failedReason, bool needsScriptFix, LogData? ld) {
        ContinuePosition = 0;
        FailedReason = failedReason;
        NormalizedText = string.Empty;
        ReturnValue = null;
        NeedsScriptFix = needsScriptFix;
        ld?.ErrorMessage = FailedReason;
    }

    public GetEndFeedback(int continuePosition, string attributetext) {
        ContinuePosition = continuePosition;
        NormalizedText = attributetext;
        ReturnValue = null;
        if (ContinuePosition == attributetext.Length) { Develop.DebugPrint("Müsste das nicht eine Variable sein?"); }
    }

    #endregion

    #region Properties

    public bool NeedsScriptFix { get; } = false;

    internal bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);

    #endregion
}