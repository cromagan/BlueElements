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

#nullable enable

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_IsType : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Variable_Any, VariableDataType.String };
        public override string Description => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "isType(Variable, num / str / lst / bol / err / ukn)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "istype" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            switch (attvar.Attributes[1].ReadableText.ToLower()) {
                case "num":
                    return attvar.Attributes[0] is VariableFloat ? DoItFeedback.Wahr() : DoItFeedback.Falsch();

                case "str":
                    return attvar.Attributes[0] is VariableString vs ? DoItFeedback.Wahr() : DoItFeedback.Falsch();

                case "lst":
                    return attvar.Attributes[0] is VariableListString ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
                //
                case "dat":
                //    if (attvar.Attributes[0].Type == enVariableDataType.Date) { return DoItFeedback.Wahr(); }
                //    return DoItFeedback.Falsch();

                case "bol":
                    return attvar.Attributes[0].Type == VariableDataType.Bool ? DoItFeedback.Wahr() : DoItFeedback.Falsch();

                case "err":
                    return attvar.Attributes[0].Type == VariableDataType.Error ? DoItFeedback.Wahr() : DoItFeedback.Falsch();

                case "ukn":
                    return attvar.Attributes[0].Type == VariableDataType.NotDefinedYet ? DoItFeedback.Wahr() : DoItFeedback.Falsch();

                default:
                    return new DoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt num, str, lst, dat, bol, err oder ukn erwartet.");
            }
        }

        #endregion
    }
}