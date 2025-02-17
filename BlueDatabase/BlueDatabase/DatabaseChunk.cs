﻿// Authors:
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
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

    #endregion

    #region Constructors

    public DatabaseChunk(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~DatabaseChunk() { Dispose(false); }

    #endregion

    #region Methods

    public static List<Chunk>? GenerateNewChunks(Database db, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed) {
        var chunks = new List<Chunk>();

        chunksAllowed = chunksAllowed && db.Column.SplitColumn != null;

        var mainChunk = new Chunk(db.Filename, Chunk_MainData);
        mainChunk.InitByteList();
        chunks.Add(mainChunk);

        var usesChunk = mainChunk;
        var varChunk = mainChunk;
        var masterUserChunk = mainChunk; // Masterchunk wird nicht gespeichert. Weil es pro chunk einen Masteruser geben kann.

        if (chunksAllowed) {
            usesChunk = new Chunk(db.Filename, Chunk_AdditionalUseCases);
            usesChunk.InitByteList();
            chunks.Add(usesChunk);

            varChunk = new Chunk(db.Filename, Chunk_Variables);
            varChunk.InitByteList();
            chunks.Add(varChunk);

            masterUserChunk = new Chunk(db.Filename, Chunk_Master);
            masterUserChunk.InitByteList();
            //chunks.Add(masterUserChunk); // Masterchunk wird nicht gespeichert. Weil es pro chunk einen Masteruser geben kann.
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

            mainChunk.SaveToByteList(DatabaseDataType.AdditionalFilesPath, db.AdditionalFilesPfad);
            mainChunk.SaveToByteList(DatabaseDataType.RowQuickInfo, db.ZeilenQuickInfo);
            mainChunk.SaveToByteList(DatabaseDataType.StandardFormulaFile, db.StandardFormulaFile);

            //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
            foreach (var columnitem in db.Column) {
                if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName) && !columnitem.IsDisposed) {
                    mainChunk.SaveToByteList(columnitem);
                    usesChunk.SaveToByteList(DatabaseDataType.ColumnSystemInfo, columnitem.SystemInfo, columnitem.KeyName);
                }
            }

            mainChunk.SaveToByteList(DatabaseDataType.SortDefinition, db.SortDefinition == null ? string.Empty : db.SortDefinition.ParseableItems().FinishParseable());

            mainChunk.SaveToByteList(DatabaseDataType.ColumnArrangement, db.ColumnArrangements);

            mainChunk.SaveToByteList(DatabaseDataType.EventScript, db.EventScript.ToString(true));
            mainChunk.SaveToByteList(DatabaseDataType.EventScriptEdited, db.EventScriptEdited.ToString(true));
            mainChunk.SaveToByteList(DatabaseDataType.EventScriptVersion, db.EventScriptVersion.ToString5());

            usesChunk.SaveToByteList(DatabaseDataType.ScriptNeedFix, db.ScriptNeedFix);
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

            foreach (var thisWorkItem in db.Undo) {
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

            if (x != db.LastChange) { return null; } // Stand stimmt nicht mehr

            return chunks;
        } catch {
            Develop.CheckStackForOverflow();
            return GenerateNewChunks(db, minLen, fileStateUtcDateToSave, chunksAllowed);
        }
    }

    public static string GetChunkId(RowItem r) {
        if (r.Database?.Column.SplitColumn is not { }) { return Chunk_MainData; }

        return GetChunkId(r.Database, DatabaseDataType.UTF8Value_withoutSizeData, GetChunkValue(r));
    }

    public static string GetChunkId(Database db, DatabaseDataType type, string chunkvalue) {
        if (db.Column.SplitColumn is not { } spc) { return Chunk_MainData; }

        if (type is DatabaseDataType.Command_RemoveColumn
                or DatabaseDataType.Command_AddColumnByName) { return Chunk_MainData; }

        if (type == DatabaseDataType.Command_NewStart) { return string.Empty; }

        if (type.IsObsolete()) { return string.Empty; }
        if (type == DatabaseDataType.ColumnSystemInfo) { return Chunk_AdditionalUseCases; }
        if (type == DatabaseDataType.DatabaseVariables) { return Chunk_Variables; }
        if (type is DatabaseDataType.TemporaryDatabaseMasterUser
                 or DatabaseDataType.TemporaryDatabaseMasterTimeUTC) {
            //Develop.DebugPrint(FehlerArt.Fehler, "Sollte nicht passieren!");
            return Chunk_Master; // Alte Undos
        }

        if (type.IsCellValue() || type is DatabaseDataType.Undo or DatabaseDataType.Command_AddRow or DatabaseDataType.Command_RemoveRow) {
            switch (spc.Function) {
                case ColumnFunction.Split_Medium:
                    return chunkvalue.ToLower().GetHashString().Right(2).ToLower();

                case ColumnFunction.Split_Large:
                    return chunkvalue.ToLower().GetHashString().Right(3).ToLower();

                case ColumnFunction.Split_Name:
                    var t = ColumnItem.MakeValidColumnName(chunkvalue);
                    return string.IsNullOrEmpty(t) ? "_" : t.ToLower().Left(12);

                default:
                    return "_rowdata";
            }
        }

        return Chunk_MainData;
    }

    public static string GetChunkValue(RowItem r) {
        if (r.Database?.Column.SplitColumn is not { } spc) { return string.Empty; }
        return r.Database.Cell.GetStringCore(spc, r);

        //return r.CellGetString(spc);
    }

    /// <summary>
    /// row == null --> false
    /// </summary>
    /// <param name="ranges"></param>
    /// <param name="rangee"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public override bool AmITemporaryMaster(int ranges, int rangee, RowItem? row) {
        if (row == null) { return false; }
        if (!base.AmITemporaryMaster(ranges, rangee, row)) { return false; }

        //var chk = GetChunkId(row);

        //if (Column.SplitColumn is { } spc) {
        //    var value = FilterCollection.InitValue(filter, spc, false);

        //    if (string.IsNullOrEmpty(value)) {
        //        return "Bei Split-Datenbanken muss ein Filter in der Split-Spalte sein.";
        //    }
        //    return IsValueEditable(DatabaseDataType.UTF8Value_withoutSizeData, first.KeyName, value, EditableErrorReasonType.EditCurrently);

        if (Column?.SplitColumn is { }) {
            var value = DatabaseChunk.GetChunkValue(row);
            return string.IsNullOrEmpty(IsValueEditable(DatabaseDataType.UTF8Value_withoutSizeData, string.Empty, value, EditableErrorReasonType.EditCurrently));
        }

        return false;
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

            var (_, ok) = LoadChunkWithChunkId(chunkId, true, null, false);
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
    public override (bool loaded, bool ok) BeSureRowIsLoaded(string chunkValue, DatabaseDataType type, bool important, NeedPassword? needPassword) {
        var chunkId = GetChunkId(this, type, chunkValue);

        if (string.IsNullOrEmpty(chunkId)) { return (false, false); }

        return LoadChunkWithChunkId(chunkId, important, needPassword, false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="chunkId"></param>
    /// <param name="important">Steuert, ob es dringen nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns>Ob ein Load stattgefunden hat</returns>
    public (bool loaded, bool ok) LoadChunkWithChunkId(string chunkId, bool important, NeedPassword? needPassword, bool mustExist) {
        //if (Column.SplitColumn == null) { return (false, true); }

        if (string.IsNullOrEmpty(Filename)) { return (true, true); } // Temporäre Datenbanken

        if (_chunks.TryGetValue(chunkId.ToLower(), out var chk)) {
            if (chk.LoadFailed) { return (false, false); }
            if (!chk.NeedsReload(important)) { return (false, true); }
        }

        if (string.IsNullOrEmpty(chunkId)) {
            return (false, false);
        }

        OnDropMessage(FehlerArt.Info, $"Lade Chunk '{chunkId.ToLower()}' der Datenbank '{Filename.FileNameWithoutSuffix()}'");

        var chunk = new Chunk(Filename, chunkId);
        chunk.LoadBytesFromDisk();

        if (chunk.LoadFailed) { return (false, false); }
        OnLoading();
        var ok = Parse(chunk, needPassword);

        OnLoaded();

        if (ok && mustExist && chunk.Bytes.Length == 0) {
            chunk.SaveRequired = true;
        }

        return (!chunk.LoadFailed, ok);
    }

    public override void ReorganizeChunks() {
        base.ReorganizeChunks();
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        #region Erst alle Chunks laden

        if (!BeSureAllDataLoaded(-1)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fehler beim Chunk laden!");
            return;
        }

        #endregion

        _chunks.Clear(); // Chunks im Speicher löschen, werden eh neue erzeugt

        #region alte Chunk-Dateien löschen

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (DirectoryExists(chunkPath)) {
            var chunkFiles = Directory.GetFiles(chunkPath, "*.bdbc");

            foreach (var file in chunkFiles) {
                DeleteFile(file, false);
            }
        }

        #endregion

        SaveInternal(FileStateUtcDate);
    }

    public List<RowItem> RowsOfChunk(Chunk chunk) => Row.Where(r => GetChunkId(r) == chunk.KeyName.ToLower()).ToList();

    internal override string IsValueEditable(DatabaseDataType type, string columnName, string ofValue, EditableErrorReasonType reason) {
        var f = base.IsValueEditable(type, columnName, ofValue, reason);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (type is DatabaseDataType.TemporaryDatabaseMasterTimeUTC or
                    DatabaseDataType.TemporaryDatabaseMasterUser) { return "MasterUser in diesen Datenbanktyp nicht möglich"; }

        var chunkId = GetChunkId(this, type, ofValue);

        var (_, ok) = LoadChunkWithChunkId(chunkId, true, null, true);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (!_chunks.TryGetValue(chunkId.ToLower(), out var chunk)) {
            return "Interner Chunk-Fehler";
        }

        return chunk.IsEditable(reason);
    }

    protected override bool BeSureToBeUpDoDate() {
        if (!base.BeSureToBeUpDoDate()) { return false; }

        OnDropMessage(FehlerArt.Info, "Lade Chunks von '" + TableName + "'");

        if (!LoadChunkWithChunkId(Chunk_MainData, false, null, true).ok) { return false; }

        Column.GetSystems();

        if (Column.SplitColumn != null) {
            if (!LoadChunkWithChunkId(Chunk_AdditionalUseCases, false, null, true).ok) { return false; }
            //if (!LoadChunkWithChunkId(Chunk_Master, false, null, true).ok) { return false; }
            if (!LoadChunkWithChunkId(Chunk_Variables, false, null, true).ok) { return false; }
        }
        IsInCache = DateTime.UtcNow;

        TryToSetMeTemporaryMaster();
        return true;
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        _chunks.Clear();
    }

    protected override bool SaveInternal(DateTime setfileStateUtcDateTo) {
        if (Develop.AllReadOnly) { return true; }

        var m = EditableErrorReason(EditableErrorReasonType.Save);

        if (!string.IsNullOrEmpty(m)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return false; }

        Develop.SetUserDidSomething();

        #region Neue Chunks-Erstellen

        OnDropMessage(FehlerArt.Info, $"Erstelle Chunks der Datenank '{Caption}'");

        var chunksnew = GenerateNewChunks(this, 1200, setfileStateUtcDateTo, true);
        if (chunksnew == null || chunksnew.Count == 0) { return false; }
        foreach (var thisChunk in chunksnew) {
            _chunks.TryGetValue(thisChunk.KeyName, out var existingChunk);
            if (existingChunk == null || existingChunk.SaveRequired) {
                OnDropMessage(FehlerArt.Info, $"Speichere Chunk '{thisChunk.KeyName}' der Datenbank '{Caption}'");
                if (!thisChunk.DoExtendedSave(5)) {
                    return false;
                }

                _ = _chunks.TryRemove(thisChunk.KeyName, out _); // Den alten Fehlerhaften Chunk entfernen
                _chunks.TryAdd(thisChunk.KeyName, thisChunk); // den neuen korrigierten dafür hinzufügen
            }
        }

        #endregion

        #region Nun gibt es noch Chunk-Leichen

        // Wenn aus einem Chunk alle Daten gelöscht wurden, den Chunk auch löschen
        var chunks = new List<Chunk>();
        chunks.AddRange(_chunks.Values);
        foreach (var thisChunk in chunks) {
            if (thisChunk.SaveRequired) {
                OnDropMessage(FehlerArt.Info, $"Lösche alten Chunk '{thisChunk.KeyName}' der Datenbank '{Caption}'");
                thisChunk.Delete();
                _ = _chunks.TryRemove(thisChunk.KeyName, out _); // Den alten Fehlerhaften Chunk entfernen
                //thisChunk.InitByteList();
                //if (!thisChunk.DoExtendedSave(5)) { return false; }
            }
        }

        #endregion

        FileStateUtcDate = setfileStateUtcDateTo;
        return true;
    }

    protected override bool SaveRequired() {
        return _chunks.Values.Any(chunk => chunk.SaveRequired);
    }

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment, string chunkId) {
        var f = base.WriteValueToDiscOrServer(type, comment, column, row, user, datetimeutc, comment, chunkId);

        if (!string.IsNullOrEmpty(f)) { return f; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_chunks.TryGetValue(chunkId.ToLower(), out var chk)) {
            chk.SaveRequired = true;
        }

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

    private bool Parse(Chunk chunk, NeedPassword? needPassword) {
        if (chunk.LoadFailed) { return false; }

        var rowsToRemove = RowsOfChunk(chunk);

        if (rowsToRemove.Count > 0) {
            // Zeilen und zugehörige Zellen entfernen
            foreach (var row in rowsToRemove) {
                Row.ExecuteCommand(DatabaseDataType.Command_RemoveRow, row.KeyName, Reason.NoUndo_NoInvalidate, null, null);
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

        _chunks[chunk.KeyName] = chunk;

        //if (!_chunks.TryUpdate(chunk.KeyName, chunk)) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Chunk nicht eingespielt!");
        //    return false;
        //}

        if (chunk.Bytes.Length == 0) {
            // Datei gelöscht
            return true;
        }

        Parse(chunk.Bytes, chunk.IsMain, needPassword, chunk.ChunkFileName);

        return true;
    }

    #endregion
}