﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Enums;
using BlueControls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static BlueBasics.Generic;
using static BlueBasics.Geometry;

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

    public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        if (OnNeedCurrentPic() is not { } pic || e.Current == null || e.MouseDown == null) { return; }

        e.DrawLine(PenRedTransp, -1, e.Current.TrimmedY, pic.Width, e.Current.TrimmedY);
        e.DrawLine(PenRedTransp, e.Current.TrimmedX, -1, e.Current.TrimmedX, pic.Height);
        if (e.Current.Button != MouseButtons.Left) {
            return;
        }

        e.DrawLine(PenRedTransp, -1, e.MouseDown.TrimmedY, pic.Width, e.MouseDown.TrimmedY);
        e.DrawLine(PenRedTransp, e.MouseDown.TrimmedX, -1, e.MouseDown.TrimmedX, pic.Height);
        e.DrawLine(PenLightWhite, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);
        e.DrawLine(PenRedTransp, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);
    }

    public override void MouseDown(MouseEventArgs1_1 e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), originalPic);
    }

    public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        OnDoInvalidate();
    }

    public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
        if (!_ausricht) { return; }
        _ausricht = false;
        CollectGarbage();
        var pic = OnNeedCurrentPic();

        if (pic == null) { return; }

        var wink = GetAngle(new PointM(e.MouseDown.X, e.MouseDown.Y), new PointM(e.Current.X, e.Current.Y));
        // Make a Matrix to represent rotation by this angle.
        Matrix rotateAtOrigin = new();
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
        Bitmap nBmp = new(b, h);
        // Create the real rotation transformation.
        Matrix rotateAtCenter = new();
        rotateAtCenter.RotateAt(wink, new PointF(b / 2f, h / 2f));
        // Draw the image onto the new bitmap rotated.
        using (var gr = Graphics.FromImage(nBmp)) {
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.Clear(Color.Magenta);
            gr.Transform = rotateAtCenter;
            // Draw the image centered on the bitmap.
            var x = (b - pic.Width) / 2;
            var y = (h - pic.Height) / 2;
            gr.DrawImage(pic, x, y, pic.Width, pic.Height);
        }
        OnOverridePic(nBmp, true);
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