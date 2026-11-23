// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Datei-Verwaltung für einen Komplettstand im Arbeitsunterordner: Die
/// Kopf-Datei trägt den Layout-Kopf, der Datenstand liegt als Einzeldatei
/// im Unterordner gleichen Namens ohne Suffix — schreiben mit Backup-Rotation,
/// laden, bedarfsgesteuertes Speichern über den Instanz-Tick. Das Einlesen
/// läuft über den Inhalts-Seam der Basis (ParseMainContent); den Speicher-
/// Inhalt erzeugt das Layout selbst über GenerateContentToSave.
/// </summary>
public class SaveableFileOnefile : SaveableFile {

    #region Layout

    /// <summary>
    /// ClassId des Komplettstand-Layouts; steht im Layout-Kopf der Kopf-Datei.
    /// </summary>
    public static string ClassId => "Onefile";

    /// <summary>
    /// Die ClassId des konkreten Layouts.
    /// </summary>
    protected override string LayoutId => ClassId;

    /// <summary>
    /// Anzeigename des Komplettstand-Layouts für Auswahldialoge.
    /// </summary>
    public override string ReadableText() => "Komplettstand";

    /// <summary>
    /// Kurzinfo des Komplettstand-Layouts für Auswahldialoge.
    /// </summary>
    public override string QuickInfo => "Der komplette Datenstand liegt als eine Datei im Arbeitsunterordner.";

    #endregion

    #region Fields

    /// <summary>
    /// Suffix der Datendatei im Arbeitsunterordner.
    /// </summary>
    public const string SuffixDataFile = "dat";

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private int _checkerTickCount = -5;

    /// <summary>
    /// UTC-Zeitpunkt der letzten Nutzung (TouchChunk), steuert IsRecentlyUsed.
    /// </summary>
    private DateTime _lastUsedUtc = DateTime.MinValue;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine Layout-Instanz ohne Datei-Bezug für die Aufzählung der Datei-Layouts.
    /// </summary>
    public SaveableFileOnefile() { }

    /// <summary>
    /// Erstellt die Datei-Verwaltung für eine Hauptdatei.
    /// </summary>
    public SaveableFileOnefile(string filename) : base(filename) {
        InitialSavePending = true;
        SaveRequired = true;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Mindestgröße des Datei-Inhalts; schützt vor korrupten Re-Saves. Beim initialen
    /// Speichern ist der Inhalt legitim klein.
    /// </summary>
    protected virtual int MinimumContentSize => 200;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC);
    /// ermittelt aus LastWriteTimeUtc der Datendatei.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(DataFile);
            if (fi is { Exists: true }) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Das Objekt gilt als kürzlich verwendet, wenn seit dem letzten TouchChunk
    /// weniger als SkipIfUnusedMinutes vergangen sind.
    /// </summary>
    public override bool IsRecentlyUsed => DateTime.UtcNow.Subtract(_lastUsedUtc).TotalMinutes < SkipIfUnusedMinutes;

    #endregion

    #region Methods

    /// <summary>
    /// Arbeitsunterordner gleichen Namens ohne Suffix; hier liegt der
    /// Komplettstand mit Backup.
    /// </summary>
    public string WorkFolder() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + "\\";

    /// <summary>
    /// Datendatei mit dem Komplettstand im Arbeitsunterordner.
    /// </summary>
    public string DataFile => string.IsNullOrEmpty(Filename) ? string.Empty : WorkFolder() + KeyName + "." + SuffixDataFile;

    /// <summary>
    /// Stempelt die letzte Nutzung.
    /// </summary>
    public override void TouchChunk(string chunkValue) => _lastUsedUtc = DateTime.UtcNow;

    /// <summary>
    /// Prüft die Hauptdatei. Bei einer Einzeldatei gibt es nichts
    /// Fremdes zum Nachladen — der Tick kümmert sich ums Speichern.
    /// </summary>
    public override bool Refresh(bool firstTime) => true;

    /// <summary>
    /// Periodischer Tick: speichert nach Inaktivität oder Vielzahl von Ticks.
    /// </summary>
    public override void CheckerTick() {
        if (!SaveRequired || !MaySaveNow) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        var timeSinceLastAction = Develop.GetUserIdleSeconds();

        // Bestimme ob gespeichert werden muss
        var mustSave = _checkerTickCount > 20 && timeSinceLastAction > 20 ||
                       _checkerTickCount > 110 ||
                       FastSaveDue && _checkerTickCount > 50;

        if (_checkerTickCount < 200 && mustSave && SaveBlockedByPendingWork) { mustSave = false; }

        // Speichern wenn nötig
        if (mustSave) { _ = Save(); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    /// <summary>
    /// True, wenn anstehende eigene Änderungen das Speichern aktuell verbieten.
    /// Basis: nein.
    /// </summary>
    protected virtual bool SaveBlockedByPendingWork => false;

    /// <summary>
    /// True, wenn häufigere Änderungen einen früheren Speichervorgang rechtfertigen.
    /// Basis: nein.
    /// </summary>
    protected virtual bool FastSaveDue => false;

    /// <summary>
    /// Speichert die Daten, wenn der Stand geändert wurde.
    /// </summary>
    public override OperationResult Save() {
        if (Develop.AllReadOnly) { return OperationResult.Success; }
        if (!SaveRequired) { return OperationResult.Success; }

        Develop.Message(ErrorType.Info, null, KeyName, ImageCode.Diskette, $"Speichere {KeyName}", 1);

        try {
            var result = SaveInternal();
            OnInvalidateView();

            if (!string.IsNullOrEmpty(result)) { return OperationResult.Failed(result); }
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Stellt den Komplettstand aus der Datendatei im Arbeitsverzeichnis wieder
    /// her; fehlt sie, wird zuerst das Backup geprüft.
    /// </summary>
    protected override bool LoadMainDataCore() {
        RecoverMainFile();

        if (!IO.FileExists(DataFile)) {
            MainFileMissing();
            return false;
        }

        var content = LoadMainContent();

        if (content is null) {
            Freeze("Laden fehlgeschlagen");
            return false;
        }

        if (content is not { Length: > 0 } || content[0] != FragmentRecord.StandardStorageType) {
            Freeze("Kein gültiger Fragment-Inhalt");
            return false;
        }

        using var ms = new MemoryStream(content);

        // Objekt-Baum inklusive Änderungs-Historie bis zum Datei-Ende
        if (!ParseMainContent(ms)) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        InitialSavePending = false; // vollständiger Stand aus der Datei geladen
        return true;
    }

    /// <summary>
    /// Liest den Inhalt der Datendatei; null beim Ladefehler oder wenn die Datei fehlt.
    /// </summary>
    private byte[]? LoadMainContent() {
        if (!IO.FileExists(DataFile)) { return null; }

        if (IO.ReadAllBytes(DataFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) { return null; }

        return rawBytes.IsZipped() ? rawBytes.UnzipIt() : rawBytes;
    }

    /// <summary>
    /// Versucht, eine fehlende/unvollständige Datendatei aus der Backup-Datei
    /// (.bak) wiederherzustellen: Einmaliger Versuch, nur mit gültigem EOF-Marker.
    /// </summary>
    private void RecoverMainFile() {
        var backup = DataFile + ".bak";

        if (IO.FileExists(DataFile) || !IO.FileExists(backup)) { return; }

        if (IO.ReadAllBytes(backup, 5).Value is not byte[] backupBytes || backupBytes.Length == 0) { return; }

        byte[]? content;
        if (backupBytes.IsZipped()) {
            content = backupBytes.UnzipIt();
            if (content is null) { return; }
        } else {
            content = backupBytes;
        }

        if (!FragmentRecord.HasValidEofMarker(content)) { return; }

        IO.FileCopy(backup, DataFile, false);

        Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Diskette, $"Backup wiederhergestellt: {backup.FileNameWithSuffix()}", 0);
    }

    /// <summary>
    /// Behandelt eine fehlende Datendatei: Freeze und Meldung.
    /// </summary>
    private void MainFileMissing() {
        Freeze("Datendatei existiert nicht");

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, "Datendatei nicht im Dateisystem vorhanden: " + DataFile.FileNameWithSuffix(), 0); }
    }

    /// <summary>
    /// Interner Speicheraufruf: erzeugt den Inhalt, prüft Größe und
    /// Änderungsfreiheit und schreibt die Datei.
    /// </summary>
    protected virtual string SaveInternal() {
        try {
            var result = SaveFullFile();

            if (string.IsNullOrEmpty(result)) {
                InitialSavePending = false;
                SaveRequired = false;
                MainLoadDone = true; // die Datei entspricht jetzt dem Speicherstand
            }

            OnInvalidateView();

            return result;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    /// <summary>
    /// Erzeugt den Inhalt, prüft Größe und Änderungsfreiheit und schreibt die Datei.
    /// </summary>
    private string SaveFullFile() {
        var f = EditableError(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        EnsureHeadFileExists();

        var x = LastChangeUtc;

        // Kompletten Bestand auf die neuesten MaxUndos Sätze kürzen.
        List<FragmentRecord> undos;
        lock (UndoLock) { undos = Undos.TrimUndos(); }

        var content = GenerateContentToSave(undos);

        if (x != LastChangeUtc) { return "Daten wurden während der Speicherung geändert."; }

        // Die Mindestgrößen-Prüfung schützt vor korrupten Re-Saves. Beim initialen
        // Speichern ist der Inhalt legitim klein und darf nicht zum Freeze führen —
        // sonst wäre das neue Objekt sofort blockiert.
        if (!InitialSavePending && content.Count < MinimumContentSize) {
            Freeze("Datei zu klein für Speicherung");
            return "Datei zu klein für Speicherung.";
        }

        var registerError = RegisterMainContent();
        if (!string.IsNullOrEmpty(registerError)) { return registerError; }

        var result = Save([.. content]);
        if (result.IsFailed) { return result.FailedReason; }

        return string.Empty;
    }

    /// <summary>
    /// Speichert den übergebenen Inhalt mit Backup-Rotation und
    /// Semaphore-Synchronisierung. Der Inhalt wird gezippt gespeichert.
    /// </summary>
    private OperationResult Save(byte[] content) {
        if (IsDisposed) { return OperationResult.Failed("Objekt bereits verworfen."); }

        var acquired = false;
        try {
            acquired = _saveSemaphore.Wait(0);
            if (!acquired) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }
        } catch (ObjectDisposedException) {
            return OperationResult.Failed("Objekt wurde während des Zugriffs verworfen.");
        }

        try {
            if (content is null || content.Length == 0) {
                return OperationResult.Failed("Keine Daten zum Speichern");
            }

            if (EditableError(false) is { Length: > 0 } f) {
                return OperationResult.Failed(f);
            }

            if (!FragmentRecord.HasValidEofMarker(content)) {
                return OperationResult.Failed("Kein gültiger EOF-Marker");
            }

            Develop.Message(ErrorType.DevelopInfo, this, DataFile.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {KeyName}", 0);

            var contentToWrite = content.ZipIt() ?? [];
            if (contentToWrite.Length == 0) {
                return OperationResult.Failed("Komprimierung fehlgeschlagen");
            }

            var result = IO.SaveExtended(DataFile, contentToWrite);

            if (result.IsSuccessful) {
                MainContentSaved();
            }

            return result;
        } finally {
            if (acquired) {
                try {
                    _saveSemaphore.Release();
                } catch { }
            }
        }
    }

    /// <summary>
    /// Nach erfolgreichem Speichern der Hauptdatei: nichts.
    /// </summary>
    private void MainContentSaved() { }

    /// <summary>
    /// Stellt sicher, dass das Arbeitsverzeichnis der Datendatei existiert.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private string RegisterMainContent() {
        var opr = IO.CreateDirectory(WorkFolder());

        return opr.IsFailed ? opr.FailedReason : string.Empty;
    }

    /// <summary>
    /// Dateiberechtigte Prüfung nach dem gelungenen Einlesen: Arbeitsverzeichnis muss schreibbar sein.
    /// </summary>
    protected override void CheckWritePermissionAfterLoad(string filename) {
        var opr = IO.CanWriteInDirectory(DataFile.FilePath());

        if (opr.IsFailed) { Freeze(opr.FailedReason); }
    }

    #endregion
}
