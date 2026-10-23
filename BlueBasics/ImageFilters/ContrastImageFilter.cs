// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.ImageFilters;

public class ContrastImageFilter : ImageFilter {

    #region Properties

    public static ContrastImageFilter Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        factor = (100.0f + factor) / 100.0f;
        factor *= factor;

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            var alpha = bits[i + 3];

            // Extrahieren der sichtbaren Farbkomponenten aus dem Pixel
            var r = VisibleFromChannel(bits[i + 2], alpha);
            var g = VisibleFromChannel(bits[i + 1], alpha);
            var b = VisibleFromChannel(bits[i], alpha);

            // Anpassen des Kontrasts für jede Farbkomponente und Begrenzen der Farbwerte
            r = (byte)Math.Clamp(((r / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);
            g = (byte)Math.Clamp(((g / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);
            b = (byte)Math.Clamp(((b / 255f - 0.5f) * factor + 0.5f) * 255.0f, 0, 255);

            // Zurückrechnen in die Kanalwerte (Alpha bleibt unverändert)
            bits[i + 2] = ChannelFromVisible(r, alpha);
            bits[i + 1] = ChannelFromVisible(g, alpha);
            bits[i] = ChannelFromVisible(b, alpha);
        }
    }

    #endregion
}