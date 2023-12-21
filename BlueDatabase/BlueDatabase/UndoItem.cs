// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

public class UndoItem : IParseable {

    #region Constructors

    public UndoItem(string tablename, DatabaseDataType command, string column, string row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container) {
        Command = command;
        ColName = column;
        RowKey = row;
        PreviousValue = previousValue;
        ChangedTo = changedTo;
        User = user;
        DateTimeUtc = timeutc;
        TableName = tablename;
        Comment = comment;
        Container = container;
    }

    public UndoItem(string tablename, DatabaseDataType command, IHasKeyName? column, IHasKeyName? row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container) : this(tablename, command, column?.KeyName ?? string.Empty, row?.KeyName ?? string.Empty, previousValue, changedTo, user, timeutc, comment, container) { }

    public UndoItem(string s) => this.Parse(s);

    #endregion

    #region Properties

    public string CellKey => CellCollection.KeyOfCell(ColName, RowKey);

    public string ChangedTo { get; private set; } = string.Empty;

    public string ColName { get; private set; } = string.Empty;

    public DatabaseDataType Command { get; private set; } = 0;

    public string Comment { get; private set; } = string.Empty;

    /// <summary>
    ///  Wird nicht vestringt!
    ///  Welchen Usprung das Undo-Item hat
    /// </summary>
    public string Container { get; set; } = string.Empty;

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
            case "t":
                TableName = value;
                return true;

            case "undotype":
            case "st":
                //_state = (ItemState)IntParse(pair.Value);
                return true;

            case "co":
                Command = (DatabaseDataType)IntParse(value);
                return true;

            case "cn":
                ColName = value;
                return true;

            case "rk":
                RowKey = value.FromNonCritical(); // Schlüssel könnten + Zeichen enthalten
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
        List<string> result = [];

        result.ParseableAdd("T", TableName);
        result.ParseableAdd("CO", Command);
        result.ParseableAdd("D", DateTimeUtc, Constants.Format_Date9);
        result.ParseableAdd("U", User);
        result.ParseableAdd("CN", ColName);
        result.ParseableAdd("RK", RowKey);
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

    internal bool LogsUndo(Database database) => database.Column.Exists(ColName) is ColumnItem c && c.ShowUndo;

    #endregion
}