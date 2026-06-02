// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;

namespace BlueControls.Renderer;

public class Renderer_CellNote : Renderer_Abstract {

    #region Fields

    private bool _showSymbol = true;
    private bool _showText = true;
    private int _symbolSize = 16;

    #endregion

    #region Properties

    public static string ClassId => "CellNote";

    public override string Description => "Stellt eine Zellnotiz mit Symbol und Text dar.\r\nFormat: Symbol|Text (z.B. Warning|Bitte prüfen)";

    public bool ShowSymbol {
        get => _showSymbol;
        set {
            if (_showSymbol == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showSymbol = value;
            OnPropertyChanged();
        }
    }

    public bool ShowText {
        get => _showText;
        set {
            if (_showText == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showText = value;
            OnPropertyChanged();
        }
    }

    public int SymbolSize {
        get => _symbolSize;
        set {
            if (_symbolSize == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _symbolSize = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        var (symbol, _) = ParseContent(content);
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        QuickImage? qi = null;
        if (_showSymbol) {
            var pix = _symbolSize.CanvasToControl(zoom);
            qi = NoteEntry.GetQuickImage(symbol, pix);
        }

        Skin.Draw_FormatedText(gr, replacedText, qi, align, drawingAreaControl, GetFont(zoom, design, state), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControlForProperty<bool>(() => ShowSymbol),
            new FlexiControlForProperty<bool>(() => ShowText),
            new FlexiControlForProperty<int>(() => SymbolSize)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ShowSymbol", _showSymbol);
        result.ParseableAdd("ShowText", _showText);
        result.ParseableAdd("SymbolSize", _symbolSize);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "showsymbol":
                _showSymbol = value.FromPlusMinus();
                return true;

            case "showtext":
                _showText = value.FromPlusMinus();
                return true;

            case "symbolsize":
                _symbolSize = Converter.IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Zellnotiz";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stift);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var contentSize = GetFont().FormatedText_NeededSize(replacedText, null, 16);

        if (_showSymbol) {
            contentSize.Width += _symbolSize + 4;
            contentSize.Height = Math.Max(contentSize.Height, _symbolSize + 4);
        }

        return contentSize;
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (!_showText && !_showSymbol) { return string.Empty; }

        var (symbol, text) = ParseContent(content);

        if (!_showText) {
            return symbol.ToString();
        }

        return LanguageTool.PrepaireText(text, style, string.Empty, string.Empty, doOpticalTranslation, null);
    }

    private static (NoteSymbols Symbol, string Text) ParseContent(string content) {
        var parts = content.SplitBy("|");
        if (parts.Length < 2) { return (NoteSymbols.Pencil, content); }

        var symbol = Enum.TryParse<NoteSymbols>(parts[0], true, out var s) ? s : NoteSymbols.Pencil;
        var text = content[(content.IndexOf('|') + 1)..];
        return (symbol, text);
    }

    #endregion
}
