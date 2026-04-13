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
using System.Drawing.Imaging;
using static BlueBasics.Extensions;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_Ausdünnen : ImageFilter {

    #region Properties

    public static ImageFilter_Ausdünnen Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not int staerke) { return; }
        if (bits.Length == 0 || bitmapData.Width <= 0 || bitmapData.Height <= 0 || staerke <= 0) { return; }

        for (var x = 0; x < bitmapData.Width - 1; x++) {
            for (var y = 0; y < bitmapData.Height - 1; y++) {
                if (!IsWhite(x, y)) {
                    for (var wi = staerke; wi > 0; wi--) {
                        var ma1 = (int)Math.Floor((float)wi / 2);
                        var ma2 = wi - ma1;
                        // X
                        if (IsWhite(x - ma1 - 1, y) && IsWhite(x + ma2 + 1, y)) {
                            var allblack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (IsWhite(x + ch, y)) { allblack = false; break; }
                            }
                            if (allblack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) { bitmapData.SetPixelArgb(bits, x + ch, y, 255, 255, 255, 255); }
                                }
                            }
                        }
                        // Y
                        if (IsWhite(x, y - ma1 - 1) && IsWhite(x, y + ma2 + 1)) {
                            var allblack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (IsWhite(x, y + ch)) { allblack = false; break; }
                            }
                            if (allblack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) { bitmapData.SetPixelArgb(bits, x, y + ch, 255, 255, 255, 255); }
                                }
                            }
                        }
                    }
                }
            }
        }

        bool IsWhite(int x, int y) => bitmapData.IsNearWhiteAt(bits, x, y, 0.9);
    }

    #endregion
}
