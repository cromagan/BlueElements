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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.IO;
using static BlueBasics.Extensions;
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
    private TrimmedCanvasMouseEventArgs? _mouseCurrent;
    private TrimmedCanvasMouseEventArgs _mouseDown = new TrimmedCanvasMouseEventArgs();
    private bool _pointAdding;

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

    public event EventHandler<PositionEventArgs>? NoteCreateRequested;

    public event EventHandler<PositionEventArgs>? OverwriteMouseImageData;

    #endregion

    #region Properties

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

    public List<string> Tags { get; } = [];

    public string UserAction { get; set; } = string.Empty;

    protected override bool ShowSliderX => true;
    protected override int SmallChangeY => 5;

    #endregion

    #region Methods

    public static string FilenameTxt(string pathOfPicture) => pathOfPicture.FilePath() + pathOfPicture.FileNameWithoutSuffix() + ".txt";

    public static Tuple<Bitmap?, List<string>> LoadFromDisk(string pathOfPicture) {
        Bitmap? bmp = null;

        if (FileExists(pathOfPicture)) {
            bmp = (Bitmap?)Image_FromFile(pathOfPicture);
        }
        return new Tuple<Bitmap?, List<string>>(bmp, LoadTags(pathOfPicture));
    }

    public static List<string> LoadTags(string pathOfPicture) {
        List<string> tags = [];

        var ftxt = FilenameTxt(pathOfPicture);
        if (FileExists(ftxt)) {
            tags = [.. ReadAllText(ftxt, Encoding.UTF8).SplitAndCutByCr()];
        }
        tags.TagSet("ImageFile", pathOfPicture);
        return tags;
    }

    public PointM? GetPoint(string name) {
        if (Items == null) { return null; }
        foreach (var item in Items) {
            if (item is NotePadItem noteItem && !noteItem.IsDisposed &&
                noteItem.Symbol == NotePadItem.PointSymbol && noteItem.Note == name) {
                return noteItem.MovablePoint[0];
            }
        }
        return null;
    }

    public void LetUserAddAPoint(string kn, Helpers symetricalHorizontal, Orientation senkrecht) {
        _mittelLinie = senkrecht;
        _helper = symetricalHorizontal;
        UserAction = kn;
        _pointAdding = true;
        Invalidate();
    }

    public void LoadData(string pathOfPicture) {
        var (bitmap, tags) = LoadFromDisk(pathOfPicture);
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(tags);
        Invalidate();
    }

    public void PointClear() {
        if (Items == null) { return; }
        var toRemove = new List<AbstractPadItem>();
        foreach (var item in Items) {
            if (item is NotePadItem noteItem && !noteItem.IsDisposed &&
                noteItem.Symbol == NotePadItem.PointSymbol) {
                toRemove.Add(noteItem);
            }
        }
        foreach (var item in toRemove) {
            Items.Remove(item);
        }
        WritePointsInTags();
        Invalidate();
    }

    public void PointSet(string name, float x, int y) => PointSet(name, x, (float)y);

    public void PointSet(string name, float x, float y) {
        if (Items == null) { return; }
        foreach (var item in Items) {
            if (item is NotePadItem noteItem && !noteItem.IsDisposed &&
                noteItem.Symbol == NotePadItem.PointSymbol && noteItem.Note == name) {
                noteItem.SetPosition(x, y);
                WritePointsInTags();
                Invalidate();
                return;
            }
        }
        var newItem = new NotePadItem("POINT_" + name, x, y, NotePadItem.PointSymbol, name);
        Items.Add(newItem);
        WritePointsInTags();
        Invalidate();
    }

    public void SaveData() {
        WritePointsInTags();
        var path = Tags.TagGet("ImageFile");
        var pathtxt = FilenameTxt(path);
        try {
            Bmp?.Save(path, ImageFormat.Png);

            Tags.TagSet("Erstellt", Generic.UserName);
            Tags.TagSet("Datum", DateTime.UtcNow.ToString5());
            Tags.WriteAllText(pathtxt, Win1252, false);
        } catch {
            Develop.DebugPrint("Fehler beim Speichern: " + pathtxt);
            Forms.MessageBox.Show("Fehler beim Speichern");
        }
    }

    public void SetData(Bitmap bitmap, List<string> tags) {
        Bmp = bitmap;
        Tags.Clear();
        Tags.AddRange(tags);
        Invalidate();
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var baseBounds = Bmp?.IsValid() == true
            ? new RectangleF(-20, -20, Bmp.Width + 40, Bmp.Height + 40)
            : new RectangleF(0, 0, 0, 0);

        if (Items != null) {
            foreach (var item in Items) {
                if (item is { IsDisposed: false }) {
                    var ua = item.CanvasUsedArea;
                    if (!ua.IsEmpty) {
                        var combined = RectangleF.Union(baseBounds, ua);
                        baseBounds = combined;
                    }
                }
            }
        }

        return baseBounds;
    }

    protected override void DrawAfterItems(Graphics gr, Rectangle drawArea) => OnDoAdditionalDrawing(new AdditionalDrawingEventArgs(gr, Zoom, OffsetX, OffsetY, _mouseDown, _mouseCurrent));

    protected override void DrawBeforeItems(Graphics gr, Rectangle drawArea) {
        if (Bmp?.IsValid() == true) {
            var imageRect = new RectangleF(0, 0, Bmp.Width, Bmp.Height).CanvasToControl(Zoom, OffsetX, OffsetY, true);

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
    }

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
        if (_pointAdding && !string.IsNullOrEmpty(UserAction)) {
            PointSet(UserAction, e.CanvasX, e.CanvasY);
            _pointAdding = false;
        }
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

        if (e.Button == MouseButtons.Right && EditAllowed) {
            var hotItem = GetHotItem(e, true);
            if (hotItem == null) {
                NoteCreateRequested?.Invoke(this, new PositionEventArgs(e.CanvasX, e.CanvasY));
                return;
            }
        }

        OnImageMouseUp(_mouseCurrent);
        base.OnMouseUp(e);
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

        var IsInBitmap = newCanvasCoords.X >= 0 && newCanvasCoords.Y >= 0 && newCanvasCoords.X <= Bmp.Width && newCanvasCoords.Y <= Bmp.Height;

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

    private void WritePointsInTags() {
        var old = Tags.TagGet("AllPointNames").FromNonCritical().SplitAndCutBy("|");
        foreach (var thisO in old) {
            Tags.TagRemove(thisO);
        }
        Tags.TagRemove("AllPointNames");

        if (Items == null) { return; }

        var pointNames = new List<string>();
        foreach (var item in Items) {
            if (item is NotePadItem noteItem && !noteItem.IsDisposed &&
                noteItem.Symbol == NotePadItem.PointSymbol && !string.IsNullOrEmpty(noteItem.Note)) {
                var pos = noteItem.MovablePoint[0];
                Tags.TagSet(noteItem.Note,
                    pos.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
                    pos.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                pointNames.Add(noteItem.Note);
            }
        }

        if (pointNames.Count > 0) {
            var encodedNames = new List<string>();
            foreach (var p in pointNames) { encodedNames.Add(p.ToNonCritical()); }
            Tags.TagSet("AllPointNames", string.Join("|", encodedNames));
        }
    }

    #endregion
}