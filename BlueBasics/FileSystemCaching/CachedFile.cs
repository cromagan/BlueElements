using System;
using System.IO;
using System.Text;
using System.Threading;

#nullable enable

namespace FileSystemCaching {

    /// <summary>
    /// Repräsentiert eine gecachte Datei mit Lazy-Loading-Unterstützung und Versionierung.
    /// Thread-sicher durch Verwendung von ReaderWriterLockSlim.
    /// Metadaten und Content werden erst bei Bedarf geladen.
    /// </summary>
    public sealed class CachedFile : IDisposable {

        #region Fields

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private DateTime? _cachedLastWriteTimeUtc;
        private string _filename;
        private volatile bool _isDisposed;
        private Lazy<byte[]>? _lazyContent;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine neue CachedFile-Instanz mit Lazy-Loading für Metadaten und Content.
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        public CachedFile(string filename) {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(_filename)) {
                throw new FileNotFoundException($"Datei nicht gefunden: {_filename}", _filename);
            }

            InitializeLazyLoaders();
        }

        #endregion

        #region Properties

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

        // Invalidate – öffentlich, mit sauberem Lock-Handling
        public void Invalidate() {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try {
                // Falls Content schon erzeugt wurde, Speicher räumen
                if (_lazyContent?.IsValueCreated == true) {
                    Array.Clear(_lazyContent.Value, 0, _lazyContent.Value.Length);
                }

                // Metadaten als ungültig markieren
                _cachedLastWriteTimeUtc = null;

                // Neu initialisieren (INNERHALB des Locks, kein rekursiver Aufruf)
                _lazyContent = new Lazy<byte[]>(
                    LoadContentFromFileSystem,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            } finally {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Prüft, ob die Datei im Dateisystem geändert wurde.
        /// </summary>
        public bool IsStale() {
            ThrowIfDisposed();
            if (_cachedLastWriteTimeUtc == null) { return true; }

            try {
                var fileInfo = new FileInfo(Filename);
                return !fileInfo.Exists || fileInfo.LastWriteTimeUtc != _cachedLastWriteTimeUtc;
            } catch {
                return true; // Bei Fehler als "stale" markieren
            }
        }

        // Refresh – alle Locks via finally
        public void Refresh() {
            ThrowIfDisposed();

            try {
                var fileInfo = new FileInfo(Filename);

                if (!fileInfo.Exists) {
                    throw new FileNotFoundException($"Datei nicht vorhanden: {Filename}", Filename);
                }

                _lock.EnterWriteLock();
                try {
                    _cachedLastWriteTimeUtc = fileInfo.LastWriteTimeUtc;

                    // Content invalidieren (OHNE Lock, da wir bereits WriteLock haben)
                    if (_lazyContent?.IsValueCreated == true) {
                        Array.Clear(_lazyContent.Value, 0, _lazyContent.Value.Length);
                    }
                    _lazyContent = new Lazy<byte[]>(
                        LoadContentFromFileSystem,
                        LazyThreadSafetyMode.ExecutionAndPublication);
                } finally {
                    _lock.ExitWriteLock();
                }
            } catch { }
        }

        public override string ToString() => $"CachedFile {Filename}, Loaded = {IsContentLoaded}";

        // UpdatePath – Lock via finally
        internal void UpdatePath(string newPath) {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try {
                _filename = newPath ?? throw new ArgumentNullException(nameof(newPath));
            } finally {
                _lock.ExitWriteLock();
            }
        }

        private void InitializeLazyLoaders() => _lazyContent = new Lazy<byte[]>(
            LoadContentFromFileSystem,
            LazyThreadSafetyMode.ExecutionAndPublication);

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