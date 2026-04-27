// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Extended_Text;

namespace BlueControls.Renderer;

public class Renderer_RichText : Renderer_Abstract {

    #region Properties

    public static string ClassId => "RichText";

    public override string Description => "Langsame, aber korrekte Anzeige mit Formatierungen.";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom) {
        if (string.IsNullOrEmpty(content)) { return; }

        //gr.SetClip(positionControl);
        //var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);
        //gr.TranslateTransform(trp.ControlX, trp.Y);
        //gr.RotateTransform(-Drehwinkel);

        using var _txt = new ExtText(SheetStyle, Style) {
            HtmlText = content,
            //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
            //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
            //etxt.DrawingArea = new Rectangle((int)CanvasUsedArea().Left, (int)CanvasUsedArea().Top, (int)(CanvasUsedArea().Width / AdditionalScale / SheetStyleScale), -1);
            //etxt.LineBreakWidth = etxt.DrawingArea.Width;
            TextDimensions = new Size(drawingAreaControl.Width, -1),
            Ausrichtung = align,
            AreaControl = drawingAreaControl,
        };

        _txt.Draw(gr, zoom, drawingAreaControl.Left, drawingAreaControl.Top);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        return result;
    }

    public override string ReadableText() => "Formatierter Text";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Sonne);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        using var _etxt = new ExtText(SheetStyle, Style) {
            HtmlText = content,
            Multiline = true
        };

        _etxt.TextDimensions = new Size(400, -1);

        return _etxt.LastSize();
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) => content;

    #endregion
}