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

using BlueScript.Variables;

namespace BlueScript.Classes;

public class ScriptEndedFeedback : DoItFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, string protocol, bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue, null) {
        Variables = variables;
        GiveItAnotherTry = false;
        ProtocolText = protocol;
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) : base(needsScriptFix, false, true, "Start abgebrochen: " + failedReason, null, null) {
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
        ProtocolText = "[" + scriptname + ", Start abgebrochen] " + failedReason;
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback(VariableCollection variables, string failedReason) : base(false, false, true, failedReason, null, null) {
        GiveItAnotherTry = false;
        ProtocolText = string.Empty;

        Variables = variables;
    }

    #endregion

    #region Properties

    public bool GiveItAnotherTry { get; }

    public string ProtocolText { get; }

    public VariableCollection? Variables { get; }

    #endregion


}