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
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueScript.Script;
using Timer = System.Threading.Timer;

namespace BlueTable;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Table : IDisposableExtendedWithEvent, IHasKeyName, IEditable {

    #region Fields

    public const string TableVersion = "4.10";
    public static readonly ObservableCollection<Table> AllFiles = [];

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich
    /// </summary>
    public static readonly int MasterBlockedMax = 180;

    /// <summary>
    /// Wert in Minuten. Ist jemand Master in diesen Range, ist kein Master der Tabelle setzen möglich
    /// </summary>
    public static readonly int MasterBlockedMin = 0;

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

    private readonly List<string> _permissionGroupsNewRow = [];

    private readonly List<string> _tableAdmin = [];

    private readonly List<string> _tags = [];

    private readonly List<Variable> _variables = [];

    private string _additionalFilesPath;

    private string _caption = string.Empty;

    private Timer? _checker;

    private string _columnArrangements = string.Empty;

    private string _createDate;

    private string _creator;

    private ReadOnlyCollection<TableScriptDescription> _eventScript = new([]);

    private DateTime _eventScriptVersion = DateTime.MinValue;

    private string _globalShowPass = string.Empty;

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

    private string _variableTmp;

    #endregion

    #region Constructors

    protected Table(string tablename) {
        // Keine Konstruktoren mit Dateiname, Filestreams oder sonst was.
        // Weil das OnLoaded-Ereigniss nicht richtig ausgelöst wird.
        Develop.StartService();

        QuickImage.NeedImage += QuickImage_NeedImage;

        KeyName = MakeValidTableName(tablename);

        if (!IsValidTableName(KeyName)) {
            Develop.DebugPrint(ErrorType.Error, "Tabellenname ungültig: " + tablename);
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
        //_tableAdmin.Clear();
        //_globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.UtcNow.ToString9();
        LastSaveMainFileUtcDate = new DateTime(0);
        //_caption = string.Empty;
        LoadedVersion = TableVersion;
        //_globalScale = 1f;
        _additionalFilesPath = "AdditionalFiles";
        //_rowQuickInfo = string.Empty;
        //_sortDefinition = null;
        //EventScript_RemoveAll(true);
        //_variables.Clear();
        _variableTmp = string.Empty;
        //Undo.Clear();

        // Muss vor dem Laden der Datan zu Allfiles hinzugfügt werde, weil das bei OnAdded
        // Die Events registriert werden, um z.B: das Passwort abzufragen
        // Zusätzlich werden z.B: Filter für den Export erstellt - auch der muss die Tabelle finden können.
        // Zusätzlich muss der Tablename stimme, dass in Added diesen verwerten kann.
        AllFiles.Add(this);
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

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

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
    public string AdditionalFilesPath {
        get => _additionalFilesPath;
        set {
            if (_additionalFilesPath == value) { return; }
            _additionalFilesPathTemp = null;
            ChangeData(TableDataType.AdditionalFilesPath, null, _additionalFilesPath, value);
            Cell.InvalidateAllSizes();
        }
    }

    public string CachePfad { get; set; } = string.Empty;

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

    public ColumnCollection Column { get; }

    public string ColumnArrangements {
        get => _columnArrangements;
        set {
            if (_columnArrangements == value) { return; }
            ChangeData(TableDataType.ColumnArrangement, null, _columnArrangements, value);
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

    public Type? Editor { get; set; }

    public ReadOnlyCollection<TableScriptDescription> EventScript {
        get => _eventScript;
        set {
            var l = new List<TableScriptDescription>();
            l.AddRange(value);
            l.Sort();

            var eventScriptOld = _eventScript.ToString(false);
            var eventScriptNew = l.ToString(false);

            if (eventScriptOld == eventScriptNew) { return; }
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
            ChangeData(TableDataType.PermissionGroupsNewRow, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr());
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
            ChangeData(TableDataType.TableAdminGroups, null, _tableAdmin.JoinWithCr(), value.JoinWithCr());
        }
    }

    public ReadOnlyCollection<string> Tags {
        get => new(_tags);
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            ChangeData(TableDataType.Tags, null, _tags.JoinWithCr(), value.JoinWithCr());
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

    private string? _additionalFilesPathTemp { get; set; }

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

        var allfiles = new List<Table>();// könnte sich ändern, deswegen Zwischenspeichern
        allfiles.AddRange(AllFiles);

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

    public static void BeSureToBeUpToDate(ObservableCollection<Table> ofTables, bool instantUpdate) {
        List<Table> l = [.. ofTables];

        foreach (var tbl in l) {
            tbl.BeSureToBeUpToDate(false, instantUpdate);
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

    public static Table Get() {
        var t = new Table(UniqueKeyValue());
        t.InitDummyTable();
        return t;
    }

    public static Table? Get(string fileOrTableName, NeedPassword? needPassword, bool instantUpdate) {
        try {
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

            #region Schauen, ob die Tabelle bereits geladen ist

            var folder = new List<string>();

            if (fileOrTableName.IsFormat(FormatHolder.FilepathAndName)) {
                folder.AddIfNotExists(fileOrTableName.FilePath());
                fileOrTableName = fileOrTableName.FileNameWithoutSuffix();
            }

            fileOrTableName = MakeValidTableName(fileOrTableName);

            foreach (var thisFile in AllFiles) {
                if (string.Equals(thisFile.KeyName, fileOrTableName, StringComparison.OrdinalIgnoreCase)) {
                    thisFile.WaitInitialDone();
                    return thisFile;
                }

                if (thisFile is TableFile tbf && tbf.Filename.IsFormat(FormatHolder.FilepathAndName)) {
                    folder.AddIfNotExists(tbf.Filename.FilePath());
                }
            }

            #endregion

            foreach (var thisfolder in folder) {
                var f = thisfolder + fileOrTableName;

                if (FileExists(f + ".cbdb")) {
                    var db = new TableChunk(fileOrTableName);
                    db.LoadFromFile(f + ".cbdb", false, needPassword, string.Empty, instantUpdate);
                    db.WaitInitialDone();
                    return db;
                }

                if (FileExists(f + ".mbdb")) {
                    var db = new TableFragments(fileOrTableName);
                    db.LoadFromFile(f + ".mbdb", false, needPassword, string.Empty, instantUpdate);
                    db.WaitInitialDone();
                    return db;
                }

                if (FileExists(f + ".bdb")) {
                    var db = new TableFile(fileOrTableName);
                    db.LoadFromFile(f + ".bdb", false, needPassword, string.Empty, instantUpdate);
                    db.WaitInitialDone();
                    return db;
                }
            }

            return null;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return Get(fileOrTableName, needPassword, instantUpdate);
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
        ProcessFile(TryGrantWriteAccess, [], false, waitforSeconds, newChunkValue, column, row, waitforSeconds, onlyTopLevel) as string ?? "Unbekannter Fehler";

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

                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 1:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressources\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 0:                         // BeCreative, At Home, 31.11.2021
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
                    var tb = new TableFile(name);
                    tb.DropMessages = false;
                    tb.LoadFromFile(pf, false, null, string.Empty, false);
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
        if (fehlerAusgeben) { Develop.DebugPrint(ErrorType.Error, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static string MakeValidTableName(string tablename) {
        var tmp = tablename.RemoveChars(Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab
        tmp = tmp.FileNameWithoutSuffix().Replace(" ", "_").Replace("-", "_");
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
            //case Routinen.CellFormat: {
            //        type = (TableDataType)bLoaded[pointer + 1];
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
            //        type = (TableDataType)bLoaded[pointer + 1];
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
            //        type = (TableDataType)bLoaded[pointer + 1];
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
                    type = (TableDataType)bLoaded[pointerIn + 1];
                    var les = NummerCode3(bLoaded, pointerIn + 2);
                    rowKey = NummerCode7(bLoaded, pointerIn + 5).ToString();
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointerIn + 12, b, 0, les);
                    value = b.ToStringUtf8();
                    pointerIn += 12 + les;
                    //colKey = -1;
                    break;
                }

            //case Routinen.DatenAllgemein: {
            //        type = (TableDataType)bLoaded[pointer + 1];
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
                    type = (TableDataType)bLoaded[pointerIn + 1];
                    var les = NummerCode3(bLoaded, pointerIn + 2);
                    rowKey = string.Empty;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointerIn + 5, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointerIn += 5 + les;
                    break;
                }
            //case Routinen.Column: {
            //        type = (TableDataType)bLoaded[pointer + 1];
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
            //        type = (TableDataType)bLoaded[pointer + 1];
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
            //        type = (TableDataType)bLoaded[pointer + 1];
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
                    type = (TableDataType)bLoaded[pointerIn + 1];

                    var cles = NummerCode1(bLoaded, pointerIn + 2);
                    var cb = new byte[cles];
                    Buffer.BlockCopy(bLoaded, pointerIn + 3, cb, 0, cles);
                    colName = cb.ToStringUtf8();

                    var les = NummerCode3(bLoaded, pointerIn + 3 + cles);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointerIn + 6 + cles, b, 0, les);
                    value = b.ToStringUtf8();

                    pointerIn += 6 + les + cles;
                    break;
                }

            case Routinen.CellFormatUTF8_V402: {
                    type = TableDataType.UTF8Value_withoutSizeData;

                    var lengthRowKey = NummerCode1(bLoaded, pointerIn + 1);
                    var rowKeyByte = new byte[lengthRowKey];
                    Buffer.BlockCopy(bLoaded, pointerIn + 2, rowKeyByte, 0, lengthRowKey);
                    rowKey = rowKeyByte.ToStringUtf8();

                    var lengthValue = NummerCode2(bLoaded, pointerIn + 2 + lengthRowKey);
                    var valueByte = new byte[lengthValue];
                    Buffer.BlockCopy(bLoaded, pointerIn + 2 + lengthRowKey + 2, valueByte, 0, lengthValue);
                    value = valueByte.ToStringUtf8();

                    pointerIn += 2 + lengthRowKey + 2 + lengthValue;

                    break;
                }

            case Routinen.CellFormatUTF8_V403: {
                    type = TableDataType.UTF8Value_withoutSizeData;

                    var lengthColumnKey = NummerCode1(bLoaded, pointerIn + 1);
                    var columnKeyByte = new byte[lengthColumnKey];
                    Buffer.BlockCopy(bLoaded, pointerIn + 2, columnKeyByte, 0, lengthColumnKey);
                    colName = columnKeyByte.ToStringUtf8();

                    var lengthRowKey = NummerCode1(bLoaded, pointerIn + 2 + lengthColumnKey);
                    var rowKeyByte = new byte[lengthRowKey];
                    Buffer.BlockCopy(bLoaded, pointerIn + 3 + lengthColumnKey, rowKeyByte, 0, lengthRowKey);
                    rowKey = rowKeyByte.ToStringUtf8();

                    var lengthValue = NummerCode2(bLoaded, pointerIn + 3 + lengthRowKey + lengthColumnKey);
                    var valueByte = new byte[lengthValue];
                    Buffer.BlockCopy(bLoaded, pointerIn + 5 + lengthRowKey + lengthColumnKey, valueByte, 0, lengthValue);
                    value = valueByte.ToStringUtf8();

                    pointerIn += 5 + lengthRowKey + lengthValue + lengthColumnKey;

                    break;
                }

            default: {
                    type = 0;
                    value = string.Empty;
                    //width = 0;
                    //height = 0;
                    Develop.DebugPrint(ErrorType.Error, $"Laderoutine nicht definiert: {bLoaded[pointerIn]}");
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
        if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss

        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            if (thisFile is TableFile tbf) {
                tbf.Save(mustSave);
                if (x != AllFiles.Count) {
                    // Die Auflistung wurde verändert! Selten, aber kann passieren!
                    SaveAll(mustSave);
                    return;
                }
            }
        }
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Table is not { IsDisposed: false } db) { return string.Empty; }

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
                    if (string.Equals(unique, thisfile.KeyName, StringComparison.Ordinal)) { ok = false; break; }
                }
            } else {
                ok = false;
            }

            if (ok) { return unique; }
        } while (true);
    }

    public static bool UpdateScript(TableScriptDescription script, string? keyname = null, string? scriptContent = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, bool isDisposed = false) {
        if (script?.Table is not { IsDisposed: false } tb) { return false; }

        if (!tb.IsEditable(false)) { return false; }

        var found = false;

        List<TableScriptDescription> updatedScripts = [];

        foreach (var existingScript in tb.EventScript) {
            if (ReferenceEquals(existingScript, script) || (existingScript.KeyName == script.KeyName && existingScript.Script == script.Script)) {
                found = true;

                if (!isDisposed) {
                    // Prüfe ob sich wirklich etwas geändert hat
                    var hasChanges = (keyname != null && keyname != existingScript.KeyName) ||
                                    (scriptContent != null && scriptContent != existingScript.Script) ||
                                    (image != null && image != existingScript.Image) ||
                                    (quickInfo != null && quickInfo != existingScript.ColumnQuickInfo) ||
                                    (adminInfo != null && adminInfo != existingScript.AdminInfo) ||
                                    (eventTypes != null && !eventTypes.Equals(existingScript.EventTypes)) ||
                                    (needRow != null && needRow != existingScript.NeedRow) ||
                                    userGroups?.SequenceEqual(existingScript.UserGroups) == false ||
                                    (failedReason != null && failedReason != existingScript.FailedReason);

                    if (hasChanges) {
                        // Erstelle neues Script mit aktualisierten Werten
                        var newScript = new TableScriptDescription(
                            existingScript.Table,
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
            } else {
                updatedScripts.Add(existingScript);
            }
        }

        if (!found) {
            updatedScripts.Add(script);
        }

        tb.EventScript = updatedScripts.AsReadOnly();

        return true;
    }

    public static void WaitScriptsDone() {
        var sw = Stopwatch.StartNew();
        var runTimeID = ExecutingScriptThreadsAnyTable.JoinWithCr();

        var myThread = Thread.CurrentThread.ManagedThreadId.ToStringInt10();

        while (HasActiveThreadsExcept(myThread)) {
            try {
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
                Pause(1, true);
                var newRunTimeID = ExecutingScriptThreadsAnyTable.JoinWithCr();

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

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public string AdditionalFilesPathWhole() {
        if (_additionalFilesPathTemp != null) { return _additionalFilesPathTemp; }

        if (!string.IsNullOrEmpty(_additionalFilesPath)) {
            var t = _additionalFilesPath.NormalizePath();
            if (DirectoryExists(t)) {
                _additionalFilesPathTemp = t;
                return t;
            }
        }

        if (this is TableFile tbf && !string.IsNullOrEmpty(tbf.Filename)) {
            var t = (tbf.Filename.FilePath() + "AdditionalFiles\\").NormalizePath();
            if (DirectoryExists(t)) {
                _additionalFilesPathTemp = t;
                return t;
            }
        }
        _additionalFilesPathTemp = string.Empty;
        return string.Empty;
    }

    public virtual List<string>? AllAvailableTables(List<Table>? allreadychecked) => null;

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
        if (!MultiUserPossible) { return true; }
        if (!IsEditable(false)) { return false; }

        if (TemporaryTableMasterUser != UserName) { return false; }
        if (TemporaryTableMasterMachine != Environment.MachineName) { return false; }

        var d = DateTimeParse(TemporaryTableMasterTimeUtc);
        var mins = DateTime.UtcNow.Subtract(d).TotalMinutes;

        ranges = Math.Max(ranges, 0);
        //rangee = Math.Min(rangee, 55);

        // Info:
        // 5 Minuten, weil alle 3 Minuten SysUndogeprüft wird
        // 55 Minuten, weil alle 60 Minuten der Master wechseln kann
        return mins > ranges && mins < rangee;
    }

    public virtual bool BeSureAllDataLoaded(int anzahl) => BeSureToBeUpToDate(false, true);

    public virtual bool BeSureRowIsLoaded(string chunkValue) => IsEditable(false);

    public virtual bool BeSureToBeUpToDate(bool firstTime, bool instantUpdate) => true;

    public bool CanDoValueChangedScript(bool returnValueCount0) => IsRowScriptPossible() && IsThisScriptBroken(ScriptEventTypes.value_changed, returnValueCount0);

    public string ChangeData(TableDataType command, ColumnItem? column, string previousValue, string changedTo) => ChangeData(command, column, null, previousValue, changedTo, UserName, DateTime.UtcNow, string.Empty, string.Empty, string.Empty);

    //    if (!FileExists(ci.AdditionalData)) { return null; }
    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und kümmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
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
    public string ChangeData(TableDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string user, DateTime datetimeutc, string comment, string oldchunkvalue, string newchunkvalue) {
        if (IsDisposed) { return "Tabelle verworfen!"; }
        if (IsFreezed) { return "Tabelle eingefroren: " + FreezedReason; }
        if (type.IsObsolete()) { return "Obsoleter Befehl angekommen!"; }

        var colName = column?.KeyName ?? string.Empty;

        // ERST Speicher setzen
        var error = SetValueInternal(type, column, row, changedTo, user, datetimeutc, Reason.SetCommand);
        if (!string.IsNullOrEmpty(error)) { return error; }

        // Bei Spaltenumbenennung auch ColumnArrangements aktualisieren
        if (type == TableDataType.ColumnName && column != null) {
            UpdateColumnArrangementsAfterRename(previousValue, changedTo);
        }

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

    public VariableCollection CreateVariableCollection(RowItem? row, bool allReadOnly, bool dbVariables, bool virtualcolumns, bool extendedVariable) {

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

        if (dbVariables) {
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
        //_ = vars.Add(new VariableString("Type", Filename.FileSuffix().ToUpperInvariant(), true, "Der Tabellentyp."));
        vars.Add(new VariableBool("ReadOnly", IsFreezed, true, "Ob die aktuelle Tabelle schreibgeschützt ist."));
        vars.Add(new VariableDouble("Rows", Row.Count, true, "Die Anzahl der Zeilen in der Tabelle")); // RowCount als Befehl belegt
        vars.Add(new VariableString("StartTimeUTC", DateTime.UtcNow.ToString7(), true, "Die Uhrzeit, wann das Skript gestartet wurde.")); // RowCount als Befehl belegt
        //var r = SortDefinition?.SortetdRows(this.Row) ?? new RowSortDefinition(this, this.Column.First, false).SortetdRows(this.Row);
        //_ = vars.Add(new VariableListRow("RowsSorted", r, true, "Alle Zeilen in der aktuellen Tabelle. Sortiert nach der Standard Sortierung."));

        //if (Column.SysCorrect is { IsDisposed: false } csc && row is { IsDisposed: false }) {
        //    vars.Add(new VariableBool("sys_correct", row.CellGetBoolean(csc), true, "Der aktuelle Zeilenstand, ob die Zeile laut Skript Fehler korrekt durchgerechnet worden ist\r\nAchtung: Das ist der eingfrohrende Stand, zu Beginn des Skriptes."));
        //}

        if (Column.First is { IsDisposed: false } fc) {
            vars.Add(new VariableString("NameOfFirstColumn", fc.KeyName, true, "Der Name der ersten Spalte"));

            if (row != null) {
                vars.Add(new VariableString("ValueOfFirstColumn", row.CellGetString(fc), true, "Der Wert der ersten Spalte als String"));
            }
        }

        vars.Add(new VariableString("AdditionalFilesPath", (AdditionalFilesPathWhole().Trim("\\") + "\\").NormalizePath(), true, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));

        //_ = vars.Add(new VariableBool("Successful", true, true, "Marker, ob das Skript erfolgreich abgeschlossen wurde."));
        //_ = vars.Add(new VariableString("NotSuccessfulReason", string.Empty, true, "Die letzte Meldung, warum es nicht erfolgreich war."));
        vars.Add(new VariableBool("Extended", extendedVariable, true, "Marker, ob das Skript erweiterte Befehle und Laufzeiten akzeptiert."));
        vars.Add(new VariableListString("ErrorColumns", [], true, "Spalten, die mit SetError fehlerhaft gesetzt wurden."));

        #endregion

        return vars;
    }

    //    EventScript = sourceTable.EventScript;
    /// <summary>
    /// AdditionalFiles/Tabellepfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { return AdditionalFilesPathWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    //    TabelleAdmin = new(sourceTable.TabelleAdmin.Clone());
    //    PermissionGroupsNewRow = new(sourceTable.PermissionGroupsNewRow.Clone());
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void EnableScript() => Column.GenerateAndAddSystem("SYS_ROWSTATE");

    /// <summary>
    ///
    /// </summary>
    /// <param name="script">Wenn keine TableScriptDescription ankommt, hat die Vorroutine entschieden, dass alles ok ist</param>
    /// <param name="produktivphase"></param>
    /// <param name="row"></param>
    /// <param name="attributes"></param>
    /// <param name="dbVariables"></param>
    /// <param name="extended">True, wenn valueChanged im erweiterten Modus aufgerufen wird</param>
    /// <returns></returns>
    public ScriptEndedFeedback ExecuteScript(TableScriptDescription script, bool produktivphase, RowItem? row, List<string>? attributes, bool dbVariables, bool extended, bool ignoreError) {
        // Vorab-Prüfungen
        var f = ExternalAbortScriptReason(extended);
        if (!string.IsNullOrEmpty(f)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + f, false, false, script.KeyName); }

        if (!IsEditable(false)) { return new ScriptEndedFeedback("Automatische Prozesse aktuell nicht möglich: " + IsNotEditableReason(false), false, false, script.KeyName); }

        //if (!MainChunkLoadDone) { return new ScriptEndedFeedback("Tabelle noch nicht geladen.", false, false, script.KeyName); }

        if (!ignoreError && !script.IsOk()) { return new ScriptEndedFeedback($"Das Skript ist fehlerhaft: {script.ErrorReason()}", false, true, script.KeyName); }

        if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, false, script.KeyName); }
        if (!script.NeedRow) { row = null; }

        if (!ignoreError && row != null && RowCollection.FailedRows.ContainsKey(row) && RowCollection.FailedRows.TryGetValue(row, out var reason)) {
            return new ScriptEndedFeedback($"Das Skript konnte die Zeile nicht durchrechnen: {reason}", false, false, script.KeyName);
        }

        var isNewId = false;
        var scriptThreadId = Thread.CurrentThread.ManagedThreadId.ToStringInt10();
        if (script.ChangeValuesAllowed) {
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

            var vars = CreateVariableCollection(row, !script.ChangeValuesAllowed, dbVariables, script.VirtalColumns, extended);
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
            var scf = sc.Parse(0, script.KeyName, attributes, abr);

            #endregion

            #region Fehlerprüfungen

            if (scf.NeedsScriptFix && string.IsNullOrEmpty(script.FailedReason)) {
                var t = "Tabelle: " + Caption + "\r\n" +
                       "Benutzer: " + UserName + "\r\n" +
                       "Zeit (UTC): " + DateTime.UtcNow.ToString5() + "\r\n" +
                       "Extended: " + extended + "\r\n";

                if (row is { IsDisposed: false } r) {
                    t = t + "Zeile: " + r.CellFirstString() + "\r\n";
                    t = t + "Zeile-Schlüssel: " + r.KeyName + "\r\n";
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
                if (row != null) { RowCollection.FailedRows.TryAdd(row, scf.FailedReason); }

                DropMessage(ErrorType.Info, $"Skript-Fehler: {scf.FailedReason}");
                return scf;
            }

            if (row != null) {
                if (row.IsDisposed) { return new ScriptEndedFeedback("Die geprüfte Zeile wurde verworfen", false, false, script.KeyName); }
                if (Column.SysRowChangeDate is null) { return new ScriptEndedFeedback("Zeilen können nur geprüft werden, wenn Änderungen der Zeile geloggt werden.", false, false, script.KeyName); }
                if (row.RowStamp() != rowstamp) { return new ScriptEndedFeedback("Zeile wurde während des Skriptes verändert.", false, false, script.KeyName); }
            }

            if (!produktivphase) { return scf; }

            #endregion

            WriteBackVariables(row, vars, script.VirtalColumns, dbVariables, script.KeyName, script.ChangeValuesAllowed && produktivphase);

            //  Erfolgreicher Abschluss
            if (isNewId) { ExecutingScriptThreadsAnyTable.Remove(scriptThreadId); }
            if (ExecutingScriptThreadsAnyTable.Count == 0) {
                RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(row, extended, null);
            }

            return scf;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended, ignoreError);
        } finally {
            //  ExecutingScriptAnyTable wird IMMER aufgeräumt - egal was passiert
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

            TableScriptDescription? script = null;
            if (string.IsNullOrWhiteSpace(scriptname) && eventname is { } ev) {
                if (!IsThisScriptBroken(ev, true)) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

                DropMessage(ErrorType.DevelopInfo, $"Ereignis ausgelöst: {eventname}");

                var l = EventScript.Get(ev);

                if (l.Count == 1) {
                    script = l[0];
                } else if (l.Count == 0) {
                    var vars = CreateVariableCollection(row, true, dbVariables, true, false);
                    return new ScriptEndedFeedback(vars, string.Empty);
                }
            } else {
                script = EventScript.GetByKey(scriptname);
            }

            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }
            if (!script.IsOk()) { return new ScriptEndedFeedback("Skript defekt", false, false, "Allgemein"); }

            return ExecuteScript(script, produktivphase, row, attributes, dbVariables, extended, false);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ExecuteScript(eventname, scriptname, produktivphase, row, attributes, dbVariables, extended);
        }
    }

    public string ExternalAbortScriptReason() => ExternalAbortScriptReason(false);

    public string ExternalAbortScriptReasonExtended() => ExternalAbortScriptReason(true);

    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionalFilesPathWhole() + _standardFormulaFile)) { return AdditionalFilesPathWhole() + _standardFormulaFile; }
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
            if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { path.Add(AdditionalFilesPathWhole()); }
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

    public virtual FileOperationResult GrantWriteAccess(TableDataType type, string? chunkValue) {
        if (!IsEditable(false)) { return new(IsNotEditableReason(false), false, true); }

        return FileOperationResult.ValueStringEmpty;
    }

    public string ImportCsv(string importText, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        if (!IsEditable(false)) {
            DropMessage(ErrorType.Warning, "Abbruch, " + IsNotEditableReason(false));
            return "Abbruch, " + IsNotEditableReason(false);
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
        var startZ = 1;

        for (var spaltNo = 0; spaltNo < zeil[0].GetUpperBound(0) + 1; spaltNo++) {
            if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                DropMessage(ErrorType.Warning, "Abbruch, leerer Spaltenname.");
                return "Abbruch,<br>leerer Spaltenname.";
            }
            zeil[0][spaltNo] = ColumnItem.MakeValidColumnName(zeil[0][spaltNo]);

            var col = Column[zeil[0][spaltNo]];
            if (col == null) {
                if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                    DropMessage(ErrorType.Warning, "Abbruch, ungültiger Spaltenname.");
                    return "Abbruch,<br>ungültiger Spaltenname.";
                }

                col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                col?.Caption = zeil[0][spaltNo];
            }

            if (col == null) {
                DropMessage(ErrorType.Warning, "Abbruch, Spaltenfehler.");
                return "Abbruch,<br>Spaltenfehler.";
            }

            columns.Add(col);
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
                dictNeu.Add(rowNo.ToStringInt1(), zeil[rowNo]);
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
                    DropMessage(ErrorType.Warning, "Abbruch, vorhandene Zeilen der Tabelle '" + Caption + "' sind nicht eindeutig.");
                    return "Abbruch, vorhandene Zeilen sind nicht eindeutig.";
                }
            }
        }

        #endregion

        #region Der eigentliche Import

        //var d1 = DateTime.Now;
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
                dictVorhanden.TryGetValue(thisD.Key, out row);
                dictVorhanden.Remove(thisD.Key); // Speedup
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

            //if (DateTime.Now.Subtract(d1).TotalMinutes > 5) {
            //    DropMessage(ErrorType.Info, "Import: Zwischenspeichern der Tabelle");
            //    Save();
            //    d1 = DateTime.Now;
            //}

            if (DateTime.Now.Subtract(d2).TotalSeconds > 4.5) {
                DropMessage(ErrorType.Info, "Import: Zeile " + no + " von " + zeil.Count);
                d2 = DateTime.Now;
            }
            Develop.SetUserDidSomething();

            #endregion
        }

        #endregion

        //_ = Save();
        DropMessage(ErrorType.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_tableAdmin.Count == 0) { return false; }
        if (_tableAdmin.Contains(Everybody, false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _tableAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _tableAdmin.Contains(UserGroup, false);
    }

    /// <summary>
    /// Überprüft, ob ein generelles Bearbeiten eine Wertes möglich ist.
    /// Dieser Wert kann sich im Laufe der Ausführung ändern. (z.B. wenn eine Tabelle komplett geladen wurde)
    /// </summary>
    public virtual bool IsEditable(bool isloading) => string.IsNullOrEmpty(IsNotEditableReason(isloading));

    public virtual string IsNotEditableReason(bool isloading) {
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
    public string IsNowEditable() => GrantWriteAccess(TableDataType.Caption, TableChunk.Chunk_Master).StringValue;

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
        return IsEditable(false);
    }

    public bool IsThisScriptBroken(ScriptEventTypes type, bool returnValueCount0) {
        var l = _eventScript.Get(type);

        if (l.Count > 1) { return false; }

        if (l.Count == 0) { return returnValueCount0; }

        return l[0].IsOk();
    }

    public void LoadFromStream(System.IO.Stream stream) {
        LogUndo = false;
        DropMessages = false;

        //OnLoading();
        byte[] bLoaded;
        using (var r = new System.IO.BinaryReader(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt() ?? bLoaded; }

        OnLoading();
        Parse(bLoaded, true);
        RepairAfterParse();
        Freeze("Stream-Tabelle");
        MainChunkLoadDone = true;
        OnLoaded(true);
    }

    public void MasterMe() {
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

        Column.GetSystems();

        if (Column.SysChapter is { IsDisposed: false } c) {
            var x = c.Contents();
            if (x.Count < 2) {
                Column.Remove(c, "Automatische Optimierung");
            }
        }
    }

    public bool Parse(byte[] data, bool isMain) {
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

                var (i, command, value, columname, rowKey) = Parse(data, pointer);
                pointer = i;

                if (!command.IsObsolete()) {

                    #region Zeile suchen oder erstellen

                    if (!string.IsNullOrEmpty(rowKey)) {
                        row = Row.GetByKey(rowKey);
                        if (row is not { IsDisposed: false }) {
                            Row.ExecuteCommand(TableDataType.Command_AddRow, rowKey, Reason.NoUndo_NoInvalidate, null, null);
                            row = Row.GetByKey(rowKey);
                        }

                        if (row is not { IsDisposed: false }) {
                            Develop.DebugPrint(ErrorType.Error, "Zeile hinzufügen Fehler");
                            Freeze("Zeile hinzufügen Fehler");
                            return false;
                        }
                    }

                    #endregion

                    #region Spalte suchen oder erstellen

                    if (!string.IsNullOrEmpty(columname)) {
                        column = Column[columname];
                        if (command == TableDataType.ColumnName) {
                            if (column is not { IsDisposed: false }) {
                                Column.ExecuteCommand(TableDataType.Command_AddColumnByName, columname, Reason.NoUndo_NoInvalidate);
                                column = Column[columname];
                                if (column is not { IsDisposed: false }) {
                                    Develop.DebugPrint(ErrorType.Error, "Spalte hinzufügen Fehler");
                                    Freeze("Spalte hinzufügen Fehler");
                                    return false;
                                }
                            }
                            columnUsed.Add(column);
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

                    var error = SetValueInternal(command, column, row, value, UserName, DateTime.UtcNow, Reason.NoUndo_NoInvalidate);
                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("Tabellen-Ladefehler");
                        Develop.DebugPrint("Schwerer Tabellenfehler:<br>Version: " + TableVersion + "<br>Datei: " + KeyName + "<br>Meldung: " + error);
                        return false;
                    }
                }
            } while (true);
        } catch {
            Freeze("Parse Fehler!");
        }

        if (isMain) {

            #region unbenutzte (gelöschte) Spalten entfernen

            var l = new List<ColumnItem>();
            foreach (var thisColumn in Column) {
                l.Add(thisColumn);
            }

            foreach (var thisColumn in l) {
                if (!columnUsed.Contains(thisColumn)) {
                    Column.ExecuteCommand(TableDataType.Command_RemoveColumn, thisColumn.KeyName, Reason.NoUndo_NoInvalidate);
                }
            }

            #endregion

            Row.RemoveNullOrEmpty();
            Cell.RemoveOrphans();
        }

        if (IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(TableVersion.Replace(".", string.Empty))) { Freeze("Tabelleversions-Konflikt"); }

        Undo.OrderBy(k => k.DateTimeUtc);
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

    public bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
        var tmpName = UserName.ToUpperInvariant();
        var tmpGroup = UserGroup.ToUpperInvariant();
        if (string.Equals(allowed, Everybody, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (Column.SysRowCreator is { IsDisposed: false } src && string.Equals(allowed, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase)) {
            if (row != null && row.CellGetString(src).ToUpperInvariant() == tmpName) { return true; }
        } else if (string.Equals(allowed, "#USER: " + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, "#USER:" + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, tmpGroup, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        return false;
    }

    public virtual void ReorganizeChunks() {
    }

    public virtual void RepairAfterParse() {
        if (!IsEditable(false)) { return; }

        Column.Repair();

        Row.Repair();

        SortDefinition?.Repair();

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
    public virtual void TryToSetMeTemporaryMaster() {
        if (!IsEditable(false)) { return; }

        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax)) { return; }

        if (!NewMasterPossible()) { return; }

        MasterMe();
    }

    public void UnMasterMe() {
        if (AmITemporaryMaster(MasterBlockedMin, MasterBlockedMax)) {
            TemporaryTableMasterUser = "Unset: " + UserName;
            TemporaryTableMasterTimeUtc = DateTime.UtcNow.AddHours(-0.25).ToString5();
        }
    }

    public bool UpdateScript(string keyName, string? newkeyname, string? script = null, string? image = null, string? quickInfo = null, string? adminInfo = null, ScriptEventTypes? eventTypes = null, bool? needRow = null, ReadOnlyCollection<string>? userGroups = null, string? failedReason = null, bool isDisposed = false) {
        var existingScript = EventScript.GetByKey(keyName);
        if (existingScript == null) { return false; }

        return UpdateScript(existingScript, newkeyname, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason, isDisposed);
    }

    public void WriteBackVariables(RowItem? row, VariableCollection vars, bool virtualcolumns, bool dbVariables, string comment, bool doWriteBack) {
        if (doWriteBack) {
            if (row is { IsDisposed: false }) {
                foreach (var thisCol in Column) {
                    row.VariableToCell(thisCol, vars, comment);
                }
            }
            if (dbVariables) {
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
        Develop.DebugPrint(ErrorType.Warning, t);
    }

    internal virtual string IsValueEditable(TableDataType type, string? chunkValue) => IsNotEditableReason(false);

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
    protected void AddUndo(TableDataType type, string column, RowItem? row, string previousValue, string changedTo, string userName, DateTime datetimeutc, string comment, string container, string chunkValue) {
        if (IsDisposed) { return; }
        if (type.IsObsolete()) { return; }
        // ReadOnly werden akzeptiert, man kann es im Speicher bearbeiten, wird aber nicht gespeichert.

        if (type == TableDataType.SystemValue) { return; }

        Undo.Add(new UndoItem(KeyName, type, column, row, previousValue, changedTo, userName, datetimeutc, comment, container, chunkValue));
    }

    protected virtual void Checker_Tick(object state) {
        // Grundlegende Überprüfungen
        if (!IsEditable(false)) { return; }

        // Script-Überprüfung
        var e = new CanDoScriptEventArgs(false);
        OnCanDoScript(e);
        if (!e.Cancel) { RowCollection.ExecuteValueChangedEvent(); }
    }

    protected void CreateWatcher() {
        _checker?.Dispose();
        _checker = null;

        if (IsEditable(true)) {
            _checker = new Timer(Checker_Tick);
            _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            try {
                //_saveSemaphore?.Dispose();
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
                //_eventScriptEdited = new ReadOnlyCollection<TableScriptDescription>([]);
                _tableAdmin.Clear();
                _permissionGroupsNewRow.Clear();
                _tags.Clear();

                // Aus statischer Liste entfernen
                AllFiles.Remove(this);
            } catch (Exception ex) {
                Develop.DebugPrint(ErrorType.Error, "Fehler beim Dispose: " + ex.Message);
            }

            IsDisposed = true;
            OnDisposed();
        }
    }

    protected void DropMessage(ErrorType type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        Develop.Message?.Invoke(type, this, Caption, ImageCode.Tabelle, message, 0);
    }

    protected void OnAdditionalRepair() {
        if (IsDisposed) { return; }
        AdditionalRepair?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoaded(bool isFirst) {
        if (IsDisposed) { return; }
        Loaded?.Invoke(this, new FirstEventArgs(isFirst));
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
        if (reason != Reason.NoUndo_NoInvalidate && !IsEditable(false)) { return "Tabelle eingefroren: " + IsNotEditableReason(false); }
        if (type.IsObsolete()) { return string.Empty; }

        LastChange = DateTime.UtcNow;

        //if (!string.IsNullOrEmpty(chunk)) {
        //    LoadChunkWithChunkId(chunk, true, null);
        //}
        if (type.IsCellValue()) {
            if (column?.Table is not { IsDisposed: false } db) { return string.Empty; }
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
                case TableDataType.Command_RemoveColumn:
                    var c = Column[value];
                    if (c == null) { return string.Empty; }
                    return Column.ExecuteCommand(type, c.KeyName, reason);

                case TableDataType.Command_AddColumnByName:
                    var f2 = Column.ExecuteCommand(type, value, reason);
                    if (!string.IsNullOrEmpty(f2)) { return f2; }

                    var thisColumn = Column[value];
                    if (thisColumn == null) { return "Hinzufügen fehlgeschlagen"; }

                    return string.Empty;

                case TableDataType.Command_RemoveRow:
                    var r = Row.GetByKey(value);
                    if (r == null) { return string.Empty; }
                    return Row.ExecuteCommand(type, r.KeyName, reason, user, datetimeutc);

                case TableDataType.Command_AddRow:
                    var f1 = Row.ExecuteCommand(type, value, reason, user, datetimeutc);
                    if (!string.IsNullOrEmpty(f1)) { return f1; }
                    //var thisRow = Row.SearchByKey(value);
                    return string.Empty;

                case TableDataType.Command_NewStart:
                    return string.Empty;

                default:
                    if (LoadedVersion == TableVersion) {
                        Freeze("Ladefehler der Tabelle");
                        if (!IsFreezed) {
                            Develop.DebugPrint(ErrorType.Error, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + KeyName);
                        }
                    }
                    return "Befehl unbekannt.";
            }
        }

        switch (type) {
            //case TableDataType.Formatkennung:
            //    break;

            case TableDataType.Version:
                LoadedVersion = value.Trim();
                break;

            case TableDataType.LastEditUser:
                break;

            case TableDataType.LastEditID:
                break;

            case TableDataType.LastEditApp:
                break;

            case TableDataType.LastEditMachineName:
                break;

            case TableDataType.LastEditTimeUTC:
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

            case TableDataType.Caption:
                _caption = value;
                break;

            //case TableDataType.GlobalScale:
            //    _globalScale = FloatParse(value);
            //    break;

            case TableDataType.AdditionalFilesPath:
                _additionalFilesPath = value;
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
                //var tc = _eventScript.Count();

                List<string> ves = [.. value.SplitAndCutByCr()];
                var vess = new List<TableScriptDescription>();
                foreach (var t in ves) {
                    vess.Add(new TableScriptDescription(this, t));
                }
                Row.InvalidateAllCheckData();
                _eventScript = vess.AsReadOnly();

                //if (vess.Count != tc && tc > 0) {
                //    Develop.DebugPrint("TEST");
                //}
                break;

            //case (TableDataType)79: //.EventScriptEdited:

            //    if (value.Length > 50) {
            //        List<string> vese = [.. value.SplitAndCutByCr()];
            //        var veses = new List<TableScriptDescription>();
            //        foreach (var t in vese) {
            //            veses.Add(new TableScriptDescription(this, t));
            //        }
            //        _eventScript = veses.AsReadOnly();
            //    }

            //    break;

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
                _columnArrangements = value;
                break;

            case TableDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case TableDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            case TableDataType.EventScriptVersion:
                _eventScriptVersion = DateTimeParse(value);
                break;

            case TableDataType.UndoInOne:
                Undo.Clear();
                var uio = value.SplitAndCutByCr();
                for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                    UndoItem tmpWork = new(uio[z]);
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
                        Develop.DebugPrint(ErrorType.Error, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + KeyName);
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
        if (IsDisposed) { return "Tabelle verworfen!"; }
        if (IsFreezed) { return "Tabelle eingefroren!"; } // Sicherheitshalber!
        if (type.IsObsolete()) { return "Obsoleter Typ darf hier nicht ankommen"; }

        return string.Empty;
    }

    private static bool HasActiveThreadsExcept(string excludeThreadId) {
        try {
            return ExecutingScriptThreadsAnyTable.Any(thread => thread != excludeThreadId);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return HasActiveThreadsExcept(excludeThreadId);
        }
    }

    private static int NummerCode1(byte[] b, int pointer) => b[pointer];

    private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(byte[] b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private static FileOperationResult TryGrantWriteAccess(List<string> affectingFiles, params object?[] args) {
        if (args.Length < 5 ||
            args[0] is not string newChunkValue ||
            args[1] is not ColumnItem column ||
            args[2] is not RowItem row ||
            args[3] is not int waitforseconds ||
             args[4] is not bool onlyTopLevel) {
            return new("Ungültige Parameter.", false, true);
        }

        try {
            if (column.Table is not { IsDisposed: false } tb) { return new("Es ist keine Spalte ausgewählt.", false, true); }

            if (!tb.IsEditable(false)) { return new(tb.IsNotEditableReason(false), false, true); }

            var fo = tb.GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, newChunkValue);
            if (fo.Failed) { return fo; }

            fo = tb.GrantWriteAccess(TableDataType.UTF8Value_withoutSizeData, row.ChunkValue);
            if (fo.Failed) { return fo; }

            if (onlyTopLevel) { return FileOperationResult.ValueStringEmpty; }

            if (column.RelationType == RelationType.CellValues) {
                var (lcolumn, lrow, info, canrepair) = row.LinkedCellData(column, false, false);
                if (!string.IsNullOrEmpty(info) && !canrepair) { return new(info, false, true); }

                if (lcolumn?.Table is not { IsDisposed: false } db2) { return new("Verknüpfte Tabelle verworfen.", false, true); }

                db2.PowerEdit = tb.PowerEdit;

                if (lrow != null) {
                    waitforseconds = Math.Max(1, waitforseconds / 2);

                    var tmp = GrantWriteAccess(lcolumn, lrow, lrow.ChunkValue, waitforseconds, true);
                    if (!string.IsNullOrEmpty(tmp)) { return new("Die verlinkte Zelle kann nicht bearbeitet werden: " + tmp, waitforseconds > 10, false); }
                    return FileOperationResult.ValueStringEmpty;
                }

                return new("Allgemeiner Fehler.", true, true);
            }

            return FileOperationResult.ValueStringEmpty;
        } catch (Exception ex) {
            return new FileOperationResult(ex.ToString(), true, false); // Retry bei Exceptions
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
    /// AdditionalFiles/Tabellepfad mit Backup und abschließenden \
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// AdditionalFiles/Tabellepfad mit Forms und abschließenden \
    /// </summary>
    /// <returns></returns>
    private string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPathWhole())) { return AdditionalFilesPathWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
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
        MainChunkLoadDone = true;

        OnLoading();

        MainChunkLoadDone = true;
        BeSureToBeUpToDate(true, true);

        RepairAfterParse();

        OnLoaded(true);

        CreateWatcher();
    }

    private bool NewMasterPossible() {
        if (!IsAdministrator()) { return false; }

        if (!IsEditable(false)) { return false; }

        if (DateTimeTryParse(TemporaryTableMasterTimeUtc, out var dt)) {
            if (DateTime.UtcNow.Subtract(dt).TotalMinutes < MasterBlockedMax) { return false; }
            if (DateTime.UtcNow.Subtract(dt).TotalDays > 1) { return true; }
        }

        if (RowCollection.WaitDelay > 90) { return true; }

        if (MasterNeeded) { return true; }

        try {
            var masters = 0;
            foreach (var thisTb in AllFiles) {
                if (MultiUserPossible && !thisTb.IsDisposed && thisTb.AmITemporaryMaster(MasterBlockedMin, MasterCount)) {
                    masters++;
                    if (masters >= MaxMasterCount) { return false; }
                }
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
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
            // Es werden alle Tabellen abgefragt, also kann nach der ersten nicht schluss sein...

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
            // Speichern ohne ChangeData aufzurufen (würde Endlosschleife verursachen)
            WriteValueToDiscOrServer(TableDataType.ColumnArrangement, _columnArrangements, string.Empty, null, UserName, DateTime.UtcNow, string.Empty, string.Empty, "Automatische Aktualisierung nach Spaltenumbenennung");
        }
    }

    #endregion
}