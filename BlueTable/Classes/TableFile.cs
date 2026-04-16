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

using BlueBasics.Classes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFile : Table {

    #region Fields

    public static readonly string Chunk_MainData = "MainData";
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

    #endregion

    #region Properties

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Gibt an, ob die Tabellendaten im Speicher verändert wurden und eine Neugenerierung der Chunks nötig ist.
    /// </summary>
    protected bool IsDirty { get; set; } = false;

    protected virtual bool SaveRequired => IsDirty;

    #endregion

    #region Methods

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

    public override string[]? AllAvailableTables(List<Table>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Tabelle

        var path = Filename.FilePath();
        var fx = Filename.FileSuffix();

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is TableFile { IsDisposed: false } tbf) {
                    if (string.Equals(tbf.Filename.FilePath(), path, StringComparison.Ordinal) &&
                        string.Equals(tbf.Filename.FileSuffix(), fx, StringComparison.Ordinal)) { return null; }
                }
            }
        }

        return CachedFileSystem.GetFileNames(path, ["*." + fx]);
    }

    public string ImportBdb(List<string> files, ColumnItem? colForFilename, bool deleteImportet) {
        foreach (var thisFile in files) {
            if (Get(thisFile, null) is not { } tb) {
                return thisFile + " konnte nicht geladen werden.";
            }

            tb.Freeze("Import im Gange.");

            foreach (var thisr in tb.Row) {
                var r = Row.GenerateAndAdd("Dummy", "BDB Import");

                if (r == null) { return "Zeile konnte nicht generiert werden."; }

                foreach (var thisc in tb.Column) {
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
                if (ok.IsFailed) { return $"Speicher-Fehler: {ok.FailedReason}"; }
                tb.Dispose();
                var d = IO.DeleteFile(thisFile, false);
                if (!d) { return "Lösch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public override string IsGenericEditable(bool isloading) {
        if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }

        // Das ist eins super schnelle Prüfung, also vorziehen.
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        var opr = IO.CanWriteInDirectory(Filename.FilePath());
        if (!string.IsNullOrEmpty(opr)) { return opr; }

        return string.Empty;
    }

    public override string IsValueEditable(TableDataType type, string? chunkValue) {
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (!string.IsNullOrEmpty(chunkValue)) { return string.Empty; }

        var chunk = CachedFileSystem.Get<Chunk>(Filename);
        if (chunk == null) {
            return "Interner Chunk-Fehler bei Editier-Prüfung.";
        }
        return chunk.IsNowEditable();
    }

    public void LoadFromFile(string fileNameToLoad, NeedPassword? needPassword, string freeze) {
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugError("Dateiname nicht angegeben!"); }

        fileNameToLoad = fileNameToLoad.NormalizeFile();

        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugError("Geladene Dateien können nicht als neue Dateien geladen werden."); }

        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

        if (!CachedFileSystem.FileExists(fileNameToLoad)) {
            Freeze("Datei existiert nicht");
            DropMessage(ErrorType.Warning, $"Tabelle nicht im Dateisystem vorhanden {fileNameToLoad.FileNameWithSuffix()}");
            return;
        }

        _needPassword = needPassword;
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        OnLoading();

        LoadMainData();

        IsDirty = false;
        MainChunkLoadDone = true;
        BeSureToBeUpToDate(true);

        RepairAfterParse();

        var opr = IO.CanWriteInDirectory(fileNameToLoad.FilePath());

        if (!string.IsNullOrEmpty(opr)) { Freeze(opr); }

        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded(true, true);

        CreateWatcher();

        DropMessage(ErrorType.Info, $"Laden der Tabelle {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen");
    }

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten TableFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(KeyName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint("Tabellenname stimmt nicht: " + Filename);
            }
        }

        base.RepairAfterParse();
    }

    public OperationResult Save(bool mustSave) {
        if (Develop.AllReadOnly) { return OperationResult.Success; }
        if (!SaveRequired) { return OperationResult.Success; }

        try {
            var result = SaveInternal(DateTime.UtcNow).GetAwaiter().GetResult();
            OnInvalidateView();

            if (!string.IsNullOrEmpty(result)) { return OperationResult.Failed(result); }
        } catch (Exception ex) {
            return OperationResult.Failed(ex);
        }

        return OperationResult.Success;
    }

    //public void SaveAsAndChangeTo(string newFileName) {
    //    if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) {
    //        Develop.DebugError( "Dateiname unterscheiden sich nicht!");
    //        return;
    //    }

    //    Save(true); // Original-Datei speichern

    //    Filename = newFileName;
    //    var currentTime = DateTime.UtcNow;
    //    var chunks = TableChunk.GenerateNewChunks(this, 100, currentTime, false);

    //    if (chunks?.Count != 1 || chunks[0] is not { } mainchunk) { return; }

    //    if (!mainchunk.SaveAs(newFileName)) { return; }

    //    MainChunkLoadDone = true;
    //}

    protected static async Task<string> SaveMainFileAsync(TableFile tbf, DateTime setfileStateUtcDateTo) {
        var f = tbf.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();

        var chunksnew = TableChunk.GenerateNewChunks(tbf, 1200, setfileStateUtcDateTo, false, true);
        if (chunksnew?.Count != 1) { return "Fehler bei der Chunk Erzeugung"; }

        var result = await chunksnew[0].Save().ConfigureAwait(false);
        if (result.IsFailed) { return result.FailedReason; }

        tbf.LastSaveMainFileUtcDate = setfileStateUtcDateTo;
        return string.Empty;
    }

    protected override void Checker_Tick(object state) {
        base.Checker_Tick(state);

        if (!SaveRequired || !LogUndo || !string.IsNullOrEmpty(IsGenericEditable(false))) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        // Zeitliche Bedingungen prüfen
        //var timeSinceLastChange = DateTime.UtcNow.Subtract(LastChange).TotalSeconds;
        var timeSinceLastAction = DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds;

        // Bestimme ob gespeichert werden muss
        var mustSave = _checkerTickCount > 20 && timeSinceLastAction > 20 ||
                         _checkerTickCount > 110 ||
                         Column.ChunkValueColumn != null && _checkerTickCount > 50;

        if (_checkerTickCount < 200) {
            // 200 * 2 Sekunden = 6,7 Minuten
            //if (e.Cancel && mustSave) { mustSave = false; }
            if (mustSave && RowCollection.InvalidatedRowsManager.PendingRowsCount > 0) { mustSave = false; }
        }

        // Speichern wenn nötig
        if (mustSave) { Save(false); }

        if (!SaveRequired) { _checkerTickCount = 0; }
    }

    protected override void Dispose(bool disposing) {
        // Keine Zusatzlogik - bewusst transparent.

        if (IsDisposed) { return; }

        if (disposing) {
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
            } catch {
            }
        }

        base.Dispose(disposing);
    }

    protected virtual bool LoadMainData() {
        var chunk = CachedFileSystem.Get<Chunk>(Chunk.ComputeChunkPath(Filename, Chunk_MainData));

        if (chunk == null || chunk.LoadFailed) {
            Freeze($"Laden fehlgeschlagen");
            return false;
        }

        var ok = Parse(chunk.Content, true, Reason.NoUndo_NoInvalidate, null);

        if (!ok) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected virtual async Task<string> SaveInternal(DateTime setfileStateUtcDateTo) {
        try {
            var result = await SaveMainFileAsync(this, setfileStateUtcDateTo).ConfigureAwait(false);

            IsDirty = !string.IsNullOrEmpty(result);

            OnInvalidateView();

            return result;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }
        IsDirty = true;
        return string.Empty;
    }

    private static void GenerateTableUpdateTimer() {
        lock (_timerLock) {
            _activeTableCount++;

            if (_tableUpdateTimer != null) { return; }

            _tableUpdateTimer = new Timer(TableUpdater, null, 10000, UpdateTable * 60 * 1000);
        }
    }

    private static void TableUpdater(object state) {
        foreach (var thisTb in AllFiles) {
            if (thisTb is TableFile { IsDisposed: false } tbf) {
                //if (!thisTb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (string.IsNullOrEmpty(tbf.Filename)) { return; } // Irgend eine Tabelle wird aktuell geladen
            }

            if (!thisTb.MainChunkLoadDone) { return; }
        }

        BeSureToBeUpToDate(AllFiles);
    }

    #endregion
}