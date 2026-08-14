// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Chars;

internal class VlChar : Char {

    #region Constructors

    public VlChar() { }

    internal VlChar(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    internal override bool HandlesOwnLayout => true;
    internal override string? StructuralTag => "VL";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        if (controlSize.Height < 1) { return; }
        gr.DrawLine(Skin.PenLinieDünn, controlPos.X, controlPos.Y, controlPos.X, controlPos.Y + controlSize.Height);
    }

    public override string HtmlText() => "<vl>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override string PlainText() => string.Empty;

    internal override (float ContinueX, float ContinueY, float MaxRight, float MaxBottom) ComputeCharLayout(float startX, float startY, float maxWidth, float lineStartX, float lineSpacing) {
        var h = Font is null ? 16f : Font.CharHeight;
        PosCanvas = new PointF(startX, startY);
        SetSize(new SizeF(0, h));
        return (startX, startY, startX, startY + h);
    }

    protected override SizeF CalculateSizeCanvas() {
        var h = Font is null ? 16f : Font.CharHeight;
        return new SizeF(0, h);
    }

    #endregion
}