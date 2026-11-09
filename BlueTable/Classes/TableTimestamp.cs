// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Dummy-Hauptdatei (.tblt, 0 Byte) und Arbeitsverzeichnis
/// ([Tabellenname]\), in dem die wirkliche Tabelle als Zeitstempel-Snapshot liegt
/// (yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tbls, UTC, write-once).
/// Änderungen werden als Fragmente (KEYNAME-Maschine-Zeitstempel.frg) in das
/// gleiche Verzeichnis geschrieben und von allen Benutzern nachgeladen —
/// eine bestehende Datei wird nie überschrieben. Bei der Komplettierung legt
/// der Master einen neuen Snapshot an; die alte Datei bleibt unangetastet
/// (kein File.Replace-Tausch, keine Backup-Rotation). Danach räumen alte
/// Fragmente und überzählige Snapshots auf.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableTimestamp : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Sekunden. Nach dieser Zeit soll der Aufräumer beendet werden.
    /// </summary>
    public static readonly int AbortFragmentDeletion = 10;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit dürfen Fragmente gelöscht werden.
    /// </summary>
    public static readonly int DeleteFragmentsAfter = 60 * 2 + 5 * 2;

    /// <summary>
    /// Wert in Stunden. Snapshots, die älter sind, werden gelöscht
    /// (sofern mindestens KeepSnapshots neuere existieren).
    /// </summary>
    public static readonly int DeleteSnapshotsAfterHours = 2;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen.
    /// </summary>
    public static readonly int DoComplete = 60;

    /// <summary>
    /// Anzahl der zu behaltenden Snapshots im Arbeitsverzeichnis.
    /// </summary>
    public const int KeepSnapshots = 3;

    /// <summary>
    /// Gibt das Suffix für Fragmentdateien zurück.
    /// </summary>
    public static readonly string SuffixOfFragments = "frg";

    /// <summary>
    /// Gibt das Suffix für Snapshot-Dateien (Komplettstände) zurück.
    /// </summary>
    public static readonly string SuffixOfSnapshots = "tbls";

    /// <summary>
    /// Liste der Änderungen, die noch nicht in einem Snapshot enthalten sind.
    /// </summary>
    private readonly List<UndoItem> _changesNotIncluded = [];

    /// <summary>
    /// Hashes bereits verbuchter Items (Set-Semantik), um doppelte Verarbeitung zu verhindern.
    /// Beim ersten Laden aus der Historie des Snapshots befüllt; ein Hash kommt nur hinzu,
    /// nachdem das Item erfolgreich angewendet wurde. Thread-safe durch ConcurrentDictionary;
    /// Value wird nicht genutzt.
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _processedHashes = new();

    /// <summary>
    /// Zähler für aktive Änderungsprozesse zur Vermeidung von Race-Conditions.
    /// Während der Daten aktualisiert werden, dürfen z.B. keine Tabellenansichten gemacht werden.
    /// </summary>
    private int _doingChanges;

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    private DateTime _isInCache = new(0);

    /// <summary>
    /// UTC-Zeitpunkt der letzten Nutzung (TouchChunk), steuert IsRecentlyUsed.
    /// </summary>
    private DateTime _lastUsedUtc = DateTime.MinValue;

    private bool _masterNeeded;

    private string _myFragmentsFilename = string.Empty;

    /// <summary>
    /// Der aktuell geladene Snapshot. Wird beim Aufräumen nie gelöscht.
    /// </summary>
    private string _mySnapshotFile = string.Empty;

    /// <summary>
    /// UTC-Zeitpunkt des neuesten bekannten Snapshots (aus dem Dateinamen).
    /// Maß für DoComplete und für die Fragment-Verwerfungs-Toleranz.
    /// </summary>
    private DateTime _newestSnapshotUtc = DateTime.MinValue;

    private System.IO.StreamWriter? _writer;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialisiert eine neue Instanz der TableTimestamp Klasse.
    /// </summary>
    /// <param name="tablename">Name der Tabelle.</param>
    public TableTimestamp(string tablename) : base(tablename) { }

    /// <summary>
    /// Initialisiert eine neue Instanz der TableTimestamp Klasse als Kopie einer bestehenden Tabelle.
    /// </summary>
    /// <param name="filename">Dateiname der neuen Tabelle.</param>
    /// <param name="source">Quelltabelle, deren Daten kopiert werden.</param>
    public TableTimestamp(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gibt an, ob die Fragmentdatei beim Schließen gelöscht werden darf (wenn keine wichtigen Änderungen enthalten sind).
    /// </summary>
    public bool CanDeleteWriter { get; private set; } = true;

    /// <summary>
    /// Die Tabelle gilt als kürzlich genutzt, wenn seit dem letzten TouchChunk
    /// weniger als SkipIfUnusedMinutes vergangen sind. Ohne diese Überschreibung
    /// würde der periodische TableUpdater die Tabelle immer überspringen,
    /// da er auf Chunk-LiveInstances der Dummy-Datei nie trifft.
    /// </summary>
    public override bool IsRecentlyUsed => DateTime.UtcNow.Subtract(_lastUsedUtc).TotalMinutes < Chunk.SkipIfUnusedMinutes;

    /// <summary>
    /// Wenn die Prüfung ergibt, dass eine Komplettierung fällig ist, wird hier auf true gesetzt.
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    /// <summary>
    /// Gibt an, ob Multi-User-Zugriff möglich ist.
    /// </summary>
    public override bool MultiUserPossible => true;

    /// <summary>
    /// UTC-Zeitpunkt des neuesten Snapshots. Aus dem Dateinamen ermittelt,
    /// nicht aus LastWriteTimeUtc — der Zeitstempel im Namen ist der alleinige
    /// Vergleich für Aktualität (robust gegen Kopieren/Archivieren).
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate =>
        _newestSnapshotUtc == DateTime.MinValue
            ? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : _newestSnapshotUtc;

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob die aktuelle Instanz als temporärer Master fungieren darf.
    /// </summary>
    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (_isInCache.Year < 2000) { return false; }

        if (updateAllowed && DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > UpdateTable) {
            if (!BeSureToBeUpToDate(false)) { return false; }
        }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    /// <summary>
    /// Stellt sicher, dass die Daten aktuell sind, indem die Fragmente nachgeladen werden.
    /// </summary>
    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (firstTime) {
            _isInCache = LastSaveMainFileUtcDate;

            // Dedup-Grundlage ist die Historie des Snapshots: Items, deren Hash dort
            // steht, gelten als verbucht und werden nicht erneut angewendet. Das ist
            // korrekt, weil Daten und Historie aus demselben Snapshot gespeichert
            // werden und die Historie nur um erfolgreich angewendete Items wächst.
            _processedHashes.Clear();
            List<UndoItem> undoSnapshot;
            lock (_undoLock) {
                undoSnapshot = [.. Undo];
            }
            foreach (var u in undoSnapshot) {
                _processedHashes.TryAdd(u.Hash(), default);
            }
        }

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Fragmente von '" + KeyName + "'", 0); }

        // Zeitstempel VOR dem Lesen der Fragmente erfassen.
        // Wird als endTimeUtc an InjectData übergeben und dort als _isInCache gesetzt.
        // Konservativ: Fragmente, die während des Lesens hinzukommen, werden beim
        // nächsten Aufruf erneut gelesen - lieber doppelt als verloren.
        var readStartedUtc = DateTime.UtcNow;

        var (changes, files, failed) = GetLastChanges();
        if (failed) { return false; }

        var opr = InjectData(files, changes, DateTime.UtcNow, readStartedUtc, firstTime);
        return opr.IsSuccessful;
    }

    /// <summary>
    /// Bereitet die Fortführung einer mit "- EOF" abgeschlossenen Fragment-Datei vor:
    /// Ordner- und Existenzprüfung, Writer-Übernahme und Kennzeichnung als eigene Datei.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    public string ContinueFragment(string fragmentFilename) {
        if (!fragmentFilename.StartsWith(WorkFolder(), StringComparison.OrdinalIgnoreCase)) { return "Die Datei liegt nicht im Arbeitsverzeichnis der Tabelle: " + fragmentFilename; }
        if (!IO.FileExists(fragmentFilename)) { return "Fragment-Datei nicht gefunden: " + fragmentFilename; }

        // Ggf. bereits offenen Writer schließen (z. B. Master-Eintrag beim Laden):
        // Diese (fast leere) Neue-Datei darf das Aufräumen dann entsorgen.
        CloseWriter();

        // Ab sofort als eigene Datei führen: hält sie beim Nachladen aus den
        // Fremd-Fragmenten heraus und schützt sie vor Aufräum-Löschungen.
        _myFragmentsFilename = fragmentFilename;
        CanDeleteWriter = false;

        var error = TableFragments.ContinueFragment(fragmentFilename, "- CONTINUED", out var writer);
        _writer = writer;
        return error;
    }

    /// <summary>
    /// Prüft, warum die Tabelle aktuell nicht editierbar ist.
    /// </summary>
    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        if (string.IsNullOrEmpty(WorkFolder())) { return "Arbeitsverzeichnis nicht gesetzt."; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    /// <summary>
    /// Ohne Chunk-Verwaltung gibt es keine Edit-Sperre pro Datei; konkurrierende
    /// Änderungen werden über die Fragment-Historie zusammengeführt.
    /// </summary>
    public override string IsValueEditable(TableDataType type, string? chunkValue) => IsGenericEditable(false);

    /// <summary>
    /// Setzt die Instanz als Master, sofern die Daten aktuell genug sind.
    /// </summary>
    public override void MasterMe() {
        if (DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > 1) { return; }
        base.MasterMe();
    }

    /// <summary>
    /// Stempelt die letzte Nutzung. Rufen TableViews und ConnectedFormula-Views
    /// beim Anzeigen von Zeilen auf, damit der periodische TableUpdater die
    /// Tabelle weiterhin aktuell hält.
    /// </summary>
    public override void TouchChunk(string chunkValue) => _lastUsedUtc = DateTime.UtcNow;

    /// <summary>
    /// Arbeitsverzeichnis der Tabelle. Hier liegen die Snapshot-Dateien
    /// (Komplettstände) und die Fragmentdateien aller Benutzer.
    /// </summary>
    public string WorkFolder() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + "\\";

    /// <summary>
    /// Gibt die Ressourcen frei.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            // Master-Schaft abmelden, bevor der Writer geschlossen wird: Die
            // Freigabe landet in der bestehenden Fragmentdatei, statt beim
            // Abmelden (base.Dispose ruft UnMasterMe) eine neue, nie mit EOF
            // abgeschlossene Fragmentdatei zu öffnen.
            UnMasterMe();
            CloseWriter();
        }

        base.Dispose(disposing);

        // Sicherheitsnetz: Falls der Abmelde-Schreiben im base.Dispose doch
        // einen neuen Writer geöffnet hat, diesen sauber abschließen.
        if (disposing && _writer is not null) {
            CloseWriter();
        }
    }

    /// <summary>
    /// Lädt den neuesten vollständigen Snapshot und stellt sicher,
    /// dass Dummy-Datei und Arbeitsverzeichnis existieren.
    /// </summary>
    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(WorkFolder()).IsFailed) {
            Freeze("Arbeitsverzeichnis konnte nicht erstellt werden");
            return false;
        }

        var snapshots = GetSnapshotFilesNewestFirst();

        foreach (var candidate in snapshots) {
            // Read mit Retry: Ein anderer Master kann die Datei gerade anlegen.
            if (IO.ReadAllBytes(candidate, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) { continue; }

            byte[] content;
            if (rawBytes.IsZipped()) {
                content = rawBytes.UnzipIt() ?? [];
            } else {
                content = rawBytes;
            }

            // Unvollständige Snapshots (Schreibvorgang läuft noch) überspringen —
            // der nächstältere vollständige Snapshot ist eine gültige Basis.
            if (!HasValidEofMarker(content)) { continue; }

            if (!Parse(content, true, null)) {
                Freeze("Snapshot '" + candidate.FileNameWithSuffix() + "' konnte nicht geparsest werden");
                return false;
            }

            _mySnapshotFile = candidate;
            _newestSnapshotUtc = TableChunk.ExtractDateFromFileName(candidate) ?? DateTime.UtcNow;
            return true;
        }

        if (snapshots.Count > 0) {
            Freeze("Kein lesbarer Snapshot der Tabelle vorhanden");
            return false;
        }

        // Kein Snapshot (z. B. abgebrochene Erstanlage): Als neue Tabelle starten.
        // Noch vorhandene Fragmente lädt BeSureToBeUpToDate nach; der nächste
        // Speichervorgang erzeugt den ersten Snapshot.
        InitialSavePending = true;
        return true;
    }

    /// <summary>
    /// Speichert die Tabelle. Bei der Erstanlage werden Dummy-Datei und der erste
    /// Snapshot erzeugt; danach werden nur noch die Fragmentdateien geflusht —
    /// Änderungen landen als Zeilen im Fragment, nie als Überschreiben einer Datei.
    /// </summary>
    protected override string SaveInternal() {
        if (InitialSavePending) {
            EnsureDummyFileExists();

            var f = WriteNewSnapshot();
            if (!string.IsNullOrEmpty(f)) { return f; }

            InitialSavePending = false;
            SaveRequired = false;
            return string.Empty;
        }

        if (_writer is null) { return "Writer Fehler"; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            SaveRequired = false;
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    /// <summary>
    /// Schreibt einen Wert als Zeile in die eigene Fragmentdatei.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string previousValue, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (base.WriteValueToDiscOrServer(type, previousValue, value, column, row, user, datetimeutc, comment) is { Length: > 0 } f) { return f; }

        if (Develop.AllReadOnly) { return string.Empty; }

        if (InitialSavePending) { return string.Empty; }

        if (_writer is null) { StartWriter(); }
        if (_writer is null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(KeyName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]");

        try {
            lock (_writer) {
                var line = l.ParseableItems().FinishParseable();
                _writer.WriteLine(line);

                // Eigene Änderungen ebenfalls in den Hash-Cache aufnehmen.
                // Hash() statt roher Zeile: PreviousValue ist im Fragment immer string.Empty,
                // wäre aber im lokalen Undo befüllt - mit Hash() sind beide Wege konsistent.
                _processedHashes.TryAdd(l.Hash(), default);

                if (!type.IsUnimportant()) { CanDeleteWriter = false; }
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }

        return string.Empty;
    }

    /// <summary>
    /// Überprüft und erstellt das Arbeitsverzeichnis.
    /// </summary>
    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        IO.CreateDirectory(WorkFolder());
    }

    /// <summary>
    /// Löscht überzählige Snapshots: Die KeepSnapshots neuesten bleiben stehen,
    /// ältere werden gelöscht, wenn sie älter als DeleteSnapshotsAfterHours sind.
    /// Der Zeitstempel im Dateinamen (UTC) ist das Alter-Kriterium; die Frist
    /// schützt Leser, die die Datei gerade geladen haben.
    /// </summary>
    private void CleanupOldSnapshots(DateTime startTimeUtc) {
        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        var snapshots = GetSnapshotFilesNewestFirst();
        if (snapshots.Count <= KeepSnapshots) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteSnapshotsAfterHours);

        for (var i = snapshots.Count - 1; i >= KeepSnapshots; i--) {
            var thisf = snapshots[i];

            // Der eigene geladene Snapshot bleibt stehen — diese Instanz könnte
            // ihn als Fallback noch einmal benötigen.
            if (string.Equals(thisf, _mySnapshotFile, StringComparison.OrdinalIgnoreCase)) { continue; }

            var fileDate = TableChunk.ExtractDateFromFileName(thisf);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Räume alten Snapshot auf: " + thisf.FileNameWithSuffix(), 0);
                try { IO.DeleteFile(thisf, false); } catch { }
            }

            if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
        }
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Fragmentdatei, falls sie keine permanenten Daten enthält.
    /// </summary>
    private void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose is not null) {
            _writer = null;

            try {
                writerToClose.WriteLine("- EOF");
                writerToClose.Flush();
            } catch { } finally {
                try {
                    writerToClose.Dispose();
                } catch { }
            }

            if (CanDeleteWriter) {
                IO.DeleteFile(_myFragmentsFilename, false);
            }
        }
    }

    /// <summary>
    /// Stellt sicher, dass die Dummy-Hauptdatei (.tblt) existiert (0 Bytes).
    /// </summary>
    private void EnsureDummyFileExists() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        if (!IO.FileExists(Filename)) {
            IO.WriteAllBytes(Filename, []);
        }
    }

    /// <summary>
    /// Führt Aufräumarbeiten nach dem Laden von Fragmenten durch,
    /// inkl. Komplettierung und Bereinigung alter Dateien.
    /// </summary>
    private void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc) {
        if (Ending) { return; }
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }

        // Auch fremde Komplettierungen erkennen: Neuesten Snapshot-Stand übernehmen.
        RefreshNewestSnapshotUtc();

        _masterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > DoComplete;

        // Alte Snapshots räumen immer auf — unabhängig davon, ob fremde
        // Fragmente gelesen wurden.
        CleanupOldSnapshots(startTimeUtc);

        if (files is not { Count: >= 1 }) { return; }

        #region Bei Bedarf neue Komplett-Tabelle erstellen

        if (_masterNeeded && AmITemporaryMaster(MasterTry, MasterUntil, true)) {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Erstelle neue Komplett-Tabelle: " + KeyName, 0);

            var f = WriteNewSnapshot();

            if (!string.IsNullOrEmpty(f)) {
                Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Komplettierung von {Caption} fehlgeschlagen: {f}", 0);
                return;
            }

            CloseWriter();
            StartWriter();

            MasterMe();

            _masterNeeded = false;
            OnInvalidateView();
            _changesNotIncluded.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien schützen, deren Änderungen noch nicht in einem Snapshot stehen

        // Zusätzliche Safety zur Zeitstempel-Prüfung unten: Dateien, die in diesem
        // Durchlauf gelesen wurden und deren Items noch nicht in einem Snapshot
        // stehen (_changesNotIncluded), werden explizit aus der Lösch-Liste entfernt.
        // Nach einer erfolgreichen Komplettierung ist _changesNotIncluded geleert -
        // dann greift ausschließlich die Zeitstempel-Prüfung.
        if (_changesNotIncluded.Count != 0) {
            foreach (var thisch in _changesNotIncluded) {
                files.Remove(thisch.Container);
            }
        }

        #endregion

        files.Shuffle();

        foreach (var thisf in files) {
            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f[^19..];

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalMinutes > DeleteFragmentsAfter &&
                         LastSaveMainFileUtcDate.Subtract(d2).TotalMinutes > DeleteFragmentsAfter) {
                        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix(), 0);
                        IO.DeleteFile(thisf, 0, false);
                        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Listet die Snapshot-Dateien im Arbeitsverzeichnis auf, sortiert nach dem
    /// Zeitstempel im Dateinamen (neueste zuerst). Dateien ohne gültiges Datum
    /// werden ans Ende sortiert. Liest direkt vom Dateisystem, damit Snapshots
    /// anderer Master sofort erkannt werden.
    /// </summary>
    private List<string> GetSnapshotFilesNewestFirst() {
        if (!IO.DirectoryExists(WorkFolder())) { return []; }

        try {
            return IO.GetFiles(WorkFolder())
                .Where(f => string.Equals(f.FileSuffix(), SuffixOfSnapshots, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => TableChunk.ExtractDateFromFileName(f) ?? DateTime.MinValue)
                .ToList();
        } catch {
            return [];
        }
    }

    /// <summary>
    /// Ermittelt die Änderungen aus den Fragmentdateien aller Benutzer.
    /// </summary>
    private (List<UndoItem>? Changes, List<string>? Files, bool failed) GetLastChanges() {
        if (!string.IsNullOrEmpty(IsGenericEditable(true))) { return (null, null, true); }

        CheckPath();

        try {
            var frgma = IO.GetFiles(WorkFolder(), KeyName.ToUpperInvariant() + "-*." + SuffixOfFragments, System.IO.SearchOption.TopDirectoryOnly).ToList();
            frgma.Remove(_myFragmentsFilename);

            if (frgma.Count == 0) { return ([], [], false); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                var fil = IO.ReadAllText(thisf, Encoding.UTF8);

                foreach (var thist in fil.SplitAndCutByCr()) {
                    if (!thist.StartsWith('-')) {
                        var u = new UndoItem(thist);

                        // Zeilen, die sicher vor dem neuesten Snapshot lagen, sind in
                        // dessen Komplett-Stand bereits enthalten - unabhängig davon, ob
                        // ihr Hash in der Historie steht. Diese wird beim Speichern
                        // gefiltert und gekürzt (max. 1000 Items, max. 10 EventScript),
                        // ist also keine vollständige Verbuchungsgrundlage. Die Toleranz
                        // überbrückt das Zeitfenster zwischen dem Lesen der Fragmente und
                        // dem Anlegen eines neuen Snapshots.
                        if (u.DateTimeUtc.AddSeconds(60) < LastSaveMainFileUtcDate) { continue; }

                        // Hash auf Basis der geparsten Werte berechnen (nicht auf der rohen Zeile),
                        // damit der Hash unabhängig vom Serialisationsformat ist und mit den aus
                        // der Snapshot-Historie gebildeten Hashes (in BeSureToBeUpToDate) übereinstimmt.
                        // Atomar als verarbeitet markieren; bei Duplikat sofort überspringen.
                        if (!_processedHashes.TryAdd(u.Hash(), default)) { continue; }

                        u.Container = thisf;
                        l.Add(u);
                    }
                }
            }

            return (l, frgma, false);
        } catch { }
        return (null, null, true);
    }

    /// <summary>
    /// Injiziert die geladenen Fragmentdaten in die aktuelle Tabellenstruktur.
    /// </summary>
    /// <param name="checkedDataFiles"></param>
    /// <param name="data"></param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    /// <param name="initialload"></param>
    private OperationResult InjectData(List<string>? checkedDataFiles, List<UndoItem>? data, DateTime startTimeUtc, DateTime endTimeUtc, bool initialload) {
        if (data is null) { return OperationResult.Success; }

        // Bei initialload die Editierbarkeits-Sperren umgehen - die Tabelle
        // befindet sich gerade im ersten Ladevorgang (IsGenericEditable prüft
        // mit isloading=true). Freeze- und Prozess-Prüfungen bleiben aktiv.
        var f = IsGenericEditable(initialload);
        if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed($"Tabelle nicht bearbeitbar: {f}"); }

        if (Column.ChunkValueColumn is { IsDisposed: false }) { return OperationResult.Failed("Falscher Tabellentyp"); }

        var dataSorted = data.Where(obj => obj?.DateTimeUtc is not null).OrderBy(obj => obj.DateTimeUtc);
        var affectingHead = false;

        try {
            List<string> myfiles = [];
            if (checkedDataFiles is not null) {
                foreach (var thisf in checkedDataFiles) {
                    if (thisf.Contains("\\" + KeyName.ToUpperInvariant() + "-")) {
                        myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            Interlocked.Increment(ref _doingChanges);
            // Während des Einspielens sind Spaltenschlüssel kurzzeitig nicht
            // auflösbar. In diesem Fenster dürfen Ansichten keine Einträge
            // endgültig entfernen (RepairArrangements), sonst verschwinden
            // Spalten dauerhaft aus der Ansicht.
            PauseDataReload();
            try {
                foreach (var thisWork in dataSorted) {
                    if (KeyName == thisWork.TableName) {
                        lock (_undoLock) {
                            Undo.Add(thisWork);
                        }
                        _changesNotIncluded.Add(thisWork);

                        affectingHead |= (!thisWork.Command.IsCellValue() && !thisWork.Command.IsUnimportant());

                        // Undo- und UndoInOne-Einträge wurden oben bereits per Undo.Add(thisWork)
                        // in die Liste übernommen. Ein zusätzliche SetValueInternal-Aufruf würde
                        // sie doppelt eintragen (Undo) bzw. die komplette Liste verwerfen (UndoInOne).
                        if (thisWork.Command == TableDataType.Undo) { continue; }

                        var c = Column[thisWork.ColName];
                        var r = Row.GetByKey(thisWork.RowKey);

                        var error = string.Empty;

                        if (initialload) {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, thisWork.Comment, ChangeFlags.IgnoreFreeze | ChangeFlags.PostProcess);
                        } else {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, thisWork.Comment, ChangeFlags.RaiseEvents | ChangeFlags.IgnoreFreeze | ChangeFlags.PostProcess);
                        }

                        if (!string.IsNullOrEmpty(error)) {
                            // Bewusst NICHT in Undo/_changesNotIncluded aufnehmen: Ein
                            // nicht anwendbares Item darf weder in der Historie noch als
                            // "erledigt" landen, sonst stuft ein späteres Seeding es
                            // fälschlich als "im Snapshot enthalten" ein und überspringt
                            // es dauerhaft (ursprüngliche Ursache des Script-Verlusts).
                            Freeze("Tabellen-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
                            return OperationResult.Failed(error);
                        }
                    }
                }
                _isInCache = endTimeUtc;
            } finally {
                ResumeDataReload();
                Interlocked.Decrement(ref _doingChanges);
                Column.GetSystems();
                DoWorkAfterLastChanges(myfiles, startTimeUtc);
                RepairAfterParse();
                TryToSetMeTemporaryMaster();
                OnInvalidateView();
                OnLoaded(false, affectingHead);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return InjectData(checkedDataFiles, data, startTimeUtc, endTimeUtc, initialload);
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Übernimmt den Zeitstempel des neuesten Snapshots vom Dateisystem.
    /// Erkennt damit auch Komplettierungen anderer Master.
    /// </summary>
    private void RefreshNewestSnapshotUtc() {
        if (GetSnapshotFilesNewestFirst().FirstOrDefault() is not { } newest) { return; }
        if (TableChunk.ExtractDateFromFileName(newest) is { } d) { _newestSnapshotUtc = d; }
    }

    /// <summary>
    /// Initialisiert den StreamWriter für die Fragmentdatei.
    /// </summary>
    private void StartWriter() {
        if (string.IsNullOrEmpty(WorkFolder())) {
            Freeze("Arbeitsverzeichnis nicht gesetzt. Stand: " + _isInCache.ToString5());
            return;
        }

        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }
        CheckPath();

        var tmp = IO.TempFile(WorkFolder(), KeyName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments);

        if (tmp.Contains("_000")) {
            Pause(1, false);
            Develop.AbortAppIfStackOverflow();
            StartWriter();
            return;
        }

        _myFragmentsFilename = tmp;

        if (Develop.AllReadOnly) { return; }

        System.IO.FileStream? fileStream = null;
        try {
            fileStream = new System.IO.FileStream(_myFragmentsFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            _writer = new System.IO.StreamWriter(fileStream, Encoding.UTF8);
            fileStream = null;

            _writer.AutoFlush = true;
            _writer.WriteLine("- Version " + TableVersion);
            _writer.WriteLine("- Filename " + Filename);
            _writer.WriteLine("- User " + UserName);

            var l = new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]");
            _writer.WriteLine(l.ParseableItems().FinishParseable());
            CanDeleteWriter = true;
            _writer.Flush();
        } catch {
            fileStream?.Dispose();
            _writer?.Dispose();
            _writer = null;

            Pause(3, false);
            Develop.AbortAppIfStackOverflow();
            StartWriter();
        }
    }

    /// <summary>
    /// Legt einen neuen Snapshot (Komplettstand) als neue Datei mit UTC-Zeitstempel
    /// im Namen an. Die bisherigen Dateien bleiben unangetastet — kein Tausch und
    /// keine Backup-Rotation. Schreibfehler (z.B. teilweiser Schreibvorgang) sind
    /// unkritisch: Loader überspringen Snapshots ohne gültigen EOF-Marker.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private string WriteNewSnapshot() {
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var x = LastChange;

        var content = GenerateFullContent(this);

        if (x != LastChange) { return "Tabelle wurde während der Speicherung geändert."; }

        // Die Mindestgrößen-Prüfung schützt vor korrupten Snapshots (z.B. wenn
        // durch einen Fehler kaum Daten generiert werden). Beim initialen Snapshot
        // einer neuen, leeren Tabelle ist der Inhalt legitim klein und darf
        // nicht zum Freeze führen — sonst wäre die neue Tabelle sofort blockiert.
        if (!InitialSavePending && content.Count < 1200) {
            Freeze("Datei zu klein für Speicherung");
            return "Datei zu klein für Speicherung.";
        }

        if (IO.CreateDirectory(WorkFolder()).IsFailed) { return "Verzeichnis konnte nicht erstellt werden."; }

        // Format wie bei TableChunk: UTC-Zeitstempel, Benutzer, Maschinen/Instanz-Hash.
        var path = WorkFolder() + $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}.{SuffixOfSnapshots}";

        // Snapshots werden gezippt gespeichert (wie Chunk-Dateien).
        var zipped = content.ToArray().ZipIt();
        if (zipped is null || zipped.Length == 0) {
            Freeze("Komprimierung fehlgeschlagen");
            return "Komprimierung fehlgeschlagen";
        }

        var wr = IO.WriteAllBytes(path, zipped);
        if (wr.IsFailed) {
            Freeze(wr.FailedReason);
            return wr.FailedReason;
        }

        _mySnapshotFile = path;
        _newestSnapshotUtc = DateTime.UtcNow;

        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Neue Komplett-Tabelle: " + path.FileNameWithSuffix(), 1);

        return string.Empty;
    }

    #endregion
}
