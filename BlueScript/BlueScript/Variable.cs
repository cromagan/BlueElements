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

#nullable enable

using System;
using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BlueScript.Methods;
using BlueScript.Structuren;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueScript.Extensions;

#nullable enable

namespace BlueScript {

    public static class VariableExtensions {

        #region Methods

        public static void AddComent(this List<Variable> vars, string additionalComent) {
            foreach (var thisvar in vars) {
                if (!string.IsNullOrEmpty(thisvar.Coment)) { thisvar.Coment += "\r"; }
                thisvar.Coment += additionalComent;
            }
        }

        public static List<string> AllNames(this List<Variable>? vars) => vars.Select(thisvar => thisvar.Name).ToList();

        public static List<string> AllValues(this List<Variable>? vars) => vars.Select(thisvar => thisvar.ValueString).ToList();

        public static Variable? Get(this List<Variable>? vars, string name) {
            if (vars == null || vars.Count == 0) { return null; }

            return vars.FirstOrDefault(thisv => !thisv.SystemVariable && string.Equals(thisv.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            return v != null && v.ValueBool;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static double GetDouble(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            return v == null ? 0d : v.ValueDouble;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static int GetInt(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            return v == null ? 0 : v.ValueInt;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static List<string> GetList(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            return v == null ? new List<string>() : v.ValueListString;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            return v == null ? string.Empty : v.ValueString;
        }

        public static Variable? GetSystem(this List<Variable?> vars, string name) => vars.FirstOrDefault(thisv => thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper());

        public static void RemoveWithComent(this List<Variable> vars, string? coment) {
            var z = 0;
            do {
                if (vars[z].Coment != null && vars[z].Coment.Contains(coment)) {
                    vars.RemoveAt(z);
                } else {
                    z++;
                }
            } while (z < vars.Count);
        }

        //public static void ScriptFinished(this List<Variable> vars) {
        //    if (vars == null || vars.Count == 0) { return; }
        //    foreach (var thisv in vars) {
        //        thisv.ScriptFinished();
        //    }
        //}

        /// <summary>
        /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this List<Variable> vars, string name, string value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new Variable(name);
                vars.Add(v);
            }
            v.Readonly = false; // sonst werden keine Daten geschrieben
            v.Type = enVariableDataType.String;
            v.ValueString = value;
            v.Readonly = true;
        }

        /// <summary>
        /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this List<Variable> vars, string name, double value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new Variable(name);
                vars.Add(v);
            }
            v.Readonly = false; // sonst werden keine Daten geschrieben
            v.Type = enVariableDataType.Numeral;
            v.ValueDouble = value;
            v.Readonly = true;
        }

        /// <summary>
        /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this List<Variable> vars, string name, List<string?>? value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new Variable(name);
                vars.Add(v);
            }
            v.Readonly = false; // sonst werden keine Daten geschrieben
            v.Type = enVariableDataType.List;
            v.ValueListString = value;

            //TODO: 29.09.2021 bei Zeiten wieder entfernen
            if (v.ValueListString.Count != value.Count) { Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Convertierung fehlgechlagen!"); }

            v.Readonly = true;
        }

        /// <summary>
        /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this List<Variable> vars, string name, bool value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new Variable(name);
                vars.Add(v);
            }
            v.Readonly = false; // sonst werden keine Daten geschrieben
            v.Type = enVariableDataType.Bool;
            v.ValueString = value ? "true" : "false";
            v.Readonly = true;
        }

        #endregion
    }

    public class Variable {

        #region Fields

        /// <summary>
        /// Listen werden immer mit einem \r am Ende gespeichert, ausser die Länge ist 0
        /// </summary>
        private string _valueString = string.Empty;

        #endregion

        #region Constructors

        public Variable(string name) {
            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();
        }

        public Variable(string name, string attributesText, Script? s) : this(name) {
            var txt = AttributeAuflösen(attributesText, s);
            if (!string.IsNullOrEmpty(txt.ErrorMessage)) { SetError(txt.ErrorMessage); return; }

            #region Testen auf Bool

            if (txt.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                txt.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Bool) { SetError("Variable ist kein Boolean"); return; }
                ValueString = txt.Value;
                Type = enVariableDataType.Bool;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf String

            if (txt.Value.Length > 1 && txt.Value.StartsWith("\"") && txt.Value.EndsWith("\"")) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.String) { SetError("Variable ist kein String"); return; }
                var tmp = txt.Value.Substring(1, txt.Value.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
                //tmp = tmp.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
                if (tmp.Contains("\"")) { SetError("Verkettungsfehler"); return; } // Beispiel: s ist nicht definiert und "jj" + s + "kk
                ValueString = tmp;
                Type = enVariableDataType.String;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Bitmap

            if (txt.Value.Length > 4 && txt.Value.StartsWith(ImageKennung + "\"") && txt.Value.EndsWith("\"" + ImageKennung)) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Bitmap) { SetError("Variable ist kein Bitmap"); return; }
                ValueString = txt.Value.Substring(2, txt.Value.Length - 4);
                Type = enVariableDataType.Bitmap;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Liste mit Strings

            if (txt.Value.Length > 1 && txt.Value.StartsWith("{") && txt.Value.EndsWith("}")) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.List) { SetError("Variable ist keine Liste"); return; }
                var t = txt.Value.DeKlammere(false, true, false, true);

                if (string.IsNullOrEmpty(t)) {
                    ValueListString = new List<string>(); // Leere Liste
                } else {
                    var l = Method.SplitAttributeToVars(t, s, new List<enVariableDataType> { enVariableDataType.String }, true);
                    if (!string.IsNullOrEmpty(l.ErrorMessage)) { SetError(l.ErrorMessage); return; }
                    ValueListString = l.Attributes.AllValues();
                }
                Type = enVariableDataType.List;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Object

            if (txt.Value.Length > 2 && txt.Value.StartsWith(ObjectKennung + "\"") && txt.Value.EndsWith("\"" + ObjectKennung)) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Object) { SetError("Variable ist kein Objekt"); return; }
                ValueString = txt.Value.Substring(2, txt.Value.Length - 4);
                Type = enVariableDataType.Object;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Number

            if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Numeral) { SetError("Variable ist keine Zahl"); return; }

            if (txt.Value.IsDouble()) {
                ValueDouble = DoubleParse(txt.Value);
                Type = enVariableDataType.Numeral;
                Readonly = true;
                return;
            }

            #endregion

            SetError("Unbekannter Typ: " + txt.Value);
        }

        public Variable(string name, string value, enVariableDataType type, bool ronly, bool system, string coment) : this(name) {
            Name = system ? "*" + name.ToLower() : name.ToLower();
            ValueString = value;
            Type = type;
            Readonly = ronly;
            SystemVariable = system;
            Coment = coment;
        }

        public Variable(string name, List<string?>? value, bool ronly, bool system, string coment) : this(name) {
            Name = system ? "*" + name.ToLower() : name.ToLower();
            ValueListString = value;
            Type = enVariableDataType.List;
            Readonly = ronly;
            SystemVariable = system;
            Coment = coment;
        }

        public Variable(string name, string value, enVariableDataType type) : this(name) {
            ValueString = value;
            Type = type;
        }

        #endregion

        #region Properties

        public string Coment { get; set; }

        /// <summary>
        /// Variablen-Namen werden immer in Kleinbuchstaben gespeichert.
        /// </summary>
        public string Name { get; set; }

        public bool Readonly { get; set; }

        public bool SystemVariable { get; set; }

        public enVariableDataType Type { get; set; }

        public bool ValueBool {
            get => _valueString == "true";
            set => ValueString = value ? "true" : "false";
        }

        public double ValueDouble {
            get => DoubleParse(_valueString);
            set => ValueString = value.ToString();
        }

        public int ValueInt => IntParse(_valueString);

        /// <summary>
        /// Es wird der String in eine Liste umgewandelt (bzw. andersrum). Leere Einträge auch am Ende bleiben erhalten.
        /// </summary>
        public List<string> ValueListString {
            get {
                if (string.IsNullOrEmpty(_valueString)) { return new List<string>(); }
                if (!_valueString.EndsWith("\r")) {
                    Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Objekttypfehler: " + _valueString);
                }
                var x = _valueString.Substring(0, _valueString.Length - 1);
                return string.IsNullOrEmpty(x) ? new List<string> { string.Empty } : x.SplitByCrToList();
            }
            set => ValueString = value == null || value.Count == 0 ? string.Empty : value.JoinWithCr() + "\r";
        }

        /// <summary>
        /// Der direkte Text, der in der Variable gespeichert ist.
        /// Ohne Anführungsstrichchen. Falls es in Wahrheit eine Liste ist, ist der Text 'gejoined' mit \r
        /// </summary>
        public string ValueString {
            get => _valueString;
            set {
                if (Readonly) { return; }
                _valueString = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert (außer List, wegen dem Endzeichne) und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
            }
        }

        #endregion

        #region Methods

        public static DoItFeedback AttributeAuflösen(string txt, Script? s) {
            //Develop.CheckStackForOverflow();

            #region Prüfen, Ob noch mehre Klammern da sind, oder Anfangs/End-Klammern entfernen

            if (txt.StartsWith("(")) {
                var (pose, _) = NextText(txt, 0, KlammerZu, false, false, KlammernStd);
                if (pose < txt.Length - 1) {
                    /// Wir haben so einen Fall: (true) || (true)
                    var txt1 = AttributeAuflösen(txt.Substring(1, pose - 1), s);
                    return !string.IsNullOrEmpty(txt1.ErrorMessage)
                        ? new DoItFeedback("Befehls-Berechnungsfehler in ():" + txt1.ErrorMessage)
                        : AttributeAuflösen(txt1.Value + txt.Substring(pose + 1), s);
                }
            }

            txt = txt.DeKlammere(true, false, false, true);

            #endregion

            #region Auf boolsche AndAlso und OrElse prüfen und nur die nötigen ausführen

            #region AndAlso

            var (uu, _) = NextText(txt, 0, Method_if.UndUnd, false, false, KlammernStd);
            if (uu > 0) {
                var txt1 = AttributeAuflösen(txt.Substring(0, uu), s);
                return !string.IsNullOrEmpty(txt1.ErrorMessage) ? new DoItFeedback("Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage)
                    : txt1.Value == "false" ? txt1
                    : AttributeAuflösen(txt.Substring(uu + 2), s);
            }

            #endregion AndAlso

            #region OrElse

            var (oo, _) = NextText(txt, 0, Method_if.OderOder, false, false, KlammernStd);
            if (oo > 0) {
                var txt1 = AttributeAuflösen(txt.Substring(0, oo), s);
                return !string.IsNullOrEmpty(txt1.ErrorMessage)
                    ? new DoItFeedback("Befehls-Berechnungsfehler vor ||: " + txt1.ErrorMessage)
                    : txt1.Value == "true" ? txt1 : AttributeAuflösen(txt.Substring(oo + 2), s);
            }

            #endregion OrElse

            #endregion

            #region Variablen ersetzen

            if (s != null) {
                var t = Method.ReplaceVariable(txt, s);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                    return new DoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
                }
                txt = t.AttributeText;
            }

            #endregion Variablen ersetzen

            #region Routinen ersetzen, vor den Klammern, das ansonsten Min(x,y,z) falsch anschlägt

            if (s != null) {
                var t2 = Method.ReplaceComands(txt, Script.Comands, s);
                if (!string.IsNullOrEmpty(t2.ErrorMessage)) {
                    return new DoItFeedback("Befehls-Berechnungsfehler: " + t2.ErrorMessage);
                }
                txt = t2.AttributeText;
            }

            #endregion

            #region Klammern am Ende berechnen, das ansonsten Min(x,y,z) falsch anschlägt

            var (posa, _) = NextText(txt, 0, KlammerAuf, false, false, KlammernStd);
            if (posa > -1) {
                var (pose, _) = NextText(txt, posa, KlammerZu, false, false, KlammernStd);
                if (pose <= posa) { return DoItFeedback.Klammerfehler(); }
                var tmp = AttributeAuflösen(txt.Substring(posa + 1, pose - posa - 1), s);
                return !string.IsNullOrEmpty(tmp.ErrorMessage)
                    ? tmp
                    : AttributeAuflösen(txt.Substring(0, posa) + tmp.Value + txt.Substring(pose + 1), s);
            }

            #endregion

            //#region Vergleichsoperatoren ersetzen und vereinfachen

            //(var pos, var _) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false, KlammernStd);
            //if (pos >= 0) {
            //    var tmp = Method_if.GetBool(txt);
            //    if (tmp == null) { return new strDoItFeedback("Der Inhalt zwischen den Klammern (" + txt + ") konnte nicht berechnet werden."); }
            //    txt = tmp;
            //}

            //#endregion

            #region Auf Restliche Boolsche Operationen testen

            //foreach (var check in Method_if.VergleichsOperatoren) {
            var (i, check) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false, KlammernStd);
            if (i > -1) {
                if (i < 1 && check != "!") { return new DoItFeedback("Operator (" + check + ") am String-Start nicht erlaubt: " + txt); } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!
                if (i >= txt.Length - 1) { return new DoItFeedback("Operator (" + check + ") am String-Ende nicht erlaubt: " + txt); } // siehe oben

                #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

                //var start = i - 1;
                //var ende = i + check.Length;
                //var trenn = "(!|&<>=)";
                //var gans = false;

                #region Ersten Wert als s1 ermitteln

                //do {
                //    if (start < 0) { break; }
                //    var ze = txt.Substring(start, 1);
                //    if (!gans && trenn.Contains(ze)) { break; }
                //    if (ze == "\"") { gans = !gans; }
                //    start--;
                //} while (true);

                //(var op, var _) = NextText(txt, 0, Method_if.UndUnd, false, false, KlammernStd);
                //if (uu > 0) {
                //    var txt1 = AttributeAuflösen(txt.Substring(0, uu), s);
                //    return !string.IsNullOrEmpty(txt1.ErrorMessage) ? new strDoItFeedback("Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage)
                //        : txt1.Value == "false" ? txt1
                //        : AttributeAuflösen(txt.Substring(uu + 2), s);
                //}

                var s1 = txt.Substring(0, i);
                if (string.IsNullOrEmpty(s1) && check != "!") { return new DoItFeedback("Wert vor Operator (" + check + ") nicht gefunden: " + txt); }
                if (!string.IsNullOrEmpty(s1)) {
                    var tmp1 = AttributeAuflösen(s1, s);
                    if (!string.IsNullOrEmpty(tmp1.ErrorMessage)) { return new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage); }
                    s1 = tmp1.Value;
                }

                #endregion

                #region Zweiten Wert als s2 ermitteln

                //do {
                //    if (ende >= txt.Length) { break; }
                //    var ze = txt.Substring(ende, 1);
                //    if (!gans && trenn.Contains(ze)) { break; }
                //    if (ze == "\"") { gans = !gans; }
                //    ende++;
                //} while (true);

                var s2 = txt.Substring(i + check.Length);
                if (string.IsNullOrEmpty(s2)) {
                    return new DoItFeedback("Wert nach Operator (" + check + ") nicht gefunden: " + txt);
                }

                {
                    var tmp1 = AttributeAuflösen(s2, s);
                    if (!string.IsNullOrEmpty(tmp1.ErrorMessage)) { return new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage); }
                    s2 = tmp1.Value;
                }

                #endregion

                Variable? v1 = null;
                if (check != "!") { v1 = new Variable("dummy7", s1, null); }
                Variable v2 = new("dummy8", s2, null);

                // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
                if (v1 != null) {
                    if (v1.Type != v2.Type) { return new DoItFeedback("Typen unterschiedlich: " + txt); }
                    if (v1.Type is not enVariableDataType.Bool and
                                   not enVariableDataType.Numeral and
                                   not enVariableDataType.String) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                } else {
                    if (v2.Type != enVariableDataType.Bool) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                }

                #endregion

                var replacer = "false";
                switch (check) {
                    case "==":
                        if (v1.ValueString == v2.ValueString) { replacer = "true"; }
                        break;

                    case "!=":
                        if (v1.ValueString != v2.ValueString) { replacer = "true"; }
                        break;

                    case ">=":
                        if (v1.Type != enVariableDataType.Numeral) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (v1.ValueDouble >= v2.ValueDouble) { replacer = "true"; }
                        break;

                    case "<=":
                        if (v1.Type != enVariableDataType.Numeral) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (v1.ValueDouble <= v2.ValueDouble) { replacer = "true"; }
                        break;

                    case "<":
                        if (v1.Type != enVariableDataType.Numeral) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (v1.ValueDouble < v2.ValueDouble) { replacer = "true"; }
                        break;

                    case ">":
                        if (v1.Type != enVariableDataType.Numeral) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (v1.ValueDouble > v2.ValueDouble) { replacer = "true"; }
                        break;

                    case "||":
                        if (v1.Type != enVariableDataType.Bool) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        replacer = "false";
                        if (v1.ValueBool || v2.ValueBool) { replacer = "true"; }
                        break;

                    case "&&":
                        if (v1.Type != enVariableDataType.Bool) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (v1.ValueBool && v2.ValueBool) { replacer = "true"; }
                        break;

                    case "!":
                        // S1 dürfte eigentlich nie was sein: !False||!false
                        // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                        if (v2.Type != enVariableDataType.Bool) { return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt); }
                        if (!v2.ValueBool) { replacer = "true"; }
                        break;

                    default:
                        return new DoItFeedback("Operator (" + check + ") unbekannt: " + txt);
                }
                if (!string.IsNullOrEmpty(replacer)) { txt = replacer; }

                //return string.IsNullOrEmpty(replacer) ? string.Empty
                //                                      : txt.Substring(0, start + 1) + replacer + txt.Substring(ende);
            }

            #endregion

            #region testen auf bool

            var x = Method_if.GetBool(txt);
            if (x != null) { return new DoItFeedback(x, enVariableDataType.NotDefinedYet); }

            #endregion

            #region String Joinen -- UND RAUS AUS DER ROUTINE

            if (txt.Length > 1 && txt.StartsWith("\"") && txt.EndsWith("\"")) {
                var tmp = txt.Substring(1, txt.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
                tmp = tmp.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
                return tmp.Contains("\"")
                    ? new DoItFeedback("Verkettungsfehler: " + txt)
                    : new DoItFeedback("\"" + tmp + "\"", enVariableDataType.NotDefinedYet); // Beispiel: s ist nicht definiert und "jj" + s + "kk
            }

            #endregion

            #region Rechenoperatoren ersetzen und vereinfachen

            // String wird vorher abgebrochen, um nicht nochmal auf Gänsefüsschen zutesten
            var (pos2, _) = NextText(txt, 0, Berechnung.RechenOperatoren, false, false, KlammernStd);
            if (pos2 >= 0) {
                var erg = Berechnung.Ergebnis(txt);
                if (erg == null) { return new DoItFeedback("Berechnungsfehler der Formel: " + txt); }
                txt = erg.ToString();
            }

            #endregion

            return new DoItFeedback(txt, enVariableDataType.NotDefinedYet);
        }

        public static string GenerateObject(string objecttype, string value) => objecttype.ToUpper().ReduceToChars(Constants.Char_AZ + Constants.Char_Numerals) + "&" + value.ToNonCritical();

        public static bool IsValidName(string v) {
            v = v.ToLower();
            var vo = v;
            v = v.ReduceToChars(Constants.AllowedCharsVariableName);
            return v == vo && !string.IsNullOrEmpty(v);
        }

        public static string ValueForReplace(string value, enVariableDataType type) {
            switch (type) {
                case enVariableDataType.String:
                    return "\"" + value.RemoveCriticalVariableChars() + "\"";

                case enVariableDataType.Bool:
                    return value;

                case enVariableDataType.Numeral:
                    return value;

                case enVariableDataType.List:
                    if (string.IsNullOrEmpty(value)) { return "{ }"; }

                    if (!value.EndsWith("\r")) {
                        Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Objekttypfehler: " + value);
                    }

                    var x = value.Substring(0, value.Length - 1);
                    return "{\"" + x.RemoveCriticalVariableChars().SplitByCrToList().JoinWith("\", \"") + "\"}";

                case enVariableDataType.NotDefinedYet: // Wenn ne Routine die Werte einfach ersetzt.
                    return value;

                case enVariableDataType.Bitmap:
                    return ImageKennung + "\"" + value.RemoveCriticalVariableChars() + "\"" + ImageKennung;

                case enVariableDataType.Object:
                    return ObjectKennung + "\"" + value.RemoveCriticalVariableChars() + "\"" + ObjectKennung;

                case enVariableDataType.Variable:
                    return value;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    return value;
            }
        }

        public string ObjectData() {
            if (Type != enVariableDataType.Object) { return string.Empty; }
            var x = _valueString.SplitAndCutBy("&");
            return x == null || x.GetUpperBound(0) != 1
                    ? string.Empty
                    : x[1].FromNonCritical();
        }

        public bool ObjectType(string toCheck) {
            if (Type != enVariableDataType.Object) { return false; }

            var x = _valueString.SplitAndCutBy("&");
            return x != null && x.GetUpperBound(0) == 1 && x[0] == toCheck.ToUpper().ReduceToChars(Constants.Char_AZ + Constants.Char_Numerals);
        }

        public string ReplaceInText(string txt) {
            if (!txt.ToLower().Contains("~" + Name.ToLower() + "~")) { return txt; }

            return Type is not enVariableDataType.String and
                        not enVariableDataType.List and
                        not enVariableDataType.Integer and
                        not enVariableDataType.Bool and
                        not enVariableDataType.Numeral
                ? txt
                : txt.Replace("~" + Name + "~", ValueString, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public override string ToString() {
            var zusatz = string.Empty;
            if (Readonly) { zusatz = " [Read Only] "; }
            return Type switch {
                enVariableDataType.String => "{str} " + zusatz + Name + " = " + ValueString,
                enVariableDataType.Numeral => "{num} " + zusatz + Name + " = " + ValueString,
                enVariableDataType.Bool => "{bol} " + zusatz + Name + " = " + ValueString,
                enVariableDataType.List => "{lst} " + zusatz + Name + " = " + ValueString,
                enVariableDataType.Bitmap => "{bmp} " + zusatz + Name + " = [BitmapData]",
                enVariableDataType.Object => "{obj} " + zusatz + Name + " = [ObjectData]",
                enVariableDataType.Error => "{err} " + zusatz + Name + " = " + ValueString,
                _ => "{ukn} " + zusatz + Name + " = " + ValueString
            };
        }

        public Bitmap? ValueBitmap(Script s) => s.BitmapCache[ValueInt];

        private void SetError(string coment) {
            Readonly = false;
            Type = enVariableDataType.Error;
            ValueString = string.Empty;
            Readonly = true;
            Coment = coment;
        }

        #endregion
    }
}