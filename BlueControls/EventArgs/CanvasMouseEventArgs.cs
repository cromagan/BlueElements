// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.EventArgs;

public class CanvasMouseEventArgs : System.EventArgs {

    #region Constructors

    public CanvasMouseEventArgs(MouseEventArgs mouseControl, float scale, float offsetX, float offsetY) : base() {
        ControlX = mouseControl.X;
        ControlY = mouseControl.Y;
        Clicks = mouseControl.Clicks;
        Delta = mouseControl.Delta;
        Button = mouseControl.Button;

        CanvasX = mouseControl.X.ControlToCanvas(scale, offsetX);
        CanvasY = mouseControl.Y.ControlToCanvas(scale, offsetY);

        ControlPoint = new Point(ControlX, ControlY);
        CanvasPoint = new PointF(CanvasX, ControlY);
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
        CanvasPoint = new PointF(CanvasX, ControlY);
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

    #region Methods

    public MouseEventArgs ToMouseEventArgs() => new MouseEventArgs(Button, Clicks, ControlX, ControlY, Delta);

    #endregion
}