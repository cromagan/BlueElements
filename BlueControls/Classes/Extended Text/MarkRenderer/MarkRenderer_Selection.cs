// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;

namespace BlueControls.Extended_Text.MarkRendering;

public sealed class MarkRenderer_Selection : MarkRenderer {

    #region Fields

    public const string Type = "Selection";

    #endregion

    #region Properties

    public static MarkRenderer_Selection Instance => (MarkRenderer_Selection)(AllRenderers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));
    public override string KeyName => Type;
    public override int Priority => 3;

    #endregion

    #region Methods

    public override void Render(Graphics gr, float zoom, float startX, float startY, float endX, float endY, float height) {
        gr.FillRectangle(BlueFont.GetBrush(Skin.Color_Back(Design.TextBox, States.Checked)), startX, startY, endX - startX, endY - startY);
    }

    #endregion
}