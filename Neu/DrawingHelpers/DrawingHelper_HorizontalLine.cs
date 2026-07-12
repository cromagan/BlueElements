// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_HorizontalLine : DrawingHelper {

	#region Fields

	public const string Type = "HorizontalLine";

	#endregion

	#region Properties

	public static DrawingHelper_HorizontalLine Instance => (DrawingHelper_HorizontalLine)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		var p1 = new PointF(0, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		var p2 = new PointF(ctx.Bmp.Width, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		ctx.GR.DrawLine(PenRotTransp, p1, p2);
	}

	#endregion
}
