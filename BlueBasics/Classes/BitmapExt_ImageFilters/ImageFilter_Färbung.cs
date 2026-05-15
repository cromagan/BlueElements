// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_Färbung : ImageFilter {

    #region Properties

    public static ImageFilter_Färbung Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not Color cf || cf.A <= 0) { return; }

        for (var i = 0; i < bits.Length; i += 4) {
            var c = Color.FromArgb(bits[i + 3], bits[i + 2], bits[i + 1], bits[i]);
            if (c.IsMagentaOrTransparent()) { continue; }
            c = cf.GetHue().FromHsb(cf.GetSaturation(), c.GetBrightness(), c.A);
            bits[i] = c.B;
            bits[i + 1] = c.G;
            bits[i + 2] = c.R;
        }
    }

    #endregion
}