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
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DatabaseMU : Database {

    #region Fields

    private string _myFragmentsFilename = string.Empty;
    private StreamWriter? _writer;
    private bool _written = false;

    #endregion

    #region Constructors

    public DatabaseMU(string filename, bool readOnly, string freezedReason, bool create, NeedPassword? needPassword) : base(filename, readOnly, freezedReason, create, needPassword) {
        //IsInCache = FileStateUTCDate;

        CheckSysUndoNow();
        StartWriter();
    }

    public DatabaseMU(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : base(ci, readOnly, needPassword) {
        //IsInCache = FileStateUTCDate;

        CheckSysUndoNow();
        StartWriter();
    }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseMU);

    #endregion

    #region Methods

    public new static DatabaseAbstract? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

        if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }
        if (ci.AdditionalData.FileSuffix().ToUpper() is not "MBDB") { return null; }
        if (!FileExists(ci.AdditionalData)) { return null; }
        return new Database(ci, readOnly, needPassword);
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

    public string FragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm\\";
    }

    public override List<UndoItem>? GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromUTC, DateTime toUTC) {
        if (string.IsNullOrEmpty(FragmengtsPath())) { return new(); }

        if (db.Count() == 0) { return new(); }

        try {

            #region Namen aller Tabellen um UCase ermitteln (tbn)

            var tbn = new List<string>();
            foreach (var thisdb in db) {
                tbn.AddIfNotExists(thisdb.TableName.ToUpper());
            }

            #endregion

            #region Alle Fragment-Dateien im Verzeichniss ermitteln und eigene Ausfiltern (frgma)

            var frgma = Directory.GetFiles(Filename.FilePath(), "*." + SuffixOfFragments(), SearchOption.AllDirectories).ToList();

            if (!frgma.Contains(_myFragmentsFilename) && !string.IsNullOrEmpty(_myFragmentsFilename)) { return null; }

            frgma.Remove(_myFragmentsFilename);

            #endregion

            #region Alle Fragments ermitteln, wo die Datenbank aktuell im Speicher ist (frgm)

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

            if (frgm.Count == 0) { return new(); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgm) {
                var fil = File.ReadAllText(thisf, System.Text.Encoding.UTF8);
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

            return l;
        } catch { }
        return null;
    }

    public string OldFragmengtsPath() {
        if (string.IsNullOrEmpty(Filename)) { return string.Empty; }
        return Filename.FilePath() + "Frgm-Done\\";
    }

    public override bool Save() {
        if (_writer == null) { return false; }

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
                    if (!_written) { IO.DeleteFile(_myFragmentsFilename, false); }
                }
                _writer = null;
            }
        }
        base.Dispose(disposing);
    }

    protected override void DoWorkAfterLastChanges(List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, List<string> cellschanged) {
        if (_changesCount.Count() < 50) { return; }
        if (!AmITemporaryMaster(false)) { return; }

        if (!Directory.Exists(OldFragmengtsPath())) { return; }

        #region Alle Dateien ermitteln (allfiles)

        var allfiles = new List<string>();

        foreach (var thisch in _changesCount) {
            allfiles.AddIfNotExists(thisch.Container);
        }

        #endregion

        #region Dateien, mit jungen Ändeurngen wieder entfernen, damit ander Datenbanken noch zugriff haben

        foreach (var thisch in _changesCount) {
            if (DateTime.UtcNow.Subtract(thisch.DateTimeUtc).TotalMinutes < 10) {
                allfiles.Remove(thisch.Container);
            }
        }

        #endregion

        if (allfiles.Count < 10) { return; }

        var tmp = _fileStateUTCDate;

        _fileStateUTCDate = IsInCache;
        // Nicht FileStateUTCDate - sonst springt der Writer an!

        if (!SaveInternal()) {
            _fileStateUTCDate = tmp;
            return;
        }

        OnInvalidateView();
        _changesCount.Clear();

        var pf = OldFragmengtsPath();

        foreach (var thisf in allfiles) {
            if (thisf.Contains("\\" + TableName.ToUpper() + "-")) {
                IO.MoveFile(thisf, pf + thisf.FileNameWithSuffix(), false);
            }
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
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(TableName, type, column, row, string.Empty, value, user, datetimeutc, comment);

        lock (_writer) {
            _writer.WriteLine(l.ToString());
            _written = true;
        }

        return string.Empty;
    }

    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));
            return;
        }

        _myFragmentsFilename = TempFile(FragmengtsPath(), TableName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString(Constants.Format_Date4, CultureInfo.InvariantCulture), SuffixOfFragments());

        _writer = new StreamWriter(new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read));

        _writer.AutoFlush = true;
        _writer.WriteLine("- DB " + DatabaseVersion);
        _writer.WriteLine("- User " + UserName);

        Directory.CreateDirectory(OldFragmengtsPath());
    }

    #endregion
}