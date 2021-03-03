﻿#region BlueElements - a collection of useful tools, database and controls
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

namespace BlueScript {
    class Method_var : Method {


        public Method_var(Script parent) : base(parent) { }


        //public Method_var(Script parent, string toParse) : base(parent, toParse) { }

        public override string ID { get => "var"; }
        public override List<string> Comand { get => new List<string>() { "var" }; }
        public override string StartSequence { get => ""; }
        public override string EndSequence { get => ";"; }
        public override List<string> AllowedInIDs { get => null; }
        public override bool GetCodeBlockAfter { get => false; }
        //public override bool ReturnsVoid { get => true; }




        internal override strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen, Method parent) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = infos.AttributText.SplitBy("=");

            if (bs.GetUpperBound(0) != 1) { return new strDoItFeedback("Fehler mit = - Zeichen"); }


            if (!Variable.IsValidName(bs[0])) { return new strDoItFeedback(bs[0] + "ist kein gültiger Variablen-Name"); }


            var v = variablen.Get(bs[0]);

            if (v != null) { return new strDoItFeedback("Variable " + bs[0] + " ist bereits vorhanden."); }

            variablen.Add(new Variable(bs[0]));


            var r = new Method_BerechneVariable(Parent);

            var f = r.CanDo(infos.AttributText + ";", 0, this);


            if (!string.IsNullOrEmpty(f.ErrorMessage)) {

                return new strDoItFeedback("Befehl nicht erkannt");
            }

            if (infos.AttributText.Length != f.ContinueOrErrorPosition - 1) {
                return new strDoItFeedback("Falsch gesetztes Semikolon");
            }


            var f2 = r.DoIt(f, variablen, this);

            if (!string.IsNullOrEmpty(f2.ErrorMessage)) {
                return new strDoItFeedback("Berechung fehlerhaft: " + f2.ErrorMessage);
            }

            return new strDoItFeedback();

        }
    }
}
