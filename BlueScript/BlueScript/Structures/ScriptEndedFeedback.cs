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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlueScript.Structures;

public class ScriptEndedFeedback : DoItFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, List<string> protocol, bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) : base(needsScriptFix, breakFired, returnFired, failedReason, returnValue, null) {
        Variables = variables;
        GiveItAnotherTry = false;
        Protocol = protocol.AsReadOnly();
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    /// <param name="failedReason"></param>
    /// <param name="giveitanothertry"></param>
    /// <param name="needsScriptFix"></param>
    /// <param name="scriptname"></param>
    public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) : base(needsScriptFix, false, true, "Start abgebrochen: " + failedReason, null, null) {
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
        Protocol = new ReadOnlyCollection<string>(["[" + scriptname + ", Start abgebrochen]@" + failedReason]);
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback(VariableCollection variables, string failedReason) : base(false, false, true, failedReason, null, null) {
        GiveItAnotherTry = false;
        Protocol = new ReadOnlyCollection<string>([]);

        Variables = variables;
    }

    #endregion

    #region Properties

    public bool GiveItAnotherTry { get; }

    public ReadOnlyCollection<string> Protocol { get; }

    public string ProtocolText => "Skript-Protokoll:\r\n\r\n" + Protocol.JoinWith("\r\n\r\n").Replace("]@", "]\r\n");

    public VariableCollection? Variables { get; }

    #endregion

    #region Methods

    public override void ChangeFailedReason(string newfailedReason, bool needsScriptFix, LogData? ld) {
        ld?.Protocol.AddRange(Protocol);
        base.ChangeFailedReason(newfailedReason, needsScriptFix, ld);
    }

    #endregion
}