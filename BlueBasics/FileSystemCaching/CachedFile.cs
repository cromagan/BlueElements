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

        public readonly string Filename;
        private int _isDisposed = 0;
        private Lazy<(byte[] Content, DateTime Timestamp)>? _lazyData;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine neue CachedFile-Instanz mit Lazy-Loading für Metadaten und Content.
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        public CachedFile(string filename) {
            Filename = filename;
            InitializeLazyLoaders();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gibt den Zeitstempel des geladenen Contents zurück (falls geladen).
        /// </summary>
        public DateTime? CachedTimestamp {
            get {
                if (_lazyData?.IsValueCreated == true) {
                    return _lazyData.Value.Timestamp;
                }
                return null;
            }
        }

        public byte[] Content {
            get {
                ThrowIfDisposed();

                // Lazy<T> ist bereits thread-sicher!
                if (_lazyData == null) {
                    throw new InvalidOperationException("Content not initialized");
                }

                return _lazyData.Value.Content;
            }
        }

        #endregion

        #region Methods

        public void Dispose() {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1) return;
            try {
                if (_lazyData?.IsValueCreated == true) {
                    Array.Clear(_lazyData.Value.Content, 0, _lazyData.Value.Content.Length);
                }

                _lazyData = null;
            } catch { }
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

            try {
                // Falls Content schon erzeugt wurde, Speicher räumen
                if (_lazyData?.IsValueCreated == true) {
                    Array.Clear(_lazyData.Value.Content, 0, _lazyData.Value.Content.Length);
                }

                // Neu initialisieren (INNERHALB des Locks, kein rekursiver Aufruf)
                _lazyData = new Lazy<(byte[], DateTime)>(
                    LoadContentFromFileSystem,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            } catch { }
        }

        /// <summary>
        /// Prüft, ob die Datei im Dateisystem geändert wurde.
        /// </summary>
        public bool IsStale() {
            ThrowIfDisposed();

            var cachedTimestamp = CachedTimestamp;
            if (cachedTimestamp == null) { return true; }

            try {
                var fileInfo = new FileInfo(Filename);
                return !fileInfo.Exists || fileInfo.LastWriteTimeUtc != cachedTimestamp;
            } catch {
                return true; // Bei Fehler als "stale" markieren
            }
        }

        // Refresh – alle Locks via finally
        public void Refresh() {
            ThrowIfDisposed();

            var fileInfo = new FileInfo(Filename);

            if (!fileInfo.Exists) {
                throw new FileNotFoundException($"Datei nicht vorhanden: {Filename}", Filename);
            }

            try {
                // Content invalidieren (OHNE Lock, da wir bereits WriteLock haben)
                if (_lazyData?.IsValueCreated == true) {
                    Array.Clear(_lazyData.Value.Content, 0, _lazyData.Value.Content.Length);
                }
                _lazyData = new Lazy<(byte[], DateTime)>(
                    LoadContentFromFileSystem,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            } catch { }
        }

        public override string ToString() => $"CachedFile {Filename}";

        private void InitializeLazyLoaders() => _lazyData = new Lazy<(byte[], DateTime)>(
            LoadContentFromFileSystem,
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Hilfsfunktion: liest Bytes vom FS ohne Locks zu hinterlassen
        // Lazy<T> kümmert sich um Thread-Safety
        private (byte[], DateTime) LoadContentFromFileSystem() {
            do {
                sss

                  var fileInfo1 = new FileInfo(Filename);
                if (!fileInfo1.Exists) { return ([], DateTime.MinValue); }

                var content = File.ReadAllBytes(Filename);

                var fileInfo2 = new FileInfo(Filename);
                if (!fileInfo2.Exists) { return ([], DateTime.MinValue); }

                if (fileInfo1.LastWriteTimeUtc == fileInfo2.LastWriteTimeUtc) {
                    return (content, fileInfo2.LastWriteTimeUtc);
                }
            } while (true);
        }

        private void ThrowIfDisposed() {
            if (_isDisposed > 0) {
                throw new ObjectDisposedException(nameof(CachedFile));
            }
        }

        #endregion
    }
}