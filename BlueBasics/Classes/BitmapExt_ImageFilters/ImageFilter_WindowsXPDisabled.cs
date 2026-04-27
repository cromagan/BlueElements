// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;
using System.Drawing.Imaging;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_WindowsXPDisabled : ImageFilter {

    #region Properties

    public static ImageFilter_WindowsXPDisabled Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        for (var i = 0; i < bits.Length; i += 4) {
            var c = Color.FromArgb(bits[i + 3], bits[i + 2], bits[i + 1], bits[i]);
            if (c.IsMagentaOrTransparent()) { continue; }
            var w = (int)(c.GetBrightness() * 100);
            w = (int)(w / 2.8);
            c = Extensions.FromHsb(0, 0, (float)(w / 100.0 + 0.5), c.A);
            bits[i] = c.B;
            bits[i + 1] = c.G;
            bits[i + 2] = c.R;
        }
    }

    #endregion
}