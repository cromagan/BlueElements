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
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

// ReSharper disable All

namespace BlueDatabase;

public sealed class RowSortDefinition : IParseable, IChangedFeedback {

    #region Fields

    public List<ColumnItem> Columns = new();
    public DatabaseAbstract Database;

    #endregion

    #region Constructors

    public RowSortDefinition(DatabaseAbstract database, string toParse) {
        Database = database;
        Parse(toParse);
    }

    public RowSortDefinition(DatabaseAbstract database, string columnName, bool reverse) {
        Database = database;
        Initialize();
        Reverse = reverse;
        SetColumn(new List<string> { columnName });
    }

    public RowSortDefinition(DatabaseAbstract database, List<string> columnNames, bool reverse) {
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

    public void Parse(string toParse) {
        Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "identifier":
                    if (pair.Value != "SortDefinition") { Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                    break;

                case "direction":
                    Reverse = pair.Value == "Z-A";
                    break;

                case "reverse":
                    Reverse = pair.Value.FromPlusMinus();
                    break;

                case "column":
                case "columnname": // ColumnName wichtig wegen CopyLayout
                    if (Database.Column.Exists(pair.Value) is ColumnItem c) { Columns.Add(c); }
                    break;

                case "columns":
                    var cols = pair.Value.FromNonCritical().SplitBy("|");
                    foreach (var thisc in cols) {
                        if (Database.Column.Exists(thisc) is ColumnItem c2) { Columns.Add(c2); }
                    }
                    break;

                case "columnkey":
                    //if (Database.Column.SearchByKey(LongParse(pair.Value)) is ColumnItem c2) { Columns.Add(c2); }
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
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
            var c = Database.Column.Exists(t);
            if (c != null && !c.IsDisposed) { Columns.Add(c); }
        }
    }

    #endregion
}