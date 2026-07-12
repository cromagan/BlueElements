// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_FilledRectangle : DrawingHelper {

	#region Fields

	public const string Type = "FilledRectangle";

	#endregion

	#region Properties

	public static DrawingHelper_FilledRectangle Instance => (DrawingHelper_FilledRectangle)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		if (ctx.MouseDownData is not null) {
			var md1 = ctx.MouseDownData.ControlPoint;
			var mc1 = new PointF(ctx.CanvasCoords.X, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
			var r = new RectangleF(Math.Min(md1.X, ctx.CanvasCoords.X), Math.Min(md1.Y, ctx.CanvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
			ctx.GR.FillRectangle(BrushRotTransp, r);
		}
	}

	#endregion
}
