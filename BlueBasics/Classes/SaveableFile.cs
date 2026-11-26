// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBasics.Classes;

/// <summary>
/// Datei-Verwaltung für das Fragment-Format: Die Kopf-Datei trägt die Verwaltung
/// selbst als Fragment-Baum mit der Layout-ClassId im Blatt-Satz. Die Daten liegen
/// im Arbeitsunterordner gleichen Namens ohne Suffix: Der Kopf-Stand als
/// Zeitstempel-Snapshot (write-once) plus Fragment-Journal je Benutzer, die Daten
/// mit Chunk-Wert zusätzlich als unveränderliche Chunk-Dateien je Chunk-Ordner.
/// Jede Änderung wird als binäres Fragment (Navigationspfad + Blatt-Satz mit Meta)
/// geschrieben — eine bestehende Datei wird nie überschrieben. Alle Benutzer laden
/// die Journale der anderen nach; bei der Komplettierung schreibt der Master einen
/// neuen Snapshot und räumt alte Journale und überzählige Snapshots auf. Ein
/// statischer Watcher tickt alle Instanzen periodisch und antreibt deren
/// CheckerTick (Nachladen und bedarfsgesteuertes Speichern).
/// <para>
/// Die Chunk-Erkennung läuft über Areas: genau EIN Wert — der Chunk-Pfad. Trifft
/// der Empfänger-Typen-Pfad eines Eintrags diesen Pfad und liefert der
/// ItemPath-Item-Kenner einen Chunk-Wert (IHasChunkValue), gehört der Eintrag in
/// den Chunk des Werts; sonst ist er ein Kopf-Fragment. Ohne Chunk-Pfad ist
/// jeder Eintrag ein Kopf-Fragment — der gesamte Inhalt wird im Kopf geführt.
/// </para>
/// </summary>
public class SaveableFile : IDisposableExtended, IReadableText, IHasQuickInfo, IHasKeyName, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Empfänger-Typen der Kopf-Datei im Fragment-Format; nur der Verwaltung selbst bekannt.
    /// Gültige Werte: 1–199 (0 ist reserviert, ≥ 200 sind systemeigene Tags).
    /// </summary>
    public enum SaveableFileFragmentType : byte {
        LayoutClassId = 1
    }

    #endregion

    #region Layout

    /// <summary>
    /// ClassId des Datei-Layouts; steht im Layout-Kopf der Kopf-Datei und bestimmt
    /// beim Laden die erzeugte Verwaltung.
    /// </summary>
    public static string ClassId => "ChunkFragment";

    /// <summary>
    /// Die ClassId des Layouts als Instanz-Zugriff.
    /// </summary>
    public string MyClassId => ClassId;

    /// <summary>
    /// Anzeigename des Datei-Layouts für Auswahldialoge.
    /// </summary>
    public string ReadableText() => "Chunk-Fragment-Journal";

    /// <summary>
    /// Symbol zum Anzeigename; die Verwaltung hat kein eigenes Symbol.
    /// </summary>
    public QuickImage? SymbolForReadableText() => null;

    /// <summary>
    /// Kurzinfo des Datei-Layouts für Auswahldialoge.
    /// </summary>
    public string QuickInfo => "Kopf-Fragmente als Journal mit Komplettierung; Daten mit Chunk-Wert liegen als unveränderliche Chunk-Dateien im Arbeitsunterordner und werden bei Bedarf geladen.";

    /// <summary>
    /// Wert in Sekunden. Eine Chunk-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public static int EditLockSeconds { get; set; } = 300;

    #endregion

    #region Fields

    /// <summary>
    /// Alle Datei-Verwaltungen über alle geladenen Assemblies; Instanzen ohne
    /// Datei-Bezug, erzeugt über den parameterlosen Konstruktor.
    /// </summary>
    public static readonly AssemblyAwareCache<SaveableFile> AllFileSaveTypes = new();

    /// <summary>
    /// Wert in Stunden. Chunk-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens ChunkFileCount neuere Dateien im gleichen Ordner existieren).
    /// </summary>
    private const int DeleteOldChunkFilesAfterHours = 2;

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner.
    /// </summary>
    private const int ChunkFileCount = 5;

    /// <summary>
    /// Suffix der Chunk-Dateien.
    /// </summary>
    private const string SuffixOfChunks = "chk";

    /// <summary>
    /// Wert in Sekunden. Nach dieser Zeit soll der Fragment-Aufräumer beendet werden.
    /// </summary>
    private const int AbortFragmentDeletion = 10;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit dürfen Fragmente gelöscht werden.
    /// </summary>
    private const int DeleteFragmentsAfter = 60 * 2 + 5 * 2;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen.
    /// </summary>
    private const int DoComplete = 60;

    /// <summary>
    /// Wert in Stunden. Snapshots, die älter sind, werden gelöscht
    /// (sofern mindestens KeepSnapshots neuere existieren).
    /// </summary>
    private const int DeleteSnapshotsAfterHours = 2;

    /// <summary>
    /// Anzahl der Snapshots im Arbeitsverzeichnis, die immer behalten werden,
    /// unabhängig von ihrem Alter.
    /// </summary>
    private const int KeepSnapshots = 5;

    /// <summary>
    /// Suffix der Snapshot-Dateien (Komplettstände).
    /// </summary>
    private const string SuffixOfSnapshots = "snp";

    /// <summary>
    /// Suffix der Fragment-Dateien (Journale).
    /// </summary>
    public const string SuffixOfFragments = "frg";

    /// <summary>
    /// Wert in Sekunden. Toleranz-Fenster zwischen dem Lesen der Journale und dem
    /// Speichern der Komplettierung: Einträge, die sicher vor der letzten
    /// Snapshot-Speicherung liegen, werden übersprungen.
    /// </summary>
    private const int JournalToleranceSeconds = 60;

    /// <summary>
    /// Wert in Minuten. Ein fremder Meister-Anspruch im Journal gilt als aktiv,
    /// solange er jünger ist; danach darf eine andere Instanz übernehmen —
    /// Ablauf unterhalb der DoComplete-Kadenz.
    /// </summary>
    private const int MasterClaimMinutes = 55;

    /// <summary>
    /// Mindestgröße des Kopf-Snapshot-Inhalts; schützt vor korrupten Re-Saves.
    /// Beim initialen Speichern ist der Inhalt legitim klein.
    /// </summary>
    private const int MinimumContentSize = 200;

    /// <summary>
    /// Zuletzt vergebener Zeitstempel für Undo-Einträge; garantiert eindeutigen Millisekunden-Abstand.
    /// </summary>
    private DateTime _lastUndoStampUtc = DateTime.MinValue;

    /// <summary>
    /// Interne Reentrancy-Zähler für Lade- und Speichervorgänge; der
    /// statische Watcher überspringt Instanzen mit Zähler &gt; 0.
    /// </summary>
    private int _timerPaused;

    /// <summary>
    /// Benutzer, der die Daten erzeugt hat; aus dem Kopf der geladenen Datei erhalten.
    /// </summary>
    private string _createUser = string.Empty;

    /// <summary>
    /// UTC-Zeitpunkt der Erzeugung der Daten; aus dem Kopf der geladenen Datei erhalten.
    /// </summary>
    private DateTime _createTimeUtc = DateTime.MinValue;

    /// <summary>
    /// Dateinamen (normiert, upper) der Ladevorgänge, die aktuell laufen;
    /// die Reservierungen gelten pro Datei global, nicht pro Instanz.
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte> FilesInLoad = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Alle lebenden Instanzen; der statische Watcher tickt sie periodisch.
    /// </summary>
    private static readonly ConcurrentDictionary<SaveableFile, byte> Instances = new();

    /// <summary>
    /// Statischer Watcher (2-Sekunden-Takt), der CheckerTick aller Instanzen antreibt.
    /// </summary>
    private static Timer? _watcherTimer;

    /// <summary>
    /// Flag gegen doppeltes Dispose.
    /// </summary>
    private int _isDisposedFlag;

    /// <summary>
    /// Zähler für kritische Daten-Einspielungen; bei &gt; 0 sollen Ansichten
    /// keine endgültigen Aufräumaktionen durchführen.
    /// </summary>
    private int _dataReloadPaused;

    /// <summary>
    /// chunkId (lowercase) → SHA256-Hash des kompletten Chunk-Inhalts.
    /// Verhindert unnötiges Schreiben bei unverändertem Inhalt (Write-once: keine
    /// identischen Duplikate erzeugen).
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _lastContentHash = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → UTC-Zeitpunkt des letzten Zugriffs (Laden, Speichern,
    /// Refresh, Anzeige via TouchChunk). Wird genutzt, um ungenutzte Chunks bei
    /// Refresh zu überspringen.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → Dateiname der zuletzt geparten oder gespeicherten Chunk-Datei.
    /// Chunks werden nach dem Parsen verworfen, nur der Dateiname wird für
    /// Aktualitätsprüfungen vorgehalten.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _processedFile = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reentrancy-Sperre für den Chunk-Refresh: 0 = frei, 1 = läuft.
    /// </summary>
    private int _isRefreshingChunks;

    /// <summary>
    /// Serialisiert Save, Reorganize und Komplettierung je Instanz: Nur einer
    /// dieser datenverändernden Vorgänge läuft gleichzeitig. Nicht blockierend
    /// genutzt (Wait(0)) — Kollisionen werden übersprungen und später nachgeholt.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    /// <summary>
    /// Hintergrund-Task des letzten Save-Aufrufs; Grundlage für IsSaving und
    /// das Warten beim Beenden.
    /// </summary>
    private Task? _saveTask;

    /// <summary>
    /// Sperre um _saveTask: dedizierte Task-Erzeugung in Save und Warten über
    /// WaitDiskOperationFinished.
    /// </summary>
    private readonly object _saveLock = new();

    /// <summary>
    /// Serialisiert Journal-Schreibzugriffe (UnMasterMe, WriteMasterClaim,
    /// LogUndoEntry, Flush) mit Writer-Wechseln (CloseWriter, StartWriter,
    /// ContinueFragment).
    /// </summary>
    private readonly object _writerLock = new();

    /// <summary>
    /// Thread-Id des aktiven Hintergrund-Speicher-Tasks; -1, wenn keiner läuft.
    /// Nur der speichernde Thread selbst darf während des Speicherns eigene
    /// Editier-Prüfungen passieren (PrepareNewSnapshot, StartWriter).
    /// </summary>
    private volatile int _savingThreadId = -1;

    /// <summary>
    /// Zähler für aktive Änderungsprozesse zur Vermeidung von Race-Conditions.
    /// </summary>
    private int _doingChanges;

    /// <summary>
    /// Letzter Lade-Stand der Kopf-Daten.
    /// </summary>
    private DateTime _isInCache = new(0);

    /// <summary>
    /// UTC-Zeitpunkt des Kopf-Snapshots, auf dem der eigene Speicherstand basiert.
    /// Wurde seitdem ein fremder Snapshot geschrieben, fehlen dem Speicher dessen
    /// absorbierte Journal-Einträge — eine eigene Komplettierung wäre ein Zurückrollen.
    /// </summary>
    private DateTime _loadedSnapshotUtcDate = new(0);

    /// <summary>
    /// Zuletzt gesehener Meister-Anspruchs-Benutzer aus den Journalen.
    /// </summary>
    private string _foreignMasterUser = string.Empty;

    /// <summary>
    /// Zuletzt gesehener Meister-Anspruchs-Rechner aus den Journalen.
    /// </summary>
    private string _foreignMasterMachine = string.Empty;

    /// <summary>
    /// Zuletzt gesehene Meister-Anspruchs-Instanz-Kennung aus den Journalen.
    /// </summary>
    private string _foreignMasterId = string.Empty;

    /// <summary>
    /// UTC-Zeitpunkt des zuletzt gesehenen Meister-Anspruchs; MinValue, wenn keiner gesehen wurde.
    /// </summary>
    private DateTime _foreignMasterSinceUtc = DateTime.MinValue;

    /// <summary>
    /// Sperrt die Meister-Anspruchs-Felder gegen gleichzeitige Buchung (Lesen der Journale) und Abfrage.
    /// </summary>
    private readonly object _masterClaimLock = new();

    /// <summary>
    /// Der offene Schreib-Stream der eigenen Journal-Datei; null, wenn keiner offen ist.
    /// </summary>
    private Stream? _writer;

    /// <summary>
    /// Der aktuell geladene Kopf-Snapshot. Wird beim Aufräumen nie gelöscht.
    /// </summary>
    private string _mySnapshotFile = string.Empty;

    /// <summary>
    /// Die verbuchten Journal-Eintrags-Hashes; Basis für das Dedup beim Schreiben und
    /// Nachladen von Journal-Einträgen.
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _processedHashes = new();

    /// <summary>
    /// True, wenn die Journal-Datei beim Schließen gelöscht werden darf
    /// (keine wichtigen Änderungen enthalten).
    /// </summary>
    private bool _canDeleteWriter = true;

    /// <summary>
    /// Dateiname der aktuell geführten Journal-Datei; leer, wenn kein Writer offen ist.
    /// </summary>
    private string _myJournalFile = string.Empty;

    /// <summary>
    /// Die Journal-Dateien, deren Änderungen noch nicht in einem Snapshot enthalten sind.
    /// </summary>
    private readonly List<string> _filesNotIncluded = [];

    /// <summary>
    /// Zähler der Speicher-Ticks seit der letzten gespeicherten Änderung.
    /// </summary>
    private int _checkerTickCount = -5;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine Instanz ohne Datei-Bezug (Aufzählung der Datei-Verwaltungen);
    /// sie ist nicht im Live-Bestand registriert und wird vom Watcher nicht betickt.
    /// </summary>
    public SaveableFile() { }

    /// <summary>
    /// Erstellt die Verwaltung; das besitzende Objekt wird über Bind() zugeordnet,
    /// sobald sein Konstruktor die Instanz übernommen hat.
    /// </summary>
    public SaveableFile(string filename) {
        Filename = filename.NormalizeFile();
        InitialSavePending = true;
        SaveRequired = true;
        _ = Instances.TryAdd(this, default);

        if (_watcherTimer is null) {
            var t = new Timer(WatcherTick, null, 2000, 2000);
            if (Interlocked.CompareExchange(ref _watcherTimer, t, null) is not null) { t.Dispose(); }
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Der Chunk-Pfad — der einzige Wert der Area-Regeln: die Empfänger-Typen
    /// von der Wurzel bis zum gegchunkten Bereich. Nur dieser Bereich wird je
    /// Chunk-Wert der Items verteilt; alles andere — inklusive Items ohne
    /// Chunk-Wert — läuft als Kopf-Fragment. Ohne Pfad (leer) wird nichts
    /// verteilt: der gesamte Inhalt läuft im Kopf-Fragment.
    /// </summary>
    public List<byte> ChunkPath { get; set; } = [];

    /// <summary>
    /// Sperre für alle Zugriffe auf die Änderungs-Historie.
    /// </summary>
    public object UndoLock { get; } = new();

    /// <summary>
    /// Änderungs-Historie als Fragment-Sätze (Basis-Tiefe 0): je Eintrag eine
    /// Navigationskette plus Blatt-Satz mit Meta. Eintragsgrenze ist der Satz mit Tiefe 0.
    /// Beim Speichern läuft sie komplett ins Kopf-Fragment und wird auf die
    /// neuesten MaxUndos Sätze gekürzt.
    /// </summary>
    public List<FragmentRecord> Undos { get; } = [];

    /// <summary>
    /// Fingerabdrücke der verbuchten Undo-Einträge; Basis der Dubletten-Erkennung
    /// beim Nachladen bereits bekannter Inhalte.
    /// </summary>
    private readonly HashSet<string> _undoEntryHashes = new(StringComparer.Ordinal);

    /// <summary>
    /// Instanz-Kennung aus Rechner und Zufallswert; dient der Unterscheidung
    /// eigener von fremden Dateien im Multi-User-Betrieb.
    /// </summary>
    public string MyId { get; } = $"{Environment.MachineName}|{Guid.NewGuid()}".GetMD5Hash()[..3].ToUpperInvariant();

    /// <summary>
    /// Wert in Minuten. Datenbereiche, die länger nicht verwendet wurden,
    /// werden bei der periodischen Aktualisierung übersprungen.
    /// </summary>
    public const int SkipIfUnusedMinutes = 5;

    /// <summary>
    /// Das besitzende Objekt; wird über Bind() zugeordnet.
    /// </summary>
    public IFragmentStoreable Owner { get; private set; } = null!;

    /// <summary>
    /// Dateiname der Hauptdatei (mit Pfad).
    /// </summary>
    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Logischer Name des Objekts; abgeleitet aus dem Dateinamen und Basis
    /// für Journal- und Meldungs-Namen. Ohne Datei-Bezug die ClassId — darüber
    /// führt AllFileSaveTypes die Layout-Instanz.
    /// </summary>
    public string KeyName => Filename is { Length: > 0 } ? Filename.FileNameWithoutSuffix() : ClassId;

    /// <summary>
    /// True, wenn Meldungen ausgegeben werden sollen.
    /// </summary>
    public bool DropMessages { get; set; } = true;

    /// <summary>
    /// True, wenn seit dem letzten Speichern etwas geändert wurde.
    /// </summary>
    public bool SaveRequired { get; set; }

    /// <summary>
    /// True, solange der initiale Speichervorgang (Erstanlage) noch aussteht.
    /// </summary>
    public bool InitialSavePending { get; set; }

    /// <summary>
    /// Freeze-Grund; nicht leer, wenn die Daten nur noch lesbar sind.
    /// </summary>
    public string FrozenReason { get; private set; } = string.Empty;

    /// <summary>
    /// True, wenn die Daten eingefroren sind und nur noch gelesen werden dürfen.
    /// </summary>
    public bool IsFreezed => FrozenReason is { Length: > 0 };

    /// <summary>
    /// True, wenn der initiale Ladevorgang abgeschlossen ist (auch im Fehlerfall).
    /// </summary>
    public bool MainLoadDone { get; protected set; }

    /// <summary>
    /// True, wenn das Objekt verworfen wurde.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// True, während gerade Daten neu eingespielt werden; Ansichten dürfen in
    /// diesem Fenster keine endgültigen Entfernungen in ihren Einträgen vornehmen.
    /// </summary>
    public bool IsDataReloading => _dataReloadPaused > 0;

    /// <summary>
    /// UTC-Zeitpunkt der letzten gemeldeten Datenänderung; Basis für die
    /// Geändert-während-Speicherung-Prüfung.
    /// </summary>
    public DateTime LastChangeUtc { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// True, wenn anstehende Änderungen aktuell gespeichert werden dürfen.
    /// </summary>
    public virtual bool MaySaveNow => !IsFreezed && !Ending;

    /// <summary>
    /// UTC-Zeitpunkt des neuesten Kopf-Snapshots. Aus dem Dateinamen ermittelt,
    /// nicht aus LastWriteTimeUtc — der Zeitstempel im Namen ist der alleinige
    /// Vergleich für Aktualität (robust gegen Kopieren/Archivieren).
    /// </summary>
    public DateTime LastSaveMainFileUtcDate {
        get =>
        field == DateTime.MinValue
            ? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : field; private set;
    } = DateTime.MinValue;

    /// <summary>
    /// True, wenn der geladene Kopf-Datenstand höchstens eine Minute alt ist.
    /// </summary>
    public bool DataFreshEnough => DateTime.UtcNow.Subtract(_isInCache).TotalMinutes <= 1;

    /// <summary>
    /// True, wenn mindestens ein Chunk kürzlich verwendet wurde.
    /// </summary>
    public bool IsRecentlyUsed {
        get {
            if (_lastUsed.IsEmpty) { return false; }
            var threshold = DateTime.UtcNow.AddMinutes(-SkipIfUnusedMinutes);
            foreach (var kvp in _lastUsed) {
                if (kvp.Value > threshold) { return true; }
            }
            return false;
        }
    }

    /// <summary>
    /// True, wenn die letzte Komplettierung zu lange zurückliegt.
    /// </summary>
    public bool MasterNeeded { get; protected set; }

    /// <summary>
    /// Multi-User-Zugriff ist über das Fragment-Journal möglich: Jeder Benutzer
    /// führt sein eigenes Journale, Komplettierung und Chunks regelt die Verwaltung.
    /// </summary>
    public bool MultiUserPossible => true;

    /// <summary>
    /// True, wenn diese Instanz den Master-Stempel trägt.
    /// </summary>
    public bool IsMaster { get; private set; }

    /// <summary>
    /// UTC-Zeitpunkt, seit dem diese Instanz Master ist.
    /// </summary>
    public DateTime MasterSinceUtc { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Die Items mit Chunk-Wert (IHasKeyName + IHasChunkValue); Quelle ist das
    /// gebundene Objekt (IHasChunkItems).
    /// </summary>
    private IEnumerable<IHasKeyName> ChunkItemSource => Owner is IHasChunkItems source ? source.ChunkItems : [];

    #endregion

    #region Events

    /// <summary>
    /// Wird ausgelöst, wenn Dispose aufgerufen wird. IsDisposed ist zu diesem
    /// Zeitpunkt bereits true, die Events werden aber erst im Anschluss genullet.
    /// </summary>
    public event EventHandler? Disposed;

    /// <summary>
    /// Meldet den Beginn eines Ladevorgangs.
    /// </summary>
    public event EventHandler? Loading;

    /// <summary>
    /// Meldet den Abschluss eines Ladevorgangs; die Nacharbeiten des besitzenden
    /// Objekts (RepairAfterParse) erfolgen in dessen Behandlung.
    /// </summary>
    public event EventHandler<SaveableFileLoadedEventArgs>? Loaded;

    /// <summary>
    /// Fordert die Ansichten des besitzenden Objekts zur Auffrischung auf.
    /// </summary>
    public event EventHandler? InvalidateView;

    /// <summary>
    /// Fordert die Aktualisierung der Nachschlage-Systeme des besitzenden Objekts an.
    /// </summary>
    public event EventHandler? RefreshDataSystems;

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Die Verwaltung ist passiv; ihre Events sind nie unterdrückt.
    /// </summary>
    public bool IsEventsSuppressed => false;

    /// <summary>
    /// Bubbling-Event der Schnittstelle; die Verwaltung löst es nie aus.
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Eine Verwaltung hat keine übernehmbaren Inhalte; jede Übernahme wird abgelehnt.
    /// </summary>
    public string Update(IFragmentStoreable source) => "Verwaltung hat keine übernehmbaren Inhalte";

    /// <summary>
    /// Verständlicher Name eines Kopf-Satzes der Verwaltung; bei unbekannten Werten dessen Zahl.
    /// </summary>
    public static string GetNameOf(byte leafType) => ((SaveableFileFragmentType)leafType).ToString();

    /// <summary>
    /// Schreibt den Kopf-Block: den Blatt-Satz mit der Layout-ClassId.
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) => FragmentRecord.Append(stream, depth, SaveableFileFragmentType.LayoutClassId, ClassId);

    /// <summary>
    /// Verarbeitet den Kopf-Block; die Layout-ClassId muss dieser Verwaltung entsprechen,
    /// unbekannte Blatt-Typen werden übersprungen.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((SaveableFileFragmentType)record.LeafType) {
            case SaveableFileFragmentType.LayoutClassId:
                if (!string.Equals(record.StringContent, ClassId, StringComparison.OrdinalIgnoreCase)) { return false; }
                break;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Die Verwaltung hat keinen Event-Bestand.
    /// </summary>
    public void SuppressEvents() {
        // Absichtlich leer
    }

    /// <summary>
    /// Die Verwaltung hat keinen Event-Bestand.
    /// </summary>
    public void ResumeEvents() {
        // Absichtlich leer
    }

    /// <summary>
    /// Löst das FragmentChanged-Event aus; die Verwaltung ändert sich nie über diesen Weg.
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) => FragmentChanged?.Invoke(this, e);

    #endregion

    #region Methods

    /// <summary>
    /// Ordnet das besitzende Objekt zu. Wird von dessen Konstruktor aufgerufen.
    /// </summary>
    public void Bind(IFragmentStoreable owner) => Owner = owner;

    /// <summary>
    /// Setzt den Freeze-Grund und schließt den Journal-Writer.
    /// </summary>
    public void Freeze(string reason) {
        if (IsFreezed) { return; }

        FrozenReason = reason;

        CloseWriter();
    }

    /// <summary>
    /// Markiert den Stand als geändert; der nächste Speichervorgang schreibt.
    /// </summary>
    public void MarkChanged() {
        SaveRequired = true;
        LastChangeUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Stempelt den Chunk des Chunk-Werts als zuletzt verwendet; steuert das
    /// Überspringen ungenutzter Chunks bei der periodischen Aktualisierung.
    /// </summary>
    public void TouchChunk(string chunkValue) {
        if (chunkValue is not { Length: > 0 }) { return; }

        _lastUsed[IHasChunkValueExtension.ChunkId(chunkValue)] = DateTime.UtcNow;
    }

    /// <summary>
    /// Disposed die Verwaltung: markiert sie als verworfen, trägt sie aus dem
    /// Watcher-Register aus und gibt die Inhalte frei.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Standard-Dispose-Ablauf: idempotent, markiert IsDisposed, trägt die
    /// Instanz aus dem Watcher-Register aus und gibt die Inhalte frei. Der
    /// Journal-Writer läuft nach dem Dispose, damit die von UnMasterMe
    /// geschriebenen Sätze in dieselbe, sauber mit EOF abgeschlossene
    /// Fragment-Datei landen und kein Stream-Handle offen bleibt.
    /// </summary>
    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }
        if (!disposing) { return; }

        // Laufende Hintergrund-Vorgänge abschließen, bevor Zustand und Writer
        // freigegeben werden: Speicher-Task abwarten und die Reserve-Semaphore
        // gegen Reorganize/Komplettierung reservieren.
        WaitDiskOperationFinished();

        var reserved = false;
        try {
            reserved = _saveSemaphore.Wait(30000);
        } catch (ObjectDisposedException) { }

        IsDisposed = true;
        _ = Instances.TryRemove(this, out _);

        OnDisposed();

        Loading = null;
        Loaded = null;
        InvalidateView = null;
        RefreshDataSystems = null;
        Disposed = null;

        // Meister-Anspruch freigeben, BEVOR der Journal-Writer geschlossen wird:
        // Die Freigabe-Sätze landen in derselben, sauber mit EOF abgeschlossenen Datei.
        UnMasterMe();

        CloseWriter();

        if (reserved) {
            try {
                _saveSemaphore.Release();
            } catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// Setzt den logischen Undo-Zeitstempel: mindestens eine Millisekunde nach dem
    /// vorherigen, damit Undo-Einträge eindeutig sortierbar bleiben.
    /// </summary>
    public DateTime UndoStampUtc(DateTime datetimeutc) {
        lock (UndoLock) {
            var t = datetimeutc;
            if (t <= _lastUndoStampUtc) { t = _lastUndoStampUtc.AddMilliseconds(1); }
            _lastUndoStampUtc = t;
            return t;
        }
    }

    /// <summary>
    /// Liest die Sätze eines Undo-Blocks und verbucht sie eintragsweise in der
    /// Änderungs-Historie; sie werden auf Basis-Tiefe zurückgeschoben. Einträge,
    /// deren Fingerabdruck bereits verbucht ist, werden verworfen — so übersteht
    /// ein Nachladen desselben Inhalts die Historie ohne Dubletten.
    /// </summary>
    /// <param name="stream">Quell-Stream, positioniert hinter dem Steuer-Satz.</param>
    /// <param name="navDepth">Tiefe des Steuer-Satzes.</param>
    public void DeserializeUndo(Stream stream, int navDepth) {
        lock (UndoLock) {
            List<FragmentRecord>? entry = null;

            foreach (var undoRecord in FragmentRecord.ReadEntryRecords(stream, navDepth)) {
                // Die Historien-Sätze liegen eine Ebene unter dem Steuer-Satz;
                // der erste Satz eines Eintrags gehört auf Basis-Tiefe 0.
                var basisRecord = undoRecord.WithDepth(undoRecord.Depth - 1);

                if (basisRecord.Depth == 0 && entry is { Count: > 0 }) {
                    AddUndoEntry(entry);
                    entry = [];
                }

                entry ??= [];
                entry.Add(basisRecord);
            }

            if (entry is { Count: > 0 }) { AddUndoEntry(entry); }
        }
    }

    /// <summary>
    /// Verbucht einen kompletten Undo-Eintrag samt Fingerabdruck; einen bereits
    /// bekannten Fingerabdruck verwirft sie. Läuft im UndoLock.
    /// </summary>
    protected void AddUndoEntry(IReadOnlyList<FragmentRecord> entry) {
        if (!_undoEntryHashes.Add(UndoEntryHash(entry))) { return; }

        Undos.AddRange(entry);
    }

    /// <summary>
    /// Fingerabdruck eines Undo-Eintrags über alle seine Sätze; Basis der
    /// Dubletten-Erkennung beim Nachladen.
    /// </summary>
    private static string UndoEntryHash(IReadOnlyList<FragmentRecord> entry) {
        var sb = new System.Text.StringBuilder();

        foreach (var record in entry) {
            _ = sb.Append(record.Depth).Append(':')
                  .Append(record.ValueId).Append(':')
                  .Append((int)record.DataType).Append(':')
                  .Append(Convert.ToBase64String(record.ByteContent)).Append(':')
                  .Append(record.User).Append(':')
                  .Append(record.DateTimeUtc.Ticks).Append(':')
                  .Append(record.Comment).Append('#');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Verwirft die gesamte Änderungs-Historie samt Dubletten-Merkern.
    /// </summary>
    public void ClearUndos() {
        lock (UndoLock) {
            Undos.Clear();
            _undoEntryHashes.Clear();
        }
    }

    /// <summary>
    /// True, wenn die Datei geladen werden darf; reserviert sie für diesen
    /// Ladevorgang. Läuft bereits ein Ladevorgang derselben Datei, liefert die
    /// Methode false, und der Aufrufer bricht still ab.
    /// </summary>
    public virtual bool AllowLoad(string filename) => FilesInLoad.TryAdd(filename.ToUpperInvariant(), default);

    /// <summary>
    /// True, wenn diese Instanz temporärer Master werden darf: Datenstand frisch,
    /// nicht eingefroren und kein fremder Meister-Anspruch aktiv.
    /// </summary>
    public virtual bool AmITemporaryMaster(bool updateAllowed) => DataFreshEnough && !IsFreezed && ForeignMasterAllowsUs();

    /// <summary>
    /// Setzt den Master-Stempel dieser Instanz und trägt den Anspruch als
    /// Steuer-Sätze ins Journal, damit andere Prozesse ihn erkennen.
    /// </summary>
    public virtual void MasterMe() {
        IsMaster = true;
        MasterSinceUtc = DateTime.UtcNow;
        WriteMasterClaim();
    }

    /// <summary>
    /// Versucht, diese Instanz zum Master zu machen, wenn sie es noch nicht ist
    /// und die Daten aktuell genug sind.
    /// </summary>
    public virtual void TryToSetMeTemporaryMaster() {
        if (!IsMaster && AmITemporaryMaster(false)) { MasterMe(); }
    }

    /// <summary>
    /// Gibt den eigenen Meister-Anspruch prozessübergreifend auf: Steuer-Sätze mit
    /// „Unset“-Kennung und zurückdatiertem Zeitpunkt werden ins Journal geschrieben.
    /// </summary>
    public void UnMasterMe() {
        if (!IsMaster) { return; }
        IsMaster = false;

        if (Develop.AllReadOnly) { return; }

        try {
            lock (_writerLock) {
                if (_writer is null) { StartWriter(); }
                if (_writer is null) { return; }

                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterUser, "Unset: " + UserName);
                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterTimeUtc, DateTime.UtcNow.AddMinutes(-2 * MasterClaimMinutes));
                _writer.Flush();
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, "Meister-Freigabe nicht geschrieben: " + KeyName, ex);
        }
    }

    /// <summary>
    /// Trägt den eigenen Meister-Anspruch als Steuer-Sätze ins Journal; andere
    /// Instanzen erkennen ihn beim Nachladen der Journale. Ein Schreibfehler ist
    /// unkritisch: Der Anspruch bleibt dann nur prozessintern.
    /// </summary>
    private void WriteMasterClaim() {
        if (Develop.AllReadOnly) { return; }

        try {
            lock (_writerLock) {
                if (_writer is null) { StartWriter(); }
                if (_writer is null) { return; }

                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterUser, UserName);
                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterTimeUtc, DateTime.UtcNow);
                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterApp, Develop.AppExe());
                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterMachine, Environment.MachineName);
                _ = FragmentRecord.Append(_writer, 0, FragmentTagType.MasterId, MyId);
                _writer.Flush();
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, "Meister-Anspruch nicht geschrieben: " + KeyName, ex);
        }
    }

    /// <summary>
    /// True, wenn kein fremder Meister-Anspruch aktiv ist: Ein Anspruch zählt nur
    /// für andere Benutzer/Rechner/Instanzen und nur bis zum Ablauf von
    /// MasterClaimMinutes; eine „Unset“-Freigabe hebt ihn sofort auf.
    /// </summary>
    private bool ForeignMasterAllowsUs() {
        lock (_masterClaimLock) {
            if (_foreignMasterSinceUtc == DateTime.MinValue) { return true; }
            if (_foreignMasterUser.StartsWith("Unset", StringComparison.Ordinal)) { return true; }
            if (_foreignMasterUser == UserName && _foreignMasterMachine == Environment.MachineName && _foreignMasterId == MyId) { return true; }
            return DateTime.UtcNow.Subtract(_foreignMasterSinceUtc).TotalMinutes > MasterClaimMinutes;
        }
    }

    /// <summary>
    /// Lädt alle verfügbaren Daten-Chunks nach (Komplett-Scan); einzelne Chunks
    /// lädt EnsureLoaded gezielt nach. Mit count wird die Auswahl begrenzt:
    /// oldest=True nimmt die am längsten unangetasteten Chunks, sonst eine
    /// zufällige Auswahl — so erreicht ein Klient mit wenigen Zugriffen einen
    /// repräsentativen Datenstand. Bereits aktuelle Chunks werden übersprungen;
    /// neu geladene Daten melden sich über die Ereignisse an die Ansichten.
    /// </summary>
    /// <param name="oldest">True wird versucht, die ältesten Chunks zu laden.</param>
    /// <param name="count">Die Mindestanzahl der Chunks zum laden. -1 für alle.</param>
    /// <param name="info">Zusatztext für die Diagnose-Ausgabe.</param>
    public bool EnsureDataLoaded(bool oldest, int count, string info) {
        if (IsDisposed || string.IsNullOrEmpty(Filename)) { return true; }

        // Während des Hintergrund-Speicherns kein Einspielen von Chunks — nur
        // der speichernde Thread selbst (Vorab-Laden in SaveChunks) darf laden.
        if (_savingThreadId != -1 && _savingThreadId != Environment.CurrentManagedThreadId) { return false; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return true; }

        var chunkIds = GetAvailableChunkIds();

        if (count >= 0) {
            if (oldest) {
                chunkIds = chunkIds
                    .AsParallel()
                    .Select(id => (Id: id, MTime: GetChunkDirLastWriteUtc(GetChunkFolder(id))))
                    .OrderBy(x => x.MTime)
                    .Take(count)
                    .Select(x => x.Id)
                    .ToList();
            } else {
                chunkIds = [.. chunkIds.OrderBy(_ => Constants.GlobalRnd.Next()).Take(count)];
            }
        }

        var loaded = false;
        var ok = true;
        OnLoading();

        Develop.Diagnose("ChunkLoad", $"{info}Nachladen: {chunkIds.Count} Chunks, oldest={oldest}, count={count}");

        foreach (var chunkId in chunkIds) {
            var result = LoadChunkWithChunkId(chunkId, false);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded && !IsDisposed) {
            OnRefreshDataSystems();
            OnInvalidateView();
            OnLoaded(false);
        }

        return ok;
    }

    /// <summary>
    /// Stellt sicher, dass der Chunk zum Chunk-Wert geladen ist.
    /// </summary>
    public OperationResult EnsureLoaded(string chunk, bool trustProcessedFile) =>
        LoadChunkWithChunkId(IHasChunkValueExtension.ChunkId(chunk), trustProcessedFile);

    /// <summary>
    /// Prüft die Edit-Sperre des Chunks des Chunk-Werts; ohne Wert entfällt die Prüfung.
    /// </summary>
    public string IsEditable(string chunk) {
        if (chunk is not { Length: > 0 }) { return string.Empty; }

        return CheckEditLock(IHasChunkValueExtension.ChunkId(chunk));
    }

    /// <summary>
    /// Tiefe Prüfung vor der Bearbeitung des Chunks des Chunk-Werts:
    /// Edit-Sperre prüfen und den Chunk nachladen.
    /// </summary>
    public string PrepareEdit(string chunk) {
        if (chunk is not { Length: > 0 }) { return string.Empty; }

        var chunkId = IHasChunkValueExtension.ChunkId(chunk);

        if (CheckEditLock(chunkId) is { Length: > 0 } f) { return f; }

        var result = LoadChunkWithChunkId(chunkId, false);

        return result.IsFailed ? result.FailedReason : string.Empty;
    }

    /// <summary>
    /// Stellt die Ablage nach einer Änderung der Chunk-Spalte neu: Diese Instanz
    /// übernimmt den Master-Stempel, es folgt eine sofortige Komplettierung und
    /// mit Chunk-Spalte das Schreiben aller Daten-Chunks; ohne Chunk-Spalte
    /// werden alle Chunk-Ordner gelöscht. Vorab werden alle Daten-Chunks
    /// nachgeladen, damit keine nur in Chunk-Dateien liegende Zeile verloren
    /// geht; mit fehlgeschlagenem Nachladen wird nichts verändert. Läuft
    /// synchron — der Aufrufer wartet auf das Ergebnis. Ein laufender
    /// Hintergrund-Speicher-Task wird dabei abgewartet.
    /// </summary>
    public void Reorganize() {
        if (IsDisposed || Develop.AllReadOnly) { return; }

        // Gegen Save und Komplettierung im Hintergrund serialisiert: Warten ist
        // hier gewollt — der Aufrufer bekommt das Ergebnis der Neuverteilung.
        var reserved = false;
        try {
            reserved = _saveSemaphore.Wait(60000);
        } catch (ObjectDisposedException) { return; }

        if (!reserved) {
            Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Reorganize abgebrochen: Auf '{KeyName}' läuft weiterhin ein anderer Speichervorgang.", 0);
            return;
        }

        try {
            ReorganizeCore();
        } finally {
            try {
                _saveSemaphore.Release();
            } catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// Eigentlicher Reorganize-Ablauf; nur unter der Reserve-Semaphore aufrufen.
    /// </summary>
    private void ReorganizeCore() {
        if (IsDisposed || Develop.AllReadOnly) { return; }

        // Fremde Snapshots nach dem eigenen Lade-Stand: Der Speicher kann deren
        // absorbierte Journal-Einträge fehlen — erst neu laden, dann neu verteilen.
        if (!InitialSavePending && LastSaveMainFileUtcDate.Subtract(_loadedSnapshotUtcDate).TotalSeconds > JournalToleranceSeconds) {
            Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, "Reorganize abgebrochen: Der Kopf-Stand von '" + KeyName + "' wurde seit dem eigenen Laden fremd komplementiert. Tabelle neu laden.", 0);
            return;
        }

        var chunkFolder = BaseChunkFolder();

        if (IO.DirectoryExists(chunkFolder) && !EnsureDataLoaded(true, -1, string.Empty)) {
            Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, "Reorganize abgebrochen: Nicht alle Chunks von '" + KeyName + "' konnten geladen werden.", 0);
            return;
        }

        var hasChunkPath = ChunkPath is { Count: > 0 };

        MasterMe();

        if (hasChunkPath) {
            // Mit Chunk-Spalte zuerst die alte Verteilung räumen und alle
            // Daten-Chunks schreiben: SaveChunks überspringt sonst als
            // verarbeitet markierte Chunks. Der Kopf-Snapshot trägt nur Items
            // ohne Chunk-Wert — schlägt das Schreiben fehl, darf nicht
            // komplettiert werden, sonst liegen diese Zeilen nirgends dauerhaft.
            RemoveAllChunkFolders(chunkFolder);

            var chunkError = SaveChunks();

            if (!string.IsNullOrEmpty(chunkError)) {
                Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Reorganize abgebrochen: Chunks von '{KeyName}' konnten nicht gespeichert werden: {chunkError}", 0);
                return;
            }
        }

        // Sofortige Komplettierung statt des zeitgesteuerten Komplettstands.
        var opr = PrepareNewSnapshot();

        if (opr.IsFailed) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {opr.FailedReason}", 0);
            return;
        }

        if (opr.Value is not List<byte> content) { return; }

        var f = WriteSnapshotFile(content);

        if (!string.IsNullOrEmpty(f)) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {f}", 0);
            return;
        }

        if (IsDisposed) { return; }

        // Die eigene Komplettierung ist der neue Basis-Stand des Speichers.
        _loadedSnapshotUtcDate = LastSaveMainFileUtcDate;

        CloseWriter();
        StartWriter();

        MasterNeeded = false;
        _filesNotIncluded.Clear();
        OnInvalidateView();

        if (hasChunkPath) { return; }

        // Ohne Chunk-Spalte liegen alle Zeilen im Kopf-Snapshot:
        // alle Chunk-Ordner löschen.
        RemoveAllChunkFolders(chunkFolder);

        SaveRequired = true;
    }

    /// <summary>
    /// Löscht alle Chunk-Ordner und verwirft deren Verarbeitungs-Stempel.
    /// </summary>
    private void RemoveAllChunkFolders(string chunkFolder) {
        if (!IO.DirectoryExists(chunkFolder)) { return; }

        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();

            IO.DeleteDir(subDir, false);
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed.TryRemove(chunkId, out _);
            _lastContentHash.TryRemove(chunkId, out _);
        }
    }

    /// <summary>
    /// Liefert die dateirelevanten Gründe, warum das Objekt aktuell nicht
    /// editierbar ist: Freeze und Lade-Zustand, danach muss das
    /// Chunk-Arbeitsverzeichnis beschreibbar sein und kein kritischer Prozess
    /// laufen.
    /// </summary>
    public string EditableError(bool isloading) {
        if (IsFreezed) { return FrozenReason; }

        // Während des Hintergrund-Speicherns: keine Skripte, Zell-Änderungen und
        // Editoren; nur der speichernde Thread selbst läuft durch diese Prüfung.
        // Kurzes Speichern wird statt einer Ablehnung begrenzt abgewartet — eine
        // Abweisung erzeugt beim Aufrufer nur Rollback und Wiederholungsaufwand.
        if (_savingThreadId != -1 && _savingThreadId != Environment.CurrentManagedThreadId) {
            for (var warte = 0; warte < 50 && _savingThreadId != -1; warte++) {
                Develop.Message(ErrorType.Info, this, "Speicher", ImageCode.Auge, "Warte auf Speicherzugriff", 0);
                Thread.Sleep(200);
            }
            if (_savingThreadId != -1 && _savingThreadId != Environment.CurrentManagedThreadId) { return "Tabelle wird gerade gespeichert."; }
        }

        // Eine frische Erstanlage ist vollständig im Speicher; der erste Save()
        // erzeugt die Datei und setzt MainLoadDone.
        if (!isloading && !MainLoadDone && !InitialSavePending) { return "Laden noch nicht abgeschlossen"; }

        if (string.IsNullOrEmpty(Filename)) { return "Fragmentpfad nicht gesetzt."; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        // Frische Erstanlage: Der Ordner fehlt noch und wird vom ersten Save() angelegt.
        if (!IO.DirectoryExists(BaseChunkFolder())) { return string.Empty; }

        var opr = IO.CanWriteInDirectory(BaseChunkFolder());

        return opr.IsFailed ? opr.FailedReason : string.Empty;
    }

    /// <summary>
    /// Liest den Objekt-Baum aus dem übergebenen Inhalt. Vorab werden
    /// Erzeuger-Stempel und Format-Version aus den Kopf-Sätzen erfasst; eine
    /// abweichende Format-Version bricht ab.
    /// </summary>
    protected bool ParseMainContent(Stream stream) {
        if (!ReadCreationStamp(stream)) { return false; }

        return Owner.Deserialize(stream, 0);
    }

    /// <summary>
    /// Erzeugt den zu speichernden Inhalt: Erzeuger-Stempel und Objekt-Baum des
    /// besitzenden Objekts. Welche Änderungs-Historie eingeht, entscheidet der
    /// Aufrufer und übergibt sie. Liefert eine leere Liste, wenn der Inhalt
    /// ungültig ist. Vor dem Objekt-Baum wird der Erzeuger-Stempel geschrieben:
    /// beim ersten Speichern wird gestempelt, danach bleibt der Stempel der
    /// geladenen Datei erhalten.
    /// </summary>
    protected List<byte> GenerateContentToSave(List<FragmentRecord> undos) {
        using var ms = new MemoryStream();

        _ = FragmentRecord.Append(ms, 0, FragmentTagType.CreateUser, _createUser is { Length: > 0 } u ? u : UserName);
        _ = FragmentRecord.Append(ms, 0, FragmentTagType.CreateTimeUtc, _createTimeUtc > DateTime.MinValue ? _createTimeUtc : DateTime.UtcNow);

        return Owner.Serialize(ms, 0, Owner.TypeName, undos) ? [.. ms.ToArray()] : [];
    }

    /// <summary>
    /// Liest Erzeuger-Stempel (CreateUser, CreateTimeUtc) und Format-Version aus
    /// den Kopf-Sätzen des Streams und setzt die Stream-Position danach zurück,
    /// damit der Inhalt unverändert weitergelesen werden kann. Liefert false bei
    /// einer abweichenden Format-Version; ohne Versions-Satz (Fragment-Journal-
    /// Einträge) wird akzeptiert.
    /// </summary>
    private bool ReadCreationStamp(Stream stream) {
        var start = stream.Position;
        string? createUser = null;
        var createTimeUtc = DateTime.MinValue;
        var serializerVersion = string.Empty;

        while (FragmentRecord.TryReadNext(stream) is { } record) {
            if (!record.IsFileTag) { break; }

            switch (record.TagType) {
                case FragmentTagType.CreateUser:
                    createUser = record.StringContent;
                    break;
                case FragmentTagType.CreateTimeUtc:
                    createTimeUtc = record.DateTimeContent;
                    break;
                case FragmentTagType.SerializerVersion:
                    serializerVersion = record.StringContent;
                    break;
            }
        }

        stream.Position = start;

        // Fremde Format-Version (alter Bestand, neuere Anwendung): gar nicht erst
        // in die Klassen-Parser lassen, sonst knallen fremde Satz-Layouts im Deep-Parse.
        if (serializerVersion is { Length: > 0 } v && !string.Equals(v, FragmentRecord.SerializerVersionCurrent, StringComparison.Ordinal)) { return false; }

        if (createUser is { Length: > 0 } u) {
            _createUser = u;
            _createTimeUtc = createTimeUtc;
        }

        return true;
    }

    /// <summary>
    /// Stellt sicher, dass die Kopf-Datei existiert und den Layout-Kopf mit der
    /// Layout-ClassId trägt. Fehlt sie oder ist sie leer (abgebrochene
    /// Erstanlage), wird sie neu geschrieben; ein abweichender Inhalt bleibt
    /// unangetastet und führt beim Laden zu einer klaren Fehlermeldung.
    /// </summary>
    protected void EnsureHeadFileExists() {
        if (Filename is not { Length: > 0 }) { return; }
        if (IO.FileExists(Filename) && IO.ReadAllBytes(Filename, 5).Value is byte[] { Length: > 0 }) { return; }

        WriteHeadFile();
    }

    /// <summary>
    /// Schreibt die Kopf-Datei: Diese Verwaltung serialisiert sich als Fragment-Baum
    /// (Standard-Kopf mit Typ-Satz, darunter der Blatt-Satz mit der Layout-ClassId).
    /// Ein bestehender Inhalt wird überschrieben.
    /// </summary>
    public void WriteHeadFile() {
        if (Filename is not { Length: > 0 }) { throw Develop.DebugError("Dateiname für die Kopf-Datei fehlt."); }

        using var ms = new MemoryStream();

        if (!this.Serialize(ms, 0)) { throw Develop.DebugError("Kopf-Datei konnte nicht erzeugt werden."); }

        var opr = IO.WriteAllBytes(Filename, ms.ToArray());
        if (opr.IsFailed) { throw Develop.DebugError("Kopf-Datei konnte nicht geschrieben werden: " + opr.FailedReason); }
    }

    /// <summary>
    /// Liest die Layout-ClassId aus dem Blatt-Satz der Kopf-Datei. False bei
    /// fehlender, leerer oder ungültiger Datei. Läuft bewusst ohne Instanz:
    /// Die ClassId bestimmt erst, welche Verwaltung erzeugt wird.
    /// </summary>
    public static bool TryReadHeadClassId(string filename, out string classId) {
        classId = string.Empty;

        if (filename is not { Length: > 0 }) { return false; }
        if (IO.ReadAllBytes(filename, 5).Value is not byte[] bytes || bytes.Length == 0) { return false; }

        using var ms = new MemoryStream(bytes);

        while (FragmentRecord.TryReadNext(ms) is { } record) {
            if (record.IsTag) { continue; } // Kopf- und Steuer-Sätze überspringen

            if ((SaveableFileFragmentType)record.LeafType != SaveableFileFragmentType.LayoutClassId) { return false; }

            classId = record.StringContent;
            return classId is { Length: > 0 };
        }

        return false;
    }

    /// <summary>
    /// True, wenn die Kopf-Datei einen gültigen Layout-Kopf trägt; Bestände vor
    /// dem Kopf-Datei-Muster liefern false.
    /// </summary>
    protected static bool HasHeadClassId(string filename) => TryReadHeadClassId(filename, out _);

    #endregion

    #region Speichern

    /// <summary>
    /// Stößt das Speichern als Fire-and-Forget-Hintergrund-Task an und kehrt
    /// sofort zurück. Läuft bereits ein Speichervorgang, wird der Aufruf
    /// verworfen — SaveRequired bleibt aktiv und der nächste Tick holt nach.
    /// Fehler werden gemeldet, nicht an den Aufrufer zurückgegeben.
    /// </summary>
    public OperationResult Save() {
        if (Develop.AllReadOnly) { return OperationResult.Success; }
        if (!SaveRequired) { return OperationResult.Success; }

        Develop.Message(ErrorType.Info, null, KeyName, ImageCode.Diskette, $"Speichere {KeyName}", 1);

        lock (_saveLock) {
            if (_saveTask is { IsCompleted: false }) {
                // Es läuft bereits ein Speichervorgang: einen Folge-Task anhängen,
                // der danach den dann aktuellen Stand nachzieht — so deckt
                // WaitDiskOperationFinished auch die neuesten Änderungen ab.
                var vorgänger = _saveTask;
                _saveTask = Task.Run(async () => {
                    await vorgänger;
                    try {
                        return SaveCore();
                    } catch (Exception ex) {
                        return OperationResult.Failed(ex);
                    }
                });
                return OperationResult.Success;
            }

            _saveTask = Task.Run(() => {
                try {
                    return SaveCore();
                } catch (Exception ex) {
                    return OperationResult.Failed(ex);
                }
            });
        }

        return OperationResult.Success;
    }


    /// <summary>
    /// Wartet auf das Ende eines laufenden Hintergrund-Speicher-Tasks —
    /// für das Beenden und andere kritische Punkte.
    /// </summary>
    public void WaitDiskOperationFinished() {
        Task? t;
        lock (_saveLock) {
            t = _saveTask;
        }
        if (t is { IsCompleted: false }) { _ = t.Wait(60000); }
    }

    /// <summary>
    /// Eigentlicher Speicher-Ablauf im Hintergrund-Task: Kopf-Teil und
    /// Daten-Chunks schreiben. Mit Reserve-Semaphore gegen Reorganize und
    /// Komplettierung serialisiert; Fehler werden gemeldet und SaveRequired
    /// bleibt aktiv, damit ein erneuter Versuch die Daten nachholt. Während
    /// des Ablaufs sind Skripte, Zell-Änderungen und Editoren der eigenen
    /// Tabelle über EditableError blockiert.
    /// </summary>
    private OperationResult SaveCore() {
        if (IsDisposed) { return OperationResult.Failed("Objekt ist bereits freigegeben"); }

        if (!_saveSemaphore.Wait(0)) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }

        // Checker-Tick (Skripte) stoppen und fremde Threads über EditableError blockieren.
        PauseTimer();
        _savingThreadId = Environment.CurrentManagedThreadId;

        try {
            var result = SaveInternal();
            OnInvalidateView();

            if (!string.IsNullOrEmpty(result)) {
                Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Diskette, $"Speichern von {KeyName} fehlgeschlagen: {result}", 0);
                return OperationResult.Failed(result);
            }
        } catch (Exception ex) {
            Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Diskette, $"Speichern von {KeyName} fehlgeschlagen: {ex.Message}", 0);
            return OperationResult.Failed(ex);
        } finally {
            _savingThreadId = -1;
            ResumeTimer();
            _ = _saveSemaphore.Release();
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Interner Speicheraufruf: Der Kopf-Teil erzeugt bei der Erstanlage einen
    /// Snapshot und flusht danach nur die Journale; danach werden die
    /// Daten-Chunks write-once geschrieben. Chunks, die durch eine fremde
    /// Edit-Sperre blockiert sind oder nicht geladen werden konnten, werden
    /// übersprungen — SaveRequired bleibt aktiv, der nächste Versuch holt
    /// sie nach.
    /// </summary>
    private string SaveInternal() {
        var headError = SaveHeadInternal();
        if (!string.IsNullOrEmpty(headError)) { return headError; }

        return SaveChunks();
    }

    /// <summary>
    /// Interner Speicheraufruf des Kopf-Teils: Bei der Erstanlage wird ein Snapshot
    /// erzeugt, danach werden nur die Journale geflusht — Änderungen landen als
    /// Fragmente, nie als Überschreiben einer Datei.
    /// </summary>
    private string SaveHeadInternal() {
        if (InitialSavePending) {
            EnsureHeadFileExists();

            var opr = PrepareNewSnapshot();

            if (opr.IsFailed) { return opr.FailedReason; }
            if (opr.Value is not List<byte> content) { return OperationResult.FailedInternalError.FailedReason; }

            var f = WriteSnapshotFile(content);
            if (!string.IsNullOrEmpty(f)) { return f; }

            // Der Erstanlage-Snapshot ist der Basis-Stand des Speichers.
            _loadedSnapshotUtcDate = LastSaveMainFileUtcDate;
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
            lock (_writerLock) {
                if (_writer is null) { SaveRequired = false; return string.Empty; }
                _writer.Flush();
            }
            SaveRequired = false;
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    #endregion

    #region Kopf-Journal

    /// <summary>
    /// Stellt sicher, dass die Daten aktuell sind: Die Kopf-Journale aller
    /// Benutzer werden nachgeladen und die geladenen Daten-Chunks auf
    /// neuere Dateien geprüft.
    /// </summary>
    public bool Refresh(bool firstTime) {
        if (!RefreshHeadJournals(firstTime)) { return false; }

        if (IsDisposed || string.IsNullOrEmpty(Filename)) { return true; }

        // Reentrancy-Sperre: Timer sowie UI-Aktionen können Refresh zeitgleich
        // aufrufen. Ohne Sperre würden sich die Aufrufe überlappen.
        if (Interlocked.CompareExchange(ref _isRefreshingChunks, 1, 0) != 0) { return true; }

        try {
            bool loaded;

            PauseDataReload();
            try {
                loaded = RefreshLoadedChunks(firstTime);
            } finally {
                ResumeDataReload();
            }

            if (loaded && !IsDisposed) {
                OnRefreshDataSystems();
                OnInvalidateView();
                OnLoaded(false);
            }

            return true;
        } finally {
            Interlocked.Exchange(ref _isRefreshingChunks, 0);
        }
    }

    /// <summary>
    /// Stellt sicher, dass die Kopf-Daten aktuell sind, indem die Journale aller
    /// Benutzer nachgeladen werden.
    /// </summary>
    private bool RefreshHeadJournals(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (firstTime) { _isInCache = LastSaveMainFileUtcDate; }

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Lade Fragmente von '" + KeyName + "'", 0); }

        // Zeitstempel VOR dem Lesen der Fragmente erfassen.
        // Wird als readStartedUtc an InjectData übergeben und dort als _isInCache
        // gesetzt sowie für die Toleranz-Prüfung der Komplettierung genutzt.
        // Konservativ: Fragmente, die während des Lesens hinzukommen, werden beim
        // nächsten Aufruf erneut gelesen - lieber doppelt als verloren.
        var readStartedUtc = DateTime.UtcNow;

        var fragmentFiles = IO.GetFiles(BaseChunkFolder(), KeyName.ToUpperInvariant() + "-*." + SuffixOfFragments, SearchOption.TopDirectoryOnly).ToList();
        fragmentFiles.Remove(_myJournalFile);

        var undoRecords = GetLastChanges(fragmentFiles);
        if (undoRecords.IsFailed) { return false; }

        var opr = InjectData(fragmentFiles, undoRecords, readStartedUtc, firstTime);
        return opr.IsSuccessful;
    }

    /// <summary>
    /// Bereitet die Fortführung einer sauber abgeschlossenen Fragment-Datei vor:
    /// Ordner- und Existenzprüfung, Writer-Übernahme und Kennzeichnung als eigene Datei.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    public string ContinueFragment(string fragmentFilename) {
        if (!fragmentFilename.StartsWith(BaseChunkFolder(), StringComparison.OrdinalIgnoreCase)) { return "Die Datei liegt nicht im Fragment-Ordner: " + fragmentFilename; }
        if (!IO.FileExists(fragmentFilename)) { return "Fragment-Datei nicht gefunden: " + fragmentFilename; }

        // Ggf. bereits offenen Writer schließen (z. B. Master-Eintrag beim Laden):
        // Diese (fast leere) Neue-Datei darf das Aufräumen dann entsorgen.
        CloseWriter();

        // Ab sofort als eigene Datei führen: hält sie beim Nachladen aus den
        // Fremd-Fragmenten heraus und schützt sie vor Aufräum-Löschungen.
        _myJournalFile = fragmentFilename;
        _canDeleteWriter = false;

        // Satz 254 (ContinueEOF) anhängen: Die Datei endet nicht mehr auf EOF,
        // daher wählt sie kein zweiter Prozess zur Fortführung.
        var error = ContinueFragmentFile(fragmentFilename, out var writer);
        lock (_writerLock) {
            _writer = writer;
        }
        return error;
    }

    /// <summary>
    /// Nimmt einen kompletten Undo-Eintrag als Kopf-Fragment entgegen und schreibt
    /// ihn als Journal-Eintrag: In einem Stück in die Journal-Datei, als verarbeitet
    /// verbucht und samt Fingerabdruck in die Änderungs-Historie gebucht.
    /// Beim Laden, beim Einspielen fremder Fragmente und im Nur-Lese-Modus still.
    /// </summary>
    public void LogUndoEntry(IReadOnlyList<FragmentRecord> entry) {
        if (IsDataReloading) { return; }

        if (IsDisposed || Ending) { return; }
        if (Develop.AllReadOnly) { return; }
        if (InitialSavePending) { return; }
        if (_doingChanges > 0) { return; }
        if (entry is not { Count: > 0 }) { return; }

        try {
            lock (_writerLock) {
                if (_writer is null) { StartWriter(); }
                if (_writer is null) { return; }

                using var ms = new MemoryStream();

                var failedRecord = entry.FirstOrDefault(record => !record.Append(ms, record.Depth));
                if (failedRecord is not null) {
                    Freeze("Undo-Satz ungültig oder zu groß: Typ " + failedRecord.ValueId);
                    return;
                }

                _canDeleteWriter = false;

                var entryBytes = ms.ToArray();
                _writer.Write(entryBytes, 0, entryBytes.Length);
                _writer.Flush();

                // Eigene Änderung als verarbeitet verbuchen: Der gleiche Eintrag
                // wird beim Nachladen fremder Journale nicht erneut angewendet.
                _ = MarkProcessed(EntryHash(entryBytes));

                lock (UndoLock) {
                    AddUndoEntry(entry);
                }
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }
    }

    /// <summary>
    /// Nimmt eine Journal-Daten-Einspielung entgegen; Editierbarkeits-Prüfung
    /// vorab. Liefert Fehlermeldung oder leer.
    /// </summary>
    private string CheckInjectAllowed(bool initialload) => EditableError(initialload);

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
        IO.CreateDirectory(BaseChunkFolder());
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Journal-Datei, falls sie keine
    /// permanenten Daten enthält. Abschließend wird der EOF-Satz geschrieben.
    /// </summary>
    private void CloseWriter() {
        lock (_writerLock) {
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

                if (_canDeleteWriter) {
                    IO.DeleteFile(_myJournalFile, false);
                }
            }
        }
    }

    /// <summary>
    /// Führt Aufräumarbeiten nach dem Laden von Fragmenten durch,
    /// inkl. Komplettierung und Bereinigung alter Dateien.
    /// </summary>
    /// <param name="fragmentFiles">Die im selben Lesezyklus ermittelten Fragment-Dateien.</param>
    /// <param name="startTimeUtc">UTC-Zeitpunkt des Lese-Starts des Nachlade-Durchlaufs</param>
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
            // Fremde Snapshots nach dem eigenen Lade-Stand: Deren absorbierte Journal-
            // Einträge wurden hier per Toleranz übersprungen und fehlen im Speicher.
            // Ein Komplett-Schreiben würde sie aus dem Kopf-Stand zurückrollen.
            if (!InitialSavePending && LastSaveMainFileUtcDate.Subtract(_loadedSnapshotUtcDate).TotalSeconds > JournalToleranceSeconds) {
                Develop.Diagnose("Fragment-Komplettierung", $"Von {KeyName} verschoben: Kopf-Stand seit dem eigenen Lade-Stand ({_loadedSnapshotUtcDate.ToString5()}) fremd komplementiert ({LastSaveMainFileUtcDate.ToString5()}). Stack: {Develop.DiagStack()}");
                UnMasterMe();
                return;
            }

            // Komplettierung Fire-and-Forget im Hintergrund; gegen Save und
            // Reorganize über die Reserve-Semaphore serialisiert. Schlägt die
            // Reservierung fehl, läuft gerade ein anderer Vorgang — MasterNeeded
            // bleibt aktiv und der nächste Refresh-Zyklus komplettiert erneut.
            if (_saveSemaphore.Wait(0)) {
                _ = Task.Run(() => {
                    try {
                        CompleteAsMaster(startTimeUtc);
                    } finally {
                        _ = _saveSemaphore.Release();
                    }
                });
            }
        }

        if (fragmentFiles is not { Count: >= 1 }) { return; }

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien schützen, deren Änderungen noch nicht in einem Snapshot stehen

        // Zusätzliche Safety zur Zeitstempel-Prüfung unten: Dateien, die in diesem
        // Durchlauf gelesen wurden und deren Items noch nicht in einem Snapshot
        // stehen (_filesNotIncluded), werden explizit aus der Lösch-Liste entfernt.
        // Nach einer erfolgreichen Komplettierung ist die Liste geleert -
        // dann greift ausschließlich die Zeitstempel-Prüfung.
        if (_filesNotIncluded.Count != 0) {
            foreach (var thisch in _filesNotIncluded) {
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
    /// Schreibt den Komplettstand im Hintergrund: Snapshot erzeugen und
    /// schreiben, Journal neu starten und den eigenen Basis-Stand nachziehen.
    /// Toleranz-Prüfungen werden erneut ausgeführt, da zwischen Anstoß und
    /// Ausführung Zeit vergehen kann.
    /// </summary>
    private void CompleteAsMaster(DateTime startTimeUtc) {
        if (IsDisposed || Ending) { return; }

        var opr = PrepareNewSnapshot();

        if (opr.IsFailed) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {opr.FailedReason}", 0);
            return;
        }

        if (opr.Value is not List<byte> content) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {OperationResult.FailedInternalError.FailedReason}", 0);
            return;
        }

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds >= JournalToleranceSeconds) {
            // Fremd-Einträge, die während dieses Lese-Zyklus geschrieben wurden,
            // wären nach dem Schreiben älter als die Toleranz in ReadJournalFile
            // und würden dauerhaft übersprungen. Der nächste Refresh-Zyklus
            // liest die Journale erneut und Komplettiert dann.
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung von {KeyName} verschoben: Nachladen benötigte länger als {JournalToleranceSeconds} Sekunden", 0);
            return;
        }

        // Fremd-Komplettierung in der Zwischenzeit? Deren absorbierte Journal-
        // Einträge fehlen im Speicher — ein Komplett-Schreiben würde sie aus
        // dem Kopf-Stand zurückrollen.
        if (!InitialSavePending && LastSaveMainFileUtcDate.Subtract(_loadedSnapshotUtcDate).TotalSeconds > JournalToleranceSeconds) {
            Develop.Diagnose("Fragment-Komplettierung", $"Von {KeyName} verschoben: Kopf-Stand seit dem eigenen Lade-Stand ({_loadedSnapshotUtcDate.ToString5()}) fremd komplementiert ({LastSaveMainFileUtcDate.ToString5()}). Stack: {Develop.DiagStack()}");
            UnMasterMe();
            return;
        }

        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Erstelle neuen Komplettstand: " + KeyName, 0);

        var f = WriteSnapshotFile(content);

        if (!string.IsNullOrEmpty(f)) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Komplettierung fehlgeschlagen: {f}", 0);
            return;
        }

        if (IsDisposed) { return; }

        CloseWriter();
        StartWriter();

        MasterMe();

        // Die eigene Komplettierung ist der neue Basis-Stand des Speichers.
        _loadedSnapshotUtcDate = LastSaveMainFileUtcDate;

        MasterNeeded = false;
        OnInvalidateView();
        _filesNotIncluded.Clear();
    }

    /// <summary>
    /// Ermittelt die neuesten Änderungen aus den Journal-Dateien: Jede Datei wird
    /// generisch in Eintrags-Bytes (Basis-Tiefe 0) zerlegt und als flache Liste der
    /// Undo-Sätze geliefert; diese tragen ihre Eintrags-Daten (Entry, Container,
    /// EntryDateTimeUtc) als Felder. Einträge, die sicher vor der letzten
    /// Snapshot-Speicherung liegen, und bereits verbuchte Einträge werden übersprungen.
    /// </summary>
    /// <param name="fragmentFiles">Die zuvor ermittelten Journal-Dateien.</param>
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
    /// Datei-Ebene der Tiefe 0 und Müll-Zeilen werden übersprungen, Einträge ohne Meta verworfen; eine
    /// Datei mit abweichender Format-Version wird komplett übergegangen. Die
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
                    // Kopf-Benutzer merken; Blockreste ohne Eintrag bzw. übrige Kopf-Sätze überspringen.
                    // Abweichende Format-Version: die ganze Datei übergehen.
                    if (first.IsFileTag && first.TagType == FragmentTagType.SaveUser) { headUser = first.StringContent; }

                    if (first.IsFileTag && first.TagType == FragmentTagType.SerializerVersion
                        && !string.Equals(first.StringContent, FragmentRecord.SerializerVersionCurrent, StringComparison.Ordinal)) { return; }

                    TrackForeignMasterClaim(first);

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
    /// Bucht Meister-Anspruchs-Sätze (MasterUser/MasterTimeUtc/MasterMachine/MasterId)
    /// aus den Journalen; der jeweils neuere Anspruch gewinnt.
    /// </summary>
    private void TrackForeignMasterClaim(FragmentRecord record) {
        lock (_masterClaimLock) {
            switch (record.TagType) {
                case FragmentTagType.MasterUser:
                    _foreignMasterUser = record.StringContent;
                    break;

                case FragmentTagType.MasterMachine:
                    _foreignMasterMachine = record.StringContent;
                    break;

                case FragmentTagType.MasterId:
                    _foreignMasterId = record.StringContent;
                    break;

                case FragmentTagType.MasterTimeUtc:
                    if (record.DateTimeContent > _foreignMasterSinceUtc) { _foreignMasterSinceUtc = record.DateTimeContent; }
                    break;
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
    /// <param name="readStartedUtc">UTC-Zeitpunkt vor dem Lesen der Journale; Grundlage der Toleranz-Prüfung bei der Komplettierung</param>
    /// <param name="initialload"></param>
    private OperationResult InjectData(List<string> fragmentFiles, OperationResult undoRecords, DateTime readStartedUtc, bool initialload) {
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
                            AddUndoEntry(pendingRecords);
                        }

                        pendingRecords.Clear();
                    }

                    // Eintrag ohne Bytes gibt es nicht anzuwenden — überspringen.
                    if (thisWork.Entry is not { } entryBytes) { continue; }

                    appliedEntry = entryBytes;
                    _filesNotIncluded.Add(thisWork.Container);

                    using var ms = new MemoryStream(entryBytes);

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
                        AddUndoEntry(pendingRecords);
                    }
                }

                _isInCache = readStartedUtc;
            } finally {
                ResumeDataReload();
                Interlocked.Decrement(ref _doingChanges);

                if (!IsDisposed) {
                    OnRefreshDataSystems();
                    // Lese-Start statt Zeitpunkt nach dem Lesen: Der Toleranz-Guard
                    // in DoWorkAfterLastChanges muss das gesamte Nachladen messen,
                    // sonst würden Fremd-Einträge, die während der Lese-Phase
                    // geschrieben wurden, nach der Komplettierung dauerhaft
                    // übersprungen.
                    DoWorkAfterLastChanges(fragmentFiles, readStartedUtc);
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
    private void StartWriter() {
        if (string.IsNullOrEmpty(Filename)) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + _isInCache.ToString5());
            return;
        }

        lock (_writerLock) {
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

                // Der Name endet immer auf den 19-stelligen Zeitstempel, den die
                // Aufräumarbeit ausliest. Belegte Namen (z. B. das eigene, eben
                // behaltene Journal aus derselben Sekunde) werden gemieden: Der
                // Sekunden-Stempel wird erhöht, bis der Name frei ist. CreateNew
                // bleibt der atomare Schiedsrichter gegen Fremd-Zugriffe.
                var stempel = DateTime.UtcNow;
                var basis = BaseChunkFolder().NormalizePath() + wunschName + "-";
                _myJournalFile = basis + stempel.ToString4() + "." + SuffixOfFragments;

                while (IO.FileExists(_myJournalFile)) {
                    stempel = stempel.AddSeconds(1);
                    _myJournalFile = basis + stempel.ToString4() + "." + SuffixOfFragments;
                }

                if (Develop.AllReadOnly) { return; }

                FileStream? fileStream = null;
                try {
                    fileStream = new FileStream(_myJournalFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                    _writer = fileStream;
                    fileStream = null;

                    WriteJournalStartRecords(_writer);

                    _canDeleteWriter = true;
                    _writer.Flush();
                    return;
                } catch (Exception ex) {
                    fileStream?.Dispose();
                    _writer?.Dispose();
                    _writer = null;

                    // Existiert die Zieldatei bereits, hat CreateNew die Namens-Kollision
                    // (gleicher Sekunden-Stempel) erkannt — erwartet und wird vom nächsten
                    // Versuch gelöst. Nur echte Fehler melden.
                    if (!IO.FileExists(_myJournalFile)) {
                        Develop.DebugPrint(ErrorType.Warning, "Journaldatei nicht offen (Versuch " + versuch.ToString1() + "): " + _myJournalFile.FileNameWithSuffix(), ex);
                    }

                    Pause(1, false);
                }
            }
        }

        Freeze("Journaldatei konnte nicht geöffnet werden: " + _myJournalFile);
    }

    /// <summary>
    /// Schreibt die Kopf-Sätze einer neuen Fragment-Datei. Basis: Version,
    /// Inhaltstyp, Erzeuger, Speicherer und Rechner.
    /// </summary>
    private void WriteJournalStartRecords(Stream writer) {
        var nowUtc = DateTime.UtcNow;

        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SerializerVersion, FragmentRecord.SerializerVersionCurrent);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.Type, Owner.TypeName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.CreateUser, UserName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.CreateTimeUtc, nowUtc);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SaveUser, UserName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.Machine, Environment.MachineName);
        _ = FragmentRecord.Append(writer, 0, FragmentTagType.SaveTimeUtc, nowUtc);
    }

    /// <summary>
    /// Listet die Snapshot-Dateien im Arbeitsverzeichnis auf, sortiert nach dem
    /// Zeitstempel im Dateinamen (neueste zuerst). Dateien ohne gültiges Datum
    /// werden ans Ende sortiert. Liest direkt vom Dateisystem, damit Snapshots
    /// anderer Master sofort erkannt werden.
    /// </summary>
    private List<string> GetSnapshotFilesNewestFirst() {
        if (!IO.DirectoryExists(BaseChunkFolder())) { return []; }

        try {
            return IO.GetFiles(BaseChunkFolder())
                .Where(f => string.Equals(f.FileSuffix(), SuffixOfSnapshots, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => ExtractDateFromFileName(f) ?? DateTime.MinValue)
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

            var fileDate = ExtractDateFromFileName(thisf);
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
        if (ExtractDateFromFileName(newest) is { } d) { LastSaveMainFileUtcDate = d; }
    }

    /// <summary>
    /// Der Kopf-Snapshot trägt die komplette Änderungs-Historie; der Chunk-Bereich
    /// wird auf die Items ohne Chunk-Wert reduziert.
    /// </summary>
    private List<byte> GenerateSnapshotContent() {
        var itemIds = ChunkItemsIndex().ChunkIds;

        List<FragmentRecord> undos;
        lock (UndoLock) {
            undos = Undos.TrimUndos();
        }

        var source = GenerateContentToSave(undos).ToArray();

        return BuildHeadContent(source, itemIds);
    }

    /// <summary>
    /// Erzeugt den Inhalt eines neuen Kopf-Snapshots und prüft Konsistenz und
    /// Mindestgröße. Bei Erfolg liegt der Inhalt in Value.
    /// </summary>
    private OperationResult PrepareNewSnapshot() {
        var f = EditableError(false);
        if (f is { Length: > 0 }) { return OperationResult.Failed(f); }

        var x = LastChangeUtc;

        var content = GenerateSnapshotContent();

        if (x != LastChangeUtc) { return OperationResult.Failed("Daten wurden während der Speicherung geändert."); }

        if (content.Count == 0) { return OperationResult.Failed("Snapshot konnte nicht erzeugt werden."); }

        // Die Mindestgrößen-Prüfung schützt vor korrupten Snapshots. Beim initialen
        // Snapshot ist der Inhalt legitim klein und darf
        // nicht zum Freeze führen — sonst wäre das neue Objekt sofort blockiert.
        if (!InitialSavePending && content.Count < MinimumContentSize) {
            Freeze("Datei zu klein für Speicherung");
            return OperationResult.Failed("Datei zu klein für Speicherung.");
        }

        return new OperationResult(content);
    }

    /// <summary>
    /// Schreibt den erzeugten Snapshot-Inhalt als neue Datei mit UTC-Zeitstempel
    /// im Namen; die bisherigen Dateien bleiben unangetastet. Schreibfehler sind
    /// unkritisch: Loader überspringen Snapshots ohne gültigen EOF-Marker.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private string WriteSnapshotFile(List<byte> content) {
        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return "Verzeichnis konnte nicht erstellt werden."; }

        // UTC-Zeitstempel, Benutzer, Maschinen/Instanz-Hash.
        var path = BaseChunkFolder() + $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}.{SuffixOfSnapshots}";

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
        LastSaveMainFileUtcDate = DateTime.UtcNow;

        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Neuer Komplettstand: " + path.FileNameWithSuffix(), 1);

        return string.Empty;
    }

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

    #region Chunk-Ziel und Inhaltszerlegung

    /// <summary>
    /// Liefert die Chunk-ID (lowercase), in die ein Undo-Eintrag gehören würde,
    /// oder leer, wenn er zum Kopf gehört: Der Pfad des Eintrags
    /// (Empfänger-Typen der Sätze) muss den Chunk-Pfad treffen und der
    /// Item-Kenner muss einen Chunk-Wert liefern.
    /// TODO: Unused
    /// </summary>
    public string EntryChunkId(IReadOnlyList<FragmentRecord> entry, Dictionary<string, string> itemIds) {
        if (!IsChunkSection(entry)) { return string.Empty; }

        if (EntryItemKey(entry) is not { Length: > 0 } key) { return string.Empty; }

        return itemIds.TryGetValue(key, out var byItem) ? byItem : string.Empty;
    }

    /// <summary>
    /// True, wenn der Pfad des Eintrags (Empfänger-Typen der Sätze) den
    /// Chunk-Pfad trifft.
    /// </summary>
    private bool IsChunkSection(IReadOnlyList<FragmentRecord> entry) {
        if (ChunkPath is not { Count: > 0 }) { return false; }

        List<byte> path = [];

        foreach (var record in entry) {
            if (record.LeafType > 0) { path.Add(record.LeafType); }
        }

        if (path.Count < ChunkPath.Count) { return false; }

        for (var i = 0; i < ChunkPath.Count; i++) {
            if (path[i] != ChunkPath[i]) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Liefert den Item-Kenner des Eintrags: den Schlüssel des ersten ItemPath-
    /// Nav-Satzes (Wert-Änderungen, Entfernungen) bzw. des ersten ItemAdd-Blattes
    /// (Aufnahmen ohne Nav-Satz); leer ohne Kenner.
    /// </summary>
    private static string EntryItemKey(IReadOnlyList<FragmentRecord> entry) {
        foreach (var record in entry) {
            if (record.TagType == FragmentTagType.ItemPath && record.DataType == FragmentDataType.String) { return record.StringContent; }

            if (record.TagType == FragmentTagType.ItemAdd && record.DataType == FragmentDataType.KeyNameType) { return record.KeyNameContent; }
        }

        return string.Empty;
    }

    /// <summary>
    /// Index der Chunk-Items: Schlüssel → Chunk-ID (lowercase, nur Items mit
    /// Chunk-Wert) und die Menge aller bekannten Schlüssel.
    /// </summary>
    private (Dictionary<string, string> ChunkIds, HashSet<string> KnownKeys) ChunkItemsIndex() {
        var ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> keys = [];

        foreach (var item in ChunkItemSource) {
            if (item.KeyName is not { Length: > 0 } key) { continue; }

            _ = keys.Add(key);

            if (item is IHasChunkValue hasChunkValue && hasChunkValue.ChunkId() is { Length: > 0 } id) { ids[key] = id; }
        }

        return (ids, keys);
    }

    /// <summary>
    /// Liefert die Chunk-IDs (lowercase, sortiert-distinkt) aller Chunk-Items;
    /// die IDs liefert IHasChunkValueExtension.ChunkId.
    /// </summary>
    public List<string> KnownChunkIds() {
        List<string> ids = [];

        foreach (var item in ChunkItemSource) {
            if (item is not IHasChunkValue hasChunkValue) { continue; }

            if (hasChunkValue.ChunkId() is not { Length: > 0 } id) { continue; }

            ids.Add(id);
        }

        return ids.SortedDistinctList();
    }

    /// <summary>
    /// True, wenn der Empfänger-Typ den Eintritt in den Chunk-Bereich markiert.
    /// </summary>
    private bool IsChunkArea(byte leafType) => ChunkPath is { Count: > 0 } && ChunkPath[0] == leafType;

    /// <summary>
    /// Erzeugt den Kopf-Inhalt (Stempel + Kopf-Bereiche + Kopf-Historie +
    /// Abschluss-Marker): Alle Bereiche inklusive kompletter Änderungs-Historie
    /// laufen unverändert in den Kopf — nur der Chunk-Bereich wird auf die Items
    /// ohne Chunk-Wert reduziert. Quelle ist der vollständige Fragment-Baum
    /// des Speicherstands.
    /// </summary>
    private List<byte> BuildHeadContent(byte[] source, Dictionary<string, string> itemIds) {
        var records = ParseRecords(source);

        var result = new List<byte>();
        var i = 0;

        // Datei-Stempel am Kopf unverändert übernehmen
        while (i < records.Count && records[i].Record.IsFileTag) {
            result.AddRange(source[records[i].Start..records[i].End]);
            i++;
        }

        // Nutzdaten regelbasiert übernehmen
        while (i < records.Count) {
            var (start, end, record) = records[i];

            if (record.TagType == FragmentTagType.Eof) {
                result.AddRange(source[start..end]);
                break;
            }

            if (record.Depth > 0) { i++; continue; } // Satz außerhalb der Wurzel-Ebene — überspringen

            // Chunk-Bereich: auf die Items ohne Chunk-Wert reduzieren.
            if (IsChunkArea(record.LeafType) && i + 1 < records.Count && records[i + 1].Record.TagType == FragmentTagType.ItemList) {
                result.AddRange(source[start..records[i + 1].End]); // Bereichs-Eintritt + Listen-Kopf

                var j = i + 2;
                while (j < records.Count && records[j].Record.Depth > record.Depth + 1) {
                    var next = BlockEnd(records, j);

                    if (!itemIds.ContainsKey(records[j].Record.KeyNameContent ?? string.Empty)) {
                        result.AddRange(source[records[j].Start..(next < records.Count ? records[next].Start : source.Length)]);
                    }

                    j = next;
                }

                i = j;
                continue;
            }

            // Fester Bereich: komplett in den Kopf.
            var areaEnd = EntryEnd(records, i);
            result.AddRange(source[start..(areaEnd < records.Count ? records[areaEnd].Start : source.Length)]);
            i = areaEnd;
        }

        return result;
    }

    /// <summary>
    /// Erzeugt den kompletten Inhalt des Chunks mit der angegebenen ID (Stempel +
    /// der Chunk-Bereich mit den Items des Chunk-Werts + Abschluss-Marker); die
    /// Änderungs-Historie bleibt komplett im Kopf-Fragment. Der Chunk-Bereich
    /// erhält einen Scoped Listen-Kopf (ItemListChunk), dessen Einlesen nur den
    /// Bestand dieses Chunks angleicht — leere verteilte Chunks räumen so ihren
    /// Altbestand selbst ab.
    /// </summary>
    private List<byte> BuildChunkContent(string chunkId, byte[] source, Dictionary<string, string> itemIds) {
        var records = ParseRecords(source);

        var result = new List<byte>();
        var i = 0;

        // Datei-Stempel am Kopf unverändert übernehmen
        while (i < records.Count && records[i].Record.IsFileTag) {
            result.AddRange(source[records[i].Start..records[i].End]);
            i++;
        }

        // Nutzdaten regelbasiert übernehmen
        while (i < records.Count) {
            var (start, end, record) = records[i];

            if (record.TagType == FragmentTagType.Eof) {
                result.AddRange(source[start..end]);
                break;
            }

            if (record.Depth > 0) { i++; continue; } // Satz außerhalb der Wurzel-Ebene — überspringen

            // Chunk-Bereich: nur die Items dieses Chunks, mit Scoped Listen-Kopf.
            if (IsChunkArea(record.LeafType) && i + 1 < records.Count && records[i + 1].Record.TagType == FragmentTagType.ItemList) {
                var j = i + 2;
                var keepItems = new List<int>();
                var distributed = false;

                while (j < records.Count && records[j].Record.Depth > record.Depth + 1) {
                    var next = BlockEnd(records, j);

                    distributed = true;

                    if (itemIds.TryGetValue(records[j].Record.KeyNameContent ?? string.Empty, out var itemId) &&
                        string.Equals(itemId, chunkId, StringComparison.OrdinalIgnoreCase)) { keepItems.Add(j); }

                    j = next;
                }

                if (keepItems.Count > 0 || distributed) {
                    result.AddRange(source[start..records[i + 1].Start]); // Bereichs-Eintritt

                    if (distributed) {
                        result.AddRange(ScopedListHead(records[i + 1].Record.Depth, chunkId));
                    } else {
                        result.AddRange(source[records[i + 1].Start..records[i + 1].End]); // Listen-Kopf
                    }

                    foreach (var itemIndex in keepItems) {
                        var next = BlockEnd(records, itemIndex);
                        result.AddRange(source[records[itemIndex].Start..(next < records.Count ? records[next].Start : source.Length)]);
                    }
                }

                i = j;
                continue;
            }

            // Fester Bereich: gehört nicht in diesen Chunk — überspringen.
            var areaEnd = EntryEnd(records, i);
            i = areaEnd;
        }

        return result;
    }

    /// <summary>
    /// Zerlegt den Quell-Inhalt in Sätze samt Byte-Bereich (Start, Ende).
    /// </summary>
    private static List<(int Start, int End, FragmentRecord Record)> ParseRecords(byte[] source) {
        var records = new List<(int Start, int End, FragmentRecord Record)>();
        using var ms = new MemoryStream(source);

        var recordStart = 0L;

        while (FragmentRecord.TryReadNext(ms) is { } record) {
            records.Add(((int)recordStart, (int)ms.Position, record));
            recordStart = ms.Position;
        }

        return records;
    }

    /// <summary>
    /// Index des ersten Satzes, der nicht mehr zum Eintrag an der angegebenen
    /// Position gehört (nächster Kopf derselben Ebene oder ein Satz darüber).
    /// </summary>
    private static int EntryEnd(List<(int Start, int End, FragmentRecord Record)> records, int i) {
        var end = i + 1;

        while (end < records.Count && records[end].Record.Depth > 0) { end++; }

        return end;
    }

    /// <summary>
    /// Index des ersten Satzes, der nicht mehr zum Block an der angegebenen
    /// Position gehört (nächster Kopf derselben Ebene oder ein Satz darüber).
    /// </summary>
    private static int BlockEnd(List<(int Start, int End, FragmentRecord Record)> records, int itemIndex) {
        var depth = records[itemIndex].Record.Depth;
        var i = itemIndex + 1;

        while (i < records.Count && records[i].Record.Depth > depth) { i++; }

        return i;
    }

    /// <summary>
    /// Erzeugt einen Scoped Listen-Kopf (ItemListChunk, Content = Chunk-ID), dessen
    /// Einlesen nur den Bestand dieses Chunks angleicht.
    /// </summary>
    private static List<byte> ScopedListHead(int depth, string chunkId) {
        using var head = new MemoryStream();
        _ = FragmentRecord.Append(head, depth, FragmentTagType.ItemListChunk, chunkId);
        return [.. head.ToArray()];
    }

    /// <summary>
    /// Inhalt ab dem ersten Nutzdaten-Satz (Datei-Stempel am Kopf ausgenommen);
    /// Vergleichsbasis des Unverändert-Checks, da die Stempel je Speicherung wechseln.
    /// </summary>
    private static byte[] ChunkCompareBasis(byte[] content) {
        using var ms = new MemoryStream(content);

        var start = 0L;
        while (FragmentRecord.TryReadNext(ms) is { IsFileTag: true }) { start = ms.Position; }

        return content[(int)start..];
    }

    #endregion

    #region Chunk-Speicherung

    /// <summary>
    /// Speichert alle Daten-Chunks write-once: je Chunk der vollständige Inhalt
    /// aus dem Speicherstand, je Chunk Edit-Sperre geprüft; unveränderte Chunks
    /// bleiben unangetastet. Wandert ein Wert in einen Chunk, muss der Ziel-Chunk
    /// vorher geladen sein — nicht geladene Chunks werden vorab geladen,
    /// fehlgeschlagene übersprungen. Ohne Chunk-Pfad wird kein Daten-Chunk
    /// geschrieben — der gesamte Inhalt läuft im Kopf-Fragment. Liefert bei
    /// Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    private string SaveChunks() {
        if (IsDisposed) { return "Objekt ist bereits freigegeben"; }
        if (Develop.AllReadOnly) { return string.Empty; }

        // Ohne Chunk-Pfad gibt es keine Chunk-Verteilung: ALLES wird im
        // Kopf-Fragment geführt (Snapshot und Journal).
        if (ChunkPath is not { Count: > 0 }) { return string.Empty; }

        // Ohne Chunk-Ordner und ohne Items mit Chunk-Wert gibt es nichts zu tun —
        // das Verhalten ist vollständig das des Fragment-Journals.
        if (!IO.DirectoryExists(BaseChunkFolder()) && KnownChunkIds().Count == 0) { return string.Empty; }

        var x = LastChangeUtc;
        var timestamp = GenerateChunkTimestamp();

        // Daten-Chunks ermitteln: alle Chunks mit aktuellem Item-Bestand plus
        // alle Chunks mit bestehendem Ordner — Letztere bekommen ohne Bestand
        // nur ihren Scoped-Listenkopf ohne Items geschrieben, damit das Einlesen
        // deren Altbestand räumt und keine verwaisten Ordner bleiben.
        var dataChunkIds = KnownChunkIds();

        foreach (var thisChunkId in GetAvailableChunkIds()) {
            if (!dataChunkIds.Contains(thisChunkId)) { dataChunkIds.Add(thisChunkId); }
        }

        if (dataChunkIds.Count == 0) { return string.Empty; }

        // Ein Daten-Chunk wird nur gespeichert, wenn er vorher geladen wurde —
        // der Inhalt wird aus dem Fragment-Baum geschnitten. Wandert ein Wert
        // in einen noch nicht geladenen Chunk, wird dieser zuerst geladen,
        // sonst fehlen seine übrigen Items im Snapshot und würden beim
        // nächsten Einlesen als veraltet entfernt. Lädt der Chunk nicht, wird
        // er übersprungen — SaveRequired bleibt aktiv, der nächste Versuch
        // holt ihn nach.
        var loadFailedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var thisChunkId in dataChunkIds) {
            if (_processedFile.ContainsKey(thisChunkId)) { continue; }

            var loadResult = LoadChunkWithChunkId(thisChunkId, false);

            if (loadResult.IsFailed) {
                loadFailedChunkIds.Add(thisChunkId);
                Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Diskette, $"Chunk '{thisChunkId}' von '{KeyName}' konnte nicht geladen werden — Speicherung übersprungen: {loadResult.FailedReason}", 0);
            }
        }

        // Item-Index und vollständiger Fragment-Baum des Speicherstands — erst
        // NACH dem Vorab-Laden: der Index ist sonst leer, und BuildChunkContent
        // würde alle Items fallen lassen (leere Chunk-Dateien schreiben).
        var itemIds = ChunkItemsIndex().ChunkIds;

        // Vollständiger Fragment-Baum des Speicherstands ohne Änderungs-Historie;
        // Quelle aller Chunk-Inhalte — die Historie bleibt komplett im Kopf-Fragment.
        var source = GenerateContentToSave([]).ToArray();

        if (x != LastChangeUtc) { return "Daten wurden während der Speicherung geändert."; }

        // path → vollständiger Inhalt, fertig zum Schreiben
        var chunkData = new ConcurrentDictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);
        // chunkId (lowercase) → Hash des Voll-Inhalts, aktualisiert nach erfolgreichem Speichern
        var newHashes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // chunkIds mit echten Änderungen (für EditLock Re-Check vor dem Schreiben)
        var changedChunkIds = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        // Anzahl der Chunks, die durch die Edit-Sperre oder einen Ladefehler
        // blockiert wurden. In diesem Fall bleibt SaveRequired true, damit der
        // nächste Speicherversuch die ungespeicherten Änderungen erneut versucht.
        var chunksSkipped = 0;

        // Aufbau und Hash je Chunk parallel: CPU-lastig (Baum-Schnitte, SHA256);
        // Quelle (source) und Item-Index sind eingefroren und nur lesend genutzt.
        Parallel.ForEach(dataChunkIds, thisChunkId => {
            var idLower = thisChunkId.ToLowerInvariant();

            if (loadFailedChunkIds.Contains(idLower)) {
                Interlocked.Increment(ref chunksSkipped);
                return;
            }

            // Per-Chunk-Editierbarkeit: ist der Chunk durch einen anderen
            // Benutzer gesperrt, wird er übersprungen — die anderen Chunks
            // werden weiter gespeichert.
            if (CheckEditLock(idLower) is { Length: > 0 }) {
                Interlocked.Increment(ref chunksSkipped);
                return;
            }

            // Vollständigen Inhalt aufbauen (inkl. Stempel und Abschluss-Marker)
            var fullContent = BuildChunkContent(idLower, source, itemIds);
            if (fullContent.Count == 0) { return; }

            var fullHash = Generic.GetSHA256HashString(ChunkCompareBasis([.. fullContent]));
            newHashes[idLower] = fullHash;

            // Unchanged-Check: bei identischem Inhalt keine neue Datei schreiben
            if (_lastContentHash.TryGetValue(idLower, out var storedHash) && storedHash == fullHash) { return; }

            chunkData[$"{GetChunkFolder(idLower)}{timestamp}.{SuffixOfChunks}"] = fullContent;
            changedChunkIds[idLower] = default;
        });

        // Re-Check: Es wird vorab kein Claim geschrieben, daher kann ein anderer
        // Benutzer zwischenzeitlich einen der zu schreibenden Chunks gesperrt haben.
        // Der betroffene Chunk wird wie im Vorab-Check übersprungen und nicht
        // geschrieben — sein Write-once-Bestand bleibt fremd aktuell, die eigenen
        // Änderungen bleiben im Kopf-Journal und der nächste Versuch holt nach.
        // Ein Freeze wäre hier falsch: Die Sperre ist zeitlich begrenzt.
        foreach (var chunkId in changedChunkIds.Keys) {
            if (CheckEditLock(chunkId) is { Length: > 0 }) {
                chunksSkipped++;
                _ = chunkData.TryRemove($"{GetChunkFolder(chunkId)}{timestamp}.{SuffixOfChunks}", out _);
                _ = newHashes.TryRemove(chunkId, out _);
            }
        }

        // Direkt auf Festplatte schreiben.
        // Chunks sind write-once — jede Datei ist neu und wird nie überschrieben.
        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks von '{KeyName}'", 2);

        // Schreiben je Chunk parallel: eigenständige Ordner, write-once —
        // keine gemeinsamen Ressourcen außer den Concurrent-Verwaltungen.
        var writeError = (string?)null;
        Parallel.ForEach(chunkData, kvp => {
            var path = kvp.Key;

            if (IO.CreateDirectory(path.FilePath()).IsFailed) {
                Freeze($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                _ = Interlocked.CompareExchange(ref writeError, $"Verzeichnis für '{path}' konnte nicht erstellt werden", null);
                return;
            }

            // Chunk-Dateien werden gezippt gespeichert
            var zipped = kvp.Value.ToArray().ZipIt();
            if (zipped is null || zipped.Length == 0) {
                Freeze($"Komprimierung fehlgeschlagen für '{path}'");
                _ = Interlocked.CompareExchange(ref writeError, $"Komprimierung fehlgeschlagen für '{path}'", null);
                return;
            }

            var writeResult = IO.WriteAllBytes(path, zipped);
            if (writeResult.IsFailed) {
                Freeze(writeResult.FailedReason);
                _ = Interlocked.CompareExchange(ref writeError, writeResult.FailedReason, null);
                return;
            }

            // Nach dem Speichern: nur den Dateinamen tracken, Daten verwerfen
            var folder = path.FilePath();
            var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            _processedFile[chunkId] = path;
            _lastUsed[chunkId] = DateTime.UtcNow;
            CleanupOldChunkFiles(GetChunkFilesOrderedByTime(folder).ToArray());
        });

        if (writeError is { Length: > 0 }) { return writeError; }

        // Hashes nach erfolgreichem Speichern aktualisieren
        foreach (var kvp2 in newHashes) {
            _lastContentHash[kvp2.Key] = kvp2.Value;
        }

        // SaveRequired nur aktiv halten, wenn Chunks ausständig blieben.
        if (chunksSkipped != 0) { SaveRequired = true; }

        return string.Empty;
    }

    /// <summary>
    /// Generiert den Timestamp-String für Chunk-Dateinamen im Format
    /// yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.
    /// </summary>
    internal string GenerateChunkTimestamp() =>
        $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}";

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Bereichs-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.Suffix).
    /// Millisekunden sind verpflichtend. Liefert null, wenn das Datum nicht geparst werden kann.
    /// </summary>
    internal static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 23) { return null; }
        var datePart = fileName[..23];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Löscht alle Chunk-Dateien in der übergebenen Liste, die älter als
    /// DeleteOldChunkFilesAfterHours Stunden sind, sofern mindestens
    /// ChunkFileCount neuere Dateien vorhanden sind. Dateien mit ungültigem
    /// Datumsformat werden ebenfalls gelöscht. Die Liste muss nach Datum
    /// absteigend (neueste zuerst) sortiert sein.
    /// </summary>
    private static void CleanupOldChunkFiles(string[] files) {
        if (files.Length <= ChunkFileCount) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldChunkFilesAfterHours);

        for (var i = files.Length - 1; i >= ChunkFileCount; i--) {
            var file = files[i];
            var fileDate = ExtractDateFromFileName(file);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Extrahiert den 3-stelligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Gibt string.Empty zurück,
    /// wenn kein Hash vorhanden ist (alte Dateien).
    /// </summary>
    private static string ExtractMachineInstanceHashFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return string.Empty; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[(sepIdx + 1)..] : string.Empty;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Der angehängte
    /// Machine/Instance-Hash wird abgeschnitten. Bei alten Dateien ohne Hash
    /// wird der komplette Teil nach dem ersten Unterstrich zurückgegeben.
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return fileName; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[..sepIdx] : userAndHash;
    }

    /// <summary>
    /// Listet alle Chunk-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
    /// </summary>
    private static IEnumerable<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return IO.GetFiles(folder)
                .Where(f => string.Equals(f.FileSuffix(), SuffixOfChunks, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => ExtractDateFromFileName(f) ?? DateTime.MinValue);
        } catch {
            return [];
        }
    }

    /// <summary>
    /// [Datenordner]\[Name]\
    /// </summary>
    public string BaseChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Prüft die Edit-Sperre für einen Chunk: Wenn die neueste Datei im Ordner
    /// jünger als die Sperrzeit ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    private string CheckEditLock(string chunkId) {
        var folder = GetChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return string.Empty; }

        var fileDate = ExtractDateFromFileName(newestFile);
        if (!fileDate.HasValue) { return "Datums-Fehler in Dateien"; }

        if (DateTime.UtcNow.Subtract(fileDate.Value).TotalSeconds >= EditLockSeconds) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        var creatorHash = ExtractMachineInstanceHashFromFileName(newestFile);
        var creatorDisplay = string.IsNullOrEmpty(creatorHash) ? creator : $"{creator} - {creatorHash}";
        var lockUntil = fileDate.Value.AddSeconds(EditLockSeconds).ToLocalTime();
        return $"Der Chunk '{chunkId}' wird bis {lockUntil:HH:mm} Uhr von '{creatorDisplay}' bearbeitet";
    }

    /// <summary>
    /// Listet alle Chunk-IDs aus dem Dateisystem auf — jeder Ordner ist ein
    /// Chunk (bis auf Altlasten-Ordner).
    /// </summary>
    private List<string> GetAvailableChunkIds() {
        if (string.IsNullOrEmpty(Filename)) { return []; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return []; }

        // Ordnernamen (= Chunk-IDs) direkt als Chunk-Werte verwenden.
        var allChunkIds = new List<string>();

        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            allChunkIds.Add(subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant());
        }

        return allChunkIds.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// [Datenordner]\[Name]\[Chunk]\
    /// </summary>
    private string GetChunkFolder(string chunkId) => $"{BaseChunkFolder()}{chunkId.ToLowerInvariant()}\\";

    private static DateTime GetChunkDirLastWriteUtc(string dir) {
        try {
            return IO.DirectoryExists(dir) ? new DirectoryInfo(dir).LastWriteTimeUtc : DateTime.MaxValue;
        } catch {
            return DateTime.MaxValue;
        }
    }

    /// <summary>
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer erstellt wurde.
    /// Der Machine/Instance-Hash dient nur der Anzeige: Auch eine neue Instanz
    /// (neue Session, neuer Prozess) darf die eigenen Dateien ihres Benutzers
    /// weiterbearbeiten; die Edit-Sperre schützt ausschließlich vor fremden
    /// Benutzern.
    /// </summary>
    private bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        return string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Chunk-Laden

    /// <summary>
    /// Lädt einen Chunk direkt von der Festplatte.
    /// Nach dem Parsen wird der Chunk-Inhalt verworfen — nur der Dateiname wird
    /// für Aktualitätsprüfungen gemerkt.
    /// </summary>
    /// <param name="chunkId">Chunk-ID (wird auf Lowercase normalisiert).</param>
    /// <param name="trustProcessedFile">True: bereits geladene Chunks (Eintrag in _processedFile)
    /// erzeugen keinen Verzeichniszugriff mehr — der erste Load bleibt unberührt,
    /// der Reload läuft ausschließlich über RefreshLoadedChunks. False: immer prüfen.</param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordnung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    private OperationResult LoadChunkWithChunkId(string chunkId, bool trustProcessedFile) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        // Reload ignorieren: Der Chunk ist geladen und wird nicht erneut von der
        // Festplatte geprüft. Aktualität liefert RefreshLoadedChunks (Fenster).
        if (trustProcessedFile && _processedFile.ContainsKey(chunkId)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} skip-verarbeitet");
            return OperationResult.SuccessFalse;
        }

        var folder = GetChunkFolder(chunkId);

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer konnte zwischenzeitlich eine neuere Datei ablegen.
        var chunkFiles = IO.DirectoryExists(folder) ? GetChunkFilesOrderedByTime(folder).ToArray() : [];
        string? newestFile = chunkFiles.Length > 0 ? chunkFiles[0] : null;

        // Bereits aktuell? (gleiche Datei wie zuletzt verarbeitet)
        if (newestFile is not null &&
            _processedFile.TryGetValue(chunkId, out var lastFile) &&
            string.Equals(lastFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (!IO.DirectoryExists(folder)) {
            // Ohne Ordner gibt es nichts zu laden — und es wird auch keiner angelegt:
            // Das Speichern erzeugt Ordner nur für Chunks mit Inhalt; so bleiben
            // Phantom-Ordner für unbekannte Chunk-IDs aus.
            // _lastUsed aktualisieren, damit nicht-existierende Chunks in Refresh
            // nicht permanent übersprungen werden, sondern nach SkipIfUnusedMinutes neu geprüft werden.
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} ohne-ordner");
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
            // Alle Dateien des Chunks wurden gelöscht — Tracking-Eintrag entfernen,
            // damit veraltete Daten nicht als "aktuell" gelten.
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} ohne-datei");
            return OperationResult.SuccessFalse;
        }

        // Raw bytes direkt von der Festplatte lesen
        return LoadChunkFile(chunkId, newestFile, chunkFiles);
    }

    /// <summary>
    /// Liest die neueste Chunk-Datei, parst ihren Inhalt in den Owner und
    /// aktualisiert das Tracking (verarbeitete Datei, Hashes, alte Dateien).
    /// </summary>
    private OperationResult LoadChunkFile(string chunkId, string newestFile, string[] chunkFiles) {
        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Lade Chunk '{chunkId}' von '{KeyName}'", 1);

        if (IO.ReadAllBytes(newestFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) {
            return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht gelesen werden");
        }

        // Für Parsing entpacken (Chunk-Dateien werden gezippt gespeichert)
        byte[] chunkContent;
        if (rawBytes.IsZipped()) {
            chunkContent = rawBytes.UnzipIt() ?? [];
            if (chunkContent.Length == 0) {
                return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht entpackt werden");
            }
        } else {
            chunkContent = rawBytes;
        }

        // Teil-Bestand: Datei-Stempel überspringen; der Inhalt wird über den
        // Standard-Deserialize des Owners eingelesen — dessen Klassen-Switch
        // entscheidet, welche Bereiche (Zeilen, …) ihn übernehmen.
        using var ms = new MemoryStream(chunkContent);

        var start = 0L;
        while (FragmentRecord.TryReadNext(ms) is { IsFileTag: true }) { start = ms.Position; }

        ms.Position = start;

        // Während des Einspielens fremder Inhalte keine Undo-Einträge verbuchen
        PauseDataReload();
        try {
            if (!Owner.Deserialize(ms, 0)) { return OperationResult.Failed($"Chunk '{chunkId}' Parsen fehlgeschlagen"); }
        } finally {
            ResumeDataReload();
        }

        // Chunk nach dem Parsen verwerfen - nur Dateiname und Hash tracken
        _processedFile[chunkId] = newestFile;
        _lastUsed[chunkId] = DateTime.UtcNow;
        _lastContentHash[chunkId] = Generic.GetSHA256HashString(ChunkCompareBasis(chunkContent));

        CleanupOldChunkFiles(chunkFiles);

        Develop.Diagnose("ChunkLoad", $"{chunkId} geladen {newestFile.FileNameWithSuffix()} aus {chunkFiles.Length} Dateien");
        return new OperationResult(true);
    }

    /// <summary>
    /// Aktualisiert alle bekannten Daten-Chunks (Einträge in _processedFile):
    /// Prüft für jeden, ob eine neuere Datei im Ordner existiert und lädt
    /// diese ggf. neu. Neue Chunk-Ordner anderer Benutzer werden hier nicht
    /// entdeckt — das übernimmt GetAvailableChunkIds via EnsureDataLoaded.
    /// Chunks, deren LastUsed-Stempel älter als SkipIfUnusedMinutes ist,
    /// werden übersprungen, sofern firstTime false ist.
    /// Da Chunks write-once sind, reicht der Dateiname-Vergleich.
    /// </summary>
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        var loaded = false;

        // Gezielter Zugriff pro bekanntem Chunk (alle im Speicher befindlichen
        // Chunk-IDs aus _processedFile). Die neueste Datei wird direkt mit der
        // zuletzt verarbeiteten verglichen.
        foreach (var chunkId in _processedFile.Keys.ToList()) {
            // SkipIfUnusedMinutes-Fenster: Niemand schaut den Chunk mehr an — kein Refresh nötig.
            if (!firstTime && _lastUsed.TryGetValue(chunkId, out var lastUsed)
                && DateTime.UtcNow.Subtract(lastUsed).TotalMinutes >= SkipIfUnusedMinutes) { continue; }

            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            // Neueste Datei ermitteln und mit der zuletzt verarbeiteten vergleichen.
            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var processedFile = _processedFile[chunkId];

            // Reload wenn eine andere Datei die neueste ist. Da Chunks write-once
            // sind, bedeutet ein anderer Dateiname zwingend anderen Inhalt.
            if (string.Equals(processedFile, newestFile, StringComparison.OrdinalIgnoreCase)) { continue; }

            var result = LoadChunkWithChunkId(chunkId, false);
            if (result.IsFailed) {
                Develop.DebugPrint(ErrorType.Warning, $"Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
            }
            loaded = loaded || result.Value is true;
        }

        return loaded;
    }

    #endregion

    #region Laden

    /// <summary>
    /// Öffnet und lädt die Ablage: Die Kopf-Datei wird mit Layout-Kopf
    /// sichergestellt, Bestände mit fremdem Inhalt ohne gültigen Layout-Kopf
    /// werden abgelehnt, und der Kopf-Datenstand kommt aus dem neuesten Snapshot
    /// des Arbeitsverzeichnisses.
    /// </summary>
    /// <param name="info">Datei-Info, z. B. Dateiname oder Bereichsname.</param>
    /// <param name="freeze">Optionaler Freeze-Grund nach dem Laden.</param>
    public virtual void LoadFromFile(string info, string freeze) {
        if (!PrepareLoad(info, out var fileNameToLoad)) { return; }

        Filename = fileNameToLoad;

        EnsureHeadFileExists();

        // Einheitliche Regel: Eine Kopf-Datei mit Inhalt, aber ohne gültigen
        // Layout-Kopf (Bestand vor dem Kopf-Datei-Muster), wird nicht geraten.
        if (!HasHeadClassId(Filename)) {
            Freeze("Kopf-Datei ohne gültigen Layout-Kopf (alter Bestand oder beschädigt): " + Filename.FileNameWithSuffix());
            MainLoadDone = true;
            _ = FilesInLoad.TryRemove(fileNameToLoad.ToUpperInvariant(), out _);
            return;
        }

        LoadCore(fileNameToLoad, freeze);
    }

    /// <summary>
    /// Prüft die Basis-Voraussetzungen zum Laden und behandelt den Konflikt-Fall
    /// (Datei wird bereits von einer anderen Instanz geladen). Fragt auch die
    /// OwnerClass ab: Ist das besitzende Objekt verworfen, wird still abgebrochen.
    /// Liefert false, wenn der Aufruf damit abgeschlossen ist.
    /// </summary>
    protected bool PrepareLoad(string info, out string fileNameToLoad) {
        fileNameToLoad = string.Empty;

        if (IsDisposed) { return false; }

        // Nicht gebunden oder Besitzer bereits verworfen: kein Laden.
        if (Owner is not IDisposableExtended { IsDisposed: false }) { return false; }

        if (string.IsNullOrEmpty(info)) { throw Develop.DebugError("Dateiname nicht angegeben!"); }
        fileNameToLoad = info.NormalizeFile();

        // Bereits geladene Datei erneut laden: No-Op.
        if (MainLoadDone && string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return false; }

        if (MainLoadDone && !string.IsNullOrEmpty(Filename)) { throw Develop.DebugError("Geladene Dateien können nicht als neue Dateien geladen werden."); }

        if (!AllowLoad(fileNameToLoad)) {
            Freeze("Datei wird bereits von einer anderen Instanz geladen");
            MainLoadDone = true;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Standard-Ablauf nach dem Setzen des Dateinamens: lädt die Kopf-Daten über
    /// LoadMainDataCore, vollzieht die Lebenszyklus-Schritte und meldet das Ergebnis.
    /// </summary>
    protected void LoadCore(string fileNameToLoad, string freeze) {
        PauseTimer();

        try {
            OnLoading();

            if (!LoadMainDataCore()) { return; }

            _ = Refresh(true);

            MainLoadDone = true;

            CheckWritePermissionAfterLoad();

            if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
            OnLoaded(true);

            if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Laden abgeschlossen: {fileNameToLoad.FileNameWithSuffix()}", 0); }
        } catch (Exception ex) {
            Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Laden von '{KeyName}' fehlgeschlagen: {ex.Message}", 0);
            Freeze("Laden fehlgeschlagen: " + ex.Message);
            MainLoadDone = true;
        } finally {
            FilesInLoad.TryRemove(fileNameToLoad.ToUpperInvariant(), out _);
            ResumeTimer();
        }
    }

    /// <summary>
    /// Lädt den neuesten vollständigen Kopf-Snapshot. Ohne Snapshot (z. B.
    /// abgebrochene Erstanlage) wird als neue Daten gestartet; noch vorhandene
    /// Fragmente lädt Refresh nach.
    /// </summary>
    private bool LoadMainDataCore() {
        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) {
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
            LastSaveMainFileUtcDate = ExtractDateFromFileName(candidate) ?? DateTime.UtcNow;

            // Merker, welche Kopf-Version der eigene Speicherstand abbildet.
            _loadedSnapshotUtcDate = LastSaveMainFileUtcDate;
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
    /// Dateiberechtigte Prüfung nach dem gelungenen Einlesen: Das Chunk-Arbeitsverzeichnis muss schreibbar sein.
    /// </summary>
    private void CheckWritePermissionAfterLoad() {
        var opr = IO.CanWriteInDirectory(BaseChunkFolder());

        if (opr.IsFailed) { Freeze(opr.FailedReason); }
    }

    #endregion

    #region Periodischer Tick

    /// <summary>
    /// Periodischer Tick: speichert nach Inaktivität oder Vielzahl von Ticks.
    /// </summary>
    public void CheckerTick() {
        if (!SaveRequired || !MaySaveNow) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        var timeSinceLastAction = Develop.GetUserIdleSeconds();

        // Speichern nach 20 Ticks bei Benutzer-Inaktivität oder spätestens nach 110 Ticks.
        var mustSave = _checkerTickCount > 20 && timeSinceLastAction > 20 || _checkerTickCount > 110;

        if (mustSave) { _ = Save(); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    #endregion

    #region Interner Lebenszyklus

    /// <summary>
    /// Setzt den Reentrancy-Zähler für Lade- und Speichervorgänge; der
    /// statische Watcher überspringt die Instanz solange.
    /// </summary>
    protected void PauseTimer() => Interlocked.Increment(ref _timerPaused);

    /// <summary>
    /// Gibt den Reentrancy-Zähler für Lade- und Speichervorgänge frei.
    /// </summary>
    protected void ResumeTimer() => Interlocked.Decrement(ref _timerPaused);

    /// <summary>
    /// Setzt den Zähler für kritische Daten-Einspielungen; in diesem Fenster
    /// führen Ansichten keine endgültigen Entfernungen in ihren Einträgen durch.
    /// </summary>
    protected void PauseDataReload() => Interlocked.Increment(ref _dataReloadPaused);

    /// <summary>
    /// Gibt den Zähler für kritische Daten-Einspielungen frei.
    /// </summary>
    protected void ResumeDataReload() => Interlocked.Decrement(ref _dataReloadPaused);

    /// <summary>
    /// Löst das Disposed-Ereignis aus.
    /// </summary>
    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Löst das Loading-Ereignis aus.
    /// </summary>
    protected virtual void OnLoading() => Loading?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Löst das Loaded-Ereignis aus.
    /// </summary>
    protected virtual void OnLoaded(bool isFirst) => Loaded?.Invoke(this, new SaveableFileLoadedEventArgs(isFirst));

    /// <summary>
    /// Löst das InvalidateView-Ereignis aus.
    /// </summary>
    protected virtual void OnInvalidateView() => InvalidateView?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Löst das RefreshDataSystems-Ereignis aus.
    /// </summary>
    protected virtual void OnRefreshDataSystems() => RefreshDataSystems?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Tick des statischen Watchers: trägt verworfene Instanzen aus und ruft
    /// CheckerTick aller geladenen, nicht gepausten Instanzen auf — dort liegt
    /// das Nachladen und das bedarfsgesteuerte Speichern. Ein Fehler im Tick
    /// wird gemeldet, darf den Prozess aber nicht beenden.
    /// </summary>
    private static void WatcherTick(object? state) {
        foreach (var instance in Instances.Keys) {
            if (instance.IsDisposed) {
                _ = Instances.TryRemove(instance, out _);
                continue;
            }

            // Während des Ladens/Speicherns und vor dem ersten Laden/Erstanlage nicht ticken.
            if (!instance.MainLoadDone || instance._timerPaused > 0) { continue; }

            try {
                instance.CheckerTick();
            } catch (Exception ex) {
                Develop.Message(ErrorType.Warning, instance, instance.KeyName, ImageCode.Tabelle, $"CheckerTick fehlgeschlagen: {ex.GetType().Name} {ex.Message} Aufrufstack={Develop.DiagStack()}", 0);
            }
        }
    }

    #endregion
}
