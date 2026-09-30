// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Interfaces;

/// <summary>
/// Für Eingabe-Elemente, deren Cursor zum nächsten Eingabe-Element springen kann, wenn die Eingabe abgeschlossen ist.
/// </summary>
public interface IAutoNext {

    #region Properties

    /// <summary>
    /// Wenn gewählt, springt der Cursor zum nächsten Eingabefeld, wenn am Ende des Textes die Nach-rechts-Taste gedrückt wird.
    /// </summary>
    bool AutoNext { get; }

    #endregion
}
