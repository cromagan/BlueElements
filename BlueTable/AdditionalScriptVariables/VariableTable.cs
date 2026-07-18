// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptVariables;

public class VariableTable : Variable {

    #region Fields

    private string _lastText = string.Empty;
    private string? _pendingTableKey;
    private Table? _table;

    #endregion

    #region Constructors

    public VariableTable(string name, Table? value, bool ronly, string comment) : base(name, ronly, comment) {
        _table = value;
        GetText();
    }

    public VariableTable() : this(string.Empty, null, true, string.Empty) { }

    public VariableTable(Table? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableTable(string name) : this(name, null, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "tbl";
    public static string ShortName_Variable => "*tbl";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _table is null && _pendingTableKey is not { Length: > 0 };

    /// <summary>
    /// Gibt den Text "Table: Caption" zurück.
    /// </summary>
    public override string ReadableText => _lastText;

    public override string SearchValue => ReadableText;

    public Table? Table {
        get {
            if (_table is null && _pendingTableKey is { Length: > 0 } key) {
                _table = Table.Get(key, null);
                _pendingTableKey = null;
            }
            return _table;
        }
        private set {
            if (ReadOnly) { return; }
            _pendingTableKey = null;
            _table = value;

            GetText();
        }
    }

    public override bool ToStringPossible => true;
    public override string ValueForCell => string.Empty;

    public override string ValueForReplace {
        get {
            if (_table is { IsDisposed: false } tb) { return "{TBL:" + tb.KeyName + "}"; }
            if (_pendingTableKey is { Length: > 0 } key) { return "{TBL:" + key + "}"; }
            return "{TBL:?}";
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() {
        _table = null;
        _pendingTableKey = null;
    }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableTable v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        _pendingTableKey = v._pendingTableKey;
        _table = v._table;
        GetText();
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        _pendingTableKey = null;
        switch (x) {
            case null:
                _table = null;
                break;
            case Table tb:
                _table = tb;
                break;
            case string key:
                _table = null;
                _pendingTableKey = key;
                break;
            default:
                Develop.DebugError("Variablenfehler!");
                break;
        }
        GetText();
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        if (txt.Length > 6 && txt.StartsWith("{TBL:", StringComparison.OrdinalIgnoreCase) && txt.EndsWith('}')) {
            var t = txt[5..^1];

            if (t == "?") { return true; }

            result = t;
            return true;
        }

        return false;
    }

    private void GetText() {
        if (_table is not null) {
            _lastText = "Table: " + _table.KeyName;
        } else if (_pendingTableKey is { Length: > 0 } key) {
            _lastText = "Table: " + key;
        } else {
            _lastText = "Table: [NULL]";
        }
    }

    #endregion
}