// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Threading;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;

namespace BlueTable.Classes;

/// <summary>
/// Verwaltet Tabellenfragmente für Multi-User-Umgebungen, um gleichzeitiges Schreiben zu ermöglichen.
/// </summary>
[Browsable(false)]
[FileSuffix(".mbdb")]
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
    public static readonly int DeleteFragmentsAfter = 60 * 2 + 5 * 2;

    /// <summary>
    /// Wert in Minuten. Nach dieser Zeit sollte eine Komplettierung erfolgen.
    /// </summary>
    public static readonly int DoComplete = 60;

    /// <summary>
    /// Gibt das Suffix für Fragmentdateien zurück.
    /// </summary>
    public static readonly string SuffixOfFragments = "frg";

    /// <summary>
    /// Liste der Änderungen, die noch nicht in der Hauptdatei enthalten sind.
    /// </summary>
    private readonly List<UndoItem> _changesNotIncluded = [];

    /// <summary>
    /// Cache für bereits verarbeitete Fragmente (Hashes der Undo-Zeilen), um doppelte Verarbeitung zu verhindern.
    /// Thread-safe durch ConcurrentDictionary; Value wird nicht genutzt (Set-Semantik).
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _processedHashes = new();

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
    public TableFragments(string tablename) : base(tablename) { }

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="TableFragments"/> Klasse als Kopie einer bestehenden Tabelle.
    /// </summary>
    /// <param name="filename">Dateiname der neuen Tabelle.</param>
    /// <param name="source">Quelltabelle, deren Daten kopiert werden.</param>
    public TableFragments(string filename, Table? source) : base(filename, source) { }

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

    #endregion

    #region Methods

    /// <summary>
    /// Fordert Schreibzugriff an und startet bei Bedarf den Fragment-Writer.
    /// </summary>
    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.AcquireWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

        if (_writer is null) { StartWriter(); }

        if (_writer is null) { return "Schreib-Objekt nicht erstellt."; }

        return string.Empty;
    }

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
            MainChunkLoadDone = true; // TODO: DIRTY FIX ENTFERNEN!
        }

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Fragmente von '" + KeyName + "'", 0); }
        var lastFragmentDate = DateTime.UtcNow;

        var (changes, files, failed) = GetLastChanges();
        if (failed) { return false; }

        var opr = InjectData(files, changes, DateTime.UtcNow, lastFragmentDate, firstTime);
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
    /// Prüft, warum die Tabelle aktuell nicht editierbar ist.
    /// </summary>
    public override string IsGenericEditable(bool isloading) {
        var f = base.IsGenericEditable(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (InitialSavePending) { return string.Empty; }

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
            CloseWriter();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Lädt die Hauptdaten und stellt sicher, dass das Fragment-Verzeichnis existiert.
    /// </summary>
    protected override bool LoadMainData() {
        if (IO.FileExists(Filename)) {
            if (IO.CreateDirectory(FragmengtsPath()).IsFailed) { return false; }
        }

        return base.LoadMainData();
    }

    /// <summary>
    /// Interner Speicheraufruf (Flush des Writers).
    /// </summary>
    protected override string SaveInternal() {
        if (InitialSavePending) { return base.SaveInternal(); }

        if (_writer is null) { return "Writer Fehler"; }

        try {
            lock (_writer) {
                _writer.Flush();
            }
            SaveRequired = false;
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    /// <summary>
    /// Schreibt einen Wert in die Fragmentdatei.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment) is { Length: > 0 } f) { return f; }

        if (Develop.AllReadOnly) { return string.Empty; }

        if (InitialSavePending) { return string.Empty; }

        if (_writer is null) { StartWriter(); }
        if (_writer is null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(KeyName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]");

        try {
            lock (_writer) {
                var line = l.ParseableItems().FinishParseable();
                _writer.WriteLine(line);

                // Eigene Änderungen ebenfalls in den Hash-Cache aufnehmen
                _processedHashes.TryAdd(line.GetMD5Hash(), default);

                if (!type.IsUnimportant()) { CanDeleteWriter = false; }
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }

        return string.Empty;
    }

    /// <summary>
    /// Überprüft und erstellt den Pfad für Fragmente.
    /// </summary>
    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        IO.CreateDirectory(FragmengtsPath());
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Fragmentdatei, falls sie keine permanenten Daten enthält.
    /// </summary>
    private void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose is not null) {
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
                IO.DeleteFile(_myFragmentsFilename, false);
            }
        }
    }

    /// <summary>
    /// Führt Aufräumarbeiten nach dem Laden von Fragmenten durch, inkl. Erstellung einer neuen Hauptdatei.
    /// </summary>
    private void DoWorkAfterLastChanges(List<string>? files, DateTime startTimeUtc) {
        if (Ending) { return; }
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }
        if (files is not { Count: >= 1 }) { return; }

        _masterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > DoComplete;

        #region Bei Bedarf neue Komplett-Tabelle erstellen

        if (_masterNeeded && AmITemporaryMaster(MasterTry, MasterUntil, true)) {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Erstelle neue Komplett-Tabelle: " + KeyName, 0);

            var f = SaveMainFile(this);

            if (!string.IsNullOrEmpty(f)) {
                Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Komplettierung von {Caption} fehlgeschlagen: {f}", 0);
                return;
            }

            CloseWriter();
            StartWriter();

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

        if (_changesNotIncluded.Count != 0) {
            foreach (var thisch in _changesNotIncluded) {
                files.Remove(thisch.Container);
            }
        }

        #endregion

        files.Shuffle();

        foreach (var thisf in files) {
            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f[^19..];

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalMinutes > DeleteFragmentsAfter &&
                         LastSaveMainFileUtcDate.Subtract(d2).TotalMinutes > DeleteFragmentsAfter) {
                        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix(), 0);
                        IO.DeleteFile(thisf, 0, false);
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
    private (List<UndoItem>? Changes, List<string>? Files, bool failed) GetLastChanges() {
        if (!string.IsNullOrEmpty(IsGenericEditable(true))) { return (null, null, true); }

        CheckPath();

        try {
            var frgma = IO.GetFiles(FragmengtsPath(), KeyName.ToUpperInvariant() + "-*." + SuffixOfFragments, System.IO.SearchOption.TopDirectoryOnly).ToList();
            frgma.Remove(_myFragmentsFilename);

            if (frgma.Count == 0) { return ([], [], false); }

            var l = new List<UndoItem>();

            foreach (var thisf in frgma) {
                var fil = IO.ReadAllText(thisf, Encoding.UTF8);

                foreach (var thist in fil.SplitAndCutByCr()) {
                    if (!thist.StartsWith('-')) {
                        var hash = thist.GetMD5Hash();

                        // Atomar als verarbeitet markieren; bei Duplikat sofort überspringen.
                        if (!_processedHashes.TryAdd(hash, default)) { continue; }

                        var u = new UndoItem(thist);

                        // Nur Änderungen übernehmen, die neuer sind als der Stand der Hauptdatei
                        if (u.DateTimeUtc <= LastSaveMainFileUtcDate) { continue; }

                        u.Container = thisf;
                        l.Add(u);
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
    /// <param name="initialload"></param>
    private OperationResult InjectData(List<string>? checkedDataFiles, List<UndoItem>? data, DateTime startTimeUtc, DateTime endTimeUtc, bool initialload) {
        if (data is null) { return OperationResult.Success; }
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed($"Tabelle nicht bearbeitbar: {f}"); }

        if (Column.ChunkValueColumn is { IsDisposed: false }) { return OperationResult.Failed("Falscher Tabellentyp"); }

        var dataSorted = data.Where(obj => obj?.DateTimeUtc is not null).OrderBy(obj => obj.DateTimeUtc);
        var affectingHead = false;

        try {
            List<string> myfiles = [];
            if (checkedDataFiles is not null) {
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
                        //Develop.Diagnose("UNDO",$"InjectData Add WAIT: cmd={thisWork.Command} row={thisWork.RowKey} T{Environment.CurrentManagedThreadId}");
                        lock (_undoLock) {
                            //Develop.Diagnose("UNDO",$"InjectData Add ENTER: Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
                            Undo.Add(thisWork);
                            //Develop.Diagnose("UNDO",$"InjectData Add DONE: Undo.Count={Undo.Count} T{Environment.CurrentManagedThreadId}");
                        }
                        _changesNotIncluded.Add(thisWork);

                        affectingHead |= (!thisWork.Command.IsCellValue() && !thisWork.Command.IsUnimportant());

                        // Undo- und UndoInOne-Einträge wurden oben bereits per Undo.Add(thisWork)
                        // in die Liste übernommen. Ein zusätzliche SetValueInternal-Aufruf würde
                        // sie doppelt eintragen (Undo) bzw. die komplette Liste verwerfen (UndoInOne).
                        if (thisWork.Command == TableDataType.Undo || thisWork.Command == TableDataType.UndoInOne) {
                            //Develop.Diagnose("UNDO",$"InjectData SKIP SetValueInternal für cmd={thisWork.Command} T{Environment.CurrentManagedThreadId}");
                            continue;
                        }

                        var c = Column[thisWork.ColName];
                        var r = Row.GetByKey(thisWork.RowKey);

                        var error = string.Empty;

                        if (initialload) {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.IgnoreFreeze | Reason.DoRepair);
                        } else {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.RaiseEvents | Reason.IgnoreFreeze | Reason.DoRepair);
                        }

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
            return InjectData(checkedDataFiles, data, startTimeUtc, endTimeUtc, initialload);
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

        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }
        CheckPath();

        var tmp = IO.TempFile(FragmengtsPath(), KeyName + "-" + Environment.MachineName + "-" + DateTime.UtcNow.ToString4(), SuffixOfFragments);

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

            var l = new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, " Dummy - systembedingt benötigt", "[Änderung in dieser Session]");
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