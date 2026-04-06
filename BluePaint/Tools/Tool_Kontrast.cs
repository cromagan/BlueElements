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

using BlueBasics;
using BlueBasics.Classes.BitmapExt_ImageFilters;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BluePaint;

public partial class Tool_Kontrast : GenericTool //System.Windows.Forms.UserControl //
{
    #region Constructors

    public Tool_Kontrast() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawingEventArgs e, Bitmap? originalPic) {
        if (originalPic?.IsValid() != true) { return; }

        using var picPreview = originalPic.CloneFromBitmap();

        var filters = new List<(ImageFilter filter, object? parameter)>();
        if (sldKontrast.Value != 0) { filters.Add((ImageFilter.AllFilters.GetByKey("Contrast")!, sldKontrast.Value)); }
        if (Math.Abs(sldGamma.Value - 1) > 0.001) { filters.Add((ImageFilter.AllFilters.GetByKey("Gamma")!, sldGamma.Value)); }
        if (Math.Abs(sldHelligkeit.Value - 1) > 0.001) { filters.Add((ImageFilter.AllFilters.GetByKey("Brightness")!, sldHelligkeit.Value)); }
        picPreview.ApplyFilter(filters.ToArray());

        e.DrawImage(picPreview);
    }

    private void btnAlleFarbenSchwarz_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = pic.CloneFromBitmap();

        picPreview.ApplyFilter((ImageFilter.AllFilters.GetByKey("AllePixelZuSchwarz")!, 0.95f));

        OnOverridePic(picPreview, false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnAusdünnen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = pic.CloneFromBitmap();

        picPreview.ApplyFilter((ImageFilter.AllFilters.GetByKey("Ausdünnen")!, 4));
        OnOverridePic(picPreview, false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnGamma_Click(object? sender, System.EventArgs e) => DoPic();

    private void btnGraustufen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var picPreview = pic.CloneFromBitmap();

        picPreview.ApplyFilter(ImageFilter.AllFilters.GetByKey("Grayscale")!);

        OnOverridePic(picPreview, false);
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

        var picPreview = pic.CloneFromBitmap();

        var filters = new List<(ImageFilter filter, object? parameter)>();
        if (sldKontrast.Value != 0) { filters.Add((ImageFilter.AllFilters.GetByKey("Contrast")!, sldKontrast.Value)); }
        if (Math.Abs(sldGamma.Value - 1) > 0.001) { filters.Add((ImageFilter.AllFilters.GetByKey("Gamma")!, sldGamma.Value)); }
        if (Math.Abs(sldHelligkeit.Value - 1) > 0.001) { filters.Add((ImageFilter.AllFilters.GetByKey("Brightness")!, sldHelligkeit.Value)); }
        picPreview.ApplyFilter(filters.ToArray());

        OnOverridePic(picPreview, false);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void sldGamma_ValueChanged(object sender, System.EventArgs e) {
        //sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capGamma.Text = sldGamma.Value.ToString1_2();
        OnDoInvalidate();
    }

    private void sldHelligkeit_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        //sldHelligkeit.Value = 0f;
        capHelligkeit.Text = sldHelligkeit.Value.ToString1_2();
        OnDoInvalidate();
    }

    private void sldKontrast_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        //sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capKontrast.Text = sldKontrast.Value.ToString1_2();
        OnDoInvalidate();
    }

    #endregion
}