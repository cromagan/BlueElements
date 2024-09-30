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
using System.Collections.Generic;
using System.Drawing;
using BlueControls.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FontSelectDialog {

    #region Fields

    private static List<AbstractListItem>? _fnList;
    private static List<AbstractListItem>? _fsList;
    private bool _adding;

    #endregion

    #region Constructors

    public FontSelectDialog() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        if (_fnList == null) {
            _fnList = new List<AbstractListItem>();
            foreach (var f in FontFamily.Families) {
                if (string.IsNullOrEmpty(f.Name)) {
                    continue;
                }

                if (f.IsStyleAvailable(FontStyle.Regular)) {
                    Font fo = new(f.Name, 100);
                    try {
                        _ = fo.MeasureString("T");
                        _fnList.Add(ItemOf(string.Empty, f.Name, BlueFont.Get(f, 12).NameInStyle(), true));
                    } catch (Exception) { }
                }
            }
            //_fnList.Sort();
            _fsList = new List<AbstractListItem>();
            _fsList.Add(ItemOf("8", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("9", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("10", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("11", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("12", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("14", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("16", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("18", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("20", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("22", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("24", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("26", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("28", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("36", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("48", SortierTyp.ZahlenwertFloat));
            _fsList.Add(ItemOf("72", SortierTyp.ZahlenwertFloat));
            //_fsList.Sort();
        }
        FName.ItemAddRange(_fnList);
        //FName.Item.Sort();
        FSize.ItemAddRange(_fsList);
        //FSize.Item.Sort();
        Font = BlueFont.Get(Skin.DummyStandardFont); //, False, False, False, False, False, "000000", string.Empty, False)
        UpdateSampleText();
    }

    #endregion

    #region Properties

    public new BlueFont? Font {
        get => BlueFont.Get(FName.Checked[0], FloatParse(FSize.Checked[0]), fFett.Checked, fKursiv.Checked, fUnterstrichen.Checked, fDurchge.Checked, fOutline.Checked, QuickImage.Get(cFarbe.ImageCode).ChangeGreenTo, QuickImage.Get(cRandF.ImageCode).ChangeGreenTo, fKap.Checked, OnlyUpper.Checked, OnlyLow.Checked);
        set {
            _adding = true;
            value ??= BlueFont.Get(Skin.DummyStandardFont);
            if (FName[value.FontName] == null) { FName.ItemAdd(ItemOf(value.FontName, value.FontName, QuickImage.Get(ImageCode.Warnung, 20))); }
            FName.UncheckAll();
            FName.Check(value.FontName);
            if (FSize[value.Size.ToStringFloat2()] == null) { FSize.ItemAdd(ItemOf(value.Size.ToStringFloat2())); }
            FSize.UncheckAll();
            FSize.Check(value.Size.ToStringFloat2());
            fFett.Checked = value.Bold;
            fKursiv.Checked = value.Italic;
            fUnterstrichen.Checked = value.Underline;
            fDurchge.Checked = value.StrikeOut;
            fOutline.Checked = value.Outline;
            cFarbe.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, value.ColorMain).Code;
            cRandF.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, value.ColorOutline).Code;
            fKap.Checked = value.Kapitälchen;
            OnlyLow.Checked = value.OnlyLower;
            OnlyUpper.Checked = value.OnlyUpper;
            _adding = false;
            UpdateSampleText();
        }
    }

    #endregion

    #region Methods

    private void cFarbe_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(cFarbe.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        cFarbe.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
        UpdateSampleText();
    }

    private void cRandF_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(cRandF.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        cRandF.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).Code;
        UpdateSampleText();
    }

    private void fFett_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void FName_Item_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void Ok_Click(object sender, System.EventArgs e) => Close();

    private void UpdateSampleText() {
        if (_adding) { return; }
        Sample.Image = Font?.SampleText()?.CloneFromBitmap();
    }

    #endregion
}