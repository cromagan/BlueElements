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
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueTable;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Chunk : IHasKeyName {

    #region Fields

    public readonly string MainFileName = string.Empty;
    private DateTime _bytesloaded = DateTime.MinValue;
    private string _fileinfo = string.Empty;
    private string _keyname = string.Empty;
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
            var tablename = MainFileName.FileNameWithoutSuffix();

            return $"{ChunkFolder()}{KeyName}.bdbc";
        }
    }

    public long DataLength => Bytes?.Count ?? 0;
    public bool IsMain => string.Equals(KeyName, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);
    public bool KeyIsCaseSensitive => false;

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

    public static void SaveToByteList(List<byte> bytes, TableDataType tableDataType, string content) {
        var b = content.UTF8_ToByte();
        bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        bytes.Add((byte)tableDataType);
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

    public string ChunkFolder() {
        var folder = MainFileName.FilePath();
        var tablename = MainFileName.FileNameWithoutSuffix();

        return $"{folder}{tablename}\\";
    }

    /// <summary>
    /// Initialisiert die Byteliste
    /// </summary>
    public void InitByteList() {
        LoadFailed = false;

        _bytesloaded = DateTime.UtcNow;

        Bytes = [];
    }

    /// <summary>
    /// Lädt die Bytes und holt sich NUR die Lock-Daten.
    /// Ansonsten wird nichts geparsed.
    /// </summary>
    public void LoadBytesFromDisk(bool mustexist) {
        var c = ChunkFileName;

        if (!FileExists(c)) {
            if (mustexist) {
                LoadFailed = true;
                return;
            }

            _minBytes = 0;
            InitByteList();
            return;
        }

        var byteData = LoadAndUnzipAllBytes(c);
        var tmp = DateTime.UtcNow;
        if (byteData is null) { LoadFailed = true; return; }

        _fileinfo = byteData.FileInfo;

        if (RemoveHeaderDataTypes(byteData.Bytes) is { } b) {
            _minBytes = (int)(b.Count * 0.1);
        }

        Bytes.Clear();
        Bytes = [.. byteData.Bytes];

        _bytesloaded = tmp;

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
        if (string.IsNullOrEmpty(MainFileName)) { return false; } // Temporäre Tabellen

        // Prüfe, ob die Datei überhaupt existiert
        if (!FileExists(ChunkFileName)) {
            return Bytes.Count > 0; // Nur neu laden, wenn wir Daten haben, die "verschwunden" sind
        }

        if (DateTime.UtcNow.Subtract(_bytesloaded).TotalMinutes > 6 || important) {
            var nf = GetFileState(ChunkFileName, false);
            return nf != _fileinfo;
        }

        return false;
    }

    public FileOperationResult Save(string filename) {
        if (LoadFailed) { return new("Chunk wurde nicht korrekt geladen", false, true); }
        if (Bytes.Count < _minBytes) { return new("Zu große Änderungen, sicherheitshalber geblockt", false, true); }
        if (_bytesloaded.Year < 2000) { return new("Chunk noch nicht geladen", false, true); }

        if (Develop.AllReadOnly) { return FileOperationResult.ValueStringEmpty; }

        try {
            Develop.SetUserDidSomething();

            // Extrahiere nur die tatsächlichen Datensätze, keine Header-Daten
            var contentBytes = RemoveHeaderDataTypes(Bytes.ToArray());
            if (contentBytes == null || contentBytes.Count < _minBytes) { return new("Zu große Änderungen, sicherheitshalber geblockt", false, true); }

            // Neuen Header erstellen
            var head = GetHeadAndSetEditor();
            if (head == null || head.Count < 100) { return new("Chunk-Kopf konnte nicht erstellt werden", false, true); }

            // Header und Datensätze zusammenführen und komprimieren
            var datacompressed = head.Concat(contentBytes).ToArray().ZipIt();
            if (datacompressed == null || datacompressed.Length < 100) { return new("Komprimierug der Daten fehlgeschlagen", true, true); }

            Develop.SetUserDidSomething();

            if (!WriteAllBytes(filename, datacompressed)) {
                return new("Speichern fehlgeschlagen", false, true);
            }

            _minBytes = (int)(contentBytes.Count * 0.1);

            Develop.SetUserDidSomething();
        } catch {
            DeleteFile(filename, false);
            return new("Allgemeiner Fehler", false, true);
        }

        return FileOperationResult.ValueStringEmpty;
    }

    public void SaveToByteList(ColumnItem column, RowItem row) {
        if (LoadFailed) { return; }
        if (column.Table is not { IsDisposed: false } db) { return; }

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

    public void SaveToByteList(TableDataType tableDataType, string content, string columnname) {
        if (LoadFailed) { return; }
        Bytes.Add((byte)Routinen.ColumnUTF8_V401);
        Bytes.Add((byte)tableDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(Bytes, n.Length, 1);
        Bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(Bytes, b.Length, 3);
        Bytes.AddRange(b);
    }

    public void SaveToByteList(TableDataType tableDataType, string content) {
        if (LoadFailed) { return; }
        var b = content.UTF8_ToByte();
        Bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        Bytes.Add((byte)tableDataType);
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

        SaveToByteList(TableDataType.ColumnName, c.KeyName, name);
        SaveToByteList(TableDataType.IsFirst, c.IsFirst.ToPlusMinus(), name);
        SaveToByteList(TableDataType.IsKeyColumn, c.IsKeyColumn.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ColumnCaption, c.Caption, name);
        //SaveToByteList(TableDataType.ColumnFunction, ((int)c.Function).ToString(), name);
        SaveToByteList(TableDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(TableDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(TableDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(TableDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(TableDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(TableDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        //SaveToByteList(l, TableDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(TableDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(TableDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(TableDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(TableDataType.AfterEditRound, c.AfterEditRound.ToString(), name);
        SaveToByteList(TableDataType.MaxCellLength, c.MaxCellLength.ToString(), name);
        SaveToByteList(TableDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(TableDataType.AfterEditAutoRemoveChar, c.AfterEditAutoRemoveChar, name);
        SaveToByteList(TableDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(TableDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(TableDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(TableDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(TableDataType.EditableWithTextInput, c.EditableWithTextInput.ToPlusMinus(), name);
        SaveToByteList(TableDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(TableDataType.Relationship_to_First, c.Relationship_to_First.ToPlusMinus(), name);
        //SaveToByteList(TableDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(TableDataType.TextFormatingAllowed, c.TextFormatingAllowed.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(TableDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(TableDataType.LineStyleLeft, ((int)c.LineStyleLeft).ToString(), name);
        SaveToByteList(TableDataType.LineStyleRight, ((int)c.LineStyleRight).ToString(), name);
        SaveToByteList(TableDataType.RelationType, ((int)c.RelationType).ToString(), name);
        SaveToByteList(TableDataType.Value_for_Chunk, ((int)c.Value_for_Chunk).ToString(), name);
        SaveToByteList(TableDataType.EditableWithDropdown, c.EditableWithDropdown.ToPlusMinus(), name);
        SaveToByteList(TableDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(TableDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        //SaveToByteList(l, TableDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(TableDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(TableDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ColumnQuickInfo, c.ColumnQuickInfo, name);
        SaveToByteList(TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        //SaveToByteList(TableDataType.ColumnSystemInfo, c.SystemInfo, name);
        //SaveToByteList(l, TableDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(TableDataType.MaxTextLength, c.MaxTextLength.ToString(), name);
        SaveToByteList(TableDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(TableDataType.ColumnTags, c.ColumnTags.JoinWithCr(), name);
        SaveToByteList(TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        //SaveToByteList(l, TableDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        //SaveToByteList(l, TableDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(TableDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        //SaveToByteList(l, TableDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(TableDataType.ColumnNameOfLinkedTable, c.ColumnNameOfLinkedTable, name);
        //SaveToByteList(l, TableDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(TableDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(TableDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, TableDataType.ColumnTimeCode, column.TimeCode, key);
    }

    public override string ToString() => KeyName;

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Initialladung ausgeführt wurde
    /// </summary>
    /// <returns>True, wenn die Initialisierung erfolgreich abgeschlossen wurde, sonst False</returns>
    public bool WaitBytesLoaded() {
        var t = Stopwatch.StartNew();
        var lastMessageTime = 0L;

        while (_bytesloaded.Year < 2000) {
            Thread.Sleep(20); // Längere Pause zur Reduzierung der CPU-Last

            if (t.ElapsedMilliseconds > 120 * 1000) {
                Develop.Message?.Invoke(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {KeyName} wurde nicht richtig initialisiert", 0);
                return false; // Explizit false zurückgeben, wenn die Initialisierung fehlschlägt
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.Message?.Invoke(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Warte auf Abschluss der Initialisierung des Chunks {KeyName}", 0);
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

    internal FileOperationResult DoExtendedSave() {
        var filename = ChunkFileName;
        Develop.Message?.Invoke(ErrorType.DevelopInfo, this, MainFileName.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere Chunk '{filename.FileNameWithoutSuffix()}'", 0);

        var backup = filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";
        var tempfile = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        var updateTime = DateTime.UtcNow;

        var f = Save(tempfile);

        if (f.Failed) { return f; }

        // KRITISCHE ÄNDERUNG: FileInfo der temporären Datei VOR dem Move ermitteln
        // So wissen wir exakt, was wir schreiben und vermeiden Race Conditions
        var tempFileInfo = GetFileState(tempfile, true);
        if (string.IsNullOrEmpty(tempFileInfo)) {
            DeleteFile(tempfile, false);
            return new("Dateiinfo konnte nicht gelesen werden", false, true);
        }

        if (FileExists(backup) && !DeleteFile(backup, false)) {
            DeleteFile(tempfile, false);
            return new("Backup konnte nicht gelöscht werden", false, true);
        }

        if (FileExists(filename) && !MoveFile(filename, backup, false)) {
            DeleteFile(tempfile, false);
            return new("Hauptdatei konnte nicht verschoben werden", false, true);
        }

        // --- TmpFile wird zum Haupt ---
        const int maxRetries = 5;
        const int retryDelayMs = 1000;

        for (var attempt = 1; attempt <= maxRetries; attempt++) {
            if (MoveFile(tempfile, filename, false)) {
                // Thread-sichere Aktualisierung in einer logischen Einheit
                lock (this) {
                    _bytesloaded = updateTime;
                    _fileinfo = tempFileInfo;
                }

                return FileOperationResult.ValueStringEmpty;
            }

            // Paralleler Prozess hat gespeichert?
            if (FileExists(filename)) {
                DeleteFile(tempfile, false);
                LoadBytesFromDisk(true);
                return new("Dateien wurden zwischenzeitlich verändert", true, true);
            }

            Thread.Sleep(retryDelayMs * attempt);

            if (!FileExists(tempfile)) { return new("Temp-Datei Zugriffsfehler", false, true); }
        }

        // Aufräumen falls alles fehlschlägt
        DeleteFile(tempfile, false);
        return new("Speichervorgang unerwartet abgebrochen", false, true);
    }

    internal FileOperationResult GrantWriteAccess() {
        var f = IsEditable();
        if (f.Failed) { return f; }

        if (NeedsReload(true)) { return new("Daten müssen neu geladen werden.", false, true); }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 8) {
            f = CanSaveFile(ChunkFileName, 5);
            if (f.Failed) { return f; }

            f = DoExtendedSave();

            if (f.Failed) {
                LastEditTimeUtc = DateTime.MinValue;
                return f; // $"Bearbeitung konnte nicht gesetzt werden ({f.ReturnValue as string})";
            }
        }

        return FileOperationResult.ValueStringEmpty;
    }

    internal FileOperationResult IsEditable() {
        if (LoadFailed) { return new("Chunk wurde nicht korrekt geladen", false, true); }

        if (NeedsReload(false)) { return new("Daten müssen neu geladen werden.", false, true); }

        var tryagain = DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 9;

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes < 10) {
            if (LastEditUser != UserName) {
                return new($"Aktueller Bearbeiter: {LastEditUser}", tryagain, true);
            } else {
                if (LastEditApp != Develop.AppExe()) {
                    return new($"Anderes Programm bearbeitet: {LastEditApp}", tryagain, true);
                } else {
                    if (LastEditMachineName != Environment.MachineName) {
                        return new($"Anderer Computer bearbeitet: {LastEditMachineName} - {LastEditUser}", tryagain, true);
                    }
                    if (LastEditID != MyId) {
                        return new($"Ein anderer Prozess auf diesem PC bearbeitet gerade.", tryagain, true);
                    }
                }
            }
        }

        return CanSaveFile(ChunkFileName, 1);
    }

    internal void SaveToByteList(RowItem thisRow) {
        if (LoadFailed) { return; }
        if (thisRow.Table is not { IsDisposed: false } db) { return; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn.SaveContent) {
                SaveToByteList(thisColumn, thisRow);
            }
        }
    }

    internal void SaveToByteListEOF() => SaveToByteList(TableDataType.EOF, "END");

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
        SaveToByteList(headBytes, TableDataType.Version, Table.TableVersion);
        SaveToByteList(headBytes, TableDataType.Werbung, "                                                                    BlueTable - (c) by Christian Peter                                                                                        ");

        SaveToByteList(headBytes, TableDataType.LastEditTimeUTC, LastEditTimeUtc.ToString5());
        SaveToByteList(headBytes, TableDataType.LastEditUser, LastEditUser);
        SaveToByteList(headBytes, TableDataType.LastEditApp, LastEditApp);
        SaveToByteList(headBytes, TableDataType.LastEditMachineName, LastEditMachineName);
        SaveToByteList(headBytes, TableDataType.LastEditID, MyId);

        return headBytes;
    }

    private void ParseLockData() {
        var pointer = 0;
        var data = Bytes.ToArray();
        var filename = ChunkFileName;

        while (pointer < data.Length) {
            var (newPointer, type, value, _, _) = Table.Parse(data, pointer);
            pointer = newPointer;

            switch (type) {
                case TableDataType.LastEditTimeUTC:
                    LastEditTimeUtc = DateTimeParse(value);
                    break;

                case TableDataType.LastEditUser:
                    LastEditUser = value;
                    break;

                case TableDataType.LastEditApp:
                    LastEditApp = value;
                    break;

                case TableDataType.LastEditMachineName:
                    LastEditMachineName = value;
                    break;

                case TableDataType.LastEditID:
                    LastEditID = value;
                    break;

                    //case TableDataType.Werbung:
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
            var (newPointer, type, _, _, _) = Table.Parse(bytes, pointer);

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