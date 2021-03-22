#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
    internal class Method_Substring : Method {

        public override string Syntax { get => "Substring(String, Start, Anzahl)"; }
        public override string Description { get => "Gibt einen Teilstring zurück. Ist der Start oder das Ende keine gültige Position, wird das bestmögliche zurückgegeben und kein Fehler ausgelöst. Subrtring(\"Hallo\", 2,2) gibt ll zurück."; }
        public override List<string> Comand(Script s) { return new List<string>() { "substring" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ")"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.String; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.String, enVariableDataType.Integer, enVariableDataType.Integer }; }
        public override bool EndlessArgs { get => false; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            List<Variable> attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            int st = attvar[1].ValueInt;
            int en = attvar[2].ValueInt;

            if (st < 0) {
                en += st;
                st = 0;
            }


            if (st + en > attvar[0].ValueString.Length) {
                en = attvar[0].ValueString.Length - st;
            }
            return new strDoItFeedback("\"" + attvar[0].ValueString.Substring(st, en) + "\"", string.Empty);
        }
    }
}
