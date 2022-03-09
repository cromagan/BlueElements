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

namespace BlueScript.Methods {

    internal class Method_ChangeType : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Variable_List_String_Numeral_or_Bool, VariableDataType.String };
        public override string Description => "Ändert den Variablentyp einfach um. Ohne jegliche Prüfung.\r\nAlle Variablen werden intern als Text gespeichert, weshalb diese Änderung möglich ist.\r\nEvtl. entstehen dadurch Variablen, die an sich kaputt sind, aber nicht als solches markiert sind.\r\nVorsicht bei Listen: Dort werden aus Kompatiblitätsgründen (analog zu Join uns Split) zusätlich ein \\r entfernt bzw. hinzugefügt! Somit ist bei Listen IMMER ein evtl. leerer Index 0 vorhanden.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "ChangeType(Variable, num / str / dat / bol);";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "changetype" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return DoItFeedback.Schreibgschützt(); }

            if (attvar.Attributes[0].Type == VariableDataType.List) {
                if (attvar.Attributes[0].ValueString.EndsWith("\r")) {
                    attvar.Attributes[0].ValueString = attvar.Attributes[0].ValueString.Substring(0, attvar.Attributes[0].ValueString.Length - 1);
                }
            }

            switch (attvar.Attributes[1].ValueString.ToLower()) {
                case "num":
                    attvar.Attributes[0].Type = VariableDataType.Numeral;
                    break;

                case "str":
                    attvar.Attributes[0].Type = VariableDataType.String;
                    break;

                case "lst":
                    attvar.Attributes[0].Type = VariableDataType.List;

                    if (!attvar.Attributes[0].ValueString.EndsWith("\r")) { attvar.Attributes[0].ValueString += "\r"; }

                    break;

                //case "dat":
                //    attvar.Attributes[0].Type = enVariableDataType.Date;
                //    break;

                case "bol":
                    attvar.Attributes[0].Type = VariableDataType.Bool;
                    break;

                default:
                    return new DoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt num, str, lst oder bol erwartet.");
            }
            return DoItFeedback.Null();
        }

        #endregion
    }
}