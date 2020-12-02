using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.BlueDatabaseDialogs {
    public partial class Filterleiste : GroupBox //  System.Windows.Forms.UserControl //
    {
        #region Variablen

        private Table _TableView;

        private enOrientation _orientation = enOrientation.Waagerecht;

        private enFilterTypesToShow _Filtertypes = enFilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter;

        private bool _isFilling = false;

        private string _AnsichtName = string.Empty;
        private string _ÄhnlicheAnsichtName = string.Empty;

        private ColumnViewCollection _ähnliche = null;

        #endregion

        #region Konstruktor
        public Filterleiste() {
            InitializeComponent();
            FillFilters();
        }

        #endregion

        #region Properties

        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation {
            get {
                return _orientation;
            }
            set {
                if (_orientation == value) { return; }
                _orientation = value;
            }
        }

        [DefaultValue(enFilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter)]
        public enFilterTypesToShow Filtertypes {
            get {
                return _Filtertypes;
            }
            set {
                if (_Filtertypes == value) { return; }
                _Filtertypes = value;
            }
        }


        /// <summary>
        /// Welche Knöpfe angezeigt werden sollen. Muss der Name einer Spaltenanordnung sein.
        /// </summary>
        [DefaultValue("")]
        public string AnsichtName {
            get {
                return _AnsichtName;
            }
            set {
                if (_AnsichtName == value) { return; }
                _AnsichtName = value;
            }
        }

        /// <summary>
        /// Wenn "Ähnliche" als Knopd vorhanden sein soll, muss hier der Name einer Spaltenanordnung stehen
        /// </summary>
        [DefaultValue("")]
        public string ÄhnlicheAnsichtName {
            get {
                return _ÄhnlicheAnsichtName;
            }
            set {
                if (_ÄhnlicheAnsichtName == value) { return; }
                _ÄhnlicheAnsichtName = value;
                GetÄhnlich();
            }
        }




        [DefaultValue((Table)null)]
        public Table Table {

            get {
                return _TableView;
            }
            set {

                if (_TableView == value) { return; }

                if (_TableView != null) {
                    _TableView.DatabaseChanged -= _TableView_DatabaseChanged;
                    _TableView.FilterChanged -= _TableView_FilterChanged;
                    _TableView.EnabledChanged -= _TableView_EnabledChanged;
                    _TableView.ViewChanged -= _TableView_ViewChanged;
                    //ChangeDatabase(null);
                }
                _TableView = value;
                GetÄhnlich();
                FillFilters();



                if (_TableView != null) {
                    //ChangeDatabase(_TableView.Database);
                    _TableView.DatabaseChanged += _TableView_DatabaseChanged;
                    //_TableView.CursorPosChanged += _TableView_CursorPosChanged;
                    //_TableView.ViewChanged += _TableView_ViewChanged;
                    _TableView.FilterChanged += _TableView_FilterChanged;
                    _TableView.EnabledChanged += _TableView_EnabledChanged;
                    _TableView.ViewChanged += _TableView_ViewChanged;
                }
            }
        }



        #endregion


        private void GetÄhnlich() {

            _ähnliche = null;

            if (_TableView == null || _TableView.Database == null) { return; }

            foreach (var thisArr in _TableView.Database.ColumnArrangements) {
                if (thisArr.Name.ToUpper() == _ÄhnlicheAnsichtName.ToUpper()) {
                    _ähnliche = thisArr;
                }
            }
        }

        private void _TableView_ViewChanged(object sender, System.EventArgs e) {
            FillFilters();
        }

        private void _TableView_FilterChanged(object sender, System.EventArgs e) {
            FillFilters();
        }

        internal void FillFilters() {


            if (InvokeRequired) {
                Invoke(new Action(() => FillFilters()));
                return;
            }

            if (_isFilling) { return; }

            _isFilling = true;


            btnAdmin.Visible = _TableView != null && _TableView.Database != null && _TableView.Database.IsAdministrator();

            #region ZeilenFilter befüllen
            if (_TableView != null && _TableView.Database != null && _TableView.Filter.IsRowFilterActiv()) {
                txbZeilenFilter.Text = _TableView.Filter.RowFilterText;
            }
            else {
                txbZeilenFilter.Text = string.Empty;
            }
            #endregion

            var toppos = 0;
            var leftpos = 0;
            var constwi = 0;
            var consthe = btnAlleFilterAus.Height;
            var down = 0;
            var right = 0;
            var anchor = System.Windows.Forms.AnchorStyles.None;
            var showPic = false;


            var breakafter = -1;
            var beginnx = -1;
            var afterBreakAddY = -1;


            #region Variablen für Waagerecht / Senkrecht bestimmen
            if (_orientation == enOrientation.Waagerecht) {

                toppos = btnAlleFilterAus.Top;
                beginnx = btnPinZurück.Right + Skin.Padding * 3;
                leftpos = beginnx;
                constwi = (int)(txbZeilenFilter.Width * 1.5);
                right = constwi + Skin.PaddingSmal;
                anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
                down = 0;
                breakafter = btnAdmin.Left;
                afterBreakAddY = txbZeilenFilter.Height + Skin.Padding;
            }
            else {
                toppos = btnAlleFilterAus.Bottom + Skin.Padding;
                leftpos = txbZeilenFilter.Left;
                constwi = Width - txbZeilenFilter.Left * 3;
                right = 0;
                anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                down = txbZeilenFilter.Height + Skin.Padding;

                showPic = _TableView != null && _TableView.Database != null && !string.IsNullOrEmpty(_TableView.Database.FilterImagePfad);
            }
            #endregion

            #region  Bild bei Bedarf laden und Visble richtig setze
            if (showPic) {
                pic.Height = (int)Math.Min(pic.Width * 0.7, Height * 0.6);
                var filename = _TableView.Database.FilterImagePfad;
                if (pic.Tag is string tx) {
                    if (tx != filename) { pic.Tag = null; }
                }

                if (pic.Tag == null) {
                    if (FileOperations.FileExists(filename)) {
                        pic.Image = BitmapExt.Image_FromFile(filename);
                    }
                }
                pic.Tag = filename;
                pic.Top = down;
                pic.Visible = true;
                toppos = pic.Bottom + Skin.Padding;
            }
            else {
                pic.Visible = false;
            }

            #endregion

            var flexsToDelete = new List<FlexiControlForFilter>();
            #region Vorhandene Flexis ermitteln

            foreach (var ThisControl in Controls) {
                if (ThisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
            }

            #endregion

            #region Neue Flexis erstellen / updaten
            if (_TableView != null && _TableView.Database != null && _TableView.Filter != null) {

                var columSort = new List<ColumnItem>();

                ColumnViewCollection orderArrangement = null;

                foreach (var thisArr in _TableView.Database.ColumnArrangements) {
                    if (thisArr.Name.ToUpper() == _AnsichtName.ToUpper()) {
                        orderArrangement = thisArr;
                    }
                }


                if (orderArrangement is null) { orderArrangement = _TableView?.CurrentArrangement; }

                #region Reihenfolge der Spalten bestimmen


                if (orderArrangement != null) {
                    foreach (var thisclsVitem in orderArrangement) {
                        columSort.AddIfNotExists(thisclsVitem.Column);
                    }
                }


                if (_TableView?.CurrentArrangement != null) {
                    foreach (var thisclsVitem in _TableView?.CurrentArrangement) {
                        columSort.AddIfNotExists(thisclsVitem.Column);
                    }
                }

                foreach (var thisColumn in _TableView.Database.Column) {
                    columSort.AddIfNotExists(thisColumn);
                }
                #endregion


                foreach (var thisColumn in columSort) {
                    var ShowMe = false;
                    var ViewItemOrder = orderArrangement[thisColumn];
                    var ViewItemCurrent = _TableView.CurrentArrangement[thisColumn];
                    var FilterItem = _TableView.Filter[thisColumn];

                    #region Sichtbarkeit des Filterelemts bestimmen

                    if (ViewItemOrder != null && _Filtertypes.HasFlag(enFilterTypesToShow.NachDefinierterAnsicht)) { ShowMe = true; }
                    if (ViewItemCurrent != null && FilterItem != null && _Filtertypes.HasFlag(enFilterTypesToShow.AktuelleAnsicht_AktiveFilter)) { ShowMe = true; }

                    #endregion


                    if (FilterItem == null && ShowMe) {
                        // Dummy-Filter, nicht in der Collection

                        if (thisColumn.FilterOptions == enFilterOptions.Enabled_OnlyAndAllowed) {
                            FilterItem = new FilterItem(thisColumn, enFilterType.Istgleich_UND_GroßKleinEgal, string.Empty);
                        }
                        else if (thisColumn.FilterOptions == enFilterOptions.Enabled_OnlyAndAllowed) {
                            FilterItem = new FilterItem(thisColumn, enFilterType.Istgleich_ODER_GroßKleinEgal, string.Empty);
                        }
                        else {
                            FilterItem = new FilterItem(thisColumn, enFilterType.Instr_GroßKleinEgal, string.Empty);
                        }

                    }


                    if (FilterItem != null && ShowMe) {
                        var flx = FlexiItemOf(FilterItem);
                        if (flx != null) {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            flexsToDelete.Remove(flx);
                        }
                        else {
                            // Na gut, eben neuen Flex erstellen
                            flx = new FlexiControlForFilter(_TableView, FilterItem, this);
                            flx.ValueChanged += Flx_ValueChanged;
                            flx.ButtonClicked += Flx_ButtonClicked;
                            Controls.Add(flx);
                        }


                        if (showPic && !FilterItem.Column.DauerFilterPos.IsEmpty) {
                            flx.Height = consthe * 2;


                            if (flx.GetComboBox() is ComboBox cbx) {
                                var data = cbx.Item.ItemData();  // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
                                var wi = Math.Min(data.Item1 + Skin.Padding + 16, 100);

                                flx.Width = wi;
                            }
                            else {
                                flx.Width = 100;
                            }


                            var sc = Math.Min(pic.Width / (float)pic.Image.Width, pic.Height / (float)pic.Image.Height);

                            var xr = (int)(pic.Image.Width * sc); // x REal angezeigte breite
                            var xab = (pic.Width - xr) / 2; // x Abstand
                            var xm = (int)(FilterItem.Column.DauerFilterPos.X / 10000f * xr) + xab; // Filter X mitte
                            flx.Left = xm - flx.Width / 2 + pic.Left;

                            var yr = (int)(pic.Image.Height * sc); // Y REal angezeigte breite
                            var yab = (pic.Height - yr) / 2; // Y Abstand
                            var ym = (int)(FilterItem.Column.DauerFilterPos.Y / 10000f * yr) + yab; ; // Filter X mitte
                            flx.Top = ym - flx.Height / 2 + pic.Top;



                            flx.BringToFront();



                        }
                        else {
                            if (breakafter > 0 && leftpos + constwi > breakafter) {
                                leftpos = beginnx;
                                toppos += afterBreakAddY;
                            }

                            flx.Top = toppos;
                            flx.Left = leftpos;
                            flx.Width = constwi;
                            flx.Height = consthe;
                            flx.Anchor = anchor;
                            toppos += down;
                            leftpos += right;
                        }

                    }
                }
            }
            #endregion

            #region  Unnötige Flexis löschen

            foreach (var thisFlexi in flexsToDelete) {
                thisFlexi.ValueChanged -= Flx_ValueChanged;
                thisFlexi.ButtonClicked -= Flx_ButtonClicked;
                thisFlexi.Visible = false;
                //thisFlexi.thisFilter = null;
                Controls.Remove(thisFlexi);
                thisFlexi.Dispose();
            }

            #endregion

            _isFilling = false;
        }

        private void Flx_ButtonClicked(object sender, System.EventArgs e) {

            var f = (FlexiControlForFilter)sender;


            if (f.CaptionPosition == enÜberschriftAnordnung.ohne) {
                // ein Großer Knopf ohne Überschrift, da wird der evl. Filter gelöscht
                _TableView.Filter.Remove(((FlexiControlForFilter)sender).Filter);
                return;
            }

            //f.Enabled = false;

            var autofilter = new AutoFilter(f.Filter.Column, _TableView.Filter);
            var p = f.PointToScreen(Point.Empty);

            autofilter.Position_LocateToPosition(new Point(p.X, p.Y + f.Height));

            autofilter.Show();

            autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();


        }

        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
            _TableView.Filter.Remove(e.Column);

            if (e.Comand != "Filter") { return; }
            e.Filter.Tag = "Filterleiste";
            _TableView.Filter.Add(e.Filter);

        }

        private void Flx_ValueChanged(object sender, System.EventArgs e) {
            if (_isFilling) { return; }

            if (sender is FlexiControlForFilter flx) {
                if (flx.EditType == enEditTypeFormula.Button) { return; }

                if (_TableView == null) { return; }

                var ISFilter = flx.WasThisValueClicked(); //  flx.Value.StartsWith("|");

                flx.Filter.Tag = "Filterleiste";

                var v = flx.Value; //.Trim("|");

                if (_TableView.Filter == null || _TableView.Filter.Count == 0 || !_TableView.Filter.Contains(flx.Filter)) {
                    if (ISFilter) { flx.Filter.FilterType = enFilterType.Istgleich_ODER_GroßKleinEgal; } // Filter noch nicht in der Collection, kann ganz einfach geändert werden
                    flx.Filter.SearchValue[0] = v;
                    _TableView.Filter.Add(flx.Filter);
                    return;
                }

                if (flx.Filter.SearchValue.Count != 1) {
                    Develop.DebugPrint_NichtImplementiert();
                    return;
                }

                if (ISFilter) {
                    flx.Filter.Changeto(enFilterType.Istgleich_ODER_GroßKleinEgal, v);
                }
                else {

                    if (string.IsNullOrEmpty(v)) {
                        _TableView.Filter.Remove(flx.Filter);
                    }
                    else {
                        flx.Filter.Changeto(enFilterType.Instr_GroßKleinEgal, v);
                        // flx.Filter.SearchValue[0] =v;
                    }
                }

            }

        }

        private FlexiControlForFilter FlexiItemOf(FilterItem filter) {

            foreach (var ThisControl in Controls) {
                if (ThisControl is FlexiControlForFilter flx) {

                    if (flx.Filter == filter) { return flx; }
                }
            }

            return null;

        }

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e) {
            GetÄhnlich();
            FillFilters();
        }

        private void _TableView_EnabledChanged(object sender, System.EventArgs e) {

            var HasDB = _TableView != null && _TableView.Database != null;
            txbZeilenFilter.Enabled = HasDB && LanguageTool.Translation == null && Enabled && _TableView.Enabled;
            btnAlleFilterAus.Enabled = HasDB && Enabled && _TableView.Enabled;

        }

        private void txbZeilenFilter_TextChanged(object sender, System.EventArgs e) {
            if (_isFilling) { return; }

            var NeuerT = txbZeilenFilter.Text.TrimStart();


            //NeuerT = NeuerT.TrimStart('+');
            //NeuerT = NeuerT.Replace("++", "+");
            //if (NeuerT == "+") { NeuerT = string.Empty; }



            if (NeuerT != txbZeilenFilter.Text) {
                txbZeilenFilter.Text = NeuerT;
                return;
            }


            Filter_ZeilenFilterSetzen();

        }

        private void txbZeilenFilter_Enter(object sender, System.EventArgs e) {
            Filter_ZeilenFilterSetzen();
        }


        private void Filter_ZeilenFilterSetzen() {

            if (_TableView == null || _TableView.Database == null) {
                btnÄhnliche.Visible = false;
                return;
            }


            var isF = _TableView.Filter.RowFilterText;

            var newF = txbZeilenFilter.Text;


            if (isF.ToUpper() == newF.ToUpper()) { return; }



            if (string.IsNullOrEmpty(newF)) {
                _TableView.Filter.Remove_RowFilter();
                btnÄhnliche.Visible = false;
                return;
            }

            _TableView.Filter.RowFilterText = newF;

            if (_ähnliche != null) {
                var fl = new List<FilterItem>() { new FilterItem(_TableView.Database.Column[0], enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text) };
                var r = _TableView.Database.Row.CalculateSortedRows(fl, null, null);
                btnÄhnliche.Visible = (r != null && r.Count == 1);

            }
            else {
                btnÄhnliche.Visible = false;
            }



        }

        private void btnAlleFilterAus_Click(object sender, System.EventArgs e) {
            if (_TableView.Database != null) { _TableView.Filter.Clear(); }
        }

        private void btnTextLöschen_Click(object sender, System.EventArgs e) {
            txbZeilenFilter.Text = string.Empty;
        }

        public bool Textbox_hasFocus() {
            return txbZeilenFilter.Focused();
        }

        private void btnPinZurück_Click(object sender, System.EventArgs e) {
            if (_TableView == null) { return; }
            _TableView.Pin(null);
        }

        private void btnPin_Click(object sender, System.EventArgs e) {
            if (_TableView == null) { return; }
            _TableView.Pin(_TableView.SortedRows());
        }

        private void btnAdmin_Click(object sender, System.EventArgs e) {
            Database.SaveAll(false);
            var x = new BlueControls.Forms.frmTableView(_TableView.Database, false, true);
            x.ShowDialog();
            x.Dispose();
            Database.SaveAll(false);
        }

        private void Filterleiste_SizeChanged(object sender, System.EventArgs e) {
            if (pic.Visible || _orientation == enOrientation.Waagerecht) { FillFilters(); }
        }

        private void btnÄhnliche_Click(object sender, System.EventArgs e) {

            var fl = new List<FilterItem>() { new FilterItem(_TableView.Database.Column[0], enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text) };
            var r = _TableView.Database.Row.CalculateSortedRows(fl, null, null);



            if (r == null || r.Count != 1 || _ähnliche == null || _ähnliche.Count == 0) {
                MessageBox.Show("Aktion fehlgeschlagen", enImageCode.Information, "OK");
                return;
            }

            btnAlleFilterAus_Click(null, null);


            foreach (var thiscolumnitem in _ähnliche) {

                if (thiscolumnitem.Column.AutoFilterSymbolPossible()) {

                    var fi = new FilterItem(thiscolumnitem.Column, r[0]);
                    _TableView.Filter.Add(fi);
                }

            }




        }
    }
}
