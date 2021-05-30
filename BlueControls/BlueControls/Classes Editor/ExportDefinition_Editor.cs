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
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Drawing;
using static BlueBasics.FileOperations;
using static BlueBasics.modConverter;

namespace BlueControls.Classes_Editor {
    internal sealed partial class ExportDefinition_Editor : AbstractClassEditor<ExportDefinition> //  System.Windows.Forms.UserControl// 
    {


        public ExportDefinition_Editor() : base() {
            InitializeComponent();
        }

        protected override void DisableAndClearFormula() {
            Enabled = false;
            ExportCSVFormat.Checked = false;
            ExportHTMLFormat.Checked = false;
            ExportOriginalFormat.Checked = false;
            ExportalsBild.Checked = false;

            ExportVerzeichnis.Text = string.Empty;
            ExportAutomatischLöschen.Text = string.Empty;
            cbxExportFormularID.Text = string.Empty;
            ExportSpaltenAnsicht.Text = string.Empty;
            lbxFilter.Item.Clear();
            lsbExportDateien.Item.Clear();
        }

        protected override void EnabledAndFillFormula() {
            Enabled = true;
            switch (Item.Typ) {
                case enExportTyp.DatenbankCSVFormat:
                    ExportCSVFormat.Checked = true;
                    break;
                case enExportTyp.DatenbankHTMLFormat:
                    ExportHTMLFormat.Checked = true;
                    break;
                case enExportTyp.DatenbankOriginalFormat:
                    ExportOriginalFormat.Checked = true;
                    break;
                case enExportTyp.EinzelnMitFormular:
                    ExportalsBild.Checked = true;
                    Item.Intervall = 0;
                    break;
                default:
                    Develop.DebugPrint(Item.Typ);
                    return;
            }

            ExportVerzeichnis.Text = Item.Verzeichnis;

            ExportIntervall.Text = Item.Intervall.ToString();
            ExportAutomatischLöschen.Text = Item.AutomatischLöschen.ToString();

            cbxExportFormularID.Text = Item.ExportFormularID;

            ExportSpaltenAnsicht.Text = Item.ExportSpaltenAnsicht.ToString();

            lbxFilter.Item.Clear();

            foreach (var thisFilter in Item.Filter) {
                if (thisFilter != null) {
                    lbxFilter.Item.Add(thisFilter);
                }
            }


            lsbExportDateien.Item.Clear();

            foreach (var t1 in Item.BereitsExportiert) {
                if (!string.IsNullOrEmpty(t1)) {
                    var t = t1.Split('|');

                    if (!FileExists(t[0])) {
                        lsbExportDateien.Item.Add(t[0], t1, QuickImage.Get(enImageCode.Kritisch), true, "0000");
                    } else {
                        var q1 = QuickImage.Get(enImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / Item.AutomatischLöschen).ToHTMLCode(), "");
                        lsbExportDateien.Item.Add(t[0], t1, q1, true, DataFormat.CompareKey(t[1], enDataFormat.Datum_und_Uhrzeit));
                    }
                }
            }





        }

        protected override void PrepaireFormula() {

            cbxExportFormularID.Item.Clear();
            BlueControls.Forms.ExportDialog.AddLayoutsOff(cbxExportFormularID.Item, Item.Database, true, string.Empty);


            ExportSpaltenAnsicht.Item.Clear();
            for (var spa = 0; spa < Item.Database.ColumnArrangements.Count; spa++) {
                ExportSpaltenAnsicht.Item.Add(Item.Database.ColumnArrangements[spa].Name, spa.ToString());
            }

            if (!string.IsNullOrEmpty(Item.Database.GlobalShowPass)) {
                ExportCSVFormat.Enabled = false;
                ExportHTMLFormat.Enabled = false;
                ExportalsBild.Enabled = false;
            } else {
                ExportCSVFormat.Enabled = true;
                ExportHTMLFormat.Enabled = true;
                ExportalsBild.Enabled = true;
            }

        }


        private void ExportVerzeichnis_TextChanged(object sender, System.EventArgs e) {
            UpdateExport(true);
        }

        private void cbxExportFormularID_ItemClicked(object sender, BasicListItemEventArgs e) {
            UpdateExport(true);
        }

        private void ExportDateien_RemoveClicked(object sender, ListOfBasicListItemEventArgs e) {
            foreach (var thisItem in e.Items) {

                if (thisItem is BasicListItem ThisItemBasic) {
                    string fil = null;
                    if (ThisItemBasic.Internal.Contains("|")) {
                        var f = ThisItemBasic.Internal.SplitBy("|");
                        fil = f[0];
                    } else {
                        fil = ThisItemBasic.Internal;

                    }

                    if (FileExists(fil)) { FileOperations.DeleteFile(fil, false); }
                }
            }
        }






        private void ExportOriginalFormat_CheckedChanged(object sender, System.EventArgs e) {
            UpdateExport(true);
        }

        private void UpdateExport(bool MustDeleteAllExportFiles) {

            if (ExportOriginalFormat.Checked) {
                ExportIntervall.Enabled = true;
                ExportSpaltenAnsicht.Enabled = false;
                cbxExportFormularID.Enabled = false;
                ExportAutomatischLöschen.Enabled = true;
                lbxFilter.Enabled = false;

            } else if (ExportCSVFormat.Checked || ExportHTMLFormat.Checked) {
                ExportIntervall.Enabled = true;
                ExportSpaltenAnsicht.Enabled = true;
                cbxExportFormularID.Enabled = false;
                ExportAutomatischLöschen.Enabled = true;
                lbxFilter.Enabled = true;

            } else if (ExportalsBild.Checked) {
                ExportIntervall.Enabled = false;
                ExportSpaltenAnsicht.Enabled = false;
                cbxExportFormularID.Enabled = true;
                ExportAutomatischLöschen.Enabled = false;
                lbxFilter.Enabled = true;

            }



            if (IsFilling) { return; }


            if (Item == null) { return; }

            if (MustDeleteAllExportFiles) {
                Item.DeleteAllBackups();
            }


            Item.Typ = enExportTyp.DatenbankOriginalFormat;
            if (ExportCSVFormat.Checked) {
                Item.Typ = enExportTyp.DatenbankCSVFormat;
            }
            if (ExportHTMLFormat.Checked) {
                Item.Typ = enExportTyp.DatenbankHTMLFormat;
            }
            if (ExportalsBild.Checked) {
                Item.Typ = enExportTyp.EinzelnMitFormular;
            }

            Item.Verzeichnis = ExportVerzeichnis.Text;

            if (!string.IsNullOrEmpty(ExportIntervall.Text)) {
                Item.Intervall = float.Parse(ExportIntervall.Text);
            }
            if (!string.IsNullOrEmpty(ExportAutomatischLöschen.Text)) {
                Item.AutomatischLöschen = float.Parse(ExportAutomatischLöschen.Text);
            }
            Item.ExportFormularID = cbxExportFormularID.Text;

            Item.ExportSpaltenAnsicht = int.Parse(ExportSpaltenAnsicht.Text);

            Item.Filter.Clear();
            foreach (TextListItem thisFilter in lbxFilter.Item) {
                Item.Filter.Add((FilterItem)thisFilter.Tag);
            }

            Item.BereitsExportiert.Clear();
            Item.BereitsExportiert.AddRange(lsbExportDateien.Item.ToListOfString());

            OnChanged(Item);
        }






        //private void ExportFilter_AddClicked(object sender, System.EventArgs e)
        //{
        //    Develop.DebugPrint_NichtImplementiert();

        //    var DummyFilter = new FilterItem(tmp.Database.Column[0], enFilterType.KeinFilter, "");
        //    var NewFilter = FilterItem_Editor();// DialogBox.eEditClass(DummyFilter, false);
        //    if (NewFilter == DummyFilter) { return; }

        //    var NewFilter2 = (FilterItem)NewFilter;
        //    if (NewFilter2.FilterType == enFilterType.KeinFilter) { return; }


        //    ExportFilter.Item.Add(new ObjectListItem(NewFilter2));

        //    UpdateExport(false);

        //}

        private void ExportDateien_ListOrItemChanged(object sender, System.EventArgs e) {
            UpdateExport(false);
        }


        #region  Filter 

        private void lbxFilter_AddClicked(object sender, System.EventArgs e) {
            var NewFilterItem = lbxFilter.Item.Add(new FilterItem(Item.Database, string.Empty));
            NewFilterItem.Checked = true;
        }

        private void lbxFilter_ItemCheckedChanged(object sender, System.EventArgs e) {
            if (lbxFilter.Item.Checked().Count != 1) {
                filterItemEditor.Item = null;
                return;
            }

            if (Item.Database.ReadOnly) {
                filterItemEditor.Item = null;
                return;
            }


            filterItemEditor.Item = (FilterItem)((TextListItem)lbxFilter.Item.Checked()[0]).Tag;
        }

        private void lbxFilter_ListOrItemChanged(object sender, System.EventArgs e) {
            UpdateExport(false);
        }





        #endregion

        private void filterItemEditor_Changed(object sender, System.EventArgs e) {
            if (IsFilling) { return; }


            foreach (var thisitem in lbxFilter.Item) {
                if (thisitem is TextListItem tli) {
                    if (tli.Tag == filterItemEditor.Item) {
                        tli.Text = filterItemEditor.Item.ReadableText();
                        tli.Symbol = filterItemEditor.Item.SymbolForReadableText();
                    }
                }
            }


            OnChanged(Item);
        }
    }
}
