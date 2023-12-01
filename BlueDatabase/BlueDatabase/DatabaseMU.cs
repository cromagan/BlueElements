// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueBasics;
using BlueDatabase.Enums;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseMU : Database {

    #region Fields

    private string _myFragmentsFilename = string.Empty;
    private StreamWriter? _writer;

    #endregion

    #region Constructors

    public DatabaseMU(string tablename) : base(tablename) { }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseMU);

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename, FreezedReason);

    #endregion

    #region Methods

    public new static DatabaseAbstract? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

        if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }
        if (ci.AdditionalData.FileSuffix().ToUpper() is not "MBDB") { return null; }
        if (!FileExists(ci.AdditionalData)) { return null; }

        var db = new DatabaseMU(ci.TableName);
        db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
        return db;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, string mustBeFreezed) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        var nn = Directory.GetFiles(Filename.FilePath(), "*.mbdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            var t = ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false, mustBeFreezed);
            if (t != null) { gb.Add(t); }
        }
        return gb;
    }

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists, string mustBeeFreezed) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".mbdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f, FreezedReason);
    }

    public string FragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm\\";
    }

    public override (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromUTC, DateTime toUTC) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return new(); }

        if (db.Count() == 0) { return new(); }

        CheckPath();

        try {

            #region Namen aller Tabellen um UCase ermitteln (tbn)

            var tbn = new List<string>();
            foreach (var thisdb in db) {
                tbn.AddIfNotExists(thisdb.TableName.ToUpper());
            }

            #endregion

            #region Alle Fragment-Dateien im Verzeichniss ermitteln und eigene Ausfiltern (frgma)

            var frgma = Directory.GetFiles(FragmengtsPath(), "*." + SuffixOfFragments(), SearchOption.TopDirectoryOnly).ToList();

            if (!frgma.Contains(_myFragmentsFilename) && !string.IsNullOrEmpty(_myFragmentsFilename)) { return (null, null); }

            frgma.Remove(_myFragmentsFilename);

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

            if (frgm.Count == 0) { return (new(), new()); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgm) {
                var reader = new StreamReader(new FileStream(thisf, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), System.Text.Encoding.UTF8);
                var fil = reader.ReadToEnd();
                reader.Close();

                //var fil = File.ReadAllText(thisf, System.Text.Encoding.UTF8);
                var fils = fil.SplitAndCutByCrToList();

                foreach (var thist in fils) {
                    if (!thist.StartsWith("-")) {
                        var u = new UndoItem(thist);
                        if (tbn.Contains(u.TableName.ToUpper())) {
                            if (u.DateTimeUtc.Subtract(IsInCache).TotalSeconds > 0 &&
                               u.DateTimeUtc.Subtract(toUTC).TotalSeconds < 0) {
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

    public override void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (FileExists(fileNameToLoad)) {
            Filename = fileNameToLoad;
            Directory.CreateDirectory(FragmengtsPath());
            Directory.CreateDirectory(OldFragmengtsPath());
            Filename = string.Empty;
        }

        base.LoadFromFile(fileNameToLoad, createWhenNotExisting, needPassword, freeze, ronly);
    }

    public string OldFragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm-Done\\";
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

    public string SuffixOfFragments() => "frg";

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_writer != null) {
                lock (_writer) {
                    _writer.WriteLine("- EOF");
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                }
                _writer = null;
            }
        }
        base.Dispose(disposing);
    }

    protected override void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, List<string> cellschanged, DateTime starttimeUTC) {
        base.DoWorkAfterLastChanges(files, columnsAdded, rowsAdded, cellschanged, starttimeUTC);
        if (ReadOnly) { return; }
        if (files == null || files.Count < 1) { return; }
        if (DateTime.UtcNow.Subtract(starttimeUTC).TotalSeconds > 20) { return; }
        if (!Directory.Exists(OldFragmengtsPath())) { return; }

        #region Dateien, mit jungen Änderungen wieder entfernen, damit andere Datenbanken noch Zugriff haben

        foreach (var thisch in _changesNotIncluded) {
            if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalMinutes < 20) {
                files.Remove(thisch.Container);
            }
        }

        #endregion

        #region Bei Bedarf neue Komplett-Datenbank erstellen

        if (_changesNotIncluded.Count() > 0 && AmITemporaryMaster(false)) {
            if (files.Count > 10 || _changesNotIncluded.Count() > 50) {
                //var tmp = _fileStateUTCDate;

                //_fileStateUTCDate = IsInCache;
                // Nicht FileStateUTCDate - sonst springt der Writer an!
                OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "Erstelle neue Komplett-Datenbank: " + TableName);
                if (!SaveInternal(IsInCache)) {
                    //_fileStateUTCDate = tmp;
                    return;
                }

                OnInvalidateView();
                _changesNotIncluded.Clear();
            }
        }

        #endregion

        #region Dateien, mit zu jungen Änderungen entfernen

        if (_changesNotIncluded.Count() > 0) {
            foreach (var thisch in _changesNotIncluded) {
                //if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalHours < 12) {
                files.Remove(thisch.Container);
                //}
            }
        }

        #endregion

        var pf = OldFragmengtsPath();

        files.Shuffle();

        foreach (var thisf in files) {
            OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix());
            IO.MoveFile(thisf, pf + thisf.FileNameWithSuffix(), 1, false);
            if (DateTime.UtcNow.Subtract(starttimeUTC).TotalSeconds > 20) { break; }
        }
    }

    protected override IEnumerable<DatabaseAbstract> LoadedDatabasesWithSameServer() {
        var oo = new List<DatabaseMU>();

        if (string.IsNullOrEmpty(Filename)) { return oo; }

        var filepath = Filename.FilePath();

        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseMU dbmu) {
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

        lock (_writer) {
            _writer.WriteLine(l.ToString());
        }

        return string.Empty;
    }

    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }

        Directory.CreateDirectory(FragmengtsPath());
        Directory.CreateDirectory(OldFragmengtsPath());
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));
            return;
        }
        CheckPath();

        //CheckSysUndoNow();

        _myFragmentsFilename = TempFile(FragmengtsPath(), TableName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString(Constants.Format_Date4, CultureInfo.InvariantCulture), SuffixOfFragments());

        _writer = new StreamWriter(new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read), System.Text.Encoding.UTF8);

        _writer.AutoFlush = true;
        _writer.WriteLine("- DB " + DatabaseVersion);
        _writer.WriteLine("- User " + UserName);

        var l = new UndoItem(TableName, DatabaseDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]");
        _writer.WriteLine(l.ToString());
        _writer.Flush();
    }

    #endregion
}