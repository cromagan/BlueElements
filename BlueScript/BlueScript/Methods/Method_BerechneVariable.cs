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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {
    internal class Method_BerechneVariable : Method {


        public override string Syntax => "VariablenName = Berechung;";


        public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
        public override List<string> Comand(Script s) { return s.Variablen.AllNames(); }
        public override string StartSequence => "=";
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool_Numeral_String_or_List };
        public override bool EndlessArgs => false;





        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var variableName = infos.ComandText.ToLower().ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);
            var variable = s.Variablen.Get(variableName);
            if (variable == null) { return new strDoItFeedback("Variable '" + variableName + "' nicht gefunden"); }

            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (variable.Type != enVariableDataType.NotDefinedYet && attvar.Attributes[0].Type != variable.Type) { return new strDoItFeedback("Variable '" + variableName + "' ist nicht der erwartete Typ: " + variable.Type.ToString()); }

            variable.ValueString = attvar.Attributes[0].ValueString;
            variable.Type = attvar.Attributes[0].Type;

            return new strDoItFeedback();
        }
    }
}
