// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.CellRenderer;

namespace BlueControls.BlueDatabaseDialogs;

public partial class Filterleiste : GenericControlReciverSender //  System.Windows.Forms.UserControl //
{
    #region Fields

    private ColumnViewCollection? _ähnliche;

    private string _ähnlicheAnsichtName = string.Empty;

    private bool _isFilling;

    private string _lastLooked = string.Empty;

    private FilterCollection? _prevFilter;

    private Table? _table;

    #endregion

    #region Constructors

    public Filterleiste() : base(false, false) {
        InitializeComponent();
        FillFilters();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Wenn "Ähnliche" als Knopf vorhanden sein soll, muss hier der Name einer Spaltenanordnung stehen
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

    [DefaultValue(FilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter)]
    public FilterTypesToShow Filtertypes { get; set; } = FilterTypesToShow.DefinierteAnsicht_Und_AktuelleAnsichtAktiveFilter;

    [DefaultValue((Table?)null)]
    public Table? Table {
        get => _table;
        set {
            if (_table == value) { return; }
            if (_table != null) {
                _table.DatabaseChanged -= _table_DatabaseChanged;
                _table.FilterChanged -= _table_PinnedOrFilterChanged;
                _table.PinnedChanged -= _table_PinnedOrFilterChanged;
                _table.EnabledChanged -= _table_EnabledChanged;
                _table.ViewChanged -= _table_ViewChanged;
            }
            _table = value;
            GetÄhnlich();
            FillFilters();
            if (_table != null) {
                _table.DatabaseChanged += _table_DatabaseChanged;
                _table.FilterChanged += _table_PinnedOrFilterChanged;
                _table.PinnedChanged += _table_PinnedOrFilterChanged;
                _table.EnabledChanged += _table_EnabledChanged;
                _table.ViewChanged += _table_ViewChanged;
            }
        }
    }

    #endregion

    #region Methods

    internal void FillFilters() {
        if (IsDisposed) { return; }

        if (InvokeRequired) {
            _ = Invoke(new Action(FillFilters));
            return;
        }
        if (_isFilling) { return; }
        _isFilling = true;

        FilterOutput.ChangeTo(_table?.Filter);

        btnPinZurück.Enabled = _table is { Database: not null, PinnedRows.Count: > 0 };

        #region ZeilenFilter befüllen

        txbZeilenFilter.Text = _table?.Database != null &&
                               _table.Filter.IsRowFilterActiv()
                                ? _table.Filter.RowFilterText
                                : string.Empty;

        #endregion

        var consthe = btnAlleFilterAus.Height;

        #region Variablen für Waagerecht / Senkrecht bestimmen

        var toppos = btnAlleFilterAus.Top;
        var beginnx = btnPinZurück.Right + (Skin.Padding * 3);
        var leftpos = beginnx;
        var constwi = (int)(txbZeilenFilter.Width * 1.5);
        var right = constwi + Skin.PaddingSmal;
        const AnchorStyles anchor = AnchorStyles.Top | AnchorStyles.Left;
        const int down = 0;

        #endregion

        List<FlexiControlForFilter> flexsToDelete = [];

        #region Vorhandene Flexis ermitteln

        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
        }

        #endregion

        var cu = _table?.CurrentArrangement;

        #region Neue Flexis erstellen / updaten

        if (_table?.Database is { } db) {
            var tcvc = ColumnViewCollection.ParseAll(db);
            List<ColumnItem> columSort = [];
            var orderArrangement = tcvc.Get(AnsichtName);

            #region Reihenfolge der Spalten bestimmen

            if (orderArrangement != null) {
                foreach (var thisclsVitem in orderArrangement) {
                    if (thisclsVitem?.Column is { IsDisposed: false } ci) { _ = columSort.AddIfNotExists(ci); }
                }
            }

            if (cu != null) {
                foreach (var thisclsVitem in cu) {
                    if (thisclsVitem?.Column is { IsDisposed: false } ci) { _ = columSort.AddIfNotExists(ci); }
                }
            }

            foreach (var thisColumn in _table.Database.Column) {
                _ = columSort.AddIfNotExists(thisColumn);
            }

            #endregion

            var count = 0;

            foreach (var thisColumn in columSort) {
                var showMe = false;
                if (thisColumn.Database is { IsDisposed: false }) {
                    var viewItemOrder = orderArrangement?[thisColumn];
                    var viewItemCurrent = cu?[thisColumn];
                    var filterItem = _table?.Filter[thisColumn];

                    #region Sichtbarkeit des Filterelements bestimmen

                    if (thisColumn.AutoFilterSymbolPossible()) {
                        if (viewItemOrder != null && Filtertypes.HasFlag(FilterTypesToShow.NachDefinierterAnsicht)) { showMe = true; }
                        if (viewItemCurrent != null && Filtertypes.HasFlag(FilterTypesToShow.AktuelleAnsicht_AktiveFilter) && filterItem != null) { showMe = true; }
                    }

                    #endregion

                    //if (filterItem == null && showMe) {
                    //    // Dummy-Filter, nicht in der Collection
                    //    filterItem = new FilterItem(thisColumn, FilterType.Instr_GroßKleinEgal, string.Empty);
                    //}

                    if (showMe) {
                        var flx = FlexiItemOf(thisColumn);
                        if (flx != null) {
                            // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                            _ = flexsToDelete.Remove(flx);
                        } else {
                            // Na gut, eben neuen Flex erstellen
                            var r = Renderer_Abstract.RendererOf(thisColumn);
                            flx = new FlexiControlForFilter(thisColumn, CaptionPosition.Links_neben_dem_Feld, r);
                            flx.FilterOutput.Database = thisColumn.Database;
                            flx.Standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
                            flx.Filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
                            ChildIsBorn(flx);
                            //flx.DoOutputSettings(this);
                            //flx.DoInputSettings(parent, this);
                            flx.FilterOutput.PropertyChanging += FlexSingeFilter_FilterOutput_PropertyChanging;
                            flx.FilterOutput.PropertyChanged += FlexSingeFilter_FilterOutput_PropertyChanged;
                            Controls.Add(flx);
                        }

                        if (leftpos + constwi > Width && count > 0) {
                            leftpos = beginnx;
                            toppos = toppos + consthe + Skin.PaddingSmal;
                        }

                        flx.Top = toppos;
                        flx.Left = leftpos;
                        flx.Width = constwi;
                        flx.Height = consthe;
                        flx.Anchor = anchor;
                        toppos += down;
                        leftpos += right;
                        count++;
                    }
                }
            }
        }

        #endregion

        #region Unnötige Flexis löschen

        foreach (var thisFlexi in flexsToDelete) {
            thisFlexi.FilterOutput.PropertyChanging -= FlexSingeFilter_FilterOutput_PropertyChanging;
            thisFlexi.FilterOutput.PropertyChanged -= FlexSingeFilter_FilterOutput_PropertyChanged;
            thisFlexi.Visible = false;
            Controls.Remove(thisFlexi);
            thisFlexi.Dispose();
        }

        #endregion Unnötige Flexis löschen

        _isFilling = false;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) => Skin.Draw_Back_Transparent(gr, ClientRectangle, this);//Intiall();

    private void _table_DatabaseChanged(object sender, System.EventArgs e) {
        GetÄhnlich();
        FillFilters();
    }

    private void _table_EnabledChanged(object sender, System.EventArgs e) {
        var hasDb = _table?.Database != null;
        var tabViewEbabled = _table?.Enabled ?? false;

        txbZeilenFilter.Enabled = hasDb && LanguageTool.Translation == null && Enabled && tabViewEbabled;
        btnAlleFilterAus.Enabled = hasDb && Enabled && tabViewEbabled;
    }

    private void _table_PinnedOrFilterChanged(object sender, System.EventArgs e) => FillFilters();

    private void _table_ViewChanged(object sender, System.EventArgs e) => FillFilters();

    private void btnÄhnliche_Click(object sender, System.EventArgs e) {
        if (IsDisposed || _table?.Database is not { IsDisposed: false } db) { return; }

        if (db.Column.First() is not { IsDisposed: false } co) { return; }

        var fl = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);

        using var fc = new FilterCollection(fl, "ähnliche");

        var r = fc.Rows;
        if (r.Count != 1 || _ähnliche is not { Count: not 0 }) {
            Forms.MessageBox.Show("Aktion fehlgeschlagen", ImageCode.Information, "OK");
            return;
        }

        btnAlleFilterAus_Click(null, System.EventArgs.Empty);
        foreach (var thiscolumnitem in _ähnliche) {
            if (thiscolumnitem?.Column != null && _table?.Filter != null) {
                if (thiscolumnitem.Column.AutoFilterSymbolPossible()) {
                    if (r[0].CellIsNullOrEmpty(thiscolumnitem.Column)) {
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, string.Empty);
                        _table.Filter.Add(fi);
                    } else if (thiscolumnitem.Column.MultiLine) {
                        var l = r[0].CellGetList(thiscolumnitem.Column).SortedDistinctList();
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, l);
                        _table.Filter.Add(fi);
                    } else {
                        var l = r[0].CellGetString(thiscolumnitem.Column);
                        var fi = new FilterItem(thiscolumnitem.Column, FilterType.Istgleich_UND_GroßKleinEgal, l);
                        _table.Filter.Add(fi);
                    }
                }
            }
        }

        btnÄhnliche.Enabled = false;
    }

    private void btnAlleFilterAus_Click(object? sender, System.EventArgs e) {
        _lastLooked = string.Empty;
        if (_table?.Database != null) {
            _table.Filter.Clear();
        }
    }

    private void btnPin_Click(object sender, System.EventArgs e) => _table?.Pin(_table.RowsVisibleUnique());

    private void btnPinZurück_Click(object sender, System.EventArgs e) {
        _lastLooked = string.Empty;
        _table?.Pin(null);
    }

    private void btnTextLöschen_Click(object sender, System.EventArgs e) => txbZeilenFilter.Text = string.Empty;

    private void DoÄhnlich() {
        if (IsDisposed || _table?.Database is not { IsDisposed: false } db || db.Column.Count == 0) { return; }

        var col = db.Column.First();

        if (col == null) { return; } // Neue Datenbank?

        var fi = new FilterItem(col, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);
        using var fc = new FilterCollection(fi, "doähnliche");

        var r = fc.Rows;
        if (_ähnliche != null) {
            btnÄhnliche.Visible = true;
            btnÄhnliche.Enabled = r.Count == 1;
        } else {
            btnÄhnliche.Visible = false;
        }

        if (AutoPin && r.Count == 1) {
            if (_lastLooked != r[0].CellFirstString()) {
                if (_table.RowsFilteredAndPinned().Get(r[0]) == null) {
                    if (Forms.MessageBox.Show("Die Zeile wird durch Filterungen <b>ausgeblendet</b>.<br>Soll sie zusätzlich <b>angepinnt</b> werden?", ImageCode.Pinnadel, "Ja", "Nein") == 0) {
                        _table.PinAdd(r[0]);
                    }
                    _lastLooked = r[0].CellFirstString();
                }
            }
        }
    }

    private void Filter_ZeilenFilterSetzen() {
        if (IsDisposed || _table is not { Database: { IsDisposed: false } db }) {
            DoÄhnlich();
            return;
        }
        var isF = _table.Filter.RowFilterText;
        var newF = txbZeilenFilter.Text;
        if (string.Equals(isF, newF, StringComparison.OrdinalIgnoreCase)) { return; }
        if (string.IsNullOrEmpty(newF)) {
            _table.Filter.Remove_RowFilter();
            DoÄhnlich();
            return;
        }

        var l = new List<ColumnItem>();
        foreach (var thisCo in db.Column) {
            if (thisCo is { IsInCache: null, IgnoreAtRowFilter: false }) { l.Add(thisCo); }
        }

        db.RefreshColumnsData(l.ToArray());

        _table.Filter.RowFilterText = newF;
        DoÄhnlich();
    }

    private void Filterleiste_SizeChanged(object sender, System.EventArgs e) => FillFilters();

    private FlexiControlForFilter? FlexiItemOf(ColumnItem column) {
        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) {
                if (flx.FilterSingleColumn == column) { return flx; }
            }
        }
        return null;
    }

    private void FlexSingeFilter_FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        if (sender is not FilterCollection fo) { return; }
        if (IsDisposed || _table?.Database is not { IsDisposed: false }) { return; }

        if (fo.Count == 0) {
            if (_prevFilter is { Count: 1 }) {
                if (_prevFilter[0] is { IsDisposed: false } fi) {
                    _table.Filter.Remove(fi.Column);
                }
            }
        } else {
            if (fo[0] is { IsDisposed: false } fi) {
                _table.Filter.RemoveOtherAndAdd(fi);
            }
        }

        _prevFilter?.Dispose();
        _prevFilter = null;
    }

    private void FlexSingeFilter_FilterOutput_PropertyChanging(object sender, System.EventArgs e) {
        _prevFilter?.Dispose();
        _prevFilter = null;

        if (sender is not FilterCollection fo) { return; }

        _prevFilter = (FilterCollection?)fo.Clone("FlexSingeFilter_FilterOutput_PropertyChanging");

        //if (fo.Count != 1) { return; }
        //if (_table?.Database is not Database db || db.IsDisposed) { return; }

        //if (fo[0] is not FilterItem fi) { return; }

        //_table.Filter.Remove(fi.Column);
    }

    //private void FilterOutput_Changing(object sender, System.EventArgs e) => throw new NotImplementedException();
    private void GetÄhnlich() {
        if (_table?.Database is not { } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);

        _ähnliche = tcvc.Get(_ähnlicheAnsichtName);
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