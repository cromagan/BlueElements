// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace BlueBasics.ClassesStatic;

public static class BorderDraw {

    #region Fields

    private static readonly ConcurrentDictionary<BorderGradientKey, LinearGradientBrush> _borderGradientCache = new();
    private static readonly ConcurrentDictionary<int, Pen> _dottedPenCache = new();
    private static readonly ConcurrentDictionary<(int, float), Pen> _penCache = new();

    #endregion

    #region Methods

    public static void ClearAll() {
        foreach (var pen in _dottedPenCache.Values) { pen.Dispose(); }
        _dottedPenCache.Clear();
        foreach (var brush in _borderGradientCache.Values) { brush.Dispose(); }
        _borderGradientCache.Clear();
    }

    public static LinearGradientBrush GetBorderGradientBrush(Color c1, Color c2, int height) => _borderGradientCache.GetOrAdd(new BorderGradientKey(c1.ToArgb(), c2.ToArgb(), height), static k => new LinearGradientBrush(new Point(0, 0), new Point(0, k.H), Color.FromArgb(k.C1), Color.FromArgb(k.C2)) { GammaCorrection = true });

    public static Pen GetDottedPen(Color color) => _dottedPenCache.GetOrAdd(color.ToArgb(), static argb => new Pen(Color.FromArgb(argb)) { DashStyle = DashStyle.Dot });

    public static Pen GetPen(Color color, float width) => _penCache.GetOrAdd((color.ToArgb(), width), key => new Pen(color, width));

    public static void TrimCaches(int maxDottedPens = 50, int maxBorderGradients = 200) {
        TrimDictionary(_dottedPenCache, maxDottedPens);
        TrimDictionary(_borderGradientCache, maxBorderGradients);
    }

    public static void FocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor2) {
        var innerW = lr.Width - 6;
        var innerH = lr.Height - 6;
        if (innerW > 0 && innerH > 0) {
            var pen = GetDottedPen(borderColor2);
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, new Rectangle(3, 3, innerW, innerH));
            } else {
                var innerPath = GraphicsPaths.GetContour(contour, innerW, innerH);
                if (innerPath != null) {
                    gr.TranslateTransform(3, 3);
                    gr.DrawPath(pen, innerPath);
                    gr.TranslateTransform(-3, -3);
                }
            }
        }
    }

    public static void Solid1Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 1);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    public static void Solid1PxDualColor(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(GetPen(borderColor1, 1), lr);
        } else {
            var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(GetPen(borderColor1, 1), path); }
        }
        var lgb = GetBorderGradientBrush(borderColor1, borderColor2, lr.Height);
        gr.FillRectangle(lgb, 0, 0, lr.Width + 1, 2);
        gr.FillRectangle(lgb, 0, lr.Height - 1, lr.Width + 1, 2);
        gr.FillRectangle(lgb, 0, 0, 2, lr.Height + 1);
        gr.FillRectangle(lgb, lr.Width - 1, 0, 2, lr.Height + 1);
    }

    public static void Solid1PxFocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(GetPen(borderColor1, 1), lr);
        } else {
            var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(GetPen(borderColor1, 1), path); }
        }
        var innerW = lr.Width - 6;
        var innerH = lr.Height - 6;
        if (innerW > 0 && innerH > 0) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(GetDottedPen(borderColor2), new Rectangle(3, 3, innerW, innerH));
            } else {
                var innerPath = GraphicsPaths.GetContour(contour, innerW, innerH);
                if (innerPath != null) {
                    gr.TranslateTransform(3, 3);
                    gr.DrawPath(GetDottedPen(borderColor2), innerPath);
                    gr.TranslateTransform(-3, -3);
                }
            }
        }
    }

    public static void Solid21Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 21);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    public static void Solid3Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 3);
        if (contour == Contour.Rectangle) {
            gr.DrawRectangle(pen, lr);
        } else {
            var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
            if (path != null) { gr.DrawPath(pen, path); }
        }
    }

    #endregion

    #region Private Methods

    private static void TrimDictionary<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, int maxItems) where TKey : notnull {
        var excess = dict.Count - maxItems;
        if (excess <= 0) { return; }
        foreach (var key in dict.Keys.Take(excess).ToList()) {
            if (dict.TryRemove(key, out var value) && value is IDisposable d) { d.Dispose(); }
        }
    }

    #endregion

    private readonly record struct BorderGradientKey(int C1, int C2, int H);
}
