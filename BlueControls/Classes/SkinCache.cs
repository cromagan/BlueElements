// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;

namespace BlueControls.Classes;

public static class SkinCache {

    #region Fields

    private static readonly ConcurrentDictionary<BorderGradientKey, LinearGradientBrush> _borderGradientCache = new();
    private static readonly ConcurrentDictionary<(Contour, int, int), System.Drawing.Drawing2D.GraphicsPath> _contourCache = new();
    private static readonly ConcurrentDictionary<int, Pen> _dottedPenCache = new();
    private static readonly ConcurrentDictionary<GradientKey, LinearGradientBrush> _gradientCache = new();
    private static Brush? _deleteBackBrush;

    #endregion

    #region Properties

    public static Brush DeleteBackBrush => _deleteBackBrush ??= BlueFont.GetBrush(Color.FromArgb(220, 255, 255, 255));

    #endregion

    #region Methods

    public static void ClearAll() {
        foreach (var path in _contourCache.Values) { path.Dispose(); }
        _contourCache.Clear();
        foreach (var brush in _gradientCache.Values) { brush.Dispose(); }
        _gradientCache.Clear();
        foreach (var pen in _dottedPenCache.Values) { pen.Dispose(); }
        _dottedPenCache.Clear();
        foreach (var brush in _borderGradientCache.Values) { brush.Dispose(); }
        _borderGradientCache.Clear();
        _deleteBackBrush = null;
    }

    public static LinearGradientBrush GetBorderGradientBrush(Color c1, Color c2, int height) => _borderGradientCache.GetOrAdd(new BorderGradientKey(c1.ToArgb(), c2.ToArgb(), height), static k => new LinearGradientBrush(new Point(0, 0), new Point(0, k.H), Color.FromArgb(k.C1), Color.FromArgb(k.C2)) { GammaCorrection = true });

    public static System.Drawing.Drawing2D.GraphicsPath? GetContour(Contour contour, int w, int h) {
        if (contour is Contour.None or Contour.Undefined || w < 1 || h < 1) { return null; }
        return _contourCache.GetOrAdd((contour, w, h), static k => BuildPath(k.Item1, k.Item2, k.Item3));
    }

    public static Pen GetDottedPen(Color color) => _dottedPenCache.GetOrAdd(color.ToArgb(), static argb => new Pen(Color.FromArgb(argb)) { DashStyle = DashStyle.Dot });

    public static LinearGradientBrush GetGradient(BackgroundStyle style, Color c1, Color c2, Color c3, int w, int h, float mp) => _gradientCache.GetOrAdd(new GradientKey(style, c1.ToArgb(), c2.ToArgb(), c3.ToArgb(), w, h, (int)(mp * 1000)), static k => CreateGradient(k));

    public static void TrimCaches(int maxContours = 500, int maxGradients = 500, int maxDottedPens = 50, int maxBorderGradients = 200) {
        TrimDictionary(_contourCache, maxContours);
        TrimDictionary(_gradientCache, maxGradients);
        TrimDictionary(_dottedPenCache, maxDottedPens);
        TrimDictionary(_borderGradientCache, maxBorderGradients);
    }

    private static System.Drawing.Drawing2D.GraphicsPath BuildPath(Contour contour, int w, int h) {
        var r = new Rectangle(0, 0, w, h);
        return contour switch {
            Contour.RoundedRectThin => GraphicsPaths.RoundRec(r, 2) ?? GraphicsPaths.Rechteck(r),
            Contour.RoundedRect => GraphicsPaths.RoundRec(r, 4) ?? GraphicsPaths.Rechteck(r),
            _ => GraphicsPaths.Rechteck(r)
        };
    }

    private static LinearGradientBrush CreateGradient(GradientKey k) {
        var rect = new Rectangle(0, 0, k.W, k.H);
        var c1 = Color.FromArgb(k.C1);
        var c2 = Color.FromArgb(k.C2);
        var c3 = Color.FromArgb(k.C3);
        var mp = k.Mp / 1000f;

        return k.Style switch {
            BackgroundStyle.GradientVertical => new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical),
            BackgroundStyle.GradientVertical3 => ThreeColorBrush(rect, c1, c2, c3, mp, LinearGradientMode.Vertical),
            BackgroundStyle.GradientHorizontal => new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Horizontal),
            BackgroundStyle.GradientHorizontal3 => ThreeColorBrush(rect, c1, c2, c3, mp, LinearGradientMode.Horizontal),
            BackgroundStyle.GradientDiagonal => DiagonalBrush(rect, c1, c2, c3, mp),
            BackgroundStyle.Glossy => GlossyBrush(rect, c1, c2, c3),
            BackgroundStyle.GlossyPressed => GlossyPressedBrush(rect, c1, c2, c3),
            BackgroundStyle.GradientVerticalHighlight => HighlightBrush(rect, c1, c2, c3, mp),
            _ => new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical)
        };
    }

    private static LinearGradientBrush DiagonalBrush(Rectangle rect, Color c1, Color c2, Color c3, float mp) {
        var b = new LinearGradientBrush(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom), c1, c2) {
            InterpolationColors = new ColorBlend { Colors = [c1, c2, c3], Positions = [0f, mp, 1f] },
            GammaCorrection = true
        };
        return b;
    }

    private static LinearGradientBrush GlossyBrush(Rectangle rect, Color c1, Color c2, Color c3) {
        var b = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical) {
            InterpolationColors = new ColorBlend { Colors = [c1, c2, c3], Positions = [0f, 0.4f, 1f] },
            GammaCorrection = true
        };
        return b;
    }

    private static LinearGradientBrush GlossyPressedBrush(Rectangle rect, Color c1, Color c2, Color c3) {
        var b = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical) {
            InterpolationColors = new ColorBlend { Colors = [c1, c2, c3], Positions = [0f, 0.6f, 1f] },
            GammaCorrection = true
        };
        return b;
    }

    private static LinearGradientBrush HighlightBrush(Rectangle rect, Color c1, Color c2, Color c3, float mp) {
        var b = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical) {
            InterpolationColors = new ColorBlend { Colors = [c1, c2, c3], Positions = [0f, mp, 1f] },
            GammaCorrection = true
        };
        return b;
    }

    private static LinearGradientBrush ThreeColorBrush(Rectangle rect, Color c1, Color c2, Color c3, float mp, LinearGradientMode mode) {
        var b = new LinearGradientBrush(rect, c1, c2, mode) {
            InterpolationColors = new ColorBlend { Colors = [c1, c2, c3], Positions = [0f, mp, 1f] },
            GammaCorrection = true
        };
        return b;
    }

    private static void TrimDictionary<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, int maxItems) where TKey : notnull {
        var excess = dict.Count - maxItems;
        if (excess <= 0) { return; }
        foreach (var key in dict.Keys.Take(excess).ToList()) {
            if (dict.TryRemove(key, out var value) && value is IDisposable d) { d.Dispose(); }
        }
    }

    #endregion

    private readonly record struct GradientKey(BackgroundStyle Style, int C1, int C2, int C3, int W, int H, int Mp);
    private readonly record struct BorderGradientKey(int C1, int C2, int H);
}