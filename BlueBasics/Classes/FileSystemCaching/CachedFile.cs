// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using static BlueBasics.ClassesStatic.Develop;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Abstrakte Basisklasse für alle gecachten Dateitypen.
/// Verwaltet das Laden roher Bytes vom Dateisystem mit Lazy-Loading und Versionierung.
/// Thread-sicher durch Double-Checked-Locking mit Semaphore.
/// Instanzen dürfen nur über CachedFileSystem erzeugt werden.
/// </summary>
public abstract class CachedFile : IDisposableExtended, IHasKeyName, IReadableText {

    #region Fields

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

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine neue CachedFile-Instanz für den angegebenen Dateipfad.
    /// Nur über CachedFileSystem.CreateCachedFile() (via Activator.CreateInstance) aufrufbar.
    /// </summary>
    /// <param name="filename">Vollständiger Dateipfad.</param>
    protected CachedFile(string filename) {
        Filename = string.IsNullOrEmpty(filename) ? string.Empty : filename.NormalizeFile();
        CachedFileSystem.AutoRegister(this);
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

            //Diagnose("CF",$"CONTENT GET: start lazy-load {Filename.FileNameWithoutSuffix()}");

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
                    //Diagnose("CF",$"CONTENT GET: SEMAPHORE TIMEOUT {Filename.FileNameWithoutSuffix()}");
                    lock (_lock) {
                        return _content ?? [];
                    }
                }

                lock (_lock) {
                    if (!NeedsLoading() && _content is not null) { return _content; }
                }

                var sw = Stopwatch.StartNew();
                var result = GetContentInternal();
                //Diagnose("CF",$"CONTENT GET: loaded {result.Length} bytes in {sw.ElapsedMilliseconds}ms {Filename.FileNameWithoutSuffix()}");
                return result;
            } catch {
                MarkLoadFailed();
                //Diagnose("CF",$"CONTENT GET: EXCEPTION {ex.Message} {Filename.FileNameWithoutSuffix()}");
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
    /// Der FreezedReason kann niemals wieder rückgängig gemacht werden.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    /// <summary>
    /// Flag zur Überwachung, ob die Instanz disposed wurde.
    /// </summary>
    public bool IsDisposed => _isDisposedFlag == 1;

    /// <summary>
    /// Gibt an, ob die Datei eingefroren ist.
    /// </summary>
    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    public bool IsLoading {
        get {
            if (IsDisposed || IsFreezed) { return false; }

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
            if (IsDisposed || IsFreezed) { return false; }

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
    /// Disposed alle zugeordneten Ressourcen.
    /// </summary>
    public virtual void Dispose() {
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
    /// Friert die Datei ein. Kann nicht rückgängig gemacht werden.
    /// </summary>
    public void Freeze(string reason) {
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    /// <summary>
    /// Invalidiert den gecachten Inhalt, damit er beim nächsten Zugriff neu geladen wird.
    /// Setzt IsParsed auf false — die Ableitung muss ihre Daten neu verarbeiten.
    /// Setzt auch _isSaved auf true (lokale Änderungen gelten als verworfen).
    /// </summary>
    public virtual void Invalidate() {
        lock (_lock) {
            //Diagnose("CF",$"INVALIDATE: {Filename.FileNameWithoutSuffix()} _content={_content?.Length ?? -1}");
            _fileInfo = null;
            _content = null;
            _contentHash = null;
            _contentOnDiskHash = null;
        }
    }

    public virtual string IsNowEditable() {
        if (IsDisposed) { return "Verworfen."; }
        if (IsFreezed) { return FreezedReason; }
        if (LoadFailed) { return "Datei wurde nicht korrekt geladen."; }
        if (NeedsLoading()) {
            if (!EnsureContentLoaded()) { return "Datei wurde nicht korrekt geladen."; }
            if (NeedsLoading()) { return "Daten müssen neu geladen werden."; }
        }
        if (IsLoading) { return "Daten werden geladen."; }
        if (_contentOnDiskHash is null) { return "Interner Fehler."; }

        var r = CanWriteFile(Filename, 2);
        return r.IsSuccessful ? string.Empty : r.FailedReason;
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Berücksichtigt: IsFreezed, IsDisposed, LoadFailed, MinimumBytes.
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
    /// Menschenlesbarer Name dieser Datei für Statusmeldungen (z. B. "Speichere ...").
    /// Muss von konkreten Ableitungen implementiert werden.
    /// </summary>
    public abstract string ReadableText();

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

            var backup = BackupName(filename);
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

        var sw = Stopwatch.StartNew();
        try {
            // Load Semaphore abarbeiten
            if (_loadSemaphore.Wait(30000)) {
                try { _loadSemaphore.Release(); } catch { }
            }

            // Save Semaphore abarbeiten
            if (_saveSemaphore.Wait(30000)) {
                try { _saveSemaphore.Release(); } catch { }
            }

            if (sw.ElapsedMilliseconds > 50) {
                //Diagnose("CF",$"WAIT DISK: slow {sw.ElapsedMilliseconds}ms {Filename.FileNameWithoutSuffix()}");
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

    /// <summary>
    /// Berechnet den Backup-Dateinamen im Format "originaldatei.suffix.bak".
    /// Beispiel: "daten.mbdb" → "daten.mbdb.bak"
    /// </summary>
    private static string BackupName(string filename) => $"{filename}.bak";

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

        //Diagnose("CF",$"GET INTERNAL: raw={content.Length} bytes, fileExists={timestamp is not null}, loadFailed={loadFailed} in {sw.ElapsedMilliseconds}ms {Filename.FileNameWithoutSuffix()}");

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
    /// Erweiterter Speicherpfad: Schreibt zuerst lokal (SSD), rotiert dann über Backup zur Hauptdatei im Netzwerk.
    /// Minimiert Netzwerk-Latenz-Probleme während des Schreibvorgangs.
    /// </summary>
    private OperationResult SaveExtended(byte[] contentToWrite, string savedContentHash) {
        var backup = BackupName(Filename);
        var tempfile = Path.GetTempFileName();
        try {
            if (IsDisposed) { return OperationResult.Failed("Vorgang abgebrochen, da Objekt verworfen."); }

            CachedFileSystem.BeginIgnoreFile(Filename);
            CachedFileSystem.BeginIgnoreFile(backup);

            // 1. Lokal schreiben (schnell, sicher vor Netzwerk-Abbrüchen)
            var result = WriteAllBytes(tempfile, contentToWrite);
            if (result.IsFailed) { return result; }
            //// File.Replace führt Replace+Backup in einem atomaren Syscall aus.
            //// Schneller und sicherer als DeleteFile + MoveFile + MoveFile.
            //if (!FileExists(Filename)) {
            //    if (!MoveFile(tempfile, Filename, false)) {
            //        DeleteFile(tempfile, false);
            //        return OperationResult.Failed("Speichervorgang fehlgeschlagen");
            //    }
            //}

            try {
                // 2. Atomares Ersetzen im Netzwerk
                File.Replace(tempfile, Filename, backup, ignoreMetadataErrors: true);
            } catch {
                // Fallback bei exotischen Filesystemen, die File.Replace nicht unterstützen
                // Zuerst altes Backup entfernen, falls vorhanden
                if (FileExists(backup) && !FileExists(Filename)) {
                    DeleteFile(tempfile, false);
                    return OperationResult.Failed("Hauptdatei fehlt!");
                }

                if (FileExists(backup) && !DeleteFile(backup, false)) {
                    return OperationResult.Failed("Backup konnte nicht gelöscht werden");
                }

                if (FileExists(Filename) && !MoveFile(Filename, backup, false)) {
                    return OperationResult.Failed("Hauptdatei konnte nicht verschoben werden");
                }

                if (!MoveFile(tempfile, Filename, false)) {
                    if (FileExists(backup) && !FileExists(Filename)) {
                        MoveFile(backup, Filename, false);
                    }
                    return OperationResult.Failed("Speichervorgang fehlgeschlagen");
                }
            }

            lock (_lock) {
                _contentOnDiskHash = savedContentHash;
                _contentHash = savedContentHash;
                _fileInfo = GetFileInfo(Filename);
            }

            OnSaved();
            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        } finally {
            DeleteFile(tempfile, false);
            CachedFileSystem.EndIgnoreFile(Filename);
            CachedFileSystem.EndIgnoreFile(backup);
        }
    }

    /// <summary>
    /// Vereinfachter Speicherpfad: Direktes Schreiben ohne Backup-Rotation und ohne CanWriteFile-Retries.
    /// Für Dateien mit ExtendedSave = false.
    /// </summary>
    private OperationResult SaveSimple(byte[] contentToWrite, string savedContentHash) {
        try {
            if (IsDisposed) { return OperationResult.Failed("Vorgang abgebrochen, da Objekt verworfen."); }

            CachedFileSystem.BeginIgnoreFile(Filename);

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
        } finally {
            CachedFileSystem.EndIgnoreFile(Filename);
        }
    }

    #endregion
}