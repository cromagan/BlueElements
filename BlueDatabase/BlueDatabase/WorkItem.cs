﻿// Authors:
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
using BlueDatabase.Enums;
using System;
using static BlueBasics.Converter;

namespace BlueDatabase {

    public class WorkItem : IParseable {

        #region Fields

        private string _changedTo;

        private long _colKey;

        private long _rowKey;

        private ItemState _state;

        #endregion

        #region Constructors

        public WorkItem(DatabaseDataType comand, long colKey, long rowKey, string previousValue, string changedTo, string user) {
            _state = ItemState.Pending;
            Comand = comand;
            _colKey = colKey;
            _rowKey = rowKey;
            PreviousValue = previousValue;
            _changedTo = changedTo;
            User = user;
            Date = DateTime.UtcNow;
        }

        public WorkItem(string s) => Parse(s);

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public string CellKey => CellCollection.KeyOfCell(ColKey, RowKey);

        public string ChangedTo {
            get => _changedTo;
            set {
                if (value == _changedTo) { return; }
                _changedTo = value;
                OnChanged();
            }
        }

        public long ColKey {
            get => _colKey;
            set {
                if (value == _colKey) { return; }
                _colKey = value;
                OnChanged();
            }
        }

        public DatabaseDataType Comand { get; private set; }
        public DateTime Date { get; private set; }
        public bool HistorischRelevant => State is ItemState.Pending or ItemState.Undo;
        public bool IsParsing { get; private set; }

        public string PreviousValue { get; private set; }

        public long RowKey {
            get => _rowKey;
            set {
                if (value == _rowKey) { return; }
                _rowKey = value;
                OnChanged();
            }
        }

        public string User { get; private set; }

        internal ItemState State {
            get => _state;
            set {
                if (value == _state) { return; }
                _state = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public string CompareKey() => Date.ToString(Constants.Format_Date) + ColKey;

        public void OnChanged() {
            if (IsParsing) {
                Develop.DebugPrint(FehlerArt.Warnung, "Falscher Parsing Zugriff!");
                return;
            }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Parse(string toParse) {
            IsParsing = true;
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "st":
                        _state = (ItemState)IntParse(pair.Value);
                        break;

                    case "co":
                        Comand = (DatabaseDataType)IntParse(pair.Value);
                        break;

                    case "ck":
                        _colKey = LongParse(pair.Value);
                        break;

                    case "rk":
                        _rowKey = LongParse(pair.Value);
                        break;

                    case "undotype":
                        //_UndoType = pair.Value;
                        break;

                    case "cell":
                        var cellKey = pair.Value.TrimStart("{ColumnKey=");
                        if (cellKey == "{Disposed}") { cellKey = "-1|-1"; }
                        cellKey = cellKey.Replace(", RowKey=", "|");
                        cellKey = cellKey.TrimEnd("}");
                        var x = cellKey.SplitAndCutBy("|");
                        if (x.GetUpperBound(0) == 1) {
                            LongTryParse(x[0], out _colKey);
                            LongTryParse(x[1], out _rowKey);
                        }
                        break;

                    case "cellkey":
                        cellKey = pair.Value;
                        var x2 = cellKey.SplitAndCutBy("|");
                        LongTryParse(x2[0], out _colKey);
                        LongTryParse(x2[1], out _rowKey);
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
                    case "pv": // Todo: alt: 10.08.2021
                        PreviousValue = pair.Value.FromNonCritical();
                        break;

                    case "c":
                        _changedTo = pair.Value.FromNonCriticalWithQuote();
                        break;

                    case "p":
                        PreviousValue = pair.Value.FromNonCriticalWithQuote();
                        break;

                    case "changedto":
                    case "ct": // Todo: alt: 10.08.2021
                        _changedTo = pair.Value.FromNonCritical();
                        break;

                    default:
                        Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            IsParsing = false;
        }

        public new string ToString() => "{ST=" + (int)_state +
                                        ", CO=" + (int)Comand +
                                        ", CK=" + _colKey +
                                        ", RK=" + _rowKey +
                                        ", D=" + Date +
                                        ", U=" + User.ToNonCritical() +
                                        ", P=" + PreviousValue.ToNonCriticalWithQuote() +
                                        ", C=" + _changedTo.ToNonCriticalWithQuote() +
                                        "}";

        public string UndoTextTableMouseOver() {
            var a = "'" + PreviousValue + "'";
            var n = "'" + ChangedTo + "'";
            if (a == "''") { a = "<IMAGECODE=Stern|16>"; }
            if (n == "''") { n = "<IMAGECODE=Papierkorb|16>"; }
            return "<b>alt: </b>" + a + "<b> <IMAGECODE=Pfeil_Rechts_Scrollbar|8|16> neu: </b>" + n + "     <i>(" + Date + ", " + User + ")</i>";
        }

        internal bool LogsUndo(Database database) => database.Column.SearchByKey(ColKey) is ColumnItem c && c.ShowUndo;

        #endregion
    }
}