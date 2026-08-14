// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.MarkRenderer;

public sealed class CellLinkMarkRenderer : MarkRenderer {

    #region Fields

    public const string Type = "CellLink";
    private static readonly SolidBrush Brush = new(Color.FromArgb(230, 230, 230));

    #endregion

    #region Properties

    public static CellLinkMarkRenderer Instance => (CellLinkMarkRenderer)(AllRenderers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));
    public override string KeyName => Type;
    public override int Priority => 0;

    #endregion

    #region Methods

    public override void Render(Graphics gr, float zoom, float startX, float startY, float endX, float endY, float height) => gr.FillRectangle(Brush, startX, startY, endX - startX, endY - startY);

    #endregion
}