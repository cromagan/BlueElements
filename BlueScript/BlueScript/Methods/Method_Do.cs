// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_Do : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { };
        public override string Description => "Führt den Codeblock dauerhaft aus, bis der Befehl Break empfangen wurde. Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
        public override bool EndlessArgs => false;
        public override string EndSequence => string.Empty;
        public override bool GetCodeBlockAfter => true;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => string.Empty;
        public override string Syntax => "Do { Break; }";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "do" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            var tmpline = s.Line;
            var du = 0;
            s.Schleife++;
            do {
                s.Line = tmpline;
                du++;
                if (du > 100000) { return new strDoItFeedback("Do-Schleife nach 100.000 Durchläufen abgebrochen."); }

                var tmpv = new List<Variable>();
                tmpv.AddRange(s.Variablen);

                (var err, var _) = s.Parse(infos.CodeBlockAfterText, false);
                if (!string.IsNullOrEmpty(err)) { return new strDoItFeedback(err); }

                s.Variablen.Clear();
                s.Variablen.AddRange(tmpv);

                if (s.BreakFired) { break; }
            } while (true);

            s.Schleife--;
            s.BreakFired = false;
            s.Line += infos.LineBreakInCodeBlock;
            return new strDoItFeedback(string.Empty);
        }

        private static string GetBoolTMP(string txt, string check) {
            (var i, var _) = NextText(txt, 0, new List<string>() { check }, false, false, KlammernStd);
            if (i < 0) { return string.Empty; }
            if (i < 1 && check != "!") { return string.Empty; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!
            if (i >= txt.Length - 1) { return string.Empty; } // siehe oben
            var start = i - 1;
            var ende = i + check.Length;
            var trenn = "(!|&<>=)";
            var gans = false;
            do {
                if (start < 0) { break; }
                var ze = txt.Substring(start, 1);
                if (!gans && trenn.Contains(ze)) { break; }
                if (ze == "\"") { gans = !gans; }
                start--;
            } while (true);
            do {
                if (ende >= txt.Length) { break; }
                var ze = txt.Substring(ende, 1);
                if (!gans && trenn.Contains(ze)) { break; }
                if (ze == "\"") { gans = !gans; }
                ende++;
            } while (true);
            var s1 = txt.Substring(start + 1, i - start - 1);
            var s2 = txt.Substring(i + check.Length, ende - check.Length - i);
            if (string.IsNullOrEmpty(s1) && check != "!") { return string.Empty; }
            if (string.IsNullOrEmpty(s2)) { return string.Empty; }
            Variable v1 = null;
            if (check != "!") { v1 = new Variable("dummy", s1, null); }
            Variable v2 = new("dummy", s2, null);
            // V2 braucht nicht gepürft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 != null) {
                if (v1.Type != v2.Type) { return string.Empty; }
                if (v1.Type is not enVariableDataType.Bool and
                               not enVariableDataType.Numeral and
                               not enVariableDataType.String) { return string.Empty; }
            } else {
                if (v2.Type != enVariableDataType.Bool) { return string.Empty; }
            }
            var replacer = string.Empty;
            switch (check) {
                case "==":
                    replacer = "false";
                    if (v1.ValueString == v2.ValueString) { replacer = "true"; }
                    break;

                case "!=":
                    replacer = "false";
                    if (v1.ValueString != v2.ValueString) { replacer = "true"; }
                    break;

                case ">=":
                    if (v1.Type != enVariableDataType.Numeral) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble >= v2.ValueDouble) { replacer = "true"; }
                    break;

                case "<=":
                    if (v1.Type != enVariableDataType.Numeral) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble <= v2.ValueDouble) { replacer = "true"; }
                    break;

                case "<":
                    if (v1.Type != enVariableDataType.Numeral) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble < v2.ValueDouble) { replacer = "true"; }
                    break;

                case ">":
                    if (v1.Type != enVariableDataType.Numeral) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble > v2.ValueDouble) { replacer = "true"; }
                    break;

                case "||":
                    if (v1.Type != enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueBool || v2.ValueBool) { replacer = "true"; }
                    break;

                case "&&":
                    if (v1.Type != enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueBool && v2.ValueBool) { replacer = "true"; }
                    break;

                case "!":
                    // S1 dürfte eigentlich nie was sein: !False||!false
                    // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                    if (v2.Type != enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (!v2.ValueBool) { replacer = "true"; }
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;
            }
            return string.IsNullOrEmpty(replacer) ? string.Empty
                                                  : txt.Substring(0, start + 1) + replacer + txt.Substring(ende);
        }

        #endregion
    }
}