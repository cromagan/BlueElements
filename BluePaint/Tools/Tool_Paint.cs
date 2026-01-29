// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueControls.EventArgs;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Classes.BitmapExt;

namespace BluePaint;

public partial class Tool_Paint {

    #region Constructors

    public Tool_Paint() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        var c = Color.FromArgb(50, 255, 0, 0);
        e.FillCircle(c, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 2);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            var pic = OnNeedCurrentPic();
            FillCircle(pic, Color.Black, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 2);
            OnDoInvalidate();
        } else {
            OnDoInvalidate();
        }
    }

    #endregion
}