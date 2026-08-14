// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Chars;

public class CellLinkCharEndChar : Char {

    #region Constructors

    public CellLinkCharEndChar() { }

    internal CellLinkCharEndChar(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    internal CellLinkCharEndChar(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    #endregion

    #region Properties

    internal override string? StructuralTag => "/CELLLINK";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "</celllink>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    #endregion
}