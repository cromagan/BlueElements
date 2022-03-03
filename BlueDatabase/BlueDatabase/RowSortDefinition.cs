// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueDatabase {

    public sealed class RowSortDefinition : IParseable {

        #region Fields

        public Database Database;
        private readonly ListExt<ColumnItem?> _columns = new();

        #endregion

        #region Constructors

        public RowSortDefinition(Database database, string code) {
            Database = database;
            Parse(code);
        }

        public RowSortDefinition(Database database, string columnName, bool reverse) {
            Database = database;
            Initialize();
            Reverse = reverse;
            SetColumn(new List<string>() { columnName });
        }

        public RowSortDefinition(Database database, List<string> columnNames, bool reverse) {
            Initialize();
            Database = database;
            Reverse = reverse;
            SetColumn(columnNames);
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public List<ColumnItem?> Columns => _columns;

        public bool IsParsing { get; private set; }

        public bool Reverse { get; private set; }

        #endregion

        #region Methods

        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Parse(string toParse) {
            IsParsing = true;
            Initialize();
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "identifier":
                        if (pair.Value != "SortDefinition") { Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                        break;

                    case "direction":
                        Reverse = Convert.ToBoolean(pair.Value == "Z-A");
                        break;

                    case "column":

                    case "columnname": // Columname wichtig wegen CopyLayout
                        _columns.Add(Database.Column[pair.Value]);
                        break;

                    case "columnkey":
                        _columns.Add(Database.Column.SearchByKey(long.Parse(pair.Value)));
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
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
            if (_columns != null) {
                foreach (var thisColumn in _columns) {
                    if (thisColumn != null) {
                        result = result + ", " + thisColumn.ParsableColumnKey();
                    }
                }
            }
            return result + "}";
        }

        public bool UsedForRowSort(ColumnItem? vcolumn) {
            if (_columns.Count == 0) { return false; }

            return _columns.Any(thisColumn => thisColumn == vcolumn);
        }

        private void Initialize() {
            Reverse = false;
            _columns.Clear();
        }

        private void SetColumn(List<string> names) {
            _columns.Clear();
            foreach (var t in names) {
                var c = Database.Column.Exists(t);
                if (c != null) { _columns.Add(c); }
            }
        }

        #endregion
    }
}