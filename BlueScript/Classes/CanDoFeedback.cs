// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public readonly struct CanDoFeedback {

    #region Constructors

    public CanDoFeedback(int errorposition, string failedreason, bool needsScriptFix, LogData? ld) {
        ContinueOrErrorPosition = errorposition;
        FailedReason = failedreason;
        NeedsScriptFix = needsScriptFix;
        AttributText = string.Empty;
        CodeBlockAfterText = string.Empty;
        LogData = ld;

        if (needsScriptFix) {
            ld?.ErrorMessage = failedreason;
        }
    }

    public CanDoFeedback(int continuePosition, string attributtext, string codeblockaftertext, LogData ld) {
        ContinueOrErrorPosition = continuePosition;
        FailedReason = string.Empty;
        NeedsScriptFix = false;
        AttributText = attributtext;
        CodeBlockAfterText = codeblockaftertext;
        LogData = ld;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Der Text zwischen dem StartString und dem EndString
    /// </summary>
    public string AttributText { get; }

    /// <summary>
    /// Falls ein Codeblock { } direkt nach dem Befehl beginnt, dessen Inhalt
    /// </summary>
    public string CodeBlockAfterText { get; }

    /// <summary>
    /// Die Position, wo der Fehler stattgefunfden hat ODER die Position wo weiter geparsesd werden muss
    /// </summary>
    public int ContinueOrErrorPosition { get; }

    /// <summary>
    /// Gibt empty zurück, wenn der Befehl ausgeführt werden kann.
    /// Ansonsten den Grund, warum er nicht ausgeführt werden kann.
    /// Nur in Zusammenhang mit NeedsScriptFix zu benutzen, weil hier auch einfach die Meldung sein kann, dass der Befehl nicht erkannt wurde - was an sich kein Fehler ist.
    /// </summary>
    public string FailedReason { get; }

    public LogData? LogData { get; }

    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public bool NeedsScriptFix { get; }

    #endregion
}