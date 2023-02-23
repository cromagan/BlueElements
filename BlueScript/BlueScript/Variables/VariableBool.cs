// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public class VariableBool : Variable {

    #region Fields

    private bool _valuebool;

    #endregion

    #region Constructors

    public VariableBool(string name, bool value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _valuebool = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableBool(string name) : this(name, false, true, false, string.Empty) { }

    public VariableBool(bool value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bol";
    public static string ShortName_Plain => "bol";
    public static string ShortName_Variable => "*bol";
    public override int CheckOrder => 0;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string MyClassId => ClassId;
    public override string ReadableText => _valuebool.ToString();
    public override bool ToStringPossible => true;

    public bool ValueBool {
        get => _valuebool;
        set {
            if (ReadOnly) { return; }
            _valuebool = value; // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    public override string ValueForReplace => _valuebool ? "true" : "false";

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableBool(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableBool v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueBool = v.ValueBool;
        return DoItFeedback.Null();
    }

    protected override Variable NewWithThisValue(object x, Script s) {
        var v = new VariableBool(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is bool val) {
            _valuebool = val;
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override object? TryParse(string txt, Script? s) {
        if (Method_if.GetBool(txt) is bool b) { return b; }

        #region Auf Restliche Boolsche Operationen testen

        //foreach (var check in Method_if.VergleichsOperatoren) {
        var (i, check) = NextText(txt, 0, Method_if.VergleichsOperatoren, false, false, KlammernStd);
        if (i > -1) {
            if (i < 1 && check != "!") {
                return false;//new DoItFeedback(infos.LogData, s, "Operator (" + check + ") am String-Start nicht erlaubt: " + txt);
            } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) {
                return false;//new DoItFeedback(infos.LogData, s, "Operator (" + check + ") am String-Ende nicht erlaubt: " + txt);
            } // siehe oben

            #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

            #region Ersten Wert als s1 ermitteln

            var s1 = txt.Substring(0, i);
            Variable? v1 = null;
            if (!string.IsNullOrEmpty(s1)) {
                var tmp1 = GetVariableByParsing(s1, s, null);
                if (!tmp1.AllOk) { return false; }//new DoItFeedback(infos.LogData, s, "Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);

                v1 = tmp1.Variable;
            } else {
                if (check != "!") { return false; }//new DoItFeedback(infos.LogData, s, "Wert vor Operator (" + check + ") nicht gefunden: " + txt);
            }

            #endregion

            #region Zweiten Wert als s2 ermitteln

            var s2 = txt.Substring(i + check.Length);
            if (string.IsNullOrEmpty(s2)) { return false; }//new DoItFeedback(infos.LogData, s, "Wert nach Operator (" + check + ") nicht gefunden: " + txt);

            var tmp2 = GetVariableByParsing(s2, s, null);
            if (!tmp2.AllOk) {
                return false;//new DoItFeedback(infos.LogData, s, "Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);
            }

            var v2 = tmp2.Variable;

            #endregion

            // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 != null) {
                if (v1.MyClassId != v2.MyClassId) { return false; }// return new DoItFeedback(infos.LogData, s, "Typen unterschiedlich: " + txt);

                if (!v1.ToStringPossible) { return false; }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
            } else {
                if (v2 is not VariableBool) { return false; }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
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
                        if (v1 is not VariableFloat v1Fl) { return false; } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableFloat v2Fl) { return false; }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Fl.ValueNum >= v2Fl.ValueNum) { replacer = "true"; }
                        break;
                    }

                case "<=": {
                        if (v1 is not VariableFloat v1Fl) { return false; } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableFloat v2Fl) { return false; }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Fl.ValueNum <= v2Fl.ValueNum) { replacer = "true"; }
                        break;
                    }

                case "<": {
                        if (v1 is not VariableFloat v1Fl) { return false; } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableFloat v2Fl) { return false; }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Fl.ValueNum < v2Fl.ValueNum) { replacer = "true"; }
                        break;
                    }

                case ">": {
                        if (v1 is not VariableFloat v1Fl) { return false; } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableFloat v2Fl) { return false; }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Fl.ValueNum > v2Fl.ValueNum) { replacer = "true"; }
                        break;
                    }

                case "||": {
                        if (v1 is not VariableBool v1Bo) { return false; }                            // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableBool v2Bo) { return false; }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Bo.ValueBool || v2Bo.ValueBool) { replacer = "true"; }
                        break;
                    }

                case "&&": {
                        if (v1 is not VariableBool v1Bo) { return false; }  // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableBool v2Bo) { return false; }                                // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v1Bo.ValueBool && v2Bo.ValueBool) { replacer = "true"; }
                        break;
                    }

                case "!": {
                        // S1 dürfte eigentlich nie was sein: !False||!false
                        // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                        if (v2 is not VariableBool v2Bo) { return false; }   // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (!v2Bo.ValueBool) { replacer = "true"; }
                        break;
                    }

                default:
                    return false; //  return new DoItFeedback(infos.LogData, s, "Operator (" + check + ") unbekannt: " + txt);
            }

            if (!string.IsNullOrEmpty(replacer)) {
                return Method_if.GetBool(replacer);
            }
        }

        #endregion

        return null;
    }

    #endregion
}