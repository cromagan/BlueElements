// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptVariables;

public class BitmapScriptVariable : ScriptVariable {

    #region Fields

    public static readonly List<string> BmpVar = [ShortName_Variable];
    private System.Drawing.Bitmap? _bmp;

    #endregion

    #region Constructors

    public BitmapScriptVariable(string name, System.Drawing.Bitmap? value, bool ronly, string comment) : base(name, ronly, comment) => _bmp = value;

    public BitmapScriptVariable() : this(string.Empty, null, true, string.Empty) { }

    public BitmapScriptVariable(string name) : this(name, null, true, string.Empty) { }

    public BitmapScriptVariable(System.Drawing.Bitmap? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bmp";
    public static string ShortName_Variable => "*bmp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _bmp is null;

    public override bool ToStringPossible => false;

    public System.Drawing.Bitmap? ValueBitmap {
        get => _bmp;
        set {
            if (ReadOnly) { return; }
            _bmp = value;
        }
    }

    public override string ValueForCell => string.Empty;

    #endregion

    #region Methods

    public override void DisposeContent() => _bmp?.Dispose();

    public override string GetValueFrom(ScriptVariable variable) {
        if (variable is not BitmapScriptVariable v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueBitmap = v.ValueBitmap;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    #endregion
}