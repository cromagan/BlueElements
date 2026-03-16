// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

    /// <summary>
    /// Ein Save ist erforderlich, wenn die Tabelle "Dirty" ist (Datenänderung)
    /// ODER wenn bereits generierte Chunks noch nicht physisch auf Disk geschrieben wurden.
    /// </summary>
    protected override bool SaveRequired => base.SaveRequired || CachedFileSystem.GetAll<Chunk>()
        .Any(chunk => chunk.MainFileName == Filename && !chunk.IsSaved);

    #endregion

    #region Methods

    public static List<Chunk>? GenerateNewChunks(TableFile tb, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed) {
        // Zentrales Dictionary zur Verwaltung ALLER Chunks (ID -> Byte-Liste)
        var chunks = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);

        // Initialisierung der Basis-Listen
        var mainBytes = new List<byte>();
        chunks[Chunk_MainData] = mainBytes;

        // Wenn Chunks erlaubt sind, eigene Listen erstellen, sonst auf mainBytes verweisen
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

            SaveToByteList(mainBytes, TableDataType.Tags, tb.Tags.JoinWithCr());
            SaveToByteList(mainBytes, TableDataType.PermissionGroupsNewRow, tb.PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(mainBytes, TableDataType.TableAdminGroups, tb.TableAdmin.JoinWithCr());

            SaveToByteList(mainBytes, TableDataType.AdditionalFilesPath, tb.AdditionalFilesPath);
            SaveToByteList(mainBytes, TableDataType.RowQuickInfo, tb.RowQuickInfo);
            SaveToByteList(mainBytes, TableDataType.StandardFormulaFile, tb.StandardFormulaFile);

            foreach (var columnitem in tb.Column) {
                if (!string.IsNullOrEmpty(columnitem?.KeyName) && !columnitem.IsDisposed) {
                    SaveToByteList(mainBytes, columnitem);
                    SaveToByteList(usesBytes, TableDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
                }
            }

            SaveToByteList(mainBytes, TableDataType.SortDefinition, tb.SortDefinition == null ? string.Empty : tb.SortDefinition.SerializableContent().Serialize());
            SaveToByteList(mainBytes, TableDataType.ColumnArrangement, tb.ColumnArrangements);
            SaveToByteList(mainBytes, TableDataType.EventScript, tb.EventScript.ToString(true));
            SaveToByteList(mainBytes, TableDataType.EventScriptVersion, tb.EventScriptVersion.ToString5());

            SaveToByteList(varBytes, TableDataType.TableVariables, tb.Variables.ToList().ToString(true));

            // Rows verarbeiten
            foreach (var thisRow in tb.Row) {
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

            if (x != tb.LastChange) { return null; }

            #region Undos

            var important = 0;
            var undoCount = 0;
            List<string> works2 = [];

            var sortedUndoItems = tb.Undo.Where(item => item?.LogsUndo(tb) == true).OrderByDescending(item => item.DateTimeUtc);

            foreach (var thisWorkItem in sortedUndoItems) {
                if (thisWorkItem?.LogsUndo(tb) == true) {
                    if (undoCount < 1000 || thisWorkItem.Command == TableDataType.EventScript && important < 10) {
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
                                SaveToByteList(targetList, TableDataType.Undo, thisWorkItem.SerializableContent().Serialize());
                            }
                        } else {
                            works2.Add(thisWorkItem.SerializableContent().Serialize());
                        }
                    }
                }
            }

            if (!chunksAllowed) {
                SaveToByteList(mainBytes, TableDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
            }

            #endregion

            // Abschluss und Übertragung in Chunks
            long totalLength = 0;
            var resultChunks = new List<Chunk>();

            // Wir iterieren über alle im Dictionary registrierten Chunks
            foreach (var kvp in chunks) {
                var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(tb.Filename, kvp.Key));
                if (chunk == null) { return null; }

                var bytes = kvp.Value;

                if (chunksAllowed) {
                    var head = chunk.GetHeadAndSetEditor(false);
                    if (head == null || head.Count < 100) { return null; }
                    bytes = head.Concat(kvp.Value).ToList();
                }

                SaveToByteList(bytes, TableDataType.EOF, "END");

                chunk.Content = bytes.ToArray();
                totalLength += chunk.Content.Length;
                resultChunks.Add(chunk);
            }

            if (totalLength < minLen) {
                CachedFileSystem.FreezeAll("Interner Datenfehler, Notbremse");
                return null;
            }

            if (x != tb.LastChange) { return null; }

            return resultChunks;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return GenerateNewChunks(tb, minLen, fileStateUtcDateToSave, chunksAllowed);
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
                    var t = ColumnItem.MakeValidColumnName(chunkvalue);
                    return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();

                default:
                    return Chunk_UnknownData.ToLowerInvariant();
            }
        }

        return Chunk_MainData.ToLowerInvariant();
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

        DropMessage(ErrorType.Info, "Lade Chunks von '" + KeyName + "'");

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
            if (!IO.CreateDirectory(ChunkFolder())) { return false; }
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
        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, Chunk_MainData));
        return chunk?.ChunkFolder() ?? string.Empty;
    }

    public bool ChunkIsLoaded(string chunkVal) {
        var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, chunkVal);
        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, chunkId));
        return chunk != null && !chunk.LoadFailed;
    }

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        var chunkId = GetChunkId(this, type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        var result = LoadChunkWithChunkId(chunkId, false, Reason.RaiseEvents);

        if (result.IsFailed) { return result.FailedReason; }

        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, chunkId));
        if (chunk == null) {
            return $"Interner Chunk-Fehler beim Schreibrecht anfordern {chunkId}";
        }
        return chunk.GrantWriteAccess().FailedReason;
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
            var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, id));
            if (chunk == null || chunk.LoadFailed) { return $"Interner Chunk-Fehler bei Chunk-{id}"; }
        }

        return string.Empty;
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
        if (!string.IsNullOrEmpty(GrantWriteAccess(TableDataType.TemporaryTableMasterUser, string.Empty))) { return; }

        base.MasterMe();
    }

    public override void ReorganizeChunks() {
        if (!IsEditable(false)) { return; }

        base.ReorganizeChunks();

        #region Erst alle Chunks laden

        if (!LoadTableRows(false, -1)) {
            Develop.DebugPrint(ErrorType.Error, "Fehler beim Chunk laden!");
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

        IsDirty = true;
        SaveInternal(DateTime.UtcNow).GetAwaiter().GetResult();
    }

    public List<RowItem> RowsOfChunk(Chunk chunk) => [.. Row.Where(r => GetChunkId(r) == chunk.KeyName)];

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Speicherung ausgeführt wurde
    /// </summary>
    /// <returns>True, wenn die Speicherung erfolgreich abgeschlossen wurde, sonst False</returns>
    public bool WaitChunkIsSaved(string chunkid) {
        var t = Stopwatch.StartNew();
        var lastMessageTime = 0L;

        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, chunkid));
        if (chunk == null)
            return true;

        // Wir prüfen den Status des Chunks direkt über seine Properties
        while (!chunk.IsSaved) {
            Thread.Sleep(20); // Längere Pause zur Reduzierung der CPU-Last

            if (t.ElapsedMilliseconds > 120 * 1000) {
                Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {chunkid} wurde nicht richtig gespeichert", 0);
                return false; // Explizit false zurückgeben, wenn die Initialisierung fehlschlägt
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.Message(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Warte auf Abschluss der Speicherung des Chunks {chunkid}", 0);
            }
        }

        return true; // Explizit true zurückgeben, wenn die Initialisierung erfolgreich ist
    }

    internal override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (string.IsNullOrEmpty(chunkValue)) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var result = LoadChunkWithChunkId(chunkId, false, Reason.RaiseEvents);

        if (result.IsFailed) { return result.FailedReason; }

        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, chunkId));
        if (chunk == null) {
            return $"Interner Chunk-Fehler bei Editier-Prüfung {chunkId}";
        } else {
            return chunk.IsNowEditable();
        }
    }

    protected override void Dispose(bool disposing) {
        UnMasterMe();
        base.Dispose(disposing);
    }

    protected override bool LoadMainData() => LoadChunkWithChunkId(Chunk_MainData, true, Reason.NoUndo_NoInvalidate).IsSuccessful;

    protected override async Task<string> SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();
        DropMessage(ErrorType.DevelopInfo, $"Erstelle Chunks der Tabelle '{Caption}'");

        // Generiere die Chunks (setzt _isDirty intern auf false bei Erfolg)
        var chunks = GenerateNewChunks(this, 1200, setfileStateUtcDateTo, true);
        if (chunks == null || chunks.Count < 5) {
            return "Fehler beim Generieren der Chunks";
        }
        IsDirty = false;

        CachedFileSystem.SaveAll(true);
        LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        OnInvalidateView();
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
        var chunk = CachedFileSystem.GetOrCreate<Chunk>(Chunk.ComputeChunkPath(Filename, chunkId));

        if (chunk != null && chunk.IsSaving) {
            if (!WaitChunkIsSaved(chunkId)) {
                return OperationResult.Failed($"Timeout beim Warten auf Speicherung von {chunkId}");
            }
        }

        if (chunk == null) {
            DropMessage(ErrorType.Info, $"Lade Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'");
            chunk = new Chunk(Filename, chunkId);
        }

        // Ein Chunk muss geladen werden, wenn er stale ist, das Laden fehlschlug oder der Inhalt leer ist.
        var needLoading = chunk.LoadFailed || chunk.IsStale() || chunk.ContentLength == 0;

        var loaded = false;

        if (needLoading) {
            Develop.AbortAppIfStackOverflow();
            chunk.WaitDiskOperationFinished();   // Sicherstellen, dass kein I/O mehr läuft

            // Invalidate nur aufrufen, wenn wir wirklich sicher sind, dass wir neu laden wollen
            // und nicht bereits ein Ladevorgang läuft.
            if (!chunk.IsLoading) {
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

        var rowsToRemove = RowsOfChunk(chunk);

        if (rowsToRemove.Count > 0) {
            // Zeilen und zugehörige Zellen entfernen
            foreach (var row in rowsToRemove) {
                Row.ExecuteCommand(TableDataType.Command_RemoveRow, row.KeyName, reason, null, null);
            }

            // Verwaiste Zellen entfernen
            Cell.RemoveOrphans();
        }

        if (chunk.IsMain) {
            Undo.Clear();
            Row.RemoveNullOrEmpty();
            Cell.Clear();
        }

        // Zuerst parsen
        var parseSuccessful = Parse(chunkContent, chunk.IsMain, reason);

        if (!parseSuccessful) {
            chunk.MarkLoadFailed();
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            return false;
        }
        return true;
    }

    #endregion
}