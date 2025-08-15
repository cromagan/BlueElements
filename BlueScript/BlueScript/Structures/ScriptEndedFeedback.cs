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
using System.Collections.ObjectModel;

namespace BlueScript.Structures;

public class ScriptEndedFeedback : DoItFeedback {

    #region Constructors

    public ScriptEndedFeedback(DoItFeedback dif, VariableCollection variables, int position) : base(dif.Subname, position, dif.Protocol, dif.Chain, dif.NeedsScriptFix, dif.BreakFired, dif.ReturnFired, dif.FailedReason, dif.ReturnValue) {
        Variables = variables;
        GiveItAnotherTry = false;
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    /// <param name="failedReason"></param>
    /// <param name="giveitanothertry"></param>
    /// <param name="needsScriptFix"></param>
    /// <param name="scriptname"></param>
    public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) : base("Main", 0, string.Empty, string.Empty, needsScriptFix, false, true, $"[{scriptname},Start abgebrochen] {failedReason}", null) {
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback(CurrentPosition cp, VariableCollection variables, string failedReason) : base(cp.Subname, cp.Position, cp.Protocol, cp.Chain, false, false, true, failedReason, null) {
        GiveItAnotherTry = false;
        Variables = variables;
    }

    public ScriptEndedFeedback(string subname, int position, string protocol, string chain, bool needsScriptFix, bool breakFired, bool returnFired, VariableCollection variables, string failedReason)
      : base(subname, position, protocol, chain, needsScriptFix, breakFired, returnFired, failedReason, null) {
        GiveItAnotherTry = false;
        Variables = variables;
    }

    #endregion

    #region Properties

    public bool GiveItAnotherTry { get; }

    public VariableCollection? Variables { get; }

    #endregion
}