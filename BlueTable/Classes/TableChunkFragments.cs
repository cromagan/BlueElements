// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Lite-Hauptdatei (.tblh, Head + CheckPoint + EOF, keine Nutzdaten)
/// und Row-Chunks in eigenen Unterordnern mit Benutzer-spezifischen .tblc-Dateien.
/// Jeder Benutzer schreibt in seine eigene Datei (yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc),
/// wobei Hash ein 3-stelliger Wert aus MachineName und Instanz-ID ist. So werden auch
/// verschiedene Maschinen/Instanzen bei gleichem Benutzernamen unterschieden.
/// Das Datum im Dateinamen (UTC) ist der alleinige Vergleich für Aktualität.
/// Dateien älter als eine Stunde werden gelöscht (wenn mehrere vorhanden).
/// Edit-Sperre: Neueste Datei &lt;10 Min → nur der Ersteller darf bearbeiten.
/// <para>
/// Im Gegensatz zu <see cref="TableChunk"/> werden Chunks hier NICHT über das
/// <see cref="BlueBasics.Classes.FileSystemCaching.CachedFileSystem"/> verwaltet.
/// Geladene Chunks werden nach dem Parsen sofort verworfen — nur der Dateiname
/// wird für Aktualitätsprüfungen gemerkt. Jeder Chunk ist ein Einweg (write-once):
/// eine einmal gespeicherte Datei wird nie wieder überschrieben. Vor dem Parsen
/// wird <see cref="TableFile.HasValidEofMarker"/> geprüft, damit nur komplett
/// gespeicherte Chunks eingelesen werden.
/// </para>
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].tblh                                                       (Lite-Chunk, keine Nutzdaten)
///   [TableName]\[ChunkID]\yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc       (Row-Chunk pro Chunk)
/// </remarks>
[Browsable(false)]
[FileSuffix(".tblh")]
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
    /// Chunk-ID für die Lite-Hauptdatei (.tblh). Enthält nur Head + CheckPoint + EOF,
    /// keine Nutzdaten. Die eigentlichen Daten liegen in den Row-Chunks (.tblc).
    /// </summary>
    public static readonly string Chunk_MainDataLite = "_MainDataLite";

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 3;

    /// <summary>
    /// chunkId (lowercase) → SHA256-Hash des kompletten Chunk-Inhalts (Head + Daten + EOF).
    /// Verhindert unnötiges Schreiben bei unverändertem Inhalt (Write-once: keine
    /// identischen Duplikate erzeugen).
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _lastContentHash = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → UTC-Zeitpunkt des letzten Zugriffs (Laden, Speichern, Refresh).
    /// Wird genutzt, um ungenutzte Chunks bei BeSureToBeUpToDate zu überspringen.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → Dateiname der zuletzt geparten oder gespeicherten Chunk-Datei.
    /// Leichtgewichtiges Tracking anstelle von im Speicher gehaltenen Chunk-Objekten:
    /// Chunks werden nach dem Parsen verworfen, nur der Dateiname wird für
    /// Aktualitätsprüfungen (Refresh) vorgehalten.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _processedFile = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public TableChunkFragments(string tablename) : base(tablename) { }

    public TableChunkFragments(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    /// <summary>
    /// Letzter UTC-Zeitpunkt der letzten Speicherung der Hauptdatei.
    /// Liest direkt vom Dateisystem (nicht über CachedFileSystem, da .tblh
    /// nicht mehr im CachedFileSystem registriert ist).
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(Filename);
            if (fi is { Exists: true }) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public override bool MultiUserPossible => true;

    /// <summary>
    /// Generiert einen 3-stelligen Hash aus MachineName und Instanz-ID (MyId).
    /// Unterscheidet verschiedene Maschinen/Instanzen bei gleichem Benutzernamen
    /// auf dem Dateisystem.
    /// </summary>
    private static string MachineInstanceHash => $"{Environment.MachineName}|{MyId}".GetMD5Hash()[..3].ToUpperInvariant();

    #endregion

    #region Methods

    /// <summary>
    /// Generiert den Content für die Lite-Hauptdatei (.tblh). Enthält ausschließlich
    /// den CheckPoint, keine Nutzdaten — die eigentlichen Daten liegen in den Row-Chunks.
    /// Head und EOF werden beim Speichern ergänzt.
    /// </summary>
    public static List<byte> GenerateMainLiteChunk() {
        var result = new List<byte>();

        SaveToByteList(result, TableDataType.CheckPoint, $"~^{Chunk_MainDataLite.ToLowerInvariant()}^~");

        return result;
    }

    /// <summary>
    /// Lazy-Strategie: Der Chunk wird beim Anfordern des Schreibrechts NICHT sofort
    /// durch eine eigene .tblc-Datei beansprucht. Es wird nur geprüft, ob ein anderer
    /// Benutzer den Chunk in den letzten <see cref="EditLockMinutes"/> Minuten
    /// reserviert hat. Der tatsächliche Claim (die neue .tblc-Datei) entsteht erst in
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
            // System-Chunks werden immer geprüft (kein SkipIfUnusedMinutes-Skip).
            // Im Gegensatz zu TableChunk gibt es hier kein CachedFileSystem, das
            // neu erscheinende Dateien automatisch erkennt. Ohne diese Prüfung
            // würden neu erstellte System-Chunks anderer Benutzer (z.B. _master)
            // nie bemerkt werden, sobald sie einmal als "nicht vorhanden" erkannt wurden.
            // LoadChunkWithChunkId kehrt bei unveränderten Dateien schnell zurück
            // (Already-Current-Check via Dateiname-Vergleich).
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
    /// Prüft, ob der Row-Chunk für den angegebenen Chunk-Wert aktuell im Speicher
    /// geladen ist (geparst wurde). Wird für verlinkte Zellen benötigt, um
    /// sicherzustellen, dass die Zielzeile verfügbar ist.
    /// </summary>
    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = TableChunk.GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        return _processedFile.ContainsKey(chunkId);
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

    /// <summary>
    /// Überschrieben, da die Basisklasse <see cref="TableFile.IsValueEditable"/>
    /// CachedFileSystem.Get&lt;Chunk&gt;(Filename) verwendet, was für .tblh-Dateien
    /// nicht mehr funktioniert (Suffix wurde vom CachedFileSystem entfernt).
    /// </summary>
    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        if (InitialSavePending) { return string.Empty; }

        var f = IsGenericEditable(false);
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

    /// <summary>
    /// Generiert den Timestamp-String für Chunk-Dateinamen im Format
    /// yyyy-MM-dd-HH-mm-ss-fff_Username-Hash. Wird von <see cref="Table.RenameChunks"/>
    /// genutzt, um beim Formatwechsel einheitliche Dateinamen zu erzeugen.
    /// </summary>
    internal static string GenerateChunkTimestamp() =>
        $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MachineInstanceHash}";

    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        var result = LoadChunkWithChunkId(Chunk_MainData);
        if (result.IsFailed) { return false; }

        // Bei einer geladenen Tabelle muss der Hauptchunk vorhanden sein.
        // Fehlt er (Ordner leer), ist die Tabelle korrupt.
        if (!_processedFile.ContainsKey(Chunk_MainData.ToLowerInvariant())) {
            Freeze("Hauptchunk der Tabelle fehlt");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Speichert alle Chunks direkt auf die Festplatte (ohne CachedFileSystem).
    /// Nach dem Speichern werden alle generierten Daten verworfen.
    /// Jeder Chunk ist ein Einweg — eine einmal gespeicherte Datei wird nie
    /// wieder überschrieben. Die Lite-Hauptdatei (.tblh) wird ebenfalls nur einmal
    /// geschrieben (write-once), da sie keine Nutzdaten enthält.
    /// </summary>
    protected override string SaveInternal() {
        if (!SaveRequired) { return string.Empty; }
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        var x = LastChange;

        var timestamp = GenerateChunkTimestamp();
        var head = GenerateHeadBytes();

        // path → vollständiger Inhalt (Head + Daten + EOF), fertig zum Schreiben
        var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);
        // chunkId (lowercase) → Hash des Voll-Inhalts, aktualisiert nach erfolgreichem Speichern
        var newHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // chunkIds mit echten Änderungen (für EditLock Re-Check vor dem Schreiben)
        var changedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddChunk(string chunkId, List<byte> data) {
            var idLower = chunkId.ToLowerInvariant();
            var isMainLite = string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase);

            // Vollständigen Inhalt aufbauen (Head + Daten + EOF) für Hash-Vergleich und Speicherung
            var fullContent = new List<byte>(head.Count + data.Count + 16);
            fullContent.AddRange(head);
            fullContent.AddRange(data);
            SaveToByteList(fullContent, TableDataType.EOF, "END");

            var fullHash = Generic.GetSHA256HashString(fullContent.ToArray());
            newHashes[idLower] = fullHash;

            // Unchanged-Check: bei identischem Inhalt keine neue Datei schreiben
            if (_lastContentHash.TryGetValue(idLower, out var storedHash) && storedHash == fullHash) {
                return;
            }

            if (isMainLite) {
                // Write-once: .tblh nur schreiben, wenn noch kein gültiger Inhalt vorhanden
                if (IO.FileExists(Filename) && HasValidEofMarker(IO.ReadAllBytes(Filename, 2).Value as byte[] ?? [])) {
                    _processedFile[Chunk_MainDataLite.ToLowerInvariant()] = Filename;
                    _lastContentHash[idLower] = fullHash;
                    return;
                }
                chunkData[Filename] = fullContent;
            } else {
                chunkData[$"{GetChunkFolder(chunkId)}{timestamp}.tblc"] = fullContent;
                changedChunkIds.Add(idLower);
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

        // Direkt auf Festplatte schreiben (ohne CachedFileSystem).
        // Chunks sind write-once — jede Datei ist neu und wird nie überschrieben.
        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks der Tabelle '{Caption}'", 2);

        foreach (var kvp in chunkData) {
            var path = kvp.Key;

            if (IO.CreateDirectory(path.FilePath()).IsFailed) {
                Freeze($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                return $"Verzeichnis für '{path}' konnte nicht erstellt werden";
            }

            // Chunk-Dateien werden gezippt gespeichert (wie bei CachedFile/Chunk mit MustZipped=true)
            var zipped = kvp.Value.ToArray().ZipIt();
            if (zipped is null || zipped.Length == 0) {
                Freeze($"Komprimierung fehlgeschlagen für '{path}'");
                return $"Komprimierung fehlgeschlagen für '{path}'";
            }

            var writeResult = IO.WriteAllBytes(path, zipped);
            if (writeResult.IsFailed) {
                Freeze(writeResult.FailedReason);
                return writeResult.FailedReason;
            }

            // Nach dem Speichern: nur den Dateinamen tracken, Daten verwerfen
            if (string.Equals(path, Filename, StringComparison.OrdinalIgnoreCase)) {
                _processedFile[Chunk_MainDataLite.ToLowerInvariant()] = path;
            } else {
                var folder = path.FilePath();
                var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
                _processedFile[chunkId] = path;
                _lastUsed[chunkId] = DateTime.UtcNow;
                CleanupOldFilesInFolder(GetChunkFilesOrderedByTime(folder).ToArray());
            }
        }

        // Hashes nach erfolgreichem Speichern aktualisieren
        foreach (var kvp in newHashes) {
            _lastContentHash[kvp.Key] = kvp.Value;
        }

        SaveRequired = false;
        InitialSavePending = false;
        OnInvalidateView();
        return string.Empty;
    }

    /// <summary>
    /// Löscht alle .tblc-Dateien in der übergebenen Liste, die älter als
    /// DeleteOldFilesAfterHours Stunden sind, sofern mindestens FileCount neuere Dateien
    /// vorhanden sind. Dateien mit ungültigem Datumsformat werden ebenfalls gelöscht.
    /// Die Liste muss nach Datum absteigend (neueste zuerst) sortiert sein.
    /// </summary>
    private static void CleanupOldFilesInFolder(string[] files) {
        if (files.Length <= FileCount) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        for (var i = files.Length - 1; i >= FileCount; i--) {
            var file = files[i];
            var fileDate = ExtractDateFromFileName(file);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Chunk-Dateinamen (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc).
    /// Millisekunden sind verpflichtend. Gibt null zurück, wenn das Datum nicht geparst werden kann.
    /// </summary>
    private static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 23) { return null; }
        var datePart = fileName[..23];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Extrahiert den 3-stelligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc). Gibt string.Empty zurück,
    /// wenn kein Hash vorhanden ist (alte Dateien).
    /// </summary>
    private static string ExtractMachineInstanceHashFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return string.Empty; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[(sepIdx + 1)..] : string.Empty;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc). Der angehängte
    /// Machine/Instance-Hash wird abgeschnitten. Bei alten Dateien ohne Hash
    /// wird der komplette Teil nach dem ersten Unterstrich zurückgegeben.
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return fileName; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[..sepIdx] : userAndHash;
    }

    /// <summary>
    /// Generiert die Head-Bytes (Version + Werbung), die jeder Chunk-Datei
    /// vorangestellt werden. Entspricht Chunk.GetHeadBytes(), aber ohne
    /// Instanz-Abhängigkeit — wird direkt ohne CachedFileSystem genutzt.
    /// </summary>
    private static List<byte> GenerateHeadBytes() {
        var headBytes = new List<byte>();

        SaveToByteList(headBytes, TableDataType.Version, Table.TableVersion);
        SaveToByteList(headBytes, TableDataType.Werbung, "                                                                    BlueTable - (c) by Christian Peter                                                                                        ");

        return headBytes;
    }

    /// <summary>
    /// Listet alle .tblc-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
    /// Liest direkt vom Dateisystem, damit neu hinzugekommene Dateien anderer
    /// Benutzer sofort erkannt werden.
    /// </summary>
    private static IEnumerable<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return IO.GetFiles(folder)
                .Where(f => string.Equals(f.FileSuffix(), "tblc", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => ExtractDateFromFileName(f) ?? DateTime.MinValue);
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
    /// Stellt sicher, dass die Dummy .tblh-Datei existiert (0 Bytes).
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
    /// Lädt einen Chunk direkt von der Festplatte (ohne CachedFileSystem).
    /// Vor dem Parsen wird <see cref="TableFile.HasValidEofMarker"/> geprüft,
    /// damit nur komplett gespeicherte Chunks eingelesen werden. Nach dem
    /// Parsen wird der Chunk-Inhalt verworfen — nur der Dateiname wird
    /// für Aktualitätsprüfungen gemerkt.
    /// </summary>
    /// <param name="chunkId">Chunk-ID (wird auf Lowercase normalisiert).</param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordnung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    private OperationResult LoadChunkWithChunkId(string chunkId) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        var folder = GetChunkFolder(chunkId);

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer konnte zwischenzeitlich eine neuere .tblc-Datei ablegen.
        var chunkFiles = IO.DirectoryExists(folder) ? GetChunkFilesOrderedByTime(folder).ToArray() : [];
        string? newestFile = chunkFiles.Length > 0 ? chunkFiles[0] : null;

        // Bereits aktuell? (gleiche Datei wie zuletzt verarbeitet)
        if (newestFile is not null &&
            _processedFile.TryGetValue(chunkId, out var lastFile) &&
            string.Equals(lastFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (!IO.DirectoryExists(folder)) {
            if (IO.CreateDirectory(folder).IsFailed) {
                return OperationResult.Failed("Ordner konnte nicht erstellt werden");
            }
            // _lastUsed aktualisieren, damit nicht-existierende Chunks in BeSureToBeUpToDate
            // nicht permanent übersprungen werden, sondern nach SkipIfUnusedMinutes neu geprüft werden.
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
            // Alle Dateien des Chunks wurden gelöscht — Tracking-Eintrag entfernen,
            // damit veraltete Daten nicht als "aktuell" gelten.
            if (_processedFile.TryRemove(chunkId, out var prevFile)) {
                Develop.Diagnose("CF", $"Chunk '{chunkId}': Alle Dateien gelöscht, bisher verarbeitet: {prevFile}");
            }
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        // Raw bytes direkt von der Festplatte lesen (ohne CachedFileSystem)
        if (IO.ReadAllBytes(newestFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht gelesen werden");
        }

        // VOR dem Parsen: EOF-Marker prüfen — nur komplett gespeicherte Chunks laden.
        // Fehlt der Marker, ist die Datei möglicherweise gerade beim Schreiben
        // (anderer Benutzer) oder korrupt. In beiden Fall nicht laden.
        if (!HasValidEofMarker(rawBytes)) {
            Develop.Diagnose("CF", $"Chunk '{chunkId}' unvollständig (kein EOF-Marker): {newestFile.FileNameWithoutSuffix()}");
            return OperationResult.Failed($"Row-Chunk '{chunkId}' ist unvollständig (kein EOF-Marker)");
        }

        // Für Parsing entpacken (Chunk-Dateien werden gezippt gespeichert)
        byte[] chunkContent;
        if (rawBytes.IsZipped()) {
            chunkContent = rawBytes.UnzipIt() ?? [];
            if (chunkContent.Length == 0) {
                return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht entpackt werden");
            }
        } else {
            chunkContent = rawBytes;
        }

        // CheckPoint prüfen (System-Chunks suchen nach ~^{KeyName}^~, Row-Chunks immer ok)
        if (!HasCheckPoint(chunkContent, chunkId)) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' enthält keinen gültigen CheckPoint");
        }

        var isMain = string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase);

        if (!ParseChunk(chunkContent, chunkId, isMain)) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' Parsen fehlgeschlagen");
        }

        // Chunk nach dem Parsen verwerfen — nur Dateiname und Hash tracken
        _processedFile[chunkId] = newestFile;
        _lastUsed[chunkId] = DateTime.UtcNow;
        _lastContentHash[chunkId] = Generic.GetSHA256HashString(chunkContent);

        CleanupOldFilesInFolder(chunkFiles);

        return OperationResult.SuccessValue(true);
    }

    /// <summary>
    /// Parst den Chunk-Inhalt in die Tabellen-Daten. Nach dem Parsen kann der
    /// Inhalt verworfen werden — die Daten sind in den Rows/Columns eingebucht.
    /// </summary>
    private bool ParseChunk(byte[] chunkContent, string chunkId, bool isMain) {
        if (chunkContent.Length == 0) { return true; }

        Develop.Diagnose("UNDO", $"ParseChunk RemoveAll WAIT: chunk={chunkId} T{Environment.CurrentManagedThreadId}");
        lock (_undoLock) {
            Develop.Diagnose("UNDO", $"ParseChunk RemoveAll ENTER: chunk={chunkId} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
            Undo.RemoveAll(item => item is not null
                && string.Equals(TableChunk.GetChunkId(this, item.Command, item.RowKey is { Length: > 0 } rk ? Row.GetByKey(rk)?.ChunkValue ?? string.Empty : string.Empty), chunkId, StringComparison.OrdinalIgnoreCase));
            Develop.Diagnose("UNDO", $"ParseChunk RemoveAll DONE: chunk={chunkId} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
        }

        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseSuccessful = Parse(chunkContent, isMain, parsedRowKeys);

        if (!parseSuccessful) {
            Freeze($"Chunk {chunkId} Parsen fehlgeschlagen");
            return false;
        }

        Row.RemoveObsoleteRows(TableChunk.RowsOfChunk(this, chunkId), parsedRowKeys);

        return true;
    }

    /// <summary>
    /// Aktualisiert alle geladenen Row-Chunks: Prüft für jeden, ob eine neuere
    /// Datei im Ordner existiert und lädt diese ggf. neu.
    /// Chunks, die länger als <see cref="Chunk.SkipIfUnusedMinutes"/> nicht verwendet wurden,
    /// werden übersprungen, sofern <paramref name="firstTime"/> false ist.
    /// Da Chunks write-once sind, reicht der Dateiname-Vergleich (kein IsStale nötig).
    /// </summary>
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }
        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return false; }

        var loaded = false;

        //// Neue Row-Chunk-Ordner entdecken (z.B. neue Zeilen anderer Benutzer).
        //// Ohne CachedFileSystem werden neue Ordner nicht automatisch erkannt.
        //foreach (var subDir in IO.GetDirectories(chunkFolder)) {
        //    var newChunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
        //    if (IsRowChunk(newChunkId) && !_processedFile.ContainsKey(newChunkId)) {
        //        var result = LoadChunkWithChunkId(newChunkId);
        //        loaded = loaded || result.Value is true;
        //    }
        //}

        foreach (var chunkId in _processedFile.Keys.ToList()) {
            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            if (!firstTime && _lastUsed.TryGetValue(chunkId, out var lastUsed) && DateTime.UtcNow.Subtract(lastUsed).TotalMinutes >= Chunk.SkipIfUnusedMinutes) { continue; }

            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var processedFile = _processedFile[chunkId];

            // Reload wenn eine andere Datei die neueste ist.
            // Da Chunks write-once sind, bedeutet ein anderer Dateiname zwingend
            // anderen Inhalt — keine zusätzlichen Stale-Checks nötig.
            if (!string.Equals(processedFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
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