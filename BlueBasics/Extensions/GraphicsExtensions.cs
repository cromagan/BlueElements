// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static void DrawImageInRectAspectRatio(this Graphics gR, Bitmap? bmp, int x, int y, int width, int height) {
        if (bmp == null) {
            return;
        }

        var sc = Math.Min((float)width / bmp.Width, (float)height / bmp.Height);
        var dw = (int)(bmp.Width * sc);
        var dh = (int)(bmp.Height * sc);
        gR.DrawImage(bmp, x + ((width - dw) / 2), y + ((height - dh) / 2), dw, dh);
    }

    #endregion
}