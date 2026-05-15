// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
