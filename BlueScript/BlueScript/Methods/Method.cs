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
using BlueBasics.Interfaces;
using Skript.Enums;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueScript {

    public abstract class Method : IReadableText {

        #region Properties

        public abstract List<enVariableDataType> Args { get; }

        public abstract string Description { get; }

        public abstract bool EndlessArgs { get; }

        public abstract string EndSequence { get; }

        public abstract bool GetCodeBlockAfter { get; }

        public abstract enVariableDataType Returns { get; }

        public abstract string StartSequence { get; }

        public abstract string Syntax { get; }

        #endregion

        #region Methods

        public static strGetEndFeedback ReplaceComands(string txt, IEnumerable<Method> comands, Script s) {
            List<string> c = new();
            foreach (var thisc in comands) {
                if (thisc.Returns != enVariableDataType.Null) {
                    foreach (var thiscs in thisc.Comand(s)) {
                        c.Add(thiscs + thisc.StartSequence);
                    }
                }
            }
            var posc = 0;
            do {
                (var pos, var _) = NextText(txt, posc, c, true, false, KlammernStd);
                if (pos < 0) { return new strGetEndFeedback(0, txt); }
                var f = Script.ComandOnPosition(txt, pos, s, true);
                if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                    return new strGetEndFeedback(f.ErrorMessage);
                }
                txt = txt.Substring(0, pos) + f.Value + txt.Substring(f.Position);
                posc = pos;
            } while (true);
        }

        public static strGetEndFeedback ReplaceVariable(string txt, Script s) {
            var posc = 0;
            var v = s.Variablen.AllNames();

            do {
                (var pos, var which) = NextText(txt, posc, v, true, true, KlammernStd);

                if (txt.Contains("~")) {
                    (var postmp, var _) = NextText(txt, posc, Tilde, false, false, KlammernStd);
                    if ((postmp >= 0 && postmp < pos) || pos < 0) { pos = postmp; which = "~"; }
                }

                if (pos < 0) { return new strGetEndFeedback(0, txt); }

                Variable thisV;
                int endz;

                if (which == "~") {
                    (var pose, var _) = NextText(txt, pos + 1, Tilde, false, false, KlammernStd);
                    if (pose <= pos) { return new strGetEndFeedback("Variablen-Findung End-~-Zeichen nicht gefunden."); }
                    var x = new Variable("dummy", txt.Substring(pos + 1, pose - pos - 1), s);
                    if (x.Type != enVariableDataType.String) { return new strGetEndFeedback("Fehler beim Berechnen des Variablen-Namens."); }
                    thisV = s.Variablen.Get(x.ValueString);
                    endz = pose + 1;
                } else {
                    thisV = s.Variablen.Get(which);
                    endz = pos + which.Length;
                }
                if (thisV == null) { return new strGetEndFeedback("Variablen-Fehler " + which); }

                if (thisV.Type == enVariableDataType.NotDefinedYet) { return new strGetEndFeedback("Variable " + thisV.Name + " ist keinem Typ zugeordnet"); }
                if (thisV.Type == enVariableDataType.List && !string.IsNullOrEmpty(thisV.ValueString) && !thisV.ValueString.EndsWith("\r")) { return new strGetEndFeedback("List-Variable " + thisV.Name + " fehlerhaft"); }

                txt = txt.Substring(0, pos) + Variable.ValueForReplace(thisV.ValueString, thisV.Type) + txt.Substring(endz);
                posc = pos;
            } while (true);
        }

        public static List<string> SplitAttributeToString(string attributtext) {
            if (string.IsNullOrEmpty(attributtext)) { return null; }
            List<string> attributes = new();

            #region Liste der Attribute splitten

            var posc = 0;
            do {
                (var pos, var _) = NextText(attributtext, posc, Komma, false, false, KlammernStd);
                if (pos < 0) {
                    attributes.Add(attributtext.Substring(posc).DeKlammere(true, false, false, true));
                    break;
                }
                attributes.Add(attributtext.Substring(posc, pos - posc).DeKlammere(true, false, false, true));
                posc = pos + 1;
            } while (true);

            #endregion Liste der Attribute splitten

            return attributes;
        }

        public static strSplittedAttributesFeedback SplitAttributeToVars(string attributtext, Script s, List<enVariableDataType> types, bool endlessArgs) {
            if (types.Count == 0) {
                return string.IsNullOrEmpty(attributtext)
                    ? new strSplittedAttributesFeedback(new List<Variable>())
                    : new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.");
            }

            var attributes = SplitAttributeToString(attributtext);
            if (attributes == null || attributes.Count == 0) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Allgemeiner Fehler."); }
            if (attributes.Count < types.Count) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Zu wenige Attribute erhalten."); ; }
            if (!endlessArgs && attributes.Count > types.Count) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.AttributAnzahl, "Zu viele Attribute erhalten."); }

            //  Variablen und Routinen ersetzen
            List<Variable> vars = new();
            for (var n = 0; n < attributes.Count; n++) {
                var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
                attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

                var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

                // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
                Variable v = null;
                if (exceptetType.HasFlag(enVariableDataType.Variable)) {
                    var varn = attributes[n];
                    if (varn.StartsWith("~") && varn.EndsWith("~")) {
                        var tmp2 = new Variable("dummy", varn.Substring(1, varn.Length - 2), s);
                        if (tmp2.Type != enVariableDataType.String) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.VariablenNamenBerechnungFehler, "Variablenname konnte nicht berechnet werden bei Attribut " + (n + 1).ToString()); }
                        varn = tmp2.ValueString;
                    }

                    if (!Variable.IsValidName(varn)) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1).ToString()); }
                    v = s.Variablen.Get(varn);
                    if (v == null) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1).ToString()); }
                } else {
                    v = new Variable("dummy", attributes[n], s);
                    if (v == null) { return new strSplittedAttributesFeedback(enSkriptFehlerTyp.BerechnungFehlgeschlagen, "Berechnungsfehler bei Attribut " + (n + 1).ToString()); }
                }

                // Den Typ der Variable checken
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

        public strCanDoFeedback CanDo(string scriptText, int pos, bool expectedvariablefeedback, Script s) {
            if (!expectedvariablefeedback && Returns != enVariableDataType.Null) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }
            if (expectedvariablefeedback && Returns == enVariableDataType.Null) {
                return new strCanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false);
            }
            var maxl = scriptText.Length;

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
                            (var posek, var _) = NextText(scriptText, cont, GeschKlammerZu, false, false, KlammernStd);
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

        public abstract List<string> Comand(Script s);

        public abstract strDoItFeedback DoIt(strCanDoFeedback infos, Script s);

        public string HintText() {
            var co = "Syntax:\r\n";
            co += "~~~~~~\r\n";
            co = co + Syntax + "\r\n";
            co += "\r\n";
            co += "Argumente:\r\n";
            co += "~~~~~~~~~~\r\n";
            for (var z = 0; z < Args.Count(); z++) {
                co = co + "  - Argument " + (z + 1).ToString() + ": " + Args[z].ToString();
                if (z == Args.Count() - 1 && EndlessArgs) {
                    co += " -> Dieses Argument kann beliebig oft wiederholt werden";
                }
                co += "\r\n";
            }
            co += "\r\n";
            co += "Rückgabe:\r\n";
            co += "~~~~~~~~~\r\n";
            co = co + "  - Rückgabetyp: " + Returns.ToString() + "\r\n";
            co += "\r\n";
            co += "Beschreibung:\r\n";
            co += "~~~~~~~~~~~~\r\n";
            co = co + Description + "\r\n";
            return co;
        }

        public string ReadableText() => Syntax;

        public QuickImage SymbolForReadableText() => null;

        private strGetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence) {
            //z.B: beim Befehl DO
            if (string.IsNullOrEmpty(EndSequence)) {
                return new strGetEndFeedback(startpos, string.Empty);
            }

            (var pos, var which) = NextText(scriptText, startpos, new List<string>() { EndSequence }, false, false, KlammernStd);
            if (pos < startpos) {
                return new strGetEndFeedback("Keinen Endpunkt gefunden.");
            }

            var txtBTW = scriptText.Substring(startpos + lenghtStartSequence, pos - startpos - lenghtStartSequence);
            return new strGetEndFeedback(pos + which.Length, txtBTW);
        }

        #endregion
    }
}