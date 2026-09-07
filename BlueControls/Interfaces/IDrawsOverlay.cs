// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.EventArgs;

namespace BlueControls.Interfaces;

/// <summary>
/// Implementiert zeichnende Overlays auf einem Controls.ZoomPic.
/// </summary>
public interface IDrawsOverlay {

    void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent);
}
