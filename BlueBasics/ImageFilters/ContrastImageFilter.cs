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

            // Anpassen des Kontrasts der sichtbaren Farbkomponenten und exaktes Zurückrechnen in die Kanalwerte
            var r = ((VisibleFromChannel(bits[i + 2], alpha) / 255f - 0.5f) * factor + 0.5f) * 255f;
            var g = ((VisibleFromChannel(bits[i + 1], alpha) / 255f - 0.5f) * factor + 0.5f) * 255f;
            var b = ((VisibleFromChannel(bits[i], alpha) / 255f - 0.5f) * factor + 0.5f) * 255f;

            bits[i + 2] = ChannelFromVisible(r, alpha);
            bits[i + 1] = ChannelFromVisible(g, alpha);
            bits[i] = ChannelFromVisible(b, alpha);
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}