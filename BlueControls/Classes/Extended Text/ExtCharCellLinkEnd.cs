// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharCellLinkEnd : ExtChar {

    #region Constructors

    public ExtCharCellLinkEnd() { }

    internal ExtCharCellLinkEnd(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    internal ExtCharCellLinkEnd(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

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

    public override bool IsWordSeparator() => false;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    #endregion
}