// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueTable.Classes;

public class UndoItem : IParseable, IFragmentStoreable {

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

    /// <summary>
    /// Leer-Eintrag zum Einlesen der Undo-Sektion; TableName ist die empfangende
    /// Tabelle, Container deren Hauptdatei.
    /// </summary>
    public UndoItem(string tablename, string container) {
        TableName = tablename;
        Container = container;
    }

    public UndoItem() { }

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

    #region IFragmentStoreable

    /// <summary>
    /// Blatt-Eintrag ohne Sub-Baum: Das Event wird nie ausgelöst.
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Blatt-Typen des eigenen Blocks in der Undo-Sektion der Hauptdatei.
    /// </summary>
    private enum UndoItemType : byte {

        /// <summary>
        /// Klassischer Ganzwert-Typ der Änderung (Int).
        /// </summary>
        Command = 1,

        /// <summary>
        /// Spalten-Key der Änderung (leer bei Kopf-Werten).
        /// </summary>
        ColName = 2,

        /// <summary>
        /// Zeilen-Key der Änderung (leer bei Kopf- und Spalten-Werten).
        /// </summary>
        RowKey = 3,

        /// <summary>
        /// Neuer Wert samt Meta (User, DateTimeUtc, Comment).
        /// </summary>
        ChangedTo = 4
    }

    /// <summary>
    /// Schreibt den Eintrag als eigenen Block: Command, Navigation (nur wenn
    /// gesetzt) und den neuen Wert samt Meta (User, DateTimeUtc, Comment).
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        var allok = true;

        allok &= FragmentRecord.Append(stream, depth, UndoItemType.Command, (int)Command);

        if (ColName is { Length: > 0 }) {
            allok &= FragmentRecord.Append(stream, depth, UndoItemType.ColName, ColName);
        }

        if (RowKey is { Length: > 0 }) {
            allok &= FragmentRecord.Append(stream, depth, UndoItemType.RowKey, RowKey);
        }

        allok &= FragmentRecord.Append(stream, depth, UndoItemType.ChangedTo, ChangedTo, User, DateTimeUtc, Comment);

        return allok;
    }

    /// <summary>
    /// Liest die Blatt-Sätze des eigenen Blocks; unbekannte Typen werden übersprungen.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((UndoItemType)record.LeafType) {
            case UndoItemType.Command:
                Command = (TableDataType)record.IntContent;
                break;

            case UndoItemType.ColName:
                ColName = record.StringContent;
                break;

            case UndoItemType.RowKey:
                RowKey = record.StringContent;
                break;

            case UndoItemType.ChangedTo:
                ChangedTo = record.StringContent;
                User = record.User;
                DateTimeUtc = record.DateTimeUtc;
                Comment = record.Comment;
                break;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Blatt-Eintrag ohne Sub-Baum: löst nichts aus.
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) {
        FragmentChanged?.Invoke(this, e);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet einen deterministischen Hash zur Dedup-Erkennung in TableFragments.
    /// Enthält nur die identitätsstiftenden Werte (ohne PreviousValue, Comment und
    /// Container), weil diese zwischen den Schreibwegen inkonsistent sein können.
    /// Damit ist der Hash unabhängig vom Serialisationsformat und vom Schreibweg.
    /// </summary>
    public string Hash() => BuildHash(TableName, Command, ColName, RowKey, DateTimeUtc, User, ChangedTo);

    /// <summary>
    /// Geteilte Dedup-Formel für klassische Einträge; muss für die Hauptfile-Historie
    /// und für aus Fragmenten gelesene Änderungen identisch sein.
    /// </summary>
    internal static string BuildHash(string tableName, TableDataType command, string colName, string rowKey, DateTime dateTimeUtc, string user, string changedTo) =>
        (tableName + "|" + (int)command + "|" + colName + "|" + rowKey + "|" +
         dateTimeUtc.ToString9() + "|" + user + "|" + changedTo).GetMD5Hash();

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

    public bool ParseFinished() => true;

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
