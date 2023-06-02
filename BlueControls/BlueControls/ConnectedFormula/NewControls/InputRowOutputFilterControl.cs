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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : Caption, IControlAcceptRow, IControlSendFilter {

    #region Fields

    private readonly List<IControlAcceptFilter> _childs = new();

    private readonly ColumnItem? _inputcolumn;
    private readonly ColumnItem? _outputcolumn;
    private readonly FilterTypeRowInputItem _type;
    private FilterItem? _filter;
    private IControlSendRow? _getRowFrom;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(ColumnItem? inputcolumn, ColumnItem? outputcolumn, FilterTypeRowInputItem type) {
        _inputcolumn = inputcolumn;
        _outputcolumn = outputcolumn;
        _type = type;
        //Visible = false;
    }

    #endregion

    #region Properties

    public FilterItem? Filter {
        get => _filter;
        set {
            if (_filter?.Equals(value) ?? false) { return; }
            _filter = value;
            this.DoChilds(_childs);

            if (_filter != null) {
                Text = "Filter: " + _filter.ReadableText();
            } else {
                Text = string.Empty;
            }

            Invalidate();
        }
    }

    public IControlSendRow? GetRowFrom {
        get => _getRowFrom;
        set {
            if (_getRowFrom == value) { return; }
            if (_getRowFrom != null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Änderung nicht erlaubt");
            }

            _getRowFrom = value;
            if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
        }
    }

    public RowItem? LastInputRow { get; private set; }
    public DatabaseAbstract? OutputDatabase { get; set; }

    #endregion

    #region Methods

    public void ChildAdd(IControlAcceptFilter c) {
        if (IsDisposed) { return; }
        _childs.AddIfNotExists(c);
        this.DoChilds(_childs);
    }

    public void SetData(DatabaseAbstract? database, long? rowkey) {
        if (database == null || _inputcolumn == null || _outputcolumn == null) {
            Filter = new FilterItem();
            return;
        }

        var nr = database.Row.SearchByKey(rowkey);
        if (nr == LastInputRow) { return; }

        LastInputRow = nr;
        LastInputRow?.CheckRowDataIfNeeded();

        if (LastInputRow == null) {
            Filter = new FilterItem();
            return;
        }

        switch (_type) {
            case FilterTypeRowInputItem.Ist_GrossKleinEgal:
                Filter = new FilterItem(_outputcolumn, BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, LastInputRow.CellGetString(_inputcolumn));
                return;

            case FilterTypeRowInputItem.Ist_genau:
                Filter = new FilterItem(_outputcolumn, BlueDatabase.Enums.FilterType.Istgleich, LastInputRow.CellGetString(_inputcolumn));
                return;

            case FilterTypeRowInputItem.Ist_eines_der_Wörter_GrossKleinEgal:
                var list = LastInputRow.CellGetString(_inputcolumn).HtmlSpecialToNormalChar(false).AllWords().SortedDistinctList();
                Filter = new FilterItem(_outputcolumn, BlueDatabase.Enums.FilterType.Istgleich_ODER_GroßKleinEgal, list);

                //List<string> names = new();
                //names.AddRange(_outputcolumn.GetUcaseNamesSortedByLenght());

                //var tmpvalue = LastInputRow.CellGetString(_outputcolumn);

                //foreach (var thisWord in names) {
                //    var fo = tmpvalue.IndexOfWord(thisWord, 0, RegexOptions.IgnoreCase);
                //    if (fo > -1) {
                //        value.Add(thisWord);
                //    }
                //}
                return;

            default:
                Develop.DebugPrint(_type);
                Filter = new FilterItem();
                return;
        }
    }

    #endregion
}