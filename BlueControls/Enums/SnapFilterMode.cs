// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Enums;

/// <summary>
/// Legt fest, wann ein Filter-Eingabefeld zu einem Filter-Button einschnappt.
/// </summary>
public enum SnapFilterMode {

    /// <summary>
    /// Das Eingabefeld schnappt nie ein und bleibt dauerhaft editierbar.
    /// </summary>
    Niemals = 0,

    /// <summary>
    /// Es schnappt ein, sobald der eingegebene Text dem Spaltenformat entspricht.
    /// </summary>
    Wenn_Format_Stimmt = 1,

    /// <summary>
    /// Es schnappt ein, sobald der Wert in der Tabelle tatsächlich vorhanden ist.
    /// </summary>
    Wenn_Vorhanden = 2,

    /// <summary>
    /// Es schnappt ein, sobald der eingegebene Text die maximale Textlänge der Spalte erreicht hat.
    /// </summary>
    Maximallänge_erreicht = 3,

    /// <summary>
    /// Es schnappt ein, sobald der Text dem Spaltenformat entspricht und zugleich die maximale Textlänge erreicht ist.
    /// </summary>
    Wenn_Format_Stimmt_UND_Maximallänge_erreicht = 4
}
