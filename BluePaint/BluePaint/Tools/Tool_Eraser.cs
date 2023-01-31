// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueBasics;
using BlueControls.EventArgs;
using System;
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BluePaint;

public partial class Tool_Eraser : GenericTool {

    #region Constructors

    public Tool_Eraser() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) {
        if (Razi.Checked && e.Current != null) {
            e.FillCircle(ColorRedTransp, e.Current.TrimmedX, e.Current.TrimmedY, 3);
        }
        //e.FillRectangle(Brush_RedTransp, new Rectangle(e.Current.TrimmedX, e.Current.TrimmedY, 1, 1));
        if (!DrawBox.Checked || e.Current == null) {
            return;
        }

        var pic = OnNeedCurrentPic();
        Point p1, p2;
        if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
            p1 = new Point(Math.Min(e.Current.TrimmedX, e.MouseDown.TrimmedX), Math.Min(e.Current.TrimmedY, e.MouseDown.TrimmedY));
            p2 = new Point(Math.Max(e.Current.TrimmedX, e.MouseDown.TrimmedX), Math.Max(e.Current.TrimmedY, e.MouseDown.TrimmedY));
            e.FillRectangle(BrushRedTransp, e.TrimmedRectangle());
        } else {
            p1 = new Point(e.Current.TrimmedX, e.Current.TrimmedY);
            p2 = new Point(e.Current.TrimmedX, e.Current.TrimmedY);
        }
        e.DrawLine(PenRedTransp, -0.5f, p1.Y - 0.5f, pic.Width + 0.5f, p1.Y - 0.5f);
        e.DrawLine(PenRedTransp, p1.X - 0.5f, -0.5f, p1.X - 0.5f, pic.Height + 0.5f);
        e.DrawLine(PenRedTransp, -0.5f, p2.Y + 0.5f, pic.Width + 0.5f, p2.Y + 0.5f);
        e.DrawLine(PenRedTransp, p2.X + 0.5f, 0, p2.X + 0.5f, pic.Height + 0.5f);
        //if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
        //    e.DrawLine(Pen_RedTransp, -1, e.MouseDown.TrimmedY, _Pic.Width, e.MouseDown.TrimmedY);
        //    e.DrawLine(Pen_RedTransp, e.MouseDown.TrimmedX, -1, e.MouseDown.TrimmedX, _Pic.Height);
        //    e.FillRectangle(Brush_RedTransp, e.TrimmedRectangle());
        //}
    }

    public override void ExcuteCommand(string command) {
        var c = command.SplitAndCutBy(";");
        if (c[0] == "Replace") {
            var originalPic = OnNeedCurrentPic();
            var cc = Color.FromArgb(IntParse(c[1]));
            OnOverridePic(ReplaceColor(originalPic, cc, Color.Transparent));
        } else {
            Develop.DebugPrint_NichtImplementiert();
        }
    }

    public override void MouseDown(MouseEventArgs1_1 e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), originalPic);
    }

    public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
        if (e.Current.Button == System.Windows.Forms.MouseButtons.Left) {
            if (Razi.Checked) {
                var pic = OnNeedCurrentPic();
                FillCircle(pic, Color.White, e.Current.TrimmedX, e.Current.TrimmedY, 3);
            }
        }
        OnDoInvalidate();
    }

    public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
        if (e == null) {
            Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Warnung, "e = null");
            return;
        }
        if (e.Current.Button != System.Windows.Forms.MouseButtons.Left) { return; }

        if (Razi.Checked) { return; }
        var pic = OnNeedCurrentPic();
        if (Eleminate.Checked) {
            if (e.Current.IsInPic) {
                var cc = pic.GetPixel(e.Current.X, e.Current.Y);
                if (cc.ToArgb() == 0) { return; }
                OnOverridePic(ReplaceColor(originalPic, cc, Color.Transparent));
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