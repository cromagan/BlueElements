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
                    if (thisvar.Stringable) { l.Add(thisvar.Name); }
                }
            }
            return l;
        }

        public static List<string> AllValues(this List<Variable>? vars) {
            var l = new List<string>();
            if (vars != null) {
                foreach (var thisvar in vars) {
                    if (thisvar.Stringable) { l.Add(thisvar.ValueForReplace); }
                }
            }
            return l;
        }

        public static Variable? Get(this List<Variable>? vars, string name) {
            if (vars == null || vars.Count == 0) { return null; }

            return vars.FirstOrDefault(thisv =>
                !thisv.SystemVariable && string.Equals(thisv.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        ///// <summary>
        ///// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static bool GetBool(this List<Variable> vars, string name) {
        //    var v = vars.Get(name);
        //    return v != null && v.ValueBool;
        //}

        ///// <summary>
        ///// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        //public static double GetDouble(this List<Variable> vars, string name) {
        //    var v = vars.Get(name);
        //    return v == null ? 0d : v.ValueDouble;
        //}

        ///// <summary>
        ///// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        //public static int GetInt(this List<Variable> vars, string name) {
        //    var v = vars.Get(name);
        //    return v == null ? 0 : v.ValueInt;
        //}

        ///// <summary>
        ///// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        //public static List<string> GetList(this List<Variable> vars, string name) {
        //    var v = vars.Get(name);
        //    return v == null ? new List<string>() : v.ValueListString;
        //}

        ///// <summary>
        ///// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static string GetString(this List<Variable> vars, string name) {
        //    var v = vars.Get(name);
        //    return v == null ? string.Empty : v.ValueString;
        //}

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

        #endregion

        //public static void ScriptFinished(this List<Variable> vars) {
        //    if (vars == null || vars.Count == 0) { return; }
        //    foreach (var thisv in vars) {
        //        thisv.ScriptFinished();
        //    }
        //}

        ///// <summary>
        ///// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //public static void Set(this List<Variable> vars, string name, string value) {
        //    var v = vars.Get(name);
        //    if (v == null) {
        //        v = new VariableString(name, string.empty);
        //        vars.Add(v);
        //    }

        //    if (v is not VariableListString vf) {
        //        Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Warnung, "Variablentyp falsch");
        //        return;
        //    }

        //    vf.Readonly = false; // sonst werden keine Daten geschrieben
        //    vf.valuestring = value;
        //    vf.Readonly = true;
        //}

        ///// <summary>
        ///// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //public static void Set(this List<Variable> vars, string name, double value) {
        //    var v = vars.Get(name);
        //    if (v == null) {
        //        v = new VariableFloat(name);
        //        vars.Add(v);
        //    }

        //    if (v is not VariableFloat vf) {
        //        Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Warnung, "Variablentyp falsch");
        //        return;
        //    }

        //    vf.Readonly = false; // sonst werden keine Daten geschrieben
        //    vf.ValueNum = value;
        //    vf.Readonly = true;
        //}

        ///// <summary>
        ///// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //public static void Set(this List<Variable> vars, string name, List<string> value) {
        //    var v = vars.Get(name);
        //    if (v == null) {
        //        v = new VariableListString(name);
        //        vars.Add(v);
        //    }

        //    if (v is not VariableListString vf) {
        //        Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Warnung, "Variablentyp falsch");
        //        return;
        //    }

        //    vf.Readonly = false; // sonst werden keine Daten geschrieben
        //    vf.ValueList = value;
        //    vf.Readonly = true;
        //}

        ///// <summary>
        ///// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
        ///// </summary>
        ///// <param name="vars"></param>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //public static void Set(this List<Variable> vars, string name, bool value) {
        //    var v = vars.Get(name);
        //    if (v == null) {
        //        v = new Variable(name);
        //        vars.Add(v);
        //    }

        //    v.Readonly = false; // sonst werden keine Daten geschrieben
        //    v.Type = VariableDataType.Bool;
        //    v.ValueString = value ? "true" : "false";
        //    v.Readonly = true;
        //}
    }

    public abstract class Variable {

        #region Fields

        public readonly bool SystemVariable;

        private static long _dummyCount;

        #endregion

        #region Constructors

        public Variable(string name, bool ronly, bool system, string coment) {
            Name = system ? "*" + name.ToLower() : name.ToLower();
            Readonly = ronly;
            SystemVariable = system;
            Coment = coment;
        }

        #endregion

        #region Properties

        public string Coment { get; set; }

        /// <summary>
        /// Variablen-Namen werden immer in Kleinbuchstaben gespeichert.
        /// </summary>
        public string Name { get; set; }

        public virtual string ReadableText => "Objekt: " + ShortName;

        public bool Readonly { get; set; }

        public abstract string ShortName { get; }

        public abstract bool Stringable { get; }

        [Obsolete]
        public abstract VariableDataType Type { get; }

        public virtual string ValueForReplace {
            get {
                if (Stringable) { Develop.DebugPrint(enFehlerArt.Warnung, "Variable kann als String zurückgegeben werden."); }
                return ObjectKennung + "\"" + ShortName + ";" + Name + "\"" + ObjectKennung;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Löst den Text auf, so dass eine Stringable Variable zurückgegeben wird.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DoItFeedback GetVariableByParsing(string txt, Script? s) {

            #region Prüfen, Ob noch mehre Klammern da sind, oder Anfangs/End-Klammern entfernen

            if (txt.StartsWith("(")) {
                var (pose, _) = NextText(txt, 0, KlammerZu, false, false, KlammernStd);
                if (pose < txt.Length - 1) {
                    // Wir haben so einen Fall: (true) || (true)
                    var txt1 = GetVariableByParsing(txt.Substring(1, pose - 1), s);
                    return !string.IsNullOrEmpty(txt1.ErrorMessage)
                        ? new DoItFeedback("Befehls-Berechnungsfehler in ():" + txt1.ErrorMessage)
                        : GetVariableByParsing(txt1.Variable.ValueForReplace + txt.Substring(pose + 1), s);
                }
            }

            txt = txt.DeKlammere(true, false, false, true);

            #endregion

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
                if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                    return new DoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
                }

                txt = t.AttributeText;
            }

            #endregion

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
                if (pose <= posa) {
                    return DoItFeedback.Klammerfehler();
                }

                var tmp = GetVariableByParsing(txt.Substring(posa + 1, pose - posa - 1), s);
                if (!tmp.Variable.Stringable) {
                    return new DoItFeedback("Falscher Variablentyp: " + tmp.Variable.ShortName);
                }

                return !string.IsNullOrEmpty(tmp.ErrorMessage)
                    ? tmp
                    : GetVariableByParsing(
                        txt.Substring(0, posa) + tmp.Variable.ValueForReplace + txt.Substring(pose + 1), s);
            }

            #endregion

            #region Jetzt die Variablen durchprüfen, ob eine ein OK gibt

            if (Script.VarTypes == null) { return new DoItFeedback("Variablentypen nicht initialisiert"); }

            foreach (var thisVT in Script.VarTypes) {
                if (thisVT.Stringable) {
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

        //private void SetError(string coment) {
        //    Readonly = false;
        //    Type = VariableDataType.Error;
        //    ValueString = string.Empty;
        //    Readonly = true;
        //    Coment = coment;
        //}
        public abstract DoItFeedback GetValueFrom(Variable attvarAttribute);

        public string ReplaceInText(string txt) {
            if (!txt.ToLower().Contains("~" + Name.ToLower() + "~")) { return txt; }
            return txt.Replace("~" + Name + "~", ReadableText, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        protected static string DummyName() {
            if (_dummyCount >= long.MaxValue) { _dummyCount = 0; }
            _dummyCount++;
            return "dummy" + _dummyCount;
        }

        //public Variable(string name, string attributesText, Script? s) : this(name) {
        //    var txt = AttributeAuflösen(attributesText, s);
        //    if (!string.IsNullOrEmpty(txt.ErrorMessage)) { SetError(txt.ErrorMessage); return; }

        //    #region Testen auf Bool

        //    if (txt.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
        //        txt.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
        //        if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.Bool) { SetError("Variable ist kein Boolean"); return; }
        //        ValueString = txt.Value;
        //        Type = VariableDataType.Bool;
        //        Readonly = true;
        //        return;
        //    }

        //    #endregion

        //    #region Testen auf String

        //    if (txt.Value.Length > 1 && txt.Value.StartsWith("\"") && txt.Value.EndsWith("\"")) {
        //        if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.String) { SetError("Variable ist kein String"); return; }
        //        var tmp = txt.Value.Substring(1, txt.Value.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
        //        //tmp = tmp.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
        //        if (tmp.Contains("\"")) { SetError("Verkettungsfehler"); return; } // Beispiel: s ist nicht definiert und "jj" + s + "kk
        //        ValueString = tmp;
        //        Type = VariableDataType.String;
        //        Readonly = true;
        //        return;
        //    }

        //    #endregion

        //    #region Testen auf Bitmap

        //    if (txt.Value.Length > 4 && txt.Value.StartsWith(ImageKennung + "\"") && txt.Value.EndsWith("\"" + ImageKennung)) {
        //        if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.Bitmap) { SetError("Variable ist kein Bitmap"); return; }
        //        ValueString = txt.Value.Substring(2, txt.Value.Length - 4);
        //        Type = VariableDataType.Bitmap;
        //        Readonly = true;
        //        return;
        //    }

        //    #endregion

        //    #region Testen auf Liste mit Strings

        //    if (txt.Value.Length > 1 && txt.Value.StartsWith("{") && txt.Value.EndsWith("}")) {
        //        if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.List) { SetError("Variable ist keine Liste"); return; }
        //        var t = txt.Value.DeKlammere(false, true, false, true);

        //        if (string.IsNullOrEmpty(t)) {
        //            ValueListString = new List<string>(); // Leere Liste
        //        } else {
        //            var l = Method.SplitAttributeToVars(t, s, new List<VariableDataType> { VariableDataType.String }, true);
        //            if (!string.IsNullOrEmpty(l.ErrorMessage)) { SetError(l.ErrorMessage); return; }
        //            ValueListString = l.Attributes.AllValues();
        //        }
        //        Type = VariableDataType.List;
        //        Readonly = true;
        //        return;
        //    }

        //    #endregion

        //    #region Testen auf Object

        //    if (txt.Value.Length > 2 && txt.Value.StartsWith(ObjectKennung + "\"") && txt.Value.EndsWith("\"" + ObjectKennung)) {
        //        if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.Object) { SetError("Variable ist kein Objekt"); return; }
        //        ValueString = txt.Value.Substring(2, txt.Value.Length - 4);
        //        Type = VariableDataType.Object;
        //        Readonly = true;
        //        return;
        //    }

        //    #endregion

        //    #region Testen auf Number

        //    if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.Numeral) { SetError("Variable ist keine Zahl"); return; }

        //    if (txt.Value.IsDouble()) {
        //        ValueDouble = DoubleParse(txt.Value);
        //        Type = VariableDataType.Numeral;
        //        Readonly = true;
        //        return;
        //    }
        protected abstract bool TryParse(string txt, out Variable? succesVar, Script s);

        #endregion

        //public static string ValueForReplace(string value, VariableDataType type) {
        //    switch (type) {
        //        case VariableDataType.String:
        //            return "\"" + value.RemoveCriticalVariableChars() + "\"";

        //        case VariableDataType.Bool:
        //            return value;

        //        case VariableDataType.Numeral:
        //            return value;

        //        case VariableDataType.List:
        //            if (string.IsNullOrEmpty(value)) { return "{ }"; }

        //            if (!value.EndsWith("\r")) {
        //                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Objekttypfehler: " + value);
        //            }

        //            var x = value.Substring(0, value.Length - 1);
        //            return "{\"" + x.RemoveCriticalVariableChars().SplitByCrToList().JoinWith("\", \"") + "\"}";

        //        case VariableDataType.NotDefinedYet: // Wenn ne Routine die Werte einfach ersetzt.
        //            return value;

        //        case VariableDataType.Bitmap:
        //            return ImageKennung + "\"" + value.RemoveCriticalVariableChars() + "\"" + ImageKennung;

        //        case VariableDataType.Object:
        //            return ObjectKennung + "\"" + value.RemoveCriticalVariableChars() + "\"" + ObjectKennung;

        //        case VariableDataType.Variable:
        //            return value;

        //        default:
        //            Develop.DebugPrint_NichtImplementiert();
        //            return value;
        //    }
        //}

        //public string ObjectData() {
        //    if (Type != VariableDataType.Object) { return string.Empty; }
        //    var x = _valueString.SplitAndCutBy("&");
        //    return x == null || x.GetUpperBound(0) != 1
        //            ? string.Empty
        //            : x[1].FromNonCritical();
        //}

        //public bool ObjectType(string toCheck) {
        //    if (Type != VariableDataType.Object) { return false; }

        //    var x = _valueString.SplitAndCutBy("&");
        //    return x != null && x.GetUpperBound(0) == 1 && x[0] == toCheck.ToUpper().ReduceToChars(Constants.Char_AZ + Constants.Char_Numerals);
        //}

        //public string ReplaceInText(string txt) {
        //    if (!txt.ToLower().Contains("~" + Name.ToLower() + "~")) { return txt; }

        //    return Type is not VariableDataType.String and
        //                not VariableDataType.List and
        //                not VariableDataType.Integer and
        //                not VariableDataType.Bool and
        //                not VariableDataType.Numeral
        //        ? txt
        //        : txt.Replace("~" + Name + "~", ValueString, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //}

        //public override string ToString() {
        //    var zusatz = string.Empty;
        //    if (Readonly) { zusatz = " [Read Only] "; }
        //    return Type switch {
        //        VariableDataType.String => "{str} " + zusatz + Name + " = " + ValueString,
        //        VariableDataType.Numeral => "{num} " + zusatz + Name + " = " + ValueString,
        //        VariableDataType.Bool => "{bol} " + zusatz + Name + " = " + ValueString,
        //        VariableDataType.List => "{lst} " + zusatz + Name + " = " + ValueString,
        //        VariableDataType.Bitmap => "{bmp} " + zusatz + Name + " = [BitmapData]",
        //        VariableDataType.Object => "{obj} " + zusatz + Name + " = [ObjectData]",
        //        VariableDataType.Error => "{err} " + zusatz + Name + " = " + ValueString,
        //        _ => "{ukn} " + zusatz + Name + " = " + ValueString
        //    };
        //}
    }
}