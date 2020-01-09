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


using BlueBasics;
using System;
using System.Drawing;

namespace BlueControls.EventArgs
{
    public class AdditionalDrawing : MouseEventArgs1_1DownAndCurrent
    {
        public AdditionalDrawing(Graphics gr, decimal zoom, decimal movex, decimal movey, MouseEventArgs1_1 mouseDown, MouseEventArgs1_1 current) : base(mouseDown, current)
        {
            this.G = gr;
            this.Zoom = zoom;
            this.MoveX = movex;
            this.MoveY = movey;
        }

        public Graphics G { get; }
        public decimal Zoom { get; }
        public decimal MoveX { get; }
        public decimal MoveY { get; }

        public void FillRectangle(Brush brush, Rectangle rectangle)
        {
            var x = new RectangleDF(rectangle).ZoomAndMoveRect(Zoom, MoveX, MoveY);
            G.FillRectangle(brush, x);
        }

        public void DrawImage(Bitmap BMP)
        {
            var r = new RectangleDF(0, 0, BMP.Width, BMP.Height).ZoomAndMoveRect(Zoom, MoveX, MoveY);

            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;



            G.DrawImage(BMP, r);

//            G.DrawImage(BMP, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);

        }


        public void FillCircle(Color C, int X, int Y, int R)
        {
            var B = new SolidBrush(C);
            for (var adx = -R; adx <= R; adx++)
            {
                for (var ady = -R; ady <= R; ady++)
                {

                    var d = Math.Sqrt(Convert.ToDouble(adx * adx + ady * ady))-0.5;

                    if (d <= R) { FillRectangle(B, new Rectangle(X + adx, Y + ady, 1, 1)); }

                }
            }
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            var p1 = new PointDF(x1, y1).ZoomAndMove(this);
            var p2 = new PointDF(x2, y2).ZoomAndMove(this);
            G.DrawLine(pen, p1, p2);
        }
    }
}
