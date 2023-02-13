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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_ReplaceList : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableListString.ShortName_Variable }, new List<string> { VariableBool.ShortName_Plain }, new List<string> { VariableBool.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Ersetzt alle Werte in der Liste. Bei Partial=True werden alle Teiltrings in den einzelnen Elementen ausgetauscht.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ReplaceList(ListVariable, CaseSensitive, Partial, SearchValue, ReplaceValue);";

    #endregion

    #region Methods

    public override List<string>Comand(List<Variable>? currentvariables) => new() { "replacelist" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        if (attvar.Attributes[0].ReadOnly) { return DoItFeedback.Schreibgschützt(infos); }

        var tmpList = ((VariableListString)attvar.Attributes[0]).ValueList;

        if (((VariableString)attvar.Attributes[3]).ValueString == ((VariableString)attvar.Attributes[4]).ValueString) { return new DoItFeedback(infos, "Suchtext und Ersetzungstext sind identisch."); }
        if (!((VariableBool)attvar.Attributes[1]).ValueBool && string.Equals(((VariableString)attvar.Attributes[3]).ValueString, ((VariableString)attvar.Attributes[4]).ValueString, StringComparison.OrdinalIgnoreCase)) { return new DoItFeedback(infos, "Suchtext und Ersetzungstext sind identisch."); }

        var ct = 0;
        bool again;
        do {
            ct++;
            if (ct > 10000) { return new DoItFeedback(infos, "Überlauf bei ReplaceList."); }
            again = false;
            for (var z = 0; z < tmpList.Count; z++) {
                if (((VariableBool)attvar.Attributes[2]).ValueBool) {
                    // Teilersetzungen
                    var orignal = tmpList[z];

                    if (((VariableBool)attvar.Attributes[1]).ValueBool) {
                        // Case Sensitive
                        tmpList[z] = tmpList[z].Replace(((VariableString)attvar.Attributes[3]).ValueString, ((VariableString)attvar.Attributes[4]).ValueString);
                    } else {
                        // Not Case Sesitive
                        tmpList[z] = tmpList[z].Replace(((VariableString)attvar.Attributes[3]).ValueString, ((VariableString)attvar.Attributes[4]).ValueString, RegexOptions.IgnoreCase);
                    }
                    again = tmpList[z] != orignal;
                } else {
                    // nur Komplett-Ersetzungen
                    if (((VariableBool)attvar.Attributes[1]).ValueBool) {
                        // Case Sensitive
                        if (tmpList[z] == ((VariableString)attvar.Attributes[3]).ValueString) {
                            tmpList[z] = ((VariableString)attvar.Attributes[4]).ValueString;
                            again = true;
                        }
                    } else {
                        // Not Case Sesitive
                        if (string.Equals(tmpList[z], ((VariableString)attvar.Attributes[3]).ValueString, StringComparison.OrdinalIgnoreCase)) {
                            tmpList[z] = ((VariableString)attvar.Attributes[4]).ValueString;
                            again = true;
                        }
                    }
                }
            }
        } while (again);

        ((VariableListString)attvar.Attributes[0]).ValueList = tmpList;
        return DoItFeedback.Null(infos);
    }

    #endregion
}