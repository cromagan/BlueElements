// Authors:
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

using BlueBasics;
using BlueControls.EventArgs;
using System;
using System.Drawing;

namespace BluePaint;

public partial class Tool_Kontrast : GenericTool //System.Windows.Forms.UserControl //
{
    #region Constructors

    public Tool_Kontrast() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        if (originalPic?.IsValid() != true) { return; }

        using var picPreview = new BitmapExt(originalPic);

        if (sldKontrast.Value != 0) { picPreview.ApplyFilter("Contrast", sldKontrast.Value); }
        if (Math.Abs(sldGamma.Value - 1) > 0.001) { picPreview.ApplyFilter("Gamma", sldGamma.Value); }
        if (Math.Abs(sldHelligkeit.Value - 1) > 0.001) { picPreview.ApplyFilter("Brightness", sldHelligkeit.Value); }

        e.DrawImage(picPreview.CloneOfBitmap());
    }

    private void btnAlleFarbenSchwarz_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = new BitmapExt(pic);

        picPreview.ApplyFilter("AllePixelZuSchwarz", 0.95f);

        OnOverridePic(picPreview.CloneOfBitmap(), false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnAusdünnen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = new BitmapExt(pic);

        picPreview.ApplyFilter("Ausdünnen", 4);

        OnOverridePic(picPreview.CloneOfBitmap(), false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnGamma_Click(object? sender, System.EventArgs e) => DoPic();

    private void btnGraustufen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = new BitmapExt(pic);

        picPreview.ApplyFilter("Grayscale");

        OnOverridePic(picPreview.CloneOfBitmap(), false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnHelligkeit_Click(object? sender, System.EventArgs e) => DoPic();

    private void btnKontrastErhoehen_Click(object? sender, System.EventArgs e) => DoPic();

    private void btnPixelHinzu_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnForceUndoSaving();
        for (var x = 0; x < pic.Width - 1; x++) {
            for (var y = 0; y < pic.Height - 1; y++) {
                if (!pic.GetPixel(x + 1, y + 1).IsNearWhite(0.9)) { pic.SetPixel(x, y, Color.Black); }
                if (!pic.GetPixel(x + 1, y).IsNearWhite(0.9)) { pic.SetPixel(x, y, Color.Black); }
                if (!pic.GetPixel(x, y + 1).IsNearWhite(0.9)) { pic.SetPixel(x, y, Color.Black); }
            }
        }
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        OnDoInvalidate();
    }

    private void DoPic() {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }

        var picPreview = new BitmapExt(pic);

        if (sldKontrast.Value != 0) { picPreview.ApplyFilter("Contrast", sldKontrast.Value); }
        if (Math.Abs(sldGamma.Value - 1) > 0.001) { picPreview.ApplyFilter("Gamma", sldGamma.Value); }
        if (Math.Abs(sldHelligkeit.Value - 1) > 0.001) { picPreview.ApplyFilter("Brightness", sldHelligkeit.Value); }

        OnOverridePic(picPreview.CloneOfBitmap(), false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void sldGamma_ValueChanged(object sender, System.EventArgs e) {
        //sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capGamma.Text = sldGamma.Value.ToStringFloat2();
        OnDoInvalidate();
    }

    private void sldHelligkeit_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        //sldHelligkeit.Value = 0f;
        capHelligkeit.Text = sldHelligkeit.Value.ToStringFloat2();
        OnDoInvalidate();
    }

    private void sldKontrast_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        //sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capKontrast.Text = sldKontrast.Value.ToStringFloat2();
        OnDoInvalidate();
    }

    #endregion
}