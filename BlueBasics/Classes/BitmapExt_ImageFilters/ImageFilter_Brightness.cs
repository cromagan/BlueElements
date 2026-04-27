// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing.Imaging;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_Brightness : ImageFilter {

    #region Properties

    public static ImageFilter_Brightness Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        factor = Math.Max(factor, 0.001f);

        // Schleife über alle Pixel im Bild
        for (var y = 0; y < bitmapData.Height; y++) {
            for (var x = 0; x < bitmapData.Width; x++) {
                // Berechnung des Index für den aktuellen Pixel im Byte-Array
                var index = y * bitmapData.Stride + x * 4; // 4 Bytes pro Pixel (RGBA)

                // Anpassen der Helligkeit für jede Farbkomponente (RGB)
                bits[index] = (byte)Math.Clamp(bits[index] * factor, 0, 255); // Blau
                bits[index + 1] = (byte)Math.Clamp(bits[index + 1] * factor, 0, 255); // Grün
                bits[index + 2] = (byte)Math.Clamp(bits[index + 2] * factor, 0, 255); // Rot
                // Alpha-Kanal bleibt unverändert (bits[index + 3])
            }
        }
    }

    #endregion
}