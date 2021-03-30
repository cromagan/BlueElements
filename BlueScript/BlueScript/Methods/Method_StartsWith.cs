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
using System.Collections.Generic;

namespace BlueScript {
    internal class Method_StartsWith : Method {

        public override string Syntax => "StartsWith(String, CaseSensitive, Value1, Value2, ...)";

        public override string Description => "Prüft, ob der String mit einem der angegeben Strings startet.";

        public override List<string> Comand(Script s) { return new() { "startswith" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Bool, enVariableDataType.String };
        public override bool EndlessArgs => true;


        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            for (var z = 2; z < attvar.Count; z++) {
                if (attvar[1].ValueBool) {
                    if (attvar[0].ValueString.StartsWith(attvar[z].ValueString)) {
                        return strDoItFeedback.Wahr();
                    }
                } else {
                    if (attvar[0].ValueString.ToLower().StartsWith(attvar[z].ValueString.ToLower())) {
                        return strDoItFeedback.Wahr();
                    }
                }

            }
            return strDoItFeedback.Falsch();
        }
    }
}
