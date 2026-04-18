// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics.Classes.BitmapExt_ImageFilters;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static void ApplyFilter(this Bitmap bitmap, params (ImageFilter filter, object? parameter)[] filterWithParams) {
        if (bitmap == null) { return; }
        foreach (var (filter, parameter) in filterWithParams) {
            filter.Parameter = parameter;
            filter.ProcessFilter(bitmap);
        }
    }

    public static void ApplyFilter(this Bitmap bitmap, params ImageFilter[] filters) {
        if (bitmap == null) { return; }
        foreach (var filter in filters) {
            filter.ProcessFilter(bitmap);
        }
    }

    public static Bitmap CloneFromBitmap(this Bitmap sourceBmp) {
        try {
            var bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height);
            using var gr = Graphics.FromImage(bmp);
            gr.DrawImage(sourceBmp, new Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height));
            return bmp;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return new Bitmap(1, 1);
        }
    }

    public static Bitmap Crop(this Bitmap sourceBmp, Rectangle re) {
        var bmp = new Bitmap(re.Width, re.Height);
        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.Transparent);
        gr.PixelOffsetMode = PixelOffsetMode.Half;
        gr.DrawImage(sourceBmp, re with { X = 0, Y = 0 }, re.Left, re.Top, re.Width, re.Height, GraphicsUnit.Pixel);
        return bmp;
    }

    public static Bitmap Crop(this Bitmap sourceBmp, int x, int y, int width, int height) {
        if (x < 0) { x = 0; }
        if (y < 0) { y = 0; }
        if (x + width > sourceBmp.Width) { width = sourceBmp.Width - x; }
        if (y + height > sourceBmp.Height) { height = sourceBmp.Height - y; }
        return sourceBmp.Crop(new Rectangle(x, y, width, height));
    }

    public static Bitmap? CropStatic(Bitmap? sourceBmp, int left, int right, int top, int bottom) {
        if (sourceBmp == null || left == 0 && right == 0 && top == 0 && bottom == 0) { return sourceBmp; }

        Generic.CollectGarbage();
        var w = Math.Max(sourceBmp.Width - left + right, 1);
        var h = Math.Max(sourceBmp.Height - top + bottom, 1);
        var bmp = new Bitmap(w, h);
        using (var gr = Graphics.FromImage(bmp)) {
            gr.DrawImage(sourceBmp, -left, -top, sourceBmp.Width, sourceBmp.Height);
        }
        Generic.CollectGarbage();
        return bmp;
    }

    public static void FillCircle(this Bitmap sourceBmp, Color c, int x, int y, int r) {
        for (var adx = -r; adx <= r; adx++) {
            for (var ady = -r; ady <= r; ady++) {
                var d = Math.Sqrt(Convert.ToDouble(adx * adx + ady * ady)) - 0.5;
                var px = x + adx;
                var py = y + ady;
                if (px >= 0 && py >= 0 && px < sourceBmp.Width && py < sourceBmp.Height && d <= r) {
                    sourceBmp.SetPixel(px, py, c);
                }
            }
        }
    }

    public static Padding GetAutoValuesForCrop(this Bitmap sourceBmp, double minBrightness) {
        var pa = new Padding(0, 0, 0, 0);
        var x = 0;
        var exitNow = false;
        int y;
        while (true) {
            for (y = 0; y < sourceBmp.Height; y++) {
                if (!sourceBmp.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            x++;
            if (x > sourceBmp.Width * 0.9) { break; }
        }
        pa.Left = x;
        // -------------
        x = sourceBmp.Width - 1;
        exitNow = false;
        while (true) {
            for (y = 0; y < sourceBmp.Height; y++) {
                if (!sourceBmp.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            x--;
            if (x < sourceBmp.Width * 0.1) { break; }
        }
        pa.Right = x - sourceBmp.Width + 1;
        // -------------
        y = 0;
        exitNow = false;
        while (true) {
            for (x = 0; x < sourceBmp.Width; x++) {
                if (!sourceBmp.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            y++;
            if (y > sourceBmp.Height * 0.9) { break; }
        }
        pa.Top = y;
        // -------------
        y = sourceBmp.Height - 1;
        exitNow = false;
        while (true) {
            for (x = 0; x < sourceBmp.Width; x++) {
                if (!sourceBmp.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            y--;
            if (y < sourceBmp.Height * 0.1) { break; }
        }
        pa.Bottom = y - sourceBmp.Height + 1;
        return pa;
    }

    public static Bitmap GetBitmap(this Bitmap sourceBmp, int maxSize) {
        if (maxSize > 0) {
            return sourceBmp.Resize(maxSize, maxSize,
                SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern,
                InterpolationMode.HighQualityBicubic, true);
        }
        return sourceBmp;
    }

    public static float GetBrightness(int r, int g, int b) => (0.3f * r + 0.59f * g + 0.11f * b) / 255.0f;

    public static float GetBrightness(this byte[] bits, int index) =>
        (0.3f * bits[index + 2] + 0.59f * bits[index + 1] + 0.11f * bits[index]) / 255.0f;

    public static Color GetColor(this byte[] bits, int index) => Color.FromArgb(bits[index + 3], bits[index + 2], bits[index + 1], bits[index]);

    public static Bitmap? GetEmmbedBitmap(Assembly assembly, string name) {
        if (name.Contains('|')) { return null; }
        if (name.Contains('[')) { return null; }
        using var d = Generic.GetEmmbedResource(assembly, name);
        if (d == null) { return null; }

        switch (name.FileType()) {
            case FileFormat.Image:
                return new Bitmap(d);

            default:
                Develop.DebugPrint(name.FileType());
                return null;
        }
    }

    public static float GetLuminance(this Color color) => GetBrightness(color.R, color.G, color.B);

    public static Color GetPixel(this BitmapData sourceBmpData, byte[] bits, int x, int y) {
        var index = sourceBmpData.GetPixelIndex(x, y);
        return bits.GetColor(index);
    }

    public static int GetPixelIndex(this BitmapData data, int x, int y) => y * data.Stride + x * 4;

    public static Color GetPixelSafe(this BitmapData? data, byte[]? bits, int x, int y, int w, int h) {
        if (data == null || bits == null || x < 0 || y < 0 || x >= w || y >= h) { return Color.FromArgb(0, 0, 0, 0); }
        return data.GetPixel(bits, x, y);
    }

    public static Color GetPixelSafe(this BitmapData data, byte[] bits, int x, int y) {
        if (x < 0 || y < 0 || x >= data.Width || y >= data.Height) { return Color.FromArgb(0, 0, 0, 0); }
        return data.GetPixel(bits, x, y);
    }

    public static int GetPixelSafeIndex(this BitmapData data, int x, int y, int w, int h) {
        if (x < 0 || y < 0 || x >= w || y >= h) { return -1; }
        return data.GetPixelIndex(x, y);
    }

    public static Bitmap? Image_Clone(Bitmap? sourceBmp) {
        try {
            if (sourceBmp?.IsValid() != true) { return null; }

            var bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.DrawImage(sourceBmp, 0, 0, sourceBmp.Width, sourceBmp.Height);
            return bmp;
        } catch { /* Bitmap-Klon fehlgeschlagen */ }

        return null;
    }

    public static Image? Image_FromFile(string filename) {
        if (string.IsNullOrEmpty(filename)) { return null; }
        if (!FileExists(filename)) { return null; }
        try {
            var bytes = ReadAllBytes(filename, 3).Value as byte[] ?? [];
            using var ms = new System.IO.MemoryStream(bytes);
            var im = Image.FromStream(ms);
            return im;
        } catch {
            return null;
        }
    }

    public static bool IntersectsWith(Bitmap bmp1, Point pos1, Bitmap bmp2, Point pos2, int accuracy) {
        if (bmp1 == null || bmp2 == null) { return false; }
        var koord1 = new Rectangle(pos1, bmp1.Size);
        var koord2 = new Rectangle(pos2, bmp2.Size);
        if (!koord1.IntersectsWith(koord2)) { return false; }
        var schnitt = new Rectangle(koord1.Location, koord1.Size);
        schnitt.Intersect(koord2);
        for (var x = schnitt.Left; x < schnitt.Right; x += accuracy) {
            for (var y = schnitt.Top; y < schnitt.Bottom; y += accuracy) {
                if (!bmp1.GetPixel(x - koord1.X, y - koord1.Y).IsNearWhite(0.9) && !bmp2.GetPixel(x - koord2.X, y - koord2.Y).IsNearWhite(0.9)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static Color Invert(this Color color) => Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);

    public static bool IsDark(this Color color, double threshold = 0.5) => color.A != 0 && color.GetBrightness() < threshold;

    public static bool IsFullyTransparent(this Color color) => color.A == 0;

    public static bool IsLight(this Color color, double threshold = 0.5) => color.A == 0 || color.GetBrightness() >= threshold;

    public static bool IsMagentaOrTransparent(this byte[] bits, int index) => bits[index + 3] == 0 || (bits[index] == 255 && bits[index + 1] == 0 && bits[index + 2] == 255);

    public static bool IsMagentaOrTransparent(this BitmapData data, byte[] bits, int x, int y) =>
        bits.IsMagentaOrTransparent(data.GetPixelIndex(x, y));

    public static bool IsMagentaOrTransparentAt(this BitmapData data, byte[] bits, int x, int y) =>
        x < 0 || y < 0 || x >= data.Width || y >= data.Height || bits.IsMagentaOrTransparent(data.GetPixelIndex(x, y));

    public static bool IsNearBlack(this byte[] bits, int index, double maxBrightness) {
        if (bits[index + 3] == 0) { return false; }
        if (bits[index] == 0 && bits[index + 1] == 0 && bits[index + 2] == 0) { return true; }
        return (0.3f * bits[index + 2] + 0.59f * bits[index + 1] + 0.11f * bits[index]) / 255.0f <= maxBrightness;
    }

    public static bool IsNearBlack(this BitmapData data, byte[] bits, int x, int y, double maxBrightness) =>
        bits.IsNearBlack(data.GetPixelIndex(x, y), maxBrightness);

    public static bool IsNearBlackAt(this BitmapData data, byte[] bits, int x, int y, double maxBrightness) =>
        x < 0 || y < 0 || x >= data.Width || y >= data.Height || bits.IsNearBlack(data.GetPixelIndex(x, y), maxBrightness);

    public static bool IsNearWhite(this byte[] bits, int index, double minBrightness) {
        if (bits[index + 3] == 0) { return true; }
        if (bits[index] == 255 && bits[index + 1] == 255 && bits[index + 2] == 255) { return true; }
        return (0.3f * bits[index + 2] + 0.59f * bits[index + 1] + 0.11f * bits[index]) / 255.0f >= minBrightness;
    }

    public static bool IsNearWhite(this BitmapData data, byte[] bits, int x, int y, double minBrightness) =>
        bits.IsNearWhite(data.GetPixelIndex(x, y), minBrightness);

    public static bool IsNearWhiteAt(this BitmapData data, byte[] bits, int x, int y, double minBrightness) =>
        x < 0 || y < 0 || x >= data.Width || y >= data.Height || bits.IsNearWhite(data.GetPixelIndex(x, y), minBrightness);

    public static bool IsOpaque(this Color color) => color.A == 255;

    public static bool IsValid(this Bitmap? bitmap) {
        if (bitmap == null) { return false; }

        try {
            _ = bitmap.Width;
            _ = bitmap.Height;
            return true;
        } catch {
            return false;
        }
    }

    public static void Magnify(this Bitmap sourceBmp, PointF point, Graphics gr, bool swapX) {
        const int w1 = 200;
        const int w5 = 10;
        int x;
        if (!swapX) {
            x = 150 - (int)(w1 / 2.0);
            if (point.X < sourceBmp.Width / 2.0) { x = sourceBmp.Width - 150 - (int)(w1 / 2.0); }
        } else {
            x = sourceBmp.Width - 150 - (int)(w1 / 2.0);
            if (point.X < sourceBmp.Width / 2.0) { x = 150 - (int)(w1 / 2.0); }
        }
        var y = 150 - (int)(w1 / 2.0);
        if (point.Y < sourceBmp.Height / 2.0) { y = sourceBmp.Height - 150 - (int)(w1 / 2.0); }
        var r = new Rectangle(x, y, w1, w1);
        for (var z = 5; z >= 0; z--) {
            r.Inflate(1, 1);
            var w = Convert.ToByte(255 / (double)10 * z);
            gr.DrawRectangle(new Pen(Color.FromArgb(w, 0, 0, 0)), r);
        }
        r.Inflate(-5, -5);
        gr.InterpolationMode = InterpolationMode.NearestNeighbor;
        gr.PixelOffsetMode = PixelOffsetMode.Half;
        gr.DrawImage(sourceBmp, r, new Rectangle((int)point.X - w5, (int)point.Y - w5, w5 * 2 + 1, w5 * 2 + 1), GraphicsUnit.Pixel);
        gr.DrawRectangle(Pens.Black, r);
        var mitte = r.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X, mitte.Y - 7, mitte.X, mitte.Y + 6);
        gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X - 7, mitte.Y, mitte.X + 6, mitte.Y);
        gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), mitte.X, r.Top, mitte.X, r.Bottom);
        gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, mitte.Y, r.Right, mitte.Y);
        gr.DrawLine(Pens.Red, mitte.X, mitte.Y - 6, mitte.X, mitte.Y + 5);
        gr.DrawLine(Pens.Red, mitte.X - 6, mitte.Y, mitte.X + 5, mitte.Y);
    }

    public static Bitmap? ReplaceColor(this Bitmap sourceBmp, Color toReplace, Color replacement) {
        var bmp = sourceBmp.CloneFromBitmap();
        bmp.ApplyFilter((ImageFilter_ColorChange.Instance, (toReplace, replacement)));
        return bmp;
    }

    public static Bitmap Resize(this Bitmap sourceBmp, int maxwidth, int maxheight, SizeModes sizeMode, InterpolationMode interpolationMode, bool collectGarbage) {
        if (collectGarbage) { Generic.CollectGarbage(); }
        if (maxwidth < 1) { maxwidth = 1; }
        if (maxheight < 1) { maxheight = 1; }

        int calcwidth;
        int calcheight;

        switch (sizeMode) {
            case SizeModes.EmptySpace:
            case SizeModes.BildAbschneiden:
                var scale3 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                calcwidth = (int)(scale3 * sourceBmp.Width);
                calcheight = (int)(scale3 * sourceBmp.Height);
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                var scale1 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                maxwidth = (int)(scale1 * sourceBmp.Width);
                maxheight = (int)(scale1 * sourceBmp.Height);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                var scale2 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                if (scale2 >= 1) { scale2 = 1; }
                maxwidth = (int)(scale2 * sourceBmp.Width);
                maxheight = (int)(scale2 * sourceBmp.Height);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            case SizeModes.Verzerren:
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            default:
                Develop.DebugPrint(sizeMode);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;
        }

        Bitmap tmp;

        switch (sourceBmp.Width) {
            case > 20000 when calcwidth < 4000:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 4.0), (int)(sourceBmp.Height / 4.0), null, IntPtr.Zero);
                break;

            case > 15000 when calcwidth < 4000:
            case > 10000 when calcwidth < 2500:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 3.0), (int)(sourceBmp.Height / 3.0), null, IntPtr.Zero);
                break;

            case > 8000 when calcwidth < 2000:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 2.5), (int)(sourceBmp.Height / 2.5), null, IntPtr.Zero);
                break;

            default:
                tmp = sourceBmp;
                break;
        }

        try {
            var bmp = new Bitmap(maxwidth, maxheight);
            using var gr = Graphics.FromImage(bmp);
            gr.InterpolationMode = interpolationMode;
            gr.PixelOffsetMode = PixelOffsetMode.Half;

            gr.DrawImage(tmp, (int)((maxwidth - calcwidth) / 2.0), (int)((maxheight - calcheight) / 2.0), calcwidth, calcheight);
            return bmp;
        } catch { /* Bitmap-Resize fehlgeschlagen, Fallback wird versucht */ }

        try {
            if (!collectGarbage) { Generic.CollectGarbage(); }

            if (sizeMode == SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern) {
                return (Bitmap)sourceBmp.GetThumbnailImage(calcwidth, calcheight, null, IntPtr.Zero);
            }
        } catch { /* GetThumbnailImage fehlgeschlagen */ }

        Develop.AbortAppIfStackOverflow();
        return new Bitmap(1, 1);
    }

    public static void SetArgb(this byte[] bits, int index, byte a, byte r, byte g, byte b) {
        bits[index] = b;
        bits[index + 1] = g;
        bits[index + 2] = r;
        bits[index + 3] = a;
    }

    public static void SetColor(this byte[] bits, int index, Color color) {
        bits[index] = color.B;
        bits[index + 1] = color.G;
        bits[index + 2] = color.R;
        bits[index + 3] = color.A;
    }

    public static void SetPixel(this BitmapData data, byte[] bits, int x, int y, Color color) {
        var index = data.GetPixelIndex(x, y);
        bits.SetColor(index, color);
    }

    public static void SetPixelArgb(this BitmapData data, byte[] bits, int x, int y, byte a, byte r, byte g, byte b) {
        var index = data.GetPixelIndex(x, y);
        bits.SetArgb(index, a, r, g, b);
    }

    public static List<Bitmap> SplitTiff(string fileName, int maxSize) {
        var imageStreamSource = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        List<Bitmap> l = [];
        var frames = 1;
        try {
            using var tiffImage = Image.FromStream(imageStreamSource);
            frames = tiffImage.FrameDimensionsList.Length > 0
                ? tiffImage.GetFrameCount(FrameDimension.Page)
                : 1;

            for (var i = 0; i < frames; i++) {
                tiffImage.SelectActiveFrame(FrameDimension.Page, i);

                var frameBitmap = new Bitmap(tiffImage);

                if (maxSize > 0) {
                    frameBitmap = frameBitmap.Resize(maxSize, maxSize,
                        SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern,
                        InterpolationMode.HighQualityBicubic, true);
                }

                l.Add(frameBitmap);
            }
        } catch {
            try {
                l.Clear();
                Generic.CollectGarbage();
                var x = (Bitmap?)Image_FromFile(fileName);
                if (x != null) {
                    l.Add(x.Resize(maxSize, maxSize, SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));
                }

                if (frames > 1) {
                    var bmp = new Bitmap(200, 200);
                    var gr = Graphics.FromImage(bmp);
                    gr.Clear(Color.White);
                    gr.DrawString("Weitere Blätter vorhanden!", new Font("Arial", 9), Brushes.Red, new Point(0, 0), DefaultWithTrailingSpaces);
                    l.Add(bmp);
                }
            } catch (Exception ex) {
                l.Clear();
                Generic.CollectGarbage();
                var bmp = new Bitmap(200, 200);
                var gr = Graphics.FromImage(bmp);
                gr.Clear(Color.White);
                gr.DrawString("Vorschaubild fehlgeschlagen!", new Font("Arial", 9), Brushes.Red, new Point(0, 0), DefaultWithTrailingSpaces);
                l.Add(bmp);
                Develop.DebugPrint("Vorschaubild fehlgeschlagen", ex);
            }
        }
        imageStreamSource.Close();
        imageStreamSource.Dispose();
        return l;
    }

    #endregion
}