// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlueTable;

public sealed class RowSortDefinition : IParseable {

    #region Fields

    private readonly List<ColumnItem> _internal = [];

    #endregion

    #region Constructors

    public RowSortDefinition(Table table, string toParse) {
        Table = table;
        this.Parse(toParse);
    }

    public RowSortDefinition(Table table, ColumnItem? colum, bool reverse) {
        Table = table;
        Reverse = reverse;

        if (colum is { IsDisposed: false }) { _internal.Add(colum); }
    }

    public RowSortDefinition(Table table, List<ColumnItem?>? column, bool reverse) {
        Table = table;
        Reverse = reverse;

        if (column != null) {
            foreach (var thisColumn in column) {
                if (thisColumn is { IsDisposed: false } c) { _internal.Add(c); }
            }
        }
    }

    #endregion

    #region Properties

    public bool Reverse { get; private set; }
    public Table Table { get; }
    public ReadOnlyCollection<ColumnItem> UsedColumns => _internal.AsReadOnly();

    #endregion

    #region Methods

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Reverse", Reverse);
        result.ParseableAdd("Columns", _internal, true);
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "SortDefinition") { Develop.DebugPrint(ErrorType.Error, "Identifier fehlerhaft: " + value); }
                return true;

            case "direction":
                Reverse = value == "Z-A";
                return true;

            case "reverse":
                Reverse = value.FromPlusMinus();
                return true;

            case "column":
            case "columnname": // ColumnName wichtig wegen CopyLayout
                if (Table.Column[value] is { } c) { _internal.Add(c); }
                return true;

            case "columns":
                var cols = value.FromNonCritical().SplitBy("|");
                foreach (var thisc in cols) {
                    if (Table.Column[thisc] is { } c2) { _internal.Add(c2); }
                }
                return true;

            case "columnkey":
                //if (Table.Column.GetByKey(LongParse(pair.Value)) is ColumnItem c2) { Columns.Add(c2); }
                return true;
        }

        return false;
    }

    public void Repair() {
        if (_internal.Count == 0) { return; }

        if (Table is not { IsDisposed: false } tb) { return; }

        if (!tb.IsEditable(false)) { return; }

        for (var i = 0; i < _internal.Count; i++) {
            if (_internal[i] is not { IsDisposed: false }) {
                _internal.RemoveAt(i);
                //OnPropertyChanged(string propertyname);
                Repair();
                return;
            }
        }
    }

    public List<RowItem> SortedRows(IEnumerable<RowItem> rows) {
        var sortedList = rows.OrderBy(item => item.CompareKey(_internal)).ToList();

        if (Reverse) { sortedList.Reverse(); }
        return sortedList;
    }

    public override string ToString() => ParseableItems().FinishParseable();

    public bool UsedForRowSort(ColumnItem? column) => _internal.Count != 0 && _internal.Any(thisColumn => thisColumn == column);

    #endregion
}