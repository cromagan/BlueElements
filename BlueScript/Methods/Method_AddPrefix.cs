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


internal sealed class Method_AddPrefix : Method {

    #region Properties

    public static List<List<string>> Args => [ListStringVar, StringVal];
    public static string Command => "addprefix";
    public static List<string> Constants => [];
    public static string Description => "Fügt am Anfang jedes Listenobjekts einen Text hinzu.";

    public static int LastArgMinCount => -1;


    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "AddPrefix(VariableListe, PrefixText)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);

        for (var z = 0; z < tmpList.Count; z++) {
            tmpList[z] = attvar.ReadableText(1) + tmpList[z];
        }

        return attvar.ValueListStringSet(0, tmpList, ld) ?? DoItFeedback.Null();
    }

    #endregion
}