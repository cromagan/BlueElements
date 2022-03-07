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
using BlueScript.Structuren;
using Skript.Enums;

namespace BlueScript.Methods {

    internal class Method_BerechneVariable : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool_Numeral_String_List_Bitmap_or_Object };
        public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "=";
        public override string Syntax => "VariablenName = Berechung;";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => s == null ? (new List<string>()) : s.Variablen.AllNames();

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var variableName = infos.ComandText.ToLower().ReduceToChars(Constants.AllowedCharsVariableName);
            var variable = s.Variablen.Get(variableName);
            if (variable == null) { return new DoItFeedback("Variable '" + variableName + "' nicht gefunden"); }
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            if (variable.Type != enVariableDataType.NotDefinedYet && attvar.Attributes[0].Type != variable.Type) { return new DoItFeedback("Variable '" + variableName + "' ist nicht der erwartete Typ " + attvar.Attributes[0].Type + ", sondern " + variable.Type); }

            if (variable.Readonly) { return DoItFeedback.Schreibgschützt(); }

            variable.ValueString = attvar.Attributes[0].ValueString;
            variable.Type = attvar.Attributes[0].Type;
            return DoItFeedback.Null();
        }

        internal DoItFeedback DoitKomplett(string newcommand, Script s, CanDoFeedback originalinfos) {
            var f = s._berechneVariable.CanDo(newcommand, 0, false, s);
            if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                return new DoItFeedback("Befehl nicht erkannt, " + f.ErrorMessage + ": " + originalinfos.AttributText);
            }
            //if (originalinfos.AttributText.Length != f.ContinueOrErrorPosition - 1) {
            //    return new strDoItFeedback("Falsch gesetztes Semikolon");
            //}
            var f2 = s._berechneVariable.DoIt(f, s);

            return !string.IsNullOrEmpty(f2.ErrorMessage)
                ? new DoItFeedback("Berechnung fehlerhaft: " + f2.ErrorMessage)
                : DoItFeedback.Null();
        }

        #endregion
    }
}