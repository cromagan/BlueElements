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
using static BlueBasics.modConverter;

namespace BlueDatabase
{
    public class WorkItem : IParseable, ICompareKey
    {




        public event EventHandler Changed;

        #region  Variablen-Deklarationen 

        private enItemState _state;
        private int _colKey;
        private int _rowKey;
        private string _changedTo;

        #endregion





        #region Properties

        public bool IsParsing { get; private set; }

        internal enItemState State
        {
            get => _state;

            set
            {
                if (value == _state) { return; }
                _state = value;
                OnChanged();
            }
        }

        public string CellKey => CellCollection.KeyOfCell(ColKey, RowKey);

        public enDatabaseDataType Comand { get; private set; }

        public int ColKey
        {
            get => _colKey;

            set
            {
                if (value == _colKey) { return; }
                _colKey = value;
                OnChanged();
            }

        }

        public int RowKey
        {
            get => _rowKey;

            set
            {
                if (value == _rowKey) { return; }
                _rowKey = value;
                OnChanged();
            }

        }

        public DateTime Date { get; private set; }

        public string User { get; private set; }

        public string PreviousValue { get; private set; }

        public string ChangedTo
        {
            get => _changedTo;

            set
            {
                if (value == _changedTo) { return; }
                _changedTo = value;
                OnChanged();
            }
        }

        public bool HistorischRelevant
        {
            get
            {
                if (State == enItemState.Pending || State == enItemState.Undo) { return true; }
                return false;
            }
        }


        #endregion

        public WorkItem(enDatabaseDataType Comand, int ColKey, int RowKey, string PreviousValue, string ChangedTo, string User)
        {

            _state = enItemState.Pending;

            this.Comand = Comand;
            _colKey = ColKey;
            _rowKey = RowKey;

            this.PreviousValue = PreviousValue;
            _changedTo = ChangedTo;
            this.User = User;

            Date = DateTime.UtcNow;
        }


        public WorkItem(string s)
        {
            Parse(s);
        }


        public void Parse(string ToParse)
        {
            IsParsing = true;
            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "st":
                        _state = (enItemState)int.Parse(pair.Value);
                        break;

                    case "co":
                        Comand = (enDatabaseDataType)int.Parse(pair.Value);
                        break;

                    case "ck":
                        _colKey = int.Parse(pair.Value);
                        break;

                    case "rk":
                        _rowKey = int.Parse(pair.Value);
                        break;

                    case "undotype":
                        //_UndoType = pair.Value;
                        break;

                    case "cell":
                        var _CellKey = pair.Value.TrimStart("{ColumnKey=");
                        if (_CellKey == "{Disposed}") { _CellKey = "-1|-1"; }
                        _CellKey = _CellKey.Replace(", RowKey=", "|");
                        _CellKey = _CellKey.TrimEnd("}");

                        var x = _CellKey.SplitBy("|");

                        if (x.GetUpperBound(0) == 1)
                        {
                            int.TryParse(x[0], out _colKey);
                            int.TryParse(x[1], out _rowKey);
                        }
                        break;

                    case "cellkey":
                        _CellKey = pair.Value;
                        var x2 = _CellKey.SplitBy("|");

                        int.TryParse(x2[0], out _colKey);
                        int.TryParse(x2[1], out _rowKey);
                        break;

                    case "date":
                    case "d":
                        Date = DateTimeParse(pair.Value);
                        break;

                    case "user":
                    case "u":
                        User = pair.Value.FromNonCritical();
                        break;

                    case "group":
                    case "g":
                        //  Group = pair.Value.FromNonCritical();
                        break;

                    case "previousvalue":
                    case "pv":
                        PreviousValue = pair.Value.FromNonCritical();
                        break;

                    case "changedto":
                    case "ct":
                        _changedTo = pair.Value.FromNonCritical();
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            IsParsing = false;
        }

        public new string ToString()
        {

            return "{ST=" + (int)_state +
                   ", CO=" + (int)Comand +
                   ", CK=" + _colKey +
                   ", RK=" + _rowKey +
                   ", D=" + Date +
                   ", U=" + User.ToNonCritical() +
                   ", PV=" + PreviousValue.ToNonCritical() +
                   ", CT=" + _changedTo.ToNonCritical() +
                   "}";
            // ", G=" + Group.ToNonCritical() +
        }

        public string UndoTextTableMouseOver()
        {


            var a = "'" + PreviousValue + "'";
            var n = "'" + ChangedTo + "'";

            if (a == "''") { a = "<IMAGECODE=Stern|16>"; }
            if (n == "''") { n = "<IMAGECODE=Papierkorb|16>"; }

            return "<b>alt: </b>" + a + "<b> <IMAGECODE=Pfeil_Rechts_Scrollbar|8|16> neu: </b>" + n + "     <i>(" + Date + ", " + User + ")</i>";


        }


        public string CompareKey()
        {

            return Date.ToString(Constants.Format_Date) + ColKey;

        }

        public void OnChanged()
        {
            if (IsParsing)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!");
                return;
            }

            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        internal bool LogsUndo(Database database)
        {
            return database.Column.SearchByKey(ColKey) is ColumnItem C && C.ShowUndo;

        }
    }
}
