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
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable All

namespace BlueDatabase;

public sealed class RowSortDefinition : IParseable, IChangedFeedback {

    #region Fields

    public List<ColumnItem> Columns = [];

    public Database Database;

    #endregion

    #region Constructors

    public RowSortDefinition(Database database, string toParse) {
        Database = database;
        this.Parse(toParse);
    }

    public RowSortDefinition(Database database, string columnName, bool reverse) {
        Database = database;
        Initialize();
        Reverse = reverse;
        SetColumn(new List<string> { columnName });
    }

    public RowSortDefinition(Database database, List<string> columnNames, bool reverse) {
        Initialize();
        Database = database;
        Reverse = reverse;
        SetColumn(columnNames);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public bool Reverse { get; private set; }

    #endregion

    #region Methods

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "SortDefinition") { Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + value); }
                return true;

            case "direction":
                Reverse = value == "Z-A";
                return true;

            case "reverse":
                Reverse = value.FromPlusMinus();
                return true;

            case "column":
            case "columnname": // ColumnName wichtig wegen CopyLayout
                if (Database.Column[value] is ColumnItem c) { Columns.Add(c); }
                return true;

            case "columns":
                var cols = value.FromNonCritical().SplitBy("|");
                foreach (var thisc in cols) {
                    if (Database.Column[thisc] is ColumnItem c2) { Columns.Add(c2); }
                }
                return true;

            case "columnkey":
                //if (Database.Column.SearchByKey(LongParse(pair.Value)) is ColumnItem c2) { Columns.Add(c2); }
                return true;
        }

        return false;
    }

    public void Repair() {
        if (Columns.Count == 0) { return; }

        for (var i = 0; i < Columns.Count; i++) {
            if (Columns[i] is not ColumnItem c || c.IsDisposed) {
                Columns.RemoveAt(i);
                OnChanged();
                Repair();
            }
        }
    }

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Reverse", Reverse);
        result.ParseableAdd("Columns", Columns);
        return result.Parseable();
    }

    public bool UsedForRowSort(ColumnItem? vcolumn) {
        if (Columns.Count == 0) { return false; }

        return Columns.Any(thisColumn => thisColumn == vcolumn);
    }

    private void Initialize() {
        Reverse = false;
        Columns.Clear();
    }

    private void SetColumn(List<string> names) {
        Columns.Clear();
        foreach (var t in names) {
            var c = Database.Column[t];
            if (c != null && !c.IsDisposed) { Columns.Add(c); }
        }
    }

    #endregion
}