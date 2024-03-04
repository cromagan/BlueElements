// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseMu : Database {

    #region Fields

    private bool _mustMakeMaster;
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
    protected override bool MultiUser => true;

    #endregion

    #region Methods

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".mbdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f, FreezedReason);
    }

    //    var db = new DatabaseMu(ci.TableName);
    //    db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
    //    return db;
    //}
    public override void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (FileExists(fileNameToLoad)) {
            Filename = fileNameToLoad;
            Directory.CreateDirectory(FragmengtsPath());
            Directory.CreateDirectory(OldFragmengtsPath());
            Filename = string.Empty;
        }

        base.LoadFromFile(fileNameToLoad, createWhenNotExisting, needPassword, freeze, ronly);
    }

    //    if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }
    //    if (ci.AdditionalData.FileSuffix().ToUpper() is not "MBDB") { return null; }
    //    if (!FileExists(ci.AdditionalData)) { return null; }
    public override bool Save() {
        if (_writer == null) { return true; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            return true;
        } catch { return false; }
    }

    //public new static Database? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
    //    if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }
    protected override List<ConnectionInfo>? AllAvailableTables(List<Database>? allreadychecked, string mustBeFreezed) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db && !db.IsDisposed) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        var nn = Directory.GetFiles(Filename.FilePath(), "*.mbdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            var t = ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false);
            if (t != null) { gb.Add(t); }
        }
        return gb;
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

    protected override void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, DateTime starttimeUtc) {
        base.DoWorkAfterLastChanges(files, columnsAdded, rowsAdded, starttimeUtc);
        if (ReadOnly) { return; }
        if (files == null || files.Count < 1) { return; }

        _mustMakeMaster = files.Count > 8 || ChangesNotIncluded.Count > 40;

        if (DateTime.UtcNow.Subtract(starttimeUtc).TotalSeconds > 20) { return; }
        if (!Directory.Exists(OldFragmengtsPath())) { return; }

        #region Dateien, mit jungen Änderungen wieder entfernen, damit andere Datenbanken noch Zugriff haben

        foreach (var thisch in ChangesNotIncluded) {
            if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalMinutes < 20) {
                files.Remove(thisch.Container);
            }
        }

        #endregion

        #region Bei Bedarf neue Komplett-Datenbank erstellen

        if (ChangesNotIncluded.Any() && AmITemporaryMaster(false)) {
            if (files.Count > 10 || ChangesNotIncluded.Count > 50) {
                //var tmp = _fileStateUTCDate;

                //_fileStateUTCDate = IsInCache;
                // Nicht FileStateUTCDate - sonst springt der Writer an!
                OnDropMessage(FehlerArt.Info, "Erstelle neue Komplett-Datenbank: " + TableName);
                if (!SaveInternal(IsInCache)) {
                    //_fileStateUTCDate = tmp;
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

        var pf = OldFragmengtsPath();

        files.Shuffle();

        foreach (var thisf in files) {
            OnDropMessage(FehlerArt.Info, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix());
            MoveFile(thisf, pf + thisf.FileNameWithSuffix(), 1, false);
            if (DateTime.UtcNow.Subtract(starttimeUtc).TotalSeconds > 20) { break; }
        }
    }

    protected override (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(ICollection<Database> db, DateTime fromUtc, DateTime toUtc) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return new(); }

        if (!db.Any()) { return new(); }

        CheckPath();

        try {

            #region Namen aller Tabellen um UCase ermitteln (tbn), alle ungültigen FRG-Dateien ermitteln (frgu)

            var tbn = new List<string>();
            var frgu = new List<string>();
            foreach (var thisdb in db) {
                if (thisdb is DatabaseMu dbmu) {
                    tbn.AddIfNotExists(dbmu.TableName.ToUpper());
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
                    if (thisn.ToUpper().Contains("\\" + thistbn + "-")) {
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
                        if (tbn.Contains(u.TableName.ToUpper())) {
                            if (u.DateTimeUtc.Subtract(IsInCache).TotalSeconds > 0 &&
                               u.DateTimeUtc.Subtract(toUtc).TotalSeconds < 0) {
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

    protected override bool IsThereNeedToMakeMeMaster() {
        if (_mustMakeMaster) { return true; }
        return base.IsThereNeedToMakeMeMaster();
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

    protected override string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }
        HasPendingChanges = false; // Datenbank kann keine Pendings haben

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!

        if (_writer == null) { StartWriter(); }
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(TableName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]");

        try {
            lock (_writer) {
                _writer.WriteLine(l.ToString());
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
        Directory.CreateDirectory(OldFragmengtsPath());
    }

    private string FragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm\\";
    }

    private string OldFragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm-Done\\";
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));
            return;
        }
        CheckPath();

        //CheckSysUndoNow();

        _myFragmentsFilename = TempFile(FragmengtsPath(), TableName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString(Constants.Format_Date4, CultureInfo.InvariantCulture), SuffixOfFragments());

        _writer = new StreamWriter(new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);

        _writer.AutoFlush = true;
        _writer.WriteLine("- DB " + DatabaseVersion);
        _writer.WriteLine("- Filename " + Filename);
        _writer.WriteLine("- User " + UserName);

        var l = new UndoItem(TableName, DatabaseDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]");
        _writer.WriteLine(l.ToString());
        _writer.Flush();
    }

    #endregion
}