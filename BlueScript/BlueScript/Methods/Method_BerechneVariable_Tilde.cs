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
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_BerechneVariable_Tilde : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool_Numeral_String_List_Bitmap_or_Object };
        public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => string.Empty;
        public override string Syntax => "~Variablennamenberechnung~ = Berechung;";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "~" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            if (infos.AttributText.Length < 5) { return new strDoItFeedback("Variablen-Namen-Berechung kann nicht durchgeführt werden."); }

            var (postilde, _) = NextText(infos.AttributText, 0, Tilde, false, false, KlammernStd);
            var (posgleich, _) = NextText(infos.AttributText, 0, Gleich, false, false, KlammernStd);

            if (postilde + 1 != posgleich) { return new strDoItFeedback("Variablen-Namen-Berechung kein gültiges End-~-Zeichen gefunden."); }

            var x = new Variable("dummy4", infos.AttributText.Substring(0, postilde), s);
            if (x.Type != enVariableDataType.String) { return new strDoItFeedback("Fehler beim Berechnen des Variablen-Namens."); }

            var newcommand = x.ValueString + infos.AttributText.Substring(posgleich) + ";";

            return s._berechneVariable.DoitKomplett(newcommand, s, infos);
        }

        #endregion
    }
}