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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {
    public class Method_SetError : BlueScript.Method {

        //public Method_SetError(Script parent) : base(parent) { }

        public override string Syntax => "SetError(Nachricht, Column1, Colum2, ...);";

        public override string Description => "Bei Zeilenprüfungen wird ein Fehler abgesetzt. Dessen Inhalt bestimmt die Nachricht. Die Spalten, die als fehlerhaft markiert werden sollen, müssen nachträglich als Variablennamen angegeben werden.";
        public override List<string> Comand(Script s) { return new() { "seterror" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Variable_List_String_Numeral_or_Bool };
        public override bool EndlessArgs => true;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                var n = attvar.Attributes[z].Name.ToLower() + "_error";
                var ve = s.Variablen.GetSystem(n);

                if (ve == null) {
                    ve = new Variable(n, string.Empty, Skript.Enums.enVariableDataType.List, false, true, string.Empty);
                    s.Variablen.Add(ve);
                }

                var l = ve.ValueListString;
                l.AddIfNotExists(attvar.Attributes[0].ValueString);
                ve.ValueListString = l;

            }

            return new strDoItFeedback();
        }
    }
}
