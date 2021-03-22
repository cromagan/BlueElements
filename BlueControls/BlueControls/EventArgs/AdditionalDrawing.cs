#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
            RectangleF x = new RectangleM(rectangle).ZoomAndMoveRect(Zoom, ShiftX, ShiftY);
            G.FillRectangle(brush, x);
        }

        public void DrawImage(Bitmap BMP) {
            RectangleF r = new RectangleM(0, 0, BMP.Width, BMP.Height).ZoomAndMoveRect(Zoom, ShiftX, ShiftY);

            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;



            G.DrawImage(BMP, r);

            //            G.DrawImage(BMP, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);

        }


        public void FillCircle(Color C, int X, int Y, int R) {
            SolidBrush B = new SolidBrush(C);
            for (int adx = -R; adx <= R; adx++) {
                for (int ady = -R; ady <= R; ady++) {

                    double d = Math.Sqrt(Convert.ToDouble(adx * adx + ady * ady)) - 0.5;

                    if (d <= R) { FillRectangle(B, new Rectangle(X + adx, Y + ady, 1, 1)); }

                }
            }
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2) {
            PointF p1 = new PointM(x1, y1).ZoomAndMove(this);
            PointF p2 = new PointM(x2, y2).ZoomAndMove(this);
            G.DrawLine(pen, p1, p2);
        }
    }
}
