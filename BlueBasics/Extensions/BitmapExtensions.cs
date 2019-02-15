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

            double Scale = 0;


            switch (_SizeMode)
            {
                case enSizeModes.WeißerRand:
                case enSizeModes.QuickPicGeneration:
                    Scale = Math.Min(_Width / (double)_BMPx.Width, _Height / (double)_BMPx.Height);
                    break;
                case enSizeModes.BildAbschneiden:
                    Scale = Math.Max(_Width / (double)_BMPx.Width, _Height / (double)_BMPx.Height);
                    break;
                case enSizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    Scale = Math.Min(_Width / (double)_BMPx.Width, _Height / (double)_BMPx.Height);
                    _Width = Convert.ToInt32(Scale * _BMPx.Width);
                    _Height = Convert.ToInt32(Scale * _BMPx.Height);
                    break;
                case enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                {
                    // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                    Scale = Math.Min(_Width / (double)_BMPx.Width, _Height / (double)_BMPx.Height);
                    if (Scale >= 1)
                    {
                        return _BMPx;
                    }
                    _Width = Convert.ToInt32(Scale * _BMPx.Width);
                    _Height = Convert.ToInt32(Scale * _BMPx.Height);
                    break;
                }
                case enSizeModes.Verzerren:
                    Scale = 1; // Dummy setzen
                    break;
                default:
                    Develop.DebugPrint(_SizeMode);
                    return null;
            }

            var nw = Convert.ToInt32(_BMPx.Width * Scale);
            var nh = Convert.ToInt32(_BMPx.Height * Scale);

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
                    if (_SizeMode != enSizeModes.QuickPicGeneration)
                    {
                        GR.Clear(Color.White);
                    }
                    GR.InterpolationMode = InterpolationMode;
                    GR.PixelOffsetMode = PixelOffsetMode.Half;


                    // 20000 / 4 = 5000, also noch 1000 zum kleiner machen
                    if (_BMPx.Width > 20000 && nw < 4000)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage(Convert.ToInt32(_BMPx.Width / 4.0), Convert.ToInt32(_BMPx.Height / 4.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, Convert.ToInt32((_Width - nw) / 2.0), Convert.ToInt32((_Height - nh) / 2.0), nw, nh);
                    }
                    else if (_BMPx.Width > 10000 && nw < 4000)
                    {
                        var tmp = (Bitmap)_BMPx.GetThumbnailImage(Convert.ToInt32(_BMPx.Width / 2.0), Convert.ToInt32(_BMPx.Height / 2.0), null, IntPtr.Zero);
                        GR.DrawImage(tmp, Convert.ToInt32((_Width - nw) / 2.0), Convert.ToInt32((_Height - nh) / 2.0), nw, nh);
                    }
                    else
                    {
                        GR.DrawImage(_BMPx, Convert.ToInt32((_Width - nw) / 2.0), Convert.ToInt32((_Height - nh) / 2.0), nw, nh);
                    }

                }
                return ImageResize;

            }
            catch
            {
                if (_SizeMode == enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern)
                {
                    return (Bitmap)_BMPx.GetThumbnailImage(nw, nh, null, IntPtr.Zero);
                }
                return null;
            }
        }


    }
}