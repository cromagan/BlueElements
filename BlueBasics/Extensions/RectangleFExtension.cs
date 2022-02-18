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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueBasics {

    public static partial class Extensions {

        #region Methods

        /// <summary>
        /// Erweitert das Rechteck, dass der Angegebene Punkt ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        public static RectangleF ExpandTo(this RectangleF r, PointF P) {
            if (P.X < r.X) {
                r.Width = r.Right - P.X;
                r.X = P.X;
            }
            if (P.Y < r.Y) {
                r.Height = r.Bottom - P.Y;
                r.Y = P.Y;
            }
            if (P.X > r.Right) {
                r.Width = P.X - r.X;
            }
            if (P.Y > r.Bottom) {
                r.Height = P.Y - r.Y;
            }

            return r;
        }

        /// <summary>
        /// Erweitert das Rechteck, dass ein Kreis mit den angegebenen Parametern ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        /// <param name="maxrad"></param>
        public static RectangleF ExpandTo(this RectangleF r, PointF middle, float radius) {
            r = r.ExpandTo(new PointF(middle.X, middle.Y + radius));
            r = r.ExpandTo(new PointF(middle.X, middle.Y - radius));
            r = r.ExpandTo(new PointF(middle.X + radius, middle.Y));
            r = r.ExpandTo(new PointF(middle.X - radius, middle.Y));
            return r;
        }

        public static PointF NearestCornerOF(this RectangleF r, PointF p) {
            var pl = new List<PointF>();
            pl.Add(r.PointOf(enAlignment.Top_Left));
            pl.Add(r.PointOf(enAlignment.Top_Right));
            pl.Add(r.PointOf(enAlignment.Bottom_Right));
            pl.Add(r.PointOf(enAlignment.Bottom_Left));
            return p.NearestPoint(pl);
        }

        public static PointF NearestLineMiddle(this RectangleF r, PointF p) {
            var pl = new List<PointF>();
            pl.Add(r.PointOf(enAlignment.Bottom_HorizontalCenter));
            pl.Add(r.PointOf(enAlignment.Top_HorizontalCenter));
            pl.Add(r.PointOf(enAlignment.VerticalCenter_Left));
            pl.Add(r.PointOf(enAlignment.VerticalCenter_Right));
            return p.NearestPoint(pl);
        }

        public static PointF PointOf(this RectangleF r, enAlignment p) {
            switch (p) {
                case enAlignment.Bottom_Left: return new PointF(r.Left, r.Bottom);
                case enAlignment.Bottom_Right: return new PointF(r.Right, r.Bottom);
                case enAlignment.Top_Left: return new PointF(r.Left, r.Top);
                case enAlignment.Top_Right: return new PointF(r.Right, r.Top);
                case enAlignment.Bottom_HorizontalCenter: return new PointF((int)(r.Left + (r.Width / 2.0)), r.Bottom);
                case enAlignment.Top_HorizontalCenter: return new PointF((int)(r.Left + (r.Width / 2.0)), r.Top);
                case enAlignment.VerticalCenter_Left: return new PointF(r.Left, (int)(r.Top + (r.Height / 2.0)));
                case enAlignment.VerticalCenter_Right: return new PointF(r.Right, (int)(r.Top + (r.Height / 2.0)));
                case enAlignment.Horizontal_Vertical_Center: return new Point((int)(r.Left + (r.Width / 2.0)), (int)(r.Top + (r.Height / 2.0)));
                default:
                    Develop.DebugPrint(p);
                    return Point.Empty;
            }
        }

        public static Rectangle ToRect(this RectangleF r) => new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

        /// <summary>
        ///
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="outerLine">true = die Punkte komplett umschlossen (für Fills), false = Mitte der Punkte</param>
        /// <returns></returns>
        public static RectangleF ZoomAndMoveRect(this Rectangle r, float zoom, float shiftX, float shiftY, bool outerLine) {
            if (outerLine) {
                // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
                //               0 und 25 rauskommen
                return new RectangleF((float)((r.X * zoom) - shiftX),
                      (float)((r.Y * zoom) - shiftY),
                      (float)(r.Width * zoom),
                      (float)(r.Height * zoom));
            } else {
                // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
                //               2,5 und 27,5 rauskommen
                var add = zoom / 2;
                return new RectangleF((r.X * zoom) - shiftX + add,
                                      (r.Y * zoom) - shiftY + add,
                                      r.Width * zoom,
                                      r.Height * zoom);
            }
        }

        #endregion
    }
}