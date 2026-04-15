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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_Zweitsymbol : ImageFilter {

    #region Properties

    public static ImageFilter_Zweitsymbol Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
    }

    public override void ProcessFilter(Bitmap image) {
        if (Parameter is not string zweitsymbol || string.IsNullOrEmpty(zweitsymbol)) { return; }

        var oriW = image.Width;
        var oriH = image.Height;
        var siz = Math.Max(oriW / 3, oriH / 3);
        siz = Math.Max(siz, 10);
        siz = Math.Min(Math.Min(siz, oriW), oriH);

        using var bmpSecond = ((Bitmap)QuickImage.Get(zweitsymbol + "|" + siz)).CloneFromBitmap();
        var secondW = bmpSecond.Width;
        var secondH = bmpSecond.Height;
        var lockArea = new Rectangle(0, 0, secondW, secondH);
        var secondData = bmpSecond.LockBits(lockArea, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var secondBits = new byte[secondData.Stride * secondH];
        Marshal.Copy(secondData.Scan0, secondBits, 0, secondBits.Length);
        try {
            var rect = new Rectangle(0, 0, oriW, oriH);
            var data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var bits = new byte[data.Stride * oriH];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);
            try {
                for (var x = 0; x < oriW; x++) {
                    for (var y = 0; y < oriH; y++) {
                        var idx = data.GetPixelIndex(x, y);

                        var secx = x - (oriW - secondW);
                        var secy = y - (oriH - secondH);

                        var si = secondData.GetPixelSafeIndex(secx, secy, secondW, secondH);

                        if (si >= 0 && !secondBits.IsMagentaOrTransparent(si)) {
                            bits[idx] = secondBits[si];
                            bits[idx + 1] = secondBits[si + 1];
                            bits[idx + 2] = secondBits[si + 2];
                            bits[idx + 3] = secondBits[si + 3];
                        } else {
                            if (secondData.GetPixelSafeIndex(secx + 1, secy + 1, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx + 1, secy + 1, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            } else if (secondData.GetPixelSafeIndex(secx + 1, secy, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx + 1, secy, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            } else if (secondData.GetPixelSafeIndex(secx, secy + 1, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx, secy + 1, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            } else if (secondData.GetPixelSafeIndex(secx - 1, secy - 1, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx - 1, secy - 1, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            } else if (secondData.GetPixelSafeIndex(secx - 1, secy, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx - 1, secy, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            } else if (secondData.GetPixelSafeIndex(secx, secy - 1, secondW, secondH) >= 0 && secondBits[secondData.GetPixelSafeIndex(secx, secy - 1, secondW, secondH) + 3] > 128) {
                                bits[idx + 3] = 0;
                            }
                        }
                    }
                }
                Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            } finally {
                image.UnlockBits(data);
            }
        } finally {
            bmpSecond.UnlockBits(secondData);
        }
    }

    #endregion
}
