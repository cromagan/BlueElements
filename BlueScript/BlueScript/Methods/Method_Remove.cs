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

    internal class Method_Remove : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List, enVariableDataType.Bool, enVariableDataType.String_or_List };
        public override string Description => "Entfernt aus der Liste die angegebenen Werte.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "Remove(ListVariable, CaseSensitive, Value1, Value2, ...);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "remove" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return strDoItFeedback.Schreibgschützt(); }

            var tmpList = attvar.Attributes[0].ValueListString;
            for (var z = 2; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type == enVariableDataType.String) {
                    tmpList.RemoveString(attvar.Attributes[z].ValueString, attvar.Attributes[1].ValueBool);
                }
                if (attvar.Attributes[z].Type == enVariableDataType.List) {
                    tmpList.RemoveString(attvar.Attributes[z].ValueListString, attvar.Attributes[1].ValueBool);
                }
            }
            attvar.Attributes[0].ValueListString = tmpList;
            return strDoItFeedback.Null();
        }

        #endregion
    }
}