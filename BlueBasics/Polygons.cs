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

using BlueBasics.Enums;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueBasics {

    public static class Polygons {

        #region Methods

        public static void AddRad(this GraphicsPath gP, PointF middle, PointF startP, float wink) {
            var radius = (float)Math.Abs(Geometry.GetLenght(middle, startP));
            var startw = (float)Geometry.GetAngle(middle, startP);
            gP.AddArc(middle.X - radius, middle.Y - radius, radius * 2, radius * 2, -startw, -wink);
        }

        public static GraphicsPath Poly_Arrow(Rectangle rect) {
            GraphicsPath p = new();
            /// --------+  >
            ///         | /
            ///         |/
            ///
            PointF plusOben = new((float)(rect.Left + (rect.Width * 0.5)), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y - (rect.Height * 0.18)));
            PointF plusUnten = new((float)(rect.Left + (rect.Width * 0.5)), (float)(rect.PointOf(enAlignment.VerticalCenter_Right).Y + (rect.Height * 0.18)));
            p.AddLine(rect.PointOf(enAlignment.VerticalCenter_Right), new PointF(plusUnten.X, rect.Bottom));
            p.AddLine(p.GetLastPoint(), plusUnten);
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusUnten.Y));
            p.AddLine(p.GetLastPoint(), new PointF(rect.Left, plusOben.Y));
            p.AddLine(p.GetLastPoint(), plusOben);
            p.AddLine(p.GetLastPoint(), new PointF(plusOben.X, rect.Top));
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath Poly_Bruchlinie(Rectangle rect) {
            GraphicsPath p = new();
            p.AddLine(rect.PointOf(enAlignment.Top_Left), rect.PointOf(enAlignment.Top_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Right));
            p.AddLine(p.GetLastPoint(), rect.PointOf(enAlignment.Bottom_Left));
            var versX = rect.Width / 6;
            var versY = -rect.Height / 10;
            var pu = p.GetLastPoint();
            for (var z = 0; z < 10; z++) {
                pu.Y += versY;
                pu.X += versX;
                versX *= -1;
                p.AddLine(p.GetLastPoint(), pu);
            }
            p.CloseFigure();
            return p;
        }

        public static GraphicsPath Poly_Rechteck(Rectangle rect) {
            GraphicsPath tempPoly_Rechteck = new();
            tempPoly_Rechteck.AddRectangle(rect);
            tempPoly_Rechteck.CloseFigure();
            return tempPoly_Rechteck;
        }

        public static GraphicsPath Poly_RoundRec(Rectangle rect, int radius) => Poly_RoundRec(rect.X, rect.Y, rect.Width, rect.Height, radius);

        public static GraphicsPath Poly_RoundRec(int x, int y, int width, int height, int radius) {
            if (width < 1 || height < 1) { return null; }
            GraphicsPath tempPoly_RoundRec = new();
            if (radius > (height / 2.0) + 2) { radius = (int)(height / 2.0) + 2; }
            if (radius > (width / 2.0) + 2) { radius = (int)(width / 2.0) + 2; }
            tempPoly_RoundRec.AddLine(x + radius, y, x + width - radius, y);
            AddRad90(x + width - radius, y, radius, 270); // OK
            tempPoly_RoundRec.AddLine(x + width, y + radius, x + width, y + height - radius);
            AddRad90(x + width - radius, y + height - radius, radius, 0);
            tempPoly_RoundRec.AddLine(x + width - radius, y + height, x + radius, y + height);
            AddRad90(x, y + height - radius, radius, 90);
            tempPoly_RoundRec.AddLine(x, y + height - radius, x, y + radius);
            AddRad90(x, y, radius, 180); // OK
            tempPoly_RoundRec.CloseFigure();
            return tempPoly_RoundRec;

            void AddRad90(int mxX, int mxY, int Radius, int gradStart) => tempPoly_RoundRec.AddArc(mxX, mxY, Radius, Radius, gradStart, 90);
        }

        public static GraphicsPath Poly_Triangle(PointF p1, PointF p2, PointF p3) {
            GraphicsPath P = new();
            P.AddLine(p1, p2);
            P.AddLine(p2, p3);
            P.CloseFigure();
            return P;
        }

        #endregion
    }
}