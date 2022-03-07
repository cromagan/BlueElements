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

using Skript.Enums;
using System.Collections.Generic;
using BlueScript.Structuren;

namespace BlueScript {

    internal class Method_IsType : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_Any, enVariableDataType.String };
        public override string Description => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "isType(Variable, num / str / lst / bol / err / ukn)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "istype" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            switch (attvar.Attributes[1].ValueString.ToLower()) {
                case "num":
                    return attvar.Attributes[0].Type == enVariableDataType.Numeral ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();

                case "str":
                    return attvar.Attributes[0].Type == enVariableDataType.String ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();

                case "lst":
                    return attvar.Attributes[0].Type == enVariableDataType.List ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();
                //
                case "dat":
                //    if (attvar.Attributes[0].Type == enVariableDataType.Date) { return strDoItFeedback.Wahr(); }
                //    return strDoItFeedback.Falsch();

                case "bol":
                    return attvar.Attributes[0].Type == enVariableDataType.Bool ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();

                case "err":
                    return attvar.Attributes[0].Type == enVariableDataType.Error ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();

                case "ukn":
                    return attvar.Attributes[0].Type == enVariableDataType.NotDefinedYet ? strDoItFeedback.Wahr() : strDoItFeedback.Falsch();

                default:
                    return new strDoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt num, str, lst, dat, bol, err oder ukn erwartet.");
            }
        }

        #endregion
    }
}