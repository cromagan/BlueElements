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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Bitmap CloneFromBitmap(this Bitmap sourceBmp) {
        try { // wird an anderer stelle verwendet
            var bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height);
            using var gr = Graphics.FromImage(bmp);
            gr.DrawImage(sourceBmp, new Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height));
            return bmp;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return sourceBmp.CloneFromBitmap();
        }
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

    public static Image? Image_FromFile(string filename) {
        if (string.IsNullOrEmpty(filename)) { return null; }
        if (!FileExists(filename)) { return null; }
        try {
            var bytes = ReadAllBytes(filename, 3).Value as byte[] ?? [];
            using var ms = new System.IO.MemoryStream(bytes);
            var im = Image.FromStream(ms);
            return im;
        } catch {
            return null;
        }
    }

    public static bool IsValid(this Bitmap? bitmap) {
        if (bitmap == null) { return false; }

        try {
            // Versuche auf einige Eigenschaften des Bitmaps zuzugreifen,
            // um sicherzustellen, dass das Bitmap-Objekt gültig ist.
            _ = bitmap.Width;
            _ = bitmap.Height;
            return true;
        } catch {
            // Wenn eine Ausnahme auftritt, ist das Bitmap nicht gültig.
            return false;
        }
    }

    public static Bitmap Resize(this Bitmap sourceBmp, int maxwidth, int maxheight, SizeModes sizeMode, InterpolationMode interpolationMode, bool collectGarbage) {
        if (collectGarbage) { Generic.CollectGarbage(); }
        if (maxwidth < 1) { maxwidth = 1; }
        if (maxheight < 1) { maxheight = 1; }

        int calcwidth;
        int calcheight;

        switch (sizeMode) {
            case SizeModes.EmptySpace:
            case SizeModes.BildAbschneiden:
                var scale3 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                calcwidth = (int)(scale3 * sourceBmp.Width);
                calcheight = (int)(scale3 * sourceBmp.Height);
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern:
                var scale1 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                maxwidth = (int)(scale1 * sourceBmp.Width);
                maxheight = (int)(scale1 * sourceBmp.Height);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            case SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern:
                var scale2 = Math.Min(maxwidth / (double)sourceBmp.Width, maxheight / (double)sourceBmp.Height);
                if (scale2 >= 1) { scale2 = 1; }
                maxwidth = (int)(scale2 * sourceBmp.Width);
                maxheight = (int)(scale2 * sourceBmp.Height);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            case SizeModes.Verzerren:
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;

            default:
                Develop.DebugPrint(sizeMode);
                calcwidth = maxwidth;
                calcheight = maxheight;
                break;
        }

        // Um abstürzte zu vermeiden, einen Faktor berechnen
        Bitmap tmp;

        switch (sourceBmp.Width) {
            case > 20000 when calcwidth < 4000:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 4.0), (int)(sourceBmp.Height / 4.0), null, IntPtr.Zero);
                break;

            case > 15000 when calcwidth < 4000:
            case > 10000 when calcwidth < 2500:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 3.0), (int)(sourceBmp.Height / 3.0), null, IntPtr.Zero);
                break;

            case > 8000 when calcwidth < 2000:
                tmp = (Bitmap)sourceBmp.GetThumbnailImage((int)(sourceBmp.Width / 2.5), (int)(sourceBmp.Height / 2.5), null, IntPtr.Zero);
                break;

            default:
                tmp = sourceBmp;
                break;
        }

        try {
            var bmp = new Bitmap(maxwidth, maxheight); // Kein Format32bppPArgb --> Fehler
            using var gr = Graphics.FromImage(bmp);
            gr.InterpolationMode = interpolationMode;
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            // 20000 / 4 = 5000, also noch 1000 zum kleiner machen

            gr.DrawImage(tmp, (int)((maxwidth - calcwidth) / 2.0), (int)((maxheight - calcheight) / 2.0), calcwidth, calcheight);
            return bmp;
        } catch { }

        try {
            if (!collectGarbage) { Generic.CollectGarbage(); }

            if (sizeMode == SizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern) {
                return (Bitmap)sourceBmp.GetThumbnailImage(calcwidth, calcheight, null, IntPtr.Zero);
            }
        } catch { }

        Develop.AbortAppIfStackOverflow();
        // Mit den modifizierten Werten nochmal probieren...
        return tmp.Resize(maxheight, maxheight, sizeMode, interpolationMode, true);
    }

    #endregion
}