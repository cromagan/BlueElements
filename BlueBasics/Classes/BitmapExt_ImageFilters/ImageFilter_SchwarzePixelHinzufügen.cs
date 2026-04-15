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

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_SchwarzePixelHinzufügen : ImageFilter {

    public static ImageFilter_SchwarzePixelHinzufügen Instance { get; } = new();

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        for (var x = 0; x < bitmapData.Width - 1; x++) {
            for (var y = 0; y < bitmapData.Height - 1; y++) {
                if (!bitmapData.IsNearWhiteAt(bits, x + 1, y + 1, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
                if (!bitmapData.IsNearWhiteAt(bits, x + 1, y, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
                if (!bitmapData.IsNearWhiteAt(bits, x, y + 1, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
            }
        }
    }
}
