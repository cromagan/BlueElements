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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using System;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.Forms;

public partial class FontSelectDialog {

    #region Fields

    private static ItemCollectionList? _fnList;
    private static ItemCollectionList? _fsList;
    private bool _adding;

    #endregion

    #region Constructors

    public FontSelectDialog() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        if (_fnList == null) {
            _fnList = new ItemCollectionList(true);
            foreach (var f in FontFamily.Families) {
                if (string.IsNullOrEmpty(f.Name)) {
                    continue;
                }

                if (f.IsStyleAvailable(FontStyle.Regular)) {
                    Font fo = new(f.Name, 100);
                    try {
                        _ = BlueFont.MeasureString("T", fo);
                        _ = _fnList.Add(string.Empty, f.Name, BlueFont.Get(f, 12).NameInStyle(), true);
                    } catch (Exception) { }
                }
            }
            //_fnList.Sort();
            _fsList = new ItemCollectionList(true)
            {
                { "8",SortierTyp.ZahlenwertFloat },
                { "9", SortierTyp.ZahlenwertFloat },
                { "10", SortierTyp.ZahlenwertFloat },
                { "11", SortierTyp.ZahlenwertFloat },
                { "12", SortierTyp.ZahlenwertFloat },
                { "14", SortierTyp.ZahlenwertFloat },
                { "16", SortierTyp.ZahlenwertFloat },
                { "18", SortierTyp.ZahlenwertFloat },
                { "20", SortierTyp.ZahlenwertFloat },
                { "22", SortierTyp.ZahlenwertFloat },
                { "24", SortierTyp.ZahlenwertFloat },
                { "26", SortierTyp.ZahlenwertFloat },
                { "28", SortierTyp.ZahlenwertFloat },
                { "36", SortierTyp.ZahlenwertFloat },
                { "48", SortierTyp.ZahlenwertFloat },
                { "72", SortierTyp.ZahlenwertFloat }
            };
            //_fsList.Sort();
        }
        FName.Item.AddRange(_fnList);
        //FName.Item.Sort();
        FSize.Item.AddRange(_fsList);
        //FSize.Item.Sort();
        Font = BlueFont.Get(Skin.DummyStandardFont); //, False, False, False, False, False, "000000", "", False)
        UpdateSampleText();
    }

    #endregion

    #region Properties

    public new BlueFont? Font {
        get => BlueFont.Get(FName.Item.Checked()[0].Internal, FloatParse(FSize.Item.Checked()[0].Internal), fFett.Checked, fKursiv.Checked, fUnterstrichen.Checked, fDurchge.Checked, fOutline.Checked, QuickImage.Get(cFarbe.ImageCode).ChangeGreenTo, QuickImage.Get(cRandF.ImageCode).ChangeGreenTo, fKap.Checked, OnlyUpper.Checked, OnlyLow.Checked);
        set {
            _adding = true;
            value ??= BlueFont.Get(Skin.DummyStandardFont);
            if (FName.Item[value.FontName] == null) { _ = FName.Item.Add(value.FontName, value.FontName, QuickImage.Get(ImageCode.Warnung, 20)); }
            FName.Item.UncheckAll();
            FName.Item[value.FontName].Checked = true;
            if (FSize.Item[value.FontSize.ToString(Constants.Format_Float1)] == null) { _ = FSize.Item.Add(value.FontSize.ToString(Constants.Format_Float1)); }
            FSize.Item.UncheckAll();
            FSize.Item[value.FontSize.ToString(Constants.Format_Float1)].Checked = true;
            fFett.Checked = value.Bold;
            fKursiv.Checked = value.Italic;
            fUnterstrichen.Checked = value.Underline;
            fDurchge.Checked = value.StrikeOut;
            fOutline.Checked = value.Outline;
            cFarbe.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, value.ColorMain).ToString();
            cRandF.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, value.ColorOutline).ToString();
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
        cFarbe.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
        UpdateSampleText();
    }

    private void cRandF_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(cRandF.ImageCode).ChangeGreenTo.FromHtmlCode();
        _ = ColorDia.ShowDialog();
        cRandF.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).ToString();
        UpdateSampleText();
    }

    private void fFett_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void FName_Item_CheckedChanged(object sender, System.EventArgs e) => UpdateSampleText();

    private void Ok_Click(object sender, System.EventArgs e) => Close();

    private void UpdateSampleText() {
        if (_adding) { return; }
        Sample.Image = Font.SampleText();
    }

    #endregion
}