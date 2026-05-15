// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Constants;

namespace BlueControls.Extended_Text;

internal class ExtCharImageCode : ExtChar {

    #region Fields

    private QuickImage? _qi;

    #endregion

    #region Constructors

    public ExtCharImageCode() { }

    internal ExtCharImageCode(ExtText parent, List<string> overrideTags, QuickImage? qi) : base(parent, overrideTags) => _qi = qi;

    internal ExtCharImageCode(ExtText parent, int styleFromPos, QuickImage? qi) : base(parent, styleFromPos) => _qi = qi;

    #endregion

    #region Properties

    public override Alignment RowAlignment => Alignment.VerticalCenter;
    internal override string? StructuralTag => "IMAGECODE";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

        if (_qi == null) { return; }
        try {
            gr.DrawImageUnscaled(Math.Abs(zoom - 1) < DefaultTolerance ? _qi : _qi.Scale(zoom), controlPos.X, controlPos.Y);
        } catch { }
    }

    public override string HtmlText() => _qi?.HTMLCode ?? string.Empty;

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => string.Empty;

    internal override void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        base.InitFromTag(parent, tags, attribut);
        var resolvedFont = ResolveFont(parent.BaseFont, tags);
        _qi = string.IsNullOrEmpty(attribut) || !attribut.Contains('|')
            ? QuickImage.Get(attribut, (int)resolvedFont.Oberlänge(1))
            : QuickImage.Get(attribut);
    }

    protected override SizeF CalculateSizeCanvas() => _qi == null ? SizeF.Empty : new SizeF(_qi.Width + 1, _qi.Height + 1);

    #endregion
}