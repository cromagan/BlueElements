// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Threading;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;
using static BlueBasics.ClassesStatic.Develop;

namespace BlueTable.Classes;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunk : TableFile {

    #region Fields

    public static readonly string Chunk_AdditionalUseCases = "_uses";

    public static readonly string Chunk_Master = "_master";
    public static readonly string Chunk_UnknownData = "_rowdata";
    public static readonly string Chunk_Variables = "_vars";

    #endregion

    #region Constructors

    public TableChunk(string tablename) : base(tablename) { }

    public TableChunk(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    public override bool MultiUserPossible => true;

    #endregion

    #region Methods

    /// <summary>
    /// Ermittelt die Chunk-ID LOWERCASE anhand des konfigurierten ChunkValueColumn-Typs (Hash/Name).
    /// Wird von <see cref="TableChunk.GetChunkId"/> und <see cref="TableChunkFragments"/> gemeinsam genutzt.
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

        SaveToByteList(varBytes, TableDataType.TableVariables, tb.Variables.ToList().ToString(true));
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
        SaveToByteList(result, TableDataType.DictionaryWords, string.Join('\r', tb.DictionaryWords));
        SaveToByteList(result, TableDataType.PermissionGroupsNewRow, string.Join('\r', tb.PermissionGroupsNewRow));
        SaveToByteList(result, TableDataType.TableAdminGroups, string.Join('\r', tb.TableAdmin));

        SaveToByteList(result, TableDataType.AssetFolder, tb.AssetFolder);
        SaveToByteList(result, TableDataType.RowQuickInfo, tb.RowQuickInfo);
        SaveToByteList(result, TableDataType.StandardFormulaFile, tb.StandardFormulaFile);

        foreach (var columnitem in tb.Column) {
            if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                SaveToByteList(result, columnitem);
            }
        }

        SaveToByteList(result, TableDataType.SortDefinition, tb.SortDefinition is null ? string.Empty : tb.SortDefinition.ParseableItems().FinishParseable());
        SaveToByteList(result, TableDataType.UniqueValues, string.Join('\r', tb.UniqueValues.Select(x => x.ParseableItems().FinishParseable())));
        SaveToByteList(result, TableDataType.ColumnArrangement, tb.ColumnArrangements.ToString(false));
        SaveToByteList(result, TableDataType.EventScript, tb.EventScript.ToString(true));
        SaveToByteList(result, TableDataType.EventScriptVersion, tb.EventScriptVersion.ToString5());

        SaveToByteList(result, TableDataType.CheckPoint, $"~^{Chunk_MainData.ToLowerInvariant()}^~");

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
        Develop.Diagnose("UNDO", $"GenerateUndoChunk WAIT: chunkId={chunkId} T{Environment.CurrentManagedThreadId}");
        lock (tb._undoLock) {
            Develop.Diagnose("UNDO", $"GenerateUndoChunk ENTER: chunkId={chunkId} Undo.Count={tb.Undo.Count} T{Environment.CurrentManagedThreadId}");
            undoSnapshot = [.. tb.Undo];
            Develop.Diagnose("UNDO", $"GenerateUndoChunk DONE: chunkId={chunkId} snapshot={undoSnapshot.Count} T{Environment.CurrentManagedThreadId}");
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

        foreach (var columnitem in tb.Column) {
            if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                SaveToByteList(usesBytes, TableDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
            }
        }

        SaveToByteList(usesBytes, TableDataType.CheckPoint, $"~^{Chunk_AdditionalUseCases.ToLowerInvariant()}^~");

        return usesBytes;
    }

    /// <summary>
    /// Ermittelt die Chunk-ID (maindata, _master, 1e3, etc.) LOWERCASE für den angegebenen Typ und Wert.
    /// Standard-Implementierung mit System-Chunks. Wird von <see cref="TableChunkFragments"/>
    /// und <see cref="TableChunk"/> gemeinsam genutzt.
    /// </summary>
    public static string GetChunkId(Table tb, TableDataType type, string chunkvalue) {
        if (tb.IsDisposed) { return string.Empty; }
        if (tb is not TableChunk and not TableChunkFragments) { return string.Empty; }
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
    /// Für die Lite-Hauptdatei (.tblh) von <see cref="TableChunkFragments"/>
    /// ist das Ergebnis immer leer — <see cref="GetChunkId"/> liefert niemals
    /// <see cref="TableChunkFragments.Chunk_MainDataLite"/>, und die Lite-Datei
    /// enthält per Definition keine Row-Daten.
    /// </summary>
    public static List<RowItem> RowsOfChunk(TableFile tb, string chunkId) {
        if (tb is null || tb.IsDisposed) { return []; }
        if (string.IsNullOrEmpty(chunkId)) { return []; }
        if (tb is not TableChunkFragments and not TableChunk) { return []; }

        var chunktype = tb.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None;

        return [.. tb.Row.Where(r => ReferenceEquals(r.Table, tb) && string.Equals(ChunkValueToId(chunktype, r.ChunkValue), chunkId, StringComparison.OrdinalIgnoreCase))];
    }

    /// <summary>
    /// Schreibt die übergebenen Chunk-Daten in die zugehörigen Dateien.
    /// Key ist der vollständige Dateipfad, Value die Daten-Bytes (ohne Head, ohne EOF).
    /// Fügt Head-Bytes und EOF hinzu, setzt den Content und speichert über CachedFileSystem.
    /// </summary>
    public static string SaveChunkFiles(Dictionary<string, List<byte>> chunkData, int minLen, string caption) {
        long totalLength = 0;

        foreach (var kvp in chunkData) {
            if (CachedFileSystem.Get<Chunk>(kvp.Key) is not { } chunk) {
                if (IO.CreateDirectory(kvp.Key.FilePath()).IsFailed) {
                    return $"Verzeichnis für Chunk '{kvp.Key}' konnte nicht erstellt werden";
                }
                chunk = new Chunk(kvp.Key);
            }

            var head = chunk.GetHeadBytes();
            if (head.Count < 100) { return $"Head-Bytes für Chunk '{kvp.Key}' ungültig"; }

            var bytes = new List<byte>(head.Count + kvp.Value.Count + 16);
            bytes.AddRange(head);
            bytes.AddRange(kvp.Value);
            SaveToByteList(bytes, TableDataType.EOF, "END");

            chunk.EnsureContentLoaded();
            chunk.Content = bytes.ToArray();
            totalLength += chunk.Content.Length;
        }

        if (totalLength < minLen) {
            return "Chunk-Daten zu klein für Speicherung";
        }

        Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks der Tabelle '{caption}'", 2);

        CachedFileSystem.SaveAll(true);

        // Verifizieren, dass alle Chunks erfolgreich gespeichert wurden.
        // SaveAll ist void und fängt Fehler nur intern ab — IsSaved zeigt, ob der
        // Inhalt wirklich auf der Platte steht.
        foreach (var kvp in chunkData) {
            if (CachedFileSystem.Get<Chunk>(kvp.Key) is { } chunk) {
                chunk.WaitDiskOperationFinished();
                if (!chunk.IsSaved) {
                    return $"Chunk '{kvp.Key}' konnte nicht gespeichert werden";
                }
            }
        }

        return string.Empty;
    }

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }
        if (InitialSavePending) { return string.Empty; }

        var chunkId = GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        OnLoading();
        var result = LoadChunkWithChunkId(chunkId);
        if (result.IsFailed) { return result.FailedReason; }
        if (result.Value is true) { OnLoaded(false, true); }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) { return $"Interner Chunk-Fehler beim Schreibrecht anfordern {chunkId}"; }
        f = chunk.AcquireWriteAccess();
        if (string.IsNullOrEmpty(f)) {
            SaveRequired = true;
        }
        return f;
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkValue">kann auch mehrere Chunkvalues mit \r\n getrennt enthalten.</param>
    /// <returns>Ob ein Load aller Chunks stattgefunden hat</returns>
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
        Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

        var loaded = false;
        var ok = true;

        OnLoading();

        if (!firstTime) {
            var result = LoadChunkWithChunkId(Chunk_MainData);
            if (result.IsFailed) { return false; }
            loaded = result.Value is true;
        }

        Column.GetSystems();

        if (firstTime) {
            if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }
        }

        List<string> list = [Chunk_AdditionalUseCases, Chunk_Master, Chunk_Variables, Chunk_UnknownData];

        foreach (var item in list) {
            if (!firstTime && !IsChunkRecentlyUsed(ComputeChunkPath(Filename, item))) { continue; }
            var result = LoadChunkWithChunkId(item);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(firstTime, true); }

        if (ok) { TryToSetMeTemporaryMaster(); }

        return ok;
    }

    public string ChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        return chunk is not null && !chunk.LoadFailed;
    }

    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        // Bei einer frisch erzeugten Tabelle (z.B. "Speichern unter" von .bdb auf .cbdb)
        // existieren die Chunks noch nicht auf der Festplatte. Ohne diese Prüfung würde
        // LoadChunkWithChunkId fälschlicherweise "Hauptchunk fehlt" melden und die neue
        // Tabelle unwiderruflich einfrieren, bevor SaveInternal überhaupt Chunks erzeugen kann.
        if (InitialSavePending) { return string.Empty; }

        string[] checkIds = [Chunk_MainData,
            Chunk_Master,
            Chunk_Variables,
            Chunk_AdditionalUseCases];

        foreach (var id in checkIds) {
            var loadResult = LoadChunkWithChunkId(id);
            if (loadResult.IsFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}' ({loadResult.FailedReason})"; }
        }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        if (InitialSavePending) { return string.Empty; }

        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var chunkId = GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        OnLoading();
        var result = LoadChunkWithChunkId(chunkId);

        if (result.IsFailed) { return result.FailedReason; }

        if (result.Value is true) { OnLoaded(false, true); }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) { return $"Interner Chunk-Fehler bei Editier-Prüfung {chunkId}"; }
        return chunk.IsNowEditable();
    }

    public override bool LoadTableRows(bool oldest, int count) {
        if (!base.LoadTableRows(oldest, count)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (!IO.DirectoryExists(chunkPath)) { return true; }

        var files = CachedFileSystem.GetFiles(chunkPath, ["*.bdbc"]);

        CachedFile[] fileQuery;

        if (count < 0) {
            fileQuery = files;
        } else {
            if (oldest) {
                fileQuery = [.. files.OrderBy(f => f?.FileInfo?.LastWriteTime ?? DateTime.Now).Take(count)];
            } else {
                fileQuery = [.. files.OrderBy(f => Constants.GlobalRnd.Next()).Take(count)];
            }
        }

        var loaded = false;
        var ok = true;
        OnLoading();

        foreach (var file in fileQuery) {
            var chunkId = file.Filename.FileNameWithoutSuffix();
            var result = LoadChunkWithChunkId(chunkId);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(false, true); }

        return ok;
    }

    public override void MasterMe() {
        if (!string.IsNullOrEmpty(AcquireWriteAccess(TableDataType.TemporaryTableMasterUser))) { return; }

        base.MasterMe();
    }

    public override void ReorganizeChunks() {
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }

        base.ReorganizeChunks();

        #region Erst alle Chunks laden

        if (!LoadTableRows(false, -1)) {
            DebugError("Fehler beim Chunk laden!");
            return;
        }

        #endregion

        #region alte Chunk-Dateien löschen

        var chunkPath = ChunkFolder();

        if (IO.DirectoryExists(chunkPath)) {
            var chunkFiles = IO.GetFiles(chunkPath)
                .Where(f => {
                    var ext = f.FileSuffix().ToLowerInvariant();
                    return ext == "bdbc" || ext == "bak" || ext == "blk";
                }).ToList();

            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_AdditionalUseCases}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Master}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Variables}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_MainData}.bdbc", false);
            IO.DeleteFile(chunkFiles);
        }

        #endregion

        SaveRequired = true;

        _ = SaveInternal();
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Speicherung ausgeführt wurde.
    /// Nutzt das Saved-Event statt Polling.
    /// </summary>
    /// <returns>True, wenn die Speicherung erfolgreich abgeschlossen wurde, sonst False</returns>
    public bool WaitChunkIsSaved(string chunkid) {
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkid));
        if (chunk is null) { return true; }

        for (var i = 0; i < 1200; i++) {
            if (!chunk.IsSaving) { return true; }
            Thread.Sleep(100);
        }

        Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {chunkid} wurde nicht richtig gespeichert", 0);
        return false;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkId"></param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    protected virtual OperationResult LoadChunkWithChunkId(string chunkId) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        // Prüfung auf laufende Speicherungen
        // Wir warten nur, wenn der Chunk wirklich gerade aktiv gespeichert wird (IsSaving).
        // Ein leerer Chunk (IsSaved = true bei Content = null) darf uns nicht blockieren.
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));

        chunk?.LastUsed = DateTime.UtcNow;

        if (chunk is not null && chunk.IsSaving) {
            if (!WaitChunkIsSaved(chunkId)) {
                return OperationResult.Failed($"Timeout beim Warten auf Speicherung von {chunkId}");
            }
        }

        if (chunk is null) {
            Diagnose("CF", $"Get<Chunk> war null für {chunkId} bei {Filename.FileNameWithSuffix()}");

            chunk = new Chunk(ComputeChunkPath(Filename, chunkId));
            chunk.LastUsed = DateTime.UtcNow;

            // Zuerst Cache prüfen (Datei kann als anderer Typ gecacht sein, z.B. TableChunk).
            // Nur wenn nicht im Cache UND nicht auf Platte -> Recovery.
            var inCache = IO.FileExists(chunk.Filename);
            var onDisk = !inCache && IO.FileExists(chunk.Filename);
            if (!inCache && !onDisk) {
                // Chunk fehlt auf der Festplatte — versuchen, aus Backup (.bak) wiederherzustellen
                var recovered = TryRecoverFromBackup(chunk.Filename, chunkId, 10000);

                if (!recovered) {
                    // Für den Hauptchunk: nicht leer erstellen — Datenverlust vermeiden
                    if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
                        Diagnose("CF", $"Hauptchunk fehlt: {Filename.FileNameWithSuffix()} inCache={inCache} onDisk={onDisk}");
                        Freeze($"Hauptchunk fehlt auf der Festplatte und kein gültiges Backup vorhanden");
                        return OperationResult.Failed("Hauptchunk fehlt, keine Wiederherstellung möglich");
                    }

                    // Für Nebenchunks: leeren Chunk erstellen (normaler Fall, z.B. neuer Hash-Chunk)
                    Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Erstelle neuen Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'", 0);
                    chunk.AcquireWriteAccess();
                    chunk.EnsureContentLoaded();
                    var head = chunk.GetHeadBytes();
                    SaveToByteList(head, TableDataType.EOF, "END");
                    chunk.Content = head.ToArray();
                    _ = chunk.Save().GetAwaiter().GetResult();
                    return OperationResult.SuccessValue(false);
                }
            }
        }

        // IsStale() prüft das Dateisystem auf externe Änderungen (im Gegensatz zu
        // NeedsLoading(), das nur den In-Memory-Zustand prüft).
        // Dadurch werden Änderungen an der Datei auf der Festplatte erkannt, die
        // NeedsLoading() verfehlen würde (z.B. wenn der FileSystemWatcher das Event verpasst).
        var needLoading = chunk.LoadFailed || chunk.IsStale();

        var loaded = false;

        if (needLoading) {
            //Develop.AbortAppIfStackOverflow();
            chunk.WaitDiskOperationFinished();   // Sicherstellen, dass kein I/O mehr läuft

            // Chunk könnte während des Wartens disposed worden sein
            if (chunk.IsDisposed) {
                return OperationResult.Failed($"Chunk {chunkId} wurde während des Wartens verworfen");
            }

            // Invalidate nur aufrufen, wenn wir wirklich sicher sind, dass wir neu laden wollen,
            // nicht bereits ein Ladevorgang läuft und keine ungespeicherten Änderungen existieren.
            // Andernfalls würden lokale Datenänderungen unwiederbringlich verworfen.
            if (!chunk.IsLoading) {
                if (!chunk.IsSaved) {
                    // Chunk hat ungespeicherte Änderungen — Reload überspringen,
                    // um Datenverlust zu vermeiden. Die aktuellen Daten bleiben erhalten.
                    return OperationResult.SuccessValue(false);
                }
                chunk.Invalidate();
            }

            if (!chunk.EnsureContentLoaded()) {
                return OperationResult.Failed("Chunk Laden fehlgeschlagen");
            }

            if (!Parse(chunk)) {
                return OperationResult.Failed("Parsen fehlgeschlagen");
            }

            loaded = true;
        }

        return OperationResult.SuccessValue(loaded);
    }

    protected override bool LoadMainData() => LoadChunkWithChunkId(Chunk_MainData).IsSuccessful;

    protected bool Parse(Chunk chunk) {
        if (chunk.LoadFailed) { return false; }

        var chunkContent = chunk.Content;
        if (chunkContent.Length == 0) { return true; }

        Develop.Diagnose("UNDO", $"Parse RemoveAll WAIT: chunk={chunk.KeyName} T{Environment.CurrentManagedThreadId}");
        lock (_undoLock) {
            Develop.Diagnose("UNDO", $"Parse RemoveAll ENTER: chunk={chunk.KeyName} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
            Undo.RemoveAll(item => item is not null
                && string.Equals(GetChunkId(this, item.Command, item.RowKey is { Length: > 0 } rk ? Row.GetByKey(rk)?.ChunkValue ?? string.Empty : string.Empty), chunk.KeyName, StringComparison.OrdinalIgnoreCase));
            Develop.Diagnose("UNDO", $"Parse RemoveAll DONE: chunk={chunk.KeyName} Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
        }

        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseSuccessful = Parse(chunkContent, chunk.IsMain, parsedRowKeys);

        if (!parseSuccessful) {
            chunk.MarkLoadFailed();
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            return false;
        }

        // Zeilen, de nicht mehr im Chunk sind. löschen
        Row.RemoveObsoleteRows(RowsOfChunk(this, chunk), parsedRowKeys);

        return true;
    }

    protected override string SaveInternal() {
        if (!SaveRequired) { return string.Empty; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Erstelle Chunks der Tabelle '{Caption}'", 2);

        var x = LastChange;

        // Chunk-Inhalte generieren: Dateipfad -> Daten-Bytes (ohne Head, ohne EOF)
        var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);

        // System-Chunks mit ihren Undo-Daten
        chunkData[ComputeChunkPath(Filename, Chunk_MainData)] = [
            .. GenerateMainChunk(this),
            .. GenerateUndoChunk(this, false, Chunk_MainData)
        ];

        chunkData[ComputeChunkPath(Filename, Chunk_AdditionalUseCases)] = [
            .. GenerateUsesChunk(this),
            .. GenerateUndoChunk(this, false, Chunk_AdditionalUseCases)
        ];

        chunkData[ComputeChunkPath(Filename, Chunk_Variables)] = [
            .. GenerateHeadVariableChunks(this),
            .. GenerateUndoChunk(this, false, Chunk_Variables)
        ];

        chunkData[ComputeChunkPath(Filename, Chunk_Master)] = [
            .. GenerateMasterUserChunk(this),
            .. GenerateUndoChunk(this, false, Chunk_Master)
        ];

        // Row-Chunks — _rowdata immer berücksichtigen (Fallback für Undo gelöschter Zeilen)
        var rowChunkIds = RowChunkIdsInMemory(this);
        if (!rowChunkIds.Contains(Chunk_UnknownData)) {
            rowChunkIds.Add(Chunk_UnknownData);
        }

        foreach (var thisChunkId in rowChunkIds) {
            chunkData[ComputeChunkPath(Filename, thisChunkId)] = [
                .. GenerateRowChunk(this, false, thisChunkId),
                .. GenerateUndoChunk(this, false, thisChunkId)
            ];
        }

        if (x != LastChange) { return "Tabelle wurde während der Chunk-Generierung geändert"; }

        var saveResult = SaveChunkFiles(chunkData, 1200, Caption);
        if (!string.IsNullOrEmpty(saveResult)) {
            Freeze(saveResult);

            return saveResult;
        }

        SaveRequired = false;

        InitialSavePending = false;
        OnInvalidateView();
        return string.Empty;
    }

    /// <summary>
    /// Berechnet den vollständigen Chunk-Dateipfad aus MainFileName und ChunkId.
    /// </summary>
    private static string ComputeChunkPath(string mainFileName, string chunkId) {
        if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
            return mainFileName;
        }

        var folder = mainFileName.FilePath();
        var tablename = mainFileName.FileNameWithoutSuffix();

        return $"{folder}{tablename}\\{chunkId.ToLowerInvariant()}.bdbc";
    }

    #endregion
}