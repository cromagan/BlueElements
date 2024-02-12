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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
    private static List<ImageFilter>? _imageFilter;

    private Bitmap? _bitmap;

    private int[]? _bits;

    #endregion

    #region Constructors

    public BitmapExt(string filename) {
        var p = Image_FromFile(filename);
        if (p is Bitmap bmp) {
            CloneFromBitmap(bmp);
        } else {
            EmptyBitmap(1, 1);
        }
    }

    /// <summary>
    /// Erstellt das BitmapExt Element mit einem Clone der angegebenen Bitmaap
    /// </summary>
    /// <param name="bmp"></param>
    public BitmapExt(Bitmap bmp) => CloneFromBitmap(bmp);

    public BitmapExt(int width, int height) => EmptyBitmap(width, height);

    protected BitmapExt() => EmptyBitmap(1, 1);

    #endregion

    #region Destructors

    ~BitmapExt() { Dispose(disposing: false); }

    #endregion

    #region Properties

    public static List<ImageFilter> ImageFilters {
        get {
            _imageFilter ??= BlueBasics.Generic.GetInstaceOfType<ImageFilter>();
            return _imageFilter;
        }
    }

    public int Height { get; private set; }

    public bool IsDisposed { get; private set; }

    public int Width { get; private set; }
    private GCHandle BitsHandle { get; set; }

    #endregion

    #region Methods

    public static Bitmap? Area(Bitmap sourceBitmap, Rectangle r) {
        if (r.Width < 2 || r.Height < 2) { return null; }
        Bitmap clipedArea = new(r.Width, r.Height);
        using var gr = Graphics.FromImage(clipedArea);
        gr.Clear(Color.Black);
        gr.DrawImage(sourceBitmap, 0, 0, r, GraphicsUnit.Pixel);
        return clipedArea;
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

    public static Bitmap? Image_Clone(Bitmap? sourceBmp) {
        if (sourceBmp == null) { return null; }
        Bitmap bmp = new(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.DrawImage(sourceBmp, 0, 0, sourceBmp.Width, sourceBmp.Height); // Unerklärlich, orgiImage.Width, orgiImage.Height muss stehen bleiben!
        return bmp;
    }

    public static implicit operator Bitmap?(BitmapExt? p) => p?._bitmap;

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

    public bool ApplyFilter(string name, float factor) {
        if (IsDisposed || _bits == null) { return false; }
        var f = ImageFilters.Get(name);
        if (f == null) { return false; }

        f.ProcessFilter(Width, Height, ref _bits, factor, 0);

        return true;
    }

    public void ApplyFilter(string name) => ApplyFilter(name, 0);

    public void CloneFromBitmap(Bitmap? bmp) {
        // Überprüfen, ob das übergebene Bitmap null ist
        if (bmp == null) {
            Width = -1;
            Height = -1;
            return;
        }

        // Initialisieren einer Stopwatch für die Zeitüberwachung
        var stopwatch = Stopwatch.StartNew();

        do {
            try {
                // Setzen der Bildabmessungen
                Width = bmp.Width;
                Height = bmp.Height;
                _bits = new int[Width * Height];

                // Fixieren des Bits-Arrays im Speicher, damit es nicht von der Garbage Collection verschoben wird.
                BitsHandle = GCHandle.Alloc(_bits, GCHandleType.Pinned);

                // Erstellen eines neuen Bitmaps mit dem fixierten Bits-Array als Puffer
                _bitmap = new Bitmap(Width, Height, Width * 4, Pixelformat, BitsHandle.AddrOfPinnedObject());

                // Zeichnen des übergebenen Bitmaps auf das neue Bitmap.
                // Dadurch werden die Pixel-Daten des Originalbildes in das Bits-Array kopiert.
                using var gr = Graphics.FromImage(_bitmap);
                gr.DrawImage(bmp, new Rectangle(0, 0, Width, Height));
            } catch (Exception ex) {
                // Freigeben des GCHandle, wenn eine Ausnahme auftritt
                if (BitsHandle.IsAllocated) { BitsHandle.Free(); }

                // Überprüfen, ob die Zeitüberschreitung erreicht ist
                if (stopwatch.Elapsed.TotalSeconds > 5) {
                    // Protokollieren des Fehlers
                    stopwatch.Stop();
                    Develop.DebugPrint("Bild konnte nicht geklont werden", ex);
                    Width = -1;
                    Height = -1;
                    _bitmap = null;
                    return;
                }

                // Kurze Pause vor dem nächsten Versuch
                Thread.Sleep(100);
            }
        } while (_bitmap == null);

        // Stopp der Stopwatch
        stopwatch.Stop();
    }

    public Bitmap? CloneOfBitmap() {
        //public static explicit operator Bitmap?(BitmapExt? p) {
        if (_bitmap == null) { return null; }
        var bmp = _bitmap.Clone();
        if (bmp is Bitmap bitmap) { return bitmap; }
        return null;
    }

    public BitmapExt Crop(Rectangle re) {
        BitmapExt newBmp = new(re.Width, re.Height);
        if (newBmp._bitmap != null) {
            using var gr = Graphics.FromImage(newBmp._bitmap);
            gr.Clear(Color.Transparent);
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            if (_bitmap != null) { gr.DrawImage(_bitmap, re with { X = 0, Y = 0 }, re.Left, re.Top, re.Width, re.Height, GraphicsUnit.Pixel); }
        }

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
        if (_bits != null && x >= 0 && y >= 0 && x < Width && y < Height) { return Color.FromArgb(_bits[x + (y * Width)]); }
        return Color.Transparent;
    }

    public void MakeTransparent(Color color) => _bitmap?.MakeTransparent(color);

    public void Save(string name, ImageFormat imageFormat) => _bitmap?.Save(name, imageFormat);

    public void SetPixel(int x, int y, Color color) {
        if (_bits != null && x >= 0 && y >= 0 && x < Width && y < Height) {
            _bits[x + (y * Width)] = color.ToArgb();
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
            }
            // Nicht verwaltete Ressourcen (Bitmap, Datenbankverbindungen, ...)
            _bitmap?.Dispose();
            BitsHandle.Free();
            IsDisposed = true;
        }
    }

    protected void EmptyBitmap(int width, int height) {
        Width = width;
        Height = height;
        _bits = new int[Width * Height];
        BitsHandle = GCHandle.Alloc(_bits, GCHandleType.Pinned);

        if (Width < 1 || height < 1) {
            _bitmap = null;
            return;
        }

        _bitmap = new Bitmap(Width, Height, Width * 4, Pixelformat, BitsHandle.AddrOfPinnedObject());
    }

    #endregion
}