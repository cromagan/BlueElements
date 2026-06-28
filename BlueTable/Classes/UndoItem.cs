// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueTable.Classes;

public class UndoItem : IParseable {

    #region Constructors

    public UndoItem(string tablename, TableDataType command, string column, string row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container) {
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

    public UndoItem(string tablename, TableDataType command, string column, IHasKeyName? row, string previousValue, string changedTo, string user, DateTime timeutc, string comment, string container) : this(tablename, command, column, row?.KeyName ?? string.Empty, previousValue, changedTo, user, timeutc, comment, container) { }

    public UndoItem(string s) => this.Parse(s);

    #endregion

    #region Properties

    public string CellKey => CellCollection.KeyOfCell(ColName, RowKey);

    public string ChangedTo { get; private set; } = string.Empty;

    public string ColName { get; private set; } = string.Empty;
    public TableDataType Command { get; private set; }

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

    /// <summary>
    /// Berechnet einen deterministischen Hash zur Dedup-Erkennung in TableFragments.
    /// Enthält nur die identitätsstiftenden Werte (ohne <see cref="PreviousValue"/>,
    /// <see cref="Comment"/> und <see cref="Container"/>), weil diese zwischen Fragment-
    /// und Undo-Schreibweg inkonsistent sein können. Damit ist der Hash unabhängig vom
    /// Serialisationsformat und vom Schreibweg (Fragment vs. lokales Undo vs. Hauptfile).
    /// </summary>
    public string Hash() {
        return (TableName + "|" + (int)Command + "|" + ColName + "|" + RowKey + "|" +
                DateTimeUtc.ToString9() + "|" + User + "|" + ChangedTo).GetMD5Hash();
    }

    public List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("T", TableName);
        result.ParseableAdd("CO", Command);
        result.ParseableAdd("D", DateTimeUtc, "yyyy-MM-dd HH:mm:ss.fff");
        result.ParseableAdd("U", User);
        result.ParseableAdd("CN", ColName);
        result.ParseableAdd("RK", RowKey);
        result.ParseableAdd("P", PreviousValue);
        result.ParseableAdd("C", ChangedTo);
        result.ParseableAdd("CMT", Comment);
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

            case "cv": // TODO: Entfernen 06.06.2026
                //ChunkValue = value.FromNonCritical();
                return true;
        }

        return false;
    }

    public string UndoTextTableMouseOver() {
        var a = "'" + PreviousValue.Replace('\r', '|').Replace('\n', '|') + "'";
        var n = "'" + ChangedTo.Replace('\r', '|').Replace('\n', '|') + "'";
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