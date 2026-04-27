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
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using System.Collections.Generic;
using static BlueTable.AdditionalScriptMethods.Method_TableGeneric;

namespace BlueTable.AdditionalScriptMethods;


public sealed class Method_RowIsNull : Method {

    #region Properties

    public static List<List<string>> Args => [RowVar];
    public static string Command => "rowisnull";
    public static List<string> Constants => [];
    public static string Description => "Prüft, ob die übergebene Zeile NULL ist.";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBool.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "RowIsNull(Row)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableRowItem vr) { return new DoItFeedback("Kein Zeilenobjekt übergeben.", true, ld); }

        return vr.RowItem == null ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}