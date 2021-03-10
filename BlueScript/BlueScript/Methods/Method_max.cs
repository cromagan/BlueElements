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

namespace BlueScript {
    class Method_max : Method {


        public Method_max(Script parent) : base(parent) { }

        public override string Syntax { get => "Max(Value1, Value2, ...)"; }

        public override List<string> Comand { get => new List<string>() { "max" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ")"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => "numeral"; }




        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, 0);
            if (attvar == null || attvar.Count <2) { return strDoItFeedback.AttributFehler(); }

            var val = double.MinValue;

            foreach (var thisval in attvar) {
                if (thisval.Type != Skript.Enums.enVariableDataType.Number ) { return strDoItFeedback.FalscherDatentyp(); }
                val = Math.Max(thisval.ValueDouble, val);
            }

            return new strDoItFeedback(val.ToString(), string.Empty);
        }
    }
}
