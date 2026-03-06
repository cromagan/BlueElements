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
/// Unbekannte Dateiendungen werden still ignoriert.
/// </summary>
public sealed class CachedFileSystem : IDisposableExtended {

    #region Fields

    /// <summary>
    /// Intervall in Millisekunden für die globale Stale-Prüfung aller gecachten Dateien.
    /// </summary>
    private const int StaleCheckIntervalMs = 180000;

    private static readonly SemaphoreSlim _globalInstanceLock = new(1, 1);
    private static readonly ConcurrentDictionary<string, CachedFileSystem> _instances = new();

    /// <summary>
    /// Globaler Timer für die Stale-Prüfung aller gecachten Dateien.
    /// Ein einziger Timer für alle CachedFileSystem-Instanzen.
    /// </summary>
    private static Timer? _staleCheckTimer;
    private static readonly object _staleTimerLock = new();

    /// <summary>
    /// Mapping von Datei-Suffix auf den zugehörigen CachedFile-Ableitungstyp.
    /// Wird einmalig per Reflection befüllt. Berücksichtigt AllowMultiple auf FileSuffixAttribute.
    /// </summary>
    private static readonly Lazy<Dictionary<string, Type>> _suffixTypeMap = new(BuildSuffixTypeMap);

    private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new();
    private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);
    private volatile int _isDisposedFlag;
    private FileSystemWatcher? _watcher;

    #endregion

    #region Constructors

    private CachedFileSystem(string watchedDirectory) {
        WatchedDirectory = watchedDirectory;
        StartStaleCheckTimer();
    }

    #endregion

    #region Properties

    public bool IsDisposed => _isDisposedFlag == 1;

    public string WatchedDirectory {
        get {
            if (IsDisposed) { return string.Empty; }
            return field;
        }
        set {
            if (IsDisposed) { return; }

            value = value.NormalizePath().ToUpperInvariant();

            if (!DirectoryExists(value)) {
                Develop.DebugPrint(Enums.ErrorType.Error, $"Verzeichnis nicht gefunden: {value}");
                return;
            }

            if (!string.IsNullOrEmpty(field) && !IsSubPath(value, field)) {
                Develop.DebugPrint(Enums.ErrorType.Error, $"Unerlaubte Modifikation: {field} -> {value}");
                return;
            }

            _watcherLock.EnterWriteLock();
            try {
                DisposeWatcher();
                _instances.TryRemove(field, out _);
                field = value;
                InitializeWatcher();
                _instances.TryAdd(field, this);
            } finally {
                _watcherLock.ExitWriteLock();
                WarmCache();
            }
        }
    } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Disposed alle CachedFileSystem-Instanzen.
    /// </summary>
    public static void DisposeAll() {
        _globalInstanceLock.Wait();
        try {
            foreach (var kvp in _instances.Values) {
                kvp.Dispose();
            }
        } finally {
            _globalInstanceLock.Release();
        }
    }

    /// <summary>
    /// Holt oder erstellt eine CachedFileSystem-Instanz für das angegebene Verzeichnis.
    /// </summary>
    public static CachedFileSystem Get(string path) {
        var normalizedPath = path.NormalizePath().ToUpperInvariant();

        if (_instances.TryGetValue(normalizedPath, out var existingInstance)) {
            return existingInstance;
        }

        _globalInstanceLock.Wait();
        try {
            if (_instances.TryGetValue(normalizedPath, out existingInstance)) {
                return existingInstance;
            }

            var parentInstanceKvp = FindParentInstance(normalizedPath);
            if (parentInstanceKvp is { }) { return parentInstanceKvp; }

            var firstChild = FindChildInstances(normalizedPath);
            if (firstChild is { }) {
                firstChild.WatchedDirectory = path;
                return firstChild;
            }

            return new CachedFileSystem(path);
        } finally {
            _globalInstanceLock.Release();
        }
    }

    /// <summary>
    /// Gibt alle gecachten Instanzen eines bestimmten Typs zurück.
    /// Durchsucht ALLE CachedFileSystem-Instanzen.
    /// </summary>
    public static List<T> GetAll<T>() where T : CachedFile {
        var result = new List<T>();

        foreach (var instance in _instances.Values) {
            if (instance.IsDisposed) { continue; }

            foreach (var file in instance._cachedFiles.Values) {
                if (file is T typed) {
                    result.Add(typed);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Prüft, ob ein Datei-Suffix einem bekannten Typ zugeordnet ist.
    /// </summary>
    public static bool IsSupportedSuffix(string suffix) {
        if (string.IsNullOrEmpty(suffix)) { return false; }
        return _suffixTypeMap.Value.ContainsKey(suffix.ToUpperInvariant());
    }

    /// <summary>
    /// Startet den globalen Stale-Check-Timer, falls noch nicht aktiv.
    /// Ein einziger Timer für alle CachedFileSystem-Instanzen.
    /// Prüft alle gecachten Dateien auf Aktualität und speichert ungespeicherte Änderungen.
    /// </summary>
    public static void StartStaleCheckTimer() {
        lock (_staleTimerLock) {
            _staleCheckTimer ??= new Timer(
                callback: _ => StaleCheckCallback(),
                state: null,
                dueTime: StaleCheckIntervalMs,
                period: StaleCheckIntervalMs
            );
        }
    }

    /// <summary>
    /// Stoppt den globalen Stale-Check-Timer.
    /// </summary>
    public static void StopStaleCheckTimer() {
        lock (_staleTimerLock) {
            _staleCheckTimer?.Dispose();
            _staleCheckTimer = null;
        }
    }

    /// <summary>
    /// Löscht eine Datei mit bekanntem Suffix.
    /// Die Instanz wird disposed und aus dem Cache entfernt. Die Datei wird vom Dateisystem gelöscht.
    /// </summary>
    /// <returns>True wenn erfolgreich, false wenn das Suffix unbekannt ist oder die Löschung fehlschlägt.</returns>
    public static bool Delete(string filename) {
        if (string.IsNullOrEmpty(filename)) { return false; }

        var normalizedFile = filename.NormalizeFile();
        var suffix = Path.GetExtension(normalizedFile);
        if (!IsSupportedSuffix(suffix)) { return false; }

        var key = normalizedFile.ToUpperInvariant();

        foreach (var instance in _instances.Values) {
            if (instance.IsDisposed) { continue; }
            if (instance._cachedFiles.TryRemove(key, out var file)) {
                file.Dispose();
                break;
            }
        }

        return DeleteFile(normalizedFile, false);
    }

    /// <summary>
    /// Gibt den Dateinamen der Blockdatei (.blk) für die angegebene Datei zurück.
    /// </summary>
    public static string GetBlockFilename(string filename) =>
        string.IsNullOrEmpty(filename) ? string.Empty :
        filename.FilePath() + filename.FileNameWithoutSuffix() + ".blk";

    /// <summary>
    /// Erstellt eine Blockdatei (.blk) für die angegebene Datei mit dem übergebenen Inhalt.
    /// </summary>
    public static void CreateBlockFile(string filename, string content) {
        var blkName = GetBlockFilename(filename);
        DeleteFile(blkName, 20);
        WriteAllText(blkName, content, Constants.Win1252, false);
    }

    /// <summary>
    /// Löscht die Blockdatei (.blk) für die angegebene Datei.
    /// </summary>
    /// <returns>True wenn erfolgreich gelöscht.</returns>
    public static bool DeleteBlockFile(string filename) {
        var blkName = GetBlockFilename(filename);
        return DeleteFile(blkName, false);
    }

    /// <summary>
    /// Gibt das Alter der Blockdatei in Sekunden zurück.
    /// -1 wenn keine Blockdatei vorhanden ist.
    /// </summary>
    public static double AgeOfBlockFile(string filename) {
        var blkName = GetBlockFilename(filename);
        if (!BlueBasics.ClassesStatic.IO.FileExists(blkName)) { return -1; }
        var f = GetFileInfo(blkName);
        if (f == null) { return -1; }
        return Math.Max(0, DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds);
    }

    /// <summary>
    /// Liest den Inhalt der Blockdatei (.blk).
    /// </summary>
    public static string ReadBlockFileContent(string filename) {
        var blkName = GetBlockFilename(filename);
        return ReadAllText(blkName, Constants.Win1252);
    }

    /// <summary>
    /// Callback des globalen Stale-Check-Timers.
    /// Prüft alle gecachten Dateien auf Aktualität und speichert ungespeicherte Änderungen.
    /// </summary>
    private static async void StaleCheckCallback() {
        foreach (var instance in _instances.Values) {
            if (instance.IsDisposed) { continue; }

            foreach (var file in instance._cachedFiles.Values) {
                if (file.IsDisposed) { continue; }

                // Zuerst ungespeicherte Änderungen sichern
                if (!file.IsSaved) {
                    try {
                        await file.DoExtendedSave().ConfigureAwait(false);
                    } catch {
                        // Fehler beim Speichern — beim nächsten Tick erneut versuchen
                    }
                }

                // Dann Aktualität prüfen
                if (file.IsStale()) {
                    file.Invalidate();
                }
            }
        }
    }

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }

        var lockAcquired = false;

        try {
            _watcherLock.EnterWriteLock();
            lockAcquired = true;
            _instances.TryRemove(WatchedDirectory, out _);

            DisposeWatcher();

            foreach (var file in _cachedFiles.Values) {
                file.Dispose();
            }
            _cachedFiles.Clear();
        } finally {
            if (lockAcquired) {
                try { _watcherLock.ExitWriteLock(); } catch { }
            }
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

    /// <summary>
    /// Holt eine gecachte Datei aus dem Cache.
    /// Gibt null zurück, wenn die Datei nicht im Cache ist oder der Typ nicht passt.
    /// </summary>
    public T? GetOrCreate<T>(string filename) where T : CachedFile {
        if (IsDisposed) { return null; }

        var normalizedFileName = filename.NormalizeFile();
        var key = normalizedFileName.ToUpperInvariant();

        if (_cachedFiles.TryGetValue(key, out var existing)) {
            return existing as T;
        }

        // Neuen Eintrag über Factory erzeugen
        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; } // Unbekanntes Suffix

        var added = _cachedFiles.GetOrAdd(key, newFile);

        // Falls ein anderer Thread schneller war, die neue Instanz verwerfen
        if (!ReferenceEquals(added, newFile)) {
            newFile.Dispose();
        }

        return added as T;
    }

    /// <summary>
    /// Gibt alle gecachten Dateien zurück, optional gefiltert nach Patterns.
    /// </summary>
    public List<string> GetFiles(string path, List<string>? includePatterns = null) {
        if (IsDisposed) { return []; }

        var normalizedPath = path.NormalizePath();

        var filesInPath = _cachedFiles.Keys
            .Where(filename => filename.FilePath().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (includePatterns == null || includePatterns.Count == 0) {
            return [.. filesInPath];
        }

        return [.. filesInPath.Where(filename => includePatterns.Exists(pattern => MatchesPattern(filename, pattern)))];
    }

    /// <summary>
    /// Erstellt das Suffix-zu-Typ-Mapping per Reflection.
    /// Berücksichtigt AllowMultiple — eine Klasse kann mehrere Suffixe registrieren.
    /// </summary>
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

    /// <summary>
    /// Erstellt eine CachedFile-Instanz des richtigen Typs basierend auf der Dateiendung.
    /// Gibt null zurück, wenn das Suffix nicht registriert ist.
    /// </summary>
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

    private static CachedFileSystem? FindChildInstances(string parentPath) {
        foreach (var kvp in _instances) {
            if (IsSubPath(parentPath, kvp.Key)) {
                return kvp.Value;
            }
        }
        return null;
    }

    private static CachedFileSystem? FindParentInstance(string childPath) {
        CachedFileSystem? bestMatch = null;
        var longestMatchLength = 0;

        foreach (var kvp in _instances) {
            if (IsSubPath(kvp.Key, childPath)) {
                var matchLength = kvp.Key.Length;
                if (matchLength > longestMatchLength) {
                    longestMatchLength = matchLength;
                    bestMatch = kvp.Value;
                }
            }
        }
        return bestMatch;
    }

    private static bool IsSubPath(string parentPath, string childPath) {
        var normalizedParentPath = parentPath.NormalizePath();
        var normalizedChildPath = childPath.NormalizePath();

        if (normalizedParentPath.Equals(normalizedChildPath, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        return normalizedChildPath.StartsWith(normalizedParentPath, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesPattern(string fileName, string pattern) {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Fügt eine Datei zum Cache hinzu — immer im richtigen Typ.
    /// Unbekannte Suffixe werden still ignoriert (return null).
    /// </summary>
    private CachedFile? AddToCache(string fileName) {
        var normalizedFileName = fileName.NormalizeFile();
        var key = normalizedFileName.ToUpperInvariant();

        if (_cachedFiles.TryGetValue(key, out var existing)) { return existing; }

        var newFile = CreateCachedFile(normalizedFileName);
        if (newFile == null) { return null; } // Unbekanntes Suffix → ignorieren

        var added = _cachedFiles.GetOrAdd(key, newFile);

        if (!ReferenceEquals(added, newFile)) {
            newFile.Dispose();
        }

        return added;
    }

    private void DisposeWatcher() {
        try {
            _watcherLock.EnterWriteLock();
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Changed -= OnFileChanged;
                _watcher.Deleted -= OnFileDeleted;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Error -= OnWatcherError;

                var watcherToDispose = _watcher;
                _watcher = null;

                Task.Run(watcherToDispose.Dispose);
            }
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { }
        }
    }

    private List<string> GetAllMatchingFiles() {
        var allFiles = Directory.GetFiles(WatchedDirectory, "*.*", SearchOption.AllDirectories);
        return [.. allFiles.Where(f => IsSupportedSuffix(Path.GetExtension(f)) && ShouldCacheFile(f))];
    }

    private void InitializeWatcher() {
        if (IsDisposed) { return; }

        try {
            _watcherLock.EnterWriteLock();
            DisposeWatcher();

            _watcher = new FileSystemWatcher(WatchedDirectory) {
                NotifyFilter = NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size
                             | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                InternalBufferSize = 64 * 1024
            };

            _watcher.Created += OnFileCreated;
            _watcher.Changed += OnFileChanged;
            _watcher.Deleted += OnFileDeleted;
            _watcher.Renamed += OnFileRenamed;
            _watcher.Error += OnWatcherError;

            _watcher.EnableRaisingEvents = true;
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }

        var key = e.FullPath.NormalizeFile().ToUpperInvariant();
        if (_cachedFiles.TryGetValue(key, out var file)) {
            file.Invalidate();
        } else {
            // Neue Datei mit bekanntem Suffix → aufnehmen
            AddToCache(e.FullPath);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }

        AddToCache(e.FullPath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        RemoveFromCache(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e) {
        if (IsDisposed) { return; }

        RemoveFromCache(e.OldFullPath);

        if (!ShouldCacheFile(e.FullPath)) { return; }
        AddToCache(e.FullPath);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e) {
        if (IsDisposed) { return; }

        Task.Run(async () => {
            var attempts = 0;

            do {
                await Task.Delay(2000).ConfigureAwait(false);
                attempts++;

                try {
                    var currentFiles = GetAllMatchingFiles().ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var cachedKeys = _cachedFiles.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var removed in cachedKeys.Except(currentFiles)) {
                        RemoveFromCache(removed);
                    }

                    foreach (var file in currentFiles.Except(cachedKeys)) {
                        AddToCache(file);
                    }

                    foreach (var file in currentFiles) {
                        var key = file.NormalizeFile().ToUpperInvariant();
                        if (_cachedFiles.TryGetValue(key, out var f) && f.IsStale()) {
                            f.Invalidate();
                        }
                    }

                    InitializeWatcher();
                    return;
                } catch { }
            } while (attempts < 15);

            Develop.DebugPrint(Enums.ErrorType.Error, $"FileSystemWatcher für '{WatchedDirectory}' konnte nicht wiederhergestellt werden.");
        });
    }

    private void RemoveFromCache(string filename) {
        if (IsDisposed || string.IsNullOrEmpty(WatchedDirectory)) { return; }
        if (_cachedFiles.TryRemove(filename.NormalizeFile().ToUpperInvariant(), out var file)) {
            file.Dispose();
        }
    }

    private bool ShouldCacheFile(string filename) {
        if (IsDisposed || string.IsNullOrEmpty(WatchedDirectory)) { return false; }
        return filename.NormalizeFile().StartsWith(WatchedDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private void WarmCache() {
        var files = GetAllMatchingFiles();
        foreach (var filePath in files) {
            try {
                AddToCache(filePath);
            } catch {
                Develop.AbortAppIfStackOverflow();
                WarmCache();
            }
        }
    }

    #endregion
}
