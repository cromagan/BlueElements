// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes.FileHelpers;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using System;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueTable.Classes;

public class UndoItem : IParseable {

    #region Constructors

    public UndoItem(string tablename, TableDataType command, string column, string row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container, string chunkvalue) {
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
        ChunkValue = chunkvalue;
    }

    public UndoItem(string tablename, TableDataType command, string column, IHasKeyName? row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container, string chunkvalue) : this(tablename, command, column, row?.KeyName ?? string.Empty, previousValue, changedTo, user, timeutc, comment, container, chunkvalue) { }

    public UndoItem(string s) => this.Parse(s);

    #endregion

    #region Properties

    public string CellKey => CellCollection.KeyOfCell(ColName, RowKey);

    public string ChangedTo { get; private set; } = string.Empty;

    public string ChunkValue { get; private set; } = string.Empty;
    public string ColName { get; private set; } = string.Empty;
    public TableDataType Command { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    /// <summary>
    ///  Wird nicht vestringt!
    ///  Welchen Usprung das Undox-Item hat
    /// </summary>
    public string Container { get; set; } = string.Empty;

    public DateTime DateTimeUtc { get; private set; } = DateTime.MinValue;

    public string PreviousValue { get; private set; } = string.Empty;

    public string RowKey { get; private set; } = string.Empty;

    public string TableName { get; private set; } = string.Empty;

    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public DataSerializer? SerializableContent() {
        var result = new IniSerializer();
        ;

        result.Add("T", TableName);
        result.Add("CO", Command);
        result.Add("D", DateTimeUtc, "yyyy-MM-dd HH:mm:ss.fff");
        result.Add("U", User);
        result.Add("CN", ColName);
        result.Add("RK", RowKey);
        result.Add("P", PreviousValue);
        result.Add("C", ChangedTo);
        result.Add("CV", ChunkValue);
        result.Add("CMT", Comment);
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "t":
                TableName = value;
                return true;

            case "ch":
                //chunk
                return true;

            case "undotype":
            case "st":
                //_state = (ItemState)IntParse(pair.Value);
                return true;

            case "co":
                Command = (TableDataType)IntParse(value);
                return true;

            case "cn":
                ColName = value;
                return true;

            case "rk":
                RowKey = value; // Schlüssel könnten + Zeichen enthalten
                return true;

            case "cell":
                return true;

            case "date":
            case "d":
                DateTimeUtc = DateTimeParse(value);
                return true;

            case "user":
            case "u":
                User = value;
                return true;

            case "group":
            case "g":
                //  Group = value;
                return true;

            case "c":
                ChangedTo = value;
                return true;

            case "previousvalue":
            case "pv": // Todo: alt: 10.08.2021
            case "p":
                PreviousValue = value;
                return true;

            case "changedto":
            case "ct": // Todo: alt: 10.08.2021
                ChangedTo = value;
                return true;

            case "cmt":
                Comment = value;
                return true;

            case "cv":
                ChunkValue = value;
                return true;
        }

        return false;
    }

    public string UndoTextTableMouseOver() {
        var a = "'" + PreviousValue.Replace("\r", "|").Replace("\n", "|") + "'";
        var n = "'" + ChangedTo.Replace("\r", "|").Replace("\n", "|") + "'";
        if (a == "''") { a = "<imagecode=Stern|16>"; }
        if (n == "''") { n = "<imagecode=Papierkorb|16>"; }
        return "<b>alt: </b>" + a + "<b> <imagecode=Pfeil_Rechts_Scrollbar|8|16> neu: </b>" + n + "     <i>(" + DateTimeUtc + ", " + User + ")</i>";
    }

    internal bool LogsUndo(Table table) {
        if (Command != TableDataType.UTF8Value_withoutSizeData) { return true; }

        return table.Column[ColName] is { IsDisposed: false, SaveContent: true } c &&
             c != table.Column.SysRowChanger &&
             c != table.Column.SysRowChangeDate;
    }

    #endregion
}