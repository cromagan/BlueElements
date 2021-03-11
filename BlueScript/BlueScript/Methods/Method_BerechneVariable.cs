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
using Skript.Enums;

namespace BlueScript {
    class Method_BerechneVariable : Method {


        public override string Syntax { get => "VariablenName = Berechung;"; }
        public override List<string> Comand(Script s) { return s.Variablen.AllNames(); }
        public override string StartSequence { get => "="; }
        public override string EndSequence { get => ";"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override enVariableDataType Returns { get => enVariableDataType.Null; }
        public override List<enVariableDataType> Args { get => new List<enVariableDataType>() { enVariableDataType.BoolNumString }; }
        public override bool EndlessArgs { get => false; }





        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            var variableName = infos.ComandText.ToLower().ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);
            var variable = s.Variablen.Get(variableName);
            if (variable == null) {
                return new strDoItFeedback("Variable " + variableName + " nicht gefunden");
            }

            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

            if (attvar == null || attvar.Count != 1) { return strDoItFeedback.AttributFehler(); }

            if (variable.Type != enVariableDataType.NotDefinedYet && attvar[0].Type != variable.Type) { return strDoItFeedback.FalscherDatentyp(); }

            variable.ValueString = attvar[0].ValueString;
            variable.Type = attvar[0].Type;

            return new strDoItFeedback();
        }
    }
}
