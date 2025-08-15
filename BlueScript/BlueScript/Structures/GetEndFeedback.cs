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

namespace BlueScript.Structures;

public class GetEndFeedback : CanDoFeedback {

    #region Fields

    internal readonly Variable? ReturnValue;

    #endregion

    #region Constructors

    public GetEndFeedback(CanDoFeedback cp, Variable? returnvalue) : base(cp) {
        ReturnValue = returnvalue;
    }

    public GetEndFeedback(CanDoFeedback cp, string failedReason, bool needsScriptFix) : base(cp, failedReason, needsScriptFix) {
        ReturnValue = null;
    }

    public GetEndFeedback(CanDoFeedback cp, string attributetext) : base(cp, cp.Position, attributetext, string.Empty) {
        ReturnValue = null;
        if (cp.Position == attributetext.Length) { Develop.DebugPrint("Müsste das nicht eine Variable sein?"); }
    }

    #endregion
}