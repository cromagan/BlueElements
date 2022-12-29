// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Linq;

namespace BlueScript.Structures;

public struct CanDoFeedback {

    #region Fields

    /// <summary>
    /// Der Text zwischen dem StartString und dem EndString
    /// </summary>
    public readonly string AttributText;

    /// <summary>
    /// Falls ein Codeblock { } direkt nach dem Befehl beginnt, dessen Inhalt
    /// </summary>
    public readonly string CodeBlockAfterText;

    /// <summary>
    /// Der Text, mit dem eingestiegen wird. Also der Befehl mit dem StartString.
    /// </summary>
    public readonly string ComandText;

    /// <summary>
    /// Die Position, wo der Fehler stattgefunfden hat ODER die Position wo weiter geparsesd werden muss
    /// </summary>
    public readonly int ContinueOrErrorPosition;

    public readonly string ErrorMessage;

    public readonly int LineBreakInCodeBlock;

    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public readonly bool MustAbort;

    #endregion

    #region Constructors

    public CanDoFeedback(int errorposition, string errormessage, bool mustabort) {
        ContinueOrErrorPosition = errorposition;
        ErrorMessage = errormessage;
        MustAbort = mustabort;
        ComandText = string.Empty;
        AttributText = string.Empty;
        CodeBlockAfterText = string.Empty;
        LineBreakInCodeBlock = 0;
    }

    public CanDoFeedback(int continuePosition, string comandText, string attributtext, string codeblockaftertext) {
        ContinueOrErrorPosition = continuePosition;
        ErrorMessage = string.Empty;
        MustAbort = false;
        ComandText = comandText;
        AttributText = attributtext;
        CodeBlockAfterText = codeblockaftertext;
        LineBreakInCodeBlock = codeblockaftertext.Count(c => c == '¶');
    }

    #endregion
}