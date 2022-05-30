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
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Point NearestCornerOf(this Rectangle r, Point p) {
        var lo = r.PointOf(Alignment.Top_Left);
        var rO = r.PointOf(Alignment.Top_Right);
        var ru = r.PointOf(Alignment.Bottom_Right);
        var lu = r.PointOf(Alignment.Bottom_Left);
        var llo = Geometry.GetLenght(p, lo);
        var lro = Geometry.GetLenght(p, rO);
        var llu = Geometry.GetLenght(p, lu);
        var lru = Geometry.GetLenght(p, ru);
        var erg = Math.Min(Math.Min(llo, lro), Math.Min(llu, lru));
        return erg == llo ? lo : erg == lro ? rO : erg == llu ? lu : erg == lru ? ru : Point.Empty;
    }

    public static Point PointOf(this Rectangle r, Alignment p) {
        switch (p) {
            case Alignment.Bottom_Left:
                return new Point(r.Left, r.Bottom);

            case Alignment.Bottom_Right:
                return new Point(r.Right, r.Bottom);

            case Alignment.Top_Left:
                return new Point(r.Left, r.Top);

            case Alignment.Top_Right:
                return new Point(r.Right, r.Top);

            case Alignment.Bottom_HorizontalCenter:
                return new Point((int)(r.Left + (r.Width / 2.0)), r.Bottom);

            case Alignment.Top_HorizontalCenter:
                return new Point((int)(r.Left + (r.Width / 2.0)), r.Top);

            case Alignment.VerticalCenter_Left:
                return new Point(r.Left, (int)(r.Top + (r.Height / 2.0)));

            case Alignment.VerticalCenter_Right:
                return new Point(r.Right, (int)(r.Top + (r.Height / 2.0)));

            case Alignment.Horizontal_Vertical_Center:
                return new Point((int)(r.Left + (r.Width / 2.0)), (int)(r.Top + (r.Height / 2.0)));

            default:
                Develop.DebugPrint(p);
                return Point.Empty;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="zoom"></param>
    /// <param name="shiftX"></param>
    /// <param name="shiftY"></param>
    /// <param name="outerLine">true = die Punkte komplett umschlossen (für Fills), false = Mitte der Punkte</param>
    /// <returns></returns>
    public static RectangleF ZoomAndMoveRect(this RectangleF r, float zoom, float shiftX, float shiftY, bool outerLine) {
        if (outerLine) {
            // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
            //               0 und 25 rauskommen
            return new RectangleF((r.X * zoom) - shiftX,
                (r.Y * zoom) - shiftY,
                r.Width * zoom,
                r.Height * zoom);
        }

        // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
        //               2,5 und 27,5 rauskommen
        var add = zoom / 2;
        return new RectangleF((r.X * zoom) - shiftX + add,
            (r.Y * zoom) - shiftY + add,
            r.Width * zoom,
            r.Height * zoom);
    }

    #endregion
}