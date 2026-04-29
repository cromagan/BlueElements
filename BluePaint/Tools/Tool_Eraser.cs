// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Windows.Forms;

namespace BluePaint;

public partial class Tool_Eraser : GenericTool {

    #region Constructors

    public Tool_Eraser() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent == null) { return; }

        if (Razi.Checked) {
            var r = 3 * zoom;
            var p = new PointF(mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY).CanvasToControl(zoom, offsetX, offsetY);
            gr.FillEllipse(new SolidBrush(ColorRedTransp), p.X - r, p.Y - r, r * 2, r * 2);
        }

        if (!DrawBox.Checked || mouseDown == null) {
            return;
        }

        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }

        Point p1, p2;
        if (mouseCurrent.Button == MouseButtons.Left) {
            p1 = new Point(Math.Min(mouseCurrent.TrimmedCanvasX, mouseDown.TrimmedCanvasX), Math.Min(mouseCurrent.TrimmedCanvasY, mouseDown.TrimmedCanvasY));
            p2 = new Point(Math.Max(mouseCurrent.TrimmedCanvasX, mouseDown.TrimmedCanvasX), Math.Max(mouseCurrent.TrimmedCanvasY, mouseDown.TrimmedCanvasY));
            var tr = new RectangleF(Math.Min(mouseCurrent.TrimmedCanvasX, mouseDown.TrimmedCanvasX), Math.Min(mouseCurrent.TrimmedCanvasY, mouseDown.TrimmedCanvasY), Math.Abs(mouseCurrent.TrimmedCanvasX - mouseDown.TrimmedCanvasX) + 1, Math.Abs(mouseCurrent.TrimmedCanvasY - mouseDown.TrimmedCanvasY) + 1);
            var ctrlRect = tr.CanvasToControl(zoom, offsetX, offsetY, true);
            gr.FillRectangle(BrushRedTransp, ctrlRect);
        } else {
            p1 = new Point(mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY);
            p2 = new Point(mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY);
        }
        gr.DrawLine(PenRedTransp, -0.5f.CanvasToControl(zoom, offsetX), (p1.Y - 0.5f).CanvasToControl(zoom, offsetY), (pic.Width + 0.5f).CanvasToControl(zoom, offsetX), (p1.Y - 0.5f).CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenRedTransp, (p1.X - 0.5f).CanvasToControl(zoom, offsetX), -0.5f.CanvasToControl(zoom, offsetY), (p1.X - 0.5f).CanvasToControl(zoom, offsetX), (pic.Height + 0.5f).CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenRedTransp, -0.5f.CanvasToControl(zoom, offsetX), (p2.Y + 0.5f).CanvasToControl(zoom, offsetY), (pic.Width + 0.5f).CanvasToControl(zoom, offsetX), (p2.Y + 0.5f).CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenRedTransp, (p2.X + 0.5f).CanvasToControl(zoom, offsetX), 0.CanvasToControl(zoom, offsetY), (p2.X + 0.5f).CanvasToControl(zoom, offsetX), (pic.Height + 0.5f).CanvasToControl(zoom, offsetY));
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            if (Razi.Checked) {
                var pic = OnNeedCurrentPic();
                if (pic == null) { return; }
                pic.FillCircle(Color.White, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 3);
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
            if (e.MouseCurrent.IsInPic && originalPic != null) {
                var cc = pic.GetPixel((int)e.MouseCurrent.CanvasX, (int)e.MouseCurrent.CanvasY);
                if (cc.ToArgb() == 0) { return; }
                OnOverridePic(originalPic.ReplaceColor(cc, Color.Transparent), false);
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
