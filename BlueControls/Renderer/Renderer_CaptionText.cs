// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Extended_Text;
using System.Text;

namespace BlueControls.Renderer;

public class Renderer_CaptionText : Renderer_Abstract {

    #region Fields

    private string _captionStartSequence = "#";
    private string _tabSequence = ";";

    #endregion

    #region Properties

    public static string ClassId => "CaptionText";

    /// <summary>
    /// Sequenz, mit der eine Zeile beginnen muss, damit sie als Überschrift
    /// (PadStyles.Title) dargestellt wird. Die Sequenz wird beim Rendern
    /// entfernt. Leerstring deaktiviert die Überschrift-Erkennung.
    /// </summary>
    public string CaptionStartSequence {
        get => _captionStartSequence;
        set {
            if (_captionStartSequence == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _captionStartSequence = value;
            OnPropertyChanged();
        }
    }

    public override string Description =>
        "Mehrzeiliger Text. Zeilen, die mit der Überschrift-Markierung beginnen, werden als Überschrift dargestellt. Die Tabulator-Sequenz springt zum nächsten Raster und zeichnet eine vertikale Trennlinie.";

    /// <summary>
    /// Sequenz, die im Zelltext durch einen Tabulator ersetzt wird.
    /// Der Tabulator springt zum nächsten Raster und zeichnet am Ende
    /// eine dünne vertikale Linie. Leerstring deaktiviert die Tabulator-Ersetzung.
    /// </summary>
    public string TabSequence {
        get => _tabSequence;
        set {
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

        var html = BuildHtml(content);
        if (string.IsNullOrEmpty(html)) { return; }

        var canvasWidth = Math.Max(1, (int)(drawingAreaControl.Width / zoom));

        using var _txt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(canvasWidth, -1),
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
                _captionStartSequence = value.FromNonCritical();
                return true;

            case "tabsequence":
                _tabSequence = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Text mit Überschriften";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var html = BuildHtml(content);

        using var _etxt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(400, -1)
        };

        return _etxt.LastSize();
    }

    protected override Size CalculateContentSizeAtWidth(string content, TranslationType doOpticalTranslation, int canvasWidth) {
        var html = BuildHtml(content);

        using var _etxt = new ExtText(SheetStyle, PadStyles.Standard) {
            HtmlText = html,
            TextDimensions = new Size(Math.Max(1, canvasWidth), -1)
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
    /// <see cref="CaptionStartSequence"/> beginnen, werden zu &lt;h1&gt;-Überschriften.
    /// Die <see cref="TabSequence"/> wird durch &lt;tab&gt;&lt;vl&gt; ersetzt.
    /// </summary>
    private string BuildHtml(string content) {
        if (string.IsNullOrEmpty(content)) { return string.Empty; }

        var hasCaption = _captionStartSequence.Length > 0;
        var hasTab = _tabSequence.Length > 0;

        if (!hasCaption && !hasTab) { return content; }

        var lines = content.SplitAndCutByCrAndBr();
        var sb = new StringBuilder(content.Length + lines.Length * 8);

        for (var i = 0; i < lines.Length; i++) {
            if (i > 0) { sb.Append("<br>"); }

            var line = lines[i];

            if (hasCaption && line.StartsWith(_captionStartSequence, StringComparison.Ordinal)) {
                var rest = line[_captionStartSequence.Length..];

                if (hasTab) {
                    rest = rest.Replace(_tabSequence, "<tab><vl>");
                }

                sb.Append("<h1>").Append(rest).Append("</h1>");
            } else {
                if (hasTab) {
                    line = line.Replace(_tabSequence, "<tab><vl>");
                }

                sb.Append(line);
            }
        }

        return sb.ToString();
    }

    #endregion
}