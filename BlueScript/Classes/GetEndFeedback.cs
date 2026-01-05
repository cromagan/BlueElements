// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Variables;

namespace BlueScript.Structures;

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
        ld?.AddMessage(FailedReason);
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