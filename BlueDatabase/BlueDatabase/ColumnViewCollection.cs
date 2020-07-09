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
using System;
using System.Collections;
using System.Collections.Generic;

namespace BlueDatabase
{
    public sealed class ColumnViewCollection : IEnumerable<ColumnViewItem>, IParseable, IEnumerable
    {
        //NICHT IReadableText, das gibt zu viele Probleme (Dropdownboxen)



        #region  Variablen-Deklarationen 
        private readonly ListExt<ColumnViewItem> _Internal = new ListExt<ColumnViewItem>();

        private readonly Database _DatabaseForParse;

        private string _Name;

        #endregion

        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion

        #region  Construktor + Initialize 

        private void Initialize()
        {
            _Name = string.Empty;
            PermissionGroups_Show.Clear();
            PermissionGroups_Show.ListOrItemChanged += _PermissionGroups_Show_ListOrItemChanged;


            _Internal.Clear();
            _Internal.ListOrItemChanged += Internal_ListOrItemChanged;


        }

        private void Internal_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }


        private void _PermissionGroups_Show_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }

        public ColumnViewCollection(Database Database, string Code)
        {
            _DatabaseForParse = Database;
            Parse(Code);
            _DatabaseForParse = null;
        }

        public ColumnViewCollection(Database Database, string Code, string NewName)
        {
            _DatabaseForParse = Database;
            Parse(Code);
            _Name = NewName;
            _DatabaseForParse = null;
        }


        #endregion

        #region  Properties 
        public bool IsParsing { get; private set; }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name == value) { return; }
                _Name = value;
                OnChanged();
            }
        }


        public ListExt<string> PermissionGroups_Show { get; } = new ListExt<string>();


        public ColumnViewItem this[ColumnItem vColumn]
        {

            get
            {
                if (vColumn == null) { return null; }

                foreach (var ThisViewItem in this)
                {
                    if (ThisViewItem != null && ThisViewItem.Column == vColumn) { return ThisViewItem; }
                }

                return null;
            }
        }


        public ColumnViewItem this[int ItemNo]
        {
            get
            {
                return _Internal[ItemNo];
            }
        }


        #endregion


        public void Remove(ColumnViewItem ColumnViewItem)
        {
            _Internal.Remove(ColumnViewItem);
        }


        public void Add(ColumnItem Column, bool Permanent)
        {
            if (Permanent)
            {
                _Internal.Add(new ColumnViewItem(Column, enViewType.PermanentColumn));
            }
            else
            {
                _Internal.Add(new ColumnViewItem(Column, enViewType.Column));
            }
        }


        public void InsertAt(int index, ColumnItem Column)
        {
            _Internal.Insert(index, new ColumnViewItem(Column, enViewType.Column));
        }

        public void Clear()
        {
            _Internal.Clear();
        }


        public void Parse(string ToParse)
        {
            IsParsing = true;
            _Internal.ThrowEvents = false;
            PermissionGroups_Show.ThrowEvents = false;
            Initialize();

            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "name":
                        _Name = pair.Value;
                        break;
                    case "columndata":
                        _Internal.Add(new ColumnViewItem(_DatabaseForParse, pair.Value));
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
            _Internal.ThrowEvents = true;
            IsParsing = false;
        }



        public override string ToString()
        {
            PermissionGroups_Show.ThrowEvents = false;

            var Result = "{Name=" + _Name.ToNonCritical();


            foreach (var ThisViewItem in this)
            {
                if (ThisViewItem != null)
                {
                    Result = Result + ", Columndata=" + ThisViewItem;
                }
            }


            PermissionGroups_Show.RemoveString("#Administrator", false);
            var tmp = PermissionGroups_Show.SortedDistinctList();

            foreach (var t in tmp)
            {
                if (!string.IsNullOrEmpty(t))
                {
                    Result = Result + ", Permissiongroup=" + t;
                }
            }

            PermissionGroups_Show.ThrowEvents = true;

            return Result + "}";
        }


        public void ShowAllColumns(Database OfDatabase)
        {


            if (OfDatabase.IsParsing()) { return; }


            var OK = true;
            for (var z = 0; z < OfDatabase.Column.Count; z++)
            {

                if (z >= _Internal.Count)
                {
                    OK = false;
                    break;
                }

                if (_Internal[z].Column != OfDatabase.Column[z])
                {
                    OK = false;
                    break;
                }


            }


            if (_Internal.Count > 0 && _Internal[0].ViewType != enViewType.PermanentColumn) { OK = false; }

            if (OK) { return; }

            _Internal.Clear();

            foreach (var ThisColumnItem in OfDatabase.Column)
            {
                if (ThisColumnItem != null)
                {
                    _Internal.Add(new ColumnViewItem(ThisColumnItem, enViewType.Column));
                }
            }

            if (_Internal.Count > 0) { _Internal[0].ViewType = enViewType.PermanentColumn; }
        }


        public ColumnViewItem LastThisViewItem()
        {
            var z = _Internal.Count;
            do
            {
                z -= 1;
                if (z < 0) { return null; }

                if (_Internal[z] != null && _Internal[z].Column != null) { return _Internal[z]; }
            } while (true);
        }


        public ColumnItem PreviousVisible(ColumnItem OfColumn)
        {
            var ViewItemNo = _Internal.Count - 1;
            var Found = false;

            do
            {
                if (ViewItemNo < 0) { return null; }

                if (_Internal[ViewItemNo] != null && _Internal[ViewItemNo].Column != null)
                {
                    if (Found) { return _Internal[ViewItemNo].Column; }
                    if (_Internal[ViewItemNo].Column == OfColumn) { Found = true; }
                }

                ViewItemNo -= 1;
            } while (true);
        }

        public ColumnItem NextVisible(ColumnItem OfColumn)
        {
            var ViewItemNo = 0;
            var Found = false;

            do
            {
                if (ViewItemNo >= _Internal.Count) { return null; }

                if (_Internal[ViewItemNo] != null && _Internal[ViewItemNo].Column != null)
                {
                    if (Found) { return _Internal[ViewItemNo].Column; }
                    if (_Internal[ViewItemNo].Column == OfColumn) { Found = true; }
                }

                ViewItemNo += 1;
            } while (true);

        }

        public ColumnViewItem PreviousVisible(ColumnViewItem OfViewItem)
        {
            var ViewItemNo = _Internal.IndexOf(OfViewItem);

            do
            {
                ViewItemNo -= 1;
                if (ViewItemNo < 0) { return null; }

                if (_Internal[ViewItemNo] != null && _Internal[ViewItemNo].Column != null) { return _Internal[ViewItemNo]; }

            } while (true);
        }

        public ColumnViewItem NextVisible(ColumnViewItem OfViewItem)
        {
            var ViewItemNo = _Internal.IndexOf(OfViewItem);

            if (ViewItemNo < 0) { return null; }




            do
            {
                ViewItemNo += 1;
                if (ViewItemNo >= _Internal.Count) { return null; }

                if (_Internal[ViewItemNo] != null && _Internal[ViewItemNo].Column != null) { return _Internal[ViewItemNo]; }
            } while (true);

        }



        public void Swap(ColumnViewItem View1, ColumnViewItem View2)
        {
            // Absichtlich anderer Name, um klarzustellen, dass hier nicht der Standard-Swap angewandt wird
            if (View1 == null) { return; }
            if (View2 == null) { return; }
            if (View1 == View2) { return; }

            var Col1 = _Internal.IndexOf(View1);
            var Col2 = _Internal.IndexOf(View2);
            if (Col1 < 0 || Col2 < 0) { return; }


            _Internal.Swap(Col1, Col2);

            if (_Internal[Col1].ViewType != enViewType.PermanentColumn) { _Internal[Col2].ViewType = enViewType.Column; }

        }


        public List<ColumnItem> ListOfUsedColumn()
        {

            var ColList = new List<ColumnItem>();
            foreach (var t in _Internal)
            {
                if (t != null) { ColList.Add(t.Column); }
            }

            return ColList;
        }



        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        public void HideSystemColumns()
        {
            foreach (var ThisViewItem in this)
            {
                if (ThisViewItem != null)
                {
                    if (ThisViewItem.Column == null || !string.IsNullOrEmpty(ThisViewItem.Column.Identifier))
                    {
                        _Internal.Remove(ThisViewItem);
                        HideSystemColumns();
                        return;
                    }
                }
            }
        }

        public void Hide(string ColumnName)
        {
            foreach (var ThisViewItem in this)
            {
                if (ThisViewItem != null)
                {
                    if (ThisViewItem.Column == null || ThisViewItem.Column.Name.ToUpper() == ColumnName.ToUpper())
                    {
                        _Internal.Remove(ThisViewItem);
                        Hide(ColumnName);
                        return;
                    }
                }
            }
        }

        public IEnumerator<ColumnViewItem> GetEnumerator()
        {
            return _Internal.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return IEnumerable_GetEnumerator();
        }
        private IEnumerator IEnumerable_GetEnumerator()
        {
            return _Internal.GetEnumerator();
        }
        public int Count()
        {
            return _Internal.Count;
        }


        public int IndexOf(ColumnViewItem vColumnViewItem)
        {
            return _Internal.IndexOf(vColumnViewItem);
        }

        internal void Repair(Database Database)
        {

            if (_Internal == null || _Internal.Count == 0) { return; }

            for (var z = 0; z < _Internal.Count; z++)
            {
                if (_Internal[z].Column == null || !Database.Column.Contains(_Internal[z].Column))
                {
                    _Internal[z] = null;
                }
            }


            _Internal.RemoveNull();

        }


    }
}