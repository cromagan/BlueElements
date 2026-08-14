// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Chars;

internal class CrLfChar : Char {

    #region Constructors

    public CrLfChar() { }

    internal CrLfChar(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    internal CrLfChar(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    internal override string? StructuralTag => "BR";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "<br>";

    public override bool IsLineBreak() => true;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => "\r\n";

    protected override SizeF CalculateSizeCanvas() => Font is null ? new SizeF(0, 16) : new SizeF(0, Font.CharHeight);

    #endregion
}