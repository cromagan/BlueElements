// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing.Imaging;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_Contrast : ImageFilter {

    #region Properties

    public static ImageFilter_Contrast Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        factor = (100.0f + factor) / 100.0f;
        factor *= factor;

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var r = bits[i + 2];
            var g = bits[i + 1];
            var b = bits[i];

            // Anpassen des Kontrasts für jede Farbkomponente und Begrenzen der Farbwerte
            r = (byte)Math.Clamp(((r / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);
            g = (byte)Math.Clamp(((g / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);
            b = (byte)Math.Clamp(((b / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);

            // Aktualisieren der Farbkomponenten im Array
            bits[i + 2] = r;
            bits[i + 1] = g;
            bits[i] = b;
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}