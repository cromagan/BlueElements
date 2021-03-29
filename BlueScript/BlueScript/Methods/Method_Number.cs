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


namespace BlueScript
{
    internal class Method_Number : Method
    {

        public override string Syntax => "Number(string, number)";

        public override string Description => "Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird der nachfolgende Zahlenwert zurückgegeben.";
        public override List<string> Comand(Script s) { return new() { "number" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Number;
        public override List<enVariableDataType> Args => new() { enVariableDataType.String, enVariableDataType.Number };
        public override bool EndlessArgs => false;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s)
        {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null)
            { return strDoItFeedback.AttributFehler(); }

            if (attvar[0].ValueString.IsNumeral())
            {
                return new strDoItFeedback(attvar[0].ValueString, string.Empty);
            }

            return new strDoItFeedback(attvar[1].ValueString, string.Empty);
        }
    }
}
