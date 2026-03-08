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

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Abstrakte Basisklasse für alle gecachten Dateitypen.
/// Verwaltet das Laden roher Bytes vom Dateisystem mit Lazy-Loading und Versionierung.
/// Thread-sicher durch Double-Checked-Locking mit Semaphore.
/// Instanzen dürfen nur über CachedFileSystem erzeugt werden.
/// </summary>
public abstract class CachedFile : IDisposable, IHasKeyName, IReadableText {

    #region Fields

    /// <summary>
    /// Synchronisierungsobjekt für Thread-sichere Zugriffe auf Dateiinhalte.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Semaphore zum Synchronisieren von Ladevorgängen.
    /// Beim Laden von Disk erworben → IsLoading liefert true.
    /// </summary>
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    /// <summary>
    /// Gepufferte Bytes der Datei (logischer Inhalt, bei gezippten Dateien bereits entpackt).
    /// </summary>
    private byte[]? _content;

    /// <summary>
    /// Gibt an, ob der aktuelle Content mit dem Dateiinhalt auf der Platte übereinstimmt.
    /// Wird beim Setzen von Content auf false gesetzt, beim Speichern auf true.
    /// </summary>
    private bool _isSaved = true;

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

    #region Events

    /// <summary>
    /// Ereignis, das beim Laden der Datei ausgelöst wird.
    /// </summary>
    public event EventHandler? Loaded;

    #endregion

    #region Properties

    public bool IsLoading {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

            if (!_loadSemaphore.Wait(0)) { return true; }
            _loadSemaphore.Release();
            return false;
        }
    }

    public bool IsSaving {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

            if (!_saveSemaphore.Wait(0)) { return true; }
            _saveSemaphore.Release();
            return false;
        }
    }

    /// <summary>
    /// Gibt den logischen Dateiinhalt zurück (bei gezippten Dateien automatisch entpackt).
    /// Beim Setzen wird _isSaved auf false gesetzt.
    /// Ersetzt die alten Methoden GetContent() und GetUnzippedContent().
    /// Erwirbt _loadSemaphore während des Ladevorgangs, sodass IsLoading korrekt true liefert.
    /// </summary>
    protected byte[] Content {
        get {
            if (IsDisposed) { return []; }

            // Schnellpfad: Inhalt bereits gecacht
            lock (_lock) {
                if (_timestamp != null && _content != null) { return _content; }
            }

            // Ladepfad: Semaphore erwerben, damit IsLoading = true
            _loadSemaphore.Wait();
            try {
                // Nach Semaphore-Erwerb nochmals prüfen (ein anderer Thread könnte geladen haben)
                lock (_lock) {
                    if (_timestamp != null && _content != null) { return _content; }
                }

                var (content, timestamp, loadFailed) = ReadContentFromFileSystem();

                lock (_lock) {
                    // Dritte Prüfung im Lock – Race Condition absichern
                    if (_timestamp != null && _content != null) { return _content; }

                    LoadFailed = loadFailed;
                    _timestamp = timestamp;

                    // Automatisch entpacken, falls gezippt
                    if (content.Length > 0 && content.IsZipped()) {
                        var unzipped = content.UnzipIt();
                        if (unzipped == null) {
                            LoadFailed = true;
                            _content = [];
                        } else {
                            _content = unzipped;
                        }
                    } else {
                        _content = content;
                    }

                    return _content;
                }
            } finally {
                _loadSemaphore.Release();
            }
        }
        set {
            lock (_lock) {
                _content = value;
                _isSaved = false;
            }
        }
    }

    /// <summary>
    /// Der vollständige Dateipfad dieser gecachten Datei.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgängig gemacht werden.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    /// <summary>
    /// Flag zur Überwachung, ob die Instanz disposed wurde.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gibt an, ob die Datei eingefroren ist.
    /// </summary>
    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    /// <summary>
    /// Gibt an, ob die abgeleitete Klasse die Rohdaten bereits verarbeitet hat.
    /// Wird automatisch auf false gesetzt, wenn die Datei veraltet ist (Invalidate).
    /// Ableitungen setzen dies auf true, nachdem sie ihre Daten erfolgreich verarbeitet haben.
    /// </summary>
    public bool IsParsed { get; protected set; }

    /// <summary>
    /// Gibt an, ob der aktuelle Inhalt gespeichert ist.
    /// </summary>
    public bool IsSaved {
        get { lock (_lock) { return _isSaved; } }
    }

    /// <summary>
    /// IHasKeyName: Entspricht dem Dateinamen.
    /// </summary>
    public virtual string KeyName => Filename;

    /// <summary>
    /// IHasKeyName: Keys sind case-insensitive.
    /// </summary>
    public virtual bool KeyIsCaseSensitive => false;

    /// <summary>
    /// Gibt an, ob das Laden der Datei fehlgeschlagen ist.
    /// </summary>
    public bool LoadFailed { get; protected set; }

    /// <summary>
    /// Gibt an, ob der Inhalt beim Speichern automatisch gezippt werden soll.
    /// Beim Laden wird unabhängig davon geprüft, ob die Datei gezippt ist.
    /// </summary>
    public abstract bool MustZipped { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt die zu speichernden Bytes zurück. Standard: aktuell gecachter Content.
    /// Abgeleitete Klassen überschreiben dies, um ihre serialisierten Daten zu liefern.
    /// </summary>
    protected virtual byte[] GetContent() {
        lock (_lock) {
            return _content ?? [];
        }
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Standard: nicht eingefroren und nicht disposed.
    /// </summary>
    public virtual bool IsSaveAbleNow() => !IsFreezed && !IsDisposed;

    /// <summary>
    /// Markiert den Inhalt als ungespeichert.
    /// Für Unterklassen, die _isSaved nicht direkt setzen können.
    /// </summary>
    protected void MarkDirty() {
        lock (_lock) { _isSaved = false; }
    }

    /// <summary>
    /// Disposed alle zugeordneten Ressourcen.
    /// </summary>
    public virtual void Dispose() {
        if (IsDisposed) { return; }
        IsDisposed = true;
        Invalidate();
        _loadSemaphore.Dispose();
        _saveSemaphore.Dispose();
    }

    /// <summary>
    /// Speichert den Inhalt asynchron mit Backup-Rotation und Semaphore-Synchronisierung.
    /// Nutzt GetContent() und IsSaveAbleNow().
    /// Nicht überschreibbar — nutze OnSaved() für Aktionen nach erfolgreichem Speichern.
    /// </summary>
    /// <returns>Leerer String bei Erfolg, Fehlermeldung bei Fehler.</returns>
    public async Task<string> DoExtendedSave() {
        if (!_saveSemaphore.Wait(0)) { return "Anderer Speichervorgang läuft"; }

        try {
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Benutzer-Aktion abwarten"; }

            if (!IsSaveAbleNow()) { return "Nicht speicherbar (IsSaveAbleNow = false)"; }

            Develop.Message(ErrorType.DevelopInfo, this, Filename.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {ReadableText()}", 0);

            return await Task.Run(() => {
                try {
                    var dataUncompressed = GetContent();
                    if (dataUncompressed.Length == 0) { return "Keine Daten zum Speichern"; }

                    var dataToWrite = MustZipped ? dataUncompressed.ZipIt() ?? [] : dataUncompressed;
                    if (dataToWrite.Length == 0) { return "Komprimierung fehlgeschlagen"; }

                    var backup = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";
                    var tempfile = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

                    if (!WriteAllBytes(tempfile, dataToWrite)) {
                        DeleteFile(tempfile, false);
                        return "Speichern fehlgeschlagen";
                    }

                    var tempFileInfo = GetFileState(tempfile, true, 60);
                    if (string.IsNullOrEmpty(tempFileInfo)) {
                        DeleteFile(tempfile, false);
                        return "Dateiinfo konnte nicht gelesen werden";
                    }

                    if (FileExists(backup) && !DeleteFile(backup, false)) {
                        DeleteFile(tempfile, false);
                        return "Backup konnte nicht gelöscht werden";
                    }

                    if (FileExists(Filename) && !MoveFile(Filename, backup, false)) {
                        DeleteFile(tempfile, false);
                        return "Hauptdatei konnte nicht verschoben werden";
                    }

                    if (!MoveFile(tempfile, Filename, false)) {
                        // Rollback
                        if (FileExists(backup) && !FileExists(Filename)) {
                            MoveFile(backup, Filename, false);
                        }
                        DeleteFile(tempfile, false);
                        return "Speichervorgang fehlgeschlagen";
                    }

                    lock (_lock) {
                        _isSaved = true;
                    }

                    OnSaved();
                    Invalidate();
                    return string.Empty;
                } catch (Exception ex) {
                    return ex.Message;
                }
            }).ConfigureAwait(false);
        } finally {
            _saveSemaphore.Release();
        }
    }

    /// <summary>
    /// Friert die Datei ein. Kann nicht rückgängig gemacht werden.
    /// </summary>
    public void Freeze(string reason) {
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    /// <summary>
    /// Invalidiert den gecachten Inhalt, damit er beim nächsten Zugriff neu geladen wird.
    /// Setzt IsParsed auf false — die Ableitung muss ihre Daten neu verarbeiten.
    /// Setzt auch _isSaved auf true (lokale Änderungen gelten als verworfen).
    /// </summary>
    public virtual void Invalidate() {
        lock (_lock) {
            _timestamp = null;
            _content = null;
            IsParsed = false;
            _isSaved = true;
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
    /// Menschenlesbarer Name dieser Datei für Statusmeldungen (z. B. "Speichere ...").
    /// Muss von konkreten Ableitungen implementiert werden.
    /// </summary>
    public abstract string ReadableText();

    /// <summary>
    /// Optionales Symbol für UI-Darstellungen. Standard: null.
    /// </summary>
    public virtual QuickImage? SymbolForReadableText() => null;

    /// <summary>
    /// Speichert den aktuellen Content synchron auf die Platte.
    /// Nutzt GetContent() und IsSaveAbleNow().
    /// </summary>
    public void Save() {
        if (!IsSaveAbleNow()) { return; }

        byte[]? dataToWrite;
        lock (_lock) {
            var data = GetContent();
            if (data.Length == 0) { return; }
            dataToWrite = MustZipped ? data.ZipIt() : data;
        }

        if (dataToWrite == null || dataToWrite.Length == 0) { return; }

        WriteAllBytes(Filename, dataToWrite);
        var newTimestamp = GetFileState(Filename, false, 0.1f);

        lock (_lock) {
            _isSaved = true;
            _timestamp = newTimestamp;
        }
    }

    /// <summary>
    /// Speichert den Inhalt unter einem neuen Dateinamen (synchron).
    /// Nutzt GetContent() für die Bytes.
    /// </summary>
    /// <returns>True wenn erfolgreich.</returns>
    public bool SaveAs(string filename) {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(filename)) { return false; }
        if (!IsSaveAbleNow()) { return false; }

        var data = GetContent();
        if (data.Length == 0) { return false; }

        var dataToWrite = MustZipped ? data.ZipIt() : data;
        if (dataToWrite == null || dataToWrite.Length == 0) { return false; }

        var backup = filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";
        var tmpFile = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        if (!WriteAllBytes(tmpFile, dataToWrite)) {
            DeleteFile(tmpFile, false);
            return false;
        }

        if (FileExists(backup)) { DeleteFile(backup, false); }
        if (FileExists(filename)) { MoveFile(filename, backup, false); }

        if (!MoveFile(tmpFile, filename, false)) {
            if (FileExists(backup) && !FileExists(filename)) { MoveFile(backup, filename, false); }
            DeleteFile(tmpFile, false);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ruft das Loaded-Ereignis auf.
    /// Kann von Ableitungen überschrieben werden, um auf Ladeabschluss zu reagieren.
    /// </summary>
    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Wird nach erfolgreichem Speichern aufgerufen (nachdem _isSaved = true gesetzt wurde).
    /// Ableitungen können hier interne Zustände nach dem Speichern aktualisieren.
    /// </summary>
    protected virtual void OnSaved() { }

    /// <summary>
    /// Gibt eine Stringdarstellung der gecachten Datei zurück.
    /// </summary>
    public override string ToString() => $"{GetType().Name}: {Filename}";

    /// <summary>
    /// Liest die Datei vom Dateisystem mit wiederholten Checks zur Konsistenzprüfung.
    /// </summary>
    private (byte[] Content, string Timestamp, bool LoadFailed) ReadContentFromFileSystem() {
        try {
            var retries = 0;
            do {
                var fileInfo1 = GetFileState(Filename, false, 0.1f);
                if (string.IsNullOrEmpty(fileInfo1)) { return ([], string.Empty, false); }

                var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
                var fileInfo2 = GetFileState(Filename, false, 2f);
                if (fileInfo1 == fileInfo2) { return (content, fileInfo2, false); }

                retries++;
            } while (retries < 20);

            return ([], string.Empty, true); // Datei ändert sich ständig, Laden fehlgeschlagen
        } catch {
            return ([], string.Empty, true);
        }
    }

    #endregion
}
