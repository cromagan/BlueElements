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

    #endregion
}