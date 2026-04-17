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

internal class ImageFilter_WindowsMEDisabled : ImageFilter {

    #region Properties

    public static ImageFilter_WindowsMEDisabled Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
    }

    public override void ProcessFilter(Bitmap image) {
        if (Parameter is not Bitmap originalImage) { return; }

        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try {
            var bits = new byte[bitmapData.Stride * image.Height];
            Marshal.Copy(bitmapData.Scan0, bits, 0, bits.Length);

            var oriRect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
            var oriData = originalImage.LockBits(oriRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try {
                var oriBits = new byte[oriData.Stride * originalImage.Height];
                Marshal.Copy(oriData.Scan0, oriBits, 0, oriBits.Length);

                var w = bitmapData.Width;
                var h = bitmapData.Height;

                for (var x = 0; x < w; x++) {
                    for (var y = 0; y < h; y++) {
                        var idx = bitmapData.GetPixelIndex(x, y);

                        if (bits.IsMagentaOrTransparent(idx)) {
                            bits.SetArgb(idx, 0, 0, 0, 0);
                            continue;
                        }

                        var randPixel = x > 0 && oriData.IsMagentaOrTransparentAt(oriBits, x - 1, y) ||
                                         y > 0 && oriData.IsMagentaOrTransparentAt(oriBits, x, y - 1) ||
                                         x < w - 1 && oriData.IsMagentaOrTransparentAt(oriBits, x + 1, y) ||
                                         y < h - 1 && oriData.IsMagentaOrTransparentAt(oriBits, x, y + 1);

                        if (bits[idx] < 128 || randPixel) {
                            var c1 = SystemColors.ControlDark;
                            if (x < w - 1 && y < h - 1 && oriData.IsMagentaOrTransparentAt(oriBits, x + 1, y + 1)) {
                                c1 = SystemColors.ControlLightLight;
                            }
                            bits.SetColor(idx, c1);
                        } else {
                            bits.SetArgb(idx, 0, 0, 0, 0);
                        }
                    }
                }

                Marshal.Copy(bits, 0, bitmapData.Scan0, bits.Length);
            } finally {
                originalImage.UnlockBits(oriData);
            }
        } finally {
            image.UnlockBits(bitmapData);
        }
    }

    #endregion
}
