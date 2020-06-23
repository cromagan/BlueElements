using System.Collections.Generic;
using System.ComponentModel;
using BlueControls.Controls;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueBasics;
using BlueBasics.Enums;
using System;
using BlueControls.Enums;

namespace BlueControls.BlueDatabaseDialogs
{
    public partial class Filterleiste : GroupBox // System.Windows.Forms.UserControl //  
    {

        private Table _TableView;

        private enOrientation _orientation = enOrientation.Waagerecht;

        private enFilterTypesToShow _Filtertypes = enFilterTypesToShow.SichtbareSpalten_DauerFilter_und_Aktive;

        private bool _isFilling = false;

        public Filterleiste()
        {
            InitializeComponent();
            FillFilters();
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
            }
        }

        [DefaultValue(enOrientation.Waagerecht)]
        public enFilterTypesToShow Filtertypes
        {
            get
            {
                return _Filtertypes;
            }
            set
            {
                if (_Filtertypes == value) { return; }
                _Filtertypes = value;
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


            if (InvokeRequired)
            {
                Invoke(new Action(() => FillFilters()));
                return;
            }

            if (_isFilling) { return; }

            _isFilling = true;


            btnAdmin.Visible = _TableView != null && _TableView.Database != null && _TableView.Database.IsAdministrator();

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
            var showPic = false;


            if (_orientation == enOrientation.Waagerecht)
            {
                toppos = btnAlleFilterAus.Top;
                leftpos = btnPinZurück.Right + Skin.Padding * 3;
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

                showPic = _TableView != null && _TableView.Database != null && !string.IsNullOrEmpty(_TableView.Database.FilterImagePfad);

            }


            if (showPic)
            {
                pic.Height = (int)(pic.Width * 0.7);
                var filename = _TableView.Database.FilterImagePfad;
                if (pic.Tag is string tx)
                {
                    if (tx != filename) { pic.Tag = null; }
                }

                if (pic.Tag == null)
                {
                    if (FileOperations.FileExists(filename))
                    {
                        pic.Image = BitmapExt.Image_FromFile(filename);
                    }
                }
                pic.Tag = filename;
                pic.Top = down;
                pic.Visible = true;
                down = pic.Bottom + Skin.Padding;
            }
            else
            {
                pic.Visible = false;
            }





            // Vorhandene Flexis ermitteln
            var flexsToDelete = new List<FlexiControlForFilter>();
            foreach (var ThisControl in Controls)
            {
                if (ThisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
            }


            if (_TableView != null && _TableView.Filter != null)
            {
                var colara = _TableView.Database.ColumnArrangements[_TableView.Arrangement];



                foreach (var thisclsVitem in colara)
                {

                    var ShowMe = false;

                    if (_Filtertypes.HasFlag(enFilterTypesToShow.SichtbareSpalten_Alle)) { ShowMe = true; }
                    if (_Filtertypes.HasFlag(enFilterTypesToShow.SichtbareSpalten_DauerFilter))
                    {
                        if (thisclsVitem.Column.AutoFilter_Dauerfilter.HasFlag(enDauerfilter.waagerecht) && _orientation == enOrientation.Waagerecht) { ShowMe = true; }
                        if (thisclsVitem.Column.AutoFilter_Dauerfilter.HasFlag(enDauerfilter.senkrecht) && _orientation == enOrientation.Senkrecht) { ShowMe = true; }
                    }


                    var f = _TableView.Filter[thisclsVitem.Column];
                    if (f != null && _Filtertypes.HasFlag(enFilterTypesToShow.SichtbareSpalten_AktiveFilter)) { ShowMe = true; }

                    if (f == null && ShowMe)
                    {
                        // Dummy-Filter, nicht in der Collection
                        f = new FilterItem(thisclsVitem.Column, enFilterType.Instr_GroßKleinEgal, string.Empty);
                    }


                    if (f != null && ShowMe)
                    {
                        var flx = FlexiItemOf(f);
                        if (flx != null)
                        {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            flexsToDelete.Remove(flx);
                        }
                        else
                        {
                            // Na gut, eben neuen Flex erstellen
                            flx = new FlexiControlForFilter(_TableView, f, enÜberschriftAnordnung.Links_neben_Dem_Feld);
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

                if (_TableView == null) { return; }

                var ISFilter = flx.WasThisValueClicked(); //  flx.Value.StartsWith("|");

                var v = flx.Value; //.Trim("|");

                if (_TableView.Filter == null || _TableView.Filter.Count == 0 || !_TableView.Filter.Contains(flx.Filter))
                {
                    if (ISFilter) { flx.Filter.FilterType = enFilterType.Istgleich_ODER_GroßKleinEgal; } // Filter noch nicht in der Collection, kann ganz einfach geändert werdern
                    flx.Filter.SearchValue[0] = v;
                    _TableView.Filter.Add(flx.Filter);
                    return;
                }

                if (flx.Filter.SearchValue.Count != 1)
                {
                    Develop.DebugPrint_NichtImplementiert();
                    return;
                }

                if (ISFilter)
                {
                    flx.Filter.Changeto(enFilterType.Istgleich_ODER_GroßKleinEgal, v);
                }
                else
                {

                    if (string.IsNullOrEmpty(v))
                    {
                        _TableView.Filter.Remove(flx.Filter);
                    }
                    else
                    {
                        flx.Filter.Changeto(enFilterType.Instr_GroßKleinEgal, v);
                        // flx.Filter.SearchValue[0] =v;
                    }
                }

            }

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


            //NeuerT = NeuerT.TrimStart('+');
            //NeuerT = NeuerT.Replace("++", "+");
            //if (NeuerT == "+") { NeuerT = string.Empty; }



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

        }



        private void btnAlleFilterAus_Click(object sender, System.EventArgs e)
        {
            if (_TableView.Database != null) { _TableView.Filter.Clear(); }
        }


        private void btnTextLöschen_Click(object sender, System.EventArgs e)
        {
            txbZeilenFilter.Text = string.Empty;
        }

        public bool Textbox_hasFocus()
        {
            return txbZeilenFilter.Focused();
        }

        private void btnPinZurück_Click(object sender, System.EventArgs e)
        {
            if (_TableView == null) { return; }
            _TableView.Pin(null);
        }

        private void btnPin_Click(object sender, System.EventArgs e)
        {
            if (_TableView == null) { return; }
            _TableView.Pin(_TableView.SortedRows());

        }

        private void btnAdmin_Click(object sender, System.EventArgs e)
        {
            Database.SaveAll(false);
            var x = new BlueControls.Forms.frmTableView(_TableView.Database, false, true);
            x.ShowDialog();
            x.Dispose();
            Database.SaveAll(false);


        }

        private void Filterleiste_SizeChanged(object sender, System.EventArgs e)
        {


           if (pic.Visible)
            {
                FillFilters();
            }

        }
    }
}
