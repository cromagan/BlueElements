// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Variables;
using BlueTable.Classes;

namespace BlueTable.AdditionalScriptVariables;

public class VariableTable : Variable {

    #region Fields

    private string _lastText = string.Empty;
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
    public override bool IsNullOrEmpty => _table == null;

    /// <summary>
    /// Gibt den Text "Table: Caption" zurück.
    /// </summary>
    public override string ReadableText => _lastText;

    public override string SearchValue => ReadableText;

    public Table? Table {
        get => _table;
        private set {
            if (ReadOnly) { return; }
            _table = value;

            GetText();
        }
    }

    public override bool ToStringPossible => true;
    public override string ValueForCell => string.Empty;
    public override string ValueForReplace => _table is not { IsDisposed: false } tb ? "{TBL:?}" : "{TBL:" + tb.KeyName + "}";

    #endregion

    #region Methods

    public override void DisposeContent() => _table = null;

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableTable v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        Table = v.Table;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        if (x is null) {
            _table = null;
        } else if (x is Table db) {
            _table = db;
        } else {
            Develop.DebugError("Variablenfehler!");
        }
        GetText();
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        if (txt.Length > 6 && txt.StartsWith("{TBL:") && txt.EndsWith('}')) {
            var t = txt[5..^1];

            if (t == "?") { return true; }

            if (Table.Get(t, null) is not { IsDisposed: false } tb) { return false; }

            result = tb;
            return true;
        }

        return false;
    }

    private void GetText() => _lastText = _table == null ? "Table: [NULL]" : "Table: " + _table.KeyName;

    #endregion
}