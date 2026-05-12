// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.ItemCollectionPad;
using BlueTable;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.CellRenderer;

public class Renderer_DynamicSymbol : Renderer_Abstract {

    #region Fields

    public static readonly Renderer_DynamicSymbol Method = new();

    #endregion

    #region Properties

    public static string ClassId => "DynamicSymbol";

    public override string Description => "Kann spezielle Skripte als Symbol rendern";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (string.IsNullOrEmpty(content)) { return; }

        if (drawingAreaControl is { Width: > 4, Height: > 4 }) {
            using var bmp = new Bitmap(drawingAreaControl.Width, drawingAreaControl.Height);

            var ok = DynamicSymbolPadItem.ExecuteScript(content, string.Empty, bmp);

            if (ok.Failed) {
                using var gr2 = Graphics.FromImage(bmp);
                gr2.DrawImage(QuickImage.Get(ImageCode.Warnung, 16), 0, 0);
            }

            gr.DrawImage(bmp, drawingAreaControl.X, drawingAreaControl.Y);
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        //result.ParseableAdd("ShowPic", _bild_anzeigen);
        //result.ParseableAdd("ShowText", _text_anzeigen);
        //result.ParseableAdd("ShowCheckState", _checkstatus_anzeigen);
        return result;
    }

    public override bool ParseThis(string key, string value) => base.ParseThis(key, value);

    public override string ReadableText() => "Skript als Symbol anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stern);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) => new(48, 48);

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