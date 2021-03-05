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
        //public override string ID { get => "var_caluclate"; }

        public override List<string> Comand { get => Parent.Variablen.AllNames(); }
        public override string StartSequence { get => "="; }
        public override string EndSequence { get => ";"; }
        //public override List<string> AllowedInIDs { get => null; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }





        internal override strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen) {

            var variableName = infos.ComandText.ToLower().ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);
            var variable = variablen.Get(variableName);
            if (variable == null) {
                return new strDoItFeedback("Variable " + variableName + " nicht gefunden");
            }



            var bs = SplitAttribute(infos.AttributText, variablen, false);

            if (bs == null || bs.Count != 1) { return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText); }


            if (bs[0].StartsWith("\"")) {

                if (variable.Type != enVariableDataType.NotDefinedYet && variable.Type != enVariableDataType.String) {
                    return new strDoItFeedback("Variable ist kein String");
                }
                variable.ValueString = bs[0].Replace("\"+\"", string.Empty).Trim("\"");
                variable.Type = enVariableDataType.String;
                return new strDoItFeedback();
            }

            if (bs[0].Contains("|") || bs[0].Contains("&") || bs[0].Contains("!") || bs[0].ToLower().Contains("true") || bs[0].ToLower().Contains("false")) {
                var b = Method_if.GetBool(bs[0]);
                if (b == null) { return new strDoItFeedback("Berechnungsfehler der Formel: " + infos.AttributText + " => " + bs[0]); }
                variable.ValueString = ((string)b).ToString();
                variable.Type = enVariableDataType.Bool;
                return new strDoItFeedback();
            }


            var erg = modErgebnis.Ergebnis(bs[0]);
            if (erg == null) { return new strDoItFeedback("Berechnungsfehler der Formel: " + infos.AttributText + " => " + bs[0]); }

            if (variable.Type != enVariableDataType.NotDefinedYet && variable.Type != enVariableDataType.Number) {
                return new strDoItFeedback("Variable ist keine Zahl");
            }
            variable.ValueString = ((double)erg).ToString();
            variable.Type = enVariableDataType.Number;

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
