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

using System;
using System.Text;
using System.Threading;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Repräsentiert eine gecachte Datei mit Lazy-Loading-Unterstützung und Versionierung.
/// Thread-sicher durch Verwendung von Double-Checked-Locking.
/// Metadaten und GetContent werden erst bei Bedarf geladen.
/// </summary>
public sealed class CachedFile : IDisposable {

    #region Fields

    /// <summary>
    /// Intervall in Millisekunden für die automatische Überprüfung auf veraltete Dateien.
    /// </summary>
    private readonly int _checkIntervalMs = 180000;

    /// <summary>
    /// Synchronisierungsobjekt für Thread-sichere Zugriffe auf Dateiinhalte.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Gepufferte Bytes der Datei.
    /// </summary>
    private byte[]? _content;

    /// <summary>
    /// Flag zur Überwachung, ob die Instanz disposed wurde.
    /// </summary>
    private int _isDisposed;

    /// <summary>
    /// Timer für die periodische Überprüfung veralteter Dateien.
    /// </summary>
    private Timer? _staleCheckTimer;

    /// <summary>
    /// Zeitstempel der letzten Dateiversion im Cache.
    /// </summary>
    private string? _timestamp;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine neue CachedFile-Instanz für Metadaten und GetContent.
    /// </summary>
    /// <param name="filename">Dateipfad</param>
    public CachedFile(string filename) => Filename = filename;

    #endregion

    #region Properties

    /// <summary>
    /// Der vollständige Dateipfad dieser gecachten Datei.
    /// </summary>
    public string Filename { get; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Disposed alle zugeordneten Ressourcen und beendet den Timer.
    /// </summary>
    public void Dispose() {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 1) { return; }
        Invalidate();
    }

    /// <summary>
    /// Lädt den Dateiinhalt aus dem Cache oder dem Dateisystem und gibt ihn als Byte-Array zurück.
    /// </summary>
    /// <returns>Der Inhalt der Datei als Byte-Array, oder leeres Array wenn disposed.</returns>
    public byte[] GetContent() {
        if (_isDisposed > 0) { return []; }

        lock (_lock) {
            if (_timestamp != null && _content != null) { return _content; }
        }

        (var content, var timestamp) = ReadContentFromFileSystem();

        lock (_lock) {
            if (_timestamp != null && _content != null) { return _content; }

            _timestamp = timestamp;
            _content = content;

            StartStaleCheckTimer();
            return _content;
        }
    }

    /// <summary>
    /// Lädt den Dateiinhalt als String mit optionalem Encoding.
    /// </summary>
    /// <param name="encoding">Das zu verwendende Encoding. Standard ist UTF8.</param>
    /// <returns>Der Dateiinhalt als String, oder leerer String wenn disposed.</returns>
    public string GetContentAsString(Encoding? encoding = null) {
        if (_isDisposed > 0) { return string.Empty; }

        var content = GetContent(); // Lokale Kopie
        if (content.Length == 0) { return string.Empty; }

        encoding ??= Encoding.UTF8;
        return encoding.GetString(content);
    }

    /// <summary>
    /// Invalidiert den gecachten Inhalt und setzt ihn zurück, damit er neu geladen wird.
    /// </summary>
    public void Invalidate() {
        lock (_lock) {
            StopStaleCheckTimer();
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

    /// <summary>
    /// Gibt eine Stringdarstellung der gecachten Datei zurück.
    /// </summary>
    /// <returns>String im Format "CachedFile [Filename]"</returns>
    public override string ToString() => $"CachedFile {Filename}";

    /// <summary>
    /// Callback-Methode für den Timer: Prüft ob Datei veraltet ist und invalidiert sie.
    /// </summary>
    private void CheckAndInvalidateIfStale() {
        if (_isDisposed > 0) { return; }

        if (IsStale()) {
            Invalidate();
        }
    }

    /// <summary>
    /// Liest die Datei vom Dateisystem mit wiederholten Checks zur Konsistenzprüfung.
    /// </summary>
    /// <returns>Tuple mit Dateiinhalt und aktuellem Zeitstempel.</returns>
    private (byte[] Content, string Timestamp) ReadContentFromFileSystem() {
        do {
            var fileInfo1 = GetFileState(Filename, false, 0.1f);
            var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
            var fileInfo2 = GetFileState(Filename, false, 2f);
            if (fileInfo1 == fileInfo2) { return (content, fileInfo2); }
        } while (true);
    }

    /// <summary>
    /// Startet den Timer für die automatische Stale-Prüfung.
    /// </summary>
    private void StartStaleCheckTimer() {
        lock (_lock) {
            // Timer nur starten, wenn noch keiner läuft
            _staleCheckTimer ??= new Timer(
                    callback: _ => CheckAndInvalidateIfStale(),
                    state: null,
                    dueTime: _checkIntervalMs,
                    period: _checkIntervalMs
                );
        }
    }

    /// <summary>
    /// Stoppt und disposed den Timer.
    /// </summary>
    private void StopStaleCheckTimer() {
        Timer? timerToDispose = null;

        lock (_lock) {
            timerToDispose = _staleCheckTimer;
            _staleCheckTimer = null;
        }

        // Dispose außerhalb des Locks
        timerToDispose?.Dispose();
    }

    #endregion
}