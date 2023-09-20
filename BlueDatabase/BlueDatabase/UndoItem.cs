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
using static BlueBasics.Interfaces.IParseableExtension;

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

    public UndoItem(string s) => this.Parse(s);

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

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "undotype":
            case "st":
                //_state = (ItemState)IntParse(pair.Value);
                return true;

            case "co":
                Comand = (DatabaseDataType)IntParse(value);
                return true;

            case "cn":
                ColName = value;
                return true;

            case "rk":
                RowKey = value;
                return true;

            case "cell":
                return true;

            case "date":
            case "d":
                DateTimeUtc = DateTimeParse(value);
                return true;

            case "user":
            case "u":
                User = value.FromNonCritical();
                return true;

            case "group":
            case "g":
                //  Group = value.FromNonCritical();
                return true;

            case "c":
                ChangedTo = value.FromNonCritical();
                return true;

            case "previousvalue":
            case "pv": // Todo: alt: 10.08.2021
            case "p":
                PreviousValue = value.FromNonCritical();
                return true;

            case "changedto":
            case "ct": // Todo: alt: 10.08.2021
                ChangedTo = value.FromNonCritical();
                return true;

            case "cmt":
                Comment = value.FromNonCritical();
                return true;
        }

        return false;
    }

    public new string ToString() {
        List<string> result = new();

        result.ParseableAdd("CO", Comand);
        result.ParseableAdd("CN", ColName);
        result.ParseableAdd("RK", RowKey);
        result.ParseableAdd("D", DateTimeUtc);
        result.ParseableAdd("U", User);
        result.ParseableAdd("P", PreviousValue);
        result.ParseableAdd("C", ChangedTo);
        result.ParseableAdd("CMT", Comment);
        return result.Parseable();
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