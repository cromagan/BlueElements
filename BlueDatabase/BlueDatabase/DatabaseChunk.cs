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
using BlueDatabase.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseChunk : Database {

    #region Fields

    public static readonly string Chunk_AdditionalUseCases = "_uses";
    public static readonly string Chunk_MainData = "MainData";

    public static readonly string Chunk_Master = "_master";
    public static readonly string Chunk_Variables = "_vars";

    /// <summary>
    /// Flüchtiger Speicher, wird nur zum Halten von Daten verwendet.
    /// </summary>
    private readonly ConcurrentDictionary<string, Chunk> _chunks = new();

    private readonly ConcurrentDictionary<string, byte> chunksBeingSaved = new ConcurrentDictionary<string, byte>();

    #endregion

    #region Constructors

    public DatabaseChunk(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~DatabaseChunk() { Dispose(false); }

    #endregion

    #region Properties

    public override bool MultiUserPossible => true;

    #endregion

    #region Methods

    public static List<Chunk>? GenerateNewChunks(Database db, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed) {
        var chunks = new List<Chunk>();

        chunksAllowed = chunksAllowed && db.Column.ChunkValueColumn != null;

        var mainChunk = new Chunk(db.Filename, Chunk_MainData);
        mainChunk.InitByteList();
        chunks.Add(mainChunk);

        var usesChunk = mainChunk;
        var varChunk = mainChunk;
        var masterUserChunk = mainChunk;

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
        }

        try {
            var x = db.LastChange;

            mainChunk.SaveToByteList(DatabaseDataType.GlobalShowPass, db.GlobalShowPass);
            mainChunk.SaveToByteList(DatabaseDataType.Creator, db.Creator);
            mainChunk.SaveToByteList(DatabaseDataType.CreateDateUTC, db.CreateDate);
            mainChunk.SaveToByteList(DatabaseDataType.FileStateUTCDate, fileStateUtcDateToSave.ToString7());
            mainChunk.SaveToByteList(DatabaseDataType.Caption, db.Caption);

            masterUserChunk.SaveToByteList(DatabaseDataType.TemporaryDatabaseMasterUser, db.TemporaryDatabaseMasterUser);
            masterUserChunk.SaveToByteList(DatabaseDataType.TemporaryDatabaseMasterTimeUTC, db.TemporaryDatabaseMasterTimeUtc);

            mainChunk.SaveToByteList(DatabaseDataType.Tags, db.Tags.JoinWithCr());
            mainChunk.SaveToByteList(DatabaseDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            mainChunk.SaveToByteList(DatabaseDataType.DatabaseAdminGroups, db.DatenbankAdmin.JoinWithCr());

            mainChunk.SaveToByteList(DatabaseDataType.AdditionalFilesPath, db.AdditionalFilesPath);
            mainChunk.SaveToByteList(DatabaseDataType.RowQuickInfo, db.RowQuickInfo);
            mainChunk.SaveToByteList(DatabaseDataType.StandardFormulaFile, db.StandardFormulaFile);

            //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
            foreach (var columnitem in db.Column) {
                if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName) && !columnitem.IsDisposed) {
                    mainChunk.SaveToByteList(columnitem);
                    usesChunk.SaveToByteList(DatabaseDataType.ColumnSystemInfo, columnitem.ColumnSystemInfo, columnitem.KeyName);
                }
            }

            mainChunk.SaveToByteList(DatabaseDataType.SortDefinition, db.SortDefinition == null ? string.Empty : db.SortDefinition.ParseableItems().FinishParseable());

            mainChunk.SaveToByteList(DatabaseDataType.ColumnArrangement, db.ColumnArrangements);

            mainChunk.SaveToByteList(DatabaseDataType.EventScript, db.EventScript.ToString(true));
            //mainChunk.SaveToByteList(DatabaseDataType.EventScriptEdited, db.EventScriptEdited.ToString(true));
            mainChunk.SaveToByteList(DatabaseDataType.EventScriptVersion, db.EventScriptVersion.ToString5());

            //mainChunk.SaveToByteList(DatabaseDataType.NeedsScriptFix, db.NeedsScriptFix);
            varChunk.SaveToByteList(DatabaseDataType.DatabaseVariables, db.Variables.ToList().ToString(true));

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
                    if (undoCount < 1000 || (thisWorkItem.Command == DatabaseDataType.EventScript && important < 10)) {
                        undoCount++;
                        if (thisWorkItem.Command == DatabaseDataType.EventScript) { important++; }

                        if (chunksAllowed) {
                            var targetChunkId = GetChunkId(db, thisWorkItem.Command, thisWorkItem.ChunkValue);
                            if (!string.IsNullOrEmpty(targetChunkId)) {
                                var targetChunk = GetOrMakechunk(chunks, db, targetChunkId);
                                targetChunk.SaveToByteList(DatabaseDataType.Undo, thisWorkItem.ParseableItems().FinishParseable());
                            }
                        } else {
                            works2.Add(thisWorkItem.ParseableItems().FinishParseable());
                        }
                    }
                }
            }

            if (!chunksAllowed) {
                mainChunk.SaveToByteList(DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
            }

            #endregion

            long l = 0;
            foreach (var thisChunk in chunks) {
                thisChunk.SaveToByteListEOF();
                l += thisChunk.DataLenght;
            }

            if (l < minLen) { return null; }

            return x != db.LastChange ? null : chunks; // Stand stimmt nicht mehr
        } catch {
            Develop.CheckStackOverflow();
            return GenerateNewChunks(db, minLen, fileStateUtcDateToSave, chunksAllowed);
        }
    }

    public static string GetChunkId(RowItem r) => r.Database?.Column.ChunkValueColumn is not { IsDisposed: false }
            ? Chunk_MainData
            : GetChunkId(r.Database, DatabaseDataType.UTF8Value_withoutSizeData, r.ChunkValue);

    public static string GetChunkId(Database db, DatabaseDataType type, string chunkvalue) {
        if (db.Column.ChunkValueColumn is not { IsDisposed: false } spc) { return Chunk_MainData.ToLower(); }

        if (type is DatabaseDataType.Command_RemoveColumn
                or DatabaseDataType.Command_AddColumnByName) { return Chunk_MainData.ToLower(); }

        if (type == DatabaseDataType.Command_NewStart) { return string.Empty; }

        if (type.IsObsolete()) { return string.Empty; }
        if (type == DatabaseDataType.ColumnSystemInfo) { return Chunk_AdditionalUseCases.ToLower(); }
        if (type == DatabaseDataType.DatabaseVariables) { return Chunk_Variables.ToLower(); }
        if (type is DatabaseDataType.TemporaryDatabaseMasterUser or DatabaseDataType.TemporaryDatabaseMasterTimeUTC) { return Chunk_Master.ToLower(); }

        if (type.IsCellValue() || type is DatabaseDataType.Undo or DatabaseDataType.Command_AddRow or DatabaseDataType.Command_RemoveRow) {
            switch (spc.Value_for_Chunk) {
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
                    return "_rowdata".ToLower();
            }
        }

        return Chunk_MainData.ToLower();
    }

    public override string AreAllDataCorrect() {
        var f = base.AreAllDataCorrect();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (Column.ChunkValueColumn == null) { return string.Empty; }

        if (!_chunks.TryGetValue(Chunk_MainData.ToLower(), out var chkmain) || chkmain.LoadFailed) { return "Interner Chunk-Fehler"; }
        if (!_chunks.TryGetValue(Chunk_Master.ToLower(), out var chkmaster) || chkmaster.LoadFailed) { return "Interner Chunk-Fehler"; }
        if (!_chunks.TryGetValue(Chunk_Variables.ToLower(), out var chkvars) || chkvars.LoadFailed) { return "Interner Chunk-Fehler"; }
        if (!_chunks.TryGetValue(Chunk_AdditionalUseCases.ToLower(), out var chkuses) || chkuses.LoadFailed) { return "Interner Chunk-Fehler"; }

        return string.Empty;
    }

    public override bool BeSureAllDataLoaded(int anzahl) {
        if (!base.BeSureAllDataLoaded(anzahl)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (!DirectoryExists(chunkPath)) { return true; }

        var files = Directory.GetFiles(chunkPath, "*.bdbc");
        var fileQuery = anzahl < 0 || anzahl >= files.Length
            ? files
            : files.Select(f => new FileInfo(f))
                  .OrderBy(f => f.LastWriteTime)
                  .Take(anzahl)
                  .Select(f => f.FullName)
                  .ToArray();

        foreach (var file in fileQuery) {
            var chunkId = file.FileNameWithoutSuffix();

            var ok = LoadChunkWithChunkId(chunkId, true, false, false);
            if (!ok) { return false; }
        }

        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="chunkValue"></param>
    /// <param name="important">Steuert, ob es dringend nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns>Ob ein Load stattgefunden hat</returns>
    public override bool BeSureRowIsLoaded(string chunkValue) {
        var chunkId = GetChunkId(this, DatabaseDataType.UTF8Value_withoutSizeData, chunkValue);

        return LoadChunkWithChunkId(chunkId, false, false, false);
    }

    public override bool BeSureToBeUpToDate() {
        if (!base.BeSureToBeUpToDate()) { return false; }

        DropMessage(ErrorType.Info, "Lade Chunks von '" + TableName + "'");

        if (!LoadChunkWithChunkId(Chunk_MainData, true, true, false)) { return false; }

        Column.GetSystems();

        if (Column.ChunkValueColumn != null) {
            if (!LoadChunkWithChunkId(Chunk_AdditionalUseCases, true, true, false)) { return false; }
            if (!LoadChunkWithChunkId(Chunk_Master, true, true, false)) { return false; }
            if (!LoadChunkWithChunkId(Chunk_Variables, true, true, false)) { return false; }
        }
        IsInCache = DateTime.UtcNow;

        TryToSetMeTemporaryMaster();
        return true;
    }

    public override string GrantWriteAccess(DatabaseDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (chunkValue is not { }) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var ok = LoadChunkWithChunkId(chunkId, true, true, false);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            return "Interner Chunk-Fehler";
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
    public bool LoadChunkWithChunkId(string chunkId, bool important, bool mustExist, bool isFirst) {
        if (string.IsNullOrEmpty(Filename)) { return true; } // Temporäre Tabellen

        if (string.IsNullOrEmpty(chunkId)) { return false; }

        var t = Stopwatch.StartNew();
        var lastMessageTime = 0L;

        chunkId = chunkId.ToLower();

        do {
            Chunk? chkToWait = null;

            // Kurzer Lock nur für die kritischen Prüfungen - Race Condition vermeiden
            lock (chunksBeingSaved) {
                _chunks.TryGetValue(chunkId, out chkToWait);

                if (chkToWait == null && chunksBeingSaved.Count == 0) { break; }
            }

            // WaitInitialDone außerhalb des Locks um Deadlock zu vermeiden
            if (chkToWait != null) {
                chkToWait.WaitInitialDone();
                if (chkToWait.LoadFailed) { return false; }
                if (!chkToWait.NeedsReload(important)) { return true; }
                Develop.CheckStackOverflow();
                chkToWait.LoadBytesFromDisk();
            }

            if (t.ElapsedMilliseconds > 150 * 1000) {
                DropMessage(ErrorType.DevelopInfo, $"Abbruch, {chunksBeingSaved.Count} Chunks wurden noch nicht gespeichert.");
                return false;
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                DropMessage(ErrorType.Info, $"Warte auf Abschluss von {chunksBeingSaved.Count} Chunk Speicherungen.... Bitte Geduld, gleich gehts weiter.");
                Develop.DoEvents();
            }

            Thread.Sleep(1000);
        } while (true);

        DropMessage(ErrorType.Info, $"Lade Chunk '{chunkId}' der Tabelle '{Filename.FileNameWithoutSuffix()}'");

        var chunk = new Chunk(Filename, chunkId);
        chunk.LoadBytesFromDisk();

        if (chunk.LoadFailed) {
            Freeze($"Chunk {chunk.KeyName} Laden fehlgeschlagen");
            return false;
        }
        OnLoading();
        var ok = Parse(chunk);

        OnLoaded(isFirst);

        // Nur als leer markieren, wenn nicht gleichzeitig ein Speichervorgang läuft
        // Kurzer Lock um Race Condition zu vermeiden
        lock (chunksBeingSaved) {
            if (ok && mustExist && chunk.Bytes.Count == 0 && !chunksBeingSaved.ContainsKey(chunkId)) {
                chunk.SaveRequired = true;
            }
        }

        return ok;
    }

    public override void ReorganizeChunks() {
        base.ReorganizeChunks();
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        #region Erst alle Chunks laden

        if (!BeSureAllDataLoaded(-1)) {
            Develop.DebugPrint(ErrorType.Error, "Fehler beim Chunk laden!");
            return;
        }

        #endregion

        _chunks.Clear(); // Chunks im Speicher löschen, werden eh neue erzeugt

        #region alte Chunk-Dateien löschen

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (DirectoryExists(chunkPath)) {
            var chunkFiles = Directory.GetFiles(chunkPath, "*.bdbc");

            foreach (var file in chunkFiles) {
                _ = DeleteFile(file, false);
            }
        }

        #endregion

        _ = SaveInternal(FileStateUtcDate);
    }

    public override void RepairAfterParse() {
        base.RepairAfterParse();
        if (!string.IsNullOrEmpty(CanWriteMainFile())) { return; }

        FileStateUtcDate = new DateTime(2000, 1, 1);
    }

    public List<RowItem> RowsOfChunk(Chunk chunk) => Row.Where(r => GetChunkId(r) == chunk.KeyName).ToList();

    internal override string IsValueEditable(DatabaseDataType type, string? chunkValue) {
        var f = base.IsValueEditable(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (chunkValue is not { }) { return "Fehlerhafter Chunk-Wert"; }

        var chunkId = GetChunkId(this, type, chunkValue);

        var ok = LoadChunkWithChunkId(chunkId, true, true, false);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (Column.ChunkValueColumn == null) { return string.Empty; }

        if (!_chunks.TryGetValue(chunkId, out var chunk)) {
            return "Interner Chunk-Fehler";
        } else {
            return chunk.IsEditable();
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        _chunks.Clear();
    }

    protected override bool LoadMainData() => LoadChunkWithChunkId(Chunk_MainData, true, true, true);

    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        var f = CanWriteMainFile();
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
                // Prüfen ob Chunk wirklich leer ist. Sollte obsolet sein, weil nur befüllte Chunks zurück gegeben werden.
                var rowsInChunk = RowsOfChunk(thisChunk);
                if (rowsInChunk.Count == 0) {
                    DropMessage(ErrorType.Info, $"Lösche leeren Chunk '{thisChunk.KeyName}' der Tabelle '{Caption}'");
                    _ = thisChunk.Delete();
                    _ = _chunks.TryRemove(thisChunk.KeyName, out _);
                } else if (rowsInChunk.Count > 0) {
                    // Debug-Info für unerwartete Fälle
                    DropMessage(ErrorType.Warning, $"Chunk '{thisChunk.KeyName}' sollte leer sein, enthält aber {rowsInChunk.Count} Zeilen");
                }
            }
        }

        #endregion

        FileStateUtcDate = setfileStateUtcDateTo;
        return string.Empty;
    }

    protected override bool SaveRequired() => _chunks.Values.Any(chunk => chunk.SaveRequired);

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        newChunkId = newChunkId.ToLower();
        oldChunkId = oldChunkId.ToLower();

        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);

        if (!string.IsNullOrEmpty(f)) { return f; }

        if (ReadOnly) { return "Tabelle schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_chunks.TryGetValue(newChunkId, out var chkn)) { chkn.SaveRequired = true; }
        if (_chunks.TryGetValue(oldChunkId, out var chko)) { chko.SaveRequired = true; }

        return string.Empty;
    }

    private static Chunk GetOrMakechunk(List<Chunk> chunks, Database db, string chunkId) {
        var rowchunk = chunks.Get(chunkId);

        if (rowchunk == null) {
            rowchunk = new Chunk(db.Filename, chunkId);
            rowchunk.InitByteList();
            chunks.Add(rowchunk);
        }

        return rowchunk;
    }

    private bool Parse(Chunk chunk) {
        if (chunk.LoadFailed) { return false; }

        var rowsToRemove = RowsOfChunk(chunk);

        if (rowsToRemove.Count > 0) {
            // Zeilen und zugehörige Zellen entfernen
            foreach (var row in rowsToRemove) {
                _ = Row.ExecuteCommand(DatabaseDataType.Command_RemoveRow, row.KeyName, Reason.NoUndo_NoInvalidate, null, null);
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

        if (chunk.Bytes.Count == 0) {
            // Bei leerer Datei trotzdem in Dictionary einfügen
            _ = _chunks.AddOrUpdate(chunk.KeyName, chunk, (key, oldValue) => chunk);
            return true;
        }

        // Zuerst parsen, bevor der Chunk in die Dictionary kommt
        var parseSuccessful = Parse(chunk.Bytes.ToArray(), chunk.IsMain, chunk.ChunkFileName);

        if (!parseSuccessful) {
            chunk.LoadFailed = true;
            Freeze($"Chunk {chunk.KeyName} Parsen fehlgeschlagen");
            // Fehlerhaften Chunk nicht in die Dictionary einfügen
            return false;
        }

        // Nur erfolgreich geparste Chunks werden zur Dictionary hinzugefügt
        _ = _chunks.AddOrUpdate(chunk.KeyName, chunk, (key, oldValue) => chunk);

        return true;
    }

    #endregion
}