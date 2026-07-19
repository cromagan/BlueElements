// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.DrawingHelpers;

public sealed class DrawingHelper_DrawRectangle : DrawingHelper {

	#region Fields

	public const string Type = "DrawRectangle";

	#endregion

	#region Properties

	public static DrawingHelper_DrawRectangle Instance => (DrawingHelper_DrawRectangle)(AllHelpers[Type] ?? throw Develop.DebugError("Fehlerhafter Instanzname"));

	public override string KeyName => Type;

	#endregion

	#region Methods

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		if (mouseDownData is not { } mdd) { return; }

		var md1 = mdd.ControlPoint;
		var mc1 = new PointF(canvasCoords.X, canvasCoords.Y).CanvasToControl(zoom, offsetX, offsetY);
		var r = new RectangleF(Math.Min(md1.X, canvasCoords.X), Math.Min(md1.Y, canvasCoords.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
		gr.DrawRectangle(PenRotTransp, r.X, r.Y, r.Width, r.Height);
	}

	#endregion
}
