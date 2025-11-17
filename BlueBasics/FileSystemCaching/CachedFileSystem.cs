// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace BlueBasics.FileSystemCaching {

    /// <summary>
    /// Hauptklasse für das Caching-System mit automatischer Synchronisation
    /// </summary>
    public sealed class CachedFileSystem : IDisposableExtended {

        #region Fields

        private static readonly SemaphoreSlim _globalInstanceLock = new(1, 1);

        private static readonly ConcurrentDictionary<string, CachedFileSystem> _instances = new();

        private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new();

        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.SupportsRecursion);

        private volatile int _isDisposedFlag = 0;
        private FileSystemWatcher? _watcher;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine neue CachedFileSystem-Instanz
        /// </summary>
        private CachedFileSystem(string watchedDirectory) {
            WatchedDirectory = watchedDirectory;
        }

        #endregion

        #region Properties

        /// <summary>Debouncing-Verzögerung für FileSystemWatcher-Events (ms)</summary>
        public int DebounceDelayMs => 500;

        // Property anpassen:
        public bool IsDisposed => _isDisposedFlag == 1;

        /// <summary>Überwachtes Verzeichnis</summary>
        public string WatchedDirectory {
            get {
                if (IsDisposed) { return string.Empty; }
                return field;
            }
            set {
                if (IsDisposed) { return; }

                value = IO.NormalizePath(value).ToUpperInvariant();

                if (!IO.DirectoryExists(value)) {
                    Develop.DebugPrint(Enums.ErrorType.Error, $"Verzeichnis nicht gefunden: {value}");
                    return;
                }

                if (!string.IsNullOrEmpty(field) && !IsSubPath(value, field)) {
                    Develop.DebugPrint(Enums.ErrorType.Error, $"Unerlaubte Modifikation: {field} -> {value}");
                    return;
                }

                // Watcher stoppen und neu initialisieren
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
        /// Disposed alle Instanzen
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
        /// Holt oder erstellt eine CachedFileSystem-Instanz für das angegebene Verzeichnis
        /// </summary>
        public static CachedFileSystem Get(string path) {
            var normalizedPath = IO.NormalizePath(path).ToUpperInvariant();

            // Fast-Path: Instanz existiert bereits exakt
            if (_instances.TryGetValue(normalizedPath, out var existingInstance)) {
                return existingInstance;
            }

            // Slow-Path: Hierarchie-Analyse erforderlich
            _globalInstanceLock.Wait();
            try {
                // Double-Check nach Lock
                if (_instances.TryGetValue(normalizedPath, out existingInstance)) {
                    return existingInstance;
                }

                // Szenario 1: Gibt es eine Parent-Instanz (flacherer Pfad)?
                var parentInstanceKvp = FindParentInstance(normalizedPath);
                if (parentInstanceKvp is { }) { return parentInstanceKvp; }

                // Szenario 2: Gibt es Child-Instanzen (tiefere Pfade)?
                var firstChild = FindChildInstances(normalizedPath);
                if (firstChild is { }) {
                    // Verzeichnis der Child-Instanz ändern
                    firstChild.WatchedDirectory = path;
                    return firstChild;
                }

                // Szenario 3: Keine Hierarchie-Konflikte -> neue Instanz erstellen
                return new CachedFileSystem(path);
            } finally {
                _globalInstanceLock.Release();
            }
        }

        public void Dispose() {
            // Atomare Prüfung und Setzen: Gibt vorherigen Wert zurück
            if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }

            var lockAcquired = false;

            try {
                _watcherLock.EnterWriteLock();
                lockAcquired = true;
                _instances.TryRemove(WatchedDirectory, out _);

                DisposeWatcher();

                // Alle CachedFile-Instanzen disposen
                foreach (var file in _cachedFiles.Values) {
                    file.Dispose();
                }
                _cachedFiles.Clear();
            } finally {
                if (lockAcquired) {
                    try {
                        _watcherLock.ExitWriteLock();
                    } catch { }
                }
            }

            _watcherLock.Dispose();
            _initializationLock.Dispose();
        }

        /// <summary>
        /// Prüft, ob eine Datei existiert
        /// </summary>
        public bool FileExists(string filename) {
            if (IsDisposed) { return false; }

            var normalizedFileName = IO.NormalizeFile(filename);

            if (!ShouldCacheFile(normalizedFileName)) {
                Develop.DebugPrint(Enums.ErrorType.Error, $"Der angegebene Pfad '{filename}' liegt nicht im überwachten Bereich '{WatchedDirectory}'.");
            }

            // Erst Cache prüfen
            return _cachedFiles.TryGetValue(normalizedFileName.ToUpperInvariant(), out _);
        }

        /// <summary>
        /// Liest eine Datei aus dem Cache oder Dateisystem
        /// </summary>
        public CachedFile? GetFile(string filename) {
            if (IsDisposed) { return null; }

            var normalizedFileName = IO.NormalizeFile(filename);

            if (!ShouldCacheFile(normalizedFileName)) {
                Develop.DebugPrint(Enums.ErrorType.Error, $"Der angegebene Pfad '{filename}' liegt nicht im überwachten Bereich '{WatchedDirectory}'.");
            }

            return AddToCache(normalizedFileName);
        }

        /// <summary>
        /// Gibt alle gecachten Dateien zurück, optional gefiltert nach Patterns
        /// </summary>
        /// <param name="path">Pfad, aus dem die Dateien abgerufen werden sollen</param>
        /// <param name="includePatterns">Optionale Filter-Patterns (z.B. "*.txt", "*.json"). Wenn null, werden alle gecachten Dateien zurückgegeben.</param>
        /// <returns>Liste der Dateipfade, die den Patterns entsprechen</returns>
        /// <exception cref="ArgumentException">Wenn der Pfad nicht im Watcher-Bereich liegt</exception>
        public List<string> GetFiles(string path, List<string>? includePatterns = null) {
            if (IsDisposed) { return []; }

            var normalizedPath = IO.NormalizePath(path);

            // Prüfen, ob Pfad im Watcher-Bereich liegt
            if (!ShouldCacheFile(normalizedPath)) {
                Develop.DebugPrint(Enums.ErrorType.Error, $"Der angegebene Pfad '{normalizedPath}' liegt nicht im überwachten Bereich '{WatchedDirectory}'.");
            }

            // Dateien filtern, die im angegebenen Pfad liegen
            var filesInPath = _cachedFiles.Keys.Where(filename => {
                return filename.FilePath().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            if (includePatterns == null || includePatterns.Count == 0) {
                return [.. filesInPath];
            }

            return [.. filesInPath.Where(filename => {
                return includePatterns.Any(pattern => MatchesPattern(filename, pattern));
            })];
        }

        /// <summary>
        /// Liest den gesamten Inhalt einer Datei als Text
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        /// <param name="encoding">Encoding (Standard: UTF8)</param>
        /// <returns>Dateiinhalt als String</returns>
        public string ReadAllText(string filename, Encoding? encoding = null) {
            if (IsDisposed) { return string.Empty; }

            encoding ??= Encoding.UTF8;

            var cachedFile = GetFile(filename);
            return cachedFile?.GetContentAsString(encoding) ?? string.Empty;
        }

        /// <summary>
        /// Findet eine Isntanz, die Unterpfade des angegebenen Pfades sind
        /// </summary>
        private static CachedFileSystem? FindChildInstances(string parentPath) {
            foreach (var kvp in _instances) {
                if (IsSubPath(parentPath, kvp.Key)) {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Findet die Instanz mit dem längsten gemeinsamen Pfad-Präfix
        /// </summary>
        private static CachedFileSystem? FindParentInstance(string childPath) {
            CachedFileSystem? bestMatch = null;
            int longestMatchLength = 0;

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

        /// <summary>
        /// Prüft, ob childPath ein Unterpfad von parentPath ist
        /// </summary>
        private static bool IsSubPath(string parentPath, string childPath) {
            var normalizedParentPath = IO.NormalizePath(parentPath);
            var normalizedChildPath = IO.NormalizePath(childPath);

            if (normalizedParentPath.Equals(normalizedChildPath, StringComparison.OrdinalIgnoreCase)) {
                return false; // Gleicher Pfad, kein Unterpfad
            }

            return normalizedChildPath.StartsWith(normalizedParentPath, StringComparison.OrdinalIgnoreCase);
        }

        private CachedFile AddToCache(string fileName) {
            var normalizedFileName = IO.NormalizeFile(fileName);

            return _cachedFiles.GetOrAdd(normalizedFileName.ToUpperInvariant(),
                key => new CachedFile(key));
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
                    _watcher.Dispose();
                    _watcher = null;
                }
            } finally {
                try {
                    _watcherLock.ExitWriteLock();
                } catch { }
            }
        }

        private List<string> GetAllMatchingFiles() {
            var allFiles = Directory.GetFiles(WatchedDirectory, "*.*", SearchOption.AllDirectories);
            return [.. allFiles.Where(ShouldCacheFile)];
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
                try {
                    _watcherLock.ExitWriteLock();
                } catch { }
            }
        }

        private bool MatchesPattern(string fileName, string pattern) {
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(fileName.FileNameWithSuffix(), regexPattern, RegexOptions.IgnoreCase);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e) {
            if (IsDisposed) { return; }
            if (!ShouldCacheFile(e.FullPath)) { return; }

            AddToCache(e.FullPath);
            GetFile(e.FullPath)?.Invalidate();
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
                int attempts = 0;

                do {
                    await Task.Delay(2000).ConfigureAwait(false);
                    attempts++;

                    try {
                        var currentFiles = GetAllMatchingFiles().ToHashSet();
                        var cachedFiles = _cachedFiles.Keys.ToHashSet();

                        // Gelöschte entfernen
                        foreach (var removed in cachedFiles.Except(currentFiles)) {
                            RemoveFromCache(removed);
                        }

                        // Neue nur registrieren (KEINE Metadaten)
                        foreach (var file in currentFiles.Except(cachedFiles)) {
                            AddToCache(file);
                        }

                        // Bestehende Cache-Einträge aktualisieren
                        foreach (var file in currentFiles) {
                            if (GetFile(file) is { } f) {
                                if (f.IsStale()) { f.Invalidate(); }
                            }
                        }

                        InitializeWatcher();
                        return;
                    } catch {
                    }
                } while (attempts < 15);

                // Nach 15 Versuchen (30 Sekunden) aufgeben
                Develop.DebugPrint(Enums.ErrorType.Error, $"FileSystemWatcher für '{WatchedDirectory}' konnte nicht wiederhergestellt werden.");
            });
        }

        private void RemoveFromCache(string filename) {
            if (IsDisposed || string.IsNullOrEmpty(WatchedDirectory)) { return; }
            if (_cachedFiles.TryRemove(IO.NormalizeFile(filename).ToUpperInvariant(), out var file)) {
                file.Dispose();
            }
        }

        private bool ShouldCacheFile(string filename) {
            if (IsDisposed || string.IsNullOrEmpty(WatchedDirectory)) { return false; }

            return IO.NormalizeFile(filename).StartsWith(WatchedDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private void WarmCache() {
            // Nur Dateiliste ermitteln und in _cachedFiles speichern
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
}