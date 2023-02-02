// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueBasics.Enums;
using static BlueBasics.Converter;

#nullable enable

namespace BluePaint;

public partial class Tool_Resize : GenericTool //BlueControls.Forms.Form //
{
    #region Constructors

    public Tool_Resize() : base() {
        InitializeComponent();
        capInfo.Text = "Bitte Skalierung in Prozent eingeben";
        flxProzent.ValueSet("100", true, false);
    }

    #endregion

    #region Methods

    public override void ExcuteCommand(string command) {
        var c = command.SplitAndCutBy(";");
        if (c[0] == "ResizeProzent") {
            flxProzent.ValueSet(c[1], true, true);
            btnDoResize_Click(null, null);
        } else {
            Develop.DebugPrint_NichtImplementiert();
        }
    }

    internal override void PictureChangedByMainWindow() {
        base.PictureChangedByMainWindow();
        DoCapInfo();
    }

    private void btnDoResize_Click(object? sender, System.EventArgs? e) {
        var p = OnNeedCurrentPic();
        if (p == null) { return; }
        if (!DoubleTryParse(flxProzent.Value, out var pr)) { return; }
        pr /= 100;
        var wi = (int)(p.Width * pr);
        var he = (int)(p.Height * pr);
        if (pr is 1 or < 0.01 or > 1000 || wi < 1 || he < 1) { return; }
        var bmp2 = BitmapExt.Resize(p, wi, he, SizeModes.Verzerren, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic, true);
        OnOverridePic(bmp2);
        OnZoomFit();
        DoCapInfo();
    }

    private void DoCapInfo() {
        var p = OnNeedCurrentPic();
        if (p == null) {
            capInfo.Text = "Kein Bild gewählt.";
            return;
        }
        if (!DoubleTryParse(flxProzent.Value, out var pr)) {
            capInfo.Text = "Keine Prozentzahl angegeben.";
            return;
        }
        pr /= 100;
        var wi = (int)(p.Width * pr);
        var he = (int)(p.Height * pr);
        if (pr is 1 or < 0.01 or > 1000 || wi < 1 || he < 1) {
            capInfo.Text = "Bitte gültigen Wert angeben.";
            return;
        }
        capInfo.Translate = false;
        capInfo.Text = "Zielgröße: " + (int)(p.Width * pr) + " x " + (int)(p.Height * pr) + " Pixel";
    }

    private void flxProzent_ValueChanged(object sender, System.EventArgs e) => DoCapInfo();

    #endregion
}