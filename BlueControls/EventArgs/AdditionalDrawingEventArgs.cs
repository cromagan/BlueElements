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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.Extensions;

namespace BlueControls.EventArgs;

public class AdditionalDrawingEventArgs : System.EventArgs {

    #region Constructors

    public AdditionalDrawingEventArgs(Graphics gr, float zoom, float offsetX, float offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) : base() {
        Graphics = gr;
        Zoom = zoom;
        OffsetX = offsetX;
        OffsetY = offsetY;
        MouseDown = mouseDown;
        MouseCurrent = mouseCurrent;
    }

    #endregion

    #region Properties

    public Graphics Graphics { get; }
    public TrimmedCanvasMouseEventArgs? MouseCurrent { get; }
    public TrimmedCanvasMouseEventArgs? MouseDown { get; }
    public float OffsetX { get; }
    public float OffsetY { get; }
    public float Zoom { get; }

    #endregion

    #region Methods

    public void DrawImage(Bitmap? bmp) {
        if (bmp == null) {
            return;
        }

        var r = new RectangleF(0, 0, bmp.Width, bmp.Height).CanvasToControl(Zoom, OffsetX, OffsetY, true);
        Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        Graphics.DrawImage(bmp, r);
    }

    public void DrawLine(Pen pen, float canvasX1, float canvasY1, float canvasX2, float canvasY2) {
        var p1 = new PointF(canvasX1, canvasY1).CanvasToControl(Zoom, OffsetX, OffsetY);
        var p2 = new PointF(canvasX2, canvasY2).CanvasToControl(Zoom, OffsetX, OffsetY);
        Graphics.DrawLine(pen, p1, p2);
    }

    public void FillCircle(Color c, int canvasX, int canvasY, int canvasR) {
        var b = new SolidBrush(c);
        for (var adx = -canvasR; adx <= canvasR; adx++) {
            for (var ady = -canvasR; ady <= canvasR; ady++) {
                var d = Math.Sqrt((adx * adx) + (ady * ady)) - 0.5;
                if (d <= canvasR) { FillRectangle(b, new Rectangle(canvasX + adx, canvasY + ady, 1, 1)); }
            }
        }
    }

    public void FillRectangle(Brush brush, Rectangle canvasRectangle) {
        var x = canvasRectangle.CanvasToControl(Zoom, OffsetX, OffsetY, true);
        Graphics.FillRectangle(brush, x);
    }

    public Rectangle TrimmedRectangle() {
        if (MouseDown == null || MouseCurrent == null) { return Rectangle.Empty; }

        return new(Math.Min(MouseDown.TrimmedCanvasX, MouseCurrent.TrimmedCanvasX), Math.Min(MouseDown.TrimmedCanvasY, MouseCurrent.TrimmedCanvasY), Math.Abs(MouseDown.TrimmedCanvasX - MouseCurrent.TrimmedCanvasX) + 1, Math.Abs(MouseDown.TrimmedCanvasY - MouseCurrent.TrimmedCanvasY) + 1);
    }

    #endregion
}