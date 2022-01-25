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

    internal class Method_Var : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool_Numeral_String_List_or_Bitmap };
        public override string Description => "Erstellt eine neue Variable, der Typ wird automtisch bestimmt.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "";
        public override string Syntax => "var VariablenName = Wert;";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "var" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var (pos, _) = NextText(infos.AttributText, 0, Gleich, false, false, null);

            if (pos < 1 || pos > infos.AttributText.Length - 2) { return new strDoItFeedback("Fehler mit = - Zeichen"); }

            var varnam = infos.AttributText.Substring(0, pos);

            if (!Variable.IsValidName(varnam)) { return new strDoItFeedback(varnam + " ist kein gültiger Variablen-Name"); }
            var v = s.Variablen.Get(varnam);
            if (v != null) { return new strDoItFeedback("Variable " + varnam + " ist bereits vorhanden."); }
            s.Variablen.Add(new Variable(varnam));

            return s._berechneVariable.DoitKomplett(infos.AttributText + ";", s, infos);
        }

        #endregion
    }
}