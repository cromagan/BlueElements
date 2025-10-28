using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace FileSystemCaching {

    /// <summary>
    /// Repräsentiert eine gecachte Datei mit Lazy-Loading-Unterstützung und Versionierung.
    /// Thread-sicher durch Verwendung von ReaderWriterLockSlim.
    /// </summary>
    public sealed class CachedFile : IDisposable {

        #region Fields

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private DateTime? _cachedLastWriteTimeUtc;
        private string _filename;
        private volatile bool _isDisposed;
        private Lazy<byte[]>? _lazyContent;
        private int _version;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine "leere" CachedFile-Instanz OHNE Metadaten (nur Filename).
        /// Wird für WarmCache verwendet, um Speicher zu sparen.
        /// </summary>
        internal CachedFile(string filename, bool loadMetadata) {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));

            if (loadMetadata) {
                Initialize();
            } else {
                // Nur Lazy-Loader initialisieren, KEINE Metadaten laden
                CachedAtUtc = DateTime.UtcNow;
                InitializeLazyLoaders();
            }
        }

        #endregion

        #region Properties

        /// <summary>Zeitpunkt der Cache-Erstellung</summary>
        public DateTime CachedAtUtc { get; private set; }

        public byte[] Content {
            get {
                ThrowIfDisposed();

                // Lazy<T> ist bereits thread-sicher!
                if (_lazyContent == null) {
                    throw new InvalidOperationException("Content not initialized");
                }

                return _lazyContent.Value;  // Lazy kümmert sich um Thread-Safety
            }
        }

        /// <summary>Erstellungsdatum (UTC, gecacht)</summary>
        public DateTime CreationTimeUtc { get; private set; }

        /// <summary>Verzeichnispfad</summary>
        public string Directory => System.IO.Path.GetDirectoryName(Filename) ?? string.Empty;

        /// <summary>Dateierweiterung (inkl. Punkt)</summary>
        public string Extension => System.IO.Path.GetExtension(Filename);

        // Path – read unter Lock mit finally
        public string Filename {
            get {
                _lock.EnterReadLock();
                try {
                    return _filename;
                } finally {
                    _lock.ExitReadLock();
                }
            }
        }

        // IsContentLoaded – garantiert ExitReadLock via finally
        public bool IsContentLoaded => _lazyContent?.IsValueCreated ?? false;

        /// <summary> Prüft, ob Metadaten geladen wurden</summary>
        public bool IsMetadataLoaded => Size > 0 || LastWriteTimeUtc != default;

        /// <summary>Letztes Änderungsdatum (UTC, gecacht)</summary>
        public DateTime LastWriteTimeUtc { get; private set; }

        /// <summary>Dateiname ohne Pfad</summary>
        public string Name => System.IO.Path.GetFileName(Filename);

        /// <summary>Dateigröße in Bytes (gecacht)</summary>
        public long Size { get; private set; }

        /// <summary>Aktuelle Version (erhöht sich bei jedem Reload)</summary>
        public int Version => _version;

        #endregion

        #region Methods

        public void Dispose() {
            if (_isDisposed) return;

            _lock.EnterWriteLock();
            try {
                if (_isDisposed) return;

                if (_lazyContent?.IsValueCreated == true) {
                    Array.Clear(_lazyContent.Value, 0, _lazyContent.Value.Length);
                }

                _lazyContent = null;
                _isDisposed = true;
            } finally {
                _lock.ExitWriteLock();
                _lock.Dispose();  // Innerhalb finally
            }
        }

        public string GetContentAsString(Encoding? encoding = null) {
            ThrowIfDisposed();
            encoding ??= Encoding.UTF8;

            // KEIN Lock - Content ist bereits thread-sicher durch Lazy<T>
            return encoding.GetString(Content);
        }

        // Invalidate – öffentlich, mit sauberem Lock-Handlinig
        public void Invalidate() {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try {
                // Falls Content schon erzeugt wurde, Speicher räumen
                if (_lazyContent?.IsValueCreated == true) {
                    Array.Clear(_lazyContent.Value, 0, _lazyContent.Value.Length);
                }

                _lazyContent = null;
                InitializeLazyLoaders(); // Neu initialisieren
            } finally {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Prüft, ob die Datei im Dateisystem geändert wurde.
        /// </summary>
        public bool IsStale() {
            ThrowIfDisposed();

            try {
                var fileInfo = new FileInfo(Filename);
                return !fileInfo.Exists || fileInfo.LastWriteTimeUtc != _cachedLastWriteTimeUtc;
            } catch {
                return true; // Bei Fehler als "stale" markieren
            }
        }

        // Refresh – alle Locks via finally abgesichert
        public void Refresh() {
            ThrowIfDisposed();

            try {
                var fileInfo = new FileInfo(Filename);

                if (!fileInfo.Exists) {
                    throw new FileNotFoundException($"Datei nicht vorhanden: {Filename}", Filename);
                }

                Size = fileInfo.Length;
                LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                CreationTimeUtc = fileInfo.CreationTimeUtc;
                _cachedLastWriteTimeUtc = LastWriteTimeUtc;

                InvalidateInternal();
            } catch { }
        }

        public override string ToString() => $"CachedFile {{ Path = {Filename}, Size = {Size:N0} bytes, Version = {Version}, Loaded = {IsContentLoaded} }}";

        // UpdatePath – Lock via finally
        internal void UpdatePath(string newPath) {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try {
                _filename = newPath ?? throw new ArgumentNullException(nameof(newPath));
                Interlocked.Increment(ref _version);
            } finally {
                _lock.ExitWriteLock();
            }
        }

        private void Initialize() {
            var fileInfo = new FileInfo(Filename);

            if (!fileInfo.Exists) {
                throw new FileNotFoundException($"Datei nicht gefunden: {Filename}", Filename);
            }

            Size = fileInfo.Length;
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            CreationTimeUtc = fileInfo.CreationTimeUtc;
            CachedAtUtc = DateTime.UtcNow;
            _cachedLastWriteTimeUtc = LastWriteTimeUtc;

            InitializeLazyLoaders();
        }

        private void InitializeLazyLoaders() => _lazyContent = new Lazy<byte[]>(
                LoadContentFromFileSystem,
                LazyThreadSafetyMode.ExecutionAndPublication);

        // Interner Invalidate-Pfad mit finally
        private void InvalidateInternal() {
            _lock.EnterWriteLock();
            try {
                if (_lazyContent?.IsValueCreated == true) {
                    Array.Clear(_lazyContent.Value, 0, _lazyContent.Value.Length);
                }
                _lazyContent = null;
            } finally {
                _lock.ExitWriteLock();
            }
        }

        // Hilfsfunktion: liest Bytes vom FS ohne Locks zu hinterlassen
        private byte[] LoadContentFromFileSystem() {
            try {
                return File.ReadAllBytes(Filename);
            } catch (Exception ex) {
                throw new IOException($"Fehler beim Laden der Datei: {Filename}", ex);
            }
        }

        private void ThrowIfDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(CachedFile));
            }
        }

        #endregion
    }
}