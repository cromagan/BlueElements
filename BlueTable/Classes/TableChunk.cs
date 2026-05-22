// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;

namespace BlueTable.Classes;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunk : TableFile {

    #region Fields

    public static readonly string Chunk_AdditionalUseCases = "_uses";

    public static readonly string Chunk_Master = "_master";
    public static readonly string Chunk_UnknownData = "_rowdata";
    public static readonly string Chunk_Variables = "_vars";

    private readonly HashSet<string> _dirtyChunks = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public TableChunk(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~TableChunk() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => _dirtyChunks.Count > 0;

    #endregion

    #region Methods

    public static List<Chunk>? GenerateNewChunks(TableFile tb, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed, bool addRows) {
        // Zentrales Dictionary zur Verwaltung ALLER Chunks (ID -> Byte-Liste)
        var chunks = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);

        // Initialisierung der Basis-Listen
        var mainBytes = new List<byte>();
        chunks[Chunk_MainData] = mainBytes;

        // Wenn Chunks erlaubt sind, eigene Listen erstellen, sonst auf mainBytes verweisen
        // HINWEIS: Wenn !chunksAllowed, schreiben alle Variablen in dieselbe Liste (mainBytes).
        var usesBytes = chunksAllowed ? [] : mainBytes;
        var varBytes = chunksAllowed ? [] : mainBytes;
        var masterUserBytes = chunksAllowed ? [] : mainBytes;
        var unknownDataBytes = chunksAllowed ? [] : mainBytes;

        if (chunksAllowed) {
            chunks[Chunk_AdditionalUseCases] = usesBytes;
            chunks[Chunk_Variables] = varBytes;
            chunks[Chunk_Master] = masterUserBytes;
            chunks[Chunk_UnknownData] = unknownDataBytes;
        }

        try {
            var x = tb.LastChange;

            // Metadaten schreiben
            SaveToByteList(mainBytes, TableDataType.GlobalShowPass, tb.GlobalShowPass);
            SaveToByteList(mainBytes, TableDataType.Creator, tb.Creator);
            SaveToByteList(mainBytes, TableDataType.CreateDateUTC, tb.CreateDate);
            SaveToByteList(mainBytes, TableDataType.LastSaveMainFileUtcDate, fileStateUtcDateToSave.ToString7());
            SaveToByteList(mainBytes, TableDataType.Caption, tb.Caption);

            SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterUser, tb.TemporaryTableMasterUser);
            SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterTimeUTC, tb.TemporaryTableMasterTimeUtc);
            SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterApp, tb.TemporaryTableMasterApp);
            SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterMachine, tb.TemporaryTableMasterMachine);
            SaveToByteList(masterUserBytes, TableDataType.TemporaryTableMasterId, tb.TemporaryTableMasterId);
            SaveToByteList(masterUserBytes, TableDataType.CheckPoint, $"~^{Chunk_Master.ToLowerInvariant()}^~");

            SaveToByteList(mainBytes, TableDataType.Tags, string.Join('\r', tb.Tags));
            SaveToByteList(mainBytes, TableDataType.DictionaryWords, string.Join('\r', tb.DictionaryWords));
            SaveToByteList(mainBytes, TableDataType.PermissionGroupsNewRow, string.Join('\r', tb.PermissionGroupsNewRow));
            SaveToByteList(mainBytes, TableDataType.TableAdminGroups, string.Join('\r', tb.TableAdmin));

            SaveToByteList(mainBytes, TableDataType.AssetFolder, tb.AssetFolder);
            SaveToByteList(mainBytes, TableDataType.RowQuickInfo, tb.RowQuickInfo);
            SaveToByteList(mainBytes, TableDataType.StandardFormulaFile, tb.StandardFormulaFile);

            try {
                foreach (var columnitem in tb.Column) {
                    if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                        SaveToByteList(mainBytes, columnitem);
                        SaveToByteList(usesBytes, TableDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ErrorType.Warning, "Fehler beim Iterieren über Column: " + ex.Message);
                return null;
            }

            if (chunksAllowed) {
                SaveToByteList(usesBytes, TableDataType.CheckPoint, $"~^{Chunk_AdditionalUseCases.ToLowerInvariant()}^~");
            }

            SaveToByteList(mainBytes, TableDataType.SortDefinition, tb.SortDefinition is null ? string.Empty : tb.SortDefinition.ParseableItems().FinishParseable());
            try {
                SaveToByteList(mainBytes, TableDataType.UniqueValues, string.Join('\r', tb.UniqueValues.Select(x => x.ParseableItems().FinishParseable())));
            } catch (Exception ex) {
                Develop.DebugPrint(ErrorType.Warning, "Fehler bei UniqueValues: " + ex.Message);
                return null;
            }
            SaveToByteList(mainBytes, TableDataType.ColumnArrangement, tb.ColumnArrangements.ToString(false));
            SaveToByteList(mainBytes, TableDataType.EventScript, tb.EventScript.ToString(true));
            SaveToByteList(mainBytes, TableDataType.EventScriptVersion, tb.EventScriptVersion.ToString5());
            SaveToByteList(mainBytes, TableDataType.CheckPoint, $"~^{Chunk_MainData.ToLowerInvariant()}^~");

            SaveToByteList(varBytes, TableDataType.TableVariables, tb.Variables.ToList().ToString(true));
            SaveToByteList(varBytes, TableDataType.CheckPoint, $"~^{Chunk_Variables.ToLowerInvariant()}^~");

            if (addRows) {
                // Rows verarbeiten
                try {
                    foreach (var thisRow in tb.Row) {
                        if (thisRow is null || thisRow.IsDisposed) { continue; }
                        var targetList = mainBytes;
                        if (chunksAllowed) {
                            var chunkId = GetChunkId(thisRow);
                            if (string.IsNullOrEmpty(chunkId)) { return null; }

                            // Universelle Lösung: Prüfen, ob ID bereits existiert (egal ob Master, Main oder Dynamisch)
                            if (!chunks.TryGetValue(chunkId, out targetList)) {
                                targetList = [];
                                chunks.Add(chunkId, targetList);
                            }
                        }
                        SaveToByteList(targetList, thisRow);
                    }
                } catch (Exception ex) {
                    Develop.DebugPrint(ErrorType.Warning, "Fehler beim Iterieren über Row: " + ex.Message);
                    return null;
                }

                if (x != tb.LastChange) { return null; }

                #region Undos

                var important = 0;
                var undoCount = 0;
                List<string> works2 = [];

                try {
                    List<UndoItem> undoSnapshot = [];
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    do {
                        try {
                            undoSnapshot = [.. tb.Undo];
                            break;
                        } catch {
                            Thread.Sleep(1);
                        }

                        if (sw.Elapsed.TotalSeconds > 3) { return null; }
                    } while (true);

                    var sortedUndoItems = undoSnapshot.Where(item => item?.LogsUndo(tb) == true).OrderByDescending(item => item.DateTimeUtc).ToList();

                    foreach (var thisWorkItem in sortedUndoItems) {
                        if (thisWorkItem?.LogsUndo(tb) == true) {
                            if (undoCount < 1000 ||
                                (thisWorkItem.Command == TableDataType.EventScript && important < 10)) {
                                undoCount++;
                                if (thisWorkItem.Command == TableDataType.EventScript) { important++; }

                                if (chunksAllowed) {
                                    var targetChunkId = GetChunkId(tb, thisWorkItem.Command, thisWorkItem.ChunkValue);
                                    if (!string.IsNullOrEmpty(targetChunkId)) {
                                        // Auch hier: Universelle Prüfung gegen das Dictionary
                                        if (!chunks.TryGetValue(targetChunkId, out var targetList)) {
                                            targetList = [];
                                            chunks.Add(targetChunkId, targetList);
                                        }
                                        SaveToByteList(targetList, TableDataType.Undo, thisWorkItem.ParseableItems().FinishParseable());
                                    }
                                } else {
                                    works2.Add(thisWorkItem.ParseableItems().FinishParseable());
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    Develop.DebugPrint(ErrorType.Warning, "Fehler beim Iterieren über Undo: " + ex.Message);
                    return null;
                }

                if (!chunksAllowed) {
                    SaveToByteList(mainBytes, TableDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
                }

                #endregion
            }

            // Abschluss und Übertragung in Chunks
            long totalLength = 0;
            var resultChunks = new List<Chunk>();

            // Wir iterieren über eine Kopie der Keys, um Modifikationen während der Iteration zu vermeiden
            foreach (var kvp_Key in chunks.Keys.ToList()) {
                var chunkPath = ComputeChunkPath(tb.Filename, kvp_Key);
                if (CachedFileSystem.Get<Chunk>(chunkPath) is not { } chunk) {
                    if (IO.CreateDirectory(chunkPath.FilePath()).IsFailed) { return null; }
                    chunk = new Chunk(tb.Filename, kvp_Key);
                }

                List<byte> bytes;

                if (chunksAllowed) {
                    var head = chunk.GetHeadBytes();
                    if (head.Count < 100) { return null; }
                    bytes = head.Concat(chunks[kvp_Key]).ToList();
                } else {
                    bytes = chunks[kvp_Key];
                }

                SaveToByteList(bytes, TableDataType.EOF, "END");

                chunk.EnsureContentLoaded();

                chunk.Content = bytes.ToArray();
                totalLength += chunk.Content.Length;
                resultChunks.Add(chunk);

                if (tb is TableChunk tc) {
                    tc._dirtyChunks.Add(chunk.KeyName);
                }
            }

            if (totalLength < minLen) {
                tb.Freeze("Chunk-Daten zu klein für Speicherung");
                return null;
            }

            if (x != tb.LastChange) { return null; }

            return resultChunks;
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fehler beim Generieren der Chunks: {ex.Message}");
            return null;
        }
    }

    public static string GetChunkId(RowItem r) => GetChunkId(r?.Table, TableDataType.UTF8Value_withoutSizeData, r?.ChunkValue ?? string.Empty);

    public static string GetChunkId(Table? tb, TableDataType type, string chunkvalue) {
        if (tb is not { IsDisposed: false }) { return string.Empty; }

        if (type is TableDataType.Command_RemoveColumn
                or TableDataType.Command_AddColumnByName) { return Chunk_MainData.ToLowerInvariant(); }

        if (type == TableDataType.Command_NewStart) { return string.Empty; }

        if (type.IsObsolete()) { return string.Empty; }
        if (type == TableDataType.ColumnSystemInfo) { return Chunk_AdditionalUseCases.ToLowerInvariant(); }
        if (type == TableDataType.TableVariables) { return Chunk_Variables.ToLowerInvariant(); }
        if (type is TableDataType.TemporaryTableMasterUser or TableDataType.TemporaryTableMasterTimeUTC) { return Chunk_Master.ToLowerInvariant(); }

        if (type.IsCellValue() || type is TableDataType.Undo or TableDataType.Command_AddRow or TableDataType.Command_RemoveRow) {
            switch (tb.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None) {
                case ChunkType.ByHash_1Char:
                    return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(1).ToLowerInvariant();

                case ChunkType.ByHash_2Chars:
                    return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(2).ToLowerInvariant();

                case ChunkType.ByHash_3Chars:
                    return chunkvalue.ToLowerInvariant().GetSHA256HashString().Right(3).ToLowerInvariant();

                case ChunkType.ByName:
                    var t = ColumnItem.MakeValidColumnKey(chunkvalue);
                    return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();

                default:
                    return Chunk_UnknownData.ToLowerInvariant();
            }
        }

        return Chunk_MainData.ToLowerInvariant();
    }

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var chunkId = GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        var result = LoadChunkWithChunkId(chunkId, false, Reason.RaiseEvents);

        if (result.IsFailed) { return result.FailedReason; }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) {
            return $"Interner Chunk-Fehler beim Schreibrecht anfordern {chunkId}";
        }
        return chunk.AcquireWriteAccess().FailedReason;
    }

    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (updateAllowed) {
            if (LoadChunkWithChunkId(Chunk_Master, false, Reason.RaiseEvents).IsFailed) { return false; }
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

        foreach (var thisvalue in chunkValues) {
            var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, thisvalue);

            if (LoadChunkWithChunkId(chunkId, false, Reason.RaiseEvents).IsFailed) { return false; }
        }

        return true;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (IsDisposed || !DropMessages) { return true; }
        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

        var loaded = false;
        var ok = true;

        OnLoading();

        if (!firstTime) {
            var result = LoadChunkWithChunkId(Chunk_MainData, false, Reason.NoUndo_NoInvalidate);
            if (result.IsFailed) { return false; }
            loaded = result.Value is true;
        }

        Column.GetSystems();

        if (firstTime) {
            if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }
        }

        List<string> list = [Chunk_AdditionalUseCases, Chunk_Master, Chunk_Variables, Chunk_UnknownData];

        foreach (var item in list) {
            var result = LoadChunkWithChunkId(item, false, Reason.NoUndo_NoInvalidate);
            loaded = loaded || result.Value is true;
            ok = ok && result.IsSuccessful;
        }

        if (loaded) { OnLoaded(firstTime, true); }

        if (ok) { TryToSetMeTemporaryMaster(); }

        return ok;
    }

    public string ChunkFolder() {
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, Chunk_MainData));
        return chunk?.ChunkFolder() ?? string.Empty;
    }

    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        return chunk is not null && !chunk.LoadFailed;
    }

    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        string[] checkIds = [Chunk_MainData,
            Chunk_Master,
            Chunk_Variables,
            Chunk_AdditionalUseCases,
            Chunk_UnknownData];

        foreach (var id in checkIds) {
            var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, id));
            if (chunk is null || chunk.LoadFailed) {
                var loadResult = LoadChunkWithChunkId(id, false, Reason.NoUndo_NoInvalidate);
                if (loadResult.IsFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}' ({loadResult.FailedReason})"; }
                chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, id));
                if (chunk is null || chunk.LoadFailed) { return $"Interner Chunk-Fehler bei Chunk '{id}'"; }
            }
        }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (string.IsNullOrEmpty(chunkValue)) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var result = LoadChunkWithChunkId(chunkId, false, Reason.RaiseEvents);

        if (result.IsFailed) { return result.FailedReason; }

        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
        if (chunk is null) {
            return $"Interner Chunk-Fehler bei Editier-Prüfung {chunkId}";
        } else {
            return chunk.IsNowEditable();
        }
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
            var result = LoadChunkWithChunkId(chunkId, false, Reason.NoUndo_NoInvalidate);
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
            Develop.DebugError("Fehler beim Chunk laden!");
            return;
        }

        #endregion

        #region alte Chunk-Dateien löschen

        var chunkPath = ChunkFolder();

        if (IO.DirectoryExists(chunkPath)) {
            var chunkFiles = CachedFileSystem.GetFileNames(chunkPath, ["*.bdbc"]).ToList();

            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_AdditionalUseCases}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Master}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_UnknownData}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Variables}.bdbc", false);

            foreach (var file in chunkFiles) {
                IO.DeleteFile(file, false);
            }
        }

        #endregion

        _dirtyChunks.Add(Chunk_MainData);
        RefreshDirtyChunks();

        _ = SaveInternal(DateTime.UtcNow);
    }

    public List<RowItem> RowsOfChunk(Chunk chunk) => [.. Row.Where(r => GetChunkId(r) == chunk.KeyName)];

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

        Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {chunkid} wurde nicht richtig gespeichert", 0);
        return false;
    }

    protected override void Dispose(bool disposing) {
        UnMasterMe();
        base.Dispose(disposing);
    }

    protected override bool LoadMainData() => LoadChunkWithChunkId(Chunk_MainData, true, Reason.NoUndo_NoInvalidate).IsSuccessful;

    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        if (IsGenericEditable(false) is { Length: > 0 } f) { return f; }

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Erstelle Chunks der Tabelle '{Caption}'", 2);

        // Generiere die Chunks
        var chunks = GenerateNewChunks(this, 1200, setfileStateUtcDateTo, true, true);
        if (chunks is null || chunks.Count < 5) {
            return "Fehler beim Generieren der Chunks";
        }

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Diskette, $"Speichere {chunks.Count} Chunks der Tabelle '{Caption}'", 2);

        CachedFileSystem.SaveAll(true);

        // DirtyChunks neu ermitteln: Chunks sind nicht mehr dirty, wenn sie gespeichert sind
        RefreshDirtyChunks();

        LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        OnInvalidateView();
        return string.Empty;
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var chunkId = GetChunkId(this, type, newChunkId);
        if (!string.IsNullOrEmpty(chunkId)) {
            _dirtyChunks.Add(chunkId);
        }

        var oldId = GetChunkId(this, type, oldChunkId);
        if (!string.IsNullOrEmpty(oldId) && oldId != chunkId) {
            _dirtyChunks.Add(oldId);
        }

        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkId"></param>
    /// <param name="isFirst"></param>
    /// <param name="reason"></param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    private OperationResult LoadChunkWithChunkId(string chunkId, bool isFirst, Reason reason) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        // Prüfung auf laufende Speicherungen
        // Wir warten nur, wenn der Chunk wirklich gerade aktiv gespeichert wird (IsSaving).
        // Ein leerer Chunk (IsSaved = true bei Content = null) darf uns nicht blockieren.
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));

        if (chunk is not null && chunk.IsSaving) {
            if (!WaitChunkIsSaved(chunkId)) {
                return OperationResult.Failed($"Timeout beim Warten auf Speicherung von {chunkId}");
            }
        }

        if (chunk is null) {
            chunk = new Chunk(Filename, chunkId);

            if (!CachedFileSystem.FileExists(chunk.Filename, true)) {
                // Chunk fehlt auf der Festplatte — versuchen, aus Backup (.bak) wiederherzustellen
                var recovered = TableFile.TryRecoverFromBackup(chunk.Filename, chunkId, 10000);

                if (!recovered) {
                    // Für den Hauptchunk: nicht leer erstellen — Datenverlust vermeiden
                    if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
                        Freeze($"Hauptchunk fehlt auf der Festplatte und kein gültiges Backup vorhanden");
                        Develop.DebugPrint(ErrorType.Warning, $"Hauptchunk fehlt, Tabelle eingefroren: {Filename.FileNameWithoutSuffix()}");
                        return OperationResult.Failed("Hauptchunk fehlt, keine Wiederherstellung möglich");
                    }

                    // Für Nebenchunks: leeren Chunk erstellen (normaler Fall, z.B. neuer Hash-Chunk)
                    Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Erstelle neuen Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'", 0);
                    chunk.AcquireWriteAccess();
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
                if (!chunk.IsSaved || _dirtyChunks.Contains(chunkId)) {
                    // Chunk hat ungespeicherte Änderungen — Reload überspringen,
                    // um Datenverlust zu vermeiden. Die aktuellen Daten bleiben erhalten.
                    return OperationResult.SuccessValue(false);
                }
                chunk.Invalidate();
            }

            if (!chunk.EnsureContentLoaded()) {
                return OperationResult.Failed("Chunk Laden fehlgeschlagen");
            }

            if (reason.HasFlag(Reason.RaiseEvents)) { OnLoading(); }

            if (!Parse(chunk, reason)) {
                return OperationResult.Failed("Parsen fehlgeschlagen");
            }

            loaded = true;
            if (reason.HasFlag(Reason.RaiseEvents)) { OnLoaded(isFirst, chunk.IsMain); }
        }

        return OperationResult.SuccessValue(loaded);
    }

    private bool Parse(Chunk chunk, Reason reason) {
        if (chunk.LoadFailed) { return false; }

        var chunkContent = chunk.Content;
        if (chunkContent.Length == 0) { return true; }

        Undo.RemoveAll(item => item is not null
            && string.Equals(GetChunkId(this, item.Command, item.ChunkValue), chunk.KeyName, StringComparison.OrdinalIgnoreCase));

        var parsedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseSuccessful = Parse(chunkContent, chunk.IsMain, reason, parsedRowKeys);

        if (!parseSuccessful) {
            chunk.MarkLoadFailed();
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            return false;
        }

        // Zeilen, de nicht mehr im Chunk sind. löschen
        Row.RemoveObsoleteRows(RowsOfChunk(chunk), parsedRowKeys, reason);

        return true;
    }

    private void RefreshDirtyChunks() {
        // Erst: Alle Chunks dieser Tabelle aus dem CachedFileSystem durchsuchen
        // und die, die nicht gespeichert sind (IsSaved == false), als dirty markieren
        var allChunks = CachedFileSystem.GetAll<Chunk>();
        foreach (var chunk in allChunks) {
            if (chunk.IsDisposed || chunk.LoadFailed) { continue; }
            if (!string.Equals(chunk.MainFileName, Filename, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (!chunk.IsSaved) {
                _dirtyChunks.Add(chunk.KeyName);
            }
        }

        // Dann: Chunks entfernen, die jetzt gespeichert sind
        var saved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var chunkId in _dirtyChunks) {
            var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, chunkId));
            if (chunk is not null && chunk.IsSaved) {
                saved.Add(chunkId);
            }
        }
        foreach (var id in saved) { _dirtyChunks.Remove(id); }
    }

    #endregion
}