// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using System.Linq;
using System.Threading;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueTable;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunk : TableFile {

    #region Fields

    public static readonly string Chunk_AdditionalUseCases = "_uses";
    public static readonly string Chunk_MainData = "MainData";
    public static readonly string Chunk_Master = "_master";
    public static readonly string Chunk_UnknownData = "_rowdata";
    public static readonly string Chunk_Variables = "_vars";

    /// <summary>
    /// Flüchtiger Speicher, wird nur zum Halten von Daten verwendet.
    /// </summary>
    private readonly ConcurrentDictionary<string, Chunk> _chunks = new();

    private readonly ConcurrentDictionary<string, byte> chunksBeingSaved = new ConcurrentDictionary<string, byte>();

    #endregion

    #region Constructors

    public TableChunk(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~TableChunk() { Dispose(false); }

    #endregion

    #region Properties

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => _chunks.Values.Any(chunk => chunk.SaveRequired);

    #endregion

    #region Methods

    public static List<Chunk>? GenerateNewChunks(TableFile db, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed) {
        var chunks = new List<Chunk>();

        var mainChunk = new Chunk(db.Filename, Chunk_MainData);
        mainChunk.InitByteList();
        chunks.Add(mainChunk);

        var usesChunk = mainChunk;
        var varChunk = mainChunk;
        var masterUserChunk = mainChunk;
        var unknownDataChunk = mainChunk;

        if (chunksAllowed) {
            usesChunk = new Chunk(db.Filename, Chunk_AdditionalUseCases);
            usesChunk.InitByteList();
            chunks.Add(usesChunk);

            varChunk = new Chunk(db.Filename, Chunk_Variables);
            varChunk.InitByteList();
            chunks.Add(varChunk);

            masterUserChunk = new Chunk(db.Filename, Chunk_Master);
            masterUserChunk.InitByteList();
            chunks.Add(masterUserChunk);

            unknownDataChunk = new Chunk(db.Filename, Chunk_UnknownData);
            unknownDataChunk.InitByteList();
            chunks.Add(unknownDataChunk);
        }

        try {
            var x = db.LastChange;

            mainChunk.SaveToByteList(TableDataType.GlobalShowPass, db.GlobalShowPass);
            mainChunk.SaveToByteList(TableDataType.Creator, db.Creator);
            mainChunk.SaveToByteList(TableDataType.CreateDateUTC, db.CreateDate);
            mainChunk.SaveToByteList(TableDataType.LastSaveMainFileUtcDate, fileStateUtcDateToSave.ToString7());
            mainChunk.SaveToByteList(TableDataType.Caption, db.Caption);

            masterUserChunk.SaveToByteList(TableDataType.TemporaryTableMasterUser, db.TemporaryTableMasterUser);
            masterUserChunk.SaveToByteList(TableDataType.TemporaryTableMasterTimeUTC, db.TemporaryTableMasterTimeUtc);

            mainChunk.SaveToByteList(TableDataType.Tags, db.Tags.JoinWithCr());
            mainChunk.SaveToByteList(TableDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            mainChunk.SaveToByteList(TableDataType.TableAdminGroups, db.TableAdmin.JoinWithCr());

            mainChunk.SaveToByteList(TableDataType.AdditionalFilesPath, db.AdditionalFilesPath);
            mainChunk.SaveToByteList(TableDataType.RowQuickInfo, db.RowQuickInfo);
            mainChunk.SaveToByteList(TableDataType.StandardFormulaFile, db.StandardFormulaFile);

            //Table.SaveToByteList(List, enTableDataType.LastColumnKey, _LastColumnKey.ToString(false));
            foreach (var columnitem in db.Column) {
                if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName) && !columnitem.IsDisposed) {
                    mainChunk.SaveToByteList(columnitem);
                    usesChunk.SaveToByteList(TableDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
                }
            }

            mainChunk.SaveToByteList(TableDataType.SortDefinition, db.SortDefinition == null ? string.Empty : db.SortDefinition.ParseableItems().FinishParseable());

            mainChunk.SaveToByteList(TableDataType.ColumnArrangement, db.ColumnArrangements);

            mainChunk.SaveToByteList(TableDataType.EventScript, db.EventScript.ToString(true));
            //mainChunk.SaveToByteList(TableDataType.EventScriptEdited, db.EventScriptEdited.ToString(true));
            mainChunk.SaveToByteList(TableDataType.EventScriptVersion, db.EventScriptVersion.ToString5());

            //mainChunk.SaveToByteList(TableDataType.NeedsScriptFix, db.NeedsScriptFix);
            varChunk.SaveToByteList(TableDataType.TableVariables, db.Variables.ToList().ToString(true));

            foreach (var thisRow in db.Row) {

                #region Chunk bestimmen

                var rowchunk = mainChunk;
                if (chunksAllowed) {
                    var chunkId = GetChunkId(thisRow);
                    if (string.IsNullOrEmpty(chunkId)) { return null; }
                    rowchunk = GetOrMakechunk(chunks, db, chunkId);
                }

                #endregion

                rowchunk.SaveToByteList(thisRow);
            }

            if (x != db.LastChange) { return null; } // Works haben sich evtl. geändert

            #region Undos

            var important = 0;
            var undoCount = 0;
            List<string> works2 = [];

            var sortedUndoItems = db.Undo.Where(item => item != null && item.LogsUndo(db)).OrderByDescending(item => item.DateTimeUtc);

            foreach (var thisWorkItem in sortedUndoItems) {
                if (thisWorkItem != null && thisWorkItem.LogsUndo(db)) {
                    if (undoCount < 1000 || (thisWorkItem.Command == TableDataType.EventScript && important < 10)) {
                        undoCount++;
                        if (thisWorkItem.Command == TableDataType.EventScript) { important++; }

                        if (chunksAllowed) {
                            var targetChunkId = GetChunkId(db, thisWorkItem.Command, thisWorkItem.ChunkValue);
                            if (!string.IsNullOrEmpty(targetChunkId)) {
                                var targetChunk = GetOrMakechunk(chunks, db, targetChunkId);
                                targetChunk.SaveToByteList(TableDataType.Undo, thisWorkItem.ParseableItems().FinishParseable());
                            }
                        } else {
                            works2.Add(thisWorkItem.ParseableItems().FinishParseable());
                        }
                    }
                }
            }

            if (!chunksAllowed) {
                mainChunk.SaveToByteList(TableDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
            }

            #endregion

            long l = 0;
            foreach (var thisChunk in chunks) {
                thisChunk.SaveToByteListEOF();
                l += thisChunk.DataLength;
            }

            if (l < minLen) { return null; }

            return x != db.LastChange ? null : chunks; // Stand stimmt nicht mehr
        } catch {
            Develop.CheckStackOverflow();
            return GenerateNewChunks(db, minLen, fileStateUtcDateToSave, chunksAllowed);
        }
    }

    public static string GetChunkId(RowItem r) => GetChunkId(r?.Table, TableDataType.UTF8Value_withoutSizeData, r?.ChunkValue ?? string.Empty);

    public static string GetChunkId(Table? db, TableDataType type, string chunkvalue) {
        if (db is not { IsDisposed: false }) { return string.Empty; }

        if (type is TableDataType.Command_RemoveColumn
                or TableDataType.Command_AddColumnByName) { return Chunk_MainData.ToLower(); }

        if (type == TableDataType.Command_NewStart) { return string.Empty; }

        if (type.IsObsolete()) { return string.Empty; }
        if (type == TableDataType.ColumnSystemInfo) { return Chunk_AdditionalUseCases.ToLower(); }
        if (type == TableDataType.TableVariables) { return Chunk_Variables.ToLower(); }
        if (type is TableDataType.TemporaryTableMasterUser or TableDataType.TemporaryTableMasterTimeUTC) { return Chunk_Master.ToLower(); }

        if (type.IsCellValue() || type is TableDataType.Undo or TableDataType.Command_AddRow or TableDataType.Command_RemoveRow) {
            switch (db.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None) {
                case ChunkType.ByHash_1Char:
                    return chunkvalue.ToLower().GetHashString().Right(1).ToLower();

                case ChunkType.ByHash_2Chars:
                    return chunkvalue.ToLower().GetHashString().Right(2).ToLower();

                case ChunkType.ByHash_3Chars:
                    return chunkvalue.ToLower().GetHashString().Right(3).ToLower();

                case ChunkType.ByName:
                    var t = ColumnItem.MakeValidColumnName(chunkvalue);
                    return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLower();

                default:
                    return Chunk_UnknownData.ToLower();
            }
        }

        return Chunk_MainData.ToLower();
    }

    public override bool AmITemporaryMaster(int ranges, int rangee) {
        if (!LoadChunkWithChunkId(Chunk_Master, false)) { return false; }

        return base.AmITemporaryMaster(ranges, rangee);
    }

    public override string AreAllDataCorrect() {
        var f = base.AreAllDataCorrect();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (!_chunks.TryGetValue(Chunk_MainData.ToLower(), out var chkmain) || chkmain.LoadFailed) { return "Interner Chunk-Fehler bei Chunk-Maindata"; }
        if (!_chunks.TryGetValue(Chunk_Master.ToLower(), out var chkmaster) || chkmaster.LoadFailed) { return "Interner Chunk-Fehler bei Chunk-Master"; }
        if (!_chunks.TryGetValue(Chunk_Variables.ToLower(), out var chkvars) || chkvars.LoadFailed) { return "Interner Chunk-Fehler bei Chunk-Variablen"; }
        if (!_chunks.TryGetValue(Chunk_AdditionalUseCases.ToLower(), out var chkuses) || chkuses.LoadFailed) { return "Interner Chunk-Fehler bei Chunk-Uses"; }
        if (!_chunks.TryGetValue(Chunk_UnknownData.ToLower(), out var chkukn) || chkukn.LoadFailed) { return "Interner Chunk-Fehler bei Chunk-UnknownData"; }

        return string.Empty;
    }

    public override bool BeSureAllDataLoaded(int anzahl) {
        if (!base.BeSureAllDataLoaded(anzahl)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (!DirectoryExists(chunkPath)) { return true; }

        var files = GetFiles(chunkPath, "*.bdbc", System.IO.SearchOption.TopDirectoryOnly);
        var fileQuery = anzahl < 0 || anzahl >= files.Length
            ? files
            : files.Select(GetFileInfo)
                  .OrderBy(f => f?.LastWriteTime ?? DateTime.Now)
                  .Take(anzahl)
                  .Select(f => f?.FullName ?? string.Empty)
                  .ToArray();

        foreach (var file in fileQuery) {
            var chunkId = file.FileNameWithoutSuffix();

            var ok = LoadChunkWithChunkId(chunkId, false);
            if (!ok) { return false; }
        }

        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkValue">kann auch mehrere Chunkvalues mit \r\n getrennt enthalten.</param>
    /// <returns>Ob ein Load aller Chunks stattgefunden hat</returns>
    public override bool BeSureRowIsLoaded(string chunkValue) {
        if (!base.BeSureRowIsLoaded(chunkValue)) { return false; }

        var chunkValues = chunkValue.SplitAndCutByCrToList().SortedDistinctList();

        foreach (var thisvalue in chunkValues) {
            var chunkId = GetChunkId(this, TableDataType.UTF8Value_withoutSizeData, thisvalue);

            if (!LoadChunkWithChunkId(chunkId, false)) { return false; }
        }

        return true;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        DropMessage(ErrorType.Info, "Lade Chunks von '" + KeyName + "'");

        if (!firstTime && !LoadChunkWithChunkId(Chunk_MainData, false)) { return false; }

        Column.GetSystems();

        if (firstTime) {
            if (!CreateDirectory(ChunkFolder())) { return false; }
        }

        if (!LoadChunkWithChunkId(Chunk_AdditionalUseCases, false)) { return false; }
        if (!LoadChunkWithChunkId(Chunk_Master, false)) { return false; }
        if (!LoadChunkWithChunkId(Chunk_Variables, false)) { return false; }
        if (!LoadChunkWithChunkId(Chunk_UnknownData, false)) { return false; }

        TryToSetMeTemporaryMaster();
        return true;
    }

    public string ChunkFolder() {
        if (!_chunks.TryGetValue(Chunk_MainData.ToLower(), out var chunk)) {
            return string.Empty;
        } else {
            return chunk.ChunkFolder();
        }
    }

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (chunkValue is not { }) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var ok = LoadChunkWithChunkId(chunkId, false);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            return $"Interner Chunk-Fehler beim Schreibrecht anfordern {chunkId}";
        } else {
            return chunk.GrantWriteAccess();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="chunkId"></param>
    /// <param name="important">Steuert, ob es dringen nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns>Ob ein Load stattgefunden hat</returns>
    public bool LoadChunkWithChunkId(string chunkId, bool isFirst) {
        if (string.IsNullOrEmpty(chunkId)) { return false; }
        chunkId = chunkId.ToLower();

        while (chunksBeingSaved.Count > 10) {
            DropMessage(ErrorType.Info, $"Warte auf Abschluss von {chunksBeingSaved.Count} Chunk Speicherungen.... Bitte Geduld, gleich gehts weiter.");
            Pause(1000, true);
        }

        bool ok;
        bool parsed;
        Chunk? chunk = null;

        // Kurzer Lock nur für die kritischen Prüfungen - Race Condition vermeiden
        lock (chunksBeingSaved) {
            _chunks.TryGetValue(chunkId, out chunk);
        }

        if (chunk == null) {
            DropMessage(ErrorType.Info, $"Lade Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'");
            chunk = new Chunk(Filename, chunkId);
            parsed = false;
        } else {
            parsed = !chunk.LoadFailed;
        }

        if (!parsed) {
            Develop.CheckStackOverflow();
            chunk.LoadBytesFromDisk(false);
            chunk.WaitBytesLoaded();
            if (chunk.LoadFailed) { return false; }

            WaitChunkIsSaved(chunkId);
            OnLoading();
            ok = Parse(chunk);

            if (ok) {
                _ = _chunks.AddOrUpdate(chunk.KeyName, chunk, (key, oldValue) => chunk);
            }

            OnLoaded(isFirst);
        } else {
            ok = true;
        }

        // Nur als leer markieren, wenn nicht gleichzeitig ein Speichervorgang läuft
        // Kurzer Lock um Race Condition zu vermeiden
        lock (chunksBeingSaved) {
            if (ok && chunk.Bytes.Count == 0 && !chunksBeingSaved.ContainsKey(chunkId)) {
                chunk.SaveRequired = true;
            }
        }

        return ok;
    }

    public override void ReorganizeChunks() {
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        base.ReorganizeChunks();

        #region Erst alle Chunks laden

        if (!BeSureAllDataLoaded(-1)) {
            Develop.DebugPrint(ErrorType.Error, "Fehler beim Chunk laden!");
            return;
        }

        #endregion

        #region alte Chunk-Dateien löschen

        var chunkPath = ChunkFolder();

        if (DirectoryExists(chunkPath)) {
            var chunkFiles = GetFiles(chunkPath, "*.bdbc", System.IO.SearchOption.TopDirectoryOnly).ToList();

            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_AdditionalUseCases}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Master}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_UnknownData}.bdbc", false);
            chunkFiles.RemoveString($"{ChunkFolder()}{Chunk_Variables}.bdbc", false);

            foreach (var file in chunkFiles) {
                _ = DeleteFile(file, false);
                var key = file.FileNameWithoutSuffix().ToLower();
                _chunks.TryRemove(key, out _);
            }
        }

        #endregion

        _ = SaveInternal(DateTime.UtcNow);
    }

    public List<RowItem> RowsOfChunk(Chunk chunk) => Row.Where(r => GetChunkId(r) == chunk.KeyName).ToList();

    public override void TryToSetMeTemporaryMaster() {
        if (!string.IsNullOrEmpty(GrantWriteAccess(TableDataType.TemporaryTableMasterUser, string.Empty))) {
            return;
        }

        base.TryToSetMeTemporaryMaster();
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Speicherung ausgeführt wurde
    /// </summary>
    /// <returns>True, wenn die Speicherung erfolgreich abgeschlossen wurde, sonst False</returns>
    public bool WaitChunkIsSaved(string chunkid) {
        var t = Stopwatch.StartNew();
        var lastMessageTime = 0L;

        while (chunksBeingSaved.ContainsKey(chunkid)) {
            Thread.Sleep(20); // Längere Pause zur Reduzierung der CPU-Last

            if (t.ElapsedMilliseconds > 120 * 1000) {
                Develop.Message?.Invoke(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Abbruch, Chunk {chunkid} wurde nicht richtig gespeichert", 0);
                return false; // Explizit false zurückgeben, wenn die Initialisierung fehlschlägt
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.Message?.Invoke(ErrorType.Info, this, "Chunk-Laden", ImageCode.Puzzle, $"Warte auf Abschluss der Speicherung des Chunks {chunkid}", 0);
            }
        }

        return true; // Explizit true zurückgeben, wenn die Initialisierung erfolgreich ist
    }

    internal override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (chunkValue is not { }) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var ok = LoadChunkWithChunkId(chunkId, false);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            return $"Interner Chunk-Fehler bei Editier-Prüfung {chunkId}";
        } else {
            return chunk.IsEditable();
        }
    }

    protected override void Dispose(bool disposing) {
        UnMasterMe();
        base.Dispose(disposing);
        _chunks.Clear();
    }

    protected override bool LoadMainData() => LoadChunkWithChunkId(Chunk_MainData, true);

    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        var f = CanSaveMainChunk();
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();

        #region Neue Chunks-Erstellen (chunksnew)

        DropMessage(ErrorType.DevelopInfo, $"Erstelle Chunks der Tabelle '{Caption}'");

        var chunksnew = GenerateNewChunks(this, 1200, setfileStateUtcDateTo, true);
        if (chunksnew == null || chunksnew.Count == 0) { return "Fehler bei der Chunk Erzeugung"; }

        #endregion

        #region Veränderte oder neue Chunks ermitteln (chunksToSave)

        var chunksToSave = new List<Chunk>();
        foreach (var thisChunk in chunksnew) {
            _ = _chunks.TryGetValue(thisChunk.KeyName, out var existingChunk);
            if (existingChunk == null || existingChunk.SaveRequired) {
                chunksBeingSaved.TryAdd(thisChunk.KeyName, 0);
                chunksToSave.Add(thisChunk);
            }
        }

        #endregion

        #region Chunks speichern, Fehler ermitteln (allok)

        var allok = string.Empty;
        try {
            foreach (var thisChunk in chunksToSave) {
                DropMessage(ErrorType.Info, $"Speichere Chunk '{thisChunk.KeyName}' der Tabelle '{Caption}'");

                f = thisChunk.DoExtendedSave();
                if (string.IsNullOrEmpty(f)) {
                    _ = _chunks.AddOrUpdate(thisChunk.KeyName, thisChunk, (key, oldValue) => thisChunk);
                } else {
                    allok = f;
                }
                _ = chunksBeingSaved.TryRemove(thisChunk.KeyName, out _);  // Hier bereits entfernen, dass andere Routinen einen Fortschritt sehen
            }
        } finally {
            // Sicherstellen, dass alle vorgemerkten Chunks aus chunksBeingSaved entfernt werden
            foreach (var thisChunk in chunksToSave) {
                _ = chunksBeingSaved.TryRemove(thisChunk.KeyName, out _);
            }
        }

        #endregion

        if (!string.IsNullOrEmpty(allok)) { return allok; }

        #region Nun gibt es noch Chunk-Leichen

        // Wenn aus einem Chunk alle Daten gelöscht wurden, den Chunk auch löschen
        var chunks = _chunks.Values.ToList();
        foreach (var thisChunk in chunks) {
            if (thisChunk.SaveRequired) {
                // Prüfen ob Chunk wirklich leer ist.
                // Kann passieren, wenn ein Chunk geändert wurde während des Speichervorgangnes.
                // Dann wird er nicht zum Speichern hzurückgegeben und fälschlicherwerise als Leer erkannt
                var rowsInChunk = RowsOfChunk(thisChunk);
                if (rowsInChunk.Count == 0) {
                    DropMessage(ErrorType.Info, $"Lösche leeren Chunk '{thisChunk.KeyName}' der Tabelle '{Caption}'");
                    _ = thisChunk.Delete();
                    _ = _chunks.TryRemove(thisChunk.KeyName, out _);
                }
            }
        }

        #endregion

        LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        return string.Empty;
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        newChunkId = newChunkId.ToLower();
        oldChunkId = oldChunkId.ToLower();

        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (IsFreezed) { return "Tabelle schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_chunks.TryGetValue(newChunkId, out var chkn)) { chkn.SaveRequired = true; }
        if (_chunks.TryGetValue(oldChunkId, out var chko)) { chko.SaveRequired = true; }

        return string.Empty;
    }

    private static Chunk GetOrMakechunk(List<Chunk> chunks, TableFile tbf, string chunkId) {
        var rowchunk = chunks.Get(chunkId);

        if (rowchunk == null) {
            rowchunk = new Chunk(tbf.Filename, chunkId);
            rowchunk.InitByteList();
            chunks.Add(rowchunk);
        }

        return rowchunk;
    }

    private bool Parse(Chunk chunk) {
        if (chunk.LoadFailed) { return false; }

        if (chunk.Bytes.Count == 0) { return true; }

        var rowsToRemove = RowsOfChunk(chunk);

        if (rowsToRemove.Count > 0) {
            // Zeilen und zugehörige Zellen entfernen
            foreach (var row in rowsToRemove) {
                _ = Row.ExecuteCommand(TableDataType.Command_RemoveRow, row.KeyName, Reason.NoUndo_NoInvalidate, null, null);
            }

            // Verwaiste Zellen entfernen
            Cell.RemoveOrphans();
        }

        if (chunk.IsMain) {
            _chunks.Clear();
            Undo.Clear();
            Row.RemoveNullOrEmpty();
            Cell.Clear();
        }

        // Zuerst parsen, bevor der Chunk in die Dictionary kommt
        var parseSuccessful = Parse(chunk.Bytes.ToArray(), chunk.IsMain);

        if (!parseSuccessful) {
            chunk.LoadFailed = true;
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            // Fehlerhaften Chunk nicht in die Dictionary einfügen
            return false;
        }
        return true;
    }

    #endregion
}