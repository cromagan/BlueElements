// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using BlueScript.Methods;
using BlueScript.Structures;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public abstract class Variable : ParsebleItem, IComparable, IParseable, ICloneable, IHasKeyName {

    #region Fields

    private static long _dummyCount;
    private static List<Variable>? _varTypes;
    private string _comment = string.Empty;
    private string _keyName = string.Empty;

    private bool _readOnly;

    #endregion

    #region Constructors

    protected Variable(string name, bool ronly, string comment) : base() {
        if (string.IsNullOrEmpty(name)) { name = Generic.GetUniqueKey(); }

        KeyName = name.ToLowerInvariant();

        ReadOnly = ronly;
        Comment = comment;
    }

    #endregion

    #region Properties

    public static string Any_Plain => "any";

    public static string Any_Variable => "*any";

    public static List<Variable> VarTypes {
        get {
            if (_varTypes == null) {
                _varTypes = Generic.GetInstaceOfType<Variable>("NAME");
                _varTypes.Sort();
            }
            return _varTypes;
        }
    }

    public abstract int CheckOrder { get; }

    public string Comment {
        get => _comment;
        set {
            if (_comment == value) { return; }
            _comment = value;
            OnPropertyChanged("Comment");
        }
    }

    public string CompareKey => CheckOrder.ToStringInt3() + "|" + KeyName.ToUpperInvariant();

    public abstract bool GetFromStringPossible { get; }

    public abstract bool IsNullOrEmpty { get; }

    public string KeyName {
        get => _keyName;
        set {
            if (_keyName == value) { return; }
            _keyName = value;
            OnPropertyChanged("KeyName");
        }
    }

    public virtual string ReadableText => "Objekt: " + MyClassId;

    public bool ReadOnly {
        get => _readOnly;
        set {
            if (_readOnly == value) { return; }
            _readOnly = value;
            OnPropertyChanged("ReadOnly");
        }
    }

    public abstract string SearchValue { get; }

    /// <summary>
    /// Wichtig für Boolsche Vergleiche
    /// </summary>
    public abstract bool ToStringPossible { get; }

    /// <summary>
    /// Wichtig für Boolsche Vergleiche
    /// </summary>
    public virtual string ValueForReplace {
        get {
            if (ToStringPossible) { Develop.DebugPrint_RoutineMussUeberschriebenWerden(true); }

            return "\"" + MyClassId + ";" + KeyName + "\"";
        }
        set {
            var (cando, result) = TryParse(value, null, null);
            if (!cando) {
                Develop.DebugPrint(ErrorType.Error, "Variablenfehler");
            }
            SetValue(result);
        }
    }

    #endregion

    #region Methods

    public static string DummyName() {
        if (_dummyCount >= long.MaxValue) { _dummyCount = 0; }
        _dummyCount++;
        return "dummy" + _dummyCount;
    }

    public static DoItFeedback GetVariableByParsing(string txt, LogData? ld, VariableCollection? varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback(ld, "Kein Wert zum Parsen angekommen."); }

        if (txt.StartsWith("(")) {
            var (pose, _) = NextText(txt, 0, KlammerRundZu, false, false, KlammernAlle);
            if (pose < txt.Length - 1) {
                // Wir haben so einen Fall: (true) || (true)
                var tmp = GetVariableByParsing(txt.Substring(1, pose - 1), ld, varCol, scp);
                if (!tmp.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler in ()"); }
                if (tmp.Variable == null) { return new DoItFeedback(ld, "Allgemeiner Befehls-Berechnungsfehler"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(ld, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(tmp.Variable.ValueForReplace + txt.Substring(pose + 1), ld, varCol, scp);
            }
        }

        txt = txt.Trim(KlammernRund);

        var (uu, _) = NextText(txt, 0, Method_If.UndUnd, false, false, KlammernAlle);
        if (uu > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, uu), ld, varCol, scp);
            if (!txt1.AllOk || txt1.Variable is null or VariableUnknown) {
                return new DoItFeedback(ld, "Befehls-Berechnungsfehler vor &&");
            }

            if (txt1.Variable.ValueForReplace == "false") {
                return txt1;
            }
            return GetVariableByParsing(txt.Substring(uu + 2), ld, varCol, scp);
        }

        var (oo, _) = NextText(txt, 0, Method_If.OderOder, false, false, KlammernAlle);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, oo), ld, varCol, scp);
            if (!txt1.AllOk || txt1.Variable is null or VariableUnknown) {
                return new DoItFeedback(ld, "Befehls-Berechnungsfehler vor ||");
            }

            if (txt1.Variable.ValueForReplace == "true") {
                return txt1;
            }
            return GetVariableByParsing(txt.Substring(oo + 2), ld, varCol, scp);
        }

        // Variablen nur ersetzen, wenn Variablen auch vorhanden sind.
        var t = Method.ReplaceVariable(txt, varCol, ld);
        if (!t.AllOk) { return new DoItFeedback(ld, "Variablen-Berechnungsfehler"); }
        if (t.Variable != null) { return new DoItFeedback(t.Variable); }
        if (txt != t.AttributeText) { return GetVariableByParsing(t.AttributeText, ld, varCol, scp); }

        var t2 = Method.ReplaceCommandsAndVars(txt, varCol, ld, scp);
        if (!t2.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler"); }
        if (t2.Variable != null) { return new DoItFeedback(t2.Variable); }
        if (txt != t2.AttributeText) { return GetVariableByParsing(t2.AttributeText, ld, varCol, scp); }

        var (posa, _) = NextText(txt, 0, KlammerRundAuf, false, false, KlammernAlle);
        if (posa > -1) {
            var (pose, _) = NextText(txt, posa, KlammerRundZu, false, false, KlammernAlle);
            if (pose <= posa) { return DoItFeedback.KlammerFehler(ld); }

            var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
            if (!string.IsNullOrEmpty(tmptxt)) {
                var tmp = GetVariableByParsing(tmptxt, ld, varCol, scp);
                if (!tmp.AllOk) { return new DoItFeedback(ld, "Befehls-Berechnungsfehler in ()"); }
                if (tmp.Variable == null) { return new DoItFeedback(ld, "Allgemeiner Berechnungsfehler in ()"); }
                if (!tmp.Variable.ToStringPossible) { return new DoItFeedback(ld, "Falscher Variablentyp: " + tmp.Variable.MyClassId); }
                return GetVariableByParsing(txt.Substring(0, posa) + tmp.Variable.ValueForReplace + txt.Substring(pose + 1), ld, varCol, scp);
            }
        }

        //if (VarTypes == null) {
        //    return new DoItFeedback(ld, "Variablentypen nicht initialisiert");
        //}

        foreach (var thisVt in VarTypes) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(txt, out var v, varCol, scp) && v != null) {
                    return new DoItFeedback(v);
                }
            }
        }

        return new DoItFeedback(ld, "Wert kann nicht geparsed werden: " + txt);
    }

    public static bool IsValidName(string v) {
        v = v.ToLowerInvariant();
        var vo = v;
        v = v.ReduceToChars(AllowedCharsVariableName);
        if (v != vo || string.IsNullOrEmpty(v)) { return false; }

        foreach (var thisc in Method.AllMethods) {
            if (thisc.Command.Equals(v)) { return false; }
        }
        return true;
    }

    public abstract object Clone();

    public int CompareTo(object obj) {
        if (obj is Variable v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public abstract void DisposeContent();

    public abstract DoItFeedback GetValueFrom(Variable variable, LogData ld);

    public new List<string> ParseableItems() {
        if (!ToStringPossible) { return []; }

        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Key", KeyName);
        result.ParseableAdd("Value", ValueForReplace);
        result.ParseableAdd("Comment", Comment);
        result.ParseableAdd("ReadOnly", ReadOnly);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "key":
            case "name":
            case "keyname":
                _keyName = value.FromNonCritical();
                return true;

            case "classid":
            case "type":
                return value.ToNonCritical() == MyClassId;

            case "value":
                ValueForReplace = value.FromNonCritical();
                return true;

            case "comment":
                _comment = value.FromNonCritical();
                return true;

            case "readonly":
                ReadOnly = value.FromPlusMinus();
                return true;

            case "system": // Todo: Entfernen 21.12.2023
                //SystemVariable = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReplaceInText(string txt) => !txt.ToLowerInvariant().Contains("~" + KeyName.ToLowerInvariant() + "~")
            ? txt
            : txt.Replace("~" + KeyName + "~", ReadableText, RegexOptions.IgnoreCase);

    protected abstract Variable? NewWithThisValue(object? x);

    protected abstract void SetValue(object? x);

    protected abstract (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp);

    protected bool TryParse(string txt, out Variable? succesVar, VariableCollection varCol, ScriptProperties scp) {
        var (cando, result) = TryParse(txt, varCol, scp);
        if (!cando) {
            succesVar = null;
            return false;
        }

        succesVar = NewWithThisValue(result);
        return succesVar != null;
    }

    #endregion
}