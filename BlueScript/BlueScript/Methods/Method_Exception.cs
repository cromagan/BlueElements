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
    class Method_Exception : Method {


        public Method_Exception(Script parent) : base(parent) { }

        public override string Syntax { get => "Exception(\"Unbehandelter Programmcode!\");"; }

        public override List<string> Comand { get => new List<string>() { "Exception" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        //public override List<string> AllowedInIDs { get => null; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }




        public override strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }

            var bs = SplitAttribute(infos.AttributText, variablen, 0);

            if (bs == null || bs.Count != 1) { return new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }



            return new strDoItFeedback(string.Empty, bs[0]);
        }
    }
}
