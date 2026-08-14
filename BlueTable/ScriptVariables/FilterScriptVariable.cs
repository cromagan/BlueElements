// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptVariables;

public class FilterScriptVariable : ScriptVariable {

    #region Fields

    private BlueTable.Classes.FilterItem? _filter;
    private string _lastText = string.Empty;

    #endregion

    #region Constructors

    public FilterScriptVariable(BlueTable.Classes.FilterItem value) : this(DummyName(), value, true, string.Empty) { }

    public FilterScriptVariable() : this(string.Empty, null, true, string.Empty) { }

    public FilterScriptVariable(string name) : this(name, null, true, string.Empty) { }

    public FilterScriptVariable(string name, BlueTable.Classes.FilterItem? value, bool ronly, string comment) : base(name, ronly, comment) {
        _filter = value;
        GetText();
    }

    #endregion

    #region Properties

    public static string ClassId => "fil";
    public static string ShortName_Variable => "*fil";
    public override int CheckOrder => 99;

    public override bool GetFromStringPossible => false;

    public override bool IsNullOrEmpty => _filter?.IsOk() != true;

    public override string ReadableText => _lastText;

    public override bool ToStringPossible => false;

    public BlueTable.Classes.FilterItem? ValueFilterItem {
        get => _filter;
        private set {
            if (ReadOnly) { return; }
            _filter = value;
            GetText();
        }
    }

    public override string ValueForCell => string.Empty;

    #endregion

    #region Methods

    public override void DisposeContent() => _filter = null;

    public override string GetValueFrom(ScriptVariable variable) {
        if (variable is not FilterScriptVariable v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueFilterItem = v.ValueFilterItem;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    private void GetText() => _lastText = _filter?.IsOk() != true ? "Filter: [ERROR]" : "Filter: " + _filter.ReadableText();

    #endregion
}