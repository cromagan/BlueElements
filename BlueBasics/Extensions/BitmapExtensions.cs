#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BlueBasics.Enums;

namespace BlueBasics
{
    public static partial class Extensions
    {


        public static Bitmap Area(this Bitmap SourceBitmap, Rectangle R)
        {

            if (R.Width < 2 || R.Height < 2) { return null; }

            var ClipedArea = new Bitmap(R.Width, R.Height);

            using (var GR = Graphics.FromImage(ClipedArea))
            {
                GR.Clear(Color.Black);
                GR.DrawImage(SourceBitmap, 0, 0, R, GraphicsUnit.Pixel);
            }


            return ClipedArea;

        }

        public static void AllePixelZuSchwarz(this Bitmap _Pic, double nearWhiteSchwelle)
        {
            for (var x = 0; x < _Pic.Width; x++)
            {
                for (var y = 0; y < _Pic.Height; y++)
                {
                    var ca = _Pic.GetPixel(x, y);
                    if (!ca.IsNearWhite(nearWhiteSchwelle))
                    {
                        _Pic.SetPixel(x, y, Color.FromArgb(ca.A, 0, 0, 0));
                    }
                }
            }
        }

        public static void AllePixelZuWeiß(this Bitmap _Pic, double nearBlackSchwelle)
        {
            for (var x = 0; x < _Pic.Width; x++)
            {
                for (var y = 0; y < _Pic.Height; y++)
                {
                    var ca = _Pic.GetPixel(x, y);
                    if (!ca.IsNearBlack(nearBlackSchwelle))
                    {
                        _Pic.SetPixel(x, y, Color.FromArgb(ca.A, 255, 255, 255));
                    }
                }
            }
        }


        public static void Ausdünnen(this Bitmap pic, int staerke)
        {


            if (pic == null) { return; }


            for (var x = 0; x < pic.Width - 1; x++)
            {
                for (var y = 0; y < pic.Height - 1; y++)
                {
                    if (!IsWhite(x, y))
                    {
                        for (var wi = staerke; wi > 0; wi--)
                        {

                            var ma1 = (int)Math.Floor((float)wi / 2);
                            var ma2 = wi - ma1;

                            // X                        
                            if (IsWhite(x - ma1 - 1, y) && IsWhite(x + ma2 + 1, y))
                            {
                                var allblack = true;

                                for (var ch = -ma1; ch <= ma2; ch++)
                                {
                                    if (IsWhite(x + ch, y)) { allblack = false; break; }
                                }

                                if (allblack)
                                {
                                    for (var ch = -ma1; ch <= ma2; ch++)
                                    {
                                        if (ch != 0) { pic.SetPixel(x + ch, y, Color.White); }
                                    }
                                }
                            }

                            // Y                        
                            if (IsWhite(x, y - ma1 - 1) && IsWhite(x, y + ma2 + 1))
                            {
                                var allblack = true;

                                for (var ch = -ma1; ch <= ma2; ch++)
                                {
                                    if (IsWhite(x, y + ch)) { allblack = false; break; }
                                }

                                if (allblack)
                                {
                                    for (var ch = -ma1; ch <= ma2; ch++)
                                    {
                                        if (ch != 0) { pic.SetPixel(x, y + ch, Color.White); }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            bool IsWhite(int x, int y)
            {
                if (x < 0 || y < 0) { return true; }
                if (x >= pic.Width || y >= pic.Height) { return true; }
                return pic.GetPixel(x, y).IsNearWhite(0.9);
            }


        }



        public static Bitmap Image_Clone(this Bitmap SourceBMP)
        {
            if (SourceBMP == null) { return null; }

            var bmp = new Bitmap(SourceBMP.Width, SourceBMP.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(SourceBMP, 0, 0, SourceBMP.Width, SourceBMP.Height); // Unerklärlich, orgiImage.Width, orgiImage.Height muss stehen bleiben!
            }
            return bmp;
        }
        public static Bitmap Resize(this Bitmap _BMPx, int _Width, int _Height, enSizeModes _SizeMode, InterpolationMode InterpolationMode, bool CollectGarbage)
        {
            if (_BMPx == null) { return null; }
            if (_Width < 1 && _Height < 1) { return null; }

            if (CollectGarbage) { modAllgemein.CollectGarbage(); }


            if (_Width < 1) { _Width = 1; }
            if (_Height < 1) { _Height = 1; }

            var Scale = Math.Min(_Width / (double)_BMPx.Width, _Height / (double)_BMPx.Height);

            switch (_SizeMode)
            {
                case enSizeModes.EmptySpace:
                    break;

                case enSizeModes.BildAbschneiden:
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    _Width = (int)(Scale * _BMPx.Width);
                    _Height = (int)(Scale * _BMPx.Height);
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    if (Scale >= 1) { return _BMPx; }
                    _Width = (int)(Scale * _BMPx.Width);
                    _Height = (int)(Scale * _BMPx.Height);
                    break;

                case enSizeModes.Verzerren:
                    Scale = 1; // Dummy setzen
                    break;

                default:
                    Develop.DebugPrint(_SizeMode);
                    return null;
            }

            var nw = (int)(_BMPx.Width * Scale);
            var nh = (int)(_BMPx.Height * Scale);

            if (_SizeMode == enSizeModes.Verzerren)
            {
                nw = _Width;
                nh = _Height;
            }



            try
            {

                var ImageResize = new Bitmap(_Width, _Height); // Kein Format32bppPArgb --> Fehler
                using (var GR = Graphics.FromImage(ImageResize))
                {
                    GR.InterpolationMode = InterpolationMode;
                    GR.PixelOffsetMode = PixelOffsetMode.Half;


                    // 20000 / 4 = 5000, also noch 1000 zum kleiner machen
                    if (_BMPx.Width > 20000 && nw < 4000)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage((int)(_BMPx.Width / 4.0), (int)(_BMPx.Height / 4.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((_Width - nw) / 2.0), (int)((_Height - nh) / 2.0), nw, nh);
                    }
                    else if (_BMPx.Width > 15000 && nw < 4000)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage((int)(_BMPx.Width / 3.0), (int)(_BMPx.Height / 3.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((_Width - nw) / 2.0), (int)((_Height - nh) / 2.0), nw, nh);
                    }
                    else if (_BMPx.Width > 10000 && nw < 2500)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage((int)(_BMPx.Width / 3.0), (int)(_BMPx.Height / 3.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((_Width - nw) / 2.0), (int)((_Height - nh) / 2.0), nw, nh);
                    }
                    else if (_BMPx.Width > 8000 && nw < 2000)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage((int)(_BMPx.Width / 2.5), (int)(_BMPx.Height / 2.5), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((_Width - nw) / 2.0), (int)((_Height - nh) / 2.0), nw, nh);
                    }
                    else
                    {
                        GR.DrawImage(_BMPx, (int)((_Width - nw) / 2.0), (int)((_Height - nh) / 2.0), nw, nh);
                    }

                }
                return ImageResize;

            }
            catch
            {
                if (!CollectGarbage) { modAllgemein.CollectGarbage(); }
                if (_SizeMode == enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern)
                {
                    return (Bitmap)_BMPx.GetThumbnailImage(nw, nh, null, IntPtr.Zero);
                }
                return null;
            }
        }

        public static Bitmap Invert(this Bitmap source)
        {
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(source.Width, source.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            // create the negative color matrix
            var colorMatrix = new ColorMatrix(new[]
            {
                new float[] {-1, 0, 0, 0, 0},
                new float[] {0, -1, 0, 0, 0},
                new float[] {0, 0, -1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
            });

            // create some image attributes
            var attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            return newBitmap;
        }

        public static Bitmap Crop(this Bitmap _Pic, Rectangle R)
        {
            return _Pic.Crop(R.Left, -(_Pic.Width - R.Right), R.Top, -(_Pic.Height - R.Bottom));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_Pic"></param>
        /// <param name="Left">Positiver Wert schneidet diese Anzahl von Pixel vom linken Rand weg.</param>
        /// <param name="Right">Negativer Wert schneidet diese Anzahl von Pixel vom rechten Rand weg.</param>
        /// <param name="Top">Positiver Wert schneidet diese Anzahl von Pixel vom oberen Rand weg.</param>
        /// <param name="Bottom">Negativer Wert schneidet diese Anzahl von Pixel vom unteren Rand weg.</param>
        /// <returns></returns>
        public static Bitmap Crop(this Bitmap _Pic, int Left, int Right, int Top, int Bottom)
        {
            if (Left == 0 && Right == 0 && Top == 0 && Bottom == 0) { return _Pic; }

            if (_Pic == null) { return null; }

            modAllgemein.CollectGarbage();

            var w = Math.Max((int)(_Pic.Width - Left + Right), 1);
            var h = Math.Max((int)(_Pic.Height - Top + Bottom), 1);

            var _BMP2 = new Bitmap(w, h);
            using (var GR = Graphics.FromImage(_BMP2))
            {
                GR.DrawImage(_Pic, -Left, -Top, _Pic.Width, _Pic.Height); // Width und Height MUSS angegeben werden. Manche Bilder (Falsches Format?) schlagen fehl, wenn es fehlt.
            }

            modAllgemein.CollectGarbage();

            return _BMP2;

        }


        public static Bitmap AutoCrop(this Bitmap _Pic, double MinBrightness)
        {
            var pa = _Pic.GetAutoValuesForCrop(MinBrightness);
            return Crop(_Pic, pa.Left, pa.Right, pa.Top, pa.Bottom);
        }

        public static System.Windows.Forms.Padding GetAutoValuesForCrop(this Bitmap _Pic, double MinBrightness)
        {
            var pa = new System.Windows.Forms.Padding(0, 0, 0, 0);
            if (_Pic == null) { return pa; }

            var x = 0;
            var Y = 0;
            var ExitNow = false;

            do
            {
                for (Y = 0; Y < _Pic.Height; Y++)
                {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(MinBrightness))
                    {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }

                x += 1;
                if (x > _Pic.Width * 0.9) { break; }
            } while (true);

            pa.Left = x;

            // -------------
            x = _Pic.Width - 1;

            ExitNow = false;
            do
            {
                for (Y = 0; Y < _Pic.Height; Y++)
                {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(MinBrightness))
                    {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                x -= 1;
                if (x < _Pic.Width * 0.1) { break; }
            } while (true);

            pa.Right = x - _Pic.Width + 1;


            // -------------
            Y = 0;
            ExitNow = false;
            do
            {
                for (x = 0; x < _Pic.Width; x++)
                {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(MinBrightness))
                    {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                Y += 1;
                if (Y > _Pic.Height * 0.9) { break; }
            } while (true);

            pa.Top = Y;


            // -------------
            Y = _Pic.Height - 1;
            ExitNow = false;
            do
            {
                for (x = 0; x < _Pic.Width; x++)
                {
                    if (!_Pic.GetPixel(x, Y).IsNearWhite(MinBrightness))
                    {
                        ExitNow = true;
                        break;
                    }
                }
                if (ExitNow) { break; }
                Y -= 1;
                if (Y < _Pic.Height * 0.1) { break; }
            } while (true);
            pa.Bottom = Y - _Pic.Height + 1;

            return pa;
        }

        public static Bitmap AdjustGamma(this Bitmap image, float gamma)
        {
            //http://csharphelper.com/blog/2016/12/provide-gamma-correction-for-an-image-in-c/
            // Set the ImageAttributes object's gamma value.
            var attributes = new ImageAttributes();
            attributes.SetGamma(Math.Max(gamma, 0.001f));

            // Draw the image onto the new bitmap
            // while applying the new gamma value.
            Point[] points =
            {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
            var rect =
                new Rectangle(0, 0, image.Width, image.Height);

            // Make the result bitmap.
            var bm = new Bitmap(image.Width, image.Height);
            using (var gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return bm;
        }


        public static Bitmap AdjustBrightness(this Bitmap image, float brightness)
        {
            // http://csharphelper.com/blog/2014/10/use-an-imageattributes-object-to-adjust-an-images-brightness-in-c/
            // Make the ColorMatrix.
            var b = Math.Max(brightness, 0.001f);
            var cm = new ColorMatrix(new float[][]
                {
            new float[] {b, 0, 0, 0, 0},
            new float[] {0, b, 0, 0, 0},
            new float[] {0, 0, b, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1},
                });
            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            // Draw the image onto the new bitmap while applying
            // the new ColorMatrix.
            Point[] points =
            {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
            var rect = new Rectangle(0, 0, image.Width, image.Height);

            // Make the result bitmap.
            var bm = new Bitmap(image.Width, image.Height);
            using (var gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return bm;
        }

        public static Bitmap AdjustContrast(this Bitmap Image, float Value)
        {
            Value = (100.0f + Value) / 100.0f;
            Value *= Value;
            var NewBitmap = Image_Clone(Image);
            var data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
            var Height = NewBitmap.Height;
            var Width = NewBitmap.Width;

            unsafe
            {
                for (var y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    var columnOffset = 0;
                    for (var x = 0; x < Width; ++x)
                    {
                        var B = row[columnOffset];
                        var G = row[columnOffset + 1];
                        var R = row[columnOffset + 2];

                        var Red = R / 255.0f;
                        var Green = G / 255.0f;
                        var Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

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

        public static unsafe Bitmap ReplaceColor(this Bitmap source, Color toReplace, Color replacement)
        {
            // https://stackoverflow.com/questions/17208254/how-to-change-pixel-color-of-an-image-in-c-net
            const int pixelSize = 4; // 32 bits per pixel

            var target = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            BitmapData sourceData = null, targetData = null;

            try
            {
                sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (var y = 0; y < source.Height; ++y)
                {
                    var sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                    var targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                    for (var x = 0; x < source.Width; ++x)
                    {
                        var b = sourceRow[x * pixelSize + 0];
                        var g = sourceRow[x * pixelSize + 1];
                        var r = sourceRow[x * pixelSize + 2];
                        var a = sourceRow[x * pixelSize + 3];

                        if (toReplace.R == r && toReplace.G == g && toReplace.B == b && toReplace.A == a)
                        {
                            r = replacement.R;
                            g = replacement.G;
                            b = replacement.B;
                            a = replacement.A;
                        }

                        targetRow[x * pixelSize + 0] = b;
                        targetRow[x * pixelSize + 1] = g;
                        targetRow[x * pixelSize + 2] = r;
                        targetRow[x * pixelSize + 3] = a;
                    }
                }
            }
            finally
            {
                if (sourceData != null) { source.UnlockBits(sourceData); }
                if (targetData != null) { target.UnlockBits(targetData); }
            }

            return target;
        }


        public static Bitmap Grayscale(this Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);
            var g = Graphics.FromImage(newBitmap);

            var colorMatrix = new ColorMatrix(
               new[]
               {
                   new[] {.3f, .3f, .3f, 0, 0},
                   new[] {.59f, .59f, .59f, 0, 0},
                   new[] {.11f, .11f, .11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
               });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();
            return newBitmap;
        }


        /// <summary>
        /// Helligkeit, Kontrast und Gammawert eines Bitmaps ändern
        /// </summary>
        /// <param name="InBitmap">Bitmap-Objekt</param>
        /// <param name="Brightness">Heligkeit (-1 bis 1) 0 = Normal</param>
        /// <param name="Contrast">Kontrast (-1 bis 1) 0 = Normal</param>
        /// <param name="Gamma">Gammawert (0 bis 2) 1 = Normal</param>
        /// <returns>Bitmap-Objekt</returns>
        public static Bitmap SetBrightnessContrastGamma(this Bitmap InBitmap, float Brightness, float Contrast, float Gamma)
        {

            // Min/Max
            if (Brightness > 1) { Brightness = 1; }
            if (Brightness < -1) { Brightness = -1; }
            if (Contrast > 1) { Contrast = 1; }
            if (Contrast < -1) { Contrast = -1; }

            // Gammawert darf nicht = 0 sein (Bug in GDI+)
            if (Gamma == 0) { Gamma = Convert.ToSingle(Gamma + 1.0E-45); }

            // Zur korrekten Darstellung:
            var Diff = Brightness / 2 - Contrast / 2;

            // ColorMatrix erstellen
            var Matrix = new ColorMatrix(new[]
            {
                new[] {1 + Contrast, 0, 0, 0, 0},
                new[] {0, 1 + Contrast, 0, 0, 0},
                new[] {0, 0, 1 + Contrast, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new[] {Brightness + Diff, Brightness + Diff, Brightness + Diff, 0, 1}
            });


            // Neue Bitmap erstellen
            var NewBmp = new Bitmap(InBitmap.Width, InBitmap.Height, PixelFormat.Format24bppRgb);

            // ImageAttribute-Objekt erstellen
            using (var ImageAttr = new ImageAttributes())
            {

                // ColorMatrix für das ImageAttribute-Objekt setzen
                ImageAttr.SetColorMatrix(Matrix);

                // Gamma für das ImageAttribute-Objekt setzen
                ImageAttr.SetGamma(Gamma);

                // Graphics-Objekt von NewBmp erstellen
                using (var NewBmpGra = Graphics.FromImage(NewBmp))
                {

                    // InBitmap in das Graphics-Objekt zeichnen
                    NewBmpGra.DrawImage(InBitmap, new Rectangle(0, 0, InBitmap.Width, InBitmap.Height), 0, 0, InBitmap.Width, InBitmap.Height, GraphicsUnit.Pixel, ImageAttr);

                    //Graphics-Objekt löschen
                }

                // ImageAttribute-Objekt löschen
            }

            return NewBmp;
        }

        public static void FillCircle(this Bitmap BMP, Color C, int X, int Y, int R)
        {

            for (var adx = -R; adx <= R; adx++)
            {
                for (var ady = -R; ady <= R; ady++)
                {

                    var d = Math.Sqrt(Convert.ToDouble(adx * adx + ady * ady)) - 0.5;

                    var px = X + adx;
                    var py = Y + ady;
                    if (px >= 0 && py >= 0 && px < BMP.Width && py < BMP.Height)
                    {
                        if (d <= R) { BMP.SetPixel(px, py, C); }
                    }



                }
            }
        }


        public static Bitmap ImageBlurFilter(this Bitmap sourceBitmap, BlurType blurType)
        {
            Bitmap resultBitmap = null;


            switch (blurType)
            {
                case BlurType.Mean3x3:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean3x3, 1.0 / 9.0, 0);
                    }
                    break;
                case BlurType.Mean5x5:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean5x5, 1.0 / 25.0, 0);
                    }
                    break;
                case BlurType.Mean7x7:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean7x7, 1.0 / 49.0, 0);
                    }
                    break;
                case BlurType.Mean9x9:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.Mean9x9, 1.0 / 81.0, 0);
                    }
                    break;
                case BlurType.GaussianBlur3x3:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.GaussianBlur3x3, 1.0 / 16.0, 0);
                    }
                    break;
                case BlurType.GaussianBlur5x5:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.GaussianBlur5x5, 1.0 / 159.0, 0);
                    }
                    break;
                case BlurType.MotionBlur5x5:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5, 1.0 / 10.0, 0);
                    }
                    break;
                case BlurType.MotionBlur5x5At45Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5At45Degrees, 1.0 / 5.0, 0);
                    }
                    break;
                case BlurType.MotionBlur5x5At135Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur5x5At135Degrees, 1.0 / 5.0, 0);
                    }
                    break;
                case BlurType.MotionBlur7x7:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7, 1.0 / 14.0, 0);
                    }
                    break;
                case BlurType.MotionBlur7x7At45Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7At45Degrees, 1.0 / 7.0, 0);
                    }
                    break;
                case BlurType.MotionBlur7x7At135Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur7x7At135Degrees, 1.0 / 7.0, 0);
                    }
                    break;
                case BlurType.MotionBlur9x9:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9, 1.0 / 18.0, 0);
                    }
                    break;
                case BlurType.MotionBlur9x9At45Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9At45Degrees, 1.0 / 9.0, 0);
                    }
                    break;
                case BlurType.MotionBlur9x9At135Degrees:
                    {
                        resultBitmap = sourceBitmap.ConvolutionFilter(clsImageMatrix.MotionBlur9x9At135Degrees, 1.0 / 9.0, 0);
                    }
                    break;
                case BlurType.Median3x3:
                    {
                        resultBitmap = sourceBitmap.MedianFilter(3);
                    }
                    break;
                case BlurType.Median5x5:
                    {
                        resultBitmap = sourceBitmap.MedianFilter(5);
                    }
                    break;
                case BlurType.Median7x7:
                    {
                        resultBitmap = sourceBitmap.MedianFilter(7);
                    }
                    break;
                case BlurType.Median9x9:
                    {
                        resultBitmap = sourceBitmap.MedianFilter(9);
                    }
                    break;
                case BlurType.Median11x11:
                    {
                        resultBitmap = sourceBitmap.MedianFilter(11);
                    }
                    break;
            }


            return resultBitmap;
        }



        public static Bitmap MedianFilter(this Bitmap sourceBitmap, int matrixSize)
        {
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
            var calcOffset = 0;


            var byteOffset = 0;


            var neighbourPixels = new List<int>();
            byte[] middlePixel;


            for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;


                    neighbourPixels.Clear();


                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

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


            var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }



        private static Bitmap ConvolutionFilter(this Bitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0)
        {
            var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            var resultBuffer = new byte[sourceData.Stride * sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);


            var blue = 0.0;
            var green = 0.0;
            var red = 0.0;


            var filterWidth = filterMatrix.GetLength(1);
            var filterHeight = filterMatrix.GetLength(0);


            var filterOffset = (filterWidth - 1) / 2;
            var calcOffset = 0;


            var byteOffset = 0;


            for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;


                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;


                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);


                            blue += (double)pixelBuffer[calcOffset] * filterMatrix[filterY + filterOffset, filterX + filterOffset];


                            green += (double)pixelBuffer[calcOffset + 1] * filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            red += (double)pixelBuffer[calcOffset + 2] * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }


                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;


                    blue = blue > 255 ? 255 : (blue < 0 ? 0 : blue);


                    green = green > 255 ? 255 : (green < 0 ? 0 : green);


                    red = red > 255 ? 255 : (red < 0 ? 0 : red);


                    resultBuffer[byteOffset] = (byte)blue;
                    resultBuffer[byteOffset + 1] = (byte)green;
                    resultBuffer[byteOffset + 2] = (byte)red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
    }
}