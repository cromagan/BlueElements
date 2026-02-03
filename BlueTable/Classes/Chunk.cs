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

using BlueBasics;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using BlueBasics.Classes;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Chunk : IHasKeyName {

    #region Fields

    public readonly string MainFileName = string.Empty;
    private DateTime _bytesloaded = DateTime.MinValue;
    private string _fileinfo = string.Empty;
    private int _minBytes;

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

            //var folder = MainFileName.FilePath();
            //var tablename = MainFileName.FileNameWithoutSuffix();

            return $"{ChunkFolder()}{KeyName}.bdbc";
        }
    }

    public long DataLength => Bytes.Count;
    public bool IsMain => string.Equals(KeyName, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);
    public bool KeyIsCaseSensitive => false;

    public string KeyName {
        get;
        private set => field = value.ToLowerInvariant();
    }

    public string LastEditApp { get; private set; } = string.Empty;

    public string LastEditID { get; private set; } = string.Empty;

    public string LastEditMachineName { get; private set; } = string.Empty;

    public DateTime LastEditTimeUtc { get; private set; } = DateTime.MinValue;

    public string LastEditUser { get; private set; } = string.Empty;

    public bool LoadFailed { get; set; }

    public bool SaveRequired { get; set; }

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

        var tmp = DateTime.UtcNow;
        if (ReadAndUnzipAllBytes(c).Value is not ByteData byteData) { LoadFailed = true; return; }

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
            var nf = GetFileState(ChunkFileName, false, 0.1f);
            return nf != _fileinfo;
        }

        return false;
    }

    public string Save(string filename) {
        if (LoadFailed) { return "Chunk wurde nicht korrekt geladen"; }
        if (Bytes.Count < _minBytes) { return "Zu große Änderungen, sicherheitshalber geblockt"; }
        if (_bytesloaded.Year < 2000) { return "Chunk noch nicht geladen"; }

        if (Develop.AllReadOnly) { return string.Empty; }

        try {
            Develop.SetUserDidSomething();

            // Extrahiere nur die tatsächlichen Datensätze, keine Header-Daten
            var contentBytes = RemoveHeaderDataTypes([.. Bytes]);
            if (contentBytes == null || contentBytes.Count < _minBytes) { return "Zu große Änderungen, sicherheitshalber geblockt"; }

            // Neuen Header erstellen
            var head = GetHeadAndSetEditor();
            if (head == null || head.Count < 100) { return "Chunk-Kopf konnte nicht erstellt werden"; }

            // Header und Datensätze zusammenführen und komprimieren
            var datacompressed = head.Concat(contentBytes).ToArray().ZipIt();
            if (datacompressed == null || datacompressed.Length < 100) { return "Komprimierug der Daten fehlgeschlagen"; }

            Develop.SetUserDidSomething();

            if (!WriteAllBytes(filename, datacompressed)) {
                return "Speichern fehlgeschlagen";
            }

            _minBytes = (int)(contentBytes.Count * 0.1);

            Develop.SetUserDidSomething();
        } catch (Exception ex) {
            DeleteFile(filename, false);
            return ex.Message;
        }

        return string.Empty;
    }

    public void SaveToByteList(ColumnItem column, RowItem row) {
        if (LoadFailed) { return; }
        if (column.Table is not { IsDisposed: false }) { return; }

        var cellContent = row.CellGetStringCore(column);
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
        SaveToByteList(TableDataType.AfterEditRound, c.AfterEditRound.ToString1(), name);
        SaveToByteList(TableDataType.MaxCellLength, c.MaxCellLength.ToString1(), name);
        SaveToByteList(TableDataType.FixedColumnWidth, c.FixedColumnWidth.ToString1(), name);
        SaveToByteList(TableDataType.AfterEditAutoRemoveChar, c.AfterEditAutoRemoveChar, name);
        SaveToByteList(TableDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(TableDataType.FilterOptions, ((int)c.FilterOptions).ToString1(), name);
        SaveToByteList(TableDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(TableDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(TableDataType.EditableWithTextInput, c.EditableWithTextInput.ToPlusMinus(), name);
        SaveToByteList(TableDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(TableDataType.Relationship_to_First, c.Relationship_to_First.ToPlusMinus(), name);
        //SaveToByteList(TableDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(TableDataType.TextFormatingAllowed, c.TextFormatingAllowed.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ForeColor, c.ForeColor.ToArgb().ToString1(), name);
        SaveToByteList(TableDataType.BackColor, c.BackColor.ToArgb().ToString1(), name);
        SaveToByteList(TableDataType.LineStyleLeft, ((int)c.LineStyleLeft).ToString1(), name);
        SaveToByteList(TableDataType.LineStyleRight, ((int)c.LineStyleRight).ToString1(), name);
        SaveToByteList(TableDataType.BackgroundStyle, ((long)c.BackgroundStyle).ToString1(), name);
        SaveToByteList(TableDataType.RelationType, ((int)c.RelationType).ToString1(), name);
        SaveToByteList(TableDataType.Value_for_Chunk, ((int)c.Value_for_Chunk).ToString1(), name);
        SaveToByteList(TableDataType.EditableWithDropdown, c.EditableWithDropdown.ToPlusMinus(), name);
        SaveToByteList(TableDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(TableDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        //SaveToByteList(l, TableDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(TableDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(TableDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        //SaveToByteList(TableDataType.ColumnSystemInfo, c.SystemInfo, name);
        //SaveToByteList(l, TableDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(TableDataType.MaxTextLength, c.MaxTextLength.ToString1(), name);
        SaveToByteList(TableDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(TableDataType.ColumnTags, c.ColumnTags.JoinWithCr(), name);
        SaveToByteList(TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        //SaveToByteList(l, TableDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        //SaveToByteList(l, TableDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString1(), name);
        SaveToByteList(TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString1(), name);
        SaveToByteList(TableDataType.ScriptType, ((int)c.ScriptType).ToString1(), name);
        //SaveToByteList(l, TableDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(TableDataType.ColumnNameOfLinkedTable, c.ColumnNameOfLinkedTable, name);
        //SaveToByteList(l, TableDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(TableDataType.ColumnAlign, ((int)c.Align).ToString1(), name);
        SaveToByteList(TableDataType.SortType, ((int)c.SortType).ToString1(), name);
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
                Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {KeyName} wurde nicht richtig initialisiert", 0);
                return false; // Explizit false zurückgeben, wenn die Initialisierung fehlschlägt
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Warte auf Abschluss der Initialisierung des Chunks {KeyName}", 0);
            }

            if (LoadFailed && t.ElapsedMilliseconds > 100) { return false; }
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
        Develop.Message(ErrorType.DevelopInfo, this, MainFileName.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere Chunk '{filename.FileNameWithoutSuffix()}'", 0);

        var backup = filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";
        var tempfile = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        var updateTime = DateTime.UtcNow;

        var f = Save(tempfile);
        if (!string.IsNullOrEmpty(f)) { return f; }

        // KRITISCHE ÄNDERUNG: FileInfo der temporären Datei VOR dem Move ermitteln
        // So wissen wir exakt, was wir schreiben und vermeiden Race Conditions
        var tempFileInfo = GetFileState(tempfile, true, 60);
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
        const int maxRetries = 8;
        const int retryDelayMs = 1000;

        for (var attempt = 1; attempt <= maxRetries; attempt++) {
            if (MoveFile(tempfile, filename, false)) {
                // Thread-sichere Aktualisierung in einer logischen Einheit
                lock (this) {
                    _bytesloaded = updateTime;
                    _fileinfo = tempFileInfo;
                }

                return string.Empty;
            }

            Thread.Sleep(retryDelayMs * attempt);

            // Haupt-Datei ist von irgendwo anders her wieder erstellt worden.
            if (FileExists(filename)) {
                DeleteFile(tempfile, false);
                LoadBytesFromDisk(true);
                return "Dateien wurden zwischenzeitlich verändert";
            }

            if (!FileExists(tempfile)) {
                break; // Raus aus der Schleife -> Rollback
            }
        }

        // ROLLBACK: Backup wiederherstellen bei jedem Fehlerfall
        if (FileExists(backup) && !FileExists(filename)) {
            MoveFile(backup, filename, false);
        }

        DeleteFile(tempfile, false);
        return "Speichervorgang unerwartet abgebrochen";
    }

    internal string GrantWriteAccess() {
        var f = IsEditable();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (NeedsReload(true)) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 8) {
            f = CanSaveFile(ChunkFileName, 5);
            if (!string.IsNullOrEmpty(f)) { return f; }

            f = DoExtendedSave();

            if (!string.IsNullOrEmpty(f)) {
                LastEditTimeUtc = DateTime.MinValue;
                return f; // $"Bearbeitung konnte nicht gesetzt werden ({f.ReturnValue as string})";
            }
        }

        return string.Empty;
    }

    internal string IsEditable() {
        if (LoadFailed) { return "Chunk wurde nicht korrekt geladen"; }

        if (NeedsReload(false)) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes < 10) {
            if (LastEditUser != UserName) {
                return $"Aktueller Bearbeiter: {LastEditUser}";
            } else {
                if (LastEditApp != Develop.AppExe()) {
                    return $"Anderes Programm bearbeitet: {LastEditApp}";
                } else {
                    if (LastEditMachineName != Environment.MachineName) {
                        return $"Anderer Computer bearbeitet: {LastEditMachineName} - {LastEditUser}";
                    }
                    if (LastEditID != MyId) {
                        return $"Ein anderer Prozess auf diesem PC bearbeitet gerade.";
                    }
                }
            }
        }

        return CanSaveFile(ChunkFileName, 1);
    }

    internal void SaveToByteList(RowItem thisRow) {
        if (LoadFailed) { return; }
        if (thisRow.Table is not { IsDisposed: false } tb) { return; }

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.SaveContent) {
                SaveToByteList(thisColumn, thisRow);
            }
        }
    }

    internal void SaveToByteListEOF() => SaveToByteList(TableDataType.EOF, "END");

    /// <summary>
    /// Diese Methode entfernt alle bekannten Header-Datentypen, unabhängig von ihrer Position
    /// </summary>
    /// <param name="bytes">Die zu verarbeitenden Bytes</param>
    /// <returns>Eine Liste von Bytes ohne Header-Daten oder null bei Fehlern</returns>
    private static List<byte>? RemoveHeaderDataTypes(byte[]? bytes) {
        if (bytes == null) { return null; }
        if (bytes.Length == 0) { return []; }

        var result = new List<byte>(bytes.Length);
        var pointer = 0;
        //var filename = ChunkFileName;

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
        //var filename = ChunkFileName;

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

    #endregion
}