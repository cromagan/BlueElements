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
public class Chunk : CachedFile, IMultiUserCapable {

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
        var suffix = fullPath.FileSuffix().ToLowerInvariant();

        if (suffix == "hbdb") {
            // .hbdb ist eine Begleitdatei zur .csv-Datei im gleichen Verzeichnis
            MainFileName = fullPath.FilePath() + fullPath.FileNameWithoutSuffix() + ".csv";
        } else if (suffix == "bdbc") {
            // .bdbc sind Chunks im Unterordner.
            // Die Hauptdatei dazu ist immer eine .cbdb Datei eine Ebene höher.
            // Beispiel: ...\MeineTabelle\daten.bdbc -> ...\MeineTabelle.cbdb
            MainFileName = fullPath.FilePath().TrimEnd('\\') + ".cbdb";
        } else {
            // .bdb/.mbdb/.cbdb sind Hauptdateien — MainFileName ist die Datei selbst
            MainFileName = fullPath;
        }
    }

    #endregion

    #region Properties

    int IMultiUserCapable.CockCount { get; set; }
    public override bool ExtendedSave => true;
    public bool IsMain => string.Equals(KeyName, TableFile.Chunk_MainData, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gibt die Chunk-ID zurück (z. B. "MainData", "variables", Hash-Wert).
    /// Für die Hauptdatei (.bdb) wird Chunk_MainData zurückgegeben,
    /// für Chunk-Dateien (.bdbc, .cbdb) der Dateiname ohne Suffix.
    /// </summary>
    public override string KeyName {
        get {
            if (string.Equals(Filename, MainFileName, StringComparison.OrdinalIgnoreCase)) {
                return TableFile.Chunk_MainData.ToLowerInvariant();
            }
            return Filename.FileNameWithoutSuffix().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Chunks werden immer gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => true;

    public bool UsesBlockFile => Filename.FileSuffix().ToLowerInvariant() is "bdbc" or "cbdb";

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet den vollständigen Chunk-Dateipfad aus MainFileName und ChunkId.
    /// </summary>
    public static string ComputeChunkPath(string mainFileName, string chunkId) {
        if (string.Equals(chunkId, TableFile.Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
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
        SaveToByteList(bytes, columnKeyByte.Length, 1);
        bytes.AddRange(columnKeyByte);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(bytes, rowKeyByte.Length, 1);
        bytes.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(bytes, cellContentByte.Length, 2);
        bytes.AddRange(cellContentByte);
    }

    public static void SaveToByteList(List<byte> bytes, TableDataType tableDataType, string content, string columnKey) {
        bytes.Add((byte)Routinen.ColumnUTF8_V401);
        bytes.Add((byte)tableDataType);

        var n = columnKey.UTF8_ToByte();
        SaveToByteList(bytes, n.Length, 1);
        bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(bytes, b.Length, 3);
        bytes.AddRange(b);
    }

    /// <summary>
    /// Alle Spaltendaten außer Systeminfo
    /// </summary>
    public static void SaveToByteList(List<byte> bytes, ColumnItem c) {
        var name = c.KeyName;

        SaveToByteList(bytes, TableDataType.ColumnKey, c.KeyName, name);
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
        SaveToByteList(bytes, TableDataType.DropDownItems, string.Join('\r', c.DropDownItems), name);
        SaveToByteList(bytes, TableDataType.LinkedCellFilter, string.Join('\r', c.LinkedCellFilter), name);
        SaveToByteList(bytes, TableDataType.AutoReplaceAfterEdit, string.Join('\r', c.AfterEditAutoReplace), name);
        SaveToByteList(bytes, TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(bytes, TableDataType.DropdownDeselectAllAllowed, c.DropdownDeselectAllAllowed.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(bytes, TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(bytes, TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(bytes, TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(bytes, TableDataType.MaxTextLength, c.MaxTextLength.ToString1(), name);
        SaveToByteList(bytes, TableDataType.PermissionGroupsChangeCell, string.Join('\r', c.PermissionGroupsChangeCell), name);
        SaveToByteList(bytes, TableDataType.ColumnTags, string.Join('\r', c.ColumnTags), name);
        SaveToByteList(bytes, TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        SaveToByteList(bytes, TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString1(), name);
        SaveToByteList(bytes, TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ScriptType, ((int)c.ScriptType).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ColumnKeyOfLinkedTable, c.ColumnKeyOfLinkedTable, name);
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

    public List<byte> GetHeadBytes() {
        if (LoadFailed) { return []; }

        var headBytes = new List<byte>();

        SaveToByteList(headBytes, TableDataType.Version, Table.TableVersion);
        SaveToByteList(headBytes, TableDataType.Werbung, "                                                                    BlueTable - (c) by Christian Peter                                                                                        ");

        return headBytes;
    }

    public override string IsNowEditable() {
        return ((IMultiUserCapable)this).IsNowEditableWithBlockFile(base.IsNowEditable());
    }

    public override bool IsSaveAbleNow() {
        return ((IMultiUserCapable)this).IsSaveAbleNowWithBlockFile(base.IsSaveAbleNow());
    }

    public override string ReadableText() => $"Chunk '{KeyName}'";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Puzzle, 16);

    public override string ToString() => KeyName;

    internal OperationResult AcquireWriteAccess() {
        return ((IMultiUserCapable)this).AcquireFullWriteAccess(IsNowEditable());
    }

    protected override void OnLoaded() {
        SetMinLen();
        base.OnLoaded();
    }

    protected override void OnSaved() {
        SetMinLen();
    }

    private void SetMinLen() {
        if (Content.Length > 0) {
            MinimumBytes = Math.Max((int)(Content.Length * 0.9), Content.Length - 100);
        } else {
            MinimumBytes = 0;
        }
    }

    #endregion
}