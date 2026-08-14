// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptVariables;

public class RowScriptVariable : ScriptVariable {

    #region Fields

    private string _lastText = string.Empty;
    private RowItem? _row;

    #endregion

    #region Constructors

    public RowScriptVariable(string name, RowItem? value, bool ronly, string comment) : base(name, ronly, comment) {
        _row = value;
        GetText();
    }

    public RowScriptVariable() : this(string.Empty, null, true, string.Empty) { }

    public RowScriptVariable(RowItem? value) : this(DummyName(), value, true, string.Empty) { }

    public RowScriptVariable(string name) : this(name, null, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "row";
    public static string ShortName_Variable => "*row";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _row is not { IsDisposed: false };

    /// <summary>
    /// Gibt den Text "Row: ReadableText" zurück.
    /// </summary>
    public override string ReadableText => _lastText;

    public override bool ToStringPossible => true;

    public override string ValueForCell => string.Empty;

    public override string ValueForReplace {
        get {
            if (_row?.Table is not { IsDisposed: false } tb) { return "{ROW:?}"; }
            return $"{{ROW:{tb.KeyName};{_row.KeyName};{_row.ChunkValue}}}";
        }
    }

    public RowItem? ValueRowItem {
        get => _row;
        private set {
            if (ReadOnly) { return; }
            _row = value;

            GetText();
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() => _row = null;

    public override string GetValueFrom(ScriptVariable variable) {
        if (variable is not RowScriptVariable v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueRowItem = v.ValueRowItem;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        switch (x) {
            case null:
                _row = null;
                break;

            case RowItem r:
                _row = r;
                break;

            default:
                Develop.DebugError("Variablenfehler!");
                break;
        }
        GetText();
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        if (txt.Length > 6 && txt.StartsWith("{ROW:", StringComparison.OrdinalIgnoreCase) && txt.EndsWith('}')) {
            var t = txt[5..^1];

            if (t == "?") { return true; }

            var tx = t.SplitBy(";");
            if (tx.Length != 3) { return true; }

            if (Table.Get(tx[0]) is not { IsDisposed: false } tb) { return true; }

            tb.BeSureRowIsLoaded(tx[2]);

            if (tb.Row.GetByKey(tx[1]) is not { IsDisposed: false } row) { return true; }

            result = row;
            return true;
        }

        return false;
    }

    private void GetText() => _lastText = _row is not { IsDisposed: false } ? "Row: [NULL]" : "Row: " + _row.ReadableText();

    #endregion
}