// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text.RegularExpressions;

namespace BlueScript.Variables;

public abstract class Variable : ParseableItem, IComparable, IParseable, IHasKeyName, IJsonParseable {

    #region Fields

    public static readonly AssemblyAwareCache<Variable> VarTypes = new();

    private static readonly ConcurrentCache<Type, Variable> _instanceCache = new(30);

    private static HashSet<string>? _commandNames;

    private static long _dummyCount;
    private string _comment = string.Empty;

    #endregion

    #region Constructors

    protected Variable(string name, bool ronly, string comment) : base() {
        if (string.IsNullOrEmpty(name)) { name = Generic.GetUniqueKey(); }

        KeyName = name;

        ReadOnly = ronly;
        Comment = comment;
    }

    #endregion

    #region Properties

    public static string Any_Plain => "any";
    public static string Any_Variable => "*any";
    public abstract int CheckOrder { get; }

    public string Comment {
        get => _comment;
        set {
            if (_comment == value) { return; }
            _comment = value;
            OnPropertyChangedExt("comment", _comment);
        }
    }

    public string CompareKey => CheckOrder.ToString3() + "|" + KeyName.ToUpperInvariant();
    public abstract bool GetFromStringPossible { get; }
    public abstract bool IsNullOrEmpty { get; }

    public string KeyName {
        get;
        set {
            value = value.ToUpperInvariant();
            if (field == value) { return; }
            field = value;
            OnPropertyChangedExt("key", field);
        }
    }

    public virtual string ReadableText => "Objekt: " + MyClassId;

    public bool ReadOnly { get; set; }

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
                Develop.DebugError($"Variablenfehler({MyClassId}): {value}");
            }
            SetValue(result);
        }
    }

    #endregion

    #region Methods

    public static string DummyName() {
        if (_dummyCount >= long.MaxValue) { _dummyCount = 0; }
        _dummyCount++;
        return "DUMMY" + _dummyCount;
    }

    public static bool IsValidName(string v) {
        v = v.ToLowerInvariant();
        var vo = v;
        v = v.ReduceToChars(AllowedCharsVariableName);
        if (v != vo || string.IsNullOrEmpty(v)) { return false; }

        _commandNames ??= new HashSet<string>(Method.AllMethods.Instances.Select(m => m.Command), StringComparer.OrdinalIgnoreCase);
        return !_commandNames.Contains(v);
    }

    public static bool TryParseValue<T>(string txt, out object? result) where T : Variable, new() {
        var instance = _instanceCache.GetOrAdd(typeof(T), _ => new T());
        return instance.TryParseValue(txt, out result);
    }

    public int CompareTo(object obj) {
        if (obj is Variable v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugError("Falscher Objecttyp!");
        return 0;
    }

    public abstract void DisposeContent();

    public abstract string GetValueFrom(Variable variable);

    public override List<string> ParseableItems() {
        if (!ToStringPossible) { return []; }

        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Key", KeyName);
        result.ParseableAdd("Value", ValueForReplace);
        result.ParseableAdd("Comment", Comment);
        result.ParseableAdd("ReadOnly", ReadOnly);
        return result;
    }

    /// <summary>
    /// JSON-Pendant zu <see cref="ParseableItems" />. Schreibt die gemeinsamen
    /// Metadaten aller Variablen (Typ, Schlüssel, Kommentar, Schreibschutz).
    /// Subklassen überschreiben diese Methode, rufen <c>base.ParseableJson()</c>
    /// auf und ergänzen ihren Wert.
    /// </summary>
    public virtual JsonObject ParseableJson() {
        var json = new JsonObject();
        json["type"] = MyClassId;
        json["key"] = KeyName;
        json["comment"] = Comment;
        json["readonly"] = ReadOnly;
        return json;
    }

    public virtual void ParseFinishedJson(JsonElement parsed) { }

    /// <summary>
    /// JSON-Pendant zu <see cref="ParseThis" />. Liest die gemeinsamen Metadaten.
    /// Subklassen überschreiben diese Methode, lesen ihren eigenen Wert aus
    /// <paramref name="json" /> und rufen am Ende <c>base.ParseJson(json)</c> auf.
    /// Nicht vorhandene Keys werden übersprungen, sodass auch Partial-Updates funktionieren.
    /// </summary>
    public virtual void ParseJson(JsonObject json) {
        KeyName = json.GetString("key");
        Comment = json.GetString("comment");
        ReadOnly = json.GetBool("readonly");
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "key":
            case "name":
            case "keyname":
                KeyName = value.FromNonCritical().ToUpperInvariant();
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

    public string ReplaceInText(string txt) => txt.Contains($"~{KeyName}~", StringComparison.OrdinalIgnoreCase)
            ? txt.Replace($"~{KeyName}~", ReadableText, RegexOptions.IgnoreCase)
            : txt;

    public string Schreibgschützt() => $"Variable '{KeyName}' ist schreibgeschützt.";

    public override string ToString() => $"({MyClassId}){KeyName}";

    public bool TryParse(string txt, out Variable? succesVar) {
        if (!TryParseValue(txt, out var result)) {
            succesVar = null;
            return false;
        }

        // Neue Instanz des gleichen Typs mit NewByTypeName erstellen
        var newVar = NewByTypeName<Variable>(MyClassId);
        if (newVar is null) {
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