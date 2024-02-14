// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

namespace BlueBasics;

internal class ImageFilter_Ausdünnen : ImageFilter {

    #region Properties

    public override string KeyName => "Ausdünnen";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Überprüfen der Eingabeparameter
        if (bits == null || bits.Length == 0 || bitmapData.Width <= 0 || bitmapData.Height <= 0 || factor <= 0) { return; }

        var staerke = (int)factor;

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
                                    if (ch != 0) { BitmapExt.SetPixel(bitmapData, bits, x + ch, y, Color.White); }
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
                                    if (ch != 0) { BitmapExt.SetPixel(bitmapData, bits, x, y + ch, Color.White); }
                                }
                            }
                        }
                    }
                }
            }
        }

        bool IsWhite(int x, int y) => x < 0 || y < 0 || x >= bitmapData.Width || y >= bitmapData.Height || BitmapExt.GetPixel(bitmapData, bits, x, y).IsNearWhite(0.9);
    }

    #endregion
}