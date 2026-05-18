// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using System.Drawing.Drawing2D;

namespace BlueBasics.ClassesStatic;

public static class BackgroundFill {

    #region Fields

    private static readonly ConcurrentCache<GradientKey, LinearGradientBrush> _gradientCache = new(500);
    private static readonly ConcurrentCache<int, SolidBrush> _brushCache = new(500);
    private static Brush? _deleteBackBrush;

    #endregion

    #region Properties

    public static Brush DeleteBackBrush => _deleteBackBrush ??= new SolidBrush(Color.FromArgb(220, 255, 255, 255));

    #endregion

    #region Methods

    public static void ClearAll() {
        _gradientCache.Clear();
        _brushCache.Clear();
        _deleteBackBrush = null;
    }


    public static Brush GetBrush(Color color) => _brushCache.GetOrAdd(color.ToArgb(), _ => new SolidBrush(color));

    public static LinearGradientBrush GetGradient(BackgroundStyle style, Color c1, Color c2, Color c3, int w, int h, float mp) => _gradientCache.GetOrAdd(new GradientKey(style, c1.ToArgb(), c2.ToArgb(), c3.ToArgb(), w, h, (int)(mp * 1000)), static k => CreateGradient(k));

    public static void Glossy(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var c3 = Color.FromArgb(180, backColor2);
        var brush = GetGradient(BackgroundStyle.Glossy, backColor1, backColor2, c3, lr.Width, lr.Height, 0);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GlossyPressed(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var c3 = Color.FromArgb(180, backColor1);
        var brush = GetGradient(BackgroundStyle.GlossyPressed, backColor2, backColor1, c3, lr.Width, lr.Height, 0);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientDiagonal(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = GetGradient(BackgroundStyle.GradientDiagonal, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientHorizontal(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var brush = GetGradient(BackgroundStyle.GradientHorizontal, backColor1, backColor2, Color.Empty, lr.Width, lr.Height, 0);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientHorizontal3(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = GetGradient(BackgroundStyle.GradientHorizontal3, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientVertical(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2) {
        var brush = GetGradient(BackgroundStyle.GradientVertical, backColor1, backColor2, Color.Empty, lr.Width, lr.Height, 0);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientVertical3(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, Color backColor3, float gradientMidpoint) {
        var brush = GetGradient(BackgroundStyle.GradientVertical3, backColor1, backColor2, backColor3, lr.Width, lr.Height, gradientMidpoint);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void GradientVerticalHighlight(Graphics gr, Contour contour, Rectangle lr, Color backColor1, Color backColor2, float gradientMidpoint) {
        var brush = GetGradient(BackgroundStyle.GradientVerticalHighlight, backColor1, Color.White, backColor2, lr.Width, lr.Height, gradientMidpoint);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    public static void Solid(Graphics gr, Contour contour, Rectangle lr, Color backColor1) {
        var brush = GetBrush(backColor1);
        lock (brush) {
            if (contour == Contour.Rectangle) {
                gr.FillRectangle(brush, lr);
            } else {
                var path = GraphicsPaths.GetContour(contour, lr.Width, lr.Height);
                if (path != null) { gr.FillPath(brush, path); }
            }
        }
    }

    #endregion

    #region Private Methods

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

    #endregion

    private readonly record struct GradientKey(BackgroundStyle Style, int C1, int C2, int C3, int W, int H, int Mp);
}
