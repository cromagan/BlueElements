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

using BlueBasics;
using BlueScript.Variables;
using System.Runtime.InteropServices;

namespace BlueScript.Structures;

public class CanDoFeedback : CurrentPosition {

    #region Fields

    /// <summary>
    /// Falls ein Codeblock { } direkt nach dem Befehl beginnt, dessen Inhalt
    /// </summary>
    public readonly string CodeBlockAfterText;

    public readonly string NormalizedText;

    #endregion

    #region Constructors

    public CanDoFeedback(CanDoFeedback cdf) : base(cdf) {
        NormalizedText = cdf.NormalizedText;
        CodeBlockAfterText = cdf.CodeBlockAfterText;
    }

    public CanDoFeedback(string subname, int position, string protocol, string chain, string failedReason, bool needsScriptFix, string attributeText, string codeBlockAfterText) : base(subname, position, protocol, chain, failedReason, needsScriptFix) {
        NormalizedText = attributeText;
        CodeBlockAfterText = codeBlockAfterText;
    }

    public CanDoFeedback(CurrentPosition cp, string failedreason, bool needsScriptFix) : this(cp.Subname, cp.Position, cp.Protocol, cp.Chain, failedreason, needsScriptFix, string.Empty, string.Empty) { }

    public CanDoFeedback(CanDoFeedback cdf, int position) : this(cdf.Subname, position, cdf.Protocol, cdf.Chain, cdf.FailedReason, cdf.NeedsScriptFix, cdf.NormalizedText, cdf.CodeBlockAfterText) { }

    public CanDoFeedback(CurrentPosition cp, int position, string attributetext, string codeblockaftertext) : this(cp.Subname, position, cp.Protocol, cp.Chain, cp.FailedReason, cp.NeedsScriptFix, attributetext, codeblockaftertext) { }

    public CanDoFeedback(CurrentPosition cp, string attributetext) : this(cp.Subname, 0, cp.Protocol, cp.Chain, cp.FailedReason, cp.NeedsScriptFix, attributetext, string.Empty) {
        if (cp.Position == attributetext.Length) { Develop.DebugPrint("Müsste das nicht eine Variable sein?"); }
    }

    #endregion

    #region Methods

    public CurrentPosition EndPosition() => new CurrentPosition(this, Position + NormalizedText.Length);

    #endregion
}