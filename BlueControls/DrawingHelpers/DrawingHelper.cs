// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.DrawingHelpers;

public abstract class DrawingHelper : IHasKeyName {

	#region Fields

	protected static readonly Pen PenRotTransp = new(Color.FromArgb(200, 255, 0, 0));
	protected static readonly Brush BrushRotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));

	public static readonly AssemblyAwareCache<DrawingHelper> AllHelpers = new();

	#endregion

	#region Constructors

	protected DrawingHelper() { }

	#endregion

	#region Properties

	public abstract string KeyName { get; }

	/// <summary>
	/// Wenn true, wird der Helper erst nach dem Zurücksetzen des Clips
	/// sowie nach Overlay und InfoText gezeichnet (z.B. Lupe).
	/// </summary>
	public virtual bool DrawsAfterOverlay => false;

	#endregion

	#region Methods

	public abstract void Draw(Graphics gr, PositionEventArgs canvasCoords, float zoom, int offsetX, int offsetY, Bitmap? bmp, CanvasMouseEventArgs? mouseDownData, CanvasMouseEventArgs? currentMouseData, Rectangle availableControlPaintArea);

	#endregion
}
