// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Chars;

internal class IconChar : Char {

    #region Fields

    private QuickImage? _qi;

    #endregion

    #region Constructors

    public IconChar() { }

    internal IconChar(ExtText parent, List<string> overrideTags, QuickImage? qi) : base(parent, overrideTags) => _qi = qi;

    internal IconChar(ExtText parent, int styleFromPos, QuickImage? qi) : base(parent, styleFromPos) => _qi = qi;

    #endregion

    #region Properties

    public override Alignment RowAlignment => Alignment.VerticalCenter;
    internal override string? StructuralTag => "IMAGECODE";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

        if (_qi is null) { return; }
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
        if (attribut is null) {
            _qi = null;
        } else if (!attribut.Contains('|')) {
            _qi = QuickImage.Get(attribut, (int)resolvedFont.Oberlänge(1));
        } else {
            _qi = QuickImage.Get(attribut);
        }
    }

    protected override SizeF CalculateSizeCanvas() => _qi is null ? SizeF.Empty : new SizeF(_qi.Width + 1, _qi.Height + 1);

    #endregion
}