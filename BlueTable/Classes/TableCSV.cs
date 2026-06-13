// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using BlueTable.ClassesStatic;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[Browsable(false)]
[FileSuffix(".csv")]
[FileSuffix(".hbdb")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableCSV : TableFile {

    #region Fields

    private const string CsvSuffix = ".csv";
    private const string HbdbSuffix = ".hbdb";
    private CachedTextFile? _cachedTextFile;
    private Chunk? _headChunk;
    private bool _headDirty;
    private char _separator = ';';

    #endregion

    #region Constructors

    public TableCSV(string tablename) : base(tablename) { }

    public TableCSV(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    public bool FirstLineIsHeader {
        get;
        set {
            if (field == value) { return; }
            field = value;
            _headDirty = true;
            SaveRequired = true;
        }
    } = true;

    public char Separator {
        get => _separator;
        set {
            if (_separator == value) { return; }
            _separator = value;
            _headDirty = true;
            SaveRequired = true;
        }
    }

    #endregion

    #region Methods

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        // Nicht-Zell-Änderungen markieren, damit die hbdb-Begleitdatei gespeichert wird
        if (type != TableDataType.UTF8Value_withoutSizeData) {
            _headDirty = true;
        }

        if (_cachedTextFile is null) {
            _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
        }

        if (_cachedTextFile is null) { return "CachedTextFile konnte nicht erstellt werden."; }

        return string.Empty;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (IsDisposed) { return false; }
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (_cachedTextFile is null) {
            _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
        }

        if (_cachedTextFile is null) { return false; }

        if (_cachedTextFile.IsStale()) {
            if (DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"CSV-Datei wurde geändert, lade neu: {KeyName}", 0); }
            _cachedTextFile.Invalidate();
            return LoadCSVFromCachedFile();
        }

        return true;
    }

    public override void LoadFromFile(string fileNameToLoad, NeedPassword? needPassword, string freeze) {
        // .hbdb ist eine Begleitdatei – die zugehörige .csv laden
        if (fileNameToLoad.EndsWith(HbdbSuffix, StringComparison.OrdinalIgnoreCase)) {
            var csvFile = fileNameToLoad.FilePath() + fileNameToLoad.FileNameWithoutSuffix() + CsvSuffix;
            if (!FileExists(csvFile)) { return; }
            fileNameToLoad = csvFile;
        }
        base.LoadFromFile(fileNameToLoad, needPassword, freeze);
    }

    public void SetSeparator(char separator) {
        if (_separator == separator) { return; }
        _separator = separator;
        _headDirty = true;
        SaveRequired = true;
    }

    protected override bool LoadMainData() {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);

        if (_cachedTextFile is null) {
            Freeze("CachedTextFile konnte nicht erstellt werden.");
            return false;
        }

        if (!_cachedTextFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        lock (_undoLock) { Undo.Clear(); }
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

    protected override string SaveInternal() {
        if (IsDisposed) { return string.Empty; }

        if (!SaveRequired) { return string.Empty; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        if (DropMessages) { Develop.Message(ErrorType.DevelopInfo, this, Caption, ImageCode.Tabelle, $"Speichere CSV-Datei '{Caption}'", 0); }

        try {
            var csvContent = CsvHelper.ExportCSV(this, _separator, FirstLineIsHeader);
            if (string.IsNullOrEmpty(csvContent)) {
                return "Fehler beim Generieren des CSV-Inhalts";
            }

            var bytes = csvContent.UTF8_ToByte();

            if (_cachedTextFile is null) {
                _cachedTextFile = CachedFileSystem.Get<CachedTextFile>(Filename);
            }

            if (_cachedTextFile is null) {
                return "CachedTextFile konnte nicht erstellt werden.";
            }

            _cachedTextFile.EnsureContentLoaded();
            _cachedTextFile.Content = bytes;
            var result = _cachedTextFile.Save().GetAwaiter().GetResult();

            if (result.IsFailed) {
                return result.FailedReason ?? "Speichern fehlgeschlagen";
            }

            // hbdb-Begleitdatei nur speichern, wenn Nicht-Zell-Werte geändert wurden oder sie bereits existiert
            if (_headDirty || _headChunk is not null) {
                var x = LastChange;

                // Head-Content aus den Metadaten-Chunks zusammensetzen (ohne Rows)
                List<byte> headContent = new();
                headContent.AddRange(TableChunk.GenerateMainChunk(this));
                headContent.AddRange(TableChunk.GenerateUsesChunk(this));

                if (TableChunk.GenerateHeadVariableChunks(this) is { } varChunk) {
                    headContent.AddRange(varChunk);
                }

                headContent.AddRange(TableChunk.GenerateMasterUserChunk(this));
                headContent.AddRange(TableChunk.GenerateUndoChunk(this, true, string.Empty));
                headContent.AddRange(TableChunk.GenerateEOF());

                if (x != LastChange) { return "Tabelle wurde während der Speicherung geändert."; }

                var headFile = HeadFile();

                if (_headChunk is null) {
                    _headChunk = CachedFileSystem.Get<Chunk>(headFile) ?? new Chunk(headFile);
                }

                if (_headChunk is null) { return "Head-Chunk konnte nicht erstellt werden."; }

                _headChunk.EnsureContentLoaded();
                _headChunk.Content = headContent.ToArray();
                var headResult = _headChunk.Save().GetAwaiter().GetResult();

                if (headResult.IsFailed) {
                    return headResult.FailedReason ?? "Head-Speichern fehlgeschlagen";
                }

                _headDirty = false;
            }

            SaveRequired = false;
            OnInvalidateView();

            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    private string HeadFile() => Path.ChangeExtension(Filename, ".hbdb");

    private bool LoadCSVFromCachedFile() {
        if (IsDisposed) { return false; }

        if (_cachedTextFile is null) { return false; }

        if (!_cachedTextFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        OnLoading();

        lock (_undoLock) { Undo.Clear(); }
        Row.RemoveNullOrEmpty();
        Cell.Clear();

        var content = _cachedTextFile.GetContentAsString(Encoding.UTF8);

        var parsedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ParseCSVContent(content, parsedColumns, parsedRowKeys)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        Row.RemoveObsoleteRows(Row, parsedRowKeys);
        Column.RemoveObsoleteColumns(Column, parsedColumns, Reason.NoUndo_NoInvalidate);

        // Head-Begleitdatei laden und Spaltenmetadaten anwenden
        LoadHeadChunk();

        Column.GetSystems();
        RepairAfterParse();

        SaveRequired = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);

        if (DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"CSV-Datei geladen: {KeyName}", 0); }
        return true;
    }

    private void LoadHeadChunk() {
        var headFile = HeadFile();

        _headChunk = CachedFileSystem.Get<Chunk>(headFile);
        if (_headChunk is null) { return; }

        if (!_headChunk.EnsureContentLoaded()) { return; }

        var data = _headChunk.Content;
        if (data is null || data.Length == 0) { return; }

        Parse(data, true, null);
    }

    private bool ParseCSVContent(string content, HashSet<string> parsedColumns, HashSet<string> parsedRowKeys) {
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r").Trim('\r');
        var lines = content.SplitAndCutByCr();

        if (lines.Length == 0) { return true; }

        var startLine = 0;

        if (FirstLineIsHeader) {
            List<string> columnKeyes = [.. CsvHelper.ParseCSVLine(lines[0], _separator)];
            startLine = 1;

            for (var i = 0; i < columnKeyes.Count; i++) {
                var colName = FormatHolder_SystemName.MakeValid(columnKeyes[i]);
                if (string.IsNullOrEmpty(colName)) {
                    colName = "Column" + i.ToString(CultureInfo.InvariantCulture);
                }

                parsedColumns.Add(colName);

                var col = Column[colName];
                if (col is null) {
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
                if (col is null) {
                    _ = Column.GenerateAndAdd(colName);
                }
            }
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var fields = new List<string>(CsvHelper.ParseCSVLine(lines[lineIndex], _separator));
            if (fields.Count == 0) { continue; }

            var rowKey = fields.Count > 0 ? fields[0] : Guid.NewGuid().ToString();
            parsedRowKeys.Add(rowKey);

            var row = Row.GenerateAndAdd(rowKey, "CSV-Import");

            if (row is null) { continue; }

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

    #endregion
}