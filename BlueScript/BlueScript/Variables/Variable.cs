﻿// Authors:
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
using System.Text;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public abstract class Variable : ParsebleItem, IComparable, IParseable, ICloneable, IHasKeyName, IChangedFeedback {

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

    public string CompareKey => CheckOrder.ToString(Constants.Format_Integer3) + "|" + Name.ToUpper();
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
            var x = TryParse(value, null, MethodType.Standard, new List<Method>(), false, string.Empty);
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

    public static DoItFeedback GetVariableByParsing(string txt, LogData ld, VariableCollection vs, MethodType allowedMethods, List<Method> lm, bool changevalues, string scriptAttributes) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback(ld, "Kein Wert zum Parsen angekommen."); }

        if (txt.StartsWith("(")) {
            var (pose, _) = NextText(txt, 0, KlammerZu, false, false, KlammernStd);
            if (pose < txt.Length - 1) {
                // Wir haben so einen Fall: (true) || (true)
                var tmp = GetVariableByParsing(txt.Substring(1, pose - 1), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
                if (!tmp.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler in ()"); }
                if (tmp.Variable == null) { return new DoItFeedback(ld, "Allgemeiner Befehls-Berechnungsfehler"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(ld, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(tmp.Variable.ValueForReplace + txt.Substring(pose + 1), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
            }
        }

        txt = txt.DeKlammere(true, false, false, true);

        var (uu, _) = NextText(txt, 0, Method_If.UndUnd, false, false, KlammernStd);
        if (uu > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, uu), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
            if (!txt1.AllOk || txt1.Variable == null) {
                return new DoItFeedback(ld, "Befehls-Berechnungsfehler vor &&");
            }

            if (txt1.Variable.ValueForReplace == "false") {
                return txt1;
            }
            return GetVariableByParsing(txt.Substring(uu + 2), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
        }

        var (oo, _) = NextText(txt, 0, Method_If.OderOder, false, false, KlammernStd);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, oo), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
            if (!txt1.AllOk || txt1.Variable == null) {
                return new DoItFeedback(ld, "Befehls-Berechnungsfehler vor ||");
            }

            if (txt1.Variable.ValueForReplace == "true") {
                return txt1;
            }
            return GetVariableByParsing(txt.Substring(oo + 2), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
        }

        var t = Method.ReplaceVariable(txt, vs, ld);
        if (!t.AllOk) { return new DoItFeedback(ld, "Variablen-Berechnungsfehler"); }
        if (t.Variable != null) { return new DoItFeedback(t.Variable); }
        if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, ld, vs, allowedMethods, lm, changevalues, scriptAttributes); }

        var t2 = Method.ReplaceComands(txt, lm, ld, vs, allowedMethods, changevalues, scriptAttributes);
        if (!t2.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler"); }
        if (t2.Variable != null) { return new DoItFeedback(t2.Variable); }
        if (txt != t.AttributeText) { return GetVariableByParsing(t2.AttributeText, ld, vs, allowedMethods, lm, changevalues, scriptAttributes); }

        var (posa, _) = NextText(txt, 0, KlammerAuf, false, false, KlammernStd);
        if (posa > -1) {
            var (pose, _) = NextText(txt, posa, KlammerZu, false, false, KlammernStd);
            if (pose <= posa) { return DoItFeedback.Klammerfehler(ld); }

            var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
            if (!string.IsNullOrEmpty(tmptxt)) {
                var tmp = GetVariableByParsing(tmptxt, ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
                if (!tmp.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler in ()"); }
                if (tmp.Variable == null) { return new DoItFeedback(ld, "Allgemeiner Berechnungsfehler in ()"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(ld, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(txt.Substring(0, posa) + tmp.Variable.ValueForReplace + txt.Substring(pose + 1), ld, vs, allowedMethods, lm, changevalues, scriptAttributes);
            }
        }

        if (Script.VarTypes == null) {
            return new DoItFeedback(ld, "Variablentypen nicht initialisiert");
        }

        foreach (var thisVt in Script.VarTypes) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(txt, out var v, vs)) {
                    return new DoItFeedback(v);
                }
            }
        }

        return new DoItFeedback(ld, "Wert kann nicht geparsed werden: " + txt);
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
            return CompareKey.CompareTo(v.CompareKey);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public abstract DoItFeedback GetValueFrom(Variable variable, LogData ld);

    public override void Parse(string toParse) {
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

    protected abstract Variable? NewWithThisValue(object x, VariableCollection vs);

    protected abstract void SetValue(object? x);

    protected abstract object? TryParse(string txt, VariableCollection? vs, MethodType allowedMethods, List<Method> lm, bool changeValues, string scriptAttributes);

    protected bool TryParse(string txt, out Variable? succesVar, VariableCollection vs) {
        var x = TryParse(txt, vs, MethodType.None, new List<Method>(), false, string.Empty);
        if (x == null) {
            succesVar = null;
            return false;
        }

        succesVar = NewWithThisValue(x, vs);
        return succesVar != null;
    }

    #endregion
}