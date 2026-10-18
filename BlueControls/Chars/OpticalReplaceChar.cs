// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Chars;

/// <summary>
/// Unsichtbares Steuerzeichen, das hinter einem Textteil steht und anzeigt, wie dessen optischer Ersatz aussehen würde.
/// Der Ersatztext wird halb so groß und hellgrau dargestellt. Das Zeichen wird nicht im Text generiert,
/// kann nicht gelöscht werden und wird vom Cursor übersprungen.
/// </summary>
public class OpticalReplaceChar : Char {

    #region Fields

    private BlueFont? _previewFont;

    #endregion

    #region Constructors

    public OpticalReplaceChar() { }

    internal OpticalReplaceChar(ExtText parent, List<string> overrideTags, string replacementText) : base(parent, overrideTags) => ReplacementText = replacementText;

    internal OpticalReplaceChar(ExtText parent, int styleFromPos, string replacementText) : base(parent, styleFromPos) => ReplacementText = replacementText;

    #endregion

    #region Properties

    public override bool IsProtected => true;

    /// <summary>
    /// Der Textteil, wie er ersetzt aussehen würde.
    /// </summary>
    public string ReplacementText { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        if (string.IsNullOrEmpty(ReplacementText)) { return; }
        try {
            PreviewFont()?.DrawString(gr, ReplacementText, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    public override string HtmlText() => string.Empty;

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => string.Empty;

    internal override void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        if (string.IsNullOrEmpty(ReplacementText)) { return; }
        try {
            PreviewFont()?.DrawString(gr, ReplacementText, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    internal override void InvalidateFont() {
        base.InvalidateFont();
        _previewFont = null;
    }

    protected override SizeF CalculateSizeCanvas() => string.IsNullOrEmpty(ReplacementText) ? SizeF.Empty : PreviewFont()?.MeasureString(ReplacementText) ?? SizeF.Empty;

    /// <summary>
    /// Halbe Schriftgröße in Hellgrau, abgeleitet von der Schrift der Umgebung.
    /// </summary>
    private BlueFont? PreviewFont() {
        if (_previewFont is not null) { return _previewFont; }
        var bf = BaseFont;
        _previewFont = BlueFont.Get(bf.FontName, bf.Size * 0.9f, false, false, false, false, Color.Gray, bf.ColorOutline, bf.ColorBack);
        return _previewFont;
    }

    #endregion
}