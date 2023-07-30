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
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

public class UndoItem : IParseable {

    #region Constructors

    public UndoItem(string tablename, DatabaseDataType comand, string column, string row, string previousValue, string changedTo, string user, string comment, DateTime timeutc) {
        Comand = comand;
        ColName = column;
        RowKey = row;
        PreviousValue = previousValue;
        ChangedTo = changedTo;
        User = user;
        DateTimeUtc = timeutc;
        TableName = tablename;
        Comment = comment;
    }

    public UndoItem(string tablename, DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, string comment, DateTime timeutc) : this(tablename, comand, column?.KeyName ?? string.Empty, row?.KeyName ?? string.Empty, previousValue, changedTo, user, comment, timeutc) { }

    public UndoItem(string s) => Parse(s);

    #endregion

    #region Properties

    public string CellKey => CellCollection.KeyOfCell(ColName, RowKey);
    public string ChangedTo { get; private set; } = string.Empty;
    public string ColName { get; private set; } = string.Empty;
    public DatabaseDataType Comand { get; private set; } = 0;
    public string Comment { get; private set; } = string.Empty;
    public DateTime DateTimeUtc { get; private set; } = DateTime.MinValue;
    public string PreviousValue { get; private set; } = string.Empty;
    public string RowKey { get; private set; } = string.Empty;
    public string TableName { get; private set; } = string.Empty;
    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "undotype":
                case "st":
                    //_state = (ItemState)IntParse(pair.Value);
                    break;

                case "co":
                    Comand = (DatabaseDataType)IntParse(pair.Value);
                    break;

                case "cn":
                    ColName = pair.Value;
                    break;

                case "rk":
                    RowKey = pair.Value;
                    break;

                case "cell":
                    break;

                case "date":
                case "d":
                    DateTimeUtc = DateTimeParse(pair.Value);
                    break;

                case "user":
                case "u":
                    User = pair.Value.FromNonCritical();
                    break;

                case "group":
                case "g":
                    //  Group = pair.Value.FromNonCritical();
                    break;

                case "c":
                    ChangedTo = pair.Value.FromNonCritical();
                    break;

                case "previousvalue":
                case "pv": // Todo: alt: 10.08.2021
                case "p":
                    PreviousValue = pair.Value.FromNonCritical();
                    break;

                case "changedto":
                case "ct": // Todo: alt: 10.08.2021
                    ChangedTo = pair.Value.FromNonCritical();
                    break;

                case "cmt":
                    Comment = pair.Value.FromNonCritical();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    public new string ToString() {
        var l = new List<string>();

        l.ParseableAdd("CO", Comand);
        l.ParseableAdd("CN", ColName);
        l.ParseableAdd("RK", RowKey);
        l.ParseableAdd("D", DateTimeUtc);
        l.ParseableAdd("U", User);
        l.ParseableAdd("P", PreviousValue);
        l.ParseableAdd("C", ChangedTo);
        l.ParseableAdd("CMT", Comment);
        return l.Parseable();
    }

    public string UndoTextTableMouseOver() {
        var a = "'" + PreviousValue.Replace("\r", "|").Replace("\n", "|") + "'";
        var n = "'" + ChangedTo.Replace("\r", "|").Replace("\n", "|") + "'";
        if (a == "''") { a = "<IMAGECODE=Stern|16>"; }
        if (n == "''") { n = "<IMAGECODE=Papierkorb|16>"; }
        return "<b>alt: </b>" + a + "<b> <IMAGECODE=Pfeil_Rechts_Scrollbar|8|16> neu: </b>" + n + "     <i>(" + DateTimeUtc + ", " + User + ")</i>";
    }

    internal bool LogsUndo(DatabaseAbstract database) => database.Column.Exists(ColName) is ColumnItem c && c.ShowUndo;

    #endregion
}