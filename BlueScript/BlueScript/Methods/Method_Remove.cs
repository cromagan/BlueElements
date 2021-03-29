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
    internal class Method_Remove : Method
    {

        public override string Syntax => "Remove(ListVariable, CaseSensitive, Value1, Value2, ...);";
        public override string Description => "Entfernt aus der Liste die angegebenen Werte.";
        public override List<string> Comand(Script s) { return new() { "remove" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.VariableList, enVariableDataType.String, enVariableDataType.String };
        public override bool EndlessArgs => true;

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s)
        {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null)
            { return strDoItFeedback.AttributFehler(); }


            if (attvar[0].Type == Skript.Enums.enVariableDataType.List)
            {

                var x = attvar[0].ValueString.SplitByCRToList();

                for (var z = 2; z < attvar.Count; z++)
                {
                    if (attvar[z].Type != Skript.Enums.enVariableDataType.String)
                    { return strDoItFeedback.FalscherDatentyp(); }
                    x.RemoveString(attvar[z].ValueString, attvar[1].ValueBool);
                }

                attvar[0].ValueString = x.JoinWithCr();
                return new strDoItFeedback();
            }

            return strDoItFeedback.FalscherDatentyp();

        }
    }
}
