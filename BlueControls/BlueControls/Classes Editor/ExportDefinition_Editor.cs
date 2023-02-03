﻿// Authors:
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
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueControls.Classes_Editor;

internal sealed partial class ExportDefinition_Editor : AbstractClassEditor<ExportDefinition> //  System.Windows.Forms.UserControl//
{
    #region Constructors

    public ExportDefinition_Editor() : base() => InitializeComponent();

    #endregion

    #region Methods

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

    protected override void EnabledAndFillFormula(ExportDefinition data) {
        if (data == null) { return; }
        Enabled = true;
        switch (data.Typ) {
            case ExportTyp.DatenbankCSVFormat:
                ExportCSVFormat.Checked = true;
                break;

            case ExportTyp.DatenbankHTMLFormat:
                ExportHTMLFormat.Checked = true;
                break;

            case ExportTyp.DatenbankOriginalFormat:
                ExportOriginalFormat.Checked = true;
                break;

            case ExportTyp.EinzelnMitFormular:
                ExportalsBild.Checked = true;
                data.BackupInterval = 0;
                break;

            default:
                Develop.DebugPrint(data.Typ);
                return;
        }
        ExportVerzeichnis.Text = data.Verzeichnis;
        ExportIntervall.Text = data.BackupInterval.ToString(Constants.Format_Float1);
        ExportAutomatischLöschen.Text = data.AutoDelete.ToString(Constants.Format_Float1);
        cbxExportFormularID.Text = data.ExportFormularId;
        ExportSpaltenAnsicht.Text = data.ExportSpaltenAnsicht.ToString();
        lbxFilter.Item.Clear();
        foreach (var thisFilter in data.Filter.Where(thisFilter => thisFilter != null)) {
            _ = lbxFilter.Item.Add(thisFilter, string.Empty, string.Empty);
        }
        lsbExportDateien.Item.Clear();
        foreach (var t1 in data.BereitsExportiert) {
            if (!string.IsNullOrEmpty(t1)) {
                var t = t1.Split('|');
                if (!FileExists(t[0])) {
                    _ = lsbExportDateien.Item.Add(t[0], t1, QuickImage.Get(ImageCode.Kritisch), true, "0000");
                } else {
                    var q1 = QuickImage.Get(ImageCode.Kugel, 16, Color.Red.MixColor(Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / data.AutoDelete), Color.Transparent);
                    _ = lsbExportDateien.Item.Add(t[0], t1, q1, true, t[1].CompareKey(SortierTyp.Datum_Uhrzeit));
                }
            }
        }
    }

    protected override void PrepaireFormula(ExportDefinition data) {
        cbxExportFormularID.Item.Clear();
        Forms.ExportDialog.AddLayoutsOff(cbxExportFormularID.Item, Item.Database, true);
        ExportSpaltenAnsicht.Item.Clear();
        for (var spa = 0; spa < Item.Database.ColumnArrangements.Count; spa++) {
            _ = ExportSpaltenAnsicht.Item.Add(data.Database.ColumnArrangements[spa].Name, spa.ToString());
        }
        if (!string.IsNullOrEmpty(data.Database.GlobalShowPass)) {
            ExportCSVFormat.Enabled = false;
            ExportHTMLFormat.Enabled = false;
            ExportalsBild.Enabled = false;
        } else {
            ExportCSVFormat.Enabled = true;
            ExportHTMLFormat.Enabled = true;
            ExportalsBild.Enabled = true;
        }
    }

    private void cbxExportFormularID_ItemClicked(object sender, BasicListItemEventArgs e) => UpdateExport(true);

    //private void ExportFilter_AddClicked(object sender, System.EventArgs e)
    //{
    //    Develop.DebugPrint_NichtImplementiert();
    //    var DummyFilter = new FilterItem(tmp.Database.Column.First, enFilterType.KeinFilter, string.Empty);
    //    var NewFilter = FilterItem_Editor();// DialogBox.eEditClass(DummyFilter, false);
    //    if (NewFilter == DummyFilter) { return; }
    //    var NewFilter2 = (FilterItem)NewFilter;
    //    if (NewFilter2.FilterType == enFilterType.KeinFilter) { return; }
    //    ExportFilter.Item.Add(new ObjectListItem(NewFilter2));
    //    UpdateExport(false);
    //}

    private void ExportDateien_ListOrItemChanged(object sender, System.EventArgs e) => UpdateExport(false);

    private void ExportDateien_RemoveClicked(object sender, ListOfBasicListItemEventArgs e) {
        foreach (var thisItem in e.Items) {
            if (thisItem is BasicListItem ThisItemBasic) {
                string fil;
                if (ThisItemBasic.Internal.Contains("|")) {
                    var f = ThisItemBasic.Internal.SplitAndCutBy("|");
                    fil = f[0];
                } else {
                    fil = ThisItemBasic.Internal;
                }
                if (FileExists(fil)) { _ = DeleteFile(fil, false); }
            }
        }
    }

    private void ExportOriginalFormat_CheckedChanged(object sender, System.EventArgs e) => UpdateExport(true);

    private void ExportVerzeichnis_TextChanged(object sender, System.EventArgs e) => UpdateExport(true);

    private void filterItemEditor_Changed(object sender, System.EventArgs e) {
        foreach (var thisitem in lbxFilter.Item) {
            if (thisitem is TextListItem tli) {
                if (tli.Tag == filterItemEditor.Item) {
                    tli.Text = filterItemEditor.Item.ReadableText();
                    tli.Symbol = filterItemEditor.Item.SymbolForReadableText();
                }
            }
        }
    }

    private void lbxFilter_AddClicked(object sender, System.EventArgs e) {
        var NewFilterItem = lbxFilter.Item.Add(new FilterItem(Item.Database, string.Empty), string.Empty, string.Empty);
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

    private void lbxFilter_ListOrItemChanged(object sender, System.EventArgs e) => UpdateExport(false);

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

        if (Item == null) { return; }
        if (MustDeleteAllExportFiles) {
            Item.DeleteAllBackups();
        }
        Item.Typ = ExportTyp.DatenbankOriginalFormat;
        if (ExportCSVFormat.Checked) {
            Item.Typ = ExportTyp.DatenbankCSVFormat;
        }
        if (ExportHTMLFormat.Checked) {
            Item.Typ = ExportTyp.DatenbankHTMLFormat;
        }
        if (ExportalsBild.Checked) {
            Item.Typ = ExportTyp.EinzelnMitFormular;
        }
        Item.Verzeichnis = ExportVerzeichnis.Text;
        if (!string.IsNullOrEmpty(ExportIntervall.Text)) {
            Item.BackupInterval = FloatParse(ExportIntervall.Text);
        }
        if (!string.IsNullOrEmpty(ExportAutomatischLöschen.Text)) {
            Item.AutoDelete = FloatParse(ExportAutomatischLöschen.Text);
        }
        Item.ExportFormularId = cbxExportFormularID.Text;
        Item.ExportSpaltenAnsicht = IntParse(ExportSpaltenAnsicht.Text);
        Item.Filter.Clear();
        foreach (var basicListItem in lbxFilter.Item) {
            var thisFilter = (TextListItem)basicListItem;
            Item.Filter.Add((FilterItem)thisFilter.Tag);
        }
        Item.BereitsExportiert.Clear();
        Item.BereitsExportiert.AddRange(lsbExportDateien.Item.ToListOfString());
    }

    #endregion
}