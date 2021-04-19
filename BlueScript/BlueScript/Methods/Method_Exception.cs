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

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {
    internal class Method_Exception : Method {

        public override string Syntax => "Exception(\"Unbehandelter Programmcode!\");";
        public override string Description => "Unterbricht das Skript mit einer Fehlermeldung.";
        public override List<string> Comand(Script s) { return new() { "Exception" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.String };
        public override bool EndlessArgs => false;



        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }

            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

            if (attvar.Attributes == null || attvar.Attributes.Count != 1) { return new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }

            return new strDoItFeedback(string.Empty, attvar.Attributes[0].ValueString);
        }
    }
}
