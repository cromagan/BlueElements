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


        //public abstract bool ReturnsVoid { get; }

        //public abstract string ID { get; }
        public abstract List<string> Comand { get; }
        public abstract string StartSequence { get; }
        public abstract string EndSequence { get; }
        //public abstract List<string> AllowedInIDs { get; }
        public abstract bool GetCodeBlockAfter { get; }
        public abstract string Returns { get; }

        internal abstract strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen);


        public strCanDoFeedback CanDo(string scriptText, int pos, string expectedvariablefeedback) {


            if (string.IsNullOrEmpty(expectedvariablefeedback) != string.IsNullOrEmpty(Returns)) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }

            if (expectedvariablefeedback != "var" && Returns !="var" && expectedvariablefeedback != Returns) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }


            var maxl = scriptText.Length;

            //if (parent != null) {
            //    var al = AllowedInIDs;
            //    if (al != null && !al.Contains(parent.ID)) {
            //        return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            //    }
            //}

            foreach (var thiscomand in Comand) {


                var comandtext = thiscomand + StartSequence;
                var l = comandtext.Length;

                if (pos + l < maxl) {


                    if (scriptText.Substring(pos, l).ToLower() == comandtext.ToLower()) {

                        var f = GetEnd(scriptText, pos + thiscomand.Length, StartSequence.Length);
                        if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                            return new strCanDoFeedback(f.ContinuePosition, "Fehler bei " + Comand + ": " + f.ErrorMessage, true);
                        }

                        var cont = f.ContinuePosition;
                        var codebltxt = string.Empty;

                        if (GetCodeBlockAfter) {
                            if (f.ContinuePosition >= maxl ||  scriptText.Substring(f.ContinuePosition,1) !="{") {
                                return new strCanDoFeedback(f.ContinuePosition, "Kein nachfolgender Codeblock bei " + Comand, true);
                            }

                            var (posek, witch) = Script.NextText(scriptText, f.ContinuePosition, new List<string>() { "}" }, false, false, false);
                            if (posek < f.ContinuePosition) {
                                return new strCanDoFeedback(f.ContinuePosition, "Kein Codeblock Ende bei " + Comand, true);
                            }

                            codebltxt = scriptText.Substring(f.ContinuePosition + 1, posek - f.ContinuePosition - 1);
                            if (string.IsNullOrEmpty(codebltxt)) {
                                return new strCanDoFeedback(f.ContinuePosition, "Leerer Codeblock bei " + Comand, true);
                            }
                            cont = posek + 1;
                        }

                        return new strCanDoFeedback(cont, comandtext, f.AttributeText, codebltxt);
                    }
                }
            }

            return new strCanDoFeedback(pos, "Kann nicht geparst werden", false);
        }


        strGetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence) {

            var (pos, witch) = Script.NextText(scriptText, startpos, new List<string>() { EndSequence }, false, false, false);


            if (pos < startpos) {
                return new strGetEndFeedback("Keinen Endpunkt gefunden.");
            }

            var txtBTW = scriptText.Substring(startpos + lenghtStartSequence, pos - startpos - lenghtStartSequence);
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


        //public string DeKlammere(string txtBTW) {

        //    string otxt;

        //    do {
        //        otxt = txtBTW;
        //        txtBTW = txtBTW.Trim(" ");
        //        if (otxt.StartsWith("(") && otxt.EndsWith(")")) {
        //            BlueBasics.Develop.DebugPrint_NichtImplementiert();
        //        }
        //        if (otxt.StartsWith("{") && otxt.EndsWith("}")) {
        //            BlueBasics.Develop.DebugPrint_NichtImplementiert();
        //        }
        //        if (otxt.StartsWith("[") && otxt.EndsWith("]")) {
        //            BlueBasics.Develop.DebugPrint_NichtImplementiert();
        //        }


        //    } while (otxt != txtBTW);

        //    return txtBTW;
        //}

        public strGetEndFeedback ReplaceComands(string txt, IEnumerable<Method> comands, List<Variable> variablen) {
            var c = new List<string>();
            foreach (var thisc in comands) {

                if (!string.IsNullOrEmpty(thisc.Returns)) {
                    foreach (var thiscs in thisc.Comand) {
                        c.Add(thiscs + thisc.StartSequence);
                    }
                }
            }


            var posc = 0;

            do {

                var (pos, witch) = Script.NextText(txt, posc, c, true, false, true);

                if (pos < 0) { return new strGetEndFeedback(0, txt); }

                var f = Script.ComandOnPosition(txt, pos, variablen, "var");

                if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                    return new strGetEndFeedback(f.ErrorMessage);
                }

                txt = txt.Substring(0, pos) + f.Value + txt.Substring(f.Position);
                posc = pos;


            } while (true);



            //var pos = -1;

            //do {
            //    Alle Routinen ohne Void übergeben und ausführen und ersetzen
            //    if (Script.ComandOnPosition(txt,))
            //        Develop.DebugPrint_NichtImplementiert();

            //}



        }

        public strGetEndFeedback ReplaceVariable(string txt, List<Variable> vars) {


            var posc = 0;


            var v = vars.AllNames();



            do {

                var (pos, witch) = Script.NextText(txt, posc, v, true, true, true);

                if (pos < 0) { return new strGetEndFeedback(0, txt); }

                var thisV = vars.Get(witch);
                if (thisV == null) { return new strGetEndFeedback("Variablen-Fehler " + witch); }

                if (thisV.Type == Skript.Enums.enVariableDataType.NotDefinedYet) {
                    return new strGetEndFeedback("Variable " + witch + " ist keinem Typ zugeordnet");
                }


                txt = txt.Substring(0, pos) + thisV.ValueForReplace + txt.Substring(pos + witch.Length);
                posc = pos;

            } while (true);









            //var t = "¶";
            //if (txt.Contains(t)) {
            //    return new strGetEndFeedback("Unerlaubtes Zeichen");
            //}

            //txt = txt.Replace(" ", t + " " + t);
            //txt = txt.Replace("\t", t + "\t" + t);
            //txt = txt.Replace("\r", t + "\r" + t);
            //txt = txt.Replace("\n", t + "\n" + t);
            //txt = txt.Replace("+", t + "+" + t);
            //txt = txt.Replace("-", t + "-" + t);
            //txt = txt.Replace("*", t + "*" + t);
            //txt = txt.Replace("/", t + "/" + t);
            //txt = txt.Replace("(", t + "(" + t);
            //txt = txt.Replace(")", t + ")" + t);
            //txt = txt.Replace("|", t + "|" + t);
            //txt = txt.Replace("&", t + "&" + t);
            //txt = txt.Replace("=", t + "=" + t);
            //txt = txt.Replace("!", t + "!" + t);
            //txt = txt.Replace("<", t + "<" + t);
            //txt = txt.Replace(">", t + ">" + t);
            //txt = txt.Replace("\"", t + "\"" + t);
            //txt = txt.Replace(",", t + "," + t);

            //foreach (var thisV in vars) {
            //    if (thisV.Type == Skript.Enums.enVariableDataType.NotDefinedYet) {
            //        if (txt.ToLower().Contains(t + thisV.Name.ToLower() + t)) {
            //            return new strGetEndFeedback("Variable " + thisV + " ist keinem Typ zugeordnet");
            //        }
            //    }

            //    txt = txt.Replace(t + thisV.Name + t, t + thisV.ValueString + t);
            //}

            //txt = txt.Replace(t, string.Empty);
            //return new strGetEndFeedback(0, txt);

        }


        public List<string> SplitAttribute(string attributtext, List<Variable> variablen) {

            var t = ReplaceVariable(attributtext, variablen);
            if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                return null; // new strDoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
            }

            var t2 = ReplaceComands(t.AttributeText, Script.Comands, variablen);

            if (!string.IsNullOrEmpty(t2.ErrorMessage)) {
                return null; // new  strDoItFeedback("Befehls-Berechnungsfehler: " + t2.ErrorMessage);
            }

            var x = new List<string>();
            var pos = -1;
            var start = 0;
            var gänsef = false;
            var nt = t2.AttributeText;
            do {
                pos++;

                if (pos >= nt.Length) {
                    if (gänsef) { return null; }
                    x.Add(nt.Substring(start, pos - start));
                    return x;
                }

                var c = nt.Substring(pos, 1);

                if (c == "\"") { gänsef = !gänsef; }

                if (!gänsef && c == ",") {
                    x.Add(nt.Substring(start, pos - start));
                    start = pos + 1;
                }



            } while (true);




        }


    }


}
