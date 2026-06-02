// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Generic;

namespace BluePaint;

public partial class Tool_Clipping {

    #region Fields

    private static readonly Pen PenBlau = new Pen(Color.FromArgb(150, Color.Blue));
    private static readonly Brush BrushBlau = new SolidBrush(Color.FromArgb(120, Color.Blue));

    #endregion

    #region Constructors

    public Tool_Clipping() : base() {
        InitializeComponent();
    }

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        var pic = OnNeedCurrentPic();
        if (pic == null || mouseCurrent == null) { return; }
        DrawZusatz(gr, zoom, offsetX, offsetY, pic);
        gr.DrawLine(PenBlau, mouseCurrent.TrimmedCanvasX.CanvasToControl(zoom, offsetX), -1.CanvasToControl(zoom, offsetY), mouseCurrent.TrimmedCanvasX.CanvasToControl(zoom, offsetX), pic.Height.CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenBlau, -1.CanvasToControl(zoom, offsetX), mouseCurrent.TrimmedCanvasY.CanvasToControl(zoom, offsetY), pic.Width.CanvasToControl(zoom, offsetX), mouseCurrent.TrimmedCanvasY.CanvasToControl(zoom, offsetY));

        if (mouseCurrent.Button == MouseButtons.Left && mouseDown != null) {
            gr.DrawLine(PenBlau, mouseDown.TrimmedCanvasX.CanvasToControl(zoom, offsetX), -1.CanvasToControl(zoom, offsetY), mouseDown.TrimmedCanvasX.CanvasToControl(zoom, offsetX), pic.Height.CanvasToControl(zoom, offsetY));
            gr.DrawLine(PenBlau, -1.CanvasToControl(zoom, offsetX), mouseDown.TrimmedCanvasY.CanvasToControl(zoom, offsetY), pic.Width.CanvasToControl(zoom, offsetX), mouseDown.TrimmedCanvasY.CanvasToControl(zoom, offsetY));
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

    public override void ToolFirstShown() {
        CheckMinMax();
        btnAutoZ_Click(this, System.EventArgs.Empty);
        ZuschnittOK_Click(this, System.EventArgs.Empty);
    }

    private void btnAutoZ_Click(object sender, System.EventArgs? e) {
        WollenSieDenZuschnittÜbernehmen();
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }

        OnZoomFit();
        var pa = pic.GetAutoValuesForCrop(0.9);
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
        Unten.Minimum = -pic.Height + 1;
    }

    private void DrawZusatz(Graphics gr, float zoom, int offsetX, int offsetY, Image? originalPic) {
        if (originalPic == null) { return; }

        if (Links.Value != 0) {
            var r = new RectangleF(0, 0, Convert.ToInt32(Links.Value), originalPic.Height).CanvasToControl(zoom, offsetX, offsetY, true);
            gr.FillRectangle(BrushBlau, r);
        }
        if (Recht.Value != 0) {
            var r = new RectangleF(originalPic.Width + Convert.ToInt32(Recht.Value), 0, (int)-Recht.Value, originalPic.Height).CanvasToControl(zoom, offsetX, offsetY, true);
            gr.FillRectangle(BrushBlau, r);
        }
        if (Oben.Value != 0) {
            var r = new RectangleF(0, 0, originalPic.Width, Convert.ToInt32(Oben.Value)).CanvasToControl(zoom, offsetX, offsetY, true);
            gr.FillRectangle(BrushBlau, r);
        }
        if (Unten.Value != 0) {
            var r = new RectangleF(0, originalPic.Height + Convert.ToInt32(Unten.Value), originalPic.Width, (int)-Unten.Value).CanvasToControl(zoom, offsetX, offsetY, true);
            gr.FillRectangle(BrushBlau, r);
        }
    }

    private void ValueChangedByClicking(object sender, System.EventArgs e) => OnDoInvalidate();

    private void WollenSieDenZuschnittÜbernehmen() {
        if (Links.Value <= 0 && Recht.Value >= 0 && Oben.Value <= 0 && Unten.Value >= 0) { return; }
        if (BlueControls.Forms.MessageBox.Show("Soll der <b>aktuelle</b> Zuschnitt<br>übernommen werden?", ImageCode.Zuschneiden, "Ja", "Nein") == 1) { return; }
        ZuschnittOK_Click(this, System.EventArgs.Empty);
    }

    private void ZuschnittOK_Click(object sender, System.EventArgs? e) {
        var pic = OnNeedCurrentPic();
        var bmp2 = CropStatic(pic, (int)Links.Value, (int)Recht.Value, (int)Oben.Value, (int)Unten.Value);
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
