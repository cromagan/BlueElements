// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.ImageFilters;

public class AllePixelZuSchwarzImageFilter : ImageFilter {

    #region Properties

    public static AllePixelZuSchwarzImageFilter Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        var threshold = (int)(255 * factor);

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            var alpha = bits[i + 3];

            // Berechnen der sichtbaren Helligkeit des Pixels
            var brightness = 0.3f * VisibleFromChannel(bits[i + 2], alpha)
                           + 0.59f * VisibleFromChannel(bits[i + 1], alpha)
                           + 0.11f * VisibleFromChannel(bits[i], alpha);

            // Überprüfen, ob die Helligkeit nahe dem Weißwert liegt
            if (brightness < threshold) {
                // Wenn nicht, setzen Sie die Farbe auf Schwarz (Alpha bleibt unverändert)
                bits[i] = 0;       // Blau
                bits[i + 1] = 0;   // Grün
                bits[i + 2] = 0;   // Rot
            }
        }
    }

    #endregion
}