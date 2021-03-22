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
    internal class Method_IsNullOrEmpty : Method {

        public override string Description { get => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist oder keinen Inhalt hat."; }
        public override string Syntax { get => "isNullOrEmpty(Variable)"; }
        public override List<string> Comand(Script s) { return new List<string>() { "isnullorempty" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ")"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Bool; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.VariableListOrStringNumBool }; }
        public override bool EndlessArgs { get => false; }


        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            List<Variable> attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.Wahr(); }

            if (string.IsNullOrEmpty(attvar[0].ValueString)) {
                return strDoItFeedback.Wahr();
            }


            if (attvar[0].Type == enVariableDataType.Null || attvar[0].Type == enVariableDataType.Error || attvar[0].Type == enVariableDataType.NotDefinedYet) {
                return strDoItFeedback.Wahr();
            }

            return strDoItFeedback.Falsch();
        }
    }
}
