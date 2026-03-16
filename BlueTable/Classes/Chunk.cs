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
using BlueBasics.Classes;
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
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
[FileSuffix(".bdbc")]
[FileSuffix(".cbdb")]
[FileSuffix(".bdb")]
[FileSuffix(".mbdb")]
[FileSuffix(".hbdb")]
public class Chunk : CachedFile, IHasKeyName {

    #region Fields

    public const int EditTimeInMinutes = 10;
    public readonly string MainFileName = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt einen Chunk anhand von MainFileName und ChunkId.
    /// Nur über CachedFileSystem oder intern aufrufbar.
    /// </summary>
    internal Chunk(string mainFileName, string chunkId) : base(ComputeChunkPath(mainFileName, chunkId)) {
        MainFileName = mainFileName;
    }

    /// <summary>
    /// Konstruktor für die Factory-Erstellung durch CachedFileSystem (via Activator.CreateInstance).
    /// Leitet MainFileName und ChunkId aus dem vollständigen Dateipfad ab.
    /// </summary>
    internal Chunk(string fullPath) : base(fullPath) {
        if (fullPath.FileSuffix().Equals("hbdb", StringComparison.OrdinalIgnoreCase)) {
            // .hbdb ist eine Begleitdatei zur .csv-Datei im gleichen Verzeichnis
            MainFileName = fullPath.FilePath() + fullPath.FileNameWithoutSuffix() + ".csv";
        } else {
            var chunkFolder = fullPath.FilePath();
            var parentFolder = chunkFolder.TrimEnd('\\').FilePath();
            var tableName = chunkFolder.TrimEnd('\\').FileNameWithSuffix();

            MainFileName = parentFolder + tableName + ".bdb";
        }
    }

    #endregion

    #region Properties

    public override bool ExtendedSave => true;
    public bool IsMain => string.Equals(KeyName, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);

    public new bool KeyIsCaseSensitive => false;

    public string LastEditApp { get; private set; } = string.Empty;

    public string LastEditID { get; private set; } = string.Empty;

    public string LastEditMachineName { get; private set; } = string.Empty;

    public DateTime LastEditTimeUtc { get; private set; } = DateTime.MinValue;

    public string LastEditUser { get; private set; } = string.Empty;

    /// <summary>
    /// Chunks werden immer gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => true;

    public bool UsesAdditionalHead => Filename.FileSuffix().ToLowerInvariant() is "bdbc" or "cbdb";

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet den vollständigen Chunk-Dateipfad aus MainFileName und ChunkId.
    /// </summary>
    public static string ComputeChunkPath(string mainFileName, string chunkId) {
        if (string.Equals(chunkId, TableChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
            return mainFileName;
        }

        var folder = mainFileName.FilePath();
        var tablename = mainFileName.FileNameWithoutSuffix();
        return $"{folder}{tablename}\\{chunkId.ToLowerInvariant()}.bdbc";
    }

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

    public static void SaveToByteList(List<byte> bytes, ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false }) { return; }

        var cellContent = row.CellGetStringCore(column);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        bytes.Add((byte)Routinen.CellFormatUTF8_V403);

        var columnKeyByte = column.KeyName.UTF8_ToByte();
        SaveToByteList(bytes!, columnKeyByte.Length, 1);
        bytes.AddRange(columnKeyByte);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(bytes!, rowKeyByte.Length, 1);
        bytes.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(bytes!, cellContentByte.Length, 2);
        bytes.AddRange(cellContentByte);
    }

    public static void SaveToByteList(List<byte> bytes, TableDataType tableDataType, string content, string columnname) {
        bytes.Add((byte)Routinen.ColumnUTF8_V401);
        bytes.Add((byte)tableDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(bytes!, n.Length, 1);
        bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(bytes!, b.Length, 3);
        bytes.AddRange(b);
    }

    /// <summary>
    /// Alle Spaltendaten außer Systeminfo
    /// </summary>
    public static void SaveToByteList(List<byte> bytes, ColumnItem c) {
        var name = c.KeyName;

        SaveToByteList(bytes, TableDataType.ColumnName, c.KeyName, name);
        SaveToByteList(bytes, TableDataType.IsFirst, c.IsFirst.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.IsKeyColumn, c.IsKeyColumn.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(bytes, TableDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(bytes, TableDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(bytes, TableDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.AfterEditRound, c.AfterEditRound.ToString1(), name);
        SaveToByteList(bytes, TableDataType.MaxCellLength, c.MaxCellLength.ToString1(), name);
        SaveToByteList(bytes, TableDataType.FixedColumnWidth, c.FixedColumnWidth.ToString1(), name);
        SaveToByteList(bytes, TableDataType.AfterEditAutoRemoveChar, c.AfterEditAutoRemoveChar, name);
        SaveToByteList(bytes, TableDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.FilterOptions, ((int)c.FilterOptions).ToString1(), name);
        SaveToByteList(bytes, TableDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(bytes, TableDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.EditableWithTextInput, c.EditableWithTextInput.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.Relationship_to_First, c.Relationship_to_First.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.TextFormatingAllowed, c.TextFormatingAllowed.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ForeColor, c.ForeColor.ToArgb().ToString1(), name);
        SaveToByteList(bytes, TableDataType.BackColor, c.BackColor.ToArgb().ToString1(), name);
        SaveToByteList(bytes, TableDataType.LineStyleLeft, ((int)c.LineStyleLeft).ToString1(), name);
        SaveToByteList(bytes, TableDataType.LineStyleRight, ((int)c.LineStyleRight).ToString1(), name);
        SaveToByteList(bytes, TableDataType.BackgroundStyle, ((long)c.BackgroundStyle).ToString1(), name);
        SaveToByteList(bytes, TableDataType.RelationType, ((int)c.RelationType).ToString1(), name);
        SaveToByteList(bytes, TableDataType.Value_for_Chunk, ((int)c.Value_for_Chunk).ToString1(), name);
        SaveToByteList(bytes, TableDataType.EditableWithDropdown, c.EditableWithDropdown.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(bytes, TableDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        SaveToByteList(bytes, TableDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(bytes, TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(bytes, TableDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(bytes, TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(bytes, TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(bytes, TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(bytes, TableDataType.MaxTextLength, c.MaxTextLength.ToString1(), name);
        SaveToByteList(bytes, TableDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(bytes, TableDataType.ColumnTags, c.ColumnTags.JoinWithCr(), name);
        SaveToByteList(bytes, TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        SaveToByteList(bytes, TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString1(), name);
        SaveToByteList(bytes, TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ScriptType, ((int)c.ScriptType).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ColumnNameOfLinkedTable, c.ColumnNameOfLinkedTable, name);
        SaveToByteList(bytes, TableDataType.ColumnAlign, ((int)c.Align).ToString1(), name);
        SaveToByteList(bytes, TableDataType.SortType, ((int)c.SortType).ToString1(), name);
    }

    public static void SaveToByteList(List<byte> bytes, RowItem thisRow) {
        if (thisRow.Table is not { IsDisposed: false } tb) { return; }

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.SaveContent) {
                SaveToByteList(bytes, thisColumn, thisRow);
            }
        }
    }

    public string ChunkFolder() {
        var folder = MainFileName.FilePath();
        var tablename = MainFileName.FileNameWithoutSuffix();

        return $"{folder}{tablename}\\";
    }

    public List<byte> GetHeadAndSetEditor(bool changeEditor) {
        if (LoadFailed) { return []; }

        var headBytes = new List<byte>();

        if (changeEditor) {
            LastEditTimeUtc = DateTime.UtcNow;
            LastEditUser = UserName;
            LastEditApp = Develop.AppExe();
            LastEditMachineName = Environment.MachineName;
            LastEditID = MyId;
        }

        SaveToByteList(headBytes, TableDataType.Version, Table.TableVersion);
        SaveToByteList(headBytes, TableDataType.Werbung, "                                                                    BlueTable - (c) by Christian Peter                                                                                        ");

        if (UsesAdditionalHead) {
            SaveToByteList(headBytes, TableDataType.LastEditTimeUTC, LastEditTimeUtc.ToString5());
            SaveToByteList(headBytes, TableDataType.LastEditUser, LastEditUser);
            SaveToByteList(headBytes, TableDataType.LastEditApp, LastEditApp);
            SaveToByteList(headBytes, TableDataType.LastEditMachineName, LastEditMachineName);
            SaveToByteList(headBytes, TableDataType.LastEditID, MyId);
        }

        return headBytes;
    }

    public override string IsNowEditable() {
        var f = base.IsNowEditable();

        if (!string.IsNullOrEmpty(f)) { return f; }

        if (UsesAdditionalHead) {
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
        }

        return string.Empty;
    }

    public override string ReadableText() => $"Chunk '{KeyName}'";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Puzzle, 16);

    public override string ToString() => KeyName;

    internal OperationResult GrantWriteAccess() {
        var f = IsNowEditable();
        if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed(f); }

        // Wenn die letzte Bearbeitung länger als 8 Minuten her ist oder wir den Lock erneuern müssen
        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes > 8) {
            f = CanWriteFile(Filename, 5);
            if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed(f); }

            // 1. Aktuellen Content holen
            var currentContent = Content;

            // 2. Header entfernen (Nutzdaten extrahieren)
            var contentBytes = RemoveHeaderDataTypes(currentContent, false);
            if (contentBytes == null) { return OperationResult.Failed("Fehler beim Extrahieren der Nutzdaten."); }

            // 3. Neuen Header generieren
            var head = GetHeadAndSetEditor(true);
            if (head == null || head.Count < 100) { return OperationResult.Failed("Chunk-Kopf konnte nicht erstellt werden"); }

            // 4. Zusammenführen und in den Content-Puffer der Basisklasse schreiben
            // Hinweis: base.Content (Setter) markiert die Datei als ungespeichert (_contentHash != _contentOnDiskHash)
            Content = head.Concat(contentBytes).ToArray();

            // 5. Speichern aufrufen (Basisklasse kümmert sich um Zip, Backup und Schreiben)
            var result = base.Save().GetAwaiter().GetResult();

            if (result.IsFailed) {
                LastEditTimeUtc = DateTime.MinValue;
                return result;
            }
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Nach dem Laden von Disk: MinimumBytes ermitteln und Lock-Daten parsen.
    /// Wird automatisch durch den Content-Getter nach einem Frisch-Ladevorgang aufgerufen.
    /// </summary>
    protected override void OnLoaded() {
        SetMinLen();
        ParseLockData();
        base.OnLoaded();
    }

    /// <summary>
    /// Nach erfolgreichem Speichern: MinimumBytes auf Basis der aktuellen Bytezahl aktualisieren.
    /// </summary>
    protected override void OnSaved() {
        SetMinLen();
    }

    /// <summary>
    /// Diese Methode entfernt alle bekannten Header-Datentypen, unabhängig von ihrer Position.
    /// </summary>
    private static List<byte>? RemoveHeaderDataTypes(byte[]? bytes, bool removeUndos) {
        if (bytes == null) { return null; }
        if (bytes.Length == 0) { return []; }

        var result = new List<byte>(bytes.Length);
        var pointer = 0;

        while (pointer < bytes.Length) {
            var startPointer = pointer;
            var (newPointer, type, _, _, _) = Table.Parse(bytes, pointer);

            if (newPointer <= startPointer) { return null; }

            var add = !type.IsHeaderType() && !type.IsObsolete();

            if (removeUndos && type is TableDataType.Undo or TableDataType.UndoInOne) { add = false; }

            if (add) {
                for (var i = startPointer; i < newPointer; i++) {
                    result.Add(bytes[i]);
                }
            }

            pointer = newPointer;
        }

        return result;
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

    private void SetMinLen() {
        if (RemoveHeaderDataTypes(Content, true) is { } b) {
            MinimumBytes = (int)(b.Count * 0.1);
        } else {
            MinimumBytes = 0;
        }
    }

    #endregion
}