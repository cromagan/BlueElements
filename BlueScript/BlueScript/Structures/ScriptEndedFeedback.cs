// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

namespace BlueScript.Structures;

public class ScriptEndedFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, List<string> protocol, bool allOk, bool scriptfehleraft, bool breakFired, bool endscript) {
        Variables = variables;
        GiveItAnotherTry = false;
        Protocol = protocol;
        AllOk = allOk;
        ProtocolText = GenNiceProtokoll(protocol);
        BreakFired = breakFired;
        EndScript = endscript;
        ScriptHasSystaxError = scriptfehleraft;
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    /// <param name="errormessage"></param>
    /// <param name="giveitanothertry"></param>
    /// <param name="scriptfehleraft"></param>
    /// <param name="scriptname"></param>
    public ScriptEndedFeedback(string errormessage, bool giveitanothertry, bool scriptfehleraft, string scriptname) {
        Variables = null;

        GiveItAnotherTry = giveitanothertry;

        Protocol = ["[" + scriptname + ", Start abgebrochen]@" + errormessage];
        ProtocolText = GenNiceProtokoll(Protocol);

        AllOk = false;
        ScriptHasSystaxError = scriptfehleraft;
    }

    /// <summary>
    /// Wird verwendet, wenn ein Script beendet wird, ohne weiter Vorkommnisse
    /// </summary>
    public ScriptEndedFeedback() {
        GiveItAnotherTry = false;

        Protocol = [];
        ProtocolText = string.Empty;

        AllOk = true;
        ScriptHasSystaxError = false;
    }

    #endregion

    #region Properties

    public bool AllOk { get; }
    public bool BreakFired { get; }
    public bool EndScript { get; }

    public bool GiveItAnotherTry { get; }
    public List<string> Protocol { get; }
    public string ProtocolText { get; }
    public bool ScriptHasSystaxError { get; }
    public VariableCollection? Variables { get; }

    #endregion

    #region Methods

    private static string GenNiceProtokoll(IEnumerable<string> protokoll) => "Skript-Protokoll:\r\n\r\n" + protokoll.JoinWith("\r\n\r\n").Replace("]@", "]\r\n");

    #endregion
}