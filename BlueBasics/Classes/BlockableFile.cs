// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes;

/// <summary>
/// Abstrakte Basisklasse für gecachte Dateien, die zusätzlich per
/// <see cref="BlockFile"/> gegen parallele Bearbeitung durch andere Benutzer
/// gesperrt werden können.
/// </summary>
/// <remarks>
/// Verwaltet das Laden roher Bytes vom Dateisystem mit Lazy-Loading,
/// Versionierung und Thread-sicherheit (Double-Checked-Locking mit Semaphoren).
/// Zusätzlich wird ein statischer Polling-Timer betrieben, der alle
/// <see cref="PollingIntervalMinutes"/> Minuten prüft, ob sich die Datei auf der
/// Festplatte geändert hat, und ggf. <see cref="Invalidate"/> aufruft.
/// Dateien mit aktivem Schreibzugriff werden beim Polling übersprungen.
/// <para>
/// Das zentrale Lifecycle-Management (<see cref="SaveAll(bool, IEnumerable{BlockableFile})"/>
/// und <see cref="DisposeAll(IEnumerable{BlockableFile})"/>) erhält die zu behandelnden
/// Instanzen vom Aufrufer — die Ableitungen (z.B. ConnectedFormula) pflegen
/// dafür eigene Live-Register.
/// </para>
/// </remarks>
public abstract class BlockableFile : IDisposableExtended, IHasKeyName, IReadableText {

    #region Fields

    /// <summary>Prüfintervall des Polling-Timers in Minuten.</summary>
    private const int PollingIntervalMinutes = 5;

    /// <summary>
    /// Alle registrierten BlockableFile-Instanzen, geordnet nach normalisiertem Dateinamen.
    /// Wird vom Polling-Timer durchlaufen.
    /// </summary>
    private static readonly ConcurrentDictionary<string, BlockableFile> _registeredFiles = new(StringComparer.OrdinalIgnoreCase);

    private static readonly object _timerLock = new();

    /// <summary>
    /// Registry aller Instanzen, die aktuell erfolgreich Schreibrechte erworben haben.
    /// Schlüssel ist der Dateipfad (<see cref="Filename"/>).
    /// Wird genutzt, um <see cref="RevokeWriteAccessAll"/> beim Shutdown ohne Kenntnis
    /// des Caching-Systems auszuführen.
    /// </summary>
    private static readonly ConcurrentDictionary<string, BlockableFile> _writeAccessHolders = new(StringComparer.OrdinalIgnoreCase);

    private static Timer? _pollingTimer;

    /// <summary>
    /// Semaphore zum Synchronisieren von Ladevorgängen.
    /// Beim Laden von Disk erworben → IsLoading liefert true.
    /// </summary>
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    /// <summary>
    /// Synchronisierungsobjekt für Thread-sichere Zugriffe auf Dateiinhalte.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    /// <summary>
    /// Schreib-/Build-Puffer für abgeleitete Klassen. Alle Werte beziehen sich auf _content.
    /// Wenn gesetzt, liefert DataLength die Anzahl der gepufferten Bytes.
    /// </summary>
    private byte[]? _content;

    /// <summary>
    /// Der SHA256-Hash des aktuellen Arbeitsinhalts (_content).
    /// Wird analog zum Content bei Bedarf generiert.
    /// </summary>
    private string? _contentHash;

    /// <summary>
    /// Der SHA256-Hash des Inhalts, wie er zuletzt vom Dateisystem geladen oder dorthin gespeichert wurde.
    /// Dient dem Vergleich in IsSaved.
    /// </summary>
    private string? _contentOnDiskHash;

    private FileInfo? _fileInfo;

    /// <summary>
    /// true, wenn diese Instanz aktuell den Schreibzugriff (Blockdatei) hält.
    /// Wird vom Polling-Timer ausgewertet: Dateien mit Write Access werden nicht invalidiert.
    /// </summary>
    private volatile bool _hasWriteAccess;

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine neue BlockableFile-Instanz für den angegebenen Dateipfad,
    /// registriert sie im Polling-Register und startet bei Bedarf den statischen
    /// Polling-Timer. Ableitungen pflegen ihre Live-Instanzen in eigenen Registern,
    /// die dem Aufrufer von <see cref="SaveAll(bool, IEnumerable{BlockableFile})"/> bzw.
    /// <see cref="DisposeAll(IEnumerable{BlockableFile})"/> übergeben werden.
    /// </summary>
    /// <param name="filename">Vollständiger Dateipfad.</param>
    protected BlockableFile(string filename) {
        Filename = string.IsNullOrEmpty(filename) ? string.Empty : filename.NormalizeFile();

        if (!string.IsNullOrEmpty(Filename)) {
            _registeredFiles[Filename] = this;
            EnsurePollingTimerStarted();
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Ereignis, das beim Laden der Datei ausgelöst wird.
    /// </summary>
    public event EventHandler? Loaded;

    /// <summary>
    /// Ereignis, das nach erfolgreichem Speichern ausgelöst wird.
    /// </summary>
    public event EventHandler? Saved;

    #endregion

    #region Properties

    /// <summary>
    /// Gibt den logischen Dateiinhalt zurück (bei gezippten Dateien automatisch entpackt).
    /// Beim Setzen wird _isSaved auf false gesetzt.
    /// Erwirbt _loadSemaphore während des Ladevorgangs, sodass IsLoading korrekt true liefert.
    /// Nach einem Frisch-Ladevorgang wird OnLoaded() automatisch aufgerufen.
    /// </summary>
    public byte[] Content {
        get {
            if (IsDisposed) { return []; }

            lock (_lock) {
                if (!NeedsLoading() && _content is not null) { return _content; }
            }

            bool acquired = false;
            try {
                try {
                    acquired = _loadSemaphore.Wait(10000);
                } catch (ObjectDisposedException) {
                    return [];
                }

                if (IsDisposed) { return []; }

                if (!acquired) {
                    MarkLoadFailed();
                    lock (_lock) {
                        return _content ?? [];
                    }
                }

                lock (_lock) {
                    if (!NeedsLoading() && _content is not null) { return _content; }
                }

                return GetContentInternal();
            } catch {
                MarkLoadFailed();
                return [];
            } finally {
                if (acquired) {
                    try {
                        _loadSemaphore.Release();
                    } catch { }
                }
            }
        }
        set {
            lock (_lock) {
                if (_content is null && value is not null) {
                    DebugPrint(ErrorType.Warning, $"Content wird überschrieben, obwohl _content null ist. Datei: {Filename}");
                }

                if (ReferenceEquals(_content, value)) { return; }
                if (_content is not null && value is not null && _content.SequenceEqual(value)) { return; }

                //Diagnose("CF",$"CONTENT SET: {value?.Length ?? -1} bytes, was {_content?.Length ?? -1} {Filename.FileNameWithoutSuffix()}");
                _content = value;
                _contentHash = null;
                if (_contentOnDiskHash is null) { _contentOnDiskHash = string.Empty; }
                if (_fileInfo is null && !string.IsNullOrEmpty(Filename)) { _fileInfo = new FileInfo(Filename); }
            }
        }
    }

    /// <summary>
    /// Anzahl der aktuell verfügbaren Bytes:
    /// Im Build-Modus (_buildBuffer gesetzt): Länge des Build-Puffers.
    /// Im Lese-Modus: Länge des gecachten Contents.
    /// </summary>
    public long ContentLength => _content?.Length ?? 0;

    public abstract bool ExtendedSave { get; }

    public FileInfo? FileInfo {
        get {
            if (_fileInfo is null) {
                _fileInfo = GetFileInfo(Filename);
            }

            return _fileInfo;
        }
    }

    /// <summary>
    /// Der vollständige Dateipfad dieser gecachten Datei.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Flag zur Überwachung, ob die Instanz disposed wurde.
    /// </summary>
    public bool IsDisposed => _isDisposedFlag == 1;

    public bool IsLoading {
        get {
            if (IsDisposed) { return false; }

            try {
                if (!_loadSemaphore.Wait(0)) { return true; }
                _loadSemaphore.Release();
            } catch { }
            return false;
        }
    }

    /// <summary>
    /// Gibt an, ob der aktuelle Inhalt gespeichert ist.
    /// Vergleicht den Hash des Arbeitsinhalts mit dem Hash des zuletzt geladenen/gespeicherten Standes.
    /// </summary>
    public bool IsSaved {
        get {
            lock (_lock) {
                if (_content is null) { return _contentOnDiskHash is null; }
                if (_contentOnDiskHash is null) { return false; }
                _contentHash ??= Generic.GetSHA256HashString(_content);
                return _contentHash == _contentOnDiskHash;
            }
        }
    }

    public bool IsSaving {
        get {
            if (IsDisposed) { return false; }

            try {
                if (!_saveSemaphore.Wait(0)) { return true; }
                _saveSemaphore.Release();
            } catch { }
            return false;
        }
    }

    /// <summary>
    /// IHasKeyName: Entspricht dem Dateinamen in UCASE
    /// </summary>
    public virtual string KeyName => field ??= Filename.ToUpperInvariant();

    /// <summary>
    /// Gibt an, ob das Laden der Datei fehlgeschlagen ist.
    /// Wird auch gesetzt, wenn der geladene Inhalt kleiner als MinimumBytes ist.
    /// </summary>
    public bool LoadFailed { get; protected set; }

    /// <summary>
    /// Mindestgröße des Inhalts in Bytes.
    /// IsSaveAbleNow und der Ladevorgang prüfen, ob der Inhalt diese Grenze erfüllt.
    /// Wird von abgeleiteten Klassen nach erfolgreichem Laden/Speichern gesetzt.
    /// </summary>
    public int MinimumBytes { get; protected set; }

    /// <summary>
    /// Gibt an, ob der Inhalt beim Speichern automatisch gezippt werden soll.
    /// Beim Laden wird unabhängig davon geprüft, ob die Datei gezippt ist.
    /// </summary>
    public abstract bool MustZipped { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Disposed die angegebenen BlockableFile-Instanzen.
    /// </summary>
    /// <param name="files">Die zu verwerfenden BlockableFile-Instanzen (z. B. aus
    /// <c>ConnectedFormula.LiveInstances</c>).</param>
    public static void DisposeAll(IEnumerable<BlockableFile> files) {
        foreach (var file in files) {
            try { file.Dispose(); } catch { }
        }
    }

    /// <summary>
    /// Widerruft alle aktiven Schreibzugriffe dieser Instanzen.
    /// Wird beim Herunterfahren der Anwendung aufgerufen, damit keine
    /// Blockdateien auf der Festplatte liegen bleiben.
    /// </summary>
    public static void RevokeWriteAccessAll() {
        var snapshot = _writeAccessHolders.Values.ToList();
        Develop.EndLog($"RevokeWriteAccessAll: {snapshot.Count} Lock(s) zu entfernen");
        var done = 0;
        foreach (var mu in snapshot) {
            done++;
            Develop.EndLog($"RevokeWriteAccessAll: [{done}/{snapshot.Count}] Vor RevokeWriteAccess '{mu.Filename}'");
            mu.RevokeWriteAccess();
            Develop.EndLog($"RevokeWriteAccessAll: [{done}/{snapshot.Count}] Nach RevokeWriteAccess '{mu.Filename}'");
        }
        Develop.EndLog("RevokeWriteAccessAll: ENDE");
    }

    /// <summary>
    /// Speichert die angegebenen BlockableFile-Instanzen.
    /// </summary>
    /// <param name="mustWait">
    /// Falls FALSE: Asynchrones Speichern anstoßen und NICHT warten.
    /// Falls TRUE: Asynchrones Speichern anstoßen und WARTEN, bis alle fertig sind.
    /// </param>
    /// <param name="files">Die zu speichernden BlockableFile-Instanzen (z. B. aus
    /// <c>ConnectedFormula.LiveInstances</c>).</param>
    public static void SaveAll(bool mustWait, IEnumerable<BlockableFile> files) {
        var tasks = new List<Task>();
        foreach (var file in files) {
            if (file.IsDisposed) { continue; }
            if (!file.IsSaved) {
                if (file.IsSaveAbleNow() is { Length: 0 }) {
                    tasks.Add(file.Save());
                } else if (mustWait && file.IsSaving) {
                    tasks.Add(Task.Run(file.WaitDiskOperationFinished));
                }
            }
        }

        if (tasks.Count > 0) {
            Message(ErrorType.Info, null, "Dateien", ImageCode.Diskette, $"Speichere {tasks.Count} Datei(en) auf die Festplatte", 3);

            if (mustWait) {
                EndLog($"BlockableFile.SaveAll(true): Warte auf {tasks.Count} Task(s) (max 60s)");
                try {
                    Task.WaitAll(tasks.ToArray(), 60000);
                    EndLog("BlockableFile.SaveAll(true): Task.WaitAll zurück");
                } catch {
                    EndLog("BlockableFile.SaveAll(true): Task.WaitAll hat Exception geworfen");
                }
            }

            Message(ErrorType.Info, null, "Dateien", ImageCode.Häkchen, $"{tasks.Count} Datei(en) gespeichert", 3);
        } else {
            EndLog("BlockableFile.SaveAll: keine anstehenden Tasks");
        }
    }

    /// <summary>
    /// Stoppt den Polling-Timer und leert das Instanz-Register.
    /// Wird beim Herunterfahren der Anwendung aufgerufen.
    /// </summary>
    public static void StopPolling() {
        lock (_timerLock) {
            _pollingTimer?.Dispose();
            _pollingTimer = null;
        }
        _registeredFiles.Clear();
    }

    /// <summary>
    /// Erwirbt den Schreibzugriff für diese Datei.
    /// Prüft die Blockdatei, erstellt sie bei Bedarf und merkt sich den Zustand.
    /// Gibt eine Fehlermeldung zurück, wenn ein anderer Benutzer die Datei sperrt;
    /// <see cref="string.Empty"/> bei Erfolg.
    /// </summary>
    public string AcquireWriteAccess() {
        if (Develop.AllReadOnly) { return string.Empty; }

        if (!IsMyLock()) {
            var blocker = BlockFile.BlockerMessage(Filename);
            if (blocker.Length > 0) { return blocker; }

            BlockFile.AcquireWriteAccessFor(Filename);
            if (!IsMyLock()) { return "Schreibrecht konnte nicht erworben werden"; }
        }

        _hasWriteAccess = true;
        _writeAccessHolders.TryAdd(Filename, this);
        return string.Empty;
    }

    /// <summary>
    /// Liefert eine Blocker-Meldung, falls ein anderer Benutzer die Datei sperrt;
    /// <see cref="string.Empty"/> wenn frei.
    /// </summary>
    public string BlockerMessage() => BlockFile.BlockerMessage(Filename);

    /// <summary>
    /// Disposed alle zugeordneten Ressourcen. Trägt die Instanz aus dem
    /// Polling-Register aus, gibt einen eventuell gehaltenen Schreibzugriff frei
    /// (dabei ggf. Save via OnReleasingWriteAccess) und schließt die Datei-Verwaltung ab.
    /// </summary>
    public virtual void Dispose() {
        // Cleanup, das passieren muss, während IsDisposed noch false ist
        // (RevokeWriteAccess kann über OnReleasingWriteAccess Save auslösen).
        if (!IsDisposed) {
            _registeredFiles.TryRemove(Filename, out _);
            if (_hasWriteAccess) { RevokeWriteAccess(); }
        }

        // Ab hier thread-sicher und idempotent.
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        Loaded = null;
        Saved = null;

        Invalidate();

        // Auf laufende I/O-Vorgänge warten, BEVOR die Semaphoren disposed werden.
        // Wir nutzen hier Try-Catch, falls Threads noch in WaitDiskOperationFinished hängen.
        try {
            if (_loadSemaphore.Wait(30000)) {
                _loadSemaphore.Release();
            }
        } catch { }

        try {
            if (_saveSemaphore.Wait(30000)) {
                _saveSemaphore.Release();
            }
        } catch { }

        GC.SuppressFinalize(this);

        // Semaphoren erst nach einer kurzen Karenzzeit disposen oder wenn sicher ist, dass kein Thread mehr wartet.
        // In hochfrequenten Systemen ist es oft sicherer, die Semaphoren dem GC zu überlassen,
        // wenn ObjectDisposedExceptions in anderen Threads drohen.
        try {
            _loadSemaphore.Dispose();
            _saveSemaphore.Dispose();
        } catch {
            // Ignorieren - Dispose-Fehler sind nicht kritisch
        }
    }

    /// <summary>
    /// Stellt sicher, dass der Inhalt vom Dateisystem geladen ist.
    /// Triggert den Lazy-Load über den Content-Getter (synchron, thread-sicher).
    /// </summary>
    /// <returns>True wenn erfolgreich geladen, false bei Fehler oder Disposed.</returns>
    public bool EnsureContentLoaded() {
        if (IsDisposed) { return false; }
        //Diagnose("CF",$"ENSURE CONTENT: {Filename.FileNameWithoutSuffix()} _content={_content?.Length ?? -1} LoadFailed={LoadFailed}");
        _ = Content;
        //Diagnose("CF",$"ENSURE CONTENT DONE: {Filename.FileNameWithoutSuffix()} _content={_content?.Length ?? -1} LoadFailed={LoadFailed}");
        return !LoadFailed;
    }

    /// <summary>
    /// Invalidiert den gecachten Inhalt, damit er beim nächsten Zugriff neu geladen wird.
    /// Setzt IsParsed auf false — die Ableitung muss ihre Daten neu verarbeiten.
    /// Setzt auch _isSaved auf true (lokale Änderungen gelten als verworfen).
    /// </summary>
    public virtual void Invalidate() {
        lock (_lock) {
            _fileInfo = null;
            _content = null;
            _contentHash = null;
            _contentOnDiskHash = null;
        }
    }

    /// <summary>
    /// Prüft, ob der aktuelle Prozess die aktive Sperre für diese Datei hält.
    /// </summary>
    public bool IsMyLock() => BlockFile.IsMyLockFor(Filename);

    /// <summary>
    /// Prüft, ob die Datei aktuell bearbeitet werden darf.
    /// Berücksichtigt: IsDisposed, LoadFailed, Ladezustand,
    /// Schreibrecht auf der Festplatte und Multi-User-Sperre via Blockdatei.
    /// </summary>
    public virtual string IsNowEditable() {
        if (IsDisposed) { return "Verworfen."; }
        if (LoadFailed) { return "Datei wurde nicht korrekt geladen."; }
        if (NeedsLoading()) {
            if (!EnsureContentLoaded()) { return "Datei wurde nicht korrekt geladen."; }
            if (NeedsLoading()) { return "Daten müssen neu geladen werden."; }
        }
        if (IsLoading) { return "Daten werden geladen."; }
        if (_contentOnDiskHash is null) { return "Interner Fehler."; }

        var r = CanWriteFile(Filename, 2);
        if (r.IsFailed) { return r.FailedReason; }

        return BlockerMessage();
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Berücksichtigt: IsDisposed, LoadFailed, MinimumBytes.
    /// </summary>
    public virtual string IsSaveAbleNow() {
        if (IsNowEditable() is { Length: > 0 } f) { return f; }

        if (_content is null || _content.Length < MinimumBytes) { return "Byte-Fehler"; }
        return string.Empty;
    }

    /// <summary>
    /// Prüft, ob die Datei im Dateisystem geändert wurde.
    /// </summary>
    public bool IsStale() {
        lock (_lock) {
            if (NeedsLoading()) { return true; }
        }

        var newFileInfo = GetFileInfo(Filename, false, 0.1f);

        // Wenn die Datei nicht mehr existiert, ist der Cache definitiv veraltet (stale)
        if (newFileInfo is null) { return true; }

        lock (_lock) {
            if (_fileInfo is null) { return true; }
            try {
                return _fileInfo.Length != newFileInfo.Length ||
                       _fileInfo.LastWriteTime != newFileInfo.LastWriteTime;
            } catch {
                return true;
            }
        }
    }

    /// <summary>
    /// Markiert den Chunk als fehlgeschlagen geladen.
    /// </summary>
    public void MarkLoadFailed() { LoadFailed = true; }

    public bool NeedsLoading() {
        if (_fileInfo is null) { return true; }
        if (_contentOnDiskHash is null) { return true; }
        if (_content is null) { return true; }
        return false;
    }

    /// <summary>
    /// Wird aufgerufen, bevor die Blockdatei freigegeben wird.
    /// Standard: speichert ungespeicherte Änderungen.
    /// </summary>
    public virtual void OnReleasingWriteAccess() {
        if (!IsSaved) { _ = Save(); }
    }

    /// <summary>
    /// Menschenlesbarer Name dieser Datei für Statusmeldungen (z. B. "Speichere ...").
    /// Muss von konkreten Ableitungen implementiert werden.
    /// </summary>
    public abstract string ReadableText();

    /// <summary>
    /// Gibt den Schreibzugriff frei. Speichert ungespeicherte Änderungen
    /// über <see cref="OnReleasingWriteAccess"/> und entfernt die Blockdatei.
    /// </summary>
    public void RevokeWriteAccess() {
        _hasWriteAccess = false;
        BlockFile.RevokeFor(Filename, OnReleasingWriteAccess);
        _writeAccessHolders.TryRemove(Filename, out _);
    }

    /// <summary>
    /// Speichert den Inhalt asynchron mit optionaler Backup-Rotation und Semaphore-Synchronisierung.
    /// Nicht überschreibbar — nutze OnSaved() für Aktionen nach erfolgreichem Speichern.
    /// </summary>
    /// <returns>Ein OperationResult, das über Erfolg oder Fehler informiert.</returns>
    public async Task<OperationResult> Save() {
        if (IsDisposed) { return OperationResult.Failed("Objekt bereits verworfen."); }

        bool acquired = false;
        try {
            acquired = _saveSemaphore.Wait(0);
            if (!acquired) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }
        } catch (ObjectDisposedException) {
            return OperationResult.Failed("Objekt wurde während des Zugriffs verworfen.");
        }

        try {
            if (IsSaveAbleNow() is { Length: > 0 } f) {
                //Diagnose("CF",$"SAVE: NOT SAVEABLE: {f} {Filename.FileNameWithoutSuffix()}");
                return OperationResult.Failed(f);
            }

            var sw = Stopwatch.StartNew();
            Message(ErrorType.DevelopInfo, this, Filename.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {ReadableText()}", 0);

            byte[] contentToWrite;
            string savedContentHash;

            lock (_lock) {
                if (_content is null || _content.Length == 0) {
                    //Diagnose("CF",$"SAVE: NO DATA {Filename.FileNameWithoutSuffix()}");
                    return OperationResult.Failed("Keine Daten zum Speichern");
                }

                contentToWrite = MustZipped ? (_content.ZipIt() ?? []) : _content;
                savedContentHash = Generic.GetSHA256HashString(_content);
            }

            if (contentToWrite.Length == 0) {
                return OperationResult.Failed(MustZipped ? "Komprimierung fehlgeschlagen" : "Keine Daten zum Speichern");
            }

            //Diagnose("CF",$"SAVE: start {contentToWrite.Length} bytes {Filename.FileNameWithoutSuffix()}");

            var result = await (ExtendedSave
                ? Task.Run(() => SaveExtended(contentToWrite, savedContentHash))
                : Task.Run(() => SaveSimple(contentToWrite, savedContentHash))).ConfigureAwait(false);

            //Diagnose("CF",$"SAVE: {(result.IsSuccessful ? "OK" : result.FailedReason)} in {sw.ElapsedMilliseconds}ms {Filename.FileNameWithoutSuffix()}");
            return result;
        } finally {
            if (acquired) {
                try {
                    _saveSemaphore.Release();
                } catch { }
            }
        }
    }

    /// <summary>
    /// Speichert den Inhalt unter einem neuen Dateinamen (synchron).
    /// </summary>
    /// <returns>True wenn erfolgreich.</returns>
    public OperationResult SaveAs(string filename) {
        if (IsDisposed) { return OperationResult.Failed("Objekt bereits verworfen."); }
        if (string.IsNullOrEmpty(filename)) { return OperationResult.Failed("Kein Dateiname angegeben."); }

        bool acquired = false;
        try {
            acquired = _saveSemaphore.Wait(0);
            if (!acquired) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }
        } catch (ObjectDisposedException) {
            return OperationResult.Failed("Objekt wurde während des Zugriffs verworfen.");
        }

        try {
            if (IsSaveAbleNow() is { Length: > 0 } f) { return OperationResult.Failed(f); }

            byte[] data;
            lock (_lock) {
                data = GetContentInternal();
            }

            if (data.Length == 0) { return OperationResult.Failed("Keine Daten zum Speichern"); }

            var dataToWrite = MustZipped ? data.ZipIt() : data;
            if (dataToWrite is null || dataToWrite.Length == 0) { return OperationResult.Failed(MustZipped ? "Komprimierung fehlgeschlagen" : "Keine Daten zum Speichern"); }

            var backup = IO.BackupName(filename);
            var tmpFile = TempFile($"{filename.FilePath()}{filename.FileNameWithoutSuffix()}.tmp-{UserName.ToUpperInvariant()}");

            var result = WriteAllBytes(tmpFile, dataToWrite);
            if (result.IsFailed) {
                DeleteFile(tmpFile, false);
                return result;
            }

            if (FileExists(backup)) { DeleteFile(backup, false); }
            if (FileExists(filename)) { MoveFile(filename, backup, false); }

            if (!MoveFile(tmpFile, filename, false)) {
                if (FileExists(backup) && !FileExists(filename)) { MoveFile(backup, filename, false); }
                DeleteFile(tmpFile, false);
                return OperationResult.Failed("Speichervorgang fehlgeschlagen");
            }

            return OperationResult.Success;
        } finally {
            if (acquired) {
                try {
                    _saveSemaphore.Release();
                } catch (ObjectDisposedException) { } catch (SemaphoreFullException) { }
            }
        }
    }

    public virtual QuickImage? SymbolForReadableText() => null;

    public override string ToString() => $"{GetType().Name}: {Filename}";

    /// <summary>
    /// Wartet, bis alle laufenden Lade- und Speichervorgänge abgeschlossen sind.
    /// </summary>
    public void WaitDiskOperationFinished() {
        if (IsDisposed) { return; }

        try {
            // Load Semaphore abarbeiten
            if (_loadSemaphore.Wait(30000)) {
                try { _loadSemaphore.Release(); } catch { }
            }

            // Save Semaphore abarbeiten
            if (_saveSemaphore.Wait(30000)) {
                try { _saveSemaphore.Release(); } catch { }
            }
        } catch (ObjectDisposedException) {
            // Falls während des Wartens Dispose aufgerufen wurde
        }
    }

    /// <summary>
    /// Ruft das Loaded-Ereignis auf.
    /// Kann von Ableitungen überschrieben werden, um auf Ladeabschluss zu reagieren.
    /// Wird automatisch nach jedem Frisch-Ladevorgang durch den Content-Getter aufgerufen.
    /// </summary>
    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Wird nach erfolgreichem Speichern aufgerufen (nachdem _isSaved = true gesetzt wurde).
    /// Ableitungen können hier interne Zustände nach dem Speichern aktualisieren.
    /// </summary>
    protected virtual void OnSaved() => Saved?.Invoke(this, System.EventArgs.Empty);

    private static void EnsurePollingTimerStarted() {
        if (_pollingTimer is not null) { return; }
        lock (_timerLock) {
            if (_pollingTimer is not null) { return; }
            _pollingTimer = new Timer(OnPollingTick, null,
                TimeSpan.FromMinutes(PollingIntervalMinutes),
                TimeSpan.FromMinutes(PollingIntervalMinutes));
        }
    }

    /// <summary>
    /// Wird vom Polling-Timer alle <see cref="PollingIntervalMinutes"/> Minuten aufgerufen.
    /// Prüft alle registrierten Dateien auf externe Änderungen und invalidiert
    /// veraltete Instanzen. Dateien mit aktivem Schreibzugriff werden übersprungen.
    /// </summary>
    private static void OnPollingTick(object? state) {
        foreach (var kvp in _registeredFiles) {
            var file = kvp.Value;

            if (file.IsDisposed) {
                _registeredFiles.TryRemove(kvp.Key, out _);
                continue;
            }

            if (file._hasWriteAccess) { continue; }

            try {
                if (file.IsStale()) {
                    file.Invalidate();
                }
            } catch { }
        }
    }

    /// <summary>
    /// Interne Logik zum Laden/Abrufen des Contents ohne Semaphore-Wait.
    /// </summary>
    private byte[] GetContentInternal() {
        lock (_lock) {
            if (!NeedsLoading() && _content is not null) { return _content; }
        }

        var sw = Stopwatch.StartNew();
        var (content, timestamp, loadFailed) = ReadContentFromFileSystem();
        var processedContent = content;
        var finalLoadFailed = loadFailed;

        var effectiveTimestamp = timestamp ?? new FileInfo(Filename);

        if (!loadFailed && content.Length > 0) {
            if (content.IsZipped()) {
                var unzipped = content.UnzipIt();
                if (unzipped is null) {
                    finalLoadFailed = true;
                    processedContent = [];
                } else {
                    processedContent = unzipped;
                }
            }

            if (MinimumBytes > 0 && !finalLoadFailed && processedContent.Length < MinimumBytes) {
                finalLoadFailed = true;
                processedContent = [];
            }
        }

        var newDiskHash = Generic.GetSHA256HashString(processedContent);

        lock (_lock) {
            LoadFailed = finalLoadFailed;
            _fileInfo = effectiveTimestamp;
            _content = processedContent;
            _contentOnDiskHash = newDiskHash;
            _contentHash = newDiskHash;
        }

        if (!IsDisposed) { OnLoaded(); }
        return processedContent;
    }

    private (byte[] Content, FileInfo? FileInfo, bool LoadFailed) ReadContentFromFileSystem() {
        try {
            var retries = 0;
            do {
                var fileInfo1 = GetFileInfo(Filename, false, 0.1f);
                if (fileInfo1 is null) { return ([], null, false); }

                var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
                var fileInfo2 = GetFileInfo(Filename, false, 2f);
                if (fileInfo2 is not null &&
                    fileInfo1.LastWriteTime == fileInfo2.LastWriteTime &&
                    fileInfo1.Length == fileInfo2.Length) { return (content, fileInfo2, false); }

                retries++;
            } while (retries < 20);

            return ([], null, true); // Datei ändert sich ständig, Laden fehlgeschlagen
        } catch {
            return ([], null, true);
        }
    }

    /// <summary>
    /// Erweiterter Speicherpfad: Delegiert die Dateioperationen (Backup-Rotation,
    /// atomares Ersetzen) an <see cref="IO.SaveExtended"/> und aktualisiert danach
    /// die Instanz-Zustände (Hashes, FileInfo) sowie löst <see cref="OnSaved"/> aus.
    /// Für Dateien mit ExtendedSave = true.
    /// </summary>
    private OperationResult SaveExtended(byte[] contentToWrite, string savedContentHash) {
        try {
            if (IsDisposed) { return OperationResult.Failed("Vorgang abgebrochen, da Objekt verworfen."); }

            var result = IO.SaveExtended(Filename, contentToWrite);
            if (result.IsFailed) { return result; }

            lock (_lock) {
                _contentOnDiskHash = savedContentHash;
                _contentHash = savedContentHash;
                _fileInfo = GetFileInfo(Filename);
            }

            OnSaved();

            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }
    }

    /// <summary>
    /// Vereinfachter Speicherpfad: Direktes Schreiben ohne Backup-Rotation und ohne CanWriteFile-Retries.
    /// Für Dateien mit ExtendedSave = false.
    /// </summary>
    private OperationResult SaveSimple(byte[] contentToWrite, string savedContentHash) {
        try {
            if (IsDisposed) { return OperationResult.Failed("Vorgang abgebrochen, da Objekt verworfen."); }

            File.WriteAllBytes(Filename, contentToWrite);

            lock (_lock) {
                _contentOnDiskHash = savedContentHash;
                _contentHash = savedContentHash;
                _fileInfo = GetFileInfo(Filename);
            }

            OnSaved();

            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }
    }

    #endregion
}