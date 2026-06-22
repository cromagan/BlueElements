// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;

namespace BlueControls.Renderer;

public class Renderer_Font : Renderer_Abstract {

    #region Fields

    private const string txt = "ABCabc123ÄÖÜäöü@ß?";

    #endregion

    #region Properties

    public static string ClassId => "Font";

    public override string Description => "Stellt eine Schriftart dar.";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }
        var baseFont = BlueFont.Get(content).Scale(zoom);
        if (state != States.Standard) {
            var skinFont = Skin.GetBlueFont(design, state);
            baseFont = BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, skinFont.ColorMain, baseFont.ColorOutline, baseFont.ColorBack);
        }

        Skin.Draw_FormatedText(gr, txt, null, align, drawingAreaControl, baseFont, false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override List<string> ParseableItems() => [];

    public override string ReadableText() => "Schriftart darstellen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Schriftart);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) => BlueFont.Get(content).FormatedText_NeededSize(txt, null, 16);

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) => string.Empty;

    #endregion
}