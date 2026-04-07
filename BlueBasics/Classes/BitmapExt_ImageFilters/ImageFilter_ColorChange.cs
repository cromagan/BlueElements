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

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_ColorChange : ImageFilter {

    #region Properties

    public static ImageFilter_ColorChange Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not (Color toReplace, Color replacement)) { return; }

        for (var i = 0; i < bits.Length; i += 4) {
            if (toReplace.B == bits[i] && toReplace.G == bits[i + 1] && toReplace.R == bits[i + 2] && toReplace.A == bits[i + 3]) {
                bits[i] = replacement.B;
                bits[i + 1] = replacement.G;
                bits[i + 2] = replacement.R;
                bits[i + 3] = replacement.A;
            }
        }
    }

    #endregion
}