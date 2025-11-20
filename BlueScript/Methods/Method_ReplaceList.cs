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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlueScript.Methods;


internal class Method_ReplaceList : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal, BoolVal, StringVal, StringVal];
    public override string Command => "replacelist";
    public override List<string> Constants => [];
    public override string Description => "Ersetzt alle Werte in der Liste. Bei Partial=True werden alle Teiltrings in den einzelnen Elementen ausgetauscht.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ReplaceList(ListVariable, CaseSensitive, Partial, SearchValue, ReplaceValue);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var tmpList = attvar.ValueListStringGet(0);

        if (attvar.ValueStringGet(3) == attvar.ValueStringGet(4)) { return new DoItFeedback("Suchtext und Ersetzungstext sind identisch.", true, ld); }
        if (!attvar.ValueBoolGet(1) && string.Equals(attvar.ValueStringGet(3), attvar.ValueStringGet(4), StringComparison.OrdinalIgnoreCase)) { return new DoItFeedback("Suchtext und Ersetzungstext sind identisch.", true, ld); }

        var ct = 0;
        bool again;
        do {
            ct++;
            if (ct > 10000) { return new DoItFeedback("Überlauf bei ReplaceList.", true, ld); }
            again = false;
            for (var z = 0; z < tmpList.Count; z++) {
                if (attvar.ValueBoolGet(2)) {
                    // Teilersetzungen
                    var orignal = tmpList[z];

                    if (attvar.ValueBoolGet(1)) {
                        // Case Sensitive
                        tmpList[z] = tmpList[z].Replace(attvar.ValueStringGet(3), attvar.ValueStringGet(4));
                    } else {
                        // Not Case Sesitive
                        tmpList[z] = tmpList[z].Replace(attvar.ValueStringGet(3), attvar.ValueStringGet(4), RegexOptions.IgnoreCase);
                    }
                    again = tmpList[z] != orignal;
                } else {
                    // nur Komplett-Ersetzungen
                    if (attvar.ValueBoolGet(1)) {
                        // Case Sensitive
                        if (tmpList[z] == attvar.ValueStringGet(3)) {
                            tmpList[z] = attvar.ValueStringGet(4);
                            again = true;
                        }
                    } else {
                        // Not Case Sesitive
                        if (string.Equals(tmpList[z], attvar.ValueStringGet(3), StringComparison.OrdinalIgnoreCase)) {
                            tmpList[z] = attvar.ValueStringGet(4);
                            again = true;
                        }
                    }
                }
            }
        } while (again);

        return attvar.ValueListStringSet(0, tmpList, ld) is { } dif ? dif : DoItFeedback.Null();
    }

    #endregion
}