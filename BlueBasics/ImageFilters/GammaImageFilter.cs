// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.ImageFilters;

public class GammaImageFilter : ImageFilter {

    #region Properties

    public static GammaImageFilter Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        factor = Math.Max(factor, 0.001f);

        // Erstellen Sie eine Lookup-Tabelle für die GammaImageFilter-Korrektur
        var gammaArray = new byte[256];
        for (var i = 0; i < 256; ++i) {
            gammaArray[i] = (byte)Math.Min(255, (int)(255.0 * Math.Pow(i / 255.0, 1.0 / factor) + 0.5));
        }

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            var alpha = bits[i + 3];

            // GammaImageFilter-Korrektur der sichtbaren Farbkomponenten
            bits[i + 2] = ChannelFromVisible(gammaArray[VisibleFromChannel(bits[i + 2], alpha)], alpha); // Rot-Komponente
            bits[i + 1] = ChannelFromVisible(gammaArray[VisibleFromChannel(bits[i + 1], alpha)], alpha); // Grün-Komponente
            bits[i] = ChannelFromVisible(gammaArray[VisibleFromChannel(bits[i], alpha)], alpha);         // Blau-Komponente
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}