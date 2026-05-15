// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_Transparenz : ImageFilter {

    #region Properties

    public static ImageFilter_Transparenz Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not int transparenz) { return; }
        if (transparenz is <= 0 or >= 100) { return; }

        for (var i = 0; i < bits.Length; i += 4) {
            var c = Color.FromArgb(bits[i + 3], bits[i + 2], bits[i + 1], bits[i]);
            if (c.IsMagentaOrTransparent()) { continue; }
            c = Color.FromArgb((int)(c.A * (100 - transparenz) / 100.0), c.R, c.G, c.B);
            bits[i] = c.B;
            bits[i + 1] = c.G;
            bits[i + 2] = c.R;
            bits[i + 3] = c.A;
        }
    }

    #endregion
}