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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueScript {

    public static class VariableExtensions {

        #region Methods

        public static void AddComent(this List<Variable> vars, string additionalComent) {
            foreach (var thisvar in vars) {
                if (!string.IsNullOrEmpty(thisvar.Coment)) { thisvar.Coment += "\r"; }
                thisvar.Coment += additionalComent;
            }
        }

        public static List<string> AllNames(this List<Variable> vars) {
            List<string> l = new();
            foreach (var thisvar in vars) {
                l.Add(thisvar.Name);
            }
            return l;
        }

        public static List<string> AllValues(this List<Variable> vars) {
            List<string> l = new();
            foreach (var thisvar in vars) {
                l.Add(thisvar.ValueString);
            }
            return l;
        }

        public static Variable Get(this List<Variable> vars, string name) {
            if (vars == null || vars.Count == 0) { return null; }
            foreach (var thisv in vars) {
                if (!thisv.SystemVariable && thisv.Name.ToLower() == name.ToLower()) {
                    return thisv;
                }
            }
            return null;
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
            return v == null ? 0d : (double)v.ValueDouble;
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

        public static Variable GetSystem(this List<Variable> vars, string name) {
            foreach (var thisv in vars) {
                if (thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper()) {
                    return thisv;
                }
            }
            return null;
        }

        public static void PrepareForScript(this List<Variable> vars) {
            if (vars == null || vars.Count == 0) { return; }
            foreach (var thisv in vars) {
                thisv.PrepareForScript();
            }
        }

        public static void RemoveWithComent(this List<Variable> vars, string coment) {
            var z = 0;
            do {
                if (vars[z].Coment != null && vars[z].Coment.Contains(coment)) {
                    vars.RemoveAt(z);
                } else {
                    z++;
                }
            } while (z < vars.Count);
        }

        public static void ScriptFinished(this List<Variable> vars) {
            if (vars == null || vars.Count == 0) { return; }
            foreach (var thisv in vars) {
                thisv.ScriptFinished();
            }
        }

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
        public static void Set(this List<Variable> vars, string name, List<string> value) {
            var v = vars.Get(name);
            if (v == null) {
                v = new Variable(name);
                vars.Add(v);
            }
            v.Readonly = false; // sonst werden keine Daten geschrieben
            v.Type = enVariableDataType.List;
            v.ValueListString = value;
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

        private string _ValueString = string.Empty;

        #endregion

        #region Constructors

        public Variable(string name) {
            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();
        }

        public Variable(string name, string attributesText, Script s) : this(name) {
            var txt = AttributeAuflösen(attributesText, s);
            if (!string.IsNullOrEmpty(txt.ErrorMessage)) { SetError(txt.ErrorMessage); return; }

            #region Testen auf Bool

            if (txt.Value.Equals("true", System.StringComparison.InvariantCultureIgnoreCase) ||
                txt.Value.Equals("false", System.StringComparison.InvariantCultureIgnoreCase)) {
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
                ValueString = txt.Value.Substring(1, txt.Value.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
                ValueString = ValueString.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
                if (ValueString.Contains("\"")) { SetError("Verkettungsfehler"); return; } // Beispiel: s ist nicht definiert und "jj" + s + "kk
                Type = enVariableDataType.String;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Bitmap

            if (txt.Value.Length > 4 && txt.Value.StartsWith(Constants.ImageKennung + "\"") && txt.Value.EndsWith("\"" + Constants.ImageKennung)) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Bitmap) { SetError("Variable ist kein Bitmap"); return; }
                ValueString = txt.Value.Substring(2, txt.Value.Length - 4).Replace(Constants.GänsefüßchenReplace, "\"");
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
                    var l = Method.SplitAttributeToVars(t, s, new List<enVariableDataType>() { enVariableDataType.String }, true);
                    if (!string.IsNullOrEmpty(l.ErrorMessage)) { SetError(l.ErrorMessage); return; }
                    ValueListString = l.Attributes.AllValues();
                }
                Type = enVariableDataType.List;
                Readonly = true;
                return;// new strDoItFeedback();
            }

            #endregion

            #region Testen auf Object

            if (txt.Value.Length > 2 && txt.Value.StartsWith(Constants.ObjectKennung + "\"") && txt.Value.EndsWith("\"" + Constants.ObjectKennung)) {
                if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Object) { SetError("Variable ist kein Object"); return; }
                ValueString = txt.Value.Substring(2, txt.Value.Length - 4).Replace(Constants.GänsefüßchenReplace, "\"");
                Type = enVariableDataType.Object;
                Readonly = true;
                return;
            }

            #endregion

            #region Testen auf Number

            if (Type is not enVariableDataType.NotDefinedYet and not enVariableDataType.Numeral) { SetError("Variable ist keine Zahl"); return; }
            var erg = modErgebnis.Ergebnis(txt.Value);
            if (erg == null) { SetError("Berechnungsfehler der Formel: " + txt.ErrorMessage); return; }//return new strDoItFeedback();
            ValueDouble = (double)erg;
            Type = enVariableDataType.Numeral;
            Readonly = true;

            #endregion
        }

        public Variable(string name, string value, enVariableDataType type, bool ronly, bool system, string coment) : this(name) {
            Name = system ? "*" + name.ToLower() : name.ToLower();
            ValueString = value;
            Type = type;
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

        public bool ValueBool => _ValueString == "true";

        public double ValueDouble {
            get => DoubleParse(_ValueString);
            set {
                if (Readonly) { return; }
                ValueString = value.ToString();
            }
        }

        public int ValueInt => IntParse(_ValueString);

        /// <summary>
        /// Es wird der String in eine Liste umgewandelt (bzw. andersrum). Leere Einträge auch am Ende bleiben erhalten.
        /// </summary>
        public List<string> ValueListString {
            get {
                List<string> w = new();
                if (string.IsNullOrEmpty(_ValueString)) { return w; }
                var x = _ValueString.Substring(0, _ValueString.Length-1).Replace("\r\n", "\r").Replace("\n", "\r");
                var nl = x.Split(new[] { "\r" }, System.StringSplitOptions.None);
                w.AddRange(nl);
                return w;
            }
            set {
                if (Readonly) { return; }
                if (value == null || value.Count == 0) {
                    ValueString = string.Empty;
                } else {
                    ValueString = value.JoinWithCr() + "\r";
                }
            }
        }

        /// <summary>
        /// Der direkte Text, der in der Variabel gespeichert ist.
        /// Ohne Anführungsstrichchen. Falls es in Wahrheit eine Liste ist, der Text gejoinded mit \r
        /// </summary>
        public string ValueString {
            get => _ValueString;
            set {
                if (Readonly) { return; }
                _ValueString = value;
            }
        }

        #endregion

        #region Methods

        public static strDoItFeedback AttributeAuflösen(string txt, Script s) {
            // Die Trims werden benötigtn, wenn eine Liste kommt, dass die Leerzeichen vor und nach den Kommas weggeschnitten werden.

            #region Prüfen, Ob noch mehre Klammern da sind, oder Anfangs/End-Klammern entfernen

            if (txt.StartsWith("(")) {
                (var pose, var _) = NextText(txt, 0, KlammerZu, false, false);
                if (pose < txt.Length - 1) {
                    /// Wir haben so einen Fall: (true) || (true)
                    var txt1 = AttributeAuflösen(txt.Substring(1, pose - 1), s);
                    return !string.IsNullOrEmpty(txt1.ErrorMessage)
                        ? new strDoItFeedback("Befehls-Berechnungsfehler in ():" + txt1.ErrorMessage)
                        : AttributeAuflösen(txt1.Value + txt.Substring(pose + 1), s);
                }
            }

            txt = txt.DeKlammere(true, false, false, true);

            #endregion

            if (s != null) {

                #region Variablen ersetzen

                var t = Method.ReplaceVariable(txt, s.Variablen);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                    return new strDoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
                }

                #endregion Variablen ersetzen

                #region auf Boolsche Operatoren prüfen

                #region AndAlso

                (var uu, var _) = NextText(txt, 0, Method_if.UndUnd, false, false);
                if (uu > 0) {
                    var txt1 = AttributeAuflösen(txt.Substring(0, uu), s);
                    return !string.IsNullOrEmpty(txt1.ErrorMessage) ? new strDoItFeedback("Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage)
                        : txt1.Value == "false" ? txt1
                        : AttributeAuflösen(txt.Substring(uu + 2), s);
                }

                #endregion AndAlso

                #region OrElse

                (var oo, var _) = NextText(txt, 0, Method_if.OderOder, false, false);
                if (oo > 0) {
                    var txt1 = AttributeAuflösen(txt.Substring(0, oo), s);
                    return !string.IsNullOrEmpty(txt1.ErrorMessage)
                        ? new strDoItFeedback("Befehls-Berechnungsfehler vor ||: " + txt1.ErrorMessage)
                        : txt1.Value == "true" ? txt1 : AttributeAuflösen(txt.Substring(oo + 2), s);
                }

                #endregion OrElse

                #endregion auf Boolsche Operatoren prüfen

                #region Routinen ersetzen, vor den Klammern, das ansonsten Min(x,y,z) falsch anschlägt

                var t2 = Method.ReplaceComands(t.AttributeText, Script.Comands, s);
                if (!string.IsNullOrEmpty(t2.ErrorMessage)) {
                    return new strDoItFeedback("Befehls-Berechnungsfehler: " + t2.ErrorMessage);
                }

                #endregion

                txt = t2.AttributeText;
            }

            #region Klammern am Ende berechnen, das ansonsten Min(x,y,z) falsch anschlägt

            (var posa, var _) = NextText(txt, 0, KlammerAuf, false, false);
            if (posa > -1) {
                (var pose, var _) = NextText(txt, posa, KlammerZu, false, false);
                if (pose <= posa) { return strDoItFeedback.Klammerfehler(); }
                var tmp = AttributeAuflösen(txt.Substring(posa + 1, pose - posa - 1), s);
                return !string.IsNullOrEmpty(tmp.ErrorMessage)
                    ? tmp
                    : AttributeAuflösen(txt.Substring(0, posa) + tmp.Value + txt.Substring(pose + 1), s);
            }

            #endregion

            #region Vergleichsoperatoren ersetzen und vereinfachen

            (var pos, var _) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false);
            if (pos >= 0) {
                txt = Method_if.GetBool(txt);
                if (txt == null) { return new strDoItFeedback("Der Inhalt zwischen den Klammern () konnte nicht berechnet werden."); }
            }

            #endregion

            return new strDoItFeedback(txt, enVariableDataType.NotDefinedYet);
        }

        public static bool IsValidName(string v) {
            v = v.ToLower();
            var vo = v;
            v = v.ReduceToChars(Constants.AllowedCharsVariableName);
            return v == vo && !string.IsNullOrEmpty(v);
        }

        public static string ValueForReplace(string value, enVariableDataType type) {
            switch (type) {
                case enVariableDataType.String:
                    return "\"" + value.Replace("\"", Constants.GänsefüßchenReplace) + "\"";

                case enVariableDataType.Bool:
                    return value;

                case enVariableDataType.Numeral:
                    return value;

                case enVariableDataType.List:
                    return "{\"" + value.Replace("\"", Constants.GänsefüßchenReplace).SplitByCRToList().JoinWith("\", \"").TrimEnd(", \"") + "\"}";

                case enVariableDataType.NotDefinedYet: // Wenn ne Routine die Werte einfach ersetzt.
                    return value;

                case enVariableDataType.Bitmap:
                    return Constants.ImageKennung + "\"" + value.Replace("\"", Constants.GänsefüßchenReplace) + "\"" + Constants.ImageKennung;

                case enVariableDataType.Object:
                    return Constants.ObjectKennung + "\"" + value.Replace("\"", Constants.GänsefüßchenReplace) + "\"" + Constants.ObjectKennung;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    return value;
            }
        }

        public Bitmap GetValueBitmap(Script s) => s.BitmapCache[ValueInt];

        public string ObjectData() {
            if (Type != enVariableDataType.Object) { return string.Empty; }
            var x = _ValueString.SplitBy("&");
            return x == null || x.GetUpperBound(0) != 1
                    ? string.Empty
                    : x[1].FromNonCritical();
        }

        public bool ObjectType(string toCheck) {
            if (Type != enVariableDataType.Object) { return false; }

            var x = _ValueString.SplitBy("&");
            return x != null && x.GetUpperBound(0) == 1 && x[0] == toCheck.ToUpper().ReduceToChars(Constants.Char_AZ + Constants.Char_Numerals);
        }

        public void PrepareForScript() => _ValueString = _ValueString.Replace("\"", Constants.GänsefüßchenReplace);

        public void ScriptFinished() => _ValueString = _ValueString.Replace(Constants.GänsefüßchenReplace, "\"");

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
                _ => "{ukn} " + zusatz + Name + " = " + ValueString,
            };
        }

        internal static string GenerateObject(string objecttype, string value) => objecttype.ToUpper().ReduceToChars(Constants.Char_AZ + Constants.Char_Numerals) + "&" + value.ToNonCritical();

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