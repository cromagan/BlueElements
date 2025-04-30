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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueControls.ItemCollectionList;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FontEditor : EditorEasy {

    #region Fields

    private static List<AbstractListItem>? _fnList;

    private static List<AbstractListItem>? _fsList;

    #endregion

    #region Constructors

    public FontEditor() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void Clear() {
        lstName.UncheckAll();
        listSize.UncheckAll();
        chkFett.Checked = false;
        chkKursiv.Checked = false;
        chkUnterstrichen.Checked = false;
        chkKap.Checked = false;
        chkDurchgestrichen.Checked = false;
        chkOutline.Checked = false;
        chkOnlyLow.Checked = false;
        chkOnlyUpper.Checked = false;
        btnFontColor.ImageCode = string.Empty;
        btnOutlineColor.ImageCode = string.Empty;
        preview.Image = null;
    }

    protected override void InitializeComponentDefaultValues() {
        if (_fnList == null) {
            _fnList = [];
            foreach (var f in FontFamily.Families) {
                if (string.IsNullOrEmpty(f.Name)) { continue; }

                if (f.IsStyleAvailable(FontStyle.Regular)) {
                    Font fo = new(f.Name, 100);
                    try {
                        _ = fo.MeasureString("T");
                        _fnList.Add(ItemOf(string.Empty, f.Name, BlueFont.Get(f, 12).NameInStyle(), true));
                    } catch { }
                }
            }

            _fsList = [
                ItemOf("8", SortierTyp.ZahlenwertFloat),
                ItemOf("9", SortierTyp.ZahlenwertFloat),
                ItemOf("10", SortierTyp.ZahlenwertFloat),
                ItemOf("11", SortierTyp.ZahlenwertFloat),
                ItemOf("12", SortierTyp.ZahlenwertFloat),
                ItemOf("14", SortierTyp.ZahlenwertFloat),
                ItemOf("16", SortierTyp.ZahlenwertFloat),
                ItemOf("18", SortierTyp.ZahlenwertFloat),
                ItemOf("20", SortierTyp.ZahlenwertFloat),
                ItemOf("22", SortierTyp.ZahlenwertFloat),
                ItemOf("24", SortierTyp.ZahlenwertFloat),
                ItemOf("26", SortierTyp.ZahlenwertFloat),
                ItemOf("28", SortierTyp.ZahlenwertFloat),
                ItemOf("36", SortierTyp.ZahlenwertFloat),
                ItemOf("48", SortierTyp.ZahlenwertFloat),
                ItemOf("72", SortierTyp.ZahlenwertFloat)
            ];
        }

        lstName.ItemAddRange(_fnList);
        listSize.ItemAddRange(_fsList);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        UpdateSampleText();
    }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        if (toEdit is not BlueFont { } bf) { return false; }

        if (lstName[bf.FontName] == null) { lstName.ItemAdd(ItemOf(bf.FontName, bf.FontName, QuickImage.Get(ImageCode.Warnung, 20))); }
        if (listSize[bf.Size.ToStringFloat2()] == null) { listSize.ItemAdd(ItemOf(bf.Size.ToStringFloat2())); }

        lstName.UncheckAll();
        lstName.Check(bf.FontName);
        listSize.UncheckAll();
        listSize.Check(bf.Size.ToStringFloat2());
        chkFett.Checked = bf.Bold;
        chkKursiv.Checked = bf.Italic;
        chkUnterstrichen.Checked = bf.Underline;
        chkDurchgestrichen.Checked = bf.StrikeOut;
        chkOutline.Checked = bf.ColorOutline.A > 0;
        btnFontColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, bf.ColorMain).Code;
        btnOutlineColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, bf.ColorOutline).Code;
        chkKap.Checked = bf.Kapitälchen;
        chkOnlyLow.Checked = bf.OnlyLower;
        chkOnlyUpper.Checked = bf.OnlyUpper;

        return true;
    }

    private void btnBackColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
        ChangeFont();
    }

    private void cFarbe_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnFontColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnFontColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
        ChangeFont();
    }

    private void ChangeFont() {
        ToEdit = BlueFont.Get(lstName.Checked[0],
                              FloatParse(listSize.Checked[0]),
                              chkFett.Checked,
                              chkKursiv.Checked,
                              chkUnterstrichen.Checked,
                              chkDurchgestrichen.Checked,
                              QuickImage.Get(btnFontColor.ImageCode).ChangeGreenTo,
                              QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo,
                              chkKap.Checked,
                              chkOnlyUpper.Checked,
                              chkOnlyLow.Checked,
                              QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo);

        UpdateSampleText();
    }

    private void cRandF_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnOutlineColor.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        btnOutlineColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
        ChangeFont();
    }

    private void FName_Item_CheckedChanged(object sender, System.EventArgs e) => ChangeFont();

    private void style_CheckedChanged(object sender, System.EventArgs e) => ChangeFont();

    private void UpdateSampleText() {
        if (ToEdit is not BlueFont { } bf) { return; }

        preview.Image = bf.SampleText()?.CloneFromBitmap();
    }

    #endregion
}