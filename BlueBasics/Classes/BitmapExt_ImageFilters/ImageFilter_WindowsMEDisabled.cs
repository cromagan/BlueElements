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
using static BlueBasics.Extensions;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_WindowsMEDisabled : ImageFilter {

    #region Properties

    public override string KeyName => "WindowsMEDisabled";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not (BitmapData oriData, byte[] oriBits)) { return; }

        var w = bitmapData.Width;
        var h = bitmapData.Height;

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                var idx = y * bitmapData.Stride + x * 4;
                var c = Color.FromArgb(bits[idx + 3], bits[idx + 2], bits[idx + 1], bits[idx]);

                var c1 = Color.FromArgb(0, 0, 0, 0);
                if (!c.IsMagentaOrTransparent()) {
                    var randPixel = x > 0 && GetPixel(oriData, oriBits, x - 1, y).IsMagentaOrTransparent() ||
                                     y > 0 && GetPixel(oriData, oriBits, x, y - 1).IsMagentaOrTransparent() ||
                                     x < w - 1 && GetPixel(oriData, oriBits, x + 1, y).IsMagentaOrTransparent() ||
                                     y < h - 1 && GetPixel(oriData, oriBits, x, y + 1).IsMagentaOrTransparent();

                    if (c.B < 128 || randPixel) {
                        c1 = SystemColors.ControlDark;
                        if (x < w - 1 && y < h - 1 && GetPixel(oriData, oriBits, x + 1, y + 1).IsMagentaOrTransparent()) {
                            c1 = SystemColors.ControlLightLight;
                        }
                    }
                }
                bits[idx] = c1.B;
                bits[idx + 1] = c1.G;
                bits[idx + 2] = c1.R;
                bits[idx + 3] = c1.A;
            }
        }
    }

    #endregion
}
