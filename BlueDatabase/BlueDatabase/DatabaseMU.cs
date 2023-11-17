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
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
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

    #endregion

    #region Constructors

    public DatabaseMU(string filename, bool readOnly, string freezedReason, bool create, NeedPassword? needPassword) : base(filename, readOnly, freezedReason, create, needPassword) {
        StartWriter();
    }

    public DatabaseMU(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : base(ci, readOnly, needPassword) {
        StartWriter();
    }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseMU);

    protected override bool DoCellChanges => true;

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

        try {
            var frgm = Directory.GetFiles(Filename.FilePath(), "*." + SuffixOfFrgaments(), SearchOption.AllDirectories).ToList();

            if (!frgm.Contains(_myFragmentsFilename)) { return null; }

            frgm.Remove(_myFragmentsFilename);

            if (frgm.Count == 0) { return new(); }
            if (db.Count() == 0) { return new(); }

            var tbn = new List<string>();
            foreach (var thisdb in db) {
                tbn.AddIfNotExists(thisdb.TableName.ToUpper());
            }

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

    public override bool Save() {
        if (_writer == null) { return false; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            return true;
        } catch { return false; }
    }

    public string SuffixOfFrgaments() => "frg";

    internal override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, Reason reason, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        if (type.IsObsolete()) { return string.Empty; }

        if (reason != Reason.LoadReload && type != DatabaseDataType.SystemValue) {
            if (ReadOnly) { return "Datenbank schreibgeschützt!"; } // Sicherheitshalber!
            if (_writer == null) { return "Schreibmodus deaktiviert"; }
            var l = new UndoItem(TableName, type, column, row, string.Empty, value, user, datetimeutc, comment);

            lock (_writer) {
                _writer.WriteLine(l.ToString());
            }
        }

        return base.SetValueInternal(type, value, column, row, reason, user, datetimeutc, comment);
    }

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

    private void StartWriter() {
        IsInCache = DateTimeParse(FileStateUTCDate);

        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + IsInCache.ToString(Constants.Format_Date5, CultureInfo.InvariantCulture));
            return;
        }

        _myFragmentsFilename = TempFile(FragmengtsPath(), UserName + "-" + DateTime.UtcNow.ToString(Constants.Format_Date4, CultureInfo.InvariantCulture), SuffixOfFrgaments());

        _writer = new StreamWriter(new FileStream(_myFragmentsFilename, FileMode.Append, FileAccess.Write, FileShare.Read));

        _writer.AutoFlush = true;
        _writer.WriteLine("- DB " + DatabaseVersion);
    }

    #endregion
}