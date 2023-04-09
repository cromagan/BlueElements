// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueBasics.Enums;
using System.Drawing.Drawing2D;
using System.Drawing;
using System;
using System.IO;
using static BlueBasics.IO;
using System.Reflection;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Bitmap? CloneFromBitmap(this Bitmap? bmp) {
        if (bmp == null) { return null; }

        var tim = DateTime.Now;

        do {
            try { // wird an anderer stelle verwendet
                var bitmap = new Bitmap(bmp.Width, bmp.Height);
                using var gr = Graphics.FromImage(bitmap);
                gr.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                return bitmap;
            } catch (Exception ex) {
                if (DateTime.Now.Subtract(tim).TotalSeconds > 5) {
                    Develop.DebugPrint("Bild konnte nicht geklont werden", ex);
                    return null;
                }
            }
        } while (true);
    }

    public static Bitmap? GetEmmbedBitmap(Assembly assembly, string name) {
        if (name.Contains("|")) { return null; }
        if (name.Contains("[")) { return null; }
        using var d = Generic.GetEmmbedResource(assembly, name);
        if (d == null) { return null; }

        switch (name.FileType()) {
            case FileFormat.Image:
                return new Bitmap(d);

            //case FileFormat.Icon:
            //    return new Icon(d);

            default:
                Develop.DebugPrint(name.FileType());
                return null;
        }
    }

    /// <summary>
    /// Diese Routine ist genau so schnell wie Image.fromFile, setzt aber KEINEN Datei-Lock.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static Image? Image_FromFile(string filename) {
        if (string.IsNullOrEmpty(filename)) { return null; }
        if (!FileExists(filename)) { return null; }
        try {
            FileStream fs = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var im = Image.FromStream(fs);
            fs.Close();
            fs.Dispose();
            return im;
        } catch (Exception) {
            return null;
        }
    }

    public static Bitmap? Resize(this Bitmap? bmp, int width, int height, SizeModes sizeMode, InterpolationMode interpolationMode, bool collectGarbage) {
        if (bmp == null) { return null; }
        if (width < 1 && height < 1) { return null; }
        if (collectGarbage) { Generic.CollectGarbage(); }
        if (width < 1) { width = 1; }
        if (height < 1) { height = 1; }
        var scale = Math.Min(width / (double)bmp.Width, height / (double)bmp.Height);

        switch (sizeMode) {
            case SizeModes.EmptySpace:
                break;

            case SizeModes.BildAbschneiden:
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                width = (int)(scale * bmp.Width);
                height = (int)(scale * bmp.Height);
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                // Bei diesem Modus werden die Rückgabehöhe oder breite verändert!!!
                if (scale >= 1) { return bmp; }
                width = (int)(scale * bmp.Width);
                height = (int)(scale * bmp.Height);
                break;

            case SizeModes.Verzerren:
                scale = 1; // Dummy setzen
                break;

            default:
                Develop.DebugPrint(sizeMode);
                return null;
        }
        var nw = (int)(bmp.Width * scale);
        var nh = (int)(bmp.Height * scale);
        if (sizeMode == SizeModes.Verzerren) {
            nw = width;
            nh = height;
        }

        try {
            Bitmap imageResize = new(width, height); // Kein Format32bppPArgb --> Fehler
            using var gr = Graphics.FromImage(imageResize);
            gr.InterpolationMode = interpolationMode;
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            // 20000 / 4 = 5000, also noch 1000 zum kleiner machen
            if (bmp.Width > 20000 && nw < 4000) {
                var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 4.0), (int)(bmp.Height / 4.0), null, IntPtr.Zero);
                gr.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
            } else if (bmp.Width > 15000 && nw < 4000) {
                var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 3.0), (int)(bmp.Height / 3.0), null, IntPtr.Zero);
                gr.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
            } else if (bmp.Width > 10000 && nw < 2500) {
                var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 3.0), (int)(bmp.Height / 3.0), null, IntPtr.Zero);
                gr.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
            } else if (bmp.Width > 8000 && nw < 2000) {
                var tmp = (Bitmap)bmp.GetThumbnailImage((int)(bmp.Width / 2.5), (int)(bmp.Height / 2.5), null, IntPtr.Zero);
                gr.DrawImage(tmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
            } else {
                gr.DrawImage(bmp, (int)((width - nw) / 2.0), (int)((height - nh) / 2.0), nw, nh);
            }

            return imageResize;
        } catch {
            if (!collectGarbage) { Generic.CollectGarbage(); }
            return sizeMode == SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern
                ? (Bitmap)bmp.GetThumbnailImage(nw, nh, null, IntPtr.Zero)
                : null;
        }
    }

    #endregion
}