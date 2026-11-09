// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.EventArgs;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BluePaint;

public partial class PenTool {

    #region Fields

    private static readonly Color ColorSchwarz = Color.FromArgb(0, 0, 0);
    private static readonly Color ColorBlau = Color.FromArgb(0, 120, 215);
    private static readonly Color ColorRot = Color.FromArgb(255, 0, 0);
    private static readonly Color ColorGruen = Color.FromArgb(0, 128, 0);
    private static readonly Color ColorWeiss = Color.FromArgb(255, 255, 255);

    private static readonly SolidBrush BrushSchwarz = new(ColorSchwarz);
    private static readonly SolidBrush BrushBlau = new(ColorBlau);
    private static readonly SolidBrush BrushRot = new(ColorRot);
    private static readonly SolidBrush BrushGruen = new(ColorGruen);
    private static readonly SolidBrush BrushWeiss = new(ColorWeiss);

    private readonly List<Point> _strokePoints = [];

    private SolidBrush _penBrush = BrushSchwarz;

    private int _brushSize = 3;

    private bool _isDrawing;

    #endregion

    #region Constructors

    public PenTool() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent is null) { return; }

        if (_isDrawing) {
            StrokeVorschau(gr, zoom, offsetX, offsetY);
        } else {
            DrawPixelExactCircle(gr, zoom, offsetX, offsetY, mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY, _brushSize - 1, _penBrush);
        }
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        if (e.Button != MouseButtons.Left) { return; }

        OnForceUndoSaving();
        _strokePoints.Clear();
        _strokePoints.Add(new Point(e.TrimmedCanvasX, e.TrimmedCanvasY));
        _isDrawing = true;
        OnDoInvalidate();
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (!_isDrawing) {
            OnDoInvalidate();
            return;
        }

        if (e.MouseCurrent.Button != MouseButtons.Left) {
            // Strich verwerfen, z.B. wenn der Button zwischenzeitlich außerhalb des Bildes losgelassen wurde
            _isDrawing = false;
            _strokePoints.Clear();
            OnDoInvalidate();
            return;
        }

        var p = new Point(e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY);

        if (_strokePoints[^1] != p) {
            _strokePoints.Add(p);
        }

        OnDoInvalidate();
    }

    public override void MouseUp(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (!_isDrawing) { return; }

        _isDrawing = false;

        var pic = OnNeedCurrentPic();
        if (pic is null) {
            _strokePoints.Clear();
            OnDoInvalidate();
            return;
        }

        StrokeEinbrennen(pic);

        _strokePoints.Clear();
        OnDoInvalidate();
    }

    public override void OnToolChanging() {
        _isDrawing = false;
        _strokePoints.Clear();
    }

    public override string ReadableText() => "Zeichnen";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Stift, 16);

    private void StrokeEinbrennen(Bitmap pic) {
        if (_strokePoints.Count == 1) {
            var p = _strokePoints[0];
            pic.FillCircle(_penBrush.Color, p.X, p.Y, _brushSize - 1);
            return;
        }

        using var g = Graphics.FromImage(pic);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        // Strichbreite = Durchmesser des Spitzen-Kreises, damit Punkt und Linie gleich dick sind
        using var pen = StrokePen(_brushSize * 2 - 1);
        g.DrawLines(pen, [.. _strokePoints]);
    }

    private void StrokeVorschau(Graphics gr, float zoom, int offsetX, int offsetY) {
        if (_strokePoints.Count == 1) {
            DrawPixelExactCircle(gr, zoom, offsetX, offsetY, _strokePoints[0].X, _strokePoints[0].Y, _brushSize - 1, _penBrush);
            return;
        }

        gr.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = StrokePen((_brushSize * 2 - 1) * zoom);

        var pts = new PointF[_strokePoints.Count];
        for (var i = 0; i < _strokePoints.Count; i++) {
            pts[i] = new PointF(_strokePoints[i].X.CanvasToControl(zoom, offsetX), _strokePoints[i].Y.CanvasToControl(zoom, offsetY));
        }

        gr.DrawLines(pen, pts);
    }

    private Pen StrokePen(float width) {
        var pen = new Pen(_penBrush.Color, width) {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        return pen;
    }

    private void Farbe_CheckedChanged(object sender, EventArgs e) {
        if (sender == optSchwarz && optSchwarz.Checked) {
            _penBrush = BrushSchwarz;
        } else if (sender == optBlau && optBlau.Checked) {
            _penBrush = BrushBlau;
        } else if (sender == optRot && optRot.Checked) {
            _penBrush = BrushRot;
        } else if (sender == optGruen && optGruen.Checked) {
            _penBrush = BrushGruen;
        } else if (sender == optWeiss && optWeiss.Checked) {
            _penBrush = BrushWeiss;
        } else {
            return;
        }

        OnDoInvalidate();
    }

    private void sldSize_ValueChanged(object sender, EventArgs e) {
        _brushSize = (int)Math.Round(sldSize.Value, MidpointRounding.AwayFromZero);
        capSize.Text = _brushSize.ToString1();
        OnDoInvalidate();
    }

    #endregion
}
