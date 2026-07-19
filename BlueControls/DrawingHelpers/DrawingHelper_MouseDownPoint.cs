// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

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

	public override void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea) {
		var m1 = new PointF(canvasCoords.X, canvasCoords.Y).CanvasToControl(zoom, offsetX, offsetY);
		gr.DrawEllipse(PenRotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));

		if (mouseDownData is { } mdd) {
			var md1 = mdd.ControlPoint;
			gr.DrawEllipse(PenRotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
			gr.DrawLine(PenRotTransp, m1, md1);
		}
	}

	#endregion
}
