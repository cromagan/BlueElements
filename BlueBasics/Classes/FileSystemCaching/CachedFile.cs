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
using System.Collections.Generic;
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
    /// Schreib-/Build-Puffer für abgeleitete Klassen (z. B. Chunk im Schreib-Modus).
    /// Wenn gesetzt, liefert DataLength die Anzahl der gepufferten Bytes.
    /// </summary>
    protected byte[]? _contentToSave;

    /// <summary>
    /// Semaphore zum Synchronisieren von Ladevorgängen.
    /// Beim Laden von Disk erworben → IsLoading liefert true.
    /// </summary>
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    /// <summary>
    /// Synchronisierungsobjekt für Thread-sichere Zugriffe auf Dateiinhalte.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    /// <summary>
    /// Gepufferte Bytes der Datei (logischer Inhalt, bei gezippten Dateien bereits entpackt).
    /// </summary>
    private byte[]? _contentLoaded;

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

    /// <summary>
    /// Gibt den logischen Dateiinhalt zurück (bei gezippten Dateien automatisch entpackt).
    /// Beim Setzen wird _isSaved auf false gesetzt.
    /// Erwirbt _loadSemaphore während des Ladevorgangs, sodass IsLoading korrekt true liefert.
    /// Nach einem Frisch-Ladevorgang wird OnLoaded() automatisch aufgerufen.
    /// </summary>
    public byte[] Content {
        get {
            if (IsDisposed) { return []; }

            // Schnellpfad: Inhalt bereits gecacht
            lock (_lock) {
                if (_timestamp != null && _contentToSave != null) { return _contentToSave; }
            }

            // Ladepfad: Semaphore erwerben, damit IsLoading = true
            _loadSemaphore.Wait();
            try {
                // Nach Semaphore-Erwerb nochmals prüfen (ein anderer Thread könnte geladen haben)
                lock (_lock) {
                    if (_timestamp != null && _contentToSave != null) { return _contentToSave; }
                }

                var (content, timestamp, loadFailed) = ReadContentFromFileSystem();

                byte[] result;
                lock (_lock) {
                    // Dritte Prüfung im Lock – Race Condition absichern
                    if (_timestamp != null && _contentToSave != null) { return _contentToSave; }

                    LoadFailed = loadFailed;
                    _timestamp = timestamp;

                    // Automatisch entpacken, falls gezippt
                    if (content.Length > 0 && content.IsZipped()) {
                        var unzipped = content.UnzipIt();
                        if (unzipped == null) {
                            LoadFailed = true;
                            _contentToSave = [];
                            _contentLoaded = [];
                        } else {
                            _contentToSave = unzipped;
                            _contentLoaded = unzipped;
                        }
                    } else {
                        _contentToSave = content;
                        _contentLoaded = content;
                    }

                    // MinimumBytes-Prüfung: Inhalt zu klein → Ladung als fehlgeschlagen markieren
                    if (MinimumBytes > 0 && !LoadFailed && _contentToSave.Length < MinimumBytes) {
                        LoadFailed = true;
                        _contentToSave = [];
                        _contentLoaded = [];
                    }

                    result = _contentToSave;
                }

                // OnLoaded nach dem Frisch-Laden aufrufen (außerhalb des Locks, aber noch im Semaphore).
                // Erneuter Content-Zugriff in OnLoaded trifft den Cache → kein Deadlock.
                if (!IsDisposed) { OnLoaded(); }

                return result;
            } finally {
                _loadSemaphore.Release();
            }
        }
        set {
            lock (_lock) {
                _contentToSave = value;
            }
        }
    }

    /// <summary>
    /// Anzahl der aktuell verfügbaren Bytes:
    /// Im Build-Modus (_buildBuffer gesetzt): Länge des Build-Puffers.
    /// Im Lese-Modus: Länge des gecachten Contents.
    /// </summary>
    public long ContentLength => _contentToSave?.Length ?? 0;

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

    public bool IsLoading {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

            if (!_loadSemaphore.Wait(0)) { return true; }
            _loadSemaphore.Release();
            return false;
        }
    }

    /// <summary>
    /// Gibt an, ob die abgeleitete Klasse die Rohdaten bereits verarbeitet hat.
    /// Wird automatisch auf false gesetzt, wenn die Datei veraltet ist (Invalidate).
    /// Ableitungen setzen dies auf true, nachdem sie ihre Daten erfolgreich verarbeitet haben.
    /// </summary>
    public bool IsParsed { get; protected set; }

    /// <summary>
    /// Gibt an, ob der aktuelle Inhalt gespeichert ist.
    /// Vergleicht den Arbeitsinhalt (_contentToSave) mit dem zuletzt geladenen/gespeicherten Stand (_contentLoaded).
    /// </summary>
    public bool IsSaved {
        get {
            lock (_lock) {
                // Wenn kein Arbeitsinhalt existiert oder dieser identisch mit dem geladenen Stand ist (Referenzprüfung)
                if (_contentToSave == null || ReferenceEquals(_contentToSave, _contentLoaded)) { return true; }

                // Wenn einer von beiden null ist (nach der obigen Prüfung), sind sie ungleich
                if (_contentLoaded == null) { return false; }

                // Längenprüfung als schneller Vorab-Check (Performance-Boost)
                if (_contentToSave.Length != _contentLoaded.Length) { return false; }

                // Tiefer Inhaltsvergleich (System.Linq erforderlich)
                // Bei großen Dateien in 4.8 die performanteste Standard-Variante
                return System.Linq.Enumerable.SequenceEqual(_contentToSave, _contentLoaded);
            }
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
    /// IHasKeyName: Keys sind case-insensitive.
    /// </summary>
    public bool KeyIsCaseSensitive => false;

    /// <summary>
    /// IHasKeyName: Entspricht dem Dateinamen.
    /// </summary>
    public virtual string KeyName => Filename;

    /// <summary>
    /// Gibt an, ob das Laden der Datei fehlgeschlagen ist.
    /// Wird auch gesetzt, wenn der geladene Inhalt kleiner als MinimumBytes ist.
    /// </summary>
    public bool LoadFailed { get; protected set; }

    /// <summary>
    /// Mindestgröße des Inhalts in Bytes.
    /// IsSaveAbleNow und der Ladevorgang prüfen, ob der Inhalt diese Grenze erfüllt.
    /// Wird von abgeleiteten Klassen nach erfolgreichem Laden/Speichern gesetzt.
    /// </summary>
    public int MinimumBytes { get; protected set; } = 0;

    /// <summary>
    /// Gibt an, ob der Inhalt beim Speichern automatisch gezippt werden soll.
    /// Beim Laden wird unabhängig davon geprüft, ob die Datei gezippt ist.
    /// </summary>
    public abstract bool MustZipped { get; }

    #endregion

    #region Methods

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
    /// Stellt sicher, dass der Inhalt vom Dateisystem geladen ist.
    /// Triggert den Lazy-Load über den Content-Getter (synchron, thread-sicher).
    /// </summary>
    /// <returns>True wenn erfolgreich geladen, false bei Fehler oder Disposed.</returns>
    public bool EnsureContentLoaded() {
        if (IsDisposed) { return false; }
        _ = Content; // Triggert Lazy-Load
        return !LoadFailed;
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
            _contentToSave = null;
            _contentLoaded = null;
            IsParsed = false;
        }
    }

    public virtual string IsEditable() {
        if (IsDisposed) { return "Verworfen."; }
        if (IsFreezed) { return FreezedReason; }
        if (LoadFailed) { return "Datei wurde nicht korrekt geladen."; }
        if (IsStale()) { return "Daten müssen neu geladen werden."; }
        if (IsLoading) { return "Daten werden geladen."; }

        return string.Empty;
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Berücksichtigt: IsFreezed, IsDisposed, LoadFailed, MinimumBytes.
    /// </summary>
    public virtual bool IsSaveAbleNow() {
        if (!string.IsNullOrEmpty(IsEditable())) { return false; }

        if (_contentToSave == null || _contentToSave.Length < MinimumBytes) { return false; }
        return true;
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
    /// Markiert den Chunk als fehlgeschlagen geladen.
    /// </summary>
    public void MarkLoadFailed() { LoadFailed = true; }

    /// <summary>
    /// Menschenlesbarer Name dieser Datei für Statusmeldungen (z. B. "Speichere ...").
    /// Muss von konkreten Ableitungen implementiert werden.
    /// </summary>
    public abstract string ReadableText();

    /// <summary>
    /// Speichert den Inhalt unter einem neuen Dateinamen (synchron).
    /// Nutzt GetContent() für die Bytes.
    /// </summary>
    /// <returns>True wenn erfolgreich.</returns>
    public bool SaveAs(string filename) {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(filename)) { return false; }
        if (!IsSaveAbleNow()) { return false; }

        var data = Content;
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
    /// Speichert den Inhalt asynchron mit Backup-Rotation und Semaphore-Synchronisierung.
    /// Nutzt GetContent() und IsSaveAbleNow().
    /// Nicht überschreibbar — nutze OnSaved() für Aktionen nach erfolgreichem Speichern.
    /// </summary>
    /// <returns>Leerer String bei Erfolg, Fehlermeldung bei Fehler.</returns>
    public async Task<string> SaveExtended() {
        if (!_saveSemaphore.Wait(0)) { return "Anderer Speichervorgang läuft"; }

        try {
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Benutzer-Aktion abwarten"; }

            if (!IsSaveAbleNow()) { return "Nicht speicherbar (IsSaveAbleNow = false)"; }

            Develop.Message(ErrorType.DevelopInfo, this, Filename.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {ReadableText()}", 0);

            return await Task.Run(() => {
                try {
                    var dataUncompressed = Content;
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
                        _contentLoaded = _contentToSave;
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
    /// Optionales Symbol für UI-Darstellungen. Standard: null.
    /// </summary>
    public virtual QuickImage? SymbolForReadableText() => null;

    /// <summary>
    /// Gibt eine Stringdarstellung der gecachten Datei zurück.
    /// </summary>
    public override string ToString() => $"{GetType().Name}: {Filename}";

    /// <summary>
    /// Wartet, bis alle laufenden Lade- und Speichervorgänge abgeschlossen sind.
    /// Nützlich, um vor einem geplanten Reload sicherzustellen, dass kein I/O läuft.
    /// </summary>
    public void WaitDiskOperationFinished() {
        if (IsDisposed) { return; }
        _loadSemaphore.Wait();
        _loadSemaphore.Release();
        _saveSemaphore.Wait();
        _saveSemaphore.Release();
    }

    /// <summary>
    /// Ruft das Loaded-Ereignis auf.
    /// Kann von Ableitungen überschrieben werden, um auf Ladeabschluss zu reagieren.
    /// Wird automatisch nach jedem Frisch-Ladevorgang durch den Content-Getter aufgerufen.
    /// </summary>
    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Wird nach erfolgreichem Speichern aufgerufen (nachdem _isSaved = true gesetzt wurde).
    /// Ableitungen können hier interne Zustände nach dem Speichern aktualisieren.
    /// </summary>
    protected virtual void OnSaved() { }

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