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

using Skript.Enums;
using System;
using System.Collections.Generic;

namespace BlueScript {
    internal class Method_Round : Method {

        public override string Syntax => "Round(Value, Nachkommastellen)";
        public override string Description => "Rundet den Zahlenwert mathematisch korrekt.";
        public override List<string> Comand(Script s) { return new() { "round" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Number;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Number, enVariableDataType.Integer };
        public override bool EndlessArgs => false;


        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            var n = (int)attvar[1].ValueDouble;

            if (n < 0) { n = 0; }
            if (n > 10) { n = 10; }

            var val = Math.Round(attvar[0].ValueDouble, n);

            return new strDoItFeedback(val.ToString(), string.Empty);
        }
    }
}
