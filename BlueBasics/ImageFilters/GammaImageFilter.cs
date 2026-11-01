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

        // Lookup-Tabelle je Alpha-Wert: Kanalwert → gefilterter Kanalwert, exakt über den sichtbaren Farbwert
        var gammaArray = new byte[256][];
        for (var a = 0; a < 256; a++) {
            var lut = new byte[256];
            for (var i = 0; i < 256; i++) {
                lut[i] = ChannelFromVisible((float)(255.0 * Math.Pow(VisibleFromChannel(i, a) / 255.0, 1.0 / factor)), a);
            }
            gammaArray[a] = lut;
        }

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            var lut = gammaArray[bits[i + 3]];

            // Gamma-Korrektur der Kanäle über die Tabelle
            bits[i + 2] = lut[bits[i + 2]]; // Rot-Komponente
            bits[i + 1] = lut[bits[i + 1]]; // Grün-Komponente
            bits[i] = lut[bits[i]];         // Blau-Komponente
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}