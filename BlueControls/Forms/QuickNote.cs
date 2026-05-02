// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Forms;

public sealed class QuickNote : FloatingForm {

    #region Fields

    private const int BaseCharCount = 12;
    private const double BaseDurationMs = 1200;
    private const double ExtraMsPerChar = 80;
    private const double FloatUpPixels = 30;
    private const double StillDuration = 500;
    private readonly Color _backColor;
    private readonly double _durationMs;
    private readonly BlueFont _font;
    private readonly QuickImage? _image;
    private readonly Pen _pen;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private readonly double _startTop;
    private readonly string _text;
    private double _floatOffset;
    private System.Threading.Timer? _timer;

    #endregion

    #region Constructors

    private QuickNote(NoteSymbols symbol, string text, int x, int y) : base(Design.Form_Notification) {
        DismissMode = DismissMode.ManualOnly;

        _text = text;
        _pen = new Pen(NoteEntry.GetTextColor(symbol));
        _backColor = NoteEntry.GetBackColor(symbol);
        _image = NoteEntry.GetQuickImage(symbol, 14);
        _font = BlueFont.Get("Arial", 9, false, false, false, false, NoteEntry.GetTextColor(symbol), Color.Transparent, Color.Transparent);
        _durationMs = BaseDurationMs + Math.Max(0, text.Length - BaseCharCount) * ExtraMsPerChar;
        _startTop = y;

        _floatOffset = 0;

        var textSize = _font.MeasureString(text);

        var imageWidth = _image?.Width ?? 0;
        var totalWidth = (int)(imageWidth + Skin.PaddingSmal + textSize.Width + Skin.PaddingSmal * 3);
        var totalHeight = (int)Math.Max(imageWidth, textSize.Height) + Skin.PaddingSmal * 2;

        Size = new Size(totalWidth, totalHeight);
        FormBorderStyle = FormBorderStyle.None;

        Left = x;
        Top = y;

        Opacity = 1;
        Show();

        _timer = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(Timer_Tick)); }
        }, null, 10, 10);
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

    protected override void OnPaint(PaintEventArgs? e) {
        if (IsClosed || IsDisposed) { return; }
        if (e?.Graphics == null) { return; }

        var gr = e.Graphics;
        gr.SmoothingMode = SmoothingMode.HighQuality;

        var r = new Rectangle(0, 0, Width - 1, Height - 1);

        using var backBrush = new SolidBrush(_backColor);
        gr.FillRectangle(backBrush, r);

        using (var borderPen = new Pen(_pen.Color, _pen.Width)) {
            gr.DrawRectangle(borderPen, r);
        }

        var imageWidth = _image?.Width ?? 0;
        var imageHeight = _image?.Height ?? 0;
        var textX = Skin.PaddingSmal + imageWidth + Skin.PaddingSmal;
        var textY = Skin.PaddingSmal;

        if (_image != null) {
            var imgY = Skin.PaddingSmal + ((Height - Skin.PaddingSmal * 2 - imageHeight) / 2);
            gr.DrawImage((Bitmap)_image, Skin.PaddingSmal, imgY, imageWidth, imageHeight);
        }

        _font.DrawString(gr, _text, textX, textY);
    }

    private void Timer_Tick() {
        var elapsed = DateTime.UtcNow.Subtract(_startTime).TotalMilliseconds;

        if (elapsed >= _durationMs) {
            try {
                _timer?.Dispose();
                Visible = false;
                Close();
                if (!IsDisposed) { Dispose(); }
            } catch { }
            return;
        }

        if (elapsed < StillDuration) { return; }

        var animElapsed = elapsed - StillDuration;
        var animDuration = _durationMs - StillDuration;
        var progress = animElapsed / animDuration;
        Opacity = 1 - progress;
        _floatOffset += FloatUpPixels * (10.0f / (float)(_durationMs - StillDuration));
        Top = (int)(_startTop - (int)_floatOffset);
    }

    #endregion
}