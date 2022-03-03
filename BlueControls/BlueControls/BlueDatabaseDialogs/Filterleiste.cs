using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class Filterleiste : GroupBox //  System.Windows.Forms.UserControl //
    {
        #region Fields

        private ColumnViewCollection _ähnliche;
        private string _ähnlicheAnsichtName = string.Empty;
        private bool _isFilling;
        private string _lastLooked = string.Empty;
        private Table _tableView;

        #endregion

        #region Constructors

        public Filterleiste() {
            InitializeComponent();
            FillFilters();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Wenn "Ähnliche" als Knopd vorhanden sein soll, muss hier der Name einer Spaltenanordnung stehen
        /// </summary>
        [DefaultValue("")]
        public string ÄhnlicheAnsichtName {
            get => _ähnlicheAnsichtName;
            set {
                if (_ähnlicheAnsichtName == value) { return; }
                _ähnlicheAnsichtName = value;
                GetÄhnlich();
            }
        }

        /// <summary>
        /// Welche Knöpfe angezeigt werden sollen. Muss der Name einer Spaltenanordnung sein.
        /// </summary>
        [DefaultValue("")]
        public string AnsichtName { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool AutoPin { get; set; }

        [DefaultValue(enFilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter)]
        public enFilterTypesToShow Filtertypes { get; set; } = enFilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter;

        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation { get; set; } = enOrientation.Waagerecht;

        [DefaultValue((Table)null)]
        public Table Table {
            get => _tableView;
            set {
                if (_tableView == value) { return; }
                if (_tableView != null) {
                    _tableView.DatabaseChanged -= _TableView_DatabaseChanged;
                    _tableView.FilterChanged -= _TableView_PinnedOrFilterChanged;
                    _tableView.PinnedChanged -= _TableView_PinnedOrFilterChanged;
                    _tableView.EnabledChanged -= _TableView_EnabledChanged;
                    _tableView.ViewChanged -= _TableView_ViewChanged;
                    //ChangeDatabase(null);
                }
                _tableView = value;
                GetÄhnlich();
                FillFilters();
                if (_tableView != null) {
                    //ChangeDatabase(_TableView.Database);
                    _tableView.DatabaseChanged += _TableView_DatabaseChanged;
                    //_TableView.CursorPosChanged += _TableView_CursorPosChanged;
                    //_TableView.ViewChanged += _TableView_ViewChanged;
                    _tableView.FilterChanged += _TableView_PinnedOrFilterChanged;
                    _tableView.PinnedChanged += _TableView_PinnedOrFilterChanged;
                    _tableView.EnabledChanged += _TableView_EnabledChanged;
                    _tableView.ViewChanged += _TableView_ViewChanged;
                }
            }
        }

        #endregion

        #region Methods

        public bool Textbox_hasFocus() => txbZeilenFilter.Focused;

        internal void FillFilters() {
            if (InvokeRequired) {
                Invoke(new Action(FillFilters));
                return;
            }
            if (_isFilling) { return; }
            _isFilling = true;
            //btnAdmin.Visible = _TableView != null && _TableView.Database != null && _TableView.Database.IsAdministrator();
            //btnPin.Enabled = !_AutoPin;
            //btnPin.Visible = !_AutoPin;
            btnPinZurück.Enabled = _tableView?.Database != null && _tableView.PinnedRows != null && _tableView.PinnedRows.Count > 0;

            #region ZeilenFilter befüllen

            txbZeilenFilter.Text = _tableView?.Database != null && _tableView.Filter.IsRowFilterActiv()
                ? _tableView.Filter.RowFilterText
                : string.Empty;

            #endregion ZeilenFilter befüllen

            int toppos;
            int leftpos;
            int constwi;
            var consthe = btnAlleFilterAus.Height;
            int down;
            int right;
            AnchorStyles anchor;
            var showPic = false;

            #region Variablen für Waagerecht / Senkrecht bestimmen

            if (Orientation == enOrientation.Waagerecht) {
                toppos = btnAlleFilterAus.Top;
                var beginnx = btnPinZurück.Right + (Skin.Padding * 3);
                leftpos = beginnx;
                constwi = (int)(txbZeilenFilter.Width * 1.5);
                right = constwi + Skin.PaddingSmal;
                anchor = AnchorStyles.Top | AnchorStyles.Left;
                down = 0;
                //breakafter = btnAdmin.Left;
                //afterBreakAddY = txbZeilenFilter.Height + Skin.Padding;
            } else {
                toppos = btnAlleFilterAus.Bottom + Skin.Padding;
                leftpos = txbZeilenFilter.Left;
                constwi = Width - (txbZeilenFilter.Left * 3);
                right = 0;
                anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                down = txbZeilenFilter.Height + Skin.Padding;
                showPic = _tableView?.Database != null && !string.IsNullOrEmpty(_tableView.Database.FilterImagePfad);
            }

            #endregion Variablen für Waagerecht / Senkrecht bestimmen

            #region Bild bei Bedarf laden und Visble richtig setze

            if (showPic) {
                pic.Height = (int)Math.Min(pic.Width * 0.7, Height * 0.6);
                var filename = _tableView.Database.FilterImagePfad;
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
            } else {
                pic.Visible = false;
            }

            #endregion Bild bei Bedarf laden und Visble richtig setze

            List<FlexiControlForFilter> flexsToDelete = new();

            #region Vorhandene Flexis ermitteln

            foreach (var thisControl in Controls) {
                if (thisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
            }

            #endregion Vorhandene Flexis ermitteln

            #region Neue Flexis erstellen / updaten

            if (_tableView?.Database != null && _tableView.Filter != null) {
                List<ColumnItem?> columSort = new();
                ColumnViewCollection? orderArrangement = null;
                foreach (var thisArr in _tableView.Database.ColumnArrangements.Where(thisArr => thisArr.Name.ToUpper() == AnsichtName.ToUpper())) {
                    orderArrangement = thisArr;
                }
                if (orderArrangement is null) { orderArrangement = _tableView?.CurrentArrangement; }

                #region Reihenfolge der Spalten bestimmen

                if (orderArrangement != null) {
                    foreach (var thisclsVitem in orderArrangement) {
                        columSort.AddIfNotExists(thisclsVitem.Column);
                    }
                }
                if (_tableView?.CurrentArrangement != null) {
                    foreach (var thisclsVitem in _tableView?.CurrentArrangement) {
                        columSort.AddIfNotExists(thisclsVitem.Column);
                    }
                }
                foreach (var thisColumn in _tableView.Database.Column) {
                    columSort.AddIfNotExists(thisColumn);
                }

                #endregion Reihenfolge der Spalten bestimmen

                foreach (var thisColumn in columSort) {
                    var showMe = false;
                    var viewItemOrder = orderArrangement[thisColumn];
                    var viewItemCurrent = _tableView.CurrentArrangement[thisColumn];
                    var filterItem = _tableView.Filter[thisColumn];

                    #region Sichtbarkeit des Filterelemts bestimmen

                    if (thisColumn.AutoFilterSymbolPossible()) {
                        if (viewItemOrder != null && Filtertypes.HasFlag(enFilterTypesToShow.NachDefinierterAnsicht)) { showMe = true; }
                        if (viewItemCurrent != null && filterItem != null && Filtertypes.HasFlag(enFilterTypesToShow.AktuelleAnsicht_AktiveFilter)) { showMe = true; }
                    }

                    #endregion Sichtbarkeit des Filterelemts bestimmen

                    if (filterItem == null && showMe) {
                        // Dummy-Filter, nicht in der Collection
                        filterItem = thisColumn.FilterOptions == enFilterOptions.Enabled_OnlyAndAllowed ? new FilterItem(thisColumn, enFilterType.Istgleich_UND_GroßKleinEgal, string.Empty)
                                   : thisColumn.FilterOptions == enFilterOptions.Enabled_OnlyOrAllowed ? new FilterItem(thisColumn, enFilterType.Istgleich_ODER_GroßKleinEgal, string.Empty)
                                   : new FilterItem(thisColumn, enFilterType.Instr_GroßKleinEgal, string.Empty);
                    }
                    if (filterItem != null && showMe) {
                        var flx = FlexiItemOf(filterItem);
                        if (flx != null) {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            flexsToDelete.Remove(flx);
                        } else {
                            // Na gut, eben neuen Flex erstellen
                            flx = new FlexiControlForFilter(_tableView, filterItem, this);
                            flx.ValueChanged += Flx_ValueChanged;
                            flx.ButtonClicked += Flx_ButtonClicked;
                            Controls.Add(flx);
                        }
                        if (showPic && !filterItem.Column.DauerFilterPos.IsEmpty && pic.Image != null) {
                            flx.Height = consthe * 2;
                            if (flx.GetComboBox() is ComboBox cbx) {
                                var (biggestItemX, _, _, _) = cbx.Item.ItemData();  // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
                                var wi = Math.Min(biggestItemX + Skin.Padding + 16, 100);
                                flx.Width = wi;
                            } else {
                                flx.Width = 100;
                            }
                            var sc = Math.Min(pic.Width / (float)pic.Image.Width, pic.Height / (float)pic.Image.Height);
                            var xr = (int)(pic.Image.Width * sc); // x REal angezeigte breite
                            var xab = (pic.Width - xr) / 2; // x Abstand
                            var xm = (int)(filterItem.Column.DauerFilterPos.X / 10000f * xr) + xab; // Filter X mitte
                            flx.Left = xm - (flx.Width / 2) + pic.Left;
                            var yr = (int)(pic.Image.Height * sc); // Y REal angezeigte breite
                            var yab = (pic.Height - yr) / 2; // Y Abstand
                            var ym = (int)(filterItem.Column.DauerFilterPos.Y / 10000f * yr) + yab;
                            flx.Top = ym - (flx.Height / 2) + pic.Top;
                            flx.BringToFront();
                        } else {
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

            #endregion Neue Flexis erstellen / updaten

            #region Unnötige Flexis löschen

            foreach (var thisFlexi in flexsToDelete) {
                thisFlexi.ValueChanged -= Flx_ValueChanged;
                thisFlexi.ButtonClicked -= Flx_ButtonClicked;
                thisFlexi.Visible = false;
                //thisFlexi.thisFilter = null;
                Controls.Remove(thisFlexi);
                thisFlexi.Dispose();
            }

            #endregion Unnötige Flexis löschen

            _isFilling = false;
        }

        private void _TableView_DatabaseChanged(object sender, System.EventArgs e) {
            GetÄhnlich();
            FillFilters();
        }

        private void _TableView_EnabledChanged(object sender, System.EventArgs e) {
            var hasDb = _tableView?.Database != null;
            txbZeilenFilter.Enabled = hasDb && LanguageTool.Translation == null && Enabled && _tableView.Enabled;
            btnAlleFilterAus.Enabled = hasDb && Enabled && _tableView.Enabled;
        }

        private void _TableView_PinnedOrFilterChanged(object sender, System.EventArgs e) => FillFilters();

        private void _TableView_ViewChanged(object sender, System.EventArgs e) => FillFilters();

        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
            _tableView.Filter.Remove(e.Column);
            if (e.Comand != "Filter") { return; }
            //e.Filter.Herkunft = "Filterleiste";
            _tableView.Filter.Add(e.Filter);
        }

        //private void btnAdmin_Click(object sender, System.EventArgs e) {
        //    BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
        //    frmTableView x = new(_TableView.Database, false, true);
        //    x.ShowDialog();
        //    x.Dispose();
        //    BlueBasics.MultiUserFile.clsMultiUserFile.SaveAll(false);
        //}

        private void btnÄhnliche_Click(object sender, System.EventArgs e) {
            List<FilterItem> fl = new() { new FilterItem(_tableView.Database.Column[0], enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text) };

            var r = _tableView.Database.Row.CalculateFilteredRows(fl);
            if (r == null || r.Count != 1 || _ähnliche == null || _ähnliche.Count == 0) {
                MessageBox.Show("Aktion fehlgeschlagen", enImageCode.Information, "OK");
                return;
            }

            btnAlleFilterAus_Click(null, null);
            foreach (var thiscolumnitem in _ähnliche.Where(thiscolumnitem => thiscolumnitem.Column.AutoFilterSymbolPossible())) {
                if (r[0].CellIsNullOrEmpty(thiscolumnitem.Column)) {
                    FilterItem fi = new(thiscolumnitem.Column, enFilterType.Istgleich_UND_GroßKleinEgal, string.Empty);
                    _tableView.Filter.Add(fi);
                } else if (thiscolumnitem.Column.MultiLine) {
                    var l = r[0].CellGetList(thiscolumnitem.Column).SortedDistinctList();
                    FilterItem fi = new(thiscolumnitem.Column, enFilterType.Istgleich_UND_GroßKleinEgal, l);
                    _tableView.Filter.Add(fi);
                } else {
                    var l = r[0].CellGetString(thiscolumnitem.Column);
                    FilterItem fi = new(thiscolumnitem.Column, enFilterType.Istgleich_UND_GroßKleinEgal, l);
                    _tableView.Filter.Add(fi);
                }
            }
            btnÄhnliche.Enabled = false;
        }

        private void btnAlleFilterAus_Click(object sender, System.EventArgs? e) {
            _lastLooked = string.Empty;
            if (_tableView.Database != null) {
                _tableView.Filter.Clear();
                //if (_AutoPin) { _TableView.Pin(null); }
            }
        }

        private void btnPin_Click(object sender, System.EventArgs e) => _tableView?.Pin(_tableView.VisibleUniqueRows());

        private void btnPinZurück_Click(object sender, System.EventArgs e) {
            _lastLooked = string.Empty;
            _tableView?.Pin(null);
        }

        private void btnTextLöschen_Click(object sender, System.EventArgs e) => txbZeilenFilter.Text = string.Empty;

        private void DoÄhnlich() {
            //_TableView.Database.Column.Count == 0 Ist eine nigelnagelneue Datenbank
            if (_tableView?.Database == null || _tableView.Database.Column.Count == 0) { return; }
            List<FilterItem> fl = new() { new FilterItem(_tableView.Database.Column[0], enFilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text) };

            var r = _tableView.Database.Row.CalculateFilteredRows(fl);
            if (_ähnliche != null) {
                btnÄhnliche.Visible = true;
                btnÄhnliche.Enabled = r != null && r.Count == 1;
            } else {
                btnÄhnliche.Visible = false;
            }

            if (AutoPin && r != null && r.Count == 1) {
                if (_lastLooked != r[0].CellFirstString()) {
                    if (_tableView.SortedRows().Get(r[0]) == null) {
                        if (MessageBox.Show("Die Zeile wird durch Filterungen <b>ausgeblendet</b>.<br>Soll sie zusätzlich <b>angepinnt</b> werden?", enImageCode.Pinnadel, "Ja", "Nein") == 0) {
                            _tableView.PinAdd(r[0]);
                        }
                        _lastLooked = r[0].CellFirstString();
                    }
                }
            }
        }

        private void Filter_ZeilenFilterSetzen() {
            if (_tableView?.Database == null) {
                DoÄhnlich();
                return;
            }
            var isF = _tableView.Filter.RowFilterText;
            var newF = txbZeilenFilter.Text;
            if (isF.ToUpper() == newF.ToUpper()) { return; }
            if (string.IsNullOrEmpty(newF)) {
                _tableView.Filter.Remove_RowFilter();
                DoÄhnlich();
                return;
            }
            _tableView.Filter.RowFilterText = newF;
            DoÄhnlich();
        }

        private void Filterleiste_SizeChanged(object sender, System.EventArgs e) {
            if (pic.Visible || Orientation == enOrientation.Waagerecht) { FillFilters(); }
        }

        private FlexiControlForFilter? FlexiItemOf(FilterItem filter) {
            foreach (var thisControl in Controls) {
                if (thisControl is FlexiControlForFilter flx) {
                    if (flx.Filter.ToString() == filter.ToString()) { return flx; }
                }
            }
            return null;
        }

        private void Flx_ButtonClicked(object sender, System.EventArgs e) {
            var f = (FlexiControlForFilter)sender;
            if (f.CaptionPosition == enÜberschriftAnordnung.ohne) {
                // ein Großer Knopf ohne Überschrift, da wird der evl. Filter gelöscht
                _tableView.Filter.Remove(((FlexiControlForFilter)sender).Filter);
                return;
            }
            //f.Enabled = false;
            AutoFilter autofilter = new(f.Filter.Column, _tableView.Filter, _tableView.PinnedRows);
            var p = f.PointToScreen(Point.Empty);
            autofilter.Position_LocateToPosition(new Point(p.X, p.Y + f.Height));
            autofilter.Show();
            autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }

        private void Flx_ValueChanged(object sender, System.EventArgs e) {
            if (_isFilling) { return; }
            if (sender is FlexiControlForFilter flx) {
                if (flx.EditType == enEditTypeFormula.Button) { return; }
                if (_tableView == null) { return; }
                var isFilter = flx.WasThisValueClicked(); //  flx.Value.StartsWith("|");
                //flx.Filter.Herkunft = "Filterleiste";
                var v = flx.Value; //.Trim("|");
                if (_tableView.Filter == null || _tableView.Filter.Count == 0 || !_tableView.Filter.Contains(flx.Filter)) {
                    if (isFilter) { flx.Filter.FilterType = enFilterType.Istgleich_ODER_GroßKleinEgal; } // Filter noch nicht in der Collection, kann ganz einfach geändert werden
                    flx.Filter.SearchValue[0] = v;
                    _tableView.Filter.Add(flx.Filter);
                    return;
                }
                if (flx.Filter.SearchValue.Count != 1) {
                    Develop.DebugPrint_NichtImplementiert();
                    return;
                }
                if (isFilter) {
                    flx.Filter.Changeto(enFilterType.Istgleich_ODER_GroßKleinEgal, v);
                } else {
                    if (string.IsNullOrEmpty(v)) {
                        _tableView.Filter.Remove(flx.Filter);
                    } else {
                        flx.Filter.Changeto(enFilterType.Instr_GroßKleinEgal, v);
                        // flx.Filter.SearchValue[0] =v;
                    }
                }
            }
        }

        private void GetÄhnlich() {
            _ähnliche = null;
            if (_tableView?.Database != null && !string.IsNullOrEmpty(_ähnlicheAnsichtName)) {
                foreach (var thisArr in _tableView.Database.ColumnArrangements.Where(thisArr => thisArr.Name.ToUpper() == _ähnlicheAnsichtName.ToUpper())) {
                    _ähnliche = thisArr;
                }
            }
            DoÄhnlich();
        }

        private void txbZeilenFilter_Enter(object sender, System.EventArgs e) => Filter_ZeilenFilterSetzen();

        private void txbZeilenFilter_TextChanged(object sender, System.EventArgs e) {
            var neuerT = txbZeilenFilter.Text.TrimStart();
            btnTextLöschen.Enabled = !string.IsNullOrEmpty(neuerT);
            if (_isFilling) { return; }

            if (neuerT != txbZeilenFilter.Text) {
                txbZeilenFilter.Text = neuerT;
                return;
            }
            Filter_ZeilenFilterSetzen();
            DoÄhnlich();
        }

        #endregion
    }
}