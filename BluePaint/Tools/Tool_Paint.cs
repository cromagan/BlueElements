// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Windows.Forms;

namespace BluePaint;

public partial class Tool_Paint {

    #region Constructors

    public Tool_Paint() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        var c = Color.FromArgb(50, 255, 0, 0);
        if (e.MouseCurrent == null) { return; }
        e.FillCircle(c, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 2);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        OnForceUndoSaving();
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (e.MouseCurrent.Button == MouseButtons.Left) {
            var pic = OnNeedCurrentPic();
            if (pic == null) { return; }
            pic.FillCircle(Color.Black, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, 2);
            OnDoInvalidate();
        } else {
            OnDoInvalidate();
        }
    }

    #endregion
}