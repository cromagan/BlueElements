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

        private enOrientation _orientation = enOrientation.Waagerecht;

        private bool _isFilling = false;

        public Filterleiste()
        {
            InitializeComponent();
            FillFilters();
            SteuerelementeAnordnen();
        }

        private void SteuerelementeAnordnen()
        {
            //         throw new NotImplementedException();
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
                    _TableView.FilterChanged -= _TableView_FilterChanged;
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
                    _TableView.FilterChanged += _TableView_FilterChanged;
                    _TableView.EnabledChanged += _TableView_EnabledChanged;
                }
            }
        }

        private void _TableView_FilterChanged(object sender, System.EventArgs e)
        {
            FillFilters();
        }

        private void FillFilters()
        {

            _isFilling = true;

            #region ZeilenFilter
            if (_TableView != null && _TableView.Database != null && _TableView.Filter.IsRowFilterActiv())
            {
                txbZeilenFilter.Text = _TableView.Filter.RowFilterText;
            }
            else
            {
                txbZeilenFilter.Text = string.Empty;
            }
            #endregion

            var toppos = 0;
            var leftpos = 0;
            var constwi = 0;
            var down = 0;
            var right = 0;
            System.Windows.Forms.AnchorStyles anchor;


            if (_orientation == enOrientation.Waagerecht)
            {
                toppos = btnAlleFilterAus.Top;
                leftpos = btnAlleFilterAus.Right + Skin.PaddingSmal;
                constwi = (int)(txbZeilenFilter.Width * 1.5);
                right = constwi + Skin.PaddingSmal;
                anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
                down = 0;
            }
            else
            {
                toppos = btnAlleFilterAus.Bottom + Skin.Padding;
                leftpos = txbZeilenFilter.Left;
                constwi = Width - txbZeilenFilter.Left * 3;
                right = 0;
                anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                down = txbZeilenFilter.Height + Skin.Padding;
            }


            // Vorhandene Flexis ermitteln
            var flexsToDelete = new List<FlexiControlForFilter>();
            foreach (var ThisControl in Controls)
            {
                if (ThisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
            }


            if (_TableView != null && _TableView.Filter != null)
            {
                /// Flexis erstellen und hinrutschen
                foreach (var thisFilter in _TableView.Filter)
                {
                    if (thisFilter.Column is ColumnItem co)
                    {
                        var flx = FlexiItemOf(thisFilter);

                        if (flx != null)
                        {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            flexsToDelete.Remove(flx);
                        }
                        else
                        {
                            // Na gut, eben neuen Flex erstellen
                            flx = new FlexiControlForFilter(thisFilter, enÜberschriftAnordnung.Links_neben_Dem_Feld);
                            flx.ValueChanged += Flx_ValueChanged;
                            flx.ButtonClicked += Flx_ButtonClicked;
                            Controls.Add(flx);
                        }

                        flx.Top = toppos;
                        flx.Left = leftpos;
                        flx.Width = constwi;
                        flx.Height = btnAlleFilterAus.Height;
                        flx.Anchor = anchor;
                        toppos += down;
                        leftpos += right;
                    }
                }
            }

            // Unnötige Flexis löschen

            foreach (var thisFlexi in flexsToDelete)
            {
                thisFlexi.ValueChanged -= Flx_ValueChanged;
                thisFlexi.ButtonClicked -= Flx_ButtonClicked;
                thisFlexi.Visible = false;
                //thisFlexi.thisFilter = null;
                Controls.Remove(thisFlexi);
                thisFlexi.Dispose();
            }

            _isFilling = false;
        }

        private void Flx_ButtonClicked(object sender, System.EventArgs e)
        {
            _TableView.Filter.Remove(((FlexiControlForFilter)sender).Filter);
        }

        private void Flx_ValueChanged(object sender, System.EventArgs e)
        {
            if (_isFilling) { return; }


            if (sender is FlexiControlForFilter flx)
            {
                if (flx.EditType == enEditTypeFormula.Button) { return; }

            }

            throw new NotImplementedException();
        }

        private FlexiControlForFilter FlexiItemOf(FilterItem filter)
        {

            foreach (var ThisControl in Controls)
            {
                if (ThisControl is FlexiControlForFilter flx)
                {

                    if (flx.Filter == filter) { return flx; }
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
            if (_isFilling) { return; }

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

            if (_TableView == null || _TableView.Database == null) { return; }


            var isF = _TableView.Filter.RowFilterText;

            var newF = txbZeilenFilter.Text;


            if (isF.ToUpper() == newF.ToUpper()) { return; }



            if (string.IsNullOrEmpty(newF))
            {
                _TableView.Filter.Remove_RowFilter();
                return;
            }



            _TableView.Filter.RowFilterText = newF;
            //foreach (var ThisFilterItem in _TableView.Filter)
            //{
            //    if (ThisFilterItem != null && ThisFilterItem.Column == null)
            //    {
            //        ThisFilterItem.SearchValue[0] = newF;
            //    }
            //}




        }



        private void btnAlleFilterAus_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Database != null) { _TableView.Filter.Clear(); }
            //txbZeilenFilter.Text = string.Empty;
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
