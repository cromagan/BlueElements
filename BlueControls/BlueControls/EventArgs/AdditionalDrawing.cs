#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using System;
using System.Drawing;

namespace BlueControls.EventArgs {
    public class AdditionalDrawing : MouseEventArgs1_1DownAndCurrent {
        public AdditionalDrawing(Graphics gr, decimal zoom, decimal shiftX, decimal shiftY, MouseEventArgs1_1 mouseDown, MouseEventArgs1_1 current) : base(mouseDown, current) {
            G = gr;
            Zoom = zoom;
            ShiftX = shiftX;
            ShiftY = shiftY;
        }

        public Graphics G { get; }
        public decimal Zoom { get; }
        public decimal ShiftX { get; }
        public decimal ShiftY { get; }

        public void FillRectangle(Brush brush, Rectangle rectangle) {
            var x = new RectangleM(rectangle).ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);
            G.FillRectangle(brush, x);
        }

        public void DrawImage(Bitmap BMP) {
            var r = new RectangleM(0, 0, BMP.Width, BMP.Height).ZoomAndMoveRect(Zoom, ShiftX, ShiftY, true);

            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            G.DrawImage(BMP, r);

            //            G.DrawImage(BMP, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);

        }

        public void FillCircle(Color C, int X, int Y, int R) {
            var B = new SolidBrush(C);
            for (var adx = -R; adx <= R; adx++) {
                for (var ady = -R; ady <= R; ady++) {

                    var d = Math.Sqrt(Convert.ToDouble((adx * adx) + (ady * ady))) - 0.5;

                    if (d <= R) { FillRectangle(B, new Rectangle(X + adx, Y + ady, 1, 1)); }
                }
            }
        }

        public void DrawLine(Pen pen, decimal x1, decimal y1, decimal x2, decimal y2) {
            var p1 = new PointM(x1, y1).ZoomAndMove(this);
            var p2 = new PointM(x2, y2).ZoomAndMove(this);
            G.DrawLine(pen, p1, p2);
        }
    }
}
