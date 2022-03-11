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
using static BlueBasics.Extensions;

namespace BlueScript.Methods {

    internal class Method_BerechneVariable_Tilde : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Bool_Numeral_String_List_or_Object };
        public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Null;
        public override string StartSequence => string.Empty;
        public override string Syntax => "~Variablennamenberechnung~ = Berechung;";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "~" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            if (infos.AttributText.Length < 5) { return new DoItFeedback("Variablen-Namen-Berechung kann nicht durchgeführt werden."); }

            var (postilde, _) = NextText(infos.AttributText, 0, Tilde, false, false, KlammernStd);
            var (posgleich, _) = NextText(infos.AttributText, 0, Gleich, false, false, KlammernStd);

            if (postilde + 1 != posgleich) { return new DoItFeedback("Variablen-Namen-Berechung kein gültiges End-~-Zeichen gefunden."); }

            var x = Variable.GetVariableByParsing(infos.AttributText.Substring(0, postilde), s);
            if (x.Variable is not VariableString vs) { return new DoItFeedback("Fehler beim Berechnen des Variablen-Namens."); }

            var newcommand = vs.ValueString + infos.AttributText.Substring(posgleich) + ";";

            return Method_BerechneVariable.VariablenBerechnung(newcommand, s, infos, false);

            //return s.BerechneVariable.DoitKomplett(newcommand, s, infos, false);
        }

        #endregion
    }
}