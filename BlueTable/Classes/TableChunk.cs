// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using static BlueTable.Classes.Chunk;
using static BlueBasics.Interfaces.IHasKeyNameExtension;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ mit Lite-Hauptdatei (.tblh, Head + CheckPoint + EOF, keine Nutzdaten)
/// und Row-Chunks in eigenen Unterordnern mit Benutzer-spezifischen .tblc-Dateien.
/// Jeder Benutzer schreibt in seine eigene Datei (yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc),
/// wobei Hash ein 3-stelliger Wert aus MachineName und Instanz-ID ist. So werden auch
/// verschiedene Maschinen/Instanzen bei gleichem Benutzernamen unterschieden.
/// Das Datum im Dateinamen (UTC) ist der alleinige Vergleich für Aktualität.
/// Dateien älter als eine Stunde werden gelöscht (wenn mehrere vorhanden).
/// Edit-Sperre: Neueste Datei &lt;10 Min → nur der Ersteller darf bearbeiten.
/// <para>
/// Im Gegensatz zur regulären Chunk-Verwaltung werden Chunks hier NICHT über das
/// <see cref="BlueBasics.Classes.FileSystemCaching.CachedFileSystem"/> verwaltet.
/// Geladene Chunks werden nach dem Parsen sofort verworfen — nur der Dateiname
/// wird für Aktualitätsprüfungen gemerkt. Jeder Chunk ist ein Einweg (write-once):
/// eine einmal gespeicherte Datei wird nie wieder überschrieben. Vor dem Parsen
/// wird <see cref="TableFile.HasValidEofMarker"/> geprüft, damit nur komplett
/// gespeicherte Chunks eingelesen werden.
/// </para>
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].tblh                                                       (Lite-Chunk, keine Nutzdaten)
///   [TableName]\[ChunkID]\yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc       (Row-Chunk pro Chunk)
/// </remarks>
[Browsable(false)]
[FileSuffix(".tblh")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunk : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Stunden. Benutzer-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens FileCount neuere Dateien im gleichen Ordner existieren).
    /// </summary>
    public const int DeleteOldFilesAfterHours = 2;

    /// <summary>
    /// Wert in Minuten. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public const int EditLockMinutes = 5;

    public static readonly string Chunk_AdditionalUseCases = "_uses";

    /// <summary>
    /// Chunk-ID für die Lite-Hauptdatei (.tblh). Enthält nur Head + CheckPoint + EOF,
    /// keine Nutzdaten. Die eigentlichen Daten liegen in den Row-Chunks (.tblc).
    /// </summary>
    public static readonly string Chunk_MainDataLite = "_MainDataLite";

    public static readonly string Chunk_Master = "_master";

    public static readonly string Chunk_UnknownData = "_rowdata";

    public static readonly string Chunk_Variables = "_vars";

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 5;

    /// <summary>
    /// Wert in Minuten. Die Master-Prüfung wird höchstens in diesem Intervall
    /// durchgeführt, sofern man Master werden kann.
    /// </summary>
    private const int MasterCheckIntervalMinutes = 15;

    /// <summary>
    /// chunkId (lowercase) → SHA256-Hash des kompletten Chunk-Inhalts (Head + Daten + EOF).
    /// Verhindert unnötiges Schreiben bei unverändertem Inhalt (Write-once: keine
    /// identischen Duplikate erzeugen).
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _lastContentHash = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → UTC-Zeitpunkt des letzten Zugriffs (Laden, Speichern, Refresh).
    /// Wird genutzt, um ungenutzte Chunks bei BeSureToBeUpToDate zu überspringen.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → Dateiname der zuletzt geparten oder gespeicherten Chunk-Datei.
    /// Leichtgewichtiges Tracking anstelle von im Speicher gehaltenen Chunk-Objekten:
    /// Chunks werden nach dem Parsen verworfen, nur der Dateiname wird für
    /// Aktualitätsprüfungen (Refresh) vorgehalten.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _processedFile = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → zuletzt beobachteter UTC-LastWriteTime des Chunk-Ordners.
    /// Ist der Wert unverändert, wurde seit dem letzten Load keine Datei hinzugefügt
    /// oder entfernt — da Chunks write-once sind, bedeutet das zwingend, dass kein
    /// neuer Inhalt vorliegen kann. In diesem Fall kann die teure Ordner-Enumeration
    /// in <see cref="LoadChunkWithChunkId"/> und <see cref="RefreshLoadedChunks"/>
    /// übersprungen werden. Auf Windows wird der Zeitstempel nur auf Ebene des
    /// Ordners aktualisiert (nicht rekursiv), was für die flachen Chunk-Ordner passt.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastFolderWriteTimeUtc = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// UTC-Zeitpunkt des letzten Master-Prüfung. Die Prüfung (lädt den Master-Chunk)
    /// wird nur noch alle <see cref="MasterCheckIntervalMinutes"/> Minuten durchgeführt,
    /// da sie auf langsamen Netzwerken teuer ist. Siehe <see cref="BeSureToBeUpToDate"/>.
    /// </summary>
    private DateTime _lastMasterAttemptUtc = DateTime.MinValue;

    private string _systemChunksError = string.Empty;

    /// <summary>
    /// Wird beim Init einmalig verifiziert, ob alle System-Chunks ladbar sind.
    /// <see cref="IsGenericEditable"/> gibt dieses Ergebnis zurück, ohne bei jedem
    /// Aufruf die Chunks neu zu laden — das würde bei jedem SetValue/Checker-Tick
    /// ewig dauern. Siehe <see cref="VerifySystemChunksEditable"/>.
    /// </summary>
    private bool _systemChunksVerified;

    /// <summary>
    /// Reentrancy-Sperre für <see cref="BeSureToBeUpToDate"/>: 0 = frei, 1 = läuft.
    /// Der statische <c>_tableUpdateTimer</c> (alle 5 Min.) sowie UI-Aktionen können
    /// die Methode zeitgleich aufrufen. Ohne Sperre überlappen sich die Aufrufe,
    /// erzeugen doppelte "Lade Chunk..."-Meldungen (Statusleiste friert ein,
    /// "Uhrzeit läuft weiter") und stapeln sich zu einem effektiven Hängende.
    /// </summary>
    private int _isRefreshingChunks;

    #endregion

    #region Constructors

    public TableChunk(string tablename) : base(tablename) { }

    public TableChunk(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    /// <summary>
    /// Letzter UTC-Zeitpunkt der letzten Speicherung der Hauptdatei.
    /// Liest direkt vom Dateisystem (nicht über CachedFileSystem, da .tblh
    /// nicht mehr im CachedFileSystem registriert ist).
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(Filename);
            if (fi is { Exists: true }) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public override bool MultiUserPossible => true;

    /// <summary>
    /// Generiert einen 3-stelligen Hash aus MachineName und Instanz-ID (MyId).
    /// Unterscheidet verschiedene Maschinen/Instanzen bei gleichem Benutzernamen
    /// auf dem Dateisystem.
    /// </summary>
    private static string MachineInstanceHash => $"{Environment.MachineName}|{MyId}".GetMD5Hash()[..3].ToUpperInvariant();

    #endregion

    #region Methods

    /// <summary>
    /// Ermittelt die Chunk-ID LOWERCASE anhand des konfigurierten ChunkValueColumn-Typs (Hash/Name).
    /// Wird von <see cref="GetChunkId"/> genutzt.
    /// </summary>
    public static string ChunkValueToId(ChunkType chunkType, string chunkvalue) {
        if (string.IsNullOrEmpty(chunkvalue)) { chunkvalue = " "; }

        switch (chunkType) {
            case ChunkType.ByHash_1Char:
                return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(1).ToLowerInvariant();

            case ChunkType.ByHash_2Chars:
                return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(2).ToLowerInvariant();

            case ChunkType.ByHash_3Chars:
                return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(3).ToLowerInvariant();

            case ChunkType.ByName:
                var t = FormatHolder_SystemName.MakeValid(chunkvalue).TrimStart('_');
                return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();

            default:
                return Chunk_UnknownData.ToLowerInvariant();
        }
    }

    public static List<byte> GenerateEOF() {
        var result = new List<byte>();

        SaveToByteList(result, TableDataType.EOF, "END");
        return result;
    }

    public static List<byte> GenerateHeadVariableChunks(TableFile tb) {
        var varBytes = new List<byte>();

        var vars = tb.Variables.ToList();
        SaveToByteList(varBytes, TableDataType.TableVariables, vars.SortByKeyName().ToString(true));
        SaveToByteList(varBytes, TableDataType.CheckPoint, $"~^{Chunk_Variables.ToLowerInvariant()}^~");
        return varBytes;
    }

    public static List<byte> GenerateMainChunk(TableFile tb) {
        List<byte> result = new();

        // Metadaten schreiben
        SaveToByteList(result, TableDataType.GlobalShowPass, tb.GlobalShowPass);
        SaveToByteList(result, TableDataType.Creator, tb.Creator);
        SaveToByteList(result, TableDataType.CreateDateUTC, tb.CreateDate);
        SaveToByteList(result, TableDataType.Caption, tb.Caption);

        SaveToByteList(result, TableDataType.Tags, string.Join('\r', tb.Tags));
        SaveToByteList(result, TableDataType.DictionaryWords, string.Join('\r', tb.DictionaryWords.SortedDistinctList()));
        SaveToByteList(result, TableDataType.PermissionGroupsNewRow, string.Join('\r', tb.PermissionGroupsNewRow.SortedDistinctList()));
        SaveToByteList(result, TableDataType.TableAdminGroups, string.Join('\r', tb.TableAdmin.SortedDistinctList()));

        SaveToByteList(result, TableDataType.AssetFolder, tb.AssetFolder);
        SaveToByteList(result, TableDataType.RowQuickInfo, tb.RowQuickInfo);
        SaveToByteList(result, TableDataType.StandardFormulaFile, tb.StandardFormulaFile);

        foreach (var columnitem in tb.Column.OrderBy(t => t.KeyName)) {
            if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                SaveToByteList(result, columnitem);
            }
        }

        SaveToByteList(result, TableDataType.SortDefinition, tb.SortDefinition is null ? string.Empty : tb.SortDefinition.ParseableItems().FinishParseable());
        SaveToByteList(result, TableDataType.UniqueValues, string.Join('\r', tb.UniqueValues.Select(x => x.ParseableItems().FinishParseable()).SortedDistinctList()));
        SaveToByteList(result, TableDataType.ColumnArrangement, tb.ColumnArrangements.ToString(false));
        SaveToByteList(result, TableDataType.EventScript, tb.EventScript.ToString(true));
        SaveToByteList(result, TableDataType.EventScriptVersion, tb.EventScriptVersion.ToString5());

        SaveToByteList(result, TableDataType.CheckPoint, $"~^{Chunk_MainData.ToLowerInvariant()}^~");

        return result;
    }

    /// <summary>
    /// Generiert den Content für die Lite-Hauptdatei (.tblh). Enthält ausschließlich
    /// den CheckPoint, keine Nutzdaten — die eigentlichen Daten liegen in den Row-Chunks.
    /// Head und EOF werden beim Speichern ergänzt.
    /// </summary>
    public static List<byte> GenerateMainLiteChunk() {
        var result = new List<byte>();

        SaveToByteList(result, TableDataType.CheckPoint, $"~^{Chunk_MainDataLite.ToLowerInvariant()}^~");

        return result;
    }

    public static List<byte> GenerateMasterUserChunk(TableFile tb) {
        var masterUserBytes = new List<byte>();

        SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterUser, tb.TemporaryTableMasterUser);
        SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterTimeUTC, tb.TemporaryTableMasterTimeUtc);
        SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterApp, tb.TemporaryTableMasterApp);
        SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterMachine, tb.TemporaryTableMasterMachine);
        SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterId, tb.TemporaryTableMasterId);
        SaveToByteList(masterUserBytes, TableDataType.CheckPoint, $"~^{Chunk_Master.ToLowerInvariant()}^~");

        return masterUserBytes;
    }

    public static List<byte> GenerateRowChunk(TableFile tb, bool allrows, string chunkId) {
        // Initialisierung der Basis-Listen
        var targetList = new List<byte>();

        // Rows verarbeitet — sortiert nach KeyName für deterministische Serialisierung
        IEnumerable<RowItem> rows = allrows
            ? tb.Row
            : RowsOfChunk(tb, chunkId);

        foreach (var thisRow in rows.OrderBy(r => r.KeyName)) {
            if (thisRow is null || thisRow.IsDisposed) { continue; }

            SaveToByteList(targetList, thisRow);
        }

        if (!string.IsNullOrEmpty(chunkId)) {
            SaveToByteList(targetList, TableDataType.CheckPoint, $"~^{chunkId.ToLowerInvariant()}^~");
        }

        return targetList;
    }

    public static List<byte> GenerateUndoChunk(TableFile tb, bool allrows, string chunkId) {
        var result = new List<byte>();
        var important = 0;
        var undoCount = 0;

        chunkId = chunkId.ToLowerInvariant();

        List<UndoItem> undoSnapshot;
        //Develop.Diagnose("UNDO",$"GenerateUndoChunk WAIT: chunkId={chunkId} T{Environment.CurrentManagedThreadId}");
        lock (tb._undoLock) {
            //Develop.Diagnose("UNDO",$"GenerateUndoChunk ENTER: chunkId={chunkId} Undo.Count={tb.Undo.Count} T{Environment.CurrentManagedThreadId}");
            undoSnapshot = [.. tb.Undo];
            //Develop.Diagnose("UNDO",$"GenerateUndoChunk DONE: chunkId={chunkId} snapshot={undoSnapshot.Count} T{Environment.CurrentManagedThreadId}");
        }

        var sortedUndoItems = undoSnapshot
            .Where(item => item?.LogsUndo(tb) == true)
            .OrderByDescending(item => item.DateTimeUtc)
            .ToList();

        foreach (var thisWorkItem in sortedUndoItems) {
            // Harte Begrenzung: maximal 10 EventScript-Items insgesamt (ungeachtet des 1000-Zählers)
            if (thisWorkItem.Command == TableDataType.EventScript && important >= 10) { continue; }

            if (undoCount < 1000 ||
                (thisWorkItem.Command == TableDataType.EventScript && important < 10)) {
                undoCount++;
                if (thisWorkItem.Command == TableDataType.EventScript) { important++; }

                if (!allrows) {
                    var lookupRow = thisWorkItem.RowKey is { Length: > 0 } rk ? tb.Row.GetByKey(rk) : null;
                    var targetChunkId = GetChunkId(tb, thisWorkItem.Command, lookupRow?.ChunkValue ?? string.Empty);

                    if (!string.Equals(targetChunkId, chunkId, StringComparison.OrdinalIgnoreCase)) { continue; }
                }

                SaveToByteList(result, TableDataType.Undo, thisWorkItem.ParseableItems().FinishParseable());
            }
        }

        if (!string.IsNullOrEmpty(chunkId)) {
            SaveToByteList(result, TableDataType.CheckPoint, $"~^{chunkId}^~");
        }

        return result;
    }

    public static List<byte> GenerateUsesChunk(TableFile tb) {
        List<byte> usesBytes = new();

        foreach (var columnitem in tb.Column.OrderBy(t => t.KeyName)) {
            if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                SaveToByteList(usesBytes, TableDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
            }
        }

        SaveToByteList(usesBytes, TableDataType.CheckPoint, $"~^{Chunk_AdditionalUseCases.ToLowerInvariant()}^~");

        return usesBytes;
    }

    /// <summary>
    /// Ermittelt die Chunk-ID (maindata, _master, 1e3, etc.) LOWERCASE für den angegebenen Typ und Wert.
    /// Standard-Implementierung mit System-Chunks. Wird von <see cref="ChunkValueToId"/>
    /// und <see cref="RowsOfChunk(TableFile, string)"/> genutzt.
    /// </summary>
    public static string GetChunkId(Table tb, TableDataType type, string chunkvalue) {
        if (tb.IsDisposed) { return string.Empty; }
        if (type is TableDataType.Command_RemoveColumn
                or TableDataType.Command_AddColumnByName) { return Chunk_MainData.ToLowerInvariant(); }
        if (type == TableDataType.Command_NewStart) { return string.Empty; }
        if (type.IsObsolete()) { return string.Empty; }
        if (type == TableDataType.ColumnSystemInfo) { return Chunk_AdditionalUseCases.ToLowerInvariant(); }
        if (type == TableDataType.TableVariables) { return Chunk_Variables.ToLowerInvariant(); }
        if (type is TableDataType.TemporaryTableMasterUser
                 or TableDataType.TemporaryTableMasterTimeUTC
                 or TableDataType.TemporaryTableMasterApp
                 or TableDataType.TemporaryTableMasterMachine
                 or TableDataType.TemporaryTableMasterId) { return Chunk_Master.ToLowerInvariant(); }
        if (type == TableDataType.CheckPoint) { return string.Empty; }
        if (type.IsCellValue() || type is TableDataType.Undo or TableDataType.Command_AddRow or TableDataType.Command_RemoveRow) {
            var chunktype = tb.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None;

            return ChunkValueToId(chunktype, chunkvalue);
        }
        return Chunk_MainData.ToLowerInvariant();
    }

    public static List<string> RowChunkIdsInMemory(TableFile tbf) {
        var chunktype = tbf.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None;
        return tbf.Row.Select(r => ChunkValueToId(chunktype, r.ChunkValue)).Distinct().ToList();
    }

    /// <summary>
    /// Liefert alle Zeilen der Tabelle <paramref name="tb"/>, deren Chunk-ID
    /// dem <see cref="Chunk.KeyName"/> von <paramref name="chunk"/> entspricht.
    /// Siehe <see cref="RowsOfChunk(TableFile, string)"/> für Details.
    /// </summary>
    public static List<RowItem> RowsOfChunk(TableFile tb, Chunk chunk) {
        if (chunk is null || chunk.IsDisposed) { return []; }
        return RowsOfChunk(tb, chunk.KeyName);
    }

    /// <summary>
    /// Liefert alle Zeilen der Tabelle <paramref name="tb"/>, deren Chunk-ID
    /// (ermittelt über <see cref="GetChunkId"/> für Cell-Daten) <paramref name="chunkId"/>
    /// entspricht. Vergleich ordinal-ignore-case, da beide Seiten laut Konvention
    /// lowercase sind, die Normalisierung aber nicht erzwungen ist.
    /// Für die Lite-Hauptdatei (.tblh) ist das Ergebnis immer leer —
    /// <see cref="GetChunkId"/> liefert niemals <see cref="Chunk_MainDataLite"/>,
    /// und die Lite-Datei enthält per Definition keine Row-Daten.
    /// </summary>
    public static List<RowItem> RowsOfChunk(TableFile tb, string chunkId) {
        if (tb is null || tb.IsDisposed) { return []; }
        if (string.IsNullOrEmpty(chunkId)) { return []; }
        if (tb is not TableChunk) { return []; }

        var chunktype = tb.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None;

        return [.. tb.Row.Where(r => ReferenceEquals(r.Table, tb) && string.Equals(ChunkValueToId(chunktype, r.ChunkValue), chunkId, StringComparison.OrdinalIgnoreCase))];
    }

    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (updateAllowed) {
            OnLoading();
            var result = LoadChunkWithChunkId(Chunk_Master);
            if (result.IsFailed) { return false; }
            if (result.Value is true) { OnLoaded(false, true); }
        }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    public override bool BeSureRowIsLoaded(string chunkValue) {
        if (!base.BeSureRowIsLoaded(chunkValue)) { return false; }

        var chunkValues = chunkValue.SplitAndCutByCr().SortedDistinctList();

        var loaded = false;
        OnLoading();

        foreach (var thisvalue in chunkValues) {
            var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, thisvalue);
            var result = LoadChunkWithChunkId(chunkId);
            if (result.IsFailed) { return false; }
            loaded = loaded || result.Value is true;
        }

        if (loaded) { OnLoaded(false, false); }

        return true;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        // Reentrancy-Sperre: Der statische _tableUpdateTimer (alle 5 Min.) sowie
        // UI-Aktionen (Button-Klicks, Editoren) können BeSureToBeUpToDate zeitgleich
        // aufrufen. Ohne Sperre würden sich die Aufrufe überlappen, doppelte
        // "Lade Chunk..."-Meldungen erzeugen und die Statusleiste mit ständig
        // neuen Zeitstempeln fluten ("Uhrzeit läuft weiter"). Bei langsamen
        // Netzwerken stapeln sich weitere Timer-Ticks und verschärfen den Effekt
        // bis zum scheinbaren Einfrieren.
        if (Interlocked.CompareExchange(ref _isRefreshingChunks, 1, 0) != 0) {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Aktualisierung von '{KeyName}' läuft bereits — übersprungen, um eine Überlagerung zu vermeiden", 0);
            return true;
        }

        try {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

            if (firstTime) {
                if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }
            }

            var loaded = false;
            var ok = true;

            OnLoading();

            if (!firstTime) {
                var result = LoadChunkWithChunkId(Chunk_MainData);
                if (result.IsFailed) {
                    Develop.Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Haupt-Chunk von '{KeyName}' konnte nicht geladen werden: {result.FailedReason}", 0);
                    return false;
                }
                loaded = result.Value is true;
            }

            Column.GetSystems();

            List<string> list = [Chunk_AdditionalUseCases, Chunk_Master, Chunk_Variables, Chunk_UnknownData];

            foreach (var item in list) {
                // System-Chunks werden immer geprüft (kein SkipIfUnusedMinutes-Skip).
                // Im Gegensatz zur regulären Chunk-Verwaltung gibt es hier kein CachedFileSystem, das
                // neu erscheinende Dateien automatisch erkennt. Ohne diese Prüfung
                // würden neu erstellte System-Chunks anderer Benutzer (z.B. _master)
                // nie bemerkt werden, sobald sie einmal als "nicht vorhanden" erkannt wurden.
                // LoadChunkWithChunkId kehrt bei unveränderten Dateien schnell zurück
                // (Already-Current-Check via Dateiname-Vergleich).
                var result = LoadChunkWithChunkId(item);
                loaded = loaded || result.Value is true;
                ok = ok && result.IsSuccessful;
            }

            loaded = loaded || RefreshLoadedChunks(firstTime);

            if (loaded) { OnLoaded(firstTime, true); }

            // Master-Prüfung nur alle MasterCheckIntervalMinutes Minuten durchführen,
            // sofern man Master werden kann. Beim ersten Mal (Init) sofort prüfen.
            // TryToSetMeTemporaryMaster selbst klärt über NewMasterPossible, ob ein
            // Master-Wechsel überhaupt möglich ist.
            if (ok && (firstTime || DateTime.UtcNow.Subtract(_lastMasterAttemptUtc).TotalMinutes >= MasterCheckIntervalMinutes)) {
                _lastMasterAttemptUtc = DateTime.UtcNow;
                TryToSetMeTemporaryMaster();
            }

            return ok;
        } finally {
            Interlocked.Exchange(ref _isRefreshingChunks, 0);
        }
    }

    /// <summary>
    /// Prüft, ob der Row-Chunk für den angegebenen Chunk-Wert aktuell im Speicher
    /// geladen ist (geparst wurde). Wird für verlinkte Zellen benötigt, um
    /// sicherzustellen, dass die Zielzeile verfügbar ist.
    /// </summary>
    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        return _processedFile.ContainsKey(chunkId);
    }

    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        // Chunk-Verifikation nur einmal beim Init durchführen und cachen.
        // Bei jedem Aufruf (z.B. SetValue, Checker-Tick) die Chunks neu zu laden,
        // dauert auf langsamen Netzwerken zu lange.
        if (!_systemChunksVerified) {
            // Flag VOR dem Laden setzen — Reentrancy-Sperre.
            // VerifySystemChunksEditable löst Chunk-Parsen aus, das über
            // VariableRowItem.TryParseValue → BeSureRowIsLoaded wieder hier
            // landet. Ohne Vorab-Setzung entstünde ein Stack Overflow.
            _systemChunksVerified = true;
            _systemChunksError = VerifySystemChunksEditable();
        }

        return _systemChunksError;
    }

    /// <summary>
    /// Überschrieben, da die Basisklasse <see cref="TableFile.IsValueEditable"/>
    /// CachedFileSystem.Get&lt;Chunk&gt;(Filename) verwendet, was für .tblh-Dateien
    /// nicht mehr funktioniert (Suffix wurde vom CachedFileSystem entfernt).
    /// </summary>
    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        if (InitialSavePending) { return string.Empty; }

        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (type == TableDataType.Command_AddRow && string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        var chunkId = GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        return CheckEditLock(chunkId);
    }

    public override bool LoadTableRows(bool oldest, int count) {
        if (!base.LoadTableRows(oldest, count)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return true; }

        var subDirs = IO.GetDirectories(chunkFolder);
        var chunkIds = new List<string>();

        foreach (var subDir in subDirs) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (IsRowChunk(chunkId)) { chunkIds.Add(chunkId); }
        }

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

        foreach (var chunkId in chunkIds) {
            var result = LoadChunkWithChunkId(chunkId);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(false, true); }

        return ok;
    }

    /// <summary>
    /// Generiert den Timestamp-String für Chunk-Dateinamen im Format
    /// yyyy-MM-dd-HH-mm-ss-fff_Username-Hash. Wird von <see cref="Table.RenameChunks"/>
    /// genutzt, um beim Formatwechsel einheitliche Dateinamen zu erzeugen.
    /// </summary>
    internal static string GenerateChunkTimestamp() =>
        $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MachineInstanceHash}";

    protected override bool LoadMainData() {
        EnsureDummyFileExists();

        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        var result = LoadChunkWithChunkId(Chunk_MainData);
        if (result.IsFailed) { return false; }

        // Bei einer geladenen Tabelle muss der Hauptchunk vorhanden sein.
        // Fehlt er (Ordner leer), ist die Tabelle korrupt.
        if (!_processedFile.ContainsKey(Chunk_MainData.ToLowerInvariant())) {
            Freeze("Hauptchunk der Tabelle fehlt");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Speichert alle Chunks direkt auf die Festplatte (ohne CachedFileSystem).
    /// Nach dem Speichern werden alle generierten Daten verworfen.
    /// Jeder Chunk ist ein Einweg — eine einmal gespeicherte Datei wird nie
    /// wieder überschrieben. Die Lite-Hauptdatei (.tblh) wird ebenfalls nur einmal
    /// geschrieben (write-once), da sie keine Nutzdaten enthält.
    /// <para>
    /// Die Editierbarkeit wird PRO CHUNK geprüft: Chunks, die von einem anderen
    /// Benutzer aktiv gesperrt sind (s. <see cref="CheckEditLock"/>), werden
    /// übersprungen und ihre Änderungen verworfen. Alle bearbeitbaren Chunks
    /// werden normal gespeichert.
    /// </para>
    /// <para>
    /// Ausnahme (sehr selten): Taucht zwischen der Per-Chunk-Prüfung und dem
    /// eigentlichen Schreiben eine neue fremde Claim-Datei auf (Re-Check via
    /// <see cref="CheckEditLock"/>), wird die gesamte Tabelle eingefroren —
    /// sonst würde die fremde Änderung überdeckt.
    /// </para>
    /// </summary>
    protected override string SaveInternal() {
        if (!SaveRequired) { return string.Empty; }
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        PauseTimer();

        try {
            // Nur die Basis-Checks (Freeze/Disposed/Version) — NICHT die
            // System-Chunks-Verifikation aus dem Override. Jene würde die
            // gesamte Speicherung blockieren, sobald ein einziger System-Chunk
            // (z.B. Master) gelocked ist. Die Per-Chunk-Prüfung erfolgt in AddChunk.
            if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

            var x = LastChange;

            var timestamp = GenerateChunkTimestamp();
            var head = GenerateHeadBytes();

            // path → vollständiger Inhalt (Head + Daten + EOF), fertig zum Schreiben
            var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);
            // chunkId (lowercase) → Hash des Voll-Inhalts, aktualisiert nach erfolgreichem Speichern
            var newHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // chunkIds mit echten Änderungen (für EditLock Re-Check vor dem Schreiben)
            var changedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddChunk(string chunkId, List<byte> data) {
                var idLower = chunkId.ToLowerInvariant();
                var isMainLite = string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase);

                // Per-Chunk-Editierbarkeit: ist der Chunk durch einen anderen
                // Benutzer gesperrt, wird er übersprungen — die anderen Chunks
                // werden weiter gespeichert. Die Änderungen am gesperrten Chunk
                // gelten damit als verworfen (werden nicht erneut versucht).
                // MainLite ist eine write-once-Datei ohne Chunk-Ordner und wird
                // nicht über CheckEditLock geprüft.
                if (!isMainLite && !string.IsNullOrEmpty(CheckEditLock(chunkId))) {
                    return;
                }

                // Vollständigen Inhalt aufbauen (Head + Daten + EOF) für Hash-Vergleich und Speicherung
                var fullContent = new List<byte>(head.Count + data.Count + 16);
                fullContent.AddRange(head);
                fullContent.AddRange(data);
                SaveToByteList(fullContent, TableDataType.EOF, "END");

                //var fullHash = Constants.Win1252.GetString(fullContent.ToArray()); //Generic.GetSHA256HashString(fullContent.ToArray());

                var fullHash = Generic.GetSHA256HashString(fullContent.ToArray());
                newHashes[idLower] = fullHash;

                // Unchanged-Check: bei identischem Inhalt keine neue Datei schreiben
                if (_lastContentHash.TryGetValue(idLower, out var storedHash) && storedHash == fullHash) {
                    return;
                }

                if (isMainLite) {
                    // Write-once: .tblh nur schreiben, wenn noch kein gültiger Inhalt vorhanden
                    if (IO.FileExists(Filename) && HasValidEofMarker(IO.ReadAllBytes(Filename, 2).Value as byte[] ?? [])) {
                        _processedFile[Chunk_MainDataLite.ToLowerInvariant()] = Filename;
                        _lastContentHash[idLower] = fullHash;
                        return;
                    }
                    chunkData[Filename] = fullContent;
                } else {
                    chunkData[$"{GetChunkFolder(chunkId)}{timestamp}.tblc"] = fullContent;
                    changedChunkIds.Add(idLower);
                    //IO.WriteAllText(@"C:\01_DATA\neu.txt", fullHash, Constants.Win1252, false);
                    //IO.WriteAllText(@"C:\01_DATA\alt.txt", storedHash, Constants.Win1252, false);
                }
            }

            AddChunk(Chunk_MainData, [
                .. GenerateMainChunk(this),
              .. GenerateUndoChunk(this, false, Chunk_MainData)
            ]);

            AddChunk(Chunk_MainDataLite, [
                 .. GenerateMainLiteChunk(),
           ]);

            AddChunk(Chunk_AdditionalUseCases, [
                .. GenerateUsesChunk(this),
              .. GenerateUndoChunk(this, false, Chunk_AdditionalUseCases)
            ]);

            AddChunk(Chunk_Variables, [
                .. GenerateHeadVariableChunks(this),
              .. GenerateUndoChunk(this, false, Chunk_Variables)
            ]);

            AddChunk(Chunk_Master, [
                .. GenerateMasterUserChunk(this),
              .. GenerateUndoChunk(this, false, Chunk_Master)
            ]);

            var rowChunkIds = RowChunkIdsInMemory(this);
            if (!rowChunkIds.Contains(Chunk_UnknownData)) {
                rowChunkIds.Add(Chunk_UnknownData);
            }

            foreach (var thisChunkId in rowChunkIds) {
                AddChunk(thisChunkId, [
                    .. GenerateRowChunk(this, false, thisChunkId),
                  .. GenerateUndoChunk(this, false, thisChunkId)
                ]);
            }

            if (x != LastChange) { return "Tabelle wurde während der Chunk-Generierung geändert"; }

            // Re-Check: Es wird vorab kein Claim geschrieben, daher kann ein anderer
            // Benutzer zwischenzeitlich einen der zu schreibenden Chunks gesperrt haben.
            // In diesem Fall Speichern abbrechen und einfrieren, statt fremde Änderungen zu
            // überschreiben. changedChunkIds enthält nur Chunks mit echten inhaltlichen Änderungen.
            foreach (var chunkId in changedChunkIds) {
                var lockCheck = CheckEditLock(chunkId);
                if (!string.IsNullOrEmpty(lockCheck)) {
                    // Spezialfall: Bestimmte System-Chunks führen NICHT zum Freeze.
                    // Hier werden die Änderungen einfach verworfen (Chunk wird nicht geschrieben).
                    if (string.Equals(chunkId, Chunk_AdditionalUseCases, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_UnknownData, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(chunkId, Chunk_Master, StringComparison.OrdinalIgnoreCase)) {
                        // Entferne den Pfad aus chunkData, damit er nicht geschrieben wird
                        var folderPrefix = GetChunkFolder(chunkId);
                        var keysToRemove = chunkData.Keys.Where(k => k.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (var key in keysToRemove) { chunkData.Remove(key); }
                        continue;
                    }

                    Freeze(lockCheck);
                    return lockCheck;
                }
            }

            // Direkt auf Festplatte schreiben (ohne CachedFileSystem).
            // Chunks sind write-once — jede Datei ist neu und wird nie überschrieben.
            Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks der Tabelle '{Caption}'", 2);

            foreach (var kvp in chunkData) {
                var path = kvp.Key;

                if (IO.CreateDirectory(path.FilePath()).IsFailed) {
                    Freeze($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                    return $"Verzeichnis für '{path}' konnte nicht erstellt werden";
                }

                // Chunk-Dateien werden gezippt gespeichert (wie bei CachedFile/Chunk mit MustZipped=true)
                var zipped = kvp.Value.ToArray().ZipIt();
                if (zipped is null || zipped.Length == 0) {
                    Freeze($"Komprimierung fehlgeschlagen für '{path}'");
                    return $"Komprimierung fehlgeschlagen für '{path}'";
                }

                var writeResult = IO.WriteAllBytes(path, zipped);
                if (writeResult.IsFailed) {
                    Freeze(writeResult.FailedReason);
                    return writeResult.FailedReason;
                }

                // Nach dem Speichern: nur den Dateinamen tracken, Daten verwerfen
                if (string.Equals(path, Filename, StringComparison.OrdinalIgnoreCase)) {
                    _processedFile[Chunk_MainDataLite.ToLowerInvariant()] = path;
                } else {
                    var folder = path.FilePath();
                    var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
                    _processedFile[chunkId] = path;
                    _lastUsed[chunkId] = DateTime.UtcNow;
                    CleanupOldFilesInFolder(GetChunkFilesOrderedByTime(folder).ToArray());
                }
            }

            // Hashes nach erfolgreichem Speichern aktualisieren
            foreach (var kvp in newHashes) {
                _lastContentHash[kvp.Key] = kvp.Value;
            }

            SaveRequired = false;
            InitialSavePending = false;

            return string.Empty;
        } finally {
            OnInvalidateView();
            ResumeTimer();
        }
    }

    /// <summary>
    /// Löscht alle .tblc-Dateien in der übergebenen Liste, die älter als
    /// DeleteOldFilesAfterHours Stunden sind, sofern mindestens FileCount neuere Dateien
    /// vorhanden sind. Dateien mit ungültigem Datumsformat werden ebenfalls gelöscht.
    /// Die Liste muss nach Datum absteigend (neueste zuerst) sortiert sein.
    /// </summary>
    private static void CleanupOldFilesInFolder(string[] files) {
        if (files.Length <= FileCount) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        for (var i = files.Length - 1; i >= FileCount; i--) {
            var file = files[i];
            var fileDate = ExtractDateFromFileName(file);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Chunk-Dateinamen (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc).
    /// Millisekunden sind verpflichtend. Gibt null zurück, wenn das Datum nicht geparst werden kann.
    /// </summary>
    private static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 23) { return null; }
        var datePart = fileName[..23];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Extrahiert den 3-stelligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc). Gibt string.Empty zurück,
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
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.tblc). Der angehängte
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
    /// Generiert die Head-Bytes (Version + Werbung), die jeder Chunk-Datei
    /// vorangestellt werden. Entspricht Chunk.GetHeadBytes(), aber ohne
    /// Instanz-Abhängigkeit — wird direkt ohne CachedFileSystem genutzt.
    /// </summary>
    private static List<byte> GenerateHeadBytes() {
        var headBytes = new List<byte>();

        SaveToByteList(headBytes, TableDataType.Version, TableVersion);
        SaveToByteList(headBytes, TableDataType.Werbung, "                                                                    BlueTable - (c) by Christian Peter                                                                                        ");

        return headBytes;
    }

    private static DateTime GetChunkDirLastWriteUtc(string dir) {
        try {
            return IO.DirectoryExists(dir) ? new DirectoryInfo(dir).LastWriteTimeUtc : DateTime.MaxValue;
        } catch {
            return DateTime.MaxValue;
        }
    }

    /// <summary>
    /// Listet alle .tblc-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
    /// Liest direkt vom Dateisystem, damit neu hinzugekommene Dateien anderer
    /// Benutzer sofort erkannt werden.
    /// </summary>
    private static IEnumerable<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return IO.GetFiles(folder)
                .Where(f => string.Equals(f.FileSuffix(), "tblc", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => ExtractDateFromFileName(f) ?? DateTime.MinValue);
        } catch {
            return [];
        }
    }

    /// <summary>
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer UND der aktuellen
    /// Maschine/Instanz erstellt wurde. Stimmt nur der Benutzername, aber nicht der
    /// Machine/Instance-Hash überein, gilt die Datei als fremd (anderer Rechner/Session).
    /// </summary>
    private static bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        if (!string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase)) { return false; }

        var creatorHash = ExtractMachineInstanceHashFromFileName(filePath);
        return string.Equals(creatorHash, MachineInstanceHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// C:\xxx\[Tablename]\
    /// </summary>
    /// <returns></returns>
    private string BaseChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Prüft die Edit-Sperre für einen Chunk: Wenn die neueste Datei im Ordner
    /// jünger als EditLockMinutes ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    private string CheckEditLock(string chunkId) {
        var folder = GetChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return string.Empty; }

        var fileDate = ExtractDateFromFileName(newestFile);
        if (!fileDate.HasValue) { return "Datums-Fehler in Dateien"; }

        if (DateTime.UtcNow.Subtract(fileDate.Value).TotalMinutes >= EditLockMinutes) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        var creatorHash = ExtractMachineInstanceHashFromFileName(newestFile);
        var creatorDisplay = string.IsNullOrEmpty(creatorHash) ? creator : $"{creator} - {creatorHash}";
        return $"Chunk '{chunkId}' wird seit {DateTime.UtcNow.Subtract(fileDate.Value).TotalMinutes:0} Minuten von '{creatorDisplay}' bearbeitet";
    }

    /// <summary>
    /// Stellt sicher, dass die Dummy .tblh-Datei existiert (0 Bytes).
    /// </summary>
    private void EnsureDummyFileExists() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        if (!IO.FileExists(Filename)) {
            IO.WriteAllBytes(Filename, []);
        }
    }

    /// <summary>
    /// C:\xxx\[Tablename]\[Chunk]\
    /// </summary>
    private string GetChunkFolder(string chunkId) => $"{BaseChunkFolder()}{chunkId.ToLowerInvariant()}\\";

    /// <summary>
    /// Lädt einen Chunk direkt von der Festplatte (ohne CachedFileSystem).
    /// Vor dem Parsen wird <see cref="TableFile.HasValidEofMarker"/> geprüft,
    /// damit nur komplett gespeicherte Chunks eingelesen werden. Nach dem
    /// Parsen wird der Chunk-Inhalt verworfen — nur der Dateiname wird
    /// für Aktualitätsprüfungen gemerkt.
    /// </summary>
    /// <param name="chunkId">Chunk-ID (wird auf Lowercase normalisiert).</param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordnung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    private OperationResult LoadChunkWithChunkId(string chunkId) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        var folder = GetChunkFolder(chunkId);

        // Schnellpfad: Ist der Ordner-Zeitstempel unverändert zur letzten Verarbeitung,
        // wurde keine Datei hinzugefügt oder entfernt. Da Chunks write-once sind,
        // bedeutet das zwingend, dass kein neuer Inhalt vorliegen kann — die teure
        // Ordner-Enumeration kann dann komplett übersprungen werden.
        // (Auf manchen Netzwerk-Shares wird LastWriteTime u.U. verzögert propagiert;
        // in diesem Fall fällt der Code durch und macht die reguläre Prüfung.)
        if (_processedFile.ContainsKey(chunkId)
            && IO.GetFolderWriteTimeUtc(folder) is DateTime folderWriteTime
            && _lastFolderWriteTimeUtc.TryGetValue(chunkId, out var lastFolderWriteTime)
            && lastFolderWriteTime == folderWriteTime) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer konnte zwischenzeitlich eine neuere .tblc-Datei ablegen.
        var chunkFiles = IO.DirectoryExists(folder) ? GetChunkFilesOrderedByTime(folder).ToArray() : [];
        string? newestFile = chunkFiles.Length > 0 ? chunkFiles[0] : null;

        // Bereits aktuell? (gleiche Datei wie zuletzt verarbeitet)
        if (newestFile is not null &&
            _processedFile.TryGetValue(chunkId, out var lastFile) &&
            string.Equals(lastFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            // Ordner-Zeitstempel cachen, damit der nächste Aufruf den Schnellpfad trifft.
            if (IO.GetFolderWriteTimeUtc(folder) is DateTime wt) { _lastFolderWriteTimeUtc[chunkId] = wt; }
            return OperationResult.SuccessFalse;
        }

        if (!IO.DirectoryExists(folder)) {
            if (IO.CreateDirectory(folder).IsFailed) {
                return OperationResult.Failed("Ordner konnte nicht erstellt werden");
            }
            // _lastUsed aktualisieren, damit nicht-existierende Chunks in BeSureToBeUpToDate
            // nicht permanent übersprungen werden, sondern nach SkipIfUnusedMinutes neu geprüft werden.
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
            // Alle Dateien des Chunks wurden gelöscht — Tracking-Eintrag entfernen,
            // damit veraltete Daten nicht als "aktuell" gelten.
            if (_processedFile.TryRemove(chunkId, out var prevFile)) {
                //Develop.Diagnose("CF",$"Chunk '{chunkId}': Alle Dateien gelöscht, bisher verarbeitet: {prevFile}");
            }
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        // Raw bytes direkt von der Festplatte lesen (ohne CachedFileSystem)
        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Lade Chunk '{chunkId}' von '{KeyName}'", 1);

        if (IO.ReadAllBytes(newestFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht gelesen werden");
        }

        // VOR dem Parsen: EOF-Marker prüfen — nur komplett gespeicherte Chunks laden.
        // Fehlt der Marker, ist die Datei möglicherweise gerade beim Schreiben
        // (anderer Benutzer) oder korrupt. In beiden Fall nicht laden.
        if (!HasValidEofMarker(rawBytes)) {
            //Develop.Diagnose("CF",$"Chunk '{chunkId}' unvollständig (kein EOF-Marker): {newestFile.FileNameWithoutSuffix()}");
            return OperationResult.Failed($"Row-Chunk '{chunkId}' ist unvollständig (kein EOF-Marker)");
        }

        // Für Parsing entpacken (Chunk-Dateien werden gezippt gespeichert)
        byte[] chunkContent;
        if (rawBytes.IsZipped()) {
            chunkContent = rawBytes.UnzipIt() ?? [];
            if (chunkContent.Length == 0) {
                return OperationResult.Failed($"Row-Chunk '{chunkId}' konnte nicht entpackt werden");
            }
        } else {
            chunkContent = rawBytes;
        }

        //// CheckPoint prüfen (System-Chunks suchen nach ~^{KeyName}^~, Row-Chunks immer ok)
        //if (!HasCheckPoint(chunkContent, chunkId)) {
        //    return OperationResult.Failed($"Row-Chunk '{chunkId}' enthält keinen gültigen CheckPoint");
        //}

        var isMain = string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase);

        if (!ParseChunk(chunkContent, chunkId, isMain)) {
            return OperationResult.Failed($"Row-Chunk '{chunkId}' Parsen fehlgeschlagen");
        }

        // Chunk nach dem Parsen verwerfen — nur Dateiname und Hash tracken
        _processedFile[chunkId] = newestFile;
        _lastUsed[chunkId] = DateTime.UtcNow;
        //_lastContentHash[chunkId] = Constants.Win1252.GetString(chunkContent);
        _lastContentHash[chunkId] = Generic.GetSHA256HashString(chunkContent);

        CleanupOldFilesInFolder(chunkFiles);

        // Ordner-Zeitstempel NACH Cleanup cachen (Cleanup kann alte Dateien löschen
        // und damit den Zeitstempel verändern). So ist der nächste Schnellpfad korrekt.
        if (IO.GetFolderWriteTimeUtc(folder) is DateTime wtAfterLoad) { _lastFolderWriteTimeUtc[chunkId] = wtAfterLoad; }

        return OperationResult.SuccessValue(true);
    }

    /// <summary>
    /// Parst den Chunk-Inhalt in die Tabellen-Daten. Nach dem Parsen kann der
    /// Inhalt verworfen werden — die Daten sind in den Rows/Columns eingebucht.
    /// </summary>
    private bool ParseChunk(byte[] chunkContent, string chunkId, bool isMain) {
        if (chunkContent.Length == 0) { return true; }

        //Develop.Diagnose("UNDO",$"ParseChunk RemoveAll WAIT: chunk={chunkId} T{Environment.CurrentManagedThreadId}");
        lock (_undoLock) {
            //Develop.Diagnose("UNDO",$"ParseChunk RemoveAll ENTER: chunk={chunkId} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
            Undo.RemoveAll(item => item is not null
                && string.Equals(GetChunkId(this, item.Command, item.RowKey is { Length: > 0 } rk ? Row.GetByKey(rk)?.ChunkValue ?? string.Empty : string.Empty), chunkId, StringComparison.OrdinalIgnoreCase));
            //Develop.Diagnose("UNDO",$"ParseChunk RemoveAll DONE: chunk={chunkId} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
        }

        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseSuccessful = Parse(chunkContent, isMain, parsedRowKeys);

        if (!parseSuccessful) {
            Freeze($"Chunk {chunkId} Parsen fehlgeschlagen");
            return false;
        }

        Row.RemoveObsoleteRows(RowsOfChunk(this, chunkId), parsedRowKeys);

        return true;
    }

    /// <summary>
    /// Aktualisiert alle geladenen Row-Chunks: Prüft für jeden, ob eine neuere
    /// Datei im Ordner existiert und lädt diese ggf. neu.
    /// Chunks, die länger als <see cref="Chunk.SkipIfUnusedMinutes"/> nicht verwendet wurden,
    /// werden übersprungen, sofern <paramref name="firstTime"/> false ist.
    /// Da Chunks write-once sind, reicht der Dateiname-Vergleich (kein IsStale nötig).
    /// </summary>
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }
        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return false; }

        var loaded = false;

        //// Neue Row-Chunk-Ordner entdecken (z.B. neue Zeilen anderer Benutzer).
        //// Ohne CachedFileSystem werden neue Ordner nicht automatisch erkannt.
        //foreach (var subDir in IO.GetDirectories(chunkFolder)) {
        //    var newChunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
        //    if (IsRowChunk(newChunkId) && !_processedFile.ContainsKey(newChunkId)) {
        //        var result = LoadChunkWithChunkId(newChunkId);
        //        loaded = loaded || result.Value is true;
        //    }
        //}

        foreach (var chunkId in _processedFile.Keys.ToList()) {
            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            if (!firstTime && _lastUsed.TryGetValue(chunkId, out var lastUsed) && DateTime.UtcNow.Subtract(lastUsed).TotalMinutes >= SkipIfUnusedMinutes) { continue; }

            // Schnellpfad: Unveränderter Ordner-Zeitstempel → keine neue Datei seit
            // dem letzten Load. Da Chunks write-once sind, ist der verarbeitete
            // Stand noch aktuell, eine Enumeration erübrigt sich.
            if (IO.GetFolderWriteTimeUtc(folder) is DateTime currentWt
                && _lastFolderWriteTimeUtc.TryGetValue(chunkId, out var lastWt)
                && lastWt == currentWt) { continue; }

            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var processedFile = _processedFile[chunkId];

            // Reload wenn eine andere Datei die neueste ist.
            // Da Chunks write-once sind, bedeutet ein anderer Dateiname zwingend
            // anderen Inhalt — keine zusätzlichen Stale-Checks nötig.
            if (!string.Equals(processedFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
                var result = LoadChunkWithChunkId(chunkId);
                if (result.IsFailed) {
                    Develop.DebugPrint(ErrorType.Warning, $"Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
                }
                loaded = loaded || result.Value is true;
            }
        }

        return loaded;
    }

    /// <summary>
    /// Lädt die System-Chunks einmalig und prüft auf Fehler.
    /// Das Ergebnis wird in <see cref="_systemChunksError"/>/<see cref="_systemChunksVerified"/>
    /// gecacht, damit <see cref="IsGenericEditable"/> bei jedem Aufruf ohne
    /// Festplattenzugriff reagieren kann.
    /// </summary>
    private string VerifySystemChunksEditable() {
        string[] checkIds = [Chunk_MainData, Chunk_Master, Chunk_Variables, Chunk_AdditionalUseCases];

        foreach (var id in checkIds) {
            var loadResult = LoadChunkWithChunkId(id);
            if (loadResult.IsFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}' ({loadResult.FailedReason})"; }
        }

        return string.Empty;
    }

    #endregion
}