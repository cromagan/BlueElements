// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Controls;

namespace BlueControls.Renderer;

public class Renderer_Layout : Renderer_Abstract {

    #region Fields

    private static readonly ConcurrentCache<string, Bitmap> _bitmapCache = new(StringComparer.OrdinalIgnoreCase, 50);
    private string _file = string.Empty;

    #endregion

    #region Properties

    public static string ClassId => "Layout";

    public override string Description => "Langsame, aber schöne Anzeige eines Layoutes";

    public string File {
        get => _file;
        set {
            if (_file == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _file = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle drawingAreaControl, TranslationType translate, Alignment align, float zoom, Design design, States state) {
        if (affectingRow == null) { return; }

        try {
            using var l = new ItemCollectionPadItem(_file);
            l.GridShow = 0;

            if (!l.Any()) {
                var replacedText = ValueReadable("Layout nicht gefunden oder fehlerhaft.", ShortenStyle.Replaced, translate);

                Skin.Draw_FormatedText(gr, replacedText, null, align, drawingAreaControl, GetFont(zoom, design, state), false);
                return;
            }

            var rowHash = affectingRow.RowStamp() + l.ParseableItems().FinishParseable().GetMD5Hash();

            Bitmap cachedBmp;
            if (!_bitmapCache.TryGetValue(rowHash, out cachedBmp)) {
                l.ResetVariables();
                var scx = l.ReplaceVariables(affectingRow);

                if (scx.Failed) {
                    var replacedText = ValueReadable("Layout Generierung fehlgeschlagen.", ShortenStyle.Replaced, translate);
                    Skin.Draw_FormatedText(gr, replacedText, null, align, drawingAreaControl, GetFont(zoom, design, state), false);
                    return;
                }

                var bmp = l.ToBitmap(zoom);
                if (bmp == null) {
                    var replacedText = ValueReadable("Bild Erstellung fehlgeschlagen.", ShortenStyle.Replaced, translate);
                    Skin.Draw_FormatedText(gr, replacedText, null, align, drawingAreaControl, GetFont(zoom, design, state), false);
                    return;
                }

                _bitmapCache.TryAdd(rowHash, bmp);
                cachedBmp = bmp;
            }

            var scale2 = Math.Min((float)drawingAreaControl.Width / cachedBmp.Width, (float)drawingAreaControl.Height / cachedBmp.Height);
            gr.DrawImage(cachedBmp, new Rectangle(drawingAreaControl.X + (drawingAreaControl.Width - cachedBmp.Width.CanvasToControl(scale2)) / 2, drawingAreaControl.Y + (drawingAreaControl.Height - cachedBmp.Height.CanvasToControl(scale2)) / 2, cachedBmp.Width.CanvasToControl(scale2), cachedBmp.Height.CanvasToControl(scale2)));
        } catch {
            var replacedText = ValueReadable("Anzeige fehlgeschlagen.", ShortenStyle.Replaced, translate);
            Skin.Draw_FormatedText(gr, replacedText, null, align, drawingAreaControl, GetFont(zoom, design, state), false);
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControlForProperty<string>(() => File)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("LayoutFile", _file);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "layoutfile":
                _file = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Layout-Anzeige";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Layout);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        using var l = new ItemCollectionPadItem(_file);

        if (!l.Any()) {
            return new Size(16, 16);
        }

        return new Size((int)l.CanvasUsedArea.Width, (int)l.CanvasUsedArea.Height);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) => LanguageTool.PrepaireText(content, style, string.Empty, string.Empty, doOpticalTranslation, null);

    #endregion
}