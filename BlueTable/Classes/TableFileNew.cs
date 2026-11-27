// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.EventArgs;
using System.ComponentModel;
using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Dateibasierte Tabelle im binären Fragment-Format: Alle Datei-Aktionen
/// (Öffnen, Speichern, Nachladen), die dateirelevanten Editier-Prüfungen,
/// Dateinamen-Abfragen und das neue Undo-System werden an die zugeordnete
/// SaveableFile-Instanz delegiert (Kopf-Fragment-Journal mit Komplettierung
/// und Chunk-Verteilung). Der Chunk-Pfad der Verwaltung folgt der Chunk-Spalte:
/// Nur mit gesetzter Chunk-Spalte verteilen sich die Zeilen je Chunk-Wert,
/// ohne läuft alles im Kopf-Fragment. Der Loader prüft den Layout-Kopf der
/// Kopf-Datei, bevor die Verwaltung erzeugt wird.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFileNew : Table {

    #region Fields

    /// <summary>
    /// Datei-Suffix der Kopf-Datei im Fragment-Format.
    /// </summary>
    public const string SuffixMainFile = "btb";



    /// <summary>
    /// Wert in Minuten. Die Master-Prüfung wird höchstens in diesem Intervall
    /// durchgeführt, sofern man Master werden kann.
    /// </summary>
    private const int MasterCheckIntervalMinutes = 15;

    /// <summary>
    /// UTC-Zeitpunkt der letzten Master-Prüfung.
    /// </summary>
    private DateTime _lastMasterAttemptUtc = DateTime.MinValue;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialisiert eine neue Tabelle im binären Fragment-Format und ordnet
    /// sie der übergebenen Datei-Verwaltung zu. Diese trägt ihre ClassId in
    /// den Layout-Kopf der Kopf-Datei.
    /// </summary>
    /// <param name="tablename">Name der Tabelle.</param>
    /// <param name="saveable">Die Datei-Verwaltung für Öffnen, Speichern, Nachladen und Undo.</param>
    public TableFileNew(string tablename, SaveableFile saveable) : base(tablename) {
        Saveable = saveable;
        saveable.Bind(this);
        saveable.DropMessages = DropMessages;

        // Lebenszyklus der Datei-Verwaltung klassisch an die Tabelle verdrahten
        saveable.Loading += SaveableLoading;
        saveable.Loaded += SaveableLoaded;
        saveable.InvalidateView += SaveableInvalidateView;
        saveable.RefreshDataSystems += SaveableRefreshDataSystems;

        // Chunk-Zuordnung: Die Verwaltung kennt genau EINEN Wert — den
        // Chunk-Pfad. Er wird nur gesetzt, wenn eine Chunk-Spalte gesetzt ist;
        // ohne Chunk-Spalte läuft alles als Kopf-Fragment. Das Nachziehen und
        // Entfernen regelt SyncChunkPath anhand der aktuellen Spaltenlage.
        SyncChunkPath(false);

        // Entfernen und Verwerfen von Spalten erkennt die Tabelle über die
        // Spalten-Events; ist die Chunk-Spalte betroffen, wird sofort
        // reorganisiert.
        Column.ColumnRemoved += ColumnCollectionColumnRemoved;
        Column.ColumnDisposed += ColumnCollectionColumnDisposed;

        // Änderungs-Events dieser Stufe als Undo-Einträge an die Datei-Verwaltung verdrahten
        FragmentChanged += SaveableFragmentChanged;

        // Kopf-Variablen als Fragment-Kind verdrahten (Navigationssatz = SubVariableCollection, leerer Key)
        this.Register(_variables);

        // Skript-Sammlung als Fragment-Kind verdrahten; ihre Journal-Events tragen
        // den Scripts-Navigationssatz (Key = Skriptname) bereits selbst, die Tabelle leitet durch.
        this.Register(EventScript);

        // Unique-Sammlung als Fragment-Kind verdrahten; ihre Journal-Events tragen
        // den Uniques-Navigationssatz (Key = Definitionsschlüssel) bereits selbst, die Tabelle leitet durch.
        this.Register(UniqueValues);

        // Ansichten-Sammlung als Fragment-Kind verdrahten; ihre Journal-Events tragen
        // den ColumnArrangements-Navigationssatz (Key = Ansichtsname) bereits selbst, die Tabelle leitet durch.
        this.Register(_columnArrangements);

        // Spalten-Sammlung durchreichen: Die Sammlung verdrahtet ihre Spalten
        // selbst mit dem SubColumn-Navigationssatz, die Tabelle ergänzt kein eigenes Segment.
        this.Register(Column);

        // Zeilen-Sammlung durchreichen: Die Sammlung verdrahtet ihre Zeilen
        // selbst mit dem SubRow-Navigationssatz, die Tabelle ergänzt kein eigenes Segment.
        this.Register(Row);

        // Eine neue Tabelle ist vollständig im Speicher; der Datei-Stand entsteht mit der Erstanlage.
        MainChunkLoadDone = true;
    }

    /// <summary>
    /// Initialisiert eine neue Tabelle im binären Fragment-Format als Kopie
    /// einer bestehenden Tabelle.
    /// </summary>
    /// <param name="filename">Dateiname der neuen Tabelle.</param>
    /// <param name="source">Quelltabelle, deren Daten kopiert werden.</param>
    /// <param name="saveable">Die Datei-Verwaltung für Öffnen, Speichern, Nachladen und Undo.</param>
    public TableFileNew(string filename, Table? source, SaveableFile saveable) : this(filename, saveable) {
        // Developer-Safeguard: Der Dateiname muss bereits ein gültiger Systemname sein.
        var baseName = filename.FileNameWithoutSuffix();
        if (!string.Equals(baseName, BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(baseName), StringComparison.OrdinalIgnoreCase)) {
            throw Develop.DebugError($"Dateiname '{baseName}' ist kein gültiger Systemname für eine Tabelle.");
        }

        if (source is not null) {
            // Update übernimmt die Ansichten und schaltet sie zum granularen Ändern frei
            _ = Update(source);

            // Die Chunk-Spalte der Quelle bestimmt die Chunk-Verteilung der neuen Datei.
            SyncChunkPath(false);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Die zugeordnete Datei-Verwaltung. Sie verwaltet alle Datei-Aktionen.
    /// </summary>
    public SaveableFile Saveable { get; }

    /// <summary>
    /// Dateiname der Hauptdatei — Abfrage an die Datei-Verwaltung.
    /// </summary>
    public string Filename => Saveable.Filename;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC) —
    /// Abfrage an die Datei-Verwaltung.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate => Saveable.LastSaveMainFileUtcDate;

    /// <summary>
    /// True, wenn eine Komplettierung fällig ist — Abfrage an die Datei-Verwaltung.
    /// </summary>
    public override bool MasterNeeded => Saveable.MasterNeeded;

    /// <summary>
    /// Neues Format: Die Sortier-Definition läuft granular über ihre Blatt-Sätze.
    /// </summary>
    protected internal override bool IsLegacyFormat => false;

    /// <summary>
    /// Multi-User-Zugriff ist über das Fragment-Journal möglich.
    /// </summary>
    public override bool MultiUserPossible => Saveable?.MultiUserPossible ?? false;

    /// <summary>
    /// True, während die Datei-Verwaltung gerade Daten neu einspielt — die
    /// Lade-/Speicher-Logik steckt vollständig im Datasystem, dieses Fenster
    /// dient der Ansicht als Halte-Signal.
    /// </summary>
    public override bool IsDataReloading => Saveable.IsDataReloading;

    #endregion

    #region Datei-Verwaltung verdrahten

    /// <summary>
    /// Meldet den Beginn eines Ladevorgangs der Datei-Verwaltung.
    /// </summary>
    private void SaveableLoading(object? sender, System.EventArgs e) => OnLoading();

    /// <summary>
    /// Meldet den Abschluss eines Ladevorgangs: Der Lade-Marker der Tabelle wird
    /// gesetzt, die Tabelle führt ihre Nacharbeiten aus (RepairAfterParse) und
    /// startet beim Erstladen ihren eigenen Watcher (Skript-Tick).
    /// </summary>
    private void SaveableLoaded(object? sender, SaveableFileLoadedEventArgs e) {
        MainChunkLoadDone = true;

        // Erstladen = die Tabelle selbst wurde deserialisiert, also Kopf betroffen;
        // alle weiteren Läufe spielen nur Zeilen-/Zell-Daten nach.
        OnLoaded(e.IsFirst, e.IsFirst);

        // Nacharbeiten immer nach dem Loaded-Durchlauf der Ansichten. Stumm:
        // Reparaturen sind keine Benutzer-Änderungen und dürfen weder Undo-Journal-
        // Einträge erzeugen noch als Änderung beim Nachspielen fremder Journale landen.
        SuppressEvents();
        try {
            RepairAfterParse();
        } finally {
            ResumeEvents();
        }

        // Der Chunk-Pfad folgt der geladenen Chunk-Spalte; ohne Chunk-Spalte
        // läuft alles im Kopf-Fragment. Beim Laden wird bewusst nicht
        // reorganisiert — die Chunk-Ordner sind gerade die Datenquelle.
        SyncChunkPath(false);

        if (e.IsFirst) { CreateWatcher(); }
    }

    /// <summary>
    /// Fordert die Ansichten zur Auffrischung.
    /// </summary>
    private void SaveableInvalidateView(object? sender, System.EventArgs e) => OnInvalidateView();

    /// <summary>
    /// Fordert die Aktualisierung der Nachschlage-Systeme; anschließend folgt
    /// der Chunk-Pfad der aktuellen Chunk-Spalten-Lage — eine übernommene
    /// Änderung an der Chunk-Spalte (z. B. deren Entfernung) reorganisiert sofort.
    /// Während des ersten Ladens wird nur der Pfad angepasst, nicht
    /// reorganisiert — die Chunk-Ordner sind gerade die Datenquelle.
    /// </summary>
    private void SaveableRefreshDataSystems(object? sender, System.EventArgs e) => SyncChunkPath(Saveable.MainLoadDone);

    /// <summary>
    /// Meldet das Entfernen einer Spalte; der Chunk-Pfad folgt sofort der
    /// neuen Lage — ist die Chunk-Spalte betroffen, wird reorganisiert.
    /// </summary>
    private void ColumnCollectionColumnRemoved(object? sender, System.EventArgs e) => SyncChunkPath(true);

    /// <summary>
    /// Meldet das Verwerfen einer Spalte; der Chunk-Pfad folgt sofort der
    /// neuen Lage — ist die Chunk-Spalte betroffen, wird reorganisiert.
    /// </summary>
    private void ColumnCollectionColumnDisposed(object? sender, ColumnEventArgs e) => SyncChunkPath(true);

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob die aktuelle Instanz als temporärer Master fungieren darf.
    /// Nur bei frischem Datenstand.
    /// </summary>
    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (!Saveable.DataFreshEnough) { return false; }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    /// <summary>
    /// Setzt die Instanz als Master, sofern die Daten aktuell genug sind; die
    /// Datei-Verwaltung erhält zusätzlich den Master-Stempel.
    /// </summary>
    public override void MasterMe() {
        if (!Saveable.DataFreshEnough) { return; }

        Saveable.MasterMe();
        base.MasterMe();
    }

    /// <summary>
    /// Stößt das Speichern aller relevanten Dateien Fire-and-Forget im
    /// Hintergrund an — Delegation an die Datei-Verwaltung. Vorab läuft die
    /// Reparatur, damit auch der erste Speicherstand bereits im reparierten
    /// Zustand vorliegt und Laden/Speichern denselben Inhalt erzeugen.
    /// </summary>
    public OperationResult Save() {
        RepairAfterParse();

        return Saveable.Save();
    }

    /// <summary>
    /// Öffnet und lädt die Datei — Delegation an die Datei-Verwaltung.
    /// </summary>
    /// <param name="info">Datei-Info, z. B. Dateiname oder Chunk-Name.</param>
    /// <param name="freeze">Optionaler Freeze-Grund nach dem Laden.</param>
    public void LoadFromFile(string info, string freeze) => Saveable.LoadFromFile(info, freeze);

    /// <summary>
    /// Prüft alle relevanten Dateien auf Aktualität und lädt Änderungen nach —
    /// Delegation an die Datei-Verwaltung. Danach läuft die periodische
    /// Master-Prüfung der Tabelle.
    /// </summary>
    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        var ok = Saveable.Refresh(firstTime);

        // Master-Prüfung nur alle MasterCheckIntervalMinutes Minuten durchführen,
        // sofern man Master werden kann. Beim ersten Mal (Init) sofort prüfen.
        if (ok && (firstTime || DateTime.UtcNow.Subtract(_lastMasterAttemptUtc).TotalMinutes >= MasterCheckIntervalMinutes)) {
            _lastMasterAttemptUtc = DateTime.UtcNow;
            TryToSetMeTemporaryMaster();
        }

        return ok;
    }

    /// <summary>
    /// Lädt Zeilen nach — Delegation an die Datei-Verwaltung.
    /// </summary>
    /// <param name="oldest">True wird versucht, die ältesten Zeilen zu laden.</param>
    /// <param name="count">Die Mindestanzahl der Zeilen zum laden. -1 für alle.</param>
    public override bool LoadTableRows(bool oldest, int count) => Saveable.EnsureDataLoaded(oldest, count, string.Empty);

    /// <summary>
    /// Stellt sicher, dass die Zeile zum Chunk-Wert geladen ist —
    /// Delegation an die Datei-Verwaltung.
    /// </summary>
    /// <param name="chunkValue">Der Chunk-Wert der Zeile (Info für die Datei-Verwaltung).</param>
    /// <param name="trustProcessedFile">True: bereits verarbeitete Dateien werden als aktuell vertraut.</param>
    public override OperationResult BeSureRowIsLoaded(string chunkValue, bool trustProcessedFile) => Saveable.EnsureLoaded(chunkValue, trustProcessedFile);

    /// <summary>
    /// Prüft, ob die Tabelle aktuell bearbeitet werden kann. Zusätzlich zu den
    /// Basis-Prüfungen die dateirelevanten Abfragen der Datei-Verwaltung.
    /// </summary>
    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        return Saveable.EditableError(isloading);
    }

    /// <summary>
    /// Prüft, ob der Wert des Datenbereichs editierbar ist — die
    /// dateirelevanten Abfragen laufen über die Datei-Verwaltung.
    /// </summary>
    /// <param name="type">Der Datentyp der Änderung.</param>
    /// <param name="chunkValue">Der Chunk-Wert (Info für die Datei-Verwaltung).</param>
    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        return Saveable.IsEditable(chunkValue ?? string.Empty);
    }

    /// <summary>
    /// Tiefe Prüfung auf Dateiebene vor einer Bearbeitung — Delegation an die
    /// Datei-Verwaltung (lädt z. B. den Chunk nach und prüft Sperren).
    /// </summary>
    protected override string PrepareForEdit(TableDataType type, string? chunkValue) => Saveable.PrepareEdit(chunkValue ?? string.Empty);

    /// <summary>
    /// Setzt den LastUsed-Stempel des Datenbereichs — Delegation an die
    /// Datei-Verwaltung.
    /// </summary>
    /// <param name="chunkValue">Der Chunk-Wert (Info für die Datei-Verwaltung).</param>
    public override void TouchChunk(string chunkValue) => Saveable.TouchChunk(chunkValue);

    /// <summary>
    /// Verteilt alle Zeilen nach einer Änderung der Chunk-Spalte neu: den
    /// Chunk-Pfad an die neue Lage anpassen und die alte Verteilung räumen —
    /// der nächste Speichervorgang schreibt den angepassten Stand.
    /// </summary>
    public override void ReorganizeChunks() {
        SyncChunkPath(false);
        Saveable.Reorganize();
    }

    /// <summary>
    /// Gleicht den Chunk-Pfad der Datei-Verwaltung an die Chunk-Spalte an: Nur
    /// mit gesetzter Chunk-Spalte werden die Zeilen je Chunk-Wert verteilt;
    /// ohne Chunk-Spalte speichert die Verwaltung alles im Kopf-Fragment.
    /// </summary>
    /// <param name="reorganizeOnChange">True: bei einem Pfad-Wechsel sofort
    /// reorganisieren (Chunk-Ordner räumen; die Zeilen liegen zuvor vollständig
    /// im Speicher); False: nur den Pfad setzen.</param>
    private void SyncChunkPath(bool reorganizeOnChange) {
        if (IsDisposed) { return; }

        Column.GetSystems();

        List<byte> path = Column.ChunkValueColumn is { IsDisposed: false }
            ? [(byte)TableFragmentType.Rows]
            : [];

        // Der Pfad ist nur leer oder genau [Rows]; eine Änderung zeigt sich
        // an der Länge bzw. dem einzigen Wert.
        var changed = Saveable.ChunkPath.Count != path.Count
            || (path.Count == 1 && Saveable.ChunkPath[0] != path[0]);

        if (!changed) { return; }

        Saveable.ChunkPath = path;

        if (reorganizeOnChange) { Saveable.Reorganize(); }
    }

    /// <summary>
    /// Friert die Tabelle ein; der Freeze-Grund und das Schließen der Schreib-Handles
    /// übernimmt die Datei-Verwaltung, die Tabelle setzt zusätzlich den eigenen Zustand.
    /// </summary>
    public override void Freeze(string reason) {
        Saveable.Freeze(reason);
        base.Freeze(reason);
    }

    /// <summary>
    /// Setzt einen Wert und bringt den Zeitstempel vor der Pipeline auf den
    /// eindeutigen Abstand der Datei-Verwaltung, damit alle Undo-Sätze der
    /// Änderung sortierbar bleiben.
    /// </summary>
    public override string ChangeData(TableDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, ChangeFlags reason) {
        if (reason.HasFlag(ChangeFlags.LogUndo)) {
            datetimeutc = Saveable.UndoStampUtc(datetimeutc);
        }

        return base.ChangeData(type, column, row, previousValue, changedTo, user, datetimeutc, comment, reason);
    }

    /// <summary>
    /// Verarbeitet den Tabellen-Block; Undo-History-Blöcke landen direkt in der
    /// Änderungs-Historie der Datei-Verwaltung.
    /// </summary>
    public override bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (record.TagType == FragmentTagType.UndoHistory) {
            if (IsDisposed) { return false; }

            Saveable.DeserializeUndo(stream, record.Depth);

            return true;
        }

        return base.DeserializeThis(stream, record);
    }

    /// <summary>
    /// Nachbereitung des Tabellenstands-Übernehmens: Die Kopie startet mit leerer
    /// Änderungs-Historie; die Undo-Sätze der Setter-Aufrufe werden verworfen.
    /// </summary>
    protected internal override void OnUpdateCompleted(Table sourceTable) {
        Saveable.ClearUndos();
    }

    /// <summary>
    /// Schreibt einen Wert: die Basis setzt den Stand im Speicher; das
    /// Journalschreiben übernimmt die Fragment-Änderung der Datei-Verwaltung.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string previousValue, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        var f = base.WriteValueToDiscOrServer(type, previousValue, value, column, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Saveable.MarkChanged();
        return string.Empty;
    }

    /// <summary>
    /// Gibt die Ressourcen frei; die Datei-Verwaltung schließt ihre Handles danach.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        base.Dispose(disposing);

        if (disposing) {
            // Fragment-Bubbling der Kinder abmelden
            this.Unregister(_variables);
            this.Unregister(EventScript);
            this.Unregister(UniqueValues);
            this.Unregister(Column);
            this.Unregister(Row);
            this.Unregister(_sortDefinition);
            this.Unregister(_columnArrangements);

            // Lebenszyklus der Datei-Verwaltung abmelden
            Saveable.Loading -= SaveableLoading;
            Saveable.Loaded -= SaveableLoaded;
            Saveable.InvalidateView -= SaveableInvalidateView;
            Saveable.RefreshDataSystems -= SaveableRefreshDataSystems;

            // Spalten-Events abmelden
            Column.ColumnRemoved -= ColumnCollectionColumnRemoved;
            Column.ColumnDisposed -= ColumnCollectionColumnDisposed;

            FragmentChanged -= SaveableFragmentChanged;

            Saveable.Dispose();
        }
    }

    /// <summary>
    /// Übergibt den Undo-Eintrag einer über das Bubbling eingetroffenen Änderung der Datei-Verwaltung.
    /// </summary>
    private void SaveableFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        Saveable.LogUndoEntry(e.UndoRecords());
    }

    #endregion
}
