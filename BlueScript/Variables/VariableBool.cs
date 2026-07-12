// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Variables;

public class VariableBool : Variable {

    #region Fields

    private bool _valuebool;

    #endregion

    #region Constructors

    public VariableBool(string name, bool value, bool ronly, string comment) : base(name, ronly, comment) => _valuebool = value;

    public VariableBool(string name) : this(name, false, true, string.Empty) { }

    public VariableBool() : this(string.Empty, false, true, string.Empty) { }

    public VariableBool(bool value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bol";
    public static string ShortName_Plain => "bol";
    public static string ShortName_Variable => "*bol";
    public override int CheckOrder => 0;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string ReadableText => _valuebool.ToString();

    /// <summary>
    /// Der Wert + oder -
    /// </summary>
    public override string SearchValue => _valuebool.ToPlusMinus();

    public override bool ToStringPossible => true;

    public bool ValueBool {
        get => _valuebool;
        set {
            if (ReadOnly) { return; }
            _valuebool = value; // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
            OnPropertyChangedExt("value", _valuebool);
        }
    }

    public override string ValueForCell => _valuebool.ToPlusMinus();
    public override string ValueForReplace => _valuebool ? "true" : "false";

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableBool v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueBool = v.ValueBool;
        return string.Empty;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("value", _valuebool);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        SetValue(json.GetBool("value", _valuebool));
        base.ParseJson(json);
    }

    protected override void SetValue(object? x) {
        if (x is bool val) {
            _valuebool = val;
        } else {
            Develop.DebugError("Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        switch (txt.ToLowerInvariant()) {
            case "true":
            case "+":
            case "wahr":
                result = true;
                return true;

            case "false":
            case "-":
            case "falsch":
                result = false;
                return true;
        }

        return false;
    }

    #endregion
}