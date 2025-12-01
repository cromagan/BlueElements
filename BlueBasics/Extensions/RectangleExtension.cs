// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="scale"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <param name="outerLine">true = die Punkte komplett umschlossen (für Fills), false = Mitte der Punkte</param>
    /// <returns></returns>
    public static Rectangle CanvasToControl(this Rectangle rect, float scale, float offsetX, float offsetY, bool outerLine) {
        if (outerLine) {
            // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
            //               0 und 25 rauskommen
            return new Rectangle(rect.X.CanvasToControl(scale) + (int)offsetX,
                                  rect.Y.CanvasToControl(scale) + (int)offsetY,
                                  rect.Width.CanvasToControl(scale),
                                  rect.Height.CanvasToControl(scale));
        }

        // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
        //               2,5 und 27,5 rauskommen
        return new Rectangle(rect.X.CanvasToControl(scale, offsetX),
                              rect.Y.CanvasToControl(scale, offsetY),
                              rect.Width.CanvasToControl(scale),
                              rect.Height.CanvasToControl(scale));
    }

    public static PointF NearestCornerOf(this Rectangle r, Point p) {
        //TODO: Unused
        List<Point> pl =
        [
            r.PointOf(Alignment.Top_Left),
            r.PointOf(Alignment.Top_Right),
            r.PointOf(Alignment.Bottom_Right),
            r.PointOf(Alignment.Bottom_Left)
        ];
        return p.NearestPoint(pl);
    }

    public static Point NearestPoint(this Point p, List<Point> pl) {
        if (pl.Count == 0) { return Point.Empty; }
        var minl = float.MaxValue;
        var rp = Point.Empty;

        foreach (var thisP in pl) {
            var l = Geometry.GetLength(p, thisP);
            if (l > minl) { continue; }

            minl = l;
            rp = thisP;
        }

        return rp;
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

    #endregion
}