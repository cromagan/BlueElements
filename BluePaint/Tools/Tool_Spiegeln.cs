// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Geometry;

namespace BluePaint;

public partial class Tool_Spiegeln : GenericTool // System.Windows.Forms.UserControl //
{
    #region Fields

    private bool _ausricht;

    #endregion

    #region Constructors

    public Tool_Spiegeln() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (!_ausricht) { return; }
        if (OnNeedCurrentPic() is not { } pic || mouseCurrent is null) { return; }

        gr.DrawLine(PenRedTransp, -1.CanvasToControl(zoom, offsetX), mouseCurrent.TrimmedCanvasY.CanvasToControl(zoom, offsetY), pic.Width.CanvasToControl(zoom, offsetX), mouseCurrent.TrimmedCanvasY.CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenRedTransp, mouseCurrent.TrimmedCanvasX.CanvasToControl(zoom, offsetX), -1.CanvasToControl(zoom, offsetY), mouseCurrent.TrimmedCanvasX.CanvasToControl(zoom, offsetX), pic.Height.CanvasToControl(zoom, offsetY));
        if (mouseCurrent.Button != MouseButtons.Left || mouseDown is null) {
            return;
        }

        gr.DrawLine(PenRedTransp, -1.CanvasToControl(zoom, offsetX), mouseDown.TrimmedCanvasY.CanvasToControl(zoom, offsetY), pic.Width.CanvasToControl(zoom, offsetX), mouseDown.TrimmedCanvasY.CanvasToControl(zoom, offsetY));
        gr.DrawLine(PenRedTransp, mouseDown.TrimmedCanvasX.CanvasToControl(zoom, offsetX), -1.CanvasToControl(zoom, offsetY), mouseDown.TrimmedCanvasX.CanvasToControl(zoom, offsetX), pic.Height.CanvasToControl(zoom, offsetY));
        var p1 = new PointF(mouseCurrent.TrimmedCanvasX, mouseCurrent.TrimmedCanvasY).CanvasToControl(zoom, offsetX, offsetY);
        var p2 = new PointF(mouseDown.TrimmedCanvasX, mouseDown.TrimmedCanvasY).CanvasToControl(zoom, offsetX, offsetY);
        gr.DrawLine(PenLightWhite, p1, p2);
        gr.DrawLine(PenRedTransp, p1, p2);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        MouseMove(new TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs(e, e), originalPic);
    }

    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        OnDoInvalidate();
    }

    public override void MouseUp(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        _ausricht = false;
        CollectGarbage();
        var pic = OnNeedCurrentPic();

        if (pic is null) { return; }

        var wink = GetAngle(e.MouseDown.CanvasPoint, e.MouseCurrent.CanvasPoint);
        // Make a Matrix to represent rotation by this angle.
        var rotateAtOrigin = new Matrix();
        rotateAtOrigin.Rotate(wink);
        // Rotate the image's corners to see how big it will be after rotation.
        PointF[] p = [
            new(0, 0), new(pic.Width, 0), new(pic.Width, pic.Height),
            new(0, pic.Height)
        ];
        rotateAtOrigin.TransformPoints(p);
        var minX = Math.Min(Math.Min(p[0].X, p[1].X), Math.Min(p[2].X, p[3].X));
        var minY = Math.Min(Math.Min(p[0].Y, p[1].Y), Math.Min(p[2].Y, p[3].Y));
        var maxX = Math.Max(Math.Max(p[0].X, p[1].X), Math.Max(p[2].X, p[3].X));
        var maxY = Math.Max(Math.Max(p[0].Y, p[1].Y), Math.Max(p[2].Y, p[3].Y));
        var b = (int)Math.Round(maxX - minX, MidpointRounding.AwayFromZero);
        var h = (int)Math.Round(maxY - minY, MidpointRounding.AwayFromZero);
        var bmp = new Bitmap(b, h);
        // Create the real rotation transformation.
        var rotateAtCenter = new Matrix();
        rotateAtCenter.RotateAt(wink, new PointF(b / 2f, h / 2f));
        // Draw the image onto the new bitmap rotated.
        using var gr = Graphics.FromImage(bmp);
        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gr.Clear(Color.Magenta);
        gr.Transform = rotateAtCenter;
        // Draw the image centered on the bitmap.
        var x = (b - pic.Width) / 2;
        var y = (h - pic.Height) / 2;
        gr.DrawImageUnscaled(pic, x, y);
        OnOverridePic(bmp, true);
    }

    private void btnAusrichten_Click(object sender, System.EventArgs e) {
        if (OnNeedCurrentPic() is null) { return; }
        _ausricht = true;
        OnDoInvalidate();
        Notification.Show("Auf dem Bild eine Linie entlang der Kante ziehen,<br>die waagerecht oder senkrecht ausgerichtet werden soll.", ImageCode.Information);
    }

    private void btnDrehenL_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.Rotate270FlipNone);

    private void btnDrehenR_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.Rotate90FlipNone);

    private void DoThis(RotateFlipType b) {
        _ausricht = false;
        var pic = OnNeedCurrentPic();
        if (pic is null) { return; }
        CollectGarbage();
        try {
            var clonedBitmap = new Bitmap(pic);
            clonedBitmap.RotateFlip(b);
            OnOverridePic(clonedBitmap, true);
        } catch (Exception ex) {
            Develop.DebugPrint("Spiegeln/Drehen fehlgeschlagen", ex);
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }

        OnZoomFit();
    }

    private void SpiegelnH_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.RotateNoneFlipY);

    private void SpiegelnV_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.RotateNoneFlipX);

    #endregion
}