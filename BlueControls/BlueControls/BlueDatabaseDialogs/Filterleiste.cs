using System.Collections.Generic;
using System.ComponentModel;
using BlueControls.Controls;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueBasics;
using System;
using BlueBasics.Enums;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class Filterleiste : GroupBox // System.Windows.Forms.UserControl
    {

        private Table _TableView;

        private enOrientation _orientation;

        public Filterleiste()
        {
            InitializeComponent();
            FillFilters();
            SteuerelementeAnordnen();
        }

        private void SteuerelementeAnordnen()
        {
            throw new NotImplementedException();
        }

        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                if (_orientation == value) { return; }
                _orientation = value;
                SteuerelementeAnordnen();

            }

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

            #region ZeilenFilter
            if (_TableView != null && _TableView.Database != null && _TableView.Filter.IsRowFilterActiv())
            {
                txbZeilenFilter.Text = _TableView.Filter.RowFilterText();
            }
            else
            {
                txbZeilenFilter.Text = string.Empty;
            }
            #endregion

            // Vorhandene Flexis ermitteln
            var flexvorhanden = new List<FlexiControlForFilter>();
            foreach (var ThisControl in Controls)
            {
                if (ThisControl is FlexiControlForFilter flx) { flexvorhanden.Add(flx); }
            }


            foreach (var thisFilter in _TableView.Filter)
            {

                if (thisFilter.Column is ColumnItem co)
                {

                    var flx = FlexiItemOf(co);
                    flexvorhanden.Remove(flx);
                    TupleExtensions richtig stellen

                }

            }


        }


        private FlexiControlForFilter FlexiItemOf(ColumnItem column)
        {

            foreach (var ThisControl in Controls)
            {
                if (ThisControl is FlexiControlForFilter flx)
                {

                    if (flx.ColumnKey == column.KeyColumnKey && flx.Database == column.Database)
                    {
                        return flx;
                    }
                }
            }

            return null;

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
            if (_TableView.Database != null) { _TableView.Filter.Remove_RowFilter(); }

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
