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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static BlueBasics.Extensions;

namespace BlueBasics;

// https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
// Todo: Obselete Routinen:
// Image_FromFile
// Resize
public class BitmapExt : IDisposableExtended {

    #region Fields

    private const PixelFormat Pixelformat = PixelFormat.Format32bppArgb;
    private Bitmap? _bitmap;

    private BitmapData? _bitmapData;
    private byte[]? _bits;
    private bool _isLocked;

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

    ~BitmapExt() {
        Dispose(disposing: false);
    }

    #endregion

    #region Properties

    public static List<ImageFilter> ImageFilters {
        get {
            field ??= Generic.GetInstaceOfType<ImageFilter>();
            return field;
        }
    }

    public int Height { get; private set; }

    public bool IsDisposed { get; private set; }

    public int Width { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="sourceBmp"></param>
    /// <param name="left">Positiver Wert schneidet diese Anzahl von Pixel vom linken Rand weg.</param>
    /// <param name="right">Negativer Wert schneidet diese Anzahl von Pixel vom rechten Rand weg.</param>
    /// <param name="top">Positiver Wert schneidet diese Anzahl von Pixel vom oberen Rand weg.</param>
    /// <param name="bottom">Negativer Wert schneidet diese Anzahl von Pixel vom unteren Rand weg.</param>
    /// <returns></returns>
    public static Bitmap? Crop(Bitmap? sourceBmp, int left, int right, int top, int bottom) {
        if (sourceBmp == null || (left == 0 && right == 0 && top == 0 && bottom == 0)) { return sourceBmp; }

        Generic.CollectGarbage();
        var w = Math.Max(sourceBmp.Width - left + right, 1);
        var h = Math.Max(sourceBmp.Height - top + bottom, 1);
        var bmp = new Bitmap(w, h);
        using (var gr = Graphics.FromImage(bmp)) {
            gr.DrawImage(sourceBmp, -left, -top, sourceBmp.Width, sourceBmp.Height); // Width und Height MUSS angegeben werden. Manche Bilder (Falsches Format?) schlagen fehl, wenn es fehlt.
        }
        Generic.CollectGarbage();
        return bmp;
    }

    public static void FillCircle(Bitmap? sourceBmp, Color c, int x, int y, int r) {
        if (sourceBmp == null) { return; }

        for (var adx = -r; adx <= r; adx++) {
            for (var ady = -r; ady <= r; ady++) {
                var d = Math.Sqrt(Convert.ToDouble((adx * adx) + (ady * ady))) - 0.5;
                var px = x + adx;
                var py = y + ady;
                if (px >= 0 && py >= 0 && px < sourceBmp.Width && py < sourceBmp.Height && d <= r) {
                    sourceBmp.SetPixel(px, py, c);
                }
            }
        }
    }

    public static Padding GetAutoValuesForCrop(Bitmap? sourceBmp, double minBrightness) {
        var pa = new Padding(0, 0, 0, 0);
        if (sourceBmp == null) { return pa; }
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

    public static Bitmap GetBitmap(Bitmap sourceBmp, int maxSize) {
        if (maxSize > 0) {
            return sourceBmp.Resize(maxSize, maxSize,
                SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern,
                InterpolationMode.HighQualityBicubic, true);
        }
        return sourceBmp;
    }

    public static Color GetPixel(BitmapData sourceBmpData, byte[] bits, int x, int y) {
        var index = (y * sourceBmpData.Stride) + (x * 4); // 4 bytes per pixel for BGRA format
        var b = bits[index];
        var g = bits[index + 1];
        var r = bits[index + 2];
        var a = bits[index + 3];
        return Color.FromArgb(a, r, g, b);
    }

    public static Bitmap? Image_Clone(Bitmap? sourceBmp) {
        //TODO: Unused
        try {
            if (sourceBmp?.IsValid() != true) { return null; }

            var bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.DrawImage(sourceBmp, 0, 0, sourceBmp.Width, sourceBmp.Height); // Unerklärlich, orgiImage.Width, orgiImage.Height muss stehen bleiben!
            return bmp;
        } catch {
        }

        return null;
    }

    /// <summary>
    /// Pixelgenaue Collisionsanalyse zweier Bitmaps
    /// </summary>
    /// <param name="bmp1"></param>
    /// <param name="pos1">Linke obere Eckte des 1. Bildes</param>
    /// <param name="bmp2"></param>
    /// <param name="pos2">Linke obere Eckte des 2. Bildes</param>
    /// <param name="accuracy">Genauigkeit der Prüfung. Bei 1 wird jeder Pixel geprüft. Bei z.B. 3 wird nur jeder dritte Pixel geprüft.</param>
    /// <returns></returns>
    public static bool IntersectsWith(Bitmap? bmp1, Point pos1, Bitmap? bmp2, Point pos2, int accuracy) {
        //Used: Only BZL
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

    public static void Magnify(Bitmap sourceBmp, PointF point, Graphics gr, bool swapX) {
        const int w1 = 200; // Größe des Rechteckes
        const int w5 = 10; // Pixel zum vergrößerm
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
            // r.Expand(0, 0, 1, 1)
            var w = Convert.ToByte(255 / (double)10 * z);
            gr.DrawRectangle(new Pen(Color.FromArgb(w, 0, 0, 0)), r);
        }
        r.Inflate(-5, -5);
        gr.InterpolationMode = InterpolationMode.NearestNeighbor;
        gr.PixelOffsetMode = PixelOffsetMode.Half;
        gr.DrawImage(sourceBmp, r, new Rectangle((int)point.X - w5, (int)point.Y - w5, (w5 * 2) + 1, (w5 * 2) + 1), GraphicsUnit.Pixel);
        gr.DrawRectangle(Pens.Black, r);
        var mitte = r.PointOf(Alignment.Horizontal_Vertical_Center);
        gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X, mitte.Y - 7, mitte.X, mitte.Y + 6);
        gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), mitte.X - 7, mitte.Y, mitte.X + 6, mitte.Y);
        gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), mitte.X, r.Top, mitte.X, r.Bottom);
        gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, mitte.Y, r.Right, mitte.Y);
        gr.DrawLine(Pens.Red, mitte.X, mitte.Y - 6, mitte.X, mitte.Y + 5);
        gr.DrawLine(Pens.Red, mitte.X - 6, mitte.Y, mitte.X + 5, mitte.Y);
    }

    public static Bitmap? ReplaceColor(Bitmap? sourceBmp, Color toReplace, Color replacement) {
        if (sourceBmp == null) { return null; }

        const int pixelSize = 4; // 32 bits per pixel
        var bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);
        BitmapData? sourceData = null;
        BitmapData? targetData = null;
        try {
            sourceData = sourceBmp.LockBits(new Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            targetData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var bytes = Math.Abs(sourceData.Stride) * sourceBmp.Height;
            var sourceBuffer = new byte[bytes];
            var targetBuffer = new byte[bytes];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, bytes);

            for (var y = 0; y < sourceBmp.Height; ++y) {
                var rowOffset = y * sourceData.Stride;
                for (var x = 0; x < sourceBmp.Width; ++x) {
                    var pixelOffset = rowOffset + (x * pixelSize);
                    var b = sourceBuffer[pixelOffset + 0];
                    var g = sourceBuffer[pixelOffset + 1];
                    var r = sourceBuffer[pixelOffset + 2];
                    var a = sourceBuffer[pixelOffset + 3];

                    if (toReplace.R == r && toReplace.G == g && toReplace.B == b && toReplace.A == a) {
                        r = replacement.R;
                        g = replacement.G;
                        b = replacement.B;
                        a = replacement.A;
                    }

                    targetBuffer[pixelOffset + 0] = b;
                    targetBuffer[pixelOffset + 1] = g;
                    targetBuffer[pixelOffset + 2] = r;
                    targetBuffer[pixelOffset + 3] = a;
                }
            }

            Marshal.Copy(targetBuffer, 0, targetData.Scan0, bytes);
        } finally {
            if (sourceData != null) { sourceBmp.UnlockBits(sourceData); }
            if (targetData != null) { bmp.UnlockBits(targetData); }
        }
        return bmp;
    }

    public static void SetPixel(BitmapData sourceBmpData, byte[] bits, int x, int y, Color color) {
        var index = (y * sourceBmpData.Stride) + (x * 4); // 4 bytes per pixel for BGRA format
        bits[index] = color.B;
        bits[index + 1] = color.G;
        bits[index + 2] = color.R;
        bits[index + 3] = color.A;
    }

    public static List<Bitmap> SplitTiff(string fileName, int maxSize) {
        //Used: Only BZL
        // Open a Stream and decode a TIFF image
        var imageStreamSource = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        List<Bitmap> l = [];
        var frames = 1;
        try {
            //TiffBitmapDecoder decoder = new(imageStreamSource,
            //    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            //frames = decoder.Frames.Count;
            //l.AddRange(decoder.Frames.Select(frame => GetBitmap(frame, maxSize)));

            using var tiffImage = Image.FromStream(imageStreamSource);
            // Anzahl der Frames ermitteln
            frames = tiffImage.FrameDimensionsList.Length > 0
                ? tiffImage.GetFrameCount(FrameDimension.Page)
                : 1;

            // Alle Frames durchgehen
            for (var i = 0; i < frames; i++) {
                tiffImage.SelectActiveFrame(FrameDimension.Page, i);

                // Frame als Bitmap kopieren
                var frameBitmap = new Bitmap(tiffImage);

                // Optional: Größe anpassen
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

    public bool ApplyFilter(string name, float factor) {
        if (IsDisposed || _bitmapData == null) { return false; }
        if (!_isLocked || _bits == null) {
            Develop.DebugPrint("unlocked!");
            return false;
        }

        var f = ImageFilters.GetByKey(name);
        if (f == null) { return false; }

        f.ProcessFilter(_bitmapData, _bits, factor, 0);

        return true;
    }

    public void ApplyFilter(string name) => ApplyFilter(name, 0);

    public void CloneFromBitmap(Bitmap? sourceBmp) {
        UnlockBits(false);
        _bitmap?.Dispose(); // Dispose of any existing bitmap
        _bitmap = null;

        if (sourceBmp == null) {
            Width = -1;
            Height = -1;
            return;
        }

        // Initialisieren einer Stopwatch für die Zeitüberwachung
        var tim = Stopwatch.StartNew();
        do {
            try {
                Width = sourceBmp.Width;
                Height = sourceBmp.Height;

                _bitmap = new Bitmap(Width, Height, Pixelformat);
                using var gr = Graphics.FromImage(_bitmap);
                gr.DrawImage(sourceBmp, new Rectangle(0, 0, Width, Height));

                LockBits();
            } catch (Exception ex) {
                // Überprüfen, ob die Zeitüberschreitung erreicht ist
                if (tim.Elapsed.TotalSeconds > 5) {
                    // Protokollieren des Fehlers
                    tim.Stop();
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
    }

    public Bitmap? CloneOfBitmap() {
        //public static explicit operator Bitmap?(BitmapExt? p) {
        if (_bitmap == null) { return null; }
        UnlockBits(true);
        var bmp = _bitmap.Clone();
        LockBits();
        return bmp as Bitmap;
    }

    public Bitmap Crop(Rectangle re) {
        UnlockBits(true);
        var bmp = new Bitmap(re.Width, re.Height);

        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.Transparent);
        gr.PixelOffsetMode = PixelOffsetMode.Half;
        UnlockBits(true);
        if (_bitmap != null) { gr.DrawImage(_bitmap, re with { X = 0, Y = 0 }, re.Left, re.Top, re.Width, re.Height, GraphicsUnit.Pixel); }

        LockBits();
        return bmp;
    }

    /// <summary>
    /// Stimmen x,y, width oder height nicht, werden sie in den Bereich eingepasst. So, dass immer ein bitmap mit heigth/Width rauskommt.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public Bitmap Crop(int x, int y, int width, int height) {
        if (_bitmap != null) {
            if (x < 0) { x = 0; }
            if (y < 0) { y = 0; }
            if (x + width > _bitmap.Width) { width = _bitmap.Width - x; }
            if (y + height > _bitmap.Height) { height = _bitmap.Height - y; }
        }

        return Crop(new Rectangle(x, y, width, height));
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void FromFile(string filename, bool setDummyPicIfFails) {
        //TODO: Unused
        var x = (Bitmap?)Image_FromFile(filename);
        if (x == null && setDummyPicIfFails) {
            x = QuickImage.Get(ImageCode.Warnung);
        }
        CloneFromBitmap(x);
    }

    public Color GetPixel(int x, int y) {
        if (!_isLocked || _bits == null) {
            Develop.DebugPrint("unlocked!");
            return Color.Transparent;
        }

        if (_bitmapData == null || x < 0 || y < 0 || x >= Width || y >= Height) { return Color.Transparent; }

        var index = (y * _bitmapData.Stride) + (x * 4); // 4 bytes per pixel in ARGB
        var blue = _bits[index];
        var green = _bits[index + 1];
        var red = _bits[index + 2];
        var alpha = _bits[index + 3];

        return Color.FromArgb(alpha, red, green, blue);
    }

    public void LockBits() {
        if (_isLocked) { return; }
        if (_bitmap == null) { return; }

        // Definieren Sie den Bereich des Bitmaps, der gesperrt werden soll.
        var lockArea = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

        // Sperren Sie den Bitmap-Bereich und erhalten Sie die BitmapData.
        _bitmapData = _bitmap.LockBits(lockArea, ImageLockMode.ReadWrite, Pixelformat);

        _bits = new byte[_bitmapData.Stride * _bitmapData.Height];
        Marshal.Copy(_bitmapData.Scan0, _bits, 0, _bits.Length);

        _isLocked = true;
    }

    public void MakeTransparent(Color color) {
        //TODO: Unused
        UnlockBits(true);
        _bitmap?.MakeTransparent(color);
        LockBits();
    }

    public void Save(string name, ImageFormat imageFormat) {
        UnlockBits(true);
        _bitmap?.Save(name, imageFormat);
        LockBits();
    }

    public void SetPixel(int x, int y, Color color) {
        if (!_isLocked || _bits == null) {
            Develop.DebugPrint("unlocked!");
            return;
        }

        if (_bitmapData == null) { return; }

        var index = (y * _bitmapData.Stride) + (x * 4); // 4 bytes per pixel in ARGB
        _bits[index] = color.B;
        _bits[index + 1] = color.G;
        _bits[index + 2] = color.R;
        _bits[index + 3] = color.A;
    }

    public void UnlockBits(bool copyback) {
        if (!_isLocked) { return; }
        if (_bitmap == null) {
            _isLocked = false;
            return;
        }

        if (copyback && _bits != null && _bitmapData != null) {
            Marshal.Copy(_bits, 0, _bitmapData.Scan0, _bits.Length);
        }

        try {
            // Entsperren Sie den Bitmap-Bereich.
            _bitmap.UnlockBits(_bitmapData); // Finalizer kann Probleme machen
        } catch { }

        _isLocked = false;
    }

    protected void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)

                UnlockBits(false); // Stellen Sie sicher, dass die Bitmap freigegeben wird
                _bitmap?.Dispose();
                _bitmap = null;
            }
            // Nicht verwaltete Ressourcen (Bitmap, Tabellenverbindungen, ...)
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

    #endregion
}