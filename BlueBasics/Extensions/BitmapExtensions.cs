#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BlueBasics {
    public static partial class Extensions {
        public static Bitmap Area(this Bitmap sourceBitmap, Rectangle r) {
            if (r.Width < 2 || r.Height < 2) { return null; }
            Bitmap ClipedArea = new(r.Width, r.Height);
            using (var GR = Graphics.FromImage(ClipedArea)) {
                GR.Clear(Color.Black);
                GR.DrawImage(sourceBitmap, 0, 0, r, GraphicsUnit.Pixel);
            }
            return ClipedArea;
        }
        public static void AllePixelZuSchwarz(this Bitmap _Pic, double nearWhiteSchwelle) {
            for (var x = 0; x < _Pic.Width; x++) {
                for (var y = 0; y < _Pic.Height; y++) {
                    var ca = _Pic.GetPixel(x, y);
                    if (!ca.IsNearWhite(nearWhiteSchwelle)) {
                        _Pic.SetPixel(x, y, Color.FromArgb(ca.A, 0, 0, 0));
                    }
                }
            }
        }
        public static void AllePixelZuWeiß(this Bitmap _Pic, double nearBlackSchwelle) {
            for (var x = 0; x < _Pic.Width; x++) {
                for (var y = 0; y < _Pic.Height; y++) {
                    var ca = _Pic.GetPixel(x, y);
                    if (!ca.IsNearBlack(nearBlackSchwelle)) {
                        _Pic.SetPixel(x, y, Color.FromArgb(ca.A, 255, 255, 255));
                    }
                }
            }
        }
        public static void Ausdünnen(this Bitmap pic, int staerke) {
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
            bool IsWhite(int x, int y) {
                return x < 0 || y < 0 || x >= pic.Width || y >= pic.Height || pic.GetPixel(x, y).IsNearWhite(0.9);
            }
        }
        public static Bitmap Image_Clone(this Bitmap sourceBMP) {
            if (sourceBMP == null) { return null; }
            Bitmap bmp = new(sourceBMP.Width, sourceBMP.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp)) {
                g.DrawImage(sourceBMP, 0, 0, sourceBMP.Width, sourceBMP.Height); // Unerklärlich, orgiImage.Width, orgiImage.Height muss stehen bleiben!
            }
            return bmp;
        }
        public static Bitmap Invert(this Bitmap source) {
            // create a blank bitmap the same size as original
            Bitmap newBitmap = new(source.Width, source.Height);
            // get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);
            // create the negative color matrix
            ColorMatrix colorMatrix = new(new[]
            {
                new float[] {-1, 0, 0, 0, 0},
                new float[] {0, -1, 0, 0, 0},
                new float[] {0, 0, -1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
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
        public static Bitmap Crop(this Bitmap _Pic, Rectangle r) => _Pic.Crop(r.Left, -(_Pic.Width - r.Right), r.Top, -(_Pic.Height - r.Bottom));
        /// <summary>
        ///
        /// </summary>
        /// <param name="_Pic"></param>
        /// <param name="left">Positiver Wert schneidet diese Anzahl von Pixel vom linken Rand weg.</param>
        /// <param name="right">Negativer Wert schneidet diese Anzahl von Pixel vom rechten Rand weg.</param>
        /// <param name="top">Positiver Wert schneidet diese Anzahl von Pixel vom oberen Rand weg.</param>
        /// <param name="bottom">Negativer Wert schneidet diese Anzahl von Pixel vom unteren Rand weg.</param>
        /// <returns></returns>
        public static Bitmap Crop(this Bitmap _Pic, int left, int right, int top, int bottom) {
            if (left == 0 && right == 0 && top == 0 && bottom == 0) { return _Pic; }
            if (_Pic == null) { return null; }
            modAllgemein.CollectGarbage();
            var w = Math.Max(_Pic.Width - left + right, 1);
            var h = Math.Max(_Pic.Height - top + bottom, 1);
            Bitmap _BMP2 = new(w, h);
            using (var GR = Graphics.FromImage(_BMP2)) {
                GR.DrawImage(_Pic, -left, -top, _Pic.Width, _Pic.Height); // Width und Height MUSS angegeben werden. Manche Bilder (Falsches Format?) schlagen fehl, wenn es fehlt.
            }
            modAllgemein.CollectGarbage();
            return _BMP2;
        }
        public static Bitmap AutoCrop(this Bitmap _Pic, double minBrightness) {
            var pa = _Pic.GetAutoValuesForCrop(minBrightness);
            return Crop(_Pic, pa.Left, pa.Right, pa.Top, pa.Bottom);
        }
        public static System.Windows.Forms.Padding GetAutoValuesForCrop(this Bitmap _Pic, double minBrightness) {
            System.Windows.Forms.Padding pa = new(0, 0, 0, 0);
            if (_Pic == null) { return pa; }
            var x = 0;
            var ExitNow = false;
            int Y;
            while (true) {
                for (Y = 0; Y < _Pic.Height; Y++) {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(minBrightness)) {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                x++;
                if (x > _Pic.Width * 0.9) { break; }
            }
            pa.Left = x;
            // -------------
            x = _Pic.Width - 1;
            ExitNow = false;
            while (true) {
                for (Y = 0; Y < _Pic.Height; Y++) {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(minBrightness)) {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                x--;
                if (x < _Pic.Width * 0.1) { break; }
            }
            pa.Right = x - _Pic.Width + 1;
            // -------------
            Y = 0;
            ExitNow = false;
            while (true) {
                for (x = 0; x < _Pic.Width; x++) {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(minBrightness)) {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                Y++;
                if (Y > _Pic.Height * 0.9) { break; }
            }
            pa.Top = Y;
            // -------------
            Y = _Pic.Height - 1;
            ExitNow = false;
            while (true) {
                for (x = 0; x < _Pic.Width; x++) {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(minBrightness)) {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                Y--;
                if (Y < _Pic.Height * 0.1) { break; }
            }
            pa.Bottom = Y - _Pic.Height + 1;
            return pa;
        }
        public static Bitmap AdjustGamma(this Bitmap image, float gamma) {
            // http://csharphelper.com/blog/2016/12/provide-gamma-correction-for-an-image-in-c/
            // Set the ImageAttributes object's gamma value.
            ImageAttributes attributes = new();
            attributes.SetGamma(Math.Max(gamma, 0.001f));
            // Draw the image onto the new bitmap
            // while applying the new gamma value.
            Point[] points =
            {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
            Rectangle rect =
                new(0, 0, image.Width, image.Height);
            // Make the result bitmap.
            Bitmap bm = new(image.Width, image.Height);
            using (var gr = Graphics.FromImage(bm)) {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            // Return the result.
            return bm;
        }
        public static Bitmap AdjustBrightness(this Bitmap image, float brightness) {
            // http://csharphelper.com/blog/2014/10/use-an-imageattributes-object-to-adjust-an-images-brightness-in-c/
            // Make the ColorMatrix.
            var b = Math.Max(brightness, 0.001f);
            ColorMatrix cm = new(new float[][]
                {
            new float[] {b, 0, 0, 0, 0},
            new float[] {0, b, 0, 0, 0},
            new float[] {0, 0, b, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1},
                });
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(cm);
            // Draw the image onto the new bitmap while applying
            // the new ColorMatrix.
            Point[] points =
            {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
            Rectangle rect = new(0, 0, image.Width, image.Height);
            // Make the result bitmap.
            Bitmap bm = new(image.Width, image.Height);
            using (var gr = Graphics.FromImage(bm)) {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            // Return the result.
            return bm;
        }
        public static Bitmap AdjustContrast(this Bitmap image, float value) {
            value = (100.0f + value) / 100.0f;
            value *= value;
            var NewBitmap = Image_Clone(image);
            var data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
            var Height = NewBitmap.Height;
            var Width = NewBitmap.Width;
            unsafe {
                for (var y = 0; y < Height; ++y) {
                    var row = (byte*)data.Scan0 + (y * data.Stride);
                    var columnOffset = 0;
                    for (var x = 0; x < Width; ++x) {
                        var B = row[columnOffset];
                        var G = row[columnOffset + 1];
                        var R = row[columnOffset + 2];
                        var Red = R / 255.0f;
                        var Green = G / 255.0f;
                        var Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * value) + 0.5f) * 255.0f;
                        var iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        var iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        var iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;
                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;
                        columnOffset += 4;
                    }
                }
            }
            NewBitmap.UnlockBits(data);
            return NewBitmap;
        }
        public static unsafe Bitmap ReplaceColor(this Bitmap source, Color toReplace, Color replacement) {
            // https://stackoverflow.com/questions/17208254/how-to-change-pixel-color-of-an-image-in-c-net
            const int pixelSize = 4; // 32 bits per pixel
            Bitmap target = new(source.Width, source.Height, PixelFormat.Format32bppArgb);
            BitmapData sourceData = null, targetData = null;
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
        public static Bitmap Grayscale(this Bitmap original) {
            Bitmap newBitmap = new(original.Width, original.Height);
            var g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new(
               new[]
               {
                   new[] {.3f, .3f, .3f, 0, 0},
                   new[] {.59f, .59f, .59f, 0, 0},
                   new[] {.11f, .11f, .11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
               });
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();
            return newBitmap;
        }
        /// <summary>
        /// Helligkeit, Kontrast und Gammawert eines Bitmaps ändern
        /// </summary>
        /// <param name="inBitmap">Bitmap-Objekt</param>
        /// <param name="brightness">Heligkeit (-1 bis 1) 0 = Normal</param>
        /// <param name="contrast">Kontrast (-1 bis 1) 0 = Normal</param>
        /// <param name="gamma">Gammawert (0 bis 2) 1 = Normal</param>
        /// <returns>Bitmap-Objekt</returns>
        public static Bitmap SetBrightnessContrastGamma(this Bitmap inBitmap, float brightness, float contrast, float gamma) {
            // Min/Max
            if (brightness > 1) { brightness = 1; }
            if (brightness < -1) { brightness = -1; }
            if (contrast > 1) { contrast = 1; }
            if (contrast < -1) { contrast = -1; }
            // Gammawert darf nicht = 0 sein (Bug in GDI+)
            if (gamma == 0) { gamma = Convert.ToSingle(gamma + 1.0E-45); }
            // Zur korrekten Darstellung:
            var Diff = (brightness / 2) - (contrast / 2);
            // ColorMatrix erstellen
            ColorMatrix Matrix = new(new[]
            {
                new[] {1 + contrast, 0, 0, 0, 0},
                new[] {0, 1 + contrast, 0, 0, 0},
                new[] {0, 0, 1 + contrast, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new[] {brightness + Diff, brightness + Diff, brightness + Diff, 0, 1}
            });
            // Neue Bitmap erstellen
            Bitmap NewBmp = new(inBitmap.Width, inBitmap.Height, PixelFormat.Format24bppRgb);
            // ImageAttribute-Objekt erstellen
            using (ImageAttributes ImageAttr = new()) {
                // ColorMatrix für das ImageAttribute-Objekt setzen
                ImageAttr.SetColorMatrix(Matrix);
                // Gamma für das ImageAttribute-Objekt setzen
                ImageAttr.SetGamma(gamma);
                // Graphics-Objekt von NewBmp erstellen
                using var NewBmpGra = Graphics.FromImage(NewBmp);                     // InBitmap in das Graphics-Objekt zeichnen
                NewBmpGra.DrawImage(inBitmap, new Rectangle(0, 0, inBitmap.Width, inBitmap.Height), 0, 0, inBitmap.Width, inBitmap.Height, GraphicsUnit.Pixel, ImageAttr);
                // Graphics-Objekt löschen
                // ImageAttribute-Objekt löschen
            }
            return NewBmp;
        }
        public static void FillCircle(this Bitmap bMP, Color c, int x, int y, int r) {
            for (var adx = -r; adx <= r; adx++) {
                for (var ady = -r; ady <= r; ady++) {
                    var d = Math.Sqrt(Convert.ToDouble((adx * adx) + (ady * ady))) - 0.5;
                    var px = x + adx;
                    var py = y + ady;
                    if (px >= 0 && py >= 0 && px < bMP.Width && py < bMP.Height && d <= r) {
                        bMP.SetPixel(px, py, c);
                    }
                }
            }
        }
        public static Bitmap ImageBlurFilter(this Bitmap sourceBitmap, BlurType blurType) {
            Bitmap resultBitmap = null;
            switch (blurType) {

                case BlurType.Mean3x3: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean3x3, 1.0 / 9.0, 0);
                    }
                    break;

                case BlurType.Mean5x5: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean5x5, 1.0 / 25.0, 0);
                    }
                    break;

                case BlurType.Mean7x7: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean7x7, 1.0 / 49.0, 0);
                    }
                    break;

                case BlurType.Mean9x9: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean9x9, 1.0 / 81.0, 0);
                    }
                    break;

                case BlurType.GaussianBlur3x3: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.GaussianBlur3x3, 1.0 / 16.0, 0);
                    }
                    break;

                case BlurType.GaussianBlur5x5: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.GaussianBlur5x5, 1.0 / 159.0, 0);
                    }
                    break;

                case BlurType.MotionBlur5x5: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5, 1.0 / 10.0, 0);
                    }
                    break;

                case BlurType.MotionBlur5x5At45Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5At45Degrees, 1.0 / 5.0, 0);
                    }
                    break;

                case BlurType.MotionBlur5x5At135Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5At135Degrees, 1.0 / 5.0, 0);
                    }
                    break;

                case BlurType.MotionBlur7x7: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7, 1.0 / 14.0, 0);
                    }
                    break;

                case BlurType.MotionBlur7x7At45Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7At45Degrees, 1.0 / 7.0, 0);
                    }
                    break;

                case BlurType.MotionBlur7x7At135Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7At135Degrees, 1.0 / 7.0, 0);
                    }
                    break;

                case BlurType.MotionBlur9x9: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9, 1.0 / 18.0, 0);
                    }
                    break;

                case BlurType.MotionBlur9x9At45Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9At45Degrees, 1.0 / 9.0, 0);
                    }
                    break;

                case BlurType.MotionBlur9x9At135Degrees: {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9At135Degrees, 1.0 / 9.0, 0);
                    }
                    break;

                case BlurType.Median3x3: {
                        resultBitmap = sourceBitmap.MedianFilter(3);
                    }
                    break;

                case BlurType.Median5x5: {
                        resultBitmap = sourceBitmap.MedianFilter(5);
                    }
                    break;

                case BlurType.Median7x7: {
                        resultBitmap = sourceBitmap.MedianFilter(7);
                    }
                    break;

                case BlurType.Median9x9: {
                        resultBitmap = sourceBitmap.MedianFilter(9);
                    }
                    break;

                case BlurType.Median11x11: {
                        resultBitmap = sourceBitmap.MedianFilter(11);
                    }
                    break;
            }
            return resultBitmap;
        }
        public static Bitmap MedianFilter(this Bitmap sourceBitmap, int matrixSize) {
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
            List<int> neighbourPixels = new();
            byte[] middlePixel;
            for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++) {
                for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++) {
                    var byteOffset = (offsetY * sourceData.Stride) + (offsetX * 4);
                    neighbourPixels.Clear();
                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++) {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++) {
                            var calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                        }
                    }
                    neighbourPixels.Sort();
                    middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);
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
        private static Bitmap ConvolutionFilter(this Bitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0) {
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
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++) {
                            var calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            blue += pixelBuffer[calcOffset] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            green += pixelBuffer[calcOffset + 1] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            red += pixelBuffer[calcOffset + 2] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    blue = (factor * blue) + bias;
                    green = (factor * green) + bias;
                    red = (factor * red) + bias;
                    blue = blue > 255 ? 255 : (blue < 0 ? 0 : blue);
                    green = green > 255 ? 255 : (green < 0 ? 0 : green);
                    red = red > 255 ? 255 : (red < 0 ? 0 : red);
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
    }
}