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

namespace BlueScript {
    public abstract class Method {


        public readonly Script Parent;

        //public Method() { } // Dummy für Ermittlung des richtigen Codes

        public Method(Script parent) {
            Parent = parent;
        }


        public abstract bool ReturnsVoid { get; }

        public abstract List<string> Command { get; }
        public abstract List<string> StartSequence { get; }
        public abstract List<string> EndSequence { get; }
        public abstract List<string> AllowedIn { get; }
        public abstract bool GetCodeBlockAfter { get; }

        internal abstract strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen, Method parent);


        public strCanDoFeedback CanDo(string scriptText, int pos, Method parent) {

            var maxl = scriptText.Length;


            foreach (var thiscomand in Command) {

                foreach (var thisseq in StartSequence) {
                    var comandtext = thiscomand + thisseq;
                    var l = comandtext.Length;

                    if (pos + l < maxl) {


                        if (scriptText.Substring(pos, l).ToLower() == comandtext.ToLower()) {

                            var f = GetEnd(scriptText, pos + l);
                            if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                                return new strCanDoFeedback(f.ContinuePosition, "Fehler bei " + Command + ": " + f.ErrorMessage, true);
                            }




                            return new strCanDoFeedback(f.ContinuePosition, comandtext, f.AttributeText, string.Empty);
                        }
                    }

                }
            }

            return new strCanDoFeedback(pos, "Kann nicht geparst werden", false);
        }


        strGetEndFeedback GetEnd(string scriptText, int startpos) {

            var (pos, witch) = Script.NextText(scriptText, startpos, EndSequence);


            if (pos< startpos) {
                return new strGetEndFeedback("Keinen Endpunkt gefunden.");
            }

            var txtBTW = DeKlammere(scriptText.Substring(startpos, pos - startpos));
            return new strGetEndFeedback(pos + witch.Length, txtBTW);



            //var klammern = 0;
            //var Gans = false;
            //var GeschwKlammern = false;
            //var EckigeKlammern = 0;

            //var pos = startpos - 1; // Letztes Zeichen noch berücksichtigen, könnte ja Klammer auf oder zu sein

            //var maxl = scriptText.Length;

            //do {

            //    if (pos > maxl) { return new strGetEndFeedback("Lesen über Textende"); }

            //    #region Klammer und " erledigen
            //    switch (scriptText.Substring(pos)) {

            //        // Gänsefüsschen, immer erlaubt
            //        case "\"":
            //            Gans = !Gans;
            //            break;

            //        // Ekige klammern könne in { oder ( vorkommen, immer erlaubt
            //        case "[":
            //            if (!Gans) {
            //                EckigeKlammern++;
            //            }
            //            break;

            //        case "]":
            //            if (!Gans) {
            //                if (EckigeKlammern <= 0) { return new strGetEndFeedback("] nicht gültig"); }
            //                EckigeKlammern--;
            //            }
            //            break;


            //        // Runde klammern können in { vorkommen
            //        case "(":
            //            if (!Gans) {
            //                if (EckigeKlammern > 0) { return new strGetEndFeedback("( nicht gültig"); }
            //                klammern++;
            //            }

            //            break;

            //        case ")":
            //            if (!Gans) {
            //                if (EckigeKlammern > 0) { return new strGetEndFeedback(") nicht gültig"); }
            //                if (klammern <= 0) { return new strGetEndFeedback(") nicht gültig"); }
            //                klammern--;
            //            }
            //            break;


            //        // Gescheifte klammern müssen immer sauber auf und zu gemacht werdrn!
            //        case "{":
            //            if (!Gans) {
            //                if (klammern > 0) { return new strGetEndFeedback("{ nicht gültig"); }
            //                if (EckigeKlammern > 0) { return new strGetEndFeedback("{ nicht gültig"); }
            //                if (GeschwKlammern) { return new strGetEndFeedback("{ nicht gültig"); }
            //                GeschwKlammern = true;
            //            }
            //            break;

            //        case "}":
            //            if (!Gans) {
            //                if (klammern > 0) { return new strGetEndFeedback("} nicht gültig"); }
            //                if (EckigeKlammern > 0) { return new strGetEndFeedback("} nicht gültig"); }
            //                if (!GeschwKlammern) { return new strGetEndFeedback("} nicht gültig"); }
            //                GeschwKlammern = false;
            //            }
            //            break;
            //    }
            //    #endregion



            //    if (klammern == 0 && !Gans && !GeschwKlammern && EckigeKlammern == 0) {

            //        foreach (var thisEnd in EndSequence) {


            //            if (pos + thisEnd.Length <= maxl) {

            //                if (scriptText.Substring(pos, thisEnd.Length).ToLower() == thisEnd.ToLower()) {

            //                    var txtBTW = DeKlammere(scriptText.Substring(startpos, pos - startpos));
            //                    return new strGetEndFeedback(pos + thisEnd.Length, txtBTW);
            //                }

            //            }

            //        }



            //    }

            //    pos++;
            //} while (true);


        }


        public string DeKlammere(string txtBTW) {

            string otxt;

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



        public strGetEndFeedback ReplaceVariable(string txt, List<Variable> vars) {
            var t = "¶";
            if (txt.Contains(t)) {
                return new strGetEndFeedback("Unerlaubtes Zeichen");
            }

            txt = txt.Replace(" ", t + " " + t);
            txt = txt.Replace("\t", t + "\t" + t);
            txt = txt.Replace("\r", t + "\r" + t);
            txt = txt.Replace("\n", t + "\n" + t);
            txt = txt.Replace("+", t + "+" + t);
            txt = txt.Replace("-", t + "-" + t);
            txt = txt.Replace("*", t + "*" + t);
            txt = txt.Replace("/", t + "/" + t);
            txt = txt.Replace("(", t + "(" + t);
            txt = txt.Replace(")", t + ")" + t);
            txt = txt.Replace("|", t + "|" + t);
            txt = txt.Replace("&", t + "&" + t);
            txt = txt.Replace("=", t + "=" + t);
            txt = txt.Replace("!", t + "!" + t);
            txt = txt.Replace("<", t + "<" + t);
            txt = txt.Replace(">", t + ">" + t);
            txt = txt.Replace("\"", t + "\"" + t);
            txt = txt.Replace(",", t + "," + t);

            foreach (var thisV in vars) {
                if (thisV.Type == Skript.Enums.enVariableDataType.NotDefinedYet) {
                    if (txt.ToLower().Contains(t + thisV.Name.ToLower() + t)) {
                        return new strGetEndFeedback("Variable " + thisV + " ist keinem Typ zugeordnet");
                    }
                }

                txt = txt.Replace(t + thisV.Name + t, t + thisV.ValueString + t);
            }

            txt = txt.Replace(t, string.Empty);
            return new strGetEndFeedback(0, txt);

        }


    }


}
