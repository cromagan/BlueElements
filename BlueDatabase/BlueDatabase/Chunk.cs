// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Chunk : IHasKeyName {

    #region Fields

    public readonly string MainFileName = string.Empty;
    private string _fileinfo = string.Empty;

    private string _keyname = string.Empty;
    private DateTime _lastcheck = DateTime.MinValue;

    private int _minBytes = 0;

    #endregion

    #region Constructors

    public Chunk(string mainFileName, string chunkId) {
        MainFileName = mainFileName;
        KeyName = chunkId;
    }

    #endregion

    #region Properties

    public List<byte> Bytes { get; private set; } = [];

    public string ChunkFileName {
        get {
            if (IsMain) { return MainFileName; }

            var folder = MainFileName.FilePath();
            var databasename = MainFileName.FileNameWithoutSuffix();

            return $"{folder}{databasename}\\{KeyName}.bdbc";
        }
    }

    public long DataLenght => Bytes?.Count ?? 0;

    public bool IsMain => string.Equals(KeyName, DatabaseChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);

    public string KeyName {
        get => _keyname;
        private set => _keyname = value.ToLower();
    }

    public string LastEditApp { get; private set; } = string.Empty;
    public string LastEditID { get; private set; } = string.Empty;
    public string LastEditMachineName { get; private set; } = string.Empty;

    public DateTime LastEditTimeUtc { get; private set; } = DateTime.MinValue;

    public string LastEditUser { get; private set; } = string.Empty;

    public bool LoadFailed { get; set; } = false;

    public bool SaveRequired { get; set; } = false;

    #endregion

    #region Methods

    public static void SaveToByteList(List<byte> bytes, DatabaseDataType databaseDataType, string content) {
        var b = content.UTF8_ToByte();
        bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        bytes.Add((byte)databaseDataType);
        SaveToByteList(bytes, b.Length, 3);
        bytes.AddRange(b);
    }

    public static void SaveToByteList(List<byte> bytes, long numberToAdd, int byteCount) {
        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            // ReSharper disable once PossibleLossOfFraction
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            bytes.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);
    }

    /// <summary>
    /// Initialisiert die Byteliste
    /// </summary>
    public void InitByteList() {
        LoadFailed = false;

        _lastcheck = DateTime.UtcNow;

        Bytes = [];
    }

    public void LoadBytesFromDisk() {
        var c = ChunkFileName;
        _minBytes = 0;

        if (!FileExists(c)) {
            InitByteList();
            return;
        }

        _lastcheck = DateTime.UtcNow;

        byte[] bytes;
        (bytes, _fileinfo, LoadFailed) = IO.LoadBytesFromDisk(c, true);

        if (LoadFailed) { return; }

        if (RemoveHeaderDataTypes(bytes) is { } b) {
            _minBytes = (int)(b.Count * 0.1);
        }

        Bytes.Clear();
        Bytes = [.. bytes];

        ParseLockData();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="important">Steuert, ob es dringen nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns></returns>
    public bool NeedsReload(bool important) {
        if (LoadFailed) { return true; }
        if (string.IsNullOrEmpty(MainFileName)) { return false; } // Temporäre Datenbanken

        // Prüfe, ob die Datei überhaupt existiert
        if (!FileExists(ChunkFileName)) {
            return Bytes.Count > 0; // Nur neu laden, wenn wir Daten haben, die "verschwunden" sind
        }

        if (DateTime.UtcNow.Subtract(_lastcheck).TotalMinutes > 3 || important) {
            var nf = GetFileInfo(ChunkFileName, false);
            return nf != _fileinfo;
        }

        return false;
    }

    public string Save(string filename) {
        if (LoadFailed) { return "Chunk wurde nicht korrekt geladen"; }
        if (Bytes.Count < _minBytes) { return "Zu große Änderungen, sicherheitshalber geblockt"; }
        if (_lastcheck.Year < 2000) { return "Chunk noch nicht geladen"; }

        if (Develop.AllReadOnly) { return string.Empty; }

        try {
            Develop.SetUserDidSomething();

            // Extrahiere nur die tatsächlichen Datensätze, keine Header-Daten
            var contentBytes = RemoveHeaderDataTypes(Bytes.ToArray());
            if (contentBytes == null || contentBytes.Count < _minBytes) { return "Zu große Änderungen, sicherheitshalber geblockt"; }

            // Neuen Header erstellen
            var head = GetHeadAndSetEditor();
            if (head == null || head.Count < 100) { return "Chunk-Kopf konnte nicht erstellt werden"; }

            // Header und Datensätze zusammenführen und komprimieren
            var datacompressed = head.Concat(contentBytes).ToArray().ZipIt();
            if (datacompressed == null || datacompressed.Length < 100) { return "Komprimierug der Daten fehlgeschlagen"; }

            Develop.SetUserDidSomething();

            using FileStream x = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            x.Write(datacompressed, 0, datacompressed.Length);
            x.Flush();
            x.Close();

            _minBytes = (int)(contentBytes.Count * 0.1);

            Develop.SetUserDidSomething();
        } catch {
            DeleteFile(filename, false);
            return "Allgemeiner Fehler";
        }

        return string.Empty;
    }

    public void SaveToByteList(ColumnItem column, RowItem row) {
        if (LoadFailed) { return; }
        if (column.Database is not { IsDisposed: false } db) { return; }

        var cellContent = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        Bytes.Add((byte)Routinen.CellFormatUTF8_V403);

        var columnKeyByte = column.KeyName.UTF8_ToByte();
        SaveToByteList(Bytes, columnKeyByte.Length, 1);
        Bytes.AddRange(columnKeyByte);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(Bytes, rowKeyByte.Length, 1);
        Bytes.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(Bytes, cellContentByte.Length, 2);
        Bytes.AddRange(cellContentByte);
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content, string columnname) {
        if (LoadFailed) { return; }
        Bytes.Add((byte)Routinen.ColumnUTF8_V401);
        Bytes.Add((byte)databaseDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(Bytes, n.Length, 1);
        Bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(Bytes, b.Length, 3);
        Bytes.AddRange(b);
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content) {
        if (LoadFailed) { return; }
        var b = content.UTF8_ToByte();
        Bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        Bytes.Add((byte)databaseDataType);
        SaveToByteList(Bytes, b.Length, 3);
        Bytes.AddRange(b);
    }

    /// <summary>
    /// Alle Spaltendaten außer Systeminfo
    /// </summary>
    /// <param name="c"></param>
    public void SaveToByteList(ColumnItem c) {
        if (LoadFailed) { return; }

        var name = c.KeyName;

        SaveToByteList(DatabaseDataType.ColumnName, c.KeyName, name);
        SaveToByteList(DatabaseDataType.IsFirst, c.IsFirst.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.IsKeyColumn, c.IsKeyColumn.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ColumnCaption, c.Caption, name);
        //SaveToByteList(DatabaseDataType.ColumnFunction, ((int)c.Function).ToString(), name);
        SaveToByteList(DatabaseDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(DatabaseDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(DatabaseDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(DatabaseDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(DatabaseDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        //SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.RoundAfterEdit, c.RoundAfterEdit.ToString(), name);
        SaveToByteList(DatabaseDataType.MaxCellLenght, c.MaxCellLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        SaveToByteList(DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.EditableWithTextInput, c.EditableWithTextInput.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.Relationship_to_First, c.Relationship_to_First.ToPlusMinus(), name);
        //SaveToByteList(DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.TextFormatingAllowed, c.TextFormatingAllowed.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleLeft, ((int)c.LineStyleLeft).ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleRight, ((int)c.LineStyleRight).ToString(), name);
        SaveToByteList(DatabaseDataType.RelationType, ((int)c.RelationType).ToString(), name);
        SaveToByteList(DatabaseDataType.Value_for_Chunk, ((int)c.Value_for_Chunk).ToString(), name);
        SaveToByteList(DatabaseDataType.EditableWithDropdown, c.EditableWithDropdown.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        //SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ColumnQuickInfo, c.ColumnQuickInfo, name);
        SaveToByteList(DatabaseDataType.ColumnAdminInfo, c.AdminInfo, name);
        //SaveToByteList(DatabaseDataType.ColumnSystemInfo, c.SystemInfo, name);
        //SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(DatabaseDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.ColumnTags, c.ColumnTags.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.LinkedDatabaseTableName, c.LinkedDatabaseTableName, name);
        //SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        //SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnNameOfLinkedDatabase, c.ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, column.TimeCode, key);
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Initialladung ausgeführt wurde
    /// </summary>
    /// <returns>True, wenn die Initialisierung erfolgreich abgeschlossen wurde, sonst False</returns>
    public bool WaitInitialDone() {
        var t = Stopwatch.StartNew();
        var lastMessageTime = 0L;

        while (_lastcheck.Year < 2000) {
            Thread.Sleep(20); // Längere Pause zur Reduzierung der CPU-Last

            if (t.ElapsedMilliseconds > 120 * 1000) {
                Develop.MonitorMessage?.Invoke("Chunk-Laden", "Puzzle", $"Abbruch, Chunk {KeyName} wurde nicht richtig initialisiert", 0);
                return false; // Explizit false zurückgeben, wenn die Initialisierung fehlschlägt
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.MonitorMessage?.Invoke("Chunk-Laden", "Puzzle", $"Warte auf Abschluss der Initialisierung des Chunks {KeyName}", 0);
            }
        }

        return true; // Explizit true zurückgeben, wenn die Initialisierung erfolgreich ist
    }

    internal bool Delete() {
        var filename = ChunkFileName;

        if (DeleteFile(filename, true)) {
            // Zuerst die Bytes leeren, um sicherzustellen, dass wir nicht versehentlich
            // anschließend wieder speichern
            Bytes = [];
            _fileinfo = string.Empty;
            return true;
        }
        return false;
    }

    internal string DoExtendedSave() {
        var filename = ChunkFileName;
        Develop.MonitorMessage?.Invoke(MainFileName.FileNameWithSuffix(), "Diskette", $"Speichere Chunk '{filename.FileNameWithoutSuffix()}'", 0);

        var backup = filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";
        var tempfile = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        var updateTime = DateTime.UtcNow;

        var f = Save(tempfile);

        if (!string.IsNullOrEmpty(f)) { return f; }

        // KRITISCHE ÄNDERUNG: FileInfo der temporären Datei VOR dem Move ermitteln
        // So wissen wir exakt, was wir schreiben und vermeiden Race Conditions
        var tempFileInfo = GetFileInfo(tempfile, true);
        if (string.IsNullOrEmpty(tempFileInfo)) {
            DeleteFile(tempfile, false);
            return "Dateiinfo konnte nicht gelesen werden";
        }

        if (FileExists(backup) && !DeleteFile(backup, false)) {
            DeleteFile(tempfile, false);
            return "Backup konnte nicht gelöscht werden";
        }

        if (FileExists(filename) && !MoveFile(filename, backup, false)) {
            DeleteFile(tempfile, false);
            return "Hauptdatei konnte nicht verschoben werden";
        }

        // --- TmpFile wird zum Haupt ---
        const int maxRetries = 5;
        const int retryDelayMs = 1000;

        for (var attempt = 1; attempt <= maxRetries; attempt++) {
            if (MoveFile(tempfile, filename, false)) {
                // Thread-sichere Aktualisierung in einer logischen Einheit
                lock (this) {
                    _lastcheck = updateTime;
                    _fileinfo = tempFileInfo;
                }

                return string.Empty;
            }

            // Paralleler Prozess hat gespeichert?
            if (FileExists(filename)) {
                DeleteFile(tempfile, false);
                LoadBytesFromDisk();
                return "Dateien wurden zwischenzeitlich verändert";
            }

            Thread.Sleep(retryDelayMs * attempt);

            if (!FileExists(tempfile)) { return "Temp-Datei Zugriffsfehler"; }
        }

        // Aufräumen falls alles fehlschlägt
        DeleteFile(tempfile, false);
        return "speichervorgang unerwartet abgebrochen";
    }

    internal string GrantWriteAccess() {
        var f = IsEditable();
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (NeedsReload(true)) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 1.5) {
            f = CanSaveFile(ChunkFileName, 5);
            if (!string.IsNullOrWhiteSpace(f)) { return f; }

            f = DoExtendedSave();

            if (!string.IsNullOrEmpty(f)) {
                LastEditTimeUtc = DateTime.MinValue;
                return $"Bearbeitung konnte nicht gesetzt werden ({f})";
            }
        }

        return string.Empty;
    }

    internal string IsEditable() {
        if (LoadFailed) { return "Chunk wurde nicht korrekt geladen"; }

        if (NeedsReload(false)) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes < 2) {
            if (LastEditUser != UserName) {
                return $"Aktueller Bearbeiter: {LastEditUser}";
            } else {
                if (LastEditApp != Develop.AppExe()) {
                    return $"Anderes Programm bearbeitet: {LastEditApp}";
                } else {
                    if (LastEditMachineName != Environment.MachineName || LastEditID != MyId) {
                        return $"Anderer Computer bearbeitet: {LastEditMachineName} - {LastEditID}";
                    }
                }
            }
        }

        return CanSaveFile(ChunkFileName, 1);
    }

    internal void SaveToByteList(RowItem thisRow) {
        if (LoadFailed) { return; }
        if (thisRow.Database is not { IsDisposed: false } db) { return; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn.SaveContent) {
                SaveToByteList(thisColumn, thisRow);
            }
        }
    }

    internal void SaveToByteListEOF() => SaveToByteList(DatabaseDataType.EOF, "END");

    private List<byte> GetHeadAndSetEditor() {
        if (LoadFailed) { return []; }

        var headBytes = new List<byte>();

        // Zuerst Werte setzen
        LastEditTimeUtc = DateTime.UtcNow;
        LastEditUser = UserName;
        LastEditApp = Develop.AppExe();
        LastEditMachineName = Environment.MachineName;
        LastEditID = MyId;

        // Dann die Werte zur ByteList hinzufügen
        SaveToByteList(headBytes, DatabaseDataType.Version, Database.DatabaseVersion);
        SaveToByteList(headBytes, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        ");

        SaveToByteList(headBytes, DatabaseDataType.LastEditTimeUTC, LastEditTimeUtc.ToString5());
        SaveToByteList(headBytes, DatabaseDataType.LastEditUser, LastEditUser);
        SaveToByteList(headBytes, DatabaseDataType.LastEditApp, LastEditApp);
        SaveToByteList(headBytes, DatabaseDataType.LastEditMachineName, LastEditMachineName);
        SaveToByteList(headBytes, DatabaseDataType.LastEditID, MyId);

        return headBytes;
    }

    private void ParseLockData() {
        var pointer = 0;
        var data = Bytes.ToArray();
        var filename = ChunkFileName;

        while (pointer < data.Length) {
            var (newPointer, type, value, _, _) = Database.Parse(data, pointer, filename);
            pointer = newPointer;

            switch (type) {
                case DatabaseDataType.LastEditTimeUTC:
                    LastEditTimeUtc = DateTimeParse(value);
                    break;

                case DatabaseDataType.LastEditUser:
                    LastEditUser = value;
                    break;

                case DatabaseDataType.LastEditApp:
                    LastEditApp = value;
                    break;

                case DatabaseDataType.LastEditMachineName:
                    LastEditMachineName = value;
                    break;

                case DatabaseDataType.LastEditID:
                    LastEditID = value;
                    break;

                    //case DatabaseDataType.Werbung:
                    //    return;
            }
        }
    }

    /// <summary>
    /// Diese Methode entfernt alle bekannten Header-Datentypen, unabhängig von ihrer Position
    /// </summary>
    /// <param name="bytes">Die zu verarbeitenden Bytes</param>
    /// <returns>Eine Liste von Bytes ohne Header-Daten oder null bei Fehlern</returns>
    private List<byte>? RemoveHeaderDataTypes(byte[]? bytes) {
        if (bytes == null) { return null; }
        if (bytes.Length == 0) { return []; }

        var result = new List<byte>(bytes.Length);
        var pointer = 0;
        var filename = ChunkFileName;

        // Durch alle Datensätze gehen
        while (pointer < bytes.Length) {
            var startPointer = pointer;
            var (newPointer, type, _, _, _) = Database.Parse(bytes, pointer, filename);

            // Wenn Parse keine Fortschritte macht, abbrechen um Endlosschleife zu vermeiden
            if (newPointer <= startPointer) {
                return null;
            }

            // Nur Nicht-Header-Datensätze zum Ergebnis hinzufügen
            if (!type.IsHeaderType() && !type.IsObsolete()) {
                // Kompletten Datensatz hinzufügen
                for (var i = startPointer; i < newPointer; i++) {
                    result.Add(bytes[i]);
                }
            }

            pointer = newPointer;
        }

        return result;
    }

    #endregion
}