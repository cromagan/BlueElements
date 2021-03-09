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
    class Method_Sort : Method {


        public Method_Sort(Script parent) : base(parent) { }

        public override string Syntax { get => "Sort(ListVariable, EliminateDupes);"; }

        public override List<string> Comand { get => new List<string>() { "sort" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }




        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = SplitAttribute(infos.AttributText, s, 1);

            if (bs == null || bs.Count != 2) { return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText); }


        
            if (bs[1].ToLower() != "false" && bs[1].ToLower() != "true") { return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText); }
            var removedouble = bs[1].ToLower() == "true";

            var variable = s.Variablen.Get(bs[0]);
            if (variable == null) {
                return new strDoItFeedback("Variable " + bs[0] + " nicht gefunden");
            }

            if (variable.Type == Skript.Enums.enVariableDataType.List) {

                var x = variable.ValueString.SplitByCRToList();


                if (removedouble) {
                    x = x.SortedDistinctList();
                }
                else {
                    x.Sort();
                }


                variable.ValueString = x.JoinWithCr();
                return new strDoItFeedback();

            }



            return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText);


            //if (string.IsNullOrEmpty(variable.ValueString)) {
            //    return new strDoItFeedback("true", string.Empty);
            //}

            //return new strDoItFeedback("false", string.Empty);

        }
    }
}
