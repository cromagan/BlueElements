﻿// Authors:
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
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

//https://www.c-sharpcorner.com/article/create-a-sql-server-database-dynamically-in-C-Sharp/
//https://www.ictdemy.com/csharp/databases/introduction-to-databases-in-csharp-net
//https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-sql-server-database-programmatically
public abstract class SqlBackAbstract {

    #region Fields

    public const string SysStyle = "SYS_STYLE";
    public const string SysUndo = "SYS_UNDO";
    public static List<SqlBackAbstract> ConnectedSqlBack = new();

    //public static List<SQLBackAbstract>? PossibleSQLBacks;
    protected DbConnection? Connection;

    private readonly object _fill = new();
    private readonly object _getChanges = new();
    private readonly object _getRow = new();
    private readonly object _openclose = new();

    #endregion

    #region Constructors

    public SqlBackAbstract() =>
        //GetSQLBacks();
        ConnectedSqlBack.Add(this);

    #endregion

    #region Properties

    public bool ConnectionOk => Connection != null;
    public string ConnectionString { get; protected set; } = string.Empty;

    public string Date => "DATE";

    /// <summary>
    /// Falls die Datenbank von einer lokalen Datei geladen wurde, ist hier der Dateiname enthalten.
    /// </summary>
    public string Filename { get; protected set; } = string.Empty;

    public abstract int MaxStringLenght { get; }
    public abstract string Primary { get; }

    public string VarChar15 => VarChar(15);
    public string VarChar255 => VarChar(255);
    public string VarChar4000 => VarChar(4000);

    #endregion

    #region Methods

    public static bool IsValidTableName(string tablename) {
        var t = tablename.ToUpper();

        if (t.StartsWith("SYS_")) { return false; }

        if (!t.ContainsOnlyChars(Constants.Char_AZ + Constants.Char_Numerals + "_")) { return false; }

        if (t.Length > 128) { return false; }

        return true;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Constants.Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().ToLower().Replace(" ", "_").Replace("-", "_");
        tmp = tmp.StarkeVereinfachung("_").ToUpper();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    /// <summary>
    /// Erstellt eine Spalte nur bei Bedarf.
    /// Aber der Key wird aktualisiert.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="columnName"></param>
    public string AddColumnToMain(string tablename, string columnName) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames(tablename.ToUpper());
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return "Spalte nicht gefunden"; }
        if (!colMain.Contains(columnName)) { AddColumn(tablename.ToUpper(), columnName, VarChar4000, true); }

        return SetStyleData(tablename, DatabaseDataType.ColumnName, columnName.ToUpper(), columnName.ToUpper());
    }

    public string AddUndo(string tablename, DatabaseDataType comand, string? columname, long? rowKey, string previousValue, string changedTo, string userName, string comment) {
        if (!OpenConnection() || Connection == null) { return "Verbindung fehlgeschlagen"; }

        //var ck = columnKey is not null and > (-1) ? columnKey.ToString() : string.Empty;
        var rk = rowKey is not null and > -1 ? rowKey.ToString() : string.Empty;

        var cmdString = "INSERT INTO " + SysUndo +
            " (TABLENAME, COMAND, COLUMNNAME, ROWKEY, PREVIOUSVALUE, CHANGEDTO, USERNAME, TIMECODEUTC, CMT) VALUES (" +
             Dbval(tablename.ToUpper()) + "," +
             Dbval(comand.ToString()) + "," +
             Dbval(columname) + "," +
             Dbval(rk) + "," +
             Dbval(previousValue) + "," +
             Dbval(changedTo) + "," +
             Dbval(userName) + "," +
             //DBVAL(dt.ToString(Constants.Format_Date)) + "," +
             Dbval(DateTime.UtcNow) + "," +
             Dbval(comment) + ")";

        using var comm = Connection.CreateCommand();
        comm.CommandText = cmdString;

        return ExecuteCommand(comm);
    }

    public void ChangeDataType(string tablename, string column, int charlenght) {
        //https://stackoverflow.com/questions/10321775/changing-the-data-type-of-a-column-in-oracle

        var s = (DataFormat)IntParse(GetStyleData(tablename, DatabaseDataType.ColumnFormat.ToString(), column));

        if (s == DataFormat.Verknüpfung_zu_anderer_Datenbank) { charlenght = Math.Max(charlenght, 35); }
        if (s == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { charlenght = Math.Max(charlenght, 15); }

        var cmdString = @"ALTER TABLE " + tablename + " MODIFY (" + column + " " + VarChar(charlenght) + ")";
        _ = ExecuteCommand(cmdString);
    }

    public bool CloseConnection() {
        if (Connection == null) { return true; }

        lock (_openclose) {
            if (Connection.State == ConnectionState.Open) { Connection.Close(); }

            return Connection.State == ConnectionState.Closed;
        }
    }

    /// <summary>
    /// Provider ist immer NULL!
    /// </summary>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public ConnectionInfo ConnectionData(string tablename) => new(tablename, null, ConnectionString, string.Empty);

    /// <summary>
    /// Datentabelle befüllen
    /// </summary>
    /// <returns>befüllte  Tabelle - Datatable</returns>
    public DataTable? Fill_Table(string commandtext) {
        if (!OpenConnection() || Connection == null) { return null; }

        try {
            lock (_fill) {
                var tbl = new DataTable();
                using var command = Connection.CreateCommand();
                command.CommandText = commandtext;
                _ = OpenConnection();
                tbl.Load(command.ExecuteReader());
                _ = CloseConnection();
                //if (TBL.Rows.Count == 1) {
                //    Develop.DebugPrint("Müssig");
                //}
                return tbl;
            }
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            return Fill_Table(commandtext);
        }
    }

    /// <summary>
    /// Gibt die Spaltenname in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public abstract List<string>? GetColumnNames(string tablename);

    public string? GetStyleData(string tablename, string type, string columnName) {
        try {
            if (!OpenConnection()) { return null; }

            //using var q = _connection.CreateCommand();

            if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

            var cmd = @"select VALUE, PART from " + SysStyle + " " +
                            "where TABLENAME = " + Dbval(tablename.ToUpper()) + " " +
                            "and TYPE = " + Dbval(type) + " " +
                            "and COLUMNNAME = " + Dbval(columnName.ToUpper()) + " " +
                            "ORDER BY PART ASC";

            var dt = Fill_Table(cmd);

            if (dt == null) { return null; }

            var value = string.Empty;

            foreach (var thisRow in dt.Rows) {
                value += ((DataRow)thisRow)[0].ToString();
            }
            //while (reader.Read()) {
            //    value += reader[0].ToString(false);
            //}

            //CloseConnection();    // Nix vorhanden!
            return value;
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            return GetStyleData(tablename, type, columnName);
        }
    }

    /// <summary>
    /// Wird kein Spaltenname angegeben, werden die Eigenschaften der Datenbank zurück gegeben.
    /// Achtung, die Connection wird nicht geschlossen!
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetStyleDataAll(string tablename, string columnName) {
        try {
            var l = new Dictionary<string, string>();

            if (!OpenConnection() || Connection == null) { return l; }

            using var q = Connection.CreateCommand();

            q.CommandText = @"select TYPE, PART, VALUE from " + SysStyle + " " +
                            "where TABLENAME = " + Dbval(tablename.ToUpper()) + " " +
                            "and COLUMNNAME = " + Dbval(columnName.ToUpper()) + " " +
                            "ORDER BY PART ASC";

            using var reader = q.ExecuteReader();

            while (reader.Read()) {
                var key = reader[0].ToString();
                var value = reader[2].ToString();

                if (reader[1].ToString() != "001") {
                    l[key] += value;
                } else {
                    l.Add(key, value);
                }
            }

            //CloseConnection();    // Nix vorhanden!
            return l;
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            return GetStyleDataAll(tablename, columnName);
        }
    }

    public void LoadAllRowKeys(string tablename, RowCollection row) {
        try {
            if (!OpenConnection()) { return; }

            var com = "SELECT RK ";
            com = com + " FROM " + tablename.ToUpper();

            var dt = Fill_Table(com);
            if (dt == null) {
                _ = CloseConnection();
                Develop.CheckStackForOverflow();
                LoadAllRowKeys(tablename, row);
                return;
            }
            //OpenConnection();

            //using var command = _connection.CreateCommand();
            //command.CommandText = com;

            //using var reader = command.ExecuteReaderAsync();

            _ = row.Clear("Row Keys werden neu geladen");

            foreach (var thisRow in dt.Rows) {
                var rk = LongParse(((DataRow)thisRow)[0].ToString());
                _ = row.SetValueInternal(DatabaseDataType.Comand_AddRow, rk, true);
                //var r = row.GenerateAndAdd(rk, string.Empty, false, false);
            }

            _ = CloseConnection();
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            LoadAllRowKeys(tablename, row);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="row"></param>
    /// <param name="refreshAlways">Bei TRUE wird die gesamte Zeile aktualistert, weil evtl. eine Änderung aufgetreten ist. Bei FLASE werden nur die fehlenden Daten der noch nicht geladenen Spalten nachgeladen.</param>
    /// <param name="sortedRows"></param>
    public void LoadRow(string tablename, List<RowItem> row, bool refreshAlways, List<RowItem>? sortedRows) {
        try {
            if (row.Count == 0 || row[0] is null) { return; }

            var db = row[0]?.Database;
            if (db == null || db.IsDisposed) { return; }

            if (!OpenConnection() || Connection == null) { return; }

            lock (_getRow) {
                var com = new StringBuilder();
                com.Append("SELECT RK, ");

                var count = 0;

                foreach (var thiscolumn in db.Column) {
                    if (refreshAlways || thiscolumn.IsInCache == null) {
                        if (count > 0) { com.Append(", "); }

                        count++;
                        com.Append(thiscolumn.Name.ToUpper());
                    }
                }

                if (count == 0) { return; }

                _ = db.Row.FillUp100(row, sortedRows);

                com.Append(" FROM " + tablename.ToUpper());

                if (row.Count < 1000) {
                    com.Append(" WHERE ");
                    count = 0;
                    foreach (var thisr in row) {
                        if (count > 0) { com.Append(" OR "); }

                        count++;
                        com.Append("RK = " + Dbval(thisr.Key));
                    }
                }

                if (!OpenConnection() || Connection == null) { return; }

                var dt = Fill_Table(com.ToString());

                if (dt == null) { return; }

                foreach (var thisRow in dt.Rows) {
                    var reader = (DataRow)thisRow;
                    var rk = LongParse(reader[0].ToString());
                    var r = db.Row.SearchByKey(rk) ?? db.Row.GenerateAndAdd(rk, string.Empty, false, false, "Load row");

                    if (r != null) {
                        for (var z = 1; z < dt.Columns.Count; z++) {
                            var cx = db.Column.Exists(dt.Columns[z].ColumnName);
                            if (cx != null) {
                                _ = db.Cell.SetValueInternal(cx.Name, r.Key, reader[z].ToString(), true);
                            }
                        }

                        r.IsInCache = DateTime.UtcNow;
                    }
                    //if (!r.RowInChache()) {
                    //    Develop.DebugPrint(FehlerArt.Warnung, "Zeile ohne Zeitstempel, repariert! + " + com);
                    //    r.CellSet(r.Database.Column.SysRowChangeDate, DateTime.UtcNow.ToString(Constants.Format_Date5));
                    //}
                }
                _ = CloseConnection();

                db.Row.DoLinkedDatabase(row);
            }
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            LoadRow(tablename, row, refreshAlways, sortedRows);
        }
    }

    //    Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds + " Sekunden");
    public bool OpenConnection() {
        lock (_openclose) {
            if (Connection == null) { return false; }
            if (Connection.State == ConnectionState.Closed) {
                Connection.Open();
            }

            return Connection.State == ConnectionState.Open;
        }
    }

    //        for (var z = 1; z < reader.FieldCount; z++) {
    //            row.Database.Cell.SetValueInternal(row.Database?.Column[z - 1].Key, r, reader[z].ToString(false), -1, -1);
    //        }
    //    }
    public abstract SqlBackAbstract OtherTable(string tablename);

    //    while (await reader.ReadAsync()) {
    //        var rk = LongParse(reader[0].ToString(false));
    //        var r = row.GenerateAndAdd(rk, string.Empty, false, false);
    public void RenameColumn(string tablename, string oldname, string newname) {
        if (oldname.Equals(newname, StringComparison.OrdinalIgnoreCase)) { return; }

        //https://www.1keydata.com/sql/alter-table-rename-column.html
        var cmdString = @"ALTER TABLE " + tablename + " RENAME COLUMN " + oldname + " TO " + newname;
        _ = ExecuteCommand(cmdString);

        cmdString = "UPDATE " + SysStyle + " SET COLUMNNAME = " + Dbval(newname) + " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) + " AND COLUMNNAME = " + Dbval(oldname.ToUpper());
        _ = ExecuteCommand(cmdString);

        var test = GetStyleData(tablename, DatabaseDataType.ColumnName.ToString(), newname);

        if (test != newname) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fataler Umbenennungs-Fehler!");
        }
    }

    //    row.Clear();
    //    var ti = DateTime.Now;
    public void RepairAll(string tablename) {
        Develop.StartService();

        if (!IsValidTableName(tablename)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            return;
        }

        var x = AllTables();

        #region Main

        if (!string.IsNullOrEmpty(tablename)) {
            if (!x.Contains(tablename.ToUpper())) {
                _ = CreateTable(tablename.ToUpper(), new List<string> { "RK" });
                ChangeDataType(tablename.ToUpper(), "RK", 15);
            }
        }

        #endregion

        #region Style

        if (!x.Contains(SysStyle)) {
            _ = CreateTable(SysStyle, new List<string> { "TABLENAME", "COLUMNNAME", "TYPE", "PART" });
            ChangeDataType(tablename.ToUpper(), "COLUMNNAME", 128);
            ChangeDataType(tablename.ToUpper(), "TABLENAME", 128);
            ChangeDataType(tablename.ToUpper(), "PART", 3);
        }

        var colStyle = GetColumnNames(SysStyle);
        if (colStyle == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }

        if (!colStyle.Contains("VALUE")) { AddColumn(SysStyle, "VALUE", VarChar4000, true); }

        #endregion

        #region  Undo

        if (!x.Contains(SysUndo)) { _ = CreateTable(SysUndo); }

        var colUndo = GetColumnNames(SysUndo);
        if (colUndo == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return; }
        if (!colUndo.Contains("TABLENAME")) { AddColumn(SysUndo, "TABLENAME", VarChar255, false); }
        if (!colUndo.Contains("COMAND")) { AddColumn(SysUndo, "COMAND", false); }
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn(SysUndo, "COLUMNKEY", VarChar15, true); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn(SysUndo, "ROWKEY", VarChar15, true); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SysUndo, "PREVIOUSVALUE", VarChar4000, true); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SysUndo, "CHANGEDTO", VarChar4000, true); }
        if (!colUndo.Contains("USERNAME")) { AddColumn(SysUndo, "USERNAME", false); }
        if (!colUndo.Contains("TIMECODEUTC")) { AddColumn(SysUndo, "TIMECODEUTC", Date, false); }
        if (!colUndo.Contains("CMT")) { AddColumn(SysUndo, "CMT", VarChar255, true); }
        if (!colUndo.Contains("COLUMNNAME")) { AddColumn(SysUndo, "COLUMNNAME", VarChar255, true); }

        #endregion

        SysUndoAufräumen();

        _ = CloseConnection();
    }

    //    using var reader = await command.ExecuteReaderAsync();
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
        if (type.Nameless()) { return string.Empty; }
        //command
        //if (type == DatabaseDataType.AddColumnKeyInfo) { return true; } // enthält zwar den Key, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
        //if (type == DatabaseDataType.AutoExport) { return true; }
        if (type == DatabaseDataType.UndoInOne) { return string.Empty; }
        var c = 0;
        var ok = string.Empty;
        var utf8 = newValue;
        var ty = type.ToString();

        columnName = columnName.ToUpper();
        if (string.IsNullOrEmpty(columnName)) { columnName = "~Database~"; }

        var isVal = GetStyleData(tablename, ty, columnName);
        if (isVal == null) { return "Kein Wert angekommen!"; }
        if (isVal == newValue) { return string.Empty; }

        _ = ExecuteCommand("DELETE FROM " + SysStyle +
                       " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) +
                       " AND COLUMNNAME = " + Dbval(columnName.ToUpper()) +
                       " AND TYPE = " + Dbval(type.ToString()));

        do {
            c++;
            var tmp = utf8.CutToUtf8Length(MaxStringLenght);

            var ok2 = SetStyleData(tablename, ty, columnName, tmp, c);
            if (!string.IsNullOrEmpty(ok2)) { ok = ok2; }

            if (tmp == utf8) { break; }

            utf8 = utf8.Substring(tmp.Length);
        } while (true);

        if (type == DatabaseDataType.ColumnName) {
            // Wichtig, erst den Wert seetzen, dann umbenennen! Somit wird der Wert richtig mit umbenannt
            RenameColumn(tablename, columnName.ToUpper(), newValue.ToUpper());
        }
        if (type == DatabaseDataType.MaxTextLenght) {
            ChangeDataType(tablename, columnName.ToUpper(), IntParse(newValue));
        }

        //if (type == DatabaseDataType.ColumnName) {
        //    //var test = GetStyleData(tablename, DatabaseDataType.ColumnName.ToString(false), columnName);
        //    //if (!string.IsNullOrEmpty(test)) {
        //    //    Develop.DebugPrint(FehlerArt.Fehler, "Fataler Namensfehler 1!");
        //    //}

        //    //if (columnName != newValue && columnName != ColumnItem.TmpNewDummy) {
        //    //    Develop.DebugPrint(FehlerArt.Fehler, "Fataler Namensfehler 1: " + columnName + " -> " + newValue);
        //    //}
        //    test = GetStyleData(tablename, DatabaseDataType.ColumnName.ToString(false), newValue);
        //    if (newValue != test) {
        //        Develop.DebugPrint(FehlerArt.Fehler, "Fataler Namensfehler 2!");
        //    }
        //}

        return ok;
    }

    /// <summary>
    /// Führt eine eine Änderung im BackEnd aus
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="columname"></param>
    /// <param name="rowkey"></param>
    /// <returns></returns>
    public string SetValueInternal(string tablename, DatabaseDataType type, string value, string? columname, long? rowkey) {

        #region Ignorieren

        switch (type) {
            case DatabaseDataType.Formatkennung:
            case DatabaseDataType.Werbung:
            //case DatabaseDataType.CryptionState:
            //case DatabaseDataType.CryptionTest:
            case DatabaseDataType.SaveContent:
            case DatabaseDataType.EOF:
                return string.Empty;
        }

        if (type.IsObsolete()) { return string.Empty; }

        #endregion

        if (!OpenConnection()) { return "Verbindung fehlgeschlagen"; }

        #region Datenbank Eigenschaften

        if (type.IsDatabaseTag()) {
            return SetStyleData(tablename, type, string.Empty, value);
        }

        #endregion

        #region Spalten Eigenschaften

        if (type.IsColumnTag() && columname != null) {
            return SetStyleData(tablename, type, columname.ToUpper(), value);
        }

        #endregion

        #region Zellen-Wert

        if (type.IsCellValue() && columname != null && rowkey != null) {
            return SetCellValue(tablename, columname, (long)rowkey, value);
        }

        #endregion

        #region Befehle

        if (type.IsCommand()) {
            switch (type) {
                //case DatabaseDataType.Comand_AddColumn:
                //case DatabaseDataType.Comand_AddColumnByKey:
                //    return AddColumnToMain(tablename, ColumnItem.TmpNewDummy, (long)columnkey);

                case DatabaseDataType.Comand_AddColumnByName:
                    return AddColumnToMain(tablename, value);

                case DatabaseDataType.Comand_RemoveColumn:
                    if (columname == null) { return "Spalte nicht definiert!"; }
                    return RemoveColumn(tablename, columname);

                case DatabaseDataType.Comand_RemoveRow:
                    return RemoveRow(tablename, LongParse(value));

                case DatabaseDataType.Comand_AddRow:
                    return AddRow(tablename, LongParse(value));

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
    public List<string> Tables() {
        var l = AllTables();

        _ = l.Remove(SysStyle);
        _ = l.Remove(SysUndo);
        return l;
    }

    public abstract string VarChar(int lenght);

    //    if (allcols.Count > 0) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Zusätzliche Spalten dem Server vorhanden: " + allcols.JoinWith(", "));
    //    }
    internal static string MakeValidColumnName(string columnname) => columnname.ToUpper().Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);

    //    com = com + " FROM " + tablename.ToUpper();
    /// <summary>
    ///
    /// </summary>
    /// <param name="db"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns>Gibt NULL zurück, wenn die Daten nicht geladen werden konnten</returns>
    internal List<(string tablename, string comand, string columname, string rowkey, DateTime timecode)>? GetLastChanges(List<DatabaseSQLLite> db, DateTime fromDate, DateTime toDate) {
        if (!OpenConnection()) { return null; }

        try {
            lock (_getChanges) {
                //using var q = _connection.CreateCommand();

                var commandText = @"select TABLENAME, COMAND, COLUMNNAME, ROWKEY, TIMECODEUTC from " + SysUndo + " ";

                // nur bestimmte Tabellen
                commandText += "WHERE (";
                foreach (var thisdb in db) {
                    commandText += "TABLENAME=" + Dbval(thisdb.TableName.ToUpper()) + " OR ";
                }
                commandText = commandText.TrimEnd(" OR ") + ") AND ";

                // Zeit einbeziehen
                commandText += "(TIMECODEUTC BETWEEN " + Dbval(fromDate)
                                 + " AND " + Dbval(toDate) + ")";

                // Sortierung nach Tabellen
                commandText += " ORDER BY TIMECODEUTC ASC";

                var fb = new List<(string tablename, string comand, string columnname, string rowid, DateTime timecode)>();

                var dt = Fill_Table(commandText);

                if (dt == null) {
                    _ = CloseConnection();
                    Develop.CheckStackForOverflow();
                    return GetLastChanges(db, fromDate, toDate);
                }

                foreach (var thisRow in dt.Rows) {
                    var reader = (DataRow)thisRow;
                    fb.Add((reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), DateTimeParse(reader[4].ToString())));
                }

                //CloseConnection();
                return fb;
            }
        } catch {
            if (Connection != null && Connection.State != ConnectionState.Open) {
                _ = CloseConnection();
                Develop.CheckStackForOverflow();
                return GetLastChanges(db, fromDate, toDate);
            }

            return null;
        }
    }

    internal SqlBackAbstract? HandleMe(ConnectionInfo ci) {
        if (ci is null) { return null; }

        //var s = connectionID.SplitBy("|");
        //if (s.Count() != 2) { return null; }

        foreach (var thisK in ConnectedSqlBack) {
            if (thisK.ConnectionOk && thisK.ConnectionString == ci.DatabaseID) {
                return thisK.OtherTable(ci.TableName);
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
    internal void LoadColumns(string tablename, List<ColumnItem>? columns) {
        try {
            if (columns == null || columns.Count == 0) { return; }

            var columnsToLoad = new List<ColumnItem>();

            foreach (var thisc in columns) {
                if (thisc != null && thisc.IsInCache == null) {
                    _ = columnsToLoad.AddIfNotExists(thisc);
                }
            }

            if (columnsToLoad.Count == 0) { return; }

            var db = columnsToLoad[0].Database;
            if (db == null || db.IsDisposed) { return; }

            var wh = string.Empty;

            var com = "SELECT RK, ";

            foreach (var thiscolumn in columnsToLoad) {
                com = com + thiscolumn.Name.ToUpper() + ", ";

                wh = wh + thiscolumn.Name.ToUpper() + " IS NOT NULL OR ";
            }
            com = com.TrimEnd(", ");
            com = com + " FROM " + tablename.ToUpper();

            com = com + " WHERE (" + wh.TrimEnd(" OR ") + ")";

            var dt = Fill_Table(com);

            if (dt == null) {
                Develop.CheckStackForOverflow();
                LoadColumns(tablename, columns);
                return;
            }

            //using var reader = command.ExecuteReader();
            foreach (var thisRow in dt.Rows) {
                var reader = (DataRow)thisRow;

                #region Zeile ermitteln, in die der Wert geschrieben werden soll

                var rk = LongParse(reader[0].ToString());
                var row = db.Row.SearchByKey(rk);

                if (row == null) {
                    _ = db.Row.SetValueInternal(DatabaseDataType.Comand_AddRow, rk, true);
                    row = db.Row.SearchByKey(rk);
                }

                #endregion

                if (row != null) {
                    for (var z = 1; z < dt.Columns.Count; z++) {
                        _ = db.Cell.SetValueInternal(columnsToLoad[z - 1].Name, row.Key, reader[z].ToString(), true);
                    }
                }
            }

            foreach (var thiscolumn in columnsToLoad) {
                //var val = GetStyleData(tablename, DatabaseDataType.ColumnTimeCode.ToString(false), thiscolumn.Name);

                thiscolumn.IsInCache = DateTime.UtcNow;

                //if (string.IsNullOrEmpty(val)) {
                //    Develop.DebugPrint(FehlerArt.Warnung, "Spalte ohne Zeitstempel, repariert! + " + com);
                //    thiscolumn.TimeCode = DateTime.UtcNow.ToString(Constants.Format_Date);
                //    //val =
                //    //SetStyleData(tablename, DatabaseDataType.ColumnTimeCode, thiscolumn.Name, val);
                //}
            }

            //Develop.DebugPrint(FehlerArt.Info, "Datenbank Ladezeit: " + row.Database.TableName + " - " + DateTime.Now.Subtract(ti).TotalSeconds + " Sekunden");

            _ = CloseConnection();
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            LoadColumns(tablename, columns);
        }
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - einschließlich der Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    protected abstract List<string> AllTables();

    protected abstract string CreateTable(string tablename);

    //    try {
    //        using var q = _connection.CreateCommand();
    protected abstract string CreateTable(string tablename, List<string> keycolumns);

    //    com = com.TrimEnd(", ");
    //internal string? GetLastColumnName(string tablename, long key) {
    //    if (!OpenConnection()) { return null; }
    protected string ExecuteCommand(string commandtext) {
        if (!OpenConnection() || Connection == null) { return "Verbindung konnte nicht geöffnet werden"; }

        using var command = Connection.CreateCommand();
        command.CommandText = commandtext;

        return ExecuteCommand(command);
    }

    //        com = com + thiscolumn.Name.ToUpper() + ", ";
    //    }
    private void AddColumn(string tablename, string column, bool nullable) => AddColumn(tablename, column, VarChar255, nullable);

    private void AddColumn(string tablename, string column, string type, bool nullable) {
        if (string.IsNullOrEmpty(column)) {
            Develop.DebugPrint(FehlerArt.Warnung, "Spalte ohne Namen!");
            return;
        }

        var n = " NOT NULL";
        if (nullable) { n = string.Empty; }

        _ = ExecuteCommand("alter table " + tablename.ToUpper() + " add " + column + " " + type + " default ''" + n);
    }

    private string AddRow(string tablename, long key) => ExecuteCommand("INSERT INTO " + tablename.ToUpper() + " (RK) VALUES (" + Dbval(key) + ")");

    private string Dbval(long original) => Dbval(original.ToString());

    private string Dbval(DateTime date) =>
        "to_timestamp(" +
        Dbval(date.ToString(Constants.Format_Date9)) + ", " +
        Dbval(Constants.Format_Date9.ToUpper().Replace(":MM:", ":MI:").Replace("HH:", "HH24:").Replace(".FFF", ".FF3")) +
        ")";

    private string Dbval(string? original) {
        if (original == null) { return "''"; }

        original = original.CutToUtf8Length(MaxStringLenght);
        original = original.Replace("'", "''");
        return "'" + original + "'";
    }

    private string ExecuteCommand(IDbCommand command) {
        if (!OpenConnection()) { return "Verbindung konnte nicht geöffnet werden"; }

        try {
            _ = command.ExecuteNonQuery();
            return string.Empty;
        } catch (Exception ex) {
            Develop.DebugPrint("Datenbank Befehl konnte nicht ausgeführt werden: " + command.CommandText, ex);
            return "Allgemeiner Fehler beim Ausführen, siehe Protocol";
        } finally {
            _ = CloseConnection();
        }
    }

    private string? GetCellValue(string tablename, string columnname, long rowkey) {
        try {
            if (Connection == null) { return null; }

            if (!OpenConnection()) { return null; }

            //using var q = _connection.CreateCommand();

            var comm = @"select " + columnname.ToUpper() + " from " + tablename.ToUpper() + " " +
                                   "where RK = " + Dbval(rowkey);

            ////command.AddParameterWithValue("" + DBVAL(columnName.ToUpper()) , column.Name.ToUpper());
            //command.AddParameterWithValue("" + DBVAL(row.Key)", row.Key.ToString(false));

            var dt = Fill_Table(comm);

            if (dt == null || dt.Rows.Count == 0) { return string.Empty; }
            if (dt.Rows.Count > 1) {
                // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
                Develop.DebugPrint(columnname + " doppelt in " + tablename.ToUpper() + " vorhanden!");
            }

            return dt.Rows[0][0].ToString();
            //using var reader = q.ExecuteReader();
            //if (reader.Read()) {
            //    // you may want to check if value is NULL: reader.IsDBNull(0)
            //    var value = reader[0].ToString(false);

            //    if (reader.Read()) {
            //        // Doppelter Wert?!?Ersten Wert zurückgeben, um unendliche erweiterungen zu erhindern
            //        Develop.DebugPrint(columnname + " doppelt in " + tablename.ToUpper() + " vorhanden!");
            //    }

            //    CloseConnection();
            //return value;
            //}

            //CloseConnection();    // Nix vorhanden!
            //return null;
        } catch {
            _ = CloseConnection();
            Develop.CheckStackForOverflow();
            return GetCellValue(tablename, columnname, rowkey);
        }
    }

    private string RemoveColumn(string tablename, string column) {
        var b = ExecuteCommand("alter table " + tablename.ToUpper() + " drop column " + column.ToUpper());
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }

        b = ExecuteCommand("DELETE FROM " + SysStyle + " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) + " AND COLUMNNAME = " + Dbval(column.ToUpper()));
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }
        return string.Empty;
    }

    private string RemoveRow(string tablename, long key) {
        var b = ExecuteCommand("DELETE FROM  " + tablename.ToUpper() + " WHERE RK = " + Dbval(key.ToString()));
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }
        return string.Empty;
    }

    /// <summary>
    /// Gibt TRUE zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde, der Wert aktuell ist oder der Wert unwichtig ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="rowkey"></param>
    /// <param name="newValue"></param>
    /// <param name="columnname"></param>
    /// <returns></returns>
    private string SetCellValue(string tablename, string columnname, long rowkey, string newValue) {
        var isVal = GetCellValue(tablename, columnname, rowkey);

        string cmdString;

        if (isVal is null) {
            cmdString = "INSERT INTO " + tablename.ToUpper() + " (RK, " + columnname.ToUpper() + " ) VALUES (" + Dbval(rowkey) + ", " + Dbval(newValue) + " )";
        } else if (isVal != newValue) {
            cmdString = "UPDATE " + tablename.ToUpper() + " SET " + columnname.ToUpper() + " = " + Dbval(newValue) + " WHERE RK = " + Dbval(rowkey);
        } else {
            return string.Empty;
        }

        return ExecuteCommand(cmdString);
    }

    /// <summary>
    /// Gibt empty zurück, wenn alles ok ist.
    /// Entweder der Wert gesetzt wurde oder der Wert aktuell ist.
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="columnName"></param>
    /// <param name="newValue"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    private string SetStyleData(string tablename, string type, string columnName, string newValue, int part) {
        var cmdString = "INSERT INTO " + SysStyle + " (TABLENAME, TYPE, COLUMNNAME, VALUE, PART)  VALUES (" + Dbval(tablename.ToUpper()) + ", " + Dbval(type) + ", " + Dbval(columnName.ToUpper()) + ", " + Dbval(newValue) + ", " + Dbval(part.ToString(Constants.Format_Integer3)) + ")";
        if (!OpenConnection()) { return "Verbindung konnt nicht geöffnet werden"; }
        return ExecuteCommand(cmdString);
    }

    private void SysUndoAufräumen() {
        var c = "DELETE FROM " + SysUndo +
                " WHERE RK NOT IN " +
                "(SELECT RK FROM (SELECT RK, ROW_NUMBER() OVER (PARTITION BY TABLENAME ORDER BY TIMECODEUTC DESC) as row_num FROM " + SysUndo + ") WHERE row_num <= 500)" +
                "AND TIMECODEUTC<" + Dbval(DateTime.UtcNow.AddDays(-7));

        _ = ExecuteCommand(c);
    }

    #endregion
}