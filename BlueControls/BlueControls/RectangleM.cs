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
using BlueBasics;
using BlueBasics.Enums;
using System;
using System.Drawing;

namespace BlueControls {
    public class RectangleM : ICloneable {
        public decimal X;
        public decimal Y;
        public decimal Width;
        public decimal Height;
        public RectangleM() : this(0M, 0M, 0M, 0M) { }
        public RectangleM(Rectangle r) : this(r.X, r.Y, r.Width, r.Height) { }
        public RectangleM(PointM p1, PointM p2) : this(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y)) { }
        public RectangleM(decimal x, decimal y, decimal width, decimal height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public decimal Left => X;
        public decimal Top => Y;
        public decimal Right => X + Width;
        public decimal Bottom => Y + Height;
        /// <summary>
        /// Positive Werte verkleinern das Rechteck, negative vergrößern es.
        /// </summary>
        /// <param name="xVal"></param>
        /// <param name="yVal"></param>
        public void Inflate(int xVal, int yVal) {
            X += xVal;
            Y += yVal;
            Width -= xVal * 2;
            Height -= yVal * 2;
        }
        public PointM PointOf(enAlignment corner) {
            switch (corner) {

                case enAlignment.Bottom_Left:
                    return new PointM(Left, Bottom);

                case enAlignment.Bottom_Right:
                    return new PointM(Right, Bottom);

                case enAlignment.Top_Left:
                    return new PointM(Left, Top);

                case enAlignment.Top_Right:
                    return new PointM(Right, Top);

                case enAlignment.Bottom_HorizontalCenter:
                    return new PointM(Left + (Width / 2m), Bottom);

                case enAlignment.Top_HorizontalCenter:
                    return new PointM(Left + (Width / 2m), Top);

                case enAlignment.VerticalCenter_Left:
                    return new PointM(Left, Top + (Height / 2m));

                case enAlignment.VerticalCenter_Right:
                    return new PointM(Right, Top + (Height / 2m));

                case enAlignment.Horizontal_Vertical_Center:
                    return new PointM(Left + (Width / 2m), Top + (Height / 2m));
                default:
                    Develop.DebugPrint(corner);
                    return new PointM();
            }
        }
        public PointM NearestCornerOF(PointM P) {
            var LO = PointOf(enAlignment.Top_Left);
            var rO = PointOf(enAlignment.Top_Right);
            var ru = PointOf(enAlignment.Bottom_Right);
            var lu = PointOf(enAlignment.Bottom_Left);
            var llo = GeometryDF.Länge(P, LO);
            var lro = GeometryDF.Länge(P, rO);
            var llu = GeometryDF.Länge(P, lu);
            var lru = GeometryDF.Länge(P, ru);
            var Erg = Math.Min(Math.Min(llo, lro), Math.Min(llu, lru));
            return Erg == llo ? LO : Erg == lro ? rO : Erg == llu ? lu : Erg == lru ? ru : null;
        }
        public bool Contains(PointM p) => Contains(p.X, p.Y);
        public bool Contains(decimal pX, decimal pY) => pX >= X && pY >= Y && pX <= X + Width && pY <= Y + Height;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="outerLine">true = die Punkte komplett umschlossen (für Fills), false = Mitte der Punkte</param>
        /// <returns></returns>
        public RectangleF ZoomAndMoveRect(decimal zoom, decimal shiftX, decimal shiftY, bool outerLine) {
            if (outerLine) {
                // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
                //               0 und 25 rauskommen
                return new RectangleF((float)((X * zoom) - shiftX),
                      (float)((Y * zoom) - shiftY),
                      (float)(Width * zoom),
                      (float)(Height * zoom));
            } else {
                // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
                //               2,5 und 27,5 rauskommen
                var add = zoom / 2;
                return new RectangleF((float)((X * zoom) - shiftX + add),
                                      (float)((Y * zoom) - shiftY + add),
                                      (float)(Width * zoom),
                                      (float)(Height * zoom));
            }
        }
        /// <summary>
        /// Erweitert das Rechteck, dass ein Kreis mit den angegebenen Parametern ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        /// <param name="maxrad"></param>
        public void ExpandTo(PointM middle, decimal radius) {
            ExpandTo(new PointM(middle.X, middle.Y + radius));
            ExpandTo(new PointM(middle.X, middle.Y - radius));
            ExpandTo(new PointM(middle.X + radius, middle.Y));
            ExpandTo(new PointM(middle.X - radius, middle.Y));
        }
        /// <summary>
        /// Erweitert das Rechteck, dass der Angegebene Punkt ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        public void ExpandTo(PointM P) {
            if (P.X < X) {
                Width = Right - P.X;
                X = P.X;
            }
            if (P.Y < Y) {
                Height = Bottom - P.Y;
                Y = P.Y;
            }
            if (P.X > Right) {
                Width = P.X - X;
            }
            if (P.Y > Bottom) {
                Height = P.Y - Y;
            }
        }
        public object Clone() => new RectangleM(X, Y, Width, Height);
        public static explicit operator RectangleF(RectangleM r) {
            return new((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        }

        public static explicit operator Rectangle(RectangleM r) {
            return new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }
    }
}
