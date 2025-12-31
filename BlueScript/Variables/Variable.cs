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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript.Methods;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;

namespace BlueScript.Variables;

public abstract class Variable : ParseableItem, IComparable, IParseable, IHasKeyName {

    #region Fields

    // Mit Caching für bessere Performance bei häufigen Aufrufen
    private static readonly ConcurrentDictionary<Type, Variable> _instanceCache = new();

    private static long _dummyCount;
    private string _comment = string.Empty;
    private string _keyName = string.Empty;

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
            if (field == null) {
                field = Generic.GetInstaceOfType<Variable>("NAME");
                field.Sort();
            }
            return field;
        }
    }

    public abstract int CheckOrder { get; }

    public string Comment {
        get => _comment;
        set {
            if (_comment == value) { return; }
            _comment = value;
        }
    }

    public string CompareKey => CheckOrder.ToString3() + "|" + KeyName.ToUpperInvariant();
    public abstract bool GetFromStringPossible { get; }
    public abstract bool IsNullOrEmpty { get; }
    public bool KeyIsCaseSensitive => false;

    public string KeyName {
        get => _keyName;
        set {
            if (_keyName == value) { return; }
            _keyName = value;
        }
    }

    public virtual string ReadableText => "Objekt: " + MyClassId;

    public bool ReadOnly {
        get;
        set {
            if (field == value) { return; }
            field = value;
        }
    }

    public abstract string SearchValue { get; }

    /// <summary>
    /// Wichtig für Boolsche Vergleiche
    /// </summary>
    public abstract bool ToStringPossible { get; }

    public abstract string ValueForCell { get; }

    /// <summary>
    /// Wichtig für Boolsche Vergleiche
    /// </summary>
    public virtual string ValueForReplace {
        get {
            if (ToStringPossible) { Develop.DebugPrint_RoutineMussUeberschriebenWerden(true); }

            return "\"" + MyClassId + ";" + KeyName + "\"";
        }
        set {
            if (!TryParseValue(value, out var result)) {
                Develop.DebugPrint(ErrorType.Error, $"Variablenfehler({MyClassId}): {value}");
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

    public static bool TryParseValue<T>(string txt, out object? result) where T : Variable, new() {
        var instance = _instanceCache.GetOrAdd(typeof(T), _ => new T());
        return instance.TryParseValue(txt, out result);
    }

    public int CompareTo(object obj) {
        if (obj is Variable v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public abstract void DisposeContent();

    public abstract string GetValueFrom(Variable variable);

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

    public string ReplaceInText(string txt) => !txt.ContainsIgnoreCase("~" + KeyName + "~")
            ? txt
            : txt.Replace("~" + KeyName + "~", ReadableText, RegexOptions.IgnoreCase);

    public string Schreibgschützt() => $"Variable '{KeyName}' ist schreibgeschützt.";

    public new string ToString() => $"({MyClassId}){_keyName}";

    public bool TryParse(string txt, out Variable? succesVar) {
        if (!TryParseValue(txt, out var result)) {
            succesVar = null;
            return false;
        }

        // Neue Instanz des gleichen Typs mit NewByTypeName erstellen
        var newVar = NewByTypeName<Variable>(MyClassId);
        if (newVar == null) {
            succesVar = null;
            return false;
        }

        // Den Wert setzen
        newVar.SetValue(result);
        succesVar = newVar;

        return true;
    }

    public string VerschiedeneTypen(Variable var2) => $"Variable '{KeyName}' ist nicht der erwartete Typ {var2.MyClassId}, sondern {MyClassId}";

    /// <summary>
    /// Wird nur für Parse operationen gebraucht. Z.B. beim Laden von aus dem Dateisystem
    /// </summary>
    /// <param name="x"></param>
    protected abstract void SetValue(object? x);

    protected abstract bool TryParseValue(string txt, out object? result);

    #endregion
}