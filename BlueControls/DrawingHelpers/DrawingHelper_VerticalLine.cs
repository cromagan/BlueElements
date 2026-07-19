// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_VerticalLine : DrawingHelper {

	#region Fields

	public const string Type = "VerticalLine";

	#endregion

	#region Properties

	public static DrawingHelper_VerticalLine Instance => (DrawingHelper_VerticalLine)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		if (bmp is null) { return; }

		var p1 = new PointF(canvasCoords.X, 0).CanvasToControl(zoom, offsetX, offsetY);
		var p2 = new PointF(canvasCoords.X, bmp.Height).CanvasToControl(zoom, offsetX, offsetY);
		gr.DrawLine(PenRotTransp, p1, p2);
	}

	#endregion
}
