// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_SymetricalHorizontal : DrawingHelper {

	#region Fields

	public const string Type = "SymetricalHorizontal";

	#endregion

	#region Properties

	public static DrawingHelper_SymetricalHorizontal Instance => (DrawingHelper_SymetricalHorizontal)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		var h = ctx.Bmp.Width / 2;
		var x = Math.Abs(h - ctx.CanvasCoords.X);
		var p1 = new PointF(h - x, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		var p2 = new PointF(h + x, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		ctx.GR.DrawLine(PenRotTransp, p1, p2);
	}

	#endregion
}
