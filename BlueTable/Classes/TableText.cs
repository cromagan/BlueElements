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
using BlueBasics.Classes.FileHelpers;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

/// <summary>
/// Text-basierte Tabelle, die CSV-Dateien nutzt.
/// Unterstützt zwei Modi:
/// - Einfach-Modus: Einzelne .csv-Datei, kein Header, kein Chunk-System.
/// - Verzeichnis-Modus: .tbdb-Header-Datei + .csv-Datendateien (optional mit Chunk-System).
///   Beim Öffnen einer .tbdb-Datei wird das gesamte Verzeichnis eingelesen.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableText : TableFile {

    #region Fields

    /// <summary>CachedTextFiles für Chunk-CSV-Dateien: ChunkId → CachedTextFile.</summary>
    private readonly Dictionary<string, CachedTextFile> _chunkFiles = new(StringComparer.OrdinalIgnoreCase);

    private ChunkType _chunkType = ChunkType.None;

    /// <summary>CachedTextFile für die einzelne .csv-Datei (nur im Einfach-Modus).</summary>
    private CachedTextFile? _dataFile;

    private bool _firstLineIsHeader = true;

    private char _separator = ';';

    /// <summary>Chunk für die .tbdb-Header-Datei (nur im Verzeichnis-Modus, binär/gezippt).</summary>
    private Chunk? _tbdbChunk;

    #endregion

    #region Constructors

    public TableText(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~TableText() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public bool FirstLineIsHeader {
        get => _firstLineIsHeader;
        set {
            if (_firstLineIsHeader == value) { return; }
            _firstLineIsHeader = value;
            IsDirty = true;
        }
    }

    public char Separator {
        get => _separator;
        set {
            if (_separator == value) { return; }
            _separator = value;
            IsDirty = true;
        }
    }

    public ChunkType TextChunkType {
        get => _chunkType;
        set {
            if (_chunkType == value) { return; }
            _chunkType = value;
            IsDirty = true;
        }
    }

    /// <summary>
    /// Gibt an, ob beim Öffnen eine .tbdb-Datei zugrunde liegt (Verzeichnis-Modus mit optionalem Chunk-System).
    /// </summary>
    public bool UseChunkSystem => Filename.FileSuffix().Equals("tbdb", StringComparison.OrdinalIgnoreCase);

    protected override bool SaveRequired {
        get {
            if (base.SaveRequired) { return true; }
            if (_tbdbChunk != null && !_tbdbChunk.IsSaved) { return true; }
            if (_dataFile != null && !_dataFile.IsSaved) { return true; }
            foreach (var cf in _chunkFiles.Values) {
                if (!cf.IsSaved) { return true; }
            }
            return false;
        }
    }

    #endregion

    #region Methods

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (UseChunkSystem) {
            // Verzeichnis-Modus: Header und alle CSV-Chunks prüfen
            if (_tbdbChunk != null && _tbdbChunk.IsStale()) {
                DropMessage(ErrorType.Info, $"tbdb-Header geändert, lade neu: {KeyName}");
                _tbdbChunk.Invalidate();
                return ReloadFromTbdb();
            }

            foreach (var kvp in _chunkFiles) {
                if (kvp.Value.IsStale()) {
                    DropMessage(ErrorType.Info, $"CSV-Chunk geändert, lade neu: {kvp.Key}");
                    kvp.Value.Invalidate();
                    return ReloadChunkCsv(kvp.Key, kvp.Value);
                }
            }
        } else {
            // Einfach-Modus: Einzelne CSV-Datei prüfen
            if (_dataFile != null && _dataFile.IsStale()) {
                DropMessage(ErrorType.Info, $"CSV-Datei geändert, lade neu: {KeyName}");
                _dataFile.Invalidate();
                return LoadSimpleCsv();
            }
        }

        return true;
    }

    /// <summary>
    /// Gibt den Chunk-Verzeichnispfad für diese Tabelle zurück.
    /// Beispiel: C:\Data\MyTable\
    /// </summary>
    public string ChunkFolder() {
        var folder = Filename.FilePath();
        var tablename = Filename.FileNameWithoutSuffix();
        return $"{folder}{tablename}\\";
    }

    /// <summary>
    /// Berechnet die Chunk-ID für eine Zeile anhand des ChunkType und des Zeilen-Keys.
    /// </summary>
    public string GetTextChunkId(RowItem row) {
        if (row == null || _chunkType == ChunkType.None) { return string.Empty; }

        var keyValue = row.KeyName.ToLowerInvariant();

        switch (_chunkType) {
            case ChunkType.ByHash_1Char:
                return keyValue.GetSHA256HashString().Right(1).ToLowerInvariant();

            case ChunkType.ByHash_2Chars:
                return keyValue.GetSHA256HashString().Right(2).ToLowerInvariant();

            case ChunkType.ByHash_3Chars:
                return keyValue.GetSHA256HashString().Right(3).ToLowerInvariant();

            case ChunkType.ByName:
                var t = ColumnItem.MakeValidColumnName(keyValue);
                return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();

            default:
                return string.Empty;
        }
    }

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (UseChunkSystem) {
            if (_tbdbChunk == null) {
                _tbdbChunk = CachedFileSystem.GetOrCreate<Chunk>(Filename);
            }
            if (_tbdbChunk == null) { return "Konnte tbdb-Header-Datei nicht erstellen."; }
        } else {
            if (_dataFile == null) {
                _dataFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);
            }
            if (_dataFile == null) { return "Konnte CSV-Datei nicht erstellen."; }
        }

        return string.Empty;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        if (UseChunkSystem) {
            return LoadFromTbdb();
        } else {
            return LoadSimpleCsvFile();
        }
    }

    protected override async Task<string> SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();

        if (UseChunkSystem) {
            return await SaveWithTbdb(setfileStateUtcDateTo).ConfigureAwait(false);
        } else {
            return await SaveSimpleCsv(setfileStateUtcDateTo).ConfigureAwait(false);
        }
    }

    private Dictionary<string, string> BuildChunkCsvData() {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Spalten ermitteln
        var columnNames = new List<string>();
        foreach (var col in Column) {
            if (!col.IsDisposed && col.SaveContent) { columnNames.Add(col.KeyName); }
        }

        // Header-Zeile für jede Chunk-Datei
        string? headerLine = null;
        if (_firstLineIsHeader) {
            headerLine = CSVHelper.EscapeFields(columnNames, _separator);
        }

        // Zeilen nach Chunk aufteilen
        foreach (var row in Row) {
            if (row.IsDisposed) { continue; }

            var chunkId = GetTextChunkId(row);
            if (string.IsNullOrEmpty(chunkId)) { chunkId = "_other"; }

            if (!result.TryGetValue(chunkId, out var sb)) {
                sb = headerLine != null ? headerLine + "\r\n" : string.Empty;
                result[chunkId] = sb;
            }

            var fields = new List<string>();
            foreach (var colName in columnNames) {
                var col = Column[colName];
                fields.Add(col != null && !col.IsDisposed ? row.CellGetString(col) : string.Empty);
            }

            result[chunkId] += CSVHelper.EscapeFields(fields, _separator) + "\r\n";
        }

        return result;
    }

    private string BuildTbdbHeader() {
        var ini = new IniHelper();

        ini.Set("Table", "Separator", _separator.ToString());
        ini.Set("Table", "FirstLineIsHeader", _firstLineIsHeader ? "+" : "-");
        ini.Set("Table", "ChunkType", ((int)_chunkType).ToString());

        if (!string.IsNullOrEmpty(Caption)) { ini.Set("Table", "Caption", Caption); }
        if (!string.IsNullOrEmpty(Creator)) { ini.Set("Table", "Creator", Creator); }

        foreach (var col in Column) {
            if (col.IsDisposed || col.IsSystemColumn()) { continue; }
            var section = "Column_" + col.KeyName;
            ini.Set(section, "Caption", col.Caption ?? col.KeyName);
        }

        return ini.SerializeContent();
    }

    private bool LoadChunkCsvRows(CachedTextFile csvFile) {
        if (!csvFile.EnsureContentLoaded()) { return false; }

        var content = csvFile.GetContentAsString(Encoding.UTF8);
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r").Trim('\r');
        var lines = content.Split('\r');

        var startLine = 0;

        // Wenn der CSV keine Kopfzeile hat, aber tbdb Spalten definiert hat → kein Header
        // Wenn es eine Kopfzeile gibt → erste Zeile überspringen oder Spalten anlegen
        if (_firstLineIsHeader && lines.Length > 0) {
            var headerFields = CSVHelper.ParseLine(lines[0], _separator);
            // Spalten ggf. anlegen falls noch nicht vorhanden
            for (var i = 0; i < headerFields.Count; i++) {
                var colName = ColumnItem.MakeValidColumnName(headerFields[i]);
                if (string.IsNullOrEmpty(colName)) { colName = "Column" + i; }
                if (Column[colName] == null) {
                    var col = Column.GenerateAndAdd(colName);
                    if (col != null) { col.Caption = headerFields[i]; }
                }
            }
            startLine = 1;
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var line = lines[lineIndex];
            if (string.IsNullOrEmpty(line)) { continue; }

            var fields = CSVHelper.ParseLine(line, _separator);
            if (fields.Count == 0) { continue; }

            var rowKey = fields.Count > 0 ? fields[0] : Guid.NewGuid().ToString();
            var row = Row.GetByKey(rowKey) ?? Row.GenerateAndAdd(rowKey, "tbdb-CSV-Import");

            if (row == null) { continue; }

            var colIndex = 0;
            foreach (var col in Column) {
                if (col.IsDisposed) { continue; }
                if (colIndex < fields.Count) {
                    row.CellSet(col, fields[colIndex], "tbdb-CSV-Import");
                }
                colIndex++;
            }
        }

        return true;
    }

    private bool LoadFromTbdb() {
        _tbdbChunk = CachedFileSystem.GetOrCreate<Chunk>(Filename);

        if (_tbdbChunk == null) {
            Freeze("tbdb-Datei konnte nicht erstellt werden.");
            return false;
        }

        return ReloadFromTbdb();
    }

    private bool LoadSimpleCsv() {
        if (_dataFile == null) { return false; }

        if (!_dataFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        OnLoading();

        Undo.Clear();
        Row.RemoveNullOrEmpty();
        Cell.Clear();

        var colsToRemove = new List<ColumnItem>();
        foreach (var c in Column) {
            if (!c.IsDisposed) { colsToRemove.Add(c); }
        }
        foreach (var col in colsToRemove) {
            Column.Remove(col, "CSV-Neuladen");
        }

        var content = _dataFile.GetContentAsString(Encoding.UTF8);

        if (!ParseCsvContent(content)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        Column.GetSystems();
        RepairAfterParse();

        IsDirty = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);
        DropMessage(ErrorType.Info, $"CSV-Datei geladen: {KeyName}");
        return true;
    }

    private bool LoadSimpleCsvFile() {
        _dataFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);

        if (_dataFile == null) {
            Freeze("CachedTextFile konnte nicht erstellt werden.");
            return false;
        }

        return LoadSimpleCsv();
    }

    private bool ParseCsvContent(string content) {
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r").Trim('\r');
        var lines = content.Split('\r');

        if (lines.Length == 0) { return true; }

        var startLine = 0;

        if (_firstLineIsHeader) {
            var columnNames = CSVHelper.ParseLine(lines[0], _separator);
            startLine = 1;

            for (var i = 0; i < columnNames.Count; i++) {
                var colName = ColumnItem.MakeValidColumnName(columnNames[i]);
                if (string.IsNullOrEmpty(colName)) { colName = "Column" + i; }
                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                    if (col != null) { col.Caption = columnNames[i]; }
                }
            }
        } else {
            var firstLineFields = CSVHelper.ParseLine(lines[0], _separator);
            for (var i = 0; i < firstLineFields.Count; i++) {
                var colName = "Column" + i;
                if (Column[colName] == null) { Column.GenerateAndAdd(colName); }
            }
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var fields = CSVHelper.ParseLine(lines[lineIndex], _separator);
            if (fields.Count == 0) { continue; }

            var rowKey = fields.Count > 0 ? fields[0] : Guid.NewGuid().ToString();
            var row = Row.GenerateAndAdd(rowKey, "CSV-Import");
            if (row == null) { continue; }

            var colIndex = 0;
            foreach (var col in Column) {
                if (col.IsDisposed) { continue; }
                if (colIndex < fields.Count) { row.CellSet(col, fields[colIndex], "CSV-Import"); }
                colIndex++;
            }
        }

        return true;
    }

    private bool ParseTbdbHeader(string content) {
        var ini = new IniHelper();
        if (!ini.ParseContent(content)) { return false; }

        // Tabellen-Einstellungen
        var sepStr = ini.Get("Table", "Separator", ";");
        _separator = sepStr.Length > 0 ? sepStr[0] : ';';

        _firstLineIsHeader = ini.Get("Table", "FirstLineIsHeader", "+") == "+";

        if (int.TryParse(ini.Get("Table", "ChunkType", "0"), out var ct)) {
            _chunkType = (ChunkType)ct;
        }

        // Spalten anlegen
        foreach (var section in ini.GetSections()) {
            if (!section.StartsWith("Column_", StringComparison.OrdinalIgnoreCase)) { continue; }

            var colKey = section.Substring("Column_".Length);
            var col = Column[colKey] ?? Column.GenerateAndAdd(colKey);

            if (col == null) { continue; }

            var caption = ini.Get(section, "Caption", string.Empty);
            if (!string.IsNullOrEmpty(caption)) { col.Caption = caption; }
        }

        return true;
    }

    private bool ReloadChunkCsv(string chunkId, CachedTextFile csvFile) {
        // Zeilen dieses Chunks entfernen und neu laden
        var rowsToRemove = new List<RowItem>();
        foreach (var row in Row) {
            if (GetTextChunkId(row) == chunkId) { rowsToRemove.Add(row); }
        }
        foreach (var row in rowsToRemove) { RowCollection.Remove(row, "Chunk-Neuladen"); }

        LoadChunkCsvRows(csvFile);
        return true;
    }

    private bool ReloadFromTbdb() {
        if (_tbdbChunk == null) { return false; }

        if (!_tbdbChunk.EnsureContentLoaded()) {
            Freeze("tbdb-Datei konnte nicht geladen werden.");
            return false;
        }

        OnLoading();

        // Spalten und Daten zurücksetzen
        Undo.Clear();
        Row.RemoveNullOrEmpty();
        Cell.Clear();
        _chunkFiles.Clear();

        var colsToRemove = new List<ColumnItem>();
        foreach (var c in Column) {
            if (!c.IsDisposed) { colsToRemove.Add(c); }
        }
        foreach (var col in colsToRemove) { Column.Remove(col, "tbdb-Neuladen"); }

        // Header parsen (INI-Format, als UTF-8 im Chunk gespeichert)
        var headerBytes = _tbdbChunk.Content;
        var headerContent = headerBytes.Length > 0 ? Encoding.UTF8.GetString(headerBytes) : string.Empty;
        if (!ParseTbdbHeader(headerContent)) {
            Freeze("tbdb-Header parsen fehlgeschlagen!");
            return false;
        }

        // Alle CSV-Dateien im Verzeichnis einlesen
        var chunkFolder = ChunkFolder();

        if (DirectoryExists(chunkFolder)) {
            var csvFiles = CachedFileSystem.GetFileNames(chunkFolder, ["*.csv"]);
            if (csvFiles != null) {
                foreach (var csvFile in csvFiles) {
                    var chunkId = csvFile.FileNameWithoutSuffix().ToLowerInvariant();
                    var csvCachedFile = CachedFileSystem.GetOrCreate<CachedTextFile>(csvFile);
                    if (csvCachedFile != null) {
                        _chunkFiles[chunkId] = csvCachedFile;
                        LoadChunkCsvRows(csvCachedFile);
                    }
                }
            }
        }

        // Auch die gleichnamige .csv neben der .tbdb prüfen (Einzeldatei-Modus mit Header)
        var singleCsvPath = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".csv";
        if (FileExists(singleCsvPath)) {
            var singleCsv = CachedFileSystem.GetOrCreate<CachedTextFile>(singleCsvPath);
            if (singleCsv != null) {
                _chunkFiles["_single"] = singleCsv;
                LoadChunkCsvRows(singleCsv);
            }
        }

        Column.GetSystems();
        RepairAfterParse();

        IsDirty = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);
        DropMessage(ErrorType.Info, $"tbdb-Tabelle geladen: {KeyName}");
        return true;
    }

    private async Task<string> SaveSimpleCsv(DateTime setfileStateUtcDateTo) {
        DropMessage(ErrorType.DevelopInfo, $"Speichere CSV-Datei '{Caption}'");

        try {
            var csvContent = ExportCSV(_separator, _firstLineIsHeader);
            if (string.IsNullOrEmpty(csvContent)) {
                return "Fehler beim Generieren des CSV-Inhalts";
            }

            if (_dataFile == null) {
                _dataFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);
            }
            if (_dataFile == null) { return "CachedTextFile konnte nicht erstellt werden."; }

            _dataFile.Content = csvContent.UTF8_ToByte();
            var result = await _dataFile.Save().ConfigureAwait(false);

            if (result.IsFailed) { return result.FailedReason ?? "Speichern fehlgeschlagen"; }

            LastSaveMainFileUtcDate = setfileStateUtcDateTo;
            IsDirty = false;
            OnInvalidateView();
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    private async Task<string> SaveWithTbdb(DateTime setfileStateUtcDateTo) {
        DropMessage(ErrorType.DevelopInfo, $"Speichere tbdb-Tabelle '{Caption}'");

        try {
            // 1) tbdb Header speichern (INI-Format als UTF-8 im Chunk)
            var headerContent = BuildTbdbHeader();

            if (_tbdbChunk == null) {
                _tbdbChunk = CachedFileSystem.GetOrCreate<Chunk>(Filename);
            }
            if (_tbdbChunk == null) { return "tbdb-Datei konnte nicht erstellt werden."; }

            _tbdbChunk.Content = headerContent.UTF8_ToByte();
            var headerResult = await _tbdbChunk.Save().ConfigureAwait(false);
            if (headerResult.IsFailed) { return headerResult.FailedReason ?? "tbdb-Speichern fehlgeschlagen"; }

            // 2) CSV-Daten speichern
            if (_chunkType == ChunkType.None) {
                // Einzeldatei-Modus: tablename.csv neben der tbdb
                var singleCsvPath = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".csv";
                var csvContent = ExportCSV(_separator, _firstLineIsHeader);

                if (!_chunkFiles.TryGetValue("_single", out var singleCsv)) {
                    singleCsv = CachedFileSystem.GetOrCreate<CachedTextFile>(singleCsvPath);
                    if (singleCsv != null) { _chunkFiles["_single"] = singleCsv; }
                }

                if (singleCsv == null) { return "Single-CSV konnte nicht erstellt werden."; }

                singleCsv.Content = csvContent.UTF8_ToByte();
                var csvResult = await singleCsv.Save().ConfigureAwait(false);
                if (csvResult.IsFailed) { return csvResult.FailedReason ?? "CSV-Speichern fehlgeschlagen"; }
            } else {
                // Chunk-Modus: je Chunk eine CSV-Datei im Unterverzeichnis
                if (!CreateDirectory(ChunkFolder())) {
                    return "Chunk-Verzeichnis konnte nicht angelegt werden.";
                }

                var chunkData = BuildChunkCsvData();

                foreach (var kvp in chunkData) {
                    var chunkId = kvp.Key;
                    var chunkCsvContent = kvp.Value;
                    var chunkPath = ChunkFolder() + chunkId + ".csv";

                    if (!_chunkFiles.TryGetValue(chunkId, out var chunkFile)) {
                        chunkFile = CachedFileSystem.GetOrCreate<CachedTextFile>(chunkPath);
                        if (chunkFile != null) { _chunkFiles[chunkId] = chunkFile; }
                    }

                    if (chunkFile == null) { continue; }

                    chunkFile.Content = chunkCsvContent.UTF8_ToByte();
                    var chunkResult = await chunkFile.Save().ConfigureAwait(false);
                    if (chunkResult.IsFailed) { return chunkResult.FailedReason ?? $"Chunk {chunkId} Speichern fehlgeschlagen"; }
                }
            }

            LastSaveMainFileUtcDate = setfileStateUtcDateTo;
            IsDirty = false;
            OnInvalidateView();
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    #endregion
}