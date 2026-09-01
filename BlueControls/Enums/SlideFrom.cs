// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

/// <summary>
/// Kante des Parents, von der ein SlideOutPanel erscheint.
/// </summary>
public enum SlideFrom {

    /// <summary>
    /// Erscheint von oben: Im ausgefahrenen Zustand beginnt das Panel an der
    /// Oberkante des Parents und reicht bis zur entworfenen Unterkante,
    /// eingefahren bleibt oben ein Tab sichtbar.
    /// </summary>
    Top = 0,

    /// <summary>
    /// Erscheint von unten: Im ausgefahrenen Zustand beginnt das Panel an der
    /// entworfenen Oberkante und berührt mit der Unterkante den Parent,
    /// eingefahren bleibt unten ein Tab sichtbar.
    /// </summary>
    Bottom = 1
}
