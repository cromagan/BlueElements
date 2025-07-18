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

using BlueScript.Variables;

namespace BlueScript.Structures;

/// <summary>
/// Extended feedback structure that includes position information
/// </summary>
public class DoItWithEndedPosFeedback : DoItFeedback {

    #region Constructors

    public DoItWithEndedPosFeedback(bool needsScriptFix, int endpos, bool breakFired, bool returnFired, string failedReason, Variable? returnValue, LogData? ld) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue, ld) {
        Position = endpos;
    }

    public DoItWithEndedPosFeedback(string failedReason, bool needsScriptFix, LogData? ld) : base(failedReason, needsScriptFix, ld) { }

    #endregion

    #region Properties

    public int Position { get; } = -1;

    #endregion
}