// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ItemCollectionPad.Abstract;

namespace BlueScript.Variables;

public class VariablePadItem : Variable {

    #region Fields

    private AbstractPadItem? _item;

    #endregion

    #region Constructors

    public VariablePadItem(string name, AbstractPadItem? value, bool ronly, string comment) : base(name, ronly, comment) => _item = value;

    public VariablePadItem(string name) : this(name, null, true, string.Empty) { }

    public VariablePadItem() : this(string.Empty, null, true, string.Empty) { }

    public VariablePadItem(AbstractPadItem? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    
    public static string ClassId => "ici";

    public static string ShortName_Variable => "*ici";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _item == null;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;
    public override string ValueForCell => string.Empty;

    public AbstractPadItem? ValuePadItem {
        get => _item;
        set {
            if (ReadOnly) { return; }
            _item = value;
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() => _item = null;

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariablePadItem v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValuePadItem = v.ValuePadItem;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    #endregion
}