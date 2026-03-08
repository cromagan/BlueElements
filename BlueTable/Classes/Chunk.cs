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
using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
[FileSuffix(".bdbc")]
public class Chunk : CachedFile, IHasKeyName {

    #region Fields

    public const int EditTimeInMinutes = 10;
    public readonly string MainFileName = string.Empty;

    private int _minBytes;

    /// <summary>
    /// Schreibpuffer – wird nur beim Erzeugen neuer Chunks (InitByteList/SaveToByteList) verwendet.
    /// Wenn null, kommen die Bytes aus CachedFile.Content (Lese-Modus).
    /// </summary>
    private List<byte>? _writeBuffer;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt einen Chunk anhand von MainFileName und ChunkId.
    /// Nur über CachedFileSystem oder intern aufrufbar.
    /// </summary>
    internal Chunk(string mainFileName, string chunkId) : base(ComputeChunkPath(mainFileName, chunkId)) {
        MainFileName = mainFileName;
        KeyName = chunkId;
    }

    /// <summary>
    /// Konstruktor für die Factory-Erstellung durch CachedFileSystem (via Activator.CreateInstance).
    /// Leitet MainFileName und ChunkId aus dem vollständigen Dateipfad ab.
    /// </summary>
    internal Chunk(string fullPath) : base(fullPath) {
        var chunkFolder = fullPath.FilePath();
        var parentFolder = chunkFolder.TrimEnd('\\').FilePath();
        var tableName = chunkFolder.TrimEnd('\\').FileNameWithSuffix();

        MainFileName = parentFolder + tableName + ".bdb";
        KeyName = fullPath.FileNameWithoutSuffix();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Im Schreib-Modus (nach InitByteList): gibt den Schreibpuffer zurück.
    /// Im Lese-Modus: gibt die entpackten Bytes über CachedFile.Content zurück.
    /// </summary>
    public List<byte> Bytes {
        get {
            if (_writeBuffer != null) { return _writeBuffer; }
            return [.. Content];
        }
        private set => _writeBuffer = value;
    }

    public long DataLength => _writeBuffer?.Count ?? Content.Length;
    public bool IsMain => string.Equals(KeyName, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// IHasKeyName — überschreibt CachedFile.KeyName.
    /// </summary>
    public new string KeyName {
        get;
        private set => field = value.ToLowerInvariant();
    }

    public new bool KeyIsCaseSensitive => false;

    /// <summary>
    /// Chunks werden immer gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => true;

    public string LastEditApp { get; private set; } = string.Empty;

    public string LastEditID { get; private set; } = string.Empty;

    public string LastEditMachineName { get; private set; } = string.Empty;

    public DateTime LastEditTimeUtc { get; private set; } = DateTime.MinValue;

    public string LastEditUser { get; private set; } = string.Empty;

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
    /// Initialisiert den Schreibpuffer für das Erzeugen neuer Chunk-Daten.
    /// </summary>
    public void InitByteList() {
        LoadFailed = false;
        _writeBuffer = [];
    }

    /// <summary>
    /// Lädt die Bytes über CachedFile und holt sich die Lock-Daten.
    /// </summary>
    public void LoadBytesFromDisk(bool mustexist) {
        if (!FileExists(Filename)) {
            if (mustexist) {
                LoadFailed = true;
                return;
            }

            _minBytes = 0;
            _writeBuffer = [];
            LoadFailed = false;
            return;
        }

        // Schreibpuffer leeren → Lese-Modus über CachedFile
        _writeBuffer = null;

        // Cache invalidieren, damit CachedFile frisch von Platte liest
        Invalidate();

        var content = Content;
        if (content.Length == 0) { LoadFailed = true; return; }

        if (RemoveHeaderDataTypes(content) is { } b) {
            _minBytes = (int)(b.Count * 0.1);
        }

        ParseLockData();
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
    public void SaveToByteList(ColumnItem c) {
        if (LoadFailed) { return; }

        var name = c.KeyName;

        SaveToByteList(TableDataType.ColumnName, c.KeyName, name);
        SaveToByteList(TableDataType.IsFirst, c.IsFirst.ToPlusMinus(), name);
        SaveToByteList(TableDataType.IsKeyColumn, c.IsKeyColumn.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(TableDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(TableDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(TableDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(TableDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(TableDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(TableDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
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
        SaveToByteList(TableDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(TableDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(TableDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(TableDataType.MaxTextLength, c.MaxTextLength.ToString1(), name);
        SaveToByteList(TableDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(TableDataType.ColumnTags, c.ColumnTags.JoinWithCr(), name);
        SaveToByteList(TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        SaveToByteList(TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString1(), name);
        SaveToByteList(TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString1(), name);
        SaveToByteList(TableDataType.ScriptType, ((int)c.ScriptType).ToString1(), name);
        SaveToByteList(TableDataType.ColumnNameOfLinkedTable, c.ColumnNameOfLinkedTable, name);
        SaveToByteList(TableDataType.ColumnAlign, ((int)c.Align).ToString1(), name);
        SaveToByteList(TableDataType.SortType, ((int)c.SortType).ToString1(), name);
    }

    public override string ToString() => KeyName;

    /// <summary>
    /// Prüft, ob die Bytes geladen werden konnten.
    /// </summary>
    public bool WaitBytesLoaded() {
        if (LoadFailed) { return false; }

        var content = Content;
        if (content.Length == 0 && _writeBuffer == null) {
            return !LoadFailed;
        }

        return true;
    }

    /// <summary>
    /// Berechnet den vollständigen Chunk-Dateipfad aus MainFileName und ChunkId.
    /// </summary>
    internal static string ComputeChunkPath(string mainFileName, string chunkId) {
        if (string.Equals(chunkId, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
            return mainFileName;
        }

        var folder = mainFileName.FilePath();
        var tablename = mainFileName.FileNameWithoutSuffix();
        return $"{folder}{tablename}\\{chunkId.ToLowerInvariant()}.bdbc";
    }

    /// <summary>
    /// Markiert den Chunk als fehlgeschlagen geladen.
    /// Erlaubt Zugriff auf den protected Setter von CachedFile.LoadFailed von außerhalb der Vererbungshierarchie.
    /// </summary>
    internal void MarkLoadFailed() { LoadFailed = true; }

    internal bool Delete() {
        if (DeleteFile(Filename, 120)) {
            _writeBuffer = [];
            Invalidate();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Liefert die zu speichernden unkomprimierten Bytes (Header + Nutzdaten).
    /// Komprimierung übernimmt DoExtendedSave der Basisklasse (MustZipped = true).
    /// </summary>
    protected override byte[] GetContent() {
        if (LoadFailed) { return []; }

        var contentBytes = RemoveHeaderDataTypes([.. Bytes]);
        if (contentBytes == null || contentBytes.Count < _minBytes) { return []; }

        var head = GetHeadAndSetEditor();
        if (head == null || head.Count < 100) { return []; }

        return [.. head, .. contentBytes];
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// </summary>
    public override bool IsSaveAbleNow() {
        if (!base.IsSaveAbleNow()) { return false; }
        if (LoadFailed) { return false; }
        if (_writeBuffer != null && _writeBuffer.Count < _minBytes) { return false; }
        return true;
    }

    /// <summary>
    /// Speichert asynchron — nutzt GetContent() und die Backup-Rotation der Basisklasse.
    /// </summary>
    public override async Task<string> DoExtendedSave() {
        Develop.Message(ErrorType.DevelopInfo, this, MainFileName.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere Chunk '{Filename.FileNameWithoutSuffix()}'", 0);

        var result = await base.DoExtendedSave().ConfigureAwait(false);

        if (string.IsNullOrEmpty(result)) {
            _minBytes = (int)(Bytes.Count * 0.1);
        }

        return result;
    }

    internal string GrantWriteAccess() {
        var f = IsEditable();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 8) {
            f = CanWriteFile(Filename, 5);
            if (!string.IsNullOrEmpty(f)) { return f; }

            f = DoExtendedSave().GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(f)) {
                LastEditTimeUtc = DateTime.MinValue;
                return f;
            }
        }

        return string.Empty;
    }

    internal string IsEditable() {
        if (LoadFailed) { return "Chunk wurde nicht korrekt geladen"; }

        if (IsStale()) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes < EditTimeInMinutes) {
            var t = LastEditTimeUtc.AddMinutes(EditTimeInMinutes).ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            if (LastEditUser != UserName) {
                return $"Aktueller Bearbeiter: {LastEditUser} noch bis {t}";
            } else {
                if (LastEditApp != Develop.AppExe()) {
                    return $"Anderes Programm bearbeitet: {LastEditApp.FileNameWithoutSuffix()} noch bis {t}";
                } else {
                    if (LastEditMachineName != Environment.MachineName) {
                        return $"Anderer Computer bearbeitet: {LastEditMachineName} - {LastEditUser} noch bis {t}";
                    }
                    if (LastEditID != MyId) {
                        return $"Ein anderer Prozess auf diesem PC bearbeitet noch bis {t}.";
                    }
                }
            }
        }

        return CanWriteFile(Filename, 2);
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
    private static List<byte>? RemoveHeaderDataTypes(byte[]? bytes) {
        if (bytes == null) { return null; }
        if (bytes.Length == 0) { return []; }

        var result = new List<byte>(bytes.Length);
        var pointer = 0;

        while (pointer < bytes.Length) {
            var startPointer = pointer;
            var (newPointer, type, _, _, _) = Table.Parse(bytes, pointer);

            if (newPointer <= startPointer) {
                return null;
            }

            if (!type.IsHeaderType() && !type.IsObsolete()) {
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

        LastEditTimeUtc = DateTime.UtcNow;
        LastEditUser = UserName;
        LastEditApp = Develop.AppExe();
        LastEditMachineName = Environment.MachineName;
        LastEditID = MyId;

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
        var data = Content;

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
            }
        }
    }

    #endregion
}
