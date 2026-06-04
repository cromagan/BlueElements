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
        var lineY = startY + height * 0.88f;
        var radius = 2f.CanvasToControl(zoom);
        var diameter = radius * 2;
        using var pen = new Pen(Color.Red, 1f.CanvasToControl(zoom));
        var x = startX;
        var isTop = true;
        while (x + diameter <= endX) {
            gr.DrawArc(pen, new RectangleF(x, lineY - radius, diameter, diameter), isTop ? 0 : 180, 180);
            x += diameter;
            isTop = !isTop;
        }
    }

    #endregion
}