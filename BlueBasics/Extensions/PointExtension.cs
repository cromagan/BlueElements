// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static PointF ControlToCanvas(this Point p, float zoom, float offsetX, float offsetY) => new(p.X.ControlToCanvas(zoom, offsetX), p.Y.ControlToCanvas(zoom, offsetY));

    public static Point PointParse(this string? toParse) {
        if (string.IsNullOrEmpty(toParse)) { return Point.Empty; }

        toParse = toParse.RemoveChars("{}XYxy= ");
        var span = toParse.AsSpan();
        var commaIdx = span.IndexOf(',');
        if (commaIdx < 0) { return Point.Empty; }

        if (!int.TryParse(span[..commaIdx], out var x)) { x = 0; }
        if (!int.TryParse(span[(commaIdx + 1)..], out var y)) { y = 0; }
        return new Point(x, y);
    }

    #endregion
}