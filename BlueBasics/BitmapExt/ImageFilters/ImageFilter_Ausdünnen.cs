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

using System.Drawing;

namespace BlueBasics;

internal class ImageFilter_Ausdünnen : ImageFilter {

    #region Properties

    public override string KeyName => "Ausdünnen";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        // Stärke des Ausdünnens basierend auf dem Faktor
        var staerke = (int)factor;

        // Schleife über alle Pixel im Array
        for (var x = 0; x < width - 1; x++) {
            for (var y = 0; y < height - 1; y++) {
                if (!IsWhite(x, y, width, height, bits)) {
                    for (var wi = staerke; wi > 0; wi--) {
                        var ma1 = wi / 2;
                        var ma2 = wi - ma1;

                        // X-Richtung
                        if (IsWhite(x - ma1 - 1, y, width, height, bits) && IsWhite(x + ma2 + 1, y, width, height, bits)) {
                            var allBlack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (!IsWhite(x + ch, y, width, height, bits)) {
                                    allBlack = false;
                                    break;
                                }
                            }
                            if (allBlack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) {
                                        bits[(y * width) + x + ch] = Color.White.ToArgb();
                                    }
                                }
                            }
                        }

                        // Y-Richtung
                        if (IsWhite(x, y - ma1 - 1, width, height, bits) && IsWhite(x, y + ma2 + 1, width, height, bits)) {
                            var allBlack = true;
                            for (var ch = -ma1; ch <= ma2; ch++) {
                                if (!IsWhite(x, y + ch, width, height, bits)) {
                                    allBlack = false;
                                    break;
                                }
                            }
                            if (allBlack) {
                                for (var ch = -ma1; ch <= ma2; ch++) {
                                    if (ch != 0) {
                                        bits[((y + ch) * width) + x] = Color.White.ToArgb();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Hilfsmethode zur Überprüfung, ob ein Pixel nahe Weiß ist
    private bool IsWhite(int x, int y, int width, int height, int[] bits) {
        if (x < 0 || y < 0 || x >= width || y >= height) {
            return true;
        }
        var argb = bits[(y * width) + x];
        var color = Color.FromArgb(argb);
        var brightness = (color.R + color.G + color.B) / (3.0 * 255.0);
        return brightness >= 0.9;
    }

    #endregion
}