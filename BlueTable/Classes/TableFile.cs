// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using static BlueBasics.ClassesStatic.Develop;

namespace BlueTable.Classes;

[FileSuffix(".bdb")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFile : Table {

    #region Fields

    public static readonly string Chunk_MainData = "MainData";

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

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// Wird aus dem FileInfo (LastWriteTimeUtc) des CachedFile ermittelt.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var chunk = CachedFileSystem.Get<Chunk>(Filename);
            if (chunk?.FileInfo is { Exists: true } fi) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

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
            throw Develop.DebugError($"{fileName} ist kein gültiger Dateiname.");
        }

        var backup = fileName + ".bak";
        var s = Stopwatch.StartNew();

        do {
            if (IO.FileExists(fileName)) { return true; }
            Diagnose("CF", $"Recovery-Versuch {s.ElapsedMilliseconds}ms: {fileName.FileNameWithSuffix()} (chunk={chunkid}, backup={IO.FileExists(backup)})");
            Thread.Sleep(300);
            if (!IO.FileExists(backup) && s.ElapsedMilliseconds > 1000) { return false; }
        } while (s.ElapsedMilliseconds < maxWaitMs);

        if (!IO.FileExists(backup)) { return false; }

        if (IO.ReadAllBytes(backup, 5).Value is not byte[] backupBytes) {
            Develop.DebugPrint(ErrorType.Warning, $"Backup ungültig (Keine Daten), Recovery abgebrochen: {fileName.FileNameWithoutSuffix()}");
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

        IO.FileCopy(backup, fileName, false);

        Develop.DebugPrint(ErrorType.Warning, $"Backup wiederhergestellt: {fileName.FileNameWithoutSuffix()}");
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

        return CachedFileSystem.GetFileNames(path, ["*.cbdb", "*.mbdb", "*.bdb"]);
    }

    public override string IsGenericEditable(bool isloading) {
        if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }

        // Das ist eins super schnelle Prüfung, also vorziehen.
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        var opr = IO.CanWriteInDirectory(Filename.FilePath());
        if (opr.IsFailed) { return opr.FailedReason; }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (!string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        if (InitialSavePending) { return string.Empty; }

        var chunk = CachedFileSystem.Get<Chunk>(Filename);
        if (chunk is null) {
            return "Interner Chunk-Fehler bei Editier-Prüfung.";
        }
        return chunk.IsNowEditable();
    }

    public virtual void LoadFromFile(string fileNameToLoad, NeedPassword? needPassword, string freeze) {
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugError("Dateiname nicht angegeben!"); }

        fileNameToLoad = fileNameToLoad.NormalizeFile();

        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugError("Geladene Dateien können nicht als neue Dateien geladen werden."); }

        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

        TryRecoverFromBackup(fileNameToLoad, TableFile.Chunk_MainData, 120000);

        if (!CachedFileSystem.FileExists(fileNameToLoad)) {
            Freeze("Datei existiert nicht");
            if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Tabelle nicht im Dateisystem vorhanden {fileNameToLoad.FileNameWithSuffix()}", 0); }
            return;
        }

        PasswordCallback = needPassword;
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        OnLoading();

        LoadMainData();

        MainChunkLoadDone = true;
        BeSureToBeUpToDate(true);

        RepairAfterParse();

        var opr = IO.CanWriteInDirectory(fileNameToLoad.FilePath());

        if (opr.IsFailed) { Freeze(opr.FailedReason); }

        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded(true, true);

        CreateWatcher();

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Laden der Tabelle {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen", 0); }
    }

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, FormatHolder_SystemName.MakeValid(Filename), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint("Tabellenname stimmt nicht: " + Filename);
            }
        }

        base.RepairAfterParse();
    }

    public OperationResult Save() {
        if (Develop.AllReadOnly) { return OperationResult.Success; }
        if (!SaveRequired) { return OperationResult.Success; }

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere Tabelle {KeyName}", 1);

        try {
            var result = SaveInternal();
            OnInvalidateView();

            if (!string.IsNullOrEmpty(result)) { return OperationResult.Failed(result); }
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }

        return OperationResult.Success;
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

        var chunk = CachedFileSystem.Get<Chunk>(tbf.Filename);

        if (chunk is null) {
            if (IO.CreateDirectory(tbf.Filename.FilePath()).IsFailed) {
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
        var timeSinceLastAction = Develop.GetUserIdleSeconds();

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
        var chunk = CachedFileSystem.Get<Chunk>(Filename);

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
        foreach (var type in Generic.GetEnumerableOfType<TableFile>()) {
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
        if (Generic.Ending) { return; }

        lock (AllFilesLocker) {
            foreach (var thisTb in AllFiles) {
                if (thisTb is TableFile { IsDisposed: false } tbf) {
                    //if (!thisTb.LogUndo) { return true; } // Irgend ein heikler Prozess
                    if (string.IsNullOrEmpty(tbf.Filename)) { return; } // Irgend eine Tabelle wird aktuell geladen
                }

                if (!thisTb.MainChunkLoadDone) { return; }
            }
        }

        List<Table> filtered = [];
        lock (AllFilesLocker) {
            foreach (var thisTb in AllFiles) {
                if (thisTb is not TableFile { IsDisposed: false } tbf) { continue; }

                if (string.IsNullOrEmpty(tbf.Filename) ||
                    (!Chunk.IsChunkRecentlyUsed(tbf.Filename) && !thisTb.MasterNeeded)) { continue; }

                filtered.Add(thisTb);
            }
        }

        BeSureToBeUpToDate(filtered);
    }

    #endregion
}