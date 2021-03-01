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

namespace BlueScript {
    public abstract class Method : Variable {


        public readonly Script Parent;

        public Method() { } // Dummy für Ermittlung des richtigen Codes

        //public Method(Script parent, string toParse) {
        //    Parent = parent;
        //    Parse(toParse);
        //}


        public abstract string Command { get; }
        public abstract List<string> StartSequence { get; }
        public abstract List<string> EndSequence { get; }


        public (int continuepos, string error, bool abbruch, string betweentext) CanDo(string scriptText, int pos) {

            var maxl = scriptText.Length;

            foreach (var thisseq in StartSequence) {
                var s = (Command + thisseq).ToLower();
                var l = s.Length;

                if (pos + l < maxl) {


                    if (scriptText.Substring(pos, l).ToLower() == s) {

                        (var endpos, var textBetween, var error) = GetEnd(scriptText, pos + l);
                        if (!string.IsNullOrEmpty(error)) {
                            return (endpos, "Fehler bei " + Command + ": " + error, true, string.Empty);
                        }




                        return (endpos, string.Empty, false, textBetween);
                    }
                }

            }

            return (pos, "Kann nicht geparst werden", false, string.Empty);
        }


        (int enspos, string textBetween, string error) GetEnd(string scriptText, int startpos) {

            var klammern = 0;
            var Gans = false;
            var GeschwKlammern = false;
            var EckigeKlammern = 0;

            var pos = startpos - 1; // Letztes Zeichen noch berücksichtigen, könnte ja Klammer auf oder zu sein

            var maxl = scriptText.Length;

            do {

                if (pos > maxl) { return (pos, string.Empty, "Lesen über Textende"); }

                #region Klammer und " erledigen
                switch (scriptText.Substring(pos)) {

                    // Gänsefüsschen, immer erlaubt
                    case "\"":
                        Gans = !Gans;
                        break;

                    // Ekige klammern könne in { oder ( vorkommen, immer erlaubt
                    case "[":
                        if (!Gans) {
                            EckigeKlammern++;
                        }
                        break;

                    case "]":
                        if (!Gans) {
                            if (EckigeKlammern <= 0) { return (pos, string.Empty, "] nicht gültig"); }
                            EckigeKlammern--;
                        }
                        break;


                    // Runde klammern können in { vorkommen
                    case "(":
                        if (!Gans) {
                            if (EckigeKlammern > 0) { return (pos, string.Empty, "( nicht gültig"); }
                            klammern++;
                        }

                        break;

                    case ")":
                        if (!Gans) {
                            if (EckigeKlammern > 0) { return (pos, string.Empty, ") nicht gültig"); }
                            if (klammern <= 0) { return (pos, string.Empty, ") nicht gültig"); }
                            klammern--;
                        }
                        break;


                    // Gescheifte klammern müssen immer sauber auf und zu gemacht werdrn!
                    case "{":
                        if (!Gans) {
                            if (klammern > 0) { return (pos, string.Empty, "{ nicht gültig"); }
                            if (EckigeKlammern > 0) { return (pos, string.Empty, "{ nicht gültig"); }
                            if (GeschwKlammern) { return (pos, string.Empty, "{ nicht gültig"); }
                            GeschwKlammern = true;
                        }
                        break;

                    case "}":
                        if (!Gans) {
                            if (klammern > 0) { return (pos, string.Empty, "} nicht gültig"); }
                            if (EckigeKlammern > 0) { return (pos, string.Empty, "} nicht gültig"); }
                            if (!GeschwKlammern) { return (pos, string.Empty, "} nicht gültig"); }
                            GeschwKlammern = false;
                        }
                        break;
                }
                #endregion



                if (klammern == 0 && !Gans && !GeschwKlammern && EckigeKlammern == 0) {

                    foreach (var thisEnd in EndSequence) {


                        if (pos + thisEnd.Length <= maxl) {

                            if (scriptText.Substring(pos, thisEnd.Length).ToLower() == thisEnd.ToLower()) {

                                var txtBTW = DeKlammere(scriptText.Substring(startpos, pos - startpos));
                                return (pos + thisEnd.Length, txtBTW, string.Empty);
                            }

                        }

                    }



                }

                pos++;
            } while (true);


        }


        public string DeKlammere(string txtBTW) {

            var otxt = txtBTW;

            do {
                otxt = txtBTW;
                txtBTW = txtBTW.Trim(" ");
                if (otxt.StartsWith("(") && otxt.EndsWith(")")) {
                    BlueBasics.Develop.DebugPrint_NichtImplementiert();
                }
                if (otxt.StartsWith("{") && otxt.EndsWith("}")) {
                    BlueBasics.Develop.DebugPrint_NichtImplementiert();
                }
                if (otxt.StartsWith("[") && otxt.EndsWith("]")) {
                    BlueBasics.Develop.DebugPrint_NichtImplementiert();
                }


            } while (otxt != txtBTW);

            return txtBTW;
        }


        internal abstract (string error, int pos) DoIt(string betweentext, List<Variable> variablen);
    }


}
