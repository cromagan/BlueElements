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

#nullable enable

using System.Drawing;
using System.Windows.Forms;
using BlueControls.EventArgs;
using static BlueBasics.BitmapExt;

namespace BluePaint;

public partial class Tool_Paint {

    #region Constructors

    public Tool_Paint() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) {
        var c = Color.FromArgb(50, 255, 0, 0);
        e.FillCircle(c, e.Current.TrimmedX, e.Current.TrimmedY, 2);
    }

    public override void MouseDown(MouseEventArgs1_1 e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), originalPic);
    }

    public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
        if (e.Current.Button == MouseButtons.Left) {
            var pic = OnNeedCurrentPic();
            FillCircle(pic, Color.Black, e.Current.TrimmedX, e.Current.TrimmedY, 2);
            OnDoInvalidate();
        } else {
            OnDoInvalidate();
        }
    }

    #endregion
}