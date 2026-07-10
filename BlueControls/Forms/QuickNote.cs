// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Forms;

public sealed class QuickNote : FloatingForm, IAnimatable {

    #region Fields

    private const int BaseCharCount = 12;
    private const double BaseDurationMs = 1200;
    private const double ExtraMsPerChar = 80;
    private const double FloatUpDurationMs = 700;
    private const double FloatUpPixels = 30;

    private readonly Design _design;
    private readonly double _durationMs;
    private readonly QuickImage? _image;
    private readonly int _startTop;
    private readonly int _startX;
    private readonly string _text;

    #endregion

    #region Constructors

    private QuickNote(NoteSymbols symbol, string text, int x, int y) : base(NoteEntry.DesignFor(symbol)) {
        DismissMode = DismissMode.ManualOnly;

        _text = text;
        _design = NoteEntry.DesignFor(symbol);
        _image = NoteEntry.GetQuickImage(symbol, 14);

        var font = Skin.GetBlueFont(_design, States.Standard);
        _durationMs = BaseDurationMs + Math.Max(0, text.Length - BaseCharCount) * ExtraMsPerChar;

        var textSize = font.MeasureString(text);

        var imageWidth = _image?.Width ?? 0;
        var totalWidth = (int)(imageWidth + Skin.PaddingSmal + textSize.Width + Skin.PaddingSmal * 3);
        var totalHeight = (int)Math.Max(imageWidth, textSize.Height) + Skin.PaddingSmal * 2;

        Size = new Size(totalWidth, totalHeight);
        FormBorderStyle = FormBorderStyle.None;

        // Position so setzen, dass die komplette QuickNote innerhalb des
        // sichtbaren Bildschirmbereichs liegt - weder außerhalb startend noch
        // über den Rand hinausragend.
        Position_SetWindowIntoScreen(Generic.PointOnScreenNr(new Point(x, y)), x, y);
        _startX = Left;
        _startTop = Top;

        // Engine übernimmt Opacity + Position komplett am UI-Thread vorbei.
        // Initial unsichtbar, das Fade-In passiert durch die Animate-Routine.
        Opacity = 0;
        Show();

        ((IAnimatable)this).StartAnimation();
    }

    #endregion

    #region Methods

    public static void Show(NoteSymbols symbol, string text, Control control) {
        if (control is null || control.IsDisposed) { return; }
        var p = control.PointToScreen(new Point(control.Width + 5, 0));
        Show(symbol, text, p.X, p.Y);
    }

    public static void Show(NoteSymbols symbol, string text, int x, int y) {
        if (string.IsNullOrWhiteSpace(text)) { return; }

        var words = text.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 3) {
            Develop.DebugPrint($"MouseNotification erlaubt maximal drei Wörter: {text}" + text);
            return;
        }

        if (text.Contains('{') || text.Contains('}')) {
            Develop.DebugPrint("MouseNotification erlaubt keine Platzhalter '{}': {text}" + text);
            return;
        }

        _ = new QuickNote(symbol, text, x, y);
    }

    public static void Show(NoteSymbols symbol, string text) => Show(symbol, text, Cursor.Position.X + 15, Cursor.Position.Y + 15);

    /// <summary>
    /// Zeitbasierte Berechnung eines Animations-Frames. Position und Opacity
    /// werden direkt aus der verstrichenen Zeit hergeleitet — nicht akkumuliert —
    /// damit die Animation auch bei Lastspitzen smooth und deterministisch bleibt.
    /// Phase 1 (Still): voll sichtbar, steht — Lesezeit, länger bei langen Texten.
    /// Phase 2 (Fade-out + Float-up): beide parallel über FloatUpDurationMs,
    /// unabhängig von der Textlänge. So bleibt die Bewegung immer gleich schnell.
    /// </summary>
    public AnimationFrame Animate(TimeSpan elapsed) {
        var ms = elapsed.TotalMilliseconds;
        var stillDuration = _durationMs - FloatUpDurationMs;

        if (ms < stillDuration) {
            return new AnimationFrame { Opacity = 1, X = _startX, Y = _startTop };
        }

        if (ms >= _durationMs) {
            return new AnimationFrame {
                Opacity = 0,
                X = _startX,
                Y = (int)(_startTop - FloatUpPixels),
                Finished = true
            };
        }

        // Fade-out und Float-up parallel über eine konstante Dauer — so sind
        // beide synchron und die Bewegungsgeschwindigkeit ist unabhängig von
        // der Textlänge.
        var progress = (ms - stillDuration) / FloatUpDurationMs;
        return new AnimationFrame {
            Opacity = 1 - progress,
            X = _startX,
            Y = (int)(_startTop - FloatUpPixels * progress)
        };
    }

    protected override void OnPaint(PaintEventArgs? e) {
        if (IsClosed || IsDisposed) { return; }
        if (e?.Graphics is null) { return; }

        var gr = e.Graphics;
        gr.SmoothingMode = SmoothingMode.HighQuality;

        var r = new Rectangle(0, 0, Width, Height);

        Skin.Draw_Back(gr, _design, States.Standard, r, this, false);

        var imageWidth = _image?.Width ?? 0;
        var imageHeight = _image?.Height ?? 0;
        var textX = Skin.PaddingSmal + imageWidth + Skin.PaddingSmal;
        var textY = Skin.PaddingSmal;

        if (_image is not null) {
            var imgY = Skin.PaddingSmal + ((Height - Skin.PaddingSmal * 2 - imageHeight) / 2);
            gr.DrawImage((Bitmap)_image, Skin.PaddingSmal, imgY, imageWidth, imageHeight);
        }

        var font = Skin.GetBlueFont(_design, States.Standard);
        font.DrawString(gr, _text, textX, textY);

        Skin.Draw_Border(gr, _design, States.Standard, r);
    }

    #endregion
}