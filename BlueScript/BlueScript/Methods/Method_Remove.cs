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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using BlueBasics;
using static BlueBasics.modConverter;

namespace BlueScript {
    class Method_Remove : Method {


        public Method_Remove(Script parent) : base(parent) { }

        public override string Syntax { get => "Remove(ListVariable, CaseSensitive, Value1, Value2, ...);"; }


        public override List<string> Comand { get => new List<string>() { "remove" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }




        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, 1);
            if (attvar == null || attvar.Count < 3) { return strDoItFeedback.AttributFehler(); }

            if (attvar[0] == null) { return strDoItFeedback.VariableNichtGefunden(); }

            if (attvar[1].Type != Skript.Enums.enVariableDataType.Bool ) { return strDoItFeedback.FalscherDatentyp(); }

            if (attvar[0].Type == Skript.Enums.enVariableDataType.List) {

                var x = attvar[0].ValueString.SplitByCRToList();

                for (var z = 2; z < attvar.Count; z++) {
                    if (attvar[z].Type != Skript.Enums.enVariableDataType.String) { return strDoItFeedback.FalscherDatentyp(); }
                    x.RemoveString(attvar[z].ValueString, attvar[1].ValueBool);
                }

                attvar[0].ValueString = x.JoinWithCr();
                return new strDoItFeedback();
            }

            return strDoItFeedback.FalscherDatentyp();

        }
    }
}
