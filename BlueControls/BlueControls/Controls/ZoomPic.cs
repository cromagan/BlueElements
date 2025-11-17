// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.Extensions;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPic : ZoomPad {

    #region Fields

    private MouseEventArgs1_1? _mouseCurrent;
    private MouseEventArgs1_1? _mouseDown;

    #endregion

    #region Constructors

    public ZoomPic() : base() {
        InitializeComponent();
    }

    #endregion

    #region Events

    public event EventHandler<AdditionalDrawing>? DoAdditionalDrawing;

    public event EventHandler<MouseEventArgs1_1>? ImageMouseDown;

    public event EventHandler<MouseEventArgs1_1DownAndCurrent>? ImageMouseMove;

    public event EventHandler<MouseEventArgs1_1DownAndCurrent>? ImageMouseUp;

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

    #endregion

    #region Methods

    public Point PointInsidePic(int x, int y) {
        if (Bmp == null || !Bmp.IsValid()) { return Point.Empty; }
        x = Math.Max(0, x);
        y = Math.Max(0, y);
        x = Math.Min(Bmp.Width - 1, x);
        y = Math.Min(Bmp.Height - 1, y);
        return new Point(x, y);
    }

    protected override void DrawControl(Graphics gr, States state) {
        base.DrawControl(gr, state);

        // Get drawable area considering scrollbars
        var drawArea = AvailablePaintArea();

        // Create and draw gradient background
        using LinearGradientBrush lgb = new(drawArea, Color.White, Color.LightGray, LinearGradientMode.Vertical);
        gr.FillRectangle(lgb, drawArea);

        if (Bmp != null && Bmp.IsValid()) {
            // Calculate image rectangle considering scrollbars
            var imageRect = new RectangleF(0, 0, Bmp.Width, Bmp.Height)
                .ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);

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

        OnDoAdditionalDrawing(new AdditionalDrawing(gr, Zoom, ShiftX, ShiftY, _mouseDown, _mouseCurrent));

        Skin.Draw_Border(gr, Design.Table_And_Pad, state, drawArea);
    }

    protected override RectangleF MaxBounds() => Bmp != null && Bmp.IsValid() ? new RectangleF(0, 0, Bmp.Width, Bmp.Height) : new RectangleF(0, 0, 0, 0);

    protected virtual void OnDoAdditionalDrawing(AdditionalDrawing e) => DoAdditionalDrawing?.Invoke(this, e);

    /// <summary>
    /// Zuerst ImageMouseUp, dann MouseUp
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnImageMouseUp(MouseEventArgs1_1 e) => ImageMouseUp?.Invoke(this, new MouseEventArgs1_1DownAndCurrent(_mouseDown, e));

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        _mouseDown = _mouseCurrent;
        OnImageMouseDown(_mouseDown);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseMove(_mouseCurrent);
    }

    /// <summary>
    /// Zuerst ImageMouseUp, dann MouseUp
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp(MouseEventArgs e) {
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseUp(_mouseCurrent);
        base.OnMouseUp(e);
        //_MouseDown = null; Wenn die Leute immer beide Maustasten gleichzeitig klicken.
    }

    protected void OnOverwriteMouseImageData(PositionEventArgs e) => OverwriteMouseImageData?.Invoke(this, e);

    private MouseEventArgs1_1 GenerateNewMouseEventArgs(MouseEventArgs e) {
        PositionEventArgs en = new(MousePos11.X, MousePos11.Y);
        OnOverwriteMouseImageData(en);
        var p = PointInsidePic(en.X, en.Y);
        return new MouseEventArgs1_1(e.Button, e.Clicks, en.X, en.Y, e.Delta, p.X, p.Y, IsInBitmap(en.X, en.Y));
    }

    private bool IsInBitmap(int x, int y) => Bmp != null && Bmp.IsValid() && x >= 0 && y >= 0 && x <= Bmp.Width && y <= Bmp.Height;

    private void OnImageMouseDown(MouseEventArgs1_1 e) => ImageMouseDown?.Invoke(this, e);

    private void OnImageMouseMove(MouseEventArgs1_1 e) => ImageMouseMove?.Invoke(this, new MouseEventArgs1_1DownAndCurrent(_mouseDown, e));

    #endregion
}