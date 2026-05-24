// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Extended_Text.MarkRendering;

public sealed class MarkRenderer_Field : MarkRenderer {

    #region Fields

    private static readonly SolidBrush Brush = new(Color.FromArgb(80, 128, 128, 128));

    public const string Type = "Field";

    #endregion

    #region Properties

    public static MarkRenderer_Field Instance => (MarkRenderer_Field)(AllRenderers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));
    public override string KeyName => Type;
    public override int Priority => 1;

    #endregion

    #region Methods

    public override void Render(Graphics gr, float zoom, float startX, float startY, float endX, float endY, float height) {
        gr.FillRectangle(Brush, startX, startY, endX - startX, endY - startY);
    }

    #endregion
}