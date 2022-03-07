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
using BlueScript.Structuren;
using Skript.Enums;

namespace BlueScript.Methods {

    internal class Method_SetIfHasValue : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_String_Numeral_or_List, enVariableDataType.Any };
        public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen.\r\nAls 'kein Wert' wird bei Zahlen ebenfalls 0 gewertet.\r\nListen, die einen Eintrag haben (auch wenn dessen Wert leer ist), zählt >nicht< als kein Eintrag.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "SetIfHasValue(Variable, Werte, ...);";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "SetIfHasValue" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return DoItFeedback.Schreibgschützt(); }

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type == enVariableDataType.Error) {
                    continue;
                }

                if (attvar.Attributes[z].Type != attvar.Attributes[0].Type) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich."); }

                var hasvalue = false;

                if (attvar.Attributes[z].Type == enVariableDataType.String) { hasvalue = !string.IsNullOrEmpty(attvar.Attributes[z].ValueString); }
                if (attvar.Attributes[z].Type == enVariableDataType.Numeral) { hasvalue = attvar.Attributes[z].ValueDouble != 0; }
                if (attvar.Attributes[z].Type == enVariableDataType.List) { hasvalue = attvar.Attributes[z].ValueListString.Count > 0; }

                if (hasvalue) {
                    attvar.Attributes[0].ValueString = attvar.Attributes[z].ValueString;
                    return DoItFeedback.Null();
                }
            }

            return DoItFeedback.Null();
        }

        #endregion
    }
}