using System.Collections.Generic;
using System.ComponentModel;
using BlueControls.Controls;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueBasics;
using System;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class Filterleiste : System.Windows.Forms.UserControl
    {

        private Table _TableView;

        public Filterleiste()
        {
            InitializeComponent();
            FillFilters();
        }

        [DefaultValue((Table)null)]
        public Table Table
        {

            get
            {
                return _TableView;
            }
            set
            {

                if (_TableView == value) { return; }

                if (_TableView != null)
                {
                    _TableView.DatabaseChanged -= _TableView_DatabaseChanged;
                    //_TableView.CursorPosChanged -= _TableView_CursorPosChanged;
                    //_TableView.ViewChanged -= _TableView_ViewChanged;
                    _TableView.EnabledChanged -= _TableView_EnabledChanged;
                    //ChangeDatabase(null);
                }
                _TableView = value;
                FillFilters();


                if (_TableView != null)
                {
                    //ChangeDatabase(_TableView.Database);
                    _TableView.DatabaseChanged += _TableView_DatabaseChanged;
                    //_TableView.CursorPosChanged += _TableView_CursorPosChanged;
                    //_TableView.ViewChanged += _TableView_ViewChanged;
                    _TableView.EnabledChanged += _TableView_EnabledChanged;
                }
            }
        }

        private void FillFilters()
        {
            if (_TableView != null && _TableView.Database != null)
            {
                if (_TableView.Filter.IsRowFilterActiv())
                {
                    txbZeilenFilter.Text = _TableView.Filter.RowFilterText();
                }
                else
                {
                    txbZeilenFilter.Text = "";
                }
            }
            else
            {
                txbZeilenFilter.Text = "";
            }



        }

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e)
        {
            FillFilters();
        }

        private void _TableView_EnabledChanged(object sender, System.EventArgs e)
        {

            var HasDB = _TableView != null && _TableView.Database != null;
            txbZeilenFilter.Enabled = HasDB && LanguageTool.Translation == null && Enabled && _TableView.Enabled;
            btnAlleFilterAus.Enabled = HasDB && Enabled && _TableView.Enabled;

        }

        private void txbZeilenFilter_TextChanged(object sender, System.EventArgs e)
        {


            var NeuerT = txbZeilenFilter.Text.TrimStart();


            NeuerT = NeuerT.TrimStart('+');
            NeuerT = NeuerT.Replace("++", "+");
            if (NeuerT == "+") { NeuerT = string.Empty; }



            if (NeuerT != txbZeilenFilter.Text)
            {
                txbZeilenFilter.Text = NeuerT;
                return;
            }


            Filter_ZeilenFilterSetzen();

        }

        private void txbZeilenFilter_Enter(object sender, System.EventArgs e)
        {
            Filter_ZeilenFilterSetzen();
        }


        private void Filter_ZeilenFilterSetzen()
        {
            if (_TableView.Database != null) { _TableView.Filter.Delete_RowFilter(); }

            if (_TableView.Database != null && !string.IsNullOrEmpty(txbZeilenFilter.Text))
            {
                _TableView.Filter.Add(enFilterType.Instr_UND_GroßKleinEgal, new List<string>(txbZeilenFilter.Text.SplitBy("+")));
            }


            //Preview_GenerateAll();
        }



        private void btnAlleFilterAus_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Database != null) { _TableView.Filter.Clear(); }
            txbZeilenFilter.Text = string.Empty;
        }


        private void btnTextLöschen_Click(object sender, System.EventArgs e)
        {
            txbZeilenFilter.Text = string.Empty;
        }

        public bool Textbox_hasFocus()
        {
           return txbZeilenFilter.Focused();
        }
    }
}
