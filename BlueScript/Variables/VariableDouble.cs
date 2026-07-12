// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Globalization;

namespace BlueScript.Variables;

public class VariableDouble : Variable {

    #region Fields

    private double _double;

    #endregion

    #region Constructors

    public VariableDouble(string name, double value, bool ronly, string comment) : base(name, ronly, comment) => _double = value;

    public VariableDouble(double value) : this(DummyName(), value, true, string.Empty) { }

    public VariableDouble() : this(string.Empty, 0f, true, string.Empty) { }

    public VariableDouble(string name) : this(name, 0f, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "num";
    public static string ShortName_Plain => "num";
    public static string ShortName_Variable => "*num";
    public override int CheckOrder => 1;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;

    /// <summary>
    /// Der Zahlenwert mit maximal 5 Kommastellen (0.#####)
    /// </summary>
    public override string ReadableText => _double.ToString1_5();

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => true;

    public override string ValueForCell => ReadableText;

    /// <summary>
    /// Der Zahlenwert mit maximal 5 Kommastellen (0.#####)
    /// </summary>
    public override string ValueForReplace => ReadableText;

    public int ValueInt => (int)_double;

    public double ValueNum {
        get => _double;
        set {
            if (ReadOnly) { return; }
            _double = Math.Round(value, 5, MidpointRounding.AwayFromZero);
            OnPropertyChangedExt("value", _double);
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableDouble v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueNum = v.ValueNum;
        return string.Empty;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("value", _double);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        SetValue(json.GetDouble("value", _double));
        base.ParseJson(json);
    }

    protected override void SetValue(object? x) {
        if (x is float val) {
            _double = val;
        } else if (x is double vald) {
            _double = vald;
        } else {
            Develop.DebugError("Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        var (pos2, _) = NextText(txt, 0, RechenOperatoren, false, false, Brackets);
        if (pos2 >= 0) {
            var erg = MathFormulaParser.Ergebnis(txt);
            if (erg is null) { result = null; return false; }
            txt = erg.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoubleTryParse(txt, out var zahl)) {
            result = zahl;
            return true;
        }

        result = null;
        return false;
    }

    #endregion
}