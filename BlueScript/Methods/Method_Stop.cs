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

using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


public sealed class Method_Stop : Method {

    #region Properties

    public static List<List<string>> Args => [];
    public static string Command => "stop";
    public static List<string> Constants => [];
    public static string Description => "Beendet die Ausführung im Testmodus.";

    public static int LastArgMinCount => -1;


    public static string Returns => string.Empty;
    public static string StartSequence => string.Empty;

    public static string Syntax => "Stop;";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (!scp.ProduktivPhase) { return new DoItFeedback("=== STOP ===", true, ld); }
        return DoItFeedback.Null();
    }

    #endregion
}