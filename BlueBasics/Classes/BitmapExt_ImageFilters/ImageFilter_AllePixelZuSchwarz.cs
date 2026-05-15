// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

public class ImageFilter_AllePixelZuSchwarz : ImageFilter {

    #region Properties

    public static ImageFilter_AllePixelZuSchwarz Instance { get; } = new();

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, int bias) {
        if (Parameter is not float factor) { return; }
        var threshold = (int)(255 * factor);

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der ARGB-Komponenten aus dem Integer-Wert
            //var a = bits[i + 3]; // Alpha-Komponente
            var r = bits[i + 2]; // Rot-Komponente
            var g = bits[i + 1]; // Grün-Komponente
            var b = bits[i];     // Blau-Komponente

            // Berechnen der Helligkeit des Pixels
            var brightness = 0.3f * r + 0.59f * g + 0.11f * b;

            // Überprüfen, ob die Helligkeit nahe dem Weißwert liegt
            if (brightness < threshold) {
                // Wenn nicht, setzen Sie die Farbe auf Schwarz
                bits[i] = 0;       // Blau
                bits[i + 1] = 0;   // Grün
                bits[i + 2] = 0;   // Rot
            }
        }
    }

    #endregion
}