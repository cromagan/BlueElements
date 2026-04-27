// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Point CanvasToControl(this PointF p, float zoom, float offsetX, float offsetY) => new(p.X.CanvasToControl(zoom, offsetX), p.Y.CanvasToControl(zoom, offsetY));

    public static PointF NearestPoint(this PointF p, List<PointF> pl) {
        if (pl.Count == 0) { return PointF.Empty; }
        var minl = float.MaxValue;
        var rp = PointF.Empty;

        foreach (var thisP in pl) {
            var l = Geometry.GetLength(p, thisP);
            if (l > minl) { continue; }

            minl = l;
            rp = thisP;
        }

        return rp;
    }

    public static bool PointInRect(this PointF p, float x1, float y1, float x2, float y2, float toleranz) {
        var r = new RectangleF(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        r.Inflate(toleranz, toleranz);
        return r.Contains(p);
    }

    #endregion
}