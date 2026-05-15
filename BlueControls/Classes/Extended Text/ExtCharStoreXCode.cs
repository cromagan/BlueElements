// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Extended_Text;

internal class ExtCharStoreXCode : ExtChar {

    #region Constructors

    public ExtCharStoreXCode() { }

    internal ExtCharStoreXCode(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    public override bool StoresXPosition => true;
    internal override string? StructuralTag => "ZBX_STORE";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "<zbx_store>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    #endregion
}