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
using System.Collections.Generic;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using static BlueScript.Extensions;
using BlueScript.Methods;
using static BlueBasics.Extensions;

namespace BlueScript.Variables {

    public static class VariableExtensions {

        #region Methods

        public static void AddComent(this List<Variable> vars, string additionalComent) {
            foreach (var thisvar in vars) {
                if (!string.IsNullOrEmpty(thisvar.Coment)) {
                    thisvar.Coment += "\r";
                }

                thisvar.Coment += additionalComent;
            }
        }

        public static List<string> AllNames(this List<Variable>? vars) {
            if (vars != null) {
                return vars.Select(thisvar => thisvar.Name).ToList();
            }

            return new List<string>();
        }

        public static List<string> AllStringableNames(this List<Variable>? vars) {
            var l = new List<string>();
            if (vars != null) {
                foreach (var thisvar in vars) {
                    if (thisvar.ToStringPossible) { l.Add(thisvar.Name); }
                }
            }
            return l;
        }

        /// <summary>
        /// Gibt von allen Variablen, die ein String sind, den Inhalt ohne " am Anfang/Ende zurück.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        public static List<string> AllStringValues(this List<Variable>? vars) {
            var l = new List<string>();
            if (vars != null) {
                foreach (var thisvar in vars) {
                    if (thisvar is VariableString vs) { l.Add(vs.ValueString); }
                }
            }
            return l;
        }

        public static Variable? Get(this List<Variable>? vars, string name) {
            if (vars == null || vars.Count == 0) { return null; }

            return vars.FirstOrDefault(thisv =>
                !thisv.SystemVariable && string.Equals(thisv.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(this List<Variable> vars, string name) {
            var v = vars.Get(name);

            if (v is not VariableBool vf) {
                Develop.DebugPrint("Falscher Datentyp");
                return false;
            }

            return vf.ValueBool;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static double GetDouble(this List<Variable> vars, string name) {
            var v = vars.Get(name);

            if (v is not VariableFloat vf) {
                Develop.DebugPrint("Falscher Datentyp");
                return 0f;
            }

            return vf.ValueNum;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static int GetInt(this List<Variable> vars, string name) {
            var v = vars.Get(name);

            if (v is not VariableFloat vf) {
                Develop.DebugPrint("Falscher Datentyp");
                return 0;
            }

            return (int)vf.ValueNum;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static List<string> GetList(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return new List<string>(); }

            if (v is not VariableListString vf) {
                Develop.DebugPrint("Falscher Datentyp");
                return new List<string>();
            }

            return vf.ValueList;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(this List<Variable> vars, string name) {
            var v = vars.Get(name);

            if (v is not VariableString vf) {
                Develop.DebugPrint("Falscher Datentyp");
                return string.Empty;
            }

            return vf.ValueString;
        }

        public static Variable? GetSystem(this List<Variable> vars, string name) => vars.FirstOrDefault(thisv =>
                                                                          thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper());

        public static void RemoveWithComent(this List<Variable> vars, string coment) {
            var z = 0;
            do {
                if (vars[z].Coment.Contains(coment)) {
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
                v = new VariableString(name, string.Empty, false, false, string.Empty);
                vars.Add(v);
            }

            if (v is not VariableString vf) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Variablentyp falsch");
                return;
            }

            vf.Readonly = false; // sonst werden keine Daten geschrieben
            vf.ValueString = value;
            vf.Readonly = true;
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
                v = new VariableFloat(name);
                vars.Add(v);
            }

            if (v is not VariableFloat vf) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Variablentyp falsch");
                return;
            }

            vf.Readonly = false; // sonst werden keine Daten geschrieben
            vf.ValueNum = value;
            vf.Readonly = true;
        }

        /// <summary>
        /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void Set(this List<Variable> vars, string name, List<string> value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new VariableListString(name);
                vars.Add(v);
            }

            if (v is not VariableListString vf) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Variablentyp falsch");
                return;
            }

            vf.Readonly = false; // sonst werden keine Daten geschrieben
            vf.ValueList = value;
            vf.Readonly = true;
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
                v = new VariableBool(name);
                vars.Add(v);
            }

            if (v is not VariableBool vf) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Variablentyp falsch");
                return;
            }

            vf.Readonly = false; // sonst werden keine Daten geschrieben
            vf.ValueBool = value;
            vf.Readonly = true;
        }

        #endregion
    }

    public abstract class Variable : IComparable {

        #region Fields

        public readonly bool SystemVariable;

        private static long _dummyCount;

        #endregion

        #region Constructors

        protected Variable(string name, bool ronly, bool system, string coment) {
            Name = system ? "*" + name.ToLower() : name.ToLower();
            Readonly = ronly;
            SystemVariable = system;
            Coment = coment;
        }

        #endregion

        #region Properties

        public abstract int CheckOrder { get; }
        public string Coment { get; set; }

        public abstract bool GetFromStringPossible { get; }
        public abstract bool IsNullOrEmpty { get; }

        /// <summary>
        /// Variablen-Namen werden immer in Kleinbuchstaben gespeichert.
        /// </summary>
        public string Name { get; set; }

        public virtual string ReadableText => "Objekt: " + ShortName;

        public bool Readonly { get; set; }

        public abstract string ShortName { get; }

        public abstract bool ToStringPossible { get; }

        [Obsolete]
        public abstract VariableDataType Type { get; }

        public virtual string ValueForReplace {
            get {
                if (ToStringPossible) { Develop.DebugPrint(enFehlerArt.Fehler, "Routine muss überschrieben werden!"); }
                return ObjectKennung + "\"" + ShortName + ";" + Name + "\"" + ObjectKennung;
            }
        }

        #endregion

        #region Methods

        public static string DummyName() {
            if (_dummyCount >= long.MaxValue) { _dummyCount = 0; }
            _dummyCount++;
            return "dummy" + _dummyCount;
        }

        public static DoItFeedback GetVariableByParsing(string txt, Script? s) {

            #region Prüfen, Ob noch mehre Klammern da sind, oder Anfangs/End-Klammern entfernen

            if (txt.StartsWith("(")) {
                var (pose, _) = NextText(txt, 0, KlammerZu, false, false, KlammernStd);
                if (pose < txt.Length - 1) {
                    // Wir haben so einen Fall: (true) || (true)
                    var tmp = GetVariableByParsing(txt.Substring(1, pose - 1), s);
                    if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp.ErrorMessage); }
                    if (tmp.Variable == null) { return new DoItFeedback("Allgemeiner Befehls-Berechnungsfehler"); }
                    if (!tmp.Variable.ToStringPossible) { return new DoItFeedback("Falscher Variablentyp: " + tmp.Variable.ShortName); }
                    return GetVariableByParsing(tmp.Variable.ValueForReplace + txt.Substring(pose + 1), s);
                }
            }

            #endregion

            txt = txt.DeKlammere(true, false, false, true);

            #region Auf boolsche AndAlso und OrElse prüfen und nur die nötigen ausführen

            #region AndAlso

            var (uu, _) = NextText(txt, 0, Method_if.UndUnd, false, false, KlammernStd);
            if (uu > 0) {
                var txt1 = GetVariableByParsing(txt.Substring(0, uu), s);
                return !string.IsNullOrEmpty(txt1.ErrorMessage)
                    ? new DoItFeedback("Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage)
                    : txt1.Variable.ValueForReplace == "false"
                        ? txt1
                        : GetVariableByParsing(txt.Substring(uu + 2), s);
            }

            #endregion AndAlso

            #region OrElse

            var (oo, _) = NextText(txt, 0, Method_if.OderOder, false, false, KlammernStd);
            if (oo > 0) {
                var txt1 = GetVariableByParsing(txt.Substring(0, oo), s);
                return !string.IsNullOrEmpty(txt1.ErrorMessage)
                    ? new DoItFeedback("Befehls-Berechnungsfehler vor ||: " + txt1.ErrorMessage)
                    : txt1.Variable.ValueForReplace == "true"
                        ? txt1
                        : GetVariableByParsing(txt.Substring(oo + 2), s);
            }

            #endregion OrElse

            #endregion

            #region Evtl. Variable an erster Stelle ersetzen

            if (s != null) {
                var t = Method.ReplaceVariable(txt, s);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) { return new DoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage); }
                if (t.Variable != null) { return new DoItFeedback(t.Variable); }
                if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, s); }
            }

            #endregion

            #region Routinen ersetzen, vor den Klammern, das ansonsten Min(x,y,z) falsch anschlägt

            if (s != null) {
                var t = Method.ReplaceComands(txt, s);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) { return new DoItFeedback("Befehls-Berechnungsfehler: " + t.ErrorMessage); }
                if (t.Variable != null) { return new DoItFeedback(t.Variable); }
                if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, s); }
            }

            #endregion

            #region Klammern am Ende berechnen, das ansonsten Min(x,y,z) falsch anschlägt

            var (posa, _) = NextText(txt, 0, KlammerAuf, false, false, KlammernStd);
            if (posa > -1) {
                var (pose, _) = NextText(txt, posa, KlammerZu, false, false, KlammernStd);
                if (pose <= posa) { return DoItFeedback.Klammerfehler(); }

                var tmp = GetVariableByParsing(txt.Substring(posa + 1, pose - posa - 1), s);
                if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp.ErrorMessage); }
                if (tmp.Variable == null) { return new DoItFeedback("Allgemeiner Berechnungsfehler in ()"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback("Falscher Variablentyp: " + tmp.Variable.ShortName); }
                return GetVariableByParsing(txt.Substring(0, posa) + tmp.Variable.ValueForReplace + txt.Substring(pose + 1), s);
            }

            #endregion

            #region Jetzt die Variablen durchprüfen, ob eine ein OK gibt

            if (Script.VarTypes == null) { return new DoItFeedback("Variablentypen nicht initialisiert"); }

            foreach (var thisVT in Script.VarTypes) {
                if (thisVT.GetFromStringPossible) {
                    if (thisVT.TryParse(txt, out var v, s)) {
                        return new DoItFeedback(v);
                    }
                }
            }

            #endregion

            return new DoItFeedback("Wert kann nicht geparsed werden: " + txt);
        }

        public static bool IsValidName(string v) {
            v = v.ToLower();
            var vo = v;
            v = v.ReduceToChars(Constants.AllowedCharsVariableName);
            return v == vo && !string.IsNullOrEmpty(v);
        }

        public int CompareTo(object obj) {
            if (obj is Variable v) {
                return CheckOrder.CompareTo(v.CheckOrder);
            }

            Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
            return 0;
        }

        public abstract DoItFeedback GetValueFrom(Variable attvarAttribute);

        public string ReplaceInText(string txt) {
            if (!txt.ToLower().Contains("~" + Name.ToLower() + "~")) { return txt; }
            return txt.Replace("~" + Name + "~", ReadableText, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        protected abstract bool TryParse(string txt, out Variable? succesVar, Script s);

        #endregion
    }
}