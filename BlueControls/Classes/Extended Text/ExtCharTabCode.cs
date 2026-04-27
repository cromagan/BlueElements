// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Extended_Text;

internal class ExtCharTabCode : ExtChar {

    #region Constructors

    public ExtCharTabCode() { }

    internal ExtCharTabCode(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    internal override string? StructuralTag => "TAB";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "<tab>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => true;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => "\t";

    protected override SizeF CalculateSizeCanvas() => new SizeF(150 - (PosCanvas.X % 150), 0);

    #endregion
}