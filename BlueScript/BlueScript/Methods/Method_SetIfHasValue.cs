﻿// Authors:
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

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_SetIfHasValue : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_String_Numeral_or_List, enVariableDataType.Any };
        public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable. Dabei müssen die Datentypen übereinstimmen. Falls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen. Als kein Wert wird bei Zahlen ebenfalls 0 gewertet. Listen, die einen Eintrag haben (auch wenn desse Wert leer ist), zählt nicht als kein Eintrag.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "SetIfHasValue(Variable, Werte, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "SetIfHasValue" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return new strDoItFeedback("Ausgangsvariable schreibgeschützt."); }

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type != enVariableDataType.Error) {
                    if (attvar.Attributes[z].Type != attvar.Attributes[0].Type) { return new strDoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich."); }

                    var hasvalue = false;

                    if (attvar.Attributes[z].Type == enVariableDataType.String) { hasvalue = !string.IsNullOrEmpty(attvar.Attributes[z].ValueString); }
                    if (attvar.Attributes[z].Type == enVariableDataType.Numeral) { hasvalue = attvar.Attributes[z].ValueDouble != 0; }
                    if (attvar.Attributes[z].Type == enVariableDataType.List) { hasvalue = attvar.Attributes[z].ValueListString.Count > 0; }

                    if (hasvalue) {
                        attvar.Attributes[0].ValueString = attvar.Attributes[z].ValueString;
                        return new strDoItFeedback();
                    }
                }
            }

            return new strDoItFeedback();
        }

        #endregion
    }
}