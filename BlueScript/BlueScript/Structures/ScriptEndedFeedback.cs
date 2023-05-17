// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Variables;

namespace BlueScript.Structures;

public class ScriptEndedFeedback {

    #region Constructors

    public ScriptEndedFeedback(VariableCollection variables, List<string> protocol, bool allOk) {
        Variables = variables;
        GiveItAnotherTry = false;
        Protocol = protocol;
        AllOk = allOk;
        ProtocolText = GenNiceProtokoll(protocol);
    }

    /// <summary>
    /// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
    /// und das Skript erst gar nicht gestartet wird.
    /// </summary>
    /// <param name="errormessage"></param>
    /// <param name="giveitanothertry"></param>
    public ScriptEndedFeedback(string errormessage, bool giveitanothertry, string scriptname) {
        Variables = null;

        GiveItAnotherTry = giveitanothertry;
        Protocol = new List<string> {
           "[" + scriptname + ", Start abgebrochen]@" + errormessage
        };

        ProtocolText = GenNiceProtokoll(Protocol);
        AllOk = false;
    }

    #endregion

    #region Properties

    public bool AllOk { get; }

    public bool GiveItAnotherTry { get; }

    public List<string> Protocol { get; }

    public string ProtocolText { get; }

    public VariableCollection? Variables { get; }

    #endregion

    #region Methods

    private string GenNiceProtokoll(List<string> protokoll) => "Skript-Protokoll:\r\n\r\n" + protokoll.JoinWith("\r\n\r\n").Replace("]@", "]\r\n");

    #endregion
}