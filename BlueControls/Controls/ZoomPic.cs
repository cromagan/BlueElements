// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.Extensions;

using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPic : ZoomPad {

    #region Fields

    protected Helpers _helper = Helpers.None;
    protected Orientation _mittelLinie = Orientation.Ohne;
    private const int DrawSize = 20;
    private static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
    private TrimmedCanvasMouseEventArgs? _mouseCurrent;
    private TrimmedCanvasMouseEventArgs _mouseDown = new TrimmedCanvasMouseEventArgs();

    #endregion

    #region Constructors

    public ZoomPic() : base() {
        InitializeComponent();
    }

    #endregion

    #region Events

    public event EventHandler<AdditionalDrawingEventArgs>? DoAdditionalDrawing;

    public event EventHandler<TrimmedCanvasMouseEventArgs>? ImageMouseDown;

    public event EventHandler<TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs>? ImageMouseMove;

    public event EventHandler<TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs>? ImageMouseUp;

    public event EventHandler<PositionEventArgs>? OverwriteMouseImageData;

    #endregion

    #region Properties

    //public Bitmap OverlayBmp = null;
    [DefaultValue(false)]
    public bool AlwaysSmooth { get; set; } = false;

    public Bitmap? Bmp {
        get;
        set {
            if (value == field) { return; }
            field = value;
            Invalidate();
        }
    }

    public override bool ControlMustPressedForZoomWithWheel => false;

    [DefaultValue(Helpers.None)]
    public Helpers Helper {
        get => _helper;
        set {
            if (_helper == value) { return; }
            _helper = value;
            Invalidate();
        }
    }

    [DefaultValue("")]
    public string InfoText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [DefaultValue((Orientation)(-1))]
    public Orientation Mittellinie {
        get => _mittelLinie;
        set {
            if (_mittelLinie == value) { return; }
            _mittelLinie = value;
            Invalidate();
        }
    }

    protected override bool ShowSliderX => true;
    protected override int SmallChangeY => 5;

    #endregion

    #region Methods

    protected override RectangleF CalculateCanvasMaxBounds() => Bmp?.IsValid() == true ? new RectangleF(-20, -20, Bmp.Width + 40, Bmp.Height + 40) : new RectangleF(0, 0, 0, 0);

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        // Get drawable area considering scrollbars
        var drawArea = AvailableControlPaintArea;

        // Create and draw gradient background
        using var lgb = new LinearGradientBrush(drawArea, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, drawArea);

        if (Bmp?.IsValid() == true) {
            // Calculate image rectangle considering scrollbars
            var imageRect = new RectangleF(0, 0, Bmp.Width, Bmp.Height).CanvasToControl(Zoom, OffsetX, OffsetY, true);

            // Clip to available area
            gr.SetClip(drawArea);

            if (Zoom < 1 || AlwaysSmooth) {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            } else {
                gr.SmoothingMode = SmoothingMode.HighSpeed;
                gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            }

            gr.PixelOffsetMode = PixelOffsetMode.Half;
            gr.DrawImage(Bmp, imageRect);
            gr.ResetClip();
        }

        OnDoAdditionalDrawing(new AdditionalDrawingEventArgs(gr, Zoom, OffsetX, OffsetY, _mouseDown, _mouseCurrent));

        Skin.Draw_Border(gr, Design.Table_And_Pad, state, drawArea);
    }

    protected virtual void OnDoAdditionalDrawing(AdditionalDrawingEventArgs e) {
        DoAdditionalDrawing?.Invoke(this, e);

        DrawHelpers(e);

        // Info Text
        if (!string.IsNullOrEmpty(InfoText)) {
            PrintInfoText(e);
        }

        // Magnifier
        if (_helper.HasFlag(Helpers.Magnifier) && Bmp != null && e.MouseCurrent != null) {
            Bmp.Magnify(e.MouseCurrent.CanvasPoint, e.Graphics, false);
        }
    }

    /// <summary>
    /// Zuerst ImageMouseUp, dann MouseUp
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnImageMouseUp(TrimmedCanvasMouseEventArgs e) {
        ImageMouseUp?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(_mouseDown, e));
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        _mouseDown = _mouseCurrent;
        OnImageMouseDown(_mouseDown);
        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseMove(_mouseCurrent);
        Invalidate();
    }

    /// <summary>
    /// Zuerst ImageMouseUp, dann MouseUp
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseUp(_mouseCurrent);
        base.OnMouseUp(e);
        //_MouseDown = null; Wenn die Leute immer beide Maustasten gleichzeitig klicken.
    }

    protected void OnOverwriteMouseImageData(PositionEventArgs e) => OverwriteMouseImageData?.Invoke(this, e);

    private void DrawHelpers(AdditionalDrawingEventArgs e) {
        if (Bmp?.IsValid() != true) { return; }

        var controlDrawArea = AvailableControlPaintArea;
        e.Graphics.SetClip(controlDrawArea);

        PositionEventArgs newCanvasCoords;
        if (e.MouseCurrent != null) {
            newCanvasCoords = new PositionEventArgs(e.MouseCurrent.CanvasX, e.MouseCurrent.CanvasY);
        } else {
            newCanvasCoords = new PositionEventArgs(0, 0);
        }
        OnOverwriteMouseImageData(newCanvasCoords);

        // Mittellinie
        var canvasPicturePos = CanvasMaxBounds;
        if (_mittelLinie.HasFlag(Orientation.Waagerecht)) {
            var p1 = canvasPicturePos.PointOf(Alignment.VerticalCenter_Left).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            var p2 = canvasPicturePos.PointOf(Alignment.VerticalCenter_Right).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
        }

        if (_mittelLinie.HasFlag(Orientation.Senkrecht)) {
            var p1 = canvasPicturePos.PointOf(Alignment.Top_HorizontalCenter).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            var p2 = canvasPicturePos.PointOf(Alignment.Bottom_HorizontalCenter).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
        }

        if (e.MouseCurrent == null) {
            e.Graphics.ResetClip();
            return;
        }

        if (_helper.HasFlag(Helpers.HorizontalLine)) {
            var p1 = new PointF(0, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            var p2 = new PointF(Bmp.Width, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawLine(PenRotTransp, p1, p2);
        }

        if (_helper.HasFlag(Helpers.VerticalLine)) {
            var p1 = new PointF(newCanvasCoords.X, 0).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            var p2 = new PointF(newCanvasCoords.X, Bmp.Height).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawLine(PenRotTransp, p1, p2);
        }

        if (_helper.HasFlag(Helpers.SymetricalHorizontal)) {
            var h = Bmp.Width / 2;
            var x = Math.Abs(h - newCanvasCoords.X);
            var p1 = new PointF(h - x, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            var p2 = new PointF(h + x, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawLine(PenRotTransp, p1, p2);
        }

        if (_helper.HasFlag(Helpers.MouseDownPoint)) {
            var m1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
            e.Graphics.DrawEllipse(PenRotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));
            if (e.MouseDown != null && MouseDownData != null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
                e.Graphics.DrawEllipse(PenRotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
                e.Graphics.DrawLine(PenRotTransp, mc1, md1);
            }
        }

        if (_helper.HasFlag(Helpers.FilledRectancle)) {
            if (e.MouseDown != null && MouseDownData != null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
                var r = new RectangleF(Math.Min(md1.X, newCanvasCoords.X), Math.Min(md1.Y, newCanvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                e.Graphics.FillRectangle(BrushRotTransp, r);
            }
        }

        // Rechteck zeichnen
        if (_helper.HasFlag(Helpers.DrawRectangle)) {
            if (e.MouseDown != null && MouseDownData != null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);
                var r = new RectangleF(Math.Min(md1.X, newCanvasCoords.X), Math.Min(md1.Y, newCanvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                e.Graphics.DrawRectangle(PenRotTransp, r.X, r.Y, r.Width, r.Height);
            }
        }

        // kleines Rechteck zeichnen
        if (_helper.HasFlag(Helpers.Draw20x10)) {
            if (e.MouseCurrent != null) {
                // Startpunkt des Rechtecks (oben links)
                var startPoint = new PointF(e.MouseCurrent.CanvasX - 10, e.MouseCurrent.CanvasY - 5);
                // Skaliere und verschiebe den Startpunkt
                var scaledStart = startPoint.CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);

                // Berechne die skalierten Dimensionen
                var scaledWidth = 20 * e.Zoom;  // 20 Pixel Breite
                var scaledHeight = 10 * e.Zoom;  // 10 Pixel Höhe

                // Zeichne das Rechteck mit den skalierten Werten
                e.Graphics.DrawRectangle(PenRotTransp,
                    scaledStart.X,
                    scaledStart.Y,
                    scaledWidth,
                    scaledHeight);
            }
        }

        e.Graphics.ResetClip();
    }

    private TrimmedCanvasMouseEventArgs GenerateNewMouseEventArgs(CanvasMouseEventArgs e) {
        var newCanvasCoords = new PositionEventArgs(e.CanvasX, e.CanvasY);

        OnOverwriteMouseImageData(newCanvasCoords);

        if (Bmp?.IsValid() != true) {
            return new TrimmedCanvasMouseEventArgs(e, -1, -1, false);
        }
        var X = (int)Math.Max(0, newCanvasCoords.X);
        var Y = (int)Math.Max(0, newCanvasCoords.Y);
        X = Math.Min(Bmp.Width - 1, X);
        Y = Math.Min(Bmp.Height - 1, Y);

        var IsInBitmap = newCanvasCoords.X >= 0 && newCanvasCoords.Y >= 0 && newCanvasCoords.X <= Bmp.Width && newCanvasCoords.Y <= Bmp.Height;

        return new TrimmedCanvasMouseEventArgs(e, X, Y, IsInBitmap);
    }

    private void OnImageMouseDown(TrimmedCanvasMouseEventArgs e) => ImageMouseDown?.Invoke(this, e);

    private void OnImageMouseMove(TrimmedCanvasMouseEventArgs e) => ImageMouseMove?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(_mouseDown, e));

    private void PrintInfoText(AdditionalDrawingEventArgs e) {
        if (string.IsNullOrEmpty(InfoText)) { return; }

        // Grundlegendes Setup
        using var bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        using var bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        using var fn = new Font("Arial", DrawSize, FontStyle.Bold);

        var drawArea = AvailableControlPaintArea;
        e.Graphics.SetClip(drawArea);

        // Hole alle Bildschirme
        var screens = Screen.AllScreens;

        foreach (var screen in screens) {
            // Konvertiere Bildschirmkoordinaten in Control-Koordinaten
            var screenBounds = screen.Bounds;
            var controlPoint = PointToClient(new Point(screenBounds.X, screenBounds.Y));

            // Berechne die CanvasPosition für diesen Bildschirm
            var textSize = fn.MeasureString(InfoText);

            // Prüfe ob die Maus auf diesem Bildschirm ist
            var mouseScreenPoint = Point.Empty;

            if (e.MouseCurrent != null) {
                mouseScreenPoint = PointToScreen(e.MouseCurrent.ControlPoint);
            }
            var mouseOnThisScreen = screen.Bounds.Contains(mouseScreenPoint);

            // Bestimme Y-CanvasPosition unter Berücksichtigung der Scrollbars
            float yPos;
            if (mouseOnThisScreen) {
                var relativeMouseY = mouseScreenPoint.Y - screenBounds.Y;
                yPos = relativeMouseY < screenBounds.Height / 2
                    ? Math.Min(drawArea.Bottom - textSize.Height, controlPoint.Y + screenBounds.Height - textSize.Height)
                    : Math.Max(drawArea.Top, controlPoint.Y);
            } else {
                yPos = Math.Max(drawArea.Top, controlPoint.Y);
            }

            // Zeichne Hintergrund und Text innerhalb des verfügbaren Bereichs
            var rectBackground = new RectangleF(
                Math.Max(drawArea.Left, controlPoint.X),
                yPos - 5,
                Math.Min(screenBounds.Width, drawArea.Width),
                textSize.Height + 10);

            e.Graphics.FillRectangle(bs, rectBackground);
            BlueFont.DrawString(e.Graphics, InfoText, fn, bf,
                Math.Max(drawArea.Left + 2, controlPoint.X + 2),
                yPos);
        }

        e.Graphics.ResetClip();
    }

    #endregion
}