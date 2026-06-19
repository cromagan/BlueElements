// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Collections.Concurrent;
using static BlueBasics.ClassesStatic.Develop;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Hauptklasse für das Caching-System mit automatischer Synchronisation.
/// Einzige Factory für alle CachedFile-Instanzen.
/// </summary>
public sealed class CachedFileSystem : IDisposableExtended {

    #region Fields

    /// <summary>
    /// Debounce-Zeit in Millisekunden. Watcher-Events werden gesammelt und erst
    /// verarbeitet, wenn für diesen Zeitraum keine weiteren Events eintreffen.
    /// Verhindert Kaskaden-Invalidierung bei Speichervorgängen (RENAMED+CREATED+DELETED+CHANGED).
    /// Erhöht auf 500ms, da Netzwerk-IO oft längere Bursts erzeugt.
    /// </summary>
    private const int DebounceMs = 500;

    /// <summary>
    /// Timeout in Millisekunden, den EndIgnoreFile maximal wartet, bis der Watcher
    /// das selbst geschriebene Event bestätigt hat.
    /// </summary>
    private const int IgnoreWaitTimeoutMs = 5000;

    /// <summary>
    /// Intervall in Millisekunden für die globale Stale-Prüfung aller gecachten Dateien.
    /// </summary>
    private const int StaleCheckIntervalMs = 180000;

    /// <summary>
    /// Lock-Objekt für den statischen Timer.
    /// Direkt initialisiert, um NullReferenceExceptions beim Zugriff zu vermeiden.
    /// MUSS vor _globalInstance deklariert werden, da der Konstruktor den Timer startet.
    /// Deswegen das a, wegen der Sortierung
    /// </summary>
    private static readonly object _astaleTimerLock = new();

    /// <summary>
    /// Singleton-Instanz. Hält alle gecachten Dateien (_cachedFiles), FileSystemWatcher pro
    /// Verzeichnis (_watchers) und die Dateiname-basierte Ignore-Map für laufende Speichervorgänge
    /// (_ignoreFiles).
    /// Wird NACH _staleTimerLock initialisiert, da der Konstruktor StartStaleCheckTimer() aufruft.
    /// </summary>
    private static readonly CachedFileSystem _globalInstance = new();

    /// <summary>
    /// Mapping von Datei-Suffix auf den zugehörigen CachedFile-Ableitungstyp.
    /// Wird einmalig per Reflection befüllt. Berücksichtigt AllowMultiple auf FileSuffixAttribute.
    /// </summary>
    private static readonly Lazy<Dictionary<string, Type>> _suffixTypeMap = new(BuildSuffixTypeMap);

    private static Timer? _staleCheckTimer;
    private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new(StringComparer.OrdinalIgnoreCase);

    private readonly object _debounceLock = new();

    /// <summary>
    /// Map der aktuell ignorierten Dateien: Key = normalisierter Dateipfad, Value = ManualResetEventSlim.
    /// Der Watcher signalisiert das MRE, wenn er das selbst geschriebene Event erkennt.
    /// </summary>
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _ignoredFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Ausstehende Watcher-Events, gruppiert nach normalisiertem Dateipfad.
    /// Value = das jeweils letzte Event für diese Datei (zusammenfassen mehrerer Events).
    /// </summary>
    private readonly ConcurrentDictionary<string, WatcherChangeTypes> _pendingEvents = new(StringComparer.OrdinalIgnoreCase);

    private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Pro überwachtem Verzeichnis ein FileSystemWatcher.
    /// </summary>
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Debounce-Timer. Wird bei jedem Watcher-Event zurückgesetzt.
    /// Feuert erst, wenn für DebounceMs keine weiteren Events eintreffen.
    /// </summary>
    private Timer? _debounceTimer;

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    private CachedFileSystem() {
        _debounceTimer = new Timer(OnDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
        StartStaleCheckTimer();
    }

    #endregion

    #region Properties

    public bool IsDisposed => _isDisposedFlag == 1;

    #endregion

    #region Methods

    /// <summary>
    /// Markiert eine Datei als "in Bearbeitung durch das System", um Watcher-Events zu ignorieren.
    /// Registriert ein ManualResetEventSlim, damit EndIgnoreFile wartet, bis der Watcher das
    /// selbst geschriebene Event gesehen hat (oder der Timeout abläuft).
    /// </summary>
    public static void BeginIgnoreFile(string filename) {
        var key = filename.NormalizeFile();
        //Diagnose("CFS",$"BEGIN IGNORE: {key.FileNameWithoutSuffix()}");
        _globalInstance._ignoredFiles.TryAdd(key, new ManualResetEventSlim(false));
    }

    public static void DisposeAll() => _globalInstance.Dispose();

    /// <summary>
    /// Hebt die Ignorierung einer Datei auf, nachdem der Watcher das Event bestätigt hat
    /// oder der Sicherheits-Timeout abgelaufen ist.
    /// Blockiert den aufrufenden Thread maximal IgnoreWaitTimeoutMs.
    /// </summary>
    public static void EndIgnoreFile(string filename) {
        var key = filename.NormalizeFile();
        if (!_globalInstance._ignoredFiles.TryRemove(key, out var mre)) { return; }
        //Diagnose("CFS",$"END IGNORE: wait start {key.FileNameWithoutSuffix()}");
        try {
            mre.Wait(IgnoreWaitTimeoutMs);
            //Diagnose("CFS",$"END IGNORE: wait done {key.FileNameWithoutSuffix()}");
        } catch (ObjectDisposedException) { }
        try { mre.Dispose(); } catch { }
    }

    /// <summary>
    /// Holt oder erstellt eine gecachte Datei.
    /// Gibt null zurück, wenn die Datei nicht im Cache ist, der Typ nicht passt oder auf Festplatte nicht existiert.
    /// </summary>
    public static T? Get<T>(string filename) where T : CachedFile {
        if (_globalInstance.IsDisposed) { return null; }

        var normalizedFileName = filename.NormalizeFile();

        if (!FileExists(normalizedFileName)) { return null; }

        _globalInstance.EnsureWatcher(normalizedFileName.FilePath());

        if (_globalInstance._cachedFiles.TryGetValue(normalizedFileName, out var existing)) {
            if (existing.IsDisposed) {
                _globalInstance._cachedFiles.TryRemove(normalizedFileName, out _);
            } else {
                return existing as T;
            }
        }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile is null) { return null; }

        var added = _globalInstance._cachedFiles.GetOrAdd(normalizedFileName, newFile);

        if (!ReferenceEquals(added, newFile)) {
            newFile.Dispose();
        }

        return added as T;
    }

    /// <summary>
    /// Gibt alle gecachten Instanzen eines bestimmten Typs zurück.
    /// </summary>
    public static List<T> GetAll<T>() where T : CachedFile {
        var result = new List<T>();
        foreach (var file in _globalInstance._cachedFiles.Values) {
            if (file is T typed) { result.Add(typed); }
        }
        return result;
    }

    /// <summary>
    /// Prüft, ob ein Datei-Suffix einem bekannten Typ zugeordnet ist.
    /// </summary>
    private static bool IsSupportedSuffix(string suffix) {
        if (string.IsNullOrEmpty(suffix)) { return false; }
        return _suffixTypeMap.Value.ContainsKey(suffix);
    }

    /// <summary>
    /// Speichert alle gecachten Dateien.
    /// </summary>
    /// <param name="mustWait">
    /// Falls FALSE: Asynchrones Speichern anstoßen und NICHT warten.
    /// Falls TRUE: Asynchrones Speichern anstoßen und WARTEN, bis alle fertig sind.
    /// </param>
    public static void SaveAll(bool mustWait) {
        var tasks = new List<Task>();

        foreach (var file in _globalInstance._cachedFiles.Values) {
            if (!file.IsSaved) {
                if (file.IsSaveAbleNow() is { Length: 0 }) {
                    tasks.Add(file.Save());
                }
            }
              // 2. Wenn bereits ein Speichervorgang läuft (IsSaving == true),
              // müssen wir bei mustWait ebenfalls darauf warten.
              else if (mustWait && file.IsSaving) {
                // Wir starten einen Task, der wartet, bis die Semaphore wieder frei ist.
                tasks.Add(Task.Run(file.WaitDiskOperationFinished));
            }
        }

        if (tasks.Count > 0) {
            Message(ErrorType.Info, null, "Dateien", ImageCode.Diskette, $"Speichere {tasks.Count} Datei(en) auf die Festplatte", 3);

            if (mustWait) {
                EndLog($"CachedFileSystem.SaveAll(true): Warte auf {tasks.Count} Task(s) (max 60s)");
                try {
                    // Warten auf alle neu angestoßenen UND bereits laufenden Vorgänge.
                    Task.WaitAll(tasks.ToArray(), 60000);
                    EndLog("CachedFileSystem.SaveAll(true): Task.WaitAll zurück");
                } catch {
                    // Fehler beim Speichern einzelner Dateien werden in SaveExtended
                    // bereits abgefangen und als String zurückgegeben,
                    // Task.WaitAll würde hier nur bei harten Abbruchausnahmen werfen.
                    // Timeout nach 60 Sekunden: verbleibende Tasks laufen im Hintergrund weiter.
                    EndLog("CachedFileSystem.SaveAll(true): Task.WaitAll hat Exception geworfen");
                }
            }

            Message(ErrorType.Info, null, "Dateien", ImageCode.Häkchen, $"{tasks.Count} Datei(en) gespeichert", 3);
        } else {
            EndLog("CachedFileSystem.SaveAll: keine anstehenden Tasks");
        }
    }

    /// <summary>
    /// Startet den globalen Stale-Check-Timer.
    /// </summary>
    private static void StartStaleCheckTimer() {
        // Falls das Feld durch einen extrem frühen Zugriff null sein sollte (Initialisierungs-Race),
        // fangen wir das hier ab, obwohl readonly object eigentlich sicher sein sollte.

        lock (_astaleTimerLock) {
            if (_staleCheckTimer is null) {
                _staleCheckTimer = new Timer(
                    callback: _ => StaleCheckCallback(),
                    state: null,
                    dueTime: StaleCheckIntervalMs,
                    period: StaleCheckIntervalMs
                );
            }
        }
    }

    private static void StopStaleCheckTimer() {
        lock (_astaleTimerLock) {
            _staleCheckTimer?.Dispose();
            _staleCheckTimer = null;
        }
    }

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        EndLog("CachedFileSystem.Dispose: START");

        EndLog("CachedFileSystem.Dispose: Vor StopStaleCheckTimer");
        StopStaleCheckTimer();
        EndLog("CachedFileSystem.Dispose: Nach StopStaleCheckTimer");

        EndLog("CachedFileSystem.Dispose: Vor EnterWriteLock");
        _watcherLock.EnterWriteLock();
        EndLog("CachedFileSystem.Dispose: Nach EnterWriteLock");
        try {
            EndLog("CachedFileSystem.Dispose: Vor DisposeAllWatchers");
            DisposeAllWatchers();
            EndLog("CachedFileSystem.Dispose: Nach DisposeAllWatchers");

            EndLog($"CachedFileSystem.Dispose: Dispose {_cachedFiles.Count} CachedFile(s)");
            foreach (var file in _cachedFiles.Values) {
                file.Dispose();
            }
            _cachedFiles.Clear();
            EndLog("CachedFileSystem.Dispose: CachedFiles disposed & cleared");
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
        }

        foreach (var mre in _ignoredFiles.Values) {
            try { mre.Set(); } catch { }
            try { mre.Dispose(); } catch { }
        }
        _ignoredFiles.Clear();

        _watcherLock.Dispose();
        GC.SuppressFinalize(this);

        EndLog("CachedFileSystem.Dispose: ENDE");
    }

    internal static void AutoRegister(CachedFile file) {
        if (_globalInstance.IsDisposed) { return; }

        var normalizedFileName = file.Filename.NormalizeFile();
        _globalInstance.EnsureWatcher(normalizedFileName.FilePath());
        _globalInstance._cachedFiles.GetOrAdd(normalizedFileName, file);
    }

    /// <summary>
    /// Entfernt eine Datei aus dem Memory-Cache und disposed sie.
    /// Wird von IO.DeleteFile aufgerufen, um den Cache konsistent zu halten.
    /// </summary>
    internal static void RemoveFileFromCache(string filename, bool warnUnsavedChanges = true) {
        _globalInstance.RemoveFromCache(filename, warnUnsavedChanges);
    }

    private static Dictionary<string, Type> BuildSuffixTypeMap() {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in GetEnumerableOfType<CachedFile>()) {
            var attrs = type.GetCustomAttributes<FileSuffixAttribute>();
            foreach (var attr in attrs) {
                if (!string.IsNullOrEmpty(attr.Suffix)) {
                    map[attr.Suffix] = type;
                }
            }
        }
        return map;
    }

    private static CachedFile? CreateCachedFile(string fileName) {
        var suffix = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(suffix)) { return null; }
        if (!_suffixTypeMap.Value.TryGetValue(suffix, out var type)) { return null; }

        try {
            return Activator.CreateInstance(
                type,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [fileName],
                null) as CachedFile;
        } catch {
            return null;
        }
    }

    private static List<string> GetAllMatchingFiles(string watchedPath) {
        if (!DirectoryExists(watchedPath)) { return []; }
        var allFiles = IO.GetFiles(watchedPath, "*.*", SearchOption.TopDirectoryOnly);
        return allFiles.Where(f => IsSupportedSuffix(Path.GetExtension(f))).ToList();
    }

    private static async Task StaleCheckCallback() {
        //Diagnose("CFS",$"STALE CHECK: start");
        try {
            foreach (var file in _globalInstance._cachedFiles.Values) {
                try {
                    if (file.IsDisposed) { continue; }
                    if (file.IsStale() && !file.IsLoading && !file.IsSaving) {
                        //Diagnose("CFS",$"STALE: {file.Filename.FileNameWithoutSuffix()} IsSaved={file.IsSaved}");
                        if (file.IsSaved) {
                            file.Invalidate();
                        } else {
                            Message(ErrorType.Warning, file, "Datei-Konflikt", ImageCode.Warnung,
                                $"Externe Änderung an '{file.Filename.FileNameWithoutSuffix()}' erkannt, aber lokale ungespeicherte Änderungen existieren. Lokale Daten bleiben erhalten.", 0);
                        }
                        continue;
                    }
                    if (!file.IsSaved && string.IsNullOrEmpty(file.IsSaveAbleNow())) {
                        try {
                            await file.Save().ConfigureAwait(false);
                        } catch { }
                    }
                } catch { }
            }
        } catch { }
        //Diagnose("CFS",$"STALE CHECK: done");
    }

    /// <summary>
    /// Fügt eine Datei zum Cache hinzu — immer im richtigen Typ.
    /// Unbekannte Suffixe werden still ignoriert.
    /// </summary>
    private void AddToCache(string fileName) {
        var normalizedFileName = fileName.NormalizeFile();
        if (_cachedFiles.ContainsKey(normalizedFileName)) { return; }

        if (CreateCachedFile(normalizedFileName) is not { } newFile) { return; }

        var added = _cachedFiles.GetOrAdd(normalizedFileName, newFile);
        if (!ReferenceEquals(added, newFile)) { newFile.Dispose(); }
    }

    private FileSystemWatcher CreateWatcher(string normalizedPath) {
        var watcher = new FileSystemWatcher(normalizedPath) {
            // Wir beschränken uns auf das Wesentliche, um den Puffer zu schonen.
            // LastAccess oder Attributes interessieren uns für den Cache nicht.
            NotifyFilter = NotifyFilters.FileName
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Size,
            IncludeSubdirectories = true,
            // 64 KB ist das empfohlene Maximum. Größere Puffer (bis 128KB möglich)
            // können die Systemperformance (Non-Paged Pool) negativ beeinflussen.
            InternalBufferSize = 64 * 1024
        };

        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileChanged;
        watcher.Deleted += OnFileDeleted;
        watcher.Renamed += OnFileRenamed;
        watcher.Error += (s, e) => OnWatcherError(normalizedPath);

        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    private void DisposeAllWatchers() {
        // Debounce-Timer stoppen und ausstehende Events verwerfen
        lock (_debounceLock) {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
        _pendingEvents.Clear();

        // SCHLEIFE 1: Sofortige Entkoppelung der Events (Sicher & Schnell)
        // Das verhindert, dass während des Disposens noch Logik in deinen Cache läuft.
        foreach (var kvp in _watchers.Values) {
            try {
                kvp.EnableRaisingEvents = false;
                kvp.Created -= OnFileCreated;
                kvp.Changed -= OnFileChanged;
                kvp.Deleted -= OnFileDeleted;
                kvp.Renamed -= OnFileRenamed;
            } catch { /* Event-Abmeldung nicht kritisch */ }
        }

        // SCHLEIFE 2: Das eigentliche Dispose in den Hintergrund schieben
        // Wir kopieren die Liste, um Thread-Sicherheit beim Iterieren zu haben.
        var watchersToDispose = _watchers.Values.ToList();
        _watchers.Clear();

        // Wir versuchen das Dispose asynchron. Wenn es hängt, "stirbt" der Task
        // einfach mit dem Prozess-Ende, ohne das UI/Hauptprogramm zu blockieren.
        Task.Run(() => {
            foreach (var watcher in watchersToDispose) {
                try {
                    // Wir geben dem Ganzen eine Chance, aber wenn es blockiert,
                    // wird der Thread vom OS beim Prozessende terminiert.
                    watcher.Dispose();
                } catch {
                    // Ignorieren, wir sind im Shutdown-Modus
                }
            }
        });
    }

    private void DisposeWatcher(string normalizedPath) {
        if (_watchers.TryRemove(normalizedPath, out var watcher)) {
            try {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            } catch { /* Watcher-Dispose nicht kritisch */ }
        }
    }

    /// <summary>
    /// Reiht ein Watcher-Event in die Debounce-Queue ein und setzt den Timer zurück.
    /// Mehrere Events für dieselbe Datei werden zusammengefasst: das letzte gewinnt,
    /// außer Delete+Create wird zu Changed (Umbenennung ohne Inhaltsverlust).
    /// Created + Deleted wird als No-Op entfernt (Datei wurde kurz erstellt und wieder gelöscht).
    /// </summary>
    private void EnqueueEvent(string fullPath, WatcherChangeTypes changeType) {
        var key = fullPath.NormalizeFile();

        var removePending = false;

        _pendingEvents.AddOrUpdate(key, changeType, (_, existing) => {
            // Delete + Created = Datei wurde ersetzt (Rename/Save) → Changed
            if (existing == WatcherChangeTypes.Deleted && changeType == WatcherChangeTypes.Created) {
                return WatcherChangeTypes.Changed;
            }
            // Created + Changed = Created (noch nicht im Cache, Changed irrelevant)
            if (existing == WatcherChangeTypes.Created && changeType == WatcherChangeTypes.Changed) {
                return WatcherChangeTypes.Created;
            }
            // Created + Deleted = Datei wurde kurz erstellt und wieder gelöscht → No-Op
            if (existing == WatcherChangeTypes.Created && changeType == WatcherChangeTypes.Deleted) {
                removePending = true;
                return changeType;
            }
            // Immer das neueste Event behalten
            return changeType;
        });

        if (removePending) { _pendingEvents.TryRemove(key, out _); }

        // Timer zurücksetzen — feuert erst, wenn DebounceMs kein weiteres Event kommt
        lock (_debounceLock) {
            if (_debounceTimer is not null && !IsDisposed) {
                _debounceTimer.Change(DebounceMs, Timeout.Infinite);
            }
        }
    }

    /// <summary>
    /// Stelle sicher, dass für das angegebene Verzeichnis ein Watcher aktiv ist.
    /// </summary>
    private void EnsureWatcher(string path) {
        if (IsDisposed) { return; }
        var key = path.NormalizePath();
        if (_watchers.ContainsKey(key)) { return; }

        if (!DirectoryExists(key)) { return; }

        _watcherLock.EnterWriteLock();
        try {
            if (_watchers.ContainsKey(key)) { return; }
            var watcher = CreateWatcher(key);
            _watchers.TryAdd(key, watcher);
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
        }
    }

    private bool IsIgnored(string fullPath) {
        return _ignoredFiles.ContainsKey(fullPath.NormalizeFile());
    }

    /// <summary>
    /// Wird vom Debounce-Timer aufgerufen, wenn die Ruhephase abgelaufen ist.
    /// Verarbeitet alle gesammelten Events atomar.
    /// </summary>
    private void OnDebounceTick(object? state) {
        if (IsDisposed) { return; }

        // Alle Events atomar entnehmen
        var events = new List<KeyValuePair<string, WatcherChangeTypes>>();
        foreach (var key in _pendingEvents.Keys) {
            if (_pendingEvents.TryRemove(key, out var ct)) {
                events.Add(new KeyValuePair<string, WatcherChangeTypes>(key, ct));
            }
        }

        if (events.Count == 0) { return; }

        //Diagnose("CFS",$"DEBOUNCE: processing {events.Count} event(s)");

        foreach (var (key, changeType) in events) {
            try {
                if (IsDisposed) { break; }
                ProcessDebouncedEvent(key, changeType);
            } catch {
                //Diagnose("CFS",$"DEBOUNCE ERROR: {ex.Message} {key.FileNameWithoutSuffix()}");
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (IsIgnored(e.FullPath)) {
            //Diagnose("CFS",$"WATCHER CHANGED (ignored): {e.FullPath.FileNameWithoutSuffix()}");
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        EnqueueEvent(e.FullPath, WatcherChangeTypes.Changed);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        //Diagnose("CFS",$"WATCHER CREATED: {e.FullPath.FileNameWithoutSuffix()}");
        if (IsIgnored(e.FullPath)) {
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        EnqueueEvent(e.FullPath, WatcherChangeTypes.Created);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        //Diagnose("CFS",$"WATCHER DELETED: {e.FullPath.FileNameWithoutSuffix()}");
        if (IsIgnored(e.FullPath)) {
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        EnqueueEvent(e.FullPath, WatcherChangeTypes.Deleted);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e) {
        if (IsDisposed) { return; }
        //Diagnose("CFS",$"WATCHER RENAMED: {e.OldFullPath.FileNameWithoutSuffix()} -> {e.FullPath.FileNameWithoutSuffix()}");

        var oldIgnored = IsIgnored(e.OldFullPath);
        var newIgnored = IsIgnored(e.FullPath);
        if (oldIgnored) { SignalIgnoreSeen(e.OldFullPath); }
        if (newIgnored) { SignalIgnoreSeen(e.FullPath); }
        if (oldIgnored && newIgnored) { return; }

        if (!oldIgnored) {
            EnqueueEvent(e.OldFullPath, WatcherChangeTypes.Deleted);
        }

        if (!newIgnored && ShouldCacheFile(e.FullPath)) {
            EnqueueEvent(e.FullPath, WatcherChangeTypes.Created);
        }
    }

    private void OnWatcherError(string watchedPath) {
        if (IsDisposed) { return; }
        //Diagnose("CFS",$"WATCHER ERROR: {watchedPath}");
        Task.Run(async () => {
            var attempts = 0;
            var normalizedPath = watchedPath.NormalizePath();
            var watchedKey = normalizedPath;

            do {
                // Exponentielles Back-off: Wir warten bei jedem Versuch etwas länger
                await Task.Delay(1000 * (attempts + 1)).ConfigureAwait(false);
                attempts++;

                _watcherLock.EnterWriteLock();
                try {
                    if (IsDisposed) { return; }

                    // 1. Alten Watcher entsorgen
                    DisposeWatcher(normalizedPath);

                    if (!DirectoryExists(normalizedPath)) {
                        // Wenn das Verzeichnis weg ist, müssen wir alle Dateien daraus aus dem Cache werfen
                        var filesToRemove = _cachedFiles.Keys.Where(k => k.StartsWith(watchedKey, StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (var key in filesToRemove) {
                            RemoveFromCache(key);
                        }
                        return; // Recovery für diesen Pfad beendet
                    }

                    var currentFilesOnDisk = GetAllMatchingFiles(normalizedPath);
                    var diskHashSet = new HashSet<string>(currentFilesOnDisk, StringComparer.OrdinalIgnoreCase);

                    var cachedKeysInPath = _cachedFiles.Keys.Where(k => k.StartsWith(watchedKey, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var key in cachedKeysInPath) {
                        if (IsIgnored(key)) { continue; }
                        if (!diskHashSet.Contains(key)) {
                            RemoveFromCache(key);
                        }
                    }

                    // 5. Cache-Aktualisierung: Alles was auf Disk ist -> Prüfen
                    foreach (var filePath in currentFilesOnDisk) {
                        var key = filePath.NormalizeFile();
                        if (_cachedFiles.TryGetValue(key, out var cachedFile)) {
                            if (cachedFile.IsStale()) {
                                cachedFile.Invalidate();
                            }
                        } else {
                            // Neue Datei gefunden, die wir noch nicht kennen
                            AddToCache(filePath);
                        }
                    }

                    // 6. Neuen Watcher initialisieren
                    var newWatcher = CreateWatcher(normalizedPath);
                    if (_watchers.TryAdd(watchedKey, newWatcher)) {
                        return; // ERFOLG!
                    }
                } catch {
                    throw DebugError($"Recovery Versuch {attempts} fehlgeschlagen.");
                } finally {
                    try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
                }
            } while (attempts < 10);
        });
    }

    /// <summary>
    /// Verarbeitet ein einzelnes debouncetes Event.
    /// Wird aufgerufen, nachdem die Ruhephase abgelaufen ist.
    /// </summary>
    private void ProcessDebouncedEvent(string key, WatcherChangeTypes changeType) {
        switch (changeType) {
            case WatcherChangeTypes.Changed:
                if (_cachedFiles.TryGetValue(key, out var file)) {
                    if (file.IsDisposed) { return; }
                    // Wenn die Datei gerade gespeichert ODER geladen wird, ignorieren wir das Event.
                    // Das verhindert Zyklen und ObjectDisposedExceptions während des Ladevorgangs.
                    if (file.IsSaving || file.IsLoading) {
                        //Diagnose("CFS",$"DEBOUNCE CHANGED (busy): {key.FileNameWithoutSuffix()} Saving={file.IsSaving} Loading={file.IsLoading}");
                        return;
                    }
                    if (!file.IsStale()) { return; }
                    //Diagnose("CFS",$"DEBOUNCE CHANGED -> INVALIDATE: {key.FileNameWithoutSuffix()} IsSaved={file.IsSaved}");
                    if (!file.IsSaved) {
                        Message(ErrorType.Warning, file, "Datei-Konflikt", ImageCode.Warnung,
                            $"Externe Änderung an '{file.Filename.FileNameWithoutSuffix()}' erkannt, lokale ungespeicherte Änderungen werden verworfen.", 0);
                    }
                    file.Invalidate();
                }
                break;

            case WatcherChangeTypes.Created:
                if (!_cachedFiles.ContainsKey(key)) {
                    AddToCache(key);
                } else if (_cachedFiles.TryGetValue(key, out var existingFile)) {
                    if (existingFile.IsDisposed) { return; }
                    // Datei existiert bereits im Cache → prüfen ob Stale
                    if (existingFile.IsSaving || existingFile.IsLoading) {
                        //Diagnose("CFS",$"DEBOUNCE CREATED (busy): {key.FileNameWithoutSuffix()}");
                        return;
                    }
                    if (existingFile.IsStale()) {
                        //Diagnose("CFS",$"DEBOUNCE CREATED (existing, stale) -> INVALIDATE: {key.FileNameWithoutSuffix()}");
                        existingFile.Invalidate();
                    }
                }
                break;

            case WatcherChangeTypes.Deleted:
                //Diagnose("CFS",$"DEBOUNCE DELETED: {key.FileNameWithoutSuffix()}");
                RemoveFromCache(key);
                break;
        }
    }

    private void RemoveFromCache(string filename, bool warnUnsavedChanges = true) {
        if (string.IsNullOrEmpty(filename)) { return; }
        var key = filename.NormalizeFile();
        if (_cachedFiles.TryRemove(key, out var file)) {
            if (warnUnsavedChanges && !file.IsDisposed && !file.IsSaved) {
                Message(ErrorType.Warning, file, "Datei-Verlust", ImageCode.Warnung,
                    $"Datei '{file.Filename.FileNameWithoutSuffix()}' wurde extern gelöscht/umbenannt, aber es gab ungespeicherte lokale Änderungen. Diese gehen verloren.", 0);
            }
            file.Dispose();
        }
    }

    private bool ShouldCacheFile(string filename) {
        if (IsDisposed) { return false; }
        if (!IsSupportedSuffix(Path.GetExtension(filename))) { return false; }
        var normFile = filename.NormalizeFile();
        return _watchers.Keys.Any(w => normFile.StartsWith(w, StringComparison.OrdinalIgnoreCase));
    }

    private void SignalIgnoreSeen(string fullPath) {
        var key = fullPath.NormalizeFile();
        if (_ignoredFiles.TryGetValue(key, out var mre)) {
            //Diagnose("CFS",$"SIGNAL IGNORE SEEN: {key.FileNameWithoutSuffix()}");
            try { mre.Set(); } catch (ObjectDisposedException) { }
        }
    }

    #endregion
}