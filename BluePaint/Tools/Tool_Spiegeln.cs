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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Generic;
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

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        if (OnNeedCurrentPic() is not { } pic || e.MouseCurrent == null || e.MouseDown == null) { return; }

        e.DrawLine(PenRedTransp, -1, e.MouseCurrent.TrimmedCanvasY, pic.Width, e.MouseCurrent.TrimmedCanvasY);
        e.DrawLine(PenRedTransp, e.MouseCurrent.TrimmedCanvasX, -1, e.MouseCurrent.TrimmedCanvasX, pic.Height);
        if (e.MouseCurrent.Button != MouseButtons.Left) {
            return;
        }

        e.DrawLine(PenRedTransp, -1, e.MouseDown.TrimmedCanvasY, pic.Width, e.MouseDown.TrimmedCanvasY);
        e.DrawLine(PenRedTransp, e.MouseDown.TrimmedCanvasX, -1, e.MouseDown.TrimmedCanvasX, pic.Height);
        e.DrawLine(PenLightWhite, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasX, e.MouseDown.TrimmedCanvasY);
        e.DrawLine(PenRedTransp, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY, e.MouseDown.TrimmedCanvasX, e.MouseDown.TrimmedCanvasY);
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

        if (pic == null) { return; }

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
        using (var gr = Graphics.FromImage(bmp)) {
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.Clear(Color.Magenta);
            gr.Transform = rotateAtCenter;
            // Draw the image centered on the bitmap.
            var x = (b - pic.Width) / 2;
            var y = (h - pic.Height) / 2;
            gr.DrawImage(pic, x, y, pic.Width, pic.Height);
        }
        OnOverridePic(bmp, true);
    }

    private void btnAusrichten_Click(object sender, System.EventArgs e) {
        _ausricht = true;
        OnDoInvalidate();
    }

    private void btnDrehenL_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.Rotate270FlipNone);

    private void btnDrehenR_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.Rotate90FlipNone);

    private void DoThis(RotateFlipType b) {
        _ausricht = false;
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        CollectGarbage();
        try {
            var clonedBitmap = new Bitmap(pic);
            clonedBitmap.RotateFlip(b);
            OnOverridePic(clonedBitmap, true);
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, "Spiegeln/Drehen fehlgeschlagen", ex);
            Notification.Show("Befehl konnte nicht\r\nausgeführt werden.");
        }

        OnZoomFit();
    }

    private void SpiegelnH_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.RotateNoneFlipY);

    private void SpiegelnV_Click(object sender, System.EventArgs e) => DoThis(RotateFlipType.RotateNoneFlipX);

    #endregion
}