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

    private const int DrawSize = 20;
    private static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
    private static readonly Pen PenCrosshairShadow = new(Color.FromArgb(10, 0, 0, 0), 3);
    private static readonly Pen PenCrosshairLine = new(Color.FromArgb(220, 100, 255, 100));
    private BitmapPadItem? _bmpItem;
    private TrimmedCanvasMouseEventArgs? _trimmedCurrentMouseData;

    #endregion

    #region Constructors

    public ZoomPic() : base() {
        InitializeComponent();
        StyleItems();
    }

    #endregion

    #region Events

    public event EventHandler<TrimmedCanvasMouseEventArgs>? ImageMouseDown;

    public event EventHandler<TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs>? ImageMouseMove;

    public event EventHandler<TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs>? ImageMouseUp;

    public event EventHandler<PositionEventArgs>? OverwriteMouseImageData;

    #endregion

    #region Properties

    public GenericTool? ActiveTool {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    }

    public Bitmap? Bmp {
        get => _bmpItem?.Bitmap;
        set {
            if (_bmpItem?.Bitmap == value) { return; }

            if (value is null || Items is null) {
                if (_bmpItem is not null) {
                    Items?.Remove(_bmpItem);
                    _bmpItem.Dispose();
                    _bmpItem = null;
                }
                Invalidate();
                return;
            }

            StyleItems();

            if (_bmpItem is null || _bmpItem.IsDisposed) {
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
            CanvasMargin = (int)(value.Width * 0.01);
            Items.SendToBack(_bmpItem);

            Invalidate();
        }
    }

    public override bool ControlMustPressedForZoomWithWheel => false;

    [DefaultValue(Helpers.None)]
    public Helpers Helper {
        get => field;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = Helpers.None;

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
        get => field;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = Orientation.Ohne;

    protected override bool ShowSliderX => true;
    protected override int SmallChangeY => 5;
    protected TrimmedCanvasMouseEventArgs TrimmedMouseDownData { get; set; } = new TrimmedCanvasMouseEventArgs();

    #endregion

    #region Methods

    protected override void DrawControl(Graphics gr, States state) {
        base.DrawControl(gr, state);

        if (Bmp?.IsValid() != true) { return; }

        var controlDrawArea = AvailableControlPaintArea;
        gr.SetClip(controlDrawArea);

        PositionEventArgs newCanvasCoords;
        if (CurrentMouseData is not null) {
            newCanvasCoords = new PositionEventArgs(CurrentMouseData.CanvasX, CurrentMouseData.CanvasY);
        } else {
            newCanvasCoords = new PositionEventArgs(0, 0);
        }
        OnOverwriteMouseImageData(newCanvasCoords);

        // Mittellinie
        var canvasPicturePos = CanvasMaxBounds;
        if (Mittellinie.HasFlag(Orientation.Waagerecht)) {
            var p1 = canvasPicturePos.PointOf(Alignment.VerticalCenter_Left).CanvasToControl(Zoom, OffsetX, OffsetY);
            var p2 = canvasPicturePos.PointOf(Alignment.VerticalCenter_Right).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawLine(PenCrosshairShadow, p1, p2);
            gr.DrawLine(PenCrosshairLine, p1, p2);
        }

        if (Mittellinie.HasFlag(Orientation.Senkrecht)) {
            var p1 = canvasPicturePos.PointOf(Alignment.Top_HorizontalCenter).CanvasToControl(Zoom, OffsetX, OffsetY);
            var p2 = canvasPicturePos.PointOf(Alignment.Bottom_HorizontalCenter).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawLine(PenCrosshairShadow, p1, p2);
            gr.DrawLine(PenCrosshairLine, p1, p2);
        }

        if (CurrentMouseData is null) {
            gr.ResetClip();
            return;
        }

        if (Helper.HasFlag(Helpers.HorizontalLine)) {
            var p1 = new PointF(0, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
            var p2 = new PointF(Bmp.Width, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawLine(PenRotTransp, p1, p2);
        }

        if (Helper.HasFlag(Helpers.VerticalLine)) {
            var p1 = new PointF(newCanvasCoords.X, 0).CanvasToControl(Zoom, OffsetX, OffsetY);
            var p2 = new PointF(newCanvasCoords.X, Bmp.Height).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawLine(PenRotTransp, p1, p2);
        }

        if (Helper.HasFlag(Helpers.SymetricalHorizontal)) {
            var h = Bmp.Width / 2;
            var x = Math.Abs(h - newCanvasCoords.X);
            var p1 = new PointF(h - x, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
            var p2 = new PointF(h + x, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawLine(PenRotTransp, p1, p2);
        }

        if (Helper.HasFlag(Helpers.MouseDownPoint)) {
            var m1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
            gr.DrawEllipse(PenRotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));
            if (MouseDownData is not null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
                gr.DrawEllipse(PenRotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
                gr.DrawLine(PenRotTransp, mc1, md1);
            }
        }

        if (Helper.HasFlag(Helpers.FilledRectancle)) {
            if (MouseDownData is not null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
                var r = new RectangleF(Math.Min(md1.X, newCanvasCoords.X), Math.Min(md1.Y, newCanvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                gr.FillRectangle(BrushRotTransp, r);
            }
        }

        // Rechteck zeichnen
        if (Helper.HasFlag(Helpers.DrawRectangle)) {
            if (MouseDownData is not null) {
                var md1 = MouseDownData.ControlPoint;
                var mc1 = new PointF(newCanvasCoords.X, newCanvasCoords.Y).CanvasToControl(Zoom, OffsetX, OffsetY);
                var r = new RectangleF(Math.Min(md1.X, newCanvasCoords.X), Math.Min(md1.Y, newCanvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                gr.DrawRectangle(PenRotTransp, r.X, r.Y, r.Width, r.Height);
            }
        }

        // kleines Rechteck zeichnen
        if (Helper.HasFlag(Helpers.Draw20x10)) {
            if (CurrentMouseData is not null) {
                var startPoint = new PointF(CurrentMouseData.CanvasX - 10, CurrentMouseData.CanvasY - 5);
                var scaledStart = startPoint.CanvasToControl(Zoom, OffsetX, OffsetY);

                var scaledWidth = 20 * Zoom;
                var scaledHeight = 10 * Zoom;

                gr.DrawRectangle(PenRotTransp,
                    scaledStart.X,
                    scaledStart.Y,
                    scaledWidth,
                    scaledHeight);
            }
        }

        gr.ResetClip();

        ActiveTool?.DrawOverlay(gr, Zoom, OffsetX, OffsetY, TrimmedMouseDownData, _trimmedCurrentMouseData);

        PrintInfoText(gr, CurrentMouseData);

        if (Helper.HasFlag(Helpers.Magnifier) && Bmp is not null && CurrentMouseData is not null) {
            const int magnifierSize = 200;
            const int magnifierMargin = 50;

            var visibleArea = AvailableControlPaintArea;
            var mouseControl = CurrentMouseData.ControlPoint;

            var mx = mouseControl.X < visibleArea.Left + visibleArea.Width / 2.0
                ? visibleArea.Right - magnifierMargin - magnifierSize
                : visibleArea.Left + magnifierMargin;
            var my = mouseControl.Y < visibleArea.Top + visibleArea.Height / 2.0
                ? visibleArea.Bottom - magnifierMargin - magnifierSize
                : visibleArea.Top + magnifierMargin;

            Bmp.Magnify(CurrentMouseData.CanvasPoint, new Rectangle(mx, my, magnifierSize, magnifierSize), gr);
        }
    }

    protected virtual void OnImageMouseUp(TrimmedCanvasMouseEventArgs e) {
        ImageMouseUp?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(TrimmedMouseDownData, e));
    }

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        _trimmedCurrentMouseData = GenerateNewMouseEventArgs(e);
        TrimmedMouseDownData = _trimmedCurrentMouseData;
        OnImageMouseDown(TrimmedMouseDownData);
        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);
        _trimmedCurrentMouseData = GenerateNewMouseEventArgs(e);
        OnImageMouseMove(_trimmedCurrentMouseData);
        Invalidate();
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        _trimmedCurrentMouseData = GenerateNewMouseEventArgs(e);
        OnImageMouseUp(_trimmedCurrentMouseData);
    }

    protected void OnOverwriteMouseImageData(PositionEventArgs e) => OverwriteMouseImageData?.Invoke(this, e);

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

    private void OnImageMouseMove(TrimmedCanvasMouseEventArgs e) => ImageMouseMove?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(TrimmedMouseDownData, e));

    private void PrintInfoText(Graphics gr, CanvasMouseEventArgs mouseCurrent) {
        if (string.IsNullOrEmpty(InfoText)) { return; }

        using var bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        using var bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        using var fn = new Font("Arial", DrawSize, FontStyle.Bold);

        var drawArea = AvailableControlPaintArea;
        gr.SetClip(drawArea);

        var screens = Screen.AllScreens;

        foreach (var screen in screens) {
            var screenBounds = screen.Bounds;
            var controlPoint = PointToClient(new Point(screenBounds.X, screenBounds.Y));

            var textSize = fn.MeasureString(InfoText);

            var mouseScreenPoint = Point.Empty;

            if (mouseCurrent is not null) {
                mouseScreenPoint = PointToScreen(mouseCurrent.ControlPoint);
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

            gr.FillRectangle(bs, rectBackground);
            BlueFont.DrawString(gr, InfoText, fn, bf,
                Math.Max(drawArea.Left + 2, controlPoint.X + 2),
                yPos);
        }

        gr.ResetClip();
    }

    private void StyleItems() {
        ShowInPrintMode = true;

        if (Items is null) { return; }

        Items.Endless = true;
        Items.GridShow = 0;
        Items.BackColor = Color.Transparent;
        CanvasMargin = 5;
    }

    #endregion
}