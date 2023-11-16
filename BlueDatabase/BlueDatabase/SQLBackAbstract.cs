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

// https://stackoverflow.com/questions/34002901/need-for-sqlconnection-and-oracleconnection-if-odbcconnection-can-be-used-in-th
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Constants;
using static BlueBasics.IO;
using static BlueDatabase.DatabaseAbstract;
using BlueDatabase.EventArgs;
using System.Diagnostics;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public abstract class SqlBackAbstract {

    #region Fields

    public const string DatabaseProperty = "~DATABASE~";
    public const string SysStyle = "SYS_STYLE";
    public const string SysUndo = "SYS_UNDO";
    public static List<SqlBackAbstract> ConnectedSqlBack = new();
    public static List<string> Log = new();
    public DbConnection? Connection;
    private static bool _didBackup;
    private readonly object _fill = new();
    private readonly object _getChanges = new();
    private readonly object _openclose = new();
    private bool _didCritical;

    #endregion

    #region Constructors

    public SqlBackAbstract() => ConnectedSqlBack.Add(this);

    #endregion

    #region Properties

    public abstract string ColumnPropertyPrimary { get; }
    public abstract string ColumnTypeDate { get; }
    public string ColumnTypeVarChar15 => VarChar(15);
    public string ColumnTypeVarChar18 => VarChar(18);
    public string ColumnTypeVarChar255 => VarChar(255);
    public string ColumnTypeVarChar4000 => VarChar(4000);
    public string ConnectionString { get; protected set; } = string.Empty;

    /// <summary>
    /// Falls die Datenbank von einer lokalen Datei geladen wurde, ist hier der Dateiname enthalten.
    /// </summary>
    public string Filename { get; protected set; } = string.Empty;

    public abstract int MaxStringLenght { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt eine Spalte nur bei Bedarf.
    /// Aber der KeyName wird aktualisiert.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="columnName"></param>
    /// <param name="allowSystemTableNames"></param>
    public string AddColumnToMain(string tablename, string columnName, bool allowSystemTableNames) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames(tablename.ToUpper());
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return "Spalte nicht gefunden"; }
        if (!colMain.Contains(columnName)) { AddColumn(tablename.ToUpper(), columnName, ColumnTypeVarChar4000, true, allowSystemTableNames); }

        return SetStyleData(tablename, DatabaseDataType.ColumnName, columnName.ToUpper(), columnName.ToUpper());
    }

    public string AddUndo(string tablename, DatabaseDataType comand, string? columname, string rowKey, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment) {
        if (!OpenConnection()) { return "Es konnte keine Verbindung zur Datenbank aufgebaut werden"; }

        if (comand == DatabaseDataType.SystemValue) { return "System-Values werden nicht geloggt."; }
        if (comand.IsObsolete()) { return "Obsoloete Befehle werden nicht geloggt."; }

        //var ck = columnKey is not null and > (-1) ? columnKey.ToString() : string.Empty;
        var rk = rowKey ?? string.Empty;

        var cmdString = "INSERT INTO " + SysUndo +
            " (TABLENAME, COMAND, COLUMNNAME, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, TIMECODEUTC, CMT) VALUES (" +
             Dbval(tablename.ToUpper()) + "," +
             Dbval(comand.ToString()) + "," +
             Dbval(columname) + "," +
             Dbval(rk) + "," +
             Dbval(previousValue) + "," +
             Dbval(changedTo) + "," +
             Dbval(user) + "," +
             Dbval(datetimeutc) + "," +
             Dbval(comment) + ")";

        return ExecuteCommand(cmdString, false);
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - einschließlich der Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    public abstract List<string>? AllTables();

    /// <summary>
    /// Gibt empty zurück, wenn das Backup erstellt wurde ODER die Datei bereits existiert
    /// </summary>
    /// <param name="pathadditionalcsv"></param>
    /// <returns></returns>
    public string BackupSysStyle(string pathadditionalcsv) {
        if (string.IsNullOrEmpty(pathadditionalcsv)) { return "Kein Backup-Verzeichniss angebeben."; }

        if (!Directory.Exists(pathadditionalcsv)) { return "Backup-Verzeichniss existiert nicht."; }

        var d = DateTime.UtcNow.ToString(Format_Date10);

        var file = pathadditionalcsv.CheckPath() + "SYS_STYLE_" + d + ".csv";
        if (FileExists(file)) { return string.Empty; }

        var dt = Fill_Table("SELECT * FROM " + SysStyle);
        if (!dt.WriteToCsvFile(file)) { return "Schreibzugriff fehlgeschlagen."; }

        var bvw = new BackupVerwalter(2, 20);
        return bvw.CleanUpDirectory(pathadditionalcsv.CheckPath(), "SYS_STYLE_*.csv");
    }

    public void ChangeDataType(string tablename, string column, int charlenght, bool allowSystemTableNames) {
        //https://stackoverflow.com/questions/10321775/changing-the-data-type-of-a-column-in-oracle

        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return;
        }

        if (!ColumnItem.IsValidColumnName(column)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + column);
            return;
        }

        var s = (DataFormat)IntParse(GetStyleData(tablename, DatabaseDataType.ColumnFormat, column, SysStyle));

        if (s == DataFormat.Verknüpfung_zu_anderer_Datenbank) { charlenght = Math.Max(charlenght, 35); }
        if (s == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { charlenght = Math.Max(charlenght, 15); }

        var cmdString = @"ALTER TABLE " + tablename + " MODIFY (" + column + " " + VarChar(charlenght) + ")";
        _ = ExecuteCommand(cmdString, true);
    }

    public bool CloseConnection() {
        try {
            if (Connection == null) { return true; }

            lock (_openclose) {
                if (Connection.State == ConnectionState.Open) { Connection.Close(); }

                return Connection.State == ConnectionState.Closed;
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CloseConnection();
        }
    }

    public abstract int ColumnLenght(string tablename, string columnname);

    public List<string>? ColumnValues(string tablename, string columnname) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return null;
        }

        if (!ColumnItem.IsValidColumnName(columnname)) {
            Develop.DebugPrint(FehlerArt.Fehler, "ColumnName ungültig: " + columnname);
            return null;
        }

        var dt = Fill_Table("SELECT DISTINCT " + columnname + "  FROM " + tablename);

        var l = new List<string>();

        foreach (var thisrow in dt.Rows) {
            l.Add(((DataRow)thisrow)[0].ToString());
        }

        return l.SortedDistinctList();
    }

    /// <summary>
    /// Provider ist immer NULL!
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="mustBeFreezed"></param>
    /// <returns></returns>
    public ConnectionInfo ConnectionData(string tablename, string mustBeFreezed) => new(tablename, null, ConnectionString, string.Empty, mustBeFreezed);

    public string ExecuteCommand(string commandtext, bool abort) {
        lock (_openclose) {
            if (Connection is not DbConnection dbcon || !OpenConnection()) { return "Es konnte keine Verbindung zur Datenbank aufgebaut werden"; }
            using var command = dbcon.CreateCommand();
            command.CommandText = commandtext;
            return ExecuteCommand(command, abort);
        }
    }

    /// <summary>
    /// Datentabelle befüllen
    /// </summary>
    /// <returns>befüllte  Tabelle - Datatable</returns>
    public DataTable Fill_Table(string commandtext) {
        lock (_fill) {
            if (Connection == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Keine Connection vorhanden");
                throw new Exception();
            }

            try {
                using var command = Connection.CreateCommand();
                command.CommandText = commandtext;

                command.CommandTimeout = 10;

                var tbl = new DataTable();
                PauseSystem();
                if (OpenConnection()) {
                    //  Log.Add(commandtext);
                    tbl.Load(command.ExecuteReader());
                    _ = CloseConnection();
                    LastLoadUtc = DateTime.UtcNow;
                    return tbl;
                }
            } catch (Exception ex) {
                StackTrace stackTrace = new();
                if (stackTrace.FrameCount > 95) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Commandtext: " + commandtext, ex);
                }
            }

            Generic.Pause(0.1, false);
            _ = CloseConnection();

            Generic.Pause(1, false);
            Develop.CheckStackForOverflow();
            return Fill_Table(commandtext);
        }
    }

    /// <summary>
    /// Gibt NULL oder die RowId zurück
    /// </summary>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public abstract string? GenerateRow(string tablename);

    /// <summary>
    /// Gibt die Spaltennamen in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public abstract List<string> GetColumnNames(string tablename);

    public string GetStyleData(string tablename, DatabaseDataType type, string columnName, string styleDb) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        if (string.IsNullOrEmpty(columnName)) { columnName = DatabaseProperty; }

        if (columnName != DatabaseProperty && !ColumnItem.IsValidColumnName(columnName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + columnName);
            throw new Exception();
        }

        var cmd = @"select VALUE, PART from " + styleDb + " " +
                        "where TABLENAME = " + Dbval(tablename.ToUpper()) + " " +
                        "and TYPE = " + Dbval(type.ToString()) + " " +
                        "and COLUMNNAME = " + Dbval(columnName.ToUpper()) + " " +
                        "ORDER BY PART ASC";

        var dt = Fill_Table(cmd);

        var value = string.Empty;

        foreach (var thisRow in dt.Rows) {
            value += ((DataRow)thisRow)[0].ToString();
        }

        return value;
    }

    /// <summary>
    /// Wird kein Spaltenname angegeben, werden die Eigenschaften der Datenbank zurück gegeben.
    /// Achtung, die Connection wird nicht geschlossen!
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public void GetStyleDataAll(DatabaseAbstract db) {
        var l = new Dictionary<string, string>();
        var commandText = @"select TYPE, PART, COLUMNNAME, VALUE from " + SysStyle + " " +
                         "where TABLENAME = " + Dbval(db.TableName.ToUpper()) + " " +
                         "ORDER BY PART ASC";

        var dt = Fill_Table(commandText);

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;
            var key = reader[0].ToString();
            var part = reader[1].ToString();
            var column = reader[2].ToString();
            var value = reader[3].ToString();

            var cc = key + "|" + column;

            if (part != "001") {
                l[cc] += value;
            } else {
                if (!l.ContainsKey(cc)) { l.Add(cc, value); }
            }
        }

        if (l.Count > 0) {
            foreach (var thisstyle in l) {
                var k = thisstyle.Key.Split('|');
                _ = Enum.TryParse(k[0], out DatabaseDataType t);

                if (!t.IsObsolete()) {
                    string? ok;
                    if (k[1].Equals(DatabaseProperty, StringComparison.OrdinalIgnoreCase)) {
                        ok = db.SetValueInternal(t, thisstyle.Value, null, null, Reason.LoadReload, Generic.UserName, DateTime.UtcNow);
                    } else {
                        var column = db.Column.Exists(k[1]); // Exists, kann sein dass noch ein Eintrag in der SysSytle ist, aber die Spalte schon gelöscjht wurde
                        ok = column?.SetValueInternal(t, thisstyle.Value, Reason.LoadReload);
                    }

                    if (!string.IsNullOrEmpty(ok)) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Ladefehler: " + ok);
                    }
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="row"></param>
    /// <param name="refreshAlways">Bei TRUE wird die gesamte Zeile aktualistert, weil evtl. eine Änderung aufgetreten ist. Bei FLASE werden nur die fehlenden Daten der noch nicht geladenen Spalten nachgeladen.</param>
    public string LoadRow(string tablename, List<RowItem> row, bool refreshAlways) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        if (row.Count == 0 || row[0] is null) { return string.Empty; }

        var db = row[0]?.Database;
        if (db == null || db.IsDisposed) { return "Datenbank verworfen"; }

        var com = new StringBuilder();
        com.Append("SELECT ROWID, ");

        #region Spalten wählen

        var count = 0;

        foreach (var thiscolumn in db.Column) {
            if (refreshAlways || thiscolumn.IsInCache == null) {
                if (count > 0) { com.Append(", "); }

                count++;
                com.Append(thiscolumn.KeyName.ToUpper());
            }
        }

        #endregion

        #region Alle spalten da?

        if (count == 0) {
            // Alle spalten im Speicher, also alle Zeilen auch als OK marieren
            foreach (var thisRow in row) {
                thisRow.IsInCache = DateTime.UtcNow;
            }

            return string.Empty;
        }

        #endregion

        com.Append(" FROM " + tablename.ToUpper());

        #region Zeilen wählen

        if (row.Count < 1000) {
            com.Append(" WHERE ");
            count = 0;
            foreach (var thisr in row) {
                if (count > 0) { com.Append(" OR "); }

                count++;
                com.Append("ROWID = " + Dbval(thisr.KeyName));
            }
        }

        #endregion

        var dt = Fill_Table(com.ToString());

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;
            var rk = reader[0].ToString();
            var r = db.Row.SearchByKey(rk) ?? db.Row.GenerateAndAdd(rk, string.Empty, false, false, "Load row");

            if (r != null && !r.IsDisposed) {
                for (var z = 1; z < dt.Columns.Count; z++) {
                    var cx = db.Column.Exists(dt.Columns[z].ColumnName);
                    if (cx != null) {
                        _ = db.Cell.SetValueInternal(cx, r, reader[z].ToString(), Reason.LoadReload);
                    }
                }
            }
        }

        //Ja, alle Rows den Stempel geben.
        // Wenn die Zeile nicht mehr existent ist, würde der Befehl unendlich oft versuchen
        // Die Zeile aus der Datenbank zu erhalten
        foreach (var thisr in row) {
            thisr.IsInCache = DateTime.UtcNow;
        }

        return string.Empty; // db.Row.DoLinkedDatabase(row);
    }

    public bool OpenConnection() {
        lock (_openclose) {
            if (Connection == null) { return false; }
            if (Connection.State == ConnectionState.Closed) {
                Connection.Open();
            }

            return Connection.State == ConnectionState.Open;
        }
    }

    //public abstract SqlBackAbstract OtherTable(string tablename);

    public void RenameColumn(string tablename, string oldname, string newname, bool allowSystemTableNames) {
        if (oldname.Equals(newname, StringComparison.OrdinalIgnoreCase)) { return; }

        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return;
        }

        if (!ColumnItem.IsValidColumnName(oldname)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + oldname);
            return;
        }

        if (!ColumnItem.IsValidColumnName(newname)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + newname);
            return;
        }

        //https://www.1keydata.com/sql/alter-table-rename-column.html
        var cmdString = @"ALTER TABLE " + tablename + " RENAME COLUMN " + oldname + " TO " + newname;
        _ = ExecuteCommand(cmdString, true);

        cmdString = "UPDATE " + SysStyle + " SET COLUMNNAME = " + Dbval(newname) + " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) + " AND COLUMNNAME = " + Dbval(oldname.ToUpper());
        _ = ExecuteCommand(cmdString, true);

        var test = GetStyleData(tablename, DatabaseDataType.ColumnName, newname, SysStyle);

        if (test != newname) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fataler Umbenennungs-Fehler!");
        }
    }

    public void RepairAll(string tablename) {
        Develop.StartService();

        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            return;
        }

        var x = AllTables();
        if (x == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Verbindung zur Datenbank gescheitert.");
            return;
        }

        #region Main

        if (!string.IsNullOrEmpty(tablename)) {
            if (!x.Contains(tablename.ToUpper())) {
                _ = CreateTable(tablename.ToUpper(), false);
                //ChangeDataType(tablename.ToUpper(), "RK", 15, false);
            }
        }

        #endregion

        if (!_didCritical) {
            _didCritical = true;

            #region Style

            if (!x.Contains(SysStyle)) {
                _ = CreateTable(SysStyle, new List<string> { "TABLENAME", "COLUMNNAME", "TYPE", "PART" }, true);
                ChangeDataType(SysStyle, "COLUMNNAME", 128, true);
                ChangeDataType(SysStyle, "TABLENAME", 128, true);
                ChangeDataType(SysStyle, "PART", 3, true);
            }

            var colStyle = GetColumnNames(SysStyle);
            if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

            if (!colStyle.Contains("VALUE")) {
                AddColumn(SysStyle, "VALUE", ColumnTypeVarChar4000, true, true);
                ChangeDataType(SysStyle, "VALUE", 511, true);
            }

            #endregion

            #region  Undo

            if (!x.Contains(SysUndo)) { _ = CreateTable(SysUndo, true); }

            var colUndo = GetColumnNames(SysUndo);
            if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
            if (!colUndo.Contains("TABLENAME")) { AddColumn(SysUndo, "TABLENAME", ColumnTypeVarChar255, false, true); }
            if (!colUndo.Contains("COMAND")) { AddColumn(SysUndo, "COMAND", false, true); }
            //if (!colUndo.Contains("COLUMNKEY")) { AddColumn(SysUndo, "COLUMNKEY", ColumnTypeVarChar15, true, true); }
            if (!colUndo.Contains("ROWKEY")) { AddColumn(SysUndo, "ROWKEY", ColumnTypeVarChar18, true, true); }
            if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SysUndo, "PREVIOUSVALUE", ColumnTypeVarChar4000, true, true); }
            if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SysUndo, "CHANGEDTO", ColumnTypeVarChar4000, true, true); }
            if (!colUndo.Contains("USERNAME")) { AddColumn(SysUndo, "USERNAME", false, true); }
            if (!colUndo.Contains("TIMECODEUTC")) { AddColumn(SysUndo, "TIMECODEUTC", ColumnTypeDate, false, true); }
            if (!colUndo.Contains("CMT")) { AddColumn(SysUndo, "CMT", ColumnTypeVarChar255, true, true); }
            if (!colUndo.Contains("COLUMNNAME")) { AddColumn(SysUndo, "COLUMNNAME", ColumnTypeVarChar255, true, true); }

            #endregion

            SysUndoAufräumen();
        }

        _ = CloseConnection();
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public string SetStyleData(string tablename, DatabaseDataType type, string columnName, string newValue) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return "Tabellenname ungültig";
        }

        if (type.Nameless()) { return string.Empty; }
        //command
        //if (type == DatabaseDataType.AddColumnKeyInfo) { return true; } // enthält zwar den KeyName, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
        //if (type == DatabaseDataType.AutoExport) { return true; }
        if (type == DatabaseDataType.UndoInOne) { return string.Empty; }
        var c = 0;
        var ok = string.Empty;
        var utf8 = newValue;
        var ty = type.ToString();

        columnName = columnName.ToUpper();
        if (string.IsNullOrEmpty(columnName)) { columnName = DatabaseProperty; }

        if (columnName != DatabaseProperty && !ColumnItem.IsValidColumnName(columnName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + tablename);
            return "Spaltenname ungültig";
        }

        var isVal = GetStyleData(tablename, type, columnName, SysStyle);
        if (isVal == null) { return "Kein Wert angekommen!"; }
        if (isVal == newValue) { return string.Empty; }

        var com = "BEGIN DELETE FROM " + SysStyle +
                  " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) +
                  " AND COLUMNNAME = " + Dbval(columnName.ToUpper()) +
                  " AND TYPE = " + Dbval(ty) + "; ";

        var maxPartStringLenght = Math.Min(MaxStringLenght, 500);

        do {
            c++;
            var tmp = utf8.CutToUtf8Length(maxPartStringLenght);

            var cmdString = "INSERT INTO " + SysStyle + " (TABLENAME, TYPE, COLUMNNAME, VALUE, PART)  VALUES (" + Dbval(tablename.ToUpper()) + ", " + Dbval(ty) + ", " + Dbval(columnName.ToUpper()) + ", " + Dbval(tmp) + ", " + Dbval(c.ToString(Format_Integer3)) + "); ";

            com += cmdString;

            if (tmp == utf8) { break; }

            utf8 = utf8.Substring(tmp.Length);
        } while (true);

        var ok2 = ExecuteCommand(com + "END; ", true);
        if (!string.IsNullOrEmpty(ok2)) { return "Eigenschaft nicht gesetzt: " + ok2; }

        if (type == DatabaseDataType.ColumnName) {
            // Wichtig, erst den Wert setzen, dann umbenennen! Somit wird der Wert richtig mit umbenannt
            RenameColumn(tablename, columnName.ToUpper(), newValue.ToUpper(), false);
        }
        if (type == DatabaseDataType.MaxCellLenght) {
            ChangeDataType(tablename, columnName.ToUpper(), IntParse(newValue), false);
        }

        return ok;
    }

    /// <summary>
    /// Führt eine eine Änderung im BackEnd aus
    /// </summary>
    /// <param name="db"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="user"></param>
    /// <param name="datetimeutc"></param>
    /// <returns></returns>
    public string SetValueInternal(DatabaseSqlLite db, DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc) {
        if (!IsValidTableName(db.TableName, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + db.TableName);
            return "Tabellenname ungültig: " + db.TableName;
        }

        #region Ignorieren !!!! AUCH SYSTEMVALUE !!!

        switch (type) {
            case DatabaseDataType.Formatkennung:
            case DatabaseDataType.Werbung:
            case DatabaseDataType.SystemValue:
            //case DatabaseDataType.CryptionState:
            //case DatabaseDataType.CryptionTest:
            //case DatabaseDataType.SaveContent:
            case DatabaseDataType.EOF:
                return string.Empty;
        }

        if (type.IsObsolete()) { return string.Empty; }

        #endregion

        if (!OpenConnection()) { return "Verbindung fehlgeschlagen"; }

        #region Datenbank Eigenschaften

        if (type.IsDatabaseTag()) {
            return SetStyleData(db.TableName, type, string.Empty, value);
        }

        #endregion

        #region Spalten Eigenschaften

        if (type.IsColumnTag() && column != null) {
            return SetStyleData(db.TableName, type, column.KeyName.ToUpper(), value);
        }

        #endregion

        #region Zellen-Wert

        if (type.IsCellValue() && column != null && row != null) {
            //  type != DatabaseDataType.SystemValue wird oben schon abgefragt
            var l = new List<CellValueEventArgs> { new(column, value) };

            if (db.Column.SysRowChangeDate is ColumnItem srd && column != srd) { l.Add(new CellValueEventArgs(srd, datetimeutc.ToString(Format_Date7))); }
            if (db.Column.SysRowChanger is ColumnItem src && column != src) { l.Add(new CellValueEventArgs(src, user)); }

            if (column.ScriptType != ScriptType.Nicht_vorhanden) {
                if (db.Column.SysRowState is ColumnItem srs && column != srs) { l.Add(new CellValueEventArgs(srs, string.Empty)); }
            }
            return SetCellValue(db.TableName, row.KeyName, l);
        }

        #endregion

        #region Befehle

        if (type.IsCommand()) {
            switch (type) {
                //case DatabaseDataType.Comand_AddColumn:
                //case DatabaseDataType.Comand_AddColumnByKey:
                //    return AddColumnToMain(tablename, ColumnItem.TmpNewDummy, (long)columnkey);

                case DatabaseDataType.Comand_AddColumnByName:
                    return AddColumnToMain(db.TableName, value, false);

                case DatabaseDataType.Comand_RemoveColumn:
                    if (column == null) { return "Spalte nicht definiert!"; }
                    return RemoveColumn(db.TableName, column.KeyName.ToUpper(), false);

                case DatabaseDataType.Comand_RemoveRow:
                    return RemoveRow(db.TableName, value, false);

                case DatabaseDataType.Comand_AddRow:
                    return string.Empty;//  AddRow(db.TableName, LongParse(value));

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, type + " nicht definiert!");
                    return "Befehl nicht definiert";
            }
        }

        #endregion

        _ = CloseConnection();
        return string.Empty;
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - außer die Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    public List<string>? Tables() {
        var l = AllTables();

        if (l == null) { return null; }

        //_ = l.Remove(SysStyle);
        //_ = l.Remove(SysUndo);

        var l2 = new List<string>();

        foreach (var thiss in l) {
            var thiss2 = thiss.ToUpper();

            if (!thiss2.StartsWith("SYS_") && !thiss2.StartsWith("BAK_")) {
                l2.Add(thiss2);
            }
        }

        return l2;
    }

    public abstract string VarChar(int lenght);

    //    if (allcols.Count > 0) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Zusätzliche Spalten dem Server vorhanden: " + allcols.JoinWith(", "));
    //    }
    internal static string MakeValidColumnName(string columnname) => columnname.ToUpper().Replace(" ", "_").ReduceToChars(AllowedCharsVariableName);

    /// <summary>
    ///
    /// </summary>
    /// <param name="db"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns>Gibt NULL zurück, wenn die Daten nicht geladen werden konnten</returns>
    internal List<UndoItem>? GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromDate, DateTime toDate) {
        lock (_getChanges) {
            var commandText = @"select TABLENAME, COMAND, COLUMNNAME, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, CMT, TIMECODEUTC from " + SysUndo + " ";

            // nur bestimmte Tabellen
            commandText += "WHERE (";
            foreach (var thisdb in db) {
                commandText += "TABLENAME=" + Dbval(thisdb.TableName.ToUpper()) + " OR ";
            }
            commandText = commandText.TrimEnd(" OR ") + ") AND ";

            // Zeit einbeziehen
            commandText += "(TIMECODEUTC BETWEEN " + Dbval(fromDate) + " AND " + Dbval(toDate) + ")";

            // Sortierung nach Tabellen
            commandText += " ORDER BY TIMECODEUTC ASC";

            var fb = new List<UndoItem>();

            var dt = Fill_Table(commandText);

            foreach (var thisRow in dt.Rows) {
                var reader = (DataRow)thisRow;
                if (Enum.TryParse(reader[1].ToString(), out DatabaseDataType t)) {
                    if (!t.IsObsolete()) {
                        var wi = new UndoItem(reader[0].ToString(),
                                             t,
                                             reader[2].ToString(),
                                             reader[3].ToString(),
                                             reader[4].ToString(),
                                             reader[5].ToString(),
                                             reader[6].ToString(),
                                             reader[7].ToString(),
                                             DateTimeParse(reader[8].ToString()));
                        fb.Add(wi);
                    }
                }
            }

            return fb;
        }
    }

    internal SqlBackAbstract? HandleMe(ConnectionInfo ci) {
        if (ci is null) { return null; }

        foreach (var thisK in ConnectedSqlBack) {
            if (thisK.Connection != null && thisK.ConnectionString == ci.DatabaseId) {
                return thisK;

                //return thisK.OtherTable(ci.TableName);
            }
        }

        return null;
    }

    //        _ = CloseConnection();
    //        return value;
    //    } catch {
    //        _ = CloseConnection();
    //        Develop.CheckStackForOverflow();
    //        return GetLastColumnName(tablename, key);
    //    }
    //}
    internal void LoadColumns(string tablename, RowCollection rows, List<ColumnItem> columns, bool addmissingRows, List<FilterItem>? preselection) {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            throw new Exception();
        }

        if (rows.Database is not DatabaseAbstract db || db.IsDisposed) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank ungültig!");
            throw new Exception();
        }

        if (columns.Count == 0 && !addmissingRows) { return; }

        #region tatsächlich benötigte Spalten ermitteln (columnsToLoad)

        var columnsToLoad = new List<ColumnItem>();

        foreach (var thisc in columns) {
            if (thisc != null && thisc.IsInCache == null) {
                _ = columnsToLoad.AddIfNotExists(thisc);
            }
        }

        if (columnsToLoad.Count == 0 && !addmissingRows) { return; }

        #endregion

        #region Command erstellen

        var wh = string.Empty;
        var com = "SELECT ROWID, ";

        foreach (var thiscolumn in columnsToLoad) {
            com = com + thiscolumn.KeyName.ToUpper() + ", ";
            wh = wh + thiscolumn.KeyName.ToUpper() + " IS NOT NULL OR ";
        }
        com = com.TrimEnd(", ");
        com = com + " FROM " + tablename.ToUpper();

        if (!addmissingRows) {
            com = com + " WHERE (" + wh.TrimEnd(" OR ") + ")";
        }

        #endregion

        var dt = Fill_Table(com);

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;

            #region Zeile ermitteln, in die der Wert geschrieben werden soll

            var rk = reader[0].ToString();
            var row = rows.SearchByKey(rk);

            if ((row == null || row.IsDisposed) && addmissingRows) {
                _ = rows.SetValueInternal(DatabaseDataType.Comand_AddRow, rk, null, Reason.LoadReload);
                row = rows.SearchByKey(rk);
            }

            #endregion

            if (row != null && !row.IsDisposed) {
                for (var z = 1; z < dt.Columns.Count; z++) {
                    _ = db.Cell.SetValueInternal(columnsToLoad[z - 1], row, reader[z].ToString(), Reason.LoadReload);
                }
            }
        }

        foreach (var thiscolumn in columnsToLoad) {
            thiscolumn.IsInCache = DateTime.UtcNow;
        }
    }

    protected void CompareBackUp(DateTime compareDate) {
        var tbl = Tables();
        if (tbl == null) { return; }

        tbl.Add(SysStyle);

        var alltb = AllTables();
        if (alltb == null) { return; }

        var l = new List<string>();

        var d = compareDate.ToString(Format_Date10);

        foreach (var thist in tbl) {
            l.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            l.Add("~~~~~~ " + thist);
            l.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            var ntc = "BAK_" + thist.ToUpper() + "_" + d;

            if (alltb.Contains(ntc)) {
                var dt1 = Fill_Table("SELECT * FROM " + thist + " MINUS SELECT * FROM " + ntc);
                var dt2 = Fill_Table("SELECT * FROM " + ntc + " MINUS SELECT * FROM " + thist);
                l.Add("Vergleich 1 Zeilen:" + dt1.Rows.Count);
                l.Add("Vergleich 2 Zeilen:" + dt2.Rows.Count);
                //l.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                //l.Add(dt1.ToCSV());
                //l.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                //l.Add(dt2.ToCSV());
                l.Add(" ");
                l.Add(" ");
            }
        }

        l.WriteAllText(TempFile("", "", "txt"), Win1252, true);
    }

    protected string CreateTable(string tablename, bool allowSystemTableNames) => CreateTable(tablename, new List<string>(), allowSystemTableNames);

    //    try {
    //        using var q = _connection.CreateCommand();
    protected abstract string CreateTable(string tablename, List<string> keycolumns, bool allowSystemTableNames);

    protected string Dbval(long original) => Dbval(original.ToString());

    protected string Dbval(DateTime date) =>
        "to_timestamp(" +
        Dbval(date.ToString(Format_Date9)) + ", " +
        Dbval(Format_Date9.ToUpper().Replace(":MM:", ":MI:").Replace("HH:", "HH24:").Replace(".FFF", ".FF3")) +
        ")";

    protected string Dbval(string? original) {
        if (original == null) { return "''"; }

        original = original.CutToUtf8Length(MaxStringLenght);
        original = original.Replace("'", "''");
        return "'" + original + "'";
    }

    protected abstract string DeleteTable(string tablename, bool allowSystemTableNames);

    /// <summary>
    /// Erstellt von allen Datenbanken eine Backup auf dem Server im Format BAK_TABLENAME_YYYY_MM_DD.
    /// Nur der erste Aufruf der Routine wir wirklich ausgeführt.
    /// </summary>
    protected void DoBackUp() {
        if (_didBackup) { return; }
        _didBackup = true;

        try {
            var tbl = Tables();
            if (tbl == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Verbindung zur Datenbank gescheitert.");
                return;
            }

            tbl.Add(SysStyle);

            var alltb = AllTables();

            if (alltb == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Verbindung zur Datenbank gescheitert.");
                return;
            }

            #region Kopie des aktuellen Standes erstellen

            var d = DateTime.UtcNow.ToString(Format_Date10);

            foreach (var thist in tbl) {
                var ntc = "BAK_" + thist.ToUpper() + "_" + d;

                if (!alltb.Contains(ntc)) {
                    if (CopyTable(thist, ntc)) {
                        alltb.Add(ntc);
                    }
                }
            }

            #endregion

            #region Altes Zeugs ermitteln und löschen

            foreach (var thist in tbl) {
                var ntc = "BAK_" + thist.ToUpper() + "_";
                var l = new BackupVerwalter(3, 1);
                foreach (var thisat in alltb) {
                    if (thisat.StartsWith(ntc)) {
                        if (DateTimeTryParse(thisat.TrimStart(ntc), out var dt)) {
                            l.AddData(dt, thisat);
                        }
                    }
                }

                foreach (var thisttd in l.Deleteable) {
                    if (thisttd.StartsWith("BAK_")) {
                        DeleteTable(thisttd, true);
                    }
                }
            }

            #endregion
        } catch { }
    }

    private void AddColumn(string tablename, string column, bool nullable, bool allowSystemTableNames) => AddColumn(tablename, column, ColumnTypeVarChar255, nullable, allowSystemTableNames);

    private void AddColumn(string tablename, string column, string type, bool nullable, bool allowSystemTableNames) {
        if (string.IsNullOrEmpty(column)) {
            Develop.DebugPrint(FehlerArt.Warnung, "Spalte ohne Namen!");
            return;
        }

        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return;
        }

        var n = " NOT NULL";
        if (nullable) { n = string.Empty; }

        _ = ExecuteCommand("ALTER TABLE " + tablename.ToUpper() + " add " + column + " " + type + " default ''" + n, true);
    }

    //private string AddRow(string tablename, long key) => ExecuteCommand("INSERT INTO " + tablename.ToUpper() + " (RK) VALUES (" + Dbval(key) + ")", true);

    private bool CopyTable(string tablename, string newtablename) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return false;
        }

        if (!IsValidTableName(newtablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return false;
        }

        var s = "CREATE TABLE " + newtablename + " AS SELECT * FROM " + tablename;

        return string.IsNullOrEmpty(ExecuteCommand(s, false));
    }

    private string ExecuteCommand(IDbCommand command, bool abort) {
        LastLoadUtc = DateTime.UtcNow;

        if (Connection == null || !OpenConnection()) { return "Verbindung konnte nicht geöffnet werden"; }
        command.CommandTimeout = 10;

        using var transaction = Connection.BeginTransaction();
        command.Transaction = transaction;

        try {
            if (Log.Count > 2000) { Log.RemoveAt(0); }
            Log.Add("[" + DateTime.UtcNow.ToString(Format_Date) + "]\r\n" + command.CommandText);
            _ = command.ExecuteNonQuery();
            transaction.Commit();
            return string.Empty;
        } catch (Exception ex) {
            transaction.Rollback();
            if (abort) {
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Befehl konnte nicht ausgeführt werden: " + command.CommandText, ex);
            }

            Develop.DebugPrint("Datenbank Befehl konnte nicht ausgeführt werden: " + command.CommandText, ex);
            return "Allgemeiner Fehler beim Ausführen, siehe Protokoll";
        } finally {
            _ = CloseConnection();
        }
    }

    //private (string? value, string error) GetCellValue(string tablename, string columnname, string rowKey) {
    //    if (!IsValidTableName(tablename, false)) {
    //            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
    //            return (null, "Tabellenname ungültig: " + tablename);
    //        }

    //        var comm = @"select " + columnname.ToUpper() + " from " + tablename.ToUpper() + " where ROWID = " + Dbval(rowkey);

    //        var dt = Fill_Table(comm);

    //        if (dt.Rows.Count == 0) { return (null, string.Empty); }
    //        if (dt.Rows.Count > 1) {
    //            // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
    //            Develop.DebugPrint(columnname + " doppelt in " + tablename.ToUpper() + " vorhanden!");
    //        }

    //        return (dt.Rows[0][0].ToString(), string.Empty);
    //}

    private void PauseSystem() {
        while (DateTime.UtcNow.Subtract(LastLoadUtc).TotalMilliseconds < 1) { }
        LastLoadUtc = DateTime.UtcNow;
    }

    private string RemoveColumn(string tablename, string column, bool allowSystemTableNames) {
        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);

            return "Tabellenname ungültig: " + tablename;
        }

        if (!ColumnItem.IsValidColumnName(column)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + column);
            return "Spaltenname ungültig: " + column;
        }

        var b = ExecuteCommand("ALTER TABLE " + tablename.ToUpper() + " drop column " + column.ToUpper(), true);
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }

        b = ExecuteCommand("DELETE FROM " + SysStyle + " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) + " AND COLUMNNAME = " + Dbval(column.ToUpper()), true);
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }
        return string.Empty;
    }

    private string RemoveRow(string tablename, string rowkey, bool allowSystemTableNames) {
        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return "Tabellenname ungültig: " + tablename;
        }

        if (!RowItem.IsValidRowKey(rowkey)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Zeilenschlüssel ungültig: " + rowkey);
            return "Zeilenschlüssel ungültig: " + rowkey;
        }

        var b = ExecuteCommand("DELETE FROM " + tablename.ToUpper() + " WHERE ROWID = " + Dbval(rowkey), true);
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }
        return string.Empty;
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="rowkey"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private string SetCellValue(string tablename, string rowkey, List<CellValueEventArgs> values) {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return "Tabellenname ungültig: " + tablename;
        }

        if (!RowItem.IsValidRowKey(rowkey)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Zeilenschlüssel ungültig: " + rowkey);
            return "Zeilenschlüssel ungültig: " + rowkey;
        }

        var sc = string.Empty;

        foreach (var thisv in values) {
            if (thisv.Column.Database is not DatabaseSqlLite db || db.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank ungültig");
                return "Datenbank ungültig";
            }

            if (!ColumnItem.IsValidColumnName(thisv.Column.KeyName)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + thisv.Column.KeyName);
                return "Spaltenname ungültig: " + thisv.Column.KeyName;
            }

            if (db.TableName != tablename) {
                Develop.DebugPrint(FehlerArt.Fehler, "Tabelle ungültig: " + tablename);
                return "Tabelle ungültig: " + tablename;
            }

            if (!string.IsNullOrEmpty(sc)) { sc += ", "; }
            sc = sc + thisv.Column.KeyName.ToUpper() + " = " + Dbval(thisv.NewValue);
        }

        return ExecuteCommand("UPDATE " + tablename.ToUpper() + " SET " + sc + " WHERE ROWID = " + Dbval(rowkey), false);
    }

    private void SysUndoAufräumen() {
        var c = "DELETE FROM " + SysUndo +
                " WHERE RK NOT IN " +
                "(SELECT RK FROM (SELECT RK, ROW_NUMBER() OVER (PARTITION BY TABLENAME ORDER BY TIMECODEUTC DESC) as row_num FROM " + SysUndo + ") WHERE row_num <= 500)" +
                "AND TIMECODEUTC<" + Dbval(DateTime.UtcNow.AddDays(-7));

        _ = ExecuteCommand(c, false);
    }

    #endregion
}