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

internal class ImageFilter_Sättigung : ImageFilter {

    #region Properties

    public static ImageFilter_Sättigung Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }

        for (var i = 0; i < bits.Length; i += 4) {
            var c = Color.FromArgb(bits[i + 3], bits[i + 2], bits[i + 1], bits[i]);
            if (c.IsMagentaOrTransparent()) { continue; }
            c = c.GetHue().FromHsb(c.GetSaturation() * factor, c.GetBrightness(), c.A);
            bits[i] = c.B;
            bits[i + 1] = c.G;
            bits[i + 2] = c.R;
        }
    }

    #endregion
}