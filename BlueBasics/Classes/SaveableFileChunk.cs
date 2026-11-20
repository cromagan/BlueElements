// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Datei-Verwaltung mit Kopf-Datei (Layout-Kopf) und Daten-Chunks in einem
/// Arbeitsunterordner
/// gleichen Namens ohne Suffix, mit Benutzer-spezifischen Schreibdateien
/// (write-once, duplizierter Ansatz des klassischen Chunk-Systems). Gechunkt wird
/// ausschließlich über den NAMEN des Chunk-Wertes (von außen geliefert); eine
/// Erweiterung auf andere Chunk-Typen ist bewusst nicht vorgesehen. Jeder
/// Benutzer schreibt in seine eigene Datei
/// (yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Dateien älter als zwei Stunden werden
/// gelöscht (wenn mehrere vorhanden). Edit-Sperre: Neueste Datei unter
/// EditLockSeconds → nur der Ersteller darf bearbeiten. Jeder Chunk ist ein
/// Einweg (write-once): eine einmal gespeicherte Datei wird nie wieder
/// überschrieben. Vor dem Parsen wird FragmentRecord.HasValidEofMarker geprüft.
/// <para>
/// Alle Datei-Aktionen erhalten eine Info (den Chunk-Namen), sodass intern
/// entschieden wird, welche Chunk-Datei geprüft, geladen oder gespeichert wird.
/// Den Inhalt je Chunk und das Einlesen geben die Hooks an die konkrete
/// Ableitung ab.
/// </para>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class SaveableFileChunk : SaveableFile {

    #region Layout

    /// <summary>
    /// ClassId des Chunk-Layouts; steht im Layout-Kopf der Kopf-Datei.
    /// </summary>
    public static string ClassId => "Chunk";

    /// <summary>
    /// Die ClassId des konkreten Layouts.
    /// </summary>
    protected override string LayoutId => ClassId;

    /// <summary>
    /// Anzeigename des Chunk-Layouts für Auswahldialoge.
    /// </summary>
    public override string ReadableText() => "Chunk basiert";

    /// <summary>
    /// Kurzinfo des Chunk-Layouts für Auswahldialoge.
    /// </summary>
    public override string QuickInfo => "Die Daten liegen als unveränderliche Chunk-Dateien im Arbeitsunterordner.\r\nEs werden nicht die kompletten Daten geladen. Bei Bedarf werden Daten nachgeladen.";

    #endregion

    #region Fields

    /// <summary>
    /// Wert in Stunden. Benutzer-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens FileCount neuere Dateien im gleichen Ordner existieren).
    /// </summary>
    public const int DeleteOldFilesAfterHours = 2;

    public static readonly string Chunk_AdditionalUseCases = "_uses";

    /// <summary>
    /// Chunk-ID für die Hauptdaten (Kopf, Struktur, Ansichten usw.).
    /// </summary>
    public static readonly string Chunk_MainData = "_MainData";

    /// <summary>
    /// Chunk-ID für die Lite-Hauptdatei. Enthält nur den CheckPoint, keine
    /// Nutzdaten. Die eigentlichen Daten liegen in den Chunks.
    /// </summary>
    public static readonly string Chunk_MainDataLite = "_MainDataLite";

    public static readonly string Chunk_Master = "_master";

    public static readonly string Chunk_UnknownData = "_rowdata";

    public static readonly string Chunk_Variables = "_vars";

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 5;

    /// <summary>
    /// Wert in Minuten. Die Master-Prüfung wird höchstens in diesem Intervall
    /// durchgeführt, sofern man Master werden kann.
    /// </summary>
    private const int MasterCheckIntervalMinutes = 15;

    /// <summary>
    /// Wert in Minuten. Ein Master-Stempel gilt als aktiv, solange sein MasterTimeUtc
    /// (Stempelzeit + dieses Intervall) in der Zukunft liegt; danach ist der
    /// Master-Platz wieder frei.
    /// </summary>
    private const int MasterClaimValidMinutes = 30;

    /// <summary>
    /// chunkId (lowercase) → SHA256-Hash des kompletten Chunk-Inhalts.
    /// Verhindert unnötiges Schreiben bei unverändertem Inhalt (Write-once: keine
    /// identischen Duplikate erzeugen).
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _lastContentHash = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → UTC-Zeitpunkt des letzten Zugriffs (Laden, Speichern,
    /// Refresh, Anzeige via Touch). Wird genutzt, um ungenutzte Chunks bei
    /// Refresh zu überspringen.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → Dateiname der zuletzt geparten oder gespeicherten Chunk-Datei.
    /// Chunks werden nach dem Parsen verworfen, nur der Dateiname wird für
    /// Aktualitätsprüfungen vorgehalten.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _processedFile = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reentrancy-Sperre für Refresh: 0 = frei, 1 = läuft.
    /// </summary>
    private int _isRefreshingChunks;

    /// <summary>
    /// UTC-Zeitpunkt der letzten Master-Prüfung.
    /// </summary>
    private DateTime _lastMasterAttemptUtc = DateTime.MinValue;

    /// <summary>
    /// Wird beim Init einmalig verifiziert, ob alle System-Chunks ladbar sind.
    /// </summary>
    protected bool SystemChunksVerified { get; set; }

    /// <summary>
    /// Fehler der einmaligen System-Chunks-Verifikation; leer bei Erfolg.
    /// </summary>
    protected string SystemChunksError { get; set; } = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Chunk-Verwaltung für eine Lite-Hauptdatei.
    /// </summary>
    /// <summary>
    /// Erstellt eine Layout-Instanz ohne Datei-Bezug für die Aufzählung der Datei-Layouts.
    /// </summary>
    public SaveableFileChunk() { }

    public SaveableFileChunk(string filename) : base(filename) {
        InitialSavePending = true;
        SaveRequired = true;
    }

    #endregion

    #region Abstracts

    /// <summary>
    /// Erzeugt den kompletten Inhalt (Kopf + Daten + Abschluss-Marker) des Chunks
    /// mit der angegebenen ID; die System-IDs sind als Konstanten bekannt.
    /// Der Master-Chunk trägt den Master-Stempel, alle anderen Chunks den
    /// kompletten Inhalt des besitzenden Objekts (GenerateContentToSave).
    /// </summary>
    protected virtual List<byte> BuildChunkContent(string chunkId) {
        if (string.Equals(chunkId, Chunk_Master, StringComparison.OrdinalIgnoreCase)) { return BuildMasterChunkContent(); }

        return GenerateContentToSave();
    }

    /// <summary>
    /// Parst den Inhalt eines Chunks in die Daten des besitzenden Objekts.
    /// Nach dem Parsen kann der Inhalt verworfen werden.
    /// Basis: Der Inhalt wird über den Inhalts-Seam (ParseMainContent) eingelesen.
    /// </summary>
    protected virtual bool ParseChunkContent(byte[] chunkContent, string chunkId, bool isMain) {
        using var ms = new MemoryStream(chunkContent);

        return ParseMainContent(ms);
    }

    /// <summary>
    /// Liefert die Chunk-IDs der Datenbereiche, die aktuell im Speicher vorhanden sind.
    /// Basis: keine bekannten Datenbereiche.
    /// </summary>
    protected virtual List<string> KnownChunkIds() => [];

    #endregion

    #region Properties

    /// <summary>
    /// Wert in Sekunden. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public static int EditLockSeconds { get; set; } = 300;

    /// <summary>
    /// Multi-User-Zugriff ist über das Chunk-Layout möglich.
    /// </summary>
    public override bool MultiUserPossible => true;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(Filename);
            if (fi is { Exists: true }) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Das Objekt gilt als kürzlich verwendet, wenn mindestens ein Chunk
    /// kürzlich verwendet wurde.
    /// </summary>
    public override bool IsRecentlyUsed {
        get {
            if (_lastUsed.IsEmpty) { return false; }
            var threshold = DateTime.UtcNow.AddMinutes(-SkipIfUnusedMinutes);
            foreach (var kvp in _lastUsed) {
                if (kvp.Value > threshold) { return true; }
            }
            return false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Ermittelt die Chunk-ID LOWERCASE aus dem Chunk-Wert — ausschließlich
    /// über den Namen.
    /// </summary>
    public static string ChunkValueToId(string chunkvalue) {
        if (string.IsNullOrEmpty(chunkvalue)) { chunkvalue = " "; }

        var t = Formats.SystemNameFormat.MakeValid(chunkvalue).TrimStart('_');
        return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();
    }

    /// <summary>
    /// Öffnet und lädt die Lite-Hauptdatei (Dummy) und richtet die Chunk-Ordner ein.
    /// </summary>
    public override void LoadFromFile(string info, string freeze) {
        if (!PrepareLoad(info, out var fileNameToLoad)) { return; }

        Filename = fileNameToLoad;

        LoadCore(fileNameToLoad, freeze);
    }

    /// <summary>
    /// Stellt sicher, dass alle relevanten Chunks aktuell sind: System-Chunks
    /// werden immer geprüft, Daten-Chunks über das Fenster-Refresh.
    /// </summary>
    public override bool Refresh(bool firstTime) {
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        // Reentrancy-Sperre: Timer sowie UI-Aktionen können Refresh zeitgleich
        // aufrufen. Ohne Sperre würden sich die Aufrufe überlappen.
        if (Interlocked.CompareExchange(ref _isRefreshingChunks, 1, 0) != 0) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Aktualisierung von '{KeyName}' läuft bereits — übersprungen, um eine Überlagerung zu vermeiden", 0);
            return true;
        }

        try {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

            if (firstTime) {
                if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }
            }

            var loaded = false;
            var ok = true;

            // Während des Neuladens sind Schlüssel kurzzeitig nicht auflösbar.
            PauseDataReload();
            try {
                OnLoading();

                if (!firstTime) {
                    var result = LoadChunkWithChunkId(Chunk_MainData, false);
                    if (result.IsFailed) {
                        Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Haupt-Chunk von '{KeyName}' konnte nicht geladen werden: {result.FailedReason}", 0);
                        return false;
                    }
                    loaded = result.Value is true;
                }

                OnRefreshDataSystems();

                List<string> list = [Chunk_AdditionalUseCases, Chunk_Master, Chunk_Variables, Chunk_UnknownData];

                foreach (var item in list) {
                    // System-Chunks werden immer geprüft (kein SkipIfUnusedMinutes-Skip).
                    // LoadChunkWithChunkId kehrt bei unveränderten Dateien schnell zurück
                    // (Already-Current-Check via Dateiname-Vergleich).
                    var result = LoadChunkWithChunkId(item, false);
                    loaded = loaded || result.Value is true;
                    ok = ok && result.IsSuccessful;
                }

                loaded = loaded || RefreshLoadedChunks(firstTime);

                if (loaded) { OnLoaded(firstTime); }
            } finally {
                ResumeDataReload();
            }

            // Master-Prüfung nur alle MasterCheckIntervalMinutes Minuten durchführen,
            // sofern man Master werden kann. Beim ersten Mal (Init) sofort prüfen.
            if (ok && (firstTime || DateTime.UtcNow.Subtract(_lastMasterAttemptUtc).TotalMinutes >= MasterCheckIntervalMinutes)) {
                _lastMasterAttemptUtc = DateTime.UtcNow;
                TryToSetMeTemporaryMaster();
            }

            return ok;
        } finally {
            Interlocked.Exchange(ref _isRefreshingChunks, 0);
        }
    }

    /// <summary>
    /// Periodischer Tick: lädt Chunks nach und speichert bedarfsgesteuert.
    /// </summary>
    public override void CheckerTick() {
        if (!SaveRequired || !MaySaveNow) { return; }

        // Speichern wenn nötig (der Chunk-Writer prüft die Edit-Sperren je Chunk)
        _ = Save();
    }

    /// <summary>
    /// True, wenn diese Instanz temporärer Master werden darf. Zusätzlich zur Basis
    /// wird ein fremder, noch gültiger Master-Stempel respektiert; ein abgelaufener
    /// oder eigene Stempel machen den Master-Platz wieder frei.
    /// </summary>
    public override bool AmITemporaryMaster(bool updateAllowed) {
        if (!base.AmITemporaryMaster(updateAllowed)) { return false; }

        if (ReadMasterClaim() is not { } claim) { return true; }

        // Eigener Stempel: Ich bin (noch) Master.
        if (claim.User.Equals(UserName, StringComparison.OrdinalIgnoreCase)) { return true; }

        // Fremder Stempel: nur übernehmbar, wenn er abgelaufen ist.
        return claim.ValidUntilUtc <= DateTime.UtcNow;
    }

    /// <summary>
    /// Erzeugt den Inhalt des Master-Chunks: den Stempel des aktuellen Masters
    /// (Benutzer, Rechner, Anwendung, Instanz, gültig bis). Ohne Master-Eigenschaft
    /// bleibt der Inhalt leer — der Chunk wird dann nicht geschrieben.
    /// </summary>
    private List<byte> BuildMasterChunkContent() {
        if (!IsMaster) { return []; }

        using var ms = new MemoryStream();

        _ = FragmentRecord.Append(ms, 0, FragmentTagType.SerializerVersion, "1.00");
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.MasterUser, UserName);
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.MasterMachine, Environment.MachineName);
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.MasterApp, Develop.AppExe());
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.MasterId, MyId);
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.MasterTimeUtc, DateTime.UtcNow.AddMinutes(MasterClaimValidMinutes));
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.Eof, string.Empty);

        return [.. ms.ToArray()];
    }

    /// <summary>
    /// Liest den jüngsten gültigen Master-Stempel aus dem Master-Chunk-Ordner.
    /// Liefert null, wenn kein lesbarer Stempel vorhanden ist.
    /// </summary>
    private MasterClaim? ReadMasterClaim() {
        var folder = GetChunkFolder(Chunk_Master);
        if (!IO.DirectoryExists(folder)) { return null; }

        foreach (var file in GetChunkFilesOrderedByTime(folder)) {
            if (IO.ReadAllBytes(file, 2).Value is not byte[] rawBytes || rawBytes.Length == 0) { continue; }

            var content = rawBytes.IsZipped() ? rawBytes.UnzipIt() : rawBytes;
            if (content is not { Length: > 0 } || !FragmentRecord.HasValidEofMarker(content)) { continue; }

            string? user = null;
            var validUntilUtc = DateTime.MinValue;

            using var ms = new MemoryStream(content);

            while (FragmentRecord.TryReadNext(ms) is { } record) {
                if (!record.IsFileTag) { break; }

                switch (record.TagType) {
                    case FragmentTagType.MasterUser:
                        user = record.StringContent;
                        break;
                    case FragmentTagType.MasterTimeUtc:
                        validUntilUtc = record.DateTimeContent;
                        break;
                }
            }

            if (user is { Length: > 0 } u) { return new MasterClaim(u, validUntilUtc); }
        }

        return null;
    }

    /// <summary>
    /// Ein gelesener Master-Stempel aus dem Master-Chunk.
    /// </summary>
    /// <param name="User">Benutzer des eingetragenen temporären Masters.</param>
    /// <param name="ValidUntilUtc">UTC-Zeitpunkt, bis zu dem der Stempel gilt.</param>
    private sealed record MasterClaim(string User, DateTime ValidUntilUtc);

    /// <summary>
    /// Prüft, ob der Chunk für den angegebenen Chunk-Wert aktuell im Speicher
    /// geladen ist (geparst wurde).
    /// </summary>
    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = ChunkValueToId(chunkVal);
        return _processedFile.ContainsKey(chunkId);
    }

    /// <summary>
    /// Setzt den LastUsed-Stempel des Chunks mit dem angegebenen Namen.
    /// </summary>
    public override void Touch(string info) {
        if (info is not { Length: > 0 }) { return; }
        _lastUsed[ChunkValueToId(info)] = DateTime.UtcNow;
    }

    /// <summary>
    /// Stellt sicher, dass der Chunk mit dem angegebenen Namen geladen ist.
    /// </summary>
    public override OperationResult EnsureLoaded(string chunk, bool trustProcessedFile) =>
        LoadChunkWithChunkId(ChunkValueToId(chunk), trustProcessedFile);

    /// <summary>
    /// Prüft die Edit-Sperre des Chunks mit dem angegebenen Namen; ohne Namen
    /// wird der Haupt-Chunk geprüft.
    /// </summary>
    public override string IsEditable(string chunk) =>
        CheckEditLock(chunk is { Length: > 0 } ? ChunkValueToId(chunk) : Chunk_MainData.ToLowerInvariant());

    /// <summary>
    /// Tiefe Prüfung vor der Bearbeitung des Chunks mit dem angegebenen Namen:
    /// Edit-Sperre prüfen und den Chunk nachladen.
    /// </summary>
    public override string PrepareEdit(string chunk) {
        var chunkId = chunk is { Length: > 0 } ? ChunkValueToId(chunk) : Chunk_MainData.ToLowerInvariant();

        if (CheckEditLock(chunkId) is { Length: > 0 } f) { return f; }

        var result = LoadChunkWithChunkId(chunkId, false);

        return result.IsFailed ? result.FailedReason : string.Empty;
    }

    /// <summary>
    /// Löscht Chunk-Ordner, deren Chunk-ID für keinen geladenen Bereich mehr zutrifft.
    /// </summary>
    public override void Reorganize(string chunk) => DeleteOrphanedChunkFolders();

    /// <summary>
    /// Speichert alle Chunks direkt auf die Festplatte.
    /// Nach dem Speichern werden alle generierten Daten verworfen.
    /// Jeder Chunk ist ein Einweg — eine einmal gespeicherte Datei wird nie
    /// wieder überschrieben. Die Lite-Datei liegt als System-Chunk im
    /// Arbeitsverzeichnis (write-once, keine Nutzdaten).
    /// <para>
    /// Die Editierbarkeit wird PRO CHUNK geprüft: Chunks, die von einem anderen
    /// Benutzer aktiv gesperrt sind, werden übersprungen und ihre Änderungen
    /// verworfen. Alle bearbeitbaren Chunks werden normal gespeichert.
    /// </para>
    /// </summary>
    public override OperationResult Save() {
        if (!SaveRequired) { return OperationResult.Success; }
        if (IsDisposed) { return OperationResult.Failed("Objekt ist bereits freigegeben"); }

        PauseTimer();

        try {
            // Nur die Basis-Checks — NICHT die System-Chunks-Verifikation aus
            // CheckFileEditable. Jene würde die gesamte Speicherung blockieren,
            // sobald ein einziger System-Chunk (z.B. Master) gelocked ist.
            // Die Per-Chunk-Prüfung erfolgt in AddChunk.
            if (EditableError(false) is { Length: > 0 } f) { return OperationResult.Failed(f); }

            EnsureHeadFileExists();

            var x = LastChangeUtc;

            var timestamp = GenerateChunkTimestamp();

            // path → vollständiger Inhalt, fertig zum Schreiben
            var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);
            // chunkId (lowercase) → Hash des Voll-Inhalts, aktualisiert nach erfolgreichem Speichern
            var newHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // chunkIds mit echten Änderungen (für EditLock Re-Check vor dem Schreiben)
            var changedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // true, wenn mindestens ein Daten-Chunk durch die Edit-Sperre blockiert
            // wurde. In diesem Fall bleibt SaveRequired true, damit der nächste
            // Speicherversuch die ungespeicherten Änderungen erneut versucht.
            var dataChunksSkipped = false;

            void AddChunk(string chunkId) {
                var idLower = chunkId.ToLowerInvariant();
                var isMainLite = string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase);

                // Per-Chunk-Editierbarkeit: ist der Chunk durch einen anderen
                // Benutzer gesperrt, wird er übersprungen — die anderen Chunks
                // werden weiter gespeichert. Bei Daten-Chunks bleibt SaveRequired
                // aktiv, damit die ungespeicherten Änderungen beim nächsten Versuch
                // erneut gespeichert werden (und nicht beim nächsten Refresh
                // gelöscht werden).
                // MainLite ist eine write-once-Datei ohne Chunk-Ordner und wird
                // nicht über CheckEditLock geprüft.
                if (!isMainLite && !string.IsNullOrEmpty(CheckEditLock(chunkId))) {
                    if (IsDataChunk(chunkId)) { dataChunksSkipped = true; }
                    return;
                }

                // Vollständigen Inhalt aufbauen (inkl. Kopf und Abschluss-Marker)
                var fullContent = BuildChunkContent(chunkId);
                if (fullContent.Count == 0) { return; }

                var fullHash = Generic.GetSHA256HashString(fullContent.ToArray());
                newHashes[idLower] = fullHash;

                // Unchanged-Check: bei identischem Inhalt keine neue Datei schreiben
                if (_lastContentHash.TryGetValue(idLower, out var storedHash) && storedHash == fullHash) { return; }

                // Die Lite-Datei liegt als System-Chunk im Arbeitsverzeichnis; die
                // Kopf-Datei trägt nur den Layout-Kopf. MainLite läuft nicht über
                // die Edit-Sperre und zählt nicht als Änderung.
                chunkData[$"{GetChunkFolder(chunkId)}{timestamp}.tblc"] = fullContent;

                if (!isMainLite) { changedChunkIds.Add(idLower); }
            }

            AddChunk(Chunk_MainData);

            AddChunk(Chunk_MainDataLite);

            AddChunk(Chunk_AdditionalUseCases);

            AddChunk(Chunk_Variables);

            AddChunk(Chunk_Master);

            var dataChunkIds = KnownChunkIds();
            if (!dataChunkIds.Contains(Chunk_UnknownData)) {
                dataChunkIds.Add(Chunk_UnknownData);
            }

            foreach (var thisChunkId in dataChunkIds) {
                AddChunk(thisChunkId);
            }

            if (x != LastChangeUtc) { return OperationResult.Failed("Daten wurden während der Chunk-Generierung geändert"); }

            // Re-Check: Es wird vorab kein Claim geschrieben, daher kann ein anderer
            // Benutzer zwischenzeitlich einen der zu schreibenden Chunks gesperrt haben.
            // In diesem Fall Speichern abbrechen und einfrieren, statt fremde Änderungen zu
            // überschreiben. changedChunkIds enthält nur Chunks mit echten inhaltlichen Änderungen.
            foreach (var chunkId in changedChunkIds) {
                var lockCheck = CheckEditLock(chunkId);
                if (!string.IsNullOrEmpty(lockCheck)) {
                    // Spezialfall: Bestimmte System-Chunks führen NICHT zum Freeze.
                    // Hier werden die Änderungen einfach verworfen (Chunk wird nicht geschrieben).
                    if (string.Equals(chunkId, Chunk_AdditionalUseCases, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_UnknownData, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_Master, StringComparison.OrdinalIgnoreCase)) {
                        // Entferne den Pfad aus chunkData, damit er nicht geschrieben wird
                        var folderPrefix = GetChunkFolder(chunkId);
                        var keysToRemove = chunkData.Keys.Where(k => k.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (var key in keysToRemove) { chunkData.Remove(key); }
                        continue;
                    }

                    Freeze(lockCheck);
                    return OperationResult.Failed(lockCheck);
                }
            }

            // Direkt auf Festplatte schreiben.
            // Chunks sind write-once — jede Datei ist neu und wird nie überschrieben.
            Develop.Message(ErrorType.Info, null, KeyName, ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks von '{KeyName}'", 2);

            foreach (var kvp in chunkData) {
                var path = kvp.Key;

                if (IO.CreateDirectory(path.FilePath()).IsFailed) {
                    Freeze($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                    return OperationResult.Failed($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                }

                // Chunk-Dateien werden gezippt gespeichert
                var zipped = kvp.Value.ToArray().ZipIt();
                if (zipped is null || zipped.Length == 0) {
                    Freeze($"Komprimierung fehlgeschlagen für '{path}'");
                    return OperationResult.Failed($"Komprimierung fehlgeschlagen für '{path}'");
                }

                var writeResult = IO.WriteAllBytes(path, zipped);
                if (writeResult.IsFailed) {
                    Freeze(writeResult.FailedReason);
                    return OperationResult.Failed(writeResult.FailedReason);
                }

                // Nach dem Speichern: nur den Dateinamen tracken, Daten verwerfen
                var folder = path.FilePath();
                var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
                _processedFile[chunkId] = path;
                _lastUsed[chunkId] = DateTime.UtcNow;
                CleanupOldFilesInFolder(GetChunkFilesOrderedByTime(folder).ToArray());
            }

            // Hashes nach erfolgreichem Speichern aktualisieren
            foreach (var kvp2 in newHashes) {
                _lastContentHash[kvp2.Key] = kvp2.Value;
            }

            // SaveRequired nur zurücksetzen, wenn alle Daten-Chunks gespeichert
            // wurden. Wurden Chunks durch die Edit-Sperre blockiert, bleibt
            // SaveRequired true — der nächste Speicherversuch (Checker-Tick)
            // versucht es erneut, sobald die Sperre abgelaufen ist.
            SaveRequired = dataChunksSkipped;
            InitialSavePending = false;
            MainLoadDone = true; // die Chunks entsprechen jetzt dem Speicherstand

            return OperationResult.Success;
        } finally {
            OnInvalidateView();
            ResumeTimer();
        }
    }

    /// <summary>
    /// Lädt den Haupt-Chunk aus dem Arbeitsverzeichnis.
    /// </summary>
    protected override bool LoadMainDataCore() {
        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        var result = LoadChunkWithChunkId(Chunk_MainData, false);
        if (result.IsFailed) { return false; }

        // Bei geladenen Daten muss der Hauptchunk vorhanden sein.
        // Fehlt er (Ordner leer), sind die Daten korrupt.
        if (!_processedFile.ContainsKey(Chunk_MainData.ToLowerInvariant())) {
            Freeze("Hauptchunk der Daten fehlt");
            return false;
        }

        InitialSavePending = false; // vollständiger Stand aus dem Haupt-Chunk geladen
        return true;
    }

    /// <summary>
    /// Generiert den Timestamp-String für Chunk-Dateinamen im Format
    /// yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.
    /// </summary>
    internal string GenerateChunkTimestamp() =>
        $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}";

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Bereichs-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.Suffix).
    /// Millisekunden sind verpflichtend. Liefert null, wenn das Datum nicht geparst werden kann.
    /// </summary>
    internal static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 23) { return null; }
        var datePart = fileName[..23];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Löscht alle Chunk-Dateien in der übergebenen Liste, die älter als
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
    /// Extrahiert den 3-stelligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Gibt string.Empty zurück,
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
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Der angehängte
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

    protected static DateTime GetChunkDirLastWriteUtc(string dir) {
        try {
            return IO.DirectoryExists(dir) ? new DirectoryInfo(dir).LastWriteTimeUtc : DateTime.MaxValue;
        } catch {
            return DateTime.MaxValue;
        }
    }

    /// <summary>
    /// Listet alle Chunk-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
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
    /// [Datenordner]\[Name]\
    /// </summary>
    protected string BaseChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Prüft die Edit-Sperre für einen Chunk: Wenn die neueste Datei im Ordner
    /// jünger als EditLockSeconds ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    protected string CheckEditLock(string chunkId) {
        var folder = GetChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return string.Empty; }

        var fileDate = ExtractDateFromFileName(newestFile);
        if (!fileDate.HasValue) { return "Datums-Fehler in Dateien"; }

        if (DateTime.UtcNow.Subtract(fileDate.Value).TotalSeconds >= EditLockSeconds) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        var creatorHash = ExtractMachineInstanceHashFromFileName(newestFile);
        var creatorDisplay = string.IsNullOrEmpty(creatorHash) ? creator : $"{creator} - {creatorHash}";
        var lockUntil = fileDate.Value.AddSeconds(EditLockSeconds).ToLocalTime();
        return $"Der Chunk '{chunkId}' wird bis {lockUntil:HH:mm} Uhr von '{creatorDisplay}' bearbeitet";
    }

    /// <summary>
    /// Löscht Daten-Chunk-Ordner, deren Chunk-ID für keinen geladenen Bereich mehr zutrifft.
    /// Ein Ordner wird nur entfernt, wenn seine neueste Datei älter als
    /// DeleteOldFilesAfterHours ist — fremde Instanzen könnten sonst beim
    /// Speichern Daten hineinschreiben.
    /// </summary>
    protected void DeleteOrphanedChunkFolders() {
        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return; }

        var validIds = KnownChunkIds();
        var unknownId = Chunk_UnknownData.ToLowerInvariant();
        if (!validIds.Contains(unknownId)) { validIds.Add(unknownId); }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (!IsDataChunk(chunkId) || validIds.Contains(chunkId)) { continue; }

            // Nur überspringen, wenn die neueste Datei ein gültiges, zu frisches
            // Datum trägt. Leere Ordner und Ordner ohne gültige Datumsangaben
            // (kann kein frischer Fremd-Schreibvorgang sein) werden gelöscht.
            if (GetChunkFilesOrderedByTime(subDir).FirstOrDefault() is { } newestFile
                && ExtractDateFromFileName(newestFile) is { } newestDate
                && newestDate > threshold) { continue; }

            IO.DeleteDir(subDir, false);
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed.TryRemove(chunkId, out _);
            _lastContentHash.TryRemove(chunkId, out _);
        }
    }

    /// <summary>
    /// Listet verfügbare Daten-Chunk-IDs aus dem Dateisystem auf.
    /// </summary>
    protected List<string> GetAvailableChunkIds() {
        if (string.IsNullOrEmpty(Filename)) { return []; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return []; }

        // Ordnernamen (= Chunk-IDs) direkt als Chunk-Werte verwenden.
        var allChunkIds = new List<string>();
        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (IsDataChunk(chunkId)) { allChunkIds.Add(chunkId); }
        }

        return allChunkIds.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// [Datenordner]\[Name]\[Chunk]\
    /// </summary>
    protected string GetChunkFolder(string chunkId) => $"{BaseChunkFolder()}{chunkId.ToLowerInvariant()}\\";

    /// <summary>
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer erstellt wurde.
    /// Der Machine/Instance-Hash dient nur der Anzeige: Auch eine neue Instanz
    /// (neue Session, neuer Prozess) darf die eigenen Dateien ihres Benutzers
    /// weiterbearbeiten; die Edit-Sperre schützt ausschließlich vor fremden
    /// Benutzern.
    /// </summary>
    private bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        return string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True, wenn die Chunk-ID ein Daten-Chunk ist (kein System-Chunk).
    /// </summary>
    private static bool IsDataChunk(string chunkId) =>
        !string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(chunkId, Chunk_Master, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(chunkId, Chunk_Variables, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(chunkId, Chunk_AdditionalUseCases, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(chunkId, Chunk_UnknownData, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Lädt einen Chunk direkt von der Festplatte.
    /// Nach dem Parsen wird der Chunk-Inhalt verworfen — nur der Dateiname wird
    /// für Aktualitätsprüfungen gemerkt.
    /// </summary>
    /// <param name="chunkId">Chunk-ID (wird auf Lowercase normalisiert).</param>
    /// <param name="trustProcessedFile">True: bereits geladene Chunks (Eintrag in _processedFile)
    /// erzeugen keinen Verzeichniszugriff mehr — der erste Load bleibt unberührt,
    /// der Reload läuft ausschließlich über RefreshLoadedChunks. False: immer prüfen.</param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordnung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    protected OperationResult LoadChunkWithChunkId(string chunkId, bool trustProcessedFile) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        // Reload ignorieren: Der Chunk ist geladen und wird nicht erneut von der
        // Festplatte geprüft. Aktualität liefert RefreshLoadedChunks (Fenster).
        if (trustProcessedFile && _processedFile.ContainsKey(chunkId)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        var folder = GetChunkFolder(chunkId);

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer konnte zwischenzeitlich eine neuere Datei ablegen.
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
            // _lastUsed aktualisieren, damit nicht-existierende Chunks in Refresh
            // nicht permanent übersprungen werden, sondern nach SkipIfUnusedMinutes neu geprüft werden.
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
            // Alle Dateien des Chunks wurden gelöscht — Tracking-Eintrag entfernen,
            // damit veraltete Daten nicht als "aktuell" gelten.
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        // Raw bytes direkt von der Festplatte lesen
        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Lade Chunk '{chunkId}' von '{KeyName}'", 1);

        if (IO.ReadAllBytes(newestFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) {
            return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht gelesen werden");
        }

        // Für Parsing entpacken (Chunk-Dateien werden gezippt gespeichert)
        byte[] chunkContent;
        if (rawBytes.IsZipped()) {
            chunkContent = rawBytes.UnzipIt() ?? [];
            if (chunkContent.Length == 0) {
                return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht entpackt werden");
            }
        } else {
            chunkContent = rawBytes;
        }

        var isMain = string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase);

        if (!ParseChunkContent(chunkContent, chunkId, isMain)) {
            return OperationResult.Failed($"Chunk '{chunkId}' Parsen fehlgeschlagen");
        }

        // Chunk nach dem Parsen verwerfen — nur Dateiname und Hash tracken
        _processedFile[chunkId] = newestFile;
        _lastUsed[chunkId] = DateTime.UtcNow;
        _lastContentHash[chunkId] = Generic.GetSHA256HashString(chunkContent);

        CleanupOldFilesInFolder(chunkFiles);

        return new OperationResult(true);
    }

    /// <summary>
    /// Aktualisiert alle bekannten Daten-Chunks (Einträge in _processedFile):
    /// Prüft für jeden, ob eine neuere Datei im Ordner existiert und lädt
    /// diese ggf. neu. Neue Chunk-Ordner anderer Benutzer werden hier nicht
    /// entdeckt — das übernimmt GetAvailableChunkIds via LoadRows.
    /// Chunks, deren LastUsed-Stempel älter als SkipIfUnusedMinutes ist,
    /// werden übersprungen, sofern <paramref name="firstTime"/> false ist.
    /// Da Chunks write-once sind, reicht der Dateiname-Vergleich.
    /// </summary>
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        var loaded = false;

        // Gezielter Zugriff pro bekanntem Chunk (alle im Speicher befindlichen
        // Chunk-IDs aus _processedFile). Die neueste Datei wird direkt mit der
        // zuletzt verarbeiteten verglichen.
        foreach (var chunkId in _processedFile.Keys.ToList()) {
            // System-Chunks werden in Refresh vor diesem Refresh explizit geladen.
            if (!IsDataChunk(chunkId)) { continue; }

            // SkipIfUnusedMinutes-Fenster: Niemand schaut den Chunk mehr an — kein Refresh nötig.
            if (!firstTime && _lastUsed.TryGetValue(chunkId, out var lastUsed)
                && DateTime.UtcNow.Subtract(lastUsed).TotalMinutes >= SkipIfUnusedMinutes) { continue; }

            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            // Neueste Datei ermitteln und mit der zuletzt verarbeiteten vergleichen.
            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var processedFile = _processedFile[chunkId];

            // Reload wenn eine andere Datei die neueste ist. Da Chunks write-once
            // sind, bedeutet ein anderer Dateiname zwingend anderen Inhalt.
            if (string.Equals(processedFile, newestFile, StringComparison.OrdinalIgnoreCase)) { continue; }

            var result = LoadChunkWithChunkId(chunkId, false);
            if (result.IsFailed) {
                Develop.DebugPrint(ErrorType.Warning, $"Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
            }
            loaded = loaded || result.Value is true;
        }

        return loaded;
    }

    /// <summary>
    /// Lädt die System-Chunks einmalig und prüft auf Fehler.
    /// Das Ergebnis wird gecacht, damit die Editierbarkeits-Prüfung bei jedem Aufruf ohne
    /// Festplattenzugriff reagieren kann.
    /// </summary>
    protected string VerifySystemChunksEditable() {
        string[] checkIds = [Chunk_MainData, Chunk_Master, Chunk_Variables, Chunk_AdditionalUseCases];

        foreach (var id in checkIds) {
            var loadResult = LoadChunkWithChunkId(id, false);
            if (loadResult.IsFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}'\r\n({loadResult.FailedReason})"; }
        }

        return string.Empty;
    }

    #endregion
}