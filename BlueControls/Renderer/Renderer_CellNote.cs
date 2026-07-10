// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;
using BlueControls.Extended_Text;

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

    // Alle Maße in Canvas-Pixeln.
    private const int Padding = 3;

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

        var pad = Padding.CanvasToControl(zoom);
        var gap = BoxGap.CanvasToControl(zoom);
        var innerWidthControl = Math.Max(1, drawingAreaControl.Width - 2 * pad);
        var innerWidthCanvas = Math.Max(1, (int)(innerWidthControl / zoom));

        var y = drawingAreaControl.Top;

        foreach (var (keyName, symbol, text) in entries) {
            var noteDesign = NoteEntry.DesignFor(symbol);

            using var extText = new ExtText(noteDesign, States.Standard) {
                TextDimensions = new Size(innerWidthCanvas, 0),
                HtmlText = BuildNoteHtml(table, keyName, symbol, text)
            };

            var boxHeight = (int)(extText.HeightControl * zoom) + 2 * pad;

            if (y + boxHeight > drawingAreaControl.Bottom) {
                // Letzter Kasten wird nur gezeichnet, wenn zumindest eine Zeile Platz hat
                var minLineH = (int)Math.Ceiling(extText.BaseFont.MeasureString("X").Height * zoom);
                if (y + pad + minLineH + pad > drawingAreaControl.Bottom) { break; }
                boxHeight = drawingAreaControl.Bottom - y;
            }

            var box = new Rectangle(drawingAreaControl.Left, y, drawingAreaControl.Width, boxHeight);

            Skin.Draw_Back(gr, noteDesign, States.Standard, box, null, false);

            extText.AreaControl = new Rectangle(box.Left + pad, box.Top + pad, innerWidthControl, boxHeight - 2 * pad);
            extText.Draw(gr, zoom, box.Left + pad, box.Top + pad);

            Skin.Draw_Border(gr, noteDesign, States.Standard, box);

            y += boxHeight + gap;
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override string ReadableText() => "Zellnotiz";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stift);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var entries = CellNoteHelper.ParseAllNotes(content);
        if (entries.Count == 0) { return new(16, 16); }

        var totalH = 0;

        for (var i = 0; i < entries.Count; i++) {
            var noteDesign = NoteEntry.DesignFor(entries[i].Symbol);

            using var extText = new ExtText(noteDesign, States.Standard) {
                TextDimensions = new Size(AssumedBodyWidth, 0),
                HtmlText = BuildNoteHtml(null, entries[i].KeyName, entries[i].Symbol, entries[i].Text)
            };

            totalH += 2 * Padding + extText.HeightControl;
            if (i < entries.Count - 1) { totalH += BoxGap; }
        }

        return new(100, Math.Max(totalH, 16));
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        var entries = CellNoteHelper.ParseAllNotes(content);
        return entries.Count == 0 ? string.Empty : string.Join("\r", entries.Select(e => $"{e.Symbol}: {e.Text}"));
    }

    private static string BuildNoteHtml(Table? table, string keyName, NoteSymbols symbol, string text) {
        var caption = ResolveCaption(table, keyName);
        var imgCode = NoteEntry.ImageCodeFor(symbol);
        var body = text.Replace("\r\n", "<br>").Replace("\n", "<br>");
        return $"<Imagecode={imgCode}><b>{caption}</b><br>{body}";
    }

    private static string ResolveCaption(Table? table, string keyName) {
        if (table is { IsDisposed: false } && table.Column[keyName] is { IsDisposed: false } col && !string.IsNullOrEmpty(col.Caption)) {
            return col.Caption.Replace("\r", " ");
        }
        return keyName;
    }

    #endregion
}