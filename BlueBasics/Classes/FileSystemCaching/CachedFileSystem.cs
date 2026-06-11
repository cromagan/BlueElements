// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
    /// Verzeichnis (_watchers), Dateiname-basierte Ignore-Map für laufende Speichervorgänge
    /// (_ignoreFiles) und die Liste der bereits vollständig eingelesenen Verzeichnisse (_warmedDirectories).
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

    /// <summary>
    /// Map der aktuell ignorierten Dateien: Key = normalisierter Dateipfad, Value = ManualResetEventSlim.
    /// Der Watcher signalisiert das MRE, wenn er das selbst geschriebene Event erkennt.
    /// </summary>
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _ignoredFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Verzeichnisse, für die WarmCache bereits vollständig durchgelaufen ist.
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _warmedDirectories = new(StringComparer.OrdinalIgnoreCase);

    private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Pro überwachtem Verzeichnis ein FileSystemWatcher.
    /// </summary>
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    private CachedFileSystem() {
        // Startet den Timer bei Instanziierung der Singleton-Instanz
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
        Diag($"BEGIN IGNORE: {key.FileNameWithoutSuffix()}");
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
        Diag($"END IGNORE: wait start {key.FileNameWithoutSuffix()}");
        try {
            mre.Wait(IgnoreWaitTimeoutMs);
            Diag($"END IGNORE: wait done {key.FileNameWithoutSuffix()}");
        } catch (ObjectDisposedException) {
        }
        try { mre.Dispose(); } catch { }
    }

    /// <summary>
    /// Prüft, ob eine Datei existiert.
    /// Mehrstufige Prüfung: 1. Im Cache → true. 2. Verzeichnis gewarmt → false. 3. IO.FileExists.
    /// WarmCache wird NICHT aufgerufen.
    /// </summary>
    /// <param name="filename">Dateipfad.</param>
    /// <param name="forceDiskCheck">Wenn true, wird die Existenz immer auf der Festplatte geprüft,
    /// auch wenn die Datei bereits im Cache ist.</param>
    public static bool FileExists(string filename, bool forceDiskCheck = false) {
        if (_globalInstance.IsDisposed) { return false; }

        var key = filename.NormalizeFile();

        if (!forceDiskCheck) {
            // Stufe 1: Im Cache → sofort true (auch wenn noch nicht auf Festplatte gespeichert)
            if (_globalInstance._cachedFiles.ContainsKey(key)) { return true; }
        }

        var dirPath = filename.FilePath().NormalizePath();
        _globalInstance.EnsureWatcher(dirPath);

        // Festplatten-Prüfung
        if (forceDiskCheck) {
            return IO.FileExists(key);
        }

        // Stufe 2: Verzeichnis vollständig gecacht → Datei existiert nicht
        if (_globalInstance._warmedDirectories.ContainsKey(dirPath)) { return false; }

        // Stufe 3: Cache unvollständig → Disk-Prüfung
        return IO.FileExists(key);
    }

    /// <summary>
    /// Friert alle gecachten Dateien mit dem angegebenen Grund ein.
    /// </summary>
    public static void FreezeAll(string reason) {
        foreach (var file in _globalInstance._cachedFiles.Values) {
            file.Freeze(reason);
        }
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
    /// Gibt alle gecachten kompletten Dateipfade zurück, optional gefiltert nach Pattern.
    /// Nur TopDirectoryOnly!
    /// </summary>
    public static string[] GetFileNames(string path, List<string>? includePatterns = null) {
        if (_globalInstance.IsDisposed) { return []; }

        var normalizedPath = path.NormalizePath();
        _globalInstance.EnsureWarmCache(normalizedPath);

        var filesInPath = _globalInstance._cachedFiles.Keys
            .Where(filename => filename.FilePath().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (includePatterns is null || includePatterns.Count == 0) {
            return filesInPath;
        }

        return filesInPath.Where(filename => includePatterns.Exists(pattern => MatchesPattern(filename, pattern))).ToArray();
    }

    public static CachedFile[] GetFiles(string path, List<string>? includePatterns = null) {
        if (_globalInstance.IsDisposed) { return []; }

        var normalizedPath = path.NormalizePath();
        _globalInstance.EnsureWarmCache(normalizedPath);

        var filesInPath = _globalInstance._cachedFiles.Values
            .Where(f => f.Filename.FilePath().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (includePatterns is null || includePatterns.Count == 0) {
            return filesInPath;
        }

        return filesInPath.Where(f => includePatterns.Exists(pattern => MatchesPattern(f.Filename, pattern))).ToArray();
    }

    /// <summary>
    /// Prüft, ob ein Datei-Suffix einem bekannten Typ zugeordnet ist.
    /// </summary>
    public static bool IsSupportedSuffix(string suffix) {
        if (string.IsNullOrEmpty(suffix)) { return false; }
        return _suffixTypeMap.Value.ContainsKey(suffix);
    }

    public static void Preload(IEnumerable<string> filenames) {
        if (_globalInstance.IsDisposed) { return; }

        // Wir kopieren die Liste, um Thread-Probleme bei der Enumeration zu vermeiden
        var filesToProcess = filenames.ToList();

        // Wir lagern den gesamten Preload in den Hintergrund aus,
        // damit der aufrufende Thread (UI) SOFORT weiterarbeiten kann.
        Task.Run(() => {
            try {
                foreach (var filename in filesToProcess) {
                    if (_globalInstance.IsDisposed)
                        break;

                    var norm = filename.NormalizeFile();
                    if (!FileExists(norm))
                        continue;

                    // Get ist synchron und sollte kurz das Dictionary locken
                    var file = Get<CachedFile>(norm);
                    if (file is null || file.IsDisposed)
                        continue;

                    // Wir laden SEQUENTIELL. Das verhindert Thread-Starvation komplett.
                    if (file.NeedsLoading() || file.LoadFailed) {
                        file.EnsureContentLoaded();
                    }
                }
            } catch (Exception ex) {
                // Hier einen Breakpoint setzen, falls es doch knallt
                System.Diagnostics.Debug.WriteLine($"Preload Error: {ex.Message}");
            }
        });
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
            Develop.Message(ErrorType.Info, null, "Dateien", ImageCode.Diskette, $"Speichere {tasks.Count} Datei(en) auf die Festplatte", 3);

            if (mustWait) {
                try {
                    // Warten auf alle neu angestoßenen UND bereits laufenden Vorgänge.
                    Task.WaitAll(tasks.ToArray(), 60000);
                } catch {
                    // Fehler beim Speichern einzelner Dateien werden in SaveExtended
                    // bereits abgefangen und als String zurückgegeben,
                    // Task.WaitAll würde hier nur bei harten Abbruchausnahmen werfen.
                    // Timeout nach 60 Sekunden: verbleibende Tasks laufen im Hintergrund weiter.
                }
            }

            Develop.Message(ErrorType.Info, null, "Dateien", ImageCode.Häkchen, $"{tasks.Count} Datei(en) gespeichert", 3);
        }
    }

    /// <summary>
    /// Startet den globalen Stale-Check-Timer.
    /// </summary>
    public static void StartStaleCheckTimer() {
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

    public static void StopStaleCheckTimer() {
        lock (_astaleTimerLock) {
            _staleCheckTimer?.Dispose();
            _staleCheckTimer = null;
        }
    }

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        StopStaleCheckTimer();

        _watcherLock.EnterWriteLock();
        try {
            DisposeAllWatchers();

            foreach (var file in _cachedFiles.Values) {
                file.Dispose();
            }
            _cachedFiles.Clear();
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

    private static void Diag(string msg) {
        Debug.WriteLine($"[CFS {CachedFile._diagSw.ElapsedMilliseconds}ms T{Environment.CurrentManagedThreadId}] {msg}");
    }

    private static List<string> GetAllMatchingFiles(string watchedPath) {
        if (!DirectoryExists(watchedPath)) { return []; }
        var allFiles = IO.GetFiles(watchedPath, "*.*", SearchOption.TopDirectoryOnly);
        return allFiles.Where(f => IsSupportedSuffix(Path.GetExtension(f))).ToList();
    }

    private static bool MatchesPattern(string fileName, string pattern) {
        var regexPattern = $"^{Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$";
        return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
    }

    private static async Task StaleCheckCallback() {
        Diag($"STALE CHECK: start");
        try {
            foreach (var file in _globalInstance._cachedFiles.Values) {
                try {
                    if (file.IsDisposed) { continue; }
                    if (file.IsStale() && !file.IsLoading && !file.IsSaving) {
                        Diag($"STALE: {file.Filename.FileNameWithoutSuffix()} IsSaved={file.IsSaved}");
                        if (file.IsSaved) {
                            file.Invalidate();
                        } else {
                            Develop.Message(ErrorType.Warning, file, "Datei-Konflikt", ImageCode.Warnung,
                                $"Externe Änderung an '{file.Filename.FileNameWithoutSuffix()}' erkannt, aber lokale ungespeicherte Änderungen existieren. Lokale Daten bleiben erhalten.", 0);
                        }
                        continue;
                    }
                    if (!file.IsSaved && string.IsNullOrEmpty(file.IsSaveAbleNow())) {
                        try {
                            await file.Save().ConfigureAwait(false);
                        } catch { }
                    }
                } catch {
                }
            }
        } catch {
        }
        Diag($"STALE CHECK: done");
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
    /// Stellt sicher, dass der Watcher aktiv ist UND das Verzeichnis vollständig gecacht wurde.
    /// Nur aufrufen, wenn vollständige Verzeichnisliste benötigt wird (GetFiles, GetFileNames).
    /// </summary>
    private void EnsureWarmCache(string normalizedPath) {
        EnsureWatcher(normalizedPath);
        if (_warmedDirectories.ContainsKey(normalizedPath)) { return; }

        _watcherLock.EnterWriteLock();
        try {
            if (_warmedDirectories.ContainsKey(normalizedPath)) { return; }
            WarmCache(normalizedPath);
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
        }
    }

    /// <summary>
    /// Stelle sicher, dass für das angegebene Verzeichnis ein Watcher aktiv ist.
    /// WarmCache wird hier NICHT aufgerufen — nur bei explizitem Bedarf über EnsureWarmCache.
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

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (IsIgnored(e.FullPath)) {
            Diag($"WATCHER CHANGED (ignored): {e.FullPath.FileNameWithoutSuffix()}");
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        var key = e.FullPath.NormalizeFile();
        if (_cachedFiles.TryGetValue(key, out var file)) {
            if (file.IsSaving) {
                Diag($"WATCHER CHANGED (saving): {e.FullPath.FileNameWithoutSuffix()}");
                return;
            }
            if (!file.IsStale()) { return; }
            Diag($"WATCHER CHANGED -> INVALIDATE: {e.FullPath.FileNameWithoutSuffix()} IsSaved={file.IsSaved}");
            if (!file.IsSaved) {
                Develop.Message(ErrorType.Warning, file, "Datei-Konflikt", ImageCode.Warnung,
                    $"Externe Änderung an '{file.Filename.FileNameWithoutSuffix()}' erkannt, lokale ungespeicherte Änderungen werden verworfen.", 0);
            }
            file.Invalidate();
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        Diag($"WATCHER CREATED: {e.FullPath.FileNameWithoutSuffix()}");
        if (IsIgnored(e.FullPath)) {
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        AddToCache(e.FullPath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        Diag($"WATCHER DELETED: {e.FullPath.FileNameWithoutSuffix()}");
        if (IsIgnored(e.FullPath)) {
            SignalIgnoreSeen(e.FullPath);
            return;
        }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        RemoveFromCache(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e) {
        if (IsDisposed) { return; }
        Diag($"WATCHER RENAMED: {e.OldFullPath.FileNameWithoutSuffix()} -> {e.FullPath.FileNameWithoutSuffix()}");

        var oldIgnored = IsIgnored(e.OldFullPath);
        var newIgnored = IsIgnored(e.FullPath);
        if (oldIgnored) { SignalIgnoreSeen(e.OldFullPath); }
        if (newIgnored) { SignalIgnoreSeen(e.FullPath); }
        if (oldIgnored && newIgnored) { return; }

        if (!oldIgnored) {
            RemoveFromCache(e.OldFullPath);
        }

        if (!newIgnored && ShouldCacheFile(e.FullPath)) {
            AddToCache(e.FullPath);
        }
    }

    private void OnWatcherError(string watchedPath) {
        if (IsDisposed) { return; }
        Diag($"WATCHER ERROR: {watchedPath}");
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

                    // 1. Alten Watcher entsorgen, Warm-Status zurücksetzen
                    DisposeWatcher(normalizedPath);
                    _warmedDirectories.TryRemove(normalizedPath, out _);

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

                    // 6. Neuen Watcher initialisieren und Verzeichnis als gewarmt markieren
                    var newWatcher = CreateWatcher(normalizedPath);
                    if (_watchers.TryAdd(watchedKey, newWatcher)) {
                        _warmedDirectories.TryAdd(normalizedPath, 0);
                        return; // ERFOLG!
                    }
                } catch {
                    throw Develop.DebugError($"Recovery Versuch {attempts} fehlgeschlagen.");
                } finally {
                    try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
                }
            } while (attempts < 10);
        });
    }

    private void RemoveFromCache(string filename, bool warnUnsavedChanges = true) {
        if (string.IsNullOrEmpty(filename)) { return; }
        var key = filename.NormalizeFile();
        if (_cachedFiles.TryRemove(key, out var file)) {
            if (warnUnsavedChanges && !file.IsDisposed && !file.IsSaved) {
                Develop.Message(ErrorType.Warning, file, "Datei-Verlust", ImageCode.Warnung,
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
            Diag($"SIGNAL IGNORE SEEN: {key.FileNameWithoutSuffix()}");
            try { mre.Set(); } catch (ObjectDisposedException) { }
        }
    }

    private void WarmCache(string normalizedPath) {
        var files = GetAllMatchingFiles(normalizedPath);
        foreach (var filePath in files) {
            try { AddToCache(filePath); } catch { /* Fehler beim Cachen einer Datei */ }
        }
        _warmedDirectories.TryAdd(normalizedPath, 0);
    }

    #endregion
}