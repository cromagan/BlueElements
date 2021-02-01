#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;

namespace BlueDatabase {

    public sealed class RowSortDefinition : IParseable {
        #region  Variablen-Deklarationen 

        public Database Database;
        private readonly ListExt<ColumnItem> _Columns = new ListExt<ColumnItem>();

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 

        private void Initialize() {
            Reverse = false;
            _Columns.Clear();
        }

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


        #region  Properties 
        public bool IsParsing { get; private set; }

        public bool Reverse { get; private set; }

        public List<ColumnItem> Columns {
            get {
                return _Columns;
            }
        }

        #endregion




        public override string ToString() {

            var Result = "{";

            if (Reverse) {
                Result += "Direction=Z-A";
            }
            else {
                Result += "Direction=A-Z";
            }

            if (_Columns != null) {
                foreach (var ThisColumn in _Columns) {
                    if (ThisColumn != null) {
                        Result = Result + ", " + ThisColumn.ParsableColumnKey();
                    }
                }
            }

            return Result + "}";
        }


        public void Parse(string ToParse) {
            IsParsing = true;
            Initialize();
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "identifier":
                        if (pair.Value != "SortDefinition") { Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                        break;

                    case "direction":
                        Reverse = Convert.ToBoolean(pair.Value == "Z-A");
                        break;

                    case "column":
                    case "columnname": // Columname wichtig wegen CopyLayout
                        _Columns.Add(Database.Column[pair.Value]);
                        break;

                    case "columnkey":
                        _Columns.Add(Database.Column.SearchByKey(int.Parse(pair.Value)));
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            IsParsing = false;
        }


        private void SetColumn(List<string> names) {
            _Columns.Clear();
            for (var z = 0; z < names.Count; z++) {
                var c = Database.Column.Exists(names[z]);

                if (c != null) { _Columns.Add(c); }
            }
        }

        public bool UsedForRowSort(ColumnItem vcolumn) {

            if (_Columns.Count == 0) { return false; }

            foreach (var ThisColumn in _Columns) {
                if (ThisColumn == vcolumn) { return true; }
            }
            return false;
        }

        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


    }
}