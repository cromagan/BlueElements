// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Pen = System.Drawing.Pen;

namespace BlueControls.Extended_Text;

internal class ExtCharHrCode : ExtChar {

    #region Constructors

    public ExtCharHrCode() { }

    internal ExtCharHrCode(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    internal override string? StructuralTag => "HR";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        if (controlSize.Width < 1) { return; }
        var lineY = controlPos.Y + (controlSize.Height / 2);
        using var pen = new Pen(Font?.ColorMain ?? Color.Black, 1.CanvasToControl(zoom));
        gr.DrawLine(pen, controlPos.X, lineY, controlPos.X + controlSize.Width, lineY);
    }

    public override string HtmlText() => "<hr>";

    public override bool IsLineBreak() => true;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => "\r\n";

    internal override (float ContinueX, float ContinueY, float MaxRight, float MaxBottom) ComputeCharLayout(float startX, float startY, float maxWidth, float lineStartX, float lineSpacing) {
        var h = Font is null ? 8f : Font.CharSize(65).Height;
        var w = maxWidth > startX ? maxWidth - startX : 0;
        PosCanvas = new PointF(startX, startY);
        SetSize(new SizeF(w, h));
        return (startX + w, startY, startX + w, startY + h);
    }

    protected override SizeF CalculateSizeCanvas() => Font is null ? new SizeF(0, 8) : new SizeF(0, Font.CharSize(65).Height);

    #endregion
}
