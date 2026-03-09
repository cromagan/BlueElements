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
    /// <summary>
    /// Intervall in Millisekunden für die globale Stale-Prüfung aller gecachten Dateien.
    /// </summary>

    #region Fields

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
    private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new();

    private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Pro überwachtem Verzeichnis ein FileSystemWatcher.
    /// Key = normalisierter Pfad (Uppercase).
    /// </summary>
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();

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

    /// <summary>
    /// Löscht eine Datei mit bekanntem Suffix.
    /// Die Instanz wird disposed und aus dem Cache entfernt. Die Datei wird vom Dateisystem gelöscht.
    /// </summary>
    /// <returns>True wenn erfolgreich, false wenn das Suffix unbekannt ist oder die Löschung fehlschlägt.</returns>

    /// <summary>
    /// Disposed die globale Instanz und alle Ressourcen.
    /// </summary>

    #region Methods

    public static void DisposeAll() => _globalInstance.Dispose();

    /// <summary>
    /// Friert alle gecachten Dateien mit dem angegebenen Grund ein.
    /// </summary>
    public static void FreezeAll(string reason) {
        foreach (var file in _globalInstance._cachedFiles.Values) {
            file.Freeze(reason);
        }
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
    /// Gibt alle gecachten Dateipfade zurück, optional gefiltert nach Pattern.
    /// </summary>
    public static List<string> GetFiles(string path, List<string>? includePatterns = null) {
        if (_globalInstance.IsDisposed) { return []; }

        var normalizedPath = path.NormalizePath();
        _globalInstance.EnsureWatcher(normalizedPath);

        var filesInPath = _globalInstance._cachedFiles.Keys
            .Where(filename => filename.FilePath().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (includePatterns == null || includePatterns.Count == 0) {
            return [.. filesInPath];
        }

        return [.. filesInPath.Where(filename => includePatterns.Exists(pattern => MatchesPattern(filename, pattern)))];
    }

    /// <summary>
    /// Holt oder erstellt eine gecachte Datei.
    /// Gibt null zurück, wenn die Datei nicht im Cache ist oder der Typ nicht passt.
    /// </summary>
    public static T? GetOrCreate<T>(string filename) where T : CachedFile {
        if (_globalInstance.IsDisposed) { return null; }

        var normalizedFileName = filename.NormalizeFile();
        _globalInstance.EnsureWatcher(normalizedFileName.FilePath());

        var key = normalizedFileName.ToUpperInvariant();

        if (_globalInstance._cachedFiles.TryGetValue(key, out var existing)) {
            return existing as T;
        }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; }

        var added = _globalInstance._cachedFiles.GetOrAdd(key, newFile);

        if (!ReferenceEquals(added, newFile)) {
            newFile.Dispose();
        }

        return added as T;
    }

    /// <summary>
    /// Prüft, ob ein Datei-Suffix einem bekannten Typ zugeordnet ist.
    /// </summary>
    public static bool IsSupportedSuffix(string suffix) {
        if (string.IsNullOrEmpty(suffix)) { return false; }
        return _suffixTypeMap.Value.ContainsKey(suffix.ToUpperInvariant());
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
            // 1. Wenn die Datei gespeichert werden muss und kann, stoßen wir es an.
            if (!file.IsSaved && file.IsSaveAbleNow()) {
                tasks.Add(file.Save());
            }
            // 2. Wenn bereits ein Speichervorgang läuft (IsSaving == true),
            // müssen wir bei mustWait ebenfalls darauf warten.
            else if (mustWait && file.IsSaving) {
                // Wir starten einen Task, der wartet, bis die Semaphore wieder frei ist.
                tasks.Add(Task.Run(() => file.WaitDiskOperationFinished()));
            }
        }

        if (mustWait && tasks.Count > 0) {
            try {
                // Warten auf alle neu angestoßenen UND bereits laufenden Vorgänge.
                Task.WaitAll([.. tasks]);
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
            try { _watcherLock.ExitWriteLock(); } catch { }
        }

        _watcherLock.Dispose();
    }

    /// <summary>
    /// Prüft, ob eine Datei im Cache existiert.
    /// </summary>
    public bool FileExists(string filename) {
        if (IsDisposed) { return false; }
        var key = filename.NormalizeFile().ToUpperInvariant();
        return _cachedFiles.ContainsKey(key);
    }

    private static Dictionary<string, Type> BuildSuffixTypeMap() {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in GetEnumerableOfType<CachedFile>()) {
            var attrs = type.GetCustomAttributes<FileSuffixAttribute>();
            foreach (var attr in attrs) {
                if (!string.IsNullOrEmpty(attr.Suffix)) {
                    map[attr.Suffix.ToUpperInvariant()] = type;
                }
            }
        }
        return map;
    }

    private static CachedFile? CreateCachedFile(string fileName) {
        var suffix = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(suffix)) { return null; }
        if (!_suffixTypeMap.Value.TryGetValue(suffix.ToUpperInvariant(), out var type)) { return null; }

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
        if (!Directory.Exists(watchedPath)) { return []; }
        var allFiles = Directory.GetFiles(watchedPath, "*.*", SearchOption.AllDirectories);
        return [.. allFiles.Where(f => IsSupportedSuffix(Path.GetExtension(f)))];
    }

    private static bool MatchesPattern(string fileName, string pattern) {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
    }

    private static async void StaleCheckCallback() {
        foreach (var file in _globalInstance._cachedFiles.Values) {
            if (file.IsDisposed) { continue; }
            if (file.IsStale()) {
                file.Invalidate();
                continue;
            }
            if (!file.IsSaved && file.IsSaveAbleNow()) {
                try {
                    await file.Save().ConfigureAwait(false);
                } catch { }
            }
        }
    }

    /// <summary>
    /// Fügt eine Datei zum Cache hinzu — immer im richtigen Typ.
    /// Unbekannte Suffixe werden still ignoriert.
    /// </summary>
    private CachedFile? AddToCache(string fileName) {
        var normalizedFileName = fileName.NormalizeFile();
        var key = normalizedFileName.ToUpperInvariant();
        if (_cachedFiles.TryGetValue(key, out var existing)) { return existing; }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; }

        var added = _cachedFiles.GetOrAdd(key, newFile);
        if (!ReferenceEquals(added, newFile)) { newFile.Dispose(); }
        return added;
    }

    private FileSystemWatcher CreateWatcher(string normalizedPath) {
        var watcher = new FileSystemWatcher(normalizedPath) {
            NotifyFilter = NotifyFilters.FileName
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Size
                         | NotifyFilters.CreationTime,
            IncludeSubdirectories = true,
            InternalBufferSize = 64 * 1024
        };
        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileChanged;
        watcher.Deleted += OnFileDeleted;
        watcher.Renamed += OnFileRenamed;
        watcher.Error += (s, e) => OnWatcherError(normalizedPath, e);
        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    private void DisposeAllWatchers() {
        foreach (var kvp in _watchers) {
            try {
                kvp.Value.EnableRaisingEvents = false;
                kvp.Value.Dispose();
            } catch { }
        }
        _watchers.Clear();
    }

    private void DisposeWatcher(string normalizedPath) {
        if (_watchers.TryRemove(normalizedPath.ToUpperInvariant(), out var watcher)) {
            try {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            } catch { }
        }
    }

    /// <summary>
    /// Stellt sicher, dass für das angegebene Verzeichnis ein Watcher aktiv ist.
    /// Wird beim ersten Aufruf von Get(path) ausgeführt.
    /// </summary>
    private void EnsureWatcher(string path) {
        if (IsDisposed) { return; }
        var normalizedPath = path.NormalizePath();
        var key = normalizedPath.ToUpperInvariant();

        if (!DirectoryExists(normalizedPath)) { return; }
        if (_watchers.ContainsKey(key)) { return; }

        _watcherLock.EnterWriteLock();
        try {
            if (_watchers.ContainsKey(key)) { return; }
            var watcher = CreateWatcher(normalizedPath);
            _watchers.TryAdd(key, watcher);
            WarmCache(normalizedPath);
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }
        var key = e.FullPath.NormalizeFile().ToUpperInvariant();
        if (_cachedFiles.TryGetValue(key, out var file)) { file.Invalidate(); } else { AddToCache(e.FullPath); }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) {
        if (IsDisposed || !ShouldCacheFile(e.FullPath)) { return; }
        AddToCache(e.FullPath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        RemoveFromCache(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e) {
        if (IsDisposed) { return; }
        RemoveFromCache(e.OldFullPath);
        if (ShouldCacheFile(e.FullPath)) { AddToCache(e.FullPath); }
    }

    private void OnWatcherError(string watchedPath, ErrorEventArgs e) {
        if (IsDisposed) { return; }
        Task.Run(async () => {
            var attempts = 0;
            do {
                await Task.Delay(2000).ConfigureAwait(false);
                attempts++;
                try {
                    var currentFiles = GetAllMatchingFiles(watchedPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var watchedKey = watchedPath.ToUpperInvariant();

                    foreach (var key in _cachedFiles.Keys.Where(k => k.StartsWith(watchedKey) && !currentFiles.Contains(k))) {
                        RemoveFromCache(key);
                    }

                    foreach (var file in currentFiles) {
                        var key = file.NormalizeFile().ToUpperInvariant();
                        if (_cachedFiles.TryGetValue(key, out var f)) { if (f.IsStale()) f.Invalidate(); } else { AddToCache(file); }
                    }

                    DisposeWatcher(watchedPath);
                    var newWatcher = CreateWatcher(watchedPath);
                    _watchers.TryAdd(watchedKey, newWatcher);
                    return;
                } catch { }
            } while (attempts < 15);
        });
    }

    private void RemoveFromCache(string filename) {
        if (string.IsNullOrEmpty(filename))
            return;
        var key = filename.NormalizeFile().ToUpperInvariant();
        if (_cachedFiles.TryRemove(key, out var file)) { file.Dispose(); }
    }

    private bool ShouldCacheFile(string filename) {
        if (IsDisposed) { return false; }
        if (!IsSupportedSuffix(Path.GetExtension(filename))) { return false; }
        var upperFile = filename.NormalizeFile().ToUpperInvariant();
        return _watchers.Keys.Any(watchedPath => upperFile.StartsWith(watchedPath));
    }

    private void WarmCache(string normalizedPath) {
        var files = GetAllMatchingFiles(normalizedPath);
        foreach (var filePath in files) {
            try { AddToCache(filePath); } catch { }
        }
    }

    #endregion
}