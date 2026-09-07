// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Chars;
using BlueControls.Controls;
using System.Text;

namespace BlueControls.Renderer;

/// <summary>
/// Mehrzeiliger Text. Zeilen, die mit der Überschrift-Markierung beginnen, werden als Überschrift dargestellt.
/// Die Tabulator-Sequenz springt zum nächsten Raster und zeichnet eine vertikale Trennlinie.
/// </summary>
public class TableRenderer : Renderer {

    #region Fields

    /// <summary>
    /// Sicherheitsabstand, damit die per MeasureString gekürzten Zeilen
    /// nicht bis an die Zellkante reichen.
    /// </summary>
    private const int _cutMargin = 8;

    private string _captionStartSequence = "#";
    private string _tabSequence = ";";

    #endregion

    #region Properties

    public static string ClassId => "TableRenderer";

    /// <summary>
    /// Sequenz, mit der eine Zeile beginnen muss, damit sie als Überschrift
    /// (PadStyles.Title) dargestellt wird. Die Sequenz wird beim Rendern
    /// entfernt. Leerstring deaktiviert die Überschrift-Erkennung.
    /// </summary>
    public string CaptionStartSequence {
        get => _captionStartSequence;
        set {
            value = value.RemoveInvisibleChars();
            if (_captionStartSequence == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _captionStartSequence = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Sequenz, die im Zelltext durch einen Tabulator ersetzt wird.
    /// Der Tabulator springt zum nächsten Raster und zeichnet am Ende
    /// eine dünne vertikale Linie. Leerstring deaktiviert die Tabulator-Ersetzung.
    /// </summary>
    public string TabSequence {
        get => _tabSequence;
        set {
            value = value.RemoveInvisibleChars();
            if (_tabSequence == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _tabSequence = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        var canvasWidth = Math.Max(1, (int)(drawingAreaControl.Width / zoom));

        var html = BuildHtml(content, canvasWidth);
        if (string.IsNullOrEmpty(html)) { return; }

        using var _txt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(canvasWidth, -1),
            WordWrap = false,
            Ausrichtung = align,
            AreaControl = drawingAreaControl,
        };

        _txt.Draw(gr, zoom, drawingAreaControl.Left, drawingAreaControl.Top);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) =>
    [
        new FlexiControlForProperty<string>(() => CaptionStartSequence),
        new FlexiControlForProperty<string>(() => TabSequence)
    ];

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("CaptionStartSequence", _captionStartSequence);
        result.ParseableAdd("TabSequence", _tabSequence);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "captionstartsequence":
                _captionStartSequence = value.FromNonCritical().RemoveInvisibleChars();
                return true;

            case "tabsequence":
                _tabSequence = value.FromNonCritical().RemoveInvisibleChars();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Text mit Überschriften";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var html = BuildHtml(content, 0);

        using var _etxt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(-1, -1),
            WordWrap = false
        };

        return _etxt.LastSize();
    }

    protected override Size CalculateContentSizeAtWidth(string content, TranslationType doOpticalTranslation, int canvasWidth) {
        canvasWidth = Math.Max(1, canvasWidth);

        var html = BuildHtml(content, canvasWidth);

        using var _etxt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(canvasWidth, -1),
            WordWrap = false
        };

        return _etxt.LastSize();
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (string.IsNullOrEmpty(content)) { return string.Empty; }

        var hasCaption = _captionStartSequence.Length > 0;
        var hasTab = _tabSequence.Length > 0;

        if (!hasCaption && !hasTab) { return content; }

        var lines = content.SplitAndCutByCrAndBr();
        var sb = new StringBuilder(content.Length);

        for (var i = 0; i < lines.Length; i++) {
            if (i > 0) { sb.Append("\r"); }

            var line = lines[i];

            if (hasCaption && line.StartsWith(_captionStartSequence, StringComparison.Ordinal)) {
                line = line[_captionStartSequence.Length..];
            }

            if (hasTab) {
                line = line.Replace(_tabSequence, "\t");
            }

            sb.Append(line);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Wandelt den Zellinhalt in ExtText-HTML um. Zeilen, die mit
    /// CaptionStartSequence beginnen, werden zu &lt;h1&gt;-Überschriften.
    /// Die TabSequence wird durch &lt;tab&gt;&lt;vl&gt; ersetzt. Bei maxWidth &gt; 0
    /// werden zu breite Zeilen mit ... abgeschnitten statt umgebrochen.
    /// </summary>
    private string BuildHtml(string content, int maxWidth) {
        if (string.IsNullOrEmpty(content)) { return string.Empty; }

        var hasCaption = _captionStartSequence.Length > 0;
        var hasTab = _tabSequence.Length > 0;

        if (!hasCaption && !hasTab && maxWidth < 1) { return content; }

        var lines = content.SplitAndCutByCrAndBr();
        var sb = new StringBuilder(content.Length + lines.Length * 8);

        for (var i = 0; i < lines.Length; i++) {
            if (i > 0) { sb.Append("<br>"); }

            var line = lines[i];
            var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard);
            var isCaption = false;

            if (hasCaption && line.StartsWith(_captionStartSequence, StringComparison.Ordinal)) {
                line = line[_captionStartSequence.Length..];
                font = Skin.GetBlueFont(SheetStyle, PadStyles.Title);
                isCaption = true;
            }

            if (maxWidth > _cutMargin) { line = CutLine(line, font, maxWidth - _cutMargin, hasTab); }

            if (hasTab) {
                line = line.Replace(_tabSequence, "<tab><vl> ");
            }

            if (isCaption) {
                sb.Append("<h1>").Append(line).Append("</h1>");
            } else {
                sb.Append(line);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Kürzt eine Zeile auf die verfügbare Breite; abgeschnittene Zeilen
    /// enden mit .... Tabulator-Sprünge ins Raster werden eingerechnet.
    /// </summary>
    private string CutLine(string line, BlueFont font, float maxWidth, bool hasTab) {
        if (maxWidth < 1) { return line; }
        if (!hasTab) { return BlueFont.TrimByWidth(font, line, maxWidth); }

        var segments = line.Split([_tabSequence], StringSplitOptions.None);
        var sb = new StringBuilder(line.Length);
        var x = 0f;

        for (var i = 0; i < segments.Length; i++) {
            if (i > 0) {
                x += TabChar.Raster - (x % TabChar.Raster);
                if (maxWidth - x < 1) { break; }
                sb.Append(_tabSequence);
            }

            var segment = BlueFont.TrimByWidth(font, segments[i], maxWidth - x);
            sb.Append(segment);

            if (segment.Length != segments[i].Length) { break; }

            x += font.MeasureString(segment).Width;
        }

        return sb.ToString();
    }

    #endregion
}