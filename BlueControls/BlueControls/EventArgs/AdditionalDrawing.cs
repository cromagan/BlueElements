// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.EventArgs {

    public class AdditionalDrawing : MouseEventArgs1_1DownAndCurrent {

        #region Constructors

        public AdditionalDrawing(Graphics gr, double zoom, double shiftX, double shiftY, MouseEventArgs1_1 mouseDown, MouseEventArgs1_1 current) : base(mouseDown, current) {
            G = gr;
            Zoom = zoom;
            ShiftX = shiftX;
            ShiftY = shiftY;
        }

        #endregion

        #region Properties

        public Graphics G { get; }
        public double ShiftX { get; }
        public double ShiftY { get; }
        public double Zoom { get; }

        #endregion

        #region Methods

        public void DrawImage(Bitmap BMP) {
            var r = new RectangleM(0, 0, BMP.Width, BMP.Height).ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);
            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            G.DrawImage(BMP, r);
            //            G.DrawImage(BMP, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }

        public void DrawLine(Pen pen, double x1, double y1, double x2, double y2) {
            var p1 = new PointM(x1, y1).ZoomAndMove(this);
            var p2 = new PointM(x2, y2).ZoomAndMove(this);
            G.DrawLine(pen, p1, p2);
        }

        public void FillCircle(Color C, int X, int Y, int R) {
            SolidBrush B = new(C);
            for (var adx = -R; adx <= R; adx++) {
                for (var ady = -R; ady <= R; ady++) {
                    var d = Math.Sqrt(Convert.ToDouble((adx * adx) + (ady * ady))) - 0.5;
                    if (d <= R) { FillRectangle(B, new Rectangle(X + adx, Y + ady, 1, 1)); }
                }
            }
        }

        public void FillRectangle(Brush brush, Rectangle rectangle) {
            var x = new RectangleM(rectangle).ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);
            G.FillRectangle(brush, x);
        }

        #endregion
    }
}