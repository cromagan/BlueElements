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

namespace BlueScript {

    internal class Method_SetIfExists : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List_String_Numeral_or_Bool, enVariableDataType.Any };
        public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte eine Variable ist, die nicht existiert, wird diese einfach übergangen.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "SetIfExists(Variable, Werte, ...);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "SetIfExists" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return strDoItFeedback.Schreibgschützt(); }

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type != enVariableDataType.Error) {
                    if (attvar.Attributes[z].Type != attvar.Attributes[0].Type) { return new strDoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich."); }
                    attvar.Attributes[0].ValueString = attvar.Attributes[z].ValueString;
                    return strDoItFeedback.Null();
                }
            }

            return strDoItFeedback.Null();
        }

        #endregion
    }
}