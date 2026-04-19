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
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.ClassesStatic;
using BlueTable.Enums;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlueTable.Classes;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableCSV : TableFile {

    #region Fields

    private CachedTextFile? _cachedTextFile;
    private Chunk? _headChunk;
    private bool _headDirty;
    private char _separator = ';';

    #endregion

    #region Constructors

    public TableCSV(string tablename) : base(tablename) {
    }

    #endregion

    #region Destructors

    ~TableCSV() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public bool FirstLineIsHeader {
        get;
        set {
            if (field == value) { return; }
            field = value;
            _headDirty = true;
            IsDirty = true;
        }
    } = true;

    public char Separator {
        get => _separator;
        set {
            if (_separator == value) { return; }
            _separator = value;
            _headDirty = true;
            IsDirty = true;
        }
    }

    protected override bool SaveRequired => base.SaveRequired ||
        (_cachedTextFile != null && !_cachedTextFile.IsSaved) ||
        (_headChunk != null && !_headChunk.IsSaved);

    #endregion

    #region Methods

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (_cachedTextFile == null) {
            _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
        }

        if (_cachedTextFile == null) { return false; }

        if (_cachedTextFile.IsStale()) {
            DropMessage(ErrorType.Info, $"CSV-Datei wurde geändert, lade neu: {KeyName}");
            _cachedTextFile.Invalidate();
            return LoadCSVFromCachedFile();
        }

        return true;
    }

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        // Nicht-Zell-Änderungen markieren, damit die hbdb-Begleitdatei gespeichert wird
        if (type != TableDataType.UTF8Value_withoutSizeData) {
            _headDirty = true;
        }

        if (_cachedTextFile == null) {
            _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
        }

        if (_cachedTextFile == null) { return "CachedTextFile konnte nicht erstellt werden."; }

        return string.Empty;
    }

    public void SetSeparator(char separator) {
        if (_separator == separator) { return; }
        _separator = separator;
        _headDirty = true;
        IsDirty = true;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);

        if (_cachedTextFile == null) {
            Freeze("CachedTextFile konnte nicht erstellt werden.");
            return false;
        }

        if (!_cachedTextFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        Undo.Clear();
        Row.RemoveNullOrEmpty();

        var content = _cachedTextFile.GetContentAsString(Encoding.UTF8);

        var parsedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ParseCSVContent(content, parsedColumns, parsedRowKeys)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        LoadHeadChunk();
        Column.GetSystems();

        return true;
    }

    protected override async Task<string> SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();
        DropMessage(ErrorType.DevelopInfo, $"Speichere CSV-Datei '{Caption}'");

        try {
            var csvContent = CsvHelper.ExportCSV(this, _separator, FirstLineIsHeader);
            if (string.IsNullOrEmpty(csvContent)) {
                return "Fehler beim Generieren des CSV-Inhalts";
            }

            var bytes = csvContent.UTF8_ToByte();

            if (_cachedTextFile == null) {
                _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
            }

            if (_cachedTextFile == null) {
                return "CachedTextFile konnte nicht erstellt werden.";
            }

            _cachedTextFile.Content = bytes;
            var result = await _cachedTextFile.Save().ConfigureAwait(false);

            if (result.IsFailed) {
                return result.FailedReason ?? "Speichern fehlgeschlagen";
            }

            // hbdb-Begleitdatei nur speichern, wenn Nicht-Zell-Werte geändert wurden oder sie bereits existiert
            if (_headDirty || _headChunk != null) {
                var headError = await SaveHeadChunk(setfileStateUtcDateTo).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(headError)) {
                    return headError;
                }
                _headDirty = false;
            }

            LastSaveMainFileUtcDate = setfileStateUtcDateTo;
            IsDirty = false;
            OnInvalidateView();

            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    private string HeadFile() => Path.ChangeExtension(Filename, ".hbdb");

    private bool LoadCSVFromCachedFile() {
        if (_cachedTextFile == null) { return false; }

        if (!_cachedTextFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        OnLoading();

        Undo.Clear();
        Row.RemoveNullOrEmpty();
        Cell.Clear();

        var content = _cachedTextFile.GetContentAsString(Encoding.UTF8);

        var parsedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ParseCSVContent(content, parsedColumns, parsedRowKeys)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        Row.RemoveObsoleteRows(Row, parsedRowKeys, Reason.NoUndo_NoInvalidate);
        Column.RemoveObsoleteColumns(Column, parsedColumns, Reason.NoUndo_NoInvalidate);

        // Head-Begleitdatei laden und Spaltenmetadaten anwenden
        LoadHeadChunk();

        Column.GetSystems();
        RepairAfterParse();

        IsDirty = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);

        DropMessage(ErrorType.Info, $"CSV-Datei geladen: {KeyName}");
        return true;
    }

    private void LoadHeadChunk() {
        var headFile = HeadFile();

        _headChunk = CachedFileSystem.Get<Chunk>(headFile);
        if (_headChunk == null) { return; }

        if (!_headChunk.EnsureContentLoaded()) { return; }

        var data = _headChunk.Content;
        if (data == null || data.Length == 0) { return; }

        Parse(data, true, Reason.NoUndo_NoInvalidate, null);
    }

    private bool ParseCSVContent(string content, HashSet<string> parsedColumns, HashSet<string> parsedRowKeys) {
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r").Trim('\r');
        var lines = content.SplitAndCutByCr();

        if (lines.Length == 0) { return true; }

        var startLine = 0;
        List<string> columnKeyes = [];

        if (FirstLineIsHeader) {
            columnKeyes = [.. CsvHelper.ParseCSVLine(lines[0], _separator)];
            startLine = 1;

            for (var i = 0; i < columnKeyes.Count; i++) {
                var colName = ColumnItem.MakeValidColumnKey(columnKeyes[i]);
                if (string.IsNullOrEmpty(colName)) {
                    colName = "Column" + i.ToString(CultureInfo.InvariantCulture);
                }

                parsedColumns.Add(colName);

                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                    col?.Caption = columnKeyes[i];
                }
            }
        } else {
            var firstLineFields = new List<string>(CsvHelper.ParseCSVLine(lines[0], _separator));
            for (var i = 0; i < firstLineFields.Count; i++) {
                var colName = "Column" + i.ToString(CultureInfo.InvariantCulture);
                parsedColumns.Add(colName);

                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                }
            }
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var fields = new List<string>(CsvHelper.ParseCSVLine(lines[lineIndex], _separator));
            if (fields.Count == 0) { continue; }

            var rowKey = fields.Count > 0 ? fields[0] : Guid.NewGuid().ToString();
            parsedRowKeys.Add(rowKey);

            var row = Row.GenerateAndAdd(rowKey, "CSV-Import");

            if (row == null) { continue; }

            var colIndex = 0;
            foreach (var col in Column) {
                if (col.IsDisposed) { continue; }

                if (colIndex < fields.Count) {
                    row.CellSet(col, fields[colIndex], "CSV-Import");
                }
                colIndex++;
            }
        }

        return true;
    }

    private async Task<string> SaveHeadChunk(DateTime setfileStateUtcDateTo) {
        var headFile = HeadFile();

        // GenerateNewChunks erzeugt die Metadaten (ohne Rows) als einzelnen Chunk
        var chunks = TableChunk.GenerateNewChunks(this, 0, setfileStateUtcDateTo, false, false);
        if (chunks == null || chunks.Count != 1) {
            return "Fehler bei der Head-Chunk Erzeugung";
        }

        if (_headChunk == null) {
            _headChunk = CachedFileSystem.Get<Chunk>(headFile) ?? new Chunk(headFile);
        }

        if (_headChunk == null) { return "Head-Chunk konnte nicht erstellt werden."; }

        _headChunk.Content = chunks[0].Content;
        var result = await _headChunk.Save().ConfigureAwait(false);

        if (result.IsFailed) {
            return result.FailedReason ?? "Head-Speichern fehlgeschlagen";
        }

        return string.Empty;
    }

    #endregion
}