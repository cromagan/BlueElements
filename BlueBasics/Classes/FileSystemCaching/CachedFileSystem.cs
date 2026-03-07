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
/// Existiert als eine einzige globale Instanz (Singleton), die sich über statische
/// Methoden selbst aktiviert. Pro überwachtem Verzeichnis wird ein eigener
/// FileSystemWatcher erzeugt.
/// Unbekannte Dateiendungen werden still ignoriert.
/// </summary>
public sealed class CachedFileSystem : IDisposableExtended {

    #region Fields

    /// <summary>
    /// Intervall in Millisekunden für die globale Stale-Prüfung aller gecachten Dateien.
    /// </summary>
    private const int StaleCheckIntervalMs = 180000;

    /// <summary>
    /// Die einzige globale Instanz.
    /// </summary>
    private static readonly CachedFileSystem _globalInstance = new();

    /// <summary>
    /// Mapping von Datei-Suffix auf den zugehörigen CachedFile-Ableitungstyp.
    /// Wird einmalig per Reflection befüllt. Berücksichtigt AllowMultiple auf FileSuffixAttribute.
    /// </summary>
    private static readonly Lazy<Dictionary<string, Type>> _suffixTypeMap = new(BuildSuffixTypeMap);

    private static Timer? _staleCheckTimer;
    private static readonly object _staleTimerLock = new();

    private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new();

    /// <summary>
    /// Pro überwachtem Verzeichnis ein FileSystemWatcher.
    /// Key = normalisierter Pfad (Uppercase).
    /// </summary>
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();

    private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);
    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    private CachedFileSystem() {
        StartStaleCheckTimer();
    }

    #endregion

    #region Properties

    public bool IsDisposed => _isDisposedFlag == 1;

    #endregion

    #region Methods

    // -----------------------------------------------------------------------
    // Statische API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disposed die globale Instanz und alle Ressourcen.
    /// </summary>
    public static void DisposeAll() => _globalInstance.Dispose();

    /// <summary>
    /// Gibt die globale CachedFileSystem-Instanz zurück und stellt sicher,
    /// dass das angegebene Verzeichnis überwacht wird.
    /// </summary>
    public static CachedFileSystem Get(string path) {
        _globalInstance.EnsureWatcher(path);
        return _globalInstance;
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
    public static bool IsSupportedSuffix(string suffix) {
        if (string.IsNullOrEmpty(suffix)) { return false; }
        return _suffixTypeMap.Value.ContainsKey(suffix.ToUpperInvariant());
    }

    /// <summary>
    /// Startet den globalen Stale-Check-Timer, falls noch nicht aktiv.
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

        if (_globalInstance._cachedFiles.TryRemove(key, out var file)) {
            file.Dispose();
        }

        return DeleteFile(normalizedFile, false);
    }

    /// <summary>
    /// Löscht die Blockdatei (.blk) für die angegebene Datei.
    /// </summary>
    /// <returns>True wenn erfolgreich gelöscht.</returns>
    public static bool DeleteBlockFile(string filename) {
        var blkName = CachedTextFile.GetBlockFilename(filename);
        return DeleteFile(blkName, false);
    }

    /// <summary>
    /// Callback des globalen Stale-Check-Timers.
    /// Reihenfolge:
    ///   1. IsStale → Invalidate (Änderungen verwerfen) → continue
    ///   2. Blockfile vorhanden → continue (andere Instanz bearbeitet)
    ///   3. !IsSaved → DoExtendedSave
    /// </summary>
    private static async void StaleCheckCallback() {
        foreach (var file in _globalInstance._cachedFiles.Values) {
            if (file.IsDisposed) { continue; }

            // 1. IsStale → Änderungen verwerfen (Invalidate setzt _isSaved=true)
            if (file.IsStale()) {
                file.Invalidate();
                continue;
            }

            // 2. Blockfile vorhanden → jemand anderes bearbeitet → nicht speichern
            var blkAge = CachedTextFile.AgeOfBlockFile(file.Filename);
            if (blkAge is >= 0 and <= 3600) { continue; }

            // 3. Ungespeicherte Änderungen → speichern
            if (!file.IsSaved) {
                try {
                    await file.DoExtendedSave().ConfigureAwait(false);
                } catch {
                    // Beim nächsten Tick erneut versuchen
                }
            }
        }
    }

    // -----------------------------------------------------------------------
    // Instanz-Methoden
    // -----------------------------------------------------------------------

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }

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

    /// <summary>
    /// Holt oder erstellt eine gecachte Datei.
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
    /// Gibt alle gecachten Dateipfade zurück, optional gefiltert nach Pattern.
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

    // -----------------------------------------------------------------------
    // Private Hilfsmethoden
    // -----------------------------------------------------------------------

    /// <summary>
    /// Stellt sicher, dass für das angegebene Verzeichnis ein Watcher aktiv ist.
    /// Wird beim ersten Aufruf von Get(path) ausgeführt.
    /// </summary>
    private void EnsureWatcher(string path) {
        if (IsDisposed) { return; }

        var normalizedPath = path.NormalizePath().ToUpperInvariant();

        if (!DirectoryExists(normalizedPath)) {
            Develop.DebugPrint(Enums.ErrorType.Error, $"Verzeichnis nicht gefunden: {normalizedPath}");
            return;
        }

        if (_watchers.ContainsKey(normalizedPath)) { return; }

        _watcherLock.EnterWriteLock();
        try {
            if (_watchers.ContainsKey(normalizedPath)) { return; }

            var watcher = CreateWatcher(normalizedPath);
            _watchers.TryAdd(normalizedPath, watcher);
        } finally {
            try { _watcherLock.ExitWriteLock(); } catch { }
        }

        WarmCache(normalizedPath);
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
        if (_watchers.TryRemove(normalizedPath, out var watcher)) {
            try {
                watcher.EnableRaisingEvents = false;
                Task.Run(watcher.Dispose);
            } catch { }
        }
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

    private static bool MatchesPattern(string fileName, string pattern) {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
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

        if (!ReferenceEquals(added, newFile)) {
            newFile.Dispose();
        }

        return added;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!ShouldCacheFile(e.FullPath)) { return; }

        var key = e.FullPath.NormalizeFile().ToUpperInvariant();
        if (_cachedFiles.TryGetValue(key, out var file)) {
            file.Invalidate();
        } else {
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

    private void OnWatcherError(string watchedPath, ErrorEventArgs e) {
        if (IsDisposed) { return; }

        Task.Run(async () => {
            var attempts = 0;

            do {
                await Task.Delay(2000).ConfigureAwait(false);
                attempts++;

                try {
                    var currentFiles = GetAllMatchingFiles(watchedPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
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

                    // Watcher neu erstellen
                    DisposeWatcher(watchedPath);
                    var newWatcher = CreateWatcher(watchedPath);
                    _watchers.TryAdd(watchedPath, newWatcher);
                    return;
                } catch { }
            } while (attempts < 15);

            Develop.DebugPrint(Enums.ErrorType.Error, $"FileSystemWatcher für '{watchedPath}' konnte nicht wiederhergestellt werden.");
        });
    }

    private void RemoveFromCache(string filename) {
        if (IsDisposed) { return; }
        if (_cachedFiles.TryRemove(filename.NormalizeFile().ToUpperInvariant(), out var file)) {
            file.Dispose();
        }
    }

    private bool ShouldCacheFile(string filename) {
        if (IsDisposed) { return false; }
        var normalizedFile = filename.NormalizeFile();

        // Prüfen ob die Datei zu einem der überwachten Verzeichnisse gehört
        foreach (var watchedPath in _watchers.Keys) {
            if (normalizedFile.StartsWith(watchedPath, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }

    private static List<string> GetAllMatchingFiles(string watchedPath) {
        var allFiles = Directory.GetFiles(watchedPath, "*.*", SearchOption.AllDirectories);
        return [.. allFiles.Where(f => IsSupportedSuffix(Path.GetExtension(f)))];
    }

    private void WarmCache(string normalizedPath) {
        var files = GetAllMatchingFiles(normalizedPath);
        foreach (var filePath in files) {
            try {
                AddToCache(filePath);
            } catch {
                Develop.AbortAppIfStackOverflow();
                WarmCache(normalizedPath);
            }
        }
    }

    #endregion
}
