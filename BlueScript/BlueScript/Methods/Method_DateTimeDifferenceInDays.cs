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
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Enums;
using static BlueBasics.Converter;

namespace BlueScript.Methods {

    internal class Method_DateTimeDifferenceInDays : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String, VariableDataType.String };
        public override string Description => "Gibt die Differnz in Tagen der beiden Datums als Gleitkommazahl zurück.\rErgebnis = DateTimeString1 - DateTimeString2";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Numeral;
        public override string StartSequence => "(";
        public override string Syntax => "DateTimeDifferenceInDays(DateTimeString1, DateTimeString2)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "datetimedifferenceindays" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var ok1 = DateTimeTryParse(attvar.Attributes[0].ValueString, out var d1);
            if (!ok1) {
                return new DoItFeedback("Der Wert '" + attvar.Attributes[0].ValueString + "' wurde nicht als Zeitformat erkannt.");
            }

            var ok2 = DateTimeTryParse(attvar.Attributes[1].ValueString, out var d2);
            return !ok2
                ? new DoItFeedback("Der Wert '" + attvar.Attributes[1].ValueString + "' wurde nicht als Zeitformat erkannt.")
                : new DoItFeedback(d1.Subtract(d2).TotalDays.ToString(Constants.Format_Float1), VariableDataType.Numeral);
        }

        #endregion
    }
}