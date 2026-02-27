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
using BlueBasics;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.Classes;

namespace BlueTable.Classes;

/// <summary>
/// Verwaltet Tabellenfragmente für Multi-User-Umgebungen, um gleichzeitiges Schreiben zu ermöglichen.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFragments : TableFile {

    #region Fields

    /// <summary>
    /// Wert in Sekunden. Nach dieser Zeit soll der FragmentAufräumer beendet werden.
    /// </summary>
    public static readonly int AbortFragmentDeletion = 10;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit dürfen Fragmente gelöscht werden.
    /// </summary>
    public static readonly int DeleteFragmentsAfter = DoComplete * 2 + UpdateTable * 2;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen.
    /// </summary>
    public static readonly int DoComplete = 60;

    /// <summary>
    /// Liste der Änderungen, die noch nicht in der Hauptdatei enthalten sind.
    /// </summary>
    private readonly List<UndoItem> _changesNotIncluded = [];

    /// <summary>
    /// Cache für bereits verarbeitete Fragmente (Hashes der Undo-Zeilen), um doppelte Verarbeitung zu verhindern.
    /// </summary>
    private readonly HashSet<string> _processedHashes = [];

    /// <summary>
    /// Zähler für aktive Änderungsprozesse zur Vermeidung von Race-Conditions.
    /// Während der Daten aktualiszer werden dürfen z.B. keine Tabellenansichten gemacht werden.
    /// Weil da Zeilen sortiert / invalidiert / Sortiert / invalidiert etc. werden
    /// </summary>
    private int _doingChanges;

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    private DateTime _isInCache = new(0);

    private bool _masterNeeded;
    private string _myFragmentsFilename = string.Empty;
    private System.IO.StreamWriter? _writer;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="TableFragments"/> Klasse.
    /// </summary>
    /// <param name="tablename">Name der Tabelle.</param>
    public TableFragments(string tablename) : base(tablename) {
    }

    #endregion

    #region Destructors

    /// <summary>
    /// Finalisator für die TableFragments Klasse.
    /// </summary>
    ~TableFragments() {
        Dispose(false);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gibt an, ob die Fragmentdatei beim Schließen gelöscht werden darf (wenn keine wichtigen Änderungen enthalten sind).
    /// </summary>
    public bool CanDeleteWriter { get; private set; } = true;

    /// <summary>
    /// Wenn die Prüfung ergibt, dass zu viele Fragmente da sind, wird hier auf true gesetzt.
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    /// <summary>
    /// Gibt an, ob Multi-User-Zugriff möglich ist.
    /// </summary>
    public override bool MultiUserPossible => true;

    /// <summary>
    /// Gibt an, ob Speichern erforderlich ist (hier immer false, da Fragmente direkt geschrieben werden).
    /// </summary>
    protected override bool SaveRequired => false;

    #endregion

    #region Methods

    /// <summary>
    /// Prüft, ob die aktuelle Instanz als temporärer Master fungieren darf.
    /// </summary>
    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (_isInCache.Year < 2000) { return false; }

        if (updateAllowed && DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > UpdateTable) {
            if (!BeSureToBeUpToDate(false)) { return false; }
        }

        return base.AmITemporaryMaster(ranges, rangee, updateAllowed);
    }

    /// <summary>
    /// Stellt sicher, dass die Daten aktuell sind, indem Fragmente nachgeladen werden.
    /// </summary>
    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (firstTime) {
            _isInCache = LastSaveMainFileUtcDate;
        }

        DropMessage(ErrorType.Info, "Lade Fragmente von '" + KeyName + "'");
        var lastFragmentDate = DateTime.UtcNow;

        var (changes, files, failed) = GetLastChanges(lastFragmentDate);
        if (failed) { return false; }

        var opr = InjectData(files, changes, DateTime.UtcNow, lastFragmentDate);
        return opr.IsSuccessful;
    }

    /// <summary>
    /// Friert die Tabelle ein und schließt den Writer.
    /// </summary>
    public override void Freeze(string reason) {
        CloseWriter();
        base.Freeze(reason);
    }

    /// <summary>
    /// Fordert Schreibzugriff an und startet bei Bedarf den Fragment-Writer.
    /// </summary>
    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (_writer == null) { StartWriter(); }

        if (_writer == null) { return "Schreib-Objekt nicht erstellt."; }

        return string.Empty;
    }

    /// <summary>
    /// Prüft, warum die Tabelle aktuell nicht editierbar ist.
    /// </summary>
    public override string IsNotEditableReason(bool isloading) {
        var aadc = base.IsNotEditableReason(isloading);
        if (!string.IsNullOrEmpty(aadc)) { return aadc; }

        if (string.IsNullOrEmpty(FragmengtsPath())) { return "Fragmentpfad nicht gesetzt."; }

        if (_doingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        return string.Empty;
    }

    /// <summary>
    /// Setzt die Instanz als Master, sofern die Daten aktuell genug sind.
    /// </summary>
    public override void MasterMe() {
        if (DateTime.UtcNow.Subtract(_isInCache).TotalMinutes > 1) { return; }
        base.MasterMe();
    }

    /// <summary>
    /// Gibt die Ressourcen frei.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            UnMasterMe();
            CloseWriter();

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Lädt die Hauptdaten und stellt sicher, dass das Fragment-Verzeichnis existiert.
    /// </summary>
    protected override bool LoadMainData() {
        if (FileExists(Filename)) {
            if (!CreateDirectory(FragmengtsPath())) { return false; }
        }

        return base.LoadMainData();
    }

    /// <summary>
    /// Interner Speicheraufruf (Flush des Writers).
    /// </summary>
    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (_writer == null) { return "Writer Fehler"; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    /// <summary>
    /// Schreibt einen Wert in die Fragmentdatei.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        var f = base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, oldChunkId, newChunkId, comment);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (IsFreezed) { return "Tabelle schreibgeschützt!"; }

        if (Develop.AllReadOnly) { return string.Empty; }

        if (_writer == null) { StartWriter(); }
        if (_writer == null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(KeyName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]", newChunkId);

        try {
            lock (_writer) {
                string line = l.ParseableItems().FinishParseable();
                _writer.WriteLine(line);

                // Eigene Änderungen ebenfalls in den Hash-Cache aufnehmen
                _processedHashes.Add(line.GetMD5Hash());

                if (!type.IsUnimportant()) { CanDeleteWriter = false; }
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }

        return string.Empty;
    }

    /// <summary>
    /// Gibt das Suffix für Fragmentdateien zurück.
    /// </summary>
    private static string SuffixOfFragments() => "frg";

    /// <summary>
    /// Überprüft und erstellt den Pfad für Fragmente.
    /// </summary>
    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        CreateDirectory(FragmengtsPath());
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Fragmentdatei, falls sie keine permanenten Daten enthält.
    /// </summary>
    private void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose != null) {
            _writer = null;

            try {
                writerToClose.WriteLine("- EOF");
                writerToClose.Flush();
            } catch { } finally {
                try {
                    writerToClose.Dispose();
                } catch { }
            }

            if (CanDeleteWriter) {
                DeleteFile(_myFragmentsFilename, false);
            }
        }
    }

    /// <summary>
    /// Führt Aufräumarbeiten nach dem Laden von Fragmenten durch, inkl. Erstellung einer neuen Hauptdatei.
    /// </summary>
    private void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc) {
        if (Ending) { return; }
        if (!IsEditable(false)) { return; }
        if (files is not { Count: >= 1 }) { return; }

        _masterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > DoComplete;

        #region Bei Bedarf neue Komplett-Tabelle erstellen

        if (_masterNeeded && AmITemporaryMaster(MasterTry, MasterUntil, true)) {
            DropMessage(ErrorType.Info, "Erstelle neue Komplett-Tabelle: " + KeyName);

            var t = LastSaveMainFileUtcDate;
            var f = SaveMainFile(this, _isInCache);

            if (!string.IsNullOrEmpty(f)) {
                DropMessage(ErrorType.Info, $"Komplettierung von {Caption} fehlgeschlagen: {f}");
                return;
            }

            CloseWriter();
            StartWriter();

            ChangeData(TableDataType.LastSaveMainFileUtcDate, null, t.ToString7(), LastSaveMainFileUtcDate.ToString7());
            MasterMe();

            _masterNeeded = false;
            OnInvalidateView();
            _changesNotIncluded.Clear();

            // Nach Komplettierung Cache leeren, da Daten nun im Hauptfile
            _processedHashes.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien, mit zu jungen Änderungen entfernen

        if (_changesNotIncluded.Any()) {
            foreach (var thisch in _changesNotIncluded) {
                files.Remove(thisch.Container);
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
                        DeleteFile(thisf, 0);
                        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { break; }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gibt den Pfad zum Fragment-Ordner zurück.
    /// </summary>
    private string FragmengtsPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Frgm\\";

    /// <summary>
    /// Ermittelt die neuesten Änderungen aus den Fragmentdateien.
    /// </summary>
    private (List<UndoItem>? Changes, List<string>? Files, bool failed) GetLastChanges(DateTime endTimeUtc) {
        if (!IsEditable(true)) { return (null, null, true); }

        CheckPath();

        try {
            var filesystem = CachedFileSystem.Get(FragmengtsPath());
            var frgma = filesystem.GetFiles(FragmengtsPath(), [KeyName.ToUpper() + "-*." + SuffixOfFragments()]);
            frgma.Remove(_myFragmentsFilename);

            if (frgma.Count == 0) { return ([], [], false); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                var fil = filesystem.ReadAllText(thisf, Encoding.UTF8);
                var fils = fil.SplitAndCutByCr().ToList();

                foreach (var thist in fils) {
                    if (!thist.StartsWith("-")) {
                        var hash = thist.GetMD5Hash();
                        if (_processedHashes.Contains(hash)) { continue; }

                        var u = new UndoItem(thist);
                        u.Container = thisf;
                        l.Add(u);
                        _processedHashes.Add(hash); // Sofort als verarbeitet markieren
                    }
                }
            }

            return (l, frgma, false);
        } catch { }
        return (null, null, true);
    }

    /// <summary>
    /// Injiziert die geladenen Fragmentdaten in die aktuelle Tabellenstruktur.
    /// </summary>
    /// <param name="checkedDataFiles"></param>
    /// <param name="data"></param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    private OperationResult InjectData(List<string>? checkedDataFiles, List<UndoItem>? data, DateTime startTimeUtc, DateTime endTimeUtc) {
        if (data == null) { return OperationResult.Success; }
        if (!IsEditable(true)) { return OperationResult.Failed("Tabelle nicht bearbeitbar"); }

        if (Column.ChunkValueColumn is { IsDisposed: false }) { return OperationResult.Failed("Falscher Tabellentyp"); }

        var dataSorted = data.Where(obj => obj?.DateTimeUtc != null).OrderBy(obj => obj.DateTimeUtc);
        var affectingHead = false;

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
                    if (KeyName == thisWork.TableName) {
                        Undo.Add(thisWork);
                        _changesNotIncluded.Add(thisWork);

                        affectingHead |= (!thisWork.Command.IsCellValue() && !thisWork.Command.IsUnimportant());

                        var c = Column[thisWork.ColName];
                        var r = Row.GetByKey(thisWork.RowKey);

                        var error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.NoUndo_NoInvalidate);

                        if (!string.IsNullOrEmpty(error)) {
                            Freeze("Tabellen-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
                            return OperationResult.Failed(error);
                        }
                    }
                }
                _isInCache = endTimeUtc;
            } finally {
                Interlocked.Decrement(ref _doingChanges);
                Column.GetSystems();
                DoWorkAfterLastChanges(myfiles, startTimeUtc);
                RepairAfterParse();
                TryToSetMeTemporaryMaster();
                OnInvalidateView();
                OnLoaded(false, affectingHead);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return InjectData(checkedDataFiles, data, startTimeUtc, endTimeUtc);
        }

        return OperationResult.Success;
    }

    /// <summary>
    /// Initialisiert den StreamWriter für die Fragmentdatei.
    /// </summary>
    private void StartWriter() {
        if (string.IsNullOrEmpty(FragmengtsPath())) {
            Freeze("Fragmentpfad nicht gesetzt. Stand: " + _isInCache.ToString5());
            return;
        }

        if (!IsEditable(false)) { return; }
        CheckPath();

        var tmp = TempFile(FragmengtsPath(), KeyName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments());

        if (tmp.Contains("_000")) {
            Pause(1, false);
            Develop.AbortAppIfStackOverflow();
            StartWriter();
            return;
        }

        _myFragmentsFilename = tmp;

        if (Develop.AllReadOnly) { return; }

        System.IO.FileStream? fileStream = null;
        try {
            fileStream = new System.IO.FileStream(_myFragmentsFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            _writer = new System.IO.StreamWriter(fileStream, Encoding.UTF8);
            fileStream = null;

            _writer.AutoFlush = true;
            _writer.WriteLine("- Version " + TableVersion);
            _writer.WriteLine("- Filename " + Filename);
            _writer.WriteLine("- User " + UserName);

            var l = new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, " Dummy - systembedingt benötigt", "[Änderung in dieser Session]", string.Empty);
            _writer.WriteLine(l.ParseableItems().FinishParseable());
            CanDeleteWriter = true;
            _writer.Flush();
        } catch {
            fileStream?.Dispose();
            _writer?.Dispose();
            _writer = null;

            Pause(3, false);
            Develop.AbortAppIfStackOverflow();
            StartWriter();
        }
    }

    #endregion
}