// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_ReplaceList : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List, enVariableDataType.Bool, enVariableDataType.Bool, enVariableDataType.String, enVariableDataType.String };
        public override string Description => "Ersetzt alle Werte in der Liste. Bei Partial=True werden alle Teiltrings in den einzelnen Elementen ausgetauscht.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "ReplaceList(ListVariable, CaseSensitive, Partial, SearchValue, ReplaceValue);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "replacelist" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return strDoItFeedback.Schreibgschützt(); }

            var tmpList = attvar.Attributes[0].ValueListString;

            if (attvar.Attributes[3].ValueString == attvar.Attributes[4].ValueString) { return new strDoItFeedback("Suchtext und Ersetzungstext sind identisch."); }
            if (!attvar.Attributes[1].ValueBool && attvar.Attributes[3].ValueString.ToUpper() == attvar.Attributes[4].ValueString.ToUpper()) { return new strDoItFeedback("Suchtext und Ersetzungstext sind identisch."); }

            var ct = 0;
            bool again;
            do {
                ct++;
                if (ct > 10000) { return new strDoItFeedback("Überlauf bei ReplaceList."); }
                again = false;
                for (var z = 0; z < tmpList.Count; z++) {
                    if (attvar.Attributes[2].ValueBool) {
                        // Teilersetzungen
                        var orignal = tmpList[z];

                        if (attvar.Attributes[1].ValueBool) {
                            // Case Sensitive
                            tmpList[z] = tmpList[z].Replace(attvar.Attributes[3].ValueString, attvar.Attributes[4].ValueString);
                        } else {
                            // Not Case Sesitive
                            tmpList[z] = tmpList[z].Replace(attvar.Attributes[3].ValueString, attvar.Attributes[4].ValueString, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        }
                        again = tmpList[z] != orignal;
                    } else {
                        // nur Komplett-Ersetzungen
                        if (attvar.Attributes[1].ValueBool) {
                            // Case Sensitive
                            if (tmpList[z] == attvar.Attributes[3].ValueString) {
                                tmpList[z] = attvar.Attributes[4].ValueString;
                                again = true;
                            }
                        } else {
                            // Not Case Sesitive
                            if (tmpList[z].ToUpper() == attvar.Attributes[3].ValueString.ToUpper()) {
                                tmpList[z] = attvar.Attributes[4].ValueString;
                                again = true;
                            }
                        }
                    }
                }
            } while (again);

            attvar.Attributes[0].ValueListString = tmpList;
            return strDoItFeedback.Null();
        }

        #endregion
    }
}