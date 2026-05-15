// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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