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
using static BlueBasics.Extensions;

namespace BlueScript {
    internal class Method_Var : Method {

        public override string Syntax => "var VariablenName = Wert;";
        public override string Description => "Erstellt eine neue Variable, der Typ wird automtisch bestimmt.";
        public override List<string> Comand(Script s) => new() { "var" };
        public override string StartSequence => "";
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool_Numeral_or_String };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = infos.AttributText.SplitBy("=");

            if (bs.GetUpperBound(0) != 1) { return new strDoItFeedback("Fehler mit = - Zeichen"); }

            if (!Variable.IsValidName(bs[0])) { return new strDoItFeedback(bs[0] + "ist kein gültiger Variablen-Name"); }

            var v = s.Variablen.Get(bs[0]);

            if (v != null) { return new strDoItFeedback("Variable " + bs[0] + " ist bereits vorhanden."); }

            s.Variablen.Add(new Variable(bs[0]));

            var r = new Method_BerechneVariable();
            var f = r.CanDo(infos.AttributText + ";", 0, false, s);

            if (!string.IsNullOrEmpty(f.ErrorMessage)) {

                return new strDoItFeedback("Befehl nicht erkannt, " + f.ErrorMessage + ": " + infos.AttributText);
            }

            if (infos.AttributText.Length != f.ContinueOrErrorPosition - 1) {
                return new strDoItFeedback("Falsch gesetztes Semikolon");
            }

            var f2 = r.DoIt(f, s);

            return !string.IsNullOrEmpty(f2.ErrorMessage)
                ? new strDoItFeedback("Berechung fehlerhaft: " + f2.ErrorMessage)
                : new strDoItFeedback();
        }
    }
}
