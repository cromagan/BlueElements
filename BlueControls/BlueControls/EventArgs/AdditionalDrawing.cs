#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
    public class AdditionalDrawing : System.EventArgs
    {
        public AdditionalDrawing(Graphics gr, decimal zoom, decimal movex, decimal movey)
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

            var p1 = new PointDF(rectangle.PointOf(BlueBasics.Enums.enAlignment.Top_Left)).ZoomAndMove(this);
            p1.X -= (float)(Zoom / 2);
            p1.Y -= (float)(Zoom / 2);

            var p2 = new PointDF(rectangle.PointOf(BlueBasics.Enums.enAlignment.Bottom_Right)).ZoomAndMove(this);
            p2.X += (float)(Zoom / 2);
            p2.Y += (float)(Zoom / 2);

            G.FillRectangle(brush, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);


        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            var p1 = new PointDF(x1, y1).ZoomAndMove(this);
            var p2 = new PointDF(x2, y2).ZoomAndMove(this);
            G.DrawLine(pen, p1, p2);
        }
    }
}
