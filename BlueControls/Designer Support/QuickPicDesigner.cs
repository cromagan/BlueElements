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
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Designer_Support;

public partial class QuickPicDesigner : Panel {

    #region Constructors

    public QuickPicDesigner() => InitializeComponent();

    #endregion

    #region Methods

    public void GeneratePreview() {
        try {
            picPreview.Image = QuickImage.Get(ImgCode());
        } catch {
            picPreview.Image = null;
        }
    }

    public string ImgCode() {
        var e = (ImageCodeEffect)(((chkbGrauStufen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Graustufen) | ((chkbDurchgestrichen.Checked ? -1 : 0) * -(int)ImageCodeEffect.Durchgestrichen) | ((chkbMEDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsMEDisabled) | ((chkbXPDisabled.Checked ? -1 : 0) * -(int)ImageCodeEffect.WindowsXPDisabled));
        return QuickImage.GenerateCode(txbName.Text, IntParse(txbWidth.Text), IntParse(txbHeight.Text), e, ColorParse(txbFaerbung.Text), ColorParse(txbChangeGreen.Text), sldSat.Value, satLum.Value, 0, satTransparenz.Value, txbZweitsymbol.Text);
    }

    public void StartAll(string code) {
        lstNames.ItemClear();
        var im = QuickImage.Images();

        foreach (var thisIm in im) {
            lstNames.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }

        var l = new QuickImage(code);
        txbName.Text = l.Name;
        txbFaerbung.Text = l.Färbung.ToHtmlCode();
        txbChangeGreen.Text = l.ChangeGreenTo.ToHtmlCode();
        chkbGrauStufen.Checked = l.Effekt.HasFlag(ImageCodeEffect.Graustufen);
        sldSat.Value = l.Sättigung;
        satLum.Value = l.Helligkeit;
        satTransparenz.Value = l.Transparenz;
        //if (l.Effekt < 0) { l.Effekt =  ImageCodeEffect.Ohne; }
        chkbDurchgestrichen.Checked = l.Effekt.HasFlag(ImageCodeEffect.Durchgestrichen);
        chkbMEDisabled.Checked = l.Effekt.HasFlag(ImageCodeEffect.WindowsMEDisabled);
        chkbXPDisabled.Checked = l.Effekt.HasFlag(ImageCodeEffect.WindowsXPDisabled);
        txbWidth.Text = l.Width.ToString1();
        txbHeight.Text = l.Height.ToString1();
        txbZweitsymbol.Text = l.Zweitsymbol;
    }

    private void LstNames_ItemClicked(object sender, AbstractListItemEventArgs e) => txbName.Text = e.Item.KeyName;

    private void SomethingChanged(object sender, System.EventArgs e) {
        Helll.Text = satLum.Value + "%";
        SATL.Text = sldSat.Value + "%";
        Transpl.Text = satTransparenz.Value + "%";
        GeneratePreview();
    }

    #endregion
}