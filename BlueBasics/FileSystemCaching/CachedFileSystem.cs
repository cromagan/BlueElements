using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BlueBasics;

#nullable enable

namespace FileSystemCaching {

    /// <summary>
    /// Hauptklasse für das Caching-System mit automatischer Synchronisation
    /// </summary>
    public sealed class CachedFileSystem : IDisposable {

        #region Fields

        private static readonly ConcurrentDictionary<string, CachedFileSystem> _instances = new();

        private readonly ConcurrentDictionary<string, CachedFile> _cachedFiles = new();

        private readonly ConcurrentDictionary<string, Timer> _debounceTimers = new();

        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private readonly ReaderWriterLockSlim _watcherLock = new(LockRecursionPolicy.NoRecursion);

        private volatile bool _isDisposed;

        private volatile bool _isInitialized;

        private FileSystemWatcher? _watcher;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine neue CachedFileSystem-Instanz
        /// </summary>
        private CachedFileSystem(string watchedDirectory) {
            WatchedDirectory = watchedDirectory ?? throw new ArgumentNullException(nameof(watchedDirectory));

            if (!Directory.Exists(WatchedDirectory)) {
                throw new DirectoryNotFoundException($"Verzeichnis nicht gefunden: {WatchedDirectory}");
            }
        }

        #endregion

        #region Properties

        public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromHours(2);

        /// <summary>Debouncing-Verzögerung für FileSystemWatcher-Events (ms)</summary>
        public int DebounceDelayMs { get; set; } = 500;

        /// <summary>Ist das System initialisiert?</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>Parallelitätsgrad für Batch-Operationen</summary>
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>Sliding Expiration für Cache-Einträge (Standard: 30 Minuten)</summary>
        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>Überwachtes Verzeichnis</summary>
        public string WatchedDirectory { get; }

        /// <summary>Buffer-Größe für FileSystemWatcher (Bytes)</summary>
        public int WatcherBufferSize { get; set; } = 1024 * 1024;

        #endregion

        #region Methods

        /// <summary>
        /// Entfernt und disposed eine Instanz
        /// </summary>
        public static void Dispose(string directory) {
            var key = BlueBasics.IO.NormalizePath(directory);
            if (_instances.TryRemove(key, out var instance)) {
                instance.Dispose();
            }
        }

        /// <summary>
        /// Disposed alle Instanzen
        /// </summary>
        public static void DisposeAll() {
            foreach (var kvp in _instances.ToArray()) {
                if (_instances.TryRemove(kvp.Key, out var instance)) {
                    instance.Dispose();
                }
            }
        }

        /// <summary>
        /// Holt oder erstellt eine CachedFileSystem-Instanz für das angegebene Verzeichnis
        /// </summary>
        public static CachedFileSystem Get(string directory) {
            var key = BlueBasics.IO.NormalizePath(directory);

            return _instances.GetOrAdd(key, _ => {
                var instance = new CachedFileSystem(directory);
                instance.InitializeAsync().GetAwaiter().GetResult();
                return instance;
            });
        }

        /// <summary>
        /// Löscht eine Datei synchron und entfernt sie aus dem Cache
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        /// <exception cref="FileNotFoundException">Datei existiert nicht</exception>
        /// <exception cref="IOException">Fehler beim Löschen</exception>
        public void DeleteFile(string filename) {
            ThrowIfDisposed();
            EnsureInitialized();

            var normalizedPath = BlueBasics.IO.NormalizeFile(filename);

            try {
                File.Delete(normalizedPath);

                // Sofort aus Cache entfernen (Watcher macht es dann nochmal, aber das ist idempotent)
                RemoveFromCache(normalizedPath);
            } catch { }
        }

        /// <summary>
        /// Löscht eine Datei asynchron und entfernt sie aus dem Cache
        /// </summary>
        /// <param name="filename">Datei</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <exception cref="FileNotFoundException">Datei existiert nicht</exception>
        /// <exception cref="IOException">Fehler beim Löschen</exception>
        public async Task DeleteFileAsync(string filename, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            EnsureInitialized();

            await Task.Run(() => DeleteFile(filename), cancellationToken)
                .ConfigureAwait(false);
        }

        public void Dispose() {
            if (_isDisposed) return;

            _watcherLock.EnterWriteLock();
            try {
                if (_isDisposed) return;

                _instances.TryRemove(BlueBasics.IO.NormalizePath(WatchedDirectory), out _);

                // Watcher stoppen
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

                // Debounce-Timer aufräumen
                foreach (var timer in _debounceTimers.Values) {
                    timer.Dispose();
                }
                _debounceTimers.Clear();

                // Alle CachedFile-Instanzen disposen
                foreach (var file in _cachedFiles.Values) {
                    file.Dispose();
                }
                _cachedFiles.Clear();

                _isDisposed = true;
            } finally {
                _watcherLock.ExitWriteLock();
            }

            _watcherLock.Dispose();
            _initializationLock.Dispose();
        }

        /// <summary>
        /// Prüft, ob eine Datei existiert
        /// </summary>
        public bool FileExists(string filename) {
            ThrowIfDisposed();
            var normalizedPath = BlueBasics.IO.NormalizeFile(filename);

            // Erst Cache prüfen
            if (_cachedFiles.TryGetValue(normalizedPath, out _)) {
                return true;
            }

            // Dann Dateisystem
            return File.Exists(normalizedPath);
        }

        /// <summary>
        /// Liest eine Datei aus dem Cache oder Dateisystem
        /// </summary>
        public async Task<CachedFile> GetFileAsync(string filename, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            EnsureInitialized();

            var normalizedPath = BlueBasics.IO.NormalizeFile(filename);

            if (!ShouldCacheFile(normalizedPath)) {
                return CreateCachedFileDirectly(normalizedPath);
            }

            // Optimistic read
            if (_cachedFiles.TryGetValue(normalizedPath, out var cachedFile)) {
                if (BlueBasics.Constants.GlobalRnd.Next(0, 5) == 0) {
                    // Fire-and-forget Validierung
                    _ = Task.Run(() => {
                        if (cachedFile.IsStale()) {
                            cachedFile.Refresh();
                        }
                    });
                }

                return cachedFile;
            }

            // Cache-Miss

            // Datei laden und cachen
            var newFile = await Task.Run(() => CreateCachedFileDirectly(normalizedPath), cancellationToken)
                .ConfigureAwait(false);

            // Atomar hinzufügen oder existierende zurückgeben
            return _cachedFiles.GetOrAdd(normalizedPath, newFile);
        }

        /// <summary>
        /// Gibt alle gecachten Dateien zurück, optional gefiltert nach Patterns
        /// </summary>
        /// <param name="includePatterns">Optionale Filter-Patterns (z.B. "*.txt", "*.json"). Wenn null, werden alle gecachten Dateien zurückgegeben.</param>
        /// <returns>Liste der Dateipfade, die den Patterns entsprechen</returns>
        public List<string> GetFiles(List<string>? includePatterns = null) {
            if (includePatterns == null || includePatterns.Count == 0) {
                return [.. _cachedFiles.Keys];
            }

            return [.. _cachedFiles.Keys.Where(filename => {
                var fileName = Path.GetFileName(filename);
                return includePatterns.Any(pattern => MatchesPattern(fileName, pattern));
            })];
        }

        /// <summary>
        /// Initialisiert das Caching-System
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

            if (_isInitialized) return;

            await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                if (_isInitialized) return;

                // FileSystemWatcher einrichten
                InitializeWatcher();

                await WarmCacheAsync(cancellationToken).ConfigureAwait(false);

                _isInitialized = true;
            } finally {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// Liest den gesamten Inhalt einer Datei als Text
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        /// <param name="encoding">Encoding (Standard: UTF8)</param>
        /// <returns>Dateiinhalt als String</returns>
        /// <exception cref="FileNotFoundException">Datei existiert nicht</exception>
        /// <exception cref="IOException">Fehler beim Lesen</exception>
        public string ReadAllText(string filename, Encoding? encoding = null) {
            ThrowIfDisposed();
            EnsureInitialized();

            encoding ??= Encoding.UTF8;
            var normalizedPath = BlueBasics.IO.NormalizeFile(filename);

            try {
                var cachedFile = GetFileAsync(normalizedPath).GetAwaiter().GetResult();
                return cachedFile.GetContentAsString(encoding);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Liest den gesamten Inhalt einer Datei als Text (asynchron)
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        /// <param name="encoding">Encoding (Standard: UTF8)</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Dateiinhalt als String</returns>
        /// <exception cref="FileNotFoundException">Datei existiert nicht</exception>
        /// <exception cref="IOException">Fehler beim Lesen</exception>
        public async Task<string> ReadAllTextAsync(string filename, Encoding? encoding = null, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            EnsureInitialized();

            encoding ??= Encoding.UTF8;
            var normalizedPath = BlueBasics.IO.NormalizeFile(filename);

            try {
                var cachedFile = await GetFileAsync(normalizedPath, cancellationToken).ConfigureAwait(false);
                return cachedFile.GetContentAsString(encoding);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Benennt eine Datei um und aktualisiert den Cache
        /// </summary>
        public async Task RenameFileAsync(string oldFile, string newFile, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            EnsureInitialized();

            var oldNormalized = BlueBasics.IO.NormalizeFile(oldFile);
            var newNormalized = BlueBasics.IO.NormalizeFile(newFile);

            try {
                await Task.Run(() => File.Move(oldNormalized, newNormalized), cancellationToken)
                    .ConfigureAwait(false);

                // Cache aktualisieren
                if (_cachedFiles.TryRemove(oldNormalized, out var cachedFile)) {
                    if (ShouldCacheFile(newNormalized)) {
                        cachedFile.UpdatePath(newNormalized);
                        _cachedFiles.TryAdd(newNormalized, cachedFile);
                    } else {
                        cachedFile.Dispose();
                    }
                }
            } catch { }
        }

        private CachedFile CreateCachedFileDirectly(string filename) {
            return new CachedFile(filename);
        }

        private void DebouncedAction(string key, Action action) {
            var newTimer = new Timer(_ => {
                try {
                    action();
                } finally {
                    if (_debounceTimers.TryRemove(key, out var t)) {
                        t?.Dispose();
                    }
                }
            }, null, DebounceDelayMs, Timeout.Infinite);

            // Atomic swap mit Dispose
            var oldTimer = _debounceTimers.AddOrUpdate(
                key,
                newTimer,
                (_, existing) => {
                    existing.Dispose();
                    return newTimer;
                }
            );

            // Falls AddOrUpdate den addValueFactory-Pfad genommen hat, gibt es keinen oldTimer
            // (newTimer wurde bereits eingefügt)
        }

        private void EnsureInitialized() {
            if (!_isInitialized) {
                throw new InvalidOperationException(
                    "CachedFileSystem ist nicht initialisiert. Rufen Sie InitializeAsync() auf.");
            }
        }

        private List<string> GetAllMatchingFiles() {
            var allFiles = Directory.GetFiles(WatchedDirectory, "*.*", SearchOption.TopDirectoryOnly);
            return [.. allFiles.Where(ShouldCacheFile)];
        }

        private void InitializeWatcher() {
            _watcherLock.EnterWriteLock();
            try {
                _watcher?.Dispose();

                _watcher = new FileSystemWatcher(WatchedDirectory) {
                    NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size
                                 | NotifyFilters.CreationTime,
                    IncludeSubdirectories = false,
                    InternalBufferSize = WatcherBufferSize
                };

                _watcher.Created += OnFileCreated;
                _watcher.Changed += OnFileChanged;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Error += OnWatcherError;

                _watcher.EnableRaisingEvents = true;
            } finally {
                _watcherLock.ExitWriteLock();
            }
        }

        private bool MatchesPattern(string fileName, string pattern) {
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            return Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e) {
            if (!ShouldCacheFile(e.FullPath)) return;

            // Nur reagieren, wenn Datei bereits im Cache ist
            if (!_cachedFiles.TryGetValue(e.FullPath, out var cachedFile)) return;

            DebouncedAction(e.FullPath, cachedFile.Invalidate);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e) {
            if (!ShouldCacheFile(e.FullPath)) return;

            // Nur Dateinamen registrieren, KEINE Metadaten laden
            DebouncedAction(e.FullPath, () => {
                try {
                    var normalizedPath = BlueBasics.IO.NormalizeFile(e.FullPath);

                    // Lazy-Instanz erstellen (ohne Metadaten zu laden)
                    _cachedFiles.GetOrAdd(normalizedPath, _ => new CachedFile(normalizedPath));
                } catch { }
            });
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e) => DebouncedAction(e.FullPath, () => {
            RemoveFromCache(e.FullPath);
        });

        private void OnFileRenamed(object sender, RenamedEventArgs e) => DebouncedAction(e.FullPath, () => {
            // Nur reagieren, wenn alte Datei im Cache war
            if (_cachedFiles.TryRemove(e.OldFullPath, out var cachedFile)) {
                if (ShouldCacheFile(e.FullPath)) {
                    cachedFile.UpdatePath(e.FullPath);
                    _cachedFiles.TryAdd(e.FullPath, cachedFile);
                } else {
                    cachedFile.Dispose();
                }
            } else {
                // Falls alte Datei nicht im Cache war, neue nur registrieren
                if (ShouldCacheFile(e.FullPath)) {
                    _cachedFiles.TryAdd(e.FullPath, new CachedFile(e.FullPath));
                }
            }
        });

        private void OnWatcherError(object sender, ErrorEventArgs e) {
            // Vollständiger Cache-Rescan
            Task.Run(async () => {
                await Task.Delay(1000).ConfigureAwait(false);
                try {
                    var currentFiles = GetAllMatchingFiles().ToHashSet();
                    var cachedFiles = _cachedFiles.Keys.ToHashSet();

                    // Gelöschte entfernen
                    foreach (var removed in cachedFiles.Except(currentFiles)) {
                        RemoveFromCache(removed);
                    }

                    // Neue nur registrieren (KEINE Metadaten)
                    foreach (var file in currentFiles.Except(cachedFiles)) {
                        _cachedFiles.TryAdd(file, new CachedFile(file));
                    }

                    // Bestehende Cache-Einträge aktualisieren
                    foreach (var file in currentFiles) {
                        if (_cachedFiles.TryGetValue(file, out var cached)) {
                            if (cached.IsStale()) cached.Refresh();
                        }
                    }

                    InitializeWatcher();
                } catch { }
            });
        }

        private void RemoveFromCache(string filename) {
            if (_cachedFiles.TryRemove(filename, out var file)) {
                file.Dispose();
            }
        }

        private bool ShouldCacheFile(string filename) {
            var directory = filename.FilePath() ?? string.Empty;
            return directory.Equals(WatchedDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private void ThrowIfDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(CachedFileSystem));
            }
        }

        private async Task WarmCacheAsync(CancellationToken cancellationToken) {
            // Nur Dateiliste ermitteln und in _cachedFiles speichern
            var files = GetAllMatchingFiles();

            await Task.Run(() => {
                foreach (var filePath in files) {
                    try {
                        var normalizedPath = BlueBasics.IO.NormalizeFile(filePath);

                        // Nur CachedFile-Instanz erstellen (KEINE Metadaten laden)
                        _cachedFiles.TryAdd(normalizedPath, new CachedFile(normalizedPath));
                    } catch { }
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}