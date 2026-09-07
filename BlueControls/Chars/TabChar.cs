// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Chars;

internal class TabChar : Char {

    #region Fields

    /// <summary>
    /// Breite des Rasters, in das der Tabulator zum nächsten Sprung positioniert.
    /// </summary>
    internal const float Raster = 150;

    #endregion

    #region Constructors

    public TabChar() { }

    internal TabChar(ExtText parent, List<string> overrideTags) : base(parent, overrideTags) { }

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

    protected override SizeF CalculateSizeCanvas() {
        var h = Font is null ? 16f : Font.CharHeight;
        return new SizeF(Raster - (PosCanvas.X % Raster), h);
    }

    #endregion
}