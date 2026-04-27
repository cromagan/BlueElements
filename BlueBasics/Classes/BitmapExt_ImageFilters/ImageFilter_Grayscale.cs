// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing.Imaging;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_Grayscale : ImageFilter {

    #region Properties

    public static ImageFilter_Grayscale Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var r = bits[i + 2];
            var g = bits[i + 1];
            var b = bits[i];

            // Berechnung des Grauwerts mit der Luma-Formel
            var gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

            // Setzen der Graustufenwerte für alle Farbkanäle
            bits[i] = gray;       // Blau
            bits[i + 1] = gray;   // Grün
            bits[i + 2] = gray;   // Rot
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}