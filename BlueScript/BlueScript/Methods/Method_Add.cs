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

using System.Collections.Generic;
using static BlueBasics.Extensions;
using BlueBasics;
using Skript.Enums;

namespace BlueScript {
    class Method_Add : Method {


        public override string Syntax { get => "Add(List-Variable, Value1, Value2, ...)"; }


        public override string Description { get => "Fügt einer Liste einen oder mehrere Werte hinzu."; }
        public override List<string> Comand(Script s) { return new List<string>() { "add" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Null; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.VariableList, enVariableDataType.String }; }
        public override bool EndlessArgs { get => true; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }



            if (attvar[0].Type == Skript.Enums.enVariableDataType.List) {

                var x = attvar[0].ValueString.SplitByCRToList();

                for (var z = 1; z < attvar.Count; z++) {
                    if (attvar[z].Type != Skript.Enums.enVariableDataType.String) { return strDoItFeedback.FalscherDatentyp(); }
                    x.Add(attvar[z].ValueString);
                }

                attvar[0].ValueString = x.JoinWithCr();
                return new strDoItFeedback();
            }

            return strDoItFeedback.FalscherDatentyp();
        }
    }
}
