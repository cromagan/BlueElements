// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;

namespace BlueTable.Classes;

/// <summary>
/// Verwaltet Tabellenfragmente für Multi-User-Umgebungen im binären Fragment-Format
/// „Typ + Key“ (Hauptdatei .fbdb). Vom klassischen TableFragments (.mbdb, Altformat
/// mit Textzeilen) unabhängig: eigene Dateien, eigener Reader/Writer. Die
/// Komplettierung der Hauptdatei läuft über TableFile und schreibt binär.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableFragmentsNew : TableFile {

    #region Enums

    /// <summary>
    /// Pfad-Typen des binären Fragment-Formats der Änderungsdateien; spiegelt die
    /// Nummernkreise von TableDataType (Werte ≤ 230). Nur dieser Klasse bekannt.
    /// </summary>
    private enum FragmentType : byte {

        SubColumn = 90,

        SubUnique = 92,

        SubScript = 93,

        Version = 1,

        TemporaryTableMasterTimeUTC = 4,

        TemporaryTableMasterUser = 5,

        TemporaryTableMasterMachine = 6,

        TemporaryTableMasterApp = 7,

        TemporaryTableMasterId = 8,

        Creator = 10,

        CreateDateUTC = 11,

        Caption = 13,

        Tags = 18,

        GlobalShowPass = 21,

        SortDefinition = 26,

        PermissionGroupsNewRow = 31,

        ColumnArrangement = 32,

        TableAdminGroups = 38,

        EventScript = 63,

        RowQuickInfo = 66,

        AssetFolder = 68,

        StandardFormulaFile = 69,

        TableVariables = 71,

        EventScriptVersion = 72,

        UniqueValues = 82,

        DictionaryWords = 83,

        SymbolFolder = 84,

        CliRights = 85,

        ColumnKey = 100,

        ColumnCaption = 101,

        MultiLine = 102,

        ForeColor = 104,

        BackColor = 105,

        AdditionalFormatCheck = 106,

        ScriptType = 107,

        LinkedCellFilter = 108,

        SaveContent = 109,

        LineStyleLeft = 110,

        LineStyleRight = 111,

        IsKeyColumn = 112,

        ColumnQuickInfo = 113,

        MaxTextLineLength = 114,

        MaxTextLength = 115,

        FixedColumnWidth = 116,

        DropDownItems = 117,

        /// <summary>
        /// Gewicht der Spalte bei der Ähnlichkeits-Suche (Float 0-1).
        /// </summary>
        SimilarityWeight = 118,

        IsFirst = 119,

        Relationship_to_First = 120,

        DefaultRenderer = 121,

        RendererSettings = 122,

        RelationType = 123,

        Value_for_Chunk = 124,

        PermissionGroupsChangeCell = 125,

        ColumnTags = 126,

        BackgroundStyle = 127,

        AllowedChars = 135,

        ColumnAdminInfo = 137,

        ColumnSystemInfo = 138,

        EditableWithTextInput = 141,

        ShowValuesOfOtherCellsInDropdown = 144,

        SortAndRemoveDoubleAfterEdit = 145,

        IgnoreAtRowFilter = 146,

        MinTextLength = 147,

        ControlStrategy = 148,

        ControlStrategyParameter = 149,

        EditAllowedDespiteLock = 154,

        DoUcaseAfterEdit = 161,

        AutoCorrectAfterEdit = 162,

        AutoFilterJoker = 163,

        AfterEditRound = 164,

        LinkedTableTableName = 166,

        CaptionGroup1 = 174,

        CaptionGroup2 = 175,

        CaptionGroup3 = 176,

        ColumnKeyOfLinkedTable = 180,

        ColumnAlign = 186,

        RegexCheck = 187,

        SortType = 188,

        ForbiddenChars = 190,

        AutoReplaceAfterEdit = 195,

        FilterOptions = 196,

        CaptionBitmapCode = 197,

        DoOpticalTranslation = 198,

        UTF8Value_withoutSizeData = 202,

        SystemValue = 205,

        RowKey = 210,

        Command_RemoveRow = 220,

        Command_AddRow = 221,

        Command_RemoveColumn = 223,

        Command_AddColumnByName = 228,

        Command_NewStart = 229
    }

    #endregion

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
    /// Dateien, deren Änderungen noch nicht in der Hauptdatei enthalten sind.
    /// </summary>
    private readonly List<string> _filesNotIncluded = [];

    /// <summary>
    /// Hashes bereits verbuchter Items (Set-Semantik), um doppelte Verarbeitung zu verhindern.
    /// Beim ersten Laden aus der Historie des Hauptfiles befüllt; ein Hash kommt nur hinzu,
    /// nachdem das Item erfolgreich angewendet wurde. Thread-safe durch ConcurrentDictionary;
    /// Value wird nicht genutzt.
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
    private System.IO.Stream? _writer;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialisiert eine neue Instanz der TableFragmentsNew Klasse.
    /// </summary>
    /// <param name="tablename">Name der Tabelle.</param>
    public TableFragmentsNew(string tablename) : base(tablename) { }

    /// <summary>
    /// Initialisiert eine neue Instanz der TableFragmentsNew Klasse als Kopie einer bestehenden Tabelle.
    /// </summary>
    /// <param name="filename">Dateiname der neuen Tabelle.</param>
    /// <param name="source">Quelltabelle, deren Daten kopiert werden.</param>
    public TableFragmentsNew(string filename, Table? source) : base(filename, source) { }

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

            // Dedup-Grundlage ist die Historie des Hauptfiles: Items, deren Hash dort
            // steht, gelten als verbucht und werden nicht erneut angewendet. Das ist
            // korrekt, weil Daten und Historie aus demselben Snapshot gespeichert
            // werden und die Historie nur um erfolgreich angewendete Items wächst.
            _processedHashes.Clear();
            List<UndoItem> undoSnapshot;
            List<FragmentChange> blockSnapshot;
            lock (_undoLock) {
                undoSnapshot = [.. Undo];
                blockSnapshot = [.. FragmentUndos];
            }
            foreach (var u in undoSnapshot) {
                _processedHashes.TryAdd(u.Hash(), default);
            }
            foreach (var f in blockSnapshot) {
                _processedHashes.TryAdd(f.Hash(), default);
            }
        }

        if (!IsDisposed && DropMessages) { Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Fragmente von '" + KeyName + "'", 0); }

        // Zeitstempel VOR dem Lesen der Fragmente erfassen.
        // Wird als endTimeUtc an InjectData übergeben und dort als _isInCache gesetzt.
        // Konservativ: Fragmente, die während des Lesens hinzukommen, werden beim
        // nächsten Aufruf erneut gelesen - lieber doppelt als verloren.
        var readStartedUtc = DateTime.UtcNow;

        var (changes, files, failed) = GetLastChanges();
        if (failed) { return false; }

        var opr = InjectData(files, changes, DateTime.UtcNow, readStartedUtc, firstTime);
        return opr.IsSuccessful;
    }

    /// <summary>
    /// Bereitet die Fortführung einer sauber abgeschlossenen Fragment-Datei vor:
    /// Ordner- und Existenzprüfung, Writer-Übernahme und Kennzeichnung als eigene Datei.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    public string ContinueFragment(string fragmentFilename) {
        if (!fragmentFilename.StartsWith(FragmengtsPath(), StringComparison.OrdinalIgnoreCase)) { return "Die Datei liegt nicht im Fragment-Ordner der Tabelle: " + fragmentFilename; }
        if (!IO.FileExists(fragmentFilename)) { return "Fragment-Datei nicht gefunden: " + fragmentFilename; }

        // Ggf. bereits offenen Writer schließen (z. B. Master-Eintrag beim Laden):
        // Diese (fast leere) Neue-Datei darf das Aufräumen dann entsorgen.
        CloseWriter();

        // Ab sofort als eigene Datei führen: hält sie beim Nachladen aus den
        // Fremd-Fragmenten heraus und schützt sie vor Aufräum-Löschungen.
        _myFragmentsFilename = fragmentFilename;
        CanDeleteWriter = false;

        // Satz 254 (ContinueEOF) anhängen: Die Datei endet nicht mehr auf EOF,
        // daher wählt sie kein zweiter Prozess zur Fortführung.
        var error = ContinueFragment(fragmentFilename, out var writer);
        _writer = writer;
        return error;
    }

    /// <summary>
    /// Öffnet die Fragment-Datei im Append-Modus und hängt sofort einen 254-Satz
    /// (ContinueEOF) an, damit kein zweiter Prozess Schreibzugriff auf dieselbe Datei erhält.
    /// Liefert bei Problemen die Fehlermeldung, sonst leer.
    /// </summary>
    public static string ContinueFragment(string fragmentFilename, out System.IO.Stream? writer) {
        writer = null;
        if (Develop.AllReadOnly) { return string.Empty; }

        System.IO.FileStream? fileStream = null;
        try {
            fileStream = new System.IO.FileStream(fragmentFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            writer = fileStream;
            fileStream = null;

            _ = FragmentRecord.Append(writer, 1, FragmentTagType.ContinueEof, string.Empty);
            writer.Flush();

            return string.Empty;
        } catch (Exception ex) {
            fileStream?.Dispose();
            writer?.Dispose();
            writer = null;

            return ex.Message;
        }
    }

    /// <summary>
    /// Gibt den Pfad zum Fragment-Ordner zurück.
    /// </summary>
    public string FragmengtsPath() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + "Frgm\\";

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
    /// Gibt die Ressourcen frei. CloseWriter läuft nach base.Dispose, damit die
    /// von UnMasterMe geschriebenen Sätze in dieselbe, sauber mit 255 abgeschlossene
    /// Fragment-Datei landen und kein Stream-Handle offen bleibt.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        base.Dispose(disposing);

        if (disposing) {
            CloseWriter();
        }
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
    /// Kein eigener Schreibweg mehr: TableFragmentsNew schreibt ausschließlich die
    /// bubbelnden FragmentChanged-Events (siehe OnFragmentChanged); das klassische
    /// UndoItem-Schreiben bleibt im Legacy-TableFragments.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string previousValue, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) => base.WriteValueToDiscOrServer(type, previousValue, value, column, row, user, datetimeutc, comment);

    /// <summary>
    /// Reicht das Root-Fragment-Event an die Basis weiter und übergibt die Änderung
    /// dem Writer: Als Crumb-Kette (RelativePath) plus Blatt-Satz mit Meta in die
    /// eigene Fragmentdatei.
    /// </summary>
    public override void OnFragmentChanged(FragmentChangedEventArgs e) {
        base.OnFragmentChanged(e);

        WriteFragmentChange(e);
    }

    /// <summary>
    /// Schreibt eine bubbelnde Fragment-Änderung in die Fragmentdatei. Beim Laden,
    /// beim Einspielen fremder Fragmente und im Nur-Lese-Modus still.
    /// </summary>
    private void WriteFragmentChange(FragmentChangedEventArgs e) {
        if (IsDisposed || Ending) { return; }
        if (Develop.AllReadOnly) { return; }
        if (InitialSavePending) { return; }
        if (_doingChanges > 0) { return; }

        var crumbs = e.CrumbPath();

        // Werte ohne Fragment-Pendant (obsolete Typen, Werbung, Undo/CheckPoint/EOF)
        // erzeugen keinen Satz — wie bisher beim UndoItem-Schreibweg.
        if (crumbs.Count == 0 && !Enum.IsDefined(typeof(FragmentType), e.TypeSelector)) { return; }

        if (_writer is null) { StartWriter(); }
        if (_writer is null) { return; }

        var user = UserName;
        var datetimeUtc = DateTime.UtcNow;

        var blockType = BlockCrumbType(crumbs);

        try {
            byte[]? blockBytes = null;

            lock (_writer) {
                // Block-Änderungen (Sub*-Typen 92-96) zuerst in den Puffer schreiben:
                // Die Bytes gehen identisch in die Fragmentdatei und als Block-Undo in
                // das Hauptfile, damit der Dedup-Hash beim Einlesen übereinstimmt.
                using var buffer = blockType != 0 ? new System.IO.MemoryStream() : null;
                var target = buffer ?? _writer;
                var depth = 1;

                foreach (var (crumbType, crumbKey) in crumbs) {
                    if (!FragmentRecord.Append(target, depth++, crumbType, crumbKey)) {
                        Freeze("Fragment-Navigationssatz ungültig: " + e.RelativePath);
                        return;
                    }
                }

                if (e.ListContent is { } list) {
                    if (!FragmentRecord.Append(target, depth, e.TypeSelector, FragmentDataType.ListSet, list, user, datetimeUtc, string.Empty)) {
                        Freeze("Fragment-Listen-Satz ungültig oder zu groß: " + e.TypeSelector);
                        return;
                    }
                } else if (!FragmentRecord.Append(target, depth, e.TypeSelector, e.ContentType, e.Content, user, datetimeUtc, string.Empty)) {
                    Freeze("Fragment-Satz ungültig oder zu groß: " + e.TypeSelector);
                    return;
                }

                if (buffer is not null) {
                    blockBytes = buffer.ToArray();
                    _writer.Write(blockBytes, 0, blockBytes.Length);
                }

                _writer.Flush();
                CanDeleteWriter = false;
            }

            if (blockBytes is not null) {
                // Block-Änderung als Undo-Eintrag merken: Sie läuft nicht durch die
                // klassische Änderungs-Pipeline und hat daher keinen UndoItem-Eintrag.
                var entry = new FragmentChange(KeyName, blockType, blockBytes, user, datetimeUtc, "[Änderung in dieser Session]");

                lock (_undoLock) {
                    FragmentUndos.Add(entry);
                }

                _processedHashes.TryAdd(entry.Hash(), default);
            } else {
                // Klassische Änderung: Dedup über die mit UndoItem geteilte Formel.
                _processedHashes.TryAdd(OwnFragmentHash(e, crumbs, user, datetimeUtc), default);
            }
        } catch {
            Freeze("Netzwerkfehler!");
        }
    }

    /// <summary>
    /// Bildet den Dedup-Hash einer selbst geschriebenen klassischen Änderung —
    /// identisch zu UndoItem.Hash(). Block-Änderungen dedupen über FragmentChange.Hash().
    /// </summary>
    private string OwnFragmentHash(FragmentChangedEventArgs e, List<(byte CrumbType, string CrumbKey)> crumbs, string user, DateTime datetimeUtc) {
        string rowKey = string.Empty;
        string colName = string.Empty;

        foreach (var (crumbType, crumbKey) in crumbs) {
            if (crumbType == (byte)FragmentType.RowKey) {
                rowKey = crumbKey;
                colName = string.Empty;
            } else if (crumbType == (byte)FragmentType.SubColumn) {
                colName = crumbKey;
            }
        }

        // Listen-Inhalte in die CR-verbundene Domänen-Form bringen, damit der Hash
        // mit den aus UndoItem.Hash() gelesenen Einträgen übereinstimmt.
        var content = e.ListContent is { } list ? string.Join('\r', list) : e.Content;

        return (KeyName + "|" + (int)(TableDataType)e.TypeSelector + "|" + colName + "|" + rowKey + "|" +
                datetimeUtc.ToString9() + "|" + user + "|" + content).GetMD5Hash();
    }

    /// <summary>
    /// Liefert das Crumb-Byte, das einen Sub*-Block eröffnet (92-96); 0 bei klassischen Pfaden.
    /// </summary>
    private static byte BlockCrumbType(List<(byte CrumbType, string CrumbKey)> crumbs) {
        foreach (var (crumbType, _) in crumbs) {
            if (IsSubBlockCrumb(crumbType)) { return crumbType; }
        }

        return 0;
    }

    /// <summary>
    /// True, wenn das Crumb-Byte einen Sub*-Block eröffnet (92-96); dessen Blätter
    /// schreibt der Writer direkt in den Block, ein Dedup über klassische Sätze
    /// entfällt.
    /// </summary>
    private static bool IsSubBlockCrumb(byte crumbType) => crumbType is >= 92 and <= 96;

    /// <summary>
    /// Überprüft und erstellt den Pfad für Fragmente.
    /// </summary>
    private void CheckPath() {
        if (string.IsNullOrEmpty(Filename)) { return; }
        IO.CreateDirectory(FragmengtsPath());
    }

    /// <summary>
    /// Schließt den aktuellen Writer und löscht die Fragmentdatei, falls sie keine permanenten Daten enthält.
    /// Zuvor werden die Steuer-Sätze 251 (User), 250 (Datum/Uhrzeit), 249 (Machine) und 255 (EOF) geschrieben.
    /// </summary>
    private void CloseWriter() {
        var writerToClose = _writer;
        if (writerToClose is not null) {
            _writer = null;

            try {
                _ = FragmentRecord.Append(writerToClose, 1, FragmentTagType.Eof, string.Empty);
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

            var f = SaveFullFile(this);

            if (!string.IsNullOrEmpty(f)) {
                Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Komplettierung von {Caption} fehlgeschlagen: {f}", 0);
                return;
            }

            CloseWriter();
            StartWriter();

            MasterMe();

            _masterNeeded = false;
            OnInvalidateView();
            _filesNotIncluded.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > AbortFragmentDeletion) { return; }

        #region Dateien schützen, deren Änderungen noch nicht im Hauptfile stehen

        // Zusätzliche Safety zur LastWriteTime-Prüfung unten: Dateien, die in diesem
        // Durchlauf gelesen wurden und deren Änderungen noch nicht im Hauptfile stehen
        // (_filesNotIncluded), werden explizit aus der Lösch-Liste entfernt.
        // Nach einer erfolgreichen Komplettierung ist _filesNotIncluded geleert -
        // dann greift ausschließlich die LastWriteTime-Prüfung.
        if (_filesNotIncluded.Count != 0) {
            foreach (var thisFile in _filesNotIncluded) {
                files.Remove(thisFile);
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
    /// Ermittelt die neuesten Änderungen aus den Fragmentdateien.
    /// </summary>
    private (List<FragmentChange>? Changes, List<string>? Files, bool failed) GetLastChanges() {
        if (!string.IsNullOrEmpty(IsGenericEditable(true))) { return (null, null, true); }

        CheckPath();

        try {
            var frgma = IO.GetFiles(FragmengtsPath(), KeyName.ToUpperInvariant() + "-*." + SuffixOfFragments, System.IO.SearchOption.TopDirectoryOnly).ToList();
            frgma.Remove(_myFragmentsFilename);

            if (frgma.Count == 0) { return ([], [], false); }

            var l = new List<FragmentChange>();

            foreach (var thisf in frgma) {
                if (IO.ReadAllBytes(thisf, 10).Value is not byte[] data) { return (null, null, true); }

                foreach (var u in ParseFragmentData(data, thisf)) {
                    // Zeilen, die sicher vor der letzten Hauptdatei-Speicherung lagen,
                    // sind in deren Komplett-Stand (Datenteil) bereits enthalten -
                    // unabhängig davon, ob ihr Hash in der Historie steht. Diese wird
                    // beim Speichern gefiltert und gekürzt (max. 1000 Items, max. 10
                    // EventScript), ist also keine vollständige Verbuchungsgrundlage.
                    // Die Toleranz überbrückt das Zeitfenster zwischen dem Lesen der
                    // Fragmente und dem Speichern der Komplettierung.
                    if (u.DateTimeUtc.AddSeconds(60) < LastSaveMainFileUtcDate) { continue; }

                    // Hash auf Basis der geparsten Werte berechnen (nicht auf der rohen Zeile),
                    // damit der Hash unabhängig vom Serialisationsformat ist und mit den aus
                    // dem Hauptfile-Undo gebildeten Hashes (in BeSureToBeUpToDate) übereinstimmt.
                    // Atomar als verarbeitet markieren; bei Duplikat sofort überspringen.
                    if (!_processedHashes.TryAdd(u.Hash(), default)) { continue; }

                    l.Add(u);
                }
            }

            return (l, frgma, false);
        } catch { }
        return (null, null, true);
    }

    /// <summary>
    /// Liest eine Fragment-Datei als FragmentChange-Liste: klassische Änderungen und
    /// ganze Sub*-Blöcke; Steuer-Sätze werden übersprungen.
    /// </summary>
    private List<FragmentChange> ParseFragmentData(byte[] data, string filename) => FragmentChange.ReadFragments(data, KeyName, filename);

    /// <summary>
    /// Injiziert die geladenen Fragmentdaten in die aktuelle Tabellenstruktur.
    /// </summary>
    /// <param name="checkedDataFiles"></param>
    /// <param name="data"></param>
    /// <param name="startTimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    /// <param name="initialload"></param>
    private OperationResult InjectData(List<string>? checkedDataFiles, List<FragmentChange>? data, DateTime startTimeUtc, DateTime endTimeUtc, bool initialload) {
        if (data is null) { return OperationResult.Success; }

        // Bei initialload die MainChunkLoadDone-Sperre umgehen - die Tabelle
        // befindet sich gerade im ersten Ladevorgang (IsGenericEditable prüft
        // mit isloading=true). Freeze- und Prozess-Prüfungen bleiben aktiv.
        var f = IsGenericEditable(initialload);
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
            // Während des Einspielens sind Spaltenschlüssel kurzzeitig nicht
            // auflösbar. In diesem Fenster dürfen Ansichten keine Einträge
            // endgültig entfernen (RepairArrangements), sonst verschwinden
            // Spalten dauerhaft aus der Ansicht.
            PauseDataReload();
            try {
                foreach (var thisWork in dataSorted) {
                    if (KeyName == thisWork.TableName) {
                        // Sub*-Blöcke als Fragment-Baum anwenden; zusätzlich als Block-Undo
                        // übernehmen, damit der Undo-Seed des Hauptfiles sie nach der
                        // Komplettierung wiedererkennt. Schutz vor dem Aufräumen, solange
                        // der Block nicht in der Hauptdatei komplettiert ist.
                        if (thisWork.Block is { Length: > 0 } block) {
                            lock (_undoLock) {
                                FragmentUndos.Add(thisWork);
                            }

                            _filesNotIncluded.Add(thisWork.Container);
                            affectingHead = true;

                            if (!ApplyFragmentBlock(block)) {
                                Freeze("Fragment-Block ungültig: " + thisWork.BlockType);
                                return OperationResult.Failed("Fragment-Block ungültig");
                            }

                            continue;
                        }

                        lock (_undoLock) {
                            Undo.Add(new UndoItem(thisWork.TableName, thisWork.Command, thisWork.ColName, thisWork.RowKey, string.Empty, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, thisWork.Comment, thisWork.Container));
                        }
                        _filesNotIncluded.Add(thisWork.Container);

                        affectingHead |= (!thisWork.Command.IsCellValue() && !thisWork.Command.IsUnimportant());

                        // Undo- und UndoInOne-Einträge wurden oben bereits per Undo.Add(thisWork)
                        // in die Liste übernommen. Ein zusätzliche SetValueInternal-Aufruf würde
                        // sie doppelt eintragen (Undo) bzw. die komplette Liste verwerfen (UndoInOne).
                        if (thisWork.Command == TableDataType.Undo) { continue; }

                        var c = Column[thisWork.ColName];
                        var r = Row.GetByKey(thisWork.RowKey);

                        var error = string.Empty;

                        if (initialload) {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, thisWork.Comment, ChangeFlags.IgnoreFreeze | ChangeFlags.PostProcess);
                        } else {
                            error = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, thisWork.Comment, ChangeFlags.RaiseEvents | ChangeFlags.IgnoreFreeze | ChangeFlags.PostProcess);
                        }

                        if (!string.IsNullOrEmpty(error)) {
                            // Bewusst NICHT in Undo/_filesNotIncluded aufnehmen: Ein
                            // nicht anwendbares Item darf weder in der Historie noch als
                            // "erledigt" landen, sonst stuft ein späteres Seeding es
                            // fälschlich als "im Hauptfile enthalten" ein und überspringt
                            // es dauerhaft (ursprüngliche Ursache des Script-Verlusts).
                            Freeze("Tabellen-Fehler: " + error + " " + thisWork.Command + " " + thisWork.ColName + " " + thisWork.RowKey + " " + thisWork.ChangedTo);
                            return OperationResult.Failed(error);
                        }
                    }
                }
                _isInCache = endTimeUtc;
            } finally {
                ResumeDataReload();
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
    /// Initialisiert den Stream für die Fragmentdatei und schreibt die Kopf-Sätze
    /// 232 (SerializerVersion), 233 (User), 235 (Machine) sowie den NewStart-Satz.
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
            _writer = fileStream;
            fileStream = null;

            _ = FragmentRecord.Append(_writer, 1, FragmentTagType.SerializerVersion, TableVersion);
            _ = FragmentRecord.Append(_writer, 1, FragmentTagType.User, UserName);
            _ = FragmentRecord.Append(_writer, 1, FragmentTagType.Machine, Environment.MachineName);

            var l = new FragmentChange(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, _myFragmentsFilename.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, "Dummy - systembedingt benötigt", "[Änderung in dieser Session]");
            _ = FragmentChange.AppendFragment(_writer, 1, l);

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