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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseFragments : Database {

    #region Fields

    private static volatile bool _isInFragmentLoader;

    /// <summary>
    /// Während der Daten aktualiszer werden dürfen z.B. keine Tabellenansichten gemacht werden.
    /// Weil da Zeilen sortiert / invalidiert / Sortiert / invalidiert etc. werden
    /// </summary>
    private int _doingChanges = 0;

    private bool _masterNeeded;

    private string _myFragmentsFilename = string.Empty;

    private StreamWriter? _writer;

    #endregion

    #region Constructors

    public DatabaseFragments(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~DatabaseFragments() { Dispose(false); }

    #endregion

    #region Properties

    /// <summary>
    /// Wenn die Prüfung ergibt, dass zu viele Fragmente da sind, wird hier auf true gesetzt
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    public override bool MultiUserPossible => true;

    #endregion

    #region Methods

    public override string AreAllDataCorrect() {
        var f = base.AreAllDataCorrect();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    public override bool BeSureToBeUpToDate() {
        if (!base.BeSureToBeUpToDate()) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (_isInFragmentLoader) { return false; }
        _isInFragmentLoader = true;

        try {
            DropMessage(ErrorType.Info, "Lade Fragmente von '" + TableName + "'");

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

    public override string GrantWriteAccess(DatabaseDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (_writer == null) { StartWriter(); }

        if (_writer == null) { return "Schreib-Objekt nicht erstellt."; }

        return string.Empty;
    }

    public override bool Save() {
        if (_writer == null) { return true; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            return true;
        } catch { return false; }
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) { }

        CloseWriter();

        base.Dispose(disposing);
    }

    protected override void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc, DateTime endTimeUtc) {
        base.DoWorkAfterLastChanges(files, startTimeUtc, endTimeUtc);
        if (ReadOnly) { return; }
        if (files is not { Count: >= 1 }) { return; }

        _masterNeeded = files.Count > 8 || ChangesNotIncluded.Count > 40 || DateTime.UtcNow.Subtract(FileStateUtcDate).TotalHours > 12;

        #region Bei Bedarf neue Komplett-Datenbank erstellen

        if (_masterNeeded && DateTime.UtcNow.Subtract(FileStateUtcDate).TotalMinutes > 15 && AmITemporaryMaster(5, 55)) {
            DropMessage(ErrorType.Info, "Erstelle neue Komplett-Datenbank: " + TableName);
            if (!SaveInternal(IsInCache)) { return; }
            _masterNeeded = false;
            OnInvalidateView();
            ChangesNotIncluded.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > 20) { return; }

        #region Dateien, mit zu jungen Änderungen entfernen

        if (ChangesNotIncluded.Any()) {
            foreach (var thisch in ChangesNotIncluded) {
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
                    if (DateTime.UtcNow.Subtract(d2).TotalHours < 1.5) { del = false; }
                    if (del && DateTime.UtcNow.Subtract(d2).TotalHours < 8) {
                        try {
                            FileInfo fi = new(thisf);
                            if (fi.Length > 400) { del = false; }
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

    protected override bool LoadMainData() {
        if (FileExists(Filename)) {
            _ = Directory.CreateDirectory(FragmengtsPath());
            if (!DirectoryExists(FragmengtsPath())) { return false; }
        }

        return base.LoadMainData();
    }

    protected override bool SaveRequired() => true;

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_writer == null) { StartWriter(); }
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(TableName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]", newChunkId);

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

        _ = Directory.CreateDirectory(FragmengtsPath());
        //Directory.CreateDirectory(OldFragmengtsPath());
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

    private string FragmengtsPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Frgm\\";

    private (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(DateTime endTimeUtc) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return (null, null); }

        if (!string.IsNullOrEmpty(FreezedReason)) { return (null, null); }

        CheckPath();

        try {

            #region Alle Fragment-Dateien im Verzeichnis ermitteln und eigene Ausfiltern (frgma)

            var frgma = Directory.GetFiles(FragmengtsPath(), TableName.ToUpper() + "-*." + SuffixOfFragments(), SearchOption.TopDirectoryOnly).ToList();
            _ = frgma.Remove(_myFragmentsFilename);

            #endregion

            if (frgma.Count == 0) { return ([], []); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                using var reader = new StreamReader(new FileStream(thisf, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
                var fil = reader.ReadToEnd();

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
            // Split-Datenbanken und Fragmente gehen nicht, siehe kommentar weiter unten
            return;
        }

        if (!string.IsNullOrEmpty(Filename) && IsInCache.Year < 2000) {
            Develop.DebugPrint(ErrorType.Error, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        data = data.OrderBy(obj => obj.DateTimeUtc).ToList();

        try {
            List<string> myfiles = [];

            if (checkedDataFiles != null) {
                foreach (var thisf in checkedDataFiles) {
                    if (thisf.Contains("\\" + TableName.ToUpperInvariant() + "-")) {
                        _ = myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            Interlocked.Increment(ref _doingChanges);
            try {
                foreach (var thisWork in data) {
                    if (TableName == thisWork.TableName && thisWork.DateTimeUtc > IsInCache) {
                        Undo.Add(thisWork);
                        ChangesNotIncluded.Add(thisWork);

                        var c = Column[thisWork.ColName];
                        var r = Row.SearchByKey(thisWork.RowKey);

                        var error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.NoUndo_NoInvalidate);

                        if (!string.IsNullOrEmpty(error)) {
                            Freeze("Datenbank-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
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

        _myFragmentsFilename = TempFile(FragmengtsPath(), TableName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments());

        if (Develop.AllReadOnly) { return; }

        FileStream? fileStream = null;
        try {
            fileStream = new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(fileStream, Encoding.UTF8);
            fileStream = null; // StreamWriter übernimmt ownership

            _writer.AutoFlush = true;
            _writer.WriteLine("- DB " + DatabaseVersion);
            _writer.WriteLine("- Filename " + Filename);
            _writer.WriteLine("- User " + UserName);

            var l = new UndoItem(TableName, DatabaseDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]", string.Empty);
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