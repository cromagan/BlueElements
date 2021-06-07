#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {
    internal class Method_ChangeType : Method {

        public override string Syntax => "ChangeType(Variable, num / str / lst / dat / bol);";

        public override string Description => "Ändert den Variabelntyp einfach um. Ohne jegliche Prüfung. Alle Variablen werden Intern als Text gespeichert, weshalb diese Änderung möglich ist. Evtl. entstehen dadurch Variablen, die an sich kaputt sind, aber nicht als solches markiert sind.";

        public override List<string> Comand(Script s) => new() { "changetype" };
        public override string StartSequence => "(";
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List_String_Numeral_or_Bool, enVariableDataType.String };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            switch (attvar.Attributes[1].ValueString.ToLower()) {

                case "num":
                    attvar.Attributes[0].Type = enVariableDataType.Numeral;
                    break;
                case "str":
                    attvar.Attributes[0].Type = enVariableDataType.String;
                    break;
                case "lst":
                    attvar.Attributes[0].Type = enVariableDataType.List;
                    break;
                //case "dat":
                //    attvar.Attributes[0].Type = enVariableDataType.Date;
                //    break;
                case "bol":
                    attvar.Attributes[0].Type = enVariableDataType.Bool;
                    break;
                default:
                    return new strDoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt num, str, lst oder bol erwartet.");
            }

            return new strDoItFeedback();
        }
    }
}
