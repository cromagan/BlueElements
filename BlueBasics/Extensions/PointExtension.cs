// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static PointF ControlToCanvas(this Point p, float zoom, float offsetX, float offsetY) => new(p.X.ControlToCanvas(zoom, offsetX), p.Y.ControlToCanvas(zoom, offsetY));


    #endregion
}
