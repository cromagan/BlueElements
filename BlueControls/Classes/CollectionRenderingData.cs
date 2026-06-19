// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes;

/// <summary>
/// Speichert berechnete Breiteninformationen für eine gesamte ColumnViewCollection,
/// z. B. die Gesamtbreite aller Spalten und der permanenten Spalten.
/// Wird als Weak-Reference-Datenträger in ConditionalWeakTable verwendet.
/// </summary>
public sealed class CollectionRenderingData {

    #region Properties

    public int ControlColumnsPermanentWidth { get; set; }

    public int ControlColumnsWidth { get; set; }

    /// <summary>
    /// Die beim letzten Durchlauf von ComputeAllColumnPositions verwendete Tabellenbreite.
    /// Dient der Erkennung, ob sich die verfügbare Breite geändert hat (z. B. durch
    /// Ein-/Ausblenden eines Sliders) und ein erneutes Berechnen nötig ist.
    /// </summary>
    public int LastUsedTableViewWidth { get; set; }

    #endregion
}