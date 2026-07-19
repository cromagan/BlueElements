// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

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

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		if (bmp is null) { return; }
		if (currentMouseData is not { } cmd) { return; }

		const int magnifierSize = 200;
		const int magnifierMargin = 50;

		var mouseControl = cmd.ControlPoint;

		var mx = mouseControl.X < availableControlPaintArea.Left + availableControlPaintArea.Width / 2.0
			? availableControlPaintArea.Right - magnifierMargin - magnifierSize
			: availableControlPaintArea.Left + magnifierMargin;
		var my = mouseControl.Y < availableControlPaintArea.Top + availableControlPaintArea.Height / 2.0
			? availableControlPaintArea.Bottom - magnifierMargin - magnifierSize
			: availableControlPaintArea.Top + magnifierMargin;

		bmp.Magnify(cmd.CanvasPoint, new Rectangle(mx, my, magnifierSize, magnifierSize), gr);
	}

	#endregion
}
