// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Extended_Text.MarkRendering;

public sealed class MarkRenderer_Ringelchen : MarkRenderer {

    #region Fields

    public const string Type = "Ringelchen";

    #endregion

    #region Properties

    public static MarkRenderer_Ringelchen Instance => (MarkRenderer_Ringelchen)(AllRenderers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

    public override string KeyName => Type;
    public override int Priority => 4;

    #endregion

    #region Methods

    public override void Render(Graphics gr, float zoom, float startX, float startY, float endX, float endY, float height) {
        using var pen = new Pen(Color.Red, 3.CanvasToControl(zoom));
        var lineY = (int)(startY + height * 0.9);
        gr.DrawLine(pen, startX, lineY, endX, lineY);
    }

    #endregion
}