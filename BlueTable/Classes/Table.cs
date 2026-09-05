// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueTable.ClassesStatic;
using BlueTable.EventArgs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.IO;
using static BlueScript.Classes.Script;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Table : LiveInstanceCache<Table>, ICreateByKey<Table>, IDisposableExtended, IHasKeyName, IEditable, IJsonParseable {

    #region Fields

    public const string KeyShortSuccessMessage = "ShortSuccessMessage";

    public const string TableVersion = "4.11";

    internal readonly object _undoLock = new();

    private static List<string> _allavailableTables = [];

    private static DateTime _lastAvailableTableCheck = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly List<string> _dictionaryWords = [];

    private readonly object _eventScriptLock = new();

    private readonly List<string> _permissionGroupsNewRow = [];

    private readonly List<string> _tableAdmin = [];

    private readonly List<string> _tags = [];

    private readonly List<ScriptVariable> _variables = [];

    private string _assetFolder;

    private string _caption = string.Empty;
    private bool? _changesRowColor;
    private Timer? _checker;
    private ReadOnlyCollection<ColumnViewCollection> _columnArrangements = new([]);
    private string _createDate;
    private string _creator;
    private ReadOnlyCollection<TableScriptDescription> _eventScript = new([]);
    private DateTime _eventScriptVersion = DateTime.MinValue;
    private string _globalShowPass = string.Empty;
    private bool? _hasValueChangedScript;
    private volatile int _isDisposedFlag;

    /// <summary>
    /// Zuletzt vergebener Zeitstempel für Undo-Log-Einträge; garantiert eindeutigen Millisekunden-Abstand.
    /// </summary>
    private DateTime _lastChangeUtc = DateTime.MinValue;

    private bool? _mayAffectUser;
    private DateTime _powerEditTime = DateTime.MinValue;
    private string _rowQuickInfo = string.Empty;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    /// <summary>
    /// Zähler für SuppressEvents/ResumeEvents. Bei &gt; 0 werden keine Events ausgelöst.
    /// </summary>
    private int _suppressEvents;

    private string _symbolFolder = string.Empty;
    private string _temporaryTableMasterApp = string.Empty;

    private string _temporaryTableMasterId = string.Empty;

    private string _temporaryTableMasterMachine = string.Empty;

    private string _temporaryTableMasterTimeUtc = string.Empty;

    private string _temporaryTableMasterUser = string.Empty;

    private int _timerPaused;

    private ReadOnlyCollection<UniqueValueDefinition> _uniqueValues = new([]);

    private string _variableTmp;

    #endregion

    #region Constructors

    protected Table(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgelöst wird.
        Develop.StartService();
        lock (AllFilesLocker) {
            KeyName = BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(tablename);

            if (!IsValidTableName(KeyName)) {
                Develop.DebugError("Tabellenname ungültig: " + tablename);
            }

            Cell = new CellCollection(this);
            Row = new RowCollection(this);
            Column = new ColumnCollection(this);

            Column.ColumnDisposed += Column_ColumnChanged;
            Column.ColumnRemoving += Column_ColumnChanged;

            Undo = [];

            _creator = UserName;
            _createDate = DateTime.UtcNow.ToString9();
            LoadedVersion = TableVersion;
            _assetFolder = "Assets";
            _symbolFolder = "Symbole";
            _variableTmp = string.Empty;

            // Muss vor dem Laden der Daten in LiveInstances eingetragen werden,
            // weil interne Logik (Passwort-Logik, Filter für den Export etc.)
            // die Tabelle über das Register finden muss. Das Added-Event wird
            // nicht mehr aus dem Konstruktor gefeuert — das übernimmt
            // GetOrCreate bzw. der direkte Erzeuger.
            LiveInstances[KeyName] = this;
        }
    }

    protected Table(string tablename, Table? source) : this(tablename) {
        MainChunkLoadDone = true;
        source?.CopyTo(this);
    }

    #endregion

    #region Events

    public event EventHandler? AdditionalRepair;

    public event EventHandler<CanDoScriptEventArgs>? CanDoScript;

    public event EventHandler<CellEventArgs>? CellValueChanged;

    public event EventHandler? Disposed;

    public event EventHandler? InvalidateView;

    public event EventHandler<FirstEventArgs>? Loaded;

    public event EventHandler? Loading;

    public event EventHandler<JsonPathChangedEventArgs>? PropertyChangedExt;

    public event EventHandler? ScriptChanged;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    public event EventHandler<WriteAccessChangedEventArgs>? WriteAccessChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Eigenes Register aller lebenden TableScriptCommand-Instanzen, geordnet nach
    /// KeyName. Schlüsselseitig Case-Insensitive.
    /// </summary>
    public static List<string> ExecutingScriptThreadsAnyTable { get; } = [];

    /// <summary>
    /// In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.
    /// </summary>
    public string AssetFolder {
        get => _assetFolder;
        set {
            if (_assetFolder == value) { return; }
            _assetFolderTemp = null;
            ChangeData(TableDataType.AssetFolder, null, _assetFolder, value);
            Cell.InvalidateAllSizes();
        }
    }

    /// <summary>
    /// Der Name der Tabelle.
    /// </summary>
    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            ChangeData(TableDataType.Caption, null, _caption, value);
        }
    }

    public string CaptionForEditor => "Tabelle";

    public CellCollection Cell { get; }

    public bool ChangedScriptMayAffectUser {
        get {
            if (_mayAffectUser is { } b) { return b; }

            var l = EventScript.Get(ScriptEventTypes.value_changed);

            var a = false;

            if (l.Count == 1) {
                a = l[0].MayAffectUser;
            }

            _mayAffectUser = a;

            return a;
        }
    }

    public bool ChangesRowColor {
        get {
            if (_changesRowColor is { } b) { return b; }
            if (EventScript.Get(ScriptEventTypes.prepare_formula) is not { } sc || sc.Count != 1) {
                _changesRowColor = false;
                return false;
            }

            var t = sc[0].Script?.IndexOfWord("rowcolor", 0, RegexOptions.IgnoreCase) >= 0;
            _changesRowColor = t;
            return t;
        }
    }

    public ColumnCollection Column { get; }

    public ReadOnlyCollection<ColumnViewCollection> ColumnArrangements {
        get => _columnArrangements;
        set {
            var l = new List<ColumnViewCollection>();
            l.AddRange(value);

            var caOld = _columnArrangements.ToString(false);
            var caNew = l.ToString(false);

            if (caOld == caNew) { return; }
            ChangeData(TableDataType.ColumnArrangement, null, caOld, caNew);
            OnViewChanged();
        }
    }

    public string CreateDate {
        get => _createDate;
        private set {
            if (_createDate == value) { return; }
            ChangeData(TableDataType.CreateDateUTC, null, _createDate, value);
        }
    }

    public string Creator {
        get => _creator.Trim();
        private set {
            if (_creator == value) { return; }
            ChangeData(TableDataType.Creator, null, _creator, value);
        }
    }

    public ReadOnlyCollection<string> DictionaryWords {
        get => new(_dictionaryWords);
        set {
            if (!_dictionaryWords.IsDifferentTo(value)) { return; }
            ChangeData(TableDataType.DictionaryWords, null, string.Join('\r', _dictionaryWords), string.Join('\r', value));
        }
    }

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public ReadOnlyCollection<TableScriptDescription> EventScript {
        get {
            lock (_eventScriptLock) { return _eventScript; }
        }
        set {
            var l = new List<TableScriptDescription>();
            l.AddRange(value);
            l.Sort();

            string eventScriptOld;
            var eventScriptNew = l.ToString(false);
            lock (_eventScriptLock) {
                eventScriptOld = _eventScript.ToString(false);
            }

            if (eventScriptOld == eventScriptNew) { return; }

            // ChangeData außerhalb des Locks: feuert OnScriptChanged, dessen Subscriber
            // (UI) synchron invoken. Unter dem Lock deadlockt das mit UI-Lesern des Getters.
            ChangeData(TableDataType.EventScript, null, eventScriptOld, eventScriptNew);
        }
    }

    public DateTime EventScriptVersion {
        get => _eventScriptVersion;
        set {
            if (_eventScriptVersion == value) { return; }
            ChangeData(TableDataType.EventScriptVersion, null, _eventScriptVersion.ToString5(), value.ToString5());
        }
    }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgängig gemacht werden.
    /// Weil keine Undos mehr geladen werden, würde da nur Chaos entstehen.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            ChangeData(TableDataType.GlobalShowPass, null, _globalShowPass, value);
        }
    }

    /// <summary>
    /// info: Erweiterte Prüfung: CanDoValueChangedScript
    /// </summary>
    public bool HasValueChangedScript {
        get {
            if (_hasValueChangedScript is { } b) { return b; }

            var l = EventScript.Get(ScriptEventTypes.value_changed);

            var a = l.Count == 1;

            _hasValueChangedScript = a;
            return a;
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public bool IsEventsSuppressed => _suppressEvents > 0;

    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    public string KeyName { get; }

    public DateTime LastChange { get; private set; } = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// Wird aus dem Datei-Datum (FileInfo) der gespeicherten Datei ermittelt, nicht mehr in der Datei selbst gespeichert.
    /// </summary>
    public virtual DateTime LastSaveMainFileUtcDate => new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Wann die Tabelle zuletzt angeschaut / geöffnet / geladen wurde.
    /// Bestimmt die Reihenfolge der Reparaturen
    /// </summary>
    public DateTime LastUsedDate { get; set; } = DateTime.UtcNow;

    public bool LogUndo { get; set; } = true;

    public bool MainChunkLoadDone { get; protected set; }

    /// <summary>
    /// Pro Instanz einmalig generierter Hash aus MachineName und einer Guid.
    /// Unterscheidet verschiedene Tabellen-Instanzen — auch im selben Prozess
    /// (z.B. für den Develop-Stresstest mit mehrfach geladener gleicher Tabelle).
    /// Taucht im Dateinamen der .tblc-Dateien auf und in der Master-Id.
    /// </summary>
    public string MyId { get; } = $"{Environment.MachineName}|{Guid.NewGuid()}".GetMD5Hash()[..3].ToUpperInvariant();

    public ReadOnlyCollection<string> PermissionGroupsNewRow {
        get => new(_permissionGroupsNewRow);
        set {
            var repaired = RepairUserGroups(value).AsReadOnly();
            if (!_permissionGroupsNewRow.IsDifferentTo(repaired)) { return; }
            ChangeData(TableDataType.PermissionGroupsNewRow, null, string.Join('\r', _permissionGroupsNewRow), string.Join('\r', repaired));
        }
    }

    public bool PowerEdit {
        get => _powerEditTime.Subtract(DateTime.UtcNow).TotalSeconds > 0;

        set {
            _powerEditTime = value ? DateTime.UtcNow.AddSeconds(300) : DateTime.UtcNow.AddSeconds(-1);
            OnViewChanged();
        }
    }

    public RowCollection Row { get; }

    public string RowQuickInfo {
        get => _rowQuickInfo;
        set {
            if (_rowQuickInfo == value) { return; }
            ChangeData(TableDataType.RowQuickInfo, null, _rowQuickInfo, value);
        }
    }

    public RowSortDefinition? SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition is not null) { alt = _sortDefinition.ParseableItems().FinishParseable(); }
            if (value is not null) { neu = value.ParseableItems().FinishParseable(); }
            if (alt == neu) { return; }
            ChangeData(TableDataType.SortDefinition, null, alt, neu);

            OnSortParameterChanged();
        }
    }

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            ChangeData(TableDataType.StandardFormulaFile, null, _standardFormulaFile, value);
        }
    }

    /// <summary>
    /// In diesem Ordner suchen verschiedene Routinen (IconChar, &lt;Imagecode=...&gt;) nach eigenen Symbol-Dateien (PNG).
    /// </summary>
    public string SymbolFolder {
        get => _symbolFolder;
        set {
            if (_symbolFolder == value) { return; }
            _symbolFolderTemp = null;
            ChangeData(TableDataType.SymbolFolder, null, _symbolFolder, value);
            Cell.InvalidateAllSizes();
        }
    }

    public ReadOnlyCollection<string> TableAdmin {
        get => new(_tableAdmin);
        set {
            var repaired = RepairUserGroups(value).AsReadOnly();
            if (!_tableAdmin.IsDifferentTo(repaired)) { return; }
            ChangeData(TableDataType.TableAdminGroups, null, string.Join('\r', _tableAdmin), string.Join('\r', repaired));
        }
    }

    public ReadOnlyCollection<string> Tags {
        get => new(_tags);
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            ChangeData(TableDataType.Tags, null, string.Join('\r', _tags), string.Join('\r', value));
        }
    }

    public string TemporaryTableMasterApp {
        get => _temporaryTableMasterApp;
        set {
            if (_temporaryTableMasterApp == value) { return; }
            ChangeData(TableDataType.TemporaryTableMasterApp, null, _temporaryTableMasterApp, value);
        }
    }

    public string TemporaryTableMasterId {
        get => _temporaryTableMasterId;
        set {
            if (_temporaryTableMasterId == value) { return; }
            ChangeData(TableDataType.TemporaryTableMasterId, null, _temporaryTableMasterId, value);
        }
    }

    public string TemporaryTableMasterMachine {
        get => _temporaryTableMasterMachine;
        set {
            if (_temporaryTableMasterMachine == value) { return; }
            ChangeData(TableDataType.TemporaryTableMasterMachine, null, _temporaryTableMasterMachine, value);
        }
    }

    public string TemporaryTableMasterTimeUtc {
        get => _temporaryTableMasterTimeUtc;
        set {
            if (_temporaryTableMasterTimeUtc == value) { return; }
            ChangeData(TableDataType.TemporaryTableMasterTimeUTC, null, _temporaryTableMasterTimeUtc, value);
        }
    }

    public string TemporaryTableMasterUser {
        get => _temporaryTableMasterUser;
        set {
            if (_temporaryTableMasterUser == value) { return; }
            ChangeData(TableDataType.TemporaryTableMasterUser, null, _temporaryTableMasterUser, value);
        }
    }

    /// <summary>
    /// Wenn diese Varianble einen Count von 0 hat, ist der Speicher nicht initialisiert worden.
    /// </summary>
    public List<UndoItem> Undo { get; }

    public ReadOnlyCollection<UniqueValueDefinition> UniqueValues {
        get => _uniqueValues;
        set {
            var oldStr = string.Join('\r', _uniqueValues.Select(x => x.ParseableItems().FinishParseable()));
            var newStr = string.Join('\r', value.Select(x => x.ParseableItems().FinishParseable()).SortedDistinctList());

            if (oldStr == newStr) { return; }
            ChangeData(TableDataType.UniqueValues, null, oldStr, newStr);
        }
    }

    /// <summary>
    /// Wenn <c>true</c>, darf die Tabelle im <c>TableView</c>-Control ohne
    /// Passwortabfrage angezeigt werden. Default: <c>true</c>. Wird beim Laden
    /// auf <c>false</c> gesetzt, wenn die Tabelle verschlüsselt ist und kein
    /// Passwort eingegeben wurde. Die Abfrage erfolgt erst bei der Anzeige.
    /// </summary>
    public bool Unlocked { get; set; } = true;

    public VariableCollection Variables {
        get => [.. _variables];
        set {
            var l = value.ToStringableListVariable();
            foreach (var thisv in l) {
                thisv.ReadOnly = true; // Weil kein OnPropertyChangedEreigniss vorhanden ist
            }

            // Serialisierung VOR dem Dispose berechnen. DisposeContent kann bei
            // einigen Variablen-Typen (VariableTable, VariableRowItem) die
            // internen Referenzen nullen - danach wuerde ValueForReplace nur
            // noch Platzhalter ({TBL:?}, {ROW:?}) liefern und die Referenz
            // waere beim naechsten Laden verloren.
            var serialized = l.ToString(true);
            if (_variableTmp == serialized) { return; }

            #region Kritische Variablen Disposen

            foreach (var thisVar in _variables) {
                thisVar.DisposeContent();
            }

            #endregion

            ChangeData(TableDataType.TableVariables, null, _variableTmp, serialized);
        }
    }

    /// <summary>
    /// Gibt an, ob der Instanz-Timer dieser Tabelle pausiert ist (Zähler &gt; 0).
    /// Wird vom statischen TableFile-Update-Timer ausgewertet, damit während
    /// kritischer Bereiche (z.B. SaveInternal) kein Reload angestoßen wird.
    /// </summary>
    internal bool IsTimerPaused => _timerPaused > 0;

    protected string LoadedVersion { get; private set; }

    /// <summary>
    /// Thread-static: zusätzliche Suchpfade, die von Get(string) befüllt
    /// und von CreateInstance ausgewertet
    /// werden. Ermöglicht die Übergabe des expliziten Pfads aus dem Aufrufer-
    /// Kontext an die Factory ohne Interface-Parameter.
    /// </summary>
    [field: ThreadStatic]
    private static List<string> AdditionalSearchPathsOnThisThread => field ??= [];

    [field: ThreadStatic]
    private static Stack<Table> LoadingOnThisThread => field ??= new Stack<Table>();

    private string? _assetFolderTemp { get; set; }

    private string? _symbolFolderTemp { get; set; }

    #endregion

    #region Methods

    public static List<string> AllAvailableTables() {
        if (DateTime.UtcNow.Subtract(_lastAvailableTableCheck).TotalMinutes < 20) {
            return _allavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
        }

        _allavailableTables.Clear();

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Tabelle z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu prüfen
        var allreadychecked = new List<Table>();

        var allfiles = new List<Table>(LiveInstances.Values); // könnte sich ändern, deswegen Zwischenspeichern

        foreach (var thisTb in allfiles) {
            var possibletables = thisTb.AllAvailableTables(allreadychecked);

            allreadychecked.Add(thisTb);

            if (possibletables is not null) {
                _allavailableTables.AddRange(possibletables);
            }
        }
        _allavailableTables = _allavailableTables.SortedDistinctList();
        _lastAvailableTableCheck = DateTime.UtcNow;
        return _allavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
    }

    public static void BeSureToBeUpToDate(IReadOnlyList<Table> ofTables) {
        List<Table> l = [.. ofTables];

        foreach (var tbl in l) {
            tbl.BeSureToBeUpToDate(false);
        }
    }

    /// <summary>
    /// Factory für LiveInstanceCache{T}.GetOrCreate{TDerived}.
    /// Sucht in den konfigurierten Suchpfaden (vom Aufrufer über Get(string),
    /// plus die Pfade bereits geladener Tabellen) nach einer passenden Datei,
    /// erzeugt über Activator die passende TableFile-Subtyp-Instanz
    /// und lädt den Inhalt ohne Passwort-Abfrage. Der Konstruktor trägt die
    /// Instanz selbst in LiveInstanceCache{T}.LiveInstances ein;
    /// das LiveInstanceCache{T}.Added-Event wird von
    /// LiveInstanceCache{T}.GetOrCreate nach erfolgreicher
    /// Konstruktion gefeuert. Wirft FileNotFoundException, wenn
    /// kein passendes File existiert — Get(string) fängt das
    /// und gibt null zurück.
    /// </summary>
    public static Table Create(string key) => CreateInstance(key);

    public static void FreezeAll(string reason) {
        List<Table> snapshot;
        lock (AllFilesLocker) {
            snapshot = [.. LiveInstances.Values];
        }

        foreach (var thisFile in snapshot) {
            thisFile.Freeze(reason);
        }
    }

    public static Table Get() {
        Table t;

        lock (AllFilesLocker) {
            t = new Table(UniqueKeyValue());
        }
        t.InitDummyTable();
        OnAdded(t);
        return t;
    }

    public static Table? Get(string fileOrTableName) {
        try {
            var file = fileOrTableName;

            if (file.Contains('|')) {
                var t = file.SplitBy("|");
                var tn = string.Empty;
                var fn = string.Empty;

                foreach (var thist in t) {
                    if (string.IsNullOrEmpty(fn) && thist.IsValidFilepathAndName()) {
                        fn = thist;
                    }
                    if (string.IsNullOrEmpty(tn) && IsValidTableName(thist)) {
                        tn = thist;
                    }
                }

                if (!string.IsNullOrEmpty(fn)) {
                    file = fn;
                } else if (!string.IsNullOrEmpty(tn)) {
                    file = tn;
                }
            }

            // Suchpfade für CreateInstance übermitteln (thread-static).
            // Reset + befüllen, damit nur die Pfade des aktuellen Aufrufs gelten.
            AdditionalSearchPathsOnThisThread.Clear();

            if (file.IsValidFilepathAndName()) {
                AdditionalSearchPathsOnThisThread.AddIfNotExists(file.FilePath());
                file = file.FileNameWithoutSuffix();
            }

            file = BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(file);

            // Race-Safe über den Helper: Caching, Locking und AllowDuplicates
            // werden dort behandelt. Create (bzw. CreateInstance) macht die
            // Datei-Suche + Laden.
            return GetOrCreate<Table>(file);
        } catch {
            Develop.AbortAppIfStackOverflow();
            // Rekursion im catch entfernt, um StackOverflow bei permanenten Fehlern zu vermeiden
            return null;
        }
    }

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// Prüft bei verknüpften Zellen (RelationType.CellValues) auch die
    /// Zielzelle in der verknüpften Tabelle.
    /// </summary>
    /// <param name="column">Die Spalte</param>
    /// <param name="row">Die Zeile</param>
    /// <param name="newChunkValue">Der neue Zellwert</param>
    /// <param name="onlyTopLevel">Wenn true, wird nur die eigene Zelle geprüft (nicht die verknüpfte)</param>
    /// <returns>Leerer String bei Erfolg, ansonsten Fehlermeldung</returns>
    public static string IsCellEditable(ColumnItem? column, RowItem? row, string newChunkValue, bool onlyTopLevel) {
        if (column?.Table is not { IsDisposed: false } tb) { return "Es ist keine Spalte ausgewählt."; }

        var f = tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (row is not null) {
            f = tb.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, row.ChunkValue);
            if (!string.IsNullOrEmpty(f)) { return f; }
        } else {
            if (column.RelationType == RelationType.CellValues) {
                return "Verknüpfte Tabelle kann keine Initialzeile erstellt werden.";
            }
        }

        if (onlyTopLevel) { return string.Empty; }

        if (column.RelationType == RelationType.CellValues && row is not null) {
            var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);
            if (!string.IsNullOrEmpty(info) && !canrepair) { return info; }

            if (lcolumn?.Table is not { IsDisposed: false } tb2) { return "Verknüpfte Tabelle verworfen."; }

            tb2.PowerEdit = tb.PowerEdit;

            if (lrow is null) { return "Interner Fehler: verknüpfte Zeile nicht ermittelbar."; }

            f = IsCellEditable(lcolumn, lrow, lrow.ChunkValue, true);
            return !string.IsNullOrEmpty(f)
                ? $"Die verlinkte Zelle kann nicht bearbeitet werden: {f}"
                : string.Empty;
        }

        return string.Empty;
    }

    public static bool IsValidTableName(string tablename) {
        if (string.IsNullOrEmpty(tablename)) { return false; }

        var t = tablename.ToUpperInvariant();

        if (t.StartsWith("SYS_", StringComparison.Ordinal)) { return false; }
        if (t.StartsWith("BAK_", StringComparison.Ordinal)) { return false; }
        if (t.StartsWith("DATABASE", StringComparison.Ordinal)) { return false; }
        if (t.StartsWith("TABLE", StringComparison.Ordinal)) { return false; }

        if (tablename.IsFormat(BlueBasics.Classes.Formats.SystemNameFormat.Instance, false) is { Length: > 0 }) { return false; }

        if (t == "ALL_TAB_COLS") { return false; } // system-name

        // eigentlich 128, aber minus BAK_ und _2023_03_28
        return t.Length <= 100;
    }

    public static (int pointer, TableDataType type, string value, string colName, string rowKey) Parse(byte[] bLoaded, int pointerIn) {
        var colName = string.Empty;
        var rowKey = string.Empty;
        string value;
        TableDataType type;

        switch ((Routinen)bLoaded[pointerIn]) {
            case Routinen.CellFormatUTF8_V401: {
                    type = (TableDataType)bLoaded[pointerIn + 1];
                    var les = NummerCode3(bLoaded, pointerIn + 2);
                    rowKey = NummerCode7(bLoaded, pointerIn + 5).ToString1();
                    value = Encoding.UTF8.GetString(bLoaded, pointerIn + 12, les);
                    pointerIn += 12 + les;
                    break;
                }

            case Routinen.DatenAllgemeinUTF8: {
                    type = (TableDataType)bLoaded[pointerIn + 1];
                    var les = NummerCode3(bLoaded, pointerIn + 2);
                    rowKey = string.Empty;
                    value = Encoding.UTF8.GetString(bLoaded, pointerIn + 5, les);
                    pointerIn += 5 + les;
                    break;
                }
            case Routinen.ColumnUTF8_V401: {
                    type = (TableDataType)bLoaded[pointerIn + 1];

                    var cles = NummerCode1(bLoaded, pointerIn + 2);
                    colName = Encoding.UTF8.GetString(bLoaded, pointerIn + 3, cles);

                    var les = NummerCode3(bLoaded, pointerIn + 3 + cles);
                    value = Encoding.UTF8.GetString(bLoaded, pointerIn + 6 + cles, les);

                    pointerIn += 6 + les + cles;
                    break;
                }

            case Routinen.CellFormatUTF8_V402: {
                    type = TableDataType.UTF8Value_withoutSizeData;

                    var lengthRowKey = NummerCode1(bLoaded, pointerIn + 1);
                    rowKey = Encoding.UTF8.GetString(bLoaded, pointerIn + 2, lengthRowKey);

                    var lengthValue = NummerCode2(bLoaded, pointerIn + 2 + lengthRowKey);
                    value = Encoding.UTF8.GetString(bLoaded, pointerIn + 2 + lengthRowKey + 2, lengthValue);

                    pointerIn += 2 + lengthRowKey + 2 + lengthValue;

                    break;
                }

            case Routinen.CellFormatUTF8_V403: {
                    type = TableDataType.UTF8Value_withoutSizeData;

                    var lengthColumnKey = NummerCode1(bLoaded, pointerIn + 1);
                    colName = Encoding.UTF8.GetString(bLoaded, pointerIn + 2, lengthColumnKey);

                    var lengthRowKey = NummerCode1(bLoaded, pointerIn + 2 + lengthColumnKey);
                    rowKey = Encoding.UTF8.GetString(bLoaded, pointerIn + 3 + lengthColumnKey, lengthRowKey);

                    var lengthValue = NummerCode2(bLoaded, pointerIn + 3 + lengthRowKey + lengthColumnKey);
                    value = Encoding.UTF8.GetString(bLoaded, pointerIn + 5 + lengthRowKey + lengthColumnKey, lengthValue);

                    pointerIn += 5 + lengthRowKey + lengthValue + lengthColumnKey;

                    break;
                }

            default: {
                    type = 0;
                    value = string.Empty;
                    Develop.DebugError($"Laderoutine nicht definiert: {bLoaded[pointerIn]}");
                    break;
                }
        }

        return (pointerIn, type, value, colName, rowKey);
    }

    /// <summary>
    /// Standardisiert Benutzergruppen und eleminiert unterschiedliche Groß/Klein-Schreibweisen
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static List<string> RepairUserGroups(IEnumerable<string> e) {
        var l = new List<string>();

        e = string.Join('|', e).SplitAndCutBy("|").Distinct();
        e = string.Join('\r', e).SplitAndCutByCr().Distinct();

        foreach (var thisUser in e) {
            if (string.Equals(thisUser, Everybody, StringComparison.OrdinalIgnoreCase)) {
                l.Add(Everybody);
            } else if (string.Equals(thisUser, Administrator, StringComparison.OrdinalIgnoreCase)) {
                l.Add(Administrator);
            } else if (string.Equals(thisUser, Cli, StringComparison.OrdinalIgnoreCase)) {
                l.Add(Cli);
            } else if (string.Equals(thisUser, "#RowCreator", StringComparison.OrdinalIgnoreCase)) {
                l.Add("#RowCreator");
            } else if (thisUser.StartsWith("#USER:", StringComparison.OrdinalIgnoreCase)) {
                var th = thisUser[6..].Trim(' ');

                l.Add("#User: " + th.ToUpperInvariant());
            } else {
                l.Add(thisUser.ToUpperInvariant());
            }
        }

        return l.SortedDistinctList();
    }

    public static void SaveAll() {
        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Tabelle, "Speichere alle Tabellen", 0);

        List<Table> snapshot;
        lock (AllFilesLocker) {
            snapshot = [.. LiveInstances.Values];
        }

        Develop.EndLog($"TableScriptCommand.SaveAll: Start mit {snapshot.Count} TableScriptCommand(n)");

        var count = 0;
        Parallel.ForEach(snapshot, thisFile => {
            if (thisFile is TableFile tbf) {
                var key = tbf.KeyName;
                Develop.EndLog($"TableScriptCommand.SaveAll: Vor Save '{key}' (T{Environment.CurrentManagedThreadId})");
                Interlocked.Increment(ref count);
                tbf.Save();
                Develop.EndLog($"TableScriptCommand.SaveAll: Nach Save '{key}' (T{Environment.CurrentManagedThreadId})");
            }
        });

        Develop.EndLog($"TableScriptCommand.SaveAll: Ende, {count} TableScriptCommand(n) gespeichert");

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Häkchen, $"{count} Tabellen gespeichert", 0);
    }

    public static string UniqueKeyValue() {
        lock (AllFilesLocker) {
            var x = 9999;
            do {
                x += 1;
                if (x > 99999) { Develop.DebugError("Unique ID konnte nicht erzeugt werden"); }

                var unique = ("X" + DateTime.UtcNow.ToString("mm.fff", CultureInfo.InvariantCulture) + x.ToString5()).RemoveChars(Char_DateiSonderZeichen + " _.");
                var ok = IsValidTableName(unique) &&
                         !LiveInstances.Values.Any(thisfile => string.Equals(unique, thisfile.KeyName, StringComparison.Ordinal));

                if (ok) { return unique; }
            } while (true);
        }
    }

    public static bool UpdateScript(TableScriptDescription script, string? keyname = null, string? scriptContent = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, List<ScriptVariable>? savedVariables = null, bool isDisposed = false, bool? readOnly = null, int? stoppedtimecount = null, long? averageruntime = null) {
        if (script?.Table is not { IsDisposed: false } tb) { return false; }

        if (failedReason is null || string.IsNullOrEmpty(failedReason)) {
            savedVariables = null;
        }

        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.EventScript, string.Empty))) { return false; }

        List<TableScriptDescription> updatedScripts = [];
        lock (tb._eventScriptLock) {
            var found = false;

            foreach (var existingScript in tb._eventScript) {
                if (ReferenceEquals(existingScript, script) || existingScript.KeyName == script.KeyName && existingScript.Script == script.Script) {
                    found = true;

                    if (!isDisposed) {
                        // Prüfe ob sich wirklich etwas geändert hat
                        var hasChanges = keyname is not null && keyname != existingScript.KeyName ||
                                        scriptContent is not null && scriptContent != existingScript.Script ||
                                        image is not null && image != existingScript.Image ||
                                        quickInfo is not null && quickInfo != existingScript.QuickInfo ||
                                        adminInfo is not null && adminInfo != existingScript.AdminInfo ||
                                        eventTypes is not null && !eventTypes.Equals(existingScript.EventTypes) ||
                                        needRow is not null && needRow != existingScript.NeedRow ||
                                        readOnly is not null && readOnly != existingScript.ValuesReadOnly ||
                                        userGroups?.SequenceEqual(existingScript.UserGroups) == false ||
                                        failedReason is not null && failedReason != existingScript.FailedReason ||
                                        savedVariables is not null && savedVariables?.ToList() != existingScript.SavedVariables?.ToList() ||
                                        stoppedtimecount is not null && stoppedtimecount != existingScript.StoppedTimeCount ||
                                        averageruntime is not null && averageruntime != existingScript.AverageRunTime;

                        if (hasChanges) {
                            // Erstelle neues Script mit aktualisierten Werten
                            var newScript = new TableScriptDescription(
                                existingScript.Table,
                                keyname ?? existingScript.KeyName,
                                scriptContent ?? existingScript.Script,
                                image ?? existingScript.Image,
                                quickInfo ?? existingScript.QuickInfo,
                                adminInfo ?? existingScript.AdminInfo,
                                userGroups ?? existingScript.UserGroups,
                                eventTypes ?? existingScript.EventTypes,
                                needRow ?? existingScript.NeedRow,
                                readOnly ?? existingScript.ValuesReadOnly,
                                failedReason ?? existingScript.FailedReason,
                                savedVariables ?? existingScript.SavedVariables,
                                stoppedtimecount ?? existingScript.StoppedTimeCount,
                                averageruntime ?? existingScript.AverageRunTime
                            );
                            updatedScripts.Add(newScript);
                        } else {
                            updatedScripts.Add(existingScript);
                        }
                    }
                } else {
                    updatedScripts.Add(existingScript);
                }
            }

            if (!found) {
                updatedScripts.Add(script);
            }
        }

        // Außerhalb des Locks: der Setter ruft ChangeData -> OnScriptChanged,
        // dessen Subscriber auf den UI-Thread invoken. Unter dem Lock deadlockt das.
        tb.EventScript = updatedScripts.AsReadOnly();

        return true;
    }

    public static void WaitScriptsDone() {
        var sw = Stopwatch.StartNew();
        var runTimeID = string.Join('\r', ExecutingScriptThreadsAnyTable);

        var myThread = Environment.CurrentManagedThreadId.ToString10();

        while (HasActiveThreadsExcept(myThread)) {
            try {
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                Pause(1, true);
                var newRunTimeID = string.Join('\r', ExecutingScriptThreadsAnyTable);

                if (runTimeID != newRunTimeID) {
                    // Aktivität erkannt - Timer zurücksetzen
                    sw.Restart();
                    runTimeID = newRunTimeID;
                } else if (sw.ElapsedMilliseconds > 10 * 60 * 1000) {
                    // Nur bei Inaktivität abbrechen
                    break;
                }
            } catch { /* WaitScriptsDone: Fehler bei der Ausführung von InvalidatedRows wird ignoriert */ }
        }
    }

    public virtual string[]? AllAvailableTables(List<Table>? allreadychecked) => null;

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AssetFolderWhole() {
        if (_assetFolderTemp is not null) { return _assetFolderTemp; }

        if (!string.IsNullOrEmpty(_assetFolder)) {
            var t = _assetFolder.NormalizePath();
            if (t.IsValidFilePath()) {
                _assetFolderTemp = t;
                return t;
            }
        }

        if (this is TableFile tbf && !string.IsNullOrEmpty(tbf.Filename)) {
            var t = tbf.Filename.FilePath();

            if (!string.IsNullOrEmpty(_assetFolder)) {
                t = t + _assetFolder + "\\";
            } else {
                t = t + "Assets\\";
            }

            t = t.NormalizePath();
            if (t.IsValidFilePath()) {
                _assetFolderTemp = t;
                return t;
            }
        }
        _assetFolderTemp = string.Empty;
        return string.Empty;
    }

    public virtual OperationResult BeSureRowIsLoaded(string chunkValue) {
        var f = IsGenericEditable(false);
        return string.IsNullOrEmpty(f) ? OperationResult.Success : OperationResult.Failed(f);
    }

    public virtual bool BeSureToBeUpToDate(bool firstTime) => true;

    /// <summary>
    /// Info: TableScriptCommand.HasValueChangedScript kann schnell die Existenz Abgefragt werden
    /// </summary>
    /// <param name="notExistingValue">Der Wert, der zurückgebenen werden soll, wenn das Skript NICHT vorhanden ist</param>
    /// <returns></returns>
    public bool CanDoValueChangedScript(bool notExistingValue) => IsRowScriptPossible() && IsThisScriptOk(ScriptEventTypes.value_changed, notExistingValue);

    public string ChangeData(TableDataType command, ColumnItem? column, string previousValue, string changedTo) => ChangeData(command, column, null, previousValue, changedTo, UserName, DateTime.UtcNow, string.Empty, ChangeFlags.UserCommand);

    /// <summary>
    /// Setzt einen Wert dauerhaft (Speicher + Festplatte/Server) und führt Undo, Events und Systemspalten-Pflege aus.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="user"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    /// <param name="reason">Flags für Undo/Logging, Events und Systemspalten-Pflege.</param>
    public string ChangeData(TableDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, ChangeFlags reason) {
        if (IsDisposed) { return "Tabelle verworfen!"; }
        if (IsFreezed) { return "Tabelle eingefroren: " + FreezedReason; }
        if (type.IsObsolete()) { return "Obsoleter Befehl angekommen!"; }

        // Logische Uhr: Einmalig vor der gesamten Pipeline (Speicher, Fragment, Undo) auf eindeutigen
        // Abstand bringen, damit Fragment- und Undo-Eintrag denselben Zeitstempel erhalten.
        if (LogUndo && reason.HasFlag(ChangeFlags.LogUndo)) {
            datetimeutc = EnsureLogTimeUtc(datetimeutc);
        }

        if (row is not null) {
            var cv = row.ChunkValue;
            if (string.IsNullOrEmpty(cv) && !string.IsNullOrEmpty(changedTo)) { cv = changedTo; }
            if (IsValueEditable(TableDataType.UTF8Value_withoutSizeData, cv) is { Length: > 0 } f) { return f; }
            if (PrepareForEdit(TableDataType.UTF8Value_withoutSizeData, cv) is { Length: > 0 } df) { return df; }
        } else {
            if (IsValueEditable(type, string.Empty) is { Length: > 0 } f) { return f; }
            if (PrepareForEdit(type, string.Empty) is { Length: > 0 } df) { return df; }
        }

        var colName = column?.KeyName ?? string.Empty;

        // Bei Zellwerten die Events erst nach erfolgreichem Schreiben feuern,
        // damit ein Rollback unsichtbar bleibt (kein Trigger - Rollback - Trigger).
        var eventsAfterSuccess = type.IsCellValue() && reason.HasFlag(ChangeFlags.RaiseEvents);
        var internalFlags = eventsAfterSuccess ? reason & ~ChangeFlags.RaiseEvents : reason;

        // ERST Speicher setzen
        var error = SetValueInternal(type, column, row, changedTo, user, datetimeutc, internalFlags);
        if (!string.IsNullOrEmpty(error)) { return error; }

        // DANN Festplatte schreiben
        var f2 = WriteValueToDiscOrServer(type, changedTo, colName, row, user, datetimeutc, comment);
        if (!string.IsNullOrEmpty(f2)) {
            Develop.Message(ErrorType.Warning, this, Caption, ImageCode.Tabelle, $"Rollback aufgrund eines Fehlers:\r\n{f2}", 0);
            // Rollback: Vorherigen Wert im Speicher wiederherstellen
            SetValueInternal(type, column, row, previousValue, user, datetimeutc, ChangeFlags.IgnoreFreeze);
            return f2;
        }

        if (eventsAfterSuccess && column is not null && row is not null) {
            OnCellValueChanged(column, row, previousValue, changedTo);
        }

        // Bei Spaltenumbenennung auch ColumnArrangements aktualisieren
        if (type == TableDataType.ColumnKey && column is not null) {
            UpdateColumnArrangementsAfterRename(column);
        }

        if (LogUndo && reason.HasFlag(ChangeFlags.LogUndo)) {
            AddUndo(type, colName, row, previousValue, changedTo, user, datetimeutc, comment, "[Änderung in dieser Session]");
        }

        return string.Empty;
    }

    public string CheckScriptError() {
        foreach (var script in _eventScript) {
            if (!script.IsOk()) { return $"{script.KeyName}: {script.ErrorReason()}"; }
        }
        return string.Empty;
    }

    /// <summary>
    /// Liefert die Spalten der Tabelle in der Speicherreihenfolge:
    /// Zuerst die Spalten aus Ansicht 1 (Index 1) in deren Reihenfolge,
    /// dann die verbleibenden Spalten alphabetisch nach KeyName.
    /// </summary>
    public List<ColumnItem> ColumnsInSaveOrder() {
        var result = new List<ColumnItem>();

        if (ColumnArrangements.Count > 1) {
            foreach (var col in ColumnArrangements[1].ListOfUsedColumn()) {
                if (col is { IsDisposed: false } c && !string.IsNullOrEmpty(c.KeyName)) { result.AddIfNotExists(c); }
            }
        }

        foreach (var col in Column.OrderBy(t => t.KeyName)) {
            if (col is { IsDisposed: false } c && !string.IsNullOrEmpty(c.KeyName)) { result.AddIfNotExists(c); }
        }

        return result;
    }

    /// <summary>
    /// Stellt alle Kapitel-Spalten auf einzeilig um, indem MultiLine deaktiviert
    /// und alle \r in den Zellen durch '; ' ersetzt werden.
    /// Erforderlich, wenn die benutzerdefinierte Sortierung (SYS_ROWSORTINDEX) aktiv ist.
    /// </summary>
    public void ConvertChapterColumnsToSingleLine() {
        if (IsDisposed) { return; }

        var chapterColumns = new HashSet<ColumnItem>();
        foreach (var ca in ColumnArrangements) {
            if (ca.ColumnForChapter is { IsDisposed: false } chapterCol) {
                chapterColumns.Add(chapterCol);
            }
        }

        foreach (var chapterCol in chapterColumns) {
            chapterCol.MultiLine = false;
            foreach (var thisRow in Row) {
                if (thisRow.IsDisposed) { continue; }
                var val = thisRow.CellGetString(chapterCol);
                if (val.Contains('\r')) {
                    thisRow.CellSet(chapterCol, val.Replace("\r\n", "\r").Replace("\r", "; "), "Kapitel-Spalte durch benutzerdefinierte Sortierung auf einzeilig umgestellt");
                }
            }
        }
    }

    public void CopyTo(Table target) {
        if (IsDisposed) { return; }

        LoadTableRows(false, -1); // Statt BeSureToBeUpToDate: lädt bei TableChunk ALLE Row-Chunks

        Column.CopyTo(target.Column);
        Row.CopyTo(target.Row);

        target.Caption = Caption;
        target.GlobalShowPass = GlobalShowPass;
        target.RowQuickInfo = RowQuickInfo;
        target.StandardFormulaFile = StandardFormulaFile;
        target.AssetFolder = AssetFolder;
        target.SymbolFolder = SymbolFolder;
        target.Tags = Tags;
        target.DictionaryWords = DictionaryWords;
        target.PermissionGroupsNewRow = PermissionGroupsNewRow;
        target.TableAdmin = TableAdmin;

        target.SortDefinition = SortDefinition is not null
            ? new RowSortDefinition(target, SortDefinition.ParseableItems().FinishParseable())
            : null;

        target.UniqueValues = UniqueValues is { Count: > 0 }
            ? new ReadOnlyCollection<UniqueValueDefinition>(
                UniqueValues.Select(u => new UniqueValueDefinition(target, u.ParseableItems().FinishParseable())).ToList())
            : new([]);

        target.ColumnArrangements = ColumnArrangements;

        target.EventScript = EventScript is { Count: > 0 }
            ? new ReadOnlyCollection<TableScriptDescription>(
                EventScript.Select(s => new TableScriptDescription(target, s.ParseableItems().FinishParseable())).ToList())
            : new([]);

        target.Variables = Variables;

        target.MainChunkLoadDone = true;

        // Metadaten direkt per Feldzuweisung übernehmen - die öffentlichen Setter
        // würden ChangeData auslösen. Muss NACH den Zuweisungen oben stehen, weil
        // deren Setter-Aufrufe Undo-Einträge in target erzeugen, die hier komplett
        // durch die Undo-History der Quelle ersetzt werden.
        target._creator = _creator;
        target._createDate = _createDate;
        target._eventScriptVersion = _eventScriptVersion;
        target._powerEditTime = _powerEditTime;
        target._temporaryTableMasterUser = _temporaryTableMasterUser;
        target._temporaryTableMasterTimeUtc = _temporaryTableMasterTimeUtc;
        target._temporaryTableMasterApp = _temporaryTableMasterApp;
        target._temporaryTableMasterMachine = _temporaryTableMasterMachine;
        target._temporaryTableMasterId = _temporaryTableMasterId;

        List<UndoItem> undoSnapshot;
        lock (_undoLock) {
            undoSnapshot = [.. Undo];
        }

        lock (target._undoLock) {
            target.Undo.Clear();
            foreach (var thisUndo in undoSnapshot) {
                if (thisUndo is null) { continue; }
                target.Undo.Add(new UndoItem(thisUndo.ParseableItems().FinishParseable()));
            }
        }
    }

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool tableHeadVariables, bool virtualcolumns, bool extendedVariable, IEnumerable<FilterItem>? filter) {

        #region Variablen für Skript erstellen

        VariableCollection vars = [];

        if (row is { IsDisposed: false }) {
            foreach (var thisCol in Column) {
                var v = RowItem.CellToVariable(thisCol, row, allReadOnly, virtualcolumns);
                if (v is not null) { vars.Add(v); }
            }

            vars.Add(new RowScriptVariable("CurrentRow", row, true, "Die Zeile, mit der das Skript aufgerufen wurde."));
            vars.Add(new StringScriptVariable(KeyInputRowKey, row.KeyName, true, "Der interne Zeilenschlüssel der Zeile, mit der das Skript aufgerufen wurde."));
            vars.Add(new StringScriptVariable(KeyChunk, row.ChunkValue, true, "Der Chunk-Wert der Eingangszeile"));
        }

        if (filter is not null) {
            var num = 0;
            foreach (var thisFilter in filter) {
                vars.Add(new FilterScriptVariable($"FilterInput{num}", thisFilter, true, "Ein Eingangsfilter"));
                num++;
            }
        }

        if (tableHeadVariables) {
            foreach (var thisvar in Variables.ToStringableListVariable()) {
                // Typ der Original-Variable erhalten (Bool, Double, …),
                // damit WriteBackVariables -> Combine den Wert typsicher zurückschreiben kann.
                // NewByTypeName + GetValueFrom statt Clone: gleicher Mechanismus wie in
                // Collection.Combine, ohne Serialisierungs-Roundtrip.
                var v = ParseableItem.NewByTypeName<ScriptVariable>(thisvar.MyClassId);
                if (v is null) {
                    Develop.DebugPrint(nameof(CreateVariableCollection) + ": Typ " + thisvar.MyClassId + " konnte nicht erzeugt werden.");
                    continue;
                }
                v.KeyName = "TB_" + thisvar.KeyName;
                v.ReadOnly = false; // notwendig, damit GetValueFrom den Wert setzen darf
                _ = v.GetValueFrom(thisvar);
                v.Comment = "Tabellen-Kopf-Variable\r\n" + thisvar.Comment;
                vars.Add(v);
            }
        }

        vars.Add(new StringScriptVariable("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new StringScriptVariable("User", UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new StringScriptVariable("UserGroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        vars.Add(new BoolScriptVariable("Administrator", IsAdministrator(), true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Tabelle ist."));
        vars.Add(new StringScriptVariable("Tablename", KeyName, true, "Der aktuelle Tabellenname."));
        vars.Add(new TableScriptVariable("CurrentTable", this, true, "Die aktuelle Tabelle"));
        vars.Add(new BoolScriptVariable("ReadOnly", IsFreezed, true, "Ob die aktuelle Tabelle schreibgeschützt ist."));
        vars.Add(new DoubleScriptVariable("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Tabelle"));
        vars.Add(new StringScriptVariable("StartTimeUTC", DateTime.UtcNow.ToString7(), true, "Die Uhrzeit, wann das Skript gestartet wurde."));
        vars.Add(new RowScriptVariable("RowEmpty", null, true, "Dummy Zeile ohne Inhalt"));

        if (Column.First is { IsDisposed: false } fc) {
            vars.Add(new StringScriptVariable("NameOfFirstColumn", fc.KeyName, true, "Der Name der ersten Spalte"));

            if (row is not null) {
                vars.Add(new StringScriptVariable("ValueOfFirstColumn", row.CellGetString(fc), true, "Der Wert der ersten Spalte der Zeile als String"));
                // Andere Row Felder siehe oben
            }
        }

        //  vars.Add(new String("additionalfilespath", AssetFolderWhole(), true, "OBSOLETE: AssetFolder benutzen!")); // TODO: entfernen

        vars.Add(new StringScriptVariable("AssetFolder", AssetFolderWhole(), true, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));
        vars.Add(new BoolScriptVariable(KeyExtendend, extendedVariable, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));
        vars.Add(new ListOfStringsScriptVariable("ErrorColumns", [], true, "Spalten, die mit SetError fehlerhaft gesetzt wurden."));

        if (virtualcolumns) {
            vars.Add(new StringScriptVariable("RowColor", string.Empty, false, "Die Zeilenfarbe\r\nMuss Werte im Format RGB oder ARGB enthalten.\r\nBeispiel: #ff0000 oder #ff120320"));
        }

        #endregion

        return vars;
    }

    /// <summary>
    /// AssetFolder/Tabellepfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AssetFolderWhole())) { return AssetFolderWhole() + "Layouts\\"; }
        return string.Empty;
    }

    /// <summary>
    /// Entfernt die Systemspalte SysRowSortIndex für die benutzerdefinierte Sortierung.
    /// Die Tabellensortierung kehrt zum Standardverhalten zurück.
    /// </summary>
    public void DisableCustomSort() {
        if (IsDisposed) { return; }
        if (Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return; }

        Column.Remove(sortCol, "Benutzerdefinierte Sortierung deaktiviert");
        RepairAfterParse();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Erstellt die Systemspalte SysRowSortIndex für die benutzerdefinierte Sortierung und nummeriert
    /// alle vorhandenen Zeilen fortlaufend. Die Tabellensortierung wird anschließend
    /// fixiert auf diese Spalte (aufsteigend) gesetzt.
    /// </summary>
    public void EnableCustomSort() {
        if (IsDisposed) { return; }
        if (this is TableChunk) { return; }
        if (Column.SysRowSortIndex is { IsDisposed: false }) { return; }

        var r = SortDefinition?.SortedRows(Row) ?? [.. Row];

        Column.GenerateAndAddSystem(SystemColumnKeys.RowSortIndex);
        RepairAfterParse();

        RenumberRows(r, "Benutzerdefinierte Sortierung aktiviert");

        ConvertChapterColumnsToSingleLine();
    }

    public void EnableScript() {
        Column.GenerateAndAddSystem(SystemColumnKeys.RowState);
        Column.GenerateAndAddSystem(SystemColumnKeys.DateChanged);
        Column?.Table?.RepairAfterParse();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="script">Wenn keine TableScriptDescription ankommt, hat die Vorroutine entschieden, dass alles ok ist</param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="args"></param>
    /// <param name="tableHeadVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <param name="ignoreError"></param>
    /// <returns></returns>
    public ScriptEndedFeedback ExecuteScript(TableScriptDescription script, bool produktivphase, RowItem? row, List<string>? args, bool tableHeadVariables, bool extended, bool ignoreError) {
        // Vorab-Prüfungen
        var f = ExternalAbortScriptReason(extended);
        if (!string.IsNullOrEmpty(f) && produktivphase) { return new ScriptEndedFeedback($"Automatische Prozesse aktuell nicht möglich: {f}", false, false, script.KeyName); }

        f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new ScriptEndedFeedback($"Automatische Prozesse aktuell nicht möglich: {f}", false, false, script.KeyName); }

        if (!ignoreError && !script.IsOk()) { return new ScriptEndedFeedback($"Das Skript ist fehlerhaft: {script.ErrorReason()}", false, true, script.KeyName); }

        if (script.NeedRow && row is null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, script.KeyName); }
        if (!script.NeedRow) { row = null; }

        if (!ignoreError && row is not null && RowCollection.FailedRows.TryGetValue(row, out var reason)) {
            return new ScriptEndedFeedback($"Das Skript konnte die Zeile nicht durchrechnen: {reason}", false, false, script.KeyName);
        }

        if (produktivphase) {
            extended = extended || !script.MayAffectUser;
        }

        var isNewId = false;
        var scriptThreadId = Environment.CurrentManagedThreadId.ToString10();
        if (!script.ValuesReadOnly) {
            WaitScriptsDone();

            if (!ExecutingScriptThreadsAnyTable.Contains(scriptThreadId)) {
                ExecutingScriptThreadsAnyTable.Add(scriptThreadId);
                isNewId = true;
            }
        }

        try {
            var rowstamp = string.Empty;
            object addinfo = this;

            if (row is { IsDisposed: false }) {
                rowstamp = row.RowStamp();
                addinfo = row;
            }

            var vars = CreateVariableCollection(row, script.ValuesReadOnly, tableHeadVariables, script.VirtalColumns, extended, null);
            AddAttributes(vars, args ?? []);

            // Nur bei Skripten, die von außerhalb angestoßen werden können (Benutzergruppen vorhanden):
            // Erfolgsmeldung, die nach dem Skript statt der Standardmeldung angezeigt wird.
            if (script.UserGroups.Count > 0) {
                vars.Add(new StringScriptVariable(KeyShortSuccessMessage, string.Empty, false, "Kann im Skript gesetzt werden, um die Erfolgsmeldung zu beeinflussen.\r\nBis drei Wörter: Anzeige als QuickNote am Mauszeiger.\r\nLängere Texte: Anzeige in einer MessageBox."));
            }

            var meth = ScriptCommand.GetMethods(script.AllowedMethodsMaxLevel(extended));

            if (script.VirtalColumns) {
                meth.Add(SetErrorScriptCommand.Method);
                var gn = ScriptCommand.AllMethods.Instances.FirstOrDefault(m => m.Command == "getnote");
                if (gn is not null) { meth.Add(gn); }
            }

            #region Diagnose-Variablen bei Skript-Fehlern

            var varCount = vars.Count;
            vars.Add(new DoubleScriptVariable("AvailableMethodCount", meth.Count, true, "Anzahl der verfügbaren Methoden. Diagnose-Variable bei Skript-Fehlern, um zu prüfen, ob alles richtig geladen wurde."));
            vars.Add(new DoubleScriptVariable("AvailableVariableCount", varCount, true, "Anzahl der verfügbaren Variablen. Diagnose-Variable bei Skript-Fehlern, um zu prüfen, ob alles richtig geladen wurde."));

            #endregion

            #region Script ausführen

            var ki = Caption;

            if (row is { IsDisposed: false }) { ki = ki + "\\" + row.CellFirstString(); }

            var scp = new ScriptProperties(script.KeyName, meth, produktivphase, script.Attributes(), addinfo, script.KeyName, ki);

            var sc = new Script(vars, scp) {
                ScriptText = script.Script
            };

            AbortReason abr = extended ? ExternalAbortScriptReasonExtended : ExternalAbortScriptReason;
            var timew = Stopwatch.StartNew();
            var scf = sc.Parse(0, $"{Caption}/{script.KeyName}", abr);

            #endregion

            #region Fehlerprüfungen

            UpdateScript(script, scf, timew, row, extended, produktivphase, ignoreError);

            if (scf.Failed) { return scf; }

            if (row is not null && !script.ValuesReadOnly) {
                if (row.IsDisposed) { return new ScriptEndedFeedback("Die geprüfte Zeile wurde verworfen", false, false, script.KeyName); }
                if (Column.SysRowChangeDate is null) { return new ScriptEndedFeedback("Zeilen können nur geprüft werden, wenn Änderungen der Zeile geloggt werden.", false, false, script.KeyName); }
                if (row.RowStamp() != rowstamp) { return new ScriptEndedFeedback("Zeile wurde während des Skriptes verändert.", false, false, script.KeyName); }
            }

            #endregion

            WriteBackVariables(row, vars, script.VirtalColumns, tableHeadVariables, script.KeyName, produktivphase && !script.ValuesReadOnly);

            //  Erfolgreicher Abschluss
            // Vor dem Count-Check entfernen, damit die Prüfung korrekt ist.
            // isNewId zurücksetzen, damit das finally das Remove nicht wiederholt.
            if (isNewId) {
                ExecutingScriptThreadsAnyTable.Remove(scriptThreadId);
                isNewId = false;
            }

            if (!produktivphase) { return scf; }

            if (ExecutingScriptThreadsAnyTable.Count == 0) {
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(row, extended, null);
            }

            return scf;
        } catch (Exception ex) {
            Develop.AbortAppIfStackOverflow();
            Develop.DebugPrint("Skript-Ausführungsfehler: ", ex);
            return new ScriptEndedFeedback("Unerwarteter Fehler: " + ex.Message, false, false, script.KeyName);
        } finally {
            if (isNewId) { ExecutingScriptThreadsAnyTable.Remove(scriptThreadId); }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventname"></param>
    /// <param name="scriptname"></param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="args"></param>
    /// <param name="tbHeadVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <param name="retrySeconds">Maximale Zeit für Retry bei GiveItAnotherTry, 0 = kein Retry</param>
    /// <returns></returns>
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string? scriptname, bool produktivphase, RowItem? row, List<string>? args, bool tbHeadVariables, bool extended, float retrySeconds) {
        scriptname ??= string.Empty;

        if (eventname is not null && !string.IsNullOrWhiteSpace(scriptname)) {
            Develop.DebugError("Event und Skript angekommen!");
            return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
        }

        if (eventname is null && string.IsNullOrWhiteSpace(scriptname)) {
            return new ScriptEndedFeedback("Weder Eventname noch Skriptname angekommen", false, false, "Allgemein");
        }

        TableScriptDescription? script = null;
        if (string.IsNullOrWhiteSpace(scriptname) && eventname is { } ev) {
            if (!IsThisScriptOk(ev, true)) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

            Develop.Message(ErrorType.DevelopInfo, this, Caption, ImageCode.Tabelle, $"Ereignis ausgelöst: {eventname}", 0);

            var l = EventScript.Get(ev);

            if (l.Count == 1) {
                script = l[0];
            } else if (l.Count == 0) {
                var vars = CreateVariableCollection(row, true, tbHeadVariables, true, false, null);
                return new ScriptEndedFeedback(vars, string.Empty);
            }
        } else {
            script = EventScript.GetByKey(scriptname, StringComparison.OrdinalIgnoreCase);
        }

        if (script is null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }
        if (!script.IsOk()) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

        if (retrySeconds <= 0) {
            return ExecuteScript(script, produktivphase, row, args, tbHeadVariables, extended, false);
        }

        var startTime = DateTime.UtcNow;
        var maxAttempts = Math.Max(5, (int)(retrySeconds * 10));
        var attempt = 0;

        do {
            attempt++;
            var erg = ExecuteScript(script, produktivphase, row, args, tbHeadVariables, extended, false);

            if (!erg.Failed) { return erg; }

            if (!erg.GiveItAnotherTry || attempt >= maxAttempts || DateTime.UtcNow.Subtract(startTime).TotalSeconds > retrySeconds) {
                return erg;
            }

            Thread.Sleep(20);
        } while (true);
    }

    public string ExternalAbortScriptReason() => ExternalAbortScriptReason(false);

    public string ExternalAbortScriptReasonExtended() => ExternalAbortScriptReason(true);

    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AssetFolderWhole() + _standardFormulaFile)) { return AssetFolderWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    /// <summary>
    /// Friert die Tabelle komplett ein, nur noch Ansicht möglich.
    /// Setzt auch ReadOnly.
    /// </summary>
    /// <param name="reason"></param>
    public virtual void Freeze(string reason) {
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }

        if (!IsFreezed) {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Tabelle {KeyName} wird eingefroren: {reason}", 0);
        }

        FreezedReason = reason;

        Develop.EndLog($"Freeze '{KeyName}': Vor OnWriteAccessChanged (Grund: {reason})");
        OnWriteAccessChanged();
        Develop.EndLog($"Freeze '{KeyName}': Nach OnWriteAccessChanged");
    }

    public List<string> GetAllLayoutsFileNames() {
        List<string> path = [];
        var r = new List<string>();
        if (!IsDisposed) {
            path.Add(DefaultLayoutPath());
            if (!string.IsNullOrEmpty(AssetFolderWhole())) { path.Add(AssetFolderWhole()); }
        }

        foreach (var thisP in path) {
            if (DirectoryExists(thisP)) {
                var e = GetFiles(thisP);
                foreach (var thisFile in e) {
                    if (thisFile.FileType() is FileFormat.HTML or FileFormat.Textdocument or FileFormat.Visitenkarte or FileFormat.BlueCreativeFile or FileFormat.XMLFile) {
                        r.Add(thisFile);
                    }
                }
            }
        }
        return r;
    }

    public IJsonParseable? GetSubItemByKey(string containerName, string key) {
        if (IsDisposed) { return null; }

        if (string.Equals(containerName, "Columns", StringComparison.OrdinalIgnoreCase)) {
            return Column[key];
        }

        if (string.Equals(containerName, "Rows", StringComparison.OrdinalIgnoreCase)) {
            return Row.GetByKey(key);
        }

        if (string.Equals(containerName, "Variables", StringComparison.OrdinalIgnoreCase)) {
            return Variables.GetByKey(key);
        }

        if (string.Equals(containerName, "ColumnArrangements", StringComparison.OrdinalIgnoreCase)) {
            var idx = 0;
            if (int.TryParse(key, out var i)) { idx = i; }
            return idx >= 0 && idx < _columnArrangements.Count ? _columnArrangements[idx] : null;
        }

        return null;
    }

    public string ImportCsv(string importText, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eliminateSplitterAtStart) =>
                                            CsvHelper.ImportCsv(this, importText, zeileZuordnen, splitChar, eliminateMultipleSplitter, eliminateSplitterAtStart);

    public string ImportCsv(string importText, bool zeileZuordnen, char separator = ';', bool eliminateMultipleSplitter = false, bool eliminateSplitterAtStart = false) =>
                                    CsvHelper.ImportCsv(this, importText, zeileZuordnen, separator, eliminateMultipleSplitter, eliminateSplitterAtStart);

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_tableAdmin.Count == 0) { return false; }
        if (_tableAdmin.Contains(Everybody, StringComparer.OrdinalIgnoreCase)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _tableAdmin.Contains("#User: " + UserName, StringComparer.OrdinalIgnoreCase)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _tableAdmin.Contains(UserGroup, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Überprüft, ob ein generelles Bearbeiten eine Wertes möglich ist.
    /// Dieser Wert kann sich im Laufe der Ausführung ändern. (z.B. wenn eine Tabelle komplett geladen wurde)
    /// </summary>
    public virtual string IsGenericEditable(bool isloading) {
        if (IsDisposed) { return "Tabelle verworfen."; }
        if (IsFreezed) { return $"Tabelle eingefroren: {FreezedReason}"; }

        if (!isloading && !MainChunkLoadDone) { return "Laden noch nicht abgeschlossen"; }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(TableVersion.Replace(".", string.Empty))) {
            return $"Dieses Programm kann Tabellen nur bis Version {TableVersion} bearbeiten.";
        }

        return string.Empty;
    }

    /// <summary>
    /// Prüft, ob die Tabelle aktuell bearbeitet werden kann.
    /// </summary>
    /// <returns></returns>
    string IEditable.IsNowEditable() => IsGenericEditable(false);

    public string IsNowNewRowPossible(string? chunkValue, bool checkUserRights) {
        if (IsDisposed) { return "Tabelle verworfen"; }
        if (Column.Count == 0) { return "Keine Spalten vorhanden"; }

        if (!IsThisScriptOk(ScriptEventTypes.InitialValues, true)) { return "Skripte nicht ausführbar"; }

        if (!checkUserRights) { return string.Empty; }

        if (Column.First is not { IsDisposed: false } fc) { return "Erste Spalte nicht definiert"; }

        return IsCellEditable(fc, null, chunkValue, true);
    }

    public bool IsRowScriptPossible() {
        if (Column.SysRowChangeDate is null) { return false; }
        if (Column.SysRowState is null) { return false; }
        return string.IsNullOrEmpty(IsGenericEditable(false));
    }

    /// <summary>
    /// Prüft, ob das Skript des angegebenen Typs ausführbar (OK) ist.
    /// Info: ValueChangedScript kann schnell mit TableScriptCommand.HasValueChangedScript abgefragt werden.
    /// </summary>
    /// <param name="type">Der Skript-Ereignistyp.</param>
    /// <param name="notExistingValue">Der Wert, der zurückgegeben werden soll, wenn das Skript NICHT vorhanden ist.</param>
    /// <returns>true, wenn genau ein Skript existiert und dieses OK ist (bzw. <paramref name="notExistingValue"/> bei keinem Skript). false, wenn mehrere Skripte vorhanden sind oder das gefundene Skript defekt ist.</returns>
    public bool IsThisScriptOk(ScriptEventTypes type, bool notExistingValue) {
        var l = _eventScript.Get(type);

        if (l.Count > 1) { return false; }

        if (l.Count == 0) { return notExistingValue; }

        return l[0].IsOk();
    }

    public virtual string IsValueEditable(TableDataType type, string? chunkValue) => IsGenericEditable(false);

    /// <summary>
    /// Lädt Zeilen der Tabelle nach. Je nach Tabellentyp werden andere Funktionen unterstützt
    /// </summary>
    /// <param name="oldest">True wird versucht, die ältesten Zeilen zu laden. Im normalfall langsamer, das Stände verglichen werden müssen</param>
    /// <param name="count">Die Mindestanzahl der Zeilen zum laden. -1 für alle</param>
    /// <returns></returns>
    public virtual bool LoadTableRows(bool oldest, int count) => BeSureToBeUpToDate(false);

    public virtual void MasterMe() {
        RowCollection.WaitDelay = 0;
        TemporaryTableMasterUser = UserName;
        TemporaryTableMasterTimeUtc = DateTime.UtcNow.ToString5();
        TemporaryTableMasterApp = Develop.AppExe();
        TemporaryTableMasterMachine = Environment.MachineName;
        TemporaryTableMasterId = MyId;
    }

    public string NextRowKey() {
        if (IsDisposed) { return string.Empty; }
        var tmp = 0;
        string key;

        do {
            key = GetUniqueKey(tmp, "row");
            tmp++;
        } while (Row.GetByKey(key) is not null);
        return key;
    }

    public void OnCanDoScript(CanDoScriptEventArgs e) {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        CanDoScript?.Invoke(this, e);
    }

    public void OnInvalidateView() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        InvalidateView?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnPropertyChangedExt(string relativePath, object? value) {
        if (IsDisposed || string.IsNullOrEmpty(relativePath)) { return; }
        PropertyChangedExt?.Invoke(this, this.BuildSubItemEventArgs(relativePath, value));
    }

    public void OnScriptChanged() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        ScriptChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnViewChanged() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void Optimize() {
        if (Row.Count < 5) { return; }

        foreach (var thisColumn in Column) {
            thisColumn.Optimize();

            if (thisColumn.RelationType == RelationType.None) {
                var x = thisColumn.Contents();
                if (x.Count == 0) {
                    Column.Remove(thisColumn, "Automatische Optimierung");
                    Optimize();
                    return;
                }
            }
        }
    }

    public bool Parse(byte[] data, bool isMain, HashSet<string>? parsedRowKeys) {
        var pointer = 0;
        var columnUsed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try {
            ColumnItem? column = null;
            RowItem? row = null;
            do {
                if (pointer >= data.Length) {
                    break;
                }

                var (i, command, value, columname, rowKey) = Parse(data, pointer);
                pointer = i;

                if (!command.IsObsolete()) {

                    #region Zeile suchen oder erstellen

                    if (!string.IsNullOrEmpty(rowKey)) {
                        parsedRowKeys?.Add(rowKey);
                        row = Row.GetByKey(rowKey);
                        if (row is not { IsDisposed: false }) {
                            Row.ExecuteCommand(TableDataType.Command_AddRow, rowKey, ChangeFlags.IgnoreFreeze, null, null);
                            row = Row.GetByKey(rowKey);
                        }

                        if (row is not { IsDisposed: false }) {
                            Develop.DebugError("Zeile hinzufügen Fehler");
                            Freeze("Zeile hinzufügen Fehler");
                            return false;
                        }
                    }

                    #endregion

                    #region Spalte suchen oder erstellen

                    if (!string.IsNullOrEmpty(columname)) {
                        column = Column[columname];
                        if (command == TableDataType.ColumnKey) {
                            if (column is not { IsDisposed: false }) {
                                Column.ExecuteCommand(TableDataType.Command_AddColumnByName, columname, ChangeFlags.IgnoreFreeze);
                                column = Column[columname];
                                if (column is not { IsDisposed: false }) {
                                    Develop.DebugError("Spalte hinzufügen Fehler");
                                    Freeze("Spalte hinzufügen Fehler");
                                    return false;
                                }
                            }
                            columnUsed.Add(column.KeyName);
                        }
                    }

                    #endregion

                    #region Bei verschlüsselten Tabellen Unlocked auf false setzen

                    // Passwort wird entkoppelt: Die Tabelle lädt IMMER ohne Passwort.
                    // Bei verschlüsselten Tabellen (GlobalShowPass gesetzt) wird
                    // Unlocked = false gesetzt. Die Passwort-Abfrage erfolgt erst
                    // bei der Anzeige im TableView.
                    if (command == TableDataType.GlobalShowPass && !string.IsNullOrEmpty(value)) {
                        Unlocked = false;
                    }

                    #endregion

                    if (command == TableDataType.EOF) {
                        break;
                    }

                    var error = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, ChangeFlags.IgnoreFreeze);
                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Tabellen-Ladefehler");
                        Develop.DebugPrint("Schwerer Tabellenfehler:<br>Version: " + TableVersion + "<br>Datei: " + KeyName + "<br>Meldung: " + error);
                        return false;
                    }
                }
            } while (true);
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, "Parse Fehler", ex);
            Freeze("Parse Fehler!");
            return false;
        }

        if (isMain) {
            Column.RemoveObsoleteColumns(Column, columnUsed, ChangeFlags.IgnoreFreeze);
            Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(TableVersion.Replace(".", string.Empty))) { Freeze("Tabelleversions-Konflikt"); }

        return true;
    }

    public JsonObject ParseableJson() {
        var json = new JsonObject();
        json.Set("key", KeyName);
        json.Set("caption", _caption);
        json.Set("creator", _creator);
        json.Set("createdate", _createDate);
        json.Set("version", TableVersion);
        json.Set("assetfolder", _assetFolder);
        json.Set("symbolfolder", _symbolFolder);
        json.Set("globalshowpass", _globalShowPass);
        json.Set("rowquickinfo", _rowQuickInfo);
        json.Set("standardformulafile", _standardFormulaFile);
        json.Set("temporarytablemasterapp", _temporaryTableMasterApp);
        json.Set("temporarytablemasterid", _temporaryTableMasterId);
        json.Set("temporarytablemastermachine", _temporaryTableMasterMachine);
        json.Set("temporarytablemastertimeutc", _temporaryTableMasterTimeUtc);
        json.Set("temporarytablemasteruser", _temporaryTableMasterUser);

        json.SetArrayIfNotEmpty("tags", _tags);
        json.SetArrayIfNotEmpty("dictionarywords", _dictionaryWords);
        json.SetArrayIfNotEmpty("permissiongroupsnewrow", _permissionGroupsNewRow);
        json.SetArrayIfNotEmpty("tableadmin", _tableAdmin);

        if (SortDefinition is { } sd) { json.Set("sortdefinition", sd.ParseableJson()); }
        json.SetArrayIfNotEmpty("uniquevalues", _uniqueValues);
        json.SetArrayIfNotEmpty("eventscript", _eventScript);
        json.SetArrayIfNotEmpty("columnarrangements", _columnArrangements.Where(c => !c.IsDisposed));

        json.Set("eventscriptversion", _eventScriptVersion);
        json.Set("poweredittime", _powerEditTime);

        if (_variables is { Count: > 0 }) {
            json.Set("variables", new VariableCollection(_variables.ToList(), false).ParseableJson());
        }

        // Undo-History in der Listen-Reihenfolge serialisieren - die Speicher-
        // Reihenfolge (.bdb) sortiert selbst neu, hier darf nichts umsortiert werden.
        lock (_undoLock) {
            if (Undo.Count > 0) {
                JsonArray undoJson = [];
                foreach (var thisUndo in Undo) {
                    if (thisUndo is null) { continue; }
                    undoJson.Add(thisUndo.ParseableJson());
                }
                if (undoJson.Count > 0) { json.Set("undo", undoJson); }
            }
        }

        // Sub-Bäume (Columns, Rows, Cells) werden über die Collections serialisiert.
        // Rows werden in Speicher-Reihenfolge ausgegeben (SortDefinition bzw. KeyName),
        // damit eine deterministische Datei entsteht - siehe TableScriptCommand.RowsInSaveOrder().
        if (Column is { IsDisposed: false, Count: > 0 }) { json.Set("columns", Column.ParseableJson()["columns"]?.DeepClone()); }
        if (Row is { IsDisposed: false, Count: > 0 }) {
            JsonArray rowsJson = [];
            foreach (var row in RowsInSaveOrder()) { rowsJson.Add(row.ParseableJson()); }
            if (rowsJson.Count > 0) { json.Set("rows", rowsJson); }
        }
        if (Cell is { IsDisposed: false } && Cell.ParseableJson() is { Count: > 0 } cells) {
            foreach (var kvp in cells) { json.Set(kvp.Key, kvp.Value?.DeepClone()); }
        }

        return json;
    }

    public void ParseFinishedJson(JsonObject parsed) { }

    public void ParseJson(JsonObject json) {
        // TableScriptCommand ist über ChangeData zentral gesteuert. Diese Implementierung
        // spiegelt den Zustand wider, ohne die bestehenden Lade-Routinen zu
        // ersetzen. Sie ist rein additiv und für Partial-Updates geeignet.
        // Felder werden direkt gesetzt (analog zu ColumnItem/RowItem.ParseJson),
        // damit beim Laden/Parsen keine Platten-Schreibvorgänge oder Undo-Einträge
        // entstehen. Für echte Benutzeränderungen sind die öffentlichen Setter
        // bzw. ChangeData zu verwenden.

        if (IsDisposed) { return; }

        #region Skalare Properties

        _caption = json.GetString("caption", _caption);
        _creator = json.GetString("creator", _creator);
        _createDate = json.GetString("createdate", _createDate);
        LoadedVersion = json.GetString("version", LoadedVersion);
        _globalShowPass = json.GetString("globalshowpass", _globalShowPass);
        // Wie beim binären Laden: Eine gesetzte Passwort-Tabelle gilt als gesperrt,
        // die Abfrage erfolgt erst bei der Anzeige (bzw. wird von Skript/CLI abgelehnt).
        if (!string.IsNullOrEmpty(_globalShowPass)) { Unlocked = false; }
        _rowQuickInfo = json.GetString("rowquickinfo", _rowQuickInfo);
        _standardFormulaFile = json.GetString("standardformulafile", _standardFormulaFile);
        _temporaryTableMasterApp = json.GetString("temporarytablemasterapp", _temporaryTableMasterApp);
        _temporaryTableMasterId = json.GetString("temporarytablemasterid", _temporaryTableMasterId);
        _temporaryTableMasterMachine = json.GetString("temporarytablemastermachine", _temporaryTableMasterMachine);
        _temporaryTableMasterTimeUtc = json.GetString("temporarytablemastertimeutc", _temporaryTableMasterTimeUtc);
        _temporaryTableMasterUser = json.GetString("temporarytablemasteruser", _temporaryTableMasterUser);

        if (json["assetfolder"] is not null) {
            _assetFolder = json.GetString("assetfolder", _assetFolder);
            _assetFolderTemp = null; // Cache verwerfen, da sich der Ordner geändert haben könnte
        }

        if (json["symbolfolder"] is not null) {
            _symbolFolder = json.GetString("symbolfolder", _symbolFolder);
            _symbolFolderTemp = null; // Cache verwerfen, da sich der Ordner geändert haben könnte
            RegisterSymbolFolder();
        }

        if (json["poweredittime"] is JsonValue pe && pe.TryGetValue(out string? pes)) {
            var pdt = DateTimeParse(pes);
            if (pdt > DateTime.MinValue) { _powerEditTime = pdt; }
        }

        if (json["eventscriptversion"] is JsonValue esv && esv.TryGetValue(out string? esvs)) {
            var esvdt = DateTimeParse(esvs);
            if (esvdt > DateTime.MinValue) { _eventScriptVersion = esvdt; }
        }

        #endregion

        #region String-Listen

        if (json["tags"] is JsonArray tagsArr) {
            _tags.Clear();
            _tags.AddRange(tagsArr.ToStringList());
        }

        if (json["dictionarywords"] is JsonArray dwArr) {
            _dictionaryWords.Clear();
            _dictionaryWords.AddRange(dwArr.ToStringList());
        }

        if (json["permissiongroupsnewrow"] is JsonArray pgnrArr) {
            _permissionGroupsNewRow.Clear();
            _permissionGroupsNewRow.AddRange(RepairUserGroups(pgnrArr.ToStringList()));
        }

        if (json["tableadmin"] is JsonArray taArr) {
            _tableAdmin.Clear();
            _tableAdmin.AddRange(RepairUserGroups(taArr.ToStringList()));
        }

        #endregion

        #region Variablen

        if (json["variables"] is JsonObject varsJson) {
            var vars = new VariableCollection();
            vars.ParseJson(varsJson);
            _variables.Clear();
            _variables.AddRange(vars.SortByKeyName());
            _variableTmp = _variables.ToString(true);
        }

        #endregion

        #region Undo-History

        if (json["undo"] is JsonArray undoArr) {
            lock (_undoLock) {
                Undo.Clear();
                foreach (var item in undoArr) {
                    if (item is not JsonObject ujo) { continue; }
                    var undoItem = new UndoItem();
                    undoItem.ParseJson(ujo);
                    Undo.Add(undoItem);
                }
            }
            SeedLogTimeFromUndo();
        }

        #endregion

        #region Struktur-Sub-Bäume (Columns, Rows, Cells)

        if (json["columns"] is JsonArray) { Column?.ParseJson(json); }

        if (json["rows"] is JsonArray) { Row?.ParseJson(json); }

        Cell?.ParseJson(json);

        #endregion

        #region Definitions-Sub-Bäume (benötigen Columns)

        if (json["sortdefinition"] is JsonObject sdJson) {
            var sd = new RowSortDefinition(this, (ColumnItem?)null, false);
            sd.ParseJson(sdJson);
            _sortDefinition = sd;
        }

        if (json["uniquevalues"] is JsonArray uvArr) {
            var uvs = new List<UniqueValueDefinition>();
            foreach (var item in uvArr) {
                if (item is not JsonObject ujo) { continue; }
                var uv = new UniqueValueDefinition(this, string.Empty);
                uv.ParseJson(ujo);
                uvs.Add(uv);
            }
            _uniqueValues = uvs.AsReadOnly();
        }

        if (json["eventscript"] is JsonArray esArr) {
            var scripts = new List<TableScriptDescription>();
            foreach (var item in esArr) {
                if (item is not JsonObject ejo) { continue; }
                var script = new TableScriptDescription(this);
                script.ParseJson(ejo);
                scripts.Add(script);
            }
            scripts.Sort();
            _eventScript = scripts.AsReadOnly();
            _hasValueChangedScript = null;
            _mayAffectUser = null;
            _changesRowColor = null;
        }

        if (json["columnarrangements"] is JsonArray caArr) {
            var cas = new List<ColumnViewCollection>();
            foreach (var item in caArr) {
                if (item is not JsonObject cjo) { continue; }
                var cvc = new ColumnViewCollection(this, string.Empty);
                cvc.ParseJson(cjo);
                cas.Add(cvc);
            }
            _columnArrangements = cas.AsReadOnly();
        }

        #endregion
    }

    public bool PermissionCheck(IList<string>? allowed, RowItem? row, bool adminValue) {
        try {
            if (IsAdministrator() || PowerEdit) { return adminValue; }
            if (allowed is not { Count: not 0 }) { return false; }

            foreach (var thisString in allowed) {
                if (string.Equals(thisString, Everybody, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (Column.SysRowCreator is { IsDisposed: false } src &&
                    string.Equals(thisString, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase) &&
                    row is not null && row.CellGetString(src).Equals(UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, "#USER: " + UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, "#USER:" + UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, UserGroup, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Rechte-Check", ex);
        }
        return false;
    }

    /// <summary>
    /// Macht den Symbol-Ordner für QuickImage verfügbar, sodass <Imagecode=Name> eigene PNG-Dateien auflösen kann.
    /// </summary>
    public void RegisterSymbolFolder() {
        var p = SymbolFolderWhole();
        if (!string.IsNullOrEmpty(p)) { QuickImage.RegisterSearchPath(p); }
    }

    /// <summary>
    /// Nummeriert die übergebenen Zeilen in der übergebenen Reihenfolge fortlaufend
    /// (1, 2, 3, ...) und schreibt die Werte in die Systemspalte für die
    /// benutzerdefinierte Sortierung (ColumnCollection.SysRowSortIndex).
    /// Dispose-Zustände werden übersprungen. Ist keine Sortierspalte aktiv, ist die
    /// Methode eine No-Op. Event-Suppression muss der Aufrufer übernehmen — bei
    /// vielen Zeilen löst jedes RowItem.CellSet synchron teure
    /// Layout-Aktualisierungen aus.
    /// </summary>
    public void RenumberRows(IEnumerable<RowItem> rowsInOrder, string reason) {
        if (IsDisposed) { return; }
        if (Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return; }

        var nr = 1;
        foreach (var thisRow in rowsInOrder) {
            if (thisRow is { IsDisposed: false }) {
                thisRow.CellSet(sortCol, nr, reason);
                nr++;
            }
        }
    }

    public virtual void ReorganizeChunks() { }

    public virtual void RepairAfterParse() {
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }

        Column.Repair();

        Row.Repair();

        if (Column.SysRowSortIndex is { IsDisposed: false } sortCol) {
            SortDefinition = new RowSortDefinition(this, sortCol, false);
        } else {
            SortDefinition ??= new RowSortDefinition(this, null as ColumnItem, false);
        }

        SortDefinition?.Repair();

        foreach (var uv in _uniqueValues) { uv.Repair(); }

        // UniqueValueDefinitions ohne Spalten entfernen
        if (_uniqueValues.Any(uv => uv.KeyColumns.Count == 0)) {
            _uniqueValues = _uniqueValues.Where(uv => uv.KeyColumns.Count > 0).ToList().AsReadOnly();
        }

        PermissionGroupsNewRow = RepairUserGroups(PermissionGroupsNewRow).AsReadOnly();
        TableAdmin = RepairUserGroups(TableAdmin).AsReadOnly();

        // Bei gleichnamigen Skripten das mit dem weniger Inhalt verwerfen
        if (_eventScript.GroupBy(s => s.KeyName, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1)) {
            var deduplicated = _eventScript
                .GroupBy(s => s.KeyName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(s => s.Script.Length).First())
                .ToList();
            deduplicated.Sort();

            _hasValueChangedScript = null;
            _mayAffectUser = null;
            _changesRowColor = null;
            _eventScript = deduplicated.AsReadOnly();
        }

        OnAdditionalRepair();
    }

    /// <summary>
    /// Nimmt die Auslösung von Events wieder auf.
    /// Wenn der Zähler auf 0 zurückgeht, werden InvalidateView und ViewChanged einmalig ausgelöst,
    /// um die UI auf den neuesten Stand zu bringen.
    /// </summary>
    public void ResumeEvents() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { _suppressEvents--; }
        if (_suppressEvents > 0) { return; }

        // _suppressEvents ist jetzt 0 — On-Methoden feuern wieder.
        // Reihenfolge: Loaded first (signalisiert Daten-Reload, FilterCollection
        // und TableView bauen ihre Row-Sets neu auf), dann ViewChanged für
        // Arrangement/ViewItems, dann InvalidateView für reinen Repaint.
        OnLoaded(false, false);
        OnViewChanged();
        OnInvalidateView();
    }

    /// <summary>
    /// Liefert die nicht-disposed Zeilen dieser Tabelle in der Reihenfolge,
    /// in der sie gespeichert werden sollen. Ist eine SortDefinition
    /// vorhanden, wird diese angewendet. Andernfalls wird aufsteigend nach
    /// RowItem.KeyName sortiert (OrdinalIgnoreCase).
    /// </summary>
    /// <returns>Eine neue, sortierte Liste aller nicht-disposed Zeilen.</returns>
    public List<RowItem> RowsInSaveOrder() {
        if (IsDisposed) { return []; }

        var rows = Row.Where(r => !r.IsDisposed).ToList();

        return SortDefinition is { } sd
            ? sd.SortedRows(rows)
            : [.. rows.OrderBy(r => r.KeyName, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Unterbricht die Auslösung aller Events (außer Disposed).
    /// Mehrfache Aufrufe sind möglich und müssen durch entsprechend viele ResumeEvents-Aufrufe aufgehoben werden.
    /// </summary>
    public void SuppressEvents() {
        if (IsDisposed) { return; }
        _suppressEvents++;
    }

    /// <summary>
    /// Der komplette Symbol-Ordner-Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string SymbolFolderWhole() {
        if (_symbolFolderTemp is not null) { return _symbolFolderTemp; }

        if (!string.IsNullOrEmpty(_symbolFolder)) {
            var t = _symbolFolder.NormalizePath();
            if (t.IsValidFilePath()) {
                _symbolFolderTemp = t;
                return t;
            }
        }

        if (this is TableFile tbf && !string.IsNullOrEmpty(tbf.Filename) && !string.IsNullOrEmpty(_symbolFolder)) {
            var t = (tbf.Filename.FilePath() + _symbolFolder + "\\").NormalizePath();
            if (t.IsValidFilePath()) {
                _symbolFolderTemp = t;
                return t;
            }
        }

        _symbolFolderTemp = string.Empty;
        return string.Empty;
    }

    public override string ToString() => IsDisposed ? string.Empty : base.ToString() + " " + KeyName;

    public void UpdateScript(TableScriptDescription script, ScriptEndedFeedback scf, Stopwatch tim, RowItem? row, bool extended, bool produktivphase, bool ignoreError) {
        var failed = script.FailedReason;
        var savedVariables = script.SavedVariables;
        var runTimeCount = script.StoppedTimeCount;
        // Bereits auf 500 ms gerundet (siehe AverageRunTime-Setter in TableScriptDescription).
        var avgRunTime = script.AverageRunTime;

        if (!string.IsNullOrEmpty(scf.FailedReason)) {
            // Skript fehlgeschlagen: Es darf keine neue Laufzeit geschrieben werden,
            // da die gemessene Zeit bei einem Abbruch verfälscht ist.
            if (scf.NeedsScriptFix && !ignoreError && produktivphase) {
                if (string.IsNullOrEmpty(failed)) {
                    failed = scf.ProtocolText;
                    savedVariables = scf.Variables?.ToList();
                }
            }
        } else {
            var newStoppedTime = tim.ElapsedMilliseconds + 500; // +500 wegen Variablen zurückschreiben und so Zeugs

            if (extended || scf.Variables?.GetByKey(KeyExtendend) is null) {
                if (runTimeCount < int.MaxValue - 100) {
                    var deviation = Math.Abs(newStoppedTime - avgRunTime) / (double)avgRunTime;

                    var newt = avgRunTime;
                    if ((runTimeCount < 100 && deviation > 0.1f) || deviation > 0.3f) {
                        newt = TableScriptDescription.RoundRunTime(((avgRunTime * runTimeCount) + newStoppedTime) / (runTimeCount + 1));
                    }

                    // runTimeCount wird weitergezählt (für MayAffectUser/Anzeige), aber persistiert
                    // wird nur, wenn sich der gerundete Durchschnitt tatsächlich geändert hat.
                    if (newt != avgRunTime || runTimeCount < 25) {
                        runTimeCount++;
                        avgRunTime = newt;
                    }
                }
            }
        }

        if (row is not null && !string.IsNullOrEmpty(scf.FailedReason)) {
            RowCollection.FailedRows[row] = scf.FailedReason;
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Skript-Fehler: {scf.FailedReason}", 0);
        }

        var failedChanged = failed != script.FailedReason;
        // Persistiert wird nur, wenn sich der gerundete Durchschnitt geändert hat —
        // ein reiner Zählerstand löst keinen Fragment-Schreib mehr aus.
        var timeChanged = avgRunTime != script.AverageRunTime;

        if (failedChanged || timeChanged) {
            // Variablen dürfen nur in Kombination mit einem geänderten FailedReason gespeichert werden
            UpdateScript(script,
                failedReason: failed,
                savedVariables: savedVariables,
                stoppedtimecount: runTimeCount,
                averageruntime: avgRunTime);
        }
    }

    public bool UpdateScript(string keyName, string? newkeyname, string? script = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, List<ScriptVariable>? savedVariables = null, bool isDisposed = false, bool? readOnly = null, int? stoppedtimecount = null, long? averageruntime = null) {
        var existingScript = EventScript.GetByKey(keyName, StringComparison.OrdinalIgnoreCase);
        if (existingScript is null) { return false; }

        return UpdateScript(existingScript, newkeyname, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason, savedVariables, isDisposed, readOnly, stoppedtimecount, averageruntime);
    }

    public void WriteBackVariables(RowItem? row, VariableCollection vars, bool virtualcolumns, bool tableHeadVariables, string comment, bool doWriteBack) {
        if (doWriteBack) {
            if (row is { IsDisposed: false }) {
                foreach (var thisCol in Column) {
                    row.VariableToCell(thisCol, vars, comment);
                }
            }
            if (tableHeadVariables) {
                Variables = VariableCollection.Combine(Variables, vars, "TB_");
            }
        }

        if (virtualcolumns) {
            if (row is { IsDisposed: false } ro) {
                foreach (var thisCol in Column) {
                    if (!thisCol.SaveContent) {
                        ro.VariableToCell(thisCol, vars, comment);
                    }
                }
            }
        }
    }

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + KeyName;
        } catch { /* DevelopWarnung: Fehler beim Abrufen der Debug-Informationen wird ignoriert */ }
        Develop.DebugPrint(t);
    }

    internal virtual void OnCellValueChanged(ColumnItem column, RowItem rowItem, string previewsValue, string currentValue) {
        if (column.Relationship_to_First) { rowItem.RepairRelationText(column, previewsValue); }

        if (column.Am_A_Key_For.Count > 0) {
            foreach (var linkedColumnName in column.Am_A_Key_For) {
                if (Column[linkedColumnName] is { IsDisposed: false } thisColumn) {
                    rowItem.LinkedCellData(thisColumn, true, true);
                }
            }
        }

        if (Column.First is { IsDisposed: false } c && c == column) {
            foreach (var thisColumn in Column) {
                if (thisColumn.Relationship_to_First) {
                    rowItem.RelationTextNameChanged(thisColumn, KeyName, previewsValue, currentValue);
                }
            }
        }

        column.UcaseNamesSortedByLength = null;
        if (_suppressEvents <= 0) {
            CellValueChanged?.Invoke(this, new CellEventArgs(column, rowItem));
        }
    }

    /// <summary>
    /// Befüllt den Undo Speicher und schreibt den auch im Filesystem
    /// </summary>
    /// <param name="type"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="userName"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    /// <param name="container"></param>
    protected void AddUndo(TableDataType type, string column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == TableDataType.SystemValue) { return; }

        lock (_undoLock) {
            Undo.Add(new UndoItem(KeyName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container));
        }
    }

    protected virtual void Checker_Tick(object? state) {
        if (Ending) { return; }
        if (_timerPaused > 0) { return; }

        // Grundlegende Überprüfungen
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }

        // Script-Überprüfung
        var e = new CanDoScriptEventArgs(false);
        OnCanDoScript(e);
        if (!e.Cancel) { RowCollection.ExecuteValueChangedEvent(); }
    }

    protected void CreateWatcher() {
        _checker?.Dispose();
        _checker = null;

        if (string.IsNullOrEmpty(IsGenericEditable(true))) {
            _checker = new Timer(Checker_Tick);
            _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            LiveInstances.TryRemove(new KeyValuePair<string, Table>(KeyName, this));

            try {
                OnDisposed();
                UnregisterEvents();

                // Timer zuerst disposen
                _checker?.Dispose();
                _checker = null;

                // Dann Collections disposen
                Column.Dispose();
                Row.Dispose();

                // Listen leeren
                lock (_undoLock) {
                    Undo.Clear();
                }
                _eventScript = new ReadOnlyCollection<TableScriptDescription>([]);
                _tableAdmin.Clear();
                _permissionGroupsNewRow.Clear();
                _tags.Clear();
                _dictionaryWords.Clear();
            } catch (Exception ex) {
                Develop.DebugError("Fehler beim Dispose: " + ex.Message);
            }
        }
    }

    protected void OnAdditionalRepair() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        AdditionalRepair?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoaded(bool isFirst, bool affectingHead) {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        Loaded?.Invoke(this, new FirstEventArgs(isFirst, affectingHead));
        // Schreibzugriff kann sich durch den Ladevorgang geändert haben
        // (z.B. MainChunkLoadDone, LoadedVersion oder Chunk-Locks), deshalb erneut prüfen.
        OnWriteAccessChanged();
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnWriteAccessChanged() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        var reason = IsValueEditable(TableDataType.Command_AddColumnByName, string.Empty);
        WriteAccessChanged?.Invoke(this, new WriteAccessChangedEventArgs(string.IsNullOrEmpty(reason), reason));
    }

    protected void PauseTimer() => Interlocked.Increment(ref _timerPaused);

    /// <summary>
    /// Tiefenprüfung der Editierbarkeit auf Dateiebene (z.B. Chunk vom Laufwerk
    /// laden und Edit-Lock prüfen). Wird ausschließlich bei akuter Bearbeitungsabsicht
    /// — in ChangeData — aufgerufen, nicht bei reinen UI-Abfragen über
    /// IsValueEditable. Letztere bleibt schnell, da sie nur In-Memory-Status prüft.
    /// </summary>
    protected virtual string PrepareForEdit(TableDataType type, string? chunkValue) => string.Empty;

    protected void ResumeTimer() => Interlocked.Decrement(ref _timerPaused);

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und führt Commands aus.
    /// Es wird WriteValueToDiscOrServer aufgerufen - echtzeitbasierte Systeme können dort den Wert speichern
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="changeFlags"></param>
    /// <param name="user"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    protected string SetValueInternal(TableDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, ChangeFlags changeFlags) {
        if (IsDisposed) { return "Tabelle verworfen!"; }

        if (!changeFlags.HasFlag(ChangeFlags.IgnoreFreeze)) {
            var f = IsGenericEditable(false);
            if (!string.IsNullOrEmpty(f)) { return $"Tabelle eingefroren: {f}"; }
        }

        if (type.IsObsolete()) { return string.Empty; }

        LastChange = DateTime.UtcNow;

        if (type.IsCellValue()) {
            if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }
            if (row is null) { return string.Empty; }
            if (!column.SaveContent) { return string.Empty; }

            var previousValue = row.CellGetStringCore(column);

            if (row.CellSetInMemory(column, value) is { Length: > 0 } f) { return f; }

            row.DoSystemColumns(column, user, datetimeutc, changeFlags);

            if (changeFlags.HasFlag(ChangeFlags.RaiseEvents)) {
                tb.OnCellValueChanged(column, row, previousValue, value);
            }
            return string.Empty;
        }

        if (type.IsColumnTag()) {
            if (column is not { IsDisposed: false } || Column.IsDisposed) {
                Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, $"Wert nicht gesetzt, Spalte nicht vorhanden", 0);
                return string.Empty;
            }

            return column.SetValueInternal(type, value);
        }

        if (type.IsCommand()) {
            switch (type) {
                case TableDataType.Command_RemoveColumn:
                    if (Column[value] is not { } c) { return string.Empty; }
                    return Column.ExecuteCommand(type, c.KeyName, changeFlags);

                case TableDataType.Command_AddColumnByName:
                    return Column.ExecuteCommand(type, value, changeFlags);

                case TableDataType.Command_RemoveRow:
                    if (Row.GetByKey(value) is not { } r) { return string.Empty; }
                    return Row.ExecuteCommand(type, r.KeyName, changeFlags, user, datetimeutc).FailedReason;

                case TableDataType.Command_AddRow:
                    return Row.ExecuteCommand(type, value, changeFlags, user, datetimeutc).FailedReason;

                case TableDataType.Command_NewStart:
                    return string.Empty;

                default:
                    if (LoadedVersion == TableVersion) {
                        Freeze("Ladefehler der Tabelle");
                        if (!IsFreezed) {
                            Develop.DebugError("Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + KeyName);
                        }
                    }
                    return "Befehl unbekannt.";
            }
        }

        switch (type) {
            case TableDataType.Version:
                LoadedVersion = value.Trim();
                break;

            case TableDataType.Werbung:
                break;

            case TableDataType.Creator:
                _creator = value;
                break;

            case TableDataType.CreateDateUTC:
                _createDate = value;
                break;

            case TableDataType.TemporaryTableMasterId:
                _temporaryTableMasterId = value;
                break;

            case TableDataType.TemporaryTableMasterApp:
                _temporaryTableMasterApp = value;
                break;

            case TableDataType.TemporaryTableMasterMachine:
                _temporaryTableMasterMachine = value;
                break;

            case TableDataType.TemporaryTableMasterUser:
                _temporaryTableMasterUser = value;
                break;

            case TableDataType.TemporaryTableMasterTimeUTC:
                _temporaryTableMasterTimeUtc = value;
                break;

            case TableDataType.TableAdminGroups:
                _tableAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case TableDataType.SortDefinition:
                _sortDefinition = new RowSortDefinition(this, value);
                break;

            case TableDataType.UniqueValues: {
                    var existingByKey = new Dictionary<string, UniqueValueDefinition>(StringComparer.OrdinalIgnoreCase);
                    foreach (var uv in _uniqueValues) { existingByKey.TryAdd(uv.KeyName, uv); }

                    _uniqueValues = value.ToUpperInvariant()
                        .SplitAndCutByCr()
                        .SortedDistinctList()
                        .Select(t => {
                            var newItem = new UniqueValueDefinition(this, t);
                            if (existingByKey.Remove(newItem.KeyName, out var existing)) {
                                existing.UpdateFrom(newItem);
                                return existing;
                            }
                            return newItem;
                        })
                        .ToList()
                        .AsReadOnly();
                    break;
                }

            case TableDataType.Caption:
                _caption = value;
                break;

            case TableDataType.AssetFolder:
                _assetFolder = value;
                break;

            case TableDataType.SymbolFolder:
                _symbolFolder = value;
                RegisterSymbolFolder();
                break;

            case TableDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case TableDataType.RowQuickInfo:
                _rowQuickInfo = value;
                break;

            case TableDataType.Tags:
                _tags.Clear();
                _tags.AddRange(value.SplitAndCutByCr());

                break;

            case TableDataType.DictionaryWords:
                _dictionaryWords.Clear();
                _dictionaryWords.AddRange(value.SplitAndCutByCr());
                break;

            case TableDataType.EventScript: {
                    var parsed = value.SplitAndCutByCr().Select(t => new TableScriptDescription(this, t)).ToList();

                    // IsOnlyStatisticsUpdate muss gegen den ALTEN Stand von _eventScript
                    // gebildet werden, deshalb vor der Aktualisierung auswerten.
                    var isOnlyStatisticsUpdate = IsOnlyStatisticsUpdate(_eventScript, parsed);

                    var existingByKey = new Dictionary<string, TableScriptDescription>(StringComparer.OrdinalIgnoreCase);
                    foreach (var s in _eventScript) { existingByKey.TryAdd(s.KeyName, s); }

                    _eventScript = parsed.Select(newItem => {
                        if (existingByKey.Remove(newItem.KeyName, out var existing)) {
                            existing.UpdateFrom(newItem);
                            return existing;
                        }
                        return newItem;
                    }).ToList().AsReadOnly();

                    _hasValueChangedScript = null;
                    _mayAffectUser = null;
                    _changesRowColor = null;

                    // OnScriptChanged() ERST nach der Aktualisierung von _eventScript feuern:
                    // Subscriber (z. B. TableViewForm.UpdateScripts) lesen tb.EventScript und
                    // werten CheckScriptError/ErrorReason aus. Mit dem alten Stand wuerde die
                    // Aufgabenbox nicht den korrigierten Fehlerstatus anzeigen.
                    if (!isOnlyStatisticsUpdate) {
                        Row.InvalidateAllCheckData();
                        OnScriptChanged();
                    }

                    break;
                }

            case TableDataType.TableVariables:
                _variables.Clear();
                // SortByKeyName liefert eine neue Liste - direkt als sortierte
                // Quelle fuer AddRange verwenden. Ohne diese Sortierung waere
                // _variableTmp (CheckOrder+KeyName) nie identisch mit der
                // Serialisierung im Setter (KeyName), und jede Zuweisung wuerde
                // ueberfluessig ein ChangeData/Fragment schreiben.
                _variables.AddRange(VariableCollection.ParseVariable(value, true).SortByKeyName());
                _variableTmp = _variables.ToString(true);
                break;

            case TableDataType.ColumnArrangement:
                var cas = value.SplitAndCutByCr();
                _columnArrangements = cas.Select(t => new ColumnViewCollection(this, t)).ToList().AsReadOnly();
                break;

            case TableDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case TableDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            case TableDataType.EventScriptVersion:
                _eventScriptVersion = DateTimeParse(value);
                _hasValueChangedScript = null; // Sicherheitshalber
                break;

            case TableDataType.UndoInOne:
                lock (_undoLock) {
                    Undo.Clear();
                    var uio = value.SplitAndCutByCr();
                    for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                        var tmpWork = new UndoItem(uio[z]);
                        Undo.Add(tmpWork);
                    }
                }
                SeedLogTimeFromUndo();
                break;

            case TableDataType.CheckPoint:
                break;

            case TableDataType.Undo:
                lock (_undoLock) {
                    Undo.Add(new(value));
                }
                break;

            case TableDataType.EOF:
                return string.Empty;

            default:
                // Variable type
                if (LoadedVersion == TableVersion) {
                    Freeze("Ladefehler der Tabelle");
                    if (!IsFreezed) {
                        Develop.DebugError("Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + KeyName);
                    }
                }

                return "Datentyp unbekannt.";
        }
        return string.Empty;
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Initallladung ausgeführt wurde
    /// </summary>
    protected void WaitInitialDone() {
        if (MainChunkLoadDone) { return; }

        var t = Stopwatch.StartNew();

        var lastMessageTime = 0L;

        while (!MainChunkLoadDone) {
            Thread.Sleep(1);
            if (t.ElapsedMilliseconds > 120 * 1000) {
                Develop.Message(ErrorType.DevelopInfo, this, Caption, ImageCode.Tabelle, $"Abbruch, Tabelle {KeyName} wurde nicht richtig initialisiert", 0);
                return;
            }

            if (IsFreezed) {
                Develop.Message(ErrorType.DevelopInfo, this, Caption, ImageCode.Tabelle, $"Abbruch, Tabelle {KeyName} eingefrohren {FreezedReason}", 0);
                return;
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                Develop.Message(ErrorType.DevelopInfo, this, Caption, ImageCode.Tabelle, $"Warte auf Abschluss der Initialsierung von {KeyName}", 0);
            }
        }
    }

    protected virtual string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }
        return IsGenericEditable(false);
    }

    private static Table CreateInstance(string key) {
        // Suchpfade sammeln: explizit vom Aufrufer (thread-static) plus
        // die Pfade bereits geladener Tabellen.
        var folder = new List<string>(AdditionalSearchPathsOnThisThread);

        foreach (var thisTb in LiveInstances.Values) {
            if (thisTb is TableFile { IsDisposed: false } tbf && tbf.Filename.IsValidFilepathAndName()) {
                folder.AddIfNotExists(tbf.Filename.FilePath());
            }
        }

        foreach (var thisfolder in folder) {
            var f = thisfolder + key;

            foreach (var (suffix, type) in TableFile.LoadableFileTypes.Value) {
                var fs = f + suffix;

                if (!FileExists(fs)) { continue; }

                if (!TableFile.IsFileAllowedToLoad(fs)) {
                    // Datei wird bereits von einer anderen Instanz geladen:
                    // Rekursion auf Get, um die bestehende Instanz zurückzugeben.
                    var existingTb = Get(fs);
                    if (existingTb is not null) { return existingTb; }
                    continue;
                }

                if (Activator.CreateInstance(type, key) is not TableFile tb) {
                    throw new InvalidOperationException($"Konnte keine Instanz erzeugen für {type.Name} mit Key '{key}'.");
                }

                LoadingOnThisThread.Push(tb);
                try {
                    tb.LoadFromFile(fs, string.Empty);
                } finally {
                    LoadingOnThisThread.Pop();
                }

                return tb;
            }
        }

        throw new FileNotFoundException($"Tabelle '{key}' konnte in keinem Suchpfad gefunden werden.");
    }

    private static bool HasActiveThreadsExcept(string excludeThreadId) {
        try {
            return ExecutingScriptThreadsAnyTable.Exists(thread => thread != excludeThreadId);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return HasActiveThreadsExcept(excludeThreadId);
        }
    }

    /// <summary>
    /// Ermittelt, ob sich zwei Skript-Collections ausschließlich in den reinen
    /// Laufzeit-Statistiken (AverageRunTime, StoppedTimeCount) unterscheiden.
    /// true = nur Statistik geändert, kein Eingriff in die Zeilen-Prüfung nötig.
    /// </summary>
    private static bool IsOnlyStatisticsUpdate(ReadOnlyCollection<TableScriptDescription> oldScripts, List<TableScriptDescription> newScripts) {
        if (oldScripts.Count != newScripts.Count) { return false; }

        foreach (var ns in newScripts) {
            var match = oldScripts.FirstOrDefault(os => os.KeyName == ns.KeyName);
            if (match is null || !match.ContentEquals(ns)) { return false; }
        }
        return true;
    }

    private static int NummerCode1(byte[] b, int pointer) => b[pointer];

    private static int NummerCode2(byte[] b, int pointer) => b[pointer] * 255 + b[pointer + 1];

    private static int NummerCode3(byte[] b, int pointer) => b[pointer] * 65025 + b[pointer + 1] * 255 + b[pointer + 2];

    private static long NummerCode7(byte[] b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private void Column_ColumnChanged(object? sender, ColumnEventArgs e) {
        if (IsDisposed) { return; }
        RepairAfterParse();
    }

    /// <summary>
    /// AssetFolder/Tabellepfad mit Forms und abschließenden \
    /// </summary>
    private string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AssetFolderWhole())) { return AssetFolderWhole() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// Liefert einen logischen Zeitstempel, der mindestens eine Millisekunde nach dem vorherigen liegt,
    /// damit Undo-Log-Einträge eindeutig sortierbar bleiben.
    /// </summary>
    private DateTime EnsureLogTimeUtc(DateTime datetimeutc) {
        lock (_undoLock) {
            var t = datetimeutc;
            if (t <= _lastChangeUtc) { t = _lastChangeUtc.AddMilliseconds(1); }
            _lastChangeUtc = t;
            return t;
        }
    }

    private string ExternalAbortScriptReason(bool extended) {
        var e = new CanDoScriptEventArgs(extended);
        OnCanDoScript(e);
        return e.CancelReason;
    }

    private void InitDummyTable() {
        LogUndo = false;
        DropMessages = false;

        PauseTimer();

        OnLoading();

        BeSureToBeUpToDate(true);

        MainChunkLoadDone = true;

        RepairAfterParse();

        OnLoaded(true, true);

        CreateWatcher();

        ResumeTimer();
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        if (_suppressEvents > 0) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Setzt die logische Uhr auf den neuesten geladenen Undo-Zeitstempel, damit neue Einträge
    /// auch bei Uhrversatz (Multi-User) danach liegen.
    /// </summary>
    private void SeedLogTimeFromUndo() {
        lock (_undoLock) {
            foreach (var item in Undo) {
                if (item is null) { continue; }
                if (item.DateTimeUtc > _lastChangeUtc) { _lastChangeUtc = item.DateTimeUtc; }
            }
        }
    }

    private void UnregisterEvents() {
        try {
            // Column Events
            if (Column is not null) {
                Column.ColumnDisposed -= Column_ColumnChanged;
                Column.ColumnRemoving -= Column_ColumnChanged;
            }

            // Eigene Events auf null setzen
            AdditionalRepair = null;
            CanDoScript = null;
            Disposed = null;
            InvalidateView = null;
            Loaded = null;
            Loading = null;
            ScriptChanged = null;
            SortParameterChanged = null;
            ViewChanged = null;
            CellValueChanged = null;
            PropertyChangedExt = null;
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Abmelden der Events", ex);
        }
    }

    private void UpdateColumnArrangementsAfterRename(ColumnItem column) {
        if (_columnArrangements.Count == 0) { return; }

        foreach (var arrangement in _columnArrangements) {
            if (arrangement[column] is not null) {
                var updatedArrangements = _columnArrangements.ToString(false);
                WriteValueToDiscOrServer(TableDataType.ColumnArrangement, updatedArrangements, string.Empty, null, UserName, DateTime.UtcNow, "Automatische Aktualisierung nach Spaltenumbenennung");
                return;
            }
        }
    }

    #endregion
}