// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.ImageFilters;

public class BrightnessImageFilter : ImageFilter {

    #region Properties

    public static BrightnessImageFilter Instance { get; } = new();

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
                var alpha = bits[index + 3];

                // Anpassen der sichtbaren Helligkeit für jede Farbkomponente (RGB) und exaktes Zurückrechnen
                bits[index] = ChannelFromVisible(VisibleFromChannel(bits[index], alpha) * factor, alpha); // Blau
                bits[index + 1] = ChannelFromVisible(VisibleFromChannel(bits[index + 1], alpha) * factor, alpha); // Grün
                bits[index + 2] = ChannelFromVisible(VisibleFromChannel(bits[index + 2], alpha) * factor, alpha); // Rot
                // Alpha-Kanal bleibt unverändert (bits[index + 3])
            }
        }
    }

    #endregion
}