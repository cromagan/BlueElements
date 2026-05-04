// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.EventArgs;

public class CanvasMouseEventArgs : System.EventArgs {

    #region Constructors

    public CanvasMouseEventArgs() { }

    public CanvasMouseEventArgs(MouseEventArgs mouseControl, float scale, float offsetX, float offsetY) : base() {
        ControlX = mouseControl.X;
        ControlY = mouseControl.Y;
        Clicks = mouseControl.Clicks;
        Delta = mouseControl.Delta;
        Button = mouseControl.Button;

        CanvasX = mouseControl.X.ControlToCanvas(scale, offsetX);
        CanvasY = mouseControl.Y.ControlToCanvas(scale, offsetY);

        ControlPoint = new Point(ControlX, ControlY);
        CanvasPoint = new PointF(CanvasX, CanvasY);
    }

    public CanvasMouseEventArgs(CanvasMouseEventArgs e) : base() {
        ControlX = e.ControlX;
        ControlY = e.ControlY;
        Clicks = e.Clicks;
        Delta = e.Delta;
        Button = e.Button;

        CanvasX = e.CanvasX;
        CanvasY = e.CanvasY;

        ControlPoint = new Point(ControlX, ControlY);
        CanvasPoint = new PointF(CanvasX, CanvasY);
    }

    public CanvasMouseEventArgs(float canvasX, float canvasY, float scale, float offsetX, float offsetY) {
        CanvasX = canvasX;
        CanvasY = canvasY;

        ControlX = canvasX.CanvasToControl(scale, offsetX);
        ControlY = canvasY.CanvasToControl(scale, offsetY);

        ControlPoint = new Point(ControlX, ControlY);
        CanvasPoint = new PointF(CanvasX, CanvasY);
    }

    #endregion

    #region Properties

    public MouseButtons Button { get; }

    public PointF CanvasPoint { get; }

    public float CanvasX { get; }

    public float CanvasY { get; }

    public int Clicks { get; }

    public Point ControlPoint { get; }

    public int ControlX { get; }

    public int ControlY { get; }

    public int Delta { get; }

    #endregion
}