// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.AdditionalScriptMethods;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
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

    public const string DatabaseVersion = "4.10";
    public static readonly ObservableCollection<Database> AllFiles = [];
    public static readonly string Chunk_AdditionalUndo = "_undo";
    public static readonly string Chunk_AdditionalUseCases = "_uses";
    public static readonly string Chunk_MainData = "MainData";

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
    private static volatile bool _isInTimer;
    private static DateTime _lastTableCheck = new(1900, 1, 1);

    /// <summary>
    /// Der Globale Timer, der die Sys_Undo Datenbank abfrägt
    /// </summary>
    private static Timer? _pendingChangesTimer;

    /// <summary>
    /// Der Zeitstempel der letzten Abfrage des _pendingChangesTimer
    /// </summary>
    private static DateTime _timerTimeStamp = DateTime.UtcNow.AddSeconds(-0.5);

    /// <summary>
    /// Flüchtiger Speicher, wird nur zum Halten von Daten verwendet.
    /// </summary>
    private readonly ConcurrentDictionary<string, DatabaseChunk> _chunks = new();

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
    private string _columnArrangements = string.Empty;
    private string _createDate;
    private string _creator;
    private string _editNormalyError = string.Empty;
    private DateTime _editNormalyNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private string _eventScriptTmp = string.Empty;
    private DateTime _eventScriptVersion = DateTime.MinValue;

    //private float _globalScale = 1f;
    private string _globalShowPass = string.Empty;

    private bool _isInSave;
    private DateTime _powerEditTime = DateTime.MinValue;
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
        FileStateUtcDate = new DateTime(0); // Wichtig, dass das Datum bei Datenbanken ohne den Wert immer alles laden
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

    ~Database() {
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

    public static List<string> ExecutingScriptAnyDatabase { get; } = [];

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

    public string ColumnArrangements {
        get => _columnArrangements;
        set {
            if (_columnArrangements == value) { return; }
            _ = ChangeData(DatabaseDataType.ColumnArrangement, null, null, _columnArrangements, value, UserName, DateTime.UtcNow, string.Empty);
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
    public int DoingChanges { get; private set; }

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

    public DateTime EventScriptVersion {
        get => _eventScriptVersion;
        set {
            if (_eventScriptVersion == value) { return; }
            _ = ChangeData(DatabaseDataType.EventScriptVersion, null, null, _eventScriptVersion.ToString5(), value.ToString5(), UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public List<string> ExecutingScript { get; } = [];

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Der Wert wird im System verankert und gespeichert.
    /// Bei Datenbanken, die Daten nachladen können, ist das der Stand, zu dem alle Daten fest abgespeichert sind.
    /// Kann hier nur gelesen werden! Da eine Änderung über die Property  die Datei wieder auf ungespeichert setzen würde, würde hier eine
    /// Kettenreaktion ausgelöst werden.
    /// </summary>
    public DateTime FileStateUtcDate { get; protected set; }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgängig gemacht werden.
    /// Weil keine Undos mehr geladen werden, würde da nur Chaos entstehen.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    //public float GlobalScale {
    //    get => _globalScale;
    //    set {
    //        if (Math.Abs(_globalScale - value) < DefaultTolerance) { return; }
    //        _ = ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToStringFloat2(), value.ToStringFloat2(), UserName, DateTime.UtcNow, string.Empty);
    //        Cell.InvalidateAllSizes();
    //    }
    //}

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            _ = ChangeData(DatabaseDataType.GlobalShowPass, null, null, _globalShowPass, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    //public bool HasPendingChanges => _chunksChanged.Count > 0;

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

    public bool PowerEdit {
        get {
            return _powerEditTime.Subtract(DateTime.UtcNow).TotalSeconds > 0;
        }

        set {
            if (value) {
                _powerEditTime = DateTime.UtcNow.AddSeconds(300);
            } else {
                _powerEditTime = DateTime.UtcNow.AddSeconds(-1);
            }
            OnInvalidateView();
        }
    }

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
            if (_sortDefinition != null) { alt = _sortDefinition.ParseableItems().FinishParseable(); }
            if (value != null) { neu = value.ParseableItems().FinishParseable(); }
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
        protected set {
            if (_temporaryDatabaseMasterTimeUtc == value) { return; }
            _ = ChangeData(DatabaseDataType.TemporaryDatabaseMasterTimeUTC, null, null, _temporaryDatabaseMasterTimeUtc, value, UserName, DateTime.UtcNow, string.Empty);
        }
    }

    public string TemporaryDatabaseMasterUser {
        get => _temporaryDatabaseMasterUser;
        protected set {
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
            var possibletables = thisDb.AllAvailableTables(allreadychecked);

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
                            Pause(1, false);
                            CheckSysUndoNow(offDatabases, mustDoIt);
                        }

                        // Später ein neuer Versuch
                        return;
                    }

                    var start = DateTime.UtcNow;
                    db.Shuffle();
                    foreach (var thisdb in db) {
                        thisdb.DoLastChanges(files, changes, start, fd);
                        thisdb.DidLastChanges();
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
        if (database is null) { return "Keine Datenbank zum Bearbeiten."; }
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

    public static void FreezeAll(string reason) {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile.Freeze(reason);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                FreezeAll(reason);
                return;
            }
        }
    }

    public static List<DatabaseChunk>? GenerateNewChunks(Database db, int minLen, DateTime fileStateUtcDateToSave, bool chunksAllowed) {
        var chunks = new List<DatabaseChunk>();

        chunksAllowed = chunksAllowed && db.Column.SplitColumn != null;

        var mainChunk = new DatabaseChunk(db.Filename, Chunk_MainData);
        mainChunk.InitByteList();
        chunks.Add(mainChunk);

        var undoChunk = mainChunk;
        var usesChunk = mainChunk;

        if (chunksAllowed) {
            undoChunk = new DatabaseChunk(db.Filename, Chunk_AdditionalUndo);
            undoChunk.InitByteList();
            chunks.Add(undoChunk);

            usesChunk = new DatabaseChunk(db.Filename, Chunk_AdditionalUseCases);
            usesChunk.InitByteList();
            chunks.Add(usesChunk);
        }

        try {
            var x = db.LastChange;

            mainChunk.SaveToByteList(DatabaseDataType.GlobalShowPass, db.GlobalShowPass);
            mainChunk.SaveToByteList(DatabaseDataType.Creator, db.Creator);
            mainChunk.SaveToByteList(DatabaseDataType.CreateDateUTC, db.CreateDate);
            mainChunk.SaveToByteList(DatabaseDataType.FileStateUTCDate, fileStateUtcDateToSave.ToString7());
            mainChunk.SaveToByteList(DatabaseDataType.Caption, db.Caption);

            mainChunk.SaveToByteList(DatabaseDataType.TemporaryDatabaseMasterUser, db.TemporaryDatabaseMasterUser);
            mainChunk.SaveToByteList(DatabaseDataType.TemporaryDatabaseMasterTimeUTC, db.TemporaryDatabaseMasterTimeUtc);

            mainChunk.SaveToByteList(DatabaseDataType.Tags, db.Tags.JoinWithCr());
            mainChunk.SaveToByteList(DatabaseDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            mainChunk.SaveToByteList(DatabaseDataType.DatabaseAdminGroups, db.DatenbankAdmin.JoinWithCr());

            mainChunk.SaveToByteList(DatabaseDataType.AdditionalFilesPath, db.AdditionalFilesPfad);
            mainChunk.SaveToByteList(DatabaseDataType.RowQuickInfo, db.ZeilenQuickInfo);
            mainChunk.SaveToByteList(DatabaseDataType.StandardFormulaFile, db.StandardFormulaFile);

            //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
            foreach (var columnitem in db.Column) {
                if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName) && !columnitem.IsDisposed) {
                    mainChunk.SaveToByteList(columnitem);
                    usesChunk.SaveToByteList(DatabaseDataType.ColumnSystemInfo, columnitem.SystemInfo, columnitem.KeyName);
                }
            }

            mainChunk.SaveToByteList(DatabaseDataType.SortDefinition, db.SortDefinition == null ? string.Empty : db.SortDefinition.ParseableItems().FinishParseable());

            mainChunk.SaveToByteList(DatabaseDataType.ColumnArrangement, db.ColumnArrangements);

            mainChunk.SaveToByteList(DatabaseDataType.EventScript, db.EventScript.ToString(true));
            mainChunk.SaveToByteList(DatabaseDataType.EventScriptVersion, db.EventScriptVersion.ToString5());
            mainChunk.SaveToByteList(DatabaseDataType.ScriptNeedFix, db.ScriptNeedFix);
            mainChunk.SaveToByteList(DatabaseDataType.DatabaseVariables, db.Variables.ToList().ToString(true));

            foreach (var thisRow in db.Row) {

                #region Chunk bestimmen

                var rowchunk = mainChunk;
                if (chunksAllowed && db.Column.SplitColumn is { } spc) {
                    var chunkname = thisRow.GetChunkName(chunksAllowed);
                    if (string.IsNullOrEmpty(chunkname)) { return null; }

                    rowchunk = chunks.Get(chunkname);

                    if (rowchunk == null) {
                        rowchunk = new DatabaseChunk(db.Filename, chunkname);
                        rowchunk.InitByteList();
                        chunks.Add(rowchunk);
                    }
                }

                #endregion

                rowchunk.SaveToByteList(thisRow);
            }

            if (x != db.LastChange) { return null; } // Works haben sich evtl. geändert

            // Beim Erstellen des Undo-Speichers die Undos nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            List<string> works2 = [];
            foreach (var thisWorkItem in db.Undo) {
                if (thisWorkItem != null) {
                    if (thisWorkItem.LogsUndo(db)) {
                        works2.Add(thisWorkItem.ParseableItems().FinishParseable());
                    }
                }
            }

            const int undoCount = 5000;
            if (works2.Count > undoCount) { works2.RemoveRange(0, works2.Count - undoCount); }
            undoChunk.SaveToByteList(DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));

            long l = 0;
            foreach (var thisChunk in chunks) {
                mainChunk.SaveToByteListEOF();
                l += thisChunk.DataLenght;
            }

            if (l < minLen) { return null; }

            if (x != db.LastChange) { return null; } // Stand stimmt nicht mehr

            return chunks;
        } catch {
            Develop.CheckStackForOverflow();
            return GenerateNewChunks(db, minLen, fileStateUtcDateToSave, chunksAllowed);
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

    public static string GetChunkName(Database db, string value) {
        if (db.Column.SplitColumn is not { } spc) { return string.Empty; }

        switch (spc.Function) {
            case ColumnFunction.Split_Medium:
                return value.GetHashString().Right(2).ToLower();

            case ColumnFunction.Split_Large:
                return value.GetHashString().Right(3).ToLower();

            case ColumnFunction.Split_Name:
                var t = ColumnItem.MakeValidColumnName(value);
                return string.IsNullOrEmpty(t) ? "_" : t.ToLower().Left(10);
        }

        return string.Empty;
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
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 1:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 2:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 3:
                        pf = Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 4:
                        pf = Application.StartupPath + "\\" + name;
                        break;

                    case 5:
                        pf = Application.StartupPath + "\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 6:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 7:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 8:
                        // warscheinlich BeCreative, Firma
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 9:
                        // Bildzeichen-Liste, Firma, 25.10.2021
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
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

    public static (int pointer, DatabaseDataType type, string value, string colName, string rowKey) Parse(byte[] bLoaded, int pointer) {
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

            case Routinen.CellFormatUTF8_V403: {
                    type = DatabaseDataType.UTF8Value_withoutSizeData;

                    var lenghtColumnKey = NummerCode1(bLoaded, pointer + 1);
                    var columnKeyByte = new byte[lenghtColumnKey];
                    Buffer.BlockCopy(bLoaded, pointer + 2, columnKeyByte, 0, lenghtColumnKey);
                    colName = columnKeyByte.ToStringUtf8();

                    var lenghtRowKey = NummerCode1(bLoaded, pointer + 2 + lenghtColumnKey);
                    var rowKeyByte = new byte[lenghtRowKey];
                    Buffer.BlockCopy(bLoaded, pointer + 3 + lenghtColumnKey, rowKeyByte, 0, lenghtRowKey);
                    rowKey = rowKeyByte.ToStringUtf8();

                    var lenghtValue = NummerCode2(bLoaded, pointer + 3 + lenghtRowKey + lenghtColumnKey);
                    var valueByte = new byte[lenghtValue];
                    Buffer.BlockCopy(bLoaded, pointer + 5 + lenghtRowKey + lenghtColumnKey, valueByte, 0, lenghtValue);
                    value = valueByte.ToStringUtf8();

                    pointer += 5 + lenghtRowKey + lenghtValue + lenghtColumnKey;

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

    /// <summary>
    /// Standardisiert Benutzergruppen und eleminiert unterschiedliche Groß/Klein-Schreibweisen
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static List<string> RepairUserGroups(IEnumerable<string> e) {
        var l = new List<string>();

        e = e.JoinWith("|").SplitAndCutBy("|").Distinct();
        e = e.JoinWithCr().SplitAndCutByCr().Distinct();

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
        // Used: Only BZL, nach Chunks Obsolete
        var chunks = GenerateNewChunks(db, minLen, db.FileStateUtcDate, false);

        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return false; }

        return mainchunk.Save(filn, minLen);
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Database is not { IsDisposed: false } db) { return string.Empty; }

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
    public virtual bool AmITemporaryMaster(int ranges, int rangee) {
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; }
        return true;
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

        string chunk = row?.GetChunkName(true) ?? Database.Chunk_MainData;

        if (command == DatabaseDataType.ColumnSystemInfo) { chunk = Database.Chunk_AdditionalUseCases; }

        if (!ReadOnly) {
            var f2 = WriteValueToDiscOrServer(command, changedTo, column, row, user, datetimeutc, comment, chunk);
            if (!string.IsNullOrEmpty(f2)) { return f2; }
        }

        var (error, _, _) = SetValueInternal(command, column, row, changedTo, user, datetimeutc, Reason.SetCommand, chunk);
        if (!string.IsNullOrEmpty(error)) { return error; }

        if (LogUndo) {
            AddUndo(command, column, row, previousValue, changedTo, user, datetimeutc, comment, "[Änderung in dieser Session]", chunk);
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

        if (l.Get(ScriptEventTypes.correct_changed).Count > 1) {
            return "Skript 'Fehlerfrei verändert' mehrfach vorhanden";
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
        // Used: Only BZL
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
        //GlobalScale = sourceDatabase.GlobalScale;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        //RulesScript = sourceDatabase.RulesScript;
        if (SortDefinition == null || SortDefinition.ParseableItems().FinishParseable() != sourceDatabase.SortDefinition?.ParseableItems().FinishParseable()) {
            if (sourceDatabase.SortDefinition != null) {
                SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition.ParseableItems().FinishParseable());
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

        EventScript = sourceDatabase.EventScript;

        if (tagsToo) {
            Variables = sourceDatabase.Variables;
        }
    }

    public virtual ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".bdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f, FreezedReason);
    }

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool dbVariables, bool virtualcolumns, bool extendedVariable, bool addSysCorrect) {

        #region Variablen für Skript erstellen

        VariableCollection vars = [];

        if (row is { IsDisposed: false }) {
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
        vars.Add(new VariableString("UserGroup", UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        vars.Add(new VariableBool("Administrator", IsAdministrator(), true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));
        vars.Add(new VariableString("Tablename", TableName, true, "Der aktuelle Tabellenname."));
        vars.Add(new VariableBool("ReadOnly", ReadOnly, true, "Ob die aktuelle Datenbank schreibgeschützt ist."));
        vars.Add(new VariableFloat("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Datenbank")); // RowCount als Befehl belegt

        if (addSysCorrect) {
            vars.Add(new VariableBool("sys_correct", row?.CellGetBoolean(Column.SysCorrect) ?? true, true, "Der aktuelle Zeilenstand, ob die Zeile laut Skript Fehler enthält."));
        }

        if (Column.First() is { IsDisposed: false } fc) {
            vars.Add(new VariableString("NameOfFirstColumn", fc.KeyName, true, "Der Name der ersten Spalte"));

            if (fc.ScriptType != ScriptType.Nicht_vorhanden && row != null) {
                vars.Add(new VariableString("ValueOfFirstColumn", row.CellGetString(fc), true, "Der Wert der ersten Spalte als String"));
            }
        }

        vars.Add(new VariableBool("Successful", true, false, "Marker, ob das Skript erfolgreich abgeschlossen wurde."));
        vars.Add(new VariableString("NotSuccessfulReason", string.Empty, false, "Die letzte Meldung, warum es nicht erfolgreich war."));
        vars.Add(new VariableBool("Extended", extendedVariable, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));

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

    public string EditableErrorReason(EditableErrorReasonType mode) {
        if (IsDisposed) { return "Datenbank verworfen."; }

        if (DoingChanges > 0) { return "Aktuell läuft ein kritischer Prozess, Änderungen werden nachgeladen."; }

        if (mode is EditableErrorReasonType.OnlyRead) { return string.Empty; }

        //if (!string.IsNullOrEmpty(Filename) && IsInCache.Year < 2000) { return "Datenbank wird noch geladen"; }

        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }

        if (ReadOnly && mode.HasFlag(EditableErrorReasonType.Save)) { return "Datenbank schreibgeschützt!"; }

        if (mode.HasFlag(EditableErrorReasonType.EditCurrently) || mode.HasFlag(EditableErrorReasonType.Save)) {
            if (Row.HasPendingWorker()) { return "Es müssen noch Daten überprüft werden."; }
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) {
            return "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern.";
        }

        ////----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
        //if (mode.HasFlag(EditableErrorReasonType.Load) || mode.HasFlag(EditableErrorReasonType.LoadForCheckingOnly)) {
        //    if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        //}

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
            if (ExecutingScript.Count > 0) { return "Es wird noch ein Skript ausgeführt."; }
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

        foreach (var thisCom in Method.AllMethods) {
            if (thisCom.Verwendung.Count < 3) {
                if (ev.Script.ContainsWord(thisCom.Command + thisCom.StartSequence, RegexOptions.IgnoreCase)) {
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
    /// <param name="script">Wenn keine DatabaseScriptDescription ankommt, hat die Vorroutine entschieden, dass alles ok ist</param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="attributes"></param>
    /// <param name="dbVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns></returns>
    public ScriptEndedFeedback ExecuteScript(DatabaseScriptDescription? script, bool produktivphase, RowItem? row, List<string>? attributes, bool dbVariables, bool extended) {
        var name = script?.KeyName ?? "Allgemein";

        var sce = CheckScriptError();
        if (!string.IsNullOrEmpty(sce)) { return new ScriptEndedFeedback("Die Skripte enthalten Fehler: " + sce, false, true, name); }

        var e = new CancelReasonEventArgs();
        OnCanDoScript(e);
        if (e.Cancel) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + e.CancelReason, false, false, name); }

        var m = EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + m, false, false, name); }

        if (script == null) {
            // Wenn keine DatabaseScriptDescription ankommt, hat die Vorroutine entschieden, dass alles ok ist
            var vars = CreateVariableCollection(row, true, dbVariables, true, false, false);
            return new ScriptEndedFeedback(vars, string.Empty);
        }

        if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, name); }
        if (!script.NeedRow) { row = null; }

        var n = row?.CellFirstString() ?? "ohne Zeile";

        var scriptId = $"{Caption}/{name}/{n}";

        ExecutingScript.Add(scriptId);
        ExecutingScriptAnyDatabase.Add(scriptId);
        try {
            var rowstamp = string.Empty;

            object addinfo = this;
            if (row is { IsDisposed: false }) {
                rowstamp = row.RowStamp();
                addinfo = row;
            }

            var vars = CreateVariableCollection(row, script.AllVariabelsReadOnly, dbVariables, script.VirtalColumns, extended, script.AddSysCorrect);

            var meth = Method.GetMethods(script.AllowedMethods(row, extended));

            if (script.VirtalColumns) { meth.Add(Method_SetError.Method); }

            #region Script ausführen

            var scp = new ScriptProperties(script.KeyName, meth, produktivphase, script.Attributes(), addinfo, 0);

            vars.Add(new VariableString("AdditionalFilesPfad", (AdditionalFilesPfadWhole().Trim("\\") + "\\").CheckPath(), true, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));

            var sc = new Script(vars, scp) {
                ScriptText = script.Script
            };

            var scf = sc.Parse(0, script.KeyName, attributes);

            #endregion

            #region Fehlerprüfungen

            if (string.IsNullOrEmpty(ScriptNeedFix)) {
                if (scf is { AllOk: false, ScriptNeedFix: true }) {
                    var t = "Datenbank: " + Caption + "\r\n" +
                                      "Benutzer: " + UserName + "\r\n" +
                                      "Zeit (UTC): " + DateTime.UtcNow.ToString5() + "\r\n" +
                                      "Extended: " + extended + "\r\n";
                    if (row is { } r) { t = t + "Zeile: " + r.CellFirstString() + "\r\n"; }

                    ScriptNeedFix = t + "\r\n\r\n\r\n" + scf.ProtocolText;
                }
            }

            if (!scf.AllOk) {
                ExecutingScript.Remove(scriptId);
                ExecutingScriptAnyDatabase.Remove(scriptId);
                OnDropMessage(FehlerArt.Info, "Das Skript '" + script.KeyName + "' hat einen Fehler verursacht\r\n" + scf.Protocol[0]);
                return scf;
            }

            if (row != null) {
                if (row.IsDisposed) {
                    ExecutingScript.Remove(scriptId);
                    ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Die geprüfte Zeile wurde verworden", false, false, script.KeyName);
                }

                if (Column.SysRowChangeDate is null) {
                    ExecutingScript.Remove(scriptId);
                    ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Zeilen können nur geprüft werden, wenn Änderungen der Zeile geloggt werden.", false, false, script.KeyName);
                }

                if (row.RowStamp() != rowstamp) {
                    ExecutingScript.Remove(scriptId);
                    ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Zeile wurde während des Skriptes verändert.", false, false, script.KeyName);
                }
            }

            if (!produktivphase) {
                ExecutingScript.Remove(scriptId);
                ExecutingScriptAnyDatabase.Remove(scriptId);
                return scf;
            }

            #endregion

            #region Variablen zurückschreiben

            if (!script.AllVariabelsReadOnly && produktivphase) {
                if (row is { IsDisposed: false }) {
                    foreach (var thisCol in Column) {
                        row.VariableToCell(thisCol, vars, script.KeyName);
                    }
                }
                if (dbVariables) {
                    Variables = VariableCollection.Combine(Variables, vars, "DB_");
                }
            }

            if (script.VirtalColumns) {
                if (row is { IsDisposed: false } r) {
                    foreach (var thisCol in Column) {
                        if (thisCol.Function == ColumnFunction.Virtuelle_Spalte) {
                            r.VariableToCell(thisCol, vars, script.KeyName);
                        }
                    }
                }
            }

            #endregion

            ExecutingScript.Remove(scriptId);
            ExecutingScriptAnyDatabase.Remove(scriptId);

            if (produktivphase && ExecutingScript.Count == 0 && ExecutingScriptAnyDatabase.Count == 0) {
                RowCollection.DoAllInvalidatedRows(row, extended);
            }

            return scf;
        } catch {
            Develop.CheckStackForOverflow();
            ExecutingScript.Remove(scriptId);
            ExecutingScriptAnyDatabase.Remove(scriptId);
            return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended);
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
            scriptname ??= string.Empty;

            if (eventname != null && !string.IsNullOrWhiteSpace(scriptname)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrWhiteSpace(scriptname)) {
                return new ScriptEndedFeedback("Weder Eventname noch Skriptname angekommen", false, false, "Allgemein");
            }

            if (string.IsNullOrWhiteSpace(scriptname) && eventname != null) {
                var l = EventScript.Get((ScriptEventTypes)eventname);
                if (l.Count == 1) {
                    return ExecuteScript(l[0], produktivphase, row, attributes, dbVariables, extended);
                }

                return ExecuteScript(null, produktivphase, row, attributes, dbVariables, extended);
            }

            var script = EventScript.Get(scriptname);
            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }
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

        foreach (var thisRow in sortedRows) {
            if (thisRow is { IsDisposed: false }) {
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

    public bool HasChangedChunks() {
        return _chunks.Values.Any(chunk => chunk.DataChanged);
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
                            c = Column.GenerateAndAdd(thisc.KeyName, thisc.Caption, null, string.Empty);
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
                var ok = Save();
                if (!ok) { return "Speicher-Fehler!"; }
                db.Dispose();
                var d = DeleteFile(thisFile, false);
                if (!d) { return "Lösch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public string ImportCsv(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        var f = EditableErrorReason(EditableErrorReasonType.EditNormaly);

        if (!string.IsNullOrEmpty(f)) {
            OnDropMessage(FehlerArt.Warnung, "Abbruch, " + f);
            return "Abbruch, " + f;
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
                var cv = thisR.CellFirstString().ToUpperInvariant();
                if (!string.IsNullOrEmpty(cv) && !dictVorhanden.ContainsKey(cv)) {
                    dictVorhanden.Add(cv, thisR);
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

    public bool LoadAllChunks() {
        if (IsDisposed) { return false; }
        if (string.IsNullOrEmpty(Filename)) { return true; }
        //Column.GetSystems();
        //if (Column.SplitColumn == null) { return true; }

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (!DirectoryExists(chunkPath)) { return true; }

        var chunkFiles = Directory.GetFiles(chunkPath, $"*.{Filename.FileSuffix()}c");
        foreach (var file in chunkFiles) {
            var chunkName = file.FileNameWithoutSuffix();

            var (_, ok) = LoadChunkWithChunkId(chunkName, true);
            if (!ok) { return false; }
        }

        return true;
    }

    public virtual void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(FehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname nicht angegeben!"); }

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

        //OnLoading();

        var MainChunk = new DatabaseChunk(Filename, Chunk_MainData);
        MainChunk.LoadBytesFromDisk();

        if (MainChunk.DataLenght < 1) { return; }

        if (!Parse(MainChunk, needPassword)) { return; }

        // Jetzt prüfen auf Split-Column und Chunks nachladen
        if (Column.SplitColumn != null) {
            var adChunk = new DatabaseChunk(Filename, Chunk_AdditionalUndo);
            adChunk.LoadBytesFromDisk();
            Parse(adChunk, needPassword);

            var usesChunk = new DatabaseChunk(Filename, Chunk_AdditionalUseCases);
            usesChunk.LoadBytesFromDisk();
            Parse(usesChunk, needPassword);
        }

        if (FileStateUtcDate.Year < 2000) {
            FileStateUtcDate = new DateTime(2000, 1, 1);
        }
        IsInCache = FileStateUtcDate;

        CheckSysUndoNow([this], true);

        RepairAfterParse();

        if (ronly) { SetReadOnly(); }
        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        //OnLoaded();

        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null, true, false);
    }

    public void LoadFromStream(Stream stream) {
        //OnLoading();
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

        var MainChunk = new DatabaseChunk(bLoaded);
        Parse(MainChunk, null);

        RepairAfterParse();
        Freeze("Stream-Datenbank");
        //OnLoaded();
        //CreateWatcher();
        //_ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
    }

    public string NextRowKey() {
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
                                      not ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems and
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

        if (Column.SysChapter is { IsDisposed: false } c) {
            var x = c.Contents();
            if (x.Count < 2) {
                Column.Remove(c, "Automatische Optimierung");
            }
        }
    }

    public bool PermissionCheck(IList<string>? allowed, RowItem? row) {
        try {
            if (IsAdministrator()) { return true; }
            if (PowerEdit) { return true; }
            if (allowed is not { Count: not 0 }) { return false; }
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

        if (Column.SysRowCreator is { IsDisposed: false } src && string.Equals(allowed, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase)) {
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

    public void ReorganizeChunks() {
        if (IsDisposed) { return; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        #region Erst alle Chunks laden

        if (!LoadAllChunks()) {
            Develop.DebugPrint(FehlerArt.Fehler, "Fehler beim Chunk laden!");
            return;
        }

        #endregion

        _chunks.Clear(); // Chunks im Speicher löschen, werden eh neue erzeugt

        #region alte Chunk-Dateien löschen

        var chunkPath = $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

        if (DirectoryExists(chunkPath)) {
            var chunkFiles = Directory.GetFiles(chunkPath, $"*.{Filename.FileSuffix()}c");

            foreach (var file in chunkFiles) {
                DeleteFile(file, false);
            }
        }

        #endregion

        SaveInternal(FileStateUtcDate);
    }

    public void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten DatabaseMU nicht mehr funktioniert

        if (!string.IsNullOrEmpty(EditableErrorReason(this, EditableErrorReasonType.EditAcut))) { return; }

        Column.Repair();

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(TableName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Tabellenname stimmt nicht: " + Filename);
            }
        }

        Row.Repair();

        SortDefinition?.Repair();

        PermissionGroupsNewRow = RepairUserGroups(PermissionGroupsNewRow).AsReadOnly();
        DatenbankAdmin = RepairUserGroups(DatenbankAdmin).AsReadOnly();

        if (string.IsNullOrEmpty(_scriptNeedFix)) { ScriptNeedFix = CheckScriptError(); }

        //if (EventScriptVersion < Date) { EventScriptVersion = 1; }

        OnAdditionalRepair();
    }

    public virtual bool Save() {
        if (_isInSave) { return false; }

        if (!HasChangedChunks()) { return true; }

        _isInSave = true;
        var v = SaveInternal(FileStateUtcDate);
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

        var chunks = GenerateNewChunks(this, 100, FileStateUtcDate, false);

        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return; }

        mainchunk.Save(newFileName, 100);
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

    internal string IsChunkEditable(string chunkid) {
        var (_, ok) = LoadChunkWithChunkId(chunkid, true);

        if (!ok) { return "Chunk Lade-Fehler"; }

        if (!_chunks.TryGetValue(chunkid, out var chunk)) {
            return "Interner Chunk-Fehler";
        }

        var row = Row.RowsOfChunk(chunkid);

        return chunk.IsEditable(row);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="important">Steuert, ob es dringend nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns>Ob ein Load stattgefunden hat</returns>
    internal (bool loaded, bool ok) LoadChunkfromValue(string value, bool important) {
        var chunkname = GetChunkName(this, value);

        if (string.IsNullOrEmpty(chunkname)) { return (false, false); }

        return LoadChunkWithChunkId(chunkname, important);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="chunkname"></param>
    /// <param name="important">Steuert, ob es dringen nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns>Ob ein Load stattgefunden hat</returns>
    internal (bool loaded, bool ok) LoadChunkWithChunkId(string chunkname, bool important) {
        if(Column.SplitColumn == null) { return (false,true); }


        if (_chunks.TryGetValue(chunkname, out var chk)) {
            if (chk.LoadFailed) { return (false, false); }
            if (!chk.NeedsReload(important)) { return (false, true); }
        }

        OnDropMessage(FehlerArt.Info, $"Lade Chunk '{chunkname}' der Datenbank {Caption}");

        var chunk = new DatabaseChunk(Filename, chunkname);
        chunk.LoadBytesFromDisk();

        //if (chunk.LoadBytesFromDisk == null) { return false; }

        return (true, Parse(chunk, null));
    }

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
    protected void AddUndo(DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container, string chunkId) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == DatabaseDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(TableName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container, chunkId));

        if (_chunks.TryGetValue(Chunk_AdditionalUndo, out var chunk)) { chunk.DataChanged = true; }
    }

    protected virtual List<ConnectionInfo>? AllAvailableTables(List<Database>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is { IsDisposed: false } db) {
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
            if (_checker != null) {
                _checker.Dispose();
            }

            _checker = new Timer(Checker_Tick);
            _ = _checker.Change(2000, 2000);
        }
    }

    protected virtual void DidLastChanges() { }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            try {
                OnDisposingEvent();

                UnregisterEvents();

                // Timer zuerst disposen
                if (_checker != null) {
                    _checker.Dispose();
                    _checker = null;
                }

                if (_pendingChangesTimer != null) {
                    _pendingChangesTimer.Dispose();
                    _pendingChangesTimer = null;
                }

                // Dann Collections disposen
                Column.Dispose();
                Row.Dispose();

                // Listen leeren
                _chunks.Clear();
                Undo.Clear();
                _eventScript.Clear();
                _variables.Clear();
                _datenbankAdmin.Clear();
                _permissionGroupsNewRow.Clear();
                _tags.Clear();

                // Aus statischer Liste entfernen
                _ = AllFiles.Remove(this);
            } catch (Exception ex) {
                Develop.DebugPrint(FehlerArt.Fehler, "Fehler beim Dispose: " + ex.Message);
            }

            IsDisposed = true;
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
    protected virtual void DoWorkAfterLastChanges(List<string>? files, List<ColumnItem> columnsAdded, List<RowItem> rowsAdded, DateTime starttimeUtc, DateTime endTimeUtc) {
    }

    protected virtual (List<UndoItem>? Changes, List<string>? Files) GetLastChanges(IEnumerable<Database> db, DateTime startTimeUtc, DateTime endTimeUtc) => ([], null);

    protected virtual List<Database> LoadedDatabasesWithSameServer() => [this];

    protected void OnAdditionalRepair() {
        if (IsDisposed) { return; }
        //IsInCache = FileStateUTCDate;
        AdditionalRepair?.Invoke(this, System.EventArgs.Empty);
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
        if (Develop.AllReadOnly) { return true; }

        var m = EditableErrorReason(EditableErrorReasonType.Save);

        if (!string.IsNullOrEmpty(m)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return false; }

        Develop.SetUserDidSomething();

        #region Neue Chunks-Erstellen

        var chunksnew = GenerateNewChunks(this, 1200, setfileStateUtcDateTo, true);
        if (chunksnew == null || chunksnew.Count == 0) { return false; }
        foreach (var thisChunk in chunksnew) {
            if (_chunks.TryGetValue(thisChunk.KeyName, out var existingChunk)) {
                if (existingChunk == null || existingChunk.DataChanged) {
                    if (!thisChunk.DoExtendedSave(5)) {
                        return false;
                    }

                    _ = _chunks.TryRemove(thisChunk.KeyName, out _); // Den alten Fehlerhaften Chunk entfernen
                    _chunks.TryAdd(thisChunk.KeyName, thisChunk); // den neuen korrigierten dafür hinzufügen
                }
            }
        }

        #endregion

        #region Nun gibt es noch Chunk-Leichen

        // Wenn aus einem Chunk alle Daten gelöscht wurden, den Chunk auch löschen
        var chunks = new List<DatabaseChunk>();
        chunks.AddRange(_chunks.Values);
        foreach (var thisChunk in chunks) {
            if (thisChunk.DataChanged) {
                thisChunk.Delete();
                _ = _chunks.TryRemove(thisChunk.KeyName, out _); // Den alten Fehlerhaften Chunk entfernen
                //thisChunk.InitByteList();
                //if (!thisChunk.DoExtendedSave(5)) { return false; }
            }
        }

        #endregion

        FileStateUtcDate = setfileStateUtcDateTo;
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
    protected (string Error, ColumnItem? Columnchanged, RowItem? Rowchanged) SetValueInternal(DatabaseDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, Reason reason, string chunk) {
        if (IsDisposed) { return ("Datenbank verworfen!", null, null); }
        if ((reason != Reason.NoUndo_NoInvalidate) && !string.IsNullOrEmpty(FreezedReason)) { return ("Datenbank eingefroren: " + FreezedReason, null, null); }
        if (type.IsObsolete()) { return (string.Empty, null, null); }

        LastChange = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(chunk)) {
            LoadChunkWithChunkId(chunk, true);
        }

        if (type.IsCellValue()) {
            if (column?.Database is not { IsDisposed: false } db) { return (string.Empty, column, row); }
            if (row == null) { return (string.Empty, column, row); }
            if (column.Function == ColumnFunction.Virtuelle_Spalte) { return (string.Empty, column, row); }

            //column.Invalidate_ContentWidth();
            //row.InvalidateCheckData();

            var f = row.SetValueInternal(column, value, reason);

            if (!string.IsNullOrEmpty(f)) { return (f, null, null); }
            row.DoSystemColumns(db, column, user, datetimeutc, reason);

            return (string.Empty, column, row);
        }

        if (type.IsColumnTag()) {
            if (column is not { IsDisposed: false } || Column.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type);
                //return ("Wert nicht gesetzt!", null, null);
                return (string.Empty, null, null);
            }

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
                    return (Row.ExecuteCommand(type, r.KeyName, reason, user, datetimeutc), null, r);

                case DatabaseDataType.Command_AddRow:
                    var f1 = Row.ExecuteCommand(type, value, reason, user, datetimeutc);
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
            //case DatabaseDataType.Formatkennung:
            //    break;

            case DatabaseDataType.Version:
                LoadedVersion = value.Trim();
                break;

            case DatabaseDataType.LastEditUser:
                break;

            case DatabaseDataType.LastEditApp:
                break;

            case DatabaseDataType.LastEditMachineName:
                break;

            case DatabaseDataType.LastEditTimeUTC:
                break;

            case DatabaseDataType.Werbung:
                break;

            case DatabaseDataType.Creator:
                _creator = value;
                break;

            case DatabaseDataType.FileStateUTCDate:
                FileStateUtcDate = DateTimeParse(value);
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

            //case DatabaseDataType.GlobalScale:
            //    _globalScale = FloatParse(value);
            //    break;

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
                Row.InvalidateAllCheckData();
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
                _columnArrangements = value;
                break;

            case DatabaseDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            case DatabaseDataType.EventScriptVersion:
                _eventScriptVersion = DateTimeParse(value);
                break;

            case DatabaseDataType.ScriptNeedFix:
                _scriptNeedFix = value;
                Row.InvalidateAllCheckData();
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

    protected virtual string WriteValueToDiscOrServer(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, string user, DateTime datetimeutc, string comment, string chunkId) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren!"; } // Sicherheitshalber!
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }

        if (_chunks.TryGetValue(chunkId, out var chk)) {
            chk.DataChanged = true;
        }

        return string.Empty;
    }

    private static void CheckSysUndo(object state) {
        if (DateTime.UtcNow.Subtract(_timerTimeStamp).TotalSeconds < 240) { return; }
        //if (DateTime.UtcNow.Subtract(LastLoadUtc).TotalSeconds < 180) { return; }

        if (CriticalState()) { return; }
        CheckSysUndoNow(AllFiles, false);
    }

    private static bool CriticalState() {
        foreach (var thisDb in AllFiles) {
            if (thisDb is { IsDisposed: false, IsInCache.Year: < 2000 }) {
                //if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (!string.IsNullOrEmpty(thisDb.Filename)) { return true; } // Irgend eine Datenbank wird aktuell geladen
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

        var changed = !HasChangedChunks();

        if (!changed) {
            _checkerTickCount = 0;
            return;
        }

        if (((_checkerTickCount > 20 && DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds > 20) ||
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
    /// <param name="startTimeUtc">Nur um die Zeot stoppen zu können und lange Prozesse zu kürzen</param>
    /// <param name="endTimeUtc"></param>
    private void DoLastChanges(List<string>? files, List<UndoItem>? data, DateTime startTimeUtc, DateTime endTimeUtc) {
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
                    var (error, columnchanged, rowchanged) = SetValueInternal(thisWork.Command, c, r, thisWork.ChangedTo, thisWork.User, thisWork.DateTimeUtc, Reason.NoUndo_NoInvalidate, thisWork.Chunk);

                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Datenbank-Fehler: " + error + " " + thisWork.ParseableItems().FinishParseable());
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
            IsInCache = endTimeUtc;
            DoWorkAfterLastChanges(myfiles, columnsAdded, rowsAdded, startTimeUtc, endTimeUtc);
            OnInvalidateView();
        } catch {
            Develop.CheckStackForOverflow();
            DoLastChanges(files, data, startTimeUtc, endTimeUtc);
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
        lock (this) {
            if (_pendingChangesTimer != null) { return; }
            _timerTimeStamp = DateTime.UtcNow.AddMinutes(-5);
            _pendingChangesTimer = new Timer(CheckSysUndo);
            _ = _pendingChangesTimer.Change(10000, 10000);
        }
    }

    private bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is { IsDisposed: false } db) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    _ = thisFile.Save();
                    Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private bool Parse(DatabaseChunk chunk, NeedPassword? needPassword) {
        if (chunk.LoadFailed) { return false; }

        OnLoading();

        var rowsToRemove = Row.RowsOfChunk(chunk.KeyName);

        if (rowsToRemove.Count > 0) {
            // Zeilen und zugehörige Zellen entfernen
            foreach (var row in rowsToRemove) {
                Row.ExecuteCommand(DatabaseDataType.Command_RemoveRow, row.KeyName, Reason.NoUndo_NoInvalidate, null, null);
            }

            // Verwaiste Zellen entfernen
            Cell.RemoveOrphans();
        }

        var pointer = 0;
        var columnUsed = new List<ColumnItem>();

        if (chunk.IsMain) {
            _chunks.Clear();
            Undo.Clear();
            Row.RemoveNullOrEmpty();
            Cell.Clear();
        }

        _ = _chunks.TryRemove(chunk.KeyName, out _);

        if (chunk.Bytes.Length == 0) {
            // Datei gelöscht
            return true;
        }

       if(! _chunks.TryAdd(chunk.KeyName, chunk)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Chunk nicht eingespielt!");
            return false;
        }

        var data = chunk.Bytes;

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
                        if (row is not { IsDisposed: false }) {
                            _ = Row.ExecuteCommand(DatabaseDataType.Command_AddRow, rowKey, Reason.NoUndo_NoInvalidate, null, null);
                            row = Row.SearchByKey(rowKey);
                        }

                        if (row is not { IsDisposed: false }) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Zeile hinzufügen Fehler");
                            Freeze("Zeile hinzufügen Fehler");
                            return false;
                        }
                    }

                    #endregion

                    #region Spalte suchen oder erstellen

                    if (!string.IsNullOrEmpty(columname)) {
                        column = Column[columname];
                        if (column is not { IsDisposed: false }) {
                            if (command != DatabaseDataType.ColumnName) {
                                Develop.DebugPrint(command + " an erster Stelle!");
                            }

                            _ = Column.ExecuteCommand(DatabaseDataType.Command_AddColumnByName, columname, Reason.NoUndo_NoInvalidate);
                            column = Column[columname];
                        }

                        if (column is not { IsDisposed: false }) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                            Freeze("Spalte hinzufügen Fehler");
                            return false;
                        }

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

                    var fehler = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, Reason.NoUndo_NoInvalidate, string.Empty);
                    if (!string.IsNullOrEmpty(fehler.Error)) {
                        Freeze("Datenbank-Ladefehler");
                        Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + TableName + "<br>Meldung: " + fehler);
                        return false;
                    }
                }
            } while (true);
        } catch {
            Freeze("Parse Fehler!");
        }

        if (chunk.IsMain) {

            #region unbenutzte (gelöschte) Spalten entfernen

            var l = new List<ColumnItem>();
            foreach (var thisColumn in Column) {
                l.Add(thisColumn);
            }

            foreach (var thisColumn in l) {
                if (!columnUsed.Contains(thisColumn)) {
                    _ = Column.ExecuteCommand(DatabaseDataType.Command_RemoveColumn, thisColumn.KeyName, Reason.NoUndo_NoInvalidate);
                }
            }

            #endregion

            Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) { Freeze("Datenbankversions-Konflikt"); }
        OnLoaded();

        return true;
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

    private void UnregisterEvents() {
        try {
            // QuickImage Event
            QuickImage.NeedImage -= QuickImage_NeedImage;

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
            DropMessage = null;
            InvalidateView = null;
            Loaded = null;
            Loading = null;
            ProgressbarInfo = null;
            SortParameterChanged = null;
            ViewChanged = null;

            // EventScript Events
            foreach (var script in _eventScript) {
                script.PropertyChanged -= EventScript_PropertyChanged;
            }
        } catch (Exception ex) {
            Develop.DebugPrint(FehlerArt.Warnung, "Fehler beim Abmelden der Events: " + ex.Message);
        }
    }

    #endregion
}