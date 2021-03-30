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
    internal class Method_IsNumeral : Method {

        public override string Description => "Prüft, ob der Inhalt der Variable eine gültige Zahl ist. ";
        public override string Syntax => "isNumeral(Value)";
        public override List<string> Comand(Script s) { return new() { "isnumeral" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bool;
        public override List<enVariableDataType> Args => new() { enVariableDataType.NumString };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (attvar == null) { return strDoItFeedback.Falsch(); }

            if (attvar[0].Type == Skript.Enums.enVariableDataType.Number) { return strDoItFeedback.Wahr(); }

            if (attvar[0].Type == Skript.Enums.enVariableDataType.String) {

                if (attvar[0].ValueString.IsNumeral()) {
                    return strDoItFeedback.Wahr();
                }
            }
            return strDoItFeedback.Falsch();
        }
    }
}
