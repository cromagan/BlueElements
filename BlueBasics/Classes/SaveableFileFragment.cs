// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Datei-Verwaltung mit Kopf-Datei und Arbeitsunterordner: Die Kopf-Datei trägt
/// den Layout-Kopf, der Datenstand liegt im Unterordner gleichen Namens ohne
/// Suffix — als Zeitstempel-Snapshot (write-once) plus Fragment-Journal je
/// Benutzer. Jede Änderung wird als binäres Fragment (Navigationspfad + Blatt-Satz mit
/// Meta) in die eigene Journal-Datei geschrieben — eine bestehende Datei wird nie
/// überschrieben. Alle Benutzer laden die Journale der anderen nach; bei der
/// Komplettierung schreibt der Master einen neuen Snapshot und räumt alte Journale
/// und überzählige Snapshots auf. Journale werden generisch als
/// rohe Eintrags-Bytes (Basis-Tiefe 0) gelesen, über den Inhalts-Seam
/// (ParseMainContent) angewendet und mit dem Benutzer des Journal-Kopfes in
/// die Änderungs-Historie übernommen — das Layout kennt weder den Inhaltstyp
/// noch den Empfänger.
/// </summary>
public class SaveableFileFragment : SaveableFileOnefile {

    #region Layout

    /// <summary>
    /// ClassId des Fragment-Journal-Layouts; steht im Layout-Kopf der Kopf-Datei.
    /// </summary>
    public static new string ClassId => "Fragment";

    /// <summary>
    /// Die ClassId des konkreten Layouts.
    /// </summary>
    protected override string LayoutId => ClassId;

    /// <summary>
    /// Anzeigename des Fragment-Journal-Layouts für Auswahldialoge.
    /// </summary>
    public override string ReadableText() => "Fragment-Journal (Standard)";

    /// <summary>
    /// Kurzinfo des Fragment-Journal-Layouts für Auswahldialoge.
    /// </summary>
    public override string QuickInfo => "Jede Änderung wird einzeln in ein Benutzer-Journal geschrieben und regelmäßig in Snapshots gesichert.";

    #endregion

    #region Fields

    /// <summary>
    /// Wert in Sekunden. Nach dieser Zeit soll der FragmentAufräumer beendet werden.
    /// </summary>
    public static readonly int AbortFragmentDeletion = 10;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit dürfen Fragmente gelöscht werden.
    /// </summary>
    public static readonly int DeleteFragmentsAfter = 60 * 2 + 5 * 2;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen.
    /// </summary>
    public static readonly int DoComplete = 60;

    /// <summary>
    /// Wert in Stunden. Snapshots, die älter sind, werden gelöscht
    /// (sofern mindestens KeepSnapshots neuere existieren).
    /// </summary>
    public static readonly int DeleteSnapshotsAfterHours = 2;

    /// <summary>
    /// Anzahl der zu behaltenden Snapshots im Arbeitsverzeichnis.
    /// </summary>
    public const int KeepSnapshots = 3;

    /// <summary>
    /// Gibt das Suffix für Snapshot-Dateien (Komplettstände) zurück.
    /// </summary>
    public static readonly string SuffixOfSnapshots = "tbls";

    /// <summary>
    /// Gibt das Suffix für Fragmentdateien zurück.
    /// </summary>
    public static readonly string SuffixOfFragments = "tblf";

    /// <summary>
    /// Wert in Sekunden. Toleranz-Fenster zwischen dem Lesen der Journale und dem
    /// Speichern der Komplettierung: Einträge, die sicher vor der letzten
    /// Snapshot-Speicherung liegen, werden übersprungen.
    /// </summary>
    private const int JournalToleranceSeconds = 60;

    /// <summary>
    /// Zähler für aktive Änderungsprozesse zur Vermeidung von Race-Conditions.
    /// </summary>
    private int _doingChanges;

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    private DateTime _isInCache = new(0);

    private Stream? _writer;

    /// <summary>
    /// Puffer des aktuell aufgebauten Undo-Eintrags; er wird mit dem Blatt-Satz
    /// (Meta) komplett in die Journal-Datei geschrieben.
    /// </summary>
    private MemoryStream? _undoEntry;

    /// <summary>
    /// Tiefe des nächsten Undo-Satzes im aktuellen Eintrag.
    /// </summary>
    private int _undoEntryDepth;

    /// <summary>
    /// Der aktuell geladene Snapshot. Wird beim Aufräumen nie gelöscht.
    /// </summary>
    private string _mySnapshotFile = string.Empty;

    /// <summary>
    /// UTC-Zeitpunkt des neuesten bekannten Snapshots (aus dem Dateinamen).
    /// Maß für DoComplete und für die Fragment-Verwerfungs-Toleranz.
    /// </summary>
    private DateTime _newestSnapshotUtc = DateTime.MinValue;

    /// <summary>
    /// Die verbuchten Eintrags-Hashes; Basis für das Dedup beim Schreiben und
    /// Nachladen von Journal-Einträgen.
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _processedHashes = new();

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Fragment-Journal-Verwaltung für eine Head-Datei.
    /// </summary>
    /// <summary>
    /// Erstellt eine Layout-Instanz ohne Datei-Bezug für die Aufzählung der Datei-Layouts.
    /// </summary>
    public SaveableFileFragment() { }

    public SaveableFileFragment(string filename) : base(filename) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gibt an, ob die Fragmentdatei beim Schließen gelöscht werden darf (wenn keine wichtigen Änderungen enthalten sind).
    /// </summary>
    public bool CanDeleteWriter { get; private set; } = true;

    /// <summary>
    /// True, wenn die letzte Komplettierung zu lange zurückliegt.
    /// </summary>
    public override bool MasterNeeded { get; protected set; }

    /// <summary>
    /// True, wenn der geladene Datenstand höchstens eine Minute alt ist.
    /// </summary>
    public override bool DataFreshEnough => DateTime.UtcNow.Subtract(_isInCache).TotalMinutes <= 1;

    /// <summary>
    /// Multi-User-Zugriff ist über das Fragment-Journal möglich.
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

    /// <summary>
    /// Anzahl aktiver Änderungsprozesse; größer 0 bedeutet, dass gerade Daten
    /// aktualisiert werden und z. B. keine Ansichten gebaut werden sollten.
    /// </summary>
    protected int DoingChanges => _doingChanges;

    /// <summary>
    /// Die Dateien, deren Änderungen noch nicht in einem Snapshot enthalten sind.
    /// Ableitungen (z. B. weitere Journal-Quellen) pflegen die Liste mit.
    /// </summary>
    protected List<string> FilesNotIncluded { get; } = [];

    /// <summary>
    /// Dateiname der aktuell geführten Journal-Datei; leer, wenn kein Writer offen ist.
    /// </summary>
    protected string MyJournalFile { get; private set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Der Journal-Ordner ist das Arbeitsverzeichnis.
    /// </summary>
    public virtual string FragmengtsPath() => WorkFolder();

    /// <summary>
    /// Stellt sicher, dass die Daten aktuell sind, indem die Journale aller
    /// Benutzer nachgeladen werden.
    /// </summary>
    public override bool Refresh(bool firstTime) {
        if (!base.Refresh(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (firstTime) { _isInCache = LastSaveMainFileUtcDate; }

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Lade Fragmente von '" + KeyName + "'", 0); }

        // Zeitstempel VOR dem Lesen der Fragmente erfassen.
        // Wird als endTimeUtc an InjectData übergeben und dort als _isInCache gesetzt.
        // Konservativ: Fragmente, die während des Lesens hinzukommen, werden beim
        // nächsten Aufruf erneut gelesen - lieber doppelt als verloren.
        var readStartedUtc = DateTime.UtcNow;

        var fragmentFiles = IO.GetFiles(FragmengtsPath(), KeyName.ToUpperInvariant() + "-*." + SuffixOfFragments, SearchOption.TopDirectoryOnly).ToList();
        fragmentFiles.Remove(MyJournalFile);

        var undoRecords = GetLastChanges(fragmentFiles);
        if (undoRecords.IsFailed) { return false; }

        var opr = InjectData(fragmentFiles, undoRecords, DateTime.UtcNow, readStartedUtc, firstTime);
        return opr.IsSuccessful;
    }

    /// <summary>
    /// Bereitet die Fortführung einer sauber abgeschlossenen Fragment-Datei vor:
    /// Ordner- und Existenzprüfung, Writer-Übernahme und Kennzeichnung als eigene Datei.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    public string ContinueFragment(string fragmentFilename) {
        if (!fragmentFilename.StartsWith(FragmengtsPath(), StringComparison.OrdinalIgnoreCase)) { return "Die Datei liegt nicht im Fragment-Ordner: " + fragmentFilename; }
        if (!IO.FileExists(fragmentFilename)) { return "Fragment-Datei nicht gefunden: " + fragmentFilename; }

        // Ggf. bereits offenen Writer schließen (z. B. Master-Eintrag beim Laden):
        // Diese (fast leere) Neue-Datei darf das Aufräumen dann entsorgen.
        CloseWriter();

        // Ab sofort als eigene Datei führen: hält sie beim Nachladen aus den
        // Fremd-Fragmenten heraus und schützt sie vor Aufräum-Löschungen.
        MyJournalFile = fragmentFilename;
        CanDeleteWriter = false;

        // Satz 254 (ContinueEOF) anhängen: Die Datei endet nicht mehr auf EOF,
        // daher wählt sie kein zweiter Prozess zur Fortführung.
        var error = ContinueFragmentFile(fragmentFilename, out var writer);
        _writer = writer;
        return error;
    }

    /// <summary>
    /// Setzt den Freeze-Grund und schließt den Writer.
    /// </summary>
    public override void Freeze(string reason) {
        if (IsFreezed) { return; }

        base.Freeze(reason);

        CloseWriter();
    }

    /// <summary>
    /// Dateirelevante Erweiterung der Editierbarkeits-Prüfung: Journal-Ordner
    /// muss gesetzt sein und es darf kein kritischer Prozess laufen.
    /// </summary>
    public override string EditableError(bool isloading) {
        var f = base.EditableError(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (string.IsNullOrEmpty(FragmengtsPath())) { return "Fragmentpfad nicht gesetzt."; }

        if (DoingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    /// <summary>
    /// Lädt den neuesten vollständigen Snapshot.
    /// </summary>
    protected override bool LoadMainDataCore() {
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
            if (!FragmentRecord.HasValidEofMarker(content)) { continue; }

            // Binärer Snapshot (Fragment-Sätze): Blockweise über den Inhalts-Seam einlesen
            if (content is not { Length: > 0 } || content[0] != FragmentRecord.StandardStorageType) {
                Freeze("Snapshot '" + candidate.FileNameWithSuffix() + "' konnte nicht geparsest werden");
                return false;
            }

            using var ms = new MemoryStream(content);

            if (!ParseMainContent(ms)) {
                Freeze("Snapshot '" + candidate.FileNameWithSuffix() + "' konnte nicht geparsest werden");
                return false;
            }

            _mySnapshotFile = candidate;
            _newestSnapshotUtc = SaveableFileChunk.ExtractDateFromFileName(candidate) ?? DateTime.UtcNow;
            InitialSavePending = false; // vollständiger Stand aus dem Snapshot geladen
            return true;
        }

        if (snapshots.Count > 0) {
            Freeze("Kein lesbarer Snapshot vorhanden");
            return false;
        }

        // Kein Snapshot (z. B. abgebrochene Erstanlage): Als neue Daten starten.
        // Noch vorhandene Fragmente lädt Refresh nach; der nächste
        // Speichervorgang erzeugt den ersten Snapshot.
        InitialSavePending = true;
        return true;
    }

    /// <summary>
    /// Interner Speicheraufrruf: Bei der Erstanlage wird ein Snapshot erzeugt,
    /// danach werden nur die Journale geflusht — Änderungen landen als
    /// Fragmente, nie als Überschreiben einer Datei. Der Hauptdatei-Schreibpfad
    /// der Basisklasse (Onefile) wird bewusst nicht mehr benutzt.
    /// </summary>
    protected override string SaveInternal() {
        if (InitialSavePending) {
            EnsureHeadFileExists();

            var f = WriteNewSnapshot();
            if (!string.IsNullOrEmpty(f)) { return f; }

            InitialSavePending = false;
            SaveRequired = false;
            MainLoadDone = true; // der Snapshot entspricht jetzt dem Speicherstand
            return string.Empty;
        }

        if (_writer is null) {
            if (IsFreezed) { return FrozenReason; } // keinen Erfolg vortäuschen
            // Ohne offenen Writer existieren keine eigenen Journal-Einträge —
            // ein frisch geladener, unveränderter Stand hat nichts zu speichern.
            SaveRequired = false;
            return string.Empty;
        }

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
    /// Nimmt einen Undo-Satz der Stufen-Kette entgegen und baut daraus den
    /// Journal-Eintrag: Die Sätze landen im Eintrags-Puffer und in der
    /// Änderungs-Historie; mit dem Meta-Satz des Blattes ist der Eintrag komplett
    /// und wird in einem Stück in die Journal-Datei geschrieben und als verarbeitet
    /// verbucht. Beim Laden, beim Einspielen fremder Fragmente und im
    /// Nur-Lese-Modus still.
    /// </summary>
    public override void LogUndo(FragmentRecord record) {
        if (IsDisposed || Ending) { return; }
        if (Develop.AllReadOnly) { return; }
        if (InitialSavePending) { return; }
        if (_doingChanges > 0) { return; }
        if (record is null) { return; }

        if (_writer is null) { StartWriter(); }
        if (_writer is null) { return; }

        try {
            lock (_writer) {
                _undoEntry ??= new MemoryStream();

                if (!record.Append(_undoEntry, _undoEntryDepth)) {
                    Freeze("Undo-Satz ungültig oder zu groß: Typ " + record.ValueId);
                    return;
                }

                CanDeleteWriter = false;

                lock (UndoLock) {
                    Undos.Add(record.WithDepth(_undoEntryDepth));
                }

                _undoEntryDepth++;

                if (!record.HasMeta) { return; } // Eintrag noch nicht komplett

                var entryBytes = _undoEntry.ToArray();
                _writer.Write(entryBytes, 0, entryBytes.Length);
                _writer.Flush();

                // Eigene Änderung als verarbeitet verbuchen: Der gleiche Eintrag
                // wird beim Nachladen fremder Journale nicht erneut angewendet.
                _ = MarkProcessed(EntryHash(entryBytes));

                _undoEntry.SetLength(0);
                _undoEntryDepth = 0;
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }
    }

    /// <summary>
    /// True, wenn das Anwenden eines Journal-Eintrags Kopf-/Struktur-Daten
    /// berührt. Basis: immer — der Empfänger reagiert konservativ.
    /// </summary>
    // TODO: Unused
    protected virtual bool EntryAffectsHead(byte[] entry) => true;

    /// <summary>
    /// Schreibt die Kopf-Sätze einer neuen Fragment-Datei. Basis: Version,
    /// Inhaltstyp, Erzeuger, Speicherer und Rechner.
    /// </summary>
    protected virtual void WriteJournalStartRecords(Stream writer) {
        var nowUtc = DateTime.UtcNow;

        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SerializerVersion, "1.00");
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.Type, Owner.TypeName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.CreateUser, UserName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.CreateTimeUtc, nowUtc);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SaveUser, UserName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.Machine, Environment.MachineName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SaveTimeUtc, nowUtc);
    }

    /// <summary>
    /// Gibt die Ressourcen frei. CloseWriter läuft nach dem Dispose, damit
    /// die von UnMasterMe geschriebenen Sätze in dieselbe, sauber mit EOF abgeschlossene
    /// Fragment-Datei landen und kein Stream-Handle offen bleibt.
    /// </summary>
    protected override void DisposeContent() {
        base.DisposeContent();
        CloseWriter();
    }

    /// <summary>
    /// Empfänger-seitige Prüfung vor dem Einspielen von Journal-Daten.
    /// </summary>
    /// <param name="initialload">True während des ersten Ladevorgangs.</param>
    /// <returns>Fehlermeldung oder leer.</returns>
    protected virtual string CheckInjectAllowed(bool initialload) => EditableError(initialload);

    /// <summary>
    /// Markiert einen Eintrags-Hash atomar als verbucht. Liefert false bei Duplikat.
    /// </summary>
    private bool MarkProcessed(string hash) => _processedHashes.TryAdd(hash, default);

    /// <summary>
    /// Liefert den Dedup-Hash eines Journal-Eintrags über seine kanonischen Bytes.
    /// </summary>
    private static string EntryHash(byte[] entry) => Convert.ToBase64String(entry).GetMD5Hash();

    /// <summary>
    /// Überprüft und erstellt den Journal-Ordner.
    /// </summary>
    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        IO.CreateDirectory(FragmengtsPath());
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Journal-Datei, falls sie keine
    /// permanenten Daten enthält. Abschließend wird der EOF-Satz geschrieben.
    /// </summary>
    protected void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose is not null) {
            _writer = null;

            try {
                _ = FragmentRecord.Append(writerToClose, 0, FragmentTagType.Eof, string.Empty);
                writerToClose.Flush();
            } catch { } finally {
                try {
                    writerToClose.Dispose();
                } catch { }
            }

            if (CanDeleteWriter) {
                IO.DeleteFile(MyJournalFile, false);
            }
        }
    }

    /// <summary>
    /// Führt Aufräumarbeiten nach dem Laden von Fragmenten durch,
    /// inkl. Komplettierung und Bereinigung alter Dateien.
    /// </summary>
    /// <param name="fragmentFiles">Die im selben Lesezyklus ermittelten Fragment-Dateien.</param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    private void DoWorkAfterLastChanges(List<string> fragmentFiles, DateTime startTimeUtc) {
        if (Ending) { return; }
        if (!string.IsNullOrEmpty(EditableError(false))) { return; }

        // Auch fremde Komplettierungen erkennen: Neuesten Snapshot-Stand übernehmen.
        RefreshNewestSnapshotUtc();

        MasterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > DoComplete;

        // Alte Snapshots räumen immer auf — unabhängig davon, ob fremde
        // Fragmente gelesen wurden.
        CleanupOldSnapshots(startTimeUtc);

        if (fragmentFiles is { Count: >= 1 } && MasterNeeded && AmITemporaryMaster(true)) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Erstelle neuen Komplettstand: " + KeyName, 0);

            var f = WriteNewSnapshot();

            if (string.IsNullOrEmpty(f)) {
                CloseWriter();
                StartWriter();

                MasterMe();

                MasterNeeded = false;
                OnInvalidateView();
                FilesNotIncluded.Clear();
            } else {
                Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {f}", 0);
            }
        }

        if (fragmentFiles is not { Count: >= 1 }) { return; }

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien schützen, deren Änderungen noch nicht in einem Snapshot stehen

        // Zusätzliche Safety zur Zeitstempel-Prüfung unten: Dateien, die in diesem
        // Durchlauf gelesen wurden und deren Items noch nicht in einem Snapshot
        // stehen (FilesNotIncluded), werden explizit aus der Lösch-Liste entfernt.
        // Nach einer erfolgreichen Komplettierung ist die Liste geleert -
        // dann greift ausschließlich die Zeitstempel-Prüfung.
        if (FilesNotIncluded.Count != 0) {
            foreach (var thisch in FilesNotIncluded) {
                fragmentFiles.Remove(thisch);
            }
        }

        #endregion

        fragmentFiles.Shuffle();

        foreach (var thisf in fragmentFiles) {
            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f[^19..];

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalMinutes > DeleteFragmentsAfter &&
                         LastSaveMainFileUtcDate.Subtract(d2).TotalMinutes > DeleteFragmentsAfter) {
                        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix(), 0);
                        IO.DeleteFile(thisf, 0, false);
                        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ermittelt die neuesten Änderungen aus den Journal-Dateien: Jede Datei wird
    /// generisch in Eintrags-Bytes (Basis-Tiefe 0) zerlegt und als flache Liste der
    /// Undo-Sätze geliefert; diese tragen ihre Eintrags-Daten (Entry, Container,
    /// EntryDateTimeUtc) als Felder. Einträge, die sicher vor der letzten
    /// Snapshot-Speicherung liegen, und bereits verbuchte Einträge werden übersprungen.
    /// </summary>
    /// <param name="fragmentFiles">Die zuvor mit GetFragmentFiles ermittelten Journal-Dateien.</param>
    private OperationResult GetLastChanges(List<string> fragmentFiles) {
        if (EditableError(true) is { Length: > 0 } f) { return OperationResult.Failed(f); }

        CheckPath();

        try {
            List<FragmentRecord> undoRecords = [];

            foreach (var thisf in fragmentFiles) {
                ReadJournalFile(thisf, undoRecords);
            }

            return new OperationResult(undoRecords);
        } catch { }
        return OperationResult.Failed("Fehler beim Lesen der Fragment-Journale.");
    }

    /// <summary>
    /// Zerlegt eine Journal-Datei in Einträge: Ein Eintrag beginnt an einem
    /// Daten-Satz der Tiefe 0 und umfasst alle tieferen Sätze; der Zeitstempel
    /// kommt aus dem Meta des ersten Satzes mit Meta. Kopf- und Steuer-Sätze der
    /// Datei-Ebene der Tiefe 0 und Müll-Zeilen werden übersprungen, Einträge ohne Meta verworfen. Die
    /// Meta trägt den Benutzer nicht mehr; er kommt aus dem Kopf-Satz des
    /// Journals und wird den Undo-Sätzen des Eintrags injiziert. Jeder Undo-Satz
    /// erhält die Eintrags-Daten (Entry, Container, EntryDateTimeUtc) als Felder.
    /// </summary>
    private void ReadJournalFile(string container, List<FragmentRecord> undoRecords) {
        if (IO.ReadAllBytes(container, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) { return; }

        byte[] data;
        if (rawBytes.IsZipped()) {
            data = rawBytes.UnzipIt() ?? [];
            if (data.Length == 0) { return; }
        } else {
            data = rawBytes;
        }

        var headUser = string.Empty;

        var pos = 0;

        while (pos < data.Length) {
            // Müll-Zeilen (z. B. unvollständiger Schreibvorgang): bis zum Zeilenende überspringen;
            // jeder Satz beginnt mit dem SpeicherTyp
            if (data[pos] != FragmentRecord.StandardStorageType) {
                var lineEnd = Array.IndexOf(data, (byte)'\n', pos);
                pos = lineEnd < 0 ? data.Length : lineEnd + 1;
                continue;
            }

            if (FragmentRecord.TryParse(data, pos, out var first, out var afterFirst) && first is not null) {
                if (first.Depth > 0 || first.IsFileTag) {
                    // Kopf-Benutzer merken; Blockreste ohne Eintrag bzw. übrige Kopf-Sätze überspringen
                    if (first.IsFileTag && first.TagType == FragmentTagType.SaveUser) { headUser = first.StringContent; }

                    pos = afterFirst;
                    continue;
                }

                // Die Eintrags-Sätze tragen den Journal-Eigentümer und werden als Undo-Sätze übernommen.
                List<FragmentRecord> entryRecords = [first.WithUser(headUser)];

                var entryStart = pos;
                var entryEnd = afterFirst;
                pos = afterFirst;

                var dateTimeUtc = first.HasMeta ? first.DateTimeUtc : DateTime.MinValue;
                var hasMeta = first.HasMeta;

                // Alle tieferen Sätze gehören zum Eintrag; der Meta-Stempel kommt
                // aus dem ersten Blatt-Satz mit Meta (Navigationssätze tragen keine).
                while (pos < data.Length && data[pos] == FragmentRecord.StandardStorageType) {
                    if (!FragmentRecord.TryParse(data, pos, out var inner, out var innerNext) || inner is null) { break; }

                    if (inner.Depth <= 0) { break; }

                    pos = innerNext;
                    entryEnd = innerNext;

                    if (!hasMeta && !inner.IsFileTag && inner.HasMeta) {
                        hasMeta = true;
                        dateTimeUtc = inner.DateTimeUtc;
                    }

                    if (!inner.IsFileTag) { entryRecords.Add(inner.WithUser(headUser)); }
                }

                pos = entryEnd;

                if (!hasMeta) { continue; }

                // Einträge, die sicher vor der letzten Snapshot-Speicherung lagen,
                // sind im Komplett-Stand enthalten und werden übersprungen. Die
                // Toleranz überbrückt das Zeitfenster zwischen dem Lesen der
                // Journale und dem Speichern der Komplettierung.
                if (dateTimeUtc.AddSeconds(JournalToleranceSeconds) < LastSaveMainFileUtcDate) { continue; }

                var entry = data[entryStart..entryEnd];

                // Atomar als verarbeitet markieren; bei Duplikat sofort überspringen.
                if (!MarkProcessed(EntryHash(entry))) { continue; }

                foreach (var thisRecord in entryRecords) {
                    undoRecords.Add(thisRecord.WithEntry(entry, container, dateTimeUtc));
                }
            } else {
                // Unlesbarer Satz: Rest der Datei verwerfen.
                pos = data.Length;
            }
        }
    }

    /// <summary>
    /// Injiziert die geladenen Journal-Einträge in die aktuelle Struktur: Die
    /// Undo-Sätze werden nach Eintrags-Zeitstempel sortiert; jeder Eintrag (gleiche
    /// Entry-Referenz) wird einmalig über den Inhalts-Seam angewendet und seine
    /// Sätze (mit injiziertem Benutzer) in die Änderungs-Historie übernommen.
    /// </summary>
    /// <param name="fragmentFiles">Die im selben Lesezyklus ermittelten Fragment-Dateien.</param>
    /// <param name="undoRecords">Die Undo-Sätze aus GetLastChanges.</param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    /// <param name="initialload"></param>
    private OperationResult InjectData(List<string> fragmentFiles, OperationResult undoRecords, DateTime startTimeUtc, DateTime endTimeUtc, bool initialload) {
        if (undoRecords.Value is not List<FragmentRecord> data) { return OperationResult.Success; }

        // Bei initialload die MainLoadDone-Sperre umgehen - das Objekt
        // befindet sich gerade im ersten Ladevorgang (EditableError prüft
        // mit isloading=true). Freeze- und Prozess-Prüfungen bleiben aktiv.
        if (CheckInjectAllowed(initialload) is { Length: > 0 } f) { return OperationResult.Failed(f); }

        var dataSorted = data.OrderBy(obj => obj.EntryDateTimeUtc);

        try {
            Interlocked.Increment(ref _doingChanges);
            // Während des Einspielens sind Schlüssel kurzzeitig nicht
            // auflösbar. In diesem Fenster dürfen Ansichten keine Einträge
            // endgültig entfernen (RepairArrangements), sonst verschwinden
            // Bereiche dauerhaft aus der Ansicht.
            PauseDataReload();
            try {
                // Die Sätze eines Journal-Eintrags tragen dieselbe Entry-Referenz und
                // stehen nach dem Sortieren direkt hintereinander; der Eintrag wird
                // einmalig angewendet, seine Sätze sammeln sich bis zum nächsten Eintrag.
                var pendingRecords = new List<FragmentRecord>();
                byte[]? appliedEntry = null;

                foreach (var thisWork in dataSorted) {
                    if (ReferenceEquals(appliedEntry, thisWork.Entry)) {
                        pendingRecords.Add(thisWork);
                        continue;
                    }

                    // Der vorherige Eintrag ist vollständig angewendet; seine
                    // Undo-Sätze in die Änderungs-Historie übernehmen.
                    if (appliedEntry is not null) {
                        lock (UndoLock) {
                            Undos.AddRange(pendingRecords);
                        }

                        pendingRecords.Clear();
                    }

                    appliedEntry = thisWork.Entry;
                    FilesNotIncluded.Add(thisWork.Container);

                    using var ms = new MemoryStream(thisWork.Entry);

                    bool parsed;

                    try {
                        parsed = ParseMainContent(ms);
                    } catch (Exception ex) {
                        // Ein kaputter Eintrag darf den Prozess nicht beenden:
                        // als Freeze mit Ursache quittieren und den Rest verwerfen.
                        Freeze($"Fehler beim Übernehmen: {thisWork.EntryDateTimeUtc.ToString9()} aus {thisWork.Container.FileNameWithSuffix()} — {ex.Message} | {ex.StackTrace}");
                        return OperationResult.Failed("Journal-Eintrag konnte nicht angewendet werden");
                    }

                    if (!parsed) {
                        // Bewusst NICHT in der Historie/_filesNotIncluded aufnehmen: Ein
                        // nicht anwendbares Item darf weder in der Historie noch als
                        // "erledigt" landen, sonst stuft ein späteres Seeding es
                        // fälschlich als "im Hauptfile enthalten" ein und überspringt
                        // es dauerhaft.
                        var detail = string.Empty;
                        var scanPos = 0;

                        while (FragmentRecord.TryParse(thisWork.Entry, scanPos, out var failRecord, out scanPos) && failRecord is not null && detail.Length < 160) {
                            detail += " [Typ " + failRecord.LeafType + ", T" + failRecord.Depth + ", '" + failRecord.StringContent.Left(20) + "']";
                        }

                        Freeze($"Fehler beim Übernehmen: {thisWork.EntryDateTimeUtc.ToString9()} aus {thisWork.Container.FileNameWithSuffix()}{detail}");
                        return OperationResult.Failed("Journal-Eintrag konnte nicht angewendet werden");
                    }

                    pendingRecords.Add(thisWork);
                }

                // Undo-Sätze des letzten Eintrags übernehmen (LogUndo bleibt beim
                // Einspielen still).
                if (appliedEntry is not null) {
                    lock (UndoLock) {
                        Undos.AddRange(pendingRecords);
                    }
                }

                _isInCache = endTimeUtc;
            } finally {
                ResumeDataReload();
                Interlocked.Decrement(ref _doingChanges);

                if (!IsDisposed) {
                    OnRefreshDataSystems();
                    DoWorkAfterLastChanges(fragmentFiles, startTimeUtc);
                    TryToSetMeTemporaryMaster();
                    OnInvalidateView();
                    // Loaded zuletzt: Die Nacharbeiten (RepairAfterParse) des
                    // besitzenden Objekts laufen in dessen Loaded-Behandlung.
                    OnLoaded(false);
                }
            }
        } catch (Exception ex) {
            // Kein Wiederholen per Rekursion: Ein bleibender Fehler würde den Stack
            // aufbrauchen (Stack-Overflow-Meldung und Stillstand). Der nächste
            // BeSureToBeUpToDate-Aufruf versucht das Einspielen erneut.
            Develop.DebugPrint(ErrorType.Warning, "InjectData fehlgeschlagen: " + KeyName, ex);
            return OperationResult.Failed("Fehler beim Einspielen der Fragmente: " + ex.Message);
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Initialisiert den Stream für die Journal-Datei und schreibt die Kopf-Sätze.
    /// </summary>
    protected void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + _isInCache.ToString5());
            return;
        }

        // Begrenzte Wiederholungen statt Rekursion: Kollisionen werden mit neuem
        // Sekunden-Stempel erneut versucht; ein bleibender Fehler Freezt das Objekt,
        // statt den Stack aufzubrauchen.
        for (var versuch = 1; versuch <= 5; versuch++) {
            if (!string.IsNullOrEmpty(EditableError(false))) { return; }
            CheckPath();

            // Der Benutzer gehört in den Namen: Jeder Benutzer führt sein eigenes
            // Journal, und mehrere Prozesse auf derselben Maschine kollidieren nicht
            // mehr. CreateNew entscheidet atomar, wem die Datei gehört; der Verlierer
            // probiert die nächste Sekunde.
            var wunschName = (KeyName + "-" + UserName + "-" + Environment.MachineName).ReduceToChars(Char_Numerals + " _+-#" + Char_Buchstaben + Char_Buchstaben.ToUpperInvariant());

            if (wunschName.Length > 60) { wunschName = wunschName[..60]; } // Platz für Zeitstempel lassen

            MyJournalFile = FragmengtsPath().NormalizePath() + wunschName + "-" + DateTime.UtcNow.ToString4() + "." + SuffixOfFragments;

            // Ein Zähler-Anhang (_000xx) macht den Namen unbrauchbar: Die Aufräumarbeit
            // liest den Zeitstempel aus den letzten 19 Zeichen des Namens.
            if (MyJournalFile.FileNameWithSuffix().Contains("_000")) {
                Freeze("Journaldatei konnte nicht eindeutig benannt werden: " + MyJournalFile.FileNameWithSuffix());
                return;
            }

            if (Develop.AllReadOnly) { return; }

            FileStream? fileStream = null;
            try {
                fileStream = new FileStream(MyJournalFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                _writer = fileStream;
                fileStream = null;

                WriteJournalStartRecords(_writer);

                CanDeleteWriter = true;
                _writer.Flush();
                return;
            } catch (Exception ex) {
                fileStream?.Dispose();
                _writer?.Dispose();
                _writer = null;

                Develop.DebugPrint(ErrorType.Warning, "Journaldatei nicht offen (Versuch " + versuch.ToString1() + "): " + MyJournalFile.FileNameWithSuffix(), ex);
                Pause(1, false);
            }
        }

        Freeze("Journaldatei konnte nicht geöffnet werden: " + MyJournalFile);
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
                .OrderByDescending(f => SaveableFileChunk.ExtractDateFromFileName(f) ?? DateTime.MinValue)
                .ToList();
        } catch {
            return [];
        }
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

            var fileDate = SaveableFileChunk.ExtractDateFromFileName(thisf);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Räume alten Snapshot auf: " + thisf.FileNameWithSuffix(), 0);
                try { IO.DeleteFile(thisf, false); } catch { }
            }

            if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
        }
    }

    /// <summary>
    /// Übernimmt den Zeitstempel des neuesten Snapshots vom Dateisystem.
    /// Erkennt damit auch Komplettierungen anderer Master.
    /// </summary>
    private void RefreshNewestSnapshotUtc() {
        if (GetSnapshotFilesNewestFirst().FirstOrDefault() is not { } newest) { return; }
        if (SaveableFileChunk.ExtractDateFromFileName(newest) is { } d) { _newestSnapshotUtc = d; }
    }

    /// <summary>
    /// Legt einen neuen Snapshot (Komplettstand) als neue Datei mit UTC-Zeitstempel
    /// im Namen an. Der Inhalt kommt aus dem binären Fragment-Format
    /// (inkl. Änderungs-Historie). Die bisherigen Dateien bleiben unangetastet — kein
    /// Tausch und keine Backup-Rotation. Schreibfehler (z. B. teilweiser
    /// Schreibvorgang) sind unkritisch: Loader überspringen Snapshots ohne gültigen
    /// EOF-Marker. Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private string WriteNewSnapshot() {
        var f = EditableError(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var x = LastChangeUtc;

        var content = GenerateContentToSave();

        if (x != LastChangeUtc) { return "Daten wurden während der Speicherung geändert."; }

        if (content.Count == 0) { return "Snapshot konnte nicht erzeugt werden."; }

        // Die Mindestgrößen-Prüfung schützt vor korrupten Snapshots. Beim initialen
        // Snapshot ist der Inhalt legitim klein und darf
        // nicht zum Freeze führen — sonst wäre das neue Objekt sofort blockiert.
        if (!InitialSavePending && content.Count < MinimumContentSize) {
            Freeze("Datei zu klein für Speicherung");
            return "Datei zu klein für Speicherung.";
        }

        if (IO.CreateDirectory(WorkFolder()).IsFailed) { return "Verzeichnis konnte nicht erstellt werden."; }

        // UTC-Zeitstempel, Benutzer, Maschinen/Instanz-Hash.
        var path = WorkFolder() + $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}.{SuffixOfSnapshots}";

        // Snapshots werden gezippt gespeichert.
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

        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Neuer Komplettstand: " + path.FileNameWithSuffix(), 1);

        return string.Empty;
    }

    #endregion

    #region Fragment-Journal (statisch)

    /// <summary>
    /// Öffnet die Fragment-Datei im Append-Modus und hängt sofort einen 254-Satz
    /// (ContinueEOF) an, damit kein zweiter Prozess Schreibzugriff auf dieselbe Datei erhält.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private static string ContinueFragmentFile(string fragmentFilename, out Stream? writer) {
        writer = null;
        if (Develop.AllReadOnly) { return string.Empty; }

        FileStream? fileStream = null;
        try {
            fileStream = new FileStream(fragmentFilename, FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = fileStream;
            fileStream = null;

            _ = FragmentRecord.Append(writer, 0, FragmentTagType.ContinueEof, string.Empty);
            writer.Flush();

            return string.Empty;
        } catch (Exception ex) {
            fileStream?.Dispose();
            writer?.Dispose();
            writer = null;

            return ex.Message;
        }
    }

    #endregion
}