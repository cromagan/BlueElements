// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_MouseDownPoint : DrawingHelper {

	#region Fields

	public const string Type = "MouseDownPoint";

	#endregion

	#region Properties

	public static DrawingHelper_MouseDownPoint Instance => (DrawingHelper_MouseDownPoint)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		var m1 = new PointF(ctx.CanvasCoords.X, ctx.CanvasCoords.Y).CanvasToControl(ctx.Zoom, ctx.OffsetX, ctx.OffsetY);
		ctx.GR.DrawEllipse(PenRotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));

		if (ctx.MouseDownData is not null) {
			var md1 = ctx.MouseDownData.ControlPoint;
			ctx.GR.DrawEllipse(PenRotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
			ctx.GR.DrawLine(PenRotTransp, m1, md1);
		}
	}

	#endregion
}
