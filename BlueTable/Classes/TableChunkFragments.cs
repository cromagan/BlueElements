// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Globalization;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Dummy-Head (.cfbdb, 0 Bytes)
/// und Row-Chunks in eigenen Unterordnern mit Benutzer-spezifischen .chk-Dateien.
/// Jeder Benutzer schreibt in seine eigene Datei (yyyy-MM-dd-HH-mm-ss_Username.chk).
/// Das Datum im Dateinamen (UTC) ist der alleinige Vergleich für Aktualität.
/// Dateien älter als eine Stunde werden gelöscht (wenn mehrere vorhanden).
/// Edit-Sperre: Neueste Datei &lt;10 Min → nur der Ersteller darf bearbeiten.
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].cfbdb                                                (Dummy, 0 Bytes)
///   [TableName]\[ChunkID]\yyyy-MM-dd-HH-mm-ss_Username.chk          (Row-Chunk pro Chunk)
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
    public const int DeleteOldFilesAfterHours = 1;

    /// <summary>
    /// Wert in Minuten. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public const int EditLockMinutes = 10;

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 3;

    private readonly Dictionary<string, Chunk> _currentChunk = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dirtyChunks = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public TableChunkFragments(string tablename) : base(tablename) { }

    public TableChunkFragments(string filename, Table? source) : base(filename, source) {
        _dirtyChunks.Add(Chunk_MainData);
    }

    #endregion

    #region Properties

    public override bool MasterNeeded => false;

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => _dirtyChunks.Count > 0;

    #endregion

    #region Methods

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        var chunkId = TableChunk.GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        OnLoading();
        var result = LoadChunkWithChunkId(chunkId);
        if (result.IsFailed) { return result.FailedReason; }
        if (result.Value is true) { OnLoaded(false, true); }

        var lockResult = CheckEditLock(chunkId);
        if (string.IsNullOrEmpty(lockResult)) {
            _dirtyChunks.Add(chunkId);
        }
        return lockResult;
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
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

        if (firstTime) {
            if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }
        }

        var loaded = false;
        var ok = true;

        OnLoading();

        if (!firstTime) {
            var result = LoadChunkWithChunkId(Chunk_MainData);
            if (result.IsFailed) { return false; }
            loaded = result.Value is true;
        }

        Column.GetSystems();

        List<string> list = [TableChunk.Chunk_AdditionalUseCases, TableChunk.Chunk_Master, TableChunk.Chunk_Variables, TableChunk.Chunk_UnknownData];

        foreach (var item in list) {
            if (!firstTime && !Chunk.IsChunkRecentlyUsed(ComputeChunkPath(Filename, item))) { continue; }
            var result = LoadChunkWithChunkId(item);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        loaded = loaded || RefreshLoadedChunks(firstTime);

        if (loaded) { OnLoaded(firstTime, true); }

        if (ok) { TryToSetMeTemporaryMaster(); }

        return ok;
    }

    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        string[] checkIds = [Chunk_MainData, TableChunk.Chunk_Master, TableChunk.Chunk_Variables, TableChunk.Chunk_AdditionalUseCases];

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

        return CheckEditLock(chunkId);
    }

    public override bool LoadTableRows(bool oldest, int count) {
        if (!base.LoadTableRows(oldest, count)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return true; }

        var subDirs = IO.GetDirectories(chunkFolder);
        var chunkIds = new List<string>();

        foreach (var subDir in subDirs) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (IsRowChunk(chunkId)) { chunkIds.Add(chunkId); }
        }

        if (count >= 0) {
            if (oldest) {
                chunkIds = [.. chunkIds.OrderBy(id => {
                    var dir = GetChunkFolder(id);
                    if (!IO.DirectoryExists(dir)) { return DateTime.MaxValue; }
                    var newestInDir = GetChunkFilesOrderedByTime(dir).FirstOrDefault();
                    return newestInDir is not null ? ExtractDateFromFileName(newestInDir) ?? DateTime.MaxValue : DateTime.MaxValue;
                }).Take(count)];
            } else {
                chunkIds = [.. chunkIds.OrderBy(_ => Constants.GlobalRnd.Next()).Take(count)];
            }
        }

        var loaded = false;
        var ok = true;
        OnLoading();

        foreach (var chunkId in chunkIds) {
            var result = LoadChunkWithChunkId(chunkId);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(false, true); }

        return ok;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            if (_dirtyChunks.Count > 0 && !IsFreezed) {
                _ = SaveInternal(DateTime.UtcNow);
            }
            UnMasterMe();
        }

        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        return LoadChunkWithChunkId(Chunk_MainData).IsSuccessful;
    }

    /// <summary>
    /// Speichert System-Chunks normal und Row-Chunks in Benutzer-spezifische Dateien.
    /// </summary>
    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere Chunks der Tabelle '{Caption}'", 2);

        var chunks = TableChunk.GenerateNewChunks(this, 0, setfileStateUtcDateTo, true, true);
        if (chunks is null) {
            return "Chunks konnten nicht generiert werden";
        }

        // Inhalt sofort extrahieren, dann ALLE Chunks invalidieren.
        // Verhindert, dass StaleCheckCallback die temporären .bdbc-Chunks auto-speichert.
        var chunksToSave = new List<(string chunkId, byte[] content)>();

        foreach (var c in chunks) {
            if (_dirtyChunks.Contains(c.KeyName)) {
                c.EnsureContentLoaded();
                chunksToSave.Add((c.KeyName, c.Content));
            }
            c.Invalidate();
        }

        foreach (var (chunkId, content) in chunksToSave) {
            SaveChunkContent(chunkId, content);
        }

        _dirtyChunks.Clear();
        LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        InitialSavePending = false;
        OnInvalidateView();
        return string.Empty;
    }

    /// <summary>
    /// Löscht alle .chk-Dateien im angegebenen Ordner, die älter als
    /// DeleteOldFilesAfterHours Stunden sind, sofern mindestens eine neuere Datei und zwei Backups vorhanden sind.
    /// </summary>
    private static void CleanupOldFilesInFolder(string folder) {
        var files = GetChunkFilesOrderedByTime(folder).ToArray();
        if (files.Length <= FileCount) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        for (var i = files.Length - 1; i >= FileCount; i--) {
            var file = files[i];
            var fileDate = ExtractDateFromFileName(file);
            if (fileDate.HasValue && fileDate.Value < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Chunk-Dateinamen (Format: yyyy-MM-dd-HH-mm-ss_Username.chk).
    /// Gibt null zurück, wenn das Datum nicht geparst werden kann.
    /// </summary>
    private static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 20) { return null; }
        var datePart = fileName[..19];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen (Format: yyyy-MM-dd-HH-mm-ss_Username.chk).
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        return idx > 0 ? fileName[(idx + 1)..] : fileName;
    }

    /// <summary>
    /// Listet alle .chk-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
    /// </summary>
    private static IEnumerable<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return CachedFileSystem.GetFileNames(folder, ["*.chk"]).OrderByDescending(f => f);
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
    /// C:\xxx\[Tablebame]\
    /// </summary>
    /// <returns></returns>
    private string BaseChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Prüft die Edit-Sperre für einen Chunk: Wenn die neueste Datei im Ordner
    /// jünger als EditLockMinutes ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    private string CheckEditLock(string chunkId) {
        var folder = GetChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return string.Empty; }

        var fileDate = ExtractDateFromFileName(newestFile);
        if (!fileDate.HasValue) { return "Datums-Fehler in Dateien"; }

        if (DateTime.UtcNow.Subtract(fileDate.Value).TotalMinutes >= EditLockMinutes) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        return $"Chunk '{chunkId}' wird seit {DateTime.UtcNow.Subtract(fileDate.Value).TotalMinutes:0} Minuten von '{creator}' bearbeitet";
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
    /// C:\xxx\[Tablebame]\[Chunk]\
    /// </summary>
    private string GetChunkFolder(string chunkId) => $"{BaseChunkFolder()}{chunkId.ToLowerInvariant()}\\";

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkId"></param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    private OperationResult LoadChunkWithChunkId(string chunkId) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        if (_currentChunk.TryGetValue(chunkId, out var existingChunk) && existingChunk is not null && !existingChunk.IsDisposed) {
            // Verwaiste .bdbc-Chunks (von GenerateNewChunks) erkennen und entfernen
            if (!string.Equals(existingChunk.Filename.FileSuffix(), "chk", StringComparison.OrdinalIgnoreCase)) {
                _currentChunk.Remove(chunkId);
            } else {
                existingChunk.LastUsed = DateTime.UtcNow;
                return OperationResult.SuccessFalse;
            }
        }

        var folder = GetChunkFolder(chunkId);

        if (!IO.DirectoryExists(folder)) {
            if (IO.CreateDirectory(folder).IsFailed) {
                return OperationResult.Failed("Ordner konnte nicht erstellt werden");
            }
            return OperationResult.SuccessFalse;
        }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) {
            return OperationResult.SuccessFalse;
        }

        var chunk = CachedFileSystem.Get<Chunk>(newestFile);
        if (chunk is null || chunk.LoadFailed) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht geladen werden");
        }

        chunk.LastUsed = DateTime.UtcNow;

        if (!chunk.NeedsLoading() && !chunk.IsStale()) {
            _currentChunk[chunkId] = chunk;
            return OperationResult.SuccessValue(false);
        }

        if (!chunk.EnsureContentLoaded()) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' Inhalt konnte nicht geladen werden");
        }

        if (!ParseChunk(chunk)) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' Parsen fehlgeschlagen");
        }

        _currentChunk[chunkId] = chunk;

        CleanupOldFilesInFolder(folder);

        return OperationResult.SuccessValue(true);
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
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }
        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return false; }

        var loaded = false;

        foreach (var kvp in _currentChunk.ToList()) {
            var chunkId = kvp.Key;
            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            if (!firstTime && _currentChunk.TryGetValue(chunkId, out var cached) && cached is not null && !cached.IsDisposed && DateTime.UtcNow.Subtract(cached.LastUsed).TotalMinutes >= Chunk.SkipIfUnusedMinutes) { continue; }

            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var currentChunk = kvp.Value;
            if (currentChunk is null || currentChunk.IsDisposed) { continue; }

            if (!string.Equals(currentChunk.Filename, newestFile, StringComparison.OrdinalIgnoreCase)) {
                _currentChunk.Remove(chunkId);
                var result = LoadChunkWithChunkId(chunkId);
                if (result.IsFailed) {
                    Develop.DebugPrint(ErrorType.Warning, $"Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
                }
                loaded = loaded || result.Value is true;
            }
        }

        return loaded;
    }

    private List<RowItem> RowsOfChunk(Chunk chunk) =>
            [.. Row.Where(r => ReferenceEquals(r.Table, this) && TableChunk.GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, r.ChunkValue) == chunk.KeyName)];

    /// <summary>
    /// Speichert einen Row-Chunk in eine Benutzer-spezifische .chk-Datei.
    /// Vorherige Benutzer-Dateien dieses Chunks werden gelöscht.
    /// Dateiname: yyyy-MM-dd-HH-mm-ss_Username.chk (UTC).
    /// </summary>
    private void SaveChunkContent(string chunkId, byte[] content) {
        var folder = GetChunkFolder(chunkId);

        if (IO.CreateDirectory(folder).IsFailed) { return; }

        var newFileName = $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}_{UserName}.chk";
        var newPath = folder + newFileName;

        CachedFileSystem.BeginIgnoreFile(newPath);
        try {
            if (content is { Length: > 0 } && content.ZipIt() is { Length: > 0 } zipped) {
                var result = IO.WriteAllBytes(newPath, zipped);
                if (result.IsFailed) {
                    Develop.DebugPrint(ErrorType.Warning, $"Row-Chunk '{chunkId}' konnte nicht gespeichert werden: {result.FailedReason}");
                    return;
                }
            } else {
                var result = IO.WriteAllBytes(newPath, []);
                if (result.IsFailed) { return; }
            }
        } finally {
            CachedFileSystem.EndIgnoreFile(newPath);
        }

        var reloadedChunk = CachedFileSystem.Get<Chunk>(newPath);
        if (reloadedChunk is not null) {
            _currentChunk[chunkId] = reloadedChunk;
        } else {
            Develop.DebugPrint(ErrorType.Warning, $"Row-Chunk '{chunkId}' konnte nach dem Speichern nicht neu geladen werden");
        }

        CleanupOldFilesInFolder(folder);
    }

    #endregion
}