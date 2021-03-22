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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;


namespace BlueScript {
    internal class Method_Number : Method {

        public override string Syntax { get => "Number(string, number)"; }

        public override string Description { get => "Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird der nachfolgende Zahlenwert zurückgegeben."; }
        public override List<string> Comand(Script s) { return new List<string>() { "number" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ")"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Number; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.String, enVariableDataType.Number }; }
        public override bool EndlessArgs { get => true; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            List<Variable> attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            if (attvar[0].ValueString.IsNumeral()) {
                return new strDoItFeedback(attvar[0].ValueString, string.Empty);
            }

            return new strDoItFeedback(attvar[1].ValueString, string.Empty);
        }
    }
}
