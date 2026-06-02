// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Windows.Forms;

namespace BluePaint;

public partial class Tool_Paint {

    #region Fields

    private static readonly Brush BrushRedSemiTransp = new SolidBrush(Color.FromArgb(50, 255, 0, 0));

    #endregion

    #region Constructors

    public Tool_Paint() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent is null) { return; }
        var r = 2 * zoom;
        var p = new PointF(mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY).CanvasToControl(zoom, offsetX, offsetY);
        gr.FillEllipse(BrushRedSemiTransp, p.X - r, p.Y - r, r * 2, r * 2);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            var pic = OnNeedCurrentPic();
            if (pic is null) { return; }
            pic.FillCircle(Color.Black, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 2);
            OnDoInvalidate();
        } else {
            OnDoInvalidate();
        }
    }

    #endregion
}