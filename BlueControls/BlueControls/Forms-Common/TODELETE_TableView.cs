using System.Collections.Generic;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Forms
{
    ///Imports Microsoft.VisualBasic

    public sealed partial class frmTableView
    {
        public frmTableView(Database cDatabase)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            TableView.Database = cDatabase;
            //  PrepareForShowing(Controls)
        }


        private void SearchT_TextChange(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchT.Text))
            {
                SearchB.Enabled = true;
            }
            else
            {
                if (SearchB.Checked == false)
                {
                    SearchB.Enabled = false;
                }
            }
        }

        private void SearchT_Enter(object sender, System.EventArgs e)
        {
            Filter_ZeilenFilterSetzenx(true);
        }

        private void SearchB_Click(object sender, System.EventArgs e)
        {
            Filter_ZeilenFilterSetzenx(SearchB.Checked);
        }

        private void Filter_ZeilenFilterSetzenx(bool NowChecked)
        {
            TableView.Filter.Delete_RowFilter();


            if (NowChecked)
            {
                TableView.Filter.Add(enFilterType.Instr_GroßKleinEgal, SearchT.Text);
            }

            SearchB.Checked = NowChecked;

            if (!NowChecked)
            {
                if (string.IsNullOrEmpty(SearchT.Text))
                {
                    SearchB.Enabled = false;
                }
            }
        }

        private void FilterAus_Click(object sender, System.EventArgs e)
        {
            TableView.Filter.Clear();
            SearchB.Checked = false;
        }


        public List<RowItem> GetFilteredItems()
        {
            ShowDialog();
            return TableView.SortedRows();
        }


        //Private Sub SuchT_TextChange(ByVal sender As Object) Handles SuchT.TextChanged
        //    CheckButtons()
        //End Sub

        //Private Sub SuchT_Enter(ByVal sender As Object) Handles SuchT.Enter
        //    tbl.Database.Filter.Delete_RowFilter()
        //    tbl.Database.Filter.Add(-1, enFilterType.Instr_GroßKleinEgal, SuchT.Text)
        //    tbl.Invalidate()
        //End Sub

        //Private Sub SuchB_Click(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles SuchB.Click
        //    If SuchB.Checked Then
        //        SuchT_Enter(Nothing)
        //    Else
        //        tbl.Database.Filter.Delete_RowFilter()
        //    End If

        //    CheckButtons()
        //End Sub


        //Private Sub CheckButtons()
        //    SuchT.Enabled = True

        //    SuchB.Checked = tbl.Database.Filter.IsRowFilterActiv

        //    If Not SuchB.Checked And String.IsNullOrEmpty(SuchT.Text) Then
        //        SuchB.Enabled = False
        //    Else
        //        SuchB.Enabled = True
        //    End If


        //    FilterAus.Enabled = tbl.Database.Filter.Activ
        //End Sub


        private void TableView_RowsSorted(object sender, System.EventArgs e)
        {
            FilterAus.Enabled = TableView.Filter.Activ();
        }

        private void OK_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void TableView_ColumnArrangementChanged(object sender, System.EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }

        private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
            TableView.Arrangement = int.Parse(e.Item.Internal());
        }

        private void TableView_ViewChanged(object sender, System.EventArgs e)
        {
            TableView.WriteColumnArrangementsInto(cbxColumnArr);
        }
    }
}