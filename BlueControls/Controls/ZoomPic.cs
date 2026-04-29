// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPic : CreativePad {

    #region Fields

    protected Helpers _helper = Helpers.None;
    protected Orientation _mittelLinie = Orientation.Ohne;
    private const int DrawSize = 20;
    private static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
    private BitmapPadItem? _bmpItem;
    private TrimmedCanvasMouseEventArgs? _mouseCurrent;
    private TrimmedCanvasMouseEventArgs _mouseDown = new TrimmedCanvasMouseEventArgs();

    #endregion

    #region Constructors

    public ZoomPic() : base() {
        InitializeComponent();
        StyleItems();
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

    public Bitmap? Bmp {
        get => _bmpItem?.Bitmap;
        set {
            if (_bmpItem?.Bitmap == value) { return; }

            if (value == null || Items == null) {
                if (_bmpItem != null) {
                    Items?.Remove(_bmpItem);
                    _bmpItem.Dispose();
                    _bmpItem = null;
                }
                Invalidate();
                return;
            }

            StyleItems();

            if (_bmpItem == null || _bmpItem.IsDisposed) {
                _bmpItem = new BitmapPadItem("ZOOM_PIC_IMAGE", value, value.Size);
            }

            if (!Items.Contains(_bmpItem)) { Items.Add(_bmpItem); }

            _bmpItem.Bitmap = value;
            _bmpItem.PixelGenau = true;
            _bmpItem.SetCoordinates(new RectangleF(0, 0, value.Width, value.Height));
            _bmpItem.Bild_Modus = SizeModes.Verzerren;
            _bmpItem.Hintergrund_Weiß_Füllen = false;
            _bmpItem.Style = PadStyles.Undefined;
            _bmpItem.Drehwinkel = 0;
            _bmpItem.Enabled = false;
            Items.SendToBack(_bmpItem);

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

    protected override void AdditionalDrawing(Graphics gr, Rectangle drawArea) => OnDoAdditionalDrawing(new AdditionalDrawingEventArgs(gr, Zoom, OffsetX, OffsetY, _mouseDown, _mouseCurrent));

    protected virtual void OnDoAdditionalDrawing(AdditionalDrawingEventArgs e) {
        DoAdditionalDrawing?.Invoke(this, e);

        DrawHelpers(e);

        if (!string.IsNullOrEmpty(InfoText)) {
            PrintInfoText(e);
        }

        if (_helper.HasFlag(Helpers.Magnifier) && Bmp != null && e.MouseCurrent != null) {
            Bmp.Magnify(e.MouseCurrent.CanvasPoint, e.Graphics, false);
        }
    }

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

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseUp(_mouseCurrent);
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
                var startPoint = new PointF(e.MouseCurrent.CanvasX - 10, e.MouseCurrent.CanvasY - 5);
                var scaledStart = startPoint.CanvasToControl(e.Zoom, e.OffsetX, e.OffsetY);

                var scaledWidth = 20 * e.Zoom;
                var scaledHeight = 10 * e.Zoom;

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
        var X = (int)Math.Clamp(newCanvasCoords.X, 0, Bmp.Width - 1);
        var Y = (int)Math.Clamp(newCanvasCoords.Y, 0, Bmp.Height - 1);

        var IsInBitmap = newCanvasCoords.X >= 0 && newCanvasCoords.Y >= 0 && newCanvasCoords.X < Bmp.Width && newCanvasCoords.Y < Bmp.Height;

        return new TrimmedCanvasMouseEventArgs(e, X, Y, IsInBitmap);
    }

    private void OnImageMouseDown(TrimmedCanvasMouseEventArgs e) => ImageMouseDown?.Invoke(this, e);

    private void OnImageMouseMove(TrimmedCanvasMouseEventArgs e) => ImageMouseMove?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(_mouseDown, e));

    private void PrintInfoText(AdditionalDrawingEventArgs e) {
        if (string.IsNullOrEmpty(InfoText)) { return; }

        using var bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        using var bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        using var fn = new Font("Arial", DrawSize, FontStyle.Bold);

        var drawArea = AvailableControlPaintArea;
        e.Graphics.SetClip(drawArea);

        var screens = Screen.AllScreens;

        foreach (var screen in screens) {
            var screenBounds = screen.Bounds;
            var controlPoint = PointToClient(new Point(screenBounds.X, screenBounds.Y));

            var textSize = fn.MeasureString(InfoText);

            var mouseScreenPoint = Point.Empty;

            if (e.MouseCurrent != null) {
                mouseScreenPoint = PointToScreen(e.MouseCurrent.ControlPoint);
            }
            var mouseOnThisScreen = screen.Bounds.Contains(mouseScreenPoint);

            float yPos;
            if (mouseOnThisScreen) {
                var relativeMouseY = mouseScreenPoint.Y - screenBounds.Y;
                yPos = relativeMouseY < screenBounds.Height / 2
                    ? Math.Min(drawArea.Bottom - textSize.Height, controlPoint.Y + screenBounds.Height - textSize.Height)
                    : Math.Max(drawArea.Top, controlPoint.Y);
            } else {
                yPos = Math.Max(drawArea.Top, controlPoint.Y);
            }

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

    private void StyleItems() {
        ShowInPrintMode = true;

        if (Items == null) { return; }

        Items.Endless = true;
        Items.GridShow = 0;
    }

    #endregion
}