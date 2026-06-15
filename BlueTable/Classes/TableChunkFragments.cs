// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Globalization;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Lite-Hauptdatei (.cfbdb, Head + CheckPoint + EOF, keine Nutzdaten)
/// und Row-Chunks in eigenen Unterordnern mit Benutzer-spezifischen .chk-Dateien.
/// Jeder Benutzer schreibt in seine eigene Datei (yyyy-MM-dd-HH-mm-ss_Username-Hash.chk),
/// wobei Hash ein 3-stelliger Wert aus MachineName und Instanz-ID ist. So werden auch
/// verschiedene Maschinen/Instanzen bei gleichem Benutzernamen unterschieden.
/// Das Datum im Dateinamen (UTC) ist der alleinige Vergleich für Aktualität.
/// Dateien älter als eine Stunde werden gelöscht (wenn mehrere vorhanden).
/// Edit-Sperre: Neueste Datei &lt;10 Min → nur der Ersteller darf bearbeiten.
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].cfbdb                                                       (Lite-Chunk, keine Nutzdaten)
///   [TableName]\[ChunkID]\yyyy-MM-dd-HH-mm-ss_Username-Hash.chk            (Row-Chunk pro Chunk)
/// </remarks>
[Browsable(false)]
[FileSuffix(".cfbdb")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunkFragments : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Stunden. Benutzer-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens FileCount neuere Dateien im gleichen Ordner existieren).
    /// </summary>
    public const int DeleteOldFilesAfterHours = 1;

    /// <summary>
    /// Wert in Minuten. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public const int EditLockMinutes = 10;

    /// <summary>
    /// Chunk-ID für die Lite-Hauptdatei (.cfbdb). Enthält nur Head + CheckPoint + EOF,
    /// keine Nutzdaten. Die eigentlichen Daten liegen in den Row-Chunks (.chk).
    /// </summary>
    public static readonly string Chunk_MainDataLite = "_MainDataLite";

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 3;

    /// <summary>
    /// Chunkid / Chunk.
    /// Azfgrund der Dateinamensverschiebung muss sich der aktuelle Chunk gemerkt werden.
    /// </summary>
    private readonly Dictionary<string, Chunk> _currentChunk = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public TableChunkFragments(string tablename) : base(tablename) { }

    public TableChunkFragments(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    public override bool MultiUserPossible => true;

    /// <summary>
    /// Generiert einen 3-zeiligen Hash aus MachineName und Instanz-ID (MyId).
    /// Unterscheidet verschiedene Maschinen/Instanzen bei gleichem Benutzernamen
    /// auf dem Dateisystem.
    /// </summary>
    private static string MachineInstanceHash => $"{Environment.MachineName}|{MyId}".GetMD5Hash()[..3].ToUpperInvariant();

    #endregion

    #region Methods

    /// <summary>
    /// Generiert den Content für die Lite-Hauptdatei (.cfbdb). Enthält ausschließlich
    /// den CheckPoint, keine Nutzdaten — die eigentlichen Daten liegen in den Row-Chunks.
    /// Head und EOF werden von SaveChunkFiles ergänzt.
    /// </summary>
    public static List<byte> GenerateMainLiteChunk() {
        var result = new List<byte>();

        SaveToByteList(result, TableDataType.CheckPoint, $"~^{Chunk_MainDataLite.ToLowerInvariant()}^~");

        return result;
    }

    /// <summary>
    /// Lazy-Strategie: Der Chunk wird beim Anfordern des Schreibrechts NICHT sofort
    /// durch eine eigene .chk-Datei beansprucht. Es wird nur geprüft, ob ein anderer
    /// Benutzer den Chunk in den letzten <see cref="EditLockMinutes"/> Minuten
    /// reserviert hat. Der tatsächliche Claim (die neue .chk-Datei) entsteht erst in
    /// <see cref="SaveInternal"/>, sobald eine echte Änderung geschrieben wird.
    /// <para>
    /// Vorteil: Auf langsamen Netzwerken entfällt das sofortige Schreiben einer
    /// Claim-Datei — die nur lesende Sperren-Prüfung ist deutlich schneller.
    /// </para>
    /// <para>
    /// Risiko: Zwischen dieser Prüfung und dem späteren Speichern kann ein anderer
    /// Benutzer den Chunk für sich beanspruchen. <see cref="SaveInternal"/> verifiziert
    /// die Sperre daher unmittelbar vor dem Schreiben erneut und schlägt ggf. fehl.
    /// </para>
    /// </summary>
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

        // SaveRequired wird bewusst NICHT gesetzt — der Claim erfolgt erst bei einer
        // echten Änderung über WriteValueToDiscOrServer → SaveRequired → SaveInternal.
        return CheckEditLock(chunkId);
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
            if (!firstTime && (!_currentChunk.TryGetValue(item, out var cached) || cached is null || cached.IsDisposed || DateTime.UtcNow.Subtract(cached.LastUsed).TotalMinutes >= SkipIfUnusedMinutes)) { continue; }
            var result = LoadChunkWithChunkId(item);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        loaded = loaded || RefreshLoadedChunks(firstTime);

        if (loaded) { OnLoaded(firstTime, true); }

        if (ok) { TryToSetMeTemporaryMaster(); }

        return ok;
    }

    /// <summary>
    /// Prüft, ob der Row-Chunk für den angegebenen Chunk-Wert aktuell im Speicher geladen ist.
    /// Wird für verlinkte Zellen benötigt, um sicherzustellen, dass die Zielzeile verfügbar ist.
    /// </summary>
    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = TableChunk.GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        return _currentChunk.TryGetValue(chunkId, out var chunk)
            && chunk is not null
            && !chunk.IsDisposed
            && !chunk.LoadFailed;
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

    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        var result = LoadChunkWithChunkId(Chunk_MainData);
        if (result.IsFailed) { return false; }

        // Bei einer geladenen Tabelle muss der Hauptchunk vorhanden sein.
        // Fehlt er (Ordner leer), ist die Tabelle korrupt.
        if (!_currentChunk.ContainsKey(Chunk_MainData.ToLowerInvariant())) {
            Freeze("Hauptchunk der Tabelle fehlt");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Speichert alle Chunks (System und Row) über SaveChunkFiles in Benutzer-spezifische .chk-Dateien.
    /// </summary>
    protected override string SaveInternal() {
        if (!SaveRequired) { return string.Empty; }
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        var x = LastChange;

        var timestamp = $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}_{UserName}-{MachineInstanceHash}";
        var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);

        // Nur Chunks mit echten Änderungen werden geschrieben und müssen vor dem
        // Speichern auf eine fremde Sperre geprüft werden (siehe Re-Check weiter unten).
        var changedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddChunk(string chunkId, List<byte> data) {
            var isMainLite = string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase);
            var path = isMainLite
                ? Filename
                : $"{GetChunkFolder(chunkId)}{timestamp}.chk";

            if (!isMainLite && _currentChunk.TryGetValue(chunkId, out var existing)
                && existing is not null
                && !existing.IsDisposed
                && !existing.LoadFailed) {
                var head = existing.GetHeadBytes();
                var fullContent = new List<byte>(head.Count + data.Count + 16);
                fullContent.AddRange(head);
                fullContent.AddRange(data);
                SaveToByteList(fullContent, TableDataType.EOF, "END");

                if (existing.Content.SequenceEqual(fullContent)) { return; }
            }

            chunkData[path] = data;
            if (!isMainLite) {
                changedChunkIds.Add(chunkId);
            }
        }

        AddChunk(Chunk_MainData, [
            .. TableChunk.GenerateMainChunk(this),
            .. TableChunk.GenerateUndoChunk(this, false, Chunk_MainData)
        ]);

        AddChunk(Chunk_MainDataLite, [
             .. GenerateMainLiteChunk(),
             ]);

        AddChunk(TableChunk.Chunk_AdditionalUseCases, [
            .. TableChunk.GenerateUsesChunk(this),
            .. TableChunk.GenerateUndoChunk(this, false, TableChunk.Chunk_AdditionalUseCases)
        ]);

        AddChunk(TableChunk.Chunk_Variables, [
            .. TableChunk.GenerateHeadVariableChunks(this),
            .. TableChunk.GenerateUndoChunk(this, false, TableChunk.Chunk_Variables)
        ]);

        AddChunk(TableChunk.Chunk_Master, [
            .. TableChunk.GenerateMasterUserChunk(this),
            .. TableChunk.GenerateUndoChunk(this, false, TableChunk.Chunk_Master)
        ]);

        var rowChunkIds = TableChunk.RowChunkIdsInMemory(this);
        if (!rowChunkIds.Contains(TableChunk.Chunk_UnknownData)) {
            rowChunkIds.Add(TableChunk.Chunk_UnknownData);
        }

        foreach (var thisChunkId in rowChunkIds) {
            AddChunk(thisChunkId, [
                .. TableChunk.GenerateRowChunk(this, false, thisChunkId),
                .. TableChunk.GenerateUndoChunk(this, false, thisChunkId)
            ]);
        }

        if (x != LastChange) { return "Tabelle wurde während der Chunk-Generierung geändert"; }

        // Lazy-Strategie — Re-Check: Da AcquireWriteAccess keinen Claim schreibt, kann ein
        // anderer Benutzer zwischenzeitlich einen der zu schreibenden Chunks gesperrt haben.
        // In diesem Fall Speichern abbrechen und einfrieren, statt fremde Änderungen zu
        // überschreiben. changedChunkIds enthält nur Chunks mit echten inhaltlichen Änderungen.
        foreach (var chunkId in changedChunkIds) {
            var lockCheck = CheckEditLock(chunkId);
            if (!string.IsNullOrEmpty(lockCheck)) {
                Freeze(lockCheck);
                return lockCheck;
            }
        }

        var saveResult = TableChunk.SaveChunkFiles(chunkData, 0, Caption);
        if (!string.IsNullOrEmpty(saveResult)) {
            Freeze(saveResult);
            return saveResult;
        }

        foreach (var path in chunkData.Keys) {
            if (string.Equals(path, Filename, StringComparison.OrdinalIgnoreCase)) {
                // .cfbdb-Hauptdatei — ChunkId ist _MainDataLite, kein Cleanup
                if (CachedFileSystem.Get<Chunk>(path) is { } mainChunk) {
                    _currentChunk[Chunk_MainDataLite.ToLowerInvariant()] = mainChunk;
                }
                continue;
            }

            var folder = path.FilePath();
            var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (CachedFileSystem.Get<Chunk>(path) is { } savedChunk) {
                _currentChunk[chunkId] = savedChunk;
            }
            CleanupOldFilesInFolder(folder);
        }

        SaveRequired = false;
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
    /// Extrahiert den 3-zeiligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss_Username-Hash.chk). Gibt string.Empty zurück,
    /// wenn kein Hash vorhanden ist (alte Dateien).
    /// </summary>
    private static string ExtractMachineInstanceHashFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return string.Empty; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        return sepIdx == userAndHash.Length - 4 ? userAndHash[(sepIdx + 1)..] : string.Empty;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss_Username-Hash.chk). Der angehängte
    /// Machine/Instance-Hash wird abgeschnitten. Bei alten Dateien ohne Hash
    /// wird der komplette Teil nach dem ersten Unterstrich zurückgegeben.
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return fileName; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen lang, gefolgt von einem Bindestrich am Ende.
        return sepIdx == userAndHash.Length - 4 ? userAndHash[..sepIdx] : userAndHash;
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
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer UND der aktuellen
    /// Maschine/Instanz erstellt wurde. Stimmt nur der Benutzername, aber nicht der
    /// Machine/Instance-Hash überein, gilt die Datei als fremd (anderer Rechner/Session).
    /// </summary>
    private static bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        if (!string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase)) { return false; }

        var creatorHash = ExtractMachineInstanceHashFromFileName(filePath);
        return string.Equals(creatorHash, MachineInstanceHash, StringComparison.OrdinalIgnoreCase);
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
        var creatorHash = ExtractMachineInstanceHashFromFileName(newestFile);
        var creatorDisplay = string.IsNullOrEmpty(creatorHash) ? creator : $"{creator} - {creatorHash}";
        return $"Chunk '{chunkId}' wird seit {DateTime.UtcNow.Subtract(fileDate.Value).TotalMinutes:0} Minuten von '{creatorDisplay}' bearbeitet";
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

        var folder = GetChunkFolder(chunkId);

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer kann zwischenzeitlich eine neuere .chk-Datei abgelegt haben.
        string? newestFile = null;
        if (IO.DirectoryExists(folder)) {
            newestFile = GetChunkFilesOrderedByTime(folder).FirstOrDefault();
        }

        if (_currentChunk.TryGetValue(chunkId, out var existingChunk) && existingChunk is not null && !existingChunk.IsDisposed) {
            if (!string.Equals(existingChunk.Filename.FileSuffix(), "chk", StringComparison.OrdinalIgnoreCase)) {
                // Veralteter Cache-Eintrag entfernen und neu laden
                _currentChunk.Remove(chunkId);
            } else if (newestFile is not null &&
                       string.Equals(existingChunk.Filename, newestFile, StringComparison.OrdinalIgnoreCase) &&
                       !existingChunk.IsStale()) {
                // Cache ist aktuell (gleiche Datei, nicht verändert) — kein Reload nötig
                existingChunk.LastUsed = DateTime.UtcNow;
                return OperationResult.SuccessFalse;
            } else {
                // Eine neuere oder veränderte Datei existiert — Cache verwerfen und neu laden
                _currentChunk.Remove(chunkId);
            }
        }

        if (!IO.DirectoryExists(folder)) {
            if (IO.CreateDirectory(folder).IsFailed) {
                return OperationResult.Failed("Ordner konnte nicht erstellt werden");
            }
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
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

        Row.RemoveObsoleteRows(TableChunk.RowsOfChunk(this, chunk), parsedRowKeys);

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

            if (!firstTime && _currentChunk.TryGetValue(chunkId, out var cached) && cached is not null && !cached.IsDisposed && DateTime.UtcNow.Subtract(cached.LastUsed).TotalMinutes >= SkipIfUnusedMinutes) { continue; }

            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var currentChunk = kvp.Value;
            if (currentChunk is null || currentChunk.IsDisposed) { continue; }

            // Reload wenn eine andere Datei die neueste ist ODER die aktuelle Datei
            // auf der Platte verändert wurde (z.B. Überschreiben durch gleichen Benutzer).
            if (!string.Equals(currentChunk.Filename, newestFile, StringComparison.OrdinalIgnoreCase) ||
                currentChunk.IsStale()) {
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

    #endregion
}