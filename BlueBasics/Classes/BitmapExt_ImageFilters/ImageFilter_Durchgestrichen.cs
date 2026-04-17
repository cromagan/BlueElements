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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_Durchgestrichen : ImageFilter {

    #region Properties

    public static ImageFilter_Durchgestrichen Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
    }

    public override void ProcessFilter(Bitmap image) {
        var oriW = image.Width;
        var oriH = image.Height;
        var n = "Kreuz|" + oriW + "|";
        if (oriW != oriH) { n += oriH; }

        using var bmpKreuz = ((Bitmap)QuickImage.Get(n.TrimEnd('|'))).CloneFromBitmap();
        var kreuzW = bmpKreuz.Width;
        var kreuzH = bmpKreuz.Height;
        var lockArea = new Rectangle(0, 0, kreuzW, kreuzH);
        var kreuzData = bmpKreuz.LockBits(lockArea, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var kreuzBits = new byte[kreuzData.Stride * kreuzH];
        Marshal.Copy(kreuzData.Scan0, kreuzBits, 0, kreuzBits.Length);
        try {
            var rect = new Rectangle(0, 0, oriW, oriH);
            var data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var bits = new byte[data.Stride * oriH];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);
            try {
                for (var x = 0; x < oriW; x++) {
                    for (var y = 0; y < oriH; y++) {
                        var idx = data.GetPixelIndex(x, y);

                        if (bits.IsMagentaOrTransparent(idx)) {
                            var kidx = kreuzData.GetPixelSafeIndex(x, y, kreuzW, kreuzH);
                            if (kidx >= 0) { bits.SetArgb(idx, kreuzBits[kidx + 3], kreuzBits[kidx + 2], kreuzBits[kidx + 1], kreuzBits[kidx]); }
                        } else {
                            var kidx = kreuzData.GetPixelSafeIndex(x, y, kreuzW, kreuzH);
                            if (kidx >= 0 && kreuzBits[kidx + 3] > 0) {
                                var c = bits.GetColor(idx).MixColor(Color.FromArgb(kreuzBits[kidx + 3], kreuzBits[kidx + 2], kreuzBits[kidx + 1], kreuzBits[kidx]), 0.5);
                                bits.SetColor(idx, c);
                            }
                        }
                    }
                }
                Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            } finally {
                image.UnlockBits(data);
            }
        } finally {
            bmpKreuz.UnlockBits(kreuzData);
        }
    }

    #endregion
}
