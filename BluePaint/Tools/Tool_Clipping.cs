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

using BlueBasics.Enums;
using BlueControls.EventArgs;
using System;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.BitmapExt;
using static BlueBasics.Generic;

namespace BluePaint;

public partial class Tool_Clipping {

    #region Constructors

    public Tool_Clipping() : base() {
        InitializeComponent();

        CheckMinMax();
        btnAutoZ_Click(null, null);
        ZuschnittOK_Click(null, null);
        BlueControls.Forms.MessageBox.Show("Automatisch zugeschnitten.");
    }

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        if (originalPic == null || e.MouseCurrent == null) { return; }
        Pen penBlau = new(Color.FromArgb(150, 0, 0, 255));
        DrawZusatz(e, originalPic);
        e.DrawLine(penBlau, e.MouseCurrent.TrimmedCanvasX, -1, e.MouseCurrent.TrimmedCanvasX, originalPic.Height);
        e.DrawLine(penBlau, -1, e.MouseCurrent.TrimmedCanvasY, originalPic.Width, e.MouseCurrent.TrimmedCanvasY);

        if (e.MouseCurrent.Button == MouseButtons.Left && e.MouseDown != null) {
            e.DrawLine(penBlau, e.MouseDown.CanvasX, -1, e.MouseDown.CanvasX, originalPic.Height);
            e.DrawLine(penBlau, -1, e.MouseDown.CanvasY, originalPic.Width, e.MouseDown.CanvasY);
        }
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) => OnDoInvalidate();

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) => OnDoInvalidate();

    public override void MouseUp(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (originalPic == null) { return; }
        Links.Value = Math.Min(e.MouseCurrent.TrimmedCanvasX, e.MouseDown.TrimmedCanvasX) + 1;
        Recht.Value = -(originalPic.Width - Math.Max(e.MouseCurrent.TrimmedCanvasX, e.MouseDown.TrimmedCanvasX));
        Oben.Value = Math.Min(e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasY) + 1;
        Unten.Value = -(originalPic.Height - Math.Max(e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasY));
        ValueChangedByClicking(this, System.EventArgs.Empty);
    }

    public override void OnToolChanging() => WollenSieDenZuschnittÜbernehmen();

    internal override void ToolFirstShown() {
        CheckMinMax();
        btnAutoZ_Click(null, null);
    }

    private void btnAutoZ_Click(object? sender, System.EventArgs? e) {
        WollenSieDenZuschnittÜbernehmen();
        var pic = OnNeedCurrentPic();
        OnZoomFit();
        var pa = GetAutoValuesForCrop(pic, 0.9);
        Links.Value = pa.Left;
        Recht.Value = pa.Right;
        Oben.Value = pa.Top;
        Unten.Value = pa.Bottom;
        OnDoInvalidate();
    }

    private void CheckMinMax() {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        Links.Maximum = pic.Width - 1;
        Recht.Minimum = -pic.Width + 1;
        Oben.Maximum = pic.Height - 1;
        Unten.Minimum = -pic.Height - 1;
    }

    private void DrawZusatz(AdditionalDrawingEventArgs e, Image? originalPic) {
        SolidBrush brushBlau = new(Color.FromArgb(120, 0, 0, 255));
        if (originalPic == null) { return; }

        if (Links.Value != 0) {
            e.FillRectangle(brushBlau, new Rectangle(0, 0, Convert.ToInt32(Links.Value), originalPic.Height));
        }
        if (Recht.Value != 0) {
            e.FillRectangle(brushBlau, new Rectangle(originalPic.Width + Convert.ToInt32(Recht.Value), 0, (int)-Recht.Value, originalPic.Height));
        }
        if (Oben.Value != 0) {
            e.FillRectangle(brushBlau, new Rectangle(0, 0, originalPic.Width, Convert.ToInt32(Oben.Value)));
        }
        if (Unten.Value != 0) {
            e.FillRectangle(brushBlau, new Rectangle(0, originalPic.Height + Convert.ToInt32(Unten.Value), originalPic.Width, (int)-Unten.Value));
        }
    }

    private void ValueChangedByClicking(object sender, System.EventArgs e) => OnDoInvalidate();

    private void WollenSieDenZuschnittÜbernehmen() {
        if (Links.Value <= 0 && Recht.Value >= 0 && Oben.Value <= 0 && Unten.Value >= 0) { return; }
        if (BlueControls.Forms.MessageBox.Show("Soll der <b>aktuelle</b> Zuschnitt<br>übernommen werden?", ImageCode.Zuschneiden, "Ja", "Nein") == 1) { return; }
        ZuschnittOK_Click(null, System.EventArgs.Empty);
    }

    private void ZuschnittOK_Click(object? sender, System.EventArgs? e) {
        var pic = OnNeedCurrentPic();
        var bmp2 = Crop(pic, (int)Links.Value, (int)Recht.Value, (int)Oben.Value, (int)Unten.Value);
        OnOverridePic(bmp2, true);
        Links.Value = 0;
        Recht.Value = 0;
        Oben.Value = 0;
        Unten.Value = 0;
        CollectGarbage();
        CheckMinMax();
        OnZoomFit();
    }

    #endregion
}