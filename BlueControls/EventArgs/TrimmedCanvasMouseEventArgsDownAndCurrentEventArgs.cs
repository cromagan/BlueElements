// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs : System.EventArgs {

    #region Constructors

    public TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(TrimmedCanvasMouseEventArgs mouseDown, TrimmedCanvasMouseEventArgs mouseCurrent) : base() {
        MouseDown = mouseDown;
        MouseCurrent = mouseCurrent;
    }

    #endregion

    #region Properties

    public TrimmedCanvasMouseEventArgs MouseCurrent { get; }
    public TrimmedCanvasMouseEventArgs MouseDown { get; }

    #endregion

    #region Methods

    public Rectangle TrimmedRectangle() => new(Math.Min(MouseDown.TrimmedCanvasX, MouseCurrent.TrimmedCanvasX), Math.Min(MouseDown.TrimmedCanvasY, MouseCurrent.TrimmedCanvasY), Math.Abs(MouseDown.TrimmedCanvasX - MouseCurrent.TrimmedCanvasX) + 1, Math.Abs(MouseDown.TrimmedCanvasY - MouseCurrent.TrimmedCanvasY) + 1);

    #endregion
}