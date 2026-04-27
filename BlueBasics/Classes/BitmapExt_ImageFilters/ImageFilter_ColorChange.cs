// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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