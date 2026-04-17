// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics.Attributes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Intervall in Millisekunden für die globale Stale-Prüfung aller gecachten Dateien.
    /// </summary>
    private const int StaleCheckIntervalMs = 180000;

    /// <summary>
    /// Die einzige globale Instanz.
    /// WICHTIG: Muss NACH _staleTimerLock deklariert werden, da der Konstruktor den Timer startet.
    /// </summary>
    private static readonly CachedFileSystem _globalInstance = new();

    /// <summary>
    /// Lock-Objekt für den statischen Timer.
    /// Direkt initialisiert, um NullReferenceExceptions beim Zugriff zu vermeiden.
    /// </summary>
    private static readonly object _staleTimerLock = new();

    /// <summary>
    /// Mapping von Datei-Suffix auf den zugehörigen CachedFile-Ableitungstyp.
    /// Wird einmalig per Reflection befüllt. Berücksichtigt AllowMultiple auf FileSuffixAttribute.
    /// </summary>
    private static readonly Lazy<Dictionary<string, Type>> _suffixTypeMap = new(BuildSuffixTypeMap);

    private static Timer? _staleCheckTimer;
    private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _ignoredFiles = new(StringComparer.OrdinalIgnoreCase);

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
    /// </summary>
    public static void BeginIgnoreFile(string filename) => _globalInstance._ignoredFiles.TryAdd(filename.NormalizeFile(), 0);

    public static void DisposeAll() => _globalInstance.Dispose();

    /// <summary>
    /// Hebt die Ignorierung einer Datei wieder auf.
    /// </summary>
    public static void EndIgnoreFile(string filename) => _globalInstance._ignoredFiles.TryRemove(filename.NormalizeFile(), out _);

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
            if (existing.IsDisposed) { Develop.DebugPrint_NichtImplementiert(true); }
            return existing as T;
        }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; }

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

        if (includePatterns == null || includePatterns.Count == 0) {
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

        if (includePatterns == null || includePatterns.Count == 0) {
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

    /// <summary>
    /// Lädt alle angegebenen Dateien parallel in den Cache vor.
    /// Kehrt erst zurück, wenn alle Dateien geladen sind.
    /// Bei Fehlern werden die betroffenen Dateien bis zu 3-mal erneut versucht.
    /// </summary>
    public static void Preload(IEnumerable<string> filenames) {
        if (_globalInstance.IsDisposed) { return; }

        var files = filenames
            .Select(f => f.NormalizeFile())
            .Where(f => FileExists(f))
            .Select(Get<CachedFile>)
            .OfType<CachedFile>()
            .ToList();

        if (files.Count == 0) { return; }

        const int maxRetries = 5;

        for (var attempt = 0; attempt < maxRetries; attempt++) {
            var toLoad = attempt == 0
                ? files
                : files.Where(f => f.LoadFailed).ToList();

            if (toLoad.Count == 0) { break; }

            if (attempt > 0) {
                foreach (var f in toLoad) { f.Invalidate(); }
            }

            Task.WhenAll(toLoad.Select(f => Task.Run(() => f.EnsureContentLoaded()))).GetAwaiter().GetResult();

            if (!files.Any(f => f.LoadFailed)) { break; }
        }
    }

    /// <summary>
    /// Registriert eine existierende CachedFile-Instanz im Cache.
    /// Stellt sicher, dass für das Verzeichnis ein Watcher aktiv ist.
    /// Falls bereits eine Instanz für denselben Dateipfad im Cache existiert,
    /// wird die übergebene Instanz verworfen und die gecachte Instanz zurückgegeben.
    /// </summary>
    public static T Register<T>(T file) where T : CachedFile {
        if (file == null) { throw Develop.DebugError(nameof(file)); }
        if (_globalInstance.IsDisposed) { throw Develop.DebugError(nameof(CachedFileSystem)); }

        var normalizedFileName = file.Filename.NormalizeFile();
        _globalInstance.EnsureWatcher(normalizedFileName.FilePath());

        var added = _globalInstance._cachedFiles.GetOrAdd(normalizedFileName, file);

        if (!ReferenceEquals(added, file)) {
            file.Dispose();
        }

        return (T)added;
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
            if (!file.IsSaved && file.IsSaveAbleNow()) {
                tasks.Add(file.Save());
            }
            // 2. Wenn bereits ein Speichervorgang läuft (IsSaving == true),
            // müssen wir bei mustWait ebenfalls darauf warten.
            else if (mustWait && file.IsSaving) {
                // Wir starten einen Task, der wartet, bis die Semaphore wieder frei ist.
                tasks.Add(Task.Run(file.WaitDiskOperationFinished));
            }
        }

        if (mustWait && tasks.Count > 0) {
            try {
                // Warten auf alle neu angestoßenen UND bereits laufenden Vorgänge.
                Task.WaitAll(tasks.ToArray());
            } catch {
                // Fehler beim Speichern einzelner Dateien werden in SaveExtended
                // bereits abgefangen und als String zurückgegeben,
                // Task.WaitAll würde hier nur bei harten Abbruchausnahmen werfen.
            }
        }
    }

    /// <summary>
    /// Startet den globalen Stale-Check-Timer.
    /// </summary>
    public static void StartStaleCheckTimer() {
        // Falls das Feld durch einen extrem frühen Zugriff null sein sollte (Initialisierungs-Race),
        // fangen wir das hier ab, obwohl readonly object eigentlich sicher sein sollte.
        var lockObj = _staleTimerLock ?? new object();

        lock (lockObj) {
            if (_staleCheckTimer == null) {
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
        lock (_staleTimerLock) {
            _staleCheckTimer?.Dispose();
            _staleCheckTimer = null;
        }
    }

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }

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

        _watcherLock.Dispose();
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

    private static bool MatchesPattern(string fileName, string pattern) {
        var regexPattern = $"^{Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$";
        return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
    }

    private static async void StaleCheckCallback() {
        foreach (var file in _globalInstance._cachedFiles.Values) {
            if (file.IsDisposed) { continue; }
            if (file.IsStale() && !file.IsLoading) {
                // Nur invalidieren, wenn keine ungespeicherten lokalen Änderungen existieren.
                // Andernfalls würden lokale Datenänderungen unwiederbringlich verworfen.
                if (file.IsSaved) {
                    file.Invalidate();
                }
                continue;
            }
            if (!file.IsSaved && file.IsSaveAbleNow()) {
                try {
                    await file.Save().ConfigureAwait(false);
                } catch { /* Speichern fehlgeschlagen, wird naechsten Versuch erneut versucht */ }
            }
        }
    }

    /// <summary>
    /// Fügt eine Datei zum Cache hinzu — immer im richtigen Typ.
    /// Unbekannte Suffixe werden still ignoriert.
    /// </summary>
    private CachedFile? AddToCache(string fileName) {
        var normalizedFileName = fileName.NormalizeFile();
        if (_cachedFiles.TryGetValue(normalizedFileName, out var existing)) { return existing; }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; }

        var added = _cachedFiles.GetOrAdd(normalizedFileName, newFile);
        if (!ReferenceEquals(added, newFile)) { newFile.Dispose(); }
        return added;
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
                kvp.Created -= OnFileCreated;
                kvp.Changed -= OnFileChanged;
                kvp.Deleted -= OnFileDeleted;
                kvp.Renamed -= OnFileRenamed;
                kvp.Error -= (s, e) => { }; // Error-Handler neutralisieren
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
                    watcher.EnableRaisingEvents = false;
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
        var normalizedPath = path.NormalizePath();
        var key = normalizedPath;

        if (!DirectoryExists(normalizedPath)) { return; }
        if (_watchers.ContainsKey(key)) { return; }

        _watcherLock.EnterWriteLock();
        try {
            if (_watchers.ContainsKey(key)) { return; }
            var watcher = CreateWatcher(normalizedPath);
            _watchers.TryAdd(key, watcher);
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { /* Lock-Freigabe nicht kritisch */ }
        }
    }

    private bool IsIgnored(string fullPath) {
        return _ignoredFiles.ContainsKey(fullPath.NormalizeFile());
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed || IsIgnored(e.FullPath)) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        var key = e.FullPath.NormalizeFile();
        if (_cachedFiles.TryGetValue(key, out var file)) { file.Invalidate(); } else { AddToCache(e.FullPath); }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) {
        if (IsDisposed || IsIgnored(e.FullPath)) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        AddToCache(e.FullPath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e) {
        if (IsDisposed || IsIgnored(e.FullPath)) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        RemoveFromCache(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e) {
        if (IsDisposed) { return; }

        // 1. Prüfen, ob einer der Pfade (alt oder neu) ignoriert werden soll
        var oldIgnored = IsIgnored(e.OldFullPath);
        var newIgnored = IsIgnored(e.FullPath);
        // Wenn beide ignoriert werden (typisch für den finalen Move von .tmp zu Hauptdatei), komplett raus.
        if (oldIgnored && newIgnored) { return; }

        // 2. Den alten Pfad aus dem Cache entfernen (falls vorhanden)
        // Das machen wir nur, wenn der alte Pfad NICHT ignoriert wurde.
        if (!oldIgnored) {
            RemoveFromCache(e.OldFullPath);
        }

        // 3. Den neuen Pfad zum Cache hinzufügen (falls unterstützt)
        // Das machen wir nur, wenn der neue Pfad NICHT ignoriert wurde.
        if (!newIgnored && ShouldCacheFile(e.FullPath)) {
            AddToCache(e.FullPath);
        }
    }

    private void OnWatcherError(string watchedPath) {
        if (IsDisposed) { return; }
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

    private void RemoveFromCache(string filename) {
        if (string.IsNullOrEmpty(filename)) { return; }
        var key = filename.NormalizeFile();
        if (_cachedFiles.TryRemove(key, out var file)) { file.Dispose(); }
    }

    private bool ShouldCacheFile(string filename) {
        if (IsDisposed) { return false; }
        if (!IsSupportedSuffix(Path.GetExtension(filename))) { return false; }
        var normFile = filename.NormalizeFile();
        return _watchers.Keys.Any(w => normFile.StartsWith(w, StringComparison.OrdinalIgnoreCase));
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