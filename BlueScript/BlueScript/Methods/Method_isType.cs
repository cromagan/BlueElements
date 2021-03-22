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
    internal class Method_IsType : Method {

        public override string Syntax { get => "isType(Variable, num / str / lst / dat / bol / err / ukn)"; }
        public override string Description { get => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführ!"; }
        public override List<string> Comand(Script s) { return new List<string>() { "istype" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Null; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.VariableAny, enVariableDataType.String }; }
        public override bool EndlessArgs { get => false; }
        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            List<Variable> attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            switch (attvar[1].ValueString.ToLower()) {

                case "num":
                    if (attvar[0].Type == enVariableDataType.Number) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "str":
                    if (attvar[0].Type == enVariableDataType.String) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "lst":
                    if (attvar[0].Type == enVariableDataType.List) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "dat":
                    if (attvar[0].Type == enVariableDataType.Date) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "bol":
                    if (attvar[0].Type == enVariableDataType.Bool) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "err":
                    if (attvar[0].Type == enVariableDataType.Error) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                case "ukn":
                    if (attvar[0].Type == enVariableDataType.NotDefinedYet) { return strDoItFeedback.Wahr(); }
                    return strDoItFeedback.Falsch();
                default:
                    return strDoItFeedback.AttributFehler();
            }
        }
    }
}
