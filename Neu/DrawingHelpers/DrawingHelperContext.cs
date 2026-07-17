// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.DrawingHelpers;

public readonly struct DrawingHelperContext {

	#region Properties

	public required Graphics GR { get; init; }
	public required float Zoom { get; init; }
	public required float OffsetX { get; init; }
	public required float OffsetY { get; init; }
	public required Bitmap Bmp { get; init; }
	public required CanvasMouseEventArgs CurrentMouseData { get; init; }
	public required CanvasMouseEventArgs? MouseDownData { get; init; }
	public required PositionEventArgs CanvasCoords { get; init; }
	public required Rectangle AvailablePaintArea { get; init; }

	#endregion
}
