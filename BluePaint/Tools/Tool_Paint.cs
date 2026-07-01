// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Windows.Forms;

namespace BluePaint;

public partial class Tool_Paint {

    #region Fields

    private static readonly Brush BrushRedSemiTransp = new SolidBrush(Color.FromArgb(50, 255, 0, 0));

    private int _brushSize = 3;

    #endregion

    #region Constructors

    public Tool_Paint() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent is null) { return; }
        DrawPixelExactCircle(gr, zoom, offsetX, offsetY, mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY, _brushSize - 1, BrushRedSemiTransp);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            var pic = OnNeedCurrentPic();
            if (pic is null) { return; }
            pic.FillCircle(Color.Black, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, _brushSize - 1);
            OnDoInvalidate();
        } else {
            OnDoInvalidate();
        }
    }

    private void sldSize_ValueChanged(object sender, System.EventArgs e) {
        _brushSize = (int)Math.Round(sldSize.Value, MidpointRounding.AwayFromZero);
        capSize.Text = _brushSize.ToString1();
        OnDoInvalidate();
    }

    #endregion
}