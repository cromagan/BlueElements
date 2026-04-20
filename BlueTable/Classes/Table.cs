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
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.AdditionalScriptVariables;
using BlueTable.ClassesStatic;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using static BlueBasics.Extensions;
using static BlueScript.Classes.Script;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Table : IDisposableExtendedWithEvent, IHasKeyName, IEditable {

    #region Fields

    public const string TableVersion = "4.10";

    public static readonly ObservableCollection<Table> AllFiles = [];

    public static readonly object AllFilesLocker = new object();

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich
    /// </summary>
    public static readonly int MasterBlockedMax = 180;

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich
    /// </summary>
    public static readonly int MasterBlockedMin;

    /// <summary>
    /// Wert in Minuten. Ist man selbst Master in diesen Range, dann zählt das, dass man ein Master ist
    /// </summary>
    public static readonly int MasterCount = 150;

    /// <summary>
    /// Wert in Minuten. Solange dürfen werden und Masters willkürlich gesetzt.
    /// </summary>
    public static readonly int MasterTry = 7;

    /// <summary>
    /// Wert in Minuten. Solange gilt der gesetzte Master Wert.
    /// </summary>
    public static readonly int MasterUntil = 175;

    /// <summary>
    /// Wert in Minuten. Ab diesen Wert dürfen Master die Zeilenberechnung übernehmen
    /// </summary>
    public static readonly int MyRowLost = 6;

    /// <summary>
    /// Wert in Minuten, in welchem Intervall die Tabellen auf Aktualität geprüft werden.
    /// </summary>
    public static readonly int UpdateTable = 5;

    protected NeedPassword? _needPassword;

    private static List<string> _allavailableTables = [];

    private static DateTime _lastAvailableTableCheck = new(1900, 1, 1);

    private readonly object _eventScriptLock = new();

    private readonly List<string> _permissionGroupsNewRow = [];

    private readonly List<string> _tableAdmin = [];

    private readonly List<string> _tags = [];

    private readonly List<Variable> _variables = [];

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

    private bool? _mayAffectUser;

    private DateTime _powerEditTime = DateTime.MinValue;

    private string _rowQuickInfo = string.Empty;

    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    private string _temporaryTableMasterApp = string.Empty;

    private string _temporaryTableMasterId = string.Empty;

    private string _temporaryTableMasterMachine = string.Empty;

    private string _temporaryTableMasterTimeUtc = string.Empty;

    private string _temporaryTableMasterUser = string.Empty;

    private ReadOnlyCollection<UniqueValueDefinition> _uniqueValues = new([]);

    private string _variableTmp;

    #endregion

    #region Constructors

    protected Table(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgelöst wird.
        Develop.StartService();
        lock (AllFilesLocker) {
            KeyName = MakeValidTableName(tablename);

            if (!IsValidTableName(KeyName)) {
                Develop.DebugError("Tabellenname ungültig: " + tablename);
            }

            Cell = new CellCollection(this);
            Row = new RowCollection(this);
            Column = new ColumnCollection(this);

            Column.ColumnDisposed += Column_ColumnDisposed;
            Column.ColumnRemoving += Column_ColumnRemoving;

            Undo = [];

            _creator = UserName;
            _createDate = DateTime.UtcNow.ToString9();
            LastSaveMainFileUtcDate = new DateTime(0);
            LoadedVersion = TableVersion;
            _assetFolder = "Assets";
            _variableTmp = string.Empty;

            // Muss vor dem Laden der Datan zu Allfiles hinzugfügt werde, weil das bei OnAdded
            // Die Events registriert werden, um z.B: das Passwort abzufragen
            // Zusätzlich werden z.B: Filter für den Export erstellt - auch der muss die Tabelle finden können.
            // Zusätzlich muss der Tablename stimme, dass in Added diesen verwerten kann.

            AllFiles.Add(this);
        }
    }

    #endregion

    #region Destructors

    ~Table() {
        try {
            Dispose(false);
        } catch {
            // Finalizer darf keine Exceptions werfen
        }
    }

    #endregion

    #region Delegates

    public delegate string NeedPassword();

    #endregion

    #region Events

    public event EventHandler? AdditionalRepair;

    public event EventHandler<CanDoScriptEventArgs>? CanDoScript;

    public event EventHandler? Disposed;

    public event EventHandler? DisposingEvent;

    public event EventHandler? InvalidateView;

    public event EventHandler<FirstEventArgs>? Loaded;

    public event EventHandler? Loading;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    public static List<string> ExecutingScriptThreadsAnyTable { get; } = [];

    /// <summary>
    /// So viele Master of Table darf man sein
    /// </summary>
    public static int MaxMasterCount { get; set; } = 3;

    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.")]
    public string AssetFolder {
        get => _assetFolder;
        set {
            if (_assetFolder == value) { return; }
            _assetFolderTemp = null;
            ChangeData(TableDataType.AssetFolder, null, _assetFolder, value);
            Cell.InvalidateAllSizes();
        }
    }

    [Description("Der Name der Tabelle.")]
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

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public ReadOnlyCollection<TableScriptDescription> EventScript {
        get {
            lock (_eventScriptLock) { return _eventScript; }
        }
        set {
            lock (_eventScriptLock) {
                var l = new List<TableScriptDescription>();
                l.AddRange(value);
                l.Sort();

                var eventScriptOld = _eventScript.ToString(false);
                var eventScriptNew = l.ToString(false);

                if (eventScriptOld == eventScriptNew) { return; }
                ChangeData(TableDataType.EventScript, null, eventScriptOld, eventScriptNew);
            }
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

    public bool IsDisposed { get; private set; }

    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    public bool KeyIsCaseSensitive => false;

    public string KeyName { get; }

    public DateTime LastChange { get; private set; } = new(1900, 1, 1);

    /// <summary>
    /// Der Wert wird im System verankert und gespeichert.
    /// Bei Tabellen, die Daten nachladen können, ist das der Stand, zu dem alle Daten fest abgespeichert sind.
    /// Kann hier nur gelesen werden! Da eine Änderung über die Property die Datei wieder auf ungespeichert setzen würde, würde hier eine
    /// Kettenreaktion ausgelöst werden.
    /// </summary>
    public DateTime LastSaveMainFileUtcDate { get; protected set; }

    /// <summary>
    /// Wann die Tabelle zuletzt angeschaut / geöffnet / geladen wurde.
    /// Bestimmt die Reihenfolge der Reparaturen
    /// </summary>
    public DateTime LastUsedDate { get; set; } = DateTime.UtcNow;

    public bool LogUndo { get; set; } = true;

    public bool MainChunkLoadDone { get; protected set; }

    public virtual bool MasterNeeded => false;

    public virtual bool MultiUserPossible => false;

    public ReadOnlyCollection<string> PermissionGroupsNewRow {
        get => new(_permissionGroupsNewRow);
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            ChangeData(TableDataType.PermissionGroupsNewRow, null, string.Join('\r', _permissionGroupsNewRow), string.Join('\r', value));
        }
    }

    public bool PowerEdit {
        get => _powerEditTime.Subtract(DateTime.UtcNow).TotalSeconds > 0;

        set {
            _powerEditTime = value ? DateTime.UtcNow.AddSeconds(300) : DateTime.UtcNow.AddSeconds(-1);
            OnInvalidateView();
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
            if (_sortDefinition != null) { alt = _sortDefinition.ParseableItems().FinishParseable(); }
            if (value != null) { neu = value.ParseableItems().FinishParseable(); }
            if (alt == neu) { return; }
            ChangeData(TableDataType.SortDefinition, null, alt, neu);

            OnSortParameterChanged();
        }
    }

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Description("Das standardmäßige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            ChangeData(TableDataType.StandardFormulaFile, null, _standardFormulaFile, value);
        }
    }

    public ReadOnlyCollection<string> TableAdmin {
        get => new(_tableAdmin);
        set {
            if (!_tableAdmin.IsDifferentTo(value)) { return; }
            ChangeData(TableDataType.TableAdminGroups, null, string.Join('\r', _tableAdmin), string.Join('\r', value));
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
            var newStr = string.Join('\r', value.Select(x => x.ParseableItems().FinishParseable()));

            if (oldStr == newStr) { return; }
            ChangeData(TableDataType.UniqueValues, null, oldStr, newStr);
        }
    }

    public VariableCollection Variables {
        get => [.. _variables];
        set {
            var l = new List<VariableString>();
            l.AddRange(value.ToListVariableString());
            foreach (var thisv in l) {
                thisv.ReadOnly = true; // Weil kein OnPropertyChangedEreigniss vorhanden ist
            }
            l.Sort();
            if (_variableTmp == l.ToString(true)) { return; }

            #region Kritische Variablen Disposen

            foreach (var thisVar in _variables) {
                thisVar.DisposeContent();
            }

            #endregion

            ChangeData(TableDataType.TableVariables, null, _variableTmp, l.ToString(true));
        }
    }

    protected string LoadedVersion { get; private set; }

    private string? _assetFolderTemp { get; set; }

    #endregion

    #region Methods

    public static List<string> AllAvailableTables() {
        if (DateTime.UtcNow.Subtract(_lastAvailableTableCheck).TotalMinutes < 20) {
            return _allavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Tabelle z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu prüfen
        var allreadychecked = new List<Table>();

        var allfiles = new List<Table>(AllFiles); // könnte sich ändern, deswegen Zwischenspeichern

        foreach (var thisTb in allfiles) {
            var possibletables = thisTb.AllAvailableTables(allreadychecked);

            allreadychecked.Add(thisTb);

            if (possibletables != null) {
                _allavailableTables.AddRange(possibletables);
            }
        }
        _allavailableTables = _allavailableTables.SortedDistinctList();
        _lastAvailableTableCheck = DateTime.UtcNow;
        return _allavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
    }

    public static void BeSureToBeUpToDate(ObservableCollection<Table> ofTables) {
        List<Table> l = [.. ofTables];

        foreach (var tbl in l) {
            tbl.BeSureToBeUpToDate(false);
        }
    }

    public static string EscapeCSVField(string field, char separator) => CsvHelper.EscapeCSVField(field, separator);

    public static List<string> EscapeCSVFields(List<string> fields, char separator) => CsvHelper.EscapeCSVFields(fields, separator);

    public static void FreezeAll(string reason) {
        List<Table> snapshot;
        lock (AllFilesLocker) {
            snapshot = [.. AllFiles];
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
        return t;
    }

    public static Table? Get(string fileOrTableName, NeedPassword? needPassword) {
        try {
            if (fileOrTableName.Contains('|')) {
                var t = fileOrTableName.SplitBy("|");
                var tn = string.Empty;
                var fn = string.Empty;

                foreach (var thist in t) {
                    if (string.IsNullOrEmpty(fn) && thist.IsFormat(FormatHolder.FilepathAndName)) {
                        fn = thist;
                    }
                    if (string.IsNullOrEmpty(tn) && IsValidTableName(thist)) {
                        tn = thist;
                    }
                }

                if (!string.IsNullOrEmpty(fn)) {
                    fileOrTableName = fn;
                } else if (!string.IsNullOrEmpty(tn)) {
                    fileOrTableName = tn;
                }
            }

            #region Schauen, ob die Tabelle bereits geladen ist

            var folder = new List<string>();

            if (fileOrTableName.IsFormat(FormatHolder.FilepathAndName)) {
                folder.AddIfNotExists(fileOrTableName.FilePath());
                fileOrTableName = fileOrTableName.FileNameWithoutSuffix();
            }

            fileOrTableName = MakeValidTableName(fileOrTableName);

            Table? ok = null;
            lock (AllFilesLocker) {
                foreach (var thisFile in AllFiles) {
                    if (string.Equals(thisFile.KeyName, fileOrTableName, StringComparison.OrdinalIgnoreCase)) {
                        ok = thisFile;
                        break;
                    }

                    if (thisFile is TableFile tbf && tbf.Filename.IsFormat(FormatHolder.FilepathAndName)) {
                        folder.AddIfNotExists(tbf.Filename.FilePath());
                    }
                }
            }

            if (ok is { } okt) {
                okt.WaitInitialDone();
                return okt;
            }

            #endregion

            foreach (var thisfolder in folder) {
                var f = thisfolder + fileOrTableName;

                var fs = f + ".cbdb";
                if (FileExists(fs)) {
                    if (!TableFile.IsFileAllowedToLoad(fs)) { return Get(fs, needPassword); }
                    var tb = new TableChunk(fileOrTableName);
                    tb.LoadFromFile(fs, needPassword, string.Empty);
                    tb.WaitInitialDone();
                    return tb;
                }

                fs = f + ".mbdb";
                if (FileExists(fs)) {
                    if (!TableFile.IsFileAllowedToLoad(fs)) { return Get(fs, needPassword); }
                    var tb = new TableFragments(fileOrTableName);
                    tb.LoadFromFile(fs, needPassword, string.Empty);
                    tb.WaitInitialDone();
                    return tb;
                }

                fs = f + ".bdb";
                if (FileExists(fs)) {
                    if (!TableFile.IsFileAllowedToLoad(fs)) { return Get(fs, needPassword); }
                    var tb = new TableFile(fileOrTableName);
                    tb.LoadFromFile(fs, needPassword, string.Empty);
                    tb.WaitInitialDone();
                    return tb;
                }

                fs = f + ".csv";
                if (FileExists(fs)) {
                    if (!TableFile.IsFileAllowedToLoad(fs)) { return Get(fs, needPassword); }
                    var tb = new TableCSV(fileOrTableName);
                    tb.LoadFromFile(fs, needPassword, string.Empty);
                    tb.WaitInitialDone();
                    return tb;
                }

                fs = f + ".hbdb";
                if (FileExists(fs)) {
                    // hbdb ist eine Begleitdatei zu einer CSV – die zugehörige CSV laden
                    var csvFile = fs.FilePath() + fs.FileNameWithoutSuffix() + ".csv";
                    if (FileExists(csvFile)) {
                        if (!TableFile.IsFileAllowedToLoad(csvFile)) { return Get(csvFile, needPassword); }
                        var tb = new TableCSV(fileOrTableName);
                        tb.LoadFromFile(csvFile, needPassword, string.Empty);
                        tb.WaitInitialDone();
                        return tb;
                    }
                }
            }

            return null;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return Get(fileOrTableName, needPassword);
        }
    }

    /// <summary>
    /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
    /// </summary>
    /// <param name="column">Die Spalte</param>
    /// <param name="row">Die Zeile</param>
    /// <param name="newChunkValue">Der neue Zellwert</param>
    /// <param name="waitforSeconds"></param>
    /// <param name="onlyTopLevel"></param>
    /// <returns>Leerer String bei Erfolg, ansonsten Fehlermeldung</returns>
    public static string GrantWriteAccess(ColumnItem? column, RowItem? row, string newChunkValue, int waitforSeconds, bool onlyTopLevel) =>
        ProcessFile(TryGrantWriteAccess, [], false, waitforSeconds, newChunkValue, column, row, waitforSeconds, onlyTopLevel).FailedReason;

    public static bool IsValidTableName(string tablename) {
        if (string.IsNullOrEmpty(tablename)) { return false; }

        var t = tablename.ToUpperInvariant();

        if (t.StartsWith("SYS_")) { return false; }
        if (t.StartsWith("BAK_")) { return false; }
        if (t.StartsWith("DATABASE")) { return false; }
        if (t.StartsWith("TABLE")) { return false; }

        if (!tablename.IsFormat(FormatHolder.SystemName)) { return false; }

        if (tablename == "ALL_TAB_COLS") { return false; } // system-name

        // eigentlich 128, aber minus BAK_ und _2023_03_28
        return t.Length <= 100;
    }

    /// <summary>
    /// Sucht die Tabelle im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    public static Table? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream) {
        if (Develop.IsHostRunning() && !mustBeStream) {
            var x = -1;
            string? pf;
            do {
                x++;
                pf = string.Empty;
                switch (x) {
                    case 2:
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\BlueControls\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 1:
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\..\\..\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 0:  // BeCreative, At Home, 31.11.2021
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 3:
                        pf = $"{Develop.AppPath()}..\\..\\..\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 4:
                        pf = $"{Develop.AppPath()}{name}";
                        break;

                    case 5:
                        pf = $"{Develop.AppPath()}{blueBasicsSubDir}\\{name}";
                        break;

                    case 6:
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 7:
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 8:
                        // warscheinlich BeCreative, Firma
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;

                    case 9:
                        // Bildzeichen-Liste, Firma, 25.10.2021
                        pf = $"{Develop.AppPath()}..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressources\\{blueBasicsSubDir}\\{name}";
                        break;
                }

                if (FileExists(pf)) {
                    var tb = new TableFile(name) {
                        DropMessages = false
                    };
                    tb.LoadFromFile(pf, null, string.Empty);
                    return tb;
                }
            } while (!string.IsNullOrEmpty(pf));
        }
        var d = GetEmmbedResource(assembly, name);
        if (d != null) {
            var tb = new Table(name);
            tb.LoadFromStream(d);
            return tb;
        }
        if (fehlerAusgeben) { Develop.DebugError("Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().Replace(' ', '_').Replace('-', '_');
        tmp = tmp.StarkeVereinfachung("_", false).ToUpperInvariant();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet, um so viel wie möglich zu speichern - falls eine Datei blokiert ist.</param>
    public static void SaveAll(bool mustSave) {
        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Tabelle, "Speichere alle Tabellen", 0);

        if (mustSave) { SaveAll(false); }

        List<Table> snapshot;
        lock (AllFilesLocker) {
            snapshot = [.. AllFiles];
        }

        foreach (var thisFile in snapshot) {
            if (thisFile is TableFile tbf) {
                tbf.Save(mustSave);
            }
        }

        Develop.Message(ErrorType.Info, null, "Tabellen", ImageCode.Häkchen, "Tabellen gespeichert", 0);
    }

    public static string UniqueKeyValue() {
        lock (AllFilesLocker) {
            var x = 9999;
            do {
                x += 1;
                if (x > 99999) { Develop.DebugError("Unique ID konnte nicht erzeugt werden"); }

                var unique = ("X" + DateTime.UtcNow.ToString("mm.fff", CultureInfo.InvariantCulture) + x.ToString5()).RemoveChars(Char_DateiSonderZeichen + " _.");
                var ok = true;

                if (IsValidTableName(unique)) {
                    foreach (var thisfile in AllFiles) {
                        if (string.Equals(unique, thisfile.KeyName, StringComparison.Ordinal)) { ok = false; break; }
                    }
                } else {
                    ok = false;
                }

                if (ok) { return unique; }
            } while (true);
        }
    }

    public static bool UpdateScript(TableScriptDescription script, string? keyname = null, string? scriptContent = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, bool isDisposed = false, bool? readOnly = null, int? stoppedtimecount = null, long? averageruntime = null) {
        if (script?.Table is not { IsDisposed: false } tb) { return false; }

        var onlyTimeAndCountUpdates = failedReason == null && keyname == null && scriptContent == null && image == null && quickInfo == null && adminInfo == null && eventTypes == null && needRow == null && userGroups == null && isDisposed == false && readOnly == null;

        if (onlyTimeAndCountUpdates) {
            if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.EventScript, string.Empty))) { return false; }
        } else {
            if (!string.IsNullOrEmpty(tb.GrantWriteAccess(TableDataType.EventScript))) { return false; }
        }

        lock (tb._eventScriptLock) {
            var found = false;

            List<TableScriptDescription> updatedScripts = [];

            foreach (var existingScript in tb._eventScript) {
                if (ReferenceEquals(existingScript, script) || existingScript.KeyName == script.KeyName && existingScript.Script == script.Script) {
                    found = true;

                    if (!isDisposed) {
                        // Prüfe ob sich wirklich etwas geändert hat
                        var hasChanges = keyname != null && keyname != existingScript.KeyName ||
                                        scriptContent != null && scriptContent != existingScript.Script ||
                                        image != null && image != existingScript.Image ||
                                        quickInfo != null && quickInfo != existingScript.QuickInfo ||
                                        adminInfo != null && adminInfo != existingScript.AdminInfo ||
                                        eventTypes != null && !eventTypes.Equals(existingScript.EventTypes) ||
                                        needRow != null && needRow != existingScript.NeedRow ||
                                        readOnly != null && readOnly != existingScript.ValuesReadOnly ||
                                        userGroups?.SequenceEqual(existingScript.UserGroups) == false ||
                                        failedReason != null && failedReason != existingScript.FailedReason ||
                                        stoppedtimecount != null && stoppedtimecount != existingScript.StoppedTimeCount ||
                                        averageruntime != null && averageruntime != existingScript.AverageRunTime;

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

            tb.EventScript = updatedScripts.AsReadOnly();
        }

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
            } catch { }
        }
    }

    public virtual string[]? AllAvailableTables(List<Table>? allreadychecked) => null;

    /// <summary>
    ///
    /// </summary>
    /// <param name="ranges">Unter 5 Minuten wird auch geprüft, ob versucht wird, einen Master zu setzen. Ab 5 minuten ist es gewiss.</param>
    /// <param name="rangee">Bis 55 Minuten ist sicher, dass es der Master ist.
    /// Werden kleiner Werte abgefragt, kann ermittelt werden, ob der Master bald ausläuft.
    /// Werden größerer Werte abgefragt, kann ermittel werden, ob man Master war,
    /// </param>
    /// <param name="updateAllowed"></param>
    /// <returns></returns>
    public virtual bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        if (!MultiUserPossible) { return true; }
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return false; }

        if (TemporaryTableMasterUser != UserName) { return false; }
        if (TemporaryTableMasterMachine != Environment.MachineName) { return false; }

        var d = DateTimeParse(TemporaryTableMasterTimeUtc);
        var mins = DateTime.UtcNow.Subtract(d).TotalMinutes;

        ranges = Math.Max(ranges, 0);

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogeprüft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return mins > ranges && mins < rangee;
    }

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AssetFolderWhole() {
        if (_assetFolderTemp != null) { return _assetFolderTemp; }

        if (!string.IsNullOrEmpty(_assetFolder)) {
            var t = _assetFolder.NormalizePath();
            if (t.IsFormat(FormatHolder.Filepath)) {
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
            if (t.IsFormat(FormatHolder.Filepath)) {
                _assetFolderTemp = t;
                return t;
            }
        }
        _assetFolderTemp = string.Empty;
        return string.Empty;
    }

    public virtual bool BeSureRowIsLoaded(string chunkValue) => string.IsNullOrEmpty(IsGenericEditable(false));

    public virtual bool BeSureToBeUpToDate(bool firstTime) => true;

    /// <summary>
    /// Info: Table.HasValueChangedScript kann schnell die Existenz Abgefragt werden
    /// </summary>
    /// <param name="notExistingValue">Der Wert, der zurückgebenen werden soll, wenn das Skript NICHT vorhanden ist</param>
    /// <returns></returns>
    public bool CanDoValueChangedScript(bool notExistingValue) => IsRowScriptPossible() && IsThisScriptBroken(ScriptEventTypes.value_changed, notExistingValue);

    public string ChangeData(TableDataType command, ColumnItem? column, string previousValue, string changedTo) => ChangeData(command, column, null, previousValue, changedTo, UserName, DateTime.UtcNow, string.Empty, string.Empty, string.Empty);

    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und kümmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
    /// Der Wert wird intern fest verankert.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="user"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    /// <param name="oldchunkvalue"></param>
    /// <param name="newchunkvalue"></param>
    public string ChangeData(TableDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, string oldchunkvalue, string newchunkvalue) {
        if (IsDisposed) { return "Tabelle verworfen!"; }
        if (IsFreezed) { return "Tabelle eingefroren: " + FreezedReason; }
        if (type.IsObsolete()) { return "Obsoleter Befehl angekommen!"; }

        //if (!type.IsCommand() &&  GrantWriteAccess(type) is { Length: > 0 } f) { return f; }

        var colName = column?.KeyName ?? string.Empty;

        // ERST Speicher setzen
        var error = SetValueInternal(type, column, row, changedTo, user, datetimeutc, Reason.SetCommand);
        if (!string.IsNullOrEmpty(error)) { return error; }

        // DANN Festplatte schreiben (nur bei nicht ReadOnly)
        if (!IsFreezed) {
            var newChunkId = TableChunk.GetChunkId(this, type, newchunkvalue);
            var oldChunkId = newChunkId;

            if (string.IsNullOrEmpty(oldChunkId) || newchunkvalue == oldchunkvalue) {
                oldChunkId = newChunkId;
            } else {
                oldChunkId = TableChunk.GetChunkId(this, type, oldchunkvalue);
            }

            var f2 = WriteValueToDiscOrServer(type, changedTo, colName, row, user, datetimeutc, oldChunkId, newChunkId, comment);
            if (!string.IsNullOrEmpty(f2)) {
                DropMessage(ErrorType.Warning, $"Rollback aufgrund eines Fehlers:\r\n{f2}");
                // Rollback: Vorherigen Wert im Speicher wiederherstellen
                SetValueInternal(type, column, row, previousValue, user, datetimeutc, Reason.NoUndo_NoInvalidate);
                return f2;
            }
        }

        // Bei Spaltenumbenennung auch ColumnArrangements aktualisieren
        if (type == TableDataType.ColumnKey && column != null) {
            UpdateColumnArrangementsAfterRename(column);
        }

        if (LogUndo) {
            AddUndo(type, colName, row, previousValue, changedTo, user, datetimeutc, comment, "[Änderung in dieser Session]", newchunkvalue);
        }

        return string.Empty;
    }

    public string CheckScriptError() {
        foreach (var script in _eventScript) {
            if (!script.IsOk()) { return $"{script.KeyName}: {script.ErrorReason()}"; }
        }
        return string.Empty;
    }

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool tableHeadVariables, bool virtualcolumns, bool extendedVariable, IEnumerable<FilterItem>? filter) {

        #region Variablen für Skript erstellen

        VariableCollection vars = [];

        if (row is { IsDisposed: false }) {
            foreach (var thisCol in Column) {
                var v = RowItem.CellToVariable(thisCol, row, allReadOnly, virtualcolumns);
                if (v != null) { vars.Add(v); }
            }
            vars.Add(new VariableString("CurrentRowKey", row.KeyName, true, "Der interne Zeilenschlüssel der Zeile,\r\nmit der das Skript aufgerufen wurde."));
            vars.Add(new VariableRowItem("CurrentRow", row, true, "Die Zeile, mit der das Skript aufgerufen wurde."));
        }

        if (filter is { }) {
            var num = 0;
            foreach (var thisFilter in filter) {
                vars.Add(new VariableFilterItem($"FilterInput{num}", thisFilter, true, "Ein Eingangsfilter"));
                num++;
            }
        }

        if (tableHeadVariables) {
            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("DB_" + thisvar.KeyName, thisvar.ValueString, false, "Tabellen-Kopf-Variable\r\n" + thisvar.Comment);
                vars.Add(v);
            }
        }

        vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new VariableString("User", UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new VariableString("UserGroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        vars.Add(new VariableBool("Administrator", IsAdministrator(), true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Tabelle ist."));
        vars.Add(new VariableString("Tablename", KeyName, true, "Der aktuelle Tabellenname."));
        vars.Add(new VariableTable("CurrentTable", this, true, "Die aktuelle Tabelle"));
        vars.Add(new VariableBool("ReadOnly", IsFreezed, true, "Ob die aktuelle Tabelle schreibgeschützt ist."));
        vars.Add(new VariableDouble("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Tabelle"));
        vars.Add(new VariableString("StartTimeUTC", DateTime.UtcNow.ToString7(), true, "Die Uhrzeit, wann das Skript gestartet wurde."));

        if (Column.First is { IsDisposed: false } fc) {
            vars.Add(new VariableString("NameOfFirstColumn", fc.KeyName, true, "Der Name der ersten Spalte"));

            if (row != null) {
                vars.Add(new VariableString("ValueOfFirstColumn", row.CellGetString(fc), true, "Der Wert der ersten Spalte als String"));
            }
        }

        vars.Add(new VariableString("additionalfilespath", AssetFolderWhole(), true, "OBSOLETE: AssetFolder benutzen!")); // TODO: entfernen

        vars.Add(new VariableString("AssetFolder", AssetFolderWhole(), true, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));
        vars.Add(new VariableBool("Extended", extendedVariable, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));
        vars.Add(new VariableListString("ErrorColumns", [], true, "Spalten, die mit SetError fehlerhaft gesetzt wurden."));

        if (virtualcolumns) {
            vars.Add(new VariableString("RowColor", string.Empty, false, "Die Zeilenfarbe\r\nMuss Werte im Format RGB oder ARGB enthalten.\r\nBeispiel: #ff0000 oder #ff120320"));
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

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        AllFiles.Remove(this);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void DropMessage(ErrorType type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        Develop.Message(type, this, Caption, ImageCode.Tabelle, message, 0);
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

        if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, script.KeyName); }
        if (!script.NeedRow) { row = null; }

        if (!ignoreError && row != null && RowCollection.FailedRows.TryGetValue(row, out var reason)) {
            return new ScriptEndedFeedback($"Das Skript konnte die Zeile nicht durchrechnen: {reason}", false, false, script.KeyName);
        }

        extended = extended || !script.MayAffectUser;

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
            var meth = Method.GetMethods(script.AllowedMethodsMaxLevel(extended));

            if (script.VirtalColumns) { meth.Add(Method_SetError.Method); }

            #region Script ausführen

            var ki = Caption;

            if (row is { IsDisposed: false }) { ki = ki + "\\" + row.CellFirstString(); }

            var scp = new ScriptProperties(script.KeyName, meth, produktivphase, script.Attributes(), addinfo, script.KeyName, ki);

            var sc = new Script(vars, scp) {
                ScriptText = script.Script
            };

            AbortReason abr = extended ? ExternalAbortScriptReasonExtended : ExternalAbortScriptReason;
            var timew = Stopwatch.StartNew();
            var scf = sc.Parse(0, script.KeyName, args, abr);

            #endregion

            #region Fehlerprüfungen

            UpdateScript(script, scf, timew, row, extended, produktivphase, ignoreError);

            if (scf.Failed) { return scf; }

            if (row != null && !script.ValuesReadOnly) {
                if (row.IsDisposed) { return new ScriptEndedFeedback("Die geprüfte Zeile wurde verworfen", false, false, script.KeyName); }
                if (Column.SysRowChangeDate is null) { return new ScriptEndedFeedback("Zeilen können nur geprüft werden, wenn Änderungen der Zeile geloggt werden.", false, false, script.KeyName); }
                if (row.RowStamp() != rowstamp) { return new ScriptEndedFeedback("Zeile wurde während des Skriptes verändert.", false, false, script.KeyName); }
            }

            #endregion

            WriteBackVariables(row, vars, script.VirtalColumns, tableHeadVariables, script.KeyName, produktivphase && !script.ValuesReadOnly);

            //  Erfolgreicher Abschluss
            if (isNewId) { ExecutingScriptThreadsAnyTable.Remove(scriptThreadId); }

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
    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string? scriptname, bool produktivphase, RowItem? row, List<string>? args, bool tbHeadVariables, bool extended, float retrySeconds = 0) {
        scriptname ??= string.Empty;

        if (eventname != null && !string.IsNullOrWhiteSpace(scriptname)) {
            Develop.DebugError("Event und Skript angekommen!");
            return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
        }

        if (eventname == null && string.IsNullOrWhiteSpace(scriptname)) {
            return new ScriptEndedFeedback("Weder Eventname noch Skriptname angekommen", false, false, "Allgemein");
        }

        TableScriptDescription? script = null;
        if (string.IsNullOrWhiteSpace(scriptname) && eventname is { } ev) {
            if (!IsThisScriptBroken(ev, true)) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

            DropMessage(ErrorType.DevelopInfo, $"Ereignis ausgelöst: {eventname}");

            var l = EventScript.Get(ev);

            if (l.Count == 1) {
                script = l[0];
            } else if (l.Count == 0) {
                var vars = CreateVariableCollection(row, true, tbHeadVariables, true, false, null);
                return new ScriptEndedFeedback(vars, string.Empty);
            }
        } else {
            script = EventScript.GetByKey(scriptname);
        }

        if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }
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
            DropMessage(ErrorType.DevelopInfo, $"Tabelle {KeyName} wird eingefohren: {reason}");
        }

        FreezedReason = reason;
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

    public string GrantWriteAccess(RowItem row) => GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, row.ChunkValue);

    public string GrantWriteAccess(TableDataType type) => GrantWriteAccess(type, string.Empty);

    public virtual string GrantWriteAccess(TableDataType type, string? chunkValue) => IsGenericEditable(false);

    public string ImportCsv(string importText, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) =>
                    CsvHelper.ImportCsv(this, importText, zeileZuordnen, splitChar, eliminateMultipleSplitter, eleminateSplitterAtStart);

    public string ImportCsv(string importText, bool zeileZuordnen, char separator = ';', bool eliminateMultipleSplitter = false, bool eleminateSplitterAtStart = false) =>
                CsvHelper.ImportCsv(this, importText, zeileZuordnen, separator, eliminateMultipleSplitter, eleminateSplitterAtStart);

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
    /// Prüft und holt sich sicher die Rechte zum Bearbeiten.
    /// </summary>
    /// <returns></returns>
    string IEditable.IsNowEditable() => GrantWriteAccess(TableDataType.Caption);

    public string IsNowNewRowPossible(string? chunkValue, bool checkUserRights) {
        if (IsDisposed) { return "Tabelle verworfen"; }
        if (Column.Count == 0) { return "Keine Spalten vorhanden"; }

        if (!IsThisScriptBroken(ScriptEventTypes.InitialValues, true)) { return "Skripte nicht ausführbar"; }

        if (!checkUserRights) { return string.Empty; }

        if (Column.First is not { IsDisposed: false } fc) { return "Erste Spalte nicht definiert"; }

        return CellCollection.IsCellEditable(fc, null, chunkValue);
    }

    public bool IsRowScriptPossible() {
        if (Column.SysRowChangeDate == null) { return false; }
        if (Column.SysRowState == null) { return false; }
        return string.IsNullOrEmpty(IsGenericEditable(false));
    }

    /// <summary>
    /// Info: ValueChanedScript kann schnell mit Table.HasValueChangedScript abgefragt werden.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="notExistingValue">Der Wert, der zurückgebenen werden soll, wenn das Skript NICHT vorhanden ist</param>
    /// <returns></returns>
    public bool IsThisScriptBroken(ScriptEventTypes type, bool notExistingValue) {
        var l = _eventScript.Get(type);

        if (l.Count > 1) { return false; }

        if (l.Count == 0) { return notExistingValue; }

        return l[0].IsOk();
    }

    public virtual string IsValueEditable(TableDataType type, string? chunkValue) => IsGenericEditable(false);

    public void LoadFromStream(System.IO.Stream stream) {
        LogUndo = false;
        DropMessages = false;

        byte[] bLoaded;
        using (var r = new System.IO.BinaryReader(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt() ?? bLoaded; }

        OnLoading();
        Parse(bLoaded, true, Reason.NoUndo_NoInvalidate, null);
        RepairAfterParse();
        Freeze("Stream-Tabelle");
        MainChunkLoadDone = true;
        OnLoaded(true, true);
    }

    /// <summary>
    /// Lädt Zeilen der Tabelle nach. Je nach Tabellentyp werden andere Funktionen unterstützt
    /// </summary>
    /// <param name="oldest">True wird versucht, die ältesten Zeilen zu laden. Im normalfall langsamer, das Stände verglichen werden müssen</param>
    /// <param name="count">Dei Mindestanzahl der Zeilen zum laden. -1 für alle</param>
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
        } while (Row.GetByKey(key) != null);
        return key;
    }

    public void OnCanDoScript(CanDoScriptEventArgs e) {
        if (IsDisposed) { return; }
        CanDoScript?.Invoke(this, e);
    }

    public void OnInvalidateView() {
        if (IsDisposed) { return; }
        InvalidateView?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnViewChanged() {
        if (IsDisposed) { return; }
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

    public bool Parse(byte[] data, bool isMain, Reason reason, HashSet<string>? parsedRowKeys) {
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
                            Row.ExecuteCommand(TableDataType.Command_AddRow, rowKey, reason, null, null);
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
                                Column.ExecuteCommand(TableDataType.Command_AddColumnByName, columname, reason);
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

                    #region Bei verschlüsselten Tabellen das Passwort abfragen

                    if (command == TableDataType.GlobalShowPass && !string.IsNullOrEmpty(value)) {
                        var pwd = string.Empty;

                        if (_needPassword != null) {
                            pwd = _needPassword();
                        }

                        if (pwd != value) {
                            Freeze("Passwort falsch");
                            break;
                        }
                    }

                    #endregion

                    if (command == TableDataType.EOF) {
                        break;
                    }

                    var error = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, reason);
                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Tabellen-Ladefehler");
                        Develop.DebugPrint("Schwerer Tabellenfehler:<br>Version: " + TableVersion + "<br>Datei: " + KeyName + "<br>Meldung: " + error);
                        return false;
                    }
                }
            } while (true);
        } catch {
            Freeze("Parse Fehler!");
            return false;
        }

        if (isMain) {
            Column.RemoveObsoleteColumns(Column, columnUsed, reason);
            Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(TableVersion.Replace(".", string.Empty))) { Freeze("Tabelleversions-Konflikt"); }

        return true;
    }

    public bool PermissionCheck(IList<string>? allowed, RowItem? row) {
        try {
            if (IsAdministrator() || PowerEdit) { return true; }
            if (allowed is not { Count: not 0 }) { return false; }

            foreach (var thisString in allowed) {
                if (string.Equals(thisString, Everybody, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (Column.SysRowCreator is { IsDisposed: false } src &&
                    string.Equals(thisString, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase) &&
                    row != null && row.CellGetString(src).Equals(UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, "#USER: " + UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, "#USER:" + UserName, StringComparison.OrdinalIgnoreCase)) { return true; }
                if (string.Equals(thisString, UserGroup, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Rechte-Check", ex);
        }
        return false;
    }

    public virtual void ReorganizeChunks() { }

    public virtual void RepairAfterParse() {
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }

        Column.Repair();

        Row.Repair();

        SortDefinition ??= new RowSortDefinition(this, null as ColumnItem, false);

        SortDefinition?.Repair();

        foreach (var uv in _uniqueValues) { uv.Repair(); }

        PermissionGroupsNewRow = RepairUserGroups(PermissionGroupsNewRow).AsReadOnly();
        TableAdmin = RepairUserGroups(TableAdmin).AsReadOnly();

        if (LastSaveMainFileUtcDate.Year < 2000) {
            LastSaveMainFileUtcDate = new DateTime(2000, 1, 1);
        }

        OnAdditionalRepair();
    }

    public override string ToString() => IsDisposed ? string.Empty : base.ToString() + " " + KeyName;

    /// <summary>
    /// Diese Routine darf nur aufgerufen werden, wenn die Daten der Tabelle von der Festplatte eingelesen wurden.
    /// </summary>
    public void TryToSetMeTemporaryMaster() {
        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, true)) { return; }
        if (!NewMasterPossible()) { return; }
        MasterMe();
    }

    public void UnMasterMe() {
        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, false)) {
            if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax, true)) {
                TemporaryTableMasterUser = "Unset: " + UserName;
                TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddHours(-0.25).ToString5();
            }
        }
    }

    public void UpdateScript(TableScriptDescription script, ScriptEndedFeedback scf, Stopwatch tim, RowItem? row, bool extended, bool produktivphase, bool ignoreError) {
        var failed = script.FailedReason;
        var runTimeCount = script.StoppedTimeCount;
        var avgRunTime = script.AverageRunTime;

        if (scf.NeedsScriptFix && !ignoreError && produktivphase) {
            failed = $"Tabelle: {Caption}\r\n" +
                     $"Benutzer: {UserName}\r\n" +
                     $"Zeit (UTC): {DateTime.UtcNow.ToString5()}\r\n" +
                     $"Extended: {extended}\r\n";

            if (row is { IsDisposed: false } r) {
                failed += $"Zeile: {r.CellFirstString()}\r\n";
                failed += $"Zeilen-Schlüssel: {r.KeyName}\r\n";
                if (Column.ChunkValueColumn is { IsDisposed: false } spc) {
                    failed += $"Chunk-Wert: {r.CellGetString(spc)}\r\n";
                }
            }

            failed += $"\r\n\r\n\r\n{scf.ProtocolText}\r\n\r\n\r\nVariablen:\r\n";

            if (scf.Variables is { } v) {
                foreach (var thisV in v) {
                    var tmpi = thisV.ReadableText.Replace('\r', ';');
                    if (tmpi.Length > 100) { tmpi = tmpi[..100] + "..."; }
                    failed += $"{thisV.KeyName}: {tmpi}\r\n";
                }
            }
        } else {
            var newStoppedTime = tim.ElapsedMilliseconds + 500; // +500 wegen Variablen zurückschreiben und so Zeugs

            if (extended || scf.Variables?.GetByKey("Extended") == null) {
                if (runTimeCount < int.MaxValue - 100) {
                    var newt = avgRunTime; // Zurücksetzen
                    var deviation = Math.Abs(newStoppedTime - avgRunTime) / (double)avgRunTime;

                    if ((runTimeCount < 100 && deviation > 0.1f) ||
                        (runTimeCount < 300 && deviation > 0.3f)) {
                        newt = ((avgRunTime * runTimeCount) + newStoppedTime) / (runTimeCount + 1);
                    }

                    if (Math.Abs(newt - avgRunTime) > 100 || runTimeCount < 25) {
                        runTimeCount++;
                        avgRunTime = newt;
                    }
                }
            }
        }

        if (row != null && !string.IsNullOrEmpty(script.FailedReason)) {
            RowCollection.FailedRows.TryAdd(row, scf.FailedReason);
            DropMessage(ErrorType.Info, $"Skript-Fehler: {scf.FailedReason}");
        }

        if (failed != script.FailedReason) {
            UpdateScript(script, failedReason: failed);
        }

        if (runTimeCount != script.StoppedTimeCount || avgRunTime != script.AverageRunTime) {
            UpdateScript(script, stoppedtimecount: runTimeCount, averageruntime: avgRunTime);
        }
    }

    public bool UpdateScript(string keyName, string? newkeyname, string? script = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, bool isDisposed = false, bool? readOnly = null, int? stoppedtimecount = null, long? averageruntime = null) {
        var existingScript = EventScript.GetByKey(keyName);
        if (existingScript == null) { return false; }

        return UpdateScript(existingScript, newkeyname, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason, isDisposed, readOnly, stoppedtimecount, averageruntime);
    }

    public void WriteBackVariables(RowItem? row, VariableCollection vars, bool virtualcolumns, bool tableHeadVariables, string comment, bool doWriteBack) {
        if (doWriteBack) {
            if (row is { IsDisposed: false }) {
                foreach (var thisCol in Column) {
                    row.VariableToCell(thisCol, vars, comment);
                }
            }
            if (tableHeadVariables) {
                Variables = VariableCollection.Combine(Variables, vars, "DB_");
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
        } catch { }
        Develop.DebugPrint(t);
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
    /// <param name="chunkValue"></param>
    protected void AddUndo(TableDataType type, string column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container, string chunkValue) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == TableDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(KeyName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container, chunkValue));
    }

    protected virtual void Checker_Tick(object? state) {
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
        if (IsDisposed) { return; }

        if (disposing) {
            try {
                OnDisposingEvent();
                UnregisterEvents();

                // Timer zuerst disposen
                _checker?.Dispose();
                _checker = null;

                // Dann Collections disposen
                Column.Dispose();
                Row.Dispose();

                // Listen leeren
                Undo.Clear();
                _eventScript = new ReadOnlyCollection<TableScriptDescription>([]);
                _tableAdmin.Clear();
                _permissionGroupsNewRow.Clear();
                _tags.Clear();

                // Aus statischer Liste entfernen
                AllFiles.Remove(this);
            } catch (Exception ex) {
                Develop.DebugError("Fehler beim Dispose: " + ex.Message);
            }

            IsDisposed = true;
            OnDisposed();
        }
    }

    protected void OnAdditionalRepair() {
        if (IsDisposed) { return; }
        AdditionalRepair?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoaded(bool isFirst, bool affectingHead) {
        if (IsDisposed) { return; }
        Loaded?.Invoke(this, new FirstEventArgs(isFirst, affectingHead));
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und führt Commands aus.
    /// Es wird WriteValueToDiscOrServer aufgerufen - echtzeitbasierte Systeme können dort den Wert speichern
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="reason"></param>
    /// <param name="user"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    protected string SetValueInternal(TableDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, Reason reason) {
        if (IsDisposed) { return "Tabelle verworfen!"; }

        if (!reason.HasFlag(Reason.IgnoreFreeze)) {
            var f = IsGenericEditable(false);
            if (!string.IsNullOrEmpty(f)) { return $"Tabelle eingefroren: {f}"; }
        }

        if (type.IsObsolete()) { return string.Empty; }

        LastChange = DateTime.UtcNow;

        if (type.IsCellValue()) {
            if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }
            if (row == null) { return string.Empty; }
            if (!column.SaveContent) { return string.Empty; }

            if (row.CellSetInternal(column, value, reason) is { Length: > 0 } f) { return f; }

            if (column.SaveContent) {
                row.DoSystemColumns(tb, column, user, datetimeutc, reason);
            }
            return string.Empty;
        }

        if (type.IsColumnTag()) {
            if (column is not { IsDisposed: false } || Column.IsDisposed) {
                DropMessage(ErrorType.Info, $"Wert nicht gesetzt, Spalte nicht vorhanden");
                return string.Empty;
            }

            return column.SetValueInternal(type, value);
        }

        if (type.IsRowTag()) {
            if (row is not { IsDisposed: false } || Column.IsDisposed) {
                DropMessage(ErrorType.Info, $"Wert nicht gesetzt, Zeile nicht vorhanden");
                return string.Empty;
            }

            return row.SetValueInternal(type, value);
        }

        if (type.IsCommand()) {
            switch (type) {
                case TableDataType.Command_RemoveColumn:
                    if (Column[value] is not { } c) { return string.Empty; }
                    return Column.ExecuteCommand(type, c.KeyName, reason);

                case TableDataType.Command_AddColumnByName:
                    return Column.ExecuteCommand(type, value, reason);

                case TableDataType.Command_RemoveRow:
                    if (Row.GetByKey(value) is not { } r) { return string.Empty; }
                    return Row.ExecuteCommand(type, r.KeyName, reason, user, datetimeutc);

                case TableDataType.Command_AddRow:
                    return Row.ExecuteCommand(type, value, reason, user, datetimeutc);

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

            case TableDataType.LastSaveMainFileUtcDate:
                LastSaveMainFileUtcDate = DateTimeParse(value);
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

            case TableDataType.UniqueValues:
                var uvs = value.SplitAndCutByCr();
                var uvsl = uvs.Select(t => new UniqueValueDefinition(this, t)).ToList();
                _uniqueValues = uvsl.AsReadOnly();
                break;

            case TableDataType.Caption:
                _caption = value;
                break;

            case TableDataType.AssetFolder:
                _assetFolder = value;
                break;

            case TableDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case TableDataType.RowQuickInfo:
                _rowQuickInfo = value;
                break;

            case TableDataType.Tags:
                _tags.SplitAndCutByCr(value);
                break;

            case TableDataType.EventScript:
                var ves = value.SplitAndCutByCr();
                var vess = ves.Select(t => new TableScriptDescription(this, t)).ToList();
                _hasValueChangedScript = null;
                Row.InvalidateAllCheckData();
                _eventScript = vess.AsReadOnly();
                _changesRowColor = null;
                break;

            case TableDataType.TableVariables:
                _variables.Clear();
                List<string> va = [.. value.SplitAndCutByCr()];
                foreach (var t in va) {
                    var l = new VariableString("dummy");
                    l.Parse(t);
                    l.ReadOnly = true; // Weil kein OnPropertyChangedEreigniss vorhanden ist
                    _variables.Add(l);
                }
                _variables.Sort();
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
                Undo.Clear();
                var uio = value.SplitAndCutByCr();
                for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                    var tmpWork = new UndoItem(uio[z]);
                    Undo.Add(tmpWork);
                }
                break;

            case TableDataType.Undo:
                Undo.Add(new(value));
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
        var t = Stopwatch.StartNew();

        var lastMessageTime = 0L;

        while (!MainChunkLoadDone) {
            Thread.Sleep(1);
            if (t.ElapsedMilliseconds > 120 * 1000) {
                DropMessage(ErrorType.DevelopInfo, $"Abbruch, Tabelle {KeyName} wurde nicht richtig initialisiert");
                return;
            }

            if (IsFreezed) {
                DropMessage(ErrorType.DevelopInfo, $"Abbruch, Tabelle {KeyName} eingefrohren {FreezedReason}");
                return;
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                DropMessage(ErrorType.DevelopInfo, $"Warte auf Abschluss der Initialsierung von {KeyName}");
            }
        }
    }

    protected virtual string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }
        return IsGenericEditable(false);
    }

    private static bool HasActiveThreadsExcept(string excludeThreadId) {
        try {
            return ExecutingScriptThreadsAnyTable.Exists(thread => thread != excludeThreadId);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return HasActiveThreadsExcept(excludeThreadId);
        }
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

    private static OperationResult TryGrantWriteAccess(List<string> affectingFiles, params object?[] args) {
        if (args.Length < 5 ||
            args[0] is not string newChunkValue ||
            args[1] is not ColumnItem column ||
            args[3] is not int waitforseconds ||
             args[4] is not bool onlyTopLevel) {
            return OperationResult.FailedInternalError;
        }

        var row = args[2] as RowItem;

        try {
            if (column.Table is not { IsDisposed: false } tb) { return OperationResult.Failed("Es ist keine Spalte ausgewählt."); }

            var f = tb.GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
            if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed(f); }

            if (row != null) {
                f = tb.GrantWriteAccess(row);
                if (!string.IsNullOrEmpty(f)) { return OperationResult.Failed(f); }
            } else {
                if (column.RelationType == RelationType.CellValues) {
                    return OperationResult.Failed("Verknüpfte Tabelle kann keine Initialzeile erstellt werden.");
                }
            }

            if (onlyTopLevel) { return OperationResult.Success; }

            if (column.RelationType == RelationType.CellValues && row != null) {
                var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);
                if (!string.IsNullOrEmpty(info) && !canrepair) { return OperationResult.Failed(info); }

                if (lcolumn?.Table is not { IsDisposed: false } tb2) { return OperationResult.Failed("Verknüpfte Tabelle verworfen."); }

                tb2.PowerEdit = tb.PowerEdit;

                if (lrow != null) {
                    waitforseconds = Math.Max(1, waitforseconds / 2);

                    f = GrantWriteAccess(lcolumn, lrow, lrow.ChunkValue, waitforseconds, true);
                    if (!string.IsNullOrEmpty(f)) { return new OperationResult(waitforseconds > 10, $"Die verlinkte Zelle kann nicht bearbeitet werden: {f}"); }
                    return OperationResult.Success;
                }

                return OperationResult.FailedInternalError;
            }

            return OperationResult.Success;
        } catch (Exception ex) {
            return OperationResult.FailedRetryable(ex); // Retry bei Exceptions
        }
    }

    private void Column_ColumnDisposed(object? sender, ColumnEventArgs e) {
        if (IsDisposed) { return; }
        RepairAfterParse();
    }

    private void Column_ColumnRemoving(object? sender, ColumnEventArgs e) {
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

    private string ExternalAbortScriptReason(bool extended) {
        var e = new CanDoScriptEventArgs(extended);
        OnCanDoScript(e);
        return e.CancelReason;
    }

    private void InitDummyTable() {
        LogUndo = false;
        DropMessages = false;

        OnLoading();

        MainChunkLoadDone = true;
        BeSureToBeUpToDate(true);

        RepairAfterParse();

        OnLoaded(true, true);

        CreateWatcher();
    }

    private bool NewMasterPossible() {
        if (!IsAdministrator()) { return false; }

        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return false; }

        if (DateTimeTryParse(TemporaryTableMasterTimeUtc, out var dt)) {
            if (DateTime.UtcNow.Subtract(dt).TotalMinutes < MasterBlockedMax) { return false; }
            if (DateTime.UtcNow.Subtract(dt).TotalDays > 1) { return true; }
        }

        if (RowCollection.WaitDelay > 90) { return true; }

        if (MasterNeeded) { return true; }

        try {
            var masters = 0;
            List<Table> snapshot;
            lock (AllFilesLocker) {
                snapshot = [.. AllFiles];
            }

            foreach (var thisTb in snapshot) {
                if (MultiUserPossible && !thisTb.IsDisposed && thisTb.AmITemporaryMaster(MasterBlockedMin, MasterCount, false)) {
                    masters++;
                    if (masters >= MaxMasterCount) { return false; }
                }
            }
        } catch {
            return false;
        }

        return true;
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void UnregisterEvents() {
        try {
            // Column Events
            if (Column != null) {
                Column.ColumnDisposed -= Column_ColumnDisposed;
                Column.ColumnRemoving -= Column_ColumnRemoving;
            }

            // Eigene Events auf null setzen
            AdditionalRepair = null;
            CanDoScript = null;
            Disposed = null;
            DisposingEvent = null;
            InvalidateView = null;
            Loaded = null;
            Loading = null;
            SortParameterChanged = null;
            ViewChanged = null;
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Abmelden der Events", ex);
        }
    }

    private void UpdateColumnArrangementsAfterRename(ColumnItem column) {
        if (_columnArrangements.Count == 0) { return; }

        foreach (var arrangement in _columnArrangements) {
            if (arrangement[column] != null) {
                var updatedArrangements = _columnArrangements.ToString(false);
                WriteValueToDiscOrServer(TableDataType.ColumnArrangement, updatedArrangements, string.Empty, null, UserName, DateTime.UtcNow, string.Empty, string.Empty, "Automatische Aktualisierung nach Spaltenumbenennung");
                return;
            }
        }
    }

    #endregion
}