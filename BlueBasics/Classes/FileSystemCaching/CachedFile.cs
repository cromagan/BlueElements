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
using System.Threading;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Abstrakte Basisklasse für alle gecachten Dateitypen.
/// Verwaltet das Laden roher Bytes vom Dateisystem mit Lazy-Loading und Versionierung.
/// Thread-sicher durch Double-Checked-Locking.
/// Instanzen dürfen nur über CachedFileSystem erzeugt werden.
/// </summary>
public abstract class CachedFile : IDisposable {

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
    /// Gepufferte Bytes der Datei (rohe Bytes, wie auf der Platte).
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
    /// Erstellt eine neue CachedFile-Instanz für den angegebenen Dateipfad.
    /// Nur über CachedFileSystem.CreateCachedFile() (via Activator.CreateInstance) aufrufbar.
    /// </summary>
    /// <param name="filename">Vollständiger Dateipfad.</param>
    protected CachedFile(string filename) {
        Filename = string.IsNullOrEmpty(filename) ? string.Empty : filename.NormalizeFile();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Der vollständige Dateipfad dieser gecachten Datei.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Gibt an, ob die abgeleitete Klasse die Rohdaten bereits verarbeitet hat.
    /// Wird automatisch auf false gesetzt, wenn die Datei veraltet ist (Invalidate).
    /// Ableitungen setzen dies auf true, nachdem sie ihre Daten erfolgreich verarbeitet haben.
    /// </summary>
    public bool IsParsed { get; protected set; }

    /// <summary>
    /// Gibt an, ob das Laden der Datei fehlgeschlagen ist.
    /// </summary>
    public bool LoadFailed { get; protected set; }

    #endregion

    #region Methods

    /// <summary>
    /// Disposed alle zugeordneten Ressourcen und beendet den Timer.
    /// </summary>
    public virtual void Dispose() {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 1) { return; }
        Invalidate();
    }

    /// <summary>
    /// Invalidiert den gecachten Inhalt, damit er beim nächsten Zugriff neu geladen wird.
    /// Setzt IsParsed auf false — die Ableitung muss ihre Daten neu verarbeiten.
    /// </summary>
    public virtual void Invalidate() {
        lock (_lock) {
            StopStaleCheckTimer();
            _timestamp = null;
            _content = null;
            IsParsed = false;
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
    public override string ToString() => $"{GetType().Name}: {Filename}";

    /// <summary>
    /// Lädt den Dateiinhalt (rohe Bytes) aus dem Cache oder dem Dateisystem.
    /// </summary>
    protected byte[] GetContent() {
        if (_isDisposed > 0) { return []; }

        lock (_lock) {
            if (_timestamp != null && _content != null) { return _content; }
        }

        (var content, var timestamp, var loadFailed) = ReadContentFromFileSystem();

        lock (_lock) {
            if (_timestamp != null && _content != null) { return _content; }

            LoadFailed = loadFailed;
            _timestamp = timestamp;
            _content = content;

            StartStaleCheckTimer();
            return _content;
        }
    }

    /// <summary>
    /// Lädt den Dateiinhalt und entpackt ihn, falls er gezippt ist.
    /// </summary>
    protected byte[] GetUnzippedContent() {
        var content = GetContent();
        if (content.Length == 0) { return content; }

        if (content.IsZipped()) {
            var unzipped = content.UnzipIt();
            if (unzipped == null) {
                LoadFailed = true;
                return [];
            }
            return unzipped;
        }

        return content;
    }

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
    private (byte[] Content, string Timestamp, bool LoadFailed) ReadContentFromFileSystem() {
        try {
            do {
                var fileInfo1 = GetFileState(Filename, false, 0.1f);
                if (string.IsNullOrEmpty(fileInfo1)) { return ([], string.Empty, false); }

                var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
                var fileInfo2 = GetFileState(Filename, false, 2f);
                if (fileInfo1 == fileInfo2) { return (content, fileInfo2, false); }
            } while (true);
        } catch {
            return ([], string.Empty, true);
        }
    }

    /// <summary>
    /// Startet den Timer für die automatische Stale-Prüfung.
    /// </summary>
    private void StartStaleCheckTimer() {
        lock (_lock) {
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

        timerToDispose?.Dispose();
    }

    #endregion
}