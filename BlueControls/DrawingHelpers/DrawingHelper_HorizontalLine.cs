// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

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

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		if (bmp is null) { return; }

		var p1 = new PointF(0, canvasCoords.Y).CanvasToControl(zoom, offsetX, offsetY);
		var p2 = new PointF(bmp.Width, canvasCoords.Y).CanvasToControl(zoom, offsetX, offsetY);
		gr.DrawLine(PenRotTransp, p1, p2);
	}

	#endregion
}
