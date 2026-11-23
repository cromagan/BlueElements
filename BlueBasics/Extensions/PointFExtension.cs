// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Point CanvasToControl(this PointF p, float zoom, float offsetX, float offsetY) => new(p.X.CanvasToControl(zoom, offsetX), p.Y.CanvasToControl(zoom, offsetY));


    public static bool PointInRect(this PointF p, float x1, float y1, float x2, float y2, float toleranz) {
        var r = new RectangleF(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        r.Inflate(toleranz, toleranz);
        return r.Contains(p);
    }

    #endregion
}