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
using System.Text;
using System.Threading;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueTable;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFragments : TableFile {

    #region Fields

    private static volatile bool _isInFragmentLoader;

    /// <summary>
    /// So viele Änderungen sind seit dem letzten erstellen der Komplett-Tabelle erstellen auf Festplatte gezählt worden
    /// </summary>
    private readonly List<UndoItem> _changesNotIncluded = [];

    /// <summary>
    /// Während der Daten aktualiszer werden dürfen z.B. keine Tabellenansichten gemacht werden.
    /// Weil da Zeilen sortiert / invalidiert / Sortiert / invalidiert etc. werden
    /// </summary>
    private int _doingChanges = 0;

    private bool _masterNeeded;

    private string _myFragmentsFilename = string.Empty;

    private System.IO.StreamWriter? _writer;

    #endregion

    #region Constructors

    public TableFragments(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~TableFragments() { Dispose(false); }

    #endregion

    #region Properties

    /// <summary>
    /// Wenn die Prüfung ergibt, dass zu viele Fragmente da sind, wird hier auf true gesetzt
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => false;

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    private DateTime IsInCache { get; set; } = new(0);

    #endregion

    #region Methods

    public override bool AmITemporaryMaster(int ranges, int rangee) {
        if (IsInCache.Year < 2000) { return false; }

        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > UpdateTable) {
            if (!BeSureToBeUpToDate(false)) { return false; }
        }

        return base.AmITemporaryMaster(ranges, rangee);
    }

    public override string AreAllDataCorrect() {
        var aadc = base.AreAllDataCorrect();
        if (!string.IsNullOrEmpty(aadc)) { return aadc; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (_isInFragmentLoader) { return false; }
        _isInFragmentLoader = true;

        if (firstTime) {
            IsInCache = LastSaveMainFileUtcDate;
        }

        try {
            DropMessage(ErrorType.Info, "Lade Fragmente von '" + KeyName + "'");

            var lastFragmentDate = DateTime.UtcNow;
            var (changes, files) = GetLastChanges(lastFragmentDate);
            if (changes == null) {
                _isInFragmentLoader = false;
                return false;
            }

            var start = DateTime.UtcNow;
            Column.GetSystems();
            InjectData(files, changes, start, lastFragmentDate);
            TryToSetMeTemporaryMaster();
        } catch {
            _isInFragmentLoader = false;
            return false;
        }

        _isInFragmentLoader = false;
        return true;
    }

    public override void Freeze(string reason) {
        CloseWriter();
        base.Freeze(reason);
    }

    public override FileOperationResult GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (f.Failed) { return f; }

        if (_writer == null) { StartWriter(); }

        if (_writer == null) { return new("Schreib-Objekt nicht erstellt.", false, true); }

        return FileOperationResult.ValueStringEmpty;
    }

    public override void TryToSetMeTemporaryMaster() {
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 1) { return; }
        base.TryToSetMeTemporaryMaster();
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) { }
        UnMasterMe();
        CloseWriter();

        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        if (FileExists(Filename)) {
            if (!CreateDirectory(FragmengtsPath())) { return false; }
        }

        return base.LoadMainData();
    }

    protected override FileOperationResult SaveInternal(DateTime setfileStateUtcDateTo) {
        if (_writer == null) { return new("Writer Fehler", false, true); }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            return FileOperationResult.ValueStringEmpty;
        } catch { return new("Allgemeiner Fehler", false, true); }
    }

    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (IsFreezed) { return "Tabelle schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_writer == null) { StartWriter(); }
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(KeyName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]", newChunkId);

        try {
            lock (_writer) {
                _writer.WriteLine(l.ParseableItems().FinishParseable());
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }

        return string.Empty;
    }

    // immer "speichern"
    private static string SuffixOfFragments() => "frg";

    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }

        _ = CreateDirectory(FragmengtsPath());
        //CreateDirectory(OldFragmengtsPath());
    }

    private void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose != null) {
            _writer = null; // Sofort null setzen um double disposal zu verhindern

            try {
                writerToClose.WriteLine("- EOF");
                writerToClose.Flush();
            } catch { } finally {
                try {
                    writerToClose.Dispose(); // Dispose ruft Close() automatisch auf
                } catch { }
            }
        }
    }

    private void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc, DateTime endTimeUtc) {
        if (IsFreezed) { return; }
        if (!InitialLoadDone) { return; }
        if (files is not { Count: >= 1 }) { return; }

        _masterNeeded = files.Count > 8 || _changesNotIncluded.Count > 40 || DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalHours > 12;

        #region Bei Bedarf neue Komplett-Tabelle erstellen

        if (_masterNeeded && DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > 15 && AmITemporaryMaster(MasterTry, MasterUntil)) {
            DropMessage(ErrorType.Info, "Erstelle neue Komplett-Tabelle: " + KeyName);

            var f = SaveMainFile(this, IsInCache);

            if (f.Failed) {
                DropMessage(ErrorType.Info, "Komplettierung von {Caption} fehlgeschlagen: {f.StringValue}");
                //Develop.DebugPrint(ErrorType.Info, $"Komplettierung von {Caption} fehlgeschlagen: {f}");
                return;
            }
            _masterNeeded = false;
            OnInvalidateView();
            _changesNotIncluded.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > 20) { return; }

        #region Dateien, mit zu jungen Änderungen entfernen

        if (_changesNotIncluded.Any()) {
            foreach (var thisch in _changesNotIncluded) {
                //if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalHours < 12) {
                _ = files.Remove(thisch.Container);
                //}
            }
        }

        #endregion

        //var pf = OldFragmengtsPath();

        files.Shuffle();

        foreach (var thisf in files) {
            var del = true;

            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f.Substring(f.Length - 19);

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalHours < 3) { del = false; }
                    if (del && DateTime.UtcNow.Subtract(d2).TotalHours < 12) {
                        try {
                            var fi = GetFileInfo(thisf);
                            if (fi == null || fi.Length > 400) { del = false; }
                        } catch {
                            del = false;
                        }
                    }
                }
            }

            if (del) {
                DropMessage(ErrorType.Info, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix());
                _ = DeleteFile(thisf, false);
                //MoveFile(thisf, pf + thisf.FileNameWithSuffix(), 1, false);
                if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > 20) { break; }
            }
        }
    }

    private string FragmengtsPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Frgm\\";

    private (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(DateTime endTimeUtc) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return (null, null); }

        if (!string.IsNullOrEmpty(FreezedReason)) { return (null, null); }

        CheckPath();

        try {

            #region Alle Fragment-Dateien im Verzeichnis ermitteln und eigene Ausfiltern (frgma)

            var frgma = GetFiles(FragmengtsPath(), KeyName.ToUpper() + "-*." + SuffixOfFragments(), System.IO.SearchOption.TopDirectoryOnly).ToList();
            _ = frgma.Remove(_myFragmentsFilename);

            #endregion

            if (frgma.Count == 0) { return ([], []); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                var fil = ReadAllText(thisf, System.IO.FileShare.ReadWrite, Encoding.UTF8);

                var fils = fil.SplitAndCutByCrToList();

                foreach (var thist in fils) {
                    if (!thist.StartsWith("-")) {
                        var u = new UndoItem(thist);

                        if (u.DateTimeUtc.Subtract(IsInCache).TotalSeconds > 0 &&
                           u.DateTimeUtc.Subtract(endTimeUtc).TotalSeconds < 0) {
                            u.Container = thisf;
                            l.Add(u);
                        }
                    }
                }
            }

            return (l, frgma);
        } catch { }
        return (null, null);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="checkedDataFiles"></param>
    /// <param name="data"></param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    private void InjectData(List<string>? checkedDataFiles, List<UndoItem>? data, DateTime startTimeUtc, DateTime endTimeUtc) {
        if (data == null) { return; }
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        if (Column.ChunkValueColumn is { IsDisposed: false }) {
            // Split-Tabellen und Fragmente gehen nicht, siehe kommentar weiter unten
            return;
        }

        if (!string.IsNullOrEmpty(Filename) && !InitialLoadDone) {
            Develop.DebugPrint(ErrorType.Error, "Tabelle noch nicht korrekt geladen!");
            return;
        }

        var dataSorted = data.Where(obj => obj?.DateTimeUtc != null).OrderBy(obj => obj.DateTimeUtc);

        try {
            List<string> myfiles = [];

            if (checkedDataFiles != null) {
                foreach (var thisf in checkedDataFiles) {
                    if (thisf.Contains("\\" + KeyName.ToUpperInvariant() + "-")) {
                        _ = myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            Interlocked.Increment(ref _doingChanges);
            try {
                foreach (var thisWork in dataSorted) {
                    if (KeyName == thisWork.TableName && thisWork.DateTimeUtc > IsInCache) {
                        Undo.Add(thisWork);
                        _changesNotIncluded.Add(thisWork);

                        var c = Column[thisWork.ColName];
                        var r = Row.SearchByKey(thisWork.RowKey);

                        var error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.NoUndo_NoInvalidate);

                        if (!string.IsNullOrEmpty(error)) {
                            Freeze("Tabellen-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
                            return;
                        }
                    }
                }
            } finally {
                Interlocked.Decrement(ref _doingChanges);
            }

            IsInCache = endTimeUtc;
            DoWorkAfterLastChanges(myfiles, startTimeUtc, endTimeUtc);
            OnInvalidateView();
        } catch {
            Develop.CheckStackOverflow();
            InjectData(checkedDataFiles, data, startTimeUtc, endTimeUtc);
        }
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString5());
            return;
        }
        CheckPath();

        _myFragmentsFilename = TempFile(FragmengtsPath(), KeyName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments());

        if (Develop.AllReadOnly) { return; }

        System.IO.FileStream? fileStream = null;
        try {
            fileStream = new System.IO.FileStream(_myFragmentsFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            _writer = new System.IO.StreamWriter(fileStream, Encoding.UTF8);
            fileStream = null; // StreamWriter übernimmt ownership

            _writer.AutoFlush = true;
            _writer.WriteLine("- DB " + TableVersion);
            _writer.WriteLine("- Filename " + Filename);
            _writer.WriteLine("- User " + UserName);

            var l = new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]", string.Empty);
            _writer.WriteLine(l.ParseableItems().FinishParseable());
            _writer.Flush();
        } catch {
            fileStream?.Dispose(); // Nur disposen wenn StreamWriter failed
            _writer?.Dispose();
            _writer = null;

            Pause(3, false);
            Develop.CheckStackOverflow();
            StartWriter();
        }
    }

    #endregion
}