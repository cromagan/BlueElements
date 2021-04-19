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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static BlueBasics.FileOperations;

namespace BlueBasics {
    // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f

    // Todo: Obselete Routinen:
    // Image_FromFile
    // Resize

    public class BitmapExt {
        public Bitmap Bitmap { get; private set; }
        public int[] Bits { get; private set; } // Int32 = int
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public PixelFormat _pixelformat = PixelFormat.Format32bppArgb;
        // public PixelFormat _pixelformat = PixelFormat.Format32bppPArgb;

        public BitmapExt(string filename) : this((Bitmap)Image_FromFile(filename)) { }

        public BitmapExt(string filename, bool setDummyPicIfFails) {
            FromFile(filename, setDummyPicIfFails);
        }

        public BitmapExt(int width, int height) {
            EmptyBitmap(width, height);
        }

        public BitmapExt(Icon icon) : this(icon.ToBitmap()) { }

        /// <summary>
        /// Achtung, das eingehende Bild wird zerstört!
        /// </summary>
        /// <param name="bmp"></param>
        public BitmapExt(Bitmap bmp) {
            SetBitmap(bmp);
        }

        /// <summary>
        /// Achtung, das eingehende Bild wird zerstört!
        /// </summary>
        /// <param name="bmp"></param>
        private void SetBitmap(Bitmap bmp) {
            if (bmp == null) {
                Width = -1;
                Height = -1;
                return;
            }

            try {
                Width = bmp.Width;
                Height = bmp.Height;
                Bits = new int[Width * Height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * 4, _pixelformat, BitsHandle.AddrOfPinnedObject());

                using (var gr = Graphics.FromImage(Bitmap)) {
                    gr.DrawImage(bmp, new Rectangle(0, 0, Width, Height));
                }

                bmp.Dispose(); // Sichherheithalber, da es ja nun ein neues Bild ist.
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
                Width = -1;
                Height = -1;
                Bitmap = null;
            }
        }

        public void SetPixel(int x, int y, Color colour) {
            Bits[x + (y * Width)] = colour.ToArgb();
        }

        public Color GetPixel(int x, int y) {
            return Color.FromArgb(Bits[x + (y * Width)]);
        }

        public void Dispose() {
            if (Disposed) { return; }
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }

        public void FromFile(string dateiName, bool setDummyPicIfFails) {
            var x = (Bitmap)Image_FromFile(dateiName);

            if (x == null && setDummyPicIfFails) {
                x = QuickImage.Get(enImageCode.Warnung).BMP;
            }

            SetBitmap(x);
        }

        /// <summary>
        /// Diese Routine ist genau so schnell wie Image.fromFile, setzt aber KEINEN Datei-Lock.
        /// </summary>
        /// <param name="dateiName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Image Image_FromFile(string dateiName) {
            if (string.IsNullOrEmpty(dateiName)) { return null; }
            if (!FileExists(dateiName)) { return null; }

            try {
                var fs = new FileStream(dateiName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var IM = Image.FromStream(fs);
                fs.Close();
                fs.Dispose();
                return IM;
            } catch (Exception) {
                return null;
            }
        }

        public static Bitmap Resize(Bitmap bmp, int width, int height, enSizeModes sizeMode, InterpolationMode interpolationMode, bool collectGarbage) {
            if (bmp == null) { return null; }
            if (width < 1 && height < 1) { return null; }

            if (collectGarbage) { modAllgemein.CollectGarbage(); }

            if (width < 1) { width = 1; }
            if (height < 1) { height = 1; }

            var Scale = Math.Min(width / (double)bmp.Width, height / (double)bmp.Height);

            switch (sizeMode) {
                case enSizeModes.EmptySpace:
                    break;

                case enSizeModes.BildAbschneiden:
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    width = (int)(Scale * bmp.Width);
                    height = (int)(Scale * bmp.Height);
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    if (Scale >= 1) { return bmp; }
                    width = (int)(Scale * bmp.Width);
                    height = (int)(Scale * bmp.Height);
                    break;

                case enSizeModes.Verzerren:
                    Scale = 1; // Dummy setzen
                    break;

                default:
                    Develop.DebugPrint(sizeMode);
                    return null;
            }

            var nw = (int)(bmp.Width * Scale);
            var nh = (int)(bmp.Height * Scale);

            if (sizeMode == enSizeModes.Verzerren) {
                nw = width;
                nh = height;
            }

            try {
                var ImageResize = new Bitmap(width, height); // Kein Format32bppPArgb --> Fehler
                using (var GR = Graphics.FromImage(ImageResize)) {
                    GR.InterpolationMode = interpolationMode;
                    GR.PixelOffsetMode = PixelOffsetMode.Half;

                    // 20000 / 4 = 5000, also noch 1000 zum kleiner machen
                    if (bmp.Width > 20000 && nw < 4000) {
                        var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 4.0), (int)(bmp.Height / 4.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                    } else if (bmp.Width > 15000 && nw < 4000) {
                        var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 3.0), (int)(bmp.Height / 3.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                    } else if (bmp.Width > 10000 && nw < 2500) {
                        var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 3.0), (int)(bmp.Height / 3.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                    } else if (bmp.Width > 8000 && nw < 2000) {
                        var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 2.5), (int)(bmp.Height / 2.5), null, IntPtr.Zero);
                        GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                    } else {
                        GR.DrawImage(bmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                    }
                }

                return ImageResize;
            } catch {
                if (!collectGarbage) { modAllgemein.CollectGarbage(); }
                if (sizeMode == enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern) {
                    return (Bitmap)bmp.GetThumbnailImage(nw, nh, null, IntPtr.Zero);
                }

                return null;
            }
        }

        public BitmapExt Crop(Rectangle re) {
            var newBMP = new BitmapExt(re.Width, re.Height);

            using (var GR = Graphics.FromImage(newBMP.Bitmap)) {
                GR.Clear(Color.Transparent);
                GR.PixelOffsetMode = PixelOffsetMode.Half;
                GR.DrawImage(Bitmap, new Rectangle(0, 0, re.Width, re.Height), re.Left, re.Top, re.Width, re.Height, GraphicsUnit.Pixel);
            }

            return newBMP;
        }

        public void Resize(int width, int height, enSizeModes sizeMode, InterpolationMode interpolationMode, bool collectGarbage) {
            if (Bitmap == null) { return; }

            if (collectGarbage) { modAllgemein.CollectGarbage(); }

            if (width < 1) { width = 1; }
            if (height < 1) { height = 1; }

            var Scale = Math.Min(width / (double)Width, height / (double)Height);

            switch (sizeMode) {
                case enSizeModes.EmptySpace:
                    break;

                case enSizeModes.BildAbschneiden:
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    width = (int)(Scale * Width);
                    height = (int)(Scale * Height);
                    break;

                case enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    if (Scale >= 1) { return; }
                    width = (int)(Scale * Width);
                    height = (int)(Scale * Height);
                    break;

                case enSizeModes.Verzerren:
                    Scale = 1; // Dummy setzen
                    break;

                default:
                    Develop.DebugPrint(sizeMode);
                    return;
            }

            var nw = (int)(Width * Scale);
            var nh = (int)(Height * Scale);

            if (sizeMode == enSizeModes.Verzerren) {
                nw = width;
                nh = height;
            }

            try {
                var oldBMP = Bitmap;

                EmptyBitmap(width, height);

                using var GR = Graphics.FromImage(Bitmap);
                GR.InterpolationMode = interpolationMode;
                GR.PixelOffsetMode = PixelOffsetMode.Half;

                // 20000 / 4 = 5000, also noch 1000 zum kleiner machen
                if (Width > 20000 && nw < 4000) {
                    var tmp = (Bitmap)oldBMP.GetThumbnailImage((int)(oldBMP.Width / 4.0), (int)(oldBMP.Height / 4.0), null, IntPtr.Zero);
                    GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                } else if (oldBMP.Width > 15000 && nw < 4000) {
                    var tmp = (Bitmap)oldBMP.GetThumbnailImage((int)(oldBMP.Width / 3.0), (int)(oldBMP.Height / 3.0), null, IntPtr.Zero);
                    GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                } else if (oldBMP.Width > 10000 && nw < 2500) {
                    var tmp = (Bitmap)oldBMP.GetThumbnailImage((int)(oldBMP.Width / 3.0), (int)(oldBMP.Height / 3.0), null, IntPtr.Zero);
                    GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                } else if (oldBMP.Width > 8000 && nw < 2000) {
                    var tmp = (Bitmap)oldBMP.GetThumbnailImage((int)(oldBMP.Width / 2.5), (int)(oldBMP.Height / 2.5), null, IntPtr.Zero);
                    GR.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                } else {
                    GR.DrawImage(oldBMP, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);

                // if (!collectGarbage) { modAllgemein.CollectGarbage(); }
                // if (sizeMode == enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern)
                // {
                //    return (Bitmap)bmp.GetThumbnailImage(nw, nh, null, IntPtr.Zero);
                // }
                // return null;
            }
        }

        private void EmptyBitmap(int width, int height) {
            Width = width;
            Height = height;
            Bits = new int[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, _pixelformat, BitsHandle.AddrOfPinnedObject());
        }

        public void MakeTransparent(Color color) {
            Bitmap.MakeTransparent(color);
        }

        public void Save(string name, ImageFormat imageFormat) {
            Bitmap.Save(name, imageFormat);
        }
    }
}