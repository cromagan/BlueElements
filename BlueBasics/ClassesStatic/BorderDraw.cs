// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using System.Drawing.Drawing2D;

namespace BlueBasics.ClassesStatic;

public static class BorderDraw {


    #region Fields

    private static readonly ConcurrentCache<BorderGradientKey, LinearGradientBrush> _borderGradientCache = new(200);
    private static readonly ConcurrentCache<int, Pen> _dottedPenCache = new(50);
    private static readonly ConcurrentCache<(int, float), Pen> _penCache = new(200);
    #endregion

    #region Methods

    public static void ClearAll() {
        _dottedPenCache.Clear();
        _borderGradientCache.Clear();
        _penCache.Clear();
    }

    public static void FocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor2) {
        var innerW = lr.Width - 6;
        var innerH = lr.Height - 6;
        if (innerW > 0 && innerH > 0) {
            var pen = GetDottedPen(borderColor2);
            lock (pen) {
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
    }

    public static LinearGradientBrush GetBorderGradientBrush(Color c1, Color c2, int height) => _borderGradientCache.GetOrAdd(new BorderGradientKey(c1.ToArgb(), c2.ToArgb(), height), static k => new LinearGradientBrush(new Point(0, 0), new Point(0, k.H), Color.FromArgb(k.C1), Color.FromArgb(k.C2)) { GammaCorrection = true });

    public static Pen GetDottedPen(Color color) => _dottedPenCache.GetOrAdd(color.ToArgb(), static argb => new Pen(Color.FromArgb(argb)) { DashStyle = DashStyle.Dot });

    public static Pen GetPen(Color color, float width) => _penCache.GetOrAdd((color.ToArgb(), width), key => new Pen(color, width));

    public static void Solid1Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 1);
        lock (pen) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.DrawPath(pen, path); }
            }
        }
    }

    public static void Solid1PxDualColor(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        var pen = GetPen(borderColor1, 1);
        lock (pen) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.DrawPath(pen, path); }
            }
            var lgb = GetBorderGradientBrush(borderColor1, borderColor2, lr.Height);
            lock (lgb) {
                gr.FillRectangle(lgb, 0, 0, lr.Width + 1, 2);
                gr.FillRectangle(lgb, 0, lr.Height - 1, lr.Width + 1, 2);
                gr.FillRectangle(lgb, 0, 0, 2, lr.Height + 1);
                gr.FillRectangle(lgb, lr.Width - 1, 0, 2, lr.Height + 1);
            }
        }
    }

    public static void Solid1PxFocusDot(Graphics gr, Contour contour, Rectangle lr, Color borderColor1, Color borderColor2) {
        var pen = GetPen(borderColor1, 1);
        var dottedPen = GetDottedPen(borderColor2);
        lock (pen) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.DrawPath(pen, path); }
            }
            lock (dottedPen) {
                var innerW = lr.Width - 6;
                var innerH = lr.Height - 6;
                if (innerW > 0 && innerH > 0) {
                    if (contour == Contour.Rectangle) {
                        gr.DrawRectangle(dottedPen, new Rectangle(3, 3, innerW, innerH));
                    } else {
                        var innerPath = GraphicsPaths.GetContour(contour, innerW, innerH);
                        if (innerPath != null) {
                            gr.TranslateTransform(3, 3);
                            gr.DrawPath(dottedPen, innerPath);
                            gr.TranslateTransform(-3, -3);
                        }
                    }
                }
            }
        }
    }

    public static void Solid21Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 21);
        lock (pen) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.DrawPath(pen, path); }
            }
        }
    }

    public static void Solid3Px(Graphics gr, Contour contour, Rectangle lr, Color borderColor1) {
        var pen = GetPen(borderColor1, 3);
        lock (pen) {
            if (contour == Contour.Rectangle) {
                gr.DrawRectangle(pen, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.DrawPath(pen, path); }
            }
        }
    }

    #endregion

    private readonly record struct BorderGradientKey(int C1, int C2, int H);
}