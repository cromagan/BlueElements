#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using System.Linq;

public struct strCanDoFeedback {

    public strCanDoFeedback(int errorposition, string errormessage, bool mustabort) {
        ContinueOrErrorPosition = errorposition;
        ErrorMessage = errormessage;
        MustAbort = mustabort;
        ComandText = string.Empty;
        AttributText = string.Empty;
        CodeBlockAfterText = string.Empty;
        LineBreakInCodeBlock = 0;
    }


    public strCanDoFeedback(int continuePosition, string comandText, string attributtext, string codeblockaftertext)
        {
        ContinueOrErrorPosition = continuePosition;
        ErrorMessage = string.Empty;
        MustAbort = false;
        ComandText = comandText;
        AttributText = attributtext;
        CodeBlockAfterText = codeblockaftertext;
        LineBreakInCodeBlock = codeblockaftertext.Count(c => c == '¶'); 
    }

    /// <summary>
    /// Die Position, wo der Fehler stattgefunfden hat ODER die Position wo weiter geparsesd werden muss
    /// </summary>
    public int ContinueOrErrorPosition;
    public string ErrorMessage;
    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public bool MustAbort;
    /// <summary>
    /// Der Text, mit dem eingetiegen wird. Also der Behel mit dem StartString.
    /// </summary>
    public string ComandText;
    /// <summary>
    /// Der Text zwischen dem StartString und dem EndString
    /// </summary>
    public string AttributText;
    /// <summary>
    /// Falls ein Codeblock { } direkt nach dem Befehl beginnt, dessen Inhalt
    /// </summary>
    public string CodeBlockAfterText;
    public int LineBreakInCodeBlock;

}