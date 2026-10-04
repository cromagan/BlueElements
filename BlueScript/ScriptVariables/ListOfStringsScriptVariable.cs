// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.ClassesStatic;

namespace BlueScript.ScriptVariables;

public class ListOfStringsScriptVariable : ScriptVariable {

    #region Fields

    private List<string> _list;

    #endregion

    #region Constructors

    public ListOfStringsScriptVariable(string name, IReadOnlyCollection<string>? value, bool ronly, string comment) : base(name,
        ronly, comment) {
        _list = [];
        if (value is not null) {
            _list.AddRange(value);
        }
    }

    public ListOfStringsScriptVariable() : this(string.Empty, null, true, string.Empty) { }

    public ListOfStringsScriptVariable(string name) : this(name, null, true, string.Empty) { }

    public ListOfStringsScriptVariable(IReadOnlyCollection<string>? value) : this(DummyName(), value, true, string.Empty) { }

    public ListOfStringsScriptVariable(IEnumerable<string> value) : this([.. value]) { }

    #endregion

    #region Properties

    public static string ClassId => "lst";
    public override int CheckOrder => 3;
    public override bool GetFromStringPossible => true;

    public override string InitializationSample => "var Name = [\"A\", \"B\"];";

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

    public override bool ToStringPossible => true;

    public override string ValueForCell {
        get => string.Join('\r', _list);
        set => ValueList = [.. value.SplitAndCutByCr()];
    }

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

    public override DoItFeedback GetValueByIndex(ScriptVariable index) {
        var e = GetIndexPosition(index, out var i);
        if (e is { Length: > 0 }) { return new DoItFeedback(e, true); }

        return new DoItFeedback(_list[i]);
    }

    public override string GetValueFrom(ScriptVariable variable) {
        if (variable is not ListOfStringsScriptVariable v) { return VerschiedeneTypen(variable); }
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
        BeginInit();
        try {
            SetValue(json.GetStringList("value"));
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override string SetValueByIndex(ScriptVariable index, ScriptVariable value) {
        var e = GetIndexPosition(index, out var i);
        if (e is { Length: > 0 }) { return e; }
        if (ReadOnly) { return Schreibgschützt(); }
        if (value is not StringScriptVariable s) { return VerschiedeneTypen(value); }

        _list[i] = s.ValueString;
        OnPropertyChangedExt("value", _list);
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        switch (x) {
            case List<string> val:
                _list = val;
                break;

            case string[] val2:
                _list = [.. val2];
                break;

            default:
                Develop.DebugError("Variablenfehler!");
                break;
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

    /// <summary>
    /// Prüft den Index und liefert die Position. Bei Fehlern wird die Meldung zurückgegeben, sonst eine leere Zeichenfolge.
    /// </summary>
    private string GetIndexPosition(ScriptVariable index, out int position) {
        position = -1;

        if (index is not DoubleScriptVariable d) { return "Der Index muss eine Zahl sein."; }

        var i = d.ValueInt;
        if (Math.Abs(d.ValueNum - i) > 0.0001) { return "Der Index muss eine ganze Zahl sein."; }
        if (i < 0 || i >= _list.Count) { return "Index " + i + " ist außerhalb des gültigen Bereichs. Die Liste '" + KeyName + "' enthält " + _list.Count + " Elemente."; }

        position = i;
        return string.Empty;
    }

    #endregion
}