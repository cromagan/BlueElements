// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace BlueBasics.ClassesStatic;

public static class GraphicsPaths {

    #region Fields

    private static readonly ConcurrentDictionary<(Contour, int, int), GraphicsPath> _contourCache = new();

    #endregion

    #region Methods

    public static void ClearAll() {
        foreach (var path in _contourCache.Values) { path.Dispose(); }
        _contourCache.Clear();
    }

    public static GraphicsPath? GetContour(Contour contour, int w, int h) {
        if (contour is Contour.None or Contour.Undefined || w < 1 || h < 1) { return null; }
        return _contourCache.GetOrAdd((contour, w, h), static k => BuildPath(k.Item1, k.Item2, k.Item3));
    }

    public static void TrimCaches(int maxContours = 500) => TrimDictionary(_contourCache, maxContours);

    public static GraphicsPath Arrow(Rectangle rect) {
        var p = new GraphicsPath();
        // --------+  >
        //         | /
        //         |/
        //
        var plusOben = new PointF((float)(rect.Left + rect.Width * 0.5),
            (float)(rect.PointOf(Alignment.VerticalCenter_Right).Y - rect.Height * 0.18));
        var plusUnten = new PointF((float)(rect.Left + rect.Width * 0.5),
            (float)(rect.PointOf(Alignment.VerticalCenter_Right).Y + rect.Height * 0.18));
        p.AddLine(rect.PointOf(Alignment.VerticalCenter_Right), plusUnten with { Y = rect.Bottom });
        p.AddLine(p.GetLastPoint(), plusUnten);
        p.AddLine(p.GetLastPoint(), plusUnten with { X = rect.Left });
        p.AddLine(p.GetLastPoint(), plusOben with { X = rect.Left });
        p.AddLine(p.GetLastPoint(), plusOben);
        p.AddLine(p.GetLastPoint(), plusOben with { Y = rect.Top });
        p.CloseFigure();

        return p;
    }

    public static GraphicsPath Bruchlinie(Rectangle rect) {
        var p = new GraphicsPath();
        p.AddLine(rect.PointOf(Alignment.Top_Left), rect.PointOf(Alignment.Top_Right));
        p.AddLine(p.GetLastPoint(), rect.PointOf(Alignment.Bottom_Right));
        p.AddLine(p.GetLastPoint(), rect.PointOf(Alignment.Bottom_Left));
        var versX = rect.Width / 6;
        var versY = -rect.Height / 10;
        var pu = p.GetLastPoint();
        for (var z = 0; z < 10; z++) {
            pu.Y += versY;
            pu.X += versX;
            versX *= -1;
            p.AddLine(p.GetLastPoint(), pu);
        }
        p.CloseFigure();
        return p;
    }

    public static GraphicsPath Rechteck(Rectangle rect) {
        var tempPolyRechteck = new GraphicsPath();
        tempPolyRechteck.AddRectangle(rect);
        tempPolyRechteck.CloseFigure();
        return tempPolyRechteck;
    }

    public static GraphicsPath? RoundRec(Rectangle rect, int radius) {
        if (rect.Width < 1 || rect.Height < 1) { return null; }

        if (radius > rect.Height / 2.0 + 2) { radius = (int)(rect.Height / 2.0) + 2; }
        if (radius > rect.Width / 2.0 + 2) { radius = (int)(rect.Width / 2.0) + 2; }

        if (radius < 1) { return Rechteck(rect); }

        var tempPolyRoundRec = new GraphicsPath();

        tempPolyRoundRec.AddLine(rect.X + radius, rect.Y, rect.X + rect.Width - radius, rect.Y);
        tempPolyRoundRec.AddArc(rect.X + rect.Width - (radius * 2), rect.Y, radius * 2, radius * 2, 270, 90);
        tempPolyRoundRec.AddLine(rect.X + rect.Width, rect.Y + radius, rect.X + rect.Width, rect.Y + rect.Height - radius);
        tempPolyRoundRec.AddArc(rect.X + rect.Width - (radius * 2), rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
        tempPolyRoundRec.AddLine(rect.X + rect.Width - radius, rect.Y + rect.Height, rect.X + radius, rect.Y + rect.Height);
        tempPolyRoundRec.AddArc(rect.X, rect.Y + rect.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
        tempPolyRoundRec.AddLine(rect.X, rect.Y + rect.Height - radius, rect.X, rect.Y + radius);
        tempPolyRoundRec.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        tempPolyRoundRec.CloseFigure();
        return tempPolyRoundRec;
    }

    public static GraphicsPath Triangle(PointF p1, PointF p2, PointF p3) {
        var p = new GraphicsPath();
        p.AddLine(p1, p2);
        p.AddLine(p2, p3);
        p.CloseFigure();
        return p;
    }

    #endregion

    #region Private Methods

    private static GraphicsPath BuildPath(Contour contour, int w, int h) {
        var r = new Rectangle(0, 0, w, h);
        return contour switch {
            Contour.RoundedRectThin => RoundRec(r, 2) ?? Rechteck(r),
            Contour.RoundedRect => RoundRec(r, 4) ?? Rechteck(r),
            Contour.Arrow => Arrow(r),
            Contour.Bruchlinie => Bruchlinie(r),
            _ => Rechteck(r)
        };
    }

    private static void TrimDictionary<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, int maxItems) where TKey : notnull {
        var excess = dict.Count - maxItems;
        if (excess <= 0) { return; }
        foreach (var key in dict.Keys.Take(excess).ToList()) {
            if (dict.TryRemove(key, out var value) && value is IDisposable d) { d.Dispose(); }
        }
    }

    #endregion
}
