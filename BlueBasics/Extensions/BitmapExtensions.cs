#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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


    }
}