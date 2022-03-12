// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_IsNullOrEmpty : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Variable_String_Numeral_List_or_Object };

        public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist oder keinen Inhalt hat.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override VariableDataType Returns => VariableDataType.Bool;

        public override string StartSequence => "(";

        public override string Syntax => "isNullOrEmpty(Variable)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "isnullorempty" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

            if (attvar.Attributes.Count == 0) {
                if (attvar.FehlerTyp != ScriptIssueType.VariableNichtGefunden) {
                    return DoItFeedback.AttributFehler(this, attvar);
                }

                return DoItFeedback.Wahr();
            }

            if (attvar.Attributes[0].IsNullOrEmpty) { return DoItFeedback.Wahr(); }

            if (attvar.Attributes[0] is VariableUnknown) {
                return DoItFeedback.Wahr();
            }

            return DoItFeedback.Falsch();
        }

        #endregion
    }
}