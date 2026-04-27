// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

internal class ExtCharTopCode : ExtChar {

    #region Constructors

    public ExtCharTopCode() { }

    internal ExtCharTopCode(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

    #endregion

    #region Properties

    public override bool ResetsYPosition => true;
    internal override string? StructuralTag => "TOP";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "<top>";

    public override bool IsLineBreak() => true;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => true;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    #endregion
}