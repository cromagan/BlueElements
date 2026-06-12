// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Threading;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;
using static BlueBasics.ClassesStatic.Develop;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Dummy-Head (.cfbdb, 0 Bytes)
/// und Row-Chunks in eigenen Unterordnern mit Benutzer-spezifischen .chk-Dateien.
/// Jeder Benutzer schreibt in seine eigene Datei (Username_Zufallszeichen.chk).
/// Beim Laden wird immer nur die neueste Datei gelesen.
/// Dateien älter als ein Tag werden gelöscht (wenn mehrere vorhanden).
/// Edit-Sperre: Neueste Datei &lt;10 Min → nur der Ersteller darf bearbeiten.
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].cfbdb                                  (Dummy, 0 Bytes)
///   [TableName]\[ChunkID]\Username_Random.chk          (Row-Chunk pro Chunk)
/// </remarks>
[Browsable(false)]
[FileSuffix(".cfbdb")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunkFragments : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Stunden. Benutzer-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens eine neuere Datei im gleichen Ordner existiert).
    /// </summary>
    public const int DeleteOldFilesAfterHours = 24;

    /// <summary>
    /// Wert in Minuten. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public const int EditLockMinutes = 10;

    private readonly Dictionary<string, string> _currentRowChunkFile = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _lastKnownNewestTime = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _userChunkFiles = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public TableChunkFragments(string tablename) : base(tablename) { }

    public TableChunkFragments(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    public override bool MasterNeeded => false;

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => LastChange > LastSaveMainFileUtcDate;

    #endregion

    #region Methods

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        var chunkId = TableChunk.GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return string.Empty; }

        if (IsRowChunk(chunkId)) {
            return CheckEditLock(chunkId);
        }

        OnLoading();
        var result = LoadChunkWithChunkId(chunkId);
        if (result.IsFailed) { return result.FailedReason; }
        if (result.Value is true) { OnLoaded(false, true); }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) {
            return $"Interner Chunk-Fehler beim Schreibrecht anfordern {chunkId}";
        }

        return chunk.AcquireWriteAccess();
    }

    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (updateAllowed) {
            OnLoading();
            var result = LoadChunkWithChunkId(TableChunk.Chunk_Master);
            if (result.IsFailed) { return false; }
            if (result.Value is true) { OnLoaded(false, true); }
        }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    public override bool BeSureRowIsLoaded(string chunkValue) {
        if (!base.BeSureRowIsLoaded(chunkValue)) { return false; }

        var chunkValues = chunkValue.SplitAndCutByCr().SortedDistinctList();

        var loaded = false;
        OnLoading();

        foreach (var thisvalue in chunkValues) {
            var chunkId = TableChunk.GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, thisvalue);
            var result = LoadChunkWithChunkId(chunkId);
            if (result.IsFailed) { return false; }
            loaded = loaded || result.Value is true;
        }

        if (loaded) { OnLoaded(false, false); }

        return true;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

        if (firstTime) {
            if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }
        }

        var loaded = false;
        OnLoading();

        if (!firstTime) {
            var result = LoadChunkWithChunkId(Chunk_MainData);
            if (result.IsFailed) { return false; }
            loaded = result.Value is true;
        }

        Column.GetSystems();

        List<string> list = [TableChunk.Chunk_AdditionalUseCases, TableChunk.Chunk_Master, TableChunk.Chunk_Variables, TableChunk.Chunk_UnknownData];

        foreach (var item in list) {
            if (!firstTime && !Chunk.IsChunkRecentlyUsed(Filename, item)) { continue; }
            var result = LoadChunkWithChunkId(item);
            loaded = loaded || result.Value is true;
        }

        RefreshLoadedRowChunks(ref loaded, firstTime);

        CleanupOldRowChunkFiles();

        if (loaded) { OnLoaded(firstTime, true); }

        TryToSetMeTemporaryMaster();

        return true;
    }

    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        string[] checkIds = [Chunk_MainData,
            TableChunk.Chunk_Master,
            TableChunk.Chunk_Variables,
            TableChunk.Chunk_AdditionalUseCases];

        foreach (var id in checkIds) {
            var loadResult = LoadChunkWithChunkId(id);
            if (loadResult.IsFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}' ({loadResult.FailedReason})"; }
        }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        if (InitialSavePending) { return string.Empty; }

        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var chunkId = TableChunk.GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        if (IsRowChunk(chunkId)) { return CheckEditLock(chunkId); }

        OnLoading();
        var result = LoadChunkWithChunkId(chunkId);

        if (result.IsFailed) { return result.FailedReason; }

        if (result.Value is true) { OnLoaded(false, true); }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) {
            return $"Interner Chunk-Fehler bei Editier-Prüfung {chunkId}";
        }
        return chunk.IsNowEditable();
    }

    public override bool LoadTableRows(bool oldest, int count) {
        if (!base.LoadTableRows(oldest, count)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkFolder = ChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return true; }

        var subDirs = System.IO.Directory.EnumerateDirectories(chunkFolder);
        var chunkIds = new List<string>();

        foreach (var subDir in subDirs) {
            var chunkId = subDir.FileNameWithoutSuffix().ToLowerInvariant();
            if (IsRowChunk(chunkId)) { chunkIds.Add(chunkId); }
        }

        if (count >= 0) {
            var rnd = new Random();
            chunkIds = [.. chunkIds.OrderBy(_ => rnd.Next()).Take(count)];
        }

        var loaded = false;
        var ok = true;
        OnLoading();

        foreach (var chunkId in chunkIds) {
            var result = LoadRowChunkFromFolder(chunkId);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(false, true); }

        return ok;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            UnMasterMe();
        }

        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }

        return LoadChunkWithChunkId(Chunk_MainData).IsSuccessful;
    }

    /// <summary>
    /// Speichert System-Chunks normal und Row-Chunks in Benutzer-spezifische Dateien.
    /// </summary>
    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        var chunks = TableChunk.GenerateNewChunks(this, 0, setfileStateUtcDateTo, true, true, true);
        if (chunks is null) {
            return "Chunks konnten nicht generiert werden";
        }

        foreach (var c in chunks) {
            if (!IsRowChunk(c.KeyName)) {
                var result = c.Save().GetAwaiter().GetResult();
                if (result.IsFailed) { return result.FailedReason; }
            } else {
                SaveRowChunkToUserFile(c);
            }
        }

        CleanupOldRowChunkFiles();

        LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        InitialSavePending = false;
        OnInvalidateView();
        return string.Empty;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen (Format: Username_Random.chk).
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        return idx > 0 ? fileName[..idx] : fileName;
    }

    /// <summary>
    /// Listet alle .chk-Dateien im Ordner auf, sortiert nach Änderungsdatum
    /// (neueste zuerst).
    /// </summary>
    private static List<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return [.. System.IO.Directory.EnumerateFiles(folder, "*.chk")
                .OrderByDescending(f => System.IO.File.GetLastWriteTimeUtc(f))];
        } catch {
            return [];
        }
    }

    /// <summary>
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer erstellt wurde.
    /// </summary>
    private static bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        return string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Prüft die Edit-Sperre für einen Row-Chunk: Wenn die neueste Datei im Ordner
    /// jünger als EditLockMinutes ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    private string CheckEditLock(string chunkId) {
        var folder = GetRowChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        var files = GetChunkFilesOrderedByTime(folder);
        if (files.Count == 0) { return string.Empty; }

        var newestFile = files[0];
        var lastWrite = System.IO.File.GetLastWriteTimeUtc(newestFile);

        if (DateTime.UtcNow.Subtract(lastWrite).TotalMinutes >= EditLockMinutes) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        return $"Chunk '{chunkId}' wird seit {DateTime.UtcNow.Subtract(lastWrite).TotalMinutes:0} Minuten von '{creator}' bearbeitet";
    }

    private string ChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Löscht alle .chk-Dateien im angegebenen Ordner, die älter als
    /// DeleteOldFilesAfterHours Stunden sind, sofern mindestens eine neuere
    /// Datei vorhanden ist.
    /// </summary>
    private void CleanupOldFilesInFolder(string folder) {
        var files = GetChunkFilesOrderedByTime(folder);
        if (files.Count <= 1) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        for (var i = files.Count - 1; i >= 1; i--) {
            var file = files[i];
            var lastWrite = System.IO.File.GetLastWriteTimeUtc(file);
            if (lastWrite < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Löscht Benutzer-Dateien in allen Row-Chunk-Ordnern, die älter als
    /// DeleteOldFilesAfterHours Stunden sind und für die mindestens eine
    /// neuere Datei im selben Ordner existiert.
    /// </summary>
    private void CleanupOldRowChunkFiles() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        var chunkFolder = ChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return; }

        try {
            foreach (var subDir in System.IO.Directory.EnumerateDirectories(chunkFolder)) {
                var chunkId = subDir.FileNameWithoutSuffix().ToLowerInvariant();
                if (!IsRowChunk(chunkId)) { continue; }

                CleanupOldFilesInFolder(subDir.TrimEnd('\\') + "\\");
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fehler beim Aufräumen alter Chunk-Dateien: {ex.Message}");
        }
    }

    /// <summary>
    /// Stellt sicher, dass die Dummy .cfbdb-Datei existiert (0 Bytes).
    /// </summary>
    private void EnsureDummyFileExists() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        if (!IO.FileExists(Filename)) {
            IO.WriteAllBytes(Filename, []);
        }
    }

    /// <summary>
    /// Berechnet den Ordnerpfad für einen Row-Chunk.
    /// </summary>
    private string GetRowChunkFolder(string chunkId) {
        return $"{ChunkFolder()}{chunkId.ToLowerInvariant()}\\";
    }

    private OperationResult LoadChunkWithChunkId(string chunkId) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        if (!IsRowChunk(chunkId)) {
            return LoadSystemChunk(chunkId);
        }

        return LoadRowChunkFromFolder(chunkId);
    }

    /// <summary>
    /// Lädt den neuesten Row-Chunk aus dem Chunk-Ordner.
    /// Wenn sich die neueste Datei geändert hat, wird der Chunk neu geladen und geparst.
    /// </summary>
    private OperationResult LoadRowChunkFromFolder(string chunkId) {
        var folder = GetRowChunkFolder(chunkId);

        if (!IO.DirectoryExists(folder)) {
            if (IO.CreateDirectory(folder).IsFailed) {
                return OperationResult.Failed("Ordner konnte nicht erstellt werden");
            }
            return OperationResult.SuccessValue(false);
        }

        var files = GetChunkFilesOrderedByTime(folder);

        if (files.Count == 0) {
            return OperationResult.SuccessValue(false);
        }

        var newestFile = files[0];
        var newestTime = System.IO.File.GetLastWriteTimeUtc(newestFile);

        if (_currentRowChunkFile.TryGetValue(chunkId, out var prevFile) &&
            string.Equals(prevFile, newestFile, StringComparison.OrdinalIgnoreCase) &&
            _lastKnownNewestTime.TryGetValue(chunkId, out var prevTime) &&
            newestTime <= prevTime) {
            return OperationResult.SuccessValue(false);
        }

        var chunk = CachedFileSystem.Get<Chunk>(newestFile);
        if (chunk is null || chunk.LoadFailed) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht geladen werden");
        }

        chunk.LastUsed = DateTime.UtcNow;

        if (chunk.IsStale() || chunk.NeedsLoading()) {
            chunk.Invalidate();
            if (!chunk.EnsureContentLoaded()) {
                return OperationResult.Failed($"Row-Chunk '{chunkId}' Inhalt konnte nicht geladen werden");
            }
        }

        if (!ParseChunk(chunk)) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' Parsen fehlgeschlagen");
        }

        _currentRowChunkFile[chunkId] = newestFile;
        _lastKnownNewestTime[chunkId] = newestTime;

        return OperationResult.SuccessValue(true);
    }

    /// <summary>
    /// Lädt einen System-Chunk (MainData, Master, Variables, Uses, UnknownData)
    /// aus dem CachedFileSystem. Enthält Recovery, Stale-Check und Parsing.
    /// </summary>
    private OperationResult LoadSystemChunk(string chunkId) {
        chunkId = chunkId.ToLowerInvariant();

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));

        chunk?.LastUsed = DateTime.UtcNow;

        if (chunk is not null && chunk.IsSaving) {
            if (!WaitChunkIsSaved(chunkId)) {
                return OperationResult.Failed($"Timeout beim Warten auf Speicherung von {chunkId}");
            }
        }

        if (chunk is null) {
            Diagnose("CF", $"Get<Chunk> war null für {chunkId} bei {Filename.FileNameWithSuffix()}");

            chunk = new Chunk(Filename, chunkId);
            chunk.LastUsed = DateTime.UtcNow;

            var inCache = CachedFileSystem.FileExists(chunk.Filename);
            var onDisk = !inCache && CachedFileSystem.FileExists(chunk.Filename, true);
            if (!inCache && !onDisk) {
                var recovered = TryRecoverFromBackup(chunk.Filename, chunkId, 10000);

                if (!recovered) {
                    if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
                        Diagnose("CF", $"Hauptchunk fehlt: {Filename.FileNameWithSuffix()} inCache={inCache} onDisk={onDisk}");
                        Freeze($"Hauptchunk fehlt auf der Festplatte und kein gültiges Backup vorhanden");
                        return OperationResult.Failed("Hauptchunk fehlt, keine Wiederherstellung möglich");
                    }

                    Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Erstelle neuen Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'", 0);
                    chunk.AcquireWriteAccess();
                    chunk.EnsureContentLoaded();
                    var head = chunk.GetHeadBytes();
                    SaveToByteList(head, TableDataType.EOF, "END");
                    chunk.Content = head.ToArray();
                    _ = chunk.Save().GetAwaiter().GetResult();
                    return OperationResult.SuccessValue(false);
                }
            }
        }

        var needLoading = chunk.LoadFailed || chunk.IsStale();

        var loaded = false;

        if (needLoading) {
            chunk.WaitDiskOperationFinished();

            if (chunk.IsDisposed) {
                return OperationResult.Failed($"Chunk {chunkId} wurde während des Wartens verworfen");
            }

            if (!chunk.IsLoading) {
                if (!chunk.IsSaved) {
                    return OperationResult.SuccessValue(false);
                }
                chunk.Invalidate();
            }

            if (!chunk.EnsureContentLoaded()) {
                return OperationResult.Failed("Chunk Laden fehlgeschlagen");
            }

            if (!ParseChunk(chunk)) {
                return OperationResult.Failed("Parsen fehlgeschlagen");
            }

            loaded = true;
        }

        return OperationResult.SuccessValue(loaded);
    }

    private bool ParseChunk(Chunk chunk) {
        if (chunk.LoadFailed) { return false; }

        var chunkContent = chunk.Content;
        if (chunkContent.Length == 0) { return true; }

        lock (_undoLock) {
            Undo.RemoveAll(item => item is not null
                && string.Equals(TableChunk.GetChunkId(this, item.Command, item.RowKey is { Length: > 0 } rk ? Row.GetByKey(rk)?.ChunkValue ?? string.Empty : string.Empty), chunk.KeyName, StringComparison.OrdinalIgnoreCase));
        }

        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseSuccessful = Parse(chunkContent, chunk.IsMain, parsedRowKeys);

        if (!parseSuccessful) {
            chunk.MarkLoadFailed();
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            return false;
        }

        Row.RemoveObsoleteRows(RowsOfChunk(chunk), parsedRowKeys);

        return true;
    }

    /// <summary>
    /// Aktualisiert alle geladenen Row-Chunks: Prüft für jeden, ob eine neuere
    /// Datei im Ordner existiert und lädt diese ggf. neu.
    /// Chunks, die länger als <see cref="Chunk.SkipIfUnusedMinutes"/> nicht verwendet wurden,
    /// werden übersprungen, sofern <paramref name="firstTime"/> false ist.
    /// </summary>
    private void RefreshLoadedRowChunks(ref bool loaded, bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return; }
        var chunkFolder = ChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return; }

        foreach (var kvp in _currentRowChunkFile.ToList()) {
            var chunkId = kvp.Key;
            var folder = GetRowChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            if (!firstTime && !Chunk.IsChunkRecentlyUsed(Filename, chunkId)) { continue; }

            var files = GetChunkFilesOrderedByTime(folder);
            if (files.Count == 0) { continue; }

            var newestFile = files[0];

            if (!string.Equals(kvp.Value, newestFile, StringComparison.OrdinalIgnoreCase)) {
                var result = LoadRowChunkFromFolder(chunkId);
                if (result.IsFailed) {
                    Develop.DebugPrint(ErrorType.Warning, $"Row-Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
                }
                loaded = loaded || result.Value is true;
            }
        }
    }

    private List<RowItem> RowsOfChunk(Chunk chunk) =>
            [.. Row.Where(r => ReferenceEquals(r.Table, this) && TableChunk.GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, r.ChunkValue) == chunk.KeyName)];

    /// <summary>
    /// Speichert einen Row-Chunk in eine Benutzer-spezifische .chk-Datei.
    /// Vorherige Benutzer-Dateien dieses Chunks werden gelöscht.
    /// </summary>
    private void SaveRowChunkToUserFile(Chunk chunk) {
        var chunkId = chunk.KeyName;
        var folder = GetRowChunkFolder(chunkId);

        if (IO.CreateDirectory(folder).IsFailed) { return; }

        if (_userChunkFiles.TryGetValue(chunkId, out var prevFile) && IO.FileExists(prevFile)) {
            IO.DeleteFile(prevFile, false);
            _userChunkFiles.Remove(chunkId);
        }

        var newFileName = $"{UserName}_{GetUniqueKey()}.chk";
        var newPath = folder + newFileName;

        chunk.EnsureContentLoaded();
        var content = chunk.Content;
        if (content is { Length: > 0 }) {
            var zipped = content.ZipIt();
            var result = IO.WriteAllBytes(newPath, zipped);
            if (result.IsFailed) {
                Develop.DebugPrint(ErrorType.Warning, $"Row-Chunk '{chunkId}' konnte nicht gespeichert werden: {result.FailedReason}");
                return;
            }
        } else {
            var result = IO.WriteAllBytes(newPath, []);
            if (result.IsFailed) { return; }
        }

        _userChunkFiles[chunkId] = newPath;
        _currentRowChunkFile[chunkId] = newPath;
        _lastKnownNewestTime[chunkId] = DateTime.UtcNow;
    }

    private bool WaitChunkIsSaved(string chunkid) {
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkid));
        if (chunk is null) { return true; }

        for (var i = 0; i < 1200; i++) {
            if (!chunk.IsSaving) { return true; }
            Thread.Sleep(100);
        }

        Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {chunkid} wurde nicht richtig gespeichert", 0);
        return false;
    }

    #endregion
}