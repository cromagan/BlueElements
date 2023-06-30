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
using System.IO;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Constants;
using static BlueBasics.Extensions;
using static BlueBasics.IO;

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

    public DbConnection? Connection;
    private static bool _didBackup;
    private readonly object _fill = new();
    private readonly object _getChanges = new();

    private readonly object _openclose = new();

    #endregion

    #region Constructors

    public SqlBackAbstract() => ConnectedSqlBack.Add(this);

    #endregion

    #region Properties

    public static DateTime LastLoadUtc { get; private set; } = DateTime.UtcNow;

    public abstract string ColumnPropertyPrimary { get; }
    public abstract string ColumnTypeDate { get; }
    public string ColumnTypeVarChar15 => VarChar(15);
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

    public static bool IsValidTableName(string tablename, bool allowSystemnames) {
        if (string.IsNullOrEmpty(tablename)) { return false; }

        var t = tablename.ToUpper();

        if (!allowSystemnames) {
            if (t.StartsWith("SYS_")) { return false; }
            if (t.StartsWith("BAK_")) { return false; }
        }

        if (!t.ContainsOnlyChars(Constants.Char_AZ + Constants.Char_Numerals + "_")) { return false; }

        if (tablename == "ALL_TAB_COLS") { return false; } // system-name

        // eigentlich 128, aber minus BAK_ und _2023_03_28
        if (t.Length > 100) { return false; }

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
    /// <param name="allowSystemTableNames"></param>
    public string AddColumnToMain(string tablename, string columnName, bool allowSystemTableNames) {
        columnName = columnName.ToUpper();

        var colMain = GetColumnNames(tablename.ToUpper());
        if (colMain == null) { Develop.DebugPrint(FehlerArt.Fehler, "Spaltenfehler"); return "Spalte nicht gefunden"; }
        if (!colMain.Contains(columnName)) { AddColumn(tablename.ToUpper(), columnName, ColumnTypeVarChar4000, true, allowSystemTableNames); }

        return SetStyleData(tablename, DatabaseDataType.ColumnName, columnName.ToUpper(), columnName.ToUpper());
    }

    public string AddUndo(string tablename, DatabaseDataType comand, string? columname, long? rowKey, string previousValue, string changedTo, string comment) {
        if (!OpenConnection()) { return "Es konnte keine Verbindung zur Datenbank aufgebaut werden"; }

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
             Dbval(Generic.UserName) + "," +
             //DBVAL(dt.ToString(Constants.Format_Date)) + "," +
             Dbval(DateTime.UtcNow) + "," +
             Dbval(comment) + ")";

        return ExecuteCommand(cmdString, false);
    }

    /// <summary>
    /// Gibt empty zurück, wenn das Backup erstellt wurde ODER die Datei bereits existiert
    /// </summary>
    /// <param name="pathadditionalcsv"></param>
    /// <returns></returns>
    public string BackupSysStyle(string pathadditionalcsv) {
        if (string.IsNullOrEmpty(pathadditionalcsv)) { return "Kein Backup-Verzeichniss angebeben."; }

        if (!Directory.Exists(pathadditionalcsv)) { return "Backup-Verzeichniss existiert nicht."; }

        var d = DateTime.UtcNow.ToString(Constants.Format_Date10);

        var file = pathadditionalcsv.CheckPath() + "SYS_STYLE_" + d + ".csv";
        if (IO.FileExists(file)) { return string.Empty; }

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

        var s = (DataFormat)IntParse(GetStyleData(tablename, DatabaseDataType.ColumnFormat.ToString(), column));

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
                    tbl.Load(command.ExecuteReader());
                    _ = CloseConnection();
                    LastLoadUtc = DateTime.UtcNow;
                    return tbl;
                }
            } catch { }

            Generic.Pause(0.1, false);
            _ = CloseConnection();

            Generic.Pause(1, false);
            Develop.CheckStackForOverflow();
            return Fill_Table(commandtext);
        }
    }

    /// <summary>
    /// Gibt die Spaltennamen in Grosschreibung zurück
    /// </summary>
    /// <returns></returns>
    public abstract List<string> GetColumnNames(string tablename);

    public string GetStyleData(string tablename, string type, string columnName) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        if (string.IsNullOrEmpty(columnName)) { columnName = DatabaseProperty; }

        if (columnName != DatabaseProperty && !ColumnItem.IsValidColumnName(columnName)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Spaltenname ungültig: " + columnName);
            throw new Exception();
        }

        var cmd = @"select VALUE, PART from " + SysStyle + " " +
                        "where TABLENAME = " + Dbval(tablename.ToUpper()) + " " +
                        "and TYPE = " + Dbval(type) + " " +
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
    /// <param name="tablename"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Dictionary<string, string>? GetStyleDataAll(string tablename, string columnName) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        var l = new Dictionary<string, string>();
        var commandText = @"select TYPE, PART, VALUE from " + SysStyle + " " +
                         "where TABLENAME = " + Dbval(tablename.ToUpper()) + " " +
                         "and COLUMNNAME = " + Dbval(columnName.ToUpper()) + " " +
                         "ORDER BY PART ASC";

        var dt = Fill_Table(commandText);

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;
            var key = reader[0].ToString();
            var value = reader[2].ToString();

            if (reader[1].ToString() != "001") {
                l[key] += value;
            } else {
                l.Add(key, value);
            }
        }

        return l;
    }

    public void LoadAllRowKeys(string tablename, RowCollection row) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        var com = "SELECT RK ";
        com = com + " FROM " + tablename.ToUpper();

        var dt = Fill_Table(com);
        _ = row.Clear("Row Keys werden neu geladen");

        foreach (var thisRow in dt.Rows) {
            var rk = LongParse(((DataRow)thisRow)[0].ToString());
            _ = row.SetValueInternal(DatabaseDataType.Comand_AddRow, rk, null, true);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tablename"></param>
    /// <param name="row"></param>
    /// <param name="refreshAlways">Bei TRUE wird die gesamte Zeile aktualistert, weil evtl. eine Änderung aufgetreten ist. Bei FLASE werden nur die fehlenden Daten der noch nicht geladenen Spalten nachgeladen.</param>
    /// <param name="sortedRows"></param>
    /// <param name="trycount"></param>
    public string LoadRow(string tablename, List<RowItem> row, bool refreshAlways, List<RowItem>? sortedRows) {
        if (!IsValidTableName(tablename, true)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellename ungültig: " + tablename);
            throw new Exception();
        }

        if (row.Count == 0 || row[0] is null) { return string.Empty; }

        var db = row[0]?.Database;
        if (db == null || db.IsDisposed) { return "Datenbank verworfen"; }

        var com = new StringBuilder();
        com.Append("SELECT RK, ");

        #region Spalten wählen

        var count = 0;

        foreach (var thiscolumn in db.Column) {
            if (refreshAlways || thiscolumn.IsInCache == null) {
                if (count > 0) { com.Append(", "); }

                count++;
                com.Append(thiscolumn.Name.ToUpper());
            }
        }

        #endregion

        _ = db.Row.FillUp100(row, sortedRows);

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
                com.Append("RK = " + Dbval(thisr.Key));
            }
        }

        #endregion

        var dt = Fill_Table(com.ToString());

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;
            var rk = LongParse(reader[0].ToString());
            var r = db.Row.SearchByKey(rk) ?? db.Row.GenerateAndAdd(rk, string.Empty, false, false, "Load row");

            if (r != null && !r.IsDisposed) {
                for (var z = 1; z < dt.Columns.Count; z++) {
                    var cx = db.Column.Exists(dt.Columns[z].ColumnName);
                    if (cx != null) {
                        _ = db.Cell.SetValueInternal(cx, r, reader[z].ToString(), true);
                    }
                }

                r.IsInCache = DateTime.UtcNow;
            }
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

    public abstract SqlBackAbstract OtherTable(string tablename);

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

        var test = GetStyleData(tablename, DatabaseDataType.ColumnName.ToString(), newname);

        if (test != newname) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fataler Umbenennungs-Fehler!");
        }
    }

    //    row.Clear();
    //    var ti = DateTime.Now;
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
                _ = CreateTable(tablename.ToUpper(), new List<string> { "RK" }, false);
                ChangeDataType(tablename.ToUpper(), "RK", 15, false);
            }
        }

        #endregion

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
        if (!colUndo.Contains("COLUMNKEY")) { AddColumn(SysUndo, "COLUMNKEY", ColumnTypeVarChar15, true, true); }
        if (!colUndo.Contains("ROWKEY")) { AddColumn(SysUndo, "ROWKEY", ColumnTypeVarChar15, true, true); }
        if (!colUndo.Contains("PREVIOUSVALUE")) { AddColumn(SysUndo, "PREVIOUSVALUE", ColumnTypeVarChar4000, true, true); }
        if (!colUndo.Contains("CHANGEDTO")) { AddColumn(SysUndo, "CHANGEDTO", ColumnTypeVarChar4000, true, true); }
        if (!colUndo.Contains("USERNAME")) { AddColumn(SysUndo, "USERNAME", false, true); }
        if (!colUndo.Contains("TIMECODEUTC")) { AddColumn(SysUndo, "TIMECODEUTC", ColumnTypeDate, false, true); }
        if (!colUndo.Contains("CMT")) { AddColumn(SysUndo, "CMT", ColumnTypeVarChar255, true, true); }
        if (!colUndo.Contains("COLUMNNAME")) { AddColumn(SysUndo, "COLUMNNAME", ColumnTypeVarChar255, true, true); }

        #endregion

        SysUndoAufräumen();

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
        //if (type == DatabaseDataType.AddColumnKeyInfo) { return true; } // enthält zwar den Key, aber Wertlos, wenn der Spaltenname noch nicht bekannt ist...
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

        var isVal = GetStyleData(tablename, ty, columnName);
        if (isVal == null) { return "Kein Wert angekommen!"; }
        if (isVal == newValue) { return string.Empty; }

        var com = "BEGIN DELETE FROM " + SysStyle +
                  " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) +
                  " AND COLUMNNAME = " + Dbval(columnName.ToUpper()) +
                  " AND TYPE = " + Dbval(type.ToString()) + "; ";

        var maxPartStringLenght = Math.Min(MaxStringLenght, 500);

        do {
            c++;
            var tmp = utf8.CutToUtf8Length(maxPartStringLenght);

            var cmdString = "INSERT INTO " + SysStyle + " (TABLENAME, TYPE, COLUMNNAME, VALUE, PART)  VALUES (" + Dbval(tablename.ToUpper()) + ", " + Dbval(ty) + ", " + Dbval(columnName.ToUpper()) + ", " + Dbval(tmp) + ", " + Dbval(c.ToString(Constants.Format_Integer3)) + "); ";

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
    /// <param name="tablename"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="columname"></param>
    /// <param name="rowkey"></param>
    /// <returns></returns>
    public string SetValueInternal(string tablename, DatabaseDataType type, string value, string? columname, long? rowkey) {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return "Tabellenname ungültig: " + tablename;
        }

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
                    return AddColumnToMain(tablename, value, false);

                case DatabaseDataType.Comand_RemoveColumn:
                    if (columname == null) { return "Spalte nicht definiert!"; }
                    return RemoveColumn(tablename, columname, false);

                case DatabaseDataType.Comand_RemoveRow:
                    return RemoveRow(tablename, LongParse(value), false);

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
    internal static string MakeValidColumnName(string columnname) => columnname.ToUpper().Replace(" ", "_").ReduceToChars(Constants.AllowedCharsVariableName);

    //    com = com + " FROM " + tablename.ToUpper();
    /// <summary>
    ///
    /// </summary>
    /// <param name="db"></param>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns>Gibt NULL zurück, wenn die Daten nicht geladen werden konnten</returns>
    internal List<(string tablename, string comand, string columnname, string rowkey, string newValue, DateTime timecode)>? GetLastChanges(List<DatabaseSqlLite> db, DateTime fromDate, DateTime toDate) {
        lock (_getChanges) {
            var commandText = @"select TABLENAME, COMAND, COLUMNNAME, ROWKEY, CHANGEDTO, TIMECODEUTC from " + SysUndo + " ";

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

            var fb = new List<(string tablename, string comand, string columnname, string rowkey, string newValue, DateTime timecode)>();

            var dt = Fill_Table(commandText);

            foreach (var thisRow in dt.Rows) {
                var reader = (DataRow)thisRow;
                fb.Add((reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), DateTimeParse(reader[5].ToString())));
            }

            return fb;
        }
    }

    internal SqlBackAbstract? HandleMe(ConnectionInfo ci) {
        if (ci is null) { return null; }

        //var s = connectionID.SplitBy("|");
        //if (s.Count() != 2) { return null; }

        foreach (var thisK in ConnectedSqlBack) {
            if (thisK.Connection != null && thisK.ConnectionString == ci.DatabaseID) {
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
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            throw new Exception();
        }

        if (columns == null || columns.Count == 0) { return; }

        #region tatsächlich benötite Spalten ermitteln (columnsToLoad)

        var columnsToLoad = new List<ColumnItem>();

        foreach (var thisc in columns) {
            if (thisc != null && thisc.IsInCache == null) {
                _ = columnsToLoad.AddIfNotExists(thisc);
            }
        }

        if (columnsToLoad.Count == 0) { return; }

        #endregion

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

        foreach (var thisRow in dt.Rows) {
            var reader = (DataRow)thisRow;

            #region Zeile ermitteln, in die der Wert geschrieben werden soll

            var rk = LongParse(reader[0].ToString());
            var row = db.Row.SearchByKey(rk);

            if (row == null || row.IsDisposed) {
                _ = db.Row.SetValueInternal(DatabaseDataType.Comand_AddRow, rk, null, true);
                row = db.Row.SearchByKey(rk);
            }

            #endregion

            if (row != null && !row.IsDisposed) {
                for (var z = 1; z < dt.Columns.Count; z++) {
                    _ = db.Cell.SetValueInternal(columnsToLoad[z - 1], row, reader[z].ToString(), true);
                }
            }
        }

        foreach (var thiscolumn in columnsToLoad) {
            thiscolumn.IsInCache = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gibt alle verfügbaren Tabellen - einschließlich der Systemtabellen - zurück
    /// </summary>
    /// <returns></returns>
    protected abstract List<string>? AllTables();

    protected void CompareBackUp(DateTime compareDate) {
        var tbl = Tables();
        if (tbl == null) { return; }

        tbl.Add(SysStyle);

        var alltb = AllTables();
        if (alltb == null) { return; }

        var l = new List<string>();

        var d = compareDate.ToString(Constants.Format_Date10);

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

        l.WriteAllText(TempFile("", "","txt"), Win1252, true);
    }

    protected abstract string CreateTable(string tablename, bool allowSystemTableNames);

    //    try {
    //        using var q = _connection.CreateCommand();
    protected abstract string CreateTable(string tablename, List<string> keycolumns, bool allowSystemTableNames);

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

            var d = DateTime.UtcNow.ToString(Constants.Format_Date10);

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

    protected string ExecuteCommand(string commandtext, bool abort) {
        if (Connection == null || !OpenConnection()) { return "Es konnte keine Verbindung zur Datenbank aufgebaut werden"; }
        using var command = Connection.CreateCommand();
        command.CommandText = commandtext;
        return ExecuteCommand(command, abort);
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

    private string AddRow(string tablename, long key) => ExecuteCommand("INSERT INTO " + tablename.ToUpper() + " (RK) VALUES (" + Dbval(key) + ")", true);

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

    private string ExecuteCommand(IDbCommand command, bool abort) {
        LastLoadUtc = DateTime.UtcNow;

        if (Connection == null || !OpenConnection()) { return "Verbindung konnte nicht geöffnet werden"; }

        try {
            _ = command.ExecuteNonQuery();
            return string.Empty;
        } catch (Exception ex) {
            if (abort) {
                Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Befehl konnte nicht ausgeführt werden: " + command.CommandText, ex);
            }

            Develop.DebugPrint("Datenbank Befehl konnte nicht ausgeführt werden: " + command.CommandText, ex);
            return "Allgemeiner Fehler beim Ausführen, siehe Protokoll";
        } finally {
            _ = CloseConnection();
        }
    }

    //private (string? value, string error) GetCellValue(string tablename, string columnname, long rowkey) {
    //    if (!IsValidTableName(tablename, false)) {
    //            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
    //            return (null, "Tabellenname ungültig: " + tablename);
    //        }

    //        var comm = @"select " + columnname.ToUpper() + " from " + tablename.ToUpper() + " where RK = " + Dbval(rowkey);

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

        var b = ExecuteCommand("ALTER TABLE " + tablename.ToUpper() + " drop column " + column.ToUpper(), true);
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }

        b = ExecuteCommand("DELETE FROM " + SysStyle + " WHERE TABLENAME = " + Dbval(tablename.ToUpper()) + " AND COLUMNNAME = " + Dbval(column.ToUpper()), true);
        if (!string.IsNullOrEmpty(b)) { return "Löschen fehgeschlagen: " + b; }
        return string.Empty;
    }

    private string RemoveRow(string tablename, long key, bool allowSystemTableNames) {
        if (!IsValidTableName(tablename, allowSystemTableNames)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
            return "Tabellenname ungültig: " + tablename;
        }

        var b = ExecuteCommand("DELETE FROM " + tablename.ToUpper() + " WHERE RK = " + Dbval(key.ToString()), true);
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
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);

            return "Tabellenname ungültig: " + tablename;
        }

        //(string? value, string error) = GetCellValue(tablename, columnname, rowkey);

        //if (!string.IsNullOrEmpty(error)) { return error; }

        string cmdString;

        //if (value is null) {
        //    cmdString = "INSERT INTO " + tablename.ToUpper() + " (RK, " + columnname.ToUpper() + " ) VALUES (" + Dbval(rowkey) + ", " + Dbval(newValue) + " )";
        //    Develop.DebugPrint(FehlerArt.Warnung, "Insert-Befehl: " + cmdString);
        //} else if (value != newValue) {
        cmdString = "UPDATE " + tablename.ToUpper() + " SET " + columnname.ToUpper() + " = " + Dbval(newValue) + " WHERE RK = " + Dbval(rowkey);
        //} else {
        //    return string.Empty;
        //}

        return ExecuteCommand(cmdString, false);
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