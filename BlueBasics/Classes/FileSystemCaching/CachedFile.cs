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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.IO;
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
    /// Schreib-/Build-Puffer für abgeleitete Klassen. Alle Werte beziehen sich auf _content.
    /// Wenn gesetzt, liefert DataLength die Anzahl der gepufferten Bytes.
    /// </summary>
    private byte[]? _content;

    /// <summary>
    /// Der SHA256-Hash des aktuellen Arbeitsinhalts (_content).
    /// Wird analog zum Content bei Bedarf generiert.
    /// </summary>
    private string? _contentHash;

    /// <summary>
    /// Der SHA256-Hash des Inhalts, wie er zuletzt vom Dateisystem geladen oder dorthin gespeichert wurde.
    /// Dient dem Vergleich in IsSaved.
    /// </summary>
    private string? _contentOnDiskHash;

    private FileInfo? _fileInfo;

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
                if (!IsStaleQuick() && _content != null) { return _content; }
            }

            // Ladepfad: Semaphore erwerben, damit IsLoading = true
            _loadSemaphore.Wait();
            try {
                // Nach Semaphore-Erwerb nochmals prüfen (ein anderer Thread könnte geladen haben)
                lock (_lock) {
                    if (!IsStaleQuick() && _content != null) { return _content; }
                }

                var (content, timestamp, loadFailed) = ReadContentFromFileSystem();

                lock (_lock) {
                    // Dritte Prüfung im Lock – Race Condition absichern
                    if (!IsStaleQuick() && _content != null) { return _content; }

                    LoadFailed = loadFailed;
                    _fileInfo = timestamp;

                    if (!loadFailed) {
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

                        // MinimumBytes-Prüfung: Inhalt zu klein → Ladung als fehlgeschlagen markieren
                        if (MinimumBytes > 0 && !LoadFailed && _content.Length < MinimumBytes) {
                            LoadFailed = true;
                            _content = [];
                        }
                    }

                    _contentOnDiskHash = Generic.GetSHA256HashString(_content);
                    _contentHash = _contentOnDiskHash;
                }

                // OnLoaded nach dem Frisch-Laden aufrufen (außerhalb des Locks, aber noch im Semaphore).
                // Erneuter Content-Zugriff in OnLoaded trifft den Cache → kein Deadlock.
                if (!IsDisposed) { OnLoaded(); }

                return _content ?? [];
            } finally {
                _loadSemaphore.Release();
            }
        }
        set {
            lock (_lock) {
                //var f = $"C:\\01_DATA\\{DateTime.UtcNow.ToString4()}";

                //if (_content != null && Filename.ToLowerInvariant().Contains("_var")) {
                //    IO.WriteAllBytes($"{f}-alt.txt", _content);
                //    IO.WriteAllBytes($"{f}-neu.txt", value);
                //}

                _content = value;
                _contentHash = null; // Reset, damit er bei Bedarf neu berechnet wird
            }
        }
    }

    /// <summary>
    /// Anzahl der aktuell verfügbaren Bytes:
    /// Im Build-Modus (_buildBuffer gesetzt): Länge des Build-Puffers.
    /// Im Lese-Modus: Länge des gecachten Contents.
    /// </summary>
    public long ContentLength => _content?.Length ?? 0;

    public abstract bool ExtendedSave { get; }

    public FileInfo? FileInfo {
        get {
            if (_fileInfo == null) {
                _fileInfo = GetFileInfo(Filename);
            }

            return _fileInfo;
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
    /// Gibt an, ob der aktuelle Inhalt gespeichert ist.
    /// Vergleicht den Hash des Arbeitsinhalts mit dem Hash des zuletzt geladenen/gespeicherten Standes.
    /// </summary>
    public bool IsSaved {
        get {
            lock (_lock) {
                if (_content == null && _contentOnDiskHash == null) {
                    // nix geladen und nix zu speichern
                    return true;
                }
                // Wenn kein Arbeitsinhalt existiert oder die Hashes identisch sind
                _contentHash ??= Generic.GetSHA256HashString(_content);
                return _contentHash == _contentOnDiskHash;
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

        // WICHTIG: Erst markieren wir IsDisposed = true (oben geschehen).
        // Das verhindert, dass neue Save-Vorgänge starten.

        Invalidate();

        // Wir müssen sicherstellen, dass wir die Semaphoren erst disposen,
        // wenn kein Thread mehr darin hängt.
        // Da Dispose synchron ist, nutzen wir einen Lock oder warten kurz.
        // Ein sauberer Weg ist, die Semaphoren erst zu "locken", bevor man sie zerstört.

        _loadSemaphore.Wait();
        _loadSemaphore.Dispose();

        _saveSemaphore.Wait();
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
            _fileInfo = null;
            _content = null;
            _contentHash = null;
            _contentOnDiskHash = null;
        }
    }

    public virtual string IsNowEditable() {
        if (IsDisposed) { return "Verworfen."; }
        if (IsFreezed) { return FreezedReason; }
        if (LoadFailed) { return "Datei wurde nicht korrekt geladen."; }
        if (IsStaleQuick()) { return "Daten müssen neu geladen werden."; }
        if (IsLoading) { return "Daten werden geladen."; }
        if (_contentOnDiskHash == null) { return "Interner Fehler."; }

        return CanWriteFile(Filename, 2);
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Berücksichtigt: IsFreezed, IsDisposed, LoadFailed, MinimumBytes.
    /// </summary>
    public virtual bool IsSaveAbleNow() {
        if (!string.IsNullOrEmpty(IsNowEditable())) { return false; }

        if (_content == null || _content.Length < MinimumBytes) { return false; }
        return true;
    }

    /// <summary>
    /// Prüft, ob die Datei im Dateisystem geändert wurde.
    /// </summary>
    public bool IsStale() {
        lock (_lock) {
            if (_fileInfo == null) { return true; }
        }

        var newFileInfo = GetFileInfo(Filename, false, 0.1f);

        // Wenn die Datei nicht mehr existiert, ist der Cache definitiv veraltet (stale)
        if (newFileInfo == null) { return true; }

        lock (_lock) {
            // Veraltet ist es, wenn sich Größe ODER Zeit geändert haben
            return _fileInfo.Length != newFileInfo.Length ||
                   _fileInfo.LastWriteTime != newFileInfo.LastWriteTime;
        }
    }

    public bool IsStaleQuick() {
        if (_fileInfo == null) { return true; }
        if (_contentOnDiskHash == null) { return true; }
        return false;
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
    /// Speichert den Inhalt asynchron mit optionaler Backup-Rotation und Semaphore-Synchronisierung.
    /// Nicht überschreibbar — nutze OnSaved() für Aktionen nach erfolgreichem Speichern.
    /// </summary>
    /// <returns>Ein OperationResult, das über Erfolg oder Fehler informiert.</returns>
    public async Task<OperationResult> Save() {
        // Prüfung auf Disposed VOR dem Zugriff auf die Semaphore
        if (IsDisposed) { return OperationResult.Failed("Objekt bereits verworfen."); }

        try {
            if (!_saveSemaphore.Wait(0)) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }
        } catch (ObjectDisposedException) {
            return OperationResult.Failed("Objekt wurde während des Zugriffs verworfen.");
        }

        try {
            if (!IsSaveAbleNow()) { return OperationResult.Failed("Nicht speicherbar (IsSaveAbleNow = false)"); }

            Develop.Message(ErrorType.DevelopInfo, this, Filename.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {ReadableText()}", 0);

            return await Task.Run(() => {
                var backup = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}.bak";
                var tempfile = TempFile($"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}.tmp-{UserName.ToUpperInvariant()}");

                try {
                    // Prüfung innerhalb des Tasks, ob wir inzwischen disposed wurden
                    if (IsDisposed) { return OperationResult.Failed("Vorgang abgebrochen, da Objekt verworfen."); }

                    // Alle beteiligten Dateien auf die Ignore-Liste setzen, um Watcher-Feedback-Loops zu vermeiden
                    CachedFileSystem.BeginIgnoreFile(Filename);
                    CachedFileSystem.BeginIgnoreFile(backup);
                    CachedFileSystem.BeginIgnoreFile(tempfile);

                    var contenToWrite = MustZipped ? Content.ZipIt() ?? [] : Content;
                    if (contenToWrite.Length == 0) { return OperationResult.Failed("Komprimierung fehlgeschlagen"); }

                    if (ExtendedSave) {
                        var result = WriteAllBytes(tempfile, contenToWrite);
                        if (result.IsFailed) {
                            DeleteFile(tempfile, false);
                            return result;
                        }

                        //var tempFileInfo = GetFileInfo(tempfile, true, 60);
                        //if (tempFileInfo == null) {
                        //    DeleteFile(tempfile, false);
                        //    return OperationResult.Failed("Dateiinfo konnte nicht gelesen werden");
                        //}

                        if (FileExists(backup) && !DeleteFile(backup, false)) {
                            DeleteFile(tempfile, false);
                            return OperationResult.Failed("Backup konnte nicht gelöscht werden");
                        }

                        if (FileExists(Filename) && !MoveFile(Filename, backup, false)) {
                            DeleteFile(tempfile, false);
                            return OperationResult.Failed("Hauptdatei konnte nicht verschoben werden");
                        }

                        if (!MoveFile(tempfile, Filename, false)) {
                            if (FileExists(backup) && !FileExists(Filename)) {
                                MoveFile(backup, Filename, false);
                            }
                            DeleteFile(tempfile, false);
                            return OperationResult.Failed("Speichervorgang fehlgeschlagen");
                        }
                    } else {
                        if (FileExists(Filename) && !DeleteFile(Filename, false)) {
                            return OperationResult.Failed("Alte Datei konnte nicht gelöscht werden");
                        }

                        _contentOnDiskHash = null;
                        var result = WriteAllBytes(Filename, contenToWrite);
                        if (result.IsFailed) { return result; }
                    }

                    lock (_lock) {
                        _contentOnDiskHash = Generic.GetSHA256HashString(_content);
                        _fileInfo = GetFileInfo(Filename);
                    }

                    OnSaved();

                    return OperationResult.Success;
                } catch (Exception ex) {
                    return OperationResult.Failed(ex);
                } finally {
                    // Wir geben dem Dateisystem einen Moment Zeit, die Events zu feuern,
                    // bevor wir die Pfade wieder aus der Ignore-Liste nehmen.
                    _ = Task.Delay(200).ContinueWith(_ => {
                        CachedFileSystem.EndIgnoreFile(Filename);
                        CachedFileSystem.EndIgnoreFile(backup);
                        CachedFileSystem.EndIgnoreFile(tempfile);
                    });
                }
            }).ConfigureAwait(false);
        } finally {
            // Hier ist die kritische Stelle: Nur releasen, wenn nicht disposed!
            try {
                if (!IsDisposed) {
                    _saveSemaphore.Release();
                }
            } catch (ObjectDisposedException) {
                // Falls es genau dazwischen passiert ist: Ignorieren, da wir eh fertig sind.
            }
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

        var data = Content;
        if (data.Length == 0) { return false; }

        var dataToWrite = MustZipped ? data.ZipIt() : data;
        if (dataToWrite == null || dataToWrite.Length == 0) { return false; }

        var backup = $"{filename.FilePath()}{filename.FileNameWithoutSuffix()}.bak";
        var tmpFile = TempFile($"{filename.FilePath()}{filename.FileNameWithoutSuffix()}.tmp-{UserName.ToUpperInvariant()}");

        var result = WriteAllBytes(tmpFile, dataToWrite);
        if (result.IsFailed) {
            DeleteFile(tmpFile, false);
            return result.IsFailed;
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
    private (byte[] Content, FileInfo? FileInfo, bool LoadFailed) ReadContentFromFileSystem() {
        try {
            var retries = 0;
            do {
                var fileInfo1 = GetFileInfo(Filename, false, 0.1f);
                if (fileInfo1 is null) { return ([], null, false); }

                var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
                var fileInfo2 = GetFileInfo(Filename, false, 2f);
                if (fileInfo2 != null &&
                    fileInfo1.LastWriteTime == fileInfo2.LastWriteTime &&
                    fileInfo1.Length == fileInfo2.Length) { return (content, fileInfo2, false); }

                retries++;
            } while (retries < 20);

            return ([], null, true); // Datei ändert sich ständig, Laden fehlgeschlagen
        } catch {
            return ([], null, true);
        }
    }

    #endregion
}