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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
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
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueBasics.Generic;
using Timer = System.Threading.Timer;
using static BlueBasics.Constants;
using System.Runtime.Remoting.Messaging;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatabaseAbstract : IDisposableExtendedWithEvent, IHasKeyName, ICanDropMessages {

    #region Fields

    public const string DatabaseVersion = "4.02";
    public static readonly ObservableCollection<DatabaseAbstract> AllFiles = new();
    public static List<Type>? DatabaseTypes;

    /// <summary>
    /// Wenn diese Varianble einen Count von 0 hat, ist der Speicher nicht initialisiert worden.
    /// </summary>
    public readonly List<UndoItem> Undo;

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfr�gt
    /// </summary>
    protected static Timer? _pendingChangesTimer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _pendingChangesTimer
    /// </summary>
    protected static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    /// <summary>
    ///  So viele �nderungen sind seit dem letzten erstellen der Komplett-Datenbank erstellen auf Festplatte gez�hlt worden
    /// </summary>
    protected readonly List<UndoItem> _changesNotIncluded = new();

    protected DateTime _fileStateUTCDate;
    private static bool _isInTimer;
    private static DateTime _lastTableCheck = new(1900, 1, 1);
    private readonly List<ColumnViewCollection> _columnArrangements = new();
    private readonly List<string> _datenbankAdmin = new();
    private readonly List<DatabaseScriptDescription> _eventScript = new();
    private readonly List<string> _permissionGroupsNewRow = new();
    private readonly List<string> _tags = new();
    private readonly List<Variable> _variables = new();
    private string _additionalFilesPfad = string.Empty;
    private string _cachePfad = string.Empty;
    private string _caption = string.Empty;
    private Timer? _checker;
    private int _checkerTickCount = -5;
    private string _createDate = string.Empty;
    private string _creator = string.Empty;
    private string _eventScriptErrorMessage = string.Empty;
    private string _eventScriptTmp = string.Empty;
    private string _eventScriptVersion = string.Empty;
    private double _globalScale = 1f;
    private string _globalShowPass = string.Empty;
    private bool _readOnly;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    private string _temporaryDatabaseMasterTimeUtc = string.Empty;
    private string _temporaryDatabaseMasterUser = string.Empty;
    private string _variableTmp = string.Empty;
    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    public DatabaseAbstract(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgel�st wird.
        Develop.StartService();

        QuickImage.NeedImage += QuickImage_NeedImage;

        TableName = MakeValidTableName(tablename);

        if (!IsValidTableName(TableName, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ung�ltig: " + tablename);
        }

        Cell = new CellCollection(this);
        Row = new RowCollection(this);
        Column = new ColumnCollection(this);
        Undo = new List<UndoItem>();

        //_columnArrangements.Clear();
        //_permissionGroupsNewRow.Clear();
        //_tags.Clear();
        //_datenbankAdmin.Clear();
        //_globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.UtcNow.ToString(Format_Date9, CultureInfo.InvariantCulture);
        _fileStateUTCDate = new DateTime(0); // Wichtig, dass das Datum bei Datenbanken ohne den Wert immer alles laden
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

        // Muss vor dem Laden der Datan zu Allfiles hinzugf�gt werde, weil das bei OnAdded
        // Die Events registriert werden, um z.B: das Passwort abzufragen
        // Zus�tzlich werden z.B: Filter f�r den Export erstellt - auch der muss die Datenbank finden k�nnen.
        // Zus�tzlich muss der Tablename stimme, dass in Added diesen verwerten kann.
        AllFiles.Add(this);
    }

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

    //public event EventHandler<VisibleEventArgs>? IsTableVisibleForUser;
    public event EventHandler? Loading;

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

    public event EventHandler<RowScriptCancelEventArgs>? ScriptError;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    public static List<ConnectionInfo> Allavailabletables { get; } = new();

    /// <summary>
    ///  Wann die Datei zuletzt geladen wurde. Einzige funktion, zu viele Ladezyklen hintereinander verhinden.
    /// </summary>
    public static DateTime LastLoadUtc { get; set; } = DateTime.UtcNow;

    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zus�tzlichen Dateien.")]
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

    public abstract ConnectionInfo ConnectionData { get; }

    public string CreateDate {
        get => _createDate;
        set {
            if (_createDate == value) { return; }
            _ = ChangeData(DatabaseDataType.CreateDateUTC, null, null, _createDate, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string Creator {
        get => _creator.Trim();
        set {
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

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public ReadOnlyCollection<DatabaseScriptDescription> EventScript {
        get => new(_eventScript);
        set {
            var l = new List<DatabaseScriptDescription>();
            l.AddRange(value);
            l.Sort();

            if (_eventScriptTmp == l.ToString(false)) { return; }
            _ = ChangeData(DatabaseDataType.EventScript, null, null, _eventScriptTmp, l.ToString(true), UserName, DateTime.UtcNow, string.Empty);

            EventScriptErrorMessage = string.Empty;
        }
    }

    public string EventScriptErrorMessage {
        get => _eventScriptErrorMessage;
        set {
            if (_eventScriptErrorMessage == value) { return; }
            _ = ChangeData(DatabaseDataType.EventScriptErrorMessage, null, null, _eventScriptErrorMessage, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string EventScriptVersion {
        get => _eventScriptVersion;
        set {
            if (_eventScriptVersion == value) { return; }
            _ = ChangeData(DatabaseDataType.EventScriptVersion, null, null, _eventScriptVersion, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Der Wert wird im System verankert und gespeichert.
    /// Bei Datenbanken, die Daten nachladen k�nnen, ist das der Stand, zu dem alle Daten fest abgespeichert sind.
    /// Kann hier nur gelesen werden! Da eine �nderung �ber die Property  die Datei wieder auf ungespeichert setzen w�rde, w�rde hier eine
    /// Kettenreaktion ausgel�st werden.
    /// </summary>
    public DateTime FileStateUTCDate => _fileStateUTCDate;

    /// <summary>
    /// Der FreezedReason kann niemals wieder r�ckg�nig gemacht werden.
    /// Weil keine Undos mehr geladen werden, w�rde da nur Chaos entstehten.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public double GlobalScale {
        get => _globalScale;
        set {
            if (Math.Abs(_globalScale - value) < 0.0001) { return; }
            _ = ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToString(CultureInfo.InvariantCulture), value.ToString(CultureInfo.InvariantCulture), UserName, DateTime.UtcNow, string.Empty);
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

    public bool HasPendingChanges { get; protected set; } = false;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Letzter Lade-Stand der Daten. Wird in RepairAfterParse gesetzt
    /// </summary>
    public DateTime IsInCache { get; protected set; } = new DateTime(0);

    public string KeyName {
        get {
            if (IsDisposed) { return string.Empty; }
            return ConnectionData.UniqueId;
        }
    }

    public DateTime LastChange { get; private set; } = new(1900, 1, 1);

    public string LoadedVersion { get; private set; } = "0.00";

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
    /// Pr�ft auch den FreezedReason
    /// </summary>
    public bool ReadOnly {
        get => _readOnly || !string.IsNullOrEmpty(FreezedReason);
        private set => _readOnly = value;
    }

    public RowCollection Row { get; }

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
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Description("Das standardm��ige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            _ = ChangeData(DatabaseDataType.StandardFormulaFile, null, null, _standardFormulaFile, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string TableName { get; protected set; }

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

    //public bool UndoLoaded { get; protected set; }

    public VariableCollection Variables {
        get => new(_variables);
        set {
            var l = new List<VariableString>();
            l.AddRange(value.ToListVariableString());
            foreach (var thisv in l) {
                thisv.ReadOnly = true; // Weil kein onChangedEreigniss vorhanden ist
            }
            l.Sort();
            if (_variableTmp == l.ToString(true)) { return; }
            _ = ChangeData(DatabaseDataType.DatabaseVariables, null, null, _variableTmp, l.ToString(true), UserName, DateTime.UtcNow, string.Empty);
            //OnViewChanged();
        }
    }

    public int VorhalteZeit { get; set; } = 30;

    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            _ = ChangeData(DatabaseDataType.RowQuickInfo, null, null, _zeilenQuickInfo, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    protected string? AdditionalFilesPfadtmp { get; set; }

    #endregion

    #region Methods

    public static List<ConnectionInfo> AllAvailableTables(string mustBeFreezed) {
        if (DateTime.UtcNow.Subtract(_lastTableCheck).TotalMinutes < 1) {
            return Allavailabletables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Datenbank z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu pr�fen
        var allreadychecked = new List<DatabaseAbstract>();

        var alf = new List<DatabaseAbstract>();// k�nnte sich �ndern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDb in alf) {
            var possibletables = thisDb.AllAvailableTables(allreadychecked, mustBeFreezed);

            allreadychecked.Add(thisDb);

            if (possibletables != null) {
                foreach (var thistable in possibletables) {
                    var canadd = true;

                    #region pr�fen, ob schon voranden, z.b. DatabaseAbstract.AllFiles

                    foreach (var checkme in Allavailabletables) {
                        if (string.Equals(checkme.TableName, thistable.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                            canadd = false;
                            break;
                        }
                    }

                    #endregion

                    if (canadd) { Allavailabletables.Add(thistable); }
                }
            }
        }

        _lastTableCheck = DateTime.UtcNow;
        return Allavailabletables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
    }

    public static void CheckSysUndoNow(ICollection<DatabaseAbstract> offDatabases, bool mustDoIt) {
        if (_isInTimer) { return; }
        _isInTimer = true;

        var fd = DateTime.UtcNow;

        try {
            var done = new List<DatabaseAbstract>();

            foreach (var thisDb in offDatabases) {
                if (!done.Contains(thisDb)) {
                    if (offDatabases.Count == 1) {
                        thisDb.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "�berpr�fe auf Ver�nderungen von '" + offDatabases.First().TableName + "'");
                    } else {
                        thisDb.OnDropMessage(BlueBasics.Enums.FehlerArt.Info, "�berpr�fe auf Ver�nderungen von " + offDatabases.Count + " Datenbanken des Typs '" + thisDb.GetType().Name + "'");
                    }

                    #region Datenbanken des gemeinsamen Servers ermittelen

                    var dbwss = thisDb.LoadedDatabasesWithSameServer();
                    done.AddRange(dbwss);
                    done.Add(thisDb); // Falls LoadedDatabasesWithSameServer einen Fehler versursacht

                    #endregion

                    #region Auf Eingangs Datenbanken beschr�nken

                    var db = new List<DatabaseAbstract>();
                    foreach (var thisDb2 in dbwss) {
                        if (offDatabases.Contains(thisDb2)) { db.Add(thisDb2); }
                    }

                    #endregion

                    var (changes, files) = thisDb.GetLastChanges(db, _timerTimeStamp.AddSeconds(-0.01), fd);
                    if (changes == null) {
                        if (mustDoIt) { Develop.DebugPrint(FehlerArt.Fehler, "Aktualiserung fehlgeschlagen!"); }
                        // Sp�ter ein neuer Versuch
                        _isInTimer = false;
                        return;
                    }

                    var start = DateTime.UtcNow;
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

    public static bool CriticalState() {
        foreach (var thisDb in AllFiles) {
            if (!thisDb.IsDisposed) {
                if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (thisDb.IsInCache == null) { return true; } // Irgend eine Datenbank wird aktuell geladen
            }
        }

        return false;
    }

    public static string EditableErrorReason(DatabaseAbstract? database, EditableErrorReasonType mode) {
        if (database is null || database.IsDisposed) { return "Keine Datenbank zum bearbeiten."; }
        return database.EditableErrorReason(mode);
    }

    public static void ForceSaveAll() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            _ = thisFile?.Save();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde ver�ndert! Selten, aber kann passieren!
                ForceSaveAll();
                return;
            }
        }
    }

    public static DatabaseAbstract? GetById(ConnectionInfo? ci, bool readOnly, NeedPassword? needPassword, bool checktablename) {
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

            if (d.AdditionalData.ToLower().EndsWith(".mdb") ||
                d.AdditionalData.ToLower().EndsWith(".bdb") ||
                d.AdditionalData.ToLower().EndsWith(".mbdb")) {
                if (d.AdditionalData.Equals(ci.AdditionalData, StringComparison.OrdinalIgnoreCase)) {
                    return thisFile; // Multiuser - nicht multiuser konflikt
                }
            }
        }

        #endregion

        #region Schauen, ob der Provider sie herstellen kann

        if (ci.Provider != null) {
            var db = ci.Provider.GetOtherTable(ci.TableName, readOnly, ci.MustBeFreezed);
            if (db != null) { return db; }
        }

        #endregion

        DatabaseTypes ??= GetEnumerableOfType<DatabaseAbstract>();

        #region Schauen, ob sie �ber den Typ definiert werden kann

        foreach (var thist in DatabaseTypes) {
            if (thist.Name.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) {
                var l = new object?[3];
                l[0] = ci;
                l[1] = readOnly;
                l[2] = needPassword;
                var v = thist.GetMethod("CanProvide")?.Invoke(null, l);

                if (v is DatabaseAbstract db) { return db; }
            }
        }

        #endregion

        #region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        if (FileExists(ci.AdditionalData)) {
            if (ci.AdditionalData.FileSuffix().ToLower() is "mdb" or "bdb") {
                var db = new Database(ci.TableName);
                db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
                return db;
            }
            if (ci.AdditionalData.FileSuffix().ToLower() is "mbdb") {
                var db = new DatabaseMU(ci.TableName);
                db.LoadFromFile(ci.AdditionalData, false, needPassword, ci.MustBeFreezed, readOnly);
                return db;
            }
        }

        #endregion

        //if (SqlBackAbstract.ConnectedSqlBack != null) {
        //    foreach (var thisSql in SqlBackAbstract.ConnectedSqlBack) {
        //        var h = thisSql.HandleMe(ci);
        //        if (h != null) {
        //            var db = new DatabaseSqlLite(ci.TableName);
        //           db.LoadFromSqlBack(needPassword, ci.MustBeFreezed, readOnly,h );
        //            return db;
        //        }
        //    }
        //}

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

        var t = tablename.ToUpper();

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
    public static DatabaseAbstract? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream) {
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
            db.LoadFromStream(d);
            return db;
        }
        if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Char_PfadSonderZeichen); // sonst st�rzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().ToLower().Replace(" ", "_").Replace("-", "_");
        tmp = tmp.StarkeVereinfachung("_").ToUpper();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Database is not DatabaseAbstract db || db.IsDisposed) { return string.Empty; }

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

    //    return null;
    //}
    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToString(Format_Integer5)).RemoveChars(Char_DateiSonderZeichen + " _.");
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

    //    foreach (var thisDb in alf) {
    //        if (thisDb.ConnectionDataOfOtherTable(tablename, true) is ConnectionInfo ci) {
    //            return ci;
    //        }
    //    }
    /// <summary>
    /// Der komplette Pfad mit abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public virtual string AdditionalFilesPfadWhole() {
        if (AdditionalFilesPfadtmp != null) { return AdditionalFilesPfadtmp; }

        if (!string.IsNullOrEmpty(_additionalFilesPfad)) {
            var t = _additionalFilesPfad.CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }

        AdditionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    //public static ConnectionInfo? ProviderOf(string tablename) {
    //    var alf = new List<DatabaseAbstract>();// k�nnte sich �ndern, deswegen Zwischenspeichern
    //    alf.AddRange(AllFiles);
    public abstract List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, string mustBeFreezed);

    public bool AmITemporaryMaster(bool checkUpcomingTo) {
        if (ReadOnly) { return false; }
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 5) { return false; }
        if (TemporaryDatabaseMasterUser != UserName + "-" + Environment.MachineName) { return false; }

        var d = DateTimeParse(TemporaryDatabaseMasterTimeUtc);

        if (checkUpcomingTo) {
            return DateTime.UtcNow.Subtract(d).TotalMinutes < 55;
        }

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogepr�ft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return DateTime.UtcNow.Subtract(d).TotalMinutes is > 5 and < 55;
    }

    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und k�mmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
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
            AddUndo(command, column, row, previousValue, changedTo, user, datetimeutc, comment);
        }

        return string.Empty;
    }

    public string CheckScriptError() {
        List<string> names = new();

        foreach (var thissc in _eventScript) {
            if (!thissc.IsOk()) {
                return thissc.KeyName + ": " + thissc.ErrorReason();
            }

            if (names.Contains(thissc.KeyName, false)) {
                return "Skriptname '" + thissc.KeyName + "' mehrfach vorhanden";
            }
        }

        var l = EventScript;
        if (l.Get(ScriptEventTypes.export).Count > 1) {
            return "Skript 'Export' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.loaded).Count > 1) {
            return "Skript 'Datenank geladen' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.prepare_formula).Count > 1) {
            return "Skript 'Formular Vorbereitung' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed_extra_thread).Count > 1) {
            return "Skript 'Wert ge�ndert Extra Thread' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.new_row).Count > 1) {
            return "Skript 'Neue Zeile' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed).Count > 1) {
            return "Skript 'Wert ge�ndert' mehrfach vorhanden";
        }

        return string.Empty;
    }

    public void CloneFrom(DatabaseAbstract sourceDatabase, bool cellDataToo, bool tagsToo) {
        _ = sourceDatabase.Save();

        Column.CloneFrom(sourceDatabase, cellDataToo);

        if (cellDataToo) { Row.CloneFrom(sourceDatabase); }

        //FirstColumn = sourceDatabase.FirstColumn;
        AdditionalFilesPfad = sourceDatabase.AdditionalFilesPfad;
        CachePfad = sourceDatabase.CachePfad; // Nicht so wichtig ;-)
        VorhalteZeit = sourceDatabase.VorhalteZeit; // Nicht so wichtig ;-)
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
        EventScriptErrorMessage = sourceDatabase.EventScriptErrorMessage;
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

    public abstract ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists, string mustBeeFreezed);

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Backup und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string DefaultBackupPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Backup\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Forms und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Layouts und abschlie�enden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string EditableErrorReason(EditableErrorReasonType mode) {
        if (IsDisposed) { return "Datenbank verworfen."; }

        if (mode is EditableErrorReasonType.OnlyRead or EditableErrorReasonType.Load) { return string.Empty; }

        if (IsInCache == null) { return "Datenbank wird noch geladen"; }

        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }

        if (ReadOnly && mode.HasFlag(EditableErrorReasonType.Save)) { return "Datenbank schreibgesch�tzt!"; }

        if (mode.HasFlag(EditableErrorReasonType.EditCurrently) || mode.HasFlag(EditableErrorReasonType.Save)) {
            if (Row.HasPendingWorker()) { return "Es m�ssen noch Daten �berpr�ft werden."; }
        }

        return IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))
            ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
            : string.Empty;
    }

    public void EnableScript() => Column.GenerateAndAddSystem("SYS_ROWSTATE");

    public void EventScript_Add(DatabaseScriptDescription ev, bool isLoading) {
        _eventScript.Add(ev);
        ev.Changed += EventScript_Changed;

        if (!isLoading) { EventScript_Changed(this, System.EventArgs.Empty); }
    }

    public void EventScript_RemoveAll(bool isLoading) {
        while (_eventScript.Count > 0) {
            var ev = _eventScript[_eventScript.Count - 1];
            ev.Changed -= EventScript_Changed;

            _eventScript.RemoveAt(_eventScript.Count - 1);
        }

        if (!isLoading) { EventScript_Changed(this, System.EventArgs.Empty); }
    }

    public ScriptEndedFeedback ExecuteScript(DatabaseScriptDescription s, bool changevalues, RowItem? row, List<string>? attributes) {
        if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, false, s.KeyName); }
        if (!string.IsNullOrEmpty(FreezedReason)) { return new ScriptEndedFeedback("Datenbank eingefroren: " + FreezedReason, false, false, s.KeyName); }

        var sce = CheckScriptError();
        if (!string.IsNullOrEmpty(sce)) { return new ScriptEndedFeedback("Die Skripte enthalten Fehler: " + sce, false, true, "Allgemein"); }

        try {
            var rowstamp = string.Empty;

            #region Variablen f�r Skript erstellen

            VariableCollection vars = new();

            if (row != null && !row.IsDisposed) {
                rowstamp = row.RowStamp();
                foreach (var thisCol in Column) {
                    var v = RowItem.CellToVariable(thisCol, row);
                    if (v != null) { vars.AddRange(v); }
                }
                vars.Add(new VariableRowItem("RowKey", row, true, true, "Die aktuelle Zeile, die ausgef�hrt wird."));
            }

            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("DB_" + thisvar.KeyName, thisvar.ValueString, false, false, "Datenbank-Kopf-Variable\r\n" + thisvar.Comment);
                vars.Add(v);
            }

            vars.Add(new VariableString("User", UserName, true, false, "ACHTUNG: Keinesfalls d�rfen benutzerabh�ngig Werte ver�ndert werden."));
            vars.Add(new VariableString("Usergroup", UserGroup, true, false, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden."));
            vars.Add(new VariableBool("Administrator", IsAdministrator(), true, false, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden.\r\nDiese Variable gibt zur�ck, ob der Benutzer Admin f�r diese Datenbank ist."));
            vars.Add(new VariableDatabase("Database", this, true, true, "Die Datenbank, die zu dem Skript geh�rt"));
            vars.Add(new VariableString("Tablename", TableName, true, false, "Der aktuelle Tabellenname."));
            vars.Add(new VariableBool("ReadOnly", ReadOnly, true, false, "Ob die aktuelle Datenbank schreibgesch�tzt ist."));
            vars.Add(new VariableFloat("Rows", Row.Count, true, false, "Die Anzahl der Zeilen in der Datenbank")); // RowCount als Befehl belegt
            vars.Add(new VariableString("NameOfFirstColumn", Column.First()?.KeyName ?? string.Empty, true, false, "Der Name der ersten Spalte"));
            vars.Add(new VariableBool("SetErrorEnabled", s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula), true, true, "Marker, ob der Befehl 'SetError' benutzt werden kann."));

            vars.Add(new VariableListString("Attributes", attributes, true, true, "Enth�lt - falls �bergeben worden - die Attribute aus dem Skript, das dieses hier aufruft."));

            #endregion

            #region  Erlaubte Methoden ermitteln

            var allowedMethods = MethodType.Standard | MethodType.Database;

            if (row != null && !row.IsDisposed) { allowedMethods |= MethodType.MyDatabaseRow; }
            if (!s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
                allowedMethods |= MethodType.IO;
                allowedMethods |= MethodType.NeedLongTime;
            }

            if (!s.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.loaded) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.value_changed)) {
                allowedMethods |= MethodType.ManipulatesUser;
            }

            if (changevalues) { allowedMethods |= MethodType.ChangeAnyDatabaseOrRow; }

            #endregion

            #region Script ausf�hren

            var scp = new ScriptProperties(allowedMethods, changevalues, s.Attributes());

            Script sc = new(vars, AdditionalFilesPfadWhole(), scp) {
                ScriptText = s.ScriptText
            };
            var scf = sc.Parse(0, s.KeyName);

            #endregion

            #region Variablen zur�ckschreiben und Special Rules ausf�hren

            if (sc.ChangeValues && changevalues && scf.AllOk) {
                if (row != null && !row.IsDisposed) {
                    if (Column.SysRowChangeDate is not ColumnItem) {
                        return new ScriptEndedFeedback("Zeilen k�nnen nur gepr�ft werden, wenn �nderungen der Zeile geloggt werden.", false, false, s.KeyName);
                    }

                    if (row.RowStamp() != rowstamp) {
                        return new ScriptEndedFeedback("Zeile wurde w�hrend des Skriptes ver�ndert.", false, false, s.KeyName);
                    }

                    foreach (var thisCol in Column) {
                        row.VariableToCell(thisCol, vars);
                    }
                }

                Variables = VariableCollection.Combine(Variables, vars, "DB_");
            }

            if (!scf.AllOk) {
                OnDropMessage(FehlerArt.Info, "Das Skript '" + s.KeyName + "' hat einen Fehler verursacht\r\n" + scf.Protocol[0]);
            }

            #endregion

            return scf;
        } catch {
            Develop.CheckStackForOverflow();
            return ExecuteScript(s, changevalues, row, attributes);
        }
    }

    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string? scriptname, bool changevalues, RowItem? row, List<string>? attributes) {
        try {
            if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, false, "Allgemein"); }

            var e = new CancelReasonEventArgs();
            OnCanDoScript(e);
            if (e.Cancel) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht m�glich: " + e.CancelReason, false, false, "Allgemein"); }

            var m = EditableErrorReason(EditableErrorReasonType.EditCurrently);

            if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht m�glich: " + m, false, false, "Allgemein"); }

            #region Script ermitteln

            if (eventname != null && !string.IsNullOrEmpty(scriptname)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback("Kein Eventname oder Skript angekommen", false, false, "Allgemein"); }

            if (string.IsNullOrEmpty(scriptname) && eventname != null) {
                var l = EventScript.Get((ScriptEventTypes)eventname);
                if (l.Count == 1) { scriptname = l[0].KeyName; }
                if (string.IsNullOrEmpty(scriptname)) {
                    // Script nicht definiert. Macht nix. ist eben keines gew�nscht
                    return new ScriptEndedFeedback();
                }
            }

            if (scriptname == null || string.IsNullOrWhiteSpace(scriptname)) { return new ScriptEndedFeedback("Kein Skriptname angekommen", false, false, "Allgemein"); }

            var script = EventScript.Get(scriptname);

            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }

            if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, scriptname); }

            if (!script.NeedRow) { row = null; }

            #endregion

            if (!script.ChangeValues) { changevalues = false; }

            return ExecuteScript(script, changevalues, row, attributes);
        } catch {
            Develop.CheckStackForOverflow();
            return ExecuteScript(eventname, scriptname, changevalues, row, attributes);
        }
    }

    public string Export_CSV(FirstRow firstRow, ColumnItem column, IEnumerable<RowItem> sortedRows) =>
                        //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
                        Export_CSV(firstRow, new List<ColumnItem> { column }, sortedRows);

    public string Export_CSV(FirstRow firstRow, List<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows) {
        columnList ??= Column.Where(thisColumnItem => thisColumnItem != null).ToList();
        //sortedRows ??= Row.AllRows();

        StringBuilder sb = new();
        switch (firstRow) {
            case FirstRow.Without:
                break;

            case FirstRow.ColumnCaption:
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        var tmp = columnList[colNr].ReadableText();
                        tmp = tmp.Replace(";", "|");
                        tmp = tmp.Replace(" |", "|");
                        tmp = tmp.Replace("| ", "|");
                        _ = sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(";"); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            case FirstRow.ColumnInternalName:
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        _ = sb.Append(columnList[colNr].KeyName);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }

        var (_, errormessage) = RefreshRowData(sortedRows, false);
        if (!string.IsNullOrEmpty(errormessage)) {
            OnDropMessage(FehlerArt.Fehler, errormessage);
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow != null && !thisRow.IsDisposed) {
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        var tmp = Cell.GetString(columnList[colNr], thisRow);
                        tmp = tmp.Replace("\r\n", "|");
                        tmp = tmp.Replace("\r", "|");
                        tmp = tmp.Replace("\n", "|");
                        tmp = tmp.Replace(";", "<sk>");
                        _ = sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
            }
        }
        return sb.ToString().TrimEnd("\r\n");
    }

    public string Export_CSV(FirstRow firstRow, ColumnViewCollection? arrangement, IEnumerable<RowItem> sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

    public bool Export_HTML(string filename, List<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows, bool execute) {
        try {
            if (columnList == null || columnList.Count == 0) {
                columnList = Column.Where(thisColumnItem => thisColumnItem != null).ToList();
            }

            //sortedRows ??= Row.AllRows();

            if (string.IsNullOrEmpty(filename)) {
                filename = TempFile(string.Empty, "Export", "html");
            }

            Html da = new(TableName.FileNameWithoutSuffix());
            da.AddCaption(_caption);
            da.TableBeginn();

            #region Spaltenk�pfe

            da.RowBeginn();
            foreach (var thisColumn in columnList) {
                if (thisColumn != null) {
                    da.CellAdd(thisColumn.ReadableText().Replace(";", "<br>"), thisColumn.BackColor);
                }
            }

            da.RowEnd();

            #endregion

            var (_, errormessage) = RefreshRowData(sortedRows, false);
            if (!string.IsNullOrEmpty(errormessage)) {
                OnDropMessage(FehlerArt.Fehler, errormessage);
                return false;
            }

            #region Zeilen

            foreach (var thisRow in sortedRows) {
                if (thisRow != null && !thisRow.IsDisposed) {
                    da.RowBeginn();
                    foreach (var thisColumn in columnList) {
                        if (thisColumn != null) {
                            var lcColumn = thisColumn;
                            var lCrow = thisRow;
                            if (thisColumn.Format is DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                                (lcColumn, lCrow, _, _) = CellCollection.LinkedCellData(thisColumn, thisRow, false, false);
                            }

                            if (lCrow != null && lcColumn != null) {
                                da.CellAdd(lCrow.CellGetValuesReadable(lcColumn, ShortenStyle.HTML).JoinWith("<br>"), thisColumn.BackColor);
                            } else {
                                da.CellAdd(" ", thisColumn.BackColor);
                            }
                        }
                    }

                    da.RowEnd();
                }
            }

            #endregion

            #region Summe

            da.RowBeginn();
            foreach (var thisColumn in columnList) {
                if (thisColumn != null) {
                    var s = thisColumn.Summe(sortedRows);
                    if (s == null) {
                        da.CellAdd("-", thisColumn.BackColor);
                        //da.GenerateAndAdd("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                    } else {
                        da.CellAdd("~sum~ " + s, thisColumn.BackColor);
                        //da.GenerateAndAdd("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">&sum; " + s + "</th>");
                    }
                }
            }
            da.RowEnd();

            #endregion

            da.TableEnd();
            da.AddFoot();
            return da.Save(filename, execute);
        } catch {
            return false;
        }
    }

    public bool Export_HTML(string filename, ColumnViewCollection? arrangement, IEnumerable<RowItem> sortedRows, bool execute) => Export_HTML(filename, arrangement?.ListOfUsedColumn(), sortedRows, execute);

    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionalFilesPfadWhole() + _standardFormulaFile)) { return AdditionalFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    /// <summary>
    /// Friert die Datenbank komplett ein, nur noch Ansicht m�glich.
    /// Setzt auch ReadOnly
    /// </summary>
    /// <param name="reason"></param>
    public void Freeze(string reason) {
        SetReadOnly();
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    public List<string> GetAllLayouts() {
        List<string> path = new();
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

    public abstract (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromUTC, DateTime toUTC);

    public string GetLayout(string ca) {
        if (FileExists(ca)) { return ca; }

        var f = GetAllLayouts();

        foreach (var thisF in f) {
            if (thisF.FileNameWithoutSuffix().Equals(ca, StringComparison.OrdinalIgnoreCase)) { return thisF; }
        }
        return string.Empty;
    }

    public DatabaseAbstract? GetOtherTable(string tablename, bool readOnly, string freezedReason) {
        if (!IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Ung�ltiger Tabellenname: " + tablename);
            return null;
        }

        var x = ConnectionDataOfOtherTable(tablename, true, freezedReason);
        if (x == null) { return null; }

        x.Provider = null;  // KEINE Vorage mitgeben, weil sonst eine Endlosschleife aufgerufen wird!

        return GetById(x, readOnly, null, true);// new DatabaseSQL(_sql, readOnly, tablename);
    }

    //public void GetUndoCache() {
    //    if (UndoLoaded) { return; }

    //    var undos = GetLastChanges(new List<DatabaseAbstract> { this }, new DateTime(2000, 1, 1), new DateTime(2100, 1, 1));

    //    Undo.Clear();

    //    if(undos.co)

    //    Undo.AddRange(undos.Changes);

    //    UndoLoaded = true;
    //}

    public bool HasErrorCheckScript() {
        if (!IsRowScriptPossible(true)) { return false; }

        var e = EventScript.Get(ScriptEventTypes.prepare_formula);
        return e.Count == 1;
    }

    public string Import(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        if (!Row.IsNewRowPossible()) {
            OnDropMessage(FehlerArt.Warnung, "Abbruch, Datenbank unterst�tzt keine neuen Zeilen.");
            return "Abbruch, Datenbank unterst�tzt keine neuen Zeilen.";
        }

        #region Text vorbereiten

        importText = importText.Replace("\r\n", "\r").Trim("\r");

        #endregion

        #region Die Zeilen (zeil) vorbereiten

        var ein = importText.SplitAndCutByCr();
        List<string[]> zeil = new();
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

        List<ColumnItem> columns = new();
        var startZ = 0;

        if (spalteZuordnen) {
            startZ = 1;
            for (var spaltNo = 0; spaltNo < zeil[0].GetUpperBound(0) + 1; spaltNo++) {
                if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, leerer Spaltenname.");
                    return "Abbruch,<br>leerer Spaltenname.";
                }
                zeil[0][spaltNo] = ColumnItem.MakeValidColumnName(zeil[0][spaltNo]);

                var col = Column.Exists(zeil[0][spaltNo]);
                if (col == null) {
                    if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                        OnDropMessage(FehlerArt.Warnung, "Abbruch, ung�ltiger Spaltenname.");
                        return "Abbruch,<br>ung�ltiger Spaltenname.";
                    }

                    col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                    if (col != null) {
                        col.Caption = zeil[0][spaltNo];
                        col.Format = DataFormat.Text;
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
                    newc.Format = DataFormat.Text;
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
                if (zeil[rowNo].GetUpperBound(0) >= 0 && !string.IsNullOrEmpty(zeil[rowNo][0]) && !dictNeu.ContainsKey(zeil[rowNo][0].ToUpper())) {
                    dictNeu.Add(zeil[rowNo][0].ToUpper(), zeil[rowNo]);
                }
                //else {
                //    OnDropMessage(FehlerArt.Warnung, "Abbruch, eingehende Werte k�nnen nicht eindeutig zugeordnet werden.");
                //    return "Abbruch, eingehende Werte k�nnen nicht eindeutig zugeordnet werden.";
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
                var f = thisR.CellFirstString().ToUpper();
                if (!string.IsNullOrEmpty(f) && !dictVorhanden.ContainsKey(f)) {
                    dictVorhanden.Add(f, thisR);
                } else {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch, vorhandene Zeilen sind nicht eindeutig.");
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
                row = Row.GenerateAndAdd(thisD.Value[0], "Import, fehlende Zeile");
            }

            if (row == null) {
                OnDropMessage(FehlerArt.Warnung, "Abbruch, Import-Fehler.");
                return "Abbruch, Import-Fehler.";
            }

            #endregion

            #region Werte in die Spalten schreiben

            for (var colNo = 0; colNo < maxColCount; colNo++) {
                row.CellSet(columns[colNo], thisD.Value[colNo].SplitAndCutBy("|").JoinWithCr());
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

            #endregion
        }

        #endregion

        Save();
        OnDropMessage(FehlerArt.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public string ImportCsv(string filename) {
        if (!FileExists(filename)) { return "Datei nicht gefunden"; }
        var importText = File.ReadAllText(filename, Win1252);

        if (string.IsNullOrEmpty(importText)) { return "Dateiinhalt leer"; }

        var x = importText.SplitAndCutByCrToList();

        if (x.Count < 2) { return "Keine Zeilen zum importieren."; }

        var sep = ",";

        if (x[0].StartsWith("sep=", StringComparison.OrdinalIgnoreCase)) {
            if (x.Count < 3) { return "Keine Zeilen zum importieren."; }
            sep = x[0].Substring(4);
            x.RemoveAt(0);
            importText = x.JoinWithCr();
        }

        return Import(importText, true, true, sep, false, false);
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains(Everybody, false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    //    Export = new(ex);
    //}
    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is Database db) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    _ = thisFile.Save();
                    Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsRowScriptPossible(bool checkMessageTo) {
        if (Column.SysRowChangeDate == null) { return false; }
        if (Column.SysRowState == null) { return false; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; }
        if (checkMessageTo && !string.IsNullOrEmpty(_eventScriptErrorMessage)) { return false; }
        return true;
    }

    public abstract string? NextRowKey();

    public void OnCanDoScript(CancelReasonEventArgs e) {
        if (IsDisposed) { return; }
        CanDoScript?.Invoke(this, e);
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    public void OnInvalidateView() {
        if (IsDisposed) { return; }
        InvalidateView?.Invoke(this, System.EventArgs.Empty);
    }

    //    foreach (var thisExport in ex) {
    //        if (thisExport != null) {
    //            if (thisExport.Typ == ExportTyp.EinzelnMitFormular) {
    //                if (thisExport.ExportFormularId == layoutId) {
    //                    thisExport.LastExportTimeUtc = new DateTime(1900, 1, 1);
    //                }
    //            }
    //        }
    //    }
    public void OnScriptError(RowScriptCancelEventArgs e) {
        if (IsDisposed) { return; }
        ScriptError?.Invoke(this, e);
    }

    public void OnViewChanged() {
        if (IsDisposed) { return; }
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void Optimize() {
        if (Row.Count < 5) { return; }

        foreach (var thisColumn in Column) {
            thisColumn.Optimize();

            if (thisColumn.Format is not DataFormat.Verkn�pfung_zu_anderer_Datenbank and
                                     not DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems and
                                     not DataFormat.Button) {
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

    //public void InvalidateExports(string layoutId) {
    //    if (ReadOnly) { return; }
    public List<string> Permission_AllUsed(bool cellLevel) {
        List<string> e = new();
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

    public void RefreshColumnsData(ColumnItem column) {
        if (column.IsInCache != null) { return; }
        RefreshColumnsData(new List<ColumnItem> { column });
    }

    public abstract void RefreshColumnsData(List<ColumnItem> columns);

    public void RefreshColumnsData(IEnumerable<FilterItem>? fi) {
        if (fi != null) {
            var c = new List<ColumnItem>();

            foreach (var thisF in fi) {
                if (thisF.Column != null && thisF.Column.IsInCache == null) {
                    _ = c.AddIfNotExists(thisF.Column);
                }
            }
            RefreshColumnsData(c);
        }
    }

    public abstract (bool didreload, string errormessage) RefreshRowData(IEnumerable<RowItem> row, bool refreshAlways);

    public (bool didreload, string errormessage) RefreshRowData(List<string> keys, bool refreshAlways) {
        if (keys.Count == 0) { return (false, string.Empty); }

        var r = new List<RowItem>();
        foreach (var thisK in keys) {
            var ro = Row.SearchByKey(thisK);
            if (ro != null) { r.Add(ro); }
        }
        return RefreshRowData(r, refreshAlways);
    }

    public (bool didreload, string errormessage) RefreshRowData(RowItem row, bool refreshAlways) {
        if (!refreshAlways && row.IsInCache != null) { return (false, string.Empty); }

        return RefreshRowData(new List<RowItem> { row }, refreshAlways);
    }

    public virtual void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten DatabaseMU nicht mehr funktioniert

        if (!string.IsNullOrEmpty(EditableErrorReason(this, EditableErrorReasonType.EditAcut))) { return; }

        Column.Repair();
        RepairColumnArrangements(Reason.SetCommand);
        //RepairViews();
        //_layouts.Check();
    }

    public abstract bool Save();

    public void SetReadOnly() => ReadOnly = true;

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und f�hrt Commands aus.
    /// Es wird WriteValueToDiscOrServer aufgerufen - echtzeitbasierte Systeme k�nnen dort den Wert speichern
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="reason"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    public (string Error, ColumnItem? Columnchanged, RowItem? Rowchanged) SetValueInternal(DatabaseDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, Reason reason) {
        if (IsDisposed) { return ("Datenbank verworfen!", null, null); }
        if ((reason is not Reason.InitialLoad and not Reason.UpdateChanges) && !string.IsNullOrEmpty(FreezedReason)) { return ("Datenbank eingefroren: " + FreezedReason, null, null); }
        if (type.IsObsolete()) { return (string.Empty, null, null); }

        LastChange = DateTime.UtcNow;

        if (type.IsCellValue()) {
            if (column?.Database is not DatabaseAbstract db) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type);
                return ("Wert nicht gesetzt!", null, null);
            }

            if (row == null) {
                Develop.DebugPrint(FehlerArt.Warnung, "Zeile ist null! " + type);
                return ("Wert nicht gesetzt!", null, null);
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
                return ("Wert nicht gesetzt!", null, null);
            }
            column.Invalidate_ContentWidth();
            return (column.SetValueInternal(type, value), column, null);
        }

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Command_RemoveColumn:
                    var c = Column.Exists(value);
                    if (c == null) { return (string.Empty, null, null); }
                    return (Column.ExecuteCommand(type, c.KeyName, reason), c, null);

                case DatabaseDataType.Command_AddColumnByName:
                    var f2 = Column.ExecuteCommand(type, value, reason);
                    if (!string.IsNullOrEmpty(f2)) { return (f2, null, null); }

                    var thisColumn = Column.Exists(value);
                    if (thisColumn == null) { return ("Hinzuf�gen fehlgeschlagen", null, null); }

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
                _fileStateUTCDate = DateTimeParse(value);
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
                List<string> ai = new(value.SplitAndCutByCr());
                foreach (var t in ai) {
                    EventScript_Add(new DatabaseScriptDescription(this, t), true);
                }

                //CheckScriptError();
                break;

            case DatabaseDataType.DatabaseVariables:
                _variables.Clear();
                List<string> va = new(value.SplitAndCutByCr());
                foreach (var t in va) {
                    var l = new VariableString("dummy");
                    l.Parse(t);
                    l.ReadOnly = true; // Weil kein onChangedEreigniss vorhanden ist
                    _variables.Add(l);
                }
                _variables.Sort();
                _variableTmp = _variables.ToString(true);
                break;

            case DatabaseDataType.ColumnArrangement:
                _columnArrangements.Clear();
                List<string> ca = new(value.SplitAndCutByCr());
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
                _eventScriptVersion = value;
                break;

            case DatabaseDataType.EventScriptErrorMessage:
                _eventScriptErrorMessage = value;
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
    internal virtual bool IsNewRowPossible() => string.IsNullOrWhiteSpace(EditableErrorReason(EditableErrorReasonType.EditNormaly));

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
    //    //    //ev.Changed -= EventScript_Changed;
    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
    }

    //public void Variables_Add(VariableString va, bool isLoading) {
    //    _variables.Add(va);
    //    if (!isLoading) { Variables = new VariableCollection(_variables); }
    //}
    internal void RefreshCellData(ColumnItem column, RowItem row, Reason reason) {
        if (reason is Reason.InitialLoad or Reason.UpdateChanges or Reason.AdditionalWorkAfterComand) { return; }

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
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Tempor�r erzeugt werden!

        var x = _columnArrangements.CloneWithClones();

        for (var z = 0; z < Math.Max(2, x.Count); z++) {
            if (x.Count < z + 1) { x.Add(new ColumnViewCollection(this, string.Empty)); }
            ColumnViewCollection.Repair(x[z], z);
        }

        if (reason is Reason.InitialLoad or Reason.UpdateChanges or Reason.AdditionalWorkAfterComand) {
            SetValueInternal(DatabaseDataType.ColumnArrangement, null, null, x.ToString(false), UserName, DateTime.UtcNow, reason);
        } else {
            ColumnArrangements = x.AsReadOnly();
        }
    }

    /// <summary>
    /// Bef�llt den Undo Speicher und schreibt den auch im Filesystem
    /// </summary>
    /// <param name="type"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="userName"></param>
    /// <param name="comment"></param>
    protected virtual void AddUndo(DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == DatabaseDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(TableName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment));
    }

    protected void CreateWatcher() {
        if (string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Save))) {
            _checker = new Timer(Checker_Tick);
            _ = _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        IsDisposed = true;

        OnDisposingEvent();
        _ = AllFiles.Remove(this);

        //base.Dispose(disposing); // speichert und l�scht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }

        _checker?.Dispose();

        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten �berschreiben.
        // TODO: gro�e Felder auf Null setzen.

        Column.Dispose();
        //Cell?.Dispose();
        Row.Dispose();
        OnDisposed();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="files"></param>
    /// <param name="data"></param>
    /// <param name="toUTC"></param>
    /// <param name="startTimeUTC">Nur um die Zeot stoppen zu k�nnen und lange Prozesse zu k�rzen</param>
    protected void DoLastChanges(List<string>? files, List<UndoItem>? data, DateTime toUTC, DateTime startTimeUTC) {
        if (data == null) { return; }
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        if (IsInCache == null) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank noch nicht korrekt geladen!");
            return;
        }

        data = data.OrderBy(obj => obj.DateTimeUtc).ToList();

        try {
            List<ColumnItem> columnsAdded = new();
            List<RowItem> rowsAdded = new();
            List<string> cellschanged = new();
            List<string> myfiles = new();

            if (files != null) {
                foreach (var thisf in files) {
                    if (thisf.Contains("\\" + TableName.ToUpper() + "-")) {
                        myfiles.AddIfNotExists(thisf);
                    }
                }
            }

            foreach (var thisWork in data) {
                if (TableName == thisWork.TableName && thisWork.DateTimeUtc > IsInCache) {
                    Undo.Add(thisWork);
                    _changesNotIncluded.Add(thisWork);

                    var c = Column.Exists(thisWork.ColName);
                    var r = Row.SearchByKey(thisWork.RowKey);
                    var (Error, Columnchanged, Rowchanged) = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.UpdateChanges);

                    if (!string.IsNullOrEmpty(Error)) {
                        Freeze("Datenbank-Fehler: " + Error + " " + thisWork.ToString());
                        //Develop.DebugPrint(FehlerArt.Fehler, "Fehler beim Nachladen: " + Error + " / " + TableName);
                        return;
                    }

                    if (c == null && Columnchanged != null) { columnsAdded.AddIfNotExists(Columnchanged); }
                    if (r == null && Rowchanged != null) { rowsAdded.AddIfNotExists(Rowchanged); }
                    if (Rowchanged != null && Columnchanged != null) { cellschanged.AddIfNotExists(CellCollection.KeyOfCell(c, r)); }
                }
            }

            IsInCache = toUTC;
            DoWorkAfterLastChanges(myfiles, columnsAdded, rowsAdded, cellschanged, startTimeUTC);
            OnInvalidateView();
        } catch {
            Develop.CheckStackForOverflow();
            DoLastChanges(files, data, toUTC, startTimeUTC);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="files"></param>
    /// <param name="columnsAdded"></param>
    /// <param name="rowsAdded"></param>
    /// <param name="cellschanged"></param>
    /// <param name="startTimeUTC">Nur um die Zeot stoppen zu k�nnen und lange Prozesse zu k�rzen</param>
    protected abstract void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, List<string> cellschanged, DateTime starttimeUTC);

    protected void GenerateTimer() {
        if (_pendingChangesTimer != null) { return; }
        _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
        _pendingChangesTimer = new Timer(CheckSysUndo);
        _ = _pendingChangesTimer.Change(10000, 10000);
    }

    protected abstract IEnumerable<DatabaseAbstract> LoadedDatabasesWithSameServer();

    protected void OnLoaded() {
        if (IsDisposed) { return; }
        //IsInCache = FileStateUTCDate;
        Loaded?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Diese Routine darf nur aufgerufen werden, wenn die Daten der Datenbank von der Festplatte eingelesen wurden.
    /// </summary>
    protected void TryToSetMeTemporaryMaster() {
        if (ReadOnly) { return; }
        if (!IsAdministrator()) { return; }
        if (!IsRowScriptPossible(true)) { return; }

        if (AmITemporaryMaster(true)) { return; }

        var d = DateTimeParse(TemporaryDatabaseMasterTimeUtc);

        if (DateTime.UtcNow.Subtract(d).TotalMinutes < 60 && !string.IsNullOrEmpty(TemporaryDatabaseMasterUser)) { return; }

        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 5) { return; }

        TemporaryDatabaseMasterUser = UserName + "-" + Environment.MachineName;
        TemporaryDatabaseMasterTimeUtc = DateTime.UtcNow.ToString(Format_Date5, CultureInfo.InvariantCulture);
    }

    protected virtual string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren!"; } // Sicherheitshalber!
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }

        return string.Empty;
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 180) { return; }
        if (DateTime.UtcNow.Subtract(LastLoadUtc).TotalSeconds < 5) { return; }

        if (CriticalState()) { return; }
        CheckSysUndoNow(DatabaseAbstract.AllFiles, false);
    }

    private void Checker_Tick(object state) {
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 6) { return; }
        if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return; }
        var e = new CancelReasonEventArgs();
        OnCanDoScript(e);
        if (e.Cancel) { return; }

        Row.AddRowsForValueChangedEvent();

        Row.ExecuteValueChangedEvent(false);

        if (!string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Save))) { return; }
        if (!LogUndo) { return; }

        Row.ExecuteExtraThread();

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

    private void EventScript_Changed(object sender, System.EventArgs e) => EventScript = _eventScript.AsReadOnly();

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    //private void OnIsTableVisibleForUser(VisibleEventArgs e) {
    //    if (IsDisposed) { return; }
    //    IsTableVisibleForUser?.Invoke(this, e);
    //}
    private bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
        var tmpName = UserName.ToUpper();
        var tmpGroup = UserGroup.ToUpper();
        if (string.Equals(allowed, Everybody, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (Column.SysRowCreator is ColumnItem src && string.Equals(allowed, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase)) {
            if (row != null && Cell.GetString(src, row).ToUpper() == tmpName) { return true; }
        } else if (string.Equals(allowed, "#USER: " + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, "#USER:" + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, tmpGroup, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        return false;
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            // Es werden alle Datenbanken abgefragt, also kann nach der ersten nicht schluss sein...

            if (string.IsNullOrWhiteSpace(AdditionalFilesPfadWhole())) { return; }

            var name = e.Name.RemoveChars(Char_DateiSonderZeichen);

            var fullname = CachePfad.TrimEnd("\\") + "\\" + name + ".PNG";

            if (FileExists(fullname)) {
                e.Done = true;
                e.Bmp = new BitmapExt(fullname);
            }
        } catch { }
    }

    #endregion
}