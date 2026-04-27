// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
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
    /// <param name="zoom"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <param name="outerLine">true = die Punkte komplett umschlossen (für Fills), false = Mitte der Punkte</param>
    /// <returns></returns>
    public static Rectangle CanvasToControl(this Rectangle rect, float zoom, float offsetX, float offsetY, bool outerLine) {
        if (outerLine) {
            // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
            //               0 und 25 rauskommen
            return new Rectangle(rect.X.CanvasToControl(zoom) + (int)offsetX,
                                  rect.Y.CanvasToControl(zoom) + (int)offsetY,
                                  rect.Width.CanvasToControl(zoom),
                                  rect.Height.CanvasToControl(zoom));
        }

        // Beispiel: bei X=0 und Width=5 muss bei einen zoom von 5
        //               2,5 und 27,5 rauskommen
        return new Rectangle(rect.X.CanvasToControl(zoom, offsetX),
                              rect.Y.CanvasToControl(zoom, offsetY),
                              rect.Width.CanvasToControl(zoom),
                              rect.Height.CanvasToControl(zoom));
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