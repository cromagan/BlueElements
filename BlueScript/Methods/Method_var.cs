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

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal sealed class Method_Var : Method {

    #region Fields

    public const string CommandText = "var";

    #endregion

    #region Properties

    public static List<List<string>> Args => [[Variable.Any_Plain]];
    public static string Command => "var";
    public static List<string> Constants => [];
    public static string Description => "Erstellt eine neue Variable, der Typ wird automatisch bestimmt.";

    public static int LastArgMinCount => -1;


    public static string Returns => string.Empty;
    public static string StartSequence => string.Empty;
    public static string Syntax => "var VariablenName = Wert;";

    #endregion

    #region Methods

    public static DoItFeedback DoItVirtual(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Kein Text angekommen.", true, infos.LogData); }

        return VariablenBerechnung(varCol, infos.LogData, scp, infos.AttributText + ";", true);
    }

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Routine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}