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

namespace BlueScript {
    class Method_var : Method {


        public Method_var() : base() { }


        //public Method_var(Script parent, string toParse) : base(parent, toParse) { }


        public override string Command { get => "var"; }
        public override List<string> StartSequence { get => new List<string>() { " " }; }
        public override List<string> EndSequence { get => new List<string>() { ";" }; }

        //public override string Parse(string txt) {


        //    return string.Empty;
        //}



        internal override (string error, int pos) DoIt(string betweentext, List<Variable> variablen) {

            if (string.IsNullOrEmpty(betweentext)) { return ("Kein Text angekommen.", 0); }

            var bs = betweentext.SplitBy("=");

            if (bs.GetUpperBound(0) !=1) { return ("Fehler mit = - Zeichen", 0); }
             
            bs[0] = DeKlammere(bs[0]);

            if (!Variable.IsValidName(bs[0])) {  return (bs[0] + "ist kein gültiger Variablen-Name", 0); }


            var v = variablen.Get(bs[0]);

            if (v!= null) { return (bs[0] + " ist bereits vorhanden.", 0); }

            ss
        }
    }
}
