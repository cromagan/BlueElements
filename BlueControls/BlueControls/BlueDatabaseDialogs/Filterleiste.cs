// Authors:
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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;
using GroupBox = BlueControls.Controls.GroupBox;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public partial class Filterleiste : GroupBox //  System.Windows.Forms.UserControl //
{
    #region Fields

    private ColumnViewCollection? _ähnliche;
    private string _ähnlicheAnsichtName = string.Empty;
    private bool _isFilling;
    private string _lastLooked = string.Empty;
    private Table? _table;

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

    public bool Textbox_hasFocus() => txbZeilenFilter.Focused;

    internal void FillFilters() {
        if (IsDisposed) { return; }

        if (InvokeRequired) {
            _ = Invoke(new Action(FillFilters));
            return;
        }
        if (_isFilling) { return; }
        _isFilling = true;

        btnPinZurück.Enabled = _table?.Database != null && _table.PinnedRows != null && _table.PinnedRows.Count > 0;

        #region ZeilenFilter befüllen

        txbZeilenFilter.Text = _table != null &&
            _table.Database != null &&
            _table.Filter != null &&
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
        var anchor = AnchorStyles.Top | AnchorStyles.Left;
        var down = 0;

        #endregion

        List<FlexiControlForFilter> flexsToDelete = new();

        #region Vorhandene Flexis ermitteln

        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) { flexsToDelete.Add(flx); }
        }

        #endregion

        var cu = _table?.CurrentArrangement;

        #region Neue Flexis erstellen / updaten

        if (_table?.Database != null && _table.Filter != null) {
            List<ColumnItem> columSort = new();
            ColumnViewCollection? orderArrangement = _table.Database.ColumnArrangements.Get(AnsichtName);

            #region Reihenfolge der Spalten bestimmen

            if (orderArrangement != null) {
                foreach (var thisclsVitem in orderArrangement) {
                    if (thisclsVitem?.Column is ColumnItem ci) { _ = columSort.AddIfNotExists(ci); }
                }
            }

            if (cu != null) {
                foreach (var thisclsVitem in cu) {
                    if (thisclsVitem?.Column is ColumnItem ci) { _ = columSort.AddIfNotExists(ci); }
                }
            }

            foreach (var thisColumn in _table.Database.Column) {
                _ = columSort.AddIfNotExists(thisColumn);
            }

            #endregion

            foreach (var thisColumn in columSort) {
                var showMe = false;
                var viewItemOrder = orderArrangement?[thisColumn];
                var viewItemCurrent = cu?[thisColumn];
                var filterItem = _table.Filter[thisColumn];

                #region Sichtbarkeit des Filterelemts bestimmen

                if (thisColumn.AutoFilterSymbolPossible()) {
                    if (viewItemOrder != null && Filtertypes.HasFlag(FilterTypesToShow.NachDefinierterAnsicht)) { showMe = true; }
                    if (viewItemCurrent != null && filterItem != null && Filtertypes.HasFlag(FilterTypesToShow.AktuelleAnsicht_AktiveFilter)) { showMe = true; }
                }

                #endregion

                if (filterItem == null && showMe) {
                    // Dummy-Filter, nicht in der Collection
                    filterItem = new FilterItem(thisColumn, FilterType.Instr_GroßKleinEgal, string.Empty);
                }

                if (filterItem != null && showMe) {
                    var flx = FlexiItemOf(filterItem);
                    if (flx != null) {
                        // Sehr Gut, Flex vorhanden, wird später nicht mehr gelöscht
                        _ = flexsToDelete.Remove(flx);
                    } else {
                        // Na gut, eben neuen Flex erstellen
                        flx = new FlexiControlForFilter(_table, filterItem);
                        flx.ValueChanged += Flx_ValueChanged;
                        flx.ButtonClicked += Flx_ButtonClicked;
                        Controls.Add(flx);
                    }

                    if (leftpos + constwi > Width) {
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
                }
            }
        }

        #endregion

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

    private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
        if (_table?.Filter == null) { return; }

        _table.Filter.Remove(e.Column);
        if (e.Comand != "Filter") { return; }

        if (e.Filter == null) { return; }
        _table.Filter.Add(e.Filter);
    }

    //private void btnAdmin_Click(object sender, System.EventArgs e) {
    //    BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
    //    frmTableView x = new(_TableView.Database, false, true);
    //    x.ShowDialog();
    //    x?.Dispose();
    //    BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
    //}

    private void btnÄhnliche_Click(object sender, System.EventArgs e) {
        if (_table?.Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (db.Column.First() is not ColumnItem co) { return; }

        var fl = new FilterItem(co, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);

        var fc = new FilterCollection(fl);

        var r = fc.Rows;
        if (r.Count != 1 || _ähnliche == null || _ähnliche.Count == 0) {
            MessageBox.Show("Aktion fehlgeschlagen", ImageCode.Information, "OK");
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
        if (_table?.Database != null && _table.Filter != null) {
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
        if (_table?.Database is not DatabaseAbstract db || db.Column.Count == 0) { return; }

        var col = db.Column.First();

        if (col == null) { return; } // Neue Datenbank?

        var fi = new FilterItem(col, FilterType.Istgleich_GroßKleinEgal_MultiRowIgnorieren, txbZeilenFilter.Text);
        var fc = new FilterCollection(fi);

        var r = fc.Rows;
        if (_ähnliche != null) {
            btnÄhnliche.Visible = true;
            btnÄhnliche.Enabled = r != null && r.Count == 1;
        } else {
            btnÄhnliche.Visible = false;
        }

        if (AutoPin && r != null && r.Count == 1) {
            if (_lastLooked != r[0].CellFirstString()) {
                if (_table.RowsFilteredAndPinned().Get(r[0]) == null) {
                    if (MessageBox.Show("Die Zeile wird durch Filterungen <b>ausgeblendet</b>.<br>Soll sie zusätzlich <b>angepinnt</b> werden?", ImageCode.Pinnadel, "Ja", "Nein") == 0) {
                        _table.PinAdd(r[0]);
                    }
                    _lastLooked = r[0].CellFirstString();
                }
            }
        }
    }

    private void Filter_ZeilenFilterSetzen() {
        if (_table == null || _table.Database is not DatabaseAbstract db || db.IsDisposed) {
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
            if (thisCo != null && thisCo.IsInCache == null && !thisCo.IgnoreAtRowFilter) { l.Add(thisCo); }
        }

        db.RefreshColumnsData(l);

        _table.Filter.RowFilterText = newF;
        DoÄhnlich();
    }

    private void Filterleiste_SizeChanged(object sender, System.EventArgs e) => FillFilters();

    private FlexiControlForFilter? FlexiItemOf(FilterItem fi) {
        foreach (var thisControl in Controls) {
            if (thisControl is FlexiControlForFilter flx) {
                if (flx.Filter.ToString() == fi.ToString()) { return flx; }
            }
        }
        return null;
    }

    private void Flx_ButtonClicked(object sender, System.EventArgs e) {
        if (_table?.Filter == null) { return; }

        var f = (FlexiControlForFilter)sender;
        if (f.CaptionPosition == ÜberschriftAnordnung.ohne) {
            // ein Großer Knopf ohne Überschrift, da wird der evl. Filter gelöscht
            _ = _table.Filter.Remove(((FlexiControlForFilter)sender).Filter);
            return;
        }

        if (f.Filter.Column == null) { return; }

        //f.Enabled = false;
        AutoFilter autofilter = new(f.Filter.Column, _table.Filter, _table.PinnedRows);
        var p = f.PointToScreen(Point.Empty);
        autofilter.Position_LocateToPosition(p with { Y = p.Y + f.Height });
        autofilter.Show();
        autofilter.FilterComand += AutoFilter_FilterComand;
        Develop.Debugprint_BackgroundThread();
    }

    private void Flx_ValueChanged(object sender, System.EventArgs e) {
        if (_isFilling) { return; }
        if (sender is FlexiControlForFilter flx) {
            if (flx.EditType == EditTypeFormula.Button) { return; }
            if (_table?.Filter == null) { return; }
            var isFilter = flx.WasThisValueClicked(); //  flx.Value.StartsWith("|");
            //flx.Filter.Herkunft = "Filterleiste";
            var v = flx.Value; //.Trim("|");
            if (_table.Filter.Count == 0 || !_table.Filter.Contains(flx.Filter)) {
                if (isFilter) { flx.Filter.FilterType = FilterType.Istgleich_ODER_GroßKleinEgal; } // Filter noch nicht in der Collection, kann ganz einfach geändert werden
                flx.Filter.Changeto(flx.Filter.FilterType, v);
                _table.Filter.Add(flx.Filter);
                return;
            }
            if (flx.Filter.SearchValue.Count != 1) {
                Develop.DebugPrint_NichtImplementiert();
                return;
            }
            if (isFilter) {
                flx.Filter.Changeto(FilterType.Istgleich_ODER_GroßKleinEgal, v);
            } else {
                if (string.IsNullOrEmpty(v)) {
                    _ = _table.Filter.Remove(flx.Filter);
                } else {
                    flx.Filter.Changeto(FilterType.Instr_GroßKleinEgal, v);
                    // flx.Filter.SearchValue[0] =v;
                }
            }
        }
    }

    private void GetÄhnlich() {
        _ähnliche = _table?.Database?.ColumnArrangements.Get(_ähnlicheAnsichtName);
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