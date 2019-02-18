#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class ExportDefinition_Editor
    {


        public ExportDefinition_Editor()
        {
            InitializeComponent();
        }

        private ExportDefinition tmp;

        protected override void DisableAndClearFormula()
        {
            Enabled = false;
            ExportCSVFormat.Checked = false;
            ExportHTMLFormat.Checked = false;
            ExportOriginalFormat.Checked = false;
            ExportalsBild.Checked = false;

            ExportVerzeichnis.Text = "";
            ExportAutomatischLöschen.Text = "";
            ExportFormular.Text = "";
            ExportSpaltenAnsicht.Text = "";
            ExportFilter.Item.Clear();
            ExportDateien.Item.Clear();

        }

        protected override void EnabledAndFillFormula()
        {
            Enabled = true;
            switch (tmp.Typ)
            {
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
                    tmp.Intervall = 0;
                    break;
                default:
                    Develop.DebugPrint(tmp.Typ);
                    return;
            }

            ExportVerzeichnis.Text = tmp.Verzeichnis;

            ExportIntervall.Text = tmp.Intervall.ToString();
            ExportAutomatischLöschen.Text = tmp.AutomatischLöschen.ToString();

            ExportFormular.Text = tmp.ExportFormular;

            ExportSpaltenAnsicht.Text = tmp.ExportSpaltenAnsicht.ToString();

            ExportFilter.Item.Clear();

            foreach (var thisFilter in tmp.Filter)
            {
                if (thisFilter != null)
                {
                    ExportFilter.Item.Add(new ObjectListItem(thisFilter));
                }
            }


            ExportDateien.Item.Clear();

            foreach (var t1 in tmp.BereitsExportiert)
            {
                if (!string.IsNullOrEmpty(t1))
                {
                    var t = t1.Split('|');

                    if (!FileExists(t[0]))
                    {
                        ExportDateien.Item.Add(new TextListItem(t1, t[0], QuickImage.Get(enImageCode.Kritisch), true, "0000"));
                    }
                    else
                    {
                        var q1 = QuickImage.Get(enImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / tmp.AutomatischLöschen).ToHTMLCode(), "");
                        ExportDateien.Item.Add(new TextListItem(t1, t[0], q1, true, DataFormat.CompareKey(t[1], enDataFormat.Datum_und_Uhrzeit)));
                    }
                }
            }





        }

        protected override void PrepaireFormula()
        {

            ExportFormular.Item.Clear();
            ExportFormular.Item.AddRange(tmp.Database, true, string.Empty);


            ExportSpaltenAnsicht.Item.Clear();
            for (var spa = 0 ; spa < tmp.Database.ColumnArrangements.Count ; spa++)
            {
                ExportSpaltenAnsicht.Item.Add(new TextListItem(spa.ToString(), tmp.Database.ColumnArrangements[spa].Name));
            }

            if (!string.IsNullOrEmpty(tmp.Database.GlobalShowPass))
            {
                ExportCSVFormat.Enabled = false;
                ExportHTMLFormat.Enabled = false;
                ExportalsBild.Enabled = false;
            }
            else
            {
                ExportCSVFormat.Enabled = true;
                ExportHTMLFormat.Enabled = true;
                ExportalsBild.Enabled = true;
            }

        }

        protected override void ConvertObject(object ThisObject)
        {
            tmp = (ExportDefinition)ThisObject;
        }

        private void ExportVerzeichnis_TextChanged(object sender, System.EventArgs e)
        {
            UpdateExport(true);
        }

        private void ExportFormular_Item_Click(object sender, BasicListItemEventArgs e)
        {
            UpdateExport(true);
        }

        private void ExportDateien_Remove_Clicked(object sender, ListOfBasicListItemEventArgs e)
        {
            foreach (var ThisItemBasic in e.Items)
            {
                if (ThisItemBasic != null)
                {
                    string fil = null;
                    if (ThisItemBasic.Internal().Contains("|"))
                    {
                        var f = ThisItemBasic.Internal().SplitBy("|");
                        fil = f[0];
                    }
                    else
                    {
                        fil = ThisItemBasic.Internal();

                    }

                    if (FileExists(fil)) { FileOperations.DeleteFile(fil, false); }
                }
            }
        }


        private void ExportFilter_Changed(object sender, System.EventArgs e)
        {
            UpdateExport(false);
        }



        private void ExportOriginalFormat_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateExport(true);
        }

        private void UpdateExport(bool MustDeleteAllExportFiles)
        {

            if (ExportOriginalFormat.Checked)
            {
                ExportIntervall.Enabled = true;
                ExportSpaltenAnsicht.Enabled = false;
                ExportFormular.Enabled = false;
                ExportAutomatischLöschen.Enabled = true;
                ExportFilter.Enabled = false;

            }
            else if (ExportCSVFormat.Checked || ExportHTMLFormat.Checked)
            {
                ExportIntervall.Enabled = true;
                ExportSpaltenAnsicht.Enabled = true;
                ExportFormular.Enabled = false;
                ExportAutomatischLöschen.Enabled = true;
                ExportFilter.Enabled = true;

            }
            else if (ExportalsBild.Checked)
            {
                ExportIntervall.Enabled = false;
                ExportSpaltenAnsicht.Enabled = false;
                ExportFormular.Enabled = true;
                ExportAutomatischLöschen.Enabled = false;
                ExportFilter.Enabled = true;

            }



            if (IsFilling()) { return; }


            if (tmp == null) { return; }

            if (MustDeleteAllExportFiles)
            {
                tmp.DeleteAllBackups();
            }


            tmp.Typ = enExportTyp.DatenbankOriginalFormat;
            if (ExportCSVFormat.Checked)
            {
                tmp.Typ = enExportTyp.DatenbankCSVFormat;
            }
            if (ExportHTMLFormat.Checked)
            {
                tmp.Typ = enExportTyp.DatenbankHTMLFormat;
            }
            if (ExportalsBild.Checked)
            {
                tmp.Typ = enExportTyp.EinzelnMitFormular;
            }

            tmp.Verzeichnis = ExportVerzeichnis.Text;

            if (!string.IsNullOrEmpty(ExportIntervall.Text))
            {
                tmp.Intervall = float.Parse(ExportIntervall.Text);
            }
            if (!string.IsNullOrEmpty(ExportAutomatischLöschen.Text))
            {
                tmp.AutomatischLöschen = float.Parse(ExportAutomatischLöschen.Text);
            }
            tmp.ExportFormular = ExportFormular.Text;

            tmp.ExportSpaltenAnsicht = int.Parse(ExportSpaltenAnsicht.Text);

            tmp.Filter.Clear();
            foreach (ObjectListItem thisFilter in ExportFilter.Item)
            {
                tmp.Filter.Add(new FilterItem(tmp.Database, thisFilter.Internal()));
            }

            tmp.BereitsExportiert.Clear();
            tmp.BereitsExportiert.AddRange(ExportDateien.Item.ToListOfString());

            OnChanged(tmp);
        }



        private void ExportFilter_Add_Clicked(object sender, AllreadyHandledEventArgs e)
        {
            Develop.DebugPrint_NichtImplementiert();

            //e.AlreadyHandled = true;

            //var DummyFilter = new FilterItem(tmp.Database.Column[0], enFilterType.KeinFilter, "");
            //var NewFilter = DialogBox.eEditClass(DummyFilter, false);
            //if (NewFilter == DummyFilter) { return; }

            //var NewFilter2 = (FilterItem)NewFilter;
            //if (NewFilter2.FilterType == enFilterType.KeinFilter) { return; }


            //ExportFilter.Item.Add(new ObjectListItem(NewFilter2));

            //UpdateExport(false);

        }

        private void ExportDateien_Changed(object sender, System.EventArgs e)
        {
            UpdateExport(false);
        }
    }
}
