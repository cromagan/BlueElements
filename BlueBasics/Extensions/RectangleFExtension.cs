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

#nullable enable

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
    public static Rectangle CanvasToControl(this RectangleF rect, float scale, float offsetX, float offsetY, bool outerLine) {
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

    /// <summary>
    /// Erweitert das Rechteck, dass der Angegebene Punkt ebenfalls umschlossen wird.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="p"></param>
    public static RectangleF ExpandTo(this RectangleF r, PointF p) {
        if (p.X < r.X) {
            r.Width = r.Right - p.X;
            r.X = p.X;
        }
        if (p.Y < r.Y) {
            r.Height = r.Bottom - p.Y;
            r.Y = p.Y;
        }
        if (p.X > r.Right) {
            r.Width = p.X - r.X;
        }
        if (p.Y > r.Bottom) {
            r.Height = p.Y - r.Y;
        }

        return r;
    }

    /// <summary>
    /// Erweitert das Rechteck, dass ein Kreis mit den angegebenen Parametern ebenfalls umschlossen wird.
    /// </summary>
    public static RectangleF ExpandTo(this RectangleF r, PointF middle, float radius) {
        r = r.ExpandTo(middle with { Y = middle.Y + radius });
        r = r.ExpandTo(middle with { Y = middle.Y - radius });
        r = r.ExpandTo(middle with { X = middle.X + radius });
        r = r.ExpandTo(middle with { X = middle.X - radius });
        return r;
    }

    public static bool IntersectsVericalyWith(this RectangleF r, RectangleF rect) => rect.X < r.X + r.Width && r.X < rect.X + rect.Width;

    public static PointF NearestCornerOf(this RectangleF r, PointF p) {
        //TODO: Unused
        List<PointF> pl =
        [
            r.PointOf(Alignment.Top_Left),
            r.PointOf(Alignment.Top_Right),
            r.PointOf(Alignment.Bottom_Right),
            r.PointOf(Alignment.Bottom_Left)
        ];
        return p.NearestPoint(pl);
    }

    /// <summary>
    /// Gibt den Punkt, der am nähesten zu einem der vier Mittelpunkte der Strecken ist, zurück
    /// </summary>
    /// <param name="r"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static PointF NearestLineMiddle(this RectangleF r, PointF p) {
        List<PointF> pl =
        [
            r.PointOf(Alignment.Bottom_HorizontalCenter),
            r.PointOf(Alignment.Top_HorizontalCenter),
            r.PointOf(Alignment.VerticalCenter_Left),
            r.PointOf(Alignment.VerticalCenter_Right)
        ];
        return p.NearestPoint(pl);
    }

    public static PointF PointOf(this RectangleF r, Alignment p) {
        switch (p) {
            case Alignment.Bottom_Left:
                return new PointF(r.Left, r.Bottom);

            case Alignment.Bottom_Right:
                return new PointF(r.Right, r.Bottom);

            case Alignment.Top_Left:
                return new PointF(r.Left, r.Top);

            case Alignment.Top_Right:
                return new PointF(r.Right, r.Top);

            case Alignment.Bottom_HorizontalCenter:
                return new PointF((int)(r.Left + (r.Width / 2.0)), r.Bottom);

            case Alignment.Top_HorizontalCenter:
                return new PointF((int)(r.Left + (r.Width / 2.0)), r.Top);

            case Alignment.VerticalCenter_Left:
                return new PointF(r.Left, (int)(r.Top + (r.Height / 2.0)));

            case Alignment.VerticalCenter_Right:
                return new PointF(r.Right, (int)(r.Top + (r.Height / 2.0)));

            case Alignment.Horizontal_Vertical_Center:
                return new Point((int)(r.Left + (r.Width / 2.0)), (int)(r.Top + (r.Height / 2.0)));

            default:
                Develop.DebugPrint(p);
                return Point.Empty;
        }
    }

    public static Rectangle ToRect(this RectangleF r) => new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

    #endregion
}