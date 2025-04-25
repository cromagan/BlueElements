// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

namespace BlueScript.Structures;

public readonly struct CanDoFeedback {

    #region Constructors

    public CanDoFeedback(int errorposition, string message, bool needsScriptFix, LogData? ld) {
        ContinueOrErrorPosition = errorposition;
        Message = message;
        NeedsScriptFix = needsScriptFix;
        AttributText = string.Empty;
        CodeBlockAfterText = string.Empty;
        LogData = ld;

        if(needsScriptFix) {
            ld?.AddMessage(message);
        }

    }

    public CanDoFeedback(int continuePosition, string attributtext, string codeblockaftertext, LogData ld) {
        ContinueOrErrorPosition = continuePosition;
        Message = string.Empty;
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

    public LogData? LogData { get; }

    /// <summary>
    /// Gibt empty zurück, wenn der Befehl ausgeführt werden kann.
    /// Ansonsten den Grund, warum er nicht ausgeführt werden kann.
    /// Nur in Zusammenhang mit NeedsScriptFix zu benutzen, weil hier auch einfach die Meldung sein kann, dass der Befehl nicht erkannt wurde - was an sich kein Fehler ist.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public bool NeedsScriptFix { get; }

    #endregion
}