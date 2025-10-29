using System;
using System.Text;
using System.Threading;
using static BlueBasics.IO;

#nullable enable

namespace BlueBasics.FileSystemCaching {

    /// <summary>
    /// Repräsentiert eine gecachte Datei mit Lazy-Loading-Unterstützung und Versionierung.
    /// Thread-sicher durch Verwendung von Double-Checked-Locking.
    /// Metadaten und Content werden erst bei Bedarf geladen.
    /// </summary>
    public sealed class CachedFile : IDisposable {

        #region Fields

        public readonly string Filename;
        private readonly object _lock = new object();
        private byte[]? _content;
        private int _isDisposed = 0;
        private string? _timestamp;

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt eine neue CachedFile-Instanz für Metadaten und Content.
        /// </summary>
        /// <param name="filename">Dateipfad</param>
        public CachedFile(string filename) {
            Filename = filename;
        }

        #endregion

        #region Properties

        public byte[] Content {
            get {
                if (_isDisposed > 0) { return []; }

                lock (_lock) {
                    if (_timestamp != null && _content != null) { return _content; }
                }

                (var content, var timestamp) = ReadContentFromFileSystem();

                lock (_lock) {
                    if (_timestamp != null && _content != null) { return _content; }

                    _timestamp = timestamp;
                    _content = content;

                    return _content;
                }
            }
        }

        #endregion

        #region Methods

        public void Dispose() {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1) { return; }
            Invalidate();
        }

        public string GetContentAsString(Encoding? encoding = null) {
            if (_isDisposed > 0) { return string.Empty; }
            encoding ??= Encoding.UTF8;
            return encoding.GetString(Content);
        }

        // Invalidate – öffentlich, mit sauberem Lock-Handling
        public void Invalidate() {
            lock (_lock) {
                _timestamp = null;
                _content = null;
            }
        }

        /// <summary>
        /// Prüft, ob die Datei im Dateisystem geändert wurde.
        /// </summary>
        public bool IsStale() {
            lock (_lock) {
                if (_timestamp == null) { return true; }
            }

            var newTimeStamp = GetFileState(Filename, false, 0.1f);

            lock (_lock) {
                return newTimeStamp != _timestamp;
            }
        }

        public override string ToString() => $"CachedFile {Filename}";

        // Hilfsfunktion: liest Bytes vom FS ohne Locks zu hinterlassen
        // Double-Checked-Locking kümmert sich um Thread-Safety
        private (byte[] Content, string Timestamp) ReadContentFromFileSystem() {
            do {
                var fileInfo1 = GetFileState(Filename, false, 0.1f);
                var content = ReadAllBytes(Filename, 20);
                var fileInfo2 = GetFileState(Filename, false, 2f);
                if (fileInfo1 == fileInfo2) { return (content, fileInfo2); }
            } while (true);
        }

        #endregion
    }
}