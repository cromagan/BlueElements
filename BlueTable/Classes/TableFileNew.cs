// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.ComponentModel;
using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Dateibasierte Tabelle im binären Fragment-Format: Alle Datei-Aktionen
/// (Öffnen, Speichern, Nachladen), die dateirelevanten Editier-Prüfungen,
/// Dateinamen-Abfragen und das neue Undo-System werden an die zugeordnete
/// SaveableFile-Instanz delegiert — je nach zugewiesener Datei-Verwaltung
/// (SaveableFileOnefile, SaveableFileFragment, SaveableFileChunk, …) in einem
/// anderen Datei-Layout. Das Layout steht im Layout-Kopf der Kopf-Datei und
/// bestimmt beim Standard-Loader die erzeugte Verwaltung.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFileNew : Table {

    #region Fields

    /// <summary>
    /// Datei-Suffix der Kopf-Datei im Fragment-Format.
    /// </summary>
    public const string SuffixMainFile = "fbdb";

    #endregion

    #region Constructors

    /// <summary>
    /// Initialisiert eine neue Tabelle im binären Fragment-Format und ordnet
    /// sie der übergebenen Datei-Verwaltung zu. Diese entscheidet über das
    /// Datei-Layout (Komplettstand, Fragment-Journal, Snapshots oder Chunks)
    /// und trägt ihre ClassId in den Layout-Kopf der Kopf-Datei.
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

        // Änderungs-Events dieser Stufe als Undo-Einträge an die Datei-Verwaltung verdrahten
        FragmentChanged += SaveableFragmentChanged;

        // Neues Format: Skripte und Unique-Definitionen werden granular über die Sammlungen geändert
        EventScript.ReadOnly = false;
        UniqueValues.ReadOnly = false;

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

        // Ansichten zum granularen Ändern freigeben
        _columnArrangements.MakeWriteable();

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
            _ = Update(source);

            // Die übernommenen Ansichten sind frisch gesperrt; zum granularen Ändern freigeben
            _columnArrangements.MakeWriteable();
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
    /// Multi-User-Zugriff ist über das Fragment-Journal bzw. Chunk-Layout möglich.
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

        // Nacharbeiten immer nach dem Loaded-Durchlauf der Ansichten.
        RepairAfterParse();

        if (e.IsFirst) { CreateWatcher(); }
    }

    /// <summary>
    /// Fordert die Ansichten zur Auffrischung.
    /// </summary>
    private void SaveableInvalidateView(object? sender, System.EventArgs e) => OnInvalidateView();

    /// <summary>
    /// Fordert die Aktualisierung der Nachschlage-Systeme.
    /// </summary>
    private void SaveableRefreshDataSystems(object? sender, System.EventArgs e) => Column.GetSystems();

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob die aktuelle Instanz als temporärer Master fungieren darf.
    /// Nur bei frischem Datenstand.
    /// </summary>
    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (Saveable is SaveableFileFragment { DataFreshEnough: false }) { return false; }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    /// <summary>
    /// Setzt die Instanz als Master, sofern die Daten aktuell genug sind; die
    /// Datei-Verwaltung erhält zusätzlich den Master-Stempel.
    /// </summary>
    public override void MasterMe() {
        if (Saveable is SaveableFileFragment { DataFreshEnough: false }) { return; }

        Saveable.MasterMe();
        base.MasterMe();
    }

    /// <summary>
    /// Speichert alle relevanten Dateien — Delegation an die Datei-Verwaltung.
    /// </summary>
    public OperationResult Save() => Saveable.Save();

    /// <summary>
    /// Öffnet und lädt die Datei — Delegation an die Datei-Verwaltung.
    /// </summary>
    /// <param name="info">Datei-Info, z. B. Dateiname oder Chunk-Name.</param>
    /// <param name="freeze">Optionaler Freeze-Grund nach dem Laden.</param>
    public void LoadFromFile(string info, string freeze) => Saveable.LoadFromFile(info, freeze);

    /// <summary>
    /// Prüft alle relevanten Dateien auf Aktualität und lädt Änderungen nach —
    /// Delegation an die Datei-Verwaltung.
    /// </summary>
    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        return Saveable.Refresh(firstTime);
    }

    /// <summary>
    /// Lädt Zeilen nach — Delegation an die Datei-Verwaltung. Je nach
    /// Datei-Layout werden andere Funktionen unterstützt.
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
    public override void TouchChunk(string chunkValue) => Saveable.Touch(chunkValue);

    /// <summary>
    /// Verteilt alle Zeilen nach einer Änderung des Chunk-Typs neu —
    /// Delegation an die Datei-Verwaltung.
    /// </summary>
    public override void ReorganizeChunks() => Saveable.Reorganize(string.Empty);

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
        if ((TableFragmentType)record.LeafType == TableFragmentType.UndoHistory) {
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
        lock (Saveable.UndoLock) {
            Saveable.Undos.Clear();
        }
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

            FragmentChanged -= SaveableFragmentChanged;

            Saveable.Dispose();
        }
    }

    /// <summary>
    /// Übergibt die Undo-Sätze einer über das Bubbling eingetroffenen Änderung der Datei-Verwaltung.
    /// </summary>
    private void SaveableFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        foreach (var record in e.UndoRecords()) { Saveable.LogUndo(record); }
    }

    /// <summary>
    /// Schreibt den kompletten Tabellenstand als tiefenbasierte Fragment-Blöcke:
    /// Der Basis-Block mit Kopf-Werten, Spalten, Ansichten, Uniques, Skripten,
    /// Kopf-Variablen und Zeilen, danach die Undo-Sektion aus der
    /// Änderungs-Historie der Datei-Verwaltung (ohne Meta).
    /// </summary>
    public override bool SerializeCore(Stream stream, int depth) {
        var allok = base.SerializeCore(stream, depth);

        allok &= Saveable.SerializeUndo(stream, depth, (byte)TableFragmentType.UndoHistory);

        return allok;
    }

    #endregion
}
