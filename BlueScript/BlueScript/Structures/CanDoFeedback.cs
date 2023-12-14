// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

namespace BlueScript.Structures;

public readonly struct CanDoFeedback {

    #region Constructors

    public CanDoFeedback(int errorposition, string errormessage, bool mustabort, LogData ld) {
        ContinueOrErrorPosition = errorposition;
        ErrorMessage = errormessage;
        MustAbort = mustabort;
        AttributText = string.Empty;
        CodeBlockAfterText = string.Empty;
        Data = ld;
    }

    public CanDoFeedback(int continuePosition, string attributtext, string codeblockaftertext, LogData ld) {
        ContinueOrErrorPosition = continuePosition;
        ErrorMessage = string.Empty;
        MustAbort = false;
        AttributText = attributtext;
        CodeBlockAfterText = codeblockaftertext;
        Data = ld;
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

    public LogData Data { get; }
    public string ErrorMessage { get; }

    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public bool MustAbort { get; }

    #endregion
}