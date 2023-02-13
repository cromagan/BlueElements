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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript.Methods;
using BlueScript.Structures;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public static class VariableExtensions {

    #region Methods

    public static void AddComment(this ICollection<Variable> vars, string additionalComment) {
        foreach (var thisvar in vars) {
            if (!string.IsNullOrEmpty(thisvar.Comment)) {
                thisvar.Comment += "\r";
            }

            thisvar.Comment += additionalComment;
        }
    }

    public static List<string> AllNames(this ICollection<Variable>? vars) => vars != null ? vars.Select(thisvar => thisvar.Name).ToList() : new List<string>();

    public static List<string> AllStringableNames(this ICollection<Variable>? vars) {
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
    public static List<string> AllStringValues(this ICollection<Variable>? vars) {
        var l = new List<string>();
        if (vars != null) {
            foreach (var thisvar in vars) {
                if (thisvar is VariableString vs) { l.Add(vs.ValueString); }
            }
        }
        return l;
    }

    public static Variable? Get(this ICollection<Variable>? vars, string name) {
        if (vars == null || vars.Count == 0) { return null; }

        return vars.FirstOrDefault(thisv =>
            !thisv.SystemVariable && string.Equals(thisv.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetBool(this ICollection<Variable> vars, string name) {
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
    public static double GetDouble(this ICollection<Variable> vars, string name) {
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
    public static int GetInt(this ICollection<Variable> vars, string name) {
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
    public static List<string> GetList(this ICollection<Variable> vars, string name) {
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
    public static string GetString(this ICollection<Variable> vars, string name) {
        var v = vars.Get(name);

        if (v is not VariableString vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return string.Empty;
        }

        return vf.ValueString;
    }

    public static Variable? GetSystem(this ICollection<Variable> vars, string name) => vars.FirstOrDefault(thisv =>
        thisv.SystemVariable && thisv.Name.ToUpper() == "*" + name.ToUpper());

    public static void RemoveWithComment(this List<Variable> vars, string comment) {
        var z = 0;
        do {
            if (vars[z].Comment.Contains(comment)) {
                vars.RemoveAt(z);
            } else {
                z++;
            }
        } while (z < vars.Count);
    }

    public static string ReplaceInText(this ICollection<Variable> vars, string originalText) {
        foreach (var thisvar in vars) {
            originalText = thisvar.ReplaceInText(originalText);
        }
        return originalText;
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
    public static void Set(this ICollection<Variable> vars, string name, string value) {
        var v = vars.Get(name);
        if (v == null) {
            v = new VariableString(name, string.Empty, false, false, string.Empty);
            vars.Add(v);
        }

        if (v is not VariableString vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueString = value;
        vf.ReadOnly = true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Set(this ICollection<Variable> vars, string name, double value) {
        var v = vars.Get(name);
        if (v == null) {
            v = new VariableFloat(name);
            vars.Add(v);
        }

        if (v is not VariableFloat vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueNum = value;
        vf.ReadOnly = true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Set(this ICollection<Variable> vars, string name, List<string> value) {
        var v = vars.Get(name);
        if (v == null) {
            v = new VariableListString(name);
            vars.Add(v);
        }

        if (v is not VariableListString vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueList = value;
        vf.ReadOnly = true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Set(this ICollection<Variable> vars, string name, bool value) {
        var v = vars.Get(name);
        if (v == null) {
            v = new VariableBool(name);
            vars.Add(v);
        }

        if (v is not VariableBool vf) {
            Develop.DebugPrint(FehlerArt.Warnung, "Variablentyp falsch");
            return;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueBool = value;
        vf.ReadOnly = true;
    }

    #endregion
}

public abstract class Variable : ParsebleItem, IComparable, IParseable, ICloneable, IHasKeyName {

    #region Fields

    private static long _dummyCount;
    private string _comment = string.Empty;
    private bool _readOnly;
    private bool _systemVariable;

    #endregion

    #region Constructors

    protected Variable(string name, bool ronly, bool system, string comment) : base(system ? "*" + name.ToLower() : name.ToLower()) {
        ReadOnly = ronly;
        SystemVariable = system;
        Comment = comment;
    }

    #endregion

    #region Properties

    public static string Any_Plain => "any";

    public static string Any_Variable => "*any";

    public abstract int CheckOrder { get; }

    //public abstract string ClassId { get; }

    public string Comment {
        get => _comment;
        set {
            if (_comment == value) { return; }
            _comment = value;
            OnChanged();
        }
    }

    public abstract bool GetFromStringPossible { get; }

    public abstract bool IsNullOrEmpty { get; }

    public abstract string MyClassId { get; }
    public string Name => KeyName;

    public virtual string ReadableText => "Objekt: " + MyClassId;

    public bool ReadOnly {
        get => _readOnly;
        set {
            if (_readOnly == value) { return; }
            _readOnly = value;
            OnChanged();
        }
    }

    public bool SystemVariable {
        get => _systemVariable;
        private set {
            if (_systemVariable == value) { return; }
            _systemVariable = value;
            OnChanged();
        }
    }

    public abstract bool ToStringPossible { get; }

    public virtual string ValueForReplace {
        get {
            if (ToStringPossible) { Develop.DebugPrint(FehlerArt.Fehler, "Routine muss überschrieben werden!"); }

            return "\"" + MyClassId + ";" + Name + "\"";
        }
        set {
            var x = TryParse(value, null);
            if (x == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler");
            }
            SetValue(x);
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
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback(null, "Kein Wert zum Parsen angekommen."); }

        if (txt.StartsWith("(")) {
            var (pose, _) = NextText(txt, 0, KlammerZu, false, false, KlammernStd);
            if (pose < txt.Length - 1) {
                // Wir haben so einen Fall: (true) || (true)
                var tmp = GetVariableByParsing(txt.Substring(1, pose - 1), s);
                if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return new DoItFeedback(null, "Befehls-Berechnungsfehler in ():" + tmp.ErrorMessage); }
                if (tmp.Variable == null) { return new DoItFeedback(null, "Allgemeiner Befehls-Berechnungsfehler"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(null, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(tmp.Variable.ValueForReplace + txt.Substring(pose + 1), s);
            }
        }

        txt = txt.DeKlammere(true, false, false, true);

        var (uu, _) = NextText(txt, 0, Method_if.UndUnd, false, false, KlammernStd);
        if (uu > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, uu), s);
            return !string.IsNullOrEmpty(txt1.ErrorMessage)
                ? new DoItFeedback(null, "Befehls-Berechnungsfehler vor &&: " + txt1.ErrorMessage)
                : txt1.Variable.ValueForReplace == "false"
                    ? txt1
                    : GetVariableByParsing(txt.Substring(uu + 2), s);
        }

        var (oo, _) = NextText(txt, 0, Method_if.OderOder, false, false, KlammernStd);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, oo), s);
            return !string.IsNullOrEmpty(txt1.ErrorMessage)
                ? new DoItFeedback(null, "Befehls-Berechnungsfehler vor ||: " + txt1.ErrorMessage)
                : txt1.Variable.ValueForReplace == "true"
                    ? txt1
                    : GetVariableByParsing(txt.Substring(oo + 2), s);
        }

        if (s != null) {
            var t = Method.ReplaceVariable(txt, s);
            if (!string.IsNullOrEmpty(t.ErrorMessage)) { return new DoItFeedback(null, "Variablen-Berechnungsfehler: " + t.ErrorMessage); }
            if (t.Variable != null) { return new DoItFeedback(null, t.Variable); }
            if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, s); }
        }

        if (s != null) {
            var t = Method.ReplaceComands(txt, s);
            if (!string.IsNullOrEmpty(t.ErrorMessage)) { return new DoItFeedback(null, "Befehls-Berechnungsfehler: " + t.ErrorMessage); }
            if (t.Variable != null) { return new DoItFeedback(null, t.Variable); }
            if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, s); }
        }

        var (posa, _) = NextText(txt, 0, KlammerAuf, false, false, KlammernStd);
        if (posa > -1) {
            var (pose, _) = NextText(txt, posa, KlammerZu, false, false, KlammernStd);
            if (pose <= posa) { return DoItFeedback.Klammerfehler(null); }

            var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
            if (!string.IsNullOrEmpty(tmptxt)) {
                var tmp = GetVariableByParsing(tmptxt, s);
                if (!string.IsNullOrEmpty(tmp.ErrorMessage)) { return new DoItFeedback(null, "Befehls-Berechnungsfehler in ():" + tmp.ErrorMessage); }
                if (tmp.Variable == null) { return new DoItFeedback(null, "Allgemeiner Berechnungsfehler in ()"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(null, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(txt.Substring(0, posa) + tmp.Variable.ValueForReplace + txt.Substring(pose + 1), s);
            }
        }

        if (Script.VarTypes == null) {
            return new DoItFeedback(null, "Variablentypen nicht initialisiert");
        }

        foreach (var thisVT in Script.VarTypes) {
            if (thisVT.GetFromStringPossible) {
                if (thisVT.TryParse(txt, out var v, s)) {
                    return new DoItFeedback(null, v);
                }
            }
        }

        return new DoItFeedback(null, "Wert kann nicht geparsed werden: " + txt);
    }

    public static bool IsValidName(string v) {
        v = v.ToLower();
        var vo = v;
        v = v.ReduceToChars(Constants.AllowedCharsVariableName);
        return v == vo && !string.IsNullOrEmpty(v);
    }

    public abstract object Clone();

    public int CompareTo(object obj) {
        if (obj is Variable v) {
            return CheckOrder.CompareTo(v.CheckOrder);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public abstract DoItFeedback GetValueFrom(Variable attvarAttribute);

    public override void Parse(string toParse) {
        //IsParsing = true;
        //ThrowEvents = false;
        //PermissionGroups_Show.ThrowEvents = false;
        //Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "key":
                case "name":
                    //if (pair.Value.FromNonCritical() != KeyName) {
                    //    Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler: " + toParse);
                    //}
                    KeyName = pair.Value.FromNonCritical();
                    break;

                case "classid":
                case "type":
                    if (pair.Value.ToNonCritical() != MyClassId) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler: " + toParse);
                    }
                    break;

                case "value":
                    ValueForReplace = pair.Value.FromNonCritical();
                    break;

                case "comment":
                    _comment = pair.Value.FromNonCritical();
                    break;

                case "readonly":
                    ReadOnly = pair.Value.FromPlusMinus();
                    break;

                case "system":
                    SystemVariable = pair.Value.FromPlusMinus();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        //PermissionGroups_Show.ThrowEvents = true;
        //ThrowEvents = true;
        //IsParsing = false;
    }

    public string ReplaceInText(string txt) {
        if (!txt.ToLower().Contains("~" + Name.ToLower() + "~")) { return txt; }
        return txt.Replace("~" + Name + "~", ReadableText, RegexOptions.IgnoreCase);
    }

    public new string ToString() {
        if (!ToStringPossible) { return string.Empty; }

        var result = new List<string>();
        //result.ParseableAdd("Type", ShortName);
        //result.ParseableAdd("Name", Name);
        result.ParseableAdd("Value", ValueForReplace);
        result.ParseableAdd("Comment", Comment);
        result.ParseableAdd("ReadOnly", ReadOnly);
        result.ParseableAdd("System", SystemVariable);

        return result.Parseable(base.ToString());
    }

    protected abstract Variable? NewWithThisValue(object x, Script s);

    protected abstract void SetValue(object? x);

    protected abstract object? TryParse(string txt, Script? s);

    protected bool TryParse(string txt, out Variable? succesVar, Script s) {
        var x = TryParse(txt, s);
        if (x == null) {
            succesVar = null;
            return false;
        }

        succesVar = NewWithThisValue(x, s);
        return succesVar != null;
    }

    #endregion
}