// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.ImageFilters;

public class GrayscaleImageFilter : ImageFilter {

    #region Properties

    public static GrayscaleImageFilter Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            var alpha = bits[i + 3];

            // Berechnung des Grauwerts mit der Luma-Formel aus den sichtbaren Farbkomponenten
            var gray = (byte)(0.3 * VisibleFromChannel(bits[i + 2], alpha)
                            + 0.59 * VisibleFromChannel(bits[i + 1], alpha)
                            + 0.11 * VisibleFromChannel(bits[i], alpha));

            // Setzen der Graustufenwerte für alle Farbkanäle
            bits[i] = ChannelFromVisible(gray, alpha);       // Blau
            bits[i + 1] = ChannelFromVisible(gray, alpha);   // Grün
            bits[i + 2] = ChannelFromVisible(gray, alpha);   // Rot
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}