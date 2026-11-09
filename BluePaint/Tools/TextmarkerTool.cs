// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.EventArgs;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BluePaint;

public partial class TextmarkerTool {

    #region Fields

    /// <summary>
    /// Transparenz des Textmarkers, damit der markierte Inhalt lesbar bleibt.
    /// </summary>
    private const int MarkerAlpha = 110;

    private static readonly Color ColorGelb = Color.FromArgb(255, 232, 64);
    private static readonly Color ColorGruen = Color.FromArgb(140, 214, 80);
    private static readonly Color ColorCyan = Color.FromArgb(80, 216, 240);
    private static readonly Color ColorBlau = Color.FromArgb(100, 160, 255);
    private static readonly Color ColorRot = Color.FromArgb(255, 100, 100);
    private static readonly Color ColorMagenta = Color.FromArgb(240, 100, 240);
    private static readonly Color ColorOrange = Color.FromArgb(255, 170, 60);
    private static readonly Color ColorViolett = Color.FromArgb(170, 110, 255);
    private static readonly Color ColorBraun = Color.FromArgb(180, 130, 80);
    private static readonly Color ColorSchwarz = Color.FromArgb(60, 60, 60);
    private static readonly Color ColorWeiss = Color.FromArgb(255, 255, 255);

    private static readonly SolidBrush BrushGelb = new(Color.FromArgb(MarkerAlpha, ColorGelb));
    private static readonly SolidBrush BrushGruen = new(Color.FromArgb(MarkerAlpha, ColorGruen));
    private static readonly SolidBrush BrushCyan = new(Color.FromArgb(MarkerAlpha, ColorCyan));
    private static readonly SolidBrush BrushBlau = new(Color.FromArgb(MarkerAlpha, ColorBlau));
    private static readonly SolidBrush BrushRot = new(Color.FromArgb(MarkerAlpha, ColorRot));
    private static readonly SolidBrush BrushMagenta = new(Color.FromArgb(MarkerAlpha, ColorMagenta));
    private static readonly SolidBrush BrushOrange = new(Color.FromArgb(MarkerAlpha, ColorOrange));
    private static readonly SolidBrush BrushViolett = new(Color.FromArgb(MarkerAlpha, ColorViolett));
    private static readonly SolidBrush BrushBraun = new(Color.FromArgb(MarkerAlpha, ColorBraun));
    private static readonly SolidBrush BrushSchwarz = new(Color.FromArgb(MarkerAlpha, ColorSchwarz));
    private static readonly SolidBrush BrushWeiss = new(Color.FromArgb(MarkerAlpha, ColorWeiss));

    private readonly List<Point> _strokePoints = [];

    private SolidBrush _markerBrush = BrushGelb;

    private int _markerSize = 24;

    private bool _isDrawing;

    #endregion

    #region Constructors

    public TextmarkerTool() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent is null) { return; }

        if (_isDrawing) {
            // Laufenden Strich nur als Vorschau zeigen. Er wird erst beim Loslassen einmalig in das Bild gezeichnet,
            // dadurch bleibt die Deckkraft auch bei mehrfachem Überfahren gleich (wie im Snipping-Tool).
            StrokeVorschau(gr, zoom, offsetX, offsetY);
        } else {
            // Spitze des Textmarkers anzeigen
            DrawPixelExactCircle(gr, zoom, offsetX, offsetY, mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY, _markerSize / 2, _markerBrush);
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

        if (Control.ModifierKeys.HasFlag(Keys.Shift)) {
            // Mit Shift wird, wie im Snipping-Tool, eine gerade Linie vom Startpunkt gezogen
            if (_strokePoints.Count > 1) { _strokePoints.RemoveAt(_strokePoints.Count - 1); }
            _strokePoints.Add(p);
        } else if (_strokePoints[^1] != p) {
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

    public override string ReadableText() => "Textmarker";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get("Kreis|16||||FFE840");

    private void StrokeEinbrennen(Bitmap pic) {
        // Einmalig zeichnen: Der halbtransparente Strich liegt über dem Bild, der Inhalt bleibt lesbar
        if (_strokePoints.Count == 1) {
            var p = _strokePoints[0];
            pic.FillCircle(_markerBrush.Color, p.X, p.Y, _markerSize / 2);
            return;
        }

        using var g = Graphics.FromImage(pic);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = StrokePen(_markerSize);
        g.DrawLines(pen, [.. _strokePoints]);
    }

    private void StrokeVorschau(Graphics gr, float zoom, int offsetX, int offsetY) {
        if (_strokePoints.Count == 1) {
            DrawPixelExactCircle(gr, zoom, offsetX, offsetY, _strokePoints[0].X, _strokePoints[0].Y, _markerSize / 2, _markerBrush);
            return;
        }

        gr.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = StrokePen(_markerSize * zoom);

        var pts = new PointF[_strokePoints.Count];
        for (var i = 0; i < _strokePoints.Count; i++) {
            pts[i] = new PointF(_strokePoints[i].X.CanvasToControl(zoom, offsetX), _strokePoints[i].Y.CanvasToControl(zoom, offsetY));
        }

        gr.DrawLines(pen, pts);
    }

    private Pen StrokePen(float width) {
        var pen = new Pen(_markerBrush.Color, width) {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        return pen;
    }

    private void Farbe_CheckedChanged(object sender, EventArgs e) {
        if (sender == optGelb && optGelb.Checked) {
            _markerBrush = BrushGelb;
        } else if (sender == optGruen && optGruen.Checked) {
            _markerBrush = BrushGruen;
        } else if (sender == optCyan && optCyan.Checked) {
            _markerBrush = BrushCyan;
        } else if (sender == optBlau && optBlau.Checked) {
            _markerBrush = BrushBlau;
        } else if (sender == optRot && optRot.Checked) {
            _markerBrush = BrushRot;
        } else if (sender == optMagenta && optMagenta.Checked) {
            _markerBrush = BrushMagenta;
        } else if (sender == optOrange && optOrange.Checked) {
            _markerBrush = BrushOrange;
        } else if (sender == optViolett && optViolett.Checked) {
            _markerBrush = BrushViolett;
        } else if (sender == optBraun && optBraun.Checked) {
            _markerBrush = BrushBraun;
        } else if (sender == optSchwarz && optSchwarz.Checked) {
            _markerBrush = BrushSchwarz;
        } else if (sender == optWeiss && optWeiss.Checked) {
            _markerBrush = BrushWeiss;
        } else {
            return;
        }

        OnDoInvalidate();
    }

    private void sldSize_ValueChanged(object sender, EventArgs e) {
        _markerSize = (int)Math.Round(sldSize.Value, MidpointRounding.AwayFromZero);
        capSize.Text = _markerSize.ToString1();
        OnDoInvalidate();
    }

    #endregion
}
