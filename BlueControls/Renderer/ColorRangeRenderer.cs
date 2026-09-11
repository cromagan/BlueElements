// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;

namespace BlueControls.Renderer;

/// <summary>
/// Färbt den Zellhintergrund als Farbverlauf zwischen MinColor und
/// MaxColor, abhängig vom Zahlenwert im Bereich MinValue–MaxValue.
/// </summary>
public class ColorRangeRenderer : Renderer {

    #region Fields

    private Color _maxColor = Color.Red;
    private double _maxValue = 100.0;
    private Color _minColor = Color.White;
    private double _minValue;

    #endregion

    #region Constructors

    public ColorRangeRenderer() : base(false) { }

    #endregion

    #region Properties

    public static string ClassId => "ColorRange";

    /// <summary>
    /// Farbe des Zellhintergrunds beim Maximalwert.
    /// </summary>
    public Color MaxColor {
        get => _maxColor;
        set {
            if (_maxColor == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _maxColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Zahlenwert, bei dem der Farbverlauf MaxColor erreicht.
    /// </summary>
    public double MaxValue {
        get => _maxValue;
        set {
            if (Math.Abs(_maxValue - value) < double.Epsilon) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _maxValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Farbe des Zellhintergrunds beim Minimalwert.
    /// </summary>
    public Color MinColor {
        get => _minColor;
        set {
            if (_minColor == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _minColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Zahlenwert, bei dem der Farbverlauf MinColor erreicht.
    /// </summary>
    public double MinValue {
        get => _minValue;
        set {
            if (Math.Abs(_minValue - value) < double.Epsilon) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _minValue = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        var backColor = ColorOfValue(content);

        if (backColor is { } bgColor) {
            using var brush = new SolidBrush(bgColor);
            gr.FillRectangle(brush, drawingAreaControl);
        }

        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);
        var font = GetFont(zoom, design, state);

        // Auf dunklem Hintergrund weiße Schrift verwenden.
        if (backColor is { } bc && bc.GetBrightness() < 0.5f) {
            font = BlueFont.Get(font.FontName, font.Size, font.Bold, font.Italic, font.Underline, font.StrikeOut, Color.White, font.ColorOutline, font.ColorBack);
        }

        Skin.Draw_FormatedText(gr, replacedText, null, align, drawingAreaControl, font, false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<double>(() => MinValue),
            new FlexiControlForProperty<Color>(() => MinColor),
            new FlexiControlForProperty<double>(() => MaxValue),
            new FlexiControlForProperty<Color>(() => MaxColor)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("MinValue", _minValue);
        result.ParseableAdd("MinColor", _minColor);
        result.ParseableAdd("MaxValue", _maxValue);
        result.ParseableAdd("MaxColor", _maxColor);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "minvalue":
                _minValue = DoubleParse(value);
                return true;

            case "mincolor":
                _minColor = ColorParse(value);
                return true;

            case "maxvalue":
                _maxValue = DoubleParse(value);
                return true;

            case "maxcolor":
                _maxColor = ColorParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Wertbereich als Farbverlauf";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Farbrad);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var contentSize = GetFont().FormatedText_NeededSize(replacedText, null, 16);

        contentSize.Width = Math.Max(contentSize.Width, 16);
        contentSize.Height = Math.Max(contentSize.Height, 16);

        return contentSize;
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) => content;

    /// <summary>
    /// Interpoliert die Hintergrundfarbe zwischen MinColor und MaxColor.
    /// Werte außerhalb des Bereichs werden auf die Randfarben begrenzt.
    /// </summary>
    private Color? ColorOfValue(string content) {
        if (!DoubleTryParse(content, out var value)) { return null; }

        var range = _maxValue - _minValue;
        var ratio = range == 0 ? 1.0 : (value - _minValue) / range;

        if (ratio < 0) { ratio = 0; }
        if (ratio > 1) { ratio = 1; }

        return Color.FromArgb(
            (int)(_minColor.A + (_maxColor.A - _minColor.A) * ratio),
            (int)(_minColor.R + (_maxColor.R - _minColor.R) * ratio),
            (int)(_minColor.G + (_maxColor.G - _minColor.G) * ratio),
            (int)(_minColor.B + (_maxColor.B - _minColor.B) * ratio));
    }

    #endregion
}