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

using System.Drawing.Imaging;
using static BlueBasics.Extensions;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_Intensify : ImageFilter {

    #region Properties

    public static ImageFilter_Intensify Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        for (var y = 0; y < bitmapData.Height; y++) {
            for (var x = 0; x < bitmapData.Width; x++) {
                var index = bitmapData.GetPixelIndex(x, y);

                if (bits[index + 3] > 127 && bits.GetBrightness(index) < 0.9) {
                    bits[index] = 0;
                    bits[index + 1] = 0;
                    bits[index + 2] = 0;
                }
            }
        }
    }

    #endregion
}
