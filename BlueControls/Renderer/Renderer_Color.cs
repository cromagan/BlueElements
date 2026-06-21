// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Renderer;

public class Renderer_Color : Renderer_Abstract {

    #region Fields

    private bool _showHex = true;
    private bool _showName = true;
    private bool _showSymbol = true;

    #endregion

    #region Properties

    public static string ClassId => "Color";

    public override string Description => "Kann Hex-Farbcode (RGB oder ARGB) anzeigen (z.B. #ff0000) ";

    public bool ShowHex {
        get => _showHex;
        set {
            if (_showHex == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showHex = value;
            OnPropertyChanged();
        }
    }

    public bool ShowName {
        get => _showName;
        set {
            if (_showName == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showName = value;
            OnPropertyChanged();
        }
    }

    public bool ShowSymbol {
        get => _showSymbol;
        set {
            if (_showSymbol == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showSymbol = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }
        QuickImage? qi = null;
        var pix = 16.CanvasToControl(zoom);

        var replacedText = content;
        if (ColorTryParse(content, out var col)) {
            replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

            if (_showSymbol) {
                qi = QuickImage.Get(ImageCode.Kreis, pix, Color.Transparent, col);
            }
        }

        if (_showSymbol && qi is null) { qi = QuickImage.Get(ImageCode.Fragezeichen, pix); }

        var pax = 4.ControlToCanvas(zoom);
        var pay = 2.ControlToCanvas(zoom);
        drawingAreaControl.Inflate((int)-pax, (int)-pay);

        Skin.Draw_FormatedText(gr, replacedText, qi, align, drawingAreaControl, GetFont(zoom, design, state), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<bool>(() => ShowSymbol),
            new FlexiControlForProperty<bool>(() => ShowHex),
            new FlexiControlForProperty<bool>(() => ShowName)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ShowSymbol", _showSymbol);
        result.ParseableAdd("ShowHex", _showHex);
        result.ParseableAdd("ShowName", _showName);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "showsymbol":
                _showSymbol = value.FromPlusMinus();
                return true;

            case "showhex":
                _showHex = value.FromPlusMinus();
                return true;

            case "showname":
                _showName = value.FromPlusMinus();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Farbanzeige";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Farbrad);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var contentSize = GetFont().FormatedText_NeededSize(replacedText, null, 16);

        if (ShowSymbol) {
            contentSize.Width += 18;
        }

        return contentSize;
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (!_showHex && !_showName) { return string.Empty; }

        if (!ColorTryParse(content, out var col)) { return content; }

        if (_showName && _showHex) {
            if (col.Name() is { Length: > 0 } n) { return $"{col.ToHtmlCode()} {n}"; }
            return col.ToHtmlCode();
        } else if (_showName) {
            if (col.Name() is { Length: > 0 } n) { return n; }
        }

        return col.ToHtmlCode();
    }

    #endregion
}