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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using Timer = System.Threading.Timer;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Database : IDisposableExtendedWithEvent, IHasKeyName, ICanDropMessages, IEditable {

    #region Fields

    public const string DatabaseVersion = "4.02";
    public static readonly ObservableCollection<Database> AllFiles = [];

    /// <summary>
    /// Wenn diese Varianble einen Count von 0 hat, ist der Speicher nicht initialisiert worden.
    /// </summary>
    public readonly List<UndoItem> Undo;

    /// <summary>
    /// Wann die Datenbank zuletzt angeschaut / geöffnet / geladen wurde.
    /// Bestimmt die Reihenfolge der Reparaturen
    /// </summary>
    public DateTime LastUsedDate = DateTime.UtcNow;

    /// <summary>
    ///  So viele Änderungen sind seit dem letzten erstellen der Komplett-Datenbank erstellen auf Festplatte gezählt worden
    /// </summary>
    protected readonly List<UndoItem> ChangesNotIncluded = [];

    private static List<Type>? _databaseTypes;
    private static bool _isInTimer;
    private static DateTime _lastTableCheck = new(1900, 1, 1);

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static Timer? _pendingChangesTimer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _pendingChangesTimer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    private readonly List<ColumnViewCollection> _columnArrangements = [];
    private readonly List<string> _datenbankAdmin = [];
    private readonly List<DatabaseScriptDescription> _eventScript = [];
    private readonly List<string> _permissionGroupsNewRow = [];
    private readonly List<string> _tags = [];
    private readonly List<Variable> _variables = [];
    private string _additionalFilesPfad;
    private string _cachePfad = string.Empty;
    private string _canWriteError = string.Empty;
    private DateTime _canWriteNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private string _caption = string.Empty;
    private Timer? _checker;
    private int _checkerTickCount = -5;
    private string _createDate;
    private string _creator;
    private string _editNormalyError = string.Empty;
    private DateTime _editNormalyNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private string _eventScriptTmp = string.Empty;
    private long _eventScriptVersion = 1;
    private double _globalScale = 1f;
    private string _globalShowPass = string.Empty;
    private bool _isInSave;
    private bool _readOnly;
    private string _scriptNeedFix = string.Empty;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    private string _temporaryDatabaseMasterTimeUtc = string.Empty;
    private string _temporaryDatabaseMasterUser = string.Empty;
    private string _variableTmp;
    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public Database(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgelöst wird.
        Develop.StartService();

        QuickImage.NeedImage += QuickImage_NeedImage;

        TableName = MakeValidTableName(tablename);

        if (!IsValidTableName(TableName, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
        }

        Cell = new CellCollection(this);
        Row = new RowCollection(this);
        Column = new ColumnCollection(this);

        Column.ColumnDisposed += Column_ColumnDisposed;
        Column.ColumnRemoving += Column_ColumnRemoving;

        Undo = [];

        //_columnArrangements.Clear();
        //_permissionGroupsNewRow.Clear();
        //_tags.Clear();
        //_datenbankAdmin.Clear();
        //_globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.UtcNow.ToString9();
        FileStateUTCDate = new DateTime(0); // Wichtig, dass das Datum bei Datenbanken ohne den Wert immer alles laden
        //_caption = string.Empty;
        LoadedVersion = DatabaseVersion;
        //_globalScale = 1f;
        _additionalFilesPfad = "AdditionalFiles";
        //_zeilenQuickInfo = string.Empty;
        //_sortDefinition = null;
        //EventScript_RemoveAll(true);
        //_variables.Clear();
        _variableTmp = string.Empty;
        //Undo.Clear();

        // Muss vor dem Laden der Datan zu Allfiles hinzugfügt werde, weil das bei OnAdded
        // Die Events registriert werden, um z.B: das Passwort abzufragen
        // Zusätzlich werden z.B: Filter für den Export erstellt - auch der muss die Datenbank finden können.
        // Zusätzlich muss der Tablename stimme, dass in Added diesen verwerten kann.
        AllFiles.Add(this);
        GenerateTimer();
    }

    #endregion

    #region Destructors

    ~Database() { Dispose(false); }

    #endregion

    #region Delegates

    public delegate string NeedPassword();

    #endregion

    #region Events

    public event EventHandler<CancelReasonEventArgs>? CanDoScript;

    public event EventHandler? Disposed;

    public event EventHandler? DisposingEvent;

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event EventHandler? InvalidateView;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    public static string DatabaseId => nameof(Database);
    public static int ExecutingScriptAnyDatabase { get; set; } = 0;

    public static string MyMasterCode => UserName + "-" + Environment.MachineName;

    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.")]
    public string AdditionalFilesPfad {
        get => _additionalFilesPfad;
        set {
            if (_additionalFilesPfad == value) { return; }
            AdditionalFilesPfadtmp = null;
            _ = ChangeData(DatabaseDataType.AdditionalFilesPath, null, null, _additionalFilesPfad, value, UserName, DateTime.UtcNow, string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    public string CachePfad {
        get => _cachePfad;
        set {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_cachePfad == value) { return; }
            _cachePfad = value;
        }
    }

    [Description("Der Name der Datenbank.")]
    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _ = ChangeData(DatabaseDataType.Caption, null, null, _caption, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string CaptionForEditor => "Datenbank";
    public CellCollection Cell { get; }
    public ColumnCollection Column { get; }

    public ReadOnlyCollection<ColumnViewCollection> ColumnArrangements {
        get => new(_columnArrangements);
        set {
            if (_columnArrangements.ToString(false) == value.ToString(false)) { return; }
            _ = ChangeData(DatabaseDataType.ColumnArrangement, null, null, _columnArrangements.ToString(false), value.ToString(false), UserName, DateTime.UtcNow, string.Empty);
            OnViewChanged();
        }
    }

    public virtual ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename, FreezedReason);

    public string CreateDate {
        get => _createDate;
        private set {
            if (_createDate == value) { return; }
            _ = ChangeData(DatabaseDataType.CreateDateUTC, null, null, _createDate, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string Creator {
        get => _creator.Trim();
        private set {
            if (_creator == value) { return; }
            _ = ChangeData(DatabaseDataType.Creator, null, null, _creator, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public ReadOnlyCollection<string> DatenbankAdmin {
        get => new(_datenbankAdmin);
        set {
            if (!_datenbankAdmin.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.DatabaseAdminGroups, null, null, _datenbankAdmin.JoinWithCr(), value.JoinWithCr(), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    /// <summary>
    /// Während der Daten aktualiszer werden dürfen z.B. keine Tabellenansichten gemachte werden.
    /// Weil da Zeilen sortiert / invalidiert / Sortiert / invalidiert etc. werden
    /// </summary>
    public int DoingChanges { get; private set; } = 0;

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public Type? Editor { get; set; }

    public ReadOnlyCollection<DatabaseScriptDescription> EventScript {
        get => new(_eventScript);
        set {
            var l = new List<DatabaseScriptDescription>();
            l.AddRange(value);
            l.Sort();

            if (_eventScriptTmp == l.ToString(false)) { return; }
            _ = ChangeData(DatabaseDataType.EventScript, null, null, _eventScriptTmp, l.ToString(true), UserName, DateTime.UtcNow, string.Empty);

            ScriptNeedFix = string.Empty;
        }
    }

    public long EventScriptVersion {
        get => _eventScriptVersion;
        set {
            if (_eventScriptVersion == value) { return; }
            _ = ChangeData(DatabaseDataType.EventScriptVersion, null, null, _eventScriptVersion.ToString(), value.ToString(), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public int ExecutingScript { get; set; } = 0;
    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Der Wert wird im System verankert und gespeichert.
    /// Bei Datenbanken, die Daten nachladen können, ist das der Stand, zu dem alle Daten fest abgespeichert sind.
    /// Kann hier nur gelesen werden! Da eine Änderung über die Property  die Datei wieder auf ungespeichert setzen würde, würde hier eine
    /// Kettenreaktion ausgelöst werden.
    /// </summary>
    public DateTime FileStateUTCDate { get; protected set; }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgänig gemacht werden.
    /// Weil keine Undos mehr geladen werden, würde da nur Chaos entstehten.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public double GlobalScale {
        get => _globalScale;
        set {
            if (Math.Abs(_globalScale - value) < DefaultTolerance) { return; }
            _ = ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToStringFloat2(), value.ToStringFloat2(), UserName, DateTime.UtcNow, string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            _ = ChangeData(DatabaseDataType.GlobalShowPass, null, null, _globalShowPass, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public bool HasPendingChanges { get; protected set; }

    public bool IsDisposed { get; private set; }

    public string KeyName => IsDisposed ? string.Empty : ConnectionData.UniqueId;

    public DateTime LastChange { get; private set; } = new(1900, 1, 1);

    public bool LogUndo { get; set; } = true;

    public ReadOnlyCollection<string> PermissionGroupsNewRow {
        get => new(_permissionGroupsNewRow);
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.PermissionGroupsNewRow, null, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr(), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public DateTime PowerEdit { get; set; }

    /// <summary>
    /// Prüft auch den FreezedReason
    /// </summary>
    public bool ReadOnly {
        get => _readOnly || !string.IsNullOrEmpty(FreezedReason);
        private set => _readOnly = value;
    }

    public RowCollection Row { get; }

    public string ScriptNeedFix {
        get => _scriptNeedFix;
        set {
            if (_scriptNeedFix == value) { return; }
            _ = ChangeData(DatabaseDataType.ScriptNeedFix, null, null, _scriptNeedFix, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public RowSortDefinition? SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
            if (value != null) { neu = value.ToString(); }
            if (alt == neu) { return; }
            _ = ChangeData(DatabaseDataType.SortDefinition, null, null, alt, neu, UserName, DateTime.UtcNow, string.Empty);

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
            _ = ChangeData(DatabaseDataType.StandardFormulaFile, null, null, _standardFormulaFile, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string TableName { get; }

    public ReadOnlyCollection<string> Tags {
        get => new(_tags);
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.Tags, null, null, _tags.JoinWithCr(), value.JoinWithCr(), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string TemporaryDatabaseMasterTimeUtc {
        get => _temporaryDatabaseMasterTimeUtc;
        private set {
            if (_temporaryDatabaseMasterTimeUtc == value) { return; }
            _ = ChangeData(DatabaseDataType.TemporaryDatabaseMasterTimeUTC, null, null, _temporaryDatabaseMasterTimeUtc, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string TemporaryDatabaseMasterUser {
        get => _temporaryDatabaseMasterUser;
        private set {
            if (_temporaryDatabaseMasterUser == value) { return; }
            _ = ChangeData(DatabaseDataType.TemporaryDatabaseMasterUser, null, null, _temporaryDatabaseMasterUser, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public VariableCollection Variables {
        get => new(_variables);
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

            _ = ChangeData(DatabaseDataType.DatabaseVariables, null, null, _variableTmp, l.ToString(true), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            _ = ChangeData(DatabaseDataType.RowQuickInfo, null, null, _zeilenQuickInfo, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    protected static List<ConnectionInfo> AllavailableTables { get; } = [];

    protected string? AdditionalFilesPfadtmp { get; set; }

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    protected DateTime IsInCache { get; set; } = new(0);

    protected string LoadedVersion { get; private set; }

    protected virtual bool MultiUser => false;

    /// <summary>
    ///  Wann die Datei zuletzt geladen wurde. Einzige funktion, zu viele Ladezyklen hintereinander verhinden.
    /// </summary>
    private static DateTime LastLoadUtc { get; } = DateTime.UtcNow;

    #endregion

    #region Methods

    public static List<ConnectionInfo> AllAvailableTables(string mustBeFreezed) {
        if (DateTime.UtcNow.Subtract(_lastTableCheck).TotalMinutes < 20) {
            return AllavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Datenbank z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu prüfen
        var allreadychecked = new List<Database>();

        var alf = new List<Database>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDb in alf) {
            var possibletables = thisDb.AllAvailableTables(allreadychecked, mustBeFreezed);

            allreadychecked.Add(thisDb);

            if (possibletables != null) {
                foreach (var thistable in possibletables) {
                    var canadd = true;

                    #region prüfen, ob schon voranden, z.b. Database.AllFiles

                    foreach (var checkme in AllavailableTables) {
                        if (string.Equals(checkme.TableName, thistable.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                            canadd = false;
                            break;
                        }
                    }

                    #endregion

                    if (canadd) { AllavailableTables.Add(thistable); }
                }
            }
        }

        _lastTableCheck = DateTime.UtcNow;
        return AllavailableTables.Clone(); // Als Clone, damit bezüge gebrochen werden und sich die Auflistung nicht mehr verändern kann
    }

    public static void CheckSysUndoNow(ICollection<Database> offDatabases, bool mustDoIt) {
        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        try {
            var done = new List<Database>();

            foreach (var thisDb in offDatabases) {
                if (!done.Contains(thisDb)) {
                    if (offDatabases.Count == 1) {
                        thisDb.OnDropMessage(FehlerArt.Info, "Überprüfe auf Veränderungen von '" + offDatabases.First().TableName + "'");
                    } else {
                        thisDb.OnDropMessage(FehlerArt.Info, "Überprüfe auf Veränderungen von " + offDatabases.Count + " Datenbanken des Typs '" + thisDb.GetType().Name + "'");
                    }

                    #region Datenbanken des gemeinsamen Servers ermittelen

                    var dbwss = thisDb.LoadedDatabasesWithSameServer();
                    done.AddRange(dbwss);
                    done.Add(thisDb); // Falls LoadedDatabasesWithSameServer einen Fehler versursacht

                    #endregion

                    #region Auf Eingangs Datenbanken beschränken

                    var db = new List<Database>();
                    foreach (var thisDb2 in dbwss) {
                        if (offDatabases.Contains(thisDb2)) { db.Add(thisDb2); }
                    }

                    #endregion

                    var (changes, files) = thisDb.GetLastChanges(db, _timerTimeStamp.AddSeconds(-0.01), fd);
                    if (changes == null) {
                        _isInTimer = false;

                        if (mustDoIt) {
                            Develop.CheckStackForOverflow();
                            Generic.Pause(1, false);
                            CheckSysUndoNow(offDatabases, mustDoIt);
                        }

                        // Später ein neuer Versuch
                        return;
                    }

                    var start = DateTime.UtcNow;
                    db.Shuffle();
                    foreach (var thisdb in db) {
                        thisdb.DoLastChanges(files, changes, fd, start);
                        thisdb.TryToSetMeTemporaryMaster();
                    }
                }
            }
        } catch {
            _isInTimer = false;
            return;
        }

        _timerTimeStamp = fd;
        _isInTimer = false;
    }

    public static string EditableErrorReason(Database? database, EditableErrorReasonType mode) {
        if (database is null || database.IsDisposed) { return "Keine Datenbank zum bearbeiten."; }
        return database.EditableErrorReason(mode);
    }

    public static void ForceSaveAll() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            _ = thisFile?.Save();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                ForceSaveAll();
                return;
            }
        }
    }

    public static Database? GetByFilename(string filname, bool readOnly, NeedPassword? needPassword, bool checktablename, string mustbefreezed) {
        var l = new ConnectionInfo(filname, null, mustbefreezed);
        return GetById(l, readOnly, needPassword, checktablename);
    }

    public static Database? GetById(ConnectionInfo? ci, bool readOnly, NeedPassword? needPassword, bool checktablename) {
        if (ci is null) { return null; }

        #region Schauen, ob die Datenbank bereits geladen ist

        foreach (var thisFile in AllFiles) {
            var d = thisFile.ConnectionData;

            if (string.Equals(d.UniqueId, ci.UniqueId, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }

            if (checktablename) {
                if (string.Equals(d.TableName, ci.TableName, StringComparison.OrdinalIgnoreCase)) {
                    return thisFile;
                }
            }

            if (d.AdditionalData.ToLowerInvariant().EndsWith(".mdb") ||
                d.AdditionalData.ToLowerInvariant().EndsWith(".bdb") ||
                d.AdditionalData.ToLowerInvariant().EndsWith(".mbdb")) {
                if (d.AdditionalData.Equals(ci.AdditionalData, StringComparison.OrdinalIgnoreCase)) {
                    return thisFile; // Multiuser - nicht multiuser konflikt
                }
            }
        }

        #endregion

        #region Schauen, ob der Provider sie herstellen kann

        if (ci.Provider != null) {
            var db = ci.Provider.GetOtherTable(ci.TableName, readOnly);
            if (db != null) { return db; }
        }

        #endregion

        _databaseTypes ??= GetEnumerableOfType<Database>();

        //#region Schauen, ob sie über den Typ definiert werden kann

        //foreach (var thist in _databaseTypes) {
        //    if (thist.Name.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) {
        //        var l = new object?[3];
        //        l[0] = ci;
        //        l[1] = readOnly;
        //        l[2] = needPassword;
        //        var v = thist.GetMethod("CanProvide")?.Invoke(null, l);

        //        if (v is Database db && !db.IsDisposed) { return db; }
        //    }
        //}

        //#endregion

        #region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        if (FileExists(ci.AdditionalData)) {
            if (ci.AdditionalData.FileSuffix().ToLowerInvariant() is "mdb" or "bdb") {
                var db = new Database(ci.TableName);
                db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
                return db;
            }
            if (ci.AdditionalData.FileSuffix().ToLowerInvariant() is "mbdb") {
                var db = new DatabaseMu(ci.TableName);
                db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
                return db;
            }
        }

        #endregion

        #region Zu guter Letzte, den Tablenamen nehmen...

        foreach (var thisFile in AllFiles) {
            var d = thisFile.ConnectionData;

            if (string.Equals(d.TableName, ci.TableName, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }

        #endregion

        return null;
    }

    public static bool IsValidTableName(string tablename, bool allowSystemnames) {
        if (string.IsNullOrEmpty(tablename)) { return false; }

        var t = tablename.ToUpperInvariant();

        if (!allowSystemnames) {
            if (t.StartsWith("SYS_")) { return false; }
            if (t.StartsWith("BAK_")) { return false; }
        }

        if (!t.ContainsOnlyChars(Char_AZ + Char_Numerals + "_")) { return false; }

        if (tablename == "ALL_TAB_COLS") { return false; } // system-name

        // eigentlich 128, aber minus BAK_ und _2023_03_28
        if (t.Length > 100) { return false; }

        return true;
    }

    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    public static Database? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream) {
        if (Develop.IsHostRunning() && !mustBeStream) {
            var x = -1;
            string? pf;
            do {
                x++;
                pf = string.Empty;
                switch (x) {
                    case 0:
                        // BeCreative, At Home, 31.11.2021
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 1:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 2:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 3:
                        pf = Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 4:
                        pf = Application.StartupPath + "\\" + name;
                        break;

                    case 5:
                        pf = Application.StartupPath + "\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 6:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 7:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 8:
                        // warscheinlich BeCreative, Firma
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 9:
                        // Bildzeichen-Liste, Firma, 25.10.2021
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;
                }
                if (FileExists(pf)) {
                    var db = new Database(name);
                    db.LoadFromFile(pf, false, null, string.Empty, false);
                    return db;
                }
            } while (pf != string.Empty);
        }
        var d = GetEmmbedResource(assembly, name);
        if (d != null) {
            var db = new Database(name);
            db.LogUndo = false;
            db.LoadFromStream(d);
            return db;
        }
        if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
        tmp = tmp.StarkeVereinfachung("_", false).ToUpperInvariant();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    public static List<string> Permission_AllUsed(bool cellLevel) {
        var l = new List<string>();

        foreach (var thisDB in AllFiles) {
            if (!thisDB.IsDisposed) {
                l.AddRange(thisDB.Permission_AllUsedInThisDB(cellLevel));
            }
        }

        return RepairUserGroups(l);
    }

    public static List<string> RepairUserGroups(IEnumerable<string> e) {
        var l = new List<string>();

        foreach (var thisUser in e) {
            if (string.Equals(thisUser, Everybody, StringComparison.OrdinalIgnoreCase)) {
                l.Add(Everybody);
            } else if (string.Equals(thisUser, Administrator, StringComparison.OrdinalIgnoreCase)) {
                l.Add(Administrator);
            } else if (string.Equals(thisUser, "#RowCreator", StringComparison.OrdinalIgnoreCase)) {
                l.Add("#RowCreator");
            } else if (thisUser.StartsWith("#USER:", StringComparison.OrdinalIgnoreCase)) {
                var th = thisUser.Substring(6).Trim(" ");

                l.Add("#User: " + th.ToUpper());
            } else {
                l.Add(thisUser.ToUpper());
            }
        }

        return l.SortedDistinctList();
    }

    public static bool SaveToFile(Database db, int minLen, string filn) {
        var bytes = ToListOfByte(db, minLen, db.FileStateUTCDate);

        if (bytes == null) { return false; }

        try {
            using FileStream x = new(filn, FileMode.Create, FileAccess.Write, FileShare.None);
            x.Write(bytes.ToArray(), 0, bytes.ToArray().Length);
            x.Flush();
            x.Close();
        } catch { return false; }

        return true;
    }

    public static List<byte>? ToListOfByte(Database db, int minLen, DateTime fileStateUtcDateToSave) {
        try {
            var x = db.LastChange;
            List<byte> l = [];
            // Wichtig, Reihenfolge und Länge NIE verändern!
            SaveToByteList(l, DatabaseDataType.Formatkennung, "BlueDatabase");
            SaveToByteList(l, DatabaseDataType.Version, DatabaseVersion);
            SaveToByteList(l, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
            // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
            //if (string.IsNullOrEmpty(GlobalShowPass)) {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, false.ToPlusMinus());
            //} else {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, true.ToPlusMinus());
            //    SaveToByteList(l, DatabaseDataType.CryptionTest, "OK");
            //}
            SaveToByteList(l, DatabaseDataType.GlobalShowPass, db.GlobalShowPass);
            //SaveToByteList(l, DatabaseDataType.FileEncryptionKey, _fileEncryptionKey);
            SaveToByteList(l, DatabaseDataType.Creator, db.Creator);
            SaveToByteList(l, DatabaseDataType.CreateDateUTC, db.CreateDate);
            SaveToByteList(l, DatabaseDataType.FileStateUTCDate, fileStateUtcDateToSave.ToString7());
            SaveToByteList(l, DatabaseDataType.Caption, db.Caption);

            SaveToByteList(l, DatabaseDataType.TemporaryDatabaseMasterUser, db.TemporaryDatabaseMasterUser);
            SaveToByteList(l, DatabaseDataType.TemporaryDatabaseMasterTimeUTC, db.TemporaryDatabaseMasterTimeUtc);

            //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString(false));
            //SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString(false));
            SaveToByteList(l, DatabaseDataType.Tags, db.Tags.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.DatabaseAdminGroups, db.DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.GlobalScale, db.GlobalScale.ToStringFloat2());
            //SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString(false));
            //SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, ReloadDelaySecond.ToString(false));
            //SaveToByteList(l, DatabaseDataType.RulesScript, db.RulesScript);
            //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
            //SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _filterImagePfad);
            SaveToByteList(l, DatabaseDataType.AdditionalFilesPath, db.AdditionalFilesPfad);
            //SaveToByteList(l, DatabaseDataType.FirstColumn, db.FirstColumn);
            SaveToByteList(l, DatabaseDataType.RowQuickInfo, db.ZeilenQuickInfo);
            SaveToByteList(l, DatabaseDataType.StandardFormulaFile, db.StandardFormulaFile);
            SaveToByteList(l, db.Column);
            //Row.SaveToByteList(l);
            //SaveToByteList(l, db.Cell, db);
            // Ganz neue Datenbank
            SaveToByteList(l, DatabaseDataType.SortDefinition, db.SortDefinition == null ? string.Empty : db.SortDefinition.ToString());
            //SaveToByteList(l, enDatabaseDataType.Rules_ALT, Rules.ToString(true));
            SaveToByteList(l, DatabaseDataType.ColumnArrangement, db.ColumnArrangements.ToString(false));
            //SaveToByteList(l, DatabaseDataType.Layouts, db.Layouts.JoinWithCr());
            //SaveToByteList(l, DatabaseDataType.AutoExport, db.Export.ToString(true));

            SaveToByteList(l, DatabaseDataType.EventScript, db.EventScript.ToString(true));
            SaveToByteList(l, DatabaseDataType.EventScriptVersion, db.EventScriptVersion.ToString());
            SaveToByteList(l, DatabaseDataType.ScriptNeedFix, db.ScriptNeedFix);
            //SaveToByteList(l, DatabaseDataType.Events, db.Events.ToString(true));
            SaveToByteList(l, DatabaseDataType.DatabaseVariables, db.Variables.ToList().ToString(true));

            if (x != db.LastChange) { return null; } // Works haben sich evtl. geändert

            // Beim Erstellen des Undo-Speichers die Undos nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            List<string> works2 = [];
            foreach (var thisWorkItem in db.Undo) {
                if (thisWorkItem != null) {
                    if (thisWorkItem.Command != DatabaseDataType.Value_withoutSizeData) {
                        works2.Add(thisWorkItem.ToString());
                    } else {
                        if (thisWorkItem.LogsUndo(db)) {
                            works2.Add(thisWorkItem.ToString());
                        }
                    }
                }
            }

            const int undoCount = 5000;
            //SaveToByteList(l, DatabaseDataType.UndoCount, db.UndoCount.ToString());
            if (works2.Count > undoCount) { works2.RemoveRange(0, works2.Count - undoCount); }
            SaveToByteList(l, DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
            SaveToByteList(l, DatabaseDataType.EOF, "END");

            if (l.Count < minLen) {
                //Develop.DebugPrint(FehlerArt.Fehler, "ToString Fehler!");
                return null;
            }

            if (x != db.LastChange) { return null; } // Stand stimmt nicht mehr

            return l;
        } catch {
            Develop.CheckStackForOverflow();
            return ToListOfByte(db, minLen, fileStateUtcDateToSave);
        }
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Database is not Database db || db.IsDisposed) { return string.Empty; }

        if (db.Undo.Count == 0) { return string.Empty; }

        var cellKey = CellCollection.KeyOfCell(column, row);
        var t = string.Empty;
        for (var z = db.Undo.Count - 1; z >= 0; z--) {
            if (db.Undo[z] != null && db.Undo[z].CellKey == cellKey) {
                t = t + db.Undo[z].UndoTextTableMouseOver() + "<br>";
            }
        }
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        return t;
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToStringInt5()).RemoveChars(Char_DateiSonderZeichen + " _.");
            var ok = true;

            if (IsValidTableName(unique, false)) {
                foreach (var thisfile in AllFiles) {
                    if (string.Equals(unique, thisfile.TableName)) { ok = false; break; }
                }
            } else {
                ok = false;
            }

            if (ok) { return unique; }
        } while (true);
    }

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AdditionalFilesPfadWhole() {
        if (AdditionalFilesPfadtmp != null) { return AdditionalFilesPfadtmp; }

        if (!string.IsNullOrEmpty(_additionalFilesPfad)) {
            var t = _additionalFilesPfad.CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionalFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }
        AdditionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ranges">Unter 5 Minuten wird auch geprüft, ob versucht wird, einen Master zu setzen. Ab 5 minuten ist es gewiss.</param>
    /// <param name="rangee">Bis 55 Minuten ist sicher, dass es der Master ist.
    /// Werden kleiner Werte abgefragt, kann ermittelt werden, ob der Master bald ausläuft.
    /// Werden größerer Werte abgefragt, kann ermittel werden, ob man Master war,
    /// </param>
    /// <returns></returns>
    public bool AmITemporaryMaster(int ranges, int rangee) {
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; }

        if (!MultiUser) { return true; }
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

    public bool CanDoPrepareFormulaCheckScript() {
        if (!IsRowScriptPossible(true)) { return false; }

        var e = EventScript.Get(ScriptEventTypes.prepare_formula);
        return e.Count == 1;
    }

    public bool CanDoValueChangedScript() {
        if (!IsRowScriptPossible(true)) { return false; }

        return EventScript.Get(ScriptEventTypes.value_changed).Count == 1;
    }

    //    if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }
    //    if (ci.AdditionalData.FileSuffix().ToUpperInvariant() is not "BDB" or "MDB") { return null; }
    //    if (!FileExists(ci.AdditionalData)) { return null; }
    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und kümmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
    /// Der Wert wird intern fest verankert - bei ReadOnly werden aber weitere Schritte ignoriert.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="user"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    public string ChangeData(DatabaseDataType command, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }
        if (command.IsObsolete()) { return "Obsoleter Befehl angekommen!"; }

        if (!ReadOnly) {
            var f2 = WriteValueToDiscOrServer(command, changedTo, column, row, user, datetimeutc, comment);
            if (!string.IsNullOrEmpty(f2)) { return f2; }
        }

        var (error, _, _) = SetValueInternal(command, column, row, changedTo, user, datetimeutc, Reason.SetCommand);
        if (!string.IsNullOrEmpty(error)) { return error; }

        if (LogUndo) {
            AddUndo(command, column, row, previousValue, changedTo, user, datetimeutc, comment, "[Änderung in dieser Session]");
        }

        return string.Empty;
    }

    //public static Database? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
    //    if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }
    public string CheckScriptError() {
        List<string> names = [];

        foreach (var thissc in _eventScript) {
            if (!thissc.IsOk()) {
                return thissc.KeyName + ": " + thissc.ErrorReason();
            }

            if (names.Contains(thissc.KeyName, false)) {
                return "Skriptname '" + thissc.KeyName + "' mehrfach vorhanden";
            }

            names.Add(thissc.KeyName);
        }

        var l = EventScript;
        if (l.Get(ScriptEventTypes.export).Count > 1) {
            return "Skript 'Export' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.row_deleting).Count > 1) {
            return "Skript 'Zeile löschen' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.loaded).Count > 1) {
            return "Skript 'Datenank geladen' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.prepare_formula).Count > 1) {
            return "Skript 'Formular Vorbereitung' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed_extra_thread).Count > 1) {
            return "Skript 'Wert geändert (Extra Thread)' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.InitialValues).Count > 1) {
            return "Skript 'Zeile Initialisieren' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed).Count > 1) {
            return "Skript 'Wert geändert' mehrfach vorhanden";
        }

        return string.Empty;
    }

    public void CloneFrom(Database sourceDatabase, bool cellDataToo, bool tagsToo) {
        _ = sourceDatabase.Save();

        Column.CloneFrom(sourceDatabase);

        if (cellDataToo) { Row.CloneFrom(sourceDatabase); }

        //FirstColumn = sourceDatabase.FirstColumn;
        AdditionalFilesPfad = sourceDatabase.AdditionalFilesPfad;
        CachePfad = sourceDatabase.CachePfad; // Nicht so wichtig ;-)
        Caption = sourceDatabase.Caption;
        //TimeCode = sourceDatabase.TimeCode;
        CreateDate = sourceDatabase.CreateDate;
        Creator = sourceDatabase.Creator;
        //FileStateUTCDate = sourceDatabase.FileStateUTCDate;
        //Filename - nope
        //Tablename - nope
        //TimeCode - nope
        GlobalScale = sourceDatabase.GlobalScale;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        //RulesScript = sourceDatabase.RulesScript;
        if (SortDefinition == null || SortDefinition.ToString() != sourceDatabase.SortDefinition?.ToString()) {
            if (sourceDatabase.SortDefinition != null) {
                SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition.ToString());
            }
        }

        StandardFormulaFile = sourceDatabase.StandardFormulaFile;
        EventScriptVersion = sourceDatabase.EventScriptVersion;
        ScriptNeedFix = sourceDatabase.ScriptNeedFix;
        ZeilenQuickInfo = sourceDatabase.ZeilenQuickInfo;
        if (tagsToo) {
            Tags = new(sourceDatabase.Tags.Clone());
        }

        DatenbankAdmin = new(sourceDatabase.DatenbankAdmin.Clone());
        PermissionGroupsNewRow = new(sourceDatabase.PermissionGroupsNewRow.Clone());

        var tcvc = new List<ColumnViewCollection>();
        foreach (var t in sourceDatabase.ColumnArrangements) {
            tcvc.Add(new ColumnViewCollection(this, t.ToString()));
        }
        ColumnArrangements = new(tcvc);

        //Export = sourceDatabase.Export;

        EventScript = sourceDatabase.EventScript;

        if (tagsToo) {
            Variables = sourceDatabase.Variables;
        }

        //Events = sourceDatabase.Events;

        //UndoCount = sourceDatabase.UndoCount;
    }

    public virtual ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".bdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f, FreezedReason);
    }

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool setErrorEnabled, bool dbVariables, bool virtualcolumns, bool? extendedVariable) {

        #region Variablen für Skript erstellen

        VariableCollection vars = [];

        if (row != null && !row.IsDisposed) {
            foreach (var thisCol in Column) {
                var v = RowItem.CellToVariable(thisCol, row, allReadOnly, virtualcolumns);
                if (v != null) { vars.Add(v); }
            }
        }

        if (dbVariables) {
            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("DB_" + thisvar.KeyName, thisvar.ValueString, false, "Datenbank-Kopf-Variable\r\n" + thisvar.Comment);
                vars.Add(v);
            }
        }

        vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new VariableString("User", UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new VariableString("Usergroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        vars.Add(new VariableBool("Administrator", IsAdministrator(), true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));
        vars.Add(new VariableString("Tablename", TableName, true, "Der aktuelle Tabellenname."));
        vars.Add(new VariableBool("ReadOnly", ReadOnly, true, "Ob die aktuelle Datenbank schreibgeschützt ist."));
        vars.Add(new VariableFloat("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Datenbank")); // RowCount als Befehl belegt
        vars.Add(new VariableString("NameOfFirstColumn", Column.First()?.KeyName ?? string.Empty, true, "Der Name der ersten Spalte"));
        vars.Add(new VariableBool("SetErrorEnabled", setErrorEnabled, true, "Marker, ob der Befehl 'SetError' benutzt werden kann."));

        if (extendedVariable is bool e) {
            vars.Add(new VariableBool("Extended", e, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));
        }

        #endregion

        return vars;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string EditableErrorReason(EditableErrorReasonType mode) {
        if (IsDisposed) { return "Datenbank verworfen."; }

        if (mode is EditableErrorReasonType.OnlyRead or EditableErrorReasonType.Load) { return string.Empty; }

        if (!string.IsNullOrEmpty(Filename) && IsInCache.Year < 2000) { return "Datenbank wird noch geladen"; }

        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }

        if (ReadOnly && mode.HasFlag(EditableErrorReasonType.Save)) { return "Datenbank schreibgeschützt!"; }

        if (mode.HasFlag(EditableErrorReasonType.EditCurrently) || mode.HasFlag(EditableErrorReasonType.Save)) {
            if (Row.HasPendingWorker()) { return "Es müssen noch Daten überprüft werden."; }
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) {
            return "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern.";
        }

        //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.Load) || mode.HasFlag(EditableErrorReasonType.LoadForCheckingOnly)) {
            if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        }

        //----------Alle Edits und Save ------------------------------------------------------------------------
        //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
        //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
        if (CheckForLastError(ref _editNormalyNextCheckUtc, ref _editNormalyError)) { return _editNormalyError; }
        if (!string.IsNullOrEmpty(Filename)) {
            if (!CanWriteInDirectory(Filename.FilePath())) {
                _editNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                return _editNormalyError;
            }
        }

        //---------- Save ------------------------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.Save)) {
            if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 1) { return "Kürzlich vorgenommene Änderung muss verarbeitet werden."; }
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
            if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------
            if (CheckForLastError(ref _canWriteNextCheckUtc, ref _canWriteError) && !string.IsNullOrEmpty(_canWriteError)) {
                return _canWriteError;
            }

            try {
                FileInfo f2 = new(Filename);
                if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5) {
                    _canWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                    return _canWriteError;
                }
            } catch {
                _canWriteError = "Dateizugriffsfehler.";
                return _canWriteError;
            }
            if (!CanWrite(Filename, 0.5)) {
                _canWriteError = "Windows blockiert die Datei.";
                return _canWriteError;
            }
        }
        return string.Empty;

        // Gibt true zurück, wenn die letzte Prüfung noch gülig ist
        static bool CheckForLastError(ref DateTime nextCheckUtc, ref string lastMessage) {
            if (DateTime.UtcNow.Subtract(nextCheckUtc).TotalSeconds < 0) { return true; }
            lastMessage = string.Empty;
            nextCheckUtc = DateTime.UtcNow.AddSeconds(5);
            return false;
        }
    }

    public void EnableScript() => Column.GenerateAndAddSystem("SYS_ROWSTATE");

    public void EventScript_Add(DatabaseScriptDescription ev, bool isLoading) {
        _eventScript.Add(ev);
        ev.PropertyChanged += EventScript_PropertyChanged;

        if (!isLoading) { EventScript_PropertyChanged(this, System.EventArgs.Empty); }

        foreach (var thisCom in Script.Commands) {
            if (thisCom.Verwendung.Count < 3) {
                if (ev.ScriptText.ContainsWord(thisCom.Command + thisCom.StartSequence, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
                    thisCom.Verwendung.AddIfNotExists($"Datenbank: {Caption} / {ev.KeyName}");
                    if (thisCom.LastArgMinCount == 3) {
                        thisCom.Verwendung.Add("[WEITERE VERWENDUNGEN VORHANDEN]");
                    }
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="s"></param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="attributes"></param>
    /// <param name="dbVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns></returns>

    public ScriptEndedFeedback ExecuteScript(DatabaseScriptDescription s, bool produktivphase, RowItem? row, List<string>? attributes, bool dbVariables, bool extended) {
        if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, false, s.KeyName); }
        if (!string.IsNullOrEmpty(FreezedReason)) { return new ScriptEndedFeedback("Datenbank eingefroren: " + FreezedReason, false, false, s.KeyName); }

        var sce = CheckScriptError();
        if (!string.IsNullOrEmpty(sce)) { return new ScriptEndedFeedback("Die Skripte enthalten Fehler: " + sce, false, true, "Allgemein"); }

        //if (ExecutingScript > 0) {
        //    return new ScriptEndedFeedback("Aktuell wird bereits ein Skript ausgeführt" + sce, false, false, "Allgemein");
        //}
        ExecutingScript++;
        ExecutingScriptAnyDatabase++;
        try {
            var rowstamp = string.Empty;

            object addinfo = this;
            if (row != null && !row.IsDisposed) {
                rowstamp = row.RowStamp();
                addinfo = row;
            }

            #region  Erlaubte Methoden ermitteln und maxtime

            var allowedMethods = MethodType.Standard | MethodType.Database | MethodType.SpecialVariables | MethodType.IO;
            float maxtime = 60 * 60;

            if (row != null && !row.IsDisposed) { allowedMethods |= MethodType.MyDatabaseRow; }

            if (s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula) ||
                s.EventTypes.HasFlag(ScriptEventTypes.InitialValues) ||
                s.EventTypes.HasFlag(ScriptEventTypes.export) ||
                (s.EventTypes.HasFlag(ScriptEventTypes.value_changed) && !extended)) {
                maxtime = 10;
            }

            if (s.EventTypes.HasFlag(ScriptEventTypes.loaded) ||
                s.EventTypes.HasFlag(ScriptEventTypes.row_deleting)) {
                maxtime = 20;
            }

            if (!s.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.loaded) &&
                 !s.EventTypes.HasFlag(ScriptEventTypes.value_changed)) {
                allowedMethods |= MethodType.ManipulatesUser;
            }

            if (extended) {
                allowedMethods |= MethodType.ManipulatesUser;
                allowedMethods |= MethodType.ChangeAnyDatabaseOrRow;
            }

            if (s.ChangeValues && produktivphase) { allowedMethods |= MethodType.ChangeAnyDatabaseOrRow; }

            #endregion

            var prepf = s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);

            bool? extv = null;
            if (s.EventTypes.HasFlag(ScriptEventTypes.value_changed)) { extv = extended; }

            var vars = CreateVariableCollection(row, !s.ChangeValues, prepf, dbVariables, prepf, extv);

            #region Script ausführen

            var scp = new ScriptProperties(s.KeyName, allowedMethods, produktivphase, s.Attributes(), addinfo);

            Script sc = new(vars, AdditionalFilesPfadWhole(), scp) {
                ScriptText = s.ScriptText
            };

            var st = new Stopwatch();
            st.Start();

            var scf = sc.Parse(0, s.KeyName, attributes);

            #endregion

            #region Fehlerprüfungen

            if (st.ElapsedMilliseconds > maxtime * 1000) {
                ExecutingScript--;
                ExecutingScriptAnyDatabase--;
                return new ScriptEndedFeedback("Das Skript hat eine zu lange Laufzeit.", false, true, s.KeyName);
            }

            if (!scf.AllOk) {
                ExecutingScript--;
                ExecutingScriptAnyDatabase--;
                OnDropMessage(FehlerArt.Info, "Das Skript '" + s.KeyName + "' hat einen Fehler verursacht\r\n" + scf.Protocol[0]);
                return scf;
            }

            if (row != null) {
                if (row.IsDisposed) {
                    ExecutingScript--;
                    ExecutingScriptAnyDatabase--;
                    return new ScriptEndedFeedback("Die geprüfte Zeile wurde verworden", false, false, s.KeyName);
                }

                if (Column.SysRowChangeDate is null) {
                    ExecutingScript--;
                    ExecutingScriptAnyDatabase--;
                    return new ScriptEndedFeedback("Zeilen können nur geprüft werden, wenn Änderungen der Zeile geloggt werden.", false, false, s.KeyName);
                }

                if (row.RowStamp() != rowstamp) {
                    ExecutingScript--;
                    ExecutingScriptAnyDatabase--;
                    return new ScriptEndedFeedback("Zeile wurde während des Skriptes verändert.", false, false, s.KeyName);
                }
            }

            if (!produktivphase) {
                ExecutingScript--;
                ExecutingScriptAnyDatabase--;
                return scf;
            }

            #endregion

            #region Variablen zurückschreiben

            if (s.ChangeValues) {
                if (row != null && !row.IsDisposed) {
                    foreach (var thisCol in Column) {
                        row.VariableToCell(thisCol, vars, s.KeyName);
                    }
                }
                if (dbVariables) {
                    Variables = VariableCollection.Combine(Variables, vars, "DB_");
                }
            }

            #endregion

            ExecutingScript--;
            ExecutingScriptAnyDatabase--;
            return scf;
        } catch {
            Develop.CheckStackForOverflow();
            ExecutingScript--;
            ExecutingScriptAnyDatabase--;
            return ExecuteScript(s, produktivphase, row, attributes, dbVariables, extended);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventname"></param>
    /// <param name="scriptname"></param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="attributes"></param>
    /// <param name="dbVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns></returns>

    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string? scriptname, bool produktivphase, RowItem? row, List<string>? attributes, bool dbVariables, bool extended) {
        try {
            if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, false, "Allgemein"); }

            var e = new CancelReasonEventArgs();
            OnCanDoScript(e);
            if (e.Cancel) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + e.CancelReason, false, false, "Allgemein"); }

            var m = EditableErrorReason(EditableErrorReasonType.EditCurrently);

            if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + m, false, false, "Allgemein"); }

            #region Script ermitteln

            if (eventname != null && !string.IsNullOrEmpty(scriptname)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback("Weder Eventname noch Skriptname angekommen", false, false, "Allgemein"); }

            if (string.IsNullOrEmpty(scriptname) && eventname != null) {
                var l = EventScript.Get((ScriptEventTypes)eventname);
                if (l.Count == 1) { scriptname = l[0].KeyName; }
                if (string.IsNullOrEmpty(scriptname)) {
                    // Script nicht definiert. Macht nix. ist eben keines gewünscht

                    if (eventname == ScriptEventTypes.export) {
                        var vars = CreateVariableCollection(row, false, false, dbVariables, true, false);
                        return new ScriptEndedFeedback(vars, new List<string>(), true, false, false, true);
                    }

                    return new ScriptEndedFeedback();
                }
            }

            if (scriptname == null || string.IsNullOrWhiteSpace(scriptname)) { return new ScriptEndedFeedback("Kein Skriptname angekommen", false, false, "Allgemein"); }

            var script = EventScript.Get(scriptname);

            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }

            if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, scriptname); }

            if (!script.NeedRow) { row = null; }

            #endregion

            return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended);
        } catch {
            Develop.CheckStackForOverflow();
            return ExecuteScript(eventname, scriptname, produktivphase, row, attributes, dbVariables, extended);
        }
    }

    public string Export_CSV(FirstRow firstRow, IEnumerable<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows) {
        var columnListtmp = columnList?.ToList();
        columnListtmp ??= Column.Where(thisColumnItem => thisColumnItem != null).ToList();

        StringBuilder sb = new();
        switch (firstRow) {
            case FirstRow.Without:
                break;

            case FirstRow.ColumnCaption:
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] != null) {
                        var tmp = columnListtmp[colNr].ReadableText();
                        tmp = tmp.Replace(";", "|");
                        tmp = tmp.Replace(" |", "|");
                        tmp = tmp.Replace("| ", "|");
                        _ = sb.Append(tmp);
                        if (colNr < columnListtmp.Count - 1) { _ = sb.Append(";"); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            case FirstRow.ColumnInternalName:
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] != null) {
                        _ = sb.Append(columnListtmp[colNr].KeyName);
                        if (colNr < columnListtmp.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }

        var (_, errormessage) = RefreshRowData(sortedRows);
        if (!string.IsNullOrEmpty(errormessage)) {
            OnDropMessage(FehlerArt.Fehler, errormessage);
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow != null && !thisRow.IsDisposed) {
                for (var colNr = 0; colNr < columnListtmp.Count; colNr++) {
                    if (columnListtmp[colNr] != null) {
                        var tmp = Cell.GetString(columnListtmp[colNr], thisRow);
                        tmp = tmp.Replace("\r\n", "|");
                        tmp = tmp.Replace("\r", "|");
                        tmp = tmp.Replace("\n", "|");
                        tmp = tmp.Replace(";", "<sk>");
                        _ = sb.Append(tmp);
                        if (colNr < columnListtmp.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
            }
        }
        return sb.ToString().TrimEnd("\r\n");
    }

    public string Export_CSV(FirstRow firstRow, ColumnViewCollection? arrangement, IEnumerable<RowItem> sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionalFilesPfadWhole() + _standardFormulaFile)) { return AdditionalFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    /// <summary>
    /// Friert die Datenbank komplett ein, nur noch Ansicht möglich.
    /// Setzt auch ReadOnly.
    /// </summary>
    /// <param name="reason"></param>
    public void Freeze(string reason) {
        SetReadOnly();
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    public List<string> GetAllLayoutsFileNames() {
        List<string> path = [];
        var r = new List<string>();
        if (!IsDisposed) {
            path.Add(DefaultLayoutPath());
            if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { path.Add(AdditionalFilesPfadWhole()); }
        }

        foreach (var thisP in path) {
            if (DirectoryExists(thisP)) {
                var e = Directory.GetFiles(thisP);
                foreach (var thisFile in e) {
                    if (thisFile.FileType() is FileFormat.HTML or FileFormat.Textdocument or FileFormat.Visitenkarte or FileFormat.BlueCreativeFile or FileFormat.XMLFile) {
                        r.Add(thisFile);
                    }
                }
            }
        }
        return r;
    }

    public Database? GetOtherTable(string tablename, bool readOnly) {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Ungültiger Tabellenname: " + tablename);
            return null;
        }

        var x = ConnectionDataOfOtherTable(tablename, true);
        if (x == null) { return null; }

        x.Provider = null;  // KEINE Vorage mitgeben, weil sonst eine Endlosschleife aufgerufen wird!

        return GetById(x, readOnly, null, true);// new DatabaseSQL(_sql, readOnly, tablename);
    }

    public string ImportBdb(List<string> files, ColumnItem? colForFilename, bool deleteImportet) {
        foreach (var thisFile in files) {
            var db = GetByFilename(thisFile, true, null, false, "Import");
            if (db == null) {
                return thisFile + " konnte nicht geladen werden.";
            }

            foreach (var thisr in db.Row) {
                var r = Row.GenerateAndAdd("Dummy", null, "BDB Import");

                if (r == null) { return "Zeile konnte nicht generiert werden."; }

                foreach (var thisc in db.Column) {
                    if (thisc != colForFilename) {
                        var c = Column[thisc.KeyName];
                        if (c == null) {
                            c = Column.GenerateAndAdd(thisc.KeyName, thisc.Caption, string.Empty, null, string.Empty);
                            if (c == null) { return "Spalte konnte nicht generiert werden."; }
                            c.CloneFrom(thisc, false);
                        }

                        var w = thisr.CellGetString(thisc);
                        r.CellSet(c, w, "Import von " + thisFile);
                        if (r.CellGetString(c) != w) { return "Setzungsfehler!"; }
                    }
                }

                if (colForFilename != null) {
                    r.CellSet(colForFilename, thisFile, "Dateiname, Import von " + thisFile);

                    if (r.CellGetString(colForFilename) != thisFile) { return "Setzungsfehler!"; }
                }
            }

            if (deleteImportet) {
                Save();
                if (HasPendingChanges) { return "Speicher-Fehler!"; }
                db.Dispose();
                var d = DeleteFile(thisFile, false);
                if (!d) { return "Lösch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public string ImportCsv(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        if (!Row.IsNewRowPossible()) {
            OnDropMessage(FehlerArt.Warnung, "Abbruch, Datenbank unterstützt keine neuen Zeilen.");
            return "Abbruch, Datenbank unterstützt keine neuen Zeilen.";
        }

        #region Text vorbereiten

        importText = importText.Replace("\r\n", "\r").Trim("\r");

        #endregion

        #region Die Zeilen (zeil) vorbereiten

        var ein = importText.SplitAndCutByCr();
        List<string[]> zeil = [];
        var neuZ = 0;
        for (var z = 0; z <= ein.GetUpperBound(0); z++) {
            if (eliminateMultipleSplitter) {
                ein[z] = ein[z].Replace(splitChar + splitChar, splitChar);
            }
            if (eleminateSplitterAtStart) {
                ein[z] = ein[z].TrimStart(splitChar);
            }
            ein[z] = ein[z].TrimEnd(splitChar);
            zeil.Add(ein[z].SplitAndCutBy(splitChar));
        }

        if (zeil.Count == 0) {
            OnDropMessage(FehlerArt.Warnung, "Abbruch, keine Zeilen zum Importieren erkannt.");
            return "Abbruch, keine Zeilen zum Importieren erkannt.";
        }

        #endregion

        #region Spaltenreihenfolge (columns) ermitteln

        List<ColumnItem> columns = [];
        var startZ = 0;

        if (spalteZuordnen) {
            startZ = 1;
            for (var spaltNo = 0; spaltNo < zeil[0].GetUpperBound(0) + 1; spaltNo++) {
                if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, leerer Spaltenname.");
                    return "Abbruch,<br>leerer Spaltenname.";
                }
                zeil[0][spaltNo] = ColumnItem.MakeValidColumnName(zeil[0][spaltNo]);

                var col = Column[zeil[0][spaltNo]];
                if (col == null) {
                    if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                        OnDropMessage(FehlerArt.Warnung, "Abbruch, ungültiger Spaltenname.");
                        return "Abbruch,<br>ungültiger Spaltenname.";
                    }

                    col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                    if (col != null) {
                        col.Caption = zeil[0][spaltNo];
                        col.Function = ColumnFunction.Normal;
                    }
                }

                if (col == null) {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, Spaltenfehler.");
                    return "Abbruch,<br>Spaltenfehler.";
                }

                columns.Add(col);
            }
        } else {
            columns.AddRange(Column.Where(thisColumn => thisColumn != null && !thisColumn.IsSystemColumn()));
            while (columns.Count < zeil[0].GetUpperBound(0) + 1) {
                var newc = Column.GenerateAndAdd();
                if (newc != null) {
                    newc.Caption = newc.KeyName;
                    newc.Function = ColumnFunction.Normal;
                    newc.MultiLine = true;
                }

                if (newc == null) {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, Spaltenfehler.");
                    return "Abbruch, Spaltenfehler.";
                }
                columns.Add(newc);
            }
        }

        #endregion

        #region Neue Werte in ein Dictionary schreiben (dictNeu)

        var dictNeu = new Dictionary<string, string[]>();

        for (var rowNo = startZ; rowNo < zeil.Count; rowNo++) {
            if (zeileZuordnen) {
                if (zeil[rowNo].GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(zeil[rowNo][0]) && !dictNeu.ContainsKey(zeil[rowNo][0].ToUpperInvariant())) {
                    dictNeu.Add(zeil[rowNo][0].ToUpperInvariant(), zeil[rowNo]);
                }
                //else {
                //    OnDropMessage(FehlerArt.Warnung, "Abbruch, eingehende Werte können nicht eindeutig zugeordnet werden.");
                //    return "Abbruch, eingehende Werte können nicht eindeutig zugeordnet werden.";
                //}
            } else {
                dictNeu.Add(rowNo.ToString(), zeil[rowNo]);
            }
        }

        #endregion

        #region Zeilen, die BEACHTET werden sollen, in ein Dictionary schreiben (dictVorhanden)

        var dictVorhanden = new Dictionary<string, RowItem>();

        if (zeileZuordnen) {
            foreach (var thisR in Row) {
                var f = thisR.CellFirstString().ToUpperInvariant();
                if (!string.IsNullOrEmpty(f) && !dictVorhanden.ContainsKey(f)) {
                    dictVorhanden.Add(f, thisR);
                } else {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, vorhandene Zeilen der Datenbank '" + Caption + "' sind nicht eindeutig.");
                    return "Abbruch, vorhandene Zeilen sind nicht eindeutig.";
                }
            }
        }

        #endregion

        #region Der eigentliche Import

        var d1 = DateTime.Now;
        var d2 = DateTime.Now;

        var no = 0;
        foreach (var thisD in dictNeu) {
            no++;

            #region Spaltenanzahl zum Import ermitteln (maxColCount)

            var maxColCount = Math.Min(thisD.Value.GetUpperBound(0) + 1, columns.Count);

            if (maxColCount == 0) {
                OnDropMessage(FehlerArt.Warnung, "Abbruch, Leere Zeile im Import.");
                return "Abbruch, Leere Zeile im Import.";
            }

            #endregion

            #region Row zum schreiben ermitteln (row)

            RowItem? row;

            if (dictVorhanden.ContainsKey(thisD.Key)) {
                dictVorhanden.TryGetValue(thisD.Key, out row);
                dictVorhanden.Remove(thisD.Key); // Speedup
            } else {
                neuZ++;
                row = Row.GenerateAndAdd(thisD.Value[0], null, "Import, fehlende Zeile");
            }

            if (row == null) {
                OnDropMessage(FehlerArt.Warnung, "Abbruch, Import-Fehler.");
                return "Abbruch, Import-Fehler.";
            }

            #endregion

            #region Werte in die Spalten schreiben

            for (var colNo = 0; colNo < maxColCount; colNo++) {
                row.CellSet(columns[colNo], thisD.Value[colNo].SplitAndCutBy("|").JoinWithCr(), "CSV-Import");
            }

            #endregion

            #region Speichern und Ausgabe

            if (DateTime.Now.Subtract(d1).TotalMinutes > 5) {
                OnDropMessage(FehlerArt.Info, "Import: Zwischenspeichern der Datenbank");
                Save();
                d1 = DateTime.Now;
            }

            if (DateTime.Now.Subtract(d2).TotalSeconds > 4.5) {
                OnDropMessage(FehlerArt.Info, "Import: Zeile " + no + " von " + zeil.Count);
                d2 = DateTime.Now;
            }
            Develop.SetUserDidSomething();

            #endregion
        }

        #endregion

        Save();
        OnDropMessage(FehlerArt.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains(Everybody, false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    public bool IsRowScriptPossible(bool checkMessageTo) {
        if (Column.SysRowChangeDate == null) { return false; }
        if (Column.SysRowState == null) { return false; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; } // Nicht ReadOnly!
        if (checkMessageTo && !string.IsNullOrEmpty(_scriptNeedFix)) { return false; }
        return true;
    }

    public virtual void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(FehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);
        if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                if (ReadOnly) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                    return;
                }
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                Freeze("Datei existiert nicht");
                return;
            }
        }
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        //LoadingEventArgs ec = new(_initialLoadDone);
        OnLoading();

        var bLoaded = LoadBytesFromDisk(EditableErrorReasonType.Load);
        if (bLoaded == null) { return; }

        Parse(bLoaded, needPassword);

        if (FileStateUTCDate.Year < 2000) {
            FileStateUTCDate = new DateTime(2000, 1, 1);
        }
        IsInCache = FileStateUTCDate;

        CheckSysUndoNow(new List<Database> { this }, true);

        RepairAfterParse();

        if (ronly) { SetReadOnly(); }
        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded();

        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null, true, false);

        TryToSetMeTemporaryMaster();
    }

    public void LoadFromStream(Stream stream) {
        OnLoading();
        byte[] bLoaded;
        using (BinaryReader r = new(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt() ?? bLoaded; }
        //if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
        //    // Gezipte Daten-Kennung gefunden
        //    bLoaded = MultiUserFile.UnzipIt(bLoaded);
        //}

        Parse(bLoaded, null);

        RepairAfterParse();
        Freeze("Stream-Datenbank");
        OnLoaded();
        //CreateWatcher();
        //_ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
    }

    public virtual string NextRowKey() {
        if (IsDisposed) { return string.Empty; }
        var tmp = 0;
        string key;

        do {
            key = GetUniqueKey(tmp, "row");
            tmp++;
        } while (Row.SearchByKey(key) != null);
        return key;
    }

    public void OnCanDoScript(CancelReasonEventArgs e) {
        if (IsDisposed) { return; }
        CanDoScript?.Invoke(this, e);
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

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

            if (thisColumn.Function is not ColumnFunction.Verknüpfung_zu_anderer_Datenbank and
                                      not ColumnFunction.Verknüpfung_zu_anderer_Datenbank2 and
                                      not ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems and
                                      not ColumnFunction.Button and
                                      not ColumnFunction.Virtuelle_Spalte) {
                var x = thisColumn.Contents();
                if (x.Count == 0) {
                    Column.Remove(thisColumn, "Automatische Optimierung");
                    Optimize();
                    return;
                }
            }
        }

        Column.GetSystems();

        if (Column.SysChapter is ColumnItem c) {
            var x = c.Contents();
            if (x.Count < 2) {
                Column.Remove(c, "Automatische Optimierung");
            }
        }
    }

    public List<string> Permission_AllUsedInThisDB(bool cellLevel) {
        List<string> e = [];
        foreach (var thisColumnItem in Column) {
            if (thisColumnItem != null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(_permissionGroupsNewRow);
        e.AddRange(_datenbankAdmin);
        foreach (var thisArrangement in _columnArrangements) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }

        foreach (var thisEv in EventScript) {
            e.AddRange(thisEv.UserGroups);
        }

        //foreach (var thisArrangement in OldFormulaViews) {
        //    e.AddRange(thisArrangement.PermissionGroups_Show);
        //}
        e.Add(Everybody);
        e.Add("#User: " + UserName);
        if (cellLevel) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.RemoveString(Administrator, false);
        if (!IsAdministrator()) { e.Add(UserGroup); }

        e = RepairUserGroups(e);
        return e.SortedDistinctList();
    }

    public bool PermissionCheck(IList<string>? allowed, RowItem? row) {
        try {
            if (IsAdministrator()) { return true; }
            if (PowerEdit.Subtract(DateTime.UtcNow).TotalSeconds > 0) { return true; }
            if (allowed == null || allowed.Count == 0) { return false; }
            if (allowed.Any(thisString => PermissionCheckWithoutAdmin(thisString, row))) {
                return true;
            }
        } catch (Exception ex) {
            Develop.DebugPrint(FehlerArt.Warnung, "Fehler beim Rechte-Check", ex);
        }
        return false;
    }

    //private void OnIsTableVisibleForUser(VisibleEventArgs e) {
    //    if (IsDisposed) { return; }
    //    IsTableVisibleForUser?.Invoke(this, e);
    //}
    public bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
        var tmpName = UserName.ToUpperInvariant();
        var tmpGroup = UserGroup.ToUpperInvariant();
        if (string.Equals(allowed, Everybody, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (Column.SysRowCreator is ColumnItem src && string.Equals(allowed, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase)) {
            if (row != null && Cell.GetString(src, row).ToUpperInvariant() == tmpName) { return true; }
        } else if (string.Equals(allowed, "#USER: " + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, "#USER:" + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, tmpGroup, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        return false;
    }

    public virtual void RefreshColumnsData(params ColumnItem[] columns) {
        if (columns.Length == 0) { return; }

        foreach (var thiscol in columns) {
            if (thiscol != null) {
                thiscol.IsInCache = DateTime.UtcNow;

                if (thiscol.LinkedDatabase is Database db && !db.IsDisposed &&
                    db.Column[thiscol.LinkedCell_ColumnNameOfLinkedDatabase] is ColumnItem col) {
                    db.RefreshColumnsData(col);
                }
            }
        }
    }

    public void RefreshColumnsData(params FilterItem[] filter) {
        if (filter != null) {
            var c = new List<ColumnItem>();

            foreach (var thisF in filter) {
                if (thisF.Column is ColumnItem ci && ci.IsInCache == null) {
                    _ = c.AddIfNotExists(ci);
                }
            }
            RefreshColumnsData(c.ToArray());
        }
    }

    public virtual (bool didreload, string errormessage) RefreshRowData(IEnumerable<RowItem> row) {
        var rowItems = row.ToList();
        if (!rowItems.Any()) { return (false, string.Empty); }

        foreach (var thisrow in rowItems) {
            thisrow.IsInCache = DateTime.UtcNow;
        }
        //var x = Row.DoLinkedDatabase(row);
        return (false, string.Empty);
    }

    public virtual void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten DatabaseMU nicht mehr funktioniert

        if (!string.IsNullOrEmpty(EditableErrorReason(this, EditableErrorReasonType.EditAcut))) { return; }

        Column.Repair();
        RepairColumnArrangements(Reason.SetCommand);

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(TableName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Tabellenname stimmt nicht: " + Filename);
            }
        }

        SortDefinition?.Repair();

        PermissionGroupsNewRow = RepairUserGroups(PermissionGroupsNewRow).AsReadOnly();
        DatenbankAdmin = RepairUserGroups(DatenbankAdmin).AsReadOnly();

        if (EventScriptVersion < 1) { EventScriptVersion = 1; }
    }

    public virtual bool Save() {
        if (_isInSave) { return false; }

        if (!HasPendingChanges) { return false; }

        _isInSave = true;
        var v = SaveInternal(FileStateUTCDate);
        _isInSave = false;
        OnInvalidateView();
        return v;
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
        _ = Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;

        var l = ToListOfByte(this, 100, FileStateUTCDate);

        if (l == null) { return; }

        using FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(l.ToArray(), 0, l.ToArray().Length);
        x.Flush();
        x.Close();
        OnInvalidateView();
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        return base.ToString() + " " + TableName;
    }

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + TableName;
            t += "\r\nID: " + ConnectionData.DatabaseId;
        } catch { }
        Develop.DebugPrint(FehlerArt.Warnung, t);
    }

    //    _variables.Clear();
    //    if (!isLoading) { Variables = new VariableCollection(); }
    //}
    internal bool IsNewRowPossible() => string.IsNullOrWhiteSpace(EditableErrorReason(EditableErrorReasonType.EditNormaly));

    //    //    _variables.RemoveAt(_variables.Count - 1);
    //    //}
    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    //public void Variables_RemoveAll(bool isLoading) {
    //    //while (_variables.Count > 0) {
    //    //    //var va = _variables[_eventScript.Count - 1];
    //    //    //ev.Changed -= EventScript_PropertyChanged;
    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
    }

    //public void Variables_Add(VariableString va, bool isLoading) {
    //    _variables.Add(va);
    //    if (!isLoading) { Variables = new VariableCollection(_variables); }
    //}
    internal void RefreshCellData(ColumnItem column, RowItem row, Reason reason) {
        if (reason is Reason.NoUndo_NoInvalidate or Reason.UpdateChanges or Reason.AdditionalWorkAfterCommand) { return; }

        if (column.IsInCache != null || row.IsInCache != null) { return; }

        var (_, errormessage) = RefreshRowData(row, false);
        if (!string.IsNullOrEmpty(errormessage)) {
            OnDropMessage(FehlerArt.Fehler, errormessage);
        }
    }

    //internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventArgs e) {
    //    if (IsDisposed) { return; }
    //    GenerateLayoutInternal?.Invoke(this, e);
    //}
    internal void RepairColumnArrangements(Reason reason) {
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Temporär erzeugt werden!

        var x = _columnArrangements.CloneWithClones();

        for (var z = 0; z < Math.Max(2, x.Count); z++) {
            if (x.Count < z + 1) { x.Add(new ColumnViewCollection(this, string.Empty)); }
            ColumnViewCollection.Repair(x[z], z);
        }

        if (reason is Reason.NoUndo_NoInvalidate or Reason.UpdateChanges or Reason.AdditionalWorkAfterCommand) {
            if (_columnArrangements.ToString(false) == x.ToString(false)) { return; }
            SetValueInternal(DatabaseDataType.ColumnArrangement, null, null, x.ToString(false), UserName, DateTime.UtcNow, reason);
        } else {
            ColumnArrangements = x.AsReadOnly();
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
    protected void AddUndo(DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == DatabaseDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(TableName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container));
    }

    protected virtual List<ConnectionInfo>? AllAvailableTables(List<Database>? allreadychecked, string mustBeFreezed) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db && !db.IsDisposed) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        //if (ignorePath != null) {
        //    foreach (var thisPf in ignorePath) {
        //        if (Filename.FilePath().StartsWith(thisPf, StringComparison.OrdinalIgnoreCase)) { return null; }
        //    }
        //}

        var nn = Directory.GetFiles(Filename.FilePath(), "*.bdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            var t = ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false);
            if (t != null) { gb.Add(t); }
        }
        return gb;
    }

    protected void CreateWatcher() {
        if (string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Save))) {
            _checker = new Timer(Checker_Tick);
            _ = _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        //base.Dispose(disposing); // speichert und löscht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
            OnDisposingEvent();
            Column.Dispose();
            //Cell?.Dispose();
            Row.Dispose();
        }
        _ = AllFiles.Remove(this);
        _checker?.Dispose();

        IsDisposed = true;

        if (disposing) {
            OnDisposed();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="files"></param>
    /// <param name="columnsAdded"></param>
    /// <param name="rowsAdded"></param>
    /// <param name="starttimeUtc">Nur um die Zeit stoppen zu können und lange Prozesse zu kürzen</param>
    protected virtual void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, DateTime starttimeUtc) {
        foreach (var thisro in rowsAdded) { thisro.IsInCache = DateTime.UtcNow; }
        foreach (var thisco in columnsAdded) { thisco.IsInCache = DateTime.UtcNow; }
    }

    protected virtual (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(IEnumerable<Database> db, DateTime fromUtc, DateTime toUtc) => ([], null);

    protected bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is Database db && !db.IsDisposed) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    _ = thisFile.Save();
                    Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    protected virtual List<Database> LoadedDatabasesWithSameServer() => [this];

    protected virtual bool NewMasterPossible() {
        if (ReadOnly) { return false; }
        if (!MultiUser) { return false; }

        if (DateTimeTryParse(TemporaryDatabaseMasterTimeUtc, out var c)) {
            if (DateTime.UtcNow.Subtract(c).TotalMinutes < 60) { return false; }
        }

        if (!IsAdministrator()) { return false; }

        // Skripte nicht abfragen! Sonst wird nie ein Master gewählt
        // und Änderungen verweilen für immer in den Fragmenten
        //if (!CanDoValueChangedScript()) { return false; }
        //if (HasValueChangedScript()) { return false; }

        if (RowCollection.WaitDelay > 90) { return true; }

        var masters = 0;
        foreach (var thisDb in Database.AllFiles) {
            if (!thisDb.IsDisposed && thisDb.AmITemporaryMaster(0, 45) && thisDb.MultiUser) {
                masters++;
                if (masters > 8) { return false; }
            }
        }

        return true;
    }

    protected void OnLoaded() {
        if (IsDisposed) { return; }
        //IsInCache = FileStateUTCDate;
        Loaded?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    protected bool SaveInternal(DateTime setfileStateUtcDateTo) {
        var m = EditableErrorReason(EditableErrorReasonType.Save);
        if (!string.IsNullOrEmpty(m)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return false; }

        Develop.SetUserDidSomething();
        var tmpFileName = WriteTempFileToDisk(setfileStateUtcDateTo);
        Develop.SetUserDidSomething();
        if (string.IsNullOrEmpty(tmpFileName)) { return false; }

        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return false; }
        }
        Develop.SetUserDidSomething();
        // Haupt-Datei wird zum Backup umbenannt
        if (!MoveFile(Filename, Backupdateiname(), false)) { return false; }

        if (FileExists(Filename)) {
            // Paralleler Prozess hat gespeichert?!?
            _ = DeleteFile(tmpFileName, false);
            return false;
        }

        // --- TmpFile wird zum Haupt ---
        _ = MoveFile(tmpFileName, Filename, true);
        Develop.SetUserDidSomething();
        HasPendingChanges = false;
        FileStateUTCDate = setfileStateUtcDateTo;
        return true;
    }

    protected void SetReadOnly() => ReadOnly = true;

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
    protected (string Error, ColumnItem? Columnchanged, RowItem? Rowchanged) SetValueInternal(DatabaseDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, Reason reason) {
        if (IsDisposed) { return ("Datenbank verworfen!", null, null); }
        if ((reason is not Reason.NoUndo_NoInvalidate and not Reason.UpdateChanges) && !string.IsNullOrEmpty(FreezedReason)) { return ("Datenbank eingefroren: " + FreezedReason, null, null); }
        if (type.IsObsolete()) { return (string.Empty, null, null); }

        LastChange = DateTime.UtcNow;

        if (type.IsCellValue()) {
            if (column?.Database is not Database db || db.IsDisposed) {
                //Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type);
                return (string.Empty, column, row);
            }

            if (row == null) {
                return (string.Empty, column, row);
            }

            column.Invalidate_ContentWidth();
            //row.InvalidateCheckData();

            var f = Cell.SetValueInternal(column, row, value, reason);

            if (!string.IsNullOrEmpty(f)) { return (f, null, null); }
            Cell.DoSystemColumns(db, column, row, user, datetimeutc, reason);

            return (string.Empty, column, row);
        }

        if (type.IsColumnTag()) {
            if (column == null || column.IsDisposed || Column.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type);
                //return ("Wert nicht gesetzt!", null, null);
                return (string.Empty, null, null);
            }
            column.Invalidate_ContentWidth();
            return (column.SetValueInternal(type, value), column, null);
        }

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Command_RemoveColumn:
                    var c = Column[value];
                    if (c == null) { return (string.Empty, null, null); }
                    return (Column.ExecuteCommand(type, c.KeyName, reason), c, null);

                case DatabaseDataType.Command_AddColumnByName:
                    var f2 = Column.ExecuteCommand(type, value, reason);
                    if (!string.IsNullOrEmpty(f2)) { return (f2, null, null); }

                    var thisColumn = Column[value];
                    if (thisColumn == null) { return ("Hinzufügen fehlgeschlagen", null, null); }

                    return (string.Empty, column, null);

                case DatabaseDataType.Command_RemoveRow:
                    var r = Row.SearchByKey(value);
                    if (r == null) { return (string.Empty, null, null); }
                    return (Row.ExecuteCommand(type, r.KeyName, reason), null, r);

                case DatabaseDataType.Command_AddRow:
                    var f1 = Row.ExecuteCommand(type, value, reason);
                    if (!string.IsNullOrEmpty(f1)) { return (f1, null, null); }
                    var thisRow = Row.SearchByKey(value);
                    return (string.Empty, null, thisRow);

                case DatabaseDataType.Command_NewStart:
                    return (string.Empty, null, null);

                default:
                    if (LoadedVersion == DatabaseVersion) {
                        Freeze("Ladefehler der Datenbank");
                        if (!ReadOnly) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData);
                        }
                    }
                    return ("Befehl unbekannt.", null, null);
            }
        }

        switch (type) {
            case DatabaseDataType.Formatkennung:
                break;

            case DatabaseDataType.Version:
                LoadedVersion = value.Trim();
                if (LoadedVersion != DatabaseVersion) {
                    Freeze("Versions-Konflikt");
                    LoadedVersion = value.Trim();
                } else {
                    //Cell.RemoveOrphans();
                    Row.RemoveNullOrEmpty();
                    Cell.Clear();
                }
                break;

            case DatabaseDataType.Werbung:
                break;

            case DatabaseDataType.Creator:
                _creator = value;
                break;

            case DatabaseDataType.FileStateUTCDate:
                FileStateUTCDate = DateTimeParse(value);
                break;

            case DatabaseDataType.CreateDateUTC:
                _createDate = value;
                break;

            case DatabaseDataType.TemporaryDatabaseMasterUser:
                _temporaryDatabaseMasterUser = value;
                break;

            case DatabaseDataType.TemporaryDatabaseMasterTimeUTC:
                _temporaryDatabaseMasterTimeUtc = value;
                break;

            case DatabaseDataType.DatabaseAdminGroups:
                _datenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.SortDefinition:
                _sortDefinition = new RowSortDefinition(this, value);
                break;

            case DatabaseDataType.Caption:
                _caption = value;
                break;

            case DatabaseDataType.GlobalScale:
                _globalScale = DoubleParse(value);
                break;

            case DatabaseDataType.AdditionalFilesPath:
                _additionalFilesPfad = value;
                break;

            case DatabaseDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case DatabaseDataType.RowQuickInfo:
                _zeilenQuickInfo = value;
                break;

            case DatabaseDataType.Tags:
                _tags.SplitAndCutByCr(value);
                break;

            case DatabaseDataType.EventScript:
                _eventScriptTmp = value;
                EventScript_RemoveAll(true);
                List<string> ai = [.. value.SplitAndCutByCr()];
                foreach (var t in ai) {
                    EventScript_Add(new DatabaseScriptDescription(this, t), true);
                }

                //CheckScriptError();
                break;

            case DatabaseDataType.DatabaseVariables:
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

            case DatabaseDataType.ColumnArrangement:
                _columnArrangements.Clear();
                List<string> ca = [.. value.SplitAndCutByCr()];
                foreach (var t in ca) {
                    _columnArrangements.Add(new ColumnViewCollection(this, t));
                }
                break;

            case DatabaseDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            //case (DatabaseDataType)67: //.RulesScript:
            //    //ConvertRules(value);
            //    //_rulesScript = value;
            //    break;

            case DatabaseDataType.EventScriptVersion:
                _eventScriptVersion = LongParse(value);
                break;

            case DatabaseDataType.ScriptNeedFix:
                _scriptNeedFix = value;
                break;

            case DatabaseDataType.UndoInOne:
                Undo.Clear();
                var uio = value.SplitAndCutByCr();
                for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                    UndoItem tmpWork = new(uio[z]);
                    Undo.Add(tmpWork);
                }
                break;

            case DatabaseDataType.EOF:
                return (string.Empty, null, null);

            default:
                // Variable type
                if (LoadedVersion == DatabaseVersion) {
                    Freeze("Ladefehler der Datenbank");
                    if (!ReadOnly) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData);
                    }
                }

                return ("Datentyp unbekannt.", null, null);
        }
        return (string.Empty, null, null);
    }

    /// <summary>
    /// Diese Routine darf nur aufgerufen werden, wenn die Daten der Datenbank von der Festplatte eingelesen wurden.
    /// </summary>
    protected void TryToSetMeTemporaryMaster() {
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 1) { return; }

        if (AmITemporaryMaster(0, 60)) { return; }

        if (!NewMasterPossible()) { return; }

        RowCollection.WaitDelay = 0;
        TemporaryDatabaseMasterUser = MyMasterCode;
        TemporaryDatabaseMasterTimeUtc = DateTime.UtcNow.ToString5();
    }

    protected virtual string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren!"; } // Sicherheitshalber!
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }

        HasPendingChanges = true;

        return string.Empty;
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 240) { return; }
        if (DateTime.UtcNow.Subtract(LastLoadUtc).TotalSeconds < 180) { return; }

        if (CriticalState()) { return; }
        CheckSysUndoNow(AllFiles, false);
    }

    private static bool CriticalState() {
        foreach (var thisDb in AllFiles) {
            if (!thisDb.IsDisposed) {
                //if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (thisDb.IsInCache.Year < 2000 && !string.IsNullOrEmpty(thisDb.Filename)) { return true; } // Irgend eine Datenbank wird aktuell geladen
            }
        }

        return false;
    }

    private static int NummerCode1(IReadOnlyList<byte> b, int pointer) => b[pointer];

    private static int NummerCode2(IReadOnlyList<byte> b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    private static int NummerCode3(IReadOnlyList<byte> b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(IReadOnlyList<byte> b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private static (int pointer, DatabaseDataType type, string value, string colName, string rowKey) Parse(byte[] bLoaded, int pointer) {
        var colName = string.Empty;
        var rowKey = string.Empty;
        string value;
        DatabaseDataType type;

        switch ((Routinen)bLoaded[pointer]) {
            //case Routinen.CellFormat: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //var width = NummerCode2(bLoaded, pointer + 11 + les);
            //        //var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
            //        pointer += 11 + les + 4;
            //        break;
            //    }
            //case Routinen.CellFormatUTF8: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //var width = NummerCode2(bLoaded, pointer + 11 + les);
            //        // var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
            //        pointer += 11 + les + 4;
            //        break;
            //    }
            //case Routinen.CellFormatUTF8_V400: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode7(bLoaded, pointer + 5);
            //        rowKey = NummerCode7(bLoaded, pointer + 12);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 19, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //var  width = NummerCode2(bLoaded, pointer + 19 + les);
            //        //var  height = NummerCode2(bLoaded, pointer + 19 + les + 2);
            //        pointer += 19 + les + 4;
            //        break;
            //    }
            case Routinen.CellFormatUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    rowKey = NummerCode7(bLoaded, pointer + 5).ToString();
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 12, b, 0, les);
                    value = b.ToStringUtf8();
                    pointer += 12 + les;
                    //colKey = -1;
                    break;
                }

            //case Routinen.DatenAllgemein: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = -1;
            //        rowKey = -1;
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 5, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 5 + les;
            //        break;
            //    }
            case Routinen.DatenAllgemeinUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    rowKey = string.Empty;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointer += 5 + les;
                    break;
                }
            //case Routinen.Column: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 11 + les;
            //        break;
            //    }
            //case Routinen.ColumnUTF8: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 11 + les;
            //        break;
            //    }
            //case Routinen.ColumnUTF8_V400: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode7(bLoaded, pointer + 5);
            //        rowKey = NummerCode7(bLoaded, pointer + 12);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 19, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 19 + les;
            //        break;
            //    }
            case Routinen.ColumnUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];

                    var cles = NummerCode1(bLoaded, pointer + 2);
                    var cb = new byte[cles];
                    Buffer.BlockCopy(bLoaded, pointer + 3, cb, 0, cles);
                    colName = cb.ToStringUtf8();

                    var les = NummerCode3(bLoaded, pointer + 3 + cles);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 6 + cles, b, 0, les);
                    value = b.ToStringUtf8();

                    pointer += 6 + les + cles;
                    break;
                }

            case Routinen.CellFormatUTF8_V402: {
                    type = DatabaseDataType.UTF8Value_withoutSizeData;

                    var lenghtRowKey = NummerCode1(bLoaded, pointer + 1);
                    var rowKeyByte = new byte[lenghtRowKey];
                    Buffer.BlockCopy(bLoaded, pointer + 2, rowKeyByte, 0, lenghtRowKey);
                    rowKey = rowKeyByte.ToStringUtf8();

                    var lenghtValue = NummerCode2(bLoaded, pointer + 2 + lenghtRowKey);
                    var valueByte = new byte[lenghtValue];
                    Buffer.BlockCopy(bLoaded, pointer + 2 + lenghtRowKey + 2, valueByte, 0, lenghtValue);
                    value = valueByte.ToStringUtf8();

                    pointer += 2 + lenghtRowKey + 2 + lenghtValue;

                    break;
                }

            default: {
                    type = 0;
                    value = string.Empty;
                    //width = 0;
                    //height = 0;
                    Develop.DebugPrint(FehlerArt.Fehler, $"Laderoutine nicht definiert: {bLoaded[pointer]}");
                    break;
                }
        }

        return (pointer, type, value, colName, rowKey);
    }

    private static void SaveToByteList(ColumnItem c, ref List<byte> l) {
        if (c.Database is not Database db || db.IsDisposed) { return; }

        var name = c.KeyName;

        SaveToByteList(l, DatabaseDataType.ColumnName, c.KeyName, name);
        SaveToByteList(l, DatabaseDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(l, DatabaseDataType.ColumnFunction, ((int)c.Function).ToString(), name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(l, DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        //SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(l, DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.RoundAfterEdit, c.RoundAfterEdit.ToString(), name);
        SaveToByteList(l, DatabaseDataType.MaxCellLenght, c.MaxCellLenght.ToString(), name);
        SaveToByteList(l, DatabaseDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(l, DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        //SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(l, DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(l, DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.EditableWithTextInput, c.TextBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.TextFormatingAllowed, c.FormatierungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(l, DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(l, DatabaseDataType.LineStyleLeft, ((int)c.LineLeft).ToString(), name);
        SaveToByteList(l, DatabaseDataType.LineStyleRight, ((int)c.LineRight).ToString(), name);
        SaveToByteList(l, DatabaseDataType.EditableWithDropdown, c.DropdownBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.RegexCheck, c.Regex, name);
        SaveToByteList(l, DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownAllesAbwählenErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(l, DatabaseDataType.ColumnAdminInfo, c.AdminInfo, name);
        //SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(l, DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(l, DatabaseDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(l, DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), name);
        SaveToByteList(l, DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.ColumnTags, c.Tags.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.Suffix, c.Suffix, name);
        SaveToByteList(l, DatabaseDataType.LinkedDatabase, c.LinkedDatabaseTableName, name);
        SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(l, DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(l, DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(l, DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        SaveToByteList(l, DatabaseDataType.Prefix, c.Prefix, name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(l, DatabaseDataType.ColumnNameOfLinkedDatabase, c.LinkedCell_ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(l, DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(l, DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, column.TimeCode, key);

        if (c.Function != ColumnFunction.Virtuelle_Spalte) {
            foreach (var thisR in db.Row) {
                SaveToByteList(l, c, thisR);
            }
        }
    }

    private static void SaveToByteList(List<byte> list, ColumnItem column, RowItem row) {
        if (column.Database is not Database db || db.IsDisposed) { return; }

        var cellContent = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        list.Add((byte)Routinen.CellFormatUTF8_V402);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(list, rowKeyByte.Length, 1);
        list.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(list, cellContentByte.Length, 2);
        list.AddRange(cellContentByte);
    }

    private static void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content, string columnname) {
        list.Add((byte)Routinen.ColumnUTF8_V401);
        list.Add((byte)databaseDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(list, n.Length, 1);
        list.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(list, b.Length, 3);
        list.AddRange(b);
    }

    private static void SaveToByteList(List<byte> list, ColumnCollection c) {
        //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
        foreach (var columnitem in c) {
            if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName)) {
                SaveToByteList(columnitem, ref list);
            }
        }
    }

    private static void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content) {
        var b = content.UTF8_ToByte();
        list.Add((byte)Routinen.DatenAllgemeinUTF8);
        list.Add((byte)databaseDataType);
        SaveToByteList(list, b.Length, 3);
        list.AddRange(b);
    }

    private static void SaveToByteList(ICollection<byte> list, long numberToAdd, int byteCount) {
        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            // ReSharper disable once PossibleLossOfFraction
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            list.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);
    }

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    private void Checker_Tick(object state) {
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        var e = new CancelReasonEventArgs();
        OnCanDoScript(e);
        if (e.Cancel) { return; }

        RowCollection.ExecuteValueChangedEvent();

        if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 10) { return; }
        if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 3) { return; }

        if (!string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Save))) { return; }
        if (!LogUndo) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (!HasPendingChanges) {
            _checkerTickCount = 0;
            return;
        }

        if (HasPendingChanges &&
            ((_checkerTickCount > 20 && DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds > 20) ||
            _checkerTickCount > 180)) {
            if (!string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Save))) { return; }

            _ = Save();
            _checkerTickCount = 0;
        }
    }

    private void Column_ColumnDisposed(object sender, ColumnEventArgs e) {
        if (IsDisposed) { return; }
        RepairAfterParse();
    }

    private void Column_ColumnRemoving(object sender, ColumnEventArgs e) {
        if (IsDisposed) { return; }
        RepairAfterParse();
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Backup und abschließenden \
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Forms und abschließenden \
    /// </summary>
    /// <returns></returns>
    private string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="files"></param>
    /// <param name="data"></param>
    /// <param name="toUtc"></param>
    /// <param name="startTimeUtc">Nur um die Zeot stoppen zu können und lange Prozesse zu kürzen</param>
    private void DoLastChanges(List<string>? files, List<UndoItem>? data, DateTime toUtc, DateTime startTimeUtc) {
        if (data == null) { return; }
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        if (!string.IsNullOrEmpty(Filename) && IsInCache.Year < 2000) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        data = data.OrderBy(obj => obj.DateTimeUtc).ToList();

        try {
            List<ColumnItem> columnsAdded = [];
            List<RowItem> rowsAdded = [];
            List<string> cellschanged = [];
            List<string> myfiles = [];

            if (files != null) {
                foreach (var thisf in files) {
                    if (thisf.Contains("\\" + TableName.ToUpperInvariant() + "-")) {
                        myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            DoingChanges++;
            foreach (var thisWork in data) {
                if (TableName == thisWork.TableName && thisWork.DateTimeUtc > IsInCache) {
                    Undo.Add(thisWork);
                    ChangesNotIncluded.Add(thisWork);

                    var c = Column[thisWork.ColName];
                    var r = Row.SearchByKey(thisWork.RowKey);
                    var (error, columnchanged, rowchanged) = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.UpdateChanges);

                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Datenbank-Fehler: " + error + " " + thisWork.ToString());
                        //Develop.DebugPrint(FehlerArt.Fehler, "Fehler beim Nachladen: " + Error + " / " + TableName);
                        DoingChanges--;
                        return;
                    }

                    if (c == null && columnchanged != null) { columnsAdded.AddIfNotExists(columnchanged); }
                    if (r == null && rowchanged != null) { rowsAdded.AddIfNotExists(rowchanged); }
                    if (rowchanged != null && columnchanged != null) { cellschanged.AddIfNotExists(CellCollection.KeyOfCell(c, r)); }
                }
            }
            DoingChanges--;
            IsInCache = toUtc;
            DoWorkAfterLastChanges(myfiles, columnsAdded, rowsAdded, startTimeUtc);
            OnInvalidateView();
        } catch {
            Develop.CheckStackForOverflow();
            DoLastChanges(files, data, toUtc, startTimeUtc);
        }
    }

    private void EventScript_PropertyChanged(object sender, System.EventArgs e) => EventScript = _eventScript.AsReadOnly();

    private void EventScript_RemoveAll(bool isLoading) {
        while (_eventScript.Count > 0) {
            var ev = _eventScript[_eventScript.Count - 1];
            ev.PropertyChanged -= EventScript_PropertyChanged;

            _eventScript.RemoveAt(_eventScript.Count - 1);
        }

        if (!isLoading) { EventScript_PropertyChanged(this, System.EventArgs.Empty); }
    }

    private void GenerateTimer() {
        if (_pendingChangesTimer != null) { return; }
        _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
        _pendingChangesTimer = new Timer(CheckSysUndo);
        _ = _pendingChangesTimer.Change(10000, 10000);
    }

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private byte[]? LoadBytesFromDisk(EditableErrorReasonType checkmode) {
        var startTime = DateTime.UtcNow;
        byte[]? bLoaded;
        while (true) {
            try {
                var f = EditableErrorReason(checkmode);
                if (string.IsNullOrEmpty(f)) {
                    //var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                    bLoaded = File.ReadAllBytes(Filename);

                    if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt(); }

                    //tmpLastSaveCode2 = GetFileInfo(Filename, true);
                    //if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }
                    //f = "Datei wurde während des Ladens verändert.";
                    break;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 20) {
                    Develop.DebugPrint(FehlerArt.Info, f + "\r\n" + Filename);
                }

                Pause(0.5, false);
            } catch (Exception ex) {
                // Home Office kann lange blokieren....
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 300) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    return null;
                }
            }
        }

        if (bLoaded != null && bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = bLoaded.UnzipIt();
        }
        return bLoaded;
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void Parse(byte[] data, NeedPassword? needPassword) {
        var pointer = 0;
        var columnUsed = new List<ColumnItem>();
        Undo.Clear();
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
                        row = Row.SearchByKey(rowKey);
                        if (row == null || row.IsDisposed) {
                            _ = Row.ExecuteCommand(DatabaseDataType.Command_AddRow, rowKey, Reason.NoUndo_NoInvalidate);
                            row = Row.SearchByKey(rowKey);
                        }

                        if (row == null || row.IsDisposed) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Zeile hinzufügen Fehler");
                            Freeze("Zeile hinzufügen Fehler");
                            return;
                        }

                        row.IsInCache = DateTime.UtcNow;
                    }

                    #endregion

                    #region Spalte suchen oder erstellen

                    //if (colKey > -1 && string.IsNullOrEmpty(columname)) {
                    //    column = db.Column.SearchByKey(colKey);
                    //    if (Column  ==null || Column .IsDisposed) {
                    //        if (art != DatabaseDataType.ColumnName) { Develop.DebugPrint(art + " an erster Stelle!"); }
                    //        _ = db.Column.SetValueInternal(DatabaseDataType.Command_AddColumnByKey, true, string.Empty);
                    //        column = db.Column.SearchByKey(colKey);
                    //    }
                    //    if (Column  ==null || Column .IsDisposed) {
                    //        Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                    //        db.SetReadOnly();
                    //        return;
                    //    }
                    //    column.IsInCache = DateTime.UtcNow;
                    //    columnUsed.Add(column);
                    //}

                    if (!string.IsNullOrEmpty(columname)) {
                        column = Column[columname];
                        if (column == null || column.IsDisposed) {
                            if (command != DatabaseDataType.ColumnName) {
                                Develop.DebugPrint(command + " an erster Stelle!");
                            }

                            _ = Column.ExecuteCommand(DatabaseDataType.Command_AddColumnByName, columname, Reason.NoUndo_NoInvalidate);
                            column = Column[columname];
                        }

                        if (column == null || column.IsDisposed) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                            Freeze("Spalte hinzufügen Fehler");
                            return;
                        }

                        column.IsInCache = DateTime.UtcNow;
                        columnUsed.Add(column);
                    }

                    #endregion

                    #region Bei verschlüsselten Datenbanken das Passwort abfragen

                    if (command == DatabaseDataType.GlobalShowPass && !string.IsNullOrEmpty(value)) {
                        var pwd = string.Empty;

                        if (needPassword != null) {
                            pwd = needPassword();
                        }

                        if (pwd != value) {
                            Freeze("Passwort falsch");
                            break;
                        }
                    }

                    #endregion

                    if (command == DatabaseDataType.EOF) {
                        break;
                    }

                    var fehler = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, Reason.NoUndo_NoInvalidate);
                    if (!string.IsNullOrEmpty(fehler.Error)) {
                        Freeze("Datenbank-Ladefehler");
                        Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + TableName + "<br>Meldung: " + fehler);
                    }
                }
            } while (true);
        } catch {
            Freeze("Parse Fehler!");
        }

        #region unbenutzte (gelöschte) Spalten entfernen

        var l = new List<ColumnItem>();
        foreach (var thisColumn in Column) {
            l.Add(thisColumn);
        }

        foreach (var thisColumn in l) {
            if (!columnUsed.Contains(thisColumn)) {
                _ = Column.ExecuteCommand(DatabaseDataType.Command_RemoveColumn, thisColumn.KeyName, Reason.NoUndo_NoInvalidate);
                //_ = SetValueInternal(DatabaseDataType.Command_RemoveColumn, thisColumn.KeyName, thisColumn, null, Reason.LoadReload, UserName, DateTime.UtcNow, "Parsen");
            }
        }

        #endregion

        Row.RemoveNullOrEmpty();
        Cell.RemoveOrphans();
        //Works?.AddRange(oldPendings);
        //oldPendings?.Clear();
        //ExecutePending();

        //if (db != null && db.Column.Count > 0 && string.IsNullOrEmpty(db.FirstColumn)) {
        //    db.FirstColumn = Col
        //}

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) { Freeze("Datenbankversions-Konflikt"); }
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            // Es werden alle Datenbanken abgefragt, also kann nach der ersten nicht schluss sein...

            if (string.IsNullOrWhiteSpace(AdditionalFilesPfadWhole())) { return; }

            var name = e.Name.RemoveChars(Char_DateiSonderZeichen);

            var fullname = CachePfad.TrimEnd("\\") + "\\" + name + ".PNG";

            if (FileExists(fullname) && Image_FromFile(fullname) is Bitmap bmp) {
                e.Done = true;
                e.Bmp = bmp;
            }
        } catch { }
    }

    private (bool didreload, string errormessage) RefreshRowData(RowItem row, bool refreshAlways) {
        if (!refreshAlways && row.IsInCache != null) { return (false, string.Empty); }

        return RefreshRowData(new List<RowItem> { row });
    }

    private string WriteTempFileToDisk(DateTime setfileStateUtcDateTo) {
        var f = EditableErrorReason(EditableErrorReasonType.Save);
        if (!string.IsNullOrEmpty(f)) { return string.Empty; }

        var dataUncompressed = ToListOfByte(this, 1200, setfileStateUtcDateTo)?.ToArray();

        if (dataUncompressed == null) { return string.Empty; }

        var datacompressed = dataUncompressed.ZipIt() ?? dataUncompressed;

        var tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        using FileStream x = new(tmpFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(datacompressed, 0, datacompressed.Length);
        x.Flush();
        x.Close();

        return tmpFileName;
    }

    #endregion
}