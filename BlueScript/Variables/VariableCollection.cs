// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections;
using System.Collections.Concurrent;

namespace BlueScript.Variables;

public class VariableCollection : IEnumerable<Variable>, IEditable, IParseable {

    #region Fields

    private readonly ConcurrentDictionary<string, Variable> _internal = new(StringComparer.OrdinalIgnoreCase);
    private List<string>? _cachedStringableNames;

    #endregion

    #region Constructors

    public VariableCollection() { }

    /// <summary>
    /// Erstellt eine neue Liste aus Variablen, die ReadOnly ist
    /// </summary>
    /// <param name="v"></param>
    public VariableCollection(List<Variable> v) : this(v, true) { }

    public VariableCollection(List<Variable> v, bool readOnly) {
        foreach (var thisV in v) {
            Add(thisV);
        }
        ReadOnly = readOnly;
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Variablen";

    public int Count => _internal.Count;
    public bool ReadOnly { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt eine neue VariableCollection, die alle Variablen aus ExistingVars enthält.
    /// Überschreibt die Werte in den existingVars mit den Werten aus newValues. Die Werte aus NewValues können dabei einen besonderen Prefix haben.
    /// Beispiel: ExistingVars die Variable Test
    /// NewValues behinhaltet XX_Test.
    /// mit dem Prefix XX_ wird der Wert aus XX_Test nach Test geschrieben.
    /// </summary>
    /// <param name="existingVars"></param>
    /// <param name="newValues"></param>
    /// <param name="newValsPrefix"></param>
    /// <returns></returns>
    public static VariableCollection Combine(VariableCollection existingVars, VariableCollection newValues, string newValsPrefix) {
        var vaa = existingVars.ToList();

        foreach (var thisvar in vaa) {
            var v = newValues.GetByKey(newValsPrefix + thisvar.KeyName);
            if (v is null) { continue; }

            thisvar.ReadOnly = false; // weil kein OnPropertyChanged vorhanden ist
            var error = thisvar.GetValueFrom(v);
            thisvar.ReadOnly = true; // weil kein OnPropertyChanged vorhanden ist

            if (error is { Length: > 0 }) { Develop.DebugPrint(error); }
        }

        return [.. vaa];
    }

    public static List<Variable> ParseVariable(string toParse, bool setReadOnly) {
        List<Variable> result = [];

        foreach (var t in toParse.SplitAndCutByCr()) {
            if (ParseableItem.NewByParsing<Variable>(t) is not { } l) { continue; }

            if (setReadOnly) {
                l.ReadOnly = true; // Weil kein OnPropertyChangedEreigniss vorhanden ist
            }

            result.Add(l);
        }

        result.Sort();
        return result;
    }

    public bool Add(Variable? variable) {
        if (ReadOnly) { return false; }
        if (variable is null) { return false; }

        if (!_internal.TryAdd(variable.KeyName, variable)) { return false; }
        InvalidateCache();
        return true;
    }

    /// <summary>
    /// True, wenn ALLE Variablen erfolgreich hinzugefügt wurden.
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public bool AddRange(IEnumerable<Variable> vars) {
        if (ReadOnly) { return false; }

        var f = true;
        foreach (var thisv in vars) {
            if (!Add(thisv)) { f = false; }
        }

        return f;
    }

    //    foreach (var thisvar in vaa) {
    //        var v = newVars.Get(newVarsPrefix + thisvar.KeyName);
    public List<string> AllStringableNames() {
        // Used: Hot-Loop in Method.ReplaceVariable. Gibt die gecachte Liste direkt zurück,
        // da der einzige Aufrufer (NextText) nur liest. Add/Remove/Set invalidieren den Cache.
        if (_cachedStringableNames is not null) { return _cachedStringableNames; }

        _cachedStringableNames = _internal.Values
            .Where(thisvar => thisvar.ToStringPossible)
            .Select(thisvar => thisvar.KeyName)
            .ToList();

        return _cachedStringableNames;
    }

    //public static VariableCollection Combine(VariableCollection existingVars, Variable thisvar) {
    //    var vaa = new List<VariableString>();
    //    vaa.AddRange(existingVars.ToListVariableString());
    /// <summary>
    /// Gibt von allen Variablen, die ein String sind, den Inhalt ohne " am Anfang/Ende zurück.
    /// </summary>
    /// <returns></returns>
    public List<string> AllStringValues() {
        var l = new List<string>();

        foreach (var thisvar in _internal.Values) {
            if (thisvar is VariableString vs) { l.Add(vs.ValueString); }
        }

        return l;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool? GetBoolean(string name) {
        _internal.TryGetValue(name, out var v);

        if (v is not VariableBool vb) {
            //Develop.DebugPrint("Falscher Datentyp");
            return null;
        }

        return vb.ValueBool;
    }

    public Variable? GetByKey(string keyname) => _internal.TryGetValue(keyname, out var v) ? v : null;

    //public object Clone() => new VariableCollection(ToList(), ReadOnly);
    public IEnumerator<Variable> GetEnumerator() => _internal.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
    /// </summary>
    /// <param name="name"></param>
    public List<string> GetList(string name) {
        _internal.TryGetValue(name, out var v);
        if (v is null) { return []; }

        if (v is not VariableListString vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return [];
        }

        return vf.ValueList;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetString(string name) {
        _internal.TryGetValue(name, out var v);

        if (v is not VariableString vf) {
            //Develop.DebugPrint("Falscher Datentyp");
            return string.Empty;
        }

        return vf.ValueString;
    }

    public string IsNowEditable() => string.Empty;

    public List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Variable", _internal.Values);
        result.ParseableAdd("ReadOnly", ReadOnly);

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "variable":
                var v = ParseableItem.NewByParsing<Variable>(value);
                if (v is not null) { Add(v); }
                return true;

            case "readonly":
                ReadOnly = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public bool Remove(string keyName) {
        if (ReadOnly) { return false; }

        if (!_internal.TryRemove(keyName, out _)) { return false; }
        InvalidateCache();
        return true;
    }

    public void RemoveWithComment(string comment) {
        if (ReadOnly) { return; }

        var changed = false;
        foreach (var kvp in _internal) {
            if (kvp.Value.Comment.Contains(comment)) {
                _internal.TryRemove(kvp.Key, out _);
                changed = true;
            }
        }
        if (changed) { InvalidateCache(); }
    }

    public string ReplaceInText(string originalText) {
        foreach (var thisvar in _internal.Values) {
            originalText = thisvar.ReplaceInText(originalText);
        }
        return originalText;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, string value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new VariableString(n, string.Empty, false, string.Empty);
            _internal.TryAdd(n, v);
            InvalidateCache();
        }

        if (v is not VariableString vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueString = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, double value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new VariableDouble(n);
            _internal.TryAdd(n, v);
            InvalidateCache();
        }

        if (v is not VariableDouble vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueNum = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, List<string> value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new VariableListString(n);
            _internal.TryAdd(n, v);
            InvalidateCache();
        }

        if (v is not VariableListString vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueList = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, bool value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new VariableBool(n);
            _internal.TryAdd(n, v);
            InvalidateCache();
        }

        if (v is not VariableBool vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueBool = value;
        vf.ReadOnly = true;
        return true;
    }

    public List<Variable> ToList() {
        var l = new List<Variable>();
        l.AddRange(_internal.Values);
        return l.OrderBy(r => r.KeyName).ToList();
    }

    /// <summary>
    /// Gibt alle Variabelen Sortiern nach Keyname zurück, die 'ToStringPossible' sind.
    /// </summary>
    /// <returns></returns>
    public List<Variable> ToListVariableString() {
        var l = new List<Variable>();

        foreach (var thiss in _internal.Values.OrderBy(r => r.KeyName)) {
            if (thiss.ToStringPossible) {
                l.Add(thiss);
            }
        }

        return l;
    }

    internal void Clear() {
        if (ReadOnly) { return; }

        _internal.Clear();
        InvalidateCache();
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void InvalidateCache() {
        _cachedStringableNames = null;
    }

    #endregion
}