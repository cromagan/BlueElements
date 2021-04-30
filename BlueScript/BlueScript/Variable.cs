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
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;
using static BlueBasics.modConverter;

namespace BlueScript {
    public class Variable {


        public override string ToString() {


            var zusatz = string.Empty;
            if (Readonly) { zusatz = " [Read Only] "; }


            switch (Type) {

                case enVariableDataType.String:
                    return "{str} " + zusatz + Name + " = " + ValueForReplace;

                case enVariableDataType.Numeral:
                    return "{num} " + zusatz + Name + " = " + ValueForReplace;

                case enVariableDataType.Date:
                    return "{dat} " + zusatz + Name + " = " + ValueForReplace;

                case enVariableDataType.Bool:
                    return "{bol} " + zusatz + Name + " = " + ValueForReplace;

                case enVariableDataType.List:
                    return "{lst} " + zusatz + Name + " = " + ValueForReplace;

                case enVariableDataType.Error:
                    return "{err} " + zusatz + Name + " = " + ValueString;

                default:
                    return "{ukn} " + zusatz + Name + " = " + ValueString;

            }

        }

        public Variable(string name) {

            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();
        }


        public static strDoItFeedback AttributeAuflösen(string txt, Script s) {

            // Die Trims werden benötigtn, wenn eine Liste kommt, dass die Leerzeichen vor und nach den Kommas weggeschnitten werden.
            txt = txt.DeKlammere(true, false, false, true);

            if (s != null) {
                #region Variablen ersetzen
                var t = Method.ReplaceVariable(txt, s.Variablen);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                    return new strDoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
                }
                #endregion

                #region auf Boolsche Operatoren prüfen

                #region AndAlso
                (var uu, var _) = NextText(txt, 0, Method_if.UndUnd, false, false);
                if (uu > 0) {
                    var txt1 = AttributeAuflösen(txt.Substring(0, uu), s);
                    if (!string.IsNullOrEmpty(txt1.ErrorMessage)) {
                        return new strDoItFeedback("Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage);
                    }
                    if (txt1.Value == "false") { return txt1; }

                    return AttributeAuflösen(txt.Substring(uu + 2), s);
                }
                #endregion

                #region OrElse
                (var oo, var _) = NextText(txt, 0, Method_if.OderOder, false, false);
                if (oo > 0) {
                    var txt1 = AttributeAuflösen(txt.Substring(0, oo), s);
                    if (!string.IsNullOrEmpty(txt1.ErrorMessage)) {
                        return new strDoItFeedback("Befehls-Berechnungsfehler vor ||: " + txt1.ErrorMessage);
                    }
                    if (txt1.Value == "true") { return txt1; }

                    return AttributeAuflösen(txt.Substring(oo + 2), s);
                }
                #endregion

                #endregion

                #region Routinen ersetzen, vor den Klammern, das ansonsten Min(x,y,z) falsch anschlägt
                var t2 = Method.ReplaceComands(t.AttributeText, Script.Comands, s);
                if (!string.IsNullOrEmpty(t2.ErrorMessage)) {
                    return new strDoItFeedback("Befehls-Berechnungsfehler: " + t2.ErrorMessage);
                }
                #endregion

                txt = t2.AttributeText;
            }


            #region Klammern am ende berechnen, das ansonsten Min(x,y,z) falsch anschlägt
            (var posa, var _) = NextText(txt, 0, KlammerAuf, false, false);
            if (posa > -1) {
                (var pose, var _) = NextText(txt, posa, KlammerZu, false, false);

                if (pose <= posa) { return strDoItFeedback.Klammerfehler(); }

                var tmp = AttributeAuflösen(txt.Substring(posa + 1, pose - posa - 1), s);
                if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return tmp; }

                return AttributeAuflösen(txt.Substring(0, posa) + tmp.Value + txt.Substring(pose + 1), s);

            }
            #endregion

            #region Vergleichsoperatoren ersetzen und vereinfachen


            (var pos, var _) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false);

            if (pos >= 0) {
                txt = Method_if.GetBool(txt);
                if (txt == null) { return new strDoItFeedback("Der Inhalt zwischen den Klammern () konnte nicht berechnet werden."); }
            }
            #endregion

            return new strDoItFeedback(txt, string.Empty);
        }

        public Variable(string name, string attributesText, Script s) {
            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();

            var txt = AttributeAuflösen(attributesText, s);


            if (!string.IsNullOrEmpty(txt.ErrorMessage)) { SetError(txt.ErrorMessage); return; }

            #region Testen auf bool
            if (txt.Value.Equals("true", System.StringComparison.InvariantCultureIgnoreCase) ||
                txt.Value.Equals("false", System.StringComparison.InvariantCultureIgnoreCase)) {
                if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.Bool) { SetError("Variable ist kein Boolean"); return; }
                ValueString = txt.Value;
                Type = enVariableDataType.Bool;
                Readonly = true;
                return;
            }
            #endregion

            #region Testen auf String
            if (txt.Value.StartsWith("\"") && txt.Value.EndsWith("\"")) {
                if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.String) { SetError("Variable ist kein String"); return; }
                ValueString = txt.Value.Trim("\"").Replace("\"+\"", string.Empty); // Erst trimmen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
                Type = enVariableDataType.String;
                Readonly = true;
                return;// new strDoItFeedback();
            }
            #endregion

            #region Testen auf Liste mit Strings
            if (txt.Value.StartsWith("{\"") && txt.Value.EndsWith("\"}")) {
                if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.List) { SetError("Variable ist keine Liste"); return; }
                var t = txt.Value.DeKlammere(false, true, false, true);
                var l = Method.SplitAttributeToVars(t, s, new List<enVariableDataType>() { enVariableDataType.String }, true);
                if (!string.IsNullOrEmpty(l.ErrorMessage)) { SetError(l.ErrorMessage); return; }
                ValueListString = l.Attributes.AllValues();
                Type = enVariableDataType.List;
                Readonly = true;
                return;// new strDoItFeedback();
            }

            #endregion

            #region Testen auf Number
            if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.Numeral) { SetError("Variable ist keine Zahl"); return; }

            var erg = modErgebnis.Ergebnis(txt.Value);
            if (erg == null) { SetError("Berechnungsfehler der Formel: " + txt.ErrorMessage); return; }//return new strDoItFeedback(); 

            ValueDouble = (double)erg;
            Type = enVariableDataType.Numeral;
            Readonly = true;
            #endregion
        }

        private void SetError(string coment) {
            Readonly = false;
            Type = enVariableDataType.Error;
            ValueString = string.Empty;
            Readonly = true;
            Coment = coment;
        }


        public Variable(string name, string value, enVariableDataType type, bool ronly, bool system, string coment) {

            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }

            if (system) {
                Name = "*" + name.ToLower();
            } else {

                Name = name.ToLower();
            }

            ValueString = value;
            Type = type;
            Readonly = ronly;
            SystemVariable = system;
            Coment = coment;
        }

        public Variable(string name, string value, enVariableDataType type) {

            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();
            ValueString = value;
            Type = type;
        }

        public string ValueForReplace {
            get {

                switch (Type) {

                    case enVariableDataType.String:
                        return "\"" + ValueString + "\"";

                    case enVariableDataType.Numeral:
                        return ValueString;

                    case enVariableDataType.Bool:
                        return ValueString;

                    case enVariableDataType.List:
                        return "{\"" + ValueString.SplitByCRToList().JoinWith("\", \"").TrimEnd(", \"") + "\"}";

                    default:
                        Develop.DebugPrint_NichtImplementiert();
                        return ValueString;
                }
            }
        }

        public bool SystemVariable { get; set; }
        public bool Readonly { get; set; }
        public string Name { get; set; }
        public string Coment { get; set; }

        private string _ValueString = string.Empty;

        /// <summary>
        /// Der direkte Text, der in der Variabel gespeichert ist.
        /// Ohne Anführungsstrichchen. Falls es in Wahrheit eine Liste ist, der Text gejoinded mit \r
        /// </summary>
        public string ValueString {
            get => _ValueString;
            set {
                if (Readonly) { return; }
                _ValueString = value.Replace("\"", "''");
            }
        }

        public List<string> ValueListString {
            get => _ValueString.SplitByCRToList();
            set {
                if (Readonly) { return; }
                ValueString = value.JoinWithCr();
            }
        }

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


        public static bool IsValidName(string v) {

            v = v.ToLower();

            var vo = v;
            v = v.ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);


            if (v != vo) { return false; }

            if (string.IsNullOrEmpty(v)) { return false; }

            return true;


        }
    }


    public static class VariableExtensions {



        public static Variable Get(this List<Variable> vars, string name) {
            if (vars == null || vars.Count == 0) { return null; }

            foreach (var thisv in vars) {
                if (!thisv.SystemVariable && thisv.Name.ToUpper() == name.ToUpper()) {
                    return thisv;
                }
            }
            return null;
        }


        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static List<string> GetList(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return new List<string>(); }
            return v.ValueListString;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static double GetDouble(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return 0f; }
            return v.ValueDouble;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static int GetInt(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return 0; }
            return v.ValueInt;
        }


        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten 0
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        public static decimal GetDecimal(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return 0m; }
            return (decimal)v.ValueDouble;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return string.Empty; }
            return v.ValueString;
        }

        /// <summary>
        /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(this List<Variable> vars, string name) {
            var v = vars.Get(name);
            if (v == null) { return false; }
            return v.ValueBool;
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

            if (value) {
                v.ValueString = "true";
            } else {
                v.ValueString = "false";
            }
            v.Readonly = true;
        }

        public static Variable GetSystem(this List<Variable> vars, string name) {

            foreach (var thisv in vars) {
                if (thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper()) {
                    return thisv;
                }

            }
            return null;
        }

        public static List<string> AllNames(this List<Variable> vars) {

            var l = new List<string>();
            foreach (var thisvar in vars) {
                l.Add(thisvar.Name);
            }

            return l;
        }

        public static List<string> AllValues(this List<Variable> vars) {

            var l = new List<string>();
            foreach (var thisvar in vars) {
                l.Add(thisvar.ValueString);
            }

            return l;
        }
    }
}