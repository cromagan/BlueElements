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

using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.Extensions;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ZoomPic : ZoomPad {

    #region Fields

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

    public override bool ControlMustPressed => false;
    protected override bool AutoCenter => true;
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
        using LinearGradientBrush lgb = new(drawArea, Color.White, Color.LightGray, LinearGradientMode.Vertical);
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

    protected virtual void OnDoAdditionalDrawing(AdditionalDrawingEventArgs e) => DoAdditionalDrawing?.Invoke(this, e);

    /// <summary>
    /// Zuerst ImageMouseUp, dann MouseUp
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnImageMouseUp(TrimmedCanvasMouseEventArgs e) => ImageMouseUp?.Invoke(this, new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(_mouseDown, e));

    protected override void OnMouseDown(CanvasMouseEventArgs e) {
        base.OnMouseDown(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        _mouseDown = _mouseCurrent;
        OnImageMouseDown(_mouseDown);
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);
        _mouseCurrent = GenerateNewMouseEventArgs(e);
        OnImageMouseMove(_mouseCurrent);
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

    #endregion
}