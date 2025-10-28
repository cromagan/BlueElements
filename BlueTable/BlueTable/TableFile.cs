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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using static BlueBasics.IO;

namespace BlueTable;

[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFile : Table {

    #region Fields

    private static readonly object _timerLock = new object();
    private static int _activeTableCount = 0;

    /// <summary>
    /// Der Globale Timer, der die Tabellen regelm��ig updated
    /// </summary>
    private static System.Threading.Timer? _tableUpdateTimer;

    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private int _checkerTickCount = -5;

    private bool _saveRequired_File = false;

    #endregion

    #region Constructors

    public TableFile(string tablename) : base(tablename) {
        GenerateTableUpdateTimer();
    }

    #endregion

    #region Properties

    public string Filename { get; protected set; } = string.Empty;

    public override bool MasterNeeded => base.MasterNeeded;

    public override bool MultiUserPossible => base.MultiUserPossible;
    protected virtual bool SaveRequired => _saveRequired_File;

    #endregion

    #region Methods

    public override List<string>? AllAvailableTables(List<Table>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Tabelle

        var path = Filename.FilePath();
        var fx = Filename.FileSuffix();

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is TableFile { IsDisposed: false } tbf) {
                    if (string.Equals(tbf.Filename.FilePath(), path) &&
                        string.Equals(tbf.Filename.FileSuffix(), fx)) { return null; }
                }
            }
        }

        return [.. GetFiles(path, "*." + fx, System.IO.SearchOption.TopDirectoryOnly)];
    }

    public override bool AmITemporaryMaster(int ranges, int rangee) => base.AmITemporaryMaster(ranges, rangee);

    public override string AreAllDataCorrect() {
        if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        if (!CanWriteInDirectory(Filename.FilePath())) { return "Sie haben im Verzeichnis der Datei keine Schreibrechte."; }

        return base.AreAllDataCorrect();
    }

    public override bool BeSureAllDataLoaded(int anzahl) => base.BeSureAllDataLoaded(anzahl);

    public override bool BeSureRowIsLoaded(string chunkValue) => base.BeSureRowIsLoaded(chunkValue);

    public override bool BeSureToBeUpToDate(bool firstTime) => base.BeSureToBeUpToDate(firstTime);

    /// <summary>
    /// Konkrete Pr�fung, ob jetzt gespeichert werden kann
    /// </summary>
    /// <returns></returns>
    public FileOperationResult CanSaveMainChunk() {
        var aadc = AreAllDataCorrect();
        if (!string.IsNullOrEmpty(aadc)) { return new(aadc, false, true); }

        if (!InitialLoadDone) { return new("Tabelle noch nicht geladen.", false, true); }

        if (Row.HasPendingWorker()) { return new("Es m�ssen noch Daten �berpr�ft werden.", true, true); }

        //if (ExecutingScriptThreadsAnyTable.Count > 0) { return "Es wird noch ein Skript ausgef�hrt."; }

        if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 1) { return new("K�rzlich vorgenommene �nderung muss verarbeitet werden.", true, true); }

        //if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massen�nderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden f�r ein zca4

        return CanSaveFile(Filename, 5);
    }

    public override void Freeze(string reason) => base.Freeze(reason);

    public override FileOperationResult GrantWriteAccess(TableDataType type, string? chunkValue) => base.GrantWriteAccess(type, chunkValue);

    public string ImportBdb(List<string> files, ColumnItem? colForFilename, bool deleteImportet) {
        foreach (var thisFile in files) {
            var db = Get(thisFile, null);
            if (db == null) {
                return thisFile + " konnte nicht geladen werden.";
            }

            db.Freeze("Import im Gange.");

            foreach (var thisr in db.Row) {
                var r = Row.GenerateAndAdd("Dummy", "BDB Import");

                if (r == null) { return "Zeile konnte nicht generiert werden."; }

                foreach (var thisc in db.Column) {
                    if (thisc != colForFilename) {
                        var c = Column[thisc.KeyName];
                        if (c == null) {
                            c = Column.GenerateAndAdd(thisc.KeyName, thisc.Caption, null, string.Empty);
                            if (c == null) { return "Spalte konnte nicht generiert werden."; }
                            c.CloneFrom(thisc, false);
                        }

                        var w = thisr.CellGetString(thisc);
                        r.CellSet(c, w, "Import von " + thisFile);
                        if (r.CellGetString(c) != w) { return "Setzungsfehler!"; }
                    }
                }

                if (colForFilename != null) {
                    r.CellSet(colForFilename, thisFile, "Dateiname, Import von " + thisFile);

                    if (r.CellGetString(colForFilename) != thisFile) { return "Setzungsfehler!"; }
                }
            }

            if (deleteImportet) {
                var ok = Save(true);
                if (!ok) { return "Speicher-Fehler!"; }
                db.Dispose();
                var d = DeleteFile(thisFile, false);
                if (!d) { return "L�sch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(ErrorType.Error, "Geladene Dateien k�nnen nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(ErrorType.Error, "Dateiname nicht angegeben!"); }

        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Freeze("Datei existiert nicht");
                DropMessage(ErrorType.Warning, $"Tabelle nicht im Dateisystem vorhanden {fileNameToLoad.FileNameWithSuffix()}");
                return;
            }
        }

        _needPassword = needPassword;
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        OnLoading();

        LoadMainData();

        _saveRequired_File = false;
        InitialLoadDone = true;
        _ = BeSureToBeUpToDate(true);

        RepairAfterParse();

        if (!CanWriteInDirectory(fileNameToLoad.FilePath())) { Freeze("Keine Schreibrechte im Verzeichniss."); }

        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded(true);

        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        CreateWatcher();

        DropMessage(ErrorType.Info, $"Laden der Tabelle {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen");
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null, true, false);
    }

    public override void ReorganizeChunks() => base.ReorganizeChunks();

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(ErrorType.Warning, "Tabellenname stimmt nicht: " + Filename);
            }
        }

        base.RepairAfterParse();
    }

    public bool Save(bool mustSave) => ProcessFile(TrySave, [Filename], false, mustSave ? 120 : 10) is true;

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(ErrorType.Error, "Dateiname unterscheiden sich nicht!"); }
        Save(true); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;
        var currentTime = DateTime.UtcNow;
        var chunks = TableChunk.GenerateNewChunks(this, 100, currentTime, false);

        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return; }

        _ = mainchunk.Save(newFileName);

        InitialLoadDone = true;
    }

    public override string ToString() => base.ToString();

    public override void TryToSetMeTemporaryMaster() => base.TryToSetMeTemporaryMaster();

    protected static FileOperationResult SaveMainFile(TableFile tbf, DateTime setfileStateUtcDateTo) {
        var f = tbf.CanSaveMainChunk();
        if (f.Failed) { return f; }

        Develop.SetUserDidSomething();

        var chunksnew = TableChunk.GenerateNewChunks(tbf, 1200, setfileStateUtcDateTo, false);
        if (chunksnew == null || chunksnew.Count != 1) { return new("Fehler bei der Chunk Erzeugung", false, true); }

        f = chunksnew[0].DoExtendedSave();
        if (f.Failed) { return f; }

        tbf.LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        return FileOperationResult.ValueStringEmpty;
    }

    protected override void Checker_Tick(object state) {
        base.Checker_Tick(state);

        if (IsDisposed || !string.IsNullOrEmpty(FreezedReason) || !SaveRequired || !LogUndo) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        // Zeitliche Bedingungen pr�fen
        //var timeSinceLastChange = DateTime.UtcNow.Subtract(LastChange).TotalSeconds;
        var timeSinceLastAction = DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds;

        // Bestimme ob gespeichert werden muss
        var mustSave = (_checkerTickCount > 20 && timeSinceLastAction > 20) ||
                         _checkerTickCount > 110 ||
                         (Column.ChunkValueColumn != null && _checkerTickCount > 50);

        if (_checkerTickCount < 200) {
            // 200 * 2 Sekunden = 6,7 Minuten
            //if (e.Cancel && mustSave) { mustSave = false; }
            if (mustSave && RowCollection.InvalidatedRowsManager.PendingRowsCount > 0) { mustSave = false; }
        }

        // Speichern wenn n�tig
        if (mustSave) { Save(false); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    protected override void Dispose(bool disposing) {
        // Keine Zusatzlogik � bewusst transparent.

        if (IsDisposed) { return; }

        if (disposing) {
            try {
                _saveSemaphore?.Dispose();
                // L�SUNG: Static Timer verwalten basierend auf aktiven Table-Instanzen
                lock (_timerLock) {
                    _activeTableCount--;
                    if (_activeTableCount <= 0) {
                        _activeTableCount = 0;
                        if (_tableUpdateTimer != null) {
                            _tableUpdateTimer.Dispose();
                            _tableUpdateTimer = null;
                        }
                    }
                }
            } catch {
            }
        }

        base.Dispose(disposing);
    }

    protected virtual bool LoadMainData() {
        var byteData = LoadAndUnzipAllBytes(Filename);

        if (byteData is null) {
            Freeze("Laden fehlgeschlagen!");
            return false;
        }

        var ok = Parse(byteData.Bytes, true);

        if (!ok) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected virtual FileOperationResult SaveInternal(DateTime setfileStateUtcDateTo) {
        try {
            var result = SaveMainFile(this, DateTime.UtcNow);

            _saveRequired_File = result.Failed;

            OnInvalidateView();

            return result;
        } catch {
            return new("Allgemeiner Fehler.", false, true);
        }
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        _saveRequired_File = true;

        return string.Empty;
    }

    private static void TableUpdater(object state) {
        foreach (var thisTbl in AllFiles) {
            if (thisTbl is TableFile { IsDisposed: false } tbf) {
                //if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (!string.IsNullOrEmpty(tbf.Filename)) { return; } // Irgend eine Tabelle wird aktuell geladen
            }

            if (!thisTbl.InitialLoadDone) { return; }
        }

        BeSureToBeUpToDate(AllFiles);
    }

    private void GenerateTableUpdateTimer() {
        lock (_timerLock) {
            _activeTableCount++;

            if (_tableUpdateTimer != null) { return; }

            _tableUpdateTimer = new System.Threading.Timer(TableUpdater, null, 10000, UpdateTable * 60 * 1000);
        }
    }

    private bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is TableFile { IsDisposed: false } tbf) {
                if (string.Equals(tbf.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    tbf.Save(false);
                    Develop.DebugPrint(ErrorType.Warning, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    private FileOperationResult TrySave(List<string> affectingFiles, params object?[] args) {
        if (affectingFiles.Count != 1 || affectingFiles[0] is not string filename) { return FileOperationResult.ValueFailed; }

        if (!string.Equals(filename, Filename, StringComparison.OrdinalIgnoreCase)) { return FileOperationResult.ValueFailed; }

        if (Develop.AllReadOnly) { return FileOperationResult.ValueTrue; }

        if (!SaveRequired) { return FileOperationResult.ValueTrue; }

        // Sofortiger Exit wenn bereits ein Save l�uft (non-blocking check)
        if (!_saveSemaphore.Wait(0)) { return FileOperationResult.DoRetry; }

        try {
            var result = SaveInternal(DateTime.UtcNow);
            OnInvalidateView();

            return result;
        } finally {
            _saveSemaphore.Release();
        }
    }

    #endregion
}