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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseMu : Database {

    #region Fields

    private bool _masterNeeded;

    private string _myFragmentsFilename = string.Empty;

    private StreamWriter? _writer;

    #endregion

    #region Constructors

    public DatabaseMu(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~DatabaseMu() { Dispose(false); }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseMu);

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename, FreezedReason);

    /// <summary>
    /// Wenn die Prüfung ergibt, dass zu viele Fragmente da sind, wird hier auf true gesetzt
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="ranges">Unter 5 Minuten wird auch geprüft, ob versucht wird, einen Master zu setzen. Ab 5 minuten ist es gewiss.</param>
    /// <param name="rangee">Bis 55 Minuten ist sicher, dass es der Master ist.
    /// Werden kleiner Werte abgefragt, kann ermittelt werden, ob der Master bald ausläuft.
    /// Werden größerer Werte abgefragt, kann ermittel werden, ob man Master war,
    /// </param>
    /// <returns></returns>
    public override bool AmITemporaryMaster(int ranges, int rangee) {
        if (!base.AmITemporaryMaster(ranges, rangee)) { return false; }
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 5) { return false; }
        if (TemporaryDatabaseMasterUser != MyMasterCode) { return false; }

        var d = DateTimeParse(TemporaryDatabaseMasterTimeUtc);
        var mins = DateTime.UtcNow.Subtract(d).TotalMinutes;

        ranges = Math.Max(ranges, 0);
        //rangee = Math.Min(rangee, 55);

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogeprüft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return mins > ranges && mins < rangee;
    }

    public override void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (FileExists(fileNameToLoad)) {
            Filename = fileNameToLoad;
            Directory.CreateDirectory(FragmengtsPath());
            //Directory.CreateDirectory(OldFragmengtsPath());
            Filename = string.Empty;
        }

        base.LoadFromFile(fileNameToLoad, createWhenNotExisting, needPassword, freeze, ronly);
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

    protected override void DidLastChanges() {
        base.DidLastChanges();
        TryToSetMeTemporaryMaster();
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
        }

        if (_writer != null) {
            lock (_writer) {
                try {
                    _writer.WriteLine("- EOF");
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                } catch { }
            }
            _writer = null;
        }

        base.Dispose(disposing);
    }

    protected override void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, DateTime startTimeUtc, DateTime endTimeUtc) {
        base.DoWorkAfterLastChanges(files, columnsAdded, rowsAdded, startTimeUtc, endTimeUtc);
        if (ReadOnly) { return; }
        if (files is not { Count: >= 1 }) { return; }

        _masterNeeded = files.Count > 8 || ChangesNotIncluded.Count > 40 || DateTime.UtcNow.Subtract(FileStateUtcDate).TotalHours > 12;

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > 20) { return; }

        #region Dateien, mit jungen Änderungen wieder entfernen, damit andere Datenbanken noch Zugriff haben

        foreach (var thisch in ChangesNotIncluded) {
            if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalMinutes < 20) {
                files.Remove(thisch.Container);
            }
        }

        #endregion

        #region Bei Bedarf neue Komplett-Datenbank erstellen

        if (!Develop.AllReadOnly && DateTime.UtcNow.Subtract(FileStateUtcDate).TotalMinutes > 15 && AmITemporaryMaster(5, 55)) {
            if (ChangesNotIncluded.Count > 50 || DateTime.UtcNow.Subtract(FileStateUtcDate).TotalHours > 12) {
                OnDropMessage(FehlerArt.Info, "Erstelle neue Komplett-Datenbank: " + TableName);
                if (!SaveInternal(IsInCache)) {
                    return;
                }

                OnInvalidateView();
                ChangesNotIncluded.Clear();
            }
        }

        #endregion

        #region Dateien, mit zu jungen Änderungen entfernen

        if (ChangesNotIncluded.Any()) {
            foreach (var thisch in ChangesNotIncluded) {
                //if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalHours < 12) {
                files.Remove(thisch.Container);
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
                OnDropMessage(FehlerArt.Info, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix());
                DeleteFile(thisf, 1, false);
                //MoveFile(thisf, pf + thisf.FileNameWithSuffix(), 1, false);
                if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > 20) { break; }
            }
        }
    }

    protected override (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(IEnumerable<Database> db, DateTime fromUtc, DateTime endTimeUtc) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return new(); }

        if (!db.Any()) { return new(); }

        CheckPath();

        try {

            #region Namen aller Tabellen um UCase ermitteln (tbn), alle ungültigen FRG-Dateien ermitteln (frgu)

            var tbn = new List<string>();
            var frgu = new List<string>();
            foreach (var thisdb in db) {
                if (thisdb is DatabaseMu dbmu && string.IsNullOrEmpty(dbmu.FreezedReason)) {
                    tbn.AddIfNotExists(dbmu.TableName.ToUpperInvariant());
                    if (!string.IsNullOrEmpty(dbmu._myFragmentsFilename)) {
                        frgu.Add(dbmu._myFragmentsFilename);
                    }
                }
            }

            #endregion

            #region Alle Fragment-Dateien im Verzeichniss ermitteln und eigene Ausfiltern (frgma)

            var frgma = Directory.GetFiles(FragmengtsPath(), "*." + SuffixOfFragments(), SearchOption.TopDirectoryOnly).ToList();

            if (!frgma.Contains(_myFragmentsFilename) && !string.IsNullOrEmpty(_myFragmentsFilename)) { return (null, null); }

            frgma.RemoveRange(frgu);

            #endregion

            #region Alle Fragments-Dateien ermitteln, wo die Datenbank aktuell im Speicher ist (frgm)

            var frgm = new List<string>();

            foreach (var thisn in frgma) {
                foreach (var thistbn in tbn) {
                    if (thisn.ToUpperInvariant().Contains("\\" + thistbn + "-")) {
                        frgm.Add(thisn);
                        break;
                    }
                }
            }

            #endregion

            if (frgm.Count == 0) { return ([], []); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgm) {
                var reader = new StreamReader(new FileStream(thisf, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
                var fil = reader.ReadToEnd();
                reader.Close();

                //var fil = File.ReadAllText(thisf, System.Text.Encoding.UTF8);
                var fils = fil.SplitAndCutByCrToList();

                foreach (var thist in fils) {
                    if (!thist.StartsWith("-")) {
                        var u = new UndoItem(thist);
                        if (tbn.Contains(u.TableName.ToUpperInvariant())) {
                            if (u.DateTimeUtc.Subtract(IsInCache).TotalSeconds > 0 &&
                               u.DateTimeUtc.Subtract(endTimeUtc).TotalSeconds < 0) {
                                u.Container = thisf;
                                l.Add(u);
                            }
                        }
                    }
                }
            }

            return (l, frgm);
        } catch { }
        return (null, null);
    }

    protected override List<Database> LoadedDatabasesWithSameServer() {
        var oo = new List<Database>();

        if (string.IsNullOrEmpty(Filename)) { return oo; }

        var filepath = Filename.FilePath();

        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseMu dbmu) {
                if (dbmu.Filename.FilePath().Equals(filepath, StringComparison.OrdinalIgnoreCase)) {
                    oo.Add(dbmu);
                }
            }
        }

        return oo;
    }

    protected override bool SaveRequired() => true; // immer "speichern"

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment, string chunk) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment, chunk);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_writer == null) { StartWriter(); }
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(TableName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]", chunk);

        try {
            lock (_writer) {
                _writer.WriteLine(l.ParseableItems().FinishParseable());
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }

        return string.Empty;
    }

    private static string SuffixOfFragments() => "frg";

    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }

        Directory.CreateDirectory(FragmengtsPath());
        //Directory.CreateDirectory(OldFragmengtsPath());
    }

    private string FragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm\\";
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString5());
            return;
        }
        CheckPath();

        //CheckSysUndoNow();

        _myFragmentsFilename = TempFile(FragmengtsPath(), TableName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments());

        if (Develop.AllReadOnly) { return; }

        try {
            _writer = new StreamWriter(new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        } catch {
            Generic.Pause(3, false);
            Develop.CheckStackForOverflow();
            StartWriter();
            return;
        }

        try {
            _writer.AutoFlush = true;
            _writer.WriteLine("- DB " + DatabaseVersion);
            _writer.WriteLine("- Filename " + Filename);
            _writer.WriteLine("- User " + UserName);

            var l = new UndoItem(TableName, DatabaseDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]", string.Empty);
            _writer.WriteLine(l.ParseableItems().FinishParseable());
            _writer.Flush();
        } catch { }
    }

    #endregion
}