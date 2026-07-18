// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.ClassesStatic;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableCSV : TableFile {

    #region Fields

    private const string CsvSuffix = ".csv";
    private const string HbdbSuffix = ".hbdb";
    private FileInfo? _csvFileInfo;
    private Chunk? _headChunk;
    private bool _headDirty;
    private char _separator = ';';

    #endregion

    #region Constructors

    public TableCSV(string tablename) : base(tablename) { }

    public TableCSV(string filename, Table? source) : base(filename, source) {
        // Beim "Speichern unter" kopiert der Basiskonstruktor per CopyTo sämtliche
        // Head-Daten (Spalten-Metadaten, Variablen, Tags, …) in diese Instanz.
        // Daher muss die .hbdb-Begleitdatei beim ersten Speichern erzeugt werden.
        if (source is not null) { _headDirty = true; }
    }

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

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (IsDisposed) { return false; }
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (!FileExists(Filename)) { return false; }

        if (IsCsvStale()) {
            if (DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"CSV-Datei wurde geändert, lade neu: {KeyName}", 0); }
            return ReloadCSVFromDisk();
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

        if (!FileExists(Filename)) {
            Freeze("CSV-Datei existiert nicht.");
            return false;
        }

        var content = ReadAllText(Filename, Encoding.UTF8);
        _csvFileInfo = GetFileInfo(Filename);

        //Develop.Diagnose("UNDO",$"CSV Load Clear WAIT: T{Environment.CurrentManagedThreadId}");
        lock (_undoLock) {
            //Develop.Diagnose("UNDO",$"CSV Load Clear ENTER: Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
            Undo.Clear();
            //Develop.Diagnose("UNDO",$"CSV Load Clear DONE: T{Environment.CurrentManagedThreadId}");
        }
        Row.RemoveNullOrEmpty();

        // Head-Chunk VOR dem CSV-Content parsen, damit die Spalten-Metadaten
        // (insbesondere IsFirst -> Column.First) bereits gesetzt sind.
        // ParseCSVContent benötigt Column.First für Row.GenerateAndAdd,
        // sonst werden keine Zeilen angelegt.
        LoadHeadChunk();
        Column.GetSystems();

        var parsedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ParseCSVContent(content, parsedColumns, parsedRowKeys)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

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

            var writeResult = WriteAllBytes(Filename, bytes);
            if (writeResult.IsFailed) {
                return writeResult.FailedReason ?? "Speichern fehlgeschlagen";
            }

            _csvFileInfo = GetFileInfo(Filename);

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
                    _headChunk = LiveInstanceCacheHelper.GetLiveInstance<Chunk>(headFile) ?? new Chunk(headFile);
                }

                if (_headChunk is null) { return "Head-Chunk konnte nicht erstellt werden."; }

                var headResult = SaveExtended(headFile, headContent.ToArray().ZipIt() ?? []);

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

    /// <summary>
    /// Prüft, ob sich die CSV-Datei auf der Festplatte geändert hat,
    /// indem LastWriteTime und Length mit dem zuletzt bekannten Stand verglichen werden.
    /// </summary>
    private bool IsCsvStale() {
        if (_csvFileInfo is null) { return true; }

        var current = GetFileInfo(Filename, false, 0.1f);
        if (current is null) { return true; }

        try {
            return _csvFileInfo.Length != current.Length ||
                   _csvFileInfo.LastWriteTime != current.LastWriteTime;
        } catch {
            return true;
        }
    }

    private void LoadHeadChunk() {
        var headFile = HeadFile();

        _headChunk = LiveInstanceCacheHelper.GetLiveInstance<Chunk>(headFile);
        if (_headChunk is null) { return; }

        _ = _headChunk.LoadContent();

        if (_headChunk.LoadFailed) { return; }

        var data = _headChunk.LoadContent();
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
                var original = columnKeyes[i];
                var colName = FormatHolder_SystemName.MakeValid(original);
                if (!ColumnItem.IsValidColumnKey(colName)) {
                    colName = "Column" + i.ToString(CultureInfo.InvariantCulture);
                }

                parsedColumns.Add(colName);

                var col = Column[colName];
                if (col is null) {
                    // Direkt über ExecuteCommand mit NoUndo_NoInvalidate (= IgnoreFreeze),
                    // analog zum .bdb/.tblh-Parser in Table.cs. Während des Ladens wäre
                    // die IsValueEditable-Prüfung (Chunk-Lock, MultiUser) blockiert.
                    if (!string.IsNullOrEmpty(Column.ExecuteCommand(
                        TableDataType.Command_AddColumnByName, colName, Reason.NoUndo_NoInvalidate))) {
                        Develop.DebugError($"CSV-Spalte konnte nicht erzeugt werden: '{colName}'");
                    }
                    col = Column[colName];
                    if (col is not null && !col.IsSystemColumn()) { col.Caption = original; }
                }
            }
        } else {
            var firstLineFields = new List<string>(CsvHelper.ParseCSVLine(lines[0], _separator));
            for (var i = 0; i < firstLineFields.Count; i++) {
                var colName = "Column" + i.ToString(CultureInfo.InvariantCulture);
                parsedColumns.Add(colName);

                var col = Column[colName];
                if (col is null) {
                    if (!string.IsNullOrEmpty(Column.ExecuteCommand(
                        TableDataType.Command_AddColumnByName, colName, Reason.NoUndo_NoInvalidate))) {
                        Develop.DebugError($"CSV-Spalte konnte nicht erzeugt werden: '{colName}'");
                    }
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

    private bool ReloadCSVFromDisk() {
        if (IsDisposed) { return false; }

        if (!FileExists(Filename)) { return false; }

        var content = ReadAllText(Filename, Encoding.UTF8);
        _csvFileInfo = GetFileInfo(Filename);

        OnLoading();

        lock (_undoLock) {
            Undo.Clear();
        }
        Row.RemoveNullOrEmpty();
        Cell.Clear();

        // Head-Chunk VOR dem CSV-Content parsen, damit die Spalten-Metadaten
        // (insbesondere IsFirst -> Column.First) bereits gesetzt sind.
        // ParseCSVContent benötigt Column.First für Row.GenerateAndAdd,
        // sonst werden keine Zeilen angelegt.
        LoadHeadChunk();

        var parsedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ParseCSVContent(content, parsedColumns, parsedRowKeys)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        Row.RemoveObsoleteRows(Row, parsedRowKeys);
        Column.RemoveObsoleteColumns(Column, parsedColumns, Reason.NoUndo_NoInvalidate);

        Column.GetSystems();
        RepairAfterParse();

        SaveRequired = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);

        if (DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"CSV-Datei geladen: {KeyName}", 0); }
        return true;
    }

    #endregion
}