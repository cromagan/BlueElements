// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Enums;

/// <summary>
/// Legt fest, wie ein Bild an einen vorgegebenen Bereich angepasst wird.
/// </summary>
public enum SizeModes {

    /// <summary>
    /// Passt das Bild komplett in den Bereich ein. Übrig bleibender Platz bleibt leer.
    /// </summary>
    [Image("BildmodusEinpassen|32", "Einpassen")]
    EmptySpace = 1,

    /// <summary>
    /// Füllt den Bereich komplett aus. Bildteile, die nicht hineinpassen, werden abgeschnitten.
    /// </summary>
    [Image("BildmodusAbschneiden|32", "Abschneiden")]
    BildAbschneiden = 2,

    /// <summary>
    /// Passt das Bild in der Breite oder Höhe an. Kleine Bilder bleiben unverändert.
    /// </summary>
    Breite_oder_Höhe_Anpassen_OhneVergrößern = 3,

    /// <summary>
    /// Passt das Bild in der Breite oder Höhe an und vergrößert es dabei bei Bedarf.
    /// </summary>
    Breite_oder_Höhe_Anpassen_MitVergrößern = 4,

    // QuickPicGeneration = 5,

    /// <summary>
    /// Streckt oder staucht das Bild auf den Bereich. Das Bild kann dabei verzerrt wirken.
    /// </summary>
    [Image("BildmodusVerzerren|32")]
    Verzerren = 6
}