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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Converter;

// ReSharper disable All

namespace BlueDatabase;

public sealed class RowSortDefinition : IParseable {

    #region Fields

    public readonly ListExt<ColumnItem> Columns = new();
    public DatabaseAbstract Database;

    #endregion

    #region Constructors

    public RowSortDefinition(DatabaseAbstract database, string code) {
        Database = database;
        Parse(code);
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

    public bool IsParsing { get; private set; }

    public bool Reverse { get; private set; }

    #endregion

    #region Methods

    public void OnChanged() {
        if (IsParsing) { Develop.DebugPrint(FehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void Parse(string toParse) {
        IsParsing = true;
        Initialize();
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "identifier":
                    if (pair.Value != "SortDefinition") { Develop.DebugPrint(FehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                    break;

                case "direction":
                    Reverse = pair.Value == "Z-A";
                    break;

                case "column":
                case "columnname": // ColumnName wichtig wegen CopyLayout
                    if (Database.Column.Exists(pair.Value) is ColumnItem c) { Columns.Add(c); }

                    break;

                case "columnkey":
                    if (Database.Column.SearchByKey(LongParse(pair.Value)) is ColumnItem c2) { Columns.Add(c2); }
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        IsParsing = false;
    }

    public override string ToString() {
        var result = "{";
        if (Reverse) {
            result += "Direction=Z-A";
        } else {
            result += "Direction=A-Z";
        }

        foreach (var thisColumn in Columns) {
            if (thisColumn != null) {
                result = result + ", ColumnName=" + thisColumn.Name;
            }
        }

        return result + "}";
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
            if (c != null) { Columns.Add(c); }
        }
    }

    #endregion
}