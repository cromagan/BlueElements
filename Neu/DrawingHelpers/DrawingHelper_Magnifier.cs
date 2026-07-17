// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_Magnifier : DrawingHelper {

	#region Fields

	public const string Type = "Magnifier";

	#endregion

	#region Properties

	public static DrawingHelper_Magnifier Instance => (DrawingHelper_Magnifier)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	// Wird nach Overlay und InfoText gezeichnet, damit die Lupe oben liegt.
	public override bool DrawsAfterOverlay => true;

	#endregion

	#region Methods

	public override void Draw(in DrawingHelperContext ctx) {
		const int magnifierSize = 200;
		const int magnifierMargin = 50;

		var visibleArea = ctx.AvailablePaintArea;
		var mouseControl = ctx.CurrentMouseData.ControlPoint;

		var mx = mouseControl.X < visibleArea.Left + visibleArea.Width / 2.0
			? visibleArea.Right - magnifierMargin - magnifierSize
			: visibleArea.Left + magnifierMargin;
		var my = mouseControl.Y < visibleArea.Top + visibleArea.Height / 2.0
			? visibleArea.Bottom - magnifierMargin - magnifierSize
			: visibleArea.Top + magnifierMargin;

		ctx.Bmp.Magnify(ctx.CurrentMouseData.CanvasPoint, new Rectangle(mx, my, magnifierSize, magnifierSize), ctx.GR);
	}

	#endregion
}
