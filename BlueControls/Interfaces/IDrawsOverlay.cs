// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.Interfaces;

/// <summary>
/// Implementiert zeichnende Overlays auf einem <see cref="Controls.ZoomPic" />.
/// </summary>
public interface IDrawsOverlay {

    void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent);
}
