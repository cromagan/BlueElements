﻿// Authors:
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

public readonly struct GetEndFeedback {

    #region Fields

    internal readonly string AttributeText;
    internal readonly int ContinuePosition;
    internal readonly string FailedReason = string.Empty;
    internal readonly Variable? Variable;

    #endregion

    #region Constructors

    public GetEndFeedback(Variable? variable) {
        ContinuePosition = 0;
        AttributeText = string.Empty;
        Variable = variable;
    }

    public GetEndFeedback(string failedReason, LogData? ld, bool needsScriptFix) {
        ContinuePosition = 0;
        FailedReason = failedReason;
        AttributeText = string.Empty;
        Variable = null;
        NeedsScriptFix = needsScriptFix;
        ld?.AddMessage(FailedReason);
    }

    public GetEndFeedback(int continuePosition, string attributetext) {
        ContinuePosition = continuePosition;
        AttributeText = attributetext;
        Variable = null;
        if (ContinuePosition == attributetext.Length) { Develop.DebugPrint("Müsste das nicht eine Variable sein?"); }
    }

    #endregion

    #region Properties

    public bool NeedsScriptFix { get; } = false;

    internal bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);

    #endregion
}