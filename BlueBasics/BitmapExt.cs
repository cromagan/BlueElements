// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Extensions;

namespace BlueBasics;

// https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
// Todo: Obselete Routinen:
// Image_FromFile
// Resize
public class BitmapExt : IDisposable, IDisposableExtended {

    #region Fields

    private const PixelFormat Pixelformat = PixelFormat.Format32bppArgb;

    private Bitmap? _bitmap;

    private BitmapData bitmapData;
    private bool isLocked;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt das BitmapExt Element mit einem Clone der angegebenen Bitmaap
    /// </summary>
    /// <param name="bmp"></param>
    public BitmapExt(Bitmap bmp) => CloneFromBitmap(bmp);

    public BitmapExt(string filename) {
        var p = Image_FromFile(filename);
        if (p is Bitmap bmp) {
            CloneFromBitmap(bmp);
        } else {
            EmptyBitmap(1, 1);
        }
    }

    public BitmapExt(int width, int height) => EmptyBitmap(width, height);

    protected BitmapExt() => EmptyBitmap(1, 1);

    #endregion

    #region Destructors

    ~BitmapExt() { Dispose(disposing: false); }

    #endregion

    #region Properties

    public int Height { get; private set; }
    public bool IsDisposed { get; private set; }
    public int Width { get; private set; }
    private byte[]? Bits { get; set; }

    #endregion

    #region Methods

    public static void AllePixelZuSchwarz(Bitmap pic, double nearWhiteSchwelle) {
        for (var x = 0; x < pic.Width; x++) {
            for (var y = 0; y < pic.Height; y++) {
                var ca = pic.GetPixel(x, y);
                if (!ca.IsNearWhite(nearWhiteSchwelle)) {
                    pic.SetPixel(x, y, Color.FromArgb(ca.A, 0, 0, 0));
                }
            }
        }
    }

    public static Bitmap? Area(Bitmap sourceBitmap, Rectangle r) {
        if (r.Width < 2 || r.Height < 2) { return null; }
        Bitmap clipedArea = new(r.Width, r.Height);
        using var gr = Graphics.FromImage(clipedArea);
        gr.Clear(Color.Black);
        gr.DrawImage(sourceBitmap, 0, 0, r, GraphicsUnit.Pixel);
        return clipedArea;
    }

    public static void Ausdünnen(Bitmap? pic, int staerke) {
        if (pic == null) { return; }
        for (var x = 0; x < pic.Width - 1; x++) {
            for (var y = 0; y < pic.Height - 1; y++) {
                if (!IsWhite(x, y)) {
                    for (var wi = staerke; wi > 0; wi--) {
                        var ma1 = (int)Math.Floor((float)wi / 2);
                        var ma2 = wi - ma1;
                        // X
                        if (IsWhite(x - ma1 - 1, y) && IsWhite(x + ma2 + 1, y)) {
                            var allblack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (IsWhite(x + ch, y)) { allblack = false; break; }
                            }
                            if (allblack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) { pic.SetPixel(x + ch, y, Color.White); }
                                }
                            }
                        }
                        // Y
                        if (IsWhite(x, y - ma1 - 1) && IsWhite(x, y + ma2 + 1)) {
                            var allblack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (IsWhite(x, y + ch)) { allblack = false; break; }
                            }
                            if (allblack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) { pic.SetPixel(x, y + ch, Color.White); }
                                }
                            }
                        }
                    }
                }
            }
        }
        bool IsWhite(int x, int y) => x < 0 || y < 0 || x >= pic.Width || y >= pic.Height || pic.GetPixel(x, y).IsNearWhite(0.9);
    }

    public static Bitmap? AutoCrop(Bitmap? pic, double minBrightness) {
        var pa = GetAutoValuesForCrop(pic, minBrightness);
        return Crop(pic, pa.Left, pa.Right, pa.Top, pa.Bottom);
    }

    public static Bitmap? Crop(Bitmap? bmp, Rectangle r) {
        if (bmp == null) { return null; }
        return Crop(bmp, r.Left, -(bmp.Width - r.Right), r.Top, -(bmp.Height - r.Bottom));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="left">Positiver Wert schneidet diese Anzahl von Pixel vom linken Rand weg.</param>
    /// <param name="right">Negativer Wert schneidet diese Anzahl von Pixel vom rechten Rand weg.</param>
    /// <param name="top">Positiver Wert schneidet diese Anzahl von Pixel vom oberen Rand weg.</param>
    /// <param name="bottom">Negativer Wert schneidet diese Anzahl von Pixel vom unteren Rand weg.</param>
    /// <returns></returns>
    public static Bitmap? Crop(Bitmap? bmp, int left, int right, int top, int bottom) {
        if (bmp == null || (left == 0 && right == 0 && top == 0 && bottom == 0)) { return bmp; }

        Generic.CollectGarbage();
        var w = Math.Max(bmp.Width - left + right, 1);
        var h = Math.Max(bmp.Height - top + bottom, 1);
        Bitmap bmp2 = new(w, h);
        using (var gr = Graphics.FromImage(bmp2)) {
            gr.DrawImage(bmp, -left, -top, bmp.Width, bmp.Height); // Width und Height MUSS angegeben werden. Manche Bilder (Falsches Format?) schlagen fehl, wenn es fehlt.
        }
        Generic.CollectGarbage();
        return bmp2;
    }

    public static void FillCircle(Bitmap? bmp, Color c, int x, int y, int r) {
        if (bmp == null) { return; }

        for (var adx = -r; adx <= r; adx++) {
            for (var ady = -r; ady <= r; ady++) {
                var d = Math.Sqrt(Convert.ToDouble((adx * adx) + (ady * ady))) - 0.5;
                var px = x + adx;
                var py = y + ady;
                if (px >= 0 && py >= 0 && px < bmp.Width && py < bmp.Height && d <= r) {
                    bmp.SetPixel(px, py, c);
                }
            }
        }
    }

    public static Padding GetAutoValuesForCrop(Bitmap? pic, double minBrightness) {
        Padding pa = new(0, 0, 0, 0);
        if (pic == null) { return pa; }
        var x = 0;
        var exitNow = false;
        int y;
        while (true) {
            for (y = 0; y < pic.Height; y++) {
                if (!pic.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            x++;
            if (x > pic.Width * 0.9) { break; }
        }
        pa.Left = x;
        // -------------
        x = pic.Width - 1;
        exitNow = false;
        while (true) {
            for (y = 0; y < pic.Height; y++) {
                if (!pic.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            x--;
            if (x < pic.Width * 0.1) { break; }
        }
        pa.Right = x - pic.Width + 1;
        // -------------
        y = 0;
        exitNow = false;
        while (true) {
            for (x = 0; x < pic.Width; x++) {
                if (!pic.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            y++;
            if (y > pic.Height * 0.9) { break; }
        }
        pa.Top = y;
        // -------------
        y = pic.Height - 1;
        exitNow = false;
        while (true) {
            for (x = 0; x < pic.Width; x++) {
                if (!pic.GetPixel(x, y).IsNearWhite(minBrightness)) {
                    exitNow = true;
                    break;
                }
            }
            if (exitNow) { break; }
            y--;
            if (y < pic.Height * 0.1) { break; }
        }
        pa.Bottom = y - pic.Height + 1;
        return pa;
    }

    public static Bitmap GetBitmap(BitmapSource bitmapsource, int maxSize) {
        Generic.CollectGarbage();
        Generic.Pause(0.1, true);
        Bitmap? bitmap;
        using (MemoryStream outStream = new()) {
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapsource));
            enc.Save(outStream);
            bitmap = new Bitmap(outStream);
        }
        if (maxSize > 0) {
            bitmap = bitmap.Resize(maxSize, maxSize, SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true);
        }
        return bitmap;
    }

    public static Bitmap Grayscale(Bitmap original) {
        Bitmap newBitmap = new(original.Width, original.Height);
        var g = Graphics.FromImage(newBitmap);
        ColorMatrix colorMatrix = new(new[]
        {
            new[] {.3f, .3f, .3f, 0, 0}, new[] {.59f, .59f, .59f, 0, 0}, new[] {.11f, .11f, .11f, 0, 0},
            new float[] {0, 0, 0, 1, 0}, new float[] {0, 0, 0, 0, 1}
        });
        ImageAttributes attributes = new();
        attributes.SetColorMatrix(colorMatrix);
        g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        g.Dispose();
        return newBitmap;
    }

    public static Bitmap? Image_Clone(Bitmap? sourceBmp) {
        if (sourceBmp == null) { return null; }
        Bitmap bmp = new(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.DrawImage(sourceBmp, 0, 0, sourceBmp.Width, sourceBmp.Height); // Unerklärlich, orgiImage.Width, orgiImage.Height muss stehen bleiben!
        return bmp;
    }

    public static void IntensifyBitmap(ref Bitmap bMp) {
        for (var x = 0; x < bMp.Width; x++) {
            for (var y = 0; y < bMp.Height; y++) {
                var c = bMp.GetPixel(x, y);
                if (c.A > 0.5 && bMp.GetPixel(x, y).GetBrightness() < 0.9) { bMp.SetPixel(x, y, Color.Black); }
            }
        }
    }

    /// <summary>
    /// Pixelgenaue Collisionsanalyse zweier Bitmaps
    /// </summary>
    /// <param name="image1"></param>
    /// <param name="pos1">Linke obere Eckte des 1. Bildes</param>
    /// <param name="image2"></param>
    /// <param name="pos2">Linke obere Eckte des 2. Bildes</param>
    /// <param name="accuracy">Genauigkeit der Prüfung. Bei 1 wird jeder Pixel geprüft. Bei z.B. 3 wird nur jeder dritte Pixel geprüft.</param>
    /// <returns></returns>
    public static bool IntersectsWith(Bitmap? image1, Point pos1, Bitmap? image2, Point pos2, int accuracy) {
        if (image1 == null || image2 == null) { return false; }
        Rectangle koord1 = new(pos1, image1.Size);
        Rectangle koord2 = new(pos2, image2.Size);
        if (!koord1.IntersectsWith(koord2)) { return false; }
        Rectangle schnitt = new(koord1.Location, koord1.Size);
        schnitt.Intersect(koord2);
        for (var x = schnitt.Left; x < schnitt.Right; x += accuracy) {
            for (var y = schnitt.Top; y < schnitt.Bottom; y += accuracy) {
                if (!image1.GetPixel(x - koord1.X, y - koord1.Y).IsNearWhite(0.9) && !image2.GetPixel(x - koord2.X, y - koord2.Y).IsNearWhite(0.9)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static Bitmap Invert(Bitmap source) {
        // create a blank bitmap the same size as original
        Bitmap newBitmap = new(source.Width, source.Height);
        // get a graphics object from the new image
        var g = Graphics.FromImage(newBitmap);
        // create the negative color matrix
        ColorMatrix colorMatrix = new(new[]
        {
            new float[] {-1, 0, 0, 0, 0}, new float[] {0, -1, 0, 0, 0}, new float[] {0, 0, -1, 0, 0},
            new float[] {0, 0, 0, 1, 0}, new float[] {1, 1, 1, 0, 1}
        });
        // create some image attributes
        ImageAttributes attributes = new();
        attributes.SetColorMatrix(colorMatrix);
        g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
            0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        // dispose the Graphics object
        g.Dispose();
        return newBitmap;
    }

    public static void Magnify(Bitmap screenshot, Point point, Graphics gR, bool swapX) {
        const int w1 = 200; // Größe des Rechteckes
        const int w5 = 10; // Pixel zum vergrößerm
        int x;
        if (!swapX) {
            x = 150 - (int)(w1 / 2.0);
            if (point.X < screenshot.Width / 2.0) { x = screenshot.Width - 150 - (int)(w1 / 2.0); }
        } else {
            x = screenshot.Width - 150 - (int)(w1 / 2.0);
            if (point.X < screenshot.Width / 2.0) { x = 150 - (int)(w1 / 2.0); }
        }
        var y = 150 - (int)(w1 / 2.0);
        if (point.Y < screenshot.Height / 2.0) { y = screenshot.Height - 150 - (int)(w1 / 2.0); }
        Rectangle r = new(x, y, w1, w1);
        for (var z = 5; z >= 0; z--) {
            r.Inflate(1, 1);
            // r.Expand(0, 0, 1, 1)
            var w = Convert.ToByte(255 / (double)10 * z);
            gR.DrawRectangle(new Pen(Color.FromArgb(w, 0, 0, 0)), r);
        }
        r.Inflate(-5, -5);
        gR.InterpolationMode = InterpolationMode.NearestNeighbor;
        gR.PixelOffsetMode = PixelOffsetMode.Half;
        gR.DrawImage(screenshot, r, new Rectangle(point.X - w5, point.Y - w5, (w5 * 2) + 1, (w5 * 2) + 1), GraphicsUnit.Pixel);
        gR.DrawRectangle(Pens.Black, r);
        var mitte = r.PointOf(Alignment.Horizontal_Vertical_Center);
        gR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X, mitte.Y - 7, mitte.X, mitte.Y + 6);
        gR.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X - 7, mitte.Y, mitte.X + 6, mitte.Y);
        gR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), mitte.X, r.Top, mitte.X, r.Bottom);
        gR.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, mitte.Y, r.Right, mitte.Y);
        gR.DrawLine(Pens.Red, mitte.X, mitte.Y - 6, mitte.X, mitte.Y + 5);
        gR.DrawLine(Pens.Red, mitte.X - 6, mitte.Y, mitte.X + 5, mitte.Y);
    }

    public static unsafe Bitmap? ReplaceColor(Bitmap? source, Color toReplace, Color replacement) {
        if (source == null) { return null; }

        // https://stackoverflow.com/questions/17208254/how-to-change-pixel-color-of-an-image-in-c-net
        const int pixelSize = 4; // 32 bits per pixel
        Bitmap target = new(source.Width, source.Height, PixelFormat.Format32bppArgb);
        BitmapData? sourceData = null;
        BitmapData? targetData = null;
        try {
            sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            targetData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (var y = 0; y < source.Height; ++y) {
                var sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                var targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);
                for (var x = 0; x < source.Width; ++x) {
                    var b = sourceRow[(x * pixelSize) + 0];
                    var g = sourceRow[(x * pixelSize) + 1];
                    var r = sourceRow[(x * pixelSize) + 2];
                    var a = sourceRow[(x * pixelSize) + 3];
                    if (toReplace.R == r && toReplace.G == g && toReplace.B == b && toReplace.A == a) {
                        r = replacement.R;
                        g = replacement.G;
                        b = replacement.B;
                        a = replacement.A;
                    }
                    targetRow[(x * pixelSize) + 0] = b;
                    targetRow[(x * pixelSize) + 1] = g;
                    targetRow[(x * pixelSize) + 2] = r;
                    targetRow[(x * pixelSize) + 3] = a;
                }
            }
        } finally {
            if (sourceData != null) { source.UnlockBits(sourceData); }
            if (targetData != null) { target.UnlockBits(targetData); }
        }
        return target;
    }

    public static List<Bitmap> SplitTiff(string fileName, int maxSize) {
        // Open a Stream and decode a TIFF image
        FileStream imageStreamSource = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        List<Bitmap> l = [];
        var frames = 1;
        try {
            TiffBitmapDecoder decoder = new(imageStreamSource,
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            frames = decoder.Frames.Count;
            l.AddRange(decoder.Frames.Select(frame => GetBitmap(frame, maxSize)));
        } catch {
            try {
                l.Clear();
                Generic.CollectGarbage();
                var x = (Bitmap?)Image_FromFile(fileName);
                if (x != null)
                    l.Add(x.Resize(maxSize, maxSize, SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));
                if (frames > 1) {
                    Bitmap x2 = new(200, 200);
                    var gr = Graphics.FromImage(x2);
                    gr.Clear(Color.White);
                    gr.DrawString("Weitere Blätter vorhanden!", new Font("Arial", 9), Brushes.Red, new Point(0, 0));
                    l.Add(x2);
                }
            } catch (Exception ex) {
                l.Clear();
                Generic.CollectGarbage();
                Bitmap x2 = new(200, 200);
                var gr = Graphics.FromImage(x2);
                gr.Clear(Color.White);
                gr.DrawString("Vorschaubild fehlgeschlagen!", new Font("Arial", 9), Brushes.Red, new Point(0, 0));
                l.Add(x2);
                Develop.DebugPrint("Vorschaubild fehlgeschlagen", ex);
            }
        }
        imageStreamSource.Close();
        imageStreamSource.Dispose();
        return l;
    }

    public void AdjustBrightness(float value) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        int bytes = bitmapData.Stride * bitmapData.Height;

        // Apply brightness adjustment
        for (int i = 0; i < bytes; i += 4) {
            Bits[i] = (byte)Math.Max(0, Math.Min(255, Bits[i] + value));
            Bits[i + 1] = (byte)Math.Max(0, Math.Min(255, Bits[i + 1] + value));
            Bits[i + 2] = (byte)Math.Max(0, Math.Min(255, Bits[i + 2] + value));
            // Alpha channel remains unchanged (Bits[i + 3])
        }
    }

    public void AdjustContrast(float value) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        value = (100.0f + value) / 100.0f;
        value *= value;

        int bytes = bitmapData.Stride * bitmapData.Height;

        // Apply contrast adjustment
        for (int i = 0; i < bytes; i += 4) {
            float b = Bits[i] / 255.0f;
            float g = Bits[i + 1] / 255.0f;
            float r = Bits[i + 2] / 255.0f;

            r = (((r - 0.5f) * value) + 0.5f) * 255.0f;
            g = (((g - 0.5f) * value) + 0.5f) * 255.0f;
            b = (((b - 0.5f) * value) + 0.5f) * 255.0f;

            Bits[i] = (byte)Math.Max(0, Math.Min(255, b));       // Blue
            Bits[i + 1] = (byte)Math.Max(0, Math.Min(255, g));   // Green
            Bits[i + 2] = (byte)Math.Max(0, Math.Min(255, r));     // Red
                                                                   // Bits[i + 3] is Alpha and remains unchanged
        }
    }

    public void AdjustGamma(float gamma) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        // Create gamma correction table
        byte[] gammaTable = new byte[256];
        for (int i = 0; i < 256; ++i) {
            gammaTable[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
        }

        int bytes = bitmapData.Stride * bitmapData.Height;

        // Apply gamma correction
        for (int i = 0; i < bytes; i += 4) {
            Bits[i] = gammaTable[Bits[i]];         // Blue
            Bits[i + 1] = gammaTable[Bits[i + 1]]; // Green
            Bits[i + 2] = gammaTable[Bits[i + 2]]; // Red
            Bits[i + 3] = Bits[i + 3];             // Alpha
        }
    }

    public void CloneFromBitmap(Bitmap? sourcebmp) {
        UnlockBits(false);
        _bitmap?.Dispose(); // Dispose of any existing bitmap
        _bitmap = null;

        if (sourcebmp == null) {
            Width = -1;
            Height = -1;
            return;
        }

        var tim = DateTime.UtcNow;
        do {
            try {
                Width = sourcebmp.Width;
                Height = sourcebmp.Height;

                _bitmap = new Bitmap(Width, Height, Pixelformat);
                using var gr = Graphics.FromImage(_bitmap);
                gr.DrawImage(sourcebmp, new Rectangle(0, 0, Width, Height));

                LockBits();
            } catch (Exception ex) {
                Width = -1;
                Height = -1;
                _bitmap = null;

                if (DateTime.UtcNow.Subtract(tim).TotalSeconds > 5) {
                    Develop.DebugPrint("Bild konnte nicht geklont werden", ex);
                    return;
                }
            }
        } while (_bitmap == null);
    }

    public Bitmap? CloneOfBitmap() {
        //public static explicit operator Bitmap?(BitmapExt? p) {
        if (_bitmap == null) { return null; }
        UnlockBits(true);
        var bmp = _bitmap.Clone();
        LockBits();
        if (bmp is Bitmap bitmap) { return bitmap; }
        return null;
    }

    public Bitmap Crop(Rectangle re) {
        UnlockBits(true);
        Bitmap newBmp = new(re.Width, re.Height);

        using var gr = Graphics.FromImage(newBmp);
        gr.Clear(Color.Transparent);
        gr.PixelOffsetMode = PixelOffsetMode.Half;
        UnlockBits(true);
        if (_bitmap != null) { gr.DrawImage(_bitmap, re with { X = 0, Y = 0 }, re.Left, re.Top, re.Width, re.Height, GraphicsUnit.Pixel); }

        LockBits();
        return newBmp;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FromFile(string dateiName, bool setDummyPicIfFails) {
        var x = (Bitmap?)Image_FromFile(dateiName);
        if (x == null && setDummyPicIfFails) {
            x = QuickImage.Get(ImageCode.Warnung);
        }
        CloneFromBitmap(x);
    }

    public Color GetPixel(int x, int y) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return Color.Transparent;
        }

        int index = (y * bitmapData.Stride) + (x * 4); // 4 bytes per pixel in ARGB
        byte blue = Bits[index];
        byte green = Bits[index + 1];
        byte red = Bits[index + 2];
        byte alpha = Bits[index + 3];

        return Color.FromArgb(alpha, red, green, blue);
    }

    public void ImageBlurFilter(BlurType blurType) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        switch (blurType) {
            case BlurType.Mean3x3:
                ConvolutionFilter(ImageMatrix.Mean3X3, 1.0 / 9.0, 0);
                break;

            case BlurType.Mean5x5:
                ConvolutionFilter(ImageMatrix.Mean5X5, 1.0 / 25.0, 0);
                break;

            case BlurType.Mean7x7:
                ConvolutionFilter(ImageMatrix.Mean7X7, 1.0 / 49.0, 0);
                break;

            case BlurType.Mean9x9:
                ConvolutionFilter(ImageMatrix.Mean9X9, 1.0 / 81.0, 0);
                break;

            case BlurType.GaussianBlur3x3:
                ConvolutionFilter(ImageMatrix.GaussianBlur3X3, 1.0 / 16.0, 0);
                break;

            case BlurType.GaussianBlur5x5:
                ConvolutionFilter(ImageMatrix.GaussianBlur5X5, 1.0 / 159.0, 0);
                break;

            case BlurType.MotionBlur5x5:
                ConvolutionFilter(ImageMatrix.MotionBlur5X5, 1.0 / 10.0, 0);
                break;

            case BlurType.MotionBlur5x5At45Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur5X5At45Degrees, 1.0 / 5.0, 0);
                break;

            case BlurType.MotionBlur5x5At135Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur5X5At135Degrees, 1.0 / 5.0, 0);
                break;

            case BlurType.MotionBlur7x7:
                ConvolutionFilter(ImageMatrix.MotionBlur7X7, 1.0 / 14.0, 0);
                break;

            case BlurType.MotionBlur7x7At45Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur7X7At45Degrees, 1.0 / 7.0, 0);
                break;

            case BlurType.MotionBlur7x7At135Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur7X7At135Degrees, 1.0 / 7.0, 0);
                break;

            case BlurType.MotionBlur9x9:
                ConvolutionFilter(ImageMatrix.MotionBlur9X9, 1.0 / 18.0, 0);
                break;

            case BlurType.MotionBlur9x9At45Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur9X9At45Degrees, 1.0 / 9.0, 0);
                break;

            case BlurType.MotionBlur9x9At135Degrees:
                ConvolutionFilter(ImageMatrix.MotionBlur9X9At135Degrees, 1.0 / 9.0, 0);
                break;

            case BlurType.Median3x3:
                MedianFilter(3);
                break;

            case BlurType.Median5x5:
                MedianFilter(5);
                break;

            case BlurType.Median7x7:
                MedianFilter(7);
                break;

            case BlurType.Median9x9:
                MedianFilter(9);
                break;

            case BlurType.Median11x11:
                MedianFilter(11);
                break;
        }
    }

    public void LockBits() {
        if (isLocked) { return; }
        if (_bitmap == null) { return; }

        // Definieren Sie den Bereich des Bitmaps, der gesperrt werden soll.
        Rectangle lockArea = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

        // Sperren Sie den Bitmap-Bereich und erhalten Sie die BitmapData.
        bitmapData = _bitmap.LockBits(lockArea, ImageLockMode.ReadWrite, Pixelformat);

        Bits = new byte[bitmapData.Stride * bitmapData.Height];
        Marshal.Copy(bitmapData.Scan0, Bits, 0, Bits.Length);

        isLocked = true;
    }

    public void MakeTransparent(Color color) {
        UnlockBits(true);
        _bitmap?.MakeTransparent(color);
        LockBits();
    }

    public void MedianFilter(int matrixSize) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        var filterOffset = (matrixSize - 1) / 2;
        List<int> neighbourPixels = [];
        for (var offsetY = filterOffset; offsetY < Height - filterOffset; offsetY++) {
            for (var offsetX = filterOffset; offsetX < Width - filterOffset; offsetX++) {
                var byteOffset = (offsetY * bitmapData.Stride) + (offsetX * 4);
                neighbourPixels.Clear();
                for (var filterY = -filterOffset; filterY <= filterOffset; filterY++) {
                    for (var filter = -filterOffset; filter <= filterOffset; filter++) {
                        var calcOffset = byteOffset + (filter * 4) + (filterY * bitmapData.Stride);
                        neighbourPixels.Add(BitConverter.ToInt32(Bits, calcOffset));
                    }
                }
                neighbourPixels.Sort();
                var middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);
                Bits[byteOffset] = middlePixel[0];
                Bits[byteOffset + 1] = middlePixel[1];
                Bits[byteOffset + 2] = middlePixel[2];
                Bits[byteOffset + 3] = middlePixel[3];
            }
        }
    }

    public void Save(string name, ImageFormat imageFormat) {
        UnlockBits(true);
        _bitmap?.Save(name, imageFormat);
        LockBits();
    }

    public void SetPixel(int x, int y, Color color) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        int index = (y * bitmapData.Stride) + (x * 4); // 4 bytes per pixel in ARGB
        Bits[index] = color.B;
        Bits[index + 1] = color.G;
        Bits[index + 2] = color.R;
        Bits[index + 3] = color.A;
    }

    public void UnlockBits(bool copyback) {
        if (!isLocked) { return; }
        if (_bitmap == null) {
            isLocked = false;
            return;
        }

        if (copyback && Bits != null) {
            Marshal.Copy(Bits, 0, bitmapData.Scan0, Bits.Length);
        }

        try {
            // Entsperren Sie den Bitmap-Bereich.
            _bitmap.UnlockBits(bitmapData); // Finalizer kann Probleme machen
        } catch { }

        isLocked = false;
    }

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)

                UnlockBits(false); // Stellen Sie sicher, dass die Bitmap freigegeben wird
                _bitmap?.Dispose();
                _bitmap = null;
            }
            // Nicht verwaltete Ressourcen (Bitmap, Datenbankverbindungen, ...)
            //UnlockBits(false);
            //_bitmap?.Dispose();
            IsDisposed = true;
        }
    }

    protected void EmptyBitmap(int width, int height) {
        // Entsperren Sie das vorhandene Bitmap, falls es gesperrt ist.
        UnlockBits(false);

        // Entsorgen Sie das vorhandene Bitmap, bevor Sie ein neues erstellen.
        _bitmap?.Dispose();
        _bitmap = null;

        // Überprüfen Sie zuerst die Gültigkeit der Dimensionen.
        if (width < 1 || height < 1) {
            Width = 0;
            Height = 0;
            return;
        }

        // Setzen Sie die neuen Dimensionen.
        Width = width;
        Height = height;

        // Erstellen Sie ein neues Bitmap mit den angegebenen Dimensionen und dem Pixelformat.
        _bitmap = new Bitmap(Width, Height, Pixelformat);

        // Sperren Sie das neue Bitmap.
        LockBits();
    }

    private void ConvolutionFilter(double[,] filterMatrix, double factor, int bias) {
        if (!isLocked || Bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }
        var filterWidth = filterMatrix.GetLength(1);
        var filterOffset = (filterWidth - 1) / 2;

        for (var offsetY = filterOffset; offsetY < Height - filterOffset; offsetY++) {
            for (var offsetX = filterOffset; offsetX < Width - filterOffset; offsetX++) {
                double b = 0.0;
                double g = 0.0;
                double r = 0.0;

                var byteOffset = (offsetY * bitmapData.Stride) + (offsetX * 4);

                for (var filterY = -filterOffset; filterY <= filterOffset; filterY++) {
                    for (var filterX = -filterOffset; filterX <= filterOffset; filterX++) {
                        var calcOffset = byteOffset + (filterX * 4) + (filterY * bitmapData.Stride);

                        b += Bits[calcOffset] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        g += Bits[calcOffset + 1] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        r += Bits[calcOffset + 2] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                    }
                }
                b = (factor * b) + bias;
                g = (factor * g) + bias;
                r = (factor * r) + bias;

                b = Math.Max(0, Math.Min(255, b));
                g = Math.Max(0, Math.Min(255, g));
                r = Math.Max(0, Math.Min(255, r));

                Bits[byteOffset] = (byte)b;
                Bits[byteOffset + 1] = (byte)g;
                Bits[byteOffset + 2] = (byte)r;
                Bits[byteOffset + 3] = 255; // Set alpha to 255 (fully opaque)
            }
        }
    }

    #endregion
}