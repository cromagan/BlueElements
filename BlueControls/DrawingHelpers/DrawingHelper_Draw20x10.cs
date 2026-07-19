// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

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

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		if (currentMouseData is not { } cmd) { return; }

		var startPoint = new PointF(cmd.CanvasX - 10, cmd.CanvasY - 5);
		var scaledStart = startPoint.CanvasToControl(zoom, offsetX, offsetY);
		var scaledWidth = 20 * zoom;
		var scaledHeight = 10 * zoom;
		gr.DrawRectangle(PenRotTransp, scaledStart.X, scaledStart.Y, scaledWidth, scaledHeight);
	}

	#endregion
}
