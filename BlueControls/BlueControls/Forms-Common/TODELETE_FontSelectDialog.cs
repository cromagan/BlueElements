#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.ItemCollection;
using System;
using System.Drawing;

namespace BlueControls.Forms {
    public partial class FontSelectDialog {
        private bool Adding;

        private static ItemCollectionList FNList;
        private static ItemCollectionList FSList;

        public FontSelectDialog() {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            if (FNList == null) {
                FNList = new ItemCollectionList();
                foreach (var f in FontFamily.Families) {
                    if (!string.IsNullOrEmpty(f.Name)) {
                        if (f.IsStyleAvailable(FontStyle.Regular)) {
                            var fo = new Font(f.Name, 100);

                            try {
                                BlueFont.MeasureString("T", fo);
                                FNList.Add(string.Empty, f.Name, BlueFont.Get(f, 12).NameInStyle(), true);
                            } catch (Exception) {

                            }
                        }
                    }
                }
                FNList.Sort();

                FSList = new ItemCollectionList
                {
                    { "8", enDataFormat.Gleitkommazahl },
                    { "9", enDataFormat.Gleitkommazahl },
                    { "10", enDataFormat.Gleitkommazahl },
                    { "11", enDataFormat.Gleitkommazahl },
                    { "12", enDataFormat.Gleitkommazahl },
                    { "14", enDataFormat.Gleitkommazahl },
                    { "16", enDataFormat.Gleitkommazahl },
                    { "18", enDataFormat.Gleitkommazahl },
                    { "20", enDataFormat.Gleitkommazahl },
                    { "22", enDataFormat.Gleitkommazahl },
                    { "24", enDataFormat.Gleitkommazahl },
                    { "26", enDataFormat.Gleitkommazahl },
                    { "28", enDataFormat.Gleitkommazahl },
                    { "36", enDataFormat.Gleitkommazahl },
                    { "48", enDataFormat.Gleitkommazahl },
                    { "72", enDataFormat.Gleitkommazahl }
                };

                FSList.Sort();
            }

            FName.Item.AddRange(FNList);
            FName.Item.Sort();

            FSize.Item.AddRange(FSList);
            FSize.Item.Sort();

            Font = BlueFont.Get(Skin.DummyStandardFont); //, False, False, False, False, False, "000000", "", False)

            UpdateSampleText();
        }

        public new BlueFont Font {
            get => BlueFont.Get(FName.Item.Checked()[0].Internal, float.Parse(FSize.Item.Checked()[0].Internal), fFett.Checked, fKursiv.Checked, fUnterstrichen.Checked, fDurchge.Checked, fOutline.Checked, QuickImage.Get(cFarbe.ImageCode).ChangeGreenTo, QuickImage.Get(cRandF.ImageCode).ChangeGreenTo, fKap.Checked, OnlyUpper.Checked, OnlyLow.Checked);
            set {

                Adding = true;
                if (value == null) { value = BlueFont.Get(Skin.DummyStandardFont); }

                if (FName.Item[value.FontName] == null) { FName.Item.Add(value.FontName, value.FontName, QuickImage.Get(enImageCode.Warnung, 20)); }
                FName.Item.UncheckAll();
                FName.Item[value.FontName].Checked = true;

                if (FSize.Item[value.FontSize.ToString()] == null) { FSize.Item.Add(value.FontSize.ToString()); }
                FSize.Item.UncheckAll();
                FSize.Item[value.FontSize.ToString()].Checked = true;
                fFett.Checked = value.Bold;
                fKursiv.Checked = value.Italic;
                fUnterstrichen.Checked = value.Underline;
                fDurchge.Checked = value.StrikeOut;
                fOutline.Checked = value.Outline;
                cFarbe.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", value.Color_Main.ToHTMLCode()).ToString();
                cRandF.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", value.Color_Outline.ToHTMLCode()).ToString();
                fKap.Checked = value.Kapitälchen;
                OnlyLow.Checked = value.OnlyLower;
                OnlyUpper.Checked = value.OnlyUpper;
                Adding = false;

                UpdateSampleText();
            }
        }

        private void UpdateSampleText() {
            if (Adding) { return; }

            Sample.Image = Font.SampleText().Bitmap;
        }

        private void FName_Item_CheckedChanged(object sender, System.EventArgs e) {
            UpdateSampleText();
        }

        private void fFett_CheckedChanged(object sender, System.EventArgs e) {
            UpdateSampleText();
        }

        private void cFarbe_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(cFarbe.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            cFarbe.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
            UpdateSampleText();
        }

        private void cRandF_Click(object sender, System.EventArgs e) {
            ColorDia.Color = QuickImage.Get(cRandF.ImageCode).ChangeGreenTo.FromHTMLCode();
            ColorDia.ShowDialog();
            cRandF.ImageCode = QuickImage.Get(enImageCode.Kreis, 16, "", ColorDia.Color.ToHTMLCode()).ToString();
            UpdateSampleText();
        }

        private void Ok_Click(object sender, System.EventArgs e) {
            Close();
        }
    }
}