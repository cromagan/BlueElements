// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.ClassesStatic;

namespace BlueScript.Variables;

public class VariableListString : Variable {

    #region Fields

    private List<string> _list;

    #endregion

    #region Constructors

    public VariableListString(string name, IReadOnlyCollection<string>? value, bool ronly, string comment) : base(name,
        ronly, comment) {
        _list = [];
        if (value is not null) {
            _list.AddRange(value);
        }
    }

    public VariableListString() : this(string.Empty, null, true, string.Empty) { }

    public VariableListString(string name) : this(name, null, true, string.Empty) { }

    public VariableListString(IReadOnlyCollection<string>? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableListString(IEnumerable<string> value) : this([.. value]) { }

    #endregion

    #region Properties

    public static string ClassId => "lst";
    public static string ShortName_Plain => "lst";
    public static string ShortName_Variable => "*lst";
    public override int CheckOrder => 3;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _list.Count == 0;

    /// <summary>
    /// Die Liste als Text formatiert. z.B. ["A", "B", "C"]
    /// Kritische Zeichen innerhalb eines Eintrags wurden unschädlich gemacht.
    /// </summary>
    public override string ReadableText {
        get {
            if (_list.Count == 0) { return "[ ]"; }

            var s = string.Empty;

            foreach (var thiss in _list) {
                s = s + "\"" + thiss.RemoveCriticalVariableChars() + "\", ";
            }

            return "[" + s.TrimEnd(", ") + "]";
        }
    }

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => true;
    public override string ValueForCell => string.Join('\r', _list);
    public override string ValueForReplace => ReadableText;

    public List<string> ValueList {
        get => _list;
        set {
            if (ReadOnly) { return; }

            _list = value;
            //if (value is not null) { _list.AddRange(value); }
            OnPropertyChangedExt("value", _list);
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableListString v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueList = v.ValueList;
        return string.Empty;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.SetArrayIfNotEmpty("value", _list);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        SetValue(json.GetStringList("value"));
        base.ParseJson(json);
    }

    protected override void SetValue(object? x) {
        if (x is List<string> val) {
            _list = val;
        } else if (x is string[] val2) {
            _list = [.. val2];
        } else {
            Develop.DebugError("Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        if (txt is "[]" or "[ ]") { result = new List<string>(); return true; } // Leere Liste

        if (txt.Length > 3 && txt.StartsWith("[\"", StringComparison.Ordinal) && txt.EndsWith("\"]", StringComparison.Ordinal)) {
            var t = txt[2..^2];

            t = t.Replace("\", \"", "\",\"");

            if (string.IsNullOrEmpty(t)) { result = new List<string>() { string.Empty }; return true; } // Leere Liste

            result = t.SplitBy("\",\"");
            return true;
        }

        result = null;
        return false;
    }

    #endregion
}