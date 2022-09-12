// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.Converter;

#nullable enable

namespace BluePaint;

public partial class Tool_Kontrast : GenericTool //System.Windows.Forms.UserControl //
{
    #region Constructors

    public Tool_Kontrast() : base() {
        InitializeComponent();
    }

    #endregion

    #region Methods

    public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) {
        if (originalPic == null) { return; }
        if (sldKontrast.Value != 0) {
            var picPreview = AdjustContrast(originalPic, sldKontrast.Value);
            e.DrawImage(picPreview);
            return;
        }
        if (sldGamma.Value != 1) {
            var picPreview = AdjustGamma(originalPic, sldGamma.Value);
            e.DrawImage(picPreview);
            return;
        }
        if (sldHelligkeit.Value != 1) {
            var picPreview = AdjustBrightness(originalPic, sldHelligkeit.Value);
            e.DrawImage(picPreview);
            return;
        }
    }

    public override void ExcuteCommand(string command) {
        var c = command.SplitAndCutBy(";");
        switch (c[0]) {
            case "Kontrast":
                sldKontrast.Value = FloatParse(c[1]);
                btnKontrastErhoehen_Click(null, System.EventArgs.Empty);
                break;

            case "Graustufen":
                btnGraustufen_Click(null, System.EventArgs.Empty);
                break;

            case "AlleFarbenSchwarz":
                btnAlleFarbenSchwarz_Click(null, System.EventArgs.Empty);
                break;

            case "PixelHinzu":
                btnPixelHinzu_Click(null, System.EventArgs.Empty);
                break;

            case "Ausdünnen":
                btnAusdünnen_Click(null, System.EventArgs.Empty);
                break;

            case "Gamma":
                sldGamma.Value = FloatParse(c[1]);
                btnGamma_Click(null, System.EventArgs.Empty);
                break;

            case "Helligkeit":
                sldHelligkeit.Value = FloatParse(c[1]);
                btnHelligkeit_Click(null, System.EventArgs.Empty);
                break;

            default:
                Develop.DebugPrint_NichtImplementiert();
                break;
        }
        //if (c[0] == "Replace")
        //{
        //    var OriginalPic = OnNeedCurrentPic();
        //    var cc = Color.FromArgb(IntParse(c[1]));
        //    OnOverridePic(OriginalPic.ReplaceColor(cc, Color.Transparent));
        //}
        //else
        //{
        //    Develop.DebugPrint_NichtImplementiert();
        //}
    }

    private void btnAlleFarbenSchwarz_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnForceUndoSaving();
        AllePixelZuSchwarz(pic, 1f);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        OnDoInvalidate();
    }

    private void btnAusdünnen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnForceUndoSaving();
        Ausdünnen(pic, 4);
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        OnDoInvalidate();
    }

    private void btnGamma_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnOverridePic(AdjustGamma(pic, sldGamma.Value));
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnGraustufen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnOverridePic(Grayscale(pic));
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnHelligkeit_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnOverridePic(AdjustBrightness(pic, sldHelligkeit.Value));
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

    private void btnKontrastErhoehen_Click(object? sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        OnOverridePic(AdjustContrast(pic, sldKontrast.Value));
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
    }

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

    private void sldGamma_ValueChanged(object sender, System.EventArgs e) {
        //sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capGamma.Text = sldGamma.Value.ToString(Constants.Format_Float1);
        OnDoInvalidate();
    }

    private void sldHelligkeit_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        sldKontrast.Value = 0f;
        //sldHelligkeit.Value = 0f;
        capHelligkeit.Text = sldHelligkeit.Value.ToString(Constants.Format_Float1);
        OnDoInvalidate();
    }

    private void sldKontrast_ValueChanged(object sender, System.EventArgs e) {
        sldGamma.Value = 1f;
        //sldKontrast.Value = 0f;
        sldHelligkeit.Value = 1f;
        capKontrast.Text = sldKontrast.Value.ToString(Constants.Format_Float1);
        OnDoInvalidate();
    }

    #endregion
}