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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using ComboBox = BlueControls.Controls.ComboBox;

namespace BlueControls.ConnectedFormula;

internal class FlexiControlRowSelector : FlexiControl, IControlSendRow, IControlAcceptFilter, ICalculateRows {

    #region Fields

    private readonly List<IControlAcceptRow> _childs = new();
    private readonly List<IControlSendFilter> _getFilterFrom = new();
    private readonly string _showformat;
    private List<RowItem>? _filteredRows;
    private RowItem? _row;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(DatabaseAbstract? database, string caption, string showFormat) : base() {
        CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database != null && database.Column.Count > 0 && database.Column.First() is ColumnItem fc) {
            _showformat = "~" + fc.Name + "~";
        }
    }

    #endregion

    #region Properties

    public List<RowItem> FilteredRows => this.CalculateFilteredRows(ref _filteredRows, this.FilterOfSender(), this.InputDatabase);
    public ReadOnlyCollection<IControlSendFilter> GetFilterFrom => new(_getFilterFrom);
    public DatabaseAbstract? InputDatabase { get; set; }
    public DatabaseAbstract? OutputDatabase { get; set; }

    public RowItem? Row {
        get => IsDisposed ? null : _row;
        private set {
            if (IsDisposed) { return; }
            if (value == _row) { return; }
            _row = value;
            this.DoChilds(_childs, _row);
        }
    }

    #endregion

    #region Methods

    public void AddGetFilterFrom(IControlSendFilter item) {
        _getFilterFrom.AddIfNotExists(item);
        FilterFromParentsChanged();
        item.ChildAdd(this);
    }

    public void ChildAdd(IControlAcceptRow c) {
        if (IsDisposed) { return; }
        _childs.AddIfNotExists(c);
        this.DoChilds(_childs, _row);
    }

    public void FilterFromParentsChanged() => Invalidate_FilteredRows();

    public void Invalidate_FilteredRows() {
        if (IsDisposed) { return; }
        _filteredRows = null;
        Invalidate();
    }

    //public void SetData(DatabaseAbstract? database, long? rowkey) {
    //    if (_disposing || IsDisposed) { return; }

    //    if (OutputDatabase != database) {
    //        return;
    //        //            Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken inkonsitent!");
    //    }

    //    BlueBasics.Develop.DebugPrint_NichtImplementiert();

    //    //if (FilterDefiniton == null || FilterDefiniton.IsDisposed) { return; }

    //    //if (FilterDefiniton.Row.Count > 0) {
    //    //    #region Filter erstellen

    //    //    var f = new FilterCollection(OutputDatabase);

    //    //    foreach (var thisR in FilterDefiniton.Row) {
    //    //        #region Column ermitteln

    //    //        var column = OutputDatabase?.Column.Exists(thisR.CellGetString("Spalte"));
    //    //        if (Column  ==null || Column .IsDisposed) {
    //    //            return;
    //    //        }

    //    //        #endregion

    //    //        #region Type ermitteln

    //    //        var onlyifhasvalue = false;
    //    //        var word = false;

    //    //        FilterType ft;
    //    //        switch (thisR.CellGetString("Filterart").ToLower()) {
    //    //            case "=":
    //    //                ft = FilterType.Istgleich_GroßKleinEgal;
    //    //                break;

    //    //            case "=!empty":
    //    //                ft = FilterType.Istgleich_GroßKleinEgal;
    //    //                onlyifhasvalue = true;
    //    //                break;

    //    //            case "=word":
    //    //                ft = FilterType.Istgleich_ODER_GroßKleinEgal;
    //    //                word = true;
    //    //                break;

    //    //            default:
    //    //                ft = FilterType.Istgleich_GroßKleinEgal;
    //    //                DebugPrint("Filter " + thisR.CellGetInteger("Filterart") + " nicht definiert.");
    //    //                break;
    //    //        }

    //    //        #endregion

    //    //        #region Value ermitteln

    //    //        var calcrows = false;

    //    //        List<string> value = new();

    //    //        if (otherdatabase?.Row.SearchByKey(rowkey) is RowItem r) {
    //    //            r.CheckRowDataIfNeeded();
    //    //            calcrows = true;

    //    //            var tmpvalue = thisR.CellGetString("suchtxt");

    //    //            if (tmpvalue.Equals("#first", StringComparison.OrdinalIgnoreCase)) {
    //    //                tmpvalue = r.CellFirstString();
    //    //            } else {
    //    //                tmpvalue = r.ReplaceVariables(tmpvalue, false, false);
    //    //            }

    //    //            if (word) {
    //    //                List<string> names = new();
    //    //                names.AddRange(column.GetUcaseNamesSortedByLenght());

    //    //                foreach (var thisWord in names) {
    //    //                    var fo = tmpvalue.IndexOfWord(thisWord, 0, RegexOptions.IgnoreCase);
    //    //                    if (fo > -1) {
    //    //                        value.Add(thisWord);
    //    //                    }
    //    //                }

    //    //                if (value.Count == 0) {
    //    //                    calcrows = false;
    //    //                }
    //    //            } else {
    //    //                value.Add(tmpvalue); // Immer hinzufügen. Es gibt Einträge, wo der erste Befüllt ist, und der Zweite leer sein kann.
    //    //            }
    //    //        }

    //    //        #endregion

    //    //        if (value.Count > 0 || !onlyifhasvalue) {
    //    //            f.Add(new FilterItem(column, ft, value));
    //    //        }

    //    //        #endregion

    //    //        #region Zeile(n) ermitteln und Script löschen

    //    //        _rows = calcrows ? OutputDatabase?.Row.CalculateFilteredRows(f) : null;

    //    //        #endregion
    //    //    }

    //    //    if (_disposing || IsDisposed) { return; } // Multitasking...
    //    //}

    //    UpdateMyCollection();
    //}

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            //_disposing = true;
            Row = null;

            Tag = null;

            //_disposing = true;
            _childs.Clear();

            _filteredRows = null;
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (_filteredRows == null) {
            UpdateMyCollection();
        }
        base.DrawControl(gr, state);
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();
        Row = string.IsNullOrEmpty(Value) ? null : OutputDatabase?.Row.SearchByKey(LongParse(Value));
    }

    private void UpdateMyCollection() {
        if (IsDisposed) { return; }
        if (!Allinitialized) { _ = CreateSubControls(); }

        #region Combobox suchen

        ComboBox? cb = null;
        foreach (var thiscb in Controls) {
            if (thiscb is ComboBox cbx) { cb = cbx; break; }
        }

        #endregion

        if (cb == null) { return; }

        List<BasicListItem> ex = new();
        ex.AddRange(cb.Item);

        #region Zeilen erzeugen

        var f = FilteredRows;
        if (f != null) {
            foreach (var thisR in f) {
                if (cb?.Item?[thisR.Key.ToString()] == null) {
                    var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
                    _ = cb?.Item?.Add(tmpQuickInfo, thisR.Key.ToString());
                    //cb.Item.Add(thisR, string.Empty);
                } else {
                    foreach (var thisIt in ex) {
                        if (thisIt.KeyName == thisR.Key.ToString()) {
                            _ = ex.Remove(thisIt);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            cb?.Item?.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (cb?.Item != null && cb.Item.Count == 1) {
            ValueSet(cb.Item[0].KeyName, true, true);
        }

        if (cb?.Item == null || cb.Item.Count < 2) {
            DisabledReason = "Keine Auswahl möglich.";
        } else {
            DisabledReason = string.Empty;
        }

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb?.Item?[Value] == null) {
            ValueSet(string.Empty, true, true);
        }

        #endregion
    }

    #endregion
}