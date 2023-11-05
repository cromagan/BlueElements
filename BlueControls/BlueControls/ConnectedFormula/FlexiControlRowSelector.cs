﻿// Authors:
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
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using ComboBox = BlueControls.Controls.ComboBox;

namespace BlueControls.ConnectedFormula;

internal class FlexiControlRowSelector : FlexiControl, IControlSendRow, IControlAcceptFilter, ICalculateRows {

    #region Fields

    private readonly List<IControlAcceptRow> _childs = new();

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
            _showformat = "~" + fc.KeyName + "~";
        }
    }

    #endregion

    #region Properties

    public List<IControlSendFilter> GetFilterFrom { get; } = new();
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

    public List<RowItem> RowsFiltered => this.RowsFiltered(ref _filteredRows, this.FilterOfSender(), InputDatabase);

    #endregion

    #region Methods

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
        Row = string.IsNullOrEmpty(Value) ? null : OutputDatabase?.Row.SearchByKey(Value);
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

        List<AbstractListItem> ex = new();
        ex.AddRange(cb.Item);

        #region Zeilen erzeugen

        var f = RowsFiltered;
        if (f != null) {
            foreach (var thisR in f) {
                if (cb?.Item?[thisR.KeyName] == null) {
                    var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
                    _ = cb?.Item?.Add(tmpQuickInfo, thisR.KeyName);
                    //cb.Item.Add(thisR, string.Empty);
                } else {
                    foreach (var thisIt in ex) {
                        if (thisIt.KeyName == thisR.KeyName) {
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