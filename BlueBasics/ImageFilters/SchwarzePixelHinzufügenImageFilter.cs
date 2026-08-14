// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.ImageFilters;

public class SchwarzePixelHinzufügenImageFilter : ImageFilter {

    #region Properties

    public static SchwarzePixelHinzufügenImageFilter Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        for (var x = 0; x < bitmapData.Width - 1; x++) {
            for (var y = 0; y < bitmapData.Height - 1; y++) {
                if (!bitmapData.IsNearWhiteAt(bits, x + 1, y + 1, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
                if (!bitmapData.IsNearWhiteAt(bits, x + 1, y, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
                if (!bitmapData.IsNearWhiteAt(bits, x, y + 1, 0.9)) { bitmapData.SetPixelArgb(bits, x, y, 255, 0, 0, 0); }
            }
        }
    }

    #endregion
}