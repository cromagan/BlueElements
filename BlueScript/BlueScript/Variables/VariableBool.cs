﻿// Authors:
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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Methods;
using static BlueBasics.Extensions;

namespace BlueScript.Variables {

    public class VariableBool : Variable {

        #region Fields

        private bool _valuebool;

        #endregion

        #region Constructors

        public VariableBool(string name, bool value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) => _valuebool = value;

        public VariableBool(string name) : this(name, false, true, false, string.Empty) { }

        public VariableBool(bool value) : this(Variable.DummyName(), value, true, false, string.Empty) { }

        #endregion

        #region Properties

        public override int CheckOrder => 0;
        public override string ShortName => "bol";
        public override bool Stringable => true;
        public override VariableDataType Type => VariableDataType.String;

        public bool ValueBool {
            get => _valuebool;
            set {
                if (Readonly) { return; }
                _valuebool = value; // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
            }
        }

        public override string ValueForReplace => _valuebool ? "true" : "false";

        #endregion

        #region Methods

        public override DoItFeedback GetValueFrom(Variable variable) {
            if (variable is not VariableBool v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
            if (Readonly) { return DoItFeedback.Schreibgschützt(); }
            ValueBool = v.ValueBool;
            return DoItFeedback.Null();
        }

        protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
            succesVar = null;

            #region Auf Restliche Boolsche Operationen testen

            //foreach (var check in Method_if.VergleichsOperatoren) {
            var (i, check) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false, KlammernStd);
            if (i > -1) {
                if (i < 1 && check != "!") {
                    return false;//new DoItFeedback("Operator (" + check + ") am String-Start nicht erlaubt: " + txt);
                } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

                if (i >= txt.Length - 1) {
                    return false;//new DoItFeedback("Operator (" + check + ") am String-Ende nicht erlaubt: " + txt);
                } // siehe oben

                #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

                #region Ersten Wert als s1 ermitteln

                var s1 = txt.Substring(0, i);
                Variable v1 = null;
                if (!string.IsNullOrEmpty(s1)) {
                    if (check != "!") { return false; }//new DoItFeedback("Wert vor Operator (" + check + ") nicht gefunden: " + txt);
                    var tmp1 = GetVariableByParsing(s1, s);
                    if (!string.IsNullOrEmpty(tmp1.ErrorMessage)) { return false; }//new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);

                    v1 = tmp1.Variable;
                }

                #endregion

                #region Zweiten Wert als s2 ermitteln

                var s2 = txt.Substring(i + check.Length);
                Variable v2 = null;
                if (string.IsNullOrEmpty(s2)) { return false; }//new DoItFeedback("Wert nach Operator (" + check + ") nicht gefunden: " + txt);

                var tmp2 = GetVariableByParsing(s2, s);
                if (!string.IsNullOrEmpty(tmp2.ErrorMessage)) {
                    return false;//new DoItFeedback("Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);
                }

                v2 = tmp2.Variable;

                #endregion

                // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
                if (v1 != null) {
                    if (v1.ShortName != v2.ShortName) { return false; }// return new DoItFeedback("Typen unterschiedlich: " + txt);

                    if (v1.Stringable) { return false; }// return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                } else {
                    if (v2 is not VariableBool) { return false; }// return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                }

                #endregion

                var replacer = "false";
                switch (check) {
                    case "==": {
                            if (v1.ValueForReplace == v2.ValueForReplace) { replacer = "true"; }
                            break;
                        }

                    case "!=": {
                            if (v1.ValueForReplace != v2.ValueForReplace) { replacer = "true"; }
                            break;
                        }

                    case ">=": {
                            if (v1 is not VariableFloat v1fl) { return false; } //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableFloat v2fl) { return false; }  //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1fl.ValueNum >= v2fl.ValueNum) { replacer = "true"; }
                            break;
                        }

                    case "<=": {
                            if (v1 is not VariableFloat v1fl) { return false; } //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableFloat v2fl) { return false; }  //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1fl.ValueNum <= v2fl.ValueNum) { replacer = "true"; }
                            break;
                        }

                    case "<": {
                            if (v1 is not VariableFloat v1fl) { return false; } //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableFloat v2fl) { return false; }  //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1fl.ValueNum < v2fl.ValueNum) { replacer = "true"; }
                            break;
                        }

                    case ">": {
                            if (v1 is not VariableFloat v1fl) { return false; } //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableFloat v2fl) { return false; }  //  return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1fl.ValueNum > v2fl.ValueNum) { replacer = "true"; }
                            break;
                        }

                    case "||": {
                            if (v1 is not VariableBool v1bo) { return false; }                            // return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableBool v2bo) { return false; }// return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1bo.ValueBool || v2bo.ValueBool) { replacer = "true"; }
                            break;
                        }

                    case "&&": {
                            if (v1 is not VariableBool v1bo) { return false; }  // return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1 is not VariableBool v2bo) { return false; }                                // return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (v1bo.ValueBool && v2bo.ValueBool) { replacer = "true"; }
                            break;
                        }

                    case "!": {
                            // S1 dürfte eigentlich nie was sein: !False||!false
                            // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                            if (v2 is not VariableBool v2bo) { return false; }   // return new DoItFeedback("Datentyp nicht zum Vergleichen geeignet: " + txt);
                            if (!v2bo.ValueBool) { replacer = "true"; }
                            break;
                        }

                    default:
                        return false; //  return new DoItFeedback("Operator (" + check + ") unbekannt: " + txt);
                }

                if (!string.IsNullOrEmpty(replacer)) {
                    succesVar = new VariableBool((bool)Method_if.GetBool((string)replacer));
                    return true;
                }
            }

            #endregion

            return false;
            //var x = Method_if.GetBool(txt);
            //if (x != null) {
            //    succesVar = new VariableBool((bool)x);
            //    return true;
            //}

            //succesVar = null;
            //return false;
        }

        #endregion
    }
}