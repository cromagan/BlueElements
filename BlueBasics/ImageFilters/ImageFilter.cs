// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Runtime.InteropServices;

namespace BlueBasics.ImageFilters;

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

    /// <summary>
    /// Liefert den sichtbaren Farbwert eines Kanals über weißem Untergrund.
    /// </summary>
    protected static byte VisibleFromChannel(byte value, byte alpha) {
        if (alpha == 255) { return value; }
        if (alpha == 0) { return 255; }
        return (byte)((value * alpha + 255 * (255 - alpha) + 127) / 255);
    }

    /// <summary>
    /// Rechnet einen gefilterten sichtbaren Farbwert unter Erhalt des Alphas in den Kanalwert zurück: c' = (v − (255 − a)) · 255 / a.
    /// </summary>
    protected static byte ChannelFromVisible(byte visible, byte alpha) {
        if (alpha == 255) { return visible; }
        if (alpha == 0) { return 0; }
        return (byte)Math.Clamp((visible - (255 - alpha)) * 255 / alpha, 0, 255);
    }

    #endregion
}