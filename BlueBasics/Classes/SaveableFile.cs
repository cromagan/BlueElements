// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Basis-Verwaltung für ein besitzendes Objekt (IFragmentStoreable): Dateiname,
/// Öffnen, Speichern, Änderungs-Meldung und Master-Verwaltung. Die Basis kennt
/// weder den Inhaltstyp noch das Datei-Layout: Alles Empfänger-spezifische
/// (Freeze-Grund, Meldungen, Lebenszyklus) hält sie selbst als Zustand und meldet
/// Lebenszyklus-Schritte über Ereignisse. Die konkrete Vorgehensweise beim Laden,
/// Speichern und beim Verbuchen einer Änderung (Changed) entscheiden die
/// Ableitungen (Komplettstand, Fragment-Journal, Snapshots, Chunks). Das
/// Kopf-Datei-Muster stellt die Basis bereit: Die Kopf-Datei trägt einen
/// Layout-Kopf (Magic, Version, Layout-ClassId), aus dem der Loader die
/// passende Verwaltung erzeugt; die Daten liegen im Arbeitsunterordner gleichen
/// Namens ohne Suffix. Ein statischer Watcher tickt alle Instanzen periodisch
/// und antreibt deren CheckerTick (Nachladen und bedarfsgesteuertes Speichern).
/// </summary>
public abstract class SaveableFile : IDisposableExtended {

    #region Fields

    /// <summary>
    /// Magic-Kennung des Layout-Kopfs; kennzeichnet die Kopf-Datei als
    /// Fragment-Format-Ablage.
    /// </summary>
    private const string HeadFileMagic = "TFNH";

    /// <summary>
    /// Format-Version des Layout-Kopfs.
    /// </summary>
    private const byte HeadFileVersion = 1;

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

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Verwaltung; das besitzende Objekt wird über Bind() zugeordnet,
    /// sobald sein Konstruktor die Instanz übernommen hat.
    /// </summary>
    protected SaveableFile(string filename) {
        Filename = filename.NormalizeFile();
        _ = Instances.TryAdd(this, default);

        if (_watcherTimer is null) {
            var t = new Timer(WatcherTick, null, 2000, 2000);
            if (Interlocked.CompareExchange(ref _watcherTimer, t, null) is not null) { t.Dispose(); }
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Sperre für alle Zugriffe auf die Änderungs-Historie.
    /// </summary>
    public object UndoLock { get; } = new();

    /// <summary>
    /// Änderungs-Historie als Fragment-Sätze (Basis-Tiefe 1): je Eintrag eine
    /// Navigationskette plus Blatt-Satz mit Meta. Eintragsgrenze ist der Satz mit Tiefe 1.
    /// Inhaltstyp-frei; der Empfänger versteht seine eigenen Navigationstypen.
    /// </summary>
    public List<FragmentRecord> Undos { get; } = [];

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
    /// für Journal- und Meldungs-Namen.
    /// </summary>
    public string KeyName => Filename.FileNameWithoutSuffix();

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
    public virtual bool MaySaveNow => !IsFreezed && !Generic.Ending;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// </summary>
    public virtual DateTime LastSaveMainFileUtcDate => new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// True, wenn der geladene Datenstand aktuell genug ist, um Master zu werden.
    /// </summary>
    public virtual bool DataFreshEnough => true;

    /// <summary>
    /// True, wenn das Objekt vor kurzem aktiv verwendet wurde und daher in die
    /// periodische Aktualisierung einbezogen werden soll.
    /// </summary>
    public virtual bool IsRecentlyUsed => false;

    /// <summary>
    /// True, wenn das Datei-Layout Multi-User-Zugriff erlaubt. Basis: Single-User.
    /// </summary>
    public virtual bool MultiUserPossible => false;

    /// <summary>
    /// True, wenn das Datei-Layout eine Komplettierung fällig hat. Basis: nie.
    /// </summary>
    public virtual bool MasterNeeded { get; protected set; }

    /// <summary>
    /// True, wenn diese Instanz den Master-Stempel trägt.
    /// </summary>
    public bool IsMaster { get; private set; }

    /// <summary>
    /// UTC-Zeitpunkt, seit dem diese Instanz Master ist.
    /// </summary>
    public DateTime MasterSinceUtc { get; private set; } = DateTime.MinValue;

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

    #region Abstracts

    /// <summary>
    /// Die ClassId des konkreten Datei-Layouts; sie steht im Layout-Kopf der
    /// Kopf-Datei und bestimmt beim Laden die erzeugte Verwaltung.
    /// </summary>
    protected abstract string LayoutId { get; }

    /// <summary>
    /// Speichert alle relevanten Dateien.
    /// </summary>
    public abstract OperationResult Save();

    /// <summary>
    /// Prüft alle relevanten Dateien auf Aktualität und lädt Änderungen nach.
    /// </summary>
    public abstract bool Refresh(bool firstTime);

    /// <summary>
    /// Periodischer Aufruf: Nachladen und bedarfsgesteuertes Speichern.
    /// </summary>
    public abstract void CheckerTick();

    #endregion

    #region Methods

    /// <summary>
    /// Ordnet das besitzende Objekt zu. Wird von dessen Konstruktor aufgerufen.
    /// </summary>
    public void Bind(IFragmentStoreable owner) => Owner = owner;

    /// <summary>
    /// Setzt den Freeze-Grund; die Daten sind danach nur noch lesbar. Ableitungen
    /// schließen offene Schreib-Handles in einem Freeze-Override.
    /// </summary>
    public virtual void Freeze(string reason) {
        if (IsFreezed) { return; }

        FrozenReason = reason;
    }

    /// <summary>
    /// Markiert den Stand als geändert; der nächste Speichervorgang schreibt.
    /// </summary>
    public void MarkChanged() {
        SaveRequired = true;
        LastChangeUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Stempelt den Datenbereich der Info als zuletzt verwendet; steuert das
    /// Überspringen ungenutzter Bereiche bei der periodischen Aktualisierung.
    /// Basis: nichts.
    /// </summary>
    public virtual void Touch(string info) { }

    /// <summary>
    /// Nimmt einen Undo-Satz der Stufen-Kette entgegen (Navigationssatz einer Stufe oder
    /// Blatt-Satz mit Meta). Die Ableitung entscheidet, wie gesichert wird — als
    /// Fragment ins Journal oder still. Basis: still.
    /// </summary>
    public virtual void LogUndo(FragmentRecord record) { }

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
    /// Instanz aus dem Watcher-Register aus und gibt die Inhalte frei.
    /// </summary>
    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }
        if (!disposing) { return; }

        IsDisposed = true;
        _ = Instances.TryRemove(this, out _);

        OnDisposed();

        Loading = null;
        Loaded = null;
        InvalidateView = null;
        RefreshDataSystems = null;
        Disposed = null;

        DisposeContent();
    }

    /// <summary>
    /// Gibt die Inhalte frei; Ableitungen schließen hier ihre Handles.
    /// </summary>
    protected virtual void DisposeContent() { }

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
    /// Serialisiert die Änderungs-Historie: Je Eintrag (Basis-Tiefe 1) ein
    /// Steuer-Satz des Empfänger-Typs, darunter die Historien-Sätze ohne Meta;
    /// die Meta lebt nur im RAM und Journal, nie im Snapshot.
    /// </summary>
    /// <param name="stream">Ziel-Stream.</param>
    /// <param name="depth">Tiefe der Steuer-Sätze.</param>
    /// <param name="undoLeafType">Empfänger-Typ der Steuer-Sätze.</param>
    public bool SerializeUndo(Stream stream, int depth, byte undoLeafType) {
        List<FragmentRecord> undoSave;
        lock (UndoLock) {
            undoSave = [.. Undos];
        }

        var allok = true;
        foreach (var u in undoSave) {
            if (u.Depth == 1) {
                allok &= FragmentRecord.Append(stream, depth, undoLeafType, string.Empty);
            }

            allok &= u.WithoutMeta().Append(stream, u.Depth + depth);
        }

        return allok;
    }

    /// <summary>
    /// Liest die Sätze eines Undo-Blocks und verbucht sie in der Änderungs-Historie;
    /// sie werden auf Basis-Tiefe zurückgeschoben.
    /// </summary>
    /// <param name="stream">Quell-Stream, positioniert hinter dem Steuer-Satz.</param>
    /// <param name="navDepth">Tiefe des Steuer-Satzes.</param>
    public void DeserializeUndo(Stream stream, int navDepth) {
        lock (UndoLock) {
            foreach (var undoRecord in FragmentRecord.ReadEntryRecords(stream, navDepth)) {
                Undos.Add(undoRecord);
            }
        }
    }

    /// <summary>
    /// True, wenn die Datei geladen werden darf; reserviert sie für diesen
    /// Ladevorgang. Läuft bereits ein Ladevorgang derselben Datei, liefert die
    /// Methode false, und der Aufrufer bricht still ab.
    /// </summary>
    public virtual bool AllowLoad(string filename) => FilesInLoad.TryAdd(filename.ToUpperInvariant(), default);

    /// <summary>
    /// True, wenn diese Instanz temporärer Master werden darf. Basis: Datenstand
    /// frisch und nicht eingefroren.
    /// </summary>
    public virtual bool AmITemporaryMaster(bool updateAllowed) => DataFreshEnough && !IsFreezed;

    /// <summary>
    /// Setzt den Master-Stempel dieser Instanz.
    /// </summary>
    public virtual void MasterMe() {
        IsMaster = true;
        MasterSinceUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Versucht, diese Instanz zum Master zu machen, wenn sie es noch nicht ist
    /// und die Daten aktuell genug sind.
    /// </summary>
    public virtual void TryToSetMeTemporaryMaster() {
        if (!IsMaster && AmITemporaryMaster(false)) { MasterMe(); }
    }

    /// <summary>
    /// Lädt die Datenbereiche der Info nach; je nach Datei-Layout werden andere
    /// Funktionen unterstützt. Basis: alles geladen.
    /// </summary>
    public virtual bool EnsureDataLoaded(bool oldest, int count, string info) => true;

    /// <summary>
    /// Stellt sicher, dass der Datenbereich des Chunk-Werts geladen ist.
    /// Basis: nichts nachzuladen.
    /// </summary>
    public virtual OperationResult EnsureLoaded(string chunk, bool trustProcessedFile) => OperationResult.Success;

    /// <summary>
    /// Liefert den dateirelevanten Grund, warum der Datenbereich des Chunk-Werts
    /// aktuell nicht editierbar ist; leer, wenn bearbeitbar.
    /// </summary>
    public virtual string IsEditable(string chunk) => string.Empty;

    /// <summary>
    /// Tiefe Prüfung auf Dateiebene vor einer Bearbeitung des Datenbereichs des
    /// Chunk-Werts (lädt z. B. den betroffenen Bereich nach und prüft Sperren).
    /// Basis: nichts zu tun.
    /// </summary>
    public virtual string PrepareEdit(string chunk) => string.Empty;

    /// <summary>
    /// Verteilt die Datenbereiche nach einer Layout-Änderung neu.
    /// Basis: nichts zu tun.
    /// </summary>
    public virtual void Reorganize(string chunk) { }

    /// <summary>
    /// Liefert die dateirelevanten Gründe, warum das Objekt aktuell nicht
    /// editierbar ist; leer, wenn bearbeitbar.
    /// </summary>
    public virtual string EditableError(bool isloading) {
        if (IsFreezed) { return FrozenReason; }

        // Eine frische Erstanlage ist vollständig im Speicher; der erste Save()
        // erzeugt die Datei und setzt MainLoadDone.
        if (!isloading && !MainLoadDone && !InitialSavePending) { return "Laden noch nicht abgeschlossen"; }

        return string.Empty;
    }

    /// <summary>
    /// Liest den Objekt-Baum aus dem übergebenen Inhalt. Basis: Das besitzende
    /// Objekt liest als Fragment-Baum; Ableitungen mit anderem Inhalt
    /// überschreiben den Seam. Vorab wird der Erzeuger-Stempel aus den
    /// Kopf-Sätzen erfasst.
    /// </summary>
    protected virtual bool ParseMainContent(Stream stream) {
        ReadCreationStamp(stream);

        return Owner.Deserialize(stream, 1);
    }

    /// <summary>
    /// Erzeugt den zu speichernden Inhalt. Basis: Das besitzende Objekt
    /// schreibt als Fragment-Baum; Ableitungen mit anderem Inhalt überschreiben
    /// den Seam. Liefert eine leere Liste, wenn der Inhalt ungültig ist.
    /// Vor dem Objekt-Baum wird der Erzeuger-Stempel geschrieben: beim ersten
    /// Speichern wird gestempelt, danach bleibt der Stempel der geladenen Datei erhalten.
    /// </summary>
    protected virtual List<byte> GenerateContentToSave() {
        using var ms = new MemoryStream();

        _ = FragmentRecord.Append(ms, 1, FragmentTagType.CreateUser, _createUser is { Length: > 0 } u ? u : Generic.UserName);
        _ = FragmentRecord.Append(ms, 1, FragmentTagType.CreateTimeUtc, _createTimeUtc > DateTime.MinValue ? _createTimeUtc : DateTime.UtcNow);

        return Owner.Serialize(ms, 1) ? [.. ms.ToArray()] : [];
    }

    /// <summary>
    /// Liest Erzeuger-Stempel (CreateUser, CreateTimeUtc) aus den Kopf-Sätzen des
    /// Streams und setzt die Stream-Position danach zurück, damit der Inhalt
    /// unverändert weitergelesen werden kann.
    /// </summary>
    private void ReadCreationStamp(Stream stream) {
        var start = stream.Position;
        string? createUser = null;
        var createTimeUtc = DateTime.MinValue;

        while (FragmentRecord.TryReadNext(stream) is { } record) {
            if (!record.IsTag) { break; }

            switch (record.TagType) {
                case FragmentTagType.CreateUser:
                    createUser = record.StringContent;
                    break;
                case FragmentTagType.CreateTimeUtc:
                    createTimeUtc = record.DateTimeContent;
                    break;
            }
        }

        stream.Position = start;

        if (createUser is { Length: > 0 } u) {
            _createUser = u;
            _createTimeUtc = createTimeUtc;
        }
    }

    /// <summary>
    /// Stellt sicher, dass die Kopf-Datei existiert und den Layout-Kopf (Magic,
    /// Version, Layout-ClassId) trägt. Fehlt sie oder ist sie leer (abgebrochene
    /// Erstanlage), wird sie neu geschrieben; ein abweichender Inhalt bleibt
    /// unangetastet und führt beim Laden zu einer klaren Fehlermeldung.
    /// </summary>
    protected void EnsureHeadFileExists() {
        if (Filename is not { Length: > 0 }) { return; }
        if (IO.FileExists(Filename) && IO.ReadAllBytes(Filename, 5).Value is byte[] { Length: > 0 }) { return; }

        WriteHeadFile(Filename, LayoutId);
    }

    /// <summary>
    /// Schreibt die Kopf-Datei mit dem Layout-Kopf: Magic-Kennung,
    /// Format-Version und Layout-ClassId. Ein bestehender Inhalt wird überschrieben.
    /// </summary>
    public static void WriteHeadFile(string filename, string classId) {
        if (filename is not { Length: > 0 }) { throw Develop.DebugError("Dateiname für die Kopf-Datei fehlt."); }
        if (classId is not { Length: > 0 }) { throw Develop.DebugError("ClassId für die Kopf-Datei fehlt."); }

        var idBytes = Encoding.UTF8.GetBytes(classId);
        if (idBytes.Length > 255) { throw Develop.DebugError("ClassId ist zu lang (max. 255 Bytes)."); }

        var head = new byte[6 + idBytes.Length];
        Encoding.ASCII.GetBytes(HeadFileMagic).CopyTo(head, 0);
        head[4] = HeadFileVersion;
        head[5] = (byte)idBytes.Length;
        idBytes.CopyTo(head, 6);

        var opr = IO.WriteAllBytes(filename, head);
        if (opr.IsFailed) { throw Develop.DebugError("Kopf-Datei konnte nicht geschrieben werden: " + opr.FailedReason); }
    }

    /// <summary>
    /// Liest die Layout-ClassId aus dem Kopf der Kopf-Datei. False bei fehlender,
    /// leerer oder ungültiger Datei (z. B. Bestand vor dem Kopf-Datei-Muster).
    /// </summary>
    public static bool TryReadHeadClassId(string filename, out string classId) {
        classId = string.Empty;

        if (filename is not { Length: > 0 }) { return false; }
        if (IO.ReadAllBytes(filename, 5).Value is not byte[] bytes || bytes.Length < 6) { return false; }

        if (Encoding.ASCII.GetString(bytes, 0, 4) != HeadFileMagic) { return false; }
        if (bytes[4] != HeadFileVersion) { return false; }

        var len = bytes[5];
        if (len == 0 || bytes.Length < 6 + len) { return false; }

        classId = Encoding.UTF8.GetString(bytes, 6, len);
        return classId is { Length: > 0 };
    }

    /// <summary>
    /// True, wenn die Kopf-Datei einen gültigen Layout-Kopf trägt; Bestände vor
    /// dem Kopf-Datei-Muster liefern false.
    /// </summary>
    protected static bool HasHeadClassId(string filename) => TryReadHeadClassId(filename, out _);

    #endregion

    #region Laden

    /// <summary>
    /// Öffnet und lädt die Ablage — einheitlich für alle Datei-Layouts: Die
    /// Kopf-Datei wird mit Layout-Kopf sichergestellt, Bestände mit fremdem
    /// Inhalt ohne gültigen Layout-Kopf werden abgelehnt, und der Datenstand
    /// kommt aus dem Arbeitsverzeichnis über LoadMainDataCore des Layouts.
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
    /// Standard-Ablauf nach dem Setzen des Dateinamens: lädt die Hauptdaten über
    /// LoadMainDataCore, vollzieht die Lebenszyklus-Schritte und meldet das Ergebnis.
    /// </summary>
    protected void LoadCore(string fileNameToLoad, string freeze) {
        PauseTimer();

        try {
            OnLoading();

            if (!LoadMainDataCore()) { return; }

            _ = Refresh(true);

            MainLoadDone = true;

            CheckWritePermissionAfterLoad(fileNameToLoad);

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
    /// Lädt die Hauptdaten; false bricht den Ladevorgang ab.
    /// </summary>
    protected abstract bool LoadMainDataCore();

    /// <summary>
    /// Dateiberechtigte Prüfung nach dem gelungenen Einlesen; Basis: nichts.
    /// </summary>
    protected virtual void CheckWritePermissionAfterLoad(string filename) { }

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
    /// das Nachladen und das bedarfsgesteuerte Speichern.
    /// </summary>
    private static void WatcherTick(object? state) {
        foreach (var instance in Instances.Keys) {
            if (instance.IsDisposed) {
                _ = Instances.TryRemove(instance, out _);
                continue;
            }

            // Während des Ladens/Speicherns und vor dem ersten Laden/Erstanlage nicht ticken.
            if (!instance.MainLoadDone || instance._timerPaused > 0) { continue; }

            instance.CheckerTick();
        }
    }

    #endregion
}
