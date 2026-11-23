// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFile : Table {

    #region Fields

    public static readonly string Chunk_MainData = "_MainData";

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich
    /// </summary>
    public static readonly int MasterBlockedMax = 180;

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich.
    /// Bewusst 0: TableFile selbst ist immer Single-User (MultiUserPossible = false),
    /// Unterklassen (TableChunk, TableFragments) definieren ihre eigene Master-Logik.
    /// </summary>
    public static readonly int MasterBlockedMin;

    /// <summary>
    /// Wert in Minuten. Ist man selbst Master in diesen Range, dann zählt das, dass man ein Master ist
    /// </summary>
    public static readonly int MasterCount = 150;

    /// <summary>
    /// Wert in Minuten. Solange dürfen werden und Masters willkürlich gesetzt.
    /// </summary>
    public static readonly int MasterTry = 7;

    /// <summary>
    /// Wert in Minuten. Solange gilt der gesetzte Master Wert.
    /// </summary>
    public static readonly int MasterUntil = 175;

    /// <summary>
    /// Wert in Minuten. Ab diesen Wert dürfen Master die Zeilenberechnung übernehmen
    /// </summary>
    public static readonly int MyRowLost = 15;

    /// <summary>
    /// Wert in Minuten, in welchem Intervall die Tabellen auf Aktualität geprüft werden.
    /// </summary>
    public static readonly int UpdateTable = 5;

    /// <summary>
    /// Mapping von Datei-Suffix auf die zugehörige TableFile-Ableitung.
    /// Wird einmalig beim ersten Zugriff aus der hardcoded Tabelle in
    /// BuildSuffixTypeMap erzeugt. Neue TableFile-Ableitungen
    /// müssen dort eingetragen werden.
    /// </summary>
    internal static readonly Lazy<Dictionary<string, Type>> LoadableFileTypes = new(BuildSuffixTypeMap);

    private static readonly object _timerLock = new();
    private static int _activeTableCount;

    /// <summary>
    /// Der Globale Timer, der die Tabellen regelmäßig updated
    /// </summary>
    private static Timer? _tableUpdateTimer;

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private int _checkerTickCount = -5;

    /// <summary>
    /// Sperre für alle Zugriffe auf die Undo-Historie dieser Datei.
    /// </summary>
    internal readonly object _undoLock = new();

    /// <summary>
    /// Zuletzt vergebener Zeitstempel für Undo-Log-Einträge; garantiert eindeutigen Millisekunden-Abstand.
    /// </summary>
    private DateTime _lastChangeUtc = DateTime.MinValue;

    #endregion

    #region Constructors

    public TableFile(string tablename) : base(tablename) => GenerateTableUpdateTimer();

    public TableFile(string filename, Table? source) : base(BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(filename), null) {
        // Developer-Safeguard: Der Dateiname muss bereits ein gültiger Systemname sein.
        // Die Validierung beim Aufrufer (z.B. CreateTable) muss sicherstellen,
        // dass MakeValid den Basisnamen nicht verändert.
        var baseName = filename.FileNameWithoutSuffix();
        if (!string.Equals(baseName, BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(baseName), StringComparison.OrdinalIgnoreCase)) {
            throw DebugError($"Dateiname '{baseName}' ist kein gültiger Systemname für eine Tabelle.");
        }

        Filename = filename.NormalizeFile();
        GenerateTableUpdateTimer();
        InitialSavePending = true;
        SaveRequired = true;
        if (source is not null) { _ = Update(source); }
    }

    #endregion

    #region Properties

    /// <summary>
    /// So viele Master of Table darf man sein
    /// </summary>
    public static int MaxMasterCount { get; set; } = 3;

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Klassische Undo-Historie der Datei (legacy): wird beim Speichern in die
    /// Undo-Chunks geschrieben und beim Laden wieder eingelesen.
    /// </summary>
    public List<UndoItem> Undo { get; } = [];

    /// <summary>
    /// Steuert, ob Änderungen in die klassische Undo-Historie aufgenommen werden.
    /// </summary>
    public bool LogUndo { get; set; } = true;

    /// <summary>
    /// Gibt an, ob die Tabelle vor kurzem aktiv verwendet wurde und daher
    /// in die periodische Aktualisierung (TableUpdater) einbezogen
    /// werden soll. Die Basisklasse prüft dies über die zugehörige Chunk-LiveInstance.
    /// Unterklassen mit eigener Chunk-Verwaltung (z.B. TableChunk)
    /// überschreiben dies, da sie keine Chunk-LiveInstances verwenden.
    /// </summary>
    public virtual bool IsRecentlyUsed => Chunk.IsChunkRecentlyUsed(Filename);

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// Wird aus dem FileInfo (LastWriteTimeUtc) des zugehörigen Chunk ermittelt.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var chunk = Chunk.Get(Filename);
            if (chunk?.FileInfo is { Exists: true } fi) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public override bool MasterNeeded => false;

    public override bool MultiUserPossible => false;

    /// <summary>
    /// Markiert die initiale Speicherung als abgeschlossen. Muss von SaveInternal-Ableitungen
    /// nach der ersten erfolgreichen Speicherung aufgerufen werden.
    /// </summary>
    protected bool InitialSavePending { get; set; }

    protected bool SaveRequired { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob Byte-Daten (ggf. gezippt) einen gültigen EOF-Marker enthalten.
    /// Binäres Fragment-Format: endet auf den 255-EOF-Satz (valueId FF, leerer Content, 0x0D).
    /// Classic-Format: Der Marker besteht aus
    /// [0x03 DatenAllgemeinUTF8] [0xFF EOF] [3 Bytes Länge] [UTF8 "END"].
    /// </summary>
    public static bool HasValidEofMarker(byte[] rawBytes) {
        try {
            if (rawBytes is not { Length: > 8 }) { return false; }

            byte[]? data;
            if (rawBytes.IsZipped()) {
                data = rawBytes.UnzipIt();
                if (data is null) { return false; }
            } else {
                data = rawBytes;
            }

            if (data.Length < 9) { return false; }

            if (data[0] == 0x01) { // Fragment-Format: erster Satz der Wurzel-Ebene, ohne Tab
                // 255-EOF-Satz: ... 0x20, valueId 0xFF, Content-Typ, 4 Längen-Bytes (leerer Content), 0x0D
                return data[^7] == (byte)FragmentTagType.Eof
                    && data[^6] <= FragmentRecord.MaxUserTypeValue
                    && data[^5] == 0x00
                    && data[^4] == 0x00
                    && data[^3] == 0x00
                    && data[^2] == 0x00
                    && data[^1] == 0x0D;
            }

            var offset = data.Length - 8;
            return data[offset] == 0x03
                && data[offset + 1] == (byte)FragmentTagType.Eof
                && data[offset + 2] == 0x00
                && data[offset + 3] == 0x00
                && data[offset + 4] == 0x03
                && data[offset + 5] == 0x45  // 'E'
                && data[offset + 6] == 0x4E  // 'N'
                && data[offset + 7] == 0x44; // 'D'
        } catch {
            return false;
        }
    }

    public static bool IsFileAllowedToLoad(string fileName) {
        lock (AllFilesLocker) {
            foreach (var thisFile in LiveInstances.Values) {
                if (thisFile is TableFile { IsDisposed: false } tbf) {
                    if (string.Equals(tbf.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                        //tbf.Save(false);
                        //Develop.DebugPrint("Doppletes Laden von " + fileName);
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Versucht, eine Datei aus dem Backup (.bak) wiederherzustellen.
    /// Wartet maximal <paramref name="maxWaitMs"/> auf das Erscheinen der Datei.
    /// Backup wird nur verwendet, wenn es einen gültigen EOF-Marker enthält.
    /// </summary>
    public static bool TryRecoverFromBackup(string fileName, string chunkid, int maxWaitMs) {
        if (!fileName.IsValidFilepathAndName()) {
            throw DebugError($"{fileName} ist kein gültiger Dateiname.");
        }

        var backup = fileName + ".bak";
        var s = Stopwatch.StartNew();

        do {
            if (FileExists(fileName)) { return true; }
            //Diagnose("CF",$"Recovery-Versuch {s.ElapsedMilliseconds}ms: {fileName.FileNameWithSuffix()} (chunk={chunkid}, backup={FileExists(backup)})");
            Thread.Sleep(300);
            if (!FileExists(backup) && s.ElapsedMilliseconds > 1000) { return false; }
        } while (s.ElapsedMilliseconds < maxWaitMs);

        if (!FileExists(backup)) { return false; }

        if (ReadAllBytes(backup, 5).Value is not byte[] backupBytes) {
            DebugPrint(ErrorType.Warning, $"Backup ungültig (Keine Daten), Recovery abgebrochen: {fileName.FileNameWithoutSuffix()}");
            return false;
        }

        if (backupBytes.IsZipped()) {
            if (backupBytes.UnzipIt() is not { } unzipped) { return false; }
            backupBytes = unzipped;
        }

        if (!HasValidEofMarker(backupBytes)) {
            //Develop.DebugPrint(ErrorType.Warning, $"Backup ungültig (kein EOF-Marker), Recovery abgebrochen: {fileName.FileNameWithoutSuffix()}");
            return false;
        }

        if (!Chunk.HasCheckPoint(backupBytes, chunkid)) {
            //Develop.DebugPrint(ErrorType.Warning, $"Backup ungültig (ID fehlt: {chunkid}), Recovery abgebrochen: {fileName.FileNameWithoutSuffix()}");
            return false;
        }

        FileCopy(backup, fileName, false);

        DebugPrint(ErrorType.Warning, $"Backup wiederhergestellt: {fileName.FileNameWithoutSuffix()}");
        return true;
    }

    public override string[]? AllAvailableTables(List<Table>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; }

        var path = Filename.FilePath();

        if (allreadychecked is not null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is TableFile { IsDisposed: false } tbf &&
                    string.Equals(tbf.Filename.FilePath(), path, StringComparison.Ordinal)) { return null; }
            }
        }

        var suffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {"btb", "bdb", "nbdb", "mbdb", "tblh" };
        return GetFiles(path).Where(f => suffixes.Contains(f.FileSuffix())).ToArray();
    }

    /// <summary>
    /// Prüft, warum die Tabelle aktuell nicht editierbar ist.
    /// </summary>
    public override string IsGenericEditable(bool isloading) {
        if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }

        // Das ist eins super schnelle Prüfung, also vorziehen.
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        var opr = CanWriteInDirectory(Filename.FilePath());
        if (opr.IsFailed) { return opr.FailedReason; }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (type == TableDataType.Command_AddRow && string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        if (!string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        if (InitialSavePending) { return string.Empty; }

        var chunk = Chunk.Get(Filename);
        if (chunk is null) {
            return "Interner Chunk-Fehler bei Editier-Prüfung.";
        }
        return chunk.IsNowEditable();
    }

    public virtual void LoadFromFile(string fileNameToLoad, string freeze) {
        if (string.IsNullOrEmpty(fileNameToLoad)) { throw DebugError("Dateiname nicht angegeben!"); }

        fileNameToLoad = fileNameToLoad.NormalizeFile();

        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { throw DebugError("Geladene Dateien können nicht als neue Dateien geladen werden."); }

        var allowed = IsFileAllowedToLoad(fileNameToLoad);

        if (!allowed) {
            // Eine andere Instanz lädt diese Datei bereits (Race-Condition).
            // Diese Instanz ist ein "Ghost" — ohne Freeze/MainChunkLoadDone
            // würde WaitInitialDone 120 s hängen.
            Freeze("Datei wird bereits von einer anderen Instanz geladen");
            MainChunkLoadDone = true;
            return;
        }

        TryRecoverFromBackup(fileNameToLoad, Chunk_MainData, 120000);

        if (!FileExists(fileNameToLoad)) {
            Freeze("Datei existiert nicht");
            if (!IsDisposed && DropMessages) { Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Tabelle nicht im Dateisystem vorhanden {fileNameToLoad.FileNameWithSuffix()}", 0); }
            return;
        }

        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        PauseTimer();

        try {
            OnLoading();

            LoadMainData();

            BeSureToBeUpToDate(true);

            MainChunkLoadDone = true;

            RepairAfterParse();

            var opr = CanWriteInDirectory(fileNameToLoad.FilePath());

            if (opr.IsFailed) { Freeze(opr.FailedReason); }

            if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
            OnLoaded(true, true);

            CreateWatcher();

            if (!IsDisposed && DropMessages) { Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Laden der Tabelle {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen", 0); }
        } catch (Exception ex) {
            // Schlägt das Laden fehl (z.B. Netzwerk-/IO-Fehler, Parser-Exception),
            // MUSS MainChunkLoadDone trotzdem gesetzt werden — sonst hängen alle
            // WaitInitialDone-Aufrufe anderer Threads bis zum 120-s-Timeout.
            // Freeze markiert die Tabelle als unbrauchbar, IsFreezed löst WaitInitialDone zusätzlich aus.
            Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Laden der Tabelle {KeyName} fehlgeschlagen: {ex.Message}", 0);
            Freeze("Laden der Tabelle fehlgeschlagen: " + ex.Message);
            MainChunkLoadDone = true;
        } finally {
            ResumeTimer();
        }
    }

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(Filename), StringComparison.OrdinalIgnoreCase)) {
                DebugPrint("Tabellenname stimmt nicht: " + Filename);
            }
        }

        base.RepairAfterParse();
    }

    public OperationResult Save() {
        if (AllReadOnly) { return OperationResult.Success; }
        if (!SaveRequired) { return OperationResult.Success; }

        Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere Tabelle {KeyName}", 1);

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
    /// Speichert den übergebenen Inhalt mit Backup-Rotation und
    /// Semaphore-Synchronisierung. Der Inhalt wird gezippt gespeichert und
    /// nutzt den erweiterten Speicherpfad. Der Inhalt wird NICHT im Speicher gehalten.
    /// </summary>
    public OperationResult Save(byte[] content) {
        if (IsDisposed) { return OperationResult.Failed("Objekt bereits verworfen."); }

        bool acquired = false;
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

            if (IsGenericEditable(false) is { Length: > 0 } f) {
                return OperationResult.Failed(f);
            }

            if (!HasValidEofMarker(content)) {
                return OperationResult.Failed("Kein gültiger EOF-Marker");
            }

            Message(ErrorType.DevelopInfo, this, Filename.FileNameWithSuffix(), ImageCode.Diskette, $"Speichere {KeyName}", 0);

            var contentToWrite = content.ZipIt() ?? [];
            if (contentToWrite.Length == 0) {
                return OperationResult.Failed("Komprimierung fehlgeschlagen");
            }

            var result = SaveExtended(Filename, contentToWrite);

            if (result.IsSuccessful && Chunk.Get(Filename) is { } chunk) {
                chunk.Touch();
                chunk.Invalidate();
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
    /// Erzeugt den kompletten Datei-Inhalt einer Tabelle (Head-Daten, Spalten,
    /// Zeilen, Historie, EOF). Reihenfolge ist für den Parser irrelevant —
    /// er verarbeitet jeden Datentyp unabhängig.
    /// </summary>
    protected static List<byte> GenerateFullContent(TableFile tbf) {
        List<byte> content =
        [
            .. TableChunk.GenerateMainChunk(tbf),
            .. TableChunk.GenerateUsesChunk(tbf),
            .. TableChunk.GenerateHeadVariableChunks(tbf),
            .. TableChunk.GenerateMasterUserChunk(tbf),
            .. TableChunk.GenerateRowChunk(tbf, true, string.Empty),
            .. TableChunk.GenerateUndoChunk(tbf, true, string.Empty),
            .. TableChunk.GenerateEOF(),
        ];

        return content;
    }

    /// <summary>
    /// Erzeugt den kompletten Datei-Inhalt einer Tabelle (Head-Daten, Spalten,
    /// Zeilen, Historie, EOF). Reihenfolge ist für den Parser irrelevant —
    /// er verarbeitet jeden Datentyp unabhängig.
    /// </summary>
    protected virtual List<byte> GenerateContentToSave() => GenerateFullContent(this);

    /// <summary>
    /// Mindestgröße des Datei-Inhalts; schützt vor korrupten Re-Saves (z. B. wenn
    /// durch einen Fehler kaum Daten generiert werden). Beim initialen Save einer
    /// neuen, leeren Tabelle ist der Inhalt legitim klein und darf nicht zum Freeze
    /// führen — sonst wäre die neue Tabelle sofort blockiert.
    /// </summary>
    protected virtual int MinimumContentSize => 1200;

    protected static string SaveFullFile(TableFile tbf) {
        var f = tbf.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var x = tbf.LastChange;

        // Alle Daten in einer einzigen Byte-Liste sammeln (.bdb ist nicht chunked).
        var content = tbf.GenerateContentToSave();

        if (x != tbf.LastChange) { return "Tabelle wurde während der Speicherung geändert."; }

        // Die Mindestgrößen-Prüfung schützt vor korrupten Re-Saves (z.B. wenn
        // durch einen Fehler kaum Daten generiert werden). Beim initialen Save
        // einer neuen, leeren Tabelle ist der Inhalt legitim klein und darf
        // nicht zum Freeze führen — sonst wäre die neue Tabelle sofort blockiert.
        if (!tbf.InitialSavePending && content.Count < tbf.MinimumContentSize) {
            tbf.Freeze("Datei zu klein für Speicherung");
            return "Datei zu klein für Speicherung.";
        }

        if (Chunk.Get(tbf.Filename) is null) {
            if (CreateDirectory(tbf.Filename.FilePath()).IsFailed) {
                return "Verzeichnis konnte nicht erstellt werden.";
            }
            _ = new Chunk(tbf.Filename);
        }

        var result = tbf.Save(content.ToArray());
        if (result.IsFailed) { return result.FailedReason; }

        return string.Empty;
    }

    #region Undo (klassisch)

    /// <summary>
    /// Setzt einen Wert und führt bei LogUndo die klassische Undo-Aufzeichnung aus:
    /// Der Zeitstempel wird vor der Pipeline auf eindeutigen Abstand gebracht, nach
    /// Erfolg landet die Änderung in der Undo-Historie.
    /// </summary>
    public override string ChangeData(TableDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, ChangeFlags reason) {
        // Spaltennamen VOR der Änderung sichern: Bei Umbenennungen ändert der
        // Basis-Durchlauf den Schlüssel, der Undo-Eintrag zeigt auf den alten.
        var colName = column?.KeyName ?? string.Empty;

        // Logische Uhr: Einmalig vor der gesamten Pipeline (Speicher, Undo) auf eindeutigen
        // Abstand bringen, damit alle Undo-Einträge denselben Zeitstempel erhalten.
        if (LogUndo && reason.HasFlag(ChangeFlags.LogUndo)) {
            datetimeutc = UndoStampUtc(datetimeutc);
        }

        var error = base.ChangeData(type, column, row, previousValue, changedTo, user, datetimeutc, comment, reason);

        if (!string.IsNullOrEmpty(error) || !LogUndo || !reason.HasFlag(ChangeFlags.LogUndo)) { return error; }

        AddUndo(type, colName, row, previousValue, changedTo, user, datetimeutc, comment, "[Änderung in dieser Session]");

        return string.Empty;
    }

    /// <summary>
    /// Nachbereitung des Tabellenstands-Übernehmens: Die klassische Undo-Historie
    /// wird übertragen, wenn die Quelle ebenfalls dateibasiert ist.
    /// </summary>
    protected internal override void OnUpdateCompleted(Table sourceTable) {
        if (sourceTable is not TableFile sourceFile) { return; }

        // Muss NACH dem Kopieren laufen: Die Setter-Aufrufe des Kopierens erzeugen
        // Undo-Einträge, die hier komplett durch die Undo-History der Quelle
        // ersetzt werden.
        List<UndoItem> undoSnapshot;
        lock (sourceFile._undoLock) {
            undoSnapshot = [.. sourceFile.Undo];
        }

        lock (_undoLock) {
            Undo.Clear();
            foreach (var thisUndo in undoSnapshot) {
                if (thisUndo is null) { continue; }

                // Nur Einträge des Ziel-Tabellennamens übernehmen: Bei Formatwandlung
                // unter gleichem Namen bleibt die Historie erhalten, bei einer Kopie
                // unter neuem Namen startet die Datei mit eigener, leerer Historie.
                // InjectData würde fremd benannte Einträge ohnehin überspringen.
                if (!string.Equals(thisUndo.TableName, KeyName, StringComparison.OrdinalIgnoreCase)) { continue; }

                Undo.Add(new UndoItem(thisUndo.ParseableItems().FinishParseable()));
            }
        }
    }

    /// <summary>
    /// Setzt einen Wert; Undo-Chunk-Sätze des Classic-Formats landen direkt in der Historie.
    /// </summary>
    internal protected override string SetValueInternal(TableDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, string comment, ChangeFlags changeFlags) {
        if (type == TableDataType.Undo) {
            if (IsDisposed) { return "Tabelle verworfen!"; }
            AddLoadedUndo(new UndoItem(value));
            return string.Empty;
        }

        return base.SetValueInternal(type, column, row, value, user, datetimeutc, comment, changeFlags);
    }

    /// <summary>
    /// Nimmt eine Änderung in die klassische Undo-Historie auf; sie wird beim
    /// Speichern in die Undo-Chunks geschrieben.
    /// </summary>
    private void AddUndo(TableDataType type, string column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        if (type == TableDataType.SystemValue) { return; }

        lock (_undoLock) {
            Undo.Add(new UndoItem(KeyName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container));
        }
    }

    /// <summary>
    /// Nimmt ein beim Laden gelesenes Undo-Item in die Historie auf.
    /// </summary>
    private void AddLoadedUndo(UndoItem item) {
        lock (_undoLock) {
            Undo.Add(item);
        }
    }

    /// <summary>
    /// Logische Uhr: garantiert eindeutigen Millisekunden-Abstand für Undo-Einträge.
    /// </summary>
    private DateTime UndoStampUtc(DateTime datetimeutc) {
        lock (_undoLock) {
            var t = datetimeutc;
            if (t <= _lastChangeUtc) { t = _lastChangeUtc.AddMilliseconds(1); }
            _lastChangeUtc = t;
            return t;
        }
    }

    #endregion

    protected override void Checker_Tick(object? state) {
        base.Checker_Tick(state);

        if (!SaveRequired || !LogUndo || !string.IsNullOrEmpty(IsGenericEditable(false))) {
            _checkerTickCount = 0;
            return;
        }        
        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        // Zeitliche Bedingungen prüfen
        //var timeSinceLastChange = DateTime.UtcNow.Subtract(LastChange).TotalSeconds;
        var timeSinceLastAction = GetUserIdleSeconds();

        // Bestimme ob gespeichert werden muss
        var mustSave = _checkerTickCount > 20 && timeSinceLastAction > 20 ||
                         _checkerTickCount > 110 ||
                         Column.ChunkValueColumn is not null && _checkerTickCount > 50;

        if (_checkerTickCount < 200) {
            // 200 * 2 Sekunden = 6,7 Minuten
            //if (e.Cancel && mustSave) { mustSave = false; }
            if (mustSave && RowCollection.InvalidatedRowsManager.PendingRowsCount > 0) { mustSave = false; }
        }

        // Speichern wenn nötig
        if (mustSave) { Save(); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    protected override void Dispose(bool disposing) {
        // Keine Zusatzlogik - bewusst transparent.

        if (IsDisposed) { return; }

        if (disposing) {
            if (SaveRequired && !IsFreezed) {
                _ = SaveInternal();
            }

            UnMasterMe();

            try {
                // Listen leeren
                lock (_undoLock) {
                    Undo.Clear();
                }

                // Auf laufende Speichervorgänge warten, BEVOR die Semaphore disposed wird.
                if (_saveSemaphore.Wait(30000)) {
                    _saveSemaphore.Release();
                }
            } catch { }

            try {
                // LÖSUNG: Static Timer verwalten basierend auf aktiven Table-Instanzen
                lock (_timerLock) {
                    _activeTableCount--;
                    if (_activeTableCount <= 0) {
                        _activeTableCount = 0;
                        _tableUpdateTimer?.Dispose();
                        _tableUpdateTimer = null;
                    }
                }
            } catch { }

            try {
                _saveSemaphore.Dispose();
            } catch { }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Lädt die Hauptdaten über den Classic-Parser.
    /// </summary>
    protected virtual bool LoadMainData() {
        var chunk = Chunk.Get(Filename);

        if (chunk is null || chunk.LoadFailed) {
            Freeze("Laden fehlgeschlagen");
            return false;
        }

        var data = chunk.LoadContent();

        if (!Parse(data, true, null)) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected virtual string SaveInternal() {
        try {
            var result = SaveFullFile(this);

            if (string.IsNullOrEmpty(result)) {
                InitialSavePending = false;
                SaveRequired = false;
            }

            OnInvalidateView();

            return result;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string previousValue, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        var f = base.WriteValueToDiscOrServer(type, previousValue, value, column, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }
        SaveRequired = true;
        return string.Empty;
    }

    /// <summary>
    /// Erstellt eine neue Tabellendatei im Format der Ziel-Endung und übernimmt
    /// die Daten der Quelltabelle. Prüft Zielpfad, Tabellenname und Kollisionen
    /// mit gleichnamigen Dateien anderer Tabellenformate.
    /// </summary>
    public static OperationResult Create(string targetPath, Table? source) {
        if (string.IsNullOrEmpty(targetPath)) { return OperationResult.Failed("Zielpfad ist leer."); }

        targetPath = targetPath.NormalizeFile();

        // Relative Angaben gegen das Arbeitsverzeichnis auflösen: Filename.FilePath()
        // liefert bei reinen Dateinamen sonst einen leeren Ordner und jede
        // Verzeichnis-Prüfung würde fehlschlagen.
        if (!System.IO.Path.IsPathRooted(targetPath)) { targetPath = System.IO.Path.GetFullPath(targetPath); }

        var targetSuffix = targetPath.FileSuffix().ToLowerInvariant();
        var suffixTypes = BuildSuffixTypeMap();

        if (!suffixTypes.TryGetValue("." + targetSuffix, out var targetType)) {
            return OperationResult.Failed($"Zieldatei-Erweiterung '{targetSuffix}' wird nicht unterstützt. Erlaubt: .btb, .bdb, .nbdb, .mbdb, .tblh");
        }

        var targetBase = targetPath.FileNameWithoutSuffix();
        var targetDir = targetPath.FilePath();

        // Tabellennamen müssen gültige Systemnamen sein, damit KeyName und Dateiname konsistent bleiben.
        // BlueBasics.Classes.Formats.SystemNameFormat.MakeValid darf den Namen nicht verändern.
        var validBase = BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(targetBase);
        if (!string.Equals(targetBase, validBase, StringComparison.OrdinalIgnoreCase) || !IsValidTableName(validBase)) {
            return OperationResult.Failed($"Der Name '{targetBase}' ist als Tabellenname ungültig.\r\nNur Buchstaben, Zahlen und Unterstriche erlaubt (z.B. '{validBase}').\r\nReservierte Präfixe: SYS_, BAK_, DATABASE, TABLE.");
        }

        foreach (var ext in suffixTypes.Keys) {
            if (string.Equals(ext.TrimStart('.'), targetSuffix, StringComparison.OrdinalIgnoreCase)) { continue; }
            var collisionFile = targetDir + targetBase + ext;
            if (FileExists(collisionFile)) {
                return OperationResult.Failed($"Im Zielverzeichnis existiert bereits eine Datei mit dem gleichen Namen aber anderer Erweiterung: {collisionFile}");
            }
        }

        if (FileExists(targetPath)) { return OperationResult.Failed($"Die Datei existiert bereits: {targetPath}"); }

        var target = (TableFile?)Activator.CreateInstance(targetType, targetPath, source);

        if (target is null) { return OperationResult.Failed($"Tabellentyp für '.{targetSuffix}' konnte nicht erstellt werden."); }

        return target.Save();
    }

    private static Dictionary<string, Type> BuildSuffixTypeMap() => new(StringComparer.OrdinalIgnoreCase) {
        [".btb"] = typeof(TableFileNew),
        [".bdb"] = typeof(TableFile),
        [".nbdb"] = typeof(TableFile),
        [".mbdb"] = typeof(TableFragments),
        [".tblh"] = typeof(TableChunk)
    };

    private static void GenerateTableUpdateTimer() {
        lock (_timerLock) {
            _activeTableCount++;

            if (_tableUpdateTimer is not null) { return; }

            _tableUpdateTimer = new Timer(TableUpdater, null, 10000, UpdateTable * 60 * 1000);
        }
    }

    private static void TableUpdater(object? state) {
        if (Ending) { return; }

        List<Table> filtered = [];
        lock (AllFilesLocker) {
            foreach (var thisTb in LiveInstances.Values) {
                if (thisTb is not TableFile { IsDisposed: false } tbf) { continue; }

                // Tabelle wird aktuell noch geladen — kein Reload anstoßen,
                // aber andere Tabellen davon nicht ausschließen (früher `return`).
                if (!thisTb.MainChunkLoadDone) { continue; }
                if (string.IsNullOrEmpty(tbf.Filename)) { continue; }

                // Tabelle befindet sich in einem kritischen Bereich (z.B. SaveInternal),
                // dessen Instanz-Timer pausiert ist — kein Reload anstoßen.
                if (thisTb.IsTimerPaused) { continue; }

                if (!tbf.IsRecentlyUsed && !tbf.MasterNeeded) { continue; }

                filtered.Add(thisTb);
            }
        }

        BeSureToBeUpToDate(filtered);
    }

    #endregion
}