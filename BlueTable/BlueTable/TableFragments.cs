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
using BlueBasics.FileSystemCaching;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueTable;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFragments : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Sekundn. Nach dieser Zeit soll der FragmentAufräumer beendet werden
    /// </summary>
    public static readonly int AbortFragmentDeletion = 10;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit dürfen Fragmente gelöscht werden.
    /// </summary>
    public static readonly int DeleteFragmentsAfter = (DoComplete * 2) + (UpdateTable * 2);

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen
    /// </summary>
    public static readonly int DoComplete = 60;

    /// <summary>
    /// So viele Änderungen sind seit dem letzten erstellen der Komplett-Tabelle erstellen auf Festplatte gezählt worden
    /// </summary>
    private readonly List<UndoItem> _changesNotIncluded = [];

    /// <summary>
    /// Während der Daten aktualiszer werden dürfen z.B. keine Tabellenansichten gemacht werden.
    /// Weil da Zeilen sortiert / invalidiert / Sortiert / invalidiert etc. werden
    /// </summary>
    private int _doingChanges = 0;

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    private DateTime _isInCache = new(0);

    private bool _masterNeeded;
    private string _myFragmentsFilename = string.Empty;
    private System.IO.StreamWriter? _writer;

    #endregion

    #region Constructors

    public TableFragments(string tablename) : base(tablename) {
    }

    #endregion

    #region Destructors

    ~TableFragments() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public bool CanDeleteWriter { get; private set; } = true;
    public bool FirstTimAlleFragmentsLoaded { get; private set; } = false;

    /// <summary>
    /// Wenn die Prüfung ergibt, dass zu viele Fragmente da sind, wird hier auf true gesetzt
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    public override bool MultiUserPossible => true;

    protected override bool SaveRequired => false;

    #endregion

    #region Methods

    public override bool AmITemporaryMaster(int ranges, int rangee) {
        if (_isInCache.Year < 2000) { return false; }
        if (!FirstTimAlleFragmentsLoaded) { return false; }

        if (DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > UpdateTable) {
            if (!BeSureToBeUpToDate(false, true)) { return false; }
        }

        return base.AmITemporaryMaster(ranges, rangee);
    }

    public override bool BeSureToBeUpToDate(bool firstTime, bool instantUpdate) {
        if (!base.BeSureToBeUpToDate(firstTime, instantUpdate)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (!firstTime && !FirstTimAlleFragmentsLoaded) { return false; }

        if (firstTime) {
            _isInCache = LastSaveMainFileUtcDate;
        }

        FirstTimAlleFragmentsLoaded = false;
        DropMessage(ErrorType.Info, "Lade Fragmente von '" + KeyName + "'");
        var lastFragmentDate = DateTime.UtcNow;

        if (firstTime && !instantUpdate) {
            // Parallele Ausführung mit Callback
            Task.Run(() => {
                var (changes, files) = GetLastChanges(lastFragmentDate);
                InjectData(files, changes, DateTime.UtcNow, lastFragmentDate);
            });
            return false;
        }

        var (changes, files) = GetLastChanges(lastFragmentDate);
        InjectData(files, changes, DateTime.UtcNow, lastFragmentDate);
        return FirstTimAlleFragmentsLoaded;
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

    public override string IsNotEditableReason(bool isLoading) {
        var aadc = base.IsNotEditableReason(isLoading);
        if (!string.IsNullOrEmpty(aadc)) { return aadc; }

        if (string.IsNullOrEmpty(FragmengtsPath())) { return "Fragmentpfad nicht gesetzt."; }

        if (!FirstTimAlleFragmentsLoaded) { return "Tabelle wird noch geladen."; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    public override void TryToSetMeTemporaryMaster() {
        if (DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > 1) { return; }
        if (!FirstTimAlleFragmentsLoaded) { return; }
        base.TryToSetMeTemporaryMaster();
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            UnMasterMe();
            CloseWriter();

            base.Dispose(disposing);
        }
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

                if (!type.IsUnimportant()) { CanDeleteWriter = false; }
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

        CreateDirectory(FragmengtsPath());
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

            if (CanDeleteWriter) {
                IO.DeleteFile(_myFragmentsFilename, false);
            }
        }
    }

    private void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc) {
        if (Generic.Ending) { return; }
        if (!IsEditable(false)) { return; }
        if (files is not { Count: >= 1 }) { return; }
        if (!FirstTimAlleFragmentsLoaded) { return; }

        _masterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > DoComplete;

        #region Bei Bedarf neue Komplett-Tabelle erstellen

        if (_masterNeeded && AmITemporaryMaster(MasterTry, MasterUntil)) {
            DropMessage(ErrorType.Info, "Erstelle neue Komplett-Tabelle: " + KeyName);

            var t = LastSaveMainFileUtcDate;

            var f = SaveMainFile(this, _isInCache);

            if (f.Failed) {
                DropMessage(ErrorType.Info, $"Komplettierung von {Caption} fehlgeschlagen: {f.StringValue}");
                //Develop.DebugPrint(ErrorType.Info, $"Komplettierung von {Caption} fehlgeschlagen: {f}");
                return;
            }

            CloseWriter();
            StartWriter();

            ChangeData(TableDataType.LastSaveMainFileUtcDate, null, t.ToString7(), LastSaveMainFileUtcDate.ToString7());
            MasterMe();

            _masterNeeded = false;
            OnInvalidateView();
            _changesNotIncluded.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien, mit zu jungen Änderungen entfernen

        if (_changesNotIncluded.Any()) {
            foreach (var thisch in _changesNotIncluded) {
                //if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalMinutes < DeleteFragmentsAfter) {
                files.Remove(thisch.Container);
                //}
            }
        }

        #endregion

        files.Shuffle();

        foreach (var thisf in files) {
            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f.Substring(f.Length - 19);

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalMinutes > DeleteFragmentsAfter &&
                         LastSaveMainFileUtcDate.Subtract(d2).TotalMinutes > DeleteFragmentsAfter) {
                        DropMessage(ErrorType.Info, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix());
                        DeleteFile(thisf, false);
                        //MoveFile(thisf, pf + thisf.FileNameWithSuffix(), 1, false);
                        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
                    }
                }
            }
        }
    }

    private string FragmengtsPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Frgm\\";

    private (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(DateTime endTimeUtc) {
        if (IsEditable(true)) { return (null, null); }

        CheckPath();

        try {

            #region Alle Fragment-Dateien im Verzeichnis ermitteln und eigene Ausfiltern (frgma)

            var filesystem = CachedFileSystem.Get(FragmengtsPath());

            var frgma = filesystem.GetFiles(FragmengtsPath(), [KeyName.ToUpper() + "-*." + SuffixOfFragments()]); // GetFiles(FragmengtsPath(), KeyName.ToUpper() + "-*." + SuffixOfFragments(), System.IO.SearchOption.TopDirectoryOnly).ToList();
            frgma.Remove(_myFragmentsFilename);

            #endregion

            if (frgma.Count == 0) { return ([], []); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                var fil = filesystem.ReadAllText(thisf, Encoding.UTF8);
                var fils = fil.SplitAndCutByCr().ToList();

                foreach (var thist in fils) {
                    if (!thist.StartsWith("-")) {
                        var u = new UndoItem(thist);

                        if (u.DateTimeUtc.Subtract(_isInCache).TotalSeconds > 0 &&
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
        if (IsEditable(true)) { return; }

        if (Column.ChunkValueColumn is { IsDisposed: false }) { return; }

        var dataSorted = data.Where(obj => obj?.DateTimeUtc != null).OrderBy(obj => obj.DateTimeUtc);

        try {
            List<string> myfiles = [];

            if (checkedDataFiles != null) {
                foreach (var thisf in checkedDataFiles) {
                    if (thisf.Contains("\\" + KeyName.ToUpperInvariant() + "-")) {
                        myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            Interlocked.Increment(ref _doingChanges);
            try {
                foreach (var thisWork in dataSorted) {
                    if (KeyName == thisWork.TableName && thisWork.DateTimeUtc > _isInCache) {
                        Undo.Add(thisWork);
                        _changesNotIncluded.Add(thisWork);

                        var c = Column[thisWork.ColName];
                        var r = Row.GetByKey(thisWork.RowKey);

                        var error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.NoUndo_NoInvalidate);

                        if (!string.IsNullOrEmpty(error)) {
                            Freeze("Tabellen-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
                            return;
                        }
                    }
                }
                _isInCache = endTimeUtc;
                FirstTimAlleFragmentsLoaded = true;
            } finally {
                Interlocked.Decrement(ref _doingChanges);
                Column.GetSystems();
                DoWorkAfterLastChanges(myfiles, startTimeUtc);
                RepairAfterParse();
                TryToSetMeTemporaryMaster();
                OnInvalidateView();
                OnLoaded(false);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            InjectData(checkedDataFiles, data, startTimeUtc, endTimeUtc);
        }
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + _isInCache.ToString5());
            return;
        }

        if (!IsEditable(false)) { return; }
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

            var l = new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, " Dummy - systembedingt benötigt", "[Änderung in dieser Session]", string.Empty);
            _writer.WriteLine(l.ParseableItems().FinishParseable());
            CanDeleteWriter = true;
            _writer.Flush();
        } catch {
            fileStream?.Dispose(); // Nur disposen wenn StreamWriter failed
            _writer?.Dispose();
            _writer = null;

            Pause(3, false);
            Develop.AbortAppIfStackOverflow();
            StartWriter();
        }
    }

    #endregion
}