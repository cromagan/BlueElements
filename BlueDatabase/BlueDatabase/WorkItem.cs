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
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

public class WorkItem : IParseable {

    #region Constructors

    public WorkItem(DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user) {
        Comand = comand;
        ColName = column?.Name ?? string.Empty;
        RowKey = row?.KeyName ?? string.Empty;
        PreviousValue = previousValue;
        ChangedTo = changedTo;
        User = user;
        DateTimeUTC = DateTime.UtcNow;
    }

    public WorkItem(string s) => Parse(s);

    #endregion

    #region Properties

    public string CellKey => CellCollection.KeyOfCell(ColName, RowKey);
    public string ChangedTo { get; private set; } = string.Empty;
    public string ColName { get; private set; } = string.Empty;
    public DatabaseDataType Comand { get; private set; } = (DatabaseDataType)0;
    public string Coment { get; private set; }
    public DateTime DateTimeUTC { get; private set; }
    public string? PreviousValue { get; private set; }
    public string RowKey { get; private set; } = string.Empty;
    public string TableName { get; private set; }
    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public string CompareKey() => DateTimeUTC.ToString(Constants.Format_Date) + ColName;

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
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

                case "undotype":
                    break;

                case "cell":
                    break;

                case "date":
                case "d":
                    DateTimeUTC = DateTimeParse(pair.Value);
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
                    ChangedTo = pair.Value.FromNonCriticalWithQuote();
                    break;

                case "p":
                    PreviousValue = pair.Value.FromNonCriticalWithQuote();
                    break;

                case "changedto":
                case "ct": // Todo: alt: 10.08.2021
                    ChangedTo = pair.Value.FromNonCritical();
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    public new string ToString() => "{CO=" + (int)Comand +
                                    ", CN=" + ColName +
                                    ", RK=" + RowKey +
                                    ", D=" + DateTimeUTC +
                                    ", U=" + User.ToNonCritical() +
                                    ", P=" + PreviousValue.ToNonCriticalWithQuote() +
                                    ", C=" + ChangedTo.ToNonCriticalWithQuote() +
                                    "}";

    public string UndoTextTableMouseOver() {
        var a = "'" + PreviousValue + "'";
        var n = "'" + ChangedTo + "'";
        if (a == "''") { a = "<IMAGECODE=Stern|16>"; }
        if (n == "''") { n = "<IMAGECODE=Papierkorb|16>"; }
        return "<b>alt: </b>" + a + "<b> <IMAGECODE=Pfeil_Rechts_Scrollbar|8|16> neu: </b>" + n + "     <i>(" + DateTimeUTC + ", " + User + ")</i>";
    }

    internal bool LogsUndo(DatabaseAbstract database) => database.Column.Exists(ColName) is ColumnItem c && c.ShowUndo;

    #endregion
}