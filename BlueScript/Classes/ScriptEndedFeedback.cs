// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public class ScriptEndedFeedback : DoItFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, string protocol, bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue) {
        Variables = variables;
        GiveItAnotherTry = false;
        ProtocolText = protocol;
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) : base(needsScriptFix, false, true, "Start abgebrochen: " + failedReason, null) {
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
        ProtocolText = "[" + scriptname + ", Start abgebrochen] " + failedReason;
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback(VariableCollection variables, string failedReason) : base(false, false, true, failedReason, null) {
        GiveItAnotherTry = false;
        ProtocolText = string.Empty;

        Variables = variables;
    }

    #endregion

    #region Properties

    public bool GiveItAnotherTry { get; }

    public string ProtocolText { get; }

    public VariableCollection? Variables { get; }

    #endregion
}