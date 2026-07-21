// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public class ScriptEndedFeedback : DoItFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, LogData ld, bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue) {
        Variables = variables;
        GiveItAnotherTry = false;
        Routine = ld.Subname;
        Line = ld.Line;
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) : base(needsScriptFix, false, true, "Start abgebrochen: " + failedReason, null) {
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
        Routine = scriptname;
        Line = 0;
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback(VariableCollection variables, string failedReason) : base(false, false, true, failedReason, null) {
        GiveItAnotherTry = false;
        Variables = variables;
        Routine = string.Empty;
        Line = 0;
    }

    #endregion

    #region Properties

    public bool GiveItAnotherTry { get; }

    /// <summary>
    /// Überschreibt die virtuale <see cref="DoItFeedback.Line"/>. Wird beim
    /// Konstruieren aus dem übergebenen <see cref="LogData"/> gesetzt.
    /// </summary>
    public override int Line { get; }

    /// <summary>
    /// Liefert den FailedReason inkl. Kontext-Informationen (Routine, Zeile),
    /// formatiert für die Anzeige beim Benutzer.
    /// Ist FailedReason leer, wird ein leerer String zurückgegeben (OK).
    /// </summary>
    public string ProtocolText {
        get {
            if (string.IsNullOrEmpty(FailedReason)) { return string.Empty; }
            if (Line > 0) { return $"[{Routine}, Zeile: {Line}]\r\n{FailedReason}"; }
            if (!string.IsNullOrEmpty(Routine)) { return $"[{Routine}]\r\n{FailedReason}"; }
            return FailedReason;
        }
    }

    public string Routine { get; }

    public VariableCollection? Variables { get; }

    #endregion
}
