// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.EventArgs;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BluePaint;

/// <summary>
/// Platziert einen Text mit Farbe, Größe und Stil per Mausklick fix im Bild; der Vorschautext folgt der Maus.
/// </summary>
public partial class TextTool {

    #region Fields

    private static readonly Color ColorSchwarz = Color.FromArgb(0, 0, 0);
    private static readonly Color ColorWeiss = Color.FromArgb(255, 255, 255);
    private static readonly Color ColorRot = Color.FromArgb(255, 0, 0);
    private static readonly Color ColorGelb = Color.FromArgb(255, 232, 64);
    private static readonly Color ColorGruen = Color.FromArgb(140, 214, 80);
    private static readonly Color ColorCyan = Color.FromArgb(80, 216, 240);
    private static readonly Color ColorBlau = Color.FromArgb(100, 160, 255);

    private static readonly Pen PenAuswahl = new(Color.FromArgb(160, 120, 120, 120)) { DashStyle = DashStyle.Dash };

    private Color _color = ColorSchwarz;

    private bool _italic;

    private bool _bold;

    private int _size = 32;

    private string _text = string.Empty;

    #endregion

    #region Constructors

    public TextTool() : base() => InitializeComponent();

    #endregion

    #region Methods

    /// <summary>
    /// Zeichnet den Text als Vorschau mit Auswahl-Rahmen; er folgt der Maus.
    /// </summary>
    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (_text is not { Length: > 0 } t) { return; }

        var px = MausPosition(mouseCurrent);
        if (px is null) { return; }

        using var font = CurrentFont(_size * zoom);
        var sizeF = gr.MeasureString(t, font);
        var left = px.Value.X.CanvasToControl(zoom, offsetX);
        var top = px.Value.Y.CanvasToControl(zoom, offsetY);

        gr.SmoothingMode = SmoothingMode.AntiAlias;
        gr.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var brush = new SolidBrush(_color);
        gr.DrawString(t, font, brush, left, top);

        gr.DrawRectangle(PenAuswahl, left - 4, top - 4, sizeF.Width + 8, sizeF.Height + 8);
    }

    /// <summary>
    /// Brennt den Text an der geklickten Stelle fix ins Bild ein.
    /// </summary>
    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        if (e.Button != MouseButtons.Left) { return; }
        if (_text is not { Length: > 0 }) { return; }
        if (OnNeedCurrentPic() is not { } pic) { return; }

        OnForceUndoSaving();

        var pos = new Point(e.TrimmedCanvasX, e.TrimmedCanvasY);
        using var font = CurrentFont(_size);
        using var g = Graphics.FromImage(pic);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        using var brush = new SolidBrush(_color);
        g.DrawString(_text, font, brush, pos);

        OnDoInvalidate();
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) => OnDoInvalidate();

    public override string ReadableText() => "Text";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld, 16);

    private Font CurrentFont(float pixelSize) => new("Arial", pixelSize, Stil(), GraphicsUnit.Pixel);

    private void Farbe_CheckedChanged(object sender, EventArgs e) {
        if (sender == optSchwarz && optSchwarz.Checked) {
            _color = ColorSchwarz;
        } else if (sender == optWeiss && optWeiss.Checked) {
            _color = ColorWeiss;
        } else if (sender == optRot && optRot.Checked) {
            _color = ColorRot;
        } else if (sender == optGelb && optGelb.Checked) {
            _color = ColorGelb;
        } else if (sender == optGruen && optGruen.Checked) {
            _color = ColorGruen;
        } else if (sender == optCyan && optCyan.Checked) {
            _color = ColorCyan;
        } else if (sender == optBlau && optBlau.Checked) {
            _color = ColorBlau;
        } else {
            return;
        }

        OnDoInvalidate();
    }

    /// <summary>
    /// Liefert die Mausposition als Bildpunkt, sofern der Zeiger über dem Bild steht.
    /// </summary>
    private static Point? MausPosition(TrimmedCanvasMouseEventArgs? mouseCurrent) => mouseCurrent is { IsInPic: true } mc ? new Point(mc.TrimmedCanvasX, mc.TrimmedCanvasY) : null;

    private void SldSize_ValueChanged(object sender, EventArgs e) {
        _size = (int)Math.Round(sldSize.Value, MidpointRounding.AwayFromZero);
        capSize.Text = _size.ToString1();
        OnDoInvalidate();
    }

    private FontStyle Stil() {
        var s = FontStyle.Regular;
        if (_bold) { s |= FontStyle.Bold; }
        if (_italic) { s |= FontStyle.Italic; }
        return s;
    }

    private void Stil_CheckedChanged(object sender, EventArgs e) {
        _bold = chkFett.Checked;
        _italic = chkKursiv.Checked;
        OnDoInvalidate();
    }

    private void TxbText_TextChanged(object sender, EventArgs e) {
        _text = txbText.Text;
        OnDoInvalidate();
    }

    #endregion
}
