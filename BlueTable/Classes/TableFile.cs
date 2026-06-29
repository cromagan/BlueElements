// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[FileSuffix(".bdb")]
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
    public static readonly int MyRowLost = 6;

    /// <summary>
    /// Wert in Minuten, in welchem Intervall die Tabellen auf Aktualität geprüft werden.
    /// </summary>
    public static readonly int UpdateTable = 5;

    /// <summary>
    /// Mapping von Datei-Suffix auf die zugehörige TableFile-Ableitung.
    /// Wird einmalig per Reflection aus den [FileSuffix]-Attributen aller TableFile-Ableitungen befüllt.
    /// Berücksichtigt AllowMultiple auf FileSuffixAttribute.
    /// </summary>
    internal static readonly Lazy<Dictionary<string, Type>> LoadableFileTypes = new(BuildSuffixTypeMap);

    private static readonly object _timerLock = new();
    private static int _activeTableCount;

    /// <summary>
    /// Der Globale Timer, der die Tabellen regelmäßig updated
    /// </summary>
    private static Timer? _tableUpdateTimer;

    private int _checkerTickCount = -5;

    #endregion

    #region Constructors

    public TableFile(string tablename) : base(tablename) => GenerateTableUpdateTimer();

    public TableFile(string filename, Table? source) : base(FormatHolder_SystemName.MakeValid(filename), null) {
        // Developer-Safeguard: Der Dateiname muss bereits ein gültiger Systemname sein.
        // Die Validierung beim Aufrufer (z.B. CreateTable) muss sicherstellen,
        // dass MakeValid den Basisnamen nicht verändert.
        var baseName = filename.FileNameWithoutSuffix();
        if (!string.Equals(baseName, FormatHolder_SystemName.MakeValid(baseName), StringComparison.OrdinalIgnoreCase)) {
            throw DebugError($"Dateiname '{baseName}' ist kein gültiger Systemname für eine Tabelle.");
        }

        Filename = filename.NormalizeFile();
        GenerateTableUpdateTimer();
        if (source is not null) {
            MainChunkLoadDone = true;
            InitialSavePending = true;
            SaveRequired = true;
            source.CopyTo(this);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// So viele Master of Table darf man sein
    /// </summary>
    public static int MaxMasterCount { get; set; } = 3;

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// Wird aus dem FileInfo (LastWriteTimeUtc) des CachedFile ermittelt.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var chunk = Chunk.Get(Filename);
            if (chunk?.FileInfo is { Exists: true } fi) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public virtual bool MasterNeeded => false;

    public virtual bool MultiUserPossible => false;

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
    /// Der Marker besteht aus: [0x03 DatenAllgemeinUTF8] [0xFF EOF] [3 Bytes Länge] [UTF8 "END"]
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

            if (data.Length < 8) { return false; }

            var offset = data.Length - 8;
            return data[offset] == 0x03
                && data[offset + 1] == 0xFF
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
            foreach (var thisFile in AllFiles) {
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
        if (!fileName.IsFormat(FormatHolder_FilepathAndName.Instance)) {
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
            backupBytes = backupBytes.UnzipIt();
            if (backupBytes is null) {
                //Develop.DebugPrint(ErrorType.Warning, $"Backup ungültig (Unzip Failed), Recovery abgebrochen: {fileName.FileNameWithoutSuffix()}");
                return false;
            }
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

        var suffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tblh", "mbdb", "bdb" };
        return GetFiles(path).Where(f => suffixes.Contains(f.FileSuffix())).ToArray();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ranges">Unter 5 Minuten wird auch geprüft, ob versucht wird, einen Master zu setzen. Ab 5 minuten ist es gewiss.</param>
    /// <param name="rangee">Bis 55 Minuten ist sicher, dass es der Master ist.
    /// Werden kleiner Werte abgefragt, kann ermittelt werden, ob der Master bald ausläuft.
    /// Werden größerer Werte abgefragt, kann ermittel werden, ob man Master war,
    /// </param>
    /// <param name="updateAllowed"></param>
    /// <returns></returns>
    public virtual bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (!MultiUserPossible) { return true; }
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return false; }

        if (TemporaryTableMasterUser != UserName) { return false; }
        if (TemporaryTableMasterMachine != Environment.MachineName) { return false; }

        var d = DateTimeParse(TemporaryTableMasterTimeUtc);
        var mins = DateTime.UtcNow.Subtract(d).TotalMinutes;

        ranges = Math.Max(ranges, 0);

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogeprüft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return mins > ranges && mins < rangee;
    }

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

        if (!string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        if (InitialSavePending) { return string.Empty; }

        var chunk = Chunk.Get(Filename);
        if (chunk is null) {
            return "Interner Chunk-Fehler bei Editier-Prüfung.";
        }
        return chunk.IsNowEditable();
    }

    public virtual void LoadFromFile(string fileNameToLoad, NeedPassword? needPassword, string freeze) {
        if (string.IsNullOrEmpty(fileNameToLoad)) { throw DebugError("Dateiname nicht angegeben!"); }

        fileNameToLoad = fileNameToLoad.NormalizeFile();

        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { throw DebugError("Geladene Dateien können nicht als neue Dateien geladen werden."); }

        if (!IsFileAllowedToLoad(fileNameToLoad)) {
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

        PasswordCallback = needPassword;
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
            Develop.Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Laden der Tabelle {KeyName} fehlgeschlagen: {ex.Message}", 0);
            Freeze("Laden der Tabelle fehlgeschlagen: " + ex.Message);
            MainChunkLoadDone = true;
        } finally {
        ResumeTimer();
    }
    }

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, FormatHolder_SystemName.MakeValid(Filename), StringComparison.OrdinalIgnoreCase)) {
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
    /// Diese Routine darf nur aufgerufen werden, wenn die Daten der Tabelle von der Festplatte eingelesen wurden.
    /// </summary>
    public void TryToSetMeTemporaryMaster() {
        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, true)) { return; }
        if (!NewMasterPossible()) { return; }
        MasterMe();
    }

    public void UnMasterMe() {
        Develop.EndLog($"UnMasterMe '{KeyName}': START");
        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, false)) {
            if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, true)) {
                TemporaryTableMasterUser = "Unset: " + UserName;
                TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddHours(-0.25).ToString5();
            }
        }
        Develop.EndLog($"UnMasterMe '{KeyName}': ENDE");
    }

    protected static string SaveMainFile(TableFile tbf) {
        var f = tbf.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var x = tbf.LastChange;

        // Alle Daten in einer einzigen Byte-Liste sammeln (.bdb ist nicht chunked).
        // Reihenfolge ist für den Parser irrelevant — er verarbeitet jeden Datentyp unabhängig.
        List<byte> content = new();

        content.AddRange(TableChunk.GenerateMainChunk(tbf));
        content.AddRange(TableChunk.GenerateUsesChunk(tbf));

        if (TableChunk.GenerateHeadVariableChunks(tbf) is { } varChunk) {
            content.AddRange(varChunk);
        }

        content.AddRange(TableChunk.GenerateMasterUserChunk(tbf));
        content.AddRange(TableChunk.GenerateRowChunk(tbf, true, string.Empty));
        content.AddRange(TableChunk.GenerateUndoChunk(tbf, true, string.Empty));
        content.AddRange(TableChunk.GenerateEOF());

        if (x != tbf.LastChange) { return "Tabelle wurde während der Speicherung geändert."; }

        if (content.Count < 1200) {
            tbf.Freeze("Datei zu klein für Speicherung");
            return "Datei zu klein für Speicherung.";
        }

        var chunk = Chunk.Get(tbf.Filename);

        if (chunk is null) {
            if (CreateDirectory(tbf.Filename.FilePath()).IsFailed) {
                return "Verzeichnis konnte nicht erstellt werden.";
            }
            chunk = new Chunk(tbf.Filename);
        }

        chunk.EnsureContentLoaded();

        chunk.Content = content.ToArray();

        var result = chunk.Save().GetAwaiter().GetResult();
        if (result.IsFailed) { return result.FailedReason; }

        return string.Empty;
    }

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
        }

        base.Dispose(disposing);
    }

    protected virtual bool LoadMainData() {
        var chunk = Chunk.Get(Filename);

        if (chunk is null || chunk.LoadFailed) {
            Freeze($"Laden fehlgeschlagen");
            return false;
        }

        var ok = Parse(chunk.Content, true, null);

        if (!ok) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected virtual string SaveInternal() {
        try {
            var result = SaveMainFile(this);

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

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }
        SaveRequired = true;
        return string.Empty;
    }

    private static Dictionary<string, Type> BuildSuffixTypeMap() {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in GetEnumerableOfType<TableFile>()) {
            var attrs = type.GetCustomAttributes<FileSuffixAttribute>();
            foreach (var attr in attrs) {
                if (!string.IsNullOrEmpty(attr.Suffix)) {
                    map[attr.Suffix] = type;
                }
            }
        }
        return map;
    }

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
            foreach (var thisTb in AllFiles) {
                if (thisTb is not TableFile { IsDisposed: false } tbf) { continue; }

                // Tabelle wird aktuell noch geladen — kein Reload anstoßen,
                // aber andere Tabellen davon nicht ausschließen (früher `return`).
                if (!thisTb.MainChunkLoadDone) { continue; }
                if (string.IsNullOrEmpty(tbf.Filename)) { continue; }

                // Tabelle befindet sich in einem kritischen Bereich (z.B. SaveInternal),
                // dessen Instanz-Timer pausiert ist — kein Reload anstoßen.
                if (thisTb.IsTimerPaused) { continue; }

                if (!Chunk.IsChunkRecentlyUsed(tbf.Filename) && !tbf.MasterNeeded) { continue; }

                filtered.Add(thisTb);
            }
        }

        BeSureToBeUpToDate(filtered);
    }

    private bool NewMasterPossible() {
        if (!IsAdministrator()) { return false; }

        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return false; }

        if (DateTimeTryParse(TemporaryTableMasterTimeUtc, out var dt)) {
            if (DateTime.UtcNow.Subtract(dt).TotalMinutes < MasterBlockedMax) { return false; }
            if (DateTime.UtcNow.Subtract(dt).TotalDays > 1) { return true; }
        }

        if (RowCollection.WaitDelay > 90) { return true; }

        if (MasterNeeded) { return true; }

        try {
            var masters = 0;
            List<Table> snapshot;
            lock (AllFilesLocker) {
                snapshot = [.. AllFiles];
            }

            foreach (var thisTb in snapshot) {
                if (thisTb is TableFile { IsDisposed: false, MultiUserPossible: true } tbf &&
                    tbf.AmITemporaryMaster(MasterBlockedMin, MasterCount, false)) {
                    masters++;
                    if (masters >= MaxMasterCount) { return false; }
                }
            }
        } catch {
            return false;
        }

        return true;
    }

    #endregion
}