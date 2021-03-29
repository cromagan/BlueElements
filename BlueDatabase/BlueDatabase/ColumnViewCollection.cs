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
using BlueDatabase.Enums;
using System.Collections.Generic;

namespace BlueDatabase {
    public sealed class ColumnViewCollection : ListExt<ColumnViewItem>, IParseable //, IEnumerable
    {
        //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)

        #region  Variablen-Deklarationen 

        public readonly Database Database;
        private string _Name;

        #endregion

        #region  Construktor + Initialize 

        private void Initialize() {
            _Name = string.Empty;
            PermissionGroups_Show.Clear();
            PermissionGroups_Show.Changed += _PermissionGroups_Show_ListOrItemChanged;
        }

        public ColumnViewCollection(Database database, string code) {
            Database = database;
            Parse(code);

        }

        public ColumnViewCollection(Database database, string code, string newname) {
            Database = database;
            Parse(code);
            _Name = newname;
        }


        #endregion

        #region  Properties 
        public bool IsParsing { get; private set; }

        public string Name {
            get => _Name;
            set {
                if (_Name == value) { return; }
                _Name = value;
                OnChanged();
            }
        }


        public ListExt<string> PermissionGroups_Show { get; } = new ListExt<string>();


        public ColumnViewItem this[ColumnItem vColumn] {

            get {
                if (vColumn == null) { return null; }

                foreach (var ThisViewItem in this) {
                    if (ThisViewItem != null && ThisViewItem.Column == vColumn) { return ThisViewItem; }
                }

                return null;
            }
        }

        #endregion



        private void _PermissionGroups_Show_ListOrItemChanged(object sender, System.EventArgs e) {
            OnChanged();
        }
        public void Add(ColumnItem Column, bool Permanent) {
            if (Permanent) {
                Add(new ColumnViewItem(Column, enViewType.PermanentColumn));
            } else {
                Add(new ColumnViewItem(Column, enViewType.Column));
            }
        }


        public void Insert(int index, ColumnItem Column) {
            Insert(index, new ColumnViewItem(Column, enViewType.Column));
        }

        public void Parse(string ToParse) {
            IsParsing = true;
            ThrowEvents = false;
            PermissionGroups_Show.ThrowEvents = false;
            Initialize();

            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "name":
                        _Name = pair.Value;
                        break;
                    case "columndata":
                        Add(new ColumnViewItem(Database, pair.Value));
                        break;
                    case "permissiongroup":
                        PermissionGroups_Show.Add(pair.Value);
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }


            PermissionGroups_Show.ThrowEvents = true;
            ThrowEvents = true;
            IsParsing = false;
        }

        public override string ToString() {
            var Result = "{Name=" + _Name.ToNonCritical();

            foreach (var ThisViewItem in this) {
                if (ThisViewItem != null) {
                    Result = Result + ", Columndata=" + ThisViewItem;
                }
            }

            var tmp = PermissionGroups_Show.SortedDistinctList();
            tmp.RemoveString("#Administrator", false);

            foreach (var t in tmp) {
                if (!string.IsNullOrEmpty(t)) {
                    Result = Result + ", Permissiongroup=" + t;
                }
            }

            return Result + "}";
        }

        public void ShowAllColumns(Database OfDatabase) {
            if (OfDatabase.IsParsing) { return; }

            var OK = true;
            for (var z = 0; z < OfDatabase.Column.Count; z++) {
                if (z >= Count) {
                    OK = false;
                    break;
                }

                if (this[z].Column != OfDatabase.Column[z]) {
                    OK = false;
                    break;
                }
            }

            if (Count > 0 && this[0].ViewType != enViewType.PermanentColumn) { OK = false; }

            if (OK) { return; }

            Clear();

            foreach (var ThisColumnItem in OfDatabase.Column) {
                if (ThisColumnItem != null) {
                    Add(new ColumnViewItem(ThisColumnItem, enViewType.Column));
                }
            }

            if (Count > 0) { this[0].ViewType = enViewType.PermanentColumn; }
        }

        public ColumnItem PreviousVisible(ColumnItem OfColumn) {
            var ViewItemNo = Count - 1;
            var Found = false;

            do {
                if (ViewItemNo < 0) { return null; }

                if (this[ViewItemNo] != null && this[ViewItemNo].Column != null) {
                    if (Found) { return this[ViewItemNo].Column; }
                    if (this[ViewItemNo].Column == OfColumn) { Found = true; }
                }

                ViewItemNo--;
            } while (true);
        }

        public ColumnItem NextVisible(ColumnItem OfColumn) {
            var ViewItemNo = 0;
            var Found = false;

            do {
                if (ViewItemNo >= Count) { return null; }

                if (this[ViewItemNo] != null && this[ViewItemNo].Column != null) {
                    if (Found) { return this[ViewItemNo].Column; }
                    if (this[ViewItemNo].Column == OfColumn) { Found = true; }
                }

                ViewItemNo++;
            } while (true);

        }

        public ColumnViewItem PreviousVisible(ColumnViewItem OfViewItem) {
            var ViewItemNo = IndexOf(OfViewItem);

            do {
                ViewItemNo--;
                if (ViewItemNo < 0) { return null; }

                if (this[ViewItemNo] != null && this[ViewItemNo].Column != null) { return this[ViewItemNo]; }

            } while (true);
        }

        public ColumnViewItem NextVisible(ColumnViewItem OfViewItem) {
            var ViewItemNo = IndexOf(OfViewItem);

            if (ViewItemNo < 0) { return null; }


            do {
                ViewItemNo++;
                if (ViewItemNo >= Count) { return null; }

                if (this[ViewItemNo] != null && this[ViewItemNo].Column != null) { return this[ViewItemNo]; }
            } while (true);

        }

        public void Swap(ColumnViewItem View1, ColumnViewItem View2) {
            // Absichtlich anderer Name, um klarzustellen, dass hier nicht der Standard-Swap angewandt wird
            if (View1 == null) { return; }
            if (View2 == null) { return; }
            if (View1 == View2) { return; }

            var Col1 = IndexOf(View1);
            var Col2 = IndexOf(View2);
            if (Col1 < 0 || Col2 < 0) { return; }


            Swap(Col1, Col2);

            if (this[Col1].ViewType != enViewType.PermanentColumn) { this[Col2].ViewType = enViewType.Column; }

        }

        public List<ColumnItem> ListOfUsedColumn() {

            var ColList = new List<ColumnItem>();
            foreach (var t in this) {
                if (t != null) { ColList.Add(t.Column); }
            }

            return ColList;
        }

        public override void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }

            base.OnChanged();
            //Changed?.Invoke(this, System.EventArgs.Empty);
        }


        public void HideSystemColumns() {
            foreach (var ThisViewItem in this) {
                if (ThisViewItem != null) {
                    if (ThisViewItem.Column == null || !string.IsNullOrEmpty(ThisViewItem.Column.Identifier)) {
                        Remove(ThisViewItem);
                        HideSystemColumns();
                        return;
                    }
                }
            }
        }

        public void Hide(string ColumnName) {
            foreach (var ThisViewItem in this) {
                if (ThisViewItem != null) {
                    if (ThisViewItem.Column == null || ThisViewItem.Column.Name.ToUpper() == ColumnName.ToUpper()) {
                        Remove(ThisViewItem);
                        Hide(ColumnName);
                        return;
                    }
                }
            }
        }


        internal void Repair() {

            if (this == null || Count == 0) { return; }

            for (var z = 0; z < Count; z++) {
                if (this[z].Column == null || !Database.Column.Contains(this[z].Column)) {
                    this[z] = null;
                }
            }

            this.RemoveNull();

            var tmp = PermissionGroups_Show.SortedDistinctList();
            tmp.RemoveString("#Administrator", false);
            tmp.RemoveNullOrEmpty();

            if (PermissionGroups_Show.IsDifferentTo(tmp)) {
                PermissionGroups_Show.Clear();
                PermissionGroups_Show.AddRange(tmp);
            }
        }
    }
}