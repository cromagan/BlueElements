// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes;

/// <summary>
/// Speichert berechnete Rendering-Daten für ein einzelnes ColumnViewItem,
/// z. B. Position, Breite, Canvas-Inhaltsbreite und den zugehörigen Renderer.
/// Wird als Weak-Reference-Datenträger in ConditionalWeakTable verwendet.
/// </summary>
public sealed class RenderingData {

    #region Properties

    public int? CanvasContentWidth { get; set; }

    public int ControlColumnLeft { get; set; }

    public int? ControlColumnWidth { get; set; }

    public Renderer.Renderer? Renderer { get; set; }

    #endregion
}
