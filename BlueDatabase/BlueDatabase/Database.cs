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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using Timer = System.Threading.Timer;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Database : IDisposableExtendedWithEvent, IHasKeyName, IEditable {

    #region Fields

    public const string DatabaseVersion = "4.10";

    public static readonly ObservableCollection<Database> AllFiles = [];

    public static int MaxMasterCount = 3;

    /// <summary>
    /// Wenn diese Varianble einen Count von 0 hat, ist der Speicher nicht initialisiert worden.
    /// </summary>
    public readonly List<UndoItem> Undo;

    /// <summary>
    /// Wann die Datenbank zuletzt angeschaut / ge�ffnet / geladen wurde.
    /// Bestimmt die Reihenfolge der Reparaturen
    /// </summary>
    public DateTime LastUsedDate = DateTime.UtcNow;

    /// <summary>
    /// So viele �nderungen sind seit dem letzten erstellen der Komplett-Datenbank erstellen auf Festplatte gez�hlt worden
    /// </summary>
    protected readonly List<UndoItem> ChangesNotIncluded = [];

    protected NeedPassword? _needPassword;

    private static readonly object _timerLock = new object();

    private static int _activeDatabaseCount = 0;

    private static List<string> _allavailableTables = [];

    /// <summary>
    /// Der Globale Timer, der die Datenbanken regelm��ig updated
    /// </summary>
    private static Timer? _databaseUpdateTimer;

    private static DateTime _lastAvailableTableCheck = new(1900, 1, 1);

    private readonly List<string> _datenbankAdmin = [];

    private readonly List<string> _permissionGroupsNewRow = [];

    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);
    private readonly List<string> _tags = [];

    private readonly List<Variable> _variables = [];

    private string _additionalFilesPath;

    private string _cachePfad = string.Empty;

    private string _caption = string.Empty;

    private Timer? _checker;

    private int _checkerTickCount = -5;

    private string _columnArrangements = string.Empty;

    private string _createDate;

    private string _creator;

    private ReadOnlyCollection<DatabaseScriptDescription> _eventScript = new ReadOnlyCollection<DatabaseScriptDescription>([]);

    //private ReadOnlyCollection<DatabaseScriptDescription> _eventScriptEdited = new ReadOnlyCollection<DatabaseScriptDescription>([]);

    private DateTime _eventScriptVersion = DateTime.MinValue;

    private string _globalShowPass = string.Empty;

    private bool _isInSave;

    private DateTime _powerEditTime = DateTime.MinValue;

    private bool _readOnly;

    private string _rowQuickInfo = string.Empty;

    private bool _saveRequired = false;

    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    private string _temporaryDatabaseMasterTimeUtc = string.Empty;

    private string _temporaryDatabaseMasterUser = string.Empty;

    private string _variableTmp;

    #endregion

    #region Constructors

    public Database(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgel�st wird.
        Develop.StartService();

        QuickImage.NeedImage += QuickImage_NeedImage;

        TableName = MakeValidTableName(tablename);

        if (!IsValidTableName(TableName)) {
            Develop.DebugPrint(ErrorType.Error, "Tabellenname ung�ltig: " + tablename);
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
        _additionalFilesPath = "AdditionalFiles";
        //_rowQuickInfo = string.Empty;
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
        GenerateDatabaseUpdateTimer();
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

    public event EventHandler? InvalidateView;

    public event EventHandler<FirstEventArgs>? Loaded;

    public event EventHandler? Loading;

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    public static List<string> ExecutingScriptAnyDatabase { get; } = [];

    public static string MyMasterCode => UserName + "-" + Environment.MachineName;

    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zus�tzlichen Dateien.")]
    public string AdditionalFilesPath {
        get => _additionalFilesPath;
        set {
            if (_additionalFilesPath == value) { return; }
            _additionalFilesPathTemp = null;
            _ = ChangeData(DatabaseDataType.AdditionalFilesPath, null, _additionalFilesPath, value);
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
            _ = ChangeData(DatabaseDataType.Caption, null, _caption, value);
        }
    }

    public string CaptionForEditor => "Datenbank";

    public CellCollection Cell { get; }

    public ColumnCollection Column { get; }

    public string ColumnArrangements {
        get => _columnArrangements;
        set {
            if (_columnArrangements == value) { return; }
            _ = ChangeData(DatabaseDataType.ColumnArrangement, null, _columnArrangements, value);
            OnViewChanged();
        }
    }

    public string CreateDate {
        get => _createDate;
        private set {
            if (_createDate == value) { return; }
            _ = ChangeData(DatabaseDataType.CreateDateUTC, null, _createDate, value);
        }
    }

    public string Creator {
        get => _creator.Trim();
        private set {
            if (_creator == value) { return; }
            _ = ChangeData(DatabaseDataType.Creator, null, _creator, value);
        }
    }

    public ReadOnlyCollection<string> DatenbankAdmin {
        get => new(_datenbankAdmin);
        set {
            if (!_datenbankAdmin.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.DatabaseAdminGroups, null, _datenbankAdmin.JoinWithCr(), value.JoinWithCr());
        }
    }

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public Type? Editor { get; set; }

    public ReadOnlyCollection<DatabaseScriptDescription> EventScript {
        get => _eventScript;
        set {
            var l = new List<DatabaseScriptDescription>();
            l.AddRange(value);
            l.Sort();

            var eventScriptOld = _eventScript.ToString(false);
            var eventScriptNew = l.ToString(false);

            if (eventScriptOld == eventScriptNew) { return; }
            _ = ChangeData(DatabaseDataType.EventScript, null, eventScriptOld, eventScriptNew);
        }
    }

    //public ReadOnlyCollection<DatabaseScriptDescription> EventScriptEdited {
    //    get => new(_eventScriptEdited);
    //    set {
    //        var l = new List<DatabaseScriptDescription>();
    //        l.AddRange(value);
    //        l.Sort();

    //        var eventScriptEditedOld = _eventScriptEdited.ToString(false);
    //        var eventScriptEditedNew = l.ToString(false);

    //        if (eventScriptEditedOld == eventScriptEditedNew) { return; }
    //        _ = ChangeData(DatabaseDataType.EventScriptEdited, null, eventScriptEditedOld, eventScriptEditedNew);
    //    }
    //}

    public DateTime EventScriptVersion {
        get => _eventScriptVersion;
        set {
            if (_eventScriptVersion == value) { return; }
            _ = ChangeData(DatabaseDataType.EventScriptVersion, null, _eventScriptVersion.ToString5(), value.ToString5());
        }
    }

    public List<string> ExecutingScript { get; } = [];

    public string Filename { get; protected set; } = string.Empty;

    /// <summary>
    /// Der Wert wird im System verankert und gespeichert.
    /// Bei Datenbanken, die Daten nachladen k�nnen, ist das der Stand, zu dem alle Daten fest abgespeichert sind.
    /// Kann hier nur gelesen werden! Da eine �nderung �ber die Property die Datei wieder auf ungespeichert setzen w�rde, w�rde hier eine
    /// Kettenreaktion ausgel�st werden.
    /// </summary>
    public DateTime FileStateUtcDate { get; protected set; }

    /// <summary>
    /// Der FreezedReason kann niemals wieder r�ckg�ngig gemacht werden.
    /// Weil keine Undos mehr geladen werden, w�rde da nur Chaos entstehen.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            _ = ChangeData(DatabaseDataType.GlobalShowPass, null, _globalShowPass, value);
        }
    }

    public bool IsDisposed { get; private set; }

    public string KeyName => IsDisposed ? string.Empty : TableName.ToUpper();

    public DateTime LastChange { get; private set; } = new(1900, 1, 1);

    public bool LogUndo { get; set; } = true;

    public virtual bool MasterNeeded => false;

    public virtual bool MultiUserPossible => false;

    public ReadOnlyCollection<string> PermissionGroupsNewRow {
        get => new(_permissionGroupsNewRow);
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.PermissionGroupsNewRow, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr());
        }
    }

    public bool PowerEdit {
        get => _powerEditTime.Subtract(DateTime.UtcNow).TotalSeconds > 0;

        set {
            _powerEditTime = value ? DateTime.UtcNow.AddSeconds(300) : DateTime.UtcNow.AddSeconds(-1);
            OnInvalidateView();
        }
    }

    /// <summary>
    /// Pr�ft auch den FreezedReason
    /// </summary>
    public bool ReadOnly {
        get => _readOnly || !string.IsNullOrEmpty(FreezedReason);
        private set => _readOnly = value;
    }

    public RowCollection Row { get; }

    public string RowQuickInfo {
        get => _rowQuickInfo;
        set {
            if (_rowQuickInfo == value) { return; }
            _ = ChangeData(DatabaseDataType.RowQuickInfo, null, _rowQuickInfo, value);
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
            _ = ChangeData(DatabaseDataType.SortDefinition, null, alt, neu);

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
            _ = ChangeData(DatabaseDataType.StandardFormulaFile, null, _standardFormulaFile, value);
        }
    }

    public string TableName { get; }

    public ReadOnlyCollection<string> Tags {
        get => new(_tags);
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.Tags, null, _tags.JoinWithCr(), value.JoinWithCr());
        }
    }

    public string TemporaryDatabaseMasterTimeUtc {
        get => _temporaryDatabaseMasterTimeUtc;
        set {
            if (_temporaryDatabaseMasterTimeUtc == value) { return; }
            _ = ChangeData(DatabaseDataType.TemporaryDatabaseMasterTimeUTC, null, _temporaryDatabaseMasterTimeUtc, value);
        }
    }

    public string TemporaryDatabaseMasterUser {
        get => _temporaryDatabaseMasterUser;
        set {
            if (_temporaryDatabaseMasterUser == value) { return; }
            _ = ChangeData(DatabaseDataType.TemporaryDatabaseMasterUser, null, _temporaryDatabaseMasterUser, value);
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

            _ = ChangeData(DatabaseDataType.DatabaseVariables, null, _variableTmp, l.ToString(true));
        }
    }

    /// <summary>
    /// Letzter Lade-Stand der Daten.
    /// </summary>
    protected DateTime IsInCache { get; set; } = new(0);

    protected string LoadedVersion { get; private set; }

    private string? _additionalFilesPathTemp { get; set; }

    #endregion

    #region Methods

    public static List<string> AllAvailableTables() {
        if (DateTime.UtcNow.Subtract(_lastAvailableTableCheck).TotalMinutes < 20) {
            return _allavailableTables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Datenbank z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu pr�fen
        var allreadychecked = new List<Database>();

        var alf = new List<Database>();// k�nnte sich �ndern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDb in alf) {
            var possibletables = thisDb.AllAvailableTables(allreadychecked);

            allreadychecked.Add(thisDb);

            if (possibletables != null) {
                _allavailableTables.AddRange(possibletables);
                //foreach (var thistable in possibletables) {
                ////    var canadd = true;

                ////    #region pr�fen, ob schon vorhanden, z.b. Database.AllFiles

                ////    foreach (var checkme in AllavailableTables) {
                ////        if (string.Equals(checkme.FileNameWithoutSuffix(), thistable.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                ////            canadd = false;
                ////            break;
                ////        }
                ////    }

                ////    #endregion

                ////    if (canadd) { AllavailableTables.Add(thistable); }
                //}
            }
        }
        _allavailableTables = _allavailableTables.SortedDistinctList();
        _lastAvailableTableCheck = DateTime.UtcNow;
        return _allavailableTables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
    }

    public static void BeSureToBeUpToDate(ObservableCollection<Database> ofDatabases) {
        List<Database> l = [.. ofDatabases];

        foreach (var db in l) {
            _ = db.BeSureToBeUpToDate();
        }
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

    public static void FreezeAll(string reason) {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile.Freeze(reason);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde ver�ndert! Selten, aber kann passieren!
                FreezeAll(reason);
                return;
            }
        }
    }

    public static Database? Get(string fileOrTableName, bool readOnly, NeedPassword? needPassword) {
        if (fileOrTableName.Contains("|")) {
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

        #region Schauen, ob die Datenbank bereits geladen ist

        var folder = new List<string>();

        if (fileOrTableName.IsFormat(FormatHolder.FilepathAndName)) {
            _ = folder.AddIfNotExists(fileOrTableName.FilePath());
            fileOrTableName = fileOrTableName.FileNameWithoutSuffix();
        }

        fileOrTableName = MakeValidTableName(fileOrTableName);

        foreach (var thisFile in AllFiles) {
            if (string.Equals(thisFile.TableName, fileOrTableName, StringComparison.OrdinalIgnoreCase)) {
                thisFile.WaitInitialDone();
                return thisFile;
            }

            if (thisFile.Filename.IsFormat(FormatHolder.FilepathAndName)) {
                _ = folder.AddIfNotExists(thisFile.Filename.FilePath());
            }
        }

        #endregion

        foreach (var thisfolder in folder) {
            var f = thisfolder + fileOrTableName;

            if (FileExists(f + ".cbdb")) {
                var db = new DatabaseChunk(fileOrTableName);
                db.LoadFromFile(f + ".cbdb", false, needPassword, string.Empty, readOnly);
                db.WaitInitialDone();
                return db;
            }

            if (FileExists(f + ".mbdb")) {
                var db = new DatabaseFragments(fileOrTableName);
                db.LoadFromFile(f + ".mbdb", false, needPassword, string.Empty, readOnly);
                db.WaitInitialDone();
                return db;
            }

            if (FileExists(f + ".bdb")) {
                var db = new Database(fileOrTableName);
                db.LoadFromFile(f + ".bdb", false, needPassword, string.Empty, readOnly);
                db.WaitInitialDone();
                return db;
            }
        }

        return null;
    }

    public static bool IsValidTableName(string tablename) {
        if (string.IsNullOrEmpty(tablename)) { return false; }

        var t = tablename.ToUpperInvariant();

        if (t.StartsWith("SYS_")) { return false; }
        if (t.StartsWith("BAK_")) { return false; }
        if (t.StartsWith("DATABASE")) { return false; }

        if (!tablename.IsFormat(FormatHolder.SystemName)) { return false; }

        if (tablename == "ALL_TAB_COLS") { return false; } // system-name

        // eigentlich 128, aber minus BAK_ und _2023_03_28
        return t.Length <= 100;
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
                    db.DropMessages = false;
                    db.LoadFromFile(pf, false, null, string.Empty, false);
                    return db;
                }
            } while (pf != string.Empty);
        }
        var d = GetEmmbedResource(assembly, name);
        if (d != null) {
            var db = new Database(name) {
                LogUndo = false,
                DropMessages = false
            };
            db.LoadFromStream(d);
            return db;
        }
        if (fehlerAusgeben) { Develop.DebugPrint(ErrorType.Error, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Char_PfadSonderZeichen); // sonst st�rzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
        tmp = tmp.StarkeVereinfachung("_", false).ToUpperInvariant();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    public static (int pointer, DatabaseDataType type, string value, string colName, string rowKey) Parse(byte[] bLoaded, int pointer, string filename) {
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
                    Develop.DebugPrint(ErrorType.Error, $"Laderoutine nicht definiert: {bLoaded[pointer]} {filename}");
                    break;
                }
        }

        return (pointer, type, value, colName, rowKey);
    }

    /// <summary>
    /// Standardisiert Benutzergruppen und eleminiert unterschiedliche Gro�/Klein-Schreibweisen
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
            if (x > 99999) { Develop.DebugPrint(ErrorType.Error, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToStringInt5()).RemoveChars(Char_DateiSonderZeichen + " _.");
            var ok = true;

            if (IsValidTableName(unique)) {
                foreach (var thisfile in AllFiles) {
                    if (string.Equals(unique, thisfile.TableName)) { ok = false; break; }
                }
            } else {
                ok = false;
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static bool UpdateScript(DatabaseScriptDescription script, string? keyname = null, string? scriptContent = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null) {
        if (script?.Database is not { IsDisposed: false } db) { return false; }

        if (!string.IsNullOrEmpty(db.CanWriteMainFile())) { return false; }

        var found = false;

        List<DatabaseScriptDescription> updatedScripts = [];

        foreach (var existingScript in db.EventScript) {
            if (ReferenceEquals(existingScript, script)) {
                found = true;
                // Erstelle neues Script mit aktualisierten Werten
                var newScript = new DatabaseScriptDescription(
                    existingScript.Database,
                    keyname ?? existingScript.KeyName,
                    scriptContent ?? existingScript.Script,
                    image ?? existingScript.Image,
                    quickInfo ?? existingScript.ColumnQuickInfo,
                    adminInfo ?? existingScript.AdminInfo,
                    userGroups ?? existingScript.UserGroups,
                    eventTypes ?? existingScript.EventTypes,
                    needRow ?? existingScript.NeedRow,
                    failedReason ?? existingScript.FailedReason
                );
                updatedScripts.Add(newScript);
            } else {
                updatedScripts.Add(existingScript);
            }
        }

        if (!found) {
            updatedScripts.Add(script);
        }

        db.EventScript = updatedScripts.AsReadOnly();

        return true;
    }

    /// <summary>
    /// Der komplette Pfad mit abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string AdditionalFilesPathWhole() {
        if (_additionalFilesPathTemp != null) { return _additionalFilesPathTemp; }

        if (!string.IsNullOrEmpty(_additionalFilesPath)) {
            var t = _additionalFilesPath.CheckPath();
            if (DirectoryExists(t)) {
                _additionalFilesPathTemp = t;
                return t;
            }
        }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionalFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                _additionalFilesPathTemp = t;
                return t;
            }
        }
        _additionalFilesPathTemp = string.Empty;
        return string.Empty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ranges">Unter 5 Minuten wird auch gepr�ft, ob versucht wird, einen Master zu setzen. Ab 5 minuten ist es gewiss.</param>
    /// <param name="rangee">Bis 55 Minuten ist sicher, dass es der Master ist.
    /// Werden kleiner Werte abgefragt, kann ermittelt werden, ob der Master bald ausl�uft.
    /// Werden gr��erer Werte abgefragt, kann ermittel werden, ob man Master war,
    /// </param>
    /// <returns></returns>
    public bool AmITemporaryMaster(int ranges, int rangee) {
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; }
        if (!MultiUserPossible) { return true; }
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 5) {
            if (!BeSureToBeUpToDate()) { return false; }
        }
        if (TemporaryDatabaseMasterUser != MyMasterCode) { return false; }

        var d = DateTimeParse(TemporaryDatabaseMasterTimeUtc);
        var mins = DateTime.UtcNow.Subtract(d).TotalMinutes;

        ranges = Math.Max(ranges, 0);
        //rangee = Math.Min(rangee, 55);

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogepr�ft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return mins > ranges && mins < rangee;
    }

    //    StandardFormulaFile = sourceDatabase.StandardFormulaFile;
    //    EventScriptVersion = sourceDatabase.EventScriptVersion;
    //    NeedsScriptFix = sourceDatabase.NeedsScriptFix;
    //    RowQuickInfo = sourceDatabase.RowQuickInfo;
    //    if (tagsToo) {
    //        Tags = new(sourceDatabase.Tags.Clone());
    //    }
    public virtual string AreAllDataCorrect() {
        if (IsDisposed) { return "Datenbank verworfen."; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }
        return string.Empty;
    }

    public virtual bool BeSureAllDataLoaded(int anzahl) => !IsDisposed && BeSureToBeUpToDate();

    public virtual bool BeSureRowIsLoaded(string chunkValue) => true;

    public virtual bool BeSureToBeUpToDate() {
        if (IsInCache.Year < 2000) { return false; }
        return !IsDisposed;
    }

    public bool CanDoValueChangedScript(bool returnValueCount0) => IsRowScriptPossible() && IsScriptsExecutable(ScriptEventTypes.value_changed, returnValueCount0);

    /// <summary>
    /// Konkrete Pr�fung, ob jetzt gespeichert werden kann
    /// </summary>
    /// <returns></returns>
    public string CanSaveMainChunk() {
        var f = CanWriteMainFile();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (Row.HasPendingWorker()) { return "Es m�ssen noch Daten �berpr�ft werden."; }

        if (ExecutingScript.Count > 0) { return "Es wird noch ein Skript ausgef�hrt."; }

        if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 1) { return "K�rzlich vorgenommene �nderung muss verarbeitet werden."; }

        //if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massen�nderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden f�r ein zca4

        var fileSaveResult = CanSaveFile(Filename, 5);
        return fileSaveResult;
    }

    /// <summary>
    /// Allgemeine Pr�fung, ob es generell m�glich ist, eine Bearbeitung auszuf�hren.
    /// </summary>
    /// <returns></returns>
    public string CanWriteMainFile() {
        var f = AreAllDataCorrect();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (ReadOnly) { return "Datenbank schreibgesch�tzt!"; }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) {
            return "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern.";
        }

        if (!string.IsNullOrEmpty(Filename)) {
            if (!CanWriteInDirectory(Filename.FilePath())) { return "Sie haben im Verzeichnis der Datei keine Schreibrechte."; }
        }
        return string.Empty;
    }

    public string ChangeData(DatabaseDataType command, ColumnItem? column, string previousValue, string changedTo) => ChangeData(command, column, null, previousValue, changedTo, UserName, DateTime.UtcNow, string.Empty, string.Empty, string.Empty);

    //    if (!FileExists(ci.AdditionalData)) { return null; }
    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und k�mmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
    /// Der Wert wird intern fest verankert - bei ReadOnly werden aber weitere Schritte ignoriert.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="user"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    public string ChangeData(DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, string oldchunkvalue, string newchunkvalue) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }
        if (type.IsObsolete()) { return "Obsoleter Befehl angekommen!"; }

        _saveRequired = true;

        var colName = column?.KeyName ?? string.Empty;

        // ERST Speicher setzen
        var error = SetValueInternal(type, column, row, changedTo, user, datetimeutc, Reason.SetCommand);
        if (!string.IsNullOrEmpty(error)) { return error; }

        // Bei Spaltenumbenennung auch ColumnArrangements aktualisieren
        if (type == DatabaseDataType.ColumnName && column != null) {
            UpdateColumnArrangementsAfterRename(previousValue, changedTo);
        }

        // DANN Festplatte schreiben (nur bei nicht ReadOnly)
        if (!ReadOnly) {
            var newChunkId = DatabaseChunk.GetChunkId(this, type, newchunkvalue);
            var oldChunkId = newChunkId;

            if (string.IsNullOrEmpty(oldChunkId) || newchunkvalue == oldchunkvalue) {
                oldChunkId = newChunkId;
            } else {
                oldChunkId = DatabaseChunk.GetChunkId(this, type, oldchunkvalue);
            }

            var f2 = WriteValueToDiscOrServer(type, changedTo, colName, row, user, datetimeutc, oldChunkId, newChunkId, comment);
            if (!string.IsNullOrEmpty(f2)) {
                DropMessage(ErrorType.Warning, $"Rollback aufgrund eines Fehlers:\r\n{f2}");
                // Rollback: Vorherigen Wert im Speicher wiederherstellen
                _ = SetValueInternal(type, column, row, previousValue, user, datetimeutc, Reason.NoUndo_NoInvalidate);
                return f2;
            }
        }

        if (LogUndo) {
            AddUndo(type, colName, row, previousValue, changedTo, user, datetimeutc, comment, "[�nderung in dieser Session]", newchunkvalue);
        }

        return string.Empty;
    }

    public string CheckScriptError() {
        foreach (var script in _eventScript) {
            if (!script.IsOk()) { return $"{script.KeyName}: {script.ErrorReason()}"; }
        }
        return string.Empty;
    }

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool dbVariables, bool virtualcolumns, bool extendedVariable) {

        #region Variablen f�r Skript erstellen

        VariableCollection vars = [];

        if (row is { IsDisposed: false }) {
            foreach (var thisCol in Column) {
                var v = RowItem.CellToVariable(thisCol, row, allReadOnly, virtualcolumns);
                if (v != null) { _ = vars.Add(v); }
            }
            _ = vars.Add(new VariableString("RowKey", row.KeyName, true, "Der interne Zeilenschl�ssel."));
        }

        if (dbVariables) {
            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("DB_" + thisvar.KeyName, thisvar.ValueString, false, "Datenbank-Kopf-Variable\r\n" + thisvar.Comment);
                _ = vars.Add(v);
            }
        }

        _ = vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade ge�ffnet ist."));
        _ = vars.Add(new VariableString("User", UserName, true, "ACHTUNG: Keinesfalls d�rfen benutzerabh�ngig Werte ver�ndert werden."));
        _ = vars.Add(new VariableString("UserGroup", UserGroup, true, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden."));
        _ = vars.Add(new VariableBool("Administrator", IsAdministrator(), true, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden.\r\nDiese Variable gibt zur�ck, ob der Benutzer Admin f�r diese Datenbank ist."));
        _ = vars.Add(new VariableString("Tablename", TableName, true, "Der aktuelle Tabellenname."));
        _ = vars.Add(new VariableString("Type", Filename.FileSuffix().ToUpperInvariant(), true, "Der Tabellentyp."));
        _ = vars.Add(new VariableBool("ReadOnly", ReadOnly, true, "Ob die aktuelle Datenbank schreibgesch�tzt ist."));
        _ = vars.Add(new VariableDouble("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Datenbank")); // RowCount als Befehl belegt

        //if (Column.SysCorrect is { IsDisposed: false } csc && row is { IsDisposed: false }) {
        //    _ = vars.Add(new VariableBool("sys_correct", row.CellGetBoolean(csc), true, "Der aktuelle Zeilenstand, ob die Zeile laut Skript Fehler korrekt durchgerechnet worden ist\r\nAchtung: Das ist der eingfrohrende Stand, zu Beginn des Skriptes."));
        //}

        if (Column.First is { IsDisposed: false } fc) {
            _ = vars.Add(new VariableString("NameOfFirstColumn", fc.KeyName, true, "Der Name der ersten Spalte"));

            if (row != null) {
                _ = vars.Add(new VariableString("ValueOfFirstColumn", row.CellGetString(fc), true, "Der Wert der ersten Spalte als String"));
            }
        }

        _ = vars.Add(new VariableString("AdditionalFilesPath", (AdditionalFilesPathWhole().Trim("\\") + "\\").CheckPath(), true, "Der Dateipfad, in dem zus�tzliche Daten gespeichert werden."));

        //_ = vars.Add(new VariableBool("Successful", true, true, "Marker, ob das Skript erfolgreich abgeschlossen wurde."));
        //_ = vars.Add(new VariableString("NotSuccessfulReason", string.Empty, true, "Die letzte Meldung, warum es nicht erfolgreich war."));
        _ = vars.Add(new VariableBool("Extended", extendedVariable, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));
        _ = vars.Add(new VariableListString("ErrorColumns", [], false, "Spalten, die mit SetError fehlerhaft gesetzt wurden."));

        #endregion

        return vars;
    }

    //    EventScript = sourceDatabase.EventScript;
    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Layouts und abschlie�enden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { return AdditionalFilesPathWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    //    DatenbankAdmin = new(sourceDatabase.DatenbankAdmin.Clone());
    //    PermissionGroupsNewRow = new(sourceDatabase.PermissionGroupsNewRow.Clone());
    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void EnableScript() => Column.GenerateAndAddSystem("SYS_ROWSTATE");

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
    public ScriptEndedFeedback ExecuteScript(DatabaseScriptDescription? script, bool produktivphase, RowItem? row, List<string>? attributes, bool dbVariables, bool extended, bool ignoreError) {
        var name = script?.KeyName ?? "Allgemein";
        var e = new CancelReasonEventArgs();
        OnCanDoScript(e);
        if (e.Cancel) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht m�glich: " + e.CancelReason, false, false, name); }

        var m = CanWriteMainFile();
        if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht m�glich: " + m, false, false, name); }

        if (script == null) {
            // Wenn keine DatabaseScriptDescription ankommt, hat die Vorroutine entschieden, dass alles ok ist
            var vars = CreateVariableCollection(row, true, dbVariables, true, false);
            return new ScriptEndedFeedback(vars, string.Empty);
        }

        if (!ignoreError && !script.IsOk()) { return new ScriptEndedFeedback("Das Skript ist fehlerhaft.", false, true, name); }

        if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, name); }
        if (!script.NeedRow) { row = null; }

        if (!ignoreError && row != null && RowCollection.FailedRows.ContainsKey(row) && RowCollection.FailedRows.TryGetValue(row, out var reason)) {
            return new ScriptEndedFeedback($"Das Skript konnte die Zeile nicht durchrechnen: {reason}", false, false, name);
        }

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

            var vars = CreateVariableCollection(row, !script.ChangeValuesAllowed, dbVariables, script.VirtalColumns, extended);

            var meth = Method.GetMethods(script.AllowedMethods(row, extended));

            if (script.VirtalColumns) { meth.Add(Method_SetError.Method); }

            #region Script ausf�hren

            var ki = Caption;

            if (row is { IsDisposed: false }) { ki = ki + "\\" + row.CellFirstString(); }

            var scp = new ScriptProperties(script.KeyName, meth, produktivphase, script.Attributes(), addinfo, script.KeyName, ki);

            var sc = new Script(vars, scp) {
                ScriptText = script.Script
            };

            var scf = sc.Parse(0, script.KeyName, attributes);

            #endregion

            #region Fehlerpr�fungen

            if (scf.NeedsScriptFix && string.IsNullOrEmpty(script.FailedReason)) {
                var t = "Datenbank: " + Caption + "\r\n" +
                                  "Benutzer: " + UserName + "\r\n" +
                                  "Zeit (UTC): " + DateTime.UtcNow.ToString5() + "\r\n" +
                                  "Extended: " + extended + "\r\n";

                if (row is { IsDisposed: false } r) {
                    t = t + "Zeile: " + r.CellFirstString() + "\r\n";
                    if (Column.ChunkValueColumn is { IsDisposed: false } spc) {
                        t = t + "Chunk-Wert: " + r.CellGetString(spc) + "\r\n";
                    }
                }

                if (produktivphase && !ignoreError) {
                    t = t + "\r\n\r\n\r\n" + scf.ProtocolText;
                }

                UpdateScript(script, failedReason: t);
            }

            if (scf.Failed) {
                if (row != null) { _ = RowCollection.FailedRows.TryAdd(row, scf.FailedReason); }

                DropMessage(ErrorType.Info, $"Skript-Fehler: {scf.FailedReason}");
                _ = ExecutingScript.Remove(scriptId);
                _ = ExecutingScriptAnyDatabase.Remove(scriptId);
                return scf;
            }

            if (row != null) {
                if (row.IsDisposed) {
                    _ = ExecutingScript.Remove(scriptId);
                    _ = ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Die gepr�fte Zeile wurde verworden", false, false, script.KeyName);
                }

                if (Column.SysRowChangeDate is null) {
                    _ = ExecutingScript.Remove(scriptId);
                    _ = ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Zeilen k�nnen nur gepr�ft werden, wenn �nderungen der Zeile geloggt werden.", false, false, script.KeyName);
                }

                if (row.RowStamp() != rowstamp) {
                    _ = ExecutingScript.Remove(scriptId);
                    _ = ExecutingScriptAnyDatabase.Remove(scriptId);
                    return new ScriptEndedFeedback("Zeile wurde w�hrend des Skriptes ver�ndert.", false, false, script.KeyName);
                }
            }

            if (!produktivphase) {
                _ = ExecutingScript.Remove(scriptId);
                _ = ExecutingScriptAnyDatabase.Remove(scriptId);
                return scf;
            }

            #endregion

            #region Variablen zur�ckschreiben

            if (script.ChangeValuesAllowed && produktivphase) {
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
                if (row is { IsDisposed: false } ro) {
                    foreach (var thisCol in Column) {
                        if (!thisCol.SaveContent) {
                            ro.VariableToCell(thisCol, vars, script.KeyName);
                        }
                    }
                }
            }

            #endregion

            _ = ExecutingScript.Remove(scriptId);
            _ = ExecutingScriptAnyDatabase.Remove(scriptId);

            if (produktivphase && ExecutingScript.Count == 0 && ExecutingScriptAnyDatabase.Count == 0) {
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(row, extended, null);
            }

            return scf;
        } catch {
            Develop.CheckStackOverflow();
            _ = ExecutingScript.Remove(scriptId);
            _ = ExecutingScriptAnyDatabase.Remove(scriptId);
            return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended, ignoreError);
        }
    }

    //public void CloneFrom(Database sourceDatabase, bool cellDataToo, bool tagsToo) {
    //    // Used: Only BZL
    //    _ = sourceDatabase.Save();
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
                Develop.DebugPrint(ErrorType.Error, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrWhiteSpace(scriptname)) {
                return new ScriptEndedFeedback("Weder Eventname noch Skriptname angekommen", false, false, "Allgemein");
            }

            if (string.IsNullOrWhiteSpace(scriptname) && eventname is { } ev) {
                if (!IsScriptsExecutable(ev, true)) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

                DropMessage(ErrorType.DevelopInfo, $"Ereignis ausgel�st: {eventname}");

                var l = EventScript.Get(ev);

                return l.Count == 1
                    ? ExecuteScript(l[0], produktivphase, row, attributes, dbVariables, extended, false)
                    : ExecuteScript(null, produktivphase, row, attributes, dbVariables, extended, false);
            }

            var script = EventScript.Get(scriptname);
            if (script == null) {
                return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname);
            } else {
                if (!script.IsOk()) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }
                return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended, false);
            }
        } catch {
            Develop.CheckStackOverflow();
            return ExecuteScript(eventname, scriptname, produktivphase, row, attributes, dbVariables, extended);
        }
    }

    public string Export_CSV(FirstRow firstRow, IEnumerable<ColumnItem>? columnList, IEnumerable<RowItem> sortedRows) {
        BeSureAllDataLoaded(-1);

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
        if (FileExists(AdditionalFilesPathWhole() + _standardFormulaFile)) { return AdditionalFilesPathWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    /// <summary>
    /// Friert die Datenbank komplett ein, nur noch Ansicht m�glich.
    /// Setzt auch ReadOnly.
    /// </summary>
    /// <param name="reason"></param>
    public virtual void Freeze(string reason) {
        SetReadOnly();
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }

        if (string.IsNullOrEmpty(FreezedReason)) {
            DropMessage(ErrorType.DevelopInfo, $"Datenbank {TableName} wird eingefohren: {reason}");
        }

        FreezedReason = reason;
    }

    public List<string> GetAllLayoutsFileNames() {
        List<string> path = [];
        var r = new List<string>();
        if (!IsDisposed) {
            path.Add(DefaultLayoutPath());
            if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { path.Add(AdditionalFilesPathWhole()); }
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

    public virtual string GrantWriteAccess(DatabaseDataType type, string? chunkValue) {
        var f = CanWriteMainFile();
        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        return string.Empty;
    }

    public string ImportBdb(List<string> files, ColumnItem? colForFilename, bool deleteImportet) {
        foreach (var thisFile in files) {
            var db = Get(thisFile, true, null);
            if (db == null) {
                return thisFile + " konnte nicht geladen werden.";
            }

            foreach (var thisr in db.Row) {
                var r = Row.GenerateAndAdd("Dummy", "BDB Import");

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
                if (!d) { return "L�sch-Fehler!"; }
            }
        }

        return string.Empty;
    }

    public string ImportCsv(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        var f = CanWriteMainFile();

        if (!string.IsNullOrEmpty(f)) {
            DropMessage(ErrorType.Warning, "Abbruch, " + f);
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
            DropMessage(ErrorType.Warning, "Abbruch, keine Zeilen zum Importieren erkannt.");
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
                    DropMessage(ErrorType.Warning, "Abbruch, leerer Spaltenname.");
                    return "Abbruch,<br>leerer Spaltenname.";
                }
                zeil[0][spaltNo] = ColumnItem.MakeValidColumnName(zeil[0][spaltNo]);

                var col = Column[zeil[0][spaltNo]];
                if (col == null) {
                    if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                        DropMessage(ErrorType.Warning, "Abbruch, ung�ltiger Spaltenname.");
                        return "Abbruch,<br>ung�ltiger Spaltenname.";
                    }

                    col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                    if (col != null) {
                        col.Caption = zeil[0][spaltNo];
                    }
                }

                if (col == null) {
                    DropMessage(ErrorType.Warning, "Abbruch, Spaltenfehler.");
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
                    newc.MultiLine = true;
                }

                if (newc == null) {
                    DropMessage(ErrorType.Warning, "Abbruch, Spaltenfehler.");
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
                    DropMessage(ErrorType.Warning, "Abbruch, vorhandene Zeilen der Datenbank '" + Caption + "' sind nicht eindeutig.");
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
                DropMessage(ErrorType.Warning, "Abbruch, Leere Zeile im Import.");
                return "Abbruch, Leere Zeile im Import.";
            }

            #endregion

            #region Row zum schreiben ermitteln (row)

            RowItem? row;

            if (dictVorhanden.ContainsKey(thisD.Key)) {
                _ = dictVorhanden.TryGetValue(thisD.Key, out row);
                _ = dictVorhanden.Remove(thisD.Key); // Speedup
            } else {
                neuZ++;
                row = Row.GenerateAndAdd(thisD.Value[0], "Import, fehlende Zeile");
            }

            if (row == null) {
                DropMessage(ErrorType.Warning, "Abbruch, Import-Fehler.");
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
                DropMessage(ErrorType.Info, "Import: Zwischenspeichern der Datenbank");
                _ = Save();
                d1 = DateTime.Now;
            }

            if (DateTime.Now.Subtract(d2).TotalSeconds > 4.5) {
                DropMessage(ErrorType.Info, "Import: Zeile " + no + " von " + zeil.Count);
                d2 = DateTime.Now;
            }
            Develop.SetUserDidSomething();

            #endregion
        }

        #endregion

        _ = Save();
        DropMessage(ErrorType.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains(Everybody, false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    public string IsNowEditable() => GrantWriteAccess(DatabaseDataType.Caption, DatabaseChunk.Chunk_Master);

    public bool IsRowScriptPossible() {
        if (Column.SysRowChangeDate == null) { return false; }
        if (Column.SysRowState == null) { return false; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return false; } // Nicht ReadOnly!
        //if (checkMessageTo && !AreScriptsExecutable()) { return false; }
        return true;
    }

    public bool IsScriptsExecutable(ScriptEventTypes type, bool returnValueCount0) {
        var l = _eventScript.Get(type);

        if (l.Count > 1) { return false; }

        if (l.Count == 0) { return returnValueCount0; }

        return l[0].IsOk();
    }

    public void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword, string freeze, bool ronly) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(ErrorType.Error, "Geladene Dateien k�nnen nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(ErrorType.Error, "Dateiname nicht angegeben!"); }

        if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }

        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                if (ReadOnly) {
                    Develop.DebugPrint(ErrorType.Error, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                    return;
                }
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Freeze("Datei existiert nicht");
                DropMessage(ErrorType.Warning, $"Datenbank nicht im Dateisystem vorhanden {fileNameToLoad.FileNameWithSuffix()}");
                return;
            }
        }

        _needPassword = needPassword;
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        OnLoading();

        LoadMainData();

        if (FileStateUtcDate.Year < 2000) {
            FileStateUtcDate = new DateTime(2000, 1, 1);
        }
        _saveRequired = false;
        IsInCache = FileStateUtcDate;

        _ = BeSureToBeUpToDate();

        RepairAfterParse();

        if (ronly) { SetReadOnly(); }
        if (!string.IsNullOrEmpty(freeze)) { Freeze(freeze); }
        OnLoaded(true);

        if (!string.IsNullOrEmpty(FreezedReason)) { return; }

        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null, true, false);

        DropMessage(ErrorType.Info, $"Laden der Datenbank {fileNameToLoad.FileNameWithoutSuffix()} abgeschlossen");
    }

    public void LoadFromStream(Stream stream) {
        //OnLoading();
        byte[] bLoaded;
        using (BinaryReader r = new(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt() ?? bLoaded; }

        OnLoading();
        _ = Parse(bLoaded, true, "Stream");
        RepairAfterParse();
        Freeze("Stream-Datenbank");
        _saveRequired = false;
        OnLoaded(true);
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

            if (thisColumn.RelationType == RelationType.None) {
                var x = thisColumn.Contents();
                if (x.Count == 0) {
                    _ = Column.Remove(thisColumn, "Automatische Optimierung");
                    Optimize();
                    return;
                }
            }
        }

        Column.GetSystems();

        if (Column.SysChapter is { IsDisposed: false } c) {
            var x = c.Contents();
            if (x.Count < 2) {
                _ = Column.Remove(c, "Automatische Optimierung");
            }
        }
    }

    public bool Parse(byte[] data, bool isMain, string filename) {
        var pointer = 0;
        var columnUsed = new List<ColumnItem>();

        if (isMain) {
            Undo.Clear();
            Row.RemoveNullOrEmpty();
            Cell.Clear();
        }

        try {
            ColumnItem? column = null;
            RowItem? row = null;
            do {
                if (pointer >= data.Length) {
                    break;
                }

                var (i, command, value, columname, rowKey) = Parse(data, pointer, filename);
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
                            Develop.DebugPrint(ErrorType.Error, "Zeile hinzuf�gen Fehler");
                            Freeze("Zeile hinzuf�gen Fehler");
                            return false;
                        }
                    }

                    #endregion

                    #region Spalte suchen oder erstellen

                    if (!string.IsNullOrEmpty(columname)) {
                        column = Column[columname];
                        if (command == DatabaseDataType.ColumnName) {
                            if (column is not { IsDisposed: false }) {
                                _ = Column.ExecuteCommand(DatabaseDataType.Command_AddColumnByName, columname, Reason.NoUndo_NoInvalidate);
                                column = Column[columname];
                                if (column is not { IsDisposed: false }) {
                                    Develop.DebugPrint(ErrorType.Error, "Spalte hinzuf�gen Fehler");
                                    Freeze("Spalte hinzuf�gen Fehler");
                                    return false;
                                }
                            }
                            columnUsed.Add(column);
                        }
                    }

                    #endregion

                    #region Bei verschl�sselten Datenbanken das Passwort abfragen

                    if (command == DatabaseDataType.GlobalShowPass && !string.IsNullOrEmpty(value)) {
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

                    if (command == DatabaseDataType.EOF) {
                        break;
                    }

                    var error = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, Reason.NoUndo_NoInvalidate);
                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Datenbank-Ladefehler");
                        Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + TableName + "<br>Meldung: " + error);
                        return false;
                    }
                }
            } while (true);
        } catch {
            Freeze("Parse Fehler!");
        }

        if (isMain) {

            #region unbenutzte (gel�schte) Spalten entfernen

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

        _ = Undo.OrderBy(k => k.DateTimeUtc);
        //Undo.OrderByDescending(k => k.DateTimeUtc)

        return true;
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
            Develop.DebugPrint(ErrorType.Warning, "Fehler beim Rechte-Check", ex);
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

    public virtual void ReorganizeChunks() { }

    public virtual void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten DatabaseFragments nicht mehr funktioniert

        if (!string.IsNullOrEmpty(CanWriteMainFile())) { return; }

        Column.Repair();

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(TableName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(ErrorType.Warning, "Tabellenname stimmt nicht: " + Filename);
            }
        }

        Row.Repair();

        SortDefinition?.Repair();

        PermissionGroupsNewRow = RepairUserGroups(PermissionGroupsNewRow).AsReadOnly();
        DatenbankAdmin = RepairUserGroups(DatenbankAdmin).AsReadOnly();

        OnAdditionalRepair();
    }

    public virtual bool Save() {
        if (string.IsNullOrEmpty(Filename)) { return true; }

        // Sofortiger Exit wenn bereits ein Save l�uft (non-blocking check)
        if (!_saveSemaphore.Wait(0)) {
            return false;
        }

        try {
            if (!SaveRequired()) { return true; }

            var result = SaveInternal(FileStateUtcDate);
            OnInvalidateView();
            return string.IsNullOrEmpty(result);
        } finally {
            _saveSemaphore.Release();
        }
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(ErrorType.Error, "Dateiname unterscheiden sich nicht!"); }
        _ = Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;

        var chunks = DatabaseChunk.GenerateNewChunks(this, 100, FileStateUtcDate, false);

        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return; }

        _ = mainchunk.Save(newFileName);
    }

    public override string ToString() => IsDisposed ? string.Empty : base.ToString() + " " + TableName;

    /// <summary>
    /// Diese Routine darf nur aufgerufen werden, wenn die Daten der Datenbank von der Festplatte eingelesen wurden.
    /// </summary>
    public void TryToSetMeTemporaryMaster() {
        if (DateTime.UtcNow.Subtract(IsInCache).TotalMinutes > 1) { return; }

        if (AmITemporaryMaster(0, 60)) { return; }

        if (!NewMasterPossible()) { return; }

        var code = MyMasterCode;

        if (!string.IsNullOrEmpty(GrantWriteAccess(DatabaseDataType.TemporaryDatabaseMasterUser, string.Empty))) {
            return;
        }

        RowCollection.WaitDelay = 0;
        TemporaryDatabaseMasterUser = code;
        TemporaryDatabaseMasterTimeUtc = DateTime.UtcNow.ToString5();
    }

    public bool UpdateScript(string keyName, string? newkeyname, string? script = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null) {
        var existingScript = EventScript.Get(keyName);
        if (existingScript == null) { return false; }

        return UpdateScript(existingScript, newkeyname, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason);
    }

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + TableName;
        } catch { }
        Develop.DebugPrint(ErrorType.Warning, t);
    }

    internal virtual string IsValueEditable(DatabaseDataType type, string? chunkValue) => CanWriteMainFile();

    //public void Variables_RemoveAll(bool isLoading) {
    //    //while (_variables.Count > 0) {
    //    //    //var va = _variables[_eventScript.Count - 1];
    //    //    //ev.Changed -= EventScript_PropertyChanged;
    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
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
    /// <param name="datetimeutc"></param>
    /// <param name="comment"></param>
    protected void AddUndo(DatabaseDataType type, string column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container, string chunkValue) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == DatabaseDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(TableName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container, chunkValue));
    }

    protected void CreateWatcher() {
        if (string.IsNullOrEmpty(CanWriteMainFile())) {
            _checker?.Dispose();

            _checker = new Timer(Checker_Tick);
            _ = _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            try {
                _saveSemaphore?.Dispose();
                OnDisposingEvent();
                UnregisterEvents();

                // Timer zuerst disposen
                if (_checker != null) {
                    _checker.Dispose();
                    _checker = null;
                }

                // L�SUNG: Static Timer verwalten basierend auf aktiven Database-Instanzen
                lock (_timerLock) {
                    _activeDatabaseCount--;
                    if (_activeDatabaseCount <= 0) {
                        _activeDatabaseCount = 0;
                        if (_databaseUpdateTimer != null) {
                            _databaseUpdateTimer.Dispose();
                            _databaseUpdateTimer = null;
                        }
                    }
                }

                // Dann Collections disposen
                Column.Dispose();
                Row.Dispose();

                // Listen leeren

                Undo.Clear();
                _eventScript = new ReadOnlyCollection<DatabaseScriptDescription>([]);
                //_eventScriptEdited = new ReadOnlyCollection<DatabaseScriptDescription>([]);
                _datenbankAdmin.Clear();
                _permissionGroupsNewRow.Clear();
                _tags.Clear();

                // Aus statischer Liste entfernen
                _ = AllFiles.Remove(this);
            } catch (Exception ex) {
                Develop.DebugPrint(ErrorType.Error, "Fehler beim Dispose: " + ex.Message);
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
    /// <param name="starttimeUtc">Nur um die Zeit stoppen zu k�nnen und lange Prozesse zu k�rzen</param>
    protected virtual void DoWorkAfterLastChanges(List<string>? files, DateTime starttimeUtc, DateTime endTimeUtc) { }

    protected void DropMessage(ErrorType type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        Develop.Message?.Invoke(type, this, Caption, ImageCode.Datenbank, message, 0);
    }

    protected virtual bool LoadMainData() {
        var (bytes, _, failed) = LoadBytesFromDisk(Filename, true);

        if (failed) {
            Freeze("Laden fehlgeschlagen!");
            return false;
        }

        var ok = Parse(bytes, true, Filename);

        if (!ok) {
            Freeze("Parsen fehlgeschlagen!");
            return false;
        }

        return true;
    }

    protected void OnAdditionalRepair() {
        if (IsDisposed) { return; }
        //IsInCache = FileStateUTCDate;
        AdditionalRepair?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoaded(bool isFirst) {
        if (IsDisposed) { return; }
        //IsInCache = FileStateUTCDate;
        Loaded?.Invoke(this, new FirstEventArgs(isFirst));
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    protected virtual string SaveInternal(DateTime setfileStateUtcDateTo) {
        var f = CanSaveMainChunk();
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();

        var chunksnew = DatabaseChunk.GenerateNewChunks(this, 1200, setfileStateUtcDateTo, false);
        if (chunksnew == null || chunksnew.Count != 1) { return "Fehler bei der Chunk Erzeugung"; }

        f = chunksnew[0].DoExtendedSave();
        if (!string.IsNullOrEmpty(f)) { return f; }

        _saveRequired = false;
        FileStateUtcDate = setfileStateUtcDateTo;
        return string.Empty;
    }

    protected virtual bool SaveRequired() => _saveRequired;

    protected void SetReadOnly() => ReadOnly = true;

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und f�hrt Commands aus.
    /// Es wird WriteValueToDiscOrServer aufgerufen - echtzeitbasierte Systeme k�nnen dort den Wert speichern
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="datetimeutc"></param>
    /// <param name="reason"></param>
    /// <param name="user"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    protected string SetValueInternal(DatabaseDataType type, ColumnItem? column, RowItem? row, string value, string user, DateTime datetimeutc, Reason reason) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if ((reason != Reason.NoUndo_NoInvalidate) && !string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren: " + FreezedReason; }
        if (type.IsObsolete()) { return string.Empty; }

        LastChange = DateTime.UtcNow;

        //if (!string.IsNullOrEmpty(chunk)) {
        //    LoadChunkWithChunkId(chunk, true, null);
        //}
        if (type.IsCellValue()) {
            if (column?.Database is not { IsDisposed: false } db) { return string.Empty; }
            if (row == null) { return string.Empty; }
            if (!column.SaveContent) { return string.Empty; }

            //column.Invalidate_ContentWidth();
            //row.InvalidateCheckData();

            var f = row.SetValueInternal(column, value, reason);

            if (!string.IsNullOrEmpty(f)) { return f; }

            if (column.SaveContent) {
                row.DoSystemColumns(db, column, user, datetimeutc, reason);
            }
            return string.Empty;
        }

        if (type.IsColumnTag()) {
            if (column is not { IsDisposed: false } || Column.IsDisposed) {
                //Develop.DebugPrint(ErrorType.Warning, "Spalte ist null! " + type);
                //return ("Wert nicht gesetzt!", null, null);
                DropMessage(ErrorType.Info, $"Wert nicht gesetzt, Spalte nicht vorhanden");
                return string.Empty;
            }

            return column.SetValueInternal(type, value);
        }

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Command_RemoveColumn:
                    var c = Column[value];
                    if (c == null) { return string.Empty; }
                    return Column.ExecuteCommand(type, c.KeyName, reason);

                case DatabaseDataType.Command_AddColumnByName:
                    var f2 = Column.ExecuteCommand(type, value, reason);
                    if (!string.IsNullOrEmpty(f2)) { return f2; }

                    var thisColumn = Column[value];
                    if (thisColumn == null) { return "Hinzuf�gen fehlgeschlagen"; }

                    return string.Empty;

                case DatabaseDataType.Command_RemoveRow:
                    var r = Row.SearchByKey(value);
                    if (r == null) { return string.Empty; }
                    return Row.ExecuteCommand(type, r.KeyName, reason, user, datetimeutc);

                case DatabaseDataType.Command_AddRow:
                    var f1 = Row.ExecuteCommand(type, value, reason, user, datetimeutc);
                    if (!string.IsNullOrEmpty(f1)) { return f1; }
                    //var thisRow = Row.SearchByKey(value);
                    return string.Empty;

                case DatabaseDataType.Command_NewStart:
                    return string.Empty;

                default:
                    if (LoadedVersion == DatabaseVersion) {
                        Freeze("Ladefehler der Datenbank");
                        if (!ReadOnly) {
                            Develop.DebugPrint(ErrorType.Error, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + TableName);
                        }
                    }
                    return "Befehl unbekannt.";
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

            case DatabaseDataType.LastEditID:
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
                _additionalFilesPath = value;
                break;

            case DatabaseDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case DatabaseDataType.RowQuickInfo:
                _rowQuickInfo = value;
                break;

            case DatabaseDataType.Tags:
                _tags.SplitAndCutByCr(value);
                break;

            case DatabaseDataType.EventScript:
                List<string> ves = [.. value.SplitAndCutByCr()];
                var vess = new List<DatabaseScriptDescription>();
                foreach (var t in ves) {
                    vess.Add(new DatabaseScriptDescription(this, t));
                }
                Row.InvalidateAllCheckData();
                _eventScript = vess.AsReadOnly();
                break;

            //case (DatabaseDataType)79: //.EventScriptEdited:

            //    if (value.Length > 50) {
            //        List<string> vese = [.. value.SplitAndCutByCr()];
            //        var veses = new List<DatabaseScriptDescription>();
            //        foreach (var t in vese) {
            //            veses.Add(new DatabaseScriptDescription(this, t));
            //        }
            //        _eventScript = veses.AsReadOnly();
            //    }

            //    break;

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

            case DatabaseDataType.UndoInOne:
                Undo.Clear();
                var uio = value.SplitAndCutByCr();
                for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                    UndoItem tmpWork = new(uio[z]);
                    Undo.Add(tmpWork);
                }
                break;

            case DatabaseDataType.Undo:
                Undo.Add(new(value));
                break;

            case DatabaseDataType.EOF:
                return string.Empty;

            default:
                // Variable type
                if (LoadedVersion == DatabaseVersion) {
                    Freeze("Ladefehler der Datenbank");
                    if (!ReadOnly) {
                        Develop.DebugPrint(ErrorType.Error, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + TableName);
                    }
                }

                return "Datentyp unbekannt.";
        }
        return string.Empty;
    }

    protected virtual string WriteValueToDiscOrServer(DatabaseDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string oldChunkId, string newChunkId, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (!string.IsNullOrEmpty(FreezedReason)) { return "Datenbank eingefroren!"; } // Sicherheitshalber!
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }

        return string.Empty;
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

    private static void DatabaseUpdater(object state) {
        if (CriticalState()) { return; }
        BeSureToBeUpToDate(AllFiles);
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

    private List<string>? AllAvailableTables(List<Database>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        var path = Filename.FilePath();
        var fx = Filename.FileSuffix();

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is { IsDisposed: false } db) {
                    if (string.Equals(db.Filename.FilePath(), path) &&
                        string.Equals(db.Filename.FileSuffix(), fx)) { return null; }
                }
            }
        }

        return Directory.GetFiles(path, "*." + fx, SearchOption.TopDirectoryOnly).ToList();
    }

    private void Checker_Tick(object state) {
        // Grundlegende �berpr�fungen
        if (IsDisposed || !string.IsNullOrEmpty(FreezedReason)) { return; }

        // Script-�berpr�fung
        var e = new CancelReasonEventArgs();
        OnCanDoScript(e);
        if (!e.Cancel) { RowCollection.ExecuteValueChangedEvent(); }

        if (!SaveRequired() || !LogUndo) {
            _checkerTickCount = 0;
            return;
        }

        _checkerTickCount++;
        _checkerTickCount = Math.Min(_checkerTickCount, 5000);

        // Zeitliche Bedingungen pr�fen
        //var timeSinceLastChange = DateTime.UtcNow.Subtract(LastChange).TotalSeconds;
        var timeSinceLastAction = DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds;

        // Bestimme ob gespeichert werden muss
        var mustSave = (_checkerTickCount > 20 && timeSinceLastAction > 20) ||
                         _checkerTickCount > 110 ||
                         (Column.ChunkValueColumn != null && _checkerTickCount > 50);

        // Speichern wenn n�tig
        if (mustSave && Save()) {
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
    /// AdditionalFiles/Datenbankpfad mit Backup und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Forms und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    private string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { return AdditionalFilesPathWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    private void GenerateDatabaseUpdateTimer() {
        lock (_timerLock) {
            _activeDatabaseCount++;

            if (_databaseUpdateTimer != null) { return; }

            _databaseUpdateTimer = new Timer(DatabaseUpdater, null, 10000, 3 * 60 * 1000);
        }
    }

    private bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is { IsDisposed: false } db) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    _ = thisFile.Save();
                    Develop.DebugPrint(ErrorType.Warning, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    private bool NewMasterPossible() {
        if (ReadOnly) { return false; }
        if (!IsAdministrator()) { return false; }

        if (DateTimeTryParse(TemporaryDatabaseMasterTimeUtc, out var dt)) {
            if (DateTime.UtcNow.Subtract(dt).TotalMinutes < 60) { return false; }
            if (DateTime.UtcNow.Subtract(dt).TotalDays > 1) { return true; }
        }

        if (RowCollection.WaitDelay > 90) { return true; }

        if (MasterNeeded) { return true; }

        try {
            var masters = 0;
            foreach (var thisDb in AllFiles) {
                if (MultiUserPossible && !thisDb.IsDisposed && thisDb.AmITemporaryMaster(0, 45)) {
                    masters++;
                    if (masters >= MaxMasterCount) { return false; }
                }
            }
        } catch {
            Develop.CheckStackOverflow();
            return NewMasterPossible();
        }

        return true;
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            // Es werden alle Datenbanken abgefragt, also kann nach der ersten nicht schluss sein...

            if (string.IsNullOrWhiteSpace(AdditionalFilesPathWhole())) { return; }

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
            InvalidateView = null;
            Loaded = null;
            Loading = null;
            ProgressbarInfo = null;
            SortParameterChanged = null;
            ViewChanged = null;
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, "Fehler beim Abmelden der Events: " + ex.Message);
        }
    }

    private void UpdateColumnArrangementsAfterRename(string oldColumnName, string newColumnName) {
        if (string.IsNullOrEmpty(_columnArrangements)) { return; }

        var oldPattern = $"ColumnName={oldColumnName}".ToNonCritical();
        var newPattern = $"ColumnName={newColumnName}".ToNonCritical();

        var updatedArrangements = _columnArrangements.ReplaceWord(oldPattern, newPattern, RegexOptions.IgnoreCase);

        if (updatedArrangements != _columnArrangements) {
            _columnArrangements = updatedArrangements;
            // Speichern ohne ChangeData aufzurufen (w�rde Endlosschleife verursachen)
            _ = WriteValueToDiscOrServer(DatabaseDataType.ColumnArrangement, _columnArrangements, string.Empty, null, UserName, DateTime.UtcNow, string.Empty, string.Empty, "Automatische Aktualisierung nach Spaltenumbenennung");
        }
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Initallladung ausgef�hrt wurde
    /// </summary>
    private void WaitInitialDone() {
        var t = Stopwatch.StartNew();

        var lastMessageTime = 0L;

        while (IsInCache.Year < 2000) {
            Thread.Sleep(1);
            if (t.ElapsedMilliseconds > 120 * 1000) {
                DropMessage(ErrorType.DevelopInfo, $"Abbruch, Datenbank {Filename.FileNameWithSuffix()} wurde nicht richtig initialisiert");
                return;
            }

            if (!string.IsNullOrEmpty(FreezedReason)) {
                DropMessage(ErrorType.DevelopInfo, $"Abbruch, Datenbank {Filename.FileNameWithSuffix()} eingefrohren {FreezedReason}");
                return;
            }

            if (t.ElapsedMilliseconds - lastMessageTime >= 5000) {
                lastMessageTime = t.ElapsedMilliseconds;
                DropMessage(ErrorType.DevelopInfo, $"Warte auf Abschluss der Initialsierung von {TableName}\\{Filename.FileNameWithSuffix()}");
            }
        }
    }

    #endregion
}