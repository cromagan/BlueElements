// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueControls.EventArgs;

public class AdditionalDrawing : MouseEventArgs1_1DownAndCurrent {

    #region Constructors

    public AdditionalDrawing(Graphics gr, float zoom, float shiftX, float shiftY, MouseEventArgs1_1 mouseDown, MouseEventArgs1_1 current) : base(mouseDown, current) {
        G = gr;
        Zoom = zoom;
        ShiftX = shiftX;
        ShiftY = shiftY;
    }

    #endregion

    #region Properties

    public Graphics G { get; }
    public float ShiftX { get; }
    public float ShiftY { get; }
    public float Zoom { get; }

    #endregion

    #region Methods

    public void DrawImage(Bitmap? bmp) {
        if (bmp == null) {
            return;
        }

        var r = new RectangleF(0, 0, bmp.Width, bmp.Height).ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);
        G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        G.DrawImage(bmp, r);
    }

    public void DrawLine(Pen pen, float x1, float y1, float x2, float y2) {
        var p1 = new PointM(x1, y1).ZoomAndMove(this);
        var p2 = new PointM(x2, y2).ZoomAndMove(this);
        G.DrawLine(pen, p1, p2);
    }

    public void FillCircle(Color c, int x, int y, int r) {
        SolidBrush b = new(c);
        for (var adx = -r; adx <= r; adx++) {
            for (var ady = -r; ady <= r; ady++) {
                var d = Math.Sqrt((adx * adx) + (ady * ady)) - 0.5;
                if (d <= r) { FillRectangle(b, new Rectangle(x + adx, y + ady, 1, 1)); }
            }
        }
    }

    public void FillRectangle(Brush brush, Rectangle rectangle) {
        var x = rectangle.ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);
        G.FillRectangle(brush, x);
    }

    #endregion
}