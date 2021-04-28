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
using BlueBasics.Interfaces;
using Skript.Enums;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueScript {
    public abstract class Method : IReadableText {

        public abstract List<string> Comand(Script s);

        public abstract string Description { get; }

        public abstract string Syntax { get; }
        public abstract string StartSequence { get; }
        public abstract string EndSequence { get; }
        public abstract bool GetCodeBlockAfter { get; }
        public abstract enVariableDataType Returns { get; }

        public abstract List<enVariableDataType> Args { get; }
        public abstract bool EndlessArgs { get; }

        public abstract strDoItFeedback DoIt(strCanDoFeedback infos, Script s);


        public strCanDoFeedback CanDo(string scriptText, int pos, bool expectedvariablefeedback, Script s) {


            if (!expectedvariablefeedback && Returns != enVariableDataType.Null) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }

            if (expectedvariablefeedback && Returns == enVariableDataType.Null) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }


            var maxl = scriptText.Length;

            //if (parent != null) {
            //    var al = AllowedInIDs;
            //    if (al != null && !al.Contains(parent.ID)) {
            //        return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            //    }
            //}

            foreach (var thiscomand in Comand(s)) {


                var comandtext = thiscomand + StartSequence;
                var l = comandtext.Length;

                if (pos + l < maxl) {


                    if (scriptText.Substring(pos, l).ToLower() == comandtext.ToLower()) {

                        var f = GetEnd(scriptText, pos + thiscomand.Length, StartSequence.Length);
                        if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                            return new strCanDoFeedback(f.ContinuePosition, "Fehler bei " + comandtext + ": " + f.ErrorMessage, true);
                        }

                        var cont = f.ContinuePosition;
                        var codebltxt = string.Empty;

                        if (GetCodeBlockAfter) {

                            do {
                                if (cont >= maxl) { return new strCanDoFeedback(f.ContinuePosition, "Kein nachfolgender Codeblock bei " + comandtext, true); }
                                if (scriptText.Substring(cont, 1) == "{") { break; }
                                if (scriptText.Substring(cont, 1) != "¶") { return new strCanDoFeedback(f.ContinuePosition, "Kein nachfolgender Codeblock bei " + comandtext, true); }
                                cont++;
                                s.Line++;
                            } while (true);


                            (var posek, var _) = NextText(scriptText, cont, GeschKlammerZu, false, false);
                            if (posek < cont) {
                                return new strCanDoFeedback(cont, "Kein Codeblock Ende bei " + comandtext, true);
                            }

                            codebltxt = scriptText.Substring(cont + 1, posek - cont - 1);
                            //if (string.IsNullOrEmpty(codebltxt)) {
                            //    return new strCanDoFeedback(cont, "Leerer Codeblock bei " + comandtext, true);
                            //}
                            cont = posek + 1;
                        }

                        return new strCanDoFeedback(cont, comandtext, f.AttributeText, codebltxt);
                    }
                }
            }

            return new strCanDoFeedback(pos, "Kann nicht geparst werden", false);
        }

        private strGetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence) {

            (var pos, var witch) = NextText(scriptText, startpos, new List<string>() { EndSequence }, false, false);


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

        public static strGetEndFeedback ReplaceComands(string txt, IEnumerable<Method> comands, Script s) {
            var c = new List<string>();
            foreach (var thisc in comands) {

                if (thisc.Returns != enVariableDataType.Null) {
                    foreach (var thiscs in thisc.Comand(s)) {
                        c.Add(thiscs + thisc.StartSequence);
                    }
                }
            }


            var posc = 0;

            do {

                (var pos, var _) = NextText(txt, posc, c, true, false);

                if (pos < 0) { return new strGetEndFeedback(0, txt); }

                var f = Script.ComandOnPosition(txt, pos, s, true);

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

        public static strGetEndFeedback ReplaceVariable(string txt, List<Variable> vars) {


            var posc = 0;

            var v = vars.AllNames();

            do {

                (var pos, var witch) = NextText(txt, posc, v, true, true);

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

        public static List<string> SplitAttributeToString(string attributtext) {
            if (string.IsNullOrEmpty(attributtext)) { return null; }

            var attributes = new List<string>();

            #region Liste der Attribute splitten
            var posc = 0;
            do {
                (var pos, var _) = NextText(attributtext, posc, Komma, false, false);
                if (pos < 0) {
                    attributes.Add((attributtext.Substring(posc)).DeKlammere(true, false, false, true));
                    break;
                }

                attributes.Add((attributtext.Substring(posc, pos - posc)).DeKlammere(true, true, false, true));
                posc = pos + 1;

            } while (true);

            #endregion

            return attributes;
        }


        public static strSplittedAttributesFeedback SplitAttributeToVars(string attributtext, Script s, List<enVariableDataType> types, bool EndlessArgs) {
            var attributes = SplitAttributeToString(attributtext);
            if (attributes == null || attributes.Count == 0) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Allgemeiner Fehler."); }

            if (attributes.Count < types.Count) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Zu wenige Attribute erhalten."); ; }
            if (!EndlessArgs && attributes.Count > types.Count) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Zu viele Attribute erhalten."); }


            //  Variablen und Routinen ersetzen
            var vars = new List<Variable>();


            for (var n = 0; n < attributes.Count; n++) {

                var lb = attributes[n].Count(c => c == '¶');

                attributes[n] = attributes[n].RemoveChars("¶");

                enVariableDataType exceptetType;
                if (n < types.Count) {
                    exceptetType = types[n];
                } else {
                    exceptetType = types[types.Count - 1];
                }

                Variable v = null;


                if (exceptetType.HasFlag(enVariableDataType.Variable)) {
                    if (!Variable.IsValidName(attributes[n])) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1).ToString()); }
                    v = s.Variablen.Get(attributes[n]);
                    if (v == null) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1).ToString()); }

                } else {
                    v = new Variable("dummy", attributes[n], s);
                    if (v == null) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.BerechnungFehlgeschlagen, "Berechnungsfehler bei Attribut " + (n + 1).ToString()); }

                }


                if (!exceptetType.HasFlag(v.Type)) {

                    if (v.Type == enVariableDataType.Error) {
                        return new strSplittedAttributesFeedback(enSkriptFehlerTyp.BerechnungFehlgeschlagen, "Attribut " + (n + 1).ToString() + ": " + v.Coment);
                    }


                    if (exceptetType == enVariableDataType.Integer) {
                        if (v.Type != enVariableDataType.Numeral) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.FalscherDatentyp, "Attribut " + (n + 1).ToString() + " ist keine Ganzahl."); }
                        if (v.ValueDouble != (int)v.ValueDouble) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.FalscherDatentyp, "Attribut " + (n + 1).ToString() + " ist keine Ganzahl."); }
                    } else {
                        return new strSplittedAttributesFeedback(enSkriptFehlerTyp.FalscherDatentyp, "Attribut " + (n + 1).ToString() + " ist nicht der erwartete Typ");
                    }

                }
                vars.Add(v);
                if (s != null) { s.Line += lb; }
            }

            return new strSplittedAttributesFeedback(vars);
        }

        public string ReadableText() => Syntax;
        public QuickImage SymbolForReadableText() => null;
    }
}
