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
    /// Der Globale Timer, der die Tabellen regelmäßig updated
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

        return GetFiles(path, "*." + fx, System.IO.SearchOption.TopDirectoryOnly).ToList();
    }

    public override string AreAllDataCorrect() {
        if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        if (!CanWriteInDirectory(Filename.FilePath())) { return "Sie haben im Verzeichnis der Datei keine Schreibrechte."; }

        return base.AreAllDataCorrect();
    }

    public override bool BeSureAllDataLoaded(int anzahl) => base.BeSureAllDataLoaded(anzahl);

    public override bool BeSureRowIsLoaded(string chunkValue) => base.BeSureRowIsLoaded(chunkValue);

    public override bool BeSureToBeUpToDate() => base.BeSureToBeUpToDate();

    /// <summary>
    /// Konkrete Prüfung, ob jetzt gespeichert werden kann
    /// </summary>
    /// <returns></returns>
    public string CanSaveMainChunk() {
        var f = AreAllDataCorrect();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (Row.HasPendingWorker()) { return "Es müssen noch Daten überprüft werden."; }

        if (ExecutingScriptAnyTable.Count > 0) { return "Es wird noch ein Skript ausgeführt."; }

        if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 1) { return "Kürzlich vorgenommene Änderung muss verarbeitet werden."; }

        //if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4

        var fileSaveResult = CanSaveFile(Filename, 5);
        return fileSaveResult;
    }

    public override void Freeze(string reason) => base.Freeze(reason);

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) => base.GrantWriteAccess(type, chunkValue);

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
                var ok = Save();
                if (!ok) { return "Speicher-Fehler!"; }
                db.Dispose();
                var d = DeleteFile(thisFile, false);
                if (!d) { return "Lösch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(ErrorType.Error, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
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

        if (FileStateUtcDate.Year < 2000) {
            FileStateUtcDate = new DateTime(2000, 1, 1);
        }
        _saveRequired_File = false;
        IsInCache = FileStateUtcDate;

        _ = BeSureToBeUpToDate();

        RepairAfterParse();

        if (!CanWriteInDirectory(fileNameToLoad.FilePath())) { Freeze("Keine Schreibrechte im Verzeichniss."); }

        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded(true);

        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null, true, false);

        DropMessage(ErrorType.Info, $"Laden der Tabelle {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen");
    }

    public override void ReorganizeChunks() => base.ReorganizeChunks();

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(AreAllDataCorrect())) { return; }

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(ErrorType.Warning, "Tabellenname stimmt nicht: " + Filename);
            }
        }

        base.RepairAfterParse();
    }

    public bool Save() {
        if (!SaveRequired) { return true; }

        // Sofortiger Exit wenn bereits ein Save läuft (non-blocking check)
        if (!_saveSemaphore.Wait(0)) {
            return false;
        }

        try {
            var result = SaveInternal(FileStateUtcDate);
            OnInvalidateView();
            return string.IsNullOrEmpty(result);
        } finally {
            _saveSemaphore.Release();
        }
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(ErrorType.Error, "Dateiname unterscheiden sich nicht!"); }
        Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;
        var currentTime = DateTime.UtcNow;
        var chunks = TableChunk.GenerateNewChunks(this, 100, currentTime, false);

        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return; }

        _ = mainchunk.Save(newFileName);

        // IsInCache auf FileStateUtcDate setzen, damit WaitInitialDone() nicht wartet
        FileStateUtcDate = currentTime;
        IsInCache = currentTime;
    }

    public override string ToString() => base.ToString();

    protected static string SaveMainFile(TableFile tbf, DateTime setfileStateUtcDateTo) {
        var f = tbf.CanSaveMainChunk();
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();

        var chunksnew = TableChunk.GenerateNewChunks(tbf, 1200, setfileStateUtcDateTo, false);
        if (chunksnew == null || chunksnew.Count != 1) { return "Fehler bei der Chunk Erzeugung"; }

        f = chunksnew[0].DoExtendedSave();
        if (!string.IsNullOrEmpty(f)) { return f; }

        tbf.FileStateUtcDate = setfileStateUtcDateTo;
        return string.Empty;
    }

    protected override void Checker_Tick(object state) {
        base.Checker_Tick(state);

        if (IsDisposed || !string.IsNullOrEmpty(FreezedReason) || !SaveRequired || !LogUndo) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        // Zeitliche Bedingungen prüfen
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

        // Speichern wenn nötig
        if (mustSave) { Save(); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    protected override void Dispose(bool disposing) {
        // Keine Zusatzlogik – bewusst transparent.

        if (IsDisposed) { return; }

        if (disposing) {
            try {
                _saveSemaphore?.Dispose();
                // LÖSUNG: Static Timer verwalten basierend auf aktiven Table-Instanzen
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

    protected override void DoWorkAfterLastChanges(List<string>? files, DateTime starttimeUtc, DateTime endTimeUtc) =>
        base.DoWorkAfterLastChanges(files, starttimeUtc, endTimeUtc);

    protected virtual bool LoadMainData() {
        var (bytes, _, failed) = LoadAndUnzipAllBytes(Filename);

        if (failed) {
            Freeze("Laden fehlgeschlagen!");
            return false;
        }

        var ok = Parse(bytes, true, Filename);

        if (!ok) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected virtual string SaveInternal(DateTime setfileStateUtcDateTo) {
        try {
            var result = SaveMainFile(this, FileStateUtcDate);

            _saveRequired_File = !string.IsNullOrEmpty(result);

            OnInvalidateView();

            return result;
        } catch {
            return "Allgemeiner Fehler.";
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
            if (thisTbl is TableFile { IsDisposed: false, IsInCache.Year: < 2000 } tbf) {
                //if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (!string.IsNullOrEmpty(tbf.Filename)) { return; } // Irgend eine Tabelle wird aktuell geladen
            }
        }

        BeSureToBeUpToDate(AllFiles);
    }

    private void GenerateTableUpdateTimer() {
        lock (_timerLock) {
            _activeTableCount++;

            if (_tableUpdateTimer != null) { return; }

            _tableUpdateTimer = new System.Threading.Timer(TableUpdater, null, 10000, 3 * 60 * 1000);
        }
    }

    private bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is TableFile { IsDisposed: false } tbf) {
                if (string.Equals(tbf.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    tbf.Save();
                    Develop.DebugPrint(ErrorType.Warning, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    #endregion
}