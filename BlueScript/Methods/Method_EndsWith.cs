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
using System;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_EndsWith : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, BoolVal, StringVal];
    public static string Command => "endswith";
    public static List<string> Constants => [];
    public static string Description => "Prüft, ob der String mit einem der angegeben Strings endet.";

    public static int LastArgMinCount => 1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBool.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "EndsWith(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.ValueBoolGet(1)) {
                if (attvar.ValueStringGet(0).EndsWith(attvar.ValueStringGet(z))) {
                    return DoItFeedback.Wahr();
                }
            } else {
                if (attvar.ValueStringGet(0).EndsWith(attvar.ValueStringGet(z), StringComparison.OrdinalIgnoreCase)) {
                    return DoItFeedback.Wahr();
                }
            }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}