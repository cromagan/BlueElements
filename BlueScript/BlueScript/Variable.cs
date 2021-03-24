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

                case enVariableDataType.Number:
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


            txt = txt.DeKlammere(true, true, false);

            if (s != null) {
                #region Variablen ersetzen
                var t = Method.ReplaceVariable(txt, s.Variablen);
                if (!string.IsNullOrEmpty(t.ErrorMessage)) {
                    return new strDoItFeedback("Variablen-Berechnungsfehler: " + t.ErrorMessage);
                }
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
            (var posa, var _) = Script.NextText(txt, 0, new List<string>() { "(" }, false, false);
            if (posa > -1) {
                (var pose, var _) = Script.NextText(txt, posa, new List<string>() { ")" }, false, false);

                if (pose < posa) { return strDoItFeedback.Klammerfehler(); }

                var tmp = AttributeAuflösen(txt.Substring(posa + 1, pose - posa - 1), s);
                if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return tmp; }

                return AttributeAuflösen(txt.Substring(0, posa) + tmp.Value + txt.Substring(pose + 1), s);

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


            if (!string.IsNullOrEmpty(txt.ErrorMessage)) { SetError(); return; }



            var bl = new List<string>() { "true", "false", "||", "&&", "==", "!=", "<", ">", ">=", "<=" };

            (var pos, var witch) = Script.NextText(txt.Value, 0, bl, false, false);

            if (pos >= 0) {
                if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.Bool) { SetError(); return; }//return new strDoItFeedback("Variable ist kein Boolean");
                var b = Method_if.GetBool(txt.Value);
                if (b == null) { SetError(); return; }//return new strDoItFeedback("Berechnungsfehler der Formel: " + txt); 
                ValueString = b;
                Type = enVariableDataType.Bool;
                return;
            }

            if (txt.Value.StartsWith("\"")) {

                if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.String) {
                    SetError();
                    return;
                    //return new strDoItFeedback("Variable ist kein String");
                }
                ValueString = txt.Value.Trim("\"").Replace("\"+\"", string.Empty); // Erst trimmen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
                Type = enVariableDataType.String;
                return;// new strDoItFeedback();
            }




            if (Type != enVariableDataType.NotDefinedYet && Type != enVariableDataType.Number) { SetError(); return; } //return new strDoItFeedback("Variable ist keine Zahl");

            var erg = modErgebnis.Ergebnis(txt.Value);
            if (erg == null) { SetError(); return; }//return new strDoItFeedback("Berechnungsfehler der Formel: " + txt); 


            ValueDouble = (double)erg;
            Type = enVariableDataType.Number;
            Readonly = true;
        }

        private void SetError() {
            Readonly = false;
            Type = enVariableDataType.Error;
            ValueString = string.Empty;
            Readonly = true;
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

                    case enVariableDataType.Number:
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
        public string ValueString {
            get { return _ValueString; }
            set {
                if (Readonly) { return; }
                _ValueString = value;
            }
        }

        public List<string> ValueListString {
            get { return _ValueString.SplitByCRToList(); }
            set {
                if (Readonly) { return; }
                _ValueString = value.JoinWithCr();
            }
        }


        public enVariableDataType Type { get; set; }
        public bool ValueBool {
            get {
                return _ValueString == "true";
            }
        }

        public double ValueDouble {
            get {
                return DoubleParse(_ValueString);
            }
            set {
                if (Readonly) { return; }
                _ValueString = value.ToString();
            }
        }

        public int ValueInt {
            get {
                return IntParse(_ValueString);
            }
        }



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

            v.Type = enVariableDataType.Number;
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
    }
}






