// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_Draw20x10 : DrawingHelper {

	#region Fields

	public const string Type = "Draw20x10";

	#endregion

	#region Properties

	public static DrawingHelper_Draw20x10 Instance => (DrawingHelper_Draw20x10)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		var startPoint = new PointF(ctx.CurrentMouseData.CanvasX - 10, ctx.CurrentMouseData.CanvasY - 5);
		var scaledStart = startPoint.CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		var scaledWidth = 20 * ctx.Zoom;
		var scaledHeight = 10 * ctx.Zoom;
		ctx.GR.DrawRectangle(PenRotTransp, scaledStart.X, scaledStart.Y, scaledWidth, scaledHeight);
	}

	#endregion
}
