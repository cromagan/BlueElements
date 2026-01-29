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
using System;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Classes.BitmapExt;

namespace BluePaint;

public partial class Tool_Eraser : GenericTool {

    #region Constructors

    public Tool_Eraser() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent == null) { return; }

        if (Razi.Checked) {
            e.FillCircle(ColorRedTransp, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 3);
        }

        if (!DrawBox.Checked) {
            return;
        }

        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }

        Point p1, p2;
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            p1 = new Point(Math.Min(e.MouseCurrent.TrimmedCanvasX, e.MouseDown.TrimmedCanvasX), Math.Min(e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasY));
            p2 = new Point(Math.Max(e.MouseCurrent.TrimmedCanvasX, e.MouseDown.TrimmedCanvasX), Math.Max(e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasY));
            e.FillRectangle(BrushRedTransp, e.TrimmedRectangle());
        } else {
            p1 = new Point(e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY);
            p2 = new Point(e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY);
        }
        e.DrawLine(PenRedTransp, -0.5f, p1.Y - 0.5f, pic.Width + 0.5f, p1.Y - 0.5f);
        e.DrawLine(PenRedTransp, p1.X - 0.5f, -0.5f, p1.X - 0.5f, pic.Height + 0.5f);
        e.DrawLine(PenRedTransp, -0.5f, p2.Y + 0.5f, pic.Width + 0.5f, p2.Y + 0.5f);
        e.DrawLine(PenRedTransp, p2.X + 0.5f, 0, p2.X + 0.5f, pic.Height + 0.5f);
        //if (e.MouseCurrent.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
        //    e.DrawLine(Pen_RedTransp, -1, e.MouseDown.TrimmedCanvasY, _Pic.Width, e.MouseDown.TrimmedCanvasY);
        //    e.DrawLine(Pen_RedTransp, e.MouseDown.TrimmedX, -1, e.MouseDown.TrimmedX, _Pic.Height);
        //    e.FillRectangle(Brush_RedTransp, e.TrimmedRectangle());
        //}
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            if (Razi.Checked) {
                var pic = OnNeedCurrentPic();
                FillCircle(pic, Color.White, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 3);
            }
        }
        OnDoInvalidate();
    }

    public override void MouseUp(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button != MouseButtons.Left) { return; }

        if (Razi.Checked) { return; }
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }

        if (Eleminate.Checked) {
            if (e.MouseCurrent.IsInPic) {
                var cc = pic.GetPixel((int)e.MouseCurrent.CanvasX, (int)e.MouseCurrent.CanvasY);
                if (cc.ToArgb() == 0) { return; }
                OnOverridePic(ReplaceColor(originalPic, cc, Color.Transparent), false);
                return;
            }
        }
        if (DrawBox.Checked) {
            var g = Graphics.FromImage(pic);
            g.FillRectangle(Brushes.White, e.TrimmedRectangle());
            OnDoInvalidate();
        }
    }

    private void DrawBox_CheckedChanged(object sender, System.EventArgs e) => OnDoInvalidate();

    #endregion
}