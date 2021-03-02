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


        public Method_BerechneVariable(Script parent) : base(parent) { }


        //public Method_var(Script parent, string toParse) : base(parent, toParse) { }


        public override List<string> Command { get => Parent.Variablen.AllNames(); }
        public override List<string> StartSequence { get => new List<string>() { " = ", "=", " =", "= " }; }
        public override List<string> EndSequence { get => new List<string>() { ";" }; }
        public override List<string> AllowedIn { get => null; }
        public override bool GetCodeBlockAfter { get => false; }
        public override bool ReturnsVoid { get => true; }




        internal override strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen, Method parent) {

            var variableName = infos.ComandText.ToLower().ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);
            var variable = variablen.Get(variableName);
            if (variable == null) {
                return new strDoItFeedback("Variable " + variableName + " nicht gefunden");
            }


            var t = ReplaceVariable(infos.AttributText, variablen);

            if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                return new strDoItFeedback("Berechnungsfehler: " + t.ErrorMessage);
            }




            var pos = -1;

            //do {
                // Alle Routinen ohne Void übergeben und ausführen und ersetzen
                //if (Script.ComandOnPosition(txt,))
                Develop.DebugPrint_NichtImplementiert();

            //}



            //var mustType = enVariableDataType.NotDefinedYet;


            //if (txt.Contains("<") || txt.Contains(">") || txt.Contains("=") || txt.Contains("|") || txt.Contains("&") || txt.Contains("!")) {
            //    if (mustType != enVariableDataType.Bool && mustType != enVariableDataType.NotDefinedYet) { return new strDoItFeedback("Unverträglicher Datentyp"); }
            //    mustType = enVariableDataType.Bool;
            //}

            //if (txt.Contains("<") || txt.Contains(">") || txt.Contains("=") || txt.Contains("|") || txt.Contains("&")) {
            //    if (mustType != enVariableDataType.Bool && mustType != enVariableDataType.NotDefinedYet) { return new strDoItFeedback("Unverträglicher Datentyp"); }
            //    mustType = enVariableDataType.Bool;
            //}



            //f

            return new strDoItFeedback();
        }
    }
}
