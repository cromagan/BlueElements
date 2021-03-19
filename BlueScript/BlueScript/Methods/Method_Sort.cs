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
    internal class Method_Sort : Method {


        //public Method_Sort(Script parent) : base(parent) { }

        public override string Syntax { get => "Sort(ListVariable, EliminateDupes);"; }

        public override string Description { get => "Sortiert die Liste und falls der zweite Wert TRUE ist, entfernt Doubletten."; }

        public override List<string> Comand(Script s) { return new List<string>() { "sort" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Null; }

        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.VariableList, enVariableDataType.Bool }; }
        public override bool EndlessArgs { get => false; }



        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            var x = attvar[0].ValueListString;

            if (attvar[1].ValueBool) {
                x = x.SortedDistinctList();
            }
            else {
                x.Sort();
            }

            attvar[0].ValueListString = x;
            return new strDoItFeedback();
        }
    }
}
