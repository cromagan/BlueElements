// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;

namespace BlueControls.Renderer;

public class Renderer_CellNote : Renderer_Abstract {

    #region Fields

    /// <summary>
    /// Angenommene Textbreite in Canvas-Pixeln, die nur für die Höhenberechnung
    /// (CalculateContentSize) verwendet wird, da dort die tatsächliche Spaltenbreite
    /// nicht zur Verfügung steht. Bewusst schmal gewählt, damit Notizen mit langem
    /// Text ausreichend Höhe reserviert bekommen.
    /// </summary>
    private const int AssumedBodyWidth = 120;

    private const int BoxGap = 4;

    private const int HeaderToTextGap = 2;

    private const int Padding = 3;

    // Alle Maße in Canvas-Pixeln.
    private const int SymbolSize = 16;

    #endregion

    #region Properties

    public static string ClassId => "CellNote";

    public override string Description => "Stellt alle Zellnotizen dieser Zeile dar.\r\nJede Notiz erscheint als eigener Kasten mit Symbol, Spaltenname und Text.";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        var entries = CellNoteHelper.ParseAllNotes(content);
        if (entries.Count == 0) { return; }

        var table = affectingRow is { IsDisposed: false } r && r.Table is { IsDisposed: false } t ? t : null;

        var font = GetFont(zoom, design, state);
        var headerFont = BoldVariant(font);

        var pad = Padding.CanvasToControl(zoom);
        var gap = BoxGap.CanvasToControl(zoom);
        var hgt = HeaderToTextGap.CanvasToControl(zoom);
        var lineH = (int)Math.Ceiling(font.MeasureString("X").Height);
        var headerH = Math.Max(SymbolSize.CanvasToControl(zoom), (int)Math.Ceiling(headerFont.MeasureString("X").Height));

        var innerWidth = drawingAreaControl.Width - 2 * pad;
        if (innerWidth < 1) { innerWidth = 1; }

        var y = drawingAreaControl.Top;

        foreach (var (keyName, symbol, text) in entries) {
            var bodyLines = WrapBody(font, text, innerWidth);
            var boxHeight = pad + headerH + hgt + (bodyLines.Count * lineH) + pad;

            if (y + boxHeight > drawingAreaControl.Bottom) {
                // Letzter Kasten wird nur gezeichnet, wenn zumindest Kopfzeile + eine Textzeile Platz haben
                if (y + pad + headerH + hgt + lineH + pad > drawingAreaControl.Bottom) { break; }
                boxHeight = drawingAreaControl.Bottom - y;
            }

            var box = new Rectangle(drawingAreaControl.Left, y, drawingAreaControl.Width, boxHeight);

            gr.FillRectangle(BlueFont.GetBrush(NoteEntry.GetBackColor(symbol)), box);
            gr.DrawRectangle(NoteEntry.PenForSymbol(symbol), box);

            var headerRect = new Rectangle(box.Left + pad, box.Top + pad, box.Width - 2 * pad, headerH);
            var qi = NoteEntry.GetQuickImage(symbol, SymbolSize)?.Scale(zoom);
            var caption = ResolveCaption(table, keyName);
            Skin.Draw_FormatedText(gr, caption, qi, Alignment.VerticalCenter_Left, headerRect, headerFont, false);

            var bodyY = headerRect.Bottom + hgt;
            foreach (var line in bodyLines) {
                if (bodyY + lineH > box.Bottom) { break; }
                font.DrawString(gr, line, box.Left + pad, bodyY);
                bodyY += lineH;
            }

            y += boxHeight + gap;
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override string ReadableText() => "Zellnotiz";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stift);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var entries = CellNoteHelper.ParseAllNotes(content);
        if (entries.Count == 0) { return new(16, 16); }

        var font = GetFont();
        var headerFont = BoldVariant(font);

        var lineH = (int)Math.Ceiling(font.MeasureString("X").Height);
        var headerH = Math.Max(SymbolSize, (int)Math.Ceiling(headerFont.MeasureString("X").Height));

        var totalH = 0;
        for (var i = 0; i < entries.Count; i++) {
            var bodyLines = WrapBody(font, entries[i].Text, AssumedBodyWidth).Count;
            totalH += Padding + headerH + HeaderToTextGap + (bodyLines * lineH) + Padding;
            if (i < entries.Count - 1) { totalH += BoxGap; }
        }

        return new(100, Math.Max(totalH, 16));
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        var entries = CellNoteHelper.ParseAllNotes(content);
        return entries.Count == 0 ? string.Empty : string.Join("\r", entries.Select(e => $"{e.Symbol}: {e.Text}"));
    }

    private static BlueFont BoldVariant(BlueFont font) => BlueFont.Get(font.FontName, font.Size, true, font.Italic, font.Underline, font.StrikeOut, font.ColorMain, font.ColorOutline, font.ColorBack);

    private static string ResolveCaption(Table? table, string keyName) {
        if (table is { IsDisposed: false } && table.Column[keyName] is { IsDisposed: false } col && !string.IsNullOrEmpty(col.Caption)) {
            return col.Caption.Replace("\r", " ");
        }
        return keyName;
    }

    private static List<string> WrapBody(BlueFont font, string text, float maxWidth) {
        if (string.IsNullOrEmpty(text)) { return [string.Empty]; }
        if (maxWidth <= 5) { return [text]; }
        return BlueFont.SplitByWidth(font, text, maxWidth, 100);
    }

    #endregion
}