// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.Designer_Support;

public partial class QuickPicDesigner : UserControl {

    #region Constructors

    public QuickPicDesigner() {
        InitializeComponent();
    }

    #endregion

    #region Methods

    public void GeneratePreview() {
        try {
            Preview.Image = QuickImage.Get(ImgCode());
        } catch {
            Preview.Image = null;
        }
    }

    public string ImgCode() {
        var e = (ImageCodeEffect)(((chkbGrauStufen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Graustufen) | ((chkbDurchgestrichen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Durchgestrichen) | ((chkbMEDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsMEDisabled) | ((chkbXPDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsXPDisabled));
        return QuickImage.GenerateCode(PicName.Text, IntParse(GrX.Text), IntParse(GrY.Text), e, Färb.Text, grün.Text, SAT.Value, Hell.Value, 0, Transp.Value, txbZweitsymbol.Text);
    }

    public void StartAll(string code) {
        LB.Items.Clear();
        var im = QuickImage.Images();

        foreach (var thisIm in im) {
            _ = LB.Items.Add(thisIm);
        }

        QuickImage l = new(code);
        PicName.Text = l.Name;
        Färb.Text = l.Färbung;
        grün.Text = l.ChangeGreenTo;
        chkbGrauStufen.Checked = l.Effekt.HasFlag(ImageCodeEffect.Graustufen);
        SAT.Value = l.Sättigung;
        Hell.Value = l.Helligkeit;
        Transp.Value = l.Transparenz;
        //if (l.Effekt < 0) { l.Effekt =  ImageCodeEffect.Ohne; }
        chkbDurchgestrichen.Checked = l.Effekt.HasFlag(ImageCodeEffect.Durchgestrichen);
        chkbMEDisabled.Checked = l.Effekt.HasFlag(ImageCodeEffect.WindowsMEDisabled);
        chkbXPDisabled.Checked = l.Effekt.HasFlag(ImageCodeEffect.WindowsXPDisabled);
        GrX.Text = l.Width.ToString();
        GrY.Text = l.Height.ToString();
        txbZweitsymbol.Text = l.Zweitsymbol;
    }

    private static void SomethingCheckedChanged(object sender, System.EventArgs e) { }

    private void LB_DoubleClick(object sender, System.EventArgs e) => PicName.Text = Convert.ToString(LB.SelectedItem);

    private void SomethingChanged(object sender, System.EventArgs e) {
        Helll.Text = Hell.Value + "%";
        SATL.Text = SAT.Value + "%";
        Transpl.Text = Transp.Value + "%";
        GeneratePreview();
    }

    #endregion
}