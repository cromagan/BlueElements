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

public abstract class ImageFilter {

    #region Properties

    public object? Parameter { get; set; }

    #endregion

    #region Methods

    public abstract void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias);

    public virtual void ProcessFilter(Bitmap image) {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try {
            var bits = new byte[bitmapData.Stride * image.Height];
            Marshal.Copy(bitmapData.Scan0, bits, 0, bits.Length);
            ProcessFilter(bitmapData, bits, 0);
            Marshal.Copy(bits, 0, bitmapData.Scan0, bits.Length);
        } finally {
            image.UnlockBits(bitmapData);
        }
    }

    #endregion
}