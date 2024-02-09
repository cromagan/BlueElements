﻿// Authors:
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

    private Bitmap? _bitmap;

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

    public int Height { get; private set; }
    public bool IsDisposed { get; private set; }
    public int Width { get; private set; }
    private int[]? Bits { get; set; }
    private GCHandle BitsHandle { get; set; }

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

    //    // http://csharphelper.com/blog/2016/12/provide-gamma-correction-for-an-image-in-c/
    //    // Set the ImageAttributes object's gamma value.
    //    ImageAttributes attributes = new();
    //    attributes.SetGamma(Math.Max(gamma, 0.001f));
    //    // Draw the image onto the new bitmap
    //    // while applying the new gamma value.
    //    Point[] points = { new(0, 0), new(image.Width, 0), new(0, image.Height) };
    //    Rectangle rect = new(0, 0, image.Width, image.Height);
    //    // Make the result bitmap.
    //    Bitmap bm = new(image.Width, image.Height);
    //    using var gr = Graphics.FromImage(bm);
    //    gr.DrawImage(image, points, rect,
    //        GraphicsUnit.Pixel, attributes);
    //    // Return the result.
    //    return bm;
    //}
    public static Bitmap? Area(Bitmap sourceBitmap, Rectangle r) {
        if (r.Width < 2 || r.Height < 2) { return null; }
        Bitmap clipedArea = new(r.Width, r.Height);
        using var gr = Graphics.FromImage(clipedArea);
        gr.Clear(Color.Black);
        gr.DrawImage(sourceBitmap, 0, 0, r, GraphicsUnit.Pixel);
        return clipedArea;
    }

    //public static Bitmap? AdjustGamma(Bitmap? image, float gamma) {
    //    if (image == null) { return null; }
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

    //    value = (100.0f + value) / 100.0f;
    //    value *= value;
    //    var newBitmap = Image_Clone(image);
    //    if (newBitmap == null) { return null; }
    //    var data = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
    //    var height = newBitmap.Height;
    //    var width = newBitmap.Width;
    //    unsafe {
    //        for (var y = 0; y < height; ++y) {
    //            var row = (byte*)data.Scan0 + (y * data.Stride);
    //            var columnOffset = 0;
    //            for (var x = 0; x < width; ++x) {
    //                var b = row[columnOffset];
    //                var g = row[columnOffset + 1];
    //                var r = row[columnOffset + 2];
    //                var red = r / 255.0f;
    //                var green = g / 255.0f;
    //                var blue = b / 255.0f;
    //                red = (((red - 0.5f) * value) + 0.5f) * 255.0f;
    //                green = (((green - 0.5f) * value) + 0.5f) * 255.0f;
    //                blue = (((blue - 0.5f) * value) + 0.5f) * 255.0f;
    //                var iR = (int)red;
    //                iR = iR > 255 ? 255 : iR;
    //                iR = iR < 0 ? 0 : iR;
    //                var iG = (int)green;
    //                iG = iG > 255 ? 255 : iG;
    //                iG = iG < 0 ? 0 : iG;
    //                var iB = (int)blue;
    //                iB = iB > 255 ? 255 : iB;
    //                iB = iB < 0 ? 0 : iB;
    //                row[columnOffset] = (byte)iB;
    //                row[columnOffset + 1] = (byte)iG;
    //                row[columnOffset + 2] = (byte)iR;
    //                columnOffset += 4;
    //            }
    //        }
    //    }
    //    newBitmap.UnlockBits(data);
    //    return newBitmap;
    //}
    public static Bitmap? AutoCrop(Bitmap? pic, double minBrightness) {
        var pa = GetAutoValuesForCrop(pic, minBrightness);
        return Crop(pic, pa.Left, pa.Right, pa.Top, pa.Bottom);
    }

    //public static Bitmap? AdjustContrast(Bitmap? image, float value) {
    //    if (image == null) { return null; }
    public static Bitmap? Crop(Bitmap? bmp, Rectangle r) {
        if (bmp == null) { return null; }
        return Crop(bmp, r.Left, -(bmp.Width - r.Right), r.Top, -(bmp.Height - r.Bottom));
    }

    //    // http://csharphelper.com/blog/2014/10/use-an-imageattributes-object-to-adjust-an-images-brightness-in-c/
    //    // Make the ColorMatrix.
    //    var b = Math.Max(brightness, 0.001f);
    //    ColorMatrix cm = new(new[]
    //    {
    //        new[] {b, 0, 0, 0, 0}, new[] {0, b, 0, 0, 0}, new[] {0, 0, b, 0, 0}, new[] {0, 0, 0, 1f, 0},
    //        new[] {0, 0, 0, 0, 1f}
    //    });
    //    ImageAttributes attributes = new();
    //    attributes.SetColorMatrix(cm);
    //    // Draw the image onto the new bitmap while applying
    //    // the new ColorMatrix.
    //    Point[] points =
    //    {
    //        new(0, 0), new(image.Width, 0), new(0, image.Height)
    //    };
    //    Rectangle rect = new(0, 0, image.Width, image.Height);
    //    // Make the result bitmap.
    //    Bitmap bm = new(image.Width, image.Height);
    //    using var gr = Graphics.FromImage(bm);
    //    gr.DrawImage(image, points, rect,
    //        GraphicsUnit.Pixel, attributes);
    //    // Return the result.
    //    return bm;
    //}
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

    //public static Bitmap? AdjustBrightness(Bitmap? image, float brightness) {
    //    if (image == null) { return null; }
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

    //// ReSharper disable once UnusedMember.Global
    //public static Bitmap? ImageBlurFilter(Bitmap? bmp, BlurType blurType) {
    //    if (bmp == null) { return null; }

    //    Bitmap? resultBitmap = null;
    //    switch (blurType) {
    //        case BlurType.Mean3x3:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.Mean3X3, 1.0 / 9.0, 0);
    //            break;

    //        case BlurType.Mean5x5:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.Mean5X5, 1.0 / 25.0, 0);
    //            break;

    //        case BlurType.Mean7x7:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.Mean7X7, 1.0 / 49.0, 0);
    //            break;

    //        case BlurType.Mean9x9:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.Mean9X9, 1.0 / 81.0, 0);

    //            break;

    //        case BlurType.GaussianBlur3x3:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.GaussianBlur3X3, 1.0 / 16.0, 0);
    //            break;

    //        case BlurType.GaussianBlur5x5:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.GaussianBlur5X5, 1.0 / 159.0, 0);
    //            break;

    //        case BlurType.MotionBlur5x5:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur5X5, 1.0 / 10.0, 0);
    //            break;

    //        case BlurType.MotionBlur5x5At45Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur5X5At45Degrees, 1.0 / 5.0, 0);
    //            break;

    //        case BlurType.MotionBlur5x5At135Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur5X5At135Degrees, 1.0 / 5.0, 0);
    //            break;

    //        case BlurType.MotionBlur7x7:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur7X7, 1.0 / 14.0, 0);
    //            break;

    //        case BlurType.MotionBlur7x7At45Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur7X7At45Degrees, 1.0 / 7.0, 0);
    //            break;

    //        case BlurType.MotionBlur7x7At135Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur7X7At135Degrees, 1.0 / 7.0, 0);
    //            break;

    //        case BlurType.MotionBlur9x9:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur9X9, 1.0 / 18.0, 0);
    //            break;

    //        case BlurType.MotionBlur9x9At45Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur9X9At45Degrees, 1.0 / 9.0, 0);
    //            break;

    //        case BlurType.MotionBlur9x9At135Degrees:
    //            resultBitmap = ConvolutionFilter(bmp, ImageMatrix.MotionBlur9X9At135Degrees, 1.0 / 9.0, 0);
    //            break;

    //        case BlurType.Median3x3:
    //            resultBitmap = MedianFilter(bmp, 3);
    //            break;

    //        case BlurType.Median5x5:
    //            resultBitmap = MedianFilter(bmp, 5);
    //            break;

    //        case BlurType.Median7x7:
    //            resultBitmap = MedianFilter(bmp, 7);
    //            break;

    //        case BlurType.Median9x9:
    //            resultBitmap = MedianFilter(bmp, 9);
    //            break;

    //        case BlurType.Median11x11:
    //            resultBitmap = MedianFilter(bmp, 11);
    //            break;
    //    }
    //    return resultBitmap;
    //}

    public static implicit operator Bitmap?(BitmapExt? p) => p?._bitmap;

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

    public static Bitmap MedianFilter(Bitmap sourceBitmap, int matrixSize) {
        var sourceData =
            sourceBitmap.LockBits(new Rectangle(0, 0,
                    sourceBitmap.Width, sourceBitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
        var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
        var resultBuffer = new byte[sourceData.Stride * sourceData.Height];
        Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
        sourceBitmap.UnlockBits(sourceData);
        var filterOffset = (matrixSize - 1) / 2;
        List<int> neighbourPixels = [];
        for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++) {
            for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++) {
                var byteOffset = (offsetY * sourceData.Stride) + (offsetX * 4);
                neighbourPixels.Clear();
                for (var filterY = -filterOffset; filterY <= filterOffset; filterY++) {
                    for (var filter = -filterOffset; filter <= filterOffset; filter++) {
                        var calcOffset = byteOffset + (filter * 4) + (filterY * sourceData.Stride);
                        neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                    }
                }
                neighbourPixels.Sort();
                var middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);
                resultBuffer[byteOffset] = middlePixel[0];
                resultBuffer[byteOffset + 1] = middlePixel[1];
                resultBuffer[byteOffset + 2] = middlePixel[2];
                resultBuffer[byteOffset + 3] = middlePixel[3];
            }
        }
        Bitmap resultBitmap = new(sourceBitmap.Width, sourceBitmap.Height);
        var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
        resultBitmap.UnlockBits(resultData);
        return resultBitmap;
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
                if (x != null) l.Add(x.Resize(maxSize, maxSize, SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));
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

    public void AdjustBrightness(float brightness) {
        if (Bits == null) { return; }

        brightness = Math.Max(brightness, 0.001f); // Stellen Sie sicher, dass die Helligkeit nicht 0 ist

        for (int i = 0; i < Bits.Length; i++) {
            int argb = Bits[i];

            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            int a = (argb >> 24) & 0xff;
            int r = (argb >> 16) & 0xff;
            int g = (argb >> 8) & 0xff;
            int b = argb & 0xff;

            // Anpassen der Helligkeit für jede Farbkomponente
            r = (int)Math.Min(255, r * brightness);
            g = (int)Math.Min(255, g * brightness);
            b = (int)Math.Min(255, b * brightness);

            // Kombinieren der Komponenten zurück in ein Pixel
            Bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

    public void AdjustContrast(float value) {
        if (Bits == null) { return; }

        value = (100.0f + value) / 100.0f;
        value *= value;

        for (int i = 0; i < Bits.Length; i++) {
            int argb = Bits[i];

            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            int a = (argb >> 24) & 0xff;
            int r = (argb >> 16) & 0xff;
            int g = (argb >> 8) & 0xff;
            int b = argb & 0xff;

            // Anpassen des Kontrasts für jede Farbkomponente und Begrenzen der Farbwerte
            r = (int)Math.Max(0, Math.Min(255, (((r / 255f) - 0.5f) * value + 0.5f) * 255.0f));
            g = (int)Math.Max(0, Math.Min(255, (((g / 255f) - 0.5f) * value + 0.5f) * 255.0f));
            b = (int)Math.Max(0, Math.Min(255, (((b / 255f) - 0.5f) * value + 0.5f) * 255.0f));

            // Kombinieren der Komponenten zurück in ein Pixel und direkt im Array aktualisieren
            Bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

    public void AdjustGamma(float gamma) {
        if (Bits == null) { return; }

        // Stellen Sie sicher, dass der Gamma-Wert gültig ist
        gamma = Math.Max(gamma, 0.001f);

        // Erstellen Sie eine Lookup-Tabelle für die Gamma-Korrektur
        int[] gammaArray = new int[256];
        for (int i = 0; i < 256; ++i) {
            gammaArray[i] = (int)Math.Min(255,
                (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
        }

        // Anpassen des Gammas für jedes Pixel im Bits-Array
        for (int i = 0; i < Bits.Length; i++) {
            int argb = Bits[i];
            int a = (argb >> 24) & 0xff; // Alpha-Komponente
            int r = (argb >> 16) & 0xff; // Rot-Komponente
            int g = (argb >> 8) & 0xff;  // Grün-Komponente
            int b = argb & 0xff;         // Blau-Komponente

            // Gamma-Korrektur für jede Farbkomponente
            r = gammaArray[r];
            g = gammaArray[g];
            b = gammaArray[b];

            // Kombinieren der Komponenten zurück in ein Pixel und direkt im Array aktualisieren
            Bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

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
                Bits = new int[Width * Height];

                // Fixieren des Bits-Arrays im Speicher, damit es nicht von der Garbage Collection verschoben wird.
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

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
        if (Bits != null && x >= 0 && y >= 0 && x < Width && y < Height) { return Color.FromArgb(Bits[x + (y * Width)]); }
        return Color.Transparent;
    }

    public void MakeTransparent(Color color) => _bitmap?.MakeTransparent(color);

    public void Save(string name, ImageFormat imageFormat) => _bitmap?.Save(name, imageFormat);

    public void SetPixel(int x, int y, Color color) {
        if (Bits != null && x >= 0 && y >= 0 && x < Width && y < Height) {
            Bits[x + (y * Width)] = color.ToArgb();
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
        Bits = new int[Width * Height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

        if (Width < 1 || height < 1) {
            _bitmap = null;
            return;
        }

        _bitmap = new Bitmap(Width, Height, Width * 4, Pixelformat, BitsHandle.AddrOfPinnedObject());
    }

    private static Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,] filterMatrix, double factor, int bias) {
        var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
        var resultBuffer = new byte[sourceData.Stride * sourceData.Height];
        Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
        sourceBitmap.UnlockBits(sourceData);
        var filterWidth = filterMatrix.GetLength(1);
        // var filterHeight = filterMatrix.GetLength(0);
        var filterOffset = (filterWidth - 1) / 2;
        for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++) {
            for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++) {
                double blue = 0;
                double green = 0;
                double red = 0;
                var byteOffset = (offsetY * sourceData.Stride) + (offsetX * 4);
                for (var filterY = -filterOffset; filterY <= filterOffset; filterY++) {
                    for (var filter = -filterOffset; filter <= filterOffset; filter++) {
                        var calcOffset = byteOffset + (filter * 4) + (filterY * sourceData.Stride);
                        blue += pixelBuffer[calcOffset] * filterMatrix[filterY + filterOffset, filter + filterOffset];
                        green += pixelBuffer[calcOffset + 1] * filterMatrix[filterY + filterOffset, filter + filterOffset];
                        red += pixelBuffer[calcOffset + 2] * filterMatrix[filterY + filterOffset, filter + filterOffset];
                    }
                }
                blue = (factor * blue) + bias;
                green = (factor * green) + bias;
                red = (factor * red) + bias;
                blue = blue > 255 ? 255 : blue < 0 ? 0 : blue;
                green = green > 255 ? 255 : green < 0 ? 0 : green;
                red = red > 255 ? 255 : red < 0 ? 0 : red;
                resultBuffer[byteOffset] = (byte)blue;
                resultBuffer[byteOffset + 1] = (byte)green;
                resultBuffer[byteOffset + 2] = (byte)red;
                resultBuffer[byteOffset + 3] = 255;
            }
        }
        Bitmap resultBitmap = new(sourceBitmap.Width, sourceBitmap.Height);
        var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
        resultBitmap.UnlockBits(resultData);
        return resultBitmap;
    }

    #endregion
}